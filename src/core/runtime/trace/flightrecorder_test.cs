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

public static void TestFlightRecorderDoubleStart(ж<testing.T> Ꮡt) {
    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(nil));
    {
        var err = fr.Start(); if (err != default!) {
            Ꮡt.Fatalf("unexpected error on Start: %v"u8, err);
        }
    }
    {
        var err = fr.Start(); if (err == default!) {
            Ꮡt.Fatalf("expected error from double Start: %v"u8, err);
        }
    }
    fr.Stop();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object flightRecorderIsEnabledˢ = (@string)"flight recorder is enabled, but never started"u8;
private static readonly object flightRecorderIsNotˢ = (@string)"flight recorder is not enabled, but started"u8;
private static readonly object flightRecorderIsEnabledˢ2 = (@string)"flight recorder is enabled, but stopped"u8;

public static void TestFlightRecorderEnabled(ж<testing.T> Ꮡt) {
    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(nil));

    if (fr.Enabled()) {
        Ꮡt.Fatal(flightRecorderIsEnabledˢ);
    }
    {
        var err = fr.Start(); if (err != default!) {
            Ꮡt.Fatalf("unexpected error on Start: %v"u8, err);
        }
    }
    if (!fr.Enabled()) {
        Ꮡt.Fatal(flightRecorderIsNotˢ);
    }
    fr.Stop();
    if (fr.Enabled()) {
        Ꮡt.Fatal(flightRecorderIsEnabledˢ2);
    }
}

public static void TestFlightRecorderWriteToDisabled(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);

    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(nil));
    {
        var (n, err) = fr.WriteTo(new bytes_BufferжWriter(Ꮡbuf)); if (err == default!) {
            Ꮡt.Fatalf("successfully wrote %d bytes from disabled flight recorder"u8, n);
        }
    }
    {
        var err = fr.Start(); if (err != default!) {
            Ꮡt.Fatalf("unexpected error on Start: %v"u8, err);
        }
    }
    fr.Stop();
    {
        var (n, err) = fr.WriteTo(new bytes_BufferжWriter(Ꮡbuf)); if (err == default!) {
            Ꮡt.Fatalf("successfully wrote %d bytes from disabled flight recorder"u8, n);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object expectedAtLeastOneˢ = (@string)"expected at least one success to write a snapshot, got zero"u8;

public static partial void TestFlightRecorderConcurrentWriteTo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(nil));
    {
        var err = fr.Start(); if (err != default!) {
            Ꮡt.Fatalf("unexpected error on Start: %v"u8, err);
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
                if (err != default! && err.Error() == "call to WriteTo for trace.FlightRecorder already in progress"u8) {
                    if (n != 0) {
                        Ꮡt.Errorf("(goroutine %d) WriteTo bytes written is non-zero for early bail out: %d"u8, i, n);
                    }
                    return;
                }
                if (err != default!) {
                    Ꮡt.Errorf("(goroutine %d) failed to write snapshot for unexpected reason: %v"u8, i, err);
                }
                Ꮡsuccesses.Add(1);

                if (n == 0) {
                    Ꮡt.Errorf("(goroutine %d) wrote invalid trace of zero bytes in size"u8, i);
                }
                if (n != (int64)Ꮡbufs.Value[i].Len()) {
                    Ꮡt.Errorf("(goroutine %d) trace length doesn't match WriteTo result: got %d, want %d"u8, i, n, (int64)Ꮡbufs.Value[i].Len());
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
        Ꮡt.Fatal(expectedAtLeastOneˢ);
    }

    // Validate the traces that came out.
    foreach (var i in range(bufs)) {
        var buf = Ꮡbufs.at<bytes.Buffer>(i);
        if (buf.Len() == 0) {
            continue;
        }
        testReader(Ꮡt, buf.Bytes(), testtrace.ExpectSuccess());
    }
}

public static void TestFlightRecorder(ж<testing.T> Ꮡt) {
    testFlightRecorder(Ꮡt, trace.NewFlightRecorder(new trace.FlightRecorderConfig(nil)), (Action snapshot) => {
        snapshot();
    });
}

public static void TestFlightRecorderStartStop(ж<testing.T> Ꮡt) {
    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(nil));
    for (nint i = 0; i < 5; i++) {
        testFlightRecorder(Ꮡt, fr, (Action snapshot) => {
            snapshot();
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string messageˢ = "message"u8;
private static readonly @string helloˢ = "hello"u8;

public static void TestFlightRecorderLog(ж<testing.T> Ꮡt) {
    var tr = testFlightRecorder(Ꮡt, trace.NewFlightRecorder(new trace.FlightRecorderConfig(nil)), (Action snapshot) => {
        trace.Log(context.Background(), messageˢ, helloˢ);
        snapshot();
    });

    // Prepare to read the trace snapshot.
    var (r, err) = inttrace.NewReader(new bytes_ReaderжReader(bytes.NewReader(tr)));
    if (err != default!) {
        Ꮡt.Fatalf("unexpected error creating trace reader: %v"u8, err);
    }

    // Find the log message in the trace.
    var found = false;
    while (ᐧ) {
        var (ev, errΔ1) = r.ReadEvent();
        if (AreEqual(errΔ1, io.EOF)) {
            break;
        }
        if (errΔ1 != default!) {
            Ꮡt.Fatalf("unexpected error reading trace: %v"u8, errΔ1);
        }
        if (!found && ev.Kind() == inttrace.EventLog) {
            var log = ev.Log();
            found = log.Category == "message"u8 && log.Message == "hello"u8;
        }
    }
    if (!found) {
        Ꮡt.Errorf("failed to find expected log message (%q, %q) in snapshot"u8, messageˢ, helloˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string minAgeˢ = "MinAge"u8;
private static readonly @string maxBytesˢ = "MaxBytes"u8;

public static void TestFlightRecorderGenerationCount(ж<testing.T> Ꮡt) {
    void test(ж<testing.T> tΔ1, ж<trace.FlightRecorder> fr) {
        var tr = testFlightRecorder(tΔ1, fr, (Action snapshot) => {
            // Sleep to let a few generations pass.
            time.Sleep((time.Duration)(3000000000L));
            snapshot();
        });

        // Prepare to read the trace snapshot.
        var (r, err) = inttrace.NewReader(new bytes_ReaderжReader(bytes.NewReader(tr)));
        if (err != default!) {
            tΔ1.Fatalf("unexpected error creating trace reader: %v"u8, err);
        }

        // Make sure there are Sync events: at the start and end.
        slice<nint> syncs = default!;
        nint evs = 0;
        while (ᐧ) {
            var (ev, errΔ1) = r.ReadEvent();
            if (AreEqual(errΔ1, io.EOF)) {
                break;
            }
            if (errΔ1 != default!) {
                tΔ1.Fatalf("unexpected error reading trace: %v"u8, errΔ1);
            }
            if (ev.Kind() == inttrace.EventSync) {
                syncs = append(syncs, evs);
            }
            evs++;
        }
        const nint wantMaxSyncs = 3;
        if (len(syncs) > wantMaxSyncs) {
            tΔ1.Errorf("expected at most %d sync events, found %d at %d"u8,
                (nint)(wantMaxSyncs), len(syncs), syncs);
        }
        var ends = new nint[]{syncs[0], syncs[len(syncs) - 1]}.slice();
        {
            var wantEnds = new nint[]{0, evs - 1}.slice(); if (!slices.Equal<slice<nint>, nint>(wantEnds, ends)) {
                tΔ1.Errorf("expected a sync event at each end of the trace, found sync events at %d instead of %d"u8,
                    ends, wantEnds);
            }
        }
    }
    var testʗ1 = test;
    Ꮡt.Run(minAgeˢ, (ж<testing.T> tΔ2) => {
        var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(MinAge: time.Millisecond));
        testʗ1(tΔ2, fr);
    });
    var testʗ2 = test;
    Ꮡt.Run(maxBytesˢ, (ж<testing.T> tΔ3) => {
        var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(MaxBytes: 16));
        testʗ2(tΔ3, fr);
    });
}

// type flightRecorderTestFunc is a methodless func type — rendered inline as its base delegate

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object cannotRunFlightRecorderˢ = (@string)"cannot run flight recorder tests when tracing is enabled"u8;

internal static slice<byte> testFlightRecorder(ж<testing.T> Ꮡt, ж<trace.FlightRecorder> Ꮡfr, Action<Action> f) {
    ref var t = ref Ꮡt.DerefOrNull();
    ref var fr = ref Ꮡfr.DerefOrNull();

    if (trace.IsEnabled()) {
        Ꮡt.Skip(cannotRunFlightRecorderˢ);
    }

    // Start the flight recorder.
    {
        var err = Ꮡfr.Start(); if (err != default!) {
            Ꮡt.Fatalf("unexpected error on Start: %v"u8, err);
        }
    }

    // Set up snapshot callback.
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    var callback = () => {
        var (n, err) = Ꮡfr.WriteTo(new bytes_BufferжWriter(Ꮡbuf));
        if (err != default!) {
            Ꮡt.Errorf("unexpected failure during flight recording: %v"u8, err);
            return;
        }
        if (n < 16) {
            Ꮡt.Errorf("expected a trace size of at least 16 bytes, got %d"u8, n);
        }
        if (n != (int64)Ꮡbuf.Value.Len()) {
            Ꮡt.Errorf("WriteTo result doesn't match trace size: got %d, want %d"u8, n, (int64)Ꮡbuf.Value.Len());
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
    testReader(Ꮡt, traceBytes, testtrace.ExpectSuccess());
    return traceBytes;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string traceˢ = "trace"u8;

internal static void testReader(ж<testing.T> Ꮡt, slice<byte> tb, ж<testtrace.Expectation> Ꮡexp) {
    ref var exp = ref Ꮡexp.DerefOrNull();

    var (r, err) = inttrace.NewReader(new bytes_ReaderжReader(bytes.NewReader(tb)));
    if (err != default!) {
        {
            var errΔ1 = exp.Check(err); if (errΔ1 != default!) {
                Ꮡt.Error(errΔ1);
            }
        }
        return;
    }
    var v = testtrace.NewValidator();
    v.SkipClockSnapshotChecks();
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
    if (Ꮡt.Failed() || dumpTraces.Value) {
        testtrace.Dump(Ꮡt, traceˢ, tb, dumpTraces.Value);
    }
}

public static void TestTraceAndFlightRecorder(ж<testing.T> Ꮡt) {
    ref var tBuf = ref heap(new bytes.Buffer(), out var ᏑtBuf);
    ref var frBuf = ref heap(new bytes.Buffer(), out var ᏑfrBuf);
    {
        var err = trace.Start(new bytes_BufferжWriter(ᏑtBuf)); if (err != default!) {
            Ꮡt.Errorf("unable to start execution tracer: %s"u8, err);
        }
    }
    var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig(MaxBytes: 16));
    fr.Start();
    fr.WriteTo(new bytes_BufferжWriter(ᏑfrBuf));
    fr.Stop();
    trace.Stop();
    if (tBuf.Len() == 0 || frBuf.Len() == 0) {
        Ꮡt.Errorf("None of these should be equal to zero: %d %d"u8, tBuf.Len(), frBuf.Len());
    }
    if (tBuf.Len() <= frBuf.Len()) {
        Ꮡt.Errorf("trace should be longer than the flight recorder: trace=%d flight record=%d"u8, tBuf.Len(), frBuf.Len());
    }
}

} // end trace_test_package
