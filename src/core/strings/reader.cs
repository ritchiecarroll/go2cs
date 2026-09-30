// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using io = io_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;

partial class strings_package {

// A Reader implements the [io.Reader], [io.ReaderAt], [io.ByteReader], [io.ByteScanner],
// [io.RuneReader], [io.RuneScanner], [io.Seeker], and [io.WriterTo] interfaces by reading
// from a string.
// The zero value for Reader operates like a Reader of an empty string.
[GoType] partial struct Reader {
    internal @string s;
    internal int64 i; // current reading index
    internal nint prevRune;  // index of previous rune; or < 0
}

// Len returns the number of bytes of the unread portion of the
// string.
[GoRecv] public static nint Len(this ref Reader r) {
    if (r.i >= (int64)len(r.s)) {
        return 0;
    }
    return (nint)((int64)len(r.s) - r.i);
}

// Size returns the original length of the underlying string.
// Size is the number of bytes available for reading via [Reader.ReadAt].
// The returned value is always the same and is not affected by calls
// to any other method.
[GoRecv] public static int64 Size(this ref Reader r) {
    return (int64)len(r.s);
}

// Read implements the [io.Reader] interface.
[GoRecv] public static (nint n, error err) Read(this ref Reader r, slice<byte> b) {
    nint n = default!;
    error err = default!;

    if (r.i >= (int64)len(r.s)) {
        return (0, io.EOF);
    }
    r.prevRune = -1;
    n = copy(b, r.s.slice((nint)(r.i)));
    r.i += (int64)n;
    return (n, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringsReaderReadAtˢ = "strings.Reader.ReadAt: negative offset"u8;

// ReadAt implements the [io.ReaderAt] interface.
[GoRecv] public static (nint n, error err) ReadAt(this ref Reader r, slice<byte> b, int64 off) {
    nint n = default!;
    error err = default!;

    // cannot modify state - see io.ReaderAt
    if (off < 0) {
        return (0, errors.New(stringsReaderReadAtˢ));
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
[GoRecv] public static (byte, error) ReadByte(this ref Reader r) {
    r.prevRune = -1;
    if (r.i >= (int64)len(r.s)) {
        return (0, io.EOF);
    }
    var b = r.s[(nint)(r.i)];
    r.i++;
    return (b, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringsReaderUnreadByteˢ = "strings.Reader.UnreadByte: at beginning of string"u8;

// UnreadByte implements the [io.ByteScanner] interface.
[GoRecv] public static error UnreadByte(this ref Reader r) {
    if (r.i <= 0) {
        return errors.New(stringsReaderUnreadByteˢ);
    }
    r.prevRune = -1;
    r.i--;
    return default!;
}

// ReadRune implements the [io.RuneReader] interface.
[GoRecv] public static (rune ch, nint size, error err) ReadRune(this ref Reader r) {
    rune ch = default!;
    nint size = default!;
    error err = default!;

    if (r.i >= (int64)len(r.s)) {
        r.prevRune = -1;
        return (0, 0, io.EOF);
    }
    r.prevRune = (nint)r.i;
    {
        var c = r.s[(nint)(r.i)]; if (c < utf8.RuneSelf) {
            r.i++;
            return ((rune)c, 1, default!);
        }
    }
    (ch, size) = utf8.DecodeRuneInString(r.s.slice((nint)(r.i)));
    r.i += (int64)size;
    return (ch, size, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringsReaderUnreadRuneˢ = "strings.Reader.UnreadRune: at beginning of string"u8;
internal static readonly @string stringsReaderUnreadRuneˢ2 = "strings.Reader.UnreadRune: previous operation was not ReadRune"u8;

// UnreadRune implements the [io.RuneScanner] interface.
[GoRecv] public static error UnreadRune(this ref Reader r) {
    if (r.i <= 0) {
        return errors.New(stringsReaderUnreadRuneˢ);
    }
    if (r.prevRune < 0) {
        return errors.New(stringsReaderUnreadRuneˢ2);
    }
    r.i = (int64)r.prevRune;
    r.prevRune = -1;
    return default!;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stringsReaderSeekInvalidˢ = "strings.Reader.Seek: invalid whence"u8;
internal static readonly @string stringsReaderSeekˢ = "strings.Reader.Seek: negative position"u8;

// Seek implements the [io.Seeker] interface.
[GoRecv] public static (int64, error) Seek(this ref Reader r, int64 offset, nint whence) {
    r.prevRune = -1;
    int64 abs = default!;
    var exprᴛ1 = whence;
    if (exprᴛ1 == io.SeekStart) {
        abs = offset;
    }
    else if (exprᴛ1 == io.SeekCurrent) {
        abs = r.i + offset;
    }
    else if (exprᴛ1 == io.SeekEnd) {
        abs = (int64)len(r.s) + offset;
    }
    else { /* default: */
        return (0, errors.New(stringsReaderSeekInvalidˢ));
    }

    if (abs < 0) {
        return (0, errors.New(stringsReaderSeekˢ));
    }
    r.i = abs;
    return (abs, default!);
}

// WriteTo implements the [io.WriterTo] interface.
[GoRecv] public static (int64 n, error err) WriteTo(this ref Reader r, io.Writer w) {
    int64 n = default!;
    error err = default!;

    r.prevRune = -1;
    if (r.i >= (int64)len(r.s)) {
        return (0, default!);
    }
    @string s = r.s.slice((nint)(r.i));
    (var m, err) = io.WriteString(w, s);
    if (m > len(s)) {
        throw panic("strings.Reader.WriteTo: invalid WriteString count");
    }
    r.i += (int64)m;
    n = (int64)m;
    if (m != len(s) && err == default!) {
        err = io.ErrShortWrite;
    }
    return (n, err);
}

// Reset resets the [Reader] to be reading from s.
[GoRecv] public static void Reset(this ref Reader r, @string s) {
    r = new Reader(s, 0, -1);
}

// NewReader returns a new [Reader] reading from s.
// It is similar to [bytes.NewBufferString] but more efficient and non-writable.
public static ж<Reader> NewReader(@string s) {
    return Ꮡ(new Reader(s, 0, -1));
}

} // end strings_package
