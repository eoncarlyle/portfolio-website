using Model;


public class PrismCs
{

    public static LinkedList<ITokenElement> MatchGrammar(string text, Grammar grammar)
    {
        var tokenList = new LinkedList<ITokenElement>();
        MatchGrammar(text, tokenList, grammar, 0, null);
        return tokenList;
    }

    private static void MatchGrammar(string text, LinkedList<ITokenElement> tokenList, Grammar grammar, int startPos, RematchOptions? rematch)
    {
        var tokens = grammar switch
        {
            Grammar.RestGrammar restGrammar => throw new NotImplementedException("Haven't implemented the reflection for this yet"),
            Grammar.GeneralGrammar generalGrammar => generalGrammar.Value,
            _ => throw new ArgumentException($"Invalid `Grammar` type {grammar}") // Should never throw
        };

        foreach (var tokenPair in tokens)
        {
            String token = tokenPair.Key;
            //GrammarValue patterns = tokenPair.Value;
            var patterns = tokenPair.Value switch
            {
                GrammarValue.MultipleGrammar arrayValue => arrayValue.Values,
                _ => [tokenPair.Value]
            };

            var _pat = tokenPair.Value is Single ? [tokenPair.Value] : tokenPair.Value.Values;


            if (patterns is null) continue;

            for (var j = 0; j < patterns.Length; ++j)
            {
                if (rematch is not null && rematch.GetCause().Equals(token + ',' + j.ToString()))
                {
                    return;
                }

                var patternObj = patterns[j];
                if (patternObj is null) continue;
                var inside = patternObj.Inside;
            }
        }


    }

}
