// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using crypto = crypto_package;
using rsa = go.crypto.rsa_package;
using Δx509 = go.crypto.x509_package;
using testing = testing_package;
using go.crypto;
using static go.crypto.rsa_internal_test_package;

partial class rsa_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string godebugˢ = "GODEBUG"u8;
internal static readonly @string rsa1024min0ˢ = "rsa1024min=0"u8;

public static void TestEqual(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);

    var @private = test512Key;
    var @public = @private.of(rsa.PrivateKey.ᏑPublicKey);

    if (!@public.Equal(@public.OrTypedNil())) {
        Ꮡt.Errorf("public key is not equal to itself: %v"u8, @public.OrTypedNil());
    }
    if (!@public.Equal(((crypto.Signer)new rsa_test_package.rsa_PrivateKeyжSigner(@private)).Public()._<ж<rsa.PublicKey>>().OrTypedNil())) {
        Ꮡt.Errorf("private.Public() is not Equal to public: %q"u8, @public.OrTypedNil());
    }
    if (!@private.Equal(@private.OrTypedNil())) {
        Ꮡt.Errorf("private key is not equal to itself: %v"u8, @private.OrTypedNil());
    }

    var (enc, err) = Δx509.MarshalPKCS8PrivateKey(@private.OrTypedNil());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    (var decoded, err) = Δx509.ParsePKCS8PrivateKey(enc);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!@public.Equal(decoded._<crypto.Signer>().Public())) {
        Ꮡt.Errorf("public key is not equal to itself after decoding: %v"u8, @public.OrTypedNil());
    }
    if (!@private.Equal(decoded)) {
        Ꮡt.Errorf("private key is not equal to itself after decoding: %v"u8, @private.OrTypedNil());
    }

    var other = test512KeyTwo;
    if (@public.Equal(other.Public())) {
        Ꮡt.Errorf("different public keys are Equal"u8);
    }
    if (@private.Equal(other.OrTypedNil())) {
        Ꮡt.Errorf("different private keys are Equal"u8);
    }
}

} // end rsa_test_package
