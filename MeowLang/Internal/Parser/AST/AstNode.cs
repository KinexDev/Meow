namespace MeowLang.Internal.Parser.AST
{
    public class AstNode
    {
        public virtual object Visit(Script context)
        {
            return null;
        }

        public override string ToString()
        {
            return $"{GetType().Name.ToLower()}";
        }
    }
}