// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package io provides basic interfaces to I/O primitives.
// Its primary job is to wrap existing implementations of such primitives,
// such as those in package os, into shared public interfaces that
// abstract the functionality, plus some other related primitives.
//
// Because these interfaces and primitives wrap lower-level operations with
// various implementations, unless otherwise informed clients should not
// assume they are safe for parallel execution.
namespace go;

using errors = errors_package;
using Δsync = sync_package;

partial class io_package {

// Seek whence values.
public static UntypedInt SeekStart => 0; // seek relative to the origin of the file

public static UntypedInt SeekCurrent => 1; // seek relative to the current offset

public static UntypedInt SeekEnd => 2; // seek relative to the end

// ErrShortWrite means that a write accepted fewer bytes than requested
// but failed to return an explicit error.
public static error ErrShortWrite = errors.New("short write"u8);

// errInvalidWrite means that a write returned an impossible count.
internal static error errInvalidWrite = errors.New("invalid write result"u8);

// ErrShortBuffer means that a read required a longer buffer than was provided.
public static error ErrShortBuffer = errors.New("short buffer"u8);

// EOF is the error returned by Read when no more input is available.
// (Read must return EOF itself, not an error wrapping EOF,
// because callers will test for EOF using ==.)
// Functions should return EOF only to signal a graceful end of input.
// If the EOF occurs unexpectedly in a structured data stream,
// the appropriate error is either [ErrUnexpectedEOF] or some other error
// giving more detail.
public static error EOF = errors.New("EOF"u8);

// ErrUnexpectedEOF means that EOF was encountered in the
// middle of reading a fixed-size block or data structure.
public static error ErrUnexpectedEOF = errors.New("unexpected EOF"u8);

// ErrNoProgress is returned by some clients of a [Reader] when
// many calls to Read have failed to return any data or error,
// usually the sign of a broken [Reader] implementation.
public static error ErrNoProgress = errors.New("multiple Read calls return no data or error"u8);

// Reader is the interface that wraps the basic Read method.
//
// Read reads up to len(p) bytes into p. It returns the number of bytes
// read (0 <= n <= len(p)) and any error encountered. Even if Read
// returns n < len(p), it may use all of p as scratch space during the call.
// If some data is available but not len(p) bytes, Read conventionally
// returns what is available instead of waiting for more.
//
// When Read encounters an error or end-of-file condition after
// successfully reading n > 0 bytes, it returns the number of
// bytes read. It may return the (non-nil) error from the same call
// or return the error (and n == 0) from a subsequent call.
// An instance of this general case is that a Reader returning
// a non-zero number of bytes at the end of the input stream may
// return either err == EOF or err == nil. The next Read should
// return 0, EOF.
//
// Callers should always process the n > 0 bytes returned before
// considering the error err. Doing so correctly handles I/O errors
// that happen after reading some bytes and also both of the
// allowed EOF behaviors.
//
// If len(p) == 0, Read should always return n == 0. It may return a
// non-nil error if some error condition is known, such as EOF.
//
// Implementations of Read are discouraged from returning a
// zero byte count with a nil error, except when len(p) == 0.
// Callers should treat a return of 0 and nil as indicating that
// nothing happened; in particular it does not indicate EOF.
//
// Implementations must not retain p.
[GoType] partial interface Reader {
    (nint n, error err) Read(slice<byte> p);
}

// Writer is the interface that wraps the basic Write method.
//
// Write writes len(p) bytes from p to the underlying data stream.
// It returns the number of bytes written from p (0 <= n <= len(p))
// and any error encountered that caused the write to stop early.
// Write must return a non-nil error if it returns n < len(p).
// Write must not modify the slice data, even temporarily.
//
// Implementations must not retain p.
[GoType] partial interface Writer {
    (nint n, error err) Write(slice<byte> p);
}

// Closer is the interface that wraps the basic Close method.
//
// The behavior of Close after the first call is undefined.
// Specific implementations may document their own behavior.
[GoType] partial interface Closer {
    error Close();
}

// Seeker is the interface that wraps the basic Seek method.
//
// Seek sets the offset for the next Read or Write to offset,
// interpreted according to whence:
// [SeekStart] means relative to the start of the file,
// [SeekCurrent] means relative to the current offset, and
// [SeekEnd] means relative to the end
// (for example, offset = -2 specifies the penultimate byte of the file).
// Seek returns the new offset relative to the start of the
// file or an error, if any.
//
// Seeking to an offset before the start of the file is an error.
// Seeking to any positive offset may be allowed, but if the new offset exceeds
// the size of the underlying object the behavior of subsequent I/O operations
// is implementation-dependent.
[GoType] partial interface Seeker {
    (int64, error) Seek(int64 offset, nint whence);
}

// ReadWriter is the interface that groups the basic Read and Write methods.
[GoType] partial interface ReadWriter :
    Reader,
    Writer
{
}

// ReadCloser is the interface that groups the basic Read and Close methods.
[GoType] partial interface ReadCloser :
    Reader,
    Closer
{
}

// WriteCloser is the interface that groups the basic Write and Close methods.
[GoType] partial interface WriteCloser :
    Writer,
    Closer
{
}

// ReadWriteCloser is the interface that groups the basic Read, Write and Close methods.
[GoType] partial interface ReadWriteCloser :
    Reader,
    Writer,
    Closer
{
}

// ReadSeeker is the interface that groups the basic Read and Seek methods.
[GoType] partial interface ReadSeeker :
    Reader,
    Seeker
{
}

// ReadSeekCloser is the interface that groups the basic Read, Seek and Close
// methods.
[GoType] partial interface ReadSeekCloser :
    Reader,
    Seeker,
    Closer
{
}

// WriteSeeker is the interface that groups the basic Write and Seek methods.
[GoType] partial interface WriteSeeker :
    Writer,
    Seeker
{
}

// ReadWriteSeeker is the interface that groups the basic Read, Write and Seek methods.
[GoType] partial interface ReadWriteSeeker :
    Reader,
    Writer,
    Seeker
{
}

// ReaderFrom is the interface that wraps the ReadFrom method.
//
// ReadFrom reads data from r until EOF or error.
// The return value n is the number of bytes read.
// Any error except EOF encountered during the read is also returned.
//
// The [Copy] function uses [ReaderFrom] if available.
[GoType] partial interface ReaderFrom {
    (int64 n, error err) ReadFrom(Reader r);
}

// WriterTo is the interface that wraps the WriteTo method.
//
// WriteTo writes data to w until there's no more data to write or
// when an error occurs. The return value n is the number of bytes
// written. Any error encountered during the write is also returned.
//
// The Copy function uses WriterTo if available.
[GoType] partial interface WriterTo {
    (int64 n, error err) WriteTo(Writer w);
}

// ReaderAt is the interface that wraps the basic ReadAt method.
//
// ReadAt reads len(p) bytes into p starting at offset off in the
// underlying input source. It returns the number of bytes
// read (0 <= n <= len(p)) and any error encountered.
//
// When ReadAt returns n < len(p), it returns a non-nil error
// explaining why more bytes were not returned. In this respect,
// ReadAt is stricter than Read.
//
// Even if ReadAt returns n < len(p), it may use all of p as scratch
// space during the call. If some data is available but not len(p) bytes,
// ReadAt blocks until either all the data is available or an error occurs.
// In this respect ReadAt is different from Read.
//
// If the n = len(p) bytes returned by ReadAt are at the end of the
// input source, ReadAt may return either err == EOF or err == nil.
//
// If ReadAt is reading from an input source with a seek offset,
// ReadAt should not affect nor be affected by the underlying
// seek offset.
//
// Clients of ReadAt can execute parallel ReadAt calls on the
// same input source.
//
// Implementations must not retain p.
[GoType] partial interface ReaderAt {
    (nint n, error err) ReadAt(slice<byte> p, int64 off);
}

// WriterAt is the interface that wraps the basic WriteAt method.
//
// WriteAt writes len(p) bytes from p to the underlying data stream
// at offset off. It returns the number of bytes written from p (0 <= n <= len(p))
// and any error encountered that caused the write to stop early.
// WriteAt must return a non-nil error if it returns n < len(p).
//
// If WriteAt is writing to a destination with a seek offset,
// WriteAt should not affect nor be affected by the underlying
// seek offset.
//
// Clients of WriteAt can execute parallel WriteAt calls on the same
// destination if the ranges do not overlap.
//
// Implementations must not retain p.
[GoType] partial interface WriterAt {
    (nint n, error err) WriteAt(slice<byte> p, int64 off);
}

// ByteReader is the interface that wraps the ReadByte method.
//
// ReadByte reads and returns the next byte from the input or
// any error encountered. If ReadByte returns an error, no input
// byte was consumed, and the returned byte value is undefined.
//
// ReadByte provides an efficient interface for byte-at-time
// processing. A [Reader] that does not implement  ByteReader
// can be wrapped using bufio.NewReader to add this method.
[GoType] partial interface ByteReader {
    (byte, error) ReadByte();
}

// ByteScanner is the interface that adds the UnreadByte method to the
// basic ReadByte method.
//
// UnreadByte causes the next call to ReadByte to return the last byte read.
// If the last operation was not a successful call to ReadByte, UnreadByte may
// return an error, unread the last byte read (or the byte prior to the
// last-unread byte), or (in implementations that support the [Seeker] interface)
// seek to one byte before the current offset.
[GoType] partial interface ByteScanner :
    ByteReader
{
    error UnreadByte();
}

// ByteWriter is the interface that wraps the WriteByte method.
[GoType] partial interface ByteWriter {
    error WriteByte(byte c);
}

// RuneReader is the interface that wraps the ReadRune method.
//
// ReadRune reads a single encoded Unicode character
// and returns the rune and its size in bytes. If no character is
// available, err will be set.
[GoType] partial interface RuneReader {
    (rune r, nint size, error err) ReadRune();
}

// RuneScanner is the interface that adds the UnreadRune method to the
// basic ReadRune method.
//
// UnreadRune causes the next call to ReadRune to return the last rune read.
// If the last operation was not a successful call to ReadRune, UnreadRune may
// return an error, unread the last rune read (or the rune prior to the
// last-unread rune), or (in implementations that support the [Seeker] interface)
// seek to the start of the rune before the current offset.
[GoType] partial interface RuneScanner :
    RuneReader
{
    error UnreadRune();
}

// StringWriter is the interface that wraps the WriteString method.
[GoType] partial interface StringWriter {
    (nint n, error err) WriteString(@string s);
}

// WriteString writes the contents of the string s to w, which accepts a slice of bytes.
// If w implements [StringWriter], [StringWriter.WriteString] is invoked directly.
// Otherwise, [Writer.Write] is called exactly once.
public static (nint n, error err) WriteString(Writer w, @string s) {
    {
        var (sw, ok) = w._<StringWriter>(ᐧ); if (ok) {
            return sw.WriteString(s);
        }
    }
    return w.Write(slice<byte>(s));
}

// ReadAtLeast reads from r into buf until it has read at least min bytes.
// It returns the number of bytes copied and an error if fewer bytes were read.
// The error is EOF only if no bytes were read.
// If an EOF happens after reading fewer than min bytes,
// ReadAtLeast returns [ErrUnexpectedEOF].
// If min is greater than the length of buf, ReadAtLeast returns [ErrShortBuffer].
// On return, n >= min if and only if err == nil.
// If r returns an error having read at least min bytes, the error is dropped.
public static (nint n, error err) ReadAtLeast(Reader r, slice<byte> buf, nint min) {
    nint n = default!;
    error err = default!;

    if (len(buf) < min) {
        return (0, ErrShortBuffer);
    }
    while (n < min && err == default!) {
        nint nn = default!;
        (nn, err) = r.Read(buf.slice(n));
        n += nn;
    }
    if (n >= min){
        err = default!;
    } else 
    if (n > 0 && AreEqual(err, EOF)) {
        err = ErrUnexpectedEOF;
    }
    return (n, err);
}

// ReadFull reads exactly len(buf) bytes from r into buf.
// It returns the number of bytes copied and an error if fewer bytes were read.
// The error is EOF only if no bytes were read.
// If an EOF happens after reading some but not all the bytes,
// ReadFull returns [ErrUnexpectedEOF].
// On return, n == len(buf) if and only if err == nil.
// If r returns an error having read at least len(buf) bytes, the error is dropped.
public static (nint n, error err) ReadFull(Reader r, slice<byte> buf) {
    return ReadAtLeast(r, buf, len(buf));
}

// CopyN copies n bytes (or until an error) from src to dst.
// It returns the number of bytes copied and the earliest
// error encountered while copying.
// On return, written == n if and only if err == nil.
//
// If dst implements [ReaderFrom], the copy is implemented using it.
public static (int64 written, error err) CopyN(Writer dst, Reader src, int64 n) {
    int64 written = default!;
    error err = default!;

    (written, err) = Copy(dst, LimitReader(src, n));
    if (written == n) {
        return (n, default!);
    }
    if (written < n && err == default!) {
        // src stopped early; must have been EOF.
        err = EOF;
    }
    return (written, err);
}

// Copy copies from src to dst until either EOF is reached
// on src or an error occurs. It returns the number of bytes
// copied and the first error encountered while copying, if any.
//
// A successful Copy returns err == nil, not err == EOF.
// Because Copy is defined to read from src until EOF, it does
// not treat an EOF from Read as an error to be reported.
//
// If src implements [WriterTo],
// the copy is implemented by calling src.WriteTo(dst).
// Otherwise, if dst implements [ReaderFrom],
// the copy is implemented by calling dst.ReadFrom(src).
public static (int64 written, error err) Copy(Writer dst, Reader src) {
    return copyBuffer(dst, src, default!);
}

// CopyBuffer is identical to Copy except that it stages through the
// provided buffer (if one is required) rather than allocating a
// temporary one. If buf is nil, one is allocated; otherwise if it has
// zero length, CopyBuffer panics.
//
// If either src implements [WriterTo] or dst implements [ReaderFrom],
// buf will not be used to perform the copy.
public static (int64 written, error err) CopyBuffer(Writer dst, Reader src, slice<byte> buf) {
    if (buf != default! && len(buf) == 0) {
        throw panic("empty buffer in CopyBuffer");
    }
    return copyBuffer(dst, src, buf);
}

// copyBuffer is the actual implementation of Copy and CopyBuffer.
// if buf is nil, one is allocated.
internal static (int64 written, error err) copyBuffer(Writer dst, Reader src, slice<byte> buf) {
    int64 written = default!;
    error err = default!;

    // If the reader has a WriteTo method, use it to do the copy.
    // Avoids an allocation and a copy.
    {
        var (wt, ok) = src._<WriterTo>(ᐧ); if (ok) {
            return wt.WriteTo(dst);
        }
    }
    // Similarly, if the writer has a ReadFrom method, use it to do the copy.
    {
        var (rf, ok) = dst._<ReaderFrom>(ᐧ); if (ok) {
            return rf.ReadFrom(src);
        }
    }
    if (buf == default!) {
        nint size = 32 * 1024;
        {
            var (l, ok) = src._<ж<LimitedReader>>(ᐧ); if (ok && (int64)size > (~l).N) {
                if ((~l).N < 1){
                    size = 1;
                } else {
                    size = (nint)(~l).N;
                }
            }
        }
        buf = new slice<byte>(size);
    }
    while (ᐧ) {
        var (nr, er) = src.Read(buf);
        if (nr > 0) {
            var (nw, ew) = dst.Write(buf.slice(0, nr));
            if (nw < 0 || nr < nw) {
                nw = 0;
                if (ew == default!) {
                    ew = errInvalidWrite;
                }
            }
            written += (int64)nw;
            if (ew != default!) {
                err = ew;
                break;
            }
            if (nr != nw) {
                err = ErrShortWrite;
                break;
            }
        }
        if (er != default!) {
            if (!AreEqual(er, EOF)) {
                err = er;
            }
            break;
        }
    }
    return (written, err);
}

// LimitReader returns a Reader that reads from r
// but stops with EOF after n bytes.
// The underlying implementation is a *LimitedReader.
public static Reader LimitReader(Reader r, int64 n) {
    return new LimitedReaderжReader(Ꮡ(new LimitedReader(r, n)));
}

// A LimitedReader reads from R but limits the amount of
// data returned to just N bytes. Each call to Read
// updates N to reflect the new amount remaining.
// Read returns EOF when N <= 0 or when the underlying R returns EOF.
[GoType] partial struct LimitedReader {
    public Reader R; // underlying reader
    public int64 N;  // max bytes remaining
}

[GoRecv] public static (nint n, error err) Read(this ref LimitedReader l, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (l.N <= 0) {
        return (0, EOF);
    }
    if ((int64)len(p) > l.N) {
        p = p.slice(0, (nint)(l.N));
    }
    (n, err) = l.R.Read(p);
    l.N -= (int64)n;
    return (n, err);
}

// NewSectionReader returns a [SectionReader] that reads from r
// starting at offset off and stops with EOF after n bytes.
public static ж<SectionReader> NewSectionReader(ReaderAt r, int64 off, int64 n) {
    ref var remaining = ref heap(new int64(), out var Ꮡremaining);
    const int64 maxint64 = /* 1<<63 - 1 */ 9223372036854775807;
    if (off <= maxint64 - n){
        remaining = n + off;
    } else {
        // Overflow, with no way to return error.
        // Assume we can read up to an offset of 1<<63 - 1.
        remaining = maxint64;
    }
    return Ꮡ(new SectionReader(r, off, off, remaining, n));
}

// SectionReader implements Read, Seek, and ReadAt on a section
// of an underlying [ReaderAt].
[GoType] partial struct SectionReader {
    internal ReaderAt r; // constant after creation
    internal int64 @base;    // constant after creation
    internal int64 off;
    internal int64 limit; // constant after creation
    internal int64 n; // constant after creation
}

[GoRecv] public static (nint n, error err) Read(this ref SectionReader s, slice<byte> p) {
    nint n = default!;
    error err = default!;

    if (s.off >= s.limit) {
        return (0, EOF);
    }
    {
        var max = s.limit - s.off; if ((int64)len(p) > max) {
            p = p.slice(0, (nint)(max));
        }
    }
    (n, err) = s.r.ReadAt(p, s.off);
    s.off += (int64)n;
    return (n, err);
}

internal static error errWhence = errors.New("Seek: invalid whence"u8);

internal static error errOffset = errors.New("Seek: invalid offset"u8);

[GoRecv] public static (int64, error) Seek(this ref SectionReader s, int64 offset, nint whence) {
    var exprᴛ1 = whence;
    if (exprᴛ1 == SeekStart) {
        offset += s.@base;
    }
    else if (exprᴛ1 == SeekCurrent) {
        offset += s.off;
    }
    else if (exprᴛ1 == SeekEnd) {
        offset += s.limit;
    }
    else { /* default: */
        return (0, errWhence);
    }

    if (offset < s.@base) {
        return (0, errOffset);
    }
    s.off = offset;
    return (offset - s.@base, default!);
}

[GoRecv] public static (nint n, error err) ReadAt(this ref SectionReader s, slice<byte> p, int64 off) {
    nint n = default!;
    error err = default!;

    if (off < 0 || off >= s.Size()) {
        return (0, EOF);
    }
    off += s.@base;
    {
        var max = s.limit - off; if ((int64)len(p) > max) {
            p = p.slice(0, (nint)(max));
            (n, err) = s.r.ReadAt(p, off);
            if (err == default!) {
                err = EOF;
            }
            return (n, err);
        }
    }
    return s.r.ReadAt(p, off);
}

// Size returns the size of the section in bytes.
[GoRecv] public static int64 Size(this ref SectionReader s) {
    return s.limit - s.@base;
}

// Outer returns the underlying [ReaderAt] and offsets for the section.
//
// The returned values are the same that were passed to [NewSectionReader]
// when the [SectionReader] was created.
[GoRecv] public static (ReaderAt r, int64 off, int64 n) Outer(this ref SectionReader s) {
    return (s.r, s.@base, s.n);
}

// An OffsetWriter maps writes at offset base to offset base+off in the underlying writer.
[GoType] partial struct OffsetWriter {
    internal WriterAt w;
    internal int64 @base; // the original offset
    internal int64 off; // the current offset
}

// NewOffsetWriter returns an [OffsetWriter] that writes to w
// starting at offset off.
public static ж<OffsetWriter> NewOffsetWriter(WriterAt w, int64 off) {
    return Ꮡ(new OffsetWriter(w, off, off));
}

[GoRecv] public static (nint n, error err) Write(this ref OffsetWriter o, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = o.w.WriteAt(p, o.off);
    o.off += (int64)n;
    return (n, err);
}

[GoRecv] public static (nint n, error err) WriteAt(this ref OffsetWriter o, slice<byte> p, int64 off) {
    if (off < 0) {
        return (0, errOffset);
    }
    off += o.@base;
    return o.w.WriteAt(p, off);
}

[GoRecv] public static (int64, error) Seek(this ref OffsetWriter o, int64 offset, nint whence) {
    var exprᴛ1 = whence;
    if (exprᴛ1 == SeekStart) {
        offset += o.@base;
    }
    else if (exprᴛ1 == SeekCurrent) {
        offset += o.off;
    }
    else { /* default: */
        return (0, errWhence);
    }

    if (offset < o.@base) {
        return (0, errOffset);
    }
    o.off = offset;
    return (offset - o.@base, default!);
}

// TeeReader returns a [Reader] that writes to w what it reads from r.
// All reads from r performed through it are matched with
// corresponding writes to w. There is no internal buffering -
// the write must complete before the read completes.
// Any error encountered while writing is reported as a read error.
public static Reader TeeReader(Reader r, Writer w) {
    return new teeReaderжReader(Ꮡ(new teeReader(r, w)));
}

[GoType] partial struct teeReader {
    internal Reader r;
    internal Writer w;
}

[GoRecv] internal static (nint n, error err) Read(this ref teeReader t, slice<byte> p) {
    nint n = default!;
    error err = default!;

    (n, err) = t.r.Read(p);
    if (n > 0) {
        {
            var (nΔ1, errΔ1) = t.w.Write(p.slice(0, n)); if (errΔ1 != default!) {
                return (nΔ1, errΔ1);
            }
        }
    }
    return (n, err);
}

// Discard is a [Writer] on which all Write calls succeed
// without doing anything.
public static Writer Discard = new discard(nil);

[GoType] partial struct discard {
}

// discard implements ReaderFrom as an optimization so Copy to
// io.Discard can avoid doing unnecessary work.
internal static ReaderFrom _ᴛ1ʗ = new discard(nil);

internal static (nint, error) Write(this discard _, slice<byte> p) {
    return (len(p), default!);
}

internal static (nint, error) WriteString(this discard _, @string s) {
    return (len(s), default!);
}

internal static ж<Δsync.Pool> ᏑblackHolePool = new StandardBox<Δsync.Pool>(new Δsync.Pool(
    New: () => {
        var b = new slice<byte>(8192);
        return Ꮡ(b);
    }
));
internal static ref Δsync.Pool blackHolePool => ref ᏑblackHolePool.Value;

internal static (int64 n, error err) ReadFrom(this discard _, Reader r) {
    int64 n = default!;
    error err = default!;

    var bufp = ᏑblackHolePool.Get()._<ж<slice<byte>>>();
    nint readSize = 0;
    while (ᐧ) {
        (readSize, err) = r.Read(bufp.ValueSlot);
        n += (int64)readSize;
        if (err != default!) {
            ᏑblackHolePool.Put(bufp.OrTypedNil());
            if (AreEqual(err, EOF)) {
                return (n, default!);
            }
            return (n, err);
        }
    }
}

// NopCloser returns a [ReadCloser] with a no-op Close method wrapping
// the provided [Reader] r.
// If r implements [WriterTo], the returned [ReadCloser] will implement [WriterTo]
// by forwarding calls to r.
public static ReadCloser NopCloser(Reader r) {
    {
        var (_, ok) = r._<WriterTo>(ᐧ); if (ok) {
            return new nopCloserWriterTo(r);
        }
    }
    return new nopCloser(r);
}

[GoType] partial struct nopCloser {
    public Reader Reader;
}

internal static error Close(this nopCloser _) {
    return default!;
}

[GoType] partial struct nopCloserWriterTo {
    public Reader Reader;
}

internal static error Close(this nopCloserWriterTo _) {
    return default!;
}

internal static (int64 n, error err) WriteTo(this nopCloserWriterTo c, Writer w) {
    return c.Reader._<WriterTo>().WriteTo(w);
}

// ReadAll reads from r until an error or EOF and returns the data it read.
// A successful call returns err == nil, not err == EOF. Because ReadAll is
// defined to read from src until EOF, it does not treat an EOF from Read
// as an error to be reported.
public static (slice<byte>, error) ReadAll(Reader r) {
    var b = new slice<byte>(0, 512);
    while (ᐧ) {
        var (n, err) = r.Read(b.slice(len(b), cap(b)));
        b = b.slice(0, len(b) + n);
        if (err != default!) {
            if (AreEqual(err, EOF)) {
                err = default!;
            }
            return (b, err);
        }
        if (len(b) == cap(b)) {
            // Add more capacity (let append pick how much).
            b = append(b, (byte)(0)).slice(0, len(b));
        }
    }
}

} // end io_package
