// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Hashing algorithm inspired by
// wyhash: https://github.com/wangyi-fudan/wyhash
//go:build amd64 || arm64 || loong64 || mips64 || mips64le || ppc64 || ppc64le || riscv64 || s390x || wasm
namespace go;

using Δmath = runtime.@internal.math_package;
using @unsafe = unsafe_package;
using runtime.@internal;

partial class runtime_package {

internal static UntypedInt m5 => 0x1d8e4e27c47d124f;

internal static uintptr memhashFallback(@unsafe.Pointer Δp, uintptr seed, uintptr s) {
    uintptr a = default!;
    uintptr b = default!;
    seed ^= (uintptr)(hashkey[0]);
    switch (ᐧ) {
    case {} when s == 0: {
        return seed;
    }
    case {} when s < 4: {
        a = (uintptr)(~(ж<byte>)(uintptr)(Δp));
        a |= (uintptr)(((uintptr)(~(ж<byte>)(uintptr)(add(Δp, (s >> (int)(1))))) << (int)(8)));
        a |= (uintptr)(((uintptr)(~(ж<byte>)(uintptr)(add(Δp, s - 1))) << (int)(16)));
        break;
    }
    case {} when s == 4: {
        a = r4(Δp);
        b = a;
        break;
    }
    case {} when s < 8: {
        a = r4(Δp);
        b = r4((uintptr)add(Δp, s - 4));
        break;
    }
    case {} when s == 8: {
        a = r8(Δp);
        b = a;
        break;
    }
    case {} when s <= 16: {
        a = r8(Δp);
        b = r8((uintptr)add(Δp, s - 8));
        break;
    }
    default: {
        var l = s;
        if (l > 48) {
            var seed1 = seed;
            var seed2 = seed;
            for (; l > 48; l -= 48) {
                seed = mix((uintptr)(r8(Δp) ^ hashkey[1]), (uintptr)(r8((uintptr)add(Δp, 8)) ^ seed));
                seed1 = mix((uintptr)(r8((uintptr)add(Δp, 16)) ^ hashkey[2]), (uintptr)(r8((uintptr)add(Δp, 24)) ^ seed1));
                seed2 = mix((uintptr)(r8((uintptr)add(Δp, 32)) ^ hashkey[3]), (uintptr)(r8((uintptr)add(Δp, 40)) ^ seed2));
                Δp.Value = (uintptr)add(Δp, 48);
            }
            seed ^= (uintptr)((uintptr)(seed1 ^ seed2));
        }
        for (; l > 16; l -= 16) {
            seed = mix((uintptr)(r8(Δp) ^ hashkey[1]), (uintptr)(r8((uintptr)add(Δp, 8)) ^ seed));
            Δp.Value = (uintptr)add(Δp, 16);
        }
        a = r8((uintptr)add(Δp, l - 16));
        b = r8((uintptr)add(Δp, l - 8));
        break;
    }}

    return mix((uintptr)((uintptr)m5 ^ s), mix((uintptr)(a ^ hashkey[1]), (uintptr)(b ^ seed)));
}

internal static uintptr memhash32Fallback(@unsafe.Pointer Δp, uintptr seed) {
    var a = r4(Δp);
    return mix((uintptr)((uintptr)m5 ^ 4), mix((uintptr)(a ^ hashkey[1]), (uintptr)((uintptr)(a ^ seed) ^ hashkey[0])));
}

internal static uintptr memhash64Fallback(@unsafe.Pointer Δp, uintptr seed) {
    var a = r8(Δp);
    return mix((uintptr)((uintptr)m5 ^ 8), mix((uintptr)(a ^ hashkey[1]), (uintptr)((uintptr)(a ^ seed) ^ hashkey[0])));
}

internal static uintptr mix(uintptr a, uintptr b) {
    var (hi, lo) = Δmath.Mul64((uint64)a, (uint64)b);
    return (uintptr)((uint64)(hi ^ lo));
}

internal static uintptr r4(@unsafe.Pointer Δp) {
    return (uintptr)readUnaligned32(Δp);
}

internal static uintptr r8(@unsafe.Pointer Δp) {
    return (uintptr)readUnaligned64(Δp);
}

} // end runtime_package
