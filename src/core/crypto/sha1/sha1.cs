// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha1 implements the SHA-1 hash algorithm as defined in RFC 3174.
//
// SHA-1 is cryptographically broken and should not be used for secure
// applications.
namespace go.crypto;

using crypto = crypto_package;
using boring = go.crypto.@internal.boring_package;
using errors = errors_package;
using hash = hash_package;
using byteorder = go.@internal.byteorder_package;
using go.@internal;
using go.crypto.@internal;

partial class sha1_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸcrypto() {
    builtin.initPackage(typeof(crypto_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸcryptoꓸinternalꓸboring() {
    builtin.initPackage(typeof(go.crypto.@internal.boring_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸerrors() {
    builtin.initPackage(typeof(errors_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸhash() {
    builtin.initPackage(typeof(hash_package));
}

[GoInit] internal static void init() {
    crypto.RegisterHash(crypto.SHA1, New);
}

// The size of a SHA-1 checksum in bytes.
public static UntypedInt ΔSize => 20;

// The blocksize of SHA-1 in bytes.
public static UntypedInt ΔBlockSize => 64;

internal static UntypedInt chunk => 64;
internal static UntypedInt init0 => 0x67452301;
internal static UntypedInt init1 => 0xEFCDAB89;
internal static UntypedInt init2 => 0x98BADCFE;
internal static UntypedInt init3 => 0x10325476;
internal static UntypedInt init4 => 0xC3D2E1F0;

// digest represents the partial evaluation of a checksum.
[GoType] partial struct digest {
    internal array<uint32> h = new(5);
    internal array<byte> x = new(chunk);
    internal nint nx;
    internal uint64 len;
}

internal static readonly @string magic = "sha\x01"u8;
internal const nint marshaledSize = /* len(magic) + 5*4 + chunk + 8 */ 96;

[GoRecv] internal static (slice<byte>, error) MarshalBinary(this ref digest d) {
    var b = new slice<byte>(0, marshaledSize);
    b = append(b, magic.ꓸꓸꓸ);
    b = byteorder.BeAppendUint32(b, d.h[0]);
    b = byteorder.BeAppendUint32(b, d.h[1]);
    b = byteorder.BeAppendUint32(b, d.h[2]);
    b = byteorder.BeAppendUint32(b, d.h[3]);
    b = byteorder.BeAppendUint32(b, d.h[4]);
    b = appendꓸꓸꓸ(b, d.x[..(int)(d.nx)]);
    b = b[..(int)(len(b) + len(d.x) - d.nx)]; // already zero
    b = byteorder.BeAppendUint64(b, d.len);
    return (b, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoSha1InvalidHashˢ = "crypto/sha1: invalid hash state identifier"u8;
internal static readonly @string cryptoSha1InvalidHashˢ2 = "crypto/sha1: invalid hash state size"u8;

[GoRecv] internal static error UnmarshalBinary(this ref digest d, slice<byte> b) {
    if (len(b) < len(magic) || ((sstring)(b[..(int)(len(magic))])) != magic) {
        return errors.New(cryptoSha1InvalidHashˢ);
    }
    if (len(b) != marshaledSize) {
        return errors.New(cryptoSha1InvalidHashˢ2);
    }
    b = b[(int)(len(magic))..];
    (b, d.h[0]) = consumeUint32(b);
    (b, d.h[1]) = consumeUint32(b);
    (b, d.h[2]) = consumeUint32(b);
    (b, d.h[3]) = consumeUint32(b);
    (b, d.h[4]) = consumeUint32(b);
    b = b[(int)(copy(d.x[..], b))..];
    (b, d.len) = consumeUint64(b);
    d.nx = (nint)(d.len % (uint64)chunk);
    return default!;
}

internal static (slice<byte>, uint64) consumeUint64(slice<byte> b) {
    return (b[8..], byteorder.BeUint64(b));
}

internal static (slice<byte>, uint32) consumeUint32(slice<byte> b) {
    return (b[4..], byteorder.BeUint32(b));
}

[GoRecv] internal static void Reset(this ref digest d) {
    d.h[0] = init0;
    d.h[1] = init1;
    d.h[2] = init2;
    d.h[3] = init3;
    d.h[4] = init4;
    d.nx = 0;
    d.len = 0;
}

// New returns a new hash.Hash computing the SHA1 checksum. The Hash also
// implements [encoding.BinaryMarshaler] and [encoding.BinaryUnmarshaler] to
// marshal and unmarshal the internal state of the hash.
public static hash.Hash New() {
    if (boring.Enabled) {
        return boring.NewSHA1();
    }
    var d = @new<digest>();
    d.Reset();
    return new digestжHash(d);
}

[GoRecv] internal static nint Size(this ref digest d) {
    return ΔSize;
}

[GoRecv] internal static nint BlockSize(this ref digest d) {
    return ΔBlockSize;
}

internal static (nint nn, error err) Write(this ж<digest> Ꮡd, slice<byte> p) {
    nint nn = default!;
    error err = default!;

    ref var d = ref Ꮡd.DerefOrNull();
    boring.Unreachable();
    nn = len(p);
    d.len += (uint64)nn;
    if (d.nx > 0) {
        nint n = copy(d.x[(int)(d.nx)..], p);
        d.nx += n;
        if (d.nx == chunk) {
            block(ref (Ꮡd).DerefOrNull(), d.x[..]);
            d.nx = 0;
        }
        p = p[(int)(n)..];
    }
    if (len(p) >= chunk) {
        nint n = (nint)(len(p) & ~(nint)(chunk - 1));
        block(ref (Ꮡd).DerefOrNull(), p[..(int)(n)]);
        p = p[(int)(n)..];
    }
    if (len(p) > 0) {
        d.nx = copy(d.x[..], p);
    }
    return (nn, err);
}

[GoRecv] internal static slice<byte> Sum(this ref digest d, slice<byte> @in) {
    boring.Unreachable();
    // Make a copy of d so that caller can keep writing and summing.
    ref var d0 = ref heap<digest>(out var Ꮡd0);
    d0 = d.ΔClone();
    var hash = Ꮡd0.checkSum();
    return appendꓸꓸꓸ(@in, hash[..]);
}

internal static array<byte> checkSum(this ж<digest> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    var len = d.len;
    // Padding.  Add a 1 bit and 0 bits until 56 bytes mod 64.
    array<byte> tmp = new(72); /* 64 + 8 */                   // padding + length buffer
    tmp[0] = 0x80;
    uint64 t = default!;
    if (len % 64 < 56){
        t = 56 - len % 64;
    } else {
        t = 64 + 56 - len % 64;
    }
    // Length in bits.
    len <<= (int)(3);
    var padlen = tmp[..(int)(t + 8)];
    byteorder.BePutUint64(padlen[(int)(t)..], len);
    Ꮡd.Write(padlen);
    if (d.nx != 0) {
        throw panic("d.nx != 0");
    }
    array<byte> digest = new(20); /* ΔSize */
    byteorder.BePutUint32(digest[0..], d.h[0]);
    byteorder.BePutUint32(digest[4..], d.h[1]);
    byteorder.BePutUint32(digest[8..], d.h[2]);
    byteorder.BePutUint32(digest[12..], d.h[3]);
    byteorder.BePutUint32(digest[16..], d.h[4]);
    return digest.Clone();
}

// ConstantTimeSum computes the same result of [Sum] but in constant time
[GoRecv] internal static slice<byte> ConstantTimeSum(this ref digest d, slice<byte> @in) {
    ref var d0 = ref heap<digest>(out var Ꮡd0);
    d0 = d.ΔClone();
    var hash = Ꮡd0.constSum();
    return appendꓸꓸꓸ(@in, hash[..]);
}

internal static array<byte> constSum(this ж<digest> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    array<byte> length = new(8);
    var l = (d.len << (int)(3));
    for (nuint i = (nuint)0; i < 8; i++) {
        length[(nint)(i)] = (byte)(l.Rsh((56 - 8 * i)));
    }
    var nx = (byte)d.nx;
    var t = (byte)(nx - 56); // if nx < 56 then the MSB of t is one
    var mask1b = (byte)(((int8)t >> (int)(7))); // mask1b is 0xFF iff one block is enough
    var separator = (byte)0x80; // gets reset to 0x00 once used
    for (var i = (byte)0; i < chunk; i++) {
        var mask = (byte)(((int8)(i - nx) >> (int)(7))); // 0x00 after the end of data
        // if we reached the end of the data, replace with 0x80 or 0x00
        d.x[i] = (byte)(((byte)(((byte)(~mask)) & separator)) | ((byte)(mask & d.x[i])));
        // zero the separator once used
        separator &= (byte)(mask);
        if (i >= 56) {
            // we might have to write the length here if all fit in one block
            d.x[i] |= (byte)((byte)(mask1b & length[i - 56]));
        }
    }
    // compress, and only keep the digest if all fit in one block
    block(ref (Ꮡd).DerefOrNull(), d.x[..]);
    array<byte> digest = new(20); /* ΔSize */
    foreach (var (i, s) in d.h.ΔRangeSnapshot()) {
        digest[i * 4] = (byte)(mask1b & (byte)((s >> (int)(24))));
        digest[i * 4 + 1] = (byte)(mask1b & (byte)((s >> (int)(16))));
        digest[i * 4 + 2] = (byte)(mask1b & (byte)((s >> (int)(8))));
        digest[i * 4 + 3] = (byte)(mask1b & (byte)s);
    }
    for (var i = (byte)0; i < chunk; i++) {
        // second block, it's always past the end of data, might start with 0x80
        if (i < 56){
            d.x[i] = separator;
            separator = 0;
        } else {
            d.x[i] = length[i - 56];
        }
    }
    // compress, and only keep the digest if we actually needed the second block
    block(ref (Ꮡd).DerefOrNull(), d.x[..]);
    foreach (var (i, s) in d.h.ΔRangeSnapshot()) {
        digest[i * 4] |= (byte)((byte)(((byte)(~mask1b)) & (byte)((s >> (int)(24)))));
        digest[i * 4 + 1] |= (byte)((byte)(((byte)(~mask1b)) & (byte)((s >> (int)(16)))));
        digest[i * 4 + 2] |= (byte)((byte)(((byte)(~mask1b)) & (byte)((s >> (int)(8)))));
        digest[i * 4 + 3] |= (byte)((byte)(((byte)(~mask1b)) & (byte)s));
    }
    return digest.Clone();
}

// Sum returns the SHA-1 checksum of the data.
public static array<byte> Sum(slice<byte> data) {
    if (boring.Enabled) {
        return boring.SHA1(data);
    }
    ref var d = ref heap(new digest(), out var Ꮡd);
    d.Reset();
    Ꮡd.Write(data);
    return Ꮡd.checkSum();
}

} // end sha1_package
