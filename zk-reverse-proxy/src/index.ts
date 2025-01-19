import http, { createServer, IncomingMessage, ServerResponse } from "node:http";
import * as R from "ramda";
import {
  CACHE_DATE_ZNODE_PATH,
  createZkClient,
  createZnodeIfAbsent,
  getSocket,
  HttpMethod,
  Target,
  TARGETS_ZNODE_PATH,
  cacheResetWatch,
  getZkConfig,
} from "./Main.js";
//import * as console from "node:console";
import NodeCache from "node-cache";
import ZooKeeperPromise from "zookeeper";
import { argv } from "node:process";

// TODO fix typing
const getKey = (reqUrl: string, method: string | undefined) => {
  return JSON.stringify({
    method: method,
    path: reqUrl,
  });
};

const getHttpOptions = (
  targets: Target[],
  reqUrl: string,
  index: number,
  method: HttpMethod = HttpMethod.GET,
) => {
  const target = targets[index];
  const [hostname, port]: string[] = target.endpoint.split(":");

  return {
    hostname: hostname,
    port: parseInt(port),
    method: method,
    path: reqUrl,
  };
};

const updateTargetHostCount = async (
  zkClient: ZooKeeperPromise,
  candidateSockets: Target[],
  candidateIndex: number,
) => {
  const selectedTargetHost = candidateSockets[candidateIndex];

  try {
    await zkClient.set(
      getSocket(selectedTargetHost.endpoint),
      String(selectedTargetHost.count + 1),
      selectedTargetHost.version, // Non `-1` version reference didn't work on artillery tests until started caching
    );
  } catch (e: any) {
    console.error(
      `Error with update attempt: ${JSON.stringify(selectedTargetHost)}`,
    );
    throw e;
  }
};

const getTargets = async (
  zkClient: ZooKeeperPromise,
  incomingTargets: Target[],
) => {
  const sockets = await zkClient.get_children(TARGETS_ZNODE_PATH, false);
  if (incomingTargets.length === 0) {
    return R.sort(
      R.ascend(R.prop("count")),
      await Promise.all(
        sockets.map(async (socket) => {
          const [znodeStat, data] = (await zkClient.get(
            getSocket(socket),
            false,
          )) as [stat, object];
          return {
            endpoint: socket,
            count: data ? parseInt(data.toString()) : 0,
            version: znodeStat.version,
          };
        }),
      ),
    );
  } else return incomingTargets;
};

const requestListener =
  (zkClient: ZooKeeperPromise, httpCache: NodeCache) =>
  async (
    outerReq: IncomingMessage,
    outerRes: ServerResponse,
    incomingTargets: Target[] = [],
    targetIndex: number = 0,
  ) => {
    if (
      outerReq.url !== undefined &&
      (incomingTargets.length === 0 || targetIndex < incomingTargets.length)
    ) {
      try {
        //const key = JSON.stringify(options) //Why did `options.path` not work?
        const key = getKey(outerReq.url, outerReq.method);
        if (outerReq.method !== HttpMethod.GET || !httpCache.get(key)) {
          const targets = await getTargets(zkClient, incomingTargets);

          // Should only have to make ZK writes in event of cache miss
          await updateTargetHostCount(zkClient, targets, targetIndex); // Replacing with shuffle didn't meaningfully improve: search commits
          const innerReq = http.request(
            getHttpOptions(targets, outerReq.url, targetIndex),
          );
          // Writing JSON, multipart form, etc. in body would need to happen before this point
          // While this isn't needed for current implementation, would be required later on

          innerReq.end();
          innerReq.on("response", (innerRes) => {
            // I think the fact that incoming message doesn't just have a body - and that is streamed instead -is meaningful
            outerRes.writeHead(innerRes.statusCode || 200, outerReq.headers);
            innerRes.setEncoding("utf-8");
            const chunks: string[] = [];

            innerRes.on("data", (chunk) => {
              chunks.push(chunk);
            });

            innerRes.on("end", async () => {
              // Caching required going over to event handlers rather than
              try {
                const body = chunks.join("");
                outerRes.write(body);
                outerRes.end();
                httpCache.set<string>(key, body, 100);
              } catch (e) {
                requestListener(zkClient, httpCache)(
                  outerReq,
                  outerRes,
                  targets,
                  targetIndex + 1,
                );
              }
            });
          });

          innerReq.on("error", async () => {
            // Try/catch is not enough, need explicit error event listener
            await requestListener(zkClient, httpCache)(
              outerReq,
              outerRes,
              targets,
              targetIndex + 1,
            );
          });
        } else if (httpCache.get(key)) {
          const body = httpCache.get<string>(key);
          outerRes.writeHead(200, outerReq.headers);
          outerRes.end(body);
        }
      } catch (e: any) {
        await requestListener(zkClient, httpCache)(
          outerReq,
          outerRes,
          await getTargets(zkClient, incomingTargets),
          targetIndex + 1,
        );
      }
    } else if (targetIndex >= incomingTargets.length) {
      outerRes.writeHead(500);
      outerRes.write("Internal Error");
      outerRes.end();
    } else {
      outerRes.writeHead(400);
      outerRes.end("Bad Request");
    }
  };

if (argv.length != 4) {
  console.error(`Illegal arguments: '${argv}'`);
  process.exit(1);
} else {
  const zkConnectString = argv[2];
  const port = argv[3];

  const reverseProxyZkClient = createZkClient(getZkConfig(zkConnectString));
  await createZnodeIfAbsent(reverseProxyZkClient, TARGETS_ZNODE_PATH);
  await createZnodeIfAbsent(reverseProxyZkClient, CACHE_DATE_ZNODE_PATH);

  const httpCache = new NodeCache({});
  await cacheResetWatch(reverseProxyZkClient, CACHE_DATE_ZNODE_PATH, httpCache);

  createServer(requestListener(reverseProxyZkClient, httpCache)).listen(port);
}
