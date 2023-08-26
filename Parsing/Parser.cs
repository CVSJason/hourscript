using HourScript.Lexing;
using HourScript.AST;
using HourScript.Executing;

namespace HourScript.Parsing;

public partial class Parser
{
    readonly TokenReader tokenReader;

    public Parser(List<Token> tokens, string fromFile)
    {
        tokenReader = new(tokens, fromFile);
    }

    public Block Parse()
    {
        return ParseBlock(readAll: true);
    }

    Block ParseBlock(bool readAll = false)
    {
        List<Node> expressions = new();

        while (tokenReader.hasNext && (readAll || !tokenReader.Peek().Match(TokenType.keyword, "end", "else")))
        {
            Node? expr = ParseSingleExpression();

            if (expr is not null)
            {
                expressions.Add(expr);
            }

            if (!tokenReader.Peek().Match(TokenType.keyword, ";"))
            {
                Token nextToken = tokenReader.Peek();

                Errors.AddError($"ERR!  at {nextToken.positionAsString}: A ';' expected.");
            }
            else
            {
                tokenReader.Read();
            }  
        }

        return new(expressions);
    }

    string[][] operatorPrecedence = new string[][] {
        new string[] { "=", "+=", "-=", "*=", "/=", "%=", "&&=", "||=", "**=", "..=" },
        new string[] { "==", "!=", "===", "!==" },
        new string[] { ">", "<", ">=", "<=" },
        new string[] { "+", "-" },
        new string[] { "*", "/" },
        new string[] { ".." },
        new string[] { "**" },
    };

    Node? ParseSingleExpression()
    {
        if (tokenReader.Peek().Match(TokenType.keyword, ";"))
        {
            return null;
        }

        return ParseExpression();
    }

    Node? ParseExpression(int precedenceLvl = 0)
    {
        if (precedenceLvl >= operatorPrecedence.Length) return ParsePrefix();

        Node? a = ParseExpression(precedenceLvl + 1);

        while (true)
        {
            Token operToken = tokenReader.Peek();

            if (operToken.type != TokenType.keyword || !operatorPrecedence[precedenceLvl].Contains(operToken.content))
            {
                return a;
            }

            tokenReader.Read();

            Node? b = ParseExpression(precedenceLvl == 0 ? 0 : precedenceLvl + 1);

            a = (a is null || b is null) ? null : new BinaryAST(a, operToken.content, b);

            if (precedenceLvl == 0) return a;
        }
    }

    Node? ParsePrefix()
    {
        if (tokenReader.Peek().Match(TokenType.keyword, "local"))
        {
            tokenReader.Read();

            Node? name = ParsePrefix();

            if (name is null) return null;

            if (name is Name n)
            {
                return new LocalVariableNode(n.target);
            }
            else 
            {
                Errors.AddError($"ERR!  {name} is not a valid variable name.");

                return name;
            }
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "export"))
        {
            tokenReader.Read();

            Node? n = ParseSuffix();

            if (n is null) return null;

            if (n is DefinitionNode d)
            {
                d.SetToExport();
            }
            else
            {
                Errors.AddError($"ERR!  Can't export a common expression.");
            }

            return n;
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "static"))
        {
            tokenReader.Read();

            Node? n = ParseSuffix();

            if (n is null) return null;

            if (n is DefinitionNode d)
            {
                d.SetStatic();
            }
            else
            {
                Errors.AddError($"ERR!  Can't set a common expression to be static.");
            }

            return n;
        }

        return ParseSuffix();
    }

    Node? ParseSuffix()
    {
        Node? n = ParsePrimary();

        while (true)
        {
            if (tokenReader.Peek().Match(TokenType.keyword, "("))
            {
                tokenReader.Read();

                List<Node> args = new();
                bool failed = false;

                while (tokenReader.hasNext && !tokenReader.Peek().Match(TokenType.keyword, ")"))
                {
                    Node? arg = ParseExpression();

                    if (arg is null) failed = true;
                    else args.Add(arg);

                    if (tokenReader.Peek().Match(TokenType.keyword, ","))
                    {
                        tokenReader.Read();
                    }
                    else if (!tokenReader.Peek().Match(TokenType.keyword, ")"))
                    {
                        Errors.AddError($"ERR!  at {tokenReader.Read().positionAsString}: A comma should be inserted between the arguments.");
                    }
                }

                tokenReader.Read();

                if (failed)
                {
                    n = null;
                }
                else if (n is not null)
                {
                    n = new InvokeAST(n, args);
                }

                continue;
            }
            else if (tokenReader.Peek().Match(TokenType.keyword, "."))
            {
                tokenReader.Read();

                Token tok = tokenReader.Read();

                if (n is not null) n = new PropertyAccessNode(n, tok.content);

                continue;
            }
            else if (tokenReader.Peek().Match(TokenType.keyword, "["))
            {
                ListLiteral? indices = ParseList();

                if (indices == null)
                {
                    n = null;
                }
                else if (n is not null)
                {
                    n = new IndexNode(n, indices.elements);
                }

                continue;
            }
            break;
        }

        return n;
    }

    Node? ParsePrimary()
    {
        if (tokenReader.Peek().type == TokenType.num)
        {
            return new NumberLiteral(tokenReader.Read().content);
        }
        else if (tokenReader.Peek().type == TokenType.str)
        {
            return new StringLiteral(tokenReader.Read().content);
        }
        else if (tokenReader.Peek().type == TokenType.id)
        {
            return new Name(tokenReader.Read().content);
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "true"))
        {
            tokenReader.Read();
            return new BooleanLiteral(true);
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "false"))
        {
            tokenReader.Read();
            return new BooleanLiteral(false);
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "continue"))
        {
            tokenReader.Read();
            return new ContinueNode();
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "break"))
        {
            tokenReader.Read();
            return new BreakNode();
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "return"))
        {
            tokenReader.Read();
            
            if (tokenReader.Peek().Match(TokenType.keyword, ";"))
            {
                return new ReturnNode(null);
            }
            else
            {
                return new ReturnNode(ParseExpression());
            }
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "if"))
        {
            return ParseIf();
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "while"))
        {
            return ParseWhile();
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "do"))
        {
            return ParseDo();
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "func"))
        {
            return ParseFunction();
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "class"))
        {
            return ParseClass();
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "import"))
        {
            tokenReader.Read();

            Token tok = tokenReader.Read();

            return new ImportNode(tok.content, tok.type == TokenType.id);
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "("))
        {
            tokenReader.Read();

            Node? n = ParseExpression();

            tokenReader.Read();

            return n;
        }
        else if (tokenReader.Peek().Match(TokenType.keyword, "["))
        {
            return ParseList();
        } 

        Errors.AddError($"ERR!  at {tokenReader.Read().positionAsString}: Unexpected token");
        return null;
    }
}