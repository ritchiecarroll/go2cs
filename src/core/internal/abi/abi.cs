// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using goarch = go.@internal.goarch_package;
using @unsafe = unsafe_package;
using go.@internal;

partial class abi_package {

// RegArgs is a struct that has space for each argument
// and return value register on the current architecture.
//
// Assembly code knows the layout of the first two fields
// of RegArgs.
//
// RegArgs also contains additional space to hold pointers
// when it may not be safe to keep them only in the integer
// register space otherwise.
[GoType] partial struct RegArgs {
    // Values in these slots should be precisely the bit-by-bit
    // representation of how they would appear in a register.
    //
    // This means that on big endian arches, integer values should
    // be in the top bits of the slot. Floats are usually just
    // directly represented, but some architectures treat narrow
    // width floating point values specially (e.g. they're promoted
    // first, or they need to be NaN-boxed).
    public array<uintptr> Ints = new(IntArgRegs); // untyped integer registers
    public array<uint64> Floats = new(FloatArgRegs); // untyped float registers
// Fields above this point are known to assembly.

    // Ptrs is a space that duplicates Ints but with pointer type,
    // used to make pointers passed or returned  in registers
    // visible to the GC by making the type unsafe.Pointer.
    public array<@unsafe.Pointer> Ptrs = new(IntArgRegs);
    // ReturnIsPtr is a bitmap that indicates which registers
    // contain or will contain pointers on the return path from
    // a reflectcall. The i'th bit indicates whether the i'th
    // register contains or will contain a valid Go pointer.
    public IntArgRegBitmap ReturnIsPtr;
}

[GoRecv] public static void Dump(this ref RegArgs r) {
    print((@string)"Ints:"u8);
    foreach (var (_, x) in r.Ints.ΔRangeSnapshot()) {
        print((@string)" "u8, x);
    }
    println();
    print((@string)"Floats:"u8);
    foreach (var (_, x) in r.Floats.ΔRangeSnapshot()) {
        print((@string)" "u8, x);
    }
    println();
    print((@string)"Ptrs:"u8);
    foreach (var (_, x) in r.Ptrs.ΔRangeSnapshot()) {
        print((@string)" "u8, x);
    }
    println();
}

// IntRegArgAddr returns a pointer inside of r.Ints[reg] that is appropriately
// offset for an argument of size argSize.
//
// argSize must be non-zero, fit in a register, and a power-of-two.
//
// This method is a helper for dealing with the endianness of different CPU
// architectures, since sub-word-sized arguments in big endian architectures
// need to be "aligned" to the upper edge of the register to be interpreted
// by the CPU correctly.
[GoRecv] public static @unsafe.Pointer IntRegArgAddr(this ref RegArgs r, nint reg, uintptr argSize) {
    if (argSize > goarch.PtrSize || argSize == 0 || (uintptr)(argSize & (argSize - 1)) != 0) {
        throw panic("invalid argSize");
    }
    var offset = (uintptr)0;
    if (goarch.BigEndian) {
        offset = (uintptr)goarch.PtrSize - argSize;
    }
    return (@unsafe.Pointer)((uintptr)@unsafe.Pointer.FromBox(Ꮡ(r.Ints, reg)) + offset);
}

[GoType("[2]uint8")] /* [(IntArgRegs + 7) / 8]uint8 */
partial struct IntArgRegBitmap;

// Set sets the i'th bit of the bitmap to 1.
[GoRecv] public static void Set(this ref IntArgRegBitmap b, nint i) {
    b.Value[i / 8] |= (uint8)((uint8)((uint8)1 << (int)((i % 8))));
}

// Get returns whether the i'th bit of the bitmap is set.
//
// nosplit because it's called in extremely sensitive contexts, like
// on the reflectcall return path.
//
//go:nosplit
[GoRecv] public static bool Get(this ref IntArgRegBitmap b, nint i) {
    return (uint8)(b.Value[i / 8] & ((uint8)((uint8)1 << (int)((i % 8))))) != 0;
}

} // end abi_package
