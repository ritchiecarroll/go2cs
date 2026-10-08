// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// CFB (Cipher Feedback) Mode.
namespace go.crypto;

using alias = go.crypto.@internal.fips140.alias_package;
using fips140only = go.crypto.@internal.fips140only_package;
using subtle = go.crypto.subtle_package;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class cipher_package {

partial struct cfb {
    internal Block b;
    internal slice<byte> next;
    internal slice<byte> @out;
    internal nint outUsed;
    internal bool decrypt;
}

internal static void XORKeyStream(this ref cfb x, slice<byte> dst, slice<byte> src) {
    if (len(dst) < len(src)) {
        throw panic("crypto/cipher: output smaller than input");
    }
    if (alias.InexactOverlap(dst.slice(0, len(src)), src)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    while (len(src) > 0) {
        if (x.outUsed == len(x.@out)) {
            x.b.Encrypt(x.@out, x.next);
            x.outUsed = 0;
        }
        if (x.decrypt) {
            // We can precompute a larger segment of the
            // keystream on decryption. This will allow
            // larger batches for xor, and we should be
            // able to match CTR/OFB performance.
            copy(x.next.slice(x.outUsed), src);
        }
        nint n = subtle.XORBytes(dst, src, x.@out.slice(x.outUsed));
        if (!x.decrypt) {
            copy(x.next.slice(x.outUsed), dst);
        }
        dst = dst.slice(n);
        src = src.slice(n);
        x.outUsed += n;
    }
}

// NewCFBEncrypter returns a [Stream] which encrypts with cipher feedback mode,
// using the given [Block]. The iv must be the same length as the [Block]'s block
// size.
//
// Deprecated: CFB mode is not authenticated, which generally enables active
// attacks to manipulate and recover the plaintext. It is recommended that
// applications use [AEAD] modes instead. The standard library implementation of
// CFB is also unoptimized and not validated as part of the FIPS 140-3 module.
// If an unauthenticated [Stream] mode is required, use [NewCTR] instead.
public static Stream NewCFBEncrypter(Block block, slice<byte> iv) {
    if (fips140only.Enabled) {
        throw panic("crypto/cipher: use of CFB is not allowed in FIPS 140-only mode");
    }
    return newCFB(block, iv, false);
}

// NewCFBDecrypter returns a [Stream] which decrypts with cipher feedback mode,
// using the given [Block]. The iv must be the same length as the [Block]'s block
// size.
//
// Deprecated: CFB mode is not authenticated, which generally enables active
// attacks to manipulate and recover the plaintext. It is recommended that
// applications use [AEAD] modes instead. The standard library implementation of
// CFB is also unoptimized and not validated as part of the FIPS 140-3 module.
// If an unauthenticated [Stream] mode is required, use [NewCTR] instead.
public static Stream NewCFBDecrypter(Block block, slice<byte> iv) {
    if (fips140only.Enabled) {
        throw panic("crypto/cipher: use of CFB is not allowed in FIPS 140-only mode");
    }
    return newCFB(block, iv, true);
}

internal static Stream newCFB(Block block, slice<byte> iv, bool decrypt) {
    ref var blockSize = ref heap<nint>(out var ᏑblockSize);
    blockSize = block.BlockSize();
    if (len(iv) != blockSize) {
        // stack trace will indicate whether it was de or encryption
        throw panic("cipher.newCFB: IV length must equal block size");
    }
    var x = Ꮡ(new cfb(
        b: block,
        @out: new slice<byte>(blockSize),
        next: new slice<byte>(blockSize),
        outUsed: blockSize,
        decrypt: decrypt
    ));
    copy((~x).next, iv);
    return new cfbжStream(x);
}

} // end cipher_package
