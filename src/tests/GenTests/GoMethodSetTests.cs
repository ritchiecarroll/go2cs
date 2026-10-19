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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Guards the generated method set (golib's IGoMethodSet, MethodSet.cs): every value type with exported Go methods
/// answers each one as a compiled delegate, unbound (receiver first) and bound (a method value), through T and through
/// *T as Go's method sets allow, promoted methods, variadic tails and more than 16 parameters included. GolibTests hold
/// golib to taking the arms; this holds the generator to writing arms that compile and behave as Go's method values do
/// when executed.
/// </summary>
[TestClass]
public class GoMethodSetTests
{
    private const string Path = @"C:\go2cs\src\core\mtest\mtest.cs";

    // A plain struct with a value method, a pointer method emitted `this ref` (answered through RecvGenerator's ж twin),
    // an unexported method, a variadic one and one with 16 parameters; a generic struct with a value and a pointer method
    // over type parameters named apart from the type's; a struct named `file`, a contextual keyword the partial must
    // escape (CS9056); and Outer, which declares no method and promotes Inner's value method and pointer method.
    private const string Package =
        """
        namespace go;

        [GoPackage("mtest")]
        public static partial class mtest_package
        {
            public partial struct Plain { public nint n; }
            public partial struct Pair<K, V> { public K k; public V v; }
            public partial struct @file { public nint size; }
            [GoType] public partial struct Inner { public nint i; }
            [GoType] public partial struct Outer
            {
                /*embed*/ public Inner Inner;
                public nint o;
            }

            public static nint Get(this Plain p) => p.n;

            public static void Set(this ref Plain p, nint v) => p.n = v;

            public static nint get(this Plain p) => p.n;

            public static nint Sum(this Plain p, params Span<nint> more) => p.n + more.Length;

            public static nint Wide(this Plain p, nint a1, nint a2, nint a3, nint a4, nint a5, nint a6, nint a7, nint a8,
                nint a9, nint a10, nint a11, nint a12, nint a13, nint a14, nint a15, nint a16) => p.n * 100 + a1 + a16;

            public static nint Twice(this Inner x) => x.i * 2;

            public static void Bump(this ref Inner x) => x.i++;

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

                // A variadic tail takes golib's ꓸꓸꓸ family; 16 parameters plus the receiver take golib's ladder past the BCL's.
                nint sum = ((Funcꓸꓸꓸ<Plain, nint, nint>)plain.ᴛMethod("Sum", false, false, null)!)(new Plain { n = 1 }, 5, 6);
                nint sumBound = ((Funcꓸꓸꓸ<nint, nint>)plain.ᴛMethod("Sum", true, true, box)!)();
                nint wide = ((global::go.Func<Plain, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint>)
                    plain.ᴛMethod("Wide", false, false, null)!)(new Plain { n = 2 }, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16);
                nint wideBound = ((Func<nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint, nint>)
                    plain.ᴛMethod("Wide", false, true, new Plain { n = 3 })!)(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16);

                // Promoted through the value embed: Twice in T's and *T's sets, Bump (a pointer method) in *T's alone.
                IGoMethodSet outer = default(Outer);
                ж<Outer> outerBox = Ꮡ(new Outer { Inner = new Inner { i = 4 } });
                nint twice = ((Func<Outer, nint>)outer.ᴛMethod("Twice", false, false, null)!)(new Outer { Inner = new Inner { i = 5 } });
                ((Action)outer.ᴛMethod("Bump", true, true, outerBox)!)();
                nint twiceP = ((Func<ж<Outer>, nint>)outer.ᴛMethod("Twice", true, false, null)!)(outerBox);

                return $"{unboundT} {unboundP} {boundP()} {box.Value.n} {text} {old} {pairBox.Value.v} " +
                       $"{plain.ᴛMethod("Set", false, false, null) is null} {plain.ᴛMethod("get", false, false, null) is null} " +
                       $"{pair.ᴛMethod("Swap", false, true, pairBox.Value) is null} " +
                       $"{sum} {sumBound} {wide} {wideBound} {twice} {twiceP} {outerBox.Value.Inner.i} {outer.ᴛMethod("Bump", false, false, null) is null}";
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
        // ToString's string.Join over the net10 reference assemblies); the real builds compile it. Its promoted-method
        // forwarders, which the promoted arms call, are taken from it alone.
        Compilation executed = compilation.AddSyntaxTrees(output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => tree.FilePath.EndsWith(".MethodSet.g.cs", StringComparison.Ordinal) ||
                           tree.FilePath.EndsWith(".MethodSource.g.cs", StringComparison.Ordinal) ||
                           tree.FilePath.Contains("go2cs.RecvGenerator"))
            .Append(Forwarders(output.SyntaxTrees.Except(compilation.SyntaxTrees), options)));

        Diagnostic[] errors = [.. executed.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];

        return (sources, errors, executed);
    }

    // The package class's static extension methods from the TypeGenerator's main parts: the promoted forwarders.
    private static SyntaxTree Forwarders(IEnumerable<SyntaxTree> generated, CSharpParseOptions options)
    {
        IEnumerable<MethodDeclarationSyntax> forwarders = generated
            .Where(tree => tree.FilePath.Contains("go2cs.TypeGenerator") && !tree.FilePath.EndsWith(".MethodSet.g.cs", StringComparison.Ordinal) &&
                           !tree.FilePath.EndsWith(".MethodSource.g.cs", StringComparison.Ordinal))
            .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>())
            .Where(method => method.Parent is ClassDeclarationSyntax && method.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                             method.ParameterList.Parameters.FirstOrDefault()?.Modifiers.Any(SyntaxKind.ThisKeyword) == true);

        return CSharpSyntaxTree.ParseText($"namespace go;\npublic static partial class mtest_package\n{{\n{string.Join("\n", forwarders.Select(method => method.ToFullString()))}\n}}\n",
            options, path: @"C:\go2cs\src\core\mtest\obj\forwarders.g.cs");
    }

    [TestMethod]
    public void EachExportedMethodIsACompiledDelegateUnboundAndBound()
    {
        (Dictionary<string, string> sources, Diagnostic[] errors, Compilation output) = Run();

        Assert.AreEqual(0, errors.Length, "the package, its method sets and the ж twins compile: " + string.Join("; ", errors.Take(3)));
        Assert.AreEqual(5, sources.Count, "Plain, Pair, file, Inner and Outer each carry a method set");
        StringAssert.Contains(sources.Single(source => source.Key.Contains("Outer")).Value, "case (\"Bump\", true):", "Outer's set holds its promoted pointer method");
        StringAssert.Contains(sources.Single(source => source.Key.Contains("Plain")).Value, "partial struct Plain : global::go.IGoMethodSet");

        using MemoryStream image = new();
        Assert.IsTrue(output.Emit(image).Success, "the package emits an assembly");

        System.Reflection.MethodInfo probe = Assembly.Load(image.ToArray()).GetType("go.mtest_package", throwOnError: true)!.GetMethod("Probe")!;

        // unbound over T and *T; the *T method value read 3 at its bind; Set through the unbound func then the bound one;
        // the generic pair; no arm for a pointer method through T, an unexported method, or Swap through T; the variadic
        // tail unbound (1+2) and bound to the box (5+0); 16 parameters unbound (200+1+16) and bound (300+1+16); Outer's
        // promoted Twice through T (10), Bump through *T then Twice through *T (5*2), Inner.i now 5, and no Bump through T.
        Assert.AreEqual("1 3 3 5 z=26 26 7 True True True 3 5 217 317 10 10 5 True", probe.Invoke(null, null));
    }
}
