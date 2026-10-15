// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Trace goroutine and P status management.
namespace go;

using atomic = @internal.runtime.atomic_package;
using tracev2 = @internal.trace.tracev2_package;
using @internal.runtime;
using @internal.trace;

partial class runtime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string attemptedToTraceABadˢ = "attempted to trace a bad status for a goroutine"u8;

// writeGoStatus emits a GoStatus event as well as any active ranges on the goroutine.
//
// nosplit because it's part of writing an event for an M, which must not
// have any stack growth.
//
//go:nosplit
internal static traceWriter writeGoStatus(this traceWriter w, uint64 goid, int64 mid, tracev2.GoStatus status, bool markAssist, uint64 stackID) {
    // The status should never be bad. Some invariant must have been violated.
    if (status == tracev2.GoBad) {
        print((@string)"runtime: goid="u8, goid, (@string)"\n"u8);
        @throw(attemptedToTraceABadˢ);
    }

    // Trace the status.
    if (stackID == 0){
        w = w.@event(tracev2.EvGoStatus, ((traceArg)goid), ((traceArg)(uint64)mid), ((traceArg)(uint64)(uint8)status));
    } else {
        w = w.@event(tracev2.EvGoStatusStack, ((traceArg)goid), ((traceArg)(uint64)mid), ((traceArg)(uint64)(uint8)status), ((traceArg)stackID));
    }

    // Trace any special ranges that are in-progress.
    if (markAssist) {
        w = w.@event(tracev2.EvGCMarkAssistActive, ((traceArg)goid));
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
    tracev2.ProcStatus status = default!;
    var exprᴛ1 = pp.status;
    if (exprᴛ1 == _Pidle || exprᴛ1 == _Pgcstop) {
        status = tracev2.ProcIdle;
        if (pp.status == _Pgcstop && inSTW) {
            // N.B. a P that is running and currently has the world stopped will be
            // in _Pgcstop, but we model it as running in the tracer.
            status = tracev2.ProcRunning;
        }
    }
    else if (exprᴛ1 == _Prunning) {
        status = tracev2.ProcRunning;
        // A P is considered to be in a syscall if its attached G is. Since we fully
        // own P, then the goroutine isn't going to transition and we can trivially
        // check if the goroutine is in a syscall. This used to be just a small problematic
        // window, but this is now the default since _Psyscall no longer exists. See #64318
        // for the history on why it was needed while _Psyscall still existed.
        if ((~w.mp).p.ptr() == Ꮡpp && (~w.mp).curg != nil && (uint32)(readgstatus((~w.mp).curg) & ~(uint32)_Gscan) == _Gsyscall) {
            status = tracev2.ProcSyscall;
        }
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
internal static traceWriter writeProcStatus(this traceWriter w, uint64 pid, tracev2.ProcStatus status, bool inSweep) {
    // The status should never be bad. Some invariant must have been violated.
    if (status == tracev2.ProcBad) {
        print((@string)"runtime: pid="u8, pid, (@string)"\n"u8);
        @throw(attemptedToTraceABadˢ2);
    }

    // Trace the status.
    w = w.@event(tracev2.EvProcStatus, ((traceArg)pid), ((traceArg)(uint64)(uint8)status));

    // Trace any special ranges that are in-progress.
    if (inSweep) {
        w = w.@event(tracev2.EvGCSweepActive, ((traceArg)pid));
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
internal static tracev2.GoStatus goStatusToTraceGoStatus(uint32 status, waitReason wr) {
    // N.B. Ignore the _Gscan bit. We don't model it in the tracer.
    tracev2.GoStatus tgs = default!;
    var exprᴛ1 = (uint32)(status & ~(uint32)_Gscan);
    if (exprᴛ1 == _Grunnable) {
        tgs = tracev2.GoRunnable;
    }
    else if (exprᴛ1 == _Grunning || exprᴛ1 == _Gcopystack) {
        tgs = tracev2.GoRunning;
    }
    else if (exprᴛ1 == _Gsyscall) {
        tgs = tracev2.GoSyscall;
    }
    else if (exprᴛ1 == _Gwaiting || exprᴛ1 == _Gpreempted || exprᴛ1 == _Gleaked) {
        // There are a number of cases where a G might end up in
        // _Gwaiting but it's actually running in a non-preemptive
        // state but needs to present itself as preempted to the
        // garbage collector and traceAdvance (via suspendG). In
        // these cases, we're not going to emit an event, and we
        // want these goroutines to appear in the final trace as
        // if they're running, not blocked.
        tgs = tracev2.GoWaiting;
        if (status == _Gwaiting && wr.isWaitingForSuspendG()) {
            tgs = tracev2.GoRunning;
        }
    }
    else if (exprᴛ1 == _Gdead || exprᴛ1 == _Gdeadextra) {
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
