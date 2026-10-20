// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using fmt = fmt_package;
using slices = slices_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // added for go linkname usage

partial class trace_package {

// A recorder receives bytes from the runtime tracer, processes it.
partial struct recorder {
    internal ж<FlightRecorder> r;
    internal bool headerReceived;
}

internal static (nint n, error err) Write(this ж<recorder> Ꮡw, slice<byte> b) {
    nint n = default!;
    heap<error>(out var Ꮡerr);
    GoFrame ᒐ = default;
    try {
        ref var w = ref Ꮡw.DerefOrNull();
        ref var err = ref Ꮡerr.ValueSlot;

        var r = w.r;

        var rʗ1 = r;
        defer(() => {
            if (Ꮡerr.ValueSlot != default!) {
                // Propagate errors to the flightrecorder.
                if ((~rʗ1).err == default!) {
                    rʗ1.Value.err = Ꮡerr.ValueSlot;
                }
            }
        }, ref ᒐ);

        if (!w.headerReceived) {
            if (len(b) < len((~r).header)) {
                (n, err) = (0, fmt.Errorf("expected at least %d bytes in the first write"u8, len((~r).header))); goto ᒐdone;
            }
            r.Value.header = new array<byte>(b[..16], 16);
            n += 16;
            w.headerReceived = true;
        }
        if (len(b) == n) {
            (n, err) = (n, default!); goto ᒐdone;
        }
        (var ba, var nb, err) = readBatch(b.slice(n)); // Every write from the runtime is guaranteed to be a complete batch.
        if (err != default!) {
            (n, err) = (len(b) - (nint)nb - n, err); goto ᒐdone;
        }
        n += (nint)nb;

        // Append the batch to the current generation.
        if (ba.gen != 0 && (~r).active.gen == 0) {
            r.Value.active.gen = ba.gen;
        }
        if (ba.time != 0 && ((~r).active.minTime == 0 || (~r).active.minTime > (~r).freq.mul(ba.time))) {
            r.Value.active.minTime = (~r).freq.mul(ba.time);
        }
        r.Value.active.size += len(ba.data);
        r.Value.active.batches = append((~r).active.batches, ba.data);

        (n, err) = (len(b), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (n, Ꮡerr.ValueSlot);
}

internal static void endGeneration(this ref recorder w) {
    var r = w.r;

    // Check if we're entering a new generation.
    r.of(FlightRecorder.ᏑringMu).Lock();

    // Get the current trace clock time.
    var now = traceTimeNow((~r).freq);

    // Add the current generation to the ring. Make sure we always have at least one
    // complete generation by putting the active generation onto the new list, regardless
    // of whatever our settings are.
    //
    // N.B. Let's completely replace the ring here, so that WriteTo can just make a copy
    // and not worry about aliasing. This creates allocations, but at a very low rate.
    var newRing = new rawGeneration[]{(~r).active}.slice();
    nint size = r.Value.active.size;
    for (nint i = len((~r).ring) - 1; i >= 0; i--) {
        // Stop adding older generations if the new ring already exceeds the thresholds.
        // This ensures we keep generations that cross a threshold, but not any that lie
        // entirely outside it.
        if ((uint64)size > (~r).wantSize || now.Sub(newRing[len(newRing) - 1].minTime) > (~r).wantDur) {
            break;
        }
        size += (~r).ring[i].size;
        newRing = append(newRing, (~r).ring[i]);
    }
    slices.Reverse<slice<rawGeneration>, rawGeneration>(newRing);
    r.Value.ring = newRing;
    r.of(FlightRecorder.ᏑringMu).Unlock();

    // Start a new active generation.
    r.Value.active = new rawGeneration(nil);
}

partial struct rawGeneration {
    internal uint64 gen;
    internal nint size;
    internal eventTime minTime;
    internal slice<slice<byte>> batches;
}

internal static eventTime traceTimeNow(frequency freq) {
    return freq.mul((timestamp)runtime_traceClockNow());
}

//go:linkname runtime_traceClockNow runtime.traceClockNow
internal static partial uint64 runtime_traceClockNow();

// frequency is nanoseconds per timestamp unit.
partial struct frequency /*num:float64*/;

// mul multiplies an unprocessed timestamp to produce a time in nanoseconds.
internal static eventTime mul(this frequency f, timestamp t) {
    return (eventTime)(int64)((float64)(uint64)t * (float64)f);
}

// eventTime is a timestamp in nanoseconds.
//
// It corresponds to the monotonic clock on the platform that the
// trace was taken, and so is possible to correlate with timestamps
// for other traces taken on the same machine using the same clock
// (i.e. no reboots in between).
//
// The actual absolute value of the timestamp is only meaningful in
// relation to other timestamps from the same clock.
//
// BUG: Timestamps coming from traces on Windows platforms are
// only comparable with timestamps from the same trace. Timestamps
// across traces cannot be compared, because the system clock is
// not used as of Go 1.22.
//
// BUG: Traces produced by Go versions 1.21 and earlier cannot be
// compared with timestamps from other traces taken on the same
// machine. This is because the system clock was not used at all
// to collect those timestamps.
partial struct eventTime /*num:int64*/;

// Sub subtracts t0 from t, returning the duration in nanoseconds.
internal static time.Duration Sub(this eventTime t, eventTime t0) {
    return (time.Duration)((int64)t - (int64)t0);
}

} // end trace_package
