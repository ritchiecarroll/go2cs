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

public static void TestEndsWithCSSKeyword(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestEndsWithCSSKeyword_tests[]{
        new(""u8, "url"u8, false),
        new("url"u8, "url"u8, true),
        new("URL"u8, "url"u8, true),
        new("Url"u8, "url"u8, true),
        new("url"u8, "important"u8, false),
        new("important"u8, "important"u8, true),
        new("image-url"u8, "url"u8, false),
        new("imageurl"u8, "url"u8, false),
        new("image url"u8, "url"u8, true)
    }.slice();
    foreach (var (_, test) in tests) {
        var got = endsWithCSSKeyword(slice<byte>(test.css), test.kw);
        if (got != test.want) {
            Ꮡt.Errorf("want %t but got %t for css=%v, kw=%v"u8, test.want, got, test.css, test.kw);
        }
    }
}

internal partial struct TestIsCSSNmchar_tests /*dyn*/ {
    internal rune rune;
    internal bool want;
}

public static void TestIsCSSNmchar(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestIsCSSNmchar_tests[]{
        new(0, false),
        new((rune)'0', true),
        new((rune)'9', true),
        new((rune)'A', true),
        new((rune)'Z', true),
        new((rune)'a', true),
        new((rune)'z', true),
        new((rune)'_', true),
        new((rune)'-', true),
        new((rune)':', false),
        new((rune)';', false),
        new((rune)' ', false),
        new(0x7f, false),
        new(0x80, true),
        new(0x1234, true),
        new(0xd800, false),
        new(0xdc00, false),
        new(0xfffe, false),
        new(0x10000, true),
        new(0x110000, false)
    }.slice();
    foreach (var (_, test) in tests) {
        var got = isCSSNmchar(test.rune);
        if (got != test.want) {
            Ꮡt.Errorf("%q: want %t but got %t"u8, ((@string)test.rune), test.want, got);
        }
    }
}

internal partial struct TestDecodeCSS_tests /*dyn*/ {
    internal @string css, want;
}

public static void TestDecodeCSS(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestDecodeCSS_tests[]{
        new(@""u8, @""u8),
        new(@"foo"u8, @"foo"u8),
        new(@"foo\"u8, @"foo"u8),
        new(@"foo\\"u8, @"foo\"u8),
        new(@"\"u8, @""u8),
        new(@"\A"u8, "\n"u8),
        new(@"\a"u8, "\n"u8),
        new(@"\0a"u8, "\n"u8),
        new(@"\00000a"u8, "\n"u8),
        new(@"\000000a"u8, "\u0000a"u8),
        new(@"\1234 5"u8, "\u1234"u8 + "5"u8),
        new(@"\1234\20 5"u8, "\u1234"u8 + " 5"u8),
        new(@"\1234\A 5"u8, "\u1234"u8 + "\n5"u8),
        new("\\1234\t5"u8, "\u1234"u8 + "5"u8),
        new("\\1234\n5"u8, "\u1234"u8 + "5"u8),
        new("\\1234\r\n5"u8, "\u1234"u8 + "5"u8),
        new(@"\12345"u8, "\U00012345"u8),
        new(@"\\"u8, @"\"u8),
        new(@"\\ "u8, @"\ "u8),
        new(@"\"""u8, @""""u8),
        new(@"\'"u8, @"'"u8),
        new(@"\."u8, @"."u8),
        new(@"\. ."u8, @". ."u8),
        new(
            @"The \3c i\3equick\3c/i\3e,\d\A\3cspan style=\27 color:brown\27\3e brown\3c/span\3e  fox jumps\2028over the \3c canine class=\22lazy\22 \3e dog\3c/canine\3e"u8,
            "The <i>quick</i>,\r\n<span style='color:brown'>brown</span> fox jumps\u2028over the <canine class=\"lazy\">dog</canine>"u8
        )
    }.slice();
    foreach (var (_, test) in tests) {
        @string got1 = ((@string)decodeCSS(slice<byte>(test.css)));
        if (got1 != test.want) {
            Ꮡt.Errorf("%q: want\n\t%q\nbut got\n\t%q"u8, test.css, test.want, got1);
        }
        @string recoded = cssEscaper(got1);
        {
            @string got2 = ((@string)decodeCSS(slice<byte>(recoded))); if (got2 != test.want) {
                Ꮡt.Errorf("%q: escape & decode not dual for %q"u8, test.css, recoded);
            }
        }
    }
}

public static void TestHexDecode(ж<testing.T> Ꮡt) {
    for (nint i = 0; i < 0x200000; i += 101) {
        /* coprime with 16 */
        @string s = strconv.FormatInt((int64)i, 16);
        {
            nint got = (nint)hexDecode(slice<byte>(s)); if (got != i) {
                Ꮡt.Errorf("%s: want %d but got %d"u8, s, i, got);
            }
        }
        s = strings.ToUpper(s);
        {
            nint got = (nint)hexDecode(slice<byte>(s)); if (got != i) {
                Ꮡt.Errorf("%s: want %d but got %d"u8, s, i, got);
            }
        }
    }
}

internal partial struct TestSkipCSSSpace_tests /*dyn*/ {
    internal @string css, want;
}

public static void TestSkipCSSSpace(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestSkipCSSSpace_tests[]{
        new(""u8, ""u8),
        new("foo"u8, "foo"u8),
        new("\n"u8, ""u8),
        new("\r\n"u8, ""u8),
        new("\r"u8, ""u8),
        new("\t"u8, ""u8),
        new(" "u8, ""u8),
        new("\f"u8, ""u8),
        new(" foo"u8, "foo"u8),
        new("  foo"u8, " foo"u8),
        new(@"\20"u8, @"\20"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        @string got = ((@string)skipCSSSpace(slice<byte>(test.css)));
        if (got != test.want) {
            Ꮡt.Errorf("%q: want %q but got %q"u8, test.css, test.want, got);
        }
    }
}

public static void TestCSSEscaper(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    @string input = ("\x00\x01\x02\x03\x04\x05\x06\x07\x08\t\n\x0b\x0c\r\x0e\x0f"u8 + "\x10\x11\x12\x13\x14\x15\x16\x17\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"u8 + @" !""#$%&'()*+,-./"u8 + @"0123456789:;<=>?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\]^_"u8 + "`abcdefghijklmno"u8 + "pqrstuvwxyz{|}~\x7f"u8 + "\u00A0\u0100\u2028\u2029\ufeff\U0001D11E"u8);
    @string want = ("\\0\x01\x02\x03\x04\x05\x06\x07"u8 + "\x08\\9 \\a\x0b\\c \\d\x0E\x0F"u8 + "\x10\x11\x12\x13\x14\x15\x16\x17"u8 + "\x18\x19\x1a\x1b\x1c\x1d\x1e\x1f"u8 + @" !\22#$%\26\27\28\29*\2b,-.\2f "u8 + @"0123456789\3a\3b\3c=\3e?"u8 + @"@ABCDEFGHIJKLMNO"u8 + @"PQRSTUVWXYZ[\\]^_"u8 + "`abcdefghijklmno"u8 + @"pqrstuvwxyz\7b|\7d~"u8 + "\u007f"u8 + "\u00A0\u0100\u2028\u2029\ufeff\U0001D11E"u8);
    @string got = cssEscaper(input);
    if (got != want) {
        Ꮡt.Errorf("encode: want\n\t%q\nbut got\n\t%q"u8, want, got);
    }
    got = ((@string)decodeCSS(slice<byte>(got)));
    if (input != got) {
        Ꮡt.Errorf("decode: want\n\t%q\nbut got\n\t%q"u8, input, got);
    }
}

internal partial struct TestCSSValueFilter_tests /*dyn*/ {
    internal @string css, want;
}

public static void TestCSSValueFilter(ж<testing.T> Ꮡt) {
    var tests = new TestCSSValueFilter_tests[]{
        new(""u8, ""u8),
        new("foo"u8, "foo"u8),
        new("0"u8, "0"u8),
        new("0px"u8, "0px"u8),
        new("-5px"u8, "-5px"u8),
        new("1.25in"u8, "1.25in"u8),
        new("+.33em"u8, "+.33em"u8),
        new("100%"u8, "100%"u8),
        new("12.5%"u8, "12.5%"u8),
        new(".foo"u8, ".foo"u8),
        new("#bar"u8, "#bar"u8),
        new("corner-radius"u8, "corner-radius"u8),
        new("-moz-corner-radius"u8, "-moz-corner-radius"u8),
        new("#000"u8, "#000"u8),
        new("#48f"u8, "#48f"u8),
        new("#123456"u8, "#123456"u8),
        new("U+00-FF, U+980-9FF"u8, "U+00-FF, U+980-9FF"u8),
        new("color: red"u8, "color: red"u8),
        new("<!--"u8, "ZgotmplZ"u8),
        new("-->"u8, "ZgotmplZ"u8),
        new("<![CDATA["u8, "ZgotmplZ"u8),
        new("]]>"u8, "ZgotmplZ"u8),
        new("</style"u8, "ZgotmplZ"u8),
        new(@""""u8, "ZgotmplZ"u8),
        new(@"'"u8, "ZgotmplZ"u8),
        new("`"u8, "ZgotmplZ"u8),
        new("\x00"u8, "ZgotmplZ"u8),
        new("/* foo */"u8, "ZgotmplZ"u8),
        new("//"u8, "ZgotmplZ"u8),
        new("[href=~"u8, "ZgotmplZ"u8),
        new("expression(alert(1337))"u8, "ZgotmplZ"u8),
        new("-expression(alert(1337))"u8, "ZgotmplZ"u8),
        new("expression"u8, "ZgotmplZ"u8),
        new("Expression"u8, "ZgotmplZ"u8),
        new("EXPRESSION"u8, "ZgotmplZ"u8),
        new("-moz-binding"u8, "ZgotmplZ"u8),
        new(((@string)(new byte[]{0x2d, 0x65, 0x78, 0x70, 0x72, 0x00, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x28, 0x61, 0x6c, 0x65, 0x72, 0x74, 0x28, 0x31, 0x33, 0x33, 0x37, 0x29, 0x29})), "ZgotmplZ"u8),
        new(@"-expr\0ession(alert(1337))"u8, "ZgotmplZ"u8),
        new(@"-express\69on(alert(1337))"u8, "ZgotmplZ"u8),
        new(@"-express\69 on(alert(1337))"u8, "ZgotmplZ"u8),
        new(@"-exp\72 ession(alert(1337))"u8, "ZgotmplZ"u8),
        new(@"-exp\52 ession(alert(1337))"u8, "ZgotmplZ"u8),
        new(@"-exp\000052 ession(alert(1337))"u8, "ZgotmplZ"u8),
        new(@"-expre\0000073sion"u8, ((@string)(new byte[]{0x2d, 0x65, 0x78, 0x70, 0x72, 0x65, 0x07, 0x33, 0x73, 0x69, 0x6f, 0x6e}))),
        new(@"@import url evil.css"u8, "ZgotmplZ"u8),
        new("<"u8, "ZgotmplZ"u8),
        new(">"u8, "ZgotmplZ"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        @string got = cssValueFilter(test.css);
        if (got != test.want) {
            Ꮡt.Errorf("%q: want %q but got %q"u8, test.css, test.want, got);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string theIQuickISpanStyleColorˢ = "The <i>quick</i>,\r\n<span style='color:brown'>brown</span> fox jumps\u2028over the <canine class=\"lazy\">dog</canine>"u8;

public static void BenchmarkCSSEscaper(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        cssEscaper(theIQuickISpanStyleColorˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string theQuickBrownFoxJumpsˢ = "The quick, brown fox jumps over the lazy dog."u8;

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
