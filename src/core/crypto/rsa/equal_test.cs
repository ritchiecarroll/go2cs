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
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;
internal static ref readonly @string rsa1024min0ˢ => ref rsa1024min0ˢᵇ;
private static readonly @string rsa1024min0ˢᵇ = "rsa1024min=0"u8;

public static void TestEqual(ж<testing.T> t) {
    t.Setenv(godebugˢ, rsa1024min0ˢ);

    var @private = test512Key;
    var @public = @private.ᏑPublicKey;

    if (!@public.Equal(@public.OrTypedNil())) {
        t.Errorf("public key is not equal to itself: %v", @public.OrTypedNil());
    }
    if (!@public.Equal(((crypto.Signer)new rsa_test_package.rsa_PrivateKeyжSigner(@private)).Public()._<ж<rsa.PublicKey>>().OrTypedNil())) {
        t.Errorf("private.Public() is not Equal to public: %v", @public.OrTypedNil());
    }
    if (!@private.Equal(@private.OrTypedNil())) {
        t.Errorf("private key is not equal to itself: %v", @private.OrTypedNil());
    }

    var (enc, err) = Δx509.MarshalPKCS8PrivateKey(@private.OrTypedNil());
    if (err != nil) {
        t.Fatal(err);
    }
    (var decoded, err) = Δx509.ParsePKCS8PrivateKey(enc);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!@public.Equal(decoded._<crypto.Signer>().Public())) {
        t.Errorf("public key is not equal to itself after decoding: %v", @public.OrTypedNil());
    }
    if (!@private.Equal(decoded)) {
        t.Errorf("private key is not equal to itself after decoding: %v", @private.OrTypedNil());
    }

    var other = test512KeyTwo;
    if (@public.Equal(other.Public())) {
        t.Errorf("different public keys are Equal");
    }
    if (@private.Equal(other.OrTypedNil())) {
        t.Errorf("different private keys are Equal");
    }

    ref var noPrecomp = ref heap<rsa.PrivateKey>(@private.Value, out var ᏑnoPrecomp);
    noPrecomp.Precomputed = new rsa.PrecomputedValues();
    if (!@private.Equal(ᏑnoPrecomp)) {
        t.Errorf("private key with no precomputation is not equal to itself: %v", @private.OrTypedNil());
    }
}

} // end rsa_test_package
