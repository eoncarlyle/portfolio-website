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
    let header = viewData.Header
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
            | PostMarkdown -> postMarkdownView path pageTitle header body
            | ErrorMarkdown -> Option.defaultValue 500 errorCode |> errorView pageTitle body

        cacheHandler
        >=> setHttpHeader "Content-Type" "text/html; charset=utf-8"
        >=> htmlView xml
        <|| (next, ctx)

let private postListHtml postMarkdownRoot maybeLimit =
    let postLinkList =
        match maybeLimit with
        | Some(limit) -> postLinksFromYamlHeaders postMarkdownRoot |> Array.take limit
        | _ -> postLinksFromYamlHeaders postMarkdownRoot

    String.Join("\n", Array.concat [ [| "<ul>" |]; postLinkList; [| "</ul>" |] ])

let private markdownFileHandler
    isStatic
    postMarkdownRoot
    markdownPath
    markdownViewName
    markdownHeader
    (maybeMetaPageTitle: string option)
    : HttpHandler =
    let metaPageTitle = Option.defaultValue markdownHeader maybeMetaPageTitle

    let getHtmlContents () =
        let htmlContentsFromFile =
            MarkdownPath.toString markdownPath
            |> File.ReadAllText
            |> fun markdownContents -> Markdown.ToHtml(markdownContents, markdownPipeline)
            |> _.Replace("&#8617", "&#8617&#65038")
            |> _.Replace("<em>", "<i>")
            |> _.Replace("</em>", "</i>")

        match markdownFileName markdownPath with
        | "landing" -> [ htmlContentsFromFile; postListHtml postMarkdownRoot (Some 5) ] |> String.concat "\n"
        | _ -> htmlContentsFromFile

    let viewData body =
        { PageTitle = metaPageTitle
          Header = markdownHeader
          Body = body
          IsCached = isStatic
          ErrorCode = None }

    if isStatic then
        getHtmlContents () |> viewData |> viewHandler markdownViewName
    else
        warbler (fun _ -> getHtmlContents () |> viewData |> viewHandler markdownViewName)

let private getPostMarkdownRoot baseDirectory =
    Path.Combine(baseDirectory, "posts", "source")

let private markdownRoutes isStatic (baseDirectory: String) : HttpHandler list =

    let postMarkdownRoot = getPostMarkdownRoot baseDirectory

    let markdownPaths =
        [| postMarkdownRoot; Path.Combine(baseDirectory, "WebRoot", "markdown") |]
        |> Array.map getMarkdownPaths
        |> Array.concat

    let markdownRouteHandler isStatic postMarkdownRoot markdownPath : HttpHandler =
        //Note: providing dependencies via functions works better due to partial application here
        let render = markdownFileHandler isStatic postMarkdownRoot markdownPath

        match MarkdownPath.toString markdownPath |> Path.GetFileName with
        | "landing.md" ->
            route "/"
            >=> render DirectMarkdown "Iain Schmitt" (Some "Iain Schmitt's Personal Website")
        | "uses.md" -> route "/uses" >=> render PostMarkdown "Iain Schmitt's Uses Page" None
        | "resume.md" -> route "/resume" >=> render DirectMarkdown "Iain Schmitt's Resume" None
        | _ ->
            route $"/post/{markdownFileName markdownPath}"
            >=> render PostMarkdown "Iain Schmitt" (maybeYamlHeader markdownPath |> Option.map _.Title)

    // /posts is generated, there isn't a good 
    let postsRoute: HttpHandler =
        let render () =
            viewHandler
                DirectMarkdown
                { PageTitle = "Iain Schmitt's Posts"
                  Header = "Posts"
                  Body = $"<h1>Posts</h1>\n{postListHtml postMarkdownRoot None}"
                  IsCached = isStatic
                  ErrorCode = None }

        route "/posts"
        >=> if isStatic then render () else warbler (fun _ -> render ())

    postsRoute
    :: (markdownPaths
        |> Array.map (markdownRouteHandler isStatic postMarkdownRoot)
        |> Array.toList)


let private nonHtmlRoutes baseDirectory =
    let pdfHandler baseDirectory pdfFileName : HttpHandler =
        let pdfPath = Path.Combine(baseDirectory, "pdf", pdfFileName)
        streamFile true pdfPath None None

    let rssHandler (baseDirectory: string) (baseUrl: string) : HttpHandler =
        let postMarkdownRoot = getPostMarkdownRoot baseDirectory
        let rss = rssChannel baseUrl postMarkdownRoot

        fun _ ctx ->
            let xml = RenderView.AsString.xmlNode rss
            ctx.SetContentType "application/rss+xml; charset=utf-8"
            ctx.WriteStringAsync xml

    [ route "/rss" >=> rssHandler baseDirectory "https://iainschmitt.com"
      route "/wedding/julias-game"
      >=> redirectTo true "https://connectionsgame.org/game/?661NPZ"
      route "/wedding/iains-game"
      >=> redirectTo true "https://connectionsgame.org/game/?X5SMRJ"
      route "/pdf/2025-tech-jam-talk"
      >=> pdfHandler baseDirectory "2025-TechJam-BearTerritory.pdf" ]

let appRoutes baseDirectory isStatic =
    (markdownRoutes isStatic baseDirectory) @ (nonHtmlRoutes baseDirectory)
