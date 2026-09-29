// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using base64 = encoding.base64_package;
using errors = errors_package;
using fmt = fmt_package;
using Δio = io_package;
using strings = strings_package;
using Δunicode = unicode_package;
using utf8 = go.unicode.utf8_package;
using encoding;
using go.unicode;

partial class mime_package {

[GoType("num:byte")] partial struct WordEncoder;

public static WordEncoder BEncoding => /* WordEncoder('b') */ 98;
public static WordEncoder QEncoding => /* WordEncoder('q') */ 113;

internal static error errInvalidWord = errors.New("mime: invalid RFC 2047 encoded-word"u8);

// Encode returns the encoded-word form of s. If s is ASCII without special
// characters, it is returned unchanged. The provided charset is the IANA
// charset name of s. It is case insensitive.
public static @string Encode(this WordEncoder e, @string charset, @string s) {
    if (!needsEncoding(s)) {
        return s;
    }
    return e.encodeWord(charset, s);
}

internal static bool needsEncoding(@string s) {
    foreach (var (_, b) in s) {
        if ((b < (rune)' ' || b > (rune)'~') && b != (rune)'\t') {
            return true;
        }
    }
    return false;
}

// encodeWord encodes a string into an encoded-word.
internal static @string encodeWord(this WordEncoder e, @string charset, @string s) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    // Could use a hint like len(s)*3, but that's not enough for cases
    // with word splits and too much for simpler inputs.
    // 48 is close to maxEncodedWordLen/2, but adjusted to allocator size class.
    Ꮡbuf.Grow(48);
    e.openWord(Ꮡbuf, charset);
    if (e == BEncoding){
        e.bEncode(Ꮡbuf, charset, s);
    } else {
        e.qEncode(Ꮡbuf, charset, s);
    }
    closeWord(Ꮡbuf);
    return buf.String();
}

internal static UntypedInt maxEncodedWordLen => 75;
internal const nint maxContentLen = /* maxEncodedWordLen - len("=?UTF-8?q?") - len("?=") */ 63;

internal static nint maxBase64Len = base64.StdEncoding.DecodedLen(maxContentLen);

// bEncode encodes s using base64 encoding and writes it to buf.
internal static void bEncode(this WordEncoder e, ж<strings.Builder> Ꮡbuf, @string charset, @string s) {
    var w = base64.NewEncoder(base64.StdEncoding, new strings_BuilderжWriter(Ꮡbuf));
    // If the charset is not UTF-8 or if the content is short, do not bother
    // splitting the encoded-word.
    if (!isUTF8(charset) || base64.StdEncoding.EncodedLen(len(s)) <= maxContentLen) {
        Δio.WriteString(w, s);
        w.Close();
        return;
    }
    nint currentLen = default!;
    nint last = default!;
    nint runeLen = default!;
    for (nint i = 0; i < len(s); i += runeLen) {
        // Multi-byte characters must not be split across encoded-words.
        // See RFC 2047, section 5.3.
        (_, runeLen) = utf8.DecodeRuneInString(s.slice(i));
        if (currentLen + runeLen <= maxBase64Len){
            currentLen += runeLen;
        } else {
            Δio.WriteString(w, s.slice(last, i));
            w.Close();
            e.splitWord(Ꮡbuf, charset);
            last = i;
            currentLen = runeLen;
        }
    }
    Δio.WriteString(w, s.slice(last));
    w.Close();
}

// qEncode encodes s using Q encoding and writes it to buf. It splits the
// encoded-words when necessary.
internal static void qEncode(this WordEncoder e, ж<strings.Builder> Ꮡbuf, @string charset, @string s) {
    // We only split encoded-words when the charset is UTF-8.
    if (!isUTF8(charset)) {
        writeQString(Ꮡbuf, s);
        return;
    }
    nint currentLen = default!;
    nint runeLen = default!;
    for (nint i = 0; i < len(s); i += runeLen) {
        var b = s[i];
        // Multi-byte characters must not be split across encoded-words.
        // See RFC 2047, section 5.3.
        nint encLen = default!;
        if (b >= (rune)' ' && b <= (rune)'~' && b != (rune)'=' && b != (rune)'?' && b != (rune)'_'){
            (runeLen, encLen) = (1, 1);
        } else {
            (_, runeLen) = utf8.DecodeRuneInString(s.slice(i));
            encLen = 3 * runeLen;
        }
        if (currentLen + encLen > maxContentLen) {
            e.splitWord(Ꮡbuf, charset);
            currentLen = 0;
        }
        writeQString(Ꮡbuf, s.slice(i, i + runeLen));
        currentLen += encLen;
    }
}

// writeQString encodes s using Q encoding and writes it to buf.
internal static void writeQString(ж<strings.Builder> Ꮡbuf, @string s) {
    for (nint i = 0; i < len(s); i++) {
        {
            var b = s[i];
            switch (ᐧ) {
            case {} when b is (rune)' ': {
                Ꮡbuf.WriteByte((rune)'_');
                break;
            }
            case {} when b >= (rune)'!' && b <= (rune)'~' && b != (rune)'=' && b != (rune)'?' && b != (rune)'_': {
                Ꮡbuf.WriteByte(b);
                break;
            }
            default: {
                Ꮡbuf.WriteByte((rune)'=');
                Ꮡbuf.WriteByte(upperhex[(b >> (int)(4))]);
                Ꮡbuf.WriteByte(upperhex[(byte)(b & 0x0f)]);
                break;
            }}
        }

    }
}

// openWord writes the beginning of an encoded-word into buf.
internal static void openWord(this WordEncoder e, ж<strings.Builder> Ꮡbuf, @string charset) {
    Ꮡbuf.WriteString("=?"u8);
    Ꮡbuf.WriteString(charset);
    Ꮡbuf.WriteByte((rune)'?');
    Ꮡbuf.WriteByte((byte)e);
    Ꮡbuf.WriteByte((rune)'?');
}

// closeWord writes the end of an encoded-word into buf.
internal static void closeWord(ж<strings.Builder> Ꮡbuf) {
    Ꮡbuf.WriteString("?="u8);
}

// splitWord closes the current encoded-word and opens a new one.
internal static void splitWord(this WordEncoder e, ж<strings.Builder> Ꮡbuf, @string charset) {
    closeWord(Ꮡbuf);
    Ꮡbuf.WriteByte((rune)' ');
    e.openWord(Ꮡbuf, charset);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string utf8ˢ = "UTF-8"u8;

internal static bool isUTF8(@string charset) {
    return strings.EqualFold(charset, utf8ˢ);
}

internal static readonly @string upperhex = "0123456789ABCDEF"u8;

// A WordDecoder decodes MIME headers containing RFC 2047 encoded-words.
[GoType] partial struct WordDecoder {
    // CharsetReader, if non-nil, defines a function to generate
    // charset-conversion readers, converting from the provided
    // charset into UTF-8.
    // Charsets are always lower-case. utf-8, iso-8859-1 and us-ascii charsets
    // are handled by default.
    // One of the CharsetReader's result values must be non-nil.
    public Func<@string, Δio.Reader, (Δio.Reader, error)> CharsetReader;
}

// Decode decodes an RFC 2047 encoded-word.
[GoRecv] public static (@string, error) Decode(this ref WordDecoder d, @string word) {
    // See https://tools.ietf.org/html/rfc2047#section-2 for details.
    // Our decoder is permissive, we accept empty encoded-text.
    if (len(word) < 8 || !strings.HasPrefix(word, "=?"u8) || !strings.HasSuffix(word, "?="u8) || strings.Count(word, "?"u8) != 4) {
        return ("", errInvalidWord);
    }
    word = word.slice(2, len(word) - 2);
    // split word "UTF-8?q?text" into "UTF-8", 'q', and "text"
    var (charset, text, _) = strings.Cut(word, "?"u8);
    if (charset == ""u8) {
        return ("", errInvalidWord);
    }
    (var encoding, text, _) = strings.Cut(text, "?"u8);
    if (len(encoding) != 1) {
        return ("", errInvalidWord);
    }
    var (content, err) = decode(encoding[0], text);
    if (err != default!) {
        return ("", err);
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    {
        var errΔ1 = d.convert(Ꮡbuf, charset, content); if (errΔ1 != default!) {
            return ("", errΔ1);
        }
    }
    return (buf.String(), default!);
}

// DecodeHeader decodes all encoded-words of the given string. It returns an
// error if and only if [WordDecoder.CharsetReader] of d returns an error.
[GoRecv] public static (@string, error) DecodeHeader(this ref WordDecoder d, @string header) {
    // If there is no encoded-word, returns before creating a buffer.
    nint i = strings.Index(header, "=?"u8);
    if (i == -1) {
        return (header, default!);
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    Ꮡbuf.WriteString(header.slice(0, i));
    header = header.slice(i);
    var betweenWords = false;
    while (ᐧ) {
        nint start = strings.Index(header, "=?"u8);
        if (start == -1) {
            break;
        }
        nint cur = start + len("=?");
        nint iΔ1 = strings.Index(header.slice(cur), "?"u8);
        if (iΔ1 == -1) {
            break;
        }
        @string charset = header.slice(cur, cur + iΔ1);
        cur += iΔ1 + len("?");
        if (len(header) < cur + len("Q??=")) {
            break;
        }
        var encoding = header[cur];
        cur++;
        if (header[cur] != (rune)'?') {
            break;
        }
        cur++;
        nint j = strings.Index(header.slice(cur), "?="u8);
        if (j == -1) {
            break;
        }
        @string text = header.slice(cur, cur + j);
        nint end = cur + j + len("?=");
        var (content, err) = decode(encoding, text);
        if (err != default!) {
            betweenWords = false;
            Ꮡbuf.WriteString(header.slice(0, start + 2));
            header = header.slice(start + 2);
            continue;
        }
        // Write characters before the encoded-word. White-space and newline
        // characters separating two encoded-words must be deleted.
        if (start > 0 && (!betweenWords || hasNonWhitespace(header.slice(0, start)))) {
            Ꮡbuf.WriteString(header.slice(0, start));
        }
        {
            var errΔ1 = d.convert(Ꮡbuf, charset, content); if (errΔ1 != default!) {
                return ("", errΔ1);
            }
        }
        header = header.slice(end);
        betweenWords = true;
    }
    if (len(header) > 0) {
        Ꮡbuf.WriteString(header);
    }
    return (buf.String(), default!);
}

internal static (slice<byte>, error) decode(byte encoding, @string text) {
    switch (encoding) {
    case (rune)'B' or (rune)'b': {
        return base64.StdEncoding.DecodeString(text);
    }
    case (rune)'Q' or (rune)'q': {
        return qDecode(text);
    }
    default: {
        return (default!, errInvalidWord);
    }}

}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string utf8ˢ2 = "utf-8"u8;
internal static readonly @string iso88591ˢ = "iso-8859-1"u8;
internal static readonly @string usAsciiˢ = "us-ascii"u8;

[GoRecv] internal static error convert(this ref WordDecoder d, ж<strings.Builder> Ꮡbuf, @string charset, slice<byte> content) {
    switch (ᐧ) {
    case {} when strings.EqualFold(utf8ˢ2, charset): {
        Ꮡbuf.Write(content);
        break;
    }
    case {} when strings.EqualFold(iso88591ˢ, charset): {
        foreach (var (_, c) in content) {
            Ꮡbuf.WriteRune((rune)c);
        }
        break;
    }
    case {} when strings.EqualFold(usAsciiˢ, charset): {
        foreach (var (_, c) in content) {
            if (c >= utf8.RuneSelf){
                Ꮡbuf.WriteRune(Δunicode.ReplacementChar);
            } else {
                Ꮡbuf.WriteByte(c);
            }
        }
        break;
    }
    default: {
        if (d.CharsetReader == default!) {
            return fmt.Errorf("mime: unhandled charset %q"u8, charset);
        }
        var (r, errΔ1) = d.CharsetReader(strings.ToLower(charset), new bytes_ReaderжReader(bytes.NewReader(content)));
        if (errΔ1 != default!) {
            return errΔ1;
        }
        {
            (_, errΔ1) = Δio.Copy(new strings_BuilderжWriter(Ꮡbuf), r); if (errΔ1 != default!) {
                return errΔ1;
            }
        }
        break;
    }}

    return default!;
}

// hasNonWhitespace reports whether s (assumed to be ASCII) contains at least
// one byte of non-whitespace.
internal static bool hasNonWhitespace(@string s) {
    foreach (var (_, b) in s) {
        switch (b) {
        case (rune)' ' or (rune)'\t' or (rune)'\n' or (rune)'\r': {
            break;
        }
        default: {
            return true;
        }}

    }
    // Encoded-words can only be separated by linear white spaces which does
    // not include vertical tabs (\v).
    return false;
}

// qDecode decodes a Q encoded string.
internal static (slice<byte>, error) qDecode(@string s) {
    var dec = new slice<byte>(len(s));
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        {
            var c = s[i];
            switch (ᐧ) {
            case {} when c is (rune)'_': {
                dec[n] = (rune)' ';
                break;
            }
            case {} when c is (rune)'=': {
                if (i + 2 >= len(s)) {
                    return (default!, errInvalidWord);
                }
                var (b, err) = readHexByte(s[i + 1], s[i + 2]);
                if (err != default!) {
                    return (default!, err);
                }
                dec[n] = b;
                i += 2;
                break;
            }
            case {} when (c <= (rune)'~' && c >= (rune)' ') || c == (rune)'\n' || c == (rune)'\r' || c == (rune)'\t': {
                dec[n] = c;
                break;
            }
            default: {
                return (default!, errInvalidWord);
            }}
        }

        n++;
    }
    return (dec.slice(0, n), default!);
}

// readHexByte returns the byte from its quoted-printable representation.
internal static (byte, error) readHexByte(byte a, byte b) {
    byte hb = default!;
    byte lb = default!;
    error err = default!;
    {
        (hb, err) = fromHex(a); if (err != default!) {
            return (0, err);
        }
    }
    {
        (lb, err) = fromHex(b); if (err != default!) {
            return (0, err);
        }
    }
    return ((byte)((byte)(hb << (int)(4)) | lb), default!);
}

internal static (byte, error) fromHex(byte b) {
    switch (ᐧ) {
    case {} when b >= (rune)'0' && b <= (rune)'9': {
        return ((byte)(b - (rune)'0'), default!);
    }
    case {} when b >= (rune)'A' && b <= (rune)'F': {
        return ((byte)(b - (rune)'A' + 10), default!);
    }
    case {} when b >= (rune)'a' && b <= (rune)'f': {
        return ((byte)(b - (rune)'a' + 10), default!);
    }}

    // Accept badly encoded bytes.
    return (0, fmt.Errorf("mime: invalid hex byte %#02x"u8, b));
}

} // end mime_package
