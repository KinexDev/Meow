using System.Collections.Generic;

namespace MeowLang.Internal.Parser.AST
{
    public class Function : AstNode
    {
        public List<AstNode> FunctionNodes { get; set; } = new();
        public List<string> parameters { get; set; } = new();

        public override object Visit(Script context)
        {
            return this;
        }

        public object Call(object[] args, Script context)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (parameters.Count > i)
                    context.SetGlobal(parameters[i], args[i]);
            }
            
            foreach (var statement in FunctionNodes)
            {
                
                var result = statement.Visit(context);

                if (result is ReturnNode returnNode)
                {
                    return returnNode.ReturnValue.Visit(context);
                }
            }
            
            return null;
        }
    }
}