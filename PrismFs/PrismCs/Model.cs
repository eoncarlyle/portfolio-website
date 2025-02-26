using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Model;

public class Environment
{
    public string? Selector { get; set; }
    public object? Element { get; set; }
    public string? Language { get; set; }
    public object? Grammar { get; set; }
    public string? Code { get; set; }
    public string? HighlightedCode { get; set; }
    public string? EnvironmentType { get; set; }
    public string? Content { get; set; }
    public string? Tag { get; set; }
    public string[]? Classes { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public object? Parent { get; set; }
}

public class Identifier
{
    public int Value { get; set; }
}

public static class Util
{
    public static object Encode(object tokens)
    {
        throw new NotImplementedException();
    }

    public static Identifier ObjId(object obj)
    {
        throw new NotImplementedException();
    }

    public static T Clone<T>(T o)
    {
        throw new NotImplementedException();
    }
}

public class TokenObject
{
    public Regex Pattern { get; set; } = null!;
    public bool? Lookbehind { get; set; }
    public bool? Greedy { get; set; }
    public object? Alias { get; set; } // Can be string or string[]
    public object? Inside { get; set; }
}

public abstract class SingleGrammarValue
{
    private SingleGrammarValue() { }

    public class RegexValue : SingleGrammarValue
    {
        public Regex Value { get; set; } = null!;
    }

    public class TokenValue : SingleGrammarValue
    {
        public TokenObject Value { get; set; } = null!;
    }
}

public abstract class GrammarValue
{
    private GrammarValue() { }

    public class SingleGrammar : GrammarValue
    {
        public SingleGrammarValue Value { get; set; } = null!;
    }

    public class MultipleGrammar : GrammarValue
    {
        public IReadOnlyList<SingleGrammarValue> Values { get; set; } = null!;
    }

    // Helper methods for type checking and conversion
    public bool IsSingle => this is SingleGrammar;
    public bool IsMultiple => this is MultipleGrammar;

    public SingleGrammarValue? AsSingle => this as SingleGrammar is var s ? s?.Value : null;
    public IReadOnlyList<SingleGrammarValue>? AsMultiple => this as MultipleGrammar is var m ? m?.Values : null;
}

public class GrammarRest
{
    public GrammarValue? Keyword { get; set; }
    public GrammarValue? Number { get; set; }
    public GrammarValue? GrammarFunction { get; set; }
    public GrammarValue? String { get; set; }
    public GrammarValue? Boolean { get; set; }
    public GrammarValue? Operator { get; set; }
    public GrammarValue? Punctuation { get; set; }
    public GrammarValue? Atrule { get; set; }
    public GrammarValue? Url { get; set; }
    public GrammarValue? Selector { get; set; }
    public GrammarValue? Property { get; set; }
    public GrammarValue? Important { get; set; }
    public GrammarValue? Style { get; set; }
    public GrammarValue? Comment { get; set; }
    public GrammarValue? ClassName { get; set; }
    public Grammar? Rest { get; set; }
}

public abstract class Grammar
{
    private Grammar() { }

    public class RestGrammar : Grammar
    {
        public GrammarRest Value { get; set; } = null!;
    }

    public class GeneralGrammar : Grammar
    {
        public Dictionary<string, GrammarValue> Value { get; set; } = null!;
    }
}

public interface ILanguageMapProtocol
{
    Grammar Extend(string id, Grammar grammar);
    Grammar InsertBefore(string id, string beforeId, Grammar grammar, Dictionary<string, Grammar>? insertions = null);
}

public class LanguageMapProtocol : ILanguageMapProtocol
{
    public Grammar Extend(string id, Grammar grammar)
    {
        throw new NotImplementedException();
    }

    public Grammar InsertBefore(string id, string beforeId, Grammar grammar, Dictionary<string, Grammar>? insertions = null)
    {
        throw new NotImplementedException();
    }
}

public class Languages
{
    public ILanguageMapProtocol Protocol { get; set; } = null!;
    public Dictionary<string, Grammar> Map { get; set; } = null!;
}

public delegate void HookCallback(Environment environment);

public enum HookType
{
    BeforeHighlightAll,
    BeforeSanityCheck,
    BeforeHighlight,
    BeforeInsert,
    AfterHighlight,
    Complete,
    BeforeTokenize,
    AfterTokenize,
    Wrap,
    Custom // For custom hooks
}

public class CustomHookType
{
    public string Name { get; set; } = null!;
}

public class RegisteredHooks
{
    public Dictionary<string, HookCallback[]> Hooks { get; set; } = null!;
}

public abstract class TokenStream
{
    private TokenStream() { }

    public class StringToken : TokenStream
    {
        public string Value { get; set; } = null!;
    }

    public class TokenInstance : TokenStream
    {
        public Token Value { get; set; } = null!;
    }

    public class TokenArray : TokenStream
    {
        public TokenStream[] Values { get; set; } = null!;
    }
}

public interface ITokenElement { }

public class TokenString : ITokenElement { }

public class Token : ITokenElement
{
    public string TokenType { get; set; } = null!;
    public TokenStream Content { get; set; } = null!;
    public object Alias { get; set; } = null!; // Can be string or string[]
    public int Length { get; set; }
    public bool Greedy { get; set; }
}

public class RematchOptions
{
    private string cause;
    private int reach;
    public RematchOptions(string cause, int reach)
    {
        this.cause = cause;
        this.reach = reach;
    }
    public string GetCause() { return cause; }
    public int GetReach() { return reach; }

}
