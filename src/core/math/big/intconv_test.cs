// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using bytes = bytes_package;
using fmt = fmt_package;
using testing = testing_package;
using io = io_package;
using static go.math.big_package;

partial class big_internal_test_package {

// invalid inputs
// invalid inputs with separators
// (smoke tests only - a comprehensive set of tests is in natconv_test.go)
// separators are not permitted for bases != 0
// valid inputs
// valid input with separators
// (smoke tests only - a comprehensive set of tests is in natconv_test.go)

partial struct stringTestsᴛ1 /*dyn*/ {
    internal @string @in;
    internal @string @out;
    internal nint @base;
    internal int64 val;
    internal bool ok;
}
internal static slice<stringTestsᴛ1> stringTests = new stringTestsᴛ1[]{
    new(@in: ""u8),
    new(@in: "a"u8),
    new(@in: "z"u8),
    new(@in: "+"u8),
    new(@in: "-"u8),
    new(@in: "0b"u8),
    new(@in: "0o"u8),
    new(@in: "0x"u8),
    new(@in: "0y"u8),
    new(@in: "2"u8, @base: 2),
    new(@in: "0b2"u8, @base: 0),
    new(@in: "08"u8),
    new(@in: "8"u8, @base: 8),
    new(@in: "0xg"u8, @base: 0),
    new(@in: "g"u8, @base: 16),
    new(@in: "_"u8),
    new(@in: "0_"u8),
    new(@in: "_0"u8),
    new(@in: "-1__0"u8),
    new(@in: "0x10_"u8),
    new(@in: "1_000"u8, @base: 10),
    new(@in: "d_e_a_d"u8, @base: 16),
    new("0"u8, "0"u8, 0, 0, true),
    new("0"u8, "0"u8, 10, 0, true),
    new("0"u8, "0"u8, 16, 0, true),
    new("+0"u8, "0"u8, 0, 0, true),
    new("-0"u8, "0"u8, 0, 0, true),
    new("10"u8, "10"u8, 0, 10, true),
    new("10"u8, "10"u8, 10, 10, true),
    new("10"u8, "10"u8, 16, 16, true),
    new("-10"u8, "-10"u8, 16, -16, true),
    new("+10"u8, "10"u8, 16, 16, true),
    new("0b10"u8, "2"u8, 0, 2, true),
    new("0o10"u8, "8"u8, 0, 8, true),
    new("0x10"u8, "16"u8, 0, 16, true),
    new(@in: "0x10"u8, @base: 16),
    new("-0x10"u8, "-16"u8, 0, -16, true),
    new("+0x10"u8, "16"u8, 0, 16, true),
    new("00"u8, "0"u8, 0, 0, true),
    new("0"u8, "0"u8, 8, 0, true),
    new("07"u8, "7"u8, 0, 7, true),
    new("7"u8, "7"u8, 8, 7, true),
    new("023"u8, "19"u8, 0, 19, true),
    new("23"u8, "23"u8, 8, 19, true),
    new("cafebabe"u8, "cafebabe"u8, 16, 0xcafebabeL, true),
    new("0b0"u8, "0"u8, 0, 0, true),
    new("-111"u8, "-111"u8, 2, -7, true),
    new("-0b111"u8, "-7"u8, 0, -7, true),
    new("0b1001010111"u8, "599"u8, 0, 0x257, true),
    new("1001010111"u8, "1001010111"u8, 2, 0x257, true),
    new("A"u8, "a"u8, 36, 10, true),
    new("A"u8, "A"u8, 37, 36, true),
    new("ABCXYZ"u8, "abcxyz"u8, 36, 623741435, true),
    new("ABCXYZ"u8, "ABCXYZ"u8, 62, 33536793425L, true),
    new("1_000"u8, "1000"u8, 0, 1000, true),
    new("0b_1010"u8, "10"u8, 0, 10, true),
    new("+0o_660"u8, "432"u8, 0, 432, true),
    new("-0xF00D_1E"u8, "-15731998"u8, 0, -0xf00d1e, true)
}.slice();

public static void TestIntText(ж<testing.T> Ꮡt) {
    var z = @new<global::go.math.big_package.ΔInt>();
    foreach (var (_, test) in stringTests) {
        if (!test.ok) {
            continue;
        }
        var (_, ok) = z.SetString(test.@in, test.@base);
        if (!ok) {
            Ꮡt.Errorf("%v: failed to parse"u8, test);
            continue;
        }
        nint @base = test.@base;
        if (@base == 0) {
            @base = 10;
        }
        {
            @string got = z.Text(@base); if (got != test.@out) {
                Ꮡt.Errorf("%v: got %s; want %s"u8, test, got, test.@out);
            }
        }
    }
}

public static void TestAppendText(ж<testing.T> Ꮡt) {
    var z = @new<global::go.math.big_package.ΔInt>();
    slice<byte> buf = default!;
    foreach (var (_, test) in stringTests) {
        if (!test.ok) {
            continue;
        }
        var (_, ok) = z.SetString(test.@in, test.@base);
        if (!ok) {
            Ꮡt.Errorf("%v: failed to parse"u8, test);
            continue;
        }
        nint @base = test.@base;
        if (@base == 0) {
            @base = 10;
        }
        nint i = len(buf);
        buf = z.Append(buf, @base);
        {
            @string got = ((@string)(buf.slice(i))); if (got != test.@out) {
                Ꮡt.Errorf("%v: got %s; want %s"u8, test, got, test.@out);
            }
        }
    }
}

internal static @string format(nint @base) {
    switch (@base) {
    case 2: {
        return "%b"u8;
    }
    case 8: {
        return "%o"u8;
    }
    case 16: {
        return "%x"u8;
    }}

    return "%d"u8;
}

public static void TestGetString(ж<testing.T> Ꮡt) {
    var z = @new<global::go.math.big_package.ΔInt>();
    foreach (var (i, test) in stringTests) {
        if (!test.ok) {
            continue;
        }
        z.SetInt64(test.val);
        if (test.@base == 10) {
            {
                @string gotΔ1 = z.String(); if (gotΔ1 != test.@out) {
                    Ꮡt.Errorf("#%da got %s; want %s"u8, i, gotΔ1, test.@out);
                }
            }
        }
        @string f = format(test.@base);
        @string got = fmt.Sprintf(f, z.OrTypedNil());
        if (f == "%d"u8){
            if (got != fmt.Sprintf("%d"u8, test.val)) {
                Ꮡt.Errorf("#%db got %s; want %d"u8, i, got, test.val);
            }
        } else {
            if (got != test.@out) {
                Ꮡt.Errorf("#%dc got %s; want %s"u8, i, got, test.@out);
            }
        }
    }
}

public static void TestSetString(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tmp = @new<global::go.math.big_package.ΔInt>();
    foreach (var (i, test) in stringTests) {
        // initialize to a non-zero value so that issues with parsing
        // 0 are detected
        tmp.SetInt64(1234567890);
        var (n1, ok1) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, test.@base);
        var (n2, ok2) = tmp.SetString(test.@in, test.@base);
        var expected = NewInt(test.val);
        if (ok1 != test.ok || ok2 != test.ok) {
            Ꮡt.Errorf("#%d (input '%s') ok incorrect (should be %t)"u8, i, test.@in, test.ok);
            continue;
        }
        if (!ok1) {
            if (n1 != nil) {
                Ꮡt.Errorf("#%d (input '%s') n1 != nil"u8, i, test.@in);
            }
            continue;
        }
        if (!ok2) {
            if (n2 != nil) {
                Ꮡt.Errorf("#%d (input '%s') n2 != nil"u8, i, test.@in);
            }
            continue;
        }
        if (ok1 && !isNormalized(n1)) {
            Ꮡt.Errorf("#%d (input '%s'): %v is not normalized"u8, i, test.@in, n1.Value);
        }
        if (ok2 && !isNormalized(n2)) {
            Ꮡt.Errorf("#%d (input '%s'): %v is not normalized"u8, i, test.@in, n2.Value);
        }
        if (n1.Cmp(expected) != 0) {
            Ꮡt.Errorf("#%d (input '%s') got: %s want: %d"u8, i, test.@in, n1.OrTypedNil(), test.val);
        }
        if (n2.Cmp(expected) != 0) {
            Ꮡt.Errorf("#%d (input '%s') got: %s want: %d"u8, i, test.@in, n2.OrTypedNil(), test.val);
        }
    }
}

// 2**24 - 1

partial struct formatTestsᴛ1 /*dyn*/ {
    internal @string input;
    internal @string format;
    internal @string output;
}
internal static slice<formatTestsᴛ1> formatTests = new formatTestsᴛ1[]{
    new("<nil>"u8, "%x"u8, "<nil>"u8),
    new("<nil>"u8, "%#x"u8, "<nil>"u8),
    new("<nil>"u8, "%#y"u8, "%!y(big.Int=<nil>)"u8),
    new("10"u8, "%b"u8, "1010"u8),
    new("10"u8, "%o"u8, "12"u8),
    new("10"u8, "%d"u8, "10"u8),
    new("10"u8, "%v"u8, "10"u8),
    new("10"u8, "%x"u8, "a"u8),
    new("10"u8, "%X"u8, "A"u8),
    new("-10"u8, "%X"u8, "-A"u8),
    new("10"u8, "%y"u8, "%!y(big.Int=10)"u8),
    new("-10"u8, "%y"u8, "%!y(big.Int=-10)"u8),
    new("10"u8, "%#b"u8, "0b1010"u8),
    new("10"u8, "%#o"u8, "012"u8),
    new("10"u8, "%O"u8, "0o12"u8),
    new("-10"u8, "%#b"u8, "-0b1010"u8),
    new("-10"u8, "%#o"u8, "-012"u8),
    new("-10"u8, "%O"u8, "-0o12"u8),
    new("10"u8, "%#d"u8, "10"u8),
    new("10"u8, "%#v"u8, "10"u8),
    new("10"u8, "%#x"u8, "0xa"u8),
    new("10"u8, "%#X"u8, "0XA"u8),
    new("-10"u8, "%#X"u8, "-0XA"u8),
    new("10"u8, "%#y"u8, "%!y(big.Int=10)"u8),
    new("-10"u8, "%#y"u8, "%!y(big.Int=-10)"u8),
    new("1234"u8, "%d"u8, "1234"u8),
    new("1234"u8, "%3d"u8, "1234"u8),
    new("1234"u8, "%4d"u8, "1234"u8),
    new("-1234"u8, "%d"u8, "-1234"u8),
    new("1234"u8, "% 5d"u8, " 1234"u8),
    new("1234"u8, "%+5d"u8, "+1234"u8),
    new("1234"u8, "%-5d"u8, "1234 "u8),
    new("1234"u8, "%x"u8, "4d2"u8),
    new("1234"u8, "%X"u8, "4D2"u8),
    new("-1234"u8, "%3x"u8, "-4d2"u8),
    new("-1234"u8, "%4x"u8, "-4d2"u8),
    new("-1234"u8, "%5x"u8, " -4d2"u8),
    new("-1234"u8, "%-5x"u8, "-4d2 "u8),
    new("1234"u8, "%03d"u8, "1234"u8),
    new("1234"u8, "%04d"u8, "1234"u8),
    new("1234"u8, "%05d"u8, "01234"u8),
    new("1234"u8, "%06d"u8, "001234"u8),
    new("-1234"u8, "%06d"u8, "-01234"u8),
    new("1234"u8, "%+06d"u8, "+01234"u8),
    new("1234"u8, "% 06d"u8, " 01234"u8),
    new("1234"u8, "%-6d"u8, "1234  "u8),
    new("1234"u8, "%-06d"u8, "1234  "u8),
    new("-1234"u8, "%-06d"u8, "-1234 "u8),
    new("1234"u8, "%.3d"u8, "1234"u8),
    new("1234"u8, "%.4d"u8, "1234"u8),
    new("1234"u8, "%.5d"u8, "01234"u8),
    new("1234"u8, "%.6d"u8, "001234"u8),
    new("-1234"u8, "%.3d"u8, "-1234"u8),
    new("-1234"u8, "%.4d"u8, "-1234"u8),
    new("-1234"u8, "%.5d"u8, "-01234"u8),
    new("-1234"u8, "%.6d"u8, "-001234"u8),
    new("1234"u8, "%8.3d"u8, "    1234"u8),
    new("1234"u8, "%8.4d"u8, "    1234"u8),
    new("1234"u8, "%8.5d"u8, "   01234"u8),
    new("1234"u8, "%8.6d"u8, "  001234"u8),
    new("-1234"u8, "%8.3d"u8, "   -1234"u8),
    new("-1234"u8, "%8.4d"u8, "   -1234"u8),
    new("-1234"u8, "%8.5d"u8, "  -01234"u8),
    new("-1234"u8, "%8.6d"u8, " -001234"u8),
    new("1234"u8, "%+8.3d"u8, "   +1234"u8),
    new("1234"u8, "%+8.4d"u8, "   +1234"u8),
    new("1234"u8, "%+8.5d"u8, "  +01234"u8),
    new("1234"u8, "%+8.6d"u8, " +001234"u8),
    new("-1234"u8, "%+8.3d"u8, "   -1234"u8),
    new("-1234"u8, "%+8.4d"u8, "   -1234"u8),
    new("-1234"u8, "%+8.5d"u8, "  -01234"u8),
    new("-1234"u8, "%+8.6d"u8, " -001234"u8),
    new("1234"u8, "% 8.3d"u8, "    1234"u8),
    new("1234"u8, "% 8.4d"u8, "    1234"u8),
    new("1234"u8, "% 8.5d"u8, "   01234"u8),
    new("1234"u8, "% 8.6d"u8, "  001234"u8),
    new("-1234"u8, "% 8.3d"u8, "   -1234"u8),
    new("-1234"u8, "% 8.4d"u8, "   -1234"u8),
    new("-1234"u8, "% 8.5d"u8, "  -01234"u8),
    new("-1234"u8, "% 8.6d"u8, " -001234"u8),
    new("1234"u8, "%.3x"u8, "4d2"u8),
    new("1234"u8, "%.4x"u8, "04d2"u8),
    new("1234"u8, "%.5x"u8, "004d2"u8),
    new("1234"u8, "%.6x"u8, "0004d2"u8),
    new("-1234"u8, "%.3x"u8, "-4d2"u8),
    new("-1234"u8, "%.4x"u8, "-04d2"u8),
    new("-1234"u8, "%.5x"u8, "-004d2"u8),
    new("-1234"u8, "%.6x"u8, "-0004d2"u8),
    new("1234"u8, "%8.3x"u8, "     4d2"u8),
    new("1234"u8, "%8.4x"u8, "    04d2"u8),
    new("1234"u8, "%8.5x"u8, "   004d2"u8),
    new("1234"u8, "%8.6x"u8, "  0004d2"u8),
    new("-1234"u8, "%8.3x"u8, "    -4d2"u8),
    new("-1234"u8, "%8.4x"u8, "   -04d2"u8),
    new("-1234"u8, "%8.5x"u8, "  -004d2"u8),
    new("-1234"u8, "%8.6x"u8, " -0004d2"u8),
    new("1234"u8, "%+8.3x"u8, "    +4d2"u8),
    new("1234"u8, "%+8.4x"u8, "   +04d2"u8),
    new("1234"u8, "%+8.5x"u8, "  +004d2"u8),
    new("1234"u8, "%+8.6x"u8, " +0004d2"u8),
    new("-1234"u8, "%+8.3x"u8, "    -4d2"u8),
    new("-1234"u8, "%+8.4x"u8, "   -04d2"u8),
    new("-1234"u8, "%+8.5x"u8, "  -004d2"u8),
    new("-1234"u8, "%+8.6x"u8, " -0004d2"u8),
    new("1234"u8, "% 8.3x"u8, "     4d2"u8),
    new("1234"u8, "% 8.4x"u8, "    04d2"u8),
    new("1234"u8, "% 8.5x"u8, "   004d2"u8),
    new("1234"u8, "% 8.6x"u8, "  0004d2"u8),
    new("1234"u8, "% 8.7x"u8, " 00004d2"u8),
    new("1234"u8, "% 8.8x"u8, " 000004d2"u8),
    new("-1234"u8, "% 8.3x"u8, "    -4d2"u8),
    new("-1234"u8, "% 8.4x"u8, "   -04d2"u8),
    new("-1234"u8, "% 8.5x"u8, "  -004d2"u8),
    new("-1234"u8, "% 8.6x"u8, " -0004d2"u8),
    new("-1234"u8, "% 8.7x"u8, "-00004d2"u8),
    new("-1234"u8, "% 8.8x"u8, "-000004d2"u8),
    new("1234"u8, "%-8.3d"u8, "1234    "u8),
    new("1234"u8, "%-8.4d"u8, "1234    "u8),
    new("1234"u8, "%-8.5d"u8, "01234   "u8),
    new("1234"u8, "%-8.6d"u8, "001234  "u8),
    new("1234"u8, "%-8.7d"u8, "0001234 "u8),
    new("1234"u8, "%-8.8d"u8, "00001234"u8),
    new("-1234"u8, "%-8.3d"u8, "-1234   "u8),
    new("-1234"u8, "%-8.4d"u8, "-1234   "u8),
    new("-1234"u8, "%-8.5d"u8, "-01234  "u8),
    new("-1234"u8, "%-8.6d"u8, "-001234 "u8),
    new("-1234"u8, "%-8.7d"u8, "-0001234"u8),
    new("-1234"u8, "%-8.8d"u8, "-00001234"u8),
    new("16777215"u8, "%b"u8, "111111111111111111111111"u8),
    new("0"u8, "%.d"u8, ""u8),
    new("0"u8, "%.0d"u8, ""u8),
    new("0"u8, "%3.d"u8, ""u8)
}.slice();

public static void TestFormat(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in formatTests) {
        ж<global::go.math.big_package.ΔInt> x = default!;
        if (test.input != "<nil>"u8) {
            bool ok = default!;
            (x, ok) = @new<global::go.math.big_package.ΔInt>().SetString(test.input, 0);
            if (!ok) {
                Ꮡt.Errorf("#%d failed reading input %s"u8, i, test.input);
            }
        }
        @string output = fmt.Sprintf(test.format, x.OrTypedNil());
        if (output != test.output) {
            Ꮡt.Errorf("#%d got %q; want %q, {%q, %q, %q}"u8, i, output, test.output, test.input, test.format, test.output);
        }
    }
}


partial struct scanTestsᴛ1 /*dyn*/ {
    internal @string input;
    internal @string format;
    internal @string output;
    internal nint remaining;
}
internal static slice<scanTestsᴛ1> scanTests = new scanTestsᴛ1[]{
    new("1010"u8, "%b"u8, "10"u8, 0),
    new("0b1010"u8, "%v"u8, "10"u8, 0),
    new("12"u8, "%o"u8, "10"u8, 0),
    new("012"u8, "%v"u8, "10"u8, 0),
    new("10"u8, "%d"u8, "10"u8, 0),
    new("10"u8, "%v"u8, "10"u8, 0),
    new("a"u8, "%x"u8, "10"u8, 0),
    new("0xa"u8, "%v"u8, "10"u8, 0),
    new("A"u8, "%X"u8, "10"u8, 0),
    new("-A"u8, "%X"u8, "-10"u8, 0),
    new("+0b1011001"u8, "%v"u8, "89"u8, 0),
    new("0xA"u8, "%v"u8, "10"u8, 0),
    new("0 "u8, "%v"u8, "0"u8, 1),
    new("2+3"u8, "%v"u8, "2"u8, 2),
    new("0XABC 12"u8, "%v"u8, "2748"u8, 3)
}.slice();

public static void TestScan(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (i, test) in scanTests) {
        var x = @new<global::go.math.big_package.ΔInt>();
        buf.Reset();
        buf.WriteString(test.input);
        {
            var (_, err) = fmt.Fscanf(new big_test_package.bytes_BufferжReader(Ꮡbuf), test.format, x.OrTypedNil()); if (err != default!) {
                Ꮡt.Errorf("#%d error: %s"u8, i, err);
            }
        }
        if (x.String() != test.output) {
            Ꮡt.Errorf("#%d got %s; want %s"u8, i, x.String(), test.output);
        }
        if (buf.Len() != test.remaining) {
            Ꮡt.Errorf("#%d got %d bytes remaining; want %d"u8, i, buf.Len(), test.remaining);
        }
    }
}

} // end big_internal_test_package
