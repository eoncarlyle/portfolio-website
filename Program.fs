module Portfolio.App

open System
open System.IO
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Giraffe.Razor
open org.apache.zookeeper

let webApp =
    choose [ GET >=> choose AppHandlers.appRoutes; AppHandlers.error404Handler ]

let internalErrorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    AppHandlers.error500Handler

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

let noOpWatcherFunction (event: WatchedEvent) : Task = Task.CompletedTask

type NoOpWatcher() =
    inherit Watcher()

    override _.process(event: WatchedEvent) : Task =
        // No operation - return a completed task
        Task.CompletedTask

let zooKeeper = new ZooKeeper("localhost:2181", 3000, NoOpWatcher())

let configureZookeeper (hostPort: string) =

    task {
        let! hostListStat = zooKeeper.existsAsync "/portfolio-hosts"

        if (hostListStat = null) then
            zooKeeper.createAsync ("/portfolio-hosts", [||], ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT)
            |> ignore

        zooKeeper.createAsync (
            $"/portfolio-hosts/localhost:{hostPort}",
            [||],
            ZooDefs.Ids.OPEN_ACL_UNSAFE,
            CreateMode.EPHEMERAL
        )
        |> ignore

    }
    |> ignore


[<EntryPoint>]
let main args =

    let hostPort = Array.tryHead args |> Option.orElse (Some "4080") |> Option.get
    configureZookeeper hostPort

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseUrls($"http://localhost:{hostPort}")
                .UseContentRoot(AppHandlers.contentRoot)
                .UseWebRoot(AppHandlers.webRoot)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
