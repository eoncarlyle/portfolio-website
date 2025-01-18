module Portfolio.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
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


let getRuntimeArgs args: AppZooKeeper.RuntimeArgs option=
    match (Array.length args) with
    | 4 ->
        Some
            { ZkConnectString = Array.get args 0
              HostAddress = Array.get args 1
              HostPort = Array.get args 2
              CommitSHA = Array.get args 3}
    | _ -> None

[<EntryPoint>]
let main args =
    // Intentionally fails if absent
    let runtimeArgs = getRuntimeArgs args |> Option.get

    match runtimeArgs.ZkConnectString with
    | "-1" -> ()
    | _ -> AppZooKeeper.configureZookeeper runtimeArgs

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseUrls($"http://{runtimeArgs.HostAddress}:{runtimeArgs.HostPort}")
                .UseContentRoot(AppHandlers.contentRoot)
                .UseWebRoot(AppHandlers.webRoot)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
