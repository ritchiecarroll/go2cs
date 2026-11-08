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

internal static void init(this ref buffer b, nint n) { b.a = new slice<byte>(0, n); }

internal static void clear(this ref buffer b) { b.a = b.a[0..0]; }

internal static (nint written, error err) Write(this ref buffer b, slice<byte> buf) {
    nint n = len(b.a);
    nint m = len(buf);
    if (n + m <= cap(b.a)) {
        b.a = b.a.slice(0, n + m);
        for (nint i = 0; i < m; i++) {
            b.a[n + i] = buf[i];
        }
    } else {
        throw panic("buffer.Write: buffer too small");
    }
    return (len(buf), default);
}

internal static @string String(this ref buffer b) { return (@string)b.a; }

internal static void write(ж<testing.T> t, @string testname, ж<tabwriter.Writer> w, @string src) {
    var (written, err) = io.WriteString(new tabwriter.WriterжWriter(w), src);
    if (err != nil) {
        t.Errorf("--- test: %s\n--- src:\n%q\n--- write error: %v\n", testname, src, err);
    }
    if (written != len(src)) {
        t.Errorf("--- test: %s\n--- src:\n%q\n--- written = %d, len(src) = %d\n", testname, src, written, len(src));
    }
}

internal static void verify(ж<testing.T> t, @string testname, ж<tabwriter.Writer> w, ж<buffer> Ꮡb, @string src, @string expected) {
    ref var b = ref Ꮡb.DerefOrNull();

    var err = w.Flush();
    if (err != nil) {
        t.Errorf("--- test: %s\n--- src:\n%q\n--- flush error: %v\n", testname, src, err);
    }

    @string res = b.String();
    if (res != expected) {
        t.Errorf("--- test: %s\n--- src:\n%q\n--- found:\n%q\n--- expected:\n%q\n", testname, src, res, expected);
    }
}

internal static void check(ж<testing.T> t, @string testname, nint minwidth, nint tabwidth, nint padding, byte padchar, nuint flags, @string src, @string expected) {
    ref var b = ref heap(new buffer(), out var Ꮡb);
    b.init(1000);

    ref var w = ref heap(new tabwriter.Writer(), out var Ꮡw);
    Ꮡw.Init(new bufferжWriter(Ꮡb), minwidth, tabwidth, padding, padchar, flags);

    // write all at once
    @string title = testname + " (written all at once)"u8;
    b.clear();
    write(t, title, Ꮡw, src);
    verify(t, title, Ꮡw, Ꮡb, src, expected);

    // write byte-by-byte
    title = testname + " (written byte-by-byte)"u8;
    b.clear();
    for (nint i = 0; i < len(src); i++) {
        write(t, title, Ꮡw, src.slice(i, i + 1));
    }
    verify(t, title, Ꮡw, Ꮡb, src, expected);

    // write using Fibonacci slice sizes
    title = testname + " (written in fibonacci slices)"u8;
    b.clear();
    for ((nint i, nint d) = (0, 0); i < len(src); ) {
        write(t, title, Ꮡw, src.slice(i, i + d));
        (i, d) = (i + d, d + 1);
        if (i + d > len(src)) {
            d = len(src) - i;
        }
    }
    verify(t, title, Ꮡw, Ꮡb, src, expected);
}

partial struct testsᴛ1 /*dyn*/ {
    internal @string testname;
    internal nint minwidth, tabwidth, padding;
    internal byte padchar;
    internal nuint flags;
    internal @string src, expected;
}
internal static slice<testsᴛ1> tests = [
    new(
        "1a",
        8, 0, 1, (byte)'.', 0,
        "",
        ""
    ),

    new(
        "1a debug",
        8, 0, 1, (byte)'.', Debug,
        "",
        ""
    ),

    new(
        "1b esc stripped",
        8, 0, 1, (byte)'.', StripEscape,
        ((@string)(new byte[]{0xff, 0xff})),
        ""
    ),

    new(
        "1b esc",
        8, 0, 1, (byte)'.', 0,
        ((@string)(new byte[]{0xff, 0xff})),
        ((@string)(new byte[]{0xff, 0xff}))
    ),

    new(
        "1c esc stripped",
        8, 0, 1, (byte)'.', StripEscape,
        ((@string)(new byte[]{0xff, 0x09, 0xff})),
        "\t"
    ),

    new(
        "1c esc",
        8, 0, 1, (byte)'.', 0,
        ((@string)(new byte[]{0xff, 0x09, 0xff})),
        ((@string)(new byte[]{0xff, 0x09, 0xff}))
    ),

    new(
        "1d esc stripped",
        8, 0, 1, (byte)'.', StripEscape,
        ((@string)(new byte[]{0xff, 0x22, 0x66, 0x6f, 0x6f, 0x09, 0x0a, 0x09, 0x62, 0x61, 0x72, 0x22, 0xff})),
        "\"foo\t\n\tbar\""
    ),

    new(
        "1d esc",
        8, 0, 1, (byte)'.', 0,
        ((@string)(new byte[]{0xff, 0x22, 0x66, 0x6f, 0x6f, 0x09, 0x0a, 0x09, 0x62, 0x61, 0x72, 0x22, 0xff})),
        ((@string)(new byte[]{0xff, 0x22, 0x66, 0x6f, 0x6f, 0x09, 0x0a, 0x09, 0x62, 0x61, 0x72, 0x22, 0xff}))
    ),

    new(
        "1e esc stripped",
        8, 0, 1, (byte)'.', StripEscape,
        ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x09, 0x64, 0x65, 0x66})), // unterminated escape
        "abc\tdef"
    ),

    new(
        "1e esc",
        8, 0, 1, (byte)'.', 0,
        ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x09, 0x64, 0x65, 0x66})), // unterminated escape
        ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x09, 0x64, 0x65, 0x66}))
    ),

    new(
        "2",
        8, 0, 1, (byte)'.', 0,
        "\n\n\n",
        "\n\n\n"
    ),

    new(
        "3",
        8, 0, 1, (byte)'.', 0,
        "a\nb\nc",
        "a\nb\nc"
    ),

    new(
        "4a",
        8, 0, 1, (byte)'.', 0,
        "\t", // '\t' terminates an empty cell on last line - nothing to print
        ""
    ),

    new(
        "4b",
        8, 0, 1, (byte)'.', AlignRight,
        "\t", // '\t' terminates an empty cell on last line - nothing to print
        ""
    ),

    new(
        "5",
        8, 0, 1, (byte)'.', 0,
        "*\t*",
        "*.......*"
    ),

    new(
        "5b",
        8, 0, 1, (byte)'.', 0,
        "*\t*\n",
        "*.......*\n"
    ),

    new(
        "5c",
        8, 0, 1, (byte)'.', 0,
        "*\t*\t",
        "*.......*"
    ),

    new(
        "5c debug",
        8, 0, 1, (byte)'.', Debug,
        "*\t*\t",
        "*.......|*"
    ),

    new(
        "5d",
        8, 0, 1, (byte)'.', AlignRight,
        "*\t*\t",
        ".......**"
    ),

    new(
        "6",
        8, 0, 1, (byte)'.', 0,
        "\t\n",
        "........\n"
    ),

    new(
        "7a",
        8, 0, 1, (byte)'.', 0,
        "a) foo",
        "a) foo"
    ),

    new(
        "7b",
        8, 0, 1, (byte)' ', 0,
        "b) foo\tbar",
        "b) foo  bar"
    ),

    new(
        "7c",
        8, 0, 1, (byte)'.', 0,
        "c) foo\tbar\t",
        "c) foo..bar"
    ),

    new(
        "7d",
        8, 0, 1, (byte)'.', 0,
        "d) foo\tbar\n",
        "d) foo..bar\n"
    ),

    new(
        "7e",
        8, 0, 1, (byte)'.', 0,
        "e) foo\tbar\t\n",
        "e) foo..bar.....\n"
    ),

    new(
        "7f",
        8, 0, 1, (byte)'.', FilterHTML,
        "f) f&lt;o\t<b>bar</b>\t\n",
        "f) f&lt;o..<b>bar</b>.....\n"
    ),

    new(
        "7g",
        8, 0, 1, (byte)'.', FilterHTML,
        "g) f&lt;o\t<b>bar</b>\t non-terminated entity &amp",
        "g) f&lt;o..<b>bar</b>..... non-terminated entity &amp"
    ),

    new(
        "7g debug",
        8, 0, 1, (byte)'.', (nuint)(FilterHTML | Debug),
        "g) f&lt;o\t<b>bar</b>\t non-terminated entity &amp",
        "g) f&lt;o..|<b>bar</b>.....| non-terminated entity &amp"
    ),

    new(
        "8",
        8, 0, 1, (byte)'*', 0,
        "Hello, world!\n",
        "Hello, world!\n"
    ),

    new(
        "9a",
        1, 0, 0, (byte)'.', 0,
        "1\t2\t3\t4\n"u8 + "11\t222\t3333\t44444\n"u8,

        "1.2..3...4\n"u8 + "11222333344444\n"u8
    ),

    new(
        "9b",
        1, 0, 0, (byte)'.', FilterHTML,
        "1\t2<!---\f--->\t3\t4\n"u8 + // \f inside HTML is ignored
            "11\t222\t3333\t44444\n"u8,

        "1.2<!---\f--->..3...4\n"u8 + "11222333344444\n"u8
    ),

    new(
        "9c",
        1, 0, 0, (byte)'.', 0,
        "1\t2\t3\t4\f"u8 + // \f causes a newline and flush
            "11\t222\t3333\t44444\n"u8,

        "1234\n"u8 + "11222333344444\n"u8
    ),

    new(
        "9c debug",
        1, 0, 0, (byte)'.', Debug,
        "1\t2\t3\t4\f"u8 + // \f causes a newline and flush
            "11\t222\t3333\t44444\n"u8,

        "1|2|3|4\n"u8 + "---\n"u8 + "11|222|3333|44444\n"u8
    ),

    new(
        "10a",
        5, 0, 0, (byte)'.', 0,
        "1\t2\t3\t4\n",
        "1....2....3....4\n"
    ),

    new(
        "10b",
        5, 0, 0, (byte)'.', 0,
        "1\t2\t3\t4\t\n",
        "1....2....3....4....\n"
    ),

    new(
        "11",
        8, 0, 1, (byte)'.', 0,
        "本\tb\tc\n"u8 + "aa\t\u672c\u672c\u672c\tcccc\tddddd\n"u8 + "aaa\tbbbb\n"u8,

        "本.......b.......c\n"u8 + "aa......本本本.....cccc....ddddd\n"u8 + "aaa.....bbbb\n"u8
    ),

    new(
        "12a",
        8, 0, 1, (byte)' ', AlignRight,
        "a\tè\tc\t\n"u8 + "aa\tèèè\tcccc\tddddd\t\n"u8 + "aaa\tèèèè\t\n"u8,

        "       a       è       c\n"u8 + "      aa     èèè    cccc   ddddd\n"u8 + "     aaa    èèèè\n"u8
    ),

    new(
        "12b",
        2, 0, 0, (byte)' ', 0,
        "a\tb\tc\n"u8 + "aa\tbbb\tcccc\n"u8 + "aaa\tbbbb\n"u8,

        "a  b  c\n"u8 + "aa bbbcccc\n"u8 + "aaabbbb\n"u8
    ),

    new(
        "12c",
        8, 0, 1, (byte)'_', 0,
        "a\tb\tc\n"u8 + "aa\tbbb\tcccc\n"u8 + "aaa\tbbbb\n"u8,

        "a_______b_______c\n"u8 + "aa______bbb_____cccc\n"u8 + "aaa_____bbbb\n"u8
    ),

    new(
        "13a",
        4, 0, 1, (byte)'-', 0,
        "4444\t日本語\t22\t1\t333\n"u8 + "999999999\t22\n"u8 + "7\t22\n"u8 + "\t\t\t88888888\n"u8 + "\n"u8 + "666666\t666666\t666666\t4444\n"u8 + "1\t1\t999999999\t0000000000\n"u8,

        "4444------日本語-22--1---333\n"u8 + "999999999-22\n"u8 + "7---------22\n"u8 + "------------------88888888\n"u8 + "\n"u8 + "666666-666666-666666----4444\n"u8 + "1------1------999999999-0000000000\n"u8
    ),

    new(
        "13b",
        4, 0, 3, (byte)'.', 0,
        "4444\t333\t22\t1\t333\n"u8 + "999999999\t22\n"u8 + "7\t22\n"u8 + "\t\t\t88888888\n"u8 + "\n"u8 + "666666\t666666\t666666\t4444\n"u8 + "1\t1\t999999999\t0000000000\n"u8,

        "4444........333...22...1...333\n"u8 + "999999999...22\n"u8 + "7...........22\n"u8 + "....................88888888\n"u8 + "\n"u8 + "666666...666666...666666......4444\n"u8 + "1........1........999999999...0000000000\n"u8
    ),

    new(
        "13c",
        8, 8, 1, (byte)'\t', FilterHTML,
        "4444\t333\t22\t1\t333\n"u8 + "999999999\t22\n"u8 + "7\t22\n"u8 + "\t\t\t88888888\n"u8 + "\n"u8 + "666666\t666666\t666666\t4444\n"u8 + "1\t1\t<font color=red attr=日本語>999999999</font>\t0000000000\n"u8,

        "4444\t\t333\t22\t1\t333\n"u8 + "999999999\t22\n"u8 + "7\t\t22\n"u8 + "\t\t\t\t88888888\n"u8 + "\n"u8 + "666666\t666666\t666666\t\t4444\n"u8 + "1\t1\t<font color=red attr=日本語>999999999</font>\t0000000000\n"u8
    ),

    new(
        "14",
        1, 0, 2, (byte)' ', AlignRight,
        ".0\t.3\t2.4\t-5.1\t\n"u8 + "23.0\t12345678.9\t2.4\t-989.4\t\n"u8 + "5.1\t12.0\t2.4\t-7.0\t\n"u8 + ".0\t0.0\t332.0\t8908.0\t\n"u8 + ".0\t-.3\t456.4\t22.1\t\n"u8 + ".0\t1.2\t44.4\t-13.3\t\t"u8,

        "    .0          .3    2.4    -5.1\n"u8 + "  23.0  12345678.9    2.4  -989.4\n"u8 + "   5.1        12.0    2.4    -7.0\n"u8 + "    .0         0.0  332.0  8908.0\n"u8 + "    .0         -.3  456.4    22.1\n"u8 + "    .0         1.2   44.4   -13.3"u8
    ),

    new(
        "14 debug",
        1, 0, 2, (byte)' ', (nuint)(AlignRight | Debug),
        ".0\t.3\t2.4\t-5.1\t\n"u8 + "23.0\t12345678.9\t2.4\t-989.4\t\n"u8 + "5.1\t12.0\t2.4\t-7.0\t\n"u8 + ".0\t0.0\t332.0\t8908.0\t\n"u8 + ".0\t-.3\t456.4\t22.1\t\n"u8 + ".0\t1.2\t44.4\t-13.3\t\t"u8,

        "    .0|          .3|    2.4|    -5.1|\n"u8 + "  23.0|  12345678.9|    2.4|  -989.4|\n"u8 + "   5.1|        12.0|    2.4|    -7.0|\n"u8 + "    .0|         0.0|  332.0|  8908.0|\n"u8 + "    .0|         -.3|  456.4|    22.1|\n"u8 + "    .0|         1.2|   44.4|   -13.3|"u8
    ),

    new(
        "15a",
        4, 0, 0, (byte)'.', 0,
        "a\t\tb",
        "a.......b"
    ),

    new(
        "15b",
        4, 0, 0, (byte)'.', DiscardEmptyColumns,
        "a\t\tb", // htabs - do not discard column
        "a.......b"
    ),

    new(
        "15c",
        4, 0, 0, (byte)'.', DiscardEmptyColumns,
        "a\v\vb",
        "a...b"
    ),

    new(
        "15d",
        4, 0, 0, (byte)'.', (nuint)(AlignRight | DiscardEmptyColumns),
        "a\v\vb",
        "...ab"
    ),

    new(
        "16a",
        100, 100, 0, (byte)'\t', 0,
        "a\tb\t\td\n"u8 + "a\tb\t\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8,

        "a\tb\t\td\n"u8 + "a\tb\t\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8
    ),

    new(
        "16b",
        100, 100, 0, (byte)'\t', DiscardEmptyColumns,
        "a\vb\v\vd\n"u8 + "a\vb\v\vd\ve\n"u8 + "a\n"u8 + "a\vb\vc\vd\n"u8 + "a\vb\vc\vd\ve\n"u8,

        "a\tb\td\n"u8 + "a\tb\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8
    ),

    new(
        "16b debug",
        100, 100, 0, (byte)'\t', (nuint)(DiscardEmptyColumns | Debug),
        "a\vb\v\vd\n"u8 + "a\vb\v\vd\ve\n"u8 + "a\n"u8 + "a\vb\vc\vd\n"u8 + "a\vb\vc\vd\ve\n"u8,

        "a\t|b\t||d\n"u8 + "a\t|b\t||d\t|e\n"u8 + "a\n"u8 + "a\t|b\t|c\t|d\n"u8 + "a\t|b\t|c\t|d\t|e\n"u8
    ),

    new(
        "16c",
        100, 100, 0, (byte)'\t', DiscardEmptyColumns,
        "a\tb\t\td\n"u8 + // hard tabs - do not discard column
            "a\tb\t\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8,

        "a\tb\t\td\n"u8 + "a\tb\t\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8
    ),

    new(
        "16c debug",
        100, 100, 0, (byte)'\t', (nuint)(DiscardEmptyColumns | Debug),
        "a\tb\t\td\n"u8 + // hard tabs - do not discard column
            "a\tb\t\td\te\n"u8 + "a\n"u8 + "a\tb\tc\td\n"u8 + "a\tb\tc\td\te\n"u8,

        "a\t|b\t|\t|d\n"u8 + "a\t|b\t|\t|d\t|e\n"u8 + "a\n"u8 + "a\t|b\t|c\t|d\n"u8 + "a\t|b\t|c\t|d\t|e\n"u8
    )
];

public static void Test(ж<testing.T> t) {
    foreach (var (_, e) in range(tests)) {
        check(t, e.testname, e.minwidth, e.tabwidth, e.padding, e.padchar, e.flags, e.src, e.expected);
    }
}

partial struct panicWriter {
}

internal static (nint, error) Write(this panicWriter _Δp0, slice<byte> _Δp1) {
    throw panic("cannot write");
}

internal static void wantPanicString(ж<testing.T> t, @string want) {
    GoFrame ᒐ = default;
    try {
        var calledByDefer = GoFrame.TakeDeferredCall(); // Go: is this call the deferred one?

        var e = recover(calledByDefer); if (e != nil) {
            var (got, ok) = e._<@string>(ᐧ);
            switch (ᐧ) {
            case {} when !ok: {
                t.Errorf("got %v (%T), want panic string", e, e);
                break;
            }
            case {} when got != want: {
                t.Errorf("wrong panic message: got %q, want %q", got, want);
                break;
            }}
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string tabwriterPanicDuringˢ => ref tabwriterPanicDuringˢᵇ;
private static readonly @string tabwriterPanicDuringˢᵇ = "tabwriter: panic during Flush (cannot write)"u8;

public static void TestPanicDuringFlush(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(wantPanicString, t, tabwriterPanicDuringˢ, ref ᒐ, recovers: true);
        panicWriter p = default;
        var w = @new<tabwriter.Writer>();
        w.Init(p, 0, 0, 5, (byte)' ', 0);
        io.WriteString(new tabwriter.WriterжWriter(w), "a");
        w.Flush();
        t.Errorf("failed to panic during Flush");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string tabwriterPanicDuringˢ2 => ref tabwriterPanicDuringˢ2ᵇ;
private static readonly @string tabwriterPanicDuringˢ2ᵇ = "tabwriter: panic during Write (cannot write)"u8;

public static void TestPanicDuringWrite(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(wantPanicString, t, tabwriterPanicDuringˢ2, ref ᒐ, recovers: true);
        panicWriter p = default;
        var w = @new<tabwriter.Writer>();
        w.Init(p, 0, 0, 5, (byte)' ', 0);
        io.WriteString(new tabwriter.WriterжWriter(w), "a\n\n"); // the second \n triggers a call to w.Write and thus a panic
        t.Errorf("failed to panic during Write");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string newˢ => ref newˢᵇ;
private static readonly @string newˢᵇ = "new"u8;
private static ref readonly @string reuseˢ => ref reuseˢᵇ;
private static readonly @string reuseˢᵇ = "reuse"u8;

public static void BenchmarkTable(ж<testing.B> b) {
    foreach (var (_, w) in range(new nint[]{1, 10, 100}.array())) {
        // Build a line with w cells.
        ref var line = ref heap<slice<byte>>(bytes.Repeat(slice<byte>("a\t"u8), w), out var Ꮡline);
        line = append(line, (byte)'\n');
        foreach (var (_, h) in range(new nint[]{10, 1000, 100000}.array())) {
            b.Run(fmt.Sprintf("%dx%d"u8, w, h), (ж<testing.B> bΔ1) => {
                bΔ1.Run(newˢ, (ж<testing.B> bΔ2) => {
                    bΔ2.ReportAllocs();
                    for (nint i = 0; i < (~bΔ2).N; i++) {
                        var wΔ1 = NewWriter(io.Discard, 4, 4, 1, (byte)' ', 0); // no particular reason for these settings
                        // Write the line h times.
                        for (nint j = 0; j < h; j++) {
                            wΔ1.Write(Ꮡline.ValueSlot);
                        }
                        wΔ1.Flush();
                    }
                });

                bΔ1.Run(reuseˢ, (ж<testing.B> bΔ3) => {
                    bΔ3.ReportAllocs();
                    var wΔ2 = NewWriter(io.Discard, 4, 4, 1, (byte)' ', 0); // no particular reason for these settings
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

public static void BenchmarkPyramid(ж<testing.B> b) {
    foreach (var (_, x) in range(new nint[]{10, 100, 1000}.array())) {
        // Build a line with x cells.
        var line = bytes.Repeat(slice<byte>("a\t"u8), x);
        var lineʗ1 = line;
        b.Run(fmt.Sprintf("%d"u8, x), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var w = NewWriter(io.Discard, 4, 4, 1, (byte)' ', 0); // no particular reason for these settings
                // Write increasing prefixes of that line.
                for (nint j = 0; j < x; j++) {
                    w.Write(lineʗ1.slice(0, j * 2));
                    w.Write([(byte)'\n']);
                }
                w.Flush();
            }
        });
    }
}

public static void BenchmarkRagged(ж<testing.B> b) {
    ref var lines = ref heap(new array<slice<byte>>(8), out var Ꮡlines);
    foreach (var (i, w) in range(new nint[]{6, 2, 9, 5, 5, 7, 3, 8}.array())) {
        // Build a line with w cells.
        lines[i] = bytes.Repeat(slice<byte>("a\t"u8), w);
    }
    foreach (var (_, h) in range(new nint[]{10, 100, 1000}.array())) {
        var linesʗ1 = lines;
        b.Run(fmt.Sprintf("%d"u8, h), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var w = NewWriter(io.Discard, 4, 4, 1, (byte)' ', 0); // no particular reason for these settings
                // Write the lines in turn h times.
                for (nint j = 0; j < h; j++) {
                    w.Write(linesʗ1[j % len(linesʗ1)]);
                    w.Write([(byte)'\n']);
                }
                w.Flush();
            }
        });
    }
}

internal static ref readonly @string codeSnippet => ref codeSnippetᵇ;
private static readonly @string codeSnippetᵇ = """

some command

foo	# aligned
barbaz	# comments

but
mostly
single
cell
lines

"""u8.Raw();

public static void BenchmarkCode(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        var w = NewWriter(io.Discard, 4, 4, 1, (byte)' ', 0); // no particular reason for these settings
        // The code is small, so it's reasonable for the tabwriter user
        // to write it all at once, or buffer the writes.
        w.Write(slice<byte>(codeSnippet));
        w.Flush();
    }
}

} // end tabwriter_test_package
