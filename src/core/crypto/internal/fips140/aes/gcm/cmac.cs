// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.aes;

using fips140 = go.crypto.@internal.fips140_package;
using aes = go.crypto.@internal.fips140.aes_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using static gcm_package;

partial class gcm_package {

// CMAC implements the CMAC mode from NIST SP 800-38B.
//
// It is optimized for use in Counter KDF (SP 800-108r1) and XAES-256-GCM
// (https://c2sp.org/XAES-256-GCM), rather than for exposing it to applications
// as a stand-alone MAC.
partial struct CMAC {
    internal aes.Block b;
    internal array<byte> k1 = new(aes.BlockSize);
    internal array<byte> k2 = new(aes.BlockSize);
}

public static ж<CMAC> NewCMAC(ж<aes.Block> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var c = Ꮡ(new CMAC(b: b));
    c.deriveSubkeys();
    return c;
}

} partial class gcmᴛCMAC {

internal static void deriveSubkeys(this ж<CMAC> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    aes.EncryptBlockInternal(Ꮡc.Ꮡb, c.k1[..], c.k1[..]);
    var msb = shiftLeft(Ꮡc.Ꮡk1);
    c.k1[len(c.k1) - 1] ^= (byte)(msb * 0b10000111);

    c.k2 = c.k1.Clone();
    msb = shiftLeft(Ꮡc.Ꮡk2);
    c.k2[len(c.k2) - 1] ^= (byte)(msb * 0b10000111);
}

public static array<byte> MAC(this ж<CMAC> Ꮡc, slice<byte> m) {
    ref var c = ref Ꮡc.DerefOrNull();

    fips140.RecordApproved();
    _ = c.b; // Hoist the nil check out of the loop.
    array<byte> x = new(16); /* aes.BlockSize */
    if (len(m) == 0) {
        // Special-cased as a single empty partial final block.
        x = c.k2.Clone();
        x[len(m)] ^= (byte)(0b10000000);
        aes.EncryptBlockInternal(Ꮡc.Ꮡb, x[..], x[..]);
        return x.Clone();
    }
    while (len(m) >= aes.BlockSize) {
        subtle.XORBytes(x[..], m[..(int)(aes.BlockSize)], x[..]);
        if (len(m) == aes.BlockSize) {
            // Final complete block.
            subtle.XORBytes(x[..], c.k1[..], x[..]);
        }
        aes.EncryptBlockInternal(Ꮡc.Ꮡb, x[..], x[..]);
        m = m[(int)(aes.BlockSize)..];
    }
    if (len(m) > 0) {
        // Final incomplete block.
        subtle.XORBytes(x[..], m, x[..]);
        subtle.XORBytes(x[..], c.k2[..], x[..]);
        x[len(m)] ^= (byte)(0b10000000);
        aes.EncryptBlockInternal(Ꮡc.Ꮡb, x[..], x[..]);
    }
    return x.Clone();
}

} partial class gcm_package {

// shiftLeft sets x to x << 1, and returns MSB₁(x).
internal static byte shiftLeft(/*[16]*/ ж<array<byte>> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    byte msb = default;
    for (nint i = 16 - 1; i >= 0; i--) {
        (msb, x[i]) = ((byte)((x[i] >> 7)), (byte)((byte)(x[i] << 1) | msb));
    }
    return msb;
}

} // end gcm_package
