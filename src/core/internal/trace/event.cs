// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using iter = iter_package;
using math = math_package;
using regexp = regexp_package;
using slices = slices_package;
using strconv = go.strconv_package;
using strings = strings_package;
using time = time_package;
using tracev2 = go.@internal.trace.tracev2_package;
using version = go.@internal.trace.version_package;
using go;
using go.@internal.trace;

partial class trace_package {

// EventKind indicates the kind of event this is.
//
// Use this information to obtain a more specific event that
// allows access to more detailed information.
partial struct EventKind /*num:uint16*/;

public static EventKind EventBad => /* iota */ 0;

// EventKindSync is an event that indicates a global synchronization
// point in the trace. At the point of a sync event, the
// trace reader can be certain that all resources (e.g. threads,
// goroutines) that have existed until that point have been enumerated.
public static EventKind EventSync => 1;

// EventMetric is an event that represents the value of a metric at
// a particular point in time.
public static EventKind EventMetric => 2;

// EventLabel attaches a label to a resource.
public static EventKind EventLabel => 3;

// EventStackSample represents an execution sample, indicating what a
// thread/proc/goroutine was doing at a particular point in time via
// its backtrace.
//
// Note: Samples should be considered a close approximation of
// what a thread/proc/goroutine was executing at a given point in time.
// These events may slightly contradict the situation StateTransitions
// describe, so they should only be treated as a best-effort annotation.
public static EventKind EventStackSample => 4;

// EventRangeBegin and EventRangeEnd are a pair of generic events representing
// a special range of time. Ranges are named and scoped to some resource
// (identified via ResourceKind). A range that has begun but has not ended
// is considered active.
//
// EvRangeBegin and EvRangeEnd will share the same name, and an End will always
// follow a Begin on the same instance of the resource. The associated
// resource ID can be obtained from the Event. ResourceNone indicates the
// range is globally scoped. That is, any goroutine/proc/thread can start or
// stop, but only one such range may be active at any given time.
//
// EventRangeActive is like EventRangeBegin, but indicates that the range was
// already active. In this case, the resource referenced may not be in the current
// context.
public static EventKind EventRangeBegin => 5;
public static EventKind EventRangeActive => 6;
public static EventKind EventRangeEnd => 7;

// EvTaskBegin and EvTaskEnd are a pair of events representing a runtime/trace.Task.
public static EventKind EventTaskBegin => 8;
public static EventKind EventTaskEnd => 9;

// EventRegionBegin and EventRegionEnd are a pair of events represent a runtime/trace.Region.
public static EventKind EventRegionBegin => 10;
public static EventKind EventRegionEnd => 11;

// EventLog represents a runtime/trace.Log call.
public static EventKind EventLog => 12;

// EventStateTransition represents a state change for some resource.
public static EventKind EventStateTransition => 13;

// EventExperimental is an experimental event that is unvalidated and exposed in a raw form.
// Users are expected to understand the format and perform their own validation. These events
// may always be safely ignored.
public static EventKind EventExperimental => 14;

// String returns a string form of the EventKind.
public static @string String(this EventKind e) {
    if ((nint)(uint16)e >= len(eventKindStrings)) {
        return eventKindStrings[0];
    }
    return eventKindStrings[e];
}

internal static array<@string> eventKindStrings = new golib.SparseArray<@string>{
    [EventBad] = "Bad"u8,
    [EventSync] = "Sync"u8,
    [EventMetric] = "Metric"u8,
    [EventLabel] = "Label"u8,
    [EventStackSample] = "StackSample"u8,
    [EventRangeBegin] = "RangeBegin"u8,
    [EventRangeActive] = "RangeActive"u8,
    [EventRangeEnd] = "RangeEnd"u8,
    [EventTaskBegin] = "TaskBegin"u8,
    [EventTaskEnd] = "TaskEnd"u8,
    [EventRegionBegin] = "RegionBegin"u8,
    [EventRegionEnd] = "RegionEnd"u8,
    [EventLog] = "Log"u8,
    [EventStateTransition] = "StateTransition"u8,
    [EventExperimental] = "Experimental"u8
}.array();

internal static ΔTime maxTime => /* Time(math.MaxInt64) */ 9223372036854775807;

// Time is a timestamp in nanoseconds.
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
partial struct ΔTime /*num:int64*/;

// Sub subtracts t0 from t, returning the duration in nanoseconds.
public static time.Duration Sub(this ΔTime t, ΔTime t0) {
    return (time.Duration)((int64)t - (int64)t0);
}

// Metric provides details about a Metric event.
partial struct ΔMetric {
    // Name is the name of the sampled metric.
    //
    // Names follow the same convention as metric names in the
    // runtime/metrics package, meaning they include the unit.
    // Names that match with the runtime/metrics package represent
    // the same quantity. Note that this corresponds to the
    // runtime/metrics package for the Go version this trace was
    // collected for.
    public @string Name;
    // Value is the sampled value of the metric.
    //
    // The Value's Kind is tied to the name of the metric, and so is
    // guaranteed to be the same for metric samples for the same metric.
    public Value Value;
}

// Label provides details about a Label event.
partial struct ΔLabel {
    // Label is the label applied to some resource.
    public @string ΔΔLabel;
    // Resource is the resource to which this label should be applied.
    public ResourceID Resource;
}

// Range provides details about a Range event.
partial struct ΔRange {
    // Name is a human-readable name for the range.
    //
    // This name can be used to identify the end of the range for the resource
    // its scoped to, because only one of each type of range may be active on
    // a particular resource. The relevant resource should be obtained from the
    // Event that produced these details. The corresponding RangeEnd will have
    // an identical name.
    public @string Name;
    // Scope is the resource that the range is scoped to.
    //
    // For example, a ResourceGoroutine scope means that the same goroutine
    // must have a start and end for the range, and that goroutine can only
    // have one range of a particular name active at any given time. The
    // ID that this range is scoped to may be obtained via Event.Goroutine.
    //
    // The ResourceNone scope means that the range is globally scoped. As a
    // result, any goroutine/proc/thread may start or end the range, and only
    // one such named range may be active globally at any given time.
    //
    // For RangeBegin and RangeEnd events, this will always reference some
    // resource ID in the current execution context. For RangeActive events,
    // this may reference a resource not in the current context. Prefer Scope
    // over the current execution context.
    public ResourceID Scope;
}

// RangeAttribute provides attributes about a completed Range.
partial struct RangeAttribute {
    // Name is the human-readable name for the range.
    public @string Name;
    // Value is the value of the attribute.
    public Value Value;
}

// TaskID is the internal ID of a task used to disambiguate tasks (even if they
// are of the same type).
partial struct TaskID /*num:uint64*/;

// NoTask indicates the lack of a task.
public static TaskID NoTask => /* TaskID(^uint64(0)) */ unchecked((TaskID)18446744073709551615);

// BackgroundTask is the global task that events are attached to if there was
// no other task in the context at the point the event was emitted.
public static TaskID BackgroundTask => /* TaskID(0) */ 0;

// Task provides details about a Task event.
partial struct ΔTask {
    // ID is a unique identifier for the task.
    //
    // This can be used to associate the beginning of a task with its end.
    public TaskID ID;
    // ParentID is the ID of the parent task.
    public TaskID Parent;
    // Type is the taskType that was passed to runtime/trace.NewTask.
    //
    // May be "" if a task's TaskBegin event isn't present in the trace.
    public @string Type;
}

// Region provides details about a Region event.
partial struct ΔRegion {
    // Task is the ID of the task this region is associated with.
    public TaskID Task;
    // Type is the regionType that was passed to runtime/trace.StartRegion or runtime/trace.WithRegion.
    public @string Type;
}

// Log provides details about a Log event.
partial struct ΔLog {
    // Task is the ID of the task this region is associated with.
    public TaskID Task;
    // Category is the category that was passed to runtime/trace.Log or runtime/trace.Logf.
    public @string Category;
    // Message is the message that was passed to runtime/trace.Log or runtime/trace.Logf.
    public @string Message;
}

// StackSample is used to construct StackSample events via MakeEvent. There are
// no details associated with it, use EventConfig.Stack instead.
partial struct StackSample {
}

// MakeStack create a stack from a list of stack frames.
public static ΔStack MakeStack(slice<StackFrame> frames) {
    // TODO(felixge): support evTable reuse.
    var tbl = Ꮡ(new evTable(pcs: new map<uint64, frame>()));
    tbl.of(evTable.Ꮡstrings).compactify();
    tbl.of(evTable.Ꮡstacks).compactify();
    return new ΔStack(table: tbl, id: addStack(tbl, frames));
}

// Stack represents a stack. It's really a handle to a stack and it's trivially comparable.
//
// If two Stacks are equal then their Frames are guaranteed to be identical. If they are not
// equal, however, their Frames may still be equal.
partial struct ΔStack {
    internal ж<evTable> table;
    internal stackID id;
}

// Frames is an iterator over the frames in a Stack.
public static iter.Seq<StackFrame> Frames(this ΔStack s) {
    var sʗ1 = s;
    return (Func<StackFrame, bool> yield) => {
        if (sʗ1.id == 0) {
            return;
        }
        var stk = sʗ1.table.of(evTable.Ꮡstacks).mustGet(sʗ1.id);
        foreach (var (_, pc) in stk.pcs) {
            var f = (~sʗ1.table).pcs[pc];
            var sf = new StackFrame(
                PC: f.pc,
                Func: sʗ1.table.of(evTable.Ꮡstrings).mustGet(f.funcID),
                File: sʗ1.table.of(evTable.Ꮡstrings).mustGet(f.fileID),
                Line: f.line
            );
            if (!yield(sf)) {
                return;
            }
        }
    };
}

// String returns the stack as a human-readable string.
//
// The format of the string is intended for debugging and is subject to change.
public static @string String(this ΔStack s) {
    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    printStack(new strings_BuilderжWriter(Ꮡsb), ""u8, s.Frames());
    return sb.String();
}

internal static void printStack(io.Writer w, @string prefix, iter.Seq<StackFrame> frames) {
    foreach (var f in range<StackFrame>(frames.Invoke)) {
        fmt.Fprintf(w, "%s%s @ 0x%x\n"u8, prefix, f.Func, f.PC);
        fmt.Fprintf(w, "%s\t%s:%d\n"u8, prefix, f.File, f.Line);
    }
}

// NoStack is a sentinel value that can be compared against any Stack value, indicating
// a lack of a stack trace.
public static ΔStack NoStack = new ΔStack(nil);

// StackFrame represents a single frame of a stack.
partial struct StackFrame {
    // PC is the program counter of the function call if this
    // is not a leaf frame. If it's a leaf frame, it's the point
    // at which the stack trace was taken.
    public uint64 PC;
    // Func is the name of the function this frame maps to.
    public @string Func;
    // File is the file which contains the source code of Func.
    public @string File;
    // Line is the line number within File which maps to PC.
    public uint64 Line;
}

// ExperimentalEvent presents a raw view of an experimental event's arguments and their names.
partial struct ExperimentalEvent {
    // Name is the name of the event.
    public @string Name;
    // Experiment is the name of the experiment this event is a part of.
    public @string Experiment;
    // Args lists the names of the event's arguments in order.
    public slice<@string> Args;
    // argValues contains the raw integer arguments which are interpreted
    // by ArgValue using table.
    internal ж<evTable> table;
    internal slice<uint64> argValues;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringˢ = "string"u8;

// ArgValue returns a typed Value for the i'th argument in the experimental event.
public static Value ArgValue(this ExperimentalEvent e, nint i) {
    if (i < 0 || i >= len(e.Args)) {
        throw panic(fmt.Sprintf("experimental event argument index %d out of bounds [0, %d)"u8, i, len(e.Args)));
    }
    if (strings.HasSuffix(e.Args[i], stringˢ)) {
        @string s = e.table.of(evTable.Ꮡstrings).mustGet((stringID)e.argValues[i]);
        return StringValue(s);
    }
    return Uint64Value(e.argValues[i]);
}

// ExperimentalBatch represents a packet of unparsed data along with metadata about that packet.
partial struct ExperimentalBatch {
    // Thread is the ID of the thread that produced a packet of data.
    public ThreadID Thread;
    // Data is a packet of unparsed data all produced by one thread.
    public slice<byte> Data;
}

partial interface EventDetails<ΔT> {
    //  Type constraints: Metric | Label | Range | StateTransition | Sync | Task | Region | Log | StackSample
    // Derived operators: none
}

// EventConfig holds the data for constructing a trace event.
partial struct EventConfig<T>
    where T : /* EventDetails = Metric | Label | Range | StateTransition | Sync | Task | Region | Log | StackSample */ new()
{
    // Time is the timestamp of the event.
    public ΔTime Time;
    // Kind is the kind of the event.
    public EventKind Kind;
    // Goroutine is the goroutine ID of the event.
    public GoID Goroutine;
    // Proc is the proc ID of the event.
    public ProcID Proc;
    // Thread is the thread ID of the event.
    public ThreadID Thread;
    // Stack is the stack of the event.
    public ΔStack Stack;
    // Details is the kind specific details of the event.
    public T Details;
}

// MakeEvent creates a new trace event from the given configuration.
public static (ΔEvent e, error err) MakeEvent<T>(EventConfig<T> c)
    where T : /* EventDetails = Metric | Label | Range | StateTransition | Sync | Task | Region | Log | StackSample */ new() {
    ΔEvent e = new();
    error err = default!;
    GoFrame ᒐ = default;
    try
    {
        // TODO(felixge): make the evTable reusable.
        e = new ΔEvent(
            table: Ꮡ(new evTable(pcs: new map<uint64, frame>(), sync: new sync(freq: 1D))),
            @base: new baseEvent(time: c.Time),
            ctx: new schedCtx(G: c.Goroutine, P: c.Proc, M: c.Thread)
        );
        var cʗ1 = c;
        defer(() => {
            // N.b. evSync is not in tracev2.Specs()
            if (err != default! || e.@base.typ == evSync) {
                return;
            }
            ref var spec = ref heap<tracev2.EventSpec>(out var Ꮡspec);
            spec = tracev2.Specs()[e.@base.typ];
            if (len(spec.StackIDs) > 0 && cʗ1.Stack != NoStack) {
                // The stack for the main execution context is always the
                // first stack listed in StackIDs. Subtract one from this
                // because we've peeled away the timestamp argument.
                e.@base.args[spec.StackIDs[0] - 1] = (uint64)addStack(e.table, slices.Collect(cʗ1.Stack.Frames()));
            }

            e.table.of(evTable.Ꮡstrings).compactify();
            e.table.of(evTable.Ꮡstacks).compactify();
        }, ref ᒐ);
        EventKind defaultKind = default!;
        var exprᴛ1 = c.Kind;
        if (exprᴛ1 == defaultKind) {
            (e, err) = (new ΔEvent(nil), fmt.Errorf("the Kind field must be provided"u8)); goto ᒐdone;
        }
        if (exprᴛ1 == EventMetric) {
            {
                var (m, ok) = ((any)c.Details)._<ΔMetric>(ᐧ); if (ok) {
                    (e, err) = makeMetricEvent(e, m); goto ᒐdone;
                }
            }
        }
        else if (exprᴛ1 == EventLabel) {
            {
                var (l, ok) = ((any)c.Details)._<ΔLabel>(ᐧ); if (ok) {
                    (e, err) = makeLabelEvent(e, l); goto ᒐdone;
                }
            }
        }
        else if (exprᴛ1 == EventRangeBegin || exprᴛ1 == EventRangeActive || exprᴛ1 == EventRangeEnd) {
            {
                var (r, ok) = ((any)c.Details)._<ΔRange>(ᐧ); if (ok) {
                    (e, err) = makeRangeEvent(e, c.Kind, r); goto ᒐdone;
                }
            }
        }
        else if (exprᴛ1 == EventStateTransition) {
            {
                var (t, ok) = ((any)c.Details)._<ΔStateTransition>(ᐧ); if (ok) {
                    (e, err) = makeStateTransitionEvent(e, t); goto ᒐdone;
                }
            }
        }
        else if (exprᴛ1 == EventSync) {
            {
                var (s, ok) = ((any)c.Details)._<ΔSync>(ᐧ); if (ok) {
                    (e, err) = makeSyncEvent(e, s); goto ᒐdone;
                }
            }
        }
        else if (exprᴛ1 == EventTaskBegin || exprᴛ1 == EventTaskEnd) {
            {
                var (t, ok) = ((any)c.Details)._<ΔTask>(ᐧ); if (ok) {
                    (e, err) = makeTaskEvent(e, c.Kind, t); goto ᒐdone;
                }
            }
        }
        else if (exprᴛ1 == EventRegionBegin || exprᴛ1 == EventRegionEnd) {
            {
                var (r, ok) = ((any)c.Details)._<ΔRegion>(ᐧ); if (ok) {
                    (e, err) = makeRegionEvent(e, c.Kind, r); goto ᒐdone;
                }
            }
        }
        else if (exprᴛ1 == EventLog) {
            {
                var (l, ok) = ((any)c.Details)._<ΔLog>(ᐧ); if (ok) {
                    (e, err) = makeLogEvent(e, l); goto ᒐdone;
                }
            }
        }
        else if (exprᴛ1 == EventStackSample) {
            {
                var (_, ok) = ((any)c.Details)._<StackSample>(ᐧ); if (ok) {
                    (e, err) = makeStackSampleEvent(e, c.Stack); goto ᒐdone;
                }
            }
        }
        (e, err) = (new ΔEvent(nil), fmt.Errorf("the Kind field %s is incompatible with Details type %T"u8, c.Kind, c.Details));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (e, err);
}

internal static (ΔEvent, error) makeMetricEvent(ΔEvent e, ΔMetric m) {
    e = e.ΔClone();

    if (m.Value.Kind() != ValueUint64) {
        return (new ΔEvent(nil), fmt.Errorf("metric value must be a uint64, got: %s"u8, m.Value.String()));
    }
    var exprᴛ1 = m.Name;
    if (exprᴛ1 == "/sched/gomaxprocs:threads"u8) {
        e.@base.typ = tracev2.EvProcsChange;
    }
    else if (exprᴛ1 == "/memory/classes/heap/objects:bytes"u8) {
        e.@base.typ = tracev2.EvHeapAlloc;
    }
    else if (exprᴛ1 == "/gc/heap/goal:bytes"u8) {
        e.@base.typ = tracev2.EvHeapGoal;
    }
    else { /* default: */
        return (new ΔEvent(nil), fmt.Errorf("unknown metric name: %s"u8, m.Name));
    }
    e.@base.args[0] = (uint64)m.Value.Uint64();
    return (e.ΔClone(), default!);
}

internal static (ΔEvent, error) makeLabelEvent(ΔEvent e, ΔLabel l) {
    e = e.ΔClone();

    if (l.Resource.Kind != ResourceGoroutine) {
        return (new ΔEvent(nil), fmt.Errorf("resource must be a goroutine: %s"u8, l.Resource));
    }
    e.@base.typ = tracev2.EvGoLabel;
    e.@base.args[0] = (uint64)e.table.of(evTable.Ꮡstrings).append(l.ΔΔLabel);
    // TODO(felixge): check against sched ctx and return error on mismatch
    e.ctx.G = l.Resource.Goroutine();
    return (e.ΔClone(), default!);
}

internal static ж<regexp.Regexp> stwRangeRegexp = regexp.MustCompile(@"^stop-the-world \((.*)\)$"u8);

// TODO(felixge): should this ever manipulate the e ctx? Or just report mismatches?
internal static (ΔEvent, error) makeRangeEvent(ΔEvent e, EventKind kind, ΔRange r) {
    e = e.ΔClone();

    // TODO(felixge): Should we add dedicated range kinds rather than using
    // string names?
    var exprᴛ1 = r.Name;
    if (exprᴛ1 == "GC concurrent mark phase"u8) {
        if (r.Scope.Kind != ResourceNone) {
            return (new ΔEvent(nil), fmt.Errorf("unexpected scope: %s"u8, r.Scope));
        }
        var exprᴛ2 = kind;
        if (exprᴛ2 == EventRangeBegin) {
            e.@base.typ = tracev2.EvGCBegin;
        }
        else if (exprᴛ2 == EventRangeActive) {
            e.@base.typ = tracev2.EvGCActive;
        }
        else if (exprᴛ2 == EventRangeEnd) {
            e.@base.typ = tracev2.EvGCEnd;
        }
        else { /* default: */
            return (new ΔEvent(nil), fmt.Errorf("unexpected range kind: %s"u8, kind));
        }
    }
    else if (exprᴛ1 == "GC incremental sweep"u8) {
        if (r.Scope.Kind != ResourceProc) {
            return (new ΔEvent(nil), fmt.Errorf("unexpected scope: %s"u8, r.Scope));
        }
        var exprᴛ3 = kind;
        if (exprᴛ3 == EventRangeBegin) {
            e.@base.typ = tracev2.EvGCSweepBegin;
            e.ctx.P = r.Scope.Proc();
        }
        else if (exprᴛ3 == EventRangeActive) {
            e.@base.typ = tracev2.EvGCSweepActive;
            e.@base.args[0] = (uint64)(int64)r.Scope.Proc();
        }
        else if (exprᴛ3 == EventRangeEnd) {
            e.@base.typ = tracev2.EvGCSweepEnd;
            // TODO(felixge): check against sched ctx and return error on mismatch
            e.ctx.P = r.Scope.Proc();
        }
        else { /* default: */
            return (new ΔEvent(nil), fmt.Errorf("unexpected range kind: %s"u8, kind));
        }
    }
    else if (exprᴛ1 == "GC mark assist"u8) {
        if (r.Scope.Kind != ResourceGoroutine) {
            return (new ΔEvent(nil), fmt.Errorf("unexpected scope: %s"u8, r.Scope));
        }
        var exprᴛ4 = kind;
        if (exprᴛ4 == EventRangeBegin) {
            e.@base.typ = tracev2.EvGCMarkAssistBegin;
            e.ctx.G = r.Scope.Goroutine();
        }
        else if (exprᴛ4 == EventRangeActive) {
            e.@base.typ = tracev2.EvGCMarkAssistActive;
            e.@base.args[0] = (uint64)(int64)r.Scope.Goroutine();
        }
        else if (exprᴛ4 == EventRangeEnd) {
            e.@base.typ = tracev2.EvGCMarkAssistEnd;
            // TODO(felixge): check against sched ctx and return error on mismatch
            e.ctx.G = r.Scope.Goroutine();
        }
        else { /* default: */
            return (new ΔEvent(nil), fmt.Errorf("unexpected range kind: %s"u8, kind));
        }
    }
    else { /* default: */
        var match = stwRangeRegexp.FindStringSubmatch(r.Name);
        if (len(match) != 2) {
            return (new ΔEvent(nil), fmt.Errorf("unexpected range name: %s"u8, r.Name));
        }
        if (r.Scope.Kind != ResourceGoroutine) {
            return (new ΔEvent(nil), fmt.Errorf("unexpected scope: %s"u8, r.Scope));
        }
        var exprᴛ5 = kind;
        if (exprᴛ5 == EventRangeBegin) {
            e.@base.typ = tracev2.EvSTWBegin;
            // TODO(felixge): check against sched ctx and return error on mismatch
            e.ctx.G = r.Scope.Goroutine();
        }
        else if (exprᴛ5 == EventRangeEnd) {
            e.@base.typ = tracev2.EvSTWEnd;
            // TODO(felixge): check against sched ctx and return error on mismatch
            e.ctx.G = r.Scope.Goroutine();
        }
        else { /* default: */
            return (new ΔEvent(nil), fmt.Errorf("unexpected range kind: %s"u8, kind));
        }
        e.@base.args[0] = (uint64)e.table.of(evTable.Ꮡstrings).append(match[1]);
    }
    return (e.ΔClone(), default!);
}

internal static (ΔEvent, error) makeStateTransitionEvent(ΔEvent e, ΔStateTransition t) {
    e = e.ΔClone();

    var exprᴛ1 = t.Resource.Kind;
    if (exprᴛ1 == ResourceProc) {
        var (from, to) = ((ProcState)t.oldState, (ProcState)t.newState);
        switch (ᐧ) {
        case {} when from == ProcIdle && to == ProcIdle: {
            // TODO(felixge): Could this also be a ProcStatus event?
            e.@base.typ = tracev2.EvProcSteal;
            e.@base.args[0] = (uint64)(int64)t.Resource.Proc();
            e.@base.extra(version.Go122)[0] = (uint64)(uint8)tracev2.ProcSyscallAbandoned;
            break;
        }
        case {} when from == ProcIdle && to == ProcRunning: {
            e.@base.typ = tracev2.EvProcStart;
            e.@base.args[0] = (uint64)(int64)t.Resource.Proc();
            break;
        }
        case {} when from == ProcRunning && to == ProcIdle: {
            e.@base.typ = tracev2.EvProcStop;
            if (t.Resource.Proc() != e.ctx.P) {
                e.@base.typ = tracev2.EvProcSteal;
                e.@base.args[0] = (uint64)(int64)t.Resource.Proc();
            }
            break;
        }
        default: {
            e.@base.typ = tracev2.EvProcStatus;
            e.@base.args[0] = (uint64)(int64)t.Resource.Proc();
            e.@base.args[1] = (uint64)(uint8)procState2Tracev2ProcStatus[to];
            e.@base.extra(version.Go122)[0] = (uint64)(uint8)procState2Tracev2ProcStatus[from];
            return (e.ΔClone(), default!);
        }}
    }
    else if (exprᴛ1 == ResourceGoroutine) {
        var (from, to) = ((GoState)t.oldState, (GoState)t.newState);
        var stack = slices.Collect(t.Stack.Frames());
        var goroutine = t.Resource.Goroutine();

        if ((from == GoUndetermined || from == to) && from != GoNotExist){
            e.@base.typ = tracev2.EvGoStatus;
            if (len(stack) > 0) {
                e.@base.typ = tracev2.EvGoStatusStack;
            }
            e.@base.args[0] = (uint64)(int64)goroutine;
            e.@base.args[2] = (uint64)(uint8)from << 32 | (uint64)(uint8)goState2Tracev2GoStatus[to];
        } else {
            var exprᴛ2 = from;
            if (exprᴛ2 == GoNotExist) {
                var exprᴛ3 = to;
                if (exprᴛ3 == GoWaiting) {
                    e.@base.typ = tracev2.EvGoCreateBlocked;
                    e.@base.args[0] = (uint64)(int64)goroutine;
                    e.@base.args[1] = (uint64)addStack(e.table, stack);
                }
                else if (exprᴛ3 == GoRunnable) {
                    e.@base.typ = tracev2.EvGoCreate;
                    e.@base.args[0] = (uint64)(int64)goroutine;
                    e.@base.args[1] = (uint64)addStack(e.table, stack);
                }
                else if (exprᴛ3 == GoSyscall) {
                    e.@base.typ = tracev2.EvGoCreateSyscall;
                    e.@base.args[0] = (uint64)(int64)goroutine;
                }
                else { /* default: */
                    return (new ΔEvent(nil), fmt.Errorf("unexpected transition: %s -> %s"u8, from, to));
                }
            }
            else if (exprᴛ2 == GoRunnable) {
                e.@base.typ = tracev2.EvGoStart;
                e.@base.args[0] = (uint64)(int64)goroutine;
            }
            else if (exprᴛ2 == GoRunning) {
                var exprᴛ4 = to;
                if (exprᴛ4 == GoNotExist) {
                    e.@base.typ = tracev2.EvGoDestroy;
                    e.ctx.G = goroutine;
                }
                else if (exprᴛ4 == GoRunnable) {
                    e.@base.typ = tracev2.EvGoStop;
                    e.ctx.G = goroutine;
                    e.@base.args[0] = (uint64)e.table.of(evTable.Ꮡstrings).append(t.Reason);
                }
                else if (exprᴛ4 == GoWaiting) {
                    e.@base.typ = tracev2.EvGoBlock;
                    e.ctx.G = goroutine;
                    e.@base.args[0] = (uint64)e.table.of(evTable.Ꮡstrings).append(t.Reason);
                }
                else if (exprᴛ4 == GoSyscall) {
                    e.@base.typ = tracev2.EvGoSyscallBegin;
                    e.ctx.G = goroutine;
                }
                else { /* default: */
                    return (new ΔEvent(nil), fmt.Errorf("unexpected transition: %s -> %s"u8, from, to));
                }
            }
            else if (exprᴛ2 == GoSyscall) {
                var exprᴛ5 = to;
                if (exprᴛ5 == GoNotExist) {
                    e.@base.typ = tracev2.EvGoDestroySyscall;
                    e.ctx.G = goroutine;
                }
                else if (exprᴛ5 == GoRunning) {
                    e.@base.typ = tracev2.EvGoSyscallEnd;
                    e.ctx.G = goroutine;
                }
                else if (exprᴛ5 == GoRunnable) {
                    e.@base.typ = tracev2.EvGoSyscallEndBlocked;
                    e.ctx.G = goroutine;
                }
                else { /* default: */
                    return (new ΔEvent(nil), fmt.Errorf("unexpected transition: %s -> %s"u8, from, to));
                }
            }
            else if (exprᴛ2 == GoWaiting) {
                var exprᴛ6 = to;
                if (exprᴛ6 == GoRunnable) {
                    e.@base.typ = tracev2.EvGoUnblock;
                    e.@base.args[0] = (uint64)(int64)goroutine;
                }
                else { /* default: */
                    return (new ΔEvent(nil), fmt.Errorf("unexpected transition: %s -> %s"u8, from, to));
                }
            }
            else { /* default: */
                return (new ΔEvent(nil), fmt.Errorf("unexpected transition: %s -> %s"u8, from, to));
            }
        }
    }
    else { /* default: */
        return (new ΔEvent(nil), fmt.Errorf("unsupported state transition resource: %s"u8, t.Resource));
    }
    return (e.ΔClone(), default!);
}

internal static (ΔEvent, error) makeSyncEvent(ΔEvent e, ΔSync s) {
    e = e.ΔClone();

    e.@base.typ = evSync;
    e.@base.args[0] = (uint64)s.N;
    if ((~e.table).expBatches == default!) {
        e.table.Value.expBatches = new map<tracev2.Experiment, slice<ExperimentalBatch>>();
    }
    foreach (var (name, batches) in s.ExperimentalBatches) {
        bool found = default!;
        foreach (var (id, exp) in tracev2.Experiments()) {
            if (exp == name) {
                found = true;
                e.table.Value.expBatches[((tracev2.Experiment)(nuint)id)] = batches;
                break;
            }
        }
        if (!found) {
            return (new ΔEvent(nil), fmt.Errorf("unknown experiment: %s"u8, name));
        }
    }
    if (s.ClockSnapshot != nil) {
        e.table.Value.hasClockSnapshot = true;
        e.table.Value.snapWall = s.ClockSnapshot.Value.Wall;
        e.table.Value.snapMono = s.ClockSnapshot.Value.Mono;
        // N.b. MakeEvent sets e.table.freq to 1.
        e.table.Value.snapTime = (timestamp)(uint64)(int64)(~s.ClockSnapshot).Trace;
    }
    return (e.ΔClone(), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string taskIdCannotBeNoTaskˢ = "task ID cannot be NoTask"u8;

internal static (ΔEvent, error) makeTaskEvent(ΔEvent e, EventKind kind, ΔTask t) {
    e = e.ΔClone();

    if (t.ID == NoTask) {
        return (new ΔEvent(nil), errors.New(taskIdCannotBeNoTaskˢ));
    }
    e.@base.args[0] = (uint64)t.ID;
    var exprᴛ1 = kind;
    if (exprᴛ1 == EventTaskBegin) {
        e.@base.typ = tracev2.EvUserTaskBegin;
        e.@base.args[1] = (uint64)t.Parent;
        e.@base.args[2] = (uint64)e.table.of(evTable.Ꮡstrings).append(t.Type);
    }
    else if (exprᴛ1 == EventTaskEnd) {
        e.@base.typ = tracev2.EvUserTaskEnd;
        e.@base.extra(version.Go122)[0] = (uint64)t.Parent;
        e.@base.extra(version.Go122)[1] = (uint64)e.table.addExtraString(t.Type);
    }
    else { /* default: */
        // TODO(felixge): also do this for ranges?
        throw panic("unexpected task kind");
    }
    return (e.ΔClone(), default!);
}

internal static (ΔEvent, error) makeRegionEvent(ΔEvent e, EventKind kind, ΔRegion r) {
    e = e.ΔClone();

    e.@base.args[0] = (uint64)r.Task;
    e.@base.args[1] = (uint64)e.table.of(evTable.Ꮡstrings).append(r.Type);
    var exprᴛ1 = kind;
    if (exprᴛ1 == EventRegionBegin) {
        e.@base.typ = tracev2.EvUserRegionBegin;
    }
    else if (exprᴛ1 == EventRegionEnd) {
        e.@base.typ = tracev2.EvUserRegionEnd;
    }
    else { /* default: */
        throw panic("unexpected region kind");
    }
    return (e.ΔClone(), default!);
}

internal static (ΔEvent, error) makeLogEvent(ΔEvent e, ΔLog l) {
    e = e.ΔClone();

    e.@base.typ = tracev2.EvUserLog;
    e.@base.args[0] = (uint64)l.Task;
    e.@base.args[1] = (uint64)e.table.of(evTable.Ꮡstrings).append(l.Category);
    e.@base.args[2] = (uint64)e.table.of(evTable.Ꮡstrings).append(l.Message);
    return (e.ΔClone(), default!);
}

internal static (ΔEvent, error) makeStackSampleEvent(ΔEvent e, ΔStack s) {
    e = e.ΔClone();

    e.@base.typ = tracev2.EvCPUSample;
    var frames = slices.Collect(s.Frames());
    e.@base.args[0] = (uint64)addStack(e.table, frames);
    return (e.ΔClone(), default!);
}

internal static stackID addStack(ж<evTable> Ꮡtable, slice<StackFrame> frames) {
    ref var table = ref Ꮡtable.DerefOrNull();

    slice<uint64> pcs = default!;
    foreach (var (_, f) in frames) {
        table.pcs[f.PC] = new frame(
            pc: f.PC,
            funcID: table.strings.append(f.Func),
            fileID: table.strings.append(f.File),
            line: f.Line
        );
        pcs = builtin.append(pcs, f.PC);
    }
    return table.stacks.append(new stack(pcs: pcs));
}

// Event represents a single event in the trace.
partial struct ΔEvent {
    internal ж<evTable> table;
    internal schedCtx ctx;
    internal baseEvent @base;
}

// Kind returns the kind of event that this is.
public static EventKind Kind(this ΔEvent e) {
    e = e.ΔClone();

    return tracev2Type2Kind[e.@base.typ];
}

// Time returns the timestamp of the event.
public static ΔTime Time(this ΔEvent e) {
    e = e.ΔClone();

    return e.@base.time;
}

// Goroutine returns the ID of the goroutine that was executing when
// this event happened. It describes part of the execution context
// for this event.
//
// Note that for goroutine state transitions this always refers to the
// state before the transition. For example, if a goroutine is just
// starting to run on this thread and/or proc, then this will return
// NoGoroutine. In this case, the goroutine starting to run will be
// can be found at Event.StateTransition().Resource.
public static GoID Goroutine(this ΔEvent e) {
    e = e.ΔClone();

    return e.ctx.G;
}

// Proc returns the ID of the proc this event event pertains to.
//
// Note that for proc state transitions this always refers to the
// state before the transition. For example, if a proc is just
// starting to run on this thread, then this will return NoProc.
public static ProcID Proc(this ΔEvent e) {
    e = e.ΔClone();

    return e.ctx.P;
}

// Thread returns the ID of the thread this event pertains to.
//
// Note that for thread state transitions this always refers to the
// state before the transition. For example, if a thread is just
// starting to run, then this will return NoThread.
//
// Note: tracking thread state is not currently supported, so this
// will always return a valid thread ID. However thread state transitions
// may be tracked in the future, and callers must be robust to this
// possibility.
public static ThreadID Thread(this ΔEvent e) {
    e = e.ΔClone();

    return e.ctx.M;
}

// Stack returns a handle to a stack associated with the event.
//
// This represents a stack trace at the current moment in time for
// the current execution context.
public static ΔStack Stack(this ΔEvent e) {
    e = e.ΔClone();

    if (e.@base.typ == evSync) {
        return NoStack;
    }
    if (e.@base.typ == tracev2.EvCPUSample) {
        return new ΔStack(table: e.table, id: ((stackID)e.@base.args[0]));
    }
    var spec = tracev2.Specs()[e.@base.typ];
    if (len(spec.StackIDs) == 0) {
        return NoStack;
    }
    // The stack for the main execution context is always the
    // first stack listed in StackIDs. Subtract one from this
    // because we've peeled away the timestamp argument.
    var id = (stackID)e.@base.args[spec.StackIDs[0] - 1];
    if (id == 0) {
        return NoStack;
    }
    return new ΔStack(table: e.table, id: id);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string schedGomaxprocsThreadsˢ = "/sched/gomaxprocs:threads"u8;
internal static readonly @string memoryClassesHeapObjectsˢ = "/memory/classes/heap/objects:bytes"u8;
internal static readonly @string gcHeapGoalBytesˢ = "/gc/heap/goal:bytes"u8;

// Metric returns details about a Metric event.
//
// Panics if Kind != EventMetric.
public static ΔMetric Metric(this ΔEvent e) {
    e = e.ΔClone();

    if (e.Kind() != EventMetric) {
        throw panic("Metric called on non-Metric event");
    }
    ΔMetric m = default!;
    var exprᴛ1 = e.@base.typ;
    if (exprᴛ1 == tracev2.EvProcsChange) {
        m.Name = schedGomaxprocsThreadsˢ;
        m.Value = Uint64Value(e.@base.args[0]);
    }
    else if (exprᴛ1 == tracev2.EvHeapAlloc) {
        m.Name = memoryClassesHeapObjectsˢ;
        m.Value = Uint64Value(e.@base.args[0]);
    }
    else if (exprᴛ1 == tracev2.EvHeapGoal) {
        m.Name = gcHeapGoalBytesˢ;
        m.Value = Uint64Value(e.@base.args[0]);
    }
    else { /* default: */
        throw panic(fmt.Sprintf("internal error: unexpected wire-format event type for Metric kind: %d"u8, e.@base.typ));
    }
    return m;
}

// Label returns details about a Label event.
//
// Panics if Kind != EventLabel.
public static ΔLabel Label(this ΔEvent e) {
    e = e.ΔClone();

    if (e.Kind() != EventLabel) {
        throw panic("Label called on non-Label event");
    }
    if (e.@base.typ != tracev2.EvGoLabel) {
        throw panic(fmt.Sprintf("internal error: unexpected wire-format event type for Label kind: %d"u8, e.@base.typ));
    }
    return new ΔLabel(
        ΔΔLabel: e.table.of(evTable.Ꮡstrings).mustGet((stringID)e.@base.args[0]),
        Resource: new ResourceID(Kind: ResourceGoroutine, id: (int64)e.ctx.G)
    );
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcConcurrentMarkPhaseˢ = "GC concurrent mark phase"u8;
internal static readonly @string gcIncrementalSweepˢ = "GC incremental sweep"u8;
internal static readonly @string gcMarkAssistˢ = "GC mark assist"u8;

// Range returns details about an EventRangeBegin, EventRangeActive, or EventRangeEnd event.
//
// Panics if Kind != EventRangeBegin, Kind != EventRangeActive, and Kind != EventRangeEnd.
public static ΔRange Range(this ΔEvent e) {
    e = e.ΔClone();

    {
        var kind = e.Kind(); if (kind != EventRangeBegin && kind != EventRangeActive && kind != EventRangeEnd) {
            throw panic("Range called on non-Range event");
        }
    }
    ΔRange r = default!;
    var exprᴛ1 = e.@base.typ;
    if (exprᴛ1 == tracev2.EvSTWBegin || exprᴛ1 == tracev2.EvSTWEnd) {
        // N.B. ordering.advance smuggles in the STW reason as e.base.args[0]
        // for tracev2.EvSTWEnd (it's already there for Begin).
        r.Name = "stop-the-world ("u8 + e.table.of(evTable.Ꮡstrings).mustGet((stringID)e.@base.args[0]) + ")"u8;
        r.Scope = new ResourceID(Kind: ResourceGoroutine, id: (int64)e.Goroutine());
    }
    else if (exprᴛ1 == tracev2.EvGCBegin || exprᴛ1 == tracev2.EvGCActive || exprᴛ1 == tracev2.EvGCEnd) {
        r.Name = gcConcurrentMarkPhaseˢ;
        r.Scope = new ResourceID(Kind: ResourceNone);
    }
    else if (exprᴛ1 == tracev2.EvGCSweepBegin || exprᴛ1 == tracev2.EvGCSweepActive || exprᴛ1 == tracev2.EvGCSweepEnd) {
        r.Name = gcIncrementalSweepˢ;
        r.Scope = new ResourceID(Kind: ResourceProc);
        if (e.@base.typ == tracev2.EvGCSweepActive){
            r.Scope.id = (int64)e.@base.args[0];
        } else {
            r.Scope.id = (int64)e.Proc();
        }
    }
    else if (exprᴛ1 == tracev2.EvGCMarkAssistBegin || exprᴛ1 == tracev2.EvGCMarkAssistActive || exprᴛ1 == tracev2.EvGCMarkAssistEnd) {
        r.Name = gcMarkAssistˢ;
        r.Scope = new ResourceID(Kind: ResourceGoroutine);
        if (e.@base.typ == tracev2.EvGCMarkAssistActive){
            r.Scope.id = (int64)e.@base.args[0];
        } else {
            r.Scope.id = (int64)e.Goroutine();
        }
    }
    else { /* default: */
        throw panic(fmt.Sprintf("internal error: unexpected wire-event type for Range kind: %d"u8, e.@base.typ));
    }
    return r;
}

// RangeAttributes returns attributes for a completed range.
//
// Panics if Kind != EventRangeEnd.
public static slice<RangeAttribute> RangeAttributes(this ΔEvent e) {
    e = e.ΔClone();

    if (e.Kind() != EventRangeEnd) {
        throw panic("Range called on non-Range event");
    }
    if (e.@base.typ != tracev2.EvGCSweepEnd) {
        return default!;
    }
    return new RangeAttribute[]{
        new(
            Name: "bytes swept"u8,
            Value: Uint64Value(e.@base.args[0])
        ),
        new(
            Name: "bytes reclaimed"u8,
            Value: Uint64Value(e.@base.args[1])
        )
    }.slice();
}

// Task returns details about a TaskBegin or TaskEnd event.
//
// Panics if Kind != EventTaskBegin and Kind != EventTaskEnd.
public static ΔTask Task(this ΔEvent e) {
    e = e.ΔClone();

    {
        var kind = e.Kind(); if (kind != EventTaskBegin && kind != EventTaskEnd) {
            throw panic("Task called on non-Task event");
        }
    }
    var parentID = NoTask;
    @string typ = default!;
    var exprᴛ1 = e.@base.typ;
    if (exprᴛ1 == tracev2.EvUserTaskBegin) {
        parentID = (TaskID)e.@base.args[1];
        typ = e.table.of(evTable.Ꮡstrings).mustGet((stringID)e.@base.args[2]);
    }
    else if (exprᴛ1 == tracev2.EvUserTaskEnd) {
        parentID = (TaskID)e.@base.extra(version.Go122)[0];
        typ = e.table.getExtraString((extraStringID)e.@base.extra(version.Go122)[1]);
    }
    else { /* default: */
        throw panic(fmt.Sprintf("internal error: unexpected wire-format event type for Task kind: %d"u8, e.@base.typ));
    }
    return new ΔTask(
        ID: ((TaskID)e.@base.args[0]),
        Parent: parentID,
        Type: typ
    );
}

// Region returns details about a RegionBegin or RegionEnd event.
//
// Panics if Kind != EventRegionBegin and Kind != EventRegionEnd.
public static ΔRegion Region(this ΔEvent e) {
    e = e.ΔClone();

    {
        var kind = e.Kind(); if (kind != EventRegionBegin && kind != EventRegionEnd) {
            throw panic("Region called on non-Region event");
        }
    }
    if (e.@base.typ != tracev2.EvUserRegionBegin && e.@base.typ != tracev2.EvUserRegionEnd) {
        throw panic(fmt.Sprintf("internal error: unexpected wire-format event type for Region kind: %d"u8, e.@base.typ));
    }
    return new ΔRegion(
        Task: ((TaskID)e.@base.args[0]),
        Type: e.table.of(evTable.Ꮡstrings).mustGet((stringID)e.@base.args[1])
    );
}

// Log returns details about a Log event.
//
// Panics if Kind != EventLog.
public static ΔLog Log(this ΔEvent e) {
    e = e.ΔClone();

    if (e.Kind() != EventLog) {
        throw panic("Log called on non-Log event");
    }
    if (e.@base.typ != tracev2.EvUserLog) {
        throw panic(fmt.Sprintf("internal error: unexpected wire-format event type for Log kind: %d"u8, e.@base.typ));
    }
    return new ΔLog(
        Task: ((TaskID)e.@base.args[0]),
        Category: e.table.of(evTable.Ꮡstrings).mustGet((stringID)e.@base.args[1]),
        Message: e.table.of(evTable.Ꮡstrings).mustGet((stringID)e.@base.args[2])
    );
}

// StateTransition returns details about a StateTransition event.
//
// Panics if Kind != EventStateTransition.
public static ΔStateTransition StateTransition(this ΔEvent e) {
    e = e.ΔClone();

    if (e.Kind() != EventStateTransition) {
        throw panic("StateTransition called on non-StateTransition event");
    }
    ΔStateTransition s = default!;
    var exprᴛ1 = e.@base.typ;
    if (exprᴛ1 == tracev2.EvProcStart) {
        s = MakeProcStateTransition((ProcID)(int64)e.@base.args[0], ProcIdle, ProcRunning);
    }
    else if (exprᴛ1 == tracev2.EvProcStop) {
        s = MakeProcStateTransition(e.ctx.P, ProcRunning, ProcIdle);
    }
    else if (exprᴛ1 == tracev2.EvProcSteal) {
        // N.B. ordering.advance populates e.base.extra.
        var beforeState = ProcRunning;
        if (((tracev2.ProcStatus)(uint8)e.@base.extra(version.Go122)[0]) == tracev2.ProcSyscallAbandoned) {
            // We've lost information because this ProcSteal advanced on a
            // SyscallAbandoned state. Treat the P as idle because ProcStatus
            // treats SyscallAbandoned as Idle. Otherwise we'll have an invalid
            // transition.
            beforeState = ProcIdle;
        }
        s = MakeProcStateTransition((ProcID)(int64)e.@base.args[0], beforeState, ProcIdle);
    }
    else if (exprᴛ1 == tracev2.EvProcStatus) {
        // N.B. ordering.advance populates e.base.extra.
        s = MakeProcStateTransition((ProcID)(int64)e.@base.args[0], (ProcState)(uint8)e.@base.extra(version.Go122)[0], tracev2ProcStatus2ProcState[e.@base.args[1]]);
    }
    else if (exprᴛ1 == tracev2.EvGoCreate || exprᴛ1 == tracev2.EvGoCreateBlocked) {
        var status = GoRunnable;
        if (e.@base.typ == tracev2.EvGoCreateBlocked) {
            status = GoWaiting;
        }
        s = MakeGoStateTransition((GoID)(int64)e.@base.args[0], GoNotExist, status);
        s.Stack = new ΔStack(table: e.table, id: ((stackID)e.@base.args[1]));
    }
    else if (exprᴛ1 == tracev2.EvGoCreateSyscall) {
        s = MakeGoStateTransition((GoID)(int64)e.@base.args[0], GoNotExist, GoSyscall);
    }
    else if (exprᴛ1 == tracev2.EvGoStart) {
        s = MakeGoStateTransition((GoID)(int64)e.@base.args[0], GoRunnable, GoRunning);
    }
    else if (exprᴛ1 == tracev2.EvGoDestroy) {
        s = MakeGoStateTransition(e.ctx.G, GoRunning, GoNotExist);
    }
    else if (exprᴛ1 == tracev2.EvGoDestroySyscall) {
        s = MakeGoStateTransition(e.ctx.G, GoSyscall, GoNotExist);
    }
    else if (exprᴛ1 == tracev2.EvGoStop) {
        s = MakeGoStateTransition(e.ctx.G, GoRunning, GoRunnable);
        s.Reason = e.table.of(evTable.Ꮡstrings).mustGet((stringID)e.@base.args[0]);
        s.Stack = e.Stack(); // This event references the resource the event happened on.
    }
    else if (exprᴛ1 == tracev2.EvGoBlock) {
        s = MakeGoStateTransition(e.ctx.G, GoRunning, GoWaiting);
        s.Reason = e.table.of(evTable.Ꮡstrings).mustGet((stringID)e.@base.args[0]);
        s.Stack = e.Stack(); // This event references the resource the event happened on.
    }
    else if (exprᴛ1 == tracev2.EvGoUnblock || exprᴛ1 == tracev2.EvGoSwitch || exprᴛ1 == tracev2.EvGoSwitchDestroy) {
        // N.B. GoSwitch and GoSwitchDestroy both emit additional events, but
        // the first thing they both do is unblock the goroutine they name,
        // identically to an unblock event (even their arguments match).
        s = MakeGoStateTransition((GoID)(int64)e.@base.args[0], GoWaiting, GoRunnable);
    }
    else if (exprᴛ1 == tracev2.EvGoSyscallBegin) {
        s = MakeGoStateTransition(e.ctx.G, GoRunning, GoSyscall);
        s.Stack = e.Stack(); // This event references the resource the event happened on.
    }
    else if (exprᴛ1 == tracev2.EvGoSyscallEnd) {
        s = MakeGoStateTransition(e.ctx.G, GoSyscall, GoRunning);
    }
    else if (exprᴛ1 == tracev2.EvGoSyscallEndBlocked) {
        s = MakeGoStateTransition(e.ctx.G, GoSyscall, GoRunnable);
    }
    else if (exprᴛ1 == tracev2.EvGoStatus || exprᴛ1 == tracev2.EvGoStatusStack) {
        var packedStatus = e.@base.args[2];
        var (from, to) = (packedStatus >> 32, (uint64)(packedStatus & ((uint64)((4294967296L) - 1))));
        s = MakeGoStateTransition((GoID)(int64)e.@base.args[0], (GoState)(uint8)from, tracev2GoStatus2GoState[to]);
        s.Stack = e.Stack(); // This event references the resource the event happened on.
    }
    else { /* default: */
        throw panic(fmt.Sprintf("internal error: unexpected wire-format event type for StateTransition kind: %d"u8, e.@base.typ));
    }
    return s;
}

// Sync returns details that are relevant for the following events, up to but excluding the
// next EventSync event.
public static ΔSync Sync(this ΔEvent e) {
    e = e.ΔClone();

    if (e.Kind() != EventSync) {
        throw panic("Sync called on non-Sync event");
    }
    var s = new ΔSync(N: (nint)e.@base.args[0]);
    if (e.table != nil) {
        var expBatches = new map<@string, slice<ExperimentalBatch>>();
        foreach (var (exp, batches) in (~e.table).expBatches) {
            expBatches[tracev2.Experiments()[exp]] = batches;
        }
        s.ExperimentalBatches = expBatches;
        if ((~e.table).hasClockSnapshot) {
            s.ClockSnapshot = Ꮡ(new ClockSnapshot(
                Trace: (~e.table).freq.mul((~e.table).snapTime),
                Wall: (~e.table).snapWall,
                Mono: (~e.table).snapMono
            ));
        }
    }
    return s;
}

// Sync contains details potentially relevant to all the following events, up to but excluding
// the next EventSync event.
partial struct ΔSync {
    // N indicates that this is the Nth sync event in the trace.
    public nint N;
    // ClockSnapshot represents a near-simultaneous clock reading of several
    // different system clocks. The snapshot can be used as a reference to
    // convert timestamps to different clocks, which is helpful for correlating
    // timestamps with data captured by other tools. The value is nil for traces
    // before go1.25.
    public ж<ClockSnapshot> ClockSnapshot;
    // ExperimentalBatches contain all the unparsed batches of data for a given experiment.
    public map<@string, slice<ExperimentalBatch>> ExperimentalBatches;
}

// ClockSnapshot represents a near-simultaneous clock reading of several
// different system clocks. The snapshot can be used as a reference to convert
// timestamps to different clocks, which is helpful for correlating timestamps
// with data captured by other tools.
partial struct ClockSnapshot {
    // Trace is a snapshot of the trace clock.
    public ΔTime Trace;
    // Wall is a snapshot of the system's wall clock.
    public time.Time Wall;
    // Mono is a snapshot of the system's monotonic clock.
    public uint64 Mono;
}

// Experimental returns a view of the raw event for an experimental event.
//
// Panics if Kind != EventExperimental.
public static ExperimentalEvent Experimental(this ΔEvent e) {
    e = e.ΔClone();

    if (e.Kind() != EventExperimental) {
        throw panic("Experimental called on non-Experimental event");
    }
    var spec = tracev2.Specs()[e.@base.typ];
    var argNames = spec.Args[1..]; // Skip timestamp; already handled.
    return new ExperimentalEvent(
        Name: spec.Name,
        Experiment: tracev2.Experiments()[spec.Experiment],
        Args: argNames,
        table: e.table,
        argValues: e.@base.args.slice(0, len(argNames))
    );
}

internal static tracev2.EventType evSync => /* ^tracev2.EventType(0) */ 255;

internal static array<EventKind> tracev2Type2Kind = new golib.SparseArray<EventKind>{
    [tracev2.EvCPUSample] = EventStackSample,
    [tracev2.EvProcsChange] = EventMetric,
    [tracev2.EvProcStart] = EventStateTransition,
    [tracev2.EvProcStop] = EventStateTransition,
    [tracev2.EvProcSteal] = EventStateTransition,
    [tracev2.EvProcStatus] = EventStateTransition,
    [tracev2.EvGoCreate] = EventStateTransition,
    [tracev2.EvGoCreateSyscall] = EventStateTransition,
    [tracev2.EvGoStart] = EventStateTransition,
    [tracev2.EvGoDestroy] = EventStateTransition,
    [tracev2.EvGoDestroySyscall] = EventStateTransition,
    [tracev2.EvGoStop] = EventStateTransition,
    [tracev2.EvGoBlock] = EventStateTransition,
    [tracev2.EvGoUnblock] = EventStateTransition,
    [tracev2.EvGoSyscallBegin] = EventStateTransition,
    [tracev2.EvGoSyscallEnd] = EventStateTransition,
    [tracev2.EvGoSyscallEndBlocked] = EventStateTransition,
    [tracev2.EvGoStatus] = EventStateTransition,
    [tracev2.EvSTWBegin] = EventRangeBegin,
    [tracev2.EvSTWEnd] = EventRangeEnd,
    [tracev2.EvGCActive] = EventRangeActive,
    [tracev2.EvGCBegin] = EventRangeBegin,
    [tracev2.EvGCEnd] = EventRangeEnd,
    [tracev2.EvGCSweepActive] = EventRangeActive,
    [tracev2.EvGCSweepBegin] = EventRangeBegin,
    [tracev2.EvGCSweepEnd] = EventRangeEnd,
    [tracev2.EvGCMarkAssistActive] = EventRangeActive,
    [tracev2.EvGCMarkAssistBegin] = EventRangeBegin,
    [tracev2.EvGCMarkAssistEnd] = EventRangeEnd,
    [tracev2.EvHeapAlloc] = EventMetric,
    [tracev2.EvHeapGoal] = EventMetric,
    [tracev2.EvGoLabel] = EventLabel,
    [tracev2.EvUserTaskBegin] = EventTaskBegin,
    [tracev2.EvUserTaskEnd] = EventTaskEnd,
    [tracev2.EvUserRegionBegin] = EventRegionBegin,
    [tracev2.EvUserRegionEnd] = EventRegionEnd,
    [tracev2.EvUserLog] = EventLog,
    [tracev2.EvGoSwitch] = EventStateTransition,
    [tracev2.EvGoSwitchDestroy] = EventStateTransition,
    [tracev2.EvGoCreateBlocked] = EventStateTransition,
    [tracev2.EvGoStatusStack] = EventStateTransition,
    [tracev2.EvSpan] = EventExperimental,
    [tracev2.EvSpanAlloc] = EventExperimental,
    [tracev2.EvSpanFree] = EventExperimental,
    [tracev2.EvHeapObject] = EventExperimental,
    [tracev2.EvHeapObjectAlloc] = EventExperimental,
    [tracev2.EvHeapObjectFree] = EventExperimental,
    [tracev2.EvGoroutineStack] = EventExperimental,
    [tracev2.EvGoroutineStackAlloc] = EventExperimental,
    [tracev2.EvGoroutineStackFree] = EventExperimental,
    [evSync] = EventSync
}.array(256);

internal static array<GoState> tracev2GoStatus2GoState = new golib.SparseArray<GoState>{
    [tracev2.GoRunnable] = GoRunnable,
    [tracev2.GoRunning] = GoRunning,
    [tracev2.GoWaiting] = GoWaiting,
    [tracev2.GoSyscall] = GoSyscall
}.array(5);

internal static array<tracev2.GoStatus> goState2Tracev2GoStatus = new golib.SparseArray<tracev2.GoStatus>{
    [GoRunnable] = tracev2.GoRunnable,
    [GoRunning] = tracev2.GoRunning,
    [GoWaiting] = tracev2.GoWaiting,
    [GoSyscall] = tracev2.GoSyscall
}.array(6);

internal static array<ProcState> tracev2ProcStatus2ProcState = new golib.SparseArray<ProcState>{
    [tracev2.ProcRunning] = ProcRunning,
    [tracev2.ProcIdle] = ProcIdle,
    [tracev2.ProcSyscall] = ProcRunning,
    [tracev2.ProcSyscallAbandoned] = ProcIdle
}.array(5);

internal static array<tracev2.ProcStatus> procState2Tracev2ProcStatus = new golib.SparseArray<tracev2.ProcStatus>{
    [ProcRunning] = tracev2.ProcRunning,
    [ProcIdle] = tracev2.ProcIdle
    // TODO(felixge): how to map ProcSyscall and ProcSyscallAbandoned?
}.array(4);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object transitionStackˢ = (@string)"TransitionStack="u8;
internal static readonly object stackˢ = (@string)"Stack="u8;

// String returns the event as a human-readable string.
//
// The format of the string is intended for debugging and is subject to change.
public static @string String(this ΔEvent e) {
    e = e.ΔClone();

    ref var sb = ref heap(new strings.Builder(), out var Ꮡsb);
    fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "M=%d P=%d G=%d"u8, e.Thread(), e.Proc(), e.Goroutine());
    fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " %s Time=%d"u8, e.Kind(), e.Time());
    // Kind-specific fields.
    {
        var kind = e.Kind();
        var exprᴛ1 = kind;
        if (exprᴛ1 == EventMetric) {
            var m = e.Metric();
            @string v = m.Value.String();
            if (m.Value.Kind() == ValueString) {
                v = strconv.Quote(v);
            }
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " Name=%q Value=%s"u8, m.Name, m.Value);
        }
        else if (exprᴛ1 == EventLabel) {
            var l = e.Label();
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " Label=%q Resource=%s"u8, l.ΔΔLabel, l.Resource);
        }
        else if (exprᴛ1 == EventRangeBegin || exprᴛ1 == EventRangeActive || exprᴛ1 == EventRangeEnd) {
            var r = e.Range();
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " Name=%q Scope=%s"u8, r.Name, r.Scope);
            if (kind == EventRangeEnd) {
                fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " Attributes=["u8);
                foreach (var (i, attr) in e.RangeAttributes()) {
                    if (i != 0) {
                        fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " "u8);
                    }
                    fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "%q=%s"u8, attr.Name, attr.Value);
                }
                fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "]"u8);
            }
        }
        else if (exprᴛ1 == EventTaskBegin || exprᴛ1 == EventTaskEnd) {
            var t = e.Task();
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " ID=%d Parent=%d Type=%q"u8, t.ID, t.Parent, t.Type);
        }
        else if (exprᴛ1 == EventRegionBegin || exprᴛ1 == EventRegionEnd) {
            var r = e.Region();
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " Task=%d Type=%q"u8, r.Task, r.Type);
        }
        else if (exprᴛ1 == EventLog) {
            var l = e.Log();
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " Task=%d Category=%q Message=%q"u8, l.Task, l.Category, l.Message);
        }
        else if (exprᴛ1 == EventStateTransition) {
            var s = e.StateTransition();
            var exprᴛ2 = s.Resource.Kind;
            if (exprᴛ2 == ResourceGoroutine) {
                var id = s.Resource.Goroutine();
                var (old, @new) = s.Goroutine();
                fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " GoID=%d %s->%s"u8, id, old, @new);
            }
            else if (exprᴛ2 == ResourceProc) {
                var id = s.Resource.Proc();
                var (old, @new) = s.Proc();
                fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " ProcID=%d %s->%s"u8, id, old, @new);
            }
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " Reason=%q"u8, s.Reason);
            if (s.Stack != NoStack) {
                fmt.Fprintln(new strings_BuilderжWriter(Ꮡsb));
                fmt.Fprintln(new strings_BuilderжWriter(Ꮡsb), transitionStackˢ);
                printStack(new strings_BuilderжWriter(Ꮡsb), "\t"u8, s.Stack.Frames());
            }
        }
        else if (exprᴛ1 == EventExperimental) {
            var r = e.Experimental();
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " Name=%s Args=["u8, r.Name);
            foreach (var (i, arg) in r.Args) {
                if (i != 0) {
                    fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), ", "u8);
                }
                fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "%s=%s"u8, arg, r.ArgValue(i).String());
            }
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), "]"u8);
        }
        else if (exprᴛ1 == EventSync) {
            var s = e.Sync();
            fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " N=%d"u8, s.N);
            if (s.ClockSnapshot != nil) {
                fmt.Fprintf(new strings_BuilderжWriter(Ꮡsb), " Trace=%d Mono=%d Wall=%s"u8,
                    (~s.ClockSnapshot).Trace,
                    (~s.ClockSnapshot).Mono,
                    (~s.ClockSnapshot).Wall.Format(time.RFC3339Nano));
            }
        }
    }
    {
        var stk = e.Stack(); if (stk != NoStack) {
            fmt.Fprintln(new strings_BuilderжWriter(Ꮡsb));
            fmt.Fprintln(new strings_BuilderжWriter(Ꮡsb), stackˢ);
            printStack(new strings_BuilderжWriter(Ꮡsb), "\t"u8, stk.Frames());
        }
    }
    return sb.String();
}

// validateTableIDs checks to make sure lookups in e.table
// will work.
internal static error validateTableIDs(this ΔEvent e) {
    e = e.ΔClone();

    if (e.@base.typ == evSync) {
        return default!;
    }
    var spec = tracev2.Specs()[e.@base.typ];

    // Check stacks.
    foreach (var (_, i) in spec.StackIDs) {
        var id = (stackID)e.@base.args[i - 1];
        var (_, ok) = e.table.of(evTable.Ꮡstacks).get(id);
        if (!ok) {
            return fmt.Errorf("found invalid stack ID %d for event %s"u8, id, spec.Name);
        }
    }
    // N.B. Strings referenced by stack frames are validated
    // early on, when reading the stacks in to begin with.

    // Check strings.
    foreach (var (_, i) in spec.StringIDs) {
        var id = (stringID)e.@base.args[i - 1];
        var (_, ok) = e.table.of(evTable.Ꮡstrings).get(id);
        if (!ok) {
            return fmt.Errorf("found invalid string ID %d for event %s"u8, id, spec.Name);
        }
    }
    return default!;
}

internal static ΔEvent syncEvent(ж<evTable> Ꮡtable, ΔTime ts, nint n) {
    var ev = new ΔEvent(
        table: Ꮡtable,
        ctx: new schedCtx(
            G: NoGoroutine,
            P: NoProc,
            M: NoThread
        ),
        @base: new baseEvent(
            typ: evSync,
            time: ts
        )
    );
    ev.@base.args[0] = (uint64)n;
    return ev.ΔClone();
}

} // end trace_package
