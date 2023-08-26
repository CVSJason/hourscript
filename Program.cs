using HourScript;
using HourScript.Executing;

if (args.Length != 1)
{
    Console.WriteLine("Hourscript v0.0.0");
    Console.WriteLine("Usage: hourscript <entry file>");
    Console.WriteLine();

    return -1;
}

Context.InitBuiltIn();

FileHandler.HandlePath(args[0]);

return 0;