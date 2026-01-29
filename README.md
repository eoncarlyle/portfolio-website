# Iain's Portfolio Website

This is my personal website available on [iainschmitt.com](https://iainschmitt.com), where I have both ways to review my work and write the occasional post.
It is an F# application running on .NET Core 8 using the Giraffe framework, and it is deployed using GitHub Actions to my home server but hosted behind a public cloud reverse proxy.

## Build Commands
```
$ dotnet build  -p:RunSyntaxHighlighter=false  && dotnet ./bin/Debug/net8.0/portfolio-website.App.dll 127.0.0.1 4000 --dynamic

$ dotnet build && dotnet ./bin/Debug/net8.0/portfolio-website.App.dll 127.0.0.1 4000
```
