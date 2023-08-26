namespace HourScript.AST;
using HourScript.Executing;

public class ImportNode: Node
{
    string module;
    bool isIdentifier;

    public ImportNode(string module, bool isIdentifier)
    {
        this.module = module;
        this.isIdentifier = isIdentifier;
    }

    public override Value Eval(Context context)
    {
        if (context.currentScope != context.publicScope)
        {
            Console.WriteLine();
            Errors.AddError($"ERR!  An import statement must be in the top scope.");
            Environment.Exit(1);

            throw new Exception();
        }

        ModuleValue moduleVal = FileHandler.HandlePath(module);

        if (isIdentifier)
        {
            context.currentScope.AddVariable(module, moduleVal);
        }

        return moduleVal;
    }
}
