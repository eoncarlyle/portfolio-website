module AppHandlers

open System
open System.IO
open System.Collections.Generic
open Giraffe
open Markdig
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc.ModelBinding
open Microsoft.AspNetCore.Mvc.Razor
open Microsoft.AspNetCore.Mvc.ViewFeatures
open Microsoft.Extensions.DependencyInjection
open Giraffe.ViewEngine

open Types
open Yaml
open Views

let error404Msg = "The page that you are looking for does not exist!"
let error500Msg = "Internal server error"

type Roots =
    { StaticMarkdownRoot: String
      PostMarkdownRoot: String }

type ViewData =
    { PageTitle: String
      Header: String
      Body: String
      ErrorCode: int option }

let viewHandler markdownViewName (viewData: ViewData) =
    let pageTitle = viewData.PageTitle
    let header = viewData.Header
    let body = viewData.Body
    let errorCode = viewData.ErrorCode

    let xml =
        match markdownViewName with
        | DirectMarkdown -> directMarkdownView pageTitle body
        | LeftHeaderMarkdown -> leftHeaderMarkdownView pageTitle header body
        | PostMarkdown -> postMarkdownView pageTitle header body
        | ErrorMarkdown -> Option.defaultValue 500 errorCode |> errorView pageTitle body

    publicResponseCaching 60 None
    >=> setHttpHeader "Content-Type" "text/html; charset=utf-8"
    >=> htmlView xml

let renderMarkdown postMarkdownRoot markdownPath =
    let htmlContentsFromFile =
        MarkdownPath.toString markdownPath
        |> File.ReadAllText
        |> fun markdownContents -> Markdown.ToHtml(markdownContents, markdownPipeline)
        |> _.Replace("&#8617", "&#8617&#65038")

    let landingPostList =
        String.Join("\n", Array.concat [ [| "<ul>" |]; (postLinksFromYamlHeaders postMarkdownRoot); [| "</ul>" |] ])

    match markdownFileName markdownPath with
    | "landing" -> [ htmlContentsFromFile; landingPostList ] |> String.concat "\n"
    | _ -> htmlContentsFromFile

let markdownFileHandler
    isDynamic
    postMarkdownRoot
    markdownPath
    markdownViewName
    markdownHeader
    (maybeMetaPageTitle: string option)
    =
    let metaPageTitle = Option.defaultValue markdownHeader maybeMetaPageTitle

    if isDynamic then
        fun next ctx ->
            let htmlContents = renderMarkdown postMarkdownRoot markdownPath

            (viewHandler
                markdownViewName
                { PageTitle = metaPageTitle
                  Header = markdownHeader
                  Body = htmlContents
                  ErrorCode = None })
                next
                ctx
    else
        let htmlContents = renderMarkdown postMarkdownRoot markdownPath

        viewHandler
            markdownViewName
            { PageTitle = metaPageTitle
              Header = markdownHeader
              Body = htmlContents
              ErrorCode = None }

let errorViewHandler errorCode body =
    viewHandler
        ErrorMarkdown
        { PageTitle = "Error Page"
          Header = "Error Page"
          Body = body
          ErrorCode = Some errorCode }

let headHandler: Handler = setStatusCode 200

let error404Handler: Handler =
    setStatusCode 404
    >=> publicResponseCaching 60 None
    >=> errorViewHandler 404 error404Msg

let error500Handler: Handler =
    clearResponse >=> setStatusCode 500 >=> errorViewHandler 500 error500Msg

let markdownRouteHandler isDynamic postMarkdownRoot markdownPath : HttpHandler =
    //Note: providing dependencies via functions works better due to partial application here
    let render = markdownFileHandler isDynamic postMarkdownRoot markdownPath

    match MarkdownPath.toString markdownPath |> Path.GetFileName with
    | "landing.md" ->
        route "/"
        >=> render LeftHeaderMarkdown "Iain Schmitt" (Some "Iain Schmitt's Personal Website")
    | "uses.md" -> route "/uses" >=> render PostMarkdown "Iain Schmitt's Uses Page" None
    | "resume.md" -> route "/resume" >=> render LeftHeaderMarkdown "Iain Schmitt's Resume" None
    | _ ->
        route $"/post/{markdownFileName markdownPath}"
        >=> render PostMarkdown "Iain Schmitt" (maybeYamlHeader markdownPath |> Option.map _.Title)

let getWebRoot webRoot = Path.Combine(webRoot, "WebRoot")

let markdownRoutes isDynamic (baseDirectory: String) : list<HttpHandler> =

    let postMarkdownRoot =
        if isDynamic then
            Path.Combine(baseDirectory, "posts")
        else
            Path.Combine(baseDirectory, "WebRoot", "markdown")


    let markdownPaths =
        [| postMarkdownRoot; Path.Combine(baseDirectory, "WebRoot", "markdown") |]
        |> Array.map getMarkdownPaths
        |> Array.concat

    markdownPaths
    |> Array.map (markdownRouteHandler isDynamic postMarkdownRoot)
    |> Array.map (fun handler -> if isDynamic then warbler (fun _ -> handler) else handler)
    |> Array.toList

let pdfHandler webRoot pdfFileName : HttpHandler =
    let pdfPath = Path.Combine(webRoot, "pdf", pdfFileName)
    streamFile true pdfPath None None

let rssHandler markdownRoot (baseUrl: string) : HttpHandler =
    let rss = rssChannel markdownRoot baseUrl

    fun _ ctx ->
        let xml = RenderView.AsString.xmlNode rss
        ctx.SetContentType "application/rss+xml; charset=utf-8"
        ctx.WriteStringAsync xml

let nonHtmlRoutes webRoot =
    [ route "/rss" >=> rssHandler (getWebRoot webRoot) "https://iainschmitt.com"
      route "/wedding/julias-game"
      >=> redirectTo true "https://connectionsgame.org/game/?661NPZ"
      route "/wedding/iains-game"
      >=> redirectTo true "https://connectionsgame.org/game/?X5SMRJ"
      route "/pdf/2025-tech-jam-talk"
      >=> pdfHandler webRoot "2025-TechJam-BearTerritory.pdf" ]

let appRoutes baseDirectory isDynamic =
    (markdownRoutes isDynamic baseDirectory) @ (nonHtmlRoutes baseDirectory)
