module AppHandlers

open System.IO
open System.Collections.Generic
open Giraffe
open Giraffe.Razor
open Markdig
open Microsoft.AspNetCore

type MarkdownViewName =
    | DirectMarkdown
    | LeftHeaderMarkdown
    | PostMarkdown
    | ErrorMarkdown

type MarkdownPath = private MarkdownPath of string

type Handler = HttpFunc -> Http.HttpContext -> HttpFuncResult

module MarkdownPath =
    let create path =
        match path with
        | path when (File.Exists path) && (Path.GetExtension path = ".md") -> Some(MarkdownPath path)
        | _ -> None

    let toString (MarkdownPath path) = path

let contentRoot = Directory.GetCurrentDirectory()
let webRoot = Path.Combine(contentRoot, "WebRoot")
let markdownRoot = Path.Combine(webRoot, "markdown")

let error404Msg = "The page that you are looking for does not exist!"
let error500Msg = "Internal server error"

let markdownPipeline = MarkdownPipelineBuilder().UseAdvancedExtensions().Build()

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
    >=> razorHtmlView (fst renderTuple) None (snd renderTuple) None

let markdownFileHandler markdownViewName markdownPath header =
    let htmlContents =
        MarkdownPath.toString markdownPath
        |> File.ReadAllText
        |> fun markdownContents -> Markdown.ToHtml(markdownContents, markdownPipeline)
        |> _.Replace("&#8617", "&#8617&#65038")

    razorViewHandler markdownViewName (dict [ ("Body", box htmlContents); ("Header", box header) ])

let errorRazorViewHandler errorCode body =
    razorViewHandler ErrorMarkdown (dict [ ("ErrorCode", box errorCode); ("Body", box body) ])

let error404Handler: Handler =
    setStatusCode 404
    >=> publicResponseCaching 60 None
    >=> errorRazorViewHandler 404 error404Msg

let error500Handler: Handler =
    clearResponse >=> setStatusCode 500 >=> errorRazorViewHandler 500 error500Msg

let createRouteHandler markdownPath =
    match MarkdownPath.toString markdownPath |> Path.GetFileName with
    | "landing.md" -> route "/" >=> markdownFileHandler LeftHeaderMarkdown markdownPath "Iain Schmitt"
    | "resume.md" ->
        route "/resume"
        >=> markdownFileHandler LeftHeaderMarkdown markdownPath "Iain Schmitt's Resume"
    | _ ->
        route $"/post/{Path.GetFileNameWithoutExtension(MarkdownPath.toString markdownPath)}"
        >=> markdownFileHandler PostMarkdown markdownPath "Iain Schmitt"

let appRoutes: list<HttpHandler> =
    Directory.GetFiles markdownRoot
    |> Array.choose MarkdownPath.create
    |> Array.map createRouteHandler
    |> Array.toList
