// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static strconv_package;
using strings = strings_package;
using testing = testing_package;
using Δunicode = unicode_package;
using static go.strconv_internal_test_package;

partial class strconv_test_package {

// Verify that our IsPrint agrees with unicode.IsPrint.
public static void TestIsPrint(ж<testing.T> t) {
    nint n = 0;
    for (var r = (rune)0; r <= Δunicode.MaxRune; r++) {
        if (IsPrint(r) != Δunicode.IsPrint(r)) {
            t.Errorf("IsPrint(%U)=%t incorrect", r, IsPrint(r));
            n++;
            if (n > 10) {
                return;
            }
        }
    }
}

// Verify that our IsGraphic agrees with unicode.IsGraphic.
public static void TestIsGraphic(ж<testing.T> t) {
    nint n = 0;
    for (var r = (rune)0; r <= Δunicode.MaxRune; r++) {
        if (IsGraphic(r) != Δunicode.IsGraphic(r)) {
            t.Errorf("IsGraphic(%U)=%t incorrect", r, IsGraphic(r));
            n++;
            if (n > 10) {
                return;
            }
        }
    }
}

partial struct quoteTest {
    internal @string @in;
    internal @string @out;
    internal @string ascii;
    internal @string graphic;
}

internal static slice<quoteTest> quotetests = slice<quoteTest>([
    new("\a\b\f\r\n\t\v", @"""\a\b\f\r\n\t\v""", @"""\a\b\f\r\n\t\v""", @"""\a\b\f\r\n\t\v"""),
    new("\\", @"""\\""", @"""\\""", @"""\\"""),
    new(((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x64, 0x65, 0x66})), @"""abc\xffdef""", @"""abc\xffdef""", @"""abc\xffdef"""),
    new("\u263a", @"""☺""", @"""\u263a""", @"""☺"""),
    new("\U0010ffff", @"""\U0010ffff""", @"""\U0010ffff""", @"""\U0010ffff"""),
    new("\x04", @"""\x04""", @"""\x04""", @"""\x04"""),
    // Some non-printable but graphic runes. Final column is double-quoted.
    new("!\u00a0!\u2000!\u3000!", @"""!\u00a0!\u2000!\u3000!""", @"""!\u00a0!\u2000!\u3000!""", "\"!\u00a0!\u2000!\u3000!\""),
    new("\x7f", @"""\x7f""", @"""\x7f""", @"""\x7f""")
]);

public static void TestQuote(ж<testing.T> t) {
    foreach (var (_, tt) in quotetests) {
        {
            @string @out = Quote(tt.@in); if (@out != tt.@out) {
                t.Errorf("Quote(%s) = %s, want %s", tt.@in, @out, tt.@out);
            }
        }
        {
            var @out = AppendQuote(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.@out) {
                t.Errorf("AppendQuote(%q, %s) = %s, want %s", abcˢ, tt.@in, @out, "abc" + tt.@out);
            }
        }
    }
}

public static void TestQuoteToASCII(ж<testing.T> t) {
    foreach (var (_, tt) in quotetests) {
        {
            @string @out = QuoteToASCII(tt.@in); if (@out != tt.ascii) {
                t.Errorf("QuoteToASCII(%s) = %s, want %s", tt.@in, @out, tt.ascii);
            }
        }
        {
            var @out = AppendQuoteToASCII(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.ascii) {
                t.Errorf("AppendQuoteToASCII(%q, %s) = %s, want %s", abcˢ, tt.@in, @out, "abc" + tt.ascii);
            }
        }
    }
}

public static void TestQuoteToGraphic(ж<testing.T> t) {
    foreach (var (_, tt) in quotetests) {
        {
            @string @out = QuoteToGraphic(tt.@in); if (@out != tt.graphic) {
                t.Errorf("QuoteToGraphic(%s) = %s, want %s", tt.@in, @out, tt.graphic);
            }
        }
        {
            var @out = AppendQuoteToGraphic(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.graphic) {
                t.Errorf("AppendQuoteToGraphic(%q, %s) = %s, want %s", abcˢ, tt.@in, @out, "abc" + tt.graphic);
            }
        }
    }
}

public static void BenchmarkQuote(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Quote("\a\b\f\r\n\t\v\a\b\f\r\n\t\v\a\b\f\r\n\t\v");
    }
}

public static void BenchmarkQuoteRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        QuoteRune('\a');
    }
}

internal static slice<byte> benchQuoteBuf;

public static void BenchmarkAppendQuote(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        benchQuoteBuf = AppendQuote(benchQuoteBuf[..0], "\a\b\f\r\n\t\v\a\b\f\r\n\t\v\a\b\f\r\n\t\v");
    }
}

internal static slice<byte> benchQuoteRuneBuf;

public static void BenchmarkAppendQuoteRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        benchQuoteRuneBuf = AppendQuoteRune(benchQuoteRuneBuf[..0], '\a');
    }
}

partial struct quoteRuneTest {
    internal rune @in;
    internal @string @out;
    internal @string ascii;
    internal @string graphic;
}

internal static slice<quoteRuneTest> quoterunetests = slice<quoteRuneTest>([
    new('a', @"'a'", @"'a'", @"'a'"),
    new('\a', @"'\a'", @"'\a'", @"'\a'"),
    new('\\', @"'\\'", @"'\\'", @"'\\'"),
    new(0xFF, @"'ÿ'", @"'\u00ff'", @"'ÿ'"),
    new(0x263a, @"'☺'", @"'\u263a'", @"'☺'"),
    new(0xdead, @"'�'", @"'\ufffd'", @"'�'"),
    new(0xfffd, @"'�'", @"'\ufffd'", @"'�'"),
    new(0x0010ffff, @"'\U0010ffff'", @"'\U0010ffff'", @"'\U0010ffff'"),
    new(0x0010ffff + 1, @"'�'", @"'\ufffd'", @"'�'"),
    new(0x04, @"'\x04'", @"'\x04'", @"'\x04'"),
    // Some differences between graphic and printable. Note the last column is double-quoted.
    new('\u00a0', @"'\u00a0'", @"'\u00a0'", "'\u00a0'"),
    new('\u2000', @"'\u2000'", @"'\u2000'", "'\u2000'"),
    new('\u3000', @"'\u3000'", @"'\u3000'", "'\u3000'")
]);

public static void TestQuoteRune(ж<testing.T> t) {
    foreach (var (_, tt) in quoterunetests) {
        {
            @string @out = QuoteRune(tt.@in); if (@out != tt.@out) {
                t.Errorf("QuoteRune(%U) = %s, want %s", tt.@in, @out, tt.@out);
            }
        }
        {
            var @out = AppendQuoteRune(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.@out) {
                t.Errorf("AppendQuoteRune(%q, %U) = %s, want %s", abcˢ, tt.@in, @out, "abc" + tt.@out);
            }
        }
    }
}

public static void TestQuoteRuneToASCII(ж<testing.T> t) {
    foreach (var (_, tt) in quoterunetests) {
        {
            @string @out = QuoteRuneToASCII(tt.@in); if (@out != tt.ascii) {
                t.Errorf("QuoteRuneToASCII(%U) = %s, want %s", tt.@in, @out, tt.ascii);
            }
        }
        {
            var @out = AppendQuoteRuneToASCII(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.ascii) {
                t.Errorf("AppendQuoteRuneToASCII(%q, %U) = %s, want %s", abcˢ, tt.@in, @out, "abc" + tt.ascii);
            }
        }
    }
}

public static void TestQuoteRuneToGraphic(ж<testing.T> t) {
    foreach (var (_, tt) in quoterunetests) {
        {
            @string @out = QuoteRuneToGraphic(tt.@in); if (@out != tt.graphic) {
                t.Errorf("QuoteRuneToGraphic(%U) = %s, want %s", tt.@in, @out, tt.graphic);
            }
        }
        {
            var @out = AppendQuoteRuneToGraphic(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.graphic) {
                t.Errorf("AppendQuoteRuneToGraphic(%q, %U) = %s, want %s", abcˢ, tt.@in, @out, "abc" + tt.graphic);
            }
        }
    }
}

partial struct canBackquoteTest {
    internal @string @in;
    internal bool @out;
}

internal static slice<canBackquoteTest> canbackquotetests = slice<canBackquoteTest>([
    new("`", false),
    new((@string)(rune)0, false),
    new((@string)(rune)1, false),
    new((@string)(rune)2, false),
    new((@string)(rune)3, false),
    new((@string)(rune)4, false),
    new((@string)(rune)5, false),
    new((@string)(rune)6, false),
    new((@string)(rune)7, false),
    new((@string)(rune)8, false),
    new((@string)(rune)9, true), // \t
    new((@string)(rune)10, false),
    new((@string)(rune)11, false),
    new((@string)(rune)12, false),
    new((@string)(rune)13, false),
    new((@string)(rune)14, false),
    new((@string)(rune)15, false),
    new((@string)(rune)16, false),
    new((@string)(rune)17, false),
    new((@string)(rune)18, false),
    new((@string)(rune)19, false),
    new((@string)(rune)20, false),
    new((@string)(rune)21, false),
    new((@string)(rune)22, false),
    new((@string)(rune)23, false),
    new((@string)(rune)24, false),
    new((@string)(rune)25, false),
    new((@string)(rune)26, false),
    new((@string)(rune)27, false),
    new((@string)(rune)28, false),
    new((@string)(rune)29, false),
    new((@string)(rune)30, false),
    new((@string)(rune)31, false),
    new((@string)(rune)0x7F, false),
    new(@"' !""#$%&'()*+,-./:;<=>?@[\]^_{|}~", true),
    new(@"0123456789", true),
    new(@"ABCDEFGHIJKLMNOPQRSTUVWXYZ", true),
    new(@"abcdefghijklmnopqrstuvwxyz", true),
    new(@"☺", true),
    new(((@string)(new byte[]{0x80})), false),
    new(((@string)(new byte[]{0x61, 0xe0, 0xa0, 0x7a})), false),
    new("\ufeffabc", false),
    new("a\ufeffz", false)
]);

public static void TestCanBackquote(ж<testing.T> t) {
    foreach (var (_, tt) in canbackquotetests) {
        {
            var @out = CanBackquote(tt.@in); if (@out != tt.@out) {
                t.Errorf("CanBackquote(%q) = %v, want %v", tt.@in, @out, tt.@out);
            }
        }
    }
}

partial struct unQuoteTest {
    internal @string @in;
    internal @string @out;
}

internal static slice<unQuoteTest> unquotetests = slice<unQuoteTest>([
    new(@"""""", ""),
    new(@"""a""", "a"),
    new(@"""abc""", "abc"),
    new(@"""☺""", "☺"),
    new(@"""hello world""", "hello world"),
    new(@"""\xFF""", ((@string)(new byte[]{0xff}))),
    new(@"""\377""", ((@string)(new byte[]{0xff}))),
    new(@"""\u1234""", "\u1234"),
    new(@"""\U00010111""", "\U00010111"),
    new(@"""\U0001011111""", "\U0001011111"),
    new(@"""\a\b\f\n\r\t\v\\\""""", "\a\b\f\n\r\t\v\\\""),
    new(@"""'""", "'"),

    new(@"'a'", "a"),
    new(@"'☹'", "☹"),
    new(@"'\a'", "\a"),
    new(@"'\x10'", "\x10"),
    new(@"'\377'", ((@string)(new byte[]{0xff}))),
    new(@"'\u1234'", "\u1234"),
    new(@"'\U00010111'", "\U00010111"),
    new(@"'\t'", "\t"),
    new(@"' '", " "),
    new(@"'\''", "'"),
    new(@"'""'", "\""),

    new("``", @""),
    new("`a`", @"a"),
    new("`abc`", @"abc"),
    new("`☺`", @"☺"),
    new("`hello world`", @"hello world"),
    new("`\\xFF`", @"\xFF"),
    new("`\\377`", @"\377"),
    new("`\\`", @"\"),
    new("`\n`", "\n"),
    new("`	`", @"	"),
    new("` `", @" "),
    new("`a\rb`", "ab")
]);

internal static slice<@string> misquoted = slice<@string>([
    @"",
    @"""",
    @"""a",
    @"""'",
    @"b""",
    @"""\""",
    @"""\9""",
    @"""\19""",
    @"""\129""",
    @"'\'",
    @"'\9'",
    @"'\19'",
    @"'\129'",
    @"'ab'",
    @"""\x1!""",
    @"""\U12345678""",
    @"""\z""",
    "`",
    "`xxx",
    "``x\r",
    "`\"",
    @"""\'""",
    @"'\""'",
    "\"\n\"",
    "\"\\n\n\"",
    "'\n'",
    @"""\udead""",
    @"""\ud83d\ude4f"""
]);

public static void TestUnquote(ж<testing.T> t) {
    foreach (var (_, tt) in unquotetests) {
        testUnquote(t, tt.@in, tt.@out, default);
    }
    foreach (var (_, tt) in quotetests) {
        testUnquote(t, tt.@out, tt.@in, default);
    }
    foreach (var (_, s) in misquoted) {
        testUnquote(t, s, ""u8, ErrSyntax);
    }
}

internal partial struct TestUnquoteInvalidUTF8_tests /*dyn*/ {
    internal @string @in;
    // one of:
    internal @string want;
    internal error wantErr;
}

// Issue 23685: invalid UTF-8 should not go through the fast path.
public static void TestUnquoteInvalidUTF8(ж<testing.T> t) {
    var tests = slice<TestUnquoteInvalidUTF8_tests>([
        new(@in: @"""foo""", want: "foo"),
        new(@in: @"""foo", wantErr: ErrSyntax),
        new(@in: @""""u8 + ((@string)(new byte[]{0xc0})) + @""""u8, want: ((@string)(new byte[]{0xef, 0xbf, 0xbd}))),
        new(@in: @"""a"u8 + ((@string)(new byte[]{0xc0})) + @""""u8, want: ((@string)(new byte[]{0x61, 0xef, 0xbf, 0xbd}))),
        new(@in: @"""\t"u8 + ((@string)(new byte[]{0xc0})) + @""""u8, want: ((@string)(new byte[]{0x09, 0xef, 0xbf, 0xbd})))
    ]);
    foreach (var (_, tt) in tests) {
        testUnquote(t, tt.@in, tt.want, tt.wantErr);
    }
}

internal static void testUnquote(ж<testing.T> t, @string @in, @string want, error wantErr) {
    // Test Unquote.
    var (got, gotErr) = Unquote(@in);
    if (got != want || !AreEqual(gotErr, wantErr)) {
        t.Errorf("Unquote(%q) = (%q, %v), want (%q, %v)", @in, got, gotErr, want, wantErr);
    }

    // Test QuotedPrefix.
    // Adding an arbitrary suffix should not change the result of QuotedPrefix
    // assume that the suffix doesn't accidentally terminate a truncated input.
    if (gotErr == nil) {
        want = @in;
    }
    @string suffix = "\n\r\\\"`'"; // special characters for quoted strings
    if (len(@in) > 0) {
        suffix = strings.ReplaceAll(suffix, @in[..1], ""u8);
    }
    @in += suffix;
    (got, gotErr) = QuotedPrefix(@in);
    if (gotErr == nil && wantErr != nil) {
        (_, wantErr) = Unquote(got); // original input had trailing junk, reparse with only valid prefix
        want = got;
    }
    if (got != want || !AreEqual(gotErr, wantErr)) {
        t.Errorf("QuotedPrefix(%q) = (%q, %v), want (%q, %v)", @in, got, gotErr, want, wantErr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string giveMeARockPaperAndˢ => ref giveMeARockPaperAndˢᵇ;
private static readonly @string giveMeARockPaperAndˢᵇ = @"""Give me a rock, paper and scissors and I will move the world."""u8;

public static void BenchmarkUnquoteEasy(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Unquote(giveMeARockPaperAndˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string x47iveMeAX72ockX70aperˢ => ref x47iveMeAX72ockX70aperˢᵇ;
private static readonly @string x47iveMeAX72ockX70aperˢᵇ = @"""\x47ive me a \x72ock, \x70aper and \x73cissors and \x49 will move the world."""u8;

public static void BenchmarkUnquoteHard(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Unquote(x47iveMeAX72ockX70aperˢ);
    }
}

} // end strconv_test_package
