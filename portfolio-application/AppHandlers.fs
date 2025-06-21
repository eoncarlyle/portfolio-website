module AppHandlers

open System
open System.IO
open System.Collections.Generic
open AngleSharp.Html
open Giraffe
open Giraffe.Razor
open Markdig
open Microsoft.AspNetCore.Http
open AngleSharp
open AngleSharp.Html.Parser
open AngleSharp.Html.Dom
open Microsoft.AspNetCore.Mvc.ModelBinding
open Microsoft.AspNetCore.Mvc.Razor
open Microsoft.AspNetCore.Mvc.ViewFeatures
open Microsoft.Extensions.DependencyInjection
open FSharp.Control.Tasks
open RazorEngine

open Types
open Yaml

let error404Msg = "The page that you are looking for does not exist!"
let error500Msg = "Internal server error"

let angleSharpParser = HtmlParser()
let angleSharpFormatter: IMarkupFormatter = PrettyMarkupFormatter()

let formattedRazorHtmlView (razorRenderPair: RazorRenderPair) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let viewName = razorRenderPair.ViewName
            let viewData = razorRenderPair.ViewData
            let metadataProvider = ctx.RequestServices.GetService<IModelMetadataProvider>()
            let engine = ctx.RequestServices.GetService<IRazorViewEngine>()

            let tempDataDict =
                ctx.RequestServices.GetService<ITempDataDictionaryFactory>().GetTempData ctx

            let! result = renderView engine metadataProvider tempDataDict ctx viewName None (Some viewData) None

            match result with
            | Error msg -> return! (setStatusCode 500 >=> text ($"Critical Razor view rendering error: {msg}")) next ctx
            | Ok output ->
                let maybeFormattedHtml (htmlString: string): string option =
                    try
                        Some (angleSharpParser.ParseDocument(htmlString).ToHtml(angleSharpFormatter))
                    with
                    | ex -> None

                match maybeFormattedHtml output with
                | Some formattedHtml ->
                    return!
                        (setHttpHeader "Content-Type" "text/html; charset=utf-8"
                         >=> setBodyFromString formattedHtml)
                            next
                            ctx
                | None -> return! (setStatusCode 500 >=> text "Critical HTML view rendering error") next ctx
        }

let razorViewHandler markdownViewName (viewData: IDictionary<string, obj>) =
    let isStandardView = viewData.ContainsKey "Header" && viewData.ContainsKey "Body"

    let isErrorView = viewData.ContainsKey "Body" && viewData.ContainsKey "ErrorCode"

    let razorRenderPair =
        match markdownViewName with
        | DirectMarkdown when isStandardView ->
            { ViewName = "DirectMarkdown"
              ViewData = viewData }
        | LeftHeaderMarkdown when isStandardView ->
            { ViewName = "LeftHeaderMarkdown"
              ViewData = viewData }
        | PostMarkdown when isStandardView ->
            { ViewName = "PostMarkdown"
              ViewData = viewData }
        | ErrorMarkdown when isErrorView ->
            { ViewName = "ErrorMarkdown"
              ViewData = viewData }
        | _ ->
            let errorData = dict [ ("ErrorCode", box 500); ("Body", box error500Msg) ]

            { ViewName = "ErrorMarkdown"
              ViewData = errorData }

    publicResponseCaching 60 None >=> formattedRazorHtmlView razorRenderPair

let landingPostList markdownRoot =
    String.Join("\n", Array.concat [ [| "<ul>" |]; (postLinksFromYamlHeaders markdownRoot); [| "</ul>" |] ])

let markdownFileHandler markdownViewName markdownRoot markdownPath markdownHeader (maybeMetaPageTitle: string option) =
    let htmlContentsFromFile =
        MarkdownPath.toString markdownPath
        |> File.ReadAllText
        |> fun markdownContents -> Markdown.ToHtml(markdownContents, markdownPipeline)
        |> _.Replace("&#8617", "&#8617&#65038")

    let htmlContents =
        match markdownFileName markdownPath with
        | "landing" -> [ htmlContentsFromFile; landingPostList markdownRoot ] |> String.concat "\n"
        | _ -> htmlContentsFromFile

    let metaPageTitle = Option.defaultValue markdownHeader maybeMetaPageTitle

    razorViewHandler
        markdownViewName
        (dict
            [ ("Body", box htmlContents)
              ("Header", box markdownHeader)
              ("Title", box metaPageTitle) ])

let errorRazorViewHandler errorCode body =
    razorViewHandler ErrorMarkdown (dict [ ("ErrorCode", box errorCode); ("Body", box body) ])

let headHandler: Handler = setStatusCode 200

let error404Handler: Handler =
    setStatusCode 404
    >=> publicResponseCaching 60 None
    >=> errorRazorViewHandler 404 error404Msg

let error500Handler: Handler =
    clearResponse >=> setStatusCode 500 >=> errorRazorViewHandler 500 error500Msg

let markdownRouteHandler markdownRoot markdownPath : HttpHandler =
    match MarkdownPath.toString markdownPath |> Path.GetFileName with
    | "landing.md" ->
        route "/"
        >=> markdownFileHandler
                LeftHeaderMarkdown
                markdownRoot
                markdownPath
                "Iain Schmitt"
                (Some "Iain Schmitt's Personal Website")
    | "uses.md" ->
        route "/uses"
        >=> markdownFileHandler PostMarkdown markdownRoot markdownPath "Iain Schmitt's Uses Page" None
    | "resume.md" ->
        route "/resume"
        >=> markdownFileHandler LeftHeaderMarkdown markdownRoot markdownPath "Iain Schmitt's Resume" None
    | _ ->
        route $"/post/{markdownFileName markdownPath}"
        >=> markdownFileHandler
                PostMarkdown
                markdownRoot
                markdownPath
                "Iain Schmitt"
                (maybeYamlHeader markdownPath |> Option.map _.Title)


let markdownRoutes (webRoot: String) : list<HttpHandler> =
    let markdownRoot = Path.Combine(webRoot, "markdown")

    markdownPaths markdownRoot
    |> Array.map (markdownRouteHandler markdownRoot)
    |> Array.toList

let pdfHandler webRoot pdfFileName : HttpHandler =
    let pdfPath = Path.Combine(webRoot, "pdf", pdfFileName)
    streamFile true pdfPath None None

let nonHtmlRoutes webRoot =
    [ route "/wedding/julias-game"
      >=> redirectTo true "https://connectionsgame.org/game/?661NPZ"
      route "/wedding/iains-game"
      >=> redirectTo true "https://connectionsgame.org/game/?X5SMRJ" ]

let appRoutes webRoot =
    (markdownRoutes webRoot) @ (nonHtmlRoutes webRoot)
