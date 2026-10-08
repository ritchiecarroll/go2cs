// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package bufio implements buffered I/O. It wraps an io.Reader or io.Writer
// object, creating another object (Reader or Writer) that also implements
// the interface but provides buffering and some help for textual I/O.
namespace go;

using bytes = bytes_package;
using errors = errors_package;
using io = io_package;
using strings = strings_package;
using utf8 = unicode.utf8_package;
using unicode;

partial class bufio_package {

internal static UntypedInt defaultBufSize => 4096;

public static error ErrInvalidUnreadByte = errors.New("bufio: invalid use of UnreadByte"u8);
public static error ErrInvalidUnreadRune = errors.New("bufio: invalid use of UnreadRune"u8);
public static error ErrBufferFull = errors.New("bufio: buffer full"u8);
public static error ErrNegativeCount = errors.New("bufio: negative count"u8);

// Buffered input.

// Reader implements buffering for an io.Reader object.
// A new Reader is created by calling [NewReader] or [NewReaderSize];
// alternatively the zero value of a Reader may be used after calling [Reset]
// on it.
partial struct Reader {
    internal slice<byte> buf;
    internal io.Reader rd; // reader provided by the client
    internal nint r, w;      // buf read and write positions
    internal error err;
    internal nint lastByte; // last byte read for UnreadByte; -1 means invalid
    internal nint lastRuneSize; // size of last rune read for UnreadRune; -1 means invalid
}

internal static UntypedInt minReadBufferSize => 16;

internal static UntypedInt maxConsecutiveEmptyReads => 100;

// NewReaderSize returns a new [Reader] whose buffer has at least the specified
// size. If the argument io.Reader is already a [Reader] with large enough
// size, it returns the underlying [Reader].
public static ж<Reader> NewReaderSize(io.Reader rd, nint size) {
    // Is it already a Reader?
    var (b, ok) = rd._<ж<Reader>>(ᐧ);
    if (ok && len((~b).buf) >= size) {
        return b;
    }
    var r = @new<Reader>();
    r.reset(new slice<byte>(max(size, (nint)(minReadBufferSize))), rd);
    return r;
}

// NewReader returns a new [Reader] whose buffer has the default size.
public static ж<Reader> NewReader(io.Reader rd) {
    return NewReaderSize(rd, defaultBufSize);
}

// Size returns the size of the underlying buffer in bytes.
public static nint Size(this ref Reader b) {
    return len(b.buf);
}

// Reset discards any buffered data, resets all state, and switches
// the buffered reader to read from r.
// Calling Reset on the zero value of [Reader] initializes the internal buffer
// to the default size.
// Calling b.Reset(b) (that is, resetting a [Reader] to itself) does nothing.
public static void Reset(this ж<Reader> Ꮡb, io.Reader r) {
    ref var b = ref Ꮡb.DerefOrNull();

    // If a Reader r is passed to NewReader, NewReader will return r.
    // Different layers of code may do that, and then later pass r
    // to Reset. Avoid infinite recursion in that case.
    if (AreEqual(Ꮡb, r)) {
        return;
    }
    if (b.buf == default!) {
        b.buf = new slice<byte>(defaultBufSize);
    }
    b.reset(b.buf, r);
}

internal static void reset(this ref Reader b, slice<byte> buf, io.Reader r) {
    b = new Reader(
        buf: buf,
        rd: r,
        lastByte: -1,
        lastRuneSize: -1
    );
}

internal static error errNegativeRead = errors.New("bufio: reader returned negative count from Read"u8);

// fill reads a new chunk into the buffer.
internal static void fill(this ref Reader b) {
    // Slide existing data to beginning.
    if (b.r > 0) {
        copy(b.buf, b.buf.slice(b.r, b.w));
        b.w -= b.r;
        b.r = 0;
    }
    if (b.w >= len(b.buf)) {
        throw panic("bufio: tried to fill full buffer");
    }
    // Read new data: try a limited number of times.
    for (nint i = maxConsecutiveEmptyReads; i > 0; i--) {
        var (n, err) = b.rd.Read(b.buf.slice(b.w));
        if (n < 0) {
            throw panic(errNegativeRead);
        }
        b.w += n;
        if (err != default!) {
            b.err = err;
            return;
        }
        if (n > 0) {
            return;
        }
    }
    b.err = io.ErrNoProgress;
}

internal static error readErr(this ref Reader b) {
    var err = b.err;
    b.err = default!;
    return err;
}

// Peek returns the next n bytes without advancing the reader. The bytes stop
// being valid at the next read call. If necessary, Peek will read more bytes
// into the buffer in order to make n bytes available. If Peek returns fewer
// than n bytes, it also returns an error explaining why the read is short.
// The error is [ErrBufferFull] if n is larger than b's buffer size.
//
// Calling Peek prevents a [Reader.UnreadByte] or [Reader.UnreadRune] call from succeeding
// until the next read operation.
public static (slice<byte>, error) Peek(this ref Reader b, nint n) {
    if (n < 0) {
        return (default!, ErrNegativeCount);
    }
    b.lastByte = -1;
    b.lastRuneSize = -1;
    while (b.w - b.r < n && b.w - b.r < len(b.buf) && b.err == default!) {
        b.fill(); // b.w-b.r < len(b.buf) => buffer is not full
    }
    if (n > len(b.buf)) {
        return (b.buf.slice(b.r, b.w), ErrBufferFull);
    }
    // 0 <= n <= len(b.buf)
    error err = default!;
    {
        nint avail = b.w - b.r; if (avail < n) {
            // not enough data in buffer
            n = avail;
            err = b.readErr();
            if (err == default!) {
                err = ErrBufferFull;
            }
        }
    }
    return (b.buf.slice(b.r, b.r + n), err);
}

// Discard skips the next n bytes, returning the number of bytes discarded.
//
// If Discard skips fewer than n bytes, it also returns an error.
// If 0 <= n <= b.Buffered(), Discard is guaranteed to succeed without
// reading from the underlying io.Reader.
public static (nint discarded, error err) Discard(this ref Reader b, nint n) {
    nint discarded = default!;
    error err = default!;

    if (n < 0) {
        return (0, ErrNegativeCount);
    }
    if (n == 0) {
        return (discarded, err);
    }
    b.lastByte = -1;
    b.lastRuneSize = -1;
    nint remain = n;
    while (ᐧ) {
        nint skip = b.Buffered();
        if (skip == 0) {
            b.fill();
            skip = b.Buffered();
        }
        if (skip > remain) {
            skip = remain;
        }
        b.r += skip;
        remain -= skip;
        if (remain == 0) {
            return (n, default!);
        }
        if (b.err != default!) {
            return (n - remain, b.readErr());
        }
    }
}

// Read reads data into p.
// It returns the number of bytes read into p.
// The bytes are taken from at most one Read on the underlying [Reader],
// hence n may be less than len(p).
// To read exactly len(p) bytes, use io.ReadFull(b, p).
// If the underlying [Reader] can return a non-zero count with io.EOF,
// then this Read method can do so as well; see the [io.Reader] docs.
public static (nint n, error err) Read(this ref Reader b, slice<byte> p) {
    nint n = default!;

    n = len(p);
    if (n == 0) {
        if (b.Buffered() > 0) {
            return (0, default!);
        }
        return (0, b.readErr());
    }
    if (b.r == b.w) {
        if (b.err != default!) {
            return (0, b.readErr());
        }
        if (len(p) >= len(b.buf)) {
            // Large read, empty buffer.
            // Read directly into p to avoid copy.
            (n, b.err) = b.rd.Read(p);
            if (n < 0) {
                throw panic(errNegativeRead);
            }
            if (n > 0) {
                b.lastByte = (nint)p[n - 1];
                b.lastRuneSize = -1;
            }
            return (n, b.readErr());
        }
        // One read.
        // Do not use b.fill, which will loop.
        b.r = 0;
        b.w = 0;
        (n, b.err) = b.rd.Read(b.buf);
        if (n < 0) {
            throw panic(errNegativeRead);
        }
        if (n == 0) {
            return (0, b.readErr());
        }
        b.w += n;
    }
    // copy as much as we can
    // Note: if the slice panics here, it is probably because
    // the underlying reader returned a bad count. See issue 49795.
    n = copy(p, b.buf.slice(b.r, b.w));
    b.r += n;
    b.lastByte = (nint)b.buf[b.r - 1];
    b.lastRuneSize = -1;
    return (n, default!);
}

// ReadByte reads and returns a single byte.
// If no byte is available, returns an error.
public static (byte, error) ReadByte(this ref Reader b) {
    b.lastRuneSize = -1;
    while (b.r == b.w) {
        if (b.err != default!) {
            return (0, b.readErr());
        }
        b.fill(); // buffer is empty
    }
    var c = b.buf[b.r];
    b.r++;
    b.lastByte = (nint)c;
    return (c, default!);
}

// UnreadByte unreads the last byte. Only the most recently read byte can be unread.
//
// UnreadByte returns an error if the most recent method called on the
// [Reader] was not a read operation. Notably, [Reader.Peek], [Reader.Discard], and [Reader.WriteTo] are not
// considered read operations.
public static error UnreadByte(this ref Reader b) {
    if (b.lastByte < 0 || b.r == 0 && b.w > 0) {
        return ErrInvalidUnreadByte;
    }
    // b.r > 0 || b.w == 0
    if (b.r > 0){
        b.r--;
    } else {
        // b.r == 0 && b.w == 0
        b.w = 1;
    }
    b.buf[b.r] = (byte)b.lastByte;
    b.lastByte = -1;
    b.lastRuneSize = -1;
    return default!;
}

// ReadRune reads a single UTF-8 encoded Unicode character and returns the
// rune and its size in bytes. If the encoded rune is invalid, it consumes one byte
// and returns unicode.ReplacementChar (U+FFFD) with a size of 1.
public static (rune r, nint size, error err) ReadRune(this ref Reader b) {
    rune r = default!;
    nint size = default!;

    while (b.r + (nint)utf8.UTFMax > b.w && !utf8.FullRune(b.buf.slice(b.r, b.w)) && b.err == default! && b.w - b.r < len(b.buf)) {
        b.fill(); // b.w-b.r < len(buf) => buffer is not full
    }
    b.lastRuneSize = -1;
    if (b.r == b.w) {
        return (0, 0, b.readErr());
    }
    (r, size) = ((rune)b.buf[b.r], 1);
    if (r >= utf8.RuneSelf) {
        (r, size) = utf8.DecodeRune(b.buf.slice(b.r, b.w));
    }
    b.r += size;
    b.lastByte = (nint)b.buf[b.r - 1];
    b.lastRuneSize = size;
    return (r, size, default!);
}

// UnreadRune unreads the last rune. If the most recent method called on
// the [Reader] was not a [Reader.ReadRune], [Reader.UnreadRune] returns an error. (In this
// regard it is stricter than [Reader.UnreadByte], which will unread the last byte
// from any read operation.)
public static error UnreadRune(this ref Reader b) {
    if (b.lastRuneSize < 0 || b.r < b.lastRuneSize) {
        return ErrInvalidUnreadRune;
    }
    b.r -= b.lastRuneSize;
    b.lastByte = -1;
    b.lastRuneSize = -1;
    return default!;
}

// Buffered returns the number of bytes that can be read from the current buffer.
public static nint Buffered(this ref Reader b) {
    return b.w - b.r;
}

// ReadSlice reads until the first occurrence of delim in the input,
// returning a slice pointing at the bytes in the buffer.
// The bytes stop being valid at the next read.
// If ReadSlice encounters an error before finding a delimiter,
// it returns all the data in the buffer and the error itself (often io.EOF).
// ReadSlice fails with error [ErrBufferFull] if the buffer fills without a delim.
// Because the data returned from ReadSlice will be overwritten
// by the next I/O operation, most clients should use
// [Reader.ReadBytes] or ReadString instead.
// ReadSlice returns err != nil if and only if line does not end in delim.
public static (slice<byte> line, error err) ReadSlice(this ref Reader b, byte delim) {
    slice<byte> line = default!;
    error err = default!;

    nint s = 0; // search start index
    while (ᐧ) {
        // Search buffer.
        {
            nint i = bytes.IndexByte(b.buf.slice(b.r + s, b.w), delim); if (i >= 0) {
                i += s;
                line = b.buf.slice(b.r, b.r + i + 1);
                b.r += i + 1;
                break;
            }
        }
        // Pending error?
        if (b.err != default!) {
            line = b.buf.slice(b.r, b.w);
            b.r = b.w;
            err = b.readErr();
            break;
        }
        // Buffer full?
        if (b.Buffered() >= len(b.buf)) {
            b.r = b.w;
            line = b.buf;
            err = ErrBufferFull;
            break;
        }
        s = b.w - b.r; // do not rescan area we scanned before
        b.fill(); // buffer is not full
    }
    // Handle last byte, if any.
    {
        nint i = len(line) - 1; if (i >= 0) {
            b.lastByte = (nint)line[i];
            b.lastRuneSize = -1;
        }
    }
    return (line, err);
}

// ReadLine is a low-level line-reading primitive. Most callers should use
// [Reader.ReadBytes]('\n') or [Reader.ReadString]('\n') instead or use a [Scanner].
//
// ReadLine tries to return a single line, not including the end-of-line bytes.
// If the line was too long for the buffer then isPrefix is set and the
// beginning of the line is returned. The rest of the line will be returned
// from future calls. isPrefix will be false when returning the last fragment
// of the line. The returned buffer is only valid until the next call to
// ReadLine. ReadLine either returns a non-nil line or it returns an error,
// never both.
//
// The text returned from ReadLine does not include the line end ("\r\n" or "\n").
// No indication or error is given if the input ends without a final line end.
// Calling [Reader.UnreadByte] after ReadLine will always unread the last byte read
// (possibly a character belonging to the line end) even if that byte is not
// part of the line returned by ReadLine.
public static (slice<byte> line, bool isPrefix, error err) ReadLine(this ref Reader b) {
    slice<byte> line = default!;
    bool isPrefix = default!;
    error err = default!;

    (line, err) = b.ReadSlice((rune)'\n');
    if (AreEqual(err, ErrBufferFull)) {
        // Handle the case where "\r\n" straddles the buffer.
        if (len(line) > 0 && line[len(line) - 1] == (rune)'\r') {
            // Put the '\r' back on buf and drop it from line.
            // Let the next call to ReadLine check for "\r\n".
            if (b.r == 0) {
                // should be unreachable
                throw panic("bufio: tried to rewind past start of buffer");
            }
            b.r--;
            line = line.slice(0, len(line) - 1);
        }
        return (line, true, default!);
    }
    if (len(line) == 0) {
        if (err != default!) {
            line = default!;
        }
        return (line, isPrefix, err);
    }
    err = default!;
    if (line[len(line) - 1] == (rune)'\n') {
        nint drop = 1;
        if (len(line) > 1 && line[len(line) - 2] == (rune)'\r') {
            drop = 2;
        }
        line = line.slice(0, len(line) - drop);
    }
    return (line, isPrefix, err);
}

// collectFragments reads until the first occurrence of delim in the input. It
// returns (slice of full buffers, remaining bytes before delim, total number
// of bytes in the combined first two elements, error).
// The complete result is equal to
// `bytes.Join(append(fullBuffers, finalFragment), nil)`, which has a
// length of `totalLen`. The result is structured in this way to allow callers
// to minimize allocations and copies.
internal static (slice<slice<byte>> fullBuffers, slice<byte> finalFragment, nint totalLen, error err) collectFragments(this ref Reader b, byte delim) {
    slice<slice<byte>> fullBuffers = default!;
    nint totalLen = default!;
    error err = default!;

    slice<byte> frag = default!;
    // Use ReadSlice to look for delim, accumulating full buffers.
    while (ᐧ) {
        error e = default!;
        (frag, e) = b.ReadSlice(delim);
        if (e == default!) {
            // got final fragment
            break;
        }
        if (!AreEqual(e, ErrBufferFull)) {
            // unexpected error
            err = e;
            break;
        }
        // Make a copy of the buffer.
        var buf = bytes.Clone(frag);
        fullBuffers = append(fullBuffers, buf);
        totalLen += len(buf);
    }
    totalLen += len(frag);
    return (fullBuffers, frag, totalLen, err);
}

// ReadBytes reads until the first occurrence of delim in the input,
// returning a slice containing the data up to and including the delimiter.
// If ReadBytes encounters an error before finding a delimiter,
// it returns the data read before the error and the error itself (often io.EOF).
// ReadBytes returns err != nil if and only if the returned data does not end in
// delim.
// For simple uses, a Scanner may be more convenient.
public static (slice<byte>, error) ReadBytes(this ref Reader b, byte delim) {
    var (full, frag, n, err) = b.collectFragments(delim);
    // Allocate new buffer to hold the full pieces and the fragment.
    var buf = new slice<byte>(n);
    n = 0;
    // Copy full pieces and fragment in.
    foreach (var (i, _) in full) {
        n += copy(buf.slice(n), full[i]);
    }
    copy(buf.slice(n), frag);
    return (buf, err);
}

// ReadString reads until the first occurrence of delim in the input,
// returning a string containing the data up to and including the delimiter.
// If ReadString encounters an error before finding a delimiter,
// it returns the data read before the error and the error itself (often io.EOF).
// ReadString returns err != nil if and only if the returned data does not end in
// delim.
// For simple uses, a Scanner may be more convenient.
public static (@string, error) ReadString(this ref Reader b, byte delim) {
    var (full, frag, n, err) = b.collectFragments(delim);
    // Allocate new buffer to hold the full pieces and the fragment.
    ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
    Ꮡbuf.Grow(n);
    // Copy full pieces and fragment in.
    foreach (var (_, fb) in full) {
        Ꮡbuf.Write(fb);
    }
    Ꮡbuf.Write(frag);
    return (buf.String(), err);
}

// WriteTo implements io.WriterTo.
// This may make multiple calls to the [Reader.Read] method of the underlying [Reader].
// If the underlying reader supports the [Reader.WriteTo] method,
// this calls the underlying [Reader.WriteTo] without buffering.
public static (int64 n, error err) WriteTo(this ref Reader b, io.Writer w) {
    int64 n = default!;
    error err = default!;

    b.lastByte = -1;
    b.lastRuneSize = -1;
    (n, err) = b.writeBuf(w);
    if (err != default!) {
        return (n, err);
    }
    {
        var (r, ok) = b.rd._<io.WriterTo>(ᐧ); if (ok) {
            var (m, errΔ1) = r.WriteTo(w);
            n += m;
            return (n, errΔ1);
        }
    }
    {
        var (wΔ1, ok) = w._<io.ReaderFrom>(ᐧ); if (ok) {
            var (m, errΔ2) = wΔ1.ReadFrom(b.rd);
            n += m;
            return (n, errΔ2);
        }
    }
    if (b.w - b.r < len(b.buf)) {
        b.fill(); // buffer not full
    }
    while (b.r < b.w) {
        // b.r < b.w => buffer is not empty
        var (m, errΔ3) = b.writeBuf(w);
        n += m;
        if (errΔ3 != default!) {
            return (n, errΔ3);
        }
        b.fill(); // buffer is empty
    }
    if (AreEqual(b.err, io.EOF)) {
        b.err = default!;
    }
    return (n, b.readErr());
}

internal static error errNegativeWrite = errors.New("bufio: writer returned negative count from Write"u8);

// writeBuf writes the [Reader]'s buffer to the writer.
internal static (int64, error) writeBuf(this ref Reader b, io.Writer w) {
    var (n, err) = w.Write(b.buf.slice(b.r, b.w));
    if (n < 0) {
        throw panic(errNegativeWrite);
    }
    b.r += n;
    return ((int64)n, err);
}

// buffered output

// Writer implements buffering for an [io.Writer] object.
// If an error occurs writing to a [Writer], no more data will be
// accepted and all subsequent writes, and [Writer.Flush], will return the error.
// After all data has been written, the client should call the
// [Writer.Flush] method to guarantee all data has been forwarded to
// the underlying [io.Writer].
partial struct Writer {
    internal error err;
    internal slice<byte> buf;
    internal nint n;
    internal io.Writer wr;
}

// NewWriterSize returns a new [Writer] whose buffer has at least the specified
// size. If the argument io.Writer is already a [Writer] with large enough
// size, it returns the underlying [Writer].
public static ж<Writer> NewWriterSize(io.Writer w, nint size) {
    // Is it already a Writer?
    var (b, ok) = w._<ж<Writer>>(ᐧ);
    if (ok && len((~b).buf) >= size) {
        return b;
    }
    if (size <= 0) {
        size = defaultBufSize;
    }
    return Ꮡ(new Writer(
        buf: new slice<byte>(size),
        wr: w
    ));
}

// NewWriter returns a new [Writer] whose buffer has the default size.
// If the argument io.Writer is already a [Writer] with large enough buffer size,
// it returns the underlying [Writer].
public static ж<Writer> NewWriter(io.Writer w) {
    return NewWriterSize(w, defaultBufSize);
}

// Size returns the size of the underlying buffer in bytes.
public static nint Size(this ref Writer b) {
    return len(b.buf);
}

// Reset discards any unflushed buffered data, clears any error, and
// resets b to write its output to w.
// Calling Reset on the zero value of [Writer] initializes the internal buffer
// to the default size.
// Calling w.Reset(w) (that is, resetting a [Writer] to itself) does nothing.
public static void Reset(this ж<Writer> Ꮡb, io.Writer w) {
    ref var b = ref Ꮡb.DerefOrNull();

    // If a Writer w is passed to NewWriter, NewWriter will return w.
    // Different layers of code may do that, and then later pass w
    // to Reset. Avoid infinite recursion in that case.
    if (AreEqual(Ꮡb, w)) {
        return;
    }
    if (b.buf == default!) {
        b.buf = new slice<byte>(defaultBufSize);
    }
    b.err = default!;
    b.n = 0;
    b.wr = w;
}

// Flush writes any buffered data to the underlying [io.Writer].
public static error Flush(this ref Writer b) {
    if (b.err != default!) {
        return b.err;
    }
    if (b.n == 0) {
        return default!;
    }
    var (n, err) = b.wr.Write(b.buf.slice(0, b.n));
    if (n < b.n && err == default!) {
        err = io.ErrShortWrite;
    }
    if (err != default!) {
        if (n > 0 && n < b.n) {
            copy(b.buf.slice(0, b.n - n), b.buf.slice(n, b.n));
        }
        b.n -= n;
        b.err = err;
        return err;
    }
    b.n = 0;
    return default!;
}

// Available returns how many bytes are unused in the buffer.
public static nint Available(this ref Writer b) {
    return len(b.buf) - b.n;
}

// AvailableBuffer returns an empty buffer with b.Available() capacity.
// This buffer is intended to be appended to and
// passed to an immediately succeeding [Writer.Write] call.
// The buffer is only valid until the next write operation on b.
public static slice<byte> AvailableBuffer(this ref Writer b) {
    return b.buf.slice(b.n)[..0];
}

// Buffered returns the number of bytes that have been written into the current buffer.
public static nint Buffered(this ref Writer b) {
    return b.n;
}

// Write writes the contents of p into the buffer.
// It returns the number of bytes written.
// If nn < len(p), it also returns an error explaining
// why the write is short.
public static (nint nn, error err) Write(this ref Writer b, slice<byte> p) {
    nint nn = default!;

    while (len(p) > b.Available() && b.err == default!) {
        nint nΔ1 = default!;
        if (b.Buffered() == 0){
            // Large write, empty buffer.
            // Write directly from p to avoid copy.
            (nΔ1, b.err) = b.wr.Write(p);
        } else {
            nΔ1 = copy(b.buf.slice(b.n), p);
            b.n += nΔ1;
            b.Flush();
        }
        nn += nΔ1;
        p = p.slice(nΔ1);
    }
    if (b.err != default!) {
        return (nn, b.err);
    }
    nint n = copy(b.buf.slice(b.n), p);
    b.n += n;
    nn += n;
    return (nn, default!);
}

// WriteByte writes a single byte.
public static error WriteByte(this ref Writer b, byte c) {
    if (b.err != default!) {
        return b.err;
    }
    if (b.Available() <= 0 && b.Flush() != default!) {
        return b.err;
    }
    b.buf[b.n] = c;
    b.n++;
    return default!;
}

// WriteRune writes a single Unicode code point, returning
// the number of bytes written and any error.
public static (nint size, error err) WriteRune(this ref Writer b, rune r) {
    nint size = default!;
    error err = default!;

    // Compare as uint32 to correctly handle negative runes.
    if ((uint32)r < utf8.RuneSelf) {
        err = b.WriteByte((byte)r);
        if (err != default!) {
            return (0, err);
        }
        return (1, default!);
    }
    if (b.err != default!) {
        return (0, b.err);
    }
    nint n = b.Available();
    if (n < utf8.UTFMax) {
        {
            b.Flush(); if (b.err != default!) {
                return (0, b.err);
            }
        }
        n = b.Available();
        if (n < utf8.UTFMax) {
            // Can only happen if buffer is silly small.
            return b.WriteString(((@string)r));
        }
    }
    size = utf8.EncodeRune(b.buf.slice(b.n), r);
    b.n += size;
    return (size, default!);
}

// WriteString writes a string.
// It returns the number of bytes written.
// If the count is less than len(s), it also returns an error explaining
// why the write is short.
public static (nint, error) WriteString(this ref Writer b, @string s) {
    io.StringWriter sw = default!;
    var tryStringWriter = true;
    nint nn = 0;
    while (len(s) > b.Available() && b.err == default!) {
        nint nΔ1 = default!;
        if (b.Buffered() == 0 && sw == default! && tryStringWriter) {
            // Check at most once whether b.wr is a StringWriter.
            (sw, tryStringWriter) = b.wr._<io.StringWriter>(ᐧ);
        }
        if (b.Buffered() == 0 && tryStringWriter){
            // Large write, empty buffer, and the underlying writer supports
            // WriteString: forward the write to the underlying StringWriter.
            // This avoids an extra copy.
            (nΔ1, b.err) = sw.WriteString(s);
        } else {
            nΔ1 = copy(b.buf.slice(b.n), s);
            b.n += nΔ1;
            b.Flush();
        }
        nn += nΔ1;
        s = s.slice(nΔ1);
    }
    if (b.err != default!) {
        return (nn, b.err);
    }
    nint n = copy(b.buf.slice(b.n), s);
    b.n += n;
    nn += n;
    return (nn, default!);
}

// ReadFrom implements [io.ReaderFrom]. If the underlying writer
// supports the ReadFrom method, this calls the underlying ReadFrom.
// If there is buffered data and an underlying ReadFrom, this fills
// the buffer and writes it before calling ReadFrom.
public static (int64 n, error err) ReadFrom(this ref Writer b, io.Reader r) {
    int64 n = default!;
    error err = default!;

    if (b.err != default!) {
        return (0, b.err);
    }
    var (readerFrom, readerFromOK) = b.wr._<io.ReaderFrom>(ᐧ);
    nint m = default!;
    while (ᐧ) {
        if (b.Available() == 0) {
            {
                var err1 = b.Flush(); if (err1 != default!) {
                    return (n, err1);
                }
            }
        }
        if (readerFromOK && b.Buffered() == 0) {
            var (nn, errΔ1) = readerFrom.ReadFrom(r);
            b.err = errΔ1;
            n += nn;
            return (n, errΔ1);
        }
        nint nr = 0;
        while (nr < maxConsecutiveEmptyReads) {
            (m, err) = r.Read(b.buf.slice(b.n));
            if (m != 0 || err != default!) {
                break;
            }
            nr++;
        }
        if (nr == maxConsecutiveEmptyReads) {
            return (n, io.ErrNoProgress);
        }
        b.n += m;
        n += (int64)m;
        if (err != default!) {
            break;
        }
    }
    if (AreEqual(err, io.EOF)) {
        // If we filled the buffer exactly, flush preemptively.
        if (b.Available() == 0){
            err = b.Flush();
        } else {
            err = default!;
        }
    }
    return (n, err);
}

// buffered input and output

// ReadWriter stores pointers to a [Reader] and a [Writer].
// It implements [io.ReadWriter].
partial struct ReadWriter {
    public partial ref ж<Reader> Reader { get; }
    public partial ref ж<Writer> Writer { get; }
}

// NewReadWriter allocates a new [ReadWriter] that dispatches to r and w.
public static ж<ReadWriter> NewReadWriter(ж<Reader> Ꮡr, ж<Writer> Ꮡw) {
    return Ꮡ(new ReadWriter(Ꮡr, Ꮡw));
}

} // end bufio_package
