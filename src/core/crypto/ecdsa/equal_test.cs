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

internal static void testEqual(ж<testing.T> Ꮡt, elliptic.Curve c) {
    var (@private, _) = ecdsa.GenerateKey(c, rand.Reader);
    var @public = @private.of(ecdsa.PrivateKey.ᏑPublicKey);

    if (!@public.Equal(@public.OrTypedNil())) {
        Ꮡt.Errorf("public key is not equal to itself: %v"u8, @public.OrTypedNil());
    }
    if (!@public.Equal(((crypto.Signer)new ecdsa_test_package.ecdsa_PrivateKeyжSigner(@private)).Public()._<ж<ecdsa.PublicKey>>().OrTypedNil())) {
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

    var (other, _) = ecdsa.GenerateKey(c, rand.Reader);
    if (@public.Equal(other.Public())) {
        Ꮡt.Errorf("different public keys are Equal"u8);
    }
    if (@private.Equal(other.OrTypedNil())) {
        Ꮡt.Errorf("different private keys are Equal"u8);
    }

    // Ensure that keys with the same coordinates but on different curves
    // aren't considered Equal.
    var differentCurve = Ꮡ(new ecdsa.PublicKey(nil));
    differentCurve.Value = @public.Value; // make a copy of the public key
    if (AreEqual((~differentCurve).Curve, elliptic.P256())){
        differentCurve.Value.Curve = elliptic.P224();
    } else {
        differentCurve.Value.Curve = elliptic.P256();
    }
    if (@public.Equal(differentCurve.OrTypedNil())) {
        Ꮡt.Errorf("public keys with different curves are Equal"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string p224ˢ = "P224"u8;
internal static readonly @string p256ˢ = "P256"u8;
internal static readonly @string p384ˢ = "P384"u8;
internal static readonly @string p521ˢ = "P521"u8;

public static void TestEqual(ж<testing.T> Ꮡt) {
    Ꮡt.Run(p224ˢ, (ж<testing.T> tΔ1) => {
        testEqual(tΔ1, elliptic.P224());
    });
    if (testing.Short()) {
        return;
    }
    Ꮡt.Run(p256ˢ, (ж<testing.T> tΔ2) => {
        testEqual(tΔ2, elliptic.P256());
    });
    Ꮡt.Run(p384ˢ, (ж<testing.T> tΔ3) => {
        testEqual(tΔ3, elliptic.P384());
    });
    Ꮡt.Run(p521ˢ, (ж<testing.T> tΔ4) => {
        testEqual(tΔ4, elliptic.P521());
    });
}

} // end ecdsa_test_package
