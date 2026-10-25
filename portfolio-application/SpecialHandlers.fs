module SpecialHandlers

open Giraffe
open System.IO
open Giraffe.ViewEngine

open StandardHandlers
open Types
open Yaml


let error404Msg = "The page that you are looking for does not exist!"
let error500Msg = "Internal server error"

let errorViewHandler errorCode body : HttpHandler =
    viewHandler
        ErrorMarkdown
        { PageTitle = "Error Page"
          Body = body
          IsCached = true
          ErrorCode = Some errorCode }

let headMethodHandler: Handler = setStatusCode 200

let error404Handler: Handler =
    setStatusCode 404 >=> errorViewHandler 404 error404Msg

let error500Handler: Handler =
    clearResponse >=> setStatusCode 500 >=> errorViewHandler 500 error500Msg

let nonHtmlRoutes baseDirectory =
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
