// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.strconv_package;
using math = math_package;
using cmplx = go.math.cmplx_package;
using testing = testing_package;
using go.math;
using static go.@internal.strconv_internal_test_package;

partial class strconv_test_package {

internal static complex128 infp0 = complex(math.Inf(+1), 0D);
internal static complex128 infm0 = complex(math.Inf(-1), 0D);
internal static complex128 inf0p = complex(0D, math.Inf(+1));
internal static complex128 inf0m = complex(0D, math.Inf(-1));

internal static complex128 infpp = complex(math.Inf(+1), math.Inf(+1));
internal static complex128 infpm = complex(math.Inf(+1), math.Inf(-1));
internal static complex128 infmp = complex(math.Inf(-1), math.Inf(+1));
internal static complex128 infmm = complex(math.Inf(-1), math.Inf(-1));

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
        new("NaN", complex(math.NaN(), 0D), default),
        new("NANi", complex(0D, math.NaN()), default),
        new("nan+nAni", complex(math.NaN(), math.NaN()), default),
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
    foreach (var i in range(tests)) {
        var test = Ꮡ(tests, i);
        var (c, e) = ParseComplex(test.@in, 128);
        if (!sameComplex(c, test.@out) || !AreEqual(e, test.err)) {
            t.Errorf("ParseComplex(%s, 128) = %v, %v, want %v, %v", test.@in, c, e, test.@out, test.err);
        }
        if ((complex128)(complex64)test.@out == test.@out) {
            var (cΔ1, eΔ1) = ParseComplex(test.@in, 64);
            var c64 = (complex64)cΔ1;
            if (!sameComplex((complex128)c64, test.@out) || !AreEqual(eΔ1, test.err)) {
                t.Errorf("ParseComplex(%s, 64) = %v, %v, want %v, %v", test.@in, cΔ1, eΔ1, test.@out, test.err);
            }
        }
    }
}

internal static bool sameComplex(complex128 c1, complex128 c2) {
    return cmplx.IsNaN(c1) && cmplx.IsNaN(c2) || c1 == c2;
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string sᶜ => ref sᶜᵇ;
private static readonly @string sᶜᵇ = "1.5e308+1.0e307i"u8;

// Issue 42297: allow ParseComplex(s, not_32_or_64) for legacy reasons
public static void TestParseComplexIncorrectBitSize(ж<testing.T> t) {
    @string s = sᶜ;
    complex128 want = /* 1.5e308 + 1.0e307i */ 1.5e+308D + 1e+307D.i();

    foreach (var (_, bitSize) in slice<nint>([0, 10, 100, 256])) {
        var (c, err) = ParseComplex(s, bitSize);
        if (err != nil) {
            t.Fatalf("ParseComplex(%q, %d) gave error %s", s, bitSize, err);
        }
        if (c != want) {
            t.Fatalf("ParseComplex(%q, %d) = %g (expected %g)", s, bitSize, c, want);
        }
    }
}

} // end strconv_test_package
