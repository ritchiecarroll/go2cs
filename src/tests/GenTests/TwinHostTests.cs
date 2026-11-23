// TwinHostTests.cs - Gbtc
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
/// M:N (experimental, opt-in), stage (c)'s SC-1 (2026-11-15): the closure's first twinned runtime build failed with CS0260. A twin is
/// written into its host reopened as `partial`, so a method of a hand-owned NON-partial type (runtime's `Sysmon`) cannot
/// get one. It is refused by name, "host-not-partial", and its callers call it plain: record 10.8's owed count.
/// </summary>
[TestClass]
public class TwinHostTests
{
    private const string Source = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class host_package
        {
            internal static channel<nint> ch;

            // A hand-owned nested helper, not partial, whose method parks.
            private static class Ticker
            {
                internal static void Loop() { ᐸꟷ(ch); }
            }

            // A partial host beside it, the control: its method is twinned.
            private static partial class Waiter
            {
                internal static void Wait() { ᐸꟷ(ch); }
            }

            public static void run()
            {
                Ticker.Loop();
                Waiter.Wait();
            }
        }
        """;

    // Stage (c), arm B: runtime's hand-owned `interface IGoCpuSampler` is not partial; under the all-members rule its
    // members would get twin slots in the interface reopened as `partial` (CS0260).
    private const string IfaceSource = """
        namespace go;

        public static partial class host_package
        {
            internal interface ISampler
            {
                void Sample();
            }

            public partial interface Reader
            {
                void Read();
            }
        }
        """;

    [TestMethod]
    public void ANonPartialInterfaceGetsNoTwinSlotsUnderTheAllMembersRule()
    {
        CSharpParseOptions options = new(LanguageVersion.CSharp13);
        CSharpCompilation compilation = CSharpCompilation.Create("twinhostiface",
            [CSharpSyntaxTree.ParseText(IfaceSource, options, path: @"C:\twinhost\iface.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new()
        {
            ["build_property.GoTwins"] = "true",
            ["build_property.GoTwinsIface"] = "default",
            ["build_property.GoTwinsIfaceAll"] = "true"
        };

        CSharpGeneratorDriver.Create([new TwinGenerator()], parseOptions: options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        global::go2cs.Tests.TwinStampTests.AssertStampsHold(output); // 7d: every stamp agrees with the compiler

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));
        Diagnostic[] errors = output.Emit(Stream.Null).Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        Assert.IsFalse(twins.Contains("Sampleꞏtwin(", StringComparison.Ordinal), "ISampler is not partial: no twin slot");
        StringAssert.Contains(twins, "Readꞏtwin(", "the partial interface's member keeps its slot");
        StringAssert.Contains(twins, "iface-refused-by host-not-partial=1", "the report counts it");
    }

    [TestMethod]
    public void AMethodOfANonPartialHostIsRefusedByNameAndCalledPlain()
    {
        CSharpParseOptions options = new(LanguageVersion.CSharp13);
        CSharpCompilation compilation = CSharpCompilation.Create("twinhost",
            [CSharpSyntaxTree.ParseText(Source, options, path: @"C:\twinhost\main.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new() { ["build_property.GoTwins"] = "true" };

        CSharpGeneratorDriver.Create([new TwinGenerator()], parseOptions: options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out var generated);

        global::go2cs.Tests.TwinStampTests.AssertStampsHold(output); // 7d: every stamp agrees with the compiler

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));
        Diagnostic[] errors = output.Emit(Stream.Null).Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        Assert.IsFalse(twins.Contains("Loopꞏtwin(", StringComparison.Ordinal), "Ticker is not partial: Loop gets no twin");
        StringAssert.Contains(twins, "|Loop|host-not-partial", "the report's refusal row names it");
        StringAssert.Contains(twins, "Waitꞏtwin(", "the partial host's method is twinned");
        StringAssert.Contains(twins, "runꞏtwin(", "the caller keeps its twin");

        // Row 7 piece 1 (GO2CS0021): Loop PARKS in a host that is not partial, so the build names it and its host, once;
        // Wait, in the partial host beside it, is not named.
        Diagnostic[] parking = generated.Where(diagnostic => diagnostic.Id == "GO2CS0021").ToArray();

        Assert.AreEqual(1, parking.Length, string.Join("\n", parking.Select(diagnostic => diagnostic.ToString())));
        Assert.AreEqual(DiagnosticSeverity.Error, parking[0].Severity);
        StringAssert.Contains(parking[0].GetMessage(), "Loop", "names the member");
        StringAssert.Contains(parking[0].GetMessage(), "'Ticker' is not partial", "names the host");
    }

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
