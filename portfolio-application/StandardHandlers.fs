module StandardHandlers

open System
open System.IO
open Giraffe
open Markdig
open Giraffe.ViewEngine

open Types
open Yaml
open Views

type private Roots =
    { StaticMarkdownRoot: String
      PostMarkdownRoot: String }

let viewHandler markdownViewName viewData : HttpHandler =
    let pageTitle = withoutTags viewData.PageTitle
    let body = viewData.Body
    let errorCode = viewData.ErrorCode

    let cacheHandler =
        if viewData.IsCached then
            publicResponseCaching 60 None
        else
            noResponseCaching

    // Decides which nav link is marked current
    fun next ctx ->
        let path = ctx.Request.Path.Value

        let xml =
            match markdownViewName with
            | DirectMarkdown -> directMarkdownView path pageTitle body
            | ErrorMarkdown -> Option.defaultValue 500 errorCode |> errorView pageTitle body

        cacheHandler
        >=> setHttpHeader "Content-Type" "text/html; charset=utf-8"
        >=> htmlView xml
        <|| (next, ctx)

type private IncludedPostLinks =
    | NoPostLinks
    | AllPostLinks
    | LimitedPostLinks of int

let private markdownFileHandler
    isStatic
    postMarkdownRoot
    markdownPath
    markdownViewName
    metaPageTitle
    includedPostLinks
    : HttpHandler =

    let postListHtml postMarkdownRoot maybeLimit =
        let postLinkList =
            match maybeLimit with
            | Some(limit) -> postLinksFromYamlHeaders postMarkdownRoot |> Array.take limit
            | _ -> postLinksFromYamlHeaders postMarkdownRoot

        String.Join("\n", Array.concat [ [| "<ul>" |]; postLinkList; [| "</ul>" |] ])

    let getHtmlContents () =
        let htmlContentsFromFile =
            MarkdownPath.toString markdownPath
            |> File.ReadAllText
            |> fun markdownContents -> Markdown.ToHtml(markdownContents, markdownPipeline)
            |> _.Replace("&#8617", "&#8617&#65038")
            |> _.Replace("<em>", "<i>")
            |> _.Replace("</em>", "</i>")

        match includedPostLinks with
        | NoPostLinks -> htmlContentsFromFile
        | LimitedPostLinks(count) ->
            [ htmlContentsFromFile; postListHtml postMarkdownRoot <| Some count ]
            |> String.concat "\n"
        | AllPostLinks ->
            [ htmlContentsFromFile; postListHtml postMarkdownRoot None ]
            |> String.concat "\n"

    let viewData body =
        { PageTitle = metaPageTitle
          Body = body
          IsCached = isStatic
          ErrorCode = None }

    if isStatic then
        getHtmlContents () |> viewData |> viewHandler markdownViewName
    else
        warbler (fun _ -> getHtmlContents () |> viewData |> viewHandler markdownViewName)

let getPostMarkdownRoot baseDirectory =
    Path.Combine(baseDirectory, "posts", "source")

let markdownRoutes isStatic (baseDirectory: String) : HttpHandler list =

    let postMarkdownRoot = getPostMarkdownRoot baseDirectory

    let markdownPaths =
        [| postMarkdownRoot; Path.Combine(baseDirectory, "WebRoot", "markdown") |]
        |> Array.map getMarkdownPaths
        |> Array.concat

    let markdownRouteHandler isStatic postMarkdownRoot markdownPath : HttpHandler =
        //Note: providing dependencies via functions works better due to partial application here
        let render =
            markdownFileHandler isStatic postMarkdownRoot markdownPath DirectMarkdown

        match MarkdownPath.toString markdownPath |> Path.GetFileName with
        | "landing.md" -> route "/" >=> render "Iain Schmitt's Personal Website" (LimitedPostLinks 5)
        | "posts.md" -> route "/posts" >=> render "Iain Schmitt's Resume" AllPostLinks
        | "uses.md" -> route "/uses" >=> render "Iain Schmitt's Uses Page" NoPostLinks
        | "resume.md" -> route "/resume" >=> render "Iain Schmitt's Resume" NoPostLinks
        | _ ->
            route $"/post/{markdownFileName markdownPath}"
            >=> render
                    (maybeYamlHeader markdownPath
                     |> Option.map _.Title
                     |> Option.defaultValue "Iain Schmitt's Personal Website")
                    NoPostLinks

    markdownPaths
    |> Array.map (markdownRouteHandler isStatic postMarkdownRoot)
    |> Array.toList
