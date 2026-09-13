// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δmath = runtime.@internal.math_package;
using @unsafe = unsafe_package;
using runtime.@internal;

partial class runtime_package {

internal static void unsafestring(@unsafe.Pointer ptr, nint len) {
    if (len < 0) {
        panicunsafestringlen();
    }
    if ((uintptr)len > ((uintptr)0 - (uintptr)ptr)) {
        if (ptr == nil) {
            panicunsafestringnilptr();
        }
        panicunsafestringlen();
    }
}

// Keep this code in sync with cmd/compile/internal/walk/builtin.go:walkUnsafeString
internal static void unsafestring64(@unsafe.Pointer ptr, int64 len64) {
    nint len = (nint)len64;
    if ((int64)len != len64) {
        panicunsafestringlen();
    }
    unsafestring(ptr, len);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string checkptrUnsafeStringˢ = "checkptr: unsafe.String result straddles multiple allocations"u8;

internal static void unsafestringcheckptr(@unsafe.Pointer ptr, int64 len64) {
    unsafestring64(ptr, len64);
    // Check that underlying array doesn't straddle multiple heap objects.
    // unsafestring64 has already checked for overflow.
    if (checkptrStraddles(ptr, (uintptr)len64)) {
        @throw(checkptrUnsafeStringˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsafeStringLenOutOfˢ = "unsafe.String: len out of range"u8;

internal static void panicunsafestringlen() {
    throw panic(((errorString)(@string)unsafeStringLenOutOfˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsafeStringPtrIsNilAndˢ = "unsafe.String: ptr is nil and len is not zero"u8;

internal static void panicunsafestringnilptr() {
    throw panic(((errorString)(@string)unsafeStringPtrIsNilAndˢ));
}

// Keep this code in sync with cmd/compile/internal/walk/builtin.go:walkUnsafeSlice
internal static void unsafeslice(ref _type et, @unsafe.Pointer ptr, nint len) {
    if (len < 0) {
        panicunsafeslicelen1(getcallerpc());
    }
    if (et.Size_ == 0) {
        if (ptr == nil && len > 0) {
            panicunsafeslicenilptr1(getcallerpc());
        }
    }
    var (mem, overflow) = Δmath.MulUintptr(et.Size_, (uintptr)len);
    if (overflow || mem > ((uintptr)0 - (uintptr)ptr)) {
        if (ptr == nil) {
            panicunsafeslicenilptr1(getcallerpc());
        }
        panicunsafeslicelen1(getcallerpc());
    }
}

// Keep this code in sync with cmd/compile/internal/walk/builtin.go:walkUnsafeSlice
internal static void unsafeslice64(ref _type et, @unsafe.Pointer ptr, int64 len64) {
    nint len = (nint)len64;
    if ((int64)len != len64) {
        panicunsafeslicelen1(getcallerpc());
    }
    unsafeslice(ref et, ptr, len);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string checkptrUnsafeSliceˢ = "checkptr: unsafe.Slice result straddles multiple allocations"u8;

internal static void unsafeslicecheckptr(ref _type et, @unsafe.Pointer ptr, int64 len64) {
    unsafeslice64(ref et, ptr, len64);
    // Check that underlying array doesn't straddle multiple heap objects.
    // unsafeslice64 has already checked for overflow.
    if (checkptrStraddles(ptr, (uintptr)len64 * et.Size_)) {
        @throw(checkptrUnsafeSliceˢ);
    }
}

internal static void panicunsafeslicelen() {
    // This is called only from compiler-generated code, so we can get the
    // source of the panic.
    panicunsafeslicelen1(getcallerpc());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsafeSliceLenOutOfRangeˢ = "unsafe.Slice: len out of range"u8;

//go:yeswritebarrierrec
internal static void panicunsafeslicelen1(uintptr pc) {
    panicCheck1(pc, unsafeSliceLenOutOfRangeˢ);
    throw panic(((errorString)(@string)unsafeSliceLenOutOfRangeˢ));
}

internal static void panicunsafeslicenilptr() {
    // This is called only from compiler-generated code, so we can get the
    // source of the panic.
    panicunsafeslicenilptr1(getcallerpc());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unsafeSlicePtrIsNilAndˢ = "unsafe.Slice: ptr is nil and len is not zero"u8;

//go:yeswritebarrierrec
internal static void panicunsafeslicenilptr1(uintptr pc) {
    panicCheck1(pc, unsafeSlicePtrIsNilAndˢ);
    throw panic(((errorString)(@string)unsafeSlicePtrIsNilAndˢ));
}

//go:linkname reflect_unsafeslice reflect.unsafeslice
internal static void reflect_unsafeslice(ref _type et, @unsafe.Pointer ptr, nint len) {
    unsafeslice(ref et, ptr, len);
}

} // end runtime_package
