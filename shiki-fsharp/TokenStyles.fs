module ShikiFSharp.TokenStyles

open System
open Types

let getTokenStyleObject (token: ThemedToken) : (string * string) list =
    [
        match token.Color with
        | Some c when c <> "" -> yield "color", c
        | _ -> ()

        match token.BgColor with
        | Some c when c <> "" -> yield "background-color", c
        | _ -> ()

        if token.FontStyle > 0 then
            if token.FontStyle &&& int FontStyle.Italic <> 0 then
                yield "font-style", "italic"
            if token.FontStyle &&& int FontStyle.Bold <> 0 then
                yield "font-weight", "bold"
            let decorations = [
                if token.FontStyle &&& int FontStyle.Underline <> 0 then
                    yield "underline"
                if token.FontStyle &&& int FontStyle.Strikethrough <> 0 then
                    yield "line-through"
            ]
            if not decorations.IsEmpty then
                yield "text-decoration", String.concat " " decorations
    ]

let stringifyTokenStyle (styles: (string * string) list) : string =
    styles |> List.map (fun (k, v) -> $"{k}:{v}") |> String.concat ";"
