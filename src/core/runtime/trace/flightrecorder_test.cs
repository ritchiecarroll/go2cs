// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using bytes = bytes_package;
using context = context_package;
using inttrace = @internal.trace_package;
using testtrace = @internal.trace.testtrace_package;
using io = io_package;
using trace = go.runtime.trace_package;
using slices = slices_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using @internal;
using @internal.trace;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.runtime;
using go.sync;

partial class trace_test_package {

public static void TestFlightRecorderDoubleStart(ж<testing.T> t) {
    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig());
    {
        var err = fr.Start(); if (err != nil) {
            t.Fatalf("unexpected error on Start: %v", err);
        }
    }
    {
        var err = fr.Start(); if (err == nil) {
            t.Fatalf("expected error from double Start: %v", err);
        }
    }
    fr.Stop();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object flightRecorderIsEnabledˢ = (@string)"flight recorder is enabled, but never started"u8;
private static readonly object flightRecorderIsNotˢ = (@string)"flight recorder is not enabled, but started"u8;
private static readonly object flightRecorderIsEnabledˢ2 = (@string)"flight recorder is enabled, but stopped"u8;

public static void TestFlightRecorderEnabled(ж<testing.T> t) {
    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig());

    if (fr.Enabled()) {
        t.Fatal(flightRecorderIsEnabledˢ);
    }
    {
        var err = fr.Start(); if (err != nil) {
            t.Fatalf("unexpected error on Start: %v", err);
        }
    }
    if (!fr.Enabled()) {
        t.Fatal(flightRecorderIsNotˢ);
    }
    fr.Stop();
    if (fr.Enabled()) {
        t.Fatal(flightRecorderIsEnabledˢ2);
    }
}

public static void TestFlightRecorderWriteToDisabled(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig());
    {
        var (n, err) = fr.WriteTo(new bytes_BufferжWriter(Ꮡbuf)); if (err == nil) {
            t.Fatalf("successfully wrote %d bytes from disabled flight recorder", n);
        }
    }
    {
        var err = fr.Start(); if (err != nil) {
            t.Fatalf("unexpected error on Start: %v", err);
        }
    }
    fr.Stop();
    {
        var (n, err) = fr.WriteTo(new bytes_BufferжWriter(Ꮡbuf)); if (err == nil) {
            t.Fatalf("successfully wrote %d bytes from disabled flight recorder", n);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object expectedAtLeastOneˢ = (@string)"expected at least one success to write a snapshot, got zero"u8;

public static partial void TestFlightRecorderConcurrentWriteTo(ж<testing.T> t) {
    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig());
    {
        var err = fr.Start(); if (err != nil) {
            t.Fatalf("unexpected error on Start: %v", err);
        }
    }

    // Start two goroutines to write snapshots.
    //
    // Most of the time one will fail and one will succeed, but we don't require this.
    // Due to a variety of factors, it's definitely possible for them both to succeed.
    // However, at least one should succeed.
    ref var bufs = ref heap(new array<bytes.Buffer>(2), out var Ꮡbufs);
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    ref var successes = ref heap(new atomic.Uint32(), out var Ꮡsuccesses);
    foreach (var i in range(bufs)) {
        Ꮡwg.Add(1);
        var frʗ1 = fr;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);

                var (n, err) = frʗ1.WriteTo(new bytes_BufferжWriter(Ꮡbufs.at<bytes.Buffer>(i)));
                // TODO(go.dev/issue/63185) was an exported error. Consider refactoring.
                if (err != nil && err.Error() == "call to WriteTo for trace.FlightRecorder already in progress"u8) {
                    if (n != 0) {
                        t.Errorf("(goroutine %d) WriteTo bytes written is non-zero for early bail out: %d", i, n);
                    }
                    return;
                }
                if (err != nil) {
                    t.Errorf("(goroutine %d) failed to write snapshot for unexpected reason: %v", i, err);
                }
                Ꮡsuccesses.Add(1);

                if (n == 0) {
                    t.Errorf("(goroutine %d) wrote invalid trace of zero bytes in size", i);
                }
                if (n != (int64)Ꮡbufs.Value[i].Len()) {
                    t.Errorf("(goroutine %d) trace length doesn't match WriteTo result: got %d, want %d", i, n, (int64)Ꮡbufs.Value[i].Len());
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();

    // Stop tracing.
    fr.Stop();

    // Make sure at least one succeeded to write.
    if (Ꮡsuccesses.Load() == 0) {
        t.Fatal(expectedAtLeastOneˢ);
    }

    // Validate the traces that came out.
    foreach (var i in range(bufs)) {
        var buf = Ꮡbufs.at<bytes.Buffer>(i);
        if (buf.Len() == 0) {
            continue;
        }
        testReader(t, buf.Bytes(), testtrace.ExpectSuccess());
    }
}

public static void TestFlightRecorder(ж<testing.T> t) {
    testFlightRecorder(t, trace.NewFlightRecorder(new trace.FlightRecorderConfig()), (Action snapshot) => {
        snapshot();
    });
}

public static void TestFlightRecorderStartStop(ж<testing.T> t) {
    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig());
    for (nint i = 0; i < 5; i++) {
        testFlightRecorder(t, fr, (Action snapshot) => {
            snapshot();
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string messageˢ => ref messageˢᵇ;
private static readonly @string messageˢᵇ = "message"u8;
private static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "hello"u8;

public static void TestFlightRecorderLog(ж<testing.T> t) {
    var tr = testFlightRecorder(t, trace.NewFlightRecorder(new trace.FlightRecorderConfig()), (Action snapshot) => {
        trace.Log(context.Background(), messageˢ, helloˢ);
        snapshot();
    });

    // Prepare to read the trace snapshot.
    var (r, err) = inttrace.NewReader(new bytes_ReaderжReader(bytes.NewReader(tr)));
    if (err != nil) {
        t.Fatalf("unexpected error creating trace reader: %v", err);
    }

    // Find the log message in the trace.
    var found = false;
    for (;;) {
        var (ev, errΔ1) = r.ReadEvent();
        if (AreEqual(errΔ1, io.EOF)) {
            break;
        }
        if (errΔ1 != nil) {
            t.Fatalf("unexpected error reading trace: %v", errΔ1);
        }
        if (!found && ev.Kind() == inttrace.EventLog) {
            var log = ev.Log();
            found = log.Category == "message"u8 && log.Message == "hello"u8;
        }
    }
    if (!found) {
        t.Errorf("failed to find expected log message (%q, %q) in snapshot", messageˢ, helloˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string minAgeˢ => ref minAgeˢᵇ;
private static readonly @string minAgeˢᵇ = "MinAge"u8;
private static ref readonly @string maxBytesˢ => ref maxBytesˢᵇ;
private static readonly @string maxBytesˢᵇ = "MaxBytes"u8;

public static void TestFlightRecorderGenerationCount(ж<testing.T> t) {
    void test(ж<testing.T> tΔ1, ж<trace.FlightRecorder> fr) {
        var tr = testFlightRecorder(tΔ1, fr, (Action snapshot) => {
            // Sleep to let a few generations pass.
            time.Sleep((time.Duration)(3000000000L));
            snapshot();
        });

        // Prepare to read the trace snapshot.
        var (r, err) = inttrace.NewReader(new bytes_ReaderжReader(bytes.NewReader(tr)));
        if (err != nil) {
            tΔ1.Fatalf("unexpected error creating trace reader: %v", err);
        }

        // Make sure there are Sync events: at the start and end.
        slice<nint> syncs = default;
        nint evs = 0;
        for (;;) {
            var (ev, errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != nil) {
                tΔ1.Fatalf("unexpected error reading trace: %v", errΔ1);
            }
            if (ev.Kind() == inttrace.EventSync) {
                syncs = append(syncs, evs);
            }
            evs++;
        }
        const nint wantMaxSyncs = 3;
        if (len(syncs) > wantMaxSyncs) {
            tΔ1.Errorf("expected at most %d sync events, found %d at %d",
                (nint)(wantMaxSyncs), len(syncs), syncs);
        }
        var ends = slice<nint>([syncs[0], syncs[len(syncs) - 1]]);
        {
            var wantEnds = slice<nint>([0, evs - 1]); if (!slices.Equal<slice<nint>, nint>(wantEnds, ends)) {
                tΔ1.Errorf("expected a sync event at each end of the trace, found sync events at %d instead of %d",
                    ends, wantEnds);
            }
        }
    }
    var testʗ1 = test;
    t.Run(minAgeˢ, (ж<testing.T> tΔ2) => {
        var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(MinAge: time.Millisecond));
        testʗ1(tΔ2, fr);
    });
    var testʗ2 = test;
    t.Run(maxBytesˢ, (ж<testing.T> tΔ3) => {
        var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(MaxBytes: 16));
        testʗ2(tΔ3, fr);
    });
}

// type flightRecorderTestFunc is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object cannotRunFlightRecorderˢ = (@string)"cannot run flight recorder tests when tracing is enabled"u8;

internal static slice<byte> testFlightRecorder(ж<testing.T> t, ж<trace.FlightRecorder> Ꮡfr, Action<Action> f) {
    ref var fr = ref Ꮡfr.DerefOrNull();

    if (trace.IsEnabled()) {
        t.Skip(cannotRunFlightRecorderˢ);
    }

    // Start the flight recorder.
    {
        var err = Ꮡfr.Start(); if (err != nil) {
            t.Fatalf("unexpected error on Start: %v", err);
        }
    }

    // Set up snapshot callback.
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var callback = () => {
        var (n, err) = Ꮡfr.WriteTo(new bytes_BufferжWriter(Ꮡbuf));
        if (err != nil) {
            t.Errorf("unexpected failure during flight recording: %v", err);
            return;
        }
        if (n < 16) {
            t.Errorf("expected a trace size of at least 16 bytes, got %d", n);
        }
        if (n != (int64)Ꮡbuf.Value.Len()) {
            t.Errorf("WriteTo result doesn't match trace size: got %d, want %d", n, (int64)Ꮡbuf.Value.Len());
        }
    };

    // Call the test function.
    f(callback);

    // Stop the flight recorder.
    fr.Stop();

    // Get the trace bytes; we don't want to use the Buffer as a Reader directly
    // since we may want to consume this data more than once.
    var traceBytes = buf.Bytes();

    // Parse the trace to make sure it's not broken.
    testReader(t, traceBytes, testtrace.ExpectSuccess());
    return traceBytes;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string traceˢ => ref traceˢᵇ;
private static readonly @string traceˢᵇ = "trace"u8;

internal static void testReader(ж<testing.T> t, slice<byte> tb, ж<testtrace.Expectation> Ꮡexp) {
    ref var exp = ref Ꮡexp.DerefOrNull();

    var (r, err) = inttrace.NewReader(new bytes_ReaderжReader(bytes.NewReader(tb)));
    if (err != nil) {
        {
            var errΔ1 = exp.Check(err); if (errΔ1 != nil) {
                t.Error(errΔ1);
            }
        }
        return;
    }
    var v = testtrace.NewValidator();
    v.SkipClockSnapshotChecks();
    for (;;) {
        var (ev, errΔ2) = r.ReadEvent();
        if (AreEqual(errΔ2, io.EOF)) {
            break;
        }
        if (errΔ2 != nil) {
            {
                var errΔ3 = exp.Check(errΔ2); if (errΔ3 != nil) {
                    t.Error(errΔ3);
                }
            }
            return;
        }
        {
            var errΔ4 = v.Event(ev); if (errΔ4 != nil) {
                t.Error(errΔ4);
            }
        }
    }
    {
        var errΔ5 = exp.Check(default); if (errΔ5 != nil) {
            t.Error(errΔ5);
        }
    }
    if (t.Failed() || dumpTraces.Value) {
        testtrace.Dump(t, traceˢ, tb, dumpTraces.Value);
    }
}

public static void TestTraceAndFlightRecorder(ж<testing.T> t) {
    ref var tBuf = ref heap(new bytes.Buffer(), out var ᏑtBuf);
    ref var frBuf = ref heap(new bytes.Buffer(), out var ᏑfrBuf);
    {
        var err = trace.Start(new bytes_BufferжWriter(ᏑtBuf)); if (err != nil) {
            t.Errorf("unable to start execution tracer: %s", err);
        }
    }
    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(MaxBytes: 16));
    fr.Start();
    fr.WriteTo(new bytes_BufferжWriter(ᏑfrBuf));
    fr.Stop();
    trace.Stop();
    if (tBuf.Len() == 0 || frBuf.Len() == 0) {
        t.Errorf("None of these should be equal to zero: %d %d", tBuf.Len(), frBuf.Len());
    }
    if (tBuf.Len() <= frBuf.Len()) {
        t.Errorf("trace should be longer than the flight recorder: trace=%d flight record=%d", tBuf.Len(), frBuf.Len());
    }
}

} // end trace_test_package
