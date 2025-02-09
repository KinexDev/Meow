using MeowLang.Internal.Parser.AST;
using MeowLang.Internal.Tokenizer;
using System;
using System.Collections.Generic;

namespace MeowLang.Internal.Parser
{
    public static class Parser
    {
        public class DispatchData
        {
            public DispatchType type { get; set; }
            public Token Token;
            public AstNode value;

            public DispatchData(DispatchType type, Token token, AstNode value)
            {
                this.type = type;
                Token = token;
                this.value = value;
            }
        }

        public enum DispatchType
        {
            Brackets,
            Unary,
            FunctionCall,
            VariableDeclaration
        }
        
        public static ProgramAST Parse(Token[] tokens)
        {
            var dispatch = new List<DispatchData>();
            ProgramAST program = new ProgramAST();
            
            AstNode nodeCurrentlyIn = new AstNode();
            
            
            for (var i = 0; i < tokens.Length; i++)
            {
                switch (tokens[i].TokenType)
                {
                    case TokenType.Number:
                        NumberNode numberNode = new NumberNode(float.Parse(tokens[i].Value));
                        if (CollectMinusUnary(tokens, numberNode, i, ref nodeCurrentlyIn, ref dispatch))
                            continue;
                        ParseType(numberNode, ref nodeCurrentlyIn);
                        break;
                    case TokenType.String:
                        StringNode stringNode = new StringNode(tokens[i].Value);
                        ParseType(stringNode, ref nodeCurrentlyIn);
                        break;
                    case TokenType.Operator:
                        if (i + 1 < tokens.Length)
                        {
                            //Unary parsing
                            if (tokens[i].Value == "not")
                            {
                                var nextToken = tokens[i + 1];
                                ParseUnaryOperator(nextToken, tokens[i],
                                    nextToken.TokenType != TokenType.Keyword &&
                                    nextToken.TokenType != TokenType.Operator &&
                                    nextToken.TokenType != TokenType.Bracket, ref nodeCurrentlyIn, ref dispatch);
                                continue;
                            }
                            if (tokens[i].Value == "-")
                            {
                                if (i == 0 || (tokens[i - 1].TokenType != TokenType.Number && tokens[i - 1].TokenType != TokenType.Identifier && tokens[i - 1].Value != ")"))
                                {
                                    var nextToken = tokens[i + 1];
                                    ParseUnaryOperator(nextToken, tokens[i],
                                        nextToken.TokenType != TokenType.Bracket && 
                                        nextToken.TokenType != TokenType.Number && 
                                        nextToken.TokenType != TokenType.Identifier, ref nodeCurrentlyIn, ref dispatch);   
                                    continue;
                                }
                            }

                            //binary parsing
                            if (tokens[i + 1].TokenType == TokenType.Number)
                            {
                                NumberNode nextNum = new NumberNode(float.Parse(tokens[i + 1].Value));
                                ParseOperatorPrecedence(tokens[i], nextNum, ref nodeCurrentlyIn, ref i);
                            } else if (tokens[i + 1].TokenType == TokenType.Keyword)
                            {
                                if (tokens[i + 1].Value == "false" || tokens[i + 1].Value == "true")
                                {
                                    BooleanNode nextNum = new BooleanNode(bool.Parse(tokens[i + 1].Value));
                                    ParseOperatorPrecedence(tokens[i], nextNum, ref nodeCurrentlyIn, ref i);
                                }
                                else
                                    throw new InterpreterException(tokens[i + 1].Line, $"Incorrect keyword '{tokens[i + 1].Value}', expected 'true' or 'false'");
                            } else if (tokens[i + 1].TokenType == TokenType.String)
                            {
                                StringNode nextNum = new StringNode(tokens[i + 1].Value);
                                ParseOperatorPrecedence(tokens[i], nextNum, ref nodeCurrentlyIn, ref i);
                            } else if (tokens[i + 1].TokenType == TokenType.Identifier)
                            {
                                GetVariableNode nextNum = new GetVariableNode(tokens[i + 1].Value);
                                ParseOperatorPrecedence(tokens[i], nextNum, ref nodeCurrentlyIn, ref i);
                            }
                            else
                            {
                                if (tokens[i + 1].TokenType == TokenType.Bracket)
                                {
                                    if (tokens[i + 1].Value == ")")
                                        throw new InterpreterException(tokens[i + 1].Line, $"Expected a value after the operator '{tokens[i].Value}' near ')'");
                                    
                                    BinaryExpressionNode newBinaryNode = new BinaryExpressionNode(tokens[i].Value, nodeCurrentlyIn, new AstNode());
                                    nodeCurrentlyIn = newBinaryNode;
                                    continue;
                                }

                                if (tokens[i + 1].TokenType == TokenType.Operator && tokens[i + 1].Value == "not" ||
                                    tokens[i + 1].Value == "-")
                                {
                                    BinaryExpressionNode newBinaryNode = new BinaryExpressionNode(tokens[i].Value, nodeCurrentlyIn, new AstNode());
                                    nodeCurrentlyIn = newBinaryNode;
                                    continue;
                                }
                                
                                if (!string.IsNullOrEmpty(tokens[i + 1].Value))
                                    throw new InterpreterException(tokens[i + 1].Line, $"Expected a value after the operator '{tokens[i].Value}' near token '{tokens[i + 1].Value}'");
                                else
                                    throw new InterpreterException(tokens[i + 1].Line, $"Expected a value after the operator '{tokens[i].Value}'");
                            }
                        }
                        else
                            throw new ArithmeticException($"Expected a value after the operator.");
                        break;
                    case TokenType.Bracket :
                        if (tokens[i].Value == "(")
                        {
                            dispatch.Add(new DispatchData(DispatchType.Brackets, tokens[i], nodeCurrentlyIn));
                            nodeCurrentlyIn = new AstNode();
                        }

                        if (tokens[i].Value == ")")
                        {
                            if (dispatch.Count == 0)
                                throw new InterpreterException(tokens[i].Line, "Unexpected closing bracket. No opening bracket found to match it.");
                            if (dispatch[^1].type == DispatchType.Brackets)
                            {
                                var bracketNode = nodeCurrentlyIn;

                                if (bracketNode is BinaryExpressionNode bebracketNode)
                                {
                                    bebracketNode.InBracket = true;
                                }
                                if (dispatch[^1].value is VariableDeclarationNode varNode)
                                {
                                    varNode.Value = nodeCurrentlyIn;
                                    nodeCurrentlyIn = varNode;
                                    dispatch.RemoveAt(dispatch.Count - 1);
                                }
                                else
                                {
                                    nodeCurrentlyIn = dispatch[^1].value;
                                }
                                
                                ParseBrackets(bracketNode, ref nodeCurrentlyIn, ref dispatch);
                                dispatch.RemoveAt(dispatch.Count - 1);
                            } else if (dispatch[^1].type == DispatchType.FunctionCall)
                            {
                                var functionCallNode = (FunctionCallNode)dispatch[^1].value;
                                functionCallNode.Arguments.Add(nodeCurrentlyIn);
                                nodeCurrentlyIn = dispatch[^1].value;
                                dispatch.RemoveAt(dispatch.Count - 1);
                            }
                            else
                                throw new InterpreterException(tokens[i + 1].Line, "Expected a ')' after the bracket in line.");
                        }
                        break;
                    case TokenType.Keyword:
                        //handeling not
                        if (dispatch.Count > 0)
                        {
                            var previousDispatchData = dispatch[^1];

                            if (previousDispatchData.type == DispatchType.Unary)
                            {
                                if ((tokens[i].Value == "false" || tokens[i].Value == "true") &&
                                    nodeCurrentlyIn is UnaryExpressionNode node)
                                {
                                    node.Operand =
                                        new BooleanNode(bool.Parse(tokens[i].Value));   
                                }
                                else
                                    throw new InterpreterException(tokens[i].Line, $"Incorrect keyword '{tokens[i].Value}'");
                                
                                nodeCurrentlyIn = previousDispatchData.value;

                                ParseType(node, ref nodeCurrentlyIn);
                                
                                dispatch.RemoveAt(dispatch.Count - 1);
                                continue;
                            }
                        }
                        
                        if (tokens[i].Value == "false" || tokens[i].Value == "true")
                        {
                            BooleanNode booleanNode = new BooleanNode(bool.Parse(tokens[i].Value));
                            ParseType(booleanNode, ref nodeCurrentlyIn);
                        }

                        if (tokens[i].Value == "null")
                        {
                            NullNode nullNode = new NullNode();
                            ParseType(nullNode, ref nodeCurrentlyIn);
                        }
                        break;
                    case TokenType.Identifier:
                        var additional = SkipIfTypeHinting(tokens, ref i);
                        
                        if (tokens[i + 1].TokenType == TokenType.Bracket && tokens[i + 1].Value == "(")
                        {
                            if (nodeCurrentlyIn is FunctionCallNode)
                                throw new InterpreterException(tokens[i].Line, "semicolon is required.");
                            nodeCurrentlyIn = new FunctionCallNode(tokens[i].Value);
                            dispatch.Add(new DispatchData(DispatchType.FunctionCall, tokens[i], nodeCurrentlyIn));
                            nodeCurrentlyIn = new AstNode();
                            i++;
                        } else if ((tokens[i + 1].TokenType == TokenType.Operator && tokens[i + 1].Value == "=") || (tokens[i + 1].Value == ":"))
                        {
                            nodeCurrentlyIn = new VariableDeclarationNode(tokens[i].Value);
                            dispatch.Add(new DispatchData(DispatchType.VariableDeclaration, tokens[i], nodeCurrentlyIn));
                            i += additional;
                        }
                        else
                        {
                            GetVariableNode variable = new GetVariableNode(tokens[i].Value);
                            if (CollectMinusUnary(tokens, variable, i, ref nodeCurrentlyIn, ref dispatch))
                                continue;
                            ParseType(variable, ref nodeCurrentlyIn);
                        }
                        break;
                    case TokenType.Punctuation:
                        if (dispatch.Count > 0)
                        {
                            var previousDispatchData = dispatch[^1];
                            if (previousDispatchData.type == DispatchType.FunctionCall)
                            {
                                //we are done with variable declaration now!
                                var functionCallNode = (FunctionCallNode)previousDispatchData.value;
                                functionCallNode.Arguments.Add(nodeCurrentlyIn);
                                nodeCurrentlyIn = new AstNode();
                            }
                        }
                        break;
                    case TokenType.Terminator or TokenType.Eol:
                        if (dispatch.Count > 0)
                        {
                            var previousDispatchData = dispatch[^1];
                            if (previousDispatchData.type == DispatchType.VariableDeclaration)
                            {
                                //we are done with variable declaration now!
                                var declarationNode = (VariableDeclarationNode)previousDispatchData.value;
                                declarationNode.Value = nodeCurrentlyIn;
                                nodeCurrentlyIn = declarationNode;
                                dispatch.RemoveAt(dispatch.Count - 1);
                            }
                        }

                        program.Statements.Add(nodeCurrentlyIn);
                        nodeCurrentlyIn = new AstNode();
                        break;
                }
            }

            if (dispatch.Count != 0)
            {
                throw new InterpreterException(dispatch[0].Token.Line, $"Code not ended at '{dispatch[0].Token.Value}'");
            }
            return program;
        }

        private static bool CollectMinusUnary(Token[] tokens, AstNode passingIn, int i, ref AstNode nodeCurrentlyIn, ref List<DispatchData> dispatch)
        {
            if (dispatch.Count > 0)
            {
                var previousDispatchData = dispatch[^1];

                if (previousDispatchData.type == DispatchType.Unary)
                {
                    if (nodeCurrentlyIn is UnaryExpressionNode node)
                    {
                        node.Operand = passingIn;   
                                    
                        nodeCurrentlyIn = previousDispatchData.value;

                        ParseType(node, ref nodeCurrentlyIn);
                                
                        dispatch.RemoveAt(dispatch.Count - 1);

                        return true;
                    }
                }
            }

            return false;
        }
        
        private static int SkipIfTypeHinting(Token[] tokens, ref int i)
        {
            if (tokens[i + 1].TokenType == TokenType.Punctuation && tokens[i + 1].Value == ":")
            {
                if (tokens[i + 2].TokenType != TokenType.Identifier && tokens[i + 2].TokenType != TokenType.Keyword)
                {
                    throw new InterpreterException(tokens[i + 2].Line, "Invalid type.");
                }

                return 3;
            }

            return 1;
        }
        
        private static void ParseType(AstNode nodeType, ref AstNode nodeCurrentlyIn)
        {
            if (nodeCurrentlyIn is BinaryExpressionNode binaryNode)
            {
                binaryNode.Right = nodeType;
            }
            else
            {
                nodeCurrentlyIn = nodeType;
            }
        }

        private static void ParseUnaryOperator(Token nextToken, Token currentToken, bool check, ref AstNode nodeCurrentlyIn, ref List<DispatchData> dispatch)
        {
            if (check)
            {
                throw new InterpreterException(nextToken.Line, $"Invalid token for '{currentToken.Value}' operator");
            }
            
            UnaryExpressionNode node = new UnaryExpressionNode(currentToken.Value);
            dispatch.Add(new DispatchData(DispatchType.Unary, currentToken, nodeCurrentlyIn));
            nodeCurrentlyIn = node;
        }
        
        private static void ParseBrackets(AstNode bracketNode, ref AstNode nodeCurrentlyIn, ref List<DispatchData> dispatch)
        {
            if (nodeCurrentlyIn is BinaryExpressionNode brBinaryNode)
            {
                if (brBinaryNode.Left is BinaryExpressionNode leftBinaryNode)
                {
                    if (OperatorPrecedence(brBinaryNode.Expression) > OperatorPrecedence(leftBinaryNode.Expression) &&
                        !leftBinaryNode.InBracket)
                    {
                        BinaryExpressionNode newBinaryNode = new BinaryExpressionNode(brBinaryNode.Expression,
                            leftBinaryNode.Right, bracketNode);

                        leftBinaryNode.Right = newBinaryNode;

                        nodeCurrentlyIn = leftBinaryNode;
                    }
                    else
                    {
                        BinaryExpressionNode newBinaryNode =
                            new BinaryExpressionNode(brBinaryNode.Expression, leftBinaryNode, bracketNode);
                        nodeCurrentlyIn = newBinaryNode;
                    }
                }
                else
                {
                    brBinaryNode.Right = bracketNode;
                    nodeCurrentlyIn = brBinaryNode;
                }
            }
            else if (nodeCurrentlyIn is UnaryExpressionNode unaryNode)
            {
                unaryNode.Operand = bracketNode;
                dispatch.RemoveAt(dispatch.Count - 1);
                // well unarys also use dispatch, so saafe to assume to go through and get the unary correctly. and so we will check just rq because then essentially we are scrapping the entire thing we just built.
                var oldNode = dispatch[^1].value;
                ParseBrackets(unaryNode, ref oldNode, ref dispatch);
                
                nodeCurrentlyIn = oldNode;
            }
            else if (nodeCurrentlyIn.GetType() == typeof(AstNode))
            {
                nodeCurrentlyIn = bracketNode;
            }
        }

        private static void ParseOperatorPrecedence(Token currentToken, AstNode NewNode, ref AstNode nodeCurrentlyIn, ref int i)
        {
            if (nodeCurrentlyIn is BinaryExpressionNode opBinaryNode)
            {
                if (OperatorPrecedence(opBinaryNode.Expression) < OperatorPrecedence(currentToken.Value) && !opBinaryNode.InBracket)
                {
                    BinaryExpressionNode newBinaryNode = new BinaryExpressionNode(currentToken.Value, opBinaryNode.Right, NewNode);

                    opBinaryNode.Right = newBinaryNode;
                                    
                    nodeCurrentlyIn = opBinaryNode;
                                    
                }
                else
                {
                    BinaryExpressionNode newBinaryNode = new BinaryExpressionNode(currentToken.Value, opBinaryNode, NewNode);
                    nodeCurrentlyIn = newBinaryNode;
                }
            }
            else
            {
                BinaryExpressionNode newBinaryNode = new BinaryExpressionNode(currentToken.Value, nodeCurrentlyIn, NewNode);

                nodeCurrentlyIn = newBinaryNode;
            }

            i++;
        }
        
        private static int OperatorPrecedence(string operatorToken)
        {
            switch (operatorToken)
            {
                case "+" or "-":
                    return 1;
                case "*" or "/":
                    return 2;
                case "or":
                    return -2;
                case "and":
                    return -1;
                default:
                    return 1;
            }
        }
    }
}
