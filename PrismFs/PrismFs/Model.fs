module PrismFs.Model
open System.Text.RegularExpressions 

type Environment = {
    selector: string option
    element: obj option
    language: string option
    grammar: obj option
    code: string option
    highlightedCode: string option
    environmentType: string option
    content: string option
    tag: string option
    classes: string array option
    attributes: Map<string, string> option
    parent: obj option
}

type Identifier = {
    value: int
}

module Util =
    let encode (tokens: obj) : obj = failwith "Not implemented"
    let objId (obj: obj) : Identifier = failwith "Not implemented"
    let clone<'T> (o: 'T) : 'T = failwith "Not implemented"

type TokenObject = {
    pattern: Regex
    lookbehind: bool option
    greedy: bool option
    alias: Choice<string, string array> option
    inside: obj option
}

type GrammarValue = 
    | RegexValue of Regex
    | TokenValue of TokenObject
    | ArrayValue of GrammarValue array

type GrammarRest = {
    keyword: GrammarValue option
    number: GrammarValue option
    grammarFunction: GrammarValue option
    string: GrammarValue option
    boolean: GrammarValue option
    operator: GrammarValue option
    punctuation: GrammarValue option
    atrule: GrammarValue option
    url: GrammarValue option
    selector: GrammarValue option
    property: GrammarValue option
    important: GrammarValue option
    style: GrammarValue option
    comment: GrammarValue option
    className: GrammarValue option
    rest: Grammar option
}
and Grammar = 
    | RestGrammar of GrammarRest
    | GeneralGrammar of Map<string, GrammarValue>

type LanguageMapProtocol = {
    extend: string -> Grammar -> Grammar
    insertBefore: string -> string -> Grammar -> Map<string, Grammar> option -> Grammar
}

type LanguageMap = Map<string, Grammar>

type Languages = {
    protocol: LanguageMapProtocol
    map: LanguageMap
}

type HookCallback = Environment -> unit

type HookType =
    | BeforeHighlightAll
    | BeforeSanityCheck
    | BeforeHighlight
    | BeforeInsert
    | AfterHighlight
    | Complete
    | BeforeTokenize
    | AfterTokenize
    | Wrap
    | Custom of string

type RegisteredHooks = Map<string, HookCallback array>

type TokenStream =
    | StringToken of string
    | Token of Token
    | TokenArray of TokenStream array

and Token = {
    tokenType: string
    content: TokenStream
    alias: Choice<string, string array>
    length: int
    greedy: bool
}
