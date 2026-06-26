module ShikiFSharp.Highlighter

//#nowarn "3261"   // Newtonsoft.Json predates nullable-reference annotations

open System
open System.IO
open System.Text
open Newtonsoft.Json.Linq
open System.Collections.Generic
open TextMateSharp.Grammars
open TextMateSharp.Internal.Grammars.Reader
open TextMateSharp.Internal.Themes.Reader
open TextMateSharp.Registry
open Types

let private notEmpty (arg: String) = String.IsNullOrEmpty arg |> not

let private normalizeThemeJson (json: string) : string =
    let obj = JObject.Parse(json)
    // VS Code themes use "tokenColors", TextMateSharp expects "settings"
    if not (isNull (obj["tokenColors"])) && isNull (obj["settings"]) then
        obj["settings"] <- obj["tokenColors"]
        obj.Remove("tokenColors") |> ignore

    obj.ToString()

let private loadThemeColors (json: string) : string * string * string =
    let obj = JObject.Parse(json)
    let maybeColors = Option.ofObj obj["colors"]

    let strOrDefault (maybeToken: JToken option) (fallback: string) : string =
        maybeToken
        |> Option.bind (fun t -> t.Value<string>() |> Option.ofObj)
        |> Option.filter notEmpty
        |> Option.defaultValue fallback

    let fg, bg =
        match maybeColors with
        | Some colors ->
            strOrDefault (Option.ofObj colors["editor.foreground"]) "#ffffff",
            strOrDefault (Option.ofObj colors["editor.background"]) "#000000"
        | _ -> "#ffffff", "#000000"

    let name = strOrDefault (Option.ofObj obj["name"]) "unknown"
    fg, bg, name

let private resolveScope (registryOptions: RegistryOptions) (lang: string) : string option =
    match lang.ToLowerInvariant() with
    | "text"
    | "plain"
    | "txt"
    | "" -> None
    | "fsharp"
    | "fs"
    | "f#" -> Some "source.fsharp"
    | "csharp"
    | "cs"
    | "c#" -> Some "source.cs"
    | "javascript"
    | "js" -> Some "source.js"
    | "typescript"
    | "ts" -> Some "source.ts"
    | "tsx" -> Some "source.tsx"
    | "jsx" -> Some "source.jsx"
    | "python"
    | "py" -> Some "source.python"
    | "rust"
    | "rs" -> Some "source.rust"
    | "go" -> Some "source.go"
    | "html" -> Some "text.html.basic"
    | "css" -> Some "source.css"
    | "json" -> Some "source.json"
    | "yaml"
    | "yml" -> Some "source.yaml"
    | "toml" -> Some "source.toml"
    | "bash"
    | "sh"
    | "shell" -> Some "source.shell"
    | "sql" -> Some "source.sql"
    | "kotlin"
    | "kt" -> Some "source.kotlin"
    | "java" -> Some "source.java"
    | "xml" -> Some "text.xml"
    | "markdown"
    | "md" -> Some "text.html.markdown"
    | "c" -> Some "source.c"
    | "cpp"
    | "c++" -> Some "source.cpp"
    | "lua" -> Some "source.lua"
    | "ruby"
    | "rb" -> Some "source.ruby"
    | "swift" -> Some "source.swift"
    | "scala" -> Some "source.scala"
    | "haskell"
    | "hs" -> Some "source.haskell"
    | "ocaml" -> Some "source.ocaml"
    | "elixir"
    | "ex"
    | "exs" -> Some "source.elixir"
    | "erlang"
    | "erl" -> Some "source.erlang"
    | "clojure"
    | "clj" -> Some "source.clojure"
    | "r" -> Some "source.r"
    | "dart" -> Some "source.dart"
    | "php" -> Some "source.php"
    | "perl"
    | "pl" -> Some "source.perl"
    | "powershell"
    | "ps1" -> Some "source.powershell"
    | "dockerfile" -> Some "source.dockerfile"
    | "diff" -> Some "source.diff"
    | other ->
        let scope = registryOptions.GetScopeByLanguageId(other)
        if String.IsNullOrEmpty(scope) then None else Some scope

let private getGrammarsDir (maybeGrammarsDir: string option) =
    maybeGrammarsDir
    |> Option.filter Directory.Exists
    |> Option.map (fun dir ->
        Directory.GetFiles(dir, "*.json")
        |> Array.choose (fun file ->
            let obj = JObject.Parse(File.ReadAllText(file))

            obj["scopeName"]
            |> Option.ofObj
            |> Option.bind (fun t -> t.Value<string>() |> Option.ofObj)
            |> Option.filter notEmpty
            |> Option.map (fun scopeName -> scopeName, file))
        |> dict
        |> Dictionary)
    |> Option.defaultWith Dictionary

let private getRegistry
    (extras: Dictionary<String, string>)
    (normalizedJson: String)
    (registryOptions: IRegistryOptions)
    =
    let registry = Registry(registryOptions)
    use ms = new MemoryStream(Encoding.UTF8.GetBytes(normalizedJson))
    use reader = new StreamReader(ms)
    let rawTheme = ThemeReader.ReadThemeSync(reader)
    registry.SetTheme(rawTheme)
    // Pre-load extra grammars before any CodeToHtml call so they take precedence
    // over any bundled grammar with the same scope (Registry caches the first one it sees).
    for kvp in extras do
        try
            registry.LoadGrammarFromPathSync(kvp.Value, 0, Dictionary<string, int>())
            |> ignore
        with _ ->
            ()

    registry

type private ExtendedRegistryOptions(registryOptions: RegistryOptions, extras: Dictionary<string, string>) =
    interface IRegistryOptions with
        member _.GetDefaultTheme() =
            (registryOptions :> IRegistryOptions).GetDefaultTheme()

        member _.GetTheme(s) =
            (registryOptions :> IRegistryOptions).GetTheme(s)

        member _.GetInjections(s) =
            (registryOptions :> IRegistryOptions).GetInjections(s)

        member _.GetGrammar(scopeName) =
            match extras.TryGetValue(scopeName) with
            | true, filePath ->
                use ms = new MemoryStream(File.ReadAllBytes(filePath))
                use reader = new StreamReader(ms)
                GrammarReader.ReadGrammarSync(reader)
            | _ -> (registryOptions :> IRegistryOptions).GetGrammar(scopeName)

type Highlighter(themePath: string, maybeGrammarsDir: string option) =

    let themeJson = File.ReadAllText(themePath)
    let fg, bg, themeName = loadThemeColors themeJson
    let normalizedJson = normalizeThemeJson themeJson
    let baseOptions = RegistryOptions(ThemeName.DarkPlus)
    let extras = getGrammarsDir maybeGrammarsDir

    let registry =
        ExtendedRegistryOptions(baseOptions, extras)
        |> getRegistry extras normalizedJson

    let colorMap = registry.GetColorMap() |> Seq.toArray

    let plainTokens (code: string) : ThemedToken[][] =
        code.Split('\n')
        |> Array.map (fun rawLine ->
            let line = rawLine.TrimEnd('\r')

            [| { Content = line
                 Offset = 0
                 Color = None
                 BgColor = None
                 FontStyle = 0 } |])
        
    member _.CodeToHtml(code: string, lang: string) : string =
        let render' = HtmlRenderer.tokensToHtml fg bg themeName
        match resolveScope baseOptions lang with
        | None -> plainTokens code |> render'
        | Some scope ->
            let maybeGrammar = registry.LoadGrammar(scope)
                            |> Option.ofObj
            
            match maybeGrammar with
            | None ->
                eprintfn
                    $"[shiki-fsharp] warning: no grammar for lang='{lang}' (scope='{scope}'), rendering as plain text"

                plainTokens code |> render'
            | Some grammar ->
                Tokenizer.tokenizeWithTheme code grammar colorMap |> render'

let codeToHtml (code: string) (lang: string) (themePath: string) : string =
    Highlighter(themePath, None).CodeToHtml(code, lang)
