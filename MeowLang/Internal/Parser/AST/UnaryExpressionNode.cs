using System;

namespace MeowLang.Internal.Parser.AST
{
    public class UnaryExpressionNode : AstNode
    {
        public string Expression { get; set; }
        public AstNode Operand { get; set; }

        public UnaryExpressionNode(string expression, AstNode operand = null)
        {
            Expression = expression;
            Operand = operand;
        }

        public override object Visit(Script context)
        {
            switch (Expression)
            {
                case "not":
                    if (Operand is BooleanNode booleanNode)
                    {
                        return !booleanNode.Boolean;
                    }
                    else if (Operand is BinaryExpressionNode binaryExpressionNode)
                    {
                        var returnNode = binaryExpressionNode.Visit(context);
                        if (returnNode is bool boolean)
                        {
                            return !boolean;
                        }
                    }
                    else if (Operand is UnaryExpressionNode unaryExpressionNode)
                    {
                        return !(bool)unaryExpressionNode.Visit(context);
                    }

                    break;
                case "-":
                    if (Operand is NumberNode numberNode)
                    {
                        return -numberNode.Literal;
                    }
                    else if (Operand is BinaryExpressionNode binaryExpressionNode)
                    {
                        var returnNode = binaryExpressionNode.Visit(context);
                        if (returnNode is float number)
                        {
                            return -number;
                        }
                    }
                    else if (Operand is UnaryExpressionNode unaryExpressionNode)
                    {
                        return -(float)unaryExpressionNode.Visit(context);
                    }
                    else if (Operand is GetVariableNode variableNode)
                    {
                        var result = variableNode.Visit(context);

                        if (result is int)
                        {
                            return -(double)(int)result;
                        }
                        else if (result is float)
                        {
                            return -(float)result;
                        }
                        else if (result is double)
                        {
                            return -(double)result;
                        }

                    }

                    break;
            }

            throw new InvalidOperationException(
                $"Unsupported operand type for '{Expression}' operation: {Operand.GetType()}");
        }
    }
}