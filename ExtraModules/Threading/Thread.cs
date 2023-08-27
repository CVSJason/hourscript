using System.Security.Cryptography.X509Certificates;
using HourScript.Executing;

namespace HourScript.Modules.Threading;

public class ThreadClass: Callable
{
    public override string ToString() => "Thread";
    public override bool ToCondition() => true;
    public override bool HasTheSameTypeOf(Value other) => other is ThreadClass;

    public override Value GetProperty(string name)
    {
        return name switch {
            "current" => new ThreadObject(Thread.CurrentThread),
            _ => base.GetProperty(name)
        };
    }

    public override Value Invoke(Context context, params Value[] args)
    {
        if (args.Length == 0 || args[0] is not Callable c)
        {
            Errors.AddError($"ERR!  A callable required to create a thread.");
            Environment.Exit(-1);
            throw new Exception();
        }

        return new ThreadObject(new Thread(() => {
            Context ctx = new()
            {
                globalScope = context.globalScope,
                exportScope = context.exportScope,
                publicScope = context.publicScope,
                currentScope = context.currentScope
            };

            c.Invoke(ctx);
        }));
    }
}

public class ThreadObject: Value
{
    Thread thread;

    public ThreadObject(Thread thread)
    {
        this.thread = thread;
    }

    public override bool Equals(object? obj)
    {
        return obj is ThreadObject t && t.thread == thread;
    }

    public override int GetHashCode()
    {
        return thread.GetHashCode();
    }

    public override Value GetProperty(string name)
    {
        return name switch {
            "start" => new NativeCallable((_, a) => {
                thread.Start();
                return this;
            }, 0),
            "join" => new NativeCallable((_, a) => {
                thread.Join();
                return this;
            }, 0),
            //"state" => new DoubleValue((int)thread.ThreadState),
            _ => base.GetProperty(name)
        };
    }

    public override string ToString() => "Thread";
    public override bool ToCondition() => true;
    public override bool HasTheSameTypeOf(Value other) => other is ThreadObject;
}