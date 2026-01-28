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

let routes baseDirectory isDynamic =
    choose [ GET >=> choose (appRoutes baseDirectory isDynamic); HEAD >=> headHandler; error404Handler ]

let internalErrorHandler =
    fun (ex: Exception) (logger: ILogger) ->
        logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
        error500Handler

let configureApp isDynamic (app: IApplicationBuilder) =
    let staticFileOptions = StaticFileOptions()

    let prepareResponse =
        fun (context: StaticFileResponseContext) ->
            let headers = context.Context.Response.Headers
            headers.CacheControl <- "public,max-age=604800"
            headers.Expires <- DateTimeOffset.UtcNow.AddDays(7).ToString("R")

    staticFileOptions.OnPrepareResponse <- prepareResponse
    
    let env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>()

    let baseDirectory = if isDynamic then env.ContentRootPath else AppContext.BaseDirectory

    app
        .UseGiraffeErrorHandler(internalErrorHandler)
        .UseHttpsRedirection()
        .UseStaticFiles(staticFileOptions)
        .UseResponseCaching()
        .UseGiraffe(routes baseDirectory isDynamic)

let configureServices (services: IServiceCollection) =
    let sp = services.BuildServiceProvider()
    let env = sp.GetService<IWebHostEnvironment>()
    services.AddCors().AddResponseCaching().AddGiraffe() |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

type AppArgs =
    { HostAddress: string
      HostPort: string
      IsDynamic: bool }

let getAppArgs args =
    let argList = Array.toList args
    match argList with
    | hostAddress :: hostPort :: rest ->
        Some
            { HostAddress = hostAddress
              HostPort = hostPort
              IsDynamic = List.contains "--dynamic" rest }
    | _ -> None

[<EntryPoint>]
let main args =
    // Intentional unrecoverable failure if incorrect
    let appArgs = getAppArgs args |> Option.get
    let hostAddress = appArgs.HostAddress
    let hostPort = appArgs.HostPort
    let dynamic = appArgs.IsDynamic

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseUrls($"http://{hostAddress}:{hostPort}")
                .UseWebRoot(Path.Combine(AppContext.BaseDirectory))
                .UseWebRoot(Path.Combine(AppContext.BaseDirectory, "WebRoot"))
                .Configure(Action<IApplicationBuilder> (configureApp dynamic))
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
