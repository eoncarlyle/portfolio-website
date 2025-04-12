---
title: Prism Proxy Bug
date: 2025.04.12
---
# Prism Proxy Bug: STOP-2386
A few disclaimers to start - nothing in this blog post discloses proprietary information from either SPS or our customers, and I did get company permission prior to writing this. This post and everything else on this website is written in my personal capacity and not on behalf of SPS or any previous employers; all opinions expressed here are my own. With that out of the way, my company uses the `@stoplight/prism-cli` reverse proxy to enforce Open-API specifications on HTTP ingress for some of our REST API applications. When working properly, this requires less defensive programming from API authors and automates interface enforcement between services. Earlier this year, one of our Prism proxies crashed while processing a relatively large request that included a special character, `ü`.

```text
/Users/iain/code/prism/node_modules/split2/index.js:44
      push(this, this.mapper(list[i]))
                      ^
sourcemap-register.js:1
SyntaxError: Expected ',' or '}' after property value in JSON at position 8193
    at Transform.parse [as mapper] (<anonymous>)
    at Transform.transform [as _transform] (/Users/iain/code/prism/node_modules/split2/index.js:44:23)
    at Transform._write (node:internal/streams/transform:175:8)
    at writeOrBuffer (node:internal/streams/writable:447:12)
    at _write (node:internal/streams/writable:389:10)
    at Transform.Writable.write (node:internal/streams/writable:393:10)
    at Socket.ondata (node:internal/streams/readable:817:22)
    at Socket.emit (node:events:514:28)
    at Socket.emit (node:domain:488:12)
    at addChunk (node:internal/streams/readable:376:12)
```

The `split2` NPM package is used in Prism proxy to concatenate chunks of a readable stream together, and as shown in the stack trace the exception was thrown in that package. The `stop-2386-bug-demonstration` branch of my `@stoplight/prism-cli` [fork](https://github.com/eoncarlyle/prism) has a `cli:stop-2386` [NPM script](https://github.com/eoncarlyle/prism/blob/stop-2386-bug-demonstration/packages/cli/package.json#L48) that I used to reproduce the bug without using any proprietary information from SPS or SPS customers. `cli:stop-2386` uses the most current version of Prism proxy as of time of writing, version 5.12.0; while I used Node 20.9.0 while preparing this explanation, I have yet to find a Node version where I couldn't reproduce the error. The proxy can be configured to run as either a single process or in multiprocess mode, where the HTTP server and logger are run in separate processes of the same node cluster. The `cli:stop-2386` script runs using a multiprocess configuration in a manner equivalent to our production configuration.

Node event handling makes this unclear from the stack trace, but the error itself is raised when incoming HTTP request bodies are logged to standard output. To demonstrate the bug, a legal JSON object `badInput` is logged on startup for both multi-process and single-process configurations of the reverse proxy, but using this as a request body to the reverse proxy would also reproduce the issue.

```typescript
const createMultiProcessPrism: CreatePrism = async options => {
  if (cluster.isMaster) {
    cluster.setupMaster({ silent: true });

    signale.await({ prefix: chalk.bgWhiteBright.black('[CLI]'), message: 'Starting Prism…' });

    const worker = cluster.fork();

    if (worker.process.stdout) {
      pipeOutputToSignale(worker.process.stdout);
    }

    return;
  } else {
    const logInstance = createLogger('CLI', { ...cliSpecificLoggerOptions, level: options.verboseLevel });

    // Forcing the error
    logInstance.info({ badInput }, 'Request received');
    return createPrismServerWithLogger(options, logInstance).catch((e: Error) => {
      logInstance.fatal(e.message);
      cluster.worker.kill();
      throw e;
    });
  }
};

const createSingleProcessPrism: CreatePrism = options => {
  signale.await({ prefix: chalk.bgWhiteBright.black('[CLI]'), message: 'Starting Prism…' });

  const logStream = new PassThrough();
  const logInstance = createLogger('CLI', { ...cliSpecificLoggerOptions, level: options.verboseLevel }, logStream);
  pipeOutputToSignale(logStream);

  // Attempt to force the error
  logInstance.info({ badInput }, 'Request received');
  return createPrismServerWithLogger(options, logInstance).catch((e: Error) => {
    logInstance.fatal(e.message);
    throw e;
  });
};
```

The `transform` function of `split2` is invoked twice because when `badInput` is logged during `cli:stop-2386` it is broken into two integer buffers with 8162 and 5069 elements respectively, it seems that this is the literal representation of a readable stream. When the two chunks are concatenated, a exception is thrown because a missing character prevents the result from being parsed into JSON. While I am no expert in Node readable streams, it appears that a `pipe` call in `pipeOutputToSignale` of the cluster's master process is transforming the worker process standard output when the error occurs. Much to my surprise, one of the buffers already had a missing character it reached `split2`: `Pipe.callbackTrampoline` is the very first function called while piping to the master process, and the incoming chunk is passed as `args[0]`: the end of the first `badInput` chunk is `{"id":"2cd9545e58` and `,"values":["8015751025"]}` is the start of the second chunk.[^hex-literals-0] The closing quote for `"2cd9545e58` breaks the JSON parsing, but earlier in the first chunk the special character seems to be represented correctly as `{"id":"56c8","values":["be0bümmmmmmmmm"]},`.[^hex-literals-1]

When running the proxy as a single process the `badInput` log is passed as single chunk and no exceptions are thrown. When the `ü` in `badInput` is repaced with a `u`, the end of the first chunk is `{"id":"2cd9545e58"`, so parsing succeeds.[^hex-literals-2] Now is a good moment to discuss how `badInput` was made: it appears that a quote, comma, or curly brace must be the final character of a chunk to force the error. I have gone as far as to build a Node runtime with debugging symbols to understand the error but I didn't get very far in understanding where the comma is dropped, and why this is only happening for special characters. This is a strange issue that I have spun my wheels on a lot, so I've wanted to write about this for a larger audience to get some input on the root cause from people who have more experience with Node readable streams. While Smartbear has prepared a [PR](https://github.com/stoplightio/prism/pulls) to address the issue, it calls `jsonrepair` on the concatenated result rather than addressing that the worker process is writing valid JSON to standard output but the resulting readable stream chunks in the master process are broken.


[^hex-literals-0]: Hex literals as viewed with the Visual Studio Code Hex Editor.
```text
\x7b\x22\x69\x64\x22\x3a\x22\x32\x63\x64\x39\x35\x34\x35\x65\x35\x38
\x2c\x22\x76\x61\x6c\x75\x65\x73\x22\x3a\x5b\x22\x38\x30\x31\x35\x37\x35\x31\x30\x32\x35\x22\x5d\x7d
```

[^hex-literals-1]: Hex literal:
```text
\x7b\x22\x69\x64\x22\x3a\x22\x35\x36\x63\x38\x22\x2c\x22\x76\x61\x6c\x75\x65\x73\x22\x3a\x5b\x22\x62\x65\x30\x62\xc3\xbc\x6d\x6d\x6d\x6d\x6d\x6d\x6d\x6d\x6d\x22\x5d\x7d
```

[^hex-literals-2]: Hex literals:
```text
\x7b\x22\x69\x64\x22\x3a\x22\x32\x63\x64\x39\x35\x34\x35\x65\x35\x38\x22
\x2c\x22\x76\x61\x6c\x75\x65\x73\x22\x3a\x5b\x22\x38\x30\x31\x35\x37\x35\x31\x30\x32\x35\x22\x5d\x7d
```
