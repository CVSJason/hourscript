using HourScript.AST;

namespace HourScript.Executing;

public abstract class Callable: Value
{
    public abstract Value Invoke(Context context, params Value[] args);

    public override string ToString() => "function";
    public override bool ToCondition() => true;
    public override bool HasTheSameTypeOf(Value other) => other is Callable;
}

public class HourFunctionCallable: Callable
{
    FunctionDefinition definition;
    Scope definitionScope;

    public HourFunctionCallable(FunctionDefinition definition, Scope definitionScope)
    {
        this.definition = definition;
        this.definitionScope = definitionScope;
    }

    public override Value Invoke(Context context, params Value[] args)
    {
        Scope lastScope = context.currentScope;
        
        Scope parameterScope = new(definitionScope, definition.staticScope);

        for (int i = 0; i < definition.parameterNames.Count; i++)
        {
            if (i < args.Length)
            {
                parameterScope.AddVariable(definition.parameterNames[i], args[i]);
            }
            else
            {
                parameterScope.AddVariable(definition.parameterNames[i], UndefinedValue.value);
            }
        }

        context.currentScope = parameterScope;

        Value result = VoidValue.value;

        try
        {
            definition.body.Eval(context);
        }
        catch (ReturnOperation ret)
        {
            result = ret.value ?? VoidValue.value;
        }
        
        context.currentScope = lastScope;

        return result;
    }
}

public class NativeCallable: Callable
{
    public delegate Value NativeTarget(Context context, Value? thisValue, params Value[] parameters);
    public delegate Value NativeTargetNoThis(Context context, params Value[] parameters);

    NativeTarget target;
    int minimumParams;
    Value? thisValue;

    public NativeCallable(NativeTarget target, int minimumParams, Value? thisValue = null)
    {
        this.target = target;
        this.minimumParams = minimumParams;
        this.thisValue = thisValue;
    }

    public NativeCallable(NativeTargetNoThis target, int minimumParams)
    {
        this.target = (ctxt, _, args) => target(ctxt, args);
        this.minimumParams = minimumParams;
    }

    public override Value Invoke(Context context, params Value[] args)
    {
        List<Value> _args = new(args);

        while (_args.Count < minimumParams)
        {
            _args.Add(UndefinedValue.value);
        }

        Value result = target(context, thisValue, _args.ToArray());

        return result;
    }

    public override string ToString() => "native function";
}