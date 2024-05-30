import express from "express"
import { Scopes } from "dioma"

export default class Base {
  static scope = Scopes.Singleton();
  
  public readonly contentPath; 
  public readonly logPath;
  public readonly port;
  public readonly app;
  
  constructor() {
    this.contentPath = process.argv.at(2) || "public";
    this.logPath = "opt/portfolio.log";
    this.port = 4000;
    console.log(`Command line arguments: ${process.argv}`);
    
    if (!this.contentPath) {
      throw Error(
        `Illegal arguments ${process.argv}, correct form "node index [contentPath] [port]`,
      );
    }

    console.log(
      `Assumed content path from command-line arguments: ${this.contentPath}`,
    );

    console.log(`Assumed port number from command-line arguments: ${this.port}`);
    this.app = express()
  }
}