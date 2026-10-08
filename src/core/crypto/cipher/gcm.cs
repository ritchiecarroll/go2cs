// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using aes = go.crypto.@internal.fips140.aes_package;
using gcm = go.crypto.@internal.fips140.aes.gcm_package;
using alias = go.crypto.@internal.fips140.alias_package;
using fips140only = go.crypto.@internal.fips140only_package;
using subtle = go.crypto.subtle_package;
using errors = errors_package;
using byteorder = go.@internal.byteorder_package;
using go.@internal;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140.aes;

partial class cipher_package {

internal static UntypedInt gcmBlockSize => 16;
internal static UntypedInt gcmStandardNonceSize => 12;
internal static UntypedInt gcmTagSize => 16;
internal static UntypedInt gcmMinimumTagSize => 12; // NIST SP 800-38D recommends tags with 12 or more bytes.

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoCipherUseOfGcmWithˢ = "crypto/cipher: use of GCM with arbitrary IVs is not allowed in FIPS 140-only mode, use NewGCMWithRandomNonce"u8;

// NewGCM returns the given 128-bit, block cipher wrapped in Galois Counter Mode
// with the standard nonce length.
//
// In general, the GHASH operation performed by this implementation of GCM is not constant-time.
// An exception is when the underlying [Block] was created by aes.NewCipher
// on systems with hardware support for AES. See the [crypto/aes] package documentation for details.
public static (AEAD, error) NewGCM(Block cipher) {
    if (fips140only.Enabled) {
        return (default!, errors.New(cryptoCipherUseOfGcmWithˢ));
    }
    return newGCM(cipher, gcmStandardNonceSize, gcmTagSize);
}

// NewGCMWithNonceSize returns the given 128-bit, block cipher wrapped in Galois
// Counter Mode, which accepts nonces of the given length. The length must not
// be zero.
//
// Only use this function if you require compatibility with an existing
// cryptosystem that uses non-standard nonce lengths. All other users should use
// [NewGCM], which is faster and more resistant to misuse.
public static (AEAD, error) NewGCMWithNonceSize(Block cipher, nint size) {
    if (fips140only.Enabled) {
        return (default!, errors.New(cryptoCipherUseOfGcmWithˢ));
    }
    return newGCM(cipher, size, gcmTagSize);
}

// NewGCMWithTagSize returns the given 128-bit, block cipher wrapped in Galois
// Counter Mode, which generates tags with the given length.
//
// Tag sizes between 12 and 16 bytes are allowed.
//
// Only use this function if you require compatibility with an existing
// cryptosystem that uses non-standard tag lengths. All other users should use
// [NewGCM], which is more resistant to misuse.
public static (AEAD, error) NewGCMWithTagSize(Block cipher, nint tagSize) {
    if (fips140only.Enabled) {
        return (default!, errors.New(cryptoCipherUseOfGcmWithˢ));
    }
    return newGCM(cipher, gcmStandardNonceSize, tagSize);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cryptoCipherUseOfGcmWithˢ2 = "crypto/cipher: use of GCM with non-AES ciphers is not allowed in FIPS 140-only mode"u8;

internal static (AEAD, error) newGCM(Block cipher, nint nonceSize, nint tagSize) {
    var (c, ok) = cipher._<ж<aes.Block>>(ᐧ);
    if (!ok) {
        if (fips140only.Enabled) {
            return (default!, errors.New(cryptoCipherUseOfGcmWithˢ2));
        }
        return newGCMFallback(cipher, nonceSize, tagSize);
    }
    // We don't return gcm.New directly, because it would always return a non-nil
    // AEAD interface value with type *gcm.GCM even if the *gcm.GCM is nil.
    var (g, err) = gcm.New(c, nonceSize, tagSize);
    if (err != default!) {
        return (default!, err);
    }
    return (new gcm_GCMжAEAD(g), default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cipherˢ = "cipher: NewGCMWithRandomNonce requires aes.Block"u8;

// NewGCMWithRandomNonce returns the given cipher wrapped in Galois Counter
// Mode, with randomly-generated nonces. The cipher must have been created by
// [aes.NewCipher].
//
// It generates a random 96-bit nonce, which is prepended to the ciphertext by Seal,
// and is extracted from the ciphertext by Open. The NonceSize of the AEAD is zero,
// while the Overhead is 28 bytes (the combination of nonce size and tag size).
//
// A given key MUST NOT be used to encrypt more than 2^32 messages, to limit the
// risk of a random nonce collision to negligible levels.
public static (AEAD, error) NewGCMWithRandomNonce(Block cipher) {
    var (c, ok) = cipher._<ж<aes.Block>>(ᐧ);
    if (!ok) {
        return (default!, errors.New(cipherˢ));
    }
    var (g, err) = gcm.New(c, gcmStandardNonceSize, gcmTagSize);
    if (err != default!) {
        return (default!, err);
    }
    return (new gcmWithRandomNonce(g), default!);
}

partial struct gcmWithRandomNonce {
    public partial ref ж<crypto.@internal.fips140.aes.gcm_package.GCM> GCM { get; }
}

internal static nint NonceSize(this gcmWithRandomNonce g) {
    return 0;
}

internal static nint Overhead(this gcmWithRandomNonce g) {
    return gcmStandardNonceSize + gcmTagSize;
}

internal static slice<byte> Seal(this gcmWithRandomNonce g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    if (len(nonce) != 0) {
        throw panic("crypto/cipher: non-empty nonce passed to GCMWithRandomNonce");
    }
    var (ret, @out) = sliceForAppend(dst, (nint)gcmStandardNonceSize + len(plaintext) + (nint)gcmTagSize);
    if (alias.InexactOverlap(@out, plaintext)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and input");
    }
    if (alias.AnyOverlap(@out, additionalData)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and additional data");
    }
    nonce = @out[..(int)(gcmStandardNonceSize)];
    var ciphertext = @out[(int)(gcmStandardNonceSize)..];
    // The AEAD interface allows using plaintext[:0] or ciphertext[:0] as dst.
    //
    // This is kind of a problem when trying to prepend or trim a nonce, because the
    // actual AES-GCTR blocks end up overlapping but not exactly.
    //
    // In Open, we write the output *before* the input, so unless we do something
    // weird like working through a chunk of block backwards, it works out.
    //
    // In Seal, we could work through the input backwards or intentionally load
    // ahead before writing.
    //
    // However, the crypto/internal/fips140/aes/gcm APIs also check for exact overlap,
    // so for now we just do a memmove if we detect overlap.
    //
    //     ┌───────────────────────────┬ ─ ─
    //     │PPPPPPPPPPPPPPPPPPPPPPPPPPP│    │
    //     └▽─────────────────────────▲┴ ─ ─
    //       ╲ Seal                    ╲
    //        ╲                    Open ╲
    //     ┌───▼─────────────────────────△──┐
    //     │NN|CCCCCCCCCCCCCCCCCCCCCCCCCCC|T│
    //     └────────────────────────────────┘
    //
    if (alias.AnyOverlap(@out, plaintext)) {
        copy(ciphertext, plaintext);
        plaintext = ciphertext.slice(0, len(plaintext));
    }
    gcm.SealWithRandomNonce(g.GCM, nonce, ciphertext, plaintext, additionalData);
    return ret;
}

internal static (slice<byte>, error) Open(this gcmWithRandomNonce g, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    if (len(nonce) != 0) {
        throw panic("crypto/cipher: non-empty nonce passed to GCMWithRandomNonce");
    }
    if (len(ciphertext) < (nint)(gcmStandardNonceSize + gcmTagSize)) {
        return (default!, errOpen);
    }
    var (ret, @out) = sliceForAppend(dst, len(ciphertext) - (nint)gcmStandardNonceSize - (nint)gcmTagSize);
    if (alias.InexactOverlap(@out, ciphertext)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and input");
    }
    if (alias.AnyOverlap(@out, additionalData)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and additional data");
    }
    // See the discussion in Seal. Note that if there is any overlap at this
    // point, it's because out = ciphertext, so out must have enough capacity
    // even if we sliced the tag off. Also note how [AEAD] specifies that "the
    // contents of dst, up to its capacity, may be overwritten".
    if (alias.AnyOverlap(@out, ciphertext)){
        nonce = new slice<byte>(gcmStandardNonceSize);
        copy(nonce, ciphertext);
        copy(@out.slice(0, len(ciphertext)), ciphertext[(int)(gcmStandardNonceSize)..]);
        ciphertext = @out.slice(0, len(ciphertext) - (nint)gcmStandardNonceSize);
    } else {
        nonce = ciphertext[..(int)(gcmStandardNonceSize)];
        ciphertext = ciphertext[(int)(gcmStandardNonceSize)..];
    }
    var (_, err) = g.GCM.Open(@out[..0], nonce, ciphertext, additionalData);
    if (err != default!) {
        return (default!, err);
    }
    return (ret, default!);
}

// gcmAble is an interface implemented by ciphers that have a specific optimized
// implementation of GCM. crypto/aes doesn't use this anymore, and we'd like to
// eventually remove it.
partial interface gcmAble {
    (AEAD, error) NewGCM(nint nonceSize, nint tagSize);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cipherIncorrectTagSizeˢ = "cipher: incorrect tag size given to GCM"u8;
private static readonly @string cipherTheNonceCanTHaveˢ = "cipher: the nonce can't have zero length"u8;
private static readonly @string cipherNewGCMRequires128ˢ = "cipher: NewGCM requires 128-bit block cipher"u8;

internal static (AEAD, error) newGCMFallback(Block cipher, nint nonceSize, nint tagSize) {
    if (tagSize < gcmMinimumTagSize || tagSize > gcmBlockSize) {
        return (default!, errors.New(cipherIncorrectTagSizeˢ));
    }
    if (nonceSize <= 0) {
        return (default!, errors.New(cipherTheNonceCanTHaveˢ));
    }
    {
        var (cipherΔ1, ok) = cipher._<gcmAble>(ᐧ); if (ok) {
            return cipherΔ1.NewGCM(nonceSize, tagSize);
        }
    }
    if (cipher.BlockSize() != gcmBlockSize) {
        return (default!, errors.New(cipherNewGCMRequires128ˢ));
    }
    return (new gcmFallbackжAEAD(Ꮡ(new gcmFallback(cipher: cipher, nonceSize: nonceSize, tagSize: tagSize))), default!);
}

// gcmFallback is only used for non-AES ciphers, which regrettably we
// theoretically support. It's a copy of the generic implementation from
// crypto/internal/fips140/aes/gcm/gcm_generic.go, refer to that file for more details.
partial struct gcmFallback {
    internal Block cipher;
    internal nint nonceSize;
    internal nint tagSize;
}

internal static nint NonceSize(this ref gcmFallback g) {
    return g.nonceSize;
}

internal static nint Overhead(this ref gcmFallback g) {
    return g.tagSize;
}

internal static slice<byte> Seal(this ref gcmFallback g, slice<byte> dst, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
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
    if (alias.AnyOverlap(@out, additionalData)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and additional data");
    }
    ref var H = ref heap(new array<byte>(16), out var ᏑH);
    ref var counter = ref heap(new array<byte>(16), out var Ꮡcounter);
    ref var tagMask = ref heap(new array<byte>(16), out var ᏑtagMask);
    g.cipher.Encrypt(H[..], H[..]);
    deriveCounter(ᏑH, Ꮡcounter, nonce);
    gcmCounterCryptGeneric(g.cipher, tagMask[..], tagMask[..], Ꮡcounter);
    gcmCounterCryptGeneric(g.cipher, @out, plaintext, Ꮡcounter);
    array<byte> tag = new(16); /* gcmTagSize */
    gcmAuth(tag[..], ᏑH, ᏑtagMask, @out.slice(0, len(plaintext)), additionalData);
    copy(@out.slice(len(plaintext)), tag[..]);
    return ret;
}

internal static error errOpen = errors.New("cipher: message authentication failed"u8);

internal static (slice<byte>, error) Open(this ref gcmFallback g, slice<byte> dst, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    if (len(nonce) != g.nonceSize) {
        throw panic("crypto/cipher: incorrect nonce length given to GCM");
    }
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
    if (alias.AnyOverlap(@out, additionalData)) {
        throw panic("crypto/cipher: invalid buffer overlap of output and additional data");
    }
    ref var H = ref heap(new array<byte>(16), out var ᏑH);
    ref var counter = ref heap(new array<byte>(16), out var Ꮡcounter);
    ref var tagMask = ref heap(new array<byte>(16), out var ᏑtagMask);
    g.cipher.Encrypt(H[..], H[..]);
    deriveCounter(ᏑH, Ꮡcounter, nonce);
    gcmCounterCryptGeneric(g.cipher, tagMask[..], tagMask[..], Ꮡcounter);
    var tag = ciphertext.slice(len(ciphertext) - g.tagSize);
    ciphertext = ciphertext.slice(0, len(ciphertext) - g.tagSize);
    array<byte> expectedTag = new(16); /* gcmTagSize */
    gcmAuth(expectedTag[..], ᏑH, ᏑtagMask, ciphertext, additionalData);
    if (subtle.ConstantTimeCompare(expectedTag.slice(0, g.tagSize), tag) != 1) {
        // We sometimes decrypt and authenticate concurrently, so we overwrite
        // dst in the event of a tag mismatch. To be consistent across platforms
        // and to avoid releasing unauthenticated plaintext, we clear the buffer
        // in the event of an error.
        clear(@out);
        return (default!, errOpen);
    }
    gcmCounterCryptGeneric(g.cipher, @out, ciphertext, Ꮡcounter);
    return (ret, default!);
}

internal static void deriveCounter(/*[16]*/ ж<array<byte>> ᏑH, /*[16]*/ ж<array<byte>> Ꮡcounter, slice<byte> nonce) {
    ref var counter = ref Ꮡcounter.DerefOrNull();

    if (len(nonce) == gcmStandardNonceSize){
        copy(counter[..], nonce);
        counter[gcmBlockSize - 1] = 1;
    } else {
        var lenBlock = new slice<byte>(16);
        byteorder.BEPutUint64(lenBlock[8..], (uint64)len(nonce) * 8);
        var J = gcm.GHASH(ᏑH, nonce, lenBlock);
        copy(counter[..], J);
    }
}

internal static void gcmCounterCryptGeneric(Block b, slice<byte> @out, slice<byte> src, /*[16]*/ ж<array<byte>> Ꮡcounter) {
    ref var counter = ref Ꮡcounter.DerefOrNull();

    array<byte> mask = new(16); /* gcmBlockSize */
    while (len(src) >= gcmBlockSize) {
        b.Encrypt(mask[..], counter[..]);
        gcmInc32(Ꮡcounter);
        subtle.XORBytes(@out, src, mask[..]);
        @out = @out[(int)(gcmBlockSize)..];
        src = src[(int)(gcmBlockSize)..];
    }
    if (len(src) > 0) {
        b.Encrypt(mask[..], counter[..]);
        gcmInc32(Ꮡcounter);
        subtle.XORBytes(@out, src, mask[..]);
    }
}

internal static void gcmInc32(/*[16]*/ ж<array<byte>> ᏑcounterBlock) {
    ref var counterBlock = ref ᏑcounterBlock.DerefOrNull();

    var ctr = counterBlock[(int)(16 - 4)..];
    byteorder.BEPutUint32(ctr, byteorder.BEUint32(ctr) + 1);
}

internal static void gcmAuth(slice<byte> @out, /*[16]*/ ж<array<byte>> ᏑH, /*[16]*/ ж<array<byte>> ᏑtagMask, slice<byte> ciphertext, slice<byte> additionalData) {
    ref var tagMask = ref ᏑtagMask.DerefOrNull();

    var lenBlock = new slice<byte>(16);
    byteorder.BEPutUint64(lenBlock[..8], (uint64)len(additionalData) * 8);
    byteorder.BEPutUint64(lenBlock[8..], (uint64)len(ciphertext) * 8);
    var S = gcm.GHASH(ᏑH, additionalData, ciphertext, lenBlock);
    subtle.XORBytes(@out, S, tagMask[..]);
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

} // end cipher_package
