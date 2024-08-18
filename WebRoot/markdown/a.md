# Heading

Test file

```fsharp
let webApp =
    choose
        [ GET
          >=> choose
                  [ route "/" >=> directMarkdownHandler "landing"
                    route "/resume" >=> markdownHandler "ResumeMarkdown" "resume"
                    routef "/post/%s" directMarkdownHandler ]
          setStatusCode 404
          >=> publicResponseCaching 60 None
          >=> errorHandler 404 "The page that you are looking for does not exist!" ]
```
