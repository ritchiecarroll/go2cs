// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package cryptobyte contains types that help with parsing and constructing
// length-prefixed, binary messages, including ASN.1 DER. (The asn1 subpackage
// contains useful ASN.1 constants.)
//
// The String type is for parsing. It wraps a []byte slice and provides helper
// functions for consuming structures, value by value.
//
// The Builder type is for constructing messages. It providers helper functions
// for appending values and also for appending length-prefixed submessages –
// without having to worry about calculating the length prefix ahead of time.
//
// See the documentation and examples for the Builder and String types to get
// started.
namespace go.vendor.golang.org.x.crypto;

partial class cryptobyte_package {

// String represents a string of bytes. It provides methods for parsing
// fixed-length and length-prefixed values from it.
partial struct String /*[]byte*/;

// read advances a String by n bytes and returns them. If less than n bytes
// remain, it returns nil.
internal static slice<byte> read(this ref String s, nint n) {
    if (len(s) < n || n < 0) {
        return default;
    }
    var v = (s).slice(0, n);
    s = (s).slice(n);
    return v;
}

// Skip advances the String by n byte and reports whether it was successful.
public static bool Skip(this ref String s, nint n) {
    return s.read(n) != nil;
}

// ReadUint8 decodes an 8-bit value into out and advances over it.
// It reports whether the read was successful.
public static bool ReadUint8(this ref String s, ж<uint8> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    var v = s.read(1);
    if (v == nil) {
        return false;
    }
    @out = (uint8)v[0];
    return true;
}

// ReadUint16 decodes a big-endian, 16-bit value into out and advances over it.
// It reports whether the read was successful.
public static bool ReadUint16(this ref String s, ж<uint16> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    var v = s.read(2);
    if (v == nil) {
        return false;
    }
    @out = (uint16)((uint16)((uint16)v[0] << 8) | (uint16)v[1]);
    return true;
}

// ReadUint24 decodes a big-endian, 24-bit value into out and advances over it.
// It reports whether the read was successful.
public static bool ReadUint24(this ref String s, ж<uint32> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    var v = s.read(3);
    if (v == nil) {
        return false;
    }
    @out = (uint32)v[0] << 16 | (uint32)v[1] << 8 | (uint32)v[2];
    return true;
}

// ReadUint32 decodes a big-endian, 32-bit value into out and advances over it.
// It reports whether the read was successful.
public static bool ReadUint32(this ref String s, ж<uint32> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    var v = s.read(4);
    if (v == nil) {
        return false;
    }
    @out = (uint32)v[0] << 24 | (uint32)v[1] << 16 | (uint32)v[2] << 8 | (uint32)v[3];
    return true;
}

// ReadUint48 decodes a big-endian, 48-bit value into out and advances over it.
// It reports whether the read was successful.
public static bool ReadUint48(this ref String s, ж<uint64> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    var v = s.read(6);
    if (v == nil) {
        return false;
    }
    @out = (uint64)v[0] << 40 | (uint64)v[1] << 32 | (uint64)v[2] << 24 | (uint64)v[3] << 16 | (uint64)v[4] << 8 | (uint64)v[5];
    return true;
}

// ReadUint64 decodes a big-endian, 64-bit value into out and advances over it.
// It reports whether the read was successful.
public static bool ReadUint64(this ref String s, ж<uint64> Ꮡout) {
    ref var @out = ref Ꮡout.DerefOrNull();

    var v = s.read(8);
    if (v == nil) {
        return false;
    }
    @out = (uint64)v[0] << 56 | (uint64)v[1] << 48 | (uint64)v[2] << 40 | (uint64)v[3] << 32 | (uint64)v[4] << 24 | (uint64)v[5] << 16 | (uint64)v[6] << 8 | (uint64)v[7];
    return true;
}

internal static bool readUnsigned(this ref String s, ж<uint32> Ꮡout, nint length) {
    ref var @out = ref Ꮡout.DerefOrNull();

    var v = s.read(length);
    if (v == nil) {
        return false;
    }
    uint32 result = default;
    for (nint i = 0; i < length; i++) {
        result <<= 8;
        result |= (uint32)((uint32)v[i]);
    }
    @out = result;
    return true;
}

internal static bool readLengthPrefixed(this ref String s, nint lenLen, ж<String> ᏑoutChild) {
    ref var outChild = ref ᏑoutChild.DerefOrNull();

    var lenBytes = s.read(lenLen);
    if (lenBytes == nil) {
        return false;
    }
    uint32 length = default;
    foreach (var (_, b) in lenBytes) {
        length = length << 8;
        length = length | (uint32)b;
    }
    var v = s.read((nint)length);
    if (v == nil) {
        return false;
    }
    outChild = v;
    return true;
}

// ReadUint8LengthPrefixed reads the content of an 8-bit length-prefixed value
// into out and advances over it. It reports whether the read was successful.
public static bool ReadUint8LengthPrefixed(this ref String s, ж<String> @out) {
    return s.readLengthPrefixed(1, @out);
}

// ReadUint16LengthPrefixed reads the content of a big-endian, 16-bit
// length-prefixed value into out and advances over it. It reports whether the
// read was successful.
public static bool ReadUint16LengthPrefixed(this ref String s, ж<String> @out) {
    return s.readLengthPrefixed(2, @out);
}

// ReadUint24LengthPrefixed reads the content of a big-endian, 24-bit
// length-prefixed value into out and advances over it. It reports whether
// the read was successful.
public static bool ReadUint24LengthPrefixed(this ref String s, ж<String> @out) {
    return s.readLengthPrefixed(3, @out);
}

// ReadBytes reads n bytes into out and advances over them. It reports
// whether the read was successful.
public static bool ReadBytes(this ref String s, ж<slice<byte>> Ꮡout, nint n) {
    ref var @out = ref Ꮡout.DerefOrNull();

    var v = s.read(n);
    if (v == nil) {
        return false;
    }
    @out = v;
    return true;
}

// CopyBytes copies len(out) bytes into out and advances over them. It reports
// whether the copy operation was successful
public static bool CopyBytes(this ref String s, slice<byte> @out) {
    nint n = len(@out);
    var v = s.read(n);
    if (v == nil) {
        return false;
    }
    return copy(@out, v) == n;
}

// Empty reports whether the string does not contain any bytes.
public static bool Empty(this String s) {
    return len(s) == 0;
}

} // end cryptobyte_package
