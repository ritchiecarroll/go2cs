// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.aes;

using fips140 = go.crypto.@internal.fips140_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140deps;

partial class gcm_package {

// gcmFieldElement represents a value in GF(2¹²⁸). In order to reflect the GCM
// standard and make binary.BigEndian suitable for marshaling these values, the
// bits are stored in big endian order. For example:
//
//	the coefficient of x⁰ can be obtained by v.low >> 63.
//	the coefficient of x⁶³ can be obtained by v.low & 1.
//	the coefficient of x⁶⁴ can be obtained by v.high >> 63.
//	the coefficient of x¹²⁷ can be obtained by v.high & 1.
partial struct gcmFieldElement {
    internal uint64 low, high;
}

// GHASH is exposed to allow crypto/cipher to implement non-AES GCM modes.
// It is not allowed as a stand-alone operation in FIPS mode because it
// is not ACVP tested.
public static slice<byte> GHASH(/*[16]*/ ж<array<byte>> Ꮡkey, params Span<slice<byte>> inputsʗp) {
    var inputs = inputsʗp.sslice();

    fips140.RecordNonApproved();
    ref var @out = ref heap(new array<byte>(16), out var Ꮡout);
    ghash(Ꮡout, Ꮡkey, inputs.ꓸꓸꓸ);
    return @out[..];
}

// ghash is a variable-time generic implementation of GHASH, which shouldn't
// be used on any architecture with hardware support for AES-GCM.
//
// Each input is zero-padded to 128-bit before being absorbed.
internal static void ghash(/*[16]*/ ж<array<byte>> Ꮡout, /*[16]*/ ж<array<byte>> ᏑH, params Span<slice<byte>> inputsʗp) {
    var inputs = inputsʗp.sslice();

    ref var @out = ref Ꮡout.DerefOrNull();
    ref var H = ref ᏑH.DerefOrNull();
    // productTable contains the first sixteen powers of the key, H.
    // However, they are in bit reversed order.
    array<gcmFieldElement> productTable = new(16);
    // We precompute 16 multiples of H. However, when we do lookups
    // into this table we'll be using bits from a field element and
    // therefore the bits will be in the reverse order. So normally one
    // would expect, say, 4*H to be in index 4 of the table but due to
    // this bit ordering it will actually be in index 0010 (base 2) = 2.
    var x = new gcmFieldElement(
        byteorder.BEUint64(H[..8]),
        byteorder.BEUint64(H[8..])
    );
    productTable[reverseBits(1)] = x;
    for (nint i = 2; i < 16; i += 2) {
        productTable[reverseBits(i)] = ghashDouble(ref productTable[reverseBits(i / 2)]);
        productTable[reverseBits(i + 1)] = ghashAdd(ref productTable[reverseBits(i)], ref x);
    }
    gcmFieldElement y = default!;
    foreach (var (_, input) in inputs) {
        ghashUpdate(ref productTable, ref y, input);
    }
    byteorder.BEPutUint64(@out[..], y.low);
    byteorder.BEPutUint64(@out[8..], y.high);
}

// reverseBits reverses the order of the bits of 4-bit number in i.
internal static nint reverseBits(nint i) {
    i = (nint)(((nint)(((i << (int)(2))) & 0xc)) | ((nint)(((i >> (int)(2))) & 0x3)));
    i = (nint)(((nint)(((i << (int)(1))) & 0xa)) | ((nint)(((i >> (int)(1))) & 0x5)));
    return i;
}

// ghashAdd adds two elements of GF(2¹²⁸) and returns the sum.
internal static gcmFieldElement ghashAdd(ref gcmFieldElement x, ref gcmFieldElement y) {
    // Addition in a characteristic 2 field is just XOR.
    return new gcmFieldElement((uint64)(x.low ^ y.low), (uint64)(x.high ^ y.high));
}

// ghashDouble returns the result of doubling an element of GF(2¹²⁸).
internal static gcmFieldElement /*double*/ ghashDouble(ref gcmFieldElement x) {
    gcmFieldElement @double = default!;

    var msbSet = (uint64)(x.high & 1) == 1;
    // Because of the bit-ordering, doubling is actually a right shift.
    @double.high = (x.high >> (int)(1));
    @double.high |= (uint64)((x.low << (int)(63)));
    @double.low = (x.low >> (int)(1));
    // If the most-significant bit was set before shifting then it,
    // conceptually, becomes a term of x^128. This is greater than the
    // irreducible polynomial so the result has to be reduced. The
    // irreducible polynomial is 1+x+x^2+x^7+x^128. We can subtract that to
    // eliminate the term at x^128 which also means subtracting the other
    // four terms. In characteristic 2 fields, subtraction == addition ==
    // XOR.
    if (msbSet) {
        @double.low ^= (uint64)(0xe100000000000000UL);
    }
    return @double;
}

internal static slice<uint16> ghashReductionTable = new uint16[]{
    0x0000, 0x1c20, 0x3840, 0x2460, 0x7080, 0x6ca0, 0x48c0, 0x54e0,
    0xe100, 0xfd20, 0xd940, 0xc560, 0x9180, 0x8da0, 0xa9c0, 0xb5e0
}.slice();

// ghashMul sets y to y*H, where H is the GCM key, fixed during New.
internal static void ghashMul(ref array<gcmFieldElement> productTable, ref gcmFieldElement y) {
    gcmFieldElement z = default!;
    for (nint i = 0; i < 2; i++) {
        var word = y.high;
        if (i == 1) {
            word = y.low;
        }
        // Multiplication works by multiplying z by 16 and adding in
        // one of the precomputed multiples of H.
        for (nint j = 0; j < 64; j += 4) {
            var msw = (uint64)(z.high & 0xf);
            z.high >>= (int)(4);
            z.high |= (uint64)((z.low << (int)(60)));
            z.low >>= (int)(4);
            z.low ^= (uint64)(((uint64)ghashReductionTable[msw] << (int)(48)));
            // the values in |table| are ordered for little-endian bit
            // positions. See the comment in New.
            var t = productTable[(uint64)(word & 0xf)];
            z.low ^= (uint64)(t.low);
            z.high ^= (uint64)(t.high);
            word >>= (int)(4);
        }
    }
    y = z;
}

// updateBlocks extends y with more polynomial terms from blocks, based on
// Horner's rule. There must be a multiple of gcmBlockSize bytes in blocks.
internal static void updateBlocks(ref array<gcmFieldElement> productTable, ref gcmFieldElement y, slice<byte> blocks) {
    while (len(blocks) > 0) {
        y.low ^= (uint64)(byteorder.BEUint64(blocks));
        y.high ^= (uint64)(byteorder.BEUint64(blocks[8..]));
        ghashMul(ref productTable, ref y);
        blocks = blocks[(int)(gcmBlockSize)..];
    }
}

// ghashUpdate extends y with more polynomial terms from data. If data is not a
// multiple of gcmBlockSize bytes long then the remainder is zero padded.
internal static void ghashUpdate(ref array<gcmFieldElement> productTable, ref gcmFieldElement y, slice<byte> data) {
    nint fullBlocks = (((len(data) >> (int)(4))) << (int)(4));
    updateBlocks(ref productTable, ref y, data.slice(0, fullBlocks));
    if (len(data) != fullBlocks) {
        array<byte> partialBlock = new(16); /* gcmBlockSize */
        copy(partialBlock[..], data.slice(fullBlocks));
        updateBlocks(ref productTable, ref y, partialBlock[..]);
    }
}

} // end gcm_package
