namespace MeowLang.Internal.Tokenizer
{
    public enum TokenType
    {
        Number,
        Comment,
        Operator,
        Bracket,
        Terminator,
        Punctuation,
        Identifier,
        String,
        Keyword,
        Eol
    }

    public class Token
    {
        public TokenType TokenType { get; }
        public string Value { get; }
        public ushort Line { get; set; }

        public Token(TokenType tokenType, string value, ushort line)
        {
            this.TokenType = tokenType;
            this.Value = value;
            this.Line = line;
        }

        public Token(TokenType tokenType, ushort line)
        {
            this.TokenType = tokenType;
            this.Line = line;
        }
    }
}
