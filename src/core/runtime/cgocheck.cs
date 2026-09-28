// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Code to check that pointer writes follow the cgo rules.
// These functions are invoked when GOEXPERIMENT=cgocheck2 is enabled.
namespace go;

using goarch = @internal.goarch_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

internal static readonly @string cgoWriteBarrierFail = "unpinned Go pointer stored into non-Go memory"u8;

// cgoCheckPtrWrite is called whenever a pointer is stored into memory.
// It throws if the program is storing an unpinned Go pointer into non-Go
// memory.
//
// This is called from generated code when GOEXPERIMENT=cgocheck2 is enabled.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckPtrWrite(ж<@unsafe.Pointer> Ꮡdst, @unsafe.Pointer src) {
    ref var dst = ref Ꮡdst.DerefOrNull();

    if (!mainStarted) {
        // Something early in startup hates this function.
        // Don't start doing any actual checking until the
        // runtime has set itself up.
        return;
    }
    if (!cgoIsGoPointer(src)) {
        return;
    }
    if (cgoIsGoPointer(@unsafe.Pointer.FromPinnedBox(Ꮡdst))) {
        return;
    }
    // If we are running on the system stack then dst might be an
    // address on the stack, which is OK.
    var gp = getg();
    if (gp == (~(~gp).m).g0 || gp == (~(~gp).m).gsignal) {
        return;
    }
    // Allocating memory can write to various mfixalloc structs
    // that look like they are non-Go memory.
    if ((~(~gp).m).mallocing != 0) {
        return;
    }
    // If the object is pinned, it's safe to store it in C memory. The GC
    // ensures it will not be moved or freed.
    if (isPinned(src)) {
        return;
    }
    // It's OK if writing to memory allocated by persistentalloc.
    // Do this check last because it is more expensive and rarely true.
    // If it is false the expense doesn't matter since we are crashing.
    if (inPersistentAlloc((uintptr)Ꮡdst)) {
        return;
    }
    systemstack(() => {
        println((@string)"write of unpinned Go pointer"u8, ((Δhex)(uint64)(uintptr)src), (@string)"to non-Go memory"u8, ((Δhex)(uint64)(uintptr)Ꮡdst));
        @throw(cgoWriteBarrierFail);
    });
}

// cgoCheckMemmove is called when moving a block of memory.
// It throws if the program is copying a block that contains an unpinned Go
// pointer into non-Go memory.
//
// This is called from generated code when GOEXPERIMENT=cgocheck2 is enabled.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckMemmove(ж<_type> Ꮡtyp, @unsafe.Pointer dst, @unsafe.Pointer src) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    cgoCheckMemmove2(Ꮡtyp, dst, src, 0, typ.Size_);
}

// cgoCheckMemmove2 is called when moving a block of memory.
// dst and src point off bytes into the value to copy.
// size is the number of bytes to copy.
// It throws if the program is copying a block that contains an unpinned Go
// pointer into non-Go memory.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckMemmove2(ж<_type> Ꮡtyp, @unsafe.Pointer dst, @unsafe.Pointer src, uintptr off, uintptr size) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (!typ.Pointers()) {
        return;
    }
    if (!cgoIsGoPointer(src)) {
        return;
    }
    if (cgoIsGoPointer(dst)) {
        return;
    }
    cgoCheckTypedBlock(Ꮡtyp, src, off, size);
}

// cgoCheckSliceCopy is called when copying n elements of a slice.
// src and dst are pointers to the first element of the slice.
// typ is the element type of the slice.
// It throws if the program is copying slice elements that contain unpinned Go
// pointers into non-Go memory.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckSliceCopy(ж<_type> Ꮡtyp, @unsafe.Pointer dst, @unsafe.Pointer src, nint n) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (!typ.Pointers()) {
        return;
    }
    if (!cgoIsGoPointer(src)) {
        return;
    }
    if (cgoIsGoPointer(dst)) {
        return;
    }
    @unsafe.Pointer Δp = src;
    for (nint i = 0; i < n; i++) {
        cgoCheckTypedBlock(Ꮡtyp, Δp, 0, typ.Size_);
        Δp = (uintptr)add(Δp, typ.Size_);
    }
}

// cgoCheckTypedBlock checks the block of memory at src, for up to size bytes,
// and throws if it finds an unpinned Go pointer. The type of the memory is typ,
// and src is off bytes into that type.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckTypedBlock(ж<_type> Ꮡtyp, @unsafe.Pointer src, uintptr off, uintptr size) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    // Anything past typ.PtrBytes is not a pointer.
    if (typ.PtrBytes <= off) {
        return;
    }
    {
        var ptrdataSize = typ.PtrBytes - off; if (size > ptrdataSize) {
            size = ptrdataSize;
        }
    }
    cgoCheckBits(src, getGCMask(Ꮡtyp), off, size);
}

// cgoCheckBits checks the block of memory at src, for up to size
// bytes, and throws if it finds an unpinned Go pointer. The gcbits mark each
// pointer value. The src pointer is off bytes into the gcbits.
//
//go:nosplit
//go:nowritebarrier
internal static void cgoCheckBits(@unsafe.Pointer src, ж<byte> Ꮡgcbits, uintptr off, uintptr size) {
    var skipMask = off / (uintptr)goarch.PtrSize / 8;
    var skipBytes = skipMask * (uintptr)goarch.PtrSize * 8;
    var ptrmask = addb(Ꮡgcbits, skipMask);
    src = (uintptr)add(src, skipBytes);
    off -= skipBytes;
    size += off;
    uint32 bits = default!;
    for (var i = (uintptr)0; i < size; i += goarch.PtrSize) {
        if ((uintptr)(i & (uintptr)(goarch.PtrSize * 8 - 1)) == 0){
            bits = (uint32)(ptrmask.Value);
            ptrmask = addb(ptrmask, 1);
        } else {
            bits >>= (int)(1);
        }
        if (off > 0){
            off -= goarch.PtrSize;
        } else {
            if ((uint32)(bits & 1) != 0) {
                @unsafe.Pointer v = ~(ж<@unsafe.Pointer>)(uintptr)((uintptr)add(src, i));
                if (cgoIsGoPointer(v) && !isPinned(v)) {
                    @throw(cgoWriteBarrierFail);
                }
            }
        }
    }
}

// cgoCheckUsingType is like cgoCheckTypedBlock, but is a last ditch
// fall back to look for pointers in src using the type information.
// We only use this when looking at a value on the stack when the type
// uses a GC program, because otherwise it's more efficient to use the
// GC bits. This is called on the system stack.
//
//go:nowritebarrier
//go:systemstack
internal static void cgoCheckUsingType(ж<_type> Ꮡtyp, @unsafe.Pointer src, uintptr off, uintptr size) {
    ref var typ = ref Ꮡtyp.DerefOrNull();

    if (!typ.Pointers()) {
        return;
    }
    // Anything past typ.PtrBytes is not a pointer.
    if (typ.PtrBytes <= off) {
        return;
    }
    {
        var ptrdataSize = typ.PtrBytes - off; if (size > ptrdataSize) {
            size = ptrdataSize;
        }
    }
    cgoCheckBits(src, getGCMask(Ꮡtyp), off, size);
}

} // end runtime_package
