module AppHandlers

open System.IO
open Giraffe
open Giraffe.Razor
open FSharp.Formatting.Markdown

type MarkdownViewName =
    | DirectMarkdown
    | ResumeMarkdown
    | Error

let markdownFilesDirectory = "./WebRoot/markdown"

let getMarkdownFilePaths =
    Directory.GetFiles markdownFilesDirectory
    |> Array.filter (fun path -> Path.GetExtension path = ".md")

let razorViewHandler markdownViewName fileContents =
    let viewData = dict [ ("Body", box fileContents) ] |> Some

    let viewName =
        match markdownViewName with
        | DirectMarkdown -> "DirectMarkdown"
        | ResumeMarkdown -> "ResumeMarkdown"
        | Error -> "Error"

    publicResponseCaching 60 None >=> razorHtmlView viewName None viewData None

let errorHandler errorCode body =
    let viewData = dict [ ("ErrorCode", box errorCode); ("Body", box body) ] |> Some
    razorHtmlView "Error" None viewData None

let notFoundHandler =
    errorHandler 404 "The page that you are looking for does not exist!"

let markdownFileHandler appViewName (markdownFilePath: string) =
    match File.Exists markdownFilePath with
    | true ->
        File.ReadAllText markdownFilePath
        |> Markdown.ToHtml
        |> razorViewHandler appViewName
    | false -> notFoundHandler

let getRoute (markdownFilePath: string) =
    match Path.GetFileName markdownFilePath with
    | "landing.md" -> route "/" >=> markdownFileHandler DirectMarkdown markdownFilePath
    | "resume.md" -> route "/resume" >=> markdownFileHandler ResumeMarkdown markdownFilePath
    | _ ->
        route $"/post/{Path.GetFileNameWithoutExtension markdownFilePath}"
        >=> markdownFileHandler DirectMarkdown markdownFilePath

let appRoutes: list<HttpHandler> =
    Array.map getRoute getMarkdownFilePaths |> Array.toList
