using System.Collections.Generic;

namespace MeowLang.Internal.Parser.AST
{
    public class FunctionNode : AstNode
    {
        public List<AstNode> FunctionNodes { get; set; }
        public List<string> parameters { get; set; }

        public override object Visit(Script context)
        {
            return null;
        }

        public object Call(object[] args, Script context)
        {
            return null;
        }
    }
}