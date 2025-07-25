module AppHandlers

open System
open System.IO
open System.Collections.Generic
open Giraffe
open Giraffe.Razor
open Markdig
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc.ModelBinding
open Microsoft.AspNetCore.Mvc.Razor
open Microsoft.AspNetCore.Mvc.ViewFeatures
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks
open RazorEngine

open Types
open Yaml

let error404Msg = "The page that you are looking for does not exist!"
let error500Msg = "Internal server error"

let razorViewHandler markdownViewName (viewData: IDictionary<string, obj>) =
    let formattedRazorHtmlView (razorRenderPair: RazorRenderPair) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let viewName = razorRenderPair.ViewName
                let viewData = razorRenderPair.ViewData
                let metadataProvider = ctx.RequestServices.GetService<IModelMetadataProvider>()
                let engine = ctx.RequestServices.GetService<IRazorViewEngine>()

                let tempDataDict =
                    ctx.RequestServices.GetService<ITempDataDictionaryFactory>().GetTempData ctx

                let! result = renderView engine metadataProvider tempDataDict ctx viewName None (Some viewData) None

                match result with
                | Ok output ->
                    return!
                        (setHttpHeader "Content-Type" "text/html; charset=utf-8"
                         >=> setBodyFromString output)
                            next
                            ctx
                | Error msg ->
                    return! (setStatusCode 500 >=> text ($"Critical Razor view rendering error: {msg}")) next ctx
            }

    let isStandardView = viewData.ContainsKey "Header" && viewData.ContainsKey "Body"
    let isErrorView = viewData.ContainsKey "Body" && viewData.ContainsKey "ErrorCode"

    let razorRenderPair =
        match markdownViewName with
        | DirectMarkdown when isStandardView ->
            { ViewName = "DirectMarkdown"
              ViewData = viewData }
        | LeftHeaderMarkdown when isStandardView ->
            { ViewName = "LeftHeaderMarkdown"
              ViewData = viewData }
        | PostMarkdown when isStandardView ->
            { ViewName = "PostMarkdown"
              ViewData = viewData }
        | ErrorMarkdown when isErrorView ->
            { ViewName = "ErrorMarkdown"
              ViewData = viewData }
        | _ ->
            let errorData = dict [ ("ErrorCode", box 500); ("Body", box error500Msg) ]

            { ViewName = "ErrorMarkdown"
              ViewData = errorData }

    publicResponseCaching 60 None >=> formattedRazorHtmlView razorRenderPair


let markdownFileHandler markdownRoot markdownPath markdownViewName markdownHeader (maybeMetaPageTitle: string option) =
    let htmlContentsFromFile =
        MarkdownPath.toString markdownPath
        |> File.ReadAllText
        |> fun markdownContents -> Markdown.ToHtml(markdownContents, markdownPipeline)
        |> _.Replace("&#8617", "&#8617&#65038")

    let landingPostList =
        String.Join("\n", Array.concat [ [| "<ul>" |]; (postLinksFromYamlHeaders markdownRoot); [| "</ul>" |] ])

    let htmlContents =
        match markdownFileName markdownPath with
        | "landing" -> [ htmlContentsFromFile; landingPostList ] |> String.concat "\n"
        | _ -> htmlContentsFromFile

    let metaPageTitle = Option.defaultValue markdownHeader maybeMetaPageTitle

    razorViewHandler
        markdownViewName
        (dict
            [ ("Body", box htmlContents)
              ("Header", box markdownHeader)
              ("Title", box metaPageTitle) ])

let errorRazorViewHandler errorCode body =
    razorViewHandler ErrorMarkdown (dict [ ("ErrorCode", box errorCode); ("Body", box body) ])

let headHandler: Handler = setStatusCode 200

let error404Handler: Handler =
    setStatusCode 404
    >=> publicResponseCaching 60 None
    >=> errorRazorViewHandler 404 error404Msg

let error500Handler: Handler =
    clearResponse >=> setStatusCode 500 >=> errorRazorViewHandler 500 error500Msg

let markdownRouteHandler markdownRoot markdownPath : HttpHandler =
    //Note: providing dependencies via functions works better due to partial application here
    let render = markdownFileHandler markdownRoot markdownPath

    match MarkdownPath.toString markdownPath |> Path.GetFileName with
    | "landing.md" ->
        route "/"
        >=> render LeftHeaderMarkdown "Iain Schmitt" (Some "Iain Schmitt's Personal Website")
    | "uses.md" -> route "/uses" >=> render PostMarkdown "Iain Schmitt's Uses Page" None
    | "resume.md" -> route "/resume" >=> render LeftHeaderMarkdown "Iain Schmitt's Resume" None
    | _ ->
        route $"/post/{markdownFileName markdownPath}"
        >=> render PostMarkdown "Iain Schmitt" (maybeYamlHeader markdownPath |> Option.map _.Title)

let markdownRoutes (webRoot: String) : list<HttpHandler> =
    let markdownRoot = Path.Combine(webRoot, "markdown")

    markdownPaths markdownRoot
    |> Array.map (markdownRouteHandler markdownRoot)
    |> Array.toList

let pdfHandler webRoot pdfFileName : HttpHandler =
    let pdfPath = Path.Combine(webRoot, "pdf", pdfFileName)
    streamFile true pdfPath None None

let nonHtmlRoutes webRoot =
    [ route "/wedding/julias-game"
      >=> redirectTo true "https://connectionsgame.org/game/?661NPZ"
      route "/wedding/iains-game"
      >=> redirectTo true "https://connectionsgame.org/game/?X5SMRJ" ]

let appRoutes webRoot =
    (markdownRoutes webRoot) @ (nonHtmlRoutes webRoot)
