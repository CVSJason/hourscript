namespace HourScript.Lexing;

using System.Reflection.Emit;
using System.Text;

public class Lexer
{
    static readonly HashSet<string> keywords = new() 
    {
        "nil", "true", "false",
        "if", "then", "else", "for", "in", "while", "do", "end", "func",
        "to", "until", "downto", "step",
        "local",
        "import", "export", "static",
        "class",
        "continue", "break", "return"
    };

    static readonly string operatorChar = "~!%^&*()-+={}[]|;:<>,.?/";
    static readonly HashSet<string> operators2 = new() 
    {
        ">=", "<=", "==", "!=", "..", "**", "+=", "-=", "*=", "/=", "%="
    };
    static readonly HashSet<string> operators3 = new()
    {
        "===", "!==", "&&=", "||=", "**=", "..="
    };

    public static List<Token> Lex(string source, string fromFile)
    {
        int index = 0, line = 1, column = 1;
        List<Token> result = new();

        while (index < source.Length)
        {
            if (source[index] is '\n') 
            {
                line++;
                column = 1;
                index++;
            }
            else if (source[index] is '\t' or ' ')
            {
                column++; index++;
            }
            else if (source[index] is >= '0' and <= '9')
            {
                int startCol = column, startIndex = index;

                while (index < source.Length && source[index] is >= '0' and <= '9')
                {
                    index++;
                    column++;
                }

                result.Add(new(source.Substring(startIndex, index - startIndex), TokenType.num, line, startCol, fromFile));
            }
            else if (source[index] is '\'' or '"')
            {
                int startLine = line, startCol = column, startChar = source[index];
                StringBuilder sb = new();

                index++; column++;

                while (index < source.Length && source[index] != startChar)
                {
                    if (source[index] == '\\')
                    {
                        if (index + 1 < source.Length)
                        {
                            if (source[index + 1] is '\'' or '"')
                            {
                                sb.Append(source[index + 1]);
                                index += 2; column += 2;
                                continue;
                            }
                            else if (source[index + 1] == 't')
                            {
                                sb.Append('\t');
                                index += 2; column += 2; continue;
                            }
                            else if (source[index + 1] == 'n')
                            {
                                sb.Append('\n');
                                index += 2; column += 2; continue;
                            }
                        }
                    }
                    if (source[index] == '\n')
                    {
                        sb.Append('\n');
                        index++; line++; column = 1;
                        continue;
                    }

                    sb.Append(source[index]);
                    index++; column++;
                }

                index++; column++;

                result.Add(new(sb.ToString(), TokenType.str, startLine, startCol, fromFile));
            }
            else if (source[index] is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_')
            {
                int startCol = column, startIndex = index;

                index++; column++;

                while (index < source.Length && source[index] is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_')
                {
                    index++; column++;
                }

                string str = source[startIndex..index];

                if (keywords.Contains(str))
                {
                    result.Add(new(str, TokenType.keyword, line, startCol, fromFile));
                }
                else 
                {
                    result.Add(new(str, TokenType.id, line, startCol, fromFile));
                }
            }
            else if (operatorChar.Contains(source[index]))
            {
                if (index + 2 < source.Length && operators3.Contains(source.Substring(index, 3)))
                {
                    result.Add(new(source.Substring(index, 3), TokenType.keyword, line, column, fromFile));
                    index += 3; column += 3;
                }
                else if (index + 1 < source.Length && operators2.Contains(source.Substring(index, 2)))
                {
                    result.Add(new(source.Substring(index, 2), TokenType.keyword, line, column, fromFile));
                    index += 2; column += 2;
                }
                else 
                {
                    result.Add(new(source.Substring(index, 1), TokenType.keyword, line, column, fromFile));
                    index++; column++;
                }
            }
            else 
            {
                Errors.AddError($"ERR!  at {column}:{index} {fromFile}: Invalid character: {source[index]}");
                column++; index++;
            }
        }

        return result;
    }
}