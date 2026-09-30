// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

internal static UntypedInt _WorkbufSize => 2048; // in bytes; larger values result in less contention
internal static UntypedInt workbufAlloc => /* 32 << 10 */ 32768;

/* [GoInit] runtime bootstrap init - not run; .NET is the runtime */ internal static void initΔ3() {
    if (workbufAlloc % pageSize != 0 || workbufAlloc % _WorkbufSize != 0) {
        @throw("bad workbufAlloc"u8);
    }
}

// Garbage collector work pool abstraction.
//
// This implements a producer/consumer model for pointers to grey
// objects. A grey object is one that is marked and on a work
// queue. A black object is marked and not on a work queue.
//
// Write barriers, root discovery, stack scanning, and object scanning
// produce pointers to grey objects. Scanning consumes pointers to
// grey objects, thus blackening them, and then scans them,
// potentially producing new pointers to grey objects.

// A gcWork provides the interface to produce and consume work for the
// garbage collector.
//
// A gcWork can be used on the stack as follows:
//
//	(preemption must be disabled)
//	gcw := &getg().m.p.ptr().gcw
//	.. call gcw.put() to produce and gcw.tryGet() to consume ..
//
// It's important that any use of gcWork during the mark phase prevent
// the garbage collector from transitioning to mark termination since
// gcWork may locally hold GC work buffers. This can be done by
// disabling preemption (systemstack or acquirem).
[GoType] partial struct gcWork {
    // wbuf1 and wbuf2 are the primary and secondary work buffers.
    //
    // This can be thought of as a stack of both work buffers'
    // pointers concatenated. When we pop the last pointer, we
    // shift the stack up by one work buffer by bringing in a new
    // full buffer and discarding an empty one. When we fill both
    // buffers, we shift the stack down by one work buffer by
    // bringing in a new empty buffer and discarding a full one.
    // This way we have one buffer's worth of hysteresis, which
    // amortizes the cost of getting or putting a work buffer over
    // at least one buffer of work and reduces contention on the
    // global work lists.
    //
    // wbuf1 is always the buffer we're currently pushing to and
    // popping from and wbuf2 is the buffer that will be discarded
    // next.
    //
    // Invariant: Both wbuf1 and wbuf2 are nil or neither are.
    internal ж<workbuf> wbuf1, wbuf2;
    // Bytes marked (blackened) on this gcWork. This is aggregated
    // into work.bytesMarked by dispose.
    internal uint64 bytesMarked;
    // Heap scan work performed on this gcWork. This is aggregated into
    // gcController by dispose and may also be flushed by callers.
    // Other types of scan work are flushed immediately.
    internal int64 heapScanWork;
    // flushedWork indicates that a non-empty work buffer was
    // flushed to the global work list since the last gcMarkDone
    // termination check. Specifically, this indicates that this
    // gcWork may have communicated work to another gcWork.
    internal bool flushedWork;
}

// Most of the methods of gcWork are go:nowritebarrierrec because the
// write barrier itself can invoke gcWork methods but the methods are
// not generally re-entrant. Hence, if a gcWork method invoked the
// write barrier while the gcWork was in an inconsistent state, and
// the write barrier in turn invoked a gcWork method, it could
// permanently corrupt the gcWork.
[GoRecv] internal static void init(this ref gcWork w) {
    w.wbuf1 = getempty();
    var wbuf2 = trygetfull();
    if (wbuf2 == nil) {
        wbuf2 = getempty();
    }
    w.wbuf2 = wbuf2;
}

// put enqueues a pointer for the garbage collector to trace.
// obj must point to the beginning of a heap object or an oblet.
//
//go:nowritebarrierrec
[GoRecv] internal static void put(this ref gcWork w, uintptr obj) {
    var flushed = false;
    var wbuf = w.wbuf1;
    // Record that this may acquire the wbufSpans or heap lock to
    // allocate a workbuf.
    lockWithRankMayAcquire(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock), lockRankWbufSpans);
    lockWithRankMayAcquire(Ꮡmheap_.of(mheap.Ꮡlock), lockRankMheap);
    if (wbuf == nil){
        w.init();
        wbuf = w.wbuf1;
    } else 
    if ((~wbuf).nobj == len((~wbuf).obj)) {
        // wbuf is empty at this point.
        (w.wbuf1, w.wbuf2) = (w.wbuf2, w.wbuf1);
        wbuf = w.wbuf1;
        if ((~wbuf).nobj == len((~wbuf).obj)) {
            putfull(wbuf);
            w.flushedWork = true;
            wbuf = getempty();
            w.wbuf1 = wbuf;
            flushed = true;
        }
    }
    wbuf.Value.obj[(~wbuf).nobj] = obj;
    wbuf.Value.nobj++;
    // If we put a buffer on full, let the GC controller know so
    // it can encourage more workers to run. We delay this until
    // the end of put so that w is in a consistent state, since
    // enlistWorker may itself manipulate w.
    if (flushed && gcphase == _GCmark) {
        ᏑgcController.enlistWorker();
    }
}

// putFast does a put and reports whether it can be done quickly
// otherwise it returns false and the caller needs to call put.
//
//go:nowritebarrierrec
[GoRecv] internal static bool putFast(this ref gcWork w, uintptr obj) {
    var wbuf = w.wbuf1;
    if (wbuf == nil || (~wbuf).nobj == len((~wbuf).obj)) {
        return false;
    }
    wbuf.Value.obj[(~wbuf).nobj] = obj;
    wbuf.Value.nobj++;
    return true;
}

// putBatch performs a put on every pointer in obj. See put for
// constraints on these pointers.
//
//go:nowritebarrierrec
[GoRecv] internal static void putBatch(this ref gcWork w, slice<uintptr> obj) {
    if (len(obj) == 0) {
        return;
    }
    var flushed = false;
    var wbuf = w.wbuf1;
    if (wbuf == nil) {
        w.init();
        wbuf = w.wbuf1;
    }
    while (len(obj) > 0) {
        while ((~wbuf).nobj == len((~wbuf).obj)) {
            putfull(wbuf);
            w.flushedWork = true;
            (w.wbuf1, w.wbuf2) = (w.wbuf2, getempty());
            wbuf = w.wbuf1;
            flushed = true;
        }
        nint n = copy((~wbuf).obj.slice((~wbuf).nobj), obj);
        wbuf.Value.nobj += n;
        obj = obj.slice(n);
    }
    if (flushed && gcphase == _GCmark) {
        ᏑgcController.enlistWorker();
    }
}

// tryGet dequeues a pointer for the garbage collector to trace.
//
// If there are no pointers remaining in this gcWork or in the global
// queue, tryGet returns 0.  Note that there may still be pointers in
// other gcWork instances or other caches.
//
//go:nowritebarrierrec
[GoRecv] internal static uintptr tryGet(this ref gcWork w) {
    var wbuf = w.wbuf1;
    if (wbuf == nil) {
        w.init();
        wbuf = w.wbuf1;
    }
    // wbuf is empty at this point.
    if ((~wbuf).nobj == 0) {
        (w.wbuf1, w.wbuf2) = (w.wbuf2, w.wbuf1);
        wbuf = w.wbuf1;
        if ((~wbuf).nobj == 0) {
            var owbuf = wbuf;
            wbuf = trygetfull();
            if (wbuf == nil) {
                return 0;
            }
            putempty(owbuf);
            w.wbuf1 = wbuf;
        }
    }
    wbuf.Value.nobj--;
    return (~wbuf).obj[(~wbuf).nobj];
}

// tryGetFast dequeues a pointer for the garbage collector to trace
// if one is readily available. Otherwise it returns 0 and
// the caller is expected to call tryGet().
//
//go:nowritebarrierrec
[GoRecv] internal static uintptr tryGetFast(this ref gcWork w) {
    var wbuf = w.wbuf1;
    if (wbuf == nil || (~wbuf).nobj == 0) {
        return 0;
    }
    wbuf.Value.nobj--;
    return (~wbuf).obj[(~wbuf).nobj];
}

// dispose returns any cached pointers to the global queue.
// The buffers are being put on the full queue so that the
// write barriers will not simply reacquire them before the
// GC can inspect them. This helps reduce the mutator's
// ability to hide pointers during the concurrent mark phase.
//
//go:nowritebarrierrec
[GoRecv] internal static void dispose(this ref gcWork w) {
    {
        var wbuf = w.wbuf1; if (wbuf != nil) {
            if ((~wbuf).nobj == 0){
                putempty(wbuf);
            } else {
                putfull(wbuf);
                w.flushedWork = true;
            }
            w.wbuf1 = default!;
            wbuf = w.wbuf2;
            if ((~wbuf).nobj == 0){
                putempty(wbuf);
            } else {
                putfull(wbuf);
                w.flushedWork = true;
            }
            w.wbuf2 = default!;
        }
    }
    if (w.bytesMarked != 0) {
        // dispose happens relatively infrequently. If this
        // atomic becomes a problem, we should first try to
        // dispose less and if necessary aggregate in a per-P
        // counter.
        atomic.Xadd64(Ꮡwork.of(workType.ᏑbytesMarked), (int64)w.bytesMarked);
        w.bytesMarked = 0;
    }
    if (w.heapScanWork != 0) {
        ᏑgcController.of(gcControllerState.ᏑheapScanWork).Add(w.heapScanWork);
        w.heapScanWork = 0;
    }
}

// balance moves some work that's cached in this gcWork back on the
// global queue.
//
//go:nowritebarrierrec
[GoRecv] internal static void balance(this ref gcWork w) {
    if (w.wbuf1 == nil) {
        return;
    }
    {
        var wbuf = w.wbuf2; if ((~wbuf).nobj != 0){
            putfull(wbuf);
            w.flushedWork = true;
            w.wbuf2 = getempty();
        } else 
        {
            var wbufΔ1 = w.wbuf1; if ((~wbufΔ1).nobj > 4){
                w.wbuf1 = handoff(wbufΔ1);
                w.flushedWork = true; // handoff did putfull
            } else {
                return;
            }
        }
    }
    // We flushed a buffer to the full list, so wake a worker.
    if (gcphase == _GCmark) {
        ᏑgcController.enlistWorker();
    }
}

// empty reports whether w has no mark work available.
//
//go:nowritebarrierrec
[GoRecv] internal static bool empty(this ref gcWork w) {
    return w.wbuf1 == nil || ((~w.wbuf1).nobj == 0 && (~w.wbuf2).nobj == 0);
}

// Internally, the GC work pool is kept in arrays in work buffers.
// The gcWork interface caches a work buffer until full (or empty) to
// avoid contending on the global work buffer lists.
[GoType] partial struct workbufhdr {
    internal lfnode node; // must be first
    internal nint nobj;
}

[GoType] partial struct workbuf {
    internal sys.NotInHeap _;
    internal partial ref workbufhdr workbufhdr { get; }
    // account for the above fields
    internal array<uintptr> obj = new((uintptr)((uintptr)_WorkbufSize - /* unsafe.Sizeof(workbufhdr{}) */ (uintptr)24) / goarch.PtrSize);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string workbufIsEmptyˢ = "workbuf is empty"u8;

// workbuf factory routines. These funcs are used to manage the
// workbufs.
// If the GC asks for some work these are the only routines that
// make wbufs available to the GC.
[GoRecv] internal static void checknonempty(this ref workbuf b) {
    if (b.nobj == 0) {
        @throw(workbufIsEmptyˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string workbufIsNotEmptyˢ = "workbuf is not empty"u8;

[GoRecv] internal static void checkempty(this ref workbuf b) {
    if (b.nobj != 0) {
        @throw(workbufIsNotEmptyˢ);
    }
}

// getempty pops an empty work buffer off the work.empty list,
// allocating new buffers if none are available.
//
//go:nowritebarrier
internal static ж<workbuf> getempty() {
    ж<workbuf> b = default!;
    if (work.empty != 0) {
        b = (ж<workbuf>)(uintptr)(Ꮡwork.of(workType.Ꮡempty).pop());
        if (b != nil) {
            b.checkempty();
        }
    }
    // Record that this may acquire the wbufSpans or heap lock to
    // allocate a workbuf.
    lockWithRankMayAcquire(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock), lockRankWbufSpans);
    lockWithRankMayAcquire(Ꮡmheap_.of(mheap.Ꮡlock), lockRankMheap);
    if (b == nil) {
        // Allocate more workbufs.
        ref var s = ref heap<ж<mspan>>(out var Ꮡs);
        if (work.wbufSpans.free.first != nil) {
            @lock(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock));
            s = work.wbufSpans.free.first;
            if (s != nil) {
                Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡfree).remove(s);
                Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡbusy).insert(s);
            }
            unlock(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock));
        }
        if (s == nil) {
            systemstack(() => {
                Ꮡs.ValueSlot = Ꮡmheap_.allocManual(workbufAlloc / pageSize, spanAllocWorkBuf);
            });
            if (s == nil) {
                @throw(outOfMemoryˢ);
            }
            // Record the new span in the busy list.
            @lock(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock));
            Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡbusy).insert(s);
            unlock(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock));
        }
        // Slice up the span into new workbufs. Return one and
        // put the rest on the empty list.
        for (var i = (uintptr)0; i + (uintptr)_WorkbufSize <= workbufAlloc; i += _WorkbufSize) {
            var newb = (ж<workbuf>)(uintptr)((@unsafe.Pointer)(s.@base() + i));
            newb.Value.nobj = 0;
            lfnodeValidate(newb.of(workbuf.Ꮡnode));
            if (i == 0){
                b = newb;
            } else {
                putempty(newb);
            }
        }
    }
    return b;
}

// putempty puts a workbuf onto the work.empty list.
// Upon entry this goroutine owns b. The lfstack.push relinquishes ownership.
//
//go:nowritebarrier
internal static void putempty(ж<workbuf> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.checkempty();
    Ꮡwork.of(workType.Ꮡempty).push(Ꮡb.of(workbuf.Ꮡnode));
}

// putfull puts the workbuf on the work.full list for the GC.
// putfull accepts partially full buffers so the GC can avoid competing
// with the mutators for ownership of partially full buffers.
//
//go:nowritebarrier
internal static void putfull(ж<workbuf> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.checknonempty();
    Ꮡwork.of(workType.Ꮡfull).push(Ꮡb.of(workbuf.Ꮡnode));
}

// trygetfull tries to get a full or partially empty workbuffer.
// If one is not immediately available return nil.
//
//go:nowritebarrier
internal static ж<workbuf> trygetfull() {
    var b = (ж<workbuf>)(uintptr)(Ꮡwork.of(workType.Ꮡfull).pop());
    if (b != nil) {
        b.checknonempty();
        return b;
    }
    return b;
}

//go:nowritebarrier
internal static ж<workbuf> handoff(ж<workbuf> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Make new buffer with half of b's pointers.
    var b1 = getempty();
    nint n = b.nobj / 2;
    b.nobj -= n;
    b1.Value.nobj = n;
    memmove(@unsafe.Pointer.FromBox(b1.at(workbuf.Ꮡobj, 0)), @unsafe.Pointer.FromBox(Ꮡb.at(workbuf.Ꮡobj, b.nobj)), (uintptr)n * /* unsafe.Sizeof(b1.obj[0]) */ (uintptr)8);
    // Put b on full list - let first half of b get stolen.
    putfull(Ꮡb);
    return b1;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotFreeWorkbufsWhenˢ = "cannot free workbufs when work.full != 0"u8;

// prepareFreeWorkbufs moves busy workbuf spans to free list so they
// can be freed to the heap. This must only be called when all
// workbufs are on the empty list.
internal static void prepareFreeWorkbufs() {
    @lock(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock));
    if (work.full != 0) {
        @throw(cannotFreeWorkbufsWhenˢ);
    }
    // Since all workbufs are on the empty list, we don't care
    // which ones are in which spans. We can wipe the entire empty
    // list and move all workbuf spans to the free list.
    work.empty = 0;
    Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡfree).takeAll(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡbusy));
    unlock(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock));
}

// freeSomeWbufs frees some workbufs back to the heap and returns
// true if it should be called again to free more.
internal static bool freeSomeWbufs(bool preemptible) {
    const nint batchSize = 64; // ~1–2 µs per span.
    @lock(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock));
    if (gcphase != _GCoff || Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡfree).isEmpty()) {
        unlock(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock));
        return false;
    }
    systemstack(() => {
        var gp = getg().Value.m.Value.curg;
        for (nint i = 0; i < batchSize && !(preemptible && (~gp).preempt); i++) {
            var span = work.wbufSpans.free.first;
            if (span == nil) {
                break;
            }
            Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡfree).remove(span);
            Ꮡmheap_.freeManual(span, spanAllocWorkBuf);
        }
    });
    var more = !Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡfree).isEmpty();
    unlock(Ꮡwork.of(workType.ᏑwbufSpans).of(workType_wbufSpans.Ꮡlock));
    return more;
}

} // end runtime_package
