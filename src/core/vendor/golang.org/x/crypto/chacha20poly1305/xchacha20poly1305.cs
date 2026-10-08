// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.vendor.golang.org.x.crypto;

using cipher = go.crypto.cipher_package;
using errors = errors_package;
using chacha20 = go.vendor.golang.org.x.crypto.chacha20_package;
using go.crypto;
using go.vendor.golang.org.x.crypto;

partial class chacha20poly1305_package {

partial struct xchacha20poly1305 {
    internal array<byte> key = new(KeySize);
}

// NewX returns a XChaCha20-Poly1305 AEAD that uses the given 256-bit key.
//
// XChaCha20-Poly1305 is a ChaCha20-Poly1305 variant that takes a longer nonce,
// suitable to be generated randomly without risk of collisions. It should be
// preferred when nonce uniqueness cannot be trivially ensured, or whenever
// nonces are randomly generated.
public static (cipher.AEAD, error) NewX(slice<byte> key) {
    if (len(key) != KeySize) {
        return (default!, errors.New(chacha20poly1305BadKeyˢ));
    }
    var ret = @new<xchacha20poly1305>();
    copy((~ret).key[..], key);
    return (new xchacha20poly1305жAEAD(ret), default!);
}

internal static nint NonceSize(this ref xchacha20poly1305 _) {
    return NonceSizeX;
}

internal static nint Overhead(this ref xchacha20poly1305 _) {
    return ΔOverhead;
}

internal static slice<byte> Seal(this ref xchacha20poly1305 x, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    if (len(nonce) != NonceSizeX) {
        throw panic("chacha20poly1305: bad nonce length passed to Seal");
    }
    // XChaCha20-Poly1305 technically supports a 64-bit counter, so there is no
    // size limit. However, since we reuse the ChaCha20-Poly1305 implementation,
    // the second half of the counter is not available. This is unlikely to be
    // an issue because the cipher.AEAD API requires the entire message to be in
    // memory, and the counter overflows at 256 GB.
    if ((uint64)len(plaintext) > (uint64)((274877906944L) - 64)) {
        throw panic("chacha20poly1305: plaintext too large");
    }
    var c = @new<chacha20poly1305>();
    var (hKey, _) = chacha20.HChaCha20(x.key[..], nonce[0..16]);
    copy((~c).key[..], hKey);
    // The first 4 bytes of the final nonce are unused counter space.
    var cNonce = new slice<byte>(ΔNonceSize);
    copy(cNonce[4..12], nonce[16..24]);
    return c.seal(dst, cNonce[..], plaintext, additionalData);
}

internal static (slice<byte>, error) Open(this ref xchacha20poly1305 x, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    if (len(nonce) != NonceSizeX) {
        throw panic("chacha20poly1305: bad nonce length passed to Open");
    }
    if (len(ciphertext) < 16) {
        return (default!, errOpen);
    }
    if ((uint64)len(ciphertext) > (uint64)((274877906944L) - 48)) {
        throw panic("chacha20poly1305: ciphertext too large");
    }
    var c = @new<chacha20poly1305>();
    var (hKey, _) = chacha20.HChaCha20(x.key[..], nonce[0..16]);
    copy((~c).key[..], hKey);
    // The first 4 bytes of the final nonce are unused counter space.
    var cNonce = new slice<byte>(ΔNonceSize);
    copy(cNonce[4..12], nonce[16..24]);
    return c.open(dst, cNonce[..], ciphertext, additionalData);
}

} // end chacha20poly1305_package
