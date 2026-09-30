// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package oldtrace implements a parser for Go execution traces from versions
// 1.11–1.21.
//
// The package started as a copy of Go 1.19's internal/trace, but has been
// optimized to be faster while using less memory and fewer allocations. It has
// been further modified for the specific purpose of converting traces to the
// new 1.22+ format.
namespace go.@internal.trace.@internal;

using bytes = bytes_package;
using cmp = cmp_package;
using binary = encoding.binary_package;
using errors = errors_package;
using fmt = fmt_package;
using @event = go.@internal.trace.event_package;
using version = go.@internal.trace.version_package;
using io = io_package;
using math = math_package;
using slices = slices_package;
using sort = sort_package;
using encoding;
using go.@internal.trace;

partial class oldtrace_package {

[GoType("num:int64")] partial struct Timestamp;

// Event describes one event in the trace.
[GoType] partial struct Event {
// The Event type is carefully laid out to optimize its size and to avoid
// pointers, the latter so that the garbage collector won't have to scan any
// memory of our millions of events.
    public Timestamp Ts;  // timestamp in nanoseconds
    public uint64 G;     // G on which the event happened
    public array<uint64> Args = new(4); // event-type-specific arguments
    public uint32 StkID;     // unique stack ID
    public int32 P;      // P on which the event happened (can be a real P or one of TimerP, NetpollP, SyscallP)
    public @event.Type Type; // one of Ev*
}

// Frame is a frame in stack traces.
[GoType] partial struct Frame {
    public uint64 PC;
    // string ID of the function name
    public uint64 Fn;
    // string ID of the file name
    public uint64 File;
    public nint Line;
}

public static UntypedInt FakeP => /* 1000000 + iota */ 1000000;
public static UntypedInt TimerP => 1000001; // contains timer unblocks
public static UntypedInt NetpollP => 1000002; // contains network unblocks
public static UntypedInt SyscallP => 1000003; // contains returns from syscalls
public static UntypedInt GCP => 1000004; // contains GC state
public static UntypedInt ProfileP => 1000005; // contains recording of CPU profile samples

// Trace is the result of Parse.
[GoType] partial struct Trace {
    public version.Version Version;
    // Events is the sorted list of Events in the trace.
    public Events Events;
    // Stacks is the stack traces (stored as slices of PCs), keyed by stack IDs
    // from the trace.
    public map<uint32, slice<uint64>> Stacks;
    public map<uint64, Frame> PCs;
    public map<uint64, @string> Strings;
    public slice<@string> InlineStrings;
}

// batchOffset records the byte offset of, and number of events in, a batch. A
// batch is a sequence of events emitted by a P. Events within a single batch
// are sorted by time.
[GoType] partial struct batchOffset {
    internal nint offset;
    internal nint numEvents;
}

[GoType] partial struct parser {
    internal version.Version ver;
    internal slice<byte> data;
    internal nint off;
    internal map<uint64, @string> strings;
    internal slice<@string> inlineStrings;
    internal map<@string, nint> inlineStringsMapping;
    // map from Ps to their batch offsets
    internal map<int32, slice<batchOffset>> batchOffsets;
    internal map<uint32, slice<uint64>> stacks;
    internal slice<uint64> stacksData;
    internal int64 ticksPerSec;
    internal map<uint64, Frame> pcs;
    internal slice<Event> cpuSamples;
    internal map<uint64, bool> timerGoids;
    // state for readRawEvent
    internal slice<uint64> args;
    // state for parseEvent
    internal Timestamp lastTs;
    internal uint64 lastG;
    // map from Ps to the last Gs that ran on them
    internal map<int32, uint64> lastGs;
    internal int32 lastP;
}

[GoRecv] internal static bool discard(this ref parser p, uint64 n) {
    if (n > math.MaxInt) {
        return false;
    }
    {
        nint noff = p.off + (nint)n; if (noff < p.off || noff > len(p.data)){
            return false;
        } else {
            p.off = noff;
        }
    }
    return true;
}

internal static (ж<parser>, error) newParser(io.Reader r, version.Version ver) {
    slice<byte> buf = default!;
    {
        var (seeker, ok) = r._<io.Seeker>(ᐧ); if (ok){
            // Determine the size of the reader so that we can allocate a buffer
            // without having to grow it later.
            var (cur, err) = seeker.Seek(0, io.SeekCurrent);
            if (err != default!) {
                return (default!, err);
            }
            (var end, err) = seeker.Seek(0, io.SeekEnd);
            if (err != default!) {
                return (default!, err);
            }
            (_, err) = seeker.Seek(cur, io.SeekStart);
            if (err != default!) {
                return (default!, err);
            }
            buf = new slice<byte>((nint)(end - cur));
            (_, err) = io.ReadFull(r, buf);
            if (err != default!) {
                return (default!, err);
            }
        } else {
            error err = default!;
            (buf, err) = io.ReadAll(r);
            if (err != default!) {
                return (default!, err);
            }
        }
    }
    return (Ꮡ(new parser(data: buf, ver: ver, timerGoids: new map<uint64, bool>())), default!);
}

// Parse parses Go execution traces from versions 1.11–1.21. The provided reader
// will be read to completion and the entire trace will be materialized in
// memory. That is, this function does not allow incremental parsing.
//
// The reader has to be positioned just after the trace header and vers needs to
// be the version of the trace. This can be achieved by using
// version.ReadHeader.
public static (Trace, error) Parse(io.Reader r, version.Version vers) {
    // We accept the version as an argument because internal/trace will have
    // already read the version to determine which parser to use.
    var (p, err) = newParser(r, vers);
    if (err != default!) {
        return (new Trace(nil), err);
    }
    return p.parse();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noEvFrequencyEventˢ = "no EvFrequency event"u8;

// parse parses, post-processes and verifies the trace.
internal static (Trace, error) parse(this ж<parser> Ꮡp) {
    GoFrame ᒐ = default;
    try {
        ref var p = ref Ꮡp.DerefOrNull();

        defer(() => {
            Ꮡp.Value.data = default!;
        }, ref ᒐ);
        // We parse a trace by running the following steps in order:
        //
        // 1. In the initial pass we collect information about batches (their
        //    locations and sizes.) We also parse CPU profiling samples in this
        //    step, simply to reduce the number of full passes that we need.
        //
        // 2. In the second pass we parse batches and merge them into a globally
        //    ordered event stream. This uses the batch information from the first
        //    pass to quickly find batches.
        //
        // 3. After all events have been parsed we convert their timestamps from CPU
        //    ticks to wall time. Furthermore we move timers and syscalls to
        //    dedicated, fake Ps.
        //
        // 4. Finally, we validate the trace.
        p.strings = new map<uint64, @string>();
        p.batchOffsets = new map<int32, slice<batchOffset>>();
        p.lastGs = new map<int32, uint64>();
        p.stacks = new map<uint32, slice<uint64>>();
        p.pcs = new map<uint64, Frame>();
        p.inlineStringsMapping = new map<@string, nint>();
        {
            var errΔ1 = p.collectBatchesAndCPUSamples(); if (errΔ1 != default!) {
                return (new Trace(nil), errΔ1);
            }
        }
        var (events, err) = p.parseEventBatches();
        if (err != default!) {
            return (new Trace(nil), err);
        }
        if (p.ticksPerSec == 0) {
            return (new Trace(nil), errors.New(noEvFrequencyEventˢ));
        }
        if (events.Len() > 0) {
            // Translate cpu ticks to real time.
            var minTs = events.Ptr(0).Value.Ts;
            // Use floating point to avoid integer overflows.
            var freq = 1e9D / (float64)p.ticksPerSec;
            for (nint i = 0; i < events.Len(); i++) {
                var ev = events.Ptr(i);
                ev.Value.Ts = ((Timestamp)(int64)((float64)(int64)((~ev).Ts - minTs) * freq));
                // Move timers and syscalls to separate fake Ps.
                if (p.timerGoids[(~ev).G] && (~ev).Type == EvGoUnblock) {
                    ev.Value.P = TimerP;
                }
                if ((~ev).Type == EvGoSysExit) {
                    ev.Value.P = SyscallP;
                }
            }
        }
        {
            var errΔ2 = Ꮡp.postProcessTrace(events); if (errΔ2 != default!) {
                return (new Trace(nil), errΔ2);
            }
        }
        var res = new Trace(
            Version: p.ver,
            Events: events,
            Stacks: p.stacks,
            Strings: p.strings,
            InlineStrings: p.inlineStrings,
            PCs: p.pcs
        );
        return (res, default!);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// rawEvent is a helper type used during parsing.
[GoType] partial struct rawEvent {
    internal @event.Type typ;
    internal slice<uint64> args;
    internal slice<@string> sargs;
    // if typ == EvBatch, these fields describe the batch.
    internal int32 batchPid;
    internal nint batchOffset;
}

[GoType] partial struct proc {
    internal int32 pid;
    // the remaining events in the current batch
    internal slice<Event> events;
    // buffer for reading batches into, aliased by proc.events
    internal slice<Event> buf;
    // there are no more batches left
    internal bool done;
}

internal static UntypedInt eventsBucketSize => 524288; // 32 MiB of events

[GoType] partial struct Events {
    // Events is a slice of slices that grows one slice of size eventsBucketSize
    // at a time. This avoids the O(n) cost of slice growth in append, and
    // additionally allows consumers to drop references to parts of the data,
    // freeing memory piecewise.
    internal nint n;
    [GoArrayDims(524288)]
    internal slice<ж<array<Event>>> buckets;
    internal nint off;
}

// grow grows the slice by one and returns a pointer to the new element, without
// overwriting it.
[GoRecv] internal static ж<Event> grow(this ref Events l) {
    var (a, b) = l.index(l.n);
    if (a >= len(l.buckets)) {
        l.buckets = builtin.append(l.buckets, Ꮡ(new array<Event>(524288, () => new())));
    }
    var ptr = l.buckets[a].at<Event>(b);
    l.n++;
    return ptr;
}

// append appends v to the slice and returns a pointer to the new element.
[GoRecv] internal static ж<Event> append(this ref Events l, Event v) {
    v = v.ΔClone();

    var ptr = l.grow();
    ptr.Value = v.ΔClone();
    return ptr;
}

[GoRecv] public static ж<Event> Ptr(this ref Events l, nint i) {
    var (a, b) = l.index(i + l.off);
    return l.buckets[a].at<Event>(b);
}

[GoRecv] internal static (nint, nint) index(this ref Events l, nint i) {
    // Doing the division on uint instead of int compiles this function to a
    // shift and an AND (for power of 2 bucket sizes), versus a whole bunch of
    // instructions for int.
    return ((nint)((nuint)i / (nuint)eventsBucketSize), (nint)((nuint)i % (nuint)eventsBucketSize));
}

[GoRecv] public static nint Len(this ref Events l) {
    return l.n - l.off;
}

[GoRecv] public static bool Less(this ref Events l, nint i, nint j) {
    return (~l.Ptr(i)).Ts < (~l.Ptr(j)).Ts;
}

[GoRecv] public static void Swap(this ref Events l, nint i, nint j) {
    (l.Ptr(i).Value, l.Ptr(j).Value) = (l.Ptr(j).Value.ΔClone(), l.Ptr(i).Value.ΔClone());
}

[GoRecv] public static (ж<Event>, bool) Pop(this ref Events l) {
    if (l.off == l.n) {
        return (default!, false);
    }
    var (a, b) = l.index(l.off);
    var ptr = l.buckets[a].at<Event>(b);
    l.off++;
    if (b == (nint)(eventsBucketSize - 1) || l.off == l.n) {
        // We've consumed the last event from the bucket, so drop the bucket and
        // allow GC to collect it.
        l.buckets[a] = ж<array<Event>>.NilBoxOfDims(524288L);
    }
    return (ptr, true);
}

public static Action<Func<ж<Event>, bool>> All(this ж<Events> Ꮡl) {
    return (Func<ж<Event>, bool> yield) => {
        for (nint i = 0; i < Ꮡl.Value.Len(); i++) {
            var (a, b) = Ꮡl.Value.index(i + Ꮡl.Value.off);
            var ptr = Ꮡl.Value.buckets[a].at<Event>(b);
            if (!yield(ptr)) {
                return;
            }
        }
    };
}

// parseEventBatches reads per-P event batches and merges them into a single, consistent
// stream. The high level idea is as follows. Events within an individual batch
// are in correct order, because they are emitted by a single P. So we need to
// produce a correct interleaving of the batches. To do this we take first
// unmerged event from each batch (frontier). Then choose subset that is "ready"
// to be merged, that is, events for which all dependencies are already merged.
// Then we choose event with the lowest timestamp from the subset, merge it and
// repeat. This approach ensures that we form a consistent stream even if
// timestamps are incorrect (condition observed on some machines).
[GoRecv] internal static (Events, error) parseEventBatches(this ref parser p) {
    // The ordering of CPU profile sample events in the data stream is based on
    // when each run of the signal handler was able to acquire the spinlock,
    // with original timestamps corresponding to when ReadTrace pulled the data
    // off of the profBuf queue. Re-sort them by the timestamp we captured
    // inside the signal handler.
    slices.SortFunc(p.cpuSamples, (Event a, Event b) => {
        a = a.ΔClone();
        b = b.ΔClone();
        return cmp.Compare(a.Ts, b.Ts);
    });
    var allProcs = new slice<proc>(0, len(p.batchOffsets));
    foreach (var (pid, _) in p.batchOffsets) {
        allProcs = builtin.append(allProcs, new proc(pid: pid));
    }
    allProcs = builtin.append(allProcs, new proc(pid: ProfileP, events: p.cpuSamples));
    ref var events = ref heap<Events>(out var Ꮡevents);
    events = new Events(nil);
    // Merge events as long as at least one P has more events
    var gs = new map<uint64, gState>();
    // Note: technically we don't need a priority queue here. We're only ever
    // interested in the earliest eligible event, which means we just have to
    // track the smallest element. However, in practice, the priority queue
    // performs better, because for each event we only have to compute its state
    // transition once, not on each iteration. If it was eligible before, it'll
    // already be in the queue. Furthermore, on average, we only have one P to
    // look at in each iteration, because all other Ps are already in the queue.
    ref var frontier = ref heap<orderEventList>(out var Ꮡfrontier);
    var availableProcs = new slice<ж<proc>>(len(allProcs));
    foreach (var (i, _) in allProcs) {
        availableProcs[i] = Ꮡ(allProcs, i);
    }
    while (ᐧ) {
pidLoop:
        for (nint i = 0; i < len(availableProcs); i++) {
            var proc = availableProcs[i];
            while (len((~proc).events) == 0) {
                // Call loadBatch in a loop because sometimes batches are empty
                var (evs, err) = p.loadBatch((~proc).pid, (~proc).buf[..0]);
                proc.Value.buf = evs[..0];
                if (AreEqual(err, io.EOF)){
                    // This P has no more events
                    proc.Value.done = true;
                    (availableProcs[i], availableProcs[len(availableProcs) - 1]) = (availableProcs[len(availableProcs) - 1], availableProcs[i]);
                    availableProcs = availableProcs.slice(0, len(availableProcs) - 1);
                    // We swapped the element at i with another proc, so look at
                    // the index again
                    i--;
                    goto continue_pidLoop;
                } else 
                if (err != default!){
                    return (new Events(nil), err);
                } else {
                    proc.Value.events = evs;
                }
            }
            var ev = Ꮡ((~proc).events, 0);
            var (gΔ1, initΔ1, _) = stateTransition(ref (ev).DerefOrNull());
            // TODO(dh): This implementation matches the behavior of the
            // upstream 'go tool trace', and works in practice, but has run into
            // the following inconsistency during fuzzing: what happens if
            // multiple Ps have events for the same G? While building the
            // frontier we will check all of the events against the current
            // state of the G. However, when we process the frontier, the state
            // of the G changes, and a transition that was valid while building
            // the frontier may no longer be valid when processing the frontier.
            // Is this something that can happen for real, valid traces, or is
            // this only possible with corrupt data?
            if (!transitionReady(gΔ1, gs[gΔ1], initΔ1)) {
                continue;
            }
            proc.Value.events = (~proc).events[1..];
            (availableProcs[i], availableProcs[len(availableProcs) - 1]) = (availableProcs[len(availableProcs) - 1], availableProcs[i]);
            availableProcs = availableProcs.slice(0, len(availableProcs) - 1);
            Ꮡfrontier.Push(new orderEvent(ev.Value.ΔClone(), proc));
            // We swapped the element at i with another proc, so look at the
            // index again
            i--;
continue_pidLoop:;
        }
break_pidLoop:;
        if (len(frontier) == 0) {
            foreach (var (i, _) in allProcs) {
                if (!allProcs[i].done) {
                    return (new Events(nil), fmt.Errorf("no consistent ordering of events possible"u8));
                }
            }
            break;
        }
        var f = Ꮡfrontier.Pop();
        // We're computing the state transition twice, once when computing the
        // frontier, and now to apply the transition. This is fine because
        // stateTransition is a pure function. Computing it again is cheaper
        // than storing large items in the frontier.
        var (g, init, next) = stateTransition(ref f.ev);
        // Get rid of "Local" events, they are intended merely for ordering.
        var exprᴛ1 = f.ev.Type;
        if (exprᴛ1 == EvGoStartLocal) {
            f.ev.Type = EvGoStart;
        }
        else if (exprᴛ1 == EvGoUnblockLocal) {
            f.ev.Type = EvGoUnblock;
        }
        else if (exprᴛ1 == EvGoSysExitLocal) {
            f.ev.Type = EvGoSysExit;
        }

        events.append(f.ev);
        {
            var err = transition(gs, g, init, next); if (err != default!) {
                return (new Events(nil), err);
            }
        }
        availableProcs = builtin.append(availableProcs, f.proc);
    }
    // At this point we have a consistent stream of events. Make sure time
    // stamps respect the ordering. The tests will skip (not fail) the test case
    // if they see this error.
    if (!sort.IsSorted(new EventsжInterface(Ꮡevents))) {
        return (new Events(nil), ErrTimeOrder);
    }
    // The last part is giving correct timestamps to EvGoSysExit events. The
    // problem with EvGoSysExit is that actual syscall exit timestamp
    // (ev.Args[2]) is potentially acquired long before event emission. So far
    // we've used timestamp of event emission (ev.Ts). We could not set ev.Ts =
    // ev.Args[2] earlier, because it would produce seemingly broken timestamps
    // (misplaced event). We also can't simply update the timestamp and resort
    // events, because if timestamps are broken we will misplace the event and
    // later report logically broken trace (instead of reporting broken
    // timestamps).
    var lastSysBlock = new map<uint64, Timestamp>();
    for (nint i = 0; i < events.Len(); i++) {
        var ev = events.Ptr(i);
        var exprᴛ2 = (~ev).Type;
        if (exprᴛ2 == EvGoSysBlock || exprᴛ2 == EvGoInSyscall) {
            lastSysBlock[(~ev).G] = ev.Value.Ts;
        }
        else if (exprᴛ2 == EvGoSysExit) {
            var ts = ((Timestamp)(int64)(~ev).Args[2]);
            if (ts == 0) {
                continue;
            }
            var block = lastSysBlock[(~ev).G];
            if (block == 0) {
                return (new Events(nil), fmt.Errorf("stray syscall exit"u8));
            }
            if (ts < block) {
                return (new Events(nil), ErrTimeOrder);
            }
            ev.Value.Ts = ts;
        }

    }
    sort.Stable(new EventsжInterface(Ꮡevents));
    return (events, default!);
}

// collectBatchesAndCPUSamples records the offsets of batches and parses CPU samples.
[GoRecv] internal static error collectBatchesAndCPUSamples(this ref parser p) {
    // Read events.
    ref var raw = ref heap(new rawEvent(), out var Ꮡraw);
    int32 curP = default!;
    for (var n = (uint64)0; ᐧ ; n++) {
        var err = p.readRawEvent((nuint)((nuint)skipArgs | (nuint)skipStrings), Ꮡraw);
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != default!) {
            return err;
        }
        if (raw.typ == EvNone) {
            continue;
        }
        if (raw.typ == EvBatch) {
            var bo = new batchOffset(offset: raw.batchOffset);
            p.batchOffsets[raw.batchPid] = builtin.append(p.batchOffsets[raw.batchPid], bo);
            curP = raw.batchPid;
        }
        var batches = p.batchOffsets[curP];
        if (len(batches) == 0) {
            return fmt.Errorf("read event %d with current P of %d, but P has no batches yet"u8,
                raw.typ, curP);
        }
        batches[len(batches) - 1].numEvents++;
        if (raw.typ == EvCPUSample) {
            var e = new Event(Type: raw.typ);
            nint argOffset = 1;
            nint narg = raw.argNum();
            if (len(raw.args) != narg) {
                return fmt.Errorf("CPU sample has wrong number of arguments: want %d, got %d"u8, narg, len(raw.args));
            }
            for (nint i = argOffset; i < narg; i++) {
                if (i == narg - 1){
                    e.StkID = (uint32)raw.args[i];
                } else {
                    e.Args[i - argOffset] = raw.args[i];
                }
            }
            e.Ts = ((Timestamp)(int64)e.Args[0]);
            e.P = (int32)e.Args[1];
            e.G = e.Args[2];
            e.Args[0] = 0;
            // Most events are written out by the active P at the exact moment
            // they describe. CPU profile samples are different because they're
            // written to the tracing log after some delay, by a separate worker
            // goroutine, into a separate buffer.
            //
            // We keep these in their own batch until all of the batches are
            // merged in timestamp order. We also (right before the merge)
            // re-sort these events by the timestamp captured in the profiling
            // signal handler.
            //
            // Note that we're not concerned about the memory usage of storing
            // all CPU samples during the indexing phase. There are orders of
            // magnitude fewer CPU samples than runtime events.
            p.cpuSamples = builtin.append(p.cpuSamples, e.ΔClone());
        }
    }
    return default!;
}

internal static UntypedInt skipArgs => /* 1 << iota */ 1;
internal static UntypedInt skipStrings => 2;

[GoRecv] internal static (byte, bool) readByte(this ref parser p) {
    if (p.off < len(p.data) && p.off >= 0){
        var b = p.data[p.off];
        p.off++;
        return (b, true);
    } else {
        return (0, false);
    }
}

[GoRecv] internal static (slice<byte>, error) readFull(this ref parser p, nint n) {
    if (p.off >= len(p.data) || p.off < 0 || p.off + n > len(p.data)) {
        // p.off < 0 is impossible but makes BCE happy.
        //
        // We do fail outright if there's not enough data, we don't care about
        // partial results.
        return (default!, io.ErrUnexpectedEOF);
    }
    var buf = p.data.slice(p.off, p.off + n);
    p.off += n;
    return (buf, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringHasInvalidId0ˢ = "string has invalid id 0"u8;
internal static readonly @string stringHasInvalidLength0ˢ = "string has invalid length 0"u8;

// readRawEvent reads a raw event into ev. The slices in ev are only valid until
// the next call to readRawEvent, even when storing to a different location.
[GoRecv] internal static error readRawEvent(this ref parser p, nuint flags, ж<rawEvent> Ꮡev) {
    ref var ev = ref Ꮡev.DerefOrNull();

    // The number of arguments is encoded using two bits and can thus only
    // represent the values 0–3. The value 3 (on the wire) indicates that
    // arguments are prefixed by their byte length, to encode >=3 arguments.
    const byte inlineArgs = 3;
    // Read event type and number of arguments (1 byte).
    var (b, ok) = p.readByte();
    if (!ok) {
        return io.EOF;
    }
    var typ = ((@event.Type)(((byte)(b << (int)(2)) >> (int)(2))));
    // Most events have a timestamp before the actual arguments, so we add 1 and
    // parse it like it's the first argument. EvString has a special format and
    // the number of arguments doesn't matter. EvBatch writes '1' as the number
    // of arguments, but actually has two: a pid and a timestamp, but here the
    // timestamp is the second argument, not the first; adding 1 happens to come
    // up with the correct number, but it doesn't matter, because EvBatch has
    // custom logic for parsing.
    //
    // Note that because we're adding 1, inlineArgs == 3 describes the largest
    // number of logical arguments that isn't length-prefixed, even though the
    // value 3 on the wire indicates length-prefixing. For us, that becomes narg
    // == 4.
    var narg = (byte)((b >> (int)(6)) + 1);
    if (typ == EvNone || typ >= EvCount || EventDescriptions[typ].minVersion > p.ver) {
        return fmt.Errorf("unknown event type %d"u8, typ);
    }
    var exprᴛ1 = typ;
    if (exprᴛ1 == EvString) {
        if ((nuint)(flags & (nuint)skipStrings) != 0){
            // String dictionary entry [ID, length, string].
            {
                var (_, errΔ7) = p.readVal(); if (errΔ7 != default!) {
                    return errMalformedVarint;
                }
            }
            var (ln, err) = p.readVal();
            if (err != default!) {
                return err;
            }
            if (!p.discard(ln)) {
                return fmt.Errorf("failed to read trace: %w"u8, io.EOF);
            }
        } else {
            // String dictionary entry [ID, length, string].
            var (id, err) = p.readVal();
            if (err != default!) {
                return err;
            }
            if (id == 0) {
                return errors.New(stringHasInvalidId0ˢ);
            }
            if (p.strings[id] != "") {
                return fmt.Errorf("string has duplicate id %d"u8, id);
            }
            uint64 ln = default!;
            (ln, err) = p.readVal();
            if (err != default!) {
                return err;
            }
            if (ln == 0) {
                return errors.New(stringHasInvalidLength0ˢ);
            }
            if (ln > 1000000) {
                return fmt.Errorf("string has too large length %d"u8, ln);
            }
            (var buf, err) = p.readFull((nint)ln);
            if (err != default!) {
                return fmt.Errorf("failed to read trace: %w"u8, err);
            }
            p.strings[id] = ((@string)buf);
        }
        ev.typ = EvNone;
        return default!;
    }
    if (exprᴛ1 == EvBatch) {
        {
            var want = (byte)2; if (narg != want) {
                return fmt.Errorf("EvBatch has wrong number of arguments: got %d, want %d"u8, narg, want);
            }
        }
        nint off = p.off - 1;
        var (pid, err) = p.readVal();
        if (err != default!) {
            // -1 because we've already read the first byte of the batch
            return err;
        }
        if (pid != math.MaxUint64 && pid > math.MaxInt32) {
            return fmt.Errorf("processor ID %d is larger than maximum of %d"u8, pid, (uint64)math.MaxUint);
        }
        int32 pid32 = default!;
        if (pid == math.MaxUint64){
            pid32 = -1;
        } else {
            pid32 = (int32)pid;
        }
        (var v, err) = p.readVal();
        if (err != default!) {
            return err;
        }
        ev = new rawEvent(
            typ: EvBatch,
            args: p.args[..0],
            batchPid: pid32,
            batchOffset: off
        );
        ev.args = builtin.append(ev.args, pid, v);
        return default!;
    }
    { /* default: */
        ev = new rawEvent(typ: typ, args: p.args[..0]);
        if (narg <= inlineArgs){
            if ((nuint)(flags & (nuint)skipArgs) == 0){
                for (nint i = 0; i < (nint)narg; i++) {
                    var (v, err) = p.readVal();
                    if (err != default!) {
                        return fmt.Errorf("failed to read event %d argument: %w"u8, typ, err);
                    }
                    ev.args = builtin.append(ev.args, v);
                }
            } else {
                for (nint i = 0; i < (nint)narg; i++) {
                    {
                        var (_, err) = p.readVal(); if (err != default!) {
                            return fmt.Errorf("failed to read event %d argument: %w"u8, typ, errMalformedVarint);
                        }
                    }
                }
            }
        } else {
            // More than inlineArgs args, the first value is length of the event
            // in bytes.
            var (v, err) = p.readVal();
            if (err != default!) {
                return fmt.Errorf("failed to read event %d argument: %w"u8, typ, err);
            }
            {
                var limit = (uint64)2048; if (v > limit) {
                    // At the time of Go 1.19, v seems to be at most 128. Set 2048
                    // as a generous upper limit and guard against malformed traces.
                    return fmt.Errorf("failed to read event %d argument: length-prefixed argument too big: %d bytes, limit is %d"u8, typ, v, limit);
                }
            }
            if ((nuint)(flags & (nuint)skipArgs) == 0 || typ == EvCPUSample){
                var (buf, errΔ1) = p.readFull((nint)v);
                if (errΔ1 != default!) {
                    return fmt.Errorf("failed to read trace: %w"u8, errΔ1);
                }
                while (len(buf) > 0) {
                    uint64 vΔ1 = default!;
                    (vΔ1, buf, errΔ1) = readValFrom(buf);
                    if (errΔ1 != default!) {
                        return errΔ1;
                    }
                    ev.args = builtin.append(ev.args, vΔ1);
                }
            } else {
                // Skip over arguments
                if (!p.discard(v)) {
                    return fmt.Errorf("failed to read trace: %w"u8, io.EOF);
                }
            }
            if (typ == EvUserLog) {
                // EvUserLog records are followed by a value string
                if ((nuint)(flags & (nuint)skipArgs) == 0){
                    // Read string
                    var (s, errΔ2) = p.readStr();
                    if (errΔ2 != default!) {
                        return errΔ2;
                    }
                    ev.sargs = builtin.append(ev.sargs, s);
                } else {
                    // Skip string
                    var (vΔ2, errΔ3) = p.readVal();
                    if (errΔ3 != default!) {
                        return errΔ3;
                    }
                    if (!p.discard(vΔ2)) {
                        return io.EOF;
                    }
                }
            }
        }
        p.args = ev.args[..0];
        return default!;
    }

}

// loadBatch loads the next batch for pid and appends its contents to events.
[GoRecv] internal static (slice<Event>, error) loadBatch(this ref parser p, int32 pid, slice<Event> events) {
    var offsets = p.batchOffsets[pid];
    if (len(offsets) == 0) {
        return (default!, io.EOF);
    }
    nint n = offsets[0].numEvents;
    nint offset = offsets[0].offset;
    offsets = offsets[1..];
    p.batchOffsets[pid] = offsets;
    p.off = offset;
    if (cap(events) < n) {
        events = new slice<Event>(0, () => new(), n);
    }
    var gotHeader = false;
    ref var raw = ref heap(new rawEvent(), out var Ꮡraw);
    ref var ev = ref heap(new Event(), out var Ꮡev);
    while (ᐧ) {
        var err = p.readRawEvent(0, Ꮡraw);
        if (AreEqual(err, io.EOF)) {
            break;
        }
        if (err != default!) {
            return (default!, err);
        }
        if (raw.typ == EvNone || raw.typ == EvCPUSample) {
            continue;
        }
        if (raw.typ == EvBatch) {
            if (gotHeader){
                break;
            } else {
                gotHeader = true;
            }
        }
        err = p.parseEvent(Ꮡraw, Ꮡev);
        if (err != default!) {
            return (default!, err);
        }
        if (ev.Type != EvNone) {
            events = builtin.append(events, ev.ΔClone());
        }
    }
    return (events, default!);
}

[GoRecv] internal static (@string s, error err) readStr(this ref parser p) {
    error err = default!;

    (var sz, err) = p.readVal();
    if (err != default!) {
        return ("", err);
    }
    if (sz == 0) {
        return ("", default!);
    }
    if (sz > 1000000) {
        return ("", fmt.Errorf("string is too large (len=%d)"u8, sz));
    }
    (var buf, err) = p.readFull((nint)sz);
    if (err != default!) {
        return ("", fmt.Errorf("failed to read trace: %w"u8, err));
    }
    return (((@string)buf), default!);
}

// parseEvent transforms raw events into events.
// It does analyze and verify per-event-type arguments.
[GoRecv] internal static error parseEvent(this ref parser p, ж<rawEvent> Ꮡraw, ж<Event> Ꮡev) {
    ref var raw = ref Ꮡraw.DerefOrNull();
    ref var ev = ref Ꮡev.DerefOrNull();

    var desc = ᏑEventDescriptions.at<EventDescriptionsᴛ1>((nint)(raw.typ));
    if ((~desc).Name == ""u8) {
        return fmt.Errorf("missing description for event type %d"u8, raw.typ);
    }
    nint narg = raw.argNum();
    if (len(raw.args) != narg) {
        return fmt.Errorf("%s has wrong number of arguments: want %d, got %d"u8, (~desc).Name, narg, len(raw.args));
    }
    var exprᴛ1 = raw.typ;
    if (exprᴛ1 == EvBatch) {
        p.lastGs[p.lastP] = p.lastG;
        if (raw.args[0] != math.MaxUint64 && raw.args[0] > math.MaxInt32) {
            return fmt.Errorf("processor ID %d is larger than maximum of %d"u8, raw.args[0], (uint64)math.MaxInt32);
        }
        if (raw.args[0] == math.MaxUint64){
            p.lastP = -1;
        } else {
            p.lastP = (int32)raw.args[0];
        }
        p.lastG = p.lastGs[p.lastP];
        p.lastTs = ((Timestamp)(int64)raw.args[1]);
    }
    else if (exprᴛ1 == EvFrequency) {
        p.ticksPerSec = (int64)raw.args[0];
        if (p.ticksPerSec <= 0) {
            // The most likely cause for this is tick skew on different CPUs.
            // For example, solaris/amd64 seems to have wildly different
            // ticks on different CPUs.
            return ErrTimeOrder;
        }
    }
    else if (exprᴛ1 == EvTimerGoroutine) {
        p.timerGoids[raw.args[0]] = true;
    }
    else if (exprᴛ1 == EvStack) {
        if (len(raw.args) < 2) {
            return fmt.Errorf("EvStack has wrong number of arguments: want at least 2, got %d"u8, len(raw.args));
        }
        var size = raw.args[1];
        if (size > 1000) {
            return fmt.Errorf("EvStack has bad number of frames: %d"u8, size);
        }
        var want = 2 + 4 * size;
        if ((uint64)len(raw.args) != want) {
            return fmt.Errorf("EvStack has wrong number of arguments: want %d, got %d"u8, want, len(raw.args));
        }
        var id = (uint32)raw.args[0];
        if (id != 0 && size > 0) {
            var stk = p.allocateStack(size);
            for (nint i = 0; i < (nint)size; i++) {
                var pc = raw.args[2 + i * 4 + 0];
                var fn = raw.args[2 + i * 4 + 1];
                var @file = raw.args[2 + i * 4 + 2];
                var line = raw.args[2 + i * 4 + 3];
                stk[i] = pc;
                {
                    var (_, ok) = p.pcs[pc, ꟷ]; if (!ok) {
                        p.pcs[pc] = new Frame(PC: pc, Fn: fn, File: @file, Line: (nint)line);
                    }
                }
            }
            p.stacks[id] = stk;
        }
    }
    else if (exprᴛ1 == EvCPUSample) {
    }
    else { /* default: */
        ev = new Event( // These events get parsed during the indexing step and don't strictly
 // belong to the batch.
Type: raw.typ, P: p.lastP, G: p.lastG);
        nint argOffset = default!;
        ev.Ts = p.lastTs + ((Timestamp)(int64)raw.args[0]);
        argOffset = 1;
        p.lastTs = ev.Ts;
        for (nint i = argOffset; i < narg; i++) {
            if (i == narg - 1 && (~desc).Stack){
                ev.StkID = (uint32)raw.args[i];
            } else {
                ev.Args[i - argOffset] = raw.args[i];
            }
        }
        var exprᴛ2 = raw.typ;
        if (exprᴛ2 == EvGoStart || exprᴛ2 == EvGoStartLocal || exprᴛ2 == EvGoStartLabel) {
            p.lastG = ev.Args[0];
            ev.G = p.lastG;
        }
        else if (exprᴛ2 == EvGoEnd || exprᴛ2 == EvGoStop || exprᴛ2 == EvGoSched || exprᴛ2 == EvGoPreempt || exprᴛ2 == EvGoSleep || exprᴛ2 == EvGoBlock || exprᴛ2 == EvGoBlockSend || exprᴛ2 == EvGoBlockRecv || exprᴛ2 == EvGoBlockSelect || exprᴛ2 == EvGoBlockSync || exprᴛ2 == EvGoBlockCond || exprᴛ2 == EvGoBlockNet || exprᴛ2 == EvGoSysBlock || exprᴛ2 == EvGoBlockGC) {
            p.lastG = 0;
        }
        else if (exprᴛ2 == EvGoSysExit || exprᴛ2 == EvGoWaiting || exprᴛ2 == EvGoInSyscall) {
            ev.G = ev.Args[0];
        }
        else if (exprᴛ2 == EvUserTaskCreate) {
        }
        else if (exprᴛ2 == EvUserRegion) {
        }
        else if (exprᴛ2 == EvUserLog) {
            {
                var (id, ok) = p.inlineStringsMapping[raw.sargs[0], ꟷ]; if (ok){
                    // e.Args 0: taskID, 1:parentID, 2:nameID
                    // e.Args 0: taskID, 1: mode, 2:nameID
                    // e.Args 0: taskID, 1:keyID, 2: stackID, 3: messageID
                    // raw.sargs 0: message
                    ev.Args[3] = (uint64)id;
                } else {
                    nint idΔ1 = len(p.inlineStrings);
                    p.inlineStringsMapping[raw.sargs[0]] = idΔ1;
                    p.inlineStrings = builtin.append(p.inlineStrings, raw.sargs[0]);
                    ev.Args[3] = (uint64)idΔ1;
                }
            }
        }

        return default!;
    }

    ev.Type = EvNone;
    return default!;
}

// ErrTimeOrder is returned by Parse when the trace contains
// time stamps that do not respect actual event ordering.
public static error ErrTimeOrder = errors.New("time stamps out of order"u8);

[GoType("dyn")] internal partial struct postProcessTrace_gdesc {
    internal nint state;
    internal ж<Event> ev;
    internal ж<Event> evStart;
    internal ж<Event> evCreate;
    internal ж<Event> evMarkAssist;
}

[GoType("dyn")] internal partial struct postProcessTrace_pdesc {
    internal bool running;
    internal uint64 g;
    internal ж<Event> evSweep;
}

// postProcessTrace does inter-event verification and information restoration.
// The resulting trace is guaranteed to be consistent
// (for example, a P does not run two Gs at the same time, or a G is indeed
// blocked before an unblock event).
internal static error postProcessTrace(this ж<parser> Ꮡp, Events events) {
    ref var p = ref Ꮡp.DerefOrNull();

    const nint gDead = iota;
    const nint gRunnable = 1;
    const nint gRunning = 2;
    const nint gWaiting = 3;
    var gs = new map<uint64, postProcessTrace_gdesc>();
    var ps = new map<int32, postProcessTrace_pdesc>();
    var tasks = new map<uint64, ж<Event>>(); // task id to task creation events
    var activeRegions = new map<uint64, slice<ж<Event>>>(); // goroutine id to stack of regions
    gs[0] = new postProcessTrace_gdesc(state: gRunning);
    ж<Event> evGC = default!;
    ref var evSTW = ref heap<ж<Event>>(out var ᏑevSTW);
    error checkRunning(postProcessTrace_pdesc pΔ1, postProcessTrace_gdesc g, ж<Event> ev, bool allowG0) {
        @string name = EventDescriptions[(~ev).Type].Name;
        if (g.state != gRunning) {
            return fmt.Errorf("g %d is not running while %s (time %d)"u8, (~ev).G, name, (~ev).Ts);
        }
        if (pΔ1.g != (~ev).G) {
            return fmt.Errorf("p %d is not running g %d while %s (time %d)"u8, (~ev).P, (~ev).G, name, (~ev).Ts);
        }
        if (!allowG0 && (~ev).G == 0) {
            return fmt.Errorf("g 0 did %s (time %d)"u8, name, (~ev).Ts);
        }
        return default!;
    }
    for (nint evIdx = 0; evIdx < events.Len(); evIdx++) {
        var ev = events.Ptr(evIdx);
        var exprᴛ1 = (~ev).Type;
        if (exprᴛ1 == EvProcStart) {
            var pΔ3 = ps[(~ev).P];
            if (pΔ3.running) {
                return fmt.Errorf("p %d is running before start (time %d)"u8, (~ev).P, (~ev).Ts);
            }
            pΔ3.running = true;
            ps[(~ev).P] = pΔ3;
        }
        else if (exprᴛ1 == EvProcStop) {
            var pΔ4 = ps[(~ev).P];
            if (!pΔ4.running) {
                return fmt.Errorf("p %d is not running before stop (time %d)"u8, (~ev).P, (~ev).Ts);
            }
            if (pΔ4.g != 0) {
                return fmt.Errorf("p %d is running a goroutine %d during stop (time %d)"u8, (~ev).P, pΔ4.g, (~ev).Ts);
            }
            pΔ4.running = false;
            ps[(~ev).P] = pΔ4;
        }
        else if (exprᴛ1 == EvGCStart) {
            if (evGC != nil) {
                return fmt.Errorf("previous GC is not ended before a new one (time %d)"u8, (~ev).Ts);
            }
            evGC = ev;
            ev.Value.P = GCP;
        }
        else if (exprᴛ1 == EvGCDone) {
            if (evGC == nil) {
                // Attribute this to the global GC state.
                return fmt.Errorf("bogus GC end (time %d)"u8, (~ev).Ts);
            }
            evGC = default!;
        }
        else if (exprᴛ1 == EvSTWStart) {
            var evp = ᏑevSTW;
            if (evp.ValueSlot != nil) {
                return fmt.Errorf("previous STW is not ended before a new one (time %d)"u8, (~ev).Ts);
            }
            evp.ValueSlot = ev;
        }
        else if (exprᴛ1 == EvSTWDone) {
            var evp = ᏑevSTW;
            if (evp.ValueSlot == nil) {
                return fmt.Errorf("bogus STW end (time %d)"u8, (~ev).Ts);
            }
            evp.ValueSlot = default!;
        }
        else if (exprᴛ1 == EvGCSweepStart) {
            var pΔ5 = ps[(~ev).P];
            if (pΔ5.evSweep != nil) {
                return fmt.Errorf("previous sweeping is not ended before a new one (time %d)"u8, (~ev).Ts);
            }
            pΔ5.evSweep = ev;
            ps[(~ev).P] = pΔ5;
        }
        else if (exprᴛ1 == EvGCMarkAssistStart) {
            var g = gs[(~ev).G];
            if (g.evMarkAssist != nil) {
                return fmt.Errorf("previous mark assist is not ended before a new one (time %d)"u8, (~ev).Ts);
            }
            g.evMarkAssist = ev;
            gs[(~ev).G] = g;
        }
        else if (exprᴛ1 == EvGCMarkAssistDone) {
            var g = gs[(~ev).G];
            if (g.evMarkAssist != nil) {
                // Unlike most events, mark assists can be in progress when a
                // goroutine starts tracing, so we can't report an error here.
                g.evMarkAssist = default!;
            }
            gs[(~ev).G] = g;
        }
        else if (exprᴛ1 == EvGCSweepDone) {
            var pΔ6 = ps[(~ev).P];
            if (pΔ6.evSweep == nil) {
                return fmt.Errorf("bogus sweeping end (time %d)"u8, (~ev).Ts);
            }
            pΔ6.evSweep = default!;
            ps[(~ev).P] = pΔ6;
        }
        else if (exprᴛ1 == EvGoWaiting) {
            var g = gs[(~ev).G];
            if (g.state != gRunnable) {
                return fmt.Errorf("g %d is not runnable before EvGoWaiting (time %d)"u8, (~ev).G, (~ev).Ts);
            }
            g.state = gWaiting;
            g.ev = ev;
            gs[(~ev).G] = g;
        }
        else if (exprᴛ1 == EvGoInSyscall) {
            var g = gs[(~ev).G];
            if (g.state != gRunnable) {
                return fmt.Errorf("g %d is not runnable before EvGoInSyscall (time %d)"u8, (~ev).G, (~ev).Ts);
            }
            g.state = gWaiting;
            g.ev = ev;
            gs[(~ev).G] = g;
        }
        else if (exprᴛ1 == EvGoCreate) {
            var g = gs[(~ev).G];
            var pΔ7 = ps[(~ev).P];
            {
                var err = checkRunning(pΔ7, g, ev, true); if (err != default!) {
                    return err;
                }
            }
            {
                var (_, ok) = gs[(~ev).Args[0], ꟷ]; if (ok) {
                    return fmt.Errorf("g %d already exists (time %d)"u8, (~ev).Args[0], (~ev).Ts);
                }
            }
            gs[(~ev).Args[0]] = new postProcessTrace_gdesc(state: gRunnable, ev: ev, evCreate: ev);
        }
        else if (exprᴛ1 == EvGoStart || exprᴛ1 == EvGoStartLabel) {
            var g = gs[(~ev).G];
            var pΔ8 = ps[(~ev).P];
            if (g.state != gRunnable) {
                return fmt.Errorf("g %d is not runnable before start (time %d)"u8, (~ev).G, (~ev).Ts);
            }
            if (pΔ8.g != 0) {
                return fmt.Errorf("p %d is already running g %d while start g %d (time %d)"u8, (~ev).P, pΔ8.g, (~ev).G, (~ev).Ts);
            }
            g.state = gRunning;
            g.evStart = ev;
            pΔ8.g = ev.Value.G;
            if (g.evCreate != nil) {
                ev.Value.StkID = (uint32)(~g.evCreate).Args[1];
                g.evCreate = default!;
            }
            if (g.ev != nil) {
                g.ev = default!;
            }
            gs[(~ev).G] = g;
            ps[(~ev).P] = pΔ8;
        }
        else if (exprᴛ1 == EvGoEnd || exprᴛ1 == EvGoStop) {
            var g = gs[(~ev).G];
            var pΔ9 = ps[(~ev).P];
            {
                var err = checkRunning(pΔ9, g, ev, false); if (err != default!) {
                    return err;
                }
            }
            g.evStart = default!;
            g.state = gDead;
            pΔ9.g = 0;
            if ((~ev).Type == EvGoEnd) {
                // flush all active regions
                delete(activeRegions, (~ev).G);
            }
            gs[(~ev).G] = g;
            ps[(~ev).P] = pΔ9;
        }
        else if (exprᴛ1 == EvGoSched || exprᴛ1 == EvGoPreempt) {
            var g = gs[(~ev).G];
            var pΔ10 = ps[(~ev).P];
            {
                var err = checkRunning(pΔ10, g, ev, false); if (err != default!) {
                    return err;
                }
            }
            g.state = gRunnable;
            g.evStart = default!;
            pΔ10.g = 0;
            g.ev = ev;
            gs[(~ev).G] = g;
            ps[(~ev).P] = pΔ10;
        }
        else if (exprᴛ1 == EvGoUnblock) {
            var g = gs[(~ev).G];
            var pΔ11 = ps[(~ev).P];
            if (g.state != gRunning) {
                return fmt.Errorf("g %d is not running while unpark (time %d)"u8, (~ev).G, (~ev).Ts);
            }
            if ((~ev).P != TimerP && pΔ11.g != (~ev).G) {
                return fmt.Errorf("p %d is not running g %d while unpark (time %d)"u8, (~ev).P, (~ev).G, (~ev).Ts);
            }
            var g1 = gs[(~ev).Args[0]];
            if (g1.state != gWaiting) {
                return fmt.Errorf("g %d is not waiting before unpark (time %d)"u8, (~ev).Args[0], (~ev).Ts);
            }
            if (g1.ev != nil && (~g1.ev).Type == EvGoBlockNet) {
                ev.Value.P = NetpollP;
            }
            g1.state = gRunnable;
            g1.ev = ev;
            gs[(~ev).Args[0]] = g1;
        }
        else if (exprᴛ1 == EvGoSysCall) {
            var g = gs[(~ev).G];
            var pΔ12 = ps[(~ev).P];
            {
                var err = checkRunning(pΔ12, g, ev, false); if (err != default!) {
                    return err;
                }
            }
            g.ev = ev;
            gs[(~ev).G] = g;
        }
        else if (exprᴛ1 == EvGoSysBlock) {
            var g = gs[(~ev).G];
            var pΔ13 = ps[(~ev).P];
            {
                var err = checkRunning(pΔ13, g, ev, false); if (err != default!) {
                    return err;
                }
            }
            g.state = gWaiting;
            g.evStart = default!;
            pΔ13.g = 0;
            gs[(~ev).G] = g;
            ps[(~ev).P] = pΔ13;
        }
        else if (exprᴛ1 == EvGoSysExit) {
            var g = gs[(~ev).G];
            if (g.state != gWaiting) {
                return fmt.Errorf("g %d is not waiting during syscall exit (time %d)"u8, (~ev).G, (~ev).Ts);
            }
            g.state = gRunnable;
            g.ev = ev;
            gs[(~ev).G] = g;
        }
        else if (exprᴛ1 == EvGoSleep || exprᴛ1 == EvGoBlock || exprᴛ1 == EvGoBlockSend || exprᴛ1 == EvGoBlockRecv || exprᴛ1 == EvGoBlockSelect || exprᴛ1 == EvGoBlockSync || exprᴛ1 == EvGoBlockCond || exprᴛ1 == EvGoBlockNet || exprᴛ1 == EvGoBlockGC) {
            var g = gs[(~ev).G];
            var pΔ14 = ps[(~ev).P];
            {
                var err = checkRunning(pΔ14, g, ev, false); if (err != default!) {
                    return err;
                }
            }
            g.state = gWaiting;
            g.ev = ev;
            g.evStart = default!;
            pΔ14.g = 0;
            gs[(~ev).G] = g;
            ps[(~ev).P] = pΔ14;
        }
        else if (exprᴛ1 == EvUserTaskCreate) {
            var taskid = (~ev).Args[0];
            {
                var (prevEv, ok) = tasks[taskid, ꟷ]; if (ok) {
                    return fmt.Errorf("task id conflicts (id:%d), %q vs %q"u8, taskid, ev.OrTypedNil(), prevEv.OrTypedNil());
                }
            }
            tasks[(~ev).Args[0]] = ev;
        }
        else if (exprᴛ1 == EvUserTaskEnd) {
            var taskid = (~ev).Args[0];
            delete(tasks, taskid);
        }
        else if (exprᴛ1 == EvUserRegion) {
            var mode = (~ev).Args[1];
            var regions = activeRegions[(~ev).G];
            if (mode == 0){
                // region start
                activeRegions[(~ev).G] = builtin.append(regions, ev); // push
            } else 
            if (mode == 1){
                // region end
                nint n = len(regions);
                if (n > 0) {
                    // matching region start event is in the trace.
                    var s = regions[n - 1];
                    if ((~s).Args[0] != (~ev).Args[0] || (~s).Args[2] != (~ev).Args[2]) {
                        // task id, region name mismatch
                        return fmt.Errorf("misuse of region in goroutine %d: span end %q when the inner-most active span start event is %q"u8, (~ev).G, ev.OrTypedNil(), s.OrTypedNil());
                    }
                    if (n > 1){
                        activeRegions[(~ev).G] = regions.slice(0, n - 1);
                    } else {
                        delete(activeRegions, (~ev).G);
                    }
                }
            } else {
                return fmt.Errorf("invalid user region mode: %q"u8, ev.OrTypedNil());
            }
        }

        if ((~ev).StkID != 0 && len(p.stacks[(~ev).StkID]) == 0) {
            // Make sure events don't refer to stacks that don't exist or to
            // stacks with zero frames. Neither of these should be possible, but
            // better be safe than sorry.
            ev.Value.StkID = 0;
        }
    }
    // TODO(mknyszek): restore stacks for EvGoStart events.
    return default!;
}

internal static error errMalformedVarint = errors.New("malformatted base-128 varint"u8);

// readVal reads unsigned base-128 value from r.
[GoRecv] internal static (uint64, error) readVal(this ref parser p) {
    var (v, n) = binary.Uvarint(p.data.slice(p.off));
    if (n <= 0) {
        return (0, errMalformedVarint);
    }
    p.off += n;
    return (v, default!);
}

internal static (uint64 v, slice<byte> rem, error err) readValFrom(slice<byte> buf) {
    uint64 v = default!;

    (v, var n) = binary.Uvarint(buf);
    if (n <= 0) {
        return (0, default!, errMalformedVarint);
    }
    return (v, buf.slice(n), default!);
}

[GoRecv] public static @string String(this ref Event ev) {
    var desc = ᏑEventDescriptions.at<EventDescriptionsᴛ1>((nint)(ev.Type));
    var w = @new<bytes.Buffer>();
    fmt.Fprintf(new bytes_BufferжWriter(w), "%d %s p=%d g=%d stk=%d"u8, ev.Ts, (~desc).Name, ev.P, ev.G, ev.StkID);
    foreach (var (i, a) in (~desc).Args) {
        fmt.Fprintf(new bytes_BufferжWriter(w), " %s=%d"u8, a, ev.Args[i]);
    }
    return w.String();
}

// argNum returns total number of args for the event accounting for timestamps,
// sequence numbers and differences between trace format versions.
[GoRecv] internal static nint argNum(this ref rawEvent raw) {
    var desc = ᏑEventDescriptions.at<EventDescriptionsᴛ1>((nint)(raw.typ));
    if (raw.typ == EvStack) {
        return len(raw.args);
    }
    nint narg = len((~desc).Args);
    if ((~desc).Stack) {
        narg++;
    }
    var exprᴛ1 = raw.typ;
    if (exprᴛ1 == EvBatch || exprᴛ1 == EvFrequency || exprᴛ1 == EvTimerGoroutine) {
        return narg;
    }

    narg++; // timestamp
    return narg;
}

// Event types in the trace.
// Verbatim copy from src/runtime/trace.go with the "trace" prefix removed.
public static @event.Type EvNone => 0;            // unused

public static @event.Type EvBatch => 1;           // start of per-P batch of events [pid, timestamp]

public static @event.Type EvFrequency => 2;       // contains tracer timer frequency [frequency (ticks per second)]

public static @event.Type EvStack => 3;           // stack [stack id, number of PCs, array of {PC, func string ID, file string ID, line}]

public static @event.Type EvGomaxprocs => 4;      // current value of GOMAXPROCS [timestamp, GOMAXPROCS, stack id]

public static @event.Type EvProcStart => 5;       // start of P [timestamp, thread id]

public static @event.Type EvProcStop => 6;        // stop of P [timestamp]

public static @event.Type EvGCStart => 7;         // GC start [timestamp, seq, stack id]

public static @event.Type EvGCDone => 8;          // GC done [timestamp]

public static @event.Type EvSTWStart => 9;        // GC mark termination start [timestamp, kind]

public static @event.Type EvSTWDone => 10;         // GC mark termination done [timestamp]

public static @event.Type EvGCSweepStart => 11;    // GC sweep start [timestamp, stack id]

public static @event.Type EvGCSweepDone => 12;     // GC sweep done [timestamp, swept, reclaimed]

public static @event.Type EvGoCreate => 13;        // goroutine creation [timestamp, new goroutine id, new stack id, stack id]

public static @event.Type EvGoStart => 14;         // goroutine starts running [timestamp, goroutine id, seq]

public static @event.Type EvGoEnd => 15;           // goroutine ends [timestamp]

public static @event.Type EvGoStop => 16;          // goroutine stops (like in select{}) [timestamp, stack]

public static @event.Type EvGoSched => 17;         // goroutine calls Gosched [timestamp, stack]

public static @event.Type EvGoPreempt => 18;       // goroutine is preempted [timestamp, stack]

public static @event.Type EvGoSleep => 19;         // goroutine calls Sleep [timestamp, stack]

public static @event.Type EvGoBlock => 20;         // goroutine blocks [timestamp, stack]

public static @event.Type EvGoUnblock => 21;       // goroutine is unblocked [timestamp, goroutine id, seq, stack]

public static @event.Type EvGoBlockSend => 22;     // goroutine blocks on chan send [timestamp, stack]

public static @event.Type EvGoBlockRecv => 23;     // goroutine blocks on chan recv [timestamp, stack]

public static @event.Type EvGoBlockSelect => 24;   // goroutine blocks on select [timestamp, stack]

public static @event.Type EvGoBlockSync => 25;     // goroutine blocks on Mutex/RWMutex [timestamp, stack]

public static @event.Type EvGoBlockCond => 26;     // goroutine blocks on Cond [timestamp, stack]

public static @event.Type EvGoBlockNet => 27;      // goroutine blocks on network [timestamp, stack]

public static @event.Type EvGoSysCall => 28;       // syscall enter [timestamp, stack]

public static @event.Type EvGoSysExit => 29;       // syscall exit [timestamp, goroutine id, seq, real timestamp]

public static @event.Type EvGoSysBlock => 30;      // syscall blocks [timestamp]

public static @event.Type EvGoWaiting => 31;       // denotes that goroutine is blocked when tracing starts [timestamp, goroutine id]

public static @event.Type EvGoInSyscall => 32;     // denotes that goroutine is in syscall when tracing starts [timestamp, goroutine id]

public static @event.Type EvHeapAlloc => 33;       // gcController.heapLive change [timestamp, heap live bytes]

public static @event.Type EvHeapGoal => 34;        // gcController.heapGoal change [timestamp, heap goal bytes]

public static @event.Type EvTimerGoroutine => 35;  // denotes timer goroutine [timer goroutine id]

public static @event.Type EvFutileWakeup => 36;    // denotes that the previous wakeup of this goroutine was futile [timestamp]

public static @event.Type EvString => 37;          // string dictionary entry [ID, length, string]

public static @event.Type EvGoStartLocal => 38;    // goroutine starts running on the same P as the last event [timestamp, goroutine id]

public static @event.Type EvGoUnblockLocal => 39;  // goroutine is unblocked on the same P as the last event [timestamp, goroutine id, stack]

public static @event.Type EvGoSysExitLocal => 40;  // syscall exit on the same P as the last event [timestamp, goroutine id, real timestamp]

public static @event.Type EvGoStartLabel => 41;    // goroutine starts running with label [timestamp, goroutine id, seq, label string id]

public static @event.Type EvGoBlockGC => 42;       // goroutine blocks on GC assist [timestamp, stack]

public static @event.Type EvGCMarkAssistStart => 43; // GC mark assist start [timestamp, stack]

public static @event.Type EvGCMarkAssistDone => 44; // GC mark assist done [timestamp]

public static @event.Type EvUserTaskCreate => 45;  // trace.NewTask [timestamp, internal task id, internal parent id, stack, name string]

public static @event.Type EvUserTaskEnd => 46;     // end of task [timestamp, internal task id, stack]

public static @event.Type EvUserRegion => 47;      // trace.WithRegion [timestamp, internal task id, mode(0:start, 1:end), name string]

public static @event.Type EvUserLog => 48;         // trace.Log [timestamp, internal id, key string id, stack, value string]

public static @event.Type EvCPUSample => 49;       // CPU profiling sample [timestamp, stack, real timestamp, real P id (-1 when absent), goroutine id]

public static @event.Type EvCount => 50;

// in 1.5 format it was {"p", "seq", "ticks"}
// in 1.5 format it was {"freq", "unused"}
// in 1.5 format it was {}
// <= 1.9, args was {} (implicitly {0})
// before 1.9, format was {}
// in 1.5 format it was {"g"}
// in 1.5 format it was {"g"}
// in 1.5 format it was {"g", "unused"}

[GoType("dyn")] partial struct EventDescriptionsᴛ1 {
    public @string Name;
    internal version.Version minVersion;
    public bool Stack;
    public slice<@string> Args;
    public slice<@string> SArgs; // string arguments
}
public static ж<array<EventDescriptionsᴛ1>> ᏑEventDescriptions = new StandardBox<array<EventDescriptionsᴛ1>>(new golib.SparseArray<EventDescriptionsᴛ1>{
    [EvNone] = new("None"u8, 5, false, new @string[]{}.slice(), default!),
    [EvBatch] = new("Batch"u8, 5, false, new @string[]{"p"u8, "ticks"u8}.slice(), default!),
    [EvFrequency] = new("Frequency"u8, 5, false, new @string[]{"freq"u8}.slice(), default!),
    [EvStack] = new("Stack"u8, 5, false, new @string[]{"id"u8, "siz"u8}.slice(), default!),
    [EvGomaxprocs] = new("Gomaxprocs"u8, 5, true, new @string[]{"procs"u8}.slice(), default!),
    [EvProcStart] = new("ProcStart"u8, 5, false, new @string[]{"thread"u8}.slice(), default!),
    [EvProcStop] = new("ProcStop"u8, 5, false, new @string[]{}.slice(), default!),
    [EvGCStart] = new("GCStart"u8, 5, true, new @string[]{"seq"u8}.slice(), default!),
    [EvGCDone] = new("GCDone"u8, 5, false, new @string[]{}.slice(), default!),
    [EvSTWStart] = new("GCSTWStart"u8, 5, false, new @string[]{"kindid"u8}.slice(), new @string[]{"kind"u8}.slice()),
    [EvSTWDone] = new("GCSTWDone"u8, 5, false, new @string[]{}.slice(), default!),
    [EvGCSweepStart] = new("GCSweepStart"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGCSweepDone] = new("GCSweepDone"u8, 5, false, new @string[]{"swept"u8, "reclaimed"u8}.slice(), default!),
    [EvGoCreate] = new("GoCreate"u8, 5, true, new @string[]{"g"u8, "stack"u8}.slice(), default!),
    [EvGoStart] = new("GoStart"u8, 5, false, new @string[]{"g"u8, "seq"u8}.slice(), default!),
    [EvGoEnd] = new("GoEnd"u8, 5, false, new @string[]{}.slice(), default!),
    [EvGoStop] = new("GoStop"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoSched] = new("GoSched"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoPreempt] = new("GoPreempt"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoSleep] = new("GoSleep"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoBlock] = new("GoBlock"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoUnblock] = new("GoUnblock"u8, 5, true, new @string[]{"g"u8, "seq"u8}.slice(), default!),
    [EvGoBlockSend] = new("GoBlockSend"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoBlockRecv] = new("GoBlockRecv"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoBlockSelect] = new("GoBlockSelect"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoBlockSync] = new("GoBlockSync"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoBlockCond] = new("GoBlockCond"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoBlockNet] = new("GoBlockNet"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoSysCall] = new("GoSysCall"u8, 5, true, new @string[]{}.slice(), default!),
    [EvGoSysExit] = new("GoSysExit"u8, 5, false, new @string[]{"g"u8, "seq"u8, "ts"u8}.slice(), default!),
    [EvGoSysBlock] = new("GoSysBlock"u8, 5, false, new @string[]{}.slice(), default!),
    [EvGoWaiting] = new("GoWaiting"u8, 5, false, new @string[]{"g"u8}.slice(), default!),
    [EvGoInSyscall] = new("GoInSyscall"u8, 5, false, new @string[]{"g"u8}.slice(), default!),
    [EvHeapAlloc] = new("HeapAlloc"u8, 5, false, new @string[]{"mem"u8}.slice(), default!),
    [EvHeapGoal] = new("HeapGoal"u8, 5, false, new @string[]{"mem"u8}.slice(), default!),
    [EvTimerGoroutine] = new("TimerGoroutine"u8, 5, false, new @string[]{"g"u8}.slice(), default!),
    [EvFutileWakeup] = new("FutileWakeup"u8, 5, false, new @string[]{}.slice(), default!),
    [EvString] = new("String"u8, 7, false, new @string[]{}.slice(), default!),
    [EvGoStartLocal] = new("GoStartLocal"u8, 7, false, new @string[]{"g"u8}.slice(), default!),
    [EvGoUnblockLocal] = new("GoUnblockLocal"u8, 7, true, new @string[]{"g"u8}.slice(), default!),
    [EvGoSysExitLocal] = new("GoSysExitLocal"u8, 7, false, new @string[]{"g"u8, "ts"u8}.slice(), default!),
    [EvGoStartLabel] = new("GoStartLabel"u8, 8, false, new @string[]{"g"u8, "seq"u8, "labelid"u8}.slice(), new @string[]{"label"u8}.slice()),
    [EvGoBlockGC] = new("GoBlockGC"u8, 8, true, new @string[]{}.slice(), default!),
    [EvGCMarkAssistStart] = new("GCMarkAssistStart"u8, 9, true, new @string[]{}.slice(), default!),
    [EvGCMarkAssistDone] = new("GCMarkAssistDone"u8, 9, false, new @string[]{}.slice(), default!),
    [EvUserTaskCreate] = new("UserTaskCreate"u8, 11, true, new @string[]{"taskid"u8, "pid"u8, "typeid"u8}.slice(), new @string[]{"name"u8}.slice()),
    [EvUserTaskEnd] = new("UserTaskEnd"u8, 11, true, new @string[]{"taskid"u8}.slice(), default!),
    [EvUserRegion] = new("UserRegion"u8, 11, true, new @string[]{"taskid"u8, "mode"u8, "typeid"u8}.slice(), new @string[]{"name"u8}.slice()),
    [EvUserLog] = new("UserLog"u8, 11, true, new @string[]{"id"u8, "keyid"u8}.slice(), new @string[]{"category"u8, "message"u8}.slice()),
    [EvCPUSample] = new("CPUSample"u8, 19, true, new @string[]{"ts"u8, "p"u8, "g"u8}.slice(), default!)
}.array(256));
public static ref array<EventDescriptionsᴛ1> EventDescriptions => ref ᏑEventDescriptions.Value;

//gcassert:inline
[GoRecv] internal static slice<uint64> allocateStack(this ref parser p, uint64 size) {
    if (size == 0) {
        return default!;
    }
    // Stacks are plentiful but small. For our "Staticcheck on std" trace with
    // 11e6 events, we have roughly 500,000 stacks, using 200 MiB of memory. To
    // avoid making 500,000 small allocations we allocate backing arrays 1 MiB
    // at a time.
    var @out = p.stacksData;
    if ((uint64)len(@out) < size) {
        @out = new slice<uint64>(1024 * 128);
    }
    p.stacksData = @out.slice((nint)(size));
    return @out.slice(0, (nint)(size), (nint)(size));
}

[GoRecv] public static ΔSTWReason STWReason(this ref Trace tr, uint64 kindID) {
    if (tr.Version < 21){
        if (kindID == 0 || kindID == 1){
            return ((ΔSTWReason)(nint)(kindID + 1));
        } else {
            return STWUnknown;
        }
    } else 
    if (tr.Version == 21){
        if (kindID < NumSTWReasons){
            return ((ΔSTWReason)(nint)kindID);
        } else {
            return STWUnknown;
        }
    } else {
        return STWUnknown;
    }
}

[GoType("num:nint")] partial struct ΔSTWReason;

public static ΔSTWReason STWUnknown => 0;
public static ΔSTWReason STWGCMarkTermination => 1;
public static ΔSTWReason STWGCSweepTermination => 2;
public static ΔSTWReason STWWriteHeapDump => 3;
public static ΔSTWReason STWGoroutineProfile => 4;
public static ΔSTWReason STWGoroutineProfileCleanup => 5;
public static ΔSTWReason STWAllGoroutinesStackTrace => 6;
public static ΔSTWReason STWReadMemStats => 7;
public static ΔSTWReason STWAllThreadsSyscall => 8;
public static ΔSTWReason STWGOMAXPROCS => 9;
public static ΔSTWReason STWStartTrace => 10;
public static ΔSTWReason STWStopTrace => 11;
public static ΔSTWReason STWCountPagesInUse => 12;
public static ΔSTWReason STWReadMetricsSlow => 13;
public static ΔSTWReason STWReadMemStatsSlow => 14;
public static ΔSTWReason STWPageCachePagesLeaked => 15;
public static ΔSTWReason STWResetDebugLog => 16;
public static UntypedInt NumSTWReasons => 17;

} // end oldtrace_package
