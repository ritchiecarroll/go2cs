// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !arm64 && !loong64 && !ppc64 && !ppc64le) || purego

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted xor_generic.go output).
// Everything here except the word-at-a-time step is the converted output verbatim. That one step
// cannot work as literally converted: Go reinterprets the three byte slices as []uintptr and XORs a
// WORD at a time —
//
//     func words(x []byte) []uintptr {
//         return unsafe.Slice((*uintptr)(unsafe.Pointer(&x[0])), uintptr(len(x))/wordSize)
//     }
//
// — and a `uintptr[]` view over a `byte[]` does not exist in the managed model. golib's `slice<T>` is
// a window on a real `T[]`, so the converted `words()` could only SNAPSHOT the bytes into a detached
// `slice<uintptr>`; the word loop then XORed the snapshot and dropped it. For every length that is a
// multiple of 8 `XORBytes` therefore wrote NOTHING (TestXORBytes compared dst against its untouched
// 0xdd fill), and for other lengths only the trailing `n % 8` bytes landed. This is the raw-metal /
// memory-layout fork documented in src/archived/Baseline-vs-FullConversion.md — the reinterpret is not
// convertible, so the declaration is hand-owned.
//
// xorWords below does the same reinterpret the managed way: MemoryMarshal.Cast over the slices' own
// spans is a genuine ALIASING view of the same backing storage, so the word writes land in place. It
// keeps Go's word-at-a-time behavior, and with it the performance contract crypto/cipher's CTR and
// GCM modes depend on. `words` is gone with the pointer reinterpret it existed to express; the
// generic `xorLoop` stays for the trailing partial word, exactly as Go uses it.

using System.Runtime.InteropServices;
using go;

[module: GoManualConversion]

namespace go.crypto.@internal.fips140;

using runtime = runtime_package;
using @unsafe = unsafe_package;

partial class subtle_package {

internal static readonly uintptr wordSize = /* unsafe.Sizeof(uintptr(0)) */ 8;

internal const bool supportsUnaligned = /* runtime.GOARCH == "386" ||
	runtime.GOARCH == "amd64" ||
	runtime.GOARCH == "ppc64" ||
	runtime.GOARCH == "ppc64le" ||
	runtime.GOARCH == "s390x" */ true;

internal static void xorBytes(ж<byte> Ꮡdstb, ж<byte> Ꮡxb, ж<byte> Ꮡyb, nint n) {
    // xorBytes assembly is written using pointers and n. Back to slices.
    var dst = @unsafe.Slice(Ꮡdstb, n);
    var x = @unsafe.Slice(Ꮡxb, n);
    var y = @unsafe.Slice(Ꮡyb, n);
    if (supportsUnaligned || aligned(Ꮡdstb, Ꮡxb, Ꮡyb)) {
        xorWords(dst, x, y);
        if ((uintptr)n % wordSize == 0) {
            return;
        }
        nint done = (nint)(n & ~(nint)(nint)(wordSize - 1));
        dst = dst[(int)(done)..];
        x = x[(int)(done)..];
        y = y[(int)(done)..];
    }
    xorLoop(dst, x, y);
}

// xorWords XORs whole machine words in place — Go's `xorLoop(words(dst), words(x), words(y))`. The
// cast reinterprets each slice's own span, dropping any trailing partial word exactly as `words()`
// does, and needs no alignment test: Go's supportsUnaligned/aligned gate exists for architectures
// whose unaligned word loads fault, which the CLR's targets do not (and on the arch this file is
// built for, supportsUnaligned is already the constant true).
internal static void xorWords(slice<byte> dst, slice<byte> x, slice<byte> y) {
    Span<uint64> dstWords = MemoryMarshal.Cast<byte, uint64>(dst.ToSpan());
    Span<uint64> xWords = MemoryMarshal.Cast<byte, uint64>(x.ToSpan());
    Span<uint64> yWords = MemoryMarshal.Cast<byte, uint64>(y.ToSpan());
    xWords = xWords[..dstWords.Length];
    // remove bounds check in loop
    yWords = yWords[..dstWords.Length];
    // remove bounds check in loop
    for (int i = 0; i < dstWords.Length; i++) {
        dstWords[i] = xWords[i] ^ yWords[i];
    }
}

// aligned reports whether dst, x, and y are all word-aligned pointers.
internal static bool aligned(ж<byte> Ꮡdst, ж<byte> Ꮡx, ж<byte> Ꮡy) {
    return (uintptr)(((uintptr)((uintptr)((uintptr)new @unsafe.Pointer(Ꮡdst) | (uintptr)new @unsafe.Pointer(Ꮡx)) | (uintptr)new @unsafe.Pointer(Ꮡy))) & (uintptr)(wordSize - 1)) == 0;
}

internal static void xorLoop<T>(slice<T> dst, slice<T> x, slice<T> y)
    where T : /* byte | uintptr */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    x = x[..(int)(len(dst))];
    // remove bounds check in loop
    y = y[..(int)(len(dst))];
    // remove bounds check in loop
    foreach (var i in range(dst)) {
        dst[i] = (T)(x[i] ^ y[i]);
    }
}

} // end subtle_package
