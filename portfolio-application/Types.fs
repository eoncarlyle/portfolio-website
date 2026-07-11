module Types

open System
open System.IO
open Microsoft.AspNetCore.Http
open Giraffe
open Markdig
open System.Collections.Generic
open System.Text.RegularExpressions

type MarkdownViewName =
    | DirectMarkdown
    | LeftHeaderMarkdown
    | PostMarkdown
    | ErrorMarkdown

type MarkdownPath = private MarkdownPath of string

type Handler = HttpFunc -> HttpContext -> HttpFuncResult

type PostYamlHeader = { Title: string; Date: String }

type PostYamlHeaderPair =
    { Path: MarkdownPath
      Header: PostYamlHeader }

type RazorRenderPair =
    { ViewName: string
      ViewData: IDictionary<string, obj> }

module MarkdownPath =
    let create path =
        match path with
        | path when (File.Exists path) && (Path.GetExtension path = ".md") -> Some(MarkdownPath path)
        | _ -> None

    let toString (MarkdownPath path) = path

let markdownPipeline =
    let builder = MarkdownPipelineBuilder().UseAdvancedExtensions().UseYamlFrontMatter()
    builder.Extensions.Add(ShikiMarkdig.ShikiExtension(ShikiMarkdig.shikiHighlighter))
    builder.Build()

let getMarkdownPaths path =
    Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly) |> Array.choose MarkdownPath.create

let markdownFileName markdownPath =
    Path.GetFileNameWithoutExtension(MarkdownPath.toString markdownPath)

let withoutTags value = Regex.Replace(value, "<[^>]*>", "")
