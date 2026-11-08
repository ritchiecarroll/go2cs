// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using math = math_package;
using reflect = reflect_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using static go.math.big_package;

partial class big_internal_test_package {

partial struct exponentTestsᴛ1 /*dyn*/ {
    internal @string s;    // string to be scanned
    internal bool base2ok; // true if 'p'/'P' exponents are accepted
    internal bool sepOk;   // true if '_' separators are accepted
    internal int64 x;      // expected exponent
    internal nint b;       // expected exponent base
    internal error err;    // expected error
    internal rune next;    // next character (or 0, if at EOF)
}
internal static slice<exponentTestsᴛ1> exponentTests;
internal static void initᴛexponentTests() { exponentTests = [
    // valid, without separators
    new("", false, false, 0, 10, default, 0),
    new("1", false, false, 0, 10, default, '1'),
    new("e0", false, false, 0, 10, default, 0),
    new("E1", false, false, 1, 10, default, 0),
    new("e+10", false, false, 10, 10, default, 0),
    new("e-10", false, false, -10, 10, default, 0),
    new("e123456789a", false, false, 123456789, 10, default, 'a'),
    new("p", false, false, 0, 10, default, 'p'),
    new("P+100", false, false, 0, 10, default, 'P'),
    new("p0", true, false, 0, 2, default, 0),
    new("P-123", true, false, -123, 2, default, 0),
    new("p+0a", true, false, 0, 2, default, 'a'),
    new("p+123__", true, false, 123, 2, default, '_'), // '_' is not part of the number anymore

    // valid, with separators
    new("e+1_0", false, true, 10, 10, default, 0),
    new("e-1_0", false, true, -10, 10, default, 0),
    new("e123_456_789a", false, true, 123456789, 10, default, 'a'),
    new("P+1_00", false, true, 0, 10, default, 'P'),
    new("p-1_2_3", true, true, -123, 2, default, 0),

    // invalid: no digits
    new("e", false, false, 0, 10, errNoDigits, 0),
    new("ef", false, false, 0, 10, errNoDigits, 'f'),
    new("e+", false, false, 0, 10, errNoDigits, 0),
    new("E-x", false, false, 0, 10, errNoDigits, 'x'),
    new("p", true, false, 0, 2, errNoDigits, 0),
    new("P-", true, false, 0, 2, errNoDigits, 0),
    new("p+e", true, false, 0, 2, errNoDigits, 'e'),
    new("e+_x", false, true, 0, 10, errNoDigits, 'x'),

    // invalid: incorrect use of separator
    new("e0_", false, true, 0, 10, errInvalSep, 0),
    new("e_0", false, true, 0, 10, errInvalSep, 0),
    new("e-1_2__3", false, true, -123, 10, errInvalSep, 0)
]; }

public static void TestScanExponent(ж<testing.T> t) {
    foreach (var (_, a) in range(exponentTests)) {
        var r = strings.NewReader(a.s);
        var (x, b, err) = scanExponent(new big_test_package.strings_ReaderжByteScanner(r), a.base2ok, a.sepOk);
        if (!AreEqual(err, a.err)) {
            t.Errorf("scanExponent%+v\n\tgot error = %v; want %v", a, err, a.err);
        }
        if (x != a.x) {
            t.Errorf("scanExponent%+v\n\tgot z = %v; want %v", a, x, a.x);
        }
        if (b != a.b) {
            t.Errorf("scanExponent%+v\n\tgot b = %d; want %d", a, b, a.b);
        }
        (var next, _, err) = r.ReadRune();
        if (AreEqual(err, io.EOF)) {
            next = 0;
            err = default;
        }
        if (err == nil && next != a.next) {
            t.Errorf("scanExponent%+v\n\tgot next = %q; want %q", a, next, a.next);
        }
    }
}

public partial struct StringTest {
    internal @string @in, @out;
    internal bool ok;
}

internal static slice<StringTest> setStringTests = [
    // invalid
    new(@in: "1e"),
    new(@in: "1.e"),
    new(@in: "1e+14e-5"),
    new(@in: "1e4.5"),
    new(@in: "r"),
    new(@in: "a/b"),
    new(@in: "a.b"),
    new(@in: "1/0"),
    new(@in: "4/3/2"), // issue 17001
    new(@in: "4/3/"),
    new(@in: "4/3."),
    new(@in: "4/"),
    new(@in: "13e-9223372036854775808"), // CVE-2022-23772

    // valid
    new("0", "0", true),
    new("-0", "0", true),
    new("1", "1", true),
    new("-1", "-1", true),
    new("1.", "1", true),
    new("1e0", "1", true),
    new("1.e1", "10", true),
    new("-0.1", "-1/10", true),
    new("-.1", "-1/10", true),
    new("2/4", "1/2", true),
    new(".25", "1/4", true),
    new("-1/5", "-1/5", true),
    new("8129567.7690E14", "812956776900000000000", true),
    new("78189e+4", "781890000", true),
    new("553019.8935e+8", "55301989350000", true),
    new("98765432109876543210987654321e-10", "98765432109876543210987654321/10000000000", true),
    new("9877861857500000E-7", "3951144743/4", true),
    new("2169378.417e-3", "2169378417/1000000", true),
    new("884243222337379604041632732738665534", "884243222337379604041632732738665534", true),
    new("53/70893980658822810696", "53/70893980658822810696", true),
    new("106/141787961317645621392", "53/70893980658822810696", true),
    new("204211327800791583.81095", "4084226556015831676219/20000", true),
    new("0e9999999999", "0", true) // issue #16176
];

// These are not supported by fmt.Fscanf.
internal static slice<StringTest> setStringTests2 = [
    // invalid
    new(@in: "4/3x"),
    new(@in: "0/-1"),
    new(@in: "-1/-1"),

    // invalid with separators
    // (smoke tests only - a comprehensive set of tests is in natconv_test.go)
    new(@in: "10_/1"),
    new(@in: "_10/1"),
    new(@in: "1/1__0"),

    // valid
    new("0b1000/3", "8/3", true),
    new("0B1000/0x8", "1", true),
    new("-010/1", "-8", true), // 0-prefix indicates octal in this case
    new("-010.0", "-10", true),
    new("-0o10/1", "-8", true),
    new("0x10/1", "16", true),
    new("0x10/0x20", "1/2", true),

    new("0010", "10", true), // 0-prefix is ignored in this case (not a fraction)
    new("0x10.0", "16", true),
    new("0x1.8", "3/2", true),
    new("0X1.8p4", "24", true),
    new("0x1.1E2", "2289/2048", true), // E is part of hex mantissa, not exponent
    new("0b1.1E2", "150", true),
    new("0B1.1P3", "12", true),
    new("0o10e-2", "2/25", true),
    new("0O10p-3", "1", true),

    // valid with separators
    // (smoke tests only - a comprehensive set of tests is in natconv_test.go)
    new("0b_1000/3", "8/3", true),
    new("0B_10_00/0x8", "1", true),
    new("0xdead/0B1101_1110_1010_1101", "1", true),
    new("0B1101_1110_1010_1101/0XD_E_A_D", "1", true),
    new("1_000.0", "1000", true),

    new("0x_10.0", "16", true),
    new("0x1_0.0", "16", true),
    new("0x1.8_0", "3/2", true),
    new("0X1.8p0_4", "24", true),
    new("0b1.1_0E2", "150", true),
    new("0o1_0e-2", "2/25", true),
    new("0O_10p-3", "1", true)
];

public static void TestRatSetString(ж<testing.T> t) {
    slice<StringTest> tests = default;
    tests = appendꓸꓸꓸ(tests, setStringTests);
    tests = appendꓸꓸꓸ(tests, setStringTests2);

    foreach (var (i, test) in range(tests)) {
        var (x, ok) = @new<global::go.math.big_package.ΔRat>().SetString(test.@in);

        if (ok) {
            if (!test.ok) {
                t.Errorf("#%d SetString(%q) expected failure", i, test.@in);
            } else if (x.RatString() != test.@out) {
                t.Errorf("#%d SetString(%q) got %s want %s", i, test.@in, x.RatString(), test.@out);
            }
        } else {
            if (test.ok) {
                t.Errorf("#%d SetString(%q) expected success", i, test.@in);
            } else if (x != nil) {
                t.Errorf("#%d SetString(%q) got %p want nil", i, test.@in, x.OrTypedNil());
            }
        }
    }
}

public static void TestRatSetStringZero(ж<testing.T> t) {
    var (got, _) = @new<global::go.math.big_package.ΔRat>().SetString("0");
    var want = @new<global::go.math.big_package.ΔRat>().SetInt64(0);
    if (!reflect.DeepEqual(got.OrTypedNil(), want.OrTypedNil())) {
        t.Errorf("got %#+v, want %#+v", got.OrTypedNil(), want.OrTypedNil());
    }
}

public static void TestRatScan(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (i, test) in range(setStringTests)) {
        var x = @new<global::go.math.big_package.ΔRat>();
        buf.Reset();
        buf.WriteString(test.@in);

        var (_, err) = fmt.Fscanf(new big_test_package.bytes_BufferжReader(Ꮡbuf), "%v"u8, x.OrTypedNil());
        if (err == nil != test.ok) {
            if (test.ok) {
                t.Errorf("#%d (%s) error: %s", i, test.@in, err);
            } else {
                t.Errorf("#%d (%s) expected error", i, test.@in);
            }
            continue;
        }
        if (err == nil && x.RatString() != test.@out) {
            t.Errorf("#%d got %s want %s", i, x.RatString(), test.@out);
        }
    }
}

partial struct floatStringTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal nint prec;
    internal @string @out;
}
internal static slice<floatStringTestsᴛ1> floatStringTests = [
    new("0", 0, "0"),
    new("0", 4, "0.0000"),
    new("1", 0, "1"),
    new("1", 2, "1.00"),
    new("-1", 0, "-1"),
    new("0.05", 1, "0.1"),
    new("-0.05", 1, "-0.1"),
    new(".25", 2, "0.25"),
    new(".25", 1, "0.3"),
    new(".25", 3, "0.250"),
    new("-1/3", 3, "-0.333"),
    new("-2/3", 4, "-0.6667"),
    new("0.96", 1, "1.0"),
    new("0.999", 2, "1.00"),
    new("0.9", 0, "1"),
    new(".25", -1, "0"),
    new(".55", -1, "1")
];

public static void TestFloatString(ж<testing.T> t) {
    foreach (var (i, test) in range(floatStringTests)) {
        var (x, _) = @new<global::go.math.big_package.ΔRat>().SetString(test.@in);

        if (x.FloatString(test.prec) != test.@out) {
            t.Errorf("#%d got %s want %s", i, x.FloatString(test.prec), test.@out);
        }
    }
}

// Test inputs to Rat.SetString. The prefix "long:" causes the test
// to be skipped except in -long mode.  (The threshold is about 500us.)
internal static slice<@string> float64inputs = slice<@string>([
    // Constants plundered from strconv/testfp.txt.

    // Table 1: Stress Inputs for Conversion to 53-bit Binary, < 1/2 ULP
    "5e+125",
    "69e+267",
    "999e-026",
    "7861e-034",
    "75569e-254",
    "928609e-261",
    "9210917e+080",
    "84863171e+114",
    "653777767e+273",
    "5232604057e-298",
    "27235667517e-109",
    "653532977297e-123",
    "3142213164987e-294",
    "46202199371337e-072",
    "231010996856685e-073",
    "9324754620109615e+212",
    "78459735791271921e+049",
    "272104041512242479e+200",
    "6802601037806061975e+198",
    "20505426358836677347e-221",
    "836168422905420598437e-234",
    "4891559871276714924261e+222",

    // Table 2: Stress Inputs for Conversion to 53-bit Binary, > 1/2 ULP
    "9e-265",
    "85e-037",
    "623e+100",
    "3571e+263",
    "81661e+153",
    "920657e-023",
    "4603285e-024",
    "87575437e-309",
    "245540327e+122",
    "6138508175e+120",
    "83356057653e+193",
    "619534293513e+124",
    "2335141086879e+218",
    "36167929443327e-159",
    "609610927149051e-255",
    "3743626360493413e-165",
    "94080055902682397e-242",
    "899810892172646163e+283",
    "7120190517612959703e+120",
    "25188282901709339043e-252",
    "308984926168550152811e-052",
    "6372891218502368041059e+064",

    // Table 14: Stress Inputs for Conversion to 24-bit Binary, <1/2 ULP
    "5e-20",
    "67e+14",
    "985e+15",
    "7693e-42",
    "55895e-16",
    "996622e-44",
    "7038531e-32",
    "60419369e-46",
    "702990899e-20",
    "6930161142e-48",
    "25933168707e+13",
    "596428896559e+20",

    // Table 15: Stress Inputs for Conversion to 24-bit Binary, >1/2 ULP
    "3e-23",
    "57e+18",
    "789e-35",
    "2539e-18",
    "76173e+28",
    "887745e-11",
    "5382571e-37",
    "82381273e-35",
    "750486563e-38",
    "3752432815e-39",
    "75224575729e-45",
    "459926601011e+15",

    // Constants plundered from strconv/atof_test.go.

    "0",
    "1",
    "+1",
    "1e23",
    "1E23",
    "100000000000000000000000",
    "1e-100",
    "123456700",
    "99999999999999974834176",
    "100000000000000000000001",
    "100000000000000008388608",
    "100000000000000016777215",
    "100000000000000016777216",
    "-1",
    "-0.1",
    "-0", // NB: exception made for this input
    "1e-20",
    "625e-3",

    // largest float64
    "1.7976931348623157e308",
    "-1.7976931348623157e308",
    // next float64 - too large
    "1.7976931348623159e308",
    "-1.7976931348623159e308",
    // the border is ...158079
    // borderline - okay
    "1.7976931348623158e308",
    "-1.7976931348623158e308",
    // borderline - too large
    "1.797693134862315808e308",
    "-1.797693134862315808e308",

    // a little too large
    "1e308",
    "2e308",
    "1e309",

    // way too large
    "1e310",
    "-1e310",
    "1e400",
    "-1e400",
    "long:1e400000",
    "long:-1e400000",

    // denormalized
    "1e-305",
    "1e-306",
    "1e-307",
    "1e-308",
    "1e-309",
    "1e-310",
    "1e-322",
    // smallest denormal
    "5e-324",
    "4e-324",
    "3e-324",
    // too small
    "2e-324",
    // way too small
    "1e-350",
    "long:1e-400000",
    // way too small, negative
    "-1e-350",
    "long:-1e-400000",

    // try to overflow exponent
    // [Disabled: too slow and memory-hungry with rationals.]
    // "1e-4294967296",
    // "1e+4294967296",
    // "1e-18446744073709551616",
    // "1e+18446744073709551616",

    // https://www.exploringbinary.com/java-hangs-when-converting-2-2250738585072012e-308/
    "2.2250738585072012e-308",
    // https://www.exploringbinary.com/php-hangs-on-numeric-value-2-2250738585072011e-308/
    "2.2250738585072011e-308",

    // A very large number (initially wrongly parsed by the fast algorithm).
    "4.630813248087435e+307",

    // A different kind of very large number.
    "22.222222222222222",
    "long:2."u8 + strings.Repeat("2", 4000) + "e+1"u8,

    // Exactly halfway between 1 and math.Nextafter(1, 2).
    // Round to even (down).
    "1.00000000000000011102230246251565404236316680908203125",
    // Slightly lower; still round down.
    "1.00000000000000011102230246251565404236316680908203124",
    // Slightly higher; round up.
    "1.00000000000000011102230246251565404236316680908203126",
    // Slightly higher, but you have to read all the way to the end.
    "long:1.00000000000000011102230246251565404236316680908203125"u8 + strings.Repeat("0", 10000) + "1"u8,

    // Smallest denormal, 2^(-1022-52)
    "4.940656458412465441765687928682213723651e-324",
    // Half of smallest denormal, 2^(-1022-53)
    "2.470328229206232720882843964341106861825e-324",
    // A little more than the exact half of smallest denormal
    // 2^-1075 + 2^-1100.  (Rounds to 1p-1074.)
    "2.470328302827751011111470718709768633275e-324",
    // The exact halfway between smallest normal and largest denormal:
    // 2^-1022 - 2^-1075.  (Rounds to 2^-1022.)
    "2.225073858507201136057409796709131975935e-308",

    "1152921504606846975",  //   1<<60 - 1
    "-1152921504606846975", // -(1<<60 - 1)
    "1152921504606846977",  //   1<<60 + 1
    "-1152921504606846977", // -(1<<60 + 1)

    "1/3"
]);

// isFinite reports whether f represents a finite rational value.
// It is equivalent to !math.IsNan(f) && !math.IsInf(f, 0).
internal static bool isFinite(float64 f) {
    return math.Abs(f) <= math.MaxFloat64;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string longˢ => ref longˢᵇ;
private static readonly @string longˢᵇ = "long:"u8;

public static void TestFloat32SpecialCases(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(float64inputs)) {
        var input = vᴛ1;

        if (strings.HasPrefix(input, longˢ)) {
            if (!@long.Value) {
                continue;
            }
            input = input[(int)(len("long:"))..];
        }

        var (r, ok) = @new<global::go.math.big_package.ΔRat>().SetString(input);
        if (!ok) {
            t.Errorf("Rat.SetString(%q) failed", input);
            continue;
        }
        var (f, exact) = r.Float32();

        // 1. Check string -> Rat -> float32 conversions are
        // consistent with strconv.ParseFloat.
        // Skip this check if the input uses "a/b" rational syntax.
        if (!strings.Contains(input, "/")) {
            var (e64, _) = strconv.ParseFloat(input, 32);
            var e = (float32)e64;

            // Careful: negative Rats too small for
            // float64 become -0, but Rat obviously cannot
            // preserve the sign from SetString("-0").
            switch (ᐧ) {
            case {} when math.Float32bits(e) == math.Float32bits(f): {
                // Ok: bitwise equal.
                break;
            }
            case {} when f == 0F && r.Num().BitLen() == 0: {
                // Ok: Rat(0) is equivalent to both +/- float64(0).
                break;
            }
            default: {
                t.Errorf("strconv.ParseFloat(%q) = %g (%b), want %g (%b); delta = %g", input, e, e, f, f, f - e);
                break;
            }}
        }

        if (!isFinite((float64)f)) {
            continue;
        }

        // 2. Check f is best approximation to r.
        if (!checkIsBestApprox32(t, f, r)) {
            // Append context information.
            t.Errorf("(input was %q)", input);
        }

        // 3. Check f->R->f roundtrip is non-lossy.
        checkNonLossyRoundtrip32(t, f);

        // 4. Check exactness using slow algorithm.
        var wasExact = @new<global::go.math.big_package.ΔRat>().SetFloat64((float64)f).Cmp(r) == 0; if (wasExact != exact) {
            t.Errorf("Rat.SetString(%q).Float32().exact = %t, want %t", input, exact, wasExact);
        }
    }
}

public static void TestFloat64SpecialCases(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in range(float64inputs)) {
        var input = vᴛ1;

        if (strings.HasPrefix(input, longˢ)) {
            if (!@long.Value) {
                continue;
            }
            input = input[(int)(len("long:"))..];
        }

        var (r, ok) = @new<global::go.math.big_package.ΔRat>().SetString(input);
        if (!ok) {
            t.Errorf("Rat.SetString(%q) failed", input);
            continue;
        }
        var (f, exact) = r.Float64();

        // 1. Check string -> Rat -> float64 conversions are
        // consistent with strconv.ParseFloat.
        // Skip this check if the input uses "a/b" rational syntax.
        if (!strings.Contains(input, "/")) {
            var (e, _) = strconv.ParseFloat(input, 64);

            // Careful: negative Rats too small for
            // float64 become -0, but Rat obviously cannot
            // preserve the sign from SetString("-0").
            switch (ᐧ) {
            case {} when math.Float64bits(e) == math.Float64bits(f): {
                // Ok: bitwise equal.
                break;
            }
            case {} when f == 0D && r.Num().BitLen() == 0: {
                // Ok: Rat(0) is equivalent to both +/- float64(0).
                break;
            }
            default: {
                t.Errorf("strconv.ParseFloat(%q) = %g (%b), want %g (%b); delta = %g", input, e, e, f, f, f - e);
                break;
            }}
        }

        if (!isFinite(f)) {
            continue;
        }

        // 2. Check f is best approximation to r.
        if (!checkIsBestApprox64(t, f, r)) {
            // Append context information.
            t.Errorf("(input was %q)", input);
        }

        // 3. Check f->R->f roundtrip is non-lossy.
        checkNonLossyRoundtrip64(t, f);

        // 4. Check exactness using slow algorithm.
        var wasExact = @new<global::go.math.big_package.ΔRat>().SetFloat64(f).Cmp(r) == 0; if (wasExact != exact) {
            t.Errorf("Rat.SetString(%q).Float64().exact = %t, want %t", input, exact, wasExact);
        }
    }
}

public static void TestIssue31184(ж<testing.T> t) {
    ref var x = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡx);
    foreach (var (_, want) in range(slice<@string>([
        "-213.090",
        "8.192",
        "16.000"
    ]))) {
        Ꮡx.SetString(want);
        @string got = Ꮡx.FloatString(3);
        if (got != want) {
            t.Errorf("got %s, want %s", got, want);
        }
    }
}

internal partial struct TestIssue45910_type /*dyn*/ {
    internal @string input;
    internal bool want;
}

public static void TestIssue45910(ж<testing.T> t) {
    ref var x = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡx);
    foreach (var (_, test) in range(slice<TestIssue45910_type>([
        new("1e-1000001", false),
        new("1e-1000000", true),
        new("1e+1000000", true),
        new("1e+1000001", false),

        new("0p1000000000000", true),
        new("1p-10000001", false),
        new("1p-10000000", true),
        new("1p+10000000", true),
        new("1p+10000001", false),
        new("1.770p02041010010011001001", false) // test case from issue
    ]))) {
        var (_, got) = Ꮡx.SetString(test.input);
        if (got != test.want) {
            t.Errorf("SetString(%s) got ok = %v; want %v", test.input, got, test.want);
        }
    }
}
internal partial struct TestFloatPrec_type /*dyn*/ {
    internal @string f;
    internal nint prec;
    internal bool ok;
    internal @string fdec;
}

public static void TestFloatPrec(ж<testing.T> t) {
    slice<TestFloatPrec_type> tests = slice<TestFloatPrec_type>([
        // examples from the issue #50489
        new("10/100", 1, true, "0.1"),
        new("3/100", 2, true, "0.03"),
        new("10", 0, true, "10"),

        // more examples
        new("zero", 0, true, "0"),      // test uninitialized zero value for Rat
        new("0", 0, true, "0"),         // 0
        new("1", 0, true, "1"),         // 1
        new("1/2", 1, true, "0.5"),     // 0.5
        new("1/3", 0, false, "0"),      // 0.(3)
        new("1/4", 2, true, "0.25"),    // 0.25
        new("1/5", 1, true, "0.2"),     // 0.2
        new("1/6", 1, false, "0.2"),    // 0.1(6)
        new("1/7", 0, false, "0"),      // 0.(142857)
        new("1/8", 3, true, "0.125"),   // 0.125
        new("1/9", 0, false, "0"),      // 0.(1)
        new("1/10", 1, true, "0.1"),    // 0.1
        new("1/11", 0, false, "0"),     // 0.(09)
        new("1/12", 2, false, "0.08"),  // 0.08(3)
        new("1/13", 0, false, "0"),     // 0.(076923)
        new("1/14", 1, false, "0.1"),   // 0.0(714285)
        new("1/15", 1, false, "0.1"),   // 0.0(6)
        new("1/16", 4, true, "0.0625"), // 0.0625

        new("10/2", 0, true, "5"),                   // 5
        new("10/3", 0, false, "3"),                  // 3.(3)
        new("10/6", 0, false, "2"),                  // 1.(6)
        new("1/10000000", 7, true, "0.0000001"),     // 0.0000001
        new("1/3125", 5, true, "0.00032"),           // "0.00032"
        new("1/1024", 10, true, "0.0009765625"),     // 0.0009765625
        new("1/304000", 7, false, "0.0000033"),      // 0.0000032(894736842105263157)
        new("1/48828125", 11, true, "0.00000002048") // 0.00000002048
    ]);

    foreach (var (_, test) in range(tests)) {
        ref var f = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡf);

        // check uninitialized zero value
        if (test.f != "zero"u8) {
            var (_, ok) = Ꮡf.SetString(test.f);
            if (!ok) {
                t.Fatalf("invalid test case: f = %s", test.f);
            }
        }

        // results for f and -f must be the same
        @string fdec = test.fdec;
        for (nint i = 0; i < 2; i++) {
            var (prec, ok) = Ꮡf.FloatPrec();
            if (prec != test.prec || ok != test.ok) {
                t.Errorf("%s: FloatPrec(%s): got prec, ok = %d, %v; want %d, %v", test.f, Ꮡf, prec, ok, test.prec, test.ok);
            }
            @string s = Ꮡf.FloatString(test.prec);
            if (s != fdec) {
                t.Errorf("%s: FloatString(%s, %d): got %s; want %s", test.f, Ꮡf, prec, s, fdec);
            }
            // proceed with -f but don't add a "-" before a "0"
            if (f.Sign() > 0) {
                Ꮡf.Neg(Ꮡf);
                fdec = "-"u8 + fdec;
            }
        }
    }
}

public static void BenchmarkFloatPrecExact(ж<testing.B> b) {
    foreach (var (_, n) in range(slice<nint>([1, 10, 100, 1000, 10000, 100000, 1000000]))) {
        // d := 5^n
        var d = NewInt(5);
        var p = NewInt((int64)n);
        d.Exp(d, p, nil);

        // r := 1/d
        ref var r = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡr);
        Ꮡr.SetFrac(NewInt(1), d);

        b.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (prec, ok) = Ꮡr.FloatPrec();
                if (prec != n || !ok) {
                    bΔ1.Fatalf("got exact, ok = %d, %v; want %d, %v", prec, ok, (uint64)n, true);
                }
            }
        });
    }
}

public static void BenchmarkFloatPrecMixed(ж<testing.B> b) {
    foreach (var (_, n) in range(slice<nint>([1, 10, 100, 1000, 10000, 100000, 1000000]))) {
        // d := (3·5·7·11)^n
        var d = NewInt(3 * 5 * 7 * 11);
        var p = NewInt((int64)n);
        d.Exp(d, p, nil);

        // r := 1/d
        ref var r = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡr);
        Ꮡr.SetFrac(NewInt(1), d);

        b.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (prec, ok) = Ꮡr.FloatPrec();
                if (prec != n || ok) {
                    bΔ1.Fatalf("got exact, ok = %d, %v; want %d, %v", prec, ok, (uint64)n, false);
                }
            }
        });
    }
}

public static void BenchmarkFloatPrecInexact(ж<testing.B> b) {
    foreach (var (_, n) in range(slice<nint>([1, 10, 100, 1000, 10000, 100000, 1000000]))) {
        // d := 5^n + 1
        var d = NewInt(5);
        var p = NewInt((int64)n);
        d.Exp(d, p, nil);
        d.Add(d, NewInt(1));

        // r := 1/d
        ref var r = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡr);
        Ꮡr.SetFrac(NewInt(1), d);

        b.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (_, ok) = Ꮡr.FloatPrec();
                if (ok) {
                    bΔ1.Fatalf("got unexpected ok");
                }
            }
        });
    }
}

} // end big_internal_test_package
