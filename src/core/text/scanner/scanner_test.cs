// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.text;

using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;
using strings = strings_package;
using testing = testing_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using static go.text.scanner_package;

partial class scanner_internal_test_package {

// A StringReader delivers its data one string segment at a time via Read.
public partial struct StringReader {
    internal slice<@string> data;
    internal nint step;
}

public static (nint n, error err) Read(this ref StringReader r, slice<byte> p) {
    nint n = default;
    error err = default;

    if (r.step < len(r.data)) {
        @string s = r.data[r.step];
        n = copy(p, s);
        r.step++;
    } else {
        err = io.EOF;
    }
    return (n, err);
}

internal static void readRuneSegments(ж<testing.T> t, slice<@string> segments) {
    @string got = ""u8;
    @string want = strings.Join(segments, ""u8);
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_internal_test_package.StringReaderжReader(Ꮡ(new StringReader(data: segments))));
    for (;;) {
        var ch = s.Next();
        if (ch == EOF) {
            break;
        }
        got += ((@string)ch);
    }
    if (got != want) {
        t.Errorf("segments=%v got=%s want=%s", segments, got, want);
    }
}

internal static slice<slice<@string>> segmentList = [
    [],
    [""],
    ["日", "本語"],
    ["\u65e5", "\u672c", "\u8a9e"],
    ["\U000065e5", " ", "\U0000672c", "\U00008a9e"],
    [((@string)(new byte[]{0xe6})), ((@string)(new byte[]{0x97, 0xa5, 0xe6})), ((@string)(new byte[]{0x9c, 0xac, 0xe8, 0xaa, 0x9e}))],
    ["Hello", ", ", "World", "!"],
    ["Hello", ", ", "", "World", "!"]
];

public static void TestNext(ж<testing.T> t) {
    foreach (var (_, s) in range(segmentList)) {
        readRuneSegments(t, s);
    }
}

internal partial struct token {
    internal rune tok;
    internal @string text;
}

internal static @string f100 = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";

internal static slice<token> tokenList = [
    new(Comment, "// line comments"),
    new(Comment, "//"),
    new(Comment, "////"),
    new(Comment, "// comment"),
    new(Comment, "// /* comment */"),
    new(Comment, "// // comment //"),
    new(Comment, "//"u8 + f100),

    new(Comment, "// general comments"),
    new(Comment, "/**/"),
    new(Comment, "/***/"),
    new(Comment, "/* comment */"),
    new(Comment, "/* // comment */"),
    new(Comment, "/* /* comment */"),
    new(Comment, "/*\n comment\n*/"),
    new(Comment, "/*"u8 + f100 + "*/"u8),

    new(Comment, "// identifiers"),
    new(Ident, "a"),
    new(Ident, "a0"),
    new(Ident, "foobar"),
    new(Ident, "abc123"),
    new(Ident, "LGTM"),
    new(Ident, "_"),
    new(Ident, "_abc123"),
    new(Ident, "abc123_"),
    new(Ident, "_abc_123_"),
    new(Ident, "_äöü"),
    new(Ident, "_本"),
    new(Ident, "äöü"),
    new(Ident, "本"),
    new(Ident, "a۰۱۸"),
    new(Ident, "foo६४"),
    new(Ident, "bar９８７６"),
    new(Ident, f100),

    new(Comment, "// decimal ints"),
    new(Int, "0"),
    new(Int, "1"),
    new(Int, "9"),
    new(Int, "42"),
    new(Int, "1234567890"),

    new(Comment, "// octal ints"),
    new(Int, "00"),
    new(Int, "01"),
    new(Int, "07"),
    new(Int, "042"),
    new(Int, "01234567"),

    new(Comment, "// hexadecimal ints"),
    new(Int, "0x0"),
    new(Int, "0x1"),
    new(Int, "0xf"),
    new(Int, "0x42"),
    new(Int, "0x123456789abcDEF"),
    new(Int, "0x"u8 + f100),
    new(Int, "0X0"),
    new(Int, "0X1"),
    new(Int, "0XF"),
    new(Int, "0X42"),
    new(Int, "0X123456789abcDEF"),
    new(Int, "0X"u8 + f100),

    new(Comment, "// floats"),
    new(Float, "0."),
    new(Float, "1."),
    new(Float, "42."),
    new(Float, "01234567890."),
    new(Float, ".0"),
    new(Float, ".1"),
    new(Float, ".42"),
    new(Float, ".0123456789"),
    new(Float, "0.0"),
    new(Float, "1.0"),
    new(Float, "42.0"),
    new(Float, "01234567890.0"),
    new(Float, "0e0"),
    new(Float, "1e0"),
    new(Float, "42e0"),
    new(Float, "01234567890e0"),
    new(Float, "0E0"),
    new(Float, "1E0"),
    new(Float, "42E0"),
    new(Float, "01234567890E0"),
    new(Float, "0e+10"),
    new(Float, "1e-10"),
    new(Float, "42e+10"),
    new(Float, "01234567890e-10"),
    new(Float, "0E+10"),
    new(Float, "1E-10"),
    new(Float, "42E+10"),
    new(Float, "01234567890E-10"),

    new(Comment, "// chars"),
    new(Char, @"' '"),
    new(Char, @"'a'"),
    new(Char, @"'本'"),
    new(Char, @"'\a'"),
    new(Char, @"'\b'"),
    new(Char, @"'\f'"),
    new(Char, @"'\n'"),
    new(Char, @"'\r'"),
    new(Char, @"'\t'"),
    new(Char, @"'\v'"),
    new(Char, @"'\''"),
    new(Char, @"'\000'"),
    new(Char, @"'\777'"),
    new(Char, @"'\x00'"),
    new(Char, @"'\xff'"),
    new(Char, @"'\u0000'"),
    new(Char, @"'\ufA16'"),
    new(Char, @"'\U00000000'"),
    new(Char, @"'\U0000ffAB'"),

    new(Comment, "// strings"),
    new(ΔString, @""" """),
    new(ΔString, @"""a"""),
    new(ΔString, @"""本"""),
    new(ΔString, @"""\a"""),
    new(ΔString, @"""\b"""),
    new(ΔString, @"""\f"""),
    new(ΔString, @"""\n"""),
    new(ΔString, @"""\r"""),
    new(ΔString, @"""\t"""),
    new(ΔString, @"""\v"""),
    new(ΔString, @"""\"""""),
    new(ΔString, @"""\000"""),
    new(ΔString, @"""\777"""),
    new(ΔString, @"""\x00"""),
    new(ΔString, @"""\xff"""),
    new(ΔString, @"""\u0000"""),
    new(ΔString, @"""\ufA16"""),
    new(ΔString, @"""\U00000000"""),
    new(ΔString, @"""\U0000ffAB"""),
    new(ΔString, @""""u8 + f100 + @""""u8),

    new(Comment, "// raw strings"),
    new(RawString, "``"),
    new(RawString, "`\\`"),
    new(RawString, "`"u8 + "\n\n/* foobar */\n\n"u8 + "`"u8),
    new(RawString, "`"u8 + f100 + "`"u8),

    new(Comment, "// individual characters"),
    // NUL character is not allowed
    new('\x01', "\x01"),
    new((rune)' ' - 1, (@string)((rune)(rune)' ' - 1)),
    new('+', "+"),
    new('/', "/"),
    new('.', "."),
    new('~', "~"),
    new('(', "(")
];

internal static ж<bytes.Buffer> makeSource(@string pattern) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (_, k) in range(tokenList)) {
        fmt.Fprintf(new scanner_test_package.bytes_BufferжWriter(Ꮡbuf), pattern, k.text);
    }
    return Ꮡbuf;
}

internal static void checkTok(ж<testing.T> t, ж<global::go.text.scanner_package.Scanner> Ꮡs, nint line, rune got, rune want, @string text) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (got != want) {
        t.Fatalf("tok = %s, want %s for %q", TokenString(got), TokenString(want), text);
    }
    if (s.Line != line) {
        t.Errorf("line = %d, want %d for %q", s.Line, line, text);
    }
    @string stext = Ꮡs.TokenText();
    if (stext != text) {
        t.Errorf("text = %q, want %q", stext, text);
    } else {
        // check idempotency of TokenText() call
        stext = Ꮡs.TokenText();
        if (stext != text) {
            t.Errorf("text = %q, want %q (idempotency check)", stext, text);
        }
    }
}

internal static void checkTokErr(ж<testing.T> t, ж<global::go.text.scanner_package.Scanner> Ꮡs, nint line, rune want, @string text) {
    ref var s = ref Ꮡs.DerefOrNull();

    nint prevCount = s.ErrorCount;
    checkTok(t, Ꮡs, line, Ꮡs.Scan(), want, text);
    if (s.ErrorCount != prevCount + 1) {
        t.Fatalf("want error for %q", text);
    }
}

internal static nint countNewlines(@string s) {
    nint n = 0;
    foreach (var (_, ch) in s) {
        if (ch == '\n') {
            n++;
        }
    }
    return n;
}

internal static void testScan(ж<testing.T> t, nuint mode) {
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.bytes_BufferжReader(makeSource(" \t%s\n")));
    s.Mode = mode;
    var tok = s.Scan();
    nint line = 1;
    foreach (var (_, k) in range(tokenList)) {
        if ((mode & (nuint)SkipComments) == 0 || k.tok != Comment) {
            checkTok(t, s, line, tok, k.tok, k.text);
            tok = s.Scan();
        }
        line += countNewlines(k.text) + 1; // each token is on a new line
    }
    checkTok(t, s, line, tok, EOF, ""u8);
}

public static void TestScan(ж<testing.T> t) {
    testScan(t, GoTokens);
    testScan(t, (nuint)((nuint)GoTokens & ~(nuint)(nuint)SkipComments));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ => ref srcᶜᵇ;
private static readonly @string srcᶜᵇ = "1.5e 1.5E 1e+ 1e- 1.5z"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ => ref wantᶜᵇ;
private static readonly @string wantᶜᵇ = "exponent has no digits"u8;

public static void TestInvalidExponent(ж<testing.T> t) {
    @string src = srcᶜ;
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(src)));
    s.Error = (ж<global::go.text.scanner_package.Scanner> sΔ1, @string msg) => {
        @string want = wantᶜ;
        if (msg != want) {
            t.Errorf("%s: got error %q; want %q", sΔ1.TokenText(), msg, want);
        }
    };
    checkTokErr(t, s, 1, Float, "1.5e");
    checkTokErr(t, s, 1, Float, "1.5E");
    checkTokErr(t, s, 1, Float, "1e+");
    checkTokErr(t, s, 1, Float, "1e-");
    checkTok(t, s, 1, s.Scan(), Float, "1.5");
    checkTok(t, s, 1, s.Scan(), Ident, "z");
    checkTok(t, s, 1, s.Scan(), EOF, ""u8);
    if (s.ErrorCount != 4) {
        t.Errorf("%d errors, want 4", s.ErrorCount);
    }
}

public static void TestPosition(ж<testing.T> t) {
    var src = makeSource("\t\t\t\t%s\n");
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.bytes_BufferжReader(src));
    s.Mode = (nuint)((nuint)GoTokens & ~(nuint)(nuint)SkipComments);
    s.Scan();
    var pos = new Position("", 4, 1, 5);
    foreach (var (_, k) in range(tokenList)) {
        if (s.Offset != pos.Offset) {
            t.Errorf("offset = %d, want %d for %q", s.Offset, pos.Offset, k.text);
        }
        if (s.Line != pos.Line) {
            t.Errorf("line = %d, want %d for %q", s.Line, pos.Line, k.text);
        }
        if (s.Column != pos.Column) {
            t.Errorf("column = %d, want %d for %q", s.Column, pos.Column, k.text);
        }
        pos.Offset += 4 + len(k.text) + 1;     // 4 tabs + token bytes + newline
        pos.Line += countNewlines(k.text) + 1; // each token is on a new line
        s.Scan();
    }
    // make sure there were no token-internal errors reported by scanner
    if (s.ErrorCount != 0) {
        t.Errorf("%d errors", s.ErrorCount);
    }
}

public static void TestScanZeroMode(ж<testing.T> t) {
    var src = makeSource("%s\n");
    @string str = src.String();
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.bytes_BufferжReader(src));
    s.Mode = 0;       // don't recognize any token classes
    s.Whitespace = 0; // don't skip any whitespace
    var tok = s.Scan();
    foreach (var (i, ch) in str) {
        if (tok != ch) {
            t.Fatalf("%d. tok = %s, want %s", i, TokenString(tok), TokenString(ch));
        }
        tok = s.Scan();
    }
    if (tok != EOF) {
        t.Fatalf("tok = %s, want EOF", TokenString(tok));
    }
    if (s.ErrorCount != 0) {
        t.Errorf("%d errors", s.ErrorCount);
    }
}

internal static void testScanSelectedMode(ж<testing.T> t, nuint mode, rune @class) {
    var src = makeSource("%s\n");
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.bytes_BufferжReader(src));
    s.Mode = mode;
    var tok = s.Scan();
    while (tok != EOF) {
        if (tok < 0 && tok != @class) {
            t.Fatalf("tok = %s, want %s", TokenString(tok), TokenString(@class));
        }
        tok = s.Scan();
    }
    if (s.ErrorCount != 0) {
        t.Errorf("%d errors", s.ErrorCount);
    }
}

public static void TestScanSelectedMask(ж<testing.T> t) {
    testScanSelectedMode(t, 0, 0);
    testScanSelectedMode(t, ScanIdents, Ident);
    // Don't test ScanInts and ScanNumbers since some parts of
    // the floats in the source look like (invalid) octal ints
    // and ScanNumbers may return either Int or Float.
    testScanSelectedMode(t, ScanChars, Char);
    testScanSelectedMode(t, ScanStrings, ΔString);
    testScanSelectedMode(t, SkipComments, 0);
    testScanSelectedMode(t, ScanComments, Comment);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string b123ˢ => ref b123ˢᵇ;
private static readonly @string b123ˢᵇ = "b123"u8;
internal static ref readonly @string a12ˢ => ref a12ˢᵇ;
private static readonly @string a12ˢᵇ = "a12"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string srcᶜ1 => ref srcᶜ1ᵇ;
private static readonly @string srcᶜ1ᵇ = "faab12345 a12b123 a12 3b"u8;

public static void TestScanCustomIdent(ж<testing.T> t) {
    @string src = srcᶜ1;
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(src)));
    // ident = ( 'a' | 'b' ) { digit } .
    // digit = '0' .. '3' .
    // with a maximum length of 4
    s.IsIdentRune = (rune ch, nint i) => i == 0 && (ch == 'a' || ch == 'b') || 0 < i && i < 4 && '0' <= ch && ch <= '3';
    checkTok(t, s, 1, s.Scan(), 'f', "f");
    checkTok(t, s, 1, s.Scan(), Ident, "a");
    checkTok(t, s, 1, s.Scan(), Ident, "a");
    checkTok(t, s, 1, s.Scan(), Ident, b123ˢ);
    checkTok(t, s, 1, s.Scan(), Int, "45");
    checkTok(t, s, 1, s.Scan(), Ident, a12ˢ);
    checkTok(t, s, 1, s.Scan(), Ident, b123ˢ);
    checkTok(t, s, 1, s.Scan(), Ident, a12ˢ);
    checkTok(t, s, 1, s.Scan(), Int, "3");
    checkTok(t, s, 1, s.Scan(), Ident, "b");
    checkTok(t, s, 1, s.Scan(), EOF, ""u8);
}

public static void TestScanNext(ж<testing.T> t) {
    UntypedInt BOM = /* '\uFEFF' */ 65279;
    @string BOMs = (@string)(rune)BOM;
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(BOMs + "if a == bcd /* com"u8 + BOMs + "ment */ {\n\ta += c\n}"u8 + BOMs + "// line comment ending in eof"u8)));
    checkTok(t, s, 1, s.Scan(), Ident, "if"); // the first BOM is ignored
    checkTok(t, s, 1, s.Scan(), Ident, "a");
    checkTok(t, s, 1, s.Scan(), '=', "=");
    checkTok(t, s, 0, s.Next(), '=', ""u8);
    checkTok(t, s, 0, s.Next(), ' ', ""u8);
    checkTok(t, s, 0, s.Next(), 'b', ""u8);
    checkTok(t, s, 1, s.Scan(), Ident, "cd");
    checkTok(t, s, 1, s.Scan(), '{', "{");
    checkTok(t, s, 2, s.Scan(), Ident, "a");
    checkTok(t, s, 2, s.Scan(), '+', "+");
    checkTok(t, s, 0, s.Next(), '=', ""u8);
    checkTok(t, s, 2, s.Scan(), Ident, "c");
    checkTok(t, s, 3, s.Scan(), '}', "}");
    checkTok(t, s, 3, s.Scan(), BOM, BOMs);
    checkTok(t, s, 3, s.Scan(), -1, ""u8);
    if (s.ErrorCount != 0) {
        t.Errorf("%d errors", s.ErrorCount);
    }
}

public static void TestScanWhitespace(ж<testing.T> t) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    uint64 ws = default;
    // start at 1, NUL character is not allowed
    for (var ch = (byte)1; ch < ' '; ch++) {
        buf.WriteByte(ch);
        ws |= (uint64)(((uint64)1).Lsh((uint64)(ch)));
    }
    UntypedInt orig = /* 'x' */ 120;
    buf.WriteByte(orig);

    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.bytes_BufferжReader(Ꮡbuf));
    s.Mode = 0;
    s.Whitespace = ws;
    var tok = s.Scan();
    if (tok != orig) {
        t.Errorf("tok = %s, want %s", TokenString(tok), TokenString(orig));
    }
}

internal static void testError(ж<testing.T> t, @string src, @string pos, @string msg, rune tok) {
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(src)));
    var errorCalled = false;
    s.Error = (ж<global::go.text.scanner_package.Scanner> sΔ1, @string m) => {
        if (!errorCalled) {
            // only look at first error
            @string p = sΔ1.Pos().String(); if (p != pos) {
                t.Errorf("pos = %q, want %q for %q", p, pos, src);
            }
            if (m != msg) {
                t.Errorf("msg = %q, want %q for %q", m, msg, src);
            }
            errorCalled = true;
        }
    };
    var tk = s.Scan();
    if (tk != tok) {
        t.Errorf("tok = %s, want %s for %q", TokenString(tk), TokenString(tok), src);
    }
    if (!errorCalled) {
        t.Errorf("error handler not called for %q", src);
    }
    if (s.ErrorCount == 0) {
        t.Errorf("count = %d, want > 0 for %q", s.ErrorCount, src);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string input11ˢ => ref input11ˢᵇ;
private static readonly @string input11ˢᵇ = "<input>:1:1"u8;
internal static ref readonly @string input12ˢ => ref input12ˢᵇ;
private static readonly @string input12ˢᵇ = "<input>:1:2"u8;
internal static ref readonly @string input13ˢ => ref input13ˢᵇ;
private static readonly @string input13ˢᵇ = "<input>:1:3"u8;
internal static ref readonly @string input14ˢ => ref input14ˢᵇ;
private static readonly @string input14ˢᵇ = "<input>:1:4"u8;
internal static ref readonly @string input15ˢ => ref input15ˢᵇ;
private static readonly @string input15ˢᵇ = "<input>:1:5"u8;
internal static ref readonly @string input16ˢ => ref input16ˢᵇ;
private static readonly @string input16ˢᵇ = "<input>:1:6"u8;
internal static ref readonly @string invalidDigit8InOctalˢ => ref invalidDigit8InOctalˢᵇ;
private static readonly @string invalidDigit8InOctalˢᵇ = "invalid digit '8' in octal literal"u8;
internal static ref readonly @string input19ˢ => ref input19ˢᵇ;
private static readonly @string input19ˢᵇ = "<input>:1:9"u8;
internal static ref readonly @string hexadecimalLiteralHasNoˢ => ref hexadecimalLiteralHasNoˢᵇ;
private static readonly @string hexadecimalLiteralHasNoˢᵇ = "hexadecimal literal has no digits"u8;
internal static ref readonly @string abcˢ => ref abcˢᵇ;
private static readonly @string abcˢᵇ = @"""abc"u8;
internal static ref readonly @string abcˢ2 => ref abcˢ2ᵇ;
private static readonly @string abcˢ2ᵇ = "`abc\n"u8;
internal static ref readonly @string input21ˢ => ref input21ˢᵇ;
private static readonly @string input21ˢᵇ = "<input>:2:1"u8;

public static void TestError(ж<testing.T> t) {
    testError(t, "\x00", input11ˢ, invalidCharacterNulˢ, 0);
    testError(t, ((@string)(new byte[]{0x80})), input11ˢ, invalidUtf8Encodingˢ, utf8.RuneError);
    testError(t, ((@string)(new byte[]{0xff})), input11ˢ, invalidUtf8Encodingˢ, utf8.RuneError);

    testError(t, "a\x00", input12ˢ, invalidCharacterNulˢ, Ident);
    testError(t, ((@string)(new byte[]{0x61, 0x62, 0x80})), input13ˢ, invalidUtf8Encodingˢ, Ident);
    testError(t, ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff})), input14ˢ, invalidUtf8Encodingˢ, Ident);

    testError(t, @"""a"u8 + "\x00"u8, input13ˢ, invalidCharacterNulˢ, ΔString);
    testError(t, @"""ab"u8 + ((@string)(new byte[]{0x80})), input14ˢ, invalidUtf8Encodingˢ, ΔString);
    testError(t, @"""abc"u8 + ((@string)(new byte[]{0xff})), input15ˢ, invalidUtf8Encodingˢ, ΔString);

    testError(t, "`a"u8 + "\x00"u8, input13ˢ, invalidCharacterNulˢ, RawString);
    testError(t, "`ab"u8 + ((@string)(new byte[]{0x80})), input14ˢ, invalidUtf8Encodingˢ, RawString);
    testError(t, "`abc"u8 + ((@string)(new byte[]{0xff})), input15ˢ, invalidUtf8Encodingˢ, RawString);

    testError(t, @"'\""'", input13ˢ, invalidCharEscapeˢ, Char);
    testError(t, @"""\'""", input13ˢ, invalidCharEscapeˢ, ΔString);

    testError(t, @"01238", input16ˢ, invalidDigit8InOctalˢ, Int);
    testError(t, @"01238123", input19ˢ, invalidDigit8InOctalˢ, Int);
    testError(t, @"0x", input13ˢ, hexadecimalLiteralHasNoˢ, Int);
    testError(t, @"0xg", input13ˢ, hexadecimalLiteralHasNoˢ, Int);
    testError(t, @"'aa'", input14ˢ, invalidCharLiteralˢ, Char);
    testError(t, @"1.5e", input15ˢ, exponentHasNoDigitsˢ, Float);
    testError(t, @"1.5E", input15ˢ, exponentHasNoDigitsˢ, Float);
    testError(t, @"1.5e+", input16ˢ, exponentHasNoDigitsˢ, Float);
    testError(t, @"1.5e-", input16ˢ, exponentHasNoDigitsˢ, Float);

    testError(t, @"'", input12ˢ, literalNotTerminatedˢ, Char);
    testError(t, @"'"u8 + "\n"u8, input12ˢ, literalNotTerminatedˢ, Char);
    testError(t, abcˢ, input15ˢ, literalNotTerminatedˢ, ΔString);
    testError(t, @"""abc"u8 + "\n"u8, input15ˢ, literalNotTerminatedˢ, ΔString);
    testError(t, abcˢ2, input21ˢ, literalNotTerminatedˢ, RawString);
    testError(t, @"/*/", input14ˢ, commentNotTerminatedˢ, EOF);
}

// An errReader returns (0, err) where err is not io.EOF.
internal partial struct errReader {
}

internal static (nint, error) Read(this errReader _, slice<byte> b) {
    return (0, io.ErrNoProgress); // some error that is not io.EOF
}

public static void TestIOError(ж<testing.T> t) {
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new errReader());
    var errorCalled = false;
    s.Error = (ж<global::go.text.scanner_package.Scanner> sΔ1, @string msg) => {
        if (!errorCalled) {
            @string want = io.ErrNoProgress.Error(); if (msg != want) {
                t.Errorf("msg = %q, want %q", msg, want);
            }
            errorCalled = true;
        }
    };
    var tok = s.Scan();
    if (tok != EOF) {
        t.Errorf("tok = %s, want EOF", TokenString(tok));
    }
    if (!errorCalled) {
        t.Errorf("error handler not called");
    }
}

internal static partial void checkPos(ж<testing.T> t, global::go.text.scanner_package.Position got, global::go.text.scanner_package.Position want) {
    if (got.Offset != want.Offset || got.Line != want.Line || got.Column != want.Column) {
        t.Errorf("got offset, line, column = %d, %d, %d; want %d, %d, %d",
            got.Offset, got.Line, got.Column, want.Offset, want.Line, want.Column);
    }
}

internal static void checkNextPos(ж<testing.T> t, ж<global::go.text.scanner_package.Scanner> Ꮡs, nint offset, nint line, nint column, rune @char) {
    ref var s = ref Ꮡs.DerefOrNull();

    var ch = Ꮡs.Next(); if (ch != @char) {
        t.Errorf("ch = %s, want %s", TokenString(ch), TokenString(@char));
    }
    var want = new Position(Offset: offset, Line: line, Column: column);
    checkPos(t, s.Pos(), want);
}

internal static void checkScanPos(ж<testing.T> t, ж<global::go.text.scanner_package.Scanner> Ꮡs, nint offset, nint line, nint column, rune @char) {
    ref var s = ref Ꮡs.DerefOrNull();

    var want = new Position(Offset: offset, Line: line, Column: column);
    checkPos(t, s.Pos(), want);
    var ch = Ꮡs.Scan(); if (ch != @char) {
        t.Errorf("ch = %s, want %s", TokenString(ch), TokenString(@char));
        if (((@string)ch) != Ꮡs.TokenText()) {
            t.Errorf("tok = %q, want %q", Ꮡs.TokenText(), (@string)ch);
        }
    }
    checkPos(t, s.Position, want);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "  foo६४  \n\n本語\n"u8;
internal static ref readonly @string abcXˢ => ref abcXˢᵇ;
private static readonly @string abcXˢᵇ = "abc\n本語\n\nx"u8;

public static void TestPos(ж<testing.T> t) {
    // corner case: empty source
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(""u8)));
    checkPos(t, s.Pos(), new Position(Offset: 0, Line: 1, Column: 1));
    s.Peek(); // peek doesn't affect the position
    checkPos(t, s.Pos(), new Position(Offset: 0, Line: 1, Column: 1));

    // corner case: source with only a newline
    s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader("\n")));
    checkPos(t, s.Pos(), new Position(Offset: 0, Line: 1, Column: 1));
    checkNextPos(t, s, 1, 2, 1, '\n');
    // after EOF position doesn't change
    for (nint i = 10; i > 0; i--) {
        checkScanPos(t, s, 1, 2, 1, EOF);
    }
    if (s.ErrorCount != 0) {
        t.Errorf("%d errors", s.ErrorCount);
    }

    // corner case: source with only a single character
    s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader("本")));
    checkPos(t, s.Pos(), new Position(Offset: 0, Line: 1, Column: 1));
    checkNextPos(t, s, 3, 1, 2, '本');
    // after EOF position doesn't change
    for (nint i = 10; i > 0; i--) {
        checkScanPos(t, s, 3, 1, 2, EOF);
    }
    if (s.ErrorCount != 0) {
        t.Errorf("%d errors", s.ErrorCount);
    }

    // positions after calling Next
    s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(fooˢ)));
    checkNextPos(t, s, 1, 1, 2, ' ');
    s.Peek(); // peek doesn't affect the position
    checkNextPos(t, s, 2, 1, 3, ' ');
    checkNextPos(t, s, 3, 1, 4, 'f');
    checkNextPos(t, s, 4, 1, 5, 'o');
    checkNextPos(t, s, 5, 1, 6, 'o');
    checkNextPos(t, s, 8, 1, 7, '६');
    checkNextPos(t, s, 11, 1, 8, '४');
    checkNextPos(t, s, 12, 1, 9, ' ');
    checkNextPos(t, s, 13, 1, 10, ' ');
    checkNextPos(t, s, 14, 2, 1, '\n');
    checkNextPos(t, s, 15, 3, 1, '\n');
    checkNextPos(t, s, 18, 3, 2, '本');
    checkNextPos(t, s, 21, 3, 3, '語');
    checkNextPos(t, s, 22, 4, 1, '\n');
    // after EOF position doesn't change
    for (nint i = 10; i > 0; i--) {
        checkScanPos(t, s, 22, 4, 1, EOF);
    }
    if (s.ErrorCount != 0) {
        t.Errorf("%d errors", s.ErrorCount);
    }

    // positions after calling Scan
    s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(abcXˢ)));
    s.Mode = 0;
    s.Whitespace = 0;
    checkScanPos(t, s, 0, 1, 1, 'a');
    s.Peek(); // peek doesn't affect the position
    checkScanPos(t, s, 1, 1, 2, 'b');
    checkScanPos(t, s, 2, 1, 3, 'c');
    checkScanPos(t, s, 3, 1, 4, '\n');
    checkScanPos(t, s, 4, 2, 1, '本');
    checkScanPos(t, s, 7, 2, 2, '語');
    checkScanPos(t, s, 10, 2, 3, '\n');
    checkScanPos(t, s, 11, 3, 1, '\n');
    checkScanPos(t, s, 12, 4, 1, 'x');
    // after EOF position doesn't change
    for (nint i = 10; i > 0; i--) {
        checkScanPos(t, s, 13, 4, 2, EOF);
    }
    if (s.ErrorCount != 0) {
        t.Errorf("%d errors", s.ErrorCount);
    }
}

internal partial struct countReader /*num:nint*/;

internal static (nint, error) Read(this ref countReader r, slice<byte> _) {
    r++;
    return (0, io.EOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object eofNotReportedˢ = (@string)"1) EOF not reported"u8;
internal static readonly object eofNotReportedˢ2 = (@string)"2) EOF not reported"u8;

public static void TestNextEOFHandling(ж<testing.T> t) {
    ref var r = ref heap(new countReader(), out var Ꮡr);

    // corner case: empty source
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_internal_test_package.countReaderжReader(Ꮡr));

    var tok = s.Next();
    if (tok != EOF) {
        t.Error(eofNotReportedˢ);
    }

    tok = s.Peek();
    if (tok != EOF) {
        t.Error(eofNotReportedˢ2);
    }

    if (r != 1) {
        t.Errorf("scanner called Read %d times, not once", r);
    }
}

public static void TestScanEOFHandling(ж<testing.T> t) {
    ref var r = ref heap(new countReader(), out var Ꮡr);

    // corner case: empty source
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_internal_test_package.countReaderжReader(Ꮡr));

    var tok = s.Scan();
    if (tok != EOF) {
        t.Error(eofNotReportedˢ);
    }

    tok = s.Peek();
    if (tok != EOF) {
        t.Error(eofNotReportedˢ2);
    }

    if (r != 1) {
        t.Errorf("scanner called Read %d times, not once", r);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string wantᶜ1 => ref wantᶜ1ᵇ;
private static readonly @string wantᶜ1ᵇ = @""""u8;

public static void TestIssue29723(ж<testing.T> t) {
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(@"x """)));
    s.Error = (ж<global::go.text.scanner_package.Scanner> sΔ1, @string _) => {
        @string got = sΔ1.TokenText(); // this call shouldn't panic
        @string want = wantᶜ1;
        if (got != want) {
            t.Errorf("got %q; want %q", got, want);
        }
    };
    for (var r = s.Scan(); r != EOF; r = s.Scan()) {
    }
}

internal partial struct TestNumbers_type /*dyn*/ {
    internal rune tok;
    internal @string src, tokens, err;
}

public static void TestNumbers(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestNumbers_type>([
        // binaries
        new(Int, "0b0", "0b0", ""),
        new(Int, "0b1010", "0b1010", ""),
        new(Int, "0B1110", "0B1110", ""),

        new(Int, "0b", "0b", "binary literal has no digits"),
        new(Int, "0b0190", "0b0190", "invalid digit '9' in binary literal"),
        new(Int, "0b01a0", "0b01 a0", ""), // only accept 0-9

        // binary floats (invalid)
        new(Float, "0b.", "0b.", "invalid radix point in binary literal"),
        new(Float, "0b.1", "0b.1", "invalid radix point in binary literal"),
        new(Float, "0b1.0", "0b1.0", "invalid radix point in binary literal"),
        new(Float, "0b1e10", "0b1e10", "'e' exponent requires decimal mantissa"),
        new(Float, "0b1P-1", "0b1P-1", "'P' exponent requires hexadecimal mantissa"),

        // octals
        new(Int, "0o0", "0o0", ""),
        new(Int, "0o1234", "0o1234", ""),
        new(Int, "0O1234", "0O1234", ""),

        new(Int, "0o", "0o", "octal literal has no digits"),
        new(Int, "0o8123", "0o8123", "invalid digit '8' in octal literal"),
        new(Int, "0o1293", "0o1293", "invalid digit '9' in octal literal"),
        new(Int, "0o12a3", "0o12 a3", ""), // only accept 0-9

        // octal floats (invalid)
        new(Float, "0o.", "0o.", "invalid radix point in octal literal"),
        new(Float, "0o.2", "0o.2", "invalid radix point in octal literal"),
        new(Float, "0o1.2", "0o1.2", "invalid radix point in octal literal"),
        new(Float, "0o1E+2", "0o1E+2", "'E' exponent requires decimal mantissa"),
        new(Float, "0o1p10", "0o1p10", "'p' exponent requires hexadecimal mantissa"),

        // 0-octals
        new(Int, "0", "0", ""),
        new(Int, "0123", "0123", ""),

        new(Int, "08123", "08123", "invalid digit '8' in octal literal"),
        new(Int, "01293", "01293", "invalid digit '9' in octal literal"),
        new(Int, "0F.", "0 F .", ""), // only accept 0-9
        new(Int, "0123F.", "0123 F .", ""),
        new(Int, "0123456x", "0123456 x", ""),

        // decimals
        new(Int, "1", "1", ""),
        new(Int, "1234", "1234", ""),

        new(Int, "1f", "1 f", ""), // only accept 0-9

        // decimal floats
        new(Float, "0.", "0.", ""),
        new(Float, "123.", "123.", ""),
        new(Float, "0123.", "0123.", ""),

        new(Float, ".0", ".0", ""),
        new(Float, ".123", ".123", ""),
        new(Float, ".0123", ".0123", ""),

        new(Float, "0.0", "0.0", ""),
        new(Float, "123.123", "123.123", ""),
        new(Float, "0123.0123", "0123.0123", ""),

        new(Float, "0e0", "0e0", ""),
        new(Float, "123e+0", "123e+0", ""),
        new(Float, "0123E-1", "0123E-1", ""),

        new(Float, "0.e+1", "0.e+1", ""),
        new(Float, "123.E-10", "123.E-10", ""),
        new(Float, "0123.e123", "0123.e123", ""),

        new(Float, ".0e-1", ".0e-1", ""),
        new(Float, ".123E+10", ".123E+10", ""),
        new(Float, ".0123E123", ".0123E123", ""),

        new(Float, "0.0e1", "0.0e1", ""),
        new(Float, "123.123E-10", "123.123E-10", ""),
        new(Float, "0123.0123e+456", "0123.0123e+456", ""),

        new(Float, "0e", "0e", "exponent has no digits"),
        new(Float, "0E+", "0E+", "exponent has no digits"),
        new(Float, "1e+f", "1e+ f", "exponent has no digits"),
        new(Float, "0p0", "0p0", "'p' exponent requires hexadecimal mantissa"),
        new(Float, "1.0P-1", "1.0P-1", "'P' exponent requires hexadecimal mantissa"),

        // hexadecimals
        new(Int, "0x0", "0x0", ""),
        new(Int, "0x1234", "0x1234", ""),
        new(Int, "0xcafef00d", "0xcafef00d", ""),
        new(Int, "0XCAFEF00D", "0XCAFEF00D", ""),

        new(Int, "0x", "0x", "hexadecimal literal has no digits"),
        new(Int, "0x1g", "0x1 g", ""),

        // hexadecimal floats
        new(Float, "0x0p0", "0x0p0", ""),
        new(Float, "0x12efp-123", "0x12efp-123", ""),
        new(Float, "0xABCD.p+0", "0xABCD.p+0", ""),
        new(Float, "0x.0189P-0", "0x.0189P-0", ""),
        new(Float, "0x1.ffffp+1023", "0x1.ffffp+1023", ""),

        new(Float, "0x.", "0x.", "hexadecimal literal has no digits"),
        new(Float, "0x0.", "0x0.", "hexadecimal mantissa requires a 'p' exponent"),
        new(Float, "0x.0", "0x.0", "hexadecimal mantissa requires a 'p' exponent"),
        new(Float, "0x1.1", "0x1.1", "hexadecimal mantissa requires a 'p' exponent"),
        new(Float, "0x1.1e0", "0x1.1e0", "hexadecimal mantissa requires a 'p' exponent"),
        new(Float, "0x1.2gp1a", "0x1.2 gp1a", "hexadecimal mantissa requires a 'p' exponent"),
        new(Float, "0x0p", "0x0p", "exponent has no digits"),
        new(Float, "0xeP-", "0xeP-", "exponent has no digits"),
        new(Float, "0x1234PAB", "0x1234P AB", "exponent has no digits"),
        new(Float, "0x1.2p1a", "0x1.2p1 a", ""),

        // separators
        new(Int, "0b_1000_0001", "0b_1000_0001", ""),
        new(Int, "0o_600", "0o_600", ""),
        new(Int, "0_466", "0_466", ""),
        new(Int, "1_000", "1_000", ""),
        new(Float, "1_000.000_1", "1_000.000_1", ""),
        new(Int, "0x_f00d", "0x_f00d", ""),
        new(Float, "0x_f00d.0p1_2", "0x_f00d.0p1_2", ""),

        new(Int, "0b__1000", "0b__1000", "'_' must separate successive digits"),
        new(Int, "0o60___0", "0o60___0", "'_' must separate successive digits"),
        new(Int, "0466_", "0466_", "'_' must separate successive digits"),
        new(Float, "1_.", "1_.", "'_' must separate successive digits"),
        new(Float, "0._1", "0._1", "'_' must separate successive digits"),
        new(Float, "2.7_e0", "2.7_e0", "'_' must separate successive digits"),
        new(Int, "0x___0", "0x___0", "'_' must separate successive digits"),
        new(Float, "0x1.0_p0", "0x1.0_p0", "'_' must separate successive digits")
    ]))) {
        var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(test.src)));
        @string err = default;
        s.Error = (ж<global::go.text.scanner_package.Scanner> sΔ1, @string msg) => {
            if (err == ""u8) {
                err = msg;
            }
        };

        foreach (var (i, want) in range(strings.Split(test.tokens, " "))) {
            err = ""u8;
            var tok = s.Scan();
            @string lit = s.TokenText();
            if (i == 0) {
                if (tok != test.tok) {
                    t.Errorf("%q: got token %s; want %s", test.src, TokenString(tok), TokenString(test.tok));
                }
                if (err != test.err) {
                    t.Errorf("%q: got error %q; want %q", test.src, err, test.err);
                }
            }
            if (lit != want) {
                t.Errorf("%q: got literal %q (%s); want %s", test.src, lit, TokenString(tok), want);
            }
        }

        // make sure we read all
        {
            var tok = s.Scan(); if (tok != EOF) {
                t.Errorf("%q: got %s; want EOF", test.src, TokenString(tok));
            }
        }
    }
}

internal partial struct TestIssue30320_type /*dyn*/ {
    internal @string @in, want;
    internal nuint mode;
}

public static void TestIssue30320(ж<testing.T> t) {
    foreach (var (_, test) in range(slice<TestIssue30320_type>([
        new("foo01.bar31.xx-0-1-1-0", "01 31 0 1 1 0", ScanInts),
        new("foo0/12/0/5.67", "0 12 0 5 67", ScanInts),
        new("xxx1e0yyy", "1 0", ScanInts),
        new("1_2", "1_2", ScanInts),
        new("xxx1.0yyy2e3ee", "1 0 2 3", ScanInts),
        new("xxx1.0yyy2e3ee", "1.0 2e3", ScanFloats)
    ]))) {
        @string got = extractInts(test.@in, test.mode);
        if (got != test.want) {
            t.Errorf("%q: got %q; want %q", test.@in, got, test.want);
        }
    }
}

internal static @string /*res*/ extractInts(@string t, nuint mode) {
    @string res = default;

    ref var s = ref heap(new global::go.text.scanner_package.Scanner(), out var Ꮡs);
    Ꮡs.Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(t)));
    s.Mode = mode;
    for (;;) {
        var tok = Ꮡs.Scan();
        switch (tok) {
        case var ᴛ when ᴛ == Int || ᴛ == Float: {
            if (len(res) > 0) {
                res += " "u8;
            }
            res += Ꮡs.TokenText();
            break;
        }
        case var ᴛ when ᴛ == EOF: {
            return res;
        }}
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "hello \n\nworld\n!\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string Rᶜ => ref Rᶜᵇ;
private static readonly @string Rᶜᵇ = "hello world!"u8;

public static void TestIssue50909(ж<testing.T> t) {
    ref var s = ref heap(new global::go.text.scanner_package.Scanner(), out var Ꮡs);
    Ꮡs.Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(helloWorldˢ)));
    s.IsIdentRune = (rune ch, nint _) => ch != '\n';

    @string r = ""u8;
    nint n = 0;
    while (Ꮡs.Scan() != EOF && n < 10) {
        r += Ꮡs.TokenText();
        n++;
    }

    @string R = Rᶜ;
    const nint N = 3;
    if (r != R || n != N) {
        t.Errorf("got %q (n = %d); want %q (n = %d)", r, n, R, (nint)(N));
    }
}

} // end scanner_internal_test_package
