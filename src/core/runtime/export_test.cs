// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Export guts for testing.
global using TimeTimer = global::go.runtime_package.timeTimer;
global using CPUStats = global::go.runtime_package.cpuStats;
global using G = global::go.runtime_package.g;
global using Sudog = global::go.runtime_package.sudog;
global using Mutex = global::go.runtime_package.mutex;

namespace go;

using abi = @internal.abi_package;
using goarch = @internal.goarch_package;
using goos = @internal.goos_package;
using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using static global::go.runtime_package;
using ꓸꓸꓸAddrRange = Span<runtime_internal_test_package.AddrRange>;
using ꓸꓸꓸunsafeꓸPointer = Span<unsafe_package.Pointer>;

partial class runtime_internal_test_package {

public static Func<uint64, uint64, uint64> Fadd64 = fadd64;

public static Func<uint64, uint64, uint64> Fsub64 = fsub64;

public static Func<uint64, uint64, uint64> Fmul64 = fmul64;

public static Func<uint64, uint64, uint64> Fdiv64 = fdiv64;

public static Func<uint64, uint32> F64to32 = f64to32;

public static Func<uint32, uint64> F32to64 = f32to64;

public static Func<uint64, uint64, (int32, bool)> Fcmp64 = fcmp64;

public static Func<int64, uint64> Fintto64 = fintto64;

public static Func<uint64, (int64, bool)> F64toint = f64toint;

public static Action Entersyscall;
internal static void initᴛEntersyscall() { Entersyscall = entersyscall; }

public static Action Exitsyscall;
internal static void initᴛExitsyscall() { Exitsyscall = exitsyscall; }

public static Func<bool> LockedOSThread = lockedOSThread;

public static Func<ж<uintptr>, uintptr, uintptr> Xadduintptr = atomic.Xadduintptr;

public static ж<bool> ReadRandomFailed;
internal static void initᴛReadRandomFailed() { ReadRandomFailed = ᏑreadRandomFailed; }

public static Func<float64, float64> Fastlog2;
internal static void initᴛFastlog2() { Fastlog2 = fastlog2; }

public static Func<@string, (nint, bool)> Atoi = atoi;

public static Func<@string, (int32, bool)> Atoi32 = atoi32;

public static Func<@string, (int64, bool)> ParseByteCount = parseByteCount;

public static Func<int64> Nanotime = nanotime;

public static Action NetpollBreak;
internal static void initᴛNetpollBreak() { NetpollBreak = netpollBreak; }

public static Action<uint32> Usleep;
internal static void initᴛUsleep() { Usleep = usleep; }

public static uintptr PhysPageSize;
internal static void initᴛPhysPageSize() { PhysPageSize = physPageSize; }

public static uintptr PhysHugePageSize;
internal static void initᴛPhysHugePageSize() { PhysHugePageSize = physHugePageSize; }

public static Action NetpollGenericInit;
internal static void initᴛNetpollGenericInit() { NetpollGenericInit = netpollGenericInit; }

public static Action<@unsafe.Pointer, @unsafe.Pointer, uintptr> Memmove = memmove;

public static Action<@unsafe.Pointer, uintptr> MemclrNoHeapPointers = memclrNoHeapPointers;

public static Action<any, any> CgoCheckPointer;
internal static void initᴛCgoCheckPointer() { CgoCheckPointer = cgoCheckPointer; }

public const bool CrashStackImplemented = /* crashStackImplemented */ false;

public static UntypedInt TracebackInnerFrames => /* tracebackInnerFrames */ 50;

public static UntypedInt TracebackOuterFrames => /* tracebackOuterFrames */ 50;

public static Action<any, @unsafe.Pointer> MapKeys = keys;

public static Action<any, @unsafe.Pointer> MapValues = values;

internal static slice<slice<global::go.runtime_package.lockRank>> LockPartialOrder;
internal static void initᴛLockPartialOrder() { LockPartialOrder = lockPartialOrder; }

[GoType("num:nint")] public partial struct LockRank;

public static @string String(this LockRank l) {
    return ((global::go.runtime_package.lockRank)(nint)l).String();
}

public const bool PreemptMSupported = /* preemptMSupported */ true;

[GoType] public partial struct LFNode {
    public uint64 Next;
    public uintptr Pushcnt;
}

public static void LFStackPush(ж<uint64> Ꮡhead, ж<LFNode> Ꮡnode) {
    (Ꮡhead.Reinterpret<uint64, global::go.runtime_package.lfstack>()).push(Ꮡnode.Reinterpret<LFNode, global::go.runtime_package.lfnode>());
}

public static ж<LFNode> LFStackPop(ж<uint64> Ꮡhead) {
    return (ж<LFNode>)(uintptr)((Ꮡhead.Reinterpret<uint64, global::go.runtime_package.lfstack>()).pop());
}

public static void LFNodeValidate(ж<LFNode> Ꮡnode) {
    lfnodeValidate(Ꮡnode.Reinterpret<LFNode, global::go.runtime_package.lfnode>());
}

public static void Netpoll(int64 delta) {
    systemstack(() => {
        netpoll(delta);
    });
}

public static slice<byte> /*ret*/ PointerMask(any x) {
    slice<byte> ret = default!;

    systemstack(() => {
        ret = pointerMask(x);
    });
    return ret;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runqIsNotEmptyInitiallyˢ = "runq is not empty initially"u8;
internal static readonly @string badElementˢ = "bad element"u8;
internal static readonly @string runqIsNotEmptyAfterwardsˢ = "runq is not empty afterwards"u8;

public static void RunSchedLocalQueueTest() {
    var pp = @new<global::go.runtime_package.Δp>();
    var gs = new slice<global::go.runtime_package.g>(len((~pp).runq), () => new());
    Escape(gs); // Ensure gs doesn't move, since we use guintptrs
    for (nint i = 0; i < len((~pp).runq); i++) {
        {
            var (g, _) = runqget(pp); if (g != nil) {
                @throw(runqIsNotEmptyInitiallyˢ);
            }
        }
        for (nint j = 0; j < i; j++) {
            runqput(pp, Ꮡ(gs, i), false);
        }
        for (nint j = 0; j < i; j++) {
            {
                var (g, _) = runqget(pp); if (g != Ꮡ(gs, i)) {
                    print((@string)"bad element at iter "u8, i, (@string)"/"u8, j, (@string)"\n"u8);
                    @throw(badElementˢ);
                }
            }
        }
        {
            var (g, _) = runqget(pp); if (g != nil) {
                @throw(runqIsNotEmptyAfterwardsˢ);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badStealˢ = "bad steal"u8;

public static void RunSchedLocalQueueStealTest() {
    var p1 = @new<global::go.runtime_package.Δp>();
    var p2 = @new<global::go.runtime_package.Δp>();
    var gs = new slice<global::go.runtime_package.g>(len((~p1).runq), () => new());
    Escape(gs); // Ensure gs doesn't move, since we use guintptrs
    for (nint i = 0; i < len((~p1).runq); i++) {
        for (nint j = 0; j < i; j++) {
            gs[j].sig = 0;
            runqput(p1, Ꮡ(gs, j), false);
        }
        var gp = runqsteal(p2, p1, true);
        nint s = 0;
        if (gp != nil) {
            s++;
            gp.Value.sig++;
        }
        while (ᐧ) {
            (gp, _) = runqget(p2);
            if (gp == nil) {
                break;
            }
            s++;
            gp.Value.sig++;
        }
        while (ᐧ) {
            (gp, _) = runqget(p1);
            if (gp == nil) {
                break;
            }
            gp.Value.sig++;
        }
        for (nint j = 0; j < i; j++) {
            if (gs[j].sig != 1) {
                print((@string)"bad element "u8, j, (@string)"("u8, gs[j].sig, (@string)") at iter "u8, i, (@string)"\n"u8);
                @throw(badElementˢ);
            }
        }
        if (s != i / 2 && s != i / 2 + 1) {
            print((@string)"bad steal "u8, s, (@string)", want "u8, i / 2, (@string)" or "u8, i / 2 + 1, (@string)", iter "u8, i, (@string)"\n"u8);
            @throw(badStealˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string queueIsEmptyˢ = "queue is empty"u8;

public static void RunSchedLocalQueueEmptyTest(nint iters) {
    // Test that runq is not spuriously reported as empty.
    // Runq emptiness affects scheduling decisions and spurious emptiness
    // can lead to underutilization (both runnable Gs and idle Ps coexist
    // for arbitrary long time).
    var done = new channel<bool>(1);
    var Δp = @new<global::go.runtime_package.Δp>();
    var gs = new slice<global::go.runtime_package.g>(2, () => new());
    Escape(gs); // Ensure gs doesn't move, since we use guintptrs
    var ready = @new<uint32>();
    for (nint i = 0; i < iters; i++) {
        ready.Value = 0;
        var next0 = ((nint)(i & 1)) == 0;
        var next1 = ((nint)(i & 2)) == 0;
        runqput(Δp, Ꮡ(gs, 0), next0);
        var doneʗ1 = done;
        var pʗ1 = Δp;
        var readyʗ1 = ready;
        goǃ(() => {
            for (atomic.Xadd(readyʗ1, 1); atomic.Load(readyʗ1) != 2; ) {
            }
            if (runqempty(pʗ1)) {
                println((@string)"next:"u8, next0, next1);
                @throw(queueIsEmptyˢ);
            }
            doneʗ1.ᐸꟷ(true);
        });
        for (atomic.Xadd(ready, 1); atomic.Load(ready) != 2; ) {
        }
        runqput(Δp, Ꮡ(gs, 1), next1);
        runqget(Δp);
        ᐸꟷ(done);
        runqget(Δp);
    }
}

public static Func<@string, uintptr, uintptr> StringHash = stringHash;
public static Func<slice<byte>, uintptr, uintptr> BytesHash = bytesHash;
public static Func<uint32, uintptr, uintptr> Int32Hash = int32Hash;
public static Func<uint64, uintptr, uintptr> Int64Hash = int64Hash;
public static Func<@unsafe.Pointer, uintptr, uintptr, uintptr> MemHash = memhash;
public static Func<@unsafe.Pointer, uintptr, uintptr> MemHash32 = memhash32;
public static Func<@unsafe.Pointer, uintptr, uintptr> MemHash64 = memhash64;
public static Func<any, uintptr, uintptr> EfaceHash;
internal static void initᴛEfaceHash() { EfaceHash = efaceHash; }
internal static Func<ifaceHash_i, uintptr, uintptr> IfaceHash;
internal static void initᴛIfaceHash() { IfaceHash = ifaceHash; }

public static ж<bool> UseAeshash;
internal static void initᴛUseAeshash() { UseAeshash = ᏑuseAeshash; }

public static void MemclrBytes(slice<byte> bʗp) {
    ref var b = ref heap(bʗp, out var Ꮡb);

    var s = Ꮡb.Reinterpret<slice<byte>, global::go.runtime_package.Δsliceᴛ>();
    memclrNoHeapPointers((~s).Δarray, (uintptr)(~s).len);
}

public const float32 HashLoad = /* hashLoad */ 0.875f;

// entry point for testing
public static @string /*s*/ GostringW(slice<uint16> w) {
    @string s = default!;

    var wʗ1 = w;
    systemstack(() => {
        s = gostringw(Ꮡ(wʗ1, 0));
    });
    return s;
}

public static Func<ж<byte>, int32, int32, int32> Open;
internal static void initᴛOpen() { Open = (ж<byte> ᴛ0, int32 ᴛ1, int32 ᴛ2) => open(ref ᴛ0.DerefOrNull(), ᴛ1, ᴛ2); }

public static Func<int32, int32> ΔClose;
internal static void initᴛΔClose() { ΔClose = closefd; }

public static Func<int32, @unsafe.Pointer, int32, int32> ΔRead;
internal static void initᴛΔRead() { ΔRead = read; }

public static Func<uintptr, @unsafe.Pointer, int32, int32> ΔWrite;
internal static void initᴛΔWrite() { ΔWrite = write; }

public static slice<@string> Envs() {
    return envs;
}

public static void SetEnvs(slice<@string> e) {
    envs = e;
}

public static UntypedInt PtrSize => /* goarch.PtrSize */ 8;

public static ж<int64> ForceGCPeriod;
internal static void initᴛForceGCPeriod() { ForceGCPeriod = Ꮡforcegcperiod; }

// SetTracebackEnv is like runtime/debug.SetTraceback, but it raises
// the "environment" traceback level, so later calls to
// debug.SetTraceback (e.g., from testing timeouts) can't lower it.
public static void SetTracebackEnv(@string level) {
    setTraceback(level);
    traceback_env = traceback_cache;
}

public static Func<@unsafe.Pointer, uint32> ReadUnaligned32 = readUnaligned32;

public static Func<@unsafe.Pointer, uint64> ReadUnaligned64 = readUnaligned64;

public static (uintptr pagesInUse, uintptr counted) CountPagesInUse() {
    uintptr pagesInUse = default!;
    uintptr counted = default!;

    var stw = stopTheWorld(stwForTestCountPagesInUse);
    pagesInUse = Ꮡmheap_.of(global::go.runtime_package.mheap.ᏑpagesInUse).Load();
    foreach (var (_, s) in mheap_.allspans) {
        if (s.of(global::go.runtime_package.mspan.Ꮡstate).get() == mSpanInUse) {
            counted += s.Value.npages;
        }
    }
    startTheWorld(stw);
    return (pagesInUse, counted);
}

public static uint32 Fastrand() {
    return (uint32)rand();
}

public static uint64 Fastrand64() {
    return rand();
}

public static uint32 Fastrandn(uint32 n) {
    return randn(n);
}

[GoType("global::go.runtime_package.profBuf")] public partial struct ProfBuf;

public static ж<ProfBuf> NewProfBuf(nint hdrsize, nint bufwords, nint tags) {
    return newProfBuf(hdrsize, bufwords, tags).Reinterpret<global::go.runtime_package.profBuf, ProfBuf>();
}

public static void Write(this ж<ProfBuf> Ꮡp, ж<@unsafe.Pointer> Ꮡtag, int64 now, slice<uint64> hdr, slice<uintptr> stk) {
    (Ꮡp.Reinterpret<ProfBuf, global::go.runtime_package.profBuf>()).write(Ꮡtag, now, hdr, stk);
}

internal static global::go.runtime_package.profBufReadMode ProfBufBlocking => /* profBufBlocking */ 0;
internal static global::go.runtime_package.profBufReadMode ProfBufNonBlocking => /* profBufNonBlocking */ 1;

internal static (slice<uint64>, slice<@unsafe.Pointer>, bool) Read(this ж<ProfBuf> Ꮡp, global::go.runtime_package.profBufReadMode mode) {
    return (Ꮡp.Reinterpret<ProfBuf, global::go.runtime_package.profBuf>()).read(mode);
}

public static void Close(this ж<ProfBuf> Ꮡp) {
    (Ꮡp.Reinterpret<ProfBuf, global::go.runtime_package.profBuf>()).close();
}

internal static CPUStats ReadCPUStats() {
    return work.cpuStats;
}

public static void ReadMetricsSlow(ж<global::go.runtime_package.MemStats> ᏑmemStats, @unsafe.Pointer samplesp, nint len, nint cap) {
    var stw = stopTheWorld(stwForTestReadMetricsSlow);
    // Initialize the metrics beforehand because this could
    // allocate and skew the stats.
    metricsLock();
    initMetrics();
    systemstack(() => {
        // Donate the racectx to g0. readMetricsLocked calls into the race detector
        // via map access.
        getg().Value.racectx = getg().Value.m.Value.curg.Value.racectx;
        // Read the metrics once before in case it allocates and skews the metrics.
        // readMetricsLocked is designed to only allocate the first time it is called
        // with a given slice of samples. In effect, this extra read tests that this
        // remains true, since otherwise the second readMetricsLocked below could
        // allocate before it returns.
        readMetricsLocked(samplesp, len, cap);
        // Read memstats first. It's going to flush
        // the mcaches which readMetrics does not do, so
        // going the other way around may result in
        // inconsistent statistics.
        readmemstats_m(ref (ᏑmemStats).DerefOrNull());
        // Read metrics again. We need to be sure we're on the
        // system stack with readmemstats_m so that we don't call into
        // the stack allocator and adjust metrics between there and here.
        readMetricsLocked(samplesp, len, cap);
        // Undo the donation.
        getg().Value.racectx = 0;
    });
    metricsUnlock();
    startTheWorld(stw);
}

public static ж<bool> DoubleCheckReadMemStats;
internal static void initᴛDoubleCheckReadMemStats() { DoubleCheckReadMemStats = ᏑdoubleCheckReadMemStats; }

[GoType("dyn")] internal partial struct ReadMemStatsSlow_bySize {
    public uint64 Mallocs, Frees;
}

// ReadMemStatsSlow returns both the runtime-computed MemStats and
// MemStats accumulated by scanning the heap.
public static (global::go.runtime_package.MemStats @base, global::go.runtime_package.MemStats slow) ReadMemStatsSlow() {
    ref var @base = ref heap(new global::go.runtime_package.MemStats(), out var Ꮡbase);
    global::go.runtime_package.MemStats slow = new();

    var stw = stopTheWorld(stwForTestReadMemStatsSlow);
    // Run on the system stack to avoid stack growth allocation.
    systemstack(() => {
        // Make sure stats don't change.
        getg().Value.m.Value.mallocing++;
        readmemstats_m(ref (Ꮡbase).DerefOrNull());
        // Initialize slow from base and zero the fields we're
        // recomputing.
        slow = Ꮡbase.Value.ΔClone();
        slow.Alloc = 0;
        slow.TotalAlloc = 0;
        slow.Mallocs = 0;
        slow.Frees = 0;
        slow.HeapReleased = 0;
        array<ReadMemStatsSlow_bySize> bySize = new(68); /* _NumSizeClasses */
        // Add up current allocations in spans.
        foreach (var (_, s) in mheap_.allspans) {
            if (s.of(global::go.runtime_package.mspan.Ꮡstate).get() != mSpanInUse) {
                continue;
            }
            if (s.isUnusedUserArenaChunk()) {
                continue;
            }
            {
                var sizeclass = (~s).spanclass.sizeclass(); if (sizeclass == 0){
                    slow.Mallocs++;
                    slow.Alloc += (uint64)(~s).elemsize;
                } else {
                    slow.Mallocs += (uint64)(~s).allocCount;
                    slow.Alloc += (uint64)(~s).allocCount * (uint64)(~s).elemsize;
                    bySize[sizeclass].Mallocs += (uint64)(~s).allocCount;
                }
            }
        }
        // Add in frees by just reading the stats for those directly.
        ref var m = ref heap(new global::go.runtime_package.heapStatsDelta(), out var Ꮡm);
        Ꮡmemstats.of(global::go.runtime_package.mstats.ᏑheapStats).unsafeRead(Ꮡm);
        // Collect per-sizeclass free stats.
        uint64 smallFree = default!;
        for (nint i = 0; i < _NumSizeClasses; i++) {
            slow.Frees += m.smallFreeCount[i];
            bySize[i].Frees += m.smallFreeCount[i];
            bySize[i].Mallocs += m.smallFreeCount[i];
            smallFree += m.smallFreeCount[i] * (uint64)class_to_size[i];
        }
        slow.Frees += m.tinyAllocCount + m.largeFreeCount;
        slow.Mallocs += slow.Frees;
        slow.TotalAlloc = slow.Alloc + m.largeFree + smallFree;
        foreach (var (i, _) in slow.BySize) {
            slow.BySize[i].Mallocs = bySize[i].Mallocs;
            slow.BySize[i].Frees = bySize[i].Frees;
        }
        for (global::go.runtime_package.chunkIdx i = mheap_.pages.start; i < mheap_.pages.end; i++) {
            var chunk = Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡpages).tryChunkOf(i);
            if (chunk == nil) {
                continue;
            }
            nuint pg = chunk.of(global::go.runtime_package.pallocData.Ꮡscavenged).popcntRange(0, pallocChunkPages);
            slow.HeapReleased += (uint64)pg * (uint64)pageSize;
        }
        foreach (var (_, Δp) in allp) {
            nint pg = sys.OnesCount64((~Δp).pcache.scav);
            slow.HeapReleased += (uint64)pg * (uint64)pageSize;
        }
        getg().Value.m.Value.mallocing--;
    });
    startTheWorld(stw);
    return (@base, slow);
}

// ShrinkStackAndVerifyFramePointers attempts to shrink the stack of the current goroutine
// and verifies that unwinding the new stack doesn't crash, even if the old
// stack has been freed or reused (simulated via poisoning).
public static void ShrinkStackAndVerifyFramePointers() {
    GoFrame ᒐ = default;
    try {
        nint before = stackPoisonCopy;
        defer(() => {
            stackPoisonCopy = before;
        }, ref ᒐ);
        stackPoisonCopy = 1;
        var gp = getg();
        var gpʗ1 = gp;
        systemstack(() => {
            shrinkstack(gpʗ1);
        });
        // If our new stack contains frame pointers into the old stack, this will
        // crash because the old stack has been poisoned.
        FPCallers(new slice<uintptr>(1024));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// BlockOnSystemStack switches to the system stack, prints "x\n" to
// stderr, and blocks in a stack containing
// "runtime.blockOnSystemStackInternal".
public static void BlockOnSystemStack() {
    systemstack(blockOnSystemStackInternal);
}

internal static void blockOnSystemStackInternal() {
    print((@string)"x\n"u8);
    @lock(Ꮡdeadlock);
    @lock(Ꮡdeadlock);
}

[GoType] public partial struct RWMutex {
    internal global::go.runtime_package.rwmutex rw;
}

public static void Init(this ж<RWMutex> Ꮡrw) {
    Ꮡrw.of(RWMutex.Ꮡrw).init(lockRankTestR, lockRankTestRInternal, lockRankTestW);
}

public static void RLock(this ж<RWMutex> Ꮡrw) {
    Ꮡrw.of(RWMutex.Ꮡrw).rlock();
}

public static void RUnlock(this ж<RWMutex> Ꮡrw) {
    Ꮡrw.of(RWMutex.Ꮡrw).runlock();
}

public static void Lock(this ж<RWMutex> Ꮡrw) {
    Ꮡrw.of(RWMutex.Ꮡrw).@lock();
}

public static void Unlock(this ж<RWMutex> Ꮡrw) {
    Ꮡrw.of(RWMutex.Ꮡrw).unlock();
}

public static (uint32 external, uint32 @internal) LockOSCounts() {
    var gp = getg();
    if ((~(~gp).m).lockedExt + (~(~gp).m).lockedInt == 0){
        if ((~gp).lockedm != 0) {
            throw panic("lockedm on non-locked goroutine");
        }
    } else {
        if ((~gp).lockedm == 0) {
            throw panic("nil lockedm on locked goroutine");
        }
    }
    return ((~(~gp).m).lockedExt, (~(~gp).m).lockedInt);
}

//go:noinline
public static nint TracebackSystemstack(slice<uintptr> stk, nint i) {
    if (i == 0) {
        var (pc, sp) = (sys.GetCallerPC(), sys.GetCallerSP());
        ref var u = ref heap(new global::go.runtime_package.unwinder(), out var Ꮡu);
        Ꮡu.initAt(pc, sp, 0, getg(), unwindJumpStack); // Don't ignore errors, for testing
        return tracebackPCs(Ꮡu, 0, stk);
    }
    nint n = 0;
    var stkʗ1 = stk;
    systemstack(() => {
        n = TracebackSystemstack(stkʗ1, i - 1);
    });
    return n;
}

public static void KeepNArenaHints(nint n) {
    var hint = mheap_.arenaHints;
    for (nint i = 1; i < n; i++) {
        hint = hint.Value.next;
        if (hint == nil) {
            return;
        }
    }
    hint.Value.next = default!;
}

// MapNextArenaHint reserves a page at the next arena growth hint,
// preventing the arena from growing there, and returns the range of
// addresses that are no longer viable.
//
// This may fail to reserve memory. If it fails, it still returns the
// address range it attempted to reserve.
public static (uintptr start, uintptr end, bool ok) MapNextArenaHint() {
    uintptr start = default!;
    uintptr end = default!;
    bool ok = default!;

    var hint = mheap_.arenaHints;
    var addr = hint.Value.addr;
    if ((~hint).down){
        (start, end) = (addr - (uintptr)heapArenaBytes, addr);
        addr -= physPageSize;
    } else {
        (start, end) = (addr, addr + (uintptr)heapArenaBytes);
    }
    @unsafe.Pointer got = (uintptr)sysReserve((@unsafe.Pointer)addr, physPageSize);
    ok = (addr == (uintptr)got);
    if (!ok) {
        // We were unable to get the requested reservation.
        // Release what we did get and fail.
        sysFreeOS(got, physPageSize);
    }
    return (start, end, ok);
}

public static uintptr GetNextArenaHint() {
    return (~mheap_.arenaHints).addr;
}

internal static ж<G> Getg() {
    return getg();
}

public static uint64 Goid() {
    return (~getg()).goid;
}

internal static bool GIsWaitingOnMutex(ж<G> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    return readgstatus(Ꮡgp) == _Gwaiting && gp.waitreason.isMutexWait();
}

public static ж<bool> CasGStatusAlwaysTrack;
internal static void initᴛCasGStatusAlwaysTrack() { CasGStatusAlwaysTrack = ᏑcasgstatusAlwaysTrack; }

//go:noinline
public static byte PanicForTesting(slice<byte> b, nint i) {
    return unexportedPanicForTesting(b, i);
}

//go:noinline
internal static byte unexportedPanicForTesting(slice<byte> b, nint i) {
    return b[i];
}

public static void G0StackOverflow() {
    systemstack(() => {
        var g0 = getg();
        var sp = sys.GetCallerSP();
        // The stack bounds for g0 stack is not always precise.
        // Use an artificially small stack, to trigger a stack overflow
        // without actually run out of the system stack (which may seg fault).
        g0.Value.stack.lo = sp - 4096 - (uintptr)stackSystem;
        g0.Value.stackguard0 = (~g0).stack.lo + (uintptr)stackGuard;
        g0.Value.stackguard1 = g0.Value.stackguard0;
        stackOverflow(nil);
    });
}

internal static void stackOverflow(ж<byte> Ꮡx) {
    ref var buf = ref heap(new array<byte>(256), out var Ꮡbuf);
    stackOverflow(Ꮡbuf.at<byte>(0));
}

public static void RunGetgThreadSwitchTest() {
    // Test that getg works correctly with thread switch.
    // With gccgo, if we generate getg inlined, the backend
    // may cache the address of the TLS variable, which
    // will become invalid after a thread switch. This test
    // checks that the bad caching doesn't happen.
    var ch = new channel<nint>(0);
    goǃ((channel<nint> chΔ1) => {
        chΔ1.ᐸꟷ(5);
        LockOSThread();
    }, ch);
    var g1 = getg();
    // Block on a receive. This is likely to get us a thread
    // switch. If we yield to the sender goroutine, it will
    // lock the thread, forcing us to resume on a different
    // thread.
    ᐸꟷ(ch);
    var g2 = getg();
    if (g1 != g2) {
        throw panic("g1 != g2");
    }
    // Also test getg after some control flow, as the
    // backend is sensitive to control flow.
    var g3 = getg();
    if (g1 != g3) {
        throw panic("g1 != g3");
    }
}

public static UntypedInt PageSize => /* pageSize */ 8192;
public static UntypedInt PallocChunkPages => /* pallocChunkPages */ 512;
public static UntypedInt PageAlloc64Bit => /* pageAlloc64Bit */ 1;
public static uintptr PallocSumBytes => /* pallocSumBytes */ 8;

[GoType("num:uint64")] public partial struct PallocSum;

public static PallocSum PackPallocSum(nuint start, nuint max, nuint end) {
    return ((PallocSum)(uint64)packPallocSum(start, max, end));
}

public static nuint Start(this PallocSum m) {
    return ((global::go.runtime_package.pallocSum)(uint64)m).start();
}

public static nuint Max(this PallocSum m) {
    return ((global::go.runtime_package.pallocSum)(uint64)m).max();
}

public static nuint End(this PallocSum m) {
    return ((global::go.runtime_package.pallocSum)(uint64)m).end();
}

[GoType("global::go.runtime_package.pallocBits")] public partial struct ΔPallocBits;

public static (nuint, nuint) Find(this ж<ΔPallocBits> Ꮡb, uintptr npages, nuint searchIdx) {
    return (Ꮡb.Reinterpret<ΔPallocBits, global::go.runtime_package.pallocBits>()).find(npages, searchIdx);
}

public static void AllocRange(this ж<ΔPallocBits> Ꮡb, nuint i, nuint n) {
    (Ꮡb.Reinterpret<ΔPallocBits, global::go.runtime_package.pallocBits>()).allocRange(i, n);
}

public static void Free(this ж<ΔPallocBits> Ꮡb, nuint i, nuint n) {
    (Ꮡb.Reinterpret<ΔPallocBits, global::go.runtime_package.pallocBits>()).free(i, n);
}

public static PallocSum Summarize(this ж<ΔPallocBits> Ꮡb) {
    return ((PallocSum)(uint64)(Ꮡb.Reinterpret<ΔPallocBits, global::go.runtime_package.pallocBits>()).summarize());
}

public static nuint PopcntRange(this ж<ΔPallocBits> Ꮡb, nuint i, nuint n) {
    return (Ꮡb.Reinterpret<ΔPallocBits, global::go.runtime_package.pageBits>()).popcntRange(i, n);
}

// SummarizeSlow is a slow but more obviously correct implementation
// of (*pallocBits).summarize. Used for testing.
public static PallocSum SummarizeSlow(ж<ΔPallocBits> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nuint start = default!;
    nuint most = default!;
    nuint end = default!;
    const nuint N = /* uint(len(b)) * 64 */ 512;
    while (start < N && (Ꮡb.Reinterpret<ΔPallocBits, global::go.runtime_package.pageBits>()).get(start) == 0) {
        start++;
    }
    while (end < N && (Ꮡb.Reinterpret<ΔPallocBits, global::go.runtime_package.pageBits>()).get(N - end - 1) == 0) {
        end++;
    }
    nuint run = (nuint)0;
    for (nuint i = (nuint)0; i < N; i++) {
        if ((Ꮡb.Reinterpret<ΔPallocBits, global::go.runtime_package.pageBits>()).get(i) == 0){
            run++;
        } else {
            run = 0;
        }
        most = builtin.max(most, run);
    }
    return PackPallocSum(start, most, end);
}

// Expose non-trivial helpers for testing.
public static nuint FindBitRange64(uint64 c, nuint n) {
    return findBitRange64(c, n);
}

// Given two PallocBits, returns a set of bit ranges where
// they differ.
public static slice<BitRange> DiffPallocBits(ж<ΔPallocBits> Ꮡa, ж<ΔPallocBits> Ꮡb) {
    var ba = Ꮡa.Reinterpret<ΔPallocBits, global::go.runtime_package.pageBits>();
    var bb = Ꮡb.Reinterpret<ΔPallocBits, global::go.runtime_package.pageBits>();
    slice<BitRange> d = default!;
    nuint @base = (nuint)0;
    nuint size = (nuint)0;
    for (nuint i = (nuint)0; i < (nuint)8 * 64; i++) {
        if (ba.get(i) != bb.get(i)){
            if (size == 0) {
                @base = i;
            }
            size++;
        } else {
            if (size != 0) {
                d = append(d, new BitRange(@base, size));
            }
            size = 0;
        }
    }
    if (size != 0) {
        d = append(d, new BitRange(@base, size));
    }
    return d;
}

// StringifyPallocBits gets the bits in the bit range r from b,
// and returns a string containing the bits as ASCII 0 and 1
// characters.
public static @string StringifyPallocBits(ж<ΔPallocBits> Ꮡb, BitRange r) {
    @string str = ""u8;
    for (nuint j = r.I; j < r.I + r.N; j++) {
        if ((Ꮡb.Reinterpret<ΔPallocBits, global::go.runtime_package.pageBits>()).get(j) != 0){
            str += "1"u8;
        } else {
            str += "0"u8;
        }
    }
    return str;
}

[GoType("global::go.runtime_package.pallocData")] [GoValueClone("Value")] public partial struct ΔPallocData;

public static (nuint, nuint) FindScavengeCandidate(this ж<ΔPallocData> Ꮡd, nuint searchIdx, uintptr min, uintptr max) {
    return (Ꮡd.Reinterpret<ΔPallocData, global::go.runtime_package.pallocData>()).findScavengeCandidate(searchIdx, min, max);
}

public static void AllocRange(this ж<ΔPallocData> Ꮡd, nuint i, nuint n) {
    (Ꮡd.Reinterpret<ΔPallocData, global::go.runtime_package.pallocData>()).allocRange(i, n);
}

public static void ScavengedSetRange(this ж<ΔPallocData> Ꮡd, nuint i, nuint n) {
    (Ꮡd.Reinterpret<ΔPallocData, global::go.runtime_package.pallocData>()).Value.scavenged.setRange(i, n);
}

public static ж<ΔPallocBits> PallocBits(this ж<ΔPallocData> Ꮡd) {
    return Ꮡ((Ꮡd.Reinterpret<ΔPallocData, global::go.runtime_package.pallocData>()).Value.pallocBits).Reinterpret<global::go.runtime_package.pallocBits, ΔPallocBits>();
}

public static ж<ΔPallocBits> Scavenged(this ж<ΔPallocData> Ꮡd) {
    return Ꮡ((Ꮡd.Reinterpret<ΔPallocData, global::go.runtime_package.pallocData>()).Value.scavenged).Reinterpret<global::go.runtime_package.pageBits, ΔPallocBits>();
}

// Expose fillAligned for testing.
public static uint64 FillAligned(uint64 x, nuint m) {
    return fillAligned(x, m);
}

[GoType("global::go.runtime_package.pageCache")] public partial struct PageCache;

public static uintptr PageCachePages => /* pageCachePages */ 64;

public static PageCache NewPageCache(uintptr @base, uint64 cache, uint64 scav) {
    return (new PageCache(new pageCache(@base: @base, cache: cache, scav: scav)));
}

public static bool Empty(this ж<PageCache> Ꮡc) {
    return (Ꮡc.Reinterpret<PageCache, global::go.runtime_package.pageCache>()).empty();
}

public static uintptr Base(this ж<PageCache> Ꮡc) {
    return (Ꮡc.Reinterpret<PageCache, global::go.runtime_package.pageCache>()).Value.@base;
}

public static uint64 Cache(this ж<PageCache> Ꮡc) {
    return (Ꮡc.Reinterpret<PageCache, global::go.runtime_package.pageCache>()).Value.cache;
}

public static uint64 Scav(this ж<PageCache> Ꮡc) {
    return (Ꮡc.Reinterpret<PageCache, global::go.runtime_package.pageCache>()).Value.scav;
}

public static (uintptr, uintptr) Alloc(this ж<PageCache> Ꮡc, uintptr npages) {
    return (Ꮡc.Reinterpret<PageCache, global::go.runtime_package.pageCache>()).alloc(npages);
}

public static void Flush(this ж<PageCache> Ꮡc, ж<PageAlloc> Ꮡs) {
    var cp = Ꮡc.Reinterpret<PageCache, global::go.runtime_package.pageCache>();
    var sp = Ꮡs.Reinterpret<PageAlloc, global::go.runtime_package.pageAlloc>();
    var cpʗ1 = cp;
    var spʗ1 = sp;
    systemstack(() => {
        // None of the tests need any higher-level locking, so we just
        // take the lock internally.
        @lock((~spʗ1).mheapLock);
        cpʗ1.flush(spʗ1);
        unlock((~spʗ1).mheapLock);
    });
}

[GoType("num:nuint")] public partial struct ChunkIdx;

[GoType("global::go.runtime_package.pageAlloc")] [GoValueClone("Value")] public partial struct PageAlloc;

public static (uintptr, uintptr) Alloc(this ж<PageAlloc> Ꮡp, uintptr npages) {
    var pp = Ꮡp.Reinterpret<PageAlloc, global::go.runtime_package.pageAlloc>();
    uintptr addr = default!;
    uintptr scav = default!;
    var ppʗ1 = pp;
    systemstack(() => {
        // None of the tests need any higher-level locking, so we just
        // take the lock internally.
        @lock((~ppʗ1).mheapLock);
        (addr, scav) = ppʗ1.alloc(npages);
        unlock((~ppʗ1).mheapLock);
    });
    return (addr, scav);
}

public static PageCache AllocToCache(this ж<PageAlloc> Ꮡp) {
    var pp = Ꮡp.Reinterpret<PageAlloc, global::go.runtime_package.pageAlloc>();
    ref var c = ref heap(new PageCache(), out var Ꮡc);
    var ppʗ1 = pp;
    systemstack(() => {
        // None of the tests need any higher-level locking, so we just
        // take the lock internally.
        @lock((~ppʗ1).mheapLock);
        Ꮡc.Value = (new PageCache(ppʗ1.allocToCache()));
        unlock((~ppʗ1).mheapLock);
    });
    return c;
}

public static void Free(this ж<PageAlloc> Ꮡp, uintptr @base, uintptr npages) {
    var pp = Ꮡp.Reinterpret<PageAlloc, global::go.runtime_package.pageAlloc>();
    var ppʗ1 = pp;
    systemstack(() => {
        // None of the tests need any higher-level locking, so we just
        // take the lock internally.
        @lock((~ppʗ1).mheapLock);
        ppʗ1.free(@base, npages);
        unlock((~ppʗ1).mheapLock);
    });
}

public static (ChunkIdx, ChunkIdx) Bounds(this ж<PageAlloc> Ꮡp) {
    return (((ChunkIdx)(nuint)(Ꮡp.Reinterpret<PageAlloc, global::go.runtime_package.pageAlloc>()).Value.start), ((ChunkIdx)(nuint)(Ꮡp.Reinterpret<PageAlloc, global::go.runtime_package.pageAlloc>()).Value.end));
}

public static uintptr /*r*/ Scavenge(this ж<PageAlloc> Ꮡp, uintptr nbytes) {
    uintptr r = default!;

    var pp = Ꮡp.Reinterpret<PageAlloc, global::go.runtime_package.pageAlloc>();
    var ppʗ1 = pp;
    systemstack(() => {
        r = ppʗ1.scavenge(nbytes, default!, true);
    });
    return r;
}

[GoRecv] public static slice<AddrRange> InUse(this ref PageAlloc Δp) {
    var ranges = new slice<AddrRange>(0, () => new(nil), len(Δp.inUse.ranges));
    foreach (var (_, r) in Δp.inUse.ranges) {
        ranges = append(ranges, new AddrRange(r));
    }
    return ranges;
}

// Returns nil if the PallocData's L2 is missing.
public static ж<ΔPallocData> PallocData(this ж<PageAlloc> Ꮡp, ChunkIdx i) {
    global::go.runtime_package.chunkIdx ci = ((global::go.runtime_package.chunkIdx)(nuint)i);
    return (Ꮡp.Reinterpret<PageAlloc, global::go.runtime_package.pageAlloc>()).tryChunkOf(ci).Reinterpret<global::go.runtime_package.pallocData, ΔPallocData>();
}

// AddrRange is a wrapper around addrRange for testing.
[GoType] public partial struct AddrRange {
    internal partial ref global::go.runtime_package.addrRange addrRange { get; }
}

// MakeAddrRange creates a new address range.
public static AddrRange MakeAddrRange(uintptr @base, uintptr limit) {
    return new AddrRange(makeAddrRange(@base, limit));
}

// Base returns the virtual base address of the address range.
public static uintptr Base(this AddrRange a) {
    return a.addrRange.@base.addr();
}

// Base returns the virtual address of the limit of the address range.
public static uintptr Limit(this AddrRange a) {
    return a.addrRange.limit.addr();
}

// Equals returns true if the two address ranges are exactly equal.
public static bool ΔEquals(this AddrRange a, AddrRange b) {
    return a == b;
}

// Size returns the size in bytes of the address range.
public static uintptr Size(this AddrRange a) {
    return a.addrRange.size();
}

// testSysStat is the sysStat passed to test versions of various
// runtime structures. We do actually have to keep track of this
// because otherwise memstats.mappedReady won't actually line up
// with other stats in the runtime during tests.
internal static ж<global::go.runtime_package.sysMemStat> testSysStat;
internal static void initᴛtestSysStat() { testSysStat = Ꮡmemstats.of(global::go.runtime_package.mstats.Ꮡother_sys); }

// AddrRanges is a wrapper around addrRanges for testing.
[GoType] public partial struct AddrRanges {
    internal partial ref global::go.runtime_package.addrRanges addrRanges { get; }
    internal bool mutable;
}

// NewAddrRanges creates a new empty addrRanges.
//
// Note that this initializes addrRanges just like in the
// runtime, so its memory is persistentalloc'd. Call this
// function sparingly since the memory it allocates is
// leaked.
//
// This AddrRanges is mutable, so we can test methods like
// Add.
public static AddrRanges NewAddrRanges() {
    ref var r = ref heap<global::go.runtime_package.addrRanges>(out var Ꮡr);
    r = new addrRanges(nil);
    Ꮡr.init(testSysStat);
    return new AddrRanges(r, true);
}

// MakeAddrRanges creates a new addrRanges populated with
// the ranges in a.
//
// The returned AddrRanges is immutable, so methods like
// Add will fail.
public static AddrRanges MakeAddrRanges(params ꓸꓸꓸAddrRange aʗp) {
    var a = aʗp.sslice();

    // Methods that manipulate the backing store of addrRanges.ranges should
    // not be used on the result from this function (e.g. add) since they may
    // trigger reallocation. That would normally be fine, except the new
    // backing store won't come from the heap, but from persistentalloc, so
    // we'll leak some memory implicitly.
    var ranges = new slice<global::go.runtime_package.addrRange>(0, len(a));
    var total = (uintptr)0;
    foreach (var (_, r) in a) {
        ranges = append(ranges, r.addrRange);
        total += r.Size();
    }
    return new AddrRanges(new addrRanges(
        ranges: ranges,
        totalBytes: total,
        sysStat: testSysStat
    ), false);
}

// Ranges returns a copy of the ranges described by the
// addrRanges.
[GoRecv] public static slice<AddrRange> Ranges(this ref AddrRanges a) {
    var result = new slice<AddrRange>(0, () => new(nil), len(a.addrRanges.ranges));
    foreach (var (_, r) in a.addrRanges.ranges) {
        result = append(result, new AddrRange(r));
    }
    return result;
}

// FindSucc returns the successor to base. See addrRanges.findSucc
// for more details.
[GoRecv] public static nint FindSucc(this ref AddrRanges a, uintptr @base) {
    return a.addrRanges.findSucc(@base);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string attemptToMutateImmutableˢ = "attempt to mutate immutable AddrRanges"u8;

// Add adds a new AddrRange to the AddrRanges.
//
// The AddrRange must be mutable (i.e. created by NewAddrRanges),
// otherwise this method will throw.
public static void Add(this ж<AddrRanges> Ꮡa, AddrRange r) {
    ref var a = ref Ꮡa.DerefOrNull();

    if (!a.mutable) {
        @throw(attemptToMutateImmutableˢ);
    }
    Ꮡa.of(AddrRanges.ᏑaddrRanges).add(r.addrRange);
}

// TotalBytes returns the totalBytes field of the addrRanges.
[GoRecv] public static uintptr TotalBytes(this ref AddrRanges a) {
    return a.addrRanges.totalBytes;
}

// BitRange represents a range over a bitmap.
[GoType] public partial struct BitRange {
    public nuint I, N; // bit index and length in bits
}

// NewPageAlloc creates a new page allocator for testing and
// initializes it with the scav and chunks maps. Each key in these maps
// represents a chunk index and each value is a series of bit ranges to
// set within each bitmap's chunk.
//
// The initialization of the pageAlloc preserves the invariant that if a
// scavenged bit is set the alloc bit is necessarily unset, so some
// of the bits described by scav may be cleared in the final bitmap if
// ranges in chunks overlap with them.
//
// scav is optional, and if nil, the scavenged bitmap will be cleared
// (as opposed to all 1s, which it usually is). Furthermore, every
// chunk index in scav must appear in chunks; ones that do not are
// ignored.
public static ж<PageAlloc> NewPageAlloc(map<ChunkIdx, slice<BitRange>> chunks, map<ChunkIdx, slice<BitRange>> scav) {
    var Δp = @new<global::go.runtime_package.pageAlloc>();
    // We've got an entry, so initialize the pageAlloc.
    Δp.init(@new<global::go.runtime_package.mutex>(), testSysStat, true);
    lockInit((~Δp).mheapLock, lockRankMheap);
    foreach (var (i, init) in chunks) {
        var addr = chunkBase(((global::go.runtime_package.chunkIdx)(nuint)i));
        // Mark the chunk's existence in the pageAlloc.
        var pʗ1 = Δp;
        systemstack(() => {
            @lock((~pʗ1).mheapLock);
            pʗ1.grow(addr, pallocChunkBytes);
            unlock((~pʗ1).mheapLock);
        });
        // Initialize the bitmap and update pageAlloc metadata.
        global::go.runtime_package.chunkIdx ci = chunkIndex(addr);
        var chunk = Δp.chunkOf(ci);
        // Clear all the scavenged bits which grow set.
        chunk.of(global::go.runtime_package.pallocData.Ꮡscavenged).clearRange(0, pallocChunkPages);
        // Simulate the allocation and subsequent free of all pages in
        // the chunk for the scavenge index. This sets the state equivalent
        // with all pages within the index being free.
        Δp.of(global::go.runtime_package.pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).alloc(ci, pallocChunkPages);
        Δp.of(global::go.runtime_package.pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).free(ci, 0, pallocChunkPages);
        // Apply scavenge state if applicable.
        if (scav != default!) {
            {
                var (scvg, ok) = scav[i, ꟷ]; if (ok) {
                    foreach (var (_, s) in scvg) {
                        // Ignore the case of s.N == 0. setRange doesn't handle
                        // it and it's a no-op anyway.
                        if (s.N != 0) {
                            chunk.of(global::go.runtime_package.pallocData.Ꮡscavenged).setRange(s.I, s.N);
                        }
                    }
                }
            }
        }
        // Apply alloc state.
        foreach (var (_, s) in init) {
            // Ignore the case of s.N == 0. allocRange doesn't handle
            // it and it's a no-op anyway.
            if (s.N != 0) {
                chunk.allocRange(s.I, s.N);
                // Make sure the scavenge index is updated.
                Δp.of(global::go.runtime_package.pageAlloc.Ꮡscav).of(pageAlloc_scav.Ꮡindex).alloc(ci, s.N);
            }
        }
        // Update heap metadata for the allocRange calls above.
        var pʗ2 = Δp;
        systemstack(() => {
            @lock((~pʗ2).mheapLock);
            pʗ2.update(addr, pallocChunkPages, false, false);
            unlock((~pʗ2).mheapLock);
        });
    }
    return Δp.Reinterpret<global::go.runtime_package.pageAlloc, PageAlloc>();
}

// FreePageAlloc releases hard OS resources owned by the pageAlloc. Once this
// is called the pageAlloc may no longer be used. The object itself will be
// collected by the garbage collector once it is no longer live.
public static void FreePageAlloc(ж<PageAlloc> Ꮡpp) {
    var Δp = Ꮡpp.Reinterpret<PageAlloc, global::go.runtime_package.pageAlloc>();
    // Free all the mapped space for the summary levels.
    if (pageAlloc64Bit != 0){
        for (nint l = 0; l < summaryLevels; l++) {
            sysFreeOS(@unsafe.Pointer.FromPinnedBox(Ꮡ((~Δp).summary[l], 0)), (uintptr)cap((~Δp).summary[l]) * pallocSumBytes);
        }
    } else {
        var resSize = (uintptr)0;
        foreach (var (_, s) in (~Δp).summary.ΔRangeSnapshot()) {
            resSize += (uintptr)cap(s) * pallocSumBytes;
        }
        sysFreeOS(@unsafe.Pointer.FromPinnedBox(Ꮡ((~Δp).summary[0], 0)), alignUp(resSize, physPageSize));
    }
    // Free extra data structures.
    sysFreeOS(@unsafe.Pointer.FromPinnedBox(Ꮡ((~Δp).scav.index.chunks, 0)), (uintptr)cap((~Δp).scav.index.chunks) * /* unsafe.Sizeof(atomicScavChunkData{}) */ (uintptr)8);
    // Subtract back out whatever we mapped for the summaries.
    // sysUsed adds to p.sysStat and memstats.mappedReady no matter what
    // (and in anger should actually be accounted for), and there's no other
    // way to figure out how much we actually mapped.
    ᏑgcController.of(global::go.runtime_package.gcControllerState.ᏑmappedReady).Add(-(int64)(~Δp).summaryMappedReady);
    testSysStat.add(-(int64)(~Δp).summaryMappedReady);
    // Free the mapped space for chunks.
    foreach (var (i, _) in (~Δp).chunks) {
        {
            var x = (~Δp).chunks[i]; if (x != nil) {
                Δp.Value.chunks[i] = ж<array<global::go.runtime_package.pallocData>>.NilBoxOfDims(8192L);
                // This memory comes from sysAlloc and will always be page-aligned.
                sysFree(@unsafe.Pointer.FromPinnedBox(x), /* unsafe.Sizeof(*p.chunks[0]) */ (uintptr)1048576, testSysStat);
            }
        }
    }
}

// BaseChunkIdx is a convenient chunkIdx value which works on both
// 64 bit and 32 bit platforms, allowing the tests to share code
// between the two.
//
// This should not be higher than 0x100*pallocChunkBytes to support
// mips and mipsle, which only have 31-bit address spaces.
public static ChunkIdx BaseChunkIdx = ((Func<ChunkIdx>)(() => {
    uintptr prefix = default!;
    if (pageAlloc64Bit != 0){
        prefix = 0xc000;
    } else {
        prefix = 0x100;
    }
    var baseAddr = prefix * (uintptr)pallocChunkBytes;
    if (goos.IsAix != 0) {
        baseAddr += arenaBaseOffset;
    }
    return ((ChunkIdx)(nuint)chunkIndex(baseAddr));
}))();

// PageBase returns an address given a chunk index and a page index
// relative to that chunk.
public static uintptr PageBase(ChunkIdx c, nuint pageIdx) {
    return chunkBase(((global::go.runtime_package.chunkIdx)(nuint)c)) + (uintptr)pageIdx * (uintptr)pageSize;
}

[GoType] public partial struct BitsMismatch {
    public uintptr Base;
    public uint64 Got, Want;
}

public static (nint n, bool ok) CheckScavengedBitsCleared(slice<BitsMismatch> mismatches) {
    nint n = default!;
    bool ok = default!;

    ok = true;
    // Run on the system stack to avoid stack growth allocation.
    var mismatchesʗ1 = mismatches;
    systemstack(() => {
        getg().Value.m.Value.mallocing++;
        // Lock so that we can safely access the bitmap.
        @lock(Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡlock));
chunkLoop:
        for (global::go.runtime_package.chunkIdx i = mheap_.pages.start; i < mheap_.pages.end; i++) {
            var chunk = Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡpages).tryChunkOf(i);
            if (chunk == nil) {
                continue;
            }
            for (nint j = 0; j < (nint)(pallocChunkPages / 64); j++) {
                // Run over each 64-bit bitmap section and ensure
                // scavenged is being cleared properly on allocation.
                // If a used bit and scavenged bit are both set, that's
                // an error, and could indicate a larger problem, or
                // an accounting problem.
                var want = (uint64)((~chunk).scavenged[j] & ~(~chunk).pallocBits[j]);
                var got = (~chunk).scavenged[j];
                if (want != got) {
                    ok = false;
                    if (n >= len(mismatchesʗ1)) {
                        goto break_chunkLoop;
                    }
                    mismatchesʗ1[n] = new BitsMismatch(
                        Base: chunkBase(i) + (uintptr)j * 64 * (uintptr)pageSize,
                        Got: got,
                        Want: want
                    );
                    n++;
                }
            }
continue_chunkLoop:;
        }
break_chunkLoop:;
        unlock(Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡlock));
        getg().Value.m.Value.mallocing--;
    });
    return (n, ok);
}

public static uintptr /*leaked*/ PageCachePagesLeaked() {
    uintptr leaked = default!;

    var stw = stopTheWorld(stwForTestPageCachePagesLeaked);
    // Walk over destroyed Ps and look for unflushed caches.
    var deadp = allp[(int)(len(allp))..(int)(cap(allp))];
    foreach (var (_, Δp) in deadp) {
        // Since we're going past len(allp) we may see nil Ps.
        // Just ignore them.
        if (Δp != nil) {
            leaked += (uintptr)sys.OnesCount64((~Δp).pcache.cache);
        }
    }
    startTheWorld(stw);
    return leaked;
}

public static Action<uint32> ProcYield = procyield;

public static Action OSYield;
internal static void initᴛOSYield() { OSYield = osyield; }

internal static Action<ж<global::go.runtime_package.mutex>> ΔLock;
internal static void initᴛΔLock() { ΔLock = @lock; }

internal static Action<ж<global::go.runtime_package.mutex>> ΔUnlock;
internal static void initᴛΔUnlock() { ΔUnlock = unlock; }

internal static Func<ж<global::go.runtime_package.mutex>, bool> MutexContended = mutexContended;

internal static ж<global::go.runtime_package.mutex> SemRootLock(ж<uint32> Ꮡaddr) {
    var root = semtable.rootFor(Ꮡaddr);
    return root.of(global::go.runtime_package.semaRoot.Ꮡlock);
}

public static Action<ж<uint32>> Semacquire;
internal static void initᴛSemacquire() { Semacquire = semacquire; }

public static Action<ж<uint32>, bool, nint> Semrelease1;
internal static void initᴛSemrelease1() { Semrelease1 = semrelease1; }

public static uint32 SemNwait(ж<uint32> Ꮡaddr) {
    var root = semtable.rootFor(Ꮡaddr);
    return root.of(global::go.runtime_package.semaRoot.Ꮡnwait).Load();
}

public static UntypedInt SemTableSize => /* semTabSize */ 251;

// SemTable is a wrapper around semTable exported for testing.
[GoType] public partial struct SemTable {
    internal partial ref global::go.runtime_package.semTable semTable { get; }
}

// Enqueue simulates enqueuing a waiter for a semaphore (or lock) at addr.
[GoRecv] public static void Enqueue(this ref SemTable t, ж<uint32> Ꮡaddr) {
    var s = acquireSudog();
    s.Value.releasetime = 0;
    s.Value.acquiretime = 0;
    s.Value.ticket = 0;
    t.semTable.rootFor(Ꮡaddr).queue(Ꮡaddr, s, false);
}

// Dequeue simulates dequeuing a waiter for a semaphore (or lock) at addr.
//
// Returns true if there actually was a waiter to be dequeued.
[GoRecv] public static bool Dequeue(this ref SemTable t, ж<uint32> Ꮡaddr) {
    var (s, _, _) = t.semTable.rootFor(Ꮡaddr).dequeue(Ꮡaddr);
    if (s != nil) {
        releaseSudog(s);
        return true;
    }
    return false;
}

[GoType("global::go.runtime_package.mspan")] public partial struct MSpan;

// Allocate an mspan for testing.
public static ж<MSpan> AllocMSpan() {
    ref var s = ref heap<ж<global::go.runtime_package.mspan>>(out var Ꮡs);
    systemstack(() => {
        @lock(Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡlock));
        Ꮡs.ValueSlot = (ж<global::go.runtime_package.mspan>)(uintptr)(Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡspanalloc).alloc());
        unlock(Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡlock));
    });
    return s.Reinterpret<global::go.runtime_package.mspan, MSpan>();
}

// Free an allocated mspan.
public static void FreeMSpan(ж<MSpan> Ꮡs) {
    systemstack(() => {
        @lock(Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡlock));
        Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡspanalloc).free(@unsafe.Pointer.FromPinnedBox(Ꮡs));
        unlock(Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡlock));
    });
}

public static nint MSpanCountAlloc(ж<MSpan> Ꮡms, slice<byte> bits) {
    var s = Ꮡms.Reinterpret<MSpan, global::go.runtime_package.mspan>();
    s.Value.nelems = (uint16)(len(bits) * 8);
    s.Value.gcmarkBits = Ꮡ(bits, 0).Reinterpret<byte, global::go.runtime_package.gcBits>();
    nint result = s.countAlloc();
    s.Value.gcmarkBits = default!;
    return result;
}

public static UntypedInt TimeHistSubBucketBits => /* timeHistSubBucketBits */ 2;
public static UntypedInt TimeHistNumSubBuckets => /* timeHistNumSubBuckets */ 4;
public static UntypedInt TimeHistNumBuckets => /* timeHistNumBuckets */ 40;
public static UntypedInt TimeHistMinBucketBits => /* timeHistMinBucketBits */ 9;
public static UntypedInt TimeHistMaxBucketBits => /* timeHistMaxBucketBits */ 48;

[GoType("global::go.runtime_package.timeHistogram")] [GoValueClone("Value")] public partial struct TimeHistogram;

// Count returns the counts for the given bucket, subBucket indices.
// Returns true if the bucket was valid, otherwise returns the counts
// for the overflow bucket if bucket > 0 or the underflow bucket if
// bucket < 0, and false.
public static (uint64, bool) Count(this ж<TimeHistogram> Ꮡth, nint bucket, nint subBucket) {
    var t = Ꮡth.Reinterpret<TimeHistogram, global::go.runtime_package.timeHistogram>();
    if (bucket < 0) {
        return (t.of(global::go.runtime_package.timeHistogram.Ꮡunderflow).Load(), false);
    }
    nint i = bucket * (nint)TimeHistNumSubBuckets + subBucket;
    if (i >= len((~t).counts)) {
        return (t.of(global::go.runtime_package.timeHistogram.Ꮡoverflow).Load(), false);
    }
    return (t.at(global::go.runtime_package.timeHistogram.Ꮡcounts, i).Load(), true);
}

public static void Record(this ж<TimeHistogram> Ꮡth, int64 duration) {
    (Ꮡth.Reinterpret<TimeHistogram, global::go.runtime_package.timeHistogram>()).record(duration);
}

public static Func<slice<float64>> TimeHistogramMetricsBuckets = timeHistogramMetricsBuckets;

public static nint SetIntArgRegs(nint a) {
    @lock(Ꮡfinlock);
    nint old = intArgRegs;
    if (a >= 0) {
        intArgRegs = a;
    }
    unlock(Ꮡfinlock);
    return old;
}

public static bool FinalizerGAsleep() {
    return (uint32)(ᏑfingStatus.Load() & fingWait) != 0;
}

// For GCTestMoveStackOnNextCall, it's important not to introduce an
// extra layer of call, since then there's a return before the "real"
// next call.
public static Action GCTestMoveStackOnNextCall = gcTestMoveStackOnNextCall;

// For GCTestIsReachable, it's important that we do this as a call so
// escape analysis can see through it.
public static uint64 /*mask*/ GCTestIsReachable(params ꓸꓸꓸunsafeꓸPointer ptrsʗp) {
    var ptrs = ptrsʗp.slice();

    return gcTestIsReachable(ptrs.ꓸꓸꓸ);
}

// For GCTestPointerClass, it's important that we do this as a call so
// escape analysis can see through it.
//
// This is nosplit because gcTestPointerClass is.
//
//go:nosplit
public static @string GCTestPointerClass(@unsafe.Pointer Δp) {
    return gcTestPointerClass(Δp);
}

public const bool Raceenabled = /* raceenabled */ false;

public static UntypedFloat GCBackgroundUtilization => /* gcBackgroundUtilization */ 0.25;
public static UntypedFloat GCGoalUtilization => /* gcGoalUtilization */ 0.25;
public static UntypedInt DefaultHeapMinimum => /* defaultHeapMinimum */ 4194304;
public static UntypedInt MemoryLimitHeapGoalHeadroomPercent => /* memoryLimitHeapGoalHeadroomPercent */ 3;
public static UntypedInt MemoryLimitMinHeapGoalHeadroom => /* memoryLimitMinHeapGoalHeadroom */ 1048576;

[GoType] public partial struct GCController {
    internal partial ref global::go.runtime_package.gcControllerState gcControllerState { get; }
}

public static ж<GCController> NewGCController(nint gcPercent, int64 memoryLimit) {
    // Force the controller to escape. We're going to
    // do 64-bit atomics on it, and if it gets stack-allocated
    // on a 32-bit architecture, it may get allocated unaligned
    // space.
    var g = Escape(@new<GCController>());
    g.Value.gcControllerState.test = true; // Mark it as a test copy.
    g.of(GCController.ᏑgcControllerState).init((int32)gcPercent, memoryLimit);
    return g;
}

public static void StartCycle(this ж<GCController> Ꮡc, uint64 stackSize, uint64 globalsSize, float64 scannableFrac, nint gomaxprocs) {
    ref var c = ref Ꮡc.DerefOrNull();

    var (trigger, _) = Ꮡc.of(GCController.ᏑgcControllerState).trigger();
    if (c.heapMarked > trigger) {
        trigger = c.heapMarked;
    }
    Ꮡc.of(GCController.ᏑmaxStackScan).Store(stackSize);
    Ꮡc.of(GCController.ᏑglobalsScan).Store(globalsSize);
    Ꮡc.of(GCController.ᏑheapLive).Store(trigger);
    Ꮡc.of(GCController.ᏑheapScan).Add((int64)((float64)(trigger - c.heapMarked) * scannableFrac));
    Ꮡc.of(GCController.ᏑgcControllerState).startCycle(0, gomaxprocs, new gcTrigger(kind: gcTriggerHeap));
}

public static float64 AssistWorkPerByte(this ж<GCController> Ꮡc) {
    return Ꮡc.of(GCController.ᏑassistWorkPerByte).Load();
}

public static uint64 HeapGoal(this ж<GCController> Ꮡc) {
    return Ꮡc.of(GCController.ᏑgcControllerState).heapGoal();
}

public static uint64 HeapLive(this ж<GCController> Ꮡc) {
    return Ꮡc.of(GCController.ᏑheapLive).Load();
}

[GoRecv] public static uint64 HeapMarked(this ref GCController c) {
    return c.heapMarked;
}

[GoRecv] public static uint64 Triggered(this ref GCController c) {
    return c.triggered;
}

[GoType] public partial struct GCControllerReviseDelta {
    public int64 HeapLive;
    public int64 HeapScan;
    public int64 HeapScanWork;
    public int64 StackScanWork;
    public int64 GlobalsScanWork;
}

public static void Revise(this ж<GCController> Ꮡc, GCControllerReviseDelta d) {
    Ꮡc.of(GCController.ᏑheapLive).Add(d.HeapLive);
    Ꮡc.of(GCController.ᏑheapScan).Add(d.HeapScan);
    Ꮡc.of(GCController.ᏑheapScanWork).Add(d.HeapScanWork);
    Ꮡc.of(GCController.ᏑstackScanWork).Add(d.StackScanWork);
    Ꮡc.of(GCController.ᏑglobalsScanWork).Add(d.GlobalsScanWork);
    Ꮡc.of(GCController.ᏑgcControllerState).revise();
}

public static void EndCycle(this ж<GCController> Ꮡc, uint64 bytesMarked, int64 assistTime, int64 elapsed, nint gomaxprocs) {
    Ꮡc.of(GCController.ᏑassistTime).Store(assistTime);
    Ꮡc.of(GCController.ᏑgcControllerState).endCycle(elapsed, gomaxprocs, false);
    Ꮡc.of(GCController.ᏑgcControllerState).resetLive(bytesMarked);
    Ꮡc.of(GCController.ᏑgcControllerState).commit(false);
}

public static bool AddIdleMarkWorker(this ж<GCController> Ꮡc) {
    return Ꮡc.of(GCController.ᏑgcControllerState).addIdleMarkWorker();
}

public static bool NeedIdleMarkWorker(this ж<GCController> Ꮡc) {
    return Ꮡc.of(GCController.ᏑgcControllerState).needIdleMarkWorker();
}

public static void RemoveIdleMarkWorker(this ж<GCController> Ꮡc) {
    Ꮡc.of(GCController.ᏑgcControllerState).removeIdleMarkWorker();
}

public static void SetMaxIdleMarkWorkers(this ж<GCController> Ꮡc, int32 max) {
    Ꮡc.of(GCController.ᏑgcControllerState).setMaxIdleMarkWorkers(max);
}

internal static bool alwaysFalse;

internal static any escapeSink;

public static T Escape<T>(T x) {
    if (alwaysFalse) {
        escapeSink = x;
    }
    return x;
}

// Acquirem blocks preemption.
public static void Acquirem() {
    acquirem();
}

public static void Releasem() {
    releasem(ref ((~getg()).m).DerefOrNull());
}

public static Func<int64, int32, ж<int32>, int32> Timediv = timediv;

[GoType] public partial struct PIController {
    internal partial ref global::go.runtime_package.piController piController { get; }
}

public static ж<PIController> NewPIController(float64 kp, float64 ti, float64 tt, float64 min, float64 max) {
    return Ꮡ(new PIController(new piController(
        kp: kp,
        ti: ti,
        tt: tt,
        min: min,
        max: max
    )
    ));
}

[GoRecv] public static (float64, bool) Next(this ref PIController c, float64 input, float64 setpoint, float64 period) {
    return c.piController.next(input, setpoint, period);
}

public static UntypedFloat CapacityPerProc => /* capacityPerProc */ 1e+09;
public static UntypedFloat GCCPULimiterUpdatePeriod => /* gcCPULimiterUpdatePeriod */ 1e+07;

[GoType] public partial struct GCCPULimiter {
    internal global::go.runtime_package.gcCPULimiterState limiter;
}

public static ж<GCCPULimiter> NewGCCPULimiter(int64 now, int32 gomaxprocs) {
    // Force the controller to escape. We're going to
    // do 64-bit atomics on it, and if it gets stack-allocated
    // on a 32-bit architecture, it may get allocated unaligned
    // space.
    var l = Escape(@new<GCCPULimiter>());
    l.Value.limiter.test = true;
    l.of(GCCPULimiter.Ꮡlimiter).resetCapacity(now, gomaxprocs);
    return l;
}

[GoRecv] public static uint64 Fill(this ref GCCPULimiter l) {
    return l.limiter.bucket.fill;
}

[GoRecv] public static uint64 Capacity(this ref GCCPULimiter l) {
    return l.limiter.bucket.capacity;
}

[GoRecv] public static uint64 Overflow(this ref GCCPULimiter l) {
    return l.limiter.overflow;
}

public static bool Limiting(this ж<GCCPULimiter> Ꮡl) {
    return Ꮡl.of(GCCPULimiter.Ꮡlimiter).limiting();
}

public static bool NeedUpdate(this ж<GCCPULimiter> Ꮡl, int64 now) {
    return Ꮡl.of(GCCPULimiter.Ꮡlimiter).needUpdate(now);
}

public static void StartGCTransition(this ж<GCCPULimiter> Ꮡl, bool enableGC, int64 now) {
    Ꮡl.of(GCCPULimiter.Ꮡlimiter).startGCTransition(enableGC, now);
}

public static void FinishGCTransition(this ж<GCCPULimiter> Ꮡl, int64 now) {
    Ꮡl.of(GCCPULimiter.Ꮡlimiter).finishGCTransition(now);
}

public static void Update(this ж<GCCPULimiter> Ꮡl, int64 now) {
    Ꮡl.of(GCCPULimiter.Ꮡlimiter).update(now);
}

public static void AddAssistTime(this ж<GCCPULimiter> Ꮡl, int64 t) {
    Ꮡl.of(GCCPULimiter.Ꮡlimiter).addAssistTime(t);
}

public static void ResetCapacity(this ж<GCCPULimiter> Ꮡl, int64 now, int32 nprocs) {
    Ꮡl.of(GCCPULimiter.Ꮡlimiter).resetCapacity(now, nprocs);
}

public static UntypedInt ScavengePercent => /* scavengePercent */ 1;

[GoType] public partial struct Scavenger {
    public Func<int64, int64> Sleep;
    public Func<uintptr, (uintptr, int64)> Scavenge;
    public Func<bool> ShouldStop;
    public Func<int32> GoMaxProcs;
    internal atomic.Uintptr released;
    internal global::go.runtime_package.scavengerState scavenger;
    internal channel/*<-*/<EmptyStruct> stop = channel/*<-*/<EmptyStruct>.SendOnly;
    internal /*<-*/channel<EmptyStruct> done = /*<-*/channel<EmptyStruct>.RecvOnly;
}

public static void Start(this ж<Scavenger> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (s.Sleep == default! || s.Scavenge == default! || s.ShouldStop == default! || s.GoMaxProcs == default!) {
        throw panic("must populate all stubs");
    }
    // Install hooks.
    s.scavenger.sleepStub = s.Sleep;
    s.scavenger.scavenge = s.Scavenge;
    s.scavenger.shouldStop = s.ShouldStop;
    s.scavenger.gomaxprocs = s.GoMaxProcs;
    // Start up scavenger goroutine, and wait for it to be ready.
    var stop = new channel<EmptyStruct>(0);
    s.stop = stop.WithDirection(GoChanDir.Send);
    var done = new channel<EmptyStruct>(0);
    s.done = done.WithDirection(GoChanDir.Recv);
    var doneʗ1 = done;
    var stopʗ1 = stop;
    goǃ(() => {
        // This should match bgscavenge, loosely.
        Ꮡs.of(Scavenger.Ꮡscavenger).init();
        Ꮡs.of(Scavenger.Ꮡscavenger).park();
        while (ᐧ) {
            var selᴛ1 = stopʗ1;
            switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                builtin.close(doneʗ1);
                return;
            }
            default: {
                break;
            }}
            var (released, workTime) = Ꮡs.of(Scavenger.Ꮡscavenger).run();
            if (released == 0) {
                Ꮡs.of(Scavenger.Ꮡscavenger).park();
                continue;
            }
            Ꮡs.of(Scavenger.Ꮡreleased).Add(released);
            Ꮡs.of(Scavenger.Ꮡscavenger).sleep(workTime);
        }
    });
    if (!Ꮡs.BlockUntilParked(1000000000)) {
        /* 1 second */
        throw panic("timed out waiting for scavenger to get ready");
    }
}

// BlockUntilParked blocks until the scavenger parks, or until
// timeout is exceeded. Returns true if the scavenger parked.
//
// Note that in testing, parked means something slightly different.
// In anger, the scavenger parks to sleep, too, but in testing,
// it only parks when it actually has no work to do.
public static bool BlockUntilParked(this ж<Scavenger> Ꮡs, int64 timeout) {
    ref var s = ref Ꮡs.DerefOrNull();

    // Just spin, waiting for it to park.
    //
    // The actual parking process is racy with respect to
    // wakeups, which is fine, but for testing we need something
    // a bit more robust.
    var start = nanotime();
    while (nanotime() - start < timeout) {
        @lock(Ꮡs.of(Scavenger.Ꮡscavenger).of(global::go.runtime_package.scavengerState.Ꮡlock));
        var parked = s.scavenger.parked;
        unlock(Ꮡs.of(Scavenger.Ꮡscavenger).of(global::go.runtime_package.scavengerState.Ꮡlock));
        if (parked) {
            return true;
        }
        Gosched();
    }
    return false;
}

// Released returns how many bytes the scavenger released.
public static uintptr Released(this ж<Scavenger> Ꮡs) {
    return Ꮡs.of(Scavenger.Ꮡreleased).Load();
}

// Wake wakes up a parked scavenger to keep running.
public static void Wake(this ж<Scavenger> Ꮡs) {
    Ꮡs.of(Scavenger.Ꮡscavenger).wake();
}

// Stop cleans up the scavenger's resources. The scavenger
// must be parked for this to work.
public static void Stop(this ж<Scavenger> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    @lock(Ꮡs.of(Scavenger.Ꮡscavenger).of(global::go.runtime_package.scavengerState.Ꮡlock));
    var parked = s.scavenger.parked;
    unlock(Ꮡs.of(Scavenger.Ꮡscavenger).of(global::go.runtime_package.scavengerState.Ꮡlock));
    if (!parked) {
        throw panic("tried to clean up scavenger that is not parked");
    }
    builtin.close(s.stop);
    Ꮡs.Wake();
    ᐸꟷ(s.done);
}

[GoType] public partial struct ScavengeIndex {
    internal global::go.runtime_package.scavengeIndex i;
}

public static ж<ScavengeIndex> NewScavengeIndex(ChunkIdx min, ChunkIdx max) {
    var s = @new<ScavengeIndex>();
    // This is a bit lazy but we easily guarantee we'll be able
    // to reference all the relevant chunks. The worst-case
    // memory usage here is 512 MiB, but tests generally use
    // small offsets from BaseChunkIdx, which results in ~100s
    // of KiB in memory use.
    //
    // This may still be worth making better, at least by sharing
    // this fairly large array across calls with a sync.Pool or
    // something. Currently, when the tests are run serially,
    // it takes around 0.5s. Not all that much, but if we have
    // a lot of tests like this it could add up.
    s.Value.i.chunks = new slice<global::go.runtime_package.atomicScavChunkData>((nint)(nuint)(max));
    s.of(ScavengeIndex.Ꮡi).of(global::go.runtime_package.scavengeIndex.Ꮡmin).Store((uintptr)(nuint)min);
    s.of(ScavengeIndex.Ꮡi).of(global::go.runtime_package.scavengeIndex.Ꮡmax).Store((uintptr)(nuint)max);
    s.of(ScavengeIndex.Ꮡi).of(global::go.runtime_package.scavengeIndex.ᏑminHeapIdx).Store((uintptr)(nuint)min);
    s.Value.i.test = true;
    return s;
}

public static (ChunkIdx, nuint) Find(this ж<ScavengeIndex> Ꮡs, bool force) {
    var (ci, off) = Ꮡs.of(ScavengeIndex.Ꮡi).find(force);
    return (((ChunkIdx)(nuint)ci), off);
}

[GoRecv] public static void AllocRange(this ref ScavengeIndex s, uintptr @base, uintptr limit) {
    global::go.runtime_package.chunkIdx sc = chunkIndex(@base);
    global::go.runtime_package.chunkIdx ec = chunkIndex(limit - 1);
    nuint si = chunkPageIndex(@base);
    nuint ei = chunkPageIndex(limit - 1);
    if (sc == ec){
        // The range doesn't cross any chunk boundaries.
        s.i.alloc(sc, ei + 1 - si);
    } else {
        // The range crosses at least one chunk boundary.
        s.i.alloc(sc, (nuint)pallocChunkPages - si);
        for (global::go.runtime_package.chunkIdx c = sc + 1; c < ec; c++) {
            s.i.alloc(c, pallocChunkPages);
        }
        s.i.alloc(ec, ei + 1);
    }
}

public static void FreeRange(this ж<ScavengeIndex> Ꮡs, uintptr @base, uintptr limit) {
    global::go.runtime_package.chunkIdx sc = chunkIndex(@base);
    global::go.runtime_package.chunkIdx ec = chunkIndex(limit - 1);
    nuint si = chunkPageIndex(@base);
    nuint ei = chunkPageIndex(limit - 1);
    if (sc == ec){
        // The range doesn't cross any chunk boundaries.
        Ꮡs.of(ScavengeIndex.Ꮡi).free(sc, si, ei + 1 - si);
    } else {
        // The range crosses at least one chunk boundary.
        Ꮡs.of(ScavengeIndex.Ꮡi).free(sc, si, (nuint)pallocChunkPages - si);
        for (global::go.runtime_package.chunkIdx c = sc + 1; c < ec; c++) {
            Ꮡs.of(ScavengeIndex.Ꮡi).free(c, 0, pallocChunkPages);
        }
        Ꮡs.of(ScavengeIndex.Ꮡi).free(ec, 0, ei + 1);
    }
}

public static void ResetSearchAddrs(this ж<ScavengeIndex> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    foreach (var (_, a) in new ж<global::go.runtime_package.atomicOffAddr>[]{Ꮡs.of(ScavengeIndex.Ꮡi).of(global::go.runtime_package.scavengeIndex.ᏑsearchAddrBg), Ꮡs.of(ScavengeIndex.Ꮡi).of(global::go.runtime_package.scavengeIndex.ᏑsearchAddrForce)}.slice()) {
        var (addr, marked) = a.Load();
        if (marked) {
            a.StoreUnmark(addr, addr);
        }
        a.Clear();
    }
    s.i.freeHWM = minOffAddr;
}

public static void NextGen(this ж<ScavengeIndex> Ꮡs) {
    Ꮡs.of(ScavengeIndex.Ꮡi).nextGen();
}

[GoRecv] public static void SetEmpty(this ref ScavengeIndex s, ChunkIdx ci) {
    s.i.setEmpty(((global::go.runtime_package.chunkIdx)(nuint)ci));
}

public static bool CheckPackScavChunkData(uint32 gen, uint16 inUse, uint16 lastInUse, uint8 flags) {
    var sc0 = new scavChunkData(
        gen: gen,
        inUse: inUse,
        lastInUse: lastInUse,
        scavChunkFlags: ((global::go.runtime_package.scavChunkFlags)flags)
    );
    var scp = sc0.pack();
    var sc1 = unpackScavChunkData(scp);
    return sc0 == sc1;
}

public static UntypedInt GTrackingPeriod => /* gTrackingPeriod */ 8;

public static @unsafe.Pointer ZeroBase;
internal static void initᴛZeroBase() { ZeroBase = @unsafe.Pointer.FromBox(Ꮡzerobase); }

public static uintptr UserArenaChunkBytes => /* userArenaChunkBytes */ 4194304;

[GoType] public partial struct UserArena {
    internal ж<global::go.runtime_package.userArena> arena;
}

public static ж<UserArena> NewUserArena() {
    return Ꮡ(new UserArena(newUserArena()));
}

// go2cs generated this placeholder — func New is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

[GoRecv] public static void Slice(this ref UserArena a, any sl, nint cap) {
    a.arena.Δslice(sl, cap);
}

[GoRecv] public static void Free(this ref UserArena a) {
    a.arena.free();
}

public static nint GlobalWaitingArenaChunks() {
    nint n = 0;
    systemstack(() => {
        @lock(Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡlock));
        for (var s = mheap_.userArena.quarantineList.first; s != nil; s = s.Value.next) {
            n++;
        }
        unlock(Ꮡmheap_.of(global::go.runtime_package.mheap.Ꮡlock));
    });
    return n;
}

public static T UserArenaClone<T>(T s) {
    return arena_heapify(s)._<T>();
}

public static Func<uintptr, uintptr, uintptr> AlignUp = alignUp;

public static bool BlockUntilEmptyFinalizerQueue(int64 timeout) {
    return blockUntilEmptyFinalizerQueue(timeout);
}

public static nint FrameStartLine(ж<global::go.runtime_package.Frame> Ꮡf) {
    ref var f = ref Ꮡf.DerefOrNull();

    return f.startLine;
}

// PersistentAlloc allocates some memory that lives outside the Go heap.
// This memory will never be freed; use sparingly.
public static @unsafe.Pointer PersistentAlloc(uintptr n) {
    return (uintptr)persistentalloc(n, 0, Ꮡmemstats.of(global::go.runtime_package.mstats.Ꮡother_sys));
}

// FPCallers works like Callers and uses frame pointer unwinding to populate
// pcBuf with the return addresses of the physical frames on the stack.
public static nint FPCallers(slice<uintptr> pcBuf) {
    return fpTracebackPCs((@unsafe.Pointer)getfp(), pcBuf);
}

public const bool FramePointerEnabled = /* framepointer_enabled */ true;

public static Func<@unsafe.Pointer, bool> IsPinned;
internal static void initᴛIsPinned() { IsPinned = isPinned; }
public static Func<@unsafe.Pointer, ж<uintptr>> GetPinCounter;
internal static void initᴛGetPinCounter() { GetPinCounter = pinnerGetPinCounter; }

public static void SetPinnerLeakPanic(Action f) {
    pinnerLeakPanic = f;
}

public static Action GetPinnerLeakPanic() {
    return pinnerLeakPanic;
}

internal static uintptr testUintptr;

public static void MyGenericFunc<T>() {
    systemstack(() => {
        testUintptr = 4;
    });
}

public static bool UnsafePoint(uintptr pc) {
    var fi = findfunc(pc);
    var v = pcdatavalue(fi, abi.PCDATA_UnsafePoint, pc);
    var exprᴛ1 = v;
    if (exprᴛ1 == abi.UnsafePointUnsafe) {
        return true;
    }
    if (exprᴛ1 == abi.UnsafePointSafe) {
        return false;
    }
    if (exprᴛ1 == abi.UnsafePointRestart1 || exprᴛ1 == abi.UnsafePointRestart2 || exprᴛ1 == abi.UnsafePointRestartAtEntry) {
        return false;
    }
    { /* default: */
// These are all interruptible, they just encode a nonstandard
// way of recovering when interrupted.
        array<byte> buf = new(20);
        throw panic("invalid unsafe point code " + ((sstring)itoa(buf[..], (uint64)v)));
    }

}

[GoType] public partial struct TraceMap {
    internal partial ref global::go.runtime_package.traceMap traceMap { get; }
}

public static (uint64, bool) PutString(this ж<TraceMap> Ꮡm, @string s) {
    return Ꮡm.of(TraceMap.ᏑtraceMap).put(@unsafe.Pointer.FromPinnedBox(@unsafe.StringData(s)), (uintptr)len(s));
}

public static void Reset(this ж<TraceMap> Ꮡm) {
    Ꮡm.of(TraceMap.ᏑtraceMap).reset();
}

public static void SetSpinInGCMarkDone(bool spin) {
    ᏑgcDebugMarkDone.of(gcDebugMarkDoneᴛ1.ᏑspinAfterRaggedBarrier).Store(spin);
}

public static bool GCMarkDoneRestarted() {
    // Only read this outside of the GC. If we're running during a GC, just report false.
    var mp = acquirem();
    if (gcphase != _GCoff) {
        releasem(ref (mp).DerefOrNull());
        return false;
    }
    var restarted = gcDebugMarkDone.restartedDueTo27993;
    releasem(ref (mp).DerefOrNull());
    return restarted;
}

public static void GCMarkDoneResetRestartFlag() {
    var mp = acquirem();
    while (gcphase != _GCoff) {
        releasem(ref (mp).DerefOrNull());
        Gosched();
        mp = acquirem();
    }
    gcDebugMarkDone.restartedDueTo27993 = false;
    releasem(ref (mp).DerefOrNull());
}

[GoType] public partial struct BitCursor {
    internal global::go.runtime_package.bitCursor b;
}

public static BitCursor NewBitCursor(ж<byte> Ꮡbuf) {
    return new BitCursor(b: new bitCursor(ptr: Ꮡbuf, n: 0));
}

public static void Write(this BitCursor b, ж<byte> Ꮡdata, uintptr cnt) {
    b.b.write(Ꮡdata, cnt);
}

public static BitCursor Offset(this BitCursor b, uintptr cnt) {
    return new BitCursor(b: b.b.offset(cnt));
}

} // end runtime_internal_test_package
