// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package rc4 implements RC4 encryption, as defined in Bruce Schneier's
// Applied Cryptography.
//
// RC4 is cryptographically broken and should not be used for secure
// applications.
namespace go.crypto;

using alias = go.crypto.@internal.fips140.alias_package;
using fips140only = go.crypto.@internal.fips140only_package;
using errors = errors_package;
using strconv = strconv_package;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class rc4_package {

// A Cipher is an instance of RC4 using a particular key.
[GoType] partial struct Cipher {
    internal array<uint32> s = new(256);
    internal uint8 i, j;
}

[GoType("num:nint")] partial struct KeySizeError;

public static @string Error(this KeySizeError k) {
    return "crypto/rc4: invalid key size "u8 + strconv.Itoa((nint)k);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cryptoRc4UseOfRc4IsNotˢ = "crypto/rc4: use of RC4 is not allowed in FIPS 140-only mode"u8;

// NewCipher creates and returns a new [Cipher]. The key argument should be the
// RC4 key, at least 1 byte and at most 256 bytes.
public static (ж<Cipher>, error) NewCipher(slice<byte> key) {
    if (fips140only.Enabled) {
        return (default!, errors.New(cryptoRc4UseOfRc4IsNotˢ));
    }
    nint k = len(key);
    if (k < 1 || k > 256) {
        return (default!, ((KeySizeError)k));
    }
    ref var c = ref heap(new Cipher(), out var Ꮡc);
    for (nint i = 0; i < 256; i++) {
        c.s[i] = (uint32)i;
    }
    uint8 j = 0;
    for (nint i = 0; i < 256; i++) {
        j += (uint8)((uint8)c.s[i] + key[rem(i, k)]);
        (c.s[i], c.s[j]) = (c.s[j], c.s[i]);
    }
    return (Ꮡc, default!);
}

// Reset zeros the key data and makes the [Cipher] unusable.
//
// Deprecated: Reset can't guarantee that the key will be entirely removed from
// the process's memory.
[GoRecv] public static void Reset(this ref Cipher c) {
    foreach (var (i, _) in c.s) {
        c.s[i] = 0;
    }
    (c.i, c.j) = (0, 0);
}

// XORKeyStream sets dst to the result of XORing src with the key stream.
// Dst and src must overlap entirely or not at all.
[GoRecv] public static void XORKeyStream(this ref Cipher c, slice<byte> dst, slice<byte> src) {
    if (len(src) == 0) {
        return;
    }
    if (alias.InexactOverlap(dst.slice(0, len(src)), src)) {
        throw panic("crypto/rc4: invalid buffer overlap");
    }
    var (i, j) = (c.i, c.j);
    _ = dst[len(src) - 1];
    dst = dst.slice(0, len(src)); // eliminate bounds check from loop
    foreach (var (k, v) in src) {
        i += 1;
        var x = c.s[i];
        j += (uint8)x;
        var y = c.s[j];
        (c.s[i], c.s[j]) = (y, x);
        dst[k] = (byte)(v ^ (uint8)c.s[(uint8)(x + y)]);
    }
    (c.i, c.j) = (i, j);
}

} // end rc4_package
