using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

// The recorded-literal shapes below report their frames under this hand-authored record, exactly
// as a converted file's package_info.cs would carry it: `#line` remaps their lambda bodies into
// the fictional `litguard_probe_recorded.cs`, the table maps C# lines 100/110 to Go lines 100/110
// (identity entries, hand-encoded and pinned by the converter's own round-trip tests), and the
// funcLits map records two literal spans — an outer one and a nested one INSIDE it sharing lines
// 110-120, so the nested test also proves innermost-wins containment. The suffixes deliberately
// start at 2, not 1: a fallback derivation that happened to answer 1 could not fake them.
[assembly: go.GoPositionMap("litguard/probe/probe.go", "litguard_probe_recorded.cs", "AGPIAQAJFA==", "100-120:2;110-120:2.1")]

// The FUNCTION half of a converted frame's identity — `goFrameName` in runtime's managed_impl.cs —
// derives a Go import path from the emitted namespace + package-class name. That derivation is the
// design's ruled position (DESIGN-position-map.md §8: the FILE half is RECORDED, but "a function
// name IS a property of the package, and Go's own traceback spells it from the package"), so the
// suffix rules the file half retired remain necessary HERE, and they are what this file pins.
//
// Go's own answer, measured against the go1.23.12 toolchain rather than reasoned about:
//
//     internal test (`package callerprobe`      in cp_test.go)  -> callerprobe.TestInternalCallerName
//     external test (`package callerprobe_test` in cpx_test.go) -> callerprobe_test.TestExternalCallerName
//
// The two suffixes are therefore NOT symmetric, which is the whole defect this guard exists for. An
// internal test file is compiled INTO the package under test, so Go names its frames with the
// package's own import path and no suffix at all; an external test file is a genuinely separate
// package and Go keeps the `_test`. The `-tests` pipeline emits the internal variant as
// `<pkg>_internal_test_package` (testConversion.go's `production.Name + "_internal_test" +
// PackageSuffix`), and before the rule existed the derivation carried that converter-invented token
// straight out to the program: `log/slog` asserts `wantFunc = "log/slog.TestCallDepth"` in
// logger_test.go and got `log/slog_internal_test.TestCallDepth`.
//
// WHY THIS TIER. The shape cannot be reached from a behavioral test: a behavioral project is a
// single `package main` compared against `go run`, so it has no `_test`/`_internal_test` variant to
// name. It cannot be reached from the converter's own `go test` either — the rule lives in the C#
// runtime, not in the emission. What it needs is a frame whose declaring type carries the emitted
// shape, which an MSTest tier can simply DECLARE — the same precedent Sha3ReinterpretVectorTests and
// TestExecutionOutputCapTests set for surfaces no other harness can reach. The classes at the foot
// of this file are hand-written stand-ins for the shapes the converter emits, named exactly as
// it names them.
//
// This file also pins the FUNCTION-LITERAL half of the same site (shapes 4-6): a literal frame's
// counter suffix is READ from the file's recorded GoPositionMap funcLits map — Go's
// per-enclosing-function source-order counter, dotted per nesting level — and derived from
// Roslyn's `b__X_Y` only when no record exists. The recorded shapes carry their record and their
// `#line`-pinned positions in this file, so the MSTest tier reaches the recorded path without a
// conversion; the end-to-end emission is the FuncLiteralCallerNames behavioral guard.
//
// Failing-first, measured before each fix landed: shape (2) answered
// `slcguard/probe_internal_test.internalTestShapedFrame`, while shapes (1) and (3) already answered
// correctly — so the guard reported exactly the one rule that was missing and nothing else. Shapes
// (4) and (5) were measured red against the pre-record runtime (funcLits consult neutered): both
// answered `…func0` — the Roslyn-derived ordinal, with the nesting unrepresentable — while (1),
// (2), (3) and the fallback shape (6) stayed green, so the record path is the only thing they detect.
namespace GolibTests
{
    [TestClass]
    public class CallerFrameTestVariantNamingTests
    {
        // The package-path-qualified function name runtime reports for the CALLER of this helper.
        // Callers(1, …) skips the helper's own frame, so the answer names the shaped method below.
        // A one-element pc slice is deliberate: Callers fills at most len(pc), so the single entry
        // is the caller's frame and no unfilled tail can reach Frames.Next.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string CallerFunctionName()
        {
            slice<uintptr> pcs = new slice<uintptr>(1);

            if (runtime_package.Callers(1, pcs) == 0)
                return string.Empty;

            var frames = runtime_package.CallersFrames(pcs);
            var (frame, _) = frames.Next();

            return frame.Function;
        }

        // (1) A production package class — the shape every converted non-test file compiles to. Go
        //     names such a frame with the bare import path, and this has always worked; it is here
        //     so a regression in the shared derivation cannot hide behind the two variant rules.
        [TestMethod]
        public void ProductionPackageFrameNamesTheImportPath()
        {
            Assert.AreEqual(
                "slcguard/probe.productionShapedFrame",
                go.slcguard.probe_package.productionShapedFrame(),
                "a production package class must name its bare import path");
        }

        // (2) THE DEFECT. An internal test file is compiled into the package under test, so Go names
        //     its frames with the package's OWN import path — the `_internal_test` token is a go2cs
        //     emission detail and naming it is a divergence a Go program can read.
        [TestMethod]
        public void InternalTestFrameNamesThePackageUnderTest()
        {
            Assert.AreEqual(
                "slcguard/probe.internalTestShapedFrame",
                go.slcguard.probe_internal_test_package.internalTestShapedFrame(),
                "an internal test variant must name the package under test, with no suffix — Go " +
                "compiles the file INTO that package (measured: callerprobe.TestInternalCallerName)");
        }

        // (3) The external test package is a genuinely separate Go package and Go KEEPS the `_test`.
        //     Pinned in the same file as (2) because the tempting over-broad fix — strip any trailing
        //     `_test` — breaks this one, and `runtime/debug`'s own TestStack greps a rendered
        //     traceback for `runtime/debug_test.(*T).ptrmethod`.
        [TestMethod]
        public void ExternalTestFrameKeepsTheTestSuffix()
        {
            Assert.AreEqual(
                "slcguard/probe_test.externalTestShapedFrame",
                go.slcguard.probe_test_package.externalTestShapedFrame(),
                "an external test package must KEEP its _test suffix — it is a separate Go package " +
                "(measured: callerprobe_test.TestExternalCallerName)");
        }

        // (4) A RECORDED function literal answers Go's counter — read from the GoPositionMap
        //     funcLits map, never derived from Roslyn's `b__X_Y`, whose closure-group numbering
        //     answered func0 here for Go's func1 (measured against go1.23.12; the net/http rows
        //     TestWriteHeaderNoCodeCheck and TestTimeoutHandlerSuperfluousLogs read the value).
        [TestMethod]
        public void RecordedLiteralFrameNamesGoCounter()
        {
            Assert.AreEqual(
                "litguard/probe.recordedOuterLiteralFrame.func2",
                go.litguard.probe_package.recordedOuterLiteralFrame(),
                "a literal frame with a recorded span must answer the RECORDED counter suffix");
        }

        // (5) A NESTED literal's dotted counter (`func2.1`) — a name the Roslyn derivation cannot
        //     even represent — and, because its recorded span sits INSIDE the outer literal's, the
        //     proof that containment picks the innermost span.
        [TestMethod]
        public void RecordedNestedLiteralFrameNamesDottedCounter()
        {
            Assert.AreEqual(
                "litguard/probe.recordedNestedLiteralFrame.func2.1",
                go.litguard.probe_package.recordedNestedLiteralFrame(),
                "a nested literal frame must answer the recorded DOTTED counter, innermost span first");
        }

        // (6) The ruled fallback: a literal frame NO conversion recorded — an older artifact, a
        //     hand-written lambda — keeps the Roslyn-derived ordinal it always had. Only the shape
        //     is pinned; the exact ordinal is Roslyn's own closure-group numbering, which is the
        //     very thing the record exists to stop leaning on.
        [TestMethod]
        public void UnrecordedLiteralFrameKeepsTheDerivedOrdinal()
        {
            string function = go.slcguard.probe_package.fallbackLiteralFrame();

            StringAssert.StartsWith(
                function,
                "slcguard/probe.fallbackLiteralFrame.func",
                "an unrecorded literal frame must keep the pre-record derived shape, got: " + function);
        }

        // (7)-(10) THE SAME NAMES FROM A SYNTHETIC PC. A CPU profile has no live frame: the sampler maps
        //     a sampled method to GoSyntheticPC.Of(method), and pprof resolves that PC through
        //     CallersFrames (runtime/pprof's allFrames). The PC must name the frame exactly as Callers
        //     does above, closures included, or a profiled closure reads as the CLR's
        //     `<>c.<Outer>b__0_0` where Go prints `Outer.func1`.
        internal static string SyntheticFunctionName(MethodBase method) => SyntheticFrame(method).Function;

        // The frame CallersFrames resolves a function's synthetic PC to: what runtime/pprof's profile
        // builder reads for a location (FuncPCABIInternal hands pprof exactly such a PC).
        internal static runtime_package.Frame SyntheticFrame(MethodBase method)
        {
            slice<uintptr> pcs = new slice<uintptr>(1);
            pcs[0] = (uintptr)GoSyntheticPC.Of(method);

            var frames = runtime_package.CallersFrames(pcs);
            var (frame, _) = frames.Next();

            return frame;
        }

        // The backing method of the one lambda declared inside `outer` on `packageClass`.
        private static MethodBase LiteralOf(Type packageClass, string outer) =>
            packageClass.GetNestedTypes(BindingFlags.NonPublic)
                .SelectMany(nested => nested.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                .Single(method => method.Name.StartsWith("<" + outer + ">", StringComparison.Ordinal));

        [TestMethod]
        public void SyntheticPCOfAFunctionNamesTheImportPath()
        {
            Assert.AreEqual(
                "slcguard/probe.productionShapedFrame",
                SyntheticFunctionName(typeof(go.slcguard.probe_package).GetMethod(nameof(go.slcguard.probe_package.productionShapedFrame))!));
        }

        // A synthetic PC's frame is COMPLETE: Go's runtime/pprof marks a location lookupFailed -- and its
        // mapping HasFunctions false -- when any frame has an empty Function, an empty File or Line 0.
        // A function with a position record reports the Go file and line its first statement maps to;
        // one without reports its converted C# position, as a live frame with no record does.
        [TestMethod]
        public void SyntheticPCOfARecordedFunctionReportsItsGoFileAndLine()
        {
            var frame = SyntheticFrame(LiteralOf(typeof(go.litguard.probe_package), nameof(go.litguard.probe_package.recordedOuterLiteralFrame)));

            StringAssert.EndsWith(frame.File.ToString(), "litguard/probe/probe.go", "the recorded Go file, got: " + frame.File);
            Assert.IsTrue(frame.Line >= 100 && frame.Line <= 120, $"a Go line inside the recorded span, got {frame.Line}");
        }

        // A GOROOT-relative record (a standard-library source, recorded without a host path) is ROOTED
        // at run time against runtime.GOROOT()'s src directory: default `go test` bakes the absolute
        // GOROOT path, and a re-exec child that opens its own source through runtime.Caller
        // (TestTracebackSystem) needs a path that exists. With no GOROOT, the recorded form stands.
        [TestMethod]
        public void AGorootRelativeRecordResolvesAgainstGoroot()
        {
            var frame = SyntheticFrame(LiteralOf(typeof(go.litguard.probe_package), nameof(go.litguard.probe_package.recordedOuterLiteralFrame)));
            string goroot = go.runtime_package.GOROOT().ToString().Replace('\\', '/').TrimEnd('/');
            string expected = goroot.Length == 0 ? "litguard/probe/probe.go" : goroot + "/src/litguard/probe/probe.go";

            Assert.AreEqual(expected, frame.File.ToString(), $"the recorded file rooted at GOROOT ({(goroot.Length == 0 ? "none set" : goroot)})");
        }

        [TestMethod]
        public void SyntheticPCOfAnUnrecordedFunctionReportsItsConvertedPosition()
        {
            var frame = SyntheticFrame(typeof(go.slcguard.probe_package).GetMethod(nameof(go.slcguard.probe_package.productionShapedFrame))!);

            StringAssert.EndsWith(frame.File.ToString(), "CallerFrameTestVariantNamingTests.cs", "the converted C# file, got: " + frame.File);
            Assert.IsTrue(frame.Line > 0, $"a real line, got {frame.Line}");
        }

        [TestMethod]
        public void SyntheticPCOfARecordedLiteralNamesGoCounter()
        {
            Assert.AreEqual(
                "litguard/probe.recordedOuterLiteralFrame.func2",
                SyntheticFunctionName(LiteralOf(typeof(go.litguard.probe_package), nameof(go.litguard.probe_package.recordedOuterLiteralFrame))),
                "a profiled literal must answer the RECORDED counter suffix, as its live frame does");
        }

        [TestMethod]
        public void SyntheticPCOfARecordedNestedLiteralNamesDottedCounter()
        {
            Assert.AreEqual(
                "litguard/probe.recordedNestedLiteralFrame.func2.1",
                SyntheticFunctionName(LiteralOf(typeof(go.litguard.probe_package), nameof(go.litguard.probe_package.recordedNestedLiteralFrame))),
                "a profiled nested literal must answer the recorded DOTTED counter, innermost span first");
        }

        [TestMethod]
        public void SyntheticPCOfAnUnrecordedLiteralKeepsTheDerivedOrdinal()
        {
            string function = SyntheticFunctionName(LiteralOf(typeof(go.slcguard.probe_package), nameof(go.slcguard.probe_package.fallbackLiteralFrame)));

            StringAssert.StartsWith(
                function,
                "slcguard/probe.fallbackLiteralFrame.func",
                "a profiled unrecorded literal must take the derived fallback, got: " + function);
        }
    }
}

// The three emitted shapes, declared exactly as the converter spells them: namespace `go.<dir>`,
// class `<pkg>_package` / `<pkg>_internal_test_package` / `<pkg>_test_package`. runtime's
// `isConvertedGoFrame` accepts a frame on these terms alone (namespace under `go`, top-level type
// name ending in `_package`), which is precisely why the naming rule has to be right about them.
namespace go.slcguard
{
    public static class probe_package
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string productionShapedFrame() =>
            GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();

        // The unrecorded-literal shape: this file has no GoPositionMap record, so the lambda's
        // frame reports the real source file, finds nothing, and takes the derived fallback.
        //
        // The LAMBDA carries NoInlining as well as the method, for the reason spelled out over the
        // recorded shapes below: at Release with tiering off the backing method is inlined into this
        // one and there is no literal frame left to name.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string fallbackLiteralFrame()
        {
            Func<string> literal = [MethodImpl(MethodImplOptions.NoInlining)] () => GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();

            return literal();
        }
    }

    public static class probe_internal_test_package
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string internalTestShapedFrame() =>
            GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();
    }

    public static class probe_test_package
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string externalTestShapedFrame() =>
            GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();
    }
}

// The recorded-literal shapes. `#line` remaps each method — its lambda's sequence points included —
// into the fictional file the assembly record at the top of this file describes, so the lambda
// frames' PDB positions land inside the recorded spans: the first method's lines start at 100 (the
// outer literal's span alone), the second's at 110 (inside BOTH spans, where innermost must win).
// Everything else about the shapes matches the classes above: namespace `go.<dir>`, a
// `<pkg>_package` class, which is all isConvertedGoFrame asks.
//
// EVERY LAMBDA BELOW CARRIES `[MethodImpl(MethodImplOptions.NoInlining)]`, and that is what makes
// these three guards measure the naming RULE instead of the JIT's inlining budget. A literal frame
// can only be named while it is ON the stack, and the CLR's StackTrace does not report inlined
// frames — the same fact runtime's own `captureCallers` pins one layer down. At the configuration
// validation runs under (Release with DOTNET_TieredCompilation=0, so full optimization from the
// first call) these one-expression lambdas are inlined into their enclosing method and the `.funcN`
// suffix has nothing to attach to: measured on this file before the attributes, all three answered
// the ENCLOSING frame — `litguard/probe.recordedOuterLiteralFrame` where the guard wants
// `…recordedOuterLiteralFrame.func2` — while the same build under default tiering was green. An
// attribute on a lambda expression reaches its synthesized backing method (verified in an isolated
// probe: `implFlags=NoInlining`), so pinning the frame costs one attribute and no shape change.
//
// The attributes are deliberately INLINE on the lambdas' own lines and this note sits OUTSIDE the
// `#line` regions: the mapped line of each lambda is what the recorded spans 100-120 and 110-120
// are read against, so a line added between `#line 100` and a lambda would move the very position
// the guard is checking.
namespace go.litguard
{
    public static class probe_package
    {
#line 100 "litguard_probe_recorded.cs"
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string recordedOuterLiteralFrame()
        {
            Func<string> literal = [MethodImpl(MethodImplOptions.NoInlining)] () => GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();

            return literal();
        }
#line 110 "litguard_probe_recorded.cs"
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string recordedNestedLiteralFrame()
        {
            Func<string> literal = [MethodImpl(MethodImplOptions.NoInlining)] () => GolibTests.CallerFrameTestVariantNamingTests.CallerFunctionName();

            return literal();
        }
#line default
    }
}
