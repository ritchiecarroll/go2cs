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
[GoInit] internal static void init() {
    if (MaxRune != unicode.MaxRune) {
        throw panic("utf8.MaxRune is wrong");
    }
    if (RuneError != unicode.ReplacementChar) {
        throw panic("utf8.RuneError is wrong");
    }
}

// Validate the constants redefined from unicode.
public static void TestConstants(ж<testing.T> Ꮡt) {
    if (MaxRune != unicode.MaxRune) {
        Ꮡt.Errorf("utf8.MaxRune is wrong: %x should be %x"u8, (int32)(MaxRune), (int32)(unicode.MaxRune));
    }
    if (RuneError != unicode.ReplacementChar) {
        Ꮡt.Errorf("utf8.RuneError is wrong: %x should be %x"u8, (int32)(RuneError), (int32)(unicode.ReplacementChar));
    }
}

partial struct Utf8Map {
    internal rune r;
    internal @string str;
}

// last code point before surrogate half.
// first code point after surrogate half.
internal static slice<Utf8Map> utf8map = new Utf8Map[]{
    new(0x0000, "\x00"u8),
    new(0x0001, "\x01"u8),
    new(0x007e, "\x7e"u8),
    new(0x007f, "\x7f"u8),
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
    new(0xd7ff, ((@string)(new byte[]{0xed, 0x9f, 0xbf}))),
    new(0xe000, ((@string)(new byte[]{0xee, 0x80, 0x80}))),
    new(0xfffe, ((@string)(new byte[]{0xef, 0xbf, 0xbe}))),
    new(0xffff, ((@string)(new byte[]{0xef, 0xbf, 0xbf}))),
    new(0x10000, ((@string)(new byte[]{0xf0, 0x90, 0x80, 0x80}))),
    new(0x10001, ((@string)(new byte[]{0xf0, 0x90, 0x80, 0x81}))),
    new(0x40000, ((@string)(new byte[]{0xf1, 0x80, 0x80, 0x80}))),
    new(0x10fffe, ((@string)(new byte[]{0xf4, 0x8f, 0xbf, 0xbe}))),
    new(0x10ffff, ((@string)(new byte[]{0xf4, 0x8f, 0xbf, 0xbf}))),
    new(0xFFFD, ((@string)(new byte[]{0xef, 0xbf, 0xbd})))
}.slice();

// surrogate min decodes to (RuneError, 1)
// surrogate max decodes to (RuneError, 1)
internal static slice<Utf8Map> surrogateMap = new Utf8Map[]{
    new(0xd800, ((@string)(new byte[]{0xed, 0xa0, 0x80}))),
    new(0xdfff, ((@string)(new byte[]{0xed, 0xbf, 0xbf})))
}.slice();

internal static slice<@string> testStrings = new @string[]{
    ""u8,
    "abcd"u8,
    "☺☻☹"u8,
    "日a本b語ç日ð本Ê語þ日¥本¼語i日©"u8,
    "日a本b語ç日ð本Ê語þ日¥本¼語i日©日a本b語ç日ð本Ê語þ日¥本¼語i日©日a本b語ç日ð本Ê語þ日¥本¼語i日©"u8,
    ((@string)(new byte[]{0x80, 0x80, 0x80, 0x80}))
}.slice();

public static void TestFullRune(ж<testing.T> Ꮡt) {
    foreach (var (_, m) in utf8map) {
        var b = slice<byte>(m.str);
        if (!FullRune(b)) {
            Ꮡt.Errorf("FullRune(%q) (%U) = false, want true"u8, b, m.r);
        }
        @string s = m.str;
        if (!FullRuneInString(s)) {
            Ꮡt.Errorf("FullRuneInString(%q) (%U) = false, want true"u8, s, m.r);
        }
        var b1 = b.slice(0, len(b) - 1);
        if (FullRune(b1)) {
            Ꮡt.Errorf("FullRune(%q) = true, want false"u8, b1);
        }
        @string s1 = ((@string)b1);
        if (FullRuneInString(s1)) {
            Ꮡt.Errorf("FullRune(%q) = true, want false"u8, s1);
        }
    }
    foreach (var (_, s) in new @string[]{((@string)(new byte[]{0xc0})), ((@string)(new byte[]{0xc1}))}.slice()) {
        var b = slice<byte>(s);
        if (!FullRune(b)) {
            Ꮡt.Errorf("FullRune(%q) = false, want true"u8, s);
        }
        if (!FullRuneInString(s)) {
            Ꮡt.Errorf("FullRuneInString(%q) = false, want true"u8, s);
        }
    }
}

public static void TestEncodeRune(ж<testing.T> Ꮡt) {
    foreach (var (_, m) in utf8map) {
        var b = slice<byte>(m.str);
        array<byte> buf = new(10);
        nint n = EncodeRune(buf[0..], m.r);
        var b1 = buf.slice(0, n);
        if (!bytes.Equal(b, b1)) {
            Ꮡt.Errorf("EncodeRune(%#04x) = %q want %q"u8, m.r, b1, b);
        }
    }
}

public static void TestAppendRune(ж<testing.T> Ꮡt) {
    foreach (var (_, m) in utf8map) {
        {
            var buf = AppendRune(default!, m.r); if (((sstring)buf) != m.str) {
                Ꮡt.Errorf("AppendRune(nil, %#04x) = %s, want %s"u8, m.r, buf, m.str);
            }
        }
        {
            var buf = AppendRune(slice<byte>("init"u8), m.r); if (((@string)buf) != "init"u8 + m.str) {
                Ꮡt.Errorf("AppendRune(init, %#04x) = %s, want %s"u8, m.r, buf, "init" + m.str);
            }
        }
    }
}

public static void TestDecodeRune(ж<testing.T> Ꮡt) {
    foreach (var (_, m) in utf8map) {
        var b = slice<byte>(m.str);
        var (r, size) = DecodeRune(b);
        if (r != m.r || size != len(b)) {
            Ꮡt.Errorf("DecodeRune(%q) = %#04x, %d want %#04x, %d"u8, b, r, size, m.r, len(b));
        }
        @string s = m.str;
        (r, size) = DecodeRuneInString(s);
        if (r != m.r || size != len(b)) {
            Ꮡt.Errorf("DecodeRuneInString(%q) = %#04x, %d want %#04x, %d"u8, s, r, size, m.r, len(b));
        }
        // there's an extra byte that bytes left behind - make sure trailing byte works
        (r, size) = DecodeRune(b.slice(0, cap(b)));
        if (r != m.r || size != len(b)) {
            Ꮡt.Errorf("DecodeRune(%q) = %#04x, %d want %#04x, %d"u8, b, r, size, m.r, len(b));
        }
        s = m.str + "\x00"u8;
        (r, size) = DecodeRuneInString(s);
        if (r != m.r || size != len(b)) {
            Ꮡt.Errorf("DecodeRuneInString(%q) = %#04x, %d want %#04x, %d"u8, s, r, size, m.r, len(b));
        }
        // make sure missing bytes fail
        nint wantsize = 1;
        if (wantsize >= len(b)) {
            wantsize = 0;
        }
        (r, size) = DecodeRune(b.slice(0, len(b) - 1));
        if (r != RuneError || size != wantsize) {
            Ꮡt.Errorf("DecodeRune(%q) = %#04x, %d want %#04x, %d"u8, b.slice(0, len(b) - 1), r, size, (int32)(RuneError), wantsize);
        }
        s = m.str.slice(0, len(m.str) - 1);
        (r, size) = DecodeRuneInString(s);
        if (r != RuneError || size != wantsize) {
            Ꮡt.Errorf("DecodeRuneInString(%q) = %#04x, %d want %#04x, %d"u8, s, r, size, (int32)(RuneError), wantsize);
        }
        // make sure bad sequences fail
        if (len(b) == 1){
            b[0] = 0x80;
        } else {
            b[len(b) - 1] = 0x7F;
        }
        (r, size) = DecodeRune(b);
        if (r != RuneError || size != 1) {
            Ꮡt.Errorf("DecodeRune(%q) = %#04x, %d want %#04x, %d"u8, b, r, size, (int32)(RuneError), (nint)(1));
        }
        s = ((@string)b);
        (r, size) = DecodeRuneInString(s);
        if (r != RuneError || size != 1) {
            Ꮡt.Errorf("DecodeRuneInString(%q) = %#04x, %d want %#04x, %d"u8, s, r, size, (int32)(RuneError), (nint)(1));
        }
    }
}

public static void TestDecodeSurrogateRune(ж<testing.T> Ꮡt) {
    foreach (var (_, m) in surrogateMap) {
        var b = slice<byte>(m.str);
        var (r, size) = DecodeRune(b);
        if (r != RuneError || size != 1) {
            Ꮡt.Errorf("DecodeRune(%q) = %x, %d want %x, %d"u8, b, r, size, (int32)(RuneError), (nint)(1));
        }
        @string s = m.str;
        (r, size) = DecodeRuneInString(s);
        if (r != RuneError || size != 1) {
            Ꮡt.Errorf("DecodeRuneInString(%q) = %x, %d want %x, %d"u8, b, r, size, (int32)(RuneError), (nint)(1));
        }
    }
}

// Check that DecodeRune and DecodeLastRune correspond to
// the equivalent range loop.
public static void TestSequencing(ж<testing.T> Ꮡt) {
    foreach (var (_, ts) in testStrings) {
        foreach (var (_, m) in utf8map) {
            foreach (var (_, s) in new @string[]{ts + m.str, m.str + ts, ts + m.str + ts}.slice()) {
                testSequence(Ꮡt, s);
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
public static void TestRuntimeConversion(ж<testing.T> Ꮡt) {
    foreach (var (_, ts) in testStrings) {
        nint count = RuneCountInString(ts);
        {
            nint n = runtimeRuneCount(ts); if (n != count) {
                Ꮡt.Errorf("%q: len([]rune()) counted %d runes; got %d from RuneCountInString"u8, ts, n, count);
                break;
            }
        }
        var runes = slice<rune>(ts);
        {
            nint n = len(runes); if (n != count) {
                Ꮡt.Errorf("%q: []rune() has length %d; got %d from RuneCountInString"u8, ts, n, count);
                break;
            }
        }
        nint i = 0;
        foreach (var (_, r) in ts) {
            if (r != runes[i]) {
                Ꮡt.Errorf("%q[%d]: expected %c (%U); got %c (%U)"u8, ts, i, runes[i], runes[i], r, r);
            }
            i++;
        }
    }
}

// surrogate min
// surrogate max
// xx
// s1
// s2
// s3
//s4
// s5
// s6
// s7
internal static slice<@string> invalidSequenceTests = new @string[]{
    ((@string)(new byte[]{0xed, 0xa0, 0x80, 0x80})),
    ((@string)(new byte[]{0xed, 0xbf, 0xbf, 0x80})),
    ((@string)(new byte[]{0x91, 0x80, 0x80, 0x80})),
    ((@string)(new byte[]{0xc2, 0x7f, 0x80, 0x80})),
    ((@string)(new byte[]{0xc2, 0xc0, 0x80, 0x80})),
    ((@string)(new byte[]{0xdf, 0x7f, 0x80, 0x80})),
    ((@string)(new byte[]{0xdf, 0xc0, 0x80, 0x80})),
    ((@string)(new byte[]{0xe0, 0x9f, 0xbf, 0x80})),
    ((@string)(new byte[]{0xe0, 0xa0, 0x7f, 0x80})),
    ((@string)(new byte[]{0xe0, 0xbf, 0xc0, 0x80})),
    ((@string)(new byte[]{0xe0, 0xc0, 0x80, 0x80})),
    ((@string)(new byte[]{0xe1, 0x7f, 0xbf, 0x80})),
    ((@string)(new byte[]{0xe1, 0x80, 0x7f, 0x80})),
    ((@string)(new byte[]{0xe1, 0xbf, 0xc0, 0x80})),
    ((@string)(new byte[]{0xe1, 0xc0, 0x80, 0x80})),
    ((@string)(new byte[]{0xed, 0x7f, 0xbf, 0x80})),
    ((@string)(new byte[]{0xed, 0x80, 0x7f, 0x80})),
    ((@string)(new byte[]{0xed, 0x9f, 0xc0, 0x80})),
    ((@string)(new byte[]{0xed, 0xa0, 0x80, 0x80})),
    ((@string)(new byte[]{0xf0, 0x8f, 0xbf, 0xbf})),
    ((@string)(new byte[]{0xf0, 0x90, 0x7f, 0xbf})),
    ((@string)(new byte[]{0xf0, 0x90, 0x80, 0x7f})),
    ((@string)(new byte[]{0xf0, 0xbf, 0xbf, 0xc0})),
    ((@string)(new byte[]{0xf0, 0xbf, 0xc0, 0x80})),
    ((@string)(new byte[]{0xf0, 0xc0, 0x80, 0x80})),
    ((@string)(new byte[]{0xf1, 0x7f, 0xbf, 0xbf})),
    ((@string)(new byte[]{0xf1, 0x80, 0x7f, 0xbf})),
    ((@string)(new byte[]{0xf1, 0x80, 0x80, 0x7f})),
    ((@string)(new byte[]{0xf1, 0xbf, 0xbf, 0xc0})),
    ((@string)(new byte[]{0xf1, 0xbf, 0xc0, 0x80})),
    ((@string)(new byte[]{0xf1, 0xc0, 0x80, 0x80})),
    ((@string)(new byte[]{0xf4, 0x7f, 0xbf, 0xbf})),
    ((@string)(new byte[]{0xf4, 0x80, 0x7f, 0xbf})),
    ((@string)(new byte[]{0xf4, 0x80, 0x80, 0x7f})),
    ((@string)(new byte[]{0xf4, 0x8f, 0xbf, 0xc0})),
    ((@string)(new byte[]{0xf4, 0x8f, 0xc0, 0x80})),
    ((@string)(new byte[]{0xf4, 0x90, 0x80, 0x80}))
}.slice();

internal static rune runtimeDecodeRune(@string s) {
    foreach (var (_, r) in s) {
        return r;
    }
    return -1;
}

public static void TestDecodeInvalidSequence(ж<testing.T> Ꮡt) {
    foreach (var (_, s) in invalidSequenceTests) {
        var (r1, _) = DecodeRune(slice<byte>(s));
        {
            rune want = RuneError; if (r1 != want) {
                Ꮡt.Errorf("DecodeRune(%#x) = %#04x, want %#04x"u8, s, r1, want);
                return;
            }
        }
        var (r2, _) = DecodeRuneInString(s);
        {
            rune want = RuneError; if (r2 != want) {
                Ꮡt.Errorf("DecodeRuneInString(%q) = %#04x, want %#04x"u8, s, r2, want);
                return;
            }
        }
        if (r1 != r2) {
            Ꮡt.Errorf("DecodeRune(%#x) = %#04x mismatch with DecodeRuneInString(%q) = %#04x"u8, s, r1, s, r2);
            return;
        }
        var r3 = runtimeDecodeRune(s);
        if (r2 != r3) {
            Ꮡt.Errorf("DecodeRuneInString(%q) = %#04x mismatch with runtime.decoderune(%q) = %#04x"u8, s, r2, s, r3);
            return;
        }
    }
}

internal partial struct testSequence_info /*dyn*/ {
    internal nint index;
    internal rune r;
}

internal static void testSequence(ж<testing.T> Ꮡt, @string s) {
    var index = new slice<testSequence_info>(len(s));
    var b = slice<byte>(s);
    nint si = 0;
    nint j = 0;
    foreach (var (i, r) in s) {
        if (si != i) {
            Ꮡt.Errorf("Sequence(%q) mismatched index %d, want %d"u8, s, si, i);
            return;
        }
        index[j] = new testSequence_info(i, r);
        j++;
        var (r1, size1) = DecodeRune(b.slice(i));
        if (r != r1) {
            Ꮡt.Errorf("DecodeRune(%q) = %#04x, want %#04x"u8, s.slice(i), r1, r);
            return;
        }
        var (r2, size2) = DecodeRuneInString(s.slice(i));
        if (r != r2) {
            Ꮡt.Errorf("DecodeRuneInString(%q) = %#04x, want %#04x"u8, s.slice(i), r2, r);
            return;
        }
        if (size1 != size2) {
            Ꮡt.Errorf("DecodeRune/DecodeRuneInString(%q) size mismatch %d/%d"u8, s.slice(i), size1, size2);
            return;
        }
        si += size1;
    }
    j--;
    for (si = len(s); si > 0; ) {
        var (r1, size1) = DecodeLastRune(b.slice(0, si));
        var (r2, size2) = DecodeLastRuneInString(s.slice(0, si));
        if (size1 != size2) {
            Ꮡt.Errorf("DecodeLastRune/DecodeLastRuneInString(%q, %d) size mismatch %d/%d"u8, s, si, size1, size2);
            return;
        }
        if (r1 != index[j].r) {
            Ꮡt.Errorf("DecodeLastRune(%q, %d) = %#04x, want %#04x"u8, s, si, r1, index[j].r);
            return;
        }
        if (r2 != index[j].r) {
            Ꮡt.Errorf("DecodeLastRuneInString(%q, %d) = %#04x, want %#04x"u8, s, si, r2, index[j].r);
            return;
        }
        si -= size1;
        if (si != index[j].index) {
            Ꮡt.Errorf("DecodeLastRune(%q) index mismatch at %d, want %d"u8, s, si, index[j].index);
            return;
        }
        j--;
    }
    if (si != 0) {
        Ꮡt.Errorf("DecodeLastRune(%q) finished at %d, not 0"u8, s, si);
    }
}

// Check that negative runes encode as U+FFFD.
public static void TestNegativeRune(ж<testing.T> Ꮡt) {
    var errorbuf = new slice<byte>(UTFMax);
    errorbuf = errorbuf.slice(0, EncodeRune(errorbuf, RuneError));
    var buf = new slice<byte>(UTFMax);
    buf = buf.slice(0, EncodeRune(buf, -1));
    if (!bytes.Equal(buf, errorbuf)) {
        Ꮡt.Errorf("incorrect encoding [% x] for -1; expected [% x]"u8, buf, errorbuf);
    }
}

partial struct RuneCountTest {
    internal @string @in;
    internal nint @out;
}

internal static slice<RuneCountTest> runecounttests = new RuneCountTest[]{
    new("abcd"u8, 4),
    new("☺☻☹"u8, 3),
    new("1,2,3,4"u8, 7),
    new(((@string)(new byte[]{0xe2, 0x00})), 2),
    new(((@string)(new byte[]{0xe2, 0x80})), 2),
    new(((@string)(new byte[]{0x61, 0xe2, 0x80})), 3)
}.slice();

public static void TestRuneCount(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in runecounttests) {
        {
            nint @out = RuneCountInString(tt.@in); if (@out != tt.@out) {
                Ꮡt.Errorf("RuneCountInString(%q) = %d, want %d"u8, tt.@in, @out, tt.@out);
            }
        }
        {
            nint @out = RuneCount(slice<byte>(tt.@in)); if (@out != tt.@out) {
                Ꮡt.Errorf("RuneCount(%q) = %d, want %d"u8, tt.@in, @out, tt.@out);
            }
        }
    }
}

public static void TestRuneCountNonASCIIAllocation(ж<testing.T> Ꮡt) {
    {
        var n = testing.AllocsPerRun(10, () => {
            var s = slice<byte>("日本語日本語日本語日"u8);
            _ = RuneCount(s);
        }); if (n > 0D) {
            Ꮡt.Errorf("unexpected RuneCount allocation, got %v, want 0"u8, n);
        }
    }
}

partial struct RuneLenTest {
    internal rune r;
    internal nint size;
}

internal static slice<RuneLenTest> runelentests = new RuneLenTest[]{
    new(0, 1),
    new((rune)'e', 1),
    new((rune)'é', 2),
    new((rune)'☺', 3),
    new(RuneError, 3),
    new(MaxRune, 4),
    new(0xD800, -1),
    new(0xDFFF, -1),
    new(MaxRune + 1, -1),
    new(-1, -1)
}.slice();

public static void TestRuneLen(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in runelentests) {
        {
            nint size = RuneLen(tt.r); if (size != tt.size) {
                Ꮡt.Errorf("RuneLen(%#U) = %d, want %d"u8, tt.r, size, tt.size);
            }
        }
    }
}

partial struct ValidTest {
    internal @string @in;
    internal bool @out;
}

// U+10FFFF
// U+10FFFF+1; out of range
// 0x1FFFFF; out of range
// 0x3FFFFFF; out of range
// U+0000 encoded in two bytes: incorrect
// U+D800 high surrogate (sic)
// U+DFFF low surrogate (sic)
internal static slice<ValidTest> validTests = new ValidTest[]{
    new(""u8, true),
    new("a"u8, true),
    new("abc"u8, true),
    new("Ж"u8, true),
    new("ЖЖ"u8, true),
    new("брэд-ЛГТМ"u8, true),
    new("☺☻☹"u8, true),
    new(((@string)(new byte[]{0x61, 0x61, 0xe2})), false),
    new(((@string)new byte[]{66, 250}.slice()), false),
    new(((@string)new byte[]{66, 250, 67}.slice()), false),
    new("a\uFFFDb"u8, true),
    new(((@string)((@string)(new byte[]{0xf4, 0x8f, 0xbf, 0xbf}))), true),
    new(((@string)((@string)(new byte[]{0xf4, 0x90, 0x80, 0x80}))), false),
    new(((@string)((@string)(new byte[]{0xf7, 0xbf, 0xbf, 0xbf}))), false),
    new(((@string)((@string)(new byte[]{0xfb, 0xbf, 0xbf, 0xbf, 0xbf}))), false),
    new(((@string)((@string)(new byte[]{0xc0, 0x80}))), false),
    new(((@string)((@string)(new byte[]{0xed, 0xa0, 0x80}))), false),
    new(((@string)((@string)(new byte[]{0xed, 0xbf, 0xbf}))), false)
}.slice();

public static void TestValid(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in validTests) {
        if (Valid(slice<byte>(tt.@in)) != tt.@out) {
            Ꮡt.Errorf("Valid(%q) = %v; want %v"u8, tt.@in, !tt.@out, tt.@out);
        }
        if (ValidString(tt.@in) != tt.@out) {
            Ꮡt.Errorf("ValidString(%q) = %v; want %v"u8, tt.@in, !tt.@out, tt.@out);
        }
    }
}

partial struct ValidRuneTest {
    internal rune r;
    internal bool ok;
}

internal static slice<ValidRuneTest> validrunetests = new ValidRuneTest[]{
    new(0, true),
    new((rune)'e', true),
    new((rune)'é', true),
    new((rune)'☺', true),
    new(RuneError, true),
    new(MaxRune, true),
    new(0xD7FF, true),
    new(0xD800, false),
    new(0xDFFF, false),
    new(0xE000, true),
    new(MaxRune + 1, false),
    new(-1, false)
}.slice();

public static void TestValidRune(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, tt) in validrunetests) {
        {
            var ok = ValidRune(tt.r); if (ok != tt.ok) {
                Ꮡt.Errorf("ValidRune(%#U) = %t, want %t"u8, tt.r, ok, tt.ok);
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

internal static @string ascii100000 = strings.Repeat("0123456789"u8, 10000);

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
        ValidString("0123456789"u8);
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
        ValidString("日本語日本語日本語日"u8);
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

internal static @string longStringJapanese; // ~100KB, non-ASCII

[GoInit] internal static void initΔ1() {
    @string japanese = "日本語日本語日本語日"u8;
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    for (nint i = 0; b.Len() < 100_000; i++) {
        if (i % 100 == 0){
            Ꮡb.WriteString(japanese);
        } else {
            Ꮡb.WriteString("0123456789"u8);
        }
    }
    longStringMostlyASCII = b.String();
    longStringJapanese = strings.Repeat(japanese, 100_000 / len(japanese));
}

public static void BenchmarkEncodeASCIIRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        EncodeRune(buf, (rune)'a'); // 1 byte
    }
}

public static void BenchmarkEncodeSpanishRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        EncodeRune(buf, (rune)'Ñ'); // 2 bytes
    }
}

public static void BenchmarkEncodeJapaneseRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        EncodeRune(buf, (rune)'本'); // 3 bytes
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
        AppendRune(buf[..0], (rune)'a'); // 1 byte
    }
}

public static void BenchmarkAppendSpanishRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        AppendRune(buf[..0], (rune)'Ñ'); // 2 bytes
    }
}

public static void BenchmarkAppendJapaneseRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(UTFMax);
    for (nint i = 0; i < b.N; i++) {
        AppendRune(buf[..0], (rune)'本'); // 3 bytes
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

    var a = new byte[]{(rune)'a'}.slice();
    for (nint i = 0; i < b.N; i++) {
        DecodeRune(a);
    }
}

public static void BenchmarkDecodeJapaneseRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var nihon = slice<byte>("本"u8);
    for (nint i = 0; i < b.N; i++) {
        DecodeRune(nihon);
    }
}

// boolSink is used to reference the return value of benchmarked
// functions to avoid dead code elimination.
internal static bool boolSink;

internal partial struct BenchmarkFullRune_benchmarks /*dyn*/ {
    internal @string name;
    internal slice<byte> data;
}

public static void BenchmarkFullRune(ж<testing.B> Ꮡb) {
    var benchmarks = new BenchmarkFullRune_benchmarks[]{
        new("ASCII"u8, slice<byte>("a"u8)),
        new("Incomplete"u8, slice<byte>(((@string)(new byte[]{0xf0, 0x90, 0x80})))),
        new("Japanese"u8, slice<byte>("本"u8))
    }.slice();
    foreach (var (_, vᴛ1) in benchmarks) {
        ref var bm = ref heap(new BenchmarkFullRune_benchmarks(), out var Ꮡbm);
        bm = vᴛ1;

        var bmʗ1 = bm;
        Ꮡb.Run(bm.name, (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                boolSink = FullRune(bmʗ1.data);
            }
        });
    }
}

} // end utf8_test_package
