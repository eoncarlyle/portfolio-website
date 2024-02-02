import express, { Express, NextFunction, Request, Response } from "express";
import { engine } from "express-handlebars";
import path from "path";
import { readFile, stat } from "fs/promises";
import marked from "marked";

export default class Backend {
  app: Express;
  contentPath: string;
  port: number;

  constructor(contentPath: string, port: number) {
    this.app = express();
    this.contentPath = contentPath;
    this.port = port;

    this.app.engine("handlebars", engine({ defaultLayout: false }));
    this.app.set("view engine", "handlebars");
    this.app.set("views", path.join(__dirname, "views"));
    this.app.use(express.static("public"));

    const getMarkdownPath = (markdownFileName: string) => `${this.contentPath}/markdown/${markdownFileName}.md`
    const getMarkdownText = (markdownFilePath: string) => stat(markdownFilePath).then(() => readFile(markdownFilePath, { encoding: "utf-8" })); 

    this.app.get("/", (req: Request, res: Response, next: NextFunction) => {
      getMarkdownText(getMarkdownPath("landing"))
        .then((markdownText: string) => res.render("default", { body: marked.parse(markdownText) }))
        .catch(() => next());
    });

    this.app.get("/:markdownFileName", (req: Request, res: Response, next: NextFunction) => {
      getMarkdownText(getMarkdownPath(req.params.markdownFileName))
        .then((markdownText: string) => res.render("default", { body: marked.parse(markdownText) }))
        .catch(() => next());
    });

    // custom 404 page
    this.app.use((_req: Request, res: Response) => {
      res.render("error", { errorCode: "404", body: "The page that you are looking for does not exist!" })
    });

    this.app.use((err: Error, req: Request, res: Response) => {
      res.render("error", { errorCode: "500", body: "Internal server error" })
    });
  }

  launch() {
    this.app.listen(this.port, () => {
      console.log(`[server]: Server is running at http://localhost:${this.port}`);
    });
  }

  static main() {
    const contentPath = process.argv.at(2) || "public";
    const port = Number(process.argv.at(3)) || 3000;
    console.log(process.argv);
    if (!contentPath) {
      throw Error(`Illegal arguments ${process.argv}, correct form "node index [contentPath] [port]`);
    }

    console.log(`Assumed content path from command-line arguments: ${contentPath}`);
    console.log(`Assumed port number from command-line arguments: ${port}`);
    new Backend(contentPath, port).launch();
  }
}
