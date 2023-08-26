namespace HourScript.Modules.FileIO;

using HourScript.Executing;

public class FileObject: Value
{
    FileStream? fileStream;
    StreamReader? streamReader;
    StreamWriter? streamWriter;

    public FileObject(string path)
    {   
        fileStream = new FileStream(Path.GetFullPath(path), FileMode.OpenOrCreate);
        streamReader = new StreamReader(fileStream);
        streamWriter = new StreamWriter(fileStream);
    }

    public override string ToString() => "File";
    public override bool ToCondition() => true;
    public override bool HasTheSameTypeOf(Value other) => other is FileObject;

    bool writed = false;

    public override Value GetProperty(string name)
    {
        return name switch {
            "truncate" => new NativeCallable((_, o, _) => {
                fileStream?.SetLength(0);

                return VoidValue.value;
            }, 0, this),
            "write" => new NativeCallable((_, o, a) => {
                foreach (var v in a)
                {
                    writed = true;
                    streamWriter?.Write(v.ToString());
                }

                return VoidValue.value;
            }, 0, this),
            "readLine" => new NativeCallable((_, o, a) => {
                if (streamReader is null)
                {
                    return NilValue.value;
                }
                else
                {
                    if (writed)
                    {
                        streamWriter!.Flush();
                        writed = false;
                    }

                    string? result = streamReader.ReadLine();
                    
                    return result is null ? NilValue.value : new StringValue(result);
                }
            }, 0, this),
            "readAll" => new NativeCallable((_, o, a) => {
                if (streamReader is null)
                {
                    return NilValue.value;
                }
                else
                {
                    if (writed)
                    {
                        streamWriter!.Flush();
                        writed = false;
                    }

                    string result = streamReader.ReadToEnd();
                    
                    return new StringValue(result);
                }
            }, 0, this),
            "readByte" => new NativeCallable((_, o, a) => {
                if (fileStream is null)
                {
                    return NilValue.value;
                }
                else
                {
                    if (writed)
                    {
                        streamWriter!.Flush();
                        writed = false;
                    }

                    return new DoubleValue(fileStream.ReadByte());
                }
            }, 0, this),
            "seek" => new NativeCallable((_, o, a) => {
                if (fileStream is not null)
                {   
                    if (writed)
                    {
                        streamWriter!.Flush();
                        writed = false;
                    }

                    int origin = a[0] is UndefinedValue ? 0 : (int)a[0].ToDouble();

                    fileStream.Seek((int)a[0].ToDouble(), origin switch {
                        0 => SeekOrigin.Begin,
                        1 => SeekOrigin.Current,
                        2 => SeekOrigin.End,
                        _ => SeekOrigin.Begin
                    });
                }

                return VoidValue.value;
            }, 2, this),
            "close" => new NativeCallable((_, o, a) => {
                if (writed)
                    {
                        streamWriter!.Flush();
                        writed = false;
                    }

                fileStream?.Close();
                
                fileStream = null;
                streamReader = null;
                streamWriter = null;

                return VoidValue.value;
            }, 0, this),
            "closed" => BooleanValue.Get(fileStream is null),
            _ => base.GetProperty(name)
        };  
    }
}