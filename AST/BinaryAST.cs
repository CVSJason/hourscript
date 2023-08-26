using HourScript.Executing;

namespace HourScript.AST;

public class BinaryAST: Node
{
    Node lhs, rhs;
    string oper;

    public BinaryAST(Node lhs, string oper, Node rhs)
    {
        this.lhs = lhs;
        this.oper = oper;
        this.rhs = rhs;
    }

    public override string ToString()
    {
        return $"({lhs} {oper} {rhs})";
    }

    public override Value Eval(Context context)
    {
        Value EvalCommon(string oper, Node lhs, Node rhs)
        {
            switch (oper)
            {
                case "+": return new DoubleValue(lhs.Eval(context).ToDouble() + rhs.Eval(context).ToDouble());
                case "-": return new DoubleValue(lhs.Eval(context).ToDouble() - rhs.Eval(context).ToDouble());
                case "*": return new DoubleValue(lhs.Eval(context).ToDouble() * rhs.Eval(context).ToDouble());
                case "/": return new DoubleValue(lhs.Eval(context).ToDouble() / rhs.Eval(context).ToDouble());
                case "%": return new DoubleValue(lhs.Eval(context).ToDouble() % rhs.Eval(context).ToDouble());
                case "**": return new DoubleValue(Math.Pow(lhs.Eval(context).ToDouble(), rhs.Eval(context).ToDouble()));
                case "..": return new StringValue(lhs.Eval(context).ToString() + rhs.Eval(context).ToString());
                case "&&": return BooleanValue.Get(lhs.Eval(context).ToCondition() ? rhs.Eval(context).ToCondition() : false);
                case "||": return BooleanValue.Get(lhs.Eval(context).ToCondition() ? true : rhs.Eval(context).ToCondition());
                case ">": return BooleanValue.Get(lhs.Eval(context).ToDouble() > rhs.Eval(context).ToDouble());
                case "<": return BooleanValue.Get(lhs.Eval(context).ToDouble() < rhs.Eval(context).ToDouble());
                case ">=": return BooleanValue.Get(lhs.Eval(context).ToDouble() >= rhs.Eval(context).ToDouble());
                case "<=": return BooleanValue.Get(lhs.Eval(context).ToDouble() <= rhs.Eval(context).ToDouble());
                case "==": return BooleanValue.Get(lhs.Eval(context) == rhs.Eval(context));
                case "!=": return BooleanValue.Get(lhs.Eval(context) != rhs.Eval(context));
                case "===": {
                    Value l = lhs.Eval(context), r = rhs.Eval(context);

                    return BooleanValue.Get(l.HasTheSameTypeOf(r) && l == r);
                }
                case "!==": {
                    Value l = lhs.Eval(context), r = rhs.Eval(context);

                    return BooleanValue.Get(!l.HasTheSameTypeOf(r) || l != r);
                }
            }

            throw new Exception("Operator Not Supported");
        }

        if (oper == "=")
        {
            if (lhs is MayBeAssignableNode a)
            {
                Value value = rhs.Eval(context);

                a.Assign(context, value);

                return value;
            }
            else
            {
                throw new Exception("Cannot assign to a common expression.");
            }
        }
        else if (oper.EndsWith("=") && !(oper is "==" or "!=" or "===" or "!==" or ">=" or "<="))
        {
            string _oper = oper[..^1];

            if (lhs is Name name)
            {
                Value value = EvalCommon(_oper, rhs, lhs);

                context.currentScope.SetVariable(name.target, value);

                return value;
            }
            else
            {
                throw new Exception("Cannot assign to a common expression.");
            }
        }

        return EvalCommon(oper, lhs, rhs);
    }
}