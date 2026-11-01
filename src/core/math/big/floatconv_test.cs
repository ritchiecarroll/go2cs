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

public static void TestFloatSetFloat64String(ж<testing.T> t) {
    var inf = math.Inf(0);
    var nan = math.NaN();

    foreach (var (_, test) in slice<TestFloatSetFloat64String_type>([
        // basics
        new("0", 0D),
        new("-0", -zero_),
        new("+0", 0D),
        new("1", 1D),
        new("-1", -1D),
        new("+1", 1D),
        new("1.234", 1.234D),
        new("-1.234", -1.234D),
        new("+1.234", 1.234D),
        new(".1", 0.1D),
        new("1.", 1D),
        new("+1.", 1D),

        // various zeros
        new("0e100", 0D),
        new("-0e+100", -zero_),
        new("+0e-100", 0D),
        new("0E100", 0D),
        new("-0E+100", -zero_),
        new("+0E-100", 0D),

        // various decimal exponent formats
        new("1.e10", 1e10D),
        new("1e+10", 1e10D),
        new("+1e-10", 1e-10D),
        new("1E10", 1e10D),
        new("1.E+10", 1e10D),
        new("+1E-10", 1e-10D),

        // infinities
        new("Inf", inf),
        new("+Inf", inf),
        new("-Inf", -inf),
        new("inf", inf),
        new("+inf", inf),
        new("-inf", -inf),

        // invalid numbers
        new("", nan),
        new("-", nan),
        new("0x", nan),
        new("0e", nan),
        new("1.2ef", nan),
        new("2..3", nan),
        new("123..", nan),
        new("infinity", nan),
        new("foobar", nan),

        // invalid underscores
        new("_", nan),
        new("0_", nan),
        new("1__0", nan),
        new("123_.", nan),
        new("123._", nan),
        new("123._4", nan),
        new("1_2.3_4_", nan),
        new("_.123", nan),
        new("_123.456", nan),
        new("10._0", nan),
        new("10.0e_0", nan),
        new("10.0e0_", nan),
        new("0P-0__0", nan),

        // misc decimal values
        new("3.14159265", 3.14159265D),
        new("-687436.79457e-245", -687436.79457e-245D),
        new("-687436.79457E245", -687436.79457e245D),
        new(".0000000000000000000000000000000000000001", 1e-40D),
        new("+10000000000000000000000000000000000000000e-0", 1e40D),

        // decimal mantissa, binary exponent
        new("0p0", 0D),
        new("-0p0", -zero_),
        new("1p10", 1 << 10),
        new("1p+10", 1 << 10),
        new("+1p-10", 1.0D / (1 << 10)),
        new("1024p-12", 0.25D),
        new("-1p10", -1024D),
        new("1.5p1", 3D),

        // binary mantissa, decimal exponent
        new("0b0", 0D),
        new("-0b0", -zero_),
        new("0b0e+10", 0D),
        new("-0b0e-10", -zero_),
        new("0b1010", 10D),
        new("0B1010E2", 1000D),
        new("0b.1", 0.5D),
        new("0b.001", 0.125D),
        new("0b.001e3", 125D),

        // binary mantissa, binary exponent
        new("0b0p+10", 0D),
        new("-0b0p-10", -zero_),
        new("0b.1010p4", 10D),
        new("0b1p-1", 0.5D),
        new("0b001p-3", 0.125D),
        new("0b.001p3", 1D),
        new("0b0.01p2", 1D),
        new("0b0.01P+2", 1D),

        // octal mantissa, decimal exponent
        new("0o0", 0D),
        new("-0o0", -zero_),
        new("0o0e+10", 0D),
        new("-0o0e-10", -zero_),
        new("0o12", 10D),
        new("0O12E2", 1000D),
        new("0o.4", 0.5D),
        new("0o.01", 0.015625D),
        new("0o.01e3", 15.625D),

        // octal mantissa, binary exponent
        new("0o0p+10", 0D),
        new("-0o0p-10", -zero_),
        new("0o.12p6", 10D),
        new("0o4p-3", 0.5D),
        new("0o0014p-6", 0.1875D),
        new("0o.001p9", 1D),
        new("0o0.01p7", 2D),
        new("0O0.01P+2", 0.0625D),

        // hexadecimal mantissa and exponent
        new("0x0", 0D),
        new("-0x0", -zero_),
        new("0x0p+10", 0D),
        new("-0x0p-10", -zero_),
        new("0xff", 255D),
        new("0X.8p1", 1D),
        new("-0X0.00008p16", -0.5D),
        new("-0X0.00008P+16", -0.5D),
        new("0x0.0000000000001p-1022", math.SmallestNonzeroFloat64),
        new("0x1.fffffffffffffp1023", math.MaxFloat64),

        // underscores
        new("0_0", 0D),
        new("1_000.", 1000D),
        new("1_2_3.4_5_6", 123.456D),
        new("1.0e0_0", 1D),
        new("1p+1_0", 1024D),
        new("0b_1000", 8D),
        new("0b_1011_1101", 189D),
        new("0x_f0_0d_1eP+0_8", 4027391488D)
    ])) {
        ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
        Ꮡx.SetPrec(53);
        var (_, ok) = Ꮡx.SetString(test.s);
        if (math.IsNaN(test.x)) {
            // test.s is invalid
            if (ok) {
                t.Errorf("%s: want parse error", test.s);
            }
            continue;
        }
        // test.s is valid
        if (!ok) {
            t.Errorf("%s: got parse error", test.s);
            continue;
        }
        var (f, _) = Ꮡx.Float64();
        var want = @new<global::go.math.big_package.Float>().SetFloat64(test.x);
        if (Ꮡx.Cmp(want) != 0 || x.Signbit() != want.Signbit()) {
            t.Errorf("%s: got %v (%v); want %v", test.s, Ꮡx, f, test.x);
        }
    }
}

internal static float64 fdiv(float64 a, float64 b) {
    return a / b;
}

internal static ref readonly GoBigConst below1e23 => ref below1e23ᵇ;
private static readonly GoBigConst below1e23ᵇ = /* 99999999999999974834176 */
    GoBigConst.Parse("99999999999999974834176");
internal static ref readonly GoBigConst above1e23 => ref above1e23ᵇ;
private static readonly GoBigConst above1e23ᵇ = /* 100000000000000008388608 */
    GoBigConst.Parse("100000000000000008388608");

internal partial struct TestFloat64Text_type /*dyn*/ {
    internal float64 x;
    internal byte format;
    internal nint prec;
    internal @string want;
}

public static void TestFloat64Text(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloat64Text_type>([
        new(0D, (byte)'f', 0, "0"),
        new(math.Copysign(0D, -1D), (byte)'f', 0, "-0"),
        new(1D, (byte)'f', 0, "1"),
        new(-1D, (byte)'f', 0, "-1"),

        new(0.001D, (byte)'e', 0, "1e-03"),
        new(0.459D, (byte)'e', 0, "5e-01"),
        new(1.459D, (byte)'e', 0, "1e+00"),
        new(2.459D, (byte)'e', 1, "2.5e+00"),
        new(3.459D, (byte)'e', 2, "3.46e+00"),
        new(4.459D, (byte)'e', 3, "4.459e+00"),
        new(5.459D, (byte)'e', 4, "5.4590e+00"),

        new(0.001D, (byte)'f', 0, "0"),
        new(0.459D, (byte)'f', 0, "0"),
        new(1.459D, (byte)'f', 0, "1"),
        new(2.459D, (byte)'f', 1, "2.5"),
        new(3.459D, (byte)'f', 2, "3.46"),
        new(4.459D, (byte)'f', 3, "4.459"),
        new(5.459D, (byte)'f', 4, "5.4590"),

        new(0D, (byte)'b', 0, "0"),
        new(math.Copysign(0D, -1D), (byte)'b', 0, "-0"),
        new(1.0D, (byte)'b', 0, "4503599627370496p-52"),
        new(-1.0D, (byte)'b', 0, "-4503599627370496p-52"),
        new(4503599627370496D, (byte)'b', 0, "4503599627370496p+0"),

        new(0D, (byte)'p', 0, "0"),
        new(math.Copysign(0D, -1D), (byte)'p', 0, "-0"),
        new(1024.0D, (byte)'p', 0, "0x.8p+11"),
        new(-1024.0D, (byte)'p', 0, "-0x.8p+11"),

        // all test cases below from strconv/ftoa_test.go
        new(1D, (byte)'e', 5, "1.00000e+00"),
        new(1D, (byte)'f', 5, "1.00000"),
        new(1D, (byte)'g', 5, "1"),
        new(1D, (byte)'g', -1, "1"),
        new(20D, (byte)'g', -1, "20"),
        new(1234567.8D, (byte)'g', -1, "1.2345678e+06"),
        new(200000D, (byte)'g', -1, "200000"),
        new(2000000D, (byte)'g', -1, "2e+06"),

        // g conversion and zero suppression
        new(400D, (byte)'g', 2, "4e+02"),
        new(40D, (byte)'g', 2, "40"),
        new(4D, (byte)'g', 2, "4"),
        new(.4D, (byte)'g', 2, "0.4"),
        new(.04D, (byte)'g', 2, "0.04"),
        new(.004D, (byte)'g', 2, "0.004"),
        new(.0004D, (byte)'g', 2, "0.0004"),
        new(.00004D, (byte)'g', 2, "4e-05"),
        new(.000004D, (byte)'g', 2, "4e-06"),

        new(0D, (byte)'e', 5, "0.00000e+00"),
        new(0D, (byte)'f', 5, "0.00000"),
        new(0D, (byte)'g', 5, "0"),
        new(0D, (byte)'g', -1, "0"),

        new(-1D, (byte)'e', 5, "-1.00000e+00"),
        new(-1D, (byte)'f', 5, "-1.00000"),
        new(-1D, (byte)'g', 5, "-1"),
        new(-1D, (byte)'g', -1, "-1"),

        new(12D, (byte)'e', 5, "1.20000e+01"),
        new(12D, (byte)'f', 5, "12.00000"),
        new(12D, (byte)'g', 5, "12"),
        new(12D, (byte)'g', -1, "12"),

        new(123456700D, (byte)'e', 5, "1.23457e+08"),
        new(123456700D, (byte)'f', 5, "123456700.00000"),
        new(123456700D, (byte)'g', 5, "1.2346e+08"),
        new(123456700D, (byte)'g', -1, "1.234567e+08"),

        new(1.2345e6D, (byte)'e', 5, "1.23450e+06"),
        new(1.2345e6D, (byte)'f', 5, "1234500.00000"),
        new(1.2345e6D, (byte)'g', 5, "1.2345e+06"),

        new(1e23D, (byte)'e', 17, "9.99999999999999916e+22"),
        new(1e23D, (byte)'f', 17, "99999999999999991611392.00000000000000000"),
        new(1e23D, (byte)'g', 17, "9.9999999999999992e+22"),

        new(1e23D, (byte)'e', -1, "1e+23"),
        new(1e23D, (byte)'f', -1, "100000000000000000000000"),
        new(1e23D, (byte)'g', -1, "1e+23"),

        new((float64)below1e23, (byte)'e', 17, "9.99999999999999748e+22"),
        new((float64)below1e23, (byte)'f', 17, "99999999999999974834176.00000000000000000"),
        new((float64)below1e23, (byte)'g', 17, "9.9999999999999975e+22"),

        new((float64)below1e23, (byte)'e', -1, "9.999999999999997e+22"),
        new((float64)below1e23, (byte)'f', -1, "99999999999999970000000"),
        new((float64)below1e23, (byte)'g', -1, "9.999999999999997e+22"),

        new((float64)above1e23, (byte)'e', 17, "1.00000000000000008e+23"),
        new((float64)above1e23, (byte)'f', 17, "100000000000000008388608.00000000000000000"),
        new((float64)above1e23, (byte)'g', 17, "1.0000000000000001e+23"),

        new((float64)above1e23, (byte)'e', -1, "1.0000000000000001e+23"),
        new((float64)above1e23, (byte)'f', -1, "100000000000000010000000"),
        new((float64)above1e23, (byte)'g', -1, "1.0000000000000001e+23"),

        new(5e-304D / 1e20D, (byte)'g', -1, "5e-324"),
        new(-5e-304D / 1e20D, (byte)'g', -1, "-5e-324"),
        new(fdiv(5e-304D, 1e20D), (byte)'g', -1, "5e-324"),   // avoid constant arithmetic
        new(fdiv(-5e-304D, 1e20D), (byte)'g', -1, "-5e-324"), // avoid constant arithmetic

        new(32D, (byte)'g', -1, "32"),
        new(32D, (byte)'g', 0, "3e+01"),

        new(100D, (byte)'x', -1, "0x1.9p+06"),

        // {math.NaN(), 'g', -1, "NaN"},  // Float doesn't support NaNs
        // {-math.NaN(), 'g', -1, "NaN"}, // Float doesn't support NaNs
        new(math.Inf(0), (byte)'g', -1, "+Inf"),
        new(math.Inf(-1), (byte)'g', -1, "-Inf"),
        new(-math.Inf(0), (byte)'g', -1, "-Inf"),

        new(-1D, (byte)'b', -1, "-4503599627370496p-52"),

        // fixed bugs
        new(0.9D, (byte)'f', 1, "0.9"),
        new(0.09D, (byte)'f', 1, "0.1"),
        new(0.0999D, (byte)'f', 1, "0.1"),
        new(0.05D, (byte)'f', 1, "0.1"),
        new(0.05D, (byte)'f', 0, "0"),
        new(0.5D, (byte)'f', 1, "0.5"),
        new(0.5D, (byte)'f', 0, "0"),
        new(1.5D, (byte)'f', 0, "2"),

        // https://www.exploringbinary.com/java-hangs-when-converting-2-2250738585072012e-308/
        new(2.2250738585072012e-308D, (byte)'g', -1, "2.2250738585072014e-308"),
        // https://www.exploringbinary.com/php-hangs-on-numeric-value-2-2250738585072011e-308/
        new(2.2250738585072011e-308D, (byte)'g', -1, "2.225073858507201e-308"),

        // Issue 2625.
        new(383260575764816448D, (byte)'f', 0, "383260575764816448"),
        new(383260575764816448D, (byte)'g', -1, "3.8326057576481645e+17"),

        // Issue 15918.
        new(1D, (byte)'f', -10, "1"),
        new(1D, (byte)'f', -11, "1"),
        new(1D, (byte)'f', -12, "1")
    ])) {
        // The test cases are from the strconv package which tests float64 values.
        // When formatting values with prec = -1 (shortest representation),
        // the actually available mantissa precision matters.
        // For denormalized values, that precision is < 53 (SetFloat64 default).
        // Compute and set the actual precision explicitly.
        var f = @new<global::go.math.big_package.Float>().SetPrec(actualPrec(test.x)).SetFloat64(test.x);
        @string got = f.Text(test.format, test.prec);
        if (got != test.want) {
            t.Errorf("%v: got %s; want %s", test, got, test.want);
            continue;
        }

        if (test.format == 'b' && test.x == 0D) {
            continue; // 'b' format in strconv.Float requires knowledge of bias for 0.0
        }
        if (test.format == 'p') {
            continue; // 'p' format not supported in strconv.Format
        }

        // verify that Float format matches strconv format
        @string want = strconv.FormatFloat(test.x, test.format, test.prec, 64);
        if (got != want) {
            t.Errorf("%v: got %s; want %s (strconv)", test, got, want);
        }
    }
}

// actualPrec returns the number of actually used mantissa bits.
internal static nuint actualPrec(float64 x) {
    {
        var mant = math.Float64bits(x); if (x != 0D && (uint64)(mant & ((uint64)0x7ff << 52)) == 0) {
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

public static void TestFloatText(ж<testing.T> t) {
    global::go.math.big_package.RoundingMode defaultRound = /* ^RoundingMode(0) */ 255;

    foreach (var (_, test) in slice<TestFloatText_type>([
        new("0", defaultRound, 10, (byte)'f', 0, "0"),
        new("-0", defaultRound, 10, (byte)'f', 0, "-0"),
        new("1", defaultRound, 10, (byte)'f', 0, "1"),
        new("-1", defaultRound, 10, (byte)'f', 0, "-1"),

        new("1.459", defaultRound, 100, (byte)'e', 0, "1e+00"),
        new("2.459", defaultRound, 100, (byte)'e', 1, "2.5e+00"),
        new("3.459", defaultRound, 100, (byte)'e', 2, "3.46e+00"),
        new("4.459", defaultRound, 100, (byte)'e', 3, "4.459e+00"),
        new("5.459", defaultRound, 100, (byte)'e', 4, "5.4590e+00"),

        new("1.459", defaultRound, 100, (byte)'E', 0, "1E+00"),
        new("2.459", defaultRound, 100, (byte)'E', 1, "2.5E+00"),
        new("3.459", defaultRound, 100, (byte)'E', 2, "3.46E+00"),
        new("4.459", defaultRound, 100, (byte)'E', 3, "4.459E+00"),
        new("5.459", defaultRound, 100, (byte)'E', 4, "5.4590E+00"),

        new("1.459", defaultRound, 100, (byte)'f', 0, "1"),
        new("2.459", defaultRound, 100, (byte)'f', 1, "2.5"),
        new("3.459", defaultRound, 100, (byte)'f', 2, "3.46"),
        new("4.459", defaultRound, 100, (byte)'f', 3, "4.459"),
        new("5.459", defaultRound, 100, (byte)'f', 4, "5.4590"),

        new("1.459", defaultRound, 100, (byte)'g', 0, "1"),
        new("2.459", defaultRound, 100, (byte)'g', 1, "2"),
        new("3.459", defaultRound, 100, (byte)'g', 2, "3.5"),
        new("4.459", defaultRound, 100, (byte)'g', 3, "4.46"),
        new("5.459", defaultRound, 100, (byte)'g', 4, "5.459"),

        new("1459", defaultRound, 53, (byte)'g', 0, "1e+03"),
        new("2459", defaultRound, 53, (byte)'g', 1, "2e+03"),
        new("3459", defaultRound, 53, (byte)'g', 2, "3.5e+03"),
        new("4459", defaultRound, 53, (byte)'g', 3, "4.46e+03"),
        new("5459", defaultRound, 53, (byte)'g', 4, "5459"),

        new("1459", defaultRound, 53, (byte)'G', 0, "1E+03"),
        new("2459", defaultRound, 53, (byte)'G', 1, "2E+03"),
        new("3459", defaultRound, 53, (byte)'G', 2, "3.5E+03"),
        new("4459", defaultRound, 53, (byte)'G', 3, "4.46E+03"),
        new("5459", defaultRound, 53, (byte)'G', 4, "5459"),

        new("3", defaultRound, 10, (byte)'e', 40, "3.0000000000000000000000000000000000000000e+00"),
        new("3", defaultRound, 10, (byte)'f', 40, "3.0000000000000000000000000000000000000000"),
        new("3", defaultRound, 10, (byte)'g', 40, "3"),

        new("3e40", defaultRound, 100, (byte)'e', 40, "3.0000000000000000000000000000000000000000e+40"),
        new("3e40", defaultRound, 100, (byte)'f', 4, "30000000000000000000000000000000000000000.0000"),
        new("3e40", defaultRound, 100, (byte)'g', 40, "3e+40"),

        // make sure "stupid" exponents don't stall the machine
        new("1e1000000", defaultRound, 64, (byte)'p', 0, "0x.88b3a28a05eade3ap+3321929"),
        new("1e646456992", defaultRound, 64, (byte)'p', 0, "0x.e883a0c5c8c7c42ap+2147483644"),
        new("1e646456993", defaultRound, 64, (byte)'p', 0, "+Inf"),
        new("1e1000000000", defaultRound, 64, (byte)'p', 0, "+Inf"),
        new("1e-1000000", defaultRound, 64, (byte)'p', 0, "0x.efb4542cc8ca418ap-3321928"),
        new("1e-646456993", defaultRound, 64, (byte)'p', 0, "0x.e17c8956983d9d59p-2147483647"),
        new("1e-646456994", defaultRound, 64, (byte)'p', 0, "0"),
        new("1e-1000000000", defaultRound, 64, (byte)'p', 0, "0"),

        // minimum and maximum values
        new("1p2147483646", defaultRound, 64, (byte)'p', 0, "0x.8p+2147483647"),
        new("0x.8p2147483647", defaultRound, 64, (byte)'p', 0, "0x.8p+2147483647"),
        new("0x.8p-2147483647", defaultRound, 64, (byte)'p', 0, "0x.8p-2147483647"),
        new("1p-2147483649", defaultRound, 64, (byte)'p', 0, "0x.8p-2147483648"),

        // TODO(gri) need tests for actual large Floats

        new("0", defaultRound, 53, (byte)'b', 0, "0"),
        new("-0", defaultRound, 53, (byte)'b', 0, "-0"),
        new("1.0", defaultRound, 53, (byte)'b', 0, "4503599627370496p-52"),
        new("-1.0", defaultRound, 53, (byte)'b', 0, "-4503599627370496p-52"),
        new("4503599627370496", defaultRound, 53, (byte)'b', 0, "4503599627370496p+0"),

        // issue 9939
        new("3", defaultRound, 350, (byte)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"),
        new("03", defaultRound, 350, (byte)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"),
        new("3.", defaultRound, 350, (byte)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"),
        new("3.0", defaultRound, 350, (byte)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"),
        new("3.00", defaultRound, 350, (byte)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"),
        new("3.000", defaultRound, 350, (byte)'b', 0, "1720123961992553633708115671476565205597423741876210842803191629540192157066363606052513914832594264915968p-348"),

        new("3", defaultRound, 350, (byte)'p', 0, "0x.cp+2"),
        new("03", defaultRound, 350, (byte)'p', 0, "0x.cp+2"),
        new("3.", defaultRound, 350, (byte)'p', 0, "0x.cp+2"),
        new("3.0", defaultRound, 350, (byte)'p', 0, "0x.cp+2"),
        new("3.00", defaultRound, 350, (byte)'p', 0, "0x.cp+2"),
        new("3.000", defaultRound, 350, (byte)'p', 0, "0x.cp+2"),

        new("0", defaultRound, 64, (byte)'p', 0, "0"),
        new("-0", defaultRound, 64, (byte)'p', 0, "-0"),
        new("1024.0", defaultRound, 64, (byte)'p', 0, "0x.8p+11"),
        new("-1024.0", defaultRound, 64, (byte)'p', 0, "-0x.8p+11"),

        new("0", defaultRound, 64, (byte)'x', -1, "0x0p+00"),
        new("0", defaultRound, 64, (byte)'x', 0, "0x0p+00"),
        new("0", defaultRound, 64, (byte)'x', 1, "0x0.0p+00"),
        new("0", defaultRound, 64, (byte)'x', 5, "0x0.00000p+00"),
        new("3.25", defaultRound, 64, (byte)'x', 0, "0x1p+02"),
        new("-3.25", defaultRound, 64, (byte)'x', 0, "-0x1p+02"),
        new("3.25", defaultRound, 64, (byte)'x', 1, "0x1.ap+01"),
        new("-3.25", defaultRound, 64, (byte)'x', 1, "-0x1.ap+01"),
        new("3.25", defaultRound, 64, (byte)'x', -1, "0x1.ap+01"),
        new("-3.25", defaultRound, 64, (byte)'x', -1, "-0x1.ap+01"),
        new("1024.0", defaultRound, 64, (byte)'x', 0, "0x1p+10"),
        new("-1024.0", defaultRound, 64, (byte)'x', 0, "-0x1p+10"),
        new("1024.0", defaultRound, 64, (byte)'x', 5, "0x1.00000p+10"),
        new("8191.0", defaultRound, 53, (byte)'x', -1, "0x1.fffp+12"),
        new("8191.5", defaultRound, 53, (byte)'x', -1, "0x1.fff8p+12"),
        new("8191.53125", defaultRound, 53, (byte)'x', -1, "0x1.fff88p+12"),
        new("8191.53125", defaultRound, 53, (byte)'x', 4, "0x1.fff8p+12"),
        new("8191.53125", defaultRound, 53, (byte)'x', 3, "0x1.000p+13"),
        new("8191.53125", defaultRound, 53, (byte)'x', 0, "0x1p+13"),
        new("8191.533203125", defaultRound, 53, (byte)'x', -1, "0x1.fff888p+12"),
        new("8191.533203125", defaultRound, 53, (byte)'x', 5, "0x1.fff88p+12"),
        new("8191.533203125", defaultRound, 53, (byte)'x', 4, "0x1.fff9p+12"),

        new("8191.53125", defaultRound, 53, (byte)'x', -1, "0x1.fff88p+12"),
        new("8191.53125", ToNearestEven, 53, (byte)'x', 5, "0x1.fff88p+12"),
        new("8191.53125", ToNearestAway, 53, (byte)'x', 5, "0x1.fff88p+12"),
        new("8191.53125", ToZero, 53, (byte)'x', 5, "0x1.fff88p+12"),
        new("8191.53125", AwayFromZero, 53, (byte)'x', 5, "0x1.fff88p+12"),
        new("8191.53125", ToNegativeInf, 53, (byte)'x', 5, "0x1.fff88p+12"),
        new("8191.53125", ToPositiveInf, 53, (byte)'x', 5, "0x1.fff88p+12"),

        new("8191.53125", defaultRound, 53, (byte)'x', 4, "0x1.fff8p+12"),
        new("8191.53125", defaultRound, 53, (byte)'x', 3, "0x1.000p+13"),
        new("8191.53125", defaultRound, 53, (byte)'x', 0, "0x1p+13"),
        new("8191.533203125", defaultRound, 53, (byte)'x', -1, "0x1.fff888p+12"),
        new("8191.533203125", defaultRound, 53, (byte)'x', 6, "0x1.fff888p+12"),
        new("8191.533203125", defaultRound, 53, (byte)'x', 5, "0x1.fff88p+12"),
        new("8191.533203125", defaultRound, 53, (byte)'x', 4, "0x1.fff9p+12"),

        new("8191.53125", ToNearestEven, 53, (byte)'x', 4, "0x1.fff8p+12"),
        new("8191.53125", ToNearestAway, 53, (byte)'x', 4, "0x1.fff9p+12"),
        new("8191.53125", ToZero, 53, (byte)'x', 4, "0x1.fff8p+12"),
        new("8191.53125", ToZero, 53, (byte)'x', 2, "0x1.ffp+12"),
        new("8191.53125", AwayFromZero, 53, (byte)'x', 4, "0x1.fff9p+12"),
        new("8191.53125", ToNegativeInf, 53, (byte)'x', 4, "0x1.fff8p+12"),
        new("-8191.53125", ToNegativeInf, 53, (byte)'x', 4, "-0x1.fff9p+12"),
        new("8191.53125", ToPositiveInf, 53, (byte)'x', 4, "0x1.fff9p+12"),
        new("-8191.53125", ToPositiveInf, 53, (byte)'x', 4, "-0x1.fff8p+12"),

        // issue 34343
        new("0x.8p-2147483648", ToNearestEven, 4, (byte)'p', -1, "0x.8p-2147483648"),
        new("0x.8p-2147483648", ToNearestEven, 4, (byte)'x', -1, "0x1p-2147483649")
    ])) {
        var (f, _, err) = ParseFloat(test.x, 0, test.prec, ToNearestEven);
        if (err != nil) {
            t.Errorf("%v: %s", test, err);
            continue;
        }
        if (test.round != defaultRound) {
            f.SetMode(test.round);
        }

        @string got = f.Text(test.format, test.digits);
        if (got != test.want) {
            t.Errorf("%v: got %s; want %s", test, got, test.want);
        }

        // compare with strconv.FormatFloat output if possible
        // ('p' format is not supported by strconv.FormatFloat,
        // and its output for 0.0 prints a biased exponent value
        // as in 0p-1074 which makes no sense to emulate here)
        if (test.prec == 53 && test.format != 'p' && f.Sign() != 0 && (test.round == ToNearestEven || test.round == defaultRound)) {
            var (f64, acc) = f.Float64();
            if (acc != Exact) {
                t.Errorf("%v: expected exact conversion to float64", test);
                continue;
            }
            @string gotΔ1 = strconv.FormatFloat(f64, test.format, test.digits, 64);
            if (gotΔ1 != test.want) {
                t.Errorf("%v: got %s; want %s", test, gotΔ1, test.want);
            }
        }
    }
}

internal partial struct TestFloatFormat_type /*dyn*/ {
    internal @string format;
    internal any value; // float32, float64, or string (== 512bit *Float)
    internal @string want;
}

public static void TestFloatFormat(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatFormat_type>([
        // from fmt/fmt_test.go
        new("%+.3e", 0.0D, "+0.000e+00"),
        new("%+.3e", 1.0D, "+1.000e+00"),
        new("%+.3f", -1.0D, "-1.000"),
        new("%+.3F", -1.0D, "-1.000"),
        new("%+.3F", (float32)(-1.0F), "-1.000"),
        new("%+07.2f", 1.0D, "+001.00"),
        new("%+07.2f", -1.0D, "-001.00"),
        new("%+10.2f", +1.0D, "     +1.00"),
        new("%+10.2f", -1.0D, "     -1.00"),
        new("% .3E", -1.0D, "-1.000E+00"),
        new("% .3e", 1.0D, " 1.000e+00"),
        new("%+.3g", 0.0D, "+0"),
        new("%+.3g", 1.0D, "+1"),
        new("%+.3g", -1.0D, "-1"),
        new("% .3g", -1.0D, "-1"),
        new("% .3g", 1.0D, " 1"),
        new("%b", (float32)1.0F, "8388608p-23"),
        new("%b", 1.0D, "4503599627370496p-52"),

        // from fmt/fmt_test.go: old test/fmt_test.go
        new("%e", 1.0D, "1.000000e+00"),
        new("%e", 1234.5678e3D, "1.234568e+06"),
        new("%e", 1234.5678e-8D, "1.234568e-05"),
        new("%e", -7.0D, "-7.000000e+00"),
        new("%e", -1e-9D, "-1.000000e-09"),
        new("%f", 1234.5678e3D, "1234567.800000"),
        new("%f", 1234.5678e-8D, "0.000012"),
        new("%f", -7.0D, "-7.000000"),
        new("%f", -1e-9D, "-0.000000"),
        new("%g", 1234.5678e3D, "1.2345678e+06"),
        new("%g", (float32)1234.5678e3F, "1.2345678e+06"),
        new("%g", 1234.5678e-8D, "1.2345678e-05"),
        new("%g", -7.0D, "-7"),
        new("%g", -1e-9D, "-1e-09"),
        new("%g", (float32)(-1e-9F), "-1e-09"),
        new("%E", 1.0D, "1.000000E+00"),
        new("%E", 1234.5678e3D, "1.234568E+06"),
        new("%E", 1234.5678e-8D, "1.234568E-05"),
        new("%E", -7.0D, "-7.000000E+00"),
        new("%E", -1e-9D, "-1.000000E-09"),
        new("%G", 1234.5678e3D, "1.2345678E+06"),
        new("%G", (float32)1234.5678e3F, "1.2345678E+06"),
        new("%G", 1234.5678e-8D, "1.2345678E-05"),
        new("%G", -7.0D, "-7"),
        new("%G", -1e-9D, "-1E-09"),
        new("%G", (float32)(-1e-9F), "-1E-09"),

        new("%20.6e", 1.2345e3D, "        1.234500e+03"),
        new("%20.6e", 1.2345e-3D, "        1.234500e-03"),
        new("%20e", 1.2345e3D, "        1.234500e+03"),
        new("%20e", 1.2345e-3D, "        1.234500e-03"),
        new("%20.8e", 1.2345e3D, "      1.23450000e+03"),
        new("%20f", 1.23456789e3D, "         1234.567890"),
        new("%20f", 1.23456789e-3D, "            0.001235"),
        new("%20f", 12345678901.23456789D, "  12345678901.234568"),
        new("%-20f", 1.23456789e3D, "1234.567890         "),
        new("%20.8f", 1.23456789e3D, "       1234.56789000"),
        new("%20.8f", 1.23456789e-3D, "          0.00123457"),
        new("%g", 1.23456789e3D, "1234.56789"),
        new("%g", 1.23456789e-3D, "0.00123456789"),
        new("%g", 1.23456789e20D, "1.23456789e+20"),
        new("%20e", math.Inf(1), "                +Inf"),
        new("%-20f", math.Inf(-1), "-Inf                "),

        // from fmt/fmt_test.go: comparison of padding rules with C printf
        new("%.2f", 1.0D, "1.00"),
        new("%.2f", -1.0D, "-1.00"),
        new("% .2f", 1.0D, " 1.00"),
        new("% .2f", -1.0D, "-1.00"),
        new("%+.2f", 1.0D, "+1.00"),
        new("%+.2f", -1.0D, "-1.00"),
        new("%7.2f", 1.0D, "   1.00"),
        new("%7.2f", -1.0D, "  -1.00"),
        new("% 7.2f", 1.0D, "   1.00"),
        new("% 7.2f", -1.0D, "  -1.00"),
        new("%+7.2f", 1.0D, "  +1.00"),
        new("%+7.2f", -1.0D, "  -1.00"),
        new("%07.2f", 1.0D, "0001.00"),
        new("%07.2f", -1.0D, "-001.00"),
        new("% 07.2f", 1.0D, " 001.00"),
        new("% 07.2f", -1.0D, "-001.00"),
        new("%+07.2f", 1.0D, "+001.00"),
        new("%+07.2f", -1.0D, "-001.00"),

        // from fmt/fmt_test.go: zero padding does not apply to infinities
        new("%020f", math.Inf(-1), "                -Inf"),
        new("%020f", math.Inf(+1), "                +Inf"),
        new("% 020f", math.Inf(-1), "                -Inf"),
        new("% 020f", math.Inf(+1), "                 Inf"),
        new("%+020f", math.Inf(-1), "                -Inf"),
        new("%+020f", math.Inf(+1), "                +Inf"),
        new("%20f", -1.0D, "           -1.000000"),

        // handle %v like %g
        new("%v", 0.0D, "0"),
        new("%v", -7.0D, "-7"),
        new("%v", -1e-9D, "-1e-09"),
        new("%v", (float32)(-1e-9F), "-1e-09"),
        new("%010v", 0.0D, "0000000000"),

        // *Float cases
        new("%.20f", (@string)"1e-20", "0.00000000000000000001"),
        new("%.20f", (@string)"-1e-20", "-0.00000000000000000001"),
        new("%30.20f", (@string)"-1e-20", "       -0.00000000000000000001"),
        new("%030.20f", (@string)"-1e-20", "-00000000.00000000000000000001"),
        new("%030.20f", (@string)"+1e-20", "000000000.00000000000000000001"),
        new("% 030.20f", (@string)"+1e-20", " 00000000.00000000000000000001"),

        // erroneous formats
        new("%s", 1.0D, "%!s(*big.Float=1)")
    ])) {
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
            t.Fatalf("unsupported test value: %v (%T)", v, v);
            break;
        }}

        {
            @string got = fmt.Sprintf(test.format, value.OrTypedNil()); if (got != test.want) {
                t.Errorf("%v: got %q; want %q", test, got, test.want);
            }
        }
    }
}

public static void BenchmarkParseFloatSmallExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, s) in slice<@string>([
            "1e0",
            "1e-1",
            "1e-2",
            "1e-3",
            "1e-4",
            "1e-5",
            "1e-10",
            "1e-20",
            "1e-50",
            "1e1",
            "1e2",
            "1e3",
            "1e4",
            "1e5",
            "1e10",
            "1e20",
            "1e50"
        ])) {
            ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
            var (_, _, err) = Ꮡx.Parse(s, 0);
            if (err != nil) {
                Ꮡb.Fatalf("%s: %v", s, err);
            }
        }
    }
}

public static void BenchmarkParseFloatLargeExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, s) in slice<@string>([
            "1e0",
            "1e-10",
            "1e-20",
            "1e-30",
            "1e-40",
            "1e-50",
            "1e-100",
            "1e-500",
            "1e-1000",
            "1e-5000",
            "1e-10000",
            "1e10",
            "1e20",
            "1e30",
            "1e40",
            "1e50",
            "1e100",
            "1e500",
            "1e1000",
            "1e5000",
            "1e10000"
        ])) {
            ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
            var (_, _, err) = Ꮡx.Parse(s, 0);
            if (err != nil) {
                Ꮡb.Fatalf("%s: %v", s, err);
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

public static void TestFloatScan(ж<testing.T> t) {
    slice<TestFloatScan_type> floatScanTests = new slice<TestFloatScan_type>(13){
        [0] = new("10.0", "%f", "10", 0, false),
        [1] = new("23.98+2.0", "%v", "23.98", 4, false),
        [2] = new("-1+1", "%v", "-1", 2, false),
        [3] = new(" 00000", "%v", "0", 0, false),
        [4] = new("-123456p-78", "%b", "-4.084816388e-19", 0, false),
        [5] = new("+123", "%b", "123", 0, false),
        [6] = new("-1.234e+56", "%e", "-1.234e+56", 0, false),
        [7] = new("-1.234E-56", "%E", "-1.234e-56", 0, false),
        [8] = new("-1.234e+567", "%g", "-1.234e+567", 0, false),
        [9] = new("+1234567891011.234", "%G", "1.234567891e+12", 0, false),

        // Scan doesn't handle ±Inf.
        [10] = new("Inf", "%v", "", 3, true),
        [11] = new("-Inf", "%v", "", 3, true),
        [12] = new("-Inf", "%v", "", 3, true)
    };

    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (i, test) in floatScanTests) {
        var x = @new<global::go.math.big_package.Float>();
        buf.Reset();
        buf.WriteString(test.input);
        var (_, err) = fmt.Fscanf(new big_test_package.bytes_BufferжReader(Ꮡbuf), test.format, x.OrTypedNil());
        if (test.wantErr) {
            if (err == nil) {
                t.Errorf("#%d want non-nil err", i);
            }
            continue;
        }

        if (err != nil) {
            t.Errorf("#%d error: %s", i, err);
        }

        if (x.String() != test.output) {
            t.Errorf("#%d got %s; want %s", i, x.String(), test.output);
        }
        if (buf.Len() != test.remaining) {
            t.Errorf("#%d got %d bytes remaining; want %d", i, buf.Len(), test.remaining);
        }
    }
}

} // end big_internal_test_package
