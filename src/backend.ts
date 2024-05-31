import express, { NextFunction, Request, Response } from "express";
import { engine } from "express-handlebars";
import path from "path";
import { readFile, stat } from "fs/promises";
import marked from "marked";
import { inject, Scopes } from "dioma";
import BaseProvider from "./baseProvider";

type TemplateName = "directMarkdown" | "error" | "resumeMarkdown";

export default class Backend {
  private readonly baseProvider: BaseProvider;
  static scope = Scopes.Singleton();

  constructor(base = inject(BaseProvider)) {
    this.baseProvider = base;
    this.bindUtilityMiddleware();
    this.bindRouteMiddleware();
  }

  public launch() {
    this.baseProvider.app.listen(this.baseProvider.port, () => {
      console.log(
        `[server]: Server is running at http://localhost:${this.baseProvider.port}`,
      );
    });
  }

  private bindUtilityMiddleware() {
    this.baseProvider.app
      .engine("handlebars", engine({ defaultLayout: "default" }))
      .set("view engine", "handlebars")
      .set("views", path.join(__dirname, "views"))
      .use(express.static(this.baseProvider.contentPath));
  }

  private bindRouteMiddleware() {
    this.baseProvider.app
      .get("/", this.renderDefinedMarkdownSupplier("landing"))
      .get(
        "/resume",
        this.renderDefinedMarkdownSupplier("resume", "resumeMarkdown"),
      )
      .get(
        "/post/:markdownFileName",
        this.renderParamaterisedMarkdownSupplier("markdownFileName"),
      );

    this.baseProvider.app
      .use(this.render404Supplier())
      .use(this.render500Supplier);
  }

  private renderDefinedMarkdownSupplier = (
    markdownFileName: string,
    templateName: TemplateName = "directMarkdown",
  ) => {
    return (_req: Request, res: Response, next: NextFunction) => {
      this.getMarkdownTextFromFile(markdownFileName)
        .then((markdownText: string) =>
          res.render(templateName, { body: marked.parse(markdownText) }),
        )
        .catch(() => next());
    };
  };

  private renderParamaterisedMarkdownSupplier(paramName: string) {
    return (req: Request, res: Response, next: NextFunction) => {
      this.getMarkdownTextFromFile(req.params[paramName])
        .then((markdownText: string) =>
          res.render("directMarkdown", { body: marked.parse(markdownText) }),
        )
        .catch(() => next());
    };
  }

  private render404Supplier() {
    return (_req: Request, res: Response) => {
      res.render("error", {
        errorCode: "404",
        body: "The page that you are looking for does not exist!",
      });
    };
  }

  private render500Supplier() {
    return (_err: Error, _req: Request, res: Response) => {
      res.render("error", { errorCode: "500", body: "Internal server error" });
    };
  }

  private getMarkdownPath(markdownFileName: string) {
    return `${this.baseProvider.contentPath}/markdown/${markdownFileName}.md`;
  }

  private getMarkdownTextFromFile(markdownFileName: string) {
    const path = this.getMarkdownPath(markdownFileName);
    return stat(path).then(() => readFile(path, { encoding: "utf-8" }));
  }
}
