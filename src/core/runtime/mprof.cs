// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Malloc profiling.
// Patterned after tcmalloc's algorithms; shorter code.
namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using profilerecord = @internal.profilerecord_package;
using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using System.Runtime.CompilerServices;

partial class runtime_package {

// NOTE(rsc): Everything here could use cas if contention became an issue.
// profInsertLock protects changes to the start of all *bucket linked lists
internal static readonly ж<mutex> ᏑprofInsertLock = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex profInsertLock => ref ((StaticBox<mutex>)ᏑprofInsertLock).ValueSlot;
// profBlockLock protects the contents of every blockRecord struct
internal static readonly ж<mutex> ᏑprofBlockLock = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex profBlockLock => ref ((StaticBox<mutex>)ᏑprofBlockLock).ValueSlot;
// profMemActiveLock protects the active field of every memRecord struct
internal static readonly ж<mutex> ᏑprofMemActiveLock = new StaticBox<mutex>(new mutex(nil));
internal static ref mutex profMemActiveLock => ref ((StaticBox<mutex>)ᏑprofMemActiveLock).ValueSlot;
// profMemFutureLock is a set of locks that protect the respective elements
// of the future array of every memRecord struct
internal static readonly ж<array<mutex>> ᏑprofMemFutureLock = new StaticBox<array<mutex>>(new array<mutex>(3, () => new(nil)));
internal static ref array<mutex> profMemFutureLock => ref ((StaticBox<array<mutex>>)ᏑprofMemFutureLock).ValueSlot;

// All memory allocations are local and do not escape outside of the profiler.
// The profiler is forbidden from referring to garbage-collected memory.

// profile types
internal static bucketType memProfile => /* 1 + iota */ 1;
internal static bucketType blockProfile => 2;
internal static bucketType mutexProfile => 3;

// size of bucket hash table
internal static UntypedInt buckHashSize => 179999;

// maxSkip is to account for deferred inline expansion
// when using frame pointer unwinding. We record the stack
// with "physical" frame pointers but handle skipping "logical"
// frames at some point after collecting the stack. So
// we need extra space in order to avoid getting fewer than the
// desired maximum number of frames after expansion.
// This should be at least as large as the largest skip value
// used for profiling; otherwise stacks may be truncated inconsistently
internal static UntypedInt maxSkip => 6;

// maxProfStackDepth is the highest valid value for debug.profstackdepth.
// It's used for the bucket.stk func.
// TODO(fg): can we get rid of this?
internal static UntypedInt maxProfStackDepth => 1024;

partial struct bucketType /*num:nint*/;

// A bucket holds per-call-stack profiling information.
// The representation is a bit sleazy, inherited from C.
// This struct defines the bucket header. It is followed in
// memory by the stack words and then the actual record
// data, either a memRecord or a blockRecord.
//
// Per-call-stack profiling information.
// Lookup by hashing call stack into a linked-list hash table.
//
// None of the fields in this bucket header are modified after
// creation, including its next and allnext links.
//
// No heap pointers.
partial struct bucket {
    internal sys.NotInHeap _;
    internal ж<bucket> next;
    internal ж<bucket> allnext;
    internal bucketType typ; // memBucket or blockBucket (includes mutexProfile)
    internal uintptr hash;
    internal uintptr size;
    internal uintptr nstk;
}

// A memRecord is the bucket data for a bucket of type memProfile,
// part of the memory profile.
partial struct memRecord {
// The following complex 3-stage scheme of stats accumulation
// is required to obtain a consistent picture of mallocs and frees
// for some point in time.
// The problem is that mallocs come in real time, while frees
// come only after a GC during concurrent sweeping. So if we would
// naively count them, we would get a skew toward mallocs.
//
// Hence, we delay information to get consistent snapshots as
// of mark termination. Allocations count toward the next mark
// termination's snapshot, while sweep frees count toward the
// previous mark termination's snapshot:
//
//              MT          MT          MT          MT
//             .·|         .·|         .·|         .·|
//          .·˙  |      .·˙  |      .·˙  |      .·˙  |
//       .·˙     |   .·˙     |   .·˙     |   .·˙     |
//    .·˙        |.·˙        |.·˙        |.·˙        |
//
//       alloc → ▲ ← free
//               ┠┅┅┅┅┅┅┅┅┅┅┅P
//       C+2     →    C+1    →  C
//
//                   alloc → ▲ ← free
//                           ┠┅┅┅┅┅┅┅┅┅┅┅P
//                   C+2     →    C+1    →  C
//
// Since we can't publish a consistent snapshot until all of
// the sweep frees are accounted for, we wait until the next
// mark termination ("MT" above) to publish the previous mark
// termination's snapshot ("P" above). To do this, allocation
// and free events are accounted to *future* heap profile
// cycles ("C+n" above) and we only publish a cycle once all
// of the events from that cycle must be done. Specifically:
//
// Mallocs are accounted to cycle C+2.
// Explicit frees are accounted to cycle C+2.
// GC frees (done during sweeping) are accounted to cycle C+1.
//
// After mark termination, we increment the global heap
// profile cycle counter and accumulate the stats from cycle C
// into the active profile.

    // active is the currently published profile. A profiling
    // cycle can be accumulated into active once its complete.
    internal memRecordCycle active;
    // future records the profile events we're counting for cycles
    // that have not yet been published. This is ring buffer
    // indexed by the global heap profile cycle C and stores
    // cycles C, C+1, and C+2. Unlike active, these counts are
    // only for a single cycle; they are not cumulative across
    // cycles.
    //
    // We store cycle C here because there's a window between when
    // C becomes the active cycle and when we've flushed it to
    // active.
    internal array<memRecordCycle> future = new(3);
}

// memRecordCycle
partial struct memRecordCycle {
    internal uintptr allocs, frees;
    internal uintptr alloc_bytes, free_bytes;
}

// add accumulates b into a. It does not zero b.
internal static void add(this ref memRecordCycle a, ж<memRecordCycle> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    a.allocs += b.allocs;
    a.frees += b.frees;
    a.alloc_bytes += b.alloc_bytes;
    a.free_bytes += b.free_bytes;
}

// A blockRecord is the bucket data for a bucket of type blockProfile,
// which is used in blocking and mutex profiles.
partial struct blockRecord {
    internal float64 count;
    internal int64 cycles;
}

internal static readonly ж<atomic.UnsafePointer> Ꮡmbuckets = new StaticBox<atomic.UnsafePointer>(default(atomic.UnsafePointer));
internal static ref atomic.UnsafePointer mbuckets => ref ((StaticBox<atomic.UnsafePointer>)Ꮡmbuckets).ValueSlot;                    // *bucket, memory profile buckets
internal static readonly ж<atomic.UnsafePointer> Ꮡbbuckets = new StaticBox<atomic.UnsafePointer>(default(atomic.UnsafePointer));
internal static ref atomic.UnsafePointer bbuckets => ref ((StaticBox<atomic.UnsafePointer>)Ꮡbbuckets).ValueSlot;                    // *bucket, blocking profile buckets
internal static readonly ж<atomic.UnsafePointer> Ꮡxbuckets = new StaticBox<atomic.UnsafePointer>(default(atomic.UnsafePointer));
internal static ref atomic.UnsafePointer xbuckets => ref ((StaticBox<atomic.UnsafePointer>)Ꮡxbuckets).ValueSlot;                    // *bucket, mutex profile buckets
internal static readonly ж<atomic.UnsafePointer> Ꮡbuckhash = new StaticBox<atomic.UnsafePointer>(default(atomic.UnsafePointer));
internal static ref atomic.UnsafePointer buckhash => ref ((StaticBox<atomic.UnsafePointer>)Ꮡbuckhash).ValueSlot;                    // *buckhashArray

internal static readonly ж<mProfCycleHolder> ᏑmProfCycle = new StaticBox<mProfCycleHolder>(default(mProfCycleHolder));
internal static ref mProfCycleHolder mProfCycle => ref ((StaticBox<mProfCycleHolder>)ᏑmProfCycle).ValueSlot;

/* [buckHashSize]@internal.runtime.atomic_package.UnsafePointer */
partial struct buckhashArray /*[179999]global::go.@internal.runtime.atomic_package.UnsafePointer*/; // *bucket

internal const uint32 mProfCycleWrap = /* uint32(len(memRecord{}.future)) * (2 << 24) */ 100663296;

// mProfCycleHolder holds the global heap profile cycle number (wrapped at
// mProfCycleWrap, stored starting at bit 1), and a flag (stored at bit 0) to
// indicate whether future[cycle] in all buckets has been queued to flush into
// the active profile.
partial struct mProfCycleHolder {
    internal atomic.Uint32 value;
}

// read returns the current cycle count.
internal static uint32 /*cycle*/ read(this ж<mProfCycleHolder> Ꮡc) {
    uint32 cycle = default!;

    var v = Ꮡc.of(mProfCycleHolder.Ꮡvalue).Load();
    cycle = v >> 1;
    return cycle;
}

// setFlushed sets the flushed flag. It returns the current cycle count and the
// previous value of the flushed flag.
internal static (uint32 cycle, bool alreadyFlushed) setFlushed(this ж<mProfCycleHolder> Ꮡc) {
    uint32 cycle = default!;
    bool alreadyFlushed = default!;

    while (ᐧ) {
        var prev = Ꮡc.of(mProfCycleHolder.Ꮡvalue).Load();
        cycle = prev >> 1;
        alreadyFlushed = (prev & 0x1) != 0;
        var next = prev | 0x1;
        if (Ꮡc.of(mProfCycleHolder.Ꮡvalue).CompareAndSwap(prev, next)) {
            return (cycle, alreadyFlushed);
        }
    }
}

// increment increases the cycle count by one, wrapping the value at
// mProfCycleWrap. It clears the flushed flag.
internal static void increment(this ж<mProfCycleHolder> Ꮡc) {
    // We explicitly wrap mProfCycle rather than depending on
    // uint wraparound because the memRecord.future ring does not
    // itself wrap at a power of two.
    while (ᐧ) {
        var prev = Ꮡc.of(mProfCycleHolder.Ꮡvalue).Load();
        var cycle = prev >> 1;
        cycle = (cycle + 1) % mProfCycleWrap;
        var next = cycle << 1;
        if (Ꮡc.of(mProfCycleHolder.Ꮡvalue).CompareAndSwap(prev, next)) {
            break;
        }
    }
}

// go2cs generated this placeholder — func newBucket is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func stk is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func mp is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func bp is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func stkbucket is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static bool eqslice(slice<uintptr> x, slice<uintptr> y) {
    if (len(x) != len(y)) {
        return false;
    }
    foreach (var (i, xi) in x) {
        if (xi != y[i]) {
            return false;
        }
    }
    return true;
}

// mProf_NextCycle publishes the next heap profile cycle and creates a
// fresh heap profile cycle. This operation is fast and can be done
// during STW. The caller must call mProf_Flush before calling
// mProf_NextCycle again.
//
// This is called by mark termination during STW so allocations and
// frees after the world is started again count towards a new heap
// profiling cycle.
internal static void mProf_NextCycle() {
    ᏑmProfCycle.increment();
}

// mProf_Flush flushes the events from the current heap profiling
// cycle into the active profile. After this it is safe to start a new
// heap profiling cycle with mProf_NextCycle.
//
// This is called by GC after mark termination starts the world. In
// contrast with mProf_NextCycle, this is somewhat expensive, but safe
// to do concurrently.
internal static void mProf_Flush() {
    var (cycle, alreadyFlushed) = ᏑmProfCycle.setFlushed();
    if (alreadyFlushed) {
        return;
    }

    var index = cycle % (uint32)len(new memRecord(nil).future);
    @lock(ᏑprofMemActiveLock);
    @lock(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));
    mProf_FlushLocked(index);
    unlock(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));
    unlock(ᏑprofMemActiveLock);
}

// mProf_FlushLocked flushes the events from the heap profiling cycle at index
// into the active profile. The caller must hold the lock for the active profile
// (profMemActiveLock) and for the profiling cycle at index
// (profMemFutureLock[index]).
internal static void mProf_FlushLocked(uint32 index) {
    assertLockHeld(ᏑprofMemActiveLock);
    assertLockHeld(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));
    var head = (ж<bucket>)(uintptr)(Ꮡmbuckets.Load());
    for (var b = head; b != nil; b = b.Value.allnext) {
        var mp = b.mp();

        // Flush cycle C into the published profile and clear
        // it for reuse.
        var mpc = mp.at(memRecord.Ꮡfuture, (ulong)(index));
        mp.of(memRecord.Ꮡactive).add(mpc);
        mpc.Value = new memRecordCycle(nil);
    }
}

// mProf_PostSweep records that all sweep frees for this GC cycle have
// completed. This has the effect of publishing the heap profile
// snapshot as of the last mark termination without advancing the heap
// profile cycle.
internal static void mProf_PostSweep() {
    // Flush cycle C+1 to the active profile so everything as of
    // the last mark termination becomes visible. *Don't* advance
    // the cycle, since we're still accumulating allocs in cycle
    // C+2, which have to become C+1 in the next mark termination
    // and so on.
    var cycle = ᏑmProfCycle.read() + 1;

    var index = cycle % (uint32)len(new memRecord(nil).future);
    @lock(ᏑprofMemActiveLock);
    @lock(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));
    mProf_FlushLocked(index);
    unlock(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));
    unlock(ᏑprofMemActiveLock);
}

// Called by malloc to record a profiled block.
internal static partial void mProf_Malloc(ref m mp, @unsafe.Pointer Δp, uintptr size) {
    if (mp.profStack == default!) {
        // mp.profStack is nil if we happen to sample an allocation during the
        // initialization of mp. This case is rare, so we just ignore such
        // allocations. Change MemProfileRate to 1 if you need to reproduce such
        // cases for testing purposes.
        return;
    }
    // Only use the part of mp.profStack we need and ignore the extra space
    // reserved for delayed inline expansion with frame pointer unwinding.
    nint nstk = callers(3, mp.profStack.slice(0, debug.profstackdepth + 2));
    var index = (ᏑmProfCycle.read() + 2) % (uint32)len(new memRecord(nil).future);

    var b = stkbucket(memProfile, size, mp.profStack.slice(0, nstk), true);
    var mr = b.mp();
    var mpc = mr.at(memRecord.Ꮡfuture, (ulong)(index));

    @lock(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));
    mpc.Value.allocs++;
    mpc.Value.alloc_bytes += size;
    unlock(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));

    // Setprofilebucket locks a bunch of other mutexes, so we call it outside of
    // the profiler locks. This reduces potential contention and chances of
    // deadlocks. Since the object must be alive during the call to
    // mProf_Malloc, it's fine to do this non-atomically.
    var bʗ1 = b;
    systemstack(() => {
        setprofilebucket(Δp, bʗ1);
    });
}

// Called when freeing a profiled block.
internal static void mProf_Free(ж<bucket> Ꮡb, uintptr size) {
    var index = (ᏑmProfCycle.read() + 1) % (uint32)len(new memRecord(nil).future);

    var mp = Ꮡb.mp();
    var mpc = mp.at(memRecord.Ꮡfuture, (ulong)(index));

    @lock(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));
    mpc.Value.frees++;
    mpc.Value.free_bytes += size;
    unlock(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));
}

internal static readonly ж<uint64> Ꮡblockprofilerate = new StaticBox<uint64>(default(uint64));
internal static ref uint64 blockprofilerate => ref ((StaticBox<uint64>)Ꮡblockprofilerate).ValueSlot; // in CPU ticks

// SetBlockProfileRate controls the fraction of goroutine blocking events
// that are reported in the blocking profile. The profiler aims to sample
// an average of one blocking event per rate nanoseconds spent blocked.
//
// To include every blocking event in the profile, pass rate = 1.
// To turn off profiling entirely, pass rate <= 0.
public static void SetBlockProfileRate(nint rate) {
    int64 r = default!;
    if (rate <= 0){
        r = 0; // disable profiling
    } else 
    if (rate == 1){
        r = 1; // profile everything
    } else {
        // convert ns to cycles, use float64 to prevent overflow during multiplication
        r = (int64)((float64)rate * (float64)ticksPerSecond() / (1000D * 1000D * 1000D));
        if (r == 0) {
            r = 1;
        }
    }

    atomic.Store64(Ꮡblockprofilerate, (uint64)r);
}

public static partial void blockevent(int64 cycles, nint skip) {
    if (cycles <= 0) {
        cycles = 1;
    }

    var rate = (int64)atomic.Load64(Ꮡblockprofilerate);
    if (blocksampled(cycles, rate)) {
        saveblockevent(cycles, rate, skip + 1, blockProfile);
    }
}

// blocksampled returns true for all events where cycles >= rate. Shorter
// events have a cycles/rate random chance of returning true.
internal static bool blocksampled(int64 cycles, int64 rate) {
    if (rate <= 0 || (rate > cycles && rem(cheaprand64(), rate) > cycles)) {
        return false;
    }
    return true;
}

// go2cs generated this placeholder — func saveblockevent is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// fpTracebackPartialExpand records a call stack obtained starting from fp.
// This function will skip the given number of frames, properly accounting for
// inlining, and save remaining frames as "physical" return addresses. The
// consumer should later use CallersFrames or similar to expand inline frames.
internal static nint fpTracebackPartialExpand(nint skip, @unsafe.Pointer fp, slice<uintptr> pcBuf) {
    nint n = default!;
    var lastFuncID = abi.FuncIDNormal;
    var pcBufʗ1 = pcBuf;
    bool skipOrAdd(uintptr retPC) {
        if (skip > 0){
            skip--;
        } else 
        if (n < len(pcBufʗ1)) {
            pcBufʗ1[n] = retPC;
            n++;
        }
        return n < len(pcBufʗ1);
    }
    while (n < len(pcBuf) && fp != nil) {
        // return addr sits one word above the frame pointer
        var pc = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)((uintptr)fp + (uintptr)goarch.PtrSize));

        if (skip > 0){
            var callPC = pc - 1;
            var fi = findfunc(callPC);
            var (u, uf) = newInlineUnwinder(fi, callPC);
            for (; uf.valid(); uf = u.next(uf)) {
                var sf = u.srcFunc(uf);
                if (sf.funcID == abi.FuncIDWrapper && elideWrapperCalling(lastFuncID)){
                    // ignore wrappers
                } else 
                {
                    var more = skipOrAdd(uf.pc + 1); if (!more) {
                        return n;
                    }
                }
                lastFuncID = sf.funcID;
            }
        } else {
            // We've skipped the desired number of frames, so no need
            // to perform further inline expansion now.
            pcBuf[n] = pc;
            n++;
        }

        // follow the frame pointer to the next one
        fp = (@unsafe.Pointer)(~(ж<uintptr>)(uintptr)(fp));
    }
    return n;
}

// mLockProfile holds information about the runtime-internal lock contention
// experienced and caused by this M, to report in metrics and profiles.
//
// These measurements are subject to some notable constraints: First, the fast
// path for lock and unlock must remain very fast, with a minimal critical
// section. Second, the critical section during contention has to remain small
// too, so low levels of contention are less likely to snowball into large ones.
// The reporting code cannot acquire new locks until the M has released all
// other locks, which means no memory allocations and encourages use of
// (temporary) M-local storage.
//
// The M has space for storing one call stack that caused contention, and the
// magnitude of that contention. It also has space to store the magnitude of
// additional contention the M caused, since it might encounter several
// contention events before it releases all of its locks and is thus able to
// transfer the locally buffered call stack and magnitude into the profile.
//
// The M collects the call stack when it unlocks the contended lock. The
// traceback takes place outside of the lock's critical section.
//
// The profile for contention on sync.Mutex blames the caller of Unlock for the
// amount of contention experienced by the callers of Lock which had to wait.
// When there are several critical sections, this allows identifying which of
// them is responsible. We must match that reporting behavior for contention on
// runtime-internal locks.
//
// When the M unlocks its last mutex, it transfers the locally buffered call
// stack and magnitude into the profile. As part of that step, it also transfers
// any "additional contention" time to the profile. Any lock contention that it
// experiences while adding samples to the profile will be recorded later as
// "additional contention" and not include a call stack, to avoid an echo.
partial struct mLockProfile {
    internal atomic.Int64 waitTime; // (nanotime) total time this M has spent waiting in runtime.lockWithRank. Read by runtime/metrics.
    internal slice<uintptr> stack; // call stack at the point of this M's unlock call, when other Ms had to wait
    internal int64 cycles;        // (cputicks) cycles attributable to "stack"
    internal int64 cyclesLost;        // (cputicks) contention for which we weren't able to record a call stack
    internal bool haveStack;         // stack and cycles are to be added to the mutex profile (even if cycles is 0)
    internal bool disabled;         // attribute all time to "lost"
}

internal static int64 start(this ref mLockProfile prof) {
    if (cheaprandn(gTrackingPeriod) == 0) {
        return nanotime();
    }
    return 0;
}

internal static void end(this ж<mLockProfile> Ꮡprof, int64 start) {
    if (start != 0) {
        Ꮡprof.of(mLockProfile.ᏑwaitTime).Add((nanotime() - start) * (int64)gTrackingPeriod);
    }
}

// recordUnlock prepares data for later addition to the mutex contention
// profile. The M may hold arbitrary locks during this call.
//
// From unlock2, we might not be holding a p in this code.
//
//go:nowritebarrierrec
internal static void recordUnlock(this ж<mLockProfile> Ꮡprof, int64 cycles) {
    ref var prof = ref Ꮡprof.DerefOrNull();

    if (cycles < 0) {
        cycles = 0;
    }

    if (prof.disabled) {
        // We're experiencing contention while attempting to report contention.
        // Make a note of its magnitude, but don't allow it to be the sole cause
        // of another contention report.
        prof.cyclesLost += cycles;
        return;
    }

    {
        var prev = prof.cycles; if (prev > 0) {
            // We can only store one call stack for runtime-internal lock contention
            // on this M, and we've already got one. Decide which should stay, and
            // add the other to the report for runtime._LostContendedRuntimeLock.
            if (cycles == 0) {
                return;
            }
            var prevScore = (uint64)cheaprand64() % (uint64)prev;
            var thisScore = (uint64)cheaprand64() % (uint64)cycles;
            if (prevScore > thisScore){
                prof.cyclesLost += cycles;
                return;
            } else {
                prof.cyclesLost += prev;
            }
        }
    }
    Ꮡprof.captureStack();
    prof.cycles = cycles;
}

internal static void captureStack(this ж<mLockProfile> Ꮡprof) {
    ref var prof = ref Ꮡprof.DerefOrNull();

    if (debug.profstackdepth == 0) {
        // profstackdepth is set to 0 by the user, so mp.profStack is nil and we
        // can't record a stack trace.
        return;
    }

    nint skip = 4; // runtime.(*mLockProfile).recordUnlock runtime.unlock2Wake runtime.unlock2 runtime.unlockWithRank
    if (staticLockRanking) {
        // When static lock ranking is enabled, we'll always be on the system
        // stack at this point. There will be a runtime.unlockWithRank.func1
        // frame, and if the call to runtime.unlock took place on a user stack
        // then there'll also be a runtime.systemstack frame. To keep stack
        // traces somewhat consistent whether or not static lock ranking is
        // enabled, we'd like to skip those. But it's hard to tell how long
        // we've been on the system stack so accept an extra frame in that case,
        // with a leaf of "runtime.unlockWithRank runtime.unlock" instead of
        // "runtime.unlock".
        skip += 1; // runtime.unlockWithRank.func1
    }
    prof.haveStack = true;

    prof.stack[0] = logicalStackSentinel;

    nint nstk = default!;
    var gp = getg();
    var sp = sys.GetCallerSP();
    var pc = sys.GetCallerPC();
    var gpʗ1 = gp;
    systemstack(() => {
        ref var u = ref heap(new unwinder(), out var Ꮡu);
        Ꮡu.initAt(pc, sp, 0, gpʗ1, (unwindFlags)(unwindSilentErrors | unwindJumpStack));
        nstk = 1 + tracebackPCs(Ꮡu, skip, Ꮡprof.Value.stack[1..]);
    });
    if (nstk < len(prof.stack)) {
        prof.stack[nstk] = 0;
    }
}

// store adds the M's local record to the mutex contention profile.
//
// From unlock2, we might not be holding a p in this code.
//
//go:nowritebarrierrec
internal static void store(this ref mLockProfile prof) {
    {
        var gp = getg(); if ((~(~gp).m).locks == 1 && (~(~gp).m).mLockProfile.haveStack) {
            prof.storeSlow();
        }
    }
}

internal static void storeSlow(this ref mLockProfile prof) {
    // Report any contention we experience within this function as "lost"; it's
    // important that the act of reporting a contention event not lead to a
    // reportable contention event. This also means we can use prof.stack
    // without copying, since it won't change during this function.
    var mp = acquirem();
    prof.disabled = true;

    nint nstk = (nint)debug.profstackdepth;
    for (nint i = 0; i < nstk; i++) {
        {
            var pc = prof.stack[i]; if (pc == 0) {
                nstk = i;
                break;
            }
        }
    }

    var (cycles, lost) = (prof.cycles, prof.cyclesLost);
    (prof.cycles, prof.cyclesLost) = (0, 0);
    prof.haveStack = false;

    var rate = (int64)atomic.Load64(Ꮡmutexprofilerate);
    saveBlockEventStack(cycles, rate, prof.stack.slice(0, nstk), mutexProfile);
    if (lost > 0) {
        var lostStk = new uintptr[]{
            logicalStackSentinel,
            abi.FuncPCABIInternal(_LostContendedRuntimeLock) + (uintptr)sys.PCQuantum
        }.array();
        saveBlockEventStack(lost, rate, lostStk[..], mutexProfile);
    }

    prof.disabled = false;
    releasem(ref (mp).DerefOrNull());
}

internal static void saveBlockEventStack(int64 cycles, int64 rate, slice<uintptr> stk, bucketType which) {
    var b = stkbucket(which, 0, stk, true);
    var bp = b.bp();

    @lock(ᏑprofBlockLock);
    // We want to up-scale the count and cycles according to the
    // probability that the event was sampled. For block profile events,
    // the sample probability is 1 if cycles >= rate, and cycles / rate
    // otherwise. For mutex profile events, the sample probability is 1 / rate.
    // We scale the events by 1 / (probability the event was sampled).
    if (which == blockProfile && cycles < rate){
        // Remove sampling bias, see discussion on http://golang.org/cl/299991.
        bp.Value.count += (float64)rate / (float64)cycles;
        bp.Value.cycles += rate;
    } else 
    if (which == mutexProfile){
        bp.Value.count += (float64)rate;
        bp.Value.cycles += rate * cycles;
    } else {
        bp.Value.count++;
        bp.Value.cycles += cycles;
    }
    unlock(ᏑprofBlockLock);
}

internal static readonly ж<uint64> Ꮡmutexprofilerate = new StaticBox<uint64>(default(uint64));
internal static ref uint64 mutexprofilerate => ref ((StaticBox<uint64>)Ꮡmutexprofilerate).ValueSlot; // fraction sampled

// SetMutexProfileFraction controls the fraction of mutex contention events
// that are reported in the mutex profile. On average 1/rate events are
// reported. The previous rate is returned.
//
// To turn off profiling entirely, pass rate 0.
// To just read the current rate, pass rate < 0.
// (For n>1 the details of sampling may change.)
public static nint SetMutexProfileFraction(nint rate) {
    if (rate < 0) {
        return (nint)mutexprofilerate;
    }
    var old = mutexprofilerate;
    atomic.Store64(Ꮡmutexprofilerate, (uint64)rate);
    return (nint)old;
}

//go:linkname mutexevent sync.event
internal static partial void mutexevent(int64 cycles, nint skip) {
    if (cycles < 0) {
        cycles = 0;
    }
    var rate = (int64)atomic.Load64(Ꮡmutexprofilerate);
    if (rate > 0 && rem(cheaprand64(), rate) == 0) {
        saveblockevent(cycles, rate, skip + 1, mutexProfile);
    }
}

// Go interface to profile data.

// A StackRecord describes a single execution stack.
partial struct StackRecord {
    public array<uintptr> Stack0 = new(32); // stack trace for this record; ends at first 0 entry
}

// Stack returns the stack trace associated with the record,
// a prefix of r.Stack0.
public static slice<uintptr> Stack(this ref StackRecord r) {
    foreach (var (i, v) in r.Stack0.ΔRangeSnapshot()) {
        if (v == 0) {
            return r.Stack0.slice(0, i);
        }
    }
    return r.Stack0[0..];
}

// MemProfileRate controls the fraction of memory allocations
// that are recorded and reported in the memory profile.
// The profiler aims to sample an average of
// one allocation per MemProfileRate bytes allocated.
//
// To include every allocated block in the profile, set MemProfileRate to 1.
// To turn off profiling entirely, set MemProfileRate to 0.
//
// The tools that process the memory profiles assume that the
// profile rate is constant across the lifetime of the program
// and equal to the current value. Programs that change the
// memory profiling rate should do so just once, as early as
// possible in the execution of the program (for example,
// at the beginning of main).
// go2cs generated this placeholder — var MemProfileRate is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// disableMemoryProfiling is set by the linker if memory profiling
// is not used and the link type guarantees nobody else could use it
// elsewhere.
// We check if the runtime.memProfileInternal symbol is present.
internal static bool disableMemoryProfiling;

// A MemProfileRecord describes the live objects allocated
// by a particular call sequence (stack trace).
partial struct MemProfileRecord {
    public int64 AllocBytes, FreeBytes;       // number of bytes allocated, freed
    public int64 AllocObjects, FreeObjects;       // number of objects allocated, freed
    public array<uintptr> Stack0 = new(32); // stack trace for this record; ends at first 0 entry
}

// InUseBytes returns the number of bytes in use (AllocBytes - FreeBytes).
public static int64 InUseBytes(this ref MemProfileRecord r) {
    return r.AllocBytes - r.FreeBytes;
}

// InUseObjects returns the number of objects in use (AllocObjects - FreeObjects).
public static int64 InUseObjects(this ref MemProfileRecord r) {
    return r.AllocObjects - r.FreeObjects;
}

// Stack returns the stack trace associated with the record,
// a prefix of r.Stack0.
public static slice<uintptr> Stack(this ref MemProfileRecord r) {
    foreach (var (i, v) in r.Stack0.ΔRangeSnapshot()) {
        if (v == 0) {
            return r.Stack0.slice(0, i);
        }
    }
    return r.Stack0[0..];
}

// MemProfile returns a profile of memory allocated and freed per allocation
// site.
//
// MemProfile returns n, the number of records in the current memory profile.
// If len(p) >= n, MemProfile copies the profile into p and returns n, true.
// If len(p) < n, MemProfile does not change p and returns n, false.
//
// If inuseZero is true, the profile includes allocation records
// where r.AllocBytes > 0 but r.AllocBytes == r.FreeBytes.
// These are sites where memory was allocated, but it has all
// been released back to the runtime.
//
// The returned profile may be up to two garbage collection cycles old.
// This is to avoid skewing the profile toward allocations; because
// allocations happen in real time but frees are delayed until the garbage
// collector performs sweeping, the profile only accounts for allocations
// that have had a chance to be freed by the garbage collector.
//
// Most clients should use the runtime/pprof package or
// the testing package's -test.memprofile flag instead
// of calling MemProfile directly.
public static (nint n, bool ok) MemProfile(slice<MemProfileRecord> Δp, bool inuseZero) {
    return memProfileInternal(len(Δp), inuseZero, (profilerecord.MemProfileRecord r) => {
        copyMemProfileRecord(Ꮡ(Δp, 0), r);
        Δp = Δp[1..];
    });
}

// memProfileInternal returns the number of records n in the profile. If there
// are less than size records, copyFn is invoked for each record, and ok returns
// true.
//
// The linker set disableMemoryProfiling to true to disable memory profiling
// if this function is not reachable. Mark it noinline to ensure the symbol exists.
// (This function is big and normally not inlined anyway.)
// See also disableMemoryProfiling above and cmd/link/internal/ld/lib.go:linksetup.
//
//go:noinline
internal static partial (nint n, bool ok) memProfileInternal(nint size, bool inuseZero, Action<profilerecord.MemProfileRecord> copyFn) {
    nint n = default!;
    bool ok = default!;

    var cycle = ᏑmProfCycle.read();
    // If we're between mProf_NextCycle and mProf_Flush, take care
    // of flushing to the active profile so we only have to look
    // at the active profile below.
    var index = cycle % (uint32)len(new memRecord(nil).future);
    @lock(ᏑprofMemActiveLock);
    @lock(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));
    mProf_FlushLocked(index);
    unlock(ᏑprofMemFutureLock.at<mutex>((ulong)(index)));
    var clear = true;
    var head = (ж<bucket>)(uintptr)(Ꮡmbuckets.Load());
    for (var b = head; b != nil; b = b.Value.allnext) {
        var mp = b.mp();
        if (inuseZero || (~mp).active.alloc_bytes != (~mp).active.free_bytes) {
            n++;
        }
        if ((~mp).active.allocs != 0 || (~mp).active.frees != 0) {
            clear = false;
        }
    }
    if (clear) {
        // Absolutely no data, suggesting that a garbage collection
        // has not yet happened. In order to allow profiling when
        // garbage collection is disabled from the beginning of execution,
        // accumulate all of the cycles, and recount buckets.
        n = 0;
        for (var b = head; b != nil; b = b.Value.allnext) {
            var mp = b.mp();
            foreach (var c in range((~mp).future)) {
                @lock(ᏑprofMemFutureLock.at<mutex>(c));
                mp.of(memRecord.Ꮡactive).add(mp.at(memRecord.Ꮡfuture, c));
                mp.Value.future[c] = new memRecordCycle(nil);
                unlock(ᏑprofMemFutureLock.at<mutex>(c));
            }
            if (inuseZero || (~mp).active.alloc_bytes != (~mp).active.free_bytes) {
                n++;
            }
        }
    }
    if (n <= size) {
        ok = true;
        for (var b = head; b != nil; b = b.Value.allnext) {
            var mp = b.mp();
            if (inuseZero || (~mp).active.alloc_bytes != (~mp).active.free_bytes) {
                var r = new profilerecord.MemProfileRecord(
                    AllocBytes: (int64)(~mp).active.alloc_bytes,
                    FreeBytes: (int64)(~mp).active.free_bytes,
                    AllocObjects: (int64)(~mp).active.allocs,
                    FreeObjects: (int64)(~mp).active.frees,
                    Stack: b.stk()
                );
                copyFn(r);
            }
        }
    }
    unlock(ᏑprofMemActiveLock);
    return (n, ok);
}

internal static void copyMemProfileRecord(ж<MemProfileRecord> Ꮡdst, profilerecord.MemProfileRecord src) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    dst.AllocBytes = src.AllocBytes;
    dst.FreeBytes = src.FreeBytes;
    dst.AllocObjects = src.AllocObjects;
    dst.FreeObjects = src.FreeObjects;
    if (raceenabled) {
        racewriterangepc(@unsafe.Pointer.FromBox(Ꮡdst.at(MemProfileRecord.ᏑStack0, 0)), /* unsafe.Sizeof(dst.Stack0) */ (uintptr)256, sys.GetCallerPC(), abi.FuncPCABIInternal(MemProfile));
    }
    if (msanenabled) {
        msanwrite(@unsafe.Pointer.FromBox(Ꮡdst.at(MemProfileRecord.ᏑStack0, 0)), /* unsafe.Sizeof(dst.Stack0) */ (uintptr)256);
    }
    if (asanenabled) {
        asanwrite(@unsafe.Pointer.FromBox(Ꮡdst.at(MemProfileRecord.ᏑStack0, 0)), /* unsafe.Sizeof(dst.Stack0) */ (uintptr)256);
    }
    nint i = copy(dst.Stack0[..], src.Stack);
    builtin.clear(dst.Stack0.slice(i));
}

//go:linkname pprof_memProfileInternal
public static (nint n, bool ok) pprof_memProfileInternal(slice<profilerecord.MemProfileRecord> Δp, bool inuseZero) {
    return memProfileInternal(len(Δp), inuseZero, (profilerecord.MemProfileRecord r) => {
        Δp[0] = r;
        Δp = Δp[1..];
    });
}

internal static void iterate_memprof(Action<ж<bucket>, uintptr, ж<uintptr>, uintptr, uintptr, uintptr> fn) {
    @lock(ᏑprofMemActiveLock);
    var head = (ж<bucket>)(uintptr)(Ꮡmbuckets.Load());
    for (var b = head; b != nil; b = b.Value.allnext) {
        var mp = b.mp();
        fn(b, (~b).nstk, Ꮡ(b.stk(), 0), (~b).size, (~mp).active.allocs, (~mp).active.frees);
    }
    unlock(ᏑprofMemActiveLock);
}

// BlockProfileRecord describes blocking events originated
// at a particular call sequence (stack trace).
partial struct BlockProfileRecord {
    public int64 Count;
    public int64 Cycles;
    /*embed*/ public StackRecord StackRecord;
}

// BlockProfile returns n, the number of records in the current blocking profile.
// If len(p) >= n, BlockProfile copies the profile into p and returns n, true.
// If len(p) < n, BlockProfile does not change p and returns n, false.
//
// Most clients should use the [runtime/pprof] package or
// the [testing] package's -test.blockprofile flag instead
// of calling BlockProfile directly.
public static (nint n, bool ok) BlockProfile(slice<BlockProfileRecord> Δp) {
    nint n = default!;
    bool ok = default!;

    nint m = default!;
    var pʗ1 = Δp;
    (n, ok) = blockProfileInternal(len(Δp), (profilerecord.BlockProfileRecord r) => {
        copyBlockProfileRecord(Ꮡ(pʗ1, m), r);
        m++;
    });
    if (ok) {
        expandFrames(Δp.slice(0, n));
    }
    return (n, ok);
}

internal static partial void expandFrames(slice<BlockProfileRecord> Δp) {
    var expandedStack = makeProfStack();
    foreach (var i in range(Δp)) {
        var cf = CallersFrames(Δp[i].StackRecord.Stack());
        nint j = 0;
        while (j < len(expandedStack)) {
            var (f, more) = cf.Next();
            // f.PC is a "call PC", but later consumers will expect
            // "return PCs"
            expandedStack[j] = f.PC + 1;
            j++;
            if (!more) {
                break;
            }
        }
        nint k = copy(Δp[i].Stack0[..], expandedStack.slice(0, j));
        builtin.clear(Δp[i].Stack0.slice(k));
    }
}

// blockProfileInternal returns the number of records n in the profile. If there
// are less than size records, copyFn is invoked for each record, and ok returns
// true.
internal static (nint n, bool ok) blockProfileInternal(nint size, Action<profilerecord.BlockProfileRecord> copyFn) {
    nint n = default!;
    bool ok = default!;

    @lock(ᏑprofBlockLock);
    var head = (ж<bucket>)(uintptr)(Ꮡbbuckets.Load());
    for (var b = head; b != nil; b = b.Value.allnext) {
        n++;
    }
    if (n <= size) {
        ok = true;
        for (var b = head; b != nil; b = b.Value.allnext) {
            var bp = b.bp();
            var r = new profilerecord.BlockProfileRecord(
                Count: (int64)(~bp).count,
                Cycles: (~bp).cycles,
                Stack: b.stk()
            );
            // Prevent callers from having to worry about division by zero errors.
            // See discussion on http://golang.org/cl/299991.
            if (r.Count == 0) {
                r.Count = 1;
            }
            copyFn(r);
        }
    }
    unlock(ᏑprofBlockLock);
    return (n, ok);
}

// copyBlockProfileRecord copies the sample values and call stack from src to dst.
// The call stack is copied as-is. The caller is responsible for handling inline
// expansion, needed when the call stack was collected with frame pointer unwinding.
internal static void copyBlockProfileRecord(ж<BlockProfileRecord> Ꮡdst, profilerecord.BlockProfileRecord src) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    dst.Count = src.Count;
    dst.Cycles = src.Cycles;
    if (raceenabled) {
        racewriterangepc(@unsafe.Pointer.FromBox(Ꮡdst.at(BlockProfileRecord.ᏑStack0, 0)), /* unsafe.Sizeof(dst.Stack0) */ (uintptr)256, sys.GetCallerPC(), abi.FuncPCABIInternal(BlockProfile));
    }
    if (msanenabled) {
        msanwrite(@unsafe.Pointer.FromBox(Ꮡdst.at(BlockProfileRecord.ᏑStack0, 0)), /* unsafe.Sizeof(dst.Stack0) */ (uintptr)256);
    }
    if (asanenabled) {
        asanwrite(@unsafe.Pointer.FromBox(Ꮡdst.at(BlockProfileRecord.ᏑStack0, 0)), /* unsafe.Sizeof(dst.Stack0) */ (uintptr)256);
    }
    // We just copy the stack here without inline expansion
    // (needed if frame pointer unwinding is used)
    // since this function is called under the profile lock,
    // and doing something that might allocate can violate lock ordering.
    nint i = copy(dst.Stack0[..], src.Stack);
    builtin.clear(dst.Stack0.slice(i));
}

//go:linkname pprof_blockProfileInternal
public static (nint n, bool ok) pprof_blockProfileInternal(slice<profilerecord.BlockProfileRecord> Δp) {
    return blockProfileInternal(len(Δp), (profilerecord.BlockProfileRecord r) => {
        Δp[0] = r;
        Δp = Δp[1..];
    });
}

// MutexProfile returns n, the number of records in the current mutex profile.
// If len(p) >= n, MutexProfile copies the profile into p and returns n, true.
// Otherwise, MutexProfile does not change p, and returns n, false.
//
// Most clients should use the [runtime/pprof] package
// instead of calling MutexProfile directly.
public static (nint n, bool ok) MutexProfile(slice<BlockProfileRecord> Δp) {
    nint n = default!;
    bool ok = default!;

    nint m = default!;
    var pʗ1 = Δp;
    (n, ok) = mutexProfileInternal(len(Δp), (profilerecord.BlockProfileRecord r) => {
        copyBlockProfileRecord(Ꮡ(pʗ1, m), r);
        m++;
    });
    if (ok) {
        expandFrames(Δp.slice(0, n));
    }
    return (n, ok);
}

// mutexProfileInternal returns the number of records n in the profile. If there
// are less than size records, copyFn is invoked for each record, and ok returns
// true.
internal static (nint n, bool ok) mutexProfileInternal(nint size, Action<profilerecord.BlockProfileRecord> copyFn) {
    nint n = default!;
    bool ok = default!;

    @lock(ᏑprofBlockLock);
    var head = (ж<bucket>)(uintptr)(Ꮡxbuckets.Load());
    for (var b = head; b != nil; b = b.Value.allnext) {
        n++;
    }
    if (n <= size) {
        ok = true;
        for (var b = head; b != nil; b = b.Value.allnext) {
            var bp = b.bp();
            var r = new profilerecord.BlockProfileRecord(
                Count: (int64)(~bp).count,
                Cycles: (~bp).cycles,
                Stack: b.stk()
            );
            copyFn(r);
        }
    }
    unlock(ᏑprofBlockLock);
    return (n, ok);
}

//go:linkname pprof_mutexProfileInternal
public static (nint n, bool ok) pprof_mutexProfileInternal(slice<profilerecord.BlockProfileRecord> Δp) {
    return mutexProfileInternal(len(Δp), (profilerecord.BlockProfileRecord r) => {
        Δp[0] = r;
        Δp = Δp[1..];
    });
}

// ThreadCreateProfile returns n, the number of records in the thread creation profile.
// If len(p) >= n, ThreadCreateProfile copies the profile into p and returns n, true.
// If len(p) < n, ThreadCreateProfile does not change p and returns n, false.
//
// Most clients should use the runtime/pprof package instead
// of calling ThreadCreateProfile directly.
public static (nint n, bool ok) ThreadCreateProfile(slice<StackRecord> Δp) {
    return threadCreateProfileInternal(len(Δp), (profilerecord.StackRecord r) => {
        nint i = copy(Δp[0].Stack0[..], r.Stack);
        builtin.clear(Δp[0].Stack0.slice(i));
        Δp = Δp[1..];
    });
}

// threadCreateProfileInternal returns the number of records n in the profile.
// If there are less than size records, copyFn is invoked for each record, and
// ok returns true.
internal static (nint n, bool ok) threadCreateProfileInternal(nint size, Action<profilerecord.StackRecord> copyFn) {
    nint n = default!;
    bool ok = default!;

    var first = (ж<m>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(Ꮡallm)));
    for (var mp = first; mp != nil; mp = mp.Value.alllink) {
        n++;
    }
    if (n <= size) {
        ok = true;
        for (var mp = first; mp != nil; mp = mp.Value.alllink) {
            var r = new profilerecord.StackRecord(Stack: (~mp).createstack[..]);
            copyFn(r);
        }
    }
    return (n, ok);
}

//go:linkname pprof_threadCreateInternal
public static (nint n, bool ok) pprof_threadCreateInternal(slice<profilerecord.StackRecord> Δp) {
    return threadCreateProfileInternal(len(Δp), (profilerecord.StackRecord r) => {
        Δp[0] = r;
        Δp = Δp[1..];
    });
}

//go:linkname pprof_goroutineProfileWithLabels
public static (nint n, bool ok) pprof_goroutineProfileWithLabels(slice<profilerecord.StackRecord> Δp, slice<@unsafe.Pointer> labels) {
    return goroutineProfileWithLabels(Δp, labels);
}

// go2cs generated this placeholder — func goroutineProfileWithLabels is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

//go:linkname pprof_goroutineLeakProfileWithLabels
internal static (nint n, bool ok) pprof_goroutineLeakProfileWithLabels(slice<profilerecord.StackRecord> Δp, slice<@unsafe.Pointer> labels) {
    return goroutineLeakProfileWithLabels(Δp, labels);
}

// labels may be nil. If labels is non-nil, it must have the same length as p.
internal static (nint n, bool ok) goroutineLeakProfileWithLabels(slice<profilerecord.StackRecord> Δp, slice<@unsafe.Pointer> labels) {
    if (labels != default! && len(labels) != len(Δp)) {
        labels = default!;
    }

    return goroutineLeakProfileWithLabelsConcurrent(Δp, labels);
}

partial struct goroutineProfileᴛ1 /*dyn*/ {
    internal uint32 sema;
    internal bool active;
    internal atomic.Int64 offset;
    internal slice<profilerecord.StackRecord> records;
    internal slice<@unsafe.Pointer> labels;
}
[GoLazy] partial class lazyᴛgoroutineProfile { internal static readonly ж<goroutineProfileᴛ1> ᏑgoroutineProfile = new StaticBox<goroutineProfileᴛ1>(new goroutineProfileᴛ1(
    sema: 1
)); }
internal static ref goroutineProfileᴛ1 goroutineProfile => ref ((StaticBox<goroutineProfileᴛ1>)ᏑgoroutineProfile).ValueSlot;

// goroutineProfileState indicates the status of a goroutine's stack for the
// current in-progress goroutine profile. Goroutines' stacks are initially
// "Absent" from the profile, and end up "Satisfied" by the time the profile is
// complete. While a goroutine's stack is being captured, its
// goroutineProfileState will be "InProgress" and it will not be able to run
// until the capture completes and the state moves to "Satisfied".
//
// Some goroutines (the finalizer goroutine, which at various times can be
// either a "system" or a "user" goroutine, and the goroutine that is
// coordinating the profile, any goroutines created during the profile) move
// directly to the "Satisfied" state.
partial struct goroutineProfileState /*num:uint32*/;

internal static goroutineProfileState goroutineProfileAbsent => /* iota */ 0;
internal static goroutineProfileState goroutineProfileInProgress => 1;
internal static goroutineProfileState goroutineProfileSatisfied => 2;

partial struct goroutineProfileStateHolder /*global::go.@internal.runtime.atomic_package.Uint32*/;

internal static goroutineProfileState Load(this ж<goroutineProfileStateHolder> Ꮡp) {
    return (goroutineProfileState)(Ꮡp.Reinterpret<goroutineProfileStateHolder, atomic.Uint32>()).Load();
}

internal static void Store(this ж<goroutineProfileStateHolder> Ꮡp, goroutineProfileState value) {
    (Ꮡp.Reinterpret<goroutineProfileStateHolder, atomic.Uint32>()).Store((uint32)value);
}

internal static bool CompareAndSwap(this ж<goroutineProfileStateHolder> Ꮡp, goroutineProfileState old, goroutineProfileState @new) {
    return (Ꮡp.Reinterpret<goroutineProfileStateHolder, atomic.Uint32>()).CompareAndSwap((uint32)old, (uint32)@new);
}

internal static (nint n, bool ok) goroutineLeakProfileWithLabelsConcurrent(slice<profilerecord.StackRecord> Δp, slice<@unsafe.Pointer> labels) {
    nint n = default!;

    if (len(Δp) == 0) {
        // An empty slice is obviously too small. Return a rough
        // allocation estimate.
        return (work.goroutineLeak.count, false);
    }

    var pcbuf = makeProfStack(); // see saveg() for explanation

    // Prepare a profile large enough to store all leaked goroutines.
    n = work.goroutineLeak.count;

    if (n > len(Δp)) {
        // There's not enough space in p to store the whole profile, so
        // we're not allowed to write to p at all and must return n, false.
        return (n, false);
    }

    // Visit each leaked goroutine and try to record its stack.
    nint offset = default!;
    var labelsʗ1 = labels;
    var pʗ1 = Δp;
    var pcbufʗ1 = pcbuf;
    forEachGRace((ж<g> gp1) => {
        if ((readgstatus(gp1) & ~(uint32)_Gscan) == _Gleaked) {
            var pʗ2 = pʗ1;
            var pcbufʗ2 = pcbufʗ1;
            systemstack(() => {
                saveg(~(uintptr)0, ~(uintptr)0, gp1, ref (Ꮡ(pʗ2, offset)).DerefOrNull(), pcbufʗ2);
            });
            if (labelsʗ1 != default!) {
                labelsʗ1[offset] = gp1.Value.labels;
            }
            offset++;
        }
    });

    if (raceenabled) {
        raceacquire(@unsafe.Pointer.FromBox(ᏑlabelSync));
    }

    return (n, true);
}

internal static (nint n, bool ok) goroutineProfileWithLabelsConcurrent(slice<profilerecord.StackRecord> Δp, slice<@unsafe.Pointer> labels) {
    nint n = default!;

    if (len(Δp) == 0) {
        // An empty slice is obviously too small. Return a rough
        // allocation estimate without bothering to STW. As long as
        // this is close, then we'll only need to STW once (on the next
        // call).
        return ((nint)gcount(false), false);
    }

    semacquire(ᏑgoroutineProfile.of(goroutineProfileᴛ1.Ꮡsema));

    var ourg = getg();

    var pcbuf = makeProfStack(); // see saveg() for explanation
    var stw = stopTheWorld(stwGoroutineProfile);
    // Using gcount while the world is stopped should give us a consistent view
    // of the number of live goroutines, minus the number of goroutines that are
    // alive and permanently marked as "system". But to make this count agree
    // with what we'd get from isSystemGoroutine, we need special handling for
    // goroutines that can vary between user and system to ensure that the count
    // doesn't change during the collection. So, check the finalizer goroutine
    // and cleanup goroutines in particular.
    n = (nint)gcount(false);
    if ((ᏑfingStatus.Load() & fingRunningFinalizer) != 0) {
        n++;
    }
    n += (nint)ᏑgcCleanups.of(cleanupQueue.Ꮡrunning).Load();

    if (n > len(Δp)) {
        // There's not enough space in p to store the whole profile, so (per the
        // contract of runtime.GoroutineProfile) we're not allowed to write to p
        // at all and must return n, false.
        startTheWorld(stw);
        semrelease(ᏑgoroutineProfile.of(goroutineProfileᴛ1.Ꮡsema));
        return (n, false);
    }

    // Save current goroutine.
    var sp = sys.GetCallerSP();
    var pc = sys.GetCallerPC();
    var ourgʗ1 = ourg;
    var pʗ1 = Δp;
    var pcbufʗ1 = pcbuf;
    systemstack(() => {
        saveg(pc, sp, ourgʗ1, ref (Ꮡ(pʗ1, 0)).DerefOrNull(), pcbufʗ1);
    });
    if (labels != default!) {
        labels[0] = ourg.Value.labels;
    }
    ourg.of(g.ᏑgoroutineProfiled).Store(goroutineProfileSatisfied);
    ᏑgoroutineProfile.of(goroutineProfileᴛ1.Ꮡoffset).Store(1);

    // Prepare for all other goroutines to enter the profile. Aside from ourg,
    // every goroutine struct in the allgs list has its goroutineProfiled field
    // cleared. Any goroutine created from this point on (while
    // goroutineProfile.active is set) will start with its goroutineProfiled
    // field set to goroutineProfileSatisfied.
    goroutineProfile.active = true;
    goroutineProfile.records = Δp;
    goroutineProfile.labels = labels;
    startTheWorld(stw);

    // Visit each goroutine that existed as of the startTheWorld call above.
    //
    // New goroutines may not be in this list, but we didn't want to know about
    // them anyway. If they do appear in this list (via reusing a dead goroutine
    // struct, or racing to launch between the world restarting and us getting
    // the list), they will already have their goroutineProfiled field set to
    // goroutineProfileSatisfied before their state transitions out of _Gdead.
    //
    // Any goroutine that the scheduler tries to execute concurrently with this
    // call will start by adding itself to the profile (before the act of
    // executing can cause any changes in its stack).
    var pcbufʗ2 = pcbuf;
    forEachGRace((ж<g> gp1) => {
        tryRecordGoroutineProfile(gp1, pcbufʗ2, Gosched);
    });

    stw = stopTheWorld(stwGoroutineProfileCleanup);
    var endOffset = ᏑgoroutineProfile.of(goroutineProfileᴛ1.Ꮡoffset).Swap(0);
    goroutineProfile.active = false;
    goroutineProfile.records = default!;
    goroutineProfile.labels = default!;
    startTheWorld(stw);

    // Restore the invariant that every goroutine struct in allgs has its
    // goroutineProfiled field cleared.
    forEachGRace((ж<g> gp1) => {
        gp1.of(g.ᏑgoroutineProfiled).Store(goroutineProfileAbsent);
    });

    if (raceenabled) {
        raceacquire(@unsafe.Pointer.FromBox(ᏑlabelSync));
    }

    if (n != (nint)endOffset) {
        // It's a big surprise that the number of goroutines changed while we
        // were collecting the profile. But probably better to return a
        // truncated profile than to crash the whole process.
        //
        // For instance, needm moves a goroutine out of the _Gdeadextra state and so
        // might be able to change the goroutine count without interacting with
        // the scheduler. For code like that, the race windows are small and the
        // combination of features is uncommon, so it's hard to be (and remain)
        // sure we've caught them all.
    }

    semrelease(ᏑgoroutineProfile.of(goroutineProfileᴛ1.Ꮡsema));
    return (n, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noPAvailableWriteˢ = "no P available, write barriers are forbidden"u8;

// tryRecordGoroutineProfileWB asserts that write barriers are allowed and calls
// tryRecordGoroutineProfile.
//
//go:yeswritebarrierrec
internal static void tryRecordGoroutineProfileWB(ж<g> Ꮡgp1) {
    if ((~(~getg()).m).p.ptr() == nil) {
        @throw(noPAvailableWriteˢ);
    }
    tryRecordGoroutineProfile(Ꮡgp1, default!, osyield);
}

// tryRecordGoroutineProfile ensures that gp1 has the appropriate representation
// in the current goroutine profile: either that it should not be profiled, or
// that a snapshot of its call stack and labels are now in the profile.
internal static void tryRecordGoroutineProfile(ж<g> Ꮡgp1, slice<uintptr> pcbuf, Action yield) {
    {
        var status = readgstatus(Ꮡgp1); if (status == _Gdead || status == _Gdeadextra) {
            // Dead goroutines should not appear in the profile. Goroutines that
            // start while profile collection is active will get goroutineProfiled
            // set to goroutineProfileSatisfied before transitioning out of _Gdead,
            // so here we check _Gdead first.
            return;
        }
    }

    while (ᐧ) {
        var prev = Ꮡgp1.of(g.ᏑgoroutineProfiled).Load();
        if (prev == goroutineProfileSatisfied) {
            // This goroutine is already in the profile (or is new since the
            // start of collection, so shouldn't appear in the profile).
            break;
        }
        if (prev == goroutineProfileInProgress) {
            // Something else is adding gp1 to the goroutine profile right now.
            // Give that a moment to finish.
            yield();
            continue;
        }

        // While we have gp1.goroutineProfiled set to
        // goroutineProfileInProgress, gp1 may appear _Grunnable but will not
        // actually be able to run. Disable preemption for ourselves, to make
        // sure we finish profiling gp1 right away instead of leaving it stuck
        // in this limbo.
        var mp = acquirem();
        if (Ꮡgp1.of(g.ᏑgoroutineProfiled).CompareAndSwap(goroutineProfileAbsent, goroutineProfileInProgress)) {
            doRecordGoroutineProfile(Ꮡgp1, pcbuf);
            Ꮡgp1.of(g.ᏑgoroutineProfiled).Store(goroutineProfileSatisfied);
        }
        releasem(ref (mp).DerefOrNull());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotReadStackOfRunningˢ = "cannot read stack of running goroutine"u8;

// doRecordGoroutineProfile writes gp1's call stack and labels to an in-progress
// goroutine profile. Preemption is disabled.
//
// This may be called via tryRecordGoroutineProfile in two ways: by the
// goroutine that is coordinating the goroutine profile (running on its own
// stack), or from the scheduler in preparation to execute gp1 (running on the
// system stack).
internal static void doRecordGoroutineProfile(ж<g> Ꮡgp1, slice<uintptr> pcbuf) {
    ref var gp1 = ref Ꮡgp1.DerefOrNull();

    if (isSystemGoroutine(Ꮡgp1, false)) {
        // System goroutines should not appear in the profile.
        // Check this here and not in tryRecordGoroutineProfile because isSystemGoroutine
        // may change on a goroutine while it is executing, so while the scheduler might
        // see a system goroutine, goroutineProfileWithLabelsConcurrent might not, and
        // this inconsistency could cause invariants to be violated, such as trying to
        // record the stack of a running goroutine below. In short, we still want system
        // goroutines to participate in the same state machine on gp1.goroutineProfiled as
        // everything else, we just don't record the stack in the profile.
        return;
    }
    // Double-check that we didn't make a grave mistake. If the G is running then in
    // general, we cannot safely read its stack.
    //
    // However, there is one case where it's OK. There's a small window of time in
    // exitsyscall where a goroutine could be in _Grunning as it's exiting a syscall.
    // This is OK because goroutine will not exit the syscall until it passes through
    // a call to tryRecordGoroutineProfile. (An explicit one on the fast path, an
    // implicit one via the scheduler on the slow path.)
    //
    // This is also why it's safe to check syscallsp here. The syscall path mutates
    // syscallsp only after passing through tryRecordGoroutineProfile.
    if (readgstatus(Ꮡgp1) == _Grunning && gp1.syscallsp == 0) {
        print((@string)"doRecordGoroutineProfile gp1="u8, gp1.goid, (@string)"\n"u8);
        @throw(cannotReadStackOfRunningˢ);
    }

    nint offset = (nint)ᏑgoroutineProfile.of(goroutineProfileᴛ1.Ꮡoffset).Add(1) - 1;

    if (offset >= len(goroutineProfile.records)) {
        // Should be impossible, but better to return a truncated profile than
        // to crash the entire process at this point. Instead, deal with it in
        // goroutineProfileWithLabelsConcurrent where we have more context.
        return;
    }

    // saveg calls gentraceback, which may call cgo traceback functions. When
    // called from the scheduler, this is on the system stack already so
    // traceback.go:cgoContextPCs will avoid calling back into the scheduler.
    //
    // When called from the goroutine coordinating the profile, we still have
    // set gp1.goroutineProfiled to goroutineProfileInProgress and so are still
    // preventing it from being truly _Grunnable. So we'll use the system stack
    // to avoid schedule delays.
    var pcbufʗ1 = pcbuf;
    systemstack(() => {
        saveg(~(uintptr)0, ~(uintptr)0, Ꮡgp1, ref (Ꮡ(goroutineProfile.records, offset)).DerefOrNull(), pcbufʗ1);
    });

    if (goroutineProfile.labels != default!) {
        goroutineProfile.labels[offset] = gp1.labels;
    }
}

internal static (nint n, bool ok) goroutineProfileWithLabelsSync(slice<profilerecord.StackRecord> Δp, slice<@unsafe.Pointer> labels) {
    nint n = default!;
    bool ok = default!;

    var gp = getg();

    var gpʗ1 = gp;
    bool isOK(ж<g> gp1) {
        // Checking isSystemGoroutine here makes GoroutineProfile
        // consistent with both NumGoroutine and Stack.
        if (gp1 == gpʗ1) {
            return false;
        }
        {
            var status = readgstatus(gp1); if (status == _Gdead || status == _Gdeadextra) {
                return false;
            }
        }
        if (isSystemGoroutine(gp1, false)) {
            return false;
        }
        return true;
    }

    var pcbuf = makeProfStack(); // see saveg() for explanation
    var stw = stopTheWorld(stwGoroutineProfile);

    // World is stopped, no locking required.
    n = 1;
    var isOKʗ1 = isOK;
    forEachGRace((ж<g> gp1) => {
        if (isOKʗ1(gp1)) {
            n++;
        }
    });

    if (n <= len(Δp)) {
        ok = true;
        ref var r = ref heap<slice<profilerecord.StackRecord>>(out var Ꮡr);
        r = Δp;
        ref var lbl = ref heap<slice<@unsafe.Pointer>>(out var Ꮡlbl);
        lbl = labels;

        // Save current goroutine.
        var sp = sys.GetCallerSP();
        var pc = sys.GetCallerPC();
        var gpʗ2 = gp;
        var pcbufʗ1 = pcbuf;
        systemstack(() => {
            saveg(pc, sp, gpʗ2, ref (Ꮡ(Ꮡr.ValueSlot, 0)).DerefOrNull(), pcbufʗ1);
        });
        r = r[1..];

        // If we have a place to put our goroutine labelmap, insert it there.
        if (labels != default!) {
            lbl[0] = gp.Value.labels;
            lbl = lbl[1..];
        }

        // Save other goroutines.
        var isOKʗ2 = isOK;
        var labelsʗ1 = labels;
        var pcbufʗ2 = pcbuf;
        forEachGRace((ж<g> gp1) => {
            if (!isOKʗ2(gp1)) {
                return;
            }

            if (len(Ꮡr.ValueSlot) == 0) {
                // Should be impossible, but better to return a
                // truncated profile than to crash the entire process.
                return;
            }
            // saveg calls gentraceback, which may call cgo traceback functions.
            // The world is stopped, so it cannot use cgocall (which will be
            // blocked at exitsyscall). Do it on the system stack so it won't
            // call into the schedular (see traceback.go:cgoContextPCs).
            var pcbufʗ3 = pcbufʗ2;
            systemstack(() => {
                saveg(~(uintptr)0, ~(uintptr)0, gp1, ref (Ꮡ(Ꮡr.ValueSlot, 0)).DerefOrNull(), pcbufʗ3);
            });
            if (labelsʗ1 != default!) {
                Ꮡlbl.ValueSlot[0] = gp1.Value.labels;
                Ꮡlbl.ValueSlot = Ꮡlbl.ValueSlot[1..];
            }
            Ꮡr.ValueSlot = Ꮡr.ValueSlot[1..];
        });
    }

    if (raceenabled) {
        raceacquire(@unsafe.Pointer.FromBox(ᏑlabelSync));
    }

    startTheWorld(stw);
    return (n, ok);
}

// GoroutineProfile returns n, the number of records in the active goroutine stack profile.
// If len(p) >= n, GoroutineProfile copies the profile into p and returns n, true.
// If len(p) < n, GoroutineProfile does not change p and returns n, false.
//
// Most clients should use the [runtime/pprof] package instead
// of calling GoroutineProfile directly.
public static (nint n, bool ok) GoroutineProfile(slice<StackRecord> Δp) {
    nint n = default!;
    bool ok = default!;

    var records = new slice<profilerecord.StackRecord>(len(Δp));
    (n, ok) = goroutineProfileInternal(records);
    if (!ok) {
        return (n, ok);
    }
    foreach (var (i, mr) in records.slice(0, n)) {
        nint l = copy(Δp[i].Stack0[..], mr.Stack);
        builtin.clear(Δp[i].Stack0.slice(l));
    }
    return (n, ok);
}

internal static (nint n, bool ok) goroutineProfileInternal(slice<profilerecord.StackRecord> Δp) {
    return goroutineProfileWithLabels(Δp, default!);
}

internal static void saveg(uintptr pc, uintptr sp, ж<g> Ꮡgp, ref profilerecord.StackRecord r, slice<uintptr> pcbuf) {
    // To reduce memory usage, we want to allocate a r.Stack that is just big
    // enough to hold gp's stack trace. Naively we might achieve this by
    // recording our stack trace into mp.profStack, and then allocating a
    // r.Stack of the right size. However, mp.profStack is also used for
    // allocation profiling, so it could get overwritten if the slice allocation
    // gets profiled. So instead we record the stack trace into a temporary
    // pcbuf which is usually given to us by our caller. When it's not, we have
    // to allocate one here. This will only happen for goroutines that were in a
    // syscall when the goroutine profile started or for goroutines that manage
    // to execute before we finish iterating over all the goroutines.
    if (pcbuf == default!) {
        pcbuf = makeProfStack();
    }

    ref var u = ref heap(new unwinder(), out var Ꮡu);
    Ꮡu.initAt(pc, sp, 0, Ꮡgp, unwindSilentErrors);
    nint n = tracebackPCs(Ꮡu, 0, pcbuf);
    r.Stack = new slice<uintptr>(n);
    copy(r.Stack, pcbuf);
}

// go2cs generated this placeholder — func Stack is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end runtime_package
