// twinscan: the M:N spike's P1b (COORD's second G1 trigger). For every function canpark says can park, find its
// emitted C# method and classify what an async TWIN of it would meet:
//
//   TRANSFORMS (a generator rewrites them mechanically in the twin only; DESIGN-mn-scheduler §5 O6's table):
//     boxed ref local   `ref var x = ref heap(..., out var Ꮡx)` or `ref var b = ref Ꮡb.DerefOrNull()`: the box exists,
//                        so the twin reads through it
//     defer frame       a `GoFrame` local (a ref struct): the twin uses a class frame
//     span param        a variadic (`ꓸꓸꓸT` = Span<T>) or Span parameter: the twin takes a slice
//     ref param         a ref/in/out parameter (a by-ref receiver included): the twin takes the box
//   HARD (the design's "no twin" fallback: the function keeps running synchronously and costs a thread when it parks):
//     fixed, unsafe, stackalloc, a Span/ReadOnlySpan local, any other ref local
//
// Usage: twinscan <src/core dir> <goos> <canpark.json>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TwinScan;

internal sealed record Features(bool BoxedRef, bool DeferFrame, bool SpanParam, bool RefParam, bool Fixed, bool Unsafe, bool Stackalloc, bool SpanLocal, bool OtherRef)
{
    public bool Hard => Fixed || Unsafe || Stackalloc || SpanLocal || OtherRef;

    public Features Or(Features o) => new(BoxedRef || o.BoxedRef, DeferFrame || o.DeferFrame, SpanParam || o.SpanParam, RefParam || o.RefParam,
        Fixed || o.Fixed, Unsafe || o.Unsafe, Stackalloc || o.Stackalloc, SpanLocal || o.SpanLocal, OtherRef || o.OtherRef);

    public string HardReasons => string.Join("+", new[] { Fixed ? "fixed" : null, Unsafe ? "unsafe" : null, Stackalloc ? "stackalloc" : null,
        SpanLocal ? "span-local" : null, OtherRef ? "other-ref-local" : null }.Where(s => s != null));
}

internal static class Program
{
    private static readonly Regex MethodName = new(@"^\((?<ptr>\*)?(?<pkg>.+)\.(?<recv>[^.\[\]()]+)(\[[^\]]*\])?\)\.(?<name>[^.\[\]]+)$");
    private static readonly Regex FuncName = new(@"^(?<pkg>.+)\.(?<name>[^.\[\]()]+)(\[[^\]]*\])?$");

    public static int Main(string[] args)
    {
        if (args.Length != 3) { Console.Error.WriteLine("usage: twinscan <src/core dir> <goos> <canpark.json>"); return 2; }
        string core = args[0], goos = args[1];

        using JsonDocument json = JsonDocument.Parse(File.ReadAllText(args[2]));
        string[] canPark = json.RootElement.GetProperty("canParkNames").EnumerateArray().Select(e => e.GetString()!).ToArray();
        string[] direct = json.RootElement.GetProperty("directParkerNames").EnumerateArray().Select(e => e.GetString()!).ToArray();

        var index = new Dictionary<string, Dictionary<(string recv, string name), Features>>();

        Dictionary<(string, string), Features> MethodsOf(string pkg)
        {
            if (index.TryGetValue(pkg, out var known)) return known;
            var map = new Dictionary<(string, string), Features>();
            string dir = Path.Combine(core, pkg.Replace('/', Path.DirectorySeparatorChar));
            var files = new List<string>();
            if (Directory.Exists(dir)) files.AddRange(Directory.GetFiles(dir, "*.cs"));
            string flavour = Path.Combine(dir, goos);
            if (Directory.Exists(flavour)) files.AddRange(Directory.GetFiles(flavour, "*.cs"));

            foreach (string file in files.Where(f => !f.EndsWith("_test.cs", StringComparison.Ordinal)))
            {
                SyntaxNode root = CSharpSyntaxTree.ParseText(File.ReadAllText(file)).GetRoot();
                foreach (MethodDeclarationSyntax m in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
                {
                    if (m.Body is null && m.ExpressionBody is null) continue;
                    var key = (ReceiverOf(m), m.Identifier.ValueText.TrimStart('@'));
                    Features f = Classify(m);
                    map[key] = map.TryGetValue(key, out var prior) ? prior.Or(f) : f;
                }
            }

            index[pkg] = map;
            return map;
        }

        Report("CAN-PARK SET (canpark's upper bound)", canPark, MethodsOf, listHard: false);
        Report("DIRECT PARKERS (the common park paths: chan, select, sync, sleep, netpoll)", direct, MethodsOf, listHard: true);

        if (Environment.GetEnvironmentVariable("TWINSCAN_SHAPES") == "1")
        {
            Console.WriteLine("== OTHER REF-LOCAL SHAPES (every scanned method, not only the can-park set), by count");
            foreach (var g in OtherRefShapes.GroupBy(s => Regex.Replace(s, @"\b(?!ref\b)[a-zᴛ]\w*\b", "x")).OrderByDescending(g => g.Count()).Take(40))
                Console.WriteLine($"  {g.Count(),6}  {g.Key}   e.g. {g.First()}");
        }

        return 0;
    }

    private static void Report(string title, string[] names, Func<string, Dictionary<(string, string), Features>> methodsOf, bool listHard)
    {
        int matched = 0, unmatched = 0, hard = 0, boxed = 0, defer = 0, spanParam = 0, refParam = 0;
        var reasons = new Dictionary<string, int>();
        var hardNames = new List<string>();
        var unmatchedNames = new List<string>();

        foreach (string go in names)
        {
            (string pkg, string recv, string name)? key = Parse(go);
            if (key is null || !methodsOf(key.Value.pkg).TryGetValue((key.Value.recv, key.Value.name), out Features? f))
            {
                unmatched++;
                unmatchedNames.Add(go);
                continue;
            }

            matched++;
            if (f.BoxedRef) boxed++;
            if (f.DeferFrame) defer++;
            if (f.SpanParam) spanParam++;
            if (f.RefParam) refParam++;
            if (f.Hard)
            {
                hard++;
                foreach (string r in f.HardReasons.Split('+')) reasons[r] = reasons.GetValueOrDefault(r) + 1;
                hardNames.Add($"{go} [{f.HardReasons}]");
            }
        }

        Console.WriteLine($"== {title}");
        Console.WriteLine($"  functions {names.Length}: matched to emitted C# {matched}, unmatched {unmatched}");
        Console.WriteLine($"  TRANSFORMS: boxed-ref-local {boxed}, defer-frame {defer}, span-param {spanParam}, ref-param {refParam}");
        Console.WriteLine($"  HARD (no twin, the fallback): {hard} of {matched} ({(matched == 0 ? 0 : 100.0 * hard / matched):F1}%) by reason {string.Join(", ", reasons.OrderByDescending(r => r.Value).Select(r => $"{r.Key} {r.Value}"))}");
        if (listHard) foreach (string h in hardNames.OrderBy(h => h)) Console.WriteLine($"    hard: {h}");
        foreach (string u in unmatchedNames.OrderBy(u => u).Take(listHard ? 50 : 12)) Console.WriteLine($"    unmatched: {u}");
    }

    private static (string pkg, string recv, string name)? Parse(string go)
    {
        Match m = MethodName.Match(go);
        if (m.Success) return (m.Groups["pkg"].Value, m.Groups["recv"].Value, m.Groups["name"].Value);
        m = FuncName.Match(go);
        if (m.Success) return (m.Groups["pkg"].Value, "", m.Groups["name"].Value);
        return null;
    }

    // The receiver a converted method carries as its `this` parameter: `this ж<conn> Ꮡc`, `this ref conn c`, `this conn c`.
    private static string ReceiverOf(MethodDeclarationSyntax m)
    {
        ParameterSyntax? first = m.ParameterList.Parameters.FirstOrDefault();
        if (first is null || !first.Modifiers.Any(SyntaxKind.ThisKeyword) || first.Type is null) return "";
        TypeSyntax t = first.Type;
        if (t is RefTypeSyntax r) t = r.Type;
        if (t is GenericNameSyntax g && g.Identifier.ValueText == "ж" && g.TypeArgumentList.Arguments.Count == 1) t = g.TypeArgumentList.Arguments[0];
        return t switch
        {
            GenericNameSyntax gn => gn.Identifier.ValueText,
            SimpleNameSyntax sn => sn.Identifier.ValueText,
            QualifiedNameSyntax qn => qn.Right.Identifier.ValueText,
            _ => t.ToString()
        };
    }

    // Every "other" ref-local initializer seen, as a shape: box names (Ꮡx) become ᏑX and every other identifier keeps
    // its name, so a mechanical pattern shows up as one repeated shape. Printed with TWINSCAN_SHAPES=1.
    internal static readonly List<string> OtherRefShapes = new();

    private static string Shape(string init) => Regex.Replace(Regex.Replace(init, @"Ꮡ\w+", "ᏑX"), @"\s+", " ");

    private static Features Classify(MethodDeclarationSyntax m)
    {
        bool boxed = false, defer = false, spanParam = false, refParam = false, isFixed = false, isUnsafe = false, stackAlloc = false, spanLocal = false, otherRef = false;

        foreach (ParameterSyntax p in m.ParameterList.Parameters)
        {
            string type = p.Type?.ToString() ?? "";
            if (type.StartsWith("ꓸꓸꓸ", StringComparison.Ordinal) || type.StartsWith("Span<", StringComparison.Ordinal) || type.StartsWith("ReadOnlySpan<", StringComparison.Ordinal)) spanParam = true;
            if (p.Modifiers.Any(SyntaxKind.RefKeyword) || p.Modifiers.Any(SyntaxKind.InKeyword) || p.Modifiers.Any(SyntaxKind.OutKeyword) || p.Type is RefTypeSyntax) refParam = true;
        }

        if (m.Modifiers.Any(SyntaxKind.UnsafeKeyword)) isUnsafe = true;

        foreach (SyntaxNode n in m.DescendantNodes())
        {
            switch (n)
            {
                case FixedStatementSyntax: isFixed = true; break;
                case UnsafeStatementSyntax: isUnsafe = true; break;
                case StackAllocArrayCreationExpressionSyntax or ImplicitStackAllocArrayCreationExpressionSyntax: stackAlloc = true; break;
                case LocalDeclarationStatementSyntax local:
                {
                    TypeSyntax type = local.Declaration.Type;
                    string text = type.ToString();
                    if (text == "GoFrame") defer = true;
                    if (text.StartsWith("Span<", StringComparison.Ordinal) || text.StartsWith("ReadOnlySpan<", StringComparison.Ordinal) || text.StartsWith("ꓸꓸꓸ", StringComparison.Ordinal)) spanLocal = true;
                    if (type is RefTypeSyntax)
                    {
                        string init = local.Declaration.Variables.FirstOrDefault()?.Initializer?.Value.ToString() ?? "";
                        // The box shapes, from the shape census over every scanned method (2026-11-09): heap(...) and
                        // heap<T>(..., out var Ꮡx); Ꮡx.DerefOrNull(); Ꮡx.Value / .ValueSlot; box.Value.
                        if (Regex.IsMatch(init, @"^ref\s+heap(<.*>)?\(") || init.EndsWith(".DerefOrNull()", StringComparison.Ordinal)
                            || Regex.IsMatch(init, @"^ref\s+[\wᏑ]+\.(Value|ValueSlot)$")) boxed = true;
                        else { otherRef = true; OtherRefShapes.Add(Shape(init)); }
                    }
                    break;
                }
            }
        }

        return new Features(boxed, defer, spanParam, refParam, isFixed, isUnsafe, stackAlloc, spanLocal, otherRef);
    }
}
