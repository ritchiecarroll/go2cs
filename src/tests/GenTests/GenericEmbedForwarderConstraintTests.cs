// GenericEmbedForwarderConstraintTests.cs - Gbtc
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Go 1.26's <c>internal/runtime/gc/internal/gen</c> declares <c>type Array[T Value] struct{ valAny }</c>: a GENERIC
/// struct with a CONSTRAINED type parameter that embeds a value type. The TypeGenerator promotes the embed's methods
/// as generic extension methods carrying the struct's own type parameters (<c>initOp&lt;T&gt;(this ref Array&lt;T&gt;
/// target, …)</c>) but not its where clauses, so every <c>Array&lt;T&gt;</c> in a forwarder's signature names a
/// <c>T</c> that does not satisfy <c>where T : Value</c>: CS0314, 22 of them in gen on every flavour. The
/// forwarders now repeat the struct's constraints, as the ImplementGenerator's adapter for a generic struct already
/// does.
/// </summary>
[TestClass]
public class GenericEmbedForwarderConstraintTests
{
    private const string FilePath = @"C:\go2cs\src\core\utest\utest.cs";

    private const string Source =
        """
        namespace go;

        [GoPackage("utest")]
        public static partial class utest_package
        {
            public interface Value { }

            partial struct valAny {
                internal nint id;
            }

            // A pointer-receiver method in the ref form and a value-receiver method, as gen's valAny has.
            internal static void initOp(this ref valAny v, nint id) { }

            internal static nint getOp(this valAny v) => v.id;

            partial struct Array<T>
                where T : Value
            {
                internal partial ref valAny valAny { get; }
            }

            internal static nint probe<T>(ж<Array<T>> a)
                where T : Value
            {
                a.Value.initOp(1);
                return a.Value.getOp();
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
    public void AGenericStructsPromotedForwardersCarryItsConstraints()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("utest",
            [CSharpSyntaxTree.ParseText(Source, options, path: FilePath), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\utest\obj\utest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        SyntaxTree[] arrayTrees = output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => tree.FilePath.Contains("Array", StringComparison.Ordinal))
            .ToArray();

        string array = string.Join("\n", arrayTrees.Select(tree => tree.ToString()));

        Assert.IsTrue(array.Length > 0, "the generator emitted no part for Array<T>");

        // The forwarders exist (the subject is their constraints, not their presence).
        StringAssert.Contains(array, "initOp<T>(this ref Array<T> target", "the embed's ref-receiver initOp is not promoted:\n" + array);
        StringAssert.Contains(array, "getOp<T>(this Array<T> target", "the embed's value-receiver getOp is not promoted:\n" + array);

        // Every forwarder over Array<T> compiles: the generated part reports no CS0314. GenTests' Roslyn (4.10,
        // C# 12) has no partial properties, so the embed declarations themselves report CS0267/CS8145 here, as
        // in every sibling fixture; CS0314 is the code that names this defect.
        Diagnostic[] unsatisfied = output.GetDiagnostics()
            .Where(diagnostic => diagnostic.Id == "CS0314" && arrayTrees.Contains(diagnostic.Location.SourceTree))
            .ToArray();

        Assert.AreEqual(0, unsatisfied.Length, "a promoted forwarder names Array<T> without Array's constraint:\n" + string.Join("\n", unsatisfied) + "\n" + array);
    }
}
