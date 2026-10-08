// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package ascii85 implements the ascii85 data encoding
// as used in the btoa tool and Adobe's PostScript and PDF document formats.
namespace go.encoding;

using io = io_package;
using strconv = strconv_package;

partial class ascii85_package {

/*
 * Encoder
 */

// Encode encodes src into at most [MaxEncodedLen](len(src))
// bytes of dst, returning the actual number of bytes written.
//
// The encoding handles 4-byte chunks, using a special encoding
// for the last fragment, so Encode is not appropriate for use on
// individual blocks of a large data stream. Use [NewEncoder] instead.
//
// Often, ascii85-encoded data is wrapped in <~ and ~> symbols.
// Encode does not add these.
public static nint Encode(slice<byte> dst, slice<byte> src) {
    if (len(src) == 0) {
        return 0;
    }
    nint n = 0;
    while (len(src) > 0) {
        dst[0] = 0;
        dst[1] = 0;
        dst[2] = 0;
        dst[3] = 0;
        dst[4] = 0;
        // Unpack 4 bytes into uint32 to repack into base 85 5-byte.
        uint32 v = default!;
        var exprᴛ1 = len(src);
        var matchᴛ1 = false;
        var matchᴛ2 = exprᴛ1 is 3 || exprᴛ1 is 2 || exprᴛ1 is 1;
        if (!matchᴛ2) { /* default: */
            v |= (uint32)((uint32)src[3]);
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 3) { matchᴛ1 = true;
            v |= (uint32)(((uint32)src[2] << (int)(8)));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 2) {
            v |= (uint32)(((uint32)src[1] << (int)(16)));
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1 && exprᴛ1 is 1) { matchᴛ1 = true;
            v |= (uint32)(((uint32)src[0] << (int)(24)));
        }

        // Special case: zero (!!!!!) shortens to z.
        if (v == 0 && len(src) >= 4) {
            dst[0] = (rune)'z';
            dst = dst[1..];
            src = src[4..];
            n++;
            continue;
        }
        // Otherwise, 5 base 85 digits starting at !.
        for (nint i = 4; i >= 0; i--) {
            dst[i] = (byte)((rune)'!' + (byte)(v % 85));
            v /= 85;
        }
        // If src was short, discard the low destination bytes.
        nint m = 5;
        if (len(src) < 4){
            m -= 4 - len(src);
            src = default!;
        } else {
            src = src[4..];
        }
        dst = dst.slice(m);
        n += m;
    }
    return n;
}

// MaxEncodedLen returns the maximum length of an encoding of n source bytes.
public static nint MaxEncodedLen(nint n) {
    return (n + 3) / 4 * 5;
}

// NewEncoder returns a new ascii85 stream encoder. Data written to
// the returned writer will be encoded and then written to w.
// Ascii85 encodings operate in 32-bit blocks; when finished
// writing, the caller must Close the returned encoder to flush any
// trailing partial block.
public static io.WriteCloser NewEncoder(io.Writer w) {
    return new encoderжWriteCloser(Ꮡ(new encoder(w: w)));
}

partial struct encoder {
    internal error err;
    internal io.Writer w;
    internal array<byte> buf = new(4); // buffered data waiting to be encoded
    internal nint nbuf;       // number of bytes in buf
    internal array<byte> @out = new(1024); // output buffer
}

internal static (nint n, error err) Write(this ref encoder e, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (e.err != default!) {
        return (0, e.err);
    }
    // Leading fringe.
    if (e.nbuf > 0) {
        nint i = default!;
        for (i = 0; i < len(p) && e.nbuf < 4; i++) {
            e.buf[e.nbuf] = p[i];
            e.nbuf++;
        }
        n += i;
        p = p.slice(i);
        if (e.nbuf < 4) {
            return (n, err);
        }
        nint nout = Encode(e.@out[0..], e.buf[0..]);
        {
            (_, e.err) = e.w.Write(e.@out.slice(0, nout)); if (e.err != default!) {
                return (n, e.err);
            }
        }
        e.nbuf = 0;
    }
    // Large interior chunks.
    while (len(p) >= 4) {
        nint nn = len(e.@out) / 5 * 4;
        if (nn > len(p)) {
            nn = len(p);
        }
        nn -= nn % 4;
        if (nn > 0) {
            nint nout = Encode(e.@out[0..], p.slice(0, nn));
            {
                (_, e.err) = e.w.Write(e.@out.slice(0, nout)); if (e.err != default!) {
                    return (n, e.err);
                }
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
internal static error Close(this ref encoder e) {
    // If there's anything left in the buffer, flush it out
    if (e.err == default! && e.nbuf > 0) {
        nint nout = Encode(e.@out[0..], e.buf.slice(0, e.nbuf));
        e.nbuf = 0;
        (_, e.err) = e.w.Write(e.@out.slice(0, nout));
    }
    return e.err;
}

partial struct CorruptInputError /*num:int64*/;

/*
 * Decoder
 */
public static @string Error(this CorruptInputError e) {
    return "illegal ascii85 data at input byte "u8 + strconv.FormatInt((int64)e, 10);
}

// Decode decodes src into dst, returning both the number
// of bytes written to dst and the number consumed from src.
// If src contains invalid ascii85 data, Decode will return the
// number of bytes successfully written and a [CorruptInputError].
// Decode ignores space and control characters in src.
// Often, ascii85-encoded data is wrapped in <~ and ~> symbols.
// Decode expects these to have been stripped by the caller.
//
// If flush is true, Decode assumes that src represents the
// end of the input stream and processes it completely rather
// than wait for the completion of another 32-bit block.
//
// [NewDecoder] wraps an [io.Reader] interface around Decode.
public static (nint ndst, nint nsrc, error err) Decode(slice<byte> dst, slice<byte> src, bool flush) {
    nint ndst = default!;
    nint nsrc = default!;
    error err = default!;

    uint32 v = default!;
    nint nb = default!;
    foreach (var (i, b) in src) {
        if (len(dst) - ndst < 4) {
            return (ndst, nsrc, err);
        }
        switch (ᐧ) {
        case {} when b is <= (rune)' ': {
            continue;
            break;
        }
        case {} when b == (rune)'z' && nb == 0: {
            nb = 5;
            v = 0;
            break;
        }
        case {} when (rune)'!' <= b && b <= (rune)'u': {
            v = v * 85 + (uint32)((byte)(b - (rune)'!'));
            nb++;
            break;
        }
        default: {
            return (0, 0, ((CorruptInputError)(int64)i));
        }}

        if (nb == 5) {
            nsrc = i + 1;
            dst[ndst] = (byte)((v >> (int)(24)));
            dst[ndst + 1] = (byte)((v >> (int)(16)));
            dst[ndst + 2] = (byte)((v >> (int)(8)));
            dst[ndst + 3] = (byte)v;
            ndst += 4;
            nb = 0;
            v = 0;
        }
    }
    if (flush) {
        nsrc = len(src);
        if (nb > 0) {
            // The number of output bytes in the last fragment
            // is the number of leftover input bytes - 1:
            // the extra byte provides enough bits to cover
            // the inefficiency of the encoding for the block.
            if (nb == 1) {
                return (0, 0, ((CorruptInputError)(int64)len(src)));
            }
            for (nint i = nb; i < 5; i++) {
                // The short encoding truncated the output value.
                // We have to assume the worst case values (digit 84)
                // in order to ensure that the top bits are correct.
                v = v * 85 + 84;
            }
            for (nint i = 0; i < nb - 1; i++) {
                dst[ndst] = (byte)((v >> (int)(24)));
                v <<= (int)(8);
                ndst++;
            }
        }
    }
    return (ndst, nsrc, err);
}

// NewDecoder constructs a new ascii85 stream decoder.
public static io.Reader NewDecoder(io.Reader r) {
    return new decoderжReader(Ꮡ(new decoder(r: r)));
}

partial struct decoder {
    internal error err;
    internal error readErr;
    internal io.Reader r;
    internal array<byte> buf = new(1024); // leftover input
    internal nint nbuf;
    internal slice<byte> @out; // leftover decoded output
    internal array<byte> outbuf = new(1024);
}

internal static (nint n, error err) Read(this ref decoder d, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (len(p) == 0) {
        return (0, default!);
    }
    if (d.err != default!) {
        return (0, d.err);
    }
    while (ᐧ) {
        // Copy leftover output from last decode.
        if (len(d.@out) > 0) {
            n = copy(p, d.@out);
            d.@out = d.@out.slice(n);
            return (n, err);
        }
        // Decode leftover input from last read.
        nint nn = default!;
        nint nsrc = default!;
        nint ndst = default!;
        if (d.nbuf > 0) {
            (ndst, nsrc, d.err) = Decode(d.outbuf[0..], d.buf.slice(0, d.nbuf), d.readErr != default!);
            if (ndst > 0) {
                d.@out = d.outbuf.slice(0, ndst);
                d.nbuf = copy(d.buf[0..], d.buf.slice(nsrc, d.nbuf));
                continue; // copy out and return
            }
            if (ndst == 0 && d.err == default!) {
                // Special case: input buffer is mostly filled with non-data bytes.
                // Filter out such bytes to make room for more input.
                nint off = 0;
                for (nint i = 0; i < d.nbuf; i++) {
                    if (d.buf[i] > (rune)' ') {
                        d.buf[off] = d.buf[i];
                        off++;
                    }
                }
                d.nbuf = off;
            }
        }
        // Out of input, out of decoded output. Check errors.
        if (d.err != default!) {
            return (0, d.err);
        }
        if (d.readErr != default!) {
            d.err = d.readErr;
            return (0, d.err);
        }
        // Read more data.
        (nn, d.readErr) = d.r.Read(d.buf.slice(d.nbuf));
        d.nbuf += nn;
    }
}

} // end ascii85_package
