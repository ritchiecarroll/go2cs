// GoNameStampTests.cs - Gbtc
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
/// Guards go2cs-gen's reads of the converter's <c>[GoName]</c> stamp (golib's GoNameAttribute), which carries the Go
/// name of a declaration whose Go name starts with <c>Δ</c>. The converter also writes <c>Δ</c> as its rename marker, and
/// a rename projects to its Go name by removing one <c>Δ</c>; a stamped name is read as written. Each row pairs a stamped
/// declaration with an unstamped renamed one one name away, which keeps the projection it had.
/// </summary>
[TestClass]
public class GoNameStampTests
{
    private const string Path = @"C:\go2cs\src\core\dtest\dtest.cs";

    // Plain: a field the Go source spells Δt (stamped) beside a field the converter renamed (ΔB, Go's B), and a field
    // renamed with two markers (ΔΔfile, Go's file, stamped as the converter does for a field named like its struct); value and
    // pointer methods the Go source spells with a Δ (stamped), renamed methods (ΔGot, ΔSet), and Δlow beside a renamed
    // lower (Go's lower, unexported) for the export rule. Def is a
    // defined type over Plain. Δm is a struct whose Go name is Δm (stamped on the accessibility record, as the converter
    // writes it); Δn is a type the converter renamed (Go's n).
    private const string Package =
        """
        namespace go;

        [GoPackage("dtest")]
        public static partial class dtest_package
        {
            public partial struct Plain { [GoName("Δt")] public nint Δt; internal nint ΔB; [GoName("file")] internal nint ΔΔfile; }
            [GoType("Plain")] public partial struct Def;

            [GoType] partial struct Δm { internal nint x; }
            [GoName("Δm")] public partial struct Δm { }
            [GoType] partial struct Δn { internal nint y; }

            [GoName("ΔGet")] public static nint ΔGet(this Plain p) => p.Δt;
            public static nint ΔGot(this Plain p) => p.ΔB;
            [GoName("ΔScale")] public static void ΔScale(this ref Plain p, nint k) => p.Δt *= k;
            public static void ΔSet(this ref Plain p, nint k) => p.ΔB = k;
            [GoName("Δlow")] public static nint Δlow(this Plain p) => 1;
            internal static nint Δlower(this Plain p) => 2;
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

    private static Dictionary<string, string> Run() => Run(Package, out _, new TypeGenerator(), new RecvGenerator());

    private static Dictionary<string, string> Run(string source, out Compilation output, params ISourceGenerator[] generators)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);

        CSharpCompilation compilation = CSharpCompilation.Create("dtest",
            [CSharpSyntaxTree.ParseText(source, options, path: Path), CSharpSyntaxTree.ParseText(TemplateUsings, options, path: @"C:\go2cs\src\core\dtest\obj\dtest.GlobalUsings.g.cs")],
            References(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        CSharpGeneratorDriver.Create(generators)
            .WithUpdatedParseOptions(options)
            .RunGeneratorsAndUpdateCompilation(compilation, out output, out _);

        return output.SyntaxTrees.Except(compilation.SyntaxTrees).ToDictionary(tree => tree.FilePath, tree => tree.ToString());
    }

    private static string PartFor(Dictionary<string, string> parts, string declaration) =>
        parts.Values.Single(source => source.Contains(declaration) && source.Contains("IGoFieldBoxSource.FieldBox"));

    private static string TwinFor(Dictionary<string, string> parts, string method) =>
        parts.Single(part => part.Key.Contains("go2cs.RecvGenerator") && part.Value.Contains($" {method}(this ")).Value;

    [TestMethod]
    public void TheFieldBoxTableKeysAStampedFieldByItsGoName()
    {
        Dictionary<string, string> parts = Run();
        string plain = PartFor(parts, " partial struct Plain");

        StringAssert.Contains(plain, "\"Δt\" => ((ж<Plain>)parentBox).of(ᏑΔt),", "a field the Go source spells Δt is keyed Δt, golib's projection of its stamp");
        StringAssert.Contains(plain, "\"B\" => ((ж<Plain>)parentBox).of(ᏑΔB),", "a renamed field keeps the marker-off key");
        Assert.IsFalse(plain.Contains("\"t\" =>"), "a stamped field is never keyed with its Δ removed");
        StringAssert.Contains(plain, "\"file\" => ((ж<Plain>)parentBox).of(ᏑΔΔfile),", "a field renamed with two markers is keyed by its stamp");
        Assert.IsFalse(plain.Contains("\"Δfile\" =>"), "removing one marker from ΔΔfile is not its Go name");
    }

    [TestMethod]
    public void ADefinedTypeKeysTheFieldsItForwardsByTheirStamps()
    {
        string def = PartFor(Run(), " partial struct Def");

        StringAssert.Contains(def, "\"Δt\" => ((global::go.ж<Def>)parentBox).of(ᏑΔt),", "the underlying struct's stamp keys the forwarded field");
        StringAssert.Contains(def, "\"B\" => ((global::go.ж<Def>)parentBox).of(ᏑΔB),", "a renamed forwarded field keeps the marker-off key");
    }

    [TestMethod]
    public void AStampedTypeTakesGosScopeForItsGoName()
    {
        Dictionary<string, string> parts = Run();

        Assert.IsTrue(parts.Values.Any(source => source.Contains("public partial struct Δm")), "Go exports Δm: its generated part is public, as its record is");
        Assert.IsTrue(parts.Values.Any(source => source.Contains("internal partial struct Δn")), "a renamed n stays internal");
    }

    [TestMethod]
    public void AReceiverTwinCarriesItsSourcesStamp()
    {
        Dictionary<string, string> parts = Run();

        StringAssert.Contains(TwinFor(parts, "ΔScale"), "global::go.GoName(\"ΔScale\")", "a pointer method's ж twin carries the Go name");
        StringAssert.Contains(TwinFor(parts, "ΔGet"), "global::go.GoName(\"ΔGet\")", "a value method's ж twin carries the Go name");
        Assert.IsFalse(TwinFor(parts, "ΔSet").Contains("GoName("), "a renamed pointer method's twin carries no stamp");
        Assert.IsFalse(TwinFor(parts, "ΔGot").Contains("GoName("), "a renamed value method's twin carries no stamp");
    }

    [TestMethod]
    public void TheMethodSetCountsAStampedMethodExported()
    {
        string methodSet = Run().Single(part => part.Key.EndsWith(".MethodSet.g.cs", StringComparison.Ordinal) && part.Value.Contains("partial struct Plain")).Value;

        StringAssert.Contains(methodSet, "case (\"ΔGet\", false):", "Go exports ΔGet: reflect sees it");
        StringAssert.Contains(methodSet, "case (\"ΔScale\", true):", "and ΔScale in *T's set");
        StringAssert.Contains(methodSet, "case (\"ΔGot\", false):", "a renamed Got is exported too: the projection reads Got");
        StringAssert.Contains(methodSet, "case (\"Δlow\", false):", "Go exports Δlow, whose stamp starts with an upper-case letter");
        Assert.IsFalse(methodSet.Contains("\"Δlower\""), "a renamed lower is unexported: the projection reads lower");
    }

    // A struct field the Go source spells Δt (stamped, exported in Go) beside a field the converter renamed (Δb, Go's b,
    // unexported): the public constructor takes Δt and leaves Δb to the internal one, as Go's export rule reads the two.
    private const string CtorPackage =
        """
        namespace go;

        [GoPackage("dtest")]
        public static partial class dtest_package
        {
            [GoType] public partial struct Rec { [GoName("Δt")] public nint Δt; internal nint Δb; }
        }
        """;

    [TestMethod]
    public void AStampedExportedFieldIsAPublicConstructorParameter()
    {
        Dictionary<string, string> parts = Run(CtorPackage, out Compilation output, new TypeGenerator());
        string rec = parts.Single(part => part.Value.Contains("partial struct Rec")).Value;
        string publicCtor = rec.Split('\n').Single(line => line.Contains("public Rec(") && !line.Contains("NilType") && line.Contains("nint"));

        StringAssert.Contains(publicCtor, "Δt", "Go exports Δt, so the public constructor takes it");
        Assert.IsFalse(publicCtor.Contains("Δb"), "Go's b is unexported: only the internal constructor takes it");
        Assert.IsTrue(rec.Contains("internal Rec("), "the internal constructor takes every field");
    }

    // An interface whose Go name is ΔFooer (stamped), embedded in Wrap under that name, and H, whose fields one Δ away
    // from embeds are named Matrix (of type *ΔMatrix, a stamped struct) and Fooer (of type ΔFooer): Go embeds neither.
    // ΔRenamed2 is a struct the converter renamed (Go's Renamed2), embedded by pointer in Holder under its Go name.
    private const string EmbedPackage =
        """
        using go;

        [assembly: GoImplement<global::go.dtest_package.Wrap, global::go.dtest_package.ΔFooer>(Pointer = true)]
        [assembly: GoImplement<global::go.dtest_package.Wrap, global::go.dtest_package.ΔFooer>(Promoted = true)]

        namespace go;

        [GoPackage("dtest")]
        public static partial class dtest_package
        {
            [GoType] public partial interface ΔFooer { [GoName("ΔFoo")] @string ΔFoo(); }
            [GoName("ΔFooer")] public partial interface ΔFooer { }

            [GoType] partial struct Wrap { [GoName("ΔFooer")] /*embed*/ public ΔFooer ΔFooer; }

            [GoType] partial struct ΔMatrix { internal nint n; }
            [GoName("ΔMatrix")] public partial struct ΔMatrix { }

            [GoType] partial struct H { public ж<ΔMatrix> Matrix; public ΔFooer Fooer; }

            [GoType] partial struct ΔRenamed2 { internal nint r; }
            [GoType] partial struct Holder { /*embed*/ public ж<ΔRenamed2> Renamed2; }
        }
        """;

    [TestMethod]
    public void APromotedAdapterForwardsThroughTheStampedEmbedName()
    {
        Dictionary<string, string> parts = Run(EmbedPackage, out Compilation output, new TypeGenerator(), new RecvGenerator(), new ImplementGenerator());
        KeyValuePair<string, string> adapter = parts.Single(part => part.Value.Contains("class WrapжΔFooer"));

        StringAssert.Contains(adapter.Value, ".ΔFooer.ΔFoo()", "Wrap embeds ΔFooer under its Go name, which its stamp carries");
        Assert.IsFalse(adapter.Value.Contains(".Fooer."), "the marker-off name addresses a field Wrap does not declare");

        string[] errors = [.. output.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree?.FilePath == adapter.Key)
            .Select(diagnostic => diagnostic.ToString())];

        Assert.AreEqual(0, errors.Length, "the adapter compiles:\n" + string.Join("\n", errors));
    }

    [TestMethod]
    public void AFieldOneMarkerAwayFromAStampedTypeIsNotAPointerEmbed()
    {
        Run(EmbedPackage, out Compilation output, new TypeGenerator());

        INamedTypeSymbol Type(string name) => output.GetTypeByMetadataName("go.dtest_package+" + name)!;

        Assert.AreEqual(0, StructDeclarationSyntaxExtensions.GetPointerEmbeds(Type("H")).Count, "H's Matrix field is of type *ΔMatrix, whose Go name keeps its Δ: Go embeds nothing");
        CollectionAssert.AreEqual(new[] { "Renamed2" }, StructDeclarationSyntaxExtensions.GetPointerEmbeds(Type("Holder")).Select(embed => embed.Name).ToArray(),
            "a renamed struct embedded by pointer is still read under its Go name");
    }
}
