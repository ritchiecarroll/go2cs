// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package adler32 implements the Adler-32 checksum.
//
// It is defined in RFC 1950:
//
//	Adler-32 is composed of two sums accumulated per byte: s1 is
//	the sum of all bytes, s2 is the sum of all s1 values. Both sums
//	are done modulo 65521. s1 is initialized to 1, s2 to zero.  The
//	Adler-32 checksum is stored as s2*65536 + s1 in most-
//	significant-byte first (network) order.
namespace go.hash;

using errors = errors_package;
using hash = hash_package;
using byteorder = @internal.byteorder_package;
using @internal;

partial class adler32_package {

internal static UntypedInt mod => 65521;
internal static UntypedInt nmax => 5552;

// The size of an Adler-32 checksum in bytes.
public static UntypedInt ΔSize => 4;

partial struct digest /*num:uint32*/;

internal static void Reset(this ref digest d) {
    d = 1;
}

// New returns a new hash.Hash32 computing the Adler-32 checksum. Its
// Sum method will lay the value out in big-endian byte order. The
// returned Hash32 also implements [encoding.BinaryMarshaler] and
// [encoding.BinaryUnmarshaler] to marshal and unmarshal the internal
// state of the hash.
public static hash.Hash32 New() {
    var d = @new<digest>();
    d.Reset();
    return new digestжHash32(d);
}

internal static nint Size(this ref digest d) {
    return ΔSize;
}

internal static nint BlockSize(this ref digest d) {
    return 4;
}

internal static readonly @string magic = "adl\x01"u8;
internal const nint marshaledSize = /* len(magic) + 4 */ 8;

internal static (slice<byte>, error) AppendBinary(this ref digest d, slice<byte> b) {
    b = append(b, magic.ꓸꓸꓸ);
    b = byteorder.BEAppendUint32(b, (uint32)(d));
    return (b, default!);
}

internal static (slice<byte>, error) MarshalBinary(this ref digest d) {
    return d.AppendBinary(new slice<byte>(0, marshaledSize));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string hashAdler32InvalidHashˢ = "hash/adler32: invalid hash state identifier"u8;
internal static readonly @string hashAdler32InvalidHashˢ2 = "hash/adler32: invalid hash state size"u8;

internal static error UnmarshalBinary(this ref digest d, slice<byte> b) {
    if (len(b) < len(magic) || ((sstring)(b[..(int)(len(magic))])) != magic) {
        return errors.New(hashAdler32InvalidHashˢ);
    }
    if (len(b) != marshaledSize) {
        return errors.New(hashAdler32InvalidHashˢ2);
    }
    d = ((digest)byteorder.BEUint32(b[(int)(len(magic))..]));
    return default!;
}

// Add p to the running checksum d.
internal static digest update(digest d, slice<byte> p) {
    var (s1, s2) = ((uint32)((digest)(d & 0xffff)), (uint32)((d >> (int)(16))));
    while (len(p) > 0) {
        slice<byte> q = default!;
        if (len(p) > nmax) {
            (p, q) = (p[..(int)(nmax)], p[(int)(nmax)..]);
        }
        while (len(p) >= 4) {
            s1 += (uint32)p[0];
            s2 += s1;
            s1 += (uint32)p[1];
            s2 += s1;
            s1 += (uint32)p[2];
            s2 += s1;
            s1 += (uint32)p[3];
            s2 += s1;
            p = p[4..];
        }
        foreach (var (_, x) in p) {
            s1 += (uint32)x;
            s2 += s1;
        }
        s1 %= mod;
        s2 %= mod;
        p = q;
    }
    return ((digest)((uint32)((s2 << (int)(16)) | s1)));
}

internal static (nint nn, error err) Write(this ref digest d, slice<byte> p) {
    d = update(d, p);
    return (len(p), default!);
}

internal static uint32 Sum32(this ref digest d) {
    return (uint32)(d);
}

internal static slice<byte> Sum(this ref digest d, slice<byte> @in) {
    var s = (uint32)(d);
    return append(@in, (byte)((s >> (int)(24))), (byte)((s >> (int)(16))), (byte)((s >> (int)(8))), (byte)s);
}

// Checksum returns the Adler-32 checksum of data.
public static uint32 Checksum(slice<byte> data) {
    return (uint32)update(1, data);
}

} // end adler32_package
