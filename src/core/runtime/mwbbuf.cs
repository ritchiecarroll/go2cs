// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This implements the write barrier buffer. The write barrier itself
// is gcWriteBarrier and is implemented in assembly.
//
// See mbarrier.go for algorithmic details on the write barrier. This
// file deals only with the buffer.
//
// The write barrier has a fast path and a slow path. The fast path
// simply enqueues to a per-P write barrier buffer. It's written in
// assembly and doesn't clobber any general purpose registers, so it
// doesn't have the usual overheads of a Go call.
//
// When the buffer fills up, the write barrier invokes the slow path
// (wbBufFlush) to flush the buffer to the GC work queues. In this
// path, since the compiler didn't spill registers, we spill *all*
// registers and disallow any GC safe points that could observe the
// stack frame (since we don't know the types of the spilled
// registers).
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// testSmallBuf forces a small write barrier buffer to stress write
// barrier flushing.
internal const bool testSmallBuf = false;

// wbBuf is a per-P buffer of pointers queued by the write barrier.
// This buffer is flushed to the GC workbufs when it fills up and on
// various GC transitions.
//
// This is closely related to a "sequential store buffer" (SSB),
// except that SSBs are usually used for maintaining remembered sets,
// while this is used for marking.
partial struct wbBuf {
    // next points to the next slot in buf. It must not be a
    // pointer type because it can point past the end of buf and
    // must be updated without write barriers.
    //
    // This is a pointer rather than an index to optimize the
    // write barrier assembly.
    internal uintptr next;
    // end points to just past the end of buf. It must not be a
    // pointer type because it points past the end of buf and must
    // be updated without write barriers.
    internal uintptr end;
    // buf stores a series of pointers to execute write barriers on.
    internal array<uintptr> buf = new(wbBufEntries);
}

internal static UntypedInt wbBufEntries => 512;
internal static UntypedInt wbMaxEntriesPerCall => 8;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badWriteBarrierBufferˢ = "bad write barrier buffer bounds"u8;

// reset empties b by resetting its next and end pointers.
internal static void reset(this ref wbBuf b) {
    var start = (uintptr)@unsafe.Pointer.FromBox(Ꮡ(b.buf, 0));
    b.next = start;
    if (testSmallBuf){
        // For testing, make the buffer smaller but more than
        // 1 write barrier's worth, so it tests both the
        // immediate flush and delayed flush cases.
        b.end = (uintptr)@unsafe.Pointer.FromBox(Ꮡ(b.buf, wbMaxEntriesPerCall + 1));
    } else {
        b.end = start + (uintptr)len(b.buf) * /* unsafe.Sizeof(b.buf[0]) */ (uintptr)8;
    }
    if ((b.end - b.next) % /* unsafe.Sizeof(b.buf[0]) */ (uintptr)8 != 0) {
        @throw(badWriteBarrierBufferˢ);
    }
}

// discard resets b's next pointer, but not its end pointer.
//
// This must be nosplit because it's called by wbBufFlush.
//
//go:nosplit
internal static void discard(this ref wbBuf b) {
    b.next = (uintptr)@unsafe.Pointer.FromBox(Ꮡ(b.buf, 0));
}

// empty reports whether b contains no pointers.
internal static bool empty(this ref wbBuf b) {
    return b.next == (uintptr)@unsafe.Pointer.FromBox(Ꮡ(b.buf, 0));
}

// getX returns space in the write barrier buffer to store X pointers.
// getX will flush the buffer if necessary. Callers should use this as:
//
//	buf := &getg().m.p.ptr().wbBuf
//	p := buf.get2()
//	p[0], p[1] = old, new
//	... actual memory write ...
//
// The caller must ensure there are no preemption points during the
// above sequence. There must be no preemption points while buf is in
// use because it is a per-P resource. There must be no preemption
// points between the buffer put and the write to memory because this
// could allow a GC phase change, which could result in missed write
// barriers.
//
// getX must be nowritebarrierrec to because write barriers here would
// corrupt the write barrier buffer. It (and everything it calls, if
// it called anything) has to be nosplit to avoid scheduling on to a
// different P and a different buffer.
//
//go:nowritebarrierrec
//go:nosplit
internal static ж<array<uintptr>> get1(this ref wbBuf b) {
    if (b.next + (uintptr)goarch.PtrSize > b.end) {
        wbBufFlush();
    }
    var Δp = (ж<array<uintptr>>)(uintptr)((@unsafe.Pointer)b.next);
    b.next += goarch.PtrSize;
    return Δp;
}

//go:nowritebarrierrec
//go:nosplit
internal static ж<array<uintptr>> get2(this ref wbBuf b) {
    if (b.next + (uintptr)(2 * goarch.PtrSize) > b.end) {
        wbBufFlush();
    }
    var Δp = (ж<array<uintptr>>)(uintptr)((@unsafe.Pointer)b.next);
    b.next += 2 * goarch.PtrSize;
    return Δp;
}

// wbBufFlush flushes the current P's write barrier buffer to the GC
// workbufs.
//
// This must not have write barriers because it is part of the write
// barrier implementation.
//
// This and everything it calls must be nosplit because 1) the stack
// contains untyped slots from gcWriteBarrier and 2) there must not be
// a GC safe point between the write barrier test in the caller and
// flushing the buffer.
//
// TODO: A "go:nosplitrec" annotation would be perfect for this.
//
//go:nowritebarrierrec
//go:nosplit
internal static void wbBufFlush() {
    // Note: Every possible return from this function must reset
    // the buffer's next pointer to prevent buffer overflow.
    if ((~(~getg()).m).dying > 0) {
        // We're going down. Not much point in write barriers
        // and this way we can allow write barriers in the
        // panic path.
        (~(~getg()).m).p.ptr().of(runtime_package.Δp.ᏑwbBuf).discard();
        return;
    }
    // Switch to the system stack so we don't have to worry about
    // safe points.
    systemstack(() => {
        wbBufFlush1((~(~getg()).m).p.ptr());
    });
}

// wbBufFlush1 flushes p's write barrier buffer to the GC work queue.
//
// This must not have write barriers because it is part of the write
// barrier implementation, so this may lead to infinite loops or
// buffer corruption.
//
// This must be non-preemptible because it uses the P's workbuf.
//
//go:nowritebarrierrec
//go:systemstack
internal static void wbBufFlush1(ж<Δp> Ꮡpp) {
    ref var pp = ref Ꮡpp.DerefOrNull();

    // Get the buffered pointers.
    var start = (uintptr)@unsafe.Pointer.FromBox(Ꮡpp.of(runtime_package.Δp.ᏑwbBuf).at(wbBuf.Ꮡbuf, 0));
    var n = (pp.wbBuf.next - start) / /* unsafe.Sizeof(pp.wbBuf.buf[0]) */ (uintptr)8;
    var ptrs = pp.wbBuf.buf.slice(0, (nint)(n));
    // Poison the buffer to make extra sure nothing is enqueued
    // while we're processing the buffer.
    pp.wbBuf.next = 0;
    if (useCheckmark) {
        // Slow path for checkmark mode.
        foreach (var (_, ptr) in ptrs) {
            shade(ptr);
        }
        pp.wbBuf.reset();
        return;
    }
    // Mark all of the pointers in the buffer and record only the
    // pointers we greyed. We use the buffer itself to temporarily
    // record greyed pointers.
    //
    // TODO: Should scanobject/scanblock just stuff pointers into
    // the wbBuf? Then this would become the sole greying path.
    //
    // TODO: We could avoid shading any of the "new" pointers in
    // the buffer if the stack has been shaded, or even avoid
    // putting them in the buffer at all (which would double its
    // capacity). This is slightly complicated with the buffer; we
    // could track whether any un-shaded goroutine has used the
    // buffer, or just track globally whether there are any
    // un-shaded stacks and flush after each stack scan.
    var gcw = Ꮡpp.of(runtime_package.Δp.Ꮡgcw);
    nint pos = 0;
    foreach (var (_, ptr) in ptrs) {
        if (ptr < minLegalPointer) {
            // nil pointers are very common, especially
            // for the "old" values. Filter out these and
            // other "obvious" non-heap pointers ASAP.
            //
            // TODO: Should we filter out nils in the fast
            // path to reduce the rate of flushes?
            continue;
        }
        var (obj, span, objIndex) = findObject(ptr, 0, 0);
        if (obj == 0) {
            continue;
        }
        // TODO: Consider making two passes where the first
        // just prefetches the mark bits.
        var mbits = span.markBitsForIndex(objIndex);
        if (mbits.isMarked()) {
            continue;
        }
        mbits.setMarked();
        // Mark span.
        var (arena, pageIdx, pageMask) = pageIndexOf(span.@base());
        if ((uint8)((~arena).pageMarks[pageIdx] & pageMask) == 0) {
            atomic.Or8(arena.at(heapArena.ᏑpageMarks, (ulong)(pageIdx)), pageMask);
        }
        if ((~span).spanclass.noscan()) {
            gcw.Value.bytesMarked += (uint64)(~span).elemsize;
            continue;
        }
        ptrs[pos] = obj;
        pos++;
    }
    // Enqueue the greyed objects.
    gcw.putBatch(ptrs.slice(0, pos));
    pp.wbBuf.reset();
}

} // end runtime_package
