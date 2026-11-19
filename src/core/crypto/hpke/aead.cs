// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using cipher = go.crypto.cipher_package;
using errors = errors_package;
using fmt = fmt_package;
using chacha20poly1305 = vendor.golang.org.x.crypto.chacha20poly1305_package;
using go.crypto;
using vendor.golang.org.x.crypto;
using static hpke_package;

partial class hpke_package {

// The AEAD is one of the three components of an HPKE ciphersuite, implementing
// symmetric encryption.
partial interface AEAD {
    uint16 ID();
    nint keySize();
    nint nonceSize();
    (cipher.AEAD, error) aead(slice<byte> key);
}

// NewAEAD returns the AEAD implementation for the given AEAD ID.
//
// Applications are encouraged to use specific implementations like [AES128GCM]
// or [ChaCha20Poly1305] instead, unless runtime agility is required.
public static (AEAD, error) NewAEAD(uint16 id) {
    switch (id) {
    case 0x0001: { // AES-128-GCM
        return (AES128GCM(), default);
    }
    case 0x0002: { // AES-256-GCM
        return (AES256GCM(), default);
    }
    case 0x0003: { // ChaCha20Poly1305
        return (ChaCha20Poly1305(), default);
    }
    case 0xFFFF: { // Export-only
        return (ExportOnly(), default);
    }
    default: {
        return (default, fmt.Errorf("unsupported AEAD %04x"u8, id));
    }}
}

// AES128GCM returns an AES-128-GCM AEAD implementation.
public static AEAD AES128GCM() { return new aeadжAEAD(aes128GCM); }

// AES256GCM returns an AES-256-GCM AEAD implementation.
public static AEAD AES256GCM() { return new aeadжAEAD(aes256GCM); }

// ChaCha20Poly1305 returns a ChaCha20Poly1305 AEAD implementation.
public static AEAD ChaCha20Poly1305() { return new aeadжAEAD(chacha20poly1305AEAD); }

// ExportOnly returns a placeholder AEAD implementation that cannot encrypt or
// decrypt, but only export secrets with [Sender.Export] or [Recipient.Export].
//
// When this is used, [Sender.Seal] and [Recipient.Open] return errors.
public static AEAD ExportOnly() { return new exportOnlyAEAD(); }

partial struct aead {
    internal nint nK;
    internal nint nN;
    internal Func<slice<byte>, (cipher.AEAD, error)> @new;
    internal uint16 id;
}

internal static ж<aead> aes128GCM = Ꮡ(new aead(
    nK: 128 / 8,
    nN: 96 / 8,
    @new: newAESGCM,
    id: 0x0001
));

internal static ж<aead> aes256GCM = Ꮡ(new aead(
    nK: 256 / 8,
    nN: 96 / 8,
    @new: newAESGCM,
    id: 0x0002
));

[GoLazy] partial class lazyᴛchacha20poly1305AEAD { internal static ж<aead> chacha20poly1305AEAD = Ꮡ(new aead(
    nK: chacha20poly1305.KeySize,
    nN: chacha20poly1305.NonceSize,
    @new: chacha20poly1305.New,
    id: 0x0003
)); }

} partial class hpkeᴛaead {

internal static uint16 ID(this ref aead a) {
    return a.id;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string invalidKeySizeˢ => ref invalidKeySizeˢᵇ;
private static readonly @string invalidKeySizeˢᵇ = "invalid key size"u8;

internal static (cipher.AEAD, error) aead(this ref aead a, slice<byte> key) {
    if (len(key) != a.nK) {
        return (default, errors.New(invalidKeySizeˢ));
    }
    return a.@new(key);
}

internal static nint keySize(this ref aead a) {
    return a.nK;
}

internal static nint nonceSize(this ref aead a) {
    return a.nN;
}

} partial class hpke_package {

partial struct exportOnlyAEAD {
}

} partial class hpkeᴛexportOnlyAEAD {

internal static uint16 ID(this exportOnlyAEAD _) {
    return 0xFFFF;
}

internal static (cipher.AEAD, error) aead(this exportOnlyAEAD _, slice<byte> key) {
    return (default, default);
}

internal static nint keySize(this exportOnlyAEAD _) {
    return 0;
}

internal static nint nonceSize(this exportOnlyAEAD _) {
    return 0;
}

} // end hpkeᴛexportOnlyAEAD
