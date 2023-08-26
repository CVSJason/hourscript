namespace HourScript.AST;

using HourScript.Executing;

public class Name: Node, MayBeAssignableNode, DefinitionNode
{
    public readonly string target;
    bool toExport = false;
    bool isStatic = false;

    public Name(string target)
    {
        this.target = target;
    }

    public override string ToString()
    {
        return target;
    }

    public override Value Eval(Context context)
    {
        if (isStatic && !context.currentScope.HasVariableInStaticScope(target))
        {
            context.currentScope.AddStaticVariable(target, UndefinedValue.value);   
        }

        return (toExport ? context.exportScope : context.currentScope)
            .GetVariable(target);
    }

    public void Assign(Context context, Value value)
    {
        if (isStatic)
        {
            if (!context.currentScope.HasVariableInStaticScope(target))
                context.currentScope.SetStaticVariable(target, value);   
        }
        else
        {
            (toExport ? context.exportScope : context.currentScope)
                .SetVariable(target, value);
        }
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

public class LocalVariableNode: Node, MayBeAssignableNode, DefinitionNode
{
    public string target;
    public bool isStatic = false;

    public LocalVariableNode(string target)
    {
        this.target = target;
    }

    public override Value Eval(Context context)
    {
        if (isStatic)
        {
            if (!context.currentScope.HasVariableInStaticScope(target))
            {
                context.currentScope.AddStaticVariable(target, UndefinedValue.value);   
            }
            
        }
        else
        {
            if (!context.currentScope.HasVariableInThisScope(target))
            {
                context.currentScope.AddVariable(target, UndefinedValue.value);
            } 
        }
        

        return context.currentScope.GetVariable(target);
    }

    public void Assign(Context context, Value value)
    {
        if (isStatic)
        {
            if (!context.currentScope.HasVariableInStaticScope(target))
                context.currentScope.AddStaticVariable(target, UndefinedValue.value);   
        }
        else
        {
            if (!context.currentScope.HasVariableInThisScope(target))
            {
                context.currentScope.AddVariable(target, value);
            } 
            else
            {
                context.currentScope.SetVariable(target, value);
            }
        }
    }

    public void SetToExport()
    {
        Errors.AddError("ERR!  A local variable can't be exported");
        Environment.Exit(-1);
    }

    public void SetStatic()
    {
        isStatic = true;
    }
}