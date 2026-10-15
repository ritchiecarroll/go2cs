// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains stub functions that are not meant to be called directly,
// but that will be assembled together using the inlining logic in runtime/_mkmalloc
// to produce a full mallocgc function that's specialized for a span class
// or specific size in the case of the tiny allocator.
//
// To generate the specialized mallocgc functions, do 'go run .' inside runtime/_mkmalloc.
//
// To assemble a mallocgc function, the mallocStub function is cloned, and the call to
// inlinedMalloc is replaced with the inlined body of smallScanNoHeaderStub,
// smallNoScanStub or tinyStub, depending on the parameters being specialized.
//
// The size_ (for the tiny case) and elemsize_, sizeclass_, and noscanint_ (for all three cases)
// identifiers are replaced with the value of the parameter in the specialized case.
// The nextFreeFastStub, nextFreeFastTiny, heapSetTypeNoHeaderStub, and writeHeapBitsSmallStub
// functions are also inlined by _mkmalloc.
namespace go;

using goarch = @internal.goarch_package;
using goexperiment = @internal.goexperiment_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// These identifiers will all be replaced by the inliner. So their values don't
// really matter: they just need to be set so that the stub functions, which
// will never be used on their own, can compile. elemsize_ can't be  set to
// zero because we divide by it in nextFreeFastTiny, and the compiler would
// complain about a division by zero. Its replaced value will always be greater
// than zero.
internal static UntypedInt elemsize_ => 8;

internal static UntypedInt sizeclass_ => 0;

internal static UntypedInt noscanint_ => 0;

internal static UntypedInt size_ => 0;

internal const bool isTiny_ = false;

internal static @unsafe.Pointer malloc0(uintptr size, ref _type typ, bool needzero) {
    if (doubleCheckMalloc) {
        if (gcphase == _GCmarktermination) {
            @throw(mallocgcCalledWithˢ);
        }
    }

    // Short-circuit zero-sized allocation requests.
    return @unsafe.Pointer.FromBox(Ꮡzerobase);
}

internal static @unsafe.Pointer mallocPanic(uintptr size, ж<_type> Ꮡtyp, bool needzero) {
    throw panic("not defined for sizeclass");
}

// WARNING: mallocStub does not do any work for sanitizers so callers need
// to steer out of this codepath early if sanitizers are enabled.
internal static @unsafe.Pointer mallocStub(uintptr size, ж<_type> Ꮡtyp, bool needzero) {
    if (isTiny_) {
        // secret code, need to avoid the tiny allocator since it might keep
        // co-located values alive longer and prevent timely zero-ing
        //
        // Call directly into the NoScan allocator.
        // See go.dev/issue/76356
        var gp = getg();
        if (goexperiment.RuntimeSecret && (~gp).secret > 0) {
            return (uintptr)mallocgcSmallNoScanSC2(size, Ꮡtyp, needzero);
        }
    }
    if (doubleCheckMalloc) {
        if (gcphase == _GCmarktermination) {
            @throw(mallocgcCalledWithˢ);
        }
    }

    // It's possible for any malloc to trigger sweeping, which may in
    // turn queue finalizers. Record this dynamic lock edge.
    // N.B. Compiled away if lockrank experiment is not enabled.
    lockRankMayQueueFinalizer();

    // Pre-malloc debug hooks.
    if (debug.malloc) {
        {
            @unsafe.Pointer xΔ1 = (uintptr)preMallocgcDebug(size, Ꮡtyp); if (xΔ1 != nil) {
                return xΔ1;
            }
        }
    }

    // Assist the GC if needed. (On the reuse path, we currently compensate for this;
    // changes here might require changes there.)
    if (gcBlackenEnabled != 0) {
        deductAssistCredit(size);
    }

    // Actually do the allocation.
    var (x, elemsize) = inlinedMalloc(size, ref (Ꮡtyp).DerefOrNull(), needzero);

    if (!isTiny_) {
        var gp = getg();
        if (goexperiment.RuntimeSecret && (~gp).secret > 0) {
            // Mark any object allocated while in secret mode as secret.
            // This ensures we zero it immediately when freeing it.
            addSecret(x, size);
        }
    }

    // Notify valgrind, if enabled.
    // To allow the compiler to not know about valgrind, we do valgrind instrumentation
    // unlike the other sanitizers.
    if (valgrindenabled) {
        valgrindMalloc(x, size);
    }

    // Adjust our GC assist debt to account for internal fragmentation.
    if (gcBlackenEnabled != 0 && elemsize != 0) {
        {
            var assistG = getg().Value.m.Value.curg; if (assistG != nil) {
                assistG.Value.gcAssistBytes -= (int64)(elemsize - size);
            }
        }
    }

    // Post-malloc debug hooks.
    if (debug.malloc) {
        postMallocgcDebug(x, elemsize, Ꮡtyp);
    }
    return x;
}

// inlinedMalloc will never be called. It is defined just so that the compiler can compile
// the mallocStub function, which will also never be called, but instead used as a template
// to generate a size-specialized malloc function. The call to inlinedMalloc in mallocStub
// will be replaced with the inlined body of smallScanNoHeaderStub, smallNoScanStub, or tinyStub
// when generating the size-specialized malloc function. See the comment at the top of this
// file for more information.
internal static (@unsafe.Pointer, uintptr) inlinedMalloc(uintptr size, ref _type typ, bool needzero) {
    return ((@unsafe.Pointer)(uintptr)0, 0);
}

internal static void doubleCheckSmallScanNoHeader(uintptr size, ж<_type> Ꮡtyp, ref m mp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (mp.mallocing != 0) {
        @throw(mallocDeadlockˢ);
    }
    if (mp.gsignal == getg()) {
        @throw(mallocDuringSignalˢ);
    }
    if (Ꮡtyp == nil || !typ.Pointers()) {
        @throw(noscanAllocatedInScanˢ);
    }
    if (!heapBitsInSpan(size)) {
        @throw(heapBitsInNotInSpanForˢ);
    }
}

internal static (@unsafe.Pointer, uintptr) smallScanNoHeaderStub(uintptr size, ж<_type> Ꮡtyp, bool needzero) {
    UntypedInt sizeclass = /* sizeclass_ */ 0;
    UntypedInt elemsize = /* elemsize_ */ 8;

    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if (doubleCheckMalloc) {
        doubleCheckSmallScanNoHeader(size, Ꮡtyp, ref (mp).DerefOrNull());
    }
    mp.Value.mallocing = 1;

    var checkGCTrigger = false;
    var c = getMCache(ref (mp).DerefOrNull());
    spanClass spc = /* spanClass(sizeclass<<1) | spanClass(noscanint_) */ 0;
    var span = (~c).alloc[spc];
    var v = nextFreeFastStub(span);
    if (v == 0) {
        (v, span, checkGCTrigger) = c.nextFree(spc);
    }
    @unsafe.Pointer x = ((@unsafe.Pointer)(uintptr)v);
    if ((~span).needzero != 0) {
        memclrNoHeapPointers(x, elemsize);
    }
    if (goarch.PtrSize == 8 && sizeclass == 1){
        // initHeapBits already set the pointer bits for the 8-byte sizeclass
        // on 64-bit platforms.
        c.Value.scanAlloc += 8;
    } else {
        var dataSize = size; // make the inliner happy
        var xΔ1 = (uintptr)x;
        var scanSize = heapSetTypeNoHeaderStub(xΔ1, dataSize, Ꮡtyp, span);
        c.Value.scanAlloc += scanSize;
    }

    // Ensure that the stores above that initialize x to
    // type-safe memory and set the heap bits occur before
    // the caller can make x observable to the garbage
    // collector. Otherwise, on weakly ordered machines,
    // the garbage collector could follow a pointer to x,
    // but see uninitialized memory or stale heap bits.
    publicationBarrier();

    if (writeBarrier.enabled){
        // Allocate black during GC.
        // All slots hold nil so no scanning is needed.
        // This may be racing with GC so do it atomically if there can be
        // a race marking the bit.
        gcmarknewobject(span, (uintptr)x);
    } else {
        // Track the last free index before the mark phase. This field
        // is only used by the garbage collector. During the mark phase
        // this is used by the conservative scanner to filter out objects
        // that are both free and recently-allocated. It's safe to do that
        // because we allocate-black if the GC is enabled. The conservative
        // scanner produces pointers out of thin air, so without additional
        // synchronization it might otherwise observe a partially-initialized
        // object, which could crash the program.
        span.Value.freeIndexForScan = span.Value.freeindex;
    }

    // Note cache c only valid while m acquired; see #47302
    //
    // N.B. Use the full size because that matches how the GC
    // will update the mem profile on the "free" side.
    //
    // TODO(mknyszek): We should really count the header as part
    // of gc_sys or something. The code below just pretends it is
    // internal fragmentation and matches the GC's accounting by
    // using the whole allocation slot.
    c.Value.nextSample -= (int64)elemsize;
    if ((~c).nextSample < 0 || MemProfileRate != (~c).memProfRate) {
        profilealloc(ref (mp).DerefOrNull(), x, elemsize);
    }
    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());

    if (checkGCTrigger) {
        {
            var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
                gcStart(t);
            }
        }
    }

    return (x, elemsize);
}

internal static void doubleCheckSmallNoScan(ж<_type> Ꮡtyp, ref m mp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (mp.mallocing != 0) {
        @throw(mallocDeadlockˢ);
    }
    if (mp.gsignal == getg()) {
        @throw(mallocDuringSignalˢ);
    }
    if (Ꮡtyp != nil && typ.Pointers()) {
        @throw(expectedNoscanTypeForˢ);
    }
}

internal static (@unsafe.Pointer, uintptr) smallNoScanStub(uintptr size, ж<_type> Ꮡtyp, bool needzero) {
    // TODO(matloob): Add functionality to mkmalloc to allow us to inline a non-constant
    // sizeclass_ and elemsize_ value (instead just set to the expressions to look up the size class
    // and elemsize. We'd also need to teach mkmalloc that values that are touched by these (specifically
    // spc below) should turn into vars. This would allow us to generate mallocgcSmallNoScan itself,
    // so that its code could not diverge from the generated functions.
    UntypedInt sizeclass = /* sizeclass_ */ 0;
    UntypedInt elemsize = /* elemsize_ */ 8;

    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if (doubleCheckMalloc) {
        doubleCheckSmallNoScan(Ꮡtyp, ref (mp).DerefOrNull());
    }
    mp.Value.mallocing = 1;

    var checkGCTrigger = false;
    var c = getMCache(ref (mp).DerefOrNull());
    spanClass spc = /* spanClass(sizeclass<<1) | spanClass(noscanint_) */ 0;
    var span = (~c).alloc[spc];

    // First, check for a reusable object.
    if (runtimeFreegcEnabled && c.hasReusableNoscan(spc)) {
        // We have a reusable object, use it.
        @unsafe.Pointer vΔ1 = (uintptr)mallocgcSmallNoscanReuse(c, span, spc, elemsize, needzero);
        mp.Value.mallocing = 0;
        releasem(ref (mp).DerefOrNull());

        // TODO(thepudds): note that the generated return path is essentially duplicated
        // by the generator. For example, see the two postMallocgcDebug calls and
        // related duplicated code on the return path currently in the generated
        // mallocgcSmallNoScanSC2 function. One set of those correspond to this
        // return here. We might be able to de-duplicate the generated return path
        // by updating the generator, perhaps by jumping to a shared return or similar.
        return (vΔ1, elemsize);
    }

    var v = nextFreeFastStub(span);
    if (v == 0) {
        (v, span, checkGCTrigger) = c.nextFree(spc);
    }
    @unsafe.Pointer x = ((@unsafe.Pointer)(uintptr)v);
    if (needzero && (~span).needzero != 0) {
        memclrNoHeapPointers(x, elemsize);
    }

    // Ensure that the stores above that initialize x to
    // type-safe memory and set the heap bits occur before
    // the caller can make x observable to the garbage
    // collector. Otherwise, on weakly ordered machines,
    // the garbage collector could follow a pointer to x,
    // but see uninitialized memory or stale heap bits.
    publicationBarrier();

    if (writeBarrier.enabled){
        // Allocate black during GC.
        // All slots hold nil so no scanning is needed.
        // This may be racing with GC so do it atomically if there can be
        // a race marking the bit.
        gcmarknewobject(span, (uintptr)x);
    } else {
        // Track the last free index before the mark phase. This field
        // is only used by the garbage collector. During the mark phase
        // this is used by the conservative scanner to filter out objects
        // that are both free and recently-allocated. It's safe to do that
        // because we allocate-black if the GC is enabled. The conservative
        // scanner produces pointers out of thin air, so without additional
        // synchronization it might otherwise observe a partially-initialized
        // object, which could crash the program.
        span.Value.freeIndexForScan = span.Value.freeindex;
    }

    // Note cache c only valid while m acquired; see #47302
    //
    // N.B. Use the full size because that matches how the GC
    // will update the mem profile on the "free" side.
    //
    // TODO(mknyszek): We should really count the header as part
    // of gc_sys or something. The code below just pretends it is
    // internal fragmentation and matches the GC's accounting by
    // using the whole allocation slot.
    c.Value.nextSample -= (int64)elemsize;
    if ((~c).nextSample < 0 || MemProfileRate != (~c).memProfRate) {
        profilealloc(ref (mp).DerefOrNull(), x, elemsize);
    }
    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());

    if (checkGCTrigger) {
        {
            var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
                gcStart(t);
            }
        }
    }
    return (x, elemsize);
}

internal static void doubleCheckTiny(uintptr size, ж<_type> Ꮡtyp, ref m mp) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (mp.mallocing != 0) {
        @throw(mallocDeadlockˢ);
    }
    if (mp.gsignal == getg()) {
        @throw(mallocDuringSignalˢ);
    }
    if (Ꮡtyp != nil && typ.Pointers()) {
        @throw(expectedNoscanForTinyˢ);
    }
}

internal static (@unsafe.Pointer, uintptr) tinyStub(uintptr size, ж<_type> Ꮡtyp, bool needzero) {
    UntypedInt constsize = /* size_ */ 0;
    UntypedInt elemsize = /* elemsize_ */ 8;

    // Set mp.mallocing to keep from being preempted by GC.
    var mp = acquirem();
    if (doubleCheckMalloc) {
        doubleCheckTiny(constsize, Ꮡtyp, ref (mp).DerefOrNull());
    }
    mp.Value.mallocing = 1;

    // Tiny allocator.
    //
    // Tiny allocator combines several tiny allocation requests
    // into a single memory block. The resulting memory block
    // is freed when all subobjects are unreachable. The subobjects
    // must be noscan (don't have pointers), this ensures that
    // the amount of potentially wasted memory is bounded.
    //
    // Size of the memory block used for combining (maxTinySize) is tunable.
    // Current setting is 16 bytes, which relates to 2x worst case memory
    // wastage (when all but one subobjects are unreachable).
    // 8 bytes would result in no wastage at all, but provides less
    // opportunities for combining.
    // 32 bytes provides more opportunities for combining,
    // but can lead to 4x worst case wastage.
    // The best case winning is 8x regardless of block size.
    //
    // Objects obtained from tiny allocator must not be freed explicitly.
    // So when an object will be freed explicitly, we ensure that
    // its size >= maxTinySize.
    //
    // SetFinalizer has a special case for objects potentially coming
    // from tiny allocator, it such case it allows to set finalizers
    // for an inner byte of a memory block.
    //
    // The main targets of tiny allocator are small strings and
    // standalone escaping variables. On a json benchmark
    // the allocator reduces number of allocations by ~12% and
    // reduces heap size by ~20%.
    var c = getMCache(ref (mp).DerefOrNull());
    var off = c.Value.tinyoffset;
    // Align tiny pointer for required (conservative) alignment.
    if ((UntypedInt)(constsize & 7) == 0){
        off = alignUp(off, 8);
    } else 
    if (goarch.PtrSize == 4 && constsize == 12){
        // Conservatively align 12-byte objects to 8 bytes on 32-bit
        // systems so that objects whose first field is a 64-bit
        // value is aligned to 8 bytes and does not cause a fault on
        // atomic access. See issue 37262.
        // TODO(mknyszek): Remove this workaround if/when issue 36606
        // is resolved.
        off = alignUp(off, 8);
    } else 
    if ((UntypedInt)(constsize & 3) == 0){
        off = alignUp(off, 4);
    } else 
    if ((UntypedInt)(constsize & 1) == 0) {
        off = alignUp(off, 2);
    }
    if (off + (uintptr)constsize <= maxTinySize && (~c).tiny != 0) {
        // The object fits into existing tiny block.
        @unsafe.Pointer xΔ1 = (@unsafe.Pointer)((~c).tiny + off);
        c.Value.tinyoffset = off + (uintptr)constsize;
        c.Value.tinyAllocs++;
        mp.Value.mallocing = 0;
        releasem(ref (mp).DerefOrNull());
        return (xΔ1, 0);
    }
    // Allocate a new maxTinySize block.
    var checkGCTrigger = false;
    var span = (~c).alloc[tinySpanClass];
    var v = nextFreeFastTiny(span);
    if (v == 0) {
        (v, span, checkGCTrigger) = c.nextFree(tinySpanClass);
    }
    @unsafe.Pointer x = ((@unsafe.Pointer)(uintptr)v);
    ((ж<array<uint64>>)(uintptr)(x)).Value[0] = 0; // Always zero
    ((ж<array<uint64>>)(uintptr)(x)).Value[1] = 0;
    // See if we need to replace the existing tiny block with the new one
    // based on amount of remaining free space.
    if (!raceenabled && (constsize < (~c).tinyoffset || (~c).tiny == 0)) {
        // Note: disabled when race detector is on, see comment near end of this function.
        c.Value.tiny = (uintptr)x;
        c.Value.tinyoffset = constsize;
    }

    // Ensure that the stores above that initialize x to
    // type-safe memory and set the heap bits occur before
    // the caller can make x observable to the garbage
    // collector. Otherwise, on weakly ordered machines,
    // the garbage collector could follow a pointer to x,
    // but see uninitialized memory or stale heap bits.
    publicationBarrier();

    if (writeBarrier.enabled){
        // Allocate black during GC.
        // All slots hold nil so no scanning is needed.
        // This may be racing with GC so do it atomically if there can be
        // a race marking the bit.
        gcmarknewobject(span, (uintptr)x);
    } else {
        // Track the last free index before the mark phase. This field
        // is only used by the garbage collector. During the mark phase
        // this is used by the conservative scanner to filter out objects
        // that are both free and recently-allocated. It's safe to do that
        // because we allocate-black if the GC is enabled. The conservative
        // scanner produces pointers out of thin air, so without additional
        // synchronization it might otherwise observe a partially-initialized
        // object, which could crash the program.
        span.Value.freeIndexForScan = span.Value.freeindex;
    }

    // Note cache c only valid while m acquired; see #47302
    //
    // N.B. Use the full size because that matches how the GC
    // will update the mem profile on the "free" side.
    //
    // TODO(mknyszek): We should really count the header as part
    // of gc_sys or something. The code below just pretends it is
    // internal fragmentation and matches the GC's accounting by
    // using the whole allocation slot.
    c.Value.nextSample -= (int64)elemsize;
    if ((~c).nextSample < 0 || MemProfileRate != (~c).memProfRate) {
        profilealloc(ref (mp).DerefOrNull(), x, elemsize);
    }
    mp.Value.mallocing = 0;
    releasem(ref (mp).DerefOrNull());

    if (checkGCTrigger) {
        {
            var t = (new gcTrigger(kind: gcTriggerHeap)); if (t.test()) {
                gcStart(t);
            }
        }
    }

    if (raceenabled) {
        // Pad tinysize allocations so they are aligned with the end
        // of the tinyalloc region. This ensures that any arithmetic
        // that goes off the top end of the object will be detectable
        // by checkptr (issue 38872).
        // Note that we disable tinyalloc when raceenabled for this to work.
        // TODO: This padding is only performed when the race detector
        // is enabled. It would be nice to enable it if any package
        // was compiled with checkptr, but there's no easy way to
        // detect that (especially at compile time).
        // TODO: enable this padding for all allocations, not just
        // tinyalloc ones. It's tricky because of pointer maps.
        // Maybe just all noscan objects?
        x = (uintptr)add(x, elemsize - constsize);
    }
    return (x, elemsize);
}

// TODO(matloob): Should we let the go compiler inline this instead of using mkmalloc?
// We won't be able to use elemsize_ but that's probably ok.
internal static gclinkptr nextFreeFastTiny(ж<mspan> Ꮡspan) {
    ref var span = ref Ꮡspan.DerefOrNull();

    UntypedInt nbytes = 8192;
    const uint16 nelems = /* uint16((nbytes - unsafe.Sizeof(spanInlineMarkBits{})) / elemsize_) */ 1008;
    gclinkptr nextFreeFastResult = default!;
    if (span.allocCache != 0) {
        nint theBit = sys.TrailingZeros64(span.allocCache); // Is there a free object in the allocCache?
        var result = (uint16)(span.freeindex + (uint16)theBit);
        if (result < nelems) {
            var freeidx = (uint16)(result + 1);
            if (!((uint16)(freeidx % 64) == 0 && freeidx != nelems)) {
                span.allocCache.RshAssign((nuint)(theBit + 1));
                span.freeindex = freeidx;
                span.allocCount++;
                nextFreeFastResult = ((gclinkptr)((uintptr)result * (uintptr)elemsize_ + span.@base()));
            }
        }
    }
    return nextFreeFastResult;
}

internal static gclinkptr nextFreeFastStub(ж<mspan> Ꮡspan) {
    ref var span = ref Ꮡspan.DerefOrNull();

    gclinkptr nextFreeFastResult = default!;
    if (span.allocCache != 0) {
        nint theBit = sys.TrailingZeros64(span.allocCache); // Is there a free object in the allocCache?
        var result = (uint16)(span.freeindex + (uint16)theBit);
        if (result < span.nelems) {
            var freeidx = (uint16)(result + 1);
            if (!((uint16)(freeidx % 64) == 0 && freeidx != span.nelems)) {
                span.allocCache.RshAssign((nuint)(theBit + 1));
                span.freeindex = freeidx;
                span.allocCount++;
                nextFreeFastResult = ((gclinkptr)((uintptr)result * (uintptr)elemsize_ + span.@base()));
            }
        }
    }
    return nextFreeFastResult;
}

internal static uintptr heapSetTypeNoHeaderStub(uintptr x, uintptr dataSize, ж<_type> Ꮡtyp, ж<mspan> Ꮡspan) {
    if (doubleCheckHeapSetType && (!heapBitsInSpan(dataSize) || !heapBitsInSpan(elemsize_))) {
        @throw(triedToWriteHeapBitsButˢ);
    }
    var scanSize = writeHeapBitsSmallStub(Ꮡspan, x, dataSize, Ꮡtyp);
    if (doubleCheckHeapSetType) {
        doubleCheckHeapType(x, dataSize, Ꮡtyp, nil, Ꮡspan);
    }
    return scanSize;
}

// writeHeapBitsSmallStub writes the heap bits for small objects whose ptr/scalar data is
// stored as a bitmap at the end of the span.
//
// Assumes dataSize is <= ptrBits*goarch.PtrSize. x must be a pointer into the span.
// heapBitsInSpan(dataSize) must be true. dataSize must be >= typ.Size_.
//
//go:nosplit
internal static uintptr writeHeapBitsSmallStub(ж<mspan> Ꮡspan, uintptr x, uintptr dataSize, ж<_type> Ꮡtyp) {
    ref var span = ref Ꮡspan.DerefOrNull();
    ref var typ = ref Ꮡtyp.DerefOrNull();

    // The objects here are always really small, so a single load is sufficient.
    var src0 = readUintptr(getGCMask(Ꮡtyp));

    UntypedInt elemsize = /* elemsize_ */ 8;

    // Create repetitions of the bitmap if we have a small slice backing store.
    var scanSize = typ.PtrBytes;
    var src = src0;
    if (typ.Size_ == goarch.PtrSize){
        src = (((uintptr)1).Lsh((uint64)((dataSize / (uintptr)goarch.PtrSize)))) - 1;
    } else {
        // N.B. We rely on dataSize being an exact multiple of the type size.
        // The alternative is to be defensive and mask out src to the length
        // of dataSize. The purpose is to save on one additional masking operation.
        if (doubleCheckHeapSetType && !asanenabled && dataSize % typ.Size_ != 0) {
            @throw(runtimeMspanˢ);
        }
        for (var iΔ1 = typ.Size_; iΔ1 < dataSize; iΔ1 += typ.Size_) {
            src |= (uintptr)(src0.Lsh((uint64)((iΔ1 / (uintptr)goarch.PtrSize))));
            scanSize += typ.Size_;
        }
    }

    // Since we're never writing more than one uintptr's worth of bits, we're either going
    // to do one or two writes.
    var (dstBase, _) = spanHeapBitsRange(span.@base(), pageSize, elemsize);
    @unsafe.Pointer dst = (@unsafe.Pointer)dstBase;
    var o = (x - span.@base()) / (uintptr)goarch.PtrSize;
    var i = o / (uintptr)ptrBits;
    var j = o % (uintptr)ptrBits;
    uintptr bits = /* elemsize / goarch.PtrSize */ 1;
    // In the if statement below, we have to do two uintptr writes if the bits
    // we need to write straddle across two different memory locations. But if
    // the number of bits we're writing divides evenly into the number of bits
    // in the uintptr we're writing, this can never happen. Since bitsIsPowerOfTwo
    // is a compile-time constant in the generated code, in the case where the size is
    // a power of two less than or equal to ptrBits, the compiler can remove the
    // 'two writes' branch of the if statement and always do only one write without
    // the check.
    const bool bitsIsPowerOfTwo = /* bits&(bits-1) == 0 */ true;
    if (bits > ptrBits || (!bitsIsPowerOfTwo && j + bits > ptrBits)){
        // Two writes.
        var bits0 = (uintptr)ptrBits - j;
        var bits1 = bits - bits0;
        var dst0 = (ж<uintptr>)(uintptr)((uintptr)add(dst, (i + 0) * (uintptr)goarch.PtrSize));
        var dst1 = (ж<uintptr>)(uintptr)((uintptr)add(dst, (i + 1) * (uintptr)goarch.PtrSize));
        dst0.Value = (uintptr)((uintptr)((dst0.Value) & ((~(uintptr)0).Rsh((uint64)(bits0)))) | (src.Lsh((uint64)(j))));
        dst1.Value = (uintptr)((uintptr)((dst1.Value) & ~((((uintptr)1).Lsh((uint64)(bits1))) - 1)) | (src.Rsh((uint64)(bits0))));
    } else {
        // One write.
        var dstΔ1 = (ж<uintptr>)(uintptr)((uintptr)add(dst, i * (uintptr)goarch.PtrSize));
        dstΔ1.Value = (uintptr)((uintptr)((dstΔ1.Value) & ~(((((1 << (int)((min(bits, (uintptr)(ptrBits)))))) - 1) << (int)(j)))) | (src.Lsh((uint64)(j)))); // We're taking the min so this compiles on 32 bit platforms. But if bits > ptrbits we always take the other branch
    }

    const bool doubleCheck = false;
    if (doubleCheck) {
        writeHeapBitsDoubleCheck(Ꮡspan, x, dataSize, src, src0, i, j, bits, ref (Ꮡtyp).DerefOrNull());
    }
    return scanSize;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badPointerBitsWrittenForˢ = "bad pointer bits written for small object"u8;

internal static void writeHeapBitsDoubleCheck(ж<mspan> Ꮡspan, uintptr x, uintptr dataSize, uintptr src, uintptr src0, uintptr i, uintptr j, uintptr bits, ref _type typ) {
    ref var span = ref Ꮡspan.DerefOrNull();

    var srcRead = span.heapBitsSmallForAddr(x);
    if (srcRead != src) {
        print((@string)"runtime: x="u8, ((Δhex)(uint64)x), (@string)" i="u8, i, (@string)" j="u8, j, (@string)" bits="u8, bits, (@string)"\n"u8);
        print((@string)"runtime: dataSize="u8, dataSize, (@string)" typ.Size_="u8, typ.Size_, (@string)" typ.PtrBytes="u8, typ.PtrBytes, (@string)"\n"u8);
        print((@string)"runtime: src0="u8, ((Δhex)(uint64)src0), (@string)" src="u8, ((Δhex)(uint64)src), (@string)" srcRead="u8, ((Δhex)(uint64)srcRead), (@string)"\n"u8);
        @throw(badPointerBitsWrittenForˢ);
    }
}

} // end runtime_package
