// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.aes;

using fips140 = go.crypto.@internal.fips140_package;
using aes = go.crypto.@internal.fips140.aes_package;
using alias = go.crypto.@internal.fips140.alias_package;
using errors = errors_package;
using System.Runtime.CompilerServices;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class gcm_package {

// GCM represents a Galois Counter Mode with a specific key.
partial struct GCM {
    internal aes.Block cipher;
    internal nint nonceSize;
    internal nint tagSize;
    internal partial ref gcmPlatformData gcmPlatformData { get; }
}

public static (ж<GCM>, error) New(ж<aes.Block> Ꮡcipher, nint nonceSize, nint tagSize) {
    // This function is outlined to let the allocation happen on the parent stack.
    return newGCM(Ꮡ(new GCM(nil)), Ꮡcipher, nonceSize, tagSize);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cipherIncorrectTagSizeˢ = "cipher: incorrect tag size given to GCM"u8;
private static readonly @string cipherTheNonceCanTHaveˢ = "cipher: the nonce can't have zero length"u8;
private static readonly @string cipherNewGCMRequires128ˢ = "cipher: NewGCM requires 128-bit block cipher"u8;

// newGCM is marked go:noinline to avoid it inlining into New, and making New
// too complex to inline itself.
//
//go:noinline
internal static partial (ж<GCM>, error) newGCM(ж<GCM> Ꮡg, ж<aes.Block> Ꮡcipher, nint nonceSize, nint tagSize) {
    ref var g = ref Ꮡg.DerefOrNull();
    ref var cipher = ref Ꮡcipher.DerefOrNull();

    if (tagSize < gcmMinimumTagSize || tagSize > gcmBlockSize) {
        return (default!, errors.New(cipherIncorrectTagSizeˢ));
    }
    if (nonceSize <= 0) {
        return (default!, errors.New(cipherTheNonceCanTHaveˢ));
    }
    if (cipher.BlockSize() != gcmBlockSize) {
        return (default!, errors.New(cipherNewGCMRequires128ˢ));
    }
    g.cipher = cipher;
    g.nonceSize = nonceSize;
    g.tagSize = tagSize;
    initGCM(ref (Ꮡg).DerefOrNull());
    return (Ꮡg, default!);
}

internal static UntypedInt gcmBlockSize => 16;
internal static UntypedInt gcmTagSize => 16;
internal static UntypedInt gcmMinimumTagSize => 12; // NIST SP 800-38D recommends tags with 12 or more bytes.
internal static UntypedInt gcmStandardNonceSize => 12;

public static nint NonceSize(this ref GCM g) {
    return g.nonceSize;
}

public static nint Overhead(this ref GCM g) {
    return g.tagSize;
}

public static slice<byte> Seal(this ж<GCM> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    fips140.RecordNonApproved();
    return Ꮡg.sealAfterIndicator(dst, nonce, plaintext, data);
}

internal static slice<byte> sealAfterIndicator(this ж<GCM> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> data) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (len(nonce) != g.nonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    if (g.nonceSize == 0) {
        throw panic("crypto/cipher: incorrect GCM nonce size");
    }
    if ((uint64)len(plaintext) > (uint64)((uint64)((4294967296L) - 2) * (uint64)gcmBlockSize)) {
        throw panic("crypto/cipher: message too large for GCM");
    }
    var (ret, @out) = sliceForAppend(dst, len(plaintext) + g.tagSize);
    if (alias.InexactOverlap(@out, plaintext)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and input");
    }
    if (alias.AnyOverlap(@out, data)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and additional data");
    }
    seal(@out, Ꮡg, nonce, plaintext, data);
    return ret;
}

internal static error errOpen = errors.New("cipher: message authentication failed"u8);

public static (slice<byte>, error) Open(this ж<GCM> Ꮡg, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> data) {
    ref var g = ref Ꮡg.DerefOrNull();

    if (len(nonce) != g.nonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }
    // Sanity check to prevent the authentication from always succeeding if an
    // implementation leaves tagSize uninitialized, for example.
    if (g.tagSize < gcmMinimumTagSize) {
        throw panic("crypto/cipher: incorrect GCM tag size");
    }
    if (len(ciphertext) < g.tagSize) {
        return (default!, errOpen);
    }
    if ((uint64)len(ciphertext) > (uint64)((uint64)((4294967296L) - 2) * (uint64)gcmBlockSize) + (uint64)g.tagSize) {
        return (default!, errOpen);
    }
    var (ret, @out) = sliceForAppend(dst, len(ciphertext) - g.tagSize);
    if (alias.InexactOverlap(@out, ciphertext)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and input");
    }
    if (alias.AnyOverlap(@out, data)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and additional data");
    }
    fips140.RecordApproved();
    {
        var err = open(@out, Ꮡg, nonce, ciphertext, data); if (err != default!) {
            // We sometimes decrypt and authenticate concurrently, so we overwrite
            // dst in the event of a tag mismatch. To be consistent across platforms
            // and to avoid releasing unauthenticated plaintext, we clear the buffer
            // in the event of an error.
            clear(@out);
            return (default!, err);
        }
    }
    return (ret, default!);
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

} // end gcm_package
