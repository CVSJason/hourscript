namespace HourScript;
using HourScript.Executing;
using HourScript.Lexing;
using HourScript.Parsing;
using HourScript.AST;
using System.IO;

public static class FileHandler
{
    static Dictionary<string, ModuleValue> handledFiles = new();
    static List<string> runningFiles = new();

    public static ModuleValue HandlePath(string path)
    {
        if (Path.HasExtension(path) && Path.GetExtension(path) != ".hour")
        {
            Errors.AddError($"ERR!  import error: File '{path}' is not supported.");
        }

        string lastWorkingDir = Directory.GetCurrentDirectory();

        string fullPath = Path.GetFullPath(path);
        string sourceFileName = Path.ChangeExtension(fullPath, ".hour");

        bool fileExists = File.Exists(sourceFileName);
        bool directoryExists = Directory.Exists(fullPath);

        ModuleValue result;
        
        if (fileExists && directoryExists)
        {
            Errors.AddError($"ERR!  import error: {path} is ambigous between '{path}.hour' and '{path}/'");

            Environment.Exit(-1);

            throw new Exception();
        }
        else if (fileExists)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(fullPath)!);

            result = HandleFile(sourceFileName);
        }
        else if (directoryExists)
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.Join(fullPath, "main.hour"))!);

            if (!File.Exists("main.hour"))
            {
                Errors.AddError($"ERR!  import error: Module '{path}' must contain a file named 'main.hour'");

                Environment.Exit(-1);

                throw new Exception();
            }

            result = HandleFile("main.hour");
        }
        else
        {
            Errors.AddError($"ERR!  import error: Module '{path}' does not exist.");

            Environment.Exit(-1);

            throw new Exception();
        }

        Directory.SetCurrentDirectory(lastWorkingDir);

        return result;
    }

    static ModuleValue HandleFile(string sourceFileName)
    {
        string fullPath = Path.GetFullPath(sourceFileName);

        if (handledFiles.ContainsKey(fullPath))
        {
            return handledFiles[fullPath];
        }
        else if (runningFiles.Contains(fullPath))
        {
            Errors.AddError($"ERR!  import error: '{fullPath}' depends on itself");

            Environment.Exit(-1);
            
            throw new Exception();
        }
        else 
        {
            runningFiles.Add(fullPath);

            string source;

            using (var file = new FileStream(fullPath, FileMode.Open))
            {
                source = new StreamReader(file).ReadToEnd();
            }

            List<Token> tokens = Lexer.Lex(source, fullPath);
            Block ast = new Parser(tokens, fullPath).Parse();
            Context context = new();

            if (!Errors.errorOccured) 
                try 
                {
                    ast.EvalAsGlobal(context);
                }
                catch (BreakOperation)
                {
                    Errors.AddError("ERR!  Break statement without matching loop");
                    Environment.Exit(-1);
                }
                catch (ContinueOperation)
                {
                    Errors.AddError("ERR!  Continue statement without matching loop");
                    Environment.Exit(-1);
                }
                catch (ReturnOperation)
                {
                    Errors.AddError("ERR!  Return statement should not be in the top scope");
                    Environment.Exit(-1);
                }

            ModuleValue ns = new(context.exportScope);

            runningFiles.RemoveAt(runningFiles.Count - 1);

            return ns;
        }
    }
}