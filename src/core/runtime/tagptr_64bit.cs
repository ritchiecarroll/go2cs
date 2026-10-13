// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build amd64 || arm64 || loong64 || mips64 || mips64le || ppc64 || ppc64le || riscv64 || s390x || wasm
namespace go;

using goarch = @internal.goarch_package;
using goos = @internal.goos_package;
using @unsafe = unsafe_package;
using @internal;

partial class runtime_package {

// addrBits is the number of bits needed to represent a virtual address.
//
// See heapAddrBits for a table of address space sizes on
// various architectures. 48 bits is enough for all
// architectures except s390x.
//
// On AMD64, virtual addresses are 48-bit (or 57-bit) numbers sign extended to 64.
// We shift the address left 16 to eliminate the sign extended part and make
// room in the bottom for the count.
//
// On s390x, virtual addresses are 64-bit. There's not much we
// can do about this, so we just hope that the kernel doesn't
// get to really high addresses and panic if it does.
internal static UntypedInt addrBits => 48;

// In addition to the 16 bits taken from the top, we can take 3 from the
// bottom, because node must be pointer-aligned, giving a total of 19 bits
// of count.
internal static UntypedInt tagBits => /* 64 - addrBits + 3 */ 19;

// On AIX, 64-bit addresses are split into 36-bit segment number and 28-bit
// offset in segment.  Segment numbers in the range 0x0A0000000-0x0AFFFFFFF(LSA)
// are available for mmap.
// We assume all tagged addresses are from memory allocated with mmap.
// We use one bit to distinguish between the two ranges.
internal static UntypedInt aixAddrBits => 57;
internal static UntypedInt aixTagBits => /* 64 - aixAddrBits + 3 */ 10;

// riscv64 SV57 mode gives 56 bits of userspace VA.
// tagged pointer code supports it,
// but broader support for SV57 mode is incomplete,
// and there may be other issues (see #54104).
internal static UntypedInt riscv64AddrBits => 56;
internal static UntypedInt riscv64TagBits => /* 64 - riscv64AddrBits + 3 */ 11;

// The number of bits stored in the numeric tag of a taggedPointer
internal static UntypedInt taggedPointerBits => /* (goos.IsAix * aixTagBits) + (goarch.IsRiscv64 * riscv64TagBits) + ((1 - goos.IsAix) * (1 - goarch.IsRiscv64) * tagBits) */ 19;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string checkThisCodeForAixOnNonˢ = "check this code for aix on non-ppc64"u8;

// taggedPointerPack created a taggedPointer from a pointer and a tag.
// Tag bits that don't fit in the result are discarded.
internal static taggedPointer taggedPointerPack(@unsafe.Pointer ptr, uintptr tag) {
    if (GOOS == "aix"u8) {
        if (GOARCH != "ppc64"u8) {
            @throw(checkThisCodeForAixOnNonˢ);
        }
        return ((taggedPointer)((uint64)(((uint64)(uintptr)ptr << (int)((64 - aixAddrBits))) | (uint64)((uintptr)(tag & (uintptr)((1 << (int)(aixTagBits)) - 1))))));
    }
    if (GOARCH == "riscv64"u8) {
        return ((taggedPointer)((uint64)(((uint64)(uintptr)ptr << (int)((64 - riscv64AddrBits))) | (uint64)((uintptr)(tag & (uintptr)((1 << (int)(riscv64TagBits)) - 1))))));
    }
    return ((taggedPointer)((uint64)(((uint64)(uintptr)ptr << (int)((64 - addrBits))) | (uint64)((uintptr)(tag & (uintptr)((1 << (int)(tagBits)) - 1))))));
}

// Pointer returns the pointer from a taggedPointer.
internal static @unsafe.Pointer pointer(this taggedPointer tp) {
    if (GOARCH == "amd64"u8) {
        // amd64 systems can place the stack above the VA hole, so we need to sign extend
        // val before unpacking.
        return (@unsafe.Pointer)(uintptr)((((int64)(uint64)tp >> (int)(tagBits)) << (int)(3)));
    }
    if (GOOS == "aix"u8) {
        return (@unsafe.Pointer)(uintptr)(uint64)((taggedPointer)((((tp >> (int)(aixTagBits)) << (int)(3))) | (taggedPointer)((uint64)0xa << (int)(56))));
    }
    if (GOARCH == "riscv64"u8) {
        return (@unsafe.Pointer)(uintptr)(uint64)(((tp >> (int)(riscv64TagBits)) << (int)(3)));
    }
    return (@unsafe.Pointer)(uintptr)(uint64)(((tp >> (int)(tagBits)) << (int)(3)));
}

// Tag returns the tag from a taggedPointer.
internal static uintptr tag(this taggedPointer tp) {
    return (uintptr)(uint64)((taggedPointer)(tp & (uint64)(((1 << (int)(taggedPointerBits)) - 1))));
}

} // end runtime_package
