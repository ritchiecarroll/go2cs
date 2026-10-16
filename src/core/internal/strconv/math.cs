// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using bits = math.bits_package;
using math;

partial class strconv_package {

// A uint128 is a 128-bit uint.
// The fields are exported to make them visible to package strconv_test.
partial struct uint128 {
    public uint64 Hi;
    public uint64 Lo;
}

// umul128 returns the 128-bit product x*y.
internal static uint128 umul128(uint64 x, uint64 y) {
    var (hi, lo) = bits.Mul64(x, y);
    return new uint128(hi, lo);
}

// umul192 returns the 192-bit product x*y in three uint64s.
internal static (uint64 hi, uint64 mid, uint64 lo) umul192(uint64 x, uint128 y) {
    uint64 hi = default!;
    uint64 mid = default!;
    uint64 lo = default!;

    (var mid1, lo) = bits.Mul64(x, y.Lo);
    (hi, var mid2) = bits.Mul64(x, y.Hi);
    (mid, var carry) = bits.Add64(mid1, mid2, 0);
    return (hi + carry, mid, lo);
}

// pow10 returns the 128-bit mantissa and binary exponent of 10**e.
// That is, 10^e = mant/2^128 * 2**exp.
// If e is out of range, pow10 returns ok=false.
internal static (uint128 mant, nint exp, bool ok) pow10(nint e) {
    uint128 mant = default!;
    nint exp = default!;
    bool ok = default!;

    if (e < pow10Min || e > pow10Max) {
        return (mant, exp, ok);
    }
    return (pow10Tab[e - (nint)pow10Min], 1 + mulLog2_10(e), true);
}

// mulLog10_2 returns math.Floor(x * log(2)/log(10)) for an integer x in
// the range -1600 <= x && x <= +1600.
//
// The range restriction lets us work in faster integer arithmetic instead of
// slower floating point arithmetic. Correctness is verified by unit tests.
internal static nint mulLog10_2(nint x) {
    // log(2)/log(10) ≈ 0.30102999566 ≈ 78913 / 2^18
    return ((x * 78913) >> (int)(18));
}

// mulLog2_10 returns math.Floor(x * log(10)/log(2)) for an integer x in
// the range -500 <= x && x <= +500.
//
// The range restriction lets us work in faster integer arithmetic instead of
// slower floating point arithmetic. Correctness is verified by unit tests.
internal static nint mulLog2_10(nint x) {
    // log(10)/log(2) ≈ 3.32192809489 ≈ 108853 / 2^15
    return ((x * 108853) >> (int)(15));
}

internal static nuint bool2uint(bool b) {
    if (b) {
        return 1;
    }
    return 0;
}

// Exact Division and Remainder Checking
//
// An exact division x/c (exact means x%c == 0)
// can be implemented by x*m where m is the multiplicative inverse of c (m*c == 1).
//
// Since c is also the multiplicative inverse of m, x*m is lossless,
// and all the exact multiples of c map to all of [0, maxUint64/c].
// The non-multiples are forced to map to larger values.
// This also gives a quick test for whether x is an exact multiple of c:
// compute the exact division and check whether it's at most maxUint64/c:
//	x%c == 0 => x*m <= maxUint64/c.
//
// Only odd c have multiplicative inverses mod powers of two.
// To do an exact divide x / (c<<s) we can use (x/c)>>s instead.
// And to check for remainder, we need to check that those low s
// bits are all zero before we shift them away. We can merge that
// with the <= for the exact odd remainder check by rotating the
// shifted bits into the high part instead:
// 	x%(c<<s) == 0 => bits.RotateLeft64(x*m, -s) <= maxUint64/c.
//
// The compiler does this transformation automatically in general,
// but we apply it here by hand in a few ways that the compiler can't help with.
//
// For a more detailed explanation, see
// Henry S. Warren, Jr., Hacker's Delight, 2nd ed., sections 10-16 and 10-17.

// divisiblePow5 reports whether x is divisible by 5^p.
// It returns false for p not in [1, 22],
// because we only care about float64 mantissas, and 5^23 > 2^53.
internal static bool divisiblePow5(uint64 x, nint p) {
    return 1 <= p && p <= 22 && x * div5Tab[p - 1][0] <= div5Tab[p - 1][1];
}

internal static UntypedInt maxUint64 => /* 1<<64 - 1 */ 18446744073709551615;

// div5Tab[p-1] is the multiplicative inverse of 5^p and maxUint64/5^p.
internal static array<array<uint64>> div5Tab = new array<uint64>[]{
    new uint64[]{0xcccccccccccccccdUL, /* maxUint64 / 5 */ 3689348814741910323UL}.array(),
    new uint64[]{0x8f5c28f5c28f5c29UL, /* maxUint64 / 5 / 5 */ 737869762948382064UL}.array(),
    new uint64[]{0x1cac083126e978d5UL, /* maxUint64 / 5 / 5 / 5 */ 147573952589676412UL}.array(),
    new uint64[]{0xd288ce703afb7e91UL, /* maxUint64 / 5 / 5 / 5 / 5 */ 29514790517935282UL}.array(),
    new uint64[]{0x5d4e8fb00bcbe61dUL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 */ 5902958103587056UL}.array(),
    new uint64[]{0x790fb65668c26139UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 */ 1180591620717411UL}.array(),
    new uint64[]{0xe5032477ae8d46a5UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 236118324143482UL}.array(),
    new uint64[]{0xc767074b22e90e21UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 47223664828696UL}.array(),
    new uint64[]{0x8e47ce423a2e9c6dUL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 9444732965739UL}.array(),
    new uint64[]{0x4fa7f60d3ed61f49UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 1888946593147UL}.array(),
    new uint64[]{0x0fee64690c913975UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 377789318629UL}.array(),
    new uint64[]{0x3662e0e1cf503eb1UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 75557863725UL}.array(),
    new uint64[]{0xa47a2cf9f6433fbdUL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 15111572745UL}.array(),
    new uint64[]{0x54186f653140a659UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 3022314549UL}.array(),
    new uint64[]{0x7738164770402145UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 604462909UL}.array(),
    new uint64[]{0xe4a4d1417cd9a041UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 120892581UL}.array(),
    new uint64[]{0xc75429d9e5c5200dUL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 24178516UL}.array(),
    new uint64[]{0xc1773b91fac10669UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 4835703UL}.array(),
    new uint64[]{0x26b172506559ce15UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 967140UL}.array(),
    new uint64[]{0xd489e3a9addec2d1UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 193428UL}.array(),
    new uint64[]{0x90e860bb892c8d5dUL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 38685UL}.array(),
    new uint64[]{0x502e79bf1b6f4f79UL, /* maxUint64 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 / 5 */ 7737UL}.array()
}.array();

// trimZeros trims trailing zeros from x.
// It finds the largest p such that x % 10^p == 0
// and then returns x / 10^p, p.
//
// This is here for reference and tested, because it is an optimization
// used by other ftoa algorithms, but in our implementations it has
// never been benchmarked to be faster than trimming zeros after
// formatting into decimal bytes.
internal static (uint64, nint) trimZeros(uint64 x) {
    UntypedInt div1e8m = 0xc767074b22e90e21;
    const uint64 div1e8le = /* maxUint64 / 100000000 */ 184467440737;

    UntypedInt div1e4m = 0xd288ce703afb7e91;
    const uint64 div1e4le = /* maxUint64 / 10000 */ 1844674407370955;

    UntypedInt div1e2m = 0x8f5c28f5c28f5c29;
    const uint64 div1e2le = /* maxUint64 / 100 */ 184467440737095516;

    UntypedInt div1e1m = 0xcccccccccccccccd;
    const uint64 div1e1le = /* maxUint64 / 10 */ 1844674407370955161;

    // _ = assert[x - y] asserts at compile time that x == y.
    // Assert that the multiplicative inverses are correct
    // by checking that (div1eNm * 5^N) % 1<<64 == 1.
    array<EmptyStruct> assert = new(1);
    _ = assert[(nint)(/* (div1e8m*5*5*5*5*5*5*5*5)%(1<<64) - 1 */ 0L)];
    _ = assert[(nint)(/* (div1e4m*5*5*5*5)%(1<<64) - 1 */ 0L)];
    _ = assert[(nint)(/* (div1e2m*5*5)%(1<<64) - 1 */ 0L)];
    _ = assert[(nint)(/* (div1e1m*5)%(1<<64) - 1 */ 0L)];

    // Cut 8 zeros, then 4, then 2, then 1.
    nint p = 0;
    for (var d = bits.RotateLeft64(x * (uint64)div1e8m, -8); d <= div1e8le; d = bits.RotateLeft64(x * (uint64)div1e8m, -8)) {
        x = d;
        p += 8;
    }
    {
        var d = bits.RotateLeft64(x * (uint64)div1e4m, -4); if (d <= div1e4le) {
            x = d;
            p += 4;
        }
    }
    {
        var d = bits.RotateLeft64(x * (uint64)div1e2m, -2); if (d <= div1e2le) {
            x = d;
            p += 2;
        }
    }
    {
        var d = bits.RotateLeft64(x * (uint64)div1e1m, -1); if (d <= div1e1le) {
            x = d;
            p += 1;
        }
    }
    return (x, p);
}

} // end strconv_package
