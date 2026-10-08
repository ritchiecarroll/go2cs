// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace goroutine and P status management.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @internal.runtime;

partial class runtime_package {

partial struct traceGoStatus /*num:uint8*/;

internal static traceGoStatus traceGoBad => /* iota */ 0;
internal static traceGoStatus traceGoRunnable => 1;
internal static traceGoStatus traceGoRunning => 2;
internal static traceGoStatus traceGoSyscall => 3;
internal static traceGoStatus traceGoWaiting => 4;

partial struct traceProcStatus /*num:uint8*/;

internal static traceProcStatus traceProcBad => /* iota */ 0;
internal static traceProcStatus traceProcRunning => 1;
internal static traceProcStatus traceProcIdle => 2;
internal static traceProcStatus traceProcSyscall => 3;
internal static traceProcStatus traceProcSyscallAbandoned => 4;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string attemptedToTraceABadˢ = "attempted to trace a bad status for a goroutine"u8;

// writeGoStatus emits a GoStatus event as well as any active ranges on the goroutine.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static traceWriter writeGoStatus(this traceWriter w, uint64 goid, int64 mid, traceGoStatus status, bool markAssist, uint64 stackID) {
    // The status should never be bad. Some invariant must have been violated.
    if (status == traceGoBad) {
        print((@string)"runtime: goid="u8, goid, (@string)"\n"u8);
        @throw(attemptedToTraceABadˢ);
    }
    // Trace the status.
    if (stackID == 0){
        w = w.@event(traceEvGoStatus, ((traceArg)goid), ((traceArg)(uint64)mid), ((traceArg)(uint64)(uint8)status));
    } else {
        w = w.@event(traceEvGoStatusStack, ((traceArg)goid), ((traceArg)(uint64)mid), ((traceArg)(uint64)(uint8)status), ((traceArg)stackID));
    }
    // Trace any special ranges that are in-progress.
    if (markAssist) {
        w = w.@event(traceEvGCMarkAssistActive, ((traceArg)goid));
    }
    return w;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string attemptToTraceInvalidOrˢ = "attempt to trace invalid or unsupported P status"u8;

// writeProcStatusForP emits a ProcStatus event for the provided p based on its status.
//
// The caller must fully own pp and it must be prevented from transitioning (e.g. this can be
// called by a forEachP callback or from a STW).
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static traceWriter writeProcStatusForP(this traceWriter w, ж<Δp> Ꮡpp, bool inSTW) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    if (!Ꮡpp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).acquireStatus(w.gen)) {
        return w;
    }
    traceProcStatus status = default!;
    var exprᴛ1 = pp.status;
    if (exprᴛ1 == _Pidle || exprᴛ1 == _Pgcstop) {
        status = traceProcIdle;
        if (pp.status == _Pgcstop && inSTW) {
            // N.B. a P that is running and currently has the world stopped will be
            // in _Pgcstop, but we model it as running in the tracer.
            status = traceProcRunning;
        }
    }
    else if (exprᴛ1 == _Prunning) {
        status = traceProcRunning;
        if ((~w.mp).p.ptr() == Ꮡpp && (~w.mp).curg != nil && (uint32)(readgstatus((~w.mp).curg) & ~(uint32)_Gscan) == _Gsyscall) {
            // There's a short window wherein the goroutine may have entered _Gsyscall
            // but it still owns the P (it's not in _Psyscall yet). The goroutine entering
            // _Gsyscall is the tracer's signal that the P its bound to is also in a syscall,
            // so we need to emit a status that matches. See #64318.
            status = traceProcSyscall;
        }
    }
    else if (exprᴛ1 == _Psyscall) {
        status = traceProcSyscall;
    }
    else { /* default: */
        @throw(attemptToTraceInvalidOrˢ);
    }

    w = w.writeProcStatus((uint64)pp.id, status, pp.trace.inSweep);
    return w;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string attemptedToTraceABadˢ2 = "attempted to trace a bad status for a proc"u8;

// writeProcStatus emits a ProcStatus event with all the provided information.
//
// The caller must have taken ownership of a P's status writing, and the P must be
// prevented from transitioning.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static traceWriter writeProcStatus(this traceWriter w, uint64 pid, traceProcStatus status, bool inSweep) {
    // The status should never be bad. Some invariant must have been violated.
    if (status == traceProcBad) {
        print((@string)"runtime: pid="u8, pid, (@string)"\n"u8);
        @throw(attemptedToTraceABadˢ2);
    }
    // Trace the status.
    w = w.@event(traceEvProcStatus, ((traceArg)pid), ((traceArg)(uint64)(uint8)status));
    // Trace any special ranges that are in-progress.
    if (inSweep) {
        w = w.@event(traceEvGCSweepActive, ((traceArg)pid));
    }
    return w;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string triedToTraceDeadˢ = "tried to trace dead goroutine"u8;
internal static readonly @string triedToTraceGoroutineˢ = "tried to trace goroutine with invalid or unsupported status"u8;

// goStatusToTraceGoStatus translates the internal status to tracGoStatus.
//
// status must not be _Gdead or any status whose name has the suffix "_unused."
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static traceGoStatus goStatusToTraceGoStatus(uint32 status, waitReason wr) {
    // N.B. Ignore the _Gscan bit. We don't model it in the tracer.
    traceGoStatus tgs = default!;
    var exprᴛ1 = (uint32)(status & ~(uint32)_Gscan);
    if (exprᴛ1 == _Grunnable) {
        tgs = traceGoRunnable;
    }
    else if (exprᴛ1 == _Grunning || exprᴛ1 == _Gcopystack) {
        tgs = traceGoRunning;
    }
    else if (exprᴛ1 == _Gsyscall) {
        tgs = traceGoSyscall;
    }
    else if (exprᴛ1 == _Gwaiting || exprᴛ1 == _Gpreempted) {
        tgs = traceGoWaiting;
        if (status == _Gwaiting && wr.isWaitingForSuspendG()) {
            // There are a number of cases where a G might end up in
            // _Gwaiting but it's actually running in a non-preemptive
            // state but needs to present itself as preempted to the
            // garbage collector and traceAdvance (via suspendG). In
            // these cases, we're not going to emit an event, and we
            // want these goroutines to appear in the final trace as
            // if they're running, not blocked.
            tgs = traceGoRunning;
        }
    }
    else if (exprᴛ1 == _Gdead) {
        @throw(triedToTraceDeadˢ);
    }
    else { /* default: */
        @throw(triedToTraceGoroutineˢ);
    }

    return tgs;
}

// traceSchedResourceState is shared state for scheduling resources (i.e. fields common to
// both Gs and Ps).
partial struct traceSchedResourceState {
    // statusTraced indicates whether a status event was traced for this resource
    // a particular generation.
    //
    // There are 3 of these because when transitioning across generations, traceAdvance
    // needs to be able to reliably observe whether a status was traced for the previous
    // generation, while we need to clear the value for the next generation.
    internal array<atomic.Uint32> statusTraced = new(3);
    // seq is the sequence counter for this scheduling resource's events.
    // The purpose of the sequence counter is to establish a partial order between
    // events that don't obviously happen serially (same M) in the stream ofevents.
    //
    // There are two of these so that we can reset the counter on each generation.
    // This saves space in the resulting trace by keeping the counter small and allows
    // GoStatus and GoCreate events to omit a sequence number (implicitly 0).
    internal array<uint64> seq = new(2);
}

// acquireStatus acquires the right to emit a Status event for the scheduling resource.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static bool acquireStatus(this ref traceSchedResourceState r, uintptr gen) {
    if (!Ꮡ(r.statusTraced, gen % 3).CompareAndSwap(0, 1)) {
        return false;
    }
    r.readyNextGen(gen);
    return true;
}

// readyNextGen readies r for the generation following gen.
internal static void readyNextGen(this ref traceSchedResourceState r, uintptr gen) {
    var nextGen = traceNextGen(gen);
    r.seq[nextGen % 2] = 0;
    Ꮡ(r.statusTraced, nextGen % 3).Store(0);
}

// statusWasTraced returns true if the sched resource's status was already acquired for tracing.
internal static bool statusWasTraced(this ref traceSchedResourceState r, uintptr gen) {
    return Ꮡ(r.statusTraced, gen % 3).Load() != 0;
}

// setStatusTraced indicates that the resource's status was already traced, for example
// when a goroutine is created.
internal static void setStatusTraced(this ref traceSchedResourceState r, uintptr gen) {
    Ꮡ(r.statusTraced, gen % 3).Store(1);
}

// nextSeq returns the next sequence number for the resource.
internal static traceArg nextSeq(this ref traceSchedResourceState r, uintptr gen) {
    r.seq[gen % 2]++;
    return ((traceArg)r.seq[gen % 2]);
}

} // end runtime_package
