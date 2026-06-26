/// Port of packages/primitive/src/highlight/code-to-tokens-base.ts: _tokenizeWithTheme.
module ShikiFSharp.Tokenizer

open System
open TextMateSharp.Grammars
open TextMateSharp.Internal.Grammars
open Types

let private splitLines (code: string) : (string * int) array =
    let mutable offset = 0
    code.Split('\n')
    |> Array.map (fun rawLine ->
        let line   = rawLine.TrimEnd('\r')
        let result = line, offset
        offset <- offset + rawLine.Length + 1   // +1 for the consumed `\n`
        result)

let tokenizeWithTheme
    (code:     string)
    (grammar:  IGrammar)
    (colorMap: string array) : ThemedToken[][] =

    let lines  = splitLines code
    let mutable stateStack: IStateStack = StateStack.NULL
    let result  = Array.zeroCreate<ThemedToken[]> lines.Length

    for i = 0 to lines.Length - 1 do
        let line, lineOffset = lines[i]

        if line = "" then
            result[i] <- [||]
        else
            let lineResult  = grammar.TokenizeLine2(LineText(line), stateStack, TimeSpan.FromMilliseconds(500.0))
            let rawTokens   = lineResult.Tokens       // int[] of (startIndex, encodedMetadata) pairs
            let tokenCount  = rawTokens.Length / 2
            let lineTokens  = ResizeArray<ThemedToken>(tokenCount)

            for j = 0 to tokenCount - 1 do
                let startIndex     = rawTokens[2 * j]
                let nextStartIndex =
                    if j + 1 < tokenCount then rawTokens[2 * j + 2]
                    else line.Length

                if startIndex <> nextStartIndex then
                    let metadata   = rawTokens[2 * j + 1]
                    let foreground = EncodedTokenAttributes.GetForeground(metadata)
                    let fontStyle  = int (EncodedTokenAttributes.GetFontStyle(metadata))

                    // TextMateSharp returns 1-indexed foreground values, but colorMap is 0-indexed
                    let color =
                        if foreground > 0 && foreground <= colorMap.Length then
                            let c = colorMap[foreground - 1]
                            if String.IsNullOrEmpty(c) then None else Some c
                        else
                            None

                    lineTokens.Add {
                        Content   = line[startIndex .. nextStartIndex - 1]
                        Offset    = lineOffset + startIndex
                        Color     = color
                        BgColor   = None
                        FontStyle = fontStyle
                    }

            result[i]  <- lineTokens.ToArray()
            stateStack <- lineResult.RuleStack

    result
