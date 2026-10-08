// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using alias = go.crypto.@internal.fips140.alias_package;
using subtle = go.crypto.@internal.fips140.subtle_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class aes_package {

partial struct CBCEncrypter {
    internal Block b;
    internal array<byte> iv = new(ΔBlockSize);
}

// NewCBCEncrypter returns a [cipher.BlockMode] which encrypts in cipher block
// chaining mode, using the given Block.
public static ж<CBCEncrypter> NewCBCEncrypter(ж<Block> Ꮡb, /*[16]*/ array<byte> iv) {
    iv = iv.Clone();

    ref var b = ref Ꮡb.DerefOrNull();
    return Ꮡ(new CBCEncrypter(b: b, iv: iv.Clone()));
}

public static nint BlockSize(this ref CBCEncrypter c) {
    return ΔBlockSize;
}

public static void CryptBlocks(this ж<CBCEncrypter> Ꮡc, slice<byte> dst, slice<byte> src) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (len(src) % (nint)ΔBlockSize != 0) {
        throw panic("crypto/cipher: input not full blocks");
    }
    if (len(dst) < len(src)) {
        throw panic("crypto/cipher: output smaller than input");
    }
    if (alias.InexactOverlap(dst.slice(0, len(src)), src)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    fips140.RecordApproved();
    if (len(src) == 0) {
        return;
    }
    cryptBlocksEnc(ref nonnil(ref c).b, Ꮡc.of(CBCEncrypter.Ꮡiv), dst, src);
}

public static void SetIV(this ref CBCEncrypter x, slice<byte> iv) {
    if (len(iv) != len(x.iv)) {
        throw panic("cipher: incorrect length IV");
    }
    copy(x.iv[..], iv);
}

internal static void cryptBlocksEncGeneric(ref Block b, /*[16]*/ ж<array<byte>> Ꮡciv, slice<byte> dst, slice<byte> src) {
    ref var civ = ref Ꮡciv.DerefOrNull();

    var iv = civ[..];
    while (len(src) > 0) {
        // Write the xor to dst, then encrypt in place.
        subtle.XORBytes(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)], iv);
        encryptBlock(ref b, dst[..(int)(ΔBlockSize)], dst[..(int)(ΔBlockSize)]);
        // Move to the next block with this block as the next iv.
        iv = dst[..(int)(ΔBlockSize)];
        src = src[(int)(ΔBlockSize)..];
        dst = dst[(int)(ΔBlockSize)..];
    }
    // Save the iv for the next CryptBlocks call.
    copy(civ[..], iv);
}

partial struct CBCDecrypter {
    internal Block b;
    internal array<byte> iv = new(ΔBlockSize);
}

// NewCBCDecrypter returns a [cipher.BlockMode] which decrypts in cipher block
// chaining mode, using the given Block.
public static ж<CBCDecrypter> NewCBCDecrypter(ж<Block> Ꮡb, /*[16]*/ array<byte> iv) {
    iv = iv.Clone();

    ref var b = ref Ꮡb.DerefOrNull();
    return Ꮡ(new CBCDecrypter(b: b, iv: iv.Clone()));
}

public static nint BlockSize(this ref CBCDecrypter c) {
    return ΔBlockSize;
}

public static void CryptBlocks(this ж<CBCDecrypter> Ꮡc, slice<byte> dst, slice<byte> src) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (len(src) % (nint)ΔBlockSize != 0) {
        throw panic("crypto/cipher: input not full blocks");
    }
    if (len(dst) < len(src)) {
        throw panic("crypto/cipher: output smaller than input");
    }
    if (alias.InexactOverlap(dst.slice(0, len(src)), src)) {
        throw panic("crypto/cipher: invalid buffer overlap");
    }
    fips140.RecordApproved();
    if (len(src) == 0) {
        return;
    }
    cryptBlocksDec(ref nonnil(ref c).b, Ꮡc.of(CBCDecrypter.Ꮡiv), dst, src);
}

public static void SetIV(this ref CBCDecrypter x, slice<byte> iv) {
    if (len(iv) != len(x.iv)) {
        throw panic("cipher: incorrect length IV");
    }
    copy(x.iv[..], iv);
}

internal static void cryptBlocksDecGeneric(ref Block b, /*[16]*/ ж<array<byte>> Ꮡciv, slice<byte> dst, slice<byte> src) {
    ref var civ = ref Ꮡciv.DerefOrNull();

    // For each block, we need to xor the decrypted data with the previous
    // block's ciphertext (the iv). To avoid making a copy each time, we loop
    // over the blocks backwards.
    nint end = len(src);
    nint start = end - (nint)ΔBlockSize;
    nint prev = start - (nint)ΔBlockSize;
    // Copy the last block of ciphertext as the IV of the next call.
    var iv = civ.Clone();
    copy(civ[..], src.slice(start, end));
    while (start >= 0) {
        decryptBlock(ref b, dst.slice(start, end), src.slice(start, end));
        if (start > 0){
            subtle.XORBytes(dst.slice(start, end), dst.slice(start, end), src.slice(prev, start));
        } else {
            // The first block is special because it uses the saved iv.
            subtle.XORBytes(dst.slice(start, end), dst.slice(start, end), iv[..]);
        }
        end -= ΔBlockSize;
        start -= ΔBlockSize;
        prev -= ΔBlockSize;
    }
}

} // end aes_package
