using HourScript.AST;
using HourScript.Lexing;

namespace HourScript.Parsing;

public partial class Parser
{
    ClassDefinition? ParseClass()
    {
        tokenReader.Read();

        string? name = null;

        if (tokenReader.Peek().type == TokenType.id)
        {
            name = tokenReader.Read().content;
        }

        Node? baseClass = null;

        if (tokenReader.Peek().Match(TokenType.keyword, ":"))
        {
            tokenReader.Read();

            baseClass = ParseExpression();
        }

        if (!tokenReader.Peek().Match(TokenType.keyword, "in"))
        {
            Errors.AddError($"ERR!  at {tokenReader.Peek().positionAsString}: An in expected.");

            return null;
        }

        tokenReader.Read();

        Block block = ParseBlock();

        if (!tokenReader.Peek().Match(TokenType.keyword, "end"))
        {
            Errors.AddError($"ERR!  at {tokenReader.Peek().positionAsString}: An end expected.");

            return null;
        }

        tokenReader.Read();

        return new ClassDefinition(name, block, baseClass);
    }
}