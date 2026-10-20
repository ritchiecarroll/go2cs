// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bufio = bufio_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using slices = slices_package;
using strings = strings_package;
using tracev1 = go.@internal.trace.@internal.tracev1_package;
using tracev2 = go.@internal.trace.tracev2_package;
using version = go.@internal.trace.version_package;
using go.@internal.trace;
using go.@internal.trace.@internal;

partial class trace_package {

// Reader reads a byte stream, validates it, and produces trace events.
//
// Provided the trace is non-empty the Reader always produces a Sync
// event as the first event, and a Sync event as the last event.
// (There may also be any number of Sync events in the middle, too.)
partial struct Reader {
    internal version.Version version;
    internal ж<bufio.Reader> r;
    internal ΔTime lastTs;
    internal ж<generation> gen;
    internal slice<ж<batchCursor>> frontier;
    internal slice<cpuSample> cpuSamples;
    internal ordering order;
    internal nint syncs;
    internal error readGenErr;
    internal bool done;
    // Spill state.
    //
    // Traces before Go 1.26 had no explicit end-of-generation signal, and
    // so the first batch of the next generation needed to be parsed to identify
    // a new generation. This batch is the "spilled" so we don't lose track
    // of it when parsing the next generation.
    //
    // This is unnecessary after Go 1.26 because of an explicit end-of-generation
    // signal.
    internal ж<spilledBatch> spill;
    internal error spillErr; // error from reading spill
    internal bool spillErrSync;  // whether we emitted a Sync before reporting spillErr
    internal ж<traceV1Converter> v1Events;
}

// NewReader creates a new trace reader.
public static (ж<Reader>, error) NewReader(io.Reader r) {
    var br = bufio.NewReader(r);
    ref var v = ref heap<version.Version>(out var Ꮡv);
    (v, var err) = version.ReadHeader(new bufio_ReaderжReader(br));
    if (err != default!) {
        return (default!, err);
    }
    var exprᴛ1 = v;
    if (exprᴛ1 == version.Go111 || exprᴛ1 == version.Go119 || exprᴛ1 == version.Go121) {
        var (tr, errΔ2) = tracev1.Parse(new bufio_ReaderжReader(br), v);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        return (Ꮡ(new Reader(
            v1Events: convertV1Trace(tr)
        )), default!);
    }
    if (exprᴛ1 == version.Go122 || exprᴛ1 == version.Go123 || exprᴛ1 == version.Go125 || exprᴛ1 == version.Go126) {
        return (Ꮡ(new Reader(
            version: v,
            r: br,
            order: new ordering(
                traceVer: v,
                mStates: new map<ThreadID, ж<mState>>(),
                pStates: new map<ProcID, ж<pState>>(),
                gStates: new map<GoID, ж<gState>>(),
                activeTasks: new map<TaskID, taskState>()
            )
        )), default!);
    }
    { /* default: */
        return (default!, fmt.Errorf("unknown or unsupported version go 1.%d"u8, v));
    }
}

// ReadEvent reads a single event from the stream.
//
// If the stream has been exhausted, it returns an invalid event and io.EOF.
public static (ΔEvent e, error err) ReadEvent(this ж<Reader> Ꮡr) {
    ΔEvent e = new();
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        // Return only io.EOF if we're done.
        if (r.done) {
            (e, err) = (new ΔEvent(nil), io.EOF); goto ᒐdone;
        }

        // Handle v1 execution traces.
        if (r.v1Events != nil) {
            if (r.syncs == 0) {
                // Always emit a sync event first, if we have any events at all.
                var (evΔ1, okΔ1) = r.v1Events.of(traceV1Converter.Ꮡevents).Peek();
                if (okΔ1) {
                    r.syncs++;
                    (e, err) = (syncEvent((~r.v1Events).evt, (ΔTime)(int64)((~evΔ1).Ts - 1), r.syncs), default!); goto ᒐdone;
                }
            }
            var (evΔ2, errΔ1) = r.v1Events.next();
            if (AreEqual(errΔ1, io.EOF)){
                // Always emit a sync event at the end.
                r.done = true;
                r.syncs++;
                (e, err) = (syncEvent(nil, (~r.v1Events).lastTs + 1, r.syncs), default!); goto ᒐdone;
            } else 
            if (errΔ1 != default!) {
                (e, err) = (new ΔEvent(nil), errΔ1); goto ᒐdone;
            }
            (e, err) = (evΔ2.ΔClone(), default!); goto ᒐdone;
        }

        // Trace v2 parsing algorithm.
        //
        // (1) Read in all the batches for the next generation from the stream.
        //   (a) Use the size field in the header to quickly find all batches.
        // (2) Parse out the strings, stacks, CPU samples, and timestamp conversion data.
        // (3) Group each event batch by M, sorted by timestamp. (batchCursor contains the groups.)
        // (4) Organize batchCursors in a min-heap, ordered by the timestamp of the next event for each M.
        // (5) Try to advance the next event for the M at the top of the min-heap.
        //   (a) On success, select that M.
        //   (b) On failure, sort the min-heap and try to advance other Ms. Select the first M that advances.
        //   (c) If there's nothing left to advance, goto (1).
        // (6) Select the latest event for the selected M and get it ready to be returned.
        // (7) Read the next event for the selected M and update the min-heap.
        // (8) Return the selected event, goto (5) on the next call.

        // Set us up to track the last timestamp and fix up
        // the timestamp of any event that comes through.
        defer(() => {
            if (err != default!) {
                return;
            }
            {
                err = e.validateTableIDs(); if (err != default!) {
                    return;
                }
            }
            if (e.@base.time <= Ꮡr.Value.lastTs) {
                e.@base.time = Ꮡr.Value.lastTs + 1;
            }
            Ꮡr.Value.lastTs = e.@base.time;
        }, ref ᒐ);

        // Consume any events in the ordering first.
        {
            var (evΔ3, okΔ2) = r.order.Next(); if (okΔ2) {
                (e, err) = (evΔ3.ΔClone(), default!); goto ᒐdone;
            }
        }

        // Check if we need to refresh the generation.
        if (len(r.frontier) == 0 && len(r.cpuSamples) == 0) {
            if (r.version < version.Go126) {
                (e, err) = r.nextGenWithSpill(); goto ᒐdone;
            }
            if (r.readGenErr != default!) {
                (e, err) = (new ΔEvent(nil), r.readGenErr); goto ᒐdone;
            }
            var (gen, errΔ2) = readGeneration(r.r, r.version);
            if (errΔ2 != default!) {
                // Before returning an error, emit the sync event
                // for the current generation and queue up the error
                // for the next call.
                r.readGenErr = errΔ2;
                r.gen = default!;
                r.syncs++;
                (e, err) = (syncEvent(nil, r.lastTs, r.syncs), default!); goto ᒐdone;
            }
            (e, err) = r.installGen(gen); goto ᒐdone;
        }
        (bool, error) tryAdvance(nint i) {
            var bc = Ꮡr.Value.frontier[i];

            {
                var (okΔ3, errΔ3) = Ꮡr.of(Reader.Ꮡorder).Advance(bc.of(batchCursor.Ꮡev), (~Ꮡr.Value.gen).evTable, (~bc).m, (~Ꮡr.Value.gen).gen); if (!okΔ3 || errΔ3 != default!) {
                    return (okΔ3, errΔ3);
                }
            }

            // Refresh the cursor's event.
            var (okΔ4, errΔ4) = bc.nextEvent((~Ꮡr.Value.gen).batches[(~bc).m], (~Ꮡr.Value.gen).freq);
            if (errΔ4 != default!) {
                return (false, errΔ4);
            }
            if (okΔ4){
                // If we successfully refreshed, update the heap.
                heapUpdate(Ꮡr.Value.frontier, i);
            } else {
                // There's nothing else to read. Delete this cursor from the frontier.
                Ꮡr.Value.frontier = heapRemove(Ꮡr.Value.frontier, i);
            }
            return (true, default!);
        }
        // Inject a CPU sample if it comes next.
        if (len(r.cpuSamples) != 0) {
            if (len(r.frontier) == 0 || r.cpuSamples[0].time < (~r.frontier[0]).ev.time) {
                var eΔ1 = r.cpuSamples[0].asEvent((~r.gen).evTable);
                r.cpuSamples = r.cpuSamples[1..];
                (e, err) = (eΔ1.ΔClone(), default!); goto ᒐdone;
            }
        }
        // Try to advance the head of the frontier, which should have the minimum timestamp.
        // This should be by far the most common case
        if (len(r.frontier) == 0) {
            (e, err) = (new ΔEvent(nil), fmt.Errorf("broken trace: frontier is empty:\n[gen=%d]\n\n%s\n%s\n"u8, (~r.gen).gen, dumpFrontier(r.frontier), dumpOrdering(ref nonnil(ref r).order))); goto ᒐdone;
        }
        {
            var (okΔ5, errΔ5) = tryAdvance(0); if (errΔ5 != default!){
                (e, err) = (new ΔEvent(nil), errΔ5); goto ᒐdone;
            } else 
            if (!okΔ5) {
                // Try to advance the rest of the frontier, in timestamp order.
                //
                // To do this, sort the min-heap. A sorted min-heap is still a
                // min-heap, but now we can iterate over the rest and try to
                // advance in order. This path should be rare.
                slices.SortFunc<slice<ж<batchCursor>>, ж<batchCursor>>(r.frontier, ((Func<ж<batchCursor>, ж<batchCursor>, nint>)(compare)));
                var success = false;
                for (nint i = 1; i < len(r.frontier); i++) {
                    {
                        (okΔ5, errΔ5) = tryAdvance(i); if (errΔ5 != default!){
                            (e, err) = (new ΔEvent(nil), errΔ5); goto ᒐdone;
                        } else 
                        if (okΔ5) {
                            success = true;
                            break;
                        }
                    }
                }
                if (!success) {
                    (e, err) = (new ΔEvent(nil), fmt.Errorf("broken trace: failed to advance: frontier:\n[gen=%d]\n\n%s\n%s\n"u8, (~r.gen).gen, dumpFrontier(r.frontier), dumpOrdering(ref nonnil(ref r).order))); goto ᒐdone;
                }
            }
        }

        // Pick off the next event on the queue. At this point, one must exist.
        var (ev, ok) = r.order.Next();
        if (!ok) {
            throw panic("invariant violation: advance successful, but queue is empty");
        }
        (e, err) = (ev.ΔClone(), default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (e, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string internalErrorˢ4 = "internal error: nextGenWithSpill called for Go 1.26+ trace"u8;

// nextGenWithSpill reads the generation and calls nextGen while
// also handling any spilled batches.
internal static (ΔEvent, error) nextGenWithSpill(this ref Reader r) {
    if (r.version >= version.Go126) {
        return (new ΔEvent(nil), errors.New(internalErrorˢ4));
    }
    if (r.spillErr != default!) {
        if (r.spillErrSync) {
            return (new ΔEvent(nil), r.spillErr);
        }
        r.spillErrSync = true;
        r.syncs++;
        return (syncEvent(nil, r.lastTs, r.syncs), default!);
    }
    if (r.gen != nil && r.spill == nil) {
        // If we have a generation from the last read,
        // and there's nothing left in the frontier, and
        // there's no spilled batch, indicating that there's
        // no further generation, it means we're done.
        // Emit the final sync event.
        r.done = true;
        r.syncs++;
        return (syncEvent(nil, r.lastTs, r.syncs), default!);
    }

    // Read the next generation.
    ж<generation> gen = default!;
    (gen, r.spill, r.spillErr) = readGenerationWithSpill(r.r, r.spill, r.version);
    if (gen == nil) {
        r.gen = default!;
        r.spillErrSync = true;
        r.syncs++;
        return (syncEvent(nil, r.lastTs, r.syncs), default!);
    }
    return r.installGen(gen);
}

// installGen installs the new generation into the Reader and returns
// a Sync event for the new generation.
internal static (ΔEvent, error) installGen(this ref Reader r, ж<generation> Ꮡgen) {
    ref var gen = ref Ꮡgen.DerefOrNull();

    if (Ꮡgen == nil) {
        // Emit the final sync event.
        r.gen = default!;
        r.done = true;
        r.syncs++;
        return (syncEvent(nil, r.lastTs, r.syncs), default!);
    }
    r.gen = Ꮡgen;

    // Reset CPU samples cursor.
    r.cpuSamples = r.gen.Value.cpuSamples;

    // Reset frontier.
    foreach (var (_, vᴛ1) in (~r.gen).batchMs) {
        ref var m = ref heap(new ThreadID(), out var Ꮡm);
        m = vᴛ1;

        var batches = (~r.gen).batches[m];
        var bc = Ꮡ(new batchCursor(m: m));
        var (ok, err) = bc.nextEvent(batches, (~r.gen).freq);
        if (err != default!) {
            return (new ΔEvent(nil), err);
        }
        if (!ok) {
            // Turns out there aren't actually any events in these batches.
            continue;
        }
        r.frontier = heapInsert(r.frontier, bc);
    }
    r.syncs++;

    // Always emit a sync event at the beginning of the generation.
    return (syncEvent((~r.gen).evTable, (~r.gen).freq.mul((~r.gen).minTs), r.syncs), default!);
}

internal static @string dumpFrontier(slice<ж<batchCursor>> frontier) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    foreach (var (_, bc) in frontier) {
        var spec = tracev2.Specs()[(~bc).ev.typ];
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "M %d [%s time=%d"u8, (~bc).m, spec.Name, (~bc).ev.time);
        foreach (var (i, arg) in spec.Args[1..]) {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " %s=%d"u8, arg, (~bc).ev.args[i]);
        }
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "]\n"u8);
    }
    return sb.String();
}

} // end trace_package
