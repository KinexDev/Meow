namespace MeowLang.Internal.Tokenizer
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public static class Tokenizer
    {
        // i'll switch to a manual tokenizer soon, but regex is good enough for testing.
        public static void FindTokens(string code, out Token[] tokens)
        {
            List<Token> tokenList = new List<Token>();

            // i used chatgpt to make the regex for this lol
            string pattern = @"(?<Number>\d+(\.\d+)?)" +
                             @"|(?<Eol>(\n))" +
                             @"|(?<Comment>//.*?(?:\r?\n|$))" +
                             @"|(?<Operator>\b(and|or|not)\b|==|!=|<=|>=|\+=|-=|\*=|/=|=|>|<|[+\-*/|])" +
                             @"|(?<Keyword>\b(if|else|function|while|null|return|true|false)\b)" +
                             @"|(?<Bracket>[()])" +
                             @"|(?<Terminator>[;])" +
                             @"|(?<Punctuation>[{}.,:])" +
                             @"|(?<Identifier>[a-zA-Z_]\w*)" +
                             @"|(?<String>""[^""]*"")";

            int lineNum = 1;

            foreach (Match match in Regex.Matches(code, pattern, RegexOptions.Singleline))
            {
                foreach (var tokenName in Enum.GetNames(typeof(TokenType)))
                {
                    if (tokenName == "Comment") continue;

                    if (match.Groups[tokenName].Success)
                    {
                        if (Enum.TryParse(tokenName, out TokenType tokenType))
                        {
                            if (tokenType == TokenType.Eol)
                            {
                                lineNum++;
                                break;
                            }

                            if (tokenType == TokenType.String)
                            {
                                tokenList.Add(new Token(
                                    tokenType,
                                    match.Value.Substring(1, match.Value.Length - 2),
                                    (ushort)lineNum));
                                break;
                            }

                            tokenList.Add(new Token(
                                tokenType,
                                match.Value,
                                (ushort)lineNum));
                        }

                        break;
                    }
                }
            }

            lineNum++;
            tokens = tokenList.ToArray();
        }
    }
}