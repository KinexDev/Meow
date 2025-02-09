namespace MeowLang.Internal.Parser.AST
{
    public class VariableDeclarationNode : AstNode
    {
        public string Identifier { get; set; }
        public AstNode Value { get; set; }

        public VariableDeclarationNode(string identifier)
        {
            Identifier = identifier;
        }

        public VariableDeclarationNode(string identifier, AstNode value)
        {
            Identifier = identifier;
            Value = value;
        }

        public override object Visit(Script context)
        {
            context.SetGlobal(Identifier, Value.Visit(context));
            return null;
        }
    }
}