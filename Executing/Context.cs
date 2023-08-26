namespace HourScript.Executing;

using HourScript.Modules.FileIO;

public class Context
{
    public static Scope builtInScope = new();
    public Scope exportScope = new(builtInScope);
    public Scope globalScope;
    public Scope publicScope;
    public Scope currentScope;

    public static void InitBuiltIn()
    {
        builtInScope.AddVariable("print", new NativeCallable((ctxt, p) => {
            bool first = true;

            foreach (var val in p)
            {
                if (first) first = false;
                else Console.Write(" ");

                Console.Write(val);
            }
            
            return VoidValue.value;
        }, 0));

        builtInScope.AddVariable("println", new NativeCallable((ctxt, p) => {
            bool first = true;

            foreach (var val in p)
            {
                if (first) first = false;
                else Console.Write(" ");

                Console.Write(val);
            }
            Console.WriteLine();
            
            return VoidValue.value;
        }, 0));

        builtInScope.AddVariable("input", new NativeCallable((ctxt, p) => {
            bool first = true;

            foreach (var val in p)
            {
                if (first) first = false;
                else Console.Write(" ");

                Console.Write(val);
            }
            
            string value = Console.ReadLine() ?? "";
            
            return new StringValue(value);
        }, 0));

        builtInScope.AddVariable("open", new NativeCallable((ctxt, p) => {
            return new FileObject(p[0].ToString());
        }, 1));

        builtInScope.AddVariable("NaN", new DoubleValue(double.NaN));
        builtInScope.AddVariable("nil", NilValue.value);
        builtInScope.AddVariable("void", VoidValue.value);
    }

    public Context()
    {
        globalScope = new(exportScope);

        publicScope = new(globalScope);

        currentScope = publicScope;
    }
}

public class Scope
{
    public Scope? outerScope;

    readonly Dictionary<string, Value> names = new();
    readonly Dictionary<string, Value>? staticScope = new();

    public Scope() {}
    public Scope(Scope outerScope)
    {
        this.outerScope = outerScope;
    }
    public Scope(Scope outerScope, Dictionary<string, Value>? staticScpoe)
    {
        this.outerScope = outerScope;
        this.staticScope = staticScpoe;
    }

    public void AddVariable(string name, Value value)
    {
        if (names.ContainsKey(name))
        {
            throw new Exception("Repeat variable name");
        }

        names[name] = value;
    }

    public void AddStaticVariable(string name, Value value)
    {
        if (staticScope is null)
        {
            Errors.AddError("\nERR!  Can't define a static variable in global scope.");
            Environment.Exit(-1);
            throw new Exception();
        }

        if (staticScope.ContainsKey(name))
        {
            throw new Exception("Repeat variable name");
        }

        staticScope[name] = value;
    }

    public void SetVariable(string name, Value value)
    {
        if (!HasVariable(name) || names.ContainsKey(name))
        {
            names[name] = value;
        }
        else if (staticScope is not null && staticScope.ContainsKey(name))
        {
            staticScope[name] = value;
        }
        else 
        {
            outerScope!.SetVariable(name, value);
        }
    }

    public void SetStaticVariable(string name, Value value)
    {
        if (staticScope is not null && (!HasStaticVariable(name) || staticScope.ContainsKey(name)))
        {
            staticScope[name] = value;
        }
        else
        {
            outerScope!.SetStaticVariable(name, value);
        }
    }

    public bool HasVariable(string name)
    {
        return names.ContainsKey(name) || (staticScope?.ContainsKey(name) ?? false) || (outerScope?.HasVariable(name) ?? false);
    }

    public bool HasStaticVariable(string name)
    {
        return (staticScope?.ContainsKey(name) ?? false) || (outerScope?.HasStaticVariable(name) ?? false);
    }

    public bool HasVariableInThisScope(string name)
    {
        return names.ContainsKey(name);
    }

    public bool HasVariableInStaticScope(string name)
    {
        if (staticScope is null)
        {
            Errors.AddError("\nERR!  Can't define a static variable in global scope.");
            Environment.Exit(-1);
            throw new Exception();
        }

        return staticScope.ContainsKey(name);
    }

    public Value GetVariable(string name)
    {
        if (names.ContainsKey(name))
        {
            return names[name];
        }
        else if (staticScope is not null && staticScope.ContainsKey(name))
        {
            return staticScope[name];
        }
        else if (outerScope is not null)
        {
            return outerScope.GetVariable(name);
        }
        else 
        {
            return UndefinedValue.value;
        }
    }
}