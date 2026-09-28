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
[GoType] public partial struct StringReader {
    internal slice<@string> data;
    internal nint step;
}

[GoRecv] public static (nint n, error err) Read(this ref StringReader r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (r.step < len(r.data)){
        @string s = r.data[r.step];
        n = copy(p, s);
        r.step++;
    } else {
        err = io.EOF;
    }
    return (n, err);
}

internal static void readRuneSegments(ж<testing.T> Ꮡt, slice<@string> segments) {
    @string got = ""u8;
    @string want = strings.Join(segments, ""u8);
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_internal_test_package.StringReaderжReader(Ꮡ(new StringReader(data: segments))));
    while (ᐧ) {
        var ch = s.Next();
        if (ch == EOF) {
            break;
        }
        got += ((@string)ch);
    }
    if (got != want) {
        Ꮡt.Errorf("segments=%v got=%s want=%s"u8, segments, got, want);
    }
}

internal static slice<slice<@string>> segmentList = new slice<@string>[]{
    new @string[]{}.slice(),
    new @string[]{""u8}.slice(),
    new @string[]{"日"u8, "本語"u8}.slice(),
    new @string[]{"\u65e5"u8, "\u672c"u8, "\u8a9e"u8}.slice(),
    new @string[]{"\U000065e5"u8, " "u8, "\U0000672c"u8, "\U00008a9e"u8}.slice(),
    new @string[]{((@string)(new byte[]{0xe6})), ((@string)(new byte[]{0x97, 0xa5, 0xe6})), ((@string)(new byte[]{0x9c, 0xac, 0xe8, 0xaa, 0x9e}))}.slice(),
    new @string[]{"Hello"u8, ", "u8, "World"u8, "!"u8}.slice(),
    new @string[]{"Hello"u8, ", "u8, ""u8, "World"u8, "!"u8}.slice()
}.slice();

public static void TestNext(ж<testing.T> Ꮡt) {
    foreach (var (_, s) in segmentList) {
        readRuneSegments(Ꮡt, s);
    }
}

[GoType] internal partial struct token {
    internal rune tok;
    internal @string text;
}

internal static @string f100 = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8;

// NUL character is not allowed
internal static slice<token> tokenList = new token[]{
    new(Comment, "// line comments"u8),
    new(Comment, "//"u8),
    new(Comment, "////"u8),
    new(Comment, "// comment"u8),
    new(Comment, "// /* comment */"u8),
    new(Comment, "// // comment //"u8),
    new(Comment, "//"u8 + f100),
    new(Comment, "// general comments"u8),
    new(Comment, "/**/"u8),
    new(Comment, "/***/"u8),
    new(Comment, "/* comment */"u8),
    new(Comment, "/* // comment */"u8),
    new(Comment, "/* /* comment */"u8),
    new(Comment, "/*\n comment\n*/"u8),
    new(Comment, "/*"u8 + f100 + "*/"u8),
    new(Comment, "// identifiers"u8),
    new(Ident, "a"u8),
    new(Ident, "a0"u8),
    new(Ident, "foobar"u8),
    new(Ident, "abc123"u8),
    new(Ident, "LGTM"u8),
    new(Ident, "_"u8),
    new(Ident, "_abc123"u8),
    new(Ident, "abc123_"u8),
    new(Ident, "_abc_123_"u8),
    new(Ident, "_äöü"u8),
    new(Ident, "_本"u8),
    new(Ident, "äöü"u8),
    new(Ident, "本"u8),
    new(Ident, "a۰۱۸"u8),
    new(Ident, "foo६४"u8),
    new(Ident, "bar９８７６"u8),
    new(Ident, f100),
    new(Comment, "// decimal ints"u8),
    new(Int, "0"u8),
    new(Int, "1"u8),
    new(Int, "9"u8),
    new(Int, "42"u8),
    new(Int, "1234567890"u8),
    new(Comment, "// octal ints"u8),
    new(Int, "00"u8),
    new(Int, "01"u8),
    new(Int, "07"u8),
    new(Int, "042"u8),
    new(Int, "01234567"u8),
    new(Comment, "// hexadecimal ints"u8),
    new(Int, "0x0"u8),
    new(Int, "0x1"u8),
    new(Int, "0xf"u8),
    new(Int, "0x42"u8),
    new(Int, "0x123456789abcDEF"u8),
    new(Int, "0x"u8 + f100),
    new(Int, "0X0"u8),
    new(Int, "0X1"u8),
    new(Int, "0XF"u8),
    new(Int, "0X42"u8),
    new(Int, "0X123456789abcDEF"u8),
    new(Int, "0X"u8 + f100),
    new(Comment, "// floats"u8),
    new(Float, "0."u8),
    new(Float, "1."u8),
    new(Float, "42."u8),
    new(Float, "01234567890."u8),
    new(Float, ".0"u8),
    new(Float, ".1"u8),
    new(Float, ".42"u8),
    new(Float, ".0123456789"u8),
    new(Float, "0.0"u8),
    new(Float, "1.0"u8),
    new(Float, "42.0"u8),
    new(Float, "01234567890.0"u8),
    new(Float, "0e0"u8),
    new(Float, "1e0"u8),
    new(Float, "42e0"u8),
    new(Float, "01234567890e0"u8),
    new(Float, "0E0"u8),
    new(Float, "1E0"u8),
    new(Float, "42E0"u8),
    new(Float, "01234567890E0"u8),
    new(Float, "0e+10"u8),
    new(Float, "1e-10"u8),
    new(Float, "42e+10"u8),
    new(Float, "01234567890e-10"u8),
    new(Float, "0E+10"u8),
    new(Float, "1E-10"u8),
    new(Float, "42E+10"u8),
    new(Float, "01234567890E-10"u8),
    new(Comment, "// chars"u8),
    new(Char, @"' '"u8),
    new(Char, @"'a'"u8),
    new(Char, @"'本'"u8),
    new(Char, @"'\a'"u8),
    new(Char, @"'\b'"u8),
    new(Char, @"'\f'"u8),
    new(Char, @"'\n'"u8),
    new(Char, @"'\r'"u8),
    new(Char, @"'\t'"u8),
    new(Char, @"'\v'"u8),
    new(Char, @"'\''"u8),
    new(Char, @"'\000'"u8),
    new(Char, @"'\777'"u8),
    new(Char, @"'\x00'"u8),
    new(Char, @"'\xff'"u8),
    new(Char, @"'\u0000'"u8),
    new(Char, @"'\ufA16'"u8),
    new(Char, @"'\U00000000'"u8),
    new(Char, @"'\U0000ffAB'"u8),
    new(Comment, "// strings"u8),
    new(ΔString, @""" """u8),
    new(ΔString, @"""a"""u8),
    new(ΔString, @"""本"""u8),
    new(ΔString, @"""\a"""u8),
    new(ΔString, @"""\b"""u8),
    new(ΔString, @"""\f"""u8),
    new(ΔString, @"""\n"""u8),
    new(ΔString, @"""\r"""u8),
    new(ΔString, @"""\t"""u8),
    new(ΔString, @"""\v"""u8),
    new(ΔString, @"""\"""""u8),
    new(ΔString, @"""\000"""u8),
    new(ΔString, @"""\777"""u8),
    new(ΔString, @"""\x00"""u8),
    new(ΔString, @"""\xff"""u8),
    new(ΔString, @"""\u0000"""u8),
    new(ΔString, @"""\ufA16"""u8),
    new(ΔString, @"""\U00000000"""u8),
    new(ΔString, @"""\U0000ffAB"""u8),
    new(ΔString, @""""u8 + f100 + @""""u8),
    new(Comment, "// raw strings"u8),
    new(RawString, "``"u8),
    new(RawString, "`\\`"u8),
    new(RawString, "`"u8 + "\n\n/* foobar */\n\n"u8 + "`"u8),
    new(RawString, "`"u8 + f100 + "`"u8),
    new(Comment, "// individual characters"u8),
    new((rune)'\x01', "\x01"u8),
    new((rune)' ' - 1, ((@string)((rune)(rune)' ' - 1))),
    new((rune)'+', "+"u8),
    new((rune)'/', "/"u8),
    new((rune)'.', "."u8),
    new((rune)'~', "~"u8),
    new((rune)'(', "("u8)
}.slice();

internal static ж<bytes.Buffer> makeSource(@string pattern) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    foreach (var (_, k) in tokenList) {
        fmt.Fprintf(new scanner_test_package.bytes_BufferжWriter(Ꮡbuf), pattern, k.text);
    }
    return Ꮡbuf;
}

internal static void checkTok(ж<testing.T> Ꮡt, ж<global::go.text.scanner_package.Scanner> Ꮡs, nint line, rune got, rune want, @string text) {
    ref var s = ref Ꮡs.DerefOrNull();

    if (got != want) {
        Ꮡt.Fatalf("tok = %s, want %s for %q"u8, TokenString(got), TokenString(want), text);
    }
    if (s.Line != line) {
        Ꮡt.Errorf("line = %d, want %d for %q"u8, s.Line, line, text);
    }
    @string stext = Ꮡs.TokenText();
    if (stext != text){
        Ꮡt.Errorf("text = %q, want %q"u8, stext, text);
    } else {
        // check idempotency of TokenText() call
        stext = Ꮡs.TokenText();
        if (stext != text) {
            Ꮡt.Errorf("text = %q, want %q (idempotency check)"u8, stext, text);
        }
    }
}

internal static void checkTokErr(ж<testing.T> Ꮡt, ж<global::go.text.scanner_package.Scanner> Ꮡs, nint line, rune want, @string text) {
    ref var s = ref Ꮡs.DerefOrNull();

    nint prevCount = s.ErrorCount;
    checkTok(Ꮡt, Ꮡs, line, Ꮡs.Scan(), want, text);
    if (s.ErrorCount != prevCount + 1) {
        Ꮡt.Fatalf("want error for %q"u8, text);
    }
}

internal static nint countNewlines(@string s) {
    nint n = 0;
    foreach (var (_, ch) in s) {
        if (ch == (rune)'\n') {
            n++;
        }
    }
    return n;
}

internal static void testScan(ж<testing.T> Ꮡt, nuint mode) {
    ref var t = ref Ꮡt.DerefOrNull();

    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.bytes_BufferжReader(makeSource(" \t%s\n"u8)));
    s.Value.Mode = mode;
    var tok = s.Scan();
    nint line = 1;
    foreach (var (_, k) in tokenList) {
        if ((nuint)(mode & (nuint)SkipComments) == 0 || k.tok != Comment) {
            checkTok(Ꮡt, s, line, tok, k.tok, k.text);
            tok = s.Scan();
        }
        line += countNewlines(k.text) + 1; // each token is on a new line
    }
    checkTok(Ꮡt, s, line, tok, EOF, ""u8);
}

public static void TestScan(ж<testing.T> Ꮡt) {
    testScan(Ꮡt, GoTokens);
    testScan(Ꮡt, (nuint)((nuint)GoTokens & ~(nuint)(nuint)SkipComments));
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ = "1.5e 1.5E 1e+ 1e- 1.5z"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ = "exponent has no digits"u8;

public static void TestInvalidExponent(ж<testing.T> Ꮡt) {
    @string src = srcᶜ;
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(src)));
    s.Value.Error = (ж<global::go.text.scanner_package.Scanner> sΔ1, @string msg) => {
        @string want = wantᶜ;
        if (msg != want) {
            Ꮡt.Errorf("%s: got error %q; want %q"u8, sΔ1.TokenText(), msg, want);
        }
    };
    checkTokErr(Ꮡt, s, 1, Float, "1.5e"u8);
    checkTokErr(Ꮡt, s, 1, Float, "1.5E"u8);
    checkTokErr(Ꮡt, s, 1, Float, "1e+"u8);
    checkTokErr(Ꮡt, s, 1, Float, "1e-"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), Float, "1.5"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, "z"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), EOF, ""u8);
    if ((~s).ErrorCount != 4) {
        Ꮡt.Errorf("%d errors, want 4"u8, (~s).ErrorCount);
    }
}

public static void TestPosition(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var src = makeSource("\t\t\t\t%s\n"u8);
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.bytes_BufferжReader(src));
    s.Value.Mode = (nuint)((nuint)GoTokens & ~(nuint)(nuint)SkipComments);
    s.Scan();
    var pos = new Position(""u8, 4, 1, 5);
    foreach (var (_, k) in tokenList) {
        if ((~s).Offset != pos.Offset) {
            Ꮡt.Errorf("offset = %d, want %d for %q"u8, (~s).Offset, pos.Offset, k.text);
        }
        if ((~s).Line != pos.Line) {
            Ꮡt.Errorf("line = %d, want %d for %q"u8, (~s).Line, pos.Line, k.text);
        }
        if ((~s).Column != pos.Column) {
            Ꮡt.Errorf("column = %d, want %d for %q"u8, (~s).Column, pos.Column, k.text);
        }
        pos.Offset += 4 + len(k.text) + 1; // 4 tabs + token bytes + newline
        pos.Line += countNewlines(k.text) + 1; // each token is on a new line
        s.Scan();
    }
    // make sure there were no token-internal errors reported by scanner
    if ((~s).ErrorCount != 0) {
        Ꮡt.Errorf("%d errors"u8, (~s).ErrorCount);
    }
}

public static void TestScanZeroMode(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var src = makeSource("%s\n"u8);
    @string str = src.String();
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.bytes_BufferжReader(src));
    s.Value.Mode = 0; // don't recognize any token classes
    s.Value.Whitespace = 0; // don't skip any whitespace
    var tok = s.Scan();
    foreach (var (i, ch) in str) {
        if (tok != ch) {
            Ꮡt.Fatalf("%d. tok = %s, want %s"u8, i, TokenString(tok), TokenString(ch));
        }
        tok = s.Scan();
    }
    if (tok != EOF) {
        Ꮡt.Fatalf("tok = %s, want EOF"u8, TokenString(tok));
    }
    if ((~s).ErrorCount != 0) {
        Ꮡt.Errorf("%d errors"u8, (~s).ErrorCount);
    }
}

internal static void testScanSelectedMode(ж<testing.T> Ꮡt, nuint mode, rune @class) {
    var src = makeSource("%s\n"u8);
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.bytes_BufferжReader(src));
    s.Value.Mode = mode;
    var tok = s.Scan();
    while (tok != EOF) {
        if (tok < 0 && tok != @class) {
            Ꮡt.Fatalf("tok = %s, want %s"u8, TokenString(tok), TokenString(@class));
        }
        tok = s.Scan();
    }
    if ((~s).ErrorCount != 0) {
        Ꮡt.Errorf("%d errors"u8, (~s).ErrorCount);
    }
}

public static void TestScanSelectedMask(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testScanSelectedMode(Ꮡt, 0, 0);
    testScanSelectedMode(Ꮡt, ScanIdents, Ident);
    // Don't test ScanInts and ScanNumbers since some parts of
    // the floats in the source look like (invalid) octal ints
    // and ScanNumbers may return either Int or Float.
    testScanSelectedMode(Ꮡt, ScanChars, Char);
    testScanSelectedMode(Ꮡt, ScanStrings, ΔString);
    testScanSelectedMode(Ꮡt, SkipComments, 0);
    testScanSelectedMode(Ꮡt, ScanComments, Comment);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string b123ˢ = "b123"u8;
internal static readonly @string a12ˢ = "a12"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string srcᶜ1 = "faab12345 a12b123 a12 3b"u8;

public static void TestScanCustomIdent(ж<testing.T> Ꮡt) {
    @string src = srcᶜ1;
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(src)));
    // ident = ( 'a' | 'b' ) { digit } .
    // digit = '0' .. '3' .
    // with a maximum length of 4
    s.Value.IsIdentRune = (rune ch, nint i) => i == 0 && (ch == (rune)'a' || ch == (rune)'b') || 0 < i && i < 4 && (rune)'0' <= ch && ch <= (rune)'3';
    checkTok(Ꮡt, s, 1, s.Scan(), (rune)'f', "f"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, "a"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, "a"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, b123ˢ);
    checkTok(Ꮡt, s, 1, s.Scan(), Int, "45"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, a12ˢ);
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, b123ˢ);
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, a12ˢ);
    checkTok(Ꮡt, s, 1, s.Scan(), Int, "3"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, "b"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), EOF, ""u8);
}

public static void TestScanNext(ж<testing.T> Ꮡt) {
    UntypedInt BOM = /* '\uFEFF' */ 65279;
    @string BOMs = ((@string)(rune)BOM);
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(BOMs + "if a == bcd /* com"u8 + BOMs + "ment */ {\n\ta += c\n}"u8 + BOMs + "// line comment ending in eof"u8)));
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, "if"u8); // the first BOM is ignored
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, "a"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), (rune)'=', "="u8);
    checkTok(Ꮡt, s, 0, s.Next(), (rune)'=', ""u8);
    checkTok(Ꮡt, s, 0, s.Next(), (rune)' ', ""u8);
    checkTok(Ꮡt, s, 0, s.Next(), (rune)'b', ""u8);
    checkTok(Ꮡt, s, 1, s.Scan(), Ident, "cd"u8);
    checkTok(Ꮡt, s, 1, s.Scan(), (rune)'{', "{"u8);
    checkTok(Ꮡt, s, 2, s.Scan(), Ident, "a"u8);
    checkTok(Ꮡt, s, 2, s.Scan(), (rune)'+', "+"u8);
    checkTok(Ꮡt, s, 0, s.Next(), (rune)'=', ""u8);
    checkTok(Ꮡt, s, 2, s.Scan(), Ident, "c"u8);
    checkTok(Ꮡt, s, 3, s.Scan(), (rune)'}', "}"u8);
    checkTok(Ꮡt, s, 3, s.Scan(), BOM, BOMs);
    checkTok(Ꮡt, s, 3, s.Scan(), -1, ""u8);
    if ((~s).ErrorCount != 0) {
        Ꮡt.Errorf("%d errors"u8, (~s).ErrorCount);
    }
}

public static void TestScanWhitespace(ж<testing.T> Ꮡt) {
    ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
    uint64 ws = default!;
    // start at 1, NUL character is not allowed
    for (var ch = (byte)1; ch < (rune)' '; ch++) {
        buf.WriteByte(ch);
        ws |= (uint64)(((uint64)1).Lsh((uint64)(ch)));
    }
    UntypedInt orig = /* 'x' */ 120;
    buf.WriteByte(orig);
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.bytes_BufferжReader(Ꮡbuf));
    s.Value.Mode = 0;
    s.Value.Whitespace = ws;
    var tok = s.Scan();
    if (tok != orig) {
        Ꮡt.Errorf("tok = %s, want %s"u8, TokenString(tok), TokenString(orig));
    }
}

internal static void testError(ж<testing.T> Ꮡt, @string src, @string pos, @string msg, rune tok) {
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(src)));
    var errorCalled = false;
    s.Value.Error = (ж<global::go.text.scanner_package.Scanner> sΔ1, @string m) => {
        if (!errorCalled) {
            // only look at first error
            {
                @string p = sΔ1.Pos().String(); if (p != pos) {
                    Ꮡt.Errorf("pos = %q, want %q for %q"u8, p, pos, src);
                }
            }
            if (m != msg) {
                Ꮡt.Errorf("msg = %q, want %q for %q"u8, m, msg, src);
            }
            errorCalled = true;
        }
    };
    var tk = s.Scan();
    if (tk != tok) {
        Ꮡt.Errorf("tok = %s, want %s for %q"u8, TokenString(tk), TokenString(tok), src);
    }
    if (!errorCalled) {
        Ꮡt.Errorf("error handler not called for %q"u8, src);
    }
    if ((~s).ErrorCount == 0) {
        Ꮡt.Errorf("count = %d, want > 0 for %q"u8, (~s).ErrorCount, src);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string input11ˢ = "<input>:1:1"u8;
internal static readonly @string input12ˢ = "<input>:1:2"u8;
internal static readonly @string input13ˢ = "<input>:1:3"u8;
internal static readonly @string input14ˢ = "<input>:1:4"u8;
internal static readonly @string input15ˢ = "<input>:1:5"u8;
internal static readonly @string input16ˢ = "<input>:1:6"u8;
internal static readonly @string invalidDigit8InOctalˢ = "invalid digit '8' in octal literal"u8;
internal static readonly @string input19ˢ = "<input>:1:9"u8;
internal static readonly @string hexadecimalLiteralHasNoˢ = "hexadecimal literal has no digits"u8;
internal static readonly @string abcˢ = @"""abc"u8;
internal static readonly @string abcˢ2 = "`abc\n"u8;
internal static readonly @string input21ˢ = "<input>:2:1"u8;

public static void TestError(ж<testing.T> Ꮡt) {
    testError(Ꮡt, "\x00"u8, input11ˢ, invalidCharacterNulˢ, 0);
    testError(Ꮡt, ((@string)(new byte[]{0x80})), input11ˢ, invalidUtf8Encodingˢ, utf8.RuneError);
    testError(Ꮡt, ((@string)(new byte[]{0xff})), input11ˢ, invalidUtf8Encodingˢ, utf8.RuneError);
    testError(Ꮡt, "a\x00"u8, input12ˢ, invalidCharacterNulˢ, Ident);
    testError(Ꮡt, ((@string)(new byte[]{0x61, 0x62, 0x80})), input13ˢ, invalidUtf8Encodingˢ, Ident);
    testError(Ꮡt, ((@string)(new byte[]{0x61, 0x62, 0x63, 0xff})), input14ˢ, invalidUtf8Encodingˢ, Ident);
    testError(Ꮡt, @"""a"u8 + "\x00"u8, input13ˢ, invalidCharacterNulˢ, ΔString);
    testError(Ꮡt, @"""ab"u8 + ((@string)(new byte[]{0x80})), input14ˢ, invalidUtf8Encodingˢ, ΔString);
    testError(Ꮡt, @"""abc"u8 + ((@string)(new byte[]{0xff})), input15ˢ, invalidUtf8Encodingˢ, ΔString);
    testError(Ꮡt, "`a"u8 + "\x00"u8, input13ˢ, invalidCharacterNulˢ, RawString);
    testError(Ꮡt, "`ab"u8 + ((@string)(new byte[]{0x80})), input14ˢ, invalidUtf8Encodingˢ, RawString);
    testError(Ꮡt, "`abc"u8 + ((@string)(new byte[]{0xff})), input15ˢ, invalidUtf8Encodingˢ, RawString);
    testError(Ꮡt, @"'\""'"u8, input13ˢ, invalidCharEscapeˢ, Char);
    testError(Ꮡt, @"""\'"""u8, input13ˢ, invalidCharEscapeˢ, ΔString);
    testError(Ꮡt, @"01238"u8, input16ˢ, invalidDigit8InOctalˢ, Int);
    testError(Ꮡt, @"01238123"u8, input19ˢ, invalidDigit8InOctalˢ, Int);
    testError(Ꮡt, @"0x"u8, input13ˢ, hexadecimalLiteralHasNoˢ, Int);
    testError(Ꮡt, @"0xg"u8, input13ˢ, hexadecimalLiteralHasNoˢ, Int);
    testError(Ꮡt, @"'aa'"u8, input14ˢ, invalidCharLiteralˢ, Char);
    testError(Ꮡt, @"1.5e"u8, input15ˢ, exponentHasNoDigitsˢ, Float);
    testError(Ꮡt, @"1.5E"u8, input15ˢ, exponentHasNoDigitsˢ, Float);
    testError(Ꮡt, @"1.5e+"u8, input16ˢ, exponentHasNoDigitsˢ, Float);
    testError(Ꮡt, @"1.5e-"u8, input16ˢ, exponentHasNoDigitsˢ, Float);
    testError(Ꮡt, @"'"u8, input12ˢ, literalNotTerminatedˢ, Char);
    testError(Ꮡt, @"'"u8 + "\n"u8, input12ˢ, literalNotTerminatedˢ, Char);
    testError(Ꮡt, abcˢ, input15ˢ, literalNotTerminatedˢ, ΔString);
    testError(Ꮡt, @"""abc"u8 + "\n"u8, input15ˢ, literalNotTerminatedˢ, ΔString);
    testError(Ꮡt, abcˢ2, input21ˢ, literalNotTerminatedˢ, RawString);
    testError(Ꮡt, @"/*/"u8, input14ˢ, commentNotTerminatedˢ, EOF);
}

// An errReader returns (0, err) where err is not io.EOF.
[GoType] internal partial struct errReader {
}

internal static (nint, error) Read(this errReader _, slice<byte> b) {
    return (0, io.ErrNoProgress); // some error that is not io.EOF
}

public static void TestIOError(ж<testing.T> Ꮡt) {
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new errReader(nil));
    var errorCalled = false;
    s.Value.Error = (ж<global::go.text.scanner_package.Scanner> sΔ1, @string msg) => {
        if (!errorCalled) {
            {
                @string want = io.ErrNoProgress.Error(); if (msg != want) {
                    Ꮡt.Errorf("msg = %q, want %q"u8, msg, want);
                }
            }
            errorCalled = true;
        }
    };
    var tok = s.Scan();
    if (tok != EOF) {
        Ꮡt.Errorf("tok = %s, want EOF"u8, TokenString(tok));
    }
    if (!errorCalled) {
        Ꮡt.Errorf("error handler not called"u8);
    }
}

internal static void checkPos(ж<testing.T> Ꮡt, global::go.text.scanner_package.Position got, global::go.text.scanner_package.Position want) {
    if (got.Offset != want.Offset || got.Line != want.Line || got.Column != want.Column) {
        Ꮡt.Errorf("got offset, line, column = %d, %d, %d; want %d, %d, %d"u8,
            got.Offset, got.Line, got.Column, want.Offset, want.Line, want.Column);
    }
}

internal static void checkNextPos(ж<testing.T> Ꮡt, ж<global::go.text.scanner_package.Scanner> Ꮡs, nint offset, nint line, nint column, rune @char) {
    ref var s = ref Ꮡs.DerefOrNull();

    {
        var ch = Ꮡs.Next(); if (ch != @char) {
            Ꮡt.Errorf("ch = %s, want %s"u8, TokenString(ch), TokenString(@char));
        }
    }
    var want = new Position(Offset: offset, Line: line, Column: column);
    checkPos(Ꮡt, s.Pos(), want);
}

internal static void checkScanPos(ж<testing.T> Ꮡt, ж<global::go.text.scanner_package.Scanner> Ꮡs, nint offset, nint line, nint column, rune @char) {
    ref var s = ref Ꮡs.DerefOrNull();

    var want = new Position(Offset: offset, Line: line, Column: column);
    checkPos(Ꮡt, s.Pos(), want);
    {
        var ch = Ꮡs.Scan(); if (ch != @char) {
            Ꮡt.Errorf("ch = %s, want %s"u8, TokenString(ch), TokenString(@char));
            if (((@string)ch) != Ꮡs.TokenText()) {
                Ꮡt.Errorf("tok = %q, want %q"u8, Ꮡs.TokenText(), ((@string)ch));
            }
        }
    }
    checkPos(Ꮡt, s.Position, want);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string fooˢ = "  foo६४  \n\n本語\n"u8;
internal static readonly @string abcXˢ = "abc\n本語\n\nx"u8;

public static void TestPos(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // corner case: empty source
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(""u8)));
    checkPos(Ꮡt, s.Pos(), new Position(Offset: 0, Line: 1, Column: 1));
    s.Peek(); // peek doesn't affect the position
    checkPos(Ꮡt, s.Pos(), new Position(Offset: 0, Line: 1, Column: 1));
    // corner case: source with only a newline
    s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader("\n"u8)));
    checkPos(Ꮡt, s.Pos(), new Position(Offset: 0, Line: 1, Column: 1));
    checkNextPos(Ꮡt, s, 1, 2, 1, (rune)'\n');
    // after EOF position doesn't change
    for (nint i = 10; i > 0; i--) {
        checkScanPos(Ꮡt, s, 1, 2, 1, EOF);
    }
    if ((~s).ErrorCount != 0) {
        Ꮡt.Errorf("%d errors"u8, (~s).ErrorCount);
    }
    // corner case: source with only a single character
    s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader("本"u8)));
    checkPos(Ꮡt, s.Pos(), new Position(Offset: 0, Line: 1, Column: 1));
    checkNextPos(Ꮡt, s, 3, 1, 2, (rune)'本');
    // after EOF position doesn't change
    for (nint i = 10; i > 0; i--) {
        checkScanPos(Ꮡt, s, 3, 1, 2, EOF);
    }
    if ((~s).ErrorCount != 0) {
        Ꮡt.Errorf("%d errors"u8, (~s).ErrorCount);
    }
    // positions after calling Next
    s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(fooˢ)));
    checkNextPos(Ꮡt, s, 1, 1, 2, (rune)' ');
    s.Peek(); // peek doesn't affect the position
    checkNextPos(Ꮡt, s, 2, 1, 3, (rune)' ');
    checkNextPos(Ꮡt, s, 3, 1, 4, (rune)'f');
    checkNextPos(Ꮡt, s, 4, 1, 5, (rune)'o');
    checkNextPos(Ꮡt, s, 5, 1, 6, (rune)'o');
    checkNextPos(Ꮡt, s, 8, 1, 7, (rune)'६');
    checkNextPos(Ꮡt, s, 11, 1, 8, (rune)'४');
    checkNextPos(Ꮡt, s, 12, 1, 9, (rune)' ');
    checkNextPos(Ꮡt, s, 13, 1, 10, (rune)' ');
    checkNextPos(Ꮡt, s, 14, 2, 1, (rune)'\n');
    checkNextPos(Ꮡt, s, 15, 3, 1, (rune)'\n');
    checkNextPos(Ꮡt, s, 18, 3, 2, (rune)'本');
    checkNextPos(Ꮡt, s, 21, 3, 3, (rune)'語');
    checkNextPos(Ꮡt, s, 22, 4, 1, (rune)'\n');
    // after EOF position doesn't change
    for (nint i = 10; i > 0; i--) {
        checkScanPos(Ꮡt, s, 22, 4, 1, EOF);
    }
    if ((~s).ErrorCount != 0) {
        Ꮡt.Errorf("%d errors"u8, (~s).ErrorCount);
    }
    // positions after calling Scan
    s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(abcXˢ)));
    s.Value.Mode = 0;
    s.Value.Whitespace = 0;
    checkScanPos(Ꮡt, s, 0, 1, 1, (rune)'a');
    s.Peek(); // peek doesn't affect the position
    checkScanPos(Ꮡt, s, 1, 1, 2, (rune)'b');
    checkScanPos(Ꮡt, s, 2, 1, 3, (rune)'c');
    checkScanPos(Ꮡt, s, 3, 1, 4, (rune)'\n');
    checkScanPos(Ꮡt, s, 4, 2, 1, (rune)'本');
    checkScanPos(Ꮡt, s, 7, 2, 2, (rune)'語');
    checkScanPos(Ꮡt, s, 10, 2, 3, (rune)'\n');
    checkScanPos(Ꮡt, s, 11, 3, 1, (rune)'\n');
    checkScanPos(Ꮡt, s, 12, 4, 1, (rune)'x');
    // after EOF position doesn't change
    for (nint i = 10; i > 0; i--) {
        checkScanPos(Ꮡt, s, 13, 4, 2, EOF);
    }
    if ((~s).ErrorCount != 0) {
        Ꮡt.Errorf("%d errors"u8, (~s).ErrorCount);
    }
}

[GoType("num:nint")] internal partial struct countReader;

[GoRecv] internal static (nint, error) Read(this ref countReader r, slice<byte> _) {
    r++;
    return (0, io.EOF);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object eofNotReportedˢ = (@string)"1) EOF not reported"u8;
internal static readonly object eofNotReportedˢ2 = (@string)"2) EOF not reported"u8;

public static void TestNextEOFHandling(ж<testing.T> Ꮡt) {
    ref var r = ref heap(new countReader(), out var Ꮡr);
    // corner case: empty source
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_internal_test_package.countReaderжReader(Ꮡr));
    var tok = s.Next();
    if (tok != EOF) {
        Ꮡt.Error(eofNotReportedˢ);
    }
    tok = s.Peek();
    if (tok != EOF) {
        Ꮡt.Error(eofNotReportedˢ2);
    }
    if (r != 1) {
        Ꮡt.Errorf("scanner called Read %d times, not once"u8, r);
    }
}

public static void TestScanEOFHandling(ж<testing.T> Ꮡt) {
    ref var r = ref heap(new countReader(), out var Ꮡr);
    // corner case: empty source
    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_internal_test_package.countReaderжReader(Ꮡr));
    var tok = s.Scan();
    if (tok != EOF) {
        Ꮡt.Error(eofNotReportedˢ);
    }
    tok = s.Peek();
    if (tok != EOF) {
        Ꮡt.Error(eofNotReportedˢ2);
    }
    if (r != 1) {
        Ꮡt.Errorf("scanner called Read %d times, not once"u8, r);
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string wantᶜ1 = @""""u8;

public static void TestIssue29723(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(@"x """u8)));
    s.Value.Error = (ж<global::go.text.scanner_package.Scanner> sΔ1, @string _) => {
        @string got = sΔ1.TokenText(); // this call shouldn't panic
        @string want = wantᶜ1;
        if (got != want) {
            Ꮡt.Errorf("got %q; want %q"u8, got, want);
        }
    };
    for (var r = s.Scan(); r != EOF; r = s.Scan()) {
    }
}

[GoType("dyn")] internal partial struct TestNumbers_type {
    internal rune tok;
    internal @string src, tokens, err;
}

public static void TestNumbers(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestNumbers_type[]{ // binaries

        new(Int, "0b0"u8, "0b0"u8, ""u8),
        new(Int, "0b1010"u8, "0b1010"u8, ""u8),
        new(Int, "0B1110"u8, "0B1110"u8, ""u8),
        new(Int, "0b"u8, "0b"u8, "binary literal has no digits"u8),
        new(Int, "0b0190"u8, "0b0190"u8, "invalid digit '9' in binary literal"u8),
        new(Int, "0b01a0"u8, "0b01 a0"u8, ""u8), // only accept 0-9
 // binary floats (invalid)

        new(Float, "0b."u8, "0b."u8, "invalid radix point in binary literal"u8),
        new(Float, "0b.1"u8, "0b.1"u8, "invalid radix point in binary literal"u8),
        new(Float, "0b1.0"u8, "0b1.0"u8, "invalid radix point in binary literal"u8),
        new(Float, "0b1e10"u8, "0b1e10"u8, "'e' exponent requires decimal mantissa"u8),
        new(Float, "0b1P-1"u8, "0b1P-1"u8, "'P' exponent requires hexadecimal mantissa"u8), // octals

        new(Int, "0o0"u8, "0o0"u8, ""u8),
        new(Int, "0o1234"u8, "0o1234"u8, ""u8),
        new(Int, "0O1234"u8, "0O1234"u8, ""u8),
        new(Int, "0o"u8, "0o"u8, "octal literal has no digits"u8),
        new(Int, "0o8123"u8, "0o8123"u8, "invalid digit '8' in octal literal"u8),
        new(Int, "0o1293"u8, "0o1293"u8, "invalid digit '9' in octal literal"u8),
        new(Int, "0o12a3"u8, "0o12 a3"u8, ""u8), // only accept 0-9
 // octal floats (invalid)

        new(Float, "0o."u8, "0o."u8, "invalid radix point in octal literal"u8),
        new(Float, "0o.2"u8, "0o.2"u8, "invalid radix point in octal literal"u8),
        new(Float, "0o1.2"u8, "0o1.2"u8, "invalid radix point in octal literal"u8),
        new(Float, "0o1E+2"u8, "0o1E+2"u8, "'E' exponent requires decimal mantissa"u8),
        new(Float, "0o1p10"u8, "0o1p10"u8, "'p' exponent requires hexadecimal mantissa"u8), // 0-octals

        new(Int, "0"u8, "0"u8, ""u8),
        new(Int, "0123"u8, "0123"u8, ""u8),
        new(Int, "08123"u8, "08123"u8, "invalid digit '8' in octal literal"u8),
        new(Int, "01293"u8, "01293"u8, "invalid digit '9' in octal literal"u8),
        new(Int, "0F."u8, "0 F ."u8, ""u8), // only accept 0-9

        new(Int, "0123F."u8, "0123 F ."u8, ""u8),
        new(Int, "0123456x"u8, "0123456 x"u8, ""u8), // decimals

        new(Int, "1"u8, "1"u8, ""u8),
        new(Int, "1234"u8, "1234"u8, ""u8),
        new(Int, "1f"u8, "1 f"u8, ""u8), // only accept 0-9
 // decimal floats

        new(Float, "0."u8, "0."u8, ""u8),
        new(Float, "123."u8, "123."u8, ""u8),
        new(Float, "0123."u8, "0123."u8, ""u8),
        new(Float, ".0"u8, ".0"u8, ""u8),
        new(Float, ".123"u8, ".123"u8, ""u8),
        new(Float, ".0123"u8, ".0123"u8, ""u8),
        new(Float, "0.0"u8, "0.0"u8, ""u8),
        new(Float, "123.123"u8, "123.123"u8, ""u8),
        new(Float, "0123.0123"u8, "0123.0123"u8, ""u8),
        new(Float, "0e0"u8, "0e0"u8, ""u8),
        new(Float, "123e+0"u8, "123e+0"u8, ""u8),
        new(Float, "0123E-1"u8, "0123E-1"u8, ""u8),
        new(Float, "0.e+1"u8, "0.e+1"u8, ""u8),
        new(Float, "123.E-10"u8, "123.E-10"u8, ""u8),
        new(Float, "0123.e123"u8, "0123.e123"u8, ""u8),
        new(Float, ".0e-1"u8, ".0e-1"u8, ""u8),
        new(Float, ".123E+10"u8, ".123E+10"u8, ""u8),
        new(Float, ".0123E123"u8, ".0123E123"u8, ""u8),
        new(Float, "0.0e1"u8, "0.0e1"u8, ""u8),
        new(Float, "123.123E-10"u8, "123.123E-10"u8, ""u8),
        new(Float, "0123.0123e+456"u8, "0123.0123e+456"u8, ""u8),
        new(Float, "0e"u8, "0e"u8, "exponent has no digits"u8),
        new(Float, "0E+"u8, "0E+"u8, "exponent has no digits"u8),
        new(Float, "1e+f"u8, "1e+ f"u8, "exponent has no digits"u8),
        new(Float, "0p0"u8, "0p0"u8, "'p' exponent requires hexadecimal mantissa"u8),
        new(Float, "1.0P-1"u8, "1.0P-1"u8, "'P' exponent requires hexadecimal mantissa"u8), // hexadecimals

        new(Int, "0x0"u8, "0x0"u8, ""u8),
        new(Int, "0x1234"u8, "0x1234"u8, ""u8),
        new(Int, "0xcafef00d"u8, "0xcafef00d"u8, ""u8),
        new(Int, "0XCAFEF00D"u8, "0XCAFEF00D"u8, ""u8),
        new(Int, "0x"u8, "0x"u8, "hexadecimal literal has no digits"u8),
        new(Int, "0x1g"u8, "0x1 g"u8, ""u8), // hexadecimal floats

        new(Float, "0x0p0"u8, "0x0p0"u8, ""u8),
        new(Float, "0x12efp-123"u8, "0x12efp-123"u8, ""u8),
        new(Float, "0xABCD.p+0"u8, "0xABCD.p+0"u8, ""u8),
        new(Float, "0x.0189P-0"u8, "0x.0189P-0"u8, ""u8),
        new(Float, "0x1.ffffp+1023"u8, "0x1.ffffp+1023"u8, ""u8),
        new(Float, "0x."u8, "0x."u8, "hexadecimal literal has no digits"u8),
        new(Float, "0x0."u8, "0x0."u8, "hexadecimal mantissa requires a 'p' exponent"u8),
        new(Float, "0x.0"u8, "0x.0"u8, "hexadecimal mantissa requires a 'p' exponent"u8),
        new(Float, "0x1.1"u8, "0x1.1"u8, "hexadecimal mantissa requires a 'p' exponent"u8),
        new(Float, "0x1.1e0"u8, "0x1.1e0"u8, "hexadecimal mantissa requires a 'p' exponent"u8),
        new(Float, "0x1.2gp1a"u8, "0x1.2 gp1a"u8, "hexadecimal mantissa requires a 'p' exponent"u8),
        new(Float, "0x0p"u8, "0x0p"u8, "exponent has no digits"u8),
        new(Float, "0xeP-"u8, "0xeP-"u8, "exponent has no digits"u8),
        new(Float, "0x1234PAB"u8, "0x1234P AB"u8, "exponent has no digits"u8),
        new(Float, "0x1.2p1a"u8, "0x1.2p1 a"u8, ""u8), // separators

        new(Int, "0b_1000_0001"u8, "0b_1000_0001"u8, ""u8),
        new(Int, "0o_600"u8, "0o_600"u8, ""u8),
        new(Int, "0_466"u8, "0_466"u8, ""u8),
        new(Int, "1_000"u8, "1_000"u8, ""u8),
        new(Float, "1_000.000_1"u8, "1_000.000_1"u8, ""u8),
        new(Int, "0x_f00d"u8, "0x_f00d"u8, ""u8),
        new(Float, "0x_f00d.0p1_2"u8, "0x_f00d.0p1_2"u8, ""u8),
        new(Int, "0b__1000"u8, "0b__1000"u8, "'_' must separate successive digits"u8),
        new(Int, "0o60___0"u8, "0o60___0"u8, "'_' must separate successive digits"u8),
        new(Int, "0466_"u8, "0466_"u8, "'_' must separate successive digits"u8),
        new(Float, "1_."u8, "1_."u8, "'_' must separate successive digits"u8),
        new(Float, "0._1"u8, "0._1"u8, "'_' must separate successive digits"u8),
        new(Float, "2.7_e0"u8, "2.7_e0"u8, "'_' must separate successive digits"u8),
        new(Int, "0x___0"u8, "0x___0"u8, "'_' must separate successive digits"u8),
        new(Float, "0x1.0_p0"u8, "0x1.0_p0"u8, "'_' must separate successive digits"u8)
    }.slice()) {
        var s = @new<global::go.text.scanner_package.Scanner>().Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(test.src)));
        @string err = default!;
        s.Value.Error = (ж<global::go.text.scanner_package.Scanner> sΔ1, @string msg) => {
            if (err == ""u8) {
                err = msg;
            }
        };
        foreach (var (i, want) in strings.Split(test.tokens, " "u8)) {
            err = ""u8;
            var tok = s.Scan();
            @string lit = s.TokenText();
            if (i == 0) {
                if (tok != test.tok) {
                    Ꮡt.Errorf("%q: got token %s; want %s"u8, test.src, TokenString(tok), TokenString(test.tok));
                }
                if (err != test.err) {
                    Ꮡt.Errorf("%q: got error %q; want %q"u8, test.src, err, test.err);
                }
            }
            if (lit != want) {
                Ꮡt.Errorf("%q: got literal %q (%s); want %s"u8, test.src, lit, TokenString(tok), want);
            }
        }
        // make sure we read all
        {
            var tok = s.Scan(); if (tok != EOF) {
                Ꮡt.Errorf("%q: got %s; want EOF"u8, test.src, TokenString(tok));
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestIssue30320_type {
    internal @string @in, want;
    internal nuint mode;
}

public static void TestIssue30320(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestIssue30320_type[]{
        new("foo01.bar31.xx-0-1-1-0"u8, "01 31 0 1 1 0"u8, ScanInts),
        new("foo0/12/0/5.67"u8, "0 12 0 5 67"u8, ScanInts),
        new("xxx1e0yyy"u8, "1 0"u8, ScanInts),
        new("1_2"u8, "1_2"u8, ScanInts),
        new("xxx1.0yyy2e3ee"u8, "1 0 2 3"u8, ScanInts),
        new("xxx1.0yyy2e3ee"u8, "1.0 2e3"u8, ScanFloats)
    }.slice()) {
        @string got = extractInts(test.@in, test.mode);
        if (got != test.want) {
            Ꮡt.Errorf("%q: got %q; want %q"u8, test.@in, got, test.want);
        }
    }
}

internal static @string /*res*/ extractInts(@string t, nuint mode) {
    @string res = default!;

    ref var s = ref heap(new global::go.text.scanner_package.Scanner(), out var Ꮡs);
    Ꮡs.Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(t)));
    s.Mode = mode;
    while (ᐧ) {
        {
            var tok = Ꮡs.Scan();
            var exprᴛ1 = tok;
            if (exprᴛ1 == Int || exprᴛ1 == Float) {
                if (len(res) > 0) {
                    res += " "u8;
                }
                res += Ꮡs.TokenText();
            }
            else if (exprᴛ1 == EOF) {
                return res;
            }
        }

    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloWorldˢ = "hello \n\nworld\n!\n"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string Rᶜ = "hello world!"u8;

public static void TestIssue50909(ж<testing.T> Ꮡt) {
    ref var s = ref heap(new global::go.text.scanner_package.Scanner(), out var Ꮡs);
    Ꮡs.Init(new scanner_test_package.strings_ReaderжReader(strings.NewReader(helloWorldˢ)));
    s.IsIdentRune = (rune ch, nint _) => ch != (rune)'\n';
    @string r = ""u8;
    nint n = 0;
    while (Ꮡs.Scan() != EOF && n < 10) {
        r += Ꮡs.TokenText();
        n++;
    }
    @string R = Rᶜ;
    const nint N = 3;
    if (r != R || n != N) {
        Ꮡt.Errorf("got %q (n = %d); want %q (n = %d)"u8, r, n, R, (nint)(N));
    }
}

} // end scanner_internal_test_package
