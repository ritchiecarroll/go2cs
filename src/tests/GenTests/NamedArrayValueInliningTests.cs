// NamedArrayValueInliningTests.cs - Gbtc
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
/// P-256 R2 (COORD ruling 2026-10-22): a named ARRAY wrapper's <c>Value</c> getter is the warm path alone
/// (the published holder's load and a null test) marked AggressiveInlining, and the first touch, which
/// allocates the lazy backing and publishes it by CAS, is the out-of-line <c>materializeValue</c>, marked
/// NoInlining. The wrapper's <c>int</c>/<c>nint</c> element indexers inline with it. With the allocation in
/// the getter's body the JIT kept <c>get_Value</c> as a call on every element read: fiat's <c>p256Mul</c>
/// reads 24 limbs of its <c>[4]uint64</c> wrappers per multiply. The split must keep the door-2 publish
/// (docs/phase4/INVESTIGATION-element-aliasing.md §6a): racing first touches of one zero wrapper still
/// see ONE backing.
/// </summary>
[TestClass]
public class NamedArrayValueInliningTests
{
    private const string FilePath = @"C:\go2cs\src\core\atest\atest.cs";

    private const string AggressiveInlining = "global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining";

    private const string NoInlining = "global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining";

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
        global using uint64 = System.UInt64;
        """;

    // fe is fiat's shape, `type p256MontgomeryDomainFieldElement [4]uint64`. Race is the door-2 probe
    // (arm7's shape): `threads` threads released together each take the FIRST touch of one zero wrapper
    // held in shared storage, and every one of them must come back with the backing that stayed published.
    private const string Declarations =
        """
            [GoType("[4]uint64")] partial struct fe;

            public static int LostPublishes(int threads, int trials)
            {
                int lost = 0;

                for (int trial = 0; trial < trials; trial++)
                {
                    fe[] cell = new fe[1];
                    uint64[]?[] seen = new uint64[]?[threads];
                    using global::System.Threading.Barrier start = new(threads);
                    global::System.Threading.Thread[] workers = new global::System.Threading.Thread[threads];

                    for (int i = 0; i < threads; i++)
                    {
                        int worker = i;
                        workers[i] = new global::System.Threading.Thread(() =>
                        {
                            start.SignalAndWait();
                            seen[worker] = cell[0].Value.Source;
                        });
                        workers[i].Start();
                    }

                    foreach (global::System.Threading.Thread worker in workers)
                        worker.Join();

                    uint64[]? published = cell[0].Value.Source;

                    if (seen.Any(backing => !ReferenceEquals(backing, published)))
                        lost++;
                }

                return lost;
            }
        """;

    private static (string generated, Compilation output) Run()
    {
        string source =
            $$"""
            namespace go;

            [GoPackage("atest")]
            public static partial class atest_package
            {
            {{Declarations}}
            }
            """;

        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("atest",
            [CSharpSyntaxTree.ParseText(source, options, path: FilePath), CSharpSyntaxTree.ParseText(TemplateUsings + "\nglobal using System.Linq;", options, path: @"C:\go2cs\src\core\atest\obj\atest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string generated = string.Join("\n", output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .Where(tree => tree.FilePath.EndsWith("fe.g.cs", StringComparison.Ordinal))
            .Select(tree => tree.ToString()));

        return (generated, output);
    }

    // the members of every generated part of fe (the generator emits more than one)
    private static MemberDeclarationSyntax[] Wrapper(string generated) =>
        CSharpSyntaxTree.ParseText(generated).GetRoot().DescendantNodes().OfType<StructDeclarationSyntax>()
            .Where(type => type.Identifier.Text == "fe").SelectMany(type => type.Members).ToArray();

    private static bool Marked(AccessorDeclarationSyntax accessor, string option) =>
        accessor.AttributeLists.SelectMany(list => list.Attributes).Any(attribute => attribute.ToString().Contains(option, StringComparison.Ordinal));

    private static AccessorDeclarationSyntax Getter(BasePropertyDeclarationSyntax property) =>
        property.AccessorList?.Accessors.SingleOrDefault(accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration)) ??
        throw new AssertFailedException($"no block-bodied get accessor on {property}");

    [TestMethod]
    public void TheValueGetterIsTheWarmPathAloneAndInlines()
    {
        (string generated, _) = Run();
        Assert.IsTrue(generated.Length > 0, "the generator emitted no part for fe");

        PropertyDeclarationSyntax value = Wrapper(generated).OfType<PropertyDeclarationSyntax>().Single(property => property.Identifier.Text == "Value");
        AccessorDeclarationSyntax get = Getter(value);

        Assert.IsTrue(Marked(get, AggressiveInlining), "Value's getter is not AggressiveInlining:\n" + value);
        Assert.IsFalse(get.ToString().Contains("new ", StringComparison.Ordinal), "Value's getter still allocates in its own body:\n" + value);
        Assert.IsFalse(get.ToString().Contains("CompareExchange", StringComparison.Ordinal), "Value's getter still publishes in its own body:\n" + value);
        StringAssert.Contains(get.ToString(), "materializeValue()", "Value's getter does not reach the out-of-line first touch:\n" + value);
    }

    [TestMethod]
    public void TheFirstTouchIsOutOfLineAndStillPublishesByCas()
    {
        (string generated, _) = Run();

        MethodDeclarationSyntax materialize = Wrapper(generated).OfType<MethodDeclarationSyntax>().Single(method => method.Identifier.Text == "materializeValue");

        Assert.IsTrue(materialize.Modifiers.Any(SyntaxKind.PrivateKeyword), "materializeValue is not private:\n" + materialize);
        Assert.IsTrue(materialize.AttributeLists.SelectMany(list => list.Attributes).Any(attribute => attribute.ToString().Contains(NoInlining, StringComparison.Ordinal)),
            "materializeValue is not NoInlining, so its allocation can count against every caller's budget:\n" + materialize);
        StringAssert.Contains(materialize.ToString(), "global::System.Threading.Interlocked.CompareExchange(ref m_value, created, null) ?? created",
            "the first touch no longer publishes by CAS, which reopens door 2:\n" + materialize);
    }

    [TestMethod]
    public void TheIntAndNintElementIndexersInline()
    {
        (string generated, _) = Run();

        IndexerDeclarationSyntax[] indexers = Wrapper(generated).OfType<IndexerDeclarationSyntax>()
            .Where(indexer => indexer.ExplicitInterfaceSpecifier is null && indexer.ParameterList.Parameters.Single().Type?.ToString() is "int" or "nint").ToArray();

        Assert.AreEqual(2, indexers.Length, "fe does not declare one int and one nint element indexer:\n" + generated);

        foreach (IndexerDeclarationSyntax indexer in indexers)
            Assert.IsTrue(Marked(Getter(indexer), AggressiveInlining), "an element indexer is not AggressiveInlining:\n" + indexer);
    }

    [TestMethod]
    public void RacingFirstTouchesStillShareOneBacking()
    {
        (_, Compilation output) = Run();

        Diagnostic[] errors = output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.AreEqual(0, errors.Length, "the package does not compile: " + string.Join("; ", errors.Take(3)));

        using MemoryStream image = new();
        Assert.IsTrue(output.Emit(image).Success, "the package emits an assembly");

        System.Reflection.MethodInfo race = Assembly.Load(image.ToArray()).GetType("go.atest_package", throwOnError: true)!.GetMethod("LostPublishes")!;

        // the `??=` emission lost 872 of 900 such trials (arm7); a CAS publish loses none
        // Scope: this compilation takes Roslyn's default OptimizationLevel.Debug, so the JIT runs it
        // unoptimized and nothing inlines. The arm proves the split kept the CAS publish (door 2); it does
        // not prove the inlined warm read under the optimizing JIT. The perf canary is the only optimized
        // execution of that read.
        Assert.AreEqual(0, (int)race.Invoke(null, [16, 300])!, "a racing first touch kept a backing other than the published one");
    }
}
