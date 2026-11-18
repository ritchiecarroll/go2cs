// ForeignPairStructConversionTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// A Go conversion between two struct types that are BOTH declared in other packages, <c>lib.Pitch(o)</c>
/// where <c>o</c> is an <c>other.Pitch</c>, has no C# operator: an operator is declared in one of its two
/// operand types, and a type of a referenced assembly cannot be reopened. The record
/// <c>GoImplicitConv&lt;other.Pitch, lib.Pitch&gt;</c> makes the ImplicitConvGenerator write a static
/// FUNCTION in the converting package's class instead, named as Go names the target and overloaded by the
/// source type, and the converter calls it: <c>libꓸPitch(o)</c>.
/// </summary>
/// <remarks>
/// The upstream assembly grants no InternalsVisibleTo, so an <c>internal</c> field there is what an
/// unexported field of another package is to a converted project: out of reach.
/// </remarks>
[TestClass]
public class ForeignPairStructConversionTests
{
    private const string Attributes =
        """
        namespace go
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public class GoImplicitConvAttribute<TSource, TTarget> : System.Attribute
            {
                public bool Inverted { get; set; }
                public bool Indirect { get; set; }
                public string? ValueType { get; set; }
            }
        }
        """;

    private const string Upstream =
        """
        namespace go
        {
            public static partial class lib_package
            {
                public partial struct Pitch
                {
                    public nint Hz;

                    public Pitch(nint Hz = default!) { this.Hz = Hz; }
                }

                public partial struct Span
                {
                    public nint Lo;
                    public @string Hi;

                    public Span(NilType _) { }
                    public Span(nint Lo = default!, @string Hi = default!) { this.Lo = Lo; this.Hi = Hi; }
                }

                public partial struct Unit
                {
                }

                public partial struct Sealed
                {
                    public nint Hz;
                    internal nint serial;

                    public Sealed(nint Hz = default!, nint serial = default!) { this.Hz = Hz; this.serial = serial; }
                }

                public partial struct Wax
                {
                    public nint Hz;
                    internal nint serial;

                    public Wax(nint Hz = default!, nint serial = default!) { this.Hz = Hz; this.serial = serial; }
                }
            }

            public static partial class other_package
            {
                public partial struct Pitch
                {
                    public nint Hz;

                    public Pitch(nint Hz = default!) { this.Hz = Hz; }
                }

                public partial struct Span
                {
                    public nint Lo;
                    public @string Hi;

                    public Span(nint Lo = default!, @string Hi = default!) { this.Lo = Lo; this.Hi = Hi; }
                }

                public partial struct Unit
                {
                }
            }

            public static partial class third_package
            {
                public partial struct Pitch
                {
                    public nint Hz;

                    public Pitch(nint Hz = default!) { this.Hz = Hz; }
                }
            }

            public struct @string { }

            public struct NilType { }
        }
        """;

    // The body of a consumer's package class: the records come first, as package_info.cs holds them.
    private static string Consumer(string records, string members, string usings = "") =>
        $$"""
        using go;
        {{usings}}

        {{records}}

        namespace go;

        public static partial class main_package
        {
        {{members}}
        }
        """;

    [TestMethod]
    public void TwoForeignStructsConvertThroughAFunctionNamedAsGoNamesTheTarget()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.other_package.Pitch, go.lib_package.Pitch>]",
            "    internal static nint Convert(other_package.Pitch value) => libꓸPitch(value).Hz;"));

        Assert.AreEqual(1, run.GeneratedSources.Length, "the record of two foreign structs must generate its function");
        Assert.AreEqual(0, errors.Length, $"the call must compile with the generated function: {Text(errors)}");

        string generated = run.GeneratedSources[0].SourceText.ToString();

        StringAssert.Contains(generated, "internal static global::go.lib_package.Pitch libꓸPitch(global::go.other_package.Pitch src) => new global::go.lib_package.Pitch(src.Hz);");
        Assert.IsFalse(generated.Contains("operator"), "no operator can be declared for two foreign types");
        Assert.IsFalse(generated.Contains("partial struct"), "neither foreign type may be redeclared here");
    }

    // One name per target, overloaded by the source type: Go writes `lib.Pitch(o)` for every source.
    [TestMethod]
    public void SourcesFromTwoPackagesOverloadTheTargetsOneName()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            """
            [assembly: GoImplicitConv<go.other_package.Pitch, go.lib_package.Pitch>]
            [assembly: GoImplicitConv<go.third_package.Pitch, go.lib_package.Pitch>]
            """,
            """
                internal static nint FromOther(other_package.Pitch value) => libꓸPitch(value).Hz;
                internal static nint FromThird(third_package.Pitch value) => libꓸPitch(value).Hz;
            """));

        Assert.AreEqual(2, run.GeneratedSources.Length, "each source type has its own overload");
        Assert.AreEqual(0, errors.Length, $"both calls must compile: {Text(errors)}");
    }

    // The two directions of one pair are two functions, each named after its own target.
    [TestMethod]
    public void BothDirectionsOfOnePairEachGetTheirFunction()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            """
            [assembly: GoImplicitConv<go.other_package.Pitch, go.lib_package.Pitch>]
            [assembly: GoImplicitConv<go.lib_package.Pitch, go.other_package.Pitch>]
            """,
            "    internal static nint RoundTrip(other_package.Pitch value) => otherꓸPitch(libꓸPitch(value)).Hz;"));

        Assert.AreEqual(2, run.GeneratedSources.Length, "each direction has its own function");
        Assert.AreEqual(0, errors.Length, $"the round trip must compile: {Text(errors)}");
    }

    // A record repeated (one per file that converts the pair) is one function.
    [TestMethod]
    public void ARepeatedRecordGeneratesOneFunction()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            """
            [assembly: GoImplicitConv<go.other_package.Pitch, go.lib_package.Pitch>]
            [assembly: GoImplicitConv<go.other_package.Pitch, go.lib_package.Pitch>]
            """,
            "    internal static nint Convert(other_package.Pitch value) => libꓸPitch(value).Hz;"));

        Assert.AreEqual(1, run.GeneratedSources.Length, "a repeated record must not declare the function twice (CS0111)");
        Assert.AreEqual(0, errors.Length, $"the call must compile: {Text(errors)}");
    }

    // The members are the target constructor's parameters, in its order, and the constructor is the one
    // that takes every field: the nil constructor beside it is not a candidate.
    [TestMethod]
    public void EveryFieldIsCopiedInTheOrderOfTheTargetsConstructor()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.other_package.Span, go.lib_package.Span>]",
            "    internal static nint Convert(other_package.Span value) => libꓸSpan(value).Lo;"));

        Assert.AreEqual(1, run.GeneratedSources.Length, "the record must generate its function");
        Assert.AreEqual(0, errors.Length, $"the call must compile: {Text(errors)}");
        StringAssert.Contains(run.GeneratedSources[0].SourceText.ToString(), "=> new global::go.lib_package.Span(src.Lo, src.Hi);");
    }

    [TestMethod]
    public void AnEmptyStructPairIsBuiltWithNoArguments()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.other_package.Unit, go.lib_package.Unit>]",
            "    internal static lib_package.Unit Convert(other_package.Unit value) => libꓸUnit(value);"));

        Assert.AreEqual(1, run.GeneratedSources.Length, "the record must generate its function");
        Assert.AreEqual(0, errors.Length, $"the call must compile: {Text(errors)}");
        StringAssert.Contains(run.GeneratedSources[0].SourceText.ToString(), "=> new global::go.lib_package.Unit();");
    }

    // An import alias of the same spelling, `using libꓸPitch = …`, names the TYPE; the function of that
    // name is found where a call is written. Both are in scope in one file and neither hides the other.
    [TestMethod]
    public void TheFunctionCompilesBesideAnImportAliasOfItsOwnSpelling()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.other_package.Pitch, go.lib_package.Pitch>]",
            """
                internal static nint Convert(other_package.Pitch value)
                {
                    libꓸPitch converted = libꓸPitch(value);
                    libꓸPitch cast = (libꓸPitch)converted;
                    return converted.Hz + cast.Hz + new libꓸPitch(1).Hz;
                }
            """,
            "using libꓸPitch = go.lib_package.Pitch;"));

        Assert.AreEqual(1, run.GeneratedSources.Length, "the record must generate its function");
        Assert.AreEqual(0, errors.Length, $"the alias and the function must compile side by side: {Text(errors)}");
    }

    // An unexported field of another package is `internal` in its assembly: this assembly can neither read
    // it from the source nor pass it to the target, so there is no function to write, and nothing generated
    // fails to compile. Go allows this conversion only between two types of ONE package; its cure is an
    // operator in that package.
    [TestMethod]
    public void APairWithAFieldOutOfReachGeneratesNoFunction()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.lib_package.Wax, go.lib_package.Sealed>]",
            "    internal static nint Probe(lib_package.Wax value) => value.Hz;"));

        Assert.AreEqual(0, run.GeneratedSources.Length, "a pair with a field out of reach has no function");
        Assert.AreEqual(0, errors.Length, $"nothing generated may fail to compile: {Text(errors)}");
    }

    private static string Text(Diagnostic[] errors) => string.Join("; ", errors.Select(error => error.ToString()));

    private static (GeneratorRunResult run, Diagnostic[] errors) Run(string consumerSource)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        List<MetadataReference> references =
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"))
        ];

        foreach ((string name, string source) in new[] { ("pair-attributes", Attributes), ("pair-lib", Upstream) })
        {
            CSharpCompilation upstream = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source, parseOptions)], references, libraryOptions);

            using MemoryStream image = new();
            EmitResult emitted = upstream.Emit(image);
            Assert.IsTrue(emitted.Success, $"the upstream assembly {name} must emit clean: {string.Join("; ", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))}");
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
        }

        CSharpCompilation consumer = CSharpCompilation.Create("pair-main", [CSharpSyntaxTree.ParseText(consumerSource, parseOptions)], references, libraryOptions);

        CSharpGeneratorDriver.Create(new ImplicitConvGenerator())
            .RunGeneratorsAndUpdateCompilation(consumer, out Compilation updated, out _);

        GeneratorRunResult run = CSharpGeneratorDriver.Create(new ImplicitConvGenerator()).RunGenerators(consumer).GetRunResult().Results.Single();

        Assert.IsNull(run.Exception, $"the generator must not throw: {run.Exception}");

        return (run, [.. updated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)]);
    }
}
