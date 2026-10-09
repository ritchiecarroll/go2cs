// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static strconv_package;
using testing = testing_package;
using static go.strconv_internal_test_package;

partial class strconv_test_package {

internal partial struct TestFormatComplex_tests /*dyn*/ {
    internal complex128 c;
    internal byte fmt;
    internal nint prec;
    internal nint bitSize;
    internal @string @out;
}

public static void TestFormatComplex(ж<testing.T> Ꮡt) {
    var tests = new TestFormatComplex_tests[]{ // a variety of signs

        new(1D + 2D.i(), (rune)'g', -1, 128, "(1+2i)"u8),
        new(3D + -4D.i(), (rune)'g', -1, 128, "(3-4i)"u8),
        new(-5D + 6D.i(), (rune)'g', -1, 128, "(-5+6i)"u8),
        new(-7D + -8D.i(), (rune)'g', -1, 128, "(-7-8i)"u8), // test that fmt and prec are working

        new(3.14159D + 0.00123D.i(), (rune)'e', 3, 128, "(3.142e+00+1.230e-03i)"u8),
        new(3.14159D + 0.00123D.i(), (rune)'f', 3, 128, "(3.142+0.001i)"u8),
        new(3.14159D + 0.00123D.i(), (rune)'g', 3, 128, "(3.14+0.00123i)"u8), // ensure bitSize rounding is working

        new(1.2345678901234567D + 9.876543210987654D.i(), (rune)'f', -1, 128, "(1.2345678901234567+9.876543210987654i)"u8),
        new(1.2345678901234567D + 9.876543210987654D.i(), (rune)'f', -1, 64, "(1.2345679+9.876543i)"u8)
    }.slice();
    // other cases are handled by FormatFloat tests
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

} // end strconv_test_package
