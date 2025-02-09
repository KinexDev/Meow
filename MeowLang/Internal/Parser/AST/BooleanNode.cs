namespace MeowLang.Internal.Parser.AST;

public class BooleanNode : AstNode
{
    public bool Boolean { get; set; }

    public BooleanNode(bool boolean)
    {
        Boolean = boolean;
    }

    public override object Visit(Script context)
    {
        return Boolean;
    }
}