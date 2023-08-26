namespace HourScript.AST;

using HourScript.Executing;

public class ClassDefinition: Node, DefinitionNode
{
    public readonly string? name;
    public readonly Block body;
    public readonly Node? baseClass;
    public readonly Dictionary<string, Value> staticScope = new();
    bool toExport;

    public ClassDefinition(string? name, Block body, Node? baseClass)
    {
        this.name = name;
        this.body = body;
        this.baseClass = baseClass;
    }

    public override Value Eval(Context context)
    {
        ClassValue? baseClass = null;

        if (this.baseClass is not null)
        {
            Value baseClassVal = this.baseClass.Eval(context);

            if (baseClassVal is ClassValue c)
            {
                baseClass = c;
            }
            else
            {
                Errors.AddError($"ERR!  Can't derive from an object that is not a class.");
                Environment.Exit(-1);
                throw new Exception();
            }
        }

        ClassValue val = new(this, baseClass);

        if (name is not null)
        {
            if (toExport)
            {
                context.exportScope.AddVariable(name, val);
            }
            else
            {
                context.currentScope.AddStaticVariable(name, val);
            }
        }

        return val;
    }

    public void SetStatic()
    {
        Errors.AddError("Extra 'static': Class definitions are all static");
    }

    public void SetToExport()
    {
        toExport = true;
    }
}