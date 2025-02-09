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
                             @"|(?<EOL>(\r?\n))" +
                             @"|(?<Comment>//.*?(?:\r?\n|$))" +
                             @"|(?<Operator>\b(and|or|not)\b|==|!=|<=|>=|\+=|-=|\*=|/=|=|>|<|[+\-*/|])" +
                             @"|(?<Keyword>\b(if|function|while|null|true|false)\b)" +
                             @"|(?<Bracket>[()])" +
                             @"|(?<Terminator>[;])" +
                             @"|(?<Punctuation>[{}.,:])" +
                             @"|(?<Identifier>[a-zA-Z_]\w*)" +
                             @"|(?<String>""[^""]*"")";

            int lineNum = 0;

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
                                tokenList.Add(new Token(
                                    TokenType.Eol, (ushort)lineNum));
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
            tokenList.Add(new Token(
                TokenType.Eol, (ushort)lineNum));

            tokens = tokenList.ToArray();
        }
    }
}