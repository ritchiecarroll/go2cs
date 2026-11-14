// TwinFuncPairTests.cs - Gbtc
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
/// M:N PROTOTYPE, N3's D3 core (docs/phase4/DESIGN-mn-func-values.md section 4; COORD's condition 2026-11-14): in an
/// EXISTING twin, a call through a func value tests for a pair and awaits the pair's twin; a can-park literal created
/// in a twin body becomes a pair. The core does not decide which functions get twins for calling a func value: that is
/// the caller rule's one predicate, unruled, under which no function is twinned for that reason alone.
/// </summary>
[TestClass]
public class TwinFuncPairTests
{
    private const string Source = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class demo_package
        {
            internal static channel<nint> ch;

            // Already a twin: it parks on the receive. Its call through f gets the pair test.
            public static void caller(Action f)
            {
                ᐸꟷ(ch);
                f();
            }

            // A value call: in a declaration it moves ahead into a temp and takes the pair test; assigned through a
            // slice's ref-returning indexer it cannot move (a ref cannot live across an await), so it stays plain.
            public static nint valueCaller(Func<nint> g)
            {
                ᐸꟷ(ch);
                slice<nint> s = default;
                s[0] = g();
                var v = g();
                return v + s[0];
            }

            // Calls through a func value and parks nowhere else: under the unruled caller rule, NOT twinned.
            public static void onlyCalls(Action f)
            {
                f();
            }

            // A value-receiver method whose body parks; `r.wait` is a bound method value. (A pointer receiver is
            // `this ref`, which the generator refuses to twin, byref-param: no twin, so no pair.)
            public static void wait(this nint r) { ᐸꟷ(ch); }

            // A twin (it reaches caller); its literal parks, so the twin body makes it a pair.
            public static void main()
            {
                caller(() => { ᐸꟷ(ch); });
                onlyCalls(() => { });

                // Fix 2: a bound method value, as the converter emits it -- the receiver snapshotted ONCE into a local,
                // then an expression-bodied literal over it. Both halves of the pair capture that one local.
                nint r = 3;
                var rʗ1 = r;
                caller(() => rʗ1.wait());
            }
        }
        """;

    private static (string twins, Diagnostic[] errors) Run(bool pairs = true)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);
        CSharpCompilation compilation = CSharpCompilation.Create("twinpairs",
            [CSharpSyntaxTree.ParseText(Source, options, path: @"C:\twinpairs\main.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new() { ["build_property.GoTwins"] = "true" };

        if (!pairs)
            properties["build_property.GoTwinsPairs"] = "false";

        CSharpGeneratorDriver.Create([new TwinGenerator()], parseOptions: options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out _);

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));

        // GO2CS_DUMP_TWINS=<path> writes the emission for a reader.
        if (Environment.GetEnvironmentVariable("GO2CS_DUMP_TWINS") is { Length: > 0 } dump)
            File.WriteAllText(dump, twins);

        return (twins, output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray());
    }

    private static string Body(string twins, string method)
    {
        int start = twins.IndexOf(method + "ꞏtwin(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, $"{method}ꞏtwin is emitted");
        int end = twins.IndexOf("#line default", start, StringComparison.Ordinal);
        return twins[start..(end < 0 ? twins.Length : end)];
    }

    [TestMethod]
    public void TheTwinsCompileAndOnlyTheParkingFunctionsAreTwinned()
    {
        (string twins, Diagnostic[] errors) = Run();

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        StringAssert.Contains(twins, "callerꞏtwin(", "control: a function that parks is twinned");
        StringAssert.Contains(twins, "mainꞏtwin(", "control: a function that reaches a twin is twinned");
        Assert.IsFalse(twins.Contains("onlyCallsꞏtwin(", StringComparison.Ordinal), "the unruled caller rule twins nothing for a func-value call alone");
    }

    [TestMethod]
    public void AnExistingTwinsCallThroughAFuncValueTestsForAPair()
    {
        string caller = Body(Run().twins, "caller");

        StringAssert.Contains(caller, "global::go.golib.GoFuncPair", "the call through f tests its target for a pair");
        StringAssert.Contains(caller, ".Twin", "and awaits the pair's twin");
    }

    [TestMethod]
    public void ABoundMethodValueIsAPairOverItsOneReceiverSnapshot()
    {
        string main = Body(Run().twins, "main");

        StringAssert.Contains(main, "async () => await rʗ1.waitꞏtwin()", "the twin half awaits the method's twin over the same snapshot");
        StringAssert.Contains(main, "(() => rʗ1.wait(), ", "the plain half is the literal as written, over the same snapshot");
    }

    [TestMethod]
    public void AValueCallSpillsAheadAndAnUnmovableOneStaysPlain()
    {
        string valueCaller = Body(Run().twins, "valueCaller");

        StringAssert.Contains(valueCaller, "var ᴛtwin", "the declaration's call moves ahead into a temp");
        StringAssert.Contains(valueCaller, "s[0] = g();", "the indexer assignment's call stays plain");
    }

    [TestMethod]
    public void ACanParkLiteralInATwinBodyBecomesAPair()
    {
        (string twins, _) = Run();
        string main = Body(twins, "main");

        StringAssert.Contains(twins, ": global::go.golib.GoFuncPair", "a pair class is emitted for the literal's delegate type");
        StringAssert.Contains(main, ".Invoke", "main's twin hands out the plain delegate type bound to the pair's Invoke");
    }

    // The A/B control: GoTwinsPairs=false emits no pair and no pair test, and twins the same functions.
    [TestMethod]
    public void GoTwinsPairsOffEmitsTheCoreFreeTwins()
    {
        (string twins, Diagnostic[] errors) = Run(pairs: false);

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        StringAssert.Contains(twins, "callerꞏtwin(");
        StringAssert.Contains(twins, "mainꞏtwin(");
        Assert.IsFalse(twins.Contains("GoFuncPair", StringComparison.Ordinal), "no pair test and no pair class with the core off");
        Assert.IsFalse(twins.Contains("onlyCallsꞏtwin(", StringComparison.Ordinal));
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
