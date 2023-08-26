namespace HourScript.Lexing;

public enum TokenType
{
    num, str, id, keyword, eol, eof
}

public class Token
{
    public readonly string content;
    public readonly TokenType type;
    public readonly int line;
    public readonly int column;
    public readonly string fromFile;

    public string positionAsString {
        get {
            if (line == -1 && column == -1)
            {
                return $"the end of {fromFile}";
            }
            else
            {
                return $"{fromFile}:{line}:{column}";
            }
        }
    }

    public Token(string content, TokenType type, int line, int column, string fromFile)
    {
        this.content = content;
        this.type = type;
        this.line = line;
        this.column = column;
        this.fromFile = fromFile;
    }

    public bool Match(TokenType type, string content)
    {
        return this.type == type && this.content == content;
    }

    public bool Match(TokenType type, params string[] contents)
    {
        if (this.type != type) return false;

        foreach (string content in contents)
        {
            if (this.content == content) return true;
        }

        return false;
    }
}