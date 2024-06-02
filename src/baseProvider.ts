import express from "express";
import { Scopes } from "dioma";
import morgan from "morgan";
import { createStream } from "rotating-file-stream";

export default class BaseProvider {
  static scope = Scopes.Singleton();

  public readonly contentPath;
  public readonly port;
  public readonly loggingMiddleware;
  public readonly app;

  constructor() {
    console.log(`Command line arguments: ${process.argv}`);
    const cliArgs = [process.argv.at(2), process.argv.at(3)];
    if (process.argv.length < 4 || !cliArgs[0] || !cliArgs[1])
      throw Error(
        `Illegal arguments '${process.argv}', correct form "node index [contentPath] [logDirname]`,
      );

    this.contentPath = cliArgs[0];
    const logDirname = cliArgs[1];

    this.port = 4000;

    const stream = createStream("portfolio-access.log", {
      path: logDirname,
      size: "10M",
      maxFiles: 30,
    })

    this.loggingMiddleware = morgan(
      function (tokens, req, res) {
        return [
          (new Date()).toISOString(),
          tokens.method(req, res),
          tokens.url(req, res),
          tokens.status(req, res),
          tokens.res(req, res, "content-length"),
          "-",
          tokens["user-agent"](req, res),
          tokens["response-time"](req, res),
          "ms",
        ].join(" ");
      },
      {stream: stream}
    );

    this.app = express();
  }
}
