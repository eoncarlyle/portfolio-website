# shiki-fsharp

An F# port of the `codeToHtml` codepath from [Shiki](https://shiki.style), using [TextMateSharp](https://github.com/danipen/TextMateSharp) as the .NET TextMate grammar engine.

## How it maps to the JS codepaths

| Shiki | This library |
|---|---|
| `codeToHtml(code, {lang, theme})` | `Highlighter.CodeToHtml` / `codeToHtml` |
| `codeToTokens` → `codeToTokensBase` → `_tokenizeWithTheme` | `Tokenizer.tokenizeWithTheme` |
| `grammar.tokenizeLine2()` via vscode-textmate | `IGrammar.TokenizeLine2(LineText)` via TextMateSharp |
| `EncodedTokenMetadata.getForeground/getFontStyle` | `EncodedTokenAttributes.GetForeground/GetFontStyle` |
| `tokensToHast` + `hastToHtml` | `HtmlRenderer.tokensToHtml` (direct, no HAST intermediate) |
| `mergeWhitespaceTokens` | `HtmlRenderer.mergeWhitespaceTokens` |
| `getTokenStyleObject` + `stringifyTokenStyle` | `TokenStyles.fs` |

The HAST layer is skipped because there are no transformers. `codeToHtml` is the only Shiki call that I used, so this port is built around it.

## File structure

| File | Mirrors |
|---|---|
| `Types.fs` | `@shikijs/types`: `ThemedToken`, `TokensResult`, `FontStyle` |
| `TokenStyles.fs` | `packages/core/src/utils/tokens.ts` |
| `Tokenizer.fs` | `packages/primitive/src/highlight/code-to-tokens-base.ts` |
| `HtmlRenderer.fs` | `packages/core/src/highlight/code-to-hast.ts` + `code-to-html.ts` |
| `Highlighter.fs` | `packages/core/src/constructors/highlighter.ts` + `bundle-factory.ts` |
| `themes/catppuccin-mocha.json` | `@shikijs/themes` catppuccin-mocha
