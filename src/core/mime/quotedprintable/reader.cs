// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package quotedprintable implements quoted-printable encoding as specified by
// RFC 2045.
namespace go.mime;

using bufio = bufio_package;
using bytes = bytes_package;
using fmt = fmt_package;
using io = io_package;

partial class quotedprintable_package {

// Reader is a quoted-printable decoder.
[GoType] partial struct Reader {
    internal ж<bufio.Reader> br;
    internal error rerr;  // last read error
    internal slice<byte> line; // to be consumed before more of br
}

// NewReader returns a quoted-printable reader, decoding from r.
public static ж<Reader> NewReader(io.Reader r) {
    return Ꮡ(new Reader(
        br: bufio.NewReader(r)
    ));
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
    return (0, fmt.Errorf("quotedprintable: invalid hex byte 0x%02x"u8, b));
}

internal static (byte b, error err) readHexByte(slice<byte> v) {
    error err = default!;

    if (len(v) < 2) {
        return (0, io.ErrUnexpectedEOF);
    }
    byte hb = default!;
    byte lb = default!;
    {
        (hb, err) = fromHex(v[0]); if (err != default!) {
            return (0, err);
        }
    }
    {
        (lb, err) = fromHex(v[1]); if (err != default!) {
            return (0, err);
        }
    }
    return ((byte)((byte)(hb << (int)(4)) | lb), default!);
}

internal static bool isQPDiscardWhitespace(rune r) {
    switch (r) {
    case (rune)'\n' or (rune)'\r' or (rune)' ' or (rune)'\t': {
        return true;
    }}

    return false;
}

internal static slice<byte> crlf = slice<byte>("\r\n"u8);
internal static slice<byte> lf = slice<byte>("\n"u8);
internal static slice<byte> softSuffix = slice<byte>("="u8);

// Read reads and decodes quoted-printable data from the underlying reader.
[GoRecv] public static (nint n, error err) Read(this ref Reader r, slice<byte> p) {
    nint n = default!;
    error err = default!;

    // Deviations from RFC 2045:
    // 1. in addition to "=\r\n", "=\n" is also treated as soft line break.
    // 2. it will pass through a '\r' or '\n' not preceded by '=', consistent
    //    with other broken QP encoders & decoders.
    // 3. it accepts soft line-break (=) at end of message (issue 15486); i.e.
    //    the final byte read from the underlying reader is allowed to be '=',
    //    and it will be silently ignored.
    // 4. it takes = as literal = if not followed by two hex digits
    //    but not at end of line (issue 13219).
    while (len(p) > 0) {
        if (len(r.line) == 0) {
            if (r.rerr != default!) {
                return (n, r.rerr);
            }
            (r.line, r.rerr) = r.br.ReadSlice((rune)'\n');
            // Does the line end in CRLF instead of just LF?
            var hasLF = bytes.HasSuffix(r.line, lf);
            var hasCR = bytes.HasSuffix(r.line, crlf);
            var wholeLine = r.line;
            r.line = bytes.TrimRightFunc(wholeLine, isQPDiscardWhitespace);
            if (bytes.HasSuffix(r.line, softSuffix)){
                var rightStripped = wholeLine.slice(len(r.line));
                r.line = r.line.slice(0, len(r.line) - 1);
                if (!bytes.HasPrefix(rightStripped, lf) && !bytes.HasPrefix(rightStripped, crlf) && !(len(rightStripped) == 0 && len(r.line) > 0 && AreEqual(r.rerr, io.EOF))) {
                    r.rerr = fmt.Errorf("quotedprintable: invalid bytes after =: %q"u8, rightStripped);
                }
            } else 
            if (hasLF) {
                if (hasCR){
                    r.line = append(r.line, (byte)((rune)'\r'), (byte)((rune)'\n'));
                } else {
                    r.line = append(r.line, (byte)((rune)'\n'));
                }
            }
            continue;
        }
        var b = r.line[0];
        switch (ᐧ) {
        case {} when b is (rune)'=': {
            (b, err) = readHexByte(r.line[1..]);
            if (err != default!) {
                if (len(r.line) >= 2 && r.line[1] != (rune)'\r' && r.line[1] != (rune)'\n') {
                    // Take the = as a literal =.
                    b = (rune)'=';
                    break;
                }
                return (n, err);
            }
            r.line = r.line[2..]; // 2 of the 3; other 1 is done below
            break;
        }
        case {} when b == (rune)'\t' || b == (rune)'\r' || b == (rune)'\n': {
            break;
            break;
        }
        case {} when b is >= 0x80: {
            break;
            break;
        }
        case {} when b < (rune)' ' || b > (rune)'~': {
            return (n, fmt.Errorf("quotedprintable: invalid unescaped byte 0x%02x in body"u8, // As an extension to RFC 2045, we accept
 // values >= 0x80 without complaint. Issue 22597.
 b));
        }}

        p[0] = b;
        p = p[1..];
        r.line = r.line[1..];
        n++;
    }
    return (n, default!);
}

} // end quotedprintable_package
