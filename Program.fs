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

let webApp =
    choose
        [ GET >=> choose AppHandlers.appRoutes
          HEAD >=> AppHandlers.headHandler
          AppHandlers.error404Handler ]

let internalErrorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    AppHandlers.error500Handler

let configureApp (app: IApplicationBuilder) =
    app
        .UseGiraffeErrorHandler(internalErrorHandler)
        .UseHttpsRedirection()
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


//TODO: better validation
type NetworkArgs =
    { ZkConnectString: string
      HostAddress: string
      HostPort: string }

let getNetworkArgs args =
    match (Array.length args) with
    | 3 ->
        Some
            { ZkConnectString = Array.get args 0
              HostAddress = Array.get args 1
              HostPort = Array.get args 2 }
    | _ -> None

[<EntryPoint>]
let main args =

    // Intentionally unrecoverably fails if bad
    let networkArgs = getNetworkArgs args |> Option.get

    let zkConnectString = networkArgs.ZkConnectString
    let hostAddress = networkArgs.HostAddress
    let hostPort = networkArgs.HostPort

    AppZooKeeper.configureZookeeper zkConnectString hostAddress hostPort

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseUrls($"http://{hostAddress}:{hostPort}")
                .UseContentRoot(AppHandlers.contentRoot)
                .UseWebRoot(AppHandlers.webRoot)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
