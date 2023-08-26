namespace HourScript.Executing;

public class BreakOperation: Exception
{

}

public class ContinueOperation: Exception
{

}

public class ReturnOperation: Exception
{
    public readonly Value? value;

    public ReturnOperation(Value? value)
    {
        this.value = value;
    }
}