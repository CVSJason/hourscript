namespace HourScript.Parsing;

using HourScript.AST;
using HourScript.Lexing;

public partial class Parser
{
    IfNode? ParseIf()
    {
        tokenReader.Read();

        Node? condition = ParseExpression();
        if (condition == null) return null;

        if (!tokenReader.Peek().Match(TokenType.keyword, "then"))
        {
            Token tok = tokenReader.Peek();
            Errors.AddError($"ERR!  at {tok.positionAsString}: A then expected.");

            return null;
        }

        tokenReader.Read();

        Block branchTrue = ParseBlock();
        Node? branchFalse = null;

        if (tokenReader.Peek().Match(TokenType.keyword, "else"))
        {
            tokenReader.Read();

            if (tokenReader.Peek().Match(TokenType.keyword, "if"))
            {
                branchFalse = ParseIf();

                if (branchFalse == null) return null;

                return new IfNode(condition, branchTrue, branchFalse);
            }
            else 
            {
                branchFalse = ParseBlock();
            }
        }

        if (!tokenReader.Peek().Match(TokenType.keyword, "end"))
        {
            Errors.AddError($"ERR!  at {tokenReader.Peek().positionAsString}: An end expected.");

            return null;
        }

        tokenReader.Read();

        return new IfNode(condition, branchTrue, branchFalse);
    }

    WhileNode? ParseWhile()
    {
        tokenReader.Read();

        Node? condition = ParseExpression();

        if (!tokenReader.Peek().Match(TokenType.keyword, "do"))
        {
            Errors.AddError($"ERR!  at {tokenReader.Peek().positionAsString}: A do expected.");

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

        if (condition == null) return null;

        return new WhileNode(condition, block);
    }

    Block? ParseDo()
    {
        tokenReader.Read();

        Block block = ParseBlock();

        if (!tokenReader.Peek().Match(TokenType.keyword, "end"))
        {
            Errors.AddError($"ERR!  at {tokenReader.Peek().positionAsString}: An end expected.");

            return null;
        }   

        tokenReader.Read();

        return block;
    }
}