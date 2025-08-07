module Portfolio.App

open System
open System.IO
open AppHandlers
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.AspNetCore.Mvc.Razor
open Microsoft.AspNetCore.StaticFiles

let routes webRoot =
    choose [ GET >=> choose (appRoutes webRoot); HEAD >=> headHandler; error404Handler ]

let internalErrorHandler =
    fun (ex: Exception) (logger: ILogger) ->
        logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
        error500Handler

let configureApp (app: IApplicationBuilder) =
    let staticFileOptions = StaticFileOptions()

    let prepareResponse =
        fun (context: StaticFileResponseContext) ->
            let headers = context.Context.Response.Headers
            headers.CacheControl <- "public,max-age=604800"
            headers.Expires <- DateTimeOffset.UtcNow.AddDays(7).ToString("R")

    staticFileOptions.OnPrepareResponse <- prepareResponse

    let webRoot = Path.Combine(AppContext.BaseDirectory, "WebRoot")

    app
        .UseGiraffeErrorHandler(internalErrorHandler)
        .UseHttpsRedirection()
        .UseStaticFiles(staticFileOptions)
        .UseResponseCaching()
        .UseGiraffe(routes webRoot)

let configureServices (services: IServiceCollection) =
    let sp = services.BuildServiceProvider()
    let env = sp.GetService<IWebHostEnvironment>()
    let viewsFolderPath = Path.Combine(env.ContentRootPath, "Views")
    services.AddCors().AddResponseCaching().AddGiraffe() |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

type NetworkArgs =
    { HostAddress: string
      HostPort: string }

let getNetworkArgs args =
    match (Array.length args) with
    | 2 ->
        Some
            { HostAddress = Array.get args 0
              HostPort = Array.get args 1 }
    | _ -> None

[<EntryPoint>]
let main args =
    // Intentional unrecoverable failure if incorrect
    let networkArgs = getNetworkArgs args |> Option.get
    let hostAddress = networkArgs.HostAddress
    let hostPort = networkArgs.HostPort

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseUrls($"http://{hostAddress}:{hostPort}")
                .UseWebRoot(Path.Combine(AppContext.BaseDirectory))
                .UseWebRoot(Path.Combine(AppContext.BaseDirectory, "WebRoot"))
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
