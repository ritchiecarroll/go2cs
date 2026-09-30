// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// Simple byte buffer for marshaling data.
using errors = errors_package;
using io = io_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class bytes_package {

// smallBufferSize is an initial allocation minimal capacity.
internal static UntypedInt smallBufferSize => 64;

// A Buffer is a variable-sized buffer of bytes with [Buffer.Read] and [Buffer.Write] methods.
// The zero value for Buffer is an empty buffer ready to use.
[GoType] partial struct Buffer {
    internal slice<byte> buf; // contents are the bytes buf[off : len(buf)]
    internal nint off;   // read at &buf[off], write at &buf[len(buf)]
    internal readOp lastRead; // last read operation, so that Unread* can work correctly.
}

[GoType("num:int8")] partial struct readOp;

// Don't use iota for these, as the values need to correspond with the
// names and comments, which is easier to see when being explicit.
internal static readOp opRead => -1;   // Any other read operation.

internal static readOp opInvalid => 0; // Non-read operation.

internal static readOp opReadRune1 => 1; // Read rune of size 1.

internal static readOp opReadRune2 => 2; // Read rune of size 2.

internal static readOp opReadRune3 => 3; // Read rune of size 3.

internal static readOp opReadRune4 => 4; // Read rune of size 4.

// ErrTooLarge is passed to panic if memory cannot be allocated to store data in a buffer.
public static error ErrTooLarge = errors.New("bytes.Buffer: too large"u8);

internal static error errNegativeRead = errors.New("bytes.Buffer: reader returned negative count from Read"u8);

internal static nint maxInt => /* int(^uint(0) >> 1) */ unchecked((nint)9223372036854775807);

// Bytes returns a slice of length b.Len() holding the unread portion of the buffer.
// The slice is valid for use only until the next buffer modification (that is,
// only until the next call to a method like [Buffer.Read], [Buffer.Write], [Buffer.Reset], or [Buffer.Truncate]).
// The slice aliases the buffer content at least until the next buffer modification,
// so immediate changes to the slice will affect the result of future reads.
[GoRecv] public static slice<byte> Bytes(this ref Buffer b) {
    return b.buf.slice(b.off);
}

// AvailableBuffer returns an empty buffer with b.Available() capacity.
// This buffer is intended to be appended to and
// passed to an immediately succeeding [Buffer.Write] call.
// The buffer is only valid until the next write operation on b.
[GoRecv] public static slice<byte> AvailableBuffer(this ref Buffer b) {
    return b.buf.slice(len(b.buf));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "<nil>"u8;

// String returns the contents of the unread portion of the buffer
// as a string. If the [Buffer] is a nil pointer, it returns "<nil>".
//
// To build strings more efficiently, see the [strings.Builder] type.
public static @string String(this ж<Buffer> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (Ꮡb == nil) {
        // Special case, useful in debugging.
        return nilˢ;
    }
    return ((@string)(b.buf.slice(b.off)));
}

// empty reports whether the unread portion of the buffer is empty.
[GoRecv] internal static bool empty(this ref Buffer b) {
    return len(b.buf) <= b.off;
}

// Len returns the number of bytes of the unread portion of the buffer;
// b.Len() == len(b.Bytes()).
[GoRecv] public static nint Len(this ref Buffer b) {
    return len(b.buf) - b.off;
}

// Cap returns the capacity of the buffer's underlying byte slice, that is, the
// total space allocated for the buffer's data.
[GoRecv] public static nint Cap(this ref Buffer b) {
    return cap(b.buf);
}

// Available returns how many bytes are unused in the buffer.
[GoRecv] public static nint Available(this ref Buffer b) {
    return cap(b.buf) - len(b.buf);
}

// Truncate discards all but the first n unread bytes from the buffer
// but continues to use the same allocated storage.
// It panics if n is negative or greater than the length of the buffer.
[GoRecv] public static void Truncate(this ref Buffer b, nint n) {
    if (n == 0) {
        b.Reset();
        return;
    }
    b.lastRead = opInvalid;
    if (n < 0 || n > b.Len()) {
        throw panic("bytes.Buffer: truncation out of range");
    }
    b.buf = b.buf.slice(0, b.off + n);
}

// Reset resets the buffer to be empty,
// but it retains the underlying storage for use by future writes.
// Reset is the same as [Buffer.Truncate](0).
[GoRecv] public static void Reset(this ref Buffer b) {
    b.buf = b.buf[..0];
    b.off = 0;
    b.lastRead = opInvalid;
}

// tryGrowByReslice is an inlineable version of grow for the fast-case where the
// internal buffer only needs to be resliced.
// It returns the index where bytes should be written and whether it succeeded.
[GoRecv] internal static (nint, bool) tryGrowByReslice(this ref Buffer b, nint n) {
    {
        nint l = len(b.buf); if (n <= cap(b.buf) - l) {
            b.buf = b.buf.slice(0, l + n);
            return (l, true);
        }
    }
    return (0, false);
}

// grow grows the buffer to guarantee space for n more bytes.
// It returns the index where bytes should be written.
// If the buffer can't grow it will panic with ErrTooLarge.
[GoRecv] internal static nint grow(this ref Buffer b, nint n) {
    nint m = b.Len();
    // If buffer is empty, reset to recover space.
    if (m == 0 && b.off != 0) {
        b.Reset();
    }
    // Try to grow by means of a reslice.
    {
        var (i, ok) = b.tryGrowByReslice(n); if (ok) {
            return i;
        }
    }
    if (b.buf == default! && n <= smallBufferSize) {
        b.buf = new slice<byte>(n, smallBufferSize);
        return 0;
    }
    nint c = cap(b.buf);
    if (n <= c / 2 - m){
        // We can slide things down instead of allocating a new
        // slice. We only need m+n <= c to slide, but
        // we instead let capacity get twice as large so we
        // don't spend all our time copying.
        copy(b.buf, b.buf.slice(b.off));
    } else 
    if (c > maxInt - c - n){
        throw panic(ErrTooLarge);
    } else {
        // Add b.off to account for b.buf[:b.off] being sliced off the front.
        b.buf = growSlice(b.buf.slice(b.off), b.off + n);
    }
    // Restore b.off and len(b.buf).
    b.off = 0;
    b.buf = b.buf.slice(0, m + n);
    return m;
}

// Grow grows the buffer's capacity, if necessary, to guarantee space for
// another n bytes. After Grow(n), at least n bytes can be written to the
// buffer without another allocation.
// If n is negative, Grow will panic.
// If the buffer can't grow it will panic with [ErrTooLarge].
[GoRecv] public static void Grow(this ref Buffer b, nint n) {
    if (n < 0) {
        throw panic("bytes.Buffer.Grow: negative count");
    }
    nint m = b.grow(n);
    b.buf = b.buf.slice(0, m);
}

// Write appends the contents of p to the buffer, growing the buffer as
// needed. The return value n is the length of p; err is always nil. If the
// buffer becomes too large, Write will panic with [ErrTooLarge].
[GoRecv] public static (nint n, error err) Write(this ref Buffer b, slice<byte> p) {
    b.lastRead = opInvalid;
    var (m, ok) = b.tryGrowByReslice(len(p));
    if (!ok) {
        m = b.grow(len(p));
    }
    return (copy(b.buf.slice(m), p), default!);
}

// WriteString appends the contents of s to the buffer, growing the buffer as
// needed. The return value n is the length of s; err is always nil. If the
// buffer becomes too large, WriteString will panic with [ErrTooLarge].
[GoRecv] public static (nint n, error err) WriteString(this ref Buffer b, @string s) {
    b.lastRead = opInvalid;
    var (m, ok) = b.tryGrowByReslice(len(s));
    if (!ok) {
        m = b.grow(len(s));
    }
    return (copy(b.buf.slice(m), s), default!);
}

// MinRead is the minimum slice size passed to a [Buffer.Read] call by
// [Buffer.ReadFrom]. As long as the [Buffer] has at least MinRead bytes beyond
// what is required to hold the contents of r, [Buffer.ReadFrom] will not grow the
// underlying buffer.
public static UntypedInt MinRead => 512;

// ReadFrom reads data from r until EOF and appends it to the buffer, growing
// the buffer as needed. The return value n is the number of bytes read. Any
// error except io.EOF encountered during the read is also returned. If the
// buffer becomes too large, ReadFrom will panic with [ErrTooLarge].
[GoRecv] public static (int64 n, error err) ReadFrom(this ref Buffer b, io.Reader r) {
    int64 n = default!;

    b.lastRead = opInvalid;
    while (ᐧ) {
        nint i = b.grow(MinRead);
        b.buf = b.buf.slice(0, i);
        var (m, e) = r.Read(b.buf.slice(i, cap(b.buf)));
        if (m < 0) {
            throw panic(errNegativeRead);
        }
        b.buf = b.buf.slice(0, i + m);
        n += (int64)m;
        if (AreEqual(e, io.EOF)) {
            return (n, default!); // e is EOF, so return nil explicitly
        }
        if (e != default!) {
            return (n, e);
        }
    }
}

// growSlice grows b by n, preserving the original content of b.
// If the allocation fails, it panics with ErrTooLarge.
internal static slice<byte> growSlice(slice<byte> b, nint n) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() != default!) {
                throw panic(ErrTooLarge);
            }
        }, ref ᒐ);
        // TODO(http://golang.org/issue/51462): We should rely on the append-make
        // pattern so that the compiler can call runtime.growslice. For example:
        //	return append(b, make([]byte, n)...)
        // This avoids unnecessary zero-ing of the first len(b) bytes of the
        // allocated slice, but this pattern causes b to escape onto the heap.
        //
        // Instead use the append-make pattern with a nil slice to ensure that
        // we allocate buffers rounded up to the closest size class.
        nint c = len(b) + n; // ensure enough space for n elements
        if (c < 2 * cap(b)) {
            // The growth rate has historically always been 2x. In the future,
            // we could rely purely on append to determine the growth rate.
            c = 2 * cap(b);
        }
        var b2 = appendꓸꓸꓸ(slice<byte>(default!), makeꓸꓸꓸ<byte>(c));
        nint i = copy(b2, b);
        return b2.slice(0, i);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// WriteTo writes data to w until the buffer is drained or an error occurs.
// The return value n is the number of bytes written; it always fits into an
// int, but it is int64 to match the [io.WriterTo] interface. Any error
// encountered during the write is also returned.
[GoRecv] public static (int64 n, error err) WriteTo(this ref Buffer b, io.Writer w) {
    int64 n = default!;

    b.lastRead = opInvalid;
    {
        nint nBytes = b.Len(); if (nBytes > 0) {
            var (m, e) = w.Write(b.buf.slice(b.off));
            if (m > nBytes) {
                throw panic("bytes.Buffer.WriteTo: invalid Write count");
            }
            b.off += m;
            n = (int64)m;
            if (e != default!) {
                return (n, e);
            }
            // all bytes should have been written, by definition of
            // Write method in io.Writer
            if (m != nBytes) {
                return (n, io.ErrShortWrite);
            }
        }
    }
    // Buffer is now empty; reset.
    b.Reset();
    return (n, default!);
}

// WriteByte appends the byte c to the buffer, growing the buffer as needed.
// The returned error is always nil, but is included to match [bufio.Writer]'s
// WriteByte. If the buffer becomes too large, WriteByte will panic with
// [ErrTooLarge].
[GoRecv] public static error WriteByte(this ref Buffer b, byte c) {
    b.lastRead = opInvalid;
    var (m, ok) = b.tryGrowByReslice(1);
    if (!ok) {
        m = b.grow(1);
    }
    b.buf[m] = c;
    return default!;
}

// WriteRune appends the UTF-8 encoding of Unicode code point r to the
// buffer, returning its length and an error, which is always nil but is
// included to match [bufio.Writer]'s WriteRune. The buffer is grown as needed;
// if it becomes too large, WriteRune will panic with [ErrTooLarge].
[GoRecv] public static (nint n, error err) WriteRune(this ref Buffer b, rune r) {
    // Compare as uint32 to correctly handle negative runes.
    if ((uint32)r < utf8.RuneSelf) {
        b.WriteByte((byte)r);
        return (1, default!);
    }
    b.lastRead = opInvalid;
    var (m, ok) = b.tryGrowByReslice(utf8.UTFMax);
    if (!ok) {
        m = b.grow(utf8.UTFMax);
    }
    b.buf = utf8.AppendRune(b.buf.slice(0, m), r);
    return (len(b.buf) - m, default!);
}

// Read reads the next len(p) bytes from the buffer or until the buffer
// is drained. The return value n is the number of bytes read. If the
// buffer has no data to return, err is [io.EOF] (unless len(p) is zero);
// otherwise it is nil.
[GoRecv] public static (nint n, error err) Read(this ref Buffer b, slice<byte> p) {
    nint n = default!;

    b.lastRead = opInvalid;
    if (b.empty()) {
        // Buffer is empty, reset to recover space.
        b.Reset();
        if (len(p) == 0) {
            return (0, default!);
        }
        return (0, io.EOF);
    }
    n = copy(p, b.buf.slice(b.off));
    b.off += n;
    if (n > 0) {
        b.lastRead = opRead;
    }
    return (n, default!);
}

// Next returns a slice containing the next n bytes from the buffer,
// advancing the buffer as if the bytes had been returned by [Buffer.Read].
// If there are fewer than n bytes in the buffer, Next returns the entire buffer.
// The slice is only valid until the next call to a read or write method.
[GoRecv] public static slice<byte> Next(this ref Buffer b, nint n) {
    b.lastRead = opInvalid;
    nint m = b.Len();
    if (n > m) {
        n = m;
    }
    var data = b.buf.slice(b.off, b.off + n);
    b.off += n;
    if (n > 0) {
        b.lastRead = opRead;
    }
    return data;
}

// ReadByte reads and returns the next byte from the buffer.
// If no byte is available, it returns error [io.EOF].
[GoRecv] public static (byte, error) ReadByte(this ref Buffer b) {
    if (b.empty()) {
        // Buffer is empty, reset to recover space.
        b.Reset();
        return (0, io.EOF);
    }
    var c = b.buf[b.off];
    b.off++;
    b.lastRead = opRead;
    return (c, default!);
}

// ReadRune reads and returns the next UTF-8-encoded
// Unicode code point from the buffer.
// If no bytes are available, the error returned is io.EOF.
// If the bytes are an erroneous UTF-8 encoding, it
// consumes one byte and returns U+FFFD, 1.
[GoRecv] public static (rune r, nint size, error err) ReadRune(this ref Buffer b) {
    rune r = default!;

    if (b.empty()) {
        // Buffer is empty, reset to recover space.
        b.Reset();
        return (0, 0, io.EOF);
    }
    var c = b.buf[b.off];
    if (c < utf8.RuneSelf) {
        b.off++;
        b.lastRead = opReadRune1;
        return ((rune)c, 1, default!);
    }
    (r, var n) = utf8.DecodeRune(b.buf.slice(b.off));
    b.off += n;
    b.lastRead = ((readOp)(int8)n);
    return (r, n, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string bytesBufferUnreadRuneˢ = "bytes.Buffer: UnreadRune: previous operation was not a successful ReadRune"u8;

// UnreadRune unreads the last rune returned by [Buffer.ReadRune].
// If the most recent read or write operation on the buffer was
// not a successful [Buffer.ReadRune], UnreadRune returns an error.  (In this regard
// it is stricter than [Buffer.UnreadByte], which will unread the last byte
// from any read operation.)
[GoRecv] public static error UnreadRune(this ref Buffer b) {
    if (b.lastRead <= opInvalid) {
        return errors.New(bytesBufferUnreadRuneˢ);
    }
    if (b.off >= (nint)(int8)b.lastRead) {
        b.off -= (nint)(int8)b.lastRead;
    }
    b.lastRead = opInvalid;
    return default!;
}

internal static error errUnreadByte = errors.New("bytes.Buffer: UnreadByte: previous operation was not a successful read"u8);

// UnreadByte unreads the last byte returned by the most recent successful
// read operation that read at least one byte. If a write has happened since
// the last read, if the last read returned an error, or if the read read zero
// bytes, UnreadByte returns an error.
[GoRecv] public static error UnreadByte(this ref Buffer b) {
    if (b.lastRead == opInvalid) {
        return errUnreadByte;
    }
    b.lastRead = opInvalid;
    if (b.off > 0) {
        b.off--;
    }
    return default!;
}

// ReadBytes reads until the first occurrence of delim in the input,
// returning a slice containing the data up to and including the delimiter.
// If ReadBytes encounters an error before finding a delimiter,
// it returns the data read before the error and the error itself (often [io.EOF]).
// ReadBytes returns err != nil if and only if the returned data does not end in
// delim.
[GoRecv] public static (slice<byte> line, error err) ReadBytes(this ref Buffer b, byte delim) {
    slice<byte> line = default!;
    error err = default!;

    (var Δslice, err) = b.readSlice(delim);
    // return a copy of slice. The buffer's backing array may
    // be overwritten by later calls.
    line = appendꓸꓸꓸ(line, Δslice);
    return (line, err);
}

// readSlice is like ReadBytes but returns a reference to internal buffer data.
[GoRecv] internal static (slice<byte> line, error err) readSlice(this ref Buffer b, byte delim) {
    slice<byte> line = default!;
    error err = default!;

    nint i = IndexByte(b.buf.slice(b.off), delim);
    nint end = b.off + i + 1;
    if (i < 0) {
        end = len(b.buf);
        err = io.EOF;
    }
    line = b.buf.slice(b.off, end);
    b.off = end;
    b.lastRead = opRead;
    return (line, err);
}

// ReadString reads until the first occurrence of delim in the input,
// returning a string containing the data up to and including the delimiter.
// If ReadString encounters an error before finding a delimiter,
// it returns the data read before the error and the error itself (often [io.EOF]).
// ReadString returns err != nil if and only if the returned data does not end
// in delim.
[GoRecv] public static (@string line, error err) ReadString(this ref Buffer b, byte delim) {
    error err = default!;

    (var Δslice, err) = b.readSlice(delim);
    return (((@string)Δslice), err);
}

// NewBuffer creates and initializes a new [Buffer] using buf as its
// initial contents. The new [Buffer] takes ownership of buf, and the
// caller should not use buf after this call. NewBuffer is intended to
// prepare a [Buffer] to read existing data. It can also be used to set
// the initial size of the internal buffer for writing. To do that,
// buf should have the desired capacity but a length of zero.
//
// In most cases, new([Buffer]) (or just declaring a [Buffer] variable) is
// sufficient to initialize a [Buffer].
public static ж<Buffer> NewBuffer(slice<byte> buf) {
    return Ꮡ(new Buffer(buf: buf));
}

// NewBufferString creates and initializes a new [Buffer] using string s as its
// initial contents. It is intended to prepare a buffer to read an existing
// string.
//
// In most cases, new([Buffer]) (or just declaring a [Buffer] variable) is
// sufficient to initialize a [Buffer].
public static ж<Buffer> NewBufferString(@string s) {
    return Ꮡ(new Buffer(buf: slice<byte>(s)));
}

} // end bytes_package
