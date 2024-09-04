module AppHandlers

open System.IO
open System.Collections.Generic
open Giraffe
open Giraffe.Razor
open FSharp.Formatting.Markdown

type MarkdownViewName =
    | DirectMarkdown
    | ResumeMarkdown
    | ErrorMarkdown

type MarkdownPath = private MarkdownPath of string

module MarkdownPath =
    let create (path: string) =
        match path with
        | path when (File.Exists path) && (Path.GetExtension path = ".md") -> Some(MarkdownPath path)
        | _ -> None

    let toString (MarkdownPath path) = path

let markdownFilesDirectory = "./WebRoot/markdown"
let error400Msg = "The page that you are looking for does not exist!"
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

let razorViewHandler markdownViewName (viewData: IDictionary<string, obj>) =
    let renderTuple =
        match markdownViewName with
        | markdownViewName when (markdownViewName = DirectMarkdown) && isStandardView viewData ->
            ("DirectMarkdown", Some viewData)
        | markdownViewName when (markdownViewName = ResumeMarkdown) && isStandardView viewData ->
            ("ResumeMarkdown", Some viewData)
        | markdownViewName when (markdownViewName = ErrorMarkdown) && isErrorView viewData ->
            ("ErrorMarkdown", Some viewData)
        | _ -> ("ErrorMarkdown", dict [ ("ErrorCode", box 500); ("Body", box error500Msg) ] |> Some)

    publicResponseCaching 60 None
    >=> razorHtmlView (fst renderTuple) None (snd renderTuple) None

let markdownFileHandler markdownViewName (markdownPath: MarkdownPath) =
    let fileContents =
        MarkdownPath.toString markdownPath |> File.ReadAllText |> Markdown.ToHtml

    razorViewHandler markdownViewName (dict [ ("Body", box fileContents) ])

let errorRazorViewHandler errorCode body =
    razorViewHandler ErrorMarkdown (dict [ ("ErrorCode", box errorCode); ("Body", box body) ])

let getRoute (markdownPath: MarkdownPath) =
    match MarkdownPath.toString markdownPath |> Path.GetFileName with
    | "landing.md" -> route "/" >=> markdownFileHandler DirectMarkdown markdownPath
    | "resume.md" -> route "/resume" >=> markdownFileHandler ResumeMarkdown markdownPath
    | _ ->
        route $"/post/{Path.GetFileNameWithoutExtension(MarkdownPath.toString markdownPath)}"
        >=> markdownFileHandler DirectMarkdown markdownPath

let appRoutes: list<HttpHandler> =
    Directory.GetFiles markdownFilesDirectory
    |> Array.map MarkdownPath.create
    |> Array.filter Option.isSome
    |> Array.map Option.get
    |> Array.map getRoute
    |> Array.toList
