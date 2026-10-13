// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using flag = flag_package;
using fmt = fmt_package;
using io = io_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using testing = testing_package;
using Δtrace = go.@internal.trace_package;
using raw = go.@internal.trace.raw_package;
using testtrace = go.@internal.trace.testtrace_package;
using version = go.@internal.trace.version_package;
using go.@internal;
using go.@internal.trace;
using path;
using static go.@internal.trace_internal_test_package;

partial class trace_test_package {

internal static ж<bool> logEvents = flag.Bool("log-events"u8, false, "whether to log high-level events; significantly slows down tests"u8);
internal static ж<bool> dumpTraces = flag.Bool("dump-traces"u8, false, "dump traces even on success"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataTestsTestˢ = "./testdata/tests/*.test"u8;
internal static readonly @string testdataˢ = "./testdata"u8;

public static void TestReaderGolden(ж<testing.T> Ꮡt) {
    var (matches, err) = filepath.Glob(testdataTestsTestˢ);
    if (err != default!) {
        Ꮡt.Fatalf("failed to glob for tests: %v"u8, err);
    }
    foreach (var (_, testPath) in matches) {
        @string testPathΔ1 = testPath;
        var (testName, errΔ1) = filepath.Rel(testdataˢ, testPathΔ1);
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("failed to relativize testdata path: %v"u8, errΔ1);
        }
        Ꮡt.Run(testName, (ж<testing.T> tΔ1) => {
            var (tr, exp, errΔ2) = testtrace.ParseFile(testPathΔ1);
            if (errΔ2 != default!) {
                tΔ1.Fatalf("failed to parse test file at %s: %v"u8, testPathΔ1, errΔ2);
            }
            testReader(tΔ1, tr, exp);
        });
    }
}

public static void FuzzReader(ж<testing.F> Ꮡf) {
    // Currently disabled because the parser doesn't do much validation and most
    // getters can be made to panic. Turn this on once the parser is meant to
    // reject invalid traces.
    const bool testGetters = false;

    Ꮡf.Fuzz((ж<testing.T> t, slice<byte> b) => {
        var (r, err) = Δtrace.NewReader(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
        if (err != default!) {
            return;
        }
        while (ᐧ) {
            var (ev, errΔ1) = r.ReadEvent();
            if (errΔ1 != default!) {
                break;
            }

            if (!testGetters) {
                continue;
            }
            // Make sure getters don't do anything that panics
            var exprᴛ1 = ev.Kind();
            if (exprᴛ1 == Δtrace.EventLabel) {
                ev.Label();
            }
            else if (exprᴛ1 == Δtrace.EventLog) {
                ev.Log();
            }
            else if (exprᴛ1 == Δtrace.EventMetric) {
                ev.Metric();
            }
            else if (exprᴛ1 == Δtrace.EventRangeActive || exprᴛ1 == Δtrace.EventRangeBegin) {
                ev.Range();
            }
            else if (exprᴛ1 == Δtrace.EventRangeEnd) {
                ev.Range();
                ev.RangeAttributes();
            }
            else if (exprᴛ1 == Δtrace.EventStateTransition) {
                ev.StateTransition();
            }
            else if (exprᴛ1 == Δtrace.EventRegionBegin || exprᴛ1 == Δtrace.EventRegionEnd) {
                ev.Region();
            }
            else if (exprᴛ1 == Δtrace.EventTaskBegin || exprᴛ1 == Δtrace.EventTaskEnd) {
                ev.Task();
            }
            else if (exprᴛ1 == Δtrace.EventSync) {
            }
            else if (exprᴛ1 == Δtrace.EventStackSample) {
            }
            else if (exprᴛ1 == Δtrace.EventBad) {
            }
        }
    });
}

internal static void testReader(ж<testing.T> Ꮡt, io.Reader tr, ж<testtrace.Expectation> Ꮡexp) {
    ref var exp = ref Ꮡexp.DerefOrNull();

    var (r, err) = Δtrace.NewReader(tr);
    if (err != default!) {
        {
            var errΔ1 = exp.Check(err); if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
            }
        }
        return;
    }
    var v = testtrace.NewValidator();
    while (ᐧ) {
        var (ev, errΔ2) = r.ReadEvent();
        if (AreEqual(errΔ2, io.EOF)) {
            break;
        }
        if (errΔ2 != default!) {
            {
                var errΔ3 = exp.Check(errΔ2); if (errΔ3 != default!) {
                    Ꮡt.Error(errΔ3);
                }
            }
            return;
        }
        if (logEvents.Value) {
            Ꮡt.Log(ev.String());
        }
        {
            var errΔ4 = v.Event(ev); if (errΔ4 != default!) {
                Ꮡt.Error(errΔ4);
            }
        }
    }
    {
        var errΔ5 = exp.Check(default!); if (errΔ5 != default!) {
            Ꮡt.Error(errΔ5);
        }
    }
}

internal static @string dumpTraceToText(ж<testing.T> Ꮡt, slice<byte> b) {
    Ꮡt.Helper();

    var (br, err) = raw.NewReader(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
    if (err != default!) {
        Ꮡt.Fatalf("dumping trace: %v"u8, err);
    }
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    (var tw, err) = raw.NewTextWriter(new trace_test_package.strings_BuilderжWriter(Ꮡsb), version.Current);
    if (err != default!) {
        Ꮡt.Fatalf("dumping trace: %v"u8, err);
    }
    while (ᐧ) {
        var (ev, errΔ1) = br.ReadEvent();
        if (AreEqual(errΔ1, io.EOF)) {
            break;
        }
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("dumping trace: %v"u8, errΔ1);
        }
        {
            var errΔ2 = tw.WriteEvent(ev); if (errΔ2 != default!) {
                Ꮡt.Fatalf("dumping trace: %v"u8, errΔ2);
            }
        }
    }
    return sb.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string defaultˢ = "default"u8;
internal static readonly @string stressˢ = "stress"u8;

internal static @string dumpTraceToFile(ж<testing.T> Ꮡt, @string testName, bool stress, slice<byte> b) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();

        @string desc = defaultˢ;
        if (stress) {
            desc = stressˢ;
        }
        @string name = fmt.Sprintf("%s.%s.trace."u8, testName, desc);
        var (f, err) = os.CreateTemp(""u8, name);
        if (err != default!) {
            Ꮡt.Fatalf("creating temp file: %v"u8, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        {
            var (_, errΔ1) = io.Copy(new os.FileжWriter(f), new trace_test_package.bytes_ReaderжReader(bytes.NewReader(b))); if (errΔ1 != default!) {
                Ꮡt.Fatalf("writing trace dump to %q: %v"u8, f.Name(), errΔ1);
            }
        }
        return f.Name();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

} // end trace_test_package
