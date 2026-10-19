// GoMethodSourceTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards the generated method source (golib's IGoMethodSource, MethodSource.cs): every GENERIC value type with Go methods
/// answers each method closed over its own type parameters, through a delegate compiled code creates, so golib never
/// needs MakeGenericMethod to bind a generic type's method (Native AOT finds code only for an instantiation it compiled).
/// GolibTests' GenericMethodSourceTests holds golib to taking the source; this holds the generator to writing one that
/// compiles and answers correctly when executed.
/// </summary>
[TestClass]
public class GoMethodSourceTests
{
    private const string Path = @"C:\go2cs\src\core\stest\stest.cs";

    // Two type parameters, a value method, and a pointer method emitted `this ref` (answered by RecvGenerator's ж twin),
    // declared over type parameters named apart from the type's, as Go allows (`func (p *Pair[A, B]) Swap(v B) B`).
    private const string Package =
        """
        using System.Reflection;

        namespace go;

        [GoPackage("stest")]
        public static partial class stest_package
        {
            public partial struct Pair<K, V> { public K k; public V v; }
            public partial struct Plain { public nint n; }

            public static @string String<K, V>(this Pair<K, V> p) => (@string)$"{p.k}={p.v}";

            public static B Swap<A, B>(this ref Pair<A, B> p, B v)
            {
                B old = p.v;
                p.v = v;
                return old;
            }

            public static nint Get(this Plain p) => p.n;

            // Executed by the test: the source's methods, closed over <@string, nint>, called on a value and on a box.
            public static string Probe()
            {
                IGoMethodSource source = default(Pair<@string, nint>);
                MethodInfo text = source.GoMethod("String", false)!;
                MethodInfo swap = source.GoMethod("Swap", true)!;
                ж<Pair<@string, nint>> box = Ꮡ(new Pair<@string, nint> { k = "z", v = 26 });
                object? old = swap.Invoke(null, [box, (nint)5]);

                return $"{text.Invoke(null, [box.Value])} {old} {box.Value.v} {text.ContainsGenericParameters} {swap.GetParameters()[0].ParameterType.Name} " +
                       $"{source.GoMethod("Swap", false) is null} {source.GoMethod("Missing", false) is null}";
            }
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

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private static (Dictionary<string, string> sources, Diagnostic[] errors, Compilation output) Run()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("stest",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\stest\obj\stest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        // RecvGenerator runs too, so the ж<Pair> twin the pointer arm names exists to resolve against.
        CSharpGeneratorDriver.Create(new TypeGenerator(), new RecvGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        Dictionary<string, string> sources = output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => tree.FilePath.EndsWith(".MethodSource.g.cs", StringComparison.Ordinal))
            .ToDictionary(tree => System.IO.Path.GetFileName(tree.FilePath), tree => tree.ToString());

        // The executed compilation is the package with the parts under test: the method source, and RecvGenerator's ж
        // twins it names. The type's main generated part is left out, because this harness's Roslyn cannot resolve its
        // ToString's string.Join over the net10 reference assemblies (CS0121); the real builds compile it.
        Compilation executed = compilation.AddSyntaxTrees(output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => tree.FilePath.EndsWith(".MethodSource.g.cs", StringComparison.Ordinal) || tree.FilePath.Contains("go2cs.RecvGenerator")));

        Diagnostic[] errors = [.. executed.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];

        return (sources, errors, executed);
    }

    [TestMethod]
    public void AGenericTypesSourceAnswersEachMethodClosedOverItsOwnParameters()
    {
        (Dictionary<string, string> sources, Diagnostic[] errors, Compilation output) = Run();

        Assert.AreEqual(0, errors.Length, "the package, its method source and the ж twins compile: " + string.Join("; ", errors.Take(3)));

        string source = sources.Single().Value;
        StringAssert.Contains(source, "partial struct Pair<K, V> : global::go.IGoMethodSource");
        StringAssert.Contains(source, "private delegate V ᴛGoMethod1(global::go.ж<global::go.stest_package.Pair<K, V>> recv, V arg1);",
            "the pointer method is named through its ж twin, over the TYPE's parameters");
        StringAssert.Contains(source, "(\"Swap\", true) => ((ᴛGoMethod1)global::go.stest_package.Swap<K, V>).Method,");

        using MemoryStream image = new();
        Assert.IsTrue(output.Emit(image).Success, "the package emits an assembly");

        System.Reflection.MethodInfo probe = Assembly.Load(image.ToArray()).GetType("go.stest_package", throwOnError: true)!.GetMethod("Probe")!;

        Assert.AreEqual("z=5 26 5 False ж`1 True True", probe.Invoke(null, null));
    }

    [TestMethod]
    public void ANonGenericTypeCarriesNoSource()
    {
        Assert.IsFalse(Run().sources.Keys.Any(name => name.Contains("Plain")), "Plain is not generic");
    }
}
