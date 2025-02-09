using System.Collections.Generic;

namespace MeowLang.Internal.Parser.AST
{
    public class ProgramAST : AstNode
    {
        public List<AstNode> Statements = new List<AstNode>();

        public override object Visit(Script context)
        {
            foreach (var statement in Statements)
            {
                statement.Visit(context);
            }

            return null;
        }
    }
}