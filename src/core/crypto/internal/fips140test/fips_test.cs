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

internal static void moduleStatus(ж<testing.T> Ꮡt) {
    if (fips140.Enabled){
        Ꮡt.Log(fips1403ModeEnabledˢ);
    } else {
        Ꮡt.Log(fips1403ModeNotEnabledˢ);
    }

    Ꮡt.Logf("Module name: %s"u8, fips140.Name());
    Ꮡt.Logf("Module version: %s"u8, fips140.Version());

    if (noPAAPAI){
        Ꮡt.Log(paaPaiDisabledˢ);
    } else {
        Ꮡt.Log(paaPaiEnabledˢ);
    }

    if (check.Verified){
        Ꮡt.Log(fips1403IntegritySelfˢ);
    } else {
        Ꮡt.Log(fips1403IntegritySelfˢ2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noBuildInfoˢ = (@string)"no build info"u8;

public static void TestVersion(ж<testing.T> Ꮡt) {
    var (bi, ok) = debug.ReadBuildInfo();
    if (!ok) {
        Ꮡt.Skip(noBuildInfoˢ);
    }
    foreach (var (_, setting) in (~bi).Settings) {
        if (setting.Key != "GOFIPS140"u8) {
            continue;
        }
        @string exp = setting.Value;
        // Remove the -hash suffix, if any.
        // The version from fips140.Version omits it.
        (exp, _, _) = strings.Cut(exp, "-"u8);
        {
            @string v = fips140.Version(); if (v != exp) {
                Ꮡt.Errorf("Version is %q, expected %q"u8, v, exp);
            }
        }
        return;
    }
    // Without GOFIPS140, the Version should be "latest".
    {
        @string v = fips140.Version(); if (v != "latest"u8) {
            Ꮡt.Errorf("Version is %q, expected latest"u8, v);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object goBoringCryptoShimsˢ = (@string)"Go+BoringCrypto shims prevent the service indicator from being set"u8;
internal static readonly @string aesCtrˢ = "AES-CTR"u8;
internal static readonly @string aesCbcˢ = "AES-CBC"u8;
internal static readonly @string aesGcmˢ = "AES-GCM"u8;
internal static readonly @string counterKdfˢ = "Counter KDF"u8;
internal static readonly @string kasEccSscˢ = "KAS-ECC-SSC ephemeralUnified"u8;
internal static readonly @string ecdsaKeyGenSigGenSigVerˢ = "ECDSA KeyGen, SigGen, SigVer"u8;
internal static readonly @string eddsaKeyGenSigGenSigVerˢ = "EDDSA KeyGen, SigGen, SigVer"u8;
internal static readonly @string ctrDRBGˢ = "ctrDRBG"u8;
internal static readonly @string hmacˢ = "HMAC"u8;
internal static readonly @string mlKemKeyGenEncapDecapˢ = "ML-KEM KeyGen, Encap, Decap"u8;
internal static readonly @string rsaKeyGenˢ = "RSA KeyGen"u8;
internal static readonly object rsaKeyGeneratedˢ = (@string)"RSA key generated"u8;
internal static readonly @string rsaSigGenSigVerPkcs15ˢ = "RSA SigGen, SigVer PKCS 1.5"u8;
internal static readonly @string sha256ˢ = "SHA-256"u8;
internal static readonly @string rsaSigGenSigVerPssˢ = "RSA SigGen, SigVer PSS"u8;
internal static readonly @string rsaKeyGenWSmallKeyNotˢ = "RSA KeyGen w/ small key [NOT APPROVED]"u8;
internal static readonly @string ktsIfcOaepˢ = "KTS IFC OAEP"u8;
internal static readonly @string sha2224ˢ = "SHA2-224"u8;
internal static readonly @string sha2512ˢ = "SHA2-512"u8;
internal static readonly @string sha2512224ˢ = "SHA2-512/224"u8;
internal static readonly @string sha2512256ˢ = "SHA2-512/256"u8;
internal static readonly @string sha3224ˢ = "SHA3-224"u8;
internal static readonly @string sha3256ˢ = "SHA3-256"u8;
internal static readonly @string sha3384ˢ = "SHA3-384"u8;
internal static readonly @string sha3512ˢ = "SHA3-512"u8;
internal static readonly @string shake128ˢ = "SHAKE-128"u8;
internal static readonly @string shake256ˢ = "SHAKE-256"u8;
internal static readonly @string cSHAKE128ˢ = "cSHAKE-128"u8;
internal static readonly @string cSHAKE256ˢ = "cSHAKE-256"u8;
internal static readonly @string kdaHkdfˢ = "KDA HKDF"u8;
internal static readonly @string infoˢ = "info"u8;
internal static readonly @string kdaOneStepNoCounterˢ = "KDA OneStepNoCounter"u8;
internal static readonly @string feedbackKdfˢ = "Feedback KDF"u8;
internal static readonly @string pbkdfˢ = "PBKDF"u8;
internal static readonly @string passwordˢ = "password"u8;
internal static readonly @string kdfTlsV12Cvlˢ = "KDF TLS v1.2 CVL"u8;
internal static readonly @string kdfTlsV13Cvlˢ = "KDF TLS v1.3 CVL"u8;

public static void TestFIPS140(ж<testing.T> Ꮡt) {
    moduleStatus(Ꮡt);
    if (boring.Enabled) {
        Ꮡt.Skip(goBoringCryptoShimsˢ);
    }

    var aesKey = new slice<byte>(128 / 8);
    var aesIV = new slice<byte>(aes.ΔBlockSize);
    var plaintext = slice<byte>("Go Cryptographic Module TestFIPS140 plaintext..."u8);
    var plaintextSHA256 = decodeHex(Ꮡt, "06b2614e2ef315832b23f5d0ff70294d8ddd3889527dfbe75707fe41da929325"u8);
    var (aesBlock, err) = aes.New(aesKey);
    fatalIfErr(Ꮡt, err);

    testFIPS140v126(Ꮡt, plaintext);

    var aesBlockʗ1 = aesBlock;
    var aesIVʗ1 = aesIV;
    var plaintextʗ1 = plaintext;
    Ꮡt.Run(aesCtrˢ, (ж<testing.T> tΔ1) => {
        ensureServiceIndicator(tΔ1);
        var ctr = aes.NewCTR(aesBlockʗ1, aesIVʗ1);
        var ciphertext = new slice<byte>(len(plaintextʗ1));
        ctr.XORKeyStream(ciphertext, plaintextʗ1);
        tΔ1.Logf("AES-CTR ciphertext: %x"u8, ciphertext);
        var @out = new slice<byte>(len(plaintextʗ1));
        ctr = aes.NewCTR(aesBlockʗ1, aesIVʗ1);
        ctr.XORKeyStream(@out, ciphertext);
        tΔ1.Logf("AES-CTR decrypted plaintext: %s"u8, @out);
        if (!bytes.Equal(plaintextʗ1, @out)) {
            tΔ1.Errorf("AES-CTR round trip failed"u8);
        }
    });

    var aesBlockʗ2 = aesBlock;
    var aesIVʗ2 = aesIV;
    var plaintextʗ2 = plaintext;
    Ꮡt.Run(aesCbcˢ, (ж<testing.T> tΔ2) => {
        ensureServiceIndicator(tΔ2);
        var cbcEnc = aes.NewCBCEncrypter(aesBlockʗ2, new array<byte>(aesIVʗ2, 16));
        var ciphertext = new slice<byte>(len(plaintextʗ2));
        cbcEnc.CryptBlocks(ciphertext, plaintextʗ2);
        tΔ2.Logf("AES-CBC ciphertext: %x"u8, ciphertext);
        var cbcDec = aes.NewCBCDecrypter(aesBlockʗ2, new array<byte>(aesIVʗ2, 16));
        var @out = new slice<byte>(len(plaintextʗ2));
        cbcDec.CryptBlocks(@out, ciphertext);
        tΔ2.Logf("AES-CBC decrypted plaintext: %s"u8, @out);
        if (!bytes.Equal(plaintextʗ2, @out)) {
            tΔ2.Errorf("AES-CBC round trip failed"u8);
        }
    });

    var aesBlockʗ3 = aesBlock;
    var plaintextʗ3 = plaintext;
    Ꮡt.Run(aesGcmˢ, (ж<testing.T> tΔ3) => {
        ensureServiceIndicator(tΔ3);
        var (g, errΔ1) = gcm.New(aesBlockʗ3, 12, 16);
        fatalIfErr(tΔ3, errΔ1);
        var nonce = new slice<byte>(12);
        var ciphertext = new slice<byte>(len(plaintextʗ3) + g.Overhead());
        gcm.SealWithRandomNonce(g, nonce, ciphertext, plaintextʗ3, default!);
        tΔ3.Logf("AES-GCM ciphertext: %x"u8, ciphertext);
        (var @out, errΔ1) = g.Open(default!, nonce, ciphertext, default!);
        fatalIfErr(tΔ3, errΔ1);
        tΔ3.Logf("AES-GCM decrypted plaintext: %s"u8, @out);
        if (!bytes.Equal(plaintextʗ3, @out)) {
            tΔ3.Errorf("AES-GCM round trip failed"u8);
        }
    });

    var aesBlockʗ4 = aesBlock;
    Ꮡt.Run(counterKdfˢ, (ж<testing.T> tΔ4) => {
        ensureServiceIndicator(tΔ4);
        var k = gcm.NewCounterKDF(aesBlockʗ4);
        var context = new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12}.array();
        var key = k.DeriveKey(0x01, context);
        tΔ4.Logf("Counter KDF key: %x"u8, key);
    });

    Ꮡt.Run(kasEccSscˢ, (ж<testing.T> tΔ5) => {
        ensureServiceIndicator(tΔ5);
        var (k, errΔ2) = ecdh.GenerateKey<ecdh.P256PointжPoint>(ecdh.P256(), rand.Reader);
        fatalIfErr(tΔ5, errΔ2);
        var pk = k.PublicKey();
        (var shared, errΔ2) = ecdh.ECDH<ecdh.P256PointжPoint>(ecdh.P256(), k, pk);
        fatalIfErr(tΔ5, errΔ2);
        tΔ5.Logf("KAS-ECC-SSC shared secret: %x"u8, shared);
    });

    var plaintextSHA256ʗ1 = plaintextSHA256;
    Ꮡt.Run(ecdsaKeyGenSigGenSigVerˢ, (ж<testing.T> tΔ6) => {
        ensureServiceIndicator(tΔ6);
        var (k, errΔ3) = ecdsa.GenerateKey<ecdsa.P256PointжPoint>(ecdsa.P256(), rand.Reader);
        fatalIfErr(tΔ6, errΔ3);

        (var sig, errΔ3) = ecdsa.Sign<ecdsa.P256PointжPoint, hash.Hash>(ecdsa.P256(), widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ1 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ1)), k, rand.Reader, plaintextSHA256ʗ1);
        fatalIfErr(tΔ6, errΔ3);
        tΔ6.Logf("ECDSA signature: %x"u8, sig.OrTypedNil());
        errΔ3 = ecdsa.Verify<ecdsa.P256PointжPoint>(ecdsa.P256(), k.PublicKey(), plaintextSHA256ʗ1, sig);
        if (errΔ3 != default!) {
            tΔ6.Errorf("ECDSA signature verification failed"u8);
        }

        (sig, errΔ3) = ecdsa.SignDeterministic<ecdsa.P256PointжPoint, hash.Hash>(ecdsa.P256(), widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ1 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ1)), k, plaintextSHA256ʗ1);
        fatalIfErr(tΔ6, errΔ3);
        tΔ6.Logf("ECDSA deterministic signature: %x"u8, sig.OrTypedNil());
        errΔ3 = ecdsa.Verify<ecdsa.P256PointжPoint>(ecdsa.P256(), k.PublicKey(), plaintextSHA256ʗ1, sig);
        if (errΔ3 != default!) {
            tΔ6.Errorf("ECDSA deterministic signature verification failed"u8);
        }
    });

    var plaintextʗ4 = plaintext;
    Ꮡt.Run(eddsaKeyGenSigGenSigVerˢ, (ж<testing.T> tΔ7) => {
        ensureServiceIndicator(tΔ7);
        var (k, errΔ4) = ed25519.GenerateKey();
        fatalIfErr(tΔ7, errΔ4);

        var sig = ed25519.Sign(k, plaintextʗ4);
        tΔ7.Logf("EDDSA signature: %x"u8, sig);

        (var pk, errΔ4) = ed25519.NewPublicKey(k.PublicKey());
        fatalIfErr(tΔ7, errΔ4);
        errΔ4 = ed25519.Verify(pk, plaintextʗ4, sig);
        if (errΔ4 != default!) {
            tΔ7.Errorf("EDDSA signature verification failed"u8);
        }
    });

    var plaintextʗ5 = plaintext;
    Ꮡt.Run(ctrDRBGˢ, (ж<testing.T> tΔ8) => {
        ensureServiceIndicator(tΔ8);
        var r = drbg.NewCounter(Ꮡ(array<byte>.Alias(plaintextʗ5, 48)));
        r.Reseed(Ꮡ(array<byte>.Alias(plaintextʗ5, 48)), Ꮡ(array<byte>.Alias(plaintextʗ5, 48)));
        var @out = new slice<byte>(16);
        r.Generate(@out, Ꮡ(array<byte>.Alias(plaintextʗ5, 48)));
        tΔ8.Logf("ctrDRBG output: %x"u8, @out);
    });

    var plaintextʗ6 = plaintext;
    Ꮡt.Run(hmacˢ, (ж<testing.T> tΔ9) => {
        ensureServiceIndicator(tΔ9);
        var h = hmac.New<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextʗ6);
        h.Write(plaintextʗ6);
        var @out = h.Sum(default!);
        tΔ9.Logf("HMAC output: %x"u8, @out);
    });

    Ꮡt.Run(mlKemKeyGenEncapDecapˢ, (ж<testing.T> tΔ10) => {
        ensureServiceIndicator(tΔ10);
        var (k, errΔ5) = mlkem.GenerateKey768();
        fatalIfErr(tΔ10, errΔ5);

        var (ss, c) = k.EncapsulationKey().Encapsulate();
        tΔ10.Logf("ML-KEM encapsulation: %x"u8, c);

        (var ss2, errΔ5) = k.Decapsulate(c);
        fatalIfErr(tΔ10, errΔ5);
        tΔ10.Logf("ML-KEM shared secret: %x"u8, ss);
        if (!bytes.Equal(ss, ss2)) {
            tΔ10.Errorf("ML-KEM round trip failed"u8);
        }
    });

    ref var rsaKey = ref heap<ж<rsa.PrivateKey>>(out var ᏑrsaKey);
    Ꮡt.Run(rsaKeyGenˢ, (ж<testing.T> tΔ11) => {
        ensureServiceIndicator(tΔ11);
        error errΔ6 = default!;
        (ᏑrsaKey.ValueSlot, errΔ6) = rsa.GenerateKey(rand.Reader, 2048);
        fatalIfErr(tΔ11, errΔ6);
        tΔ11.Log(rsaKeyGeneratedˢ);
    });

    var plaintextSHA256ʗ2 = plaintextSHA256;
    Ꮡt.Run(rsaSigGenSigVerPkcs15ˢ, (ж<testing.T> tΔ12) => {
        ensureServiceIndicator(tΔ12);
        var (sig, errΔ7) = rsa.SignPKCS1v15(ᏑrsaKey.ValueSlot, sha256ˢ, plaintextSHA256ʗ2);
        fatalIfErr(tΔ12, errΔ7);
        tΔ12.Logf("RSA PKCS1v15 signature: %x"u8, sig);

        errΔ7 = rsa.VerifyPKCS1v15(ᏑrsaKey.ValueSlot.PublicKey(), sha256ˢ, plaintextSHA256ʗ2, sig);
        fatalIfErr(tΔ12, errΔ7);
    });

    var plaintextSHA256ʗ3 = plaintextSHA256;
    Ꮡt.Run(rsaSigGenSigVerPssˢ, (ж<testing.T> tΔ13) => {
        ensureServiceIndicator(tΔ13);
        var (sig, errΔ8) = rsa.SignPSS(rand.Reader, ᏑrsaKey.ValueSlot, new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), plaintextSHA256ʗ3, 16);
        fatalIfErr(tΔ13, errΔ8);
        tΔ13.Logf("RSA PSS signature: %x"u8, sig);

        errΔ8 = rsa.VerifyPSS(ᏑrsaKey.ValueSlot.PublicKey(), new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), plaintextSHA256ʗ3, sig);
        fatalIfErr(tΔ13, errΔ8);
    });

    Ꮡt.Run(rsaKeyGenWSmallKeyNotˢ, (ж<testing.T> tΔ14) => {
        ensureServiceIndicatorFalse(tΔ14);
        var (_, errΔ9) = rsa.GenerateKey(rand.Reader, 512);
        fatalIfErr(tΔ14, errΔ9);
        tΔ14.Log(rsaKeyGeneratedˢ);
    });

    var plaintextSHA256ʗ4 = plaintextSHA256;
    Ꮡt.Run(ktsIfcOaepˢ, (ж<testing.T> tΔ15) => {
        ensureServiceIndicator(tΔ15);
        var (c, errΔ10) = rsa.EncryptOAEP(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), rand.Reader, ᏑrsaKey.ValueSlot.PublicKey(), plaintextSHA256ʗ4, default!);
        fatalIfErr(tΔ15, errΔ10);
        tΔ15.Logf("RSA OAEP ciphertext: %x"u8, c);

        (var @out, errΔ10) = rsa.DecryptOAEP(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()), ᏑrsaKey.ValueSlot, c, default!);
        fatalIfErr(tΔ15, errΔ10);
        tΔ15.Logf("RSA OAEP decrypted plaintext: %x"u8, @out);
        if (!bytes.Equal(plaintextSHA256ʗ4, @out)) {
            tΔ15.Errorf("RSA OAEP round trip failed"u8);
        }
    });

    var plaintextʗ7 = plaintext;
    Ꮡt.Run(sha2224ˢ, (ж<testing.T> tΔ16) => {
        ensureServiceIndicator(tΔ16);
        var h = sha256.New224();
        h.Write(plaintextʗ7);
        var @out = h.Sum(default!);
        tΔ16.Logf("SHA2-224 output: %x"u8, @out);
    });

    var plaintextʗ8 = plaintext;
    Ꮡt.Run(sha2256ˢ, (ж<testing.T> tΔ17) => {
        ensureServiceIndicator(tΔ17);
        var h = sha256.New();
        h.Write(plaintextʗ8);
        var @out = h.Sum(default!);
        tΔ17.Logf("SHA2-256 output: %x"u8, @out);
    });

    var plaintextʗ9 = plaintext;
    Ꮡt.Run(sha2384ˢ, (ж<testing.T> tΔ18) => {
        ensureServiceIndicator(tΔ18);
        var h = sha512.New384();
        h.Write(plaintextʗ9);
        var @out = h.Sum(default!);
        tΔ18.Logf("SHA2-384 output: %x"u8, @out);
    });

    var plaintextʗ10 = plaintext;
    Ꮡt.Run(sha2512ˢ, (ж<testing.T> tΔ19) => {
        ensureServiceIndicator(tΔ19);
        var h = sha512.New();
        h.Write(plaintextʗ10);
        var @out = h.Sum(default!);
        tΔ19.Logf("SHA2-512 output: %x"u8, @out);
    });

    var plaintextʗ11 = plaintext;
    Ꮡt.Run(sha2512224ˢ, (ж<testing.T> tΔ20) => {
        ensureServiceIndicator(tΔ20);
        var h = sha512.New512_224();
        h.Write(plaintextʗ11);
        var @out = h.Sum(default!);
        tΔ20.Logf("SHA2-512/224 output: %x"u8, @out);
    });

    var plaintextʗ12 = plaintext;
    Ꮡt.Run(sha2512256ˢ, (ж<testing.T> tΔ21) => {
        ensureServiceIndicator(tΔ21);
        var h = sha512.New512_256();
        h.Write(plaintextʗ12);
        var @out = h.Sum(default!);
        tΔ21.Logf("SHA2-512/256 output: %x"u8, @out);
    });

    var plaintextʗ13 = plaintext;
    Ꮡt.Run(sha3224ˢ, (ж<testing.T> tΔ22) => {
        ensureServiceIndicator(tΔ22);
        var h = sha3.New224();
        h.Write(plaintextʗ13);
        var @out = h.Sum(default!);
        tΔ22.Logf("SHA3-224 output: %x"u8, @out);
    });

    var plaintextʗ14 = plaintext;
    Ꮡt.Run(sha3256ˢ, (ж<testing.T> tΔ23) => {
        ensureServiceIndicator(tΔ23);
        var h = sha3.New256();
        h.Write(plaintextʗ14);
        var @out = h.Sum(default!);
        tΔ23.Logf("SHA3-256 output: %x"u8, @out);
    });

    var plaintextʗ15 = plaintext;
    Ꮡt.Run(sha3384ˢ, (ж<testing.T> tΔ24) => {
        ensureServiceIndicator(tΔ24);
        var h = sha3.New384();
        h.Write(plaintextʗ15);
        var @out = h.Sum(default!);
        tΔ24.Logf("SHA3-384 output: %x"u8, @out);
    });

    var plaintextʗ16 = plaintext;
    Ꮡt.Run(sha3512ˢ, (ж<testing.T> tΔ25) => {
        ensureServiceIndicator(tΔ25);
        var h = sha3.New512();
        h.Write(plaintextʗ16);
        var @out = h.Sum(default!);
        tΔ25.Logf("SHA3-512 output: %x"u8, @out);
    });

    var plaintextʗ17 = plaintext;
    Ꮡt.Run(shake128ˢ, (ж<testing.T> tΔ26) => {
        ensureServiceIndicator(tΔ26);
        var h = sha3.NewShake128();
        h.Write(plaintextʗ17);
        var @out = new slice<byte>(16);
        h.Read(@out);
        tΔ26.Logf("SHAKE-128 output: %x"u8, @out);
    });

    var plaintextʗ18 = plaintext;
    Ꮡt.Run(shake256ˢ, (ж<testing.T> tΔ27) => {
        ensureServiceIndicator(tΔ27);
        var h = sha3.NewShake256();
        h.Write(plaintextʗ18);
        var @out = new slice<byte>(16);
        h.Read(@out);
        tΔ27.Logf("SHAKE-256 output: %x"u8, @out);
    });

    var plaintextʗ19 = plaintext;
    Ꮡt.Run(cSHAKE128ˢ, (ж<testing.T> tΔ28) => {
        ensureServiceIndicator(tΔ28);
        var h = sha3.NewCShake128(default!, slice<byte>("test"u8));
        h.Write(plaintextʗ19);
        var @out = new slice<byte>(16);
        h.Read(@out);
        tΔ28.Logf("cSHAKE-128 output: %x"u8, @out);
    });

    var plaintextʗ20 = plaintext;
    Ꮡt.Run(cSHAKE256ˢ, (ж<testing.T> tΔ29) => {
        ensureServiceIndicator(tΔ29);
        var h = sha3.NewCShake256(default!, slice<byte>("test"u8));
        h.Write(plaintextʗ20);
        var @out = new slice<byte>(16);
        h.Read(@out);
        tΔ29.Logf("cSHAKE-256 output: %x"u8, @out);
    });

    var plaintextSHA256ʗ5 = plaintextSHA256;
    Ꮡt.Run(kdaHkdfˢ, (ж<testing.T> tΔ30) => {
        ensureServiceIndicator(tΔ30);
        var key = hkdf.Key<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextSHA256ʗ5, slice<byte>("salt"u8), infoˢ, 16);
        tΔ30.Logf("HKDF key: %x"u8, key);
    });

    var plaintextSHA256ʗ6 = plaintextSHA256;
    Ꮡt.Run(kdaOneStepNoCounterˢ, (ж<testing.T> tΔ31) => {
        ensureServiceIndicator(tΔ31);
        var key = hkdf.Extract<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextSHA256ʗ6, slice<byte>("salt"u8));
        tΔ31.Logf("KDA OneStepNoCounter key: %x"u8, key);
    });

    var plaintextSHA256ʗ7 = plaintextSHA256;
    Ꮡt.Run(feedbackKdfˢ, (ж<testing.T> tΔ32) => {
        ensureServiceIndicator(tΔ32);
        var key = hkdf.Expand<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextSHA256ʗ7, infoˢ, 16);
        tΔ32.Logf("Feedback KDF key: %x"u8, key);
    });

    var plaintextSHA256ʗ8 = plaintextSHA256;
    Ꮡt.Run(pbkdfˢ, (ж<testing.T> tΔ33) => {
        ensureServiceIndicator(tΔ33);
        var (key, errΔ11) = pbkdf2.Key<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), passwordˢ, plaintextSHA256ʗ8, 2, 16);
        fatalIfErr(tΔ33, errΔ11);
        tΔ33.Logf("PBKDF key: %x"u8, key);
    });

    var plaintextSHA256ʗ9 = plaintextSHA256;
    Ꮡt.Run(kdfTlsV12Cvlˢ, (ж<testing.T> tΔ34) => {
        ensureServiceIndicator(tΔ34);
        var key = tls12.MasterSecret<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextSHA256ʗ9, slice<byte>("test"u8));
        tΔ34.Logf("TLS v1.2 CVL Master Secret: %x"u8, key);
    });

    var plaintextSHA256ʗ10 = plaintextSHA256;
    Ꮡt.Run(kdfTlsV13Cvlˢ, (ж<testing.T> tΔ35) => {
        ensureServiceIndicator(tΔ35);
        var es = tls13.NewEarlySecret<hash.Hash>(widen<ж<sha256.Digest>, hash.Hash>(sha256.New, elemᴛ0 => new fipstest_internal_test_package.sha256_DigestжHash(elemᴛ0)), plaintextSHA256ʗ10);
        var hs = es.HandshakeSecret(plaintextSHA256ʗ10);
        var ms = hs.MasterSecret();
        var client = ms.ClientApplicationTrafficSecret(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()));
        var server = ms.ServerApplicationTrafficSecret(new fipstest_internal_test_package.sha256_DigestжHash(sha256.New()));
        tΔ35.Logf("TLS v1.3 CVL Application Traffic Secrets: client %x, server %x"u8, client, server);
    });
}

internal static void ensureServiceIndicator(ж<testing.T> Ꮡt) {
    fips140.ResetServiceIndicator();
    Ꮡt.Cleanup(() => {
        if (fips140.ServiceIndicator()){
            Ꮡt.Logf("Service indicator is set"u8);
        } else {
            Ꮡt.Errorf("Service indicator is not set"u8);
        }
    });
}

internal static void ensureServiceIndicatorFalse(ж<testing.T> Ꮡt) {
    fips140.ResetServiceIndicator();
    Ꮡt.Cleanup(() => {
        if (!fips140.ServiceIndicator()){
            Ꮡt.Logf("Service indicator is not set"u8);
        } else {
            Ꮡt.Errorf("Service indicator is set"u8);
        }
    });
}

internal static partial void fatalIfErr(ж<testing.T> Ꮡt, error err) {
    Ꮡt.Helper();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
}

internal static slice<byte> decodeHex(ж<testing.T> Ꮡt, @string s) {
    Ꮡt.Helper();
    s = strings.ReplaceAll(s, " "u8, ""u8);
    var (b, err) = hex.DecodeString(s);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    return b;
}

} // end fipstest_internal_test_package
