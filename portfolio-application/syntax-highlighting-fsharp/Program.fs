module SyntaxHighlighting

open System
open System.IO
open System.Text.RegularExpressions
open ShikiFSharp.Highlighter

let private langPattern = Regex(@"```(\w+)\n", RegexOptions.Compiled)

let highlightFileAndMigrate (highlighter: Highlighter) (inputFile: string) (outputFile: string) =
    let content = File.ReadAllText(inputFile)

    let backticks = ResizeArray<int>()
    let mutable pos = 0
    let mutable cont = true
    while cont do
        let idx = content.IndexOf("```", pos)
        if idx <> -1 then
            backticks.Add(idx)
            pos <- idx + 1
        else
            cont <- false

    if backticks.Count % 2 <> 0 then
        failwithf "[Syntax Highlighting]: Uneven number of backticks in %s" inputFile

    let replacements = ResizeArray<string * string>()

    for i = 0 to backticks.Count / 2 - 1 do
        let openIdx  = backticks[2 * i]
        let closeIdx = backticks[2 * i + 1]

        // Slice from opening ``` up to (not including) the closing ```.
        let sliceIncludingLang = content[openIdx .. closeIdx - 1]

        let firstNewline = sliceIncludingLang.IndexOf('\n')
        if firstNewline = -1 then
            failwithf "[Syntax Highlighting]: Newline not found in code block in %s" inputFile

        let m = langPattern.Match(sliceIncludingLang)
        if not m.Success then
            failwithf "[Syntax Highlighting]: Could not parse language from code block in %s" inputFile
        let language = m.Groups[1].Value

        let code = sliceIncludingLang[firstNewline + 1 ..]

        let defaultHtml = highlighter.CodeToHtml(code, language)
        let adjustedHtml =
            defaultHtml.Replace(
                "background-color:#1e1e2e;color:#cdd6f4",
                "background-color:#181825;color:#cdd6f4;padding:1em;border-radius:0.3em;overflow:auto")

        // Full pattern to replace: opening ``` through end of closing ```.
        let pattern = content[openIdx .. closeIdx + 2]
        replacements.Add(pattern, adjustedHtml)

    let mutable result = content
    for pattern, replacement in replacements do
        result <- result.Replace(pattern, replacement)

    File.WriteAllText(outputFile, result)

[<EntryPoint>]
let main argv =
    if argv.Length <> 2 then
        eprintfn "[Syntax Highlighting]: Expected 2 arguments: <postsDirectory> <markdownDirectory>, got %d" argv.Length
        1
    else
        let postsDirectory    = argv[0]
        let markdownDirectory = argv[1]
        printfn "[Syntax Highlighting]: postsDirectory %s" postsDirectory
        printfn "[Syntax Highlighting]: markdownDirectory %s" markdownDirectory

        let themePath    = Path.Combine(AppContext.BaseDirectory, "themes", "catppuccin-mocha.json")
        let grammarsDir  = Path.Combine(AppContext.BaseDirectory, "grammars")
        let highlighter  = Highlighter(themePath, grammarsDir)

        for yearDir in Directory.GetDirectories(postsDirectory) do
            let yearName     = Path.GetFileName(yearDir)
            let outputYearDir = Path.Combine(markdownDirectory, yearName)
            Directory.CreateDirectory(outputYearDir) |> ignore

            for file in Directory.GetFiles(yearDir) do
                let filename   = Path.GetFileName(file)
                let outputFile = Path.Combine(outputYearDir, filename)
                printfn "[Syntax Highlighting]: Processing %s" filename
                highlightFileAndMigrate highlighter file outputFile

        0
