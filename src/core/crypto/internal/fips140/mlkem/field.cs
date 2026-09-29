// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using sha3 = go.crypto.@internal.fips140.sha3_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using errors = errors_package;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140deps;

partial class mlkem_package {

[GoType("num:uint16")] public partial struct fieldElement;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unreducedFieldElementˢ = "unreduced field element"u8;

// fieldCheckReduced checks that a value a is < q.
internal static (fieldElement, error) fieldCheckReduced(uint16 a) {
    if (a >= q) {
        return (0, errors.New(unreducedFieldElementˢ));
    }
    return (((fieldElement)a), default!);
}

// fieldReduceOnce reduces a value a < 2q.
internal static fieldElement fieldReduceOnce(uint16 a) {
    var x = (uint16)(a - (uint16)q);
    // If x underflowed, then x >= 2¹⁶ - q > 2¹⁵, so the top bit is set.
    x += (uint16)(((x >> (int)(15))) * (uint16)q);
    return ((fieldElement)x);
}

internal static fieldElement fieldAdd(fieldElement a, fieldElement b) {
    var x = (uint16)(a + b);
    return fieldReduceOnce(x);
}

internal static fieldElement fieldSub(fieldElement a, fieldElement b) {
    var x = (uint16)(a - b + (uint16)q);
    return fieldReduceOnce(x);
}

internal static UntypedInt barrettMultiplier => 5039; // 2¹² * 2¹² / q
internal static UntypedInt barrettShift => 24; // log₂(2¹² * 2¹²)

// fieldReduce reduces a value a < 2q² using Barrett reduction, to avoid
// potentially variable-time division.
internal static fieldElement fieldReduce(uint32 a) {
    var quotient = (uint32)((((uint64)a * (uint64)barrettMultiplier) >> (int)(barrettShift)));
    return fieldReduceOnce((uint16)(a - quotient * (uint32)q));
}

internal static fieldElement fieldMul(fieldElement a, fieldElement b) {
    var x = (uint32)(uint16)a * (uint32)(uint16)b;
    return fieldReduce(x);
}

// fieldMulSub returns a * (b - c). This operation is fused to save a
// fieldReduceOnce after the subtraction.
internal static fieldElement fieldMulSub(fieldElement a, fieldElement b, fieldElement c) {
    var x = (uint32)(uint16)a * (uint32)(uint16)(b - c + (uint16)q);
    return fieldReduce(x);
}

// fieldAddMul returns a * b + c * d. This operation is fused to save a
// fieldReduceOnce and a fieldReduce.
internal static fieldElement fieldAddMul(fieldElement a, fieldElement b, fieldElement c, fieldElement d) {
    var x = (uint32)(uint16)a * (uint32)(uint16)b;
    x += (uint32)(uint16)c * (uint32)(uint16)d;
    return fieldReduce(x);
}

// compress maps a field element uniformly to the range 0 to 2ᵈ-1, according to
// FIPS 203, Definition 4.7.
internal static uint16 compress(fieldElement x, uint8 d) {
    // We want to compute (x * 2ᵈ) / q, rounded to nearest integer, with 1/2
    // rounding up (see FIPS 203, Section 2.3).
    // Barrett reduction produces a quotient and a remainder in the range [0, 2q),
    // such that dividend = quotient * q + remainder.
    var dividend = ((uint32)(uint16)x).Lsh((uint64)(d)); // x * 2ᵈ
    var quotient = (uint32)(((uint64)dividend * (uint64)barrettMultiplier >> (int)(barrettShift)));
    var remainder = dividend - quotient * (uint32)q;
    // Since the remainder is in the range [0, 2q), not [0, q), we need to
    // portion it into three spans for rounding.
    //
    //     [ 0,       q/2     ) -> round to 0
    //     [ q/2,     q + q/2 ) -> round to 1
    //     [ q + q/2, 2q      ) -> round to 2
    //
    // We can convert that to the following logic: add 1 if remainder > q/2,
    // then add 1 again if remainder > q + q/2.
    //
    // Note that if remainder > x, then ⌊x⌋ - remainder underflows, and the top
    // bit of the difference will be set.
    quotient += (uint32)((((uint32)(q / 2) - remainder) >> (int)(31)) & 1);
    quotient += (uint32)((((uint32)(q + q / 2) - remainder) >> (int)(31)) & 1);
    // quotient might have overflowed at this point, so reduce it by masking.
    uint32 mask = (((uint32)1).Lsh((uint64)(d))) - 1;
    return (uint16)((uint32)(quotient & mask));
}

// decompress maps a number x between 0 and 2ᵈ-1 uniformly to the full range of
// field elements, according to FIPS 203, Definition 4.8.
internal static fieldElement decompress(uint16 y, uint8 d) {
    // We want to compute (y * q) / 2ᵈ, rounded to nearest integer, with 1/2
    // rounding up (see FIPS 203, Section 2.3).
    var dividend = (uint32)y * (uint32)q;
    var quotient = dividend.Rsh((uint64)(d)); // (y * q) / 2ᵈ
    // The d'th least-significant bit of the dividend (the most significant bit
    // of the remainder) is 1 for the top half of the values that divide to the
    // same quotient, which are the ones that round up.
    quotient += (uint32)(dividend.Rsh((uint64)((uint8)(d - 1))) & 1);
    // quotient is at most (2¹¹-1) * q / 2¹¹ + 1 = 3328, so it didn't overflow.
    return ((fieldElement)(uint16)quotient);
}

[GoType("[256]fieldElement")] /* [n]fieldElement */
partial struct ringElement;

// polyAdd adds two ringElements or nttElements.
internal static T /*s*/ polyAdd<T>(T a, T b)
    where T : /* ~[256]crypto/internal/fips140/mlkem.fieldElement */ IArray<fieldElement>, new()
{
    T s = GoZero<T>();

    foreach (var (i, _) in s) {
        s[i] = fieldAdd(a[i], b[i]);
    }
    return s;
}

// polySub subtracts two ringElements or nttElements.
internal static T /*s*/ polySub<T>(T a, T b)
    where T : /* ~[256]crypto/internal/fips140/mlkem.fieldElement */ IArray<fieldElement>, new()
{
    T s = GoZero<T>();

    foreach (var (i, _) in s) {
        s[i] = fieldSub(a[i], b[i]);
    }
    return s;
}

// polyByteEncode appends the 384-byte encoding of f to b.
//
// It implements ByteEncode₁₂, according to FIPS 203, Algorithm 5.
internal static slice<byte> polyByteEncode<T>(slice<byte> b, T f)
    where T : /* ~[256]crypto/internal/fips140/mlkem.fieldElement */ IArray<fieldElement>, new()
{
    var (@out, B) = sliceForAppend(b, encodingSize12);
    for (nint i = 0; i < n; i += 2) {
        var x = (uint32)((uint32)(uint16)f[i] | ((uint32)(uint16)f[i + 1] << (int)(12)));
        B[0] = (uint8)x;
        B[1] = (uint8)((x >> (int)(8)));
        B[2] = (uint8)((x >> (int)(16)));
        B = B[3..];
    }
    return @out;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mlkemInvalidEncodingˢ = "mlkem: invalid encoding length"u8;
internal static readonly @string mlkemInvalidPolynomialˢ = "mlkem: invalid polynomial encoding"u8;

// polyByteDecode decodes the 384-byte encoding of a polynomial, checking that
// all the coefficients are properly reduced. This fulfills the "Modulus check"
// step of ML-KEM Encapsulation.
//
// It implements ByteDecode₁₂, according to FIPS 203, Algorithm 6.
internal static (T, error) polyByteDecode<T>(slice<byte> b)
    where T : /* ~[256]crypto/internal/fips140/mlkem.fieldElement */ IArray<fieldElement>, new()
{
    if (len(b) != encodingSize12) {
        return (new T{}, errors.New(mlkemInvalidEncodingˢ));
    }
    T f = GoZero<T>();
    for (nint i = 0; i < n; i += 2) {
        var d = (uint32)((uint32)((uint32)b[0] | ((uint32)b[1] << (int)(8))) | ((uint32)b[2] << (int)(16)));
        const uint32 mask12 = 0b1111_1111_1111;
        error err = default!;
        {
            (f[i], err) = fieldCheckReduced((uint16)((uint32)(d & mask12))); if (err != default!) {
                return (new T{}, errors.New(mlkemInvalidPolynomialˢ));
            }
        }
        {
            (f[i + 1], err) = fieldCheckReduced((uint16)((d >> (int)(12)))); if (err != default!) {
                return (new T{}, errors.New(mlkemInvalidPolynomialˢ));
            }
        }
        b = b[3..];
    }
    return (f, default!);
}

// sliceForAppend takes a slice and a requested number of bytes. It returns a
// slice with the contents of the given slice followed by that many bytes and a
// second slice that aliases into it and contains only the extra bytes. If the
// original slice has sufficient capacity then no allocation is performed.
internal static (slice<byte> head, slice<byte> tail) sliceForAppend(slice<byte> @in, nint n) {
    slice<byte> head = default!;
    slice<byte> tail = default!;

    {
        nint total = len(@in) + n; if (cap(@in) >= total){
            head = @in[..(int)(total)];
        } else {
            head = new slice<byte>(total);
            copy(head, @in);
        }
    }
    tail = head[(int)(len(@in))..];
    return (head, tail);
}

// ringCompressAndEncode1 appends a 32-byte encoding of a ring element to s,
// compressing one coefficients per bit.
//
// It implements Compress₁, according to FIPS 203, Definition 4.7,
// followed by ByteEncode₁, according to FIPS 203, Algorithm 5.
internal static slice<byte> ringCompressAndEncode1(slice<byte> s, ringElement f) {
    f = f.Clone();

    (s, var b) = sliceForAppend(s, encodingSize1);
    foreach (var (i, _) in b) {
        b[i] = 0;
    }
    foreach (var (i, _) in f) {
        b[i / 8] |= (uint8)((uint8)(compress(f[i], 1).Lsh((int64)((i % 8)))));
    }
    return s;
}

// ringDecodeAndDecompress1 decodes a 32-byte slice to a ring element where each
// bit is mapped to 0 or ⌈q/2⌋.
//
// It implements ByteDecode₁, according to FIPS 203, Algorithm 6,
// followed by Decompress₁, according to FIPS 203, Definition 4.8.
internal static ringElement ringDecodeAndDecompress1(ref array<byte> b) {
    ringElement f = default!;
    foreach (var (i, _) in f) {
        var b_i = (byte)(b[i / 8].Rsh((int64)((i % 8))) & 1);
        UntypedInt halfQ = /* (q + 1) / 2 */ 1665; // ⌈q/2⌋, rounded up per FIPS 203, Section 2.3
        f[i] = (fieldElement)(((fieldElement)(uint16)b_i) * (uint16)halfQ); // 0 decompresses to 0, and 1 to ⌈q/2⌋
    }
    return f.Clone();
}

// ringCompressAndEncode4 appends a 128-byte encoding of a ring element to s,
// compressing two coefficients per byte.
//
// It implements Compress₄, according to FIPS 203, Definition 4.7,
// followed by ByteEncode₄, according to FIPS 203, Algorithm 5.
internal static slice<byte> ringCompressAndEncode4(slice<byte> s, ringElement f) {
    f = f.Clone();

    (s, var b) = sliceForAppend(s, encodingSize4);
    for (nint i = 0; i < n; i += 2) {
        b[i / 2] = (uint8)((uint16)(compress(f[i], 4) | (uint16)(compress(f[i + 1], 4) << (int)(4))));
    }
    return s;
}

// ringDecodeAndDecompress4 decodes a 128-byte encoding of a ring element where
// each four bits are mapped to an equidistant distribution.
//
// It implements ByteDecode₄, according to FIPS 203, Algorithm 6,
// followed by Decompress₄, according to FIPS 203, Definition 4.8.
internal static ringElement ringDecodeAndDecompress4(ref array<byte> b) {
    ringElement f = default!;
    for (nint i = 0; i < n; i += 2) {
        f[i] = decompress((uint16)((byte)(b[i / 2] & 0b1111)), 4);
        f[i + 1] = decompress((uint16)((b[i / 2] >> (int)(4))), 4);
    }
    return f.Clone();
}

// ringCompressAndEncode10 appends a 320-byte encoding of a ring element to s,
// compressing four coefficients per five bytes.
//
// It implements Compress₁₀, according to FIPS 203, Definition 4.7,
// followed by ByteEncode₁₀, according to FIPS 203, Algorithm 5.
internal static slice<byte> ringCompressAndEncode10(slice<byte> s, ringElement f) {
    f = f.Clone();

    (s, var b) = sliceForAppend(s, encodingSize10);
    for (nint i = 0; i < n; i += 4) {
        uint64 x = default!;
        x |= (uint64)((uint64)compress(f[i], 10));
        x |= (uint64)(((uint64)compress(f[i + 1], 10) << (int)(10)));
        x |= (uint64)(((uint64)compress(f[i + 2], 10) << (int)(20)));
        x |= (uint64)(((uint64)compress(f[i + 3], 10) << (int)(30)));
        b[0] = (uint8)x;
        b[1] = (uint8)((x >> (int)(8)));
        b[2] = (uint8)((x >> (int)(16)));
        b[3] = (uint8)((x >> (int)(24)));
        b[4] = (uint8)((x >> (int)(32)));
        b = b[5..];
    }
    return s;
}

// ringDecodeAndDecompress10 decodes a 320-byte encoding of a ring element where
// each ten bits are mapped to an equidistant distribution.
//
// It implements ByteDecode₁₀, according to FIPS 203, Algorithm 6,
// followed by Decompress₁₀, according to FIPS 203, Definition 4.8.
internal static ringElement ringDecodeAndDecompress10([GoArrayDims(320)] ж<array<byte>> Ꮡbb) {
    ref var bb = ref Ꮡbb.DerefOrNull();

    var b = bb[..];
    ringElement f = default!;
    for (nint i = 0; i < n; i += 4) {
        var x = (uint64)((uint64)((uint64)((uint64)((uint64)b[0] | ((uint64)b[1] << (int)(8))) | ((uint64)b[2] << (int)(16))) | ((uint64)b[3] << (int)(24))) | ((uint64)b[4] << (int)(32)));
        b = b[5..];
        f[i] = decompress((uint16)((uint64)((x >> (int)(0)) & 0b11_1111_1111)), 10);
        f[i + 1] = decompress((uint16)((uint64)((x >> (int)(10)) & 0b11_1111_1111)), 10);
        f[i + 2] = decompress((uint16)((uint64)((x >> (int)(20)) & 0b11_1111_1111)), 10);
        f[i + 3] = decompress((uint16)((uint64)((x >> (int)(30)) & 0b11_1111_1111)), 10);
    }
    return f.Clone();
}

// ringCompressAndEncode appends an encoding of a ring element to s,
// compressing each coefficient to d bits.
//
// It implements Compress, according to FIPS 203, Definition 4.7,
// followed by ByteEncode, according to FIPS 203, Algorithm 5.
internal static slice<byte> ringCompressAndEncode(slice<byte> s, ringElement f, uint8 d) {
    f = f.Clone();

    byte b = default!;
    uint8 bIdx = default!;
    for (nint i = 0; i < n; i++) {
        var c = compress(f[i], d);
        uint8 cIdx = default!;
        while (cIdx < d) {
            b |= (byte)(((byte)(c.Rsh((uint64)(cIdx)))).Lsh((uint64)(bIdx)));
            var bits = min((uint8)(8 - bIdx), (uint8)(d - cIdx));
            bIdx += bits;
            cIdx += bits;
            if (bIdx == 8) {
                s = append(s, b);
                b = 0;
                bIdx = 0;
            }
        }
    }
    if (bIdx != 0) {
        throw panic("mlkem: internal error: bitsFilled != 0");
    }
    return s;
}

// ringDecodeAndDecompress decodes an encoding of a ring element where
// each d bits are mapped to an equidistant distribution.
//
// It implements ByteDecode, according to FIPS 203, Algorithm 6,
// followed by Decompress, according to FIPS 203, Definition 4.8.
internal static ringElement ringDecodeAndDecompress(slice<byte> b, uint8 d) {
    ringElement f = default!;
    uint8 bIdx = default!;
    for (nint i = 0; i < n; i++) {
        uint16 c = default!;
        uint8 cIdx = default!;
        while (cIdx < d) {
            c |= (uint16)(((uint16)(b[0].Rsh((uint64)(bIdx)))).Lsh((uint64)(cIdx)));
            c &= (uint16)((((uint16)1).Lsh((uint64)(d))) - 1);
            var bits = min((uint8)(8 - bIdx), (uint8)(d - cIdx));
            bIdx += bits;
            cIdx += bits;
            if (bIdx == 8) {
                b = b[1..];
                bIdx = 0;
            }
        }
        f[i] = decompress(c, d);
    }
    if (len(b) != 0) {
        throw panic("mlkem: internal error: leftover bytes");
    }
    return f.Clone();
}

// ringCompressAndEncode5 appends a 160-byte encoding of a ring element to s,
// compressing eight coefficients per five bytes.
//
// It implements Compress₅, according to FIPS 203, Definition 4.7,
// followed by ByteEncode₅, according to FIPS 203, Algorithm 5.
internal static slice<byte> ringCompressAndEncode5(slice<byte> s, ringElement f) {
    f = f.Clone();

    return ringCompressAndEncode(s, f, 5);
}

// ringDecodeAndDecompress5 decodes a 160-byte encoding of a ring element where
// each five bits are mapped to an equidistant distribution.
//
// It implements ByteDecode₅, according to FIPS 203, Algorithm 6,
// followed by Decompress₅, according to FIPS 203, Definition 4.8.
internal static ringElement ringDecodeAndDecompress5([GoArrayDims(160)] ж<array<byte>> Ꮡbb) {
    ref var bb = ref Ꮡbb.DerefOrNull();

    return ringDecodeAndDecompress(bb[..], 5);
}

// ringCompressAndEncode11 appends a 352-byte encoding of a ring element to s,
// compressing eight coefficients per eleven bytes.
//
// It implements Compress₁₁, according to FIPS 203, Definition 4.7,
// followed by ByteEncode₁₁, according to FIPS 203, Algorithm 5.
internal static slice<byte> ringCompressAndEncode11(slice<byte> s, ringElement f) {
    f = f.Clone();

    return ringCompressAndEncode(s, f, 11);
}

// ringDecodeAndDecompress11 decodes a 352-byte encoding of a ring element where
// each eleven bits are mapped to an equidistant distribution.
//
// It implements ByteDecode₁₁, according to FIPS 203, Algorithm 6,
// followed by Decompress₁₁, according to FIPS 203, Definition 4.8.
internal static ringElement ringDecodeAndDecompress11([GoArrayDims(352)] ж<array<byte>> Ꮡbb) {
    ref var bb = ref Ꮡbb.DerefOrNull();

    return ringDecodeAndDecompress(bb[..], 11);
}

// samplePolyCBD draws a ringElement from the special Dη distribution given a
// stream of random bytes generated by the PRF function, according to FIPS 203,
// Algorithm 8 and Definition 4.3.
internal static ringElement samplePolyCBD(slice<byte> s, byte b) {
    var prf = sha3.NewShake256();
    prf.Write(s);
    prf.Write(new byte[]{b}.slice());
    var B = new slice<byte>(64 * 2); // η = 2
    prf.Read(B);
    // SamplePolyCBD simply draws four (2η) bits for each coefficient, and adds
    // the first two and subtracts the last two.
    ringElement f = default!;
    for (nint i = 0; i < n; i += 2) {
        var bΔ1 = B[i / 2];
        var (b_7, b_6, b_5, b_4) = ((byte)((bΔ1 >> (int)(7))), (byte)((bΔ1 >> (int)(6)) & 1), (byte)((bΔ1 >> (int)(5)) & 1), (byte)((bΔ1 >> (int)(4)) & 1));
        var (b_3, b_2, b_1, b_0) = ((byte)((bΔ1 >> (int)(3)) & 1), (byte)((bΔ1 >> (int)(2)) & 1), (byte)((bΔ1 >> (int)(1)) & 1), (byte)(bΔ1 & 1));
        f[i] = fieldSub(((fieldElement)(uint16)((byte)(b_0 + b_1))), ((fieldElement)(uint16)((byte)(b_2 + b_3))));
        f[i + 1] = fieldSub(((fieldElement)(uint16)((byte)(b_4 + b_5))), ((fieldElement)(uint16)((byte)(b_6 + b_7))));
    }
    return f.Clone();
}

[GoType("[256]fieldElement")] /* [n]fieldElement */
partial struct nttElement;

// gammas are the values ζ^2BitRev7(i)+1 mod q for each index i, according to
// FIPS 203, Appendix A (with negative values reduced to positive).
internal static array<fieldElement> gammas = new fieldElement[]{17, 3312, 2761, 568, 583, 2746, 2649, 680, 1637, 1692, 723, 2606, 2288, 1041, 1100, 2229, 1409, 1920, 2662, 667, 3281, 48, 233, 3096, 756, 2573, 2156, 1173, 3015, 314, 3050, 279, 1703, 1626, 1651, 1678, 2789, 540, 1789, 1540, 1847, 1482, 952, 2377, 1461, 1868, 2687, 642, 939, 2390, 2308, 1021, 2437, 892, 2388, 941, 733, 2596, 2337, 992, 268, 3061, 641, 2688, 1584, 1745, 2298, 1031, 2037, 1292, 3220, 109, 375, 2954, 2549, 780, 2090, 1239, 1645, 1684, 1063, 2266, 319, 3010, 2773, 556, 757, 2572, 2099, 1230, 561, 2768, 2466, 863, 2594, 735, 2804, 525, 1092, 2237, 403, 2926, 1026, 2303, 1143, 2186, 2150, 1179, 2775, 554, 886, 2443, 1722, 1607, 1212, 2117, 1874, 1455, 1029, 2300, 2110, 1219, 2935, 394, 885, 2444, 2154, 1175}.array();

// nttMul multiplies two nttElements.
//
// It implements MultiplyNTTs, according to FIPS 203, Algorithm 11.
internal static nttElement nttMul(nttElement f, nttElement g) {
    f = f.Clone();
    g = g.Clone();

    nttElement h = default!;
    // We use i += 2 for bounds check elimination. See https://go.dev/issue/66826.
    for (nint i = 0; i < 256; i += 2) {
        var (a0, a1) = (f[i], f[i + 1]);
        var (b0, b1) = (g[i], g[i + 1]);
        h[i] = fieldAddMul(a0, b0, fieldMul(a1, b1), gammas[i / 2]);
        h[i + 1] = fieldAddMul(a0, b1, a1, b0);
    }
    return h.Clone();
}

// zetas are the values ζ^BitRev7(k) mod q for each index k, according to FIPS
// 203, Appendix A.
internal static array<fieldElement> zetas = new fieldElement[]{1, 1729, 2580, 3289, 2642, 630, 1897, 848, 1062, 1919, 193, 797, 2786, 3260, 569, 1746, 296, 2447, 1339, 1476, 3046, 56, 2240, 1333, 1426, 2094, 535, 2882, 2393, 2879, 1974, 821, 289, 331, 3253, 1756, 1197, 2304, 2277, 2055, 650, 1977, 2513, 632, 2865, 33, 1320, 1915, 2319, 1435, 807, 452, 1438, 2868, 1534, 2402, 2647, 2617, 1481, 648, 2474, 3110, 1227, 910, 17, 2761, 583, 2649, 1637, 723, 2288, 1100, 1409, 2662, 3281, 233, 756, 2156, 3015, 3050, 1703, 1651, 2789, 1789, 1847, 952, 1461, 2687, 939, 2308, 2437, 2388, 733, 2337, 268, 641, 1584, 2298, 2037, 3220, 375, 2549, 2090, 1645, 1063, 319, 2773, 757, 2099, 561, 2466, 2594, 2804, 1092, 403, 1026, 1143, 2150, 2775, 886, 1722, 1212, 1874, 1029, 2110, 2935, 885, 2154}.array();

// ntt maps a ringElement to its nttElement representation.
//
// It implements NTT, according to FIPS 203, Algorithm 9.
internal static nttElement ntt(ringElement f) {
    f = f.Clone();

    nint k = 1;
    for (nint len = 128; len >= 2; len /= 2) {
        for (nint start = 0; start < 256; start += 2 * len) {
            var zeta = zetas[k];
            k++;
            // Bounds check elimination hint.
            var (fΔ1, flen) = (f[(int)(start)..(int)(start + len)], f[(int)(start + len)..(int)(start + len + len)]);
            for (nint j = 0; j < len; j++) {
                var t = fieldMul(zeta, flen[j]);
                flen[j] = fieldSub(fΔ1[j], t);
                fΔ1[j] = fieldAdd(fΔ1[j], t);
            }
        }
    }
    return ((nttElement)(array<fieldElement>)f);
}

// inverseNTT maps a nttElement back to the ringElement it represents.
//
// It implements NTT⁻¹, according to FIPS 203, Algorithm 10.
internal static ringElement inverseNTT(nttElement f) {
    f = f.Clone();

    nint k = 127;
    for (nint len = 2; len <= 128; len *= 2) {
        for (nint start = 0; start < 256; start += 2 * len) {
            var zeta = zetas[k];
            k--;
            // Bounds check elimination hint.
            var (fΔ1, flen) = (f[(int)(start)..(int)(start + len)], f[(int)(start + len)..(int)(start + len + len)]);
            for (nint j = 0; j < len; j++) {
                var t = fΔ1[j];
                fΔ1[j] = fieldAdd(t, flen[j]);
                flen[j] = fieldMulSub(zeta, flen[j], t);
            }
        }
    }
    foreach (var (i, _) in f) {
        f[i] = fieldMul(f[i], 3303); // 3303 = 128⁻¹ mod q
    }
    return ((ringElement)(array<fieldElement>)f);
}

// sampleNTT draws a uniformly random nttElement from a stream of uniformly
// random bytes generated by the XOF function, according to FIPS 203,
// Algorithm 7.
internal static nttElement sampleNTT(slice<byte> rho, byte ii, byte jj) {
    var B = sha3.NewShake128();
    B.Write(rho);
    B.Write(new byte[]{ii, jj}.slice());
    // SampleNTT essentially draws 12 bits at a time from r, interprets them in
    // little-endian, and rejects values higher than q, until it drew 256
    // values. (The rejection rate is approximately 19%.)
    //
    // To do this from a bytes stream, it draws three bytes at a time, and
    // splits them into two uint16 appropriately masked.
    //
    //               r₀              r₁              r₂
    //       |- - - - - - - -|- - - - - - - -|- - - - - - - -|
    //
    //               Uint16(r₀ || r₁)
    //       |- - - - - - - - - - - - - - - -|
    //       |- - - - - - - - - - - -|
    //                   d₁
    //
    //                                Uint16(r₁ || r₂)
    //                       |- - - - - - - - - - - - - - - -|
    //                               |- - - - - - - - - - - -|
    //                                           d₂
    //
    // Note that in little-endian, the rightmost bits are the most significant
    // bits (dropped with a mask) and the leftmost bits are the least
    // significant bits (dropped with a right shift).
    nttElement a = default!;
    nint j = default!;              // index into a
    array<byte> buf = new(24);               // buffered reads from B
    nint off = len(buf); // index into buf, starts in a "buffer fully consumed" state
    while (ᐧ) {
        if (off >= len(buf)) {
            B.Read(buf[..]);
            off = 0;
        }
        var d1 = (uint16)(byteorder.LEUint16(buf[(int)(off)..]) & 0b1111_1111_1111);
        var d2 = (uint16)((byteorder.LEUint16(buf[(int)(off + 1)..]) >> (int)(4)));
        off += 3;
        if (d1 < q) {
            a[j] = ((fieldElement)d1);
            j++;
        }
        if (j >= len(a)) {
            break;
        }
        if (d2 < q) {
            a[j] = ((fieldElement)d2);
            j++;
        }
        if (j >= len(a)) {
            break;
        }
    }
    return a.Clone();
}

} // end mlkem_package
