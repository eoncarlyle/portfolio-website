module AppHandlers

open System
open System.IO
open System.Collections.Generic
open Giraffe
open Giraffe.Razor
open Markdig
open Markdig.Extensions.Yaml
open Markdig.Syntax
open Microsoft.AspNetCore

type MarkdownViewName =
    | DirectMarkdown
    | LeftHeaderMarkdown
    | PostMarkdown
    | ErrorMarkdown

type MarkdownPath = private MarkdownPath of string

type Handler = HttpFunc -> Http.HttpContext -> HttpFuncResult

type PostYamlHeader = { Title: string; Date: String }

type PostYamlHeaderPair =
    { Path: MarkdownPath
      Header: PostYamlHeader }

module MarkdownPath =
    let create path =
        match path with
        | path when (File.Exists path) && (Path.GetExtension path = ".md") -> Some(MarkdownPath path)
        | _ -> None

    let toString (MarkdownPath path) = path

let error404Msg = "The page that you are looking for does not exist!"
let error500Msg = "Internal server error"

let markdownPipeline =
    MarkdownPipelineBuilder().UseAdvancedExtensions().UseYamlFrontMatter().Build()

let markdownPaths markdownRoot =
    Directory.GetFiles markdownRoot |> Array.choose MarkdownPath.create

let markdownFileName markdownPath =
    Path.GetFileNameWithoutExtension(MarkdownPath.toString markdownPath)

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


let postList markdownRoot =

    let tryExtractPostYamlHeaderValue (prefix: string) (line: string) =
        if line.StartsWith(prefix) then
            Some(line.Substring(prefix.Length).Trim())
        else
            None

    let tryParsePostYamlHeader (lines: string array) =
        let tryTitle = lines |> Array.tryPick (tryExtractPostYamlHeaderValue "title:")
        let tryDate = lines |> Array.tryPick (tryExtractPostYamlHeaderValue "date:")

        match tryTitle, tryDate with
        | Some title, Some date -> Some { Title = title; Date = date }
        | _ -> None

    let tryGetHeader (markdownPath: MarkdownPath) =
        MarkdownPath.toString markdownPath
        |> File.ReadAllText
        |> (fun markdownContents ->
            Markdown
                .Parse(markdownContents, markdownPipeline)
                .Descendants<YamlFrontMatterBlock>())
        |> Seq.tryHead
        |> Option.map (fun yamlBlock -> yamlBlock.Lines.Lines |> Seq.map _.ToString() |> Seq.toArray)
        |> Option.bind tryParsePostYamlHeader

    let getHeaderHtml (pair: PostYamlHeaderPair) =
        $"  <li>{pair.Header.Date}: <a href=\"/post/{markdownFileName pair.Path}\">{pair.Header.Title}</a></li>"

    let postLinks =
        markdownPaths markdownRoot
        |> Array.map (fun markdownPath ->
            let maybeHeader = tryGetHeader markdownPath

            match maybeHeader with
            | Some header -> Some { Path = markdownPath; Header = header }
            | _ -> None)
        |> Array.choose id
        |> Array.sortBy (fun pair -> DateTime.Parse(pair.Header.Date))
        |> Array.rev
        |> Array.map getHeaderHtml

    String.Join("\n", Array.concat [ [| "<ul>" |]; postLinks; [| "</ul>" |] ])

let markdownFileHandler markdownViewName markdownRoot markdownPath header =
    let htmlContentsFromFile =
        MarkdownPath.toString markdownPath
        |> File.ReadAllText
        |> fun markdownContents -> Markdown.ToHtml(markdownContents, markdownPipeline)
        |> _.Replace("&#8617", "&#8617&#65038")

    let htmlContents =
        match markdownFileName markdownPath with
        | "landing" -> [ htmlContentsFromFile; postList markdownRoot ] |> String.concat "\n"
        | _ -> htmlContentsFromFile

    razorViewHandler markdownViewName (dict [ ("Body", box htmlContents); ("Header", box header) ])

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
        >=> markdownFileHandler LeftHeaderMarkdown markdownRoot markdownPath "Iain Schmitt"
    | "uses.md" ->
        route "/uses"
        >=> markdownFileHandler PostMarkdown markdownRoot markdownPath "Iain Schmitt's Uses Page"
    | "resume.md" ->
        route "/resume"
        >=> markdownFileHandler LeftHeaderMarkdown markdownRoot markdownPath "Iain Schmitt's Resume"
    | _ ->
        route $"/post/{markdownFileName markdownPath}"
        >=> markdownFileHandler PostMarkdown markdownRoot markdownPath "Iain Schmitt"


let appRoutes (markdownRoot: String) : list<HttpHandler> =
    markdownPaths markdownRoot
    |> Array.map (createRouteHandler markdownRoot)
    |> Array.toList
