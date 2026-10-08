// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;

partial class runtime_package {

// A Pinner is a set of Go objects each pinned to a fixed location in memory. The
// [Pinner.Pin] method pins one object, while [Pinner.Unpin] unpins all pinned
// objects. See their comments for more information.
partial struct Pinner {
    internal partial ref ж<pinner> pinner { get; }
}

// go2cs generated this placeholder — func Pin is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func Unpin is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static UntypedInt pinnerSize => 64;
internal static uintptr pinnerRefStoreSize => /* (pinnerSize - unsafe.Sizeof([]unsafe.Pointer{})) / unsafe.Sizeof(unsafe.Pointer(nil)) */ 5;

partial struct pinner {
    internal slice<@unsafe.Pointer> refs;
    internal array<@unsafe.Pointer> refStore = new(pinnerRefStoreSize);
}

internal static void unpin(this ж<pinner> Ꮡp) {
    ref var Δp = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil || Δp.refs == default!) {
        return;
    }
    foreach (var (i, _) in Δp.refs) {
        setPinned(Δp.refs[i], false);
    }
    // The following two lines make all pointers to references
    // in p.refs unreachable, either by deleting them or dropping
    // p.refs' backing store (if it was not backed by refStore).
    Δp.refStore = new @unsafe.Pointer[]{}.array(5);
    Δp.refs = Δp.refStore[..0];
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimePinnerArgumentIsˢ = "runtime.Pinner: argument is nil"u8;
internal static readonly @string runtimePinnerObjectWasˢ = "runtime.Pinner: object was allocated into an arena"u8;

internal static @unsafe.Pointer pinnerGetPtr(ж<any> Ꮡi) {
    var e = efaceOf(Ꮡi);
    var etyp = e.Value._type;
    if (etyp == nil) {
        throw panic(((errorString)(@string)runtimePinnerArgumentIsˢ));
    }
    {
        var kind = (abiꓸKind)((~etyp).Kind_ & abi.KindMask); if (kind != abi.Pointer && kind != abi.UnsafePointer) {
            throw panic(((errorString)("runtime.Pinner: argument is not a pointer: "u8 + toRType(etyp).@string())));
        }
    }
    if (inUserArenaChunk((uintptr)(~e).data)) {
        // Arena-allocated objects are not eligible for pinning.
        throw panic(((errorString)(@string)runtimePinnerObjectWasˢ));
    }
    return (~e).data;
}

// go2cs generated this placeholder — func isPinned is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string triedToUnpinNonGoPointerˢ = "tried to unpin non-Go pointer"u8;
internal static readonly @string runtimePinnerObjectˢ = "runtime.Pinner: object already unpinned"u8;

// setPinned marks or unmarks a Go pointer as pinned, when the ptr is a Go pointer.
// It will be ignored while try to pin a non-Go pointer,
// and it will be panic while try to unpin a non-Go pointer,
// which should not happen in normal usage.
internal static bool setPinned(@unsafe.Pointer ptr, bool pin) {
    var span = spanOfHeap((uintptr)ptr);
    if (span == nil) {
        if (!pin) {
            throw panic(((errorString)(@string)triedToUnpinNonGoPointerˢ));
        }
        // This is a linker-allocated, zero size object or other object,
        // nothing to do, silently ignore it.
        return false;
    }
    // ensure that the span is swept, b/c sweeping accesses the specials list
    // w/o locks.
    var mp = acquirem();
    span.ensureSwept();
    KeepAlive(@unsafe.Pointer.OrTypedNil(ptr)); // make sure ptr is still alive after span is swept
    var objIndex = span.objIndex((uintptr)ptr);
    @lock(span.of(mspan.Ꮡspeciallock)); // guard against concurrent calls of setPinned on same span
    var pinnerBits = span.getPinnerBits();
    if (pinnerBits == nil) {
        pinnerBits = span.newPinnerBits();
        span.setPinnerBits(pinnerBits);
    }
    var pinState = pinnerBits.ofObject(objIndex);
    if (pin){
        if (pinState.isPinned()){
            // multiple pins on same object, set multipin bit
            pinState.setMultiPinned(true);
            // and increase the pin counter
            // TODO(mknyszek): investigate if systemstack is necessary here
            var spanʗ1 = span;
            systemstack(() => {
                var offset = objIndex * (~spanʗ1).elemsize;
                spanʗ1.incPinCounter(offset);
            });
        } else {
            // set pin bit
            pinState.setPinned(true);
        }
    } else {
        // unpin
        if (pinState.isPinned()){
            if (pinState.isMultiPinned()){
                bool exists = default!;
                // TODO(mknyszek): investigate if systemstack is necessary here
                var spanʗ2 = span;
                systemstack(() => {
                    var offset = objIndex * (~spanʗ2).elemsize;
                    exists = spanʗ2.decPinCounter(offset);
                });
                if (!exists) {
                    // counter is 0, clear multipin bit
                    pinState.setMultiPinned(false);
                }
            } else {
                // no multipins recorded. unpin object.
                pinState.setPinned(false);
            }
        } else {
            // unpinning unpinned object, bail out
            @throw(runtimePinnerObjectˢ);
        }
    }
    unlock(span.of(mspan.Ꮡspeciallock));
    releasem(ref (mp).DerefOrNull());
    return true;
}

partial struct pinState {
    internal ж<uint8> bytep;
    internal uint8 byteVal;
    internal uint8 mask;
}

// nosplit, because it's called by isPinned, which is nosplit
//
//go:nosplit
internal static bool isPinned(this ref pinState v) {
    return ((uint8)(v.byteVal & v.mask)) != 0;
}

internal static bool isMultiPinned(this ref pinState v) {
    return ((uint8)(v.byteVal & ((uint8)(v.mask << (int)(1))))) != 0;
}

internal static void setPinned(this ref pinState v, bool val) {
    v.set(val, false);
}

internal static void setMultiPinned(this ref pinState v, bool val) {
    v.set(val, true);
}

// set sets the pin bit of the pinState to val. If multipin is true, it
// sets/unsets the multipin bit instead.
internal static void set(this ref pinState v, bool val, bool multipin) {
    var mask = v.mask;
    if (multipin) {
        mask <<= (int)(1);
    }
    if (val){
        atomic.Or8(v.bytep, mask);
    } else {
        atomic.And8(v.bytep, (uint8)(((uint8)(~mask))));
    }
}

partial struct pinnerBits /*gcBits*/;

// ofObject returns the pinState of the n'th object.
// nosplit, because it's called by isPinned, which is nosplit
//
//go:nosplit
internal static pinState ofObject(this ж<pinnerBits> Ꮡp, uintptr n) {
    var (bytep, mask) = (Ꮡp.Reinterpret<pinnerBits, gcBits>()).bitp(n * 2);
    var byteVal = atomic.Load8(bytep);
    return new pinState(bytep, byteVal, mask);
}

internal static uintptr pinnerBitSize(this ref mspan s) {
    return divRoundUp((uintptr)s.nelems * 2, 8);
}

// newPinnerBits returns a pointer to 8 byte aligned bytes to be used for this
// span's pinner bits. newPinnerBits is used to mark objects that are pinned.
// They are copied when the span is swept.
internal static ж<pinnerBits> newPinnerBits(this ref mspan s) {
    return newMarkBits((uintptr)s.nelems * 2).Reinterpret<gcBits, pinnerBits>();
}

// nosplit, because it's called by isPinned, which is nosplit
//
//go:nosplit
internal static ж<pinnerBits> getPinnerBits(this ж<mspan> Ꮡs) {
    return (ж<pinnerBits>)(uintptr)(atomic.Loadp(@unsafe.Pointer.FromBox(Ꮡs.of(mspan.ᏑpinnerBits))));
}

internal static void setPinnerBits(this ж<mspan> Ꮡs, ж<pinnerBits> Ꮡp) {
    atomicstorep(@unsafe.Pointer.FromBox(Ꮡs.of(mspan.ᏑpinnerBits)), @unsafe.Pointer.FromPinnedBox(Ꮡp));
}

// refreshPinnerBits replaces pinnerBits with a fresh copy in the arenas for the
// next GC cycle. If it does not contain any pinned objects, pinnerBits of the
// span is set to nil.
internal static void refreshPinnerBits(this ж<mspan> Ꮡs) {
    ref var s = ref Ꮡs.DerefOrNull();

    var Δp = Ꮡs.getPinnerBits();
    if (Δp == nil) {
        return;
    }
    var hasPins = false;
    var bytes = alignUp(s.pinnerBitSize(), 8);
    // Iterate over each 8-byte chunk and check for pins. Note that
    // newPinnerBits guarantees that pinnerBits will be 8-byte aligned, so we
    // don't have to worry about edge cases, irrelevant bits will simply be
    // zero.
    foreach (var (_, x) in @unsafe.Slice(Δp.of(pinnerBits.Ꮡx).Reinterpret<uint8, uint64>(), bytes / 8)) {
        if (x != 0) {
            hasPins = true;
            break;
        }
    }
    if (hasPins){
        var newPinnerBits = s.newPinnerBits();
        memmove(@unsafe.Pointer.FromPinnedBox(newPinnerBits.of(pinnerBits.Ꮡx)), @unsafe.Pointer.FromPinnedBox(Δp.of(pinnerBits.Ꮡx)), bytes);
        Ꮡs.setPinnerBits(newPinnerBits);
    } else {
        Ꮡs.setPinnerBits(nil);
    }
}

// incPinCounter is only called for multiple pins of the same object and records
// the _additional_ pins.
internal static void incPinCounter(this ж<mspan> Ꮡspan, uintptr offset) {
    ж<specialPinCounter> rec = default!;
    var (@ref, exists) = Ꮡspan.specialFindSplicePoint(offset, _KindSpecialPinCounter);
    if (!exists){
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        rec = (ж<specialPinCounter>)(uintptr)(Ꮡmheap_.of(mheap.ᏑspecialPinCounterAlloc).alloc());
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        // splice in record, fill in offset.
        rec.Value.special.offset = offset;
        rec.Value.special.kind = _KindSpecialPinCounter;
        rec.Value.special.next = @ref.ValueSlot;
        @ref.ValueSlot = rec.Reinterpret<specialPinCounter, special>();
        spanHasSpecials(Ꮡspan);
    } else {
        rec = @ref.ValueSlot.Reinterpret<special, specialPinCounter>();
    }
    rec.Value.counter++;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimePinnerDecreasedˢ = "runtime.Pinner: decreased non-existing pin counter"u8;

// decPinCounter decreases the counter. If the counter reaches 0, the counter
// special is deleted and false is returned. Otherwise true is returned.
internal static bool decPinCounter(this ж<mspan> Ꮡspan, uintptr offset) {
    ref var span = ref Ꮡspan.DerefOrNull();

    var (@ref, exists) = Ꮡspan.specialFindSplicePoint(offset, _KindSpecialPinCounter);
    if (!exists) {
        @throw(runtimePinnerDecreasedˢ);
    }
    var counter = @ref.ValueSlot.Reinterpret<special, specialPinCounter>();
    counter.Value.counter--;
    if ((~counter).counter == 0) {
        @ref.ValueSlot = counter.Value.special.next;
        if (span.specials == nil) {
            spanHasNoSpecials(Ꮡspan);
        }
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        Ꮡmheap_.of(mheap.ᏑspecialPinCounterAlloc).free(@unsafe.Pointer.FromPinnedBox(counter));
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        return false;
    }
    return true;
}

// go2cs generated this placeholder — func pinnerGetPinCounter is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// to be able to test that the GC panics when a pinned pointer is leaking, this
// panic function is a variable, that can be overwritten by a test.
internal static Action pinnerLeakPanic = () => {
    throw panic(((errorString)(@string)"runtime.Pinner: found leaking pinned pointer; forgot to call Unpin()?"u8));
};

} // end runtime_package
