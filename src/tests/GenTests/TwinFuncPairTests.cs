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
/// M:N (experimental, opt-in), N3's D3 core (docs/phase4/DESIGN-mn-func-values.md section 4; COORD's condition 2026-11-14): in an
/// EXISTING twin, a call through a func value tests for a pair and awaits the pair's twin; a can-park literal created
/// in a twin body becomes a pair. Which functions get twins for calling a func value is the caller rule's one predicate:
/// the owner ruled it PROPAGATING (2026-11-14), so such a function is twinned, and so is every function that reaches it.
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

            // Calls through a func value and parks nowhere else: under the owner's PROPAGATING caller rule (2026-11-14)
            // it is twinned for that call alone, and its never-reassigned parameter is tested once, at the twin's entry.
            public static void onlyCalls(Action f)
            {
                f();
            }

            // Parks nowhere and calls no func value; it only reaches onlyCalls, so the rule PROPAGATES to it.
            public static void callsOnlyCalls()
            {
                onlyCalls(() => { });
            }

            // Calls through a func value but is refused a twin (an `in` receiver, byref-recv: N2's box twin takes only a
            // plain `ref` receiver, since Go copies an `in` one): under the rule it warns ONCE (must-fix 3), however many
            // such calls it makes.
            public static void refusedCaller(this in nint r, Action f)
            {
                f();
                f();
            }

            // A value-receiver method whose body parks; `r.wait` is a bound method value. (A pointer receiver is
            // `this ref`, which gets N2's box twin; a bound method value over one is still not paired: a named limit.)
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

                // Fix 5: the converter's creation sites `new D(methodGroup | literal)` (delegateArgWrap, convExprList).
                caller(new Action(park2));
                caller(new Action(() => { ᐸꟷ(ch); }));

                // Fix 6: `go g()` on a func value in a twin launches the pair's twin when g is pair-bound.
                Action g = () => { ᐸꟷ(ch); };
                goǃ(g);
            }

            // A static function that parks, converted to a func value through `new D(F)`.
            public static void park2() { ᐸꟷ(ch); }
        }
        """;

    private static (string twins, Diagnostic[] errors) Run(bool pairs = true) => RunAll(pairs) is var (twins, errors, _) ? (twins, errors) : default;

    // The emission, the compilation's errors and the GENERATOR's own diagnostics (warnings included).
    private static (string twins, Diagnostic[] errors, Diagnostic[] generator) RunAll(bool pairs = true)
    {
        CSharpParseOptions options = new(LanguageVersion.Preview);
        CSharpCompilation compilation = CSharpCompilation.Create("twinpairs",
            [CSharpSyntaxTree.ParseText(Source, options, path: @"C:\twinpairs\main.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new() { ["build_property.GoTwins"] = "true" };

        if (!pairs)
            properties["build_property.GoTwinsPairs"] = "false";

        CSharpGeneratorDriver.Create([new TwinGenerator()], parseOptions: options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out var generatorDiagnostics);

        global::go2cs.Tests.TwinStampTests.AssertStampsHold(output); // 7d: every stamp agrees with the compiler

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));

        // GO2CS_DUMP_TWINS=<path> writes the emission for a reader.
        if (Environment.GetEnvironmentVariable("GO2CS_DUMP_TWINS") is { Length: > 0 } dump)
            File.WriteAllText(dump, twins);

        return (twins, output.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray(),
            [.. generatorDiagnostics]);
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
    }

    // The owner's ruling (2026-11-14, via COORD): the caller rule PROPAGATES. A function whose body calls through a func
    // value a twin can test gets a twin, and every function that reaches it does too (DESIGN-mn-func-values.md 10.2).
    [TestMethod]
    public void UnderThePropagatingCallerRuleAFuncValueCallerAndItsCallersAreTwinned()
    {
        (string twins, Diagnostic[] errors) = Run();

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));

        string onlyCalls = Body(twins, "onlyCalls");
        StringAssert.Contains(onlyCalls, "fꞏpair", "the never-reassigned parameter's pair test is HOISTED to the twin's entry");
        StringAssert.Contains(onlyCalls, ".Twin", "and the call awaits the pair's twin");

        StringAssert.Contains(twins, "callsOnlyCallsꞏtwin(", "the rule PROPAGATES: a function that only reaches onlyCalls is twinned");

        Assert.IsFalse(twins.Contains("refusedCallerꞏtwin(", StringComparison.Ordinal), "control: a refused function stays refused");
    }

    // Must-fix 3 (10.4): a refused function whose body calls through a func value warns ONCE, at the function, naming
    // its refusal -- a silent fallback there would be the rule's largest silent hole.
    [TestMethod]
    public void ARefusedFuncValueCallerWarnsOnceNamingItsRefusal()
    {
        (_, _, Diagnostic[] generator) = RunAll();

        Diagnostic[] warnings = generator.Where(diagnostic => diagnostic.Id == "GO2CS0014").ToArray();

        Assert.AreEqual(1, warnings.Length, "one warning, for refusedCaller's two calls: " + string.Join("\n", generator.Select(d => d.ToString())));
        Assert.AreEqual(DiagnosticSeverity.Warning, warnings[0].Severity);
        StringAssert.Contains(warnings[0].GetMessage(), "refusedCaller");
        StringAssert.Contains(warnings[0].GetMessage(), "byref");
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
    public void TheConvertersNewDelegateSitesMakeOnePairAndNoDelegateOverADelegate()
    {
        string main = Body(Run().twins, "main");

        StringAssert.Contains(main, "new global::System.Action(new global::go2cs.TwinPairs.ꞏFuncPair0(park2, park2ꞏtwin).Invoke)",
            "new D(F) over a function that parks is a pair of F and its twin");
        Assert.IsFalse(main.Contains("new Action(new global::System.Action(", StringComparison.Ordinal) ||
                       main.Contains("new global::System.Action(new global::System.Action(", StringComparison.Ordinal),
            "a literal paired inside new D(...) binds the outer creation to the pair, never a delegate over a delegate");
    }

    // A static function's pair is made ONCE per function and delegate type (the D3s arm's 0 B per conversion): a field
    // in the function's own class, filled on first use; every later conversion reads it.
    [TestMethod]
    public void AStaticFunctionsPairIsCachedOncePerFunction()
    {
        (string twins, Diagnostic[] errors) = Run();
        string main = Body(twins, "main");

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        StringAssert.Contains(main, "(ꞏpair0 ??= new global::System.Action(new global::go2cs.TwinPairs.ꞏFuncPair0(park2, park2ꞏtwin).Invoke))",
            "the conversion reads the cached pair, making it on first use");
        StringAssert.Contains(twins, "private static global::System.Action? ꞏpair0;", "the cache is a field of the function's own class");
    }

    [TestMethod]
    public void AGoStatementOnAPairBoundValueLaunchesTheTwin()
    {
        string main = Body(Run().twins, "main");

        StringAssert.Contains(main, "goǃtwin(g.Invoke, ((global::System.Func<global::go.golib.GoTask>)", "a pair-bound g launches its twin");
        StringAssert.Contains(main, "else goǃ(g);", "anything else launches as before");
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
        TwinSurfaceTests.AssertForwarderOnly(twins, "onlyCalls", "onlyCalls parks nowhere");
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
