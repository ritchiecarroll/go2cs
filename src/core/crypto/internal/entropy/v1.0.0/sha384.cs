// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.entropy.v1_0_0;

using bits = math.bits_package;
using math;

partial class entropy_package {

// This file includes a SHA-384 implementation to insulate the entropy source
// from any changes in the FIPS 140-3 module's crypto/internal/fips140/sha512
// package. We support 1024-byte inputs for the entropy source, and arbitrary
// length inputs for ACVP testing.
internal static array<uint64> initState = new uint64[]{
    0xcbbb9d5dc1059ed8UL,
    0x629a292a367cd507UL,
    0x9159015a3070dd17UL,
    0x152fecd8f70e5939UL,
    0x67332667ffc00b31UL,
    0x8eb44a8768581511UL,
    0xdb0c2e0d64f98fa7UL,
    0x47b5481dbefa4fa4UL
}.array();

public static array<byte> SHA384(/*[1024]*/ ж<array<byte>> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    var h = initState.Clone();

    sha384Block(ref h, Ꮡ(array<byte>.Alias(p[0..128], 128)));
    sha384Block(ref h, Ꮡ(array<byte>.Alias(p[128..256], 128)));
    sha384Block(ref h, Ꮡ(array<byte>.Alias(p[256..384], 128)));
    sha384Block(ref h, Ꮡ(array<byte>.Alias(p[384..512], 128)));
    sha384Block(ref h, Ꮡ(array<byte>.Alias(p[512..640], 128)));
    sha384Block(ref h, Ꮡ(array<byte>.Alias(p[640..768], 128)));
    sha384Block(ref h, Ꮡ(array<byte>.Alias(p[768..896], 128)));
    sha384Block(ref h, Ꮡ(array<byte>.Alias(p[896..1024], 128)));

    ref var padlen = ref heap(new array<byte>(128), out var Ꮡpadlen);
    padlen[0] = 0x80;
    bePutUint64(padlen[(int)(112 + 8)..], 1024 * 8);
    sha384Block(ref h, Ꮡpadlen);

    return digestBytes(ref h);
}

public static array<byte> TestingOnlySHA384(slice<byte> p) {
    if (len(p) == 1024) {
        return SHA384(Ꮡ(array<byte>.Alias(p, 1024)));
    }

    var h = initState.Clone();
    var bitLen = (uint64)len(p) * 8;

    // Process full 128-byte blocks.
    while (len(p) >= 128) {
        sha384Block(ref h, Ꮡ(array<byte>.Alias(p[..128], 128)));
        p = p[128..];
    }

    // Process final block and padding.
    ref var finalBlock = ref heap(new array<byte>(128), out var ᏑfinalBlock);
    copy(finalBlock[..], p);
    finalBlock[len(p)] = 0x80;
    if (len(p) >= 112) {
        sha384Block(ref h, ᏑfinalBlock);
        finalBlock = new byte[]{}.array(128);
    }
    bePutUint64(finalBlock[(int)(112 + 8)..], bitLen);
    sha384Block(ref h, ᏑfinalBlock);

    return digestBytes(ref h);
}

internal static array<byte> digestBytes(ref array<uint64> h) {
    array<byte> digest = new(48);
    bePutUint64(digest[0..], h[0]);
    bePutUint64(digest[8..], h[1]);
    bePutUint64(digest[16..], h[2]);
    bePutUint64(digest[24..], h[3]);
    bePutUint64(digest[32..], h[4]);
    bePutUint64(digest[40..], h[5]);
    return digest.Clone();
}

[GoLazy] partial class lazyᴛ_K { internal static array<uint64> _K = new uint64[]{
    0x428a2f98d728ae22UL,
    0x7137449123ef65cdUL,
    0xb5c0fbcfec4d3b2fUL,
    0xe9b5dba58189dbbcUL,
    0x3956c25bf348b538UL,
    0x59f111f1b605d019UL,
    0x923f82a4af194f9bUL,
    0xab1c5ed5da6d8118UL,
    0xd807aa98a3030242UL,
    0x12835b0145706fbeUL,
    0x243185be4ee4b28cUL,
    0x550c7dc3d5ffb4e2UL,
    0x72be5d74f27b896fUL,
    0x80deb1fe3b1696b1UL,
    0x9bdc06a725c71235UL,
    0xc19bf174cf692694UL,
    0xe49b69c19ef14ad2UL,
    0xefbe4786384f25e3UL,
    0x0fc19dc68b8cd5b5UL,
    0x240ca1cc77ac9c65UL,
    0x2de92c6f592b0275UL,
    0x4a7484aa6ea6e483UL,
    0x5cb0a9dcbd41fbd4UL,
    0x76f988da831153b5UL,
    0x983e5152ee66dfabUL,
    0xa831c66d2db43210UL,
    0xb00327c898fb213fUL,
    0xbf597fc7beef0ee4UL,
    0xc6e00bf33da88fc2UL,
    0xd5a79147930aa725UL,
    0x06ca6351e003826fUL,
    0x142929670a0e6e70UL,
    0x27b70a8546d22ffcUL,
    0x2e1b21385c26c926UL,
    0x4d2c6dfc5ac42aedUL,
    0x53380d139d95b3dfUL,
    0x650a73548baf63deUL,
    0x766a0abb3c77b2a8UL,
    0x81c2c92e47edaee6UL,
    0x92722c851482353bUL,
    0xa2bfe8a14cf10364UL,
    0xa81a664bbc423001UL,
    0xc24b8b70d0f89791UL,
    0xc76c51a30654be30UL,
    0xd192e819d6ef5218UL,
    0xd69906245565a910UL,
    0xf40e35855771202aUL,
    0x106aa07032bbd1b8UL,
    0x19a4c116b8d2d0c8UL,
    0x1e376c085141ab53UL,
    0x2748774cdf8eeb99UL,
    0x34b0bcb5e19b48a8UL,
    0x391c0cb3c5c95a63UL,
    0x4ed8aa4ae3418acbUL,
    0x5b9cca4f7763e373UL,
    0x682e6ff3d6b2b8a3UL,
    0x748f82ee5defb2fcUL,
    0x78a5636f43172f60UL,
    0x84c87814a1f0ab72UL,
    0x8cc702081a6439ecUL,
    0x90befffa23631e28UL,
    0xa4506cebde82bde9UL,
    0xbef9a3f7b2c67915UL,
    0xc67178f2e372532bUL,
    0xca273eceea26619cUL,
    0xd186b8c721c0c207UL,
    0xeada7dd6cde0eb1eUL,
    0xf57d4f7fee6ed178UL,
    0x06f067aa72176fbaUL,
    0x0a637dc5a2c898a6UL,
    0x113f9804bef90daeUL,
    0x1b710b35131c471bUL,
    0x28db77f523047d84UL,
    0x32caab7b40c72493UL,
    0x3c9ebe0a15c9bebcUL,
    0x431d67c49c100d4cUL,
    0x4cc5d4becb3e42b6UL,
    0x597f299cfc657e2aUL,
    0x5fcb6fab3ad6faecUL,
    0x6c44198c4a475817UL
}.array(); }

internal static void sha384Block(ref array<uint64> dh, /*[128]*/ ж<array<byte>> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    array<uint64> w = new(80);
    foreach (var i in range(80)) {
        if (i < 16){
            w[i] = beUint64(p.slice(i * 8));
        } else {
            var v1 = w[i - 2];
            var t1 = bits.RotateLeft64(v1, -19) ^ bits.RotateLeft64(v1, -61) ^ (v1 >> 6);
            var v2 = w[i - 15];
            var t2 = bits.RotateLeft64(v2, -1) ^ bits.RotateLeft64(v2, -8) ^ (v2 >> 7);

            w[i] = t1 + w[i - 7] + t2 + w[i - 16];
        }
    }

    var (a, b, c, d, e, f, g, h) = (dh[0], dh[1], dh[2], dh[3], dh[4], dh[5], dh[6], dh[7]);

    foreach (var i in range(80)) {
        var t1 = h + (bits.RotateLeft64(e, -14) ^ bits.RotateLeft64(e, -18) ^ bits.RotateLeft64(e, -41)) + ((e & f) ^ (~e & g)) + _K[i] + w[i];
        var t2 = (bits.RotateLeft64(a, -28) ^ bits.RotateLeft64(a, -34) ^ bits.RotateLeft64(a, -39)) + ((a & b) ^ (a & c) ^ (b & c));

        h = g;
        g = f;
        f = e;
        e = d + t1;
        d = c;
        c = b;
        b = a;
        a = t1 + t2;
    }

    dh[0] += a;
    dh[1] += b;
    dh[2] += c;
    dh[3] += d;
    dh[4] += e;
    dh[5] += f;
    dh[6] += g;
    dh[7] += h;
}

internal static uint64 beUint64(slice<byte> b) {
    _ = b[7]; // bounds check hint to compiler; see golang.org/issue/14808
    return (uint64)b[7] | (uint64)b[6] << 8 | (uint64)b[5] << 16 | (uint64)b[4] << 24 | (uint64)b[3] << 32 | (uint64)b[2] << 40 | (uint64)b[1] << 48 | (uint64)b[0] << 56;
}

internal static void bePutUint64(slice<byte> b, uint64 v) {
    _ = b[7]; // early bounds check to guarantee safety of writes below
    b[0] = (byte)(v >> 56);
    b[1] = (byte)(v >> 48);
    b[2] = (byte)(v >> 40);
    b[3] = (byte)(v >> 32);
    b[4] = (byte)(v >> 24);
    b[5] = (byte)(v >> 16);
    b[6] = (byte)(v >> 8);
    b[7] = (byte)v;
}

} // end entropy_package
