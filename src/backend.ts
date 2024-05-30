import express, { NextFunction, Request, Response } from "express";
import { engine } from "express-handlebars";
import path from "path";
import { readFile, stat } from "fs/promises";
import marked from "marked";
import { inject, Scopes } from "dioma";
import Base from "./base";

type TemplateName = "directMarkdown" | "error" | "resumeMarkdown";

export default class Backend {
  public readonly base: Base;
  static scope = Scopes.Singleton();

  constructor(base = inject(Base)) {
    this.base = base;
  }

  private bindUtilityMiddleware() {
    this.base.app.engine("handlebars", engine({ defaultLayout: "default" }));
    this.base.app.set("view engine", "handlebars");
    this.base.app.set("views", path.join(__dirname, "views"));
    this.base.app.use(express.static(this.base.contentPath));
  }

  private bindRouteMiddleware() {
    this.base.app.get("/", this.renderMarkdown("landing"));
    this.base.app.get("/resume", this.renderMarkdown("resume", "resumeMarkdown"));
    this.base.app.get(
      "/post/:markdownFileName",
      (req: Request, res: Response, next: NextFunction) => {
        this.getMarkdownTextFromFile(req.params.markdownFileName)
          .then((markdownText: string) =>
            res.render("directMarkdown", { body: marked.parse(markdownText) }),
          )
          .catch(() => next());
      },
    );

    this.base.app.use((_req: Request, res: Response) => {
      res.render("error", {
        errorCode: "404",
        body: "The page that you are looking for does not exist!",
      });
    });

    this.base.app.use((_err: Error, _req: Request, res: Response) => {
      res.render("error", { errorCode: "500", body: "Internal server error" });
    });
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

  private renderMarkdown = (
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

  private getMarkdownPath(markdownFileName: string) {
    return `${this.base.contentPath}/markdown/${markdownFileName}.md`;
  }

  private getMarkdownTextFromFile(markdownFileName: string) {
    const path = this.getMarkdownPath(markdownFileName);
    return stat(path).then(() => readFile(path, { encoding: "utf-8" }));
  }
}
