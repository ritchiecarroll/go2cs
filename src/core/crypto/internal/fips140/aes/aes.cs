// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using fips140 = go.crypto.@internal.fips140_package;
using alias = go.crypto.@internal.fips140.alias_package;
using strconv = strconv_package;
using System.Runtime.CompilerServices;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class aes_package {

// BlockSize is the AES block size in bytes.
public static UntypedInt ΔBlockSize => 16;

// A Block is an instance of AES using a particular key.
// It is safe for concurrent use.
[GoType] partial struct Block {
    internal partial ref block block { get; }
}

// blockExpanded is the block type used for all architectures except s390x,
// which feeds the raw key directly to its instructions.
[GoType] partial struct blockExpanded {
    internal nint rounds;
    // Round keys, where only the first (rounds + 1) × (128 ÷ 32) words are used.
    internal array<uint32> enc = new(60);
    internal array<uint32> dec = new(60);
}

// AES-128 has 128-bit keys, 10 rounds, and uses 11 128-bit round keys
// (11×128÷32 = 44 32-bit words).
// AES-192 has 192-bit keys, 12 rounds, and uses 13 128-bit round keys
// (13×128÷32 = 52 32-bit words).
// AES-256 has 256-bit keys, 14 rounds, and uses 15 128-bit round keys
// (15×128÷32 = 60 32-bit words).
internal static UntypedInt aes128KeySize => 16;
internal static UntypedInt aes192KeySize => 24;
internal static UntypedInt aes256KeySize => 32;
internal static UntypedInt aes128Rounds => 10;
internal static UntypedInt aes192Rounds => 12;
internal static UntypedInt aes256Rounds => 14;

// roundKeysSize returns the number of uint32 of c.end or c.dec that are used.
[GoRecv] internal static nint roundKeysSize(this ref blockExpanded b) {
    return (b.rounds + 1) * (128 / 32);
}

[GoType("num:nint")] partial struct KeySizeError;

public static @string Error(this KeySizeError k) {
    return "crypto/aes: invalid key size "u8 + strconv.Itoa((nint)k);
}

// New creates and returns a new [cipher.Block] implementation.
// The key argument should be the AES key, either 16, 24, or 32 bytes to select
// AES-128, AES-192, or AES-256.
public static (ж<Block>, error) New(slice<byte> key) {
    // This call is outline to let the allocation happen on the parent stack.
    return newOutlined(Ꮡ(new Block(nil)), key);
}

// newOutlined is marked go:noinline to avoid it inlining into New, and making New
// too complex to inline itself.
//
//go:noinline
internal static partial (ж<Block>, error) newOutlined(ж<Block> Ꮡb, slice<byte> key) {
    var exprᴛ1 = len(key);
    if (exprᴛ1 == aes128KeySize || exprᴛ1 == aes192KeySize || exprᴛ1 == aes256KeySize) {
    }
    else { /* default: */
        return (default!, ((KeySizeError)len(key)));
    }

    return (newBlock(Ꮡb, key), default!);
}

internal static void newBlockExpanded(ж<blockExpanded> Ꮡc, slice<byte> key) {
    ref var c = ref Ꮡc.DerefOrNull();

    var exprᴛ1 = len(key);
    if (exprᴛ1 == aes128KeySize) {
        c.rounds = aes128Rounds;
    }
    else if (exprᴛ1 == aes192KeySize) {
        c.rounds = aes192Rounds;
    }
    else if (exprᴛ1 == aes256KeySize) {
        c.rounds = aes256Rounds;
    }

    expandKeyGeneric(Ꮡc, key);
}

[GoRecv] public static nint BlockSize(this ref Block c) {
    return ΔBlockSize;
}

public static void Encrypt(this ж<Block> Ꮡc, slice<byte> dst, slice<byte> src) {
    // AES-ECB is not approved in FIPS 140-3 mode.
    fips140.RecordNonApproved();
    if (len(src) < ΔBlockSize) {
        throw panic("crypto/aes: input not full block");
    }
    if (len(dst) < ΔBlockSize) {
        throw panic("crypto/aes: output not full block");
    }
    if (alias.InexactOverlap(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)])) {
        throw panic("crypto/aes: invalid buffer overlap");
    }
    encryptBlock(ref (Ꮡc).DerefOrNull(), dst, src);
}

public static void Decrypt(this ж<Block> Ꮡc, slice<byte> dst, slice<byte> src) {
    // AES-ECB is not approved in FIPS 140-3 mode.
    fips140.RecordNonApproved();
    if (len(src) < ΔBlockSize) {
        throw panic("crypto/aes: input not full block");
    }
    if (len(dst) < ΔBlockSize) {
        throw panic("crypto/aes: output not full block");
    }
    if (alias.InexactOverlap(dst[..(int)(ΔBlockSize)], src[..(int)(ΔBlockSize)])) {
        throw panic("crypto/aes: invalid buffer overlap");
    }
    decryptBlock(ref (Ꮡc).DerefOrNull(), dst, src);
}

// EncryptBlockInternal applies the AES encryption function to one block.
//
// It is an internal function meant only for the gcm package.
public static void EncryptBlockInternal(ж<Block> Ꮡc, slice<byte> dst, slice<byte> src) {
    encryptBlock(ref (Ꮡc).DerefOrNull(), dst, src);
}

} // end aes_package
