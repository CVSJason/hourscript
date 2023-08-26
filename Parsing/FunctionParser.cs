using System.Data.Common;
using HourScript.AST;
using HourScript.Lexing;

namespace HourScript.Parsing;

public partial class Parser
{
    FunctionDefinition? ParseFunction()
    {
        tokenReader.Read();

        string? name = null;

        if (tokenReader.Peek().type == TokenType.id)
        {
            name = tokenReader.Read().content;
        }

        List<string> paramNames = new();

        if (tokenReader.Peek().Match(TokenType.keyword, "("))
        {
            tokenReader.Read();

            while (!tokenReader.Peek().Match(TokenType.keyword, ")"))
            {
                if (tokenReader.Peek().type == TokenType.id)
                {
                    paramNames.Add(tokenReader.Read().content);;
                }
                else
                {
                    Errors.AddError($"ERR!  at {tokenReader.Read().positionAsString}: Parameter name expected");
                }

                if (tokenReader.Peek().Match(TokenType.keyword, ","))
                {
                    tokenReader.Read();
                }
                else if (!tokenReader.Peek().Match(TokenType.keyword, ")"))
                {
                    Errors.AddError($"ERR!  at {tokenReader.Read().positionAsString}: A comma should be inserted between the parameters.");
                }
            }

            tokenReader.Read();
        }

        Block body = ParseBlock();

        if (!tokenReader.Peek().Match(TokenType.keyword, "end"))
        {
            Errors.AddError($"ERR!  at {tokenReader.Peek().positionAsString}: An end expected.");

            return null;
        }   

        tokenReader.Read();

        return new FunctionDefinition(name, paramNames, body);
    }
}