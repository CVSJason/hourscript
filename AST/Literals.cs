using HourScript.Executing;

namespace HourScript.AST;

public class NumberLiteral: Node
{
    string value;

    public NumberLiteral(string value)
    {
        this.value = value;
    }

    public override string ToString()
    {
        return value;
    }

    public override Value Eval(Context context)
    {
        try
        {
            return new DoubleValue(double.Parse(value));
        }
        catch
        {
            return new DoubleValue(double.NaN);
        }
    }
}

public class StringLiteral: Node
{
    string value;

    public StringLiteral(string value)
    {
        this.value = value;
    }

    public override string ToString()
    {
        string converted = value
            .Replace("\n", "\\n")
            .Replace("\t", "\\t")
            .Replace("\"", "\\\"");

        return $"{converted}";
    }

    public override Value Eval(Context context)
    {
        return new StringValue(value);
    }
}

public class BooleanLiteral: Node
{
    bool value;

    public BooleanLiteral(bool value)
    {
        this.value = value;
    }

    public override Value Eval(Context context)
    {
        return BooleanValue.Get(value);
    }
}

public class ListLiteral: Node
{
    public readonly List<Node> elements;

    public ListLiteral(List<Node> elements)
    {
        this.elements = elements;
    }

    public override Value Eval(Context context)
    {
        return new ListValue(Calc.Map(elements, e => e.Eval(context)));
    }
}