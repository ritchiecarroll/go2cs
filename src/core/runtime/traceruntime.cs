// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Runtime -> tracer API.
namespace go;

using atomic = @internal.runtime.atomic_package;
using tracev2 = @internal.trace.tracev2_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using @internal.runtime;
using @internal.trace;

partial class runtime_package {

// gTraceState is per-G state for the tracer.
partial struct gTraceState {
    internal partial ref traceSchedResourceState traceSchedResourceState { get; }
}

// reset resets the gTraceState for a new goroutine.
internal static void reset(this ref gTraceState s) {
    s.seq = new uint64[]{}.array(2);
    // N.B. s.statusTraced is managed and cleared separately.
}

// mTraceState is per-M state for the tracer.
partial struct mTraceState {
    internal atomic.Bool writing;                          // flag indicating that this M is writing to a trace buffer.
    internal array<array<ж<traceBuf>>> buf = new(2, () => new(2)); // Per-M traceBuf for writing. Indexed by trace.gen%2.
    internal ж<m> link;                                // Snapshot of alllink or freelink.
    internal uint32 reentered;                               // Whether we've reentered tracing from within tracing.
    internal uintptr entryGen;                              // The generation value on first entry.
    internal bool oldthrowsplit;                                 // gp.throwsplit upon calling traceLocker.writer. For debugging.
}

// pTraceState is per-P state for the tracer.
partial struct pTraceState {
    internal partial ref traceSchedResourceState traceSchedResourceState { get; }
    // mSyscallID is the ID of the M this was bound to before entering a syscall.
    internal int64 mSyscallID;
    // maySweep indicates the sweep events should be traced.
    // This is used to defer the sweep start event until a span
    // has actually been swept.
    internal bool maySweep;
    // inSweep indicates that at least one sweep event has been traced.
    internal bool inSweep;
    // swept and reclaimed track the number of bytes swept and reclaimed
    // by sweeping in the current sweep loop (while maySweep was true).
    internal uintptr swept, reclaimed;
}

// traceLockInit initializes global trace locks.
internal static void traceLockInit() {
    // Sharing a lock rank here is fine because they should never be accessed
    // together. If they are, we want to find out immediately.
    lockInit(ref Δtrace.stringTab[0].@lock, lockRankTraceStrings);
    lockInit(ref Δtrace.stringTab[0].tab.mem.@lock, lockRankTraceStrings);
    lockInit(ref Δtrace.stringTab[1].@lock, lockRankTraceStrings);
    lockInit(ref Δtrace.stringTab[1].tab.mem.@lock, lockRankTraceStrings);
    lockInit(ref Δtrace.stackTab[0].tab.mem.@lock, lockRankTraceStackTab);
    lockInit(ref Δtrace.stackTab[1].tab.mem.@lock, lockRankTraceStackTab);
    lockInit(ref Δtrace.typeTab[0].tab.mem.@lock, lockRankTraceTypeTab);
    lockInit(ref Δtrace.typeTab[1].tab.mem.@lock, lockRankTraceTypeTab);
    lockInit(ref Δtrace.@lock, lockRankTrace);
}

// lockRankMayTraceFlush records the lock ranking effects of a
// potential call to traceFlush.
//
// nosplit because traceAcquire is nosplit.
//
//go:nosplit
internal static void lockRankMayTraceFlush() {
    lockWithRankMayAcquire(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock), getLockRank(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock)));
}

// traceBlockReason is an enumeration of reasons a goroutine might block.
// This is the interface the rest of the runtime uses to tell the
// tracer why a goroutine blocked. The tracer then propagates this information
// into the trace however it sees fit.
//
// Note that traceBlockReasons should not be compared, since reasons that are
// distinct by name may *not* be distinct by value.
partial struct traceBlockReason /*num:uint8*/;

internal static traceBlockReason traceBlockGeneric => /* iota */ 0;
internal static traceBlockReason traceBlockForever => 1;
internal static traceBlockReason traceBlockNet => 2;
internal static traceBlockReason traceBlockSelect => 3;
internal static traceBlockReason traceBlockCondWait => 4;
internal static traceBlockReason traceBlockSync => 5;
internal static traceBlockReason traceBlockChanSend => 6;
internal static traceBlockReason traceBlockChanRecv => 7;
internal static traceBlockReason traceBlockGCMarkAssist => 8;
internal static traceBlockReason traceBlockGCSweep => 9;
internal static traceBlockReason traceBlockSystemGoroutine => 10;
internal static traceBlockReason traceBlockPreempted => 11;
internal static traceBlockReason traceBlockDebugCall => 12;
internal static traceBlockReason traceBlockUntilGCEnds => 13;
internal static traceBlockReason traceBlockSleep => 14;
internal static traceBlockReason traceBlockGCWeakToStrongWait => 15;
internal static traceBlockReason traceBlockSynctest => 16;

internal static array<@string> traceBlockReasonStrings = new golib.SparseArray<@string>{
    [traceBlockGeneric] = "unspecified"u8,
    [traceBlockForever] = "forever"u8,
    [traceBlockNet] = "network"u8,
    [traceBlockSelect] = "select"u8,
    [traceBlockCondWait] = "sync.(*Cond).Wait"u8,
    [traceBlockSync] = "sync"u8,
    [traceBlockChanSend] = "chan send"u8,
    [traceBlockChanRecv] = "chan receive"u8,
    [traceBlockGCMarkAssist] = "GC mark assist wait for work"u8,
    [traceBlockGCSweep] = "GC background sweeper wait"u8,
    [traceBlockSystemGoroutine] = "system goroutine wait"u8,
    [traceBlockPreempted] = "preempted"u8,
    [traceBlockDebugCall] = "wait for debug call"u8,
    [traceBlockUntilGCEnds] = "wait until GC ends"u8,
    [traceBlockSleep] = "sleep"u8,
    [traceBlockGCWeakToStrongWait] = "GC weak to strong wait"u8,
    [traceBlockSynctest] = "synctest"u8
}.array();

// traceGoStopReason is an enumeration of reasons a goroutine might yield.
//
// Note that traceGoStopReasons should not be compared, since reasons that are
// distinct by name may *not* be distinct by value.
partial struct traceGoStopReason /*num:uint8*/;

internal static traceGoStopReason traceGoStopGeneric => /* iota */ 0;
internal static traceGoStopReason traceGoStopGoSched => 1;
internal static traceGoStopReason traceGoStopPreempted => 2;

internal static array<@string> traceGoStopReasonStrings = new golib.SparseArray<@string>{
    [traceGoStopGeneric] = "unspecified"u8,
    [traceGoStopGoSched] = "runtime.Gosched"u8,
    [traceGoStopPreempted] = "preempted"u8
}.array();

// traceEnabled returns true if the trace is currently enabled.
//
//go:nosplit
internal static bool traceEnabled() {
    return Δtrace.enabled;
}

// traceAllocFreeEnabled returns true if the trace is currently enabled
// and alloc/free events are also enabled.
//
//go:nosplit
internal static bool traceAllocFreeEnabled() {
    return Δtrace.enabledWithAllocFree;
}

// traceShuttingDown returns true if the trace is currently shutting down.
internal static bool traceShuttingDown() {
    return ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡshutdown).Load();
}

// traceLocker represents an M writing trace events. While a traceLocker value
// is valid, the tracer observes all operations on the G/M/P or trace events being
// written as happening atomically.
partial struct traceLocker {
    internal ж<m> mp;
    internal uintptr gen;
}

// debugTraceReentrancy checks if the trace is reentrant.
//
// This is optional because throwing in a function makes it instantly
// not inlineable, and we want traceAcquire to be inlineable for
// low overhead when the trace is disabled.
internal const bool debugTraceReentrancy = false;

// traceAcquire prepares this M for writing one or more trace events.
//
// nosplit because it's called on the syscall path when stack movement is forbidden.
//
//go:nosplit
internal static traceLocker traceAcquire() {
    if (!traceEnabled()) {
        return new traceLocker(nil);
    }
    return traceAcquireEnabled();
}

// traceAcquireEnabled is the traceEnabled path for traceAcquire. It's explicitly
// broken out to make traceAcquire inlineable to keep the overhead of the tracer
// when it's disabled low.
//
// nosplit because it's called by traceAcquire, which is nosplit.
//
//go:nosplit
internal static traceLocker traceAcquireEnabled() {
    // Any time we acquire a traceLocker, we may flush a trace buffer. But
    // buffer flushes are rare. Record the lock edge even if it doesn't happen
    // this time.
    lockRankMayTraceFlush();

    // Prevent preemption.
    var mp = acquirem();

    // Check if we're already tracing. It's safe to be reentrant in general,
    // because this function (and the invariants of traceLocker.writer) ensure
    // that it is.
    if (mp.of(m.Ꮡtrace).of(mTraceState.Ꮡwriting).Load()) {
        mp.Value.trace.reentered++;
        return new traceLocker(mp, (~mp).trace.entryGen);
    }

    // Set the write flag. This prevents traceAdvance from moving forward
    // until all Ms are observed to be outside of a write critical section.
    //
    // Note: The write flag is mutated here and also in traceCPUSample. If you update
    // usage of the write flag here, make sure to also look at what traceCPUSample is
    // doing.
    mp.of(m.Ꮡtrace).of(mTraceState.Ꮡwriting).Store(true);

    // N.B. This load of gen appears redundant with the one in traceEnabled.
    // However, it's very important that the gen we use for writing to the trace
    // is acquired under a traceLocker so traceAdvance can make sure no stale
    // gen values are being used.
    //
    // Because we're doing this load again, it also means that the trace
    // might end up being disabled when we load it. In that case we need to undo
    // what we did and bail.
    var gen = ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡgen).Load();
    if (gen == 0) {
        mp.of(m.Ꮡtrace).of(mTraceState.Ꮡwriting).Store(false);
        releasem(ref (mp).DerefOrNull());
        return new traceLocker(nil);
    }
    mp.Value.trace.entryGen = gen;
    return new traceLocker(mp, gen);
}

// ok returns true if the traceLocker is valid (i.e. tracing is enabled).
//
// nosplit because it's called on the syscall path when stack movement is forbidden.
//
//go:nosplit
internal static bool ok(this traceLocker tl) {
    return tl.gen != 0;
}

// traceRelease indicates that this M is done writing trace events.
//
// nosplit because it's called on the syscall path when stack movement is forbidden.
//
//go:nosplit
internal static void traceRelease(traceLocker tl) {
    if ((~tl.mp).trace.reentered > 0){
        tl.mp.Value.trace.reentered--;
    } else {
        tl.mp.of(m.Ꮡtrace).of(mTraceState.Ꮡwriting).Store(false);
    }
    releasem(ref (tl.mp).DerefOrNull());
}

// traceExitingSyscall marks a goroutine as exiting the syscall slow path.
//
// Must be paired with a traceExitedSyscall call.
internal static void traceExitingSyscall() {
    ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑexitingSyscall).Add(1);
}

// traceExitedSyscall marks a goroutine as having exited the syscall slow path.
internal static void traceExitedSyscall() {
    ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑexitingSyscall).Add(-1);
}

// Gomaxprocs emits a ProcsChange event.
internal static void Gomaxprocs(this traceLocker tl, int32 procs) {
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvProcsChange, ((traceArg)(uint64)procs), tl.stack(1));
}

// ProcStart traces a ProcStart event.
//
// Must be called with a valid P.
internal static void ProcStart(this traceLocker tl) {
    var pp = (~tl.mp).p.ptr();
    // Procs are typically started within the scheduler when there is no user goroutine. If there is a user goroutine,
    // it must be in _Gsyscall because the only time a goroutine is allowed to have its Proc moved around from under it
    // is during a syscall.
    tl.eventWriter(tracev2.GoSyscall, tracev2.ProcIdle).@event(tracev2.EvProcStart, ((traceArg)(uint64)(~pp).id), pp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).nextSeq(tl.gen));
}

// ProcStop traces a ProcStop event.
internal static void ProcStop(this traceLocker tl, ж<Δp> Ꮡpp) {
    // The only time a goroutine is allowed to have its Proc moved around
    // from under it is during a syscall.
    tl.eventWriter(tracev2.GoSyscall, tracev2.ProcRunning).@event(tracev2.EvProcStop);
}

// GCActive traces a GCActive event.
//
// Must be emitted by an actively running goroutine on an active P. This restriction can be changed
// easily and only depends on where it's currently called.
internal static void GCActive(this traceLocker tl) {
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGCActive, ((traceArg)Δtrace.seqGC));
    // N.B. Only one GC can be running at a time, so this is naturally
    // serialized by the caller.
    Δtrace.seqGC++;
}

// GCStart traces a GCBegin event.
//
// Must be emitted by an actively running goroutine on an active P. This restriction can be changed
// easily and only depends on where it's currently called.
internal static void GCStart(this traceLocker tl) {
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGCBegin, ((traceArg)Δtrace.seqGC), tl.stack(3));
    // N.B. Only one GC can be running at a time, so this is naturally
    // serialized by the caller.
    Δtrace.seqGC++;
}

// GCDone traces a GCEnd event.
//
// Must be emitted by an actively running goroutine on an active P. This restriction can be changed
// easily and only depends on where it's currently called.
internal static void GCDone(this traceLocker tl) {
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGCEnd, ((traceArg)Δtrace.seqGC));
    // N.B. Only one GC can be running at a time, so this is naturally
    // serialized by the caller.
    Δtrace.seqGC++;
}

// STWStart traces a STWBegin event.
internal static void STWStart(this traceLocker tl, stwReason reason) {
    // Although the current P may be in _Pgcstop here, we model the P as running during the STW. This deviates from the
    // runtime's state tracking, but it's more accurate and doesn't result in any loss of information.
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvSTWBegin, tl.@string(reason.String()), tl.stack(2));
}

// STWDone traces a STWEnd event.
internal static void STWDone(this traceLocker tl) {
    // Although the current P may be in _Pgcstop here, we model the P as running during the STW. This deviates from the
    // runtime's state tracking, but it's more accurate and doesn't result in any loss of information.
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvSTWEnd);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string doubleTraceGCSweepStartˢ = "double traceGCSweepStart"u8;

// GCSweepStart prepares to trace a sweep loop. This does not
// emit any events until traceGCSweepSpan is called.
//
// GCSweepStart must be paired with traceGCSweepDone and there
// must be no preemption points between these two calls.
//
// Must be called with a valid P.
internal static void GCSweepStart(this traceLocker tl) {
    // Delay the actual GCSweepBegin event until the first span
    // sweep. If we don't sweep anything, don't emit any events.
    var pp = (~tl.mp).p.ptr();
    if ((~pp).trace.maySweep) {
        @throw(doubleTraceGCSweepStartˢ);
    }
    (pp.Value.trace.maySweep, pp.Value.trace.swept, pp.Value.trace.reclaimed) = (true, 0, 0);
}

// GCSweepSpan traces the sweep of a single span. If this is
// the first span swept since traceGCSweepStart was called, this
// will emit a GCSweepBegin event.
//
// This may be called outside a traceGCSweepStart/traceGCSweepDone
// pair; however, it will not emit any trace events in this case.
//
// Must be called with a valid P.
internal static void GCSweepSpan(this traceLocker tl, uintptr bytesSwept) {
    var pp = (~tl.mp).p.ptr();
    if ((~pp).trace.maySweep) {
        if ((~pp).trace.swept == 0) {
            tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGCSweepBegin, tl.stack(1));
            pp.Value.trace.inSweep = true;
        }
        pp.Value.trace.swept += bytesSwept;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingTraceGCSweepStartˢ = "missing traceGCSweepStart"u8;

// GCSweepDone finishes tracing a sweep loop. If any memory was
// swept (i.e. traceGCSweepSpan emitted an event) then this will emit
// a GCSweepEnd event.
//
// Must be called with a valid P.
internal static void GCSweepDone(this traceLocker tl) {
    var pp = (~tl.mp).p.ptr();
    if (!(~pp).trace.maySweep) {
        @throw(missingTraceGCSweepStartˢ);
    }
    if ((~pp).trace.inSweep) {
        tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGCSweepEnd, ((traceArg)(uint64)(~pp).trace.swept), ((traceArg)(uint64)(~pp).trace.reclaimed));
        pp.Value.trace.inSweep = false;
    }
    pp.Value.trace.maySweep = false;
}

// GCMarkAssistStart emits a MarkAssistBegin event.
internal static void GCMarkAssistStart(this traceLocker tl) {
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGCMarkAssistBegin, tl.stack(1));
}

// GCMarkAssistDone emits a MarkAssistEnd event.
internal static void GCMarkAssistDone(this traceLocker tl) {
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGCMarkAssistEnd);
}

// GoCreate emits a GoCreate event.
internal static void GoCreate(this traceLocker tl, ж<g> Ꮡnewg, uintptr pc, bool blocked) {
    ref var newg = ref Ꮡnewg.DerefOrNull();

    Ꮡnewg.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).setStatusTraced(tl.gen);
    var ev = tracev2.EvGoCreate;
    if (blocked) {
        ev = tracev2.EvGoCreateBlocked;
    }
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(ev, ((traceArg)newg.goid), tl.startPC(pc), tl.stack(2));
}

// GoStart emits a GoStart event.
//
// Must be called with a valid P.
internal static void GoStart(this traceLocker tl) {
    var gp = getg().Value.m.Value.curg;
    var pp = gp.Value.m.Value.p;
    var w = tl.eventWriter(tracev2.GoRunnable, tracev2.ProcRunning);
    w.@event(tracev2.EvGoStart, ((traceArg)(~gp).goid), gp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).nextSeq(tl.gen));
    if ((~pp.ptr()).gcMarkWorkerMode != gcMarkWorkerNotWorker) {
        w.@event(tracev2.EvGoLabel, Δtrace.markWorkerLabels[tl.gen % 2][(~pp.ptr()).gcMarkWorkerMode]);
    }
}

// GoEnd emits a GoDestroy event.
//
// TODO(mknyszek): Rename this to GoDestroy.
internal static void GoEnd(this traceLocker tl) {
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGoDestroy);
}

// GoSched emits a GoStop event with a GoSched reason.
internal static void GoSched(this traceLocker tl) {
    tl.GoStop(traceGoStopGoSched);
}

// GoPreempt emits a GoStop event with a GoPreempted reason.
internal static void GoPreempt(this traceLocker tl) {
    tl.GoStop(traceGoStopPreempted);
}

// GoStop emits a GoStop event with the provided reason.
internal static void GoStop(this traceLocker tl, traceGoStopReason reason) {
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGoStop, Δtrace.goStopReasons[tl.gen % 2][reason], tl.stack(0));
}

// GoPark emits a GoBlock event with the provided reason.
//
// TODO(mknyszek): Replace traceBlockReason with waitReason. It's silly
// that we have both, and waitReason is way more descriptive.
internal static void GoPark(this traceLocker tl, traceBlockReason reason, nint skip) {
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGoBlock, Δtrace.goBlockReasons[tl.gen % 2][reason], tl.stack(skip));
}

// GoUnpark emits a GoUnblock event.
internal static void GoUnpark(this traceLocker tl, ж<g> Ꮡgp, nint skip) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Emit a GoWaiting status if necessary for the unblocked goroutine.
    tl.emitUnblockStatus(Ꮡgp, tl.gen);
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGoUnblock, ((traceArg)gp.goid), Ꮡgp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).nextSeq(tl.gen), tl.stack(skip));
}

// GoSwitch emits a GoSwitch event. If destroy is true, the calling goroutine
// is simultaneously being destroyed.
internal static void GoSwitch(this traceLocker tl, ж<g> Ꮡnextg, bool destroy) {
    ref var nextg = ref Ꮡnextg.DerefOrNull();

    // Emit a GoWaiting status if necessary for the unblocked goroutine.
    tl.emitUnblockStatus(Ꮡnextg, tl.gen);
    var w = tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning);
    var ev = tracev2.EvGoSwitch;
    if (destroy) {
        ev = tracev2.EvGoSwitchDestroy;
    }
    w.@event(ev, ((traceArg)nextg.goid), Ꮡnextg.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).nextSeq(tl.gen));
}

// emitUnblockStatus emits a GoStatus GoWaiting event for a goroutine about to be
// unblocked to the trace writer.
internal static void emitUnblockStatus(this traceLocker tl, ж<g> Ꮡgp, uintptr gen) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (!Ꮡgp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).statusWasTraced(gen) && Ꮡgp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).acquireStatus(gen)) {
        // TODO(go.dev/issue/65634): Although it would be nice to add a stack trace here of gp,
        // we cannot safely do so. gp is in _Gwaiting and so we don't have ownership of its stack.
        // We can fix this by acquiring the goroutine's scan bit.
        tl.writer().writeGoStatus(gp.goid, -1, tracev2.GoWaiting, gp.inMarkAssist, 0).end();
    }
}

// GoSysCall emits a GoSyscallBegin event.
//
// Must be called with a valid P.
internal static void GoSysCall(this traceLocker tl) {
    // Scribble down the M that the P is currently attached to.
    var pp = (~tl.mp).p.ptr();
    pp.Value.trace.mSyscallID = (int64)(~tl.mp).procid;
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvGoSyscallBegin, pp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).nextSeq(tl.gen), tl.stack(1));
}

// GoSysExit emits a GoSyscallEnd event, possibly along with a GoSyscallBlocked event
// if lostP is true.
//
// lostP must be true in all cases that a goroutine loses its P during a syscall.
// This means it's not sufficient to check if it has no P. In particular, it needs to be
// true in the following cases:
// - The goroutine lost its P, it ran some other code, and then got it back. It's now running with that P.
// - The goroutine lost its P and was unable to reacquire it, and is now running without a P.
// - The goroutine lost its P and acquired a different one, and is now running with that P.
internal static void GoSysExit(this traceLocker tl, bool lostP) {
    var ev = tracev2.EvGoSyscallEnd;
    var procStatus = tracev2.ProcSyscall; // Procs implicitly enter tracev2.ProcSyscall on GoSyscallBegin.
    if (lostP){
        ev = tracev2.EvGoSyscallEndBlocked;
        procStatus = tracev2.ProcRunning; // If a G has a P when emitting this event, it reacquired a P and is indeed running.
    } else {
        (~tl.mp).p.ptr().Value.trace.mSyscallID = -1;
    }
    tl.eventWriter(tracev2.GoSyscall, procStatus).@event(ev);
}

// ProcSteal indicates that our current M stole a P from another M.
//
// The caller must have ownership of pp.
internal static void ProcSteal(this traceLocker tl, ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    // Grab the M ID we stole from.
    var mStolenFrom = pp.trace.mSyscallID;
    pp.trace.mSyscallID = -1;

    // Emit the status of the P we're stealing. We may be just about to do this when creating the event
    // writer but it's not guaranteed, even if we're stealing from a syscall. Although it might seem like
    // from a syscall context we're always stealing a P for ourselves, we may have not wired it up yet (so
    // it wouldn't be visible to eventWriter) or we may not even intend to wire it up to ourselves
    // at all and plan to hand it back to the runtime.
    if (!Ꮡpp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).statusWasTraced(tl.gen) && Ꮡpp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).acquireStatus(tl.gen)) {
        // Careful: don't use the event writer. We never want status or in-progress events
        // to trigger more in-progress events.
        tl.writer().writeProcStatus((uint64)pp.id, tracev2.ProcSyscallAbandoned, pp.trace.inSweep).end();
    }

    // The status of the proc and goroutine, if we need to emit one here, is not evident from the
    // context of just emitting this event alone. There are two cases. Either we're trying to steal
    // the P just to get its attention (e.g. STW or sysmon retake) or we're trying to steal a P for
    // ourselves specifically to keep running. The two contexts look different, but can be summarized
    // fairly succinctly. In the former, we're a regular running goroutine and proc, if we have either.
    // In the latter, we're a goroutine in a syscall.
    var goStatus = tracev2.GoRunning;
    var procStatus = tracev2.ProcRunning;
    if ((~tl.mp).curg != nil && (~(~tl.mp).curg).syscallsp != 0) {
        goStatus = tracev2.GoSyscall;
        procStatus = tracev2.ProcSyscallAbandoned;
    }
    tl.eventWriter(goStatus, procStatus).@event(tracev2.EvProcSteal, ((traceArg)(uint64)pp.id), Ꮡpp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).nextSeq(tl.gen), ((traceArg)(uint64)mStolenFrom));
}

// HeapAlloc emits a HeapAlloc event.
internal static void HeapAlloc(this traceLocker tl, uint64 live) {
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvHeapAlloc, ((traceArg)live));
}

// HeapGoal reads the current heap goal and emits a HeapGoal event.
internal static void HeapGoal(this traceLocker tl) {
    var heapGoal = ᏑgcController.heapGoal();
    // The heapGoal calculations will result in strange numbers if the GC if off. See go.dev/issue/63864.
    // Check gcPercent before using the heapGoal in the trace.
    if (heapGoal == ~(uint64)0 || ᏑgcController.of(gcControllerState.ᏑgcPercent).Load() < 0) {
        // Heap-based triggering is disabled.
        heapGoal = 0;
    }
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvHeapGoal, ((traceArg)heapGoal));
}

// GoCreateSyscall indicates that a goroutine has transitioned from dead to GoSyscall.
//
// Unlike GoCreate, the caller must be running on gp.
//
// This occurs when C code calls into Go. On pthread platforms it occurs only when
// a C thread calls into Go code for the first time.
internal static void GoCreateSyscall(this traceLocker tl, ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // N.B. We should never trace a status for this goroutine (which we're currently running on),
    // since we want this to appear like goroutine creation.
    Ꮡgp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).setStatusTraced(tl.gen);

    // We might have a P left over on the thread from the last cgo callback,
    // but in a syscall context, it is NOT ours. Act as if we do not have a P,
    // and don't record a status.
    tl.rawEventWriter().@event(tracev2.EvGoCreateSyscall, ((traceArg)gp.goid));
}

// GoDestroySyscall indicates that a goroutine has transitioned from GoSyscall to dead.
//
// This occurs when Go code returns back to C. On pthread platforms it occurs only when
// the C thread is destroyed.
internal static void GoDestroySyscall(this traceLocker tl) {
    // Write the status for the goroutine if necessary.
    {
        var gp = tl.mp.Value.curg; if (gp != nil && !gp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).statusWasTraced(tl.gen) && gp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).acquireStatus(tl.gen)) {
            tl.writer().writeGoStatus((~gp).goid, (int64)(~tl.mp).procid, tracev2.GoSyscall, false, 0 /* no stack */).end();
        }
    }

    // We might have a P left over on the thread from the last cgo callback,
    // but in a syscall context, it is NOT ours. Act as if we do not have a P,
    // and don't record a status.
    tl.rawEventWriter().@event(tracev2.EvGoDestroySyscall);
}

// To access runtime functions from runtime/trace.
// See runtime/trace/annotation.go

// trace_userTaskCreate emits a UserTaskCreate event.
//
//go:linkname trace_userTaskCreate runtime/trace.userTaskCreate
internal static void trace_userTaskCreate(uint64 id, uint64 parentID, @string taskType) {
    var tl = traceAcquire();
    if (!tl.ok()) {
        // Need to do this check because the caller won't have it.
        return;
    }
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvUserTaskBegin, ((traceArg)id), ((traceArg)parentID), tl.@string(taskType), tl.stack(3));
    traceRelease(tl);
}

// trace_userTaskEnd emits a UserTaskEnd event.
//
//go:linkname trace_userTaskEnd runtime/trace.userTaskEnd
internal static void trace_userTaskEnd(uint64 id) {
    var tl = traceAcquire();
    if (!tl.ok()) {
        // Need to do this check because the caller won't have it.
        return;
    }
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvUserTaskEnd, ((traceArg)id), tl.stack(2));
    traceRelease(tl);
}

// trace_userRegion emits a UserRegionBegin or UserRegionEnd event,
// depending on mode (0 == Begin, 1 == End).
//
// TODO(mknyszek): Just make this two functions.
//
//go:linkname trace_userRegion runtime/trace.userRegion
internal static void trace_userRegion(uint64 id, uint64 mode, @string name) {
    var tl = traceAcquire();
    if (!tl.ok()) {
        // Need to do this check because the caller won't have it.
        return;
    }
    tracev2.EventType ev = default!;
    switch (mode) {
    case 0: {
        ev = tracev2.EvUserRegionBegin;
        break;
    }
    case 1: {
        ev = tracev2.EvUserRegionEnd;
        break;
    }
    default: {
        return;
    }}
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(ev, ((traceArg)id), tl.@string(name), tl.stack(3));
    traceRelease(tl);
}

// trace_userLog emits a UserRegionBegin or UserRegionEnd event.
//
//go:linkname trace_userLog runtime/trace.userLog
internal static void trace_userLog(uint64 id, @string category, @string message) {
    var tl = traceAcquire();
    if (!tl.ok()) {
        // Need to do this check because the caller won't have it.
        return;
    }
    tl.eventWriter(tracev2.GoRunning, tracev2.ProcRunning).@event(tracev2.EvUserLog, ((traceArg)id), tl.@string(category), tl.uniqueString(message), tl.stack(3));
    traceRelease(tl);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badUseOfTraceWritingˢ = "bad use of trace.writing"u8;

// traceThreadDestroy is called when a thread is removed from
// sched.freem.
//
// mp must not be able to emit trace events anymore.
//
// sched.lock must be held to synchronize with traceAdvance.
internal static void traceThreadDestroy(ж<m> Ꮡmp) {
    assertLockHeld(Ꮡsched.of(schedt.Ꮡlock));

    // Flush all outstanding buffers to maintain the invariant
    // that an M only has active buffers while on sched.freem
    // or allm.
    //
    // Perform a traceAcquire/traceRelease on behalf of mp to
    // synchronize with the tracer trying to flush our buffer
    // as well.
    if (debugTraceReentrancy && Ꮡmp.of(m.Ꮡtrace).of(mTraceState.Ꮡwriting).Load()) {
        @throw(badUseOfTraceWritingˢ);
    }
    Ꮡmp.of(m.Ꮡtrace).of(mTraceState.Ꮡwriting).Store(true);
    systemstack(() => {
        @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
        foreach (var (i, _) in Ꮡmp.Value.trace.buf) {
            foreach (var (exp, buf) in Ꮡmp.Value.trace.buf[i].ΔRangeSnapshot()) {
                if (buf != nil) {
                    // N.B. traceBufFlush accepts a generation, but it
                    // really just cares about gen%2.
                    traceBufFlush(buf, (uintptr)i);
                    Ꮡmp.Value.trace.buf[i][exp] = default!;
                }
            }
        }
        unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
    });
    Ꮡmp.of(m.Ꮡtrace).of(mTraceState.Ꮡwriting).Store(false);
}

} // end runtime_package
