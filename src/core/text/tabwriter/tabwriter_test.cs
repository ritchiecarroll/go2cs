// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using testing = testing_package;
using static go.text.tabwriter_package;
using go.text;
using tabwriter = go.text.tabwriter_package;

partial class tabwriter_test_package {

partial struct buffer {
    internal slice<byte> a;
}

internal static void init(this ref buffer b, nint n) {
    b.a = new slice<byte>(0, n);
}

internal static void clear(this ref buffer b) {
    b.a = b.a[0..0];
}

internal static (nint written, error err) Write(this ref buffer b, slice<byte> buf) {
    nint n = len(b.a);
    nint m = len(buf);
    if (n + m <= cap(b.a)){
        b.a = b.a.slice(0, n + m);
        for (nint i = 0; i < m; i++) {
            b.a[n + i] = buf[i];
        }
    } else {
        throw panic("buffer.Write: buffer too small");
    }
    return (len(buf), default!);
}

internal static @string String(this ref buffer b) {
    return ((@string)b.a);
}

internal static void write(ж<testing.T> Ꮡt, @string testname, ж<tabwriter.Writer> Ꮡw, @string src) {
    var (written, err) = io.WriteString(new tabwriter.WriterжWriter(Ꮡw), src);
    if (err != default!) {
        Ꮡt.Errorf("--- test: %s\n--- src:\n%q\n--- write error: %v\n"u8, testname, src, err);
    }
    if (written != len(src)) {
        Ꮡt.Errorf("--- test: %s\n--- src:\n%q\n--- written = %d, len(src) = %d\n"u8, testname, src, written, len(src));
    }
}

internal static void verify(ж<testing.T> Ꮡt, @string testname, ж<tabwriter.Writer> Ꮡw, ж<buffer> Ꮡb, @string src, @string expected) {
    ref var b = ref Ꮡb.DerefOrNull();

    var err = Ꮡw.Flush();
    if (err != default!) {
        Ꮡt.Errorf("--- test: %s\n--- src:\n%q\n--- flush error: %v\n"u8, testname, src, err);
    }
    @string res = b.String();
    if (res != expected) {
        Ꮡt.Errorf("--- test: %s\n--- src:\n%q\n--- found:\n%q\n--- expected:\n%q\n"u8, testname, src, res, expected);
    }
}

internal static void check(ж<testing.T> Ꮡt, @string testname, nint minwidth, nint tabwidth, nint padding, byte padchar, nuint flags, @string src, @string expected) {
    ref var b = ref heap(new buffer(), out var Ꮡb);
    b.init(1000);
    ref var w = ref heap(new tabwriter.Writer(), out var Ꮡw);
    Ꮡw.Init(new bufferжWriter(Ꮡb), minwidth, tabwidth, padding, padchar, flags);
    // write all at once
    @string title = testname + " (written all at once)"u8;
    b.clear();
    write(Ꮡt, title, Ꮡw, src);
    verify(Ꮡt, title, Ꮡw, Ꮡb, src, expected);
    // write byte-by-byte
    title = testname + " (written byte-by-byte)"u8;
    b.clear();
    for (nint i = 0; i < len(src); i++) {
        write(Ꮡt, title, Ꮡw, src.slice(i, i + 1));
    }
    verify(Ꮡt, title, Ꮡw, Ꮡb, src, expected);
    // write using Fibonacci slice sizes
    title = testname + " (written in fibonacci slices)"u8;
    b.clear();
    for ((nint i, nint d) = (0, 0); i < len(src); ) {
        write(Ꮡt, title, Ꮡw, src.slice(i, i + d));
        (i, d) = (i + d, d + 1);
        if (i + d > len(src)) {
            d = len(src) - i;
        }
    }
    verify(Ꮡt, title, Ꮡw, Ꮡb, src, expected);
}

// unterminated escape
// unterminated escape
// '\t' terminates an empty cell on last line - nothing to print
// '\t' terminates an empty cell on last line - nothing to print
// \f inside HTML is ignored
// \f causes a newline and flush
// \f causes a newline and flush
// htabs - do not discard column
// hard tabs - do not discard column
// hard tabs - do not discard column

partial struct testsᴛ1 /*dyn*/ {
    internal @string testname;
    internal nint minwidth, tabwidth, padding;
    internal byte padchar;
    internal nuint flags;
    internal @string src, expected;
}
internal static slice<testsᴛ1> tests = new testsᴛ1[]{
    new(
        "1a"u8,
        8, 0, 1, (rune)'.', 0,
        ""u8,
        ""u8
    ),
    new(
        "1a debug"u8,
        8, 0, 1, (rune)'.', Debug,
        ""u8,
        ""u8
    ),
    new(
        "1b esc stripped"u8,
        8, 0, 1, (rune)'.', StripEscape,
        ((@string)(new byte[]{0xff, 0xff})),
        ""u8
    ),
    new(
        "1b esc"u8,
        8, 0, 1, (rune)'.', 0,
        ((@string)(new byte[]{0xff, 0xff})),
        ((@string)(new byte[]{0xff, 0xff}))
    ),
    new(
        "1c esc stripped"u8,
        8, 0, 1, (rune)'.', StripEscape,
        ((@string)(new byte[]{0xff, 0x09, 0xff})),
        "\t"u8
    ),
    new(
        "1c esc"u8,
        8, 0, 1, (rune)'.', 0,
        ((@string)(new byte[]{0xff, 0x09, 0xff})),
        ((@string)(new byte[]{0xff, 0x09, 0xff}))
    ),
    new(
        "1d esc stripped"u8,
        8, 0, 1, (rune)'.', StripEscape,
        ((@string)(new byte[]{0xff, 0x22, 0x66, 0x6f, 0x6f, 0x09, 0x0a, 0x09, 0x62, 0x61, 0x72, 0x22, 0xff})),
        "\"foo\t\n\tbar\""u8
    ),
    new(
        "1d esc"u8,
        8, 0, 1, (rune)'.', 0,
        ((@string)(new byte[]{0xff, 0x22, 0x66, 0x6f, 0x6f, 0x09, 0x0a, 0x09, 0x62, 0x61, 0x72, 0x22, 0xff})),
        ((@string)(new byte[]{0xff, 0x22, 0x66, 0x6f, 0x6f, 0x09, 0x0a, 0x09, 0x62, 0x61, 0x72, 0x22, 0xff}))
    ),
    new(
        "1e esc stripped"u8,
        8, 0, 1, (rune)'.', StripEscape,
        ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x09, 0x64, 0x65, 0x66})),
        "abc\tdef"u8
    ),
    new(
        "1e esc"u8,
        8, 0, 1, (rune)'.', 0,
        ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x09, 0x64, 0x65, 0x66})),
        ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x09, 0x64, 0x65, 0x66}))
    ),
    new(
        "2"u8,
        8, 0, 1, (rune)'.', 0,
        "\n\n\n"u8,
        "\n\n\n"u8
    ),
    new(
        "3"u8,
        8, 0, 1, (rune)'.', 0,
        "a\nb\nc"u8,
        "a\nb\nc"u8
    ),
    new(
        "4a"u8,
        8, 0, 1, (rune)'.', 0,
        "\t"u8,
        ""u8
    ),
    new(
        "4b"u8,
        8, 0, 1, (rune)'.', AlignRight,
        "\t"u8,
        ""u8
    ),
    new(
        "5"u8,
        8, 0, 1, (rune)'.', 0,
        "*\t*"u8,
        "*.......*"u8
    ),
    new(
        "5b"u8,
        8, 0, 1, (rune)'.', 0,
        "*\t*\n"u8,
        "*.......*\n"u8
    ),
    new(
        "5c"u8,
        8, 0, 1, (rune)'.', 0,
        "*\t*\t"u8,
        "*.......*"u8
    ),
    new(
        "5c debug"u8,
        8, 0, 1, (rune)'.', Debug,
        "*\t*\t"u8,
        "*.......|*"u8
    ),
    new(
        "5d"u8,
        8, 0, 1, (rune)'.', AlignRight,
        "*\t*\t"u8,
        ".......**"u8
    ),
    new(
        "6"u8,
        8, 0, 1, (rune)'.', 0,
        "\t\n"u8,
        "........\n"u8
    ),
    new(
        "7a"u8,
        8, 0, 1, (rune)'.', 0,
        "a) foo"u8,
        "a) foo"u8
    ),
    new(
        "7b"u8,
        8, 0, 1, (rune)' ', 0,
        "b) foo\tbar"u8,
        "b) foo  bar"u8
    ),
    new(
        "7c"u8,
        8, 0, 1, (rune)'.', 0,
        "c) foo\tbar\t"u8,
        "c) foo..bar"u8
    ),
    new(
        "7d"u8,
        8, 0, 1, (rune)'.', 0,
        "d) foo\tbar\n"u8,
        "d) foo..bar\n"u8
    ),
    new(
        "7e"u8,
        8, 0, 1, (rune)'.', 0,
        "e) foo\tbar\t\n"u8,
        "e) foo..bar.....\n"u8
    ),
    new(
        "7f"u8,
        8, 0, 1, (rune)'.', FilterHTML,
        "f) f&lt;o\t<b>bar</b>\t\n"u8,
        "f) f&lt;o..<b>bar</b>.....\n"u8
    ),
    new(
        "7g"u8,
        8, 0, 1, (rune)'.', FilterHTML,
        "g) f&lt;o\t<b>bar</b>\t non-terminated entity &amp"u8,
        "g) f&lt;o..<b>bar</b>..... non-terminated entity &amp"u8
    ),
    new(
        "7g debug"u8,
        8, 0, 1, (rune)'.', (nuint)(FilterHTML | Debug),
        "g) f&lt;o\t<b>bar</b>\t non-terminated entity &amp"u8,
        "g) f&lt;o..|<b>bar</b>.....| non-terminated entity &amp"u8
    ),
    new(
        "8"u8,
        8, 0, 1, (rune)'*', 0,
        "Hello, world!\n"u8,
        "Hello, world!\n"u8
    ),
    new(
        "9a"u8,
        1, 0, 0, (rune)'.', 0,
        "1\t2\t3\t4\n"u8 + "11\t222\t3333\t44444\n"u8,
        "1.2..3...4\n"u8 + "11222333344444\n"u8
    ),
    new(
        "9b"u8,
        1, 0, 0, (rune)'.', FilterHTML,
        "1\t2<!---\f--->\t3\t4\n"u8 + "11\t222\t3333\t44444\n"u8,
        "1.2<!---\f--->..3...4\n"u8 + "11222333344444\n"u8
    ),
    new(
        "9c"u8,
        1, 0, 0, (rune)'.', 0,
        "1\t2\t3\t4\f"u8 + "11\t222\t3333\t44444\n"u8,
        "1234\n"u8 + "11222333344444\n"u8
    ),
    new(
        "9c debug"u8,
        1, 0, 0, (rune)'.', Debug,
        "1\t2\t3\t4\f"u8 + "11\t222\t3333\t44444\n"u8,
        "1|2|3|4\n"u8 + "---\n"u8 + "11|222|3333|44444\n"u8
    ),
    new(
        "10a"u8,
        5, 0, 0, (rune)'.', 0,
        "1\t2\t3\t4\n"u8,
        "1....2....3....4\n"u8
    ),
    new(
        "10b"u8,
        5, 0, 0, (rune)'.', 0,
        "1\t2\t3\t4\t\n"u8,
        "1....2....3....4....\n"u8
    ),
    new(
        "11"u8,
        8, 0, 1, (rune)'.', 0,
        "本\tb\tc\n"u8 + "aa\t\u672c\u672c\u672c\tcccc\tddddd\n"u8 + "aaa\tbbbb\n"u8,
        "本.......b.......c\n"u8 + "aa......本本本.....cccc....ddddd\n"u8 + "aaa.....bbbb\n"u8
    ),
    new(
        "12a"u8,
        8, 0, 1, (rune)' ', AlignRight,
        "a\tè\tc\t\n"u8 + "aa\tèèè\tcccc\tddddd\t\n"u8 + "aaa\tèèèè\t\n"u8,
        "       a       è       c\n"u8 + "      aa     èèè    cccc   ddddd\n"u8 + "     aaa    èèèè\n"u8
    ),
    new(
        "12b"u8,
        2, 0, 0, (rune)' ', 0,
        "a\tb\tc\n"u8 + "aa\tbbb\tcccc\n"u8 + "aaa\tbbbb\n"u8,
        "a  b  c\n"u8 + "aa bbbcccc\n"u8 + "aaabbbb\n"u8
    ),
    new(
        "12c"u8,
        8, 0, 1, (rune)'_', 0,
        "a\tb\tc\n"u8 + "aa\tbbb\tcccc\n"u8 + "aaa\tbbbb\n"u8,
        "a_______b_______c\n"u8 + "aa______bbb_____cccc\n"u8 + "aaa_____bbbb\n"u8
    ),
    new(
        "13a"u8,
        4, 0, 1, (rune)'-', 0,
        "4444\t日本語\t22\t1\t333\n"u8 + "999999999\t22\n"u8 + "7\t22\n"u8 + "\t\t\t88888888\n"u8 + "\n"u8 + "666666\t666666\t666666\t4444\n"u8 + "1\t1\t999999999\t0000000000\n"u8,
        "4444------日本語-22--1---333\n"u8 + "999999999-22\n"u8 + "7---------22\n"u8 + "------------------88888888\n"u8 + "\n"u8 + "666666-666666-666666----4444\n"u8 + "1------1------999999999-0000000000\n"u8
    ),
    new(
        "13b"u8,
        4, 0, 3, (rune)'.', 0,
        "4444\t333\t22\t1\t333\n"u8 + "999999999\t22\n"u8 + "7\t22\n"u8 + "\t\t\t88888888\n"u8 + "\n"u8 + "666666\t666666\t666666\t4444\n"u8 + "1\t1\t999999999\t0000000000\n"u8,
        "4444........333...22...1...333\n"u8 + "999999999...22\n"u8 + "7...........22\n"u8 + "....................88888888\n"u8 + "\n"u8 + "666666...666666...666666......4444\n"u8 + "1........1........999999999...0000000000\n"u8
    ),
    new(
        "13c"u8,
        8, 8, 1, (rune)'\t', FilterHTML,
        "4444\t333\t22\t1\t333\n"u8 + "999999999\t22\n"u8 + "7\t22\n"u8 + "\t\t\t88888888\n"u8 + "\n"u8 + "666666\t666666\t666666\t4444\n"u8 + "1\t1\t<font color=red attr=日本語>999999999</font>\t0000000000\n"u8,
        "4444\t\t333\t22\t1\t333\n"u8 + "999999999\t22\n"u8 + "7\t\t22\n"u8 + "\t\t\t\t88888888\n"u8 + "\n"u8 + "666666\t666666\t666666\t\t4444\n"u8 + "1\t1\t<font color=red attr=日本語>999999999</font>\t0000000000\n"u8
    ),
    new(
        "14"u8,
        1, 0, 2, (rune)' ', AlignRight,
        ".0\t.3\t2.4\t-5.1\t\n"u8 + "23.0\t12345678.9\t2.4\t-989.4\t\n"u8 + "5.1\t12.0\t2.4\t-7.0\t\n"u8 + ".0\t0.0\t332.0\t8908.0\t\n"u8 + ".0\t-.3\t456.4\t22.1\t\n"u8 + ".0\t1.2\t44.4\t-13.3\t\t"u8,
        "    .0          .3    2.4    -5.1\n"u8 + "  23.0  12345678.9    2.4  -989.4\n"u8 + "   5.1        12.0    2.4    -7.0\n"u8 + "    .0         0.0  332.0  8908.0\n"u8 + "    .0         -.3  456.4    22.1\n"u8 + "    .0         1.2   44.4   -13.3"u8
    ),
    new(
        "14 debug"u8,
        1, 0, 2, (rune)' ', (nuint)(AlignRight | Debug),
        ".0\t.3\t2.4\t-5.1\t\n"u8 + "23.0\t12345678.9\t2.4\t-989.4\t\n"u8 + "5.1\t12.0\t2.4\t-7.0\t\n"u8 + ".0\t0.0\t332.0\t8908.0\t\n"u8 + ".0\t-.3\t456.4\t22.1\t\n"u8 + ".0\t1.2\t44.4\t-13.3\t\t"u8,
        "    .0|          .3|    2.4|    -5.1|\n"u8 + "  23.0|  12345678.9|    2.4|  -989.4|\n"u8 + "   5.1|        12.0|    2.4|    -7.0|\n"u8 + "    .0|         0.0|  332.0|  8908.0|\n"u8 + "    .0|         -.3|  456.4|    22.1|\n"u8 + "    .0|         1.2|   44.4|   -13.3|"u8
    ),
    new(
        "15a"u8,
        4, 0, 0, (rune)'.', 0,
        "a\t\tb"u8,
        "a.......b"u8
    ),
    new(
        "15b"u8,
        4, 0, 0, (rune)'.', DiscardEmptyColumns,
        "a\t\tb"u8,
        "a.......b"u8
    ),
    new(
        "15c"u8,
        4, 0, 0, (rune)'.', DiscardEmptyColumns,
        "a\v\vb"u8,
        "a...b"u8
    ),
    new(
        "15d"u8,
        4, 0, 0, (rune)'.', (nuint)(AlignRight | DiscardEmptyColumns),
        "a\v\vb"u8,
        "...ab"u8
    ),
    new(
        "16a"u8,
        100, 100, 0, (rune)'\t', 0,
        "a\tb\t\td\n"u8 + "a\tb\t\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8,
        "a\tb\t\td\n"u8 + "a\tb\t\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8
    ),
    new(
        "16b"u8,
        100, 100, 0, (rune)'\t', DiscardEmptyColumns,
        "a\vb\v\vd\n"u8 + "a\vb\v\vd\ve\n"u8 + "a\n"u8 + "a\vb\vc\vd\n"u8 + "a\vb\vc\vd\ve\n"u8,
        "a\tb\td\n"u8 + "a\tb\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8
    ),
    new(
        "16b debug"u8,
        100, 100, 0, (rune)'\t', (nuint)(DiscardEmptyColumns | Debug),
        "a\vb\v\vd\n"u8 + "a\vb\v\vd\ve\n"u8 + "a\n"u8 + "a\vb\vc\vd\n"u8 + "a\vb\vc\vd\ve\n"u8,
        "a\t|b\t||d\n"u8 + "a\t|b\t||d\t|e\n"u8 + "a\n"u8 + "a\t|b\t|c\t|d\n"u8 + "a\t|b\t|c\t|d\t|e\n"u8
    ),
    new(
        "16c"u8,
        100, 100, 0, (rune)'\t', DiscardEmptyColumns,
        "a\tb\t\td\n"u8 + "a\tb\t\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8,
        "a\tb\t\td\n"u8 + "a\tb\t\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8
    ),
    new(
        "16c debug"u8,
        100, 100, 0, (rune)'\t', (nuint)(DiscardEmptyColumns | Debug),
        "a\tb\t\td\n"u8 + "a\tb\t\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8,
        "a\t|b\t|\t|d\n"u8 + "a\t|b\t|\t|d\t|e\n"u8 + "a\n"u8 + "a\t|b\t|c\t|d\n"u8 + "a\t|b\t|c\t|d\t|e\n"u8
    )
}.slice();

public static void Test(ж<testing.T> Ꮡt) {
    foreach (var (_, e) in tests) {
        check(Ꮡt, e.testname, e.minwidth, e.tabwidth, e.padding, e.padchar, e.flags, e.src, e.expected);
    }
}

partial struct panicWriter {
}

internal static (nint, error) Write(this panicWriter _Δp0, slice<byte> _Δp1) {
    throw panic("cannot write");
}

internal static void wantPanicString(ж<testing.T> Ꮡt, @string want) {
    GoFrame ᒐ = default;
    try {
        {
            var e = recover(); if (e != default!) {
                var (got, ok) = e._<@string>(ᐧ);
                switch (ᐧ) {
                case {} when !ok: {
                    Ꮡt.Errorf("got %v (%T), want panic string"u8, e, e);
                    break;
                }
                case {} when got != want: {
                    Ꮡt.Errorf("wrong panic message: got %q, want %q"u8, got, want);
                    break;
                }}

            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string tabwriterPanicDuringˢ = "tabwriter: panic during Flush (cannot write)"u8;

public static void TestPanicDuringFlush(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(wantPanicString, Ꮡt, tabwriterPanicDuringˢ, ref ᒐ);
        panicWriter p = default!;
        var w = @new<tabwriter.Writer>();
        w.Init(p, 0, 0, 5, (rune)' ', 0);
        io.WriteString(new tabwriter.WriterжWriter(w), "a"u8);
        w.Flush();
        Ꮡt.Errorf("failed to panic during Flush"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string tabwriterPanicDuringˢ2 = "tabwriter: panic during Write (cannot write)"u8;

public static void TestPanicDuringWrite(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(wantPanicString, Ꮡt, tabwriterPanicDuringˢ2, ref ᒐ);
        panicWriter p = default!;
        var w = @new<tabwriter.Writer>();
        w.Init(p, 0, 0, 5, (rune)' ', 0);
        io.WriteString(new tabwriter.WriterжWriter(w), "a\n\n"u8); // the second \n triggers a call to w.Write and thus a panic
        Ꮡt.Errorf("failed to panic during Write"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string newˢ = "new"u8;
private static readonly @string reuseˢ = "reuse"u8;

public static void BenchmarkTable(ж<testing.B> Ꮡb) {
    foreach (var (_, w) in new nint[]{1, 10, 100}.array()) {
        // Build a line with w cells.
        ref var line = ref heap<slice<byte>>(out var Ꮡline);
        line = bytes.Repeat(slice<byte>("a\t"u8), w);
        line = append(line, (byte)((rune)'\n'));
        foreach (var (_, h) in new nint[]{10, 1000, 100000}.array()) {
            Ꮡb.Run(fmt.Sprintf("%dx%d"u8, w, h), (ж<testing.B> bΔ1) => {
                bΔ1.Run(newˢ, (ж<testing.B> bΔ2) => {
                    bΔ2.ReportAllocs();
                    for (nint i = 0; i < (~bΔ2).N; i++) {
                        var wΔ1 = NewWriter(io.Discard, 4, 4, 1, (rune)' ', 0); // no particular reason for these settings
                        // Write the line h times.
                        for (nint j = 0; j < h; j++) {
                            wΔ1.Write(Ꮡline.ValueSlot);
                        }
                        wΔ1.Flush();
                    }
                });
                bΔ1.Run(reuseˢ, (ж<testing.B> bΔ3) => {
                    bΔ3.ReportAllocs();
                    var wΔ2 = NewWriter(io.Discard, 4, 4, 1, (rune)' ', 0); // no particular reason for these settings
                    for (nint i = 0; i < (~bΔ3).N; i++) {
                        // Write the line h times.
                        for (nint j = 0; j < h; j++) {
                            wΔ2.Write(Ꮡline.ValueSlot);
                        }
                        wΔ2.Flush();
                    }
                });
            });
        }
    }
}

public static void BenchmarkPyramid(ж<testing.B> Ꮡb) {
    foreach (var (_, x) in new nint[]{10, 100, 1000}.array()) {
        // Build a line with x cells.
        var line = bytes.Repeat(slice<byte>("a\t"u8), x);
        var lineʗ1 = line;
        Ꮡb.Run(fmt.Sprintf("%d"u8, x), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var w = NewWriter(io.Discard, 4, 4, 1, (rune)' ', 0); // no particular reason for these settings
                // Write increasing prefixes of that line.
                for (nint j = 0; j < x; j++) {
                    w.Write(lineʗ1.slice(0, j * 2));
                    w.Write(new byte[]{(rune)'\n'}.slice());
                }
                w.Flush();
            }
        });
    }
}

public static void BenchmarkRagged(ж<testing.B> Ꮡb) {
    ref var lines = ref heap(new array<slice<byte>>(8), out var Ꮡlines);
    foreach (var (i, w) in new nint[]{6, 2, 9, 5, 5, 7, 3, 8}.array()) {
        // Build a line with w cells.
        lines[i] = bytes.Repeat(slice<byte>("a\t"u8), w);
    }
    foreach (var (_, h) in new nint[]{10, 100, 1000}.array()) {
        var linesʗ1 = lines;
        Ꮡb.Run(fmt.Sprintf("%d"u8, h), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var w = NewWriter(io.Discard, 4, 4, 1, (rune)' ', 0); // no particular reason for these settings
                // Write the lines in turn h times.
                for (nint j = 0; j < h; j++) {
                    w.Write(linesʗ1[j % len(linesʗ1)]);
                    w.Write(new byte[]{(rune)'\n'}.slice());
                }
                w.Flush();
            }
        });
    }
}

internal static readonly @string codeSnippet = """

some command

foo	# aligned
barbaz	# comments

but
mostly
single
cell
lines

"""u8;

public static void BenchmarkCode(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        var w = NewWriter(io.Discard, 4, 4, 1, (rune)' ', 0); // no particular reason for these settings
        // The code is small, so it's reasonable for the tabwriter user
        // to write it all at once, or buffer the writes.
        w.Write(slice<byte>(codeSnippet));
        w.Flush();
    }
}

} // end tabwriter_test_package
