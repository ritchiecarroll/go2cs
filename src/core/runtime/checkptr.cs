// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string checkptrMisalignedˢ = "checkptr: misaligned pointer conversion"u8;
internal static readonly @string checkptrConvertedPointerˢ = "checkptr: converted pointer straddles multiple allocations"u8;

internal static void checkptrAlignment(@unsafe.Pointer Δp, ж<_type> Ꮡelem, uintptr n) {
    ref var elem = ref Ꮡelem.DerefOrNull();

    // nil pointer is always suitably aligned (#47430).
    if (Δp == nil) {
        return;
    }

    // Check that (*[n]elem)(p) is appropriately aligned.
    // Note that we allow unaligned pointers if the types they point to contain
    // no pointers themselves. See issue 37298.
    // TODO(mdempsky): What about fieldAlign?
    if (elem.Pointers() && (uintptr)((uintptr)Δp & ((uintptr)elem.Align_ - 1)) != 0) {
        @throw(checkptrMisalignedˢ);
    }

    // Check that (*[n]elem)(p) doesn't straddle multiple heap objects.
    // TODO(mdempsky): Fix #46938 so we don't need to worry about overflow here.
    if (checkptrStraddles(Δp, n * elem.Size_)) {
        @throw(checkptrConvertedPointerˢ);
    }
}

// checkptrStraddles reports whether the first size-bytes of memory
// addressed by ptr is known to straddle more than one Go allocation.
internal static bool checkptrStraddles(@unsafe.Pointer ptr, uintptr size) {
    if (size <= 1) {
        return false;
    }

    // Check that add(ptr, size-1) won't overflow. This avoids the risk
    // of producing an illegal pointer value (assuming ptr is legal).
    if ((uintptr)ptr >= ((uintptr)0 - (size - 1))) {
        return true;
    }
    @unsafe.Pointer end = (uintptr)add(ptr, size - 1);

    // TODO(mdempsky): Detect when [ptr, end] contains Go allocations,
    // but neither ptr nor end point into one themselves.

    return checkptrBase(ptr) != checkptrBase(end);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string checkptrPointerˢ = "checkptr: pointer arithmetic computed bad pointer value"u8;
internal static readonly @string checkptrPointerˢ2 = "checkptr: pointer arithmetic result points to invalid allocation"u8;

internal static void checkptrArithmetic(@unsafe.Pointer Δp, slice<@unsafe.Pointer> originals) {
    if (0 < (uintptr)Δp && (uintptr)Δp < minLegalPointer) {
        @throw(checkptrPointerˢ);
    }

    // Check that if the computed pointer p points into a heap
    // object, then one of the original pointers must have pointed
    // into the same object.
    var @base = checkptrBase(Δp);
    if (@base == 0) {
        return;
    }

    foreach (var (_, original) in originals) {
        if (@base == checkptrBase(original)) {
            return;
        }
    }

    @throw(checkptrPointerˢ2);
}

// checkptrBase returns the base address for the allocation containing
// the address p.
//
// Importantly, if p1 and p2 point into the same variable, then
// checkptrBase(p1) == checkptrBase(p2). However, the converse/inverse
// is not necessarily true as allocations can have trailing padding,
// and multiple variables may be packed into a single allocation.
//
// checkptrBase should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/bytedance/sonic
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname checkptrBase
internal static uintptr checkptrBase(@unsafe.Pointer Δp) {
    // stack
    {
        var gp = getg(); if ((~gp).stack.lo <= (uintptr)Δp && (uintptr)Δp < (~gp).stack.hi) {
            // TODO(mdempsky): Walk the stack to identify the
            // specific stack frame or even stack object that p
            // points into.
            //
            // In the mean time, use "1" as a pseudo-address to
            // represent the stack. This is an invalid address on
            // all platforms, so it's guaranteed to be distinct
            // from any of the addresses we might return below.
            return 1;
        }
    }

    // heap (must check after stack because of #35068)
    {
        var (@base, _, _) = findObject((uintptr)Δp, 0, 0); if (@base != 0) {
            return @base;
        }
    }

    // data or bss
    foreach (var (_, datap) in activeModules()) {
        if (datap.data <= (uintptr)Δp && (uintptr)Δp < datap.edata) {
            return datap.data;
        }
        if (datap.bss <= (uintptr)Δp && (uintptr)Δp < datap.ebss) {
            return datap.bss;
        }
    }

    return 0;
}

} // end runtime_package
