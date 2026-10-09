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

// valid, without separators
// '_' is not part of the number anymore
// valid, with separators
// invalid: no digits
// invalid: incorrect use of separator

partial struct exponentTestsᴛ1 /*dyn*/ {
    internal @string s; // string to be scanned
    internal bool base2ok;   // true if 'p'/'P' exponents are accepted
    internal bool sepOk;   // true if '_' separators are accepted
    internal int64 x;  // expected exponent
    internal nint b;   // expected exponent base
    internal error err;  // expected error
    internal rune next;   // next character (or 0, if at EOF)
}
internal static slice<exponentTestsᴛ1> exponentTests;
internal static void initᴛexponentTests() { exponentTests = new exponentTestsᴛ1[]{
    new(""u8, false, false, 0, 10, default!, 0),
    new("1"u8, false, false, 0, 10, default!, (rune)'1'),
    new("e0"u8, false, false, 0, 10, default!, 0),
    new("E1"u8, false, false, 1, 10, default!, 0),
    new("e+10"u8, false, false, 10, 10, default!, 0),
    new("e-10"u8, false, false, -10, 10, default!, 0),
    new("e123456789a"u8, false, false, 123456789, 10, default!, (rune)'a'),
    new("p"u8, false, false, 0, 10, default!, (rune)'p'),
    new("P+100"u8, false, false, 0, 10, default!, (rune)'P'),
    new("p0"u8, true, false, 0, 2, default!, 0),
    new("P-123"u8, true, false, -123, 2, default!, 0),
    new("p+0a"u8, true, false, 0, 2, default!, (rune)'a'),
    new("p+123__"u8, true, false, 123, 2, default!, (rune)'_'),
    new("e+1_0"u8, false, true, 10, 10, default!, 0),
    new("e-1_0"u8, false, true, -10, 10, default!, 0),
    new("e123_456_789a"u8, false, true, 123456789, 10, default!, (rune)'a'),
    new("P+1_00"u8, false, true, 0, 10, default!, (rune)'P'),
    new("p-1_2_3"u8, true, true, -123, 2, default!, 0),
    new("e"u8, false, false, 0, 10, errNoDigits, 0),
    new("ef"u8, false, false, 0, 10, errNoDigits, (rune)'f'),
    new("e+"u8, false, false, 0, 10, errNoDigits, 0),
    new("E-x"u8, false, false, 0, 10, errNoDigits, (rune)'x'),
    new("p"u8, true, false, 0, 2, errNoDigits, 0),
    new("P-"u8, true, false, 0, 2, errNoDigits, 0),
    new("p+e"u8, true, false, 0, 2, errNoDigits, (rune)'e'),
    new("e+_x"u8, false, true, 0, 10, errNoDigits, (rune)'x'),
    new("e0_"u8, false, true, 0, 10, errInvalSep, 0),
    new("e_0"u8, false, true, 0, 10, errInvalSep, 0),
    new("e-1_2__3"u8, false, true, -123, 10, errInvalSep, 0)
}.slice(); }

public static void TestScanExponent(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in exponentTests) {
        var r = strings.NewReader(a.s);
        var (x, b, err) = scanExponent(new big_test_package.strings_ReaderжByteScanner(r), a.base2ok, a.sepOk);
        if (!AreEqual(err, a.err)) {
            Ꮡt.Errorf("scanExponent%+v\n\tgot error = %v; want %v"u8, a, err, a.err);
        }
        if (x != a.x) {
            Ꮡt.Errorf("scanExponent%+v\n\tgot z = %v; want %v"u8, a, x, a.x);
        }
        if (b != a.b) {
            Ꮡt.Errorf("scanExponent%+v\n\tgot b = %d; want %d"u8, a, b, a.b);
        }
        (var next, _, err) = r.ReadRune();
        if (AreEqual(err, io.EOF)) {
            next = 0;
            err = default!;
        }
        if (err == default! && next != a.next) {
            Ꮡt.Errorf("scanExponent%+v\n\tgot next = %q; want %q"u8, a, next, a.next);
        }
    }
}

public partial struct StringTest {
    internal @string @in, @out;
    internal bool ok;
}

// invalid
// issue 17001
// CVE-2022-23772
// valid
// issue #16176
internal static slice<StringTest> setStringTests = new StringTest[]{
    new(@in: "1e"u8),
    new(@in: "1.e"u8),
    new(@in: "1e+14e-5"u8),
    new(@in: "1e4.5"u8),
    new(@in: "r"u8),
    new(@in: "a/b"u8),
    new(@in: "a.b"u8),
    new(@in: "1/0"u8),
    new(@in: "4/3/2"u8),
    new(@in: "4/3/"u8),
    new(@in: "4/3."u8),
    new(@in: "4/"u8),
    new(@in: "13e-9223372036854775808"u8),
    new("0"u8, "0"u8, true),
    new("-0"u8, "0"u8, true),
    new("1"u8, "1"u8, true),
    new("-1"u8, "-1"u8, true),
    new("1."u8, "1"u8, true),
    new("1e0"u8, "1"u8, true),
    new("1.e1"u8, "10"u8, true),
    new("-0.1"u8, "-1/10"u8, true),
    new("-.1"u8, "-1/10"u8, true),
    new("2/4"u8, "1/2"u8, true),
    new(".25"u8, "1/4"u8, true),
    new("-1/5"u8, "-1/5"u8, true),
    new("8129567.7690E14"u8, "812956776900000000000"u8, true),
    new("78189e+4"u8, "781890000"u8, true),
    new("553019.8935e+8"u8, "55301989350000"u8, true),
    new("98765432109876543210987654321e-10"u8, "98765432109876543210987654321/10000000000"u8, true),
    new("9877861857500000E-7"u8, "3951144743/4"u8, true),
    new("2169378.417e-3"u8, "2169378417/1000000"u8, true),
    new("884243222337379604041632732738665534"u8, "884243222337379604041632732738665534"u8, true),
    new("53/70893980658822810696"u8, "53/70893980658822810696"u8, true),
    new("106/141787961317645621392"u8, "53/70893980658822810696"u8, true),
    new("204211327800791583.81095"u8, "4084226556015831676219/20000"u8, true),
    new("0e9999999999"u8, "0"u8, true)
}.slice();

// invalid
// invalid with separators
// (smoke tests only - a comprehensive set of tests is in natconv_test.go)
// valid
// 0-prefix indicates octal in this case
// 0-prefix is ignored in this case (not a fraction)
// E is part of hex mantissa, not exponent
// valid with separators
// (smoke tests only - a comprehensive set of tests is in natconv_test.go)
// These are not supported by fmt.Fscanf.
internal static slice<StringTest> setStringTests2 = new StringTest[]{
    new(@in: "4/3x"u8),
    new(@in: "0/-1"u8),
    new(@in: "-1/-1"u8),
    new(@in: "10_/1"u8),
    new(@in: "_10/1"u8),
    new(@in: "1/1__0"u8),
    new("0b1000/3"u8, "8/3"u8, true),
    new("0B1000/0x8"u8, "1"u8, true),
    new("-010/1"u8, "-8"u8, true),
    new("-010.0"u8, "-10"u8, true),
    new("-0o10/1"u8, "-8"u8, true),
    new("0x10/1"u8, "16"u8, true),
    new("0x10/0x20"u8, "1/2"u8, true),
    new("0010"u8, "10"u8, true),
    new("0x10.0"u8, "16"u8, true),
    new("0x1.8"u8, "3/2"u8, true),
    new("0X1.8p4"u8, "24"u8, true),
    new("0x1.1E2"u8, "2289/2048"u8, true),
    new("0b1.1E2"u8, "150"u8, true),
    new("0B1.1P3"u8, "12"u8, true),
    new("0o10e-2"u8, "2/25"u8, true),
    new("0O10p-3"u8, "1"u8, true),
    new("0b_1000/3"u8, "8/3"u8, true),
    new("0B_10_00/0x8"u8, "1"u8, true),
    new("0xdead/0B1101_1110_1010_1101"u8, "1"u8, true),
    new("0B1101_1110_1010_1101/0XD_E_A_D"u8, "1"u8, true),
    new("1_000.0"u8, "1000"u8, true),
    new("0x_10.0"u8, "16"u8, true),
    new("0x1_0.0"u8, "16"u8, true),
    new("0x1.8_0"u8, "3/2"u8, true),
    new("0X1.8p0_4"u8, "24"u8, true),
    new("0b1.1_0E2"u8, "150"u8, true),
    new("0o1_0e-2"u8, "2/25"u8, true),
    new("0O_10p-3"u8, "1"u8, true)
}.slice();

public static void TestRatSetString(ж<testing.T> Ꮡt) {
    slice<StringTest> tests = default!;
    tests = appendꓸꓸꓸ(tests, setStringTests);
    tests = appendꓸꓸꓸ(tests, setStringTests2);
    foreach (var (i, test) in tests) {
        var (x, ok) = @new<global::go.math.big_package.ΔRat>().SetString(test.@in);
        if (ok){
            if (!test.ok){
                Ꮡt.Errorf("#%d SetString(%q) expected failure"u8, i, test.@in);
            } else 
            if (x.RatString() != test.@out) {
                Ꮡt.Errorf("#%d SetString(%q) got %s want %s"u8, i, test.@in, x.RatString(), test.@out);
            }
        } else {
            if (test.ok){
                Ꮡt.Errorf("#%d SetString(%q) expected success"u8, i, test.@in);
            } else 
            if (x != nil) {
                Ꮡt.Errorf("#%d SetString(%q) got %p want nil"u8, i, test.@in, x.OrTypedNil());
            }
        }
    }
}

public static void TestRatSetStringZero(ж<testing.T> Ꮡt) {
    var (got, _) = @new<global::go.math.big_package.ΔRat>().SetString("0"u8);
    var want = @new<global::go.math.big_package.ΔRat>().SetInt64(0);
    if (!reflect.DeepEqual(got.OrTypedNil(), want.OrTypedNil())) {
        Ꮡt.Errorf("got %#+v, want %#+v"u8, got.OrTypedNil(), want.OrTypedNil());
    }
}

public static void TestRatScan(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (i, test) in setStringTests) {
        var x = @new<global::go.math.big_package.ΔRat>();
        buf.Reset();
        buf.WriteString(test.@in);
        var (_, err) = fmt.Fscanf(new big_test_package.bytes_BufferжReader(Ꮡbuf), "%v"u8, x.OrTypedNil());
        if (err == default! != test.ok) {
            if (test.ok){
                Ꮡt.Errorf("#%d (%s) error: %s"u8, i, test.@in, err);
            } else {
                Ꮡt.Errorf("#%d (%s) expected error"u8, i, test.@in);
            }
            continue;
        }
        if (err == default! && x.RatString() != test.@out) {
            Ꮡt.Errorf("#%d got %s want %s"u8, i, x.RatString(), test.@out);
        }
    }
}


partial struct floatStringTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal nint prec;
    internal @string @out;
}
internal static slice<floatStringTestsᴛ1> floatStringTests = new floatStringTestsᴛ1[]{
    new("0"u8, 0, "0"u8),
    new("0"u8, 4, "0.0000"u8),
    new("1"u8, 0, "1"u8),
    new("1"u8, 2, "1.00"u8),
    new("-1"u8, 0, "-1"u8),
    new("0.05"u8, 1, "0.1"u8),
    new("-0.05"u8, 1, "-0.1"u8),
    new(".25"u8, 2, "0.25"u8),
    new(".25"u8, 1, "0.3"u8),
    new(".25"u8, 3, "0.250"u8),
    new("-1/3"u8, 3, "-0.333"u8),
    new("-2/3"u8, 4, "-0.6667"u8),
    new("0.96"u8, 1, "1.0"u8),
    new("0.999"u8, 2, "1.00"u8),
    new("0.9"u8, 0, "1"u8),
    new(".25"u8, -1, "0"u8),
    new(".55"u8, -1, "1"u8)
}.slice();

public static void TestFloatString(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in floatStringTests) {
        var (x, _) = @new<global::go.math.big_package.ΔRat>().SetString(test.@in);
        if (x.FloatString(test.prec) != test.@out) {
            Ꮡt.Errorf("#%d got %s want %s"u8, i, x.FloatString(test.prec), test.@out);
        }
    }
}

// Constants plundered from strconv/testfp.txt.
// Table 1: Stress Inputs for Conversion to 53-bit Binary, < 1/2 ULP
// Table 2: Stress Inputs for Conversion to 53-bit Binary, > 1/2 ULP
// Table 14: Stress Inputs for Conversion to 24-bit Binary, <1/2 ULP
// Table 15: Stress Inputs for Conversion to 24-bit Binary, >1/2 ULP
// Constants plundered from strconv/atof_test.go.
// NB: exception made for this input
// largest float64
// next float64 - too large
// the border is ...158079
// borderline - okay
// borderline - too large
// a little too large
// way too large
// denormalized
// smallest denormal
// too small
// way too small
// way too small, negative
// try to overflow exponent
// [Disabled: too slow and memory-hungry with rationals.]
// "1e-4294967296",
// "1e+4294967296",
// "1e-18446744073709551616",
// "1e+18446744073709551616",
// https://www.exploringbinary.com/java-hangs-when-converting-2-2250738585072012e-308/
// https://www.exploringbinary.com/php-hangs-on-numeric-value-2-2250738585072011e-308/
// A very large number (initially wrongly parsed by the fast algorithm).
// A different kind of very large number.
// Exactly halfway between 1 and math.Nextafter(1, 2).
// Round to even (down).
// Slightly lower; still round down.
// Slightly higher; round up.
// Slightly higher, but you have to read all the way to the end.
// Smallest denormal, 2^(-1022-52)
// Half of smallest denormal, 2^(-1022-53)
// A little more than the exact half of smallest denormal
// 2^-1075 + 2^-1100.  (Rounds to 1p-1074.)
// The exact halfway between smallest normal and largest denormal:
// 2^-1022 - 2^-1075.  (Rounds to 2^-1022.)
//   1<<60 - 1
// -(1<<60 - 1)
//   1<<60 + 1
// -(1<<60 + 1)
// Test inputs to Rat.SetString. The prefix "long:" causes the test
// to be skipped except in -long mode.  (The threshold is about 500us.)
internal static slice<@string> float64inputs = new @string[]{
    "5e+125"u8,
    "69e+267"u8,
    "999e-026"u8,
    "7861e-034"u8,
    "75569e-254"u8,
    "928609e-261"u8,
    "9210917e+080"u8,
    "84863171e+114"u8,
    "653777767e+273"u8,
    "5232604057e-298"u8,
    "27235667517e-109"u8,
    "653532977297e-123"u8,
    "3142213164987e-294"u8,
    "46202199371337e-072"u8,
    "231010996856685e-073"u8,
    "9324754620109615e+212"u8,
    "78459735791271921e+049"u8,
    "272104041512242479e+200"u8,
    "6802601037806061975e+198"u8,
    "20505426358836677347e-221"u8,
    "836168422905420598437e-234"u8,
    "4891559871276714924261e+222"u8,
    "9e-265"u8,
    "85e-037"u8,
    "623e+100"u8,
    "3571e+263"u8,
    "81661e+153"u8,
    "920657e-023"u8,
    "4603285e-024"u8,
    "87575437e-309"u8,
    "245540327e+122"u8,
    "6138508175e+120"u8,
    "83356057653e+193"u8,
    "619534293513e+124"u8,
    "2335141086879e+218"u8,
    "36167929443327e-159"u8,
    "609610927149051e-255"u8,
    "3743626360493413e-165"u8,
    "94080055902682397e-242"u8,
    "899810892172646163e+283"u8,
    "7120190517612959703e+120"u8,
    "25188282901709339043e-252"u8,
    "308984926168550152811e-052"u8,
    "6372891218502368041059e+064"u8,
    "5e-20"u8,
    "67e+14"u8,
    "985e+15"u8,
    "7693e-42"u8,
    "55895e-16"u8,
    "996622e-44"u8,
    "7038531e-32"u8,
    "60419369e-46"u8,
    "702990899e-20"u8,
    "6930161142e-48"u8,
    "25933168707e+13"u8,
    "596428896559e+20"u8,
    "3e-23"u8,
    "57e+18"u8,
    "789e-35"u8,
    "2539e-18"u8,
    "76173e+28"u8,
    "887745e-11"u8,
    "5382571e-37"u8,
    "82381273e-35"u8,
    "750486563e-38"u8,
    "3752432815e-39"u8,
    "75224575729e-45"u8,
    "459926601011e+15"u8,
    "0"u8,
    "1"u8,
    "+1"u8,
    "1e23"u8,
    "1E23"u8,
    "100000000000000000000000"u8,
    "1e-100"u8,
    "123456700"u8,
    "99999999999999974834176"u8,
    "100000000000000000000001"u8,
    "100000000000000008388608"u8,
    "100000000000000016777215"u8,
    "100000000000000016777216"u8,
    "-1"u8,
    "-0.1"u8,
    "-0"u8,
    "1e-20"u8,
    "625e-3"u8,
    "1.7976931348623157e308"u8,
    "-1.7976931348623157e308"u8,
    "1.7976931348623159e308"u8,
    "-1.7976931348623159e308"u8,
    "1.7976931348623158e308"u8,
    "-1.7976931348623158e308"u8,
    "1.797693134862315808e308"u8,
    "-1.797693134862315808e308"u8,
    "1e308"u8,
    "2e308"u8,
    "1e309"u8,
    "1e310"u8,
    "-1e310"u8,
    "1e400"u8,
    "-1e400"u8,
    "long:1e400000"u8,
    "long:-1e400000"u8,
    "1e-305"u8,
    "1e-306"u8,
    "1e-307"u8,
    "1e-308"u8,
    "1e-309"u8,
    "1e-310"u8,
    "1e-322"u8,
    "5e-324"u8,
    "4e-324"u8,
    "3e-324"u8,
    "2e-324"u8,
    "1e-350"u8,
    "long:1e-400000"u8,
    "-1e-350"u8,
    "long:-1e-400000"u8,
    "2.2250738585072012e-308"u8,
    "2.2250738585072011e-308"u8,
    "4.630813248087435e+307"u8,
    "22.222222222222222"u8,
    "long:2."u8 + strings.Repeat("2"u8, 4000) + "e+1"u8,
    "1.00000000000000011102230246251565404236316680908203125"u8,
    "1.00000000000000011102230246251565404236316680908203124"u8,
    "1.00000000000000011102230246251565404236316680908203126"u8,
    "long:1.00000000000000011102230246251565404236316680908203125"u8 + strings.Repeat("0"u8, 10000) + "1"u8,
    "4.940656458412465441765687928682213723651e-324"u8,
    "2.470328229206232720882843964341106861825e-324"u8,
    "2.470328302827751011111470718709768633275e-324"u8,
    "2.225073858507201136057409796709131975935e-308"u8,
    "1152921504606846975"u8,
    "-1152921504606846975"u8,
    "1152921504606846977"u8,
    "-1152921504606846977"u8,
    "1/3"u8
}.slice();

// isFinite reports whether f represents a finite rational value.
// It is equivalent to !math.IsNan(f) && !math.IsInf(f, 0).
internal static bool isFinite(float64 f) {
    return math.Abs(f) <= math.MaxFloat64;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string longˢ = "long:"u8;

public static void TestFloat32SpecialCases(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in float64inputs) {
        var input = vᴛ1;

        if (strings.HasPrefix(input, longˢ)) {
            if (!@long.Value) {
                continue;
            }
            input = input[(int)(len("long:"))..];
        }
        var (r, ok) = @new<global::go.math.big_package.ΔRat>().SetString(input);
        if (!ok) {
            Ꮡt.Errorf("Rat.SetString(%q) failed"u8, input);
            continue;
        }
        var (f, exact) = r.Float32();
        // 1. Check string -> Rat -> float32 conversions are
        // consistent with strconv.ParseFloat.
        // Skip this check if the input uses "a/b" rational syntax.
        if (!strings.Contains(input, "/"u8)) {
            var (e64, _) = strconv.ParseFloat(input, 32);
            var e = (float32)e64;
            // Careful: negative Rats too small for
            // float64 become -0, but Rat obviously cannot
            // preserve the sign from SetString("-0").
            switch (ᐧ) {
            case {} when math.Float32bits(e) == math.Float32bits(f): {
                break;
            }
            case {} when f == 0F && r.Num().BitLen() == 0: {
                break;
            }
            default: {
                Ꮡt.Errorf("strconv.ParseFloat(%q) = %g (%b), want %g (%b); delta = %g"u8, // Ok: bitwise equal.
 // Ok: Rat(0) is equivalent to both +/- float64(0).
 input, e, e, f, f, f - e);
                break;
            }}

        }
        if (!isFinite((float64)f)) {
            continue;
        }
        // 2. Check f is best approximation to r.
        if (!checkIsBestApprox32(Ꮡt, f, r)) {
            // Append context information.
            Ꮡt.Errorf("(input was %q)"u8, input);
        }
        // 3. Check f->R->f roundtrip is non-lossy.
        checkNonLossyRoundtrip32(Ꮡt, f);
        // 4. Check exactness using slow algorithm.
        {
            var wasExact = @new<global::go.math.big_package.ΔRat>().SetFloat64((float64)f).Cmp(r) == 0; if (wasExact != exact) {
                Ꮡt.Errorf("Rat.SetString(%q).Float32().exact = %t, want %t"u8, input, exact, wasExact);
            }
        }
    }
}

public static void TestFloat64SpecialCases(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, vᴛ1) in float64inputs) {
        var input = vᴛ1;

        if (strings.HasPrefix(input, longˢ)) {
            if (!@long.Value) {
                continue;
            }
            input = input[(int)(len("long:"))..];
        }
        var (r, ok) = @new<global::go.math.big_package.ΔRat>().SetString(input);
        if (!ok) {
            Ꮡt.Errorf("Rat.SetString(%q) failed"u8, input);
            continue;
        }
        var (f, exact) = r.Float64();
        // 1. Check string -> Rat -> float64 conversions are
        // consistent with strconv.ParseFloat.
        // Skip this check if the input uses "a/b" rational syntax.
        if (!strings.Contains(input, "/"u8)) {
            var (e, _) = strconv.ParseFloat(input, 64);
            // Careful: negative Rats too small for
            // float64 become -0, but Rat obviously cannot
            // preserve the sign from SetString("-0").
            switch (ᐧ) {
            case {} when math.Float64bits(e) == math.Float64bits(f): {
                break;
            }
            case {} when f == 0D && r.Num().BitLen() == 0: {
                break;
            }
            default: {
                Ꮡt.Errorf("strconv.ParseFloat(%q) = %g (%b), want %g (%b); delta = %g"u8, // Ok: bitwise equal.
 // Ok: Rat(0) is equivalent to both +/- float64(0).
 input, e, e, f, f, f - e);
                break;
            }}

        }
        if (!isFinite(f)) {
            continue;
        }
        // 2. Check f is best approximation to r.
        if (!checkIsBestApprox64(Ꮡt, f, r)) {
            // Append context information.
            Ꮡt.Errorf("(input was %q)"u8, input);
        }
        // 3. Check f->R->f roundtrip is non-lossy.
        checkNonLossyRoundtrip64(Ꮡt, f);
        // 4. Check exactness using slow algorithm.
        {
            var wasExact = @new<global::go.math.big_package.ΔRat>().SetFloat64(f).Cmp(r) == 0; if (wasExact != exact) {
                Ꮡt.Errorf("Rat.SetString(%q).Float64().exact = %t, want %t"u8, input, exact, wasExact);
            }
        }
    }
}

public static void TestIssue31184(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡx);
    foreach (var (_, want) in new @string[]{
        "-213.090"u8,
        "8.192"u8,
        "16.000"u8
    }.slice()) {
        Ꮡx.SetString(want);
        @string got = Ꮡx.FloatString(3);
        if (got != want) {
            Ꮡt.Errorf("got %s, want %s"u8, got, want);
        }
    }
}

internal partial struct TestIssue45910_type /*dyn*/ {
    internal @string input;
    internal bool want;
}

public static void TestIssue45910(ж<testing.T> Ꮡt) {
    ref var x = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡx);
    foreach (var (_, test) in new TestIssue45910_type[]{
        new("1e-1000001"u8, false),
        new("1e-1000000"u8, true),
        new("1e+1000000"u8, true),
        new("1e+1000001"u8, false),
        new("0p1000000000000"u8, true),
        new("1p-10000001"u8, false),
        new("1p-10000000"u8, true),
        new("1p+10000000"u8, true),
        new("1p+10000001"u8, false),
        new("1.770p02041010010011001001"u8, false)
    }.slice()) {
        // test case from issue
        var (_, got) = Ꮡx.SetString(test.input);
        if (got != test.want) {
            Ꮡt.Errorf("SetString(%s) got ok = %v; want %v"u8, test.input, got, test.want);
        }
    }
}

internal partial struct TestFloatPrec_type /*dyn*/ {
    internal @string f;
    internal nint prec;
    internal bool ok;
    internal @string fdec;
}

public static void TestFloatPrec(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

// examples from the issue #50489
// more examples
// test uninitialized zero value for Rat
// 0
// 1
// 0.5
// 0.(3)
// 0.25
// 0.2
// 0.1(6)
// 0.(142857)
// 0.125
// 0.(1)
// 0.1
// 0.(09)
// 0.08(3)
// 0.(076923)
// 0.0(714285)
// 0.0(6)
// 0.0625
// 5
// 3.(3)
// 1.(6)
// 0.0000001
// "0.00032"
// 0.0009765625
// 0.0000032(894736842105263157)
// 0.00000002048
    slice<TestFloatPrec_type> tests = new TestFloatPrec_type[]{
        new("10/100"u8, 1, true, "0.1"u8),
        new("3/100"u8, 2, true, "0.03"u8),
        new("10"u8, 0, true, "10"u8),
        new("zero"u8, 0, true, "0"u8),
        new("0"u8, 0, true, "0"u8),
        new("1"u8, 0, true, "1"u8),
        new("1/2"u8, 1, true, "0.5"u8),
        new("1/3"u8, 0, false, "0"u8),
        new("1/4"u8, 2, true, "0.25"u8),
        new("1/5"u8, 1, true, "0.2"u8),
        new("1/6"u8, 1, false, "0.2"u8),
        new("1/7"u8, 0, false, "0"u8),
        new("1/8"u8, 3, true, "0.125"u8),
        new("1/9"u8, 0, false, "0"u8),
        new("1/10"u8, 1, true, "0.1"u8),
        new("1/11"u8, 0, false, "0"u8),
        new("1/12"u8, 2, false, "0.08"u8),
        new("1/13"u8, 0, false, "0"u8),
        new("1/14"u8, 1, false, "0.1"u8),
        new("1/15"u8, 1, false, "0.1"u8),
        new("1/16"u8, 4, true, "0.0625"u8),
        new("10/2"u8, 0, true, "5"u8),
        new("10/3"u8, 0, false, "3"u8),
        new("10/6"u8, 0, false, "2"u8),
        new("1/10000000"u8, 7, true, "0.0000001"u8),
        new("1/3125"u8, 5, true, "0.00032"u8),
        new("1/1024"u8, 10, true, "0.0009765625"u8),
        new("1/304000"u8, 7, false, "0.0000033"u8),
        new("1/48828125"u8, 11, true, "0.00000002048"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        ref var f = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡf);
        // check uninitialized zero value
        if (test.f != "zero"u8) {
            var (_, ok) = Ꮡf.SetString(test.f);
            if (!ok) {
                Ꮡt.Fatalf("invalid test case: f = %s"u8, test.f);
            }
        }
        // results for f and -f must be the same
        @string fdec = test.fdec;
        for (nint i = 0; i < 2; i++) {
            var (prec, ok) = Ꮡf.FloatPrec();
            if (prec != test.prec || ok != test.ok) {
                Ꮡt.Errorf("%s: FloatPrec(%s): got prec, ok = %d, %v; want %d, %v"u8, test.f, Ꮡf, prec, ok, test.prec, test.ok);
            }
            @string s = Ꮡf.FloatString(test.prec);
            if (s != fdec) {
                Ꮡt.Errorf("%s: FloatString(%s, %d): got %s; want %s"u8, test.f, Ꮡf, prec, s, fdec);
            }
            // proceed with -f but don't add a "-" before a "0"
            if (f.Sign() > 0) {
                Ꮡf.Neg(Ꮡf);
                fdec = "-"u8 + fdec;
            }
        }
    }
}

public static void BenchmarkFloatPrecExact(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in new nint[]{1, 10, 100, 1000, 10000, 100000, 1000000}.slice()) {
        // d := 5^n
        var d = NewInt(5);
        var p = NewInt((int64)n);
        d.Exp(d, p, nil);
        // r := 1/d
        ref var r = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡr);
        Ꮡr.SetFrac(NewInt(1), d);
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (prec, ok) = Ꮡr.FloatPrec();
                if (prec != n || !ok) {
                    bΔ1.Fatalf("got exact, ok = %d, %v; want %d, %v"u8, prec, ok, (uint64)n, true);
                }
            }
        });
    }
}

public static void BenchmarkFloatPrecMixed(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in new nint[]{1, 10, 100, 1000, 10000, 100000, 1000000}.slice()) {
        // d := (3·5·7·11)^n
        var d = NewInt(3 * 5 * 7 * 11);
        var p = NewInt((int64)n);
        d.Exp(d, p, nil);
        // r := 1/d
        ref var r = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡr);
        Ꮡr.SetFrac(NewInt(1), d);
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (prec, ok) = Ꮡr.FloatPrec();
                if (prec != n || ok) {
                    bΔ1.Fatalf("got exact, ok = %d, %v; want %d, %v"u8, prec, ok, (uint64)n, false);
                }
            }
        });
    }
}

public static void BenchmarkFloatPrecInexact(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in new nint[]{1, 10, 100, 1000, 10000, 100000, 1000000}.slice()) {
        // d := 5^n + 1
        var d = NewInt(5);
        var p = NewInt((int64)n);
        d.Exp(d, p, nil);
        d.Add(d, NewInt(1));
        // r := 1/d
        ref var r = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡr);
        Ꮡr.SetFrac(NewInt(1), d);
        Ꮡb.Run(fmt.Sprint(n), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var (_, ok) = Ꮡr.FloatPrec();
                if (ok) {
                    bΔ1.Fatalf("got unexpected ok"u8);
                }
            }
        });
    }
}

} // end big_internal_test_package
