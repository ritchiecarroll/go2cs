// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package scanner implements a scanner for Go source text.
// It takes a []byte as source which can then be tokenized
// through repeated calls to the Scan method.
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using scannerhooks = global::go.go.@internal.scannerhooks_package;
using token = global::go.go.token_package;
using filepath = path.filepath_package;
using strconv = strconv_package;
using unicode = unicode_package;
using utf8 = global::go.unicode.utf8_package;
using global::go.go;
using global::go.go.@internal;
using global::go.unicode;
using path;
using ꓸꓸꓸany = Span<any>;

partial class scanner_package {

// An ErrorHandler may be provided to [Scanner.Init]. If a syntax error is
// encountered and a handler was installed, the handler is called with a
// position and an error message. The position points to the beginning of
// the offending token.
// type ErrorHandler is a methodless func type — rendered inline as its base delegate

// A Scanner holds the scanner's internal state while processing
// a given text. It can be allocated as part of another data
// structure but must be initialized via [Scanner.Init] before use.
partial struct Scanner {
    // immutable state
    internal ж<tokenꓸFile> @file;                 // source file handle
    internal @string dir;                         // directory portion of file.Name()
    internal slice<byte> src;                     // source
    internal Action<tokenꓸPosition, @string> err; // error reporting; or nil
    internal Mode mode;                           // scanning mode
    // scanning state
    internal rune ch;             // current character
    internal nint offset;         // character offset
    internal nint rdOffset;       // reading offset (position after current character)
    internal nint lineOffset;     // current line offset
    internal bool insertSemi;     // insert a semicolon before next newline
    internal tokenꓸPos nlPos;     // position of newline in preceding comment
    internal tokenꓸPos stringEnd; // end position; defined only for STRING tokens
    // public state - ok to modify
    public nint ErrorCount; // number of errors encountered
}

// Provide go/parser with backdoor access to the StringEnd information.
[GoInit] internal static void init() {
    scannerhooks.StringEnd = (any scanner) => scanner._<ж<Scanner>>().stringEnd;
}

internal static UntypedInt bom => 0xFEFF; // byte order mark, only permitted as very first character
internal static UntypedInt eof => -1;     // end of file

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string illegalCharacterNulˢ = "illegal character NUL"u8;
internal static readonly @string illegalUtf8EncodingGotˢ = "illegal UTF-8 encoding (got UTF-16)"u8;
internal static readonly @string illegalUtf8Encodingˢ = "illegal UTF-8 encoding"u8;
internal static readonly @string illegalByteOrderMarkˢ = "illegal byte order mark"u8;

// Read the next Unicode char into s.ch.
// s.ch < 0 means end-of-file.
//
// For optimization, there is some overlap between this method and
// s.scanIdentifier.
internal static void next(this ref Scanner s) {
    if (s.rdOffset < len(s.src)) {
        s.offset = s.rdOffset;
        if (s.ch == '\n') {
            s.lineOffset = s.offset;
            s.@file.AddLine(s.offset);
        }
        var r = (rune)s.src[s.rdOffset];
        nint w = 1;
        switch (ᐧ) {
        case {} when r is 0: {
            s.error(s.offset, illegalCharacterNulˢ);
            break;
        }
        case {} when r >= utf8.RuneSelf: {
            // not ASCII
            (r, w) = utf8.DecodeRune(s.src.slice(s.rdOffset));
            if (r == utf8.RuneError && w == 1) {
                var @in = s.src.slice(s.rdOffset);
                if (s.offset == 0 && len(@in) >= 2 && (@in[0] == 0xFF && @in[1] == 0xFE || @in[0] == 0xFE && @in[1] == 0xFF)) {
                    // U+FEFF BOM at start of file, encoded as big- or little-endian
                    // UCS-2 (i.e. 2-byte UTF-16). Give specific error (go.dev/issue/71950).
                    s.error(s.offset, illegalUtf8EncodingGotˢ);
                    s.rdOffset += len(@in); // consume all input to avoid error cascade
                } else {
                    s.error(s.offset, illegalUtf8Encodingˢ);
                }
            } else if (r == bom && s.offset > 0) {
                s.error(s.offset, illegalByteOrderMarkˢ);
            }
            break;
        }}
        s.rdOffset += w;
        s.ch = r;
    } else {
        s.offset = len(s.src);
        if (s.ch == '\n') {
            s.lineOffset = s.offset;
            s.@file.AddLine(s.offset);
        }
        s.ch = eof;
    }
}

// peek returns the byte following the most recently read character without
// advancing the scanner. If the scanner is at EOF, peek returns 0.
internal static byte peek(this ref Scanner s) {
    if (s.rdOffset < len(s.src)) {
        return s.src[s.rdOffset];
    }
    return 0;
}

// A Mode value is a set of flags (or 0).
// They control scanner behavior.
partial struct Mode /*num:nuint*/;

public static Mode ScanComments => /* 1 << iota */ 1; // return comments as COMMENT tokens
internal static Mode dontInsertSemis => 2;            // do not automatically insert semicolons - for testing only

// Init prepares the scanner s to tokenize the text src by setting the
// scanner at the beginning of src. The scanner uses the file set file
// for position information and it adds line information for each line.
// It is ok to re-use the same file when re-scanning the same file as
// line information which is already present is ignored. Init causes a
// panic if the file size does not match the src size.
//
// Calls to [Scanner.Scan] will invoke the error handler err if they encounter a
// syntax error and err is not nil. Also, for each error encountered,
// the [Scanner] field ErrorCount is incremented by one. The mode parameter
// determines how comments are handled.
//
// Note that Init may call err if there is an error in the first character
// of the file.
public static void Init(this ref Scanner s, ж<tokenꓸFile> Ꮡfile, slice<byte> src, Action<tokenꓸPosition, @string> err, Mode mode) {
    ref var @file = ref Ꮡfile.DerefOrNull();

    // Explicitly initialize all fields since a scanner may be reused.
    if (@file.Size() != len(src)) {
        throw panic(fmt.Sprintf("file size (%d) does not match src len (%d)"u8, @file.Size(), len(src)));
    }
    s.@file = Ꮡfile;
    (s.dir, _) = filepath.Split(@file.Name());
    s.src = src;
    s.err = err;
    s.mode = mode;

    s.ch = ' ';
    s.offset = 0;
    s.rdOffset = 0;
    s.lineOffset = 0;
    s.insertSemi = false;
    s.ErrorCount = 0;

    s.next();
    if (s.ch == bom) {
        s.next(); // ignore BOM at file beginning
    }
}

internal static void error(this ref Scanner s, nint offs, @string msg) {
    if (s.err != nil) {
        s.err(s.@file.Position(s.@file.Pos(offs)), msg);
    }
    s.ErrorCount++;
}

internal static void errorf(this ref Scanner s, nint offs, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    s.error(offs, fmt.Sprintf(format, args.ꓸꓸꓸ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string commentNotTerminatedˢ = "comment not terminated"u8;

// scanComment returns the text of the comment and (if nonzero)
// the offset of the first newline within it, which implies a
// /*...*/ comment.
internal static (@string, nint) scanComment(this ref Scanner s) {
    // initial '/' already consumed; s.ch == '/' || s.ch == '*'
    nint offs = s.offset - 1; // position of initial '/'
    nint next = -1;           // position immediately following the comment; < 0 means invalid comment
    nint numCR = 0;
    nint nlOffset = 0; // offset of first newline within /*...*/ comment

    if (s.ch == '/') {
        //-style comment
        // (the final '\n' is not considered part of the comment)
        s.next();
        while (s.ch != '\n' && s.ch >= 0) {
            if (s.ch == '\r') {
                numCR++;
            }
            s.next();
        }
        // if we are at '\n', the position following the comment is afterwards
        next = s.offset;
        if (s.ch == '\n') {
            next++;
        }
        goto exit;
    }

    /*-style comment */
    s.next();
    while (s.ch >= 0) {
        var ch = s.ch;
        if (ch == '\r') {
            numCR++;
        } else if (ch == '\n' && nlOffset == 0) {
            nlOffset = s.offset;
        }
        s.next();
        if (ch == '*' && s.ch == '/') {
            s.next();
            next = s.offset;
            goto exit;
        }
    }

    s.error(offs, commentNotTerminatedˢ);

exit:
    var lit = s.src.slice(offs, s.offset);

    // On Windows, a (//-comment) line may end in "\r\n".
    // Remove the final '\r' before analyzing the text for
    // line directives (matching the compiler). Remove any
    // other '\r' afterwards (matching the pre-existing be-
    // havior of the scanner).
    if (numCR > 0 && len(lit) >= 2 && lit[1] == '/' && lit[len(lit) - 1] == '\r') {
        lit = lit.slice(0, len(lit) - 1);
        numCR--;
    }

    // interpret line directives
    // (//line directives must start at the beginning of the current line)
    if (next >= 0 /* implies valid comment */ && (lit[1] == '*' || offs == s.lineOffset) && bytes.HasPrefix(lit[2..], prefix)) {
        s.updateLineInfo(next, offs, lit);
    }

    if (numCR > 0) {
        lit = stripCR(lit, lit[1] == '*');
    }

    return ((@string)lit, nlOffset);
}

internal static slice<byte> prefix = slice<byte>("line "u8);

// updateLineInfo parses the incoming comment text at offset offs
// as a line directive. If successful, it updates the line info table
// for the position next per the line directive.
internal static void updateLineInfo(this ref Scanner s, nint next, nint offs, slice<byte> text) {
    // extract comment text
    if (text[1] == '*') {
        text = text.slice(0, len(text) - 2); // lop off trailing "*/"
    }
    text = text[7..]; // lop off leading "//line " or "/*line "
    offs += 7;

    var (i, n, ok) = trailingDigits(text);
    if (i == 0) {
        return; // ignore (not a line directive)
    }
    // i > 0

    if (!ok) {
        // text has a suffix :xxx but xxx is not a number
        s.error(offs + i, "invalid line number: "u8 + ((sstring)(text.slice(i))));
        return;
    }

    // Put a cap on the maximum size of line and column numbers.
    // 30 bits allows for some additional space before wrapping an int32.
    // Keep this consistent with cmd/compile/internal/syntax.PosMax.
    const nint maxLineCol = /* 1 << 30 */ 1073741824;
    nint line = default;
    nint col = default;
    var (i2, n2, ok2) = trailingDigits(text.slice(0, i - 1));
    if (ok2) {
        //line filename:line:col
        (i, i2) = (i2, i);
        (line, col) = (n2, n);
        if (col == 0 || col > maxLineCol) {
            s.error(offs + i2, "invalid column number: "u8 + ((sstring)(text.slice(i2))));
            return;
        }
        text = text.slice(0, i2 - 1); // lop off ":col"
    } else {
        //line filename:line
        line = n;
    }

    if (line == 0 || line > maxLineCol) {
        s.error(offs + i, "invalid line number: "u8 + ((sstring)(text.slice(i))));
        return;
    }

    // If we have a column (//line filename:line:col form),
    // an empty filename means to use the previous filename.
    @string filename = (@string)(text.slice(0, i - 1)); // lop off ":line", and trim white space
    if (filename == ""u8 && ok2) {
        filename = s.@file.Position(s.@file.Pos(offs)).Filename;
    } else if (filename != ""u8) {
        // Put a relative filename in the current directory.
        // This is for compatibility with earlier releases.
        // See issue 26671.
        filename = filepath.Clean(filename);
        if (!filepath.IsAbs(filename)) {
            filename = filepath.Join(s.dir, filename);
        }
    }

    s.@file.AddLineColumnInfo(next, filename, line, col);
}

internal static (nint, nint, bool) trailingDigits(slice<byte> text) {
    nint i = bytes.LastIndexByte(text, (byte)':'); // look from right (Windows filenames may contain ':')
    if (i < 0) {
        return (0, 0, false); // no ":"
    }
    // i >= 0
    var (n, err) = strconv.ParseUint((@string)(text.slice(i + 1)), 10, 0);
    return (i + 1, (nint)n, err == nil);
}

internal static bool isLetter(rune ch) {
    return 'a' <= lower(ch) && lower(ch) <= 'z' || ch == '_' || ch >= utf8.RuneSelf && unicode.IsLetter(ch);
}

internal static bool isDigit(rune ch) {
    return isDecimal(ch) || ch >= utf8.RuneSelf && unicode.IsDigit(ch);
}

// scanIdentifier reads the string of valid identifier characters at s.offset.
// It must only be called when s.ch is known to be a valid letter.
//
// Be careful when making changes to this function: it is optimized and affects
// scanning performance significantly.
internal static @string scanIdentifier(this ref Scanner s) {
    nint offs = s.offset;

    // Optimize for the common case of an ASCII identifier.
    //
    // Ranging over s.src[s.rdOffset:] lets us avoid some bounds checks, and
    // avoids conversions to runes.
    //
    // In case we encounter a non-ASCII character, fall back on the slower path
    // of calling into s.next().
    foreach (var (rdOffset, b) in s.src.slice(s.rdOffset)) {
        if ('a' <= b && b <= 'z' || 'A' <= b && b <= 'Z' || b == '_' || '0' <= b && b <= '9') {
            // Avoid assigning a rune for the common case of an ascii character.
            continue;
        }
        s.rdOffset += rdOffset;
        if (0 < b && b < utf8.RuneSelf) {
            // Optimization: we've encountered an ASCII character that's not a letter
            // or number. Avoid the call into s.next() and corresponding set up.
            //
            // Note that s.next() does some line accounting if s.ch is '\n', so this
            // shortcut is only possible because we know that the preceding character
            // is not '\n'.
            s.ch = (rune)b;
            s.offset = s.rdOffset;
            s.rdOffset++;
            goto exit;
        }
        // We know that the preceding character is valid for an identifier because
        // scanIdentifier is only called when s.ch is a letter, so calling s.next()
        // at s.rdOffset resets the scanner state.
        s.next();
        while (isLetter(s.ch) || isDigit(s.ch)) {
            s.next();
        }
        goto exit;
    }
    s.offset = len(s.src);
    s.rdOffset = len(s.src);
    s.ch = eof;

exit:
    return (@string)(s.src.slice(offs, s.offset));
}

internal static nint digitVal(rune ch) {
    switch (ᐧ) {
    case {} when '0' <= ch && ch <= '9': {
        return (nint)(ch - '0');
    }
    case {} when 'a' <= lower(ch) && lower(ch) <= 'f': {
        return (nint)(lower(ch) - 'a' + 10);
    }}
    return 16; // larger than any legal digit val
}

internal static rune lower(rune ch) {
    return (rune)(((rune)'a' - (rune)'A') | ch); // returns lower-case ch iff ch is ASCII letter
}

internal static bool isDecimal(rune ch) {
    return '0' <= ch && ch <= '9';
}

internal static bool isHex(rune ch) {
    return '0' <= ch && ch <= '9' || 'a' <= lower(ch) && lower(ch) <= 'f';
}

// digits accepts the sequence { digit | '_' }.
// If base <= 10, digits accepts any decimal digit but records
// the offset (relative to the source start) of a digit >= base
// in *invalid, if *invalid < 0.
// digits returns a bitset describing whether the sequence contained
// digits (bit 0 is set), or separators '_' (bit 1 is set).
internal static nint /*digsep*/ digits(this ref Scanner s, nint @base, ж<nint> Ꮡinvalid) {
    nint digsep = default;
    ref var invalid = ref Ꮡinvalid.DerefOrNull();

    if (@base <= 10) {
        var max = (rune)('0' + @base);
        while (isDecimal(s.ch) || s.ch == '_') {
            nint ds = 1;
            if (s.ch == '_') {
                ds = 2;
            } else if (s.ch >= max && invalid < 0) {
                invalid = s.offset; // record invalid rune offset
            }
            digsep |= (nint)(ds);
            s.next();
        }
    } else {
        while (isHex(s.ch) || s.ch == '_') {
            nint ds = 1;
            if (s.ch == '_') {
                ds = 2;
            }
            digsep |= (nint)(ds);
            s.next();
        }
    }
    return digsep;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string exponentHasNoDigitsˢ = "exponent has no digits"u8;
internal static readonly @string hexadecimalMantissaˢ = "hexadecimal mantissa requires a 'p' exponent"u8;
internal static readonly @string mustSeparateSuccessiveˢ = "'_' must separate successive digits"u8;

internal static (token.Token, @string) scanNumber(this ref Scanner s) {
    nint offs = s.offset;
    token.Token tok = token.ILLEGAL;

    nint @base = 10;                                        // number base
    var prefix = (rune)0;                                   // one of 0 (decimal), '0' (0-octal), 'x', 'o', or 'b'
    nint digsep = 0;                                        // bit 0: digit present, bit 1: '_' present
    ref var invalid = ref heap<nint>(-1, out var Ꮡinvalid); // index of invalid digit in literal, or < 0

    // integer part
    if (s.ch != '.') {
        tok = token.INT;
        if (s.ch == '0') {
            s.next();
            switch (lower(s.ch)) {
            case 'x': {
                s.next();
                (@base, prefix) = (16, 'x');
                break;
            }
            case 'o': {
                s.next();
                (@base, prefix) = (8, 'o');
                break;
            }
            case 'b': {
                s.next();
                (@base, prefix) = (2, 'b');
                break;
            }
            default: {
                (@base, prefix) = (8, '0');
                digsep = 1; // leading 0
                break;
            }}
        }
        digsep |= (nint)(s.digits(@base, Ꮡinvalid));
    }

    // fractional part
    if (s.ch == '.') {
        tok = token.FLOAT;
        if (prefix == 'o' || prefix == 'b') {
            s.error(s.offset, "invalid radix point in "u8 + litname(prefix));
        }
        s.next();
        digsep |= (nint)(s.digits(@base, Ꮡinvalid));
    }

    if ((digsep & 1) == 0) {
        s.error(s.offset, litname(prefix) + " has no digits"u8);
    }

    // exponent
    {
        var e = lower(s.ch); if (e == 'e' || e == 'p') {
            switch (ᐧ) {
            case {} when e == 'e' && prefix != 0 && prefix != '0': {
                s.errorf(s.offset, "%q exponent requires decimal mantissa"u8, s.ch);
                break;
            }
            case {} when e == 'p' && prefix != 'x': {
                s.errorf(s.offset, "%q exponent requires hexadecimal mantissa"u8, s.ch);
                break;
            }}
            s.next();
            tok = token.FLOAT;
            if (s.ch == '+' || s.ch == '-') {
                s.next();
            }
            nint ds = s.digits(10, nil);
            digsep |= (nint)(ds);
            if ((ds & 1) == 0) {
                s.error(s.offset, exponentHasNoDigitsˢ);
            }
        } else if (prefix == 'x' && tok == token.FLOAT) {
            s.error(s.offset, hexadecimalMantissaˢ);
        }
    }

    // suffix 'i'
    if (s.ch == 'i') {
        tok = token.IMAG;
        s.next();
    }

    @string lit = (@string)(s.src.slice(offs, s.offset));
    if (tok == token.INT && invalid >= 0) {
        s.errorf(invalid, "invalid digit %q in %s"u8, lit[invalid - offs], litname(prefix));
    }
    if ((digsep & 2) != 0) {
        {
            nint i = invalidSep(lit); if (i >= 0) {
                s.error(offs + i, mustSeparateSuccessiveˢ);
            }
        }
    }

    return (tok, lit);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hexadecimalLiteralˢ = "hexadecimal literal"u8;
internal static readonly @string octalLiteralˢ = "octal literal"u8;
internal static readonly @string binaryLiteralˢ = "binary literal"u8;
internal static readonly @string decimalLiteralˢ = "decimal literal"u8;

internal static @string litname(rune prefix) {
    switch (prefix) {
    case 'x': {
        return hexadecimalLiteralˢ;
    }
    case 'o' or '0': {
        return octalLiteralˢ;
    }
    case 'b': {
        return binaryLiteralˢ;
    }}
    return decimalLiteralˢ;
}

// invalidSep returns the index of the first invalid separator in x, or -1.
internal static nint invalidSep(@string x) {
    var x1 = (rune)' '; // prefix char, we only care if it's 'x'
    var d = (rune)'.';  // digit, one of '_', '0' (a digit), or '.' (anything else)
    nint i = 0;

    // a prefix counts as a digit
    if (len(x) >= 2 && x[0] == '0') {
        x1 = lower((rune)x[1]);
        if (x1 == 'x' || x1 == 'o' || x1 == 'b') {
            d = '0';
            i = 2;
        }
    }

    // mantissa and exponent
    for (; i < len(x); i++) {
        var p = d; // previous digit
        d = (rune)x[i];
        switch (ᐧ) {
        case {} when d is '_': {
            if (p != '0') {
                return i;
            }
            break;
        }
        case {} when isDecimal(d) || x1 == 'x' && isHex(d): {
            d = '0';
            break;
        }
        default: {
            if (p == '_') {
                return i - 1;
            }
            d = '.';
            break;
        }}
    }
    if (d == '_') {
        return len(x) - 1;
    }

    return -1;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unknownEscapeSequenceˢ = "unknown escape sequence"u8;
internal static readonly @string escapeSequenceNotˢ = "escape sequence not terminated"u8;
internal static readonly @string escapeSequenceIsInvalidˢ = "escape sequence is invalid Unicode code point"u8;

// scanEscape parses an escape sequence where rune is the accepted
// escaped quote. In case of a syntax error, it stops at the offending
// character (without consuming it) and returns false. Otherwise
// it returns true.
internal static bool scanEscape(this ref Scanner s, rune quote) {
    nint offs = s.offset;

    nint n = default;
    uint32 @base = default;
    uint32 max = default;
    switch (s.ch) {
    case var ᴛ when ᴛ == 'a' || ᴛ == 'b' || ᴛ == 'f' || ᴛ == 'n' || ᴛ == 'r' || ᴛ == 't' || ᴛ == 'v' || ᴛ == '\\' || ᴛ == quote: {
        s.next();
        return true;
    }
    case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7': {
        (n, @base, max) = (3, 8, 255);
        break;
    }
    case 'x': {
        s.next();
        (n, @base, max) = (2, 16, 255);
        break;
    }
    case 'u': {
        s.next();
        (n, @base, max) = (4, 16, unicode.MaxRune);
        break;
    }
    case 'U': {
        s.next();
        (n, @base, max) = (8, 16, unicode.MaxRune);
        break;
    }
    default: {
        @string msg = unknownEscapeSequenceˢ;
        if (s.ch < 0) {
            msg = escapeSequenceNotˢ;
        }
        s.error(offs, msg);
        return false;
    }}

    uint32 x = default;
    while (n > 0) {
        var d = (uint32)digitVal(s.ch);
        if (d >= @base) {
            @string msg = fmt.Sprintf("illegal character %#U in escape sequence"u8, s.ch);
            if (s.ch < 0) {
                msg = escapeSequenceNotˢ;
            }
            s.error(s.offset, msg);
            return false;
        }
        x = x * @base + d;
        s.next();
        n--;
    }

    if (x > max || 0xD800 <= x && x < 0xE000) {
        s.error(offs, escapeSequenceIsInvalidˢ);
        return false;
    }

    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runeLiteralNotTerminatedˢ = "rune literal not terminated"u8;
internal static readonly @string illegalRuneLiteralˢ = "illegal rune literal"u8;

internal static @string scanRune(this ref Scanner s) {
    // '\'' opening already consumed
    nint offs = s.offset - 1;

    var valid = true;
    nint n = 0;
    for (;;) {
        var ch = s.ch;
        if (ch == '\n' || ch < 0) {
            // only report error if we don't have one already
            if (valid) {
                s.error(offs, runeLiteralNotTerminatedˢ);
                valid = false;
            }
            break;
        }
        s.next();
        if (ch == '\'') {
            break;
        }
        n++;
        if (ch == '\\') {
            if (!s.scanEscape('\'')) {
                valid = false;
            }
            // continue to read to closing quote
        }
    }

    if (valid && n != 1) {
        s.error(offs, illegalRuneLiteralˢ);
    }

    return (@string)(s.src.slice(offs, s.offset));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringLiteralNotˢ = "string literal not terminated"u8;

internal static @string scanString(this ref Scanner s) {
    // '"' opening already consumed
    nint offs = s.offset - 1;

    for (;;) {
        var ch = s.ch;
        if (ch == '\n' || ch < 0) {
            s.error(offs, stringLiteralNotˢ);
            break;
        }
        s.next();
        if (ch == '"') {
            break;
        }
        if (ch == '\\') {
            s.scanEscape('"');
        }
    }

    return (@string)(s.src.slice(offs, s.offset));
}

internal static slice<byte> stripCR(slice<byte> b, bool comment) {
    var c = new slice<byte>(len(b));
    nint i = 0;
    foreach (var (j, ch) in b) {
        // In a /*-style comment, don't strip \r from *\r/ (incl.
        // sequences of \r from *\r\r...\r/) since the resulting
        // */ would terminate the comment too early unless the \r
        // is immediately following the opening /* in which case
        // it's ok because /*/ is not closed yet (issue #11151).
        if (ch != '\r' || comment && i > len("/*") && c[i - 1] == '*' && j + 1 < len(b) && b[j + 1] == '/') {
            c[i] = ch;
            i++;
        }
    }
    return c.slice(0, i);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rawStringLiteralNotˢ = "raw string literal not terminated"u8;

internal static (@string, nint) scanRawString(this ref Scanner s) {
    // '`' opening already consumed
    nint offs = s.offset - 1;

    var hasCR = false;
    for (;;) {
        var ch = s.ch;
        if (ch < 0) {
            s.error(offs, rawStringLiteralNotˢ);
            break;
        }
        s.next();
        if (ch == '`') {
            break;
        }
        if (ch == '\r') {
            hasCR = true;
        }
    }

    var lit = s.src.slice(offs, s.offset);
    nint rawLen = len(lit);
    if (hasCR) {
        lit = stripCR(lit, false);
    }

    return ((@string)lit, rawLen);
}

internal static void skipWhitespace(this ref Scanner s) {
    while (s.ch == ' ' || s.ch == '\t' || s.ch == '\n' && !s.insertSemi || s.ch == '\r') {
        s.next();
    }
}

// Helper functions for scanning multi-byte tokens such as >> += >>= .
// Different routines recognize different length tok_i based on matches
// of ch_i. If a token ends in '=', the result is tok1 or tok3
// respectively. Otherwise, the result is tok0 if there was no other
// matching character, or tok2 if the matching character was ch2.
internal static token.Token switch2(this ref Scanner s, token.Token tok0, token.Token tok1) {
    if (s.ch == '=') {
        s.next();
        return tok1;
    }
    return tok0;
}

internal static token.Token switch3(this ref Scanner s, token.Token tok0, token.Token tok1, rune ch2, token.Token tok2) {
    if (s.ch == '=') {
        s.next();
        return tok1;
    }
    if (s.ch == ch2) {
        s.next();
        return tok2;
    }
    return tok0;
}

internal static token.Token switch4(this ref Scanner s, token.Token tok0, token.Token tok1, rune ch2, token.Token tok2, token.Token tok3) {
    if (s.ch == '=') {
        s.next();
        return tok1;
    }
    if (s.ch == ch2) {
        s.next();
        if (s.ch == '=') {
            s.next();
            return tok3;
        }
        return tok2;
    }
    return tok0;
}

// Scan scans the next token and returns the token position, the token,
// and its literal string if applicable. The source end is indicated by
// [token.EOF].
//
// If the returned token is a literal ([token.IDENT], [token.INT], [token.FLOAT],
// [token.IMAG], [token.CHAR], [token.STRING]) or [token.COMMENT], the literal string
// has the corresponding value.
//
// If the returned token is a keyword, the literal string is the keyword.
//
// If the returned token is [token.SEMICOLON], the corresponding
// literal string is ";" if the semicolon was present in the source,
// and "\n" if the semicolon was inserted because of a newline or
// at EOF.
//
// If the returned token is [token.ILLEGAL], the literal string is the
// offending character.
//
// In all other cases, Scan returns an empty literal string.
//
// For more tolerant parsing, Scan will return a valid token if
// possible even if a syntax error was encountered. Thus, even
// if the resulting token sequence contains no illegal tokens,
// a client may not assume that no error occurred. Instead it
// must check the scanner's ErrorCount or the number of calls
// of the error handler, if there was one installed.
//
// Scan adds line information to the file added to the file
// set with Init. Token positions are relative to that file
// and thus relative to the file set.
public static (tokenꓸPos pos, token.Token tok, @string lit) Scan(this ref Scanner s) {
    tokenꓸPos pos = default;
    token.Token tok = default;
    @string lit = default;

scanAgain:
    if (s.nlPos.IsValid()) {
        // Return artificial ';' token after /*...*/ comment
        // containing newline, at position of first newline.
        (pos, tok, lit) = (s.nlPos, token.SEMICOLON, "\n");
        s.nlPos = token.NoPos;
        return (pos, tok, lit);
    }

    s.skipWhitespace();

    // current token start
    pos = s.@file.Pos(s.offset);

    // determine token value
    var insertSemi = false;
    {
        var ch = s.ch;
        switch (ᐧ) {
        case {} when isLetter(ch): {
            lit = s.scanIdentifier();
            if (len(lit) > 1) {
                // keywords are longer than one letter - avoid lookup otherwise
                tok = token.Lookup(lit);
                switch (tok) {
                case var ᴛ when ᴛ == token.IDENT || ᴛ == token.BREAK || ᴛ == token.CONTINUE || ᴛ == token.FALLTHROUGH || ᴛ == token.RETURN: {
                    insertSemi = true;
                    break;
                }}
            } else {
                insertSemi = true;
                tok = token.IDENT;
            }
            break;
        }
        case {} when isDecimal(ch) || ch == '.' && isDecimal((rune)s.peek()): {
            insertSemi = true;
            (tok, lit) = s.scanNumber();
            break;
        }
        default: {
            s.next(); // always make progress
            switch (ch) {
            case var ᴛ when ᴛ == eof: {
                if (s.insertSemi) {
                    s.insertSemi = false; // EOF consumed
                    return (pos, token.SEMICOLON, "\n");
                }
                tok = token.EOF;
                break;
            }
            case '\n': {
                // we only reach here if s.insertSemi was
                // set in the first place and exited early
                // from s.skipWhitespace()
                s.insertSemi = false; // newline consumed
                return (pos, token.SEMICOLON, "\n");
            }
            case '"': {
                insertSemi = true;
                tok = token.STRING;
                lit = s.scanString();
                s.stringEnd = pos + ((tokenꓸPos)len(lit));
                break;
            }
            case '\'': {
                insertSemi = true;
                tok = token.CHAR;
                lit = s.scanRune();
                break;
            }
            case '`': {
                insertSemi = true;
                tok = token.STRING;
                nint rawLen = default;
                (lit, rawLen) = s.scanRawString();
                s.stringEnd = pos + ((tokenꓸPos)rawLen);
                break;
            }
            case ':': {
                tok = s.switch2(token.COLON, token.DEFINE);
                break;
            }
            case '.': {
                // fractions starting with a '.' are handled by outer switch
                tok = token.PERIOD;
                if (s.ch == '.' && s.peek() == '.') {
                    s.next();
                    s.next(); // consume last '.'
                    tok = token.ELLIPSIS;
                }
                break;
            }
            case ',': {
                tok = token.COMMA;
                break;
            }
            case ';': {
                tok = token.SEMICOLON;
                lit = ";"u8;
                break;
            }
            case '(': {
                tok = token.LPAREN;
                break;
            }
            case ')': {
                insertSemi = true;
                tok = token.RPAREN;
                break;
            }
            case '[': {
                tok = token.LBRACK;
                break;
            }
            case ']': {
                insertSemi = true;
                tok = token.RBRACK;
                break;
            }
            case '{': {
                tok = token.LBRACE;
                break;
            }
            case '}': {
                insertSemi = true;
                tok = token.RBRACE;
                break;
            }
            case '+': {
                tok = s.switch3(token.ADD, token.ADD_ASSIGN, '+', token.INC);
                if (tok == token.INC) {
                    insertSemi = true;
                }
                break;
            }
            case '-': {
                tok = s.switch3(token.SUB, token.SUB_ASSIGN, '-', token.DEC);
                if (tok == token.DEC) {
                    insertSemi = true;
                }
                break;
            }
            case '*': {
                tok = s.switch2(token.MUL, token.MUL_ASSIGN);
                break;
            }
            case '/': {
                if (s.ch == '/' || s.ch == '*') {
                    // comment
                    var (comment, nlOffset) = s.scanComment();
                    if (s.insertSemi && nlOffset != 0) {
                        // For /*...*/ containing \n, return
                        // COMMENT then artificial SEMICOLON.
                        s.nlPos = s.@file.Pos(nlOffset);
                        s.insertSemi = false;
                    } else {
                        insertSemi = s.insertSemi; // preserve insertSemi info
                    }
                    if ((Mode)(s.mode & ScanComments) == 0) {
                        // skip comment
                        goto scanAgain;
                    }
                    tok = token.COMMENT;
                    lit = comment;
                } else {
                    // division
                    tok = s.switch2(token.QUO, token.QUO_ASSIGN);
                }
                break;
            }
            case '%': {
                tok = s.switch2(token.REM, token.REM_ASSIGN);
                break;
            }
            case '^': {
                tok = s.switch2(token.XOR, token.XOR_ASSIGN);
                break;
            }
            case '<': {
                if (s.ch == '-') {
                    s.next();
                    tok = token.ARROW;
                } else {
                    tok = s.switch4(token.LSS, token.LEQ, '<', token.SHL, token.SHL_ASSIGN);
                }
                break;
            }
            case '>': {
                tok = s.switch4(token.GTR, token.GEQ, '>', token.SHR, token.SHR_ASSIGN);
                break;
            }
            case '=': {
                tok = s.switch2(token.ASSIGN, token.EQL);
                break;
            }
            case '!': {
                tok = s.switch2(token.NOT, token.NEQ);
                break;
            }
            case '&': {
                if (s.ch == '^') {
                    s.next();
                    tok = s.switch2(token.AND_NOT, token.AND_NOT_ASSIGN);
                } else {
                    tok = s.switch3(token.AND, token.AND_ASSIGN, '&', token.LAND);
                }
                break;
            }
            case '|': {
                tok = s.switch3(token.OR, token.OR_ASSIGN, '|', token.LOR);
                break;
            }
            case '~': {
                tok = token.TILDE;
                break;
            }
            default: {
                // next reports unexpected BOMs - don't repeat
                if (ch != bom) {
                    // Report an informative error for U+201[CD] quotation
                    // marks, which are easily introduced via copy and paste.
                    if (ch == '“' || ch == '”') {
                        s.errorf(s.@file.Offset(pos), "curly quotation mark %q (use neutral %q)"u8, ch, (rune)'"');
                    } else {
                        s.errorf(s.@file.Offset(pos), "illegal character %#U"u8, ch);
                    }
                }
                insertSemi = s.insertSemi; // preserve insertSemi info
                tok = token.ILLEGAL;
                lit = (@string)ch;
                break;
            }}
            break;
        }}
    }
    if ((Mode)(s.mode & dontInsertSemis) == 0) {
        s.insertSemi = insertSemi;
    }

    return (pos, tok, lit);
}

} // end scanner_package
