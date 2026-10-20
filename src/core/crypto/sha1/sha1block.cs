// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bits = math.bits_package;
using math;

partial class sha1_package {

internal static UntypedInt _K0 => 0x5A827999;
internal static UntypedInt _K1 => 0x6ED9EBA1;
internal static UntypedInt _K2 => 0x8F1BBCDC;
internal static UntypedInt _K3 => 0xCA62C1D6;

// blockGeneric is a portable, pure Go version of the SHA-1 block step.
// It's used by sha1block_generic.go and tests.
internal static void blockGeneric(ref digest dig, slice<byte> p) {
    array<uint32> w = new(16);

    var (h0, h1, h2, h3, h4) = (dig.h[0], dig.h[1], dig.h[2], dig.h[3], dig.h[4]);
    while (len(p) >= chunk) {
        // Can interlace the computation of w with the
        // rounds below if needed for speed.
        for (nint iΔ1 = 0; iΔ1 < 16; iΔ1++) {
            nint j = iΔ1 * 4;
            w[iΔ1] = (uint32)p[j] << 24 | (uint32)p[j + 1] << 16 | (uint32)p[j + 2] << 8 | (uint32)p[j + 3];
        }

        var (a, b, c, d, e) = (h0, h1, h2, h3, h4);

        // Each of the four 20-iteration rounds
        // differs only in the computation of f and
        // the choice of K (_K0, _K1, etc).
        nint i = 0;
        for (; i < 16; i++) {
            var f = b & c | (~b) & d;
            var t = bits.RotateLeft32(a, 5) + f + e + w[(i & 0xf)] + (uint32)_K0;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);
        }
        for (; i < 20; i++) {
            var tmp = w[((i - 3) & 0xf)] ^ w[((i - 8) & 0xf)] ^ w[((i - 14) & 0xf)] ^ w[((i) & 0xf)];
            w[(i & 0xf)] = bits.RotateLeft32(tmp, 1);

            var f = b & c | (~b) & d;
            var t = bits.RotateLeft32(a, 5) + f + e + w[(i & 0xf)] + (uint32)_K0;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);
        }
        for (; i < 40; i++) {
            var tmp = w[((i - 3) & 0xf)] ^ w[((i - 8) & 0xf)] ^ w[((i - 14) & 0xf)] ^ w[((i) & 0xf)];
            w[(i & 0xf)] = bits.RotateLeft32(tmp, 1);
            var f = b ^ c ^ d;
            var t = bits.RotateLeft32(a, 5) + f + e + w[(i & 0xf)] + (uint32)_K1;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);
        }
        for (; i < 60; i++) {
            var tmp = w[((i - 3) & 0xf)] ^ w[((i - 8) & 0xf)] ^ w[((i - 14) & 0xf)] ^ w[((i) & 0xf)];
            w[(i & 0xf)] = bits.RotateLeft32(tmp, 1);
            var f = ((b | c) & d) | (b & c);
            var t = bits.RotateLeft32(a, 5) + f + e + w[(i & 0xf)] + (uint32)_K2;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);
        }
        for (; i < 80; i++) {
            var tmp = w[((i - 3) & 0xf)] ^ w[((i - 8) & 0xf)] ^ w[((i - 14) & 0xf)] ^ w[((i) & 0xf)];
            w[(i & 0xf)] = bits.RotateLeft32(tmp, 1);
            var f = b ^ c ^ d;
            var t = bits.RotateLeft32(a, 5) + f + e + w[(i & 0xf)] + (uint32)_K3;
            (a, b, c, d, e) = (t, a, bits.RotateLeft32(b, 30), c, d);
        }

        h0 += a;
        h1 += b;
        h2 += c;
        h3 += d;
        h4 += e;

        p = p[(int)(chunk)..];
    }

    (dig.h[0], dig.h[1], dig.h[2], dig.h[3], dig.h[4]) = (h0, h1, h2, h3, h4);
}

} // end sha1_package
