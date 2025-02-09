using System.Collections.Generic;

namespace MeowLang.Internal.Parser.AST
{
    public class FunctionCallNode : AstNode
    {
        public string Identifier { get; set; }
        public List<AstNode> Arguments { get; set; } = new();

        public FunctionCallNode(string identifier)
        {
            Identifier = identifier;
        }
    
        public FunctionCallNode(string identifier, List<AstNode> arguments)
        {
            Identifier = identifier;
            Arguments = arguments;
        }

        public override object Visit(Script context)
        {
            List<object> arguments = new();

            foreach (AstNode argument in Arguments)
            {
                arguments.Add(argument.Visit(context));
            }
            
            if (context.GetGlobal(Identifier) is MeowDelegate meowDelegate)
            {
                return meowDelegate.Invoke(arguments.ToArray());
            } else if (context.GetGlobal(Identifier) is FunctionNode function)
            {
                return function.Call(arguments.ToArray(), context);
            }

            return null;
        }
    }   
}