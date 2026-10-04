// GeneratorSkipRecordTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// One record a generator cannot use must cost exactly its own output, never the whole generator's:
/// a source generator that THROWS contributes nothing, and every other record in the compilation goes
/// with it. BadImplementRecordTests holds the first site (a GoImplement record whose second argument
/// is not an interface); these hold the rest of go2cs-gen's record readers.
/// </summary>
/// <remarks>
/// Each arm runs the REAL generator over one compilation holding the bad record beside a good one and
/// asserts: no generator exception, exactly one diagnostic of the expected id, and the good record's
/// output present. The severity is the test's business too: GO2CS0002 (a warning) where skipping the
/// record can only fail later and loudly, GO2CS0003 (an ERROR) where a skipped record could leave a
/// build green with members missing at run time — the error keeps the build red, at the record.
/// </remarks>
[TestClass]
public class GeneratorSkipRecordTests
{
    private const string Attributes =
        """
        namespace go
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public class GoImplementAttribute<TStruct, TInterface> : System.Attribute
            {
                public bool Promoted { get; set; }
                public bool Pointer { get; set; }
            }

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

    // The good records' file: a package class with the types every arm's good record names.
    private const string PackageFile =
        """
        using go;

        [assembly: GoImplement<go.main_package.name, go.main_package.Stringer>]
        [assembly: GoImplicitConv<go.main_package.src, go.main_package.dst>]

        namespace go
        {
            public static partial class main_package
            {
                public partial struct name { public string s; }

                public static string String(this name n) => n.s;

                public partial interface Stringer
                {
                    string String();
                }

                public partial struct src { public int A; }

                public partial struct dst { public int A; }

                [GoType] public partial struct good { public int A; }
            }
        }
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    internal static (GeneratorRunResult Run, IReadOnlyList<Diagnostic> Diagnostics) Generate(ISourceGenerator generator, params string[] extraFiles)
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilation compilation = CSharpCompilation.Create(
            "skip-record",
            new[] { Attributes, PackageFile }.Concat(extraFiles).Select(source => CSharpSyntaxTree.ParseText(source, parseOptions)),
            CoreReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriverRunResult result = CSharpGeneratorDriver.Create(generator).RunGenerators(compilation).GetRunResult();
        GeneratorRunResult run = result.Results.Single();

        Assert.IsNull(run.Exception, $"one bad record must not make the generator throw: {run.Exception}");
        return (run, [.. result.Diagnostics]);
    }

    internal static Diagnostic Single(IReadOnlyList<Diagnostic> diagnostics, string id, DiagnosticSeverity severity)
    {
        List<Diagnostic> matching = [.. diagnostics.Where(diagnostic => diagnostic.Id == id)];
        Assert.AreEqual(1, matching.Count, $"exactly one {id}: {string.Join("; ", diagnostics)}");
        Assert.AreEqual(severity, matching[0].Severity, $"{id}'s severity");
        return matching[0];
    }

    internal static void AssertGenerated(GeneratorRunResult run, string hintFragment)
    {
        Assert.IsTrue(run.GeneratedSources.Any(source => source.HintName.Contains(hintFragment)),
            $"the good record beside the bad one still generates ({hintFragment}): {string.Join(", ", run.GeneratedSources.Select(source => source.HintName))}");
    }

    // A record file with NO package class: the generator has nowhere to place the output.
    private const string ClasslessImplementRecord =
        """
        [assembly: go.GoImplement<go.main_package.name, go.main_package.Stringer>(Promoted = true)]
        """;

    private const string ClasslessImplicitConvRecord =
        """
        [assembly: go.GoImplicitConv<go.main_package.dst, go.main_package.src>]
        """;

    [TestMethod]
    public void AnImplementRecordInAFileWithoutAPackageClassIsAnErrorAtTheRecordOnly()
    {
        (GeneratorRunResult run, IReadOnlyList<Diagnostic> diagnostics) = Generate(new ImplementGenerator(), ClasslessImplementRecord);

        StringAssert.Contains(Single(diagnostics, "GO2CS0003", DiagnosticSeverity.Error).GetMessage(), "GoImplement");
        AssertGenerated(run, "name");
    }

    [TestMethod]
    public void AnImplicitConvRecordInAFileWithoutAPackageClassIsSkippedWithAWarning()
    {
        (GeneratorRunResult run, IReadOnlyList<Diagnostic> diagnostics) = Generate(new ImplicitConvGenerator(), ClasslessImplicitConvRecord);

        StringAssert.Contains(Single(diagnostics, "GO2CS0002", DiagnosticSeverity.Warning).GetMessage(), "GoImplicitConv");
        AssertGenerated(run, "src");
    }

    // A record spelled with its namespace, `go.GoImplicitConv<…>` / `go.GoImplement<…>`: legal C#, and
    // the attribute finder collects it (it matches the SYMBOL), but the type-argument reader read only
    // an unqualified generic name, so the record's arguments came back unread. GoImplicitConv threw;
    // GoImplement (since F) skipped a VALID record with a warning, dropping its adapter. The reader now
    // reads the qualified form, so neither has anything to report.
    private const string QualifiedRecords =
        """
        using go;

        [assembly: go.GoImplicitConv<go.other_package.p, go.other_package.q>]
        [assembly: go.GoImplement<go.other_package.p, go.other_package.Named>]

        namespace go
        {
            public static partial class other_package
            {
                public partial struct p { public int A; }

                public partial struct q { public int A; }

                public static string Name(this p x) => "p";

                public partial interface Named
                {
                    string Name();
                }
            }
        }
        """;

    [TestMethod]
    public void AQualifiedImplicitConvRecordIsRead()
    {
        (GeneratorRunResult run, IReadOnlyList<Diagnostic> diagnostics) = Generate(new ImplicitConvGenerator(), QualifiedRecords);

        Assert.IsFalse(diagnostics.Any(diagnostic => diagnostic.Id.StartsWith("GO2CS")), string.Join("; ", diagnostics));
        AssertGenerated(run, "other_package.p");
        AssertGenerated(run, "src");
    }

    [TestMethod]
    public void AQualifiedImplementRecordIsRead()
    {
        (GeneratorRunResult run, IReadOnlyList<Diagnostic> diagnostics) = Generate(new ImplementGenerator(), QualifiedRecords);

        Assert.IsFalse(diagnostics.Any(diagnostic => diagnostic.Id.StartsWith("GO2CS")), string.Join("; ", diagnostics));
        AssertGenerated(run, "other_package.p");
        AssertGenerated(run, "name");
    }
}
