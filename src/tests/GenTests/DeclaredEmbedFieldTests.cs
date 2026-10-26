// DeclaredEmbedFieldTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Owner ruling (A), 2026-10-16: every value and pointer embed is a field the CONVERTER declares, in Go order and
/// marked <c>/*embed*/</c> (<c>/*embed*/ public Inner Inner;</c>, <c>/*embed*/ public ж&lt;Inner&gt; Inner;</c>), so the
/// CLR lays a blittable struct's fields where Go does. go2cs-gen found embeds by the <c>partial ref</c> property it
/// used to back with a generated <c>ʗ</c> field; each place it does so must now find the declared field too, and a
/// missed one must fail here rather than as a missing promoted member in the corpus.
/// </summary>
/// <remarks>
/// One case per reader: the marker read after a same-line attribute (the zero-size layout arc stamps
/// <c>[FieldOffset(n)]</c> on an embed), the syntax member walk and the storage it no longer declares, the METADATA
/// member walk, and the metadata zero-value walk. The cross-package method-surface count (Mid2's unexported
/// <c>loc2</c>) is guarded end to end by behavioral XpkgPromotedMethodSet, which read <c>Within</c>'s ambiguous
/// <c>Tok</c> as promoted until that reader learned the field form. The embed-hop readers are EmbeddedValueHopTests.
/// </remarks>
[TestClass]
public class DeclaredEmbedFieldTests
{
    // The metadata surface: the box, the attributes matched by name, and a struct whose embed is a declared field
    // recorded the way MemberRecordGenerator records it when the production assembly is built.
    private const string ProductionSource =
        """
        namespace go
        {
            public class ж<T> { public ж(T value) { } public T Value = default!; }

            public class GoTypeAttribute : System.Attribute
            {
                public GoTypeAttribute() { }
                public GoTypeAttribute(string definition) { }
            }

            public class GoPackageAttribute : System.Attribute
            {
                public GoPackageAttribute(string name) { }
            }

            public enum GoMemberFact : byte { Embedded = 1 }

            // The generator gives every converted struct this constructor; the metadata zero-value walk checks for it.
            public readonly struct NilType { }

            [System.AttributeUsage(System.AttributeTargets.Struct, AllowMultiple = true)]
            public sealed class GoMemberRecordAttribute : System.Attribute
            {
                public GoMemberRecordAttribute(string member, GoMemberFact fact) { }
            }

            [GoPackage("inner")]
            public static partial class inner_package
            {
                [GoType] public partial struct Inner
                {
                    public long X;
                }

                [GoType, GoMemberRecord("Inner", GoMemberFact.Embedded)]
                public partial struct Mid
                {
                    public Inner Inner;
                    public int Tail;

                    public Mid(NilType _) { Inner = default; Tail = 0; }
                }
            }
        }
        """;

    private const string TestSource =
        """
        using go;

        namespace go
        {
            [GoPackage("main")]
            public static partial class main_package
            {
                [GoType] public partial struct Inner
                {
                    public long X;
                }

                [GoType] public partial struct Outer
                {
                    /*embed*/ public Inner Inner;
                    public int Y;
                }

                [GoType] public partial struct Holder
                {
                    public global::go.inner_package.Mid m;
                }

                [GoType] internal partial struct deep { }

                [GoType] internal partial struct ztok
                {
                    /*embed*/ internal deep deep;
                }

                // The zero-size layout arc's shape for a zero-size embed beside a field: Go lays both at offset 0.
                [GoType] [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 8)]
                internal partial struct Leader
                {
                    [System.Runtime.InteropServices.FieldOffset(0)] /*embed*/ internal readonly ztok ztok;
                    [System.Runtime.InteropServices.FieldOffset(0)] internal long v;
                }
            }
        }
        """;

    private static IEnumerable<MetadataReference> CoreReferences()
    {
        string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        yield return MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll"));
    }

    private static CSharpCompilation TestCompilation()
    {
        CSharpParseOptions parseOptions = new(LanguageVersion.Latest);
        CSharpCompilationOptions libraryOptions = new(OutputKind.DynamicallyLinkedLibrary);

        CSharpCompilation production = CSharpCompilation.Create("embed-production",
            [CSharpSyntaxTree.ParseText(ProductionSource, parseOptions)], CoreReferences(), libraryOptions);

        using MemoryStream image = new();
        EmitResult emitted = production.Emit(image);
        Assert.IsTrue(emitted.Success, $"production compilation must emit clean: {string.Join("; ", emitted.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error))}");

        return CSharpCompilation.Create("embed-test", [CSharpSyntaxTree.ParseText(TestSource, parseOptions)],
            CoreReferences().Append(MetadataReference.CreateFromImage(image.ToArray())), libraryOptions);
    }

    private static string GeneratedFor(string structName)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new TypeGenerator()).RunGenerators(TestCompilation());

        Dictionary<string, string> sources = driver.GetRunResult().Results
            .SelectMany(generator => generator.GeneratedSources)
            .ToDictionary(source => source.HintName, source => source.SourceText.ToString());

        string? key = sources.Keys.FirstOrDefault(hint => hint.Contains($".{structName}"));
        Assert.IsNotNull(key, $"the TypeGenerator must generate for {structName}; got: {string.Join(", ", sources.Keys)}");

        return sources[key!];
    }

    [TestMethod]
    public void TheMarkIsReadAfterASameLineAttribute()
    {
        // The zero-size layout arc's stamp puts the attribute on the embed's own line, where C# makes the comment
        // the attribute list's TRAILING trivia; the marker must still be found there.
        FieldDeclarationSyntax stamped = CSharpSyntaxTree.ParseText(
                "partial struct S { [FieldOffset(0)] /*embed*/ internal readonly nih nih; }")
            .GetCompilationUnitRoot().DescendantNodes().OfType<FieldDeclarationSyntax>().Single();

        FieldDeclarationSyntax unmarked = CSharpSyntaxTree.ParseText(
                "partial struct S { [FieldOffset(0)] internal readonly nih nih; }")
            .GetCompilationUnitRoot().DescendantNodes().OfType<FieldDeclarationSyntax>().Single();

        Assert.IsTrue(MemberMarkers.HasEmbed(stamped), "an embed mark after a same-line attribute list was not read");
        Assert.IsFalse(MemberMarkers.HasEmbed(unmarked), "an unmarked attributed field read as an embed");
    }

    [TestMethod]
    public void ADeclaredEmbedFieldPromotesWithoutGeneratedStorage()
    {
        string generated = GeneratedFor("Outer");

        // Promotion through the declared field: the promoted accessor and its Ꮡ reference reach Inner.X.
        StringAssert.Contains(generated, "X => ref Inner.X", "a declared embed field promoted no instance accessor");
        StringAssert.Contains(generated, "ref instance.Inner.X", "a declared embed field promoted no Ꮡ field reference");

        // And the field is the embed's storage and accessor: nothing generated backs or shadows it.
        Assert.IsFalse(generated.Contains("ʗInner"), "the generator declared backing storage for a declared embed field");
        Assert.IsFalse(generated.Contains("partial ref global::go.main_package.Inner Inner =>") || generated.Contains("partial ref Inner Inner =>"),
            "the generator declared an accessor for a declared embed field");
    }

    [TestMethod]
    public void AReadonlyZeroSizeEmbedPromotesThroughTheSharedSlot()
    {
        // The arc stamps a zero-size embed readonly, and a writable ref into a readonly field is CS8162, so every
        // member promoted through it (all zero-size) answers golib's shared per-type slot instead.
        string generated = GeneratedFor("Leader");

        Assert.IsFalse(generated.Contains("ref ztok.deep"), "a member promoted through a readonly zero-size embed was a writable ref into it");
        StringAssert.Contains(generated, "deep => ref global::go.GoZeroSizeSlot<", "the promoted zero-size member did not answer the shared slot");
    }

    [TestMethod]
    public void TheMetadataMemberWalkReadsARecordedEmbedFieldAsTheEmbed()
    {
        CSharpCompilation compilation = TestCompilation();
        INamedTypeSymbol mid = compilation.GetTypeByMetadataName("go.inner_package+Mid")!;
        Assert.IsNotNull(mid, "the metadata struct did not resolve");

        List<(string typeName, string memberName, bool isReferenceType, bool isProperty, bool isPublic)> members =
            StructDeclarationSyntaxExtensions.GetForeignStructMembers(mid, compilation);

        Assert.IsTrue(members.Any(member => member.memberName == "Inner" && member.isProperty),
            "a field recorded [GoMemberRecord(\"Inner\", Embedded)] was not read as the embed");
        Assert.IsTrue(members.Any(member => member.memberName == "Tail" && !member.isProperty),
            "a plain field read as an embed");
    }

    [TestMethod]
    public void AMetadataStructWithAnEmbedFieldIsConstructed()
    {
        // Every embed keeps the constructor route, as the property form did: a field of a metadata struct whose only
        // need is its embed is constructed, never left at default.
        string generated = GeneratedFor("Holder");

        StringAssert.Contains(generated, "this.m = new global::go.inner_package.Mid(nil);",
            "a metadata struct carrying an embed field was not constructed in its holder's zero value");
    }
}
