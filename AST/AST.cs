namespace HourScript.AST;
using HourScript.Executing;

public abstract class Node
{
    public abstract Value Eval(Context context);
}

public interface MayBeAssignableNode
{
    public void Assign(Context context, Value val);
}

public interface DefinitionNode
{
    public void SetToExport();
    public void SetStatic();
}