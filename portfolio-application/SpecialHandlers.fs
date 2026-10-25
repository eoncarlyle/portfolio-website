module SpecialHandlers

open Giraffe

open StandardHandlers
open Types

let error404Msg = "The page that you are looking for does not exist!"
let error500Msg = "Internal server error"

let errorViewHandler errorCode body: HttpHandler =
    viewHandler
        ErrorMarkdown
        { PageTitle = "Error Page"
          Header = "Error Page"
          Body = body
          IsCached = true
          ErrorCode = Some errorCode }

let headHandler: Handler = setStatusCode 200

let error404Handler: Handler =
    setStatusCode 404 >=> errorViewHandler 404 error404Msg

let error500Handler: Handler =
    clearResponse >=> setStatusCode 500 >=> errorViewHandler 500 error500Msg
