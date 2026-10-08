// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bufio = bufio_package;
using fmt = fmt_package;
using io = io_package;
using slices = slices_package;
using strings = strings_package;
using go122 = go.@internal.trace.@event.go122_package;
using oldtrace = go.@internal.trace.@internal.oldtrace_package;
using version = go.@internal.trace.version_package;
using @event = go.@internal.trace.event_package;
using go.@internal.trace;
using go.@internal.trace.@event;
using go.@internal.trace.@internal;

partial class trace_package {

// Reader reads a byte stream, validates it, and produces trace events.
partial struct Reader {
    internal ж<bufio.Reader> r;
    internal ΔTime lastTs;
    internal ж<generation> gen;
    internal ж<spilledBatch> spill;
    internal error spillErr; // error from reading spill
    internal slice<ж<batchCursor>> frontier;
    internal slice<cpuSample> cpuSamples;
    internal ordering order;
    internal bool emittedSync;
    internal ж<oldTraceConverter> go121Events;
}

// NewReader creates a new trace reader.
public static (ж<Reader>, error) NewReader(io.Reader r) {
    var br = bufio.NewReader(r);
    var (v, err) = version.ReadHeader(new bufio_ReaderжReader(br));
    if (err != default!) {
        return (default!, err);
    }
    var exprᴛ1 = v;
    if (exprᴛ1 == version.Go111 || exprᴛ1 == version.Go119 || exprᴛ1 == version.Go121) {
        var (tr, errΔ2) = oldtrace.Parse(new bufio_ReaderжReader(br), v);
        if (errΔ2 != default!) {
            return (default!, errΔ2);
        }
        return (Ꮡ(new Reader(
            go121Events: convertOldFormat(tr)
        )), default!);
    }
    if (exprᴛ1 == version.Go122 || exprᴛ1 == version.Go123) {
        return (Ꮡ(new Reader(
            r: br,
            order: new ordering(
                mStates: new map<ThreadID, ж<mState>>(),
                pStates: new map<ProcID, ж<pState>>(),
                gStates: new map<GoID, ж<gState>>(),
                activeTasks: new map<TaskID, taskState>()
            ), // Don't emit a sync event when we first go to emit events.

            emittedSync: true
        )), default!);
    }
    { /* default: */
        return (default!, fmt.Errorf("unknown or unsupported version go 1.%d"u8, v));
    }

}

// ReadEvent reads a single event from the stream.
//
// If the stream has been exhausted, it returns an invalid
// event and io.EOF.
public static (ΔEvent e, error err) ReadEvent(this ж<Reader> Ꮡr) {
    ΔEvent e = new();
    error err = default!;
    GoFrame ᒐ = default;
    try {
        ref var r = ref Ꮡr.DerefOrNull();

        if (r.go121Events != nil) {
            var (evΔ1, errΔ1) = r.go121Events.next();
            if (errΔ1 != default!) {
                // XXX do we have to emit an EventSync when the trace is done?
                (e, err) = (new ΔEvent(nil), errΔ1); goto ᒐdone;
            }
            (e, err) = (evΔ1.ΔClone(), default!); goto ᒐdone;
        }
        // Go 1.22+ trace parsing algorithm.
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
            var (evΔ2, okΔ1) = r.order.Next(); if (okΔ1) {
                (e, err) = (evΔ2.ΔClone(), default!); goto ᒐdone;
            }
        }
        // Check if we need to refresh the generation.
        if (len(r.frontier) == 0 && len(r.cpuSamples) == 0) {
            if (!r.emittedSync) {
                r.emittedSync = true;
                (e, err) = (syncEvent((~r.gen).evTable, r.lastTs), default!); goto ᒐdone;
            }
            if (r.spillErr != default!) {
                (e, err) = (new ΔEvent(nil), r.spillErr); goto ᒐdone;
            }
            if (r.gen != nil && r.spill == nil) {
                // If we have a generation from the last read,
                // and there's nothing left in the frontier, and
                // there's no spilled batch, indicating that there's
                // no further generation, it means we're done.
                // Return io.EOF.
                (e, err) = (new ΔEvent(nil), io.EOF); goto ᒐdone;
            }
            // Read the next generation.
            error errΔ2 = default!;
            (r.gen, r.spill, errΔ2) = readGeneration(r.r, r.spill);
            if (r.gen == nil) {
                (e, err) = (new ΔEvent(nil), errΔ2); goto ᒐdone;
            }
            r.spillErr = errΔ2;
            // Reset CPU samples cursor.
            r.cpuSamples = r.gen.Value.cpuSamples;
            // Reset frontier.
            foreach (var (_, vᴛ1) in (~r.gen).batchMs) {
                ref var m = ref heap(new ThreadID(), out var Ꮡm);
                m = vᴛ1;

                var batches = (~r.gen).batches[m];
                var bc = Ꮡ(new batchCursor(m: m));
                var (okΔ2, errΔ3) = bc.nextEvent(batches, (~r.gen).freq);
                if (errΔ3 != default!) {
                    (e, err) = (new ΔEvent(nil), errΔ3); goto ᒐdone;
                }
                if (!okΔ2) {
                    // Turns out there aren't actually any events in these batches.
                    continue;
                }
                r.frontier = heapInsert(r.frontier, bc);
            }
            // Reset emittedSync.
            r.emittedSync = false;
        }
        (bool, error) tryAdvance(nint i) {
            var bc = Ꮡr.Value.frontier[i];
            {
                var (okΔ3, errΔ4) = Ꮡr.of(Reader.Ꮡorder).Advance(bc.of(batchCursor.Ꮡev), (~Ꮡr.Value.gen).evTable, (~bc).m, (~Ꮡr.Value.gen).gen); if (!okΔ3 || errΔ4 != default!) {
                    return (okΔ3, errΔ4);
                }
            }
            // Refresh the cursor's event.
            var (okΔ4, errΔ5) = bc.nextEvent((~Ꮡr.Value.gen).batches[(~bc).m], (~Ꮡr.Value.gen).freq);
            if (errΔ5 != default!) {
                return (false, errΔ5);
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
            var (okΔ5, errΔ6) = tryAdvance(0); if (errΔ6 != default!){
                (e, err) = (new ΔEvent(nil), errΔ6); goto ᒐdone;
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
                        (okΔ5, errΔ6) = tryAdvance(i); if (errΔ6 != default!){
                            (e, err) = (new ΔEvent(nil), errΔ6); goto ᒐdone;
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

internal static @string dumpFrontier(slice<ж<batchCursor>> frontier) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    foreach (var (_, bc) in frontier) {
        var spec = go122.Specs()[(~bc).ev.typ];
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "M %d [%s time=%d"u8, (~bc).m, spec.Name, (~bc).ev.time);
        foreach (var (i, arg) in spec.Args[1..]) {
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " %s=%d"u8, arg, (~bc).ev.args[i]);
        }
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "]\n"u8);
    }
    return sb.String();
}

} // end trace_package
