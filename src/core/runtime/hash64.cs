// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Hashing algorithm inspired by
// wyhash: https://github.com/wangyi-fudan/wyhash
//go:build amd64 || arm64 || loong64 || mips64 || mips64le || ppc64 || ppc64le || riscv64 || s390x
namespace go;

using Δmath = @internal.runtime.math_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

internal static UntypedInt m5 => 0x1d8e4e27c47d124f;

internal static uintptr memhashFallback(@unsafe.Pointer p, uintptr seed, uintptr s) {
    uintptr a = default;
    uintptr b = default;
    seed ^= (uintptr)(hashkey[0]);
    switch (ᐧ) {
    case {} when s == 0: {
        return seed;
    }
    case {} when s < 4: {
        a = (uintptr)(~@unsafe.Pointer.To<byte>(p));
        a |= (uintptr)(((uintptr)(~@unsafe.Pointer.To<byte>(add(p, s >> 1))) << 8));
        a |= (uintptr)(((uintptr)(~@unsafe.Pointer.To<byte>(add(p, s - 1))) << 16));
        break;
    }
    case {} when s == 4: {
        a = r4(p);
        b = a;
        break;
    }
    case {} when s < 8: {
        a = r4(p);
        b = r4(add(p, s - 4));
        break;
    }
    case {} when s == 8: {
        a = r8(p);
        b = a;
        break;
    }
    case {} when s <= 16: {
        a = r8(p);
        b = r8(add(p, s - 8));
        break;
    }
    default: {
        var l = s;
        if (l > 48) {
            var seed1 = seed;
            var seed2 = seed;
            for (; l > 48; l -= 48) {
                var evalᴛ1 = r8(p);
                var evalᴛ2 = add(p, 8);
                var evalᴛ3 = r8(evalᴛ2);
                seed = mix((uintptr)(evalᴛ1 ^ hashkey[1]), (uintptr)(evalᴛ3 ^ seed));
                var evalᴛ4 = add(p, 16);
                var evalᴛ5 = r8(evalᴛ4);
                var evalᴛ6 = add(p, 24);
                var evalᴛ7 = r8(evalᴛ6);
                seed1 = mix((uintptr)(evalᴛ5 ^ hashkey[2]), (uintptr)(evalᴛ7 ^ seed1));
                var evalᴛ8 = add(p, 32);
                var evalᴛ9 = r8(evalᴛ8);
                var evalᴛ10 = add(p, 40);
                var evalᴛ11 = r8(evalᴛ10);
                seed2 = mix((uintptr)(evalᴛ9 ^ hashkey[3]), (uintptr)(evalᴛ11 ^ seed2));
                p = add(p, 48);
            }
            seed ^= (uintptr)((uintptr)(seed1 ^ seed2));
        }
        for (; l > 16; l -= 16) {
            var evalᴛ12 = r8(p);
            var evalᴛ13 = add(p, 8);
            var evalᴛ14 = r8(evalᴛ13);
            seed = mix((uintptr)(evalᴛ12 ^ hashkey[1]), (uintptr)(evalᴛ14 ^ seed));
            p = add(p, 16);
        }
        a = r8(add(p, l - 16));
        b = r8(add(p, l - 8));
        break;
    }}

    return mix((uintptr)((uintptr)m5 ^ s), mix((uintptr)(a ^ hashkey[1]), (uintptr)(b ^ seed)));
}

internal static uintptr memhash32Fallback(@unsafe.Pointer p, uintptr seed) {
    var a = r4(p);
    return mix((uintptr)((uintptr)m5 ^ 4), mix((uintptr)(a ^ hashkey[1]), (uintptr)((uintptr)(a ^ seed) ^ hashkey[0])));
}

internal static uintptr memhash64Fallback(@unsafe.Pointer p, uintptr seed) {
    var a = r8(p);
    return mix((uintptr)((uintptr)m5 ^ 8), mix((uintptr)(a ^ hashkey[1]), (uintptr)((uintptr)(a ^ seed) ^ hashkey[0])));
}

internal static uintptr mix(uintptr a, uintptr b) {
    var (hi, lo) = Δmath.Mul64((uint64)a, (uint64)b);
    return (uintptr)(hi ^ lo);
}

internal static uintptr r4(@unsafe.Pointer p) {
    return (uintptr)readUnaligned32(p);
}

internal static uintptr r8(@unsafe.Pointer p) {
    return (uintptr)readUnaligned64(p);
}

} // end runtime_package
