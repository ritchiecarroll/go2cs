// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package aes implements AES encryption (formerly Rijndael), as defined in
// U.S. Federal Information Processing Standards Publication 197.
//
// The AES operations in this package are not implemented using constant-time algorithms.
// An exception is when running on systems with enabled hardware support for AES
// that makes these operations constant-time. Examples include amd64 systems using AES-NI
// extensions and s390x systems using Message-Security-Assist extensions.
// On such systems, when the result of NewCipher is passed to cipher.NewGCM,
// the GHASH operation used by GCM is also constant-time.
namespace go.crypto;

using cipher = go.crypto.cipher_package;
using boring = go.crypto.@internal.boring_package;
using aes = go.crypto.@internal.fips140.aes_package;
using strconv = strconv_package;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;

partial class aes_package {

// The AES block size in bytes.
public static UntypedInt BlockSize => 16;

partial struct KeySizeError /*num:nint*/;

public static @string Error(this KeySizeError k) {
    return "crypto/aes: invalid key size "u8 + strconv.Itoa((nint)k);
}

// NewCipher creates and returns a new [cipher.Block].
// The key argument should be the AES key,
// either 16, 24, or 32 bytes to select
// AES-128, AES-192, or AES-256.
public static (cipher.Block, error) NewCipher(slice<byte> key) {
    nint k = len(key);
    switch (k) {
    default: {
        return (default!, ((KeySizeError)k));
    }
    case 16 or 24 or 32: {
        break;
        break;
    }}

    if (boring.Enabled) {
        return boring.NewAESCipher(key);
    }
    var (ᴛ1, ᴛ2) = aes.New(key);
    return (new aes_BlockжBlock(ᴛ1), ᴛ2);
}

} // end aes_package
