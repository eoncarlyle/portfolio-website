module Portfolio.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.Razor
open FSharp.Formatting.Markdown

let markdownHandler viewName markdownFileName =
    let fileContents =
        File.ReadAllText $"./WebRoot/markdown/{markdownFileName}.md" |> Markdown.ToHtml

    let viewData = dict [ ("Body", box fileContents) ] |> Some
    publicResponseCaching 60 None >=> razorHtmlView viewName None viewData None

let directMarkdownHandler markdownFileName =
    markdownHandler "DirectMarkdown" markdownFileName

let errorHandler errorCode body =
    let viewData = dict [ ("ErrorCode", box errorCode); ("Body", box body) ] |> Some
    razorHtmlView "Error" None viewData None

let webApp =
    choose
        [ GET
          >=> choose
                  [ route "/" >=> directMarkdownHandler "landing"
                    route "/resume" >=> markdownHandler "ResumeMarkdown" "resume"
                    routef "/post/%s" directMarkdownHandler ]
          setStatusCode 404
          >=> publicResponseCaching 60 None
          >=> errorHandler 404 "The page that you are looking for does not exist!" ]

let internalErrorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> errorHandler 500 "Internal server error"

let configureCors (builder: CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:4000").AllowAnyMethod().AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    app
        .UseGiraffeErrorHandler(internalErrorHandler)
        .UseHttpsRedirection()
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseResponseCaching()
        .UseGiraffe(webApp)

let configureServices (services: IServiceCollection) =
    let sp = services.BuildServiceProvider()
    let env = sp.GetService<IWebHostEnvironment>()
    let viewsFolderPath = Path.Combine(env.ContentRootPath, "Views")
    services.AddRazorEngine viewsFolderPath |> ignore
    services.AddCors().AddResponseCaching().AddGiraffe() |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseUrls("http://localhost:4000")
                .UseContentRoot(contentRoot)
                .UseWebRoot(webRoot)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
