namespace HourScript.Parsing;

using HourScript.AST;
using HourScript.Lexing;

public partial class Parser
{
    ListLiteral? ParseList()
    {
        tokenReader.Read();

        List<Node> nodes = new();
        bool failed = false;

        while (tokenReader.hasNext && !tokenReader.Peek().Match(TokenType.keyword, "]"))
        {
            Node? node = ParseExpression();
            if (node is null) failed = true;
            else nodes.Add(node);
            
            if (tokenReader.Peek().Match(TokenType.keyword, ","))
            {
                tokenReader.Read();
            }
            else if (tokenReader.hasNext && !tokenReader.Peek().Match(TokenType.keyword, "]"))
            {
                Errors.AddError($"ERR!  at {tokenReader.Peek().positionAsString}: A comma expected.");
            }
        }

        tokenReader.Read();

        if (failed) return null;
        return new ListLiteral(nodes);
    }
}