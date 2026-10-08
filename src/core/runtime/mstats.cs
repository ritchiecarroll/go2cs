// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Memory statistics
namespace go;

using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

partial struct mstats {
    // Statistics about malloc heap.
    internal consistentHeapStats heapStats;
    // Statistics about stacks.
    internal sysMemStat stacks_sys; // only counts newosproc0 stack in mstats; differs from MemStats.StackSys
    // Statistics about allocation of low-level fixed-size structures.
    internal sysMemStat mspan_sys;
    internal sysMemStat mcache_sys;
    internal sysMemStat buckhash_sys; // profiling bucket hash table
    // Statistics about GC overhead.
    internal sysMemStat gcMiscSys; // updated atomically or during STW
    // Miscellaneous statistics.
    internal sysMemStat other_sys; // updated atomically or during STW
// Statistics about the garbage collector.

    // Protected by mheap or worldsema during GC.
    internal uint64 last_gc_unix; // last gc (in unix time)
    internal uint64 pause_total_ns;
    internal array<uint64> pause_ns = new(256); // circular buffer of recent gc pause lengths
    internal array<uint64> pause_end = new(256); // circular buffer of recent gc end times (nanoseconds since 1970)
    internal uint32 numgc;
    internal uint32 numforcedgc;  // number of user-forced GCs
    internal float64 gc_cpu_fraction; // fraction of CPU time used by GC
    internal uint64 last_gc_nanotime; // last gc (monotonic time)
    internal uint64 lastHeapInUse; // heapInUse at mark termination of the previous GC
    internal bool enablegc;
}

internal static ж<mstats> Ꮡmemstats = new StandardBox<mstats>(new mstats());
internal static ref mstats memstats => ref Ꮡmemstats.Value;

partial struct MemStats_BySize /*dyn*/ {
    // Size is the maximum byte size of an object in this
    // size class.
    public uint32 Size;
    // Mallocs is the cumulative count of heap objects
    // allocated in this size class. The cumulative bytes
    // of allocation is Size*Mallocs. The number of live
    // objects in this size class is Mallocs - Frees.
    public uint64 Mallocs;
    // Frees is the cumulative count of heap objects freed
    // in this size class.
    public uint64 Frees;
}

// A MemStats records statistics about the memory allocator.
partial struct MemStats {
// General statistics.

    // Alloc is bytes of allocated heap objects.
    //
    // This is the same as HeapAlloc (see below).
    public uint64 Alloc;
    // TotalAlloc is cumulative bytes allocated for heap objects.
    //
    // TotalAlloc increases as heap objects are allocated, but
    // unlike Alloc and HeapAlloc, it does not decrease when
    // objects are freed.
    public uint64 TotalAlloc;
    // Sys is the total bytes of memory obtained from the OS.
    //
    // Sys is the sum of the XSys fields below. Sys measures the
    // virtual address space reserved by the Go runtime for the
    // heap, stacks, and other internal data structures. It's
    // likely that not all of the virtual address space is backed
    // by physical memory at any given moment, though in general
    // it all was at some point.
    public uint64 Sys;
    // Lookups is the number of pointer lookups performed by the
    // runtime.
    //
    // This is primarily useful for debugging runtime internals.
    public uint64 Lookups;
    // Mallocs is the cumulative count of heap objects allocated.
    // The number of live objects is Mallocs - Frees.
    public uint64 Mallocs;
    // Frees is the cumulative count of heap objects freed.
    public uint64 Frees;
// Heap memory statistics.
//
// Interpreting the heap statistics requires some knowledge of
// how Go organizes memory. Go divides the virtual address
// space of the heap into "spans", which are contiguous
// regions of memory 8K or larger. A span may be in one of
// three states:
//
// An "idle" span contains no objects or other data. The
// physical memory backing an idle span can be released back
// to the OS (but the virtual address space never is), or it
// can be converted into an "in use" or "stack" span.
//
// An "in use" span contains at least one heap object and may
// have free space available to allocate more heap objects.
//
// A "stack" span is used for goroutine stacks. Stack spans
// are not considered part of the heap. A span can change
// between heap and stack memory; it is never used for both
// simultaneously.

    // HeapAlloc is bytes of allocated heap objects.
    //
    // "Allocated" heap objects include all reachable objects, as
    // well as unreachable objects that the garbage collector has
    // not yet freed. Specifically, HeapAlloc increases as heap
    // objects are allocated and decreases as the heap is swept
    // and unreachable objects are freed. Sweeping occurs
    // incrementally between GC cycles, so these two processes
    // occur simultaneously, and as a result HeapAlloc tends to
    // change smoothly (in contrast with the sawtooth that is
    // typical of stop-the-world garbage collectors).
    public uint64 HeapAlloc;
    // HeapSys is bytes of heap memory obtained from the OS.
    //
    // HeapSys measures the amount of virtual address space
    // reserved for the heap. This includes virtual address space
    // that has been reserved but not yet used, which consumes no
    // physical memory, but tends to be small, as well as virtual
    // address space for which the physical memory has been
    // returned to the OS after it became unused (see HeapReleased
    // for a measure of the latter).
    //
    // HeapSys estimates the largest size the heap has had.
    public uint64 HeapSys;
    // HeapIdle is bytes in idle (unused) spans.
    //
    // Idle spans have no objects in them. These spans could be
    // (and may already have been) returned to the OS, or they can
    // be reused for heap allocations, or they can be reused as
    // stack memory.
    //
    // HeapIdle minus HeapReleased estimates the amount of memory
    // that could be returned to the OS, but is being retained by
    // the runtime so it can grow the heap without requesting more
    // memory from the OS. If this difference is significantly
    // larger than the heap size, it indicates there was a recent
    // transient spike in live heap size.
    public uint64 HeapIdle;
    // HeapInuse is bytes in in-use spans.
    //
    // In-use spans have at least one object in them. These spans
    // can only be used for other objects of roughly the same
    // size.
    //
    // HeapInuse minus HeapAlloc estimates the amount of memory
    // that has been dedicated to particular size classes, but is
    // not currently being used. This is an upper bound on
    // fragmentation, but in general this memory can be reused
    // efficiently.
    public uint64 HeapInuse;
    // HeapReleased is bytes of physical memory returned to the OS.
    //
    // This counts heap memory from idle spans that was returned
    // to the OS and has not yet been reacquired for the heap.
    public uint64 HeapReleased;
    // HeapObjects is the number of allocated heap objects.
    //
    // Like HeapAlloc, this increases as objects are allocated and
    // decreases as the heap is swept and unreachable objects are
    // freed.
    public uint64 HeapObjects;
// Stack memory statistics.
//
// Stacks are not considered part of the heap, but the runtime
// can reuse a span of heap memory for stack memory, and
// vice-versa.

    // StackInuse is bytes in stack spans.
    //
    // In-use stack spans have at least one stack in them. These
    // spans can only be used for other stacks of the same size.
    //
    // There is no StackIdle because unused stack spans are
    // returned to the heap (and hence counted toward HeapIdle).
    public uint64 StackInuse;
    // StackSys is bytes of stack memory obtained from the OS.
    //
    // StackSys is StackInuse, plus any memory obtained directly
    // from the OS for OS thread stacks.
    //
    // In non-cgo programs this metric is currently equal to StackInuse
    // (but this should not be relied upon, and the value may change in
    // the future).
    //
    // In cgo programs this metric includes OS thread stacks allocated
    // directly from the OS. Currently, this only accounts for one stack in
    // c-shared and c-archive build modes and other sources of stacks from
    // the OS (notably, any allocated by C code) are not currently measured.
    // Note this too may change in the future.
    public uint64 StackSys;
// Off-heap memory statistics.
//
// The following statistics measure runtime-internal
// structures that are not allocated from heap memory (usually
// because they are part of implementing the heap). Unlike
// heap or stack memory, any memory allocated to these
// structures is dedicated to these structures.
//
// These are primarily useful for debugging runtime memory
// overheads.

    // MSpanInuse is bytes of allocated mspan structures.
    public uint64 MSpanInuse;
    // MSpanSys is bytes of memory obtained from the OS for mspan
    // structures.
    public uint64 MSpanSys;
    // MCacheInuse is bytes of allocated mcache structures.
    public uint64 MCacheInuse;
    // MCacheSys is bytes of memory obtained from the OS for
    // mcache structures.
    public uint64 MCacheSys;
    // BuckHashSys is bytes of memory in profiling bucket hash tables.
    public uint64 BuckHashSys;
    // GCSys is bytes of memory in garbage collection metadata.
    public uint64 GCSys;
    // OtherSys is bytes of memory in miscellaneous off-heap
    // runtime allocations.
    public uint64 OtherSys;
// Garbage collector statistics.

    // NextGC is the target heap size of the next GC cycle.
    //
    // The garbage collector's goal is to keep HeapAlloc ≤ NextGC.
    // At the end of each GC cycle, the target for the next cycle
    // is computed based on the amount of reachable data and the
    // value of GOGC.
    public uint64 NextGC;
    // LastGC is the time the last garbage collection finished, as
    // nanoseconds since 1970 (the UNIX epoch).
    public uint64 LastGC;
    // PauseTotalNs is the cumulative nanoseconds in GC
    // stop-the-world pauses since the program started.
    //
    // During a stop-the-world pause, all goroutines are paused
    // and only the garbage collector can run.
    public uint64 PauseTotalNs;
    // PauseNs is a circular buffer of recent GC stop-the-world
    // pause times in nanoseconds.
    //
    // The most recent pause is at PauseNs[(NumGC+255)%256]. In
    // general, PauseNs[N%256] records the time paused in the most
    // recent N%256th GC cycle. There may be multiple pauses per
    // GC cycle; this is the sum of all pauses during a cycle.
    public array<uint64> PauseNs = new(256);
    // PauseEnd is a circular buffer of recent GC pause end times,
    // as nanoseconds since 1970 (the UNIX epoch).
    //
    // This buffer is filled the same way as PauseNs. There may be
    // multiple pauses per GC cycle; this records the end of the
    // last pause in a cycle.
    public array<uint64> PauseEnd = new(256);
    // NumGC is the number of completed GC cycles.
    public uint32 NumGC;
    // NumForcedGC is the number of GC cycles that were forced by
    // the application calling the GC function.
    public uint32 NumForcedGC;
    // GCCPUFraction is the fraction of this program's available
    // CPU time used by the GC since the program started.
    //
    // GCCPUFraction is expressed as a number between 0 and 1,
    // where 0 means GC has consumed none of this program's CPU. A
    // program's available CPU time is defined as the integral of
    // GOMAXPROCS since the program started. That is, if
    // GOMAXPROCS is 2 and a program has been running for 10
    // seconds, its "available CPU" is 20 seconds. GCCPUFraction
    // does not include CPU time used for write barrier activity.
    //
    // This is the same as the fraction of CPU reported by
    // GODEBUG=gctrace=1.
    public float64 GCCPUFraction;
    // EnableGC indicates that GC is enabled. It is always true,
    // even if GOGC=off.
    public bool EnableGC;
    // DebugGC is currently unused.
    public bool DebugGC;
    // BySize reports per-size class allocation statistics.
    //
    // BySize[N] gives statistics for allocations of size S where
    // BySize[N-1].Size < S ≤ BySize[N].Size.
    //
    // This does not report allocations larger than BySize[60].Size.
    public array<MemStats_BySize> BySize = new(61);
}

/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */ internal static void initΔ4() {
    {
        var offset = /* unsafe.Offsetof(memstats.heapStats) */ (uintptr)0; if (offset % 8 != 0) {
            println(offset);
            @throw("memstats.heapStats not aligned to 8 bytes"u8);
        }
    }
    // Ensure the size of heapStatsDelta causes adjacent fields/slots (e.g.
    // [3]heapStatsDelta) to be 8-byte aligned.
    {
        var size = /* unsafe.Sizeof(heapStatsDelta{}) */ (uintptr)1176; if (size % 8 != 0) {
            println(size);
            @throw("heapStatsDelta not a multiple of 8 bytes in size"u8);
        }
    }
}

// go2cs generated this placeholder — func ReadMemStats is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// doubleCheckReadMemStats controls a double-check mode for ReadMemStats that
// ensures consistency between the values that ReadMemStats is using and the
// runtime-internal stats.
internal static ж<bool> ᏑdoubleCheckReadMemStats = new StandardBox<bool>(false);
internal static ref bool doubleCheckReadMemStats => ref ᏑdoubleCheckReadMemStats.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string heapInUseAndConsistentˢ = "heapInUse and consistent stats are not equal"u8;
internal static readonly @string heapReleasedAndˢ = "heapReleased and consistent stats are not equal"u8;
internal static readonly @string measuresOfTheRetainedˢ = "measures of the retained heap are not equal"u8;
internal static readonly @string totalAllocAndConsistentˢ = "totalAlloc and consistent stats are not equal"u8;
internal static readonly @string totalFreeAndConsistentˢ = "totalFree and consistent stats are not equal"u8;
internal static readonly @string mappedReadyAndOtherˢ = "mappedReady and other memstats are not equal"u8;

// readmemstats_m populates stats for internal runtime values.
//
// The world must be stopped.
internal static void readmemstats_m(ref MemStats stats) {
    assertWorldStopped();
    // Flush mcaches to mcentral before doing anything else.
    //
    // Flushing to the mcentral may in general cause stats to
    // change as mcentral data structures are manipulated.
    systemstack(flushallmcaches);
    // Calculate memory allocator stats.
    // During program execution we only count number of frees and amount of freed memory.
    // Current number of alive objects in the heap and amount of alive heap memory
    // are calculated by scanning all spans.
    // Total number of mallocs is calculated as number of frees plus number of alive objects.
    // Similarly, total amount of allocated memory is calculated as amount of freed memory
    // plus amount of alive heap memory.
    // Collect consistent stats, which are the source-of-truth in some cases.
    ref var consStats = ref heap(new heapStatsDelta(), out var ᏑconsStats);
    Ꮡmemstats.of(mstats.ᏑheapStats).unsafeRead(ᏑconsStats);
    // Collect large allocation stats.
    var totalAlloc = consStats.largeAlloc;
    var nMalloc = consStats.largeAllocCount;
    var totalFree = consStats.largeFree;
    var nFree = consStats.largeFreeCount;
    // Collect per-sizeclass stats.
    array<MemStats_BySize> bySize = new(68); /* _NumSizeClasses */
    foreach (var (i, _) in bySize) {
        bySize[i].Size = (uint32)class_to_size[i];
        // Malloc stats.
        var a = consStats.smallAllocCount[i];
        totalAlloc += a * (uint64)class_to_size[i];
        nMalloc += a;
        bySize[i].Mallocs = a;
        // Free stats.
        var f = consStats.smallFreeCount[i];
        totalFree += f * (uint64)class_to_size[i];
        nFree += f;
        bySize[i].Frees = f;
    }
    // Account for tiny allocations.
    // For historical reasons, MemStats includes tiny allocations
    // in both the total free and total alloc count. This double-counts
    // memory in some sense because their tiny allocation block is also
    // counted. Tracking the lifetime of individual tiny allocations is
    // currently not done because it would be too expensive.
    nFree += consStats.tinyAllocCount;
    nMalloc += consStats.tinyAllocCount;
    // Calculate derived stats.
    var stackInUse = (uint64)consStats.inStacks;
    var gcWorkBufInUse = (uint64)consStats.inWorkBufs;
    var gcProgPtrScalarBitsInUse = (uint64)consStats.inPtrScalarBits;
    var totalMapped = ᏑgcController.of(gcControllerState.ᏑheapInUse).load() + ᏑgcController.of(gcControllerState.ᏑheapFree).load() + ᏑgcController.of(gcControllerState.ᏑheapReleased).load() + Ꮡmemstats.of(mstats.Ꮡstacks_sys).load() + Ꮡmemstats.of(mstats.Ꮡmspan_sys).load() + Ꮡmemstats.of(mstats.Ꮡmcache_sys).load() + Ꮡmemstats.of(mstats.Ꮡbuckhash_sys).load() + Ꮡmemstats.of(mstats.ᏑgcMiscSys).load() + Ꮡmemstats.of(mstats.Ꮡother_sys).load() + stackInUse + gcWorkBufInUse + gcProgPtrScalarBitsInUse;
    var heapGoal = ᏑgcController.heapGoal();
    if (doubleCheckReadMemStats) {
        // Only check this if we're debugging. It would be bad to crash an application
        // just because the debugging stats are wrong. We mostly rely on tests to catch
        // these issues, and we enable the double check mode for tests.
        //
        // The world is stopped, so the consistent stats (after aggregation)
        // should be identical to some combination of memstats. In particular:
        //
        // * memstats.heapInUse == inHeap
        // * memstats.heapReleased == released
        // * memstats.heapInUse + memstats.heapFree == committed - inStacks - inWorkBufs - inPtrScalarBits
        // * memstats.totalAlloc == totalAlloc
        // * memstats.totalFree == totalFree
        //
        // Check if that's actually true.
        //
        // Prevent sysmon and the tracer from skewing the stats since they can
        // act without synchronizing with a STW. See #64401.
        @lock(Ꮡsched.of(schedt.Ꮡsysmonlock));
        @lock(ᏑΔtrace.of(Δtraceᴛ1.Ꮡlock));
        if (ᏑgcController.of(gcControllerState.ᏑheapInUse).load() != (uint64)consStats.inHeap) {
            print((@string)"runtime: heapInUse="u8, ᏑgcController.of(gcControllerState.ᏑheapInUse).load(), (@string)"\n"u8);
            print((@string)"runtime: consistent value="u8, consStats.inHeap, (@string)"\n"u8);
            @throw(heapInUseAndConsistentˢ);
        }
        if (ᏑgcController.of(gcControllerState.ᏑheapReleased).load() != (uint64)consStats.released) {
            print((@string)"runtime: heapReleased="u8, ᏑgcController.of(gcControllerState.ᏑheapReleased).load(), (@string)"\n"u8);
            print((@string)"runtime: consistent value="u8, consStats.released, (@string)"\n"u8);
            @throw(heapReleasedAndˢ);
        }
        var heapRetained = ᏑgcController.of(gcControllerState.ᏑheapInUse).load() + ᏑgcController.of(gcControllerState.ᏑheapFree).load();
        var consRetained = (uint64)(consStats.committed - consStats.inStacks - consStats.inWorkBufs - consStats.inPtrScalarBits);
        if (heapRetained != consRetained) {
            print((@string)"runtime: global value="u8, heapRetained, (@string)"\n"u8);
            print((@string)"runtime: consistent value="u8, consRetained, (@string)"\n"u8);
            @throw(measuresOfTheRetainedˢ);
        }
        if (ᏑgcController.of(gcControllerState.ᏑtotalAlloc).Load() != totalAlloc) {
            print((@string)"runtime: totalAlloc="u8, ᏑgcController.of(gcControllerState.ᏑtotalAlloc).Load(), (@string)"\n"u8);
            print((@string)"runtime: consistent value="u8, totalAlloc, (@string)"\n"u8);
            @throw(totalAllocAndConsistentˢ);
        }
        if (ᏑgcController.of(gcControllerState.ᏑtotalFree).Load() != totalFree) {
            print((@string)"runtime: totalFree="u8, ᏑgcController.of(gcControllerState.ᏑtotalFree).Load(), (@string)"\n"u8);
            print((@string)"runtime: consistent value="u8, totalFree, (@string)"\n"u8);
            @throw(totalFreeAndConsistentˢ);
        }
        // Also check that mappedReady lines up with totalMapped - released.
        // This isn't really the same type of "make sure consistent stats line up" situation,
        // but this is an opportune time to check.
        if (ᏑgcController.of(gcControllerState.ᏑmappedReady).Load() != totalMapped - (uint64)consStats.released) {
            print((@string)"runtime: mappedReady="u8, ᏑgcController.of(gcControllerState.ᏑmappedReady).Load(), (@string)"\n"u8);
            print((@string)"runtime: totalMapped="u8, totalMapped, (@string)"\n"u8);
            print((@string)"runtime: released="u8, (uint64)consStats.released, (@string)"\n"u8);
            print((@string)"runtime: totalMapped-released="u8, totalMapped - (uint64)consStats.released, (@string)"\n"u8);
            @throw(mappedReadyAndOtherˢ);
        }
        unlock(ᏑΔtrace.of(Δtraceᴛ1.Ꮡlock));
        unlock(Ꮡsched.of(schedt.Ꮡsysmonlock));
    }
    // We've calculated all the values we need. Now, populate stats.
    stats.Alloc = totalAlloc - totalFree;
    stats.TotalAlloc = totalAlloc;
    stats.Sys = totalMapped;
    stats.Mallocs = nMalloc;
    stats.Frees = nFree;
    stats.HeapAlloc = totalAlloc - totalFree;
    stats.HeapSys = ᏑgcController.of(gcControllerState.ᏑheapInUse).load() + ᏑgcController.of(gcControllerState.ᏑheapFree).load() + ᏑgcController.of(gcControllerState.ᏑheapReleased).load();
    // By definition, HeapIdle is memory that was mapped
    // for the heap but is not currently used to hold heap
    // objects. It also specifically is memory that can be
    // used for other purposes, like stacks, but this memory
    // is subtracted out of HeapSys before it makes that
    // transition. Put another way:
    //
    // HeapSys = bytes allocated from the OS for the heap - bytes ultimately used for non-heap purposes
    // HeapIdle = bytes allocated from the OS for the heap - bytes ultimately used for any purpose
    //
    // or
    //
    // HeapSys = sys - stacks_inuse - gcWorkBufInUse - gcProgPtrScalarBitsInUse
    // HeapIdle = sys - stacks_inuse - gcWorkBufInUse - gcProgPtrScalarBitsInUse - heapInUse
    //
    // => HeapIdle = HeapSys - heapInUse = heapFree + heapReleased
    stats.HeapIdle = ᏑgcController.of(gcControllerState.ᏑheapFree).load() + ᏑgcController.of(gcControllerState.ᏑheapReleased).load();
    stats.HeapInuse = ᏑgcController.of(gcControllerState.ᏑheapInUse).load();
    stats.HeapReleased = ᏑgcController.of(gcControllerState.ᏑheapReleased).load();
    stats.HeapObjects = nMalloc - nFree;
    stats.StackInuse = stackInUse;
    // memstats.stacks_sys is only memory mapped directly for OS stacks.
    // Add in heap-allocated stack memory for user consumption.
    stats.StackSys = stackInUse + Ꮡmemstats.of(mstats.Ꮡstacks_sys).load();
    stats.MSpanInuse = (uint64)mheap_.spanalloc.inuse;
    stats.MSpanSys = Ꮡmemstats.of(mstats.Ꮡmspan_sys).load();
    stats.MCacheInuse = (uint64)mheap_.cachealloc.inuse;
    stats.MCacheSys = Ꮡmemstats.of(mstats.Ꮡmcache_sys).load();
    stats.BuckHashSys = Ꮡmemstats.of(mstats.Ꮡbuckhash_sys).load();
    // MemStats defines GCSys as an aggregate of all memory related
    // to the memory management system, but we track this memory
    // at a more granular level in the runtime.
    stats.GCSys = Ꮡmemstats.of(mstats.ᏑgcMiscSys).load() + gcWorkBufInUse + gcProgPtrScalarBitsInUse;
    stats.OtherSys = Ꮡmemstats.of(mstats.Ꮡother_sys).load();
    stats.NextGC = heapGoal;
    stats.LastGC = memstats.last_gc_unix;
    stats.PauseTotalNs = memstats.pause_total_ns;
    stats.PauseNs = memstats.pause_ns.Clone();
    stats.PauseEnd = memstats.pause_end.Clone();
    stats.NumGC = memstats.numgc;
    stats.NumForcedGC = memstats.numforcedgc;
    stats.GCCPUFraction = memstats.gc_cpu_fraction;
    stats.EnableGC = true;
    // stats.BySize and bySize might not match in length.
    // That's OK, stats.BySize cannot change due to backwards
    // compatibility issues. copy will copy the minimum amount
    // of values between the two of them.
    copy(stats.BySize[..], bySize[..]);
}

//go:linkname readGCStats runtime/debug.readGCStats
internal static void readGCStats(ж<slice<uint64>> Ꮡpauses) {
    systemstack(() => {
        readGCStats_m(ref (Ꮡpauses).DerefOrNull());
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string shortSlicePassedToˢ = "short slice passed to readGCStats"u8;

// readGCStats_m must be called on the system stack because it acquires the heap
// lock. See mheap for details.
//
//go:systemstack
internal static void readGCStats_m(ref slice<uint64> pauses) {
    var Δp = pauses;
    // Calling code in runtime/debug should make the slice large enough.
    if (cap(Δp) < len(memstats.pause_ns) + 3) {
        @throw(shortSlicePassedToˢ);
    }
    // Pass back: pauses, pause ends, last gc (absolute time), number of gc, total pause ns.
    @lock(Ꮡmheap_.of(mheap.Ꮡlock));
    var n = memstats.numgc;
    if (n > (uint32)len(memstats.pause_ns)) {
        n = (uint32)len(memstats.pause_ns);
    }
    // The pause buffer is circular. The most recent pause is at
    // pause_ns[(numgc-1)%len(pause_ns)], and then backward
    // from there to go back farther in time. We deliver the times
    // most recent first (in p[0]).
    Δp = Δp.slice(0, cap(Δp));
    for (var i = (uint32)0; i < n; i++) {
        var j = (memstats.numgc - 1 - i) % (uint32)len(memstats.pause_ns);
        Δp[i] = memstats.pause_ns[j];
        Δp[n + i] = memstats.pause_end[j];
    }
    Δp[n + n] = memstats.last_gc_unix;
    Δp[n + n + 1] = (uint64)memstats.numgc;
    Δp[n + n + 2] = memstats.pause_total_ns;
    unlock(Ꮡmheap_.of(mheap.Ꮡlock));
    pauses = Δp.slice(0, (nint)(n + n + 3));
}

// flushmcache flushes the mcache of allp[i].
//
// The world must be stopped.
//
//go:nowritebarrier
internal static void flushmcache(nint i) {
    assertWorldStopped();
    var Δp = allp[i];
    var c = Δp.Value.mcache;
    if (c == nil) {
        return;
    }
    c.releaseAll();
    stackcache_clear(ref (c).DerefOrNull());
}

// go2cs generated this placeholder — func flushallmcaches is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

partial struct sysMemStat /*num:uint64*/;

// load atomically reads the value of the stat.
//
// Must be nosplit as it is called in runtime initialization, e.g. newosproc0.
//
//go:nosplit
internal static uint64 load(this ж<sysMemStat> Ꮡs) {
    return atomic.Load64(Ꮡs.Reinterpret<sysMemStat, uint64>());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sysMemStatOverflowˢ = "sysMemStat overflow"u8;

// add atomically adds the sysMemStat by n.
//
// Must be nosplit as it is called in runtime initialization, e.g. newosproc0.
//
//go:nosplit
internal static void add(this ж<sysMemStat> Ꮡs, int64 n) {
    var val = atomic.Xadd64(Ꮡs.Reinterpret<sysMemStat, uint64>(), n);
    if ((n > 0 && (int64)val < n) || (n < 0 && (int64)val + n < n)) {
        print((@string)"runtime: val="u8, val, (@string)" n="u8, n, (@string)"\n"u8);
        @throw(sysMemStatOverflowˢ);
    }
}

// heapStatsDelta contains deltas of various runtime memory statistics
// that need to be updated together in order for them to be kept
// consistent with one another.
partial struct heapStatsDelta {
    // Memory stats.
    internal int64 committed; // byte delta of memory committed
    internal int64 released; // byte delta of released memory generated
    internal int64 inHeap; // byte delta of memory placed in the heap
    internal int64 inStacks; // byte delta of memory reserved for stacks
    internal int64 inWorkBufs; // byte delta of memory reserved for work bufs
    internal int64 inPtrScalarBits; // byte delta of memory reserved for unrolled GC prog bits
    // Allocator stats.
    //
    // These are all uint64 because they're cumulative, and could quickly wrap
    // around otherwise.
    internal uint64 tinyAllocCount;                  // number of tiny allocations
    internal uint64 largeAlloc;                  // bytes allocated for large objects
    internal uint64 largeAllocCount;                  // number of large object allocations
    internal array<uint64> smallAllocCount = new(_NumSizeClasses); // number of allocs for small objects
    internal uint64 largeFree;                  // bytes freed for large objects (>maxSmallSize)
    internal uint64 largeFreeCount;                  // number of frees for large objects (>maxSmallSize)
    internal array<uint64> smallFreeCount = new(_NumSizeClasses); // number of frees for small objects (<=maxSmallSize)
}

// NOTE: This struct must be a multiple of 8 bytes in size because it
// is stored in an array. If it's not, atomic accesses to the above
// fields may be unaligned and fail on 32-bit platforms.

// merge adds in the deltas from b into a.
internal static void merge(this ref heapStatsDelta a, ж<heapStatsDelta> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    a.committed += b.committed;
    a.released += b.released;
    a.inHeap += b.inHeap;
    a.inStacks += b.inStacks;
    a.inWorkBufs += b.inWorkBufs;
    a.inPtrScalarBits += b.inPtrScalarBits;
    a.tinyAllocCount += b.tinyAllocCount;
    a.largeAlloc += b.largeAlloc;
    a.largeAllocCount += b.largeAllocCount;
    foreach (var (i, _) in b.smallAllocCount) {
        a.smallAllocCount[i] += b.smallAllocCount[i];
    }
    a.largeFree += b.largeFree;
    a.largeFreeCount += b.largeFreeCount;
    foreach (var (i, _) in b.smallFreeCount) {
        a.smallFreeCount[i] += b.smallFreeCount[i];
    }
}

// consistentHeapStats represents a set of various memory statistics
// whose updates must be viewed completely to get a consistent
// state of the world.
//
// To write updates to memory stats use the acquire and release
// methods. To obtain a consistent global snapshot of these statistics,
// use read.
partial struct consistentHeapStats {
    // stats is a ring buffer of heapStatsDelta values.
    // Writers always atomically update the delta at index gen.
    //
    // Readers operate by rotating gen (0 -> 1 -> 2 -> 0 -> ...)
    // and synchronizing with writers by observing each P's
    // statsSeq field. If the reader observes a P not writing,
    // it can be sure that it will pick up the new gen value the
    // next time it writes.
    //
    // The reader then takes responsibility by clearing space
    // in the ring buffer for the next reader to rotate gen to
    // that space (i.e. it merges in values from index (gen-2) mod 3
    // to index (gen-1) mod 3, then clears the former).
    //
    // Note that this means only one reader can be reading at a time.
    // There is no way for readers to synchronize.
    //
    // This process is why we need a ring buffer of size 3 instead
    // of 2: one is for the writers, one contains the most recent
    // data, and the last one is clear so writers can begin writing
    // to it the moment gen is updated.
    internal array<heapStatsDelta> stats = new(3, () => new());
    // gen represents the current index into which writers
    // are writing, and can take on the value of 0, 1, or 2.
    internal atomic.Uint32 gen;
    // noPLock is intended to provide mutual exclusion for updating
    // stats when no P is available. It does not block other writers
    // with a P, only other writers without a P and the reader. Because
    // stats are usually updated when a P is available, contention on
    // this lock should be minimal.
    internal mutex noPLock;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badSequenceNumberˢ = "bad sequence number"u8;

// acquire returns a heapStatsDelta to be updated. In effect,
// it acquires the shard for writing. release must be called
// as soon as the relevant deltas are updated.
//
// The returned heapStatsDelta must be updated atomically.
//
// The caller's P must not change between acquire and
// release. This also means that the caller should not
// acquire a P or release its P in between. A P also must
// not acquire a given consistentHeapStats if it hasn't
// yet released it.
//
// nosplit because a stack growth in this function could
// lead to a stack allocation that could reenter the
// function.
//
//go:nosplit
internal static ж<heapStatsDelta> acquire(this ж<consistentHeapStats> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    {
        var pp = (~(~getg()).m).p.ptr(); if (pp != nil){
            var seq = pp.of(runtime_package.Δp.ᏑstatsSeq).Add(1);
            if (seq % 2 == 0) {
                // Should have been incremented to odd.
                print((@string)"runtime: seq="u8, seq, (@string)"\n"u8);
                @throw(badSequenceNumberˢ);
            }
        } else {
            @lock(Ꮡm.of(consistentHeapStats.ᏑnoPLock));
        }
    }
    var gen = Ꮡm.of(consistentHeapStats.Ꮡgen).Load() % 3;
    return Ꮡ(m.stats, gen);
}

// release indicates that the writer is done modifying
// the delta. The value returned by the corresponding
// acquire must no longer be accessed or modified after
// release is called.
//
// The caller's P must not change between acquire and
// release. This also means that the caller should not
// acquire a P or release its P in between.
//
// nosplit because a stack growth in this function could
// lead to a stack allocation that causes another acquire
// before this operation has completed.
//
//go:nosplit
internal static void release(this ж<consistentHeapStats> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    {
        var pp = (~(~getg()).m).p.ptr(); if (pp != nil){
            var seq = pp.of(runtime_package.Δp.ᏑstatsSeq).Add(1);
            if (seq % 2 != 0) {
                // Should have been incremented to even.
                print((@string)"runtime: seq="u8, seq, (@string)"\n"u8);
                @throw(badSequenceNumberˢ);
            }
        } else {
            unlock(Ꮡm.of(consistentHeapStats.ᏑnoPLock));
        }
    }
}

// unsafeRead aggregates the delta for this shard into out.
//
// Unsafe because it does so without any synchronization. The
// world must be stopped.
internal static void unsafeRead(this ref consistentHeapStats m, ж<heapStatsDelta> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    assertWorldStopped();
    foreach (var (i, _) in m.stats) {
        @out.merge(Ꮡ(m.stats, i));
    }
}

// unsafeClear clears the shard.
//
// Unsafe because the world must be stopped and values should
// be donated elsewhere before clearing.
internal static void unsafeClear(this ref consistentHeapStats m) {
    assertWorldStopped();
    foreach (var (i, _) in m.stats) {
        m.stats[i] = new heapStatsDelta(nil);
    }
}

// go2cs generated this placeholder — func read is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

partial struct cpuStats {
// All fields are CPU time in nanoseconds computed by comparing
// calls of nanotime. This means they're all overestimates, because
// they don't accurately compute on-CPU time (so some of the time
// could be spent scheduled away by the OS).
    public int64 GCAssistTime; // GC assists
    public int64 GCDedicatedTime; // GC dedicated mark workers + pauses
    public int64 GCIdleTime; // GC idle mark workers
    public int64 GCPauseTime; // GC pauses (all GOMAXPROCS, even if just 1 is running)
    public int64 GCTotalTime;
    public int64 ScavengeAssistTime; // background scavenger
    public int64 ScavengeBgTime; // scavenge assists
    public int64 ScavengeTotalTime;
    public int64 IdleTime; // Time Ps spent in _Pidle.
    public int64 UserTime; // Time Ps spent in _Prunning or _Psyscall that's not any of the above.
    public int64 TotalTime; // GOMAXPROCS * (monotonic wall clock time elapsed)
}

// accumulateGCPauseTime add dt*stwProcs to the GC CPU pause time stats. dt should be
// the actual time spent paused, for orthogonality. maxProcs should be GOMAXPROCS,
// not work.stwprocs, since this number must be comparable to a total time computed
// from GOMAXPROCS.
internal static void accumulateGCPauseTime(this ref cpuStats s, int64 dt, int32 maxProcs) {
    var cpu = dt * (int64)maxProcs;
    s.GCPauseTime += cpu;
    s.GCTotalTime += cpu;
}

// accumulate takes a cpuStats and adds in the current state of all GC CPU
// counters.
//
// gcMarkPhase indicates that we're in the mark phase and that certain counter
// values should be used.
internal static void accumulate(this ref cpuStats s, int64 now, bool gcMarkPhase) {
    // N.B. Mark termination and sweep termination pauses are
    // accumulated in work.cpuStats at the end of their respective pauses.
    int64 markAssistCpu = default!;
    
    int64 markDedicatedCpu = default!;
    
    int64 markFractionalCpu = default!;
    
    int64 markIdleCpu = default!;
    if (gcMarkPhase) {
        // N.B. These stats may have stale values if the GC is not
        // currently in the mark phase.
        markAssistCpu = ᏑgcController.of(gcControllerState.ᏑassistTime).Load();
        markDedicatedCpu = ᏑgcController.of(gcControllerState.ᏑdedicatedMarkTime).Load();
        markFractionalCpu = ᏑgcController.of(gcControllerState.ᏑfractionalMarkTime).Load();
        markIdleCpu = ᏑgcController.of(gcControllerState.ᏑidleMarkTime).Load();
    }
    // The rest of the stats below are either derived from the above or
    // are reset on each mark termination.
    var scavAssistCpu = ᏑΔscavenge.of(runtime_package.Δscavengeᴛ1.ᏑassistTime).Load();
    var scavBgCpu = ᏑΔscavenge.of(runtime_package.Δscavengeᴛ1.ᏑbackgroundTime).Load();
    // Update cumulative GC CPU stats.
    s.GCAssistTime += markAssistCpu;
    s.GCDedicatedTime += markDedicatedCpu + markFractionalCpu;
    s.GCIdleTime += markIdleCpu;
    s.GCTotalTime += markAssistCpu + markDedicatedCpu + markFractionalCpu + markIdleCpu;
    // Update cumulative scavenge CPU stats.
    s.ScavengeAssistTime += scavAssistCpu;
    s.ScavengeBgTime += scavBgCpu;
    s.ScavengeTotalTime += scavAssistCpu + scavBgCpu;
    // Update total CPU.
    s.TotalTime = sched.totaltime + (now - sched.procresizetime) * (int64)gomaxprocs;
    s.IdleTime += Ꮡsched.of(schedt.ᏑidleTime).Load();
    // Compute userTime. We compute this indirectly as everything that's not the above.
    //
    // Since time spent in _Pgcstop is covered by gcPauseTime, and time spent in _Pidle
    // is covered by idleTime, what we're left with is time spent in _Prunning and _Psyscall,
    // the latter of which is fine because the P will either go idle or get used for something
    // else via sysmon. Meanwhile if we subtract GC time from whatever's left, we get non-GC
    // _Prunning time. Note that this still leaves time spent in sweeping and in the scheduler,
    // but that's fine. The overwhelming majority of this time will be actual user time.
    s.UserTime = s.TotalTime - (s.GCTotalTime + s.ScavengeTotalTime + s.IdleTime);
}

} // end runtime_package
