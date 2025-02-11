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
            VariableDeclaration,
            Function,
            FunctionParameters,
            Return,
            If,
            IfCondition,
            While,
            WhileCondition,
            ElseIfCondition,
            ElseIf,
            Else
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
                        ThrowErrorOnFunctionParameters(tokens[i], ref dispatch);
                        NumberNode numberNode = new NumberNode(float.Parse(tokens[i].Value));
                        if (CollectMinusUnary(tokens, numberNode, i, ref nodeCurrentlyIn, ref dispatch))
                            continue;
                        ParseType(tokens[i], numberNode, ref nodeCurrentlyIn);
                        break;
                    case TokenType.String:
                        ThrowErrorOnFunctionParameters(tokens[i], ref dispatch);
                        StringNode stringNode = new StringNode(tokens[i].Value);
                        ParseType(tokens[i], stringNode, ref nodeCurrentlyIn);
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
                                    nextToken.TokenType != TokenType.Bracket && 
                                    nextToken.TokenType != TokenType.Identifier, ref nodeCurrentlyIn, ref dispatch);
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
                                if (tokens[i + 1].Value == "null")
                                {
                                    NullNode nextNum = new NullNode();
                                    ParseOperatorPrecedence(tokens[i], nextNum, ref nodeCurrentlyIn, ref i);
                                }
                                else if (tokens[i + 1].Value == "false" || tokens[i + 1].Value == "true")
                                {
                                    BooleanNode boolNode = new BooleanNode(bool.Parse(tokens[i + 1].Value));
                                    ParseOperatorPrecedence(tokens[i], boolNode, ref nodeCurrentlyIn, ref i);
                                }
                                else
                                    throw new InterpreterException(tokens[i + 1].Line, $"Incorrect keyword '{tokens[i + 1].Value}', expected 'true', 'false'");
                            } else if (tokens[i + 1].TokenType == TokenType.String)
                            {
                                StringNode strNode = new StringNode(tokens[i + 1].Value);
                                ParseOperatorPrecedence(tokens[i], strNode, ref nodeCurrentlyIn, ref i);
                            } else if (tokens[i + 1].TokenType == TokenType.Identifier)
                            {
                                if (i + 2 < tokens.Length)
                                {
                                    // this means its a function call
                                    if (tokens[i + 2].Value == "(")
                                    {
                                        var functionCallNode = new FunctionCallNode(tokens[i + 1].Value);
                                        ParseOperatorPrecedence(tokens[i], functionCallNode, ref nodeCurrentlyIn, ref i);
                                        dispatch.Add(new DispatchData(DispatchType.FunctionCall, tokens[i], nodeCurrentlyIn));
                                        nodeCurrentlyIn = new AstNode();
                                        i++;
                                        continue;   
                                    }
                                }
                                
                                GetVariableNode variableNode = new GetVariableNode(tokens[i + 1].Value);
                                ParseOperatorPrecedence(tokens[i], variableNode, ref nodeCurrentlyIn, ref i);
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
                                FunctionCallNode functionCallNode;
                                if (dispatch[^1].value is BinaryExpressionNode br)
                                {
                                    if (br.Left is FunctionCallNode)
                                        functionCallNode = (FunctionCallNode)br.Left;
                                    else
                                        functionCallNode = (FunctionCallNode)br.Right;
                                } else
                                    functionCallNode = (FunctionCallNode)dispatch[^1].value;
                                // doesn't know when to stop parsing, so we do this.
                                functionCallNode.Arguments.Add(nodeCurrentlyIn);
                                nodeCurrentlyIn = dispatch[^1].value;
                                dispatch.RemoveAt(dispatch.Count - 1);

                                if (dispatch.Count > 0)
                                {
                                    if (dispatch[^1].type == DispatchType.Unary)
                                    {
                                        var node = new UnaryExpressionNode(dispatch[^1].Token.Value, nodeCurrentlyIn);
                                        
                                        ParseType(tokens[i], node, ref dispatch[^1].value);
                                        nodeCurrentlyIn = dispatch[^1].value;
                                        dispatch.RemoveAt(dispatch.Count - 1);
                                    }    
                                }
                            } else if (dispatch[^1].type == DispatchType.FunctionParameters)
                            {
                                nodeCurrentlyIn = dispatch[^1].value;
                                dispatch.RemoveAt(dispatch.Count - 1);
                                
                                var additionalFunctionHint = SkipIfTypeHinting(tokens, ref i);
                                
                                if (tokens[i + additionalFunctionHint].Value == "{")
                                {
                                    dispatch.Add(new DispatchData(DispatchType.Function, tokens[i], nodeCurrentlyIn));
                                    nodeCurrentlyIn = new AstNode();
                                }
                                else
                                    throw new InterpreterException(tokens[i + additionalFunctionHint].Line,
                                        "no '{' after declaring a function,");
                                
                                i += additionalFunctionHint;
                            } else if (dispatch[^1].type == DispatchType.IfCondition)
                            {
                                dispatch.RemoveAt(dispatch.Count - 1);
                                if (tokens[i + 1].Value == "{")
                                {
                                    nodeCurrentlyIn = new IfNode(nodeCurrentlyIn);
                                    dispatch.Add(new DispatchData(DispatchType.If, tokens[i], nodeCurrentlyIn));
                                    nodeCurrentlyIn = new AstNode();
                                }
                                else
                                    throw new InterpreterException(tokens[i + 1].Line,
                                        "no '{' after declaring a function,");
                                i++;
                            } else if (dispatch[^1].type == DispatchType.WhileCondition)
                            {
                                dispatch.RemoveAt(dispatch.Count - 1);
                                if (tokens[i + 1].Value == "{")
                                {
                                    nodeCurrentlyIn = new WhileNode(nodeCurrentlyIn);
                                    dispatch.Add(new DispatchData(DispatchType.While, tokens[i], nodeCurrentlyIn));
                                    nodeCurrentlyIn = new AstNode();
                                }
                                else
                                    throw new InterpreterException(tokens[i + 1].Line,
                                        "no '{' after declaring a function,");
                                i++;
                            }
                            else
                                throw new InterpreterException(tokens[i + 1].Line, "Expected a ')' after the bracket in line.");
                        }
                        
                        ThrowErrorOnFunctionParameters(tokens[i], ref dispatch);
                        
                        if (tokens[i].Value == "(")
                        {
                            dispatch.Add(new DispatchData(DispatchType.Brackets, tokens[i], nodeCurrentlyIn));
                            nodeCurrentlyIn = new AstNode();
                        }
                        break;
                    case TokenType.Keyword:
                        ThrowErrorOnFunctionParameters(tokens[i], ref dispatch);
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

                                ParseType(tokens[i], node, ref nodeCurrentlyIn);
                                
                                dispatch.RemoveAt(dispatch.Count - 1);
                                continue;
                            }
                        }
                        
                        if (tokens[i].Value == "false" || tokens[i].Value == "true")
                        {
                            BooleanNode booleanNode = new BooleanNode(bool.Parse(tokens[i].Value));
                            ParseType(tokens[i], booleanNode, ref nodeCurrentlyIn);
                        }

                        if (tokens[i].Value == "null")
                        {
                            NullNode nullNode = new NullNode();
                            ParseType(tokens[i], nullNode, ref nodeCurrentlyIn);
                        }

                        if (tokens[i].Value == "function")
                        {
                            if (tokens[i + 1].TokenType == TokenType.Bracket && tokens[i + 1].Value == "(")
                            {
                                nodeCurrentlyIn = new Function();
                                dispatch.Add(new DispatchData(DispatchType.FunctionParameters, tokens[i], nodeCurrentlyIn));
                                nodeCurrentlyIn = new AstNode();
                                i++;
                            } else 
                                throw new InterpreterException(tokens[i].Line, "Expected a bracket in function call");
                        }

                        if (tokens[i].Value == "return")
                        {
                            nodeCurrentlyIn = new ReturnNode();
                            dispatch.Add(new DispatchData(DispatchType.Return, tokens[i], nodeCurrentlyIn));
                            nodeCurrentlyIn = new AstNode();
                        }

                        if (tokens[i].Value == "if")
                        {
                            if (tokens[i + 1].TokenType == TokenType.Bracket && tokens[i + 1].Value == "(")
                            {
                                dispatch.Add(new DispatchData(DispatchType.IfCondition, tokens[i], nodeCurrentlyIn));
                                nodeCurrentlyIn = new AstNode();
                                i++;
                            } else 
                                throw new InterpreterException(tokens[i].Line, "Expected a bracket after if");
                        }
                        
                        if (tokens[i].Value == "while")
                        {
                            if (tokens[i + 1].TokenType == TokenType.Bracket && tokens[i + 1].Value == "(")
                            {
                                dispatch.Add(new DispatchData(DispatchType.WhileCondition, tokens[i], nodeCurrentlyIn));
                                nodeCurrentlyIn = new AstNode();
                                i++;
                            } else 
                                throw new InterpreterException(tokens[i].Line, "Expected a bracket after while");
                        }
                        break;
                    case TokenType.Identifier:
                        var additional = SkipIfTypeHinting(tokens, ref i);

                        if (dispatch.Count > 0)
                        {
                            var previousDispatchData = dispatch[^1];
                            if (previousDispatchData.type == DispatchType.FunctionParameters)
                            {
                                if (nodeCurrentlyIn.GetType() != typeof(AstNode))
                                    throw new InterpreterException(tokens[i].Line, $"Starting a new parameter without ',' near {tokens[i].Value}");

                                var function = (Function)previousDispatchData.value;
                                function.parameters.Add(tokens[i].Value);
                                nodeCurrentlyIn = new ParameterNode(tokens[i].Value);
                                i += additional - 1;
                                continue;
                            }
                        }
                        
                        if (tokens[i + 1].TokenType == TokenType.Bracket && tokens[i + 1].Value == "(")
                        {
                            if (nodeCurrentlyIn is FunctionCallNode)
                                throw new InterpreterException(tokens[i].Line, $"Expected semicolon at the end of '{tokens[i].Value}' function");
                            var functionCallNode = new FunctionCallNode(tokens[i].Value);
                            nodeCurrentlyIn = functionCallNode;
                            
                            dispatch.Add(new DispatchData(DispatchType.FunctionCall, tokens[i], nodeCurrentlyIn));
                            nodeCurrentlyIn = new AstNode();
                            i++;
                        } else if ((tokens[i + 1].TokenType == TokenType.Operator && tokens[i + 1].Value == "=") || (tokens[i + 1].Value == ":"))
                        {
                            if (dispatch.Count > 0)
                            {
                                var previousDispatchData = dispatch[^1];

                                if (previousDispatchData.type == DispatchType.VariableDeclaration)
                                    throw new InterpreterException(tokens[i].Line, $"Expected semicolon at the end of '{tokens[i - 1].Value}'");
                            }

                            nodeCurrentlyIn = new VariableDeclarationNode(tokens[i].Value);
                            dispatch.Add(new DispatchData(DispatchType.VariableDeclaration, tokens[i], nodeCurrentlyIn));
                            i += additional;
                            nodeCurrentlyIn = new AstNode();
                        }
                        else
                        {
                            GetVariableNode variable = new GetVariableNode(tokens[i].Value);
                            if (CollectMinusUnary(tokens, variable, i, ref nodeCurrentlyIn, ref dispatch))
                                continue;
                            ParseType(tokens[i], variable, ref nodeCurrentlyIn);
                        }
                        break;
                    case TokenType.Punctuation:
                        if (tokens[i].Value == ",")
                        {
                            if (dispatch.Count > 0)
                            {
                                var previousDispatchData = dispatch[^1];
                                if (previousDispatchData.type == DispatchType.FunctionCall)
                                {
                                    FunctionCallNode functionCallNode;
                                    if (dispatch[^1].value is BinaryExpressionNode br)
                                    {
                                        if (br.Left is FunctionCallNode)
                                            functionCallNode = (FunctionCallNode)br.Left;
                                        else
                                            functionCallNode = (FunctionCallNode)br.Right;
                                    }
                                    else
                                        functionCallNode = (FunctionCallNode)dispatch[^1].value;
                                    functionCallNode.Arguments.Add(nodeCurrentlyIn);
                                    nodeCurrentlyIn = new AstNode();
                                    continue;
                                }
                                else if (previousDispatchData.type == DispatchType.FunctionParameters)
                                {
                                    nodeCurrentlyIn = new AstNode();
                                    continue;
                                }
                            }   
                        } else if (tokens[i].Value == "}")
                        {
                            if (dispatch.Count > 0)
                            {
                                var previousDispatchData = dispatch[^1];
                                if (previousDispatchData.type == DispatchType.Function)
                                {
                                    var functionNode = (Function)previousDispatchData.value;
                                    if (!isEmptyNode(nodeCurrentlyIn))
                                        functionNode.FunctionNodes.Add(nodeCurrentlyIn);
                                    nodeCurrentlyIn = functionNode;
                                    dispatch.RemoveAt(dispatch.Count - 1);
                                    if (!(dispatch.Count > 0 && dispatch[^1].type == DispatchType.FunctionCall))
                                    {
                                        TerminatorLogic(ref program, ref nodeCurrentlyIn, ref dispatch);
                                    }
                                    continue;
                                } else if (previousDispatchData.type == DispatchType.If)
                                {
                                    var functionNode = (IfNode)previousDispatchData.value;
                                    if (!isEmptyNode(nodeCurrentlyIn))
                                        functionNode.Statements.Add(nodeCurrentlyIn);
                                    nodeCurrentlyIn = functionNode;
                                    dispatch.RemoveAt(dispatch.Count - 1);
                                    if (tokens.Length - 1 > i + 1)
                                    {
                                        if (tokens[i + 1].TokenType == TokenType.Keyword && tokens[i + 1].Value == "else")
                                        {
                                            var elseNode = new ElseNode();;
                                            functionNode.elseNode = elseNode;
                                            nodeCurrentlyIn = functionNode;
                                            
                                            if (tokens.Length - 1 > i + 2)
                                            {
                                                if (tokens[i + 2].Value == "{")
                                                {
                                                    dispatch.Add(new DispatchData(DispatchType.Else, tokens[i + 1],
                                                        nodeCurrentlyIn));
                                                    nodeCurrentlyIn = new AstNode();
                                                    i += 2;
                                                }
                                                else
                                                    throw new InterpreterException(tokens[i + 2].Line,
                                                        "expected { after else");
                                            } else
                                                throw new InterpreterException(tokens[i + 2].Line,
                                                    "expected { after else");
                                        }   
                                    }
                                    if (!(dispatch.Count > 0 && dispatch[^1].type == DispatchType.FunctionCall))
                                    {
                                        TerminatorLogic(ref program, ref nodeCurrentlyIn, ref dispatch);
                                    }
                                    continue;
                                    
                                } else if (previousDispatchData.type == DispatchType.While)
                                {
                                    var functionNode = (WhileNode)previousDispatchData.value;
                                    if (!isEmptyNode(nodeCurrentlyIn))
                                        functionNode.Statements.Add(nodeCurrentlyIn);
                                    nodeCurrentlyIn = functionNode;
                                    dispatch.RemoveAt(dispatch.Count - 1);
                                    if (!(dispatch.Count > 0 && dispatch[^1].type == DispatchType.FunctionCall))
                                    {
                                        TerminatorLogic(ref program, ref nodeCurrentlyIn, ref dispatch);
                                    }
                                    continue;
                                }else if (previousDispatchData.type == DispatchType.Else)
                                {
                                    var functionNode = (IfNode)previousDispatchData.value;
                                    if (!isEmptyNode(nodeCurrentlyIn))
                                        functionNode.elseNode.Statements.Add(nodeCurrentlyIn);
                                    nodeCurrentlyIn = functionNode;
                                    dispatch.RemoveAt(dispatch.Count - 1);
                                    if (!(dispatch.Count > 0 && dispatch[^1].type == DispatchType.FunctionCall))
                                    {
                                        TerminatorLogic(ref program, ref nodeCurrentlyIn, ref dispatch);
                                    }
                                    continue;
                                }
                            }  
                        }
                        throw new InterpreterException(tokens[i].Line, $"Unexpected token {tokens[i].Value}");
                        break;
                    case TokenType.Terminator:
                        TerminatorLogic(ref program, ref nodeCurrentlyIn, ref dispatch);
                        break;
                }
            }

            if (tokens.Length > 1)
            {
                CheckSemicolon(tokens.Length - 1, tokens, nodeCurrentlyIn);
            }

            if (dispatch.Count != 0)
            {
                throw new InterpreterException(dispatch[0].Token.Line, $"Code not ended at '{dispatch[0].Token.Value}'");
            }
            return program;
        }

        public static bool isEmptyNode(AstNode node)
        {
            return node.GetType() == typeof(AstNode);
        }
        private static void CheckSemicolon(int index, Token[] tokens, AstNode nodeCurrentlyIn, bool nonstrict = false)
        {
            if (tokens[index].TokenType != TokenType.Terminator && tokens[^1].Value != "}")
            {
                if (tokens[^1].Value == "{")
                    throw new InterpreterException(tokens[index].Line, "Expected '}' at the end of '{'");
                else
                    throw new InterpreterException(tokens[index].Line,
                        $"Expected semicolon at the end of '{tokens[index].Value}'");
            }
        }

        private static void TerminatorLogic(ref ProgramAST program, ref AstNode nodeCurrentlyIn, ref List<DispatchData> dispatch)
        {
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
                    TerminatorLogic(ref program, ref nodeCurrentlyIn, ref dispatch);
                    return;
                } else if (previousDispatchData.type == DispatchType.Return)
                {
                    //we are done with return now
                    var returnNode = (ReturnNode)previousDispatchData.value;
                    returnNode.ReturnValue = nodeCurrentlyIn;
                    nodeCurrentlyIn = returnNode;
                    dispatch.RemoveAt(dispatch.Count - 1);
                    TerminatorLogic(ref program, ref nodeCurrentlyIn, ref dispatch);
                    return;
                }else if (previousDispatchData.type == DispatchType.Function)
                {
                    var functionNode = (Function)previousDispatchData.value;
                    functionNode.FunctionNodes.Add(nodeCurrentlyIn);
                    nodeCurrentlyIn = new AstNode();
                    return;
                }else if (previousDispatchData.type == DispatchType.If)
                {
                    var functionNode = (IfNode)previousDispatchData.value;
                    functionNode.Statements.Add(nodeCurrentlyIn);
                    nodeCurrentlyIn = new AstNode();
                    return;
                }else if (previousDispatchData.type == DispatchType.While)
                {
                    var functionNode = (WhileNode)previousDispatchData.value;
                    functionNode.Statements.Add(nodeCurrentlyIn);
                    nodeCurrentlyIn = new AstNode();
                    return;
                }else if (previousDispatchData.type == DispatchType.Else)
                {
                    var functionNode = (IfNode)previousDispatchData.value;
                    functionNode.elseNode.Statements.Add(nodeCurrentlyIn);
                    nodeCurrentlyIn = new AstNode();
                    return;
                }
            }
            
            program.Statements.Add(nodeCurrentlyIn);
            nodeCurrentlyIn = new AstNode();
        }

        private static void ThrowErrorOnFunctionParameters(Token tokens,
            ref List<DispatchData> dispatch)
        {
            if (dispatch.Count > 0)
            {
                var previousDispatchData = dispatch[^1];
                if (previousDispatchData.type == DispatchType.FunctionParameters)
                    throw new InterpreterException(tokens.Line,
                        $"Cannot put type {tokens.TokenType} in parameter declaration.");
            }
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

                        ParseType(tokens[i], node, ref nodeCurrentlyIn);
                                
                        dispatch.RemoveAt(dispatch.Count - 1);

                        return true;
                    }
                }
            }

            return false;
        }

        public static List<string> validTypes = new List<string>() { "number", "string", "boolean", "function", "null", "void" };
        
        private static int SkipIfTypeHinting(Token[] tokens, ref int i)
        {
            if (tokens[i + 1].TokenType == TokenType.Punctuation && tokens[i + 1].Value == ":")
            {
                if ((tokens[i + 2].TokenType != TokenType.Identifier && tokens[i + 2].TokenType != TokenType.Keyword) || !validTypes.Contains(tokens[i + 2].Value))
                {
                    throw new InterpreterException(tokens[i + 2].Line, $"Invalid type. {tokens[i + 2].Value}");
                }

                return 3;
            }

            return 1;
        }
        
        private static void ParseType(Token token, AstNode nodeType, ref AstNode nodeCurrentlyIn)
        {
            if (nodeCurrentlyIn is BinaryExpressionNode binaryNode)
            {
                binaryNode.Right = nodeType;
                return;
            }
            
            if (nodeCurrentlyIn.GetType() != typeof(AstNode))
                throw new InterpreterException(token.Line, $"Starting a new expression before ending previous one? near {token.Value}");
            
            nodeCurrentlyIn = nodeType;
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
