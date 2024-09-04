module AppHandlers

open System.IO
open System.Collections.Generic
open Giraffe
open Giraffe.Razor
open FSharp.Formatting.Markdown
open Microsoft.AspNetCore

type MarkdownViewName =
    | DirectMarkdown
    | ResumeMarkdown
    | ErrorMarkdown

type MarkdownPath = private MarkdownPath of string

type Handler = HttpFunc -> Http.HttpContext -> HttpFuncResult

module MarkdownPath =
    let create path =
        match path with
        | path when (File.Exists path) && (Path.GetExtension path = ".md") -> Some(MarkdownPath path)
        | _ -> None

    let toString (MarkdownPath path) = path

let markdownFilesDirectory = "./WebRoot/markdown"
let error404Msg = "The page that you are looking for does not exist!"
let error500Msg = "Internal server error"

let getMarkdownFilePaths =
    Directory.GetFiles markdownFilesDirectory
    |> Array.filter (fun path -> Path.GetExtension path = ".md")

let isStandardView (viewData: IDictionary<string, obj>) =
    viewData.ContainsKey "Body" && viewData.Keys.Count = 1

let isErrorView (viewData: IDictionary<string, obj>) =
    viewData.ContainsKey "Body"
    && viewData.ContainsKey "ErrorCode"
    && viewData.Keys.Count = 2

let razorViewHandler markdownViewName viewData =
    let renderTuple =
        match markdownViewName with
        | DirectMarkdown when isStandardView viewData -> "DirectMarkdown", Some viewData
        | ResumeMarkdown when isStandardView viewData -> "ResumeMarkdown", Some viewData
        | ErrorMarkdown when isErrorView viewData -> "ErrorMarkdown", Some viewData
        | _ ->
            let errorData = dict [ ("ErrorCode", box 500); ("Body", box error500Msg) ]
            "ErrorMarkdown", Some errorData

    publicResponseCaching 60 None
    >=> razorHtmlView (fst renderTuple) None (snd renderTuple) None

let markdownFileHandler markdownViewName markdownPath =
    let fileContents =
        MarkdownPath.toString markdownPath |> File.ReadAllText |> Markdown.ToHtml

    razorViewHandler markdownViewName (dict [ ("Body", box fileContents) ])

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
    | "landing.md" -> route "/" >=> markdownFileHandler DirectMarkdown markdownPath
    | "resume.md" -> route "/resume" >=> markdownFileHandler ResumeMarkdown markdownPath
    | _ ->
        route $"/post/{Path.GetFileNameWithoutExtension(MarkdownPath.toString markdownPath)}"
        >=> markdownFileHandler DirectMarkdown markdownPath

let appRoutes: list<HttpHandler> =
    Directory.GetFiles markdownFilesDirectory
    |> Array.choose MarkdownPath.create
    |> Array.map createRouteHandler
    |> Array.toList
