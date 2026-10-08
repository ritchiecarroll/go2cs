// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package hex implements hexadecimal encoding and decoding.
namespace go.encoding;

using errors = errors_package;
using fmt = fmt_package;
using io = io_package;
using slices = slices_package;
using strings = strings_package;

partial class hex_package {

internal static readonly @string hextable = "0123456789abcdef"u8;
internal static readonly @string reverseHexTable = ((@string)(new byte[]{0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff}));

// EncodedLen returns the length of an encoding of n source bytes.
// Specifically, it returns n * 2.
public static nint EncodedLen(nint n) {
    return n * 2;
}

// Encode encodes src into [EncodedLen](len(src))
// bytes of dst. As a convenience, it returns the number
// of bytes written to dst, but this value is always [EncodedLen](len(src)).
// Encode implements hexadecimal encoding.
public static nint Encode(slice<byte> dst, slice<byte> src) {
    nint j = 0;
    foreach (var (_, v) in src) {
        dst[j] = hextable[(v >> (int)(4))];
        dst[j + 1] = hextable[(byte)(v & 0x0f)];
        j += 2;
    }
    return len(src) * 2;
}

// AppendEncode appends the hexadecimally encoded src to dst
// and returns the extended buffer.
public static slice<byte> AppendEncode(slice<byte> dst, slice<byte> src) {
    nint n = EncodedLen(len(src));
    dst = slices.Grow<slice<byte>, byte>(dst, n);
    Encode(dst.slice(len(dst)).slice(0, n), src);
    return dst.slice(0, len(dst) + n);
}

// ErrLength reports an attempt to decode an odd-length input
// using [Decode] or [DecodeString].
// The stream-based Decoder returns [io.ErrUnexpectedEOF] instead of ErrLength.
public static error ErrLength = errors.New("encoding/hex: odd length hex string"u8);

partial struct InvalidByteError /*num:byte*/;

public static @string Error(this InvalidByteError e) {
    return fmt.Sprintf("encoding/hex: invalid byte: %#U"u8, (rune)(byte)e);
}

// DecodedLen returns the length of a decoding of x source bytes.
// Specifically, it returns x / 2.
public static nint DecodedLen(nint x) {
    return x / 2;
}

// Decode decodes src into [DecodedLen](len(src)) bytes,
// returning the actual number of bytes written to dst.
//
// Decode expects that src contains only hexadecimal
// characters and that src has even length.
// If the input is malformed, Decode returns the number
// of bytes decoded before the error.
public static (nint, error) Decode(slice<byte> dst, slice<byte> src) {
    nint i = 0;
    nint j = 1;
    for (; j < len(src); j += 2) {
        var p = src[j - 1];
        var q = src[j];
        var a = reverseHexTable[p];
        var b = reverseHexTable[q];
        if (a > 0x0f) {
            return (i, ((InvalidByteError)p));
        }
        if (b > 0x0f) {
            return (i, ((InvalidByteError)q));
        }
        dst[i] = (byte)(((byte)(a << (int)(4))) | b);
        i++;
    }
    if (len(src) % 2 == 1) {
        // Check for invalid char before reporting bad length,
        // since the invalid char (if present) is an earlier problem.
        if (reverseHexTable[src[j - 1]] > 0x0f) {
            return (i, ((InvalidByteError)src[j - 1]));
        }
        return (i, ErrLength);
    }
    return (i, default!);
}

// AppendDecode appends the hexadecimally decoded src to dst
// and returns the extended buffer.
// If the input is malformed, it returns the partially decoded src and an error.
public static (slice<byte>, error) AppendDecode(slice<byte> dst, slice<byte> src) {
    nint n = DecodedLen(len(src));
    dst = slices.Grow<slice<byte>, byte>(dst, n);
    (n, var err) = Decode(dst.slice(len(dst)).slice(0, n), src);
    return (dst.slice(0, len(dst) + n), err);
}

// EncodeToString returns the hexadecimal encoding of src.
public static @string EncodeToString(slice<byte> src) {
    var dst = new slice<byte>(EncodedLen(len(src)));
    Encode(dst, src);
    return ((@string)dst);
}

// DecodeString returns the bytes represented by the hexadecimal string s.
//
// DecodeString expects that src contains only hexadecimal
// characters and that src has even length.
// If the input is malformed, DecodeString returns
// the bytes decoded before the error.
public static (slice<byte>, error) DecodeString(@string s) {
    var dst = new slice<byte>(DecodedLen(len(s)));
    var (n, err) = Decode(dst, slice<byte>(s));
    return (dst.slice(0, n), err);
}

// Dump returns a string that contains a hex dump of the given data. The format
// of the hex dump matches the output of `hexdump -C` on the command line.
public static @string Dump(slice<byte> data) {
    if (len(data) == 0) {
        return ""u8;
    }
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    // Dumper will write 79 bytes per complete 16 byte chunk, and at least
    // 64 bytes for whatever remains. Round the allocation up, since only a
    // maximum of 15 bytes will be wasted.
    Ꮡbuf.Grow((1 + ((len(data) - 1) / 16)) * 79);
    var dumper = Dumper(new strings_BuilderжWriter(Ꮡbuf));
    dumper.Write(data);
    dumper.Close();
    return buf.String();
}

// bufferSize is the number of hexadecimal characters to buffer in encoder and decoder.
internal static UntypedInt bufferSize => 1024;

partial struct encoder {
    internal io.Writer w;
    internal error err;
    internal array<byte> @out = new(bufferSize); // output buffer
}

// NewEncoder returns an [io.Writer] that writes lowercase hexadecimal characters to w.
public static io.Writer NewEncoder(io.Writer w) {
    return new encoderжWriter(Ꮡ(new encoder(w: w)));
}

internal static (nint n, error err) Write(this ref encoder e, slice<byte> p) {
    nint n = default!;

    while (len(p) > 0 && e.err == default!) {
        nint chunkSize = bufferSize / 2;
        if (len(p) < chunkSize) {
            chunkSize = len(p);
        }
        nint written = default!;
        nint encoded = Encode(e.@out[..], p.slice(0, chunkSize));
        (written, e.err) = e.w.Write(e.@out.slice(0, encoded));
        n += written / 2;
        p = p.slice(chunkSize);
    }
    return (n, e.err);
}

partial struct decoder {
    internal io.Reader r;
    internal error err;
    internal slice<byte> @in;      // input buffer (encoded form)
    internal array<byte> arr = new(bufferSize); // backing array for in
}

// NewDecoder returns an [io.Reader] that decodes hexadecimal characters from r.
// NewDecoder expects that r contain only an even number of hexadecimal characters.
public static io.Reader NewDecoder(io.Reader r) {
    return new decoderжReader(Ꮡ(new decoder(r: r)));
}

internal static (nint n, error err) Read(this ref decoder d, slice<byte> p) {
    error err = default!;

    // Fill internal buffer with sufficient bytes to decode
    if (len(d.@in) < 2 && d.err == default!) {
        nint numCopy = default!;
        nint numRead = default!;
        numCopy = copy(d.arr[..], d.@in); // Copies either 0 or 1 bytes
        (numRead, d.err) = d.r.Read(d.arr.slice(numCopy));
        d.@in = d.arr.slice(0, numCopy + numRead);
        if (AreEqual(d.err, io.EOF) && len(d.@in) % 2 != 0) {
            {
                var a = reverseHexTable[d.@in[len(d.@in) - 1]]; if (a > 0x0f){
                    d.err = ((InvalidByteError)d.@in[len(d.@in) - 1]);
                } else {
                    d.err = io.ErrUnexpectedEOF;
                }
            }
        }
    }
    // Decode internal buffer into output buffer
    {
        nint numAvail = len(d.@in) / 2; if (len(p) > numAvail) {
            p = p.slice(0, numAvail);
        }
    }
    (var numDec, err) = Decode(p, d.@in.slice(0, len(p) * 2));
    d.@in = d.@in.slice(2 * numDec);
    if (err != default!) {
        (d.@in, d.err) = (default!, err); // Decode error; discard input remainder
    }
    if (len(d.@in) < 2) {
        return (numDec, d.err); // Only expose errors when buffer fully consumed
    }
    return (numDec, default!);
}

// Dumper returns a [io.WriteCloser] that writes a hex dump of all written data to
// w. The format of the dump matches the output of `hexdump -C` on the command
// line.
public static io.WriteCloser Dumper(io.Writer w) {
    return new dumperжWriteCloser(Ꮡ(new dumper(w: w)));
}

partial struct dumper {
    internal io.Writer w;
    internal array<byte> rightChars = new(18);
    internal array<byte> buf = new(14);
    internal nint used; // number of bytes in the current line
    internal nuint n; // number of bytes, total
    internal bool closed;
}

internal static byte toChar(byte b) {
    if (b < 32 || b > 126) {
        return (rune)'.';
    }
    return b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string encodingHexDumperClosedˢ = "encoding/hex: dumper closed"u8;

internal static (nint n, error err) Write(this ref dumper h, slice<byte> data) {
    nint n = default!;
    error err = default!;

    if (h.closed) {
        return (0, errors.New(encodingHexDumperClosedˢ));
    }
    // Output lines look like:
    // 00000010  2e 2f 30 31 32 33 34 35  36 37 38 39 3a 3b 3c 3d  |./0123456789:;<=|
    // ^ offset                          ^ extra space              ^ ASCII of line.
    foreach (var (i, _) in data) {
        if (h.used == 0) {
            // At the beginning of a line we print the current
            // offset in hex.
            h.buf[0] = (byte)((h.n >> (int)(24)));
            h.buf[1] = (byte)((h.n >> (int)(16)));
            h.buf[2] = (byte)((h.n >> (int)(8)));
            h.buf[3] = (byte)h.n;
            Encode(h.buf[4..], h.buf[..4]);
            h.buf[12] = (rune)' ';
            h.buf[13] = (rune)' ';
            (_, err) = h.w.Write(h.buf[4..]);
            if (err != default!) {
                return (n, err);
            }
        }
        Encode(h.buf[..], data.slice(i, i + 1));
        h.buf[2] = (rune)' ';
        nint l = 3;
        if (h.used == 7){
            // There's an additional space after the 8th byte.
            h.buf[3] = (rune)' ';
            l = 4;
        } else 
        if (h.used == 15) {
            // At the end of the line there's an extra space and
            // the bar for the right column.
            h.buf[3] = (rune)' ';
            h.buf[4] = (rune)'|';
            l = 5;
        }
        (_, err) = h.w.Write(h.buf.slice(0, l));
        if (err != default!) {
            return (n, err);
        }
        n++;
        h.rightChars[h.used] = toChar(data[i]);
        h.used++;
        h.n++;
        if (h.used == 16) {
            h.rightChars[16] = (rune)'|';
            h.rightChars[17] = (rune)'\n';
            (_, err) = h.w.Write(h.rightChars[..]);
            if (err != default!) {
                return (n, err);
            }
            h.used = 0;
        }
    }
    return (n, err);
}

internal static error /*err*/ Close(this ref dumper h) {
    error err = default!;

    // See the comments in Write() for the details of this format.
    if (h.closed) {
        return err;
    }
    h.closed = true;
    if (h.used == 0) {
        return err;
    }
    h.buf[0] = (rune)' ';
    h.buf[1] = (rune)' ';
    h.buf[2] = (rune)' ';
    h.buf[3] = (rune)' ';
    h.buf[4] = (rune)'|';
    nint nBytes = h.used;
    while (h.used < 16) {
        nint l = 3;
        if (h.used == 7){
            l = 4;
        } else 
        if (h.used == 15) {
            l = 5;
        }
        (_, err) = h.w.Write(h.buf.slice(0, l));
        if (err != default!) {
            return err;
        }
        h.used++;
    }
    h.rightChars[nBytes] = (rune)'|';
    h.rightChars[nBytes + 1] = (rune)'\n';
    (_, err) = h.w.Write(h.rightChars.slice(0, nBytes + 2));
    return err;
}

} // end hex_package
