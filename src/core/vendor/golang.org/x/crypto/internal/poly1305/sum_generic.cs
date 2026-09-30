// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file provides the generic implementation of Sum and MAC. Other files
// might provide optimized assembly implementations of some of this code.
namespace go.vendor.golang.org.x.crypto.@internal;

using binary = encoding.binary_package;
using bits = math.bits_package;
using encoding;
using math;

partial class poly1305_package {

// Poly1305 [RFC 7539] is a relatively simple algorithm: the authentication tag
// for a 64 bytes message is approximately
//
//     s + m[0:16] * r⁴ + m[16:32] * r³ + m[32:48] * r² + m[48:64] * r  mod  2¹³⁰ - 5
//
// for some secret r and s. It can be computed sequentially like
//
//     for len(msg) > 0:
//         h += read(msg, 16)
//         h *= r
//         h %= 2¹³⁰ - 5
//     return h + s
//
// All the complexity is about doing performant constant-time math on numbers
// larger than any available numeric type.
internal static void sumGeneric([GoArrayDims(16)] ж<array<byte>> Ꮡout, slice<byte> msg, [GoArrayDims(32)] ж<array<byte>> Ꮡkey) {
    ref var h = ref heap<macGeneric>(out var Ꮡh);
    h = newMACGeneric(Ꮡkey);
    Ꮡh.Write(msg);
    h.Sum(Ꮡout);
}

internal static macGeneric newMACGeneric([GoArrayDims(32)] ж<array<byte>> Ꮡkey) {
    var m = new macGeneric(nil);
    initialize(Ꮡkey, ref m.macState);
    return m.ΔClone();
}

// macState holds numbers in saturated 64-bit little-endian limbs. That is,
// the value of [x0, x1, x2] is x[0] + x[1] * 2⁶⁴ + x[2] * 2¹²⁸.
[GoType] partial struct macState {
    // h is the main accumulator. It is to be interpreted modulo 2¹³⁰ - 5, but
    // can grow larger during and after rounds. It must, however, remain below
    // 2 * (2¹³⁰ - 5).
    internal array<uint64> h = new(3);
    // r and s are the private key components.
    internal array<uint64> r = new(2);
    internal array<uint64> s = new(2);
}

[GoType] partial struct macGeneric {
    internal partial ref macState macState { get; }
    internal array<byte> buffer = new(TagSize);
    internal nint offset;
}

// Write splits the incoming message into TagSize chunks, and passes them to
// update. It buffers incomplete chunks.
internal static (nint, error) Write(this ж<macGeneric> Ꮡh, slice<byte> p) {
    ref var h = ref Ꮡh.DerefOrNull();

    nint nn = len(p);
    if (h.offset > 0) {
        nint n = copy(h.buffer.slice(h.offset), p);
        if (h.offset + n < TagSize) {
            h.offset += n;
            return (nn, default!);
        }
        p = p.slice(n);
        h.offset = 0;
        updateGeneric(ref nonnil(ref h).macState, h.buffer[..]);
    }
    {
        nint n = len(p) - (len(p) % (nint)TagSize); if (n > 0) {
            updateGeneric(ref nonnil(ref h).macState, p.slice(0, n));
            p = p.slice(n);
        }
    }
    if (len(p) > 0) {
        h.offset += copy(h.buffer.slice(h.offset), p);
    }
    return (nn, default!);
}

// Sum flushes the last incomplete chunk from the buffer, if any, and generates
// the MAC output. It does not modify its state, in order to allow for multiple
// calls to Sum, even if no Write is allowed after Sum.
[GoRecv] internal static void Sum(this ref macGeneric h, [GoArrayDims(16)] ж<array<byte>> Ꮡout) {
    var state = h.macState.ΔClone();
    if (h.offset > 0) {
        updateGeneric(ref state, h.buffer.slice(0, h.offset));
    }
    finalize(Ꮡout, ref state.h, ref state.s);
}

// [rMask0, rMask1] is the specified Poly1305 clamping mask in little-endian. It
// clears some bits of the secret coefficient to make it possible to implement
// multiplication more efficiently.
internal static UntypedInt rMask0 => 0x0FFFFFFC0FFFFFFF;

internal static UntypedInt rMask1 => 0x0FFFFFFC0FFFFFFC;

// initialize loads the 256-bit key into the two 128-bit secret values r and s.
internal static void initialize([GoArrayDims(32)] ж<array<byte>> Ꮡkey, ref macState m) {
    ref var key = ref Ꮡkey.DerefOrNull();

    m.r[0] = (uint64)(binary.LittleEndian.Uint64(key[0..8]) & (uint64)rMask0);
    m.r[1] = (uint64)(binary.LittleEndian.Uint64(key[8..16]) & (uint64)rMask1);
    m.s[0] = binary.LittleEndian.Uint64(key[16..24]);
    m.s[1] = binary.LittleEndian.Uint64(key[24..32]);
}

// uint128 holds a 128-bit number as two 64-bit limbs, for use with the
// bits.Mul64 and bits.Add64 intrinsics.
[GoType] partial struct uint128 {
    internal uint64 lo, hi;
}

internal static uint128 mul64(uint64 a, uint64 b) {
    var (hi, lo) = bits.Mul64(a, b);
    return new uint128(lo, hi);
}

internal static uint128 add128(uint128 a, uint128 b) {
    var (lo, c) = bits.Add64(a.lo, b.lo, 0);
    (var hi, c) = bits.Add64(a.hi, b.hi, c);
    if (c != 0) {
        throw panic("poly1305: unexpected overflow");
    }
    return new uint128(lo, hi);
}

internal static uint128 shiftRightBy2(uint128 a) {
    a.lo = (uint64)((a.lo >> (int)(2)) | (((uint64)(a.hi & 3)) << (int)(62)));
    a.hi = (a.hi >> (int)(2));
    return a;
}

// updateGeneric absorbs msg into the state.h accumulator. For each chunk m of
// 128 bits of message, it computes
//
//	h₊ = (h + m) * r  mod  2¹³⁰ - 5
//
// If the msg length is not a multiple of TagSize, it assumes the last
// incomplete chunk is the final one.
internal static void updateGeneric(ref macState state, slice<byte> msg) {
    var (h0, h1, h2) = (state.h[0], state.h[1], state.h[2]);
    var (r0, r1) = (state.r[0], state.r[1]);
    while (len(msg) > 0) {
        uint64 c = default!;
        // For the first step, h + m, we use a chain of bits.Add64 intrinsics.
        // The resulting value of h might exceed 2¹³⁰ - 5, but will be partially
        // reduced at the end of the multiplication below.
        //
        // The spec requires us to set a bit just above the message size, not to
        // hide leading zeroes. For full chunks, that's 1 << 128, so we can just
        // add 1 to the most significant (2¹²⁸) limb, h2.
        if (len(msg) >= TagSize){
            (h0, c) = bits.Add64(h0, binary.LittleEndian.Uint64(msg[0..8]), 0);
            (h1, c) = bits.Add64(h1, binary.LittleEndian.Uint64(msg[8..16]), c);
            h2 += c + 1;
            msg = msg[(int)(TagSize)..];
        } else {
            array<byte> buf = new(16); /* TagSize */
            copy(buf[..], msg);
            buf[len(msg)] = 1;
            (h0, c) = bits.Add64(h0, binary.LittleEndian.Uint64(buf[0..8]), 0);
            (h1, c) = bits.Add64(h1, binary.LittleEndian.Uint64(buf[8..16]), c);
            h2 += c;
            msg = default!;
        }
        // Multiplication of big number limbs is similar to elementary school
        // columnar multiplication. Instead of digits, there are 64-bit limbs.
        //
        // We are multiplying a 3 limbs number, h, by a 2 limbs number, r.
        //
        //                        h2    h1    h0  x
        //                              r1    r0  =
        //                       ----------------
        //                      h2r0  h1r0  h0r0     <-- individual 128-bit products
        //            +   h2r1  h1r1  h0r1
        //               ------------------------
        //                 m3    m2    m1    m0      <-- result in 128-bit overlapping limbs
        //               ------------------------
        //         m3.hi m2.hi m1.hi m0.hi           <-- carry propagation
        //     +         m3.lo m2.lo m1.lo m0.lo
        //        -------------------------------
        //           t4    t3    t2    t1    t0      <-- final result in 64-bit limbs
        //
        // The main difference from pen-and-paper multiplication is that we do
        // carry propagation in a separate step, as if we wrote two digit sums
        // at first (the 128-bit limbs), and then carried the tens all at once.
        var h0r0 = mul64(h0, r0);
        var h1r0 = mul64(h1, r0);
        var h2r0 = mul64(h2, r0);
        var h0r1 = mul64(h0, r1);
        var h1r1 = mul64(h1, r1);
        var h2r1 = mul64(h2, r1);
        // Since h2 is known to be at most 7 (5 + 1 + 1), and r0 and r1 have their
        // top 4 bits cleared by rMask{0,1}, we know that their product is not going
        // to overflow 64 bits, so we can ignore the high part of the products.
        //
        // This also means that the product doesn't have a fifth limb (t4).
        if (h2r0.hi != 0) {
            throw panic("poly1305: unexpected overflow");
        }
        if (h2r1.hi != 0) {
            throw panic("poly1305: unexpected overflow");
        }
        var m0 = h0r0;
        var m1 = add128(h1r0, h0r1); // These two additions don't overflow thanks again
        var m2 = add128(h2r0, h1r1); // to the 4 masked bits at the top of r0 and r1.
        var m3 = h2r1;
        var t0 = m0.lo;
        (var t1, c) = bits.Add64(m1.lo, m0.hi, 0);
        (var t2, c) = bits.Add64(m2.lo, m1.hi, c);
        var (t3, _) = bits.Add64(m3.lo, m2.hi, c);
        // Now we have the result as 4 64-bit limbs, and we need to reduce it
        // modulo 2¹³⁰ - 5. The special shape of this Crandall prime lets us do
        // a cheap partial reduction according to the reduction identity
        //
        //     c * 2¹³⁰ + n  =  c * 5 + n  mod  2¹³⁰ - 5
        //
        // because 2¹³⁰ = 5 mod 2¹³⁰ - 5. Partial reduction since the result is
        // likely to be larger than 2¹³⁰ - 5, but still small enough to fit the
        // assumptions we make about h in the rest of the code.
        //
        // See also https://speakerdeck.com/gtank/engineering-prime-numbers?slide=23
        // We split the final result at the 2¹³⁰ mark into h and cc, the carry.
        // Note that the carry bits are effectively shifted left by 2, in other
        // words, cc = c * 4 for the c in the reduction identity.
        (h0, h1, h2) = (t0, t1, (uint64)(t2 & maskLow2Bits));
        var cc = new uint128((uint64)(t2 & maskNotLow2Bits), t3);
        // To add c * 5 to h, we first add cc = c * 4, and then add (cc >> 2) = c.
        (h0, c) = bits.Add64(h0, cc.lo, 0);
        (h1, c) = bits.Add64(h1, cc.hi, c);
        h2 += c;
        cc = shiftRightBy2(cc);
        (h0, c) = bits.Add64(h0, cc.lo, 0);
        (h1, c) = bits.Add64(h1, cc.hi, c);
        h2 += c;
    }
    // h2 is at most 3 + 1 + 1 = 5, making the whole of h at most
    //
    //     5 * 2¹²⁸ + (2¹²⁸ - 1) = 6 * 2¹²⁸ - 1
    (state.h[0], state.h[1], state.h[2]) = (h0, h1, h2);
}

internal const uint64 maskLow2Bits = 0x0000000000000003;
internal const uint64 maskNotLow2Bits = /* ^maskLow2Bits */ 18446744073709551612;

// select64 returns x if v == 1 and y if v == 0, in constant time.
internal static uint64 select64(uint64 v, uint64 x, uint64 y) {
    return (uint64)((uint64)(~(v - 1) & x) | (uint64)((v - 1) & y));
}

// [p0, p1, p2] is 2¹³⁰ - 5 in little endian order.
internal static UntypedInt p0 => 0xFFFFFFFFFFFFFFFB;

internal static UntypedInt p1 => 0xFFFFFFFFFFFFFFFF;

internal static UntypedInt p2 => 0x0000000000000003;

// finalize completes the modular reduction of h and computes
//
//	out = h + s  mod  2¹²⁸
internal static void finalize([GoArrayDims(16)] ж<array<byte>> Ꮡout, ref array<uint64> h, ref array<uint64> s) {
    ref var @out = ref Ꮡout.DerefOrNull();

    var (h0, h1, h2) = (h[0], h[1], h[2]);
    // After the partial reduction in updateGeneric, h might be more than
    // 2¹³⁰ - 5, but will be less than 2 * (2¹³⁰ - 5). To complete the reduction
    // in constant time, we compute t = h - (2¹³⁰ - 5), and select h as the
    // result if the subtraction underflows, and t otherwise.
    var (hMinusP0, b) = bits.Sub64(h0, p0, 0);
    (var hMinusP1, b) = bits.Sub64(h1, p1, b);
    (_, b) = bits.Sub64(h2, p2, b);
    // h = h if h < p else h - p
    h0 = select64(b, h0, hMinusP0);
    h1 = select64(b, h1, hMinusP1);
    // Finally, we compute the last Poly1305 step
    //
    //     tag = h + s  mod  2¹²⁸
    //
    // by just doing a wide addition with the 128 low bits of h and discarding
    // the overflow.
    (h0, var c) = bits.Add64(h0, s[0], 0);
    (h1, _) = bits.Add64(h1, s[1], c);
    binary.LittleEndian.PutUint64(@out[0..8], h0);
    binary.LittleEndian.PutUint64(@out[8..16], h1);
}

} // end poly1305_package
