// scoperules: measures the C# scope, definite-assignment and closure rules each candidate shape for
// Go's init-statement scope depends on. Every case is compiled in memory (C# preview, Release); a case
// with a `Run` method that compiles is also executed and its output printed. Then the IL of today's
// shape and of each candidate is printed for the comma-ok assertion.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var refs = new[] { typeof(object), typeof(Console), typeof(Enumerable), typeof(List<>) }
    .Select(t => t.Assembly.Location)
    .Concat(new[] { "System.Runtime.dll", "System.Collections.dll" }
        .Select(n => Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location)!, n)))
    .Distinct().Select(p => MetadataReference.CreateFromFile(p)).ToArray();
var parse = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

const string prelude = @"
using System; using System.Collections.Generic;
static class H {
    public static (T, bool) As<T>(object o) => o is T t ? (t, true) : (default!, false);
    public static bool As<T>(object o, out T v) { if (o is T t) { v = t; return true; } v = default!; return false; }
    public static (int, string) Two() => (7, null);
    public static (int, string) TwoErr() => (8, ""boom"");
}
";

var cases = new (string name, string body)[]
{
    ("R1 if-condition pattern var leaks into the enclosing block",
     "static class C { static void M(object o) { if (o is int x) { } int x = 1; } }"),
    ("R2 a pattern var is usable after the if (it leaked)",
     "static class C { static int M(object o) { if (!(o is int x)) return 0; return x; } }"),
    ("R3 an else-if condition's pattern var stays in the else-if (no leak)",
     "static class C { static void M(object o, bool a) { if (a) { } else if (o is int x) { } int x = 2; } }"),
    ("R3b sibling else-ifs reuse one name",
     "static class C { static void M(object o, object p, bool a) { if (a) { } else if (o is int x) { } else if (p is int x) { } } }"),
    ("R3c head-if and an else-if reuse one name",
     "static class C { static void M(object o, object p) { if (o is int x) { } else if (p is int x) { } } }"),
    ("R4 two dropped-brace inits declare one name in one block",
     "static class C { static void M(object o) { var (a, ok) = H.As<int>(o); if (ok) { } var (b, ok) = H.As<string>(o); if (ok) { } } }"),
    ("R5 a dropped-brace init and a braced sibling declare one name",
     "static class C { static void M(object o) { var (a, ok) = H.As<int>(o); if (ok) { } { var (b, ok) = H.As<string>(o); if (ok) { } } } }"),
    ("R5b the braced sibling comes FIRST",
     "static class C { static void M(object o) { { var (b, ok) = H.As<string>(o); if (ok) { } } var (a, ok) = H.As<int>(o); if (ok) { } } }"),
    ("R6a silent rebinding, braces kept: a later `x` is the field",
     "static class C { static int x = 42; public static string Run() { { var x = 1; if (x > 0) { } } return x.ToString(); } }"),
    ("R6b silent rebinding, braces dropped: the same `x` is now the local",
     "static class C { static int x = 42; public static string Run() { var x = 1; if (x > 0) { } return x.ToString(); } }"),
    ("R7 a use of the name BEFORE the dropped-brace init",
     "static class C { static int x = 42; static int M() { int y = x; var x = 1; if (x > 0) { } return y; } }"),
    ("R8a backward goto + capture, braces kept",
     @"static class C { public static string Run() { var fs = new List<Func<int>>(); int i = 0;
        again:
        { var v = i; if (v >= 0) { fs.Add(() => v); } }
        i++; if (i < 3) goto again;
        return string.Join("","", fs.ConvertAll(f => f())); } }"),
    ("R8b backward goto + capture, braces dropped",
     @"static class C { public static string Run() { var fs = new List<Func<int>>(); int i = 0;
        again:
        var v = i; if (v >= 0) { fs.Add(() => v); }
        i++; if (i < 3) goto again;
        return string.Join("","", fs.ConvertAll(f => f())); } }"),
    ("R8c a loop body + capture, braces dropped (the loop body is the scope)",
     @"static class C { public static string Run() { var fs = new List<Func<int>>();
        for (int i = 0; i < 3; i++) { var v = i; if (v >= 0) { fs.Add(() => v); } }
        return string.Join("","", fs.ConvertAll(f => f())); } }"),
    ("R9 comma-ok pattern: the value used in the else branch",
     "static class C { static void M(object o) { if (H.As<string>(o) is (var s, true)) { } else { Console.Write(s); } } }"),
    ("R9b comma-ok pattern: the value used in the true branch",
     "static class C { public static string Run() { object o = \"hi\"; if (H.As<string>(o) is (var s, true)) { return s; } return \"miss\"; } }"),
    ("R10 multi-result `is var (n, err) && err != null`: n used in the else branch",
     "static class C { static void M() { if (H.Two() is var (n, err) && err != null) { } else { Console.Write(n); } } }"),
    ("R10b multi-result `is var (n, err) && err != null`: both used in the true branch",
     "static class C { public static string Run() { if (H.TwoErr() is var (n, err) && err != null) { return n + err; } return \"none\"; } }"),
    ("R10c plain := `is var x && x > 0`: x used in the else branch",
     "static class C { static void M(int a) { if (a + 1 is var x && x > 5) { } else { Console.Write(x); } } }"),
    ("R11 out-var helper in an if condition leaks like a pattern var",
     "static class C { static void M(object o) { if (H.As<string>(o, out var s)) { } string s = \"\"; } }"),
    ("R12 a lambda parameter shadows an enclosing local",
     "static class C { static void M() { var ok = true; Func<int, int> f = ok => ok; } }"),
    ("R13 a switch's governing-expression pattern var",
     "static class C { static void M(object o) { switch (o is int x ? x : 0) { default: break; } int x = 1; } }"),
    ("R14 a labeled declaration (a dropped-brace init under a label)",
     "static class C { static void M(object o) { L: var (a, ok) = H.As<int>(o); if (ok) { } goto L; } }"),
    ("R15 comma-ok pattern with a discard value `is (_, true)`",
     "static class C { public static string Run() { object o = 3; if (H.As<int>(o) is (_, true)) { return \"hit\"; } return \"miss\"; } }"),
    ("R16 `!ok` as a negated pattern: the value used AFTER the if",
     "static class C { static int M(object o) { if (H.As<int>(o) is not (var v, true)) { return -1; } return v; } }"),
};

foreach (var (name, body) in cases)
{
    var comp = CSharpCompilation.Create("t" + Guid.NewGuid().ToString("N"),
        new[] { CSharpSyntaxTree.ParseText(prelude + body, parse) }, refs,
        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
    using var ms = new MemoryStream();
    var r = comp.Emit(ms);
    var diags = r.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning)
        .Select(d => $"{d.Id}({d.Severity.ToString()[0]}) {d.GetMessage()}").Distinct().ToList();
    string outcome = r.Success ? "COMPILES" : "ERROR";
    string run = "";
    if (r.Success && body.Contains("Run()"))
    {
        var asm = Assembly.Load(ms.ToArray());
        run = " -> Run() = " + asm.GetType("C")!.GetMethod("Run")!.Invoke(null, null);
    }
    Console.WriteLine($"{name}\n    {outcome}{run}");
    foreach (var d in diags) Console.WriteLine("    " + d);
}

// ---- IL of the comma-ok assertion: today's shape against each candidate
const string ilsrc = @"
static class S {
    public static int Today(object w) { { var (sw, ok) = H.As<string>(w); if (ok) { return sw.Length; } } return -1; }
    public static int Dropped(object w) { var (sw, ok) = H.As<string>(w); if (ok) { return sw.Length; } return -1; }
    public static int Pattern(object w) { if (H.As<string>(w) is (var sw, true)) { return sw.Length; } return -1; }
    public static int OutVar(object w) { if (H.As<string>(w, out var sw)) { return sw.Length; } return -1; }
    public static int MultiToday() { { var (n, err) = H.TwoErr(); if (err != null) { return n; } } return 0; }
    public static int MultiPattern() { if (H.TwoErr() is var (n, err) && err != null) { return n; } return 0; }
}";
var ilcomp = CSharpCompilation.Create("il", new[] { CSharpSyntaxTree.ParseText(prelude + ilsrc, parse) }, refs,
    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release));
using var ilms = new MemoryStream();
var ilr = ilcomp.Emit(ilms);
if (!ilr.Success) { foreach (var d in ilr.Diagnostics) Console.WriteLine(d); return; }
var ilasm = Assembly.Load(ilms.ToArray());
var ops = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (OpCode)f.GetValue(null)!)
    .ToDictionary(o => (ushort)o.Value);
foreach (var m in ilasm.GetType("S")!.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
{
    var body = m.GetMethodBody()!;
    var il = body.GetILAsByteArray()!;
    var listing = new List<string>();
    for (int i = 0; i < il.Length;)
    {
        ushort v = il[i]; int start = i; i++;
        if (v == 0xFE) { v = (ushort)(0xFE00 | il[i]); i++; }
        var op = ops[v];
        int size = op.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(il, i),
            _ => 4
        };
        listing.Add(op.Name!);
        i += size;
    }
    Console.WriteLine($"IL {m.Name}: {il.Length} bytes, {listing.Count} instructions, {body.LocalVariables.Count} locals: {string.Join(' ', listing)}");
}
