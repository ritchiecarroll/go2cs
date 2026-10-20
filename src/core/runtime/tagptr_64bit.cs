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
// arch/os combos except s390x, aix, and riscv64.
//
// On AMD64, virtual addresses are 48-bit (or 57-bit) sign-extended.
// Other archs are 48-bit zero-extended.
//
// We use one extra bit to placate systems which simulate amd64 binaries on
// an arm64 host. Allocated arm64 addresses could be as high as 1<<48-1,
// which would be invalid if we assumed 48-bit sign-extended addresses.
// See issue 69255.
// (Note that this does not help the other way around, simluating arm64
// on amd64, but we don't have that problem at the moment.)
//
// On s390x, virtual addresses are 64-bit. There's not much we
// can do about this, so we just hope that the kernel doesn't
// get to really high addresses and panic if it does.
internal static UntypedInt defaultAddrBits => /* 48 + 1 */ 49;

// On AIX, 64-bit addresses are split into 36-bit segment number and 28-bit
// offset in segment.  Segment numbers in the range 0x0A0000000-0x0AFFFFFFF(LSA)
// are available for mmap.
// We assume all tagged addresses are from memory allocated with mmap.
// We use one bit to distinguish between the two ranges.
internal static UntypedInt aixAddrBits => 57;

// Later versions of FreeBSD enable amd64's la57 by default.
internal static UntypedInt freebsdAmd64AddrBits => 57;

// riscv64 SV57 mode gives 56 bits of userspace VA.
// tagged pointer code supports it,
// but broader support for SV57 mode is incomplete,
// and there may be other issues (see #54104).
internal static UntypedInt riscv64AddrBits => 56;

internal static UntypedInt addrBits => /* goos.IsAix*aixAddrBits + goarch.IsRiscv64*riscv64AddrBits + goos.IsFreebsd*goarch.IsAmd64*freebsdAmd64AddrBits + (1-goos.IsAix)*(1-goarch.IsRiscv64)*(1-goos.IsFreebsd*goarch.IsAmd64)*defaultAddrBits */ 49;

// In addition to the 16 bits (or other, depending on arch/os) taken from the top,
// we can take 9 from the bottom, because we require pointers to be well-aligned
// (see tagptr.go:tagAlignBits). That gives us a total of 25 bits for the tag.
internal static UntypedInt tagBits => /* 64 - addrBits + tagAlignBits */ 24;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string taggedPointerPackˢ = "taggedPointerPack"u8;

// taggedPointerPack created a taggedPointer from a pointer and a tag.
// Tag bits that don't fit in the result are discarded.
internal static taggedPointer taggedPointerPack(@unsafe.Pointer ptr, uintptr tag) {
    var t = (taggedPointer)((uint64)(uintptr)ptr << (int)(tagBits - tagAlignBits) | (uint64)((uintptr)(tag & (uintptr)((1 << (int)(tagBits)) - 1))));
    if ((uintptr)t.pointer() != ptr || t.tag() != tag) {
        print((@string)"runtime: taggedPointerPack invalid packing: ptr="u8, @unsafe.Pointer.OrTypedNil(ptr), (@string)" tag="u8, (Δhex)(uint64)tag, (@string)" packed="u8, (Δhex)(uint64)t, (@string)" -> ptr="u8, @unsafe.Pointer.OrTypedNil((uintptr)t.pointer()), (@string)" tag="u8, (Δhex)(uint64)t.tag(), (@string)"\n"u8);
        @throw(taggedPointerPackˢ);
    }
    return t;
}

// Pointer returns the pointer from a taggedPointer.
internal static @unsafe.Pointer pointer(this taggedPointer tp) {
    if (GOARCH == "amd64"u8) {
        // amd64 systems can place the stack above the VA hole, so we need to sign extend
        // val before unpacking.
        return (@unsafe.Pointer)(uintptr)(((int64)(uint64)tp >> (int)(tagBits)) << (int)(tagAlignBits));
    }
    if (GOOS == "aix"u8) {
        return (@unsafe.Pointer)(uintptr)(uint64)((taggedPointer)(((tp >> (int)(tagBits)) << (int)(tagAlignBits)) | (taggedPointer)((uint64)0xa << 56)));
    }
    return (@unsafe.Pointer)(uintptr)(uint64)(((tp >> (int)(tagBits)) << (int)(tagAlignBits)));
}

// Tag returns the tag from a taggedPointer.
internal static uintptr tag(this taggedPointer tp) {
    return (uintptr)(uint64)((taggedPointer)(tp & (uint64)((1 << (int)(tagBits)) - 1)));
}

} // end runtime_package
