// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δmath = math_package;
using rand = go.math.rand_package;
using reflect = reflect_package;
using static strconv_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using go.math;
using static go.strconv_internal_test_package;
using strconv = strconv_package;

partial class strconv_test_package {

[GoType] partial struct atofTest {
    internal @string @in;
    internal @string @out;
    internal error err;
}

// Hexadecimal floating-point.
// zeros
// issue 15364
// issue 15364
// issue 15364
// issue 15364
// NaNs
// Infs
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
// Near denormals and denormals.
// 0x00e0000000000000
// 0x00dfffffffffffff
// rounded down
// rounded up
// rounded up
// 0x0020000000000000
// 0x001fffffffffffff
// rounded down
// rounded up
// rounded up
// 0x0010000000000000
// 0x000fffffffffffff
// 0x000ffffffffffffe
// rounded down
// rounded up
// rounded up
// 0x00000000003fffff
// 0x0000000000345678
// rounded down
// rounded down (half to even)
// 0x0000000000345679
// rounded up
// rounded up
// 0x000000000000000f
// 0x0000000000000006
// rounded up
// rounded down
// 0x0000000000000005
// 0x0000000000000001
// rounded up
// rounded down
// rounded down
// try to overflow exponent
// Parse errors
// https://www.exploringbinary.com/java-hangs-when-converting-2-2250738585072012e-308/
// https://www.exploringbinary.com/php-hangs-on-numeric-value-2-2250738585072011e-308/
// A very large number (initially wrongly parsed by the fast algorithm).
// A different kind of very large number.
// Exactly halfway between 1 and math.Nextafter(1, 2).
// Round to even (down).
// Slightly lower; still round down.
// Slightly higher; round up.
// Slightly higher, but you have to read all the way to the end.
// Halfway between x := math.Nextafter(1, 2) and math.Nextafter(x, 2)
// Round to even (up).
// Halfway between 1090544144181609278303144771584 and 1090544144181609419040633126912
// (15497564393479157p+46, should round to even 15497564393479156p+46, issue 36657)
// slightly above, rounds up
// Underscores.
internal static ж<slice<atofTest>> Ꮡatoftests = new StandardBox<slice<atofTest>>(new atofTest[]{
    new(""u8, "0"u8, ErrSyntax),
    new("1"u8, "1"u8, default!),
    new("+1"u8, "1"u8, default!),
    new("1x"u8, "0"u8, ErrSyntax),
    new("1.1."u8, "0"u8, ErrSyntax),
    new("1e23"u8, "1e+23"u8, default!),
    new("1E23"u8, "1e+23"u8, default!),
    new("100000000000000000000000"u8, "1e+23"u8, default!),
    new("1e-100"u8, "1e-100"u8, default!),
    new("123456700"u8, "1.234567e+08"u8, default!),
    new("99999999999999974834176"u8, "9.999999999999997e+22"u8, default!),
    new("100000000000000000000001"u8, "1.0000000000000001e+23"u8, default!),
    new("100000000000000008388608"u8, "1.0000000000000001e+23"u8, default!),
    new("100000000000000016777215"u8, "1.0000000000000001e+23"u8, default!),
    new("100000000000000016777216"u8, "1.0000000000000003e+23"u8, default!),
    new("-1"u8, "-1"u8, default!),
    new("-0.1"u8, "-0.1"u8, default!),
    new("-0"u8, "-0"u8, default!),
    new("1e-20"u8, "1e-20"u8, default!),
    new("625e-3"u8, "0.625"u8, default!),
    new("0x1p0"u8, "1"u8, default!),
    new("0x1p1"u8, "2"u8, default!),
    new("0x1p-1"u8, "0.5"u8, default!),
    new("0x1ep-1"u8, "15"u8, default!),
    new("-0x1ep-1"u8, "-15"u8, default!),
    new("-0x1_ep-1"u8, "-15"u8, default!),
    new("0x1p-200"u8, "6.223015277861142e-61"u8, default!),
    new("0x1p200"u8, "1.6069380442589903e+60"u8, default!),
    new("0x1fFe2.p0"u8, "131042"u8, default!),
    new("0x1fFe2.P0"u8, "131042"u8, default!),
    new("-0x2p3"u8, "-16"u8, default!),
    new("0x0.fp4"u8, "15"u8, default!),
    new("0x0.fp0"u8, "0.9375"u8, default!),
    new("0x1e2"u8, "0"u8, ErrSyntax),
    new("1p2"u8, "0"u8, ErrSyntax),
    new("0"u8, "0"u8, default!),
    new("0e0"u8, "0"u8, default!),
    new("-0e0"u8, "-0"u8, default!),
    new("+0e0"u8, "0"u8, default!),
    new("0e-0"u8, "0"u8, default!),
    new("-0e-0"u8, "-0"u8, default!),
    new("+0e-0"u8, "0"u8, default!),
    new("0e+0"u8, "0"u8, default!),
    new("-0e+0"u8, "-0"u8, default!),
    new("+0e+0"u8, "0"u8, default!),
    new("0e+01234567890123456789"u8, "0"u8, default!),
    new("0.00e-01234567890123456789"u8, "0"u8, default!),
    new("-0e+01234567890123456789"u8, "-0"u8, default!),
    new("-0.00e-01234567890123456789"u8, "-0"u8, default!),
    new("0x0p+01234567890123456789"u8, "0"u8, default!),
    new("0x0.00p-01234567890123456789"u8, "0"u8, default!),
    new("-0x0p+01234567890123456789"u8, "-0"u8, default!),
    new("-0x0.00p-01234567890123456789"u8, "-0"u8, default!),
    new("0e291"u8, "0"u8, default!),
    new("0e292"u8, "0"u8, default!),
    new("0e347"u8, "0"u8, default!),
    new("0e348"u8, "0"u8, default!),
    new("-0e291"u8, "-0"u8, default!),
    new("-0e292"u8, "-0"u8, default!),
    new("-0e347"u8, "-0"u8, default!),
    new("-0e348"u8, "-0"u8, default!),
    new("0x0p126"u8, "0"u8, default!),
    new("0x0p127"u8, "0"u8, default!),
    new("0x0p128"u8, "0"u8, default!),
    new("0x0p129"u8, "0"u8, default!),
    new("0x0p130"u8, "0"u8, default!),
    new("0x0p1022"u8, "0"u8, default!),
    new("0x0p1023"u8, "0"u8, default!),
    new("0x0p1024"u8, "0"u8, default!),
    new("0x0p1025"u8, "0"u8, default!),
    new("0x0p1026"u8, "0"u8, default!),
    new("-0x0p126"u8, "-0"u8, default!),
    new("-0x0p127"u8, "-0"u8, default!),
    new("-0x0p128"u8, "-0"u8, default!),
    new("-0x0p129"u8, "-0"u8, default!),
    new("-0x0p130"u8, "-0"u8, default!),
    new("-0x0p1022"u8, "-0"u8, default!),
    new("-0x0p1023"u8, "-0"u8, default!),
    new("-0x0p1024"u8, "-0"u8, default!),
    new("-0x0p1025"u8, "-0"u8, default!),
    new("-0x0p1026"u8, "-0"u8, default!),
    new("nan"u8, "NaN"u8, default!),
    new("NaN"u8, "NaN"u8, default!),
    new("NAN"u8, "NaN"u8, default!),
    new("inf"u8, "+Inf"u8, default!),
    new("-Inf"u8, "-Inf"u8, default!),
    new("+INF"u8, "+Inf"u8, default!),
    new("-Infinity"u8, "-Inf"u8, default!),
    new("+INFINITY"u8, "+Inf"u8, default!),
    new("Infinity"u8, "+Inf"u8, default!),
    new("1.7976931348623157e308"u8, "1.7976931348623157e+308"u8, default!),
    new("-1.7976931348623157e308"u8, "-1.7976931348623157e+308"u8, default!),
    new("0x1.fffffffffffffp1023"u8, "1.7976931348623157e+308"u8, default!),
    new("-0x1.fffffffffffffp1023"u8, "-1.7976931348623157e+308"u8, default!),
    new("0x1fffffffffffffp+971"u8, "1.7976931348623157e+308"u8, default!),
    new("-0x1fffffffffffffp+971"u8, "-1.7976931348623157e+308"u8, default!),
    new("0x.1fffffffffffffp1027"u8, "1.7976931348623157e+308"u8, default!),
    new("-0x.1fffffffffffffp1027"u8, "-1.7976931348623157e+308"u8, default!),
    new("1.7976931348623159e308"u8, "+Inf"u8, ErrRange),
    new("-1.7976931348623159e308"u8, "-Inf"u8, ErrRange),
    new("0x1p1024"u8, "+Inf"u8, ErrRange),
    new("-0x1p1024"u8, "-Inf"u8, ErrRange),
    new("0x2p1023"u8, "+Inf"u8, ErrRange),
    new("-0x2p1023"u8, "-Inf"u8, ErrRange),
    new("0x.1p1028"u8, "+Inf"u8, ErrRange),
    new("-0x.1p1028"u8, "-Inf"u8, ErrRange),
    new("0x.2p1027"u8, "+Inf"u8, ErrRange),
    new("-0x.2p1027"u8, "-Inf"u8, ErrRange),
    new("1.7976931348623158e308"u8, "1.7976931348623157e+308"u8, default!),
    new("-1.7976931348623158e308"u8, "-1.7976931348623157e+308"u8, default!),
    new("0x1.fffffffffffff7fffp1023"u8, "1.7976931348623157e+308"u8, default!),
    new("-0x1.fffffffffffff7fffp1023"u8, "-1.7976931348623157e+308"u8, default!),
    new("1.797693134862315808e308"u8, "+Inf"u8, ErrRange),
    new("-1.797693134862315808e308"u8, "-Inf"u8, ErrRange),
    new("0x1.fffffffffffff8p1023"u8, "+Inf"u8, ErrRange),
    new("-0x1.fffffffffffff8p1023"u8, "-Inf"u8, ErrRange),
    new("0x1fffffffffffff.8p+971"u8, "+Inf"u8, ErrRange),
    new("-0x1fffffffffffff8p+967"u8, "-Inf"u8, ErrRange),
    new("0x.1fffffffffffff8p1027"u8, "+Inf"u8, ErrRange),
    new("-0x.1fffffffffffff9p1027"u8, "-Inf"u8, ErrRange),
    new("1e308"u8, "1e+308"u8, default!),
    new("2e308"u8, "+Inf"u8, ErrRange),
    new("1e309"u8, "+Inf"u8, ErrRange),
    new("0x1p1025"u8, "+Inf"u8, ErrRange),
    new("1e310"u8, "+Inf"u8, ErrRange),
    new("-1e310"u8, "-Inf"u8, ErrRange),
    new("1e400"u8, "+Inf"u8, ErrRange),
    new("-1e400"u8, "-Inf"u8, ErrRange),
    new("1e400000"u8, "+Inf"u8, ErrRange),
    new("-1e400000"u8, "-Inf"u8, ErrRange),
    new("0x1p1030"u8, "+Inf"u8, ErrRange),
    new("0x1p2000"u8, "+Inf"u8, ErrRange),
    new("0x1p2000000000"u8, "+Inf"u8, ErrRange),
    new("-0x1p1030"u8, "-Inf"u8, ErrRange),
    new("-0x1p2000"u8, "-Inf"u8, ErrRange),
    new("-0x1p2000000000"u8, "-Inf"u8, ErrRange),
    new("1e-305"u8, "1e-305"u8, default!),
    new("1e-306"u8, "1e-306"u8, default!),
    new("1e-307"u8, "1e-307"u8, default!),
    new("1e-308"u8, "1e-308"u8, default!),
    new("1e-309"u8, "1e-309"u8, default!),
    new("1e-310"u8, "1e-310"u8, default!),
    new("1e-322"u8, "1e-322"u8, default!),
    new("5e-324"u8, "5e-324"u8, default!),
    new("4e-324"u8, "5e-324"u8, default!),
    new("3e-324"u8, "5e-324"u8, default!),
    new("2e-324"u8, "0"u8, default!),
    new("1e-350"u8, "0"u8, default!),
    new("1e-400000"u8, "0"u8, default!),
    new("0x2.00000000000000p-1010"u8, "1.8227805048890994e-304"u8, default!),
    new("0x1.fffffffffffff0p-1010"u8, "1.8227805048890992e-304"u8, default!),
    new("0x1.fffffffffffff7p-1010"u8, "1.8227805048890992e-304"u8, default!),
    new("0x1.fffffffffffff8p-1010"u8, "1.8227805048890994e-304"u8, default!),
    new("0x1.fffffffffffff9p-1010"u8, "1.8227805048890994e-304"u8, default!),
    new("0x2.00000000000000p-1022"u8, "4.450147717014403e-308"u8, default!),
    new("0x1.fffffffffffff0p-1022"u8, "4.4501477170144023e-308"u8, default!),
    new("0x1.fffffffffffff7p-1022"u8, "4.4501477170144023e-308"u8, default!),
    new("0x1.fffffffffffff8p-1022"u8, "4.450147717014403e-308"u8, default!),
    new("0x1.fffffffffffff9p-1022"u8, "4.450147717014403e-308"u8, default!),
    new("0x1.00000000000000p-1022"u8, "2.2250738585072014e-308"u8, default!),
    new("0x0.fffffffffffff0p-1022"u8, "2.225073858507201e-308"u8, default!),
    new("0x0.ffffffffffffe0p-1022"u8, "2.2250738585072004e-308"u8, default!),
    new("0x0.ffffffffffffe7p-1022"u8, "2.2250738585072004e-308"u8, default!),
    new("0x1.ffffffffffffe8p-1023"u8, "2.225073858507201e-308"u8, default!),
    new("0x1.ffffffffffffe9p-1023"u8, "2.225073858507201e-308"u8, default!),
    new("0x0.00000003fffff0p-1022"u8, "2.072261e-317"u8, default!),
    new("0x0.00000003456780p-1022"u8, "1.694649e-317"u8, default!),
    new("0x0.00000003456787p-1022"u8, "1.694649e-317"u8, default!),
    new("0x0.00000003456788p-1022"u8, "1.694649e-317"u8, default!),
    new("0x0.00000003456790p-1022"u8, "1.6946496e-317"u8, default!),
    new("0x0.00000003456789p-1022"u8, "1.6946496e-317"u8, default!),
    new("0x0.0000000345678800000000000000000000000001p-1022"u8, "1.6946496e-317"u8, default!),
    new("0x0.000000000000f0p-1022"u8, "7.4e-323"u8, default!),
    new("0x0.00000000000060p-1022"u8, "3e-323"u8, default!),
    new("0x0.00000000000058p-1022"u8, "3e-323"u8, default!),
    new("0x0.00000000000057p-1022"u8, "2.5e-323"u8, default!),
    new("0x0.00000000000050p-1022"u8, "2.5e-323"u8, default!),
    new("0x0.00000000000010p-1022"u8, "5e-324"u8, default!),
    new("0x0.000000000000081p-1022"u8, "5e-324"u8, default!),
    new("0x0.00000000000008p-1022"u8, "0"u8, default!),
    new("0x0.00000000000007fp-1022"u8, "0"u8, default!),
    new("1e-4294967296"u8, "0"u8, default!),
    new("1e+4294967296"u8, "+Inf"u8, ErrRange),
    new("1e-18446744073709551616"u8, "0"u8, default!),
    new("1e+18446744073709551616"u8, "+Inf"u8, ErrRange),
    new("0x1p-4294967296"u8, "0"u8, default!),
    new("0x1p+4294967296"u8, "+Inf"u8, ErrRange),
    new("0x1p-18446744073709551616"u8, "0"u8, default!),
    new("0x1p+18446744073709551616"u8, "+Inf"u8, ErrRange),
    new("1e"u8, "0"u8, ErrSyntax),
    new("1e-"u8, "0"u8, ErrSyntax),
    new(".e-1"u8, "0"u8, ErrSyntax),
    new("1\x00.2"u8, "0"u8, ErrSyntax),
    new("0x"u8, "0"u8, ErrSyntax),
    new("0x."u8, "0"u8, ErrSyntax),
    new("0x1"u8, "0"u8, ErrSyntax),
    new("0x.1"u8, "0"u8, ErrSyntax),
    new("0x1p"u8, "0"u8, ErrSyntax),
    new("0x.1p"u8, "0"u8, ErrSyntax),
    new("0x1p+"u8, "0"u8, ErrSyntax),
    new("0x.1p+"u8, "0"u8, ErrSyntax),
    new("0x1p-"u8, "0"u8, ErrSyntax),
    new("0x.1p-"u8, "0"u8, ErrSyntax),
    new("0x1p+2"u8, "4"u8, default!),
    new("0x.1p+2"u8, "0.25"u8, default!),
    new("0x1p-2"u8, "0.25"u8, default!),
    new("0x.1p-2"u8, "0.015625"u8, default!),
    new("2.2250738585072012e-308"u8, "2.2250738585072014e-308"u8, default!),
    new("2.2250738585072011e-308"u8, "2.225073858507201e-308"u8, default!),
    new("4.630813248087435e+307"u8, "4.630813248087435e+307"u8, default!),
    new("22.222222222222222"u8, "22.22222222222222"u8, default!),
    new("2."u8 + strings.Repeat("2"u8, 4000) + "e+1"u8, "22.22222222222222"u8, default!),
    new("0x1.1111111111111p222"u8, "7.18931911124017e+66"u8, default!),
    new("0x2.2222222222222p221"u8, "7.18931911124017e+66"u8, default!),
    new("0x2."u8 + strings.Repeat("2"u8, 4000) + "p221"u8, "7.18931911124017e+66"u8, default!),
    new("1.00000000000000011102230246251565404236316680908203125"u8, "1"u8, default!),
    new("0x1.00000000000008p0"u8, "1"u8, default!),
    new("1.00000000000000011102230246251565404236316680908203124"u8, "1"u8, default!),
    new("0x1.00000000000007Fp0"u8, "1"u8, default!),
    new("1.00000000000000011102230246251565404236316680908203126"u8, "1.0000000000000002"u8, default!),
    new("0x1.000000000000081p0"u8, "1.0000000000000002"u8, default!),
    new("0x1.00000000000009p0"u8, "1.0000000000000002"u8, default!),
    new("1.00000000000000011102230246251565404236316680908203125"u8 + strings.Repeat("0"u8, 10000) + "1"u8, "1.0000000000000002"u8, default!),
    new("0x1.00000000000008"u8 + strings.Repeat("0"u8, 10000) + "1p0"u8, "1.0000000000000002"u8, default!),
    new("1.00000000000000033306690738754696212708950042724609375"u8, "1.0000000000000004"u8, default!),
    new("0x1.00000000000018p0"u8, "1.0000000000000004"u8, default!),
    new("1090544144181609348671888949248"u8, "1.0905441441816093e+30"u8, default!),
    new("1090544144181609348835077142190"u8, "1.0905441441816094e+30"u8, default!),
    new("1_23.50_0_0e+1_2"u8, "1.235e+14"u8, default!),
    new("-_123.5e+12"u8, "0"u8, ErrSyntax),
    new("+_123.5e+12"u8, "0"u8, ErrSyntax),
    new("_123.5e+12"u8, "0"u8, ErrSyntax),
    new("1__23.5e+12"u8, "0"u8, ErrSyntax),
    new("123_.5e+12"u8, "0"u8, ErrSyntax),
    new("123._5e+12"u8, "0"u8, ErrSyntax),
    new("123.5_e+12"u8, "0"u8, ErrSyntax),
    new("123.5__0e+12"u8, "0"u8, ErrSyntax),
    new("123.5e_+12"u8, "0"u8, ErrSyntax),
    new("123.5e+_12"u8, "0"u8, ErrSyntax),
    new("123.5e_-12"u8, "0"u8, ErrSyntax),
    new("123.5e-_12"u8, "0"u8, ErrSyntax),
    new("123.5e+1__2"u8, "0"u8, ErrSyntax),
    new("123.5e+12_"u8, "0"u8, ErrSyntax),
    new("0x_1_2.3_4_5p+1_2"u8, "74565"u8, default!),
    new("-_0x12.345p+12"u8, "0"u8, ErrSyntax),
    new("+_0x12.345p+12"u8, "0"u8, ErrSyntax),
    new("_0x12.345p+12"u8, "0"u8, ErrSyntax),
    new("0x__12.345p+12"u8, "0"u8, ErrSyntax),
    new("0x1__2.345p+12"u8, "0"u8, ErrSyntax),
    new("0x12_.345p+12"u8, "0"u8, ErrSyntax),
    new("0x12._345p+12"u8, "0"u8, ErrSyntax),
    new("0x12.3__45p+12"u8, "0"u8, ErrSyntax),
    new("0x12.345_p+12"u8, "0"u8, ErrSyntax),
    new("0x12.345p_+12"u8, "0"u8, ErrSyntax),
    new("0x12.345p+_12"u8, "0"u8, ErrSyntax),
    new("0x12.345p_-12"u8, "0"u8, ErrSyntax),
    new("0x12.345p-_12"u8, "0"u8, ErrSyntax),
    new("0x12.345p+1__2"u8, "0"u8, ErrSyntax),
    new("0x12.345p+12_"u8, "0"u8, ErrSyntax),
    new("1e100x"u8, "0"u8, ErrSyntax),
    new("1e1000x"u8, "0"u8, ErrSyntax)
}.slice());
internal static ref slice<atofTest> atoftests => ref Ꮡatoftests.ValueSlot;

// Hex
// Exactly halfway between 1 and the next float32.
// Round to even (down).
// Slightly lower.
// Slightly higher.
// Slightly higher, but you have to read all the way to the end.
// largest float32: (1<<128) * (1 - 2^-24)
// next float32 - too large
// the border is 3.40282356779...e+38
// borderline - okay
// borderline - too large
// Denormals: less than 2^-126
// 4p-149 = 5.6e-45
// Smallest denormal
// 1p-149 = 1.4e-45
// Near denormals and denormals.
// 0x0089abcd
// 0x00800000
// 0x00123456
// rounded down
// rounded down
// rounded up
// 0x00123457
// 0x00000001
// rounded up
// rounded down
// rounded down
// 2^92 = 8388608p+69 = 4951760157141521099596496896 (4.9517602e27)
// is an exact power of two that needs 8 decimal digits to be correctly
// parsed back.
// The float32 before is 16777215p+68 = 4.95175986e+27
// The halfway is 4.951760009. A bad algorithm that thinks the previous
// float32 is 8388607p+69 will shorten incorrectly to 4.95176e+27.
internal static ж<slice<atofTest>> Ꮡatof32tests = new StandardBox<slice<atofTest>>(new atofTest[]{
    new("0x1p-100"u8, "7.888609e-31"u8, default!),
    new("0x1p100"u8, "1.2676506e+30"u8, default!),
    new("1.000000059604644775390625"u8, "1"u8, default!),
    new("0x1.000001p0"u8, "1"u8, default!),
    new("1.000000059604644775390624"u8, "1"u8, default!),
    new("0x1.0000008p0"u8, "1"u8, default!),
    new("0x1.000000fp0"u8, "1"u8, default!),
    new("1.000000059604644775390626"u8, "1.0000001"u8, default!),
    new("0x1.000002p0"u8, "1.0000001"u8, default!),
    new("0x1.0000018p0"u8, "1.0000001"u8, default!),
    new("0x1.0000011p0"u8, "1.0000001"u8, default!),
    new("1.000000059604644775390625"u8 + strings.Repeat("0"u8, 10000) + "1"u8, "1.0000001"u8, default!),
    new("0x1.000001"u8 + strings.Repeat("0"u8, 10000) + "1p0"u8, "1.0000001"u8, default!),
    new("340282346638528859811704183484516925440"u8, "3.4028235e+38"u8, default!),
    new("-340282346638528859811704183484516925440"u8, "-3.4028235e+38"u8, default!),
    new("0x.ffffffp128"u8, "3.4028235e+38"u8, default!),
    new("-340282346638528859811704183484516925440"u8, "-3.4028235e+38"u8, default!),
    new("-0x.ffffffp128"u8, "-3.4028235e+38"u8, default!),
    new("3.4028236e38"u8, "+Inf"u8, ErrRange),
    new("-3.4028236e38"u8, "-Inf"u8, ErrRange),
    new("0x1.0p128"u8, "+Inf"u8, ErrRange),
    new("-0x1.0p128"u8, "-Inf"u8, ErrRange),
    new("3.402823567e38"u8, "3.4028235e+38"u8, default!),
    new("-3.402823567e38"u8, "-3.4028235e+38"u8, default!),
    new("0x.ffffff7fp128"u8, "3.4028235e+38"u8, default!),
    new("-0x.ffffff7fp128"u8, "-3.4028235e+38"u8, default!),
    new("3.4028235678e38"u8, "+Inf"u8, ErrRange),
    new("-3.4028235678e38"u8, "-Inf"u8, ErrRange),
    new("0x.ffffff8p128"u8, "+Inf"u8, ErrRange),
    new("-0x.ffffff8p128"u8, "-Inf"u8, ErrRange),
    new("1e-38"u8, "1e-38"u8, default!),
    new("1e-39"u8, "1e-39"u8, default!),
    new("1e-40"u8, "1e-40"u8, default!),
    new("1e-41"u8, "1e-41"u8, default!),
    new("1e-42"u8, "1e-42"u8, default!),
    new("1e-43"u8, "1e-43"u8, default!),
    new("1e-44"u8, "1e-44"u8, default!),
    new("6e-45"u8, "6e-45"u8, default!),
    new("5e-45"u8, "6e-45"u8, default!),
    new("1e-45"u8, "1e-45"u8, default!),
    new("2e-45"u8, "1e-45"u8, default!),
    new("3e-45"u8, "3e-45"u8, default!),
    new("0x0.89aBcDp-125"u8, "1.2643093e-38"u8, default!),
    new("0x0.8000000p-125"u8, "1.1754944e-38"u8, default!),
    new("0x0.1234560p-125"u8, "1.671814e-39"u8, default!),
    new("0x0.1234567p-125"u8, "1.671814e-39"u8, default!),
    new("0x0.1234568p-125"u8, "1.671814e-39"u8, default!),
    new("0x0.1234569p-125"u8, "1.671815e-39"u8, default!),
    new("0x0.1234570p-125"u8, "1.671815e-39"u8, default!),
    new("0x0.0000010p-125"u8, "1e-45"u8, default!),
    new("0x0.00000081p-125"u8, "1e-45"u8, default!),
    new("0x0.0000008p-125"u8, "0"u8, default!),
    new("0x0.0000007p-125"u8, "0"u8, default!),
    new("4951760157141521099596496896"u8, "4.9517602e+27"u8, default!)
}.slice());
internal static ref slice<atofTest> atof32tests => ref Ꮡatof32tests.ValueSlot;

[GoType] partial struct atofSimpleTest {
    internal float64 x;
    internal @string s;
}

internal static ж<Δsync.Once> ᏑatofOnce = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once atofOnce => ref ᏑatofOnce.Value;
internal static slice<atofSimpleTest> atofRandomTests;
internal static array<@string> benchmarksRandomBits = new(1024);
internal static array<@string> benchmarksRandomNormal = new(1024);

internal static void initAtof() {
    ᏑatofOnce.Do(initAtofOnce);
}

internal static void initAtofOnce() {
    // The atof routines return NumErrors wrapping
    // the error and the string. Convert the table above.
    foreach (var (i, _) in atoftests) {
        var test = Ꮡ(atoftests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseFloat"u8, (~test).@in, (~test).err)));
        }
    }
    foreach (var (i, _) in atof32tests) {
        var test = Ꮡ(atof32tests, i);
        if ((~test).err != default!) {
            test.Value.err = new strconv.NumErrorжerror(Ꮡ(new NumError("ParseFloat"u8, (~test).@in, (~test).err)));
        }
    }
    // Generate random inputs for tests and benchmarks
    if (testing.Short()){
        atofRandomTests = new slice<atofSimpleTest>(100);
    } else {
        atofRandomTests = new slice<atofSimpleTest>(10000);
    }
    foreach (var (i, _) in atofRandomTests) {
        var n = (uint64)(((uint64)rand.Uint32() << (int)(32)) | (uint64)rand.Uint32());
        var x = Δmath.Float64frombits(n);
        @string s = FormatFloat(x, (rune)'g', -1, 64);
        atofRandomTests[i] = new atofSimpleTest(x, s);
    }
    foreach (var (i, _) in benchmarksRandomBits) {
        var bits = (uint64)(((uint64)rand.Uint32() << (int)(32)) | (uint64)rand.Uint32());
        var x = Δmath.Float64frombits(bits);
        benchmarksRandomBits[i] = FormatFloat(x, (rune)'g', -1, 64);
    }
    foreach (var (i, _) in benchmarksRandomNormal) {
        var x = rand.NormFloat64();
        benchmarksRandomNormal[i] = FormatFloat(x, (rune)'g', -1, 64);
    }
}

public static void TestParseFloatPrefix(ж<testing.T> Ꮡt) {
    foreach (var (i, _) in atoftests) {
        var test = Ꮡ(atoftests, i);
        if ((~test).err != default!) {
            continue;
        }
        // Adding characters that do not extend a number should not invalidate it.
        // Test a few. The "i" and "init" cases test that we accept "infi", "infinit"
        // correctly as "inf" with suffix.
        foreach (var (_, suffix) in new @string[]{" "u8, "q"u8, "+"u8, "-"u8, "<"u8, "="u8, ">"u8, "("u8, ")"u8, "i"u8, "init"u8}.slice()) {
            @string @in = (~test).@in + suffix;
            var (_, n, err) = strconv_internal_test_package.ParseFloatPrefix(@in, 64);
            if (err != default!) {
                Ꮡt.Errorf("ParseFloatPrefix(%q, 64): err = %v; want no error"u8, @in, err);
            }
            if (n != len((~test).@in)) {
                Ꮡt.Errorf("ParseFloatPrefix(%q, 64): n = %d; want %d"u8, @in, n, len((~test).@in));
            }
        }
    }
}

internal static void testAtof(ж<testing.T> Ꮡt, bool opt) {
    initAtof();
    var oldopt = strconv_internal_test_package.SetOptimize(opt);
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
    foreach (var (_, test) in atof32tests) {
        var (@out, err) = ParseFloat(test.@in, 32);
        var out32 = (float32)@out;
        if ((float64)out32 != @out) {
            Ꮡt.Errorf("ParseFloat(%v, 32) = %v, not a float32 (closest is %v)"u8, test.@in, @out, (float64)out32);
            continue;
        }
        @string outs = FormatFloat((float64)out32, (rune)'g', -1, 32);
        if (outs != test.@out || !reflect.DeepEqual(err, test.err)) {
            Ꮡt.Errorf("ParseFloat(%v, 32) = %v, %v want %v, %v  # %v"u8,
                test.@in, out32, err, test.@out, test.err, @out);
        }
    }
    strconv_internal_test_package.SetOptimize(oldopt);
}

public static void TestAtof(ж<testing.T> Ꮡt) {
    testAtof(Ꮡt, true);
}

public static void TestAtofSlow(ж<testing.T> Ꮡt) {
    testAtof(Ꮡt, false);
}

public static void TestAtofRandom(ж<testing.T> Ꮡt) {
    initAtof();
    foreach (var (_, test) in atofRandomTests) {
        var (x, _) = ParseFloat(test.s, 64);
        switch (ᐧ) {
        default: {
            Ꮡt.Errorf("number %s badly parsed as %b (expected %b)"u8, test.s, x, test.x);
            break;
        }
        case {} when x == test.x: {
            break;
        }
        case {} when Δmath.IsNaN(test.x) && Δmath.IsNaN(x): {
            break;
        }}

    }
    Ꮡt.Logf("tested %d random numbers"u8, len(atofRandomTests));
}

// Issue 2917.
// This test will break the optimized conversion if the
// FPU is using 80-bit registers instead of 64-bit registers,
// usually because the operating system initialized the
// thread with 80-bit precision and the Go runtime didn't
// fix the FP control word.

[GoType("dyn")] partial struct roundTripCasesᴛ1 {
    internal float64 f;
    internal @string s;
}
internal static slice<roundTripCasesᴛ1> roundTripCases = new roundTripCasesᴛ1[]{
    new(4874021953463889725235396608D, "4.87402195346389e+27"u8),
    new(4874021953463890274991210496D, "4.8740219534638903e+27"u8)
}.slice();

public static void TestRoundTrip(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in roundTripCases) {
        var old = strconv_internal_test_package.SetOptimize(false);
        @string s = FormatFloat(tt.f, (rune)'g', -1, 64);
        if (s != tt.s) {
            Ꮡt.Errorf("no-opt FormatFloat(%b) = %s, want %s"u8, tt.f, s, tt.s);
        }
        var (f, err) = ParseFloat(tt.s, 64);
        if (f != tt.f || err != default!) {
            Ꮡt.Errorf("no-opt ParseFloat(%s) = %b, %v want %b, nil"u8, tt.s, f, err, tt.f);
        }
        strconv_internal_test_package.SetOptimize(true);
        s = FormatFloat(tt.f, (rune)'g', -1, 64);
        if (s != tt.s) {
            Ꮡt.Errorf("opt FormatFloat(%b) = %s, want %s"u8, tt.f, s, tt.s);
        }
        (f, err) = ParseFloat(tt.s, 64);
        if (f != tt.f || err != default!) {
            Ꮡt.Errorf("opt ParseFloat(%s) = %b, %v want %b, nil"u8, tt.s, f, err, tt.f);
        }
        strconv_internal_test_package.SetOptimize(old);
    }
}

// TestRoundTrip32 tries a fraction of all finite positive float32 values.
public static void TestRoundTrip32(ж<testing.T> Ꮡt) {
    var step = (uint32)997;
    if (testing.Short()) {
        step = 99991;
    }
    nint count = 0;
    for (var i = (uint32)0; i < ((uint32)0xff << (int)(23)); i += step) {
        var f = Δmath.Float32frombits(i);
        if ((uint32)(i & 1) == 1) {
            f = -f; // negative
        }
        @string s = FormatFloat((float64)f, (rune)'g', -1, 32);
        var (parsed, err) = ParseFloat(s, 32);
        var parsed32 = (float32)parsed;
        switch (ᐧ) {
        case {} when err != default!: {
            Ꮡt.Errorf("ParseFloat(%q, 32) gave error %s"u8, s, err);
            break;
        }
        case {} when (float64)parsed32 != parsed: {
            Ꮡt.Errorf("ParseFloat(%q, 32) = %v, not a float32 (nearest is %v)"u8, s, parsed, parsed32);
            break;
        }
        case {} when parsed32 != f: {
            Ꮡt.Errorf("ParseFloat(%q, 32) = %b (expected %b)"u8, s, parsed32, f);
            break;
        }}

        count++;
    }
    Ꮡt.Logf("tested %d float32's"u8, count);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string sᶜ1 = "1.5e308"u8;

// Issue 42297: a lot of code in the wild accidentally calls ParseFloat(s, 10)
// or ParseFloat(s, 0), so allow bitSize values other than 32 and 64.
public static void TestParseFloatIncorrectBitSize(ж<testing.T> Ꮡt) {
    @string s = sᶜ1;
    const float64 want = 1.5e308;
    foreach (var (_, bitSize) in new nint[]{0, 10, 100, 128}.slice()) {
        var (f, err) = ParseFloat(s, bitSize);
        if (err != default!) {
            Ꮡt.Fatalf("ParseFloat(%q, %d) gave error %s"u8, s, bitSize, err);
        }
        if (f != want) {
            Ꮡt.Fatalf("ParseFloat(%q, %d) = %g (expected %g)"u8, s, bitSize, f, (float64)(want));
        }
    }
}

public static void BenchmarkAtof64Decimal(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("33909"u8, 64);
    }
}

public static void BenchmarkAtof64Float(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("339.7784"u8, 64);
    }
}

public static void BenchmarkAtof64FloatExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("-5.09e75"u8, 64);
    }
}

public static void BenchmarkAtof64Big(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("123456789123456789123456789"u8, 64);
    }
}

public static void BenchmarkAtof64RandomBits(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    initAtof();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        ParseFloat(benchmarksRandomBits[i % 1024], 64);
    }
}

public static void BenchmarkAtof64RandomFloats(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    initAtof();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        ParseFloat(benchmarksRandomNormal[i % 1024], 64);
    }
}

public static void BenchmarkAtof64RandomLongFloats(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    initAtof();
    var samples = new slice<@string>(len(atofRandomTests));
    foreach (var (i, t) in atofRandomTests) {
        samples[i] = FormatFloat(t.x, (rune)'g', 20, 64);
    }
    b.ResetTimer();
    nint idx = 0;
    for (nint i = 0; i < b.N; i++) {
        ParseFloat(samples[idx], 64);
        idx++;
        if (idx == len(samples)) {
            idx = 0;
        }
    }
}

public static void BenchmarkAtof32Decimal(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("33909"u8, 32);
    }
}

public static void BenchmarkAtof32Float(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("339.778"u8, 32);
    }
}

public static void BenchmarkAtof32FloatExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("12.3456e32"u8, 32);
    }
}

public static void BenchmarkAtof32Random(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var n = (uint32)997;
    array<@string> float32strings = new(4096);
    foreach (var (i, _) in float32strings) {
        n = (99991 * n + 42) % (((uint32)0xff << (int)(23)));
        float32strings[i] = FormatFloat((float64)Δmath.Float32frombits(n), (rune)'g', -1, 32);
    }
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        ParseFloat(float32strings[i % 4096], 32);
    }
}

public static void BenchmarkAtof32RandomLong(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var n = (uint32)997;
    array<@string> float32strings = new(4096);
    foreach (var (i, _) in float32strings) {
        n = (99991 * n + 42) % (((uint32)0xff << (int)(23)));
        float32strings[i] = FormatFloat((float64)Δmath.Float32frombits(n), (rune)'g', 20, 32);
    }
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        ParseFloat(float32strings[i % 4096], 32);
    }
}

} // end strconv_test_package
