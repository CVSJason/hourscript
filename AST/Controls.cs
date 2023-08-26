using HourScript.Executing;

namespace HourScript.AST;

public class IfNode: Node
{
    Node condition, branchTrue;
    Node? branchFalse;

    public IfNode(Node condition, Node branchTrue, Node? branchFalse)
    {
        this.condition = condition;
        this.branchTrue = branchTrue;
        this.branchFalse = branchFalse;
    }

    public override Value Eval(Context context)
    {
        if (condition.Eval(context).ToCondition())
        {
            return branchTrue.Eval(context);
        }
        else if (branchFalse is null)
        {
            return VoidValue.value;
        }
        else
        {
            return branchFalse.Eval(context);
        }
    }
}

public class WhileNode: Node
{
    Node condition, body;

    public WhileNode(Node condition, Node body)
    {
        this.condition = condition;
        this.body = body;  
    }

    public override Value Eval(Context context)
    {
        while (condition.Eval(context).ToCondition())
        {
            try 
            {
                body.Eval(context);
            }
            catch (BreakOperation)
            {
                break;
            }
            catch (ContinueOperation)
            {
                continue;
            }
        }

        return VoidValue.value;
    }
}

public class BreakNode: Node
{
    public override Value Eval(Context context)
    {
        throw new BreakOperation();
    }
}

public class ContinueNode: Node
{
    public override Value Eval(Context context)
    {
        throw new ContinueOperation();
    }
}

public class ReturnNode: Node
{
    Node? returnValue;

    public ReturnNode(Node? returnValue)
    {
        this.returnValue = returnValue;
    }

    public override Value Eval(Context context)
    {
        if (returnValue is null)
        {
            throw new ReturnOperation(null);
        }
        else
        {
            throw new ReturnOperation(returnValue.Eval(context));
        }
    }
}