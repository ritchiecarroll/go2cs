// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.unicode;

using bytes = bytes_package;
using strings = strings_package;
using testing = testing_package;
using unicode = unicode_package;
using static go.unicode.utf8_package;

partial class utf8_test_package {

// Validate the constants redefined from unicode.
public static void TestConstants(ж<testing.T> t) {
    if (MaxRune != unicode.MaxRune) {
        t.Errorf("utf8.MaxRune is wrong: %x should be %x", (int32)(MaxRune), (int32)(unicode.MaxRune));
    }
    if (RuneError != unicode.ReplacementChar) {
        t.Errorf("utf8.RuneError is wrong: %x should be %x", (int32)(RuneError), (int32)(unicode.ReplacementChar));
    }
}

partial struct Utf8Map {
    internal rune r;
    internal @string str;
}

internal static slice<Utf8Map> utf8map = slice<Utf8Map>([
    new(0x0000, "\x00"),
    new(0x0001, "\x01"),
    new(0x007e, "\x7e"),
    new(0x007f, "\x7f"),
    new(0x0080, ((@string)(new byte[]{0xc2, 0x80}))),
    new(0x0081, ((@string)(new byte[]{0xc2, 0x81}))),
    new(0x00bf, ((@string)(new byte[]{0xc2, 0xbf}))),
    new(0x00c0, ((@string)(new byte[]{0xc3, 0x80}))),
    new(0x00c1, ((@string)(new byte[]{0xc3, 0x81}))),
    new(0x00c8, ((@string)(new byte[]{0xc3, 0x88}))),
    new(0x00d0, ((@string)(new byte[]{0xc3, 0x90}))),
    new(0x00e0, ((@string)(new byte[]{0xc3, 0xa0}))),
    new(0x00f0, ((@string)(new byte[]{0xc3, 0xb0}))),
    new(0x00f8, ((@string)(new byte[]{0xc3, 0xb8}))),
    new(0x00ff, ((@string)(new byte[]{0xc3, 0xbf}))),
    new(0x0100, ((@string)(new byte[]{0xc4, 0x80}))),
    new(0x07ff, ((@string)(new byte[]{0xdf, 0xbf}))),
    new(0x0400, ((@string)(new byte[]{0xd0, 0x80}))),
    new(0x0800, ((@string)(new byte[]{0xe0, 0xa0, 0x80}))),
    new(0x0801, ((@string)(new byte[]{0xe0, 0xa0, 0x81}))),
    new(0x1000, ((@string)(new byte[]{0xe1, 0x80, 0x80}))),
    new(0xd000, ((@string)(new byte[]{0xed, 0x80, 0x80}))),
    new(0xd7ff, ((@string)(new byte[]{0xed, 0x9f, 0xbf}))), // last code point before surrogate half.
    new(0xe000, ((@string)(new byte[]{0xee, 0x80, 0x80}))), // first code point after surrogate half.
    new(0xfffe, ((@string)(new byte[]{0xef, 0xbf, 0xbe}))),
    new(0xffff, ((@string)(new byte[]{0xef, 0xbf, 0xbf}))),
    new(0x10000, ((@string)(new byte[]{0xf0, 0x90, 0x80, 0x80}))),
    new(0x10001, ((@string)(new byte[]{0xf0, 0x90, 0x80, 0x81}))),
    new(0x40000, ((@string)(new byte[]{0xf1, 0x80, 0x80, 0x80}))),
    new(0x10fffe, ((@string)(new byte[]{0xf4, 0x8f, 0xbf, 0xbe}))),
    new(0x10ffff, ((@string)(new byte[]{0xf4, 0x8f, 0xbf, 0xbf}))),
    new(0xFFFD, ((@string)(new byte[]{0xef, 0xbf, 0xbd})))
]);

internal static slice<Utf8Map> surrogateMap = slice<Utf8Map>([
    new(0xd800, ((@string)(new byte[]{0xed, 0xa0, 0x80}))), // surrogate min decodes to (RuneError, 1)
    new(0xdfff, ((@string)(new byte[]{0xed, 0xbf, 0xbf})))  // surrogate max decodes to (RuneError, 1)
]);

internal static slice<@string> testStrings = slice<@string>([
    "",
    "abcd",
    "☺☻☹",
    "日a本b語ç日ð本Ê語þ日¥本¼語i日©",
    "日a本b語ç日ð本Ê語þ日¥本¼語i日©日a本b語ç日ð本Ê語þ日¥本¼語i日©日a本b語ç日ð本Ê語þ日¥本¼語i日©",
    ((@string)(new byte[]{0x80, 0x80, 0x80, 0x80}))
]);

public static void TestFullRune(ж<testing.T> t) {
    foreach (var (_, m) in utf8map) {
        var b = slice<byte>(m.str);
        if (!FullRune(b)) {
            t.Errorf("FullRune(%q) (%U) = false, want true", b, m.r);
        }
        @string s = m.str;
        if (!FullRuneInString(s)) {
            t.Errorf("FullRuneInString(%q) (%U) = false, want true", s, m.r);
        }
        var b1 = b.slice(0, len(b) - 1);
        if (FullRune(b1)) {
            t.Errorf("FullRune(%q) = true, want false", b1);
        }
        @string s1 = (@string)b1;
        if (FullRuneInString(s1)) {
            t.Errorf("FullRune(%q) = true, want false", s1);
        }
    }
    foreach (var (_, s) in slice<@string>([((@string)(new byte[]{0xc0})), ((@string)(new byte[]{0xc1}))])) {
        var b = slice<byte>(s);
        if (!FullRune(b)) {
            t.Errorf("FullRune(%q) = false, want true", s);
        }
        if (!FullRuneInString(s)) {
            t.Errorf("FullRuneInString(%q) = false, want true", s);
        }
    }
}

public static void TestEncodeRune(ж<testing.T> t) {
    foreach (var (_, m) in utf8map) {
        var b = slice<byte>(m.str);
        array<byte> buf = new(10);
        nint n = EncodeRune(buf[0..], m.r);
        var b1 = buf.slice(0, n);
        if (!bytes.Equal(b, b1)) {
            t.Errorf("EncodeRune(%#04x) = %q want %q", m.r, b1, b);
        }
    }
}

public static void TestAppendRune(ж<testing.T> t) {
    foreach (var (_, m) in utf8map) {
        {
            var buf = AppendRune(default, m.r); if (((sstring)buf) != m.str) {
                t.Errorf("AppendRune(nil, %#04x) = %s, want %s", m.r, buf, m.str);
            }
        }
        {
            var buf = AppendRune(slice<byte>("init"u8), m.r); if (((@string)buf) != "init"u8 + m.str) {
                t.Errorf("AppendRune(init, %#04x) = %s, want %s", m.r, buf, "init" + m.str);
            }
        }
    }
}

public static void TestDecodeRune(ж<testing.T> t) {
    foreach (var (_, m) in utf8map) {
        var b = slice<byte>(m.str);
        var (r, size) = DecodeRune(b);
        if (r != m.r || size != len(b)) {
            t.Errorf("DecodeRune(%q) = %#04x, %d want %#04x, %d", b, r, size, m.r, len(b));
        }
        @string s = m.str;
        (r, size) = DecodeRuneInString(s);
        if (r != m.r || size != len(b)) {
            t.Errorf("DecodeRuneInString(%q) = %#04x, %d want %#04x, %d", s, r, size, m.r, len(b));
        }

        // there's an extra byte that bytes left behind - make sure trailing byte works
        (r, size) = DecodeRune(b.slice(0, cap(b)));
        if (r != m.r || size != len(b)) {
            t.Errorf("DecodeRune(%q) = %#04x, %d want %#04x, %d", b, r, size, m.r, len(b));
        }
        s = m.str + "\x00"u8;
        (r, size) = DecodeRuneInString(s);
        if (r != m.r || size != len(b)) {
            t.Errorf("DecodeRuneInString(%q) = %#04x, %d want %#04x, %d", s, r, size, m.r, len(b));
        }

        // make sure missing bytes fail
        nint wantsize = 1;
        if (wantsize >= len(b)) {
            wantsize = 0;
        }
        (r, size) = DecodeRune(b.slice(0, len(b) - 1));
        if (r != RuneError || size != wantsize) {
            t.Errorf("DecodeRune(%q) = %#04x, %d want %#04x, %d", b.slice(0, len(b) - 1), r, size, (int32)(RuneError), wantsize);
        }
        s = m.str.slice(0, len(m.str) - 1);
        (r, size) = DecodeRuneInString(s);
        if (r != RuneError || size != wantsize) {
            t.Errorf("DecodeRuneInString(%q) = %#04x, %d want %#04x, %d", s, r, size, (int32)(RuneError), wantsize);
        }

        // make sure bad sequences fail
        if (len(b) == 1) {
            b[0] = 0x80;
        } else {
            b[len(b) - 1] = 0x7F;
        }
        (r, size) = DecodeRune(b);
        if (r != RuneError || size != 1) {
            t.Errorf("DecodeRune(%q) = %#04x, %d want %#04x, %d", b, r, size, (int32)(RuneError), (nint)1);
        }
        s = (@string)b;
        (r, size) = DecodeRuneInString(s);
        if (r != RuneError || size != 1) {
            t.Errorf("DecodeRuneInString(%q) = %#04x, %d want %#04x, %d", s, r, size, (int32)(RuneError), (nint)1);
        }
    }
}

public static void TestDecodeSurrogateRune(ж<testing.T> t) {
    foreach (var (_, m) in surrogateMap) {
        var b = slice<byte>(m.str);
        var (r, size) = DecodeRune(b);
        if (r != RuneError || size != 1) {
            t.Errorf("DecodeRune(%q) = %x, %d want %x, %d", b, r, size, (int32)(RuneError), (nint)1);
        }
        @string s = m.str;
        (r, size) = DecodeRuneInString(s);
        if (r != RuneError || size != 1) {
            t.Errorf("DecodeRuneInString(%q) = %x, %d want %x, %d", b, r, size, (int32)(RuneError), (nint)1);
        }
    }
}

// Check that DecodeRune and DecodeLastRune correspond to
// the equivalent range loop.
public static void TestSequencing(ж<testing.T> t) {
    foreach (var (_, ts) in testStrings) {
        foreach (var (_, m) in utf8map) {
            foreach (var (_, s) in slice<@string>([ts + m.str, m.str + ts, ts + m.str + ts])) {
                testSequence(t, s);
            }
        }
    }
}

internal static nint runtimeRuneCount(@string s) {
    return len(slice<rune>(s)); // Replaced by gc with call to runtime.countrunes(s).
}

// Check that a range loop, len([]rune(string)) optimization and
// []rune conversions visit the same runes.
// Not really a test of this package, but the assumption is used here and
// it's good to verify.
public static void TestRuntimeConversion(ж<testing.T> t) {
    foreach (var (_, ts) in testStrings) {
        nint count = RuneCountInString(ts);
        {
            nint n = runtimeRuneCount(ts); if (n != count) {
                t.Errorf("%q: len([]rune()) counted %d runes; got %d from RuneCountInString", ts, n, count);
                break;
            }
        }

        var runes = slice<rune>(ts);
        {
            nint n = len(runes); if (n != count) {
                t.Errorf("%q: []rune() has length %d; got %d from RuneCountInString", ts, n, count);
                break;
            }
        }
        nint i = 0;
        foreach (var (_, r) in ts) {
            if (r != runes[i]) {
                t.Errorf("%q[%d]: expected %c (%U); got %c (%U)", ts, i, runes[i], runes[i], r, r);
            }
            i++;
        }
    }
}

internal static slice<@string> invalidSequenceTests = slice<@string>([
    ((@string)(new byte[]{0xed, 0xa0, 0x80, 0x80})), // surrogate min
    ((@string)(new byte[]{0xed, 0xbf, 0xbf, 0x80})), // surrogate max

    // xx
    ((@string)(new byte[]{0x91, 0x80, 0x80, 0x80})),

    // s1
    ((@string)(new byte[]{0xc2, 0x7f, 0x80, 0x80})),
    ((@string)(new byte[]{0xc2, 0xc0, 0x80, 0x80})),
    ((@string)(new byte[]{0xdf, 0x7f, 0x80, 0x80})),
    ((@string)(new byte[]{0xdf, 0xc0, 0x80, 0x80})),

    // s2
    ((@string)(new byte[]{0xe0, 0x9f, 0xbf, 0x80})),
    ((@string)(new byte[]{0xe0, 0xa0, 0x7f, 0x80})),
    ((@string)(new byte[]{0xe0, 0xbf, 0xc0, 0x80})),
    ((@string)(new byte[]{0xe0, 0xc0, 0x80, 0x80})),

    // s3
    ((@string)(new byte[]{0xe1, 0x7f, 0xbf, 0x80})),
    ((@string)(new byte[]{0xe1, 0x80, 0x7f, 0x80})),
    ((@string)(new byte[]{0xe1, 0xbf, 0xc0, 0x80})),
    ((@string)(new byte[]{0xe1, 0xc0, 0x80, 0x80})),

    //s4
    ((@string)(new byte[]{0xed, 0x7f, 0xbf, 0x80})),
    ((@string)(new byte[]{0xed, 0x80, 0x7f, 0x80})),
    ((@string)(new byte[]{0xed, 0x9f, 0xc0, 0x80})),
    ((@string)(new byte[]{0xed, 0xa0, 0x80, 0x80})),

    // s5
    ((@string)(new byte[]{0xf0, 0x8f, 0xbf, 0xbf})),
    ((@string)(new byte[]{0xf0, 0x90, 0x7f, 0xbf})),
    ((@string)(new byte[]{0xf0, 0x90, 0x80, 0x7f})),
    ((@string)(new byte[]{0xf0, 0xbf, 0xbf, 0xc0})),
    ((@string)(new byte[]{0xf0, 0xbf, 0xc0, 0x80})),
    ((@string)(new byte[]{0xf0, 0xc0, 0x80, 0x80})),

    // s6
    ((@string)(new byte[]{0xf1, 0x7f, 0xbf, 0xbf})),
    ((@string)(new byte[]{0xf1, 0x80, 0x7f, 0xbf})),
    ((@string)(new byte[]{0xf1, 0x80, 0x80, 0x7f})),
    ((@string)(new byte[]{0xf1, 0xbf, 0xbf, 0xc0})),
    ((@string)(new byte[]{0xf1, 0xbf, 0xc0, 0x80})),
    ((@string)(new byte[]{0xf1, 0xc0, 0x80, 0x80})),

    // s7
    ((@string)(new byte[]{0xf4, 0x7f, 0xbf, 0xbf})),
    ((@string)(new byte[]{0xf4, 0x80, 0x7f, 0xbf})),
    ((@string)(new byte[]{0xf4, 0x80, 0x80, 0x7f})),
    ((@string)(new byte[]{0xf4, 0x8f, 0xbf, 0xc0})),
    ((@string)(new byte[]{0xf4, 0x8f, 0xc0, 0x80})),
    ((@string)(new byte[]{0xf4, 0x90, 0x80, 0x80}))
]);

internal static rune runtimeDecodeRune(@string s) {
    foreach (var (_, r) in s) {
        return r;
    }
    return -1;
}

public static void TestDecodeInvalidSequence(ж<testing.T> t) {
    foreach (var (_, s) in invalidSequenceTests) {
        var (r1, _) = DecodeRune(slice<byte>(s));
        {
            rune want = RuneError; if (r1 != want) {
                t.Errorf("DecodeRune(%#x) = %#04x, want %#04x", s, r1, want);
                return;
            }
        }
        var (r2, _) = DecodeRuneInString(s);
        {
            rune want = RuneError; if (r2 != want) {
                t.Errorf("DecodeRuneInString(%q) = %#04x, want %#04x", s, r2, want);
                return;
            }
        }
        if (r1 != r2) {
            t.Errorf("DecodeRune(%#x) = %#04x mismatch with DecodeRuneInString(%q) = %#04x", s, r1, s, r2);
            return;
        }
        var r3 = runtimeDecodeRune(s);
        if (r2 != r3) {
            t.Errorf("DecodeRuneInString(%q) = %#04x mismatch with runtime.decoderune(%q) = %#04x", s, r2, s, r3);
            return;
        }
    }
}

internal partial struct testSequence_info /*dyn*/ {
    internal nint index;
    internal rune r;
}

internal static void testSequence(ж<testing.T> t, @string s) {
    var index = new slice<testSequence_info>(len(s));
    var b = slice<byte>(s);
    nint si = 0;
    nint j = 0;
    foreach (var (i, r) in s) {
        if (si != i) {
            t.Errorf("Sequence(%q) mismatched index %d, want %d", s, si, i);
            return;
        }
        index[j] = new testSequence_info(i, r);
        j++;
        var (r1, size1) = DecodeRune(b.slice(i));
        if (r != r1) {
            t.Errorf("DecodeRune(%q) = %#04x, want %#04x", s.slice(i), r1, r);
            return;
        }
        var (r2, size2) = DecodeRuneInString(s.slice(i));
        if (r != r2) {
            t.Errorf("DecodeRuneInString(%q) = %#04x, want %#04x", s.slice(i), r2, r);
            return;
        }
        if (size1 != size2) {
            t.Errorf("DecodeRune/DecodeRuneInString(%q) size mismatch %d/%d", s.slice(i), size1, size2);
            return;
        }
        si += size1;
    }
    j--;
    for (si = len(s); si > 0; ) {
        var (r1, size1) = DecodeLastRune(b.slice(0, si));
        var (r2, size2) = DecodeLastRuneInString(s.slice(0, si));
        if (size1 != size2) {
            t.Errorf("DecodeLastRune/DecodeLastRuneInString(%q, %d) size mismatch %d/%d", s, si, size1, size2);
            return;
        }
        if (r1 != index[j].r) {
            t.Errorf("DecodeLastRune(%q, %d) = %#04x, want %#04x", s, si, r1, index[j].r);
            return;
        }
        if (r2 != index[j].r) {
            t.Errorf("DecodeLastRuneInString(%q, %d) = %#04x, want %#04x", s, si, r2, index[j].r);
            return;
        }
        si -= size1;
        if (si != index[j].index) {
            t.Errorf("DecodeLastRune(%q) index mismatch at %d, want %d", s, si, index[j].index);
            return;
        }
        j--;
    }
    if (si != 0) {
        t.Errorf("DecodeLastRune(%q) finished at %d, not 0", s, si);
    }
}

// Check that negative runes encode as U+FFFD.
public static void TestNegativeRune(ж<testing.T> t) {
    var errorbuf = new slice<byte>(UTFMax);
    errorbuf = errorbuf.slice(0, EncodeRune(errorbuf, RuneError));
    var buf = new slice<byte>(UTFMax);
    buf = buf.slice(0, EncodeRune(buf, -1));
    if (!bytes.Equal(buf, errorbuf)) {
        t.Errorf("incorrect encoding [% x] for -1; expected [% x]", buf, errorbuf);
    }
}

partial struct RuneCountTest {
    internal @string @in;
    internal nint @out;
}

internal static slice<RuneCountTest> runecounttests = slice<RuneCountTest>([
    new("abcd", 4),
    new("☺☻☹", 3),
    new("1,2,3,4", 7),
    new(((@string)(new byte[]{0xe2, 0x00})), 2),
    new(((@string)(new byte[]{0xe2, 0x80})), 2),
    new(((@string)(new byte[]{0x61, 0xe2, 0x80})), 3)
]);

public static void TestRuneCount(ж<testing.T> t) {
    foreach (var (_, tt) in runecounttests) {
        {
            nint @out = RuneCountInString(tt.@in); if (@out != tt.@out) {
                t.Errorf("RuneCountInString(%q) = %d, want %d", tt.@in, @out, tt.@out);
            }
        }
        {
            nint @out = RuneCount(slice<byte>(tt.@in)); if (@out != tt.@out) {
                t.Errorf("RuneCount(%q) = %d, want %d", tt.@in, @out, tt.@out);
            }
        }
    }
}

public static void TestRuneCountNonASCIIAllocation(ж<testing.T> t) {
    {
        var n = testing.AllocsPerRun(10, () => {
            var s = slice<byte>("日本語日本語日本語日"u8);
            _ = RuneCount(s);
        }); if (n > 0D) {
            t.Errorf("unexpected RuneCount allocation, got %v, want 0", n);
        }
    }
}

partial struct RuneLenTest {
    internal rune r;
    internal nint size;
}

internal static slice<RuneLenTest> runelentests = slice<RuneLenTest>([
    new(0, 1),
    new('e', 1),
    new('é', 2),
    new('☺', 3),
    new(RuneError, 3),
    new(MaxRune, 4),
    new(0xD800, -1),
    new(0xDFFF, -1),
    new(MaxRune + 1, -1),
    new(-1, -1)
]);

public static void TestRuneLen(ж<testing.T> t) {
    foreach (var (_, tt) in runelentests) {
        {
            nint size = RuneLen(tt.r); if (size != tt.size) {
                t.Errorf("RuneLen(%#U) = %d, want %d", tt.r, size, tt.size);
            }
        }
    }
}

partial struct ValidTest {
    internal @string @in;
    internal bool @out;
}

internal static slice<ValidTest> validTests = slice<ValidTest>([
    new("", true),
    new("a", true),
    new("abc", true),
    new("Ж", true),
    new("ЖЖ", true),
    new("брэд-ЛГТМ", true),
    new("☺☻☹", true),
    new(((@string)(new byte[]{0x61, 0x61, 0xe2})), false),
    new((@string)slice<byte>([66, 250]), false),
    new((@string)slice<byte>([66, 250, 67]), false),
    new("a\uFFFDb", true),
    new((@string)((@string)(new byte[]{0xf4, 0x8f, 0xbf, 0xbf})), true),        // U+10FFFF
    new((@string)((@string)(new byte[]{0xf4, 0x90, 0x80, 0x80})), false),       // U+10FFFF+1; out of range
    new((@string)((@string)(new byte[]{0xf7, 0xbf, 0xbf, 0xbf})), false),       // 0x1FFFFF; out of range
    new((@string)((@string)(new byte[]{0xfb, 0xbf, 0xbf, 0xbf, 0xbf})), false), // 0x3FFFFFF; out of range
    new((@string)((@string)(new byte[]{0xc0, 0x80})), false),                   // U+0000 encoded in two bytes: incorrect
    new((@string)((@string)(new byte[]{0xed, 0xa0, 0x80})), false),             // U+D800 high surrogate (sic)
    new((@string)((@string)(new byte[]{0xed, 0xbf, 0xbf})), false)              // U+DFFF low surrogate (sic)
]);

[GoInit] internal static void init() {
    foreach (var i in range(100)) {
        validTests = append(validTests, new ValidTest(@in: strings.Repeat("a", i), @out: true));
        validTests = append(validTests, new ValidTest(@in: strings.Repeat("a", i) + "Ж"u8, @out: true));
        validTests = append(validTests, new ValidTest(@in: strings.Repeat("a", i) + ((@string)(new byte[]{0xe2})), @out: false));
        validTests = append(validTests, new ValidTest(@in: strings.Repeat("a", i) + "Ж"u8 + strings.Repeat("b", i), @out: true));
        validTests = append(validTests, new ValidTest(@in: strings.Repeat("a", i) + ((@string)(new byte[]{0xe2})) + strings.Repeat("b", i), @out: false));
    }
}

public static void TestValid(ж<testing.T> t) {
    foreach (var (_, tt) in validTests) {
        if (Valid(slice<byte>(tt.@in)) != tt.@out) {
            t.Errorf("Valid(%q) = %v; want %v", tt.@in, !tt.@out, tt.@out);
        }
        if (ValidString(tt.@in) != tt.@out) {
            t.Errorf("ValidString(%q) = %v; want %v", tt.@in, !tt.@out, tt.@out);
        }
    }
}

partial struct ValidRuneTest {
    internal rune r;
    internal bool ok;
}

internal static slice<ValidRuneTest> validrunetests = slice<ValidRuneTest>([
    new(0, true),
    new('e', true),
    new('é', true),
    new('☺', true),
    new(RuneError, true),
    new(MaxRune, true),
    new(0xD7FF, true),
    new(0xD800, false),
    new(0xDFFF, false),
    new(0xE000, true),
    new(MaxRune + 1, false),
    new(-1, false)
]);

public static void TestValidRune(ж<testing.T> t) {
    foreach (var (_, tt) in validrunetests) {
        {
            var ok = ValidRune(tt.r); if (ok != tt.ok) {
                t.Errorf("ValidRune(%#U) = %t, want %t", tt.r, ok, tt.ok);
            }
        }
    }
}

public static void BenchmarkRuneCountTenASCIIChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var s = slice<byte>("0123456789"u8);
    for (nint i = 0; i < b.N; i++) {
        RuneCount(s);
    }
}

public static void BenchmarkRuneCountTenJapaneseChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var s = slice<byte>("日本語日本語日本語日"u8);
    for (nint i = 0; i < b.N; i++) {
        RuneCount(s);
    }
}

public static void BenchmarkRuneCountInStringTenASCIIChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        RuneCountInString("0123456789"u8);
    }
}

public static void BenchmarkRuneCountInStringTenJapaneseChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        RuneCountInString("日本語日本語日本語日"u8);
    }
}

internal static @string ascii100000 = strings.Repeat("0123456789", 10000);

public static void BenchmarkValidTenASCIIChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var s = slice<byte>("0123456789"u8);
    for (nint i = 0; i < b.N; i++) {
        Valid(s);
    }
}

public static void BenchmarkValid100KASCIIChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var s = slice<byte>(ascii100000);
    for (nint i = 0; i < b.N; i++) {
        Valid(s);
    }
}

public static void BenchmarkValidTenJapaneseChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var s = slice<byte>("日本語日本語日本語日"u8);
    for (nint i = 0; i < b.N; i++) {
        Valid(s);
    }
}

public static void BenchmarkValidLongMostlyASCII(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var longMostlyASCII = slice<byte>(longStringMostlyASCII);
    for (nint i = 0; i < b.N; i++) {
        Valid(longMostlyASCII);
    }
}

public static void BenchmarkValidLongJapanese(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var longJapanese = slice<byte>(longStringJapanese);
    for (nint i = 0; i < b.N; i++) {
        Valid(longJapanese);
    }
}

public static void BenchmarkValidStringTenASCIIChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ValidString("0123456789");
    }
}

public static void BenchmarkValidString100KASCIIChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ValidString(ascii100000);
    }
}

public static void BenchmarkValidStringTenJapaneseChars(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ValidString("日本語日本語日本語日");
    }
}

public static void BenchmarkValidStringLongMostlyASCII(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ValidString(longStringMostlyASCII);
    }
}

public static void BenchmarkValidStringLongJapanese(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        ValidString(longStringJapanese);
    }
}

internal static @string longStringMostlyASCII; // ~100KB, ~97% ASCII

internal static @string longStringJapanese;    // ~100KB, non-ASCII

[GoInit] internal static void initΔ1() {
    @string japanese = "日本語日本語日本語日";
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    for (nint i = 0; b.Len() < 100_000; i++) {
        if (i % 100 == 0) {
            Ꮡb.WriteString(japanese);
        } else {
            Ꮡb.WriteString("0123456789");
        }
    }
    longStringMostlyASCII = b.String();
    longStringJapanese = strings.Repeat(japanese, 100_000 / len(japanese));
}

public static void BenchmarkEncodeASCIIRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        EncodeRune(buf, 'a'); // 1 byte
    }
}

public static void BenchmarkEncodeSpanishRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        EncodeRune(buf, 'Ñ'); // 2 bytes
    }
}

public static void BenchmarkEncodeJapaneseRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        EncodeRune(buf, '本'); // 3 bytes
    }
}

public static void BenchmarkEncodeMaxRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        EncodeRune(buf, MaxRune); // 4 bytes
    }
}

public static void BenchmarkEncodeInvalidRuneMaxPlusOne(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        EncodeRune(buf, MaxRune + 1); // 3 bytes: RuneError
    }
}

public static void BenchmarkEncodeInvalidRuneSurrogate(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        EncodeRune(buf, 0xD800); // 3 bytes: RuneError
    }
}

public static void BenchmarkEncodeInvalidRuneNegative(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        EncodeRune(buf, -1); // 3 bytes: RuneError
    }
}

public static void BenchmarkAppendASCIIRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        AppendRune(buf[..0], 'a'); // 1 byte
    }
}

public static void BenchmarkAppendSpanishRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        AppendRune(buf[..0], 'Ñ'); // 2 bytes
    }
}

public static void BenchmarkAppendJapaneseRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        AppendRune(buf[..0], '本'); // 3 bytes
    }
}

public static void BenchmarkAppendMaxRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        AppendRune(buf[..0], MaxRune); // 4 bytes
    }
}

public static void BenchmarkAppendInvalidRuneMaxPlusOne(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        AppendRune(buf[..0], MaxRune + 1); // 3 bytes: RuneError
    }
}

public static void BenchmarkAppendInvalidRuneSurrogate(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        AppendRune(buf[..0], 0xD800); // 3 bytes: RuneError
    }
}

public static void BenchmarkAppendInvalidRuneNegative(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        AppendRune(buf[..0], -1); // 3 bytes: RuneError
    }
}

public static void BenchmarkDecodeASCIIRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var a = slice<byte>([(byte)'a']);
    foreach (var _ᴛ1 in range(b.N)) {
        (runeSink, sizeSink) = DecodeRune(a);
    }
}

public static void BenchmarkDecodeJapaneseRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var nihon = slice<byte>("本"u8);
    foreach (var _ᴛ1 in range(b.N)) {
        (runeSink, sizeSink) = DecodeRune(nihon);
    }
}

public static void BenchmarkDecodeASCIIRuneInString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string a = "a";
    foreach (var _ᴛ1 in range(b.N)) {
        (runeSink, sizeSink) = DecodeRuneInString(a);
    }
}

public static void BenchmarkDecodeJapaneseRuneInString(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    @string nihon = "本";
    foreach (var _ᴛ1 in range(b.N)) {
        (runeSink, sizeSink) = DecodeRuneInString(nihon);
    }
}

internal static rune runeSink;
internal static nint sizeSink;

// boolSink is used to reference the return value of benchmarked
// functions to avoid dead code elimination.
internal static bool boolSink;

internal partial struct BenchmarkFullRune_benchmarks /*dyn*/ {
    internal @string name;
    internal slice<byte> data;
}

public static void BenchmarkFullRune(ж<testing.B> b) {
    var benchmarks = slice<BenchmarkFullRune_benchmarks>([
        new("ASCII", slice<byte>("a"u8)),
        new("Incomplete", slice<byte>(((@string)(new byte[]{0xf0, 0x90, 0x80})))),
        new("Japanese", slice<byte>("本"u8))
    ]);
    foreach (var (_, vᴛ1) in benchmarks) {
        ref var bm = ref heap(new BenchmarkFullRune_benchmarks(), out var Ꮡbm);
        bm = vᴛ1;

        var bmʗ1 = bm;
        b.Run(bm.name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                boolSink = FullRune(bmʗ1.data);
            }
        });
    }
}

} // end utf8_test_package
