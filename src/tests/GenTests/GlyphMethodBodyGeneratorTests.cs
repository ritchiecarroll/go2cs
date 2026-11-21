extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Holds the generators to the suffix form of a Go method (owner ruling 2026-11-21): its BODY is a plain static
/// <c>&lt;Name&gt;⁔</c> in the package class, its receiver the first parameter, and the Go-named extension method in the
/// type's holder is a one-line forwarder to it. The body is no method of the type: no method set, method source,
/// <c>[DynamicDependency]</c> or <c>ж&lt;T&gt;</c> twin names it. The forwarder is the method, as a holder's method
/// always was.
/// </summary>
/// <remarks>
/// The fixture's bodies take their receivers exactly as the forwarders do (value, <c>ref</c>, generic <c>ref</c>), so a
/// generator that read a static method's first parameter as a receiver would answer for the body too.
/// </remarks>
[TestClass]
public class GlyphMethodBodyGeneratorTests
{
    private const string PackagePath = @"C:\go2cs\src\core\gtest\gtest.cs";

    private const string Package =
        """
        using System.Reflection;

        namespace go;

        using static gtest_package;

        [GoPackage("gtest")]
        public static partial class gtest_package
        {
            public partial struct Vertex { public nint X; public nint Y; }
            public partial struct Pair<K, V> { public K k; public V v; }

            internal static nint Sum⁔(/*this*/ Vertex v) => v.X + v.Y;

            internal static nint Scale⁔(/*this*/ ref Vertex v, nint k)
            {
                v.X *= k;
                return v.X;
            }

            internal static B Swap⁔<A, B>(/*this*/ ref Pair<A, B> p, B v)
            {
                B old = p.v;
                p.v = v;
                return old;
            }

            // Executed by the test.
            public static string Probe()
            {
                ж<Vertex> p = Ꮡ(new Vertex { X = 3, Y = 4 });
                nint sum = p.Sum();
                nint scaled = p.Scale(2);

                IGoMethodSet set = default(Vertex);
                nint unbound = ((Func<Vertex, nint>)set.ᴛMethod("Sum", false, false, null)!)(new Vertex { X = 1, Y = 1 });
                nint throughPointer = ((Func<ж<Vertex>, nint, nint>)set.ᴛMethod("Scale", true, false, null)!)(p, 5);
                bool bodyIsNoMethod = set.ᴛMethod("Sum\u2054", false, false, null) is null && set.ᴛMethod("Scale\u2054", true, false, null) is null;

                IGoMethodSource source = default(Pair<@string, nint>);
                MethodInfo swap = source.GoMethod("Swap", true)!;
                ж<Pair<@string, nint>> pair = Ꮡ(new Pair<@string, nint> { k = "z", v = 26 });
                object? old = swap.Invoke(null, [pair, (nint)7]);

                return $"{sum} {scaled} {unbound} {throughPointer} {p.Value.X} {bodyIsNoMethod} {old} {pair.Value.v} {swap.DeclaringType!.Name}";
            }
        }

        [GoMethodHost(typeof(gtest_package.Vertex))]
        public static partial class gtestᴛVertex
        {
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Sum(this Vertex v) => gtest_package.Sum⁔(v);
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Scale(this ref Vertex v, nint k) => gtest_package.Scale⁔(ref v, k);
        }

        [GoMethodHost(typeof(gtest_package.Pair<,>))]
        public static partial class gtestᴛPair
        {
            [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static B Swap<A, B>(this ref Pair<A, B> p, B v) => gtest_package.Swap⁔<A, B>(ref p, v);
        }
        """;

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private static (SyntaxTree[] generated, Compilation executed) Run()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("gtest",
            [CSharpSyntaxTree.ParseText(Package, options, path: PackagePath), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\gtest\obj\gtest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator(), new RecvGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        SyntaxTree[] generated = [.. output.SyntaxTrees.Except(compilation.SyntaxTrees)];

        // As in MethodHolderGeneratorTests: the type's main generated part is left out of the executed compilation; the
        // real builds compile it.
        Compilation executed = compilation.AddSyntaxTrees(generated.Where(tree =>
            tree.FilePath.EndsWith(".MethodSet.g.cs", StringComparison.Ordinal) ||
            tree.FilePath.EndsWith(".MethodSource.g.cs", StringComparison.Ordinal) ||
            tree.FilePath.Contains("go2cs.RecvGenerator")));

        return (generated, executed);
    }

    // The class each of RecvGenerator's twins of the method named name is written into.
    private static string[] TwinClasses(SyntaxTree[] generated, string name) =>
        [.. generated.Where(tree => tree.FilePath.Contains("go2cs.RecvGenerator"))
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            .Where(method => method.Identifier.ValueText == name && method.Parent is ClassDeclarationSyntax)
            .Select(method => ((ClassDeclarationSyntax)method.Parent!).Identifier.ValueText)
            .Distinct()
            .Order(StringComparer.Ordinal)];

    // The TypeGenerator's file for the type, by the part's suffix (".g.cs" is the type's main part).
    private static string Source(SyntaxTree[] generated, string type, string suffix)
    {
        string name = $"go.gtest_package.{type}{suffix}";
        SyntaxTree[] found = [.. generated.Where(tree => tree.FilePath.Contains("go2cs.TypeGenerator") && Path.GetFileName(tree.FilePath) == name)];

        Assert.AreEqual(1, found.Length, $"one generated file is named {name}; the TypeGenerator wrote: " +
            string.Join(", ", generated.Where(tree => tree.FilePath.Contains("go2cs.TypeGenerator")).Select(tree => Path.GetFileName(tree.FilePath))));

        return found[0].ToString();
    }

    [TestMethod]
    public void ABodyGetsNoTwinAndItsForwarderKeepsItsOwn()
    {
        SyntaxTree[] generated = Run().generated;

        CollectionAssert.AreEqual(Array.Empty<string>(), TwinClasses(generated, "Sum\u2054"), "a value method's body");
        CollectionAssert.AreEqual(Array.Empty<string>(), TwinClasses(generated, "Scale\u2054"), "a pointer method's body");
        CollectionAssert.AreEqual(Array.Empty<string>(), TwinClasses(generated, "Swap\u2054"), "a generic type's pointer method's body");

        CollectionAssert.AreEqual(new[] { "gtestᴛVertex" }, TwinClasses(generated, "Sum"), "the value method's forwarder");
        CollectionAssert.AreEqual(new[] { "gtestᴛVertex" }, TwinClasses(generated, "Scale"), "the pointer method's forwarder");
        CollectionAssert.AreEqual(new[] { "gtestᴛPair" }, TwinClasses(generated, "Swap"), "the generic pointer method's forwarder");
    }

    [TestMethod]
    public void TheMethodSetTheMethodSourceAndToStringNameTheForwarderNeverTheBody()
    {
        SyntaxTree[] generated = Run().generated;

        string set = Source(generated, "Vertex", ".MethodSet.g.cs");
        StringAssert.Contains(set, "global::go.gtestᴛVertex.Sum", "the value method is its forwarder");
        StringAssert.Contains(set, "global::go.gtestᴛVertex.Scale", "the pointer method is its forwarder");
        Assert.IsFalse(set.Contains('\u2054'), "no body is a member of Vertex's method set:\n" + set);

        string source = Source(generated, "Pair_K, V_", ".MethodSource.g.cs");
        StringAssert.Contains(source, "global::go.gtestᴛPair.Swap<K, V>).Method", "a generic type's method is its forwarder");
        Assert.IsFalse(source.Contains('\u2054'), "no body is a method source of Pair:\n" + source);

        string vertex = Source(generated, "Vertex", ".g.cs");
        StringAssert.Contains(vertex, "DynamicDependency(\"Sum\", typeof(global::go.gtestᴛVertex))");
        StringAssert.Contains(vertex, "DynamicDependency(\"Scale\", typeof(global::go.gtestᴛVertex))");
        Assert.IsFalse(vertex.Contains('\u2054'), "no body is kept as a method of Vertex:\n" + vertex);
    }

    [TestMethod]
    public void ThePackageWithItsBodiesAndForwardersCompilesAndRuns()
    {
        Compilation executed = Run().executed;
        Diagnostic[] errors = [.. executed.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];

        Assert.AreEqual(0, errors.Length, "the package, its bodies, forwarders, method sets and ж twins compile: " + string.Join("; ", errors.Take(3)));

        using MemoryStream image = new();
        Assert.IsTrue(executed.Emit(image).Success, "the package emits an assembly");

        System.Reflection.MethodInfo probe = Assembly.Load(image.ToArray()).GetType("go.gtest_package", throwOnError: true)!.GetMethod("Probe")!;

        // p.Sum() 3+4 and p.Scale(2) through the forwarders' twins; the set's unbound Sum 1+1 and its Scale through
        // *Vertex (6*5); X is 30; the set answers no body; the source's Swap returns 26, leaves 7, and is the forwarder.
        Assert.AreEqual("7 6 2 30 30 True 26 7 gtestᴛPair", probe.Invoke(null, null));
    }
}
