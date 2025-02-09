namespace MeowLang.Internal.Parser.AST;

public class ParameterNode : AstNode
{
    public string Identifier { get; set; }

    public ParameterNode(string identifier)
    {
        Identifier = identifier;
    }
}