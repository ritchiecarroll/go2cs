// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package fipstest collects external tests that would ordinarily live in
// crypto/internal/fips140/... packages. That tree gets snapshot at each
// validation, while we want tests to evolve and still apply to all versions of
// the module. Also, we can't fix failing tests in a module snapshot, so we need
// to either minimize, skip, or remove them. Finally, the module needs to avoid
// importing internal packages like testenv and cryptotest to avoid locking in
// their APIs.
//
// Also, this package includes the ACVP and functional testing harnesses.
namespace go.crypto.@internal.fips140test;

using bytes = bytes_package;
using boring = go.crypto.@internal.boring_package;
using fips140 = go.crypto.@internal.fips140_package;
using aes = go.crypto.@internal.fips140.aes_package;
using gcm = go.crypto.@internal.fips140.aes.gcm_package;
using check = go.crypto.@internal.fips140.check_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using ecdh = go.crypto.@internal.fips140.ecdh_package;
using ecdsa = go.crypto.@internal.fips140.ecdsa_package;
using ed25519 = go.crypto.@internal.fips140.ed25519_package;
using hkdf = go.crypto.@internal.fips140.hkdf_package;
using hmac = go.crypto.@internal.fips140.hmac_package;
using mlkem = go.crypto.@internal.fips140.mlkem_package;
using pbkdf2 = go.crypto.@internal.fips140.pbkdf2_package;
using rsa = go.crypto.@internal.fips140.rsa_package;
using sha256 = go.crypto.@internal.fips140.sha256_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using sha512 = go.crypto.@internal.fips140.sha512_package;
using tls12 = go.crypto.@internal.fips140.tls12_package;
using tls13 = go.crypto.@internal.fips140.tls13_package;
using rand = go.crypto.rand_package;
using hex = encoding.hex_package;
using debug = go.runtime.debug_package;
using strings = strings_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using encoding;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140.aes;
using go.runtime;
using hash = hash_package;
using io = io_package;
using nistec = go.crypto.@internal.fips140.nistec_package;

partial class fipstest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fips1403ModeEnabledˢ = (@string)"FIPS 140-3 mode enabled"u8;
internal static readonly object fips1403ModeNotEnabledˢ = (@string)"FIPS 140-3 mode not enabled"u8;
internal static readonly object paaPaiDisabledˢ = (@string)"PAA/PAI disabled"u8;
internal static readonly object paaPaiEnabledˢ = (@string)"PAA/PAI enabled"u8;
internal static readonly object fips1403IntegritySelfˢ = (@string)"FIPS 140-3 integrity self-check succeeded"u8;
internal static readonly object fips1403IntegritySelfˢ2 = (@string)"FIPS 140-3 integrity self-check not succeeded"u8;

internal static void moduleStatus(ж<testing.T> t) {
    if (fips140.Enabled) {
        t.Log(fips1403ModeEnabledˢ);
    } else {
        t.Log(fips1403ModeNotEnabledˢ);
    }

    t.Logf("Module name: %s", fips140.Name());
    t.Logf("Module version: %s", fips140.Version());

    if (noPAAPAI) {
        t.Log(paaPaiDisabledˢ);
    } else {
        t.Log(paaPaiEnabledˢ);
    }

    if (check.Verified) {
        t.Log(fips1403IntegritySelfˢ);
    } else {
        t.Log(fips1403IntegritySelfˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noBuildInfoˢ = (@string)"no build info"u8;

public static void TestVersion(ж<testing.T> t) {
    var (bi, ok) = debug.ReadBuildInfo();
    if (!ok) {
        t.Skip(noBuildInfoˢ);
    }
    foreach (var (_, setting) in bi.Settings) {
        if (setting.Key != "GOFIPS140"u8) {
            continue;
        }
        @string exp = setting.Value;
        // Remove the -hash suffix, if any.
        // The version from fips140.Version omits it.
        (exp, _, _) = strings.Cut(exp, "-");
        {
            @string v = fips140.Version(); if (v != exp) {
                t.Errorf("Version is %q, expected %q", v, exp);
            }
        }
        return;
    }
    // Without GOFIPS140, the Version should be "latest".
    {
        @string v = fips140.Version(); if (v != "latest"u8) {
            t.Errorf("Version is %q, expected latest", v);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object goBoringCryptoShimsˢ = (@string)"Go+BoringCrypto shims prevent the service indicator from being set"u8;
internal static ref readonly @string aesCtrˢ => ref aesCtrˢᵇ;
private static readonly @string aesCtrˢᵇ = "AES-CTR"u8;
internal static ref readonly @string aesCbcˢ => ref aesCbcˢᵇ;
private static readonly @string aesCbcˢᵇ = "AES-CBC"u8;
internal static ref readonly @string aesGcmˢ => ref aesGcmˢᵇ;
private static readonly @string aesGcmˢᵇ = "AES-GCM"u8;
internal static ref readonly @string counterKdfˢ => ref counterKdfˢᵇ;
private static readonly @string counterKdfˢᵇ = "Counter KDF"u8;
internal static ref readonly @string kasEccSscˢ => ref kasEccSscˢᵇ;
private static readonly @string kasEccSscˢᵇ = "KAS-ECC-SSC ephemeralUnified"u8;
internal static ref readonly @string ecdsaKeyGenSigGenSigVerˢ => ref ecdsaKeyGenSigGenSigVerˢᵇ;
private static readonly @string ecdsaKeyGenSigGenSigVerˢᵇ = "ECDSA KeyGen, SigGen, SigVer"u8;
internal static ref readonly @string eddsaKeyGenSigGenSigVerˢ => ref eddsaKeyGenSigGenSigVerˢᵇ;
private static readonly @string eddsaKeyGenSigGenSigVerˢᵇ = "EDDSA KeyGen, SigGen, SigVer"u8;
internal static ref readonly @string ctrDRBGˢ => ref ctrDRBGˢᵇ;
private static readonly @string ctrDRBGˢᵇ = "ctrDRBG"u8;
internal static ref readonly @string hmacˢ => ref hmacˢᵇ;
private static readonly @string hmacˢᵇ = "HMAC"u8;
internal static ref readonly @string mlKemKeyGenEncapDecapˢ => ref mlKemKeyGenEncapDecapˢᵇ;
private static readonly @string mlKemKeyGenEncapDecapˢᵇ = "ML-KEM KeyGen, Encap, Decap"u8;
internal static ref readonly @string rsaKeyGenˢ => ref rsaKeyGenˢᵇ;
private static readonly @string rsaKeyGenˢᵇ = "RSA KeyGen"u8;
internal static readonly object rsaKeyGeneratedˢ = (@string)"RSA key generated"u8;
internal static ref readonly @string rsaSigGenSigVerPkcs15ˢ => ref rsaSigGenSigVerPkcs15ˢᵇ;
private static readonly @string rsaSigGenSigVerPkcs15ˢᵇ = "RSA SigGen, SigVer PKCS 1.5"u8;
internal static ref readonly @string sha256ˢ => ref sha256ˢᵇ;
private static readonly @string sha256ˢᵇ = "SHA-256"u8;
internal static ref readonly @string rsaSigGenSigVerPssˢ => ref rsaSigGenSigVerPssˢᵇ;
private static readonly @string rsaSigGenSigVerPssˢᵇ = "RSA SigGen, SigVer PSS"u8;
internal static ref readonly @string rsaKeyGenWSmallKeyNotˢ => ref rsaKeyGenWSmallKeyNotˢᵇ;
private static readonly @string rsaKeyGenWSmallKeyNotˢᵇ = "RSA KeyGen w/ small key [NOT APPROVED]"u8;
internal static ref readonly @string ktsIfcOaepˢ => ref ktsIfcOaepˢᵇ;
private static readonly @string ktsIfcOaepˢᵇ = "KTS IFC OAEP"u8;
internal static ref readonly @string sha2224ˢ => ref sha2224ˢᵇ;
private static readonly @string sha2224ˢᵇ = "SHA2-224"u8;
internal static ref readonly @string sha2512ˢ => ref sha2512ˢᵇ;
private static readonly @string sha2512ˢᵇ = "SHA2-512"u8;
internal static ref readonly @string sha2512224ˢ => ref sha2512224ˢᵇ;
private static readonly @string sha2512224ˢᵇ = "SHA2-512/224"u8;
internal static ref readonly @string sha2512256ˢ => ref sha2512256ˢᵇ;
private static readonly @string sha2512256ˢᵇ = "SHA2-512/256"u8;
internal static ref readonly @string sha3224ˢ => ref sha3224ˢᵇ;
private static readonly @string sha3224ˢᵇ = "SHA3-224"u8;
internal static ref readonly @string sha3256ˢ => ref sha3256ˢᵇ;
private static readonly @string sha3256ˢᵇ = "SHA3-256"u8;
internal static ref readonly @string sha3384ˢ => ref sha3384ˢᵇ;
private static readonly @string sha3384ˢᵇ = "SHA3-384"u8;
internal static ref readonly @string sha3512ˢ => ref sha3512ˢᵇ;
private static readonly @string sha3512ˢᵇ = "SHA3-512"u8;
internal static ref readonly @string shake128ˢ => ref shake128ˢᵇ;
private static readonly @string shake128ˢᵇ = "SHAKE-128"u8;
internal static ref readonly @string shake256ˢ => ref shake256ˢᵇ;
private static readonly @string shake256ˢᵇ = "SHAKE-256"u8;
internal static ref readonly @string cSHAKE128ˢ => ref cSHAKE128ˢᵇ;
private static readonly @string cSHAKE128ˢᵇ = "cSHAKE-128"u8;
internal static ref readonly @string cSHAKE256ˢ => ref cSHAKE256ˢᵇ;
private static readonly @string cSHAKE256ˢᵇ = "cSHAKE-256"u8;
internal static ref readonly @string kdaHkdfˢ => ref kdaHkdfˢᵇ;
private static readonly @string kdaHkdfˢᵇ = "KDA HKDF"u8;
internal static ref readonly @string infoˢ => ref infoˢᵇ;
private static readonly @string infoˢᵇ = "info"u8;
internal static ref readonly @string kdaOneStepNoCounterˢ => ref kdaOneStepNoCounterˢᵇ;
private static readonly @string kdaOneStepNoCounterˢᵇ = "KDA OneStepNoCounter"u8;
internal static ref readonly @string feedbackKdfˢ => ref feedbackKdfˢᵇ;
private static readonly @string feedbackKdfˢᵇ = "Feedback KDF"u8;
internal static ref readonly @string pbkdfˢ => ref pbkdfˢᵇ;
private static readonly @string pbkdfˢᵇ = "PBKDF"u8;
internal static ref readonly @string passwordˢ => ref passwordˢᵇ;
private static readonly @string passwordˢᵇ = "password"u8;
internal static ref readonly @string kdfTlsV12Cvlˢ => ref kdfTlsV12Cvlˢᵇ;
private static readonly @string kdfTlsV12Cvlˢᵇ = "KDF TLS v1.2 CVL"u8;
internal static ref readonly @string kdfTlsV13Cvlˢ => ref kdfTlsV13Cvlˢᵇ;
private static readonly @string kdfTlsV13Cvlˢᵇ = "KDF TLS v1.3 CVL"u8;

public static void TestFIPS140(ж<testing.T> t) {
    moduleStatus(t);
    if (boring.Enabled) {
        t.Skip(goBoringCryptoShimsˢ);
    }

    var aesKey = new slice<byte>(128 / 8);
    var aesIV = new slice<byte>(aes.ΔBlockSize);
    var plaintext = slice<byte>("Go Cryptographic Module TestFIPS140 plaintext..."u8);
    var plaintextSHA256 = decodeHex(t, "06b2614e2ef315832b23f5d0ff70294d8ddd3889527dfbe75707fe41da929325");
    var (aesBlock, err) = aes.New(aesKey);
    fatalIfErr(t, err);

    testFIPS140v126(t, plaintext);

    var aesBlockʗ1 = aesBlock;
    var aesIVʗ1 = aesIV;
    var plaintextʗ1 = plaintext;
    t.Run(aesCtrˢ, (ж<testing.T> tΔ1) => {
        ensureServiceIndicator(tΔ1);
        var ctr = aes.NewCTR(aesBlockʗ1, aesIVʗ1);
        var ciphertext = new slice<byte>(len(plaintextʗ1));
        ctr.XORKeyStream(ciphertext, plaintextʗ1);
        tΔ1.Logf("AES-CTR ciphertext: %x", ciphertext);
        var @out = new slice<byte>(len(plaintextʗ1));
        ctr = aes.NewCTR(aesBlockʗ1, aesIVʗ1);
        ctr.XORKeyStream(@out, ciphertext);
        tΔ1.Logf("AES-CTR decrypted plaintext: %s", @out);
        if (!bytes.Equal(plaintextʗ1, @out)) {
            tΔ1.Errorf("AES-CTR round trip failed");
        }
    });

    var aesBlockʗ2 = aesBlock;
    var aesIVʗ2 = aesIV;
    var plaintextʗ2 = plaintext;
    t.Run(aesCbcˢ, (ж<testing.T> tΔ2) => {
        ensureServiceIndicator(tΔ2);
        var cbcEnc = aes.NewCBCEncrypter(aesBlockʗ2, new array<byte>(aesIVʗ2, 16));
        var ciphertext = new slice<byte>(len(plaintextʗ2));
        cbcEnc.CryptBlocks(ciphertext, plaintextʗ2);
        tΔ2.Logf("AES-CBC ciphertext: %x", ciphertext);
        var cbcDec = aes.NewCBCDecrypter(aesBlockʗ2, new array<byte>(aesIVʗ2, 16));
        var @out = new slice<byte>(len(plaintextʗ2));
        cbcDec.CryptBlocks(@out, ciphertext);
        tΔ2.Logf("AES-CBC decrypted plaintext: %s", @out);
        if (!bytes.Equal(plaintextʗ2, @out)) {
            tΔ2.Errorf("AES-CBC round trip failed");
        }
    });

    var aesBlockʗ3 = aesBlock;
    var plaintextʗ3 = plaintext;
    t.Run(aesGcmˢ, (ж<testing.T> tΔ3) => {
        ensureServiceIndicator(tΔ3);
        var (g, errΔ1) = gcm.New(aesBlockʗ3, 12, 16);
        fatalIfErr(tΔ3, errΔ1);
        var nonce = new slice<byte>(12);
        var ciphertext = new slice<byte>(len(plaintextʗ3) + g.Overhead());
        gcm.SealWithRandomNonce(g, nonce, ciphertext, plaintextʗ3, default);
        tΔ3.Logf("AES-GCM ciphertext: %x", ciphertext);
        (var @out, errΔ1) = g.Open(default, nonce, ciphertext, default);
        fatalIfErr(tΔ3, errΔ1);
        tΔ3.Logf("AES-GCM decrypted plaintext: %s", @out);
        if (!bytes.Equal(plaintextʗ3, @out)) {
            tΔ3.Errorf("AES-GCM round trip failed");
        }
    });

    var aesBlockʗ4 = aesBlock;
    t.Run(counterKdfˢ, (ж<testing.T> tΔ4) => {
        ensureServiceIndicator(tΔ4);
        var k = gcm.NewCounterKDF(aesBlockʗ4);
        var context = new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}.array();
        var key = k.DeriveKey(0x01, context);
        tΔ4.Logf("Counter KDF key: %x", key);
    });

    t.Run(kasEccSscˢ, (ж<testing.T> tΔ5) => {
        ensureServiceIndicator(tΔ5);
        var (k, errΔ2) = ecdh.GenerateKey<ecdh.P256PointжPoint>(ecdh.P256(), rand.Reader);
        fatalIfErr(tΔ5, errΔ2);
        var pk = k.PublicKey();
        (var shared, errΔ2) = ecdh.ECDH<ecdh.P256PointжPoint>(ecdh.P256(), k, pk);
        fatalIfErr(tΔ5, errΔ2);
        tΔ5.Logf("KAS-ECC-SSC shared secret: %x", shared);
    });

    var plaintextSHA256ʗ1 = plaintextSHA256;
    t.Run(ecdsaKeyGenSigGenSigVerˢ, (ж<testing.T> tΔ6) => {
        ensureServiceIndicator(tΔ6);
        var (k, errΔ3) = ecdsa.GenerateKey<ecdsa.P256PointжPoint>(ecdsa.P256(), rand.Reader);
        fatalIfErr(tΔ6, errΔ3);

        (var sig, errΔ3) = ecdsa.Sign<ecdsa.P256PointжPoint, hash.Hash>(ecdsa.P256(), widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ1 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ1)), k, rand.Reader, plaintextSHA256ʗ1);
        fatalIfErr(tΔ6, errΔ3);
        tΔ6.Logf("ECDSA signature: %x", sig.OrTypedNil());
        errΔ3 = ecdsa.Verify<ecdsa.P256PointжPoint>(ecdsa.P256(), k.PublicKey(), plaintextSHA256ʗ1, sig);
        if (errΔ3 != nil) {
            tΔ6.Errorf("ECDSA signature verification failed");
        }

        (sig, errΔ3) = ecdsa.SignDeterministic<ecdsa.P256PointжPoint, hash.Hash>(ecdsa.P256(), widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ1 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ1)), k, plaintextSHA256ʗ1);
        fatalIfErr(tΔ6, errΔ3);
        tΔ6.Logf("ECDSA deterministic signature: %x", sig.OrTypedNil());
        errΔ3 = ecdsa.Verify<ecdsa.P256PointжPoint>(ecdsa.P256(), k.PublicKey(), plaintextSHA256ʗ1, sig);
        if (errΔ3 != nil) {
            tΔ6.Errorf("ECDSA deterministic signature verification failed");
        }
    });

    var plaintextʗ4 = plaintext;
    t.Run(eddsaKeyGenSigGenSigVerˢ, (ж<testing.T> tΔ7) => {
        ensureServiceIndicator(tΔ7);
        var (k, errΔ4) = ed25519.GenerateKey();
        fatalIfErr(tΔ7, errΔ4);

        var sig = ed25519.Sign(k, plaintextʗ4);
        tΔ7.Logf("EDDSA signature: %x", sig);

        (var pk, errΔ4) = ed25519.NewPublicKey(k.PublicKey());
        fatalIfErr(tΔ7, errΔ4);
        errΔ4 = ed25519.Verify(pk, plaintextʗ4, sig);
        if (errΔ4 != nil) {
            tΔ7.Errorf("EDDSA signature verification failed");
        }
    });

    var plaintextʗ5 = plaintext;
    t.Run(ctrDRBGˢ, (ж<testing.T> tΔ8) => {
        ensureServiceIndicator(tΔ8);
        var r = drbg.NewCounter(Ꮡ(array<byte>.Alias(plaintextʗ5, 48)));
        r.Reseed(Ꮡ(array<byte>.Alias(plaintextʗ5, 48)), Ꮡ(array<byte>.Alias(plaintextʗ5, 48)));
        var @out = new slice<byte>(16);
        r.Generate(@out, Ꮡ(array<byte>.Alias(plaintextʗ5, 48)));
        tΔ8.Logf("ctrDRBG output: %x", @out);
    });

    var plaintextʗ6 = plaintext;
    t.Run(hmacˢ, (ж<testing.T> tΔ9) => {
        ensureServiceIndicator(tΔ9);
        var h = hmac.New<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextʗ6);
        h.Write(plaintextʗ6);
        var @out = h.Sum(default);
        tΔ9.Logf("HMAC output: %x", @out);
    });

    t.Run(mlKemKeyGenEncapDecapˢ, (ж<testing.T> tΔ10) => {
        ensureServiceIndicator(tΔ10);
        var (k, errΔ5) = mlkem.GenerateKey768();
        fatalIfErr(tΔ10, errΔ5);

        var (ss, c) = k.EncapsulationKey().Encapsulate();
        tΔ10.Logf("ML-KEM encapsulation: %x", c);

        (var ss2, errΔ5) = k.Decapsulate(c);
        fatalIfErr(tΔ10, errΔ5);
        tΔ10.Logf("ML-KEM shared secret: %x", ss);
        if (!bytes.Equal(ss, ss2)) {
            tΔ10.Errorf("ML-KEM round trip failed");
        }
    });

    ref var rsaKey = ref heap<ж<rsa.PrivateKey>>(out var ᏑrsaKey);
    t.Run(rsaKeyGenˢ, (ж<testing.T> tΔ11) => {
        ensureServiceIndicator(tΔ11);
        error errΔ6 = default;
        (ᏑrsaKey.ValueSlot, errΔ6) = rsa.GenerateKey(rand.Reader, 2048);
        fatalIfErr(tΔ11, errΔ6);
        tΔ11.Log(rsaKeyGeneratedˢ);
    });

    var plaintextSHA256ʗ2 = plaintextSHA256;
    t.Run(rsaSigGenSigVerPkcs15ˢ, (ж<testing.T> tΔ12) => {
        ensureServiceIndicator(tΔ12);
        var (sig, errΔ7) = rsa.SignPKCS1v15(ᏑrsaKey.ValueSlot, sha256ˢ, plaintextSHA256ʗ2);
        fatalIfErr(tΔ12, errΔ7);
        tΔ12.Logf("RSA PKCS1v15 signature: %x", sig);

        errΔ7 = rsa.VerifyPKCS1v15(ᏑrsaKey.ValueSlot.PublicKey(), sha256ˢ, plaintextSHA256ʗ2, sig);
        fatalIfErr(tΔ12, errΔ7);
    });

    var plaintextSHA256ʗ3 = plaintextSHA256;
    t.Run(rsaSigGenSigVerPssˢ, (ж<testing.T> tΔ13) => {
        ensureServiceIndicator(tΔ13);
        var (sig, errΔ8) = rsa.SignPSS(rand.Reader, ᏑrsaKey.ValueSlot, new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), plaintextSHA256ʗ3, 16);
        fatalIfErr(tΔ13, errΔ8);
        tΔ13.Logf("RSA PSS signature: %x", sig);

        errΔ8 = rsa.VerifyPSS(ᏑrsaKey.ValueSlot.PublicKey(), new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), plaintextSHA256ʗ3, sig);
        fatalIfErr(tΔ13, errΔ8);
    });

    t.Run(rsaKeyGenWSmallKeyNotˢ, (ж<testing.T> tΔ14) => {
        ensureServiceIndicatorFalse(tΔ14);
        var (_, errΔ9) = rsa.GenerateKey(rand.Reader, 512);
        fatalIfErr(tΔ14, errΔ9);
        tΔ14.Log(rsaKeyGeneratedˢ);
    });

    var plaintextSHA256ʗ4 = plaintextSHA256;
    t.Run(ktsIfcOaepˢ, (ж<testing.T> tΔ15) => {
        ensureServiceIndicator(tΔ15);
        var (c, errΔ10) = rsa.EncryptOAEP(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), rand.Reader, ᏑrsaKey.ValueSlot.PublicKey(), plaintextSHA256ʗ4, default);
        fatalIfErr(tΔ15, errΔ10);
        tΔ15.Logf("RSA OAEP ciphertext: %x", c);

        (var @out, errΔ10) = rsa.DecryptOAEP(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), ᏑrsaKey.ValueSlot, c, default);
        fatalIfErr(tΔ15, errΔ10);
        tΔ15.Logf("RSA OAEP decrypted plaintext: %x", @out);
        if (!bytes.Equal(plaintextSHA256ʗ4, @out)) {
            tΔ15.Errorf("RSA OAEP round trip failed");
        }
    });

    var plaintextʗ7 = plaintext;
    t.Run(sha2224ˢ, (ж<testing.T> tΔ16) => {
        ensureServiceIndicator(tΔ16);
        var h = sha256.New224();
        h.Write(plaintextʗ7);
        var @out = h.Sum(default);
        tΔ16.Logf("SHA2-224 output: %x", @out);
    });

    var plaintextʗ8 = plaintext;
    t.Run(sha2256ˢ, (ж<testing.T> tΔ17) => {
        ensureServiceIndicator(tΔ17);
        var h = sha256.New();
        h.Write(plaintextʗ8);
        var @out = h.Sum(default);
        tΔ17.Logf("SHA2-256 output: %x", @out);
    });

    var plaintextʗ9 = plaintext;
    t.Run(sha2384ˢ, (ж<testing.T> tΔ18) => {
        ensureServiceIndicator(tΔ18);
        var h = sha512.New384();
        h.Write(plaintextʗ9);
        var @out = h.Sum(default);
        tΔ18.Logf("SHA2-384 output: %x", @out);
    });

    var plaintextʗ10 = plaintext;
    t.Run(sha2512ˢ, (ж<testing.T> tΔ19) => {
        ensureServiceIndicator(tΔ19);
        var h = sha512.New();
        h.Write(plaintextʗ10);
        var @out = h.Sum(default);
        tΔ19.Logf("SHA2-512 output: %x", @out);
    });

    var plaintextʗ11 = plaintext;
    t.Run(sha2512224ˢ, (ж<testing.T> tΔ20) => {
        ensureServiceIndicator(tΔ20);
        var h = sha512.New512_224();
        h.Write(plaintextʗ11);
        var @out = h.Sum(default);
        tΔ20.Logf("SHA2-512/224 output: %x", @out);
    });

    var plaintextʗ12 = plaintext;
    t.Run(sha2512256ˢ, (ж<testing.T> tΔ21) => {
        ensureServiceIndicator(tΔ21);
        var h = sha512.New512_256();
        h.Write(plaintextʗ12);
        var @out = h.Sum(default);
        tΔ21.Logf("SHA2-512/256 output: %x", @out);
    });

    var plaintextʗ13 = plaintext;
    t.Run(sha3224ˢ, (ж<testing.T> tΔ22) => {
        ensureServiceIndicator(tΔ22);
        var h = sha3.New224();
        h.Write(plaintextʗ13);
        var @out = h.Sum(default);
        tΔ22.Logf("SHA3-224 output: %x", @out);
    });

    var plaintextʗ14 = plaintext;
    t.Run(sha3256ˢ, (ж<testing.T> tΔ23) => {
        ensureServiceIndicator(tΔ23);
        var h = sha3.New256();
        h.Write(plaintextʗ14);
        var @out = h.Sum(default);
        tΔ23.Logf("SHA3-256 output: %x", @out);
    });

    var plaintextʗ15 = plaintext;
    t.Run(sha3384ˢ, (ж<testing.T> tΔ24) => {
        ensureServiceIndicator(tΔ24);
        var h = sha3.New384();
        h.Write(plaintextʗ15);
        var @out = h.Sum(default);
        tΔ24.Logf("SHA3-384 output: %x", @out);
    });

    var plaintextʗ16 = plaintext;
    t.Run(sha3512ˢ, (ж<testing.T> tΔ25) => {
        ensureServiceIndicator(tΔ25);
        var h = sha3.New512();
        h.Write(plaintextʗ16);
        var @out = h.Sum(default);
        tΔ25.Logf("SHA3-512 output: %x", @out);
    });

    var plaintextʗ17 = plaintext;
    t.Run(shake128ˢ, (ж<testing.T> tΔ26) => {
        ensureServiceIndicator(tΔ26);
        var h = sha3.NewShake128();
        h.Write(plaintextʗ17);
        var @out = new slice<byte>(16);
        h.Read(@out);
        tΔ26.Logf("SHAKE-128 output: %x", @out);
    });

    var plaintextʗ18 = plaintext;
    t.Run(shake256ˢ, (ж<testing.T> tΔ27) => {
        ensureServiceIndicator(tΔ27);
        var h = sha3.NewShake256();
        h.Write(plaintextʗ18);
        var @out = new slice<byte>(16);
        h.Read(@out);
        tΔ27.Logf("SHAKE-256 output: %x", @out);
    });

    var plaintextʗ19 = plaintext;
    t.Run(cSHAKE128ˢ, (ж<testing.T> tΔ28) => {
        ensureServiceIndicator(tΔ28);
        var h = sha3.NewCShake128(default, slice<byte>("test"u8));
        h.Write(plaintextʗ19);
        var @out = new slice<byte>(16);
        h.Read(@out);
        tΔ28.Logf("cSHAKE-128 output: %x", @out);
    });

    var plaintextʗ20 = plaintext;
    t.Run(cSHAKE256ˢ, (ж<testing.T> tΔ29) => {
        ensureServiceIndicator(tΔ29);
        var h = sha3.NewCShake256(default, slice<byte>("test"u8));
        h.Write(plaintextʗ20);
        var @out = new slice<byte>(16);
        h.Read(@out);
        tΔ29.Logf("cSHAKE-256 output: %x", @out);
    });

    var plaintextSHA256ʗ5 = plaintextSHA256;
    t.Run(kdaHkdfˢ, (ж<testing.T> tΔ30) => {
        ensureServiceIndicator(tΔ30);
        var key = hkdf.Key<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextSHA256ʗ5, slice<byte>("salt"u8), infoˢ, 16);
        tΔ30.Logf("HKDF key: %x", key);
    });

    var plaintextSHA256ʗ6 = plaintextSHA256;
    t.Run(kdaOneStepNoCounterˢ, (ж<testing.T> tΔ31) => {
        ensureServiceIndicator(tΔ31);
        var key = hkdf.Extract<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextSHA256ʗ6, slice<byte>("salt"u8));
        tΔ31.Logf("KDA OneStepNoCounter key: %x", key);
    });

    var plaintextSHA256ʗ7 = plaintextSHA256;
    t.Run(feedbackKdfˢ, (ж<testing.T> tΔ32) => {
        ensureServiceIndicator(tΔ32);
        var key = hkdf.Expand<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextSHA256ʗ7, infoˢ, 16);
        tΔ32.Logf("Feedback KDF key: %x", key);
    });

    var plaintextSHA256ʗ8 = plaintextSHA256;
    t.Run(pbkdfˢ, (ж<testing.T> tΔ33) => {
        ensureServiceIndicator(tΔ33);
        var (key, errΔ11) = pbkdf2.Key<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), passwordˢ, plaintextSHA256ʗ8, 2, 16);
        fatalIfErr(tΔ33, errΔ11);
        tΔ33.Logf("PBKDF key: %x", key);
    });

    var plaintextSHA256ʗ9 = plaintextSHA256;
    t.Run(kdfTlsV12Cvlˢ, (ж<testing.T> tΔ34) => {
        ensureServiceIndicator(tΔ34);
        var key = tls12.MasterSecret<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextSHA256ʗ9, slice<byte>("test"u8));
        tΔ34.Logf("TLS v1.2 CVL Master Secret: %x", key);
    });

    var plaintextSHA256ʗ10 = plaintextSHA256;
    t.Run(kdfTlsV13Cvlˢ, (ж<testing.T> tΔ35) => {
        ensureServiceIndicator(tΔ35);
        var es = tls13.NewEarlySecret<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextSHA256ʗ10);
        var hs = es.HandshakeSecret(plaintextSHA256ʗ10);
        var ms = hs.MasterSecret();
        var client = ms.ClientApplicationTrafficSecret(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()));
        var server = ms.ServerApplicationTrafficSecret(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()));
        tΔ35.Logf("TLS v1.3 CVL Application Traffic Secrets: client %x, server %x", client, server);
    });
}

internal static void ensureServiceIndicator(ж<testing.T> t) {
    fips140.ResetServiceIndicator();
    t.Cleanup(() => {
        if (fips140.ServiceIndicator()) {
            t.Logf("Service indicator is set");
        } else {
            t.Errorf("Service indicator is not set");
        }
    });
}

internal static void ensureServiceIndicatorFalse(ж<testing.T> t) {
    fips140.ResetServiceIndicator();
    t.Cleanup(() => {
        if (!fips140.ServiceIndicator()) {
            t.Logf("Service indicator is not set");
        } else {
            t.Errorf("Service indicator is set");
        }
    });
}

internal static partial void fatalIfErr(ж<testing.T> t, error err) {
    t.Helper();
    if (err != nil) {
        t.Fatal(err);
    }
}

internal static slice<byte> decodeHex(ж<testing.T> t, @string s) {
    t.Helper();
    s = strings.ReplaceAll(s, " ", ""u8);
    var (b, err) = hex.DecodeString(s);
    if (err != nil) {
        t.Fatal(err);
    }
    return b;
}

} // end fipstest_internal_test_package
