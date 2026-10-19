// GoMethodSetTests.cs - Gbtc
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
/// Guards the generated method set (golib's IGoMethodSet, MethodSet.cs): every value type with exported Go methods
/// answers each one as a compiled delegate, unbound (receiver first) and bound (a method value), through T and through
/// *T as Go's method sets allow. GolibTests hold golib to taking the arms; this holds the generator to writing arms that
/// compile and behave as Go's method values do when executed.
/// </summary>
[TestClass]
public class GoMethodSetTests
{
    private const string Path = @"C:\go2cs\src\core\mtest\mtest.cs";

    // A plain struct with a value method, a pointer method emitted `this ref` (answered through RecvGenerator's ж twin),
    // an unexported method and a variadic one; a generic struct with a value and a pointer method over type parameters
    // named apart from the type's; and a struct named `file`, a contextual keyword the partial must escape (CS9056).
    private const string Package =
        """
        namespace go;

        [GoPackage("mtest")]
        public static partial class mtest_package
        {
            public partial struct Plain { public nint n; }
            public partial struct Pair<K, V> { public K k; public V v; }
            public partial struct @file { public nint size; }

            public static nint Get(this Plain p) => p.n;

            public static void Set(this ref Plain p, nint v) => p.n = v;

            public static nint get(this Plain p) => p.n;

            public static nint Sum(this Plain p, params Span<nint> more) => p.n + more.Length;

            public static @string String<K, V>(this Pair<K, V> p) => (@string)$"{p.k}={p.v}";

            public static nint Size(this ж<@file> f) => f.Value.size;

            public static B Swap<A, B>(this ref Pair<A, B> p, B v)
            {
                B old = p.v;
                p.v = v;
                return old;
            }

            // Executed by the test: each arm, as reflect asks for it.
            public static string Probe()
            {
                IGoMethodSet plain = default(Plain);
                ж<Plain> box = Ꮡ(new Plain { n = 3 });

                nint unboundT = ((Func<Plain, nint>)plain.ᴛMethod("Get", false, false, null)!)(new Plain { n = 1 });
                nint unboundP = ((Func<ж<Plain>, nint>)plain.ᴛMethod("Get", true, false, null)!)(box);

                // A method value through *T copies the pointee at the bind (Go's rule): a later Set does not reach it.
                Func<nint> boundP = (Func<nint>)plain.ᴛMethod("Get", true, true, box)!;
                ((Action<ж<Plain>, nint>)plain.ᴛMethod("Set", true, false, null)!)(box, 4);
                Action<nint> set = (Action<nint>)plain.ᴛMethod("Set", true, true, box)!;
                set(5);

                IGoMethodSet pair = default(Pair<@string, nint>);
                ж<Pair<@string, nint>> pairBox = Ꮡ(new Pair<@string, nint> { k = "z", v = 26 });
                @string text = ((Func<@string>)pair.ᴛMethod("String", false, true, pairBox.Value)!)();
                nint old = ((Func<nint, nint>)pair.ᴛMethod("Swap", true, true, pairBox)!)(7);

                return $"{unboundT} {unboundP} {boundP()} {box.Value.n} {text} {old} {pairBox.Value.v} " +
                       $"{plain.ᴛMethod("Set", false, false, null) is null} {plain.ᴛMethod("get", false, false, null) is null} " +
                       $"{plain.ᴛMethod("Sum", false, false, null) is null} {pair.ᴛMethod("Swap", false, true, pairBox.Value) is null}";
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

        CSharpCompilation compilation = CSharpCompilation.Create("mtest",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\mtest\obj\mtest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        // RecvGenerator runs too, so the ж twins the pointer arms call exist to resolve against.
        CSharpGeneratorDriver.Create(new TypeGenerator(), new RecvGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        Dictionary<string, string> sources = output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => tree.FilePath.EndsWith(".MethodSet.g.cs", StringComparison.Ordinal))
            .ToDictionary(tree => System.IO.Path.GetFileName(tree.FilePath), tree => tree.ToString());

        // As in GoMethodSourceTests: the type's main generated part is left out (this harness's Roslyn cannot resolve its
        // ToString's string.Join over the net10 reference assemblies); the real builds compile it.
        Compilation executed = compilation.AddSyntaxTrees(output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => tree.FilePath.EndsWith(".MethodSet.g.cs", StringComparison.Ordinal) ||
                           tree.FilePath.EndsWith(".MethodSource.g.cs", StringComparison.Ordinal) ||
                           tree.FilePath.Contains("go2cs.RecvGenerator")));

        Diagnostic[] errors = [.. executed.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];

        return (sources, errors, executed);
    }

    [TestMethod]
    public void EachExportedMethodIsACompiledDelegateUnboundAndBound()
    {
        (Dictionary<string, string> sources, Diagnostic[] errors, Compilation output) = Run();

        Assert.AreEqual(0, errors.Length, "the package, its method sets and the ж twins compile: " + string.Join("; ", errors.Take(3)));
        Assert.AreEqual(3, sources.Count, "Plain, Pair and file each carry a method set");
        StringAssert.Contains(sources.Single(source => source.Key.Contains("Plain")).Value, "partial struct Plain : global::go.IGoMethodSet");

        using MemoryStream image = new();
        Assert.IsTrue(output.Emit(image).Success, "the package emits an assembly");

        System.Reflection.MethodInfo probe = Assembly.Load(image.ToArray()).GetType("go.mtest_package", throwOnError: true)!.GetMethod("Probe")!;

        // unbound over T and *T; the *T method value read 3 at its bind; Set through the unbound func then the bound one;
        // the generic pair; then no arm for a pointer method through T, an unexported method, a variadic one, or Swap through T.
        Assert.AreEqual("1 3 3 5 z=26 26 7 True True True True", probe.Invoke(null, null));
    }
}
