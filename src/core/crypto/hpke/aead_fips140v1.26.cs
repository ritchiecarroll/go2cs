// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !fips140v1.0
namespace go.crypto;

using cipher = go.crypto.cipher_package;
using aes = go.crypto.@internal.fips140.aes_package;
using gcm = go.crypto.@internal.fips140.aes.gcm_package;
using go.crypto;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140.aes;

partial class hpke_package {

internal static (cipher.AEAD, error) newAESGCM(slice<byte> key) {
    var (b, err) = aes.New(key);
    if (err != default!) {
        return (default!, err);
    }
    var (ᴛ1, ᴛ2) = gcm.NewGCMForHPKE(b);
    return (new gcm_GCMWithXORCounterNonceжAEAD(ᴛ1), ᴛ2);
}

} // end hpke_package
