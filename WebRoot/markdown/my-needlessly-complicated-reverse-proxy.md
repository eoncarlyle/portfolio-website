---
title: My needlessly complicated ZooKeeper-enabled reverse proxy
date: 2024.12.18
---
# My needlessly complicated ZooKeeper-enabled reverse proxy

## Self-Hosting and Apache ZooKeeper Background

After moving my personal website to my home DMZ network over a public cloud reverse proxy, I have increased my self-hosted footprint. Given how well cheap VPSs had performed for me in the past, I knew that some spare small-form-factor Lenovo desktops with more than one core and literally an order of magnitude more memory would do just fine for the same applications I had in the public cloud. Increasing this hardware footprint meant that I had an excuse to revisit an old friend that was a big part of my past role at RELEX: Apache ZooKeeper.

ZooKeeper is a key-value store for managing the state of distributed applications. The project started at Yahoo!, where many engineering teams working on distributed applications ended up duplicating effort in solving the same problems while introducing the same failure modes to their applications. What became ZooKeeper was a common solution to allow engineers to focus more on business logic and less on reading distributed systems academic research. The service is generally run as an ensemble of _2N - 1_ servers where _N>2_, and two motivations for this configuration are for fault tolerance and master election. Only a single master node in the ZooKeeper ensemble is capable of writing to the data store, and only once a write is recognised by a majority of the cluster members including the master is the write committed. This allows the data store to remain operational provided that a majority of the cluster is available. On ensemble startup, or if the master ZooKeeper server becomes unavailable, the ZooKeeper cluster elects a new master. An odd number of nodes prevents a 50/50 deadlocked master election in these cases. [^zookeeper-book]

## Reverse Proxy Server

While ZooKeeper is normally used for more interesting things, I decided to use it for service discovery and load balancing over the replicas serving the <a href="https://test.iainschmitt.com">test.iainschmitt.com</a>. static website. On startup, every replica writes to a `/targets/$hostName` znode; znodes being a node in the ZooKeeper data store. ZooKeeper supports both nodes that will persist until explicitly deleted as well as ephemeral ones that will be deleted once the client that created them in the first place is disconnected from the ZooKeeper ensemble. By using ephemeral lifetimes for replica znodes, unreachable target replicas would be removed from consideration by the reverse proxy.

When an uncached request for a particular URL reaches the reverse proxy, it lists the children of `/targets` to determine potential reverse proxy targets. The value stored in the `/targets/$hostName` znode is the count of cumulative requests to that target, so the target with the fewest requests is select and it's count incremented if the connection succeeds. If the first attempted target fails to respond, the next least commonly used is attempted. The request cache is cleared whenever a new replica comes online, which would most commonly happen during an application update.

By setting things up this way there weren't many changes that needed to happen with the portfolio website itself, almost all the new code was for the reverse proxy itself, which I wrote using Node. There would have been better choices as far as ZooKeeper support goes; while the `zookeeper` NPM package is actively maintained it falls back on more `Promise<any[]>` type definitions than preferable, but that may have to do with the native C libraries that the client is built with.[^zookeeper-npm] I can't say I've done a comprehensive side-by-side comparison, but the official ZooKeeper client written by the project team looks to be a lot more complete.

Having an 'outer' request made to the reverse proxy as well as an 'inner' request made by the reverse proxy to the target is something that I didn't do correctly at first as shown by this toy example:

```typescript
import { createServer, IncomingMessage, ServerResponse } from "node:http";
import http from "node:http";

createServer((req: IncomingMessage, res: ServerResponse) => {
  const options = {
    hostname: "127.0.0.1",
    port: 4000,
    method: "GET",
    path: req.url,
  };

  const proxyReq = http.request(options, (proxyRes) => {
    proxyRes.on("data", (chunk) => {
      res.writeHead(200, { "Content-Type": "text/plain" });
      res.end(chunk);
    });

    proxyRes.on("end", () => {
      proxyReq.end();
      res.end();
    });
  });

  proxyReq.on("error", (e) => {
    res.writeHead(500);
    res.end(e);
  });
}).listen(5001);
```

When the previous server was run, the inner `proxyRes` handler for `'data'` events was never called so it didn't function as an actual reverse proxy. I must have skipped this line in the Node docs the first time: [^node-http-request]

> In the example `req.end()` was called. With `http.request()` one must always call `req.end()` to signify the end of the request - even if there is no data being written to the request body.

After calling `end`, a readable stream of the request to the target server must be handled using event listeners. This readable stream also can emit multiple `'data'` events, so; a working reverse proxy looks something like the following:

```typescript
createServer(async (outerReq: IncomingMessage, outerRes: ServerResponse) => {
  const proxyReq = http.request({
    hostname: "localhost",
    port: 4000,
    method: "GET",
    path: "/",
  });

  proxyReq.end();
  proxyReq.on("response", (proxyRes) => {
    outerRes.writeHead(proxyRes.statusCode || 200, outerReq.headers);
    proxyRes.setEncoding("utf-8");
    const chunks: string[] = [];

    proxyRes.on("data", (chunk) => {
      chunks.push(chunk);
    });

    proxyRes.on("end", async () => {
      const body = chunks.join("");
      outerRes.write(body);
      outerRes.end();
    });
  });

  proxyReq.on("error", async () => {
    console.error("Error");
  });
}).listen(5000);
```

I hadn't placed much focus into the Node networking APIs before, and until reading the Node chapter of _JavaScript: The Definitive Guide_ I didn't have that great of a handle on it. [^flanagan] This was a book that I already had a great deal of respect for given its comprehensive detail, so I wasn't surprised that the Node chapter also was quite well written. The way that readable and writable streams work is relatively intuitive, but the Node documentation would be better if it included TypeScript types and was clearer about which events can be emitted by which readable streams. It seems strange that strings are used to represent arbitrary events. While there are readable stream method type signatures like `on(event: "data", listener: (chunk: any) => void): this` there is also a permissive `on(event: string | symbol, listener: (...args: any[]) => void): this` to support custom `EventEmitter` instances. [^node-types]

Event emitting is a pretty unique flow control primitive which I haven't seen an equivalent of in other languages. Because of this, one early mistake that I made with them was trying to catch errors by wrapping the entire `createServer` in a `try`/`catch`, but this of course does nothing to handle `'error'` events. A more embarrassing moment was when my reverse proxy was failing a [local load test](https://github.com/eoncarlyle/zk-reverse-proxy/blob/60e136df9a5627293b645c11860ebd492efe2ffe/load-test.yml), at which point I captured a [flame graph](https://github.com/eoncarlyle/zk-reverse-proxy/tree/main/zk-reverse-proxy-core/31594.0x) that pointed out something that I should have seen from reading my ZooKeeper enabled reverse proxy more carefully: I was reading from ZooKeeper regardless of if I had a response cached. After fixing this, the load test passed. This reminds me of what it is like to overuse interactive debuggers: often times they'll tell you exactly what you would have figured out if you simply read the code more methodically; a lot of debugging ultimately boils down to reading comprehension.

## Shortcomings and Future Development

This ZooKeeper enabled reverse proxy currently serves <a href="https://test.iainschmitt.com">test.iainschmitt.com</a>. across two replicas, this is naturally ridiculous overkill and there are dozens of out-of-the-box solutions to do this better. But given that this isn't for production use, that attitude is <a href="/post/reinventing-the-wheel-to-go-back-in-time">no fun</a>. With that said, one less obvious way that this is an absurd solution is that ZooKeeper was designed for distributed system workloads with more reads than writes, but right now the `/targets` znode is queried before updating the cumulative request count of the chosen target server, making for a 1:1 ratio between reads and writes. [^zookeeper-article] Right now I'm operating a single reverse proxy server, a single ZooKeeper in the ensemble, and both target server replicas on the same physical host, but that's just a little bit of system administration away from being fixed.

Apache ZooKeeper provides a few convinces for notifying clients about changes in the data store. ZooKeeper watches are one of these features, and I put them to use for clearing the reverse proxy cache when a new replica becomes available. A target server will write the current date time to `/cacheAge` during startup, and the reverse proxy calls the function below to clear the request cache accordingly. Because watches only last for a single change notification, in the code below I have reset the watch every time it is triggered but there really has to be a more elegant and error-resilient way to do this.

```typescript
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
```

The caching logic in general needs work; the cache TTL is 120 seconds and if the current target server git commit was recorded in ZooKeeper rather than the timestamp of the last replica restart then the cache could be cleared only when the content of the target server has actually changed.

Since leaving RELEX, I've heard many complaints like the following about my favourite fault-tolerant key-value store, such as on episode \#116 of the _Ship It!_ Dev Ops podcast:

> The worst outage I ever had is I was at Elastic, an engineering all hands in Berlin. It was a great place. I loved it. So all the SREs were there. And we did this to ourselves. Let me just preface this by saying… Because we relied on something that you should never rely on, and it’s called Zookeeper.
>
> Half of the gray in this beard is from Zookeeper. So many things that you know, and probably love, and also hate… You probably love it if you don’t have to actually do the operations for Zookeeper, and if you’re on operations with Zookeeper, you absolutely hate Zookeeper. Zookeeper is the bane of your infrastructure, necessary as it may be.

As someone who _was_ on the operations side of ZooKeeper, I have to disagree. But given that I went to the effort to shoehorn into a static site server, of course I disagree.


[^kingsbury]: Kyle Kingsbury. 2019. Jepsen: ZooKeeper. Retrieved 15 December, 2024 from https://aphyr.com/posts/291-jepsen-zookeeper

[^zookeeper-book]: Benjamin Reed and Flavio Junqueria. _ZooKeeper_. O'Reily Media, Sebastopol, CA.

[^zookeeper-npm]: [`zookeeper` NPM package](https://www.npmjs.com/package/zookeeper)

[^flanagan]: David Flanagan. _JavaScript: The Definitive Guide_ (7th. ed). O'Reily Media, Sebastopol, CA.

[^zookeeper-article]: Hunt, P., Konar, M., Junqueria, F. P., and Reed, B. "ZooKeeper: Wait-free Coordination for Internet-Scale Systems", in _Usenix ATC_, June 2010.

[^node-types]: [`node:stream` TypeScript types](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/node/stream.d.t)

[^ship-it]: [_Ship It!_ Episode \#116](https://changelog.com/shipit/116)

[^node-http-request]: [Node `http.request` Documentation](https://nodejs.org/api/http.html#httprequesturl-options-callback)
