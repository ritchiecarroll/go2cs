// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Garbage collector: marking and scanning
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

internal static UntypedInt fixedRootFinalizers => iota;
internal static UntypedInt fixedRootFreeGStacks => 1;
internal static UntypedInt fixedRootCount => 2;
internal static UntypedInt rootBlockBytes => /* 256 << 10 */ 262144;
internal static UntypedInt maxObletBytes => /* 128 << 10 */ 131072;
internal static UntypedInt drainCheckThreshold => 100000;
internal static UntypedInt pagesPerSpanRoot => 512;

// gcMarkRootPrepare queues root scanning jobs (stacks, globals, and
// some miscellany) and initializes scanning-related state.
//
// The world must be stopped.
internal static void gcMarkRootPrepare() {
    assertWorldStopped();
    // Compute how many data and BSS root blocks there are.
    nint nBlocks(uintptr bytes) => (nint)divRoundUp(bytes, rootBlockBytes);
    work.nDataRoots = 0;
    work.nBSSRoots = 0;
    // Scan globals.
    foreach (var (_, datap) in activeModules()) {
        nint nDataRoots = nBlocks((~datap).edata - (~datap).data);
        if (nDataRoots > work.nDataRoots) {
            work.nDataRoots = nDataRoots;
        }
        nint nBSSRoots = nBlocks((~datap).ebss - (~datap).bss);
        if (nBSSRoots > work.nBSSRoots) {
            work.nBSSRoots = nBSSRoots;
        }
    }
    // Scan span roots for finalizer specials.
    //
    // We depend on addfinalizer to mark objects that get
    // finalizers after root marking.
    //
    // We're going to scan the whole heap (that was available at the time the
    // mark phase started, i.e. markArenas) for in-use spans which have specials.
    //
    // Break up the work into arenas, and further into chunks.
    //
    // Snapshot allArenas as markArenas. This snapshot is safe because allArenas
    // is append-only.
    mheap_.markArenas = mheap_.allArenas.slice(-1, len(mheap_.allArenas), len(mheap_.allArenas));
    work.nSpanRoots = len(mheap_.markArenas) * (nint)(pagesPerArena / pagesPerSpanRoot);
    // Scan stacks.
    //
    // Gs may be created after this point, but it's okay that we
    // ignore them because they begin life without any roots, so
    // there's nothing to scan, and any roots they create during
    // the concurrent phase will be caught by the write barrier.
    work.stackRoots = allGsSnapshot();
    work.nStackRoots = len(work.stackRoots);
    work.markrootNext = 0;
    work.markrootJobs = (uint32)((nint)fixedRootCount + work.nDataRoots + work.nBSSRoots + work.nSpanRoots + work.nStackRoots);
    // Calculate base indexes of each root type
    work.baseData = (uint32)fixedRootCount;
    work.baseBSS = work.baseData + (uint32)work.nDataRoots;
    work.baseSpans = work.baseBSS + (uint32)work.nBSSRoots;
    work.baseStacks = work.baseSpans + (uint32)work.nSpanRoots;
    work.baseEnd = work.baseStacks + (uint32)work.nStackRoots;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string leftOverMarkrootJobsˢ = "left over markroot jobs"u8;
internal static readonly @string scanMissedAGˢ = "scan missed a g"u8;

// gcMarkRootCheck checks that all roots have been scanned. It is
// purely for debugging.
internal static void gcMarkRootCheck() {
    if (work.markrootNext < work.markrootJobs) {
        print(work.markrootNext, (@string)" of "u8, work.markrootJobs, (@string)" markroot jobs done\n"u8);
        @throw(leftOverMarkrootJobsˢ);
    }
    // Check that stacks have been scanned.
    //
    // We only check the first nStackRoots Gs that we should have scanned.
    // Since we don't care about newer Gs (see comment in
    // gcMarkRootPrepare), no locking is required.
    nint i = 0;
    forEachGRace((ж<g> gp) => {
        if (i >= work.nStackRoots) {
            return;
        }
        if (!(~gp).gcscandone) {
            println((@string)"gp"u8, gp.OrTypedNil(), (@string)"goid"u8, (~gp).goid,
                (@string)"status"u8, readgstatus(gp),
                (@string)"gcscandone"u8, (~gp).gcscandone);
            @throw(scanMissedAGˢ);
        }
        i++;
    });
}

// ptrmask for an allocation containing a single pointer.
internal static ж<array<uint8>> Ꮡoneptrmask = new StandardBox<array<uint8>>(new uint8[]{1}.array());
internal static ref array<uint8> oneptrmask => ref Ꮡoneptrmask.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string markrootBadIndexˢ = "markroot: bad index"u8;
internal static readonly @string gAlreadyScannedˢ = "g already scanned"u8;

// markroot scans the i'th root.
//
// Preemption must be disabled (because this uses a gcWork).
//
// Returns the amount of GC work credit produced by the operation.
// If flushBgCredit is true, then that credit is also flushed
// to the background credit pool.
//
// nowritebarrier is only advisory here.
//
//go:nowritebarrier
internal static int64 markroot(ж<gcWork> Ꮡgcw, uint32 i, bool flushBgCredit) {
    // Note: if you add a case here, please also update heapdump.go:dumproots.
    int64 workDone = default!;
    ж<atomic.Int64> workCounter = default!;
    switch (ᐧ) {
    case {} when work.baseData <= i && i < work.baseBSS: {
        workCounter = ᏑgcController.of(gcControllerState.ᏑglobalsScanWork);
        foreach (var (_, datap) in activeModules()) {
            workDone += markrootBlock((~datap).data, (~datap).edata - (~datap).data, (~datap).gcdatamask.bytedata, Ꮡgcw, (nint)(i - work.baseData));
        }
        break;
    }
    case {} when work.baseBSS <= i && i < work.baseSpans: {
        workCounter = ᏑgcController.of(gcControllerState.ᏑglobalsScanWork);
        foreach (var (_, datap) in activeModules()) {
            workDone += markrootBlock((~datap).bss, (~datap).ebss - (~datap).bss, (~datap).gcbssmask.bytedata, Ꮡgcw, (nint)(i - work.baseBSS));
        }
        break;
    }
    case {} when i == fixedRootFinalizers: {
        for (var fb = allfin; fb != nil; fb = fb.Value.alllink) {
            var cnt = (uintptr)atomic.Load(fb.of(finblock.Ꮡcnt));
            scanblock((uintptr)fb.at(finblock.Ꮡfin, 0), cnt * /* unsafe.Sizeof(fb.fin[0]) */ (uintptr)40, Ꮡfinptrmask.at<byte>(0), Ꮡgcw, nil);
        }
        break;
    }
    case {} when i == fixedRootFreeGStacks: {
        systemstack(markrootFreeGStacks);
        break;
    }
    case {} when work.baseSpans <= i && i < work.baseStacks: {
        markrootSpans(Ꮡgcw, // Switch to the system stack so we can call
 // stackfree.
 // mark mspan.specials
 (nint)(i - work.baseSpans));
        break;
    }
    default: {
        workCounter = ᏑgcController.of(gcControllerState.ᏑstackScanWork);
        if (i < work.baseStacks || work.baseEnd <= i) {
            // the rest is scanning goroutine stacks
            printlock();
            print((@string)"runtime: markroot index "u8, i, (@string)" not in stack roots range ["u8, work.baseStacks, (@string)", "u8, work.baseEnd, (@string)")\n"u8);
            @throw(markrootBadIndexˢ);
        }
        var gp = work.stackRoots[(nint)(i - work.baseStacks)];
        var status = readgstatus(gp); // We are not in a scan state
        if ((status == _Gwaiting || status == _Gsyscall) && (~gp).waitsince == 0) {
            // remember when we've first observed the G blocked
            // needed only to output in traceback
            gp.Value.waitsince = work.tstart;
        }
        var gpʗ1 = gp;
        systemstack(() => {
            // scanstack must be done on the system stack in case
            // we're trying to scan our own stack.
            // If this is a self-scan, put the user G in
            // _Gwaiting to prevent self-deadlock. It may
            // already be in _Gwaiting if this is a mark
            // worker or we're in mark termination.
            var userG = getg().Value.m.Value.curg;
            var selfScan = gpʗ1 == userG && readgstatus(userG) == _Grunning;
            if (selfScan) {
                casGToWaitingForSuspendG(userG, _Grunning, waitReasonGarbageCollectionScan);
            }
            // TODO: suspendG blocks (and spins) until gp
            // stops, which may take a while for
            // running goroutines. Consider doing this in
            // two phases where the first is non-blocking:
            // we scan the stacks we can and ask running
            // goroutines to scan themselves; and the
            // second blocks.
            var stopped = suspendG(gpʗ1);
            if (stopped.dead) {
                gpʗ1.Value.gcscandone = true;
                return;
            }
            if ((~gpʗ1).gcscandone) {
                @throw(gAlreadyScannedˢ);
            }
            workDone += scanstack(gpʗ1, Ꮡgcw);
            gpʗ1.Value.gcscandone = true;
            resumeG(stopped);
            if (selfScan) {
                casgstatus(userG, _Gwaiting, _Grunning);
            }
        });
        break;
    }}

    if (workCounter != nil && workDone != 0) {
        workCounter.Add(workDone);
        if (flushBgCredit) {
            gcFlushBgCredit(workDone);
        }
    }
    return workDone;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootBlockBytesMustBeAˢ = "rootBlockBytes must be a multiple of 8*ptrSize"u8;

// markrootBlock scans the shard'th shard of the block of memory [b0,
// b0+n0), with the given pointer mask.
//
// Returns the amount of work done.
//
//go:nowritebarrier
internal static int64 markrootBlock(uintptr b0, uintptr n0, ж<uint8> Ꮡptrmask0, ж<gcWork> Ꮡgcw, nint shard) {
    ref var ptrmask0 = ref Ꮡptrmask0.DerefOrNull();

    if (rootBlockBytes % (8 * goarch.PtrSize) != 0) {
        // This is necessary to pick byte offsets in ptrmask0.
        @throw(rootBlockBytesMustBeAˢ);
    }
    // Note that if b0 is toward the end of the address space,
    // then b0 + rootBlockBytes might wrap around.
    // These tests are written to avoid any possible overflow.
    var off = (uintptr)shard * (uintptr)rootBlockBytes;
    if (off >= n0) {
        return 0;
    }
    var b = b0 + off;
    var ptrmask = (ж<uint8>)(uintptr)(add(new @unsafe.Pointer(Ꮡptrmask0), (uintptr)shard * (uintptr)(rootBlockBytes / (8 * goarch.PtrSize))));
    var n = (uintptr)rootBlockBytes;
    if (off + n > n0) {
        n = n0 - off;
    }
    // Scan this shard.
    scanblock(b, n, ptrmask, Ꮡgcw, nil);
    return (int64)n;
}

// markrootFreeGStacks frees stacks of dead Gs.
//
// This does not free stacks of dead Gs cached on Ps, but having a few
// cached stacks around isn't a problem.
internal static void markrootFreeGStacks() {
    // Take list of dead Gs with stacks.
    @lock(Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡlock));
    var list = sched.gFree.stack;
    sched.gFree.stack = new gList(nil);
    unlock(Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡlock));
    if (list.empty()) {
        return;
    }
    // Free stacks.
    var q = new gQueue(list.head, list.head);
    for (var gp = list.head.ptr(); gp != nil; gp = (~gp).schedlink.ptr()) {
        stackfree((~gp).stack);
        gp.Value.stack.lo = 0;
        gp.Value.stack.hi = 0;
        // Manipulate the queue directly since the Gs are
        // already all linked the right way.
        q.tail.set(gp);
    }
    // Put Gs back on the free list.
    @lock(Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡlock));
    Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.ᏑnoStack).pushAll(q);
    unlock(Ꮡsched.of(schedt.ᏑgFree).of(schedt_gFree.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonInUseSpanFoundWithˢ = "non in-use span found with specials bit set"u8;
internal static readonly @string gcUnsweptSpanˢ = "gc: unswept span"u8;

// markrootSpans marks roots for one shard of markArenas.
//
//go:nowritebarrier
internal static void markrootSpans(ж<gcWork> Ꮡgcw, nint shard) {
    // Objects with finalizers have two GC-related invariants:
    //
    // 1) Everything reachable from the object must be marked.
    // This ensures that when we pass the object to its finalizer,
    // everything the finalizer can reach will be retained.
    //
    // 2) Finalizer specials (which are not in the garbage
    // collected heap) are roots. In practice, this means the fn
    // field must be scanned.
    //
    // Objects with weak handles have only one invariant related
    // to this function: weak handle specials (which are not in the
    // garbage collected heap) are roots. In practice, this means
    // the handle field must be scanned. Note that the value the
    // handle pointer referenced does *not* need to be scanned. See
    // the definition of specialWeakHandle for details.
    var sg = mheap_.sweepgen;
    // Find the arena and page index into that arena for this shard.
    arenaIdx ai = mheap_.markArenas[shard / (nint)(pagesPerArena / pagesPerSpanRoot)];
    var ha = mheap_.arenas[(nint)(ai.l1())].Value[ai.l2()];
    nuint arenaPage = (nuint)((uintptr)shard * (uintptr)pagesPerSpanRoot % (uintptr)pagesPerArena);
    // Construct slice of bitmap which we'll iterate over.
    var specialsbits = (~ha).pageSpecials[(int)(arenaPage / 8)..];
    specialsbits = specialsbits[..(int)(pagesPerSpanRoot / 8)];
    foreach (var (i, _) in specialsbits) {
        // Find set bits, which correspond to spans with specials.
        var specials = atomic.Load8(Ꮡ(specialsbits, i));
        if (specials == 0) {
            continue;
        }
        for (nuint j = (nuint)0; j < 8; j++) {
            if ((uint8)(specials & (((uint8)1).Lsh(j))) == 0) {
                continue;
            }
            // Find the span for this bit.
            //
            // This value is guaranteed to be non-nil because having
            // specials implies that the span is in-use, and since we're
            // currently marking we can be sure that we don't have to worry
            // about the span being freed and re-used.
            var s = (~ha).spans[(nint)(arenaPage + (nuint)i * 8 + j)];
            // The state must be mSpanInUse if the specials bit is set, so
            // sanity check that.
            {
                var state = s.of(mspan.Ꮡstate).get(); if (state != mSpanInUse) {
                    print((@string)"s.state = "u8, state, (@string)"\n"u8);
                    @throw(nonInUseSpanFoundWithˢ);
                }
            }
            // Check that this span was swept (it may be cached or uncached).
            if (!useCheckmark && !((~s).sweepgen == sg || (~s).sweepgen == sg + 3)) {
                // sweepgen was updated (+2) during non-checkmark GC pass
                print((@string)"sweep "u8, (~s).sweepgen, (@string)" "u8, sg, (@string)"\n"u8);
                @throw(gcUnsweptSpanˢ);
            }
            // Lock the specials to prevent a special from being
            // removed from the list while we're traversing it.
            @lock(s.of(mspan.Ꮡspeciallock));
            for (var sp = s.Value.specials; sp != nil; sp = sp.Value.next) {
                var exprᴛ1 = (~sp).kind;
                if (exprᴛ1 == _KindSpecialFinalizer) {
                    var spf = sp.Reinterpret<special, specialfinalizer>();
                    var Δp = s.@base() + (uintptr)(~spf).special.offset / (~s).elemsize * (~s).elemsize;
                    if (!(~s).spanclass.noscan()) {
                        // don't mark finalized object, but scan it so we
                        // retain everything it points to.
                        // A finalizer can be set for an inner byte of an object, find object beginning.
                        // Mark everything that can be reached from
                        // the object (but *not* the object itself or
                        // we'll never collect it).
                        scanobject(Δp, Ꮡgcw);
                    }
                    scanblock((uintptr)@unsafe.Pointer.FromBox(spf.of(specialfinalizer.Ꮡfn)), // The special itself is a root.
 goarch.PtrSize, Ꮡoneptrmask.at<uint8>(0), Ꮡgcw, nil);
                }
                else if (exprᴛ1 == _KindSpecialWeakHandle) {
                    var spw = sp.Reinterpret<special, specialWeakHandle>();
                    scanblock((uintptr)@unsafe.Pointer.FromBox(spw.of(specialWeakHandle.Ꮡhandle)), // The special itself is a root.
 goarch.PtrSize, Ꮡoneptrmask.at<uint8>(0), Ꮡgcw, nil);
                }

            }
            unlock(s.of(mspan.Ꮡspeciallock));
        }
    }
}

// gcAssistAlloc performs GC work to make gp's assist debt positive.
// gp must be the calling user goroutine.
//
// This must be called with preemption enabled.
internal static void gcAssistAlloc(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Don't assist in non-preemptible contexts. These are
    // generally fragile and won't allow the assist to block.
    if (getg() == (~gp.m).g0) {
        return;
    }
    {
        var mp = getg().Value.m; if ((~mp).locks > 0 || (~mp).preemptoff != ""u8) {
            return;
        }
    }
    // This extremely verbose boolean indicates whether we've
    // entered mark assist from the perspective of the tracer.
    //
    // In the tracer, this is just before we call gcAssistAlloc1
    // *regardless* of whether tracing is enabled. This is because
    // the tracer allows for tracing to begin (and advance
    // generations) in the middle of a GC mark phase, so we need to
    // record some state so that the tracer can pick it up to ensure
    // a consistent trace result.
    //
    // TODO(mknyszek): Hide the details of inMarkAssist in tracer
    // functions and simplify all the state tracking. This is a lot.
    var enteredMarkAssistForTracing = false;
retry:
    if (ᏑgcCPULimiter.limiting()) {
        // If the CPU limiter is enabled, intentionally don't
        // assist to reduce the amount of CPU time spent in the GC.
        if (enteredMarkAssistForTracing) {
            var Δtrace = traceAcquire();
            if (Δtrace.ok()){
                Δtrace.GCMarkAssistDone();
                // Set this *after* we trace the end to make sure
                // that we emit an in-progress event if this is
                // the first event for the goroutine in the trace
                // or trace generation. Also, do this between
                // acquire/release because this is part of the
                // goroutine's trace state, and it must be atomic
                // with respect to the tracer.
                gp.inMarkAssist = false;
                traceRelease(Δtrace);
            } else {
                // This state is tracked even if tracing isn't enabled.
                // It's only used by the new tracer.
                // See the comment on enteredMarkAssistForTracing.
                gp.inMarkAssist = false;
            }
        }
        return;
    }
    // Compute the amount of scan work we need to do to make the
    // balance positive. When the required amount of work is low,
    // we over-assist to build up credit for future allocations
    // and amortize the cost of assisting.
    var assistWorkPerByte = ᏑgcController.of(gcControllerState.ᏑassistWorkPerByte).Load();
    var assistBytesPerWork = ᏑgcController.of(gcControllerState.ᏑassistBytesPerWork).Load();
    var debtBytes = -gp.gcAssistBytes;
    var scanWork = (int64)(assistWorkPerByte * (float64)debtBytes);
    if (scanWork < gcOverAssistWork) {
        scanWork = gcOverAssistWork;
        debtBytes = (int64)(assistBytesPerWork * (float64)scanWork);
    }
    // Steal as much credit as we can from the background GC's
    // scan credit. This is racy and may drop the background
    // credit below 0 if two mutators steal at the same time. This
    // will just cause steals to fail until credit is accumulated
    // again, so in the long run it doesn't really matter, but we
    // do have to handle the negative credit case.
    var bgScanCredit = ᏑgcController.of(gcControllerState.ᏑbgScanCredit).Load();
    var stolen = (int64)0;
    if (bgScanCredit > 0) {
        if (bgScanCredit < scanWork){
            stolen = bgScanCredit;
            gp.gcAssistBytes += 1 + (int64)(assistBytesPerWork * (float64)stolen);
        } else {
            stolen = scanWork;
            gp.gcAssistBytes += debtBytes;
        }
        ᏑgcController.of(gcControllerState.ᏑbgScanCredit).Add(-stolen);
        scanWork -= stolen;
        if (scanWork == 0) {
            // We were able to steal all of the credit we
            // needed.
            if (enteredMarkAssistForTracing) {
                var Δtrace = traceAcquire();
                if (Δtrace.ok()){
                    Δtrace.GCMarkAssistDone();
                    // Set this *after* we trace the end to make sure
                    // that we emit an in-progress event if this is
                    // the first event for the goroutine in the trace
                    // or trace generation. Also, do this between
                    // acquire/release because this is part of the
                    // goroutine's trace state, and it must be atomic
                    // with respect to the tracer.
                    gp.inMarkAssist = false;
                    traceRelease(Δtrace);
                } else {
                    // This state is tracked even if tracing isn't enabled.
                    // It's only used by the new tracer.
                    // See the comment on enteredMarkAssistForTracing.
                    gp.inMarkAssist = false;
                }
            }
            return;
        }
    }
    if (!enteredMarkAssistForTracing) {
        var Δtrace = traceAcquire();
        if (Δtrace.ok()){
            Δtrace.GCMarkAssistStart();
            // Set this *after* we trace the start, otherwise we may
            // emit an in-progress event for an assist we're about to start.
            gp.inMarkAssist = true;
            traceRelease(Δtrace);
        } else {
            gp.inMarkAssist = true;
        }
        // In the new tracer, set enter mark assist tracing if we
        // ever pass this point, because we must manage inMarkAssist
        // correctly.
        //
        // See the comment on enteredMarkAssistForTracing.
        enteredMarkAssistForTracing = true;
    }
    // Perform assist work
    systemstack(() => {
        gcAssistAlloc1(Ꮡgp, scanWork);
    });
    // The user stack may have moved, so this can't touch
    // anything on it until it returns from systemstack.
    var completed = gp.param != nil;
    gp.param = default!;
    if (completed) {
        gcMarkDone();
    }
    if (gp.gcAssistBytes < 0) {
        // We were unable steal enough credit or perform
        // enough work to pay off the assist debt. We need to
        // do one of these before letting the mutator allocate
        // more to prevent over-allocation.
        //
        // If this is because we were preempted, reschedule
        // and try some more.
        if (gp.preempt) {
            Gosched();
            goto retry;
        }
        // Add this G to an assist queue and park. When the GC
        // has more background credit, it will satisfy queued
        // assists before flushing to the global credit pool.
        //
        // Note that this does *not* get woken up when more
        // work is added to the work list. The theory is that
        // there wasn't enough work to do anyway, so we might
        // as well let background marking take care of the
        // work that is available.
        if (!gcParkAssist()) {
            goto retry;
        }
    }
    // At this point either background GC has satisfied
    // this G's assist debt, or the GC cycle is over.
    if (enteredMarkAssistForTracing) {
        var Δtrace = traceAcquire();
        if (Δtrace.ok()){
            Δtrace.GCMarkAssistDone();
            // Set this *after* we trace the end to make sure
            // that we emit an in-progress event if this is
            // the first event for the goroutine in the trace
            // or trace generation. Also, do this between
            // acquire/release because this is part of the
            // goroutine's trace state, and it must be atomic
            // with respect to the tracer.
            gp.inMarkAssist = false;
            traceRelease(Δtrace);
        } else {
            // This state is tracked even if tracing isn't enabled.
            // It's only used by the new tracer.
            // See the comment on enteredMarkAssistForTracing.
            gp.inMarkAssist = false;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nwaitWorkNprocsˢ = "nwait > work.nprocs"u8;

// gcAssistAlloc1 is the part of gcAssistAlloc that runs on the system
// stack. This is a separate function to make it easier to see that
// we're not capturing anything from the user stack, since the user
// stack may move while we're in this function.
//
// gcAssistAlloc1 indicates whether this assist completed the mark
// phase by setting gp.param to non-nil. This can't be communicated on
// the stack since it may move.
//
//go:systemstack
internal static void gcAssistAlloc1(ж<g> Ꮡgp, int64 scanWork) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Clear the flag indicating that this assist completed the
    // mark phase.
    gp.param = default!;
    if (atomic.Load(ᏑgcBlackenEnabled) == 0) {
        // The gcBlackenEnabled check in malloc races with the
        // store that clears it but an atomic check in every malloc
        // would be a performance hit.
        // Instead we recheck it here on the non-preemptible system
        // stack to determine if we should perform an assist.
        // GC is done, so ignore any remaining debt.
        gp.gcAssistBytes = 0;
        return;
    }
    // Track time spent in this assist. Since we're on the
    // system stack, this is non-preemptible, so we can
    // just measure start and end time.
    //
    // Limiter event tracking might be disabled if we end up here
    // while on a mark worker.
    var startTime = nanotime();
    var trackLimiterEvent = (~gp.m).p.ptr().of(runtime_package.Δp.ᏑlimiterEvent).start(limiterEventMarkAssist, startTime);
    var decnwait = atomic.Xadd(Ꮡwork.of(workType.Ꮡnwait), -1);
    if (decnwait == work.nproc) {
        println((@string)"runtime: work.nwait ="u8, decnwait, (@string)"work.nproc="u8, work.nproc);
        @throw(nwaitWorkNprocsˢ);
    }
    // gcDrainN requires the caller to be preemptible.
    casGToWaitingForSuspendG(Ꮡgp, _Grunning, waitReasonGCAssistMarking);
    // drain own cached work first in the hopes that it
    // will be more cache friendly.
    var gcw = (~(~getg()).m).p.ptr().of(runtime_package.Δp.Ꮡgcw);
    var workDone = gcDrainN(gcw, scanWork);
    casgstatus(Ꮡgp, _Gwaiting, _Grunning);
    // Record that we did this much scan work.
    //
    // Back out the number of bytes of assist credit that
    // this scan work counts for. The "1+" is a poor man's
    // round-up, to ensure this adds credit even if
    // assistBytesPerWork is very low.
    var assistBytesPerWork = ᏑgcController.of(gcControllerState.ᏑassistBytesPerWork).Load();
    gp.gcAssistBytes += 1 + (int64)(assistBytesPerWork * (float64)workDone);
    // If this is the last worker and we ran out of work,
    // signal a completion point.
    var incnwait = atomic.Xadd(Ꮡwork.of(workType.Ꮡnwait), +1);
    if (incnwait > work.nproc) {
        println((@string)"runtime: work.nwait="u8, incnwait,
            (@string)"work.nproc="u8, work.nproc);
        @throw(workNwaitWorkNprocˢ);
    }
    if (incnwait == work.nproc && !gcMarkWorkAvailable(nil)) {
        // This has reached a background completion point. Set
        // gp.param to a non-nil value to indicate this. It
        // doesn't matter what we set it to (it just has to be
        // a valid pointer).
        gp.param = new @unsafe.Pointer(Ꮡgp);
    }
    var now = nanotime();
    var duration = now - startTime;
    var pp = (~gp.m).p.ptr();
    pp.Value.gcAssistTime += duration;
    if (trackLimiterEvent) {
        pp.of(runtime_package.Δp.ᏑlimiterEvent).stop(limiterEventMarkAssist, now);
    }
    if ((~pp).gcAssistTime > gcAssistTimeSlack) {
        ᏑgcController.of(gcControllerState.ᏑassistTime).Add((~pp).gcAssistTime);
        ᏑgcCPULimiter.update(now);
        pp.Value.gcAssistTime = 0;
    }
}

// gcWakeAllAssists wakes all currently blocked assists. This is used
// at the end of a GC cycle. gcBlackenEnabled must be false to prevent
// new assists from going to sleep after this point.
internal static void gcWakeAllAssists() {
    @lock(Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡlock));
    ref var list = ref heap<gList>(out var Ꮡlist);
    list = Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡq).popList();
    injectglist(Ꮡlist);
    unlock(Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡlock));
}

// gcParkAssist puts the current goroutine on the assist queue and parks.
//
// gcParkAssist reports whether the assist is now satisfied. If it
// returns false, the caller must retry the assist.
internal static bool gcParkAssist() {
    @lock(Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡlock));
    // If the GC cycle finished while we were getting the lock,
    // exit the assist. The cycle can't finish while we hold the
    // lock.
    if (atomic.Load(ᏑgcBlackenEnabled) == 0) {
        unlock(Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡlock));
        return true;
    }
    var gp = getg();
    var oldList = work.assistQueue.q;
    Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡq).pushBack(gp);
    // Recheck for background credit now that this G is in
    // the queue, but can still back out. This avoids a
    // race in case background marking has flushed more
    // credit since we checked above.
    if (ᏑgcController.of(gcControllerState.ᏑbgScanCredit).Load() > 0) {
        work.assistQueue.q = oldList;
        if (oldList.tail != 0) {
            oldList.tail.ptr().of(g.Ꮡschedlink).set(nil);
        }
        unlock(Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡlock));
        return false;
    }
    // Park.
    goparkunlock(Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡlock), waitReasonGCAssistWait, traceBlockGCMarkAssist, 2);
    return true;
}

// gcFlushBgCredit flushes scanWork units of background scan work
// credit. This first satisfies blocked assists on the
// work.assistQueue and then flushes any remaining credit to
// gcController.bgScanCredit.
//
// Write barriers are disallowed because this is used by gcDrain after
// it has ensured that all work is drained and this must preserve that
// condition.
//
//go:nowritebarrierrec
internal static void gcFlushBgCredit(int64 scanWork) {
    if (Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡq).empty()) {
        // Fast path; there are no blocked assists. There's a
        // small window here where an assist may add itself to
        // the blocked queue and park. If that happens, we'll
        // just get it on the next flush.
        ᏑgcController.of(gcControllerState.ᏑbgScanCredit).Add(scanWork);
        return;
    }
    var assistBytesPerWork = ᏑgcController.of(gcControllerState.ᏑassistBytesPerWork).Load();
    var scanBytes = (int64)((float64)scanWork * assistBytesPerWork);
    @lock(Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡlock));
    while (!Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡq).empty() && scanBytes > 0) {
        var gp = Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡq).pop();
        // Note that gp.gcAssistBytes is negative because gp
        // is in debt. Think carefully about the signs below.
        if (scanBytes + (~gp).gcAssistBytes >= 0){
            // Satisfy this entire assist debt.
            scanBytes += gp.Value.gcAssistBytes;
            gp.Value.gcAssistBytes = 0;
            // It's important that we *not* put gp in
            // runnext. Otherwise, it's possible for user
            // code to exploit the GC worker's high
            // scheduler priority to get itself always run
            // before other goroutines and always in the
            // fresh quantum started by GC.
            ready(gp, 0, false);
        } else {
            // Partially satisfy this assist.
            gp.Value.gcAssistBytes += scanBytes;
            scanBytes = 0;
            // As a heuristic, we move this assist to the
            // back of the queue so that large assists
            // can't clog up the assist queue and
            // substantially delay small assists.
            Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡq).pushBack(gp);
            break;
        }
    }
    if (scanBytes > 0) {
        // Convert from scan bytes back to work.
        var assistWorkPerByte = ᏑgcController.of(gcControllerState.ᏑassistWorkPerByte).Load();
        scanWork = (int64)((float64)scanBytes * assistWorkPerByte);
        ᏑgcController.of(gcControllerState.ᏑbgScanCredit).Add(scanWork);
    }
    unlock(Ꮡwork.of(workType.ᏑassistQueue).of(workType_assistQueue.Ꮡlock));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string scanstackBadStatusˢ = "scanstack - bad status"u8;
internal static readonly @string markBadStatusˢ = "mark - bad status"u8;
internal static readonly @string scanstackGoroutineNotˢ = "scanstack: goroutine not stopped"u8;
internal static readonly @string canTScanOurOwnStackˢ = "can't scan our own stack"u8;
internal static readonly @string remainingPointerBuffersˢ = "remaining pointer buffers"u8;

// scanstack scans gp's stack, greying all pointers found on the stack.
//
// Returns the amount of scan work performed, but doesn't update
// gcController.stackScanWork or flush any credit. Any background credit produced
// by this function should be flushed by its caller. scanstack itself can't
// safely flush because it may result in trying to wake up a goroutine that
// was just scanned, resulting in a self-deadlock.
//
// scanstack will also shrink the stack if it is safe to do so. If it
// is not, it schedules a stack shrink for the next synchronous safe
// point.
//
// scanstack is marked go:systemstack because it must not be preempted
// while using a workbuf.
//
//go:nowritebarrier
//go:systemstack
internal static int64 scanstack(ж<g> Ꮡgp, ж<gcWork> Ꮡgcw) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    if ((uint32)(readgstatus(Ꮡgp) & (uint32)_Gscan) == 0) {
        print((@string)"runtime:scanstack: gp="u8, Ꮡgp.OrTypedNil(), (@string)", goid="u8, gp.goid, (@string)", gp->atomicstatus="u8, ((Δhex)(uint64)readgstatus(Ꮡgp)), (@string)"\n"u8);
        @throw(scanstackBadStatusˢ);
    }
    var exprᴛ1 = (uint32)(readgstatus(Ꮡgp) & ~(uint32)_Gscan);
    if (exprᴛ1 == _Gdead) {
        return 0;
    }
    if (exprᴛ1 == _Grunning) {
        print((@string)"runtime: gp="u8, Ꮡgp.OrTypedNil(), (@string)", goid="u8, gp.goid, (@string)", gp->atomicstatus="u8, readgstatus(Ꮡgp), (@string)"\n"u8);
        @throw(scanstackGoroutineNotˢ);
    }
    else if (exprᴛ1 == _Grunnable || exprᴛ1 == _Gsyscall || exprᴛ1 == _Gwaiting) {
    }
    else { /* default: */
        print((@string)"runtime: gp="u8, Ꮡgp.OrTypedNil(), (@string)", goid="u8, gp.goid, (@string)", gp->atomicstatus="u8, readgstatus(Ꮡgp), (@string)"\n"u8);
        @throw(markBadStatusˢ);
    }

    // ok
    if (Ꮡgp == getg()) {
        @throw(canTScanOurOwnStackˢ);
    }
    // scannedSize is the amount of work we'll be reporting.
    //
    // It is less than the allocated size (which is hi-lo).
    uintptr sp = default!;
    if (gp.syscallsp != 0){
        sp = gp.syscallsp; // If in a system call this is the stack pointer (gp.sched.sp can be 0 in this case on Windows).
    } else {
        sp = gp.sched.sp;
    }
    var scannedSize = gp.stack.hi - sp;
    // Keep statistics for initial stack size calculation.
    // Note that this accumulates the scanned size, not the allocated size.
    var Δp = (~(~getg()).m).p.ptr();
    Δp.Value.scannedStackSize += (uint64)scannedSize;
    Δp.Value.scannedStacks++;
    if (isShrinkStackSafe(Ꮡgp)){
        // Shrink the stack if not much of it is being used.
        shrinkstack(Ꮡgp);
    } else {
        // Otherwise, shrink the stack at the next sync safe point.
        gp.preemptShrink = true;
    }
    ref var state = ref heap(new stackScanState(), out var Ꮡstate);
    state.stack = gp.stack;
    if (stackTraceDebug) {
        println((@string)"stack trace goroutine"u8, gp.goid);
    }
    if (debugScanConservative && gp.asyncSafePoint) {
        print((@string)"scanning async preempted goroutine "u8, gp.goid, (@string)" stack ["u8, ((Δhex)(uint64)gp.stack.lo), (@string)","u8, ((Δhex)(uint64)gp.stack.hi), (@string)")\n"u8);
    }
    // Scan the saved context register. This is effectively a live
    // register that gets moved back and forth between the
    // register and sched.ctxt without a write barrier.
    if (gp.sched.ctxt != nil) {
        scanblock((uintptr)@unsafe.Pointer.FromBox(Ꮡgp.of(g.Ꮡsched).of(gobuf.Ꮡctxt)), goarch.PtrSize, Ꮡoneptrmask.at<uint8>(0), Ꮡgcw, Ꮡstate);
    }
    // Scan the stack. Accumulate a list of stack objects.
    ref var u = ref heap(new unwinder(), out var Ꮡu);
    for (Ꮡu.init(Ꮡgp, 0); u.valid(); Ꮡu.next()) {
        scanframeworker(Ꮡu.of(unwinder.Ꮡframe), Ꮡstate, Ꮡgcw);
    }
    // Find additional pointers that point into the stack from the heap.
    // Currently this includes defers and panics. See also function copystack.
    // Find and trace other pointers in defer records.
    for (var dᴛ1 = gp._defer; dᴛ1 != nil; dᴛ1 = dᴛ1.Value.link) {
        ref var d = ref heap<ж<_defer>>(out var Ꮡd);
        d = dᴛ1;
        if ((~d).fn != default!) {
            // Scan the func value, which could be a stack allocated closure.
            // See issue 30453.
            scanblock((uintptr)d.of(_defer.Ꮡfn), goarch.PtrSize, Ꮡoneptrmask.at<uint8>(0), Ꮡgcw, Ꮡstate);
        }
        if ((~d).link != nil) {
            // The link field of a stack-allocated defer record might point
            // to a heap-allocated defer record. Keep that heap record live.
            scanblock((uintptr)@unsafe.Pointer.FromBox(d.of(_defer.Ꮡlink)), goarch.PtrSize, Ꮡoneptrmask.at<uint8>(0), Ꮡgcw, Ꮡstate);
        }
        // Retain defers records themselves.
        // Defer records might not be reachable from the G through regular heap
        // tracing because the defer linked list might weave between the stack and the heap.
        if ((~d).heap) {
            scanblock((uintptr)@unsafe.Pointer.FromBox(Ꮡd), goarch.PtrSize, Ꮡoneptrmask.at<uint8>(0), Ꮡgcw, Ꮡstate);
        }
        dᴛ1 = d;
    }
    if (gp._panic != nil) {
        // Panics are always stack allocated.
        Ꮡstate.putPtr((uintptr)gp._panic, false);
    }
    // Find and scan all reachable stack objects.
    //
    // The state's pointer queue prioritizes precise pointers over
    // conservative pointers so that we'll prefer scanning stack
    // objects precisely.
    state.buildIndex();
    while (ᐧ) {
        var (pΔ1, conservative) = Ꮡstate.getPtr();
        if (pΔ1 == 0) {
            break;
        }
        var obj = state.findObject(pΔ1);
        if (obj == nil) {
            continue;
        }
        var r = obj.Value.r;
        if (r == nil) {
            // We've already scanned this object.
            continue;
        }
        obj.setRecord(nil); // Don't scan it again.
        if (stackTraceDebug) {
            printlock();
            print((@string)"  live stkobj at"u8, ((Δhex)(uint64)(state.stack.lo + (uintptr)(~obj).off)), (@string)"of size"u8, (~obj).size);
            if (conservative) {
                print((@string)" (conservative)"u8);
            }
            println();
            printunlock();
        }
        var gcdata = r.gcdata();
        ж<mspan> s = default!;
        if (r.useGCProg()) {
            // This path is pretty unlikely, an object large enough
            // to have a GC program allocated on the stack.
            // We need some space to unpack the program into a straight
            // bitmask, which we allocate/free here.
            // TODO: it would be nice if there were a way to run a GC
            // program without having to store all its bits. We'd have
            // to change from a Lempel-Ziv style program to something else.
            // Or we can forbid putting objects on stacks if they require
            // a gc program (see issue 27447).
            s = materializeGCProg(r.ptrdata(), gcdata);
            gcdata = (ж<byte>)(uintptr)((@unsafe.Pointer)(~s).startAddr);
        }
        var b = state.stack.lo + (uintptr)(~obj).off;
        if (conservative){
            scanConservative(b, r.ptrdata(), gcdata, Ꮡgcw, Ꮡstate);
        } else {
            scanblock(b, r.ptrdata(), gcdata, Ꮡgcw, Ꮡstate);
        }
        if (s != nil) {
            dematerializeGCProg(s);
        }
    }
    // Deallocate object buffers.
    // (Pointer buffers were all deallocated in the loop above.)
    while (state.head != nil) {
        var x = state.head;
        state.head = x.Value.next;
        if (stackTraceDebug) {
            for (nint i = 0; i < (~x).nobj; i++) {
                var obj = x.at(stackObjectBuf.Ꮡobj, i);
                if ((~obj).r == nil) {
                    // reachable
                    continue;
                }
                println((@string)"  dead stkobj at"u8, ((Δhex)(uint64)(gp.stack.lo + (uintptr)(~obj).off)), (@string)"of size"u8, (~(~obj).r).size);
            }
        }
        // Note: not necessarily really dead - only reachable-from-ptr dead.
        x.Value.nobj = 0;
        putempty(x.Reinterpret<stackObjectBuf, workbuf>());
    }
    if (state.buf != nil || state.cbuf != nil || state.freeBuf != nil) {
        @throw(remainingPointerBuffersˢ);
    }
    return (int64)scannedSize;
}

// Scan a stack frame: local variables and function arguments/results.
//
//go:nowritebarrier
internal static void scanframeworker(ж<stkframe> Ꮡframe, ж<stackScanState> Ꮡstate, ж<gcWork> Ꮡgcw) {
    ref var frame = ref Ꮡframe.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    if (_DebugGC > 1 && frame.continpc != 0) {
        print((@string)"scanframe "u8, funcname(frame.fn), (@string)"\n"u8);
    }
    var isAsyncPreempt = frame.fn.valid() && frame.fn.funcID == abi.FuncID_asyncPreempt;
    var isDebugCall = frame.fn.valid() && frame.fn.funcID == abi.FuncID_debugCallV2;
    if (state.conservative || isAsyncPreempt || isDebugCall) {
        if (debugScanConservative) {
            println((@string)"conservatively scanning function"u8, funcname(frame.fn), (@string)"at PC"u8, ((Δhex)(uint64)frame.continpc));
        }
        // Conservatively scan the frame. Unlike the precise
        // case, this includes the outgoing argument space
        // since we may have stopped while this function was
        // setting up a call.
        //
        // TODO: We could narrow this down if the compiler
        // produced a single map per function of stack slots
        // and registers that ever contain a pointer.
        if (frame.varp != 0) {
            var size = frame.varp - frame.sp;
            if (size > 0) {
                scanConservative(frame.sp, size, nil, Ꮡgcw, Ꮡstate);
            }
        }
        // Scan arguments to this frame.
        {
            var n = frame.argBytes(); if (n != 0) {
                // TODO: We could pass the entry argument map
                // to narrow this down further.
                scanConservative(frame.argp, n, nil, Ꮡgcw, Ꮡstate);
            }
        }
        if (isAsyncPreempt || isDebugCall){
            // This function's frame contained the
            // registers for the asynchronously stopped
            // parent frame. Scan the parent
            // conservatively.
            state.conservative = true;
        } else {
            // We only wanted to scan those two frames
            // conservatively. Clear the flag for future
            // frames.
            state.conservative = false;
        }
        return;
    }
    var (locals, args, objs) = frame.getStackMap(false);
    // Scan local variables if stack frame has been allocated.
    if (locals.n > 0) {
        var size = (uintptr)locals.n * (uintptr)goarch.PtrSize;
        scanblock(frame.varp - size, size, locals.bytedata, Ꮡgcw, Ꮡstate);
    }
    // Scan arguments.
    if (args.n > 0) {
        scanblock(frame.argp, (uintptr)args.n * (uintptr)goarch.PtrSize, args.bytedata, Ꮡgcw, Ꮡstate);
    }
    // Add all stack objects to the stack object list.
    if (frame.varp != 0) {
        // varp is 0 for defers, where there are no locals.
        // In that case, there can't be a pointer to its args, either.
        // (And all args would be scanned above anyway.)
        foreach (var (i, _) in objs) {
            var obj = Ꮡ(objs, i);
            var off = obj.Value.off;
            var @base = frame.varp; // locals base pointer
            if (off >= 0) {
                @base = frame.argp; // arguments and return values base pointer
            }
            var ptr = @base + (uintptr)off;
            if (ptr < frame.sp) {
                // object hasn't been allocated in the frame yet.
                continue;
            }
            if (stackTraceDebug) {
                println((@string)"stkobj at"u8, ((Δhex)(uint64)ptr), (@string)"of size"u8, (~obj).size);
            }
            state.addObject(ptr, obj);
        }
    }
}

[GoType("num:nint")] partial struct gcDrainFlags;

internal static gcDrainFlags gcDrainUntilPreempt => /* 1 << iota */ 1;
internal static gcDrainFlags gcDrainFlushBgCredit => 2;
internal static gcDrainFlags gcDrainIdle => 4;
internal static gcDrainFlags gcDrainFractional => 8;

// gcDrainMarkWorkerIdle is a wrapper for gcDrain that exists to better account
// mark time in profiles.
internal static void gcDrainMarkWorkerIdle(ж<gcWork> Ꮡgcw) {
    gcDrain(Ꮡgcw, (gcDrainFlags)((gcDrainFlags)(gcDrainIdle | gcDrainUntilPreempt) | gcDrainFlushBgCredit));
}

// gcDrainMarkWorkerDedicated is a wrapper for gcDrain that exists to better account
// mark time in profiles.
internal static void gcDrainMarkWorkerDedicated(ж<gcWork> Ꮡgcw, bool untilPreempt) {
    gcDrainFlags flags = gcDrainFlushBgCredit;
    if (untilPreempt) {
        flags |= (gcDrainFlags)(gcDrainUntilPreempt);
    }
    gcDrain(Ꮡgcw, flags);
}

// gcDrainMarkWorkerFractional is a wrapper for gcDrain that exists to better account
// mark time in profiles.
internal static void gcDrainMarkWorkerFractional(ж<gcWork> Ꮡgcw) {
    gcDrain(Ꮡgcw, (gcDrainFlags)((gcDrainFlags)(gcDrainFractional | gcDrainUntilPreempt) | gcDrainFlushBgCredit));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcDrainPhaseIncorrectˢ = "gcDrain phase incorrect"u8;

// gcDrain scans roots and objects in work buffers, blackening grey
// objects until it is unable to get more work. It may return before
// GC is done; it's the caller's responsibility to balance work from
// other Ps.
//
// If flags&gcDrainUntilPreempt != 0, gcDrain returns when g.preempt
// is set.
//
// If flags&gcDrainIdle != 0, gcDrain returns when there is other work
// to do.
//
// If flags&gcDrainFractional != 0, gcDrain self-preempts when
// pollFractionalWorkerExit() returns true. This implies
// gcDrainNoBlock.
//
// If flags&gcDrainFlushBgCredit != 0, gcDrain flushes scan work
// credit to gcController.bgScanCredit every gcCreditSlack units of
// scan work.
//
// gcDrain will always return if there is a pending STW or forEachP.
//
// Disabling write barriers is necessary to ensure that after we've
// confirmed that we've drained gcw, that we don't accidentally end
// up flipping that condition by immediately adding work in the form
// of a write barrier buffer flush.
//
// Don't set nowritebarrierrec because it's safe for some callees to
// have write barriers enabled.
//
//go:nowritebarrier
internal static void gcDrain(ж<gcWork> Ꮡgcw, gcDrainFlags flags) {
    ref var gcw = ref Ꮡgcw.DerefOrNull();

    if (!writeBarrier.enabled) {
        @throw(gcDrainPhaseIncorrectˢ);
    }
    // N.B. We must be running in a non-preemptible context, so it's
    // safe to hold a reference to our P here.
    var gp = getg().Value.m.Value.curg;
    var pp = (~(~gp).m).p.ptr();
    var preemptible = (gcDrainFlags)(flags & gcDrainUntilPreempt) != 0;
    var flushBgCredit = (gcDrainFlags)(flags & gcDrainFlushBgCredit) != 0;
    var idle = (gcDrainFlags)(flags & gcDrainIdle) != 0;
    var initScanWork = gcw.heapScanWork;
    // checkWork is the scan work before performing the next
    // self-preempt check.
    var checkWork = (int64)(9223372036854775807L);
    Func<bool> check = default!;
    if ((gcDrainFlags)(flags & ((gcDrainFlags)(gcDrainIdle | gcDrainFractional))) != 0) {
        checkWork = initScanWork + (int64)drainCheckThreshold;
        if (idle){
            check = pollWork;
        } else 
        if ((gcDrainFlags)(flags & gcDrainFractional) != 0) {
            check = pollFractionalWorkerExit;
        }
    }
    // Drain root marking jobs.
    if (work.markrootNext < work.markrootJobs) {
        // Stop if we're preemptible, if someone wants to STW, or if
        // someone is calling forEachP.
        while (!((~gp).preempt && (preemptible || Ꮡsched.of(schedt.Ꮡgcwaiting).Load() || (~pp).runSafePointFn != 0))) {
            var job = atomic.Xadd(Ꮡwork.of(workType.ᏑmarkrootNext), +1) - 1;
            if (job >= work.markrootJobs) {
                break;
            }
            markroot(Ꮡgcw, job, flushBgCredit);
            if (check != default! && check()) {
                goto done;
            }
        }
    }
    // Drain heap marking jobs.
    //
    // Stop if we're preemptible, if someone wants to STW, or if
    // someone is calling forEachP.
    //
    // TODO(mknyszek): Consider always checking gp.preempt instead
    // of having the preempt flag, and making an exception for certain
    // mark workers in retake. That might be simpler than trying to
    // enumerate all the reasons why we might want to preempt, even
    // if we're supposed to be mostly non-preemptible.
    while (!((~gp).preempt && (preemptible || Ꮡsched.of(schedt.Ꮡgcwaiting).Load() || (~pp).runSafePointFn != 0))) {
        // Try to keep work available on the global queue. We used to
        // check if there were waiting workers, but it's better to
        // just keep work available than to make workers wait. In the
        // worst case, we'll do O(log(_WorkbufSize)) unnecessary
        // balances.
        if (work.full == 0) {
            gcw.balance();
        }
        var b = gcw.tryGetFast();
        if (b == 0) {
            b = gcw.tryGet();
            if (b == 0) {
                // Flush the write barrier
                // buffer; this may create
                // more work.
                wbBufFlush();
                b = gcw.tryGet();
            }
        }
        if (b == 0) {
            // Unable to get work.
            break;
        }
        scanobject(b, Ꮡgcw);
        // Flush background scan work credit to the global
        // account if we've accumulated enough locally so
        // mutator assists can draw on it.
        if (gcw.heapScanWork >= gcCreditSlack) {
            ᏑgcController.of(gcControllerState.ᏑheapScanWork).Add(gcw.heapScanWork);
            if (flushBgCredit) {
                gcFlushBgCredit(gcw.heapScanWork - initScanWork);
                initScanWork = 0;
            }
            checkWork -= gcw.heapScanWork;
            gcw.heapScanWork = 0;
            if (checkWork <= 0) {
                checkWork += drainCheckThreshold;
                if (check != default! && check()) {
                    break;
                }
            }
        }
    }
done:
    if (gcw.heapScanWork > 0) {
        // Flush remaining scan work credit.
        ᏑgcController.of(gcControllerState.ᏑheapScanWork).Add(gcw.heapScanWork);
        if (flushBgCredit) {
            gcFlushBgCredit(gcw.heapScanWork - initScanWork);
        }
        gcw.heapScanWork = 0;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcDrainNPhaseIncorrectˢ = "gcDrainN phase incorrect"u8;

// gcDrainN blackens grey objects until it has performed roughly
// scanWork units of scan work or the G is preempted. This is
// best-effort, so it may perform less work if it fails to get a work
// buffer. Otherwise, it will perform at least n units of work, but
// may perform more because scanning is always done in whole object
// increments. It returns the amount of scan work performed.
//
// The caller goroutine must be in a preemptible state (e.g.,
// _Gwaiting) to prevent deadlocks during stack scanning. As a
// consequence, this must be called on the system stack.
//
//go:nowritebarrier
//go:systemstack
internal static int64 gcDrainN(ж<gcWork> Ꮡgcw, int64 scanWork) {
    ref var gcw = ref Ꮡgcw.DerefOrNull();

    if (!writeBarrier.enabled) {
        @throw(gcDrainNPhaseIncorrectˢ);
    }
    // There may already be scan work on the gcw, which we don't
    // want to claim was done by this call.
    var workFlushed = -gcw.heapScanWork;
    // In addition to backing out because of a preemption, back out
    // if the GC CPU limiter is enabled.
    var gp = getg().Value.m.Value.curg;
    while (!(~gp).preempt && !ᏑgcCPULimiter.limiting() && workFlushed + gcw.heapScanWork < scanWork) {
        // See gcDrain comment.
        if (work.full == 0) {
            gcw.balance();
        }
        var b = gcw.tryGetFast();
        if (b == 0) {
            b = gcw.tryGet();
            if (b == 0) {
                // Flush the write barrier buffer;
                // this may create more work.
                wbBufFlush();
                b = gcw.tryGet();
            }
        }
        if (b == 0) {
            // Try to do a root job.
            if (work.markrootNext < work.markrootJobs) {
                var job = atomic.Xadd(Ꮡwork.of(workType.ᏑmarkrootNext), +1) - 1;
                if (job < work.markrootJobs) {
                    workFlushed += markroot(Ꮡgcw, job, false);
                    continue;
                }
            }
            // No heap or root jobs.
            break;
        }
        scanobject(b, Ꮡgcw);
        // Flush background scan work credit.
        if (gcw.heapScanWork >= gcCreditSlack) {
            ᏑgcController.of(gcControllerState.ᏑheapScanWork).Add(gcw.heapScanWork);
            workFlushed += gcw.heapScanWork;
            gcw.heapScanWork = 0;
        }
    }
    // Unlike gcDrain, there's no need to flush remaining work
    // here because this never flushes to bgScanCredit and
    // gcw.dispose will flush any remaining work to scanWork.
    return workFlushed + gcw.heapScanWork;
}

// scanblock scans b as scanobject would, but using an explicit
// pointer bitmap instead of the heap bitmap.
//
// This is used to scan non-heap roots, so it does not update
// gcw.bytesMarked or gcw.heapScanWork.
//
// If stk != nil, possible stack pointers are also reported to stk.putPtr.
//
//go:nowritebarrier
internal static void scanblock(uintptr b0, uintptr n0, ж<uint8> Ꮡptrmask, ж<gcWork> Ꮡgcw, ж<stackScanState> Ꮡstk) {
    ref var stk = ref Ꮡstk.DerefOrNull();

    // Use local copies of original parameters, so that a stack trace
    // due to one of the throws below shows the original block
    // base and extent.
    var b = b0;
    var n = n0;
    for (var i = (uintptr)0; i < n; ) {
        // Find bits for the next word.
        var bits = (uint32)(addb(Ꮡptrmask, i / (uintptr)(goarch.PtrSize * 8)).Value);
        if (bits == 0) {
            i += goarch.PtrSize * 8;
            continue;
        }
        for (nint j = 0; j < 8 && i < n; j++) {
            if ((uint32)(bits & 1) != 0) {
                // Same work as in scanobject; see comments there.
                var Δp = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)(b + i));
                if (Δp != 0) {
                    {
                        var (obj, span, objIndex) = findObject(Δp, b, i); if (obj != 0){
                            greyobject(obj, b, i, span, Ꮡgcw, objIndex);
                        } else 
                        if (Ꮡstk != nil && Δp >= stk.stack.lo && Δp < stk.stack.hi) {
                            Ꮡstk.putPtr(Δp, false);
                        }
                    }
                }
            }
            bits >>= (int)(1);
            i += goarch.PtrSize;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string scanobjectN0ˢ = "scanobject n == 0"u8;
internal static readonly @string scanobjectOfANoscanˢ = "scanobject of a noscan object"u8;

// scanobject scans the object starting at b, adding pointers to gcw.
// b must point to the beginning of a heap object or an oblet.
// scanobject consults the GC bitmap for the pointer mask and the
// spans for the size of the object.
//
//go:nowritebarrier
internal static void scanobject(uintptr b, ж<gcWork> Ꮡgcw) {
    ref var gcw = ref Ꮡgcw.DerefOrNull();

    // Prefetch object before we scan it.
    //
    // This will overlap fetching the beginning of the object with initial
    // setup before we start scanning the object.
    sys.Prefetch(b);
    // Find the bits for b and the size of the object at b.
    //
    // b is either the beginning of an object, in which case this
    // is the size of the object to scan, or it points to an
    // oblet, in which case we compute the size to scan below.
    var s = spanOfUnchecked(b);
    var n = s.Value.elemsize;
    if (n == 0) {
        @throw(scanobjectN0ˢ);
    }
    if ((~s).spanclass.noscan()) {
        // Correctness-wise this is ok, but it's inefficient
        // if noscan objects reach here.
        @throw(scanobjectOfANoscanˢ);
    }
    typePointers tp = default!;
    if (n > maxObletBytes){
        // Large object. Break into oblets for better
        // parallelism and lower latency.
        if (b == s.@base()) {
            // Enqueue the other oblets to scan later.
            // Some oblets may be in b's scalar tail, but
            // these will be marked as "no more pointers",
            // so we'll drop out immediately when we go to
            // scan those.
            for (var oblet = b + (uintptr)maxObletBytes; oblet < s.@base() + (~s).elemsize; oblet += maxObletBytes) {
                if (!gcw.putFast(oblet)) {
                    gcw.put(oblet);
                }
            }
        }
        // Compute the size of the oblet. Since this object
        // must be a large object, s.base() is the beginning
        // of the object.
        n = s.@base() + (~s).elemsize - b;
        n = min(n, (uintptr)(maxObletBytes));
        tp = s.typePointersOfUnchecked(s.@base());
        tp = tp.fastForward(b - tp.addr, b + n);
    } else {
        tp = s.typePointersOfUnchecked(b);
    }
    uintptr scanSize = default!;
    while (ᐧ) {
        uintptr addr = default!;
        {
            (tp, addr) = tp.nextFast(); if (addr == 0) {
                {
                    (tp, addr) = tp.next(b + n); if (addr == 0) {
                        break;
                    }
                }
            }
        }
        // Keep track of farthest pointer we found, so we can
        // update heapScanWork. TODO: is there a better metric,
        // now that we can skip scalar portions pretty efficiently?
        scanSize = addr - b + (uintptr)goarch.PtrSize;
        // Work here is duplicated in scanblock and above.
        // If you make changes here, make changes there too.
        var obj = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)addr);
        // At this point we have extracted the next potential pointer.
        // Quickly filter out nil and pointers back to the current object.
        if (obj != 0 && obj - b >= n) {
            // Test if obj points into the Go heap and, if so,
            // mark the object.
            //
            // Note that it's possible for findObject to
            // fail if obj points to a just-allocated heap
            // object because of a race with growing the
            // heap. In this case, we know the object was
            // just allocated and hence will be marked by
            // allocation itself.
            {
                var (objΔ1, span, objIndex) = findObject(obj, b, addr - b); if (objΔ1 != 0) {
                    greyobject(objΔ1, b, addr - b, span, Ꮡgcw, objIndex);
                }
            }
        }
    }
    gcw.bytesMarked += (uint64)n;
    gcw.heapScanWork += (int64)scanSize;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string misalignedMaskˢ = "misaligned mask"u8;

// scanConservative scans block [b, b+n) conservatively, treating any
// pointer-like value in the block as a pointer.
//
// If ptrmask != nil, only words that are marked in ptrmask are
// considered as potential pointers.
//
// If state != nil, it's assumed that [b, b+n) is a block in the stack
// and may contain pointers to stack objects.
internal static void scanConservative(uintptr b, uintptr n, ж<uint8> Ꮡptrmask, ж<gcWork> Ꮡgcw, ж<stackScanState> Ꮡstate) {
    ref var ptrmask = ref Ꮡptrmask.DerefOrNull();
    ref var state = ref Ꮡstate.DerefOrNull();

    if (debugScanConservative) {
        printlock();
        print((@string)"conservatively scanning ["u8, ((Δhex)(uint64)b), (@string)","u8, ((Δhex)(uint64)(b + n)), (@string)")\n"u8);
        hexdumpWords(b, b + n, (uintptr Δp) => {
            if (Ꮡptrmask != nil) {
                var word = (Δp - b) / (uintptr)goarch.PtrSize;
                var bits = addb(Ꮡptrmask, word / 8).Value;
                if ((byte)(((bits >> (int)((word % 8)))) & 1) == 0) {
                    return (rune)'$';
                }
            }
            var val = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)Δp);
            if (Ꮡstate != nil && Ꮡstate.Value.stack.lo <= val && val < Ꮡstate.Value.stack.hi) {
                return (rune)'@';
            }
            var span = spanOfHeap(val);
            if (span == nil) {
                return (rune)' ';
            }
            var idx = span.objIndex(val);
            if (span.isFree(idx)) {
                return (rune)' ';
            }
            return (rune)'*';
        });
        printunlock();
    }
    for (var i = (uintptr)0; i < n; i += goarch.PtrSize) {
        if (Ꮡptrmask != nil) {
            var word = i / (uintptr)goarch.PtrSize;
            var bits = addb(Ꮡptrmask, word / 8).Value;
            if (bits == 0) {
                // Skip 8 words (the loop increment will do the 8th)
                //
                // This must be the first time we've
                // seen this word of ptrmask, so i
                // must be 8-word-aligned, but check
                // our reasoning just in case.
                if (i % (uintptr)(goarch.PtrSize * 8) != 0) {
                    @throw(misalignedMaskˢ);
                }
                i += goarch.PtrSize * 8 - goarch.PtrSize;
                continue;
            }
            if ((byte)(((bits >> (int)((word % 8)))) & 1) == 0) {
                continue;
            }
        }
        var val = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)(b + i));
        // Check if val points into the stack.
        if (Ꮡstate != nil && state.stack.lo <= val && val < state.stack.hi) {
            // val may point to a stack object. This
            // object may be dead from last cycle and
            // hence may contain pointers to unallocated
            // objects, but unlike heap objects we can't
            // tell if it's already dead. Hence, if all
            // pointers to this object are from
            // conservative scanning, we have to scan it
            // defensively, too.
            Ꮡstate.putPtr(val, true);
            continue;
        }
        // Check if val points to a heap span.
        var span = spanOfHeap(val);
        if (span == nil) {
            continue;
        }
        // Check if val points to an allocated object.
        var idx = span.objIndex(val);
        if (span.isFree(idx)) {
            continue;
        }
        // val points to an allocated object. Mark it.
        var obj = span.@base() + idx * (~span).elemsize;
        greyobject(obj, b, i, span, Ꮡgcw, idx);
    }
}

// Shade the object if it isn't already.
// The object is not nil and known to be in the heap.
// Preemption must be disabled.
//
//go:nowritebarrier
internal static void shade(uintptr b) {
    {
        var (obj, span, objIndex) = findObject(b, 0, 0); if (obj != 0) {
            var gcw = (~(~getg()).m).p.ptr().of(runtime_package.Δp.Ꮡgcw);
            greyobject(obj, 0, 0, span, gcw, objIndex);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string greyobjectObjNotPointerˢ = "greyobject: obj not pointer-aligned"u8;
internal static readonly @string markingFreeObjectˢ = "marking free object"u8;

// obj is the start of an object with mark mbits.
// If it isn't already marked, mark it and enqueue into gcw.
// base and off are for debugging only and could be removed.
//
// See also wbBufFlush1, which partially duplicates this logic.
//
//go:nowritebarrierrec
internal static void greyobject(uintptr obj, uintptr @base, uintptr off, ж<mspan> Ꮡspan, ж<gcWork> Ꮡgcw, uintptr objIndex) {
    ref var span = ref Ꮡspan.DerefOrNull();
    ref var gcw = ref Ꮡgcw.DerefOrNull();

    // obj should be start of allocation, and so must be at least pointer-aligned.
    if ((uintptr)(obj & (uintptr)(goarch.PtrSize - 1)) != 0) {
        @throw(greyobjectObjNotPointerˢ);
    }
    var mbits = span.markBitsForIndex(objIndex);
    if (useCheckmark){
        if (setCheckmark(obj, @base, off, mbits)) {
            // Already marked.
            return;
        }
    } else {
        if (debug.gccheckmark > 0 && span.isFree(objIndex)) {
            print((@string)"runtime: marking free object "u8, ((Δhex)(uint64)obj), (@string)" found at *("u8, ((Δhex)(uint64)@base), (@string)"+"u8, ((Δhex)(uint64)off), (@string)")\n"u8);
            gcDumpObject(baseˢ, @base, off);
            gcDumpObject(objˢ, obj, ~(uintptr)0);
            getg().Value.m.Value.traceback = 2;
            @throw(markingFreeObjectˢ);
        }
        // If marked we have nothing to do.
        if (mbits.isMarked()) {
            return;
        }
        mbits.setMarked();
        // Mark span.
        var (arena, pageIdx, pageMask) = pageIndexOf(span.@base());
        if ((uint8)((~arena).pageMarks[(nint)(pageIdx)] & pageMask) == 0) {
            atomic.Or8(arena.at(heapArena.ᏑpageMarks, (nint)(pageIdx)), pageMask);
        }
        // If this is a noscan object, fast-track it to black
        // instead of greying it.
        if (span.spanclass.noscan()) {
            gcw.bytesMarked += (uint64)span.elemsize;
            return;
        }
    }
    // We're adding obj to P's local workbuf, so it's likely
    // this object will be processed soon by the same P.
    // Even if the workbuf gets flushed, there will likely still be
    // some benefit on platforms with inclusive shared caches.
    sys.Prefetch(obj);
    // Queue the obj for scanning.
    if (!gcw.putFast(obj)) {
        gcw.put(obj);
    }
}

// gcDumpObject dumps the contents of obj for debugging and marks the
// field at byte offset off in obj.
internal static void gcDumpObject(@string label, uintptr obj, uintptr off) {
    var s = spanOf(obj);
    print(label, (@string)"="u8, ((Δhex)(uint64)obj));
    if (s == nil) {
        print((@string)" s=nil\n"u8);
        return;
    }
    print((@string)" s.base()="u8, ((Δhex)(uint64)s.@base()), (@string)" s.limit="u8, ((Δhex)(uint64)(~s).limit), (@string)" s.spanclass="u8, (~s).spanclass, (@string)" s.elemsize="u8, (~s).elemsize, (@string)" s.state="u8);
    {
        var state = s.of(mspan.Ꮡstate).get(); if (0 <= state && (nint)(uint8)state < len(mSpanStateNames)){
            print(mSpanStateNames[state], (@string)"\n"u8);
        } else {
            print((@string)"unknown("u8, state, (@string)")\n"u8);
        }
    }
    var skipped = false;
    var size = s.Value.elemsize;
    if (s.of(mspan.Ꮡstate).get() == mSpanManual && size == 0) {
        // We're printing something from a stack frame. We
        // don't know how big it is, so just show up to an
        // including off.
        size = off + (uintptr)goarch.PtrSize;
    }
    for (var i = (uintptr)0; i < size; i += goarch.PtrSize) {
        // For big objects, just print the beginning (because
        // that usually hints at the object's type) and the
        // fields around off.
        if (!(i < (uintptr)(128 * goarch.PtrSize) || off - (uintptr)(16 * goarch.PtrSize) < i && i < off + (uintptr)(16 * goarch.PtrSize))) {
            skipped = true;
            continue;
        }
        if (skipped) {
            print((@string)" ...\n"u8);
            skipped = false;
        }
        print((@string)" *("u8, label, (@string)"+"u8, i, (@string)") = "u8, ((Δhex)(uint64)(~(ж<uintptr>)(uintptr)((@unsafe.Pointer)(obj + i)))));
        if (i == off) {
            print((@string)" <=="u8);
        }
        print((@string)"\n"u8);
    }
    if (skipped) {
        print((@string)" ...\n"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcmarknewobjectCalledˢ = "gcmarknewobject called while doing checkmark"u8;

// gcmarknewobject marks a newly allocated object black. obj must
// not contain any non-nil pointers.
//
// This is nosplit so it can manipulate a gcWork without preemption.
//
//go:nowritebarrier
//go:nosplit
internal static void gcmarknewobject(ж<mspan> Ꮡspan, uintptr obj) {
    ref var span = ref Ꮡspan.DerefOrNull();

    if (useCheckmark) {
        // The world should be stopped so this should not happen.
        @throw(gcmarknewobjectCalledˢ);
    }
    // Mark object.
    var objIndex = span.objIndex(obj);
    span.markBitsForIndex(objIndex).setMarked();
    // Mark span.
    var (arena, pageIdx, pageMask) = pageIndexOf(span.@base());
    if ((uint8)((~arena).pageMarks[(nint)(pageIdx)] & pageMask) == 0) {
        atomic.Or8(arena.at(heapArena.ᏑpageMarks, (nint)(pageIdx)), pageMask);
    }
    var gcw = (~(~getg()).m).p.ptr().of(runtime_package.Δp.Ꮡgcw);
    gcw.Value.bytesMarked += (uint64)span.elemsize;
}

// gcMarkTinyAllocs greys all active tiny alloc blocks.
//
// The world must be stopped.
internal static void gcMarkTinyAllocs() {
    assertWorldStopped();
    foreach (var (_, Δp) in allp) {
        var c = Δp.Value.mcache;
        if (c == nil || (~c).tiny == 0) {
            continue;
        }
        var (_, span, objIndex) = findObject((~c).tiny, 0, 0);
        var gcw = Δp.of(runtime_package.Δp.Ꮡgcw);
        greyobject((~c).tiny, 0, 0, span, gcw, objIndex);
    }
}

} // end runtime_package
