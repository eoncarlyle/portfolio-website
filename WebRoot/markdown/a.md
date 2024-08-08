# A post about F#

## H2

Some body text

```fsharp
let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> directMarkdownHandler "landing" 
                route "/resume" >=> markdownHandler "ResumeMarkdown" "resume"
                routef "/post/%s" directMarkdownHandler
            ]
        setStatusCode 404 >=> errorHandler 404 "The page that you are looking for does not exist!" ]
```

```typescript
const a = 12
```