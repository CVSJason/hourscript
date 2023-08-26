using HourScript.Executing;

namespace HourScript.AST;

public class FunctionDefinition: Node, DefinitionNode
{
    string? name;
    bool toExport;
    bool isStatic;
    public readonly List<string> parameterNames;
    public readonly Block body;
    public readonly Dictionary<string, Value> staticScope = new();

    public FunctionDefinition(string? name, List<string> parameterNames, Block body)
    {
        this.name = name;
        this.parameterNames = parameterNames;
        this.body = body;
    }

    public override Value Eval(Context context)
    {
        Callable callable = new HourFunctionCallable(this, context.currentScope);

        if (name is not null)
        {
            if (isStatic)
            {
                context.currentScope.AddStaticVariable(name, callable);
            }
            else
            {
                (toExport ? context.exportScope : context.currentScope).AddVariable(name, callable);
            }
        }

        return callable;
    }

    public void SetToExport()
    {
        toExport = true;
    }

    public void SetStatic()
    {
        isStatic = true;
    }
}