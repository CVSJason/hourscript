namespace HourScript.Parsing;

using HourScript.Lexing;

public class TokenReader
{
    List<Token> tokens;
    int nextToken = 0;
    string fromFile;

    public bool hasNext => nextToken < tokens.Count;

    public TokenReader(List<Token> tokens, string fromFile)
    {
        this.tokens = tokens;
        this.fromFile = fromFile;
    }

    public Token Read()
    {
        if (nextToken >= tokens.Count)
        {
            return new Token("", TokenType.eof, -1, -1, fromFile);
        }
        else
        {
            return tokens[nextToken++];
        }
    }

    public Token Peek(int k = 1)
    {
        int idx = nextToken + k - 1;

        if (idx >= tokens.Count)
        {
            return new Token("", TokenType.eof, -1, -1, fromFile);
        }
        else
        {
            return tokens[idx];
        }
    }
}