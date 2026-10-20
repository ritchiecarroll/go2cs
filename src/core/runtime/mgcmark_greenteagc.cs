// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Green Tea mark algorithm
//
// The core idea behind Green Tea is simple: achieve better locality during
// mark/scan by delaying scanning so that we can accumulate objects to scan
// within the same span, then scan the objects that have accumulated on the
// span all together.
//
// By batching objects this way, we increase the chance that adjacent objects
// will be accessed, amortize the cost of accessing object metadata, and create
// better opportunities for prefetching. We can take this even further and
// optimize the scan loop by size class (not yet completed) all the way to the
// point of applying SIMD techniques to really tear through the heap.
//
// Naturally, this depends on being able to create opportunties to batch objects
// together. The basic idea here is to have two sets of mark bits. One set is the
// regular set of mark bits ("marks"), while the other essentially says that the
// objects have been scanned already ("scans"). When we see a pointer for the first
// time we set its mark and enqueue its span. We track these spans in work queues
// with a FIFO policy, unlike workbufs which have a LIFO policy. Empirically, a
// FIFO policy appears to work best for accumulating objects to scan on a span.
// Later, when we dequeue the span, we find both the union and intersection of the
// mark and scan bitsets. The union is then written back into the scan bits, while
// the intersection is used to decide which objects need scanning, such that the GC
// is still precise.
//
// Below is the bulk of the implementation, focusing on the worst case
// for locality, small objects. Specifically, those that are smaller than
// a few cache lines in size and whose metadata is stored the same way (at the
// end of the span).
//go:build goexperiment.greenteagc
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using gc = @internal.runtime.gc_package;
using scan = @internal.runtime.gc.scan_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using @internal.runtime.gc;

[GoFieldAccess(typeof(spanSPMC))]
partial class runtime_package {

internal const bool doubleCheckGreenTea = false;

// spanInlineMarkBits are mark bits that are inlined into the span
// itself. gcUsesSpanInlineMarkBits may be used to check if objects
// of a particular size use inline mark bits.
//
// Inline mark bits are a little bit more than just mark bits. They
// consist of two parts: scans and marks. Marks are like pre-mark
// bits. They're set once a pointer to an object is discovered for
// the first time. The marks allow us to scan many objects in bulk
// if we queue the whole span for scanning. Before we scan such objects
// in bulk, we copy the marks to the scans, computing a diff along the
// way. The resulting bitmap tells us which objects we should scan.
//
// The inlineMarkBits also hold state sufficient for scanning any
// object in the span, as well as state for acquiring ownership of
// the span for queuing. This avoids the need to look at the mspan when
// scanning.
partial struct spanInlineMarkBits {
    internal array<uint8> scans = new(63);      // scanned bits.
    internal spanScanOwnership owned; // see the comment on spanScanOwnership.
    internal array<uint8> marks = new(63);      // mark bits.
    internal spanClass @class;
}

// spanScanOwnership indicates whether some thread has acquired
// the span for scanning, and whether there has been one or more
// attempts to acquire the span. The latter information helps to
// fast-track span scans that only apply to a single mark, skipping
// the relatively costly merge-and-diff process for scans and marks
// by allowing one to just set the mark directly.
partial struct spanScanOwnership /*num:uint8*/;

internal static spanScanOwnership spanScanUnowned => 0;      // Indicates the span is not acquired for scanning.
internal static UntypedInt spanScanOneMark => /* 1 << iota */ 2; // Indicates that only one mark bit is set relative to the scan bits.
internal static UntypedInt spanScanManyMark => 4;            // Indicates one or more scan bits may be set relative to the mark bits.

// "ManyMark" need not be exactly the value it has. In practice we just
// want to distinguish "none" from "one" from "many," so a comparison is
// sufficient (as opposed to a bit test) to check between these cases.

// load atomically loads from a pointer to a spanScanOwnership.
internal static spanScanOwnership load(this ж<spanScanOwnership> Ꮡo) {
    return (spanScanOwnership)atomic.Load8(Ꮡo.Reinterpret<spanScanOwnership, uint8>());
}

internal static spanScanOwnership or(this ж<spanScanOwnership> Ꮡo, spanScanOwnership v) {
    ref var o = ref Ꮡo.DerefOrNull();

    // N.B. We round down the address and use Or32 because Or8 doesn't
    // return a result, and it's strictly necessary for this protocol.
    //
    // Making Or8 return a result, while making the code look nicer, would
    // not be strictly better on any supported platform, as an Or8 that
    // returns a result is not a common instruction. On many platforms it
    // would be implemented exactly as it is here, and since Or8 is
    // exclusively used in the runtime and a hot function, we want to keep
    // using its no-result version elsewhere for performance.
    var o32 = (ж<uint32>)(uintptr)((@unsafe.Pointer)((uintptr)((uintptr)(uintptr)@unsafe.Pointer.FromRef(ref o) & ~(uintptr)0b11)));
    var off = ((uintptr)((uintptr)(uintptr)@unsafe.Pointer.FromRef(ref o) & 0b11)) * 8;
    if (goarch.BigEndian) {
        off = 32 - off - 8;
    }
    return (spanScanOwnership)(uint8)(atomic.Or32(o32, ((uint32)(uint8)v).Lsh((uint64)(off))).Rsh((uint64)(off)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeSpanInlineMarkˢ = "runtime: span inline mark bits nil?"u8;

internal static void init(this ж<spanInlineMarkBits> Ꮡimb, spanClass @class, bool needzero) {
    ref var imb = ref Ꮡimb.DerefOrNull();

    if (Ꮡimb == nil) {
        // This nil check and throw is almost pointless. Normally we would
        // expect imb to never be nil. However, this is called on potentially
        // freshly-allocated virtual memory. As of 2025, the compiler-inserted
        // nil check is not a branch but a memory read that we expect to fault
        // if the pointer really is nil.
        //
        // However, this causes a read of the page, and operating systems may
        // take it as a hint to back the accessed memory with a read-only zero
        // page. However, we immediately write to this memory, which can then
        // force operating systems to have to update the page table and flush
        // the TLB, causing a lot of churn for programs that are short-lived
        // and monotonically grow in size.
        //
        // This nil check is thus an explicit branch instead of what the compiler
        // would insert circa 2025, which is a memory read instruction.
        //
        // See go.dev/issue/74375 for details.
        @throw(runtimeSpanInlineMarkˢ);
    }
    if (needzero) {
        // Use memclrNoHeapPointers to avoid having the compiler make a worse
        // decision. We know that imb is both aligned and a nice power-of-two
        // size that works well for wider SIMD instructions. The compiler likely
        // has no idea that imb is aligned to 128 bytes.
        memclrNoHeapPointers((uintptr)@unsafe.Pointer.FromRef(ref imb), /* unsafe.Sizeof(spanInlineMarkBits{}) */ (uintptr)128);
    }
    imb.@class = @class;
}

// tryAcquire attempts to acquire the span for scanning. On success, the caller
// must queue the span for scanning or scan the span immediately.
internal static bool tryAcquire(this ж<spanInlineMarkBits> Ꮡimb) {
    var exprᴛ1 = Ꮡimb.of(spanInlineMarkBits.Ꮡowned).load();
    var matchᴛ1 = false;
    if (exprᴛ1 == spanScanUnowned) {
        // Try to mark the span as having only one object marked.
        if (Ꮡimb.of(spanInlineMarkBits.Ꮡowned).or(spanScanOneMark) == spanScanUnowned) {
            return true;
        }
        // If we didn't see an old value of spanScanUnowned, then we must
        // have raced with someone else and seen spanScanOneMark or greater.
        // Fall through and try to set spanScanManyMark.
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 == spanScanOneMark) { matchᴛ1 = true;
        // We may be the first to set *any* bit on owned. In such a case,
        // we still need to make sure the span is queued.
        return Ꮡimb.of(spanInlineMarkBits.Ꮡowned).or(spanScanManyMark) == spanScanUnowned;
    }
    return false;
}

// release releases the span for scanning, allowing another thread to queue the span.
//
// Returns an upper bound on the number of mark bits set since the span was queued. The
// upper bound is described as "one" (spanScanOneMark) or "many" (spanScanManyMark, with or
// without spanScanOneMark). If the return value indicates only one mark bit was set, the
// caller can be certain that it was the same mark bit that caused the span to get queued.
// Take note of the fact that this is *only* an upper-bound. In particular, it may still
// turn out that only one mark bit was set, even if the return value indicates "many".
internal static spanScanOwnership release(this ж<spanInlineMarkBits> Ꮡimb) {
    return (spanScanOwnership)atomic.Xchg8(Ꮡimb.of(spanInlineMarkBits.Ꮡowned).Reinterpret<spanScanOwnership, uint8>(), (uint8)spanScanUnowned);
}

// spanInlineMarkBitsFromBase returns the spanInlineMarkBits for a span whose start address is base.
//
// The span must be gcUsesSpanInlineMarkBits(span.elemsize).
internal static ж<spanInlineMarkBits> spanInlineMarkBitsFromBase(uintptr @base) {
    return (ж<spanInlineMarkBits>)(uintptr)((@unsafe.Pointer)(@base + (uintptr)gc.PageSize - /* unsafe.Sizeof(spanInlineMarkBits{}) */ (uintptr)128));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string expectedSpanWithInlineˢ = "expected span with inline mark bits"u8;

// initInlineMarkBits initializes the inlineMarkBits stored at the end of the span.
internal static void initInlineMarkBits(this ref mspan s) {
    if (doubleCheckGreenTea && !gcUsesSpanInlineMarkBits(s.elemsize)) {
        @throw(expectedSpanWithInlineˢ);
    }
    // Zeroing is only necessary if this span wasn't just freshly allocated from the OS.
    s.inlineMarkBits().init(s.spanclass, s.needzero != 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string marksDonTMatchScansForˢ = "marks don't match scans for span with pointer"u8;

// moveInlineMarks merges the span's inline mark bits into dst and clears them.
//
// gcUsesSpanInlineMarkBits(s.elemsize) must be true.
internal static void moveInlineMarks(this ref mspan s, ж<gcBits> Ꮡdst) {
    if (doubleCheckGreenTea && !gcUsesSpanInlineMarkBits(s.elemsize)) {
        @throw(expectedSpanWithInlineˢ);
    }
    var bytes = divRoundUp((uintptr)s.nelems, 8);
    var imb = s.inlineMarkBits();
    var imbMarks = (ж<gc.ObjMask>)(uintptr)(@unsafe.Pointer.FromPinnedBox(imb.of(spanInlineMarkBits.Ꮡmarks)));
    for (var i = (uintptr)0; i < bytes; i += goarch.PtrSize) {
        var marks = bswapIfBigEndian(imbMarks.Value[i / (uintptr)goarch.PtrSize]);
        if (i / (uintptr)goarch.PtrSize == (uintptr)((uintptr)((uintptr)(len((~imb).marks) + 1) / (uintptr)goarch.PtrSize) - 1)) {
            marks &= unchecked((uintptr)~(uintptr)(((uintptr)0xff << (int)(((goarch.PtrSize - 1) * 8))))); // mask out class
        }
        (Ꮡdst.bytep(i).Reinterpret<uint8, uintptr>()).Value |= bswapIfBigEndian(marks);
    }
    if (doubleCheckGreenTea && !s.spanclass.noscan() && (~imb).marks != (~imb).scans) {
        @throw(marksDonTMatchScansForˢ);
    }

    // Reset the inline mark bits.
    imb.init(s.spanclass, true /* We know these bits are always dirty now. */);
}

// inlineMarkBits returns the inline mark bits for the span.
//
// gcUsesSpanInlineMarkBits(s.elemsize) must be true.
internal static ж<spanInlineMarkBits> inlineMarkBits(this ref mspan s) {
    if (doubleCheckGreenTea && !gcUsesSpanInlineMarkBits(s.elemsize)) {
        @throw(expectedSpanWithInlineˢ);
    }
    return spanInlineMarkBitsFromBase(s.@base());
}

internal static markBits /*bits*/ markBitsForIndex(this ref mspan s, uintptr objIndex) {
    markBits bits = default!;

    if (gcUsesSpanInlineMarkBits(s.elemsize)){
        bits.bytep = s.inlineMarkBits().at(spanInlineMarkBits.Ꮡmarks, (ulong)(objIndex / 8));
    } else {
        bits.bytep = s.gcmarkBits.bytep(objIndex / 8);
    }
    bits.mask = (uint8)((uint8)1 << (int)((objIndex % 8)));
    bits.index = objIndex;
    return bits;
}

internal static markBits markBitsForBase(this ref mspan s) {
    if (gcUsesSpanInlineMarkBits(s.elemsize)) {
        return new markBits(s.inlineMarkBits().at(spanInlineMarkBits.Ꮡmarks, 0), (uint8)1, 0);
    }
    return new markBits(s.gcmarkBits.of(gcBits.Ꮡx), (uint8)1, 0);
}

// scannedBitsForIndex returns a markBits representing the scanned bit
// for objIndex in the inline mark bits.
internal static markBits scannedBitsForIndex(this ref mspan s, uintptr objIndex) {
    return new markBits(s.inlineMarkBits().at(spanInlineMarkBits.Ꮡscans, (ulong)(objIndex / 8)), (uint8)((uint8)1 << (int)((objIndex % 8))), objIndex);
}

// gcUsesSpanInlineMarkBits returns true if a span holding objects of a certain size
// has inline mark bits. size must be the span's elemsize.
//
// nosplit because this is called from gcmarknewobject, which is nosplit.
//
//go:nosplit
internal static bool gcUsesSpanInlineMarkBits(uintptr size) {
    return heapBitsInSpan(size) && size >= 16;
}

// tryDeferToSpanScan tries to queue p on the span it points to, if it
// points to a small object span (gcUsesSpanQueue size).
internal static bool tryDeferToSpanScan(uintptr Δp, ж<gcWork> Ꮡgcw) {
    ref var gcw = ref Ꮡgcw.DerefOrNull();

    if (useCheckmark) {
        return false;
    }

    // Quickly to see if this is a span that has inline mark bits.
    var ha = heapArenaOf(Δp);
    if (ha == nil) {
        return false;
    }
    var pageIdx = ((Δp / (uintptr)pageSize) / 8) % (uintptr)len((~ha).pageInUse);
    var pageMask = (byte)((byte)(1 << (int)(((Δp / (uintptr)pageSize) % 8))));
    if ((uint8)((~ha).pageUseSpanInlineMarkBits[pageIdx] & pageMask) == 0) {
        return false;
    }

    // Find the object's index from the span class info stored in the inline mark bits.
    var @base = alignDown(Δp, gc.PageSize);
    var q = spanInlineMarkBitsFromBase(@base);
    var objIndex = (uint16)(((uint64)(Δp - @base) * (uint64)gc.SizeClassToDivMagic[(~q).@class.sizeclass()]) >> 32);

    // Set mark bit.
    var (idx, mask) = ((uint16)(objIndex / 8), (uint8)((uint8)1 << (int)((objIndex % 8))));
    if ((uint8)(atomic.Load8(q.at(spanInlineMarkBits.Ꮡmarks, (nint)(idx))) & mask) != 0) {
        return true;
    }
    atomic.Or8(q.at(spanInlineMarkBits.Ꮡmarks, (nint)(idx)), mask);

    // Fast-track noscan objects.
    if ((~q).@class.noscan()) {
        gcw.bytesMarked += (uint64)gc.SizeClassToSize[(~q).@class.sizeclass()];
        return true;
    }

    // Queue up the pointer (as a representative for its span).
    if (q.tryAcquire()) {
        if (Ꮡgcw.of(gcWork.Ꮡspanq).put(makeObjPtr(@base, objIndex))) {
            if (gcphase == _GCmark) {
                // This is intentionally racy; the bit set here might get
                // stomped on by a stealing P. See the comment in tryStealSpan
                // for an explanation as to why this is OK.
                if (!work.spanqMask.read((uint32)gcw.id)) {
                    work.spanqMask.set(gcw.id);
                }
                gcw.mayNeedWorker = true;
            }
            gcw.flushedWork = true;
        }
    }
    return true;
}

// tryGetSpanFast attempts to get an entire span to scan.
internal static objptr tryGetSpanFast(this ref gcWork w) {
    return w.spanq.tryGetFast();
}

// tryGetSpan attempts to get an entire span to scan.
internal static objptr tryGetSpan(this ж<gcWork> Ꮡw) {
    ref var w = ref Ꮡw.DerefOrNull();

    {
        var s = w.spanq.tryGetFast(); if (s != 0) {
            return s;
        }
    }
    // "Steal" from ourselves.
    {
        var s = w.spanq.steal(Ꮡw.of(gcWork.Ꮡspanq)); if (s != 0) {
            return s;
        }
    }
    // We failed to get any local work, so we're fresh out.
    // Nobody else is going to add work for us. Clear our bit.
    if (work.spanqMask.read((uint32)w.id)) {
        work.spanqMask.clear(w.id);
    }
    return 0;
}

partial struct spanQueue_chain /*dyn*/ {
    // head is the spanSPMC to put to. This is only accessed
    // by the producer, so doesn't need to be synchronized.
    internal ж<spanSPMC> head;
    // tail is the spanSPMC to steal from. This is accessed
    // by consumers, so reads and writes must be atomic.
    internal atomic.UnsafePointer tail; // *spanSPMC
}

// spanQueue is a P-local stealable span queue.
partial struct spanQueue {
    // head, tail, and ring represent a local non-thread-safe ring buffer.
    internal uint32 head, tail;
    internal array<objptr> ring = new(256);
    // putsSinceDrain counts the number of put calls since the last drain.
    internal nint putsSinceDrain;
    // chain contains state visible to other Ps.
    //
    // In particular, that means a linked chain of single-producer multi-consumer
    // ring buffers where the single producer is this P only.
    //
    // This linked chain structure is based off the sync.Pool dequeue.
    internal spanQueue_chain chain;
}

// putFast tries to put s onto the queue, but may fail if it's full.
internal static bool /*ok*/ putFast(this ref spanQueue q, objptr s) {
    if (q.tail - q.head == (uint32)len(q.ring)) {
        return false;
    }
    q.ring[q.tail % (uint32)len(q.ring)] = s;
    q.tail++;
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string failedPutFastAfterDrainˢ = "failed putFast after drain"u8;

// put puts s onto the queue.
//
// Returns whether the caller should spin up a new worker.
internal static bool put(this ж<spanQueue> Ꮡq, objptr s) {
    ref var q = ref Ꮡq.DerefOrNull();

    // The constants below define the period of and volume of
    // spans we spill to the spmc chain when the local queue is
    // not full.
    //
    // spillPeriod must be > spillMax, otherwise that sets the
    // effective maximum size of our local span queue. Even if
    // we have a span ring of size N, but we flush K spans every
    // K puts, then K becomes our effective maximum length. When
    // spillPeriod > spillMax, then we're always spilling spans
    // at a slower rate than we're accumulating them.
    // spillPeriod defines how often to check if we should
    // spill some spans, counted in the number of calls to put.
    const nint spillPeriod = 64;

    // spillMax defines, at most, how many spans to drain with
    // each spill.
    const uint32 spillMax = 16;

    if (q.putFast(s)) {
        // Occasionally try to spill some work to generate parallelism.
        q.putsSinceDrain++;
        if (q.putsSinceDrain >= spillPeriod) {
            // Reset even if we don't drain, so we don't check every time.
            q.putsSinceDrain = 0;

            // Try to drain some spans. Don't bother if there's very
            // few of them or there's already spans in the spmc chain.
            var n = min((q.tail - q.head) / 2, (uint32)(spillMax));
            if (n > 4 && Ꮡq.chainEmpty()) {
                Ꮡq.drain(n);
                return true;
            }
        }
        return false;
    }

    // We're out of space. Drain out our local spans.
    Ꮡq.drain((uint32)len(q.ring) / 2);
    if (!q.putFast(s)) {
        @throw(failedPutFastAfterDrainˢ);
    }
    return true;
}

// flush publishes all spans in the local queue to the spmc chain.
internal static void flush(this ж<spanQueue> Ꮡq) {
    ref var q = ref Ꮡq.DerefOrNull();

    var n = q.tail - q.head;
    if (n == 0) {
        return;
    }
    Ꮡq.drain(n);
}

// empty returns true if there's no more work on the queue.
//
// Not thread-safe. Must only be called by the owner of q.
internal static bool empty(this ж<spanQueue> Ꮡq) {
    ref var q = ref Ꮡq.DerefOrNull();

    // Check the local queue for work.
    if (q.tail - q.head > 0) {
        return false;
    }
    return Ꮡq.chainEmpty();
}

// chainEmpty returns true if the spmc chain is empty.
//
// Thread-safe.
internal static bool chainEmpty(this ж<spanQueue> Ꮡq) {
    // Check the rest of the rings for work.
    var r = (ж<spanSPMC>)(uintptr)(Ꮡq.of(spanQueue.Ꮡchain).of(spanQueue_chain.Ꮡtail).Load());
    while (r != nil) {
        if (!r.empty()) {
            return false;
        }
        r = (ж<spanSPMC>)(uintptr)(r.of(spanSPMC.Ꮡprev).Load());
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string failedToPutSpanOnNewlyˢ = "failed to put span on newly-allocated spanSPMC"u8;

// drain publishes n spans from the local queue to the spmc chain.
internal static void drain(this ж<spanQueue> Ꮡq, uint32 n) {
    ref var q = ref Ꮡq.DerefOrNull();

    q.putsSinceDrain = 0;

    if (q.chain.head == nil) {
        // N.B. We target 1024, but this may be bigger if the physical
        // page size is bigger, or if we can fit more uintptrs into a
        // physical page. See newSpanSPMC docs.
        var r = newSpanSPMC(1024);
        q.chain.head = r;
        Ꮡq.of(spanQueue.Ꮡchain).of(spanQueue_chain.Ꮡtail).StoreNoWB(@unsafe.Pointer.FromPinnedBox(r));
    }

    // Try to drain some of the queue to the head spmc.
    if (q.tryDrain(q.chain.head, n)) {
        return;
    }
    // No space. Create a bigger spmc and add it to the chain.

    // Double the size of the next one, up to a maximum.
    //
    // We double each time so we can avoid taking this slow path
    // in the future, which involves a global lock. Ideally we want
    // to hit a steady-state where the deepest any queue goes during
    // a mark phase can fit in the ring.
    //
    // However, we still set a maximum on this. We set the maximum
    // to something large to amortize the cost of lock acquisition, but
    // still at a reasonable size for big heaps and/or a lot of Ps (which
    // tend to be correlated).
    //
    // It's not too bad to burn relatively large-but-fixed amounts of per-P
    // memory if we need to deal with really, really deep queues, since the
    // constants of proportionality are small. Simultaneously, we want to
    // avoid a situation where a single worker ends up queuing O(heap)
    // work and then forever retains a queue of that size.
    const uint32 maxCap = /* 1 << 20 / goarch.PtrSize */ 131072;
    var newCap = (~q.chain.head).cap * 2;
    if (newCap > maxCap) {
        newCap = maxCap;
    }
    var newHead = newSpanSPMC(newCap);
    if (!q.tryDrain(newHead, n)) {
        @throw(failedToPutSpanOnNewlyˢ);
    }
    q.chain.head.of(spanSPMC.Ꮡprev).StoreNoWB(@unsafe.Pointer.FromPinnedBox(newHead));
    q.chain.head = newHead;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string attemptToDrainTooManyˢ = "attempt to drain too many elements"u8;

// tryDrain attempts to drain n spans from q's local queue to the chain.
//
// Returns whether it succeeded.
internal static bool tryDrain(this ref spanQueue q, ж<spanSPMC> Ꮡr, uint32 n) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (q.head + n > q.tail) {
        @throw(attemptToDrainTooManyˢ);
    }
    var h = Ꮡr.of(spanSPMC.Ꮡhead).Load(); // synchronize with consumers
    var t = Ꮡr.of(spanSPMC.Ꮡtail).Load();
    var rn = t - h;
    if (rn + n <= r.cap) {
        for (var i = (uint32)0; i < n; i++) {
            r.slot(t + i).Value = q.ring[(q.head + i) % (uint32)len(q.ring)];
        }
        Ꮡr.of(spanSPMC.Ꮡtail).Store(t + n); // Makes the items avail for consumption.
        q.head += n;
        return true;
    }
    return false;
}

// tryGetFast attempts to get a span from the local queue, but may fail if it's empty,
// returning false.
internal static objptr tryGetFast(this ref spanQueue q) {
    if (q.tail - q.head == 0) {
        return 0;
    }
    var s = q.ring[q.head % (uint32)len(q.ring)];
    q.head++;
    return s;
}

// steal takes some spans from the ring chain of another span queue.
//
// q == q2 is OK.
internal static objptr steal(this ref spanQueue q, ж<spanQueue> Ꮡq2) {
    var r = (ж<spanSPMC>)(uintptr)(Ꮡq2.of(spanQueue.Ꮡchain).of(spanQueue_chain.Ꮡtail).Load());
    if (r == nil) {
        return 0;
    }
    while (ᐧ) {
        // It's important that we load the next pointer
        // *before* popping the tail. In general, r may be
        // transiently empty, but if next is non-nil before
        // the pop and the pop fails, then r is permanently
        // empty, which is the only condition under which it's
        // safe to drop r from the chain.
        var r2 = (ж<spanSPMC>)(uintptr)(r.of(spanSPMC.Ꮡprev).Load());

        // Try to refill from one of the rings
        {
            var s = q.refill(r); if (s != 0) {
                return s;
            }
        }

        if (r2 == nil) {
            // This is the only ring. It's empty right
            // now, but could be pushed to in the future.
            return 0;
        }

        // The tail of the chain has been drained, so move on
        // to the next ring. Try to drop it from the chain
        // so the next consumer doesn't have to look at the empty
        // ring again.
        if (Ꮡq2.of(spanQueue.Ꮡchain).of(spanQueue_chain.Ꮡtail).CompareAndSwapNoWB(@unsafe.Pointer.FromPinnedBox(r), @unsafe.Pointer.FromPinnedBox(r2))) {
            r.of(spanSPMC.Ꮡdead).Store(true);
        }

        r = r2;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stealWithLocalWorkˢ = "steal with local work available"u8;

// refill takes some spans from r and puts them into q's local queue.
//
// One span is removed from the stolen spans and returned on success.
// Failure to steal returns a zero objptr.
//
// steal is thread-safe with respect to r.
internal static objptr refill(this ref spanQueue q, ж<spanSPMC> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    if (q.tail - q.head != 0) {
        @throw(stealWithLocalWorkˢ);
    }

    // Steal some spans.
    uint32 n = default!;
    while (ᐧ) {
        var h = Ꮡr.of(spanSPMC.Ꮡhead).Load(); // load-acquire, synchronize with other consumers
        var t = Ꮡr.of(spanSPMC.Ꮡtail).Load(); // load-acquire, synchronize with the producer
        n = t - h;
        n = n - n / 2;
        if (n == 0) {
            return 0;
        }
        if (n > r.cap) { // read inconsistent h and t
            continue;
        }
        n = min(n, (uint32)(len(q.ring) / 2));
        for (var i = (uint32)0; i < n; i++) {
            q.ring[i] = r.slot(h + i).Value;
        }
        if (Ꮡr.of(spanSPMC.Ꮡhead).CompareAndSwap(h, h + n)) {
            break;
        }
    }

    // Update local queue head and tail to reflect new buffered values.
    q.head = 0;
    q.tail = n;

    // Pop off the head of the queue and return it.
    return q.tryGetFast();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string spanQueueDestroyDuringˢ = "spanQueue.destroy during the mark phase"u8;
internal static readonly @string spanQueueDestroyOnNonˢ = "spanQueue.destroy on non-empty queue"u8;

// destroy frees all chains in an empty spanQueue.
//
// Preconditions:
// - World is stopped.
// - GC is outside of the mark phase.
// - (Therefore) the queue is empty.
internal static void destroy(this ж<spanQueue> Ꮡq) {
    ref var q = ref Ꮡq.DerefOrNull();

    assertWorldStopped();
    if (gcphase != _GCoff) {
        @throw(spanQueueDestroyDuringˢ);
    }
    if (!Ꮡq.empty()) {
        @throw(spanQueueDestroyOnNonˢ);
    }

    @lock(Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlock));

    // Remove, deinitialize, and free each ring.
    for (var r = (ж<spanSPMC>)(uintptr)(Ꮡq.of(spanQueue.Ꮡchain).of(spanQueue_chain.Ꮡtail).Load()); r != nil; r = (ж<spanSPMC>)(uintptr)(r.of(spanSPMC.Ꮡprev).Load())) {
        Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlist).remove(@unsafe.Pointer.FromPinnedBox(r));
        r.deinit();
        Ꮡmheap_.of(mheap.ᏑspanSPMCAlloc).free(@unsafe.Pointer.FromPinnedBox(r));
    }

    q.chain.head = default!;
    Ꮡq.of(spanQueue.Ꮡchain).of(spanQueue_chain.Ꮡtail).Store(nil);
    q.putsSinceDrain = 0;

    unlock(Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlock));
}

// spanSPMC is a ring buffer of objptrs that represent spans.
// Accessed without a lock.
//
// Single-producer, multi-consumer. The only producer is the P that owns this
// queue, but any other P may consume from it.
//
// ## Invariants for memory management
//
// 1. All spanSPMCs are allocated from mheap_.spanSPMCAlloc.
// 2. All allocated spanSPMCs must be on the work.spanSPMCs list.
// 3. spanSPMCs may only be allocated if gcphase != _GCoff.
// 4. spanSPMCs may only be deallocated if gcphase == _GCoff.
//
// Invariants (3) and (4) ensure that we do not need to concern ourselves with
// tricky reuse issues that stem from not knowing when a thread is truly done
// with a spanSPMC. For example, two threads could load the same spanSPMC from
// the tail of the chain. One thread is then paused while the other steals the
// last few elements off of it. It's not safe to free at that point since the
// other thread will still inspect that spanSPMC, and we have no way of knowing
// without more complex and/or heavyweight synchronization.
//
// Instead, we rely on the global synchronization inherent to GC phases, and
// the fact that spanSPMCs are only ever used during the mark phase, to ensure
// memory safety. This means we temporarily waste some memory, but it's only
// until the end of the mark phase.
partial struct spanSPMC {
    internal sys.NotInHeap _;
    // allnode is the linked list node for work.spanSPMCs list. This is
    // used to find and free dead spanSPMCs. Protected by
    // work.spanSPMCs.lock.
    internal listNodeManual allnode;
    // dead indicates whether the spanSPMC is no longer in use.
    // Protected by the CAS to the prev field of the spanSPMC pointing
    // to this spanSPMC. That is, whoever wins that CAS takes ownership
    // of marking this spanSPMC as dead. See spanQueue.steal for details.
    internal atomic.Bool dead;
    // prev is the next link up a spanQueue's SPMC chain, from tail to head,
    // hence the name "prev." Set by a spanQueue's producer, cleared by a
    // CAS in spanQueue.steal.
    internal atomic.UnsafePointer prev; // *spanSPMC
    // head, tail, cap, and ring together represent a fixed-size SPMC lock-free
    // ring buffer of size cap. The ring buffer contains objptr values.
    internal atomic.Uint32 head;
    internal atomic.Uint32 tail;
    internal uint32 cap; // cap(ring))
    internal ж<objptr> ring;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string spmcCapacityMustBeAPowerˢ = "spmc capacity must be a power of 2"u8;
internal static readonly @string gcSpanQueueˢ = "GC span queue"u8;

// newSpanSPMC allocates and initializes a new spmc with the provided capacity.
//
// newSpanSPMC may override the capacity with a larger one if the provided one would
// waste memory.
internal static ж<spanSPMC> newSpanSPMC(uint32 cap) {
    @lock(Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlock));
    var r = (ж<spanSPMC>)(uintptr)(Ꮡmheap_.of(mheap.ᏑspanSPMCAlloc).alloc());
    Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlist).push(@unsafe.Pointer.FromPinnedBox(r));
    unlock(Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlock));

    // If cap < the capacity of a single physical page, round up.
    var pageCap = (uint32)(physPageSize / (uintptr)goarch.PtrSize); // capacity of a single page
    if (cap < pageCap) {
        cap = pageCap;
    }
    if ((cap & (cap - 1)) != 0) {
        @throw(spmcCapacityMustBeAPowerˢ);
    }

    r.cap = cap;
    @unsafe.Pointer ring = (uintptr)sysAlloc((uintptr)cap * /* unsafe.Sizeof(objptr(0)) */ (uintptr)8, Ꮡmemstats.of(mstats.ᏑgcMiscSys), gcSpanQueueˢ);
    atomic.StorepNoWB(@unsafe.Pointer.FromBox(r.of(spanSPMC.Ꮡring)), ring);
    return r;
}

// empty returns true if the spmc is empty.
//
// empty is thread-safe.
internal static bool empty(this ж<spanSPMC> Ꮡr) {
    var h = Ꮡr.of(spanSPMC.Ꮡhead).Load();
    var t = Ꮡr.of(spanSPMC.Ꮡtail).Load();
    return t == h;
}

// deinit frees any resources the spanSPMC is holding onto and zeroes it.
internal static void deinit(this ж<spanSPMC> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    sysFree(@unsafe.Pointer.FromBox(r.ring), (uintptr)r.cap * /* unsafe.Sizeof(objptr(0)) */ (uintptr)8, Ꮡmemstats.of(mstats.ᏑgcMiscSys));
    r.ring = default!;
    Ꮡr.of(spanSPMC.Ꮡdead).Store(false);
    Ꮡr.of(spanSPMC.Ꮡprev).StoreNoWB(nil);
    Ꮡr.of(spanSPMC.Ꮡhead).Store(0);
    Ꮡr.of(spanSPMC.Ꮡtail).Store(0);
    r.cap = 0;
    r.allnode = new listNodeManual(nil);
}

// slot returns a pointer to slot i%r.cap.
internal static ж<objptr> slot(this ref spanSPMC r, uint32 i) {
    var idx = (uintptr)(i & (r.cap - 1));
    return (ж<objptr>)(uintptr)((uintptr)@unsafe.Add(@unsafe.Pointer.FromBox(r.ring), idx * /* unsafe.Sizeof(objptr(0)) */ (uintptr)8));
}

// freeDeadSpanSPMCs frees dead spanSPMCs back to the OS.
internal static void freeDeadSpanSPMCs() {
    // According to the SPMC memory management invariants, we can only free
    // spanSPMCs outside of the mark phase. We ensure we do this in two ways.
    //
    // 1. We take the work.spanSPMCs lock, which we need anyway. This ensures
    //    that we are non-preemptible. If this path becomes lock-free, we will
    //    need to become non-preemptible in some other way.
    // 2. Once we are non-preemptible, we check the gcphase, and back out if
    //    it's not safe.
    //
    // This way, we ensure that we don't start freeing if we're in the wrong
    // phase, and the phase can't change on us while we're freeing.
    //
    // TODO(go.dev/issue/75771): Due to the grow semantics in
    // spanQueue.drain, we expect a steady-state of around one spanSPMC per
    // P, with some spikes higher when Ps have more than one. For high
    // GOMAXPROCS, or if this list otherwise gets long, it would be nice to
    // have a way to batch work that allows preemption during processing.
    @lock(Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlock));
    if (gcphase != _GCoff || Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlist).empty()) {
        unlock(Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlock));
        return;
    }
    var r = (ж<spanSPMC>)(uintptr)(Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlist).head());
    while (r != nil) {
        var next = (ж<spanSPMC>)(uintptr)((@unsafe.Pointer)(~r).allnode.next);
        if (r.of(spanSPMC.Ꮡdead).Load()) {
            // It's dead. Remove, deinitialize and free it.
            Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlist).remove(@unsafe.Pointer.FromPinnedBox(r));
            r.deinit();
            Ꮡmheap_.of(mheap.ᏑspanSPMCAlloc).free(@unsafe.Pointer.FromPinnedBox(r));
        }
        r = next;
    }
    unlock(Ꮡwork.of(workType.ᏑspanSPMCs).of(workType_spanSPMCs.Ꮡlock));
}

// tryStealSpan attempts to steal a span from another P's local queue.
//
// Returns a non-zero objptr on success.
internal static objptr tryStealSpan(this ref gcWork w) {
    var pp = (~(~getg()).m).p.ptr();

    for (var @enum = stealOrder.start(cheaprand()); !@enum.done(); @enum.next()) {
        if (!work.spanqMask.read(@enum.position())) {
            continue;
        }
        var p2 = allp[@enum.position()];
        if (pp == p2) {
            continue;
        }
        {
            var s = w.spanq.steal(p2.of(runtime_package.Δp.Ꮡgcw).of(gcWork.Ꮡspanq)); if (s != 0) {
                return s;
            }
        }
        // N.B. This is intentionally racy. We may stomp on a mask set by
        // a P that just put a bunch of work into its local queue.
        //
        // This is OK because the ragged barrier in gcMarkDone will set
        // the bit on each P if there's local work we missed. This race
        // should generally be rare, since the window between noticing
        // an empty local queue and this bit being set is quite small.
        work.spanqMask.clear((int32)@enum.position());
    }
    return 0;
}

// objptr consists of a span base and the index of the object in the span.
partial struct objptr /*num:uintptr*/;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string createdObjptrWithAddressˢ = "created objptr with address that is incorrectly aligned"u8;

// makeObjPtr creates an objptr from a span base address and an object index.
internal static objptr makeObjPtr(uintptr spanBase, uint16 objIndex) {
    if (doubleCheckGreenTea && (uintptr)(spanBase & (uintptr)((1 << (int)(gc.PageShift)) - 1)) != 0) {
        @throw(createdObjptrWithAddressˢ);
    }
    return (objptr)((uintptr)(spanBase | (uintptr)objIndex));
}

internal static uintptr spanBase(this objptr Δp) {
    return (uintptr)((uintptr)Δp & ~(uintptr)((1 << (int)(gc.PageShift)) - 1));
}

internal static uint16 objIndex(this objptr Δp) {
    return (uint16)((uint16)(uintptr)Δp & (uint16)(((1 << (int)(gc.PageShift)) - 1)));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noscanObjectInScanSpanˢ = "noscan object in scanSpan"u8;

// scanSpan scans objects indicated marks&^scans and then scans those objects,
// queuing the resulting pointers into gcw.
internal static void scanSpan(objptr Δp, ж<gcWork> Ꮡgcw) {
    ref var gcw = ref Ꮡgcw.DerefOrNull();

    var spanBase = Δp.spanBase();
    var imb = spanInlineMarkBitsFromBase(spanBase);
    var spanclass = imb.Value.@class;
    if (spanclass.noscan()) {
        @throw(noscanObjectInScanSpanˢ);
    }
    var elemsize = (uintptr)gc.SizeClassToSize[spanclass.sizeclass()];

    // Release span.
    if (imb.release() == spanScanOneMark) {
        // Nobody else set any mark bits on this span while it was acquired.
        // That means p is the sole object we need to handle. Fast-track it.
        var objIndex = Δp.objIndex();
        var bytep = imb.at(spanInlineMarkBits.Ꮡscans, (nint)(objIndex / 8));
        var mask = (uint8)((uint8)1 << (int)((objIndex % 8)));
        if ((uint8)(atomic.Load8(bytep) & mask) != 0) {
            return;
        }
        atomic.Or8(bytep, mask);
        gcw.bytesMarked += (uint64)elemsize;
        if (debug.gctrace > 1) {
            gcw.stats[spanclass.sizeclass()].sparseObjsScanned++;
        }
        var b = spanBase + (uintptr)objIndex * elemsize;
        scanObjectSmall(spanBase, b, elemsize, Ꮡgcw);
        return;
    }

    // Compute nelems.
    var divMagic = (uint64)gc.SizeClassToDivMagic[spanclass.sizeclass()];
    var usableSpanSize = (uint64)((uintptr)gc.PageSize - /* unsafe.Sizeof(spanInlineMarkBits{}) */ (uintptr)128);
    if (!spanclass.noscan()) {
        usableSpanSize -= gc.PageSize / goarch.PtrSize / 8;
    }
    var nelems = (uint16)((usableSpanSize * divMagic) >> 32);

    // Grey objects and return if there's nothing else to do.
    ref var toScan = ref heap(new gc.ObjMask(), out var ᏑtoScan);
    nint objsMarked = spanSetScans(spanBase, nelems, imb, ref toScan);
    if (objsMarked == 0) {
        return;
    }
    gcw.bytesMarked += (uint64)objsMarked * (uint64)elemsize;

    // Check if we have enough density to make a dartboard scan
    // worthwhile. If not, just do what scanobject does, but
    // localized to the span, using the dartboard.
    if (!scan.HasFastScanSpanPacked() || objsMarked < (nint)(nelems / 8)) {
        if (debug.gctrace > 1) {
            gcw.stats[spanclass.sizeclass()].spansSparseScanned++;
            gcw.stats[spanclass.sizeclass()].spanObjsSparseScanned += (uint64)objsMarked;
        }
        scanObjectsSmall(spanBase, elemsize, nelems, Ꮡgcw, ref toScan);
        return;
    }

    // Scan the span.
    //
    // N.B. Use gcw.ptrBuf as the output buffer. This is a bit different
    // from scanObjectsSmall, which puts addresses to dereference. ScanSpanPacked
    // on the other hand, fills gcw.ptrBuf with already dereferenced pointers.
    var nptrs = scan.ScanSpanPacked(
        (@unsafe.Pointer)spanBase,
        gcw.ptrBuf.at<uintptr>(0),
        ᏑtoScan,
        (uintptr)spanclass.sizeclass(),
        spanPtrMaskUnsafe(spanBase));
    gcw.heapScanWork += (int64)objsMarked * (int64)elemsize;

    if (debug.gctrace > 1) {
        // Write down some statistics.
        gcw.stats[spanclass.sizeclass()].spansDenseScanned++;
        gcw.stats[spanclass.sizeclass()].spanObjsDenseScanned += (uint64)objsMarked;
    }

    // Process all the pointers we just got.
    foreach (var (_, pΔ1) in gcw.ptrBuf.slice(0, nptrs)) {
        if (!tryDeferToSpanScan(pΔ1, Ꮡgcw)) {
            {
                var (obj, span, objIndex) = findObject(pΔ1, 0, 0); if (obj != 0) {
                    greyobject(obj, 0, 0, span, Ꮡgcw, objIndex);
                }
            }
        }
    }
}

// spanSetScans sets any unset mark bits that have their mark bits set in the inline mark bits.
//
// toScan is populated with bits indicating whether a particular mark bit was set.
//
// Returns the number of objects marked, which could be zero.
internal static nint spanSetScans(uintptr spanBase, uint16 nelems, ж<spanInlineMarkBits> Ꮡimb, ref gc.ObjMask toScan) {
    ref var imb = ref Ꮡimb.DerefOrNull();

    var (arena, pageIdx, pageMask) = pageIndexOf(spanBase);
    if ((uint8)((~arena).pageMarks[pageIdx] & pageMask) == 0) {
        atomic.Or8(arena.at(heapArena.ᏑpageMarks, (ulong)(pageIdx)), pageMask);
    }

    var bytes = divRoundUp((uintptr)nelems, 8);
    nint objsMarked = 0;

    // Careful: these two structures alias since ObjMask is much bigger
    // than marks or scans. We do these unsafe shenanigans so that we can
    // access the marks and scans by uintptrs rather than by byte.
    var imbMarks = (ж<gc.ObjMask>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡimb.of(spanInlineMarkBits.Ꮡmarks)));
    var imbScans = (ж<gc.ObjMask>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡimb.of(spanInlineMarkBits.Ꮡscans)));

    // Iterate over one uintptr-sized chunks at a time, computing both
    // the union and intersection of marks and scans. Store the union
    // into scans, and the intersection into toScan.
    for (var i = (uintptr)0; i < bytes; i += goarch.PtrSize) {
        var scans = atomic.Loaduintptr(imbScans.at<uintptr>((ulong)(i / (uintptr)goarch.PtrSize)));
        var marks = imbMarks.Value[i / (uintptr)goarch.PtrSize];
        scans = bswapIfBigEndian(scans);
        marks = bswapIfBigEndian(marks);
        if (i / (uintptr)goarch.PtrSize == (uintptr)((uintptr)((uintptr)(len(imb.marks) + 1) / (uintptr)goarch.PtrSize) - 1)) {
            scans &= unchecked((uintptr)~(uintptr)(((uintptr)0xff << (int)(((goarch.PtrSize - 1) * 8))))); // mask out owned
            marks &= unchecked((uintptr)~(uintptr)(((uintptr)0xff << (int)(((goarch.PtrSize - 1) * 8))))); // mask out class
        }
        var toGrey = (uintptr)(marks & ~scans);
        toScan[i / (uintptr)goarch.PtrSize] = toGrey;

        // If there's anything left to grey, do it.
        if (toGrey != 0) {
            toGrey = bswapIfBigEndian(toGrey);
            if (goarch.PtrSize == 4){
                atomic.Or32(imbScans.at<uintptr>((ulong)(i / (uintptr)goarch.PtrSize)).Reinterpret<uintptr, uint32>(), (uint32)toGrey);
            } else {
                atomic.Or64(imbScans.at<uintptr>((ulong)(i / (uintptr)goarch.PtrSize)).Reinterpret<uintptr, uint64>(), (uint64)toGrey);
            }
        }
        objsMarked += sys.OnesCount64((uint64)toGrey);
    }
    return objsMarked;
}

internal static void scanObjectSmall(uintptr spanBase, uintptr b, uintptr objSize, ж<gcWork> Ꮡgcw) {
    ref var gcw = ref Ꮡgcw.DerefOrNull();

    var (hbitsBase, _) = spanHeapBitsRange(spanBase, gc.PageSize, objSize);
    var hbits = (ж<byte>)(uintptr)((@unsafe.Pointer)hbitsBase);
    var ptrBits = extractHeapBitsSmall(hbits, spanBase, b, objSize);
    gcw.heapScanWork += (int64)(sys.Len64((uint64)ptrBits) * (nint)goarch.PtrSize);
    nint nptrs = 0;
    nint n = sys.OnesCount64((uint64)ptrBits);
    foreach (var _ᴛ1 in range(n)) {
        nint k = sys.TrailingZeros64((uint64)ptrBits);
        ptrBits &= unchecked((uintptr)~(uintptr)(((uintptr)1).Lsh((int64)(k))));
        var addr = b + (uintptr)k * (uintptr)goarch.PtrSize;

        // Prefetch addr since we're about to use it. This point for prefetching
        // was chosen empirically.
        sys.Prefetch(addr);

        // N.B. ptrBuf is always large enough to hold pointers for an entire 1-page span.
        gcw.ptrBuf.Value[nptrs] = addr;
        nptrs++;
    }

    // Process all the pointers we just got.
    foreach (var (_, vᴛ1) in gcw.ptrBuf.slice(0, nptrs)) {
        var Δp = vᴛ1;

        Δp = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)Δp);
        if (Δp == 0) {
            continue;
        }
        if (!tryDeferToSpanScan(Δp, Ꮡgcw)) {
            {
                var (obj, span, objIndex) = findObject(Δp, 0, 0); if (obj != 0) {
                    greyobject(obj, 0, 0, span, Ꮡgcw, objIndex);
                }
            }
        }
    }
}

internal static void scanObjectsSmall(uintptr @base, uintptr objSize, uint16 elems, ж<gcWork> Ꮡgcw, ref gc.ObjMask scans) {
    ref var gcw = ref Ꮡgcw.DerefOrNull();

    nint nptrs = 0;
    foreach (var (i, vᴛ1) in scans) {
        var bits = vᴛ1;

        if (i * (nint)(goarch.PtrSize * 8) > (nint)elems) {
            break;
        }
        nint n = sys.OnesCount64((uint64)bits);
        var (hbitsBase, _) = spanHeapBitsRange(@base, gc.PageSize, objSize);
        var hbits = (ж<byte>)(uintptr)((@unsafe.Pointer)hbitsBase);
        foreach (var _ᴛ1 in range(n)) {
            nint j = sys.TrailingZeros64((uint64)bits);
            bits &= unchecked((uintptr)~(uintptr)(((uintptr)1).Lsh((int64)(j))));

            var b = @base + (uintptr)(i * (nint)(goarch.PtrSize * 8) + j) * objSize;
            var ptrBits = extractHeapBitsSmall(hbits, @base, b, objSize);
            gcw.heapScanWork += (int64)(sys.Len64((uint64)ptrBits) * (nint)goarch.PtrSize);

            nint nΔ1 = sys.OnesCount64((uint64)ptrBits);
            foreach (var _ᴛ2 in range(nΔ1)) {
                nint k = sys.TrailingZeros64((uint64)ptrBits);
                ptrBits &= unchecked((uintptr)~(uintptr)(((uintptr)1).Lsh((int64)(k))));
                var addr = b + (uintptr)k * (uintptr)goarch.PtrSize;

                // Prefetch addr since we're about to use it. This point for prefetching
                // was chosen empirically.
                sys.Prefetch(addr);

                // N.B. ptrBuf is always large enough to hold pointers for an entire 1-page span.
                gcw.ptrBuf.Value[nptrs] = addr;
                nptrs++;
            }
        }
    }

    // Process all the pointers we just got.
    foreach (var (_, vᴛ2) in gcw.ptrBuf.slice(0, nptrs)) {
        var Δp = vᴛ2;

        Δp = ~(ж<uintptr>)(uintptr)((@unsafe.Pointer)Δp);
        if (Δp == 0) {
            continue;
        }
        if (!tryDeferToSpanScan(Δp, Ꮡgcw)) {
            {
                var (obj, span, objIndex) = findObject(Δp, 0, 0); if (obj != 0) {
                    greyobject(obj, 0, 0, span, Ꮡgcw, objIndex);
                }
            }
        }
    }
}

internal static uintptr extractHeapBitsSmall(ж<byte> Ꮡhbits, uintptr spanBase, uintptr addr, uintptr elemsize) {
    // These objects are always small enough that their bitmaps
    // fit in a single word, so just load the word or two we need.
    //
    // Mirrors mspan.writeHeapBitsSmall.
    //
    // We should be using heapBits(), but unfortunately it introduces
    // both bounds checks panics and throw which causes us to exceed
    // the nosplit limit in quite a few cases.
    var i = (addr - spanBase) / (uintptr)goarch.PtrSize / (uintptr)ptrBits;
    var j = (addr - spanBase) / (uintptr)goarch.PtrSize % (uintptr)ptrBits;
    var bits = elemsize / (uintptr)goarch.PtrSize;
    var word0 = addb(Ꮡhbits, (uintptr)goarch.PtrSize * (i + 0)).Reinterpret<byte, uintptr>();
    var word1 = addb(Ꮡhbits, (uintptr)goarch.PtrSize * (i + 1)).Reinterpret<byte, uintptr>();

    uintptr read = default!;
    if (j + bits > ptrBits){
        // Two reads.
        var bits0 = (uintptr)ptrBits - j;
        var bits1 = bits - bits0;
        read = (word0.Value).Rsh((uint64)(j));
        read |= (uintptr)(((uintptr)(word1.Value & ((((uintptr)1).Lsh((uint64)(bits1))) - 1))).Lsh((uint64)(bits0)));
    } else {
        // One read.
        read = (uintptr)(((word0.Value).Rsh((uint64)(j))) & ((((uintptr)1).Lsh((uint64)(bits))) - 1));
    }
    return read;
}

// spanPtrMaskUnsafe returns the pointer mask for a span with inline mark bits.
//
// The caller must ensure spanBase is the base of a span that:
// - 1 page in size,
// - Uses inline mark bits,
// - Contains pointers.
internal static ж<gc.PtrMask> spanPtrMaskUnsafe(uintptr spanBase) {
    var @base = spanBase + (uintptr)gc.PageSize - /* unsafe.Sizeof(gc.PtrMask{}) */ (uintptr)128 - /* unsafe.Sizeof(spanInlineMarkBits{}) */ (uintptr)128;
    return (ж<gc.PtrMask>)(uintptr)((@unsafe.Pointer)@base);
}

partial struct sizeClassScanStats {
    internal uint64 spansDenseScanned; // Spans scanned with ScanSpanPacked.
    internal uint64 spanObjsDenseScanned; // Objects scanned with ScanSpanPacked.
    internal uint64 spansSparseScanned; // Spans scanned with scanObjectsSmall.
    internal uint64 spanObjsSparseScanned; // Objects scanned with scanObjectsSmall.
    internal uint64 sparseObjsScanned; // Objects scanned with scanobject or scanObjectSmall.
}

// Note: sparseObjsScanned is sufficient for both cases because
// a particular size class either uses scanobject or scanObjectSmall,
// not both. In the latter case, we also know that there was one
// object scanned per span, so no need for a span counter.
internal static void dumpScanStats() {
    uint64 spansDenseScanned = default!;
    uint64 spanObjsDenseScanned = default!;
    uint64 spansSparseScanned = default!;
    uint64 spanObjsSparseScanned = default!;
    uint64 sparseObjsScanned = default!;
    foreach (var (_, stats) in memstats.lastScanStats.ΔRangeSnapshot()) {
        spansDenseScanned += stats.spansDenseScanned;
        spanObjsDenseScanned += stats.spanObjsDenseScanned;
        spansSparseScanned += stats.spansSparseScanned;
        spanObjsSparseScanned += stats.spanObjsSparseScanned;
        sparseObjsScanned += stats.sparseObjsScanned;
    }
    var totalObjs = sparseObjsScanned + spanObjsSparseScanned + spanObjsDenseScanned;
    var totalSpans = spansSparseScanned + spansDenseScanned;
    print((@string)"scan: total "u8, sparseObjsScanned, (@string)"+"u8, spanObjsSparseScanned, (@string)"+"u8, spanObjsDenseScanned, (@string)"="u8, totalObjs, (@string)" objs"u8);
    print((@string)", "u8, spansSparseScanned, (@string)"+"u8, spansDenseScanned, (@string)"="u8, totalSpans, (@string)" spans\n"u8);
    foreach (var (i, stats) in memstats.lastScanStats.ΔRangeSnapshot()) {
        if (stats == (new sizeClassScanStats(nil))) {
            continue;
        }
        var totalObjsΔ1 = stats.sparseObjsScanned + stats.spanObjsSparseScanned + stats.spanObjsDenseScanned;
        var totalSpansΔ1 = stats.spansSparseScanned + stats.spansDenseScanned;
        if (i == 0){
            print((@string)"scan: class L "u8);
        } else {
            print((@string)"scan: class "u8, gc.SizeClassToSize[i], (@string)"B "u8);
        }
        print(stats.sparseObjsScanned, (@string)"+"u8, stats.spanObjsSparseScanned, (@string)"+"u8, stats.spanObjsDenseScanned, (@string)"="u8, totalObjsΔ1, (@string)" objs"u8);
        print((@string)", "u8, stats.spansSparseScanned, (@string)"+"u8, stats.spansDenseScanned, (@string)"="u8, totalSpansΔ1, (@string)" spans\n"u8);
    }
}

internal static void flushScanStats(this ref gcWork w, /*[68]*/ ж<array<sizeClassScanStats>> Ꮡdst) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    foreach (var (i, _) in w.stats) {
        dst[i].spansDenseScanned += w.stats[i].spansDenseScanned;
        dst[i].spanObjsDenseScanned += w.stats[i].spanObjsDenseScanned;
        dst[i].spansSparseScanned += w.stats[i].spansSparseScanned;
        dst[i].spanObjsSparseScanned += w.stats[i].spanObjsSparseScanned;
        dst[i].sparseObjsScanned += w.stats[i].sparseObjsScanned;
    }
    builtin.clear(w.stats[..]);
}

// gcMarkWorkAvailable reports whether there's any non-local work available to do.
//
// This is a heavyweight check and must only be used for correctness, not
// as a hint.
internal static bool gcMarkWorkAvailable() {
    if (!Ꮡwork.of(workType.Ꮡfull).empty()) {
        return true; // global work available
    }
    if (Ꮡwork.of(workType.ᏑmarkrootNext).Load() < Ꮡwork.of(workType.ᏑmarkrootJobs).Load()) {
        return true; // root scan work available
    }
    if (work.spanqMask.any()) {
        return true; // stealable local work available
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string scanObjectN0ˢ = "scanObject n == 0"u8;
internal static readonly @string scanObjectOfANoscanˢ = "scanObject of a noscan object"u8;

// scanObject scans the object starting at b, adding pointers to gcw.
// b must point to the beginning of a heap object or an oblet.
// scanObject consults the GC bitmap for the pointer mask and the
// spans for the size of the object.
//
// Used only for !gcUsesSpanInlineMarkBits spans, but supports all
// object sizes and is safe to be called on all heap objects.
//
//go:nowritebarrier
internal static void scanObject(uintptr b, ж<gcWork> Ꮡgcw) {
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
        @throw(scanObjectN0ˢ);
    }
    if ((~s).spanclass.noscan()) {
        // Correctness-wise this is ok, but it's inefficient
        // if noscan objects reach here.
        @throw(scanObjectOfANoscanˢ);
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
                if (!gcw.putObjFast(oblet)) {
                    gcw.putObj(oblet);
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
            if (!tryDeferToSpanScan(obj, Ꮡgcw)) {
                {
                    var (objΔ1, span, objIndex) = findObject(obj, b, addr - b); if (objΔ1 != 0) {
                        greyobject(objΔ1, b, addr - b, span, Ꮡgcw, objIndex);
                    }
                }
            }
        }
    }
    gcw.bytesMarked += (uint64)n;
    gcw.heapScanWork += (int64)scanSize;
    if (debug.gctrace > 1) {
        gcw.stats[(~s).spanclass.sizeclass()].sparseObjsScanned++;
    }
}

} // end runtime_package
