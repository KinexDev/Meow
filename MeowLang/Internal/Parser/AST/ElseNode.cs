namespace MeowLang.Internal.Parser.AST
{
    public class ElseNode : AstNode
    {
        public List<AstNode> Statements { get; set; } = new();

        public override object Visit(Script context)
        {
            foreach (var statement in Statements)
            {
                var result = statement.Visit(context);

                if (result is ReturnNode returnNode)
                {
                    return returnNode;
                }
            }

            return null;
        }

        public virtual object Evaluate(Script context)
        {
            foreach (var statement in Statements)
            {
                var result = statement.Visit(context);

                if (result is ReturnNode returnNode)
                {
                    return returnNode;
                }
            }

            return null;
        }
    }
}