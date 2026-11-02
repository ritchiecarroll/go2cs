// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using bytes = bytes_package;
using inttrace = @internal.trace_package;
using testtrace = @internal.trace.testtrace_package;
using io = io_package;
using runtime = runtime_package;
using trace = go.runtime.trace_package;
using slices = slices_package;
using testing = testing_package;
using @internal;
using @internal.trace;
using go.runtime;

partial class trace_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object validatingˢ = (@string)"validating"u8;
private static ref readonly @string tracerˢ => ref tracerˢᵇ;
private static readonly @string tracerˢᵇ = "tracer"u8;
private static ref readonly @string flightRecorderˢ => ref flightRecorderˢᵇ;
private static readonly @string flightRecorderˢᵇ = "flightRecorder"u8;
private static ref readonly @string startFlightStartTraceˢ => ref startFlightStartTraceˢᵇ;
private static readonly @string startFlightStartTraceˢᵇ = "start(flight)_start(trace)_stop(trace)_stop(flight)"u8;
private static readonly object skippingBecauseTraceIsˢ = (@string)"skipping because trace is already enabled"u8;
private static ref readonly @string startTraceStartFlightˢ => ref startTraceStartFlightˢᵇ;
private static readonly @string startTraceStartFlightˢᵇ = "start(trace)_start(flight)_stop(trace)_stop(flight)"u8;
private static ref readonly @string startFlightStopFlightˢ => ref startFlightStopFlightˢᵇ;
private static readonly @string startFlightStopFlightˢᵇ = "start(flight)_stop(flight)_start(trace)_stop(trace)"u8;
private static ref readonly @string startFlightStartTraceˢ2 => ref startFlightStartTraceˢ2ᵇ;
private static readonly @string startFlightStartTraceˢ2ᵇ = "start(flight)_start(trace)_stop(flight)_stop(trace)"u8;

public static void TestSubscribers(ж<testing.T> t) {
    void validate(ж<testing.T> tΔ1, @string source, slice<byte> tr) {
        GoFrame ᒐ = default;
        try {
            var syncsᴷ = new stackAppend<nint>();
            tΔ1.Log(validatingˢ, source);
            var trʗ1 = tr;
            defer(() => {
                if (tΔ1.Failed()) {
                    testtrace.Dump(tΔ1, "TestSubscribers."u8 + source, trʗ1, dumpTraces.Value);
                }
            }, ref ᒐ);

            // Prepare to read the trace snapshot.
            var (r, err) = inttrace.NewReader(new bytes_ReaderжReader(bytes.NewReader(tr)));
            if (err != nil) {
                tΔ1.Errorf("unexpected error creating trace reader for %s: %v", source, err);
                return;
            }

            var v = testtrace.NewValidator();
            // These platforms can't guarantee a monotonically increasing clock reading in a short trace.
            if (runtime.GOOS == "windows"u8 || runtime.GOARCH == "wasm"u8) {
                v.SkipClockSnapshotChecks();
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
                    tΔ1.Errorf("unexpected error reading trace for %s: %v", source, errΔ1);
                    break;
                }
                {
                    var errΔ2 = v.Event(ev); if (errΔ2 != nil) {
                        tΔ1.Errorf("event validation failed: %s", errΔ2);
                        break;
                    }
                }
                if (ev.Kind() == inttrace.EventSync) {
                    syncs = append(syncsᴷ, syncs, evs);
                }
                evs++;
            }
            if (!tΔ1.Failed()) {
                var ends = slice<nint>([syncs[0], syncs[len(syncs) - 1]]);
                {
                    var wantEnds = slice<nint>([0, evs - 1]); if (!slices.Equal<slice<nint>, nint>(wantEnds, ends)) {
                        tΔ1.Errorf("expected a sync event at each end of the trace, found sync events at %d instead of %d for %s",
                            ends, wantEnds, source);
                    }
                }
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }

    var validateʗ1 = validate;
    void validateTraces(ж<testing.T> tΔ2, ж<bytes.Buffer> traceΔ1, ж<bytes.Buffer> frTrace) {
        validateʗ1(tΔ2, tracerˢ, traceΔ1.Bytes());
        validateʗ1(tΔ2, flightRecorderˢ, frTrace.Bytes());
    }
    ж<trace.FlightRecorder> startFlightRecorder(ж<testing.T> tΔ3) {
        var fr = trace.NewFlightRecorder(new trace.FlightRecorderConfig());
        {
            var err = fr.Start(); if (err != nil) {
                tΔ3.Fatalf("unexpected error creating flight recorder: %v", err);
            }
        }
        return fr;
    }
    void startTrace(ж<testing.T> tΔ4, io.Writer w) {
        {
            var err = trace.Start(w); if (err != nil) {
                tΔ4.Fatalf("unexpected error starting flight recorder: %v", err);
            }
        }
    }
    void stopFlightRecorder(ж<testing.T> tΔ5, ж<trace.FlightRecorder> fr, io.Writer w) {
        {
            var (_, err) = fr.WriteTo(w); if (err != nil) {
                tΔ5.Fatalf("unexpected error writing trace from flight recorder: %v", err);
            }
        }
        fr.Stop();
    }
    void stopTrace() {
        trace.Stop();
    }
    var startFlightRecorderʗ1 = startFlightRecorder;
    var startTraceʗ1 = startTrace;
    var stopFlightRecorderʗ1 = stopFlightRecorder;
    var stopTraceʗ1 = stopTrace;
    var validateTracesʗ1 = validateTraces;
    t.Run(startFlightStartTraceˢ, (ж<testing.T> tΔ6) => {
        GoFrame ᒐ = default;
        try {
            if (trace.IsEnabled()) {
                tΔ6.Skip(skippingBecauseTraceIsˢ);
            }
            var frBuf = @new<bytes.Buffer>();
            var tBuf = @new<bytes.Buffer>();
            var fr = startFlightRecorderʗ1(tΔ6);
            var frʗ1 = fr;
            defer(frʗ1.Stop, ref ᒐ);
            startTraceʗ1(tΔ6, new bytes_BufferжWriter(tBuf));
            defer(trace.Stop, ref ᒐ);
            stopTraceʗ1();
            stopFlightRecorderʗ1(tΔ6, fr, new bytes_BufferжWriter(frBuf));
            validateTracesʗ1(tΔ6, tBuf, frBuf);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var startFlightRecorderʗ2 = startFlightRecorder;
    var startTraceʗ2 = startTrace;
    var stopFlightRecorderʗ2 = stopFlightRecorder;
    var stopTraceʗ2 = stopTrace;
    var validateTracesʗ2 = validateTraces;
    t.Run(startTraceStartFlightˢ, (ж<testing.T> tΔ7) => {
        GoFrame ᒐ = default;
        try {
            if (trace.IsEnabled()) {
                tΔ7.Skip(skippingBecauseTraceIsˢ);
            }
            var frBuf = @new<bytes.Buffer>();
            var tBuf = @new<bytes.Buffer>();
            startTraceʗ2(tΔ7, new bytes_BufferжWriter(tBuf));
            defer(trace.Stop, ref ᒐ);
            var fr = startFlightRecorderʗ2(tΔ7);
            var frʗ2 = fr;
            defer(frʗ2.Stop, ref ᒐ);
            stopTraceʗ2();
            stopFlightRecorderʗ2(tΔ7, fr, new bytes_BufferжWriter(frBuf));
            validateTracesʗ2(tΔ7, tBuf, frBuf);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var startFlightRecorderʗ3 = startFlightRecorder;
    var startTraceʗ3 = startTrace;
    var stopFlightRecorderʗ3 = stopFlightRecorder;
    var stopTraceʗ3 = stopTrace;
    var validateTracesʗ3 = validateTraces;
    t.Run(startFlightStopFlightˢ, (ж<testing.T> tΔ8) => {
        GoFrame ᒐ = default;
        try {
            if (trace.IsEnabled()) {
                tΔ8.Skip(skippingBecauseTraceIsˢ);
            }
            var frBuf = @new<bytes.Buffer>();
            var tBuf = @new<bytes.Buffer>();
            var fr = startFlightRecorderʗ3(tΔ8);
            var frʗ3 = fr;
            defer(frʗ3.Stop, ref ᒐ);
            stopFlightRecorderʗ3(tΔ8, fr, new bytes_BufferжWriter(frBuf));
            startTraceʗ3(tΔ8, new bytes_BufferжWriter(tBuf));
            defer(trace.Stop, ref ᒐ);
            stopTraceʗ3();
            validateTracesʗ3(tΔ8, tBuf, frBuf);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var startFlightRecorderʗ4 = startFlightRecorder;
    var startTraceʗ4 = startTrace;
    var stopFlightRecorderʗ4 = stopFlightRecorder;
    var stopTraceʗ4 = stopTrace;
    var validateTracesʗ4 = validateTraces;
    t.Run(startFlightStopFlightˢ, (ж<testing.T> tΔ9) => {
        GoFrame ᒐ = default;
        try {
            if (trace.IsEnabled()) {
                tΔ9.Skip(skippingBecauseTraceIsˢ);
            }
            var frBuf = @new<bytes.Buffer>();
            var tBuf = @new<bytes.Buffer>();
            var fr = startFlightRecorderʗ4(tΔ9);
            var frʗ4 = fr;
            defer(frʗ4.Stop, ref ᒐ);
            stopFlightRecorderʗ4(tΔ9, fr, new bytes_BufferжWriter(frBuf));
            startTraceʗ4(tΔ9, new bytes_BufferжWriter(tBuf));
            defer(trace.Stop, ref ᒐ);
            stopTraceʗ4();
            validateTracesʗ4(tΔ9, tBuf, frBuf);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var startFlightRecorderʗ5 = startFlightRecorder;
    var startTraceʗ5 = startTrace;
    var stopFlightRecorderʗ5 = stopFlightRecorder;
    var stopTraceʗ5 = stopTrace;
    var validateTracesʗ5 = validateTraces;
    t.Run(startFlightStartTraceˢ2, (ж<testing.T> tΔ10) => {
        GoFrame ᒐ = default;
        try {
            if (trace.IsEnabled()) {
                tΔ10.Skip(skippingBecauseTraceIsˢ);
            }
            var frBuf = @new<bytes.Buffer>();
            var tBuf = @new<bytes.Buffer>();
            var fr = startFlightRecorderʗ5(tΔ10);
            var frʗ5 = fr;
            defer(frʗ5.Stop, ref ᒐ);
            startTraceʗ5(tΔ10, new bytes_BufferжWriter(tBuf));
            defer(trace.Stop, ref ᒐ);
            stopFlightRecorderʗ5(tΔ10, fr, new bytes_BufferжWriter(frBuf));
            stopTraceʗ5();
            validateTracesʗ5(tΔ10, tBuf, frBuf);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

} // end trace_test_package
