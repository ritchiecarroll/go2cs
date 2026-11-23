// TwinR5Tests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

extern alias golib;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using go2cs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenTests;

/// <summary>
/// M:N row 7 piece 4 (DATA-mn-row7-r5.md): R5, the public Go functions whose bodies are hand-written. The converter
/// declares each one as a bodyless <c>partial</c> method in a converted file; its implementing part sits in an
/// <c>_impl.cs</c> companion, or, when none exists, is PartialStubGenerator's throwing stub. Such a function is in the
/// population by its DECLARATION's file: a twin where the hand body parks, else a forwarder, and an author-written
/// twin replaces the forwarder (R5). Nothing of it reaches GO2CS0017.
/// </summary>
[TestClass]
public class TwinR5Tests
{
    private const string Forwarder = "[global::go.golib.GoParkForwarder]";

    // The converted file: four bodyless declarations, as the converter writes a Go function with no body.
    private const string Declarations = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class r5_package
        {
            internal static channel<nint> ch;

            public static partial nint load(ж<nint> addr);

            public static partial void wait();

            public static partial nint stubbed(nint a);

            public static partial void sleep(nint d);

            public static void run() { wait(); }
        }
        """;

    // The hand-written companion: load does not park, wait does, sleep carries its author's twin; stubbed has no
    // implementing part here (PartialStubGenerator writes one).
    private const string Companion = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class r5_package
        {
            public static partial nint load(ж<nint> addr) => addr.Value;

            public static partial void wait() { ᐸꟷ(ch); }

            public static partial void sleep(nint d) { }

            public static global::go.golib.GoTask sleepꞏtwin(nint d) => default;
        }
        """;

    [TestMethod]
    public void AHandBodyThatDoesNotParkGetsAMarkedForwarder()
    {
        (string twins, _) = Run();

        StringAssert.Contains(twins, "global::go.golib.GoTask<nint> loadꞏtwin(", "load's declaration puts it in the population: a forwarder");
        StringAssert.Contains(twins, Forwarder, "the forwarder carries the mark");
        StringAssert.Contains(twins, "|load|forwarder:", "the report classifies it");
    }

    [TestMethod]
    public void AHandBodyThatParksGetsATwinAndNoGuardRow()
    {
        (string twins, Diagnostic[] generated) = Run();

        StringAssert.Contains(twins, "|wait|twin", "a hand body that parks is twinned, and classified as a population member");
        Assert.AreEqual(0, generated.Count(diagnostic => diagnostic.Id == "GO2CS0017"), Names(generated, "GO2CS0017"));
    }

    [TestMethod]
    public void AStubBodiedDeclarationGetsAForwarder()
    {
        (string twins, _) = Run();

        StringAssert.Contains(twins, "global::go.golib.GoTask<nint> stubbedꞏtwin(nint a) => new global::go.golib.GoTask<nint>(stubbed(a));",
            "the declaration whose body is PartialStubGenerator's stub is the candidate itself: a forwarder");
        StringAssert.Contains(twins, "|stubbed|forwarder:bodyless", "refused bodyless, never twinned");
    }

    [TestMethod]
    public void AnAuthorWrittenTwinReplacesTheForwarder()
    {
        (string twins, Diagnostic[] generated) = Run();

        Assert.IsFalse(twins.Contains("[global::go.golib.GoParkForwarder]\r\n    public static global::go.golib.GoTask sleepꞏtwin", StringComparison.Ordinal) ||
            twins.Contains("GoTask sleepꞏtwin(nint d) { sleep(d); return default; }", StringComparison.Ordinal), "no forwarder beside the author's twin");
        StringAssert.Contains(twins, "|sleep|author-twin", "the report classifies it");
        Assert.AreEqual(0, generated.Count(diagnostic => diagnostic.Id == "GO2CS0017"), Names(generated, "GO2CS0017"));
    }

    [TestMethod]
    public void TheWholeSetBuildsClean()
    {
        CSharpCompilation compilation = Compilation();
        Dictionary<string, string> properties = new() { ["build_property.GoTwins"] = "true" };

        CSharpGeneratorDriver.Create([new PartialStubGenerator(), new TwinGenerator()], parseOptions: Options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out var generated);

        Diagnostic[] errors = output.Emit(Stream.Null).Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        Assert.AreEqual(0, generated.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error), Names(generated, null));
    }

    private static readonly CSharpParseOptions Options = new(LanguageVersion.CSharp13);

    private static CSharpCompilation Compilation() =>
        CSharpCompilation.Create("twinr5",
            [CSharpSyntaxTree.ParseText(Declarations, Options, path: @"C:\twinr5\r5.cs"), CSharpSyntaxTree.ParseText(Companion, Options, path: @"C:\twinr5\r5_impl.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

    private static (string twins, Diagnostic[] generated) Run()
    {
        CSharpCompilation compilation = Compilation();
        Dictionary<string, string> properties = new() { ["build_property.GoTwins"] = "true" };

        CSharpGeneratorDriver.Create([new PartialStubGenerator(), new TwinGenerator()], parseOptions: Options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out var generated);

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));
        return (twins, [.. generated]);
    }

    private static string Names(IEnumerable<Diagnostic> diagnostics, string? id) =>
        string.Join("\n", diagnostics.Where(diagnostic => id is null || diagnostic.Id == id).Select(diagnostic => diagnostic.ToString()));

    private static IEnumerable<MetadataReference> References()
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(golib::go.IGoEquatable).Assembly.Location));
    }

    private sealed class BuildProperties(Dictionary<string, string> properties) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
    {
        private readonly Options m_global = new(properties);

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GlobalOptions => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(SyntaxTree tree) => m_global;

        public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(AdditionalText textFile) => m_global;

        private sealed class Options(Dictionary<string, string> values) : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => values.TryGetValue(key, out value);
        }
    }
}
