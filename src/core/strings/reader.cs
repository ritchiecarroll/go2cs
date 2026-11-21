// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
using io = io_package;
using utf8 = go.unicode.utf8_package;
using go.unicode;
using static strings_package;

partial class strings_package {

// A Reader implements the [io.Reader], [io.ReaderAt], [io.ByteReader], [io.ByteScanner],
// [io.RuneReader], [io.RuneScanner], [io.Seeker], and [io.WriterTo] interfaces by reading
// from a string.
// The zero value for Reader operates like a Reader of an empty string.
partial struct Reader {
    internal @string s;
    internal int64 i;       // current reading index
    internal nint prevRune; // index of previous rune; or < 0
}

} partial class strings_package {

// Len returns the number of bytes of the unread portion of the
// string.
[GoMethodBody("Len")] internal static nint Lenᴍ(ref Reader r) {
    if (r.i >= (int64)len(r.s)) {
        return 0;
    }
    return (nint)((int64)len(r.s) - r.i);
}

// Size returns the original length of the underlying string.
// Size is the number of bytes available for reading via [Reader.ReadAt].
// The returned value is always the same and is not affected by calls
// to any other method.
[GoMethodBody("Size")] internal static int64 Sizeᴍ(ref Reader r) { return (int64)len(r.s); }

// Read implements the [io.Reader] interface.
[GoMethodBody("Read")] internal static (nint n, error err) Readᴍ(ref Reader r, slice<byte> b) {
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
internal static ref readonly @string stringsReaderReadAtˢᴍReader => ref stringsReaderReadAtˢᵇᴍReader;
private static readonly @string stringsReaderReadAtˢᵇᴍReader = "strings.Reader.ReadAt: negative offset"u8;

// ReadAt implements the [io.ReaderAt] interface.
[GoMethodBody("ReadAt")] internal static (nint n, error err) ReadAtᴍ(ref Reader r, slice<byte> b, int64 off) {
    nint n = default;
    error err = default;

    // cannot modify state - see io.ReaderAt
    if (off < 0) {
        return (0, errors.New(stringsReaderReadAtˢᴍReader));
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
[GoMethodBody("ReadByte")] internal static (byte, error) ReadByteᴍ(ref Reader r) {
    r.prevRune = -1;
    if (r.i >= (int64)len(r.s)) {
        return (0, io.EOF);
    }
    var b = r.s[(nint)(r.i)];
    r.i++;
    return (b, default);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stringsReaderUnreadByteˢᴍReader => ref stringsReaderUnreadByteˢᵇᴍReader;
private static readonly @string stringsReaderUnreadByteˢᵇᴍReader = "strings.Reader.UnreadByte: at beginning of string"u8;

// UnreadByte implements the [io.ByteScanner] interface.
[GoMethodBody("UnreadByte")] internal static error UnreadByteᴍ(ref Reader r) {
    if (r.i <= 0) {
        return errors.New(stringsReaderUnreadByteˢᴍReader);
    }
    r.prevRune = -1;
    r.i--;
    return default;
}

// ReadRune implements the [io.RuneReader] interface.
[GoMethodBody("ReadRune")] internal static (rune ch, nint size, error err) ReadRuneᴍ(ref Reader r) {
    rune ch = default;
    nint size = default;
    error err = default;

    if (r.i >= (int64)len(r.s)) {
        r.prevRune = -1;
        return (0, 0, io.EOF);
    }
    r.prevRune = (nint)r.i;
    (ch, size) = utf8.DecodeRuneInString(r.s.slice((nint)(r.i)));
    r.i += (int64)size;
    return (ch, size, err);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stringsReaderUnreadRuneˢᴍReader => ref stringsReaderUnreadRuneˢᵇᴍReader;
private static readonly @string stringsReaderUnreadRuneˢᵇᴍReader = "strings.Reader.UnreadRune: at beginning of string"u8;
internal static ref readonly @string stringsReaderUnreadRuneˢ2ᴍReader => ref stringsReaderUnreadRuneˢ2ᵇᴍReader;
private static readonly @string stringsReaderUnreadRuneˢ2ᵇᴍReader = "strings.Reader.UnreadRune: previous operation was not ReadRune"u8;

// UnreadRune implements the [io.RuneScanner] interface.
[GoMethodBody("UnreadRune")] internal static error UnreadRuneᴍ(ref Reader r) {
    if (r.i <= 0) {
        return errors.New(stringsReaderUnreadRuneˢᴍReader);
    }
    if (r.prevRune < 0) {
        return errors.New(stringsReaderUnreadRuneˢ2ᴍReader);
    }
    r.i = (int64)r.prevRune;
    r.prevRune = -1;
    return default;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string stringsReaderSeekInvalidˢᴍReader => ref stringsReaderSeekInvalidˢᵇᴍReader;
private static readonly @string stringsReaderSeekInvalidˢᵇᴍReader = "strings.Reader.Seek: invalid whence"u8;
internal static ref readonly @string stringsReaderSeekˢᴍReader => ref stringsReaderSeekˢᵇᴍReader;
private static readonly @string stringsReaderSeekˢᵇᴍReader = "strings.Reader.Seek: negative position"u8;

// Seek implements the [io.Seeker] interface.
[GoMethodBody("Seek")] internal static (int64, error) Seekᴍ(ref Reader r, int64 offset, nint whence) {
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
        return (0, errors.New(stringsReaderSeekInvalidˢᴍReader));
    }}
    if (abs < 0) {
        return (0, errors.New(stringsReaderSeekˢᴍReader));
    }
    r.i = abs;
    return (abs, default);
}

// WriteTo implements the [io.WriterTo] interface.
[GoMethodBody("WriteTo")] internal static (int64 n, error err) WriteToᴍ(ref Reader r, io.Writer w) {
    int64 n = default;
    error err = default;

    r.prevRune = -1;
    if (r.i >= (int64)len(r.s)) {
        return (0, default);
    }
    @string s = r.s.slice((nint)(r.i));
    (var m, err) = io.WriteString(w, s);
    if (m > len(s)) {
        throw panic("strings.Reader.WriteTo: invalid WriteString count");
    }
    r.i += (int64)m;
    n = (int64)m;
    if (m != len(s) && err == nil) {
        err = io.ErrShortWrite;
    }
    return (n, err);
}

// Reset resets the [Reader] to be reading from s.
[GoMethodBody("Reset")] internal static void Resetᴍ(ref Reader r, @string s) { r = new Reader(s, 0, -1); }

} partial class strings_package {

// NewReader returns a new [Reader] reading from s.
// It is similar to [bytes.NewBufferString] but more efficient and non-writable.
public static ж<Reader> NewReader(@string s) { return Ꮡ(new Reader(s, 0, -1)); }

} // end strings_package
