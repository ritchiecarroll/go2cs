// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.strconv_package;
using math = math_package;
using rand = go.math.rand_package;
using reflect = reflect_package;
using strings = strings_package;
using sync = go.sync_package;
using testing = testing_package;
using go;
using go.math;
using static go.@internal.strconv_internal_test_package;

partial class strconv_test_package {

partial struct atofTest {
    internal @string @in;
    internal @string @out;
    internal error err;
}

internal static readonly ж<slice<atofTest>> Ꮡatoftests = new StaticBox<slice<atofTest>>(slice<atofTest>([
    new("", "0", ErrSyntax),
    new("1", "1", default),
    new("+1", "1", default),
    new("1x", "0", ErrSyntax),
    new("1.1.", "0", ErrSyntax),
    new("1e23", "1e+23", default),
    new("1E23", "1e+23", default),
    new("100000000000000000000000", "1e+23", default),
    new("1e-100", "1e-100", default),
    new("123456700", "1.234567e+08", default),
    new("99999999999999974834176", "9.999999999999997e+22", default),
    new("100000000000000000000001", "1.0000000000000001e+23", default),
    new("100000000000000008388608", "1.0000000000000001e+23", default),
    new("100000000000000016777215", "1.0000000000000001e+23", default),
    new("100000000000000016777216", "1.0000000000000003e+23", default),
    new("-1", "-1", default),
    new("-0.1", "-0.1", default),
    new("-0", "-0", default),
    new("1e-20", "1e-20", default),
    new("625e-3", "0.625", default),

    // Hexadecimal floating-point.
    new("0x1p0", "1", default),
    new("0x1p1", "2", default),
    new("0x1p-1", "0.5", default),
    new("0x1ep-1", "15", default),
    new("-0x1ep-1", "-15", default),
    new("-0x1_ep-1", "-15", default),
    new("0x1p-200", "6.223015277861142e-61", default),
    new("0x1p200", "1.6069380442589903e+60", default),
    new("0x1fFe2.p0", "131042", default),
    new("0x1fFe2.P0", "131042", default),
    new("-0x2p3", "-16", default),
    new("0x0.fp4", "15", default),
    new("0x0.fp0", "0.9375", default),
    new("0x1e2", "0", ErrSyntax),
    new("1p2", "0", ErrSyntax),

    // zeros
    new("0", "0", default),
    new("0e0", "0", default),
    new("-0e0", "-0", default),
    new("+0e0", "0", default),
    new("0e-0", "0", default),
    new("-0e-0", "-0", default),
    new("+0e-0", "0", default),
    new("0e+0", "0", default),
    new("-0e+0", "-0", default),
    new("+0e+0", "0", default),
    new("0e+01234567890123456789", "0", default),
    new("0.00e-01234567890123456789", "0", default),
    new("-0e+01234567890123456789", "-0", default),
    new("-0.00e-01234567890123456789", "-0", default),
    new("0x0p+01234567890123456789", "0", default),
    new("0x0.00p-01234567890123456789", "0", default),
    new("-0x0p+01234567890123456789", "-0", default),
    new("-0x0.00p-01234567890123456789", "-0", default),

    new("0e291", "0", default), // issue 15364
    new("0e292", "0", default), // issue 15364
    new("0e347", "0", default), // issue 15364
    new("0e348", "0", default), // issue 15364
    new("-0e291", "-0", default),
    new("-0e292", "-0", default),
    new("-0e347", "-0", default),
    new("-0e348", "-0", default),
    new("0x0p126", "0", default),
    new("0x0p127", "0", default),
    new("0x0p128", "0", default),
    new("0x0p129", "0", default),
    new("0x0p130", "0", default),
    new("0x0p1022", "0", default),
    new("0x0p1023", "0", default),
    new("0x0p1024", "0", default),
    new("0x0p1025", "0", default),
    new("0x0p1026", "0", default),
    new("-0x0p126", "-0", default),
    new("-0x0p127", "-0", default),
    new("-0x0p128", "-0", default),
    new("-0x0p129", "-0", default),
    new("-0x0p130", "-0", default),
    new("-0x0p1022", "-0", default),
    new("-0x0p1023", "-0", default),
    new("-0x0p1024", "-0", default),
    new("-0x0p1025", "-0", default),
    new("-0x0p1026", "-0", default),

    // NaNs
    new("nan", "NaN", default),
    new("NaN", "NaN", default),
    new("NAN", "NaN", default),

    // Infs
    new("inf", "+Inf", default),
    new("-Inf", "-Inf", default),
    new("+INF", "+Inf", default),
    new("-Infinity", "-Inf", default),
    new("+INFINITY", "+Inf", default),
    new("Infinity", "+Inf", default),

    // largest float64
    new("1.7976931348623157e308", "1.7976931348623157e+308", default),
    new("-1.7976931348623157e308", "-1.7976931348623157e+308", default),
    new("0x1.fffffffffffffp1023", "1.7976931348623157e+308", default),
    new("-0x1.fffffffffffffp1023", "-1.7976931348623157e+308", default),
    new("0x1fffffffffffffp+971", "1.7976931348623157e+308", default),
    new("-0x1fffffffffffffp+971", "-1.7976931348623157e+308", default),
    new("0x.1fffffffffffffp1027", "1.7976931348623157e+308", default),
    new("-0x.1fffffffffffffp1027", "-1.7976931348623157e+308", default),

    // next float64 - too large
    new("1.7976931348623159e308", "+Inf", ErrRange),
    new("-1.7976931348623159e308", "-Inf", ErrRange),
    new("0x1p1024", "+Inf", ErrRange),
    new("-0x1p1024", "-Inf", ErrRange),
    new("0x2p1023", "+Inf", ErrRange),
    new("-0x2p1023", "-Inf", ErrRange),
    new("0x.1p1028", "+Inf", ErrRange),
    new("-0x.1p1028", "-Inf", ErrRange),
    new("0x.2p1027", "+Inf", ErrRange),
    new("-0x.2p1027", "-Inf", ErrRange),

    // the border is ...158079
    // borderline - okay
    new("1.7976931348623158e308", "1.7976931348623157e+308", default),
    new("-1.7976931348623158e308", "-1.7976931348623157e+308", default),
    new("0x1.fffffffffffff7fffp1023", "1.7976931348623157e+308", default),
    new("-0x1.fffffffffffff7fffp1023", "-1.7976931348623157e+308", default),
    // borderline - too large
    new("1.797693134862315808e308", "+Inf", ErrRange),
    new("-1.797693134862315808e308", "-Inf", ErrRange),
    new("0x1.fffffffffffff8p1023", "+Inf", ErrRange),
    new("-0x1.fffffffffffff8p1023", "-Inf", ErrRange),
    new("0x1fffffffffffff.8p+971", "+Inf", ErrRange),
    new("-0x1fffffffffffff8p+967", "-Inf", ErrRange),
    new("0x.1fffffffffffff8p1027", "+Inf", ErrRange),
    new("-0x.1fffffffffffff9p1027", "-Inf", ErrRange),

    // a little too large
    new("1e308", "1e+308", default),
    new("2e308", "+Inf", ErrRange),
    new("1e309", "+Inf", ErrRange),
    new("0x1p1025", "+Inf", ErrRange),

    // way too large
    new("1e310", "+Inf", ErrRange),
    new("-1e310", "-Inf", ErrRange),
    new("1e400", "+Inf", ErrRange),
    new("-1e400", "-Inf", ErrRange),
    new("1e400000", "+Inf", ErrRange),
    new("-1e400000", "-Inf", ErrRange),
    new("0x1p1030", "+Inf", ErrRange),
    new("0x1p2000", "+Inf", ErrRange),
    new("0x1p2000000000", "+Inf", ErrRange),
    new("-0x1p1030", "-Inf", ErrRange),
    new("-0x1p2000", "-Inf", ErrRange),
    new("-0x1p2000000000", "-Inf", ErrRange),

    // denormalized
    new("1e-305", "1e-305", default),
    new("1e-306", "1e-306", default),
    new("1e-307", "1e-307", default),
    new("1e-308", "1e-308", default),
    new("1e-309", "1e-309", default),
    new("1e-310", "1e-310", default),
    new("1e-322", "1e-322", default),
    // smallest denormal
    new("5e-324", "5e-324", default),
    new("4e-324", "5e-324", default),
    new("3e-324", "5e-324", default),
    // too small
    new("2e-324", "0", default),
    // way too small
    new("1e-350", "0", default),
    new("1e-400000", "0", default),

    // Near denormals and denormals.
    new("0x2.00000000000000p-1010", "1.8227805048890994e-304", default), // 0x00e0000000000000
    new("0x1.fffffffffffff0p-1010", "1.8227805048890992e-304", default), // 0x00dfffffffffffff
    new("0x1.fffffffffffff7p-1010", "1.8227805048890992e-304", default), // rounded down
    new("0x1.fffffffffffff8p-1010", "1.8227805048890994e-304", default), // rounded up
    new("0x1.fffffffffffff9p-1010", "1.8227805048890994e-304", default), // rounded up

    new("0x2.00000000000000p-1022", "4.450147717014403e-308", default),  // 0x0020000000000000
    new("0x1.fffffffffffff0p-1022", "4.4501477170144023e-308", default), // 0x001fffffffffffff
    new("0x1.fffffffffffff7p-1022", "4.4501477170144023e-308", default), // rounded down
    new("0x1.fffffffffffff8p-1022", "4.450147717014403e-308", default),  // rounded up
    new("0x1.fffffffffffff9p-1022", "4.450147717014403e-308", default),  // rounded up

    new("0x1.00000000000000p-1022", "2.2250738585072014e-308", default), // 0x0010000000000000
    new("0x0.fffffffffffff0p-1022", "2.225073858507201e-308", default),  // 0x000fffffffffffff
    new("0x0.ffffffffffffe0p-1022", "2.2250738585072004e-308", default), // 0x000ffffffffffffe
    new("0x0.ffffffffffffe7p-1022", "2.2250738585072004e-308", default), // rounded down
    new("0x1.ffffffffffffe8p-1023", "2.225073858507201e-308", default),  // rounded up
    new("0x1.ffffffffffffe9p-1023", "2.225073858507201e-308", default),  // rounded up

    new("0x0.00000003fffff0p-1022", "2.072261e-317", default),  // 0x00000000003fffff
    new("0x0.00000003456780p-1022", "1.694649e-317", default),  // 0x0000000000345678
    new("0x0.00000003456787p-1022", "1.694649e-317", default),  // rounded down
    new("0x0.00000003456788p-1022", "1.694649e-317", default),  // rounded down (half to even)
    new("0x0.00000003456790p-1022", "1.6946496e-317", default), // 0x0000000000345679
    new("0x0.00000003456789p-1022", "1.6946496e-317", default), // rounded up

    new("0x0.0000000345678800000000000000000000000001p-1022", "1.6946496e-317", default), // rounded up

    new("0x0.000000000000f0p-1022", "7.4e-323", default), // 0x000000000000000f
    new("0x0.00000000000060p-1022", "3e-323", default),   // 0x0000000000000006
    new("0x0.00000000000058p-1022", "3e-323", default),   // rounded up
    new("0x0.00000000000057p-1022", "2.5e-323", default), // rounded down
    new("0x0.00000000000050p-1022", "2.5e-323", default), // 0x0000000000000005

    new("0x0.00000000000010p-1022", "5e-324", default),  // 0x0000000000000001
    new("0x0.000000000000081p-1022", "5e-324", default), // rounded up
    new("0x0.00000000000008p-1022", "0", default),       // rounded down
    new("0x0.00000000000007fp-1022", "0", default),      // rounded down

    // try to overflow exponent
    new("1e-4294967296", "0", default),
    new("1e+4294967296", "+Inf", ErrRange),
    new("1e-18446744073709551616", "0", default),
    new("1e+18446744073709551616", "+Inf", ErrRange),
    new("0x1p-4294967296", "0", default),
    new("0x1p+4294967296", "+Inf", ErrRange),
    new("0x1p-18446744073709551616", "0", default),
    new("0x1p+18446744073709551616", "+Inf", ErrRange),

    // Parse errors
    new("1e", "0", ErrSyntax),
    new("1e-", "0", ErrSyntax),
    new(".e-1", "0", ErrSyntax),
    new("1\x00.2", "0", ErrSyntax),
    new("0x", "0", ErrSyntax),
    new("0x.", "0", ErrSyntax),
    new("0x1", "0", ErrSyntax),
    new("0x.1", "0", ErrSyntax),
    new("0x1p", "0", ErrSyntax),
    new("0x.1p", "0", ErrSyntax),
    new("0x1p+", "0", ErrSyntax),
    new("0x.1p+", "0", ErrSyntax),
    new("0x1p-", "0", ErrSyntax),
    new("0x.1p-", "0", ErrSyntax),
    new("0x1p+2", "4", default),
    new("0x.1p+2", "0.25", default),
    new("0x1p-2", "0.25", default),
    new("0x.1p-2", "0.015625", default),

    // https://www.exploringbinary.com/java-hangs-when-converting-2-2250738585072012e-308/
    new("2.2250738585072012e-308", "2.2250738585072014e-308", default),
    // https://www.exploringbinary.com/php-hangs-on-numeric-value-2-2250738585072011e-308/
    new("2.2250738585072011e-308", "2.225073858507201e-308", default),

    // A very large number (initially wrongly parsed by the fast algorithm).
    new("4.630813248087435e+307", "4.630813248087435e+307", default),

    // A different kind of very large number.
    new("22.222222222222222", "22.22222222222222", default),
    new("2."u8 + strings.Repeat("2", 4000) + "e+1"u8, "22.22222222222222", default),
    new("0x1.1111111111111p222", "7.18931911124017e+66", default),
    new("0x2.2222222222222p221", "7.18931911124017e+66", default),
    new("0x2."u8 + strings.Repeat("2", 4000) + "p221"u8, "7.18931911124017e+66", default),

    // Exactly halfway between 1 and math.Nextafter(1, 2).
    // Round to even (down).
    new("1.00000000000000011102230246251565404236316680908203125", "1", default),
    new("0x1.00000000000008p0", "1", default),
    // Slightly lower; still round down.
    new("1.00000000000000011102230246251565404236316680908203124", "1", default),
    new("0x1.00000000000007Fp0", "1", default),
    // Slightly higher; round up.
    new("1.00000000000000011102230246251565404236316680908203126", "1.0000000000000002", default),
    new("0x1.000000000000081p0", "1.0000000000000002", default),
    new("0x1.00000000000009p0", "1.0000000000000002", default),
    // Slightly higher, but you have to read all the way to the end.
    new("1.00000000000000011102230246251565404236316680908203125"u8 + strings.Repeat("0", 10000) + "1"u8, "1.0000000000000002", default),
    new("0x1.00000000000008"u8 + strings.Repeat("0", 10000) + "1p0"u8, "1.0000000000000002", default),

    // Halfway between x := math.Nextafter(1, 2) and math.Nextafter(x, 2)
    // Round to even (up).
    new("1.00000000000000033306690738754696212708950042724609375", "1.0000000000000004", default),
    new("0x1.00000000000018p0", "1.0000000000000004", default),

    // Halfway between 1090544144181609278303144771584 and 1090544144181609419040633126912
    // (15497564393479157p+46, should round to even 15497564393479156p+46, issue 36657)
    new("1090544144181609348671888949248", "1.0905441441816093e+30", default),
    // slightly above, rounds up
    new("1090544144181609348835077142190", "1.0905441441816094e+30", default),

    // Underscores.
    new("1_23.50_0_0e+1_2", "1.235e+14", default),
    new("-_123.5e+12", "0", ErrSyntax),
    new("+_123.5e+12", "0", ErrSyntax),
    new("_123.5e+12", "0", ErrSyntax),
    new("1__23.5e+12", "0", ErrSyntax),
    new("123_.5e+12", "0", ErrSyntax),
    new("123._5e+12", "0", ErrSyntax),
    new("123.5_e+12", "0", ErrSyntax),
    new("123.5__0e+12", "0", ErrSyntax),
    new("123.5e_+12", "0", ErrSyntax),
    new("123.5e+_12", "0", ErrSyntax),
    new("123.5e_-12", "0", ErrSyntax),
    new("123.5e-_12", "0", ErrSyntax),
    new("123.5e+1__2", "0", ErrSyntax),
    new("123.5e+12_", "0", ErrSyntax),

    new("0x_1_2.3_4_5p+1_2", "74565", default),
    new("-_0x12.345p+12", "0", ErrSyntax),
    new("+_0x12.345p+12", "0", ErrSyntax),
    new("_0x12.345p+12", "0", ErrSyntax),
    new("0x__12.345p+12", "0", ErrSyntax),
    new("0x1__2.345p+12", "0", ErrSyntax),
    new("0x12_.345p+12", "0", ErrSyntax),
    new("0x12._345p+12", "0", ErrSyntax),
    new("0x12.3__45p+12", "0", ErrSyntax),
    new("0x12.345_p+12", "0", ErrSyntax),
    new("0x12.345p_+12", "0", ErrSyntax),
    new("0x12.345p+_12", "0", ErrSyntax),
    new("0x12.345p_-12", "0", ErrSyntax),
    new("0x12.345p-_12", "0", ErrSyntax),
    new("0x12.345p+1__2", "0", ErrSyntax),
    new("0x12.345p+12_", "0", ErrSyntax),

    new("1e100x", "0", ErrSyntax),
    new("1e1000x", "0", ErrSyntax)
]));
internal static ref slice<atofTest> atoftests => ref ((StaticBox<slice<atofTest>>)Ꮡatoftests).ValueSlot;

internal static slice<atofTest> atof32tests = slice<atofTest>([
    // Hex
    new("0x1p-100", "7.888609e-31", default),
    new("0x1p100", "1.2676506e+30", default),

    // Exactly halfway between 1 and the next float32.
    // Round to even (down).
    new("1.000000059604644775390625", "1", default),
    new("0x1.000001p0", "1", default),
    // Slightly lower.
    new("1.000000059604644775390624", "1", default),
    new("0x1.0000008p0", "1", default),
    new("0x1.000000fp0", "1", default),
    // Slightly higher.
    new("1.000000059604644775390626", "1.0000001", default),
    new("0x1.000002p0", "1.0000001", default),
    new("0x1.0000018p0", "1.0000001", default),
    new("0x1.0000011p0", "1.0000001", default),
    // Slightly higher, but you have to read all the way to the end.
    new("1.000000059604644775390625"u8 + strings.Repeat("0", 10000) + "1"u8, "1.0000001", default),
    new("0x1.000001"u8 + strings.Repeat("0", 10000) + "1p0"u8, "1.0000001", default),

    // largest float32: (1<<128) * (1 - 2^-24)
    new("340282346638528859811704183484516925440", "3.4028235e+38", default),
    new("-340282346638528859811704183484516925440", "-3.4028235e+38", default),
    new("0x.ffffffp128", "3.4028235e+38", default),
    new("-340282346638528859811704183484516925440", "-3.4028235e+38", default),
    new("-0x.ffffffp128", "-3.4028235e+38", default),
    // next float32 - too large
    new("3.4028236e38", "+Inf", ErrRange),
    new("-3.4028236e38", "-Inf", ErrRange),
    new("0x1.0p128", "+Inf", ErrRange),
    new("-0x1.0p128", "-Inf", ErrRange),
    // the border is 3.40282356779...e+38
    // borderline - okay
    new("3.402823567e38", "3.4028235e+38", default),
    new("-3.402823567e38", "-3.4028235e+38", default),
    new("0x.ffffff7fp128", "3.4028235e+38", default),
    new("-0x.ffffff7fp128", "-3.4028235e+38", default),
    // borderline - too large
    new("3.4028235678e38", "+Inf", ErrRange),
    new("-3.4028235678e38", "-Inf", ErrRange),
    new("0x.ffffff8p128", "+Inf", ErrRange),
    new("-0x.ffffff8p128", "-Inf", ErrRange),

    // Denormals: less than 2^-126
    new("1e-38", "1e-38", default),
    new("1e-39", "1e-39", default),
    new("1e-40", "1e-40", default),
    new("1e-41", "1e-41", default),
    new("1e-42", "1e-42", default),
    new("1e-43", "1e-43", default),
    new("1e-44", "1e-44", default),
    new("6e-45", "6e-45", default), // 4p-149 = 5.6e-45
    new("5e-45", "6e-45", default),

    // Smallest denormal
    new("1e-45", "1e-45", default), // 1p-149 = 1.4e-45
    new("2e-45", "1e-45", default),
    new("3e-45", "3e-45", default),

    // Near denormals and denormals.
    new("0x0.89aBcDp-125", "1.2643093e-38", default),  // 0x0089abcd
    new("0x0.8000000p-125", "1.1754944e-38", default), // 0x00800000
    new("0x0.1234560p-125", "1.671814e-39", default),  // 0x00123456
    new("0x0.1234567p-125", "1.671814e-39", default),  // rounded down
    new("0x0.1234568p-125", "1.671814e-39", default),  // rounded down
    new("0x0.1234569p-125", "1.671815e-39", default),  // rounded up
    new("0x0.1234570p-125", "1.671815e-39", default),  // 0x00123457
    new("0x0.0000010p-125", "1e-45", default),         // 0x00000001
    new("0x0.00000081p-125", "1e-45", default),        // rounded up
    new("0x0.0000008p-125", "0", default),             // rounded down
    new("0x0.0000007p-125", "0", default),             // rounded down

    // 2^92 = 8388608p+69 = 4951760157141521099596496896 (4.9517602e27)
    // is an exact power of two that needs 8 decimal digits to be correctly
    // parsed back.
    // The float32 before is 16777215p+68 = 4.95175986e+27
    // The halfway is 4.951760009. A bad algorithm that thinks the previous
    // float32 is 8388607p+69 will shorten incorrectly to 4.95176e+27.
    new("4951760157141521099596496896", "4.9517602e+27", default)
]);

partial struct atofSimpleTest {
    internal float64 x;
    internal @string s;
}

internal static readonly ж<sync.Once> ᏑatofOnce = new StaticBox<sync.Once>(default(sync.Once));
internal static ref sync.Once atofOnce => ref ((StaticBox<sync.Once>)ᏑatofOnce).ValueSlot;
internal static slice<atofSimpleTest> atofRandomTests;
internal static array<@string> benchmarksRandomBits = new(1024);
internal static array<@string> benchmarksRandomNormal = new(1024);

internal static void initAtof() {
    ᏑatofOnce.Do(initAtofOnce);
}

internal static void initAtofOnce() {
    // Generate random inputs for tests and benchmarks
    if (testing.Short()) {
        atofRandomTests = new slice<atofSimpleTest>(100);
    } else {
        atofRandomTests = new slice<atofSimpleTest>(10000);
    }
    foreach (var i in range(atofRandomTests)) {
        var n = (uint64)rand.Uint32() << 32 | (uint64)rand.Uint32();
        var x = math.Float64frombits(n);
        @string s = FormatFloat(x, (byte)'g', -1, 64);
        atofRandomTests[i] = new atofSimpleTest(x, s);
    }

    foreach (var i in range(benchmarksRandomBits)) {
        var bits = (uint64)rand.Uint32() << 32 | (uint64)rand.Uint32();
        var x = math.Float64frombits(bits);
        benchmarksRandomBits[i] = FormatFloat(x, (byte)'g', -1, 64);
    }

    foreach (var i in range(benchmarksRandomNormal)) {
        var x = rand.NormFloat64();
        benchmarksRandomNormal[i] = FormatFloat(x, (byte)'g', -1, 64);
    }
}

public static void TestParseFloatPrefix(ж<testing.T> t) {
    foreach (var i in range(atoftests)) {
        var test = Ꮡ(atoftests, i);
        if (test.err != nil) {
            continue;
        }
        // Adding characters that do not extend a number should not invalidate it.
        // Test a few. The "i" and "init" cases test that we accept "infi", "infinit"
        // correctly as "inf" with suffix.
        foreach (var (_, suffix) in slice<@string>([" ", "q", "+", "-", "<", "=", ">", "(", ")", "i", "init"])) {
            @string @in = test.@in + suffix;
            var (_, n, err) = parseFloatPrefix(@in, 64);
            if (err != nil) {
                t.Errorf("ParseFloatPrefix(%q, 64): err = %v; want no error", @in, err);
            }
            if (n != len(test.@in)) {
                t.Errorf("ParseFloatPrefix(%q, 64): n = %d; want %d", @in, n, len(test.@in));
            }
        }
    }
}

internal static void testAtof(ж<testing.T> t, bool opt) {
    initAtof();
    var oldopt = strconv_internal_test_package.SetOptimize(opt);
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
    foreach (var (_, test) in atof32tests) {
        var (@out, err) = ParseFloat(test.@in, 32);
        var out32 = (float32)@out;
        if ((float64)out32 != @out) {
            t.Errorf("ParseFloat(%v, 32) = %v, not a float32 (closest is %v)", test.@in, @out, (float64)out32);
            continue;
        }
        @string outs = FormatFloat((float64)out32, (byte)'g', -1, 32);
        if (outs != test.@out || !reflect.DeepEqual(err, test.err)) {
            t.Errorf("ParseFloat(%v, 32) = %v, %v want %v, %v  # %v",
                test.@in, out32, err, test.@out, test.err, @out);
        }
    }
    strconv_internal_test_package.SetOptimize(oldopt);
}

public static void TestAtof(ж<testing.T> t) {
    testAtof(t, true);
}

public static void TestAtofSlow(ж<testing.T> t) {
    testAtof(t, false);
}

public static void TestAtofRandom(ж<testing.T> t) {
    initAtof();
    foreach (var (_, test) in atofRandomTests) {
        var (x, _) = ParseFloat(test.s, 64);
        switch (ᐧ) {
        default: {
            t.Errorf("number %s badly parsed as %b (expected %b)", test.s, x, test.x);
            break;
        }
        case {} when x == test.x: {
            break;
        }
        case {} when math.IsNaN(test.x) && math.IsNaN(x): {
            break;
        }}
    }
    t.Logf("tested %d random numbers", len(atofRandomTests));
}

partial struct roundTripCasesᴛ1 /*dyn*/ {
    internal float64 f;
    internal @string s;
}
internal static slice<roundTripCasesᴛ1> roundTripCases = slice<roundTripCasesᴛ1>([
    // Issue 2917.
    // This test will break the optimized conversion if the
    // FPU is using 80-bit registers instead of 64-bit registers,
    // usually because the operating system initialized the
    // thread with 80-bit precision and the Go runtime didn't
    // fix the FP control word.
    new(4874021953463889725235396608D, "4.87402195346389e+27"),
    new(4874021953463890274991210496D, "4.8740219534638903e+27")
]);

public static void TestRoundTrip(ж<testing.T> t) {
    foreach (var (_, tt) in roundTripCases) {
        var old = strconv_internal_test_package.SetOptimize(false);
        @string s = FormatFloat(tt.f, (byte)'g', -1, 64);
        if (s != tt.s) {
            t.Errorf("no-opt FormatFloat(%b) = %s, want %s", tt.f, s, tt.s);
        }
        var (f, err) = ParseFloat(tt.s, 64);
        if (f != tt.f || err != nil) {
            t.Errorf("no-opt ParseFloat(%s) = %b, %v want %b, nil", tt.s, f, err, tt.f);
        }
        strconv_internal_test_package.SetOptimize(true);
        s = FormatFloat(tt.f, (byte)'g', -1, 64);
        if (s != tt.s) {
            t.Errorf("opt FormatFloat(%b) = %s, want %s", tt.f, s, tt.s);
        }
        (f, err) = ParseFloat(tt.s, 64);
        if (f != tt.f || err != nil) {
            t.Errorf("opt ParseFloat(%s) = %b, %v want %b, nil", tt.s, f, err, tt.f);
        }
        strconv_internal_test_package.SetOptimize(old);
    }
}

// TestRoundTrip32 tries a fraction of all finite positive float32 values.
public static void TestRoundTrip32(ж<testing.T> t) {
    var step = (uint32)997;
    if (testing.Short()) {
        step = 99991;
    }
    nint count = 0;
    for (var i = (uint32)0; i < (uint32)0xff << 23; i += step) {
        var f = math.Float32frombits(i);
        if ((i & 1) == 1) {
            f = -f; // negative
        }
        @string s = FormatFloat((float64)f, (byte)'g', -1, 32);

        var (parsed, err) = ParseFloat(s, 32);
        var parsed32 = (float32)parsed;
        switch (ᐧ) {
        case {} when err != nil: {
            t.Errorf("ParseFloat(%q, 32) gave error %s", s, err);
            break;
        }
        case {} when (float64)parsed32 != parsed: {
            t.Errorf("ParseFloat(%q, 32) = %v, not a float32 (nearest is %v)", s, parsed, parsed32);
            break;
        }
        case {} when parsed32 != f: {
            t.Errorf("ParseFloat(%q, 32) = %b (expected %b)", s, parsed32, f);
            break;
        }}
        count++;
    }
    t.Logf("tested %d float32's", count);
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string sᶜ1 => ref sᶜ1ᵇ;
private static readonly @string sᶜ1ᵇ = "1.5e308"u8;

// Issue 42297: a lot of code in the wild accidentally calls ParseFloat(s, 10)
// or ParseFloat(s, 0), so allow bitSize values other than 32 and 64.
public static void TestParseFloatIncorrectBitSize(ж<testing.T> t) {
    @string s = sᶜ1;
    const float64 want = 1.5e308;

    foreach (var (_, bitSize) in slice<nint>([0, 10, 100, 128])) {
        var (f, err) = ParseFloat(s, bitSize);
        if (err != nil) {
            t.Fatalf("ParseFloat(%q, %d) gave error %s", s, bitSize, err);
        }
        if (f != want) {
            t.Fatalf("ParseFloat(%q, %d) = %g (expected %g)", s, bitSize, f, (float64)(want));
        }
    }
}

public static void BenchmarkAtof64Decimal(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("33909", 64);
    }
}

public static void BenchmarkAtof64Float(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("339.7784", 64);
    }
}

public static void BenchmarkAtof64FloatExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("-5.09e75", 64);
    }
}

public static void BenchmarkAtof64Big(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("123456789123456789123456789", 64);
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
        samples[i] = FormatFloat(t.x, (byte)'g', 20, 64);
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
        ParseFloat("33909", 32);
    }
}

public static void BenchmarkAtof32Float(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("339.778", 32);
    }
}

public static void BenchmarkAtof32FloatExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ParseFloat("12.3456e32", 32);
    }
}

public static void BenchmarkAtof32Random(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var n = (uint32)997;
    array<@string> float32strings = new(4096);
    foreach (var i in range(float32strings)) {
        n = (99991 * n + 42) % ((uint32)0xff << 23);
        float32strings[i] = FormatFloat((float64)math.Float32frombits(n), (byte)'g', -1, 32);
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
    foreach (var i in range(float32strings)) {
        n = (99991 * n + 42) % ((uint32)0xff << 23);
        float32strings[i] = FormatFloat((float64)math.Float32frombits(n), (byte)'g', 20, 32);
    }
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        ParseFloat(float32strings[i % 4096], 32);
    }
}

} // end strconv_test_package
