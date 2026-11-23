// TwinGoLaunchTests.cs - Gbtc
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
/// M:N (experimental, opt-in), the go statement's ARITY fix and its LOUD fallback (COORD's A3 ruling, 2026-11-12; the owner's severity
/// ruling, 2026-11-13, FREEZE-SHEET.md): a go statement of up to 16 arguments launches its function's twin, a go literal
/// WITH arguments launches its twin like one without, and a go statement the generator does NOT launch as a twin, where
/// its goroutine could park, keeps its plain launch and warns once, GO2CS0015, naming the site and the reason. A go
/// statement whose function provably never parks loses nothing, and does not warn.
/// </summary>
[TestClass]
public class TwinGoLaunchTests
{
    private const string Header = """
        using System;
        using static go.builtin;

        namespace go;

        public static partial class launch_package
        {
            internal static channel<nint> ch;

            internal static void work5(nint a, nint b, nint c, nint d, nint e) { ᐸꟷ(ch); }

            internal static void work16(nint a1, nint a2, nint a3, nint a4, nint a5, nint a6, nint a7, nint a8,
                nint a9, nint a10, nint a11, nint a12, nint a13, nint a14, nint a15, nint a16) { ᐸꟷ(ch); }

            internal static nint parksValue(nint x) { ᐸꟷ(ch); return x; }

            internal static void quiet() { }

            internal static void parks() { ᐸꟷ(ch); }

            internal static void parksWith(channel<nint> c, nint v, nint k) { ᐸꟷ(c); }

            internal static void parksRef(channel<nint> c, ref nint v) { ᐸꟷ(c); }

            internal static void parksRefK(channel<nint> c, ref nint v, nint k) { ᐸꟷ(c); }

            internal static void parksRefP(channel<nint> c, ref nint v, ж<nint> p) { ᐸꟷ(c); }

            internal const nint kc = 2;

        """;

    // The converter's WRAPPER for a go call with a constant argument: an implicitly typed literal over the call.
    private const string Wrapper = """
            public static void launchWrapper(channel<nint> c, nint v)
            {
                goǃ((ᴛ1, ᴛ2) => parksWith(ᴛ1, ᴛ2, 1), c, v);
            }

        """;

    // Its by-ref form (`go f(c, &v)` into a `ref` parameter). byref-param (DESIGN-mn-box-twin.md 12.1, Thunks) gives f a
    // box twin, and the wrapper's twin hands it ᴛ2, the box bound once at the go statement as Go evaluates `&v`. With the
    // switch off, or where f gets no box twin, a twin of the wrapper would only move the block from a thread onto a
    // carrier: it stays plain, and warns by name, ref-wrapper.
    private const string RefWrapper = """
            public static void launchRefWrapper(channel<nint> c, ж<nint> Ꮡv)
            {
                goǃ((ᴛ1, ᴛ2) => parksRef(ᴛ1, ref ᴛ2.DerefOrNull()), c, Ꮡv);
            }

        """;

    // A thunk whose callee is refused its box twin (its body names `Ꮡv` already): not followed.
    private const string RefWrapperRefused = """
            internal static void parksRefNamed(channel<nint> c, ref nint v) { ж<nint> Ꮡv = default; ᐸꟷ(c); }

            public static void launchRefRefused(channel<nint> c, ж<nint> Ꮡv)
            {
                goǃ((ᴛ1, ᴛ2) => parksRefNamed(ᴛ1, ref ᴛ2.DerefOrNull()), c, Ꮡv);
            }

        """;

    // A by-ref argument the go statement did not bind: `s[i]` would be read when the goroutine runs, where Go takes
    // `&s[i]` at the go statement, so the element box it could name is not handed. Not followed.
    private const string RefWrapperUnbound = """
            public static void launchRefUnbound(channel<nint> c, slice<nint> s, nint i)
            {
                goǃ((ᴛ1) => parksRef(ᴛ1, ref s[i]), c);
            }

        """;

    // P3's review, finding 3: Go evaluates every argument at the go statement. A by-VALUE name the literal reads when the
    // goroutine runs (a captured c) is not bound, so the thunk is not followed; a constant is.
    private const string RefWrapperCaptured = """
            public static void launchRefCaptured(channel<nint> c, ж<nint> Ꮡv)
            {
                goǃ((ᴛ1) => parksRef(c, ref ᴛ1.DerefOrNull()), Ꮡv);
            }

        """;

    private const string RefWrapperConstant = """
            public static void launchRefConstant(channel<nint> c, ж<nint> Ꮡv)
            {
                goǃ((ᴛ1, ᴛ2) => parksRefK(ᴛ1, ref ᴛ2.DerefOrNull(), 1), c, Ꮡv);
            }

        """;

    // P3's re-read, nit 2: a NAMED constant reaches BoundThunk's symbol test (the literal never does).
    private const string RefWrapperNamedConstant = """
            public static void launchRefNamedConstant(channel<nint> c, ж<nint> Ꮡv)
            {
                goǃ((ᴛ1, ᴛ2) => parksRefK(ᴛ1, ref ᴛ2.DerefOrNull(), kc), c, Ꮡv);
            }

        """;

    // P3's re-read, note 4: a by-value `nil` is a static readonly field, neither a constant nor a bound parameter, so the
    // thunk stays plain and warns (coverage: no such wrapper in src/core).
    private const string RefWrapperNil = """
            public static void launchRefNil(channel<nint> c, ж<nint> Ꮡv)
            {
                goǃ((ᴛ1, ᴛ2) => parksRefP(ᴛ1, ref ᴛ2.DerefOrNull(), nil), c, Ꮡv);
            }

        """;

    private const string Footer = "}\n";

    // Five arguments: one past the four goǃtwin stopped at.
    private const string Five = """
            public static void launch5(nint v)
            {
                goǃ(work5, v, v, v, v, v);
            }

        """;

    // Sixteen: the top of golib's goǃ ladder.
    private const string Sixteen = """
            public static void launch16(nint v)
            {
                goǃ(work16, v, v, v, v, v, v, v, v, v, v, v, v, v, v, v, v);
            }

        """;

    // A go literal WITH arguments, as the converter writes `go func(i int) { ... }(v)`: explicit parameter types.
    private const string LiteralArgs = """
            public static void launchLiteral(nint v)
            {
                goǃ((nint iΔ1, @string sΔ1) => {
                    ᐸꟷ(ch);
                }, v, "x");
            }

        """;

    // A go literal that RETURNS a value, as the converter writes `go func(o chan int) error { ... }(c)`: once with the
    // return type inferred and a deferred-call frame (the result reached through a label), once with it spelled out,
    // once with a blank parameter (written `nint _`, so `_ =` would assign to it), and once returning a tuple.
    // A goroutine drops its results, so both launch their twins; the value is evaluated and discarded.
    private const string ValueLiteral = """
            public static void launchValueLiteral(channel<nint> c)
            {
                goǃ((channel<nint> o) => {
                    error retErr = default;
                    try {
                        ᐸꟷ(o);
                        retErr = default; goto done;
                    }
                    finally { }
                    done: return retErr;
                }, c);

                goǃ(error (channel<nint> o, nint n) => {
                    ᐸꟷ(o);
                    if (n < 0) {
                        return (error)default!;
                    }
                    return default;
                }, c, (nint)6);

                goǃ(error (nint _) => {
                    ᐸꟷ(c);
                    return default;
                }, (nint)1);

                goǃ((nint, error) (channel<nint> o) => {
                    ᐸꟷ(o);
                    return (1, default);
                }, c);
            }

        """;

    // Plain launches: a function that returns a value (goǃ's Func rungs) and parks; a go statement in a function refused
    // a twin (by-ref parameter); and, as the control, a go statement of a function that never parks.
    private const string Plain = """
            public static void launchValue(nint v)
            {
                goǃ(parksValue, v);
            }

            // byref-param (DESIGN-mn-box-twin.md 12.1): a plain `ref` parameter now gets a box twin, so the refused
            // function takes an `out` parameter, which stays refused (byref-out).
            public static void launchInRefused(out nint x)
            {
                x = 0;
                goǃ(parks);
            }

            public static void launchQuiet()
            {
                goǃ(quiet);
            }

        """;

    private static (string twins, Diagnostic[] errors, Diagnostic[] generator) Run(params string[] shapes) => Run(null, shapes);

    private static (string twins, Diagnostic[] errors, Diagnostic[] generator) Run(Dictionary<string, string>? extra, params string[] shapes)
    {
        CSharpParseOptions options = new(LanguageVersion.CSharp13);
        string source = Header + string.Concat(shapes) + Footer;
        CSharpCompilation compilation = CSharpCompilation.Create("twingo",
            [CSharpSyntaxTree.ParseText(source, options, path: @"C:\twingo\main.cs")],
            References(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

        Dictionary<string, string> properties = new() { ["build_property.GoTwins"] = "true" };

        foreach ((string key, string value) in extra ?? [])
            properties[key] = value;

        CSharpGeneratorDriver.Create([new TwinGenerator()], parseOptions: options, optionsProvider: new BuildProperties(properties))
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation output, out var generatorDiagnostics);

        global::go2cs.Tests.TwinStampTests.AssertStampsHold(output); // 7d: every stamp agrees with the compiler

        string twins = string.Join("\n", output.SyntaxTrees.Skip(compilation.SyntaxTrees.Length).Select(tree => tree.ToString()));

        if (Environment.GetEnvironmentVariable("GO2CS_DUMP_TWINS") is { Length: > 0 } dump)
            File.WriteAllText(dump, twins);

        // EMIT, not GetDiagnostics: an async lowering error is reported only while the state machine is built.
        Microsoft.CodeAnalysis.Emit.EmitResult emitted = output.Emit(Stream.Null);

        return (twins, emitted.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray(), [.. generatorDiagnostics]);
    }

    private static void AssertLaunchesTwin(string shape, string method, string launch)
    {
        (string twins, Diagnostic[] errors, Diagnostic[] generator) = Run(shape);

        Assert.AreEqual(0, errors.Length, method + ": " + string.Join("\n", errors.Select(error => error.ToString())));
        StringAssert.Contains(twins, method + "ꞏtwin(", method + " is twinned");
        StringAssert.Contains(twins, launch, method + "'s go statement launches the twin");
        Assert.IsFalse(generator.Any(diagnostic => diagnostic.Id == "GO2CS0015"), "no fallback warning: " + string.Join("\n", generator.Select(d => d.ToString())));
    }

    [TestMethod]
    public void AGoStatementOfFiveArgumentsLaunchesTheTwin() =>
        AssertLaunchesTwin(Five, "launch5", "goǃtwin(work5, work5ꞏtwin, v, v, v, v, v)");

    [TestMethod]
    public void AGoStatementOfSixteenArgumentsLaunchesTheTwin() =>
        AssertLaunchesTwin(Sixteen, "launch16", "goǃtwin(work16, work16ꞏtwin, v, v, v, v, v, v, v, v, v, v, v, v, v, v, v, v)");

    [TestMethod]
    public void AGoLiteralWithArgumentsLaunchesItsTwin() =>
        AssertLaunchesTwin(LiteralArgs, "launchLiteral", "async (nint iΔ1, @string sΔ1) =>");

    [TestMethod]
    public void AGoLiteralThatReturnsAValueLaunchesItsTwin()
    {
        (string twins, Diagnostic[] errors, Diagnostic[] generator) = Run(ValueLiteral);

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        StringAssert.Contains(twins, "launchValueLiteralꞏtwin(", "launchValueLiteral is twinned");
        Assert.AreEqual(4, twins.Split("goǃtwin(").Length - 1, "all four go literals launch their twins: " + twins);
        StringAssert.Contains(twins, "await ᐸꟷꞏtwin(c)", "the blank-parameter literal's twin parks on its receive");
        Assert.AreEqual(3, twins.Split("await ᐸꟷꞏtwin(o)").Length - 1, "each literal's twin parks on its receive: " + twins);
        Assert.IsFalse(generator.Any(diagnostic => diagnostic.Id == "GO2CS0015"), "no fallback warning: " + string.Join("\n", generator.Select(d => d.ToString())));
    }

    [TestMethod]
    public void AnImplicitlyTypedWrapperLaunchesItsTwin() =>
        AssertLaunchesTwin(Wrapper, "launchWrapper", "async (ᴛ1, ᴛ2) => await parksWithꞏtwin(ᴛ1, ᴛ2, 1), c, v)");

    [TestMethod]
    public void ABoxedByRefThunkLaunchesItsTwin() =>
        AssertLaunchesTwin(RefWrapper, "launchRefWrapper", "async (ᴛ1, ᴛ2) => await parksRefꞏtwin(ᴛ1, ᴛ2), c, Ꮡv)");

    [TestMethod]
    public void AThunkWithACapturedByValueArgumentStaysPlainAndWarnsByName() =>
        AssertRefWrapperPlain(null, RefWrapperCaptured);

    [TestMethod]
    public void AThunkWithAConstantArgumentLaunchesItsTwin() =>
        AssertLaunchesTwin(RefWrapperConstant, "launchRefConstant", "async (ᴛ1, ᴛ2) => await parksRefKꞏtwin(ᴛ1, ᴛ2, 1), c, Ꮡv)");

    [TestMethod]
    public void AThunkWithANamedConstantArgumentLaunchesItsTwin() =>
        AssertLaunchesTwin(RefWrapperNamedConstant, "launchRefNamedConstant", "async (ᴛ1, ᴛ2) => await parksRefKꞏtwin(ᴛ1, ᴛ2, kc), c, Ꮡv)");

    [TestMethod]
    public void AThunkWithAByValueNilStaysPlainAndWarnsByName() =>
        AssertRefWrapperPlain(null, RefWrapperNil);

    [TestMethod]
    public void TheSwitchOffKeepsTheByRefWrapperPlainAndWarnsByName() =>
        AssertRefWrapperPlain(new() { ["build_property.GoTwinsBoxParam"] = "false" }, RefWrapper);

    [TestMethod]
    public void AThunkWhoseCalleeIsRefusedItsBoxTwinStaysPlainAndWarnsByName() =>
        AssertRefWrapperPlain(null, RefWrapperRefused);

    [TestMethod]
    public void AThunkWithAnUnboundByRefArgumentStaysPlainAndWarnsByName() =>
        AssertRefWrapperPlain(null, RefWrapperUnbound);

    private static void AssertRefWrapperPlain(Dictionary<string, string>? extra, string shape)
    {
        (string twins, Diagnostic[] errors, Diagnostic[] generator) = Run(extra, shape);

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));
        Assert.IsFalse(twins.Contains("goǃtwin(", StringComparison.Ordinal), "no twin launch of the by-ref wrapper");

        string[] warnings = generator.Where(diagnostic => diagnostic.Id == "GO2CS0015").Select(diagnostic => diagnostic.GetMessage()).ToArray();
        string all = string.Join("\n", generator.Select(diagnostic => diagnostic.ToString()));

        Assert.AreEqual(1, warnings.Length, all);
        StringAssert.Contains(warnings[0], "(ref-wrapper)", all);
    }

    [TestMethod]
    public void AGoStatementLaunchedPlainWhereItCouldParkWarnsByName()
    {
        (_, Diagnostic[] errors, Diagnostic[] generator) = Run(Plain);

        Assert.AreEqual(0, errors.Length, string.Join("\n", errors.Select(error => error.ToString())));

        string[] warnings = generator.Where(diagnostic => diagnostic.Id == "GO2CS0015").Select(diagnostic => diagnostic.GetMessage()).OrderBy(text => text, StringComparer.Ordinal).ToArray();
        string all = string.Join("\n", generator.Select(diagnostic => diagnostic.ToString()));

        Assert.AreEqual(2, warnings.Length, "two plain launches that could park, not the control: " + all);
        Assert.IsTrue(warnings.Any(text => text.Contains("launchInRefused", StringComparison.Ordinal) && text.Contains("byref-out", StringComparison.Ordinal)), all);
        Assert.IsTrue(warnings.Any(text => text.Contains("launchValue", StringComparison.Ordinal) && text.Contains("returns-value", StringComparison.Ordinal)), all);
        Assert.IsFalse(warnings.Any(text => text.Contains("launchQuiet", StringComparison.Ordinal)), "a function that never parks loses nothing: " + all);
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
