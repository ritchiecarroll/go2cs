// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δmath = math_package;
using rand = go.math.rand_package;
using static strconv_package;
using testing = testing_package;
using go.math;
using static go.strconv_internal_test_package;

partial class strconv_test_package {

partial struct ftoaTest {
    internal float64 f;
    internal byte fmt;
    internal nint prec;
    internal @string s;
}

internal static float64 fdiv(float64 a, float64 b) {
    return a / b;
}

internal static readonly GoBigConst below1e23 = /* 99999999999999974834176 */
    GoBigConst.Parse("99999999999999974834176");
internal static readonly GoBigConst above1e23 = /* 100000000000000008388608 */
    GoBigConst.Parse("100000000000000008388608");

// g conversion and zero suppression
// Round to even
// avoid constant arithmetic
// avoid constant arithmetic
// fixed bugs
// https://www.exploringbinary.com/java-hangs-when-converting-2-2250738585072012e-308/
// https://www.exploringbinary.com/php-hangs-on-numeric-value-2-2250738585072011e-308/
// Issue 2625.
// Issue 29491.
// Issue 52187
// rounding
internal static ж<slice<ftoaTest>> Ꮡftoatests = new StandardBox<slice<ftoaTest>>(new ftoaTest[]{
    new(1D, (rune)'e', 5, "1.00000e+00"u8),
    new(1D, (rune)'f', 5, "1.00000"u8),
    new(1D, (rune)'g', 5, "1"u8),
    new(1D, (rune)'g', -1, "1"u8),
    new(1D, (rune)'x', -1, "0x1p+00"u8),
    new(1D, (rune)'x', 5, "0x1.00000p+00"u8),
    new(20D, (rune)'g', -1, "20"u8),
    new(20D, (rune)'x', -1, "0x1.4p+04"u8),
    new(1234567.8D, (rune)'g', -1, "1.2345678e+06"u8),
    new(1234567.8D, (rune)'x', -1, "0x1.2d687cccccccdp+20"u8),
    new(200000D, (rune)'g', -1, "200000"u8),
    new(200000D, (rune)'x', -1, "0x1.86ap+17"u8),
    new(200000D, (rune)'X', -1, "0X1.86AP+17"u8),
    new(2000000D, (rune)'g', -1, "2e+06"u8),
    new(1e10D, (rune)'g', -1, "1e+10"u8),
    new(400D, (rune)'g', 2, "4e+02"u8),
    new(40D, (rune)'g', 2, "40"u8),
    new(4D, (rune)'g', 2, "4"u8),
    new(.4D, (rune)'g', 2, "0.4"u8),
    new(.04D, (rune)'g', 2, "0.04"u8),
    new(.004D, (rune)'g', 2, "0.004"u8),
    new(.0004D, (rune)'g', 2, "0.0004"u8),
    new(.00004D, (rune)'g', 2, "4e-05"u8),
    new(.000004D, (rune)'g', 2, "4e-06"u8),
    new(0D, (rune)'e', 5, "0.00000e+00"u8),
    new(0D, (rune)'f', 5, "0.00000"u8),
    new(0D, (rune)'g', 5, "0"u8),
    new(0D, (rune)'g', -1, "0"u8),
    new(0D, (rune)'x', 5, "0x0.00000p+00"u8),
    new(-1D, (rune)'e', 5, "-1.00000e+00"u8),
    new(-1D, (rune)'f', 5, "-1.00000"u8),
    new(-1D, (rune)'g', 5, "-1"u8),
    new(-1D, (rune)'g', -1, "-1"u8),
    new(12D, (rune)'e', 5, "1.20000e+01"u8),
    new(12D, (rune)'f', 5, "12.00000"u8),
    new(12D, (rune)'g', 5, "12"u8),
    new(12D, (rune)'g', -1, "12"u8),
    new(123456700D, (rune)'e', 5, "1.23457e+08"u8),
    new(123456700D, (rune)'f', 5, "123456700.00000"u8),
    new(123456700D, (rune)'g', 5, "1.2346e+08"u8),
    new(123456700D, (rune)'g', -1, "1.234567e+08"u8),
    new(1.2345e6D, (rune)'e', 5, "1.23450e+06"u8),
    new(1.2345e6D, (rune)'f', 5, "1234500.00000"u8),
    new(1.2345e6D, (rune)'g', 5, "1.2345e+06"u8),
    new(1.2345e6D, (rune)'e', 3, "1.234e+06"u8),
    new(1.2355e6D, (rune)'e', 3, "1.236e+06"u8),
    new(1.2345D, (rune)'f', 3, "1.234"u8),
    new(1.2355D, (rune)'f', 3, "1.236"u8),
    new(1234567890123456.5D, (rune)'e', 15, "1.234567890123456e+15"u8),
    new(1234567890123457.5D, (rune)'e', 15, "1.234567890123458e+15"u8),
    new(108678236358137.625D, (rune)'g', -1, "1.0867823635813762e+14"u8),
    new(1e23D, (rune)'e', 17, "9.99999999999999916e+22"u8),
    new(1e23D, (rune)'f', 17, "99999999999999991611392.00000000000000000"u8),
    new(1e23D, (rune)'g', 17, "9.9999999999999992e+22"u8),
    new(1e23D, (rune)'e', -1, "1e+23"u8),
    new(1e23D, (rune)'f', -1, "100000000000000000000000"u8),
    new(1e23D, (rune)'g', -1, "1e+23"u8),
    new((float64)below1e23, (rune)'e', 17, "9.99999999999999748e+22"u8),
    new((float64)below1e23, (rune)'f', 17, "99999999999999974834176.00000000000000000"u8),
    new((float64)below1e23, (rune)'g', 17, "9.9999999999999975e+22"u8),
    new((float64)below1e23, (rune)'e', -1, "9.999999999999997e+22"u8),
    new((float64)below1e23, (rune)'f', -1, "99999999999999970000000"u8),
    new((float64)below1e23, (rune)'g', -1, "9.999999999999997e+22"u8),
    new((float64)above1e23, (rune)'e', 17, "1.00000000000000008e+23"u8),
    new((float64)above1e23, (rune)'f', 17, "100000000000000008388608.00000000000000000"u8),
    new((float64)above1e23, (rune)'g', 17, "1.0000000000000001e+23"u8),
    new((float64)above1e23, (rune)'e', -1, "1.0000000000000001e+23"u8),
    new((float64)above1e23, (rune)'f', -1, "100000000000000010000000"u8),
    new((float64)above1e23, (rune)'g', -1, "1.0000000000000001e+23"u8),
    new(fdiv(5e-304D, 1e20D), (rune)'g', -1, "5e-324"u8),
    new(fdiv(-5e-304D, 1e20D), (rune)'g', -1, "-5e-324"u8),
    new(32D, (rune)'g', -1, "32"u8),
    new(32D, (rune)'g', 0, "3e+01"u8),
    new(100D, (rune)'x', -1, "0x1.9p+06"u8),
    new(100D, (rune)'y', -1, "%y"u8),
    new(Δmath.NaN(), (rune)'g', -1, "NaN"u8),
    new(-Δmath.NaN(), (rune)'g', -1, "NaN"u8),
    new(Δmath.Inf(0), (rune)'g', -1, "+Inf"u8),
    new(Δmath.Inf(-1), (rune)'g', -1, "-Inf"u8),
    new(-Δmath.Inf(0), (rune)'g', -1, "-Inf"u8),
    new(-1D, (rune)'b', -1, "-4503599627370496p-52"u8),
    new(0.9D, (rune)'f', 1, "0.9"u8),
    new(0.09D, (rune)'f', 1, "0.1"u8),
    new(0.0999D, (rune)'f', 1, "0.1"u8),
    new(0.05D, (rune)'f', 1, "0.1"u8),
    new(0.05D, (rune)'f', 0, "0"u8),
    new(0.5D, (rune)'f', 1, "0.5"u8),
    new(0.5D, (rune)'f', 0, "0"u8),
    new(1.5D, (rune)'f', 0, "2"u8),
    new(2.2250738585072012e-308D, (rune)'g', -1, "2.2250738585072014e-308"u8),
    new(2.2250738585072011e-308D, (rune)'g', -1, "2.225073858507201e-308"u8),
    new(383260575764816448D, (rune)'f', 0, "383260575764816448"u8),
    new(383260575764816448D, (rune)'g', -1, "3.8326057576481645e+17"u8),
    new(498484681984085570D, (rune)'f', -1, "498484681984085570"u8),
    new(-5.8339553793802237e+23D, (rune)'g', -1, "-5.8339553793802237e+23"u8),
    new(123.45D, (rune)'?', 0, "%?"u8),
    new(123.45D, (rune)'?', 1, "%?"u8),
    new(123.45D, (rune)'?', -1, "%?"u8),
    new(2.275555555555555D, (rune)'x', -1, "0x1.23456789abcdep+01"u8),
    new(2.275555555555555D, (rune)'x', 0, "0x1p+01"u8),
    new(2.275555555555555D, (rune)'x', 2, "0x1.23p+01"u8),
    new(2.275555555555555D, (rune)'x', 16, "0x1.23456789abcde000p+01"u8),
    new(2.275555555555555D, (rune)'x', 21, "0x1.23456789abcde00000000p+01"u8),
    new(2.2755555510520935D, (rune)'x', -1, "0x1.2345678p+01"u8),
    new(2.2755555510520935D, (rune)'x', 6, "0x1.234568p+01"u8),
    new(2.275555431842804D, (rune)'x', -1, "0x1.2345668p+01"u8),
    new(2.275555431842804D, (rune)'x', 6, "0x1.234566p+01"u8),
    new(3.999969482421875D, (rune)'x', -1, "0x1.ffffp+01"u8),
    new(3.999969482421875D, (rune)'x', 4, "0x1.ffffp+01"u8),
    new(3.999969482421875D, (rune)'x', 3, "0x1.000p+02"u8),
    new(3.999969482421875D, (rune)'x', 2, "0x1.00p+02"u8),
    new(3.999969482421875D, (rune)'x', 1, "0x1.0p+02"u8),
    new(3.999969482421875D, (rune)'x', 0, "0x1p+02"u8)
}.slice());
internal static ref slice<ftoaTest> ftoatests => ref Ꮡftoatests.ValueSlot;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testN64ˢ = (@string)"testN=64"u8;
internal static readonly object wantˢ2 = (@string)"want"u8;
internal static readonly object gotˢ2 = (@string)"got"u8;
internal static readonly object appendFloatTestN64ˢ = (@string)"AppendFloat testN=64"u8;
internal static readonly object testN32ˢ = (@string)"testN=32"u8;
internal static readonly object appendFloatTestN32ˢ = (@string)"AppendFloat testN=32"u8;

public static void TestFtoa(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(ftoatests); i++) {
        var test = Ꮡ(ftoatests, i);
        @string s = FormatFloat((~test).f, (~test).fmt, (~test).prec, 64);
        if (s != (~test).s) {
            Ꮡt.Error(testN64ˢ, (~test).f, ((@string)(~test).fmt), (~test).prec, wantˢ2, (~test).s, gotˢ2, s);
        }
        var x = AppendFloat(slice<byte>("abc"u8), (~test).f, (~test).fmt, (~test).prec, 64);
        if (((@string)x) != "abc"u8 + (~test).s) {
            Ꮡt.Error(appendFloatTestN64ˢ, (~test).f, ((@string)(~test).fmt), (~test).prec, wantˢ2, "abc" + (~test).s, gotˢ2, ((@string)x));
        }
        if ((float64)(float32)(~test).f == (~test).f && (~test).fmt != (rune)'b') {
            @string sΔ1 = FormatFloat((~test).f, (~test).fmt, (~test).prec, 32);
            if (sΔ1 != (~test).s) {
                Ꮡt.Error(testN32ˢ, (~test).f, ((@string)(~test).fmt), (~test).prec, wantˢ2, (~test).s, gotˢ2, sΔ1);
            }
            var xΔ1 = AppendFloat(slice<byte>("abc"u8), (~test).f, (~test).fmt, (~test).prec, 32);
            if (((@string)xΔ1) != "abc"u8 + (~test).s) {
                Ꮡt.Error(appendFloatTestN32ˢ, (~test).f, ((@string)(~test).fmt), (~test).prec, wantˢ2, "abc" + (~test).s, gotˢ2, ((@string)xΔ1));
            }
        }
    }
}

public static void TestFtoaPowersOfTwo(ж<testing.T> Ꮡt) {
    for (nint exp = -2048; exp <= 2048; exp++) {
        var f = Δmath.Ldexp(1D, exp);
        if (!Δmath.IsInf(f, 0)) {
            @string s = FormatFloat(f, (rune)'e', -1, 64);
            {
                var (x, _) = ParseFloat(s, 64); if (x != f) {
                    Ꮡt.Errorf("failed roundtrip %v => %s => %v"u8, f, s, x);
                }
            }
        }
        var f32 = (float32)f;
        if (!Δmath.IsInf((float64)f32, 0)) {
            @string s = FormatFloat((float64)f32, (rune)'e', -1, 32);
            {
                var (x, _) = ParseFloat(s, 32); if ((float32)x != f32) {
                    Ꮡt.Errorf("failed roundtrip %v => %s => %v"u8, f32, s, (float32)x);
                }
            }
        }
    }
}

public static void TestFtoaRandom(ж<testing.T> Ꮡt) {
    nint N = (nint)10000;
    if (testing.Short()) {
        N = 100;
    }
    Ꮡt.Logf("testing %d random numbers with fast and slow FormatFloat"u8, N);
    for (nint i = 0; i < N; i++) {
        var bits = (uint64)(((uint64)rand.Uint32() << (int)(32)) | (uint64)rand.Uint32());
        var x = Δmath.Float64frombits(bits);
        @string shortFast = FormatFloat(x, (rune)'g', -1, 64);
        strconv_internal_test_package.SetOptimize(false);
        @string shortSlow = FormatFloat(x, (rune)'g', -1, 64);
        strconv_internal_test_package.SetOptimize(true);
        if (shortSlow != shortFast) {
            Ꮡt.Errorf("%b printed as %s, want %s"u8, x, shortFast, shortSlow);
        }
        nint prec = rand.Intn(12) + 5;
        shortFast = FormatFloat(x, (rune)'e', prec, 64);
        strconv_internal_test_package.SetOptimize(false);
        shortSlow = FormatFloat(x, (rune)'e', prec, 64);
        strconv_internal_test_package.SetOptimize(true);
        if (shortSlow != shortFast) {
            Ꮡt.Errorf("%b printed as %s, want %s"u8, x, shortFast, shortSlow);
        }
    }
}

public static void TestFormatFloatInvalidBitSize(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatalf("expected panic due to invalid bitSize"u8);
                }
            }
        }, ref ᒐ);
        _ = FormatFloat(3.14D, (rune)'g', -1, 100);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// From testdata/testfp.txt
// Trigger slow path (see issue #15672).
// The shortest is: 8.034137530808823e+43
// This denormal is pathological because the lower/upper
// halfways to neighboring floats are:
// 622666234635.321003e-320 ~= 622666234635.321e-320
// 622666234635.321497e-320 ~= 622666234635.3215e-320
// making it hard to find the 3rd digit

partial struct ftoaBenchesᴛ1 /*dyn*/ {
    internal @string name;
    internal float64 @float;
    internal byte fmt;
    internal nint prec;
    internal nint bitSize;
}
internal static slice<ftoaBenchesᴛ1> ftoaBenches = new ftoaBenchesᴛ1[]{
    new("Decimal"u8, 33909D, (rune)'g', -1, 64),
    new("Float"u8, 339.7784D, (rune)'g', -1, 64),
    new("Exp"u8, -5.09e75D, (rune)'g', -1, 64),
    new("NegExp"u8, -5.11e-95D, (rune)'g', -1, 64),
    new("LongExp"u8, 1.234567890123456e-78D, (rune)'g', -1, 64),
    new("Big"u8, 123456789123456789123456789D, (rune)'g', -1, 64),
    new("BinaryExp"u8, -1D, (rune)'b', -1, 64),
    new("32Integer"u8, 33909D, (rune)'g', -1, 32),
    new("32ExactFraction"u8, 3.375D, (rune)'g', -1, 32),
    new("32Point"u8, 339.7784D, (rune)'g', -1, 32),
    new("32Exp"u8, -5.09e25D, (rune)'g', -1, 32),
    new("32NegExp"u8, -5.11e-25D, (rune)'g', -1, 32),
    new("32Shortest"u8, 1.234567e-8D, (rune)'g', -1, 32),
    new("32Fixed8Hard"u8, Δmath.Ldexp(15961084D, -125), (rune)'e', 8, 32),
    new("32Fixed9Hard"u8, Δmath.Ldexp(14855922D, -83), (rune)'e', 9, 32),
    new("64Fixed1"u8, 123456D, (rune)'e', 3, 64),
    new("64Fixed2"u8, 123.456D, (rune)'e', 3, 64),
    new("64Fixed3"u8, 1.23456e+78D, (rune)'e', 3, 64),
    new("64Fixed4"u8, 1.23456e-78D, (rune)'e', 3, 64),
    new("64Fixed12"u8, 1.23456e-78D, (rune)'e', 12, 64),
    new("64Fixed16"u8, 1.23456e-78D, (rune)'e', 16, 64),
    new("64Fixed12Hard"u8, Δmath.Ldexp(6965949469487146D, -249), (rune)'e', 12, 64),
    new("64Fixed17Hard"u8, Δmath.Ldexp(8887055249355788D, 665), (rune)'e', 17, 64),
    new("64Fixed18Hard"u8, Δmath.Ldexp(6994187472632449D, 690), (rune)'e', 18, 64),
    new("Slowpath64"u8, 8.03413753080882349e+43D, (rune)'e', -1, 64),
    new("SlowpathDenormal64"u8, 622666234635.3213e-320D, (rune)'e', -1, 64)
}.slice();

public static void BenchmarkFormatFloat(ж<testing.B> Ꮡb) {
    foreach (var (_, vᴛ1) in ftoaBenches) {
        ref var c = ref heap(new ftoaBenchesᴛ1(), out var Ꮡc);
        c = vᴛ1;

        var cʗ1 = c;
        Ꮡb.Run(c.name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                FormatFloat(cʗ1.@float, cʗ1.fmt, cʗ1.prec, cʗ1.bitSize);
            }
        });
    }
}

public static void BenchmarkAppendFloat(ж<testing.B> Ꮡb) {
    var dst = new slice<byte>(30);
    foreach (var (_, vᴛ1) in ftoaBenches) {
        ref var c = ref heap(new ftoaBenchesᴛ1(), out var Ꮡc);
        c = vᴛ1;

        var cʗ1 = c;
        var dstʗ1 = dst;
        Ꮡb.Run(c.name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                AppendFloat(dstʗ1[..0], cʗ1.@float, cʗ1.fmt, cʗ1.prec, cʗ1.bitSize);
            }
        });
    }
}

} // end strconv_test_package
