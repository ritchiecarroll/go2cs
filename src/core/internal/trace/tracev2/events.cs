// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.trace;

partial class tracev2_package {

// Event types in the trace, args are given in square brackets.
//
// Naming scheme:
//   - Time range event pairs have suffixes "Begin" and "End".
//   - "Start", "Stop", "Create", "Destroy", "Block", "Unblock"
//     are suffixes reserved for scheduling resources.
//
// NOTE: If you add an event type, make sure you also update all
// tables in this file!
public static EventType EvNone => /* iota */ 0; // unused

// Structural events.
public static EventType EvEventBatch => 1; // start of per-M batch of events [generation, M ID, timestamp, batch length]
public static EventType EvStacks => 2;     // start of a section of the stack table [...EvStack]
public static EventType EvStack => 3;      // stack table entry [ID, ...{PC, func string ID, file string ID, line #}]
public static EventType EvStrings => 4;    // start of a section of the string dictionary [...EvString]
public static EventType EvString => 5;     // string dictionary entry [ID, length, string]
public static EventType EvCPUSamples => 6; // start of a section of CPU samples [...EvCPUSample]
public static EventType EvCPUSample => 7;  // CPU profiling sample [timestamp, M ID, P ID, goroutine ID, stack ID]
public static EventType EvFrequency => 8;  // timestamp units per sec [freq]

// Procs.
public static EventType EvProcsChange => 9; // current value of GOMAXPROCS [timestamp, GOMAXPROCS, stack ID]
public static EventType EvProcStart => 10;  // start of P [timestamp, P ID, P seq]
public static EventType EvProcStop => 11;   // stop of P [timestamp]
public static EventType EvProcSteal => 12;  // P was stolen [timestamp, P ID, P seq, M ID]
public static EventType EvProcStatus => 13; // P status at the start of a generation [timestamp, P ID, status]

// Goroutines.
public static EventType EvGoCreate => 14;            // goroutine creation [timestamp, new goroutine ID, new stack ID, stack ID]
public static EventType EvGoCreateSyscall => 15;     // goroutine appears in syscall (cgo callback) [timestamp, new goroutine ID]
public static EventType EvGoStart => 16;             // goroutine starts running [timestamp, goroutine ID, goroutine seq]
public static EventType EvGoDestroy => 17;           // goroutine ends [timestamp]
public static EventType EvGoDestroySyscall => 18;    // goroutine ends in syscall (cgo callback) [timestamp]
public static EventType EvGoStop => 19;              // goroutine yields its time, but is runnable [timestamp, reason, stack ID]
public static EventType EvGoBlock => 20;             // goroutine blocks [timestamp, reason, stack ID]
public static EventType EvGoUnblock => 21;           // goroutine is unblocked [timestamp, goroutine ID, goroutine seq, stack ID]
public static EventType EvGoSyscallBegin => 22;      // syscall enter [timestamp, P seq, stack ID]
public static EventType EvGoSyscallEnd => 23;        // syscall exit [timestamp]
public static EventType EvGoSyscallEndBlocked => 24; // syscall exit and it blocked at some point [timestamp]
public static EventType EvGoStatus => 25;            // goroutine status at the start of a generation [timestamp, goroutine ID, M ID, status]

// STW.
public static EventType EvSTWBegin => 26; // STW start [timestamp, kind, stack ID]
public static EventType EvSTWEnd => 27;   // STW done [timestamp]

// GC events.
public static EventType EvGCActive => 28;           // GC active [timestamp, seq]
public static EventType EvGCBegin => 29;            // GC start [timestamp, seq, stack ID]
public static EventType EvGCEnd => 30;              // GC done [timestamp, seq]
public static EventType EvGCSweepActive => 31;      // GC sweep active [timestamp, P ID]
public static EventType EvGCSweepBegin => 32;       // GC sweep start [timestamp, stack ID]
public static EventType EvGCSweepEnd => 33;         // GC sweep done [timestamp, swept bytes, reclaimed bytes]
public static EventType EvGCMarkAssistActive => 34; // GC mark assist active [timestamp, goroutine ID]
public static EventType EvGCMarkAssistBegin => 35;  // GC mark assist start [timestamp, stack ID]
public static EventType EvGCMarkAssistEnd => 36;    // GC mark assist done [timestamp]
public static EventType EvHeapAlloc => 37;          // gcController.heapLive change [timestamp, heap alloc in bytes]
public static EventType EvHeapGoal => 38;           // gcController.heapGoal() change [timestamp, heap goal in bytes]

// Annotations.
public static EventType EvGoLabel => 39;         // apply string label to current running goroutine [timestamp, label string ID]
public static EventType EvUserTaskBegin => 40;   // trace.NewTask [timestamp, internal task ID, internal parent task ID, name string ID, stack ID]
public static EventType EvUserTaskEnd => 41;     // end of a task [timestamp, internal task ID, stack ID]
public static EventType EvUserRegionBegin => 42; // trace.{Start,With}Region [timestamp, internal task ID, name string ID, stack ID]
public static EventType EvUserRegionEnd => 43;   // trace.{End,With}Region [timestamp, internal task ID, name string ID, stack ID]
public static EventType EvUserLog => 44;         // trace.Log [timestamp, internal task ID, key string ID, value string ID, stack]

// Coroutines. Added in Go 1.23.
public static EventType EvGoSwitch => 45;        // goroutine switch (coroswitch) [timestamp, goroutine ID, goroutine seq]
public static EventType EvGoSwitchDestroy => 46; // goroutine switch and destroy [timestamp, goroutine ID, goroutine seq]
public static EventType EvGoCreateBlocked => 47; // goroutine creation (starts blocked) [timestamp, new goroutine ID, new stack ID, stack ID]

// GoStatus with stack. Added in Go 1.23.
public static EventType EvGoStatusStack => 48; // goroutine status at the start of a generation, with a stack [timestamp, goroutine ID, M ID, status, stack ID]

// Batch event for an experimental batch with a custom format. Added in Go 1.23.
public static EventType EvExperimentalBatch => 49; // start of extra data [experiment ID, generation, M ID, timestamp, batch length, batch data...]

// Sync batch. Added in Go 1.25. Previously a lone EvFrequency event.
public static EventType EvSync => 50;          // start of a sync batch [...EvFrequency|EvClockSnapshot]
public static EventType EvClockSnapshot => 51; // snapshot of trace, mono and wall clocks [timestamp, mono, sec, nsec]

// In-band end-of-generation signal. Added in Go 1.26.
// Used in Go 1.25 only internally.
public static EventType EvEndOfGeneration => 52;

public static EventType NumEvents => 53;

public static bool Experimental(this EventType ev) {
    return ev > MaxEvent && ev < MaxExperimentalEvent;
}

// Experiments.
// AllocFree is the alloc-free events experiment.
public static Experiment AllocFree => /* 1 + iota */ 1;

public static Experiment NumExperiments => 2;

public static slice<@string> Experiments() {
    return experiments[..];
}

internal static array<@string> experiments = new golib.SparseArray<@string>{
    [(int)((nuint)NoExperiment)] = "None"u8,
    [(int)((nuint)AllocFree)] = "AllocFree"u8
}.array();

// Experimental events.
public static EventType MaxEvent => /* 127 + iota */ 127;

// Experimental events for AllocFree.

// Experimental heap span events. Added in Go 1.23.
public static EventType EvSpan => 128;      // heap span exists [timestamp, id, npages, type/class]
public static EventType EvSpanAlloc => 129; // heap span alloc [timestamp, id, npages, type/class]
public static EventType EvSpanFree => 130;  // heap span free [timestamp, id]

// Experimental heap object events. Added in Go 1.23.
public static EventType EvHeapObject => 131;      // heap object exists [timestamp, id, type]
public static EventType EvHeapObjectAlloc => 132; // heap object alloc [timestamp, id, type]
public static EventType EvHeapObjectFree => 133;  // heap object free [timestamp, id]

// Experimental goroutine stack events. Added in Go 1.23.
public static EventType EvGoroutineStack => 134;      // stack exists [timestamp, id, order]
public static EventType EvGoroutineStackAlloc => 135; // stack alloc [timestamp, id, order]
public static EventType EvGoroutineStackFree => 136;  // stack free [timestamp, id]

public static EventType MaxExperimentalEvent => 137;

public static EventType NumExperimentalEvents => /* MaxExperimentalEvent - MaxEvent */ 10;

// MaxTimedEventArgs is the maximum number of arguments for timed events.
public static UntypedInt MaxTimedEventArgs => 5;

public static slice<EventSpec> Specs() {
    return specs[..];
}

[GoLazy] partial class lazyᴛspecs { internal static array<EventSpec> specs = new golib.SparseArray<EventSpec>{
    // "Structural" Events.
    [EvEventBatch] = new(
        Name: "EventBatch"u8,
        Args: slice<@string>(["gen"u8, "m"u8, "time"u8, "size"u8])
    ),
    [EvStacks] = new(
        Name: "Stacks"u8
    ),
    [EvStack] = new(
        Name: "Stack"u8,
        Args: slice<@string>(["id"u8, "nframes"u8]),
        IsStack: true
    ),
    [EvStrings] = new(
        Name: "Strings"u8
    ),
    [EvString] = new(
        Name: "String"u8,
        Args: slice<@string>(["id"u8]),
        HasData: true
    ),
    [EvCPUSamples] = new(
        Name: "CPUSamples"u8
    ),
    [EvCPUSample] = new(
        Name: "CPUSample"u8,
        Args: slice<@string>(["time"u8, "m"u8, "p"u8, "g"u8, "stack"u8]),
        // N.B. There's clearly a timestamp here, but these Events
        // are special in that they don't appear in the regular
        // M streams.
        StackIDs: slice<nint>([4])
    ),
    [EvFrequency] = new(
        Name: "Frequency"u8,
        Args: slice<@string>(["freq"u8])
    ),
    [EvExperimentalBatch] = new(
        Name: "ExperimentalBatch"u8,
        Args: slice<@string>(["exp"u8, "gen"u8, "m"u8, "time"u8]),
        HasData: true // Easier to represent for raw readers.

    ),
    [EvSync] = new(
        Name: "Sync"u8
    ),
    [EvEndOfGeneration] = new(
        Name: "EndOfGeneration"u8
    ),

    // "Timed" Events.
    [EvProcsChange] = new(
        Name: "ProcsChange"u8,
        Args: slice<@string>(["dt"u8, "procs_value"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([2])
    ),
    [EvProcStart] = new(
        Name: "ProcStart"u8,
        Args: slice<@string>(["dt"u8, "p"u8, "p_seq"u8]),
        IsTimedEvent: true
    ),
    [EvProcStop] = new(
        Name: "ProcStop"u8,
        Args: slice<@string>(["dt"u8]),
        IsTimedEvent: true
    ),
    [EvProcSteal] = new(
        Name: "ProcSteal"u8,
        Args: slice<@string>(["dt"u8, "p"u8, "p_seq"u8, "m"u8]),
        IsTimedEvent: true
    ),
    [EvProcStatus] = new(
        Name: "ProcStatus"u8,
        Args: slice<@string>(["dt"u8, "p"u8, "pstatus"u8]),
        IsTimedEvent: true
    ),
    [EvGoCreate] = new(
        Name: "GoCreate"u8,
        Args: slice<@string>(["dt"u8, "new_g"u8, "new_stack"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([3, 2])
    ),
    [EvGoCreateSyscall] = new(
        Name: "GoCreateSyscall"u8,
        Args: slice<@string>(["dt"u8, "new_g"u8]),
        IsTimedEvent: true
    ),
    [EvGoStart] = new(
        Name: "GoStart"u8,
        Args: slice<@string>(["dt"u8, "g"u8, "g_seq"u8]),
        IsTimedEvent: true
    ),
    [EvGoDestroy] = new(
        Name: "GoDestroy"u8,
        Args: slice<@string>(["dt"u8]),
        IsTimedEvent: true
    ),
    [EvGoDestroySyscall] = new(
        Name: "GoDestroySyscall"u8,
        Args: slice<@string>(["dt"u8]),
        IsTimedEvent: true
    ),
    [EvGoStop] = new(
        Name: "GoStop"u8,
        Args: slice<@string>(["dt"u8, "reason_string"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([2]),
        StringIDs: slice<nint>([1])
    ),
    [EvGoBlock] = new(
        Name: "GoBlock"u8,
        Args: slice<@string>(["dt"u8, "reason_string"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([2]),
        StringIDs: slice<nint>([1])
    ),
    [EvGoUnblock] = new(
        Name: "GoUnblock"u8,
        Args: slice<@string>(["dt"u8, "g"u8, "g_seq"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([3])
    ),
    [EvGoSyscallBegin] = new(
        Name: "GoSyscallBegin"u8,
        Args: slice<@string>(["dt"u8, "p_seq"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([2])
    ),
    [EvGoSyscallEnd] = new(
        Name: "GoSyscallEnd"u8,
        Args: slice<@string>(["dt"u8]),
        StartEv: EvGoSyscallBegin,
        IsTimedEvent: true
    ),
    [EvGoSyscallEndBlocked] = new(
        Name: "GoSyscallEndBlocked"u8,
        Args: slice<@string>(["dt"u8]),
        StartEv: EvGoSyscallBegin,
        IsTimedEvent: true
    ),
    [EvGoStatus] = new(
        Name: "GoStatus"u8,
        Args: slice<@string>(["dt"u8, "g"u8, "m"u8, "gstatus"u8]),
        IsTimedEvent: true
    ),
    [EvSTWBegin] = new(
        Name: "STWBegin"u8,
        Args: slice<@string>(["dt"u8, "kind_string"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([2]),
        StringIDs: slice<nint>([1])
    ),
    [EvSTWEnd] = new(
        Name: "STWEnd"u8,
        Args: slice<@string>(["dt"u8]),
        StartEv: EvSTWBegin,
        IsTimedEvent: true
    ),
    [EvGCActive] = new(
        Name: "GCActive"u8,
        Args: slice<@string>(["dt"u8, "gc_seq"u8]),
        IsTimedEvent: true,
        StartEv: EvGCBegin
    ),
    [EvGCBegin] = new(
        Name: "GCBegin"u8,
        Args: slice<@string>(["dt"u8, "gc_seq"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([2])
    ),
    [EvGCEnd] = new(
        Name: "GCEnd"u8,
        Args: slice<@string>(["dt"u8, "gc_seq"u8]),
        StartEv: EvGCBegin,
        IsTimedEvent: true
    ),
    [EvGCSweepActive] = new(
        Name: "GCSweepActive"u8,
        Args: slice<@string>(["dt"u8, "p"u8]),
        StartEv: EvGCSweepBegin,
        IsTimedEvent: true
    ),
    [EvGCSweepBegin] = new(
        Name: "GCSweepBegin"u8,
        Args: slice<@string>(["dt"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([1])
    ),
    [EvGCSweepEnd] = new(
        Name: "GCSweepEnd"u8,
        Args: slice<@string>(["dt"u8, "swept_value"u8, "reclaimed_value"u8]),
        StartEv: EvGCSweepBegin,
        IsTimedEvent: true
    ),
    [EvGCMarkAssistActive] = new(
        Name: "GCMarkAssistActive"u8,
        Args: slice<@string>(["dt"u8, "g"u8]),
        StartEv: EvGCMarkAssistBegin,
        IsTimedEvent: true
    ),
    [EvGCMarkAssistBegin] = new(
        Name: "GCMarkAssistBegin"u8,
        Args: slice<@string>(["dt"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([1])
    ),
    [EvGCMarkAssistEnd] = new(
        Name: "GCMarkAssistEnd"u8,
        Args: slice<@string>(["dt"u8]),
        StartEv: EvGCMarkAssistBegin,
        IsTimedEvent: true
    ),
    [EvHeapAlloc] = new(
        Name: "HeapAlloc"u8,
        Args: slice<@string>(["dt"u8, "heapalloc_value"u8]),
        IsTimedEvent: true
    ),
    [EvHeapGoal] = new(
        Name: "HeapGoal"u8,
        Args: slice<@string>(["dt"u8, "heapgoal_value"u8]),
        IsTimedEvent: true
    ),
    [EvGoLabel] = new(
        Name: "GoLabel"u8,
        Args: slice<@string>(["dt"u8, "label_string"u8]),
        IsTimedEvent: true,
        StringIDs: slice<nint>([1])
    ),
    [EvUserTaskBegin] = new(
        Name: "UserTaskBegin"u8,
        Args: slice<@string>(["dt"u8, "task"u8, "parent_task"u8, "name_string"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([4]),
        StringIDs: slice<nint>([3])
    ),
    [EvUserTaskEnd] = new(
        Name: "UserTaskEnd"u8,
        Args: slice<@string>(["dt"u8, "task"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([2])
    ),
    [EvUserRegionBegin] = new(
        Name: "UserRegionBegin"u8,
        Args: slice<@string>(["dt"u8, "task"u8, "name_string"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([3]),
        StringIDs: slice<nint>([2])
    ),
    [EvUserRegionEnd] = new(
        Name: "UserRegionEnd"u8,
        Args: slice<@string>(["dt"u8, "task"u8, "name_string"u8, "stack"u8]),
        StartEv: EvUserRegionBegin,
        IsTimedEvent: true,
        StackIDs: slice<nint>([3]),
        StringIDs: slice<nint>([2])
    ),
    [EvUserLog] = new(
        Name: "UserLog"u8,
        Args: slice<@string>(["dt"u8, "task"u8, "key_string"u8, "value_string"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([4]),
        StringIDs: slice<nint>([2, 3])
    ),
    [EvGoSwitch] = new(
        Name: "GoSwitch"u8,
        Args: slice<@string>(["dt"u8, "g"u8, "g_seq"u8]),
        IsTimedEvent: true
    ),
    [EvGoSwitchDestroy] = new(
        Name: "GoSwitchDestroy"u8,
        Args: slice<@string>(["dt"u8, "g"u8, "g_seq"u8]),
        IsTimedEvent: true
    ),
    [EvGoCreateBlocked] = new(
        Name: "GoCreateBlocked"u8,
        Args: slice<@string>(["dt"u8, "new_g"u8, "new_stack"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([3, 2])
    ),
    [EvGoStatusStack] = new(
        Name: "GoStatusStack"u8,
        Args: slice<@string>(["dt"u8, "g"u8, "m"u8, "gstatus"u8, "stack"u8]),
        IsTimedEvent: true,
        StackIDs: slice<nint>([4])
    ),
    [EvClockSnapshot] = new(
        Name: "ClockSnapshot"u8,
        Args: slice<@string>(["dt"u8, "mono"u8, "sec"u8, "nsec"u8]),
        IsTimedEvent: true
    ),

    // Experimental events.

    [EvSpan] = new(
        Name: "Span"u8,
        Args: slice<@string>(["dt"u8, "id"u8, "npages_value"u8, "kindclass"u8]),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvSpanAlloc] = new(
        Name: "SpanAlloc"u8,
        Args: slice<@string>(["dt"u8, "id"u8, "npages_value"u8, "kindclass"u8]),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvSpanFree] = new(
        Name: "SpanFree"u8,
        Args: slice<@string>(["dt"u8, "id"u8]),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvHeapObject] = new(
        Name: "HeapObject"u8,
        Args: slice<@string>(["dt"u8, "id"u8, "type"u8]),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvHeapObjectAlloc] = new(
        Name: "HeapObjectAlloc"u8,
        Args: slice<@string>(["dt"u8, "id"u8, "type"u8]),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvHeapObjectFree] = new(
        Name: "HeapObjectFree"u8,
        Args: slice<@string>(["dt"u8, "id"u8]),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvGoroutineStack] = new(
        Name: "GoroutineStack"u8,
        Args: slice<@string>(["dt"u8, "id"u8, "order"u8]),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvGoroutineStackAlloc] = new(
        Name: "GoroutineStackAlloc"u8,
        Args: slice<@string>(["dt"u8, "id"u8, "order"u8]),
        IsTimedEvent: true,
        Experiment: AllocFree
    ),
    [EvGoroutineStackFree] = new(
        Name: "GoroutineStackFree"u8,
        Args: slice<@string>(["dt"u8, "id"u8]),
        IsTimedEvent: true,
        Experiment: AllocFree
    )
}.array(137); }

// GoStatus is the status of a goroutine.
//
// They correspond directly to the various goroutine states.
partial struct GoStatus /*num:uint8*/;

public static GoStatus GoBad => /* iota */ 0;
public static GoStatus GoRunnable => 1;
public static GoStatus GoRunning => 2;
public static GoStatus GoSyscall => 3;
public static GoStatus GoWaiting => 4;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string runnableˢ = "Runnable"u8;
private static readonly @string runningˢ = "Running"u8;
private static readonly @string syscallˢ = "Syscall"u8;
private static readonly @string waitingˢ = "Waiting"u8;
private static readonly @string badˢ = "Bad"u8;

public static @string String(this GoStatus s) {
    switch (s) {
    case var ᴛ when ᴛ == GoRunnable: {
        return runnableˢ;
    }
    case var ᴛ when ᴛ == GoRunning: {
        return runningˢ;
    }
    case var ᴛ when ᴛ == GoSyscall: {
        return syscallˢ;
    }
    case var ᴛ when ᴛ == GoWaiting: {
        return waitingˢ;
    }}
    return badˢ;
}

// ProcStatus is the status of a P.
//
// They mostly correspond to the various P states.
partial struct ProcStatus /*num:uint8*/;

public static ProcStatus ProcBad => /* iota */ 0;
public static ProcStatus ProcRunning => 1;
public static ProcStatus ProcIdle => 2;
public static ProcStatus ProcSyscall => 3;

// ProcSyscallAbandoned is a special case of
// ProcSyscall. It's used in the very specific case
// where the first a P is mentioned in a generation is
// part of a ProcSteal event. If that's the first time
// it's mentioned, then there's no GoSyscallBegin to
// connect the P stealing back to at that point. This
// special state indicates this to the parser, so it
// doesn't try to find a GoSyscallEndBlocked that
// corresponds with the ProcSteal.
public static ProcStatus ProcSyscallAbandoned => 4;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string idleˢ = "Idle"u8;

public static @string String(this ProcStatus s) {
    switch (s) {
    case var ᴛ when ᴛ == ProcRunning: {
        return runningˢ;
    }
    case var ᴛ when ᴛ == ProcIdle: {
        return idleˢ;
    }
    case var ᴛ when ᴛ == ProcSyscall: {
        return syscallˢ;
    }}
    return badˢ;
}

// MaxBatchSize sets the maximum size that a batch can be.
//
// Directly controls the trace batch size in the runtime.
//
// NOTE: If this number decreases, the trace format version must change.
public static UntypedInt MaxBatchSize => /* 64 << 10 */ 65536;

// Maximum number of PCs in a single stack trace.
//
// Since events contain only stack ID rather than whole stack trace,
// we can allow quite large values here.
//
// Directly controls the maximum number of frames per stack
// in the runtime.
//
// NOTE: If this number decreases, the trace format version must change.
public static UntypedInt MaxFramesPerStack => 128;

// MaxEventTrailerDataSize controls the amount of trailer data that
// an event can have in bytes. Must be smaller than MaxBatchSize.
// Controls the maximum string size in the trace.
//
// Directly controls the maximum such value in the runtime.
//
// NOTE: If this number decreases, the trace format version must change.
public static UntypedInt MaxEventTrailerDataSize => /* 1 << 10 */ 1024;

} // end tracev2_package
