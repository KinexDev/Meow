namespace MeowLang.Internal.Parser.AST;

public class AstNode
{
    public virtual object Visit(Script context)
    {
        return new NullNode();
    }
}