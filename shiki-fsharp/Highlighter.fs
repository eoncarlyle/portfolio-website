#nowarn "3261"   // Newtonsoft.Json predates nullable-reference annotations
module ShikiFSharp.Highlighter

open System
open System.IO
open System.Text
open Newtonsoft.Json.Linq
open System.Collections.Generic
open TextMateSharp.Grammars
open TextMateSharp.Internal.Grammars.Reader
open TextMateSharp.Internal.Themes.Reader
open TextMateSharp.Internal.Types
open TextMateSharp.Registry
open Types

let private normalizeThemeJson (json: string) : string =
    let obj = JObject.Parse(json)
    // VS Code themes use "tokenColors", TextMateSharp expects "settings"
    if not (isNull (obj.["tokenColors"])) && isNull (obj.["settings"]) then
        obj.["settings"] <- obj.["tokenColors"]
        obj.Remove("tokenColors") |> ignore
    obj.ToString()

let private loadThemeColors (json: string) : string * string * string =
    let obj    = JObject.Parse(json)
    let colors = obj.["colors"]

    let strOrDefault (token: JToken) (fallback: string) =
        if isNull token then fallback
        else
            let v = token.Value<string>()
            if String.IsNullOrEmpty(v) then fallback else v

    let fg, bg =
        if not (isNull colors) then
            strOrDefault colors.["editor.foreground"] "#ffffff",
            strOrDefault colors.["editor.background"] "#000000"
        else
            "#ffffff", "#000000"

    let name = strOrDefault obj.["name"] "unknown"
    fg, bg, name

let private resolveScope (registryOptions: RegistryOptions) (lang: string) : string option =
    match lang.ToLowerInvariant() with
    | "text" | "plain" | "txt" | "" -> None
    | "fsharp"     | "fs"  | "f#"   -> Some "source.fsharp"
    | "csharp"     | "cs"  | "c#"   -> Some "source.cs"
    | "javascript" | "js"            -> Some "source.js"
    | "typescript" | "ts"            -> Some "source.ts"
    | "tsx"                          -> Some "source.tsx"
    | "jsx"                          -> Some "source.jsx"
    | "python"     | "py"            -> Some "source.python"
    | "rust"       | "rs"            -> Some "source.rust"
    | "go"                           -> Some "source.go"
    | "html"                         -> Some "text.html.basic"
    | "css"                          -> Some "source.css"
    | "json"                         -> Some "source.json"
    | "yaml"       | "yml"           -> Some "source.yaml"
    | "toml"                         -> Some "source.toml"
    | "bash" | "sh" | "shell"        -> Some "source.shell"
    | "sql"                          -> Some "source.sql"
    | "kotlin"     | "kt"            -> Some "source.kotlin"
    | "java"                         -> Some "source.java"
    | "xml"                          -> Some "text.xml"
    | "markdown"   | "md"            -> Some "text.html.markdown"
    | "c"                            -> Some "source.c"
    | "cpp"        | "c++"           -> Some "source.cpp"
    | "lua"                          -> Some "source.lua"
    | "ruby"       | "rb"            -> Some "source.ruby"
    | "swift"                        -> Some "source.swift"
    | "scala"                        -> Some "source.scala"
    | "haskell"    | "hs"            -> Some "source.haskell"
    | "ocaml"                        -> Some "source.ocaml"
    | "elixir"     | "ex" | "exs"   -> Some "source.elixir"
    | "erlang"     | "erl"           -> Some "source.erlang"
    | "clojure"    | "clj"           -> Some "source.clojure"
    | "r"                            -> Some "source.r"
    | "dart"                         -> Some "source.dart"
    | "php"                          -> Some "source.php"
    | "perl"       | "pl"            -> Some "source.perl"
    | "powershell" | "ps1"           -> Some "source.powershell"
    | "dockerfile"                   -> Some "source.dockerfile"
    | "diff"                         -> Some "source.diff"
    | other ->
        let scope = registryOptions.GetScopeByLanguageId(other)
        if String.IsNullOrEmpty(scope) then None else Some scope

type private ExtendedRegistryOptions(base': RegistryOptions, extras: Dictionary<string, string>) =
    interface IRegistryOptions with
        member _.GetDefaultTheme() = (base' :> IRegistryOptions).GetDefaultTheme()
        member _.GetTheme(s)       = (base' :> IRegistryOptions).GetTheme(s)
        member _.GetInjections(s)  = (base' :> IRegistryOptions).GetInjections(s)
        member _.GetGrammar(scopeName) =
            match extras.TryGetValue(scopeName) with
            | true, filePath ->
                use ms     = new MemoryStream(File.ReadAllBytes(filePath))
                use reader = new StreamReader(ms)
                GrammarReader.ReadGrammarSync(reader)
            | _ ->
                (base' :> IRegistryOptions).GetGrammar(scopeName)

type Highlighter(themePath: string, ?grammarsDir: string) =

    let themeJson         = File.ReadAllText(themePath)
    let fg, bg, themeName = loadThemeColors themeJson
    let normalizedJson    = normalizeThemeJson themeJson

    let baseOptions = RegistryOptions(ThemeName.DarkPlus)

    let extras = Dictionary<string, string>()
    do
        match grammarsDir with
        | Some dir when Directory.Exists(dir) ->
            for file in Directory.GetFiles(dir, "*.json") do
                let obj       = JObject.Parse(File.ReadAllText(file))
                let scopeName = obj.["scopeName"] |> Option.ofObj |> Option.map (fun t -> t.Value<string>()) |> Option.defaultValue ""
                if scopeName <> "" then
                    extras[scopeName] <- file
        | _ -> ()

    let registryOptions : IRegistryOptions = ExtendedRegistryOptions(baseOptions, extras)
    let registry = Registry(registryOptions)

    do
        use ms     = new MemoryStream(Encoding.UTF8.GetBytes(normalizedJson))
        use reader = new StreamReader(ms)
        let rawTheme = ThemeReader.ReadThemeSync(reader)
        registry.SetTheme(rawTheme)

    // Pre-load extra grammars before any CodeToHtml call so they take precedence
    // over any bundled grammar with the same scope (Registry caches the first one it sees).
    do
        for kvp in extras do
            try registry.LoadGrammarFromPathSync(kvp.Value, 0, Dictionary<string, int>()) |> ignore
            with _ -> ()

    let colorMap = registry.GetColorMap() |> Seq.toArray

    let plainTokens (code: string) : ThemedToken[][] =
        code.Split('\n')
        |> Array.map (fun rawLine ->
            let line = rawLine.TrimEnd('\r')
            [| { Content = line; Offset = 0; Color = None; BgColor = None; FontStyle = 0 } |])

    member _.CodeToHtml(code: string, lang: string) : string =
        match resolveScope baseOptions lang with
        | None ->
            HtmlRenderer.tokensToHtml (plainTokens code) fg bg themeName

        | Some scope ->
            let grammar = registry.LoadGrammar(scope)
            if isNull grammar then
                eprintfn $"[shiki-fsharp] warning: no grammar for lang='{lang}' (scope='{scope}'), rendering as plain text"
                HtmlRenderer.tokensToHtml (plainTokens code) fg bg themeName
            else
                let tokens = Tokenizer.tokenizeWithTheme code grammar colorMap
                HtmlRenderer.tokensToHtml tokens fg bg themeName

let codeToHtml (code: string) (lang: string) (themePath: string) : string =
    Highlighter(themePath).CodeToHtml(code, lang)
