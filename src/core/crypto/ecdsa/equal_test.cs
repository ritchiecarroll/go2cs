// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using crypto = crypto_package;
using ecdsa = go.crypto.ecdsa_package;
using elliptic = go.crypto.elliptic_package;
using rand = go.crypto.rand_package;
using Δx509 = go.crypto.x509_package;
using testing = testing_package;
using big = math.big_package;
using go.crypto;
using io = io_package;
using static go.crypto.ecdsa_internal_test_package;

partial class ecdsa_test_package {

internal static void testEqual(ж<testing.T> t, elliptic.Curve c) {
    var (@private, _) = ecdsa.GenerateKey(c, rand.Reader);
    var @public = @private.ᏑPublicKey;

    if (!@public.Equal(@public.OrTypedNil())) {
        t.Errorf("public key is not equal to itself: %v", @public.OrTypedNil());
    }
    if (!@public.Equal(((crypto.Signer)new ecdsa_test_package.ecdsa_PrivateKeyжSigner(@private)).Public()._<ж<ecdsa.PublicKey>>().OrTypedNil())) {
        t.Errorf("private.Public() is not Equal to public: %q", @public.OrTypedNil());
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

    var (other, _) = ecdsa.GenerateKey(c, rand.Reader);
    if (@public.Equal(other.Public())) {
        t.Errorf("different public keys are Equal");
    }
    if (@private.Equal(other.OrTypedNil())) {
        t.Errorf("different private keys are Equal");
    }

    // Ensure that keys with the same coordinates but on different curves
    // aren't considered Equal.
    var differentCurve = Ꮡ(new ecdsa.PublicKey());
    differentCurve.Value = @public.Value; // make a copy of the public key
    if (AreEqual(differentCurve.Curve, elliptic.P256())) {
        differentCurve.Curve = elliptic.P224();
    } else {
        differentCurve.Curve = elliptic.P256();
    }
    if (@public.Equal(differentCurve.OrTypedNil())) {
        t.Errorf("public keys with different curves are Equal");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string p224ˢ => ref p224ˢᵇ;
private static readonly @string p224ˢᵇ = "P224"u8;
internal static ref readonly @string p256ˢ => ref p256ˢᵇ;
private static readonly @string p256ˢᵇ = "P256"u8;
internal static ref readonly @string p384ˢ => ref p384ˢᵇ;
private static readonly @string p384ˢᵇ = "P384"u8;
internal static ref readonly @string p521ˢ => ref p521ˢᵇ;
private static readonly @string p521ˢᵇ = "P521"u8;

public static void TestEqual(ж<testing.T> t) {
    t.Run(p224ˢ, (ж<testing.T> tΔ1) => {
        testEqual(tΔ1, elliptic.P224());
    });
    if (testing.Short()) {
        return;
    }
    t.Run(p256ˢ, (ж<testing.T> tΔ2) => {
        testEqual(tΔ2, elliptic.P256());
    });
    t.Run(p384ˢ, (ж<testing.T> tΔ3) => {
        testEqual(tΔ3, elliptic.P384());
    });
    t.Run(p521ˢ, (ж<testing.T> tΔ4) => {
        testEqual(tΔ4, elliptic.P521());
    });
}

} // end ecdsa_test_package
