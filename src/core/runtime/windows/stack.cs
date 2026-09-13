// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using cpu = @internal.cpu_package;
using goarch = @internal.goarch_package;
using goos = @internal.goos_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

/*
Stack layout parameters.
Included both by runtime (compiled via 6c) and linkers (compiled via gcc).

The per-goroutine g->stackguard is set to point StackGuard bytes
above the bottom of the stack.  Each function compares its stack
pointer against g->stackguard to check for overflow.  To cut one
instruction from the check sequence for functions with tiny frames,
the stack is allowed to protrude StackSmall bytes below the stack
guard.  Functions with large frames don't bother with the check and
always call morestack.  The sequences are (for amd64, others are
similar):

	guard = g->stackguard
	frame = function's stack frame size
	argsize = size of function arguments (call + return)

	stack frame size <= StackSmall:
		CMPQ guard, SP
		JHI 3(PC)
		MOVQ m->morearg, $(argsize << 32)
		CALL morestack(SB)

	stack frame size > StackSmall but < StackBig
		LEAQ (frame-StackSmall)(SP), R0
		CMPQ guard, R0
		JHI 3(PC)
		MOVQ m->morearg, $(argsize << 32)
		CALL morestack(SB)

	stack frame size >= StackBig:
		MOVQ m->morearg, $((argsize << 32) | frame)
		CALL morestack(SB)

The bottom StackGuard - StackSmall bytes are important: there has
to be enough room to execute functions that refuse to check for
stack overflow, either because they need to be adjacent to the
actual caller's frame (deferproc) or because they handle the imminent
stack overflow (morestack).

For example, deferproc might call malloc, which does one of the
above checks (without allocating a full frame), which might trigger
a call to morestack.  This sequence needs to fit in the bottom
section of the stack.  On amd64, morestack's frame is 40 bytes, and
deferproc's frame is 56 bytes.  That fits well within the
StackGuard - StackSmall bytes at the bottom.
The linkers explore all possible call traces involving non-splitting
functions to make sure that this limit cannot be violated.
*/
internal static UntypedInt stackSystem => /* goos.IsWindows*4096 + goos.IsPlan9*512 + goos.IsIos*goarch.IsArm64*1024 */ 4096;
internal static UntypedInt stackMin => 2048;
internal static UntypedInt fixedStack0 => /* stackMin + stackSystem */ 6144;
internal static UntypedInt fixedStack1 => /* fixedStack0 - 1 */ 6143;
internal static UntypedInt fixedStack2 => /* fixedStack1 | (fixedStack1 >> 1) */ 8191;
internal static UntypedInt fixedStack3 => /* fixedStack2 | (fixedStack2 >> 2) */ 8191;
internal static UntypedInt fixedStack4 => /* fixedStack3 | (fixedStack3 >> 4) */ 8191;
internal static UntypedInt fixedStack5 => /* fixedStack4 | (fixedStack4 >> 8) */ 8191;
internal static UntypedInt fixedStack6 => /* fixedStack5 | (fixedStack5 >> 16) */ 8191;
internal static UntypedInt fixedStack => /* fixedStack6 + 1 */ 8192;
internal static UntypedInt stackNosplit => /* abi.StackNosplitBase * sys.StackGuardMultiplier */ 800;
internal static UntypedInt stackGuard => /* stackNosplit + stackSystem + abi.StackSmall */ 5024;

internal static UntypedInt stackDebug => 0;
internal static UntypedInt stackFromSystem => 0; // allocate stacks from system memory instead of the heap
internal static UntypedInt stackFaultOnFree => 0; // old stacks are mapped noaccess to detect use after free
internal static UntypedInt stackNoCache => 0; // disable per-P small stack caches
internal const bool debugCheckBP = false;

internal static nint stackPoisonCopy = 0; // fill stack that should not be accessed with garbage, to detect bad dereferences during copy

internal static UntypedInt uintptrMask => /* 1<<(8*goarch.PtrSize) - 1 */ 18446744073709551615;
// The values below can be stored to g.stackguard0 to force
// the next stack check to fail.
// These are all larger than any real SP.
internal static UntypedInt stackPreempt => /* uintptrMask & -1314 */ 18446744073709550302;
internal static UntypedInt stackFork => /* uintptrMask & -1234 */ 18446744073709550382;
internal static UntypedInt stackForceMove => /* uintptrMask & -275 */ 18446744073709551341;
internal static UntypedInt stackPoisonMin => /* uintptrMask & -4096 */ 18446744073709547520;

// Global pool of spans that have free stacks.
// Stacks are assigned an order according to size.
//
//	order = log_2(size/FixedStack)
//
// There is a free list for each order.

[GoType("dyn")] partial struct stackpoolᴛ1 {
    internal stackpoolItem item;
    internal array<byte> _ = new((uintptr)((uintptr)cpu.CacheLinePadSize - /* unsafe.Sizeof(stackpoolItem{}) */ (uintptr)24 % (uintptr)cpu.CacheLinePadSize) % cpu.CacheLinePadSize);
}
internal static ж<array<stackpoolᴛ1>> Ꮡstackpool = new StandardBox<array<stackpoolᴛ1>>(new array<stackpoolᴛ1>(2, () => new()));
internal static ref array<stackpoolᴛ1> stackpool => ref Ꮡstackpool.Value;

[GoType] partial struct stackpoolItem {
    internal sys.NotInHeap _;
    internal mutex mu;
    internal mSpanList span;
}

// Global pool of large stack spans.

[GoType("dyn")] partial struct stackLargeᴛ1 {
    internal mutex @lock;
    internal array<mSpanList> free = new(heapAddrBits - pageShift); // free lists by log_2(s.npages)
}
internal static ж<stackLargeᴛ1> ᏑstackLarge = new StandardBox<stackLargeᴛ1>(new stackLargeᴛ1());
internal static ref stackLargeᴛ1 stackLarge => ref ᏑstackLarge.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cacheSizeMustBeAMultipleˢ = "cache size must be a multiple of page size"u8;

internal static void stackinit() {
    if ((UntypedInt)(_StackCacheSize & _PageMask) != 0) {
        @throw(cacheSizeMustBeAMultipleˢ);
    }
    foreach (var (i, _) in stackpool) {
        stackpool[i].item.span.init();
        lockInit(ref stackpool[i].item.mu, lockRankStackpool);
    }
    foreach (var (i, _) in stackLarge.free) {
        stackLarge.free[i].init();
        lockInit(ref stackLarge.@lock, lockRankStackLarge);
    }
}

// stacklog2 returns ⌊log_2(n)⌋.
internal static nint stacklog2(uintptr n) {
    nint log2 = 0;
    while (n > 1) {
        n >>= (int)(1);
        log2++;
    }
    return log2;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badAllocCountˢ = "bad allocCount"u8;
internal static readonly @string badManualFreeListˢ = "bad manualFreeList"u8;
internal static readonly @string spanHasNoFreeStacksˢ = "span has no free stacks"u8;

// Allocates a stack from the free pool. Must be called with
// stackpool[order].item.mu held.
internal static gclinkptr stackpoolalloc(uint8 order) {
    var list = Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡspan);
    var s = list.Value.first;
    lockWithRankMayAcquire(Ꮡmheap_.of(mheap.Ꮡlock), lockRankMheap);
    if (s == nil) {
        // no free stacks. Allocate another span worth.
        s = Ꮡmheap_.allocManual(((uintptr)_StackCacheSize >> (int)(_PageShift)), spanAllocStack);
        if (s == nil) {
            @throw(outOfMemoryˢ);
        }
        if ((~s).allocCount != 0) {
            @throw(badAllocCountˢ);
        }
        if ((~s).manualFreeList.ptr() != nil) {
            @throw(badManualFreeListˢ);
        }
        osStackAlloc(ref (s).DerefOrNull());
        s.Value.elemsize = ((uintptr)fixedStack).Lsh((uint64)(order));
        for (var i = (uintptr)0; i < _StackCacheSize; i += s.Value.elemsize) {
            var xΔ1 = ((gclinkptr)(s.@base() + i));
            xΔ1.ptr().Value.next = s.Value.manualFreeList;
            s.Value.manualFreeList = xΔ1;
        }
        list.insert(s);
    }
    var x = s.Value.manualFreeList;
    if (x.ptr() == nil) {
        @throw(spanHasNoFreeStacksˢ);
    }
    s.Value.manualFreeList = x.ptr().Value.next;
    s.Value.allocCount++;
    if ((~s).manualFreeList.ptr() == nil) {
        // all stacks in s are allocated.
        list.remove(s);
    }
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string freeingStackNotInAStackˢ = "freeing stack not in a stack span"u8;

// Adds stack x to the free pool. Must be called with stackpool[order].item.mu held.
internal static void stackpoolfree(gclinkptr x, uint8 order) {
    var s = spanOfUnchecked((uintptr)x);
    if (s.of(mspan.Ꮡstate).get() != mSpanManual) {
        @throw(freeingStackNotInAStackˢ);
    }
    if ((~s).manualFreeList.ptr() == nil) {
        // s will now have a free stack
        Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡspan).insert(s);
    }
    x.ptr().Value.next = s.Value.manualFreeList;
    s.Value.manualFreeList = x;
    s.Value.allocCount--;
    if (gcphase == _GCoff && (~s).allocCount == 0) {
        // Span is completely free. Return it to the heap
        // immediately if we're sweeping.
        //
        // If GC is active, we delay the free until the end of
        // GC to avoid the following type of situation:
        //
        // 1) GC starts, scans a SudoG but does not yet mark the SudoG.elem pointer
        // 2) The stack that pointer points to is copied
        // 3) The old stack is freed
        // 4) The containing span is marked free
        // 5) GC attempts to mark the SudoG.elem pointer. The
        //    marking fails because the pointer looks like a
        //    pointer into a free span.
        //
        // By not freeing, we prevent step #4 until GC is done.
        Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡspan).remove(s);
        s.Value.manualFreeList = 0;
        osStackFree(ref (s).DerefOrNull());
        Ꮡmheap_.freeManual(s, spanAllocStack);
    }
}

// stackcacherefill/stackcacherelease implement a global pool of stack segments.
// The pool is required to prevent unlimited growth of per-thread caches.
//
//go:systemstack
internal static void stackcacherefill(ref mcache c, uint8 order) {
    if (stackDebug >= 1) {
        print((@string)"stackcacherefill order="u8, order, (@string)"\n"u8);
    }
    // Grab some stacks from the global cache.
    // Grab half of the allowed capacity (to prevent thrashing).
    gclinkptr list = default!;
    uintptr size = default!;
    @lock(Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
    while (size < (uintptr)(_StackCacheSize / 2)) {
        var x = stackpoolalloc(order);
        x.ptr().Value.next = list;
        list = x;
        size += ((uintptr)fixedStack).Lsh((uint64)(order));
    }
    unlock(Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
    c.stackcache[order].list = list;
    c.stackcache[order].size = size;
}

//go:systemstack
internal static void stackcacherelease(ref mcache c, uint8 order) {
    if (stackDebug >= 1) {
        print((@string)"stackcacherelease order="u8, order, (@string)"\n"u8);
    }
    var x = c.stackcache[order].list;
    var size = c.stackcache[order].size;
    @lock(Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
    while (size > (uintptr)(_StackCacheSize / 2)) {
        var y = x.ptr().Value.next;
        stackpoolfree(x, order);
        x = y;
        size -= ((uintptr)fixedStack).Lsh((uint64)(order));
    }
    unlock(Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
    c.stackcache[order].list = x;
    c.stackcache[order].size = size;
}

//go:systemstack
internal static void stackcache_clear(ref mcache c) {
    if (stackDebug >= 1) {
        print((@string)"stackcache clear\n"u8);
    }
    for (var order = (uint8)0; order < _NumStackOrders; order++) {
        @lock(Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
        var x = c.stackcache[order].list;
        while (x.ptr() != nil) {
            var y = x.ptr().Value.next;
            stackpoolfree(x, order);
            x = y;
        }
        c.stackcache[order].list = 0;
        c.stackcache[order].size = 0;
        unlock(Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stackallocNotOnSchedulerˢ = "stackalloc not on scheduler stack"u8;
internal static readonly @string stackSizeNotAPowerOf2ˢ = "stack size not a power of 2"u8;
internal static readonly @string outOfMemoryStackallocˢ = "out of memory (stackalloc)"u8;

// stackalloc allocates an n byte stack.
//
// stackalloc must run on the system stack because it uses per-P
// resources and must not split the stack.
//
//go:systemstack
internal static Δstack @stackalloc(uint32 n) {
    // Stackalloc must be called on scheduler stack, so that we
    // never try to grow the stack during the code that stackalloc runs.
    // Doing so would cause a deadlock (issue 1547).
    var thisg = getg();
    if (thisg != (~(~thisg).m).g0) {
        @throw(stackallocNotOnSchedulerˢ);
    }
    if ((uint32)(n & (n - 1)) != 0) {
        @throw(stackSizeNotAPowerOf2ˢ);
    }
    if (stackDebug >= 1) {
        print((@string)"stackalloc "u8, n, (@string)"\n"u8);
    }
    if (debug.efence != 0 || stackFromSystem != 0) {
        n = (uint32)alignUp((uintptr)n, physPageSize);
        @unsafe.Pointer vΔ1 = (uintptr)sysAlloc((uintptr)n, Ꮡmemstats.of(mstats.Ꮡstacks_sys));
        if (vΔ1 == nil) {
            @throw(outOfMemoryStackallocˢ);
        }
        return new Δstack((uintptr)vΔ1, (uintptr)vΔ1 + (uintptr)n);
    }
    // Small stacks are allocated with a fixed-size free-list allocator.
    // If we need a stack of a bigger size, we fall back on allocating
    // a dedicated span.
    @unsafe.Pointer v = default!;
    if (n < (uint32)(((uint32)fixedStack << (int)(_NumStackOrders))) && n < _StackCacheSize){
        var order = (uint8)0;
        var n2 = n;
        while (n2 > fixedStack) {
            order++;
            n2 >>= (int)(1);
        }
        gclinkptr x = default!;
        if (stackNoCache != 0 || (~(~thisg).m).p == 0 || (~(~thisg).m).preemptoff != ""u8){
            // thisg.m.p == 0 can happen in the guts of exitsyscall
            // or procresize. Just get a stack from the global pool.
            // Also don't touch stackcache during gc
            // as it's flushed concurrently.
            @lock(Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
            x = stackpoolalloc(order);
            unlock(Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
        } else {
            var c = (~(~thisg).m).p.ptr().Value.mcache;
            x = (~c).stackcache[order].list;
            if (x.ptr() == nil) {
                stackcacherefill(ref (c).DerefOrNull(), order);
                x = (~c).stackcache[order].list;
            }
            (~c).stackcache[order].list = x.ptr().Value.next;
            (~c).stackcache[order].size -= (uintptr)n;
        }
        v = ((@unsafe.Pointer)(uintptr)x);
    } else {
        ж<mspan> s = default!;
        var npage = ((uintptr)n >> (int)(_PageShift));
        nint log2npage = stacklog2(npage);
        // Try to get a stack from the large stack cache.
        @lock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
        if (!stackLarge.free[log2npage].isEmpty()) {
            s = stackLarge.free[log2npage].first;
            ᏑstackLarge.at(stackLargeᴛ1.Ꮡfree, log2npage).remove(s);
        }
        unlock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
        lockWithRankMayAcquire(Ꮡmheap_.of(mheap.Ꮡlock), lockRankMheap);
        if (s == nil) {
            // Allocate a new stack from the heap.
            s = Ꮡmheap_.allocManual(npage, spanAllocStack);
            if (s == nil) {
                @throw(outOfMemoryˢ);
            }
            osStackAlloc(ref (s).DerefOrNull());
            s.Value.elemsize = (uintptr)n;
        }
        v = (@unsafe.Pointer)s.@base();
    }
    if (traceAllocFreeEnabled()) {
        var Δtrace = traceTryAcquire();
        if (Δtrace.ok()) {
            Δtrace.GoroutineStackAlloc((uintptr)v, (uintptr)n);
            traceRelease(Δtrace);
        }
    }
    if (raceenabled) {
        racemalloc(v, (uintptr)n);
    }
    if (msanenabled) {
        msanmalloc(v, (uintptr)n);
    }
    if (asanenabled) {
        asanunpoison(v, (uintptr)n);
    }
    if (stackDebug >= 1) {
        print((@string)"  allocated "u8, v, (@string)"\n"u8);
    }
    return new Δstack((uintptr)v, (uintptr)v + (uintptr)n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stackNotAPowerOf2ˢ = "stack not a power of 2"u8;
internal static readonly @string badStackSizeˢ = "bad stack size"u8;
internal static readonly @string badSpanStateˢ = "bad span state"u8;

// stackfree frees an n byte stack allocation at stk.
//
// stackfree must run on the system stack because it uses per-P
// resources and must not split the stack.
//
//go:systemstack
internal static void stackfree(Δstack stk) {
    var gp = getg();
    @unsafe.Pointer v = (@unsafe.Pointer)stk.lo;
    var n = stk.hi - stk.lo;
    if ((uintptr)(n & (n - 1)) != 0) {
        @throw(stackNotAPowerOf2ˢ);
    }
    if (stk.lo + n < stk.hi) {
        @throw(badStackSizeˢ);
    }
    if (stackDebug >= 1) {
        println((@string)"stackfree"u8, v, n);
        memclrNoHeapPointers(v, n); // for testing, clobber stack data
    }
    if (debug.efence != 0 || stackFromSystem != 0) {
        if (debug.efence != 0 || stackFaultOnFree != 0){
            sysFault(v, n);
        } else {
            sysFree(v, n, Ꮡmemstats.of(mstats.Ꮡstacks_sys));
        }
        return;
    }
    if (traceAllocFreeEnabled()) {
        var Δtrace = traceTryAcquire();
        if (Δtrace.ok()) {
            Δtrace.GoroutineStackFree((uintptr)v);
            traceRelease(Δtrace);
        }
    }
    if (msanenabled) {
        msanfree(v, n);
    }
    if (asanenabled) {
        asanpoison(v, n);
    }
    if (n < (uintptr)(((uintptr)fixedStack << (int)(_NumStackOrders))) && n < _StackCacheSize){
        var order = (uint8)0;
        var n2 = n;
        while (n2 > fixedStack) {
            order++;
            n2 >>= (int)(1);
        }
        var x = ((gclinkptr)(uintptr)v);
        if (stackNoCache != 0 || (~(~gp).m).p == 0 || (~(~gp).m).preemptoff != ""u8){
            @lock(Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
            stackpoolfree(x, order);
            unlock(Ꮡstackpool.at<stackpoolᴛ1>((nint)(order)).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
        } else {
            var c = (~(~gp).m).p.ptr().Value.mcache;
            if ((~c).stackcache[order].size >= _StackCacheSize) {
                stackcacherelease(ref (c).DerefOrNull(), order);
            }
            x.ptr().Value.next = (~c).stackcache[order].list;
            (~c).stackcache[order].list = x;
            (~c).stackcache[order].size += n;
        }
    } else {
        var s = spanOfUnchecked((uintptr)v);
        if (s.of(mspan.Ꮡstate).get() != mSpanManual) {
            println(((Δhex)(uint64)s.@base()), v);
            @throw(badSpanStateˢ);
        }
        if (gcphase == _GCoff){
            // Free the stack immediately if we're
            // sweeping.
            osStackFree(ref (s).DerefOrNull());
            Ꮡmheap_.freeManual(s, spanAllocStack);
        } else {
            // If the GC is running, we can't return a
            // stack span to the heap because it could be
            // reused as a heap span, and this state
            // change would race with GC. Add it to the
            // large stack cache instead.
            nint log2npage = stacklog2((~s).npages);
            @lock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
            ᏑstackLarge.at(stackLargeᴛ1.Ꮡfree, log2npage).insert(s);
            unlock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
        }
    }
}

internal static uintptr maxstacksize = ((uintptr)1 << (int)(20)); // enough until runtime.main sets it for real

internal static uintptr maxstackceiling = maxstacksize;

internal static slice<@string> ptrnames = new slice<@string>(2){
    [0] = "scalar"u8,
    [1] = "ptr"u8
};

// Stack frame layout
//
// (x86)
// +------------------+
// | args from caller |
// +------------------+ <- frame->argp
// |  return address  |
// +------------------+
// |  caller's BP (*) | (*) if framepointer_enabled && varp > sp
// +------------------+ <- frame->varp
// |     locals       |
// +------------------+
// |  args to callee  |
// +------------------+ <- frame->sp
//
// (arm)
// +------------------+
// | args from caller |
// +------------------+ <- frame->argp
// | caller's retaddr |
// +------------------+
// |  caller's FP (*) | (*) on ARM64, if framepointer_enabled && varp > sp
// +------------------+ <- frame->varp
// |     locals       |
// +------------------+
// |  args to callee  |
// +------------------+
// |  return address  |
// +------------------+ <- frame->sp
//
// varp > sp means that the function has a frame;
// varp == sp means frameless function.
[GoType] partial struct adjustinfo {
    internal Δstack old;
    internal uintptr delta; // ptr distance from old to new stack (newbase - oldbase)
    // sghi is the highest sudog.elem on the stack.
    internal uintptr sghi;
}

// adjustpointer checks whether *vpp is in the old stack described by adjinfo.
// If so, it rewrites *vpp to point into the new stack.
internal static void adjustpointer(ref adjustinfo adjinfo, @unsafe.Pointer vpp) {
    var pp = (ж<uintptr>)(uintptr)(vpp);
    var Δp = pp.Value;
    if (stackDebug >= 4) {
        print((@string)"        "u8, pp.OrTypedNil(), (@string)":"u8, ((Δhex)(uint64)Δp), (@string)"\n"u8);
    }
    if (adjinfo.old.lo <= Δp && Δp < adjinfo.old.hi) {
        pp.Value = Δp + adjinfo.delta;
        if (stackDebug >= 3) {
            print((@string)"        adjust ptr "u8, pp.OrTypedNil(), (@string)":"u8, ((Δhex)(uint64)Δp), (@string)" -> "u8, ((Δhex)(uint64)(pp.Value)), (@string)"\n"u8);
        }
    }
}

// Information from the compiler about the layout of stack frames.
// Note: this type must agree with reflect.bitVector.
[GoType] partial struct bitvector {
    internal int32 n; // # of bits
    internal ж<uint8> bytedata;
}

// ptrbit returns the i'th bit in bv.
// ptrbit is less efficient than iterating directly over bitvector bits,
// and should only be used in non-performance-critical code.
// See adjustpointers for an example of a high-efficiency walk of a bitvector.
[GoRecv] internal static uint8 ptrbit(this ref bitvector bv, uintptr i) {
    var b = (addb(bv.bytedata, i / 8)).Value;
    return (uint8)((byte)(((b >> (int)((i % 8)))) & 1));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string invalidPointerFoundOnˢ = "invalid pointer found on stack"u8;

// bv describes the memory starting at address scanp.
// Adjust any pointers contained therein.
internal static void adjustpointers(@unsafe.Pointer scanp, ж<bitvector> Ꮡbv, ref adjustinfo adjinfo, ΔfuncInfo f) {
    ref var bv = ref Ꮡbv.DerefOrNull();

    var minp = adjinfo.old.lo;
    var maxp = adjinfo.old.hi;
    var delta = adjinfo.delta;
    var num = (uintptr)bv.n;
    // If this frame might contain channel receive slots, use CAS
    // to adjust pointers. If the slot hasn't been received into
    // yet, it may contain stack pointers and a concurrent send
    // could race with adjusting those pointers. (The sent value
    // itself can never contain stack pointers.)
    var useCAS = (uintptr)scanp < adjinfo.sghi;
    for (var i = (uintptr)0; i < num; i += 8) {
        if (stackDebug >= 4) {
            for (var j = (uintptr)0; j < 8; j++) {
                print((@string)"        "u8, (uintptr)add(scanp, (i + j) * (uintptr)goarch.PtrSize), (@string)":"u8, ptrnames[bv.ptrbit(i + j)], (@string)":"u8, ((Δhex)(uint64)(~(ж<uintptr>)(uintptr)((uintptr)add(scanp, (i + j) * (uintptr)goarch.PtrSize)))), (@string)" # "u8, i, (@string)" "u8, addb(bv.bytedata, i / 8).Value, (@string)"\n"u8);
            }
        }
        var b = (addb(bv.bytedata, i / 8)).Value;
        while (b != 0) {
            var j = (uintptr)sys.TrailingZeros8(b);
            b &= (byte)(b - 1);
            var pp = (ж<uintptr>)(uintptr)((uintptr)add(scanp, (i + j) * (uintptr)goarch.PtrSize));
retry:
            var Δp = pp.Value;
            if (f.valid() && 0 < Δp && Δp < minLegalPointer && debug.invalidptr != 0) {
                // Looks like a junk value in a pointer slot.
                // Live analysis wrong?
                getg().Value.m.Value.traceback = 2;
                print((@string)"runtime: bad pointer in frame "u8, funcname(f), (@string)" at "u8, pp.OrTypedNil(), (@string)": "u8, ((Δhex)(uint64)Δp), (@string)"\n"u8);
                @throw(invalidPointerFoundOnˢ);
            }
            if (minp <= Δp && Δp < maxp) {
                if (stackDebug >= 3) {
                    print((@string)"adjust ptr "u8, ((Δhex)(uint64)Δp), (@string)" "u8, funcname(f), (@string)"\n"u8);
                }
                if (useCAS){
                    var ppu = Ꮡ(new @unsafe.Pointer(~pp));
                    if (!atomic.Casp1(ppu, (@unsafe.Pointer)Δp, (@unsafe.Pointer)(Δp + delta))) {
                        goto retry;
                    }
                } else {
                    pp.Value = Δp + delta;
                }
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badFramePointerˢ = "bad frame pointer"u8;

// Note: the argument/return area is adjusted by the callee.
internal static void adjustframe(ж<stkframe> Ꮡframe, ref adjustinfo adjinfo) {
    ref var frame = ref Ꮡframe.DerefOrNull();

    if (frame.continpc == 0) {
        // Frame is dead.
        return;
    }
    var f = frame.fn;
    if (stackDebug >= 2) {
        print((@string)"    adjusting "u8, funcname(f), (@string)" frame=["u8, ((Δhex)(uint64)frame.sp), (@string)","u8, ((Δhex)(uint64)frame.fp), (@string)"] pc="u8, ((Δhex)(uint64)frame.pc), (@string)" continpc="u8, ((Δhex)(uint64)frame.continpc), (@string)"\n"u8);
    }
    // Adjust saved frame pointer if there is one.
    if ((goarch.ArchFamily == goarch.AMD64 || goarch.ArchFamily == goarch.ARM64) && frame.argp - frame.varp == (uintptr)(2 * goarch.PtrSize)) {
        if (stackDebug >= 3) {
            print((@string)"      saved bp\n"u8);
        }
        if (debugCheckBP) {
            // Frame pointers should always point to the next higher frame on
            // the Go stack (or be nil, for the top frame on the stack).
            var bp = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)frame.varp);
            if (bp != 0 && (bp < adjinfo.old.lo || bp >= adjinfo.old.hi)) {
                println((@string)"runtime: found invalid frame pointer"u8);
                print((@string)"bp="u8, ((Δhex)(uint64)bp), (@string)" min="u8, ((Δhex)(uint64)adjinfo.old.lo), (@string)" max="u8, ((Δhex)(uint64)adjinfo.old.hi), (@string)"\n"u8);
                @throw(badFramePointerˢ);
            }
        }
        // On AMD64, this is the caller's frame pointer saved in the current
        // frame.
        // On ARM64, this is the frame pointer of the caller's caller saved
        // by the caller in its frame (one word below its SP).
        adjustpointer(ref adjinfo, (@unsafe.Pointer)frame.varp);
    }
    ref var locals = ref heap<bitvector>(out var Ꮡlocals);
    ref var args = ref heap<bitvector>(out var Ꮡargs);
    (locals, args, var objs) = frame.getStackMap(true);
    // Adjust local variables if stack frame has been allocated.
    if (locals.n > 0) {
        var size = (uintptr)locals.n * (uintptr)goarch.PtrSize;
        adjustpointers((@unsafe.Pointer)(frame.varp - size), Ꮡlocals, ref adjinfo, f);
    }
    // Adjust arguments.
    if (args.n > 0) {
        if (stackDebug >= 3) {
            print((@string)"      args\n"u8);
        }
        adjustpointers((@unsafe.Pointer)frame.argp, Ꮡargs, ref adjinfo, new ΔfuncInfo(nil));
    }
    // Adjust pointers in all stack objects (whether they are live or not).
    // See comments in mgcmark.go:scanframeworker.
    if (frame.varp != 0) {
        foreach (var (i, _) in objs) {
            var obj = Ꮡ(objs, i);
            var off = obj.Value.off;
            var @base = frame.varp; // locals base pointer
            if (off >= 0) {
                @base = frame.argp; // arguments and return values base pointer
            }
            var Δp = @base + (uintptr)off;
            if (Δp < frame.sp) {
                // Object hasn't been allocated in the frame yet.
                // (Happens when the stack bounds check fails and
                // we call into morestack.)
                continue;
            }
            var ptrdata = obj.ptrdata();
            var gcdata = obj.gcdata();
            ж<mspan> s = default!;
            if (obj.useGCProg()) {
                // See comments in mgcmark.go:scanstack
                s = materializeGCProg(ptrdata, gcdata);
                gcdata = (ж<byte>)(uintptr)((@unsafe.Pointer)(~s).startAddr);
            }
            for (var iΔ1 = (uintptr)0; iΔ1 < ptrdata; iΔ1 += goarch.PtrSize) {
                if ((byte)((addb(gcdata, iΔ1 / (uintptr)(8 * goarch.PtrSize)).Value >> (int)(((uintptr)(iΔ1 / (uintptr)goarch.PtrSize & 7)))) & 1) != 0) {
                    adjustpointer(ref adjinfo, (@unsafe.Pointer)(Δp + iΔ1));
                }
            }
            if (s != nil) {
                dematerializeGCProg(s);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badTopFramePointerˢ = "bad top frame pointer"u8;

internal static void adjustctxt(ж<g> Ꮡgp, ref adjustinfo adjinfo) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    adjustpointer(ref adjinfo, @unsafe.Pointer.FromBox(Ꮡgp.of(g.Ꮡsched).of(gobuf.Ꮡctxt)));
    if (!framepointer_enabled) {
        return;
    }
    if (debugCheckBP) {
        var bp = gp.sched.bp;
        if (bp != 0 && (bp < adjinfo.old.lo || bp >= adjinfo.old.hi)) {
            println((@string)"runtime: found invalid top frame pointer"u8);
            print((@string)"bp="u8, ((Δhex)(uint64)bp), (@string)" min="u8, ((Δhex)(uint64)adjinfo.old.lo), (@string)" max="u8, ((Δhex)(uint64)adjinfo.old.hi), (@string)"\n"u8);
            @throw(badTopFramePointerˢ);
        }
    }
    var oldfp = gp.sched.bp;
    adjustpointer(ref adjinfo, @unsafe.Pointer.FromBox(Ꮡgp.of(g.Ꮡsched).of(gobuf.Ꮡbp)));
    if (GOARCH == "arm64"u8) {
        // On ARM64, the frame pointer is saved one word *below* the SP,
        // which is not copied or adjusted in any frame. Do it explicitly
        // here.
        if (oldfp == gp.sched.sp - (uintptr)goarch.PtrSize) {
            memmove((@unsafe.Pointer)gp.sched.bp, (@unsafe.Pointer)oldfp, goarch.PtrSize);
            adjustpointer(ref adjinfo, (@unsafe.Pointer)gp.sched.bp);
        }
    }
}

internal static void adjustdefers(ж<g> Ꮡgp, ref adjustinfo adjinfo) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Adjust pointers in the Defer structs.
    // We need to do this first because we need to adjust the
    // defer.link fields so we always work on the new stack.
    adjustpointer(ref adjinfo, @unsafe.Pointer.FromBox(Ꮡgp.of(g.Ꮡ_defer)));
    for (var d = gp._defer; d != nil; d = d.Value.link) {
        adjustpointer(ref adjinfo, new @unsafe.Pointer(d.of(_defer.Ꮡfn)));
        adjustpointer(ref adjinfo, @unsafe.Pointer.FromBox(d.of(_defer.Ꮡsp)));
        adjustpointer(ref adjinfo, @unsafe.Pointer.FromBox(d.of(_defer.Ꮡlink)));
    }
}

internal static void adjustpanics(ж<g> Ꮡgp, ref adjustinfo adjinfo) {
    // Panics are on stack and already adjusted.
    // Update pointer to head of list in G.
    adjustpointer(ref adjinfo, @unsafe.Pointer.FromBox(Ꮡgp.of(g.Ꮡ_panic)));
}

internal static void adjustsudogs(ref g gp, ref adjustinfo adjinfo) {
    // the data elements pointed to by a SudoG structure
    // might be in the stack.
    for (var s = gp.waiting; s != nil; s = s.Value.waitlink) {
        adjustpointer(ref adjinfo, @unsafe.Pointer.FromBox(s.of(sudog.Ꮡelem)));
    }
}

internal static void fillstack(Δstack stk, byte b) {
    for (var Δp = stk.lo; Δp < stk.hi; Δp++) {
        ((ж<byte>)(uintptr)((@unsafe.Pointer)Δp)).Value = b;
    }
}

internal static uintptr findsghi(ref g gp, Δstack stk) {
    uintptr sghi = default!;
    for (var sg = gp.waiting; sg != nil; sg = sg.Value.waitlink) {
        var Δp = (uintptr)(~sg).elem + (uintptr)(~(~sg).c).elemsize;
        if (stk.lo <= Δp && Δp < stk.hi && Δp > sghi) {
            sghi = Δp;
        }
    }
    return sghi;
}

// syncadjustsudogs adjusts gp's sudogs and copies the part of gp's
// stack they refer to while synchronizing with concurrent channel
// operations. It returns the number of bytes of stack copied.
internal static uintptr syncadjustsudogs(ref g gp, uintptr used, ref adjustinfo adjinfo) {
    if (gp.waiting == nil) {
        return 0;
    }
    // Lock channels to prevent concurrent send/receive.
    ж<Δhchan> lastc = default!;
    for (var sg = gp.waiting; sg != nil; sg = sg.Value.waitlink) {
        if ((~sg).c != lastc) {
            // There is a ranking cycle here between gscan bit and
            // hchan locks. Normally, we only allow acquiring hchan
            // locks and then getting a gscan bit. In this case, we
            // already have the gscan bit. We allow acquiring hchan
            // locks here as a special case, since a deadlock can't
            // happen because the G involved must already be
            // suspended. So, we get a special hchan lock rank here
            // that is lower than gscan, but doesn't allow acquiring
            // any other locks other than hchan.
            lockWithRank((~sg).c.of(runtime_package.Δhchan.Ꮡlock), lockRankHchanLeaf);
        }
        lastc = sg.Value.c;
    }
    // Adjust sudogs.
    adjustsudogs(ref gp, ref adjinfo);
    // Copy the part of the stack the sudogs point in to
    // while holding the lock to prevent races on
    // send/receive slots.
    uintptr sgsize = default!;
    if (adjinfo.sghi != 0) {
        var oldBot = adjinfo.old.hi - used;
        var newBot = oldBot + adjinfo.delta;
        sgsize = adjinfo.sghi - oldBot;
        memmove((@unsafe.Pointer)newBot, (@unsafe.Pointer)oldBot, sgsize);
    }
    // Unlock channels.
    lastc = default!;
    for (var sg = gp.waiting; sg != nil; sg = sg.Value.waitlink) {
        if ((~sg).c != lastc) {
            unlock((~sg).c.of(runtime_package.Δhchan.Ꮡlock));
        }
        lastc = sg.Value.c;
    }
    return sgsize;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stackGrowthNotAllowedInˢ = "stack growth not allowed in system call"u8;
internal static readonly @string nilStackbaseˢ = "nil stackbase"u8;
internal static readonly @string racySudogAdjustmentDueToˢ = "racy sudog adjustment due to parking on channel"u8;

// Copies gp's stack to a new stack of a different size.
// Caller must have changed gp status to Gcopystack.
internal static void copystack(ж<g> Ꮡgp, uintptr newsize) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (gp.syscallsp != 0) {
        @throw(stackGrowthNotAllowedInˢ);
    }
    var old = gp.stack;
    if (old.lo == 0) {
        @throw(nilStackbaseˢ);
    }
    var used = old.hi - gp.sched.sp;
    // Add just the difference to gcController.addScannableStack.
    // g0 stacks never move, so this will never account for them.
    // It's also fine if we have no P, addScannableStack can deal with
    // that case.
    ᏑgcController.addScannableStack((~(~getg()).m).p.ptr(), (int64)newsize - (int64)(old.hi - old.lo));
    // allocate new stack
    var @new = @stackalloc((uint32)newsize);
    if (stackPoisonCopy != 0) {
        fillstack(@new, 0xfd);
    }
    if (stackDebug >= 1) {
        print((@string)"copystack gp="u8, Ꮡgp.OrTypedNil(), (@string)" ["u8, ((Δhex)(uint64)old.lo), (@string)" "u8, ((Δhex)(uint64)(old.hi - used)), (@string)" "u8, ((Δhex)(uint64)old.hi), (@string)"]"u8, (@string)" -> ["u8, ((Δhex)(uint64)@new.lo), (@string)" "u8, ((Δhex)(uint64)(@new.hi - used)), (@string)" "u8, ((Δhex)(uint64)@new.hi), (@string)"]/"u8, newsize, (@string)"\n"u8);
    }
    // Compute adjustment.
    adjustinfo adjinfo = default!;
    adjinfo.old = old;
    adjinfo.delta = @new.hi - old.hi;
    // Adjust sudogs, synchronizing with channel ops if necessary.
    var ncopy = used;
    if (!gp.activeStackChans){
        if (newsize < old.hi - old.lo && Ꮡgp.of(g.ᏑparkingOnChan).Load()) {
            // It's not safe for someone to shrink this stack while we're actively
            // parking on a channel, but it is safe to grow since we do that
            // ourselves and explicitly don't want to synchronize with channels
            // since we could self-deadlock.
            @throw(racySudogAdjustmentDueToˢ);
        }
        adjustsudogs(ref (Ꮡgp).DerefOrNull(), ref adjinfo);
    } else {
        // sudogs may be pointing in to the stack and gp has
        // released channel locks, so other goroutines could
        // be writing to gp's stack. Find the highest such
        // pointer so we can handle everything there and below
        // carefully. (This shouldn't be far from the bottom
        // of the stack, so there's little cost in handling
        // everything below it carefully.)
        adjinfo.sghi = findsghi(ref (Ꮡgp).DerefOrNull(), old);
        // Synchronize with channel ops and copy the part of
        // the stack they may interact with.
        ncopy -= syncadjustsudogs(ref (Ꮡgp).DerefOrNull(), used, ref adjinfo);
    }
    // Copy the stack (or the rest of it) to the new location
    memmove((@unsafe.Pointer)(@new.hi - ncopy), (@unsafe.Pointer)(old.hi - ncopy), ncopy);
    // Adjust remaining structures that have pointers into stacks.
    // We have to do most of these before we traceback the new
    // stack because gentraceback uses them.
    adjustctxt(Ꮡgp, ref adjinfo);
    adjustdefers(Ꮡgp, ref adjinfo);
    adjustpanics(Ꮡgp, ref adjinfo);
    if (adjinfo.sghi != 0) {
        adjinfo.sghi += adjinfo.delta;
    }
    // Swap out old stack for new one
    gp.stack = @new;
    gp.stackguard0 = @new.lo + (uintptr)stackGuard; // NOTE: might clobber a preempt request
    gp.sched.sp = @new.hi - used;
    gp.stktopsp += adjinfo.delta;
    // Adjust pointers in the new stack.
    ref var u = ref heap(new unwinder(), out var Ꮡu);
    for (Ꮡu.init(Ꮡgp, 0); u.valid(); Ꮡu.next()) {
        adjustframe(Ꮡu.of(unwinder.Ꮡframe), ref adjinfo);
    }
    // free old stack
    if (stackPoisonCopy != 0) {
        fillstack(old, 0xfc);
    }
    stackfree(old);
}

// round x up to a power of 2.
internal static int32 round2(int32 x) {
    nuint s = (nuint)0;
    while (((int32)1).Lsh(s) < x) {
        s++;
    }
    return ((int32)1).Lsh(s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stackGrowthAfterForkˢ = "stack growth after fork"u8;
internal static readonly @string runtimeWrongGoroutineInˢ = "runtime: wrong goroutine in newstack"u8;
internal static readonly @string unknownˢ3 = "(unknown)"u8;
internal static readonly @string runtimeStackSplitAtBadˢ = "runtime: stack split at bad time"u8;
internal static readonly @string missingStackInNewstackˢ = "missing stack in newstack"u8;
internal static readonly @string runtimeSplitStackˢ = "runtime: split stack overflow"u8;
internal static readonly @string runtimePreemptG0ˢ = "runtime: preempt g0"u8;
internal static readonly @string runtimeGIsRunningButPIsˢ = "runtime: g is running but p is not"u8;
internal static readonly @string stackOverflowˢ = "stack overflow"u8;

// Called from runtime·morestack when more stack is needed.
// Allocate larger stack and relocate to new stack.
// Stack growth is multiplicative, for constant amortized cost.
//
// g->atomicstatus will be Grunning or Gscanrunning upon entry.
// If the scheduler is trying to stop this g, then it will set preemptStop.
//
// This must be nowritebarrierrec because it can be called as part of
// stack growth from other nowritebarrierrec functions, but the
// compiler doesn't check this.
//
//go:nowritebarrierrec
internal static void newstack() {
    var thisg = getg();
    // TODO: double check all gp. shouldn't be getg().
    if ((~(~(~thisg).m).morebuf.g.ptr()).stackguard0 == stackFork) {
        @throw(stackGrowthAfterForkˢ);
    }
    if ((~(~thisg).m).morebuf.g.ptr() != (~(~thisg).m).curg) {
        print((@string)"runtime: newstack called from g="u8, ((Δhex)(uint64)(uintptr)(~(~thisg).m).morebuf.g), (@string)("\n" + "\tm="), (~thisg).m.OrTypedNil(), (@string)" m->curg="u8, (~(~thisg).m).curg.OrTypedNil(), (@string)" m->g0="u8, (~(~thisg).m).g0.OrTypedNil(), (@string)" m->gsignal="u8, (~(~thisg).m).gsignal.OrTypedNil(), (@string)"\n"u8);
        var morebufΔ1 = thisg.Value.m.Value.morebuf;
        traceback(morebufΔ1.pc, morebufΔ1.sp, morebufΔ1.lr, morebufΔ1.g.ptr());
        @throw(runtimeWrongGoroutineInˢ);
    }
    var gp = thisg.Value.m.Value.curg;
    if ((~(~(~thisg).m).curg).throwsplit) {
        // Update syscallsp, syscallpc in case traceback uses them.
        var morebufΔ2 = thisg.Value.m.Value.morebuf;
        gp.Value.syscallsp = morebufΔ2.sp;
        gp.Value.syscallpc = morebufΔ2.pc;
        @string pcname = unknownˢ3;
        var pcoff = (uintptr)0;
        var f = findfunc((~gp).sched.pc);
        if (f.valid()) {
            pcname = funcname(f);
            pcoff = (~gp).sched.pc - f.entry();
        }
        print((@string)"runtime: newstack at "u8, pcname, (@string)"+"u8, ((Δhex)(uint64)pcoff),
            (@string)" sp="u8, ((Δhex)(uint64)(~gp).sched.sp), (@string)" stack=["u8, ((Δhex)(uint64)(~gp).stack.lo), (@string)", "u8, ((Δhex)(uint64)(~gp).stack.hi), (@string)"]\n"u8,
            (@string)"\tmorebuf={pc:"u8, ((Δhex)(uint64)morebufΔ2.pc), (@string)" sp:"u8, ((Δhex)(uint64)morebufΔ2.sp), (@string)" lr:"u8, ((Δhex)(uint64)morebufΔ2.lr), (@string)"}\n"u8,
            (@string)"\tsched={pc:"u8, ((Δhex)(uint64)(~gp).sched.pc), (@string)" sp:"u8, ((Δhex)(uint64)(~gp).sched.sp), (@string)" lr:"u8, ((Δhex)(uint64)(~gp).sched.lr), (@string)" ctxt:"u8, (~gp).sched.ctxt, (@string)"}\n"u8);
        thisg.Value.m.Value.traceback = 2; // Include runtime frames
        traceback(morebufΔ2.pc, morebufΔ2.sp, morebufΔ2.lr, gp);
        @throw(runtimeStackSplitAtBadˢ);
    }
    var morebuf = thisg.Value.m.Value.morebuf;
    thisg.Value.m.Value.morebuf.pc = 0;
    thisg.Value.m.Value.morebuf.lr = 0;
    thisg.Value.m.Value.morebuf.sp = 0;
    thisg.Value.m.Value.morebuf.g = 0;
    // NOTE: stackguard0 may change underfoot, if another thread
    // is about to try to preempt gp. Read it just once and use that same
    // value now and below.
    var stackguard0 = atomic.Loaduintptr(gp.of(g.Ꮡstackguard0));
    // Be conservative about where we preempt.
    // We are interested in preempting user Go code, not runtime code.
    // If we're holding locks, mallocing, or preemption is disabled, don't
    // preempt.
    // This check is very early in newstack so that even the status change
    // from Grunning to Gwaiting and back doesn't happen in this case.
    // That status change by itself can be viewed as a small preemption,
    // because the GC might change Gwaiting to Gscanwaiting, and then
    // this goroutine has to wait for the GC to finish before continuing.
    // If the GC is in some way dependent on this goroutine (for example,
    // it needs a lock held by the goroutine), that small preemption turns
    // into a real deadlock.
    var preempt = stackguard0 == stackPreempt;
    if (preempt) {
        if (!canPreemptM(ref ((~thisg).m).DerefOrNull())) {
            // Let the goroutine keep running for now.
            // gp->preempt is set, so it will be preempted next time.
            gp.Value.stackguard0 = (~gp).stack.lo + (uintptr)stackGuard;
            gogo(gp.of(g.Ꮡsched)); // never return
        }
    }
    if ((~gp).stack.lo == 0) {
        @throw(missingStackInNewstackˢ);
    }
    var sp = gp.Value.sched.sp;
    if (goarch.ArchFamily == goarch.AMD64 || goarch.ArchFamily == goarch.I386 || goarch.ArchFamily == goarch.WASM) {
        // The call to morestack cost a word.
        sp -= goarch.PtrSize;
    }
    if (stackDebug >= 1 || sp < (~gp).stack.lo) {
        print((@string)"runtime: newstack sp="u8, ((Δhex)(uint64)sp), (@string)" stack=["u8, ((Δhex)(uint64)(~gp).stack.lo), (@string)", "u8, ((Δhex)(uint64)(~gp).stack.hi), (@string)"]\n"u8,
            (@string)"\tmorebuf={pc:"u8, ((Δhex)(uint64)morebuf.pc), (@string)" sp:"u8, ((Δhex)(uint64)morebuf.sp), (@string)" lr:"u8, ((Δhex)(uint64)morebuf.lr), (@string)"}\n"u8,
            (@string)"\tsched={pc:"u8, ((Δhex)(uint64)(~gp).sched.pc), (@string)" sp:"u8, ((Δhex)(uint64)(~gp).sched.sp), (@string)" lr:"u8, ((Δhex)(uint64)(~gp).sched.lr), (@string)" ctxt:"u8, (~gp).sched.ctxt, (@string)"}\n"u8);
    }
    if (sp < (~gp).stack.lo) {
        print((@string)"runtime: gp="u8, gp.OrTypedNil(), (@string)", goid="u8, (~gp).goid, (@string)", gp->status="u8, ((Δhex)(uint64)readgstatus(gp)), (@string)"\n "u8);
        print((@string)"runtime: split stack overflow: "u8, ((Δhex)(uint64)sp), (@string)" < "u8, ((Δhex)(uint64)(~gp).stack.lo), (@string)"\n"u8);
        @throw(runtimeSplitStackˢ);
    }
    if (preempt) {
        if (gp == (~(~thisg).m).g0) {
            @throw(runtimePreemptG0ˢ);
        }
        if ((~(~thisg).m).p == 0 && (~(~thisg).m).locks == 0) {
            @throw(runtimeGIsRunningButPIsˢ);
        }
        if ((~gp).preemptShrink) {
            // We're at a synchronous safe point now, so
            // do the pending stack shrink.
            gp.Value.preemptShrink = false;
            shrinkstack(gp);
        }
        if ((~gp).preemptStop) {
            preemptPark(gp); // never returns
        }
        // Act like goroutine called runtime.Gosched.
        gopreempt_m(gp); // never return
    }
    // Allocate a bigger segment and move the stack.
    var oldsize = (~gp).stack.hi - (~gp).stack.lo;
    var newsize = oldsize * 2;
    // Make sure we grow at least as much as needed to fit the new frame.
    // (This is just an optimization - the caller of morestack will
    // recheck the bounds on return.)
    {
        var f = findfunc((~gp).sched.pc); if (f.valid()) {
            var max = (uintptr)funcMaxSPDelta(f);
            var needed = max + (uintptr)stackGuard;
            var used = (~gp).stack.hi - (~gp).sched.sp;
            while (newsize - used < needed) {
                newsize *= 2;
            }
        }
    }
    if (stackguard0 == stackForceMove) {
        // Forced stack movement used for debugging.
        // Don't double the stack (or we may quickly run out
        // if this is done repeatedly).
        newsize = oldsize;
    }
    if (newsize > maxstacksize || newsize > maxstackceiling) {
        if (maxstacksize < maxstackceiling){
            print((@string)"runtime: goroutine stack exceeds "u8, maxstacksize, (@string)"-byte limit\n"u8);
        } else {
            print((@string)"runtime: goroutine stack exceeds "u8, maxstackceiling, (@string)"-byte limit\n"u8);
        }
        print((@string)"runtime: sp="u8, ((Δhex)(uint64)sp), (@string)" stack=["u8, ((Δhex)(uint64)(~gp).stack.lo), (@string)", "u8, ((Δhex)(uint64)(~gp).stack.hi), (@string)"]\n"u8);
        @throw(stackOverflowˢ);
    }
    // The goroutine must be executing in order to call newstack,
    // so it must be Grunning (or Gscanrunning).
    casgstatus(gp, _Grunning, _Gcopystack);
    // The concurrent GC will not scan the stack while we are doing the copy since
    // the gp is in a Gcopystack status.
    copystack(gp, newsize);
    if (stackDebug >= 1) {
        print((@string)"stack grow done\n"u8);
    }
    casgstatus(gp, _Gcopystack, _Grunning);
    gogo(gp.of(g.Ꮡsched));
}

//go:nosplit
internal static void nilfunc() {
    (((ж<uint8>)nil)).Value = 0;
}

// adjust Gobuf as if it executed a call to fn
// and then stopped before the first instruction in fn.
internal static void gostartcallfn(ref gobuf gobuf, ж<funcval> Ꮡfv) {
    ref var fv = ref Ꮡfv.DerefOrNull();

    @unsafe.Pointer fn = default!;
    if (Ꮡfv != nil){
        fn = (@unsafe.Pointer)fv.fn;
    } else {
        fn = (@unsafe.Pointer)abi.FuncPCABIInternal(nilfunc);
    }
    gostartcall(ref gobuf, fn, new @unsafe.Pointer(Ꮡfv));
}

// isShrinkStackSafe returns whether it's safe to attempt to shrink
// gp's stack. Shrinking the stack is only safe when we have precise
// pointer maps for all frames on the stack. The caller must hold the
// _Gscan bit for gp or must be running gp itself.
internal static bool isShrinkStackSafe(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    // We can't copy the stack if we're in a syscall.
    // The syscall might have pointers into the stack and
    // often we don't have precise pointer maps for the innermost
    // frames.
    if (gp.syscallsp != 0) {
        return false;
    }
    // We also can't copy the stack if we're at an asynchronous
    // safe-point because we don't have precise pointer maps for
    // all frames.
    if (gp.asyncSafePoint) {
        return false;
    }
    // We also can't *shrink* the stack in the window between the
    // goroutine calling gopark to park on a channel and
    // gp.activeStackChans being set.
    if (Ꮡgp.of(g.ᏑparkingOnChan).Load()) {
        return false;
    }
    // We also can't copy the stack while tracing is enabled, and
    // gp is in _Gwaiting solely to make itself available to suspendG.
    // In these cases, the G is actually executing on the system
    // stack, and the execution tracer may want to take a stack trace
    // of the G's stack. Note: it's safe to access gp.waitreason here.
    // We're only checking if this is true if we took ownership of the
    // G with the _Gscan bit. This prevents the goroutine from transitioning,
    // which prevents gp.waitreason from changing.
    if (traceEnabled() && (uint32)(readgstatus(Ꮡgp) & ~(uint32)_Gscan) == _Gwaiting && gp.waitreason.isWaitingForSuspendG()) {
        return false;
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string missingStackInˢ = "missing stack in shrinkstack"u8;
internal static readonly @string badStatusInShrinkstackˢ = "bad status in shrinkstack"u8;
internal static readonly @string shrinkstackAtBadTimeˢ = "shrinkstack at bad time"u8;
internal static readonly @string shrinkingStackInLibcallˢ = "shrinking stack in libcall"u8;

// Maybe shrink the stack being used by gp.
//
// gp must be stopped and we must own its stack. It may be in
// _Grunning, but only if this is our own user G.
internal static void shrinkstack(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    if (gp.stack.lo == 0) {
        @throw(missingStackInˢ);
    }
    {
        var s = readgstatus(Ꮡgp); if ((uint32)(s & (uint32)_Gscan) == 0) {
            // We don't own the stack via _Gscan. We could still
            // own it if this is our own user G and we're on the
            // system stack.
            if (!(Ꮡgp == (~(~getg()).m).curg && getg() != (~(~getg()).m).curg && s == _Grunning)) {
                // We don't own the stack.
                @throw(badStatusInShrinkstackˢ);
            }
        }
    }
    if (!isShrinkStackSafe(Ꮡgp)) {
        @throw(shrinkstackAtBadTimeˢ);
    }
    // Check for self-shrinks while in a libcall. These may have
    // pointers into the stack disguised as uintptrs, but these
    // code paths should all be nosplit.
    if (Ꮡgp == (~(~getg()).m).curg && (~gp.m).libcallsp != 0) {
        @throw(shrinkingStackInLibcallˢ);
    }
    if (debug.gcshrinkstackoff > 0) {
        return;
    }
    var f = findfunc(gp.startpc);
    if (f.valid() && f.funcID == abi.FuncID_gcBgMarkWorker) {
        // We're not allowed to shrink the gcBgMarkWorker
        // stack (see gcBgMarkWorker for explanation).
        return;
    }
    var oldsize = gp.stack.hi - gp.stack.lo;
    var newsize = oldsize / 2;
    // Don't shrink the allocation below the minimum-sized stack
    // allocation.
    if (newsize < fixedStack) {
        return;
    }
    // Compute how much of the stack is currently in use and only
    // shrink the stack if gp is using less than a quarter of its
    // current stack. The currently used stack includes everything
    // down to the SP plus the stack guard space that ensures
    // there's room for nosplit functions.
    var avail = gp.stack.hi - gp.stack.lo;
    {
        var used = gp.stack.hi - gp.sched.sp + (uintptr)stackNosplit; if (used >= avail / 4) {
            return;
        }
    }
    if (stackDebug > 0) {
        print((@string)"shrinking stack "u8, oldsize, (@string)"->"u8, newsize, (@string)"\n"u8);
    }
    copystack(Ꮡgp, newsize);
}

// freeStackSpans frees unused stack spans at the end of GC.
internal static void freeStackSpans() {
    // Scan stack pools for empty stack spans.
    foreach (var (order, _) in stackpool) {
        @lock(Ꮡstackpool.at<stackpoolᴛ1>(order).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
        var list = Ꮡstackpool.at<stackpoolᴛ1>(order).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡspan);
        for (var s = list.Value.first; s != nil; ) {
            var next = s.Value.next;
            if ((~s).allocCount == 0) {
                list.remove(s);
                s.Value.manualFreeList = 0;
                osStackFree(ref (s).DerefOrNull());
                Ꮡmheap_.freeManual(s, spanAllocStack);
            }
            s = next;
        }
        unlock(Ꮡstackpool.at<stackpoolᴛ1>(order).of(stackpoolᴛ1.Ꮡitem).of(stackpoolItem.Ꮡmu));
    }
    // Free large stack spans.
    @lock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
    foreach (var (i, _) in stackLarge.free) {
        for (var s = stackLarge.free[i].first; s != nil; ) {
            var next = s.Value.next;
            ᏑstackLarge.at(stackLargeᴛ1.Ꮡfree, i).remove(s);
            osStackFree(ref (s).DerefOrNull());
            Ꮡmheap_.freeManual(s, spanAllocStack);
            s = next;
        }
    }
    unlock(ᏑstackLarge.of(stackLargeᴛ1.Ꮡlock));
}

// A stackObjectRecord is generated by the compiler for each stack object in a stack frame.
// This record must match the generator code in cmd/compile/internal/liveness/plive.go:emitStackObjects.
[GoType] partial struct stackObjectRecord {
    // offset in frame
    // if negative, offset from varp
    // if non-negative, offset from argp
    internal int32 off;
    internal int32 size;
    internal int32 _ptrdata;  // ptrdata, or -ptrdata is GC prog is used
    internal uint32 gcdataoff; // offset to gcdata from moduledata.rodata
}

[GoRecv] internal static bool useGCProg(this ref stackObjectRecord r) {
    return r._ptrdata < 0;
}

[GoRecv] internal static uintptr ptrdata(this ref stackObjectRecord r) {
    var x = r._ptrdata;
    if (x < 0) {
        return (uintptr)(-x);
    }
    return (uintptr)x;
}

// gcdata returns pointer map or GC prog of the type.
internal static ж<byte> gcdata(this ж<stackObjectRecord> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    var ptr = (uintptr)(uintptr)@unsafe.Pointer.FromRef(ref r);
    ж<moduledata> mod = default!;
    for (var datap = Ꮡfirstmoduledata; datap != nil; datap = datap.Value.next) {
        if ((~datap).gofunc <= ptr && ptr < (~datap).end) {
            mod = datap;
            break;
        }
    }
    // If you get a panic here due to a nil mod,
    // you may have made a copy of a stackObjectRecord.
    // You must use the original pointer.
    var res = (~mod).rodata + (uintptr)r.gcdataoff;
    return (ж<byte>)(uintptr)((@unsafe.Pointer)res);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string attemptToExecuteSystemˢ = "attempt to execute system stack code on user stack"u8;

// This is exported as ABI0 via linkname so obj can call it.
//
//go:nosplit
//go:linkname morestackc
internal static void morestackc() {
    @throw(attemptToExecuteSystemˢ);
}

// startingStackSize is the amount of stack that new goroutines start with.
// It is a power of 2, and between fixedStack and maxstacksize, inclusive.
// startingStackSize is updated every GC by tracking the average size of
// stacks scanned during the GC.
internal static uint32 startingStackSize = fixedStack;

internal static void gcComputeStartingStackSize() {
    if (debug.adaptivestackstart == 0) {
        return;
    }
    // For details, see the design doc at
    // https://docs.google.com/document/d/1YDlGIdVTPnmUiTAavlZxBI1d9pwGQgZT7IKFKlIXohQ/edit?usp=sharing
    // The basic algorithm is to track the average size of stacks
    // and start goroutines with stack equal to that average size.
    // Starting at the average size uses at most 2x the space that
    // an ideal algorithm would have used.
    // This is just a heuristic to avoid excessive stack growth work
    // early in a goroutine's lifetime. See issue 18138. Stacks that
    // are allocated too small can still grow, and stacks allocated
    // too large can still shrink.
    uint64 scannedStackSize = default!;
    uint64 scannedStacks = default!;
    foreach (var (_, Δp) in allp) {
        scannedStackSize += Δp.Value.scannedStackSize;
        scannedStacks += Δp.Value.scannedStacks;
        // Reset for next time
        Δp.Value.scannedStackSize = 0;
        Δp.Value.scannedStacks = 0;
    }
    if (scannedStacks == 0) {
        startingStackSize = fixedStack;
        return;
    }
    var avg = scannedStackSize / scannedStacks + (uint64)stackGuard;
    // Note: we add stackGuard to ensure that a goroutine that
    // uses the average space will not trigger a growth.
    if (avg > (uint64)maxstacksize) {
        avg = (uint64)maxstacksize;
    }
    if (avg < fixedStack) {
        avg = fixedStack;
    }
    // Note: maxstacksize fits in 30 bits, so avg also does.
    startingStackSize = (uint32)round2((int32)avg);
}

} // end runtime_package
