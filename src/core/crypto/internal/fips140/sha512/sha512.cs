// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sha512 implements the SHA-384, SHA-512, SHA-512/224, and SHA-512/256
// hash algorithms as defined in FIPS 180-4.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using errors = errors_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140deps;

partial class sha512_package {

internal static UntypedInt size512 => 64;
internal static UntypedInt size224 => 28;
internal static UntypedInt size256 => 32;
internal static UntypedInt size384 => 48;
internal static UntypedInt blockSize => 128;

internal static UntypedInt chunk => 128;
internal static UntypedInt init0 => 0x6a09e667f3bcc908;
internal static UntypedInt init1 => 0xbb67ae8584caa73b;
internal static UntypedInt init2 => 0x3c6ef372fe94f82b;
internal static UntypedInt init3 => 0xa54ff53a5f1d36f1;
internal static UntypedInt init4 => 0x510e527fade682d1;
internal static UntypedInt init5 => 0x9b05688c2b3e6c1f;
internal static UntypedInt init6 => 0x1f83d9abfb41bd6b;
internal static UntypedInt init7 => 0x5be0cd19137e2179;
internal static UntypedInt init0_224 => 0x8c3d37c819544da2;
internal static UntypedInt init1_224 => 0x73e1996689dcd4d6;
internal static UntypedInt init2_224 => 0x1dfab7ae32ff9c82;
internal static UntypedInt init3_224 => 0x679dd514582f9fcf;
internal static UntypedInt init4_224 => 0x0f6d2b697bd44da8;
internal static UntypedInt init5_224 => 0x77e36f7304c48942;
internal static UntypedInt init6_224 => 0x3f9d85a86a1d36c8;
internal static UntypedInt init7_224 => 0x1112e6ad91d692a1;
internal static UntypedInt init0_256 => 0x22312194fc2bf72c;
internal static UntypedInt init1_256 => 0x9f555fa3c84c64c2;
internal static UntypedInt init2_256 => 0x2393b86b6f53b151;
internal static UntypedInt init3_256 => 0x963877195940eabd;
internal static UntypedInt init4_256 => 0x96283ee2a88effe3;
internal static UntypedInt init5_256 => 0xbe5e1e2553863992;
internal static UntypedInt init6_256 => 0x2b0199fc2c85b8aa;
internal static UntypedInt init7_256 => 0x0eb72ddc81c52ca2;
internal static UntypedInt init0_384 => 0xcbbb9d5dc1059ed8;
internal static UntypedInt init1_384 => 0x629a292a367cd507;
internal static UntypedInt init2_384 => 0x9159015a3070dd17;
internal static UntypedInt init3_384 => 0x152fecd8f70e5939;
internal static UntypedInt init4_384 => 0x67332667ffc00b31;
internal static UntypedInt init5_384 => 0x8eb44a8768581511;
internal static UntypedInt init6_384 => 0xdb0c2e0d64f98fa7;
internal static UntypedInt init7_384 => 0x47b5481dbefa4fa4;

// Digest is a SHA-384, SHA-512, SHA-512/224, or SHA-512/256 [hash.Hash]
// implementation.
[GoType] partial struct Digest {
    internal array<uint64> h = new(8);
    internal array<byte> x = new(chunk);
    internal nint nx;
    internal uint64 len;
    internal nint size; // size224, size256, size384, or size512
}

[GoRecv] public static void Reset(this ref Digest d) {
    var exprᴛ1 = d.size;
    if (exprᴛ1 == size384) {
        d.h[0] = init0_384;
        d.h[1] = init1_384;
        d.h[2] = init2_384;
        d.h[3] = init3_384;
        d.h[4] = init4_384;
        d.h[5] = init5_384;
        d.h[6] = init6_384;
        d.h[7] = init7_384;
    }
    else if (exprᴛ1 == size224) {
        d.h[0] = init0_224;
        d.h[1] = init1_224;
        d.h[2] = init2_224;
        d.h[3] = init3_224;
        d.h[4] = init4_224;
        d.h[5] = init5_224;
        d.h[6] = init6_224;
        d.h[7] = init7_224;
    }
    else if (exprᴛ1 == size256) {
        d.h[0] = init0_256;
        d.h[1] = init1_256;
        d.h[2] = init2_256;
        d.h[3] = init3_256;
        d.h[4] = init4_256;
        d.h[5] = init5_256;
        d.h[6] = init6_256;
        d.h[7] = init7_256;
    }
    else if (exprᴛ1 == size512) {
        d.h[0] = init0;
        d.h[1] = init1;
        d.h[2] = init2;
        d.h[3] = init3;
        d.h[4] = init4;
        d.h[5] = init5;
        d.h[6] = init6;
        d.h[7] = init7;
    }
    else { /* default: */
        throw panic("unknown size");
    }

    d.nx = 0;
    d.len = 0;
}

internal static readonly @string magic384 = "sha\x04"u8;
internal static readonly @string magic512_224 = "sha\x05"u8;
internal static readonly @string magic512_256 = "sha\x06"u8;
internal static readonly @string magic512 = "sha\x07"u8;
internal const nint marshaledSize = /* len(magic512) + 8*8 + chunk + 8 */ 204;

[GoRecv] public static (slice<byte>, error) MarshalBinary(this ref Digest d) {
    return d.AppendBinary(new slice<byte>(0, marshaledSize));
}

[GoRecv] public static (slice<byte>, error) AppendBinary(this ref Digest d, slice<byte> b) {
    var exprᴛ1 = d.size;
    if (exprᴛ1 == size384) {
        b = append(b, magic384.ꓸꓸꓸ);
    }
    else if (exprᴛ1 == size224) {
        b = append(b, magic512_224.ꓸꓸꓸ);
    }
    else if (exprᴛ1 == size256) {
        b = append(b, magic512_256.ꓸꓸꓸ);
    }
    else if (exprᴛ1 == size512) {
        b = append(b, magic512.ꓸꓸꓸ);
    }
    else { /* default: */
        throw panic("unknown size");
    }

    b = byteorder.BEAppendUint64(b, d.h[0]);
    b = byteorder.BEAppendUint64(b, d.h[1]);
    b = byteorder.BEAppendUint64(b, d.h[2]);
    b = byteorder.BEAppendUint64(b, d.h[3]);
    b = byteorder.BEAppendUint64(b, d.h[4]);
    b = byteorder.BEAppendUint64(b, d.h[5]);
    b = byteorder.BEAppendUint64(b, d.h[6]);
    b = byteorder.BEAppendUint64(b, d.h[7]);
    b = appendꓸꓸꓸ(b, d.x.slice(0, d.nx));
    b = appendꓸꓸꓸ(b, makeꓸꓸꓸ<byte>(len(d.x) - d.nx));
    b = byteorder.BEAppendUint64(b, d.len);
    return (b, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoSha512InvalidHashˢ = "crypto/sha512: invalid hash state identifier"u8;
private static readonly @string cryptoSha512InvalidHashˢ2 = "crypto/sha512: invalid hash state size"u8;

[GoRecv] public static error UnmarshalBinary(this ref Digest d, slice<byte> b) {
    if (len(b) < len(magic512)) {
        return errors.New(cryptoSha512InvalidHashˢ);
    }
    switch (ᐧ) {
    case {} when d.size == size384 && ((sstring)(b[..(int)(len(magic384))])) == magic384: {
        break;
    }
    case {} when d.size == size224 && ((sstring)(b[..(int)(len(magic512_224))])) == magic512_224: {
        break;
    }
    case {} when d.size == size256 && ((sstring)(b[..(int)(len(magic512_256))])) == magic512_256: {
        break;
    }
    case {} when d.size == size512 && ((sstring)(b[..(int)(len(magic512))])) == magic512: {
        break;
    }
    default: {
        return errors.New(cryptoSha512InvalidHashˢ);
    }}

    if (len(b) != marshaledSize) {
        return errors.New(cryptoSha512InvalidHashˢ2);
    }
    b = b[(int)(len(magic512))..];
    (b, d.h[0]) = consumeUint64(b);
    (b, d.h[1]) = consumeUint64(b);
    (b, d.h[2]) = consumeUint64(b);
    (b, d.h[3]) = consumeUint64(b);
    (b, d.h[4]) = consumeUint64(b);
    (b, d.h[5]) = consumeUint64(b);
    (b, d.h[6]) = consumeUint64(b);
    (b, d.h[7]) = consumeUint64(b);
    b = b.slice(copy(d.x[..], b));
    (b, d.len) = consumeUint64(b);
    d.nx = (nint)(d.len % (uint64)chunk);
    return default!;
}

internal static (slice<byte>, uint64) consumeUint64(slice<byte> b) {
    return (b[8..], byteorder.BEUint64(b));
}

// New returns a new Digest computing the SHA-512 hash.
public static ж<Digest> New() {
    var d = Ꮡ(new Digest(size: size512));
    d.Reset();
    return d;
}

// New512_224 returns a new Digest computing the SHA-512/224 hash.
public static ж<Digest> New512_224() {
    var d = Ꮡ(new Digest(size: size224));
    d.Reset();
    return d;
}

// New512_256 returns a new Digest computing the SHA-512/256 hash.
public static ж<Digest> New512_256() {
    var d = Ꮡ(new Digest(size: size256));
    d.Reset();
    return d;
}

// New384 returns a new Digest computing the SHA-384 hash.
public static ж<Digest> New384() {
    var d = Ꮡ(new Digest(size: size384));
    d.Reset();
    return d;
}

[GoRecv] public static nint Size(this ref Digest d) {
    return d.size;
}

[GoRecv] public static nint BlockSize(this ref Digest d) {
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

[GoRecv] public static slice<byte> Sum(this ref Digest d, slice<byte> @in) {
    fips140.RecordApproved();
    // Make a copy of d so that caller can keep writing and summing.
    var d0 = @new<Digest>();
    d0.Value = d.ΔClone();
    var hash = d0.checkSum();
    return appendꓸꓸꓸ(@in, hash.slice(0, d.size));
}

internal static array<byte> checkSum(this ж<Digest> Ꮡd) {
    ref var d = ref Ꮡd.DerefOrNull();

    // Padding. Add a 1 bit and 0 bits until 112 bytes mod 128.
    var len = d.len;
    array<byte> tmp = new(144); /* 128 + 16 */                      // padding + length buffer
    tmp[0] = 0x80;
    uint64 t = default!;
    if (len % 128 < 112){
        t = 112 - len % 128;
    } else {
        t = 128 + 112 - len % 128;
    }
    // Length in bits.
    len <<= (int)(3);
    var padlen = tmp.slice(0, (nint)(t + 16));
    // Upper 64 bits are always zero, because len variable has type uint64,
    // and tmp is already zeroed at that index, so we can skip updating it.
    // byteorder.BEPutUint64(padlen[t+0:], 0)
    byteorder.BEPutUint64(padlen.slice((nint)(t + 8)), len);
    Ꮡd.Write(padlen);
    if (d.nx != 0) {
        throw panic("d.nx != 0");
    }
    array<byte> digest = new(64); /* size512 */
    byteorder.BEPutUint64(digest[0..], d.h[0]);
    byteorder.BEPutUint64(digest[8..], d.h[1]);
    byteorder.BEPutUint64(digest[16..], d.h[2]);
    byteorder.BEPutUint64(digest[24..], d.h[3]);
    byteorder.BEPutUint64(digest[32..], d.h[4]);
    byteorder.BEPutUint64(digest[40..], d.h[5]);
    if (d.size != size384) {
        byteorder.BEPutUint64(digest[48..], d.h[6]);
        byteorder.BEPutUint64(digest[56..], d.h[7]);
    }
    return digest.Clone();
}

} // end sha512_package
