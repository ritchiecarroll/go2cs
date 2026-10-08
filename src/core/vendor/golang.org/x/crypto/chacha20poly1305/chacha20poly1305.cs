// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package chacha20poly1305 implements the ChaCha20-Poly1305 AEAD and its
// extended nonce variant XChaCha20-Poly1305, as specified in RFC 8439 and
// draft-irtf-cfrg-xchacha-01.
namespace go.vendor.golang.org.x.crypto;

using cipher = go.crypto.cipher_package;
using errors = errors_package;
using go.crypto;

partial class chacha20poly1305_package {

public static UntypedInt KeySize => 32;
public static UntypedInt ΔNonceSize => 12;
public static UntypedInt NonceSizeX => 24;
public static UntypedInt ΔOverhead => 16;

partial struct chacha20poly1305 {
    internal array<byte> key = new(KeySize);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string chacha20poly1305BadKeyˢ = "chacha20poly1305: bad key length"u8;

// New returns a ChaCha20-Poly1305 AEAD that uses the given 256-bit key.
public static (cipher.AEAD, error) New(slice<byte> key) {
    if (len(key) != KeySize) {
        return (default!, errors.New(chacha20poly1305BadKeyˢ));
    }
    var ret = @new<chacha20poly1305>();
    copy((~ret).key[..], key);
    return (new chacha20poly1305жAEAD(ret), default!);
}

internal static nint NonceSize(this ref chacha20poly1305 c) {
    return ΔNonceSize;
}

internal static nint Overhead(this ref chacha20poly1305 c) {
    return ΔOverhead;
}

internal static slice<byte> Seal(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    if (len(nonce) != ΔNonceSize) {
        throw panic("chacha20poly1305: bad nonce length passed to Seal");
    }
    if ((uint64)len(plaintext) > (uint64)((274877906944L) - 64)) {
        throw panic("chacha20poly1305: plaintext too large");
    }
    return c.seal(dst, nonce, plaintext, additionalData);
}

internal static error errOpen = errors.New("chacha20poly1305: message authentication failed"u8);

internal static (slice<byte>, error) Open(this ref chacha20poly1305 c, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    if (len(nonce) != ΔNonceSize) {
        throw panic("chacha20poly1305: bad nonce length passed to Open");
    }
    if (len(ciphertext) < 16) {
        return (default!, errOpen);
    }
    if ((uint64)len(ciphertext) > (uint64)((274877906944L) - 48)) {
        throw panic("chacha20poly1305: ciphertext too large");
    }
    return c.open(dst, nonce, ciphertext, additionalData);
}

// sliceForAppend takes a slice and a requested number of bytes. It returns a
// slice with the contents of the given slice followed by that many bytes and a
// second slice that aliases into it and contains only the extra bytes. If the
// original slice has sufficient capacity then no allocation is performed.
internal static (slice<byte> head, slice<byte> tail) sliceForAppend(slice<byte> @in, nint n) {
    slice<byte> head = default!;
    slice<byte> tail = default!;

    {
        nint total = len(@in) + n; if (cap(@in) >= total){
            head = @in.slice(0, total);
        } else {
            head = new slice<byte>(total);
            copy(head, @in);
        }
    }
    tail = head.slice(len(@in));
    return (head, tail);
}

} // end chacha20poly1305_package
