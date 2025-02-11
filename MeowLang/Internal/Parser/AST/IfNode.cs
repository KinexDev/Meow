namespace MeowLang.Internal.Parser.AST;

public class IfNode : AstNode
{
    public AstNode condition { get; set; }
    public List<AstNode> Statements { get; set; } = new();
    public ElseNode elseNode { get; set; }
    public IfNode(AstNode condition)
    {
        this.condition = condition;
    }

    public override object Visit(Script context)
    {
        var conditionResult = condition.Visit(context);
        if (conditionResult is bool met)
        {
            if (met)
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
            else
            {
                if (elseNode == null)
                    return null;
                
                var result = elseNode.Visit(context);

                if (result is ReturnNode returnNode)
                {
                    return returnNode;
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