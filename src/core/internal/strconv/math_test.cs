// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.strconv_package;
using math = math_package;
using testing = testing_package;
using static go.@internal.strconv_internal_test_package;
using strconv = go.@internal.strconv_package;

partial class strconv_test_package {

partial struct pow10Testsᴛ1 /*dyn*/ {
    internal nint exp10;
    internal uint128 mant;
    internal nint exp2;
    internal bool ok;
}
internal static slice<pow10Testsᴛ1> pow10Tests = new pow10Testsᴛ1[]{
    new(-349, new uint128(0, 0), 0, false),
    new(-348, new uint128(0xFA8FD5A0081C0288UL, 0x1732C869CD60E453UL), -1156, true),
    new(0, new uint128(0x8000000000000000UL, 0x0000000000000000), 1, true),
    new(347, new uint128(0xD13EB46469447567UL, 0x4B7195F2D2D1A9FBUL), 1153, true),
    new(348, new uint128(0, 0), 0, false)
}.slice();

public static void TestPow10(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in pow10Tests) {
        var (mant, exp2, ok) = pow10(tt.exp10);
        if (mant != tt.mant || exp2 != tt.exp2) {
            Ꮡt.Errorf("pow10(%d) = %#016x, %#016x, %d, %v want %#016x,%#016x, %d, %v"u8,
                tt.exp10, mant.Hi, mant.Lo, exp2, ok,
                tt.mant.Hi, tt.mant.Lo, tt.exp2, tt.ok);
        }
    }

    for (nint p = pow10Min; p <= pow10Max; p++) {
        var (mant, exp2, ok) = pow10(p);
        if (!ok) {
            Ꮡt.Errorf("pow10(%d) not ok"u8, p);
            continue;
        }
        // Note: -64 instead of -128 because we only used mant.Hi, not all of mant.
        var have = math.Ldexp((float64)mant.Hi, exp2 - 64);
        var want = math.Pow(10D, (float64)p);
        if (math.Abs(have - want) / want > 0.00001D) {
            Ꮡt.Errorf("pow10(%d) = %#016x%016x/2^128 * 2^%d = %g want ~%g"u8, p, mant.Hi, mant.Lo, exp2, have, want);
        }
    }
}

internal static uint128 u128(uint64 hi, uint64 lo) {
    return new uint128(Hi: hi, Lo: lo);
}

partial struct umul192Testsᴛ1 /*dyn*/ {
    internal uint64 x;
    internal uint128 y;
    internal uint64 hi;
    internal uint64 mid;
    internal uint64 lo;
}
internal static slice<umul192Testsᴛ1> umul192Tests = new umul192Testsᴛ1[]{
    new(0, u128(0, 0), 0, 0, 0),
    new(~(uint64)0, u128(~(uint64)0, ~(uint64)0), ~(uint64)1, ~(uint64)0, 1)
}.slice();

public static void TestUmul192(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in umul192Tests) {
        var (hi, mid, lo) = strconv_internal_test_package.Umul192(tt.x, tt.y);
        if (hi != tt.hi || mid != tt.mid || lo != tt.lo) {
            Ꮡt.Errorf("umul192(%#x, {%#x,%#x}) = %#x, %#x, %#x, want %#x, %#x, %#x"u8,
                tt.x, tt.y.Hi, tt.y.Lo, hi, mid, lo, tt.hi, tt.mid, tt.lo);
        }
    }
}

public static void TestMulLog10_2(ж<testing.T> Ꮡt) {
    for (nint x = -1600; x <= +1600; x++) {
        nint iMath = mulLog10_2(x);
        nint fMath = (nint)math.Floor((float64)x * (float64)math.Ln2 / (float64)math.Ln10);
        if (iMath != fMath) {
            Ꮡt.Errorf("mulLog10_2(%d) failed: %d vs %d\n"u8, x, iMath, fMath);
        }
    }
}

public static void TestMulLog2_10(ж<testing.T> Ꮡt) {
    for (nint x = -500; x <= +500; x++) {
        nint iMath = mulLog2_10(x);
        nint fMath = (nint)math.Floor((float64)x * (float64)math.Ln10 / (float64)math.Ln2);
        if (iMath != fMath) {
            Ꮡt.Errorf("mulLog2_10(%d) failed: %d vs %d\n"u8, x, iMath, fMath);
        }
    }
}

internal static uint64 pow5(nint p) {
    var x = (uint64)1;
    foreach (var _ᴛ1 in range(p)) {
        x *= 5;
    }
    return x;
}

public static void TestDivisiblePow5(ж<testing.T> Ꮡt) {
    for (nint p = 1; p <= 22; p++) {
        var x = pow5(p);
        if (divisiblePow5(1, p)) {
            Ꮡt.Errorf("divisiblePow5(1, %d) = true, want, false"u8, p);
        }
        if (divisiblePow5(x - 1, p)) {
            Ꮡt.Errorf("divisiblePow5(%d, %d) = true, want false"u8, x - 1, p);
        }
        if (divisiblePow5(x + 1, p)) {
            Ꮡt.Errorf("divisiblePow5(%d, %d) = true, want false"u8, x - 1, p);
        }
        if (divisiblePow5(x / 5, p)) {
            Ꮡt.Errorf("divisiblePow5(%d, %d) = true, want false"u8, x / 5, p);
        }
        if (!divisiblePow5(0, p)) {
            Ꮡt.Errorf("divisiblePow5(0, %d) = false, want true"u8, p);
        }
        if (!divisiblePow5(x, p)) {
            Ꮡt.Errorf("divisiblePow5(%d, %d) = false, want true"u8, x, p);
        }
        if (2 * x > x && !divisiblePow5(2 * x, p)) {
            Ꮡt.Errorf("divisiblePow5(%d, %d) = false, want true"u8, 2 * x, p);
        }
    }
}

public static void TestDiv5Tab(ж<testing.T> Ꮡt) {
    for (nint p = 1; p <= 22; p++) {
        var m = div5Tab[p - 1][0];
        var le = div5Tab[p - 1][1];

        // See comment in math.go on div5Tab.
        // m needs to be multiplicative inverse of pow5(p).
        if (m * pow5(p) != 1) {
            Ꮡt.Errorf("pow5Tab[%d-1][0] = %#x, but %#x * (5**%d) = %d, want 1"u8, p, m, m, p, m * pow5(p));
        }

        // le needs to be ⌊(1<<64 - 1) / 5^p⌋.
        var want = (18446744073709551615UL) / pow5(p);
        if (le != want) {
            Ꮡt.Errorf("pow5Tab[%d-1][1] = %#x, want %#x"u8, p, le, want);
        }
    }
}

public static void TestTrimZeros(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new uint64[]{1, 2, 3, 4, 101, 123}.slice()) {
        var x = vᴛ1;

        var want = x;
        foreach (var p in range(20)) {
            var (haveX, haveP) = trimZeros(x);
            if (haveX != want || haveP != p) {
                Ꮡt.Errorf("trimZeros(%d) = %d, %d, want %d, %d"u8, x, haveX, haveP, want, p);
            }
            if (x >= 1844674407370955161UL) {
                break;
            }
            x *= 10;
        }
    }
}

} // end strconv_test_package
