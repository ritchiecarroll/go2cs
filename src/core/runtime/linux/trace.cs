// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Go execution tracer.
// The tracer captures a wide range of execution events like goroutine
// creation/blocking/unblocking, syscall enter/exit/block, GC-related events,
// changes of heap size, processor start/stop, etc and writes them to a buffer
// in a compact form. A precise nanosecond-precision timestamp and a stack
// trace is captured for most events.
//
// Tracer invariants (to keep the synchronization making sense):
// - An m that has a trace buffer must be on either the allm or sched.freem lists.
// - Any trace buffer mutation must either be happening in traceAdvance or between
//   a traceAcquire and a subsequent traceRelease.
// - traceAdvance cannot return until the previous generation's buffers are all flushed.
//
// See https://go.dev/issue/60773 for a link to the full design.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// Trace state.

// trace is global tracing context.

[GoType("dyn")] partial struct Δtraceᴛ1 {
    // trace.lock must only be acquired on the system stack where
    // stack splits cannot happen while it is held.
    internal mutex @lock;
    // Trace buffer management.
    //
    // First we check the empty list for any free buffers. If not, buffers
    // are allocated directly from the OS. Once they're filled up and/or
    // flushed, they end up on the full queue for trace.gen%2.
    //
    // The trace reader takes buffers off the full list one-by-one and
    // places them into reading until they're finished being read from.
    // Then they're placed onto the empty list.
    //
    // Protected by trace.lock.
    internal ж<traceBuf> reading; // buffer currently handed off to user
    internal ж<traceBuf> empty; // stack of empty buffers
    internal array<traceBufQueue> full = new(2);
    internal atomic.Bool workAvailable;
    // State for the trace reader goroutine.
    //
    // Protected by trace.lock.
    internal atomic.Uintptr readerGen; // the generation the reader is currently reading for
    internal atomic.Uintptr flushedGen; // the last completed generation
    internal bool headerWritten;           // whether ReadTrace has emitted trace header
    // doneSema is used to synchronize the reader and traceAdvance. Specifically,
    // it notifies traceAdvance that the reader is done with a generation.
    // Both semaphores are 0 by default (so, acquires block). traceAdvance
    // attempts to acquire for gen%2 after flushing the last buffers for gen.
    // Meanwhile the reader releases the sema for gen%2 when it has finished
    // processing gen.
    internal array<uint32> doneSema = new(2);
    // Trace data tables for deduplicating data going into the trace.
    // There are 2 of each: one for gen%2, one for 1-gen%2.
    internal array<traceStackTable> stackTab = new(2, () => new()); // maps stack traces to unique ids
    internal array<traceStringTable> stringTab = new(2, () => new()); // maps strings to unique ids
    internal array<traceTypeTable> typeTab = new(2, () => new()); // maps type pointers to unique ids
    // cpuLogRead accepts CPU profile samples from the signal handler where
    // they're generated. There are two profBufs here: one for gen%2, one for
    // 1-gen%2. These profBufs use a three-word header to hold the IDs of the P, G,
    // and M (respectively) that were active at the time of the sample. Because
    // profBuf uses a record with all zeros in its header to indicate overflow,
    // we make sure to make the P field always non-zero: The ID of a real P will
    // start at bit 1, and bit 0 will be set. Samples that arrive while no P is
    // running (such as near syscalls) will set the first header field to 0b10.
    // This careful handling of the first header field allows us to store ID of
    // the active G directly in the second field, even though that will be 0
    // when sampling g0.
    //
    // Initialization and teardown of these fields is protected by traceAdvanceSema.
    internal array<ж<profBuf>> cpuLogRead = new(2);
    internal atomic.Uint32 signalLock;              // protects use of the following member, only usable in signal handlers
    internal array<atomic.Pointer<profBuf>> cpuLogWrite = new(2); // copy of cpuLogRead for use in signal handlers, set without signalLock
    internal ж<wakeableSleep> cpuSleep;
    internal /*<-*/channel<EmptyStruct> cpuLogDone = /*<-*/channel<EmptyStruct>.RecvOnly;
    internal array<ж<traceBuf>> cpuBuf = new(2);
    internal atomic.Pointer<g> reader; // goroutine that called ReadTrace, or nil
    // Fast mappings from enumerations to string IDs that are prepopulated
    // in the trace.
    internal array<array<traceArg>> markWorkerLabels = new(2, () => new(4));
    internal array<array<traceArg>> goStopReasons = new(2, () => new(3));
    internal array<array<traceArg>> goBlockReasons = new(2, () => new(17));
    // enabled indicates whether tracing is enabled, but it is only an optimization,
    // NOT the source of truth on whether tracing is enabled. Tracing is only truly
    // enabled if gen != 0. This is used as an optimistic fast path check.
    //
    // Transitioning this value from true -> false is easy (once gen is 0)
    // because it's OK for enabled to have a stale "true" value. traceAcquire will
    // always double-check gen.
    //
    // Transitioning this value from false -> true is harder. We need to make sure
    // this is observable as true strictly before gen != 0. To maintain this invariant
    // we only make this transition with the world stopped and use the store to gen
    // as a publication barrier.
    internal bool enabled;
    // enabledWithAllocFree is set if debug.traceallocfree is != 0 when tracing begins.
    // It follows the same synchronization protocol as enabled.
    internal bool enabledWithAllocFree;
    // Trace generation counter.
    internal atomic.Uintptr gen;
    internal uintptr lastNonZeroGen; // last non-zero value of gen
    // shutdown is set when we are waiting for trace reader to finish after setting gen to 0
    //
    // Writes protected by trace.lock.
    internal atomic.Bool shutdown;
    // Number of goroutines in syscall exiting slow path.
    internal atomic.Int32 exitingSyscall;
    // seqGC is the sequence counter for GC begin/end.
    //
    // Mutated only during stop-the-world.
    internal uint64 seqGC;
    // minPageHeapAddr is the minimum address of the page heap when tracing started.
    internal uint64 minPageHeapAddr;
    // debugMalloc is the value of debug.malloc before tracing began.
    internal bool debugMalloc;
}
internal static ж<Δtraceᴛ1> ᏑΔtrace = new StandardBox<Δtraceᴛ1>(new Δtraceᴛ1());
internal static ref Δtraceᴛ1 Δtrace => ref ᏑΔtrace.Value;

// Trace public API.
internal static ж<uint32> ᏑtraceAdvanceSema = new StandardBox<uint32>(1);
internal static ref uint32 traceAdvanceSema => ref ᏑtraceAdvanceSema.Value;
internal static ж<uint32> ᏑtraceShutdownSema = new StandardBox<uint32>(1);
internal static ref uint32 traceShutdownSema => ref ᏑtraceShutdownSema.Value;

// go2cs generated this placeholder — func StartTrace is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func StopTrace is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string traceNonEmptyFullTraceˢ = "trace: non-empty full trace buffer for done generation"u8;
internal static readonly @string traceNonEmptyFullTraceˢ2 = "trace: non-empty full trace buffer for next generation"u8;
internal static readonly @string traceReadingAfterˢ = "trace: reading after shutdown"u8;

// Collect all the untraced Gs.
[GoType("dyn")] internal partial struct traceAdvance_untracedG {
    internal ж<g> gp;
    internal uint64 goid;
    internal int64 mid;
    internal uint64 stackID;
    internal uint32 status;
    internal waitReason waitreason;
    internal bool inMarkAssist;
}

// traceAdvance moves tracing to the next generation, and cleans up the current generation,
// ensuring that it's flushed out before returning. If stopTrace is true, it disables tracing
// altogether instead of advancing to the next generation.
//
// traceAdvanceSema must not be held.
//
// traceAdvance is called by golang.org/x/exp/trace using linkname.
//
//go:linkname traceAdvance
internal static void traceAdvance(bool stopTrace) {
    semacquire(ᏑtraceAdvanceSema);
    // Get the gen that we're advancing from. In this function we don't really care much
    // about the generation we're advancing _into_ since we'll do all the cleanup in this
    // generation for the next advancement.
    var gen = ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡgen).Load();
    if (gen == 0) {
        // We may end up here traceAdvance is called concurrently with StopTrace.
        semrelease(ᏑtraceAdvanceSema);
        return;
    }
    // Write an EvFrequency event for this generation.
    //
    // N.B. This may block for quite a while to get a good frequency estimate, so make sure we do
    // this here and not e.g. on the trace reader.
    traceFrequency(gen);
    ref var untracedGs = ref heap<slice<traceAdvance_untracedG>>(out var ᏑuntracedGs);
    forEachGRace((ж<g> gp) => {
        // Make absolutely sure all Gs are ready for the next
        // generation. We need to do this even for dead Gs because
        // they may come alive with a new identity, and its status
        // traced bookkeeping might end up being stale.
        // We may miss totally new goroutines, but they'll always
        // have clean bookkeeping.
        gp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).readyNextGen(gen);
        // If the status was traced, nothing else to do.
        if (gp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).statusWasTraced(gen)) {
            return;
        }
        // Scribble down information about this goroutine.
        ref var ug = ref heap<traceAdvance_untracedG>(out var Ꮡug);
        Ꮡug.Value = new traceAdvance_untracedG(gp: gp, mid: -1);
        systemstack(() => {
            var me = getg().Value.m.Value.curg;
            // We don't have to handle this G status transition because we
            // already eliminated ourselves from consideration above.
            casGToWaitingForSuspendG(me, _Grunning, waitReasonTraceGoroutineStatus);
            // We need to suspend and take ownership of the G to safely read its
            // goid. Note that we can't actually emit the event at this point
            // because we might stop the G in a window where it's unsafe to write
            // events based on the G's status. We need the global trace buffer flush
            // coming up to make sure we're not racing with the G.
            //
            // It should be very unlikely that we try to preempt a running G here.
            // The only situation that we might is that we're racing with a G
            // that's running for the first time in this generation. Therefore,
            // this should be relatively fast.
            var s = suspendG(gp);
            if (!s.dead) {
                Ꮡug.Value.goid = s.g.Value.goid;
                if ((~s.g).m != nil) {
                    Ꮡug.Value.mid = (int64)(~(~s.g).m).procid;
                }
                Ꮡug.Value.status = (uint32)(readgstatus(s.g) & ~(uint32)_Gscan);
                Ꮡug.Value.waitreason = s.g.Value.waitreason;
                Ꮡug.Value.inMarkAssist = s.g.Value.inMarkAssist;
                Ꮡug.Value.stackID = traceStack(0, gp, gen);
            }
            resumeG(s);
            casgstatus(me, _Gwaiting, _Grunning);
        });
        if (Ꮡug.Value.goid != 0) {
            ᏑuntracedGs.ValueSlot = append(ᏑuntracedGs.ValueSlot, Ꮡug.Value);
        }
    });
    if (!stopTrace) {
        // Re-register runtime goroutine labels and stop/block reasons.
        traceRegisterLabelsAndReasons(traceNextGen(gen));
    }
    // Now that we've done some of the heavy stuff, prevent the world from stopping.
    // This is necessary to ensure the consistency of the STW events. If we're feeling
    // adventurous we could lift this restriction and add a STWActive event, but the
    // cost of maintaining this consistency is low. We're not going to hold this semaphore
    // for very long and most STW periods are very short.
    // Once we hold worldsema, prevent preemption as well so we're not interrupted partway
    // through this. We want to get this done as soon as possible.
    semacquire(Ꮡworldsema);
    var mp = acquirem();
    // Advance the generation or stop the trace.
    Δtrace.lastNonZeroGen = gen;
    if (stopTrace){
        systemstack(() => {
            // Ordering is important here. Set shutdown first, then disable tracing,
            // so that conditions like (traceEnabled() || traceShuttingDown()) have
            // no opportunity to be false. Hold the trace lock so this update appears
            // atomic to the trace reader.
            @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
            ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡshutdown).Store(true);
            ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡgen).Store(0);
            unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
            // Clear trace.enabled. It is totally OK for this value to be stale,
            // because traceAcquire will always double-check gen.
            Δtrace.enabled = false;
        });
    } else {
        ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡgen).Store(traceNextGen(gen));
    }
    // Emit a ProcsChange event so we have one on record for each generation.
    // Let's emit it as soon as possible so that downstream tools can rely on the value
    // being there fairly soon in a generation.
    //
    // It's important that we do this before allowing stop-the-worlds again,
    // because the procs count could change.
    if (!stopTrace) {
        var tl = traceAcquire();
        tl.Gomaxprocs(gomaxprocs);
        traceRelease(tl);
    }
    // Emit a GCActive event in the new generation if necessary.
    //
    // It's important that we do this before allowing stop-the-worlds again,
    // because that could emit global GC-related events.
    if (!stopTrace && (gcphase == _GCmark || gcphase == _GCmarktermination)) {
        var tl = traceAcquire();
        tl.GCActive();
        traceRelease(tl);
    }
    // Preemption is OK again after this. If the world stops or whatever it's fine.
    // We're just cleaning up the last generation after this point.
    //
    // We also don't care if the GC starts again after this for the same reasons.
    releasem(ref (mp).DerefOrNull());
    semrelease(Ꮡworldsema);
    // Snapshot allm and freem.
    //
    // Snapshotting after the generation counter update is sufficient.
    // Because an m must be on either allm or sched.freem if it has an active trace
    // buffer, new threads added to allm after this point must necessarily observe
    // the new generation number (sched.lock acts as a barrier).
    //
    // Threads that exit before this point and are on neither list explicitly
    // flush their own buffers in traceThreadDestroy.
    //
    // Snapshotting freem is necessary because Ms can continue to emit events
    // while they're still on that list. Removal from sched.freem is serialized with
    // this snapshot, so either we'll capture an m on sched.freem and race with
    // the removal to flush its buffers (resolved by traceThreadDestroy acquiring
    // the thread's seqlock, which one of us must win, so at least its old gen buffer
    // will be flushed in time for the new generation) or it will have flushed its
    // buffers before we snapshotted it to begin with.
    @lock(Ꮡsched.of(schedt.Ꮡlock));
    ref var mToFlush = ref heap<ж<m>>(out var ᏑmToFlush);
    mToFlush = allm;
    for (var mpΔ1 = mToFlush; mpΔ1 != nil; mpΔ1 = mpΔ1.Value.alllink) {
        mpΔ1.Value.trace.link = mpΔ1.Value.alllink;
    }
    for (var mpΔ2 = sched.freem; mpΔ2 != nil; mpΔ2 = mpΔ2.Value.freelink) {
        mpΔ2.Value.trace.link = mToFlush;
        mToFlush = mpΔ2;
    }
    unlock(Ꮡsched.of(schedt.Ꮡlock));
    // Iterate over our snapshot, flushing every buffer until we're done.
    //
    // Because trace writers read the generation while the seqlock is
    // held, we can be certain that when there are no writers there are
    // also no stale generation values left. Therefore, it's safe to flush
    // any buffers that remain in that generation's slot.
    const bool debugDeadlock = false;
    systemstack(() => {
        // Track iterations for some rudimentary deadlock detection.
        nint i = 0;
        var detectedDeadlock = false;
        while (ᏑmToFlush.ValueSlot != nil) {
            var prev = ᏑmToFlush;
            for (var mpΔ3 = prev.ValueSlot; mpΔ3 != nil; ) {
                if (mpΔ3.of(m.Ꮡtrace).of(mTraceState.Ꮡseqlock).Load() % 2 != 0) {
                    // The M is writing. Come back to it later.
                    prev = mpΔ3.of(m.Ꮡtrace).of(mTraceState.Ꮡlink);
                    mpΔ3 = mpΔ3.Value.trace.link;
                    continue;
                }
                // Flush the trace buffer.
                //
                // trace.lock needed for traceBufFlush, but also to synchronize
                // with traceThreadDestroy, which flushes both buffers unconditionally.
                @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
                foreach (var (exp, buf) in (~mpΔ3).trace.buf[gen % 2].ΔRangeSnapshot()) {
                    if (buf != nil) {
                        traceBufFlush(buf, gen);
                        (~mpΔ3).trace.buf[gen % 2][exp] = default!;
                    }
                }
                unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
                // Remove the m from the flush list.
                prev.ValueSlot = mpΔ3.Value.trace.link;
                mpΔ3.Value.trace.link = default!;
                mpΔ3 = prev.ValueSlot;
            }
            // Yield only if we're going to be going around the loop again.
            if (ᏑmToFlush.ValueSlot != nil) {
                osyield();
            }
            if (debugDeadlock) {
                // Try to detect a deadlock. We probably shouldn't loop here
                // this many times.
                if (i > 100000 && !detectedDeadlock) {
                    detectedDeadlock = true;
                    println((@string)"runtime: failing to flush"u8);
                    for (var mpΔ4 = ᏑmToFlush.ValueSlot; mpΔ4 != nil; mpΔ4 = mpΔ4.Value.trace.link) {
                        print((@string)"runtime: m="u8, (~mpΔ4).id, (@string)"\n"u8);
                    }
                }
                i++;
            }
        }
    });
    // At this point, the old generation is fully flushed minus stack and string
    // tables, CPU samples, and goroutines that haven't run at all during the last
    // generation.
    // Check to see if any Gs still haven't had events written out for them.
    var statusWriter = unsafeTraceWriter(gen, nil);
    foreach (var (_, ug) in untracedGs) {
        if (ug.gp.of(g.Ꮡtrace).of(gTraceState.ᏑtraceSchedResourceState).statusWasTraced(gen)) {
            // It was traced, we don't need to do anything.
            continue;
        }
        // It still wasn't traced. Because we ensured all Ms stopped writing trace
        // events to the last generation, that must mean the G never had its status
        // traced in gen between when we recorded it and now. If that's true, the goid
        // and status we recorded then is exactly what we want right now.
        var status = goStatusToTraceGoStatus(ug.status, ug.waitreason);
        statusWriter = statusWriter.writeGoStatus(ug.goid, ug.mid, status, ug.inMarkAssist, ug.stackID);
    }
    statusWriter.flush().end();
    // Read everything out of the last gen's CPU profile buffer.
    traceReadCPU(gen);
    // Flush CPU samples, stacks, and strings for the last generation. This is safe,
    // because we're now certain no M is writing to the last generation.
    //
    // Ordering is important here. traceCPUFlush may generate new stacks and dumping
    // stacks may generate new strings.
    traceCPUFlush(gen);
    ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstackTab, (ulong)(gen % 2)).dump(gen);
    ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑtypeTab, (ulong)(gen % 2)).dump(gen);
    ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstringTab, (ulong)(gen % 2)).reset(gen);
    // That's it. This generation is done producing buffers.
    systemstack(() => {
        @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
        ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑflushedGen).Store(gen);
        unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
    });
    // Perform status reset on dead Ps because they just appear as idle.
    //
    // Preventing preemption is sufficient to access allp safely. allp is only
    // mutated by GOMAXPROCS calls, which require a STW.
    //
    // TODO(mknyszek): Consider explicitly emitting ProcCreate and ProcDestroy
    // events to indicate whether a P exists, rather than just making its
    // existence implicit.
    mp = acquirem();
    foreach (var (_, pp) in allp.slice(len(allp), cap(allp))) {
        pp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).readyNextGen(traceNextGen(gen));
    }
    releasem(ref (mp).DerefOrNull());
    if (stopTrace){
        // Acquire the shutdown sema to begin the shutdown process.
        semacquire(ᏑtraceShutdownSema);
        // Finish off CPU profile reading.
        traceStopReadCPU();
        // Reset debug.malloc if necessary. Note that this is set in a racy
        // way; that's OK. Some mallocs may still enter into the debug.malloc
        // block, but they won't generate events because tracing is disabled.
        // That is, it's OK if mallocs read a stale debug.malloc or
        // trace.enabledWithAllocFree value.
        if (Δtrace.enabledWithAllocFree) {
            Δtrace.enabledWithAllocFree = false;
            debug.malloc = Δtrace.debugMalloc;
        }
    } else {
        // Go over each P and emit a status event for it if necessary.
        //
        // We do this at the beginning of the new generation instead of the
        // end like we do for goroutines because forEachP doesn't give us a
        // hook to skip Ps that have already been traced. Since we have to
        // preempt all Ps anyway, might as well stay consistent with StartTrace
        // which does this during the STW.
        semacquire(Ꮡworldsema);
        forEachP(waitReasonTraceProcStatus, (ж<Δp> pp) => {
            var tl = traceAcquire();
            if (!pp.of(runtime_package.Δp.Ꮡtrace).of(pTraceState.ᏑtraceSchedResourceState).statusWasTraced(tl.gen)) {
                tl.writer().writeProcStatusForP(pp, false).end();
            }
            traceRelease(tl);
        });
        semrelease(Ꮡworldsema);
    }
    // Block until the trace reader has finished processing the last generation.
    semacquire(ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑdoneSema, (ulong)(gen % 2)));
    if (raceenabled) {
        raceacquire(@unsafe.Pointer.FromPinnedBox(ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑdoneSema, (ulong)(gen % 2))));
    }
    // Double-check that things look as we expect after advancing and perform some
    // final cleanup if the trace has fully stopped.
    systemstack(() => {
        @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
        if (!Δtrace.full[gen % 2].empty()) {
            @throw(traceNonEmptyFullTraceˢ);
        }
        if (stopTrace) {
            if (!Δtrace.full[1 - (gen % 2)].empty()) {
                @throw(traceNonEmptyFullTraceˢ2);
            }
            if (Δtrace.reading != nil || ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡreader).Load() != nil) {
                @throw(traceReadingAfterˢ);
            }
            // Free all the empty buffers.
            while (Δtrace.empty != nil) {
                var buf = Δtrace.empty;
                Δtrace.empty = buf.Value.link;
                sysFree(@unsafe.Pointer.FromPinnedBox(buf), /* unsafe.Sizeof(*buf) */ (uintptr)65536, Ꮡmemstats.of(mstats.Ꮡother_sys));
            }
            // Clear trace.shutdown and other flags.
            Δtrace.headerWritten = false;
            ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡshutdown).Store(false);
        }
        unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
    });
    if (stopTrace) {
        // Clear the sweep state on every P for the next time tracing is enabled.
        //
        // It may be stale in the next trace because we may have ended tracing in
        // the middle of a sweep on a P.
        //
        // It's fine not to call forEachP here because tracing is disabled and we
        // know at this point that nothing is calling into the tracer, but we do
        // need to look at dead Ps too just because GOMAXPROCS could have been called
        // at any point since we stopped tracing, and we have to ensure there's no
        // bad state on dead Ps too. Prevent a STW and a concurrent GOMAXPROCS that
        // might mutate allp by making ourselves briefly non-preemptible.
        var mpΔ5 = acquirem();
        foreach (var (_, pp) in allp.slice(0, cap(allp))) {
            pp.Value.trace.inSweep = false;
            pp.Value.trace.maySweep = false;
            pp.Value.trace.swept = 0;
            pp.Value.trace.reclaimed = 0;
        }
        releasem(ref (mpΔ5).DerefOrNull());
    }
    // Release the advance semaphore. If stopTrace is true we're still holding onto
    // traceShutdownSema.
    //
    // Do a direct handoff. Don't let one caller of traceAdvance starve
    // other calls to traceAdvance.
    semrelease1(ᏑtraceAdvanceSema, true, 0);
    if (stopTrace) {
        // Stop the traceAdvancer. We can't be holding traceAdvanceSema here because
        // we'll deadlock (we're blocked on the advancer goroutine exiting, but it
        // may be currently trying to acquire traceAdvanceSema).
        traceAdvancer.stop();
        semrelease(ᏑtraceShutdownSema);
    }
}

internal static uintptr traceNextGen(uintptr gen) {
    if (gen == ~(uintptr)0) {
        // gen is used both %2 and %3 and we want both patterns to continue when we loop around.
        // ^uint32(0) and ^uint64(0) are both odd and multiples of 3. Therefore the next generation
        // we want is even and one more than a multiple of 3. The smallest such number is 4.
        return 4;
    }
    return gen + 1;
}

// traceRegisterLabelsAndReasons re-registers mark worker labels and
// goroutine stop/block reasons in the string table for the provided
// generation. Note: the provided generation must not have started yet.
internal static void traceRegisterLabelsAndReasons(uintptr gen) {
    foreach (var (i, label) in gcMarkWorkerModeStrings[..]) {
        Δtrace.markWorkerLabels[gen % 2][i] = ((traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstringTab, (ulong)(gen % 2)).put(gen, label));
    }
    foreach (var (i, str) in traceBlockReasonStrings[..]) {
        Δtrace.goBlockReasons[gen % 2][i] = ((traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstringTab, (ulong)(gen % 2)).put(gen, str));
    }
    foreach (var (i, str) in traceGoStopReasonStrings[..]) {
        Δtrace.goStopReasons[gen % 2][i] = ((traceArg)ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑstringTab, (ulong)(gen % 2)).put(gen, str));
    }
}

// go2cs generated this placeholder — func ReadTrace is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectedRacectx0ˢ = "expected racectx == 0"u8;

// readTrace0 is ReadTrace's continuation on g0. This must run on the
// system stack because it acquires trace.lock.
//
//go:systemstack
internal static (slice<byte> buf, bool park) readTrace0() {
    slice<byte> buf = default!;
    bool park = default!;
    GoFrame ᒐ = default;
    try {
        if (raceenabled) {
            // g0 doesn't have a race context. Borrow the user G's.
            if ((~getg()).racectx != 0) {
                @throw(expectedRacectx0ˢ);
            }
            getg().Value.racectx = getg().Value.m.Value.curg.Value.racectx;
            // (This defer should get open-coded, which is safe on
            // the system stack.)
            defer(() => {
                getg().Value.racectx = 0;
            }, ref ᒐ);
        }
        // This function must not allocate while holding trace.lock:
        // allocation can call heap allocate, which will try to emit a trace
        // event while holding heap lock.
        @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
        if (ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡreader).Load() != nil) {
            // More than one goroutine reads trace. This is bad.
            // But we rather do not crash the program because of tracing,
            // because tracing can be enabled at runtime on prod servers.
            unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
            println((@string)"runtime: ReadTrace called from multiple goroutines simultaneously"u8);
            (buf, park) = (default!, false); goto ᒐdone;
        }
        // Recycle the old buffer.
        {
            var bufΔ1 = Δtrace.reading; if (bufΔ1 != nil) {
                bufΔ1.Value.link = Δtrace.empty;
                Δtrace.empty = bufΔ1;
                Δtrace.reading = default!;
            }
        }
        // Write trace header.
        if (!Δtrace.headerWritten) {
            Δtrace.headerWritten = true;
            unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
            (buf, park) = (slice<byte>("go 1.23 trace\x00\x00\x00"u8), false); goto ᒐdone;
        }
        // Read the next buffer.
        if (ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑreaderGen).Load() == 0) {
            ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑreaderGen).Store(1);
        }
        uintptr gen = default!;
        while (ᐧ) {
            assertLockHeld(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
            gen = ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑreaderGen).Load();
            // Check to see if we need to block for more data in this generation
            // or if we need to move our generation forward.
            if (!Δtrace.full[gen % 2].empty()) {
                break;
            }
            // Most of the time readerGen is one generation ahead of flushedGen, as the
            // current generation is being read from. Then, once the last buffer is flushed
            // into readerGen, flushedGen will rise to meet it. At this point, the tracer
            // is waiting on the reader to finish flushing the last generation so that it
            // can continue to advance.
            if (ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑflushedGen).Load() == gen) {
                if (ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡshutdown).Load()) {
                    unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
                    // Wake up anyone waiting for us to be done with this generation.
                    //
                    // Do this after reading trace.shutdown, because the thread we're
                    // waking up is going to clear trace.shutdown.
                    if (raceenabled) {
                        // Model synchronization on trace.doneSema, which te race
                        // detector does not see. This is required to avoid false
                        // race reports on writer passed to trace.Start.
                        racerelease(@unsafe.Pointer.FromPinnedBox(ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑdoneSema, (ulong)(gen % 2))));
                    }
                    semrelease(ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑdoneSema, (ulong)(gen % 2)));
                    // We're shutting down, and the last generation is fully
                    // read. We're done.
                    (buf, park) = (default!, false); goto ᒐdone;
                }
                // The previous gen has had all of its buffers flushed, and
                // there's nothing else for us to read. Advance the generation
                // we're reading from and try again.
                ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑreaderGen).Store(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡgen).Load());
                unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
                // Wake up anyone waiting for us to be done with this generation.
                //
                // Do this after reading gen to make sure we can't have the trace
                // advance until we've read it.
                if (raceenabled) {
                    // See comment above in the shutdown case.
                    racerelease(@unsafe.Pointer.FromPinnedBox(ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑdoneSema, (ulong)(gen % 2))));
                }
                semrelease(ᏑΔtrace.at(runtime_package.Δtraceᴛ1.ᏑdoneSema, (ulong)(gen % 2)));
                // Reacquire the lock and go back to the top of the loop.
                @lock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
                continue;
            }
            // Wait for new data.
            //
            // We don't simply use a note because the scheduler
            // executes this goroutine directly when it wakes up
            // (also a note would consume an M).
            //
            // Before we drop the lock, clear the workAvailable flag. Work can
            // only be queued with trace.lock held, so this is at least true until
            // we drop the lock.
            ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑworkAvailable).Store(false);
            unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
            (buf, park) = (default!, true); goto ᒐdone;
        }
        // Pull a buffer.
        var tbuf = Δtrace.full[gen % 2].pop();
        Δtrace.reading = tbuf;
        unlock(ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡlock));
        (buf, park) = ((~tbuf).arr.slice(0, (~tbuf).pos), false);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (buf, park);
}

// traceReader returns the trace reader that should be woken up, if any.
// Callers should first check (traceEnabled() || traceShuttingDown()).
//
// This must run on the system stack because it acquires trace.lock.
//
//go:systemstack
internal static ж<g> traceReader() {
    var gp = traceReaderAvailable();
    if (gp == nil || !ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡreader).CompareAndSwapNoWB(gp, nil)) {
        return default!;
    }
    return gp;
}

// traceReaderAvailable returns the trace reader if it is not currently
// scheduled and should be. Callers should first check that
// (traceEnabled() || traceShuttingDown()) is true.
internal static ж<g> traceReaderAvailable() {
    // There are three conditions under which we definitely want to schedule
    // the reader:
    // - The reader is lagging behind in finishing off the last generation.
    //   In this case, trace buffers could even be empty, but the trace
    //   advancer will be waiting on the reader, so we have to make sure
    //   to schedule the reader ASAP.
    // - The reader has pending work to process for it's reader generation
    //   (assuming readerGen is not lagging behind). Note that we also want
    //   to be careful *not* to schedule the reader if there's no work to do.
    // - The trace is shutting down. The trace stopper blocks on the reader
    //   to finish, much like trace advancement.
    //
    // We also want to be careful not to schedule the reader if there's no
    // reason to.
    if (ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑflushedGen).Load() == ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑreaderGen).Load() || ᏑΔtrace.of(runtime_package.Δtraceᴛ1.ᏑworkAvailable).Load() || ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡshutdown).Load()) {
        return ᏑΔtrace.of(runtime_package.Δtraceᴛ1.Ꮡreader).Load();
    }
    return default!;
}

// Trace advancer goroutine.
internal static ж<traceAdvancerState> ᏑtraceAdvancer = new StandardBox<traceAdvancerState>(default(traceAdvancerState));
internal static ref traceAdvancerState traceAdvancer => ref ᏑtraceAdvancer.Value;

[GoType] partial struct traceAdvancerState {
    internal ж<wakeableSleep> timer;
    internal channel<EmptyStruct> done;
}

// start starts a new traceAdvancer.
internal static void start(this ж<traceAdvancerState> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Start a goroutine to periodically advance the trace generation.
    s.done = new channel<EmptyStruct>(0);
    s.timer = newWakeableSleep();
    goǃ(() => {
        while (traceEnabled()) {
            // Set a timer to wake us up
            Ꮡs.Value.timer.sleep((int64)debug.traceadvanceperiod);
            // Try to advance the trace.
            traceAdvance(false);
        }
        Ꮡs.Value.done.ᐸꟷ(new EmptyStruct());
    });
}

// stop stops a traceAdvancer and blocks until it exits.
[GoRecv] internal static void stop(this ref traceAdvancerState s) {
    s.timer.wake();
    ᐸꟷ(s.done);
    builtin.close(s.done);
    s.timer.close();
}

// traceAdvancePeriod is the approximate period between
// new generations.
internal static UntypedFloat defaultTraceAdvancePeriod => 1e9; // 1 second.

// wakeableSleep manages a wakeable goroutine sleep.
//
// Users of this type must call init before first use and
// close to free up resources. Once close is called, init
// must be called before another use.
[GoType] partial struct wakeableSleep {
    internal ж<timer> timer;
    // lock protects access to wakeup, but not send/recv on it.
    internal mutex @lock;
    internal channel<EmptyStruct> wakeup;
}

// newWakeableSleep initializes a new wakeableSleep and returns it.
internal static ж<wakeableSleep> newWakeableSleep() {
    var s = @new<wakeableSleep>();
    lockInit(s.of(wakeableSleep.Ꮡlock), lockRankWakeableSleep);
    s.Value.wakeup = new channel<EmptyStruct>(1);
    s.Value.timer = @new<timer>();
    var f = (any sΔ1, uintptr _Δp1, int64 _Δp2) => {
        sΔ1._<ж<wakeableSleep>>().wake();
    };
    (~s).timer.init(f, s.OrTypedNil());
    return s;
}

// sleep sleeps for the provided duration in nanoseconds or until
// another goroutine calls wake.
//
// Must not be called by more than one goroutine at a time and
// must not be called concurrently with close.
internal static void sleep(this ж<wakeableSleep> Ꮡs, int64 ns) {
    ref var s = ref Ꮡs.DerefOrNull();

    s.timer.reset(nanotime() + ns, 0);
    @lock(Ꮡs.of(wakeableSleep.Ꮡlock));
    if (raceenabled) {
        raceacquire(@unsafe.Pointer.FromPinnedBox(Ꮡs.of(wakeableSleep.Ꮡlock)));
    }
    var wakeup = s.wakeup;
    if (raceenabled) {
        racerelease(@unsafe.Pointer.FromPinnedBox(Ꮡs.of(wakeableSleep.Ꮡlock)));
    }
    unlock(Ꮡs.of(wakeableSleep.Ꮡlock));
    ᐸꟷ(wakeup);
    s.timer.stop();
}

// wake awakens any goroutine sleeping on the timer.
//
// Safe for concurrent use with all other methods.
internal static void wake(this ж<wakeableSleep> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Grab the wakeup channel, which may be nil if we're
    // racing with close.
    @lock(Ꮡs.of(wakeableSleep.Ꮡlock));
    if (raceenabled) {
        raceacquire(@unsafe.Pointer.FromPinnedBox(Ꮡs.of(wakeableSleep.Ꮡlock)));
    }
    if (s.wakeup != default!) {
        // Non-blocking send.
        //
        // Others may also write to this channel and we don't
        // want to block on the receiver waking up. This also
        // effectively batches together wakeup notifications.
        var selᴛ4 = s.wakeup.ᐸꟷ(new EmptyStruct(), ꓸꓸꓸ);
        switch (trySelect(selᴛ4)) {
        case 0: {
            break;
        }
        default: {
            break;
        }}
    }
    if (raceenabled) {
        racerelease(@unsafe.Pointer.FromPinnedBox(Ꮡs.of(wakeableSleep.Ꮡlock)));
    }
    unlock(Ꮡs.of(wakeableSleep.Ꮡlock));
}

// close wakes any goroutine sleeping on the timer and prevents
// further sleeping on it.
//
// Once close is called, the wakeableSleep must no longer be used.
//
// It must only be called once no goroutine is sleeping on the
// timer *and* nothing else will call wake concurrently.
internal static void close(this ж<wakeableSleep> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Set wakeup to nil so that a late timer ends up being a no-op.
    @lock(Ꮡs.of(wakeableSleep.Ꮡlock));
    if (raceenabled) {
        raceacquire(@unsafe.Pointer.FromPinnedBox(Ꮡs.of(wakeableSleep.Ꮡlock)));
    }
    var wakeup = s.wakeup;
    s.wakeup = default!;
    // Close the channel.
    builtin.close(wakeup);
    if (raceenabled) {
        racerelease(@unsafe.Pointer.FromPinnedBox(Ꮡs.of(wakeableSleep.Ꮡlock)));
    }
    unlock(Ꮡs.of(wakeableSleep.Ꮡlock));
    return;
}

} // end runtime_package
