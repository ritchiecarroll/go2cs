namespace PublishSymbolsLib;

public static class Where
{
    // This frame's file and line, as .NET resolves them from this assembly's portable .pdb: "Where.cs:<line>" when the
    // symbol file is beside the host, ":0" when it is not.
    public static string Frame()
    {
        System.Diagnostics.StackFrame frame = new System.Diagnostics.StackTrace(true).GetFrame(0);
        return $"{System.IO.Path.GetFileName(frame?.GetFileName() ?? "")}:{frame?.GetFileLineNumber() ?? 0}";
    }
}
