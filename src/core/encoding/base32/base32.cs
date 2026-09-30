// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package base32 implements base32 encoding as specified by RFC 4648.
namespace go.encoding;

using io = io_package;
using slices = slices_package;
using strconv = strconv_package;

partial class base32_package {

/*
 * Encodings
 */

// An Encoding is a radix 32 encoding/decoding scheme, defined by a
// 32-character alphabet. The most common is the "base32" encoding
// introduced for SASL GSSAPI and standardized in RFC 4648.
// The alternate "base32hex" encoding is used in DNSSEC.
[GoType] partial struct Encoding {
    internal array<byte> encode = new(32); // mapping of symbol index to symbol byte value
    internal array<uint8> decodeMap = new(256); // mapping of symbol byte value to symbol index
    internal rune padChar;
}

public const rune StdPadding = /* '=' */ 61; // Standard padding character
public const rune NoPadding = -1;  // No padding

internal static readonly @string decodeMapInitialize = ((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}));
internal static UntypedInt invalidIndex => /* '\xff' */ 255;

// NewEncoding returns a new padded Encoding defined by the given alphabet,
// which must be a 32-byte string that contains unique byte values and
// does not contain the padding character or CR / LF ('\r', '\n').
// The alphabet is treated as a sequence of byte values
// without any special treatment for multi-byte UTF-8.
// The resulting Encoding uses the default padding character ('='),
// which may be changed or disabled via [Encoding.WithPadding].
public static ж<Encoding> NewEncoding(@string encoder) {
    if (len(encoder) != 32) {
        throw panic("encoding alphabet is not 32-bytes long");
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

// StdEncoding is the standard base32 encoding, as defined in RFC 4648.
public static ж<Encoding> StdEncoding = NewEncoding("ABCDEFGHIJKLMNOPQRSTUVWXYZ234567"u8);

// HexEncoding is the “Extended Hex Alphabet” defined in RFC 4648.
// It is typically used in DNS.
public static ж<Encoding> HexEncoding = NewEncoding("0123456789ABCDEFGHIJKLMNOPQRSTUV"u8);

// WithPadding creates a new encoding identical to enc except
// with a specified padding character, or NoPadding to disable padding.
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

/*
 * Encoder
 */

// Encode encodes src using the encoding enc,
// writing [Encoding.EncodedLen](len(src)) bytes to dst.
//
// The encoding pads the output to a multiple of 8 bytes,
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
    nint n = (len(src) / 5) * 5;
    while (si < n) {
        // Combining two 32 bit loads allows the same code to be used
        // for 32 and 64 bit platforms.
        var hi = (uint32)((uint32)((uint32)(((uint32)src[si + 0] << (int)(24)) | ((uint32)src[si + 1] << (int)(16))) | ((uint32)src[si + 2] << (int)(8))) | (uint32)src[si + 3]);
        var lo = (uint32)((hi << (int)(8)) | (uint32)src[si + 4]);
        dst[di + 0] = enc.encode[(uint32)(((hi >> (int)(27))) & 0x1F)];
        dst[di + 1] = enc.encode[(uint32)(((hi >> (int)(22))) & 0x1F)];
        dst[di + 2] = enc.encode[(uint32)(((hi >> (int)(17))) & 0x1F)];
        dst[di + 3] = enc.encode[(uint32)(((hi >> (int)(12))) & 0x1F)];
        dst[di + 4] = enc.encode[(uint32)(((hi >> (int)(7))) & 0x1F)];
        dst[di + 5] = enc.encode[(uint32)(((hi >> (int)(2))) & 0x1F)];
        dst[di + 6] = enc.encode[(uint32)(((lo >> (int)(5))) & 0x1F)];
        dst[di + 7] = enc.encode[(uint32)((lo) & 0x1F)];
        si += 5;
        di += 8;
    }
    // Add the remaining small block
    nint remain = len(src) - si;
    if (remain == 0) {
        return;
    }
    // Encode the remaining bytes in reverse order.
    var val = (uint32)0;
    var exprᴛ1 = remain;
    var matchᴛ1 = false;
    if (exprᴛ1 is 4) { matchᴛ1 = true;
        val |= (uint32)((uint32)src[si + 3]);
        dst[di + 6] = enc.encode[(uint32)((val << (int)(3)) & 0x1F)];
        dst[di + 5] = enc.encode[(uint32)((val >> (int)(2)) & 0x1F)];
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is 3) { matchᴛ1 = true;
        val |= (uint32)(((uint32)src[si + 2] << (int)(8)));
        dst[di + 4] = enc.encode[(uint32)((val >> (int)(7)) & 0x1F)];
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is 2) {
        val |= (uint32)(((uint32)src[si + 1] << (int)(16)));
        dst[di + 3] = enc.encode[(uint32)((val >> (int)(12)) & 0x1F)];
        dst[di + 2] = enc.encode[(uint32)((val >> (int)(17)) & 0x1F)];
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is 1) { matchᴛ1 = true;
        val |= (uint32)(((uint32)src[si + 0] << (int)(24)));
        dst[di + 1] = enc.encode[(uint32)((val >> (int)(22)) & 0x1F)];
        dst[di + 0] = enc.encode[(uint32)((val >> (int)(27)) & 0x1F)];
    }

    // Pad the final quantum
    if (enc.padChar != NoPadding) {
        nint nPad = (remain * 8 / 5) + 1;
        for (nint i = nPad; i < 8; i++) {
            dst[di + i] = (byte)enc.padChar;
        }
    }
}

// AppendEncode appends the base32 encoded src to dst
// and returns the extended buffer.
[GoRecv] public static slice<byte> AppendEncode(this ref Encoding enc, slice<byte> dst, slice<byte> src) {
    nint n = enc.EncodedLen(len(src));
    dst = slices.Grow<slice<byte>, byte>(dst, n);
    enc.Encode(dst.slice(len(dst)).slice(0, n), src);
    return dst.slice(0, len(dst) + n);
}

// EncodeToString returns the base32 encoding of src.
[GoRecv] public static @string EncodeToString(this ref Encoding enc, slice<byte> src) {
    var buf = new slice<byte>(enc.EncodedLen(len(src)));
    enc.Encode(buf, src);
    return ((@string)buf);
}

[GoType] partial struct encoder {
    internal error err;
    internal ж<Encoding> enc;
    internal io.Writer w;
    internal array<byte> buf = new(5); // buffered data waiting to be encoded
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
        for (i = 0; i < len(p) && e.nbuf < 5; i++) {
            e.buf[e.nbuf] = p[i];
            e.nbuf++;
        }
        n += i;
        p = p.slice(i);
        if (e.nbuf < 5) {
            return (n, err);
        }
        e.enc.Encode(e.@out[0..], e.buf[0..]);
        {
            (_, e.err) = e.w.Write(e.@out[0..8]); if (e.err != default!) {
                return (n, e.err);
            }
        }
        e.nbuf = 0;
    }
    // Large interior chunks.
    while (len(p) >= 5) {
        nint nn = len(e.@out) / 8 * 5;
        if (nn > len(p)) {
            nn = len(p);
            nn -= nn % 5;
        }
        e.enc.Encode(e.@out[0..], p.slice(0, nn));
        {
            (_, e.err) = e.w.Write(e.@out.slice(0, nn / 5 * 8)); if (e.err != default!) {
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
        e.enc.Encode(e.@out[0..], e.buf.slice(0, e.nbuf));
        nint encodedLen = e.enc.EncodedLen(e.nbuf);
        e.nbuf = 0;
        (_, e.err) = e.w.Write(e.@out.slice(0, encodedLen));
    }
    return e.err;
}

// NewEncoder returns a new base32 stream encoder. Data written to
// the returned writer will be encoded using enc and then written to w.
// Base32 encodings operate in 5-byte blocks; when finished
// writing, the caller must Close the returned encoder to flush any
// partially written blocks.
public static io.WriteCloser NewEncoder(ж<Encoding> Ꮡenc, io.Writer w) {
    return new encoderжWriteCloser(Ꮡ(new encoder(enc: Ꮡenc, w: w)));
}

// EncodedLen returns the length in bytes of the base32 encoding
// of an input buffer of length n.
[GoRecv] public static nint EncodedLen(this ref Encoding enc, nint n) {
    if (enc.padChar == NoPadding) {
        return n / 5 * 8 + (n % 5 * 8 + 4) / 5;
    }
    return (n + 4) / 5 * 8;
}

[GoType("num:int64")] partial struct CorruptInputError;

/*
 * Decoder
 */
public static @string Error(this CorruptInputError e) {
    return "illegal base32 data at input byte "u8 + strconv.FormatInt((int64)e, 10);
}

// decode is like Decode but returns an additional 'end' value, which
// indicates if end-of-message padding was encountered and thus any
// additional data is an error. This method assumes that src has been
// stripped of all supported whitespace ('\r' and '\n').
[GoRecv] internal static (nint n, bool end, error err) decode(this ref Encoding enc, slice<byte> dst, slice<byte> src) {
    nint n = default!;
    bool end = default!;

    // Lift the nil check outside of the loop.
    _ = enc.decodeMap;
    nint dsti = 0;
    nint olen = len(src);
    while (len(src) > 0 && !end) {
        // Decode quantum using the base32 alphabet
        array<byte> dbuf = new(8);
        nint dlen = 8;
        for (nint j = 0; j < 8; ) {
            if (len(src) == 0) {
                if (enc.padChar != NoPadding) {
                    // We have reached the end and are missing padding
                    return (n, false, ((CorruptInputError)(int64)(olen - len(src) - j)));
                }
                // We have reached the end and are not expecting any padding
                (dlen, end) = (j, true);
                break;
            }
            var @in = src[0];
            src = src[1..];
            if (@in == (byte)enc.padChar && j >= 2 && len(src) < 8) {
                // We've reached the end and there's padding
                if (len(src) + j < 8 - 1) {
                    // not enough padding
                    return (n, false, ((CorruptInputError)(int64)olen));
                }
                for (nint k = 0; k < 8 - 1 - j; k++) {
                    if (len(src) > k && src[k] != (byte)enc.padChar) {
                        // incorrect padding
                        return (n, false, ((CorruptInputError)(int64)(olen - len(src) + k - 1)));
                    }
                }
                (dlen, end) = (j, true);
                // 7, 5 and 2 are not valid padding lengths, and so 1, 3 and 6 are not
                // valid dlen values. See RFC 4648 Section 6 "Base 32 Encoding" listing
                // the five valid padding lengths, and Section 9 "Illustrations and
                // Examples" for an illustration for how the 1st, 3rd and 6th base32
                // src bytes do not yield enough information to decode a dst byte.
                if (dlen == 1 || dlen == 3 || dlen == 6) {
                    return (n, false, ((CorruptInputError)(int64)(olen - len(src) - 1)));
                }
                break;
            }
            dbuf[j] = enc.decodeMap[@in];
            if (dbuf[j] == 0xFF) {
                return (n, false, ((CorruptInputError)(int64)(olen - len(src) - 1)));
            }
            j++;
        }
        // Pack 8x 5-bit source blocks into 5 byte destination
        // quantum
        var exprᴛ1 = dlen;
        var matchᴛ1 = false;
        if (exprᴛ1 is 8) { matchᴛ1 = true;
            dst[dsti + 4] = (byte)((byte)(dbuf[6] << (int)(5)) | dbuf[7]);
            n++;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 7) { matchᴛ1 = true;
            dst[dsti + 3] = (byte)((byte)((byte)(dbuf[4] << (int)(7)) | (byte)(dbuf[5] << (int)(2))) | (dbuf[6] >> (int)(3)));
            n++;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 5) { matchᴛ1 = true;
            dst[dsti + 2] = (byte)((byte)(dbuf[3] << (int)(4)) | (dbuf[4] >> (int)(1)));
            n++;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 4) {
            dst[dsti + 1] = (byte)((byte)((byte)(dbuf[1] << (int)(6)) | (byte)(dbuf[2] << (int)(1))) | (dbuf[3] >> (int)(4)));
            n++;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 2) { matchᴛ1 = true;
            dst[dsti + 0] = (byte)((byte)(dbuf[0] << (int)(3)) | (dbuf[1] >> (int)(2)));
            n++;
        }

        dsti += 5;
    }
    return (n, end, default!);
}

// Decode decodes src using the encoding enc. It writes at most
// [Encoding.DecodedLen](len(src)) bytes to dst and returns the number of bytes
// written. The caller must ensure that dst is large enough to hold all
// the decoded data. If src contains invalid base32 data, it will return the
// number of bytes successfully written and [CorruptInputError].
// Newline characters (\r and \n) are ignored.
[GoRecv] public static (nint n, error err) Decode(this ref Encoding enc, slice<byte> dst, slice<byte> src) {
    nint n = default!;
    error err = default!;

    var buf = new slice<byte>(len(src));
    nint l = stripNewlines(buf, src);
    (n, _, err) = enc.decode(dst, buf.slice(0, l));
    return (n, err);
}

// AppendDecode appends the base32 decoded src to dst
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

// DecodeString returns the bytes represented by the base32 string s.
// If the input is malformed, it returns the partially decoded data and
// [CorruptInputError]. New line characters (\r and \n) are ignored.
[GoRecv] public static (slice<byte>, error) DecodeString(this ref Encoding enc, @string s) {
    var buf = slice<byte>(s);
    nint l = stripNewlines(buf, buf);
    var (n, _, err) = enc.decode(buf, buf.slice(0, l));
    return (buf.slice(0, n), err);
}

[GoType] partial struct decoder {
    internal error err;
    internal ж<Encoding> enc;
    internal io.Reader r;
    internal bool end;       // saw end of message
    internal array<byte> buf = new(1024); // leftover input
    internal nint nbuf;
    internal slice<byte> @out; // leftover decoded output
    internal array<byte> outbuf = new(1024 / 8 * 5);
}

internal static (nint n, error err) readEncodedData(io.Reader r, slice<byte> buf, nint min, bool expectsPadding) {
    nint n = default!;
    error err = default!;

    while (n < min && err == default!) {
        nint nn = default!;
        (nn, err) = r.Read(buf.slice(n));
        n += nn;
    }
    // data was read, less than min bytes could be read
    if (n < min && n > 0 && AreEqual(err, io.EOF)) {
        err = io.ErrUnexpectedEOF;
    }
    // no data was read, the buffer already contains some data
    // when padding is disabled this is not an error, as the message can be of
    // any length
    if (expectsPadding && min < 8 && n == 0 && AreEqual(err, io.EOF)) {
        err = io.ErrUnexpectedEOF;
    }
    return (n, err);
}

[GoRecv] internal static (nint n, error err) Read(this ref decoder d, slice<byte> p) {
    nint n = default!;
    error err = default!;

    // Use leftover decoded output from last read.
    if (len(d.@out) > 0) {
        n = copy(p, d.@out);
        d.@out = d.@out.slice(n);
        if (len(d.@out) == 0) {
            return (n, d.err);
        }
        return (n, default!);
    }
    if (d.err != default!) {
        return (0, d.err);
    }
    // Read a chunk.
    nint nn = (len(p) + 4) / 5 * 8;
    if (nn < 8) {
        nn = 8;
    }
    if (nn > len(d.buf)) {
        nn = len(d.buf);
    }
    // Minimum amount of bytes that needs to be read each cycle
    nint min = default!;
    bool expectsPadding = default!;
    if ((~d.enc).padChar == NoPadding){
        min = 1;
        expectsPadding = false;
    } else {
        min = 8 - d.nbuf;
        expectsPadding = true;
    }
    (nn, d.err) = readEncodedData(d.r, d.buf.slice(d.nbuf, nn), min, expectsPadding);
    d.nbuf += nn;
    if (d.nbuf < min) {
        return (0, d.err);
    }
    if (nn > 0 && d.end) {
        return (0, ((CorruptInputError)0));
    }
    // Decode chunk into p, or d.out and then p if p is too small.
    nint nr = default!;
    if ((~d.enc).padChar == NoPadding){
        nr = d.nbuf;
    } else {
        nr = d.nbuf / 8 * 8;
    }
    nint nw = d.enc.DecodedLen(d.nbuf);
    if (nw > len(p)){
        (nw, d.end, err) = d.enc.decode(d.outbuf[0..], d.buf.slice(0, nr));
        d.@out = d.outbuf.slice(0, nw);
        n = copy(p, d.@out);
        d.@out = d.@out.slice(n);
    } else {
        (n, d.end, err) = d.enc.decode(p, d.buf.slice(0, nr));
    }
    d.nbuf -= nr;
    for (nint i = 0; i < d.nbuf; i++) {
        d.buf[i] = d.buf[i + nr];
    }
    if (err != default! && (d.err == default! || AreEqual(d.err, io.EOF))) {
        d.err = err;
    }
    if (len(d.@out) > 0) {
        // We cannot return all the decoded bytes to the caller in this
        // invocation of Read, so we return a nil error to ensure that Read
        // will be called again.  The error stored in d.err, if any, will be
        // returned with the last set of decoded bytes.
        return (n, default!);
    }
    return (n, d.err);
}

[GoType] partial struct newlineFilteringReader {
    internal io.Reader wrapped;
}

// stripNewlines removes newline characters and returns the number
// of non-newline characters copied to dst.
internal static nint stripNewlines(slice<byte> dst, slice<byte> src) {
    nint offset = 0;
    foreach (var (_, b) in src) {
        if (b == (rune)'\r' || b == (rune)'\n') {
            continue;
        }
        dst[offset] = b;
        offset++;
    }
    return offset;
}

[GoRecv] internal static (nint, error) Read(this ref newlineFilteringReader r, slice<byte> p) {
    var (n, err) = r.wrapped.Read(p);
    while (n > 0) {
        var s = p.slice(0, n);
        nint offset = stripNewlines(s, s);
        if (err != default! || offset > 0) {
            return (offset, err);
        }
        // Previous buffer entirely whitespace, read again
        (n, err) = r.wrapped.Read(p);
    }
    return (n, err);
}

// NewDecoder constructs a new base32 stream decoder.
public static io.Reader NewDecoder(ж<Encoding> Ꮡenc, io.Reader r) {
    return new decoderжReader(Ꮡ(new decoder(enc: Ꮡenc, r: new newlineFilteringReaderжReader(Ꮡ(new newlineFilteringReader(r))))));
}

// DecodedLen returns the maximum length in bytes of the decoded data
// corresponding to n bytes of base32-encoded data.
[GoRecv] public static nint DecodedLen(this ref Encoding enc, nint n) {
    return decodedLen(n, enc.padChar);
}

internal static nint decodedLen(nint n, rune padChar) {
    if (padChar == NoPadding) {
        return n / 8 * 5 + n % 8 * 5 / 8;
    }
    return n / 8 * 5;
}

} // end base32_package
