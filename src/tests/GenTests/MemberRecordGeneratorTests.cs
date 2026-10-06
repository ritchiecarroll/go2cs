// MemberRecordGeneratorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace go2cs.Tests;

/// <summary>
/// Face lift F (docs/PLAN-marker-comment-parity.md, 5.6): converted code marks a Go embedded field with a
/// <c>/*embed*/</c> comment, and <see cref="MemberRecordGenerator"/> records it as <c>[GoMemberRecord]</c> on a
/// generated partial of the declaring struct. The fixture is the committed IfaceEmbedReflectAnonymous emission,
/// read from the tree, with its <c>[GoEmbedded] </c> stamps written as the converter now writes them.
/// </summary>
[TestClass]
public class MemberRecordGeneratorTests
{
    private const string Stubs = """
        namespace go;

        public sealed class GoTypeAttribute : System.Attribute { public GoTypeAttribute() { } public GoTypeAttribute(string type) { } }
        public sealed class GoTagAttribute(string tag) : System.Attribute { }
        public sealed class GoEmbeddedAttribute : System.Attribute { }
        public enum GoMemberFact : byte { Embedded = 1, Tag = 2, Dims = 3 }
        [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class, AllowMultiple = true)]
        public sealed class GoMemberRecordAttribute : System.Attribute
        {
            public GoMemberRecordAttribute(string member, GoMemberFact fact) { }
            public GoMemberRecordAttribute(string member, GoMemberFact fact, string value) { }
            public GoMemberRecordAttribute(string member, GoMemberFact fact, params long[] dims) { }
        }
        [System.AttributeUsage(System.AttributeTargets.Struct | System.AttributeTargets.Class, AllowMultiple = true)]
        public sealed class GoParamDimsAttribute(string method, System.Type[] parameterTypes, int position, params long[] dims) : System.Attribute { }
        public sealed class GoArrayDimsAttribute(params long[] dims) : System.Attribute { }
        """;

    private static readonly Lazy<string[]> s_generated = new(() => Run(FixtureSource()));

    private static string FixtureSource()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            string candidate = Path.Combine(directory.FullName, "src", "tests", "Behavioral", "IfaceEmbedReflectAnonymous", "main.cs");

            if (File.Exists(candidate))
                return File.ReadAllText(candidate).Replace("[GoEmbedded] ", MemberMarkers.Embed + " ");
        }

        throw new FileNotFoundException("src/tests/Behavioral/IfaceEmbedReflectAnonymous/main.cs not found above " + AppContext.BaseDirectory);
    }

    // The converter's own -comments output for Go comments spelled like the markers, committed beside its
    // source and held to the converter's output by src/go2cs/markerComments_test.go.
    private static string MarkerCommentsFixture()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            string candidate = Path.Combine(directory.FullName, "src", "go2cs", "testdata", "markercomments", "main.cs");

            if (File.Exists(candidate))
                return File.ReadAllText(candidate);
        }

        throw new FileNotFoundException("src/go2cs/testdata/markercomments/main.cs not found above " + AppContext.BaseDirectory);
    }

    // Each Tag record's member and value, read back as C# reads the attribute's arguments.
    private static SortedDictionary<string, string> RecordedTags(string[] generated)
    {
        SortedDictionary<string, string> tags = new(StringComparer.Ordinal);

        foreach (AttributeSyntax attribute in generated.SelectMany(source => CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<AttributeSyntax>()))
        {
            if (attribute.ArgumentList?.Arguments is [{ Expression: LiteralExpressionSyntax member }, { Expression: var fact }, { Expression: LiteralExpressionSyntax value }] && fact.ToString().EndsWith(".Tag", StringComparison.Ordinal))
                tags.Add(member.Token.ValueText, value.Token.ValueText);
        }

        return tags;
    }

    // Each generated attribute of the given name, as "<first argument> <the remaining arguments>", the first read
    // as C# reads a string literal.
    private static string[] Recorded(string[] generated, string attributeName) =>
        generated.SelectMany(source => CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<AttributeSyntax>())
            .Where(attribute => attribute.Name.ToString() == $"global::go.{attributeName}")
            .Select(attribute => string.Join(" ", attribute.ArgumentList!.Arguments.Select(argument =>
                argument.Expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression) ? literal.Token.ValueText : argument.Expression.ToString())))
            .OrderBy(text => text, StringComparer.Ordinal).ToArray();

    private static string[] RecordedMembers(string[] generated, string fact) =>
        generated.SelectMany(source => Regex.Matches(source, $@"\[global::go\.GoMemberRecord\(""([^""]+)"", global::go\.GoMemberFact\.{fact}").Select(match => match.Groups[1].Value))
            .OrderBy(member => member, StringComparer.Ordinal).ToArray();

    private static CSharpCompilation Compile(params string[] sources) =>
        CSharpCompilation.Create("test", [.. sources.Select(source => CSharpSyntaxTree.ParseText(source)), CSharpSyntaxTree.ParseText(Stubs)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    private static string[] Run(string source)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new MemberRecordGenerator());
        return driver.RunGenerators(Compile(source)).GetRunResult().GeneratedTrees.Select(tree => tree.ToString()).ToArray();
    }

    private static string Record(string member) => $"[global::go.GoMemberRecord(\"{member}\", global::go.GoMemberFact.Embedded)]";

    private static string PartFor(string structName) =>
        s_generated.Value.Single(source => source.Contains($"partial struct {structName}\r\n"));

    [TestMethod]
    public void EachMarkedFieldIsRecordedOnItsStructInsideThePackageClass()
    {
        foreach ((string type, string member) in new[] { ("w", "error"), ("Pub", "Stringer"), ("X", "error") })
        {
            string part = PartFor(type);
            StringAssert.Contains(part, Record(member));
            Assert.IsTrue(part.IndexOf("partial class main_package", StringComparison.Ordinal) < part.IndexOf($"partial struct {type}", StringComparison.Ordinal), part);
        }
    }

    [TestMethod]
    public void AMarkAfterTheFieldsAttributeListIsRead()
    {
        // `[GoTag(...)]` on the line above, `/*embed*/` before the modifier.
        StringAssert.Contains(PartFor("Tagged"), Record("Stringer"));
        StringAssert.Contains(PartFor("Omitted"), Record("Stringer"));
    }

    [TestMethod]
    public void AFieldNamedForItsTypeWithoutTheMarkIsNotRecorded()
    {
        Assert.IsFalse(s_generated.Value.Any(source => source.Contains("partial struct Named")), "Named's field is named after its type but not embedded");
    }

    [TestMethod]
    public void IsGoEmbeddedReadsTheAttributeTheCommentAndTheRecord()
    {
        const string source = """
            namespace go;

            public partial class lib_package {
                public partial struct Hand { [GoEmbedded] public nint @int; }
                public partial struct Marked {
                    /*embed*/ public nint @int;
                    public nint other;
                }
                [GoMemberRecord("int", GoMemberFact.Embedded)] public partial struct Recorded { public nint @int; }
            }
            """;

        CSharpCompilation library = Compile(source);

        // The record path is the METADATA one: an emitted image carries no syntax, only the attribute.
        using MemoryStream image = new();
        Microsoft.CodeAnalysis.Emit.EmitResult emitted = library.Emit(image);
        Assert.IsTrue(emitted.Success, string.Join("; ", emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));

        CSharpCompilation consumer = CSharpCompilation.Create("consumer", [],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), MetadataReference.CreateFromImage(image.ToArray())]);

        IFieldSymbol Field(Compilation compilation, string type, string name) =>
            compilation.GetTypeByMetadataName($"go.lib_package+{type}")!.GetMembers(name).OfType<IFieldSymbol>().Single();

        Assert.IsTrue(MemberMarkers.IsGoEmbedded(Field(library, "Hand", "int")), "the hand-written attribute");
        Assert.IsTrue(MemberMarkers.IsGoEmbedded(Field(library, "Marked", "int")), "the converter's comment, in source");
        Assert.IsFalse(MemberMarkers.IsGoEmbedded(Field(library, "Marked", "other")), "an unmarked field");
        Assert.IsTrue(MemberMarkers.IsGoEmbedded(Field(consumer, "Recorded", "int")), "the generated record, read from metadata");
    }

    [TestMethod]
    public void GoCommentsShapedLikeTheMarkerAreNeverRecorded()
    {
        // Go comments spelled exactly like the marker, in every place the converter carries one: the converter
        // writes each with a space after its `/*`, so only the real embeds (two in `marked`, one in `tagged`)
        // are recorded, as for the same source without them.
        string converted = MarkerCommentsFixture();
        string[] realEmbeds = ["Reader", "Stringer", "int"];

        CollectionAssert.AreEqual(realEmbeds, RecordedMembers(Run(converted), "Embedded"), "the converter's output");

        // The reader's position rule holds on its own: carried as spelled, each lands somewhere the converter
        // never writes a marker (its own line, after a field's `;`), so the records are unchanged.
        string unspelled = converted.Replace("/* embed*/", MemberMarkers.Embed);
        Assert.AreNotEqual(converted, unspelled);
        CollectionAssert.AreEqual(realEmbeds, RecordedMembers(Run(unspelled), "Embedded"), "the Go comments carried as spelled");
    }

    [TestMethod]
    public void EachTagCommentIsRecordedWithGosOwnValue()
    {
        // The converter's own output: every tag of `tagged`, in whichever spelling it took, read back to the
        // string Go's reflect.StructField.Tag reports. A Go comment shaped like a tag (Bare, Quote, the second
        // comment on Spaced) is carried re-spelled and records nothing, nor does Go's empty tag (Empty).
        SortedDictionary<string, string> expected = new(StringComparer.Ordinal)
        {
            ["Closer"] = "x:\"*/\"",
            ["Grouped"] = "json:\"g\"",
            ["Inner"] = "json:\"inner\"",
            ["Mixed"] = "json:\"m\"",
            ["Pair"] = "json:\"g\"",
            ["Plain"] = "json:\"plain\"",
            ["Quoted"] = "a:\"`b`\"",
            ["Separator"] = "u:\"\u2028\"",
            ["Spaced"] = "json:\"s\"",
            ["Stringer"] = "json:\"str\"",
            ["Tabbed"] = "t:\"\t\"",
            ["Wide"] = "json:\"\u00fc\"",
            ["mixed"] = "json:\"m\"",
        };

        CollectionAssert.AreEqual(expected.ToList(), RecordedTags(Run(MarkerCommentsFixture())).ToList());
    }

    [TestMethod]
    public void GoUnquoteDecodesGosEscapesAndRefusesWhatGoRefuses()
    {
        // Go literals as written in Go source, so each backslash below is one of Go's.
        Assert.AreEqual("a\a\b\f\n\r\t\v\\\"", MemberMarkers.GoUnquote(@"""a\a\b\f\n\r\t\v\\\"""""));
        Assert.AreEqual("AA\u00fc\U0001F600", MemberMarkers.GoUnquote(@"""\x41\101\u00fc\U0001F600"""));
        Assert.AreEqual("\u00fc", MemberMarkers.GoUnquote(@"""\xc3\xbc"""), "escaped bytes are UTF-8");
        Assert.AreEqual("*/", MemberMarkers.GoUnquote(@"""*\x2f"""));

        foreach (string refused in new[] { @"""\q""", @"""\x4""", @"""\400""", @"""\ud800""", @"""a""b""", @"""open", "`raw`" })
            Assert.IsNull(MemberMarkers.GoUnquote(refused), refused);
    }

    [TestMethod]
    public void EachDimsCommentIsRecordedWhereGolibReadsIt()
    {
        // The converter's own output: a parameter's dims on the package class, keyed by method name, the typeof of
        // every parameter and the position; a field's on its struct; a type's as the attribute on the type itself.
        // The generic func, the lambda and the local function keep [GoArrayDims] and record nothing.
        string[] generated = Run(MarkerCommentsFixture());

        string[] parameters = Recorded(generated, "GoParamDims");
        CollectionAssert.AreEqual(new[] { "fill", "hash", "noted", "put" }, parameters.Select(record => record.Split(' ')[0]).ToArray(), string.Join("\n", parameters));
        StringAssert.EndsWith(parameters[0], "} 2 4 8");
        StringAssert.EndsWith(parameters[1], "} 0 32");
        StringAssert.EndsWith(parameters[2], "} 0 4");
        StringAssert.EndsWith(parameters[3], "} 1 3");
        StringAssert.Contains(parameters[3], "new global::System.Type[] { typeof(global::go.example.com.main_package.holder), ", "the receiver is keyed by its element type");

        CollectionAssert.AreEqual(new[] { "m global::go.GoMemberFact.Dims 3", "p global::go.GoMemberFact.Dims 3", "q global::go.GoMemberFact.Dims 6", "s global::go.GoMemberFact.Dims 5" },
            Recorded(generated, "GoMemberRecord").Where(record => record.Contains(".Dims ")).ToArray());

        CollectionAssert.AreEqual(new[] { "2 3", "4" }, Recorded(generated, "GoArrayDims"));
        StringAssert.Contains(generated.Single(source => source.Contains("GoArrayDims(2, 3)")), "partial struct nn\r\n");
        StringAssert.Contains(generated.Single(source => source.Contains("GoArrayDims(4)")), "partial class P\r\n");
    }

    [TestMethod]
    public void ADimsCommentNoRecordCanCarryIsAnErrorNeverASilentLoss()
    {
        const string source = """
            namespace go;

            public partial class lib_package {
                internal static T first<T>(/*[2]*/ array<T> a) => default!;
            }

            public struct Sealed { internal /*[3]*/ int p; }

            public sealed class array<T> { }
            """;

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new MemberRecordGenerator());
        ImmutableArray<Diagnostic> diagnostics = driver.RunGenerators(Compile(source)).GetRunResult().Diagnostics;

        Assert.AreEqual(2, diagnostics.Count(diagnostic => diagnostic.Id == "GO2CS0003" && diagnostic.Severity == DiagnosticSeverity.Error), string.Join("\n", diagnostics));
        Assert.IsTrue(diagnostics.Any(diagnostic => diagnostic.GetMessage().Contains("first")), "the generic method is named");
    }

    [TestMethod]
    public void ParseDimsReadsGosArrayPrefixAndNothingElse()
    {
        CollectionAssert.AreEqual(new long[] { 32 }, MemberMarkers.ParseDims("/*[32]*/"));
        CollectionAssert.AreEqual(new long[] { 4, 8 }, MemberMarkers.ParseDims("/*[4][8]*/"));
        CollectionAssert.AreEqual(new long[] { 46912496118442 }, MemberMarkers.ParseDims("/*[46912496118442]*/"));

        foreach (string refused in new[] { "/*[]*/", "/* [4]*/", "/*[4] */", "/*[a]*/", "/*[4]x*/", "/*[-1]*/", "/*[4][]*/", "/*embed*/" })
            Assert.IsNull(MemberMarkers.ParseDims(refused), refused);
    }
}
