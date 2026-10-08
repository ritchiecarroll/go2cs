// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

using json = encoding.json_package;
using fmt = fmt_package;
using trace = go.@internal.trace_package;
using format = go.@internal.trace.traceviewer.format_package;
using io = io_package;
using strconv = strconv_package;
using time = time_package;
using encoding;
using go.@internal;
using go.@internal.trace.traceviewer;

partial class traceviewer_package {

partial struct TraceConsumer {
    public Action<@string> ConsumeTimeUnit;
    public Action<ж<format.Event>, bool> ConsumeViewerEvent;
    public Action<@string, format.Frame> ConsumeViewerFrame;
    public Action Flush;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string displayTimeUnitˢ = @"""displayTimeUnit"":"u8;
private static readonly @string traceEventsˢ = @"""traceEvents"": ["u8;
private static readonly @string stackFramesˢ = @"], ""stackFrames"":"u8;

// ViewerDataTraceConsumer returns a TraceConsumer that writes to w. The
// startIdx and endIdx are used for splitting large traces. They refer to
// indexes in the traceEvents output array, not the events in the trace input.
public static TraceConsumer ViewerDataTraceConsumer(io.Writer w, int64 startIdx, int64 endIdx) {
    var allFrames = new map<@string, format.Frame>();
    var requiredFrames = new map<@string, format.Frame>();
    var enc = json.NewEncoder(w);
    nint written = 0;
    var index = (int64)(-1);
    io.WriteString(w, "{"u8);
        var encʗ1 = enc;

        var allFramesʗ1 = allFrames;
        var encʗ2 = enc;
        var requiredFramesʗ1 = requiredFrames;

        var allFramesʗ4 = allFrames;

        var encʗ3 = enc;
        var requiredFramesʗ4 = requiredFrames;
    return new TraceConsumer(
        ConsumeTimeUnit: (@string unit) => {
            io.WriteString(w, displayTimeUnitˢ);
            encʗ1.Encode(unit);
            io.WriteString(w, ","u8);
        },
        ConsumeViewerEvent: (ж<format.Event> v, bool @required) => {
            index++;
            if (!@required && (index < startIdx || index > endIdx)) {
                // not in the range. Skip!
                return;
            }
            var allFramesʗ2 = allFramesʗ1;
            var requiredFramesʗ2 = requiredFramesʗ1;
            WalkStackFrames(allFramesʗ1, (~v).Stack, (nint id) => {
                @string s = strconv.Itoa(id);
                requiredFramesʗ2[s] = allFramesʗ2[s];
            });
            var allFramesʗ3 = allFramesʗ1;
            var requiredFramesʗ3 = requiredFramesʗ1;
            WalkStackFrames(allFramesʗ1, (~v).EndStack, (nint id) => {
                @string s = strconv.Itoa(id);
                requiredFramesʗ3[s] = allFramesʗ3[s];
            });
            if (written == 0) {
                io.WriteString(w, traceEventsˢ);
            }
            if (written > 0) {
                io.WriteString(w, ","u8);
            }
            encʗ2.Encode(v.OrTypedNil());
            // TODO(mknyszek): get rid of the extra \n inserted by enc.Encode.
            // Same should be applied to splittingTraceConsumer.
            written++;
        },
        ConsumeViewerFrame: (@string k, format.Frame v) => {
            allFramesʗ4[k] = v;
        },
        Flush: () => {
            io.WriteString(w, stackFramesˢ);
            encʗ3.Encode(requiredFramesʗ4);
            io.WriteString(w, @"}"u8);
        }
    );
}

internal partial struct SplittingTraceConsumer_eventSz /*dyn*/ {
    public float64 Time;
    public nint Sz;
    public slice<nint> Frames;
}

public static (ж<splitter>, TraceConsumer) SplittingTraceConsumer(nint max) {
    ref var data = ref heap(new format.Data(), out var Ꮡdata);

    data = new format.Data(Frames: new map<@string, format.Frame>());
    map<@string, format.Frame> allFrames = new map<@string, format.Frame>();
    ref var sizes = ref heap<slice<SplittingTraceConsumer_eventSz>>(out var Ꮡsizes);
    ref var cw = ref heap(new countingWriter(), out var Ꮡcw);
    var s = @new<splitter>();
        var allFramesʗ1 = allFrames;

        var allFramesʗ4 = allFrames;

        var allFramesʗ5 = allFrames;
        var sʗ1 = s;
    return (s, new TraceConsumer(
        ConsumeTimeUnit: (@string unit) => {
            Ꮡdata.Value.TimeUnit = unit;
        },
        ConsumeViewerEvent: (ж<format.Event> v, bool @required) => {
            if (@required) {
                // Store required events inside data so flush
                // can include them in the required part of the
                // trace.
                Ꮡdata.Value.Events = append(Ꮡdata.Value.Events, v);
                var allFramesʗ2 = allFramesʗ1;
                WalkStackFrames(allFramesʗ1, (~v).Stack, (nint id) => {
                    @string sΔ1 = strconv.Itoa(id);
                    Ꮡdata.Value.Frames[sΔ1] = allFramesʗ2[sΔ1];
                });
                var allFramesʗ3 = allFramesʗ1;
                WalkStackFrames(allFramesʗ1, (~v).EndStack, (nint id) => {
                    @string sΔ2 = strconv.Itoa(id);
                    Ꮡdata.Value.Frames[sΔ2] = allFramesʗ3[sΔ2];
                });
                return;
            }
            var enc = json.NewEncoder(new countingWriterжWriter(Ꮡcw));
            enc.Encode(v.OrTypedNil());
            ref var size = ref heap<SplittingTraceConsumer_eventSz>(out var Ꮡsize);
            Ꮡsize.Value = new SplittingTraceConsumer_eventSz(Time: (~v).Time, Sz: Ꮡcw.Value.size + 1); // +1 for ",".
            // Add referenced stack frames. Their size is computed
            // in flush, where we can dedup across events.
            WalkStackFrames(allFramesʗ1, (~v).Stack, (nint id) => {
                Ꮡsize.Value.Frames = append(Ꮡsize.Value.Frames, id);
            });
            WalkStackFrames(allFramesʗ1, (~v).EndStack, (nint id) => {
                Ꮡsize.Value.Frames = append(Ꮡsize.Value.Frames, id); // This may add duplicates. We'll dedup later.
            });
            Ꮡsizes.ValueSlot = append(Ꮡsizes.ValueSlot, Ꮡsize.Value);
            Ꮡcw.Value.size = 0;
        },
        ConsumeViewerFrame: (@string k, format.Frame v) => {
            allFramesʗ4[k] = v;
        },
        Flush: () => {
            // Calculate size of the mandatory part of the trace.
            // This includes thread names and stack frames for
            // required events.
            Ꮡcw.Value.size = 0;
            var enc = json.NewEncoder(new countingWriterжWriter(Ꮡcw));
            enc.Encode(Ꮡdata.Value);
            nint requiredSize = Ꮡcw.Value.size;
            // Then calculate size of each individual event and
            // their stack frames, grouping them into ranges. We
            // only include stack frames relevant to the events in
            // the range to reduce overhead.
            nint start = 0;
            nint eventsSize = 0;
            map<@string, format.Frame> frames = new map<@string, format.Frame>();
            nint framesSize = 0;
            foreach (var (i, ev) in Ꮡsizes.ValueSlot) {
                eventsSize += ev.Sz;
                // Add required stack frames. Note that they
                // may already be in the map.
                foreach (var (_, id) in ev.Frames) {
                    @string sΔ3 = strconv.Itoa(id);
                    var (_, ok) = frames[sΔ3, ꟷ];
                    if (ok) {
                        continue;
                    }
                    var f = allFramesʗ5[sΔ3];
                    frames[sΔ3] = f;
                    framesSize += stackFrameEncodedSize((nuint)id, f);
                }
                nint total = requiredSize + framesSize + eventsSize;
                if (total < max) {
                    continue;
                }
                // Reached max size, commit this range and
                // start a new range.
                var startTime = ((time.Duration)(int64)(Ꮡsizes.ValueSlot[start].Time * 1000D));
                var endTime = ((time.Duration)(int64)(ev.Time * 1000D));
                sʗ1.Value.Ranges = append((~sʗ1).Ranges, new Range(
                    Name: fmt.Sprintf("%v-%v"u8, startTime, endTime),
                    Start: start,
                    End: i + 1,
                    StartTime: (int64)startTime,
                    EndTime: (int64)endTime
                ));
                start = i + 1;
                frames = new map<@string, format.Frame>();
                framesSize = 0;
                eventsSize = 0;
            }
            if (len((~sʗ1).Ranges) <= 1) {
                sʗ1.Value.Ranges = default!;
                return;
            }
            {
                nint end = len(Ꮡsizes.ValueSlot) - 1; if (start < end) {
                    sʗ1.Value.Ranges = append((~sʗ1).Ranges, new Range(
                        Name: fmt.Sprintf("%v-%v"u8, ((time.Duration)(int64)(Ꮡsizes.ValueSlot[start].Time * 1000D)), ((time.Duration)(int64)(Ꮡsizes.ValueSlot[end].Time * 1000D))),
                        Start: start,
                        End: end,
                        StartTime: (int64)(Ꮡsizes.ValueSlot[start].Time * 1000D),
                        EndTime: (int64)(Ꮡsizes.ValueSlot[end].Time * 1000D)
                    ));
                }
            }
        }
    ));
}

public partial struct splitter {
    public slice<Range> Ranges;
}

partial struct countingWriter {
    internal nint size;
}

internal static (nint, error) Write(this ref countingWriter cw, slice<byte> data) {
    cw.size += len(data);
    return (len(data), default!);
}

internal static nint stackFrameEncodedSize(nuint id, format.Frame f) {
    // We want to know the marginal size of traceviewer.Data.Frames for
    // each event. Running full JSON encoding of the map for each event is
    // far too slow.
    //
    // Since the format is fixed, we can easily compute the size without
    // encoding.
    //
    // A single entry looks like one of the following:
    //
    //   "1":{"name":"main.main:30"},
    //   "10":{"name":"pkg.NewSession:173","parent":9},
    //
    // The parent is omitted if 0. The trailing comma is omitted from the
    // last entry, but we don't need that much precision.
    const nint baseSize = /* len(`"`) + len(`":{"name":"`) + len(`"},`) */ 15;
    
    const nint parentBaseSize = /* len(`,"parent":`) */ 10;
    nint size = baseSize;
    size += len(f.Name);
    // Bytes for id (always positive).
    while (id > 0) {
        size += 1;
        id /= 10;
    }
    if (f.Parent > 0) {
        size += parentBaseSize;
        // Bytes for parent (always positive).
        while (f.Parent > 0) {
            size += 1;
            f.Parent /= 10;
        }
    }
    return size;
}

// WalkStackFrames calls fn for id and all of its parent frames from allFrames.
public static void WalkStackFrames(map<@string, format.Frame> allFrames, nint id, Action<nint> fn) {
    while (id != 0) {
        var (f, ok) = allFrames[strconv.Itoa(id), ꟷ];
        if (!ok) {
            break;
        }
        fn(id);
        id = f.Parent;
    }
}

partial struct Mode /*num:nint*/;

public static Mode ModeGoroutineOriented => /* 1 << iota */ 1;
public static Mode ModeTaskOriented => 2;
public static Mode ModeThreadOriented => 4; // Mutually exclusive with ModeGoroutineOriented.

// NewEmitter returns a new Emitter that writes to c. The rangeStart and
// rangeEnd args are used for splitting large traces.
public static ж<Emitter> NewEmitter(TraceConsumer c, time.Duration rangeStart, time.Duration rangeEnd) {
    c.ConsumeTimeUnit("ns"u8);
    return Ꮡ(new Emitter(
        c: c,
        rangeStart: rangeStart,
        rangeEnd: rangeEnd,
        frameTree: new frameNode(children: new map<uint64, frameNode>()),
        resources: new map<uint64, @string>(),
        tasks: new map<uint64, task>()
    ));
}

partial struct Emitter {
    internal TraceConsumer c;
    internal time.Duration rangeStart;
    internal time.Duration rangeEnd;
    internal heapStats heapStats, prevHeapStats;
    internal array<int64> gstates = new(gStateCount);
    internal array<int64> prevGstates = new(gStateCount);
    internal array<int64> threadStats = new(threadStateCount);
    internal array<int64> prevThreadStats = new(threadStateCount);
    internal uint64 gomaxprocs;
    internal frameNode frameTree;
    internal nint frameSeq;
    internal uint64 arrowSeq;
    internal Func<uint64, bool> filter;
    internal @string resourceType;
    internal map<uint64, @string> resources;
    internal uint64 focusResource;
    internal map<uint64, task> tasks;
    internal uint64 asyncSliceSeq;
}

partial struct task {
    internal @string name;
    internal nint sortIndex;
}

public static void Gomaxprocs(this ref Emitter e, uint64 v) {
    if (v > e.gomaxprocs) {
        e.gomaxprocs = v;
    }
}

public static void Resource(this ref Emitter e, uint64 id, @string name) {
    if (e.filter != default! && !e.filter(id)) {
        return;
    }
    e.resources[id] = name;
}

public static void SetResourceType(this ref Emitter e, @string name) {
    e.resourceType = name;
}

public static void SetResourceFilter(this ref Emitter e, Func<uint64, bool> filter) {
    e.filter = filter;
}

public static void Task(this ref Emitter e, uint64 id, @string name, nint sortIndex) {
    e.tasks[id] = new task(name, sortIndex);
}

public static void Slice(this ref Emitter e, SliceEvent s) {
    if (e.filter != default! && !e.filter(s.Resource)) {
        return;
    }
    e.Δslice(s, format.ProcsSection, ""u8);
}

public static void TaskSlice(this ref Emitter e, SliceEvent s) {
    e.Δslice(s, format.TasksSection, pickTaskColor(s.Resource));
}

internal static void Δslice(this ref Emitter e, SliceEvent s, uint64 sectionID, @string cname) {
    if (!e.tsWithinRange(s.Ts) && !e.tsWithinRange(s.Ts + s.Dur)) {
        return;
    }
    e.OptionalEvent(Ꮡ(new format.Event(
        Name: s.Name,
        Phase: "X"u8,
        Time: viewerTime(s.Ts),
        Dur: viewerTime(s.Dur),
        PID: sectionID,
        TID: s.Resource,
        Stack: s.Stack,
        EndStack: s.EndStack,
        Arg: s.Arg,
        Cname: cname
    )));
}

partial struct SliceEvent {
    public @string Name;
    public time.Duration Ts;
    public time.Duration Dur;
    public uint64 Resource;
    public nint Stack;
    public nint EndStack;
    public any Arg;
}

public static void AsyncSlice(this ref Emitter e, AsyncSliceEvent s) {
    if (!e.tsWithinRange(s.Ts) && !e.tsWithinRange(s.Ts + s.Dur)) {
        return;
    }
    if (e.filter != default! && !e.filter(s.Resource)) {
        return;
    }
    ref var cname = ref heap<@string>(out var Ꮡcname);
    cname = ""u8;
    if (s.TaskColorIndex != 0) {
        cname = pickTaskColor(s.TaskColorIndex);
    }
    e.asyncSliceSeq++;
    e.OptionalEvent(Ꮡ(new format.Event(
        Category: s.Category,
        Name: s.Name,
        Phase: "b"u8,
        Time: viewerTime(s.Ts),
        TID: s.Resource,
        ID: e.asyncSliceSeq,
        Scope: s.Scope,
        Stack: s.Stack,
        Cname: cname
    )));
    e.OptionalEvent(Ꮡ(new format.Event(
        Category: s.Category,
        Name: s.Name,
        Phase: "e"u8,
        Time: viewerTime(s.Ts + s.Dur),
        TID: s.Resource,
        ID: e.asyncSliceSeq,
        Scope: s.Scope,
        Stack: s.EndStack,
        Arg: s.Arg,
        Cname: cname
    )));
}

partial struct AsyncSliceEvent {
    public partial ref SliceEvent SliceEvent { get; }
    public @string Category;
    public @string Scope;
    public uint64 TaskColorIndex; // Take on the same color as the task with this ID.
}

public static void Instant(this ref Emitter e, InstantEvent i) {
    if (!e.tsWithinRange(i.Ts)) {
        return;
    }
    if (e.filter != default! && !e.filter(i.Resource)) {
        return;
    }
    ref var cname = ref heap<@string>(out var Ꮡcname);
    cname = ""u8;
    e.OptionalEvent(Ꮡ(new format.Event(
        Name: i.Name,
        Category: i.Category,
        Phase: "I"u8,
        Scope: "t"u8,
        Time: viewerTime(i.Ts),
        PID: format.ProcsSection,
        TID: i.Resource,
        Stack: i.Stack,
        Cname: cname,
        Arg: i.Arg
    )));
}

partial struct InstantEvent {
    public time.Duration Ts;
    public @string Name;
    public @string Category;
    public uint64 Resource;
    public nint Stack;
    public any Arg;
}

public static void Arrow(this ref Emitter e, ArrowEvent a) {
    if (e.filter != default! && (!e.filter(a.FromResource) || !e.filter(a.ToResource))) {
        return;
    }
    e.arrow(a, format.ProcsSection);
}

public static void TaskArrow(this ref Emitter e, ArrowEvent a) {
    e.arrow(a, format.TasksSection);
}

internal static void arrow(this ref Emitter e, ArrowEvent a, uint64 sectionID) {
    if (!e.tsWithinRange(a.Start) || !e.tsWithinRange(a.End)) {
        return;
    }
    e.arrowSeq++;
    e.OptionalEvent(Ꮡ(new format.Event(
        Name: a.Name,
        Phase: "s"u8,
        TID: a.FromResource,
        PID: sectionID,
        ID: e.arrowSeq,
        Time: viewerTime(a.Start),
        Stack: a.FromStack
    )));
    e.OptionalEvent(Ꮡ(new format.Event(
        Name: a.Name,
        Phase: "t"u8,
        TID: a.ToResource,
        PID: sectionID,
        ID: e.arrowSeq,
        Time: viewerTime(a.End)
    )));
}

partial struct ArrowEvent {
    public @string Name;
    public time.Duration Start;
    public time.Duration End;
    public uint64 FromResource;
    public nint FromStack;
    public uint64 ToResource;
}

public static void Event(this ref Emitter e, ж<format.Event> Ꮡev) {
    e.c.ConsumeViewerEvent(Ꮡev, true);
}

public static void HeapAlloc(this ref Emitter e, time.Duration ts, uint64 v) {
    e.heapStats.heapAlloc = v;
    e.emitHeapCounters(ts);
}

public static void Focus(this ref Emitter e, uint64 id) {
    e.focusResource = id;
}

public static void GoroutineTransition(this ref Emitter e, time.Duration ts, GState from, GState to) {
    e.gstates[from]--;
    e.gstates[to]++;
    if (e.prevGstates == e.gstates) {
        return;
    }
    if (e.tsWithinRange(ts)) {
        e.OptionalEvent(Ꮡ(new format.Event(
            Name: "Goroutines"u8,
            Phase: "C"u8,
            Time: viewerTime(ts),
            PID: 1,
            Arg: Ꮡ(new format.GoroutineCountersArg(
                Running: (uint64)e.gstates[GRunning],
                Runnable: (uint64)e.gstates[GRunnable],
                GCWaiting: (uint64)e.gstates[GWaitingGC]
            ))
        )));
    }
    e.prevGstates = e.gstates.Clone();
}

public static void IncThreadStateCount(this ref Emitter e, time.Duration ts, ThreadState state, int64 delta) {
    e.threadStats[state] += delta;
    if (e.prevThreadStats == e.threadStats) {
        return;
    }
    if (e.tsWithinRange(ts)) {
        e.OptionalEvent(Ꮡ(new format.Event(
            Name: "Threads"u8,
            Phase: "C"u8,
            Time: viewerTime(ts),
            PID: 1,
            Arg: Ꮡ(new format.ThreadCountersArg(
                Running: (int64)e.threadStats[ThreadStateRunning],
                InSyscall: (int64)e.threadStats[ThreadStateInSyscall]
            ))
        )));
    }
    // TODO(mknyszek): Why is InSyscallRuntime not included here?
    e.prevThreadStats = e.threadStats.Clone();
}

public static void HeapGoal(this ref Emitter e, time.Duration ts, uint64 v) {
    // This cutoff at 1 PiB is a Workaround for https://github.com/golang/go/issues/63864.
    //
    // TODO(mknyszek): Remove this once the problem has been fixed.
    const uint64 PB = /* 1 << 50 */ 1125899906842624;
    if (v > PB) {
        v = 0;
    }
    e.heapStats.nextGC = v;
    e.emitHeapCounters(ts);
}

internal static void emitHeapCounters(this ref Emitter e, time.Duration ts) {
    if (e.prevHeapStats == e.heapStats) {
        return;
    }
    ref var diff = ref heap<uint64>(out var Ꮡdiff);
    diff = (uint64)0;
    if (e.heapStats.nextGC > e.heapStats.heapAlloc) {
        diff = e.heapStats.nextGC - e.heapStats.heapAlloc;
    }
    if (e.tsWithinRange(ts)) {
        e.OptionalEvent(Ꮡ(new format.Event(
            Name: "Heap"u8,
            Phase: "C"u8,
            Time: viewerTime(ts),
            PID: 1,
            Arg: Ꮡ(new format.HeapCountersArg(Allocated: e.heapStats.heapAlloc, NextGC: diff))
        )));
    }
    e.prevHeapStats = e.heapStats;
}

// Err returns an error if the emitter is in an invalid state.
public static error Err(this ref Emitter e) {
    if (e.gstates[GRunnable] < 0 || e.gstates[GRunning] < 0 || e.threadStats[ThreadStateInSyscall] < 0 || e.threadStats[ThreadStateInSyscallRuntime] < 0) {
        return fmt.Errorf(
            "runnable=%d running=%d insyscall=%d insyscallRuntime=%d"u8,
            e.gstates[GRunnable],
            e.gstates[GRunning],
            e.threadStats[ThreadStateInSyscall],
            e.threadStats[ThreadStateInSyscallRuntime]);
    }
    return default!;
}

internal static bool tsWithinRange(this ref Emitter e, time.Duration ts) {
    return e.rangeStart <= ts && ts <= e.rangeEnd;
}

// OptionalEvent emits ev if it's within the time range of the consumer, i.e.
// the selected trace split range.
public static void OptionalEvent(this ref Emitter e, ж<format.Event> Ꮡev) {
    e.c.ConsumeViewerEvent(Ꮡev, false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string statsˢ = "STATS"u8;
private static readonly @string tasksˢ = "TASKS"u8;
private static readonly @string networkˢ = "Network"u8;
private static readonly @string timersˢ = "Timers"u8;
private static readonly @string syscallsˢ = "Syscalls"u8;

public static void Flush(this ref Emitter e) {
    e.processMeta(format.StatsSection, statsˢ, 0);
    if (len(e.tasks) != 0) {
        e.processMeta(format.TasksSection, tasksˢ, 1);
    }
    foreach (var (id, task) in e.tasks) {
        e.threadMeta(format.TasksSection, id, task.name, task.sortIndex);
    }
    e.processMeta(format.ProcsSection, e.resourceType, 2);
    e.threadMeta(format.ProcsSection, trace.GCP, "GC"u8, -6);
    e.threadMeta(format.ProcsSection, trace.NetpollP, networkˢ, -5);
    e.threadMeta(format.ProcsSection, trace.TimerP, timersˢ, -4);
    e.threadMeta(format.ProcsSection, trace.SyscallP, syscallsˢ, -3);
    foreach (var (id, name) in e.resources) {
        nint priority = (nint)id;
        if (e.focusResource != 0 && id == e.focusResource) {
            // Put the focus goroutine on top.
            priority = -2;
        }
        e.threadMeta(format.ProcsSection, id, name, priority);
    }
    e.c.Flush();
}

internal static void threadMeta(this ref Emitter e, uint64 sectionID, uint64 tid, @string name, nint priority) {
    e.Event(Ꮡ(new format.Event(
        Name: "thread_name"u8,
        Phase: "M"u8,
        PID: sectionID,
        TID: tid,
        Arg: Ꮡ(new format.NameArg(Name: name))
    )));
    e.Event(Ꮡ(new format.Event(
        Name: "thread_sort_index"u8,
        Phase: "M"u8,
        PID: sectionID,
        TID: tid,
        Arg: Ꮡ(new format.SortIndexArg(Index: priority))
    )));
}

internal static void processMeta(this ref Emitter e, uint64 sectionID, @string name, nint priority) {
    e.Event(Ꮡ(new format.Event(
        Name: "process_name"u8,
        Phase: "M"u8,
        PID: sectionID,
        Arg: Ꮡ(new format.NameArg(Name: name))
    )));
    e.Event(Ꮡ(new format.Event(
        Name: "process_sort_index"u8,
        Phase: "M"u8,
        PID: sectionID,
        Arg: Ꮡ(new format.SortIndexArg(Index: priority))
    )));
}

// Stack emits the given frames and returns a unique id for the stack. No
// pointers to the given data are being retained beyond the call to Stack.
public static nint Stack(this ref Emitter e, slice<ж<trace.Frame>> stk) {
    return e.buildBranch(e.frameTree, stk);
}

// buildBranch builds one branch in the prefix tree rooted at ctx.frameTree.
internal static nint buildBranch(this ref Emitter e, frameNode parent, slice<ж<trace.Frame>> stk) {
    if (len(stk) == 0) {
        return parent.id;
    }
    nint last = len(stk) - 1;
    var frame = stk[last];
    stk = stk.slice(0, last);
    var (node, ok) = parent.children[(~frame).PC, ꟷ];
    if (!ok) {
        e.frameSeq++;
        node.id = e.frameSeq;
        node.children = new map<uint64, frameNode>();
        parent.children[(~frame).PC] = node;
        e.c.ConsumeViewerFrame(strconv.Itoa(node.id), new format.Frame(Name: fmt.Sprintf("%v:%v"u8, (~frame).Fn, (~frame).Line), Parent: parent.id));
    }
    return e.buildBranch(node, stk);
}

partial struct heapStats {
    internal uint64 heapAlloc;
    internal uint64 nextGC;
}

internal static float64 viewerTime(time.Duration t) {
    return (float64)(int64)t / (float64)(int64)time.Microsecond;
}

partial struct GState /*num:nint*/;

public static GState GDead => /* iota */ 0;
public static GState GRunnable => 1;
public static GState GRunning => 2;
public static GState GWaiting => 3;
public static GState GWaitingGC => 4;
internal static GState gStateCount => 5;

partial struct ThreadState /*num:nint*/;

public static ThreadState ThreadStateInSyscall => /* iota */ 0;
public static ThreadState ThreadStateInSyscallRuntime => 1;
public static ThreadState ThreadStateRunning => 2;
internal static ThreadState threadStateCount => 3;

partial struct frameNode {
    internal nint id;
    internal map<uint64, frameNode> children;
}

// Mapping from more reasonable color names to the reserved color names in
// https://github.com/catapult-project/catapult/blob/master/tracing/tracing/base/color_scheme.html#L50
// The chrome trace viewer allows only those as cname values.
internal static readonly @string colorLightMauve = "thread_state_uninterruptible"u8; // 182, 125, 143

internal static readonly @string colorOrange = "thread_state_iowait"u8; // 255, 140, 0

internal static readonly @string colorSeafoamGreen = "thread_state_running"u8; // 126, 200, 148

internal static readonly @string colorVistaBlue = "thread_state_runnable"u8; // 133, 160, 210

internal static readonly @string colorTan = "thread_state_unknown"u8; // 199, 155, 125

internal static readonly @string colorIrisBlue = "background_memory_dump"u8; // 0, 180, 180

internal static readonly @string colorMidnightBlue = "light_memory_dump"u8; // 0, 0, 180

internal static readonly @string colorDeepMagenta = "detailed_memory_dump"u8; // 180, 0, 180

internal static readonly @string colorBlue = "vsync_highlight_color"u8; // 0, 0, 255

internal static readonly @string colorGrey = "generic_work"u8;     // 125, 125, 125

internal static readonly @string colorGreen = "good"u8;             // 0, 125, 0

internal static readonly @string colorDarkGoldenrod = "bad"u8;              // 180, 125, 0

internal static readonly @string colorPeach = "terrible"u8;         // 180, 0, 0

internal static readonly @string colorBlack = "black"u8;            // 0, 0, 0

internal static readonly @string colorLightGrey = "grey"u8;             // 221, 221, 221

internal static readonly @string colorWhite = "white"u8;            // 255, 255, 255

internal static readonly @string colorYellow = "yellow"u8;           // 255, 255, 0

internal static readonly @string colorOlive = "olive"u8;            // 100, 100, 0

internal static readonly @string colorCornflowerBlue = "rail_response"u8;    // 67, 135, 253

internal static readonly @string colorSunsetOrange = "rail_animation"u8;   // 244, 74, 63

internal static readonly @string colorTangerine = "rail_idle"u8;        // 238, 142, 0

internal static readonly @string colorShamrockGreen = "rail_load"u8;        // 13, 168, 97

internal static readonly @string colorGreenishYellow = "startup"u8;          // 230, 230, 0

internal static readonly @string colorDarkGrey = "heap_dump_stack_frame"u8; // 128, 128, 128

internal static readonly @string colorTawny = "heap_dump_child_node_arrow"u8; // 204, 102, 0

internal static readonly @string colorLemon = "cq_build_running"u8; // 255, 255, 119

internal static readonly @string colorLime = "cq_build_passed"u8;  // 153, 238, 102

internal static readonly @string colorPink = "cq_build_failed"u8;  // 238, 136, 136

internal static readonly @string colorSilver = "cq_build_abandoned"u8; // 187, 187, 187

internal static readonly @string colorManzGreen = "cq_build_attempt_runnig"u8; // 222, 222, 75

internal static readonly @string colorKellyGreen = "cq_build_attempt_passed"u8; // 108, 218, 35

internal static readonly @string colorAnotherGrey = "cq_build_attempt_failed"u8; // 187, 187, 187

internal static slice<@string> colorForTask = new @string[]{
    colorLightMauve,
    colorOrange,
    colorSeafoamGreen,
    colorVistaBlue,
    colorTan,
    colorMidnightBlue,
    colorIrisBlue,
    colorDeepMagenta,
    colorGreen,
    colorDarkGoldenrod,
    colorPeach,
    colorOlive,
    colorCornflowerBlue,
    colorSunsetOrange,
    colorTangerine,
    colorShamrockGreen,
    colorTawny,
    colorLemon,
    colorLime,
    colorPink,
    colorSilver,
    colorManzGreen,
    colorKellyGreen
}.slice();

internal static @string pickTaskColor(uint64 id) {
    var idx = id % (uint64)len(colorForTask);
    return colorForTask[idx];
}

} // end traceviewer_package
