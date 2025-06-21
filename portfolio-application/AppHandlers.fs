module AppHandlers

open System
open System.IO
open System.Collections.Generic
open Giraffe
open Giraffe.Razor
open Giraffe.ViewEngine
open Markdig
open Markdig.Extensions.Yaml
open Markdig.Syntax
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Components.RenderTree
open Microsoft.AspNetCore.Http
open System.Text
open System.Threading.Tasks
open System.Collections.Generic
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc.ModelBinding
open Microsoft.AspNetCore.Mvc.Razor
open Microsoft.AspNetCore.Mvc.ViewFeatures
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Antiforgery
open FSharp.Control.Tasks
open RazorEngine

open Types
open Yaml

let error404Msg = "The page that you are looking for does not exist!"
let error500Msg = "Internal server error"


// Inline the _razorHtmlView and prevent this from _ever_ erroring
let _razorView
    (contentType: string)
    (viewName: string)
    (model: 'T option)
    (viewData: IDictionary<string, obj> option)
    (modelState: ModelStateDictionary option)
    : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let metadataProvider = ctx.RequestServices.GetService<IModelMetadataProvider>()
            let engine = ctx.RequestServices.GetService<IRazorViewEngine>()

            let tempDataDict =
                ctx.RequestServices.GetService<ITempDataDictionaryFactory>().GetTempData ctx

            let! result = renderView engine metadataProvider tempDataDict ctx viewName model viewData modelState

            match result with
            | Error msg -> return (failwith msg)
            | Ok output ->
                let bytes = Encoding.UTF8.GetBytes output
                return! (setHttpHeader "Content-Type" contentType >=> setBody bytes) next ctx
        }

let _razorHtmlView
    (viewName: string)
    (model: 'T option)
    (viewData: IDictionary<string, obj> option)
    (modelState: ModelStateDictionary option)
    : HttpHandler =
    _razorView "text/html; charset=utf-8" viewName model viewData modelState

let razorViewHandler markdownViewName (viewData: IDictionary<string, obj>) =
    let isStandardView = viewData.ContainsKey "Header" && viewData.ContainsKey "Body"

    let isErrorView = viewData.ContainsKey "Body" && viewData.ContainsKey "ErrorCode"

    let renderTuple =
        match markdownViewName with
        | DirectMarkdown when isStandardView -> "DirectMarkdown", Some viewData
        | LeftHeaderMarkdown when isStandardView -> "LeftHeaderMarkdown", Some viewData
        | PostMarkdown when isStandardView -> "PostMarkdown", Some viewData
        | ErrorMarkdown when isErrorView -> "ErrorMarkdown", Some viewData
        | _ ->
            let errorData = dict [ ("ErrorCode", box 500); ("Body", box error500Msg) ]
            "ErrorMarkdown", Some errorData

    publicResponseCaching 60 None
    >=> _razorHtmlView (fst renderTuple) None (snd renderTuple) None

let landingPostList markdownRoot =
    String.Join("\n", Array.concat [ [| "<ul>" |]; (postLinksFromYamlHeaders markdownRoot); [| "</ul>" |] ])

let markdownFileHandler markdownViewName markdownRoot markdownPath bodyHeader (pageTitle: string option) =
    let htmlContentsFromFile =
        MarkdownPath.toString markdownPath
        |> File.ReadAllText
        |> fun markdownContents -> Markdown.ToHtml(markdownContents, markdownPipeline)
        |> _.Replace("&#8617", "&#8617&#65038")

    let htmlContents =
        match markdownFileName markdownPath with
        | "landing" -> [ htmlContentsFromFile; landingPostList markdownRoot ] |> String.concat "\n"
        | _ -> htmlContentsFromFile

    razorViewHandler markdownViewName (dict [ ("Body", box htmlContents); ("Header", box bodyHeader) ])

let errorRazorViewHandler errorCode body =
    razorViewHandler ErrorMarkdown (dict [ ("ErrorCode", box errorCode); ("Body", box body) ])

let headHandler: Handler = setStatusCode 200

let error404Handler: Handler =
    setStatusCode 404
    >=> publicResponseCaching 60 None
    >=> errorRazorViewHandler 404 error404Msg

let error500Handler: Handler =
    clearResponse >=> setStatusCode 500 >=> errorRazorViewHandler 500 error500Msg

let createRouteHandler markdownRoot markdownPath =
    match MarkdownPath.toString markdownPath |> Path.GetFileName with
    | "landing.md" ->
        route "/"
        >=> markdownFileHandler
                LeftHeaderMarkdown
                markdownRoot
                markdownPath
                "Iain Schmitt"
                (Some "Iain Schmitt's Personal Website")
    | "uses.md" ->
        route "/uses"
        >=> markdownFileHandler PostMarkdown markdownRoot markdownPath "Iain Schmitt's Uses Page" None
    | "resume.md" ->
        route "/resume"
        >=> markdownFileHandler LeftHeaderMarkdown markdownRoot markdownPath "Iain Schmitt's Resume" None
    | _ ->
        route $"/post/{markdownFileName markdownPath}"
        >=> markdownFileHandler
                PostMarkdown
                markdownRoot
                markdownPath
                "Iain Schmitt"
                (maybeYamlHeader markdownPath |> Option.map _.Title)


let markdownRoutes (webRoot: String) : list<HttpHandler> =
    let markdownRoot = Path.Combine(webRoot, "markdown")

    markdownPaths markdownRoot
    |> Array.map (createRouteHandler markdownRoot)
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
