// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bufio = bufio_package;
using bytes = bytes_package;
using crypto = crypto_package;
using boring = go.crypto.@internal.boring_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using fips140 = go.crypto.@internal.fips140_package;
using rand = go.crypto.rand_package;
using static go.crypto.rsa_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
using sha512 = go.crypto.sha512_package;
using Δx509 = go.crypto.x509_package;
using hex = encoding.hex_package;
using pem = encoding.pem_package;
using flag = flag_package;
using fmt = fmt_package;
using io = io_package;
using big = math.big_package;
using os = os_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using encoding;
using go.crypto;
using go.crypto.@internal;
using hash = hash_package;
using math;
using rsa = go.crypto.rsa_package;
using static go.crypto.rsa_internal_test_package;

partial class rsa_test_package {

public static void TestKeyGeneration(ж<testing.T> Ꮡt) {
    var sizes = new nint[]{128, 512, 1024, 2048, 3072, 4096}.slice();
    if (testing.Short()) {
        sizes = sizes[..2];
    }
    foreach (var (_, size) in sizes) {
        Ꮡt.Run(fmt.Sprintf("%d"u8, size), (ж<testing.T> tΔ1) => {
            if (size < 1024) {
                var (_, errΔ1) = GenerateKey(rand.Reader, size);
                if (errΔ1 == default!) {
                    tΔ1.Errorf("GenerateKey(%d) succeeded without GODEBUG"u8, size);
                }
                tΔ1.Setenv(godebugˢ, rsa1024min0ˢ);
            }
            var (priv, err) = GenerateKey(rand.Reader, size);
            if (err != default!) {
                tΔ1.Errorf("GenerateKey(%d): %v"u8, size, err);
            }
            {
                nint bits = (~priv).N.BitLen(); if (bits != size) {
                    tΔ1.Errorf("key too short (%d vs %d)"u8, bits, size);
                }
            }
            testKeyBasics(tΔ1, priv);
        });
    }
}

public static void Test3PrimeKeyGeneration(ж<testing.T> Ꮡt) {
    nint size = 1024;
    if (testing.Short()) {
        Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
        size = 256;
    }

    var (priv, err) = GenerateMultiPrimeKey(rand.Reader, 3, size);
    if (err != default!) {
        Ꮡt.Errorf("failed to generate key"u8);
    }
    testKeyBasics(Ꮡt, priv);
}

public static void Test4PrimeKeyGeneration(ж<testing.T> Ꮡt) {
    nint size = 1024;
    if (testing.Short()) {
        Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
        size = 256;
    }

    var (priv, err) = GenerateMultiPrimeKey(rand.Reader, 4, size);
    if (err != default!) {
        Ꮡt.Errorf("failed to generate key"u8);
    }
    testKeyBasics(Ꮡt, priv);
}

public static void TestNPrimeKeyGeneration(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    nint primeSize = 64;
    nint maxN = 24;
    if (testing.Short()) {
        primeSize = 16;
        maxN = 16;
    }
    // Test that generation of N-prime keys works for N > 4.
    for (nint n = 5; n < maxN; n++) {
        var (priv, err) = GenerateMultiPrimeKey(rand.Reader, n, 64 + n * primeSize);
        if (err == default!){
            testKeyBasics(Ꮡt, priv);
        } else {
            Ꮡt.Errorf("failed to generate %d-prime key"u8, n);
        }
    }
}

public static void TestImpossibleKeyGeneration(ж<testing.T> Ꮡt) {
    // This test ensures that trying to generate or validate toy RSA keys
    // doesn't enter an infinite loop or panic.
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    for (nint i = 0; i < 32; i++) {
        GenerateKey(rand.Reader, i);
        GenerateMultiPrimeKey(rand.Reader, 3, i);
        GenerateMultiPrimeKey(rand.Reader, 4, i);
        GenerateMultiPrimeKey(rand.Reader, 5, i);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"skipping in short mode"u8;

public static void TestTinyKeyGeneration(ж<testing.T> Ꮡt) {
    // Toy-sized keys can randomly hit hard failures in GenerateKey.
    if (testing.Short()) {
        Ꮡt.Skip(skippingInShortModeˢ);
    }
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    foreach (var _ᴛ1 in range(10000)) {
        var (k, err) = GenerateKey(rand.Reader, 32);
        if (err != default!) {
            Ꮡt.Fatalf("GenerateKey(32): %v"u8, err);
        }
        {
            var errΔ1 = k.Validate(); if (errΔ1 != default!) {
                Ꮡt.Fatalf("Validate(32): %v"u8, errΔ1);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string beginRsaTestingKeyˢ = """
-----BEGIN RSA TESTING KEY-----
MGECAQACEQDar8EuoZuSosYtE9SeXSyPAgMBAAECEBf7XDET8e6jjTcfO7y/sykC
CQDozXjCjkBzLQIJAPB6MqNbZaQrAghbZTdQoko5LQIIUp9ZiKDdYjMCCCCpqzmX
d8Y7
-----END RSA TESTING KEY-----
"""u8.Raw();

public static void TestGnuTLSKey(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    // This is a key generated by `certtool --generate-privkey --bits 128`.
    // It's such that de ≢ 1 mod φ(n), but is congruent mod the order of
    // the group.
    var priv = parseKey(testingKey(beginRsaTestingKeyˢ));
    testKeyBasics(Ꮡt, priv);
}

internal static void testKeyBasics(ж<testing.T> Ꮡt, ж<rsa.PrivateKey> Ꮡpriv) {
    GoFrame ᒐ = default;
    try {
        ref var priv = ref Ꮡpriv.DerefOrNull();

        defer(() => {
            if (Ꮡt.Failed()) {
                Ꮡt.Logf("failed key: %#v"u8, Ꮡpriv.OrTypedNil());
            }
        }, ref ᒐ);

        {
            var errΔ1 = priv.Validate(); if (errΔ1 != default!) {
                Ꮡt.Errorf("Validate() failed: %s"u8, errΔ1);
            }
        }
        if (priv.D.Cmp(priv.N) > 0) {
            Ꮡt.Errorf("private exponent too large"u8);
        }

        var msg = slice<byte>("hi!"u8);
        var (enc, err) = EncryptPKCS1v15(rand.Reader, Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), msg);
        if (err != default!) {
            Ꮡt.Errorf("EncryptPKCS1v15: %v"u8, err);
            return;
        }

        (var dec, err) = DecryptPKCS1v15(default!, Ꮡpriv, enc);
        if (err != default!) {
            Ꮡt.Errorf("DecryptPKCS1v15: %v"u8, err);
            return;
        }
        if (!bytes.Equal(dec, msg)) {
            Ꮡt.Errorf("got:%x want:%x (%+v)"u8, dec, msg, Ꮡpriv.OrTypedNil());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestAllocations(ж<testing.T> Ꮡt) {
    cryptotest.SkipTestAllocations(Ꮡt);

    var m = slice<byte>("Hello Gophers"u8);
    var (c, err) = EncryptPKCS1v15(rand.Reader, test2048Key.of(rsa.PrivateKey.ᏑPublicKey), m);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }

    {
        var cʗ1 = c;
        var mʗ1 = m;
        var allocs = testing.AllocsPerRun(100, () => {
            var (p, errΔ1) = DecryptPKCS1v15(default!, test2048Key, cʗ1);
            if (errΔ1 != default!) {
                Ꮡt.Fatal(errΔ1);
            }
            if (!bytes.Equal(p, mʗ1)) {
                Ꮡt.Fatalf("unexpected output: %q"u8, p);
            }
        }); if (allocs > 10D) {
            Ꮡt.Errorf("expected less than 10 allocations, got %0.1f"u8, allocs);
        }
    }
}

internal static ж<bool> allFlag = flag.Bool("all"u8, false, "test all key sizes up to 2048"u8);

public static void TestEverything(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        // Skip key generation, but still test real sizes.
        foreach (var (_, key) in new ж<rsa.PrivateKey>[]{test1024Key, test2048Key}.slice()) {
            var keyʗ1 = key;
            Ꮡt.Run(fmt.Sprintf("%d"u8, (~key).N.BitLen()), (ж<testing.T> tΔ1) => {
                tΔ1.Parallel();
                testEverything(tΔ1, keyʗ1);
            });
        }
        return;
    }

    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    nint min = 32;
    nint max = 560; // any smaller than this and not all tests will run
    if (allFlag.Value) {
        max = 2048;
    }
    for (nint sizeᴛ1 = min; sizeᴛ1 <= max; sizeᴛ1++) {
        var size = sizeᴛ1;
        Ꮡt.Run(fmt.Sprintf("%d"u8, size), (ж<testing.T> tΔ2) => {
            tΔ2.Parallel();
            var (priv, err) = GenerateKey(rand.Reader, size);
            if (err != default!) {
                tΔ2.Fatalf("GenerateKey(%d): %v"u8, size, err);
            }
            {
                nint bits = (~priv).N.BitLen(); if (bits != size) {
                    tΔ2.Errorf("key too short (%d vs %d)"u8, bits, size);
                }
            }
            testEverything(tΔ2, priv);
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object keyTooSmallForˢ = (@string)"key too small for EncryptPKCS1v15"u8;
internal static readonly object keyTooSmallForˢ2 = (@string)"key too small for EncryptOAEP"u8;
internal static readonly object keyTooSmallForˢ3 = (@string)"key too small for SignPKCS1v15"u8;
internal static readonly object keyTooSmallForSignPSSˢ = (@string)"key too small for SignPSS with PSSSaltLengthAuto"u8;
internal static readonly object keyTooSmallForSignPSSˢ2 = (@string)"key too small for SignPSS with PSSSaltLengthEqualsHash"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string hashMsgᶜ = "crypto/rsa: input must be hashed message"u8;

internal static void testEverything(ж<testing.T> Ꮡt, ж<rsa.PrivateKey> Ꮡpriv) {
    ref var priv = ref Ꮡpriv.DerefOrNull();
    var cᴷ = new stackAppend<byte>();

    var validateErr = priv.Validate();
    if (validateErr != default! && len(priv.Primes) >= 2) {
        Ꮡt.Errorf("Validate() failed: %s"u8, validateErr);
    }

    var msg = slice<byte>("test"u8);
    var (enc, err) = EncryptPKCS1v15(rand.Reader, Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), msg);
    if (AreEqual(err, ErrMessageTooLong)){
        Ꮡt.Log(keyTooSmallForˢ);
    } else 
    if (err != default!) {
        Ꮡt.Errorf("EncryptPKCS1v15: %v"u8, err);
    }
    if (err == default!) {
        var (dec, errΔ1) = DecryptPKCS1v15(default!, Ꮡpriv, enc);
        if (errΔ1 != default!) {
            Ꮡt.Errorf("DecryptPKCS1v15: %v"u8, errΔ1);
        }
        errΔ1 = DecryptPKCS1v15SessionKey(default!, Ꮡpriv, enc, new slice<byte>(4));
        if (errΔ1 != default!) {
            Ꮡt.Errorf("DecryptPKCS1v15SessionKey: %v"u8, errΔ1);
        }
        if (!bytes.Equal(dec, msg)) {
            Ꮡt.Errorf("got:%x want:%x (%+v)"u8, dec, msg, Ꮡpriv.OrTypedNil());
        }
    }

    var label = slice<byte>("label"u8);
    (enc, err) = EncryptOAEP(sha256.New(), rand.Reader, Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), msg, label);
    if (AreEqual(err, ErrMessageTooLong)){
        Ꮡt.Log(keyTooSmallForˢ2);
    } else 
    if (err != default!) {
        Ꮡt.Errorf("EncryptOAEP: %v"u8, err);
    }
    if (err == default!) {
        var (dec, errΔ2) = DecryptOAEP(sha256.New(), default!, Ꮡpriv, enc, label);
        if (errΔ2 != default!) {
            Ꮡt.Errorf("DecryptOAEP: %v"u8, errΔ2);
        }
        if (!bytes.Equal(dec, msg)) {
            Ꮡt.Errorf("got:%x want:%x (%+v)"u8, dec, msg, Ꮡpriv.OrTypedNil());
        }
    }

    @string hashMsg = hashMsgᶜ;
    (var sig, err) = SignPKCS1v15(default!, Ꮡpriv, crypto.SHA256, msg);
    if (err == default! || err.Error() != hashMsg) {
        Ꮡt.Errorf("SignPKCS1v15 with bad hash: err = %q, want %q"u8, err, hashMsg);
    }

    var hash = sha256.Sum256(msg);
    (sig, err) = SignPKCS1v15(default!, Ꮡpriv, crypto.SHA256, hash[..]);
    if (AreEqual(err, ErrMessageTooLong)){
        Ꮡt.Log(keyTooSmallForˢ3);
    } else 
    if (err != default!) {
        Ꮡt.Errorf("SignPKCS1v15: %v"u8, err);
    }
    if (err == default!) {
        err = VerifyPKCS1v15(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], sig);
        if (err != default!) {
            Ꮡt.Errorf("VerifyPKCS1v15: %v"u8, err);
        }
        sig[1] ^= (byte)(0x80);
        err = VerifyPKCS1v15(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], sig);
        if (err == default!) {
            Ꮡt.Errorf("VerifyPKCS1v15 success for tampered signature"u8);
        }
        sig[1] ^= (byte)(0x80);
        hash[1] ^= (byte)(0x80);
        err = VerifyPKCS1v15(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], sig);
        if (err == default!) {
            Ꮡt.Errorf("VerifyPKCS1v15 success for tampered message"u8);
        }
        hash[1] ^= (byte)(0x80);
    }

    var opts = Ꮡ(new PSSOptions(SaltLength: PSSSaltLengthAuto));
    (sig, err) = SignPSS(rand.Reader, Ꮡpriv, crypto.SHA256, hash[..], opts);
    if (AreEqual(err, ErrMessageTooLong)){
        Ꮡt.Log(keyTooSmallForSignPSSˢ);
    } else 
    if (err != default!) {
        Ꮡt.Errorf("SignPSS: %v"u8, err);
    }
    if (err == default!) {
        err = VerifyPSS(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], sig, opts);
        if (err != default!) {
            Ꮡt.Errorf("VerifyPSS: %v"u8, err);
        }
        sig[1] ^= (byte)(0x80);
        err = VerifyPSS(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], sig, opts);
        if (err == default!) {
            Ꮡt.Errorf("VerifyPSS success for tampered signature"u8);
        }
        sig[1] ^= (byte)(0x80);
        hash[1] ^= (byte)(0x80);
        err = VerifyPSS(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], sig, opts);
        if (err == default!) {
            Ꮡt.Errorf("VerifyPSS success for tampered message"u8);
        }
        hash[1] ^= (byte)(0x80);
    }

    opts.Value.SaltLength = PSSSaltLengthEqualsHash;
    (sig, err) = SignPSS(rand.Reader, Ꮡpriv, crypto.SHA256, hash[..], opts);
    if (AreEqual(err, ErrMessageTooLong)){
        Ꮡt.Log(keyTooSmallForSignPSSˢ2);
    } else 
    if (err != default!) {
        Ꮡt.Errorf("SignPSS: %v"u8, err);
    }
    if (err == default!) {
        err = VerifyPSS(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], sig, opts);
        if (err != default!) {
            Ꮡt.Errorf("VerifyPSS: %v"u8, err);
        }
        sig[1] ^= (byte)(0x80);
        err = VerifyPSS(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], sig, opts);
        if (err == default!) {
            Ꮡt.Errorf("VerifyPSS success for tampered signature"u8);
        }
        sig[1] ^= (byte)(0x80);
        hash[1] ^= (byte)(0x80);
        err = VerifyPSS(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], sig, opts);
        if (err == default!) {
            Ꮡt.Errorf("VerifyPSS success for tampered message"u8);
        }
        hash[1] ^= (byte)(0x80);
    }

    // Check that an input bigger than the modulus is handled correctly,
    // whether it is longer than the byte size of the modulus or not.
    var c = bytes.Repeat(new byte[]{0xff}.slice(), Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey).Size());
    err = VerifyPSS(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], c, opts);
    if (err == default!) {
        Ꮡt.Errorf("VerifyPSS accepted a large signature"u8);
    }
    (_, err) = DecryptPKCS1v15(default!, Ꮡpriv, c);
    if (err == default!) {
        Ꮡt.Errorf("DecryptPKCS1v15 accepted a large ciphertext"u8);
    }
    c = append(cᴷ, c, (byte)0xff);
    err = VerifyPSS(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hash[..], c, opts);
    if (err == default!) {
        Ꮡt.Errorf("VerifyPSS accepted a long signature"u8);
    }
    (_, err) = DecryptPKCS1v15(default!, Ꮡpriv, c);
    if (err == default!) {
        Ꮡt.Errorf("DecryptPKCS1v15 accepted a long ciphertext"u8);
    }

    if (validateErr == default!) {
        var (derΔ1, errΔ3) = Δx509.MarshalPKCS8PrivateKey(Ꮡpriv.OrTypedNil());
        if (errΔ3 != default!) {
            Ꮡt.Errorf("MarshalPKCS8PrivateKey: %v"u8, errΔ3);
        }
        (var key, errΔ3) = Δx509.ParsePKCS8PrivateKey(derΔ1);
        if (errΔ3 != default!) {
            Ꮡt.Errorf("ParsePKCS8PrivateKey: %v"u8, errΔ3);
        }
        if (!key._<ж<rsa.PrivateKey>>().Equal(Ꮡpriv.OrTypedNil())) {
            Ꮡt.Errorf("private key mismatch"u8);
        }
    }

    (var der, err) = Δx509.MarshalPKIXPublicKey(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey));
    if (err != default!) {
        Ꮡt.Errorf("MarshalPKIXPublicKey: %v"u8, err);
    }
    (var pub, err) = Δx509.ParsePKIXPublicKey(der);
    if (err != default!) {
        Ꮡt.Errorf("ParsePKIXPublicKey: %v"u8, err);
    }
    if (!pub._<ж<rsa.PublicKey>>().Equal(Ꮡpriv.of(rsa.PrivateKey.ᏑPublicKey))) {
        Ꮡt.Errorf("public key mismatch"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedErrorˢ = (@string)"expected error"u8;
internal static readonly @string insecureˢ = "insecure"u8;

public static void TestKeyTooSmall(ж<testing.T> Ꮡt) {
    void checkErr(error err) {
        Ꮡt.Helper();
        if (err == default!) {
            Ꮡt.Error(expectedErrorˢ);
        }
        if (!strings.Contains(err.Error(), insecureˢ)) {
            Ꮡt.Errorf("unexpected error: %v"u8, err);
        }
    }
    var checkErrʗ1 = checkErr;
    void checkErr2(slice<byte> _, error err) {
        Ꮡt.Helper();
        checkErrʗ1(err);
    }

    var buf = new slice<byte>(512 / 8);
    var (ᴛ1, ᴛ2) = test512Key.Sign(rand.Reader, buf, crypto.SHA512);
    checkErr2(ᴛ1, ᴛ2);
    var (ᴛ3, ᴛ4) = test512Key.Sign(rand.Reader, buf, new rsa_test_package.rsa_PSSOptionsжSignerOpts(Ꮡ(new PSSOptions(SaltLength: PSSSaltLengthEqualsHash))));
    checkErr2(ᴛ3, ᴛ4);
    var (ᴛ5, ᴛ6) = test512Key.Decrypt(rand.Reader, buf, Ꮡ(new PKCS1v15DecryptOptions(nil)));
    checkErr2(ᴛ5, ᴛ6);
    var (ᴛ7, ᴛ8) = test512Key.Decrypt(rand.Reader, buf, Ꮡ(new OAEPOptions(Hash: crypto.SHA512)));
    checkErr2(ᴛ7, ᴛ8);
    checkErr(VerifyPKCS1v15(test512Key.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA512, buf, buf));
    checkErr(VerifyPSS(test512Key.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA512, buf, buf, Ꮡ(new PSSOptions(SaltLength: PSSSaltLengthEqualsHash))));
    var (ᴛ9, ᴛ10) = SignPKCS1v15(rand.Reader, test512Key, crypto.SHA512, buf);
    checkErr2(ᴛ9, ᴛ10);
    var (ᴛ11, ᴛ12) = SignPSS(rand.Reader, test512Key, crypto.SHA512, buf, Ꮡ(new PSSOptions(SaltLength: PSSSaltLengthEqualsHash)));
    checkErr2(ᴛ11, ᴛ12);
    var (ᴛ13, ᴛ14) = EncryptPKCS1v15(rand.Reader, test512Key.of(rsa.PrivateKey.ᏑPublicKey), buf);
    checkErr2(ᴛ13, ᴛ14);
    var (ᴛ15, ᴛ16) = EncryptOAEP(sha512.New(), rand.Reader, test512Key.of(rsa.PrivateKey.ᏑPublicKey), buf, default!);
    checkErr2(ᴛ15, ᴛ16);
    var (ᴛ17, ᴛ18) = DecryptPKCS1v15(default!, test512Key, buf);
    checkErr2(ᴛ17, ᴛ18);
    var (ᴛ19, ᴛ20) = DecryptOAEP(sha512.New(), default!, test512Key, buf, default!);
    checkErr2(ᴛ19, ᴛ20);
    checkErr(DecryptPKCS1v15SessionKey(default!, test512Key, buf, buf));
}

internal static @string testingKey(@string s) {
    return strings.ReplaceAll(s, "TESTING KEY"u8, "PRIVATE KEY"u8);
}

internal static ж<rsa.PrivateKey> parseKey(@string s) {
    var (p, _) = pem.Decode(slice<byte>(s));
    if ((~p).Type == "PRIVATE KEY"u8) {
        var (kΔ1, errΔ1) = Δx509.ParsePKCS8PrivateKey((~p).Bytes);
        if (errΔ1 != default!) {
            throw panic(errΔ1);
        }
        return kΔ1._<ж<rsa.PrivateKey>>();
    }
    var (k, err) = Δx509.ParsePKCS1PrivateKey((~p).Bytes);
    if (err != default!) {
        throw panic(err);
    }
    return k;
}

internal static ж<ж<rsa.PrivateKey>> ᏑrsaPrivateKey = new StandardBox<ж<rsa.PrivateKey>>(default(ж<rsa.PrivateKey>));
internal static ref ж<rsa.PrivateKey> rsaPrivateKey => ref ᏑrsaPrivateKey.ValueSlot;
internal static void initᴛrsaPrivateKey() { rsaPrivateKey = test1024Key; }

internal static ж<ж<rsa.PrivateKey>> Ꮡtest512Key = new StandardBox<ж<rsa.PrivateKey>>(parseKey(testingKey("""
-----BEGIN RSA TESTING KEY-----
MIIBOgIBAAJBALKZD0nEffqM1ACuak0bijtqE2QrI/KLADv7l3kK3ppMyCuLKoF0
fd7Ai2KW5ToIwzFofvJcS/STa6HA5gQenRUCAwEAAQJBAIq9amn00aS0h/CrjXqu
/ThglAXJmZhOMPVn4eiu7/ROixi9sex436MaVeMqSNf7Ex9a8fRNfWss7Sqd9eWu
RTUCIQDasvGASLqmjeffBNLTXV2A5g4t+kLVCpsEIZAycV5GswIhANEPLmax0ME/
EO+ZJ79TJKN5yiGBRsv5yvx5UiHxajEXAiAhAol5N4EUyq6I9w1rYdhPMGpLfk7A
IU2snfRJ6Nq2CQIgFrPsWRCkV+gOYcajD17rEqmuLrdIRexpg8N1DOSXoJ8CIGlS
tAboUGBxTDq3ZroNism3DaMIbKPyYrAqhKov1h5V
-----END RSA TESTING KEY-----
"""u8.Raw())));
internal static ref ж<rsa.PrivateKey> test512Key => ref Ꮡtest512Key.ValueSlot;

internal static ж<rsa.PrivateKey> test512KeyTwo = parseKey(testingKey("""
-----BEGIN TESTING KEY-----
MIIBVgIBADANBgkqhkiG9w0BAQEFAASCAUAwggE8AgEAAkEA0wLCoguSfgskR8tY
Fh2AzXQzBpSEmPucxtVe93HzPdQpxvtSTvZe5kIsdvPc7QZ0dCc/qbnUBRbuGIAl
Ir0c9QIDAQABAkAzul+AXhnhcFXKi9ziPwVOWIgRuuLupe//BluriXG53BEBSVrV
Hr7qFqwnSLSLroMzqhZwoqyRgjsLYyGEHDGBAiEA8T0sDPuht3w2Qv61IAvBwjLH
H4HXjRUEWYRn1XjHqAUCIQDf7BYlANRqFfvg1YK3VCM4YyK2mH1UivDi8wdPlJRk
MQIhAMp5i2WCNeNpD6n/WkqBU6kJMXPSaPZy82mm5feYHgt5AiEAkg/QnhB9fjma
1BzRqD4Uv0pDMXIkhooe+Rrn0OwtI3ECIQDP6nxML3JOjbAS7ydFBv176uVsMJib
r4PZozCXKuuGNg==
-----END PRIVATE KEY-----
"""u8.Raw()));

internal static ж<rsa.PrivateKey> test1024Key = parseKey(testingKey("""
-----BEGIN RSA TESTING KEY-----
MIICXQIBAAKBgQCw0YNSqI9T1VFvRsIOejZ9feiKz1SgGfbe9Xq5tEzt2yJCsbyg
+xtcuCswNhdqY5A1ZN7G60HbL4/Hh/TlLhFJ4zNHVylz9mDDx3yp4IIcK2lb566d
fTD0B5EQ9Iqub4twLUdLKQCBfyhmJJvsEqKxm4J4QWgI+Brh/Pm3d4piPwIDAQAB
AoGASC6fj6TkLfMNdYHLQqG9kOlPfys4fstarpZD7X+fUBJ/H/7y5DzeZLGCYAIU
+QeAHWv6TfZIQjReW7Qy00RFJdgwFlTFRCsKXhG5x+IB+jL0Grr08KbgPPDgy4Jm
xirRHZVtU8lGbkiZX+omDIU28EHLNWL6rFEcTWao/tERspECQQDp2G5Nw0qYWn7H
Wm9Up1zkUTnkUkCzhqtxHbeRvNmHGKE7ryGMJEk2RmgHVstQpsvuFY4lIUSZEjAc
DUFJERhFAkEAwZH6O1ULORp8sHKDdidyleYcZU8L7y9Y3OXJYqELfddfBgFUZeVQ
duRmJj7ryu0g0uurOTE+i8VnMg/ostxiswJBAOc64Dd8uLJWKa6uug+XPr91oi0n
OFtM+xHrNK2jc+WmcSg3UJDnAI3uqMc5B+pERLq0Dc6hStehqHjUko3RnZECQEGZ
eRYWciE+Cre5dzfZkomeXE0xBrhecV0bOq6EKWLSVE+yr6mAl05ThRK9DCfPSOpy
F6rgN3QiyCA9J/1FluUCQQC5nX+PTU1FXx+6Ri2ZCi6EjEKMHr7gHcABhMinZYOt
N59pra9UdVQw9jxCU9G7eMyb0jJkNACAuEwakX3gi27b
-----END RSA TESTING KEY-----
"""u8.Raw()));

internal static @string test2048KeyPEM = testingKey("""
-----BEGIN TESTING KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQDNoyFUYeDuqw+k
iyv47iBy/udbWmQdpbUZ8JobHv8uQrvL7sQN6l83teHgNJsXqtiLF3MC+K+XI6Dq
hxUWfQwLip8WEnv7Jx/+53S8yp/CS4Jw86Q1bQHbZjFDpcoqSuwAxlegw18HNZCY
fpipYnA1lYCm+MTjtgXJQbjA0dwUGCf4BDMqt+76Jk3XZF5975rftbkGoT9eu8Jt
Xs5F5Xkwd8q3fkQz+fpLW4u9jrfFyQ61RRFkYrCjlhtGjYIzBHGgQM4n/sNXhiy5
h0tA7Xa6NyYrN/OXe/Y1K8Rz/tzlvbMoxgZgtBuKo1N3m8ckFi7hUVK2eNv7GoAb
teTTPrg/AgMBAAECggEAAnfsVpmsL3R0Bh4gXRpPeM63H6e1a8B8kyVwiO9o0cXX
gKp9+P39izfB0Kt6lyCj/Wg+wOQT7rg5qy1yIw7fBHGmcjquxh3uN0s3YZ+Vcym6
SAY5f0vh/OyJN9r3Uv8+Pc4jtb7So7QDzdWeZurssBmUB0avAMRdGNFGP5SyILcz
l3Q59hTxQ4czRHKjZ06L1/sA+tFVbO1j39FN8nMOU/ovLF4lAmZTkQ6AP6n6XPHP
B8Nq7jSYz6RDO200jzp6UsdrnjjkJRbzOxN/fn+ckCP+WYuq+y/d05ET9PdVa4qI
Jyr80D9QgHmfztcecvYwoskGnkb2F4Tmp0WnAj/xVQKBgQD4TrMLyyHdbAr5hoSi
p+r7qBQxnHxPe2FKO7aqagi4iPEHauEDgwPIcsOYota1ACiSs3BaESdJAClbqPYd
HDI4c2DZ6opux6WYkSju+tVXYW6qarR3fzrP3fUCdz2c2NfruWOqq8YmjzAhTNPm
YzvtzTdwheNYV0Vi71t1SfZmfQKBgQDUAgSUcrgXdGDnSbaNe6KwjY5oZWOQfZe2
DUhqfN/JRFZj+EMfIIh6OQXnZqkp0FeRdfRAFl8Yz8ESHEs4j+TikLJEeOdfmYLS
TWxlMPDTUGbUvSf4g358NJ8TlfYA7dYpSTNPXMRSLtsz1palmaDBTE/V2xKtTH6p
VglRNRUKawKBgCPqBh2TkN9czC2RFkgMb4FcqycN0jEQ0F6TSnVVhtNiAzKmc8s1
POvWJZJDIzjkv/mP+JUeXAdD/bdjNc26EU126rA6KzGgsMPjYv9FymusDPybGGUc
Qt5j5RcpNgEkn/5ZPyAlXjCfjz+RxChTfAyGHRmqU9qoLMIFir3pJ7llAoGBAMNH
sIxENwlzqyafoUUlEq/pU7kZWuJmrO2FwqRDraYoCiM/NCRhxRQ/ng6NY1gejepw
abD2alXiV4alBSxubne6rFmhvA00y2mG40c6Ezmxn2ZpbX3dMQ6bMcPKp7QnXtLc
mCSL4FGK02ImUNDsd0RVVFw51DRId4rmsuJYMK9NAoGAKlYdc4784ixTD2ZICIOC
ZWPxPAyQUEA7EkuUhAX1bVNG6UJTYA8kmGcUCG4jPTgWzi00IyUUr8jK7efyU/zs
qiJuVs1bia+flYIQpysMl1VzZh8gW1nkB4SVPm5l2wBvVJDIr9Mc6rueC/oVNkh2
fLVGuFoTVIu2bF0cWAjNNMg=
-----END TESTING KEY-----
"""u8.Raw());

internal static ж<ж<rsa.PrivateKey>> Ꮡtest2048Key = new StandardBox<ж<rsa.PrivateKey>>(parseKey(test2048KeyPEM));
internal static ref ж<rsa.PrivateKey> test2048Key => ref Ꮡtest2048Key.ValueSlot;

internal static ж<rsa.PrivateKey> test2048KeyOnlyD = Ꮡ(new PrivateKey(
    PublicKey: (~test2048Key).PublicKey,
    D: (~test2048Key).D
));

internal static ж<rsa.PrivateKey> test2048KeyWithoutPrecomputed = Ꮡ(new PrivateKey(
    PublicKey: (~test2048Key).PublicKey,
    D: (~test2048Key).D,
    Primes: (~test2048Key).Primes
));

// test2048KeyWithPrecomputed is different from test2048Key because it includes
// only the public precomputed values, and not the fips140/rsa.PrivateKey.
internal static ж<rsa.PrivateKey> test2048KeyWithPrecomputed = Ꮡ(new PrivateKey(
    PublicKey: (~test2048Key).PublicKey,
    D: (~test2048Key).D,
    Primes: (~test2048Key).Primes,
    Precomputed: new PrecomputedValues(
        Dp: (~test2048Key).Precomputed.Dp,
        Dq: (~test2048Key).Precomputed.Dq,
        Qinv: (~test2048Key).Precomputed.Qinv
    )
));

internal static ж<rsa.PrivateKey> test3072Key = parseKey(testingKey("""
-----BEGIN TESTING KEY-----
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
8h02sdAMB9Yc2A4EgzOySWoD
-----END TESTING KEY-----
"""u8.Raw()));

internal static ж<rsa.PrivateKey> test4096Key = parseKey(testingKey("""
-----BEGIN TESTING KEY-----
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
Axreud3rJ0rekUp6rI1joG717Wls
-----END TESTING KEY-----
"""u8.Raw()));

public static void BenchmarkDecryptPKCS1v15(ж<testing.B> Ꮡb) {
    Ꮡb.Run("2048"u8, (ж<testing.B> bΔ1) => {
        benchmarkDecryptPKCS1v15(bΔ1, test2048Key);
    });
    Ꮡb.Run("3072"u8, (ж<testing.B> bΔ2) => {
        benchmarkDecryptPKCS1v15(bΔ2, test3072Key);
    });
    Ꮡb.Run("4096"u8, (ж<testing.B> bΔ3) => {
        benchmarkDecryptPKCS1v15(bΔ3, test4096Key);
    });
}

internal static void benchmarkDecryptPKCS1v15(ж<testing.B> Ꮡb, ж<rsa.PrivateKey> Ꮡk) {
    ref var b = ref Ꮡb.DerefOrNull();

    var r = bufio.NewReaderSize(rand.Reader, 1 << 15);

    var m = slice<byte>("Hello Gophers"u8);
    var (c, err) = EncryptPKCS1v15(new rsa_test_package.bufio_ReaderжReader(r), Ꮡk.of(rsa.PrivateKey.ᏑPublicKey), m);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }

    b.ResetTimer();
    byte sink = default!;
    for (nint i = 0; i < b.N; i++) {
        var (p, errΔ1) = DecryptPKCS1v15(new rsa_test_package.bufio_ReaderжReader(r), Ꮡk, c);
        if (errΔ1 != default!) {
            Ꮡb.Fatal(errΔ1);
        }
        if (!bytes.Equal(p, m)) {
            Ꮡb.Fatalf("unexpected output: %q"u8, p);
        }
        sink ^= (byte)(p[0]);
    }
}

public static void BenchmarkEncryptPKCS1v15(ж<testing.B> Ꮡb) {
    Ꮡb.Run("2048"u8, (ж<testing.B> bΔ1) => {
        var r = bufio.NewReaderSize(rand.Reader, 1 << 15);
        var m = slice<byte>("Hello Gophers"u8);

        byte sink = default!;
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var (c, err) = EncryptPKCS1v15(new rsa_test_package.bufio_ReaderжReader(r), test2048Key.of(rsa.PrivateKey.ᏑPublicKey), m);
            if (err != default!) {
                bΔ1.Fatal(err);
            }
            sink ^= (byte)(c[0]);
        }
    });
}

public static void BenchmarkDecryptOAEP(ж<testing.B> Ꮡb) {
    Ꮡb.Run("2048"u8, (ж<testing.B> bΔ1) => {
        var r = bufio.NewReaderSize(rand.Reader, 1 << 15);

        var m = slice<byte>("Hello Gophers"u8);
        var (c, err) = EncryptOAEP(sha256.New(), new rsa_test_package.bufio_ReaderжReader(r), test2048Key.of(rsa.PrivateKey.ᏑPublicKey), m, default!);
        if (err != default!) {
            bΔ1.Fatal(err);
        }

        bΔ1.ResetTimer();
        byte sink = default!;
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var (p, errΔ1) = DecryptOAEP(sha256.New(), new rsa_test_package.bufio_ReaderжReader(r), test2048Key, c, default!);
            if (errΔ1 != default!) {
                bΔ1.Fatal(errΔ1);
            }
            if (!bytes.Equal(p, m)) {
                bΔ1.Fatalf("unexpected output: %q"u8, p);
            }
            sink ^= (byte)(p[0]);
        }
    });
}

public static void BenchmarkEncryptOAEP(ж<testing.B> Ꮡb) {
    Ꮡb.Run("2048"u8, (ж<testing.B> bΔ1) => {
        var r = bufio.NewReaderSize(rand.Reader, 1 << 15);
        var m = slice<byte>("Hello Gophers"u8);

        byte sink = default!;
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var (c, err) = EncryptOAEP(sha256.New(), new rsa_test_package.bufio_ReaderжReader(r), test2048Key.of(rsa.PrivateKey.ᏑPublicKey), m, default!);
            if (err != default!) {
                bΔ1.Fatal(err);
            }
            sink ^= (byte)(c[0]);
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noprecompOnlyDˢ = "2048/noprecomp/OnlyD"u8;
internal static readonly @string noprecompPrimesˢ = "2048/noprecomp/Primes"u8;
internal static readonly @string noprecompAllValuesˢ = "2048/noprecomp/AllValues"u8;

public static void BenchmarkSignPKCS1v15(ж<testing.B> Ꮡb) {
    Ꮡb.Run("2048"u8, (ж<testing.B> bΔ1) => {
        benchmarkSignPKCS1v15(bΔ1, test2048Key);
    });
    Ꮡb.Run(noprecompOnlyDˢ, (ж<testing.B> bΔ2) => {
        benchmarkSignPKCS1v15(bΔ2, test2048KeyOnlyD);
    });
    Ꮡb.Run(noprecompPrimesˢ, (ж<testing.B> bΔ3) => {
        benchmarkSignPKCS1v15(bΔ3, test2048KeyWithoutPrecomputed);
    });
    // This is different from "2048" because it's only the public precomputed
    // values, and not the crypto/internal/fips140/rsa.PrivateKey.
    Ꮡb.Run(noprecompAllValuesˢ, (ж<testing.B> bΔ4) => {
        benchmarkSignPKCS1v15(bΔ4, test2048KeyWithPrecomputed);
    });
}

internal static void benchmarkSignPKCS1v15(ж<testing.B> Ꮡb, ж<rsa.PrivateKey> Ꮡk) {
    ref var b = ref Ꮡb.DerefOrNull();

    var hashed = sha256.Sum256(slice<byte>("testing"u8));

    byte sink = default!;
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (s, err) = SignPKCS1v15(rand.Reader, Ꮡk, crypto.SHA256, hashed[..]);
        if (err != default!) {
            Ꮡb.Fatal(err);
        }
        sink ^= (byte)(s[0]);
    }
}

public static void BenchmarkVerifyPKCS1v15(ж<testing.B> Ꮡb) {
    Ꮡb.Run("2048"u8, (ж<testing.B> bΔ1) => {
        var hashed = sha256.Sum256(slice<byte>("testing"u8));
        var (s, err) = SignPKCS1v15(rand.Reader, test2048Key, crypto.SHA256, hashed[..]);
        if (err != default!) {
            bΔ1.Fatal(err);
        }

        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var errΔ1 = VerifyPKCS1v15(test2048Key.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hashed[..], s);
            if (errΔ1 != default!) {
                bΔ1.Fatal(errΔ1);
            }
        }
    });
}

public static void BenchmarkSignPSS(ж<testing.B> Ꮡb) {
    Ꮡb.Run("2048"u8, (ж<testing.B> bΔ1) => {
        var hashed = sha256.Sum256(slice<byte>("testing"u8));

        byte sink = default!;
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var (s, err) = SignPSS(rand.Reader, test2048Key, crypto.SHA256, hashed[..], nil);
            if (err != default!) {
                bΔ1.Fatal(err);
            }
            sink ^= (byte)(s[0]);
        }
    });
}

public static void BenchmarkVerifyPSS(ж<testing.B> Ꮡb) {
    Ꮡb.Run("2048"u8, (ж<testing.B> bΔ1) => {
        var hashed = sha256.Sum256(slice<byte>("testing"u8));
        var (s, err) = SignPSS(rand.Reader, test2048Key, crypto.SHA256, hashed[..], nil);
        if (err != default!) {
            bΔ1.Fatal(err);
        }

        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var errΔ1 = VerifyPSS(test2048Key.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA256, hashed[..], s, nil);
            if (errΔ1 != default!) {
                bΔ1.Fatal(errΔ1);
            }
        }
    });
}

public static void BenchmarkParsePKCS8PrivateKey(ж<testing.B> Ꮡb) {
    Ꮡb.Run("2048"u8, (ж<testing.B> bΔ1) => {
        var (p, _) = pem.Decode(slice<byte>(test2048KeyPEM));
        bΔ1.ResetTimer();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            {
                var (_, err) = Δx509.ParsePKCS8PrivateKey((~p).Bytes); if (err != default!) {
                    bΔ1.Fatal(err);
                }
            }
        }
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testdataKeygen2048Txtˢ = "testdata/keygen2048.txt"u8;

public static void BenchmarkGenerateKey(ж<testing.B> Ꮡb) {
    Ꮡb.Run("2048"u8, (ж<testing.B> bΔ1) => {
        var (primes, err) = os.ReadFile(testdataKeygen2048Txtˢ);
        if (err != default!) {
            bΔ1.Fatal(err);
        }
        while (bΔ1.Loop()) {
            var r = Ꮡ(new testPrimeReader(primes: ((@string)primes)));
            {
                var (_, errΔ1) = GenerateKey(new rsa_test_package.testPrimeReaderжReader(r), 2048); if (errΔ1 != default!) {
                    bΔ1.Fatal(errΔ1);
                }
            }
        }
    });
}

// testPrimeReader feeds prime candidates from a text file,
// one per line in hex, to GenerateKey.
partial struct testPrimeReader {
    internal @string primes;
}

internal static (nint n, error err) Read(this ref testPrimeReader r, slice<byte> p) {
    error err = default!;

    // Neutralize randutil.MaybeReadByte.
    //
    // DO NOT COPY this. We *will* break you. We can do this because we're
    // in the standard library, and can update this along with the
    // GenerateKey implementation if necessary.
    //
    // You have been warned.
    if (len(p) == 1) {
        return (1, default!);
    }

    @string line = default!;
    while (line == ""u8 || line[0] == (rune)'#') {
        bool ok = default!;
        (line, r.primes, ok) = strings.Cut(r.primes, "\n"u8);
        if (!ok) {
            return (0, io.EOF);
        }
    }
    (var b, err) = hex.DecodeString(line);
    if (err != default!) {
        return (0, err);
    }
    if (len(p) != len(b)) {
        return (0, fmt.Errorf("unexpected read length: %d"u8, len(p)));
    }
    copy(p, b);
    return (len(p), default!);
}

partial struct testEncryptOAEPMessage {
    internal slice<byte> @in;
    internal slice<byte> seed;
    internal slice<byte> @out;
}

partial struct testEncryptOAEPStruct {
    internal @string modulus;
    internal nint e;
    internal @string d;
    internal slice<testEncryptOAEPMessage> msgs;
}

public static void TestEncryptOAEP(ж<testing.T> Ꮡt) {
    var sha1Δ1 = sha1.New();
    var n = @new<bigꓸInt>();
    foreach (var (i, test) in testEncryptOAEPData) {
        n.SetString(test.modulus, 16);
        ref var @public = ref heap<rsa.PublicKey>(out var Ꮡpublic);
        @public = new PublicKey(N: n, E: test.e);

        foreach (var (j, message) in test.msgs) {
            var randomSource = bytes.NewReader(message.seed);
            var (@out, err) = EncryptOAEP(sha1Δ1, new rsa_test_package.bytes_ReaderжReader(randomSource), Ꮡpublic, message.@in, default!);
            if (err != default!) {
                Ꮡt.Errorf("#%d,%d error: %s"u8, i, j, err);
            }
            if (!bytes.Equal(@out, message.@out)) {
                Ꮡt.Errorf("#%d,%d bad result: %x (want %x)"u8, i, j, @out, message.@out);
            }
        }
    }
}

public static void TestDecryptOAEP(ж<testing.T> Ꮡt) {
    var random = rand.Reader;

    var sha1Δ1 = sha1.New();
    var n = @new<bigꓸInt>();
    var d = @new<bigꓸInt>();
    foreach (var (i, test) in testEncryptOAEPData) {
        n.SetString(test.modulus, 16);
        d.SetString(test.d, 16);
        var @private = @new<rsa.PrivateKey>();
        @private.Value.PublicKey = new PublicKey(N: n, E: test.e);
        @private.Value.D = d;

        foreach (var (j, message) in test.msgs) {
            var (@out, err) = DecryptOAEP(sha1Δ1, default!, @private, message.@out, default!);
            if (err != default!){
                Ꮡt.Errorf("#%d,%d error: %s"u8, i, j, err);
            } else 
            if (!bytes.Equal(@out, message.@in)) {
                Ꮡt.Errorf("#%d,%d bad result: %#v (want %#v)"u8, i, j, @out, message.@in);
            }

            // Decrypt with blinding.
            (@out, err) = DecryptOAEP(sha1Δ1, random, @private, message.@out, default!);
            if (err != default!){
                Ꮡt.Errorf("#%d,%d (blind) error: %s"u8, i, j, err);
            } else 
            if (!bytes.Equal(@out, message.@in)) {
                Ꮡt.Errorf("#%d,%d (blind) bad result: %#v (want %#v)"u8, i, j, @out, message.@in);
            }
        }
        if (testing.Short()) {
            break;
        }
    }
}

public static void Test2DecryptOAEP(ж<testing.T> Ꮡt) {
    var random = rand.Reader;

    var msg = new byte[]{0xed, 0x36, 0x90, 0x8d, 0xbe, 0xfc, 0x35, 0x40, 0x70, 0x4f, 0xf5, 0x9d, 0x6e, 0xc2, 0xeb, 0xf5, 0x27, 0xae, 0x65, 0xb0, 0x59, 0x29, 0x45, 0x25, 0x8c, 0xc1, 0x91, 0x22}.slice();
    var @in = new byte[]{0x72, 0x26, 0x84, 0xc9, 0xcf, 0xd6, 0xa8, 0x96, 0x04, 0x3e, 0x34, 0x07, 0x2c, 0x4f, 0xe6, 0x52, 0xbe, 0x46, 0x3c, 0xcf, 0x79, 0x21, 0x09, 0x64, 0xe7, 0x33, 0x66, 0x9b, 0xf8, 0x14, 0x22, 0x43, 0xfe, 0x8e, 0x52, 0x8b, 0xe0, 0x5f, 0x98, 0xef, 0x54, 0xac, 0x6b, 0xc6, 0x26, 0xac, 0x5b, 0x1b, 0x4b, 0x7d, 0x2e, 0xd7, 0x69, 0x28, 0x5a, 0x2f, 0x4a, 0x95, 0x89, 0x6c, 0xc7, 0x53, 0x95, 0xc7, 0xd2, 0x89, 0x04, 0x6f, 0x94, 0x74, 0x9b, 0x09, 0x0d, 0xf4, 0x61, 0x2e, 0xab, 0x48, 0x57, 0x4a, 0xbf, 0x95, 0xcb, 0xff, 0x15, 0xe2, 0xa0, 0x66, 0x58, 0xf7, 0x46, 0xf8, 0xc7, 0x0b, 0xb5, 0x1e, 0xa7, 0xba, 0x36, 0xce, 0xdd, 0x36, 0x41, 0x98, 0x6e, 0x10, 0xf9, 0x3b, 0x70, 0xbb, 0xa1, 0xda, 0x00, 0x40, 0xd5, 0xa5, 0x3f, 0x87, 0x64, 0x32, 0x7c, 0xbc, 0x50, 0x52, 0x0e, 0x4f, 0x21, 0xbd}.slice();

    var n = @new<bigꓸInt>();
    var d = @new<bigꓸInt>();
    n.SetString(testEncryptOAEPData[0].modulus, 16);
    d.SetString(testEncryptOAEPData[0].d, 16);
    var priv = @new<rsa.PrivateKey>();
    priv.Value.PublicKey = new PublicKey(N: n, E: testEncryptOAEPData[0].e);
    priv.Value.D = d;
    ref var sha1 = ref heap<crypto.Hash>(out var Ꮡsha1);
    sha1 = crypto.SHA1;
    ref var sha256 = ref heap<crypto.Hash>(out var Ꮡsha256);
    sha256 = crypto.SHA256;

    var (@out, err) = priv.Decrypt(random, @in, Ꮡ(new OAEPOptions(MGFHash: sha1, Hash: sha256)));

    if (err != default!){
        Ꮡt.Errorf("error: %s"u8, err);
    } else 
    if (!bytes.Equal(@out, msg)) {
        Ꮡt.Errorf("bad result %#v (want %#v)"u8, @out, msg);
    }
}

public static void TestEncryptDecryptOAEP(ж<testing.T> Ꮡt) {
    var sha256Δ1 = sha256.New();
    var n = @new<bigꓸInt>();
    var d = @new<bigꓸInt>();
    foreach (var (i, test) in testEncryptOAEPData) {
        n.SetString(test.modulus, 16);
        d.SetString(test.d, 16);
        var priv = @new<rsa.PrivateKey>();
        priv.Value.PublicKey = new PublicKey(N: n, E: test.e);
        priv.Value.D = d;

        foreach (var (j, message) in test.msgs) {
            var label = slice<byte>(fmt.Sprintf("hi#%d"u8, j));
            var (enc, err) = EncryptOAEP(sha256Δ1, rand.Reader, priv.of(rsa.PrivateKey.ᏑPublicKey), message.@in, label);
            if (err != default!) {
                Ꮡt.Errorf("#%d,%d: EncryptOAEP: %v"u8, i, j, err);
                continue;
            }
            (var dec, err) = DecryptOAEP(sha256Δ1, rand.Reader, priv, enc, label);
            if (err != default!) {
                Ꮡt.Errorf("#%d,%d: DecryptOAEP: %v"u8, i, j, err);
                continue;
            }
            if (!bytes.Equal(dec, message.@in)) {
                Ꮡt.Errorf("#%d,%d: round trip %q -> %q"u8, i, j, message.@in, dec);
            }

            // Using different hash for MGF.
            (enc, err) = EncryptOAEPWithOptions(rand.Reader, priv.of(rsa.PrivateKey.ᏑPublicKey), message.@in, Ꮡ(new OAEPOptions(Hash: crypto.SHA256, MGFHash: crypto.SHA1, Label: label)));
            if (err != default!) {
                Ꮡt.Errorf("#%d,%d: EncryptOAEP with different MGFHash: %v"u8, i, j, err);
                continue;
            }
            (dec, err) = priv.Decrypt(rand.Reader, enc, Ꮡ(new OAEPOptions(Hash: crypto.SHA256, MGFHash: crypto.SHA1, Label: label)));
            if (err != default!) {
                Ꮡt.Errorf("#%d,%d: DecryptOAEP with different MGFHash: %v"u8, i, j, err);
                continue;
            }
            if (!bytes.Equal(dec, message.@in)) {
                Ꮡt.Errorf("#%d,%d: round trip with different MGFHash %q -> %q"u8, i, j, message.@in, dec);
            }

            // Using a zero MGFHash.
            (enc, err) = EncryptOAEPWithOptions(rand.Reader, priv.of(rsa.PrivateKey.ᏑPublicKey), message.@in, Ꮡ(new OAEPOptions(Hash: crypto.SHA256, Label: label)));
            if (err != default!) {
                Ꮡt.Errorf("#%d,%d: EncryptOAEP with zero MGFHash: %v"u8, i, j, err);
                continue;
            }
            (dec, err) = DecryptOAEP(sha256Δ1, rand.Reader, priv, enc, label);
            if (err != default!) {
                Ꮡt.Errorf("#%d,%d: DecryptOAEP with zero MGFHash: %v"u8, i, j, err);
                continue;
            }
            if (!bytes.Equal(dec, message.@in)) {
                Ꮡt.Errorf("#%d,%d: round trip with zero MGFHash %q -> %q"u8, i, j, message.@in, dec);
            }
        }
    }
}

// testEncryptOAEPData contains a subset of the vectors from RSA's "Test vectors for RSA-OAEP".
internal static slice<testEncryptOAEPStruct> testEncryptOAEPData = new testEncryptOAEPStruct[]{
    // Key 1
    new("a8b3b284af8eb50b387034a860f146c4919f318763cd6c5598c8ae4811a1e0abc4c7e0b082d693a5e7fced675cf4668512772c0cbc64a742c6c630f533c8cc72f62ae833c40bf25842e984bb78bdbf97c0107d55bdb662f5c4e0fab9845cb5148ef7392dd3aaff93ae1e6b667bb3d4247616d4f5ba10d4cfd226de88d39f16fb"u8,
        65537,
        "53339cfdb79fc8466a655c7316aca85c55fd8f6dd898fdaf119517ef4f52e8fd8e258df93fee180fa0e4ab29693cd83b152a553d4ac4d1812b8b9fa5af0e7f55fe7304df41570926f3311f15c4d65a732c483116ee3d3d2d0af3549ad9bf7cbfb78ad884f84d5beb04724dc7369b31def37d0cf539e9cfcdd3de653729ead5d1"u8,
        new testEncryptOAEPMessage[]{
            // Example 1.1
            new(
                new byte[]{0x66, 0x28, 0x19, 0x4e, 0x12, 0x07, 0x3d, 0xb0,
                    0x3b, 0xa9, 0x4c, 0xda, 0x9e, 0xf9, 0x53, 0x23, 0x97,
                    0xd5, 0x0d, 0xba, 0x79, 0xb9, 0x87, 0x00, 0x4a, 0xfe,
                    0xfe, 0x34
                }.slice(),
                new byte[]{0x18, 0xb7, 0x76, 0xea, 0x21, 0x06, 0x9d, 0x69,
                    0x77, 0x6a, 0x33, 0xe9, 0x6b, 0xad, 0x48, 0xe1, 0xdd,
                    0xa0, 0xa5, 0xef
                }.slice(),
                new byte[]{0x35, 0x4f, 0xe6, 0x7b, 0x4a, 0x12, 0x6d, 0x5d,
                    0x35, 0xfe, 0x36, 0xc7, 0x77, 0x79, 0x1a, 0x3f, 0x7b,
                    0xa1, 0x3d, 0xef, 0x48, 0x4e, 0x2d, 0x39, 0x08, 0xaf,
                    0xf7, 0x22, 0xfa, 0xd4, 0x68, 0xfb, 0x21, 0x69, 0x6d,
                    0xe9, 0x5d, 0x0b, 0xe9, 0x11, 0xc2, 0xd3, 0x17, 0x4f,
                    0x8a, 0xfc, 0xc2, 0x01, 0x03, 0x5f, 0x7b, 0x6d, 0x8e,
                    0x69, 0x40, 0x2d, 0xe5, 0x45, 0x16, 0x18, 0xc2, 0x1a,
                    0x53, 0x5f, 0xa9, 0xd7, 0xbf, 0xc5, 0xb8, 0xdd, 0x9f,
                    0xc2, 0x43, 0xf8, 0xcf, 0x92, 0x7d, 0xb3, 0x13, 0x22,
                    0xd6, 0xe8, 0x81, 0xea, 0xa9, 0x1a, 0x99, 0x61, 0x70,
                    0xe6, 0x57, 0xa0, 0x5a, 0x26, 0x64, 0x26, 0xd9, 0x8c,
                    0x88, 0x00, 0x3f, 0x84, 0x77, 0xc1, 0x22, 0x70, 0x94,
                    0xa0, 0xd9, 0xfa, 0x1e, 0x8c, 0x40, 0x24, 0x30, 0x9c,
                    0xe1, 0xec, 0xcc, 0xb5, 0x21, 0x00, 0x35, 0xd4, 0x7a,
                    0xc7, 0x2e, 0x8a
                }.slice()
            ),
            // Example 1.2
            new(
                new byte[]{0x75, 0x0c, 0x40, 0x47, 0xf5, 0x47, 0xe8, 0xe4,
                    0x14, 0x11, 0x85, 0x65, 0x23, 0x29, 0x8a, 0xc9, 0xba,
                    0xe2, 0x45, 0xef, 0xaf, 0x13, 0x97, 0xfb, 0xe5, 0x6f,
                    0x9d, 0xd5
                }.slice(),
                new byte[]{0x0c, 0xc7, 0x42, 0xce, 0x4a, 0x9b, 0x7f, 0x32,
                    0xf9, 0x51, 0xbc, 0xb2, 0x51, 0xef, 0xd9, 0x25, 0xfe,
                    0x4f, 0xe3, 0x5f
                }.slice(),
                new byte[]{0x64, 0x0d, 0xb1, 0xac, 0xc5, 0x8e, 0x05, 0x68,
                    0xfe, 0x54, 0x07, 0xe5, 0xf9, 0xb7, 0x01, 0xdf, 0xf8,
                    0xc3, 0xc9, 0x1e, 0x71, 0x6c, 0x53, 0x6f, 0xc7, 0xfc,
                    0xec, 0x6c, 0xb5, 0xb7, 0x1c, 0x11, 0x65, 0x98, 0x8d,
                    0x4a, 0x27, 0x9e, 0x15, 0x77, 0xd7, 0x30, 0xfc, 0x7a,
                    0x29, 0x93, 0x2e, 0x3f, 0x00, 0xc8, 0x15, 0x15, 0x23,
                    0x6d, 0x8d, 0x8e, 0x31, 0x01, 0x7a, 0x7a, 0x09, 0xdf,
                    0x43, 0x52, 0xd9, 0x04, 0xcd, 0xeb, 0x79, 0xaa, 0x58,
                    0x3a, 0xdc, 0xc3, 0x1e, 0xa6, 0x98, 0xa4, 0xc0, 0x52,
                    0x83, 0xda, 0xba, 0x90, 0x89, 0xbe, 0x54, 0x91, 0xf6,
                    0x7c, 0x1a, 0x4e, 0xe4, 0x8d, 0xc7, 0x4b, 0xbb, 0xe6,
                    0x64, 0x3a, 0xef, 0x84, 0x66, 0x79, 0xb4, 0xcb, 0x39,
                    0x5a, 0x35, 0x2d, 0x5e, 0xd1, 0x15, 0x91, 0x2d, 0xf6,
                    0x96, 0xff, 0xe0, 0x70, 0x29, 0x32, 0x94, 0x6d, 0x71,
                    0x49, 0x2b, 0x44
                }.slice()
            ),
            // Example 1.3
            new(
                new byte[]{0xd9, 0x4a, 0xe0, 0x83, 0x2e, 0x64, 0x45, 0xce,
                    0x42, 0x33, 0x1c, 0xb0, 0x6d, 0x53, 0x1a, 0x82, 0xb1,
                    0xdb, 0x4b, 0xaa, 0xd3, 0x0f, 0x74, 0x6d, 0xc9, 0x16,
                    0xdf, 0x24, 0xd4, 0xe3, 0xc2, 0x45, 0x1f, 0xff, 0x59,
                    0xa6, 0x42, 0x3e, 0xb0, 0xe1, 0xd0, 0x2d, 0x4f, 0xe6,
                    0x46, 0xcf, 0x69, 0x9d, 0xfd, 0x81, 0x8c, 0x6e, 0x97,
                    0xb0, 0x51
                }.slice(),
                new byte[]{0x25, 0x14, 0xdf, 0x46, 0x95, 0x75, 0x5a, 0x67,
                    0xb2, 0x88, 0xea, 0xf4, 0x90, 0x5c, 0x36, 0xee, 0xc6,
                    0x6f, 0xd2, 0xfd
                }.slice(),
                new byte[]{0x42, 0x37, 0x36, 0xed, 0x03, 0x5f, 0x60, 0x26,
                    0xaf, 0x27, 0x6c, 0x35, 0xc0, 0xb3, 0x74, 0x1b, 0x36,
                    0x5e, 0x5f, 0x76, 0xca, 0x09, 0x1b, 0x4e, 0x8c, 0x29,
                    0xe2, 0xf0, 0xbe, 0xfe, 0xe6, 0x03, 0x59, 0x5a, 0xa8,
                    0x32, 0x2d, 0x60, 0x2d, 0x2e, 0x62, 0x5e, 0x95, 0xeb,
                    0x81, 0xb2, 0xf1, 0xc9, 0x72, 0x4e, 0x82, 0x2e, 0xca,
                    0x76, 0xdb, 0x86, 0x18, 0xcf, 0x09, 0xc5, 0x34, 0x35,
                    0x03, 0xa4, 0x36, 0x08, 0x35, 0xb5, 0x90, 0x3b, 0xc6,
                    0x37, 0xe3, 0x87, 0x9f, 0xb0, 0x5e, 0x0e, 0xf3, 0x26,
                    0x85, 0xd5, 0xae, 0xc5, 0x06, 0x7c, 0xd7, 0xcc, 0x96,
                    0xfe, 0x4b, 0x26, 0x70, 0xb6, 0xea, 0xc3, 0x06, 0x6b,
                    0x1f, 0xcf, 0x56, 0x86, 0xb6, 0x85, 0x89, 0xaa, 0xfb,
                    0x7d, 0x62, 0x9b, 0x02, 0xd8, 0xf8, 0x62, 0x5c, 0xa3,
                    0x83, 0x36, 0x24, 0xd4, 0x80, 0x0f, 0xb0, 0x81, 0xb1,
                    0xcf, 0x94, 0xeb
                }.slice()
            )
        }.slice()
    ),
    // Key 10
    new("ae45ed5601cec6b8cc05f803935c674ddbe0d75c4c09fd7951fc6b0caec313a8df39970c518bffba5ed68f3f0d7f22a4029d413f1ae07e4ebe9e4177ce23e7f5404b569e4ee1bdcf3c1fb03ef113802d4f855eb9b5134b5a7c8085adcae6fa2fa1417ec3763be171b0c62b760ede23c12ad92b980884c641f5a8fac26bdad4a03381a22fe1b754885094c82506d4019a535a286afeb271bb9ba592de18dcf600c2aeeae56e02f7cf79fc14cf3bdc7cd84febbbf950ca90304b2219a7aa063aefa2c3c1980e560cd64afe779585b6107657b957857efde6010988ab7de417fc88d8f384c4e6e72c3f943e0c31c0c4a5cc36f879d8a3ac9d7d59860eaada6b83bb"u8,
        65537,
        "056b04216fe5f354ac77250a4b6b0c8525a85c59b0bd80c56450a22d5f438e596a333aa875e291dd43f48cb88b9d5fc0d499f9fcd1c397f9afc070cd9e398c8d19e61db7c7410a6b2675dfbf5d345b804d201add502d5ce2dfcb091ce9997bbebe57306f383e4d588103f036f7e85d1934d152a323e4a8db451d6f4a5b1b0f102cc150e02feee2b88dea4ad4c1baccb24d84072d14e1d24a6771f7408ee30564fb86d4393a34bcf0b788501d193303f13a2284b001f0f649eaf79328d4ac5c430ab4414920a9460ed1b7bc40ec653e876d09abc509ae45b525190116a0c26101848298509c1c3bf3a483e7274054e15e97075036e989f60932807b5257751e79"u8,
        new testEncryptOAEPMessage[]{
            // Example 10.1
            new(
                new byte[]{0x8b, 0xba, 0x6b, 0xf8, 0x2a, 0x6c, 0x0f, 0x86,
                    0xd5, 0xf1, 0x75, 0x6e, 0x97, 0x95, 0x68, 0x70, 0xb0,
                    0x89, 0x53, 0xb0, 0x6b, 0x4e, 0xb2, 0x05, 0xbc, 0x16,
                    0x94, 0xee
                }.slice(),
                new byte[]{0x47, 0xe1, 0xab, 0x71, 0x19, 0xfe, 0xe5, 0x6c,
                    0x95, 0xee, 0x5e, 0xaa, 0xd8, 0x6f, 0x40, 0xd0, 0xaa,
                    0x63, 0xbd, 0x33
                }.slice(),
                new byte[]{0x53, 0xea, 0x5d, 0xc0, 0x8c, 0xd2, 0x60, 0xfb,
                    0x3b, 0x85, 0x85, 0x67, 0x28, 0x7f, 0xa9, 0x15, 0x52,
                    0xc3, 0x0b, 0x2f, 0xeb, 0xfb, 0xa2, 0x13, 0xf0, 0xae,
                    0x87, 0x70, 0x2d, 0x06, 0x8d, 0x19, 0xba, 0xb0, 0x7f,
                    0xe5, 0x74, 0x52, 0x3d, 0xfb, 0x42, 0x13, 0x9d, 0x68,
                    0xc3, 0xc5, 0xaf, 0xee, 0xe0, 0xbf, 0xe4, 0xcb, 0x79,
                    0x69, 0xcb, 0xf3, 0x82, 0xb8, 0x04, 0xd6, 0xe6, 0x13,
                    0x96, 0x14, 0x4e, 0x2d, 0x0e, 0x60, 0x74, 0x1f, 0x89,
                    0x93, 0xc3, 0x01, 0x4b, 0x58, 0xb9, 0xb1, 0x95, 0x7a,
                    0x8b, 0xab, 0xcd, 0x23, 0xaf, 0x85, 0x4f, 0x4c, 0x35,
                    0x6f, 0xb1, 0x66, 0x2a, 0xa7, 0x2b, 0xfc, 0xc7, 0xe5,
                    0x86, 0x55, 0x9d, 0xc4, 0x28, 0x0d, 0x16, 0x0c, 0x12,
                    0x67, 0x85, 0xa7, 0x23, 0xeb, 0xee, 0xbe, 0xff, 0x71,
                    0xf1, 0x15, 0x94, 0x44, 0x0a, 0xae, 0xf8, 0x7d, 0x10,
                    0x79, 0x3a, 0x87, 0x74, 0xa2, 0x39, 0xd4, 0xa0, 0x4c,
                    0x87, 0xfe, 0x14, 0x67, 0xb9, 0xda, 0xf8, 0x52, 0x08,
                    0xec, 0x6c, 0x72, 0x55, 0x79, 0x4a, 0x96, 0xcc, 0x29,
                    0x14, 0x2f, 0x9a, 0x8b, 0xd4, 0x18, 0xe3, 0xc1, 0xfd,
                    0x67, 0x34, 0x4b, 0x0c, 0xd0, 0x82, 0x9d, 0xf3, 0xb2,
                    0xbe, 0xc6, 0x02, 0x53, 0x19, 0x62, 0x93, 0xc6, 0xb3,
                    0x4d, 0x3f, 0x75, 0xd3, 0x2f, 0x21, 0x3d, 0xd4, 0x5c,
                    0x62, 0x73, 0xd5, 0x05, 0xad, 0xf4, 0xcc, 0xed, 0x10,
                    0x57, 0xcb, 0x75, 0x8f, 0xc2, 0x6a, 0xee, 0xfa, 0x44,
                    0x12, 0x55, 0xed, 0x4e, 0x64, 0xc1, 0x99, 0xee, 0x07,
                    0x5e, 0x7f, 0x16, 0x64, 0x61, 0x82, 0xfd, 0xb4, 0x64,
                    0x73, 0x9b, 0x68, 0xab, 0x5d, 0xaf, 0xf0, 0xe6, 0x3e,
                    0x95, 0x52, 0x01, 0x68, 0x24, 0xf0, 0x54, 0xbf, 0x4d,
                    0x3c, 0x8c, 0x90, 0xa9, 0x7b, 0xb6, 0xb6, 0x55, 0x32,
                    0x84, 0xeb, 0x42, 0x9f, 0xcc
                }.slice()
            )
        }.slice()
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string beginRsaTestingKeyˢ2 = """
-----BEGIN RSA TESTING KEY-----
MIIBOgIBAAJBAKj34GkxFhD90vcNLYLInFEX6Ppy1tPf9Cnzj4p4WGeKLs1Pt8Qu
KUpRKfFLfRYC9AIKjbJTWit+CqvjWYzvQwECAwEAAQJAIJLixBy2qpFoS4DSmoEm
o3qGy0t6z09AIJtH+5OeRV1be+N4cDYJKffGzDa88vQENZiRm0GRq6a+HPGQMd2k
TQIhAKMSvzIBnni7ot/OSie2TmJLY4SwTQAevXysE2RbFDYdAiEBCUEaRQnMnbp7
9mxDXDf6AU0cN/RPBjb9qSHDcWZHGzUCIG2Es59z8ugGrDY+pxLQnwfotadxd+Uy
v/Ow5T0q5gIJAiEAyS4RaI9YG8EWx/2w0T67ZUVAw8eOMB6BIUg0Xcu+3okCIBOs
/5OiPgoTdSy7bcF9IGpSE8ZgGKzgYQVZeN97YE00
-----END RSA TESTING KEY-----
"""u8.Raw();
internal static readonly object boringCryptoModeReturnsˢ = (@string)"BoringCrypto mode returns the wrong error from SignPSS"u8;

public static void TestPSmallerThanQ(ж<testing.T> Ꮡt) {
    // This key has a 256-bit P and a 257-bit Q.
    var k = parseKey(testingKey(beginRsaTestingKeyˢ2));
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    if (boring.Enabled) {
        Ꮡt.Skip(boringCryptoModeReturnsˢ);
    }
    testEverything(Ꮡt, k);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string beginTestingKeyˢ = """
-----BEGIN TESTING KEY-----
MIICmAIBADANBgkqhkiG9w0BAQEFAASCAoIwggJ+AgEAAoGBAOB/V7qbbMLHZSHS
rU3FLNQJe88wQr5asy813wqlWsCeYUn7Imxv23vDXthpkH/54+CplWDvVri24zhU
4tHfONSEBWWKTRfQKCW+vrzf+d02rB95lVBrBDSKAUR6w1Xcx9/6ib+kQRDnMl2l
WZzDgv8jrNPrLGipYBOLcI9/Oh3HAgMBAAECgYBr85AiAX8JIoy0+POxA/GMfIr2
lERj+IVVXFhGbED5gjUBUn8kz/gOrClZAqgxJKVbdTcxn4KGOM64z427Y24H52zQ
sCq7RFJ9KDd4s1hAPQImBRUYu2blqDoqxNBQBxLHVUN7vwFp2MGsHzTz7mcx7QNG
teRbyLhCanUd3UOb4QJhAP5dyjIK1WzKBZ/jSAmjJL64bks4wEEl5eG6e2cTscCH
RE/OSpHi57dyrxgnBkczt56hOksJFzwmgk4wEM8n/JDOXwIzvAH4w4JWhu169gCW
8LgxCzJ71xv8+wUUouUTLwIhAOHwcdEAKyLo3K7X1nlYcUOX61yQ1GXRgIROGrsh
NNjpAmEAuW1nu4k4QmEXLpJB7nyWic3q4T0SsatN5HrMAL1To/U3sDHDHIxbvNiG
mcXBBuDFp4cC9rY+0OOFtDfH2SveKzW1/uX11T4iT/6Bx9cORCnEe5GNBxVOH6IQ
34hGo1WTAiEAyCYALW1AyUQPerOpQwWeEIrb7Lw/65KTjqDB/VOFRUECYQCPcc07
D19OB593kGklAtk1XwGt1W8OmfGIKhGMKzlYhb9MezjaX/3zpO19msSUmSNKszMX
RpZX4LYz9Ity0nxQ3qZYN6XYNwvr7dCV0E5eS+mgbGWRrf3utdbUkZUNwYE=
-----END TESTING KEY-----
"""u8.Raw();
internal static readonly @string beginTestingKeyˢ2 = """
-----BEGIN TESTING KEY-----
MIICVwIBADANBgkqhkiG9w0BAQEFAASCAkEwggI9AgEAAoGBAObSD1Q4cfUURAuY
RCCTDxv3TxK/VPJH/ees4eVkJHBkgErTXJsVb7df3Pyz8J5yVU7Y68osp1uRgJu1
E/v61L388oUQbpDlhpCzkpx2ZBfwx891JJBNgrRu/ZEDaWfXA4fx3iUDcA83NfY+
WWBlwwjhZG2jTQAgB0kz6fIhxODRAgMBAAECgYAJcO3i1WC25C5w1Xhy5yT68TuN
IiGWu+JbLa97NySE6tOHRvQk0QSTUGw8thsEoo3BAthlQtKHWLmvwPl1YNtEGE14
9gMlzoveiB10tMAJqmIaPoUWgQ2Wmzz+akYgr3zEloN+2ptVRYmboOWXGOHK4LJd
n5h7UvQNSqZyUvqogQIhAPOfMCgE3hvt4wA9cNUg3uQgUNnjr0ITiptNmgmoaaFB
AmEA8oxdQm/Uo1B5J2ebPj/e6mCi/wv3Ewq0CNE4Q1SiLmK1EKwyYj1pNBfrT2Vs
O5XsuFPC5V07iSjjfbaE8Q4zuKSmhVFe9aoAn8lwuuVBufGLFW7FD8PnhDZcqWsE
ksuRAiEA7sRa5y32Hbtlmquc9VV0/nJpq1NKRmFunE1PJh4IAMECYH0Q1ZHJWkqv
1xjzeoA5rPcLx2BdyhP+g+C8CRfmzw2+BgFH2V8ArXuYDdTNxmZfI0XUov1j+qv5
8nvDHn+xxAekltzNnXptI49A7qjgR+jaXM47ZM+BQ6LP6S3OqfgLkQIhAKbHdb9l
cHPGX1uUDRAU1xxtpVQ0OqXyEgqwz6y6hYRw
-----END TESTING KEY-----
"""u8.Raw();

public static void TestLargeSizeDifference(ж<testing.T> Ꮡt) {
    // This key has a 768-bit P and a 256-bit Q.
    var k1 = parseKey(testingKey(beginTestingKeyˢ));

    // This key has a 256-bit P and a 768-bit Q.
    var k2 = parseKey(testingKey(beginTestingKeyˢ2));

    if (boring.Enabled) {
        Ꮡt.Skip(boringCryptoModeReturnsˢ);
    }
    testEverything(Ꮡt, k1);
    testEverything(Ꮡt, k2);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string onlyDˢ = "OnlyD"u8;
internal static readonly @string primesˢ = "Primes"u8;
internal static readonly object precomputedValuesShouldˢ = (@string)"Precomputed values should not be nil after Precompute()"u8;
internal static readonly @string allValuesˢ = "AllValues"u8;

public static void TestNotPrecomputed(ж<testing.T> Ꮡt) {
    Ꮡt.Run(onlyDˢ, (ж<testing.T> tΔ1) => {
        testEverything(tΔ1, test2048KeyOnlyD);
        ref var k = ref heap<rsa.PrivateKey>(out var Ꮡk);
        k = test2048KeyOnlyD.Value;
        k.Precompute();
        testEverything(tΔ1, Ꮡk);
    });
    Ꮡt.Run(primesˢ, (ж<testing.T> tΔ2) => {
        testEverything(tΔ2, test2048KeyWithoutPrecomputed);
        ref var k = ref heap<rsa.PrivateKey>(out var Ꮡk);
        k = test2048KeyWithoutPrecomputed.Value;
        k.Precompute();
        if (k.Precomputed.Dp == nil || k.Precomputed.Dq == nil || k.Precomputed.Qinv == nil) {
            tΔ2.Error(precomputedValuesShouldˢ);
        }
        testEverything(tΔ2, Ꮡk);
    });
    Ꮡt.Run(allValuesˢ, (ж<testing.T> tΔ3) => {
        testEverything(tΔ3, test2048KeyWithPrecomputed);
        ref var k = ref heap<rsa.PrivateKey>(out var Ꮡk);
        k = test2048KeyWithoutPrecomputed.Value;
        k.Precompute();
        if (k.Precomputed.Dp == nil || k.Precomputed.Dq == nil || k.Precomputed.Qinv == nil) {
            tΔ3.Error(precomputedValuesShouldˢ);
        }
        testEverything(tΔ3, Ꮡk);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object test512KeyShouldBeValidˢ = (@string)"test512Key should be valid"u8;
internal static readonly @string publicKeyMismatchˢ = "PublicKey mismatch"u8;
internal static readonly @string precomputedMismatchˢ = "Precomputed mismatch"u8;
internal static readonly object thisWasFixedAfterV100ˢ = (@string)"This was fixed after v1.0.0"u8;
internal static readonly @string dIsNilˢ = "D is nil"u8;
internal static readonly @string nIsNilˢ = "N is nil"u8;
internal static readonly @string pIsNilˢ = "P is nil"u8;
internal static readonly @string qIsNilˢ = "Q is nil"u8;

public static void TestModifiedPrivateKey(ж<testing.T> Ꮡt) {
    if (test512Key.Validate() != default!) {
        Ꮡt.Fatal(test512KeyShouldBeValidˢ);
    }

    Ꮡt.Run(publicKeyMismatchˢ, (ж<testing.T> tΔ1) => {
        testModifiedPrivateKey(tΔ1, (ж<rsa.PrivateKey> k) => {
            k.Value.PublicKey = test512KeyTwo.Value.PublicKey;
        });
    });
    Ꮡt.Run(precomputedMismatchˢ, (ж<testing.T> tΔ2) => {
        testModifiedPrivateKey(tΔ2, (ж<rsa.PrivateKey> k) => {
            k.Value.Precomputed = test512KeyTwo.Value.Precomputed;
        });
    });

    Ꮡt.Run("D+2"u8, (ж<testing.T> tΔ3) => {
        if (fips140.Version() == "v1.0.0"u8) {
            tΔ3.Skip(thisWasFixedAfterV100ˢ);
        }
        testModifiedPrivateKey(tΔ3, (ж<rsa.PrivateKey> k) => {
            k.Value.D = @new<bigꓸInt>().Add((~k).D, big.NewInt(2));
        });
    });
    Ꮡt.Run("D=0"u8, (ж<testing.T> tΔ4) => {
        testModifiedPrivateKey(tΔ4, (ж<rsa.PrivateKey> k) => {
            k.Value.D = @new<bigꓸInt>();
        });
    });
    Ꮡt.Run(dIsNilˢ, (ж<testing.T> tΔ5) => {
        testModifiedPrivateKey(tΔ5, (ж<rsa.PrivateKey> k) => {
            k.Value.D = default!;
        });
    });

    Ꮡt.Run("N+2"u8, (ж<testing.T> tΔ6) => {
        testModifiedPrivateKey(tΔ6, (ж<rsa.PrivateKey> k) => {
            k.Value.N = @new<bigꓸInt>().Add((~k).N, big.NewInt(2));
        });
    });
    Ꮡt.Run("N=0"u8, (ж<testing.T> tΔ7) => {
        testModifiedPrivateKey(tΔ7, (ж<rsa.PrivateKey> k) => {
            k.Value.N = @new<bigꓸInt>();
        });
    });
    Ꮡt.Run(nIsNilˢ, (ж<testing.T> tΔ8) => {
        testModifiedPrivateKey(tΔ8, (ж<rsa.PrivateKey> k) => {
            k.Value.N = default!;
        });
    });

    Ꮡt.Run("P+2"u8, (ж<testing.T> tΔ9) => {
        testModifiedPrivateKey(tΔ9, (ж<rsa.PrivateKey> k) => {
            k.Value.Primes[0] = @new<bigꓸInt>().Add((~k).Primes[0], big.NewInt(2));
        });
    });
    Ꮡt.Run("P=0"u8, (ж<testing.T> tΔ10) => {
        testModifiedPrivateKey(tΔ10, (ж<rsa.PrivateKey> k) => {
            k.Value.Primes[0] = @new<bigꓸInt>();
        });
    });
    Ꮡt.Run(pIsNilˢ, (ж<testing.T> tΔ11) => {
        testModifiedPrivateKey(tΔ11, (ж<rsa.PrivateKey> k) => {
            k.Value.Primes[0] = default!;
        });
    });

    Ꮡt.Run("Q+2"u8, (ж<testing.T> tΔ12) => {
        testModifiedPrivateKey(tΔ12, (ж<rsa.PrivateKey> k) => {
            k.Value.Primes[1] = @new<bigꓸInt>().Add((~k).Primes[1], big.NewInt(2));
        });
    });
    Ꮡt.Run("Q=0"u8, (ж<testing.T> tΔ13) => {
        testModifiedPrivateKey(tΔ13, (ж<rsa.PrivateKey> k) => {
            k.Value.Primes[1] = @new<bigꓸInt>();
        });
    });
    Ꮡt.Run(qIsNilˢ, (ж<testing.T> tΔ14) => {
        testModifiedPrivateKey(tΔ14, (ж<rsa.PrivateKey> k) => {
            k.Value.Primes[1] = default!;
        });
    });

    Ꮡt.Run("E+2"u8, (ж<testing.T> tΔ15) => {
        testModifiedPrivateKey(tΔ15, (ж<rsa.PrivateKey> k) => {
            k.Value.E += 2;
        });
    });
    Ꮡt.Run("E=0"u8, (ж<testing.T> tΔ16) => {
        testModifiedPrivateKey(tΔ16, (ж<rsa.PrivateKey> k) => {
            k.Value.E = 0;
        });
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object validateShouldHaveFailedˢ = (@string)"Validate should have failed"u8;
internal static readonly object validateShouldHaveFailedˢ2 = (@string)"Validate should have failed after Precompute()"u8;

internal static void testModifiedPrivateKey(ж<testing.T> Ꮡt, Action<ж<rsa.PrivateKey>> f) {
    var k = @new<rsa.PrivateKey>();
    k.Value = test512Key.Value;
    k.Value.Primes = slices.Clone<slice<ж<bigꓸInt>>, ж<bigꓸInt>>((~k).Primes);
    f(k);
    {
        var err = k.Validate(); if (err == default!) {
            Ꮡt.Error(validateShouldHaveFailedˢ);
        }
    }
    k.Precompute();
    {
        var err = k.Validate(); if (err == default!) {
            Ꮡt.Error(validateShouldHaveFailedˢ2);
        }
    }
}

} // end rsa_test_package
