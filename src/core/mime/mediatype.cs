// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using fmt = fmt_package;
using maps = maps_package;
using slices = slices_package;
using strings = strings_package;
using Δunicode = unicode_package;
using iter = iter_package;

partial class mime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string utf8ˢ3 = "utf-8''"u8;

// FormatMediaType serializes mediatype t and the parameters
// param as a media type conforming to RFC 2045 and RFC 2616.
// The type and parameter names are written in lower-case.
// When any of the arguments result in a standard violation then
// FormatMediaType returns the empty string.
public static @string FormatMediaType(@string t, map<@string, @string> param) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    {
        var (major, sub, ok) = strings.Cut(t, "/"u8); if (!ok){
            if (!isToken(t)) {
                return ""u8;
            }
            Ꮡb.WriteString(strings.ToLower(t));
        } else {
            if (!isToken(major) || !isToken(sub)) {
                return ""u8;
            }
            Ꮡb.WriteString(strings.ToLower(major));
            Ꮡb.WriteByte((rune)'/');
            Ꮡb.WriteString(strings.ToLower(sub));
        }
    }
    foreach (var (_, attribute) in slices.Sorted(maps.Keys<map<@string, @string>, @string, @string>(param))) {
        @string value = param[attribute];
        Ꮡb.WriteByte((rune)';');
        Ꮡb.WriteByte((rune)' ');
        if (!isToken(attribute)) {
            return ""u8;
        }
        Ꮡb.WriteString(strings.ToLower(attribute));
        var needEnc = needsEncoding(value);
        if (needEnc) {
            // RFC 2231 section 4
            Ꮡb.WriteByte((rune)'*');
        }
        Ꮡb.WriteByte((rune)'=');
        if (needEnc) {
            Ꮡb.WriteString(utf8ˢ3);
            nint offsetΔ1 = 0;
            for (nint index = 0; index < len(value); index++) {
                var ch = value[index];
                // {RFC 2231 section 7}
                // attribute-char := <any (US-ASCII) CHAR except SPACE, CTLs, "*", "'", "%", or tspecials>
                if (ch <= (rune)' ' || ch >= 0x7F || ch == (rune)'*' || ch == (rune)'\'' || ch == (rune)'%' || isTSpecial((rune)ch)) {
                    Ꮡb.WriteString(value.slice(offsetΔ1, index));
                    offsetΔ1 = index + 1;
                    Ꮡb.WriteByte((rune)'%');
                    Ꮡb.WriteByte(upperhex[(ch >> (int)(4))]);
                    Ꮡb.WriteByte(upperhex[(byte)(ch & 0x0F)]);
                }
            }
            Ꮡb.WriteString(value.slice(offsetΔ1));
            continue;
        }
        if (isToken(value)) {
            Ꮡb.WriteString(value);
            continue;
        }
        Ꮡb.WriteByte((rune)'"');
        nint offset = 0;
        for (nint index = 0; index < len(value); index++) {
            var character = value[index];
            if (character == (rune)'"' || character == (rune)'\\') {
                Ꮡb.WriteString(value.slice(offset, index));
                offset = index;
                Ꮡb.WriteByte((rune)'\\');
            }
        }
        Ꮡb.WriteString(value.slice(offset));
        Ꮡb.WriteByte((rune)'"');
    }
    return b.String();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mimeNoMediaTypeˢ = "mime: no media type"u8;
internal static readonly @string mimeExpectedSlashAfterˢ = "mime: expected slash after first token"u8;
internal static readonly @string mimeExpectedTokenAfterˢ = "mime: expected token after slash"u8;
internal static readonly @string mimeUnexpectedContentˢ = "mime: unexpected content after media subtype"u8;

internal static error checkMediaTypeDisposition(@string s) {
    var (typ, rest) = consumeToken(s);
    if (typ == ""u8) {
        return errors.New(mimeNoMediaTypeˢ);
    }
    if (rest == ""u8) {
        return default!;
    }
    if (!strings.HasPrefix(rest, "/"u8)) {
        return errors.New(mimeExpectedSlashAfterˢ);
    }
    (var subtype, rest) = consumeToken(rest[1..]);
    if (subtype == ""u8) {
        return errors.New(mimeExpectedTokenAfterˢ);
    }
    if (rest != ""u8) {
        return errors.New(mimeUnexpectedContentˢ);
    }
    return default!;
}

// ErrInvalidMediaParameter is returned by [ParseMediaType] if
// the media type value was found but there was an error parsing
// the optional parameters
public static error ErrInvalidMediaParameter = errors.New("mime: invalid media parameter"u8);

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mimeDuplicateParameterˢ = "mime: duplicate parameter name"u8;

// ParseMediaType parses a media type value and any optional
// parameters, per RFC 1521.  Media types are the values in
// Content-Type and Content-Disposition headers (RFC 2183).
// On success, ParseMediaType returns the media type converted
// to lowercase and trimmed of white space and a non-nil map.
// If there is an error parsing the optional parameter,
// the media type will be returned along with the error
// [ErrInvalidMediaParameter].
// The returned map, params, maps from the lowercase
// attribute to the attribute value with its case preserved.
public static (@string mediatype, map<@string, @string> @params, error err) ParseMediaType(@string v) {
    @string mediatype = default!;
    map<@string, @string> @params = default!;
    error err = default!;

    var (@base, _, _) = strings.Cut(v, ";"u8);
    mediatype = strings.TrimSpace(strings.ToLower(@base));
    err = checkMediaTypeDisposition(mediatype);
    if (err != default!) {
        return ("", default!, err);
    }
    @params = new map<@string, @string>();
    // Map of base parameter name -> parameter name -> value
    // for parameters containing a '*' character.
    // Lazily initialized.
    map<@string, map<@string, @string>> continuation = default!;
    v = v.slice(len(@base));
    while (len(v) > 0) {
        v = strings.TrimLeftFunc(v, Δunicode.IsSpace);
        if (len(v) == 0) {
            break;
        }
        var (key, value, rest) = consumeMediaParam(v);
        if (key == ""u8) {
            if (strings.TrimSpace(rest) == ";"u8) {
                // Ignore trailing semicolons.
                // Not an error.
                break;
            }
            // Parse error.
            return (mediatype, default!, ErrInvalidMediaParameter);
        }
        var pmap = @params;
        {
            var (baseName, _, ok) = strings.Cut(key, "*"u8); if (ok) {
                if (continuation == default!) {
                    continuation = new map<@string, map<@string, @string>>();
                }
                bool okΔ1 = default!;
                {
                    (pmap, okΔ1) = continuation[baseName, ꟷ]; if (!okΔ1) {
                        continuation[baseName] = new map<@string, @string>();
                        pmap = continuation[baseName];
                    }
                }
            }
        }
        {
            var (vΔ1, exists) = pmap[key, ꟷ]; if (exists && vΔ1 != value) {
                // Duplicate parameter names are incorrect, but we allow them if they are equal.
                return ("", default!, errors.New(mimeDuplicateParameterˢ));
            }
        }
        pmap[key] = value;
        v = rest;
    }
    // Stitch together any continuations or things with stars
    // (i.e. RFC 2231 things with stars: "foo*0" or "foo*")
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    foreach (var (key, pieceMap) in continuation) {
        @string singlePartKey = key + "*"u8;
        {
            var (vΔ2, ok) = pieceMap[singlePartKey, ꟷ]; if (ok) {
                {
                    var (decv, okΔ2) = decode2231Enc(vΔ2); if (okΔ2) {
                        @params[key] = decv;
                    }
                }
                continue;
            }
        }
        buf.Reset();
        var valid = false;
        for (nint n = 0; ᐧ ; n++) {
            @string simplePart = fmt.Sprintf("%s*%d"u8, key, n);
            {
                var (vΔ3, okΔ3) = pieceMap[simplePart, ꟷ]; if (okΔ3) {
                    valid = true;
                    Ꮡbuf.WriteString(vΔ3);
                    continue;
                }
            }
            @string encodedPart = simplePart + "*"u8;
            var (vΔ4, ok) = pieceMap[encodedPart, ꟷ];
            if (!ok) {
                break;
            }
            valid = true;
            if (n == 0){
                {
                    var (decv, okΔ4) = decode2231Enc(vΔ4); if (okΔ4) {
                        Ꮡbuf.WriteString(decv);
                    }
                }
            } else {
                var (decv, _) = percentHexUnescape(vΔ4);
                Ꮡbuf.WriteString(decv);
            }
        }
        if (valid) {
            @params[key] = buf.String();
        }
    }
    return (mediatype, @params, err);
}

internal static (@string, bool) decode2231Enc(@string v) {
    var sv = strings.SplitN(v, "'"u8, 3);
    if (len(sv) != 3) {
        return ("", false);
    }
    // TODO: ignoring lang in sv[1] for now. If anybody needs it we'll
    // need to decide how to expose it in the API. But I'm not sure
    // anybody uses it in practice.
    @string charset = strings.ToLower(sv[0]);
    if (len(charset) == 0) {
        return ("", false);
    }
    if (charset != "us-ascii"u8 && charset != "utf-8"u8) {
        // TODO: unsupported encoding
        return ("", false);
    }
    var (encv, err) = percentHexUnescape(sv[2]);
    if (err != default!) {
        return ("", false);
    }
    return (encv, true);
}

internal static bool isNotTokenChar(rune r) {
    return !isTokenChar(r);
}

// consumeToken consumes a token from the beginning of provided
// string, per RFC 2045 section 5.1 (referenced from 2183), and return
// the token consumed and the rest of the string. Returns ("", v) on
// failure to consume at least one character.
internal static (@string token, @string rest) consumeToken(@string v) {
    nint notPos = strings.IndexFunc(v, isNotTokenChar);
    if (notPos == -1) {
        return (v, "");
    }
    if (notPos == 0) {
        return ("", v);
    }
    return (v.slice(0, notPos), v.slice(notPos));
}

// consumeValue consumes a "value" per RFC 2045, where a value is
// either a 'token' or a 'quoted-string'.  On success, consumeValue
// returns the value consumed (and de-quoted/escaped, if a
// quoted-string) and the rest of the string. On failure, returns
// ("", v).
internal static (@string value, @string rest) consumeValue(@string v) {
    @string value = default!;
    @string rest = default!;

    if (v == ""u8) {
        return (value, rest);
    }
    if (v[0] != (rune)'"') {
        return consumeToken(v);
    }
    // parse a quoted-string
    var buffer = @new<strings.Builder>();
    for (nint i = 1; i < len(v); i++) {
        var r = v[i];
        if (r == (rune)'"') {
            return (buffer.String(), v.slice(i + 1));
        }
        // When MSIE sends a full file path (in "intranet mode"), it does not
        // escape backslashes: "C:\dev\go\foo.txt", not "C:\\dev\\go\\foo.txt".
        //
        // No known MIME generators emit unnecessary backslash escapes
        // for simple token characters like numbers and letters.
        //
        // If we see an unnecessary backslash escape, assume it is from MSIE
        // and intended as a literal backslash. This makes Go servers deal better
        // with MSIE without affecting the way they handle conforming MIME
        // generators.
        if (r == (rune)'\\' && i + 1 < len(v) && isTSpecial((rune)v[i + 1])) {
            buffer.WriteByte(v[i + 1]);
            i++;
            continue;
        }
        if (r == (rune)'\r' || r == (rune)'\n') {
            return ("", v);
        }
        buffer.WriteByte(v[i]);
    }
    // Did not find end quote.
    return ("", v);
}

internal static (@string param, @string value, @string rest) consumeMediaParam(@string v) {
    @string param = default!;
    @string value = default!;
    @string rest = default!;

    rest = strings.TrimLeftFunc(v, Δunicode.IsSpace);
    if (!strings.HasPrefix(rest, ";"u8)) {
        return ("", "", v);
    }
    rest = rest[1..]; // consume semicolon
    rest = strings.TrimLeftFunc(rest, Δunicode.IsSpace);
    (param, rest) = consumeToken(rest);
    param = strings.ToLower(param);
    if (param == ""u8) {
        return ("", "", v);
    }
    rest = strings.TrimLeftFunc(rest, Δunicode.IsSpace);
    if (!strings.HasPrefix(rest, "="u8)) {
        return ("", "", v);
    }
    rest = rest[1..]; // consume equals sign
    rest = strings.TrimLeftFunc(rest, Δunicode.IsSpace);
    (value, var rest2) = consumeValue(rest);
    if (value == ""u8 && rest2 == rest) {
        return ("", "", v);
    }
    rest = rest2;
    return (param, value, rest);
}

internal static (@string, error) percentHexUnescape(@string s) {
    // Count %, check that they're well-formed.
    nint percents = 0;
    for (nint i = 0; i < len(s); ) {
        if (s[i] != (rune)'%') {
            i++;
            continue;
        }
        percents++;
        if (i + 2 >= len(s) || !ishex(s[i + 1]) || !ishex(s[i + 2])) {
            s = s.slice(i);
            if (len(s) > 3) {
                s = s[0..3];
            }
            return ("", fmt.Errorf("mime: bogus characters after %%: %q"u8, s));
        }
        i += 3;
    }
    if (percents == 0) {
        return (s, default!);
    }
    var t = new slice<byte>(len(s) - 2 * percents);
    nint j = 0;
    for (nint i = 0; i < len(s); ) {
        switch (s[i]) {
        case (rune)'%': {
            t[j] = (byte)((byte)(unhex(s[i + 1]) << (int)(4)) | unhex(s[i + 2]));
            j++;
            i += 3;
            break;
        }
        default: {
            t[j] = s[i];
            j++;
            i++;
            break;
        }}

    }
    return (((@string)t), default!);
}

internal static bool ishex(byte c) {
    switch (ᐧ) {
    case {} when (rune)'0' <= c && c <= (rune)'9': {
        return true;
    }
    case {} when (rune)'a' <= c && c <= (rune)'f': {
        return true;
    }
    case {} when (rune)'A' <= c && c <= (rune)'F': {
        return true;
    }}

    return false;
}

internal static byte unhex(byte c) {
    switch (ᐧ) {
    case {} when (rune)'0' <= c && c <= (rune)'9': {
        return (byte)(c - (rune)'0');
    }
    case {} when (rune)'a' <= c && c <= (rune)'f': {
        return (byte)(c - (rune)'a' + 10);
    }
    case {} when (rune)'A' <= c && c <= (rune)'F': {
        return (byte)(c - (rune)'A' + 10);
    }}

    return 0;
}

} // end mime_package
