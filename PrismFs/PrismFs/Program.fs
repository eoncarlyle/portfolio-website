open System
open Microsoft.FSharp.Core
open PrismFs.Model
open System.Collections.Generic

type _Token =
    | StringToken of string
    | Token of Token

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

let matchGrammar (text: String) (grammar: Grammar) = 
     let tokenList = LinkedList<_Token>()
     
     // Starting off in a kinda imperative way here makes the most sense
     let loop (startNode: LinkedListNode<_Token>) (startPos: int) (rematch: bool) =
         
         
         []
         
     loop tokenList.First 0 false


let tokenize (text: String) (grammar: Grammar) : TokenStream array =
    
    
    // Leaving out the 'rest' grammar for now, that can be implemented in other ways
    
    // Add after I don't think is neccesary in direct tokenisation
    let tokenList = [| text |]
    
       
    [|  |]
    
