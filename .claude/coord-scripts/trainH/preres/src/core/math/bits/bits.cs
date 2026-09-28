// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:generate go run make_tables.go

// Package bits implements bit counting and manipulation
// functions for the predeclared unsigned integer types.
//
// Functions in this package may be implemented directly by
// the compiler, for better performance. For those functions
// the code in this package will not be used. Which
// functions are implemented by the compiler depends on the
// architecture and the Go release.
namespace go.math;

partial class bits_package {

internal static UntypedInt uintSize => /* 32 << (^uint(0) >> 63) */ 64; // 32 or 64

// UintSize is the size of a uint in bits.
public static UntypedInt UintSize => /* uintSize */ 64;

// --- LeadingZeros ---

// LeadingZeros returns the number of leading zero bits in x; the result is [UintSize] for x == 0.
public static nint LeadingZeros(nuint x) {
    return (nint)UintSize - Len(x);
}

// LeadingZeros8 returns the number of leading zero bits in x; the result is 8 for x == 0.
public static nint LeadingZeros8(uint8 x) {
    return 8 - Len8(x);
}

// LeadingZeros16 returns the number of leading zero bits in x; the result is 16 for x == 0.
public static nint LeadingZeros16(uint16 x) {
    return 16 - Len16(x);
}

// LeadingZeros32 returns the number of leading zero bits in x; the result is 32 for x == 0.
public static nint LeadingZeros32(uint32 x) {
    return 32 - Len32(x);
}

// LeadingZeros64 returns the number of leading zero bits in x; the result is 64 for x == 0.
public static nint LeadingZeros64(uint64 x) {
    return 64 - Len64(x);
}

// --- TrailingZeros ---

// See http://keithandkatie.com/keith/papers/debruijn.html
internal static UntypedInt deBruijn32 => 0x077CB531;

internal static array<byte> deBruijn32tab = new byte[]{
    0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
    31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
}.array();

internal static UntypedInt deBruijn64 => 0x03f79d71b4ca8b09;

internal static array<byte> deBruijn64tab = new byte[]{
    0, 1, 56, 2, 57, 49, 28, 3, 61, 58, 42, 50, 38, 29, 17, 4,
    62, 47, 59, 36, 45, 43, 51, 22, 53, 39, 33, 30, 24, 18, 12, 5,
    63, 55, 48, 27, 60, 41, 37, 16, 46, 35, 44, 21, 52, 32, 23, 11,
    54, 26, 40, 15, 34, 20, 31, 10, 25, 14, 19, 9, 13, 8, 7, 6
}.array();

// TrailingZeros returns the number of trailing zero bits in x; the result is [UintSize] for x == 0.
public static nint TrailingZeros(nuint x) {
    if (UintSize == 32) {
        return TrailingZeros32((uint32)x);
    }
    return TrailingZeros64((uint64)x);
}

// TrailingZeros8 returns the number of trailing zero bits in x; the result is 8 for x == 0.
public static nint TrailingZeros8(uint8 x) {
    return (nint)ntz8tab[x];
}

// TrailingZeros16 returns the number of trailing zero bits in x; the result is 16 for x == 0.
public static nint TrailingZeros16(uint16 x) {
    if (x == 0) {
        return 16;
    }
    // see comment in TrailingZeros64
    return (nint)deBruijn32tab[((uint32)((uint16)(x & ((uint16)0 - x))) * (uint32)deBruijn32 >> (int)((32 - 5)))];
}

// TrailingZeros32 returns the number of trailing zero bits in x; the result is 32 for x == 0.
public static nint TrailingZeros32(uint32 x) {
    if (x == 0) {
        return 32;
    }
    // see comment in TrailingZeros64
    return (nint)deBruijn32tab[(((uint32)(x & ((uint32)0 - x))) * (uint32)deBruijn32 >> (int)((32 - 5)))];
}

// TrailingZeros64 returns the number of trailing zero bits in x; the result is 64 for x == 0.
public static nint TrailingZeros64(uint64 x) {
    if (x == 0) {
        return 64;
    }
    // If popcount is fast, replace code below with return popcount(^x & (x - 1)).
    //
    // x & -x leaves only the right-most bit set in the word. Let k be the
    // index of that bit. Since only a single bit is set, the value is two
    // to the power of k. Multiplying by a power of two is equivalent to
    // left shifting, in this case by k bits. The de Bruijn (64 bit) constant
    // is such that all six bit, consecutive substrings are distinct.
    // Therefore, if we have a left shifted version of this constant we can
    // find by how many bits it was shifted by looking at which six bit
    // substring ended up at the top of the word.
    // (Knuth, volume 4, section 7.3.1)
    return (nint)deBruijn64tab[(((uint64)(x & ((uint64)0 - x))) * (uint64)deBruijn64 >> (int)((64 - 6)))];
}

// --- OnesCount ---
internal static UntypedInt m0 => 0x5555555555555555; // 01010101 ...

internal static UntypedInt m1 => 0x3333333333333333; // 00110011 ...

internal static UntypedInt m2 => 0x0f0f0f0f0f0f0f0f; // 00001111 ...

internal static UntypedInt m3 => 0x00ff00ff00ff00ff; // etc.

internal static UntypedInt m4 => 0x0000ffff0000ffff;

// OnesCount returns the number of one bits ("population count") in x.
public static nint OnesCount(nuint x) {
    if (UintSize == 32) {
        return OnesCount32((uint32)x);
    }
    return OnesCount64((uint64)x);
}

// OnesCount8 returns the number of one bits ("population count") in x.
public static nint OnesCount8(uint8 x) {
    return (nint)pop8tab[x];
}

// OnesCount16 returns the number of one bits ("population count") in x.
public static nint OnesCount16(uint16 x) {
    return (nint)((byte)(pop8tab[(x >> (int)(8))] + pop8tab[(uint16)(x & 0xff)]));
}

// OnesCount32 returns the number of one bits ("population count") in x.
public static nint OnesCount32(uint32 x) {
    return (nint)((byte)(pop8tab[(x >> (int)(24))] + pop8tab[(uint32)((x >> (int)(16)) & 0xff)] + pop8tab[(uint32)((x >> (int)(8)) & 0xff)] + pop8tab[(uint32)(x & 0xff)]));
}

// OnesCount64 returns the number of one bits ("population count") in x.
public static nint OnesCount64(uint64 x) {
    // Implementation: Parallel summing of adjacent bits.
    // See "Hacker's Delight", Chap. 5: Counting Bits.
    // The following pattern shows the general approach:
    //
    //   x = x>>1&(m0&m) + x&(m0&m)
    //   x = x>>2&(m1&m) + x&(m1&m)
    //   x = x>>4&(m2&m) + x&(m2&m)
    //   x = x>>8&(m3&m) + x&(m3&m)
    //   x = x>>16&(m4&m) + x&(m4&m)
    //   x = x>>32&(m5&m) + x&(m5&m)
    //   return int(x)
    //
    // Masking (& operations) can be left away when there's no
    // danger that a field's sum will carry over into the next
    // field: Since the result cannot be > 64, 8 bits is enough
    // and we can ignore the masks for the shifts by 8 and up.
    // Per "Hacker's Delight", the first line can be simplified
    // more, but it saves at best one instruction, so we leave
    // it alone for clarity.
    UntypedInt m = /* 1<<64 - 1 */ 18446744073709551615;
    x = (uint64)((x >> (int)(1)) & ((uint64)((uint64)m0 & (uint64)m))) + (uint64)(x & ((uint64)((uint64)m0 & (uint64)m)));
    x = (uint64)((x >> (int)(2)) & ((uint64)((uint64)m1 & (uint64)m))) + (uint64)(x & ((uint64)((uint64)m1 & (uint64)m)));
    x = (uint64)(((x >> (int)(4)) + x) & ((uint64)((uint64)m2 & (uint64)m)));
    x += (x >> (int)(8));
    x += (x >> (int)(16));
    x += (x >> (int)(32));
    return (nint)((nint)x & (nint)((1 << (int)(7)) - 1));
}

// --- RotateLeft ---

// RotateLeft returns the value of x rotated left by (k mod [UintSize]) bits.
// To rotate x right by k bits, call RotateLeft(x, -k).
//
// This function's execution time does not depend on the inputs.
public static nuint RotateLeft(nuint x, nint k) {
    if (UintSize == 32) {
        return (nuint)RotateLeft32((uint32)x, k);
    }
    return (nuint)RotateLeft64((uint64)x, k);
}

// RotateLeft8 returns the value of x rotated left by (k mod 8) bits.
// To rotate x right by k bits, call RotateLeft8(x, -k).
//
// This function's execution time does not depend on the inputs.
public static uint8 RotateLeft8(uint8 x, nint k) {
    UntypedInt n = 8;
    nuint s = (nuint)((nuint)k & (nuint)(n - 1));
    return (uint8)(x.Lsh(s) | x.Rsh(((nuint)n - s)));
}

// RotateLeft16 returns the value of x rotated left by (k mod 16) bits.
// To rotate x right by k bits, call RotateLeft16(x, -k).
//
// This function's execution time does not depend on the inputs.
public static uint16 RotateLeft16(uint16 x, nint k) {
    UntypedInt n = 16;
    nuint s = (nuint)((nuint)k & (nuint)(n - 1));
    return (uint16)(x.Lsh(s) | x.Rsh(((nuint)n - s)));
}

// RotateLeft32 returns the value of x rotated left by (k mod 32) bits.
// To rotate x right by k bits, call RotateLeft32(x, -k).
//
// This function's execution time does not depend on the inputs.
public static uint32 RotateLeft32(uint32 x, nint k) {
    UntypedInt n = 32;
    nuint s = (nuint)((nuint)k & (nuint)(n - 1));
    return (uint32)(x.Lsh(s) | x.Rsh(((nuint)n - s)));
}

// RotateLeft64 returns the value of x rotated left by (k mod 64) bits.
// To rotate x right by k bits, call RotateLeft64(x, -k).
//
// This function's execution time does not depend on the inputs.
public static uint64 RotateLeft64(uint64 x, nint k) {
    UntypedInt n = 64;
    nuint s = (nuint)((nuint)k & (nuint)(n - 1));
    return (uint64)(x.Lsh(s) | x.Rsh(((nuint)n - s)));
}

// --- Reverse ---

// Reverse returns the value of x with its bits in reversed order.
public static nuint Reverse(nuint x) {
    if (UintSize == 32) {
        return (nuint)Reverse32((uint32)x);
    }
    return (nuint)Reverse64((uint64)x);
}

// Reverse8 returns the value of x with its bits in reversed order.
public static uint8 Reverse8(uint8 x) {
    return rev8tab[x];
}

// Reverse16 returns the value of x with its bits in reversed order.
public static uint16 Reverse16(uint16 x) {
    return (uint16)((uint16)rev8tab[(x >> (int)(8))] | (uint16)((uint16)rev8tab[(uint16)(x & 0xff)] << (int)(8)));
}

// Reverse32 returns the value of x with its bits in reversed order.
public static uint32 Reverse32(uint32 x) {
    UntypedInt m = /* 1<<32 - 1 */ 4294967295;
    x = (uint32)((uint32)((x >> (int)(1)) & ((uint32)((uint32)m0 & (uint32)m))) | ((uint32)(x & ((uint32)((uint32)m0 & (uint32)m))) << (int)(1)));
    x = (uint32)((uint32)((x >> (int)(2)) & ((uint32)((uint32)m1 & (uint32)m))) | ((uint32)(x & ((uint32)((uint32)m1 & (uint32)m))) << (int)(2)));
    x = (uint32)((uint32)((x >> (int)(4)) & ((uint32)((uint32)m2 & (uint32)m))) | ((uint32)(x & ((uint32)((uint32)m2 & (uint32)m))) << (int)(4)));
    return ReverseBytes32(x);
}

// Reverse64 returns the value of x with its bits in reversed order.
public static uint64 Reverse64(uint64 x) {
    UntypedInt m = /* 1<<64 - 1 */ 18446744073709551615;
    x = (uint64)((uint64)((x >> (int)(1)) & ((uint64)((uint64)m0 & (uint64)m))) | ((uint64)(x & ((uint64)((uint64)m0 & (uint64)m))) << (int)(1)));
    x = (uint64)((uint64)((x >> (int)(2)) & ((uint64)((uint64)m1 & (uint64)m))) | ((uint64)(x & ((uint64)((uint64)m1 & (uint64)m))) << (int)(2)));
    x = (uint64)((uint64)((x >> (int)(4)) & ((uint64)((uint64)m2 & (uint64)m))) | ((uint64)(x & ((uint64)((uint64)m2 & (uint64)m))) << (int)(4)));
    return ReverseBytes64(x);
}

// --- ReverseBytes ---

// ReverseBytes returns the value of x with its bytes in reversed order.
//
// This function's execution time does not depend on the inputs.
public static nuint ReverseBytes(nuint x) {
    if (UintSize == 32) {
        return (nuint)ReverseBytes32((uint32)x);
    }
    return (nuint)ReverseBytes64((uint64)x);
}

// ReverseBytes16 returns the value of x with its bytes in reversed order.
//
// This function's execution time does not depend on the inputs.
public static uint16 ReverseBytes16(uint16 x) {
    return (uint16)((x >> (int)(8)) | (uint16)(x << (int)(8)));
}

// ReverseBytes32 returns the value of x with its bytes in reversed order.
//
// This function's execution time does not depend on the inputs.
public static uint32 ReverseBytes32(uint32 x) {
    UntypedInt m = /* 1<<32 - 1 */ 4294967295;
    x = (uint32)((uint32)((x >> (int)(8)) & ((uint32)((uint32)m3 & (uint32)m))) | ((uint32)(x & ((uint32)((uint32)m3 & (uint32)m))) << (int)(8)));
    return (uint32)((x >> (int)(16)) | (x << (int)(16)));
}

// ReverseBytes64 returns the value of x with its bytes in reversed order.
//
// This function's execution time does not depend on the inputs.
public static uint64 ReverseBytes64(uint64 x) {
    UntypedInt m = /* 1<<64 - 1 */ 18446744073709551615;
    x = (uint64)((uint64)((x >> (int)(8)) & ((uint64)((uint64)m3 & (uint64)m))) | ((uint64)(x & ((uint64)((uint64)m3 & (uint64)m))) << (int)(8)));
    x = (uint64)((uint64)((x >> (int)(16)) & ((uint64)((uint64)m4 & (uint64)m))) | ((uint64)(x & ((uint64)((uint64)m4 & (uint64)m))) << (int)(16)));
    return (uint64)((x >> (int)(32)) | (x << (int)(32)));
}

// --- Len ---

// Len returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint Len(nuint x) {
    if (UintSize == 32) {
        return Len32((uint32)x);
    }
    return Len64((uint64)x);
}

// Len8 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint Len8(uint8 x) {
    return (nint)len8tab[x];
}

// Len16 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint /*n*/ Len16(uint16 x) {
    nint n = default!;

    if (x >= (uint16)(1 << (int)(8))) {
        x >>= (int)(8);
        n = 8;
    }
    return n + (nint)len8tab[x];
}

// Len32 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint /*n*/ Len32(uint32 x) {
    nint n = default!;

    if (x >= ((uint32)1 << (int)(16))) {
        x >>= (int)(16);
        n = 16;
    }
    if (x >= ((uint32)1 << (int)(8))) {
        x >>= (int)(8);
        n += 8;
    }
    return n + (nint)len8tab[x];
}

// Len64 returns the minimum number of bits required to represent x; the result is 0 for x == 0.
public static nint /*n*/ Len64(uint64 x) {
    nint n = default!;

    if (x >= ((uint64)1 << (int)(32))) {
        x >>= (int)(32);
        n = 32;
    }
    if (x >= ((uint64)1 << (int)(16))) {
        x >>= (int)(16);
        n += 16;
    }
    if (x >= ((uint64)1 << (int)(8))) {
        x >>= (int)(8);
        n += 8;
    }
    return n + (nint)len8tab[x];
}

// --- Add with carry ---
// go2cs generated this placeholder — func Add is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Add32 returns the sum with carry of x, y and carry: sum = x + y + carry.
// The carry input must be 0 or 1; otherwise the behavior is undefined.
// The carryOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
public static (uint32 sum, uint32 carryOut) Add32(uint32 x, uint32 y, uint32 carry) {
    uint32 sum = default!;
    uint32 carryOut = default!;

    var sum64 = (uint64)x + (uint64)y + (uint64)carry;
    sum = (uint32)sum64;
    carryOut = (uint32)((sum64 >> (int)(32)));
    return (sum, carryOut);
}

// Add64 returns the sum with carry of x, y and carry: sum = x + y + carry.
// The carry input must be 0 or 1; otherwise the behavior is undefined.
// The carryOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
public static (uint64 sum, uint64 carryOut) Add64(uint64 x, uint64 y, uint64 carry) {
    uint64 sum = default!;
    uint64 carryOut = default!;

    sum = x + y + carry;
    // The sum will overflow if both top bits are set (x & y) or if one of them
    // is (x | y), and a carry from the lower place happened. If such a carry
    // happens, the top bit will be 1 + 0 + 1 = 0 (&^ sum).
    carryOut = (((uint64)(((uint64)(x & y)) | ((uint64)(((uint64)(x | y)) & ~sum)))) >> (int)(63));
    return (sum, carryOut);
}

// --- Subtract with borrow ---
// go2cs generated this placeholder — func Sub is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Sub32 returns the difference of x, y and borrow, diff = x - y - borrow.
// The borrow input must be 0 or 1; otherwise the behavior is undefined.
// The borrowOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
public static (uint32 diff, uint32 borrowOut) Sub32(uint32 x, uint32 y, uint32 borrow) {
    uint32 diff = default!;
    uint32 borrowOut = default!;

    diff = x - y - borrow;
    // The difference will underflow if the top bit of x is not set and the top
    // bit of y is set (^x & y) or if they are the same (^(x ^ y)) and a borrow
    // from the lower place happens. If that borrow happens, the result will be
    // 1 - 1 - 1 = 0 - 0 - 1 = 1 (& diff).
    borrowOut = (((uint32)(((uint32)(~x & y)) | ((uint32)(~((uint32)(x ^ y)) & diff)))) >> (int)(31));
    return (diff, borrowOut);
}

// Sub64 returns the difference of x, y and borrow: diff = x - y - borrow.
// The borrow input must be 0 or 1; otherwise the behavior is undefined.
// The borrowOut output is guaranteed to be 0 or 1.
//
// This function's execution time does not depend on the inputs.
public static (uint64 diff, uint64 borrowOut) Sub64(uint64 x, uint64 y, uint64 borrow) {
    uint64 diff = default!;
    uint64 borrowOut = default!;

    diff = x - y - borrow;
    // See Sub32 for the bit logic.
    borrowOut = (((uint64)(((uint64)(~x & y)) | ((uint64)(~((uint64)(x ^ y)) & diff)))) >> (int)(63));
    return (diff, borrowOut);
}

// --- Full-width multiply ---
// go2cs generated this placeholder — func Mul is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// Mul32 returns the 64-bit product of x and y: (hi, lo) = x * y
// with the product bits' upper half returned in hi and the lower
// half returned in lo.
//
// This function's execution time does not depend on the inputs.
public static (uint32 hi, uint32 lo) Mul32(uint32 x, uint32 y) {
    uint32 hi = default!;
    uint32 lo = default!;

    var tmp = (uint64)x * (uint64)y;
    (hi, lo) = ((uint32)((tmp >> (int)(32))), (uint32)tmp);
    return (hi, lo);
}

// Mul64 returns the 128-bit product of x and y: (hi, lo) = x * y
// with the product bits' upper half returned in hi and the lower
// half returned in lo.
//
// This function's execution time does not depend on the inputs.
public static (uint64 hi, uint64 lo) Mul64(uint64 x, uint64 y) {
    uint64 hi = default!;
    uint64 lo = default!;

    const uint64 mask32 = /* 1<<32 - 1 */ 4294967295;
    var x0 = (uint64)(x & mask32);
    var x1 = (x >> (int)(32));
    var y0 = (uint64)(y & mask32);
    var y1 = (y >> (int)(32));
    var w0 = x0 * y0;
    var t = x1 * y0 + (w0 >> (int)(32));
    var w1 = (uint64)(t & mask32);
    var w2 = (t >> (int)(32));
    w1 += x0 * y1;
    hi = x1 * y1 + w2 + (w1 >> (int)(32));
    lo = x * y;
    return (hi, lo);
}

// --- Full-width divide ---

// Div returns the quotient and remainder of (hi, lo) divided by y:
// quo = (hi, lo)/y, rem = (hi, lo)%y with the dividend bits' upper
// half in parameter hi and the lower half in parameter lo.
// Div panics for y == 0 (division by zero) or y <= hi (quotient overflow).
public static (nuint quo, nuint rem) Div(nuint hi, nuint lo, nuint y) {
    if (UintSize == 32) {
        var (qΔ1, rΔ1) = Div32((uint32)hi, (uint32)lo, (uint32)y);
        return ((nuint)qΔ1, (nuint)rΔ1);
    }
    var (q, r) = Div64((uint64)hi, (uint64)lo, (uint64)y);
    return ((nuint)q, (nuint)r);
}

// Div32 returns the quotient and remainder of (hi, lo) divided by y:
// quo = (hi, lo)/y, rem = (hi, lo)%y with the dividend bits' upper
// half in parameter hi and the lower half in parameter lo.
// Div32 panics for y == 0 (division by zero) or y <= hi (quotient overflow).
public static (uint32 quo, uint32 rem) Div32(uint32 hi, uint32 lo, uint32 y) {
    uint32 quo = default!;
    uint32 rem = default!;

    if (y != 0 && y <= hi) {
        throw panic(overflowError);
    }
    var z = (uint64)(((uint64)hi << (int)(32)) | (uint64)lo);
    (quo, rem) = ((uint32)(z / (uint64)y), (uint32)(z % (uint64)y));
    return (quo, rem);
}

// Div64 returns the quotient and remainder of (hi, lo) divided by y:
// quo = (hi, lo)/y, rem = (hi, lo)%y with the dividend bits' upper
// half in parameter hi and the lower half in parameter lo.
// Div64 panics for y == 0 (division by zero) or y <= hi (quotient overflow).
public static (uint64 quo, uint64 rem) Div64(uint64 hi, uint64 lo, uint64 y) {
    if (y == 0) {
        throw panic(divideError);
    }
    if (y <= hi) {
        throw panic(overflowError);
    }
    // If high part is zero, we can directly return the results.
    if (hi == 0) {
        return (lo / y, lo % y);
    }
    nuint s = (nuint)LeadingZeros64(y);
    y.LshAssign(s);
    UntypedInt two32 = /* 1 << 32 */ 4294967296;
    const uint64 mask32 = /* two32 - 1 */ 4294967295;
    var yn1 = (y >> (int)(32));
    var yn0 = (uint64)(y & mask32);
    var un32 = (uint64)(hi.Lsh(s) | lo.Rsh((64 - s)));
    var un10 = lo.Lsh(s);
    var un1 = (un10 >> (int)(32));
    var un0 = (uint64)(un10 & mask32);
    var q1 = un32 / yn1;
    var rhat = un32 - q1 * yn1;
    while (q1 >= two32 || q1 * yn0 > (uint64)two32 * rhat + un1) {
        q1--;
        rhat += yn1;
        if (rhat >= two32) {
            break;
        }
    }
    var un21 = un32 * (uint64)two32 + un1 - q1 * y;
    var q0 = un21 / yn1;
    rhat = un21 - q0 * yn1;
    while (q0 >= two32 || q0 * yn0 > (uint64)two32 * rhat + un0) {
        q0--;
        rhat += yn1;
        if (rhat >= two32) {
            break;
        }
    }
    return (q1 * (uint64)two32 + q0, (un21 * (uint64)two32 + un0 - q0 * y).Rsh(s));
}

// Rem returns the remainder of (hi, lo) divided by y. Rem panics for
// y == 0 (division by zero) but, unlike Div, it doesn't panic on a
// quotient overflow.
public static nuint Rem(nuint hi, nuint lo, nuint y) {
    if (UintSize == 32) {
        return (nuint)Rem32((uint32)hi, (uint32)lo, (uint32)y);
    }
    return (nuint)Rem64((uint64)hi, (uint64)lo, (uint64)y);
}

// Rem32 returns the remainder of (hi, lo) divided by y. Rem32 panics
// for y == 0 (division by zero) but, unlike [Div32], it doesn't panic
// on a quotient overflow.
public static uint32 Rem32(uint32 hi, uint32 lo, uint32 y) {
    return (uint32)(((uint64)(((uint64)hi << (int)(32)) | (uint64)lo)) % (uint64)y);
}

// Rem64 returns the remainder of (hi, lo) divided by y. Rem64 panics
// for y == 0 (division by zero) but, unlike [Div64], it doesn't panic
// on a quotient overflow.
public static uint64 Rem64(uint64 hi, uint64 lo, uint64 y) {
    // We scale down hi so that hi < y, then use Div64 to compute the
    // rem with the guarantee that it won't panic on quotient overflow.
    // Given that
    //   hi ≡ hi%y    (mod y)
    // we have
    //   hi<<64 + lo ≡ (hi%y)<<64 + lo    (mod y)
    var (_, rem) = Div64(hi % y, lo, y);
    return rem;
}

} // end bits_package
