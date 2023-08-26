using HourScript.Executing;

namespace HourScript.AST;

public class InvokeAST: Node
{
    Node target;
    List<Node> arguments;

    public InvokeAST(Node target, List<Node> arguments)
    {
        this.target = target;
        this.arguments = arguments;
    }

    public override Value Eval(Context context)
    {
        Value v = target.Eval(context);

        if (v is Callable c)
        {
            return c.Invoke(context, Calc.Map(arguments, a => a.Eval(context)).ToArray());
        }
        else
        {
            throw new Exception("Object is not callable");
        }
    }
}

public class IndexNode: Node, MayBeAssignableNode
{
    Node target;
    List<Node> indices;

    public IndexNode(Node target, List<Node> indices)
    {
        this.target = target;
        this.indices = indices;
    }

    public override Value Eval(Context context)
    {
        var getter = target.Eval(context).GetProperty("`indexGet");

        if (getter is Callable c)
        {
            return c.Invoke(context, Calc.Map(indices, a => a.Eval(context)).ToArray());
        }
        else
        {
            Errors.AddError($"\n ERR!  Can't index the object.");
            Environment.Exit(-1);
            throw new Exception();
        }
    }

    public void Assign(Context context, Value v)
    {
        var setter = target.Eval(context).GetProperty("`indexSet");

        if (setter is Callable c)
        {
            var args = Calc.Map(indices, a => a.Eval(context));

            args.Insert(0, v);

            c.Invoke(context, args.ToArray());
        }
        else
        {
            Errors.AddError($"\n ERR!  Can't set the indexing result of the object.");
            Environment.Exit(-1);
            throw new Exception();
        }
    }
}