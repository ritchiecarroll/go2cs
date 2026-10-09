// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δmath = math_package;
using cmplx = go.math.cmplx_package;
using reflect = reflect_package;
using static strconv_package;
using testing = testing_package;
using go.math;
using static go.strconv_internal_test_package;
using strconv = strconv_package;

partial class strconv_test_package {

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
    var tests = new atocTest[]{ // Clearly invalid

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
        new("3+5+5i"u8, 0D, ErrSyntax), // Parentheses

        new("()"u8, 0D, ErrSyntax),
        new("(i)"u8, 0D, ErrSyntax),
        new("(0)"u8, 0D, default!),
        new("(1i)"u8, 1D.i(), default!),
        new("(3.0+5.5i)"u8, 3D + 5.5D.i(), default!),
        new("(1)+1i"u8, 0D, ErrSyntax),
        new("(3.0+5.5i"u8, 0D, ErrSyntax),
        new("3.0+5.5i)"u8, 0D, ErrSyntax), // NaNs

        new("NaN"u8, complex(Δmath.NaN(), 0D), default!),
        new("NANi"u8, complex(0D, Δmath.NaN()), default!),
        new("nan+nAni"u8, complex(Δmath.NaN(), Δmath.NaN()), default!),
        new("+NaN"u8, 0D, ErrSyntax),
        new("-NaN"u8, 0D, ErrSyntax),
        new("NaN-NaNi"u8, 0D, ErrSyntax), // Infs

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
        new("inf-inf"u8, 0D, ErrSyntax), // Zeros

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
        new("-0e+0-0e+0i"u8, 0D, default!), // Regular non-zeroes

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
        new("+3e+3+3e+3i+"u8, 0D, ErrSyntax), // Separators

        new("0.1"u8, 0.1D, default!),
        new("0.1i"u8, 0.1D.i(), default!),
        new("0.1_2_3"u8, 0.123D, default!),
        new("+0x_3p3i"u8, 24D.i(), default!),
        new("0_0+0x_0p0i"u8, 0D, default!),
        new("0x_10.3p-8+0x3p3i"u8, 0.063232421875D + 24D.i(), default!),
        new("+0x_1_0.3p-8+0x_3_0p3i"u8, 0.063232421875D + 384D.i(), default!),
        new("0x1_0.3p+8-0x_3p3i"u8, 4144D + -24D.i(), default!), // Hexadecimals

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
        new("0x1e2i"u8, 0D, ErrSyntax), // ErrRange
 // next float64 - too large

        new("+0x1p1024"u8, infp0, ErrRange),
        new("-0x1p1024"u8, infm0, ErrRange),
        new("+0x1p1024i"u8, inf0p, ErrRange),
        new("-0x1p1024i"u8, inf0m, ErrRange),
        new("+0x1p1024+0x1p1024i"u8, infpp, ErrRange),
        new("+0x1p1024-0x1p1024i"u8, infpm, ErrRange),
        new("-0x1p1024+0x1p1024i"u8, infmp, ErrRange),
        new("-0x1p1024-0x1p1024i"u8, infmm, ErrRange), // the border is ...158079
 // borderline - okay

        new("+0x1.fffffffffffff7fffp1023+0x1.fffffffffffff7fffp1023i"u8, 1.7976931348623157e+308D + 1.7976931348623157e+308D.i(), default!),
        new("+0x1.fffffffffffff7fffp1023-0x1.fffffffffffff7fffp1023i"u8, 1.7976931348623157e+308D + -1.7976931348623157e+308D.i(), default!),
        new("-0x1.fffffffffffff7fffp1023+0x1.fffffffffffff7fffp1023i"u8, -1.7976931348623157e+308D + 1.7976931348623157e+308D.i(), default!),
        new("-0x1.fffffffffffff7fffp1023-0x1.fffffffffffff7fffp1023i"u8, -1.7976931348623157e+308D + -1.7976931348623157e+308D.i(), default!), // borderline - too large

        new("+0x1.fffffffffffff8p1023"u8, infp0, ErrRange),
        new("-0x1fffffffffffff.8p+971"u8, infm0, ErrRange),
        new("+0x1.fffffffffffff8p1023i"u8, inf0p, ErrRange),
        new("-0x1fffffffffffff.8p+971i"u8, inf0m, ErrRange),
        new("+0x1.fffffffffffff8p1023+0x1.fffffffffffff8p1023i"u8, infpp, ErrRange),
        new("+0x1.fffffffffffff8p1023-0x1.fffffffffffff8p1023i"u8, infpm, ErrRange),
        new("-0x1fffffffffffff.8p+971+0x1fffffffffffff.8p+971i"u8, infmp, ErrRange),
        new("-0x1fffffffffffff8p+967-0x1fffffffffffff8p+967i"u8, infmm, ErrRange), // a little too large

        new("1e308+1e308i"u8, 1e+308D + 1e+308D.i(), default!),
        new("2e308+2e308i"u8, infpp, ErrRange),
        new("1e309+1e309i"u8, infpp, ErrRange),
        new("0x1p1025+0x1p1025i"u8, infpp, ErrRange),
        new("2e308"u8, infp0, ErrRange),
        new("1e309"u8, infp0, ErrRange),
        new("0x1p1025"u8, infp0, ErrRange),
        new("2e308i"u8, inf0p, ErrRange),
        new("1e309i"u8, inf0p, ErrRange),
        new("0x1p1025i"u8, inf0p, ErrRange), // way too large

        new("+1e310+1e310i"u8, infpp, ErrRange),
        new("+1e310-1e310i"u8, infpm, ErrRange),
        new("-1e310+1e310i"u8, infmp, ErrRange),
        new("-1e310-1e310i"u8, infmm, ErrRange), // under/overflow exponent

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
    foreach (var (i, _) in tests) {
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

} // end strconv_test_package
