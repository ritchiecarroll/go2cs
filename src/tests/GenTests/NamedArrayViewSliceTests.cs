// NamedArrayViewSliceTests.cs - Gbtc
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
/// A defined array type two or more definitions deep, such as runtime's <c>type PallocBits pallocBits</c> over
/// <c>type pallocBits pageBits</c> over <c>type pageBits [8]uint64</c>, is generated with the array-VIEW template.
/// That template had the element indexers but none of the slice members the direct array template has: the
/// <c>Range</c> indexer, <c>slice(low)</c>, <c>slice(low, high)</c>, <c>slice(low, high, max)</c> and
/// <c>Slice(start, length)</c>. So Go's <c>want[:][2]</c> (runtime's mpallocbits_test.go) read <c>[..]</c> against
/// an nint indexer, CS1503 ×9, and the whole runtime test row did not build. Both templates now carry the full
/// set, and the view forwards each through its view, so the result shares the wrapper's backing.
/// </summary>
[TestClass]
public class NamedArrayViewSliceTests
{
    private const string FilePath = @"C:\go2cs\src\core\vtest\vtest.cs";

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

    private static (Compilation output, Dictionary<string, string> generated) Run(string declarations)
    {
        string source =
            $$"""
            namespace go;

            [GoPackage("vtest")]
            public static partial class vtest_package
            {
            {{declarations}}
            }
            """;

        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("vtest",
            [CSharpSyntaxTree.ParseText(source, options, path: FilePath), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\vtest\obj\vtest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        Dictionary<string, string> generated = output.SyntaxTrees.Except(compilation.SyntaxTrees)
            .ToDictionary(tree => System.IO.Path.GetFileName(tree.FilePath), tree => tree.ToString());

        return (output, generated);
    }

    [TestMethod]
    public void ADefinedArrayChainSlicesLikeADirectOne()
    {
        (Compilation output, Dictionary<string, string> generated) = Run(
            """
                [GoType("[8]uint64")] partial struct pageBits;
                [GoType("pageBits")] partial struct pallocBits;
                [GoType("pallocBits")] partial struct PallocBits;

                // Go's `want[:][2] = v`, `b[1:3]`, `b[n:]` and `b[:2:4]` over the chain, value and pointer forms.
                internal static void Probe(ref PallocBits want, PallocBits b, nint n, ж<PallocBits> p) {
                    want[..][2] = 0x3e;
                    slice<uint64> s = b[1..3];
                    slice<uint64> t = b.slice(n);
                    slice<uint64> u = b.slice(0, 2, 4);
                    p.Value[..][3] = 7;
                    slice<uint64> w = p.Value.slice(1, 2, 8);
                }
            """);

        foreach (string name in (string[])["pallocBits", "PallocBits"])
        {
            // Matched on the declaration, not the file name: PallocBits and pallocBits differ only in case, and
            // the generator disambiguates the second file (`PallocBits.1.g.cs`).
            string? part = generated.Values.FirstOrDefault(text => text.Contains($"partial struct {name} :", StringComparison.Ordinal));

            Assert.IsNotNull(part, $"no generated part for {name}; parts: " + string.Join(", ", generated.Keys));

            foreach (string member in (string[])[
                "public slice<uint64> this[global::System.Range range] => view[range];",
                "public slice<uint64> slice(nint low) => view.slice(low);",
                "public slice<uint64> slice(nint low, nint high) => view.slice(low, high);",
                "public slice<uint64> slice(nint low, nint high, nint max) => view.slice(low, high, max);",
                "public slice<uint64> Slice(nint start, nint length) => view.Slice(start, length);"])
            {
                StringAssert.Contains(part, member, $"the view type {name} does not forward a slice member");
            }
        }

        StringAssert.Contains(generated.Single(pair => pair.Key.EndsWith(".pageBits.g.cs", StringComparison.Ordinal)).Value,
            "public slice<uint64> slice(nint low, nint high, nint max) => Value.slice(low, high, max);",
            "the direct array template lacks the 3-index slice a pointer to it is sliced with");

        Diagnostic[] errors = output.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree?.FilePath == FilePath)
            .ToArray();

        Assert.AreEqual(0, errors.Length, "the slice forms do not compile over the chain:\n" + string.Join("\n", errors.Select(error => error.ToString())));
    }
}
