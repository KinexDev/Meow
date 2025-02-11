namespace MeowLang.Internal.Parser.AST
{
    public class WhileNode : AstNode
    {
        public AstNode condition { get; set; }
        public List<AstNode> Statements { get; set; } = new();

        public WhileNode(AstNode condition)
        {
            this.condition = condition;
        }

        public override object Visit(Script context)
        {
            var conditionResult = condition.Visit(context);
            if (conditionResult is bool)
            {
                while ((bool)condition.Visit(context))
                {
                    foreach (var statement in Statements)
                    {
                        var result = statement.Visit(context);

                        if (result is ReturnNode returnNode)
                        {
                            return returnNode;
                        }
                    }
                }

                return null;
            }
            else
            {
                throw new InvalidCastException("condition is not a bool");
            }
        }
    }
}