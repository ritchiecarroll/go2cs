// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// The wire protocol for HTTP's "chunked" Transfer-Encoding.

// Package internal contains HTTP internals shared by net/http and
// net/http/httputil.
namespace go.net.http;

using bufio = bufio_package;
using bytes = bytes_package;
using errors = errors_package;
using fmt = fmt_package;
using io = io_package;

partial class internal_package {

internal static UntypedInt maxLineLength => 4096; // assumed <= bufio.defaultBufSize

public static error ErrLineTooLong = errors.New("header line too long"u8);

// NewChunkedReader returns a new chunkedReader that translates the data read from r
// out of HTTP "chunked" format before returning it.
// The chunkedReader returns [io.EOF] when the final 0-length chunk is read.
//
// NewChunkedReader is not needed by normal applications. The http package
// automatically decodes chunking when reading response bodies.
public static io.Reader NewChunkedReader(io.Reader r) {
    var (br, ok) = r._<ж<bufio.Reader>>(ᐧ);
    if (!ok) {
        br = bufio.NewReader(r);
    }
    return new chunkedReaderжReader(Ꮡ(new chunkedReader(r: br)));
}

[GoType] partial struct chunkedReader {
    internal ж<bufio.Reader> r;
    internal uint64 n; // unread bytes in chunk
    internal error err;
    internal array<byte> buf = new(2);
    internal bool checkEnd;  // whether need to check for \r\n chunk footer
    internal int64 excess; // "excessive" chunk overhead, for malicious sender detection
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chunkedEncodingContainsˢ = "chunked encoding contains too much non-data"u8;

[GoRecv] internal static void beginChunk(this ref chunkedReader cr) {
    // chunk-size CRLF
    slice<byte> line = default!;
    (line, cr.err) = readChunkLine(cr.r);
    if (cr.err != default!) {
        return;
    }
    cr.excess += (int64)len(line) + 2; // header, plus \r\n after the chunk data
    line = trimTrailingWhitespace(line);
    (line, cr.err) = removeChunkExtension(line);
    if (cr.err != default!) {
        return;
    }
    (cr.n, cr.err) = parseHexUint(line);
    if (cr.err != default!) {
        return;
    }
    // A sender who sends one byte per chunk will send 5 bytes of overhead
    // for every byte of data. ("1\r\nX\r\n" to send "X".)
    // We want to allow this, since streaming a byte at a time can be legitimate.
    //
    // A sender can use chunk extensions to add arbitrary amounts of additional
    // data per byte read. ("1;very long extension\r\nX\r\n" to send "X".)
    // We don't want to disallow extensions (although we discard them),
    // but we also don't want to allow a sender to reduce the signal/noise ratio
    // arbitrarily.
    //
    // We track the amount of excess overhead read,
    // and produce an error if it grows too large.
    //
    // Currently, we say that we're willing to accept 16 bytes of overhead per chunk,
    // plus twice the amount of real data in the chunk.
    cr.excess -= 16 + (2 * (int64)cr.n);
    cr.excess = max(cr.excess, 0);
    if (cr.excess > 16 * 1024) {
        cr.err = errors.New(chunkedEncodingContainsˢ);
    }
    if (cr.n == 0) {
        cr.err = io.EOF;
    }
}

[GoRecv] internal static bool chunkHeaderAvailable(this ref chunkedReader cr) {
    nint n = cr.r.Buffered();
    if (n > 0) {
        var (peek, _) = cr.r.Peek(n);
        return bytes.IndexByte(peek, (rune)'\n') >= 0;
    }
    return false;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string malformedChunkedEncodingˢ = "malformed chunked encoding"u8;

[GoRecv] internal static (nint n, error err) Read(this ref chunkedReader cr, slice<uint8> b) {
    nint n = default!;

    while (cr.err == default!) {
        if (cr.checkEnd) {
            if (n > 0 && cr.r.Buffered() < 2) {
                // We have some data. Return early (per the io.Reader
                // contract) instead of potentially blocking while
                // reading more.
                break;
            }
            {
                (_, cr.err) = io.ReadFull(new bufio_ReaderжReader(cr.r), cr.buf[..2]); if (cr.err == default!){
                    if (((sstring)(cr.buf[..])) != "\r\n"u8) {
                        cr.err = errors.New(malformedChunkedEncodingˢ);
                        break;
                    }
                } else {
                    if (AreEqual(cr.err, io.EOF)) {
                        cr.err = io.ErrUnexpectedEOF;
                    }
                    break;
                }
            }
            cr.checkEnd = false;
        }
        if (cr.n == 0) {
            if (n > 0 && !cr.chunkHeaderAvailable()) {
                // We've read enough. Don't potentially block
                // reading a new chunk header.
                break;
            }
            cr.beginChunk();
            continue;
        }
        if (len(b) == 0) {
            break;
        }
        var rbuf = b;
        if ((uint64)len(rbuf) > cr.n) {
            rbuf = rbuf.slice(0, (nint)(cr.n));
        }
        nint n0 = default!;
        (n0, cr.err) = cr.r.Read(rbuf);
        n += n0;
        b = b.slice(n0);
        cr.n -= (uint64)n0;
        // If we're at the end of a chunk, read the next two
        // bytes to verify they are "\r\n".
        if (cr.n == 0 && cr.err == default!){
            cr.checkEnd = true;
        } else 
        if (AreEqual(cr.err, io.EOF)) {
            cr.err = io.ErrUnexpectedEOF;
        }
    }
    return (n, cr.err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string chunkedLineEndsWithBareˢ = "chunked line ends with bare LF"u8;
internal static readonly @string invalidCrInChunkedLineˢ = "invalid CR in chunked line"u8;

// Read a line of bytes (up to \n) from b.
// Give up if the line exceeds maxLineLength.
// The returned bytes are owned by the bufio.Reader
// so they are only valid until the next bufio read.
internal static (slice<byte>, error) readChunkLine(ж<bufio.Reader> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (p, err) = b.ReadSlice((rune)'\n');
    if (err != default!) {
        // We always know when EOF is coming.
        // If the caller asked for a line, there should be a line.
        if (AreEqual(err, io.EOF)){
            err = io.ErrUnexpectedEOF;
        } else 
        if (AreEqual(err, bufio.ErrBufferFull)) {
            err = ErrLineTooLong;
        }
        return (default!, err);
    }
    // RFC 9112 permits parsers to accept a bare \n as a line ending in headers,
    // but not in chunked encoding lines. See https://www.rfc-editor.org/errata/eid7633,
    // which explicitly rejects a clarification permitting \n as a chunk terminator.
    //
    // Verify that the line ends in a CRLF, and that no CRs appear before the end.
    {
        nint idx = bytes.IndexByte(p, (rune)'\r'); if (idx == -1){
            return (default!, errors.New(chunkedLineEndsWithBareˢ));
        } else 
        if (idx != len(p) - 2) {
            return (default!, errors.New(invalidCrInChunkedLineˢ));
        }
    }
    p = p.slice(0, len(p) - 2); // trim CRLF
    if (len(p) >= maxLineLength) {
        return (default!, ErrLineTooLong);
    }
    return (p, default!);
}

internal static slice<byte> trimTrailingWhitespace(slice<byte> b) {
    while (len(b) > 0 && isOWS(b[len(b) - 1])) {
        b = b.slice(0, len(b) - 1);
    }
    return b;
}

internal static bool isOWS(byte b) {
    return b == (rune)' ' || b == (rune)'\t';
}

internal static slice<byte> semi = slice<byte>(";"u8);

// removeChunkExtension removes any chunk-extension from p.
// For example,
//
//	"0" => "0"
//	"0;token" => "0"
//	"0;token=val" => "0"
//	`0;token="quoted string"` => "0"
internal static (slice<byte>, error) removeChunkExtension(slice<byte> p) {
    (p, _, _) = bytes.Cut(p, semi);
    // TODO: care about exact syntax of chunk extensions? We're
    // ignoring and stripping them anyway. For now just never
    // return an error.
    return (p, default!);
}

// NewChunkedWriter returns a new chunkedWriter that translates writes into HTTP
// "chunked" format before writing them to w. Closing the returned chunkedWriter
// sends the final 0-length chunk that marks the end of the stream but does
// not send the final CRLF that appears after trailers; trailers and the last
// CRLF must be written separately.
//
// NewChunkedWriter is not needed by normal applications. The http
// package adds chunking automatically if handlers don't set a
// Content-Length header. Using newChunkedWriter inside a handler
// would result in double chunking or chunking with a Content-Length
// length, both of which are wrong.
public static io.WriteCloser NewChunkedWriter(io.Writer w) {
    return new chunkedWriterжWriteCloser(Ꮡ(new chunkedWriter(w)));
}

// Writing to chunkedWriter translates to writing in HTTP chunked Transfer
// Encoding wire format to the underlying Wire chunkedWriter.
[GoType] partial struct chunkedWriter {
    public io.Writer Wire;
}

// Write the contents of data as one chunk to Wire.
// NOTE: Note that the corresponding chunk-writing procedure in Conn.Write has
// a bug since it does not check for success of [io.WriteString]
[GoRecv] internal static (nint n, error err) Write(this ref chunkedWriter cw, slice<byte> data) {
    nint n = default!;
    error err = default!;

    // Don't send 0-length data. It looks like EOF for chunked encoding.
    if (len(data) == 0) {
        return (0, default!);
    }
    {
        (_, err) = fmt.Fprintf(cw.Wire, "%x\r\n"u8, len(data)); if (err != default!) {
            return (0, err);
        }
    }
    {
        (n, err) = cw.Wire.Write(data); if (err != default!) {
            return (n, err);
        }
    }
    if (n != len(data)) {
        err = io.ErrShortWrite;
        return (n, err);
    }
    {
        (_, err) = io.WriteString(cw.Wire, "\r\n"u8); if (err != default!) {
            return (n, err);
        }
    }
    {
        var (bw, ok) = cw.Wire._<ж<FlushAfterChunkWriter>>(ᐧ); if (ok) {
            err = bw.Value.Writer.Value.Flush();
        }
    }
    return (n, err);
}

[GoRecv] internal static error Close(this ref chunkedWriter cw) {
    var (_, err) = io.WriteString(cw.Wire, "0\r\n"u8);
    return err;
}

// FlushAfterChunkWriter signals from the caller of [NewChunkedWriter]
// that each chunk should be followed by a flush. It is used by the
// [net/http.Transport] code to keep the buffering behavior for headers and
// trailers, but flush out chunks aggressively in the middle for
// request bodies which may be generated slowly. See Issue 6574.
[GoType] partial struct FlushAfterChunkWriter {
    public partial ref ж<bufio_package.Writer> Writer { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string emptyHexNumberForChunkˢ = "empty hex number for chunk length"u8;
internal static readonly @string invalidByteInChunkLengthˢ = "invalid byte in chunk length"u8;
internal static readonly @string httpChunkLengthTooLargeˢ = "http chunk length too large"u8;

internal static (uint64 n, error err) parseHexUint(slice<byte> v) {
    uint64 n = default!;
    error err = default!;

    if (len(v) == 0) {
        return (0, errors.New(emptyHexNumberForChunkˢ));
    }
    foreach (var (i, vᴛ1) in v) {
        var b = vᴛ1;

        switch (ᐧ) {
        case {} when (rune)'0' <= b && b <= (rune)'9': {
            b = (byte)(b - (rune)'0');
            break;
        }
        case {} when (rune)'a' <= b && b <= (rune)'f': {
            b = (byte)(b - (rune)'a' + 10);
            break;
        }
        case {} when (rune)'A' <= b && b <= (rune)'F': {
            b = (byte)(b - (rune)'A' + 10);
            break;
        }
        default: {
            return (0, errors.New(invalidByteInChunkLengthˢ));
        }}

        if (i == 16) {
            return (0, errors.New(httpChunkLengthTooLargeˢ));
        }
        n <<= (int)(4);
        n |= (uint64)((uint64)b);
    }
    return (n, err);
}

} // end internal_package
