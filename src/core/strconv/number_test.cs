// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Note: These tests are focused mainly on generating the right errors.
// The extensive numerical tests are in ../internal/strconv.
// Add new tests there instead of here whenever possible.
namespace go;

using bytes = bytes_package;
using errors = errors_package;
using Δmath = math_package;
using cmplx = go.math.cmplx_package;
using reflect = reflect_package;
using static strconv_package;
using testing = testing_package;
using go.math;
using static go.strconv_internal_test_package;
using strconv = strconv_package;

partial class strconv_test_package {

partial struct atobTest {
    internal @string @in;
    internal bool @out;
    internal error err;
}

internal static slice<atobTest> atobtests = [
    new("", false, ErrSyntax),
    new("asdf", false, ErrSyntax),
    new("0", false, default),
    new("false", false, default),
    new("true", true, default)
];

public static void TestParseBool(ж<testing.T> t) {
    foreach (var (_, test) in range(atobtests)) {
        var (b, e) = ParseBool(test.@in);
        if (test.err != nil) {
            // expect an error
            if (e == nil) {
                t.Errorf("ParseBool(%s) = nil; want %s", test.@in, test.err);
            } else {
                // NumError assertion must succeed; it's the only thing we return.
                if (!AreEqual(e._<ж<strconv.NumError>>().Err, test.err)) {
                    t.Errorf("ParseBool(%s) = %s; want %s", test.@in, e, test.err);
                }
            }
        } else {
            if (e != nil) {
                t.Errorf("ParseBool(%s) = %s; want nil", test.@in, e);
            }
            if (b != test.@out) {
                t.Errorf("ParseBool(%s) = %t; want %t", test.@in, b, test.@out);
            }
        }
    }
}

internal static map<bool, @string> boolString = new map<bool, @string>{
    [true] = "true",
    [false] = "false"
};

public static void TestFormatBool(ж<testing.T> t) {
    foreach (var (b, s) in boolString) {
        @string f = FormatBool(b); if (f != s) {
            t.Errorf("FormatBool(%v) = %q; want %q", b, f, s);
        }
    }
}

partial struct appendBoolTest {
    internal bool b;
    internal slice<byte> @in;
    internal slice<byte> @out;
}

internal static slice<appendBoolTest> appendBoolTests = [
    new(true, slice<byte>("foo "u8), slice<byte>("foo true"u8)),
    new(false, slice<byte>("foo "u8), slice<byte>("foo false"u8))
];

public static void TestAppendBool(ж<testing.T> t) {
    foreach (var (_, test) in range(appendBoolTests)) {
        var b = AppendBool(test.@in, test.b);
        if (!bytes.Equal(b, test.@out)) {
            t.Errorf("AppendBool(%q, %v) = %q; want %q", test.@in, test.b, b, test.@out);
        }
    }
}

internal static complex128 infp0 = complex(Δmath.Inf(+1), 0D);
internal static complex128 infm0 = complex(Δmath.Inf(-1), 0D);
internal static complex128 inf0p = complex(0D, Δmath.Inf(+1));
internal static complex128 inf0m = complex(0D, Δmath.Inf(-1));

internal static complex128 infpp = complex(Δmath.Inf(+1), Δmath.Inf(+1));
internal static complex128 infpm = complex(Δmath.Inf(+1), Δmath.Inf(-1));
internal static complex128 infmp = complex(Δmath.Inf(-1), Δmath.Inf(+1));
internal static complex128 infmm = complex(Δmath.Inf(-1), Δmath.Inf(-1));

partial struct atocTest {
    internal @string @in;
    internal complex128 @out;
    internal error err;
}

public static void TestParseComplex(ж<testing.T> t) {
    var tests = slice<atocTest>([
        // Clearly invalid
        new("", 0D, ErrSyntax),
        new(" ", 0D, ErrSyntax),
        new("(", 0D, ErrSyntax),
        new(")", 0D, ErrSyntax),
        new("i", 0D, ErrSyntax),
        new("+i", 0D, ErrSyntax),
        new("-i", 0D, ErrSyntax),
        new("1I", 0D, ErrSyntax),
        new("10  + 5i", 0D, ErrSyntax),
        new("3+", 0D, ErrSyntax),
        new("3+5", 0D, ErrSyntax),
        new("3+5+5i", 0D, ErrSyntax),

        // Parentheses
        new("()", 0D, ErrSyntax),
        new("(i)", 0D, ErrSyntax),
        new("(0)", 0D, default),
        new("(1i)", 1D.i(), default),
        new("(3.0+5.5i)", 3D + 5.5D.i(), default),
        new("(1)+1i", 0D, ErrSyntax),
        new("(3.0+5.5i", 0D, ErrSyntax),
        new("3.0+5.5i)", 0D, ErrSyntax),

        // NaNs
        new("NaN", complex(Δmath.NaN(), 0D), default),
        new("NANi", complex(0D, Δmath.NaN()), default),
        new("nan+nAni", complex(Δmath.NaN(), Δmath.NaN()), default),
        new("+NaN", 0D, ErrSyntax),
        new("-NaN", 0D, ErrSyntax),
        new("NaN-NaNi", 0D, ErrSyntax),

        // Infs
        new("Inf", infp0, default),
        new("+inf", infp0, default),
        new("-inf", infm0, default),
        new("Infinity", infp0, default),
        new("+INFINITY", infp0, default),
        new("-infinity", infm0, default),
        new("+infi", inf0p, default),
        new("0-infinityi", inf0m, default),
        new("Inf+Infi", infpp, default),
        new("+Inf-Infi", infpm, default),
        new("-Infinity+Infi", infmp, default),
        new("inf-inf", 0D, ErrSyntax),

        // Zeros
        new("0", 0D, default),
        new("0i", 0D, default),
        new("-0.0i", 0D, default),
        new("0+0.0i", 0D, default),
        new("0e+0i", 0D, default),
        new("0e-0+0i", 0D, default),
        new("-0.0-0.0i", 0D, default),
        new("0e+012345", 0D, default),
        new("0x0p+012345i", 0D, default),
        new("0x0.00p-012345i", 0D, default),
        new("+0e-0+0e-0i", 0D, default),
        new("0e+0+0e+0i", 0D, default),
        new("-0e+0-0e+0i", 0D, default),

        // Regular non-zeroes
        new("0.1", 0.1D, default),
        new("0.1i", 0.1D.i(), default),
        new("0.123", 0.123D, default),
        new("0.123i", 0.123D.i(), default),
        new("0.123+0.123i", 0.123D + 0.123D.i(), default),
        new("99", 99D, default),
        new("+99", 99D, default),
        new("-99", -99D, default),
        new("+1i", 1D.i(), default),
        new("-1i", -1D.i(), default),
        new("+3+1i", 3D + 1D.i(), default),
        new("30+3i", 30D + 3D.i(), default),
        new("+3e+3-3e+3i", 3000D + -3000D.i(), default),
        new("+3e+3+3e+3i", 3000D + 3000D.i(), default),
        new("+3e+3+3e+3i+", 0D, ErrSyntax),

        // Separators
        new("0.1", 0.1D, default),
        new("0.1i", 0.1D.i(), default),
        new("0.1_2_3", 0.123D, default),
        new("+0x_3p3i", 24D.i(), default),
        new("0_0+0x_0p0i", 0D, default),
        new("0x_10.3p-8+0x3p3i", 0.063232421875D + 24D.i(), default),
        new("+0x_1_0.3p-8+0x_3_0p3i", 0.063232421875D + 384D.i(), default),
        new("0x1_0.3p+8-0x_3p3i", 4144D + -24D.i(), default),

        // Hexadecimals
        new("0x10.3p-8+0x3p3i", 0.063232421875D + 24D.i(), default),
        new("+0x10.3p-8+0x3p3i", 0.063232421875D + 24D.i(), default),
        new("0x10.3p+8-0x3p3i", 4144D + -24D.i(), default),
        new("0x1p0", 1D, default),
        new("0x1p1", 2D, default),
        new("0x1p-1", 0.5D, default),
        new("0x1ep-1", 15D, default),
        new("-0x1ep-1", -15D, default),
        new("-0x2p3", -16D, default),
        new("0x1e2", 0D, ErrSyntax),
        new("1p2", 0D, ErrSyntax),
        new("0x1e2i", 0D, ErrSyntax),

        // ErrRange
        // next float64 - too large
        new("+0x1p1024", infp0, ErrRange),
        new("-0x1p1024", infm0, ErrRange),
        new("+0x1p1024i", inf0p, ErrRange),
        new("-0x1p1024i", inf0m, ErrRange),
        new("+0x1p1024+0x1p1024i", infpp, ErrRange),
        new("+0x1p1024-0x1p1024i", infpm, ErrRange),
        new("-0x1p1024+0x1p1024i", infmp, ErrRange),
        new("-0x1p1024-0x1p1024i", infmm, ErrRange),
        // the border is ...158079
        // borderline - okay
        new("+0x1.fffffffffffff7fffp1023+0x1.fffffffffffff7fffp1023i", 1.7976931348623157e+308D + 1.7976931348623157e+308D.i(), default),
        new("+0x1.fffffffffffff7fffp1023-0x1.fffffffffffff7fffp1023i", 1.7976931348623157e+308D + -1.7976931348623157e+308D.i(), default),
        new("-0x1.fffffffffffff7fffp1023+0x1.fffffffffffff7fffp1023i", -1.7976931348623157e+308D + 1.7976931348623157e+308D.i(), default),
        new("-0x1.fffffffffffff7fffp1023-0x1.fffffffffffff7fffp1023i", -1.7976931348623157e+308D + -1.7976931348623157e+308D.i(), default),
        // borderline - too large
        new("+0x1.fffffffffffff8p1023", infp0, ErrRange),
        new("-0x1fffffffffffff.8p+971", infm0, ErrRange),
        new("+0x1.fffffffffffff8p1023i", inf0p, ErrRange),
        new("-0x1fffffffffffff.8p+971i", inf0m, ErrRange),
        new("+0x1.fffffffffffff8p1023+0x1.fffffffffffff8p1023i", infpp, ErrRange),
        new("+0x1.fffffffffffff8p1023-0x1.fffffffffffff8p1023i", infpm, ErrRange),
        new("-0x1fffffffffffff.8p+971+0x1fffffffffffff.8p+971i", infmp, ErrRange),
        new("-0x1fffffffffffff8p+967-0x1fffffffffffff8p+967i", infmm, ErrRange),
        // a little too large
        new("1e308+1e308i", 1e+308D + 1e+308D.i(), default),
        new("2e308+2e308i", infpp, ErrRange),
        new("1e309+1e309i", infpp, ErrRange),
        new("0x1p1025+0x1p1025i", infpp, ErrRange),
        new("2e308", infp0, ErrRange),
        new("1e309", infp0, ErrRange),
        new("0x1p1025", infp0, ErrRange),
        new("2e308i", inf0p, ErrRange),
        new("1e309i", inf0p, ErrRange),
        new("0x1p1025i", inf0p, ErrRange),
        // way too large
        new("+1e310+1e310i", infpp, ErrRange),
        new("+1e310-1e310i", infpm, ErrRange),
        new("-1e310+1e310i", infmp, ErrRange),
        new("-1e310-1e310i", infmm, ErrRange),
        // under/overflow exponent
        new("1e-4294967296", 0D, default),
        new("1e-4294967296i", 0D, default),
        new("1e-4294967296+1i", 1D.i(), default),
        new("1+1e-4294967296i", 1D, default),
        new("1e-4294967296+1e-4294967296i", 0D, default),
        new("1e+4294967296", infp0, ErrRange),
        new("1e+4294967296i", inf0p, ErrRange),
        new("1e+4294967296+1e+4294967296i", infpp, ErrRange),
        new("1e+4294967296-1e+4294967296i", infpm, ErrRange)
    ]);
    foreach (var i in range(len(tests))) {
        var test = Ꮡ(tests, i);
        if (test.err != nil) {
            test.err = new strconv.NumErrorжerror(Ꮡ(new NumError(Func: "ParseComplex", Num: test.@in, Err: test.err)));
        }
        var (got, err) = ParseComplex(test.@in, 128);
        if (!reflect.DeepEqual(err, test.err)) {
            t.Fatalf("ParseComplex(%q, 128) = %v, %v; want %v, %v", test.@in, got, err, test.@out, test.err);
        }
        if (!(cmplx.IsNaN(test.@out) && cmplx.IsNaN(got)) && got != test.@out) {
            t.Fatalf("ParseComplex(%q, 128) = %v, %v; want %v, %v", test.@in, got, err, test.@out, test.err);
        }

        if ((complex128)(complex64)test.@out == test.@out) {
            var (gotΔ1, errΔ1) = ParseComplex(test.@in, 64);
            if (!reflect.DeepEqual(errΔ1, test.err)) {
                t.Fatalf("ParseComplex(%q, 64) = %v, %v; want %v, %v", test.@in, gotΔ1, errΔ1, test.@out, test.err);
            }
            var got64 = (complex64)gotΔ1;
            if ((complex128)got64 != test.@out) {
                t.Fatalf("ParseComplex(%q, 64) = %v, %v; want %v, %v", test.@in, gotΔ1, errΔ1, test.@out, test.err);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string sᶜ => ref sᶜᵇ;
private static readonly @string sᶜᵇ = "1.5e308+1.0e307i"u8;

// Issue 42297: allow ParseComplex(s, not_32_or_64) for legacy reasons
public static void TestParseComplexIncorrectBitSize(ж<testing.T> t) {
    @string s = sᶜ;
    complex128 want = /* 1.5e308 + 1.0e307i */ 1.5e+308D + 1e+307D.i();

    foreach (var (_, bitSize) in range(slice<nint>([0, 10, 100, 256]))) {
        var (c, err) = ParseComplex(s, bitSize);
        if (err != nil) {
            t.Fatalf("ParseComplex(%q, %d) gave error %s", s, bitSize, err);
        }
        if (c != want) {
            t.Fatalf("ParseComplex(%q, %d) = %g (expected %g)", s, bitSize, c, want);
        }
    }
}

partial struct atofTest {
    internal @string @in;
    internal @string @out;
    internal error err;
}

internal static readonly ж<slice<atofTest>> Ꮡatoftests = new StaticBox<slice<atofTest>>([
    new("", "0", ErrSyntax),
    new("1.25", "1.25", default),
    new("+1", "1", default),
    new("1x", "0", ErrSyntax),
    new("1.1.", "0", ErrSyntax),
    new("1e23", "1e+23", default),
    new("1E23", "1e+23", default),
    new("0x1fFe2.p0", "131042", default),
    new("0x1fFe2.P0", "131042", default),
    new("-0x2p3", "-16", default),
    new("0x0.fp4", "15", default),
    new("0x0.fp0", "0.9375", default),
    new("0x1e2", "0", ErrSyntax),
    new("1p2", "0", ErrSyntax),
    new("0x1p1024", "+Inf", ErrRange),
    new("-0x1p1024", "-Inf", ErrRange),
    new("0x1.fffffffffffff7fffp1023", "1.7976931348623157e+308", default),
    new("-0x1.fffffffffffff7fffp1023", "-1.7976931348623157e+308", default),
    new("1.797693134862315808e308", "+Inf", ErrRange),
    new("-1.797693134862315808e308", "-Inf", ErrRange)
]);
internal static ref slice<atofTest> atoftests => ref ((StaticBox<slice<atofTest>>)Ꮡatoftests).ValueSlot;

[GoInit] internal static void init() {
    // The atof routines return NumErrors wrapping
    // the error and the string. Convert the table above.
    foreach (var i in range(len(atoftests))) {
        var test = Ꮡ(atoftests, i);
        if (test.err != nil) {
            test.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseFloat", test.@in, test.err)));
        }
    }
}

public static void TestAtof(ж<testing.T> t) {
    for (nint i = 0; i < len(atoftests); i++) {
        var test = Ꮡ(atoftests, i);
        var (@out, err) = ParseFloat(test.@in, 64);
        @string outs = FormatFloat(@out, (byte)'g', -1, 64);
        if (outs != test.@out || !reflect.DeepEqual(err, test.err)) {
            t.Errorf("ParseFloat(%v, 64) = %v, %v want %v, %v",
                test.@in, @out, err, test.@out, test.err);
        }

        if ((float64)(float32)@out == @out) {
            var (outΔ1, errΔ1) = ParseFloat(test.@in, 32);
            var out32 = (float32)outΔ1;
            if ((float64)out32 != outΔ1) {
                t.Errorf("ParseFloat(%v, 32) = %v, not a float32 (closest is %v)", test.@in, outΔ1, (float64)out32);
                continue;
            }
            @string outsΔ1 = FormatFloat((float64)out32, (byte)'g', -1, 32);
            if (outsΔ1 != test.@out || !reflect.DeepEqual(errΔ1, test.err)) {
                t.Errorf("ParseFloat(%v, 32) = %v, %v want %v, %v  # %v",
                    test.@in, out32, errΔ1, test.@out, test.err, outΔ1);
            }
        }
    }
}

partial struct parseUint64Test {
    internal @string @in;
    internal uint64 @out;
    internal error err;
}

internal static readonly ж<slice<parseUint64Test>> ᏑparseUint64Tests = new StaticBox<slice<parseUint64Test>>([
    new("", 0, ErrSyntax),
    new("0", 0, default),
    new("1", 1, default),
    new("12345", 12345, default),
    new("012345", 12345, default),
    new("18446744073709551616", 18446744073709551615UL, ErrRange),
    new("-1", 0, ErrSyntax)
]);
internal static ref slice<parseUint64Test> parseUint64Tests => ref ((StaticBox<slice<parseUint64Test>>)ᏑparseUint64Tests).ValueSlot;

partial struct parseUint64BaseTest {
    internal @string @in;
    internal nint @base;
    internal uint64 @out;
    internal error err;
}

internal static readonly ж<slice<parseUint64BaseTest>> ᏑparseUint64BaseTests = new StaticBox<slice<parseUint64BaseTest>>([
    new("", 0, 0, ErrSyntax),
    new("0", 0, 0, default),
    new("1", 0, 1, default),
    new("-1", 0, 0, ErrSyntax),
    new("12345", 0, 12345, default),
    new("012345", 0, 5349, default),
    new("18446744073709551616", 0, 18446744073709551615UL, ErrRange),
    new("0b", 0, 0, ErrSyntax),
    new("101", 2, 5, default),
    new("101_", 2, 0, ErrSyntax)
]);
internal static ref slice<parseUint64BaseTest> parseUint64BaseTests => ref ((StaticBox<slice<parseUint64BaseTest>>)ᏑparseUint64BaseTests).ValueSlot;

partial struct parseInt64Test {
    internal @string @in;
    internal int64 @out;
    internal error err;
}

internal static readonly ж<slice<parseInt64Test>> ᏑparseInt64Tests = new StaticBox<slice<parseInt64Test>>([
    new("", 0, ErrSyntax),
    new("0", 0, default),
    new("1", 1, default),
    new("-1", -1, default),
    new("12345", 12345, default),
    new("9223372036854775808", 9223372036854775807L, ErrRange),
    new("123%45", 0, ErrSyntax)
]);
internal static ref slice<parseInt64Test> parseInt64Tests => ref ((StaticBox<slice<parseInt64Test>>)ᏑparseInt64Tests).ValueSlot;

partial struct parseInt64BaseTest {
    internal @string @in;
    internal nint @base;
    internal int64 @out;
    internal error err;
}

internal static readonly ж<slice<parseInt64BaseTest>> ᏑparseInt64BaseTests = new StaticBox<slice<parseInt64BaseTest>>([
    new("", 0, 0, ErrSyntax),
    new("0", 0, 0, default),
    new("1", 0, 1, default),
    new("-1", 0, -1, default),
    new("12345", 0, 12345, default),
    new("12345", 9, 8303, default),
    new("012345", 0, 5349, default),
    new("9223372036854775808", 10, 9223372036854775807L, ErrRange),
    new("0b", 0, 0, ErrSyntax),
    new("101", 2, 5, default),
    new("101_", 2, 0, ErrSyntax)
]);
internal static ref slice<parseInt64BaseTest> parseInt64BaseTests => ref ((StaticBox<slice<parseInt64BaseTest>>)ᏑparseInt64BaseTests).ValueSlot;

partial struct parseUint32Test {
    internal @string @in;
    internal uint32 @out;
    internal error err;
}

internal static readonly ж<slice<parseUint32Test>> ᏑparseUint32Tests = new StaticBox<slice<parseUint32Test>>([
    new("", 0, ErrSyntax),
    new("0", 0, default),
    new("1", 1, default),
    new("12345", 12345, default),
    new("12345x", 0, ErrSyntax),
    new("987654321", 987654321, default),
    new("4294967296", unchecked((uint32)(4294967295UL)), ErrRange),
    new("1_2_3_4_5", 0, ErrSyntax), // base=10 so no underscores allowed
    new("12345_", 0, ErrSyntax)
]);
internal static ref slice<parseUint32Test> parseUint32Tests => ref ((StaticBox<slice<parseUint32Test>>)ᏑparseUint32Tests).ValueSlot;

partial struct parseInt32Test {
    internal @string @in;
    internal int32 @out;
    internal error err;
}

internal static readonly ж<slice<parseInt32Test>> ᏑparseInt32Tests = new StaticBox<slice<parseInt32Test>>([
    new("", 0, ErrSyntax),
    new("0", 0, default),
    new("-0", 0, default),
    new("1", 1, default),
    new("-1", -1, default),
    new("12345", 12345, default),
    new("-12345", -12345, default),
    new("2147483648", (int32)(2147483648L - 1), ErrRange),
    new("12345_", 0, ErrSyntax)
]);
internal static ref slice<parseInt32Test> parseInt32Tests => ref ((StaticBox<slice<parseInt32Test>>)ᏑparseInt32Tests).ValueSlot;

partial struct numErrorTest {
    internal @string num, want;
}

internal static slice<numErrorTest> numErrorTests = [
    new("0", @"strconv.ParseFloat: parsing ""0"": failed"),
    new("`", "strconv.ParseFloat: parsing \"`\": failed"),
    new("1\x00.2", @"strconv.ParseFloat: parsing ""1\x00.2"": failed")
];

[GoInit] internal static void initΔ1() {
    // The parse routines return NumErrors wrapping
    // the error and the string. Convert the tables above.
    foreach (var i in range(len(parseUint64Tests))) {
        var test = Ꮡ(parseUint64Tests, i);
        if (test.err != nil) {
            test.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseUint", test.@in, test.err)));
        }
    }
    foreach (var i in range(len(parseUint64BaseTests))) {
        var test = Ꮡ(parseUint64BaseTests, i);
        if (test.err != nil) {
            test.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseUint", test.@in, test.err)));
        }
    }
    foreach (var i in range(len(parseInt64Tests))) {
        var test = Ꮡ(parseInt64Tests, i);
        if (test.err != nil) {
            test.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseInt", test.@in, test.err)));
        }
    }
    foreach (var i in range(len(parseInt64BaseTests))) {
        var test = Ꮡ(parseInt64BaseTests, i);
        if (test.err != nil) {
            test.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseInt", test.@in, test.err)));
        }
    }
    foreach (var i in range(len(parseUint32Tests))) {
        var test = Ꮡ(parseUint32Tests, i);
        if (test.err != nil) {
            test.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseUint", test.@in, test.err)));
        }
    }
    foreach (var i in range(len(parseInt32Tests))) {
        var test = Ꮡ(parseInt32Tests, i);
        if (test.err != nil) {
            test.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseInt", test.@in, test.err)));
        }
    }
}

public static void TestParseUint32(ж<testing.T> t) {
    foreach (var i in range(len(parseUint32Tests))) {
        var test = Ꮡ(parseUint32Tests, i);
        var (@out, err) = ParseUint(test.@in, 10, 32);
        if ((uint64)test.@out != @out || !reflect.DeepEqual(test.err, err)) {
            t.Errorf("ParseUint(%q, 10, 32) = %v, %v want %v, %v",
                test.@in, @out, err, test.@out, test.err);
        }
    }
}

public static void TestParseUint64(ж<testing.T> t) {
    foreach (var i in range(len(parseUint64Tests))) {
        var test = Ꮡ(parseUint64Tests, i);
        var (@out, err) = ParseUint(test.@in, 10, 64);
        if (test.@out != @out || !reflect.DeepEqual(test.err, err)) {
            t.Errorf("ParseUint(%q, 10, 64) = %v, %v want %v, %v",
                test.@in, @out, err, test.@out, test.err);
        }
    }
}

public static void TestParseUint64Base(ж<testing.T> t) {
    foreach (var i in range(len(parseUint64BaseTests))) {
        var test = Ꮡ(parseUint64BaseTests, i);
        var (@out, err) = ParseUint(test.@in, test.@base, 64);
        if (test.@out != @out || !reflect.DeepEqual(test.err, err)) {
            t.Errorf("ParseUint(%q, %v, 64) = %v, %v want %v, %v",
                test.@in, test.@base, @out, err, test.@out, test.err);
        }
    }
}

public static void TestParseInt32(ж<testing.T> t) {
    foreach (var i in range(len(parseInt32Tests))) {
        var test = Ꮡ(parseInt32Tests, i);
        var (@out, err) = ParseInt(test.@in, 10, 32);
        if ((int64)test.@out != @out || !reflect.DeepEqual(test.err, err)) {
            t.Errorf("ParseInt(%q, 10 ,32) = %v, %v want %v, %v",
                test.@in, @out, err, test.@out, test.err);
        }
    }
}

public static void TestParseInt64(ж<testing.T> t) {
    foreach (var i in range(len(parseInt64Tests))) {
        var test = Ꮡ(parseInt64Tests, i);
        var (@out, err) = ParseInt(test.@in, 10, 64);
        if (test.@out != @out || !reflect.DeepEqual(test.err, err)) {
            t.Errorf("ParseInt(%q, 10, 64) = %v, %v want %v, %v",
                test.@in, @out, err, test.@out, test.err);
        }
    }
}

public static void TestParseInt64Base(ж<testing.T> t) {
    foreach (var i in range(len(parseInt64BaseTests))) {
        var test = Ꮡ(parseInt64BaseTests, i);
        var (@out, err) = ParseInt(test.@in, test.@base, 64);
        if (test.@out != @out || !reflect.DeepEqual(test.err, err)) {
            t.Errorf("ParseInt(%q, %v, 64) = %v, %v want %v, %v",
                test.@in, test.@base, @out, err, test.@out, test.err);
        }
    }
}

public static void TestParseUint(ж<testing.T> t) {
    switch (IntSize) {
    case var ᴛ when ᴛ == 32: {
        foreach (var i in range(len(parseUint32Tests))) {
            var test = Ꮡ(parseUint32Tests, i);
            var (@out, err) = ParseUint(test.@in, 10, 0);
            if ((uint64)test.@out != @out || !reflect.DeepEqual(test.err, err)) {
                t.Errorf("ParseUint(%q, 10, 0) = %v, %v want %v, %v",
                    test.@in, @out, err, test.@out, test.err);
            }
        }
        break;
    }
    case var ᴛ when ᴛ == 64: {
        foreach (var i in range(len(parseUint64Tests))) {
            var test = Ꮡ(parseUint64Tests, i);
            var (@out, err) = ParseUint(test.@in, 10, 0);
            if (test.@out != @out || !reflect.DeepEqual(test.err, err)) {
                t.Errorf("ParseUint(%q, 10, 0) = %v, %v want %v, %v",
                    test.@in, @out, err, test.@out, test.err);
            }
        }
        break;
    }}
}

public static void TestParseInt(ж<testing.T> t) {
    switch (IntSize) {
    case var ᴛ when ᴛ == 32: {
        foreach (var i in range(len(parseInt32Tests))) {
            var test = Ꮡ(parseInt32Tests, i);
            var (@out, err) = ParseInt(test.@in, 10, 0);
            if ((int64)test.@out != @out || !reflect.DeepEqual(test.err, err)) {
                t.Errorf("ParseInt(%q, 10, 0) = %v, %v want %v, %v",
                    test.@in, @out, err, test.@out, test.err);
            }
        }
        break;
    }
    case var ᴛ when ᴛ == 64: {
        foreach (var i in range(len(parseInt64Tests))) {
            var test = Ꮡ(parseInt64Tests, i);
            var (@out, err) = ParseInt(test.@in, 10, 0);
            if (test.@out != @out || !reflect.DeepEqual(test.err, err)) {
                t.Errorf("ParseInt(%q, 10, 0) = %v, %v want %v, %v",
                    test.@in, @out, err, test.@out, test.err);
            }
        }
        break;
    }}
}

public static void TestAtoi(ж<testing.T> t) {
    switch (IntSize) {
    case var ᴛ when ᴛ == 32: {
        foreach (var i in range(len(parseInt32Tests))) {
            var test = Ꮡ(parseInt32Tests, i);
            var (@out, err) = Atoi(test.@in);
            error testErr = default;
            if (test.err != nil) {
                testErr = new strconv.NumErrorжerror(Ꮡ(new NumError("Atoi", test.@in, test.err._<ж<strconv.NumError>>().Err)));
            }
            if ((nint)test.@out != @out || !reflect.DeepEqual(testErr, err)) {
                t.Errorf("Atoi(%q) = %v, %v want %v, %v",
                    test.@in, @out, err, test.@out, testErr);
            }
        }
        break;
    }
    case var ᴛ when ᴛ == 64: {
        foreach (var i in range(len(parseInt64Tests))) {
            var test = Ꮡ(parseInt64Tests, i);
            var (@out, err) = Atoi(test.@in);
            error testErr = default;
            if (test.err != nil) {
                testErr = new strconv.NumErrorжerror(Ꮡ(new NumError("Atoi", test.@in, test.err._<ж<strconv.NumError>>().Err)));
            }
            if (test.@out != (int64)@out || !reflect.DeepEqual(testErr, err)) {
                t.Errorf("Atoi(%q) = %v, %v want %v, %v",
                    test.@in, @out, err, test.@out, testErr);
            }
        }
        break;
    }}
}

internal static error bitSizeErrStub(@string name, nint bitSize) {
    return new strconv.NumErrorжerror(bitSizeError(name, "0", bitSize));
}

internal static error baseErrStub(@string name, nint @base) {
    return new strconv.NumErrorжerror(baseError(name, "0", @base));
}

internal static error noErrStub(@string name, nint arg) {
    return default;
}

partial struct parseErrorTest {
    internal nint arg;
    internal Func<@string, nint, error> errStub;
}

internal static readonly ж<slice<parseErrorTest>> ᏑparseBitSizeTests = new StaticBox<slice<parseErrorTest>>(default(slice<parseErrorTest>));
internal static ref slice<parseErrorTest> parseBitSizeTests => ref ((StaticBox<slice<parseErrorTest>>)ᏑparseBitSizeTests).ValueSlot;
internal static void initᴛparseBitSizeTests() { parseBitSizeTests = [
    new(-1, bitSizeErrStub),
    new(0, noErrStub),
    new(64, noErrStub),
    new(65, bitSizeErrStub)
]; }

internal static readonly ж<slice<parseErrorTest>> ᏑparseBaseTests = new StaticBox<slice<parseErrorTest>>(default(slice<parseErrorTest>));
internal static ref slice<parseErrorTest> parseBaseTests => ref ((StaticBox<slice<parseErrorTest>>)ᏑparseBaseTests).ValueSlot;
internal static void initᴛparseBaseTests() { parseBaseTests = [
    new(-1, baseErrStub),
    new(0, noErrStub),
    new(1, baseErrStub),
    new(2, noErrStub),
    new(36, noErrStub),
    new(37, baseErrStub)
]; }

internal static bool equalError(error a, error b) {
    if (a == nil) {
        return b == nil;
    }
    if (b == nil) {
        return a == nil;
    }
    return a.Error() == b.Error();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string parseIntˢ => ref parseIntˢᵇ;
private static readonly @string parseIntˢᵇ = "ParseInt"u8;

public static void TestParseIntBitSize(ж<testing.T> t) {
    foreach (var i in range(len(parseBitSizeTests))) {
        var test = Ꮡ(parseBitSizeTests, i);
        var testErr = test.errStub(parseIntˢ, test.arg);
        var (_, err) = ParseInt("0", 0, test.arg);
        if (!equalError(testErr, err)) {
            t.Errorf("ParseInt(\"0\", 0, %v) = 0, %v want 0, %v",
                test.arg, err, testErr);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string parseUintˢ => ref parseUintˢᵇ;
private static readonly @string parseUintˢᵇ = "ParseUint"u8;

public static void TestParseUintBitSize(ж<testing.T> t) {
    foreach (var i in range(len(parseBitSizeTests))) {
        var test = Ꮡ(parseBitSizeTests, i);
        var testErr = test.errStub(parseUintˢ, test.arg);
        var (_, err) = ParseUint("0", 0, test.arg);
        if (!equalError(testErr, err)) {
            t.Errorf("ParseUint(\"0\", 0, %v) = 0, %v want 0, %v",
                test.arg, err, testErr);
        }
    }
}

public static void TestParseIntBase(ж<testing.T> t) {
    foreach (var i in range(len(parseBaseTests))) {
        var test = Ꮡ(parseBaseTests, i);
        var testErr = test.errStub(parseIntˢ, test.arg);
        var (_, err) = ParseInt("0", test.arg, 0);
        if (!equalError(testErr, err)) {
            t.Errorf("ParseInt(\"0\", %v, 0) = 0, %v want 0, %v",
                test.arg, err, testErr);
        }
    }
}

public static void TestParseUintBase(ж<testing.T> t) {
    foreach (var i in range(len(parseBaseTests))) {
        var test = Ꮡ(parseBaseTests, i);
        var testErr = test.errStub(parseUintˢ, test.arg);
        var (_, err) = ParseUint("0", test.arg, 0);
        if (!equalError(testErr, err)) {
            t.Errorf("ParseUint(\"0\", %v, 0) = 0, %v want 0, %v",
                test.arg, err, testErr);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string failedˢ => ref failedˢᵇ;
private static readonly @string failedˢᵇ = "failed"u8;

public static void TestNumError(ж<testing.T> t) {
    foreach (var (_, test) in range(numErrorTests)) {
        var err = Ꮡ(new NumError(
            Func: "ParseFloat",
            Num: test.num,
            Err: errors.New(failedˢ)
        ));
        @string got = err.Error(); if (got != test.want) {
            t.Errorf(@"(&NumError{""ParseFloat"", %q, ""failed""}).Error() = %v, want %v", test.num, got, test.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorsIsFailedWantedˢ = (@string)"errors.Is failed, wanted success"u8;

public static partial void TestNumErrorUnwrap(ж<testing.T> t) {
    var err = Ꮡ(new NumError(Err: ErrSyntax));
    if (!errors.Is(new strconv.NumErrorжerror(err), ErrSyntax)) {
        t.Error(errorsIsFailedWantedˢ);
    }
}

internal partial struct TestFormatComplex_tests /*dyn*/ {
    internal complex128 c;
    internal byte fmt;
    internal nint prec;
    internal nint bitSize;
    internal @string @out;
}

public static void TestFormatComplex(ж<testing.T> t) {
    var tests = slice<TestFormatComplex_tests>([
        // a variety of signs
        new(1D + 2D.i(), (byte)'g', -1, 128, "(1+2i)"),
        new(3D + -4D.i(), (byte)'g', -1, 128, "(3-4i)"),
        new(-5D + 6D.i(), (byte)'g', -1, 128, "(-5+6i)"),
        new(-7D + -8D.i(), (byte)'g', -1, 128, "(-7-8i)"),

        // test that fmt and prec are working
        new(3.14159D + 0.00123D.i(), (byte)'e', 3, 128, "(3.142e+00+1.230e-03i)"),
        new(3.14159D + 0.00123D.i(), (byte)'f', 3, 128, "(3.142+0.001i)"),
        new(3.14159D + 0.00123D.i(), (byte)'g', 3, 128, "(3.14+0.00123i)"),

        // ensure bitSize rounding is working
        new(1.2345678901234567D + 9.876543210987654D.i(), (byte)'f', -1, 128, "(1.2345678901234567+9.876543210987654i)"),
        new(1.2345678901234567D + 9.876543210987654D.i(), (byte)'f', -1, 64, "(1.2345679+9.876543i)")

        // other cases are handled by FormatFloat tests
    ]);
    foreach (var (_, test) in range(tests)) {
        @string @out = FormatComplex(test.c, test.fmt, test.prec, test.bitSize);
        if (@out != test.@out) {
            t.Fatalf("FormatComplex(%v, %q, %d, %d) = %q; want %q",
                test.c, test.fmt, test.prec, test.bitSize, @out, test.@out);
        }
    }
}

public static void TestFormatComplexInvalidBitSize(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover(); if (r == nil) {
                t.Fatalf("expected panic due to invalid bitSize");
            }
        }, ref ᒐ);
        _ = FormatComplex(1D + 2D.i(), (byte)'g', -1, 100);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct itob64Test {
    internal int64 @in;
    internal nint @base;
    internal @string @out;
}

internal static slice<itob64Test> itob64tests = [
    new(0, 10, "0"),
    new(1, 10, "1"),
    new(-1, 10, "-1"),
    new(12345678, 10, "12345678"),
    new(-9223372036854775808L, 10, "-9223372036854775808"),
    new(16, 17, "g"),
    new(25, 25, "10"),
    new(38493362624L, 36, "holycow")
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object abcˢ = (@string)"abc"u8;

public static void TestItoa(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, test) in range(itob64tests)) {
            @string s = FormatInt(test.@in, test.@base);
            if (s != test.@out) {
                t.Errorf("FormatInt(%v, %v) = %v want %v",
                    test.@in, test.@base, s, test.@out);
            }
            var x = AppendInt(slice<byte>("abc"u8), test.@in, test.@base);
            if (((@string)x) != "abc"u8 + test.@out) {
                t.Errorf("AppendInt(%q, %v, %v) = %q want %v",
                    abcˢ, test.@in, test.@base, x, test.@out);
            }

            if (test.@in >= 0) {
                @string sΔ1 = FormatUint((uint64)test.@in, test.@base);
                if (sΔ1 != test.@out) {
                    t.Errorf("FormatUint(%v, %v) = %v want %v",
                        test.@in, test.@base, sΔ1, test.@out);
                }
                var xΔ1 = AppendUint(default, (uint64)test.@in, test.@base);
                if (((sstring)xΔ1) != test.@out) {
                    t.Errorf("AppendUint(%q, %v, %v) = %q want %v",
                        abcˢ, (uint64)test.@in, test.@base, xΔ1, test.@out);
                }
            }

            if (test.@base == 10 && (int64)(nint)test.@in == test.@in) {
                @string sΔ2 = Itoa((nint)test.@in);
                if (sΔ2 != test.@out) {
                    t.Errorf("Itoa(%v) = %v want %v",
                        test.@in, sΔ2, test.@out);
                }
            }
        }

        // Override when base is illegal
        defer(() => {
            var r = recover(); if (r == nil) {
                t.Fatalf("expected panic due to illegal base");
            }
        }, ref ᒐ);
        FormatUint(12345678, 1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct uitob64Test {
    internal uint64 @in;
    internal nint @base;
    internal @string @out;
}

internal static slice<uitob64Test> uitob64tests = [
    new(9223372036854775807UL, 10, "9223372036854775807"),
    new((uint64)1 << 63, 10, "9223372036854775808"),
    new(9223372036854775809UL, 10, "9223372036854775809"),
    new(18446744073709551614UL, 10, "18446744073709551614"),
    new(18446744073709551615UL, 10, "18446744073709551615"),
    new(18446744073709551615UL, 2, "1111111111111111111111111111111111111111111111111111111111111111")
];

public static void TestUitoa(ж<testing.T> t) {
    foreach (var (_, test) in range(uitob64tests)) {
        @string s = FormatUint(test.@in, test.@base);
        if (s != test.@out) {
            t.Errorf("FormatUint(%v, %v) = %v want %v",
                test.@in, test.@base, s, test.@out);
        }
        var x = AppendUint(slice<byte>("abc"u8), test.@in, test.@base);
        if (((@string)x) != "abc"u8 + test.@out) {
            t.Errorf("AppendUint(%q, %v, %v) = %q want %v",
                abcˢ, test.@in, test.@base, x, test.@out);
        }
    }
}

partial struct varlenUintsᴛ1 /*dyn*/ {
    internal uint64 @in;
    internal @string @out;
}
internal static slice<varlenUintsᴛ1> varlenUints = [
    new(1, "1"),
    new(12, "12"),
    new(123, "123"),
    new(1234, "1234"),
    new(12345, "12345"),
    new(123456, "123456"),
    new(1234567, "1234567"),
    new(12345678, "12345678"),
    new(123456789, "123456789"),
    new(1234567890, "1234567890"),
    new(12345678901UL, "12345678901"),
    new(123456789012UL, "123456789012"),
    new(1234567890123UL, "1234567890123"),
    new(12345678901234UL, "12345678901234"),
    new(123456789012345UL, "123456789012345"),
    new(1234567890123456UL, "1234567890123456"),
    new(12345678901234567UL, "12345678901234567"),
    new(123456789012345678UL, "123456789012345678"),
    new(1234567890123456789UL, "1234567890123456789"),
    new(12345678901234567890UL, "12345678901234567890")
];

public static void TestFormatUintVarlen(ж<testing.T> t) {
    foreach (var (_, test) in range(varlenUints)) {
        @string s = FormatUint(test.@in, 10);
        if (s != test.@out) {
            t.Errorf("FormatUint(%v, 10) = %v want %v", test.@in, s, test.@out);
        }
    }
}

} // end strconv_test_package
