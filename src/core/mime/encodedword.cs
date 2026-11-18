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
using static mime_package;

partial class mime_package {

// A WordEncoder is an RFC 2047 encoded-word encoder.
partial struct WordEncoder /*num:byte*/;

// BEncoding represents Base64 encoding scheme as defined by RFC 2045.
public static WordEncoder BEncoding => /* WordEncoder('b') */ 98;
// QEncoding represents the Q-encoding scheme as defined by RFC 2047.
public static WordEncoder QEncoding => /* WordEncoder('q') */ 113;

internal static error errInvalidWord = errors.New("mime: invalid RFC 2047 encoded-word");

} partial class mimeᴛWordEncoder {

// Encode returns the encoded-word form of s. If s is ASCII without special
// characters, it is returned unchanged. The provided charset is the IANA
// charset name of s. It is case insensitive.
public static @string Encode(this WordEncoder e, @string charset, @string s) {
    if (!needsEncoding(s)) {
        return s;
    }
    return e.encodeWord(charset, s);
}

} partial class mime_package {

internal static bool needsEncoding(@string s) {
    foreach (var (_, b) in s) {
        if ((b < ' ' || b > '~') && b != '\t') {
            return true;
        }
    }
    return false;
}

} partial class mimeᴛWordEncoder {

// encodeWord encodes a string into an encoded-word.
internal static @string encodeWord(this WordEncoder e, @string charset, @string s) {
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    // Could use a hint like len(s)*3, but that's not enough for cases
    // with word splits and too much for simpler inputs.
    // 48 is close to maxEncodedWordLen/2, but adjusted to allocator size class.
    Ꮡbuf.Grow(48);

    e.openWord(Ꮡbuf, charset);
    if (e == BEncoding) {
        e.bEncode(Ꮡbuf, charset, s);
    } else {
        e.qEncode(Ꮡbuf, charset, s);
    }
    closeWord(Ꮡbuf);

    return buf.String();
}

} partial class mime_package {

// The maximum length of an encoded-word is 75 characters.
// See RFC 2047, section 2.
internal static UntypedInt maxEncodedWordLen => 75;
// maxContentLen is how much content can be encoded, ignoring the header and
// 2-byte footer.
internal const nint maxContentLen = /* maxEncodedWordLen - len("=?UTF-8?q?") - len("?=") */ 63;

internal static nint maxBase64Len = base64.StdEncoding.DecodedLen(maxContentLen);

} partial class mimeᴛWordEncoder {

// bEncode encodes s using base64 encoding and writes it to buf.
internal static void bEncode(this WordEncoder e, ж<strings.Builder> buf, @string charset, @string s) {
    var w = base64.NewEncoder(base64.StdEncoding, new strings_BuilderжWriter(buf));
    // If the charset is not UTF-8 or if the content is short, do not bother
    // splitting the encoded-word.
    if (!isUTF8(charset) || base64.StdEncoding.EncodedLen(len(s)) <= maxContentLen) {
        Δio.WriteString(w, s);
        w.Close();
        return;
    }

    nint currentLen = default;
    nint last = default;
    nint runeLen = default;
    for (nint i = 0; i < len(s); i += runeLen) {
        // Multi-byte characters must not be split across encoded-words.
        // See RFC 2047, section 5.3.
        (_, runeLen) = utf8.DecodeRuneInString(s.slice(i));

        if (currentLen + runeLen <= maxBase64Len) {
            currentLen += runeLen;
        } else {
            Δio.WriteString(w, s.slice(last, i));
            w.Close();
            e.splitWord(buf, charset);
            last = i;
            currentLen = runeLen;
        }
    }
    Δio.WriteString(w, s.slice(last));
    w.Close();
}

// qEncode encodes s using Q encoding and writes it to buf. It splits the
// encoded-words when necessary.
internal static void qEncode(this WordEncoder e, ж<strings.Builder> buf, @string charset, @string s) {
    // We only split encoded-words when the charset is UTF-8.
    if (!isUTF8(charset)) {
        writeQString(buf, s);
        return;
    }

    nint currentLen = default;
    nint runeLen = default;
    for (nint i = 0; i < len(s); i += runeLen) {
        var b = s[i];
        // Multi-byte characters must not be split across encoded-words.
        // See RFC 2047, section 5.3.
        nint encLen = default;
        if (b >= ' ' && b <= '~' && b != '=' && b != '?' && b != '_') {
            (runeLen, encLen) = (1, 1);
        } else {
            (_, runeLen) = utf8.DecodeRuneInString(s.slice(i));
            encLen = 3 * runeLen;
        }

        if (currentLen + encLen > maxContentLen) {
            e.splitWord(buf, charset);
            currentLen = 0;
        }
        writeQString(buf, s.slice(i, i + runeLen));
        currentLen += encLen;
    }
}

} partial class mime_package {

// writeQString encodes s using Q encoding and writes it to buf.
internal static void writeQString(ж<strings.Builder> buf, @string s) {
    for (nint i = 0; i < len(s); i++) {
        var b = s[i];
        switch (ᐧ) {
        case {} when b is (byte)' ': {
            buf.WriteByte((byte)'_');
            break;
        }
        case {} when b >= (byte)'!' && b <= (byte)'~' && b != (byte)'=' && b != (byte)'?' && b != (byte)'_': {
            buf.WriteByte(b);
            break;
        }
        default: {
            buf.WriteByte((byte)'=');
            buf.WriteByte(upperhex[(b >> 4)]);
            buf.WriteByte(upperhex[(byte)(b & 0x0f)]);
            break;
        }}
    }
}

} partial class mimeᴛWordEncoder {

// openWord writes the beginning of an encoded-word into buf.
internal static void openWord(this WordEncoder e, ж<strings.Builder> buf, @string charset) {
    buf.WriteString("=?");
    buf.WriteString(charset);
    buf.WriteByte((byte)'?');
    buf.WriteByte((byte)e);
    buf.WriteByte((byte)'?');
}

} partial class mime_package {

// closeWord writes the end of an encoded-word into buf.
internal static void closeWord(ж<strings.Builder> buf) {
    buf.WriteString("?=");
}

} partial class mimeᴛWordEncoder {

// splitWord closes the current encoded-word and opens a new one.
internal static void splitWord(this WordEncoder e, ж<strings.Builder> buf, @string charset) {
    closeWord(buf);
    buf.WriteByte((byte)' ');
    e.openWord(buf, charset);
}

} partial class mime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string utf8ˢ => ref utf8ˢᵇ;
private static readonly @string utf8ˢᵇ = "UTF-8"u8;

internal static bool isUTF8(@string charset) {
    return strings.EqualFold(charset, utf8ˢ);
}

internal static ref readonly @string upperhex => ref upperhexᵇ;
private static readonly @string upperhexᵇ = "0123456789ABCDEF"u8;

// A WordDecoder decodes MIME headers containing RFC 2047 encoded-words.
partial struct WordDecoder {
    // CharsetReader, if non-nil, defines a function to generate
    // charset-conversion readers, converting from the provided
    // charset into UTF-8.
    // Charsets are always lower-case. utf-8, iso-8859-1 and us-ascii charsets
    // are handled by default.
    // One of the CharsetReader's result values must be non-nil.
    public Func<@string, Δio.Reader, (Δio.Reader, error)> CharsetReader;
}

} partial class mimeᴛWordDecoder {

// Decode decodes an RFC 2047 encoded-word.
public static (@string, error) Decode(this ref WordDecoder d, @string word) {
    // See https://tools.ietf.org/html/rfc2047#section-2 for details.
    // Our decoder is permissive, we accept empty encoded-text.
    if (len(word) < 8 || !strings.HasPrefix(word, "=?") || !strings.HasSuffix(word, "?=") || strings.Count(word, "?") != 4) {
        return ("", errInvalidWord);
    }
    word = word.slice(2, len(word) - 2);

    // split word "UTF-8?q?text" into "UTF-8", 'q', and "text"
    var (charset, text, _) = strings.Cut(word, "?");
    if (charset == ""u8) {
        return ("", errInvalidWord);
    }
    (var encoding, text, _) = strings.Cut(text, "?");
    if (len(encoding) != 1) {
        return ("", errInvalidWord);
    }

    var (content, err) = decode(encoding[0], text);
    if (err != nil) {
        return ("", err);
    }

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    var errΔ1 = d.convert(Ꮡbuf, charset, content); if (errΔ1 != nil) {
        return ("", errΔ1);
    }
    return (buf.String(), default);
}

// DecodeHeader decodes all encoded-words of the given string. It returns an
// error if and only if [WordDecoder.CharsetReader] of d returns an error.
public static (@string, error) DecodeHeader(this ref WordDecoder d, @string header) {
    // If there is no encoded-word, returns before creating a buffer.
    nint i = strings.Index(header, "=?");
    if (i == -1) {
        return (header, default);
    }

    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);

    Ꮡbuf.WriteString(header.slice(0, i));
    header = header.slice(i);

    var betweenWords = false;
    for (;;) {
        nint start = strings.Index(header, "=?");
        if (start == -1) {
            break;
        }
        nint cur = start + len("=?");

        nint iΔ1 = strings.Index(header.slice(cur), "?");
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

        if (header[cur] != '?') {
            break;
        }
        cur++;

        nint j = strings.Index(header.slice(cur), "?=");
        if (j == -1) {
            break;
        }
        @string text = header.slice(cur, cur + j);
        nint end = cur + j + len("?=");

        var (content, err) = decode(encoding, text);
        if (err != nil) {
            betweenWords = false;
            Ꮡbuf.WriteString(header.slice(0, end));
            header = header.slice(end);
            continue;
        }

        // Write characters before the encoded-word. White-space and newline
        // characters separating two encoded-words must be deleted.
        if (start > 0 && (!betweenWords || hasNonWhitespace(header.slice(0, start)))) {
            Ꮡbuf.WriteString(header.slice(0, start));
        }

        var errΔ1 = d.convert(Ꮡbuf, charset, content); if (errΔ1 != nil) {
            return ("", errΔ1);
        }

        header = header.slice(end);
        betweenWords = true;
    }

    if (len(header) > 0) {
        Ꮡbuf.WriteString(header);
    }

    return (buf.String(), default);
}

} partial class mime_package {

internal static (slice<byte>, error) decode(byte encoding, @string text) {
    switch (encoding) {
    case (byte)'B' or (byte)'b': {
        return base64.StdEncoding.DecodeString(text);
    }
    case (byte)'Q' or (byte)'q': {
        return qDecode(text);
    }
    default: {
        return (default, errInvalidWord);
    }}
}

} partial class mimeᴛWordDecoder {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string utf8ˢ2 => ref utf8ˢ2ᵇ;
private static readonly @string utf8ˢ2ᵇ = "utf-8"u8;
internal static ref readonly @string iso88591ˢ => ref iso88591ˢᵇ;
private static readonly @string iso88591ˢᵇ = "iso-8859-1"u8;
internal static ref readonly @string usAsciiˢ => ref usAsciiˢᵇ;
private static readonly @string usAsciiˢᵇ = "us-ascii"u8;

internal static error convert(this ref WordDecoder d, ж<strings.Builder> buf, @string charset, slice<byte> content) {
    switch (ᐧ) {
    case {} when strings.EqualFold(utf8ˢ2, charset): {
        buf.Write(content);
        break;
    }
    case {} when strings.EqualFold(iso88591ˢ, charset): {
        foreach (var (_, c) in range(content)) {
            buf.WriteRune((rune)c);
        }
        break;
    }
    case {} when strings.EqualFold(usAsciiˢ, charset): {
        foreach (var (_, c) in range(content)) {
            if (c >= utf8.RuneSelf) {
                buf.WriteRune(Δunicode.ReplacementChar);
            } else {
                buf.WriteByte(c);
            }
        }
        break;
    }
    default: {
        if (d.CharsetReader == nil) {
            return fmt.Errorf("mime: unhandled charset %q"u8, charset);
        }
        var (r, errΔ1) = d.CharsetReader(strings.ToLower(charset), new bytes_ReaderжReader(bytes.NewReader(content)));
        if (errΔ1 != nil) {
            return errΔ1;
        }
        (_, errΔ1) = Δio.Copy(new strings_BuilderжWriter(buf), r); if (errΔ1 != nil) {
            return errΔ1;
        }
        break;
    }}
    return default;
}

} partial class mime_package {

// hasNonWhitespace reports whether s (assumed to be ASCII) contains at least
// one byte of non-whitespace.
internal static bool hasNonWhitespace(@string s) {
    foreach (var (_, b) in s) {
        switch (b) {
        // Encoded-words can only be separated by linear white spaces which does
        // not include vertical tabs (\v).
        case ' ' or '\t' or '\n' or '\r': {
            break;
        }
        default: {
            return true;
        }}
    }
    return false;
}

// qDecode decodes a Q encoded string.
internal static (slice<byte>, error) qDecode(@string s) {
    var dec = new slice<byte>(len(s));
    nint n = 0;
    for (nint i = 0; i < len(s); i++) {
        var c = s[i];
        switch (ᐧ) {
        case {} when c is (byte)'_': {
            dec[n] = (byte)' ';
            break;
        }
        case {} when c is (byte)'=': {
            if (i + 2 >= len(s)) {
                return (default, errInvalidWord);
            }
            var (b, err) = readHexByte(s[i + 1], s[i + 2]);
            if (err != nil) {
                return (default, err);
            }
            dec[n] = b;
            i += 2;
            break;
        }
        case {} when (c <= (byte)'~' && c >= (byte)' ') || c == (byte)'\n' || c == (byte)'\r' || c == (byte)'\t': {
            dec[n] = c;
            break;
        }
        default: {
            return (default, errInvalidWord);
        }}
        n++;
    }

    return (dec.slice(0, n), default);
}

// readHexByte returns the byte from its quoted-printable representation.
internal static (byte, error) readHexByte(byte a, byte b) {
    byte hb = default;
    byte lb = default;
    error err = default;
    (hb, err) = fromHex(a); if (err != nil) {
        return (0, err);
    }
    (lb, err) = fromHex(b); if (err != nil) {
        return (0, err);
    }
    return ((byte)((byte)(hb << 4) | lb), default);
}

internal static (byte, error) fromHex(byte b) {
    switch (ᐧ) {
    case {} when b >= (byte)'0' && b <= (byte)'9': {
        return ((byte)(b - '0'), default);
    }
    case {} when b >= (byte)'A' && b <= (byte)'F': {
        return ((byte)(b - 'A' + 10), default);
    }
    // Accept badly encoded bytes.
    case {} when b >= (byte)'a' && b <= (byte)'f': {
        return ((byte)(b - 'a' + 10), default);
    }}
    return (0, fmt.Errorf("mime: invalid hex byte %#02x"u8, b));
}

} // end mime_package
