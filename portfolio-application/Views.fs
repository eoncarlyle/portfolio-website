module Views

open Giraffe.ViewEngine
open System
open Types

let private isPostsPath (path: string) = path = "/" || path.StartsWith "/post/"

let private navLink (isCurrent: bool) (href: string) (label: string) =
    let attrs =
        if isCurrent then
            [ _href href; attr "aria-current" "page" ]
        else
            [ _href href ]

    a attrs [ encodedText label ]

let private layout (currentPath: string) (pageTitle: string) (content: XmlNode list) =
    html
        [ _lang "en" ]
        [ head
              []
              [ meta [ _charset "UTF-8" ]
                link [ _rel "stylesheet"; _href "/css/normalise.css" ]
                link [ _rel "stylesheet"; _href "/css/index-2a6d498a.css" ]
                link [ _rel "preconnect"; _href "https://fonts.googleapis.com" ]
                link
                    [ _rel "preconnect"
                      _href "https://fonts.gstatic.com"
                      _crossorigin "anonymous" ]

                meta [ _name "viewport"; _content "width=device-width, initial-scale=1.0" ]
                link [ _rel "icon"; _type "image/x-icon"; _href "/images/icon.svg" ]
                title [] [ encodedText pageTitle ]
                meta [ _name "og:title"; _content pageTitle ]
                meta [ _name "og:image"; _content "/images/metaV1.png" ]
                meta [ _name "og:type"; _content "website" ]
                meta [ _name "author"; _content "Iain Schmitt" ] ]
          body
              []
              [ div
                    [ _class "page" ]
                    [ header
                          [ _class "sidebar" ]
                          [ a [ _href "/"; _class "brand" ] [ encodedText "Iain Schmitt" ]
                            nav
                                []
                                [ navLink (isPostsPath currentPath) "/#posts" "Posts"
                                  navLink (currentPath = "/resume") "/resume" "Resume"
                                  navLink (currentPath = "Uses") "/uses" "Uses"
                                  navLink false "/rss" "RSS"
                                  ] ]
                      main [ _class "md-content" ] content ] ] ]

let directMarkdownView (currentPath: string) (title: string) (body: string) =
    layout currentPath title [ rawText body ]

let errorView (title: string) (body: string) (errorCode: int) =
    layout
        ""
        title
        [ div
              [ _class "center" ]
              [ h1 [] [ encodedText "Error "; encodedText (string errorCode) ]
                p [] [ encodedText body ]
                a [ _href "/" ] [ encodedText "Back to home" ] ] ]

let leftHeaderMarkdownView (currentPath: string) (title: string) (header: string) (body: string) =
    layout
        currentPath
        title
        [
          if header <> "Iain Schmitt" then
              h1 [ _class "page-title" ] [ rawText header ]
          rawText body ]

let postMarkdownView (currentPath: string) (title: string) (header: string) (body: string) =
    layout
        currentPath
        title
        [ h1 [ _class "page-title" ] [ rawText header ]
          rawText body
          div [ _class "center" ] [ a [ _href "/" ] [ encodedText "Back to home" ] ] ]

let rssItem (pair: PostYamlHeaderPair) (content: string) (baseUrl: string) =
    tag
        "item"
        []
        [ tag "title" [] [ encodedText pair.Header.Title ]
          tag "link" [] [ encodedText $"{baseUrl}/post/{markdownFileName pair.Path}" ]
          tag "pubDate" [] [ encodedText (DateTime.Parse(pair.Header.Date).ToString("R")) ]
          tag "description" [] [ rawText $"<![CDATA[{content}]]>" ] ]

let rssChannelView (title: string) (link: string) (description: string) (items: XmlNode list) =
    tag
        "rss"
        [ attr "version" "2.0" ]
        [ tag
              "channel"
              []
              [ tag "title" [] [ encodedText title ]
                tag "link" [] [ encodedText link ]
                tag "description" [] [ encodedText description ]
                yield! items ] ]
