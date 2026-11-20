// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using io = io_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using static bytes_package;

partial class bytes_package {

// A Reader implements the [io.Reader], [io.ReaderAt], [io.WriterTo], [io.Seeker],
// [io.ByteScanner], and [io.RuneScanner] interfaces by reading from
// a byte slice.
// Unlike a [Buffer], a Reader is read-only and supports seeking.
// The zero value for Reader operates like a Reader of an empty slice.
partial struct Reader {
    internal slice<byte> s;
    internal int64 i;       // current reading index
    internal nint prevRune; // index of previous rune; or < 0
}

} partial class bytes_package {

// Len returns the number of bytes of the unread portion of the
// slice.
internal static nint Lenᴍ(ref Reader r) {
    if (r.i >= (int64)len(r.s)) {
        return 0;
    }
    return (nint)((int64)len(r.s) - r.i);
}

// Size returns the original length of the underlying byte slice.
// Size is the number of bytes available for reading via [Reader.ReadAt].
// The result is unaffected by any method calls except [Reader.Reset].
internal static int64 Sizeᴍ(ref Reader r) { return (int64)len(r.s); }

// Read implements the [io.Reader] interface.
internal static (nint n, error err) Readᴍ(ref Reader r, slice<byte> b) {
    nint n = default;
    error err = default;

    if (r.i >= (int64)len(r.s)) {
        return (0, io.EOF);
    }
    r.prevRune = -1;
    n = copy(b, r.s.slice((nint)(r.i)));
    r.i += (int64)n;
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bytesReaderReadAtˢᴍReader => ref bytesReaderReadAtˢᵇᴍReader;
private static readonly @string bytesReaderReadAtˢᵇᴍReader = "bytes.Reader.ReadAt: negative offset"u8;

// ReadAt implements the [io.ReaderAt] interface.
internal static (nint n, error err) ReadAtᴍ(ref Reader r, slice<byte> b, int64 off) {
    nint n = default;
    error err = default;

    // cannot modify state - see io.ReaderAt
    if (off < 0) {
        return (0, errors.New(bytesReaderReadAtˢᴍReader));
    }
    if (off >= (int64)len(r.s)) {
        return (0, io.EOF);
    }
    n = copy(b, r.s.slice((nint)(off)));
    if (n < len(b)) {
        err = io.EOF;
    }
    return (n, err);
}

// ReadByte implements the [io.ByteReader] interface.
internal static (byte, error) ReadByteᴍ(ref Reader r) {
    r.prevRune = -1;
    if (r.i >= (int64)len(r.s)) {
        return (0, io.EOF);
    }
    var b = r.s[(nint)(r.i)];
    r.i++;
    return (b, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bytesReaderUnreadByteAtˢᴍReader => ref bytesReaderUnreadByteAtˢᵇᴍReader;
private static readonly @string bytesReaderUnreadByteAtˢᵇᴍReader = "bytes.Reader.UnreadByte: at beginning of slice"u8;

// UnreadByte complements [Reader.ReadByte] in implementing the [io.ByteScanner] interface.
internal static error UnreadByteᴍ(ref Reader r) {
    if (r.i <= 0) {
        return errors.New(bytesReaderUnreadByteAtˢᴍReader);
    }
    r.prevRune = -1;
    r.i--;
    return default;
}

// ReadRune implements the [io.RuneReader] interface.
internal static (rune ch, nint size, error err) ReadRuneᴍ(ref Reader r) {
    rune ch = default;
    nint size = default;
    error err = default;

    if (r.i >= (int64)len(r.s)) {
        r.prevRune = -1;
        return (0, 0, io.EOF);
    }
    r.prevRune = (nint)r.i;
    var c = r.s[(nint)(r.i)]; if (c < utf8.RuneSelf) {
        r.i++;
        return ((rune)c, 1, default);
    }
    (ch, size) = utf8.DecodeRune(r.s.slice((nint)(r.i)));
    r.i += (int64)size;
    return (ch, size, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bytesReaderUnreadRuneAtˢᴍReader => ref bytesReaderUnreadRuneAtˢᵇᴍReader;
private static readonly @string bytesReaderUnreadRuneAtˢᵇᴍReader = "bytes.Reader.UnreadRune: at beginning of slice"u8;
internal static ref readonly @string bytesReaderUnreadRuneˢᴍReader => ref bytesReaderUnreadRuneˢᵇᴍReader;
private static readonly @string bytesReaderUnreadRuneˢᵇᴍReader = "bytes.Reader.UnreadRune: previous operation was not ReadRune"u8;

// UnreadRune complements [Reader.ReadRune] in implementing the [io.RuneScanner] interface.
internal static error UnreadRuneᴍ(ref Reader r) {
    if (r.i <= 0) {
        return errors.New(bytesReaderUnreadRuneAtˢᴍReader);
    }
    if (r.prevRune < 0) {
        return errors.New(bytesReaderUnreadRuneˢᴍReader);
    }
    r.i = (int64)r.prevRune;
    r.prevRune = -1;
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string bytesReaderSeekInvalidˢᴍReader => ref bytesReaderSeekInvalidˢᵇᴍReader;
private static readonly @string bytesReaderSeekInvalidˢᵇᴍReader = "bytes.Reader.Seek: invalid whence"u8;
internal static ref readonly @string bytesReaderSeekNegativeˢᴍReader => ref bytesReaderSeekNegativeˢᵇᴍReader;
private static readonly @string bytesReaderSeekNegativeˢᵇᴍReader = "bytes.Reader.Seek: negative position"u8;

// Seek implements the [io.Seeker] interface.
internal static (int64, error) Seekᴍ(ref Reader r, int64 offset, nint whence) {
    r.prevRune = -1;
    int64 abs = default;
    switch (whence) {
    case var ᴛ when ᴛ == io.SeekStart: {
        abs = offset;
        break;
    }
    case var ᴛ when ᴛ == io.SeekCurrent: {
        abs = r.i + offset;
        break;
    }
    case var ᴛ when ᴛ == io.SeekEnd: {
        abs = (int64)len(r.s) + offset;
        break;
    }
    default: {
        return (0, errors.New(bytesReaderSeekInvalidˢᴍReader));
    }}
    if (abs < 0) {
        return (0, errors.New(bytesReaderSeekNegativeˢᴍReader));
    }
    r.i = abs;
    return (abs, default);
}

// WriteTo implements the [io.WriterTo] interface.
internal static (int64 n, error err) WriteToᴍ(ref Reader r, io.Writer w) {
    int64 n = default;
    error err = default;

    r.prevRune = -1;
    if (r.i >= (int64)len(r.s)) {
        return (0, default);
    }
    var b = r.s.slice((nint)(r.i));
    (var m, err) = w.Write(b);
    if (m > len(b)) {
        throw panic("bytes.Reader.WriteTo: invalid Write count");
    }
    r.i += (int64)m;
    n = (int64)m;
    if (m != len(b) && err == nil) {
        err = io.ErrShortWrite;
    }
    return (n, err);
}

// Reset resets the [Reader] to be reading from b.
internal static void Resetᴍ(ref Reader r, slice<byte> b) { r = new Reader(b, 0, -1); }

} partial class bytes_package {

// NewReader returns a new [Reader] reading from b.
public static ж<Reader> NewReader(slice<byte> b) { return Ꮡ(new Reader(b, 0, -1)); }

} // end bytes_package

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class bytesᴛReader {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ref Reader r) => bytes_package.Lenᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static int64 Size(this ref Reader r) => bytes_package.Sizeᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) Read(this ref Reader r, slice<byte> b) => bytes_package.Readᴍ(ref r, b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint n, error err) ReadAt(this ref Reader r, slice<byte> b, int64 off) => bytes_package.ReadAtᴍ(ref r, b, off);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (byte, error) ReadByte(this ref Reader r) => bytes_package.ReadByteᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnreadByte(this ref Reader r) => bytes_package.UnreadByteᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (rune ch, nint size, error err) ReadRune(this ref Reader r) => bytes_package.ReadRuneᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error UnreadRune(this ref Reader r) => bytes_package.UnreadRuneᴍ(ref r);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64, error) Seek(this ref Reader r, int64 offset, nint whence) => bytes_package.Seekᴍ(ref r, offset, whence);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (int64 n, error err) WriteTo(this ref Reader r, io.Writer w) => bytes_package.WriteToᴍ(ref r, w);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Reset(this ref Reader r, slice<byte> b) => bytes_package.Resetᴍ(ref r, b);
}
// </GlyphForwarders>
