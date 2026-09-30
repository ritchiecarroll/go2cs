// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build purego
namespace go.hash;

using rand = crypto.rand_package;
using errors = errors_package;
using byteorder = @internal.byteorder_package;
using bits = go.math.bits_package;
using reflect = reflect_package;
using @internal;
using crypto;
using go.math;

partial class maphash_package {

internal const bool purego = true;

internal static array<uint64> hashkey = new(4);

[GoInit] internal static void init() {
    foreach (var (i, _) in hashkey) {
        hashkey[i] = randUint64();
    }
}

internal static uint64 rthash(slice<byte> buf, uint64 seed) {
    if (len(buf) == 0) {
        return seed;
    }
    return wyhash(buf, seed, (uint64)len(buf));
}

internal static uint64 rthashString(@string s, uint64 state) {
    return rthash(slice<byte>(s), state);
}

internal static uint64 randUint64() {
    var buf = new slice<byte>(8);
    (_, _) = rand.Read(buf);
    return byteorder.LEUint64(buf);
}

// This is a port of wyhash implementation in runtime/hash64.go,
// without using unsafe for purego.
internal static UntypedInt m5 => 0x1d8e4e27c47d124f;

internal static uint64 wyhash(slice<byte> key, uint64 seed, uint64 len) {
    var p = key;
    var i = len;
    uint64 a = default!;
    uint64 b = default!;
    seed ^= (uint64)(hashkey[0]);
    if (i > 16) {
        if (i > 48) {
            var seed1 = seed;
            var seed2 = seed;
            for (; i > 48; i -= 48) {
                seed = mix((uint64)(r8(p) ^ hashkey[1]), (uint64)(r8(p[8..]) ^ seed));
                seed1 = mix((uint64)(r8(p[16..]) ^ hashkey[2]), (uint64)(r8(p[24..]) ^ seed1));
                seed2 = mix((uint64)(r8(p[32..]) ^ hashkey[3]), (uint64)(r8(p[40..]) ^ seed2));
                p = p[48..];
            }
            seed ^= (uint64)((uint64)(seed1 ^ seed2));
        }
        for (; i > 16; i -= 16) {
            seed = mix((uint64)(r8(p) ^ hashkey[1]), (uint64)(r8(p[8..]) ^ seed));
            p = p[16..];
        }
    }
    switch (ᐧ) {
    case {} when i is 0: {
        return seed;
    }
    case {} when i is < 4: {
        a = r3(p, i);
        break;
    }
    default: {
        var n = (((i >> (int)(3))) << (int)(2));
        a = (uint64)((r4(p) << (int)(32)) | r4(p.slice((nint)(n))));
        b = (uint64)((r4(p.slice((nint)(i - 4))) << (int)(32)) | r4(p.slice((nint)(i - 4 - n))));
        break;
    }}

    return mix((uint64)((uint64)m5 ^ len), mix((uint64)(a ^ hashkey[1]), (uint64)(b ^ seed)));
}

internal static uint64 r3(slice<byte> p, uint64 k) {
    return (uint64)((uint64)((((uint64)p[0] << (int)(16))) | (((uint64)p[(k >> (int)(1))] << (int)(8)))) | (uint64)p[k - 1]);
}

internal static uint64 r4(slice<byte> p) {
    return (uint64)byteorder.LEUint32(p);
}

internal static uint64 r8(slice<byte> p) {
    return byteorder.LEUint64(p);
}

internal static uint64 mix(uint64 a, uint64 b) {
    // Fully qualified to avoid alias shadowing by the same-package test declaration "bits".
    var (hi, lo) = go.math.bits_package.Mul64(a, b);
    return (uint64)(hi ^ lo);
}

internal static uint64 comparableHash<T>(T v, ΔSeed seed) {
    ref var h = ref heap(new Hash(), out var Ꮡh);
    h.SetSeed(seed);
    writeComparable(Ꮡh, v);
    return h.Sum64();
}

internal static void writeComparable<T>(ж<Hash> Ꮡh, T v) {
    var vv = reflect.ValueOf(v);
    appendT(Ꮡh, vv);
}

// appendT hash a value.
internal static void appendT(ж<Hash> Ꮡh, reflectꓸValue v) {
    ref var h = ref Ꮡh.DerefOrNull();

    h.WriteString(v.Type().String());
    var exprᴛ1 = v.Kind();
    if (exprᴛ1 == reflect.Int8 || exprᴛ1 == reflect.Int16 || exprᴛ1 == reflect.Int32 || exprᴛ1 == reflect.Int64 || exprᴛ1 == reflect.ΔInt) {
        array<byte> buf = new(8);
        byteorder.LEPutUint64(buf[..], (uint64)v.Int());
        h.Write(buf[..]);
        return;
    }
    if (exprᴛ1 == reflect.Uint8 || exprᴛ1 == reflect.Uint16 || exprᴛ1 == reflect.Uint32 || exprᴛ1 == reflect.Uint64 || exprᴛ1 == reflect.ΔUint || exprᴛ1 == reflect.Uintptr) {
        array<byte> buf = new(8);
        byteorder.LEPutUint64(buf[..], v.Uint());
        h.Write(buf[..]);
        return;
    }
    if (exprᴛ1 == reflect.Array) {
        array<byte> buf = new(8);
        foreach (var i in range<uint64>((uint64)v.Len())) {
            byteorder.LEPutUint64(buf[..], i);
            // do not want to hash to the same value,
            // [2]string{"foo", ""} and [2]string{"", "foo"}.
            h.Write(buf[..]);
            appendT(Ꮡh, v.Index((nint)i));
        }
        return;
    }
    if (exprᴛ1 == reflect.ΔString) {
        h.WriteString(v.String());
        return;
    }
    if (exprᴛ1 == reflect.Struct) {
        array<byte> buf = new(8);
        foreach (var i in range(v.NumField())) {
            var f = v.Field(i);
            byteorder.LEPutUint64(buf[..], (uint64)i);
            // do not want to hash to the same value,
            // struct{a,b string}{"foo",""} and
            // struct{a,b string}{"","foo"}.
            h.Write(buf[..]);
            appendT(Ꮡh, f);
        }
        return;
    }
    if (exprᴛ1 == reflect.Complex64 || exprᴛ1 == reflect.Complex128) {
        var c = v.Complex();
        h.float64(real(c));
        h.float64(imag(c));
        return;
    }
    if (exprᴛ1 == reflect.Float32 || exprᴛ1 == reflect.Float64) {
        h.float64(v.Float());
        return;
    }
    if (exprᴛ1 == reflect.ΔBool) {
        h.WriteByte(btoi(v.Bool()));
        return;
    }
    if (exprᴛ1 == reflect.ΔUnsafePointer || exprᴛ1 == reflect.ΔPointer || exprᴛ1 == reflect.Chan) {
        array<byte> buf = new(8);
        byteorder.LEPutUint64(buf[..], // because pointing to the abi.Escape call in comparableReady,
 // So this is ok to hash pointer,
 // this way because we know their target won't be moved.
 (uint64)v.Pointer());
        h.Write(buf[..]);
        return;
    }
    if (exprᴛ1 == reflect.ΔInterface) {
        appendT(Ꮡh, v.Elem());
        return;
    }

    throw panic(errors.New("maphash: hash of unhashable type "u8 + v.Type().String()));
}

} // end maphash_package
