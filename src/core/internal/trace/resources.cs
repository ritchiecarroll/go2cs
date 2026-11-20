// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using fmt = fmt_package;
using static trace_package;

partial class trace_package {

// ThreadID is the runtime-internal M structure's ID. This is unique
// for each OS thread.
partial struct ThreadID /*num:int64*/;

// NoThread indicates that the relevant events don't correspond to any
// thread in particular.
public static ThreadID NoThread => /* ThreadID(-1) */ -1;

// ProcID is the runtime-internal G structure's id field. This is unique
// for each P.
partial struct ProcID /*num:int64*/;

// NoProc indicates that the relevant events don't correspond to any
// P in particular.
public static ProcID NoProc => /* ProcID(-1) */ -1;

// GoID is the runtime-internal G structure's goid field. This is unique
// for each goroutine.
partial struct GoID /*num:int64*/;

// NoGoroutine indicates that the relevant events don't correspond to any
// goroutine in particular.
public static GoID NoGoroutine => /* GoID(-1) */ -1;

// GoState represents the state of a goroutine.
//
// New GoStates may be added in the future. Users of this type must be robust
// to that possibility.
partial struct GoState /*num:uint8*/;

public static GoState GoUndetermined => /* iota */ 0; // No information is known about the goroutine.
public static GoState GoNotExist => 1;                // Goroutine does not exist.
public static GoState GoRunnable => 2;                // Goroutine is runnable but not running.
public static GoState GoRunning => 3;                 // Goroutine is running.
public static GoState GoWaiting => 4;                 // Goroutine is waiting on something to happen.
public static GoState GoSyscall => 5;                 // Goroutine is in a system call.

} partial class trace_package {

// Executing returns true if the state indicates that the goroutine is executing
// and bound to its thread.
internal static bool Executingᴍ(GoState s) {
    return s == GoRunning || s == GoSyscall;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string undeterminedˢᴍGoState => ref undeterminedˢᵇᴍGoState;
private static readonly @string undeterminedˢᵇᴍGoState = "Undetermined"u8;
internal static ref readonly @string notExistˢᴍGoState => ref notExistˢᵇᴍGoState;
private static readonly @string notExistˢᵇᴍGoState = "NotExist"u8;
internal static ref readonly @string runnableˢᴍGoState => ref runnableˢᵇᴍGoState;
private static readonly @string runnableˢᵇᴍGoState = "Runnable"u8;
internal static ref readonly @string runningˢᴍGoState => ref runningˢᵇᴍGoState;
private static readonly @string runningˢᵇᴍGoState = "Running"u8;
internal static ref readonly @string waitingˢᴍGoState => ref waitingˢᵇᴍGoState;
private static readonly @string waitingˢᵇᴍGoState = "Waiting"u8;
internal static ref readonly @string syscallˢᴍGoState => ref syscallˢᵇᴍGoState;
private static readonly @string syscallˢᵇᴍGoState = "Syscall"u8;
internal static ref readonly @string badˢᴍGoState => ref badˢᵇᴍGoState;
private static readonly @string badˢᵇᴍGoState = "Bad"u8;

// String returns a human-readable representation of a GoState.
//
// The format of the returned string is for debugging purposes and is subject to change.
internal static @string Stringᴍ(GoState s) {
    switch (s) {
    case var ᴛ when ᴛ == GoUndetermined: {
        return undeterminedˢᴍGoState;
    }
    case var ᴛ when ᴛ == GoNotExist: {
        return notExistˢᴍGoState;
    }
    case var ᴛ when ᴛ == GoRunnable: {
        return runnableˢᴍGoState;
    }
    case var ᴛ when ᴛ == GoRunning: {
        return runningˢᴍGoState;
    }
    case var ᴛ when ᴛ == GoWaiting: {
        return waitingˢᴍGoState;
    }
    case var ᴛ when ᴛ == GoSyscall: {
        return syscallˢᴍGoState;
    }}
    return badˢᴍGoState;
}

} partial class trace_package {

// ProcState represents the state of a proc.
//
// New ProcStates may be added in the future. Users of this type must be robust
// to that possibility.
partial struct ProcState /*num:uint8*/;

public static ProcState ProcUndetermined => /* iota */ 0; // No information is known about the proc.
public static ProcState ProcNotExist => 1;                // Proc does not exist.
public static ProcState ProcRunning => 2;                 // Proc is running.
public static ProcState ProcIdle => 3;                    // Proc is idle.

} partial class trace_package {

// Executing returns true if the state indicates that the proc is executing
// and bound to its thread.
internal static bool Executingᴍ(ProcState s) {
    return s == ProcRunning;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string undeterminedˢᴍProcState => ref undeterminedˢᵇᴍProcState;
private static readonly @string undeterminedˢᵇᴍProcState = "Undetermined"u8;
internal static ref readonly @string notExistˢᴍProcState => ref notExistˢᵇᴍProcState;
private static readonly @string notExistˢᵇᴍProcState = "NotExist"u8;
internal static ref readonly @string runningˢᴍProcState => ref runningˢᵇᴍProcState;
private static readonly @string runningˢᵇᴍProcState = "Running"u8;
internal static ref readonly @string idleˢᴍProcState => ref idleˢᵇᴍProcState;
private static readonly @string idleˢᵇᴍProcState = "Idle"u8;
internal static ref readonly @string badˢᴍProcState => ref badˢᵇᴍProcState;
private static readonly @string badˢᵇᴍProcState = "Bad"u8;

// String returns a human-readable representation of a ProcState.
//
// The format of the returned string is for debugging purposes and is subject to change.
internal static @string Stringᴍ(ProcState s) {
    switch (s) {
    case var ᴛ when ᴛ == ProcUndetermined: {
        return undeterminedˢᴍProcState;
    }
    case var ᴛ when ᴛ == ProcNotExist: {
        return notExistˢᴍProcState;
    }
    case var ᴛ when ᴛ == ProcRunning: {
        return runningˢᴍProcState;
    }
    case var ᴛ when ᴛ == ProcIdle: {
        return idleˢᴍProcState;
    }}
    return badˢᴍProcState;
}

} partial class trace_package {

// ResourceKind indicates a kind of resource that has a state machine.
//
// New ResourceKinds may be added in the future. Users of this type must be robust
// to that possibility.
partial struct ResourceKind /*num:uint8*/;

public static ResourceKind ResourceNone => /* iota */ 0; // No resource.
public static ResourceKind ResourceGoroutine => 1;       // Goroutine.
public static ResourceKind ResourceProc => 2;            // Proc.
public static ResourceKind ResourceThread => 3;          // Thread.

} partial class trace_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noneˢᴍResourceKind => ref noneˢᵇᴍResourceKind;
private static readonly @string noneˢᵇᴍResourceKind = "None"u8;
internal static ref readonly @string goroutineˢᴍResourceKind => ref goroutineˢᵇᴍResourceKind;
private static readonly @string goroutineˢᵇᴍResourceKind = "Goroutine"u8;
internal static ref readonly @string procˢᴍResourceKind => ref procˢᵇᴍResourceKind;
private static readonly @string procˢᵇᴍResourceKind = "Proc"u8;
internal static ref readonly @string threadˢᴍResourceKind => ref threadˢᵇᴍResourceKind;
private static readonly @string threadˢᵇᴍResourceKind = "Thread"u8;
internal static ref readonly @string badˢᴍResourceKind => ref badˢᵇᴍResourceKind;
private static readonly @string badˢᵇᴍResourceKind = "Bad"u8;

// String returns a human-readable representation of a ResourceKind.
//
// The format of the returned string is for debugging purposes and is subject to change.
internal static @string Stringᴍ(ResourceKind r) {
    switch (r) {
    case var ᴛ when ᴛ == ResourceNone: {
        return noneˢᴍResourceKind;
    }
    case var ᴛ when ᴛ == ResourceGoroutine: {
        return goroutineˢᴍResourceKind;
    }
    case var ᴛ when ᴛ == ResourceProc: {
        return procˢᴍResourceKind;
    }
    case var ᴛ when ᴛ == ResourceThread: {
        return threadˢᴍResourceKind;
    }}
    return badˢᴍResourceKind;
}

} partial class trace_package {

// ResourceID represents a generic resource ID.
partial struct ResourceID {
    // Kind is the kind of resource this ID is for.
    public ResourceKind Kind;
    internal int64 id;
}

// MakeResourceID creates a general resource ID from a specific resource's ID.
public static ResourceID MakeResourceID<T>(T id)
    where T : /* interface{internal/trace.GoID | internal/trace.ProcID | internal/trace.ThreadID} */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    ResourceID rd = default;
    any a = id;
    switch (a.type()) {
    case GoID: {
        rd.Kind = ResourceGoroutine;
        break;
    }
    case ProcID: {
        rd.Kind = ResourceProc;
        break;
    }
    case ThreadID: {
        rd.Kind = ResourceThread;
        break;
    }}
    rd.id = (int64)ConvertToUInt64<T>(id);
    return rd;
}

} partial class trace_package {

// Goroutine obtains a GoID from the resource ID.
//
// r.Kind must be ResourceGoroutine or this function will panic.
internal static GoID Goroutineᴍ(ResourceID r) {
    if (r.Kind != ResourceGoroutine) {
        throw panic(fmt.Sprintf("attempted to get GoID from %s resource ID"u8, r.Kind));
    }
    return (GoID)r.id;
}

// Proc obtains a ProcID from the resource ID.
//
// r.Kind must be ResourceProc or this function will panic.
internal static ProcID Procᴍ(ResourceID r) {
    if (r.Kind != ResourceProc) {
        throw panic(fmt.Sprintf("attempted to get ProcID from %s resource ID"u8, r.Kind));
    }
    return (ProcID)r.id;
}

// Thread obtains a ThreadID from the resource ID.
//
// r.Kind must be ResourceThread or this function will panic.
internal static ThreadID Threadᴍ(ResourceID r) {
    if (r.Kind != ResourceThread) {
        throw panic(fmt.Sprintf("attempted to get ThreadID from %s resource ID"u8, r.Kind));
    }
    return (ThreadID)r.id;
}

// String returns a human-readable string representation of the ResourceID.
//
// This representation is subject to change and is intended primarily for debugging.
internal static @string Stringᴍ(ResourceID r) {
    if (r.Kind == ResourceNone) {
        return r.Kind.String();
    }
    return fmt.Sprintf("%s(%d)"u8, r.Kind, r.id);
}

} partial class trace_package {

// StateTransition provides details about a StateTransition event.
partial struct StateTransition {
    // Resource is the resource this state transition is for.
    public ResourceID Resource;
    // Reason is a human-readable reason for the state transition.
    public @string Reason;
    // Stack is the stack trace of the resource making the state transition.
    //
    // This is distinct from the result (Event).Stack because it pertains to
    // the transitioning resource, not any of the ones executing the event
    // this StateTransition came from.
    //
    // An example of this difference is the NotExist -> Runnable transition for
    // goroutines, which indicates goroutine creation. In this particular case,
    // a Stack here would refer to the starting stack of the new goroutine, and
    // an (Event).Stack would refer to the stack trace of whoever created the
    // goroutine.
    public Stack Stack;
    // The actual transition data. Stored in a neutral form so that
    // we don't need fields for every kind of resource.
    internal uint8 oldState;
    internal uint8 newState;
}

// MakeGoStateTransition creates a goroutine state transition.
public static StateTransition MakeGoStateTransition(GoID id, GoState from, GoState to) {
    return new StateTransition(
        Resource: new ResourceID(Kind: ResourceGoroutine, id: (int64)id),
        oldState: (uint8)from,
        newState: (uint8)to
    );
}

// MakeProcStateTransition creates a proc state transition.
public static StateTransition MakeProcStateTransition(ProcID id, ProcState from, ProcState to) {
    return new StateTransition(
        Resource: new ResourceID(Kind: ResourceProc, id: (int64)id),
        oldState: (uint8)from,
        newState: (uint8)to
    );
}

} partial class trace_package {

// Goroutine returns the state transition for a goroutine.
//
// Transitions to and from states that are Executing are special in that
// they change the future execution context. In other words, future events
// on the same thread will feature the same goroutine until it stops running.
//
// Panics if d.Resource.Kind is not ResourceGoroutine.
internal static (GoState from, GoState to) Goroutineᴍ(StateTransition d) {
    if (d.Resource.Kind != ResourceGoroutine) {
        throw panic("Goroutine called on non-Goroutine state transition");
    }
    return ((GoState)d.oldState, (GoState)d.newState);
}

// Proc returns the state transition for a proc.
//
// Transitions to and from states that are Executing are special in that
// they change the future execution context. In other words, future events
// on the same thread will feature the same goroutine until it stops running.
//
// Panics if d.Resource.Kind is not ResourceProc.
internal static (ProcState from, ProcState to) Procᴍ(StateTransition d) {
    if (d.Resource.Kind != ResourceProc) {
        throw panic("Proc called on non-Proc state transition");
    }
    return ((ProcState)d.oldState, (ProcState)d.newState);
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class traceᴛGoState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Executing(this GoState s) => trace_package.Executingᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this GoState s) => trace_package.Stringᴍ(s);
}
partial class traceᴛProcState {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static bool Executing(this ProcState s) => trace_package.Executingᴍ(s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ProcState s) => trace_package.Stringᴍ(s);
}
partial class traceᴛResourceKind {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ResourceKind r) => trace_package.Stringᴍ(r);
}
partial class traceᴛResourceID {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static GoID Goroutine(this ResourceID r) => trace_package.Goroutineᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ProcID Proc(this ResourceID r) => trace_package.Procᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static ThreadID Thread(this ResourceID r) => trace_package.Threadᴍ(r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ResourceID r) => trace_package.Stringᴍ(r);
}
partial class traceᴛStateTransition {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (GoState from, GoState to) Goroutine(this StateTransition d) => trace_package.Goroutineᴍ(d);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (ProcState from, ProcState to) Proc(this StateTransition d) => trace_package.Procᴍ(d);
}
// </GlyphForwarders>
