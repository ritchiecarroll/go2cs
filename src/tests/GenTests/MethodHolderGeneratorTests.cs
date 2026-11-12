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
/// Holds the generators to the method holder: the class <c>&lt;pkg&gt;ᴛ&lt;Type&gt;</c>, marked
/// <c>[GoMethodHost(typeof(T))]</c>, in which the converter declares the methods a Go type declares, beside the
/// package class and not in it.
/// </summary>
/// <remarks>
/// <para>
/// Failing-first record: with the generators that read a type's methods from its package class alone, a value method
/// in a holder gets no <c>ж&lt;T&gt;</c> twin (<c>p.Sum()</c> is CS1929), a pointer method's twin is written into
/// the package class, the type carries no method set, no method source and no <c>[DynamicDependency]</c> for the
/// method, a map wrapper keeps the public member a Go method of the holder names, and a field keeps the property that
/// collides with a method its embedded type's holder declares.
/// </para>
/// <para>
/// The fixture gives the package a function <c>Sum</c> beside the type's method <c>Sum</c>, which one class cannot
/// hold as Go declares them: that is what the holder is for.
/// </para>
/// </remarks>
[TestClass]
public class MethodHolderGeneratorTests
{
    private const string PackagePath = @"C:\go2cs\src\core\htest\htest.cs";

    private const string Package =
        """
        using System.Reflection;

        namespace go;

        using static htest_package;

        [GoPackage("htest")]
        public static partial class htest_package
        {
            public partial struct Vertex { public nint X; public nint Y; }
            public partial struct Pair<K, V> { public K k; public V v; }
            public partial struct Tagged { public nint n; }
            public partial struct Legacy { public nint n; }

            // A package function named like Vertex's method.
            public static nint Sum(nint a, nint b) => a + b;

            // A type without a holder keeps its methods here, as a hand-owned file writes them.
            public static nint Get(this Legacy l) => l.n;
            public static void Set(this ref Legacy l, nint n) => l.n = n;

            // Executed by the test.
            public static string Probe()
            {
                ж<Vertex> p = Ꮡ(new Vertex { X = 3, Y = 4 });
                nint sum = p.Sum();
                nint scaled = p.Scale(2);

                IGoMethodSet set = default(Vertex);
                nint unbound = ((Func<Vertex, nint>)set.ᴛMethod("Sum", false, false, null)!)(new Vertex { X = 1, Y = 1 });
                nint throughPointer = ((Func<ж<Vertex>, nint, nint>)set.ᴛMethod("Scale", true, false, null)!)(p, 5);

                IGoMethodSource source = default(Pair<@string, nint>);
                MethodInfo swap = source.GoMethod("Swap", true)!;
                ж<Pair<@string, nint>> pair = Ꮡ(new Pair<@string, nint> { k = "z", v = 26 });
                object? old = swap.Invoke(null, [pair, (nint)7]);

                ж<Tagged> tagged = Ꮡ(new Tagged { n = 8 });
                ж<Legacy> legacy = Ꮡ(new Legacy { n = 1 });
                legacy.Set(9);

                return $"{sum} {scaled} {unbound} {throughPointer} {p.Value.X} {Sum(10, 20)} {old} {pair.Value.v} " +
                       $"{swap.DeclaringType!.Name} {tagged.Label()} {tagged.Bump()} {legacy.Get()}";
            }
        }

        [GoMethodHost(typeof(htest_package.Vertex))]
        public static partial class htestᴛVertex
        {
            public static nint Sum(this Vertex v) => htest_package.Sum(v.X, v.Y);

            public static nint Scale(this ref Vertex v, nint k)
            {
                v.X *= k;
                return v.X;
            }

            internal static nint size(this Vertex v) => 2;
        }

        [GoMethodHost(typeof(htest_package.Pair<,>))]
        public static partial class htestᴛPair
        {
            public static @string String<K, V>(this Pair<K, V> p) => (@string)$"{p.k}={p.v}";

            public static B Swap<A, B>(this ref Pair<A, B> p, B v)
            {
                B old = p.v;
                p.v = v;
                return old;
            }
        }

        // The two holders a type has where a _test.go file declares a method on it and both compile into one assembly.
        [GoMethodHost(typeof(htest_package.Tagged))]
        public static partial class htestᴛTagged
        {
            public static nint Label(this Tagged t) => t.n;
        }

        [GoMethodHost(typeof(htest_package.Tagged))]
        public static partial class htestᴛTaggedᴛtest
        {
            public static nint Bump(this ref Tagged t) => ++t.n;
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

        CSharpCompilation compilation = CSharpCompilation.Create("htest",
            [CSharpSyntaxTree.ParseText(Package, options, path: PackagePath), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\htest\obj\htest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator(), new RecvGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        SyntaxTree[] generated = [.. output.SyntaxTrees.Except(compilation.SyntaxTrees)];

        // As in GoMethodSetTests: the type's main generated part is left out of the executed compilation (this
        // harness's Roslyn cannot resolve its ToString's string.Join over the net10 reference assemblies); the real
        // builds compile it.
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
        string name = $"go.htest_package.{type}{suffix}";
        SyntaxTree[] found = [.. generated.Where(tree => tree.FilePath.Contains("go2cs.TypeGenerator") && Path.GetFileName(tree.FilePath) == name)];

        Assert.AreEqual(1, found.Length, $"one generated file is named {name}; the TypeGenerator wrote: " +
            string.Join(", ", generated.Where(tree => tree.FilePath.Contains("go2cs.TypeGenerator")).Select(tree => Path.GetFileName(tree.FilePath))));

        return found[0].ToString();
    }

    [TestMethod]
    public void ATwinIsWrittenIntoTheClassOfItsMethod()
    {
        SyntaxTree[] generated = Run().generated;

        CollectionAssert.AreEqual(new[] { "htestᴛVertex" }, TwinClasses(generated, "Sum"), "a value method of a holder");
        CollectionAssert.AreEqual(new[] { "htestᴛVertex" }, TwinClasses(generated, "Scale"), "a pointer method of a holder");
        CollectionAssert.AreEqual(new[] { "htestᴛVertex" }, TwinClasses(generated, "size"), "an unexported method of a holder");
        CollectionAssert.AreEqual(new[] { "htestᴛPair" }, TwinClasses(generated, "Swap"), "a generic type's pointer method");
        CollectionAssert.AreEqual(new[] { "htestᴛTagged" }, TwinClasses(generated, "Label"), "the first of a type's two holders");
        CollectionAssert.AreEqual(new[] { "htestᴛTaggedᴛtest" }, TwinClasses(generated, "Bump"), "the second of a type's two holders");

        // Control: a method the package class declares keeps its twin there.
        CollectionAssert.AreEqual(new[] { "htest_package" }, TwinClasses(generated, "Get"), "a value method of the package class");
        CollectionAssert.AreEqual(new[] { "htest_package" }, TwinClasses(generated, "Set"), "a pointer method of the package class");
    }

    [TestMethod]
    public void TheMethodSetAndTheMethodSourceNameTheHolder()
    {
        SyntaxTree[] generated = Run().generated;

        string set = Source(generated, "Vertex", ".MethodSet.g.cs");
        StringAssert.Contains(set, "global::go.htestᴛVertex.Sum", "the value method is called on its holder");
        StringAssert.Contains(set, "global::go.htestᴛVertex.Scale", "the pointer method is called on its holder");
        Assert.IsFalse(set.Contains("global::go.htest_package.Sum", StringComparison.Ordinal), "the package's function Sum is no method of Vertex");

        string source = Source(generated, "Pair_K, V_", ".MethodSource.g.cs");
        StringAssert.Contains(source, "global::go.htestᴛPair.Swap<K, V>).Method", "a generic type's method is named on its holder");

        string tagged = Source(generated, "Tagged", ".MethodSet.g.cs");
        StringAssert.Contains(tagged, "global::go.htestᴛTagged.Label", "a method of the first holder");
        StringAssert.Contains(tagged, "global::go.htestᴛTaggedᴛtest.Bump", "a method of the second holder");
    }

    [TestMethod]
    public void ToStringKeepsTheHoldersMethods()
    {
        SyntaxTree[] generated = Run().generated;

        string vertex = Source(generated, "Vertex", ".g.cs");
        StringAssert.Contains(vertex, "DynamicDependency(\"Sum\", typeof(global::go.htestᴛVertex))");
        StringAssert.Contains(vertex, "DynamicDependency(\"Scale\", typeof(global::go.htestᴛVertex))");
        StringAssert.Contains(vertex, "DynamicDependency(\"size\", typeof(global::go.htestᴛVertex))");
        Assert.IsFalse(vertex.Contains("DynamicDependency(\"Sum\", typeof(global::go.htest_package))", StringComparison.Ordinal),
            "the package's function Sum is kept by its callers, not by Vertex");

        string tagged = Source(generated, "Tagged", ".g.cs");
        StringAssert.Contains(tagged, "DynamicDependency(\"Label\", typeof(global::go.htestᴛTagged))");
        StringAssert.Contains(tagged, "DynamicDependency(\"Bump\", typeof(global::go.htestᴛTaggedᴛtest))");

        // Control: a type without a holder names the package class.
        StringAssert.Contains(Source(generated, "Legacy", ".g.cs"), "DynamicDependency(\"Get\", typeof(global::go.htest_package))");
    }

    [TestMethod]
    public void ThePackageWithItsHoldersCompilesAndRuns()
    {
        Compilation executed = Run().executed;
        Diagnostic[] errors = [.. executed.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];

        Assert.AreEqual(0, errors.Length, "the package, its holders, method sets and ж twins compile: " + string.Join("; ", errors.Take(3)));

        using MemoryStream image = new();
        Assert.IsTrue(executed.Emit(image).Success, "the package emits an assembly");

        System.Reflection.MethodInfo probe = Assembly.Load(image.ToArray()).GetType("go.htest_package", throwOnError: true)!.GetMethod("Probe")!;

        // p.Sum() 3+4 through the value twin; p.Scale(2) through the pointer twin; the set's unbound Sum 1+1 and its
        // Scale through *Vertex (6*5); X is 30; the package's own Sum; the source's Swap returns 26 and leaves 7, and
        // is declared on the holder; both of Tagged's holders bind on its box; a type without a holder is unchanged.
        Assert.AreEqual("7 6 2 30 30 30 26 7 htestᴛPair 8 9 9", probe.Invoke(null, null));
    }

    // TypeGenerator.GoMethodNames: a wrapper yields a member name to a Go method of the type (WrapperMemberYieldTests),
    // wherever the type's methods are declared.
    private const string Wrappers =
        """
        namespace go
        {
            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            public class GoMethodHostAttribute(System.Type receiver) : System.Attribute
            {
                public System.Type Receiver { get; } = receiver;
            }

            public readonly struct map<K, V> { private readonly object store; }

            public static partial class demo_package
            {
                [GoType("map[nint, nint]")] public partial struct Claims;

                [GoType("map[nint, nint]")] public partial struct Plain;
            }

            [GoMethodHost(typeof(demo_package.Claims))]
            public static partial class demoᴛClaims
            {
                public static void Add(this demo_package.Claims c, nint k, nint v) { }
                public static void Clear(this demo_package.Claims c) { }
            }
        }
        """;

    private static HashSet<string> PublicInstanceMethods(string type)
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        CSharpCompilation compilation = CSharpCompilation.Create("holder-wrapper-yield-test",
            [CSharpSyntaxTree.ParseText(Wrappers, new CSharpParseOptions(LanguageVersion.Latest))],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Dictionary<string, string> sources = CSharpGeneratorDriver.Create(new TypeGenerator()).RunGenerators(compilation).GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains($".{type}.") || hint.Contains($".{type}_"));
        Assert.IsNotNull(key, $"the TypeGenerator must generate {type}; got: {string.Join(", ", sources.Keys)}");

        return CSharpSyntaxTree.ParseText(sources[key!], new CSharpParseOptions(LanguageVersion.Latest))
            .GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(method => method.ExplicitInterfaceSpecifier is null && method.Modifiers.Any(SyntaxKind.PublicKeyword) && !method.Modifiers.Any(SyntaxKind.StaticKeyword))
            .Select(method => method.Identifier.Text).ToHashSet();
    }

    [TestMethod]
    public void AWrapperYieldsAMemberNameToAMethodOfItsHolder()
    {
        HashSet<string> claims = PublicInstanceMethods("Claims");

        Assert.IsFalse(claims.Contains("Add"), "Claims' holder declares a Go method Add, so its wrapper must not declare a public Add");
        Assert.IsFalse(claims.Contains("Clear"), "Claims' holder declares a Go method Clear, so its wrapper must not declare a public Clear");

        HashSet<string> plain = PublicInstanceMethods("Plain");

        Assert.IsTrue(plain.Contains("Add") && plain.Contains("Clear"), "control: Plain declares no Go method, so its wrapper keeps both");
    }

    // FieldAccessGenerator.ShadowsPromotedMethod (FieldAccessGeneratorTests' abi.ArrayType shape): the method the
    // embedded type promotes is declared in that type's holder.
    private const string Shadows =
        """
        namespace go
        {
            [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
            public sealed class GoFieldAccessAttribute(params System.Type[] types) : System.Attribute;

            public class GoMethodHostAttribute(System.Type receiver) : System.Attribute
            {
                public System.Type Receiver { get; } = receiver;
            }

            public class ж<T> { public T Value = default!; }

            [GoFieldAccess(typeof(ArrayType), typeof(unrelated))]
            public static partial class abi_package
            {
                public partial struct Type { public nint Size_; }

                public partial struct ArrayType
                {
                    /*embed*/ public Type Type;
                    public nint Len;
                    public nint Elem;
                }

                public partial struct unrelated { public nint Len; }
            }

            [GoMethodHost(typeof(abi_package.Type))]
            public static partial class abiᴛType
            {
                public static nint Len(this ж<abi_package.Type> t) => 0;
            }
        }
        """;

    [TestMethod]
    public void AFieldThatShadowsAMethodOfAnEmbeddedTypesHolderShadowsAPromotedMethod()
    {
        CSharpCompilation compilation = CSharpCompilation.Create("holder-shadow-test",
            [CSharpSyntaxTree.ParseText(Shadows, new CSharpParseOptions(LanguageVersion.Latest))],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        INamedTypeSymbol package = compilation.GetTypeByMetadataName("go.abi_package")!;
        INamedTypeSymbol array = package.GetTypeMembers("ArrayType").Single();
        INamedTypeSymbol unrelated = package.GetTypeMembers("unrelated").Single();

        Assert.IsTrue(FieldAccessGenerator.ShadowsPromotedMethod(array, "Len"), "ArrayType.Len shadows the Len its embedded Type's holder declares");
        Assert.IsFalse(FieldAccessGenerator.ShadowsPromotedMethod(array, "Elem"), "control: a sibling field shadows nothing");
        Assert.IsFalse(FieldAccessGenerator.ShadowsPromotedMethod(unrelated, "Len"), "control: a type that embeds nothing shadows nothing");
    }
}
