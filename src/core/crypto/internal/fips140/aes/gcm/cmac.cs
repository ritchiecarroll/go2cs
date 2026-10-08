// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.aes;

using fips140 = go.crypto.@internal.fips140_package;
using aes = go.crypto.@internal.fips140.aes_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class gcm_package {

// CMAC implements the CMAC mode from NIST SP 800-38B.
//
// It is optimized for use in Counter KDF (SP 800-108r1) and XAES-256-GCM
// (https://c2sp.org/XAES-256-GCM), rather than for exposing it to applications
// as a stand-alone MAC.
partial struct CMAC {
    internal aes.Block b;
    internal array<byte> k1 = new(aes.ΔBlockSize);
    internal array<byte> k2 = new(aes.ΔBlockSize);
}

public static ж<CMAC> NewCMAC(ж<aes.Block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var c = Ꮡ(new CMAC(b: b));
    c.deriveSubkeys();
    return c;
}

internal static void deriveSubkeys(this ж<CMAC> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    aes.EncryptBlockInternal(Ꮡc.of(CMAC.Ꮡb), c.k1[..], c.k1[..]);
    var msb = shiftLeft(Ꮡc.of(CMAC.Ꮡk1));
    c.k1[len(c.k1) - 1] ^= (byte)(msb * 0b10000111);
    c.k2 = c.k1.Clone();
    msb = shiftLeft(Ꮡc.of(CMAC.Ꮡk2));
    c.k2[len(c.k2) - 1] ^= (byte)(msb * 0b10000111);
}

public static array<byte> MAC(this ж<CMAC> Ꮡc, slice<byte> m) {
    ref var c = ref Ꮡc.DerefOrNull();

    fips140.RecordApproved();
    _ = c.b; // Hoist the nil check out of the loop.
    array<byte> x = new(16); /* aes.ΔBlockSize */
    if (len(m) == 0) {
        // Special-cased as a single empty partial final block.
        x = c.k2.Clone();
        x[len(m)] ^= (byte)(0b10000000);
        aes.EncryptBlockInternal(Ꮡc.of(CMAC.Ꮡb), x[..], x[..]);
        return x.Clone();
    }
    while (len(m) >= aes.ΔBlockSize) {
        subtle.XORBytes(x[..], m[..(int)(aes.ΔBlockSize)], x[..]);
        if (len(m) == aes.ΔBlockSize) {
            // Final complete block.
            subtle.XORBytes(x[..], c.k1[..], x[..]);
        }
        aes.EncryptBlockInternal(Ꮡc.of(CMAC.Ꮡb), x[..], x[..]);
        m = m[(int)(aes.ΔBlockSize)..];
    }
    if (len(m) > 0) {
        // Final incomplete block.
        subtle.XORBytes(x[..], m, x[..]);
        subtle.XORBytes(x[..], c.k2[..], x[..]);
        x[len(m)] ^= (byte)(0b10000000);
        aes.EncryptBlockInternal(Ꮡc.of(CMAC.Ꮡb), x[..], x[..]);
    }
    return x.Clone();
}

// shiftLeft sets x to x << 1, and returns MSB₁(x).
internal static byte shiftLeft(/*[16]*/ ж<array<byte>> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    byte msb = default!;
    for (nint i = 16 - 1; i >= 0; i--) {
        (msb, x[i]) = ((byte)((x[i] >> (int)(7))), (byte)((byte)(x[i] << (int)(1)) | msb));
    }
    return msb;
}

} // end gcm_package
