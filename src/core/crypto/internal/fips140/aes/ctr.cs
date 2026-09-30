// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using alias = go.crypto.@internal.fips140.alias_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using bits = math.bits_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140deps;
using math;

partial class aes_package {

[GoType] partial struct CTR {
    internal Block b;
    internal uint64 ivlo, ivhi; // start counter as 64-bit limbs
    internal uint64 offset; // for XORKeyStream only
}

public static ж<CTR> NewCTR(ж<Block> Ꮡb, slice<byte> iv) {
    // Allocate the CTR here, in an easily inlineable function, so
    // the allocation can be done in the caller's stack frame
    // instead of the heap.  See issue 70499.
    ref var c = ref heap<CTR>(out var Ꮡc);
    c = newCTR(ref (Ꮡb).DerefOrNull(), iv);
    return Ꮡc;
}

internal static CTR newCTR(ref Block b, slice<byte> iv) {
    if (len(iv) != ΔBlockSize) {
        throw panic("bad IV length");
    }
    return new CTR(
        b: b,
        ivlo: byteorder.BEUint64(iv[8..16]),
        ivhi: byteorder.BEUint64(iv[0..8]),
        offset: 0
    );
}

public static void XORKeyStream(this ж<CTR> Ꮡc, slice<byte> dst, slice<byte> src) {
    ref var c = ref Ꮡc.DerefOrNull();

    Ꮡc.XORKeyStreamAt(dst, src, c.offset);
    uint64 carry = default!;
    (c.offset, carry) = bits.Add64(c.offset, (uint64)len(src), 0);
    if (carry != 0) {
        throw panic("crypto/aes: counter overflow");
    }
}

// RoundToBlock is used by CTR_DRBG, which discards the rightmost unused bits at
// each request. It rounds the offset up to the next block boundary.
public static void RoundToBlock(ж<CTR> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    {
        var remainder = c.offset % (uint64)ΔBlockSize; if (remainder != 0) {
            uint64 carry = default!;
            (c.offset, carry) = bits.Add64(c.offset, (uint64)ΔBlockSize - remainder, 0);
            if (carry != 0) {
                throw panic("crypto/aes: counter overflow");
            }
        }
    }
}

// XORKeyStreamAt behaves like XORKeyStream but keeps no state, and instead
// seeks into the keystream by the given bytes offset from the start (ignoring
// any XORKetStream calls). This allows for random access into the keystream, up
// to 16 EiB from the start.
public static void XORKeyStreamAt(this ж<CTR> Ꮡc, slice<byte> dst, slice<byte> src, uint64 offset) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (len(dst) < len(src)) {
        throw panic("crypto/aes: len(dst) < len(src)");
    }
    dst = dst.slice(0, len(src));
    if (alias.InexactOverlap(dst, src)) {
        throw panic("crypto/aes: invalid buffer overlap");
    }
    fips140.RecordApproved();
    var (ivlo, ivhi) = add128(c.ivlo, c.ivhi, offset / (uint64)ΔBlockSize);
    {
        var blockOffset = offset % (uint64)ΔBlockSize; if (blockOffset != 0) {
            // We have a partial block at the beginning.
            ref var @in = ref heap(new array<byte>(16), out var Ꮡin);
            ref var @out = ref heap(new array<byte>(16), out var Ꮡout);
            copy(@in.slice((nint)(blockOffset)), src);
            ctrBlocks1(ref nonnil(ref c).b, Ꮡout, Ꮡin, ivlo, ivhi);
            nint n = copy(dst, @out.slice((nint)(blockOffset)));
            src = src.slice(n);
            dst = dst.slice(n);
            (ivlo, ivhi) = add128(ivlo, ivhi, 1);
        }
    }
    while (len(src) >= (nint)(8 * ΔBlockSize)) {
        ctrBlocks8(ref nonnil(ref c).b, Ꮡ(array<byte>.Alias(dst, 128)), Ꮡ(array<byte>.Alias(src, 128)), ivlo, ivhi);
        src = src[(int)(8 * ΔBlockSize)..];
        dst = dst[(int)(8 * ΔBlockSize)..];
        (ivlo, ivhi) = add128(ivlo, ivhi, 8);
    }
    // The tail can have at most 7 = 4 + 2 + 1 blocks.
    if (len(src) >= (nint)(4 * ΔBlockSize)) {
        ctrBlocks4(ref nonnil(ref c).b, Ꮡ(array<byte>.Alias(dst, 64)), Ꮡ(array<byte>.Alias(src, 64)), ivlo, ivhi);
        src = src[(int)(4 * ΔBlockSize)..];
        dst = dst[(int)(4 * ΔBlockSize)..];
        (ivlo, ivhi) = add128(ivlo, ivhi, 4);
    }
    if (len(src) >= (nint)(2 * ΔBlockSize)) {
        ctrBlocks2(ref nonnil(ref c).b, Ꮡ(array<byte>.Alias(dst, 32)), Ꮡ(array<byte>.Alias(src, 32)), ivlo, ivhi);
        src = src[(int)(2 * ΔBlockSize)..];
        dst = dst[(int)(2 * ΔBlockSize)..];
        (ivlo, ivhi) = add128(ivlo, ivhi, 2);
    }
    if (len(src) >= (nint)(1 * ΔBlockSize)) {
        ctrBlocks1(ref nonnil(ref c).b, Ꮡ(array<byte>.Alias(dst, 16)), Ꮡ(array<byte>.Alias(src, 16)), ivlo, ivhi);
        src = src[(int)(1 * ΔBlockSize)..];
        dst = dst[(int)(1 * ΔBlockSize)..];
        (ivlo, ivhi) = add128(ivlo, ivhi, 1);
    }
    if (len(src) != 0) {
        // We have a partial block at the end.
        ref var @in = ref heap(new array<byte>(16), out var Ꮡin);
        ref var @out = ref heap(new array<byte>(16), out var Ꮡout);
        copy(@in[..], src);
        ctrBlocks1(ref nonnil(ref c).b, Ꮡout, Ꮡin, ivlo, ivhi);
        copy(dst, @out[..]);
    }
}

// Each ctrBlocksN function XORs src with N blocks of counter keystream, and
// stores it in dst. src is loaded in full before storing dst, so they can
// overlap even inexactly. The starting counter value is passed in as a pair of
// little-endian 64-bit integers.
internal static void ctrBlocks(ref Block b, slice<byte> dst, slice<byte> src, uint64 ivlo, uint64 ivhi) {
    var buf = new slice<byte>(len(src), 8 * ΔBlockSize);
    for (nint i = 0; i < len(buf); i += ΔBlockSize) {
        byteorder.BEPutUint64(buf.slice(i), ivhi);
        byteorder.BEPutUint64(buf.slice(i + 8), ivlo);
        (ivlo, ivhi) = add128(ivlo, ivhi, 1);
        encryptBlock(ref b, buf.slice(i), buf.slice(i));
    }
    // XOR into buf first, in case src and dst overlap (see above).
    subtle.XORBytes(buf, src, buf);
    copy(dst, buf);
}

internal static (uint64, uint64) add128(uint64 lo, uint64 hi, uint64 x) {
    (lo, var c) = bits.Add64(lo, x, 0);
    (hi, _) = bits.Add64(hi, 0, c);
    return (lo, hi);
}

} // end aes_package
