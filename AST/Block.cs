using System.Security.Cryptography.X509Certificates;
using HourScript.Executing;

namespace HourScript.AST;

public class Block: Node
{
    List<Node> nodes;
    Dictionary<string, Value> staticScope = new();

    public Block(List<Node> nodes)
    {
        this.nodes = nodes;
    }

    public override string ToString()
    {
        return Calc.Join(nodes, "\n");
    }

    public override Value Eval(Context context)
    {
        Scope oldScope = context.currentScope;
        context.currentScope = new Scope(oldScope, staticScope);

        Value result = VoidValue.value;

        foreach (Node node in nodes)
        {
            result = node.Eval(context);
        }

        context.currentScope = oldScope;

        return result;
    }

    public Value EvalAsGlobal(Context context)
    {
        Value result = VoidValue.value;

        foreach (Node node in nodes)
        {
            result = node.Eval(context);
        }

        return result;
    }
}