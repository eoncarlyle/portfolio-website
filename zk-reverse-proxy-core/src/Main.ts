import ZooKeeper from "zookeeper";
import { IO } from "fp-ts/IO";

// TODO fix these dastardly imports

import {
  TaskOption,
  none,
  some,
  fromTask,
  chain as taskChain,
  fold as taskOptionFold,
} from "fp-ts/TaskOption";
import { Task, of as taskOf } from "fp-ts/Task";
import NodeCache from "node-cache";
import { pipe } from "fp-ts/function";
import ObsoleteOption from "Option.js";

type ZkConfig = {
  connect: string; //ZK server connection string
  timeout: number;
  debug_level: number;
  host_order_deterministic: boolean;
};

// Reffering to 'hostnames' as including ports while 'baseHostname' does not have the port
export type Target = {
  endpoint: string;
  count: number;
  version: number;
};

export const TARGETS_ZNODE_PATH = "/targets";

export const CACHE_DATE_ZNODE_PATH = "/cacheAge";

export enum HttpMethod {
  GET = "GET",
  POST = "POST",
  PUT = "PUT",
  DELETE = "DELETE",
  PATCH = "PATCH",
  HEAD = "HEAD",
  OPTIONS = "OPTIONS",
}

// Only used in target server
export const getSocketFromPort = (port: number, hostname = "127.0.0.1") =>
  `${TARGETS_ZNODE_PATH}/${hostname}:${port}`;

export const getSocket = (hostname: string) =>
  `${TARGETS_ZNODE_PATH}/${hostname}`;

export const zkConfig = {
  connect: "127.0.0.1:2181",
  timeout: 5000,
  debug_level: ZooKeeper.constants.ZOO_LOG_LEVEL_WARN,
  host_order_deterministic: false,
};

export const createZkClient = (config: ZkConfig) => {
  const client = new ZooKeeper(config);
  client.init(config);
  return client;
};

export const getMaybeZnode = async (client: ZooKeeper, path: string) => {
  return (await client.pathExists(path, false))
    ? ObsoleteOption.some(await client.exists(path, false)) //! This makes the assumption that the znode wasn't deleted between this line and the previous
    : ObsoleteOption.none<stat>();
};

export const _getMaybeZnode = (
  client: ZooKeeper,
  path: string,
): TaskOption<stat> => {
  return pipe(
    pipe(() => client.pathExists(path, false), fromTask),
    taskChain((exists: boolean) => (exists ? some(path) : none)),
    taskChain((path: string) => fromTask(() => client.exists(path, false))),
  );
};

export const createZnodeIfAbsent = async (
  client: ZooKeeper,
  path: string,
  flags?: number,
) => {
  if ((await getMaybeZnode(client, path)).isNone()) {
    await client.create(path, "", flags || ZooKeeper.constants.ZOO_PERSISTENT);
  }
};
// data_cb : function(rc, error, stat, data)

export const _createZnobdeIfAbsent = (
  client: ZooKeeper,
  path: string,
  flags?: number,
): Task<IO<void>> =>
  pipe(
    _getMaybeZnode(client, path),
    taskOptionFold(
      () => {
        return taskOf(() => {});
      },
      (_stat) => {
        return taskOf(() =>
          client.create(path, "", flags || ZooKeeper.constants.ZOO_PERSISTENT),
        );
      },
    ),
  );

export const cacheResetWatch = async (
  client: ZooKeeper,
  path: string,
  cache: NodeCache,
) => {
  if ((await getMaybeZnode(client, path)).isSome()) {
    client.aw_get(
      path,
      (_type, _state, _path) => {
        console.log("Clearing cache");
        cache.close();
        cacheResetWatch(client, path, cache);
      },
      (_rc, _error, _stat, _data) => {},
    );
  }
};

export const _cacheResetWatch = (
  client: ZooKeeper,
  path: string,
  cache: NodeCache,
): Task<IO<void>> =>
  pipe(
    _getMaybeZnode(client, path),
    taskOptionFold(
      () => {
        return taskOf(() => {});
      },
      (_stat) =>
        taskOf(() => {
          client.aw_get(
            path,
            (_type, _state, _path) => {
              console.log("Clearing cache");
              cache.close();
              _cacheResetWatch(client, path, cache)().then((io) => io());
            },
            (_rc, _error, _stat, _data) => {},
          );
        }),
    ),
  );
