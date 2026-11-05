extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// RecvGenerator's VALUE-receiver ж twins: Go's <c>p.M()</c> on a pointer, where M has a value receiver, calls M on a
/// copy of <c>*p</c>; the twin <c>M(this ж&lt;T&gt; Ꮡv, ...)</c> does exactly that, so the converter writes <c>p.M()</c>
/// rather than <c>(~p).M()</c>.
/// </summary>
/// <remarks>
/// Failing-first record: before the twin, every <c>p.M()</c> below on a <c>ж&lt;T&gt;</c> is CS1929 (no extension
/// binds), so the package does not compile. The arms cover the copy at the call (a mutation inside M never reaches
/// *p), a [GoCopyBound] by-ref value receiver, a generic receiver, Go's nil-dereference text, and the three methods
/// that must get no twin: a pointer-set method (it has its own), a helper over a type another assembly declares, and
/// a method whose twin is already declared.
/// </remarks>
[TestClass]
public class ValueReceiverBoxTwinTests
{
    private const string Package =
        """
        namespace go;

        public static partial class vtest_package
        {
            public partial struct Vertex { public nint X; public nint Y; }
            public partial struct Pair<T> { public T First; }

            public static nint Sum(this Vertex v) => v.X + v.Y;
            public static nint Bump(this Vertex v) { v.X++; return v.X; }
            public static T Head<T>(this Pair<T> p) => p.First;
            [GoCopyBound] public static nint Peek(this ref Vertex v) { v.Y++; return v.Y; }
            public static nint Scale(this ref Vertex v, nint k) { v.X *= k; return v.X; }
            public static nint Twice(this Vertex v) => 2 * v.X;
            public static nint Twice(this ж<Vertex> Ꮡv) => 2 * Ꮡv.Value.X;
            public static nint Width(this @string s) => s.Length;

            public static string Run()
            {
                ж<Vertex> p = Ꮡ(new Vertex { X = 3, Y = 4 });
                nint sum = p.Sum();
                nint bumped = p.Bump();
                nint peeked = p.Peek();
                ж<Pair<@string>> q = Ꮡ(new Pair<@string> { First = "go" });
                return $"{sum} {bumped} {p.Value.X} {peeked} {p.Value.Y} {q.Head()}";
            }

            // The twin raises exactly what today's spelled dereference raises; the converted runtime turns that into
            // Go's "invalid memory address or nil pointer dereference" (Behavioral/NilPointerValueReceiverCall).
            public static string RunNil()
            {
                ж<Vertex> p = default!;
                string spelled, twin;

                try { spelled = (~p).Sum().ToString(); }
                catch (Exception ex) { spelled = ex.GetType().FullName + ": " + ex.Message; }

                try { twin = p.Sum().ToString(); }
                catch (Exception ex) { twin = ex.GetType().FullName + ": " + ex.Message; }

                return spelled == twin ? "same: " + twin : "spelled " + spelled + " | twin " + twin;
            }
        }
        """;

    private const string GlobalUsings =
        """
        global using static go.builtin;
        global using System;
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private static (Compilation output, string[] valueTwins) Run(string source, string assemblyAttributes = "")
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("vtest",
            [CSharpSyntaxTree.ParseText(assemblyAttributes + source, options, path: @"C:\go2cs\src\core\vtest\vtest.cs"),
             CSharpSyntaxTree.ParseText(GlobalUsings, options, path: @"C:\go2cs\src\core\vtest\obj\vtest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new RecvGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string[] valueTwins = [.. output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Select(tree => Path.GetFileName(tree.FilePath))
            .Where(name => name.EndsWith(".value.g.cs", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)];

        return (output, valueTwins);
    }

    private static Type Load(Compilation output)
    {
        using MemoryStream image = new();
        EmitResult emitted = output.Emit(image);

        Assert.IsTrue(emitted.Success, "the package with its ж twins must compile: " +
            string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));

        return Assembly.Load(image.ToArray()).GetType("go.vtest_package")!;
    }

    private static string Invoke(Type package, string method) => (string)package.GetMethod(method)!.Invoke(null, null)!;

    [TestMethod]
    public void AValueMethodIsCalledOnACopyOfThePointee()
    {
        Type package = Load(Run(Package).output);

        // Sum 3+4; Bump returns 4 on its copy and *p keeps X 3; the copy-bound Peek returns 5 and *p keeps Y 4;
        // the generic Head reads the pair through its box.
        Assert.AreEqual("7 4 3 5 4 go", Invoke(package, "Run"));
    }

    [TestMethod]
    public void ANilPointerFailsExactlyAsTheSpelledDereferenceDoes()
    {
        Type package = Load(Run(Package).output);
        string result = Invoke(package, "RunNil");

        Assert.IsTrue(result.StartsWith("same: ", StringComparison.Ordinal) && !result.Contains("same: 7", StringComparison.Ordinal),
            $"a nil box must fail through the twin exactly as `(~p).M()` fails: {result}");
    }

    [TestMethod]
    public void OnlyGoValueMethodsWithoutADeclaredTwinGetOne()
    {
        string[] twins = Run(Package).valueTwins;
        string names = string.Join(", ", twins);

        foreach (string method in new[] { "Sum", "Bump", "Head", "Peek" })
            Assert.IsTrue(twins.Any(twin => twin.Contains($".{method}.", StringComparison.Ordinal)), $"{method} has no value twin: {names}");

        // Scale is a pointer-set method (its twin is the pointer arm's), Twice already declares its ж twin, and Width
        // is a helper over golib's @string, which this compilation does not declare.
        foreach (string method in new[] { "Scale", "Twice", "Width" })
            Assert.IsFalse(twins.Any(twin => twin.Contains($".{method}.", StringComparison.Ordinal)), $"{method} must have no value twin: {names}");
    }

    [TestMethod]
    public void AHandOwnedPackageGetsNoValueTwins()
    {
        string[] twins = Run(Package, "[assembly: go.GoHandOwnedPackage]\n").valueTwins;

        Assert.AreEqual(0, twins.Length, $"a hand-owned package's methods are Go methods by attribute alone: {string.Join(", ", twins)}");
    }
}
