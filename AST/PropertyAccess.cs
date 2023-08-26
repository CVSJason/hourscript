namespace HourScript.AST;

using HourScript.Executing;

public class PropertyAccessNode: Node, MayBeAssignableNode
{
    Node target;
    string property;

    public PropertyAccessNode(Node target, string property)
    {
        this.target = target;
        this.property = property;
    }

    public override Value Eval(Context context)
    {
        return target.Eval(context).GetProperty(property);
    }

    public void Assign(Context context, Value value)
    {
        target.Eval(context).SetProperty(property, value);
    }
}