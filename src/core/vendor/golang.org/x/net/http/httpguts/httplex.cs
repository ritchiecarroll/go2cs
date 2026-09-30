// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.net.http;

using net = net_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using idna = go.vendor.golang.org.x.net.idna_package;
using go.vendor.golang.org.x.net;
using unicode;

partial class httpguts_package {

internal static array<bool> isTokenTable = new array<bool>(256){
    [(rune)'!'] = true,
    [(rune)'#'] = true,
    [(rune)'$'] = true,
    [(rune)'%'] = true,
    [(rune)'&'] = true,
    [(rune)'\''] = true,
    [(rune)'*'] = true,
    [(rune)'+'] = true,
    [(rune)'-'] = true,
    [(rune)'.'] = true,
    [(rune)'0'] = true,
    [(rune)'1'] = true,
    [(rune)'2'] = true,
    [(rune)'3'] = true,
    [(rune)'4'] = true,
    [(rune)'5'] = true,
    [(rune)'6'] = true,
    [(rune)'7'] = true,
    [(rune)'8'] = true,
    [(rune)'9'] = true,
    [(rune)'A'] = true,
    [(rune)'B'] = true,
    [(rune)'C'] = true,
    [(rune)'D'] = true,
    [(rune)'E'] = true,
    [(rune)'F'] = true,
    [(rune)'G'] = true,
    [(rune)'H'] = true,
    [(rune)'I'] = true,
    [(rune)'J'] = true,
    [(rune)'K'] = true,
    [(rune)'L'] = true,
    [(rune)'M'] = true,
    [(rune)'N'] = true,
    [(rune)'O'] = true,
    [(rune)'P'] = true,
    [(rune)'Q'] = true,
    [(rune)'R'] = true,
    [(rune)'S'] = true,
    [(rune)'T'] = true,
    [(rune)'U'] = true,
    [(rune)'W'] = true,
    [(rune)'V'] = true,
    [(rune)'X'] = true,
    [(rune)'Y'] = true,
    [(rune)'Z'] = true,
    [(rune)'^'] = true,
    [(rune)'_'] = true,
    [(rune)'`'] = true,
    [(rune)'a'] = true,
    [(rune)'b'] = true,
    [(rune)'c'] = true,
    [(rune)'d'] = true,
    [(rune)'e'] = true,
    [(rune)'f'] = true,
    [(rune)'g'] = true,
    [(rune)'h'] = true,
    [(rune)'i'] = true,
    [(rune)'j'] = true,
    [(rune)'k'] = true,
    [(rune)'l'] = true,
    [(rune)'m'] = true,
    [(rune)'n'] = true,
    [(rune)'o'] = true,
    [(rune)'p'] = true,
    [(rune)'q'] = true,
    [(rune)'r'] = true,
    [(rune)'s'] = true,
    [(rune)'t'] = true,
    [(rune)'u'] = true,
    [(rune)'v'] = true,
    [(rune)'w'] = true,
    [(rune)'x'] = true,
    [(rune)'y'] = true,
    [(rune)'z'] = true,
    [(rune)'|'] = true,
    [(rune)'~'] = true
};

public static bool IsTokenRune(rune r) {
    return r < utf8.RuneSelf && isTokenTable[(byte)r];
}

// HeaderValuesContainsToken reports whether any string in values
// contains the provided token, ASCII case-insensitively.
public static bool HeaderValuesContainsToken(slice<@string> values, @string token) {
    foreach (var (_, v) in values) {
        if (headerValueContainsToken(v, token)) {
            return true;
        }
    }
    return false;
}

// isOWS reports whether b is an optional whitespace byte, as defined
// by RFC 7230 section 3.2.3.
internal static bool isOWS(byte b) {
    return b == (rune)' ' || b == (rune)'\t';
}

// trimOWS returns x with all optional whitespace removes from the
// beginning and end.
internal static @string trimOWS(@string x) {
    // TODO: consider using strings.Trim(x, " \t") instead,
    // if and when it's fast enough. See issue 10292.
    // But this ASCII-only code will probably always beat UTF-8
    // aware code.
    while (len(x) > 0 && isOWS(x[0])) {
        x = x[1..];
    }
    while (len(x) > 0 && isOWS(x[len(x) - 1])) {
        x = x.slice(0, len(x) - 1);
    }
    return x;
}

// headerValueContainsToken reports whether v (assumed to be a
// 0#element, in the ABNF extension described in RFC 7230 section 7)
// contains token amongst its comma-separated tokens, ASCII
// case-insensitively.
internal static bool headerValueContainsToken(@string v, @string token) {
    for (nint comma = strings.IndexByte(v, (rune)','); comma != -1; comma = strings.IndexByte(v, (rune)',')) {
        if (tokenEqual(trimOWS(v.slice(0, comma)), token)) {
            return true;
        }
        v = v.slice(comma + 1);
    }
    return tokenEqual(trimOWS(v), token);
}

// lowerASCII returns the ASCII lowercase version of b.
internal static byte lowerASCII(byte b) {
    if ((rune)'A' <= b && b <= (rune)'Z') {
        return (byte)(b + ((rune)'a' - (rune)'A'));
    }
    return b;
}

// tokenEqual reports whether t1 and t2 are equal, ASCII case-insensitively.
internal static bool tokenEqual(@string t1, @string t2) {
    if (len(t1) != len(t2)) {
        return false;
    }
    foreach (var (i, b) in t1) {
        if (b >= utf8.RuneSelf) {
            // No UTF-8 or non-ASCII allowed in tokens.
            return false;
        }
        if (lowerASCII((byte)b) != lowerASCII(t2[i])) {
            return false;
        }
    }
    return true;
}

// isLWS reports whether b is linear white space, according
// to http://www.w3.org/Protocols/rfc2616/rfc2616-sec2.html#sec2.2
//
//	LWS            = [CRLF] 1*( SP | HT )
internal static bool isLWS(byte b) {
    return b == (rune)' ' || b == (rune)'\t';
}

// isCTL reports whether b is a control byte, according
// to http://www.w3.org/Protocols/rfc2616/rfc2616-sec2.html#sec2.2
//
//	CTL            = <any US-ASCII control character
//	                 (octets 0 - 31) and DEL (127)>
internal static bool isCTL(byte b) {
    const byte del = 0x7f; // a CTL
    return b < (rune)' ' || b == del;
}

// ValidHeaderFieldName reports whether v is a valid HTTP/1.x header name.
// HTTP/2 imposes the additional restriction that uppercase ASCII
// letters are not allowed.
//
// RFC 7230 says:
//
//	header-field   = field-name ":" OWS field-value OWS
//	field-name     = token
//	token          = 1*tchar
//	tchar = "!" / "#" / "$" / "%" / "&" / "'" / "*" / "+" / "-" / "." /
//	        "^" / "_" / "`" / "|" / "~" / DIGIT / ALPHA
public static bool ValidHeaderFieldName(@string v) {
    if (len(v) == 0) {
        return false;
    }
    for (nint i = 0; i < len(v); i++) {
        if (!isTokenTable[v[i]]) {
            return false;
        }
    }
    return true;
}

// ValidHostHeader reports whether h is a valid host header.
public static bool ValidHostHeader(@string h) {
    // The latest spec is actually this:
    //
    // http://tools.ietf.org/html/rfc7230#section-5.4
    //     Host = uri-host [ ":" port ]
    //
    // Where uri-host is:
    //     http://tools.ietf.org/html/rfc3986#section-3.2.2
    //
    // But we're going to be much more lenient for now and just
    // search for any byte that's not a valid byte in any of those
    // expressions.
    for (nint i = 0; i < len(h); i++) {
        if (!validHostByte[h[i]]) {
            return false;
        }
    }
    return true;
}

// sub-delims
// sub-delims
// pct-encoded (and used in IPv6 zones)
// sub-delims
// sub-delims
// sub-delims
// sub-delims
// sub-delims
// sub-delims
// unreserved
// unreserved
// IPv6address + Host expression's optional port
// sub-delims
// sub-delims
// sub-delims
// unreserved
// unreserved
// See the validHostHeader comment.
internal static array<bool> validHostByte = new array<bool>(256){
    [(rune)'0'] = true, [(rune)'1'] = true, [(rune)'2'] = true, [(rune)'3'] = true, [(rune)'4'] = true, [(rune)'5'] = true, [(rune)'6'] = true, [(rune)'7'] = true,
    [(rune)'8'] = true, [(rune)'9'] = true,
    [(rune)'a'] = true, [(rune)'b'] = true, [(rune)'c'] = true, [(rune)'d'] = true, [(rune)'e'] = true, [(rune)'f'] = true, [(rune)'g'] = true, [(rune)'h'] = true,
    [(rune)'i'] = true, [(rune)'j'] = true, [(rune)'k'] = true, [(rune)'l'] = true, [(rune)'m'] = true, [(rune)'n'] = true, [(rune)'o'] = true, [(rune)'p'] = true,
    [(rune)'q'] = true, [(rune)'r'] = true, [(rune)'s'] = true, [(rune)'t'] = true, [(rune)'u'] = true, [(rune)'v'] = true, [(rune)'w'] = true, [(rune)'x'] = true,
    [(rune)'y'] = true, [(rune)'z'] = true,
    [(rune)'A'] = true, [(rune)'B'] = true, [(rune)'C'] = true, [(rune)'D'] = true, [(rune)'E'] = true, [(rune)'F'] = true, [(rune)'G'] = true, [(rune)'H'] = true,
    [(rune)'I'] = true, [(rune)'J'] = true, [(rune)'K'] = true, [(rune)'L'] = true, [(rune)'M'] = true, [(rune)'N'] = true, [(rune)'O'] = true, [(rune)'P'] = true,
    [(rune)'Q'] = true, [(rune)'R'] = true, [(rune)'S'] = true, [(rune)'T'] = true, [(rune)'U'] = true, [(rune)'V'] = true, [(rune)'W'] = true, [(rune)'X'] = true,
    [(rune)'Y'] = true, [(rune)'Z'] = true,
    [(rune)'!'] = true,
    [(rune)'$'] = true,
    [(rune)'%'] = true,
    [(rune)'&'] = true,
    [(rune)'('] = true,
    [(rune)')'] = true,
    [(rune)'*'] = true,
    [(rune)'+'] = true,
    [(rune)','] = true,
    [(rune)'-'] = true,
    [(rune)'.'] = true,
    [(rune)':'] = true,
    [(rune)';'] = true,
    [(rune)'='] = true,
    [(rune)'['] = true,
    [(rune)'\''] = true,
    [(rune)']'] = true,
    [(rune)'_'] = true,
    [(rune)'~'] = true
};

// ValidHeaderFieldValue reports whether v is a valid "field-value" according to
// http://www.w3.org/Protocols/rfc2616/rfc2616-sec4.html#sec4.2 :
//
//	message-header = field-name ":" [ field-value ]
//	field-value    = *( field-content | LWS )
//	field-content  = <the OCTETs making up the field-value
//	                 and consisting of either *TEXT or combinations
//	                 of token, separators, and quoted-string>
//
// http://www.w3.org/Protocols/rfc2616/rfc2616-sec2.html#sec2.2 :
//
//	TEXT           = <any OCTET except CTLs,
//	                  but including LWS>
//	LWS            = [CRLF] 1*( SP | HT )
//	CTL            = <any US-ASCII control character
//	                 (octets 0 - 31) and DEL (127)>
//
// RFC 7230 says:
//
//	field-value    = *( field-content / obs-fold )
//	obj-fold       =  N/A to http2, and deprecated
//	field-content  = field-vchar [ 1*( SP / HTAB ) field-vchar ]
//	field-vchar    = VCHAR / obs-text
//	obs-text       = %x80-FF
//	VCHAR          = "any visible [USASCII] character"
//
// http2 further says: "Similarly, HTTP/2 allows header field values
// that are not valid. While most of the values that can be encoded
// will not alter header field parsing, carriage return (CR, ASCII
// 0xd), line feed (LF, ASCII 0xa), and the zero character (NUL, ASCII
// 0x0) might be exploited by an attacker if they are translated
// verbatim. Any request or response that contains a character not
// permitted in a header field value MUST be treated as malformed
// (Section 8.1.2.6). Valid characters are defined by the
// field-content ABNF rule in Section 3.2 of [RFC7230]."
//
// This function does not (yet?) properly handle the rejection of
// strings that begin or end with SP or HTAB.
public static bool ValidHeaderFieldValue(@string v) {
    for (nint i = 0; i < len(v); i++) {
        var b = v[i];
        if (isCTL(b) && !isLWS(b)) {
            return false;
        }
    }
    return true;
}

internal static bool isASCII(@string s) {
    for (nint i = 0; i < len(s); i++) {
        if (s[i] >= utf8.RuneSelf) {
            return false;
        }
    }
    return true;
}

// PunycodeHostPort returns the IDNA Punycode version
// of the provided "host" or "host:port" string.
public static (@string, error) PunycodeHostPort(@string v) {
    if (isASCII(v)) {
        return (v, default!);
    }
    var (host, port, err) = net.SplitHostPort(v);
    if (err != default!) {
        // The input 'v' argument was just a "host" argument,
        // without a port. This error should not be returned
        // to the caller.
        host = v;
        port = ""u8;
    }
    (host, err) = idna.ToASCII(host);
    if (err != default!) {
        // Non-UTF-8? Not representable in Punycode, in any
        // case.
        return ("", err);
    }
    if (port == ""u8) {
        return (host, default!);
    }
    return (net.JoinHostPort(host, port), default!);
}

} // end httpguts_package
