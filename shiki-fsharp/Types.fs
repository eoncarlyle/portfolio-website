module ShikiFSharp.Types

open System

[<Flags>]
type FontStyle =
    | NotSet = -1
    | None = 0
    | Italic = 1
    | Bold = 2
    | Underline = 4
    | Strikethrough = 8

type ThemedToken =
    { Content: string
      Offset: int
      Color: string option
      BgColor: string option
      FontStyle: int }

type TokensResult =
    { Tokens: ThemedToken[][]
      Fg: string
      Bg: string
      ThemeName: string }
