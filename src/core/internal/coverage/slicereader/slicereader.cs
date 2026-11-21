// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.coverage;

using binary = encoding.binary_package;
using fmt = fmt_package;
using io = io_package;
using @unsafe = unsafe_package;
using encoding;
using static slicereader_package;

partial class slicereader_package {

// This file contains the helper "SliceReader", a utility for
// reading values from a byte slice that may or may not be backed
// by a read-only mmap'd region.
partial struct Reader {
    internal slice<byte> b;
    internal bool @readonly;
    internal int64 off;
}

public static ж<Reader> NewReader(slice<byte> b, bool @readonly) {
    ref var r = ref heap<Reader>(new Reader(
        b: b,
        @readonly: @readonly
    ), out var Ꮡr);
    return Ꮡr;
}

} partial class slicereader_package {

[GoMethodBody("Read")] internal static (nint, error) Readᴍ(ref Reader r, slice<byte> b) {
    nint amt = len(b);
    var toread = r.b.slice((nint)(r.off));
    if (len(toread) < amt) {
        amt = len(toread);
    }
    copy(b, toread);
    r.off += (int64)amt;
    return (amt, default);
}

[GoMethodBody("Seek")] internal static (int64 ret, error err) Seekᴍ(ref Reader r, int64 offset, nint whence) {
    switch (whence) {
    case var ᴛ when ᴛ == io.SeekStart: {
        if (offset < 0 || offset > (int64)len(r.b)) {
            return (0, fmt.Errorf("invalid seek: new offset %d (out of range [0 %d]"u8, offset, len(r.b)));
        }
        r.off = offset;
        return (offset, default);
    }
    case var ᴛ when ᴛ == io.SeekCurrent: {
        var newoff = r.off + offset;
        if (newoff < 0 || newoff > (int64)len(r.b)) {
            return (0, fmt.Errorf("invalid seek: new offset %d (out of range [0 %d]"u8, newoff, len(r.b)));
        }
        r.off = newoff;
        return (r.off, default);
    }
    case var ᴛ when ᴛ == io.SeekEnd: {
        var newoff = (int64)len(r.b) + offset;
        if (newoff < 0 || newoff > (int64)len(r.b)) {
            return (0, fmt.Errorf("invalid seek: new offset %d (out of range [0 %d]"u8, newoff, len(r.b)));
        }
        r.off = newoff;
        return (r.off, default);
    }}
    // other modes are not supported
    return (0, fmt.Errorf("unsupported seek mode %d"u8, whence));
}

[GoMethodBody("Offset")] internal static int64 Offsetᴍ(ref Reader r) {
    return r.off;
}

[GoMethodBody("ReadUint8")] internal static uint8 ReadUint8ᴍ(ref Reader r) {
    var rv = (uint8)r.b[(nint)r.off];
    r.off += 1;
    return rv;
}

[GoMethodBody("ReadUint32")] internal static uint32 ReadUint32ᴍ(ref Reader r) {
    nint end = (nint)r.off + 4;
    var rv = binary.LittleEndian.Uint32(r.b.slice((nint)r.off, end, end));
    r.off += 4;
    return rv;
}

[GoMethodBody("ReadUint64")] internal static uint64 ReadUint64ᴍ(ref Reader r) {
    nint end = (nint)r.off + 8;
    var rv = binary.LittleEndian.Uint64(r.b.slice((nint)r.off, end, end));
    r.off += 8;
    return rv;
}

[GoMethodBody("ReadULEB128")] internal static uint64 /*value*/ ReadULEB128ᴍ(ref Reader r) {
    uint64 value = default;

    nuint shift = default;

    for (;;) {
        var b = r.b[(nint)(r.off)];
        r.off++;
        value |= (uint64)((((uint64)((byte)(b & 0x7F))).Lsh(shift)));
        if ((byte)(b & 0x80) == 0) {
            break;
        }
        shift += 7;
    }
    return value;
}

[GoMethodBody("ReadString")] internal static @string ReadStringᴍ(ref Reader r, int64 len) {
    var b = r.b.slice((nint)(r.off), (nint)(r.off + len));
    r.off += len;
    if (r.@readonly) {
        return toString(b); // backed by RO memory, ok to make unsafe string
    }
    return (@string)b;
}

} partial class slicereader_package {

internal static @string toString(slice<byte> b) {
    if (len(b) == 0) {
        return ""u8;
    }
    return @unsafe.String(Ꮡ(b, 0), len(b));
}

} // end slicereader_package
