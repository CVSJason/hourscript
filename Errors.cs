namespace HourScript;

public static class Errors
{
    public static bool errorOccured { get; private set; } = false;

    public static void AddError<T>(T message)
    {
        errorOccured = true;

        ConsoleColor lastFg = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        
        Console.WriteLine(message);

        Console.ForegroundColor = lastFg;
    }
}