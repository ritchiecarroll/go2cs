// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Cipher block chaining (CBC) mode.
// CBC provides confidentiality by xoring (chaining) each plaintext block
// with the previous ciphertext block before applying the block cipher.
// See NIST SP 800-38A, pp 10-11
namespace go.crypto;

using bytes = bytes_package;
using aes = go.crypto.@internal.fips140.aes_package;
using alias = go.crypto.@internal.fips140.alias_package;
using fips140only = go.crypto.@internal.fips140only_package;
using subtle = go.crypto.subtle_package;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class cipher_package {

[GoType] partial struct cbc {
    internal Block b;
    internal nint blockSize;
    internal slice<byte> iv;
    internal slice<byte> tmp;
}

internal static ж<cbc> newCBC(Block b, slice<byte> iv) {
    return Ꮡ(new cbc(
        b: b,
        blockSize: b.BlockSize(),
        iv: bytes.Clone(iv),
        tmp: new slice<byte>(b.BlockSize())
    ));
}

[GoType("cbc")] partial struct cbcEncrypter;

// cbcEncAble is an interface implemented by ciphers that have a specific
// optimized implementation of CBC encryption. crypto/aes doesn't use this
// anymore, and we'd like to eventually remove it.
[GoType] partial interface cbcEncAble {
    BlockMode NewCBCEncrypter(slice<byte> iv);
}

// NewCBCEncrypter returns a BlockMode which encrypts in cipher block chaining
// mode, using the given Block. The length of iv must be the same as the
// Block's block size.
public static BlockMode NewCBCEncrypter(Block b, slice<byte> iv) {
    if (len(iv) != b.BlockSize()) {
        throw panic("cipher.NewCBCEncrypter: IV length must equal block size");
    }
    {
        var (bΔ1, ok) = b._<ж<aes.Block>>(ᐧ); if (ok) {
            return new aes_CBCEncrypterжBlockMode(aes.NewCBCEncrypter(bΔ1, new array<byte>(iv, 16)));
        }
    }
    if (fips140only.Enabled) {
        throw panic("crypto/cipher: use of CBC with non-AES ciphers is not allowed in FIPS 140-only mode");
    }
    {
        var (cbc, ok) = b._<cbcEncAble>(ᐧ); if (ok) {
            return cbc.NewCBCEncrypter(iv);
        }
    }
    return new cbcEncrypterжBlockMode(newCBC(b, iv).Reinterpret<cbc, cbcEncrypter>());
}

// newCBCGenericEncrypter returns a BlockMode which encrypts in cipher block chaining
// mode, using the given Block. The length of iv must be the same as the
// Block's block size. This always returns the generic non-asm encrypter for use
// in fuzz testing.
internal static BlockMode newCBCGenericEncrypter(Block b, slice<byte> iv) {
    if (len(iv) != b.BlockSize()) {
        throw panic("cipher.NewCBCEncrypter: IV length must equal block size");
    }
    return new cbcEncrypterжBlockMode(newCBC(b, iv).Reinterpret<cbc, cbcEncrypter>());
}

[GoRecv] internal static nint BlockSize(this ref cbcEncrypter x) {
    return x.blockSize;
}

[GoRecv] internal static void CryptBlocks(this ref cbcEncrypter x, slice<byte> dst, slice<byte> src) {
    if (len(src) % x.blockSize != 0) {
        throw panic("crypto/cipher: input not full blocks");
    }
    if (len(dst) < len(src)) {
        throw panic("crypto/cipher: output smaller than input");
    }
    if (alias.InexactOverlap(dst[..(int)(len(src))], src)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    {
        var (_, ok) = x.b._<ж<aes.Block>>(ᐧ); if (ok) {
            throw panic("crypto/cipher: internal error: generic CBC used with AES");
        }
    }
    var iv = x.iv;
    while (len(src) > 0) {
        // Write the xor to dst, then encrypt in place.
        subtle.XORBytes(dst[..(int)(x.blockSize)], src[..(int)(x.blockSize)], iv);
        x.b.Encrypt(dst[..(int)(x.blockSize)], dst[..(int)(x.blockSize)]);
        // Move to the next block with this block as the next iv.
        iv = dst[..(int)(x.blockSize)];
        src = src[(int)(x.blockSize)..];
        dst = dst[(int)(x.blockSize)..];
    }
    // Save the iv for the next CryptBlocks call.
    copy(x.iv, iv);
}

[GoRecv] internal static void SetIV(this ref cbcEncrypter x, slice<byte> iv) {
    if (len(iv) != len(x.iv)) {
        throw panic("cipher: incorrect length IV");
    }
    copy(x.iv, iv);
}

[GoType("cbc")] partial struct cbcDecrypter;

// cbcDecAble is an interface implemented by ciphers that have a specific
// optimized implementation of CBC decryption. crypto/aes doesn't use this
// anymore, and we'd like to eventually remove it.
[GoType] partial interface cbcDecAble {
    BlockMode NewCBCDecrypter(slice<byte> iv);
}

// NewCBCDecrypter returns a BlockMode which decrypts in cipher block chaining
// mode, using the given Block. The length of iv must be the same as the
// Block's block size and must match the iv used to encrypt the data.
public static BlockMode NewCBCDecrypter(Block b, slice<byte> iv) {
    if (len(iv) != b.BlockSize()) {
        throw panic("cipher.NewCBCDecrypter: IV length must equal block size");
    }
    {
        var (bΔ1, ok) = b._<ж<aes.Block>>(ᐧ); if (ok) {
            return new aes_CBCDecrypterжBlockMode(aes.NewCBCDecrypter(bΔ1, new array<byte>(iv, 16)));
        }
    }
    if (fips140only.Enabled) {
        throw panic("crypto/cipher: use of CBC with non-AES ciphers is not allowed in FIPS 140-only mode");
    }
    {
        var (cbc, ok) = b._<cbcDecAble>(ᐧ); if (ok) {
            return cbc.NewCBCDecrypter(iv);
        }
    }
    return new cbcDecrypterжBlockMode(newCBC(b, iv).Reinterpret<cbc, cbcDecrypter>());
}

// newCBCGenericDecrypter returns a BlockMode which encrypts in cipher block chaining
// mode, using the given Block. The length of iv must be the same as the
// Block's block size. This always returns the generic non-asm decrypter for use in
// fuzz testing.
internal static BlockMode newCBCGenericDecrypter(Block b, slice<byte> iv) {
    if (len(iv) != b.BlockSize()) {
        throw panic("cipher.NewCBCDecrypter: IV length must equal block size");
    }
    return new cbcDecrypterжBlockMode(newCBC(b, iv).Reinterpret<cbc, cbcDecrypter>());
}

[GoRecv] internal static nint BlockSize(this ref cbcDecrypter x) {
    return x.blockSize;
}

[GoRecv] internal static void CryptBlocks(this ref cbcDecrypter x, slice<byte> dst, slice<byte> src) {
    if (len(src) % x.blockSize != 0) {
        throw panic("crypto/cipher: input not full blocks");
    }
    if (len(dst) < len(src)) {
        throw panic("crypto/cipher: output smaller than input");
    }
    if (alias.InexactOverlap(dst[..(int)(len(src))], src)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    {
        var (_, ok) = x.b._<ж<aes.Block>>(ᐧ); if (ok) {
            throw panic("crypto/cipher: internal error: generic CBC used with AES");
        }
    }
    if (len(src) == 0) {
        return;
    }
    // For each block, we need to xor the decrypted data with the previous block's ciphertext (the iv).
    // To avoid making a copy each time, we loop over the blocks BACKWARDS.
    nint end = len(src);
    nint start = end - x.blockSize;
    nint prev = start - x.blockSize;
    // Copy the last block of ciphertext in preparation as the new iv.
    copy(x.tmp, src[(int)(start)..(int)(end)]);
    // Loop over all but the first block.
    while (start > 0) {
        x.b.Decrypt(dst[(int)(start)..(int)(end)], src[(int)(start)..(int)(end)]);
        subtle.XORBytes(dst[(int)(start)..(int)(end)], dst[(int)(start)..(int)(end)], src[(int)(prev)..(int)(start)]);
        end = start;
        start = prev;
        prev -= x.blockSize;
    }
    // The first block is special because it uses the saved iv.
    x.b.Decrypt(dst[(int)(start)..(int)(end)], src[(int)(start)..(int)(end)]);
    subtle.XORBytes(dst[(int)(start)..(int)(end)], dst[(int)(start)..(int)(end)], x.iv);
    // Set the new iv to the first block we copied earlier.
    (x.iv, x.tmp) = (x.tmp, x.iv);
}

[GoRecv] internal static void SetIV(this ref cbcDecrypter x, slice<byte> iv) {
    if (len(iv) != len(x.iv)) {
        throw panic("cipher: incorrect length IV");
    }
    copy(x.iv, iv);
}

} // end cipher_package
