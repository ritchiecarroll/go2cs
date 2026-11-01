// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bytes = bytes_package;
using flag = flag_package;
using io = io_package;
using filepath = path.filepath_package;
using Δruntime = runtime_package;
using testing = testing_package;
using time = time_package;
using Δtrace = go.@internal.trace_package;
using testtrace = go.@internal.trace.testtrace_package;
using go.@internal;
using go.@internal.trace;
using path;
using static go.@internal.trace_internal_test_package;
using version = go.@internal.trace.version_package;

partial class trace_test_package {

internal static ж<bool> logEvents = flag.Bool("log-events", false, "whether to log high-level events; significantly slows down tests");
internal static ж<bool> dumpTraces = flag.Bool("dump-traces", false, "dump traces even on success");
internal static ж<bool> allocFree = flag.Bool("alloc-free", false, "run alloc/free trace experiment tests");

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataTestsTestˢ => ref testdataTestsTestˢᵇ;
private static readonly @string testdataTestsTestˢᵇ = "./testdata/tests/*.test"u8;
internal static ref readonly @string testdataˢ => ref testdataˢᵇ;
private static readonly @string testdataˢᵇ = "./testdata"u8;

public static void TestReaderGolden(ж<testing.T> t) {
    var (matches, err) = filepath.Glob(testdataTestsTestˢ);
    if (err != nil) {
        t.Fatalf("failed to glob for tests: %v", err);
    }
    foreach (var (_, testPath) in matches) {
        var (testName, errΔ1) = filepath.Rel(testdataˢ, testPath);
        if (errΔ1 != nil) {
            t.Fatalf("failed to relativize testdata path: %v", errΔ1);
        }
        t.Run(testName, (ж<testing.T> tΔ1) => {
            var (tr, ver, exp, errΔ2) = testtrace.ParseFile(testPath);
            if (errΔ2 != nil) {
                tΔ1.Fatalf("failed to parse test file at %s: %v", testPath, errΔ2);
            }
            var v = testtrace.NewValidator();
            v.GoVersion = ver;
            testReader(tΔ1, tr, v, exp);
        });
    }
}

public static void FuzzReader(ж<testing.F> f) {
    // Currently disabled because the parser doesn't do much validation and most
    // getters can be made to panic. Turn this on once the parser is meant to
    // reject invalid traces.
    const bool testGetters = false;

    f.Fuzz((ж<testing.T> t, slice<byte> b) => {
        var (r, err) = Δtrace.NewReader(new trace_test_package.bytes_ReaderжReader(bytes.NewReader(b)));
        if (err != nil) {
            return;
        }
        for (;;) {
            var (ev, errΔ1) = r.ReadEvent();
            if (errΔ1 != nil) {
                break;
            }

            if (!testGetters) {
                continue;
            }
            // Make sure getters don't do anything that panics
            switch (ev.Kind()) {
            case var ᴛ when ᴛ == Δtrace.EventLabel: {
                ev.Label();
                break;
            }
            case var ᴛ when ᴛ == Δtrace.EventLog: {
                ev.Log();
                break;
            }
            case var ᴛ when ᴛ == Δtrace.EventMetric: {
                ev.Metric();
                break;
            }
            case var ᴛ when ᴛ == Δtrace.EventRangeActive || ᴛ == Δtrace.EventRangeBegin: {
                ev.Range();
                break;
            }
            case var ᴛ when ᴛ == Δtrace.EventRangeEnd: {
                ev.Range();
                ev.RangeAttributes();
                break;
            }
            case var ᴛ when ᴛ == Δtrace.EventStateTransition: {
                ev.StateTransition();
                break;
            }
            case var ᴛ when ᴛ == Δtrace.EventRegionBegin || ᴛ == Δtrace.EventRegionEnd: {
                ev.Region();
                break;
            }
            case var ᴛ when ᴛ == Δtrace.EventTaskBegin || ᴛ == Δtrace.EventTaskEnd: {
                ev.Task();
                break;
            }
            case var ᴛ when ᴛ == Δtrace.EventSync: {
                break;
            }
            case var ᴛ when ᴛ == Δtrace.EventStackSample: {
                break;
            }
            case var ᴛ when ᴛ == Δtrace.EventBad: {
                break;
            }}
        }
    });
}

internal static void testReader(ж<testing.T> t, io.Reader tr, ж<testtrace.Validator> Ꮡv, ж<testtrace.Expectation> Ꮡexp) {
    ref var v = ref Ꮡv.DerefOrNull();
    ref var exp = ref Ꮡexp.DerefOrNull();

    var (r, err) = Δtrace.NewReader(tr);
    if (err != nil) {
        {
            var errΔ1 = exp.Check(err); if (errΔ1 != nil) {
                t.Error(errΔ1);
            }
        }
        return;
    }
    for (;;) {
        var (ev, errΔ2) = r.ReadEvent();
        if (AreEqual(errΔ2, io.EOF)) {
            break;
        }
        v.GoVersion = r.GoVersion();
        if (Δruntime.GOOS == "windows"u8 || Δruntime.GOARCH == "wasm"u8) {
            v.SkipClockSnapshotChecks();
        }
        if (errΔ2 != nil) {
            {
                var errΔ3 = exp.Check(errΔ2); if (errΔ3 != nil) {
                    t.Error(errΔ3);
                }
            }
            return;
        }
        if (logEvents.Value) {
            t.Log(ev.String());
        }
        {
            var errΔ4 = Ꮡv.Event(ev); if (errΔ4 != nil) {
                t.Error(errΔ4);
            }
        }
    }
    {
        var errΔ5 = exp.Check(default); if (errΔ5 != nil) {
            t.Error(errΔ5);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string go123SyncTestˢ => ref go123SyncTestˢᵇ;
private static readonly @string go123SyncTestˢᵇ = "go123-sync.test"u8;
internal static ref readonly @string go125SyncTestˢ => ref go125SyncTestˢᵇ;
private static readonly @string go125SyncTestˢᵇ = "go125-sync.test"u8;

internal partial struct TestTraceGenSync_sync /*dyn*/ {
    public traceꓸTime Time;
    public ж<Δtrace.ClockSnapshot> ClockSnapshot;
}

public static void TestTraceGenSync(ж<testing.T> t) {
    void runTest(@string testName, slice<TestTraceGenSync_sync> wantSyncs) {
        var wantSyncsʗ1 = wantSyncs;
        t.Run(testName, (ж<testing.T> tΔ1) => {
            @string testPath = "testdata/tests/"u8 + testName;
            var (r, _, _, err) = testtrace.ParseFile(testPath);
            if (err != nil) {
                tΔ1.Fatalf("malformed test %s: bad trace file: %v", testPath, err);
            }
            (var tr, err) = Δtrace.NewReader(r);
            if (err != nil) {
                tΔ1.Fatalf("malformed test %s: bad trace file: %v", testPath, err);
            }
            slice<traceꓸEvent> syncEvents = default;
            for (;;) {
                var (ev, errΔ1) = tr.ReadEvent();
                if (AreEqual(errΔ1, io.EOF)) {
                    break;
                }
                if (errΔ1 != nil) {
                    tΔ1.Fatalf("malformed test %s: bad trace file: %v", testPath, errΔ1);
                }
                if (ev.Kind() == Δtrace.EventSync) {
                    syncEvents = append(syncEvents, ev.ΔClone());
                }
            }

            {
                nint got = len(syncEvents);
                nint want = len(wantSyncsʗ1); if (got != want) {
                    tΔ1.Errorf("got %d sync events, want %d", got, want);
                }
            }

            foreach (var (i, want) in wantSyncsʗ1) {
                var got = syncEvents[i].ΔClone();
                var gotSync = syncEvents[i].Sync();
                if (got.Time() != want.Time) {
                    tΔ1.Errorf("sync=%d got time %d, want %d", i + 1, got.Time(), want.Time);
                }
                if (gotSync.ClockSnapshot == nil && want.ClockSnapshot == nil) {
                    continue;
                }
                if (gotSync.ClockSnapshot.Trace != want.ClockSnapshot.Trace) {
                    tΔ1.Errorf("sync=%d got trace time %d, want %d", i + 1, gotSync.ClockSnapshot.Trace, want.ClockSnapshot.Trace);
                }
                if (!gotSync.ClockSnapshot.Wall.Equal(want.ClockSnapshot.Wall)) {
                    tΔ1.Errorf("sync=%d got wall time %s, want %s", i + 1, gotSync.ClockSnapshot.Wall, want.ClockSnapshot.Wall);
                }
                if (gotSync.ClockSnapshot.Mono != want.ClockSnapshot.Mono) {
                    tΔ1.Errorf("sync=%d got mono time %d, want %d", i + 1, gotSync.ClockSnapshot.Mono, want.ClockSnapshot.Mono);
                }
            }
        });
    }

    runTest(go123SyncTestˢ, slice<TestTraceGenSync_sync>([
        new(10, nil),
        new(40, nil),
        // The EvFrequency batch for generation 3 is emitted at trace.Time(80),
        // but 60 is the minTs of the generation, see b30 in the go generator.
        new(60, nil),
        new(63, nil)
    ]));

    runTest(go125SyncTestˢ, slice<TestTraceGenSync_sync>([
        new(9, Ꮡ(new Δtrace.ClockSnapshot(Trace: 10, Mono: 99, Wall: time.Date(2025, 2, 28, 15, 4, 9, 123, time.ΔUTC)))),
        new(38, Ꮡ(new Δtrace.ClockSnapshot(Trace: 40, Mono: 199, Wall: time.Date(2025, 2, 28, 15, 4, 10, 123, time.ΔUTC)))),
        new(58, Ꮡ(new Δtrace.ClockSnapshot(Trace: 60, Mono: 299, Wall: time.Date(2025, 2, 28, 15, 4, 11, 123, time.ΔUTC)))),
        new(83, nil)
    ]));
}

} // end trace_test_package
