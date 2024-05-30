import express, { NextFunction, Request, Response } from "express";
import { engine } from "express-handlebars";
import path from "path";
import { readFile, stat } from "fs/promises";
import marked from "marked";
import { inject, Scopes } from "dioma";
import Base from "./base";

type TemplateName = "directMarkdown" | "error" | "resumeMarkdown";

export default class Backend {
  private readonly base: Base;
  static scope = Scopes.Singleton();

  constructor(base = inject(Base)) {
    this.base = base;
  }

  private bindUtilityMiddleware() {
    this.base.app.engine("handlebars", engine({ defaultLayout: "default" }))
      .set("view engine", "handlebars")
      .set("views", path.join(__dirname, "views"))
      .use(express.static(this.base.contentPath));
  }

  private bindRouteMiddleware() {
    this.base.app.get("/", this.renderDefinedMarkdownSupplier("landing"))
      .get("/resume", this.renderDefinedMarkdownSupplier("resume", "resumeMarkdown"))
      .get("/post/:markdownFileName", this.renderParamaterisedMarkdownSupplier("markdownFileName"));

    this.base.app.use(this.render404Supplier())
      .use(this.render500Supplier);
  }


  launch() {
    this.bindUtilityMiddleware();
    this.bindRouteMiddleware();
    this.base.app.listen(this.base.port, () => {
      console.log(
        `[server]: Server is running at http://localhost:${this.base.port}`,
      );
    });
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
    }
  }

  private render404Supplier() {
    return (_req: Request, res: Response) => {
      res.render("error", {
        errorCode: "404",
        body: "The page that you are looking for does not exist!",
      });
    }
  }

  private render500Supplier() {
    return (_err: Error, _req: Request, res: Response) => {
      res.render("error", { errorCode: "500", body: "Internal server error" });
    };
  }

  private getMarkdownPath(markdownFileName: string) {
    return `${this.base.contentPath}/markdown/${markdownFileName}.md`;
  }

  private getMarkdownTextFromFile(markdownFileName: string) {
    const path = this.getMarkdownPath(markdownFileName);
    return stat(path).then(() => readFile(path, { encoding: "utf-8" }));
  }
}
