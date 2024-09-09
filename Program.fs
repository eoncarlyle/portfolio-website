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

[<EntryPoint>]
let main args =
    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseUrls("http://localhost:4000")
                .UseContentRoot(AppHandlers.contentRoot)
                .UseWebRoot(AppHandlers.webRoot)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
