// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha256 implements the SHA-224 and SHA-256 hash algorithms as defined
// in FIPS 180-4.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using errors = errors_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140deps;

partial class sha256_package {

// The size of a SHA-256 checksum in bytes.
internal static UntypedInt size => 32;

// The size of a SHA-224 checksum in bytes.
internal static UntypedInt size224 => 28;

// The block size of SHA-256 and SHA-224 in bytes.
internal static UntypedInt blockSize => 64;

internal static UntypedInt chunk => 64;
internal static UntypedInt init0 => 0x6A09E667;
internal static UntypedInt init1 => 0xBB67AE85;
internal static UntypedInt init2 => 0x3C6EF372;
internal static UntypedInt init3 => 0xA54FF53A;
internal static UntypedInt init4 => 0x510E527F;
internal static UntypedInt init5 => 0x9B05688C;
internal static UntypedInt init6 => 0x1F83D9AB;
internal static UntypedInt init7 => 0x5BE0CD19;
internal static UntypedInt init0_224 => 0xC1059ED8;
internal static UntypedInt init1_224 => 0x367CD507;
internal static UntypedInt init2_224 => 0x3070DD17;
internal static UntypedInt init3_224 => 0xF70E5939;
internal static UntypedInt init4_224 => 0xFFC00B31;
internal static UntypedInt init5_224 => 0x68581511;
internal static UntypedInt init6_224 => 0x64F98FA7;
internal static UntypedInt init7_224 => 0xBEFA4FA4;

// Digest is a SHA-224 or SHA-256 [hash.Hash] implementation.
partial struct Digest {
    internal array<uint32> h = new(8);
    internal array<byte> x = new(chunk);
    internal nint nx;
    internal uint64 len;
    internal bool is224; // mark if this digest is SHA-224
}

internal static readonly @string magic224 = "sha\x02"u8;
internal static readonly @string magic256 = "sha\x03"u8;
internal const nint marshaledSize = /* len(magic256) + 8*4 + chunk + 8 */ 108;

public static (slice<byte>, error) MarshalBinary(this ref Digest d) {
    return d.AppendBinary(new slice<byte>(0, marshaledSize));
}

public static (slice<byte>, error) AppendBinary(this ref Digest d, slice<byte> b) {
    if (d.is224){
        b = append(b, magic224.ꓸꓸꓸ);
    } else {
        b = append(b, magic256.ꓸꓸꓸ);
    }
    b = byteorder.BEAppendUint32(b, d.h[0]);
    b = byteorder.BEAppendUint32(b, d.h[1]);
    b = byteorder.BEAppendUint32(b, d.h[2]);
    b = byteorder.BEAppendUint32(b, d.h[3]);
    b = byteorder.BEAppendUint32(b, d.h[4]);
    b = byteorder.BEAppendUint32(b, d.h[5]);
    b = byteorder.BEAppendUint32(b, d.h[6]);
    b = byteorder.BEAppendUint32(b, d.h[7]);
    b = appendꓸꓸꓸ(b, d.x.slice(0, d.nx));
    b = appendꓸꓸꓸ(b, makeꓸꓸꓸ<byte>(len(d.x) - d.nx));
    b = byteorder.BEAppendUint64(b, d.len);
    return (b, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoSha256InvalidHashˢ = "crypto/sha256: invalid hash state identifier"u8;
private static readonly @string cryptoSha256InvalidHashˢ2 = "crypto/sha256: invalid hash state size"u8;

public static error UnmarshalBinary(this ref Digest d, slice<byte> b) {
    if (len(b) < len(magic224) || (d.is224 && ((sstring)(b[..(int)(len(magic224))])) != magic224) || (!d.is224 && ((sstring)(b[..(int)(len(magic256))])) != magic256)) {
        return errors.New(cryptoSha256InvalidHashˢ);
    }
    if (len(b) != marshaledSize) {
        return errors.New(cryptoSha256InvalidHashˢ2);
    }
    b = b[(int)(len(magic224))..];
    (b, d.h[0]) = consumeUint32(b);
    (b, d.h[1]) = consumeUint32(b);
    (b, d.h[2]) = consumeUint32(b);
    (b, d.h[3]) = consumeUint32(b);
    (b, d.h[4]) = consumeUint32(b);
    (b, d.h[5]) = consumeUint32(b);
    (b, d.h[6]) = consumeUint32(b);
    (b, d.h[7]) = consumeUint32(b);
    b = b.slice(copy(d.x[..], b));
    (b, d.len) = consumeUint64(b);
    d.nx = (nint)(d.len % (uint64)chunk);
    return default!;
}

internal static (slice<byte>, uint64) consumeUint64(slice<byte> b) {
    return (b[8..], byteorder.BEUint64(b));
}

internal static (slice<byte>, uint32) consumeUint32(slice<byte> b) {
    return (b[4..], byteorder.BEUint32(b));
}

public static void Reset(this ref Digest d) {
    if (!d.is224){
        d.h[0] = init0;
        d.h[1] = init1;
        d.h[2] = init2;
        d.h[3] = init3;
        d.h[4] = init4;
        d.h[5] = init5;
        d.h[6] = init6;
        d.h[7] = init7;
    } else {
        d.h[0] = init0_224;
        d.h[1] = init1_224;
        d.h[2] = init2_224;
        d.h[3] = init3_224;
        d.h[4] = init4_224;
        d.h[5] = init5_224;
        d.h[6] = init6_224;
        d.h[7] = init7_224;
    }
    d.nx = 0;
    d.len = 0;
}

// New returns a new Digest computing the SHA-256 hash.
public static ж<Digest> New() {
    var d = @new<Digest>();
    d.Reset();
    return d;
}

// New224 returns a new Digest computing the SHA-224 hash.
public static ж<Digest> New224() {
    var d = @new<Digest>();
    d.Value.is224 = true;
    d.Reset();
    return d;
}

public static nint Size(this ref Digest d) {
    if (!d.is224) {
        return size;
    }
    return size224;
}

public static nint BlockSize(this ref Digest d) {
    return blockSize;
}

public static (nint nn, error err) Write(this ж<Digest> Ꮡd, slice<byte> p) {
    nint nn = default!;
    error err = default!;

    ref var d = ref Ꮡd.DerefOrNull();
    nn = len(p);
    d.len += (uint64)nn;
    if (d.nx > 0) {
        nint n = copy(d.x.slice(d.nx), p);
        d.nx += n;
        if (d.nx == chunk) {
            block(ref (Ꮡd).DerefOrNull(), d.x[..]);
            d.nx = 0;
        }
        p = p.slice(n);
    }
    if (len(p) >= chunk) {
        nint n = (nint)(len(p) & ~(nint)(chunk - 1));
        block(ref (Ꮡd).DerefOrNull(), p.slice(0, n));
        p = p.slice(n);
    }
    if (len(p) > 0) {
        d.nx = copy(d.x[..], p);
    }
    return (nn, err);
}

public static slice<byte> Sum(this ref Digest d, slice<byte> @in) {
    fips140.RecordApproved();
    // Make a copy of d so that caller can keep writing and summing.
    ref var d0 = ref heap<Digest>(out var Ꮡd0);
    d0 = d.ΔClone();
    var hash = Ꮡd0.checkSum();
    if (d0.is224) {
        return appendꓸꓸꓸ(@in, hash[..(int)(size224)]);
    }
    return appendꓸꓸꓸ(@in, hash[..]);
}

internal static array<byte> checkSum(this ж<Digest> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    var len = d.len;
    // Padding. Add a 1 bit and 0 bits until 56 bytes mod 64.
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
    var padlen = tmp.slice(0, (nint)(t + 8));
    byteorder.BEPutUint64(padlen.slice((nint)(t + 0)), len);
    Ꮡd.Write(padlen);
    if (d.nx != 0) {
        throw panic("d.nx != 0");
    }
    array<byte> digest = new(32); /* size */
    byteorder.BEPutUint32(digest[0..], d.h[0]);
    byteorder.BEPutUint32(digest[4..], d.h[1]);
    byteorder.BEPutUint32(digest[8..], d.h[2]);
    byteorder.BEPutUint32(digest[12..], d.h[3]);
    byteorder.BEPutUint32(digest[16..], d.h[4]);
    byteorder.BEPutUint32(digest[20..], d.h[5]);
    byteorder.BEPutUint32(digest[24..], d.h[6]);
    if (!d.is224) {
        byteorder.BEPutUint32(digest[28..], d.h[7]);
    }
    return digest.Clone();
}

} // end sha256_package
