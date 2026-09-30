// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package base64 implements base64 encoding as specified by RFC 4648.
namespace go.encoding;

using binary = go.encoding.binary_package;
using io = io_package;
using slices = slices_package;
using strconv = strconv_package;
using go.encoding;

partial class base64_package {

/*
 * Encodings
 */

// An Encoding is a radix 64 encoding/decoding scheme, defined by a
// 64-character alphabet. The most common encoding is the "base64"
// encoding defined in RFC 4648 and used in MIME (RFC 2045) and PEM
// (RFC 1421).  RFC 4648 also defines an alternate encoding, which is
// the standard encoding with - and _ substituted for + and /.
[GoType] partial struct Encoding {
    internal array<byte> encode = new(64); // mapping of symbol index to symbol byte value
    internal array<uint8> decodeMap = new(256); // mapping of symbol byte value to symbol index
    internal rune padChar;
    internal bool strict;
}

public const rune StdPadding = /* '=' */ 61; // Standard padding character
public const rune NoPadding = -1;  // No padding

internal static readonly @string decodeMapInitialize = ((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}));
internal static UntypedInt invalidIndex => /* '\xff' */ 255;

// NewEncoding returns a new padded Encoding defined by the given alphabet,
// which must be a 64-byte string that contains unique byte values and
// does not contain the padding character or CR / LF ('\r', '\n').
// The alphabet is treated as a sequence of byte values
// without any special treatment for multi-byte UTF-8.
// The resulting Encoding uses the default padding character ('='),
// which may be changed or disabled via [Encoding.WithPadding].
public static ж<Encoding> NewEncoding(@string encoder) {
    if (len(encoder) != 64) {
        throw panic("encoding alphabet is not 64-bytes long");
    }
    var e = @new<Encoding>();
    e.Value.padChar = StdPadding;
    copy((~e).encode[..], encoder);
    copy((~e).decodeMap[..], decodeMapInitialize);
    for (nint i = 0; i < len(encoder); i++) {
        // Note: While we document that the alphabet cannot contain
        // the padding character, we do not enforce it since we do not know
        // if the caller intends to switch the padding from StdPadding later.
        switch (ᐧ) {
        case {} when encoder[i] == (rune)'\n' || encoder[i] == (rune)'\r': {
            throw panic("encoding alphabet contains newline character");
            break;
        }
        case {} when (~e).decodeMap[encoder[i]] != invalidIndex: {
            throw panic("encoding alphabet includes duplicate symbols");
            break;
        }}

        e.Value.decodeMap[encoder[i]] = (uint8)i;
    }
    return e;
}

// WithPadding creates a new encoding identical to enc except
// with a specified padding character, or [NoPadding] to disable padding.
// The padding character must not be '\r' or '\n',
// must not be contained in the encoding's alphabet,
// must not be negative, and must be a rune equal or below '\xff'.
// Padding characters above '\x7f' are encoded as their exact byte value
// rather than using the UTF-8 representation of the codepoint.
public static ж<Encoding> WithPadding(this Encoding encʗp, rune padding) {
    ref var enc = ref heap(encʗp.ΔClone(), out var Ꮡenc);

    switch (ᐧ) {
    case {} when padding < NoPadding || padding == (rune)'\r' || padding == (rune)'\n' || padding > 0xff: {
        throw panic("invalid padding");
        break;
    }
    case {} when padding != NoPadding && enc.decodeMap[(byte)padding] != invalidIndex: {
        throw panic("padding contained in alphabet");
        break;
    }}

    enc.padChar = padding;
    return Ꮡenc;
}

// Strict creates a new encoding identical to enc except with
// strict decoding enabled. In this mode, the decoder requires that
// trailing padding bits are zero, as described in RFC 4648 section 3.5.
//
// Note that the input is still malleable, as new line characters
// (CR and LF) are still ignored.
public static ж<Encoding> Strict(this Encoding encʗp) {
    ref var enc = ref heap(encʗp.ΔClone(), out var Ꮡenc);

    enc.strict = true;
    return Ꮡenc;
}

// StdEncoding is the standard base64 encoding, as defined in RFC 4648.
public static ж<Encoding> StdEncoding = NewEncoding("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"u8);

// URLEncoding is the alternate base64 encoding defined in RFC 4648.
// It is typically used in URLs and file names.
public static ж<Encoding> URLEncoding = NewEncoding("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"u8);

// RawStdEncoding is the standard raw, unpadded base64 encoding,
// as defined in RFC 4648 section 3.2.
// This is the same as [StdEncoding] but omits padding characters.
public static ж<Encoding> RawStdEncoding = (~StdEncoding).WithPadding(NoPadding);

// RawURLEncoding is the unpadded alternate base64 encoding defined in RFC 4648.
// It is typically used in URLs and file names.
// This is the same as [URLEncoding] but omits padding characters.
public static ж<Encoding> RawURLEncoding = (~URLEncoding).WithPadding(NoPadding);

/*
 * Encoder
 */

// Encode encodes src using the encoding enc,
// writing [Encoding.EncodedLen](len(src)) bytes to dst.
//
// The encoding pads the output to a multiple of 4 bytes,
// so Encode is not appropriate for use on individual blocks
// of a large data stream. Use [NewEncoder] instead.
[GoRecv] public static void Encode(this ref Encoding enc, slice<byte> dst, slice<byte> src) {
    if (len(src) == 0) {
        return;
    }
    // enc is a pointer receiver, so the use of enc.encode within the hot
    // loop below means a nil check at every operation. Lift that nil check
    // outside of the loop to speed up the encoder.
    _ = enc.encode;
    nint di = 0;
    nint si = 0;
    nint n = (len(src) / 3) * 3;
    while (si < n) {
        // Convert 3x 8bit source bytes into 4 bytes
        nuint valΔ1 = (nuint)((nuint)(((nuint)src[si + 0] << (int)(16)) | ((nuint)src[si + 1] << (int)(8))) | (nuint)src[si + 2]);
        dst[di + 0] = enc.encode[(nuint)((valΔ1 >> (int)(18)) & 0x3F)];
        dst[di + 1] = enc.encode[(nuint)((valΔ1 >> (int)(12)) & 0x3F)];
        dst[di + 2] = enc.encode[(nuint)((valΔ1 >> (int)(6)) & 0x3F)];
        dst[di + 3] = enc.encode[(nuint)(valΔ1 & 0x3F)];
        si += 3;
        di += 4;
    }
    nint remain = len(src) - si;
    if (remain == 0) {
        return;
    }
    // Add the remaining small block
    nuint val = ((nuint)src[si + 0] << (int)(16));
    if (remain == 2) {
        val |= (nuint)(((nuint)src[si + 1] << (int)(8)));
    }
    dst[di + 0] = enc.encode[(nuint)((val >> (int)(18)) & 0x3F)];
    dst[di + 1] = enc.encode[(nuint)((val >> (int)(12)) & 0x3F)];
    switch (remain) {
    case 2: {
        dst[di + 2] = enc.encode[(nuint)((val >> (int)(6)) & 0x3F)];
        if (enc.padChar != NoPadding) {
            dst[di + 3] = (byte)enc.padChar;
        }
        break;
    }
    case 1: {
        if (enc.padChar != NoPadding) {
            dst[di + 2] = (byte)enc.padChar;
            dst[di + 3] = (byte)enc.padChar;
        }
        break;
    }}

}

// AppendEncode appends the base64 encoded src to dst
// and returns the extended buffer.
[GoRecv] public static slice<byte> AppendEncode(this ref Encoding enc, slice<byte> dst, slice<byte> src) {
    nint n = enc.EncodedLen(len(src));
    dst = slices.Grow<slice<byte>, byte>(dst, n);
    enc.Encode(dst.slice(len(dst)).slice(0, n), src);
    return dst.slice(0, len(dst) + n);
}

// EncodeToString returns the base64 encoding of src.
[GoRecv] public static @string EncodeToString(this ref Encoding enc, slice<byte> src) {
    var buf = new slice<byte>(enc.EncodedLen(len(src)));
    enc.Encode(buf, src);
    return ((@string)buf);
}

[GoType] partial struct encoder {
    internal error err;
    internal ж<Encoding> enc;
    internal io.Writer w;
    internal array<byte> buf = new(3); // buffered data waiting to be encoded
    internal nint nbuf;       // number of bytes in buf
    internal array<byte> @out = new(1024); // output buffer
}

[GoRecv] internal static (nint n, error err) Write(this ref encoder e, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (e.err != default!) {
        return (0, e.err);
    }
    // Leading fringe.
    if (e.nbuf > 0) {
        nint i = default!;
        for (i = 0; i < len(p) && e.nbuf < 3; i++) {
            e.buf[e.nbuf] = p[i];
            e.nbuf++;
        }
        n += i;
        p = p.slice(i);
        if (e.nbuf < 3) {
            return (n, err);
        }
        e.enc.Encode(e.@out[..], e.buf[..]);
        {
            (_, e.err) = e.w.Write(e.@out[..4]); if (e.err != default!) {
                return (n, e.err);
            }
        }
        e.nbuf = 0;
    }
    // Large interior chunks.
    while (len(p) >= 3) {
        nint nn = len(e.@out) / 4 * 3;
        if (nn > len(p)) {
            nn = len(p);
            nn -= nn % 3;
        }
        e.enc.Encode(e.@out[..], p.slice(0, nn));
        {
            (_, e.err) = e.w.Write(e.@out.slice(0, nn / 3 * 4)); if (e.err != default!) {
                return (n, e.err);
            }
        }
        n += nn;
        p = p.slice(nn);
    }
    // Trailing fringe.
    copy(e.buf[..], p);
    e.nbuf = len(p);
    n += len(p);
    return (n, err);
}

// Close flushes any pending output from the encoder.
// It is an error to call Write after calling Close.
[GoRecv] internal static error Close(this ref encoder e) {
    // If there's anything left in the buffer, flush it out
    if (e.err == default! && e.nbuf > 0) {
        e.enc.Encode(e.@out[..], e.buf.slice(0, e.nbuf));
        (_, e.err) = e.w.Write(e.@out.slice(0, e.enc.EncodedLen(e.nbuf)));
        e.nbuf = 0;
    }
    return e.err;
}

// NewEncoder returns a new base64 stream encoder. Data written to
// the returned writer will be encoded using enc and then written to w.
// Base64 encodings operate in 4-byte blocks; when finished
// writing, the caller must Close the returned encoder to flush any
// partially written blocks.
public static io.WriteCloser NewEncoder(ж<Encoding> Ꮡenc, io.Writer w) {
    return new encoderжWriteCloser(Ꮡ(new encoder(enc: Ꮡenc, w: w)));
}

// EncodedLen returns the length in bytes of the base64 encoding
// of an input buffer of length n.
[GoRecv] public static nint EncodedLen(this ref Encoding enc, nint n) {
    if (enc.padChar == NoPadding) {
        return n / 3 * 4 + (n % 3 * 8 + 5) / 6; // minimum # chars at 6 bits per char
    }
    return (n + 2) / 3 * 4; // minimum # 4-char quanta, 3 bytes each
}

[GoType("num:int64")] partial struct CorruptInputError;

/*
 * Decoder
 */
public static @string Error(this CorruptInputError e) {
    return "illegal base64 data at input byte "u8 + strconv.FormatInt((int64)e, 10);
}

// decodeQuantum decodes up to 4 base64 bytes. The received parameters are
// the destination buffer dst, the source buffer src and an index in the
// source buffer si.
// It returns the number of bytes read from src, the number of bytes written
// to dst, and an error, if any.
[GoRecv] internal static (nint nsi, nint n, error err) decodeQuantum(this ref Encoding enc, slice<byte> dst, slice<byte> src, nint si) {
    error err = default!;

    // Decode quantum using the base64 alphabet
    array<byte> dbuf = new(4);
    nint dlen = 4;
    // Lift the nil check outside of the loop.
    _ = enc.decodeMap;
    for (nint j = 0; j < len(dbuf); j++) {
        if (len(src) == si) {
            switch (ᐧ) {
            case {} when j is 0: {
                return (si, 0, default!);
            }
            case {} when (j == 1) || (enc.padChar != NoPadding): {
                return (si, 0, ((CorruptInputError)(int64)(si - j)));
            }}

            dlen = j;
            break;
        }
        var @in = src[si];
        si++;
        var @out = enc.decodeMap[@in];
        if (@out != 0xff) {
            dbuf[j] = @out;
            continue;
        }
        if (@in == (rune)'\n' || @in == (rune)'\r') {
            j--;
            continue;
        }
        if ((rune)@in != enc.padChar) {
            return (si, 0, ((CorruptInputError)(int64)(si - 1)));
        }
        // We've reached the end and there's padding
        switch (j) {
        case 0 or 1: {
            return (si, 0, ((CorruptInputError)(int64)(si - 1)));
        }
        case 2: {
            while (si < len(src) && (src[si] == (rune)'\n' || src[si] == (rune)'\r')) {
                // incorrect padding
                // "==" is expected, the first "=" is already consumed.
                // skip over newlines
                si++;
            }
            if (si == len(src)) {
                // not enough padding
                return (si, 0, ((CorruptInputError)(int64)len(src)));
            }
            if ((rune)src[si] != enc.padChar) {
                // incorrect padding
                return (si, 0, ((CorruptInputError)(int64)(si - 1)));
            }
            si++;
            break;
        }}

        // skip over newlines
        while (si < len(src) && (src[si] == (rune)'\n' || src[si] == (rune)'\r')) {
            si++;
        }
        if (si < len(src)) {
            // trailing garbage
            err = ((CorruptInputError)(int64)si);
        }
        dlen = j;
        break;
    }
    // Convert 4x 6bit source bytes into 3 bytes
    nuint val = (nuint)((nuint)((nuint)(((nuint)dbuf[0] << (int)(18)) | ((nuint)dbuf[1] << (int)(12))) | ((nuint)dbuf[2] << (int)(6))) | (nuint)dbuf[3]);
    (dbuf[2], dbuf[1], dbuf[0]) = ((byte)((val >> (int)(0))), (byte)((val >> (int)(8))), (byte)((val >> (int)(16))));
    var exprᴛ1 = dlen;
    var matchᴛ1 = false;
    if (exprᴛ1 is 4) { matchᴛ1 = true;
        dst[2] = dbuf[2];
        dbuf[2] = 0;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is 3) {
        dst[1] = dbuf[1];
        if (enc.strict && dbuf[2] != 0) {
            return (si, 0, ((CorruptInputError)(int64)(si - 1)));
        }
        dbuf[1] = 0;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is 2) { matchᴛ1 = true;
        dst[0] = dbuf[0];
        if (enc.strict && (dbuf[1] != 0 || dbuf[2] != 0)) {
            return (si, 0, ((CorruptInputError)(int64)(si - 2)));
        }
    }

    return (si, dlen - 1, err);
}

// AppendDecode appends the base64 decoded src to dst
// and returns the extended buffer.
// If the input is malformed, it returns the partially decoded src and an error.
// New line characters (\r and \n) are ignored.
[GoRecv] public static (slice<byte>, error) AppendDecode(this ref Encoding enc, slice<byte> dst, slice<byte> src) {
    // Compute the output size without padding to avoid over allocating.
    nint n = len(src);
    while (n > 0 && (rune)src[n - 1] == enc.padChar) {
        n--;
    }
    n = decodedLen(n, NoPadding);
    dst = slices.Grow<slice<byte>, byte>(dst, n);
    (n, var err) = enc.Decode(dst.slice(len(dst)).slice(0, n), src);
    return (dst.slice(0, len(dst) + n), err);
}

// DecodeString returns the bytes represented by the base64 string s.
// If the input is malformed, it returns the partially decoded data and
// [CorruptInputError]. New line characters (\r and \n) are ignored.
[GoRecv] public static (slice<byte>, error) DecodeString(this ref Encoding enc, @string s) {
    var dbuf = new slice<byte>(enc.DecodedLen(len(s)));
    var (n, err) = enc.Decode(dbuf, slice<byte>(s));
    return (dbuf.slice(0, n), err);
}

[GoType] partial struct decoder {
    internal error err;
    internal error readErr; // error from r.Read
    internal ж<Encoding> enc;
    internal io.Reader r;
    internal array<byte> buf = new(1024); // leftover input
    internal nint nbuf;
    internal slice<byte> @out; // leftover decoded output
    internal array<byte> outbuf = new(1024 / 4 * 3);
}

[GoRecv] internal static (nint n, error err) Read(this ref decoder d, slice<byte> p) {
    nint n = default!;

    // Use leftover decoded output from last read.
    if (len(d.@out) > 0) {
        n = copy(p, d.@out);
        d.@out = d.@out.slice(n);
        return (n, default!);
    }
    if (d.err != default!) {
        return (0, d.err);
    }
    // This code assumes that d.r strips supported whitespace ('\r' and '\n').
    // Refill buffer.
    while (d.nbuf < 4 && d.readErr == default!) {
        nint nn = len(p) / 3 * 4;
        if (nn < 4) {
            nn = 4;
        }
        if (nn > len(d.buf)) {
            nn = len(d.buf);
        }
        (nn, d.readErr) = d.r.Read(d.buf.slice(d.nbuf, nn));
        d.nbuf += nn;
    }
    if (d.nbuf < 4) {
        if ((~d.enc).padChar == NoPadding && d.nbuf > 0) {
            // Decode final fragment, without padding.
            nint nwΔ1 = default!;
            (nwΔ1, d.err) = d.enc.Decode(d.outbuf[..], d.buf.slice(0, d.nbuf));
            d.nbuf = 0;
            d.@out = d.outbuf.slice(0, nwΔ1);
            n = copy(p, d.@out);
            d.@out = d.@out.slice(n);
            if (n > 0 || len(p) == 0 && len(d.@out) > 0) {
                return (n, default!);
            }
            if (d.err != default!) {
                return (0, d.err);
            }
        }
        d.err = d.readErr;
        if (AreEqual(d.err, io.EOF) && d.nbuf > 0) {
            d.err = io.ErrUnexpectedEOF;
        }
        return (0, d.err);
    }
    // Decode chunk into p, or d.out and then p if p is too small.
    nint nr = d.nbuf / 4 * 4;
    nint nw = d.nbuf / 4 * 3;
    if (nw > len(p)){
        (nw, d.err) = d.enc.Decode(d.outbuf[..], d.buf.slice(0, nr));
        d.@out = d.outbuf.slice(0, nw);
        n = copy(p, d.@out);
        d.@out = d.@out.slice(n);
    } else {
        (n, d.err) = d.enc.Decode(p, d.buf.slice(0, nr));
    }
    d.nbuf -= nr;
    copy(d.buf.slice(0, d.nbuf), d.buf.slice(nr));
    return (n, d.err);
}

// Decode decodes src using the encoding enc. It writes at most
// [Encoding.DecodedLen](len(src)) bytes to dst and returns the number of bytes
// written. The caller must ensure that dst is large enough to hold all
// the decoded data. If src contains invalid base64 data, it will return the
// number of bytes successfully written and [CorruptInputError].
// New line characters (\r and \n) are ignored.
[GoRecv] public static (nint n, error err) Decode(this ref Encoding enc, slice<byte> dst, slice<byte> src) {
    nint n = default!;
    error err = default!;

    if (len(src) == 0) {
        return (0, default!);
    }
    // Lift the nil check outside of the loop. enc.decodeMap is directly
    // used later in this function, to let the compiler know that the
    // receiver can't be nil.
    _ = enc.decodeMap;
    nint si = 0;
    while (strconv.IntSize >= 64 && len(src) - si >= 8 && len(dst) - n >= 8) {
        var src2 = src.slice(si, si + 8);
        {
            var (dn, ok) = assemble64(
                enc.decodeMap[src2[0]],
                enc.decodeMap[src2[1]],
                enc.decodeMap[src2[2]],
                enc.decodeMap[src2[3]],
                enc.decodeMap[src2[4]],
                enc.decodeMap[src2[5]],
                enc.decodeMap[src2[6]],
                enc.decodeMap[src2[7]]); if (ok){
                binary.BigEndian.PutUint64(dst.slice(n), dn);
                n += 6;
                si += 8;
            } else {
                nint ninc = default!;
                (si, ninc, err) = enc.decodeQuantum(dst.slice(n), src, si);
                n += ninc;
                if (err != default!) {
                    return (n, err);
                }
            }
        }
    }
    while (len(src) - si >= 4 && len(dst) - n >= 4) {
        var src2 = src.slice(si, si + 4);
        {
            var (dn, ok) = assemble32(
                enc.decodeMap[src2[0]],
                enc.decodeMap[src2[1]],
                enc.decodeMap[src2[2]],
                enc.decodeMap[src2[3]]); if (ok){
                binary.BigEndian.PutUint32(dst.slice(n), dn);
                n += 3;
                si += 4;
            } else {
                nint ninc = default!;
                (si, ninc, err) = enc.decodeQuantum(dst.slice(n), src, si);
                n += ninc;
                if (err != default!) {
                    return (n, err);
                }
            }
        }
    }
    while (si < len(src)) {
        nint ninc = default!;
        (si, ninc, err) = enc.decodeQuantum(dst.slice(n), src, si);
        n += ninc;
        if (err != default!) {
            return (n, err);
        }
    }
    return (n, err);
}

// assemble32 assembles 4 base64 digits into 3 bytes.
// Each digit comes from the decode map, and will be 0xff
// if it came from an invalid character.
internal static (uint32 dn, bool ok) assemble32(byte n1, byte n2, byte n3, byte n4) {
    // Check that all the digits are valid. If any of them was 0xff, their
    // bitwise OR will be 0xff.
    if ((byte)((byte)((byte)(n1 | n2) | n3) | n4) == 0xff) {
        return (0, false);
    }
    return ((uint32)((uint32)((uint32)(((uint32)n1 << (int)(26)) | ((uint32)n2 << (int)(20))) | ((uint32)n3 << (int)(14))) | ((uint32)n4 << (int)(8))), true);
}

// assemble64 assembles 8 base64 digits into 6 bytes.
// Each digit comes from the decode map, and will be 0xff
// if it came from an invalid character.
internal static (uint64 dn, bool ok) assemble64(byte n1, byte n2, byte n3, byte n4, byte n5, byte n6, byte n7, byte n8) {
    // Check that all the digits are valid. If any of them was 0xff, their
    // bitwise OR will be 0xff.
    if ((byte)((byte)((byte)((byte)((byte)((byte)((byte)(n1 | n2) | n3) | n4) | n5) | n6) | n7) | n8) == 0xff) {
        return (0, false);
    }
    return ((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)((uint64)(((uint64)n1 << (int)(58)) | ((uint64)n2 << (int)(52))) | ((uint64)n3 << (int)(46))) | ((uint64)n4 << (int)(40))) | ((uint64)n5 << (int)(34))) | ((uint64)n6 << (int)(28))) | ((uint64)n7 << (int)(22))) | ((uint64)n8 << (int)(16))), true);
}

[GoType] partial struct newlineFilteringReader {
    internal io.Reader wrapped;
}

[GoRecv] internal static (nint, error) Read(this ref newlineFilteringReader r, slice<byte> p) {
    var (n, err) = r.wrapped.Read(p);
    while (n > 0) {
        nint offset = 0;
        foreach (var (i, b) in p.slice(0, n)) {
            if (b != (rune)'\r' && b != (rune)'\n') {
                if (i != offset) {
                    p[offset] = b;
                }
                offset++;
            }
        }
        if (offset > 0) {
            return (offset, err);
        }
        // Previous buffer entirely whitespace, read again
        (n, err) = r.wrapped.Read(p);
    }
    return (n, err);
}

// NewDecoder constructs a new base64 stream decoder.
public static io.Reader NewDecoder(ж<Encoding> Ꮡenc, io.Reader r) {
    return new decoderжReader(Ꮡ(new decoder(enc: Ꮡenc, r: new newlineFilteringReaderжReader(Ꮡ(new newlineFilteringReader(r))))));
}

// DecodedLen returns the maximum length in bytes of the decoded data
// corresponding to n bytes of base64-encoded data.
[GoRecv] public static nint DecodedLen(this ref Encoding enc, nint n) {
    return decodedLen(n, enc.padChar);
}

internal static nint decodedLen(nint n, rune padChar) {
    if (padChar == NoPadding) {
        // Unpadded data may end with partial block of 2-3 characters.
        return n / 4 * 3 + n % 4 * 6 / 8;
    }
    // Padded base64 should always be a multiple of 4 characters in length.
    return n / 4 * 3;
}

} // end base64_package
