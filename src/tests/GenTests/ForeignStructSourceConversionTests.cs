// ForeignStructSourceConversionTests.cs - Gbtc
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
/// A Go conversion from a struct declared in ANOTHER package to a LOCAL struct, <c>hertz(p)</c> where
/// <c>p</c> is a <c>lib.Pitch</c>, is recorded as <c>GoImplicitConv&lt;lib.Pitch, hertz&gt;</c>. A C#
/// conversion operator is declared in one of its two operand types and a type of a referenced assembly
/// cannot be reopened, so the operator is declared in the local target and takes the source by its
/// QUALIFIED name. The ImplicitConvGenerator declared it in the source's simple name, which is a second,
/// empty local type (CS1061 at the member reads, CS0030 at the cast), or, where the local target is the
/// less accessible side, in the target with the source named bare (CS0246).
/// </summary>
/// <remarks>
/// The accessibility of each local struct is written on its declaration, as the TypeGenerator's output
/// writes it in a converted project: <c>hertz</c> is the unexported arm and <c>Hertz</c> the exported one.
/// </remarks>
[TestClass]
public class ForeignStructSourceConversionTests
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

            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }
        }
        """;

    private const string Upstream =
        """
        [assembly: System.Runtime.CompilerServices.InternalsVisibleTo("foreign-main")]

        namespace go
        {
            public static partial class lib_package
            {
                public partial struct Pitch
                {
                    public nint Hz;

                    public Pitch(nint Hz = default!) { this.Hz = Hz; }
                }

                internal partial struct pitch
                {
                    public nint Hz;

                    public pitch(nint Hz = default!) { this.Hz = Hz; }
                }
            }

            public static partial class other_package
            {
                public partial struct Pitch
                {
                    public nint Hz;

                    public Pitch(nint Hz = default!) { this.Hz = Hz; }
                }
            }
        }
        """;

    // The body of a consumer's package class: the records come first, as package_info.cs holds them.
    private static string Consumer(string records, string members) =>
        $$"""
        using go;

        {{records}}

        namespace go;

        public static partial class main_package
        {
        {{members}}
        }
        """;

    private static string LocalStruct(string name, string access = "internal") =>
        $$"""
            [GoType] {{access}} partial struct {{name}}
            {
                public nint Hz;

                public {{name}}(nint Hz = default!) { this.Hz = Hz; }
            }
        """;

    [TestMethod]
    public void AnUnexportedLocalTargetTakesTheForeignSourceByItsQualifiedName()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.lib_package.Pitch, go.main_package.hertz>]",
            LocalStruct("hertz") + "\n    internal static nint Convert(lib_package.Pitch value) => ((hertz)value).Hz;"));

        Assert.AreEqual(1, run.GeneratedSources.Length, "the record from a foreign struct source must generate its operator");
        string generated = run.GeneratedSources[0].SourceText.ToString();
        StringAssert.Contains(generated, "partial struct hertz");
        StringAssert.Contains(generated, "public static implicit operator hertz(global::go.lib_package.Pitch src) => new hertz(src.Hz);");
        Assert.AreEqual(0, errors.Length, $"the cast must compile with the generated operator: {Text(errors)}");
    }

    [TestMethod]
    public void AnExportedLocalTargetHostsTheOperatorTheForeignSourceCannot()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.lib_package.Pitch, go.main_package.Hertz>]",
            LocalStruct("Hertz", "public") + "\n    internal static nint Convert(lib_package.Pitch value) => ((Hertz)value).Hz;"));

        Assert.AreEqual(1, run.GeneratedSources.Length, "the record from a foreign struct source must generate its operator");
        string generated = run.GeneratedSources[0].SourceText.ToString();
        StringAssert.Contains(generated, "partial struct Hertz");
        Assert.IsFalse(generated.Contains("partial struct Pitch"), $"the foreign source cannot be reopened here; a struct of its name is a second, empty type:\n{generated}");
        StringAssert.Contains(generated, "public static implicit operator Hertz(global::go.lib_package.Pitch src) => new Hertz(src.Hz);");
        Assert.AreEqual(0, errors.Length, $"the cast must compile with the generated operator: {Text(errors)}");
    }

    // Go allows a struct named `short`; C# spells it `@short`, as the host and as the constructed type.
    [TestMethod]
    public void ALocalTargetNamedAfterACSharpKeywordHostsTheOperatorEscaped()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.lib_package.Pitch, go.main_package.@short>]",
            LocalStruct("@short") + "\n    internal static nint Convert(lib_package.Pitch value) => ((@short)value).Hz;"));

        Assert.AreEqual(1, run.GeneratedSources.Length, "the record from a foreign struct source must generate its operator");
        StringAssert.Contains(run.GeneratedSources[0].SourceText.ToString(), "partial struct @short");
        Assert.AreEqual(0, errors.Length, $"the cast must compile with the generated operator: {Text(errors)}");
    }

    // Two packages may each declare a `Pitch`. Each conversion is its own operator: told apart by simple name
    // only, the second record was taken for a repeat of the first and its cast had no operator (CS0030).
    [TestMethod]
    public void SourcesOfOneNameFromTwoPackagesEachGetTheirOperator()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.lib_package.Pitch, go.main_package.hertz>]\n[assembly: GoImplicitConv<go.other_package.Pitch, go.main_package.hertz>]",
            LocalStruct("hertz") + "\n    internal static nint Convert(lib_package.Pitch a, other_package.Pitch b) => ((hertz)a).Hz + ((hertz)b).Hz;"));

        Assert.AreEqual(2, run.GeneratedSources.Length, "each foreign source has its own operator");
        Assert.AreEqual(0, errors.Length, $"both casts must compile: {Text(errors)}");
    }

    // The same for two foreign TARGETS of one name, the direction ForeignStructTargetConversionTests covers.
    [TestMethod]
    public void TargetsOfOneNameFromTwoPackagesEachGetTheirOperator()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.main_package.hertz, go.lib_package.Pitch>]\n[assembly: GoImplicitConv<go.main_package.hertz, go.other_package.Pitch>]",
            LocalStruct("hertz") + "\n    internal static nint Convert(hertz h) => ((lib_package.Pitch)h).Hz + ((other_package.Pitch)h).Hz;"));

        Assert.AreEqual(2, run.GeneratedSources.Length, "each foreign target has its own operator");
        Assert.AreEqual(0, errors.Length, $"both casts must compile: {Text(errors)}");
    }

    // An unexported foreign source is in reach under the -tests white-box model (InternalsVisibleTo). An
    // operator is public, so it is legal in an unexported local target, which is no more visible than its
    // parameter.
    [TestMethod]
    public void AnUnexportedForeignSourceConvertsToAnUnexportedLocalTarget()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.lib_package.pitch, go.main_package.hertz>]",
            LocalStruct("hertz") + "\n    internal static nint Convert(lib_package.pitch value) => ((hertz)value).Hz;"));

        Assert.AreEqual(1, run.GeneratedSources.Length, "the record from a foreign struct source must generate its operator");
        Assert.AreEqual(0, errors.Length, $"the cast must compile with the generated operator: {Text(errors)}");
    }

    // In an EXPORTED local target the same operator would take a parameter less accessible than itself
    // (CS0057), and the foreign source cannot host it: the pair has no operator, so none is generated, and
    // nothing generated fails to compile.
    [TestMethod]
    public void AnUnexportedForeignSourceToAnExportedLocalTargetGeneratesNoOperator()
    {
        (GeneratorRunResult run, Diagnostic[] errors) = Run(Consumer(
            "[assembly: GoImplicitConv<go.lib_package.pitch, go.main_package.Hertz>]",
            LocalStruct("Hertz", "public")));

        Assert.AreEqual(0, run.GeneratedSources.Length, "a pair with no legal operator generates none");
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

        foreach ((string name, string source) in new[] { ("foreign-attributes", Attributes), ("foreign-lib", Upstream) })
        {
            CSharpCompilation upstream = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source, parseOptions)], references, libraryOptions);

            using MemoryStream image = new();
            EmitResult emitted = upstream.Emit(image);
            Assert.IsTrue(emitted.Success, $"the upstream assembly {name} must emit clean: {string.Join("; ", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))}");
            references.Add(MetadataReference.CreateFromImage(image.ToArray()));
        }

        CSharpCompilation consumer = CSharpCompilation.Create("foreign-main", [CSharpSyntaxTree.ParseText(consumerSource, parseOptions)], references, libraryOptions);

        CSharpGeneratorDriver.Create(new ImplicitConvGenerator())
            .RunGeneratorsAndUpdateCompilation(consumer, out Compilation updated, out _);

        GeneratorRunResult run = CSharpGeneratorDriver.Create(new ImplicitConvGenerator()).RunGenerators(consumer).GetRunResult().Results.Single();

        Assert.IsNull(run.Exception, $"the generator must not throw: {run.Exception}");

        return (run, [.. updated.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)]);
    }
}
