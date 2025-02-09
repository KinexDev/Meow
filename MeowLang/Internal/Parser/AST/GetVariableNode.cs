namespace MeowLang.Internal.Parser.AST;

public class GetVariableNode : AstNode
{
    public string Identifier { get; set; }
    
    public GetVariableNode(string identifier)
    {
        Identifier = identifier;
    }
    
    public override object Visit(Script context)
    {
        return context.GetGlobal(Identifier);
    }
}