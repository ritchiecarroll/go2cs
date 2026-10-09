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
public static void TestIsPrint(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    nint n = 0;
    for (var r = (rune)0; r <= Δunicode.MaxRune; r++) {
        if (IsPrint(r) != Δunicode.IsPrint(r)) {
            Ꮡt.Errorf("IsPrint(%U)=%t incorrect"u8, r, IsPrint(r));
            n++;
            if (n > 10) {
                return;
            }
        }
    }
}

// Verify that our IsGraphic agrees with unicode.IsGraphic.
public static void TestIsGraphic(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    nint n = 0;
    for (var r = (rune)0; r <= Δunicode.MaxRune; r++) {
        if (IsGraphic(r) != Δunicode.IsGraphic(r)) {
            Ꮡt.Errorf("IsGraphic(%U)=%t incorrect"u8, r, IsGraphic(r));
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

// Some non-printable but graphic runes. Final column is double-quoted.
internal static slice<quoteTest> quotetests = new quoteTest[]{
    new("\a\b\f\r\n\t\v"u8, @"""\a\b\f\r\n\t\v"""u8, @"""\a\b\f\r\n\t\v"""u8, @"""\a\b\f\r\n\t\v"""u8),
    new("\\"u8, @"""\\"""u8, @"""\\"""u8, @"""\\"""u8),
    new(((@string)(new byte[]{0x61, 0x62, 0x63, 0xff, 0x64, 0x65, 0x66})), @"""abc\xffdef"""u8, @"""abc\xffdef"""u8, @"""abc\xffdef"""u8),
    new("\u263a"u8, @"""☺"""u8, @"""\u263a"""u8, @"""☺"""u8),
    new("\U0010ffff"u8, @"""\U0010ffff"""u8, @"""\U0010ffff"""u8, @"""\U0010ffff"""u8),
    new("\x04"u8, @"""\x04"""u8, @"""\x04"""u8, @"""\x04"""u8),
    new("!\u00a0!\u2000!\u3000!"u8, @"""!\u00a0!\u2000!\u3000!"""u8, @"""!\u00a0!\u2000!\u3000!"""u8, "\"!\u00a0!\u2000!\u3000!\""u8),
    new("\x7f"u8, @"""\x7f"""u8, @"""\x7f"""u8, @"""\x7f"""u8)
}.slice();

public static void TestQuote(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in quotetests) {
        {
            @string @out = Quote(tt.@in); if (@out != tt.@out) {
                Ꮡt.Errorf("Quote(%s) = %s, want %s"u8, tt.@in, @out, tt.@out);
            }
        }
        {
            var @out = AppendQuote(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.@out) {
                Ꮡt.Errorf("AppendQuote(%q, %s) = %s, want %s"u8, abcˢ, tt.@in, @out, "abc" + tt.@out);
            }
        }
    }
}

public static void TestQuoteToASCII(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in quotetests) {
        {
            @string @out = QuoteToASCII(tt.@in); if (@out != tt.ascii) {
                Ꮡt.Errorf("QuoteToASCII(%s) = %s, want %s"u8, tt.@in, @out, tt.ascii);
            }
        }
        {
            var @out = AppendQuoteToASCII(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.ascii) {
                Ꮡt.Errorf("AppendQuoteToASCII(%q, %s) = %s, want %s"u8, abcˢ, tt.@in, @out, "abc" + tt.ascii);
            }
        }
    }
}

public static void TestQuoteToGraphic(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in quotetests) {
        {
            @string @out = QuoteToGraphic(tt.@in); if (@out != tt.graphic) {
                Ꮡt.Errorf("QuoteToGraphic(%s) = %s, want %s"u8, tt.@in, @out, tt.graphic);
            }
        }
        {
            var @out = AppendQuoteToGraphic(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.graphic) {
                Ꮡt.Errorf("AppendQuoteToGraphic(%q, %s) = %s, want %s"u8, abcˢ, tt.@in, @out, "abc" + tt.graphic);
            }
        }
    }
}

public static void BenchmarkQuote(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Quote("\a\b\f\r\n\t\v\a\b\f\r\n\t\v\a\b\f\r\n\t\v"u8);
    }
}

public static void BenchmarkQuoteRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        QuoteRune((rune)'\a');
    }
}

internal static slice<byte> benchQuoteBuf;

public static void BenchmarkAppendQuote(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        benchQuoteBuf = AppendQuote(benchQuoteBuf[..0], "\a\b\f\r\n\t\v\a\b\f\r\n\t\v\a\b\f\r\n\t\v"u8);
    }
}

internal static slice<byte> benchQuoteRuneBuf;

public static void BenchmarkAppendQuoteRune(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        benchQuoteRuneBuf = AppendQuoteRune(benchQuoteRuneBuf[..0], (rune)'\a');
    }
}

partial struct quoteRuneTest {
    internal rune @in;
    internal @string @out;
    internal @string ascii;
    internal @string graphic;
}

// Some differences between graphic and printable. Note the last column is double-quoted.
internal static slice<quoteRuneTest> quoterunetests = new quoteRuneTest[]{
    new((rune)'a', @"'a'"u8, @"'a'"u8, @"'a'"u8),
    new((rune)'\a', @"'\a'"u8, @"'\a'"u8, @"'\a'"u8),
    new((rune)'\\', @"'\\'"u8, @"'\\'"u8, @"'\\'"u8),
    new(0xFF, @"'ÿ'"u8, @"'\u00ff'"u8, @"'ÿ'"u8),
    new(0x263a, @"'☺'"u8, @"'\u263a'"u8, @"'☺'"u8),
    new(0xdead, @"'�'"u8, @"'\ufffd'"u8, @"'�'"u8),
    new(0xfffd, @"'�'"u8, @"'\ufffd'"u8, @"'�'"u8),
    new(0x0010ffff, @"'\U0010ffff'"u8, @"'\U0010ffff'"u8, @"'\U0010ffff'"u8),
    new(0x0010ffff + 1, @"'�'"u8, @"'\ufffd'"u8, @"'�'"u8),
    new(0x04, @"'\x04'"u8, @"'\x04'"u8, @"'\x04'"u8),
    new((rune)'\u00a0', @"'\u00a0'"u8, @"'\u00a0'"u8, "'\u00a0'"u8),
    new((rune)'\u2000', @"'\u2000'"u8, @"'\u2000'"u8, "'\u2000'"u8),
    new((rune)'\u3000', @"'\u3000'"u8, @"'\u3000'"u8, "'\u3000'"u8)
}.slice();

public static void TestQuoteRune(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in quoterunetests) {
        {
            @string @out = QuoteRune(tt.@in); if (@out != tt.@out) {
                Ꮡt.Errorf("QuoteRune(%U) = %s, want %s"u8, tt.@in, @out, tt.@out);
            }
        }
        {
            var @out = AppendQuoteRune(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.@out) {
                Ꮡt.Errorf("AppendQuoteRune(%q, %U) = %s, want %s"u8, abcˢ, tt.@in, @out, "abc" + tt.@out);
            }
        }
    }
}

public static void TestQuoteRuneToASCII(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in quoterunetests) {
        {
            @string @out = QuoteRuneToASCII(tt.@in); if (@out != tt.ascii) {
                Ꮡt.Errorf("QuoteRuneToASCII(%U) = %s, want %s"u8, tt.@in, @out, tt.ascii);
            }
        }
        {
            var @out = AppendQuoteRuneToASCII(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.ascii) {
                Ꮡt.Errorf("AppendQuoteRuneToASCII(%q, %U) = %s, want %s"u8, abcˢ, tt.@in, @out, "abc" + tt.ascii);
            }
        }
    }
}

public static void TestQuoteRuneToGraphic(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in quoterunetests) {
        {
            @string @out = QuoteRuneToGraphic(tt.@in); if (@out != tt.graphic) {
                Ꮡt.Errorf("QuoteRuneToGraphic(%U) = %s, want %s"u8, tt.@in, @out, tt.graphic);
            }
        }
        {
            var @out = AppendQuoteRuneToGraphic(slice<byte>("abc"u8), tt.@in); if (((@string)@out) != "abc"u8 + tt.graphic) {
                Ꮡt.Errorf("AppendQuoteRuneToGraphic(%q, %U) = %s, want %s"u8, abcˢ, tt.@in, @out, "abc" + tt.graphic);
            }
        }
    }
}

partial struct canBackquoteTest {
    internal @string @in;
    internal bool @out;
}

// \t
internal static slice<canBackquoteTest> canbackquotetests = new canBackquoteTest[]{
    new("`"u8, false),
    new(((@string)(rune)0), false),
    new(((@string)(rune)1), false),
    new(((@string)(rune)2), false),
    new(((@string)(rune)3), false),
    new(((@string)(rune)4), false),
    new(((@string)(rune)5), false),
    new(((@string)(rune)6), false),
    new(((@string)(rune)7), false),
    new(((@string)(rune)8), false),
    new(((@string)(rune)9), true),
    new(((@string)(rune)10), false),
    new(((@string)(rune)11), false),
    new(((@string)(rune)12), false),
    new(((@string)(rune)13), false),
    new(((@string)(rune)14), false),
    new(((@string)(rune)15), false),
    new(((@string)(rune)16), false),
    new(((@string)(rune)17), false),
    new(((@string)(rune)18), false),
    new(((@string)(rune)19), false),
    new(((@string)(rune)20), false),
    new(((@string)(rune)21), false),
    new(((@string)(rune)22), false),
    new(((@string)(rune)23), false),
    new(((@string)(rune)24), false),
    new(((@string)(rune)25), false),
    new(((@string)(rune)26), false),
    new(((@string)(rune)27), false),
    new(((@string)(rune)28), false),
    new(((@string)(rune)29), false),
    new(((@string)(rune)30), false),
    new(((@string)(rune)31), false),
    new(((@string)(rune)0x7F), false),
    new(@"' !""#$%&'()*+,-./:;<=>?@[\]^_{|}~"u8, true),
    new(@"0123456789"u8, true),
    new(@"ABCDEFGHIJKLMNOPQRSTUVWXYZ"u8, true),
    new(@"abcdefghijklmnopqrstuvwxyz"u8, true),
    new(@"☺"u8, true),
    new(((@string)(new byte[]{0x80})), false),
    new(((@string)(new byte[]{0x61, 0xe0, 0xa0, 0x7a})), false),
    new("\ufeffabc"u8, false),
    new("a\ufeffz"u8, false)
}.slice();

public static void TestCanBackquote(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in canbackquotetests) {
        {
            var @out = CanBackquote(tt.@in); if (@out != tt.@out) {
                Ꮡt.Errorf("CanBackquote(%q) = %v, want %v"u8, tt.@in, @out, tt.@out);
            }
        }
    }
}

partial struct unQuoteTest {
    internal @string @in;
    internal @string @out;
}

internal static slice<unQuoteTest> unquotetests = new unQuoteTest[]{
    new(@""""""u8, ""u8),
    new(@"""a"""u8, "a"u8),
    new(@"""abc"""u8, "abc"u8),
    new(@"""☺"""u8, "☺"u8),
    new(@"""hello world"""u8, "hello world"u8),
    new(@"""\xFF"""u8, ((@string)(new byte[]{0xff}))),
    new(@"""\377"""u8, ((@string)(new byte[]{0xff}))),
    new(@"""\u1234"""u8, "\u1234"u8),
    new(@"""\U00010111"""u8, "\U00010111"u8),
    new(@"""\U0001011111"""u8, "\U0001011111"u8),
    new(@"""\a\b\f\n\r\t\v\\\"""""u8, "\a\b\f\n\r\t\v\\\""u8),
    new(@"""'"""u8, "'"u8),
    new(@"'a'"u8, "a"u8),
    new(@"'☹'"u8, "☹"u8),
    new(@"'\a'"u8, "\a"u8),
    new(@"'\x10'"u8, "\x10"u8),
    new(@"'\377'"u8, ((@string)(new byte[]{0xff}))),
    new(@"'\u1234'"u8, "\u1234"u8),
    new(@"'\U00010111'"u8, "\U00010111"u8),
    new(@"'\t'"u8, "\t"u8),
    new(@"' '"u8, " "u8),
    new(@"'\''"u8, "'"u8),
    new(@"'""'"u8, "\""u8),
    new("``"u8, @""u8),
    new("`a`"u8, @"a"u8),
    new("`abc`"u8, @"abc"u8),
    new("`☺`"u8, @"☺"u8),
    new("`hello world`"u8, @"hello world"u8),
    new("`\\xFF`"u8, @"\xFF"u8),
    new("`\\377`"u8, @"\377"u8),
    new("`\\`"u8, @"\"u8),
    new("`\n`"u8, "\n"u8),
    new("`	`"u8, @"	"u8),
    new("` `"u8, @" "u8),
    new("`a\rb`"u8, "ab"u8)
}.slice();

internal static slice<@string> misquoted = new @string[]{
    @""u8,
    @""""u8,
    @"""a"u8,
    @"""'"u8,
    @"b"""u8,
    @"""\"""u8,
    @"""\9"""u8,
    @"""\19"""u8,
    @"""\129"""u8,
    @"'\'"u8,
    @"'\9'"u8,
    @"'\19'"u8,
    @"'\129'"u8,
    @"'ab'"u8,
    @"""\x1!"""u8,
    @"""\U12345678"""u8,
    @"""\z"""u8,
    "`"u8,
    "`xxx"u8,
    "``x\r"u8,
    "`\""u8,
    @"""\'"""u8,
    @"'\""'"u8,
    "\"\n\""u8,
    "\"\\n\n\""u8,
    "'\n'"u8,
    @"""\udead"""u8,
    @"""\ud83d\ude4f"""u8
}.slice();

public static void TestUnquote(ж<testing.T> Ꮡt) {
    foreach (var (_, tt) in unquotetests) {
        testUnquote(Ꮡt, tt.@in, tt.@out, default!);
    }
    foreach (var (_, tt) in quotetests) {
        testUnquote(Ꮡt, tt.@out, tt.@in, default!);
    }
    foreach (var (_, s) in misquoted) {
        testUnquote(Ꮡt, s, ""u8, ErrSyntax);
    }
}

internal partial struct TestUnquoteInvalidUTF8_tests /*dyn*/ {
    internal @string @in;
    // one of:
    internal @string want;
    internal error wantErr;
}

// Issue 23685: invalid UTF-8 should not go through the fast path.
public static void TestUnquoteInvalidUTF8(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var tests = new TestUnquoteInvalidUTF8_tests[]{
        new(@in: @"""foo"""u8, want: "foo"u8),
        new(@in: @"""foo"u8, wantErr: ErrSyntax),
        new(@in: @""""u8 + ((@string)(new byte[]{0xc0})) + @""""u8, want: ((@string)(new byte[]{0xef, 0xbf, 0xbd}))),
        new(@in: @"""a"u8 + ((@string)(new byte[]{0xc0})) + @""""u8, want: ((@string)(new byte[]{0x61, 0xef, 0xbf, 0xbd}))),
        new(@in: @"""\t"u8 + ((@string)(new byte[]{0xc0})) + @""""u8, want: ((@string)(new byte[]{0x09, 0xef, 0xbf, 0xbd})))
    }.slice();
    foreach (var (_, tt) in tests) {
        testUnquote(Ꮡt, tt.@in, tt.want, tt.wantErr);
    }
}

internal static void testUnquote(ж<testing.T> Ꮡt, @string @in, @string want, error wantErr) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Test Unquote.
    var (got, gotErr) = Unquote(@in);
    if (got != want || !AreEqual(gotErr, wantErr)) {
        Ꮡt.Errorf("Unquote(%q) = (%q, %v), want (%q, %v)"u8, @in, got, gotErr, want, wantErr);
    }
    // Test QuotedPrefix.
    // Adding an arbitrary suffix should not change the result of QuotedPrefix
    // assume that the suffix doesn't accidentally terminate a truncated input.
    if (gotErr == default!) {
        want = @in;
    }
    @string suffix = "\n\r\\\"`'"u8; // special characters for quoted strings
    if (len(@in) > 0) {
        suffix = strings.ReplaceAll(suffix, @in[..1], ""u8);
    }
    @in += suffix;
    (got, gotErr) = QuotedPrefix(@in);
    if (gotErr == default! && wantErr != default!) {
        (_, wantErr) = Unquote(got); // original input had trailing junk, reparse with only valid prefix
        want = got;
    }
    if (got != want || !AreEqual(gotErr, wantErr)) {
        Ꮡt.Errorf("QuotedPrefix(%q) = (%q, %v), want (%q, %v)"u8, @in, got, gotErr, want, wantErr);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string giveMeARockPaperAndˢ = @"""Give me a rock, paper and scissors and I will move the world."""u8;

public static void BenchmarkUnquoteEasy(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Unquote(giveMeARockPaperAndˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string x47iveMeAX72ockX70aperˢ = @"""\x47ive me a \x72ock, \x70aper and \x73cissors and \x49 will move the world."""u8;

public static void BenchmarkUnquoteHard(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        Unquote(x47iveMeAX72ockX70aperˢ);
    }
}

} // end strconv_test_package
