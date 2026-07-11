module ShikiMarkdig

open System
open System.IO
open Markdig
open Markdig.Renderers
open Markdig.Renderers.Html
open Markdig.Syntax
open ShikiFSharp.Highlighter

let private styleOverride (html: string) =
    html.Replace(
        "background-color:#1e1e2e;color:#cdd6f4",
        "background-color:#181825;color:#cdd6f4;padding:1em;border-radius:0.3em;overflow:auto")

type ShikiCodeBlockRenderer(highlighter: Highlighter) =
    inherit CodeBlockRenderer()
    override this.Write(renderer: HtmlRenderer, block: CodeBlock) =
        match block with
        | :? FencedCodeBlock as fenced ->
            let lang = if String.IsNullOrWhiteSpace(fenced.Info) then "" else fenced.Info
            highlighter.CodeToHtml(fenced.Lines.ToString(), lang)
            |> styleOverride
            |> renderer.Write
            |> ignore
        | _ -> base.Write(renderer, block)

type ShikiExtension(highlighter: Highlighter) =
    interface IMarkdownExtension with
        member _.Setup(_: MarkdownPipelineBuilder) = ()
        member _.Setup(_: MarkdownPipeline, renderer: IMarkdownRenderer) =
            match renderer with
            | :? HtmlRenderer as htmlRenderer ->
                htmlRenderer.ObjectRenderers.ReplaceOrAdd<CodeBlockRenderer>(ShikiCodeBlockRenderer(highlighter)) |> ignore
            | _ -> ()

let shikiHighlighter =
    Highlighter(
        Path.Combine(AppContext.BaseDirectory, "themes", "catppuccin-mocha.json"),
        Path.Combine(AppContext.BaseDirectory, "grammars") |> Some)
