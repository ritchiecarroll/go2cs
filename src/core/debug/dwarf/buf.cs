// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Buffered reading and decoding of DWARF data streams.
namespace go.debug;

using bytes = bytes_package;
using binary = encoding.binary_package;
using strconv = strconv_package;
using encoding;
using static dwarf_package;

partial class dwarf_package {

// Data buffer being decoded.
partial struct buf {
    internal ж<Data> dwarf;
    internal binary.ByteOrder order;
    internal dataFormat format;
    internal @string name;
    internal Offset off;
    internal slice<byte> data;
    internal error err;
}

// Data format, other than byte order. This affects the handling of
// certain field formats.
partial interface dataFormat {
    // DWARF version number. Zero means unknown.
    nint version();
    // 64-bit DWARF format?
    (bool dwarf64, bool isKnown) dwarf64();
    // Size of an address, in bytes. Zero means unknown.
    nint addrsize();
}

// Some parts of DWARF have no data format, e.g., abbrevs.
partial struct unknownFormat {
}

} partial class dwarf_package {

[GoMethodBody("version")] internal static nint versionᴍ(unknownFormat u) {
    return 0;
}

[GoMethodBody("dwarf64")] internal static (bool, bool) dwarf64ᴍ(unknownFormat u) {
    return (false, false);
}

[GoMethodBody("addrsize")] internal static nint addrsizeᴍ(unknownFormat u) {
    return 0;
}

} partial class dwarf_package {

internal static buf makeBuf(ж<Data> Ꮡd, dataFormat format, @string name, Offset off, slice<byte> data) {
    ref var d = ref Ꮡd.DerefOrNull();

    return new buf(Ꮡd, d.order, format, name, off, data, default);
}

} partial class dwarf_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string underflowˢᴍbuf => ref underflowˢᵇᴍbuf;
private static readonly @string underflowˢᵇᴍbuf = "underflow"u8;

[GoMethodBody("uint8")] internal static uint8 uint8ᴍ(ref buf b) {
    if (len(b.data) < 1) {
        b.error(underflowˢᴍbuf);
        return 0;
    }
    var val = b.data[0];
    b.data = b.data[1..];
    b.off++;
    return val;
}

[GoMethodBody("bytes")] internal static slice<byte> bytesᴍ(ref buf b, nint n) {
    if (n < 0 || len(b.data) < n) {
        b.error(underflowˢᴍbuf);
        return default;
    }
    var data = b.data.slice(0, n);
    b.data = b.data.slice(n);
    b.off += ((Offset)(uint32)n);
    return data;
}

[GoMethodBody("skip")] internal static void skipᴍ(ref buf b, nint n) { b.bytes(n); }

[GoMethodBody("string")] internal static @string @stringᴍ(ref buf b) {
    nint i = bytes_package.IndexByte(b.data, 0);
    if (i < 0) {
        b.error(underflowˢᴍbuf);
        return ""u8;
    }

    @string s = (@string)(b.data.slice(0, i));
    b.data = b.data.slice(i + 1);
    b.off += ((Offset)(uint32)(i + 1));
    return s;
}

[GoMethodBody("uint16")] internal static uint16 uint16ᴍ(ref buf b) {
    var a = b.bytes(2);
    if (a == nil) {
        return 0;
    }
    return b.order.Uint16(a);
}

[GoMethodBody("uint24")] internal static uint32 uint24ᴍ(ref buf b) {
    var a = b.bytes(3);
    if (a == nil) {
        return 0;
    }
    if (b.dwarf.bigEndian) {
        return (uint32)a[2] | (uint32)a[1] << 8 | (uint32)a[0] << 16;
    } else {
        return (uint32)a[0] | (uint32)a[1] << 8 | (uint32)a[2] << 16;
    }
}

[GoMethodBody("uint32")] internal static uint32 uint32ᴍ(ref buf b) {
    var a = b.bytes(4);
    if (a == nil) {
        return 0;
    }
    return b.order.Uint32(a);
}

[GoMethodBody("uint64")] internal static uint64 uint64ᴍ(ref buf b) {
    var a = b.bytes(8);
    if (a == nil) {
        return 0;
    }
    return b.order.Uint64(a);
}

// Read a varint, which is 7 bits per byte, little endian.
// the 0x80 bit means read another byte.
[GoMethodBody("varint")] internal static (uint64 c, nuint bits) varintᴍ(ref buf b) {
    uint64 c = default;
    nuint bits = default;

    for (nint i = 0; i < len(b.data); i++) {
        var @byte = b.data[i];
        c |= (uint64)(((uint64)((byte)(@byte & 0x7F))).Lsh(bits));
        bits += 7;
        if ((byte)(@byte & 0x80) == 0) {
            b.off += ((Offset)(uint32)(i + 1));
            b.data = b.data.slice(i + 1);
            return (c, bits);
        }
    }
    return (0, 0);
}

// Unsigned int is just a varint.
[GoMethodBody("uint")] internal static uint64 @uintᴍ(ref buf b) {
    var (x, _) = b.varint();
    return x;
}

// Signed int is a sign-extended varint.
[GoMethodBody("int")] internal static int64 @intᴍ(ref buf b) {
    var (ux, bits) = b.varint();
    var x = (int64)ux;
    if ((x & (((int64)1).Lsh((bits - 1)))) != 0) {
        x |= (int64)(((int64)(-1)).Lsh(bits));
    }
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unknownAddressSizeˢᴍbuf => ref unknownAddressSizeˢᵇᴍbuf;
private static readonly @string unknownAddressSizeˢᵇᴍbuf = "unknown address size"u8;

// Address-sized uint.
[GoMethodBody("addr")] internal static uint64 addrᴍ(ref buf b) {
    switch (b.format.addrsize()) {
    case 1: {
        return (uint64)b.uint8();
    }
    case 2: {
        return (uint64)b.uint16();
    }
    case 4: {
        return (uint64)b.uint32();
    }
    case 8: {
        return b.uint64();
    }}
    b.error(unknownAddressSizeˢᴍbuf);
    return 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string unitLengthHasReservedˢᴍbuf => ref unitLengthHasReservedˢᵇᴍbuf;
private static readonly @string unitLengthHasReservedˢᵇᴍbuf = "unit length has reserved value"u8;

[GoMethodBody("unitLength")] internal static (Offset length, bool dwarf64) unitLengthᴍ(ref buf b) {
    Offset length = default;
    bool dwarf64 = default;

    length = (Offset)b.uint32();
    if (length == 0xffffffffU) {
        dwarf64 = true;
        length = (Offset)(uint32)b.uint64();
    } else if (length >= 0xfffffff0U) {
        b.error(unitLengthHasReservedˢᴍbuf);
    }
    return (length, dwarf64);
}

[GoMethodBody("error")] internal static void errorᴍ(ref buf b, @string s) {
    if (b.err == nil) {
        b.data = default;
        b.err = new DecodeError(b.name, b.off, s);
    }
}

} partial class dwarf_package {

partial struct DecodeError {
    public @string Name;
    public Offset Offset;
    public @string Err;
}

} partial class dwarf_package {

[GoMethodBody("Error")] internal static @string Errorᴍ(DecodeError e) {
    return "decoding dwarf section "u8 + e.Name + " at offset 0x"u8 + strconv.FormatInt((int64)e.Offset, 16) + ": "u8 + e.Err;
}

}
