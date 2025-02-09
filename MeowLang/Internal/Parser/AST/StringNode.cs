namespace MeowLang.Internal.Parser.AST
{
    public class StringNode : AstNode
    {
        public string String { get; set; }

        public StringNode(string str)
        {
            String = str;
        }

        public override object Visit(Script context)
        {
            return String;
        }
    }
}