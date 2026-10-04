// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using bytes = bytes_package;
using rand = go.crypto.rand_package;
using big = math.big_package;
using mathrand = math.rand.rand_package;
using strconv = strconv_package;
using testing = testing_package;
using go.crypto;
using math;
using static go.crypto.@internal.fips140.mlkem_package;

partial class mlkem_internal_test_package {

public static void TestFieldReduce(ж<testing.T> Ꮡt) {
    for (var a = (uint32)0; a < (uint32)(2 * q * q); a++) {
        var got = fieldReduce(a);
        var exp = ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(uint16)(a % (uint32)q));
        if (got != exp) {
            Ꮡt.Fatalf("reduce(%d) = %d, expected %d"u8, a, got, exp);
        }
    }
}

public static void TestFieldAdd(ж<testing.T> Ꮡt) {
    for (var a = ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)0); a < q; a++) {
        for (var b = ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)0); b < q; b++) {
            var got = fieldAdd(a, b);
            var exp = (global::go.crypto.@internal.fips140.mlkem_package.fieldElement)((a + b) % (uint16)q);
            if (got != exp) {
                Ꮡt.Fatalf("%d + %d = %d, expected %d"u8, a, b, got, exp);
            }
        }
    }
}

public static void TestFieldSub(ж<testing.T> Ꮡt) {
    for (var a = ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)0); a < q; a++) {
        for (var b = ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)0); b < q; b++) {
            var got = fieldSub(a, b);
            var exp = (global::go.crypto.@internal.fips140.mlkem_package.fieldElement)((a - b + (uint16)q) % (uint16)q);
            if (got != exp) {
                Ꮡt.Fatalf("%d - %d = %d, expected %d"u8, a, b, got, exp);
            }
        }
    }
}

public static void TestFieldMul(ж<testing.T> Ꮡt) {
    for (var a = ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)0); a < q; a++) {
        for (var b = ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)0); b < q; b++) {
            var got = fieldMul(a, b);
            var exp = ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(uint16)(((uint32)(uint16)a * (uint32)(uint16)b) % (uint32)q));
            if (got != exp) {
                Ꮡt.Fatalf("%d * %d = %d, expected %d"u8, a, b, got, exp);
            }
        }
    }
}

public static void TestDecompressCompress(ж<testing.T> Ꮡt) {
    foreach (var (_, bits) in new uint8[]{1, 4, 10}.slice()) {
        for (var a = (uint16)0; a < (uint16)(((uint16)1).Lsh((uint64)(bits))); a++) {
            var f = decompress(a, bits);
            if (f >= q) {
                Ꮡt.Fatalf("decompress(%d, %d) = %d >= q"u8, a, bits, f);
            }
            var got = compress(f, bits);
            if (got != a) {
                Ꮡt.Fatalf("compress(decompress(%d, %d), %d) = %d"u8, a, bits, bits, got);
            }
        }
        for (var a = ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)0); a < q; a++) {
            var c = compress(a, bits);
            if (c >= (uint16)(((uint16)1).Lsh((uint64)(bits)))) {
                Ꮡt.Fatalf("compress(%d, %d) = %d >= 2^bits"u8, a, bits, c);
            }
            var got = decompress(c, bits);
            var diff = min((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(a - got), (global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(got - a), (global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(a - got + (uint16)q), (global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(got - a + (uint16)q));
            nint ceil = (nint)q / (((nint)1).Lsh((uint64)(bits)));
            if (diff > ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(uint16)ceil)) {
                Ꮡt.Fatalf("decompress(compress(%d, %d), %d) = %d (diff %d, max diff %d)"u8,
                    a, bits, bits, got, diff, ceil);
            }
        }
    }
}

internal static uint16 CompressRat(global::go.crypto.@internal.fips140.mlkem_package.fieldElement x, uint8 d) {
    if (x >= q) {
        throw panic("x out of range");
    }
    if (d <= 0 || d >= 12) {
        throw panic("d out of range");
    }
    var precise = big.NewRat((((int64)1).Lsh((uint64)(d))) * (int64)(uint16)x, q); // (2ᵈ / q) * x == (2ᵈ * x) / q
    // FloatString rounds halves away from 0, and our result should always be positive,
    // so it should work as we expect. (There's no direct way to round a Rat.)
    var (rounded, err) = strconv.ParseInt(precise.FloatString(0), 10, 64);
    if (err != default!) {
        throw panic(err);
    }
    // If we rounded up, `rounded` may be equal to 2ᵈ, so we perform a final reduction.
    return (uint16)(rem(rounded, (((int64)1).Lsh((uint64)(d)))));
}

public static void TestCompress(ж<testing.T> Ꮡt) {
    for (nint d = 1; d < 12; d++) {
        for (nint n = 0; n < q; n++) {
            var expected = CompressRat(((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(uint16)n), (uint8)d);
            var result = compress(((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(uint16)n), (uint8)d);
            if (result != expected) {
                Ꮡt.Errorf("compress(%d, %d): got %d, expected %d"u8, n, d, result, expected);
            }
        }
    }
}

internal static global::go.crypto.@internal.fips140.mlkem_package.fieldElement DecompressRat(uint16 y, uint8 d) {
    if (y >= (uint16)(((uint16)1).Lsh((uint64)(d)))) {
        throw panic("y out of range");
    }
    if (d <= 0 || d >= 12) {
        throw panic("d out of range");
    }
    var precise = big.NewRat((int64)q * (int64)y, ((int64)1).Lsh((uint64)(d))); // (q / 2ᵈ) * y  ==  (q * y) / 2ᵈ
    // FloatString rounds halves away from 0, and our result should always be positive,
    // so it should work as we expect. (There's no direct way to round a Rat.)
    var (rounded, err) = strconv.ParseInt(precise.FloatString(0), 10, 64);
    if (err != default!) {
        throw panic(err);
    }
    // If we rounded up, `rounded` may be equal to q, so we perform a final reduction.
    return ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(uint16)(rounded % (int64)q));
}

public static void TestDecompress(ж<testing.T> Ꮡt) {
    for (nint d = 1; d < 12; d++) {
        for (nint n = 0; n < (((nint)1).Lsh((int64)(d))); n++) {
            var expected = DecompressRat((uint16)n, (uint8)d);
            var result = decompress((uint16)n, (uint8)d);
            if (result != expected) {
                Ꮡt.Errorf("decompress(%d, %d): got %d, expected %d"u8, n, d, result, expected);
            }
        }
    }
}

internal static global::go.crypto.@internal.fips140.mlkem_package.ringElement randomRingElement() {
    global::go.crypto.@internal.fips140.mlkem_package.ringElement r = default!;
    foreach (var (i, _) in r) {
        r[i] = ((global::go.crypto.@internal.fips140.mlkem_package.fieldElement)(uint16)mathrand.IntN(q));
    }
    return r.Clone();
}

public static void TestEncodeDecode(ж<testing.T> Ꮡt) {
    var f = randomRingElement();
    var b = new slice<byte>(12 * n / 8);
    rand.Read(b);
    // Compare ringCompressAndEncode to ringCompressAndEncodeN.
    var e1 = ringCompressAndEncode(default!, f, 10);
    var e2 = ringCompressAndEncode10(default!, f);
    if (!bytes_package.Equal(e1, e2)) {
        Ꮡt.Errorf("ringCompressAndEncode = %x, ringCompressAndEncode10 = %x"u8, e1, e2);
    }
    e1 = ringCompressAndEncode(default!, f, 4);
    e2 = ringCompressAndEncode4(default!, f);
    if (!bytes_package.Equal(e1, e2)) {
        Ꮡt.Errorf("ringCompressAndEncode = %x, ringCompressAndEncode4 = %x"u8, e1, e2);
    }
    e1 = ringCompressAndEncode(default!, f, 1);
    e2 = ringCompressAndEncode1(default!, f);
    if (!bytes_package.Equal(e1, e2)) {
        Ꮡt.Errorf("ringCompressAndEncode = %x, ringCompressAndEncode1 = %x"u8, e1, e2);
    }
    // Compare ringDecodeAndDecompress to ringDecodeAndDecompressN.
    var g1 = ringDecodeAndDecompress(b[..(int)(encodingSize10)], 10);
    var g2 = ringDecodeAndDecompress10(Ꮡ(array<byte>.Alias(b, 320)));
    if (g1 != g2) {
        Ꮡt.Errorf("ringDecodeAndDecompress = %v, ringDecodeAndDecompress10 = %v"u8, g1, g2);
    }
    g1 = ringDecodeAndDecompress(b[..(int)(encodingSize4)], 4);
    g2 = ringDecodeAndDecompress4(ref (Ꮡ(array<byte>.Alias(b, 128))).DerefOrNull());
    if (g1 != g2) {
        Ꮡt.Errorf("ringDecodeAndDecompress = %v, ringDecodeAndDecompress4 = %v"u8, g1, g2);
    }
    g1 = ringDecodeAndDecompress(b[..(int)(encodingSize1)], 1);
    g2 = ringDecodeAndDecompress1(ref (Ꮡ(array<byte>.Alias(b, 32))).DerefOrNull());
    if (g1 != g2) {
        Ꮡt.Errorf("ringDecodeAndDecompress = %v, ringDecodeAndDecompress1 = %v"u8, g1, g2);
    }
    // Round-trip ringCompressAndEncode and ringDecodeAndDecompress.
    for (nint d = 1; d < 12; d++) {
        nint encodingSize = d * (nint)n / 8;
        var gΔ1 = ringDecodeAndDecompress(b.slice(0, encodingSize), (uint8)d);
        var outΔ1 = ringCompressAndEncode(default!, gΔ1, (uint8)d);
        if (!bytes_package.Equal(outΔ1, b.slice(0, encodingSize))) {
            Ꮡt.Errorf("roundtrip failed for d = %d"u8, d);
        }
    }
    // Round-trip ringCompressAndEncodeN and ringDecodeAndDecompressN.
    var g = ringDecodeAndDecompress10(Ꮡ(array<byte>.Alias(b, 320)));
    var @out = ringCompressAndEncode10(default!, g);
    if (!bytes_package.Equal(@out, b[..(int)(encodingSize10)])) {
        Ꮡt.Errorf("roundtrip failed for specialized 10"u8);
    }
    g = ringDecodeAndDecompress4(ref (Ꮡ(array<byte>.Alias(b, 128))).DerefOrNull());
    @out = ringCompressAndEncode4(default!, g);
    if (!bytes_package.Equal(@out, b[..(int)(encodingSize4)])) {
        Ꮡt.Errorf("roundtrip failed for specialized 4"u8);
    }
    g = ringDecodeAndDecompress1(ref (Ꮡ(array<byte>.Alias(b, 32))).DerefOrNull());
    @out = ringCompressAndEncode1(default!, g);
    if (!bytes_package.Equal(@out, b[..(int)(encodingSize1)])) {
        Ꮡt.Errorf("roundtrip failed for specialized 1"u8);
    }
}

public static uint8 BitRev7(uint8 n) {
    if ((uint8)((n >> (int)(7))) != 0) {
        throw panic("not 7 bits");
    }
    uint8 r = default!;
    r |= (uint8)((uint8)((n >> (int)(6)) & 0b0000_0001));
    r |= (uint8)((uint8)((n >> (int)(4)) & 0b0000_0010));
    r |= (uint8)((uint8)((n >> (int)(2)) & 0b0000_0100));
    r |= (uint8)((uint8)(n & 0b0000_1000));
    /**/
    r |= (uint8)((uint8)((uint8)(n << (int)(2)) & 0b0001_0000));
    r |= (uint8)((uint8)((uint8)(n << (int)(4)) & 0b0010_0000));
    r |= (uint8)((uint8)((uint8)(n << (int)(6)) & 0b0100_0000));
    return r;
}

public static void TestZetas(ж<testing.T> Ꮡt) {
    var ζ = big.NewInt(17);
    var q = big.NewInt(mlkem_package.q);
    foreach (var (k, zeta) in zetas.ΔRangeSnapshot()) {
        // ζ^BitRev7(k) mod q
        var exp = @new<bigꓸInt>().Exp(ζ, big.NewInt((int64)BitRev7((uint8)k)), q);
        if (big.NewInt((int64)(uint16)zeta).Cmp(exp) != 0) {
            Ꮡt.Errorf("zetas[%d] = %v, expected %v"u8, k, zeta, exp.OrTypedNil());
        }
    }
}

public static void TestGammas(ж<testing.T> Ꮡt) {
    var ζ = big.NewInt(17);
    var q = big.NewInt(mlkem_package.q);
    foreach (var (k, gamma) in gammas.ΔRangeSnapshot()) {
        // ζ^2BitRev7(i)+1
        var exp = @new<bigꓸInt>().Exp(ζ, big.NewInt((int64)BitRev7((uint8)k) * 2 + 1), q);
        if (big.NewInt((int64)(uint16)gamma).Cmp(exp) != 0) {
            Ꮡt.Errorf("gammas[%d] = %v, expected %v"u8, k, gamma, exp.OrTypedNil());
        }
    }
}

} // end mlkem_internal_test_package
