module ShikiFSharp.HtmlRenderer

open System
open System.Text
open System.Text.RegularExpressions
open Types
open TokenStyles

let private reWhitespaceOnly = Regex(@"^\s+$", RegexOptions.Compiled)

let private mergeWhitespaceTokens (tokens: ThemedToken[][]) : ThemedToken[][] =
    tokens
    |> Array.map (fun line ->
        let newLine = ResizeArray<ThemedToken>(line.Length)
        let mutable carry = ""
        let mutable firstOff = ValueNone

        for idx = 0 to line.Length - 1 do
            let token = line[idx]

            let isDecorated =
                token.FontStyle &&& int FontStyle.Underline <> 0
                || token.FontStyle &&& int FontStyle.Strikethrough <> 0

            let couldMerge = not isDecorated

            if couldMerge && reWhitespaceOnly.IsMatch(token.Content) && idx + 1 < line.Length then
                if firstOff.IsNone then
                    firstOff <- ValueSome token.Offset

                carry <- carry + token.Content
            else if carry <> "" then
                if couldMerge then
                    newLine.Add
                        { token with
                            Offset = firstOff.Value
                            Content = carry + token.Content }
                else
                    newLine.Add
                        { Content = carry
                          Offset = firstOff.Value
                          Color = None
                          BgColor = None
                          FontStyle = 0 }

                    newLine.Add token

                firstOff <- ValueNone
                carry <- ""
            else
                newLine.Add token

        newLine.ToArray())

let private htmlEscape (s: string) =
    s
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;")

let tokensToHtml (tokens: ThemedToken[][]) (fg: string) (bg: string) (themeName: string) : string =

    let tokens = mergeWhitespaceTokens tokens
    let sb = StringBuilder()

    sb.Append($"""<pre class="shiki {themeName}" style="background-color:{bg};color:{fg}" tabindex="0"><code>""")
    |> ignore

    for lineIdx = 0 to tokens.Length - 1 do
        if lineIdx > 0 then
            sb.Append('\n') |> ignore

        sb.Append("""<span class="line">""") |> ignore

        for token in tokens[lineIdx] do
            let style = token |> getTokenStyleObject |> stringifyTokenStyle

            if style <> "" then
                sb.Append($"""<span style="{style}">{htmlEscape token.Content}</span>""")
                |> ignore
            else
                sb.Append($"<span>{htmlEscape token.Content}</span>") |> ignore

        sb.Append("</span>") |> ignore

    sb.Append("</code></pre>") |> ignore
    sb.ToString()
