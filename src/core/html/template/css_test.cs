// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using static go.html.template_package;

partial class template_internal_test_package {

internal partial struct TestEndsWithCSSKeyword_tests /*dyn*/ {
    internal @string css, kw;
    internal bool want;
}

public static void TestEndsWithCSSKeyword(ж<testing.T> t) {
    var tests = slice<TestEndsWithCSSKeyword_tests>([
        new("", "url", false),
        new("url", "url", true),
        new("URL", "url", true),
        new("Url", "url", true),
        new("url", "important", false),
        new("important", "important", true),
        new("image-url", "url", false),
        new("imageurl", "url", false),
        new("image url", "url", true)
    ]);
    foreach (var (_, test) in range(tests)) {
        var got = endsWithCSSKeyword(slice<byte>(test.css), test.kw);
        if (got != test.want) {
            t.Errorf("want %t but got %t for css=%v, kw=%v", test.want, got, test.css, test.kw);
        }
    }
}

internal partial struct TestIsCSSNmchar_tests /*dyn*/ {
    internal rune rune;
    internal bool want;
}

public static void TestIsCSSNmchar(ж<testing.T> t) {
    var tests = slice<TestIsCSSNmchar_tests>([
        new(0, false),
        new('0', true),
        new('9', true),
        new('A', true),
        new('Z', true),
        new('a', true),
        new('z', true),
        new('_', true),
        new('-', true),
        new(':', false),
        new(';', false),
        new(' ', false),
        new(0x7f, false),
        new(0x80, true),
        new(0x1234, true),
        new(0xd800, false),
        new(0xdc00, false),
        new(0xfffe, false),
        new(0x10000, true),
        new(0x110000, false)
    ]);
    foreach (var (_, test) in range(tests)) {
        var got = isCSSNmchar(test.rune);
        if (got != test.want) {
            t.Errorf("%q: want %t but got %t", (@string)test.rune, test.want, got);
        }
    }
}

internal partial struct TestDecodeCSS_tests /*dyn*/ {
    internal @string css, want;
}

public static void TestDecodeCSS(ж<testing.T> t) {
    var tests = slice<TestDecodeCSS_tests>([
        new(@"", @""),
        new(@"foo", @"foo"),
        new(@"foo\", @"foo"),
        new(@"foo\\", @"foo\"),
        new(@"\", @""),
        new(@"\A", "\n"),
        new(@"\a", "\n"),
        new(@"\0a", "\n"),
        new(@"\00000a", "\n"),
        new(@"\000000a", "\u0000a"),
        new(@"\1234 5", "\u1234"u8 + "5"u8),
        new(@"\1234\20 5", "\u1234"u8 + " 5"u8),
        new(@"\1234\A 5", "\u1234"u8 + "\n5"u8),
        new("\\1234\t5", "\u1234"u8 + "5"u8),
        new("\\1234\n5", "\u1234"u8 + "5"u8),
        new("\\1234\r\n5", "\u1234"u8 + "5"u8),
        new(@"\12345", "\U00012345"),
        new(@"\\", @"\"),
        new(@"\\ ", @"\ "),
        new(@"\""", @""""),
        new(@"\'", @"'"),
        new(@"\.", @"."),
        new(@"\. .", @". ."),
        new(
            @"The \3c i\3equick\3c/i\3e,\d\A\3cspan style=\27 color:brown\27\3e brown\3c/span\3e  fox jumps\2028over the \3c canine class=\22lazy\22 \3e dog\3c/canine\3e",
            "The <i>quick</i>,\r\n<span style='color:brown'>brown</span> fox jumps\u2028over the <canine class=\"lazy\">dog</canine>"
        )
    ]);
    foreach (var (_, test) in range(tests)) {
        @string got1 = (@string)decodeCSS(slice<byte>(test.css));
        if (got1 != test.want) {
            t.Errorf("%q: want\n\t%q\nbut got\n\t%q", test.css, test.want, got1);
        }
        @string recoded = cssEscaper(got1);
        @string got2 = (@string)decodeCSS(slice<byte>(recoded)); if (got2 != test.want) {
            t.Errorf("%q: escape & decode not dual for %q", test.css, recoded);
        }
    }
}

public static void TestHexDecode(ж<testing.T> t) {
    for (nint i = 0; i < 0x200000; i += 101) {
        /* coprime with 16 */
        @string s = strconv.FormatInt((int64)i, 16);
        {
            nint got = (nint)hexDecode(slice<byte>(s)); if (got != i) {
                t.Errorf("%s: want %d but got %d", s, i, got);
            }
        }
        s = strings.ToUpper(s);
        {
            nint got = (nint)hexDecode(slice<byte>(s)); if (got != i) {
                t.Errorf("%s: want %d but got %d", s, i, got);
            }
        }
    }
}

internal partial struct TestSkipCSSSpace_tests /*dyn*/ {
    internal @string css, want;
}

public static void TestSkipCSSSpace(ж<testing.T> t) {
    var tests = slice<TestSkipCSSSpace_tests>([
        new("", ""),
        new("foo", "foo"),
        new("\n", ""),
        new("\r\n", ""),
        new("\r", ""),
        new("\t", ""),
        new(" ", ""),
        new("\f", ""),
        new(" foo", "foo"),
        new("  foo", " foo"),
        new(@"\20", @"\20")
    ]);
    foreach (var (_, test) in range(tests)) {
        @string got = (@string)skipCSSSpace(slice<byte>(test.css));
        if (got != test.want) {
            t.Errorf("%q: want %q but got %q", test.css, test.want, got);
        }
    }
}

public static void TestCSSEscaper(ж<testing.T> t) {
    @string input = ("\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f"u8 + "\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"u8 + @" !""#$%&'()*+,-./"u8 + @"0123456789:;<=>?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\]^_"u8 + "`abcdefghijklmno"u8 + "pqrstuvwxyz{|}~\x7f"u8 + "\u00A0\u0100\u2028\u2029\ufeff\U0001D11E"u8);

    @string want = ("\\0\x01\x02\x03\x04\x05\x06\x07"u8 + "\x08\\9 \\a\x0b\\c \\d\x0E\x0F"u8 + "\x10\x11\x12\x13\x14\x15\x16\x17"u8 + "\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"u8 + @" !\22#$%\26\27\28\29*\2b,-.\2f "u8 + @"0123456789\3a\3b\3c=\3e?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\\]^_"u8 + "`abcdefghijklmno"u8 + @"pqrstuvwxyz\7b|\7d~"u8 + "\u007f"u8 + "\u00A0\u0100\u2028\u2029\ufeff\U0001D11E"u8);

    @string got = cssEscaper(input);
    if (got != want) {
        t.Errorf("encode: want\n\t%q\nbut got\n\t%q", want, got);
    }

    got = (@string)decodeCSS(slice<byte>(got));
    if (input != got) {
        t.Errorf("decode: want\n\t%q\nbut got\n\t%q", input, got);
    }
}

internal partial struct TestCSSValueFilter_tests /*dyn*/ {
    internal @string css, want;
}

public static void TestCSSValueFilter(ж<testing.T> t) {
    var tests = slice<TestCSSValueFilter_tests>([
        new("", ""),
        new("foo", "foo"),
        new("0", "0"),
        new("0px", "0px"),
        new("-5px", "-5px"),
        new("1.25in", "1.25in"),
        new("+.33em", "+.33em"),
        new("100%", "100%"),
        new("12.5%", "12.5%"),
        new(".foo", ".foo"),
        new("#bar", "#bar"),
        new("corner-radius", "corner-radius"),
        new("-moz-corner-radius", "-moz-corner-radius"),
        new("#000", "#000"),
        new("#48f", "#48f"),
        new("#123456", "#123456"),
        new("U+00-FF, U+980-9FF", "U+00-FF, U+980-9FF"),
        new("color: red", "color: red"),
        new("<!--", "ZgotmplZ"),
        new("-->", "ZgotmplZ"),
        new("<![CDATA[", "ZgotmplZ"),
        new("]]>", "ZgotmplZ"),
        new("</style", "ZgotmplZ"),
        new(@"""", "ZgotmplZ"),
        new(@"'", "ZgotmplZ"),
        new("`", "ZgotmplZ"),
        new("\x00", "ZgotmplZ"),
        new("/* foo */", "ZgotmplZ"),
        new("//", "ZgotmplZ"),
        new("[href=~", "ZgotmplZ"),
        new("expression(alert(1337))", "ZgotmplZ"),
        new("-expression(alert(1337))", "ZgotmplZ"),
        new("expression", "ZgotmplZ"),
        new("Expression", "ZgotmplZ"),
        new("EXPRESSION", "ZgotmplZ"),
        new("-moz-binding", "ZgotmplZ"),
        new(((@string)(new byte[]{0x2d, 0x65, 0x78, 0x70, 0x72, 0x00, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x28, 0x61, 0x6c, 0x65, 0x72, 0x74, 0x28, 0x31, 0x33, 0x33, 0x37, 0x29, 0x29})), "ZgotmplZ"),
        new(@"-expr\0ession(alert(1337))", "ZgotmplZ"),
        new(@"-express\69on(alert(1337))", "ZgotmplZ"),
        new(@"-express\69 on(alert(1337))", "ZgotmplZ"),
        new(@"-exp\72 ession(alert(1337))", "ZgotmplZ"),
        new(@"-exp\52 ession(alert(1337))", "ZgotmplZ"),
        new(@"-exp\000052 ession(alert(1337))", "ZgotmplZ"),
        new(@"-expre\0000073sion", ((@string)(new byte[]{0x2d, 0x65, 0x78, 0x70, 0x72, 0x65, 0x07, 0x33, 0x73, 0x69, 0x6f, 0x6e}))),
        new(@"@import url evil.css", "ZgotmplZ"),
        new("<", "ZgotmplZ"),
        new(">", "ZgotmplZ")
    ]);
    foreach (var (_, test) in range(tests)) {
        @string got = cssValueFilter(test.css);
        if (got != test.want) {
            t.Errorf("%q: want %q but got %q", test.css, test.want, got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string theIQuickISpanStyleColorˢ => ref theIQuickISpanStyleColorˢᵇ;
private static readonly @string theIQuickISpanStyleColorˢᵇ = "The <i>quick</i>,\r\n<span style='color:brown'>brown</span> fox jumps\u2028over the <canine class=\"lazy\">dog</canine>"u8;

public static void BenchmarkCSSEscaper(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        cssEscaper(theIQuickISpanStyleColorˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string theQuickBrownFoxJumpsˢ => ref theQuickBrownFoxJumpsˢᵇ;
private static readonly @string theQuickBrownFoxJumpsˢᵇ = "The quick, brown fox jumps over the lazy dog."u8;

public static void BenchmarkCSSEscaperNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        cssEscaper(theQuickBrownFoxJumpsˢ);
    }
}

public static void BenchmarkDecodeCSS(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var s = slice<byte>(@"The \3c i\3equick\3c/i\3e,\d\A\3cspan style=\27 color:brown\27\3e brown\3c/span\3e fox jumps\2028over the \3c canine class=\22lazy\22 \3edog\3c/canine\3e"u8);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        decodeCSS(s);
    }
}

public static void BenchmarkDecodeCSSNoSpecials(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var s = slice<byte>("The quick, brown fox jumps over the lazy dog."u8);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        decodeCSS(s);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object e78preS0SioNAlert1337ˢ = (@string)@"  e\78preS\0Sio/**/n(alert(1337))"u8;

public static void BenchmarkCSSValueFilter(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        cssValueFilter(e78preS0SioNAlert1337ˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object timesNewRomanˢ = (@string)@"Times New Roman"u8;

public static void BenchmarkCSSValueFilterOk(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        cssValueFilter(timesNewRomanˢ);
    }
}

} // end template_internal_test_package
