// GenericPointerEmbedPromotionTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Go 1.26's <c>unique</c> embeds a GENERIC POINTER: <c>type uniqueMap[T comparable] struct{ *canonMap[T]; … }</c>,
/// and <c>unique.Make</c> calls the promoted <c>m.Load</c> / <c>m.LoadOrStore</c> on a <c>*uniqueMap[T]</c>. The
/// TypeGenerator promoted through it as if the embed were neither a pointer nor an instantiation: the field accessors
/// read <c>canonMap.root</c> off the <c>ж&lt;canonMap&lt;T&gt;&gt;</c> box (CS1061), the embed's type parameter
/// <c>T</c> was rewritten to the whole box's argument (<c>Δindirect&lt;canonMap&lt;T&gt;&gt;</c>), and the box-receiver
/// methods were not promoted at all (CS1929 at unique/handle.cs:49,52). A non-generic pointer embed and a generic
/// value embed were both right; only the combination was not.
/// </summary>
[TestClass]
public class GenericPointerEmbedPromotionTests
{
    private const string FilePath = @"C:\go2cs\src\core\utest\utest.cs";

    private const string Source =
        """
        namespace go;

        [GoPackage("utest")]
        public static partial class utest_package
        {
            partial struct cell<T> {
                internal T v;
            }

            partial struct canonMap<T> {
                internal ж<cell<T>> root;
                internal uintptr seed;
            }

            // A pointer-receiver method in the box form (unique's Load / LoadOrStore).
            internal static ж<T> Load<T>(this ж<canonMap<T>> Ꮡm, T key) => default!;

            // A pointer-receiver method in the ref form, whose signature names the embed's T (unique's expand).
            internal static nint grow<T>(this ref canonMap<T> m, ж<cell<T>> Ꮡc) => 0;

            partial struct uniqueMap<T> {
                internal partial ref ж<canonMap<T>> canonMap { get; }
            }

            internal static ж<T> probe<T>(ж<uniqueMap<T>> m, T key) {
                var p = m.Load(key);
                ж<cell<T>> r = m.Value.root;
                uintptr s = m.Value.seed;
                nint g = m.Value.grow(r);
                return p;
            }
        }
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        """;

    [TestMethod]
    public void AGenericPointerEmbedPromotesItsFieldsAndMethods()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("utest",
            [CSharpSyntaxTree.ParseText(Source, options, path: FilePath), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\utest\obj\utest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string uniqueMap = string.Join("\n", output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => tree.FilePath.Contains("uniqueMap", StringComparison.Ordinal))
            .Select(tree => tree.ToString()));

        Assert.IsTrue(uniqueMap.Length > 0, "the generator emitted no part for uniqueMap<T>");

        // (1) A promoted field descends the pointer embed's box, and (2) keeps the embed's own T.
        StringAssert.Contains(uniqueMap, "root => ref canonMap.Value.root;", "a promoted field descends the pointer embed's box:\n" + uniqueMap);
        StringAssert.Contains(uniqueMap, "global::go.ж<global::go.utest_package.cell<T>> root", "the promoted field keeps the embed's own T:\n" + uniqueMap);
        Assert.IsFalse(uniqueMap.Contains("cell<global::go.utest_package.canonMap<T>>", StringComparison.Ordinal),
            "the embed's T was rewritten to the box's own type argument:\n" + uniqueMap);

        // (3) The box-receiver method is promoted, bound on the box hop itself, and the ref-receiver one
        // through its deref.
        StringAssert.Contains(uniqueMap, "Load<T>(this uniqueMap<T> target, T key) => target.canonMap.Load(key);",
            "the embed's box-receiver Load is not promoted:\n" + uniqueMap);
        StringAssert.Contains(uniqueMap, "=> target.canonMap.Value.grow(Ꮡc);", "the ref-receiver grow does not descend the box:\n" + uniqueMap);

        // The uses compile. GenTests' Roslyn (4.10, C# 12) has no partial properties, so the embed
        // declarations themselves report CS0267/CS8145 here, as in every sibling fixture; the errors that
        // matter are the pass-5 ones, which land in the USING body.
        MethodDeclarationSyntax probe = output.SyntaxTrees.First(tree => tree.FilePath == FilePath).GetRoot()
            .DescendantNodes().OfType<MethodDeclarationSyntax>().Single(method => method.Identifier.Text == "probe");

        Diagnostic[] errors = output.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error &&
                diagnostic.Location.SourceTree?.FilePath == FilePath && probe.Body!.Span.Contains(diagnostic.Location.SourceSpan))
            .ToArray();

        Assert.AreEqual(0, errors.Length, "the promoted members do not bind where unique.Make uses them:\n" + string.Join("\n", errors));
    }
}
