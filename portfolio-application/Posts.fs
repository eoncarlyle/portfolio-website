module Yaml

open Types
open System.IO
open Markdig
open Markdig.Extensions.Yaml
open Markdig.Syntax
open System
open Views

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

let maybeYamlHeader (markdownPath: MarkdownPath) =
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

let getPostYamlHeaders markdownRoot : PostYamlHeaderPair array =
    getMarkdownPaths markdownRoot
    |> Array.map (fun markdownPath ->
        let maybeHeader = maybeYamlHeader markdownPath

        match maybeHeader with
        | Some header -> Some { Path = markdownPath; Header = header }
        | _ -> None)
    |> Array.choose id
    |> Array.sortBy (fun pair -> DateTime.Parse(pair.Header.Date))

let postLinksFromYamlHeaders markdownRoot =
    getPostYamlHeaders markdownRoot |> Array.map getHeaderHtml |> Array.rev

let rssChannel (baseUrl: string) (markdownRoot: string) =
    let posts = getPostYamlHeaders markdownRoot

    let items =
        posts
        |> Array.map (fun pair ->
            let content =
                MarkdownPath.toString pair.Path
                |> File.ReadAllText
                |> fun md -> Markdown.ToHtml(md, markdownPipeline)

            rssItem pair content baseUrl)
        |> Array.toList

    rssChannelView "iainschmitt.com" baseUrl "Iain Schmitt's Blog" items
