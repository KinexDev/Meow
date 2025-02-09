namespace MeowLang.Internal.Parser.AST;

public class NumberNode : AstNode
{
    public float Literal { get; set; }

    public NumberNode(float literal)
    {
        Literal = literal;
    }

    public override object Visit(Script context)
    {
        return Literal;
    }
}