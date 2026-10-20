// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// SHA512 block step.
// In its own file so that a faster assembly or C version
// can be substituted easily.
namespace go.crypto.@internal.fips140;

using bits = math.bits_package;
using math;

partial class sha512_package {

internal static array<uint64> _K = new uint64[]{
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
}.array();

internal static void blockGeneric(ref Digest dig, slice<byte> p) {
    array<uint64> w = new(80);
    var (h0, h1, h2, h3, h4, h5, h6, h7) = (dig.h[0], dig.h[1], dig.h[2], dig.h[3], dig.h[4], dig.h[5], dig.h[6], dig.h[7]);
    while (len(p) >= chunk) {
        var (a, b, c, d, e, f, g, h) = (h0, h1, h2, h3, h4, h5, h6, h7);

        foreach (var i in range(80)) {
            if (i < 16){
                nint j = i * 8;
                w[i] = (uint64)p[j] << 56 | (uint64)p[j + 1] << 48 | (uint64)p[j + 2] << 40 | (uint64)p[j + 3] << 32 | (uint64)p[j + 4] << 24 | (uint64)p[j + 5] << 16 | (uint64)p[j + 6] << 8 | (uint64)p[j + 7];
            } else {
                var v1 = w[i - 2];
                var t1Δ1 = bits.RotateLeft64(v1, -19) ^ bits.RotateLeft64(v1, -61) ^ (v1 >> 6);
                var v2 = w[i - 15];
                var t2Δ1 = bits.RotateLeft64(v2, -1) ^ bits.RotateLeft64(v2, -8) ^ (v2 >> 7);

                w[i] = t1Δ1 + w[i - 7] + t2Δ1 + w[i - 16];
            }

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

        h0 += a;
        h1 += b;
        h2 += c;
        h3 += d;
        h4 += e;
        h5 += f;
        h6 += g;
        h7 += h;

        p = p[(int)(chunk)..];
    }

    (dig.h[0], dig.h[1], dig.h[2], dig.h[3], dig.h[4], dig.h[5], dig.h[6], dig.h[7]) = (h0, h1, h2, h3, h4, h5, h6, h7);
}

} // end sha512_package
