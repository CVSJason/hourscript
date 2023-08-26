using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Markup;
using HourScript.AST;

namespace HourScript.Executing;

public abstract class Value
{
    public override abstract string ToString();
    public virtual string ToDescription() => ToString();
    public virtual double ToDouble()
    {
        string asString = ToString();

        try
        {
            return double.Parse(asString);
        }
        catch
        {
            return double.NaN;
        }
    }
    public abstract bool ToCondition();
    public abstract bool HasTheSameTypeOf(Value other);

    public virtual Value GetProperty(string name)
    {
        return name switch
        {
            "toString" => new NativeCallable((_, o, _) => new StringValue(o!.ToString()), 0, this),
            _ => UndefinedValue.value,
        };
    }

    public virtual void SetProperty(string name, Value value)
    {
        Errors.AddError($"\nERR!  Can't set property {name}.");
        Environment.Exit(0);
        throw new Exception();
    }

    public static bool operator ==(Value left, Value right)
    {
        if (left is StringValue || right is StringValue)
        {
            return left.ToString() == right.ToString();
        }

        if (left is DoubleValue && right is DoubleValue)
        {
            return left.ToDouble() == right.ToDouble();
        }

        return left.Equals(right);
    }

    public static bool operator !=(Value left, Value right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return -1;
    }
}

public class DoubleValue: Value
{
    double value;

    public DoubleValue(double value)
    {
        this.value = value;
    }

    public override string ToString() => value.ToString();

    public override double ToDouble()
    {
        return value;
    }

    public override bool ToCondition() => value != 0;

    public override bool HasTheSameTypeOf(Value other) => other is DoubleValue;

    public override Value GetProperty(string name)
    {
        return name switch {
            _ => base.GetProperty(name)
        };
    }
}

class StringValue: Value
{
    string value;

    public StringValue(string value)
    {
        this.value = value;
    }

    public override string ToString() => value;
    public override string ToDescription()
    {
        string converted = value
            .Replace("\n", "\\n")
            .Replace("\t", "\\t")
            .Replace("\"", "\\\"");

        return $"\"{converted}\"";
    }

    public override bool ToCondition() => value.Length != 0;
    public override bool HasTheSameTypeOf(Value other) => other is StringValue;

    public override Value GetProperty(string name)
    {
        return name switch {
            "length" => new DoubleValue(value.Length),
            "`indexGet" => new NativeCallable(
                (_, o, a) => {
                    var _this = (StringValue)o!;
                    int? idx = Calc.DoubleToIndex(a[0].ToDouble(), _this.value.Length);
                    if (idx is null) return UndefinedValue.value;

                    return new DoubleValue(_this.value[idx.Value]);
                },
            2, this),  
            _ => base.GetProperty(name)
        };
    }
}

class UndefinedValue: Value
{
    private UndefinedValue() {}

    public static readonly UndefinedValue value = new();

    public override string ToString() => "undefined";
    public override double ToDouble() => double.NaN;
    public override bool ToCondition() => false;
    public override bool HasTheSameTypeOf(Value other) => other is UndefinedValue;
}

class VoidValue: Value
{
    private VoidValue() {}

    public static readonly VoidValue value = new();

    public override string ToString() => "void";
    public override double ToDouble() => double.NaN;
    public override bool ToCondition() => false;
    public override bool HasTheSameTypeOf(Value other) => other is VoidValue; 
}

class NilValue: Value
{
    private NilValue() {}

    public static readonly NilValue value = new();

    public override string ToString() => "nil";
    public override double ToDouble() => double.NaN;
    public override bool ToCondition() => false;
    public override bool HasTheSameTypeOf(Value other) => other is NilValue;
}

class BooleanValue: Value
{
    bool value;

    private BooleanValue(bool value)
    {
        this.value = value;
    }

    public static readonly BooleanValue valueTrue = new(true);
    public static readonly BooleanValue valueFalse = new(false);
    public static BooleanValue Get(bool value)
    {
        return value ? valueTrue : valueFalse;
    }

    public override string ToString() => value ? "true" : "false";
    public override double ToDouble() => value ? 1 : 0;
    public override bool ToCondition() => value;
    public override bool HasTheSameTypeOf(Value other) => other is BooleanValue;
}

public class ModuleValue: Value
{
    Scope scope;

    public ModuleValue(Scope scope)
    {
        this.scope = scope;
    }

    public override string ToString() => "module";
    public override bool ToCondition() => true;
    public override bool HasTheSameTypeOf(Value other) => ((object)this).Equals(other);

    public override Value GetProperty(string name)
    {
        if (scope.HasVariableInThisScope(name))
        {
            return scope.GetVariable(name);
        }

        return base.GetProperty(name);
    }
}

public class ClassValue: Callable
{
    ClassDefinition definition;
    ClassValue? baseClass;

    public ClassValue(ClassDefinition definition, ClassValue? baseClass)
    {
        this.definition = definition;
        this.baseClass = baseClass;
    }

    public override string ToString() => definition.name ?? "anomyous class";

    public override Value Invoke(Context context, params Value[] args)
    {
        return Invoke(context, true, args);
    }

    Value Invoke(Context context, bool toCallInit, params Value[] args)
    {
        ObjectValue? baseObj = null;

        if (baseClass is not null)
        {
            baseObj = (ObjectValue)baseClass.Invoke(context, false, args);
        }

        Scope objectScope = new(
            baseObj is not null ? baseObj.objectScope : context.currentScope, 
            definition.staticScope
        );
        Scope lastScope = context.currentScope;

        ObjectValue obj = new ObjectValue(objectScope, this, baseObj);

        context.currentScope = objectScope;

        objectScope.AddVariable("this", obj);

        if (baseObj is not null)
            objectScope.AddVariable("base", baseObj);

        definition.body.EvalAsGlobal(context);

        if (toCallInit &&
            context.currentScope.HasVariableInThisScope("init") &&
            context.currentScope.GetVariable("init") is Callable c)
        {
            c.Invoke(context, args);
        }

        context.currentScope = lastScope;

        return obj;
    } 
}

public class ListValue: Value
{
    List<Value> values;

    public ListValue(List<Value> values)
    {
        this.values = values;
    }

    public override string ToString()
    {
        return $"[{Calc.Join(Calc.Map(values, v => v.ToDescription()), ", ")}]";
    }
    public override bool ToCondition() => values.Count != 0;
    public override bool HasTheSameTypeOf(Value other) => other is ListValue;

    public override Value GetProperty(string name)
    {
        return name switch {
            "length" => new DoubleValue(values.Count),
            "push" => new NativeCallable(
                (_, o, a) => {
                    var _this = (ListValue)o!;

                    _this.values.AddRange(a);

                    return this;
                },
                1, this
            ),
            "pop" => new NativeCallable(
                (_, o, a) => {
                    var _this = (ListValue)o!;

                    int idx;

                    if (a.Length == 0)
                    {
                        idx = _this.values.Count - 1;
                    }
                    else
                    {
                        int? rawIdx = Calc.DoubleToIndex(a[0].ToDouble(), _this.values.Count);
                    
                        if (rawIdx is null) return UndefinedValue.value;

                        idx = rawIdx.Value;
                    }

                    Value result = _this.values[idx];

                    _this.values.RemoveAt(idx);

                    return result;
                },
                0, this
            ),
            "insert" => new NativeCallable(
                (_, o, a) => {
                    var _this = (ListValue)o!;

                    int idx;

                    if (a.Length == 0)
                    {
                        idx = _this.values.Count - 1;
                    }
                    else
                    {
                        int? rawIdx = Calc.DoubleToIndex(a[1].ToDouble(), _this.values.Count);
                    
                        if (rawIdx is null) return _this;

                        idx = rawIdx.Value;
                    }

                    _this.values.Insert(idx, a[0]);

                    return _this;
                },
                0, this
            ),
            "`indexGet" => new NativeCallable(
                (_, o, a) => {
                    var _this = (ListValue)o!;

                    int? idx = Calc.DoubleToIndex(a[0].ToDouble(), _this.values.Count);
                    if (idx == null) return UndefinedValue.value;

                    return _this.values[idx.Value];
                },
                1, this
            ),
            "`indexSet" => new NativeCallable(
                (_, o, a) => {
                    var _this = (ListValue)o!;

                    int? idx = Calc.DoubleToIndex(a[1].ToDouble(), _this.values.Count);
                    if (idx == null) return UndefinedValue.value;

                    _this.values[idx.Value] = a[0];

                    return VoidValue.value;
                },
                1, this
            ),
            _ => base.GetProperty(name)
        };
    }
}

public class ObjectValue: Value
{
    public readonly Scope objectScope;
    ObjectValue? baseObject;
    ClassValue ofClass;

    public ObjectValue(Scope objectScope, ClassValue ofClass, ObjectValue? baseObject)
    {
        this.objectScope = objectScope;
        this.ofClass = ofClass;
        this.baseObject = baseObject;
    }

    public override string ToString() => "object";
    public override bool ToCondition() => true;
    public override bool HasTheSameTypeOf(Value other)
    {
        return other is ObjectValue o && o.ofClass == ofClass;
    }

    public override Value GetProperty(string name)
    {
        if (objectScope.HasVariableInThisScope(name))
        {
            return objectScope.GetVariable(name);
        }
        if (baseObject is not null)
        {
            return baseObject.GetProperty(name);
        }

        return base.GetProperty(name);
    }

    public override void SetProperty(string name, Value value)
    {
        if (objectScope.HasVariableInThisScope(name))
        {
            objectScope.SetVariable(name, value);

            return;
        }
        if (baseObject is not null)
        {
            baseObject.SetProperty(name, value);

            return;
        }

        base.SetProperty(name, value);
    }
}