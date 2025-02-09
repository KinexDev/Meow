namespace MeowLang.Internal.Parser.AST;

public class ReturnNode : AstNode
{
    public AstNode ReturnValue { get; set; }

    public override object Visit(Script context)
    {
        return this;
    }
}