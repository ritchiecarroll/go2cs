// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bufio = bufio_package;
using bytes = bytes_package;
using gzip = compress.gzip_package;
using crypto = crypto_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using rand = go.crypto.rand_package;
using sha512 = go.crypto.sha512_package;
using hex = encoding.hex_package;
using log = log_package;
using os = os_package;
using strings = strings_package;
using testing = testing_package;
using compress;
using encoding;
using go.crypto;
using go.crypto.@internal;
using io = io_package;
using static go.crypto.ed25519_package;

partial class ed25519_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object invalidSignatureˢ = (@string)"invalid signature"u8;

public static void Example_ed25519ctx() {
    var (pub, priv, err) = GenerateKey(default);
    if (err != nil) {
        log.Fatal(err);
    }

    var msg = slice<byte>("The quick brown fox jumps over the lazy dog"u8);

    (var sig, err) = priv.Sign(default, msg, new ed25519_test_package.ed25519_OptionsжSignerOpts(Ꮡ(new Options(
        Context: "Example_ed25519ctx"
    ))));
    if (err != nil) {
        log.Fatal(err);
    }

    var errΔ1 = VerifyWithOptions(pub, msg, sig, Ꮡ(new Options(
        Context: "Example_ed25519ctx"
    ))); if (errΔ1 != nil) {
        log.Fatal(invalidSignatureˢ);
    }
}

public static void TestGenerateKey(ж<testing.T> t) {
    // nil is like using crypto/rand.Reader.
    var (@public, @private, err) = GenerateKey(default);
    if (err != nil) {
        t.Fatal(err);
    }

    if (len(@public) != PublicKeySize) {
        t.Errorf("public key has the wrong size: %d", len(@public));
    }
    if (len(@private) != PrivateKeySize) {
        t.Errorf("private key has the wrong size: %d", len(@private));
    }
    if (!bytes.Equal(@private.Public()._<PublicKey>(), @public)) {
        t.Errorf("public key doesn't match private key");
    }
    var fromSeed = NewKeyFromSeed(@private.Seed());
    if (!bytes.Equal(@private, fromSeed)) {
        t.Errorf("recreating key pair from seed gave different private key");
    }

    (_, var k2, err) = GenerateKey(default);
    if (err != nil) {
        t.Fatal(err);
    }
    if (bytes.Equal(@private, k2)) {
        t.Errorf("GenerateKey returned the same private key twice");
    }

    (_, var k3, err) = GenerateKey(rand.Reader);
    if (err != nil) {
        t.Fatal(err);
    }
    if (bytes.Equal(@private, k3)) {
        t.Errorf("GenerateKey returned the same private key twice");
    }

    // GenerateKey is documented to be the same as NewKeyFromSeed.
    var seed = new slice<byte>(SeedSize);
    rand.Read(seed);
    (_, var k4, err) = GenerateKey(new ed25519_test_package.bytes_ReaderжReader(bytes.NewReader(seed)));
    if (err != nil) {
        t.Fatal(err);
    }
    var k4n = NewKeyFromSeed(seed);
    if (!bytes.Equal(k4, k4n)) {
        t.Errorf("GenerateKey with seed gave different private key");
    }
}

internal partial struct zeroReader {
}

internal static (nint, error) Read(this zeroReader _, slice<byte> buf) {
    clear(buf);
    return (len(buf), default);
}

public static void TestSignVerify(ж<testing.T> t) {
    zeroReader zero = default;
    var (@public, @private, _) = GenerateKey(zero);

    var message = slice<byte>("test message"u8);
    var sig = Sign(@private, message);
    if (!Verify(@public, message, sig)) {
        t.Errorf("valid signature rejected");
    }

    var wrongMessage = slice<byte>("wrong message"u8);
    if (Verify(@public, wrongMessage, sig)) {
        t.Errorf("signature of different message accepted");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object signatureDoesnTMatchTestˢ = (@string)"signature doesn't match test vector"u8;

public static void TestSignVerifyHashed(ж<testing.T> t) {
    // From RFC 8032, Section 7.3
    var (key, _) = hex.DecodeString("833fe62409237b9d62ec77587520911e9a759cec1d19755b7da901b96dca3d42ec172b93ad5e563bf4932c70e1245034c35467ef2efd4d64ebf819683467e2bf");
    var (expectedSig, _) = hex.DecodeString("98a70222f0b8121aa9d30f813d683f809e462b469c7ff87639499bb94e6dae4131f85042463c2a355a2003d062adf5aaa10b8c61e636062aaad11c2a26083406");
    var (message, _) = hex.DecodeString("616263");

    var @private = (global::go.crypto.ed25519_package.PrivateKey)key;
    var @public = @private.Public()._<PublicKey>();
    var hash = sha512.Sum512(message);
    var (sig, err) = @private.Sign(default, hash[..], crypto.SHA512);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(sig, expectedSig)) {
        t.Error(signatureDoesnTMatchTestˢ);
    }
    (sig, err) = @private.Sign(default, hash[..], new ed25519_test_package.ed25519_OptionsжSignerOpts(Ꮡ(new Options(Hash: crypto.SHA512))));
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(sig, expectedSig)) {
        t.Error(signatureDoesnTMatchTestˢ);
    }
    var errΔ1 = VerifyWithOptions(@public, hash[..], sig, Ꮡ(new Options(Hash: crypto.SHA512))); if (errΔ1 != nil) {
        t.Errorf("valid signature rejected: %v", errΔ1);
    }

    var errΔ2 = VerifyWithOptions(@public, hash[..], sig, Ꮡ(new Options(Hash: crypto.SHA256))); if (errΔ2 == nil) {
        t.Errorf("expected error for wrong hash");
    }

    var wrongHash = sha512.Sum512(slice<byte>("wrong message"u8));
    if (VerifyWithOptions(@public, wrongHash[..], sig, Ꮡ(new Options(Hash: crypto.SHA512))) == nil) {
        t.Errorf("signature of different message accepted");
    }

    sig[0] ^= (byte)(0xff);
    if (VerifyWithOptions(@public, hash[..], sig, Ꮡ(new Options(Hash: crypto.SHA512))) == nil) {
        t.Errorf("invalid signature accepted");
    }
    sig[0] ^= (byte)(0xff);
    sig[SignatureSize - 1] ^= (byte)(0xff);
    if (VerifyWithOptions(@public, hash[..], sig, Ꮡ(new Options(Hash: crypto.SHA512))) == nil) {
        t.Errorf("invalid signature accepted");
    }

    // The RFC provides no test vectors for Ed25519ph with context, so just sign
    // and verify something.
    (sig, err) = @private.Sign(default, hash[..], new ed25519_test_package.ed25519_OptionsжSignerOpts(Ꮡ(new Options(Hash: crypto.SHA512, Context: "123"))));
    if (err != nil) {
        t.Fatal(err);
    }
    var errΔ3 = VerifyWithOptions(@public, hash[..], sig, Ꮡ(new Options(Hash: crypto.SHA512, Context: "123"))); if (errΔ3 != nil) {
        t.Errorf("valid signature rejected: %v", errΔ3);
    }
    var errΔ4 = VerifyWithOptions(@public, hash[..], sig, Ꮡ(new Options(Hash: crypto.SHA512, Context: "321"))); if (errΔ4 == nil) {
        t.Errorf("expected error for wrong context");
    }
    var errΔ5 = VerifyWithOptions(@public, hash[..], sig, Ꮡ(new Options(Hash: crypto.SHA256, Context: "123"))); if (errΔ5 == nil) {
        t.Errorf("expected error for wrong hash");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string fooˢ => ref fooˢᵇ;
private static readonly @string fooˢᵇ = "foo"u8;

public static void TestSignVerifyContext(ж<testing.T> t) {
    // From RFC 8032, Section 7.2
    var (key, _) = hex.DecodeString("0305334e381af78f141cb666f6199f57bc3495335a256a95bd2a55bf546663f6dfc9425e4f968f7f0c29f0259cf5f9aed6851c2bb4ad8bfb860cfee0ab248292");
    var (expectedSig, _) = hex.DecodeString("55a4cc2f70a54e04288c5f4cd1e45a7bb520b36292911876cada7323198dd87a8b36950b95130022907a7fb7c4e9b2d5f6cca685a587b4b21f4b888e4e7edb0d");
    var (message, _) = hex.DecodeString("f726936d19c800494e3fdaff20b276a8");
    ref var context = ref heap<@string>(fooˢ, out var Ꮡcontext);

    var @private = (global::go.crypto.ed25519_package.PrivateKey)key;
    var @public = @private.Public()._<PublicKey>();
    var (sig, err) = @private.Sign(default, message, new ed25519_test_package.ed25519_OptionsжSignerOpts(Ꮡ(new Options(Context: context))));
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(sig, expectedSig)) {
        t.Error(signatureDoesnTMatchTestˢ);
    }
    var errΔ1 = VerifyWithOptions(@public, message, sig, Ꮡ(new Options(Context: context))); if (errΔ1 != nil) {
        t.Errorf("valid signature rejected: %v", errΔ1);
    }

    if (VerifyWithOptions(@public, slice<byte>("bar"u8), sig, Ꮡ(new Options(Context: context))) == nil) {
        t.Errorf("signature of different message accepted");
    }
    if (VerifyWithOptions(@public, message, sig, Ꮡ(new Options(Context: "bar"))) == nil) {
        t.Errorf("signature with different context accepted");
    }

    sig[0] ^= (byte)(0xff);
    if (VerifyWithOptions(@public, message, sig, Ꮡ(new Options(Context: context))) == nil) {
        t.Errorf("invalid signature accepted");
    }
    sig[0] ^= (byte)(0xff);
    sig[SignatureSize - 1] ^= (byte)(0xff);
    if (VerifyWithOptions(@public, message, sig, Ꮡ(new Options(Context: context))) == nil) {
        t.Errorf("invalid signature accepted");
    }
}

public static void TestCryptoSigner(ж<testing.T> t) {
    zeroReader zero = default;
    var (@public, @private, _) = GenerateKey(zero);

    var signer = ((crypto.Signer)new ed25519_test_package.ed25519_PrivateKeyᴠSigner(@private));

    var publicInterface = signer.Public();
    var (public2, ok) = publicInterface._<PublicKey>(ᐧ);
    if (!ok) {
        t.Fatalf("expected PublicKey from Public() but got %T", publicInterface);
    }

    if (!bytes.Equal(@public, public2)) {
        t.Errorf("public keys do not match: original:%x vs Public():%x", @public, public2);
    }

    var message = slice<byte>("message"u8);
    ref var noHash = ref heap(new crypto.Hash(), out var ᏑnoHash);
    var (signature, err) = signer.Sign(zero, message, noHash);
    if (err != nil) {
        t.Fatalf("error from Sign(): %s", err);
    }

    (var signature2, err) = signer.Sign(zero, message, new ed25519_test_package.ed25519_OptionsжSignerOpts(Ꮡ(new Options(Hash: noHash))));
    if (err != nil) {
        t.Fatalf("error from Sign(): %s", err);
    }
    if (!bytes.Equal(signature, signature2)) {
        t.Errorf("signatures keys do not match");
    }

    if (!Verify(@public, message, signature)) {
        t.Errorf("Verify failed on signature from Sign()");
    }
}

public static void TestEqual(ж<testing.T> t) {
    var (@public, @private, _) = GenerateKey(rand.Reader);

    if (!@public.Equal(@public)) {
        t.Errorf("public key is not equal to itself: %q", @public);
    }
    if (!@public.Equal(((crypto.Signer)new ed25519_test_package.ed25519_PrivateKeyᴠSigner(@private)).Public())) {
        t.Errorf("private.Public() is not Equal to public: %q", @public);
    }
    if (!@private.Equal(@private)) {
        t.Errorf("private key is not equal to itself: %q", @private);
    }

    var (otherPub, otherPriv, _) = GenerateKey(rand.Reader);
    if (@public.Equal(otherPub)) {
        t.Errorf("different public keys are Equal");
    }
    if (@private.Equal(otherPriv)) {
        t.Errorf("different private keys are Equal");
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testdataSignInputGzˢ => ref testdataSignInputGzˢᵇ;
private static readonly @string testdataSignInputGzˢᵇ = "testdata/sign.input.gz"u8;

public static void TestGolden(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // sign.input.gz is a selection of test cases from
        // https://ed25519.cr.yp.to/python/sign.input
        var (testDataZ, err) = os.Open(testdataSignInputGzˢ);
        if (err != nil) {
            t.Fatal(err);
        }
        var testDataZʗ1 = testDataZ;
        defer(() => testDataZʗ1.Close(), ref ᒐ);
        (var testData, err) = gzip.NewReader(new ed25519_test_package.os_FileжReader(testDataZ));
        if (err != nil) {
            t.Fatal(err);
        }
        var testDataʗ1 = testData;
        defer(() => testDataʗ1.Close(), ref ᒐ);

        var scanner = bufio.NewScanner(new ed25519_test_package.gzip_ReaderжReader(testData));
        nint lineNo = 0;

        while (scanner.Scan()) {
            lineNo++;

            @string line = scanner.Text();
            var parts = strings.Split(line, ":");
            if (len(parts) != 5) {
                t.Fatalf("bad number of parts on line %d", lineNo);
            }

            var (privBytes, _) = hex.DecodeString(parts[0]);
            var (pubKey, _) = hex.DecodeString(parts[1]);
            var (msg, _) = hex.DecodeString(parts[2]);
            var (sig, _) = hex.DecodeString(parts[3]);
            // The signatures in the test vectors also include the message
            // at the end, but we just want R and S.
            sig = sig[..(int)(SignatureSize)];

            nint l = len(pubKey); if (l != PublicKeySize) {
                t.Fatalf("bad public key length on line %d: got %d bytes", lineNo, l);
            }

            array<byte> priv = new(64); /* PrivateKeySize */
            copy(priv[..], privBytes);
            copy(priv[32..], pubKey);

            var sig2 = Sign(priv[..], msg);
            if (!bytes.Equal(sig, sig2[..])) {
                t.Errorf("different signature result on line %d: %x vs %x", lineNo, sig, sig2);
            }

            if (!Verify(pubKey, msg, sig2)) {
                t.Errorf("signature failed to verify on line %d", lineNo);
            }

            var priv2 = NewKeyFromSeed(priv[..32]);
            if (!bytes.Equal(priv[..], priv2)) {
                t.Errorf("recreating key pair gave different private key on line %d: %x vs %x", lineNo, priv[..], priv2);
            }

            var pubKey2 = priv2.Public()._<PublicKey>(); if (!bytes.Equal(pubKey, pubKey2)) {
                t.Errorf("recreating key pair gave different public key on line %d: %x vs %x", lineNo, pubKey, pubKey2);
            }

            var seed = priv2.Seed(); if (!bytes.Equal(priv[..32], seed)) {
                t.Errorf("recreating key pair gave different seed on line %d: %x vs %x", lineNo, priv[..32], seed);
            }
        }

        var errΔ1 = scanner.Err(); if (errΔ1 != nil) {
            t.Fatalf("error reading test data: %s", errΔ1);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nonCanonicalSignatureˢ = (@string)"non-canonical signature accepted"u8;

public static void TestMalleability(ж<testing.T> t) {
    // https://tools.ietf.org/html/rfc8032#section-5.1.7 adds an additional test
    // that s be in [0, order). This prevents someone from adding a multiple of
    // order to s and obtaining a second valid signature for the same message.
    var msg = slice<byte>([0x54, 0x65, 0x73, 0x74]);
    var sig = slice<byte>([
        0x7c, 0x38, 0xe0, 0x26, 0xf2, 0x9e, 0x14, 0xaa, 0xbd, 0x05, 0x9a,
        0x0f, 0x2d, 0xb8, 0xb0, 0xcd, 0x78, 0x30, 0x40, 0x60, 0x9a, 0x8b,
        0xe6, 0x84, 0xdb, 0x12, 0xf8, 0x2a, 0x27, 0x77, 0x4a, 0xb0, 0x67,
        0x65, 0x4b, 0xce, 0x38, 0x32, 0xc2, 0xd7, 0x6f, 0x8f, 0x6f, 0x5d,
        0xaf, 0xc0, 0x8d, 0x93, 0x39, 0xd4, 0xee, 0xf6, 0x76, 0x57, 0x33,
        0x36, 0xa5, 0xc5, 0x1e, 0xb6, 0xf9, 0x46, 0xb3, 0x1d
    ]);
    var publicKey = slice<byte>([
        0x7d, 0x4d, 0x0e, 0x7f, 0x61, 0x53, 0xa6, 0x9b, 0x62, 0x42, 0xb5,
        0x22, 0xab, 0xbe, 0xe6, 0x85, 0xfd, 0xa4, 0x42, 0x0f, 0x88, 0x34,
        0xb1, 0x08, 0xc3, 0xbd, 0xae, 0x36, 0x9e, 0xf5, 0x49, 0xfa
    ]);

    if (Verify(publicKey, msg, sig)) {
        t.Fatal(nonCanonicalSignatureˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object signatureDidnTVerifyˢ = (@string)"signature didn't verify"u8;

public static void TestAllocations(ж<testing.T> t) {
    cryptotest.SkipTestAllocations(t);
    var seed = new slice<byte>(SeedSize);
    var priv = NewKeyFromSeed(seed);
    {
        var privʗ1 = priv;
        var allocs = testing.AllocsPerRun(100, () => {
            var message = slice<byte>("Hello, world!"u8);
            var pub = privʗ1.Public()._<PublicKey>();
            var signature = Sign(privʗ1, message);
            if (!Verify(pub, message, signature)) {
                t.Fatal(signatureDidnTVerifyˢ);
            }
        }); if (allocs > 0D) {
            t.Errorf("expected zero allocations, got %0.1f", allocs);
        }
    }
}

public static void BenchmarkKeyGeneration(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    zeroReader zero = default;
    for (nint i = 0; i < b.N; i++) {
        var (_, _, err) = GenerateKey(zero); if (err != nil) {
            Ꮡb.Fatal(err);
        }
    }
}

public static void BenchmarkNewKeyFromSeed(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var seed = new slice<byte>(SeedSize);
    for (nint i = 0; i < b.N; i++) {
        _ = NewKeyFromSeed(seed);
    }
}

public static void BenchmarkSigning(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    zeroReader zero = default;
    var (_, priv, err) = GenerateKey(zero);
    if (err != nil) {
        Ꮡb.Fatal(err);
    }
    var message = slice<byte>("Hello, world!"u8);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Sign(priv, message);
    }
}

public static void BenchmarkVerification(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    zeroReader zero = default;
    var (pub, priv, err) = GenerateKey(zero);
    if (err != nil) {
        Ꮡb.Fatal(err);
    }
    var message = slice<byte>("Hello, world!"u8);
    var signature = Sign(priv, message);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Verify(pub, message, signature);
    }
}

} // end ed25519_internal_test_package
