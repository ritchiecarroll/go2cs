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
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.math;
using static go.strconv_internal_test_package;
using strconv = strconv_package;

partial class strconv_test_package {

partial struct atobTest {
    internal @string @in;
    internal bool @out;
    internal error err;
}

internal static slice<atobTest> atobtests = new atobTest[]{
    new(""u8, false, ErrSyntax),
    new("asdf"u8, false, ErrSyntax),
    new("0"u8, false, default!),
    new("false"u8, false, default!),
    new("true"u8, true, default!)
}.slice();

public static void TestParseBool(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in atobtests) {
        var (b, e) = ParseBool(test.@in);
        if (test.err != default!){
            // expect an error
            if (e == default!){
                Ꮡt.Errorf("ParseBool(%s) = nil; want %s"u8, test.@in, test.err);
            } else {
                // NumError assertion must succeed; it's the only thing we return.
                if (!AreEqual((~e._<ж<strconv.NumError>>()).Err, test.err)) {
                    Ꮡt.Errorf("ParseBool(%s) = %s; want %s"u8, test.@in, e, test.err);
                }
            }
        } else {
            if (e != default!) {
                Ꮡt.Errorf("ParseBool(%s) = %s; want nil"u8, test.@in, e);
            }
            if (b != test.@out) {
                Ꮡt.Errorf("ParseBool(%s) = %t; want %t"u8, test.@in, b, test.@out);
            }
        }
    }
}

internal static map<bool, @string> boolString = new map<bool, @string>{
    [true] = "true"u8,
    [false] = "false"u8
};

public static void TestFormatBool(ж<testing.T> Ꮡt) {
    foreach (var (b, s) in boolString) {
        {
            @string f = FormatBool(b); if (f != s) {
                Ꮡt.Errorf("FormatBool(%v) = %q; want %q"u8, b, f, s);
            }
        }
    }
}

partial struct appendBoolTest {
    internal bool b;
    internal slice<byte> @in;
    internal slice<byte> @out;
}

internal static slice<appendBoolTest> appendBoolTests = new appendBoolTest[]{
    new(true, slice<byte>("foo "u8), slice<byte>("foo true"u8)),
    new(false, slice<byte>("foo "u8), slice<byte>("foo false"u8))
}.slice();

public static void TestAppendBool(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in appendBoolTests) {
        var b = AppendBool(test.@in, test.b);
        if (!bytes.Equal(b, test.@out)) {
            Ꮡt.Errorf("AppendBool(%q, %v) = %q; want %q"u8, test.@in, test.b, b, test.@out);
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

public static void TestParseComplex(ж<testing.T> Ꮡt) {
    var tests = new atocTest[]{
        // Clearly invalid
        new(""u8, 0D, ErrSyntax),
        new(" "u8, 0D, ErrSyntax),
        new("("u8, 0D, ErrSyntax),
        new(")"u8, 0D, ErrSyntax),
        new("i"u8, 0D, ErrSyntax),
        new("+i"u8, 0D, ErrSyntax),
        new("-i"u8, 0D, ErrSyntax),
        new("1I"u8, 0D, ErrSyntax),
        new("10  + 5i"u8, 0D, ErrSyntax),
        new("3+"u8, 0D, ErrSyntax),
        new("3+5"u8, 0D, ErrSyntax),
        new("3+5+5i"u8, 0D, ErrSyntax),

        // Parentheses
        new("()"u8, 0D, ErrSyntax),
        new("(i)"u8, 0D, ErrSyntax),
        new("(0)"u8, 0D, default!),
        new("(1i)"u8, 1D.i(), default!),
        new("(3.0+5.5i)"u8, 3D + 5.5D.i(), default!),
        new("(1)+1i"u8, 0D, ErrSyntax),
        new("(3.0+5.5i"u8, 0D, ErrSyntax),
        new("3.0+5.5i)"u8, 0D, ErrSyntax),

        // NaNs
        new("NaN"u8, complex(Δmath.NaN(), 0D), default!),
        new("NANi"u8, complex(0D, Δmath.NaN()), default!),
        new("nan+nAni"u8, complex(Δmath.NaN(), Δmath.NaN()), default!),
        new("+NaN"u8, 0D, ErrSyntax),
        new("-NaN"u8, 0D, ErrSyntax),
        new("NaN-NaNi"u8, 0D, ErrSyntax),

        // Infs
        new("Inf"u8, infp0, default!),
        new("+inf"u8, infp0, default!),
        new("-inf"u8, infm0, default!),
        new("Infinity"u8, infp0, default!),
        new("+INFINITY"u8, infp0, default!),
        new("-infinity"u8, infm0, default!),
        new("+infi"u8, inf0p, default!),
        new("0-infinityi"u8, inf0m, default!),
        new("Inf+Infi"u8, infpp, default!),
        new("+Inf-Infi"u8, infpm, default!),
        new("-Infinity+Infi"u8, infmp, default!),
        new("inf-inf"u8, 0D, ErrSyntax),

        // Zeros
        new("0"u8, 0D, default!),
        new("0i"u8, 0D, default!),
        new("-0.0i"u8, 0D, default!),
        new("0+0.0i"u8, 0D, default!),
        new("0e+0i"u8, 0D, default!),
        new("0e-0+0i"u8, 0D, default!),
        new("-0.0-0.0i"u8, 0D, default!),
        new("0e+012345"u8, 0D, default!),
        new("0x0p+012345i"u8, 0D, default!),
        new("0x0.00p-012345i"u8, 0D, default!),
        new("+0e-0+0e-0i"u8, 0D, default!),
        new("0e+0+0e+0i"u8, 0D, default!),
        new("-0e+0-0e+0i"u8, 0D, default!),

        // Regular non-zeroes
        new("0.1"u8, 0.1D, default!),
        new("0.1i"u8, 0.1D.i(), default!),
        new("0.123"u8, 0.123D, default!),
        new("0.123i"u8, 0.123D.i(), default!),
        new("0.123+0.123i"u8, 0.123D + 0.123D.i(), default!),
        new("99"u8, 99D, default!),
        new("+99"u8, 99D, default!),
        new("-99"u8, -99D, default!),
        new("+1i"u8, 1D.i(), default!),
        new("-1i"u8, -1D.i(), default!),
        new("+3+1i"u8, 3D + 1D.i(), default!),
        new("30+3i"u8, 30D + 3D.i(), default!),
        new("+3e+3-3e+3i"u8, 3000D + -3000D.i(), default!),
        new("+3e+3+3e+3i"u8, 3000D + 3000D.i(), default!),
        new("+3e+3+3e+3i+"u8, 0D, ErrSyntax),

        // Separators
        new("0.1"u8, 0.1D, default!),
        new("0.1i"u8, 0.1D.i(), default!),
        new("0.1_2_3"u8, 0.123D, default!),
        new("+0x_3p3i"u8, 24D.i(), default!),
        new("0_0+0x_0p0i"u8, 0D, default!),
        new("0x_10.3p-8+0x3p3i"u8, 0.063232421875D + 24D.i(), default!),
        new("+0x_1_0.3p-8+0x_3_0p3i"u8, 0.063232421875D + 384D.i(), default!),
        new("0x1_0.3p+8-0x_3p3i"u8, 4144D + -24D.i(), default!),

        // Hexadecimals
        new("0x10.3p-8+0x3p3i"u8, 0.063232421875D + 24D.i(), default!),
        new("+0x10.3p-8+0x3p3i"u8, 0.063232421875D + 24D.i(), default!),
        new("0x10.3p+8-0x3p3i"u8, 4144D + -24D.i(), default!),
        new("0x1p0"u8, 1D, default!),
        new("0x1p1"u8, 2D, default!),
        new("0x1p-1"u8, 0.5D, default!),
        new("0x1ep-1"u8, 15D, default!),
        new("-0x1ep-1"u8, -15D, default!),
        new("-0x2p3"u8, -16D, default!),
        new("0x1e2"u8, 0D, ErrSyntax),
        new("1p2"u8, 0D, ErrSyntax),
        new("0x1e2i"u8, 0D, ErrSyntax),

        // ErrRange
        // next float64 - too large
        new("+0x1p1024"u8, infp0, ErrRange),
        new("-0x1p1024"u8, infm0, ErrRange),
        new("+0x1p1024i"u8, inf0p, ErrRange),
        new("-0x1p1024i"u8, inf0m, ErrRange),
        new("+0x1p1024+0x1p1024i"u8, infpp, ErrRange),
        new("+0x1p1024-0x1p1024i"u8, infpm, ErrRange),
        new("-0x1p1024+0x1p1024i"u8, infmp, ErrRange),
        new("-0x1p1024-0x1p1024i"u8, infmm, ErrRange),
        // the border is ...158079
        // borderline - okay
        new("+0x1.fffffffffffff7fffp1023+0x1.fffffffffffff7fffp1023i"u8, 1.7976931348623157e+308D + 1.7976931348623157e+308D.i(), default!),
        new("+0x1.fffffffffffff7fffp1023-0x1.fffffffffffff7fffp1023i"u8, 1.7976931348623157e+308D + -1.7976931348623157e+308D.i(), default!),
        new("-0x1.fffffffffffff7fffp1023+0x1.fffffffffffff7fffp1023i"u8, -1.7976931348623157e+308D + 1.7976931348623157e+308D.i(), default!),
        new("-0x1.fffffffffffff7fffp1023-0x1.fffffffffffff7fffp1023i"u8, -1.7976931348623157e+308D + -1.7976931348623157e+308D.i(), default!),
        // borderline - too large
        new("+0x1.fffffffffffff8p1023"u8, infp0, ErrRange),
        new("-0x1fffffffffffff.8p+971"u8, infm0, ErrRange),
        new("+0x1.fffffffffffff8p1023i"u8, inf0p, ErrRange),
        new("-0x1fffffffffffff.8p+971i"u8, inf0m, ErrRange),
        new("+0x1.fffffffffffff8p1023+0x1.fffffffffffff8p1023i"u8, infpp, ErrRange),
        new("+0x1.fffffffffffff8p1023-0x1.fffffffffffff8p1023i"u8, infpm, ErrRange),
        new("-0x1fffffffffffff.8p+971+0x1fffffffffffff.8p+971i"u8, infmp, ErrRange),
        new("-0x1fffffffffffff8p+967-0x1fffffffffffff8p+967i"u8, infmm, ErrRange),
        // a little too large
        new("1e308+1e308i"u8, 1e+308D + 1e+308D.i(), default!),
        new("2e308+2e308i"u8, infpp, ErrRange),
        new("1e309+1e309i"u8, infpp, ErrRange),
        new("0x1p1025+0x1p1025i"u8, infpp, ErrRange),
        new("2e308"u8, infp0, ErrRange),
        new("1e309"u8, infp0, ErrRange),
        new("0x1p1025"u8, infp0, ErrRange),
        new("2e308i"u8, inf0p, ErrRange),
        new("1e309i"u8, inf0p, ErrRange),
        new("0x1p1025i"u8, inf0p, ErrRange),
        // way too large
        new("+1e310+1e310i"u8, infpp, ErrRange),
        new("+1e310-1e310i"u8, infpm, ErrRange),
        new("-1e310+1e310i"u8, infmp, ErrRange),
        new("-1e310-1e310i"u8, infmm, ErrRange),
        // under/overflow exponent
        new("1e-4294967296"u8, 0D, default!),
        new("1e-4294967296i"u8, 0D, default!),
        new("1e-4294967296+1i"u8, 1D.i(), default!),
        new("1+1e-4294967296i"u8, 1D, default!),
        new("1e-4294967296+1e-4294967296i"u8, 0D, default!),
        new("1e+4294967296"u8, infp0, ErrRange),
        new("1e+4294967296i"u8, inf0p, ErrRange),
        new("1e+4294967296+1e+4294967296i"u8, infpp, ErrRange),
        new("1e+4294967296-1e+4294967296i"u8, infpm, ErrRange)
    }.slice();
    foreach (var i in range(tests)) {
        var test = Ꮡ(tests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError(Func: "ParseComplex"u8, Num: (~test).@in, Err: (~test).err)));
        }
        var (got, err) = ParseComplex((~test).@in, 128);
        if (!reflect.DeepEqual(err, (~test).err)) {
            Ꮡt.Fatalf("ParseComplex(%q, 128) = %v, %v; want %v, %v"u8, (~test).@in, got, err, (~test).@out, (~test).err);
        }
        if (!(cmplx.IsNaN((~test).@out) && cmplx.IsNaN(got)) && got != (~test).@out) {
            Ꮡt.Fatalf("ParseComplex(%q, 128) = %v, %v; want %v, %v"u8, (~test).@in, got, err, (~test).@out, (~test).err);
        }

        if ((complex128)(complex64)(~test).@out == (~test).@out) {
            var (gotΔ1, errΔ1) = ParseComplex((~test).@in, 64);
            if (!reflect.DeepEqual(errΔ1, (~test).err)) {
                Ꮡt.Fatalf("ParseComplex(%q, 64) = %v, %v; want %v, %v"u8, (~test).@in, gotΔ1, errΔ1, (~test).@out, (~test).err);
            }
            var got64 = (complex64)gotΔ1;
            if ((complex128)got64 != (~test).@out) {
                Ꮡt.Fatalf("ParseComplex(%q, 64) = %v, %v; want %v, %v"u8, (~test).@in, gotΔ1, errΔ1, (~test).@out, (~test).err);
            }
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string sᶜ = "1.5e308+1.0e307i"u8;

// Issue 42297: allow ParseComplex(s, not_32_or_64) for legacy reasons
public static void TestParseComplexIncorrectBitSize(ж<testing.T> Ꮡt) {
    @string s = sᶜ;
    complex128 want = /* 1.5e308 + 1.0e307i */ 1.5e+308D + 1e+307D.i();

    foreach (var (_, bitSize) in new nint[]{0, 10, 100, 256}.slice()) {
        var (c, err) = ParseComplex(s, bitSize);
        if (err != default!) {
            Ꮡt.Fatalf("ParseComplex(%q, %d) gave error %s"u8, s, bitSize, err);
        }
        if (c != want) {
            Ꮡt.Fatalf("ParseComplex(%q, %d) = %g (expected %g)"u8, s, bitSize, c, want);
        }
    }
}

partial struct atofTest {
    internal @string @in;
    internal @string @out;
    internal error err;
}

internal static ж<slice<atofTest>> Ꮡatoftests = new StandardBox<slice<atofTest>>(new atofTest[]{
    new(""u8, "0"u8, ErrSyntax),
    new("1.25"u8, "1.25"u8, default!),
    new("+1"u8, "1"u8, default!),
    new("1x"u8, "0"u8, ErrSyntax),
    new("1.1."u8, "0"u8, ErrSyntax),
    new("1e23"u8, "1e+23"u8, default!),
    new("1E23"u8, "1e+23"u8, default!),
    new("0x1fFe2.p0"u8, "131042"u8, default!),
    new("0x1fFe2.P0"u8, "131042"u8, default!),
    new("-0x2p3"u8, "-16"u8, default!),
    new("0x0.fp4"u8, "15"u8, default!),
    new("0x0.fp0"u8, "0.9375"u8, default!),
    new("0x1e2"u8, "0"u8, ErrSyntax),
    new("1p2"u8, "0"u8, ErrSyntax),
    new("0x1p1024"u8, "+Inf"u8, ErrRange),
    new("-0x1p1024"u8, "-Inf"u8, ErrRange),
    new("0x1.fffffffffffff7fffp1023"u8, "1.7976931348623157e+308"u8, default!),
    new("-0x1.fffffffffffff7fffp1023"u8, "-1.7976931348623157e+308"u8, default!),
    new("1.797693134862315808e308"u8, "+Inf"u8, ErrRange),
    new("-1.797693134862315808e308"u8, "-Inf"u8, ErrRange)
}.slice());
internal static ref slice<atofTest> atoftests => ref Ꮡatoftests.ValueSlot;

[GoInit] internal static void init() {
    // The atof routines return NumErrors wrapping
    // the error and the string. Convert the table above.
    foreach (var i in range(atoftests)) {
        var test = Ꮡ(atoftests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseFloat"u8, (~test).@in, (~test).err)));
        }
    }
}

public static void TestAtof(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < len(atoftests); i++) {
        var test = Ꮡ(atoftests, i);
        var (@out, err) = ParseFloat((~test).@in, 64);
        @string outs = FormatFloat(@out, (rune)'g', -1, 64);
        if (outs != (~test).@out || !reflect.DeepEqual(err, (~test).err)) {
            Ꮡt.Errorf("ParseFloat(%v, 64) = %v, %v want %v, %v"u8,
                (~test).@in, @out, err, (~test).@out, (~test).err);
        }

        if ((float64)(float32)@out == @out) {
            var (outΔ1, errΔ1) = ParseFloat((~test).@in, 32);
            var out32 = (float32)outΔ1;
            if ((float64)out32 != outΔ1) {
                Ꮡt.Errorf("ParseFloat(%v, 32) = %v, not a float32 (closest is %v)"u8, (~test).@in, outΔ1, (float64)out32);
                continue;
            }
            @string outsΔ1 = FormatFloat((float64)out32, (rune)'g', -1, 32);
            if (outsΔ1 != (~test).@out || !reflect.DeepEqual(errΔ1, (~test).err)) {
                Ꮡt.Errorf("ParseFloat(%v, 32) = %v, %v want %v, %v  # %v"u8,
                    (~test).@in, out32, errΔ1, (~test).@out, (~test).err, outΔ1);
            }
        }
    }
}

partial struct parseUint64Test {
    internal @string @in;
    internal uint64 @out;
    internal error err;
}

internal static ж<slice<parseUint64Test>> ᏑparseUint64Tests = new StandardBox<slice<parseUint64Test>>(new parseUint64Test[]{
    new(""u8, 0, ErrSyntax),
    new("0"u8, 0, default!),
    new("1"u8, 1, default!),
    new("12345"u8, 12345, default!),
    new("012345"u8, 12345, default!),
    new("18446744073709551616"u8, 18446744073709551615UL, ErrRange),
    new("-1"u8, 0, ErrSyntax)
}.slice());
internal static ref slice<parseUint64Test> parseUint64Tests => ref ᏑparseUint64Tests.ValueSlot;

partial struct parseUint64BaseTest {
    internal @string @in;
    internal nint @base;
    internal uint64 @out;
    internal error err;
}

internal static ж<slice<parseUint64BaseTest>> ᏑparseUint64BaseTests = new StandardBox<slice<parseUint64BaseTest>>(new parseUint64BaseTest[]{
    new(""u8, 0, 0, ErrSyntax),
    new("0"u8, 0, 0, default!),
    new("1"u8, 0, 1, default!),
    new("-1"u8, 0, 0, ErrSyntax),
    new("12345"u8, 0, 12345, default!),
    new("012345"u8, 0, 5349, default!),
    new("18446744073709551616"u8, 0, 18446744073709551615UL, ErrRange),
    new("0b"u8, 0, 0, ErrSyntax),
    new("101"u8, 2, 5, default!),
    new("101_"u8, 2, 0, ErrSyntax)
}.slice());
internal static ref slice<parseUint64BaseTest> parseUint64BaseTests => ref ᏑparseUint64BaseTests.ValueSlot;

partial struct parseInt64Test {
    internal @string @in;
    internal int64 @out;
    internal error err;
}

internal static ж<slice<parseInt64Test>> ᏑparseInt64Tests = new StandardBox<slice<parseInt64Test>>(new parseInt64Test[]{
    new(""u8, 0, ErrSyntax),
    new("0"u8, 0, default!),
    new("1"u8, 1, default!),
    new("-1"u8, -1, default!),
    new("12345"u8, 12345, default!),
    new("9223372036854775808"u8, 9223372036854775807L, ErrRange),
    new("123%45"u8, 0, ErrSyntax)
}.slice());
internal static ref slice<parseInt64Test> parseInt64Tests => ref ᏑparseInt64Tests.ValueSlot;

partial struct parseInt64BaseTest {
    internal @string @in;
    internal nint @base;
    internal int64 @out;
    internal error err;
}

internal static ж<slice<parseInt64BaseTest>> ᏑparseInt64BaseTests = new StandardBox<slice<parseInt64BaseTest>>(new parseInt64BaseTest[]{
    new(""u8, 0, 0, ErrSyntax),
    new("0"u8, 0, 0, default!),
    new("1"u8, 0, 1, default!),
    new("-1"u8, 0, -1, default!),
    new("12345"u8, 0, 12345, default!),
    new("12345"u8, 9, 8303, default!),
    new("012345"u8, 0, 5349, default!),
    new("9223372036854775808"u8, 10, 9223372036854775807L, ErrRange),
    new("0b"u8, 0, 0, ErrSyntax),
    new("101"u8, 2, 5, default!),
    new("101_"u8, 2, 0, ErrSyntax)
}.slice());
internal static ref slice<parseInt64BaseTest> parseInt64BaseTests => ref ᏑparseInt64BaseTests.ValueSlot;

partial struct parseUint32Test {
    internal @string @in;
    internal uint32 @out;
    internal error err;
}

internal static ж<slice<parseUint32Test>> ᏑparseUint32Tests = new StandardBox<slice<parseUint32Test>>(new parseUint32Test[]{
    new(""u8, 0, ErrSyntax),
    new("0"u8, 0, default!),
    new("1"u8, 1, default!),
    new("12345"u8, 12345, default!),
    new("12345x"u8, 0, ErrSyntax),
    new("987654321"u8, 987654321, default!),
    new("4294967296"u8, unchecked((uint32)(4294967295UL)), ErrRange),
    new("1_2_3_4_5"u8, 0, ErrSyntax), // base=10 so no underscores allowed
    new("12345_"u8, 0, ErrSyntax)
}.slice());
internal static ref slice<parseUint32Test> parseUint32Tests => ref ᏑparseUint32Tests.ValueSlot;

partial struct parseInt32Test {
    internal @string @in;
    internal int32 @out;
    internal error err;
}

internal static ж<slice<parseInt32Test>> ᏑparseInt32Tests = new StandardBox<slice<parseInt32Test>>(new parseInt32Test[]{
    new(""u8, 0, ErrSyntax),
    new("0"u8, 0, default!),
    new("-0"u8, 0, default!),
    new("1"u8, 1, default!),
    new("-1"u8, -1, default!),
    new("12345"u8, 12345, default!),
    new("-12345"u8, -12345, default!),
    new("2147483648"u8, (int32)(2147483648L - 1), ErrRange),
    new("12345_"u8, 0, ErrSyntax)
}.slice());
internal static ref slice<parseInt32Test> parseInt32Tests => ref ᏑparseInt32Tests.ValueSlot;

partial struct numErrorTest {
    internal @string num, want;
}

internal static slice<numErrorTest> numErrorTests = new numErrorTest[]{
    new("0"u8, @"strconv.ParseFloat: parsing ""0"": failed"u8),
    new("`"u8, "strconv.ParseFloat: parsing \"`\": failed"u8),
    new("1\x00.2"u8, @"strconv.ParseFloat: parsing ""1\x00.2"": failed"u8)
}.slice();

[GoInit] internal static void initΔ1() {
    // The parse routines return NumErrors wrapping
    // the error and the string. Convert the tables above.
    foreach (var i in range(parseUint64Tests)) {
        var test = Ꮡ(parseUint64Tests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseUint"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var i in range(parseUint64BaseTests)) {
        var test = Ꮡ(parseUint64BaseTests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseUint"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var i in range(parseInt64Tests)) {
        var test = Ꮡ(parseInt64Tests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseInt"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var i in range(parseInt64BaseTests)) {
        var test = Ꮡ(parseInt64BaseTests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseInt"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var i in range(parseUint32Tests)) {
        var test = Ꮡ(parseUint32Tests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseUint"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var i in range(parseInt32Tests)) {
        var test = Ꮡ(parseInt32Tests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseInt"u8, (~test).@in, (~test).err)));
        }
    }
}

public static void TestParseUint32(ж<testing.T> Ꮡt) {
    foreach (var i in range(parseUint32Tests)) {
        var test = Ꮡ(parseUint32Tests, i);
        var (@out, err) = ParseUint((~test).@in, 10, 32);
        if ((uint64)(~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseUint(%q, 10, 32) = %v, %v want %v, %v"u8,
                (~test).@in, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseUint64(ж<testing.T> Ꮡt) {
    foreach (var i in range(parseUint64Tests)) {
        var test = Ꮡ(parseUint64Tests, i);
        var (@out, err) = ParseUint((~test).@in, 10, 64);
        if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseUint(%q, 10, 64) = %v, %v want %v, %v"u8,
                (~test).@in, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseUint64Base(ж<testing.T> Ꮡt) {
    foreach (var i in range(parseUint64BaseTests)) {
        var test = Ꮡ(parseUint64BaseTests, i);
        var (@out, err) = ParseUint((~test).@in, (~test).@base, 64);
        if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseUint(%q, %v, 64) = %v, %v want %v, %v"u8,
                (~test).@in, (~test).@base, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseInt32(ж<testing.T> Ꮡt) {
    foreach (var i in range(parseInt32Tests)) {
        var test = Ꮡ(parseInt32Tests, i);
        var (@out, err) = ParseInt((~test).@in, 10, 32);
        if ((int64)(~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseInt(%q, 10 ,32) = %v, %v want %v, %v"u8,
                (~test).@in, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseInt64(ж<testing.T> Ꮡt) {
    foreach (var i in range(parseInt64Tests)) {
        var test = Ꮡ(parseInt64Tests, i);
        var (@out, err) = ParseInt((~test).@in, 10, 64);
        if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseInt(%q, 10, 64) = %v, %v want %v, %v"u8,
                (~test).@in, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseInt64Base(ж<testing.T> Ꮡt) {
    foreach (var i in range(parseInt64BaseTests)) {
        var test = Ꮡ(parseInt64BaseTests, i);
        var (@out, err) = ParseInt((~test).@in, (~test).@base, 64);
        if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
            Ꮡt.Errorf("ParseInt(%q, %v, 64) = %v, %v want %v, %v"u8,
                (~test).@in, (~test).@base, @out, err, (~test).@out, (~test).err);
        }
    }
}

public static void TestParseUint(ж<testing.T> Ꮡt) {
    var exprᴛ1 = IntSize;
    if (exprᴛ1 == 32) {
        foreach (var i in range(parseUint32Tests)) {
            var test = Ꮡ(parseUint32Tests, i);
            var (@out, err) = ParseUint((~test).@in, 10, 0);
            if ((uint64)(~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
                Ꮡt.Errorf("ParseUint(%q, 10, 0) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, (~test).err);
            }
        }
    }
    else if (exprᴛ1 == 64) {
        foreach (var i in range(parseUint64Tests)) {
            var test = Ꮡ(parseUint64Tests, i);
            var (@out, err) = ParseUint((~test).@in, 10, 0);
            if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
                Ꮡt.Errorf("ParseUint(%q, 10, 0) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, (~test).err);
            }
        }
    }
}

public static void TestParseInt(ж<testing.T> Ꮡt) {
    var exprᴛ1 = IntSize;
    if (exprᴛ1 == 32) {
        foreach (var i in range(parseInt32Tests)) {
            var test = Ꮡ(parseInt32Tests, i);
            var (@out, err) = ParseInt((~test).@in, 10, 0);
            if ((int64)(~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
                Ꮡt.Errorf("ParseInt(%q, 10, 0) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, (~test).err);
            }
        }
    }
    else if (exprᴛ1 == 64) {
        foreach (var i in range(parseInt64Tests)) {
            var test = Ꮡ(parseInt64Tests, i);
            var (@out, err) = ParseInt((~test).@in, 10, 0);
            if ((~test).@out != @out || !reflect.DeepEqual((~test).err, err)) {
                Ꮡt.Errorf("ParseInt(%q, 10, 0) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, (~test).err);
            }
        }
    }
}

public static void TestAtoi(ж<testing.T> Ꮡt) {
    var exprᴛ1 = IntSize;
    if (exprᴛ1 == 32) {
        foreach (var i in range(parseInt32Tests)) {
            var test = Ꮡ(parseInt32Tests, i);
            var (@out, err) = Atoi((~test).@in);
            error testErr = default!;
            if ((~test).err != default!) {
                testErr = new strconv.NumErrorжerror(Ꮡ(new NumError("Atoi"u8, (~test).@in, (~(~test).err._<ж<strconv.NumError>>()).Err)));
            }
            if ((nint)(~test).@out != @out || !reflect.DeepEqual(testErr, err)) {
                Ꮡt.Errorf("Atoi(%q) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, testErr);
            }
        }
    }
    else if (exprᴛ1 == 64) {
        foreach (var i in range(parseInt64Tests)) {
            var test = Ꮡ(parseInt64Tests, i);
            var (@out, err) = Atoi((~test).@in);
            error testErr = default!;
            if ((~test).err != default!) {
                testErr = new strconv.NumErrorжerror(Ꮡ(new NumError("Atoi"u8, (~test).@in, (~(~test).err._<ж<strconv.NumError>>()).Err)));
            }
            if ((~test).@out != (int64)@out || !reflect.DeepEqual(testErr, err)) {
                Ꮡt.Errorf("Atoi(%q) = %v, %v want %v, %v"u8,
                    (~test).@in, @out, err, (~test).@out, testErr);
            }
        }
    }
}

internal static error bitSizeErrStub(@string name, nint bitSize) {
    return new strconv.NumErrorжerror(bitSizeError(name, "0"u8, bitSize));
}

internal static error baseErrStub(@string name, nint @base) {
    return new strconv.NumErrorжerror(baseError(name, "0"u8, @base));
}

internal static error noErrStub(@string name, nint arg) {
    return default!;
}

partial struct parseErrorTest {
    internal nint arg;
    internal Func<@string, nint, error> errStub;
}

internal static ж<slice<parseErrorTest>> ᏑparseBitSizeTests = new StandardBox<slice<parseErrorTest>>(default(slice<parseErrorTest>));
internal static ref slice<parseErrorTest> parseBitSizeTests => ref ᏑparseBitSizeTests.ValueSlot;
internal static void initᴛparseBitSizeTests() { parseBitSizeTests = new parseErrorTest[]{
    new(-1, bitSizeErrStub),
    new(0, noErrStub),
    new(64, noErrStub),
    new(65, bitSizeErrStub)
}.slice(); }

internal static ж<slice<parseErrorTest>> ᏑparseBaseTests = new StandardBox<slice<parseErrorTest>>(default(slice<parseErrorTest>));
internal static ref slice<parseErrorTest> parseBaseTests => ref ᏑparseBaseTests.ValueSlot;
internal static void initᴛparseBaseTests() { parseBaseTests = new parseErrorTest[]{
    new(-1, baseErrStub),
    new(0, noErrStub),
    new(1, baseErrStub),
    new(2, noErrStub),
    new(36, noErrStub),
    new(37, baseErrStub)
}.slice(); }

internal static bool equalError(error a, error b) {
    if (a == default!) {
        return b == default!;
    }
    if (b == default!) {
        return a == default!;
    }
    return a.Error() == b.Error();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parseIntˢ = "ParseInt"u8;

public static void TestParseIntBitSize(ж<testing.T> Ꮡt) {
    foreach (var i in range(parseBitSizeTests)) {
        var test = Ꮡ(parseBitSizeTests, i);
        var testErr = (~test).errStub(parseIntˢ, (~test).arg);
        var (_, err) = ParseInt("0"u8, 0, (~test).arg);
        if (!equalError(testErr, err)) {
            Ꮡt.Errorf("ParseInt(\"0\", 0, %v) = 0, %v want 0, %v"u8,
                (~test).arg, err, testErr);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string parseUintˢ = "ParseUint"u8;

public static void TestParseUintBitSize(ж<testing.T> Ꮡt) {
    foreach (var i in range(parseBitSizeTests)) {
        var test = Ꮡ(parseBitSizeTests, i);
        var testErr = (~test).errStub(parseUintˢ, (~test).arg);
        var (_, err) = ParseUint("0"u8, 0, (~test).arg);
        if (!equalError(testErr, err)) {
            Ꮡt.Errorf("ParseUint(\"0\", 0, %v) = 0, %v want 0, %v"u8,
                (~test).arg, err, testErr);
        }
    }
}

public static void TestParseIntBase(ж<testing.T> Ꮡt) {
    foreach (var i in range(parseBaseTests)) {
        var test = Ꮡ(parseBaseTests, i);
        var testErr = (~test).errStub(parseIntˢ, (~test).arg);
        var (_, err) = ParseInt("0"u8, (~test).arg, 0);
        if (!equalError(testErr, err)) {
            Ꮡt.Errorf("ParseInt(\"0\", %v, 0) = 0, %v want 0, %v"u8,
                (~test).arg, err, testErr);
        }
    }
}

public static void TestParseUintBase(ж<testing.T> Ꮡt) {
    foreach (var i in range(parseBaseTests)) {
        var test = Ꮡ(parseBaseTests, i);
        var testErr = (~test).errStub(parseUintˢ, (~test).arg);
        var (_, err) = ParseUint("0"u8, (~test).arg, 0);
        if (!equalError(testErr, err)) {
            Ꮡt.Errorf("ParseUint(\"0\", %v, 0) = 0, %v want 0, %v"u8,
                (~test).arg, err, testErr);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string failedˢ = "failed"u8;

public static void TestNumError(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in numErrorTests) {
        var err = Ꮡ(new NumError(
            Func: "ParseFloat"u8,
            Num: test.num,
            Err: errors.New(failedˢ)
        ));
        {
            @string got = err.Error(); if (got != test.want) {
                Ꮡt.Errorf(@"(&NumError{""ParseFloat"", %q, ""failed""}).Error() = %v, want %v"u8, test.num, got, test.want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object errorsIsFailedWantedˢ = (@string)"errors.Is failed, wanted success"u8;

public static partial void TestNumErrorUnwrap(ж<testing.T> Ꮡt) {
    var err = Ꮡ(new NumError(Err: ErrSyntax));
    if (!errors.Is(new strconv.NumErrorжerror(err), ErrSyntax)) {
        Ꮡt.Error(errorsIsFailedWantedˢ);
    }
}

internal partial struct TestFormatComplex_tests /*dyn*/ {
    internal complex128 c;
    internal byte fmt;
    internal nint prec;
    internal nint bitSize;
    internal @string @out;
}

public static void TestFormatComplex(ж<testing.T> Ꮡt) {
    var tests = new TestFormatComplex_tests[]{
        // a variety of signs
        new(1D + 2D.i(), (rune)'g', -1, 128, "(1+2i)"u8),
        new(3D + -4D.i(), (rune)'g', -1, 128, "(3-4i)"u8),
        new(-5D + 6D.i(), (rune)'g', -1, 128, "(-5+6i)"u8),
        new(-7D + -8D.i(), (rune)'g', -1, 128, "(-7-8i)"u8),

        // test that fmt and prec are working
        new(3.14159D + 0.00123D.i(), (rune)'e', 3, 128, "(3.142e+00+1.230e-03i)"u8),
        new(3.14159D + 0.00123D.i(), (rune)'f', 3, 128, "(3.142+0.001i)"u8),
        new(3.14159D + 0.00123D.i(), (rune)'g', 3, 128, "(3.14+0.00123i)"u8),

        // ensure bitSize rounding is working
        new(1.2345678901234567D + 9.876543210987654D.i(), (rune)'f', -1, 128, "(1.2345678901234567+9.876543210987654i)"u8),
        new(1.2345678901234567D + 9.876543210987654D.i(), (rune)'f', -1, 64, "(1.2345679+9.876543i)"u8)

        // other cases are handled by FormatFloat tests
    }.slice();
    foreach (var (_, test) in tests) {
        @string @out = FormatComplex(test.c, test.fmt, test.prec, test.bitSize);
        if (@out != test.@out) {
            Ꮡt.Fatalf("FormatComplex(%v, %q, %d, %d) = %q; want %q"u8,
                test.c, test.fmt, test.prec, test.bitSize, @out, test.@out);
        }
    }
}

public static void TestFormatComplexInvalidBitSize(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatalf("expected panic due to invalid bitSize"u8);
                }
            }
        }, ref ᒐ);
        _ = FormatComplex(1D + 2D.i(), (rune)'g', -1, 100);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct itob64Test {
    internal int64 @in;
    internal nint @base;
    internal @string @out;
}

internal static slice<itob64Test> itob64tests = new itob64Test[]{
    new(0, 10, "0"u8),
    new(1, 10, "1"u8),
    new(-1, 10, "-1"u8),
    new(12345678, 10, "12345678"u8),
    new(-9223372036854775808L, 10, "-9223372036854775808"u8),
    new(16, 17, "g"u8),
    new(25, 25, "10"u8),
    new(38493362624L, 36, "holycow"u8)
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object abcˢ = (@string)"abc"u8;

public static void TestItoa(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, test) in itob64tests) {
            @string s = FormatInt(test.@in, test.@base);
            if (s != test.@out) {
                Ꮡt.Errorf("FormatInt(%v, %v) = %v want %v"u8,
                    test.@in, test.@base, s, test.@out);
            }
            var x = AppendInt(slice<byte>("abc"u8), test.@in, test.@base);
            if (((@string)x) != "abc"u8 + test.@out) {
                Ꮡt.Errorf("AppendInt(%q, %v, %v) = %q want %v"u8,
                    abcˢ, test.@in, test.@base, x, test.@out);
            }

            if (test.@in >= 0) {
                @string sΔ1 = FormatUint((uint64)test.@in, test.@base);
                if (sΔ1 != test.@out) {
                    Ꮡt.Errorf("FormatUint(%v, %v) = %v want %v"u8,
                        test.@in, test.@base, sΔ1, test.@out);
                }
                var xΔ1 = AppendUint(default!, (uint64)test.@in, test.@base);
                if (((sstring)xΔ1) != test.@out) {
                    Ꮡt.Errorf("AppendUint(%q, %v, %v) = %q want %v"u8,
                        abcˢ, (uint64)test.@in, test.@base, xΔ1, test.@out);
                }
            }

            if (test.@base == 10 && (int64)(nint)test.@in == test.@in) {
                @string sΔ2 = Itoa((nint)test.@in);
                if (sΔ2 != test.@out) {
                    Ꮡt.Errorf("Itoa(%v) = %v want %v"u8,
                        test.@in, sΔ2, test.@out);
                }
            }
        }

        // Override when base is illegal
        defer(() => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatalf("expected panic due to illegal base"u8);
                }
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

internal static slice<uitob64Test> uitob64tests = new uitob64Test[]{
    new(9223372036854775807UL, 10, "9223372036854775807"u8),
    new((uint64)1 << 63, 10, "9223372036854775808"u8),
    new(9223372036854775809UL, 10, "9223372036854775809"u8),
    new(18446744073709551614UL, 10, "18446744073709551614"u8),
    new(18446744073709551615UL, 10, "18446744073709551615"u8),
    new(18446744073709551615UL, 2, "1111111111111111111111111111111111111111111111111111111111111111"u8)
}.slice();

public static void TestUitoa(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in uitob64tests) {
        @string s = FormatUint(test.@in, test.@base);
        if (s != test.@out) {
            Ꮡt.Errorf("FormatUint(%v, %v) = %v want %v"u8,
                test.@in, test.@base, s, test.@out);
        }
        var x = AppendUint(slice<byte>("abc"u8), test.@in, test.@base);
        if (((@string)x) != "abc"u8 + test.@out) {
            Ꮡt.Errorf("AppendUint(%q, %v, %v) = %q want %v"u8,
                abcˢ, test.@in, test.@base, x, test.@out);
        }
    }
}

partial struct varlenUintsᴛ1 /*dyn*/ {
    internal uint64 @in;
    internal @string @out;
}
internal static slice<varlenUintsᴛ1> varlenUints = new varlenUintsᴛ1[]{
    new(1, "1"u8),
    new(12, "12"u8),
    new(123, "123"u8),
    new(1234, "1234"u8),
    new(12345, "12345"u8),
    new(123456, "123456"u8),
    new(1234567, "1234567"u8),
    new(12345678, "12345678"u8),
    new(123456789, "123456789"u8),
    new(1234567890, "1234567890"u8),
    new(12345678901UL, "12345678901"u8),
    new(123456789012UL, "123456789012"u8),
    new(1234567890123UL, "1234567890123"u8),
    new(12345678901234UL, "12345678901234"u8),
    new(123456789012345UL, "123456789012345"u8),
    new(1234567890123456UL, "1234567890123456"u8),
    new(12345678901234567UL, "12345678901234567"u8),
    new(123456789012345678UL, "123456789012345678"u8),
    new(1234567890123456789UL, "1234567890123456789"u8),
    new(12345678901234567890UL, "12345678901234567890"u8)
}.slice();

public static void TestFormatUintVarlen(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in varlenUints) {
        @string s = FormatUint(test.@in, 10);
        if (s != test.@out) {
            Ꮡt.Errorf("FormatUint(%v, 10) = %v want %v"u8, test.@in, s, test.@out);
        }
    }
}

} // end strconv_test_package
