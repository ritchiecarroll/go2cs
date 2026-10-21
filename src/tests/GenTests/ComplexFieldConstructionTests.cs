// ComplexFieldConstructionTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A struct's complex-typed fields through every member the TypeGenerator writes that constructs a field: the
/// NilType and parameterless zero constructors, the field-wise constructor, equality and hashing. The i9's windows
/// read of text/template/parse's test host at eb6def6292 failed CS7036 twice in its generated numberTest, whose
/// EMBEDDED <c>complex128</c> field (Go's <c>type numberTest struct { ...; complex128 }</c>) both zero
/// constructors built as <c>new global::System.Numerics.Complex(nil)</c>: a constructor System.Numerics.Complex
/// does not have. Go's zero complex is <c>0+0i</c>, which <c>default</c> already is, for complex128 and for golib's
/// complex64 alike.
/// </summary>
[TestClass]
public class ComplexFieldConstructionTests
{
    private const string Path = @"C:\go2cs\src\core\ctest\ctest.cs";

    // The converter's own shapes: a plain field and an embedded one (`/*embed*/`, named for its type) of each
    // complex kind, beside the embedded basic types numberTest carries, and a struct a constant composite literal
    // fills (parse_test.go's numberTests table).
    private const string Package =
        """
        namespace go;

        [GoPackage("ctest")]
        public static partial class ctest_package
        {
            partial struct Plain128 { internal complex128 c; }

            partial struct Plain64 { internal complex64 c; }

            partial struct numberTest
            {
                internal @string text;
                internal bool isComplex;
                /*embed*/ internal int64 int64;
                /*embed*/ internal uint64 uint64;
                /*embed*/ internal float64 float64;
                /*embed*/ internal complex128 complex128;
            }

            partial struct embeds64
            {
                /*embed*/ internal float32 float32;
                /*embed*/ internal complex64 complex64;
            }

            internal static slice<numberTest> numberTests = new numberTest[]{
                new("0"u8, false, 0, 0, 0D, 0D),
                new("1i"u8, true, 0, 0, 0D, new complex128(0, 1)),
            };

            internal static embeds64 zero64 = new embeds64(nil);

            internal static embeds64 one64 = new embeds64(1F, new complex64(1F, 2F));
        }
        """;

    private const string TemplateUsings =
        """
        global using static go.builtin;
        global using System;
        global using System.Numerics;
        global using any = System.Object;
        global using complex128 = System.Numerics.Complex;
        global using float32 = System.Single;
        global using float64 = System.Double;
        global using int64 = System.Int64;
        global using uint64 = System.UInt64;
        """;

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(System.IO.Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.GoTypeAttribute).Assembly.Location));
    }

    // The package with the TypeGenerator's output added, and the generated part of each struct by name.
    private static (Compilation Output, Dictionary<string, string> Parts) Generate()
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("ctest",
            [CSharpSyntaxTree.ParseText(Package, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\ctest\obj\ctest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(new TypeGenerator())
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out ImmutableArray<Diagnostic> _);

        Dictionary<string, string> parts = new();

        foreach (string name in new[] { "Plain128", "Plain64", "numberTest", "embeds64" })
            parts[name] = output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()).Single(source => source.Contains($" partial struct {name} "));

        return (output, parts);
    }

    [TestMethod]
    public void AComplexFieldIsNeverConstructedFromNil()
    {
        (_, Dictionary<string, string> parts) = Generate();

        foreach ((string name, string part) in parts)
        {
            Assert.IsFalse(part.Contains("System.Numerics.Complex(nil)"),
                $"{name}: System.Numerics.Complex has no NilType constructor (CS7036), and its default is Go's zero complex128");
            Assert.IsFalse(part.Contains("complex64(nil)"),
                $"{name}: golib's complex64 default is Go's zero complex64; it needs no construction");
        }
    }

    [TestMethod]
    public void EveryGeneratedMemberOverAComplexFieldCompiles()
    {
        (Compilation output, _) = Generate();

        Diagnostic[] errors = output.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.AreEqual(0, errors.Length, "the package and its generated members must compile:\n" + string.Join("\n", errors.Select(error => error.ToString())));
    }
}
