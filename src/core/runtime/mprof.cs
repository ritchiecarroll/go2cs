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
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// NOTE(rsc): Everything here could use cas if contention became an issue.
internal static ж<mutex> ᏑprofInsertLock = new StandardBox<mutex>(new mutex(nil));
internal static ref mutex profInsertLock => ref ᏑprofInsertLock.Value;

internal static ж<mutex> ᏑprofBlockLock = new StandardBox<mutex>(new mutex(nil));
internal static ref mutex profBlockLock => ref ᏑprofBlockLock.Value;

internal static ж<mutex> ᏑprofMemActiveLock = new StandardBox<mutex>(new mutex(nil));
internal static ref mutex profMemActiveLock => ref ᏑprofMemActiveLock.Value;

internal static ж<array<mutex>> ᏑprofMemFutureLock = new StandardBox<array<mutex>>(new array<mutex>(3, () => new(nil)));
internal static ref array<mutex> profMemFutureLock => ref ᏑprofMemFutureLock.Value;

// All memory allocations are local and do not escape outside of the profiler.
// The profiler is forbidden from referring to garbage-collected memory.
internal static bucketType memProfile => /* 1 + iota */ 1;
internal static bucketType blockProfile => 2;
internal static bucketType mutexProfile => 3;
internal static UntypedInt buckHashSize => 179999;
internal static UntypedInt maxSkip => 5;
internal static UntypedInt maxProfStackDepth => 1024;

[GoType("num:nint")] partial struct bucketType;

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
[GoType] partial struct bucket {
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
[GoType] partial struct memRecord {
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
[GoType] partial struct memRecordCycle {
    internal uintptr allocs, frees;
    internal uintptr alloc_bytes, free_bytes;
}

// add accumulates b into a. It does not zero b.
[GoRecv] internal static void add(this ref memRecordCycle a, ж<memRecordCycle> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    a.allocs += b.allocs;
    a.frees += b.frees;
    a.alloc_bytes += b.alloc_bytes;
    a.free_bytes += b.free_bytes;
}

// A blockRecord is the bucket data for a bucket of type blockProfile,
// which is used in blocking and mutex profiles.
[GoType] partial struct blockRecord {
    internal float64 count;
    internal int64 cycles;
}

internal static ж<atomic.UnsafePointer> Ꮡmbuckets = new StandardBox<atomic.UnsafePointer>(default(atomic.UnsafePointer));
internal static ref atomic.UnsafePointer mbuckets => ref Ꮡmbuckets.Value;                    // *bucket, memory profile buckets
internal static ж<atomic.UnsafePointer> Ꮡbbuckets = new StandardBox<atomic.UnsafePointer>(default(atomic.UnsafePointer));
internal static ref atomic.UnsafePointer bbuckets => ref Ꮡbbuckets.Value;                    // *bucket, blocking profile buckets
internal static ж<atomic.UnsafePointer> Ꮡxbuckets = new StandardBox<atomic.UnsafePointer>(default(atomic.UnsafePointer));
internal static ref atomic.UnsafePointer xbuckets => ref Ꮡxbuckets.Value;                    // *bucket, mutex profile buckets
internal static ж<atomic.UnsafePointer> Ꮡbuckhash = new StandardBox<atomic.UnsafePointer>(default(atomic.UnsafePointer));
internal static ref atomic.UnsafePointer buckhash => ref Ꮡbuckhash.Value;                    // *buckhashArray
internal static ж<mProfCycleHolder> ᏑmProfCycle = new StandardBox<mProfCycleHolder>(default(mProfCycleHolder));
internal static ref mProfCycleHolder mProfCycle => ref ᏑmProfCycle.Value;

[GoType("[179999]@internal.runtime.atomic_package.UnsafePointer")] /* [buckHashSize]@internal.runtime.atomic_package.UnsafePointer */
partial struct buckhashArray; // *bucket

internal const uint32 mProfCycleWrap = /* uint32(len(memRecord{}.future)) * (2 << 24) */ 100663296;

// mProfCycleHolder holds the global heap profile cycle number (wrapped at
// mProfCycleWrap, stored starting at bit 1), and a flag (stored at bit 0) to
// indicate whether future[cycle] in all buckets has been queued to flush into
// the active profile.
[GoType] partial struct mProfCycleHolder {
    internal atomic.Uint32 value;
}

// read returns the current cycle count.
internal static uint32 /*cycle*/ read(this ж<mProfCycleHolder> Ꮡc) {
    uint32 cycle = default!;

    var v = Ꮡc.of(mProfCycleHolder.Ꮡvalue).Load();
    cycle = (v >> (int)(1));
    return cycle;
}

// setFlushed sets the flushed flag. It returns the current cycle count and the
// previous value of the flushed flag.
internal static (uint32 cycle, bool alreadyFlushed) setFlushed(this ж<mProfCycleHolder> Ꮡc) {
    uint32 cycle = default!;
    bool alreadyFlushed = default!;

    while (ᐧ) {
        var prev = Ꮡc.of(mProfCycleHolder.Ꮡvalue).Load();
        cycle = (prev >> (int)(1));
        alreadyFlushed = ((uint32)(prev & 0x1)) != 0;
        var next = (uint32)(prev | 0x1);
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
        var cycle = (prev >> (int)(1));
        cycle = (cycle + 1) % mProfCycleWrap;
        var next = (cycle << (int)(1));
        if (Ꮡc.of(mProfCycleHolder.Ꮡvalue).CompareAndSwap(prev, next)) {
            break;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidProfileBucketTypeˢ = "invalid profile bucket type"u8;

// newBucket allocates a bucket with the given type and number of stack entries.
internal static ж<bucket> newBucket(bucketType typ, nint nstk) {
    var size = /* unsafe.Sizeof(bucket{}) */ (uintptr)48 + (uintptr)nstk * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8;
    var exprᴛ1 = typ;
    if (exprᴛ1 == memProfile) {
        size += /* unsafe.Sizeof(memRecord{}) */ (uintptr)128;
    }
    else if (exprᴛ1 == blockProfile || exprᴛ1 == mutexProfile) {
        size += /* unsafe.Sizeof(blockRecord{}) */ (uintptr)16;
    }
    else { /* default: */
        @throw(invalidProfileBucketTypeˢ);
    }

    var b = (ж<bucket>)(uintptr)(persistentalloc(size, 0, Ꮡmemstats.of(mstats.Ꮡbuckhash_sys)));
    b.Value.typ = typ;
    b.Value.nstk = (uintptr)nstk;
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badProfileStackCountˢ = "bad profile stack count"u8;

// stk returns the slice in b holding the stack. The caller can asssume that the
// backing array is immutable.
internal static slice<uintptr> stk(this ж<bucket> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var stk = (ж<array<uintptr>>)(uintptr)(add((uintptr)@unsafe.Pointer.FromRef(ref b), /* unsafe.Sizeof(*b) */ (uintptr)48));
    if (b.nstk > maxProfStackDepth) {
        // prove that slicing works; otherwise a failure requires a P
        @throw(badProfileStackCountˢ);
    }
    return (~stk).slice(-1, (int)(b.nstk), (int)(b.nstk));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badUseOfBucketMpˢ = "bad use of bucket.mp"u8;

// mp returns the memRecord associated with the memProfile bucket b.
internal static ж<memRecord> mp(this ж<bucket> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (b.typ != memProfile) {
        @throw(badUseOfBucketMpˢ);
    }
    @unsafe.Pointer data = (uintptr)add((uintptr)@unsafe.Pointer.FromRef(ref b), /* unsafe.Sizeof(*b) */ (uintptr)48 + b.nstk * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8);
    return (ж<memRecord>)(uintptr)(data);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badUseOfBucketBpˢ = "bad use of bucket.bp"u8;

// bp returns the blockRecord associated with the blockProfile bucket b.
internal static ж<blockRecord> bp(this ж<bucket> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (b.typ != blockProfile && b.typ != mutexProfile) {
        @throw(badUseOfBucketBpˢ);
    }
    @unsafe.Pointer data = (uintptr)add((uintptr)@unsafe.Pointer.FromRef(ref b), /* unsafe.Sizeof(*b) */ (uintptr)48 + b.nstk * /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8);
    return (ж<blockRecord>)(uintptr)(data);
}

// Return the bucket for stk[0:nstk], allocating new bucket if needed.
internal static ж<bucket> stkbucket(bucketType typ, uintptr size, slice<uintptr> stk, bool alloc) {
    var bh = (ж<buckhashArray>)(uintptr)(Ꮡbuckhash.Load());
    if (bh == nil) {
        @lock(ᏑprofInsertLock);
        // check again under the lock
        bh = (ж<buckhashArray>)(uintptr)(Ꮡbuckhash.Load());
        if (bh == nil) {
            bh = (ж<buckhashArray>)(uintptr)(sysAlloc(/* unsafe.Sizeof(buckhashArray{}) */ (uintptr)1439992, Ꮡmemstats.of(mstats.Ꮡbuckhash_sys)));
            if (bh == nil) {
                @throw(runtimeCannotAllocateˢ);
            }
            Ꮡbuckhash.StoreNoWB(new @unsafe.Pointer(bh));
        }
        unlock(ᏑprofInsertLock);
    }
    // Hash stack.
    uintptr h = default!;
    foreach (var (_, pc) in stk) {
        h += pc;
        h += (h << (int)(10));
        h ^= (uintptr)((h >> (int)(6)));
    }
    // hash in size
    h += size;
    h += (h << (int)(10));
    h ^= (uintptr)((h >> (int)(6)));
    // finalize
    h += (h << (int)(3));
    h ^= (uintptr)((h >> (int)(11)));
    nint i = (nint)(h % (uintptr)buckHashSize);
    // first check optimistically, without the lock
    for (var bΔ1 = (ж<bucket>)(uintptr)(bh.at<atomic.UnsafePointer>(i).Load()); bΔ1 != nil; bΔ1 = bΔ1.Value.next) {
        if ((~bΔ1).typ == typ && (~bΔ1).hash == h && (~bΔ1).size == size && eqslice(bΔ1.stk(), stk)) {
            return bΔ1;
        }
    }
    if (!alloc) {
        return default!;
    }
    @lock(ᏑprofInsertLock);
    // check again under the insertion lock
    for (var bΔ2 = (ж<bucket>)(uintptr)(bh.at<atomic.UnsafePointer>(i).Load()); bΔ2 != nil; bΔ2 = bΔ2.Value.next) {
        if ((~bΔ2).typ == typ && (~bΔ2).hash == h && (~bΔ2).size == size && eqslice(bΔ2.stk(), stk)) {
            unlock(ᏑprofInsertLock);
            return bΔ2;
        }
    }
    // Create new bucket.
    var b = newBucket(typ, len(stk));
    copy(b.stk(), stk);
    b.Value.hash = h;
    b.Value.size = size;
    ж<atomic.UnsafePointer> allnext = default!;
    if (typ == memProfile){
        allnext = Ꮡmbuckets;
    } else 
    if (typ == mutexProfile){
        allnext = Ꮡxbuckets;
    } else {
        allnext = Ꮡbbuckets;
    }
    b.Value.next = (ж<bucket>)(uintptr)(bh.at<atomic.UnsafePointer>(i).Load());
    b.Value.allnext = (ж<bucket>)(uintptr)(allnext.Load());
    bh.at<atomic.UnsafePointer>(i).StoreNoWB(new @unsafe.Pointer(b));
    allnext.StoreNoWB(new @unsafe.Pointer(b));
    unlock(ᏑprofInsertLock);
    return b;
}

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
    @lock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
    mProf_FlushLocked(index);
    unlock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
    unlock(ᏑprofMemActiveLock);
}

// mProf_FlushLocked flushes the events from the heap profiling cycle at index
// into the active profile. The caller must hold the lock for the active profile
// (profMemActiveLock) and for the profiling cycle at index
// (profMemFutureLock[index]).
internal static void mProf_FlushLocked(uint32 index) {
    assertLockHeld(ᏑprofMemActiveLock);
    assertLockHeld(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
    var head = (ж<bucket>)(uintptr)(Ꮡmbuckets.Load());
    for (var b = head; b != nil; b = b.Value.allnext) {
        var mp = b.mp();
        // Flush cycle C into the published profile and clear
        // it for reuse.
        var mpc = mp.at(memRecord.Ꮡfuture, (nint)(index));
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
    @lock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
    mProf_FlushLocked(index);
    unlock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
    unlock(ᏑprofMemActiveLock);
}

// Called by malloc to record a profiled block.
internal static void mProf_Malloc(ref m mp, @unsafe.Pointer Δp, uintptr size) {
    if (mp.profStack == default!) {
        // mp.profStack is nil if we happen to sample an allocation during the
        // initialization of mp. This case is rare, so we just ignore such
        // allocations. Change MemProfileRate to 1 if you need to reproduce such
        // cases for testing purposes.
        return;
    }
    // Only use the part of mp.profStack we need and ignore the extra space
    // reserved for delayed inline expansion with frame pointer unwinding.
    nint nstk = callers(4, mp.profStack[..(int)(debug.profstackdepth)]);
    var index = (ᏑmProfCycle.read() + 2) % (uint32)len(new memRecord(nil).future);
    var b = stkbucket(memProfile, size, mp.profStack[..(int)(nstk)], true);
    var mr = b.mp();
    var mpc = mr.at(memRecord.Ꮡfuture, (nint)(index));
    @lock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
    mpc.Value.allocs++;
    mpc.Value.alloc_bytes += size;
    unlock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
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
    var mpc = mp.at(memRecord.Ꮡfuture, (nint)(index));
    @lock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
    mpc.Value.frees++;
    mpc.Value.free_bytes += size;
    unlock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
}

internal static ж<uint64> Ꮡblockprofilerate = new StandardBox<uint64>(default(uint64));
internal static ref uint64 blockprofilerate => ref Ꮡblockprofilerate.Value; // in CPU ticks

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

internal static void blockevent(int64 cycles, nint skip) {
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
    if (rate <= 0 || (rate > cycles && cheaprand64() % rate > cycles)) {
        return false;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidSkipValueˢ = "invalid skip value"u8;

// saveblockevent records a profile event of the type specified by which.
// cycles is the quantity associated with this event and rate is the sampling rate,
// used to adjust the cycles value in the manner determined by the profile type.
// skip is the number of frames to omit from the traceback associated with the event.
// The traceback will be recorded from the stack of the goroutine associated with the current m.
// skip should be positive if this event is recorded from the current stack
// (e.g. when this is not called from a system stack)
internal static void saveblockevent(int64 cycles, int64 rate, nint skip, bucketType which) {
    if (debug.profstackdepth == 0) {
        // profstackdepth is set to 0 by the user, so mp.profStack is nil and we
        // can't record a stack trace.
        return;
    }
    if (skip > maxSkip) {
        print((@string)"requested skip="u8, skip);
        @throw(invalidSkipValueˢ);
    }
    var gp = getg();
    var mp = acquirem(); // we must not be preempted while accessing profstack
    nint nstk = default!;
    if (tracefpunwindoff() || (~gp).m.hasCgoOnStack()){
        if ((~(~gp).m).curg == nil || (~(~gp).m).curg == gp){
            nstk = callers(skip, (~mp).profStack);
        } else {
            nstk = gcallers((~(~gp).m).curg, skip, (~mp).profStack);
        }
    } else {
        if ((~(~gp).m).curg == nil || (~(~gp).m).curg == gp){
            if (skip > 0) {
                // We skip one fewer frame than the provided value for frame
                // pointer unwinding because the skip value includes the current
                // frame, whereas the saved frame pointer will give us the
                // caller's return address first (so, not including
                // saveblockevent)
                skip -= 1;
            }
            nstk = fpTracebackPartialExpand(skip, (@unsafe.Pointer)getfp(), (~mp).profStack);
        } else {
            mp.Value.profStack[0] = gp.Value.m.Value.curg.Value.sched.pc;
            nstk = 1 + fpTracebackPartialExpand(skip, (@unsafe.Pointer)(~(~(~gp).m).curg).sched.bp, (~mp).profStack[1..]);
        }
    }
    saveBlockEventStack(cycles, rate, (~mp).profStack[..(int)(nstk)], which);
    releasem(ref (mp).DerefOrNull());
}

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
                } else 
                {
                    var more = skipOrAdd(uf.pc + 1); if (!more) {
                        // ignore wrappers
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
        fp.Value = (@unsafe.Pointer)(~(ж<uintptr>)(uintptr)(fp));
    }
    return n;
}

// lockTimer assists with profiling contention on runtime-internal locks.
//
// There are several steps between the time that an M experiences contention and
// when that contention may be added to the profile. This comes from our
// constraints: We need to keep the critical section of each lock small,
// especially when those locks are contended. The reporting code cannot acquire
// new locks until the M has released all other locks, which means no memory
// allocations and encourages use of (temporary) M-local storage.
//
// The M will have space for storing one call stack that caused contention, and
// for the magnitude of that contention. It will also have space to store the
// magnitude of additional contention the M caused, since it only has space to
// remember one call stack and might encounter several contention events before
// it releases all of its locks and is thus able to transfer the local buffer
// into the profile.
//
// The M will collect the call stack when it unlocks the contended lock. That
// minimizes the impact on the critical section of the contended lock, and
// matches the mutex profile's behavior for contention in sync.Mutex: measured
// at the Unlock method.
//
// The profile for contention on sync.Mutex blames the caller of Unlock for the
// amount of contention experienced by the callers of Lock which had to wait.
// When there are several critical sections, this allows identifying which of
// them is responsible.
//
// Matching that behavior for runtime-internal locks will require identifying
// which Ms are blocked on the mutex. The semaphore-based implementation is
// ready to allow that, but the futex-based implementation will require a bit
// more work. Until then, we report contention on runtime-internal locks with a
// call stack taken from the unlock call (like the rest of the user-space
// "mutex" profile), but assign it a duration value based on how long the
// previous lock call took (like the user-space "block" profile).
//
// Thus, reporting the call stacks of runtime-internal lock contention is
// guarded by GODEBUG for now. Set GODEBUG=runtimecontentionstacks=1 to enable.
//
// TODO(rhysh): plumb through the delay duration, remove GODEBUG, update comment
//
// The M will track this by storing a pointer to the lock; lock/unlock pairs for
// runtime-internal locks are always on the same M.
//
// Together, that demands several steps for recording contention. First, when
// finally acquiring a contended lock, the M decides whether it should plan to
// profile that event by storing a pointer to the lock in its "to be profiled
// upon unlock" field. If that field is already set, it uses the relative
// magnitudes to weight a random choice between itself and the other lock, with
// the loser's time being added to the "additional contention" field. Otherwise
// if the M's call stack buffer is occupied, it does the comparison against that
// sample's magnitude.
//
// Second, having unlocked a mutex the M checks to see if it should capture the
// call stack into its local buffer. Finally, when the M unlocks its last mutex,
// it transfers the local buffer into the profile. As part of that step, it also
// transfers any "additional contention" time to the profile. Any lock
// contention that it experiences while adding samples to the profile will be
// recorded later as "additional contention" and not include a call stack, to
// avoid an echo.
[GoType] partial struct lockTimer {
    internal ж<mutex> @lock;
    internal int64 timeRate;
    internal int64 timeStart;
    internal int64 tickStart;
}

[GoRecv] internal static void begin(this ref lockTimer lt) {
    var rate = (int64)atomic.Load64(Ꮡmutexprofilerate);
    lt.timeRate = gTrackingPeriod;
    if (rate != 0 && rate < lt.timeRate) {
        lt.timeRate = rate;
    }
    if ((int64)cheaprand() % lt.timeRate == 0) {
        lt.timeStart = nanotime();
    }
    if (rate > 0 && (int64)cheaprand() % rate == 0) {
        lt.tickStart = cputicks();
    }
}

[GoRecv] internal static void end(this ref lockTimer lt) {
    var gp = getg();
    if (lt.timeStart != 0) {
        var nowTime = nanotime();
        (~gp).m.of(m.ᏑmLockProfile).of(mLockProfile.ᏑwaitTime).Add((nowTime - lt.timeStart) * lt.timeRate);
    }
    if (lt.tickStart != 0) {
        var nowTick = cputicks();
        (~gp).m.of(m.ᏑmLockProfile).recordLock(nowTick - lt.tickStart, lt.@lock);
    }
}

[GoType] partial struct mLockProfile {
    internal atomic.Int64 waitTime; // total nanoseconds spent waiting in runtime.lockWithRank
    internal slice<uintptr> stack; // stack that experienced contention in runtime.lockWithRank
    internal uintptr pending;      // *mutex that experienced contention (to be traceback-ed)
    internal int64 cycles;        // cycles attributable to "pending" (if set), otherwise to "stack"
    internal int64 cyclesLost;        // contention for which we weren't able to record a call stack
    internal bool disabled;         // attribute all time to "lost"
}

[GoRecv] internal static void recordLock(this ref mLockProfile prof, int64 cycles, ж<mutex> Ꮡl) {
    if (cycles <= 0) {
        return;
    }
    if (prof.disabled) {
        // We're experiencing contention while attempting to report contention.
        // Make a note of its magnitude, but don't allow it to be the sole cause
        // of another contention report.
        prof.cyclesLost += cycles;
        return;
    }
    if ((uintptr)Ꮡl == prof.pending) {
        // Optimization: we'd already planned to profile this same lock (though
        // possibly from a different unlock site).
        prof.cycles += cycles;
        return;
    }
    {
        var prev = prof.cycles; if (prev > 0) {
            // We can only store one call stack for runtime-internal lock contention
            // on this M, and we've already got one. Decide which should stay, and
            // add the other to the report for runtime._LostContendedRuntimeLock.
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
    // Saving the *mutex as a uintptr is safe because:
    //  - lockrank_on.go does this too, which gives it regular exercise
    //  - the lock would only move if it's stack allocated, which means it
    //      cannot experience multi-M contention
    prof.pending = (uintptr)Ꮡl;
    prof.cycles = cycles;
}

// From unlock2, we might not be holding a p in this code.
//
//go:nowritebarrierrec
internal static void recordUnlock(this ж<mLockProfile> Ꮡprof, ж<mutex> Ꮡl) {
    ref var prof = ref Ꮡprof.DerefOrNull();

    if ((uintptr)Ꮡl == prof.pending) {
        Ꮡprof.captureStack();
    }
    {
        var gp = getg(); if ((~(~gp).m).locks == 1 && (~(~gp).m).mLockProfile.cycles != 0) {
            prof.store();
        }
    }
}

internal static void captureStack(this ж<mLockProfile> Ꮡprof) {
    ref var prof = ref Ꮡprof.DerefOrNull();

    if (debug.profstackdepth == 0) {
        // profstackdepth is set to 0 by the user, so mp.profStack is nil and we
        // can't record a stack trace.
        return;
    }
    nint skip = 3; // runtime.(*mLockProfile).recordUnlock runtime.unlock2 runtime.unlockWithRank
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
    prof.pending = 0;
    prof.stack[0] = logicalStackSentinel;
    if (Ꮡdebug.of(debugᴛ1.ᏑruntimeContentionStacks).Load() == 0) {
        prof.stack[1] = abi.FuncPCABIInternal(_LostContendedRuntimeLock) + (uintptr)sys.PCQuantum;
        prof.stack[2] = 0;
        return;
    }
    nint nstk = default!;
    var gp = getg();
    var sp = getcallersp();
    var pc = getcallerpc();
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

[GoRecv] internal static void store(this ref mLockProfile prof) {
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
    var rate = (int64)atomic.Load64(Ꮡmutexprofilerate);
    saveBlockEventStack(cycles, rate, prof.stack[..(int)(nstk)], mutexProfile);
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

internal static ж<uint64> Ꮡmutexprofilerate = new StandardBox<uint64>(default(uint64));
internal static ref uint64 mutexprofilerate => ref Ꮡmutexprofilerate.Value; // fraction sampled

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
internal static void mutexevent(int64 cycles, nint skip) {
    if (cycles < 0) {
        cycles = 0;
    }
    var rate = (int64)atomic.Load64(Ꮡmutexprofilerate);
    if (rate > 0 && cheaprand64() % rate == 0) {
        saveblockevent(cycles, rate, skip + 1, mutexProfile);
    }
}

// Go interface to profile data.

// A StackRecord describes a single execution stack.
[GoType] partial struct StackRecord {
    public array<uintptr> Stack0 = new(32); // stack trace for this record; ends at first 0 entry
}

// Stack returns the stack trace associated with the record,
// a prefix of r.Stack0.
[GoRecv] public static slice<uintptr> Stack(this ref StackRecord r) {
    foreach (var (i, v) in r.Stack0.ΔRangeSnapshot()) {
        if (v == 0) {
            return r.Stack0[0..(int)(i)];
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
public static nint MemProfileRate = 512 * 1024;

// disableMemoryProfiling is set by the linker if memory profiling
// is not used and the link type guarantees nobody else could use it
// elsewhere.
// We check if the runtime.memProfileInternal symbol is present.
internal static bool disableMemoryProfiling;

// A MemProfileRecord describes the live objects allocated
// by a particular call sequence (stack trace).
[GoType] partial struct MemProfileRecord {
    public int64 AllocBytes, FreeBytes;       // number of bytes allocated, freed
    public int64 AllocObjects, FreeObjects;       // number of objects allocated, freed
    public array<uintptr> Stack0 = new(32); // stack trace for this record; ends at first 0 entry
}

// InUseBytes returns the number of bytes in use (AllocBytes - FreeBytes).
[GoRecv] public static int64 InUseBytes(this ref MemProfileRecord r) {
    return r.AllocBytes - r.FreeBytes;
}

// InUseObjects returns the number of objects in use (AllocObjects - FreeObjects).
[GoRecv] public static int64 InUseObjects(this ref MemProfileRecord r) {
    return r.AllocObjects - r.FreeObjects;
}

// Stack returns the stack trace associated with the record,
// a prefix of r.Stack0.
[GoRecv] public static slice<uintptr> Stack(this ref MemProfileRecord r) {
    foreach (var (i, v) in r.Stack0.ΔRangeSnapshot()) {
        if (v == 0) {
            return r.Stack0[0..(int)(i)];
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
internal static (nint n, bool ok) memProfileInternal(nint size, bool inuseZero, Action<profilerecord.MemProfileRecord> copyFn) {
    nint n = default!;
    bool ok = default!;

    var cycle = ᏑmProfCycle.read();
    // If we're between mProf_NextCycle and mProf_Flush, take care
    // of flushing to the active profile so we only have to look
    // at the active profile below.
    var index = cycle % (uint32)len(new memRecord(nil).future);
    @lock(ᏑprofMemActiveLock);
    @lock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
    mProf_FlushLocked(index);
    unlock(ᏑprofMemFutureLock.at<mutex>((nint)(index)));
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
            foreach (var (c, _) in (~mp).future) {
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
        racewriterangepc(@unsafe.Pointer.FromBox(Ꮡdst.at(MemProfileRecord.ᏑStack0, 0)), /* unsafe.Sizeof(dst.Stack0) */ (uintptr)256, getcallerpc(), abi.FuncPCABIInternal(MemProfile));
    }
    if (msanenabled) {
        msanwrite(@unsafe.Pointer.FromBox(Ꮡdst.at(MemProfileRecord.ᏑStack0, 0)), /* unsafe.Sizeof(dst.Stack0) */ (uintptr)256);
    }
    if (asanenabled) {
        asanwrite(@unsafe.Pointer.FromBox(Ꮡdst.at(MemProfileRecord.ᏑStack0, 0)), /* unsafe.Sizeof(dst.Stack0) */ (uintptr)256);
    }
    nint i = copy(dst.Stack0[..], src.Stack);
    builtin.clear(dst.Stack0[(int)(i)..]);
}

//go:linkname pprof_memProfileInternal
internal static (nint n, bool ok) pprof_memProfileInternal(slice<profilerecord.MemProfileRecord> Δp, bool inuseZero) {
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
[GoType] partial struct BlockProfileRecord {
    public int64 Count;
    public int64 Cycles;
    public partial ref StackRecord StackRecord { get; }
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
        expandFrames(Δp[..(int)(n)]);
    }
    return (n, ok);
}

internal static void expandFrames(slice<BlockProfileRecord> Δp) {
    var expandedStack = makeProfStack();
    foreach (var (i, _) in Δp) {
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
        nint k = copy(Δp[i].Stack0[..], expandedStack[..(int)(j)]);
        builtin.clear(Δp[i].Stack0[(int)(k)..]);
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
        racewriterangepc(@unsafe.Pointer.FromBox(Ꮡdst.at(BlockProfileRecord.ᏑStack0, 0)), /* unsafe.Sizeof(dst.Stack0) */ (uintptr)256, getcallerpc(), abi.FuncPCABIInternal(BlockProfile));
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
    builtin.clear(dst.Stack0[(int)(i)..]);
}

//go:linkname pprof_blockProfileInternal
internal static (nint n, bool ok) pprof_blockProfileInternal(slice<profilerecord.BlockProfileRecord> Δp) {
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
        expandFrames(Δp[..(int)(n)]);
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
internal static (nint n, bool ok) pprof_mutexProfileInternal(slice<profilerecord.BlockProfileRecord> Δp) {
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
        builtin.clear(Δp[0].Stack0[(int)(i)..]);
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
internal static (nint n, bool ok) pprof_threadCreateInternal(slice<profilerecord.StackRecord> Δp) {
    return threadCreateProfileInternal(len(Δp), (profilerecord.StackRecord r) => {
        Δp[0] = r;
        Δp = Δp[1..];
    });
}

//go:linkname pprof_goroutineProfileWithLabels
internal static (nint n, bool ok) pprof_goroutineProfileWithLabels(slice<profilerecord.StackRecord> Δp, slice<@unsafe.Pointer> labels) {
    return goroutineProfileWithLabels(Δp, labels);
}

// labels may be nil. If labels is non-nil, it must have the same length as p.
internal static (nint n, bool ok) goroutineProfileWithLabels(slice<profilerecord.StackRecord> Δp, slice<@unsafe.Pointer> labels) {
    if (labels != default! && len(labels) != len(Δp)) {
        labels = default!;
    }
    return goroutineProfileWithLabelsConcurrent(Δp, labels);
}


[GoType("dyn")] partial struct goroutineProfileᴛ1 {
    internal uint32 sema;
    internal bool active;
    internal atomic.Int64 offset;
    internal slice<profilerecord.StackRecord> records;
    internal slice<@unsafe.Pointer> labels;
}
internal static ж<goroutineProfileᴛ1> ᏑgoroutineProfile = new StandardBox<goroutineProfileᴛ1>(new goroutineProfileᴛ1(
    sema: 1
));
internal static ref goroutineProfileᴛ1 goroutineProfile => ref ᏑgoroutineProfile.Value;

[GoType("num:uint32")] partial struct goroutineProfileState;

internal static goroutineProfileState goroutineProfileAbsent => /* iota */ 0;
internal static goroutineProfileState goroutineProfileInProgress => 1;
internal static goroutineProfileState goroutineProfileSatisfied => 2;

[GoType("@internal.runtime.atomic_package.Uint32")] partial struct goroutineProfileStateHolder;

internal static goroutineProfileState Load(this ж<goroutineProfileStateHolder> Ꮡp) {
    return ((goroutineProfileState)(Ꮡp.Reinterpret<goroutineProfileStateHolder, atomic.Uint32>()).Load());
}

internal static void Store(this ж<goroutineProfileStateHolder> Ꮡp, goroutineProfileState value) {
    (Ꮡp.Reinterpret<goroutineProfileStateHolder, atomic.Uint32>()).Store((uint32)value);
}

internal static bool CompareAndSwap(this ж<goroutineProfileStateHolder> Ꮡp, goroutineProfileState old, goroutineProfileState @new) {
    return (Ꮡp.Reinterpret<goroutineProfileStateHolder, atomic.Uint32>()).CompareAndSwap((uint32)old, (uint32)@new);
}

internal static (nint n, bool ok) goroutineProfileWithLabelsConcurrent(slice<profilerecord.StackRecord> Δp, slice<@unsafe.Pointer> labels) {
    nint n = default!;

    if (len(Δp) == 0) {
        // An empty slice is obviously too small. Return a rough
        // allocation estimate without bothering to STW. As long as
        // this is close, then we'll only need to STW once (on the next
        // call).
        return ((nint)gcount(), false);
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
    // in particular.
    n = (nint)gcount();
    if ((uint32)(ᏑfingStatus.Load() & fingRunningFinalizer) != 0) {
        n++;
    }
    if (n > len(Δp)) {
        // There's not enough space in p to store the whole profile, so (per the
        // contract of runtime.GoroutineProfile) we're not allowed to write to p
        // at all and must return n, false.
        startTheWorld(stw);
        semrelease(ᏑgoroutineProfile.of(goroutineProfileᴛ1.Ꮡsema));
        return (n, false);
    }
    // Save current goroutine.
    var sp = getcallersp();
    var pc = getcallerpc();
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
    // The finalizer goroutine needs special handling because it can vary over
    // time between being a user goroutine (eligible for this profile) and a
    // system goroutine (to be excluded). Pick one before restarting the world.
    if (fing != nil) {
        fing.of(g.ᏑgoroutineProfiled).Store(goroutineProfileSatisfied);
        if (readgstatus(fing) != _Gdead && !isSystemGoroutine(ref (fing).DerefOrNull(), false)) {
            doRecordGoroutineProfile(fing, pcbuf);
        }
    }
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
    }
    // It's a big surprise that the number of goroutines changed while we
    // were collecting the profile. But probably better to return a
    // truncated profile than to crash the whole process.
    //
    // For instance, needm moves a goroutine out of the _Gdead state and so
    // might be able to change the goroutine count without interacting with
    // the scheduler. For code like that, the race windows are small and the
    // combination of features is uncommon, so it's hard to be (and remain)
    // sure we've caught them all.
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
    ref var gp1 = ref Ꮡgp1.DerefOrNull();

    if (readgstatus(Ꮡgp1) == _Gdead) {
        // Dead goroutines should not appear in the profile. Goroutines that
        // start while profile collection is active will get goroutineProfiled
        // set to goroutineProfileSatisfied before transitioning out of _Gdead,
        // so here we check _Gdead first.
        return;
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

    if (isSystemGoroutine(ref (Ꮡgp1).DerefOrNull(), false)) {
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
    if (readgstatus(Ꮡgp1) == _Grunning) {
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
        return gp1 != gpʗ1 && readgstatus(gp1) != _Gdead && !isSystemGoroutine(ref (gp1).DerefOrNull(), false);
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
        var sp = getcallersp();
        var pc = getcallerpc();
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
    foreach (var (i, mr) in records[0..(int)(n)]) {
        nint l = copy(Δp[i].Stack0[..], mr.Stack);
        builtin.clear(Δp[i].Stack0[(int)(l)..]);
    }
    return (n, ok);
}

internal static (nint n, bool ok) goroutineProfileInternal(slice<profilerecord.StackRecord> Δp) {
    return goroutineProfileWithLabels(Δp, default!);
}

internal static void saveg(uintptr pc, uintptr sp, ж<g> Ꮡgp, ref profilerecord.StackRecord r, slice<uintptr> pcbuf) {
    ref var gp = ref Ꮡgp.DerefOrNull();

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

// Force traceback=1 to override GOTRACEBACK setting,
// so that Stack's results are consistent.
// GOTRACEBACK is only about crash dumps.

} // end runtime_package
