// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using bytes = bytes_package;
using fmt = fmt_package;
using Δrand = go.math.rand.rand_package;
using testing = testing_package;
using go.math.rand;
using io = io_package;
using static go.math.big_package;

partial class big_internal_test_package {

partial struct stringTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string @out;
    internal nint @base;
    internal int64 val;
    internal bool ok;
}
internal static slice<stringTestsᴛ1> stringTests = [
    // invalid inputs
    new(@in: ""),
    new(@in: "a"),
    new(@in: "z"),
    new(@in: "+"),
    new(@in: "-"),
    new(@in: "0b"),
    new(@in: "0o"),
    new(@in: "0x"),
    new(@in: "0y"),
    new(@in: "2", @base: 2),
    new(@in: "0b2", @base: 0),
    new(@in: "08"),
    new(@in: "8", @base: 8),
    new(@in: "0xg", @base: 0),
    new(@in: "g", @base: 16),

    // invalid inputs with separators
    // (smoke tests only - a comprehensive set of tests is in natconv_test.go)
    new(@in: "_"),
    new(@in: "0_"),
    new(@in: "_0"),
    new(@in: "-1__0"),
    new(@in: "0x10_"),
    new(@in: "1_000", @base: 10), // separators are not permitted for bases != 0
    new(@in: "d_e_a_d", @base: 16),

    // valid inputs
    new("0", "0", 0, 0, true),
    new("0", "0", 10, 0, true),
    new("0", "0", 16, 0, true),
    new("+0", "0", 0, 0, true),
    new("-0", "0", 0, 0, true),
    new("10", "10", 0, 10, true),
    new("10", "10", 10, 10, true),
    new("10", "10", 16, 16, true),
    new("-10", "-10", 16, -16, true),
    new("+10", "10", 16, 16, true),
    new("0b10", "2", 0, 2, true),
    new("0o10", "8", 0, 8, true),
    new("0x10", "16", 0, 16, true),
    new(@in: "0x10", @base: 16),
    new("-0x10", "-16", 0, -16, true),
    new("+0x10", "16", 0, 16, true),
    new("00", "0", 0, 0, true),
    new("0", "0", 8, 0, true),
    new("07", "7", 0, 7, true),
    new("7", "7", 8, 7, true),
    new("023", "19", 0, 19, true),
    new("23", "23", 8, 19, true),
    new("cafebabe", "cafebabe", 16, 0xcafebabeL, true),
    new("0b0", "0", 0, 0, true),
    new("-111", "-111", 2, -7, true),
    new("-0b111", "-7", 0, -7, true),
    new("0b1001010111", "599", 0, 0x257, true),
    new("1001010111", "1001010111", 2, 0x257, true),
    new("A", "a", 36, 10, true),
    new("A", "A", 37, 36, true),
    new("ABCXYZ", "abcxyz", 36, 623741435, true),
    new("ABCXYZ", "ABCXYZ", 62, 33536793425L, true),

    // valid input with separators
    // (smoke tests only - a comprehensive set of tests is in natconv_test.go)
    new("1_000", "1000", 0, 1000, true),
    new("0b_1010", "10", 0, 10, true),
    new("+0o_660", "432", 0, 432, true),
    new("-0xF00D_1E", "-15731998", 0, -0xf00d1e, true)
];

public static void TestIntText(ж<testing.T> t) {
    var z = @new<global::go.math.big_package.ΔInt>();
    foreach (var (_, test) in range(stringTests)) {
        if (!test.ok) {
            continue;
        }

        var (_, ok) = z.SetString(test.@in, test.@base);
        if (!ok) {
            t.Errorf("%v: failed to parse", test);
            continue;
        }

        nint @base = test.@base;
        if (@base == 0) {
            @base = 10;
        }

        @string got = z.Text(@base); if (got != test.@out) {
            t.Errorf("%v: got %s; want %s", test, got, test.@out);
        }
    }
}

public static void TestAppendText(ж<testing.T> t) {
    var z = @new<global::go.math.big_package.ΔInt>();
    slice<byte> buf = default;
    foreach (var (_, test) in range(stringTests)) {
        if (!test.ok) {
            continue;
        }

        var (_, ok) = z.SetString(test.@in, test.@base);
        if (!ok) {
            t.Errorf("%v: failed to parse", test);
            continue;
        }

        nint @base = test.@base;
        if (@base == 0) {
            @base = 10;
        }

        nint i = len(buf);
        buf = z.Append(buf, @base);
        @string got = (@string)(buf.slice(i)); if (got != test.@out) {
            t.Errorf("%v: got %s; want %s", test, got, test.@out);
        }
    }
}

internal static @string format(nint @base) {
    switch (@base) {
    case 2: {
        return "%b";
    }
    case 8: {
        return "%o";
    }
    case 16: {
        return "%x";
    }}
    return "%d";
}

public static void TestGetString(ж<testing.T> t) {
    var z = @new<global::go.math.big_package.ΔInt>();
    foreach (var (i, test) in range(stringTests)) {
        if (!test.ok) {
            continue;
        }
        z.SetInt64(test.val);

        if (test.@base == 10) {
            @string gotΔ1 = z.String(); if (gotΔ1 != test.@out) {
                t.Errorf("#%da got %s; want %s", i, gotΔ1, test.@out);
            }
        }

        @string f = format(test.@base);
        @string got = fmt.Sprintf(f, z.OrTypedNil());
        if (f == "%d"u8) {
            if (got != fmt.Sprintf("%d"u8, test.val)) {
                t.Errorf("#%db got %s; want %d", i, got, test.val);
            }
        } else {
            if (got != test.@out) {
                t.Errorf("#%dc got %s; want %s", i, got, test.@out);
            }
        }
    }
}

public static void TestSetString(ж<testing.T> t) {
    var tmp = @new<global::go.math.big_package.ΔInt>();
    foreach (var (i, test) in range(stringTests)) {
        // initialize to a non-zero value so that issues with parsing
        // 0 are detected
        tmp.SetInt64(1234567890);
        var (n1, ok1) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, test.@base);
        var (n2, ok2) = tmp.SetString(test.@in, test.@base);
        var expected = NewInt(test.val);
        if (ok1 != test.ok || ok2 != test.ok) {
            t.Errorf("#%d (input '%s') ok incorrect (should be %t)", i, test.@in, test.ok);
            continue;
        }
        if (!ok1) {
            if (n1 != nil) {
                t.Errorf("#%d (input '%s') n1 != nil", i, test.@in);
            }
            continue;
        }
        if (!ok2) {
            if (n2 != nil) {
                t.Errorf("#%d (input '%s') n2 != nil", i, test.@in);
            }
            continue;
        }

        if (ok1 && !isNormalized(n1)) {
            t.Errorf("#%d (input '%s'): %v is not normalized", i, test.@in, n1.Value);
        }
        if (ok2 && !isNormalized(n2)) {
            t.Errorf("#%d (input '%s'): %v is not normalized", i, test.@in, n2.Value);
        }

        if (n1.Cmp(expected) != 0) {
            t.Errorf("#%d (input '%s') got: %s want: %d", i, test.@in, n1.OrTypedNil(), test.val);
        }
        if (n2.Cmp(expected) != 0) {
            t.Errorf("#%d (input '%s') got: %s want: %d", i, test.@in, n2.OrTypedNil(), test.val);
        }
    }
}

partial struct formatTestsᴛ1 /*dyn*/ {
    internal @string input;
    internal @string format;
    internal @string output;
}
internal static slice<formatTestsᴛ1> formatTests = [
    new("<nil>", "%x", "<nil>"),
    new("<nil>", "%#x", "<nil>"),
    new("<nil>", "%#y", "%!y(big.Int=<nil>)"),

    new("10", "%b", "1010"),
    new("10", "%o", "12"),
    new("10", "%d", "10"),
    new("10", "%v", "10"),
    new("10", "%x", "a"),
    new("10", "%X", "A"),
    new("-10", "%X", "-A"),
    new("10", "%y", "%!y(big.Int=10)"),
    new("-10", "%y", "%!y(big.Int=-10)"),

    new("10", "%#b", "0b1010"),
    new("10", "%#o", "012"),
    new("10", "%O", "0o12"),
    new("-10", "%#b", "-0b1010"),
    new("-10", "%#o", "-012"),
    new("-10", "%O", "-0o12"),
    new("10", "%#d", "10"),
    new("10", "%#v", "10"),
    new("10", "%#x", "0xa"),
    new("10", "%#X", "0XA"),
    new("-10", "%#X", "-0XA"),
    new("10", "%#y", "%!y(big.Int=10)"),
    new("-10", "%#y", "%!y(big.Int=-10)"),

    new("1234", "%d", "1234"),
    new("1234", "%3d", "1234"),
    new("1234", "%4d", "1234"),
    new("-1234", "%d", "-1234"),
    new("1234", "% 5d", " 1234"),
    new("1234", "%+5d", "+1234"),
    new("1234", "%-5d", "1234 "),
    new("1234", "%x", "4d2"),
    new("1234", "%X", "4D2"),
    new("-1234", "%3x", "-4d2"),
    new("-1234", "%4x", "-4d2"),
    new("-1234", "%5x", " -4d2"),
    new("-1234", "%-5x", "-4d2 "),
    new("1234", "%03d", "1234"),
    new("1234", "%04d", "1234"),
    new("1234", "%05d", "01234"),
    new("1234", "%06d", "001234"),
    new("-1234", "%06d", "-01234"),
    new("1234", "%+06d", "+01234"),
    new("1234", "% 06d", " 01234"),
    new("1234", "%-6d", "1234  "),
    new("1234", "%-06d", "1234  "),
    new("-1234", "%-06d", "-1234 "),

    new("1234", "%.3d", "1234"),
    new("1234", "%.4d", "1234"),
    new("1234", "%.5d", "01234"),
    new("1234", "%.6d", "001234"),
    new("-1234", "%.3d", "-1234"),
    new("-1234", "%.4d", "-1234"),
    new("-1234", "%.5d", "-01234"),
    new("-1234", "%.6d", "-001234"),

    new("1234", "%8.3d", "    1234"),
    new("1234", "%8.4d", "    1234"),
    new("1234", "%8.5d", "   01234"),
    new("1234", "%8.6d", "  001234"),
    new("-1234", "%8.3d", "   -1234"),
    new("-1234", "%8.4d", "   -1234"),
    new("-1234", "%8.5d", "  -01234"),
    new("-1234", "%8.6d", " -001234"),

    new("1234", "%+8.3d", "   +1234"),
    new("1234", "%+8.4d", "   +1234"),
    new("1234", "%+8.5d", "  +01234"),
    new("1234", "%+8.6d", " +001234"),
    new("-1234", "%+8.3d", "   -1234"),
    new("-1234", "%+8.4d", "   -1234"),
    new("-1234", "%+8.5d", "  -01234"),
    new("-1234", "%+8.6d", " -001234"),

    new("1234", "% 8.3d", "    1234"),
    new("1234", "% 8.4d", "    1234"),
    new("1234", "% 8.5d", "   01234"),
    new("1234", "% 8.6d", "  001234"),
    new("-1234", "% 8.3d", "   -1234"),
    new("-1234", "% 8.4d", "   -1234"),
    new("-1234", "% 8.5d", "  -01234"),
    new("-1234", "% 8.6d", " -001234"),

    new("1234", "%.3x", "4d2"),
    new("1234", "%.4x", "04d2"),
    new("1234", "%.5x", "004d2"),
    new("1234", "%.6x", "0004d2"),
    new("-1234", "%.3x", "-4d2"),
    new("-1234", "%.4x", "-04d2"),
    new("-1234", "%.5x", "-004d2"),
    new("-1234", "%.6x", "-0004d2"),

    new("1234", "%8.3x", "     4d2"),
    new("1234", "%8.4x", "    04d2"),
    new("1234", "%8.5x", "   004d2"),
    new("1234", "%8.6x", "  0004d2"),
    new("-1234", "%8.3x", "    -4d2"),
    new("-1234", "%8.4x", "   -04d2"),
    new("-1234", "%8.5x", "  -004d2"),
    new("-1234", "%8.6x", " -0004d2"),

    new("1234", "%+8.3x", "    +4d2"),
    new("1234", "%+8.4x", "   +04d2"),
    new("1234", "%+8.5x", "  +004d2"),
    new("1234", "%+8.6x", " +0004d2"),
    new("-1234", "%+8.3x", "    -4d2"),
    new("-1234", "%+8.4x", "   -04d2"),
    new("-1234", "%+8.5x", "  -004d2"),
    new("-1234", "%+8.6x", " -0004d2"),

    new("1234", "% 8.3x", "     4d2"),
    new("1234", "% 8.4x", "    04d2"),
    new("1234", "% 8.5x", "   004d2"),
    new("1234", "% 8.6x", "  0004d2"),
    new("1234", "% 8.7x", " 00004d2"),
    new("1234", "% 8.8x", " 000004d2"),
    new("-1234", "% 8.3x", "    -4d2"),
    new("-1234", "% 8.4x", "   -04d2"),
    new("-1234", "% 8.5x", "  -004d2"),
    new("-1234", "% 8.6x", " -0004d2"),
    new("-1234", "% 8.7x", "-00004d2"),
    new("-1234", "% 8.8x", "-000004d2"),

    new("1234", "%-8.3d", "1234    "),
    new("1234", "%-8.4d", "1234    "),
    new("1234", "%-8.5d", "01234   "),
    new("1234", "%-8.6d", "001234  "),
    new("1234", "%-8.7d", "0001234 "),
    new("1234", "%-8.8d", "00001234"),
    new("-1234", "%-8.3d", "-1234   "),
    new("-1234", "%-8.4d", "-1234   "),
    new("-1234", "%-8.5d", "-01234  "),
    new("-1234", "%-8.6d", "-001234 "),
    new("-1234", "%-8.7d", "-0001234"),
    new("-1234", "%-8.8d", "-00001234"),

    new("16777215", "%b", "111111111111111111111111"), // 2**24 - 1

    new("0", "%.d", ""),
    new("0", "%.0d", ""),
    new("0", "%3.d", "")
];

public static void TestFormat(ж<testing.T> t) {
    foreach (var (i, test) in range(formatTests)) {
        ж<global::go.math.big_package.ΔInt> x = default;
        if (test.input != "<nil>"u8) {
            bool ok = default;
            (x, ok) = @new<global::go.math.big_package.ΔInt>().SetString(test.input, 0);
            if (!ok) {
                t.Errorf("#%d failed reading input %s", i, test.input);
            }
        }
        @string output = fmt.Sprintf(test.format, x.OrTypedNil());
        if (output != test.output) {
            t.Errorf("#%d got %q; want %q, {%q, %q, %q}", i, output, test.output, test.input, test.format, test.output);
        }
    }
}

internal partial struct scanTest {
    internal @string input;
    internal @string format;
    internal @string output;
    internal nint remaining;
}

internal static slice<scanTest> scanTests = [
    new("1010", "%b", "10", 0),
    new("0b1010", "%v", "10", 0),
    new("12", "%o", "10", 0),
    new("012", "%v", "10", 0),
    new("10", "%d", "10", 0),
    new("10", "%v", "10", 0),
    new("a", "%x", "10", 0),
    new("0xa", "%v", "10", 0),
    new("A", "%X", "10", 0),
    new("-A", "%X", "-10", 0),
    new("+0b1011001", "%v", "89", 0),
    new("0xA", "%v", "10", 0),
    new("0 ", "%v", "0", 1),
    new("2+3", "%v", "2", 2),
    new("0XABC 12", "%v", "2748", 3),

    new("10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000ffffffffffffffffffffffffffffffff00000000000022222223333333333444444444", "%x", "72999049881955123498258745691204661198291656115976958889267080286388402675338838184094604981077942396458276955120179409196748346461468914795561487752253275293347599221664790586512596660792869956", 0),
    new("ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff33377fffffffffffffffffffffffffffffffffffffffffffff0000000000022222eee1", "%x", "1167984798111281975972139931059274579172666497855631342228273284582214442805421410945513679697247078343332431249286160621687557589604464869034163736183926240549918956767671325412748661204059352801", 0),
    new("5c0d52f451aec609b15da8e5e5626c4eaa88723bdeac9d25ca9b961269400410ca208a16af9c2fb07d7a11c7772cba02c22f9711078d51a3797eb18e691295293284d988e349fa6deba46b25a4ecd9f715", "%x", "419981998319789881681348172155240145539175961318447822049735313481433836043208347786919222066492311384432264836938599791362288343314139526391998172436831830624710446410781662672086936222288181013", 0),
    new("92fcad4b5c0d52f451aec609b15da8e5e5626c4eaa88723bdeac9d25ca9b961269400410ca208a16af9c2fb07d799c32fe2f3cc5422f9711078d51a3797eb18e691295293284d8f5e69caf6decddfe1df6", "%x", "670619546945481998414061201992255225716434798957375727890607516800039934374391281275121813279544891602026798031004764406015624866771554937391445093144221697436880587924204655403711377861305572854", 0),
    new("10000000000000000000000200000000000000000000003000000000000000000000040000000000000000000000500000000000000000000006", "%d", "10000000000000000000000200000000000000000000003000000000000000000000040000000000000000000000500000000000000000000006", 0)
];

[GoInit] internal static void init() {
    foreach (var i in range(200)) {
        var d = new slice<byte>(i + 1);
        foreach (var j in range(len(d))) {
            d[j] = (byte)('0' + Δrand.N((byte)10));
        }
        if (d[0] == '0') {
            d[0] = (byte)'1';
        }
        scanTests = append(scanTests, new scanTest(input: ((@string)d), format: "%d", output: ((@string)d)));
    }
}

public static void TestScan(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (i, test) in range(scanTests)) {
        var x = @new<global::go.math.big_package.ΔInt>();
        buf.Reset();
        buf.WriteString(test.input);
        var (_, err) = fmt.Fscanf(new big_test_package.bytes_BufferжReader(Ꮡbuf), test.format, x.OrTypedNil()); if (err != nil) {
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
