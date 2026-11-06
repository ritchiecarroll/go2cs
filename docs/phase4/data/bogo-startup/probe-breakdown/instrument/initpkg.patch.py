# Scratch instrument (never pushed): time builtin.initPackage per package, inclusive, with nesting depth.
import sys
p=sys.argv[1]; s=open(p,newline='').read()
old="""    public static void initPackage(Type packageType)
    {
        RuntimeHelpers.RunModuleConstructor(packageType.Module.ModuleHandle);
    }"""
nl='\r\n' if '\r\n' in s else '\n'
old=old.replace('\n',nl)
assert s.count(old)==1
new="""    public static void initPackage(Type packageType)
    {
        if (P1InitTrace.Out is null) { RuntimeHelpers.RunModuleConstructor(packageType.Module.ModuleHandle); return; }
        long t0 = System.Diagnostics.Stopwatch.GetTimestamp();
        int d = P1InitTrace.Depth++;
        try { RuntimeHelpers.RunModuleConstructor(packageType.Module.ModuleHandle); }
        finally { P1InitTrace.Depth--; P1InitTrace.Add(d, packageType.Module.Assembly.GetName().Name, t0, System.Diagnostics.Stopwatch.GetTimestamp()); }
    }"""
s=s.replace(old, new.replace('\n',nl))
s=s.rstrip()+nl+nl+"""internal static class P1InitTrace
{
    internal static readonly string? Out = System.Environment.GetEnvironmentVariable("P1INIT_OUT");
    internal static int Depth;
    static readonly System.Collections.Generic.List<string> rows = new();
    static P1InitTrace()
    {
        if (Out is not null)
            System.AppDomain.CurrentDomain.ProcessExit += (_, _) => { lock (rows) System.IO.File.WriteAllLines(Out, rows); };
    }
    internal static void Add(int depth, string? name, long t0, long t1)
    {
        lock (rows) rows.Add($"INIT\\t{depth}\\t{name}\\t{t0}\\t{t1}\\t{System.Environment.CurrentManagedThreadId}");
    }
}""".replace('\n',nl)+nl
open(p,'w',newline='').write(s)
print("patched")
