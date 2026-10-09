// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using bytes = bytes_package;
using fmt = fmt_package;
using math = math_package;
using bits = go.math.bits_package;
using strconv = strconv_package;
using testing = testing_package;
using go.math;
using io = io_package;
using static go.math.big_package;

partial class big_internal_test_package {

internal static float64 zero_;

internal partial struct TestFloatSetFloat64String_type /*dyn*/ {
    internal @string s;
    internal float64 x; // NaNs represent invalid inputs
}

public static void TestFloatSetFloat64String(ж<testing.T> Ꮡt) {
    var inf = math.Inf(0);
    var nan = math.NaN();
    foreach (var (_, test) in new TestFloatSetFloat64String_type[]{ // basics

        new("0"u8, 0D),
        new("-0"u8, -zero_),
        new("+0"u8, 0D),
        new("1"u8, 1D),
        new("-1"u8, -1D),
        new("+1"u8, 1D),
        new("1.234"u8, 1.234D),
        new("-1.234"u8, -1.234D),
        new("+1.234"u8, 1.234D),
        new(".1"u8, 0.1D),
        new("1."u8, 1D),
        new("+1."u8, 1D), // various zeros

        new("0e100"u8, 0D),
        new("-0e+100"u8, -zero_),
        new("+0e-100"u8, 0D),
        new("0E100"u8, 0D),
        new("-0E+100"u8, -zero_),
        new("+0E-100"u8, 0D), // various decimal exponent formats

        new("1.e10"u8, 1e10D),
        new("1e+10"u8, 1e10D),
        new("+1e-10"u8, 1e-10D),
        new("1E10"u8, 1e10D),
        new("1.E+10"u8, 1e10D),
        new("+1E-10"u8, 1e-10D), // infinities

        new("Inf"u8, inf),
        new("+Inf"u8, inf),
        new("-Inf"u8, -inf),
        new("inf"u8, inf),
        new("+inf"u8, inf),
        new("-inf"u8, -inf), // invalid numbers

        new(""u8, nan),
        new("-"u8, nan),
        new("0x"u8, nan),
        new("0e"u8, nan),
        new("1.2ef"u8, nan),
        new("2..3"u8, nan),
        new("123.."u8, nan),
        new("infinity"u8, nan),
        new("foobar"u8, nan), // invalid underscores

        new("_"u8, nan),
        new("0_"u8, nan),
        new("1__0"u8, nan),
        new("123_."u8, nan),
        new("123._"u8, nan),
        new("123._4"u8, nan),
        new("1_2.3_4_"u8, nan),
        new("_.123"u8, nan),
        new("_123.456"u8, nan),
        new("10._0"u8, nan),
        new("10.0e_0"u8, nan),
        new("10.0e0_"u8, nan),
        new("0P-0__0"u8, nan), // misc decimal values

        new("3.14159265"u8, 3.14159265D),
        new("-687436.79457e-245"u8, -687436.79457e-245D),
        new("-687436.79457E245"u8, -687436.79457e245D),
        new(".0000000000000000000000000000000000000001"u8, 1e-40D),
        new("+10000000000000000000000000000000000000000e-0"u8, 1e40D), // decimal mantissa, binary exponent

        new("0p0"u8, 0D),
        new("-0p0"u8, -zero_),
        new("1p10"u8, (1 << (int)(10))),
        new("1p+10"u8, (1 << (int)(10))),
        new("+1p-10"u8, 1.0D / ((1 << (int)(10)))),
        new("1024p-12"u8, 0.25D),
        new("-1p10"u8, -1024D),
        new("1.5p1"u8, 3D), // binary mantissa, decimal exponent

        new("0b0"u8, 0D),
        new("-0b0"u8, -zero_),
        new("0b0e+10"u8, 0D),
        new("-0b0e-10"u8, -zero_),
        new("0b1010"u8, 10D),
        new("0B1010E2"u8, 1000D),
        new("0b.1"u8, 0.5D),
        new("0b.001"u8, 0.125D),
        new("0b.001e3"u8, 125D), // binary mantissa, binary exponent

        new("0b0p+10"u8, 0D),
        new("-0b0p-10"u8, -zero_),
        new("0b.1010p4"u8, 10D),
        new("0b1p-1"u8, 0.5D),
        new("0b001p-3"u8, 0.125D),
        new("0b.001p3"u8, 1D),
        new("0b0.01p2"u8, 1D),
        new("0b0.01P+2"u8, 1D), // octal mantissa, decimal exponent

        new("0o0"u8, 0D),
        new("-0o0"u8, -zero_),
        new("0o0e+10"u8, 0D),
        new("-0o0e-10"u8, -zero_),
        new("0o12"u8, 10D),
        new("0O12E2"u8, 1000D),
        new("0o.4"u8, 0.5D),
        new("0o.01"u8, 0.015625D),
        new("0o.01e3"u8, 15.625D), // octal mantissa, binary exponent

        new("0o0p+10"u8, 0D),
        new("-0o0p-10"u8, -zero_),
        new("0o.12p6"u8, 10D),
        new("0o4p-3"u8, 0.5D),
        new("0o0014p-6"u8, 0.1875D),
        new("0o.001p9"u8, 1D),
        new("0o0.01p7"u8, 2D),
        new("0O0.01P+2"u8, 0.0625D), // hexadecimal mantissa and exponent

        new("0x0"u8, 0D),
        new("-0x0"u8, -zero_),
        new("0x0p+10"u8, 0D),
        new("-0x0p-10"u8, -zero_),
        new("0xff"u8, 255D),
        new("0X.8p1"u8, 1D),
        new("-0X0.00008p16"u8, -0.5D),
        new("-0X0.00008P+16"u8, -0.5D),
        new("0x0.0000000000001p-1022"u8, math.SmallestNonzeroFloat64),
        new("0x1.fffffffffffffp1023"u8, math.MaxFloat64), // underscores

        new("0_0"u8, 0D),
        new("1_000."u8, 1000D),
        new("1_2_3.4_5_6"u8, 123.456D),
        new("1.0e0_0"u8, 1D),
        new("1p+1_0"u8, 1024D),
        new("0b_1000"u8, 8D),
        new("0b_1011_1101"u8, 189D),
        new("0x_f0_0d_1eP+0_8"u8, 4027391488D)
    }.slice()) {
        ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
        Ꮡx.SetPrec(53);
        var (_, ok) = Ꮡx.SetString(test.s);
        if (math.IsNaN(test.x)) {
            // test.s is invalid
            if (ok) {
                Ꮡt.Errorf("%s: want parse error"u8, test.s);
            }
            continue;
        }
        // test.s is valid
        if (!ok) {
            Ꮡt.Errorf("%s: got parse error"u8, test.s);
            continue;
        }
        var (f, _) = Ꮡx.Float64();
        var want = @new<global::go.math.big_package.Float>().SetFloat64(test.x);
        if (Ꮡx.Cmp(want) != 0 || x.Signbit() != want.Signbit()) {
            Ꮡt.Errorf("%s: got %v (%v); want %v"u8, test.s, Ꮡx, f, test.x);
        }
    }
}

internal static float64 fdiv(float64 a, float64 b) {
    return a / b;
}

internal static readonly GoBigConst below1e23 = /* 99999999999999974834176 */
    GoBigConst.Parse("99999999999999974834176");
internal static readonly GoBigConst above1e23 = /* 100000000000000008388608 */
    GoBigConst.Parse("100000000000000008388608");

internal partial struct TestFloat64Text_type /*dyn*/ {
    internal float64 x;
    internal byte format;
    internal nint prec;
    internal @string want;
}

public static void TestFloat64Text(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new TestFloat64Text_type[]{
        new(0D, (rune)'f', 0, "0"u8),
        new(math.Copysign(0D, -1D), (rune)'f', 0, "-0"u8),
        new(1D, (rune)'f', 0, "1"u8),
        new(-1D, (rune)'f', 0, "-1"u8),
        new(0.001D, (rune)'e', 0, "1e-03"u8),
        new(0.459D, (rune)'e', 0, "5e-01"u8),
        new(1.459D, (rune)'e', 0, "1e+00"u8),
        new(2.459D, (rune)'e', 1, "2.5e+00"u8),
        new(3.459D, (rune)'e', 2, "3.46e+00"u8),
        new(4.459D, (rune)'e', 3, "4.459e+00"u8),
        new(5.459D, (rune)'e', 4, "5.4590e+00"u8),
        new(0.001D, (rune)'f', 0, "0"u8),
        new(0.459D, (rune)'f', 0, "0"u8),
        new(1.459D, (rune)'f', 0, "1"u8),
        new(2.459D, (rune)'f', 1, "2.5"u8),
        new(3.459D, (rune)'f', 2, "3.46"u8),
        new(4.459D, (rune)'f', 3, "4.459"u8),
        new(5.459D, (rune)'f', 4, "5.4590"u8),
        new(0D, (rune)'b', 0, "0"u8),
        new(math.Copysign(0D, -1D), (rune)'b', 0, "-0"u8),
        new(1.0D, (rune)'b', 0, "4503599627370496p-52"u8),
        new(-1.0D, (rune)'b', 0, "-4503599627370496p-52"u8),
        new(4503599627370496D, (rune)'b', 0, "4503599627370496p+0"u8),
        new(0D, (rune)'p', 0, "0"u8),
        new(math.Copysign(0D, -1D), (rune)'p', 0, "-0"u8),
        new(1024.0D, (rune)'p', 0, "0x.8p+11"u8),
        new(-1024.0D, (rune)'p', 0, "-0x.8p+11"u8), // all test cases below from strconv/ftoa_test.go

        new(1D, (rune)'e', 5, "1.00000e+00"u8),
        new(1D, (rune)'f', 5, "1.00000"u8),
        new(1D, (rune)'g', 5, "1"u8),
        new(1D, (rune)'g', -1, "1"u8),
        new(20D, (rune)'g', -1, "20"u8),
        new(1234567.8D, (rune)'g', -1, "1.2345678e+06"u8),
        new(200000D, (rune)'g', -1, "200000"u8),
        new(2000000D, (rune)'g', -1, "2e+06"u8), // g conversion and zero suppression

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
        new(5e-304D / 1e20D, (rune)'g', -1, "5e-324"u8),
        new(-5e-304D / 1e20D, (rune)'g', -1, "-5e-324"u8),
        new(fdiv(5e-304D, 1e20D), (rune)'g', -1, "5e-324"u8), // avoid constant arithmetic

        new(fdiv(-5e-304D, 1e20D), (rune)'g', -1, "-5e-324"u8), // avoid constant arithmetic

        new(32D, (rune)'g', -1, "32"u8),
        new(32D, (rune)'g', 0, "3e+01"u8),
        new(100D, (rune)'x', -1, "0x1.9p+06"u8), // {math.NaN(), 'g', -1, "NaN"},  // Float doesn't support NaNs
 // {-math.NaN(), 'g', -1, "NaN"}, // Float doesn't support NaNs

        new(math.Inf(0), (rune)'g', -1, "+Inf"u8),
        new(math.Inf(-1), (rune)'g', -1, "-Inf"u8),
        new(-math.Inf(0), (rune)'g', -1, "-Inf"u8),
        new(-1D, (rune)'b', -1, "-4503599627370496p-52"u8), // fixed bugs

        new(0.9D, (rune)'f', 1, "0.9"u8),
        new(0.09D, (rune)'f', 1, "0.1"u8),
        new(0.0999D, (rune)'f', 1, "0.1"u8),
        new(0.05D, (rune)'f', 1, "0.1"u8),
        new(0.05D, (rune)'f', 0, "0"u8),
        new(0.5D, (rune)'f', 1, "0.5"u8),
        new(0.5D, (rune)'f', 0, "0"u8),
        new(1.5D, (rune)'f', 0, "2"u8), // https://www.exploringbinary.com/java-hangs-when-converting-2-2250738585072012e-308/

        new(2.2250738585072012e-308D, (rune)'g', -1, "2.2250738585072014e-308"u8), // https://www.exploringbinary.com/php-hangs-on-numeric-value-2-2250738585072011e-308/

        new(2.2250738585072011e-308D, (rune)'g', -1, "2.225073858507201e-308"u8), // Issue 2625.

        new(383260575764816448D, (rune)'f', 0, "383260575764816448"u8),
        new(383260575764816448D, (rune)'g', -1, "3.8326057576481645e+17"u8), // Issue 15918.

        new(1D, (rune)'f', -10, "1"u8),
        new(1D, (rune)'f', -11, "1"u8),
        new(1D, (rune)'f', -12, "1"u8)
    }.slice()) {
        // The test cases are from the strconv package which tests float64 values.
        // When formatting values with prec = -1 (shortest representation),
        // the actually available mantissa precision matters.
        // For denormalized values, that precision is < 53 (SetFloat64 default).
        // Compute and set the actual precision explicitly.
        var f = @new<global::go.math.big_package.Float>().SetPrec(actualPrec(test.x)).SetFloat64(test.x);
        @string got = f.Text(test.format, test.prec);
        if (got != test.want) {
            Ꮡt.Errorf("%v: got %s; want %s"u8, test, got, test.want);
            continue;
        }
        if (test.format == (rune)'b' && test.x == 0D) {
            continue; // 'b' format in strconv.Float requires knowledge of bias for 0.0
        }
        if (test.format == (rune)'p') {
            continue; // 'p' format not supported in strconv.Format
        }
        // verify that Float format matches strconv format
        @string want = strconv.FormatFloat(test.x, test.format, test.prec, 64);
        if (got != want) {
            Ꮡt.Errorf("%v: got %s; want %s (strconv)"u8, test, got, want);
        }
    }
}

// actualPrec returns the number of actually used mantissa bits.
internal static nuint actualPrec(float64 x) {
    {
        var mant = math.Float64bits(x); if (x != 0D && (uint64)(mant & (((uint64)0x7ff << (int)(52)))) == 0) {
            // x is denormalized
            return 64 - (nuint)bits.LeadingZeros64((uint64)(mant & ((uint64)(4503599627370496L - 1))));
        }
    }
    return 53;
}

internal partial struct TestFloatText_type /*dyn*/ {
    internal @string x;
    internal global::go.math.big_package.RoundingMode round;
    internal nuint prec;
    internal byte format;
    internal nint digits;
    internal @string want;
}

public static void TestFloatText(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    global::go.math.big_package.RoundingMode defaultRound = /* ^RoundingMode(0) */ 255;
    foreach (var (_, test) in new TestFloatText_type[]{
        new("0"u8, defaultRound, 10, (rune)'f', 0, "0"u8),
        new("-0"u8, defaultRound, 10, (rune)'f', 0, "-0"u8),
        new("1"u8, defaultRound, 10, (rune)'f', 0, "1"u8),
        new("-1"u8, defaultRound, 10, (rune)'f', 0, "-1"u8),
        new("1.459"u8, defaultRound, 100, (rune)'e', 0, "1e+00"u8),
        new("2.459"u8, defaultRound, 100, (rune)'e', 1, "2.5e+00"u8),
        new("3.459"u8, defaultRound, 100, (rune)'e', 2, "3.46e+00"u8),
        new("4.459"u8, defaultRound, 100, (rune)'e', 3, "4.459e+00"u8),
        new("5.459"u8, defaultRound, 100, (rune)'e', 4, "5.4590e+00"u8),
        new("1.459"u8, defaultRound, 100, (rune)'E', 0, "1E+00"u8),
        new("2.459"u8, defaultRound, 100, (rune)'E', 1, "2.5E+00"u8),
        new("3.459"u8, defaultRound, 100, (rune)'E', 2, "3.46E+00"u8),
        new("4.459"u8, defaultRound, 100, (rune)'E', 3, "4.459E+00"u8),
        new("5.459"u8, defaultRound, 100, (rune)'E', 4, "5.4590E+00"u8),
        new("1.459"u8, defaultRound, 100, (rune)'f', 0, "1"u8),
        new("2.459"u8, defaultRound, 100, (rune)'f', 1, "2.5"u8),
        new("3.459"u8, defaultRound, 100, (rune)'f', 2, "3.46"u8),
        new("4.459"u8, defaultRound, 100, (rune)'f', 3, "4.459"u8),
        new("5.459"u8, defaultRound, 100, (rune)'f', 4, "5.4590"u8),
        new("1.459"u8, defaultRound, 100, (rune)'g', 0, "1"u8),
        new("2.459"u8, defaultRound, 100, (rune)'g', 1, "2"u8),
        new("3.459"u8, defaultRound, 100, (rune)'g', 2, "3.5"u8),
        new("4.459"u8, defaultRound, 100, (rune)'g', 3, "4.46"u8),
        new("5.459"u8, defaultRound, 100, (rune)'g', 4, "5.459"u8),
        new("1459"u8, defaultRound, 53, (rune)'g', 0, "1e+03"u8),
        new("2459"u8, defaultRound, 53, (rune)'g', 1, "2e+03"u8),
        new("3459"u8, defaultRound, 53, (rune)'g', 2, "3.5e+03"u8),
        new("4459"u8, defaultRound, 53, (rune)'g', 3, "4.46e+03"u8),
        new("5459"u8, defaultRound, 53, (rune)'g', 4, "5459"u8),
        new("1459"u8, defaultRound, 53, (rune)'G', 0, "1E+03"u8),
        new("2459"u8, defaultRound, 53, (rune)'G', 1, "2E+03"u8),
        new("3459"u8, defaultRound, 53, (rune)'G', 2, "3.5E+03"u8),
        new("4459"u8, defaultRound, 53, (rune)'G', 3, "4.46E+03"u8),
        new("5459"u8, defaultRound, 53, (rune)'G', 4, "5459"u8),
        new("3"u8, defaultRound, 10, (rune)'e', 40, "3.0000000000000000000000000000000000000000e+00"u8),
        new("3"u8, defaultRound, 10, (rune)'f', 40, "3.0000000000000000000000000000000000000000"u8),
        new("3"u8, defaultRound, 10, (rune)'g', 40, "3"u8),
        new("3e40"u8, defaultRound, 100, (rune)'e', 40, "3.0000000000000000000000000000000000000000e+40"u8),
        new("3e40"u8, defaultRound, 100, (rune)'f', 4, "30000000000000000000000000000000000000000.0000"u8),
        new("3e40"u8, defaultRound, 100, (rune)'g', 40, "3e+40"u8), // make sure "stupid" exponents don't stall the machine

        new("1e1000000"u8, defaultRound, 64, (rune)'p', 0, "0x.88b3a28a05eade3ap+3321929"u8),
        new("1e646456992"u8, defaultRound, 64, (rune)'p', 0, "0x.e883a0c5c8c7c42ap+2147483644"u8),
        new("1e646456993"u8, defaultRound, 64, (rune)'p', 0, "+Inf"u8),
        new("1e1000000000"u8, defaultRound, 64, (rune)'p', 0, "+Inf"u8),
        new("1e-1000000"u8, defaultRound, 64, (rune)'p', 0, "0x.efb4542cc8ca418ap-3321928"u8),
        new("1e-646456993"u8, defaultRound, 64, (rune)'p', 0, "0x.e17c8956983d9d59p-2147483647"u8),
        new("1e-646456994"u8, defaultRound, 64, (rune)'p', 0, "0"u8),
        new("1e-1000000000"u8, defaultRound, 64, (rune)'p', 0, "0"u8), // minimum and maximum values

        new("1p2147483646"u8, defaultRound, 64, (rune)'p', 0, "0x.8p+2147483647"u8),
        new("0x.8p2147483647"u8, defaultRound, 64, (rune)'p', 0, "0x.8p+2147483647"u8),
        new("0x.8p-2147483647"u8, defaultRound, 64, (rune)'p', 0, "0x.8p-2147483647"u8),
        new("1p-2147483649"u8, defaultRound, 64, (rune)'p', 0, "0x.8p-2147483648"u8), // TODO(gri) need tests for actual large Floats

        new("0"u8, defaultRound, 53, (rune)'b', 0, "0"u8),
        new("-0"u8, defaultRound, 53, (rune)'b', 0, "-0"u8),
        new("1.0"u8, defaultRound, 53, (rune)'b', 0, "4503599627370496p-52"u8),
        new("-1.0"u8, defaultRound, 53, (rune)'b', 0, "-4503599627370496p-52"u8),
        new("4503599627370496"u8, defaultRound, 53, (rune)'b', 0, "4503599627370496p+0"u8), // issue 9939

        new("3"u8, defaultRound, 350, (rune)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"u8),
        new("03"u8, defaultRound, 350, (rune)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"u8),
        new("3."u8, defaultRound, 350, (rune)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"u8),
        new("3.0"u8, defaultRound, 350, (rune)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"u8),
        new("3.00"u8, defaultRound, 350, (rune)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"u8),
        new("3.000"u8, defaultRound, 350, (rune)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"u8),
        new("3"u8, defaultRound, 350, (rune)'p', 0, "0x.cp+2"u8),
        new("03"u8, defaultRound, 350, (rune)'p', 0, "0x.cp+2"u8),
        new("3."u8, defaultRound, 350, (rune)'p', 0, "0x.cp+2"u8),
        new("3.0"u8, defaultRound, 350, (rune)'p', 0, "0x.cp+2"u8),
        new("3.00"u8, defaultRound, 350, (rune)'p', 0, "0x.cp+2"u8),
        new("3.000"u8, defaultRound, 350, (rune)'p', 0, "0x.cp+2"u8),
        new("0"u8, defaultRound, 64, (rune)'p', 0, "0"u8),
        new("-0"u8, defaultRound, 64, (rune)'p', 0, "-0"u8),
        new("1024.0"u8, defaultRound, 64, (rune)'p', 0, "0x.8p+11"u8),
        new("-1024.0"u8, defaultRound, 64, (rune)'p', 0, "-0x.8p+11"u8),
        new("0"u8, defaultRound, 64, (rune)'x', -1, "0x0p+00"u8),
        new("0"u8, defaultRound, 64, (rune)'x', 0, "0x0p+00"u8),
        new("0"u8, defaultRound, 64, (rune)'x', 1, "0x0.0p+00"u8),
        new("0"u8, defaultRound, 64, (rune)'x', 5, "0x0.00000p+00"u8),
        new("3.25"u8, defaultRound, 64, (rune)'x', 0, "0x1p+02"u8),
        new("-3.25"u8, defaultRound, 64, (rune)'x', 0, "-0x1p+02"u8),
        new("3.25"u8, defaultRound, 64, (rune)'x', 1, "0x1.ap+01"u8),
        new("-3.25"u8, defaultRound, 64, (rune)'x', 1, "-0x1.ap+01"u8),
        new("3.25"u8, defaultRound, 64, (rune)'x', -1, "0x1.ap+01"u8),
        new("-3.25"u8, defaultRound, 64, (rune)'x', -1, "-0x1.ap+01"u8),
        new("1024.0"u8, defaultRound, 64, (rune)'x', 0, "0x1p+10"u8),
        new("-1024.0"u8, defaultRound, 64, (rune)'x', 0, "-0x1p+10"u8),
        new("1024.0"u8, defaultRound, 64, (rune)'x', 5, "0x1.00000p+10"u8),
        new("8191.0"u8, defaultRound, 53, (rune)'x', -1, "0x1.fffp+12"u8),
        new("8191.5"u8, defaultRound, 53, (rune)'x', -1, "0x1.fff8p+12"u8),
        new("8191.53125"u8, defaultRound, 53, (rune)'x', -1, "0x1.fff88p+12"u8),
        new("8191.53125"u8, defaultRound, 53, (rune)'x', 4, "0x1.fff8p+12"u8),
        new("8191.53125"u8, defaultRound, 53, (rune)'x', 3, "0x1.000p+13"u8),
        new("8191.53125"u8, defaultRound, 53, (rune)'x', 0, "0x1p+13"u8),
        new("8191.533203125"u8, defaultRound, 53, (rune)'x', -1, "0x1.fff888p+12"u8),
        new("8191.533203125"u8, defaultRound, 53, (rune)'x', 5, "0x1.fff88p+12"u8),
        new("8191.533203125"u8, defaultRound, 53, (rune)'x', 4, "0x1.fff9p+12"u8),
        new("8191.53125"u8, defaultRound, 53, (rune)'x', -1, "0x1.fff88p+12"u8),
        new("8191.53125"u8, ToNearestEven, 53, (rune)'x', 5, "0x1.fff88p+12"u8),
        new("8191.53125"u8, ToNearestAway, 53, (rune)'x', 5, "0x1.fff88p+12"u8),
        new("8191.53125"u8, ToZero, 53, (rune)'x', 5, "0x1.fff88p+12"u8),
        new("8191.53125"u8, AwayFromZero, 53, (rune)'x', 5, "0x1.fff88p+12"u8),
        new("8191.53125"u8, ToNegativeInf, 53, (rune)'x', 5, "0x1.fff88p+12"u8),
        new("8191.53125"u8, ToPositiveInf, 53, (rune)'x', 5, "0x1.fff88p+12"u8),
        new("8191.53125"u8, defaultRound, 53, (rune)'x', 4, "0x1.fff8p+12"u8),
        new("8191.53125"u8, defaultRound, 53, (rune)'x', 3, "0x1.000p+13"u8),
        new("8191.53125"u8, defaultRound, 53, (rune)'x', 0, "0x1p+13"u8),
        new("8191.533203125"u8, defaultRound, 53, (rune)'x', -1, "0x1.fff888p+12"u8),
        new("8191.533203125"u8, defaultRound, 53, (rune)'x', 6, "0x1.fff888p+12"u8),
        new("8191.533203125"u8, defaultRound, 53, (rune)'x', 5, "0x1.fff88p+12"u8),
        new("8191.533203125"u8, defaultRound, 53, (rune)'x', 4, "0x1.fff9p+12"u8),
        new("8191.53125"u8, ToNearestEven, 53, (rune)'x', 4, "0x1.fff8p+12"u8),
        new("8191.53125"u8, ToNearestAway, 53, (rune)'x', 4, "0x1.fff9p+12"u8),
        new("8191.53125"u8, ToZero, 53, (rune)'x', 4, "0x1.fff8p+12"u8),
        new("8191.53125"u8, ToZero, 53, (rune)'x', 2, "0x1.ffp+12"u8),
        new("8191.53125"u8, AwayFromZero, 53, (rune)'x', 4, "0x1.fff9p+12"u8),
        new("8191.53125"u8, ToNegativeInf, 53, (rune)'x', 4, "0x1.fff8p+12"u8),
        new("-8191.53125"u8, ToNegativeInf, 53, (rune)'x', 4, "-0x1.fff9p+12"u8),
        new("8191.53125"u8, ToPositiveInf, 53, (rune)'x', 4, "0x1.fff9p+12"u8),
        new("-8191.53125"u8, ToPositiveInf, 53, (rune)'x', 4, "-0x1.fff8p+12"u8), // issue 34343

        new("0x.8p-2147483648"u8, ToNearestEven, 4, (rune)'p', -1, "0x.8p-2147483648"u8),
        new("0x.8p-2147483648"u8, ToNearestEven, 4, (rune)'x', -1, "0x1p-2147483649"u8)
    }.slice()) {
        var (f, _, err) = ParseFloat(test.x, 0, test.prec, ToNearestEven);
        if (err != default!) {
            Ꮡt.Errorf("%v: %s"u8, test, err);
            continue;
        }
        if (test.round != defaultRound) {
            f.SetMode(test.round);
        }
        @string got = f.Text(test.format, test.digits);
        if (got != test.want) {
            Ꮡt.Errorf("%v: got %s; want %s"u8, test, got, test.want);
        }
        // compare with strconv.FormatFloat output if possible
        // ('p' format is not supported by strconv.FormatFloat,
        // and its output for 0.0 prints a biased exponent value
        // as in 0p-1074 which makes no sense to emulate here)
        if (test.prec == 53 && test.format != (rune)'p' && f.Sign() != 0 && (test.round == ToNearestEven || test.round == defaultRound)) {
            var (f64, acc) = f.Float64();
            if (acc != Exact) {
                Ꮡt.Errorf("%v: expected exact conversion to float64"u8, test);
                continue;
            }
            @string gotΔ1 = strconv.FormatFloat(f64, test.format, test.digits, 64);
            if (gotΔ1 != test.want) {
                Ꮡt.Errorf("%v: got %s; want %s"u8, test, gotΔ1, test.want);
            }
        }
    }
}

internal partial struct TestFloatFormat_type /*dyn*/ {
    internal @string format;
    internal any value; // float32, float64, or string (== 512bit *Float)
    internal @string want;
}

public static void TestFloatFormat(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatFormat_type[]{ // from fmt/fmt_test.go

        new("%+.3e"u8, 0.0D, "+0.000e+00"u8),
        new("%+.3e"u8, 1.0D, "+1.000e+00"u8),
        new("%+.3f"u8, -1.0D, "-1.000"u8),
        new("%+.3F"u8, -1.0D, "-1.000"u8),
        new("%+.3F"u8, (float32)(-1.0F), "-1.000"u8),
        new("%+07.2f"u8, 1.0D, "+001.00"u8),
        new("%+07.2f"u8, -1.0D, "-001.00"u8),
        new("%+10.2f"u8, +1.0D, "     +1.00"u8),
        new("%+10.2f"u8, -1.0D, "     -1.00"u8),
        new("% .3E"u8, -1.0D, "-1.000E+00"u8),
        new("% .3e"u8, 1.0D, " 1.000e+00"u8),
        new("%+.3g"u8, 0.0D, "+0"u8),
        new("%+.3g"u8, 1.0D, "+1"u8),
        new("%+.3g"u8, -1.0D, "-1"u8),
        new("% .3g"u8, -1.0D, "-1"u8),
        new("% .3g"u8, 1.0D, " 1"u8),
        new("%b"u8, (float32)1.0F, "8388608p-23"u8),
        new("%b"u8, 1.0D, "4503599627370496p-52"u8), // from fmt/fmt_test.go: old test/fmt_test.go

        new("%e"u8, 1.0D, "1.000000e+00"u8),
        new("%e"u8, 1234.5678e3D, "1.234568e+06"u8),
        new("%e"u8, 1234.5678e-8D, "1.234568e-05"u8),
        new("%e"u8, -7.0D, "-7.000000e+00"u8),
        new("%e"u8, -1e-9D, "-1.000000e-09"u8),
        new("%f"u8, 1234.5678e3D, "1234567.800000"u8),
        new("%f"u8, 1234.5678e-8D, "0.000012"u8),
        new("%f"u8, -7.0D, "-7.000000"u8),
        new("%f"u8, -1e-9D, "-0.000000"u8),
        new("%g"u8, 1234.5678e3D, "1.2345678e+06"u8),
        new("%g"u8, (float32)1234.5678e3F, "1.2345678e+06"u8),
        new("%g"u8, 1234.5678e-8D, "1.2345678e-05"u8),
        new("%g"u8, -7.0D, "-7"u8),
        new("%g"u8, -1e-9D, "-1e-09"u8),
        new("%g"u8, (float32)(-1e-9F), "-1e-09"u8),
        new("%E"u8, 1.0D, "1.000000E+00"u8),
        new("%E"u8, 1234.5678e3D, "1.234568E+06"u8),
        new("%E"u8, 1234.5678e-8D, "1.234568E-05"u8),
        new("%E"u8, -7.0D, "-7.000000E+00"u8),
        new("%E"u8, -1e-9D, "-1.000000E-09"u8),
        new("%G"u8, 1234.5678e3D, "1.2345678E+06"u8),
        new("%G"u8, (float32)1234.5678e3F, "1.2345678E+06"u8),
        new("%G"u8, 1234.5678e-8D, "1.2345678E-05"u8),
        new("%G"u8, -7.0D, "-7"u8),
        new("%G"u8, -1e-9D, "-1E-09"u8),
        new("%G"u8, (float32)(-1e-9F), "-1E-09"u8),
        new("%20.6e"u8, 1.2345e3D, "        1.234500e+03"u8),
        new("%20.6e"u8, 1.2345e-3D, "        1.234500e-03"u8),
        new("%20e"u8, 1.2345e3D, "        1.234500e+03"u8),
        new("%20e"u8, 1.2345e-3D, "        1.234500e-03"u8),
        new("%20.8e"u8, 1.2345e3D, "      1.23450000e+03"u8),
        new("%20f"u8, 1.23456789e3D, "         1234.567890"u8),
        new("%20f"u8, 1.23456789e-3D, "            0.001235"u8),
        new("%20f"u8, 12345678901.23456789D, "  12345678901.234568"u8),
        new("%-20f"u8, 1.23456789e3D, "1234.567890         "u8),
        new("%20.8f"u8, 1.23456789e3D, "       1234.56789000"u8),
        new("%20.8f"u8, 1.23456789e-3D, "          0.00123457"u8),
        new("%g"u8, 1.23456789e3D, "1234.56789"u8),
        new("%g"u8, 1.23456789e-3D, "0.00123456789"u8),
        new("%g"u8, 1.23456789e20D, "1.23456789e+20"u8),
        new("%20e"u8, math.Inf(1), "                +Inf"u8),
        new("%-20f"u8, math.Inf(-1), "-Inf                "u8), // from fmt/fmt_test.go: comparison of padding rules with C printf

        new("%.2f"u8, 1.0D, "1.00"u8),
        new("%.2f"u8, -1.0D, "-1.00"u8),
        new("% .2f"u8, 1.0D, " 1.00"u8),
        new("% .2f"u8, -1.0D, "-1.00"u8),
        new("%+.2f"u8, 1.0D, "+1.00"u8),
        new("%+.2f"u8, -1.0D, "-1.00"u8),
        new("%7.2f"u8, 1.0D, "   1.00"u8),
        new("%7.2f"u8, -1.0D, "  -1.00"u8),
        new("% 7.2f"u8, 1.0D, "   1.00"u8),
        new("% 7.2f"u8, -1.0D, "  -1.00"u8),
        new("%+7.2f"u8, 1.0D, "  +1.00"u8),
        new("%+7.2f"u8, -1.0D, "  -1.00"u8),
        new("%07.2f"u8, 1.0D, "0001.00"u8),
        new("%07.2f"u8, -1.0D, "-001.00"u8),
        new("% 07.2f"u8, 1.0D, " 001.00"u8),
        new("% 07.2f"u8, -1.0D, "-001.00"u8),
        new("%+07.2f"u8, 1.0D, "+001.00"u8),
        new("%+07.2f"u8, -1.0D, "-001.00"u8), // from fmt/fmt_test.go: zero padding does not apply to infinities

        new("%020f"u8, math.Inf(-1), "                -Inf"u8),
        new("%020f"u8, math.Inf(+1), "                +Inf"u8),
        new("% 020f"u8, math.Inf(-1), "                -Inf"u8),
        new("% 020f"u8, math.Inf(+1), "                 Inf"u8),
        new("%+020f"u8, math.Inf(-1), "                -Inf"u8),
        new("%+020f"u8, math.Inf(+1), "                +Inf"u8),
        new("%20f"u8, -1.0D, "           -1.000000"u8), // handle %v like %g

        new("%v"u8, 0.0D, "0"u8),
        new("%v"u8, -7.0D, "-7"u8),
        new("%v"u8, -1e-9D, "-1e-09"u8),
        new("%v"u8, (float32)(-1e-9F), "-1e-09"u8),
        new("%010v"u8, 0.0D, "0000000000"u8), // *Float cases

        new("%.20f"u8, (@string)"1e-20"u8, "0.00000000000000000001"u8),
        new("%.20f"u8, (@string)"-1e-20"u8, "-0.00000000000000000001"u8),
        new("%30.20f"u8, (@string)"-1e-20"u8, "       -0.00000000000000000001"u8),
        new("%030.20f"u8, (@string)"-1e-20"u8, "-00000000.00000000000000000001"u8),
        new("%030.20f"u8, (@string)"+1e-20"u8, "000000000.00000000000000000001"u8),
        new("% 030.20f"u8, (@string)"+1e-20"u8, " 00000000.00000000000000000001"u8), // erroneous formats

        new("%s"u8, 1.0D, "%!s(*big.Float=1)"u8)
    }.slice()) {
        var value = @new<global::go.math.big_package.Float>();
        switch (test.value.type()) {
        case float32 v: {
            value.SetPrec(24).SetFloat64((float64)v);
            break;
        }
        case float64 v: {
            value.SetPrec(53).SetFloat64(v);
            break;
        }
        case @string v: {
            value.SetPrec(512).Parse(v, 0);
            break;
        }
        default: {
            var v = test.value;
            Ꮡt.Fatalf("unsupported test value: %v (%T)"u8, v, v);
            break;
        }}
        {
            @string got = fmt.Sprintf(test.format, value.OrTypedNil()); if (got != test.want) {
                Ꮡt.Errorf("%v: got %q; want %q"u8, test, got, test.want);
            }
        }
    }
}

public static void BenchmarkParseFloatSmallExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, s) in new @string[]{
            "1e0"u8,
            "1e-1"u8,
            "1e-2"u8,
            "1e-3"u8,
            "1e-4"u8,
            "1e-5"u8,
            "1e-10"u8,
            "1e-20"u8,
            "1e-50"u8,
            "1e1"u8,
            "1e2"u8,
            "1e3"u8,
            "1e4"u8,
            "1e5"u8,
            "1e10"u8,
            "1e20"u8,
            "1e50"u8
        }.slice()) {
            ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
            var (_, _, err) = Ꮡx.Parse(s, 0);
            if (err != default!) {
                Ꮡb.Fatalf("%s: %v"u8, s, err);
            }
        }
    }
}

public static void BenchmarkParseFloatLargeExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, s) in new @string[]{
            "1e0"u8,
            "1e-10"u8,
            "1e-20"u8,
            "1e-30"u8,
            "1e-40"u8,
            "1e-50"u8,
            "1e-100"u8,
            "1e-500"u8,
            "1e-1000"u8,
            "1e-5000"u8,
            "1e-10000"u8,
            "1e10"u8,
            "1e20"u8,
            "1e30"u8,
            "1e40"u8,
            "1e50"u8,
            "1e100"u8,
            "1e500"u8,
            "1e1000"u8,
            "1e5000"u8,
            "1e10000"u8
        }.slice()) {
            ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
            var (_, _, err) = Ꮡx.Parse(s, 0);
            if (err != default!) {
                Ꮡb.Fatalf("%s: %v"u8, s, err);
            }
        }
    }
}

internal partial struct TestFloatScan_type /*dyn*/ {
    internal @string input;
    internal @string format;
    internal @string output;
    internal nint remaining;
    internal bool wantErr;
}

public static void TestFloatScan(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// Scan doesn't handle ±Inf.
    slice<TestFloatScan_type> floatScanTests = new slice<TestFloatScan_type>(13){
        [0] = new("10.0"u8, "%f"u8, "10"u8, 0, false),
        [1] = new("23.98+2.0"u8, "%v"u8, "23.98"u8, 4, false),
        [2] = new("-1+1"u8, "%v"u8, "-1"u8, 2, false),
        [3] = new(" 00000"u8, "%v"u8, "0"u8, 0, false),
        [4] = new("-123456p-78"u8, "%b"u8, "-4.084816388e-19"u8, 0, false),
        [5] = new("+123"u8, "%b"u8, "123"u8, 0, false),
        [6] = new("-1.234e+56"u8, "%e"u8, "-1.234e+56"u8, 0, false),
        [7] = new("-1.234E-56"u8, "%E"u8, "-1.234e-56"u8, 0, false),
        [8] = new("-1.234e+567"u8, "%g"u8, "-1.234e+567"u8, 0, false),
        [9] = new("+1234567891011.234"u8, "%G"u8, "1.234567891e+12"u8, 0, false),
        [10] = new("Inf"u8, "%v"u8, ""u8, 3, true),
        [11] = new("-Inf"u8, "%v"u8, ""u8, 3, true),
        [12] = new("-Inf"u8, "%v"u8, ""u8, 3, true)
    };
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (i, test) in floatScanTests) {
        var x = @new<global::go.math.big_package.Float>();
        buf.Reset();
        buf.WriteString(test.input);
        var (_, err) = fmt.Fscanf(new big_test_package.bytes_BufferжReader(Ꮡbuf), test.format, x.OrTypedNil());
        if (test.wantErr) {
            if (err == default!) {
                Ꮡt.Errorf("#%d want non-nil err"u8, i);
            }
            continue;
        }
        if (err != default!) {
            Ꮡt.Errorf("#%d error: %s"u8, i, err);
        }
        if (x.String() != test.output) {
            Ꮡt.Errorf("#%d got %s; want %s"u8, i, x.String(), test.output);
        }
        if (buf.Len() != test.remaining) {
            Ꮡt.Errorf("#%d got %d bytes remaining; want %d"u8, i, buf.Len(), test.remaining);
        }
    }
}

} // end big_internal_test_package
