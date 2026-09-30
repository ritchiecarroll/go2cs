// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.html;

using bytes = bytes_package;
using fmt = fmt_package;
using strings = strings_package;
using unicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using ꓸꓸꓸany = Span<any>;

partial class template_package {

// endsWithCSSKeyword reports whether b ends with an ident that
// case-insensitively matches the lower-case kw.
internal static bool endsWithCSSKeyword(slice<byte> b, @string kw) {
    nint i = len(b) - len(kw);
    if (i < 0) {
        // Too short.
        return false;
    }
    if (i != 0) {
        var (r, _) = utf8.DecodeLastRune(b.slice(0, i));
        if (isCSSNmchar(r)) {
            // Too long.
            return false;
        }
    }
    // Many CSS keywords, such as "!important" can have characters encoded,
    // but the URI production does not allow that according to
    // https://www.w3.org/TR/css3-syntax/#TOK-URI
    // This does not attempt to recognize encoded keywords. For example,
    // given "\75\72\6c" and "url" this return false.
    return ((sstring)bytes.ToLower(b.slice(i))) == kw;
}

// isCSSNmchar reports whether rune is allowed anywhere in a CSS identifier.
internal static bool isCSSNmchar(rune r) {
    // Based on the CSS3 nmchar production but ignores multi-rune escape
    // sequences.
    // https://www.w3.org/TR/css3-syntax/#SUBTOK-nmchar
    return (rune)'a' <= r && r <= (rune)'z' || (rune)'A' <= r && r <= (rune)'Z' || (rune)'0' <= r && r <= (rune)'9' || r == (rune)'-' || r == (rune)'_' || 0x80 <= r && r <= 0xd7ff || 0xe000 <= r && r <= 0xfffd || 0x10000 <= r && r <= 0x10ffff;
}

// Non-ASCII cases below.

// decodeCSS decodes CSS3 escapes given a sequence of stringchars.
// If there is no change, it returns the input, otherwise it returns a slice
// backed by a new array.
// https://www.w3.org/TR/css3-syntax/#SUBTOK-stringchar defines stringchar.
internal static slice<byte> decodeCSS(slice<byte> s) {
    nint i = bytes.IndexByte(s, (rune)'\\');
    if (i == -1) {
        return s;
    }
    // The UTF-8 sequence for a codepoint is never longer than 1 + the
    // number hex digits need to represent that codepoint, so len(s) is an
    // upper bound on the output length.
    var b = new slice<byte>(0, len(s));
    while (len(s) != 0) {
        nint iΔ1 = bytes.IndexByte(s, (rune)'\\');
        if (iΔ1 == -1) {
            iΔ1 = len(s);
        }
        (b, s) = (appendꓸꓸꓸ(b, s.slice(0, iΔ1)), s.slice(iΔ1));
        if (len(s) < 2) {
            break;
        }
        // https://www.w3.org/TR/css3-syntax/#SUBTOK-escape
        // escape ::= unicode | '\' [#x20-#x7E#x80-#xD7FF#xE000-#xFFFD#x10000-#x10FFFF]
        if (isHex(s[1])){
            // https://www.w3.org/TR/css3-syntax/#SUBTOK-unicode
            //   unicode ::= '\' [0-9a-fA-F]{1,6} wc?
            nint j = 2;
            while (j < len(s) && j < 7 && isHex(s[j])) {
                j++;
            }
            var r = hexDecode(s.slice(1, j));
            if (r > unicode.MaxRune) {
                (r, j) = (r / 16, j - 1);
            }
            nint n = utf8.EncodeRune(b.slice(len(b), cap(b)), r);
            // The optional space at the end allows a hex
            // sequence to be followed by a literal hex.
            // string(decodeCSS([]byte(`\A B`))) == "\nB"
            (b, s) = (b.slice(0, len(b) + n), skipCSSSpace(s.slice(j)));
        } else {
            // `\\` decodes to `\` and `\"` to `"`.
            var (_, n) = utf8.DecodeRune(s[1..]);
            (b, s) = (appendꓸꓸꓸ(b, s.slice(1, 1 + n)), s.slice(1 + n));
        }
    }
    return b;
}

// isHex reports whether the given character is a hex digit.
internal static bool isHex(byte c) {
    return (rune)'0' <= c && c <= (rune)'9' || (rune)'a' <= c && c <= (rune)'f' || (rune)'A' <= c && c <= (rune)'F';
}

// hexDecode decodes a short hex digit sequence: "10" -> 16.
internal static rune hexDecode(slice<byte> s) {
    var n = (rune)'\x00';
    foreach (var (_, c) in s) {
        n <<= (int)(4);
        switch (ᐧ) {
        case {} when (rune)'0' <= c && c <= (rune)'9': {
            n |= (rune)((rune)((byte)(c - (rune)'0')));
            break;
        }
        case {} when (rune)'a' <= c && c <= (rune)'f': {
            n |= (rune)((rune)((byte)(c - (rune)'a')) + 10);
            break;
        }
        case {} when (rune)'A' <= c && c <= (rune)'F': {
            n |= (rune)((rune)((byte)(c - (rune)'A')) + 10);
            break;
        }
        default: {
            throw panic(fmt.Sprintf("Bad hex digit in %q"u8, s));
            break;
        }}

    }
    return n;
}

// skipCSSSpace returns a suffix of c, skipping over a single space.
internal static slice<byte> skipCSSSpace(slice<byte> c) {
    if (len(c) == 0) {
        return c;
    }
    // wc ::= #x9 | #xA | #xC | #xD | #x20
    switch (c[0]) {
    case (rune)'\t' or (rune)'\n' or (rune)'\f' or (rune)' ': {
        return c[1..];
    }
    case (rune)'\r': {
        if (len(c) >= 2 && c[1] == (rune)'\n') {
            // This differs from CSS3's wc production because it contains a
            // probable spec error whereby wc contains all the single byte
            // sequences in nl (newline) but not CRLF.
            return c[2..];
        }
        return c[1..];
    }}

    return c;
}

// isCSSSpace reports whether b is a CSS space char as defined in wc.
internal static bool isCSSSpace(byte b) {
    switch (b) {
    case (rune)'\t' or (rune)'\n' or (rune)'\f' or (rune)'\r' or (rune)' ': {
        return true;
    }}

    return false;
}

// cssEscaper escapes HTML and CSS special characters using \<hex>+ escapes.
internal static @string cssEscaper(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (s, _) = stringify(args.ꓸꓸꓸ);
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    var r = (rune)0;
    nint w = 0;
    nint written = 0;
    for (nint i = 0; i < len(s); i += w) {
        // See comment in htmlEscaper.
        (r, w) = utf8.DecodeRuneInString(s.slice(i));
        @string repl = default!;
        switch (ᐧ) {
        case {} when (nint)r < len(cssReplacementTable) && cssReplacementTable[r] != "": {
            repl = cssReplacementTable[r];
            break;
        }
        default: {
            continue;
            break;
        }}

        if (written == 0) {
            Ꮡb.Grow(len(s));
        }
        Ꮡb.WriteString(s.slice(written, i));
        Ꮡb.WriteString(repl);
        written = i + w;
        if (repl != @"\\"u8 && (written == len(s) || isHex(s[written]) || isCSSSpace(s[written]))) {
            Ꮡb.WriteByte((rune)' ');
        }
    }
    if (written == 0) {
        return s;
    }
    Ꮡb.WriteString(s.slice(written));
    return b.String();
}

// Encode HTML specials as hex so the output can be embedded
// in HTML attributes without further encoding.
internal static slice<@string> cssReplacementTable = new slice<@string>(126){
    [0] = @"\0"u8,
    [(rune)'\t'] = @"\9"u8,
    [(rune)'\n'] = @"\a"u8,
    [(rune)'\f'] = @"\c"u8,
    [(rune)'\r'] = @"\d"u8,
    [(rune)'"'] = @"\22"u8,
    [(rune)'&'] = @"\26"u8,
    [(rune)'\''] = @"\27"u8,
    [(rune)'('] = @"\28"u8,
    [(rune)')'] = @"\29"u8,
    [(rune)'+'] = @"\2b"u8,
    [(rune)'/'] = @"\2f"u8,
    [(rune)':'] = @"\3a"u8,
    [(rune)';'] = @"\3b"u8,
    [(rune)'<'] = @"\3c"u8,
    [(rune)'>'] = @"\3e"u8,
    [(rune)'\\'] = @"\\"u8,
    [(rune)'{'] = @"\7b"u8,
    [(rune)'}'] = @"\7d"u8
};

internal static slice<byte> expressionBytes = slice<byte>("expression"u8);

internal static slice<byte> mozBindingBytes = slice<byte>("mozbinding"u8);

// cssValueFilter allows innocuous CSS values in the output including CSS
// quantities (10px or 25%), ID or class literals (#foo, .bar), keyword values
// (inherit, blue), and colors (#888).
// It filters out unsafe values, such as those that affect token boundaries,
// and anything that might execute scripts.
internal static @string cssValueFilter(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    var (s, t) = stringify(args.ꓸꓸꓸ);
    if (t == contentTypeCSS) {
        return s;
    }
    var (b, id) = (decodeCSS(slice<byte>(s)), new slice<byte>(0, 64));
    // CSS3 error handling is specified as honoring string boundaries per
    // https://www.w3.org/TR/css3-syntax/#error-handling :
    //     Malformed declarations. User agents must handle unexpected
    //     tokens encountered while parsing a declaration by reading until
    //     the end of the declaration, while observing the rules for
    //     matching pairs of (), [], {}, "", and '', and correctly handling
    //     escapes. For example, a malformed declaration may be missing a
    //     property, colon (:) or value.
    // So we need to make sure that values do not have mismatched bracket
    // or quote characters to prevent the browser from restarting parsing
    // inside a string that might embed JavaScript source.
    foreach (var (i, c) in b) {
        switch (c) {
        case 0 or (rune)'"' or (rune)'\'' or (rune)'(' or (rune)')' or (rune)'/' or (rune)';' or (rune)'@' or (rune)'[' or (rune)'\\' or (rune)']' or (rune)'`' or (rune)'{' or (rune)'}' or (rune)'<' or (rune)'>': {
            return filterFailsafe;
        }
        case (rune)'-': {
            if (i != 0 && b[i - 1] == (rune)'-') {
                // Disallow <!-- or -->.
                // -- should not appear in valid identifiers.
                return filterFailsafe;
            }
            break;
        }
        default: {
            if (c < utf8.RuneSelf && isCSSNmchar((rune)c)) {
                id = append(id, c);
            }
            break;
        }}

    }
    id = bytes.ToLower(id);
    if (bytes.Contains(id, expressionBytes) || bytes.Contains(id, mozBindingBytes)) {
        return filterFailsafe;
    }
    return ((@string)b);
}

} // end template_package
