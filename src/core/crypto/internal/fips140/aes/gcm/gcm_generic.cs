// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140.aes;

using aes = go.crypto.@internal.fips140.aes_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using byteorder = go.crypto.@internal.fips140deps.byteorder_package;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140deps;

partial class gcm_package {

internal static void sealGeneric(slice<byte> @out, ж<GCM> Ꮡg, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    ref var H = ref heap(new array<byte>(16), out var ᏑH);
    ref var counter = ref heap(new array<byte>(16), out var Ꮡcounter);
    ref var tagMask = ref heap(new array<byte>(16), out var ᏑtagMask);
    aes.EncryptBlockInternal(Ꮡg.of(GCM.Ꮡcipher), H[..], H[..]);
    deriveCounterGeneric(ᏑH, Ꮡcounter, nonce);
    gcmCounterCryptGeneric(Ꮡg.of(GCM.Ꮡcipher), tagMask[..], tagMask[..], Ꮡcounter);
    gcmCounterCryptGeneric(Ꮡg.of(GCM.Ꮡcipher), @out, plaintext, Ꮡcounter);
    array<byte> tag = new(16); /* gcmTagSize */
    gcmAuthGeneric(tag[..], ᏑH, ᏑtagMask, @out.slice(0, len(plaintext)), additionalData);
    copy(@out.slice(len(plaintext)), tag[..]);
}

internal static error openGeneric(slice<byte> @out, ж<GCM> Ꮡg, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    ref var g = ref Ꮡg.DerefOrNull();

    ref var H = ref heap(new array<byte>(16), out var ᏑH);
    ref var counter = ref heap(new array<byte>(16), out var Ꮡcounter);
    ref var tagMask = ref heap(new array<byte>(16), out var ᏑtagMask);
    aes.EncryptBlockInternal(Ꮡg.of(GCM.Ꮡcipher), H[..], H[..]);
    deriveCounterGeneric(ᏑH, Ꮡcounter, nonce);
    gcmCounterCryptGeneric(Ꮡg.of(GCM.Ꮡcipher), tagMask[..], tagMask[..], Ꮡcounter);
    var tag = ciphertext.slice(len(ciphertext) - g.tagSize);
    ciphertext = ciphertext.slice(0, len(ciphertext) - g.tagSize);
    array<byte> expectedTag = new(16); /* gcmTagSize */
    gcmAuthGeneric(expectedTag[..], ᏑH, ᏑtagMask, ciphertext, additionalData);
    if (subtle.ConstantTimeCompare(expectedTag.slice(0, g.tagSize), tag) != 1) {
        return errOpen;
    }
    gcmCounterCryptGeneric(Ꮡg.of(GCM.Ꮡcipher), @out, ciphertext, Ꮡcounter);
    return default!;
}

// deriveCounterGeneric computes the initial GCM counter state from the given nonce.
// See NIST SP 800-38D, section 7.1. This assumes that counter is filled with
// zeros on entry.
internal static void deriveCounterGeneric([GoArrayDims(16)] ж<array<byte>> ᏑH, [GoArrayDims(16)] ж<array<byte>> Ꮡcounter, slice<byte> nonce) {
    ref var counter = ref Ꮡcounter.DerefOrNull();

    // GCM has two modes of operation with respect to the initial counter
    // state: a "fast path" for 96-bit (12-byte) nonces, and a "slow path"
    // for nonces of other lengths. For a 96-bit nonce, the nonce, along
    // with a four-byte big-endian counter starting at one, is used
    // directly as the starting counter. For other nonce sizes, the counter
    // is computed by passing it through the GHASH function.
    if (len(nonce) == gcmStandardNonceSize){
        copy(counter[..], nonce);
        counter[gcmBlockSize - 1] = 1;
    } else {
        var lenBlock = new slice<byte>(16);
        byteorder.BEPutUint64(lenBlock[8..], (uint64)len(nonce) * 8);
        ghash(Ꮡcounter, ᏑH, nonce, lenBlock);
    }
}

// gcmCounterCryptGeneric encrypts src using AES in counter mode with 32-bit
// wrapping (which is different from AES-CTR) and places the result into out.
// counter is the initial value and will be updated with the next value.
internal static void gcmCounterCryptGeneric(ж<aes.Block> Ꮡb, slice<byte> @out, slice<byte> src, [GoArrayDims(16)] ж<array<byte>> Ꮡcounter) {
    ref var counter = ref Ꮡcounter.DerefOrNull();

    array<byte> mask = new(16); /* gcmBlockSize */
    while (len(src) >= gcmBlockSize) {
        aes.EncryptBlockInternal(Ꮡb, mask[..], counter[..]);
        gcmInc32(Ꮡcounter);
        subtle.XORBytes(@out, src, mask[..]);
        @out = @out[(int)(gcmBlockSize)..];
        src = src[(int)(gcmBlockSize)..];
    }
    if (len(src) > 0) {
        aes.EncryptBlockInternal(Ꮡb, mask[..], counter[..]);
        gcmInc32(Ꮡcounter);
        subtle.XORBytes(@out, src, mask[..]);
    }
}

// gcmInc32 treats the final four bytes of counterBlock as a big-endian value
// and increments it.
internal static void gcmInc32([GoArrayDims(16)] ж<array<byte>> ᏑcounterBlock) {
    ref var counterBlock = ref ᏑcounterBlock.DerefOrNull();

    var ctr = counterBlock[(int)(16 - 4)..];
    byteorder.BEPutUint32(ctr, byteorder.BEUint32(ctr) + 1);
}

// gcmAuthGeneric calculates GHASH(additionalData, ciphertext), masks the result
// with tagMask and writes the result to out.
internal static void gcmAuthGeneric(slice<byte> @out, [GoArrayDims(16)] ж<array<byte>> ᏑH, [GoArrayDims(16)] ж<array<byte>> ᏑtagMask, slice<byte> ciphertext, slice<byte> additionalData) {
    ref var tagMask = ref ᏑtagMask.DerefOrNull();

    checkGenericIsExpected();
    var lenBlock = new slice<byte>(16);
    byteorder.BEPutUint64(lenBlock[..8], (uint64)len(additionalData) * 8);
    byteorder.BEPutUint64(lenBlock[8..], (uint64)len(ciphertext) * 8);
    ref var S = ref heap(new array<byte>(16), out var ᏑS);
    ghash(ᏑS, ᏑH, additionalData, ciphertext, lenBlock);
    subtle.XORBytes(@out, S[..], tagMask[..]);
}

} // end gcm_package
