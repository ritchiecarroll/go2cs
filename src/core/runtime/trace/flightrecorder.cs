// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using fmt = fmt_package;
using io = io_package;
using sync = sync_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // added for go linkname usage

partial class trace_package {

// FlightRecorder represents a single consumer of a Go execution
// trace.
// It tracks a moving window over the execution trace produced by
// the runtime, always containing the most recent trace data.
//
// At most one flight recorder may be active at any given time,
// though flight recording is allowed to be concurrently active
// with a trace consumer using trace.Start.
// This restriction of only a single flight recorder may be removed
// in the future.
partial struct FlightRecorder {
    internal error err;
    // State specific to the recorder.
    internal array<byte> header = new(16);
    internal rawGeneration active;
    internal sync.Mutex ringMu;
    internal slice<rawGeneration> ring;
    internal frequency freq; // timestamp conversion factor, from the runtime
    // Externally-set options.
    internal uint64 targetSize;
    internal time.Duration targetPeriod;
    internal bool enabled;       // whether the flight recorder is enabled.
    internal sync.Mutex writing; // protects concurrent calls to WriteTo
    // The values of targetSize and targetPeriod we've committed to since the last Start.
    internal uint64 wantSize;
    internal time.Duration wantDur;
}

// NewFlightRecorder creates a new flight recorder from the provided configuration.
public static ж<FlightRecorder> NewFlightRecorder(FlightRecorderConfig cfg) {
    var fr = @new<FlightRecorder>();
    if (cfg.MaxBytes != 0){
        fr.Value.targetSize = cfg.MaxBytes;
    } else {
        fr.Value.targetSize = (uint64)10 << 20; // 10 MiB.
    }

    if (cfg.MinAge != 0){
        fr.Value.targetPeriod = cfg.MinAge;
    } else {
        fr.Value.targetPeriod = (time.Duration)(10000000000L);
    }
    return fr;
}

// Start activates the flight recorder and begins recording trace data.
// Only one call to trace.Start may be active at any given time.
// In addition, currently only one flight recorder may be active in the program.
// Returns an error if the flight recorder cannot be started or is already started.
public static error Start(this ж<FlightRecorder> Ꮡfr) {
    ref var fr = ref Ꮡfr.DerefOrNull();

    if (fr.enabled) {
        return fmt.Errorf("cannot enable a enabled flight recorder"u8);
    }
    fr.wantSize = fr.targetSize;
    fr.wantDur = fr.targetPeriod;
    fr.err = default!;
    fr.freq = (frequency)(1.0D / ((float64)runtime_traceClockUnitsPerSecond() / 1e9D));

    // Start tracing, data is sent to a recorder which forwards it to our own
    // storage.
    {
        var err = Ꮡtracing.subscribeFlightRecorder(Ꮡ(new recorder(r: Ꮡfr))); if (err != default!) {
            return err;
        }
    }

    fr.enabled = true;
    return default!;
}

// Stop ends recording of trace data. It blocks until any concurrent WriteTo calls complete.
public static void Stop(this ref FlightRecorder fr) {
    if (!fr.enabled) {
        return;
    }
    fr.enabled = false;
    Ꮡtracing.unsubscribeFlightRecorder();

    // Reset all state. No need to lock because the reader has already exited.
    fr.active = new rawGeneration(nil);
    fr.ring = default!;
}

// Enabled returns true if the flight recorder is active.
// Specifically, it will return true if Start did not return an error, and Stop has not yet been called.
// It is safe to call from multiple goroutines simultaneously.
public static bool Enabled(this ref FlightRecorder fr) {
    return fr.enabled;
}

// WriteTo snapshots the moving window tracked by the flight recorder.
// The snapshot is expected to contain data that is up-to-date as of when WriteTo is called,
// though this is not a hard guarantee.
// Only one goroutine may execute WriteTo at a time.
// An error is returned upon failure to write to w, if another WriteTo call is already in-progress,
// or if the flight recorder is inactive.
public static (int64 n, error err) WriteTo(this ж<FlightRecorder> Ꮡfr, io.Writer w) {
    int64 n = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var fr = ref Ꮡfr.DerefOrNull();

        if (!fr.enabled) {
            (n, err) = (0, fmt.Errorf("cannot snapshot a disabled flight recorder"u8)); goto ᒐdone;
        }
        if (!fr.writing.TryLock()) {
            // Indicates that a call to WriteTo was made while one was already in progress.
            // If the caller of WriteTo sees this error, they should use the result from the other call to WriteTo.
            (n, err) = (0, fmt.Errorf("call to WriteTo for trace.FlightRecorder already in progress"u8)); goto ᒐdone;
        }
        defer(Ꮡfr.of(FlightRecorder.Ꮡwriting).Unlock, ref ᒐ);

        // Force a global buffer flush.
        runtime_traceAdvance(false);

        // Now that everything has been flushed and written, grab whatever we have.
        //
        // N.B. traceAdvance blocks until the tracer goroutine has actually written everything
        // out, which means the generation we just flushed must have been already been observed
        // by the recorder goroutine. Because we flushed twice, the first flush is guaranteed to
        // have been both completed *and* processed by the recorder goroutine.
        fr.ringMu.Lock();
        var gens = fr.ring;
        fr.ringMu.Unlock();

        // Write the header.
        (var nw, err) = w.Write(fr.header[..]);
        if (err != default!) {
            (n, err) = ((int64)nw, err); goto ᒐdone;
        }
        n += (int64)nw;

        // Write all the data.
        foreach (var (_, gen) in gens) {
            foreach (var (_, data) in gen.batches) {
                // Write batch data.
                (nw, err) = w.Write(data);
                n += (int64)nw;
                if (err != default!) {
                    goto ᒐdone;
                }
            }
        }
        (n, err) = (n, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, err);
}

partial struct FlightRecorderConfig {
    // MinAge is a lower bound on the age of an event in the flight recorder's window.
    //
    // The flight recorder will strive to promptly discard events older than the minimum age,
    // but older events may appear in the window snapshot. The age setting will always be
    // overridden by MaxBytes.
    //
    // If this is 0, the minimum age is implementation defined, but can be assumed to be on the order
    // of seconds.
    public time.Duration MinAge;
    // MaxBytes is an upper bound on the size of the window in bytes.
    //
    // This setting takes precedence over MinAge.
    // However, it does not make any guarantees on the size of the data WriteTo will write,
    // nor does it guarantee memory overheads will always stay below MaxBytes. Treat it
    // as a hint.
    //
    // If this is 0, the maximum size is implementation defined.
    public uint64 MaxBytes;
}

//go:linkname runtime_traceClockUnitsPerSecond
internal static uint64 runtime_traceClockUnitsPerSecond() {
    throw panic("go2cs: //go:linkname push runtime.traceClockUnitsPerSecond -> runtime/trace.runtime_traceClockUnitsPerSecond is not honored: the flight recorder reads the converted execution tracer's generations, and the managed tracer (golib ExecutionTracer, behind runtime.StartTrace) has none to advance or clock; runtime/trace.Start is supported, trace.FlightRecorder is not");
}

//go:linkname runtime_traceAdvance runtime.traceAdvance
internal static partial void runtime_traceAdvance(bool stopTrace);

} // end trace_package
