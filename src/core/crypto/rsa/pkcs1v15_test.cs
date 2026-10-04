// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using crypto = crypto_package;
using rand = go.crypto.rand_package;
using static go.crypto.rsa_package;
using sha1 = go.crypto.sha1_package;
using sha256 = go.crypto.sha256_package;
using Δx509 = go.crypto.x509_package;
using base64 = encoding.base64_package;
using hex = encoding.hex_package;
using pem = encoding.pem_package;
using io = io_package;
using testing = testing_package;
using quick = go.testing.quick_package;
using encoding;
using go.crypto;
using go.testing;
using hash = hash_package;
using math;
using rsa = go.crypto.rsa_package;
using static go.crypto.rsa_internal_test_package;

partial class rsa_test_package {

internal static slice<byte> decodeBase64(@string @in) {
    var @out = new slice<byte>(base64.StdEncoding.DecodedLen(len(@in)));
    var (n, err) = base64.StdEncoding.Decode(@out, slice<byte>(@in));
    if (err != default!) {
        return default!;
    }
    return @out.slice(0, n);
}

[GoType] partial struct DecryptPKCS1v15Test {
    internal @string @in, @out;
}

// These test vectors were generated with `openssl rsautl -pkcs -encrypt`
internal static slice<DecryptPKCS1v15Test> decryptPKCS1v15Tests = new DecryptPKCS1v15Test[]{
    new(
        "gIcUIoVkD6ATMBk/u/nlCZCCWRKdkfjCgFdo35VpRXLduiKXhNz1XupLLzTXAybEq15juc+EgY5o0DHv/nt3yg=="u8,
        "x"u8
    ),
    new(
        "Y7TOCSqofGhkRb+jaVRLzK8xw2cSo1IVES19utzv6hwvx+M8kFsoWQm5DzBeJCZTCVDPkTpavUuEbgp8hnUGDw=="u8,
        "testing."u8
    ),
    new(
        "arReP9DJtEVyV2Dg3dDp4c/PSk1O6lxkoJ8HcFupoRorBZG+7+1fDAwT1olNddFnQMjmkb8vxwmNMoTAT/BFjQ=="u8,
        "testing.\n"u8
    ),
    new(
        "WtaBXIoGC54+vH0NH0CHHE+dRDOsMc/6BrfFu2lEqcKL9+uDuWaf+Xj9mrbQCjjZcpQuX733zyok/jsnqe/Ftw=="u8,
        "01234567890123456789012345678901234567890123456789012"u8
    )
}.slice();

public static void TestDecryptPKCS1v15(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    var decryptionFuncs = new Func<slice<byte>, (slice<byte>, error)>[]{
        (slice<byte> ciphertext) => {
            return DecryptPKCS1v15(default!, test512Key, ciphertext);
        },
        (slice<byte> ciphertext) => {
            return test512Key.Decrypt(default!, ciphertext, default!);
        }
    }.slice();
    foreach (var (_, decryptFunc) in decryptionFuncs) {
        foreach (var (i, test) in decryptPKCS1v15Tests) {
            var (@out, err) = decryptFunc(decodeBase64(test.@in));
            if (err != default!) {
                Ꮡt.Errorf("#%d error decrypting: %v"u8, i, err);
            }
            var want = slice<byte>(test.@out);
            if (!bytes.Equal(@out, want)) {
                Ꮡt.Errorf("#%d got:%#v want:%#v"u8, i, @out, want);
            }
        }
    }
}

public static void TestEncryptPKCS1v15(ж<testing.T> Ꮡt) {
    var random = rand.Reader;
    nint k = ((~rsaPrivateKey).N.BitLen() + 7) / 8;
    var randomʗ1 = random;
    var tryEncryptDecrypt = (slice<byte> @in, bool blind) => {
        if (len(@in) > k - 11) {
            @in = @in.slice(0, k - 11);
        }
        var (ciphertext, err) = EncryptPKCS1v15(randomʗ1, rsaPrivateKey.of(rsa.PrivateKey.ᏑPublicKey), @in);
        if (err != default!) {
            Ꮡt.Errorf("error encrypting: %s"u8, err);
            return false;
        }
        io.Reader randΔ1 = default!;
        if (!blind){
            randΔ1 = default!;
        } else {
            randΔ1 = randomʗ1;
        }
        (var plaintext, err) = DecryptPKCS1v15(randΔ1, rsaPrivateKey, ciphertext);
        if (err != default!) {
            Ꮡt.Errorf("error decrypting: %s"u8, err);
            return false;
        }
        if (!bytes.Equal(plaintext, @in)) {
            Ꮡt.Errorf("output mismatch: %#v %#v"u8, plaintext, @in);
            return false;
        }
        return true;
    };
    var config = @new<quick.Config>();
    if (testing.Short()) {
        config.Value.MaxCount = 10;
    }
    quick.Check((tryEncryptDecrypt).OrTypedNilFunc(), config);
}

// These test vectors were generated with `openssl rsautl -pkcs -encrypt`
internal static slice<DecryptPKCS1v15Test> decryptPKCS1v15SessionKeyTests = new DecryptPKCS1v15Test[]{
    new(
        "e6ukkae6Gykq0fKzYwULpZehX+UPXYzMoB5mHQUDEiclRbOTqas4Y0E6nwns1BBpdvEJcilhl5zsox/6DtGsYg=="u8,
        "1234"u8
    ),
    new(
        "Dtis4uk/q/LQGGqGk97P59K03hkCIVFMEFZRgVWOAAhxgYpCRG0MX2adptt92l67IqMki6iVQyyt0TtX3IdtEw=="u8,
        "FAIL"u8
    ),
    new(
        "LIyFyCYCptPxrvTxpol8F3M7ZivlMsf53zs0vHRAv+rDIh2YsHS69ePMoPMe3TkOMZ3NupiL3takPxIs1sK+dw=="u8,
        "abcd"u8
    ),
    new(
        "bafnobel46bKy76JzqU/RIVOH0uAYvzUtauKmIidKgM0sMlvobYVAVQPeUQ/oTGjbIZ1v/6Gyi5AO4DtHruGdw=="u8,
        "FAIL"u8
    )
}.slice();

public static void TestEncryptPKCS1v15SessionKey(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    foreach (var (i, test) in decryptPKCS1v15SessionKeyTests) {
        var key = slice<byte>("FAIL"u8);
        var err = DecryptPKCS1v15SessionKey(default!, test512Key, decodeBase64(test.@in), key);
        if (err != default!) {
            Ꮡt.Errorf("#%d error decrypting"u8, i);
        }
        var want = slice<byte>(test.@out);
        if (!bytes.Equal(key, want)) {
            Ꮡt.Errorf("#%d got:%#v want:%#v"u8, i, key, want);
        }
    }
}

public static void TestEncryptPKCS1v15DecrypterSessionKey(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    foreach (var (i, test) in decryptPKCS1v15SessionKeyTests) {
        var (plaintext, err) = test512Key.Decrypt(rand.Reader, decodeBase64(test.@in), Ꮡ(new PKCS1v15DecryptOptions(SessionKeyLen: 4)));
        if (err != default!) {
            Ꮡt.Fatalf("#%d: error decrypting: %s"u8, i, err);
        }
        if (len(plaintext) != 4) {
            Ꮡt.Fatalf("#%d: incorrect length plaintext: got %d, want 4"u8, i, len(plaintext));
        }
        if (test.@out != "FAIL"u8 && !bytes.Equal(plaintext, slice<byte>(test.@out))) {
            Ꮡt.Errorf("#%d: incorrect plaintext: got %x, want %x"u8, i, plaintext, test.@out);
        }
    }
}

public static void TestNonZeroRandomBytes(ж<testing.T> Ꮡt) {
    var random = rand.Reader;
    var b = new slice<byte>(512);
    var err = rsa_internal_test_package.NonZeroRandomBytes(b, random);
    if (err != default!) {
        Ꮡt.Errorf("returned error: %s"u8, err);
    }
    foreach (var (_, bΔ1) in b) {
        if (bΔ1 == 0) {
            Ꮡt.Errorf("Zero octet found"u8);
            return;
        }
    }
}

[GoType] partial struct signPKCS1v15Test {
    internal @string @in, @out;
}

// These vectors have been tested with
//
//	`openssl rsautl -verify -inkey pk -in signature | hexdump -C`
internal static slice<signPKCS1v15Test> signPKCS1v15Tests = new signPKCS1v15Test[]{
    new("Test.\n"u8, "a4f3fa6ea93bcdd0c57be020c1193ecbfd6f200a3d95c409769b029578fa0e336ad9a347600e40d3ae823b8c7e6bad88cc07c1d54c3a1523cbbb6d58efc362ae"u8)
}.slice();

public static void TestSignPKCS1v15(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    foreach (var (i, test) in signPKCS1v15Tests) {
        var h = sha1.New();
        h.Write(slice<byte>(test.@in));
        var digest = h.Sum(default!);
        var (s, err) = SignPKCS1v15(default!, test512Key, crypto.SHA1, digest);
        if (err != default!) {
            Ꮡt.Errorf("#%d %s"u8, i, err);
        }
        var (expected, _) = hex.DecodeString(test.@out);
        if (!bytes.Equal(s, expected)) {
            Ꮡt.Errorf("#%d got: %x want: %x"u8, i, s, expected);
        }
    }
}

public static void TestVerifyPKCS1v15(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    foreach (var (i, test) in signPKCS1v15Tests) {
        var h = sha1.New();
        h.Write(slice<byte>(test.@in));
        var digest = h.Sum(default!);
        var (sig, _) = hex.DecodeString(test.@out);
        var err = VerifyPKCS1v15(test512Key.of(rsa.PrivateKey.ᏑPublicKey), crypto.SHA1, digest, sig);
        if (err != default!) {
            Ꮡt.Errorf("#%d %s"u8, i, err);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rsaDecryptedAMessageThatˢ = (@string)"RSA decrypted a message that was too long."u8;

public static void TestOverlongMessagePKCS1v15(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    var ciphertext = decodeBase64("fjOVdirUzFoLlukv80dBllMLjXythIf22feqPrNo0YoIjzyzyoMFiLjAc/Y4krkeZ11XFThIrEvw\nkRiZcCq5ng=="u8);
    var (_, err) = DecryptPKCS1v15(default!, test512Key, ciphertext);
    if (err == default!) {
        Ꮡt.Error(rsaDecryptedAMessageThatˢ);
    }
}

public static void TestUnpaddedSignature(ж<testing.T> Ꮡt) {
    Ꮡt.Setenv(godebugˢ, rsa1024min0ˢ);
    var msg = slice<byte>("Thu Dec 19 18:06:16 EST 2013\n"u8);
    // This base64 value was generated with:
    // % echo Thu Dec 19 18:06:16 EST 2013 > /tmp/msg
    // % openssl rsautl -sign -inkey key -out /tmp/sig -in /tmp/msg
    //
    // Where "key" contains the RSA private key given at the bottom of this
    // file.
    var expectedSig = decodeBase64("pX4DR8azytjdQ1rtUiC040FjkepuQut5q2ZFX1pTjBrOVKNjgsCDyiJDGZTCNoh9qpXYbhl7iEym30BWWwuiZg=="u8);
    var (sig, err) = SignPKCS1v15(default!, test512Key, ((crypto.Hash)0), msg);
    if (err != default!) {
        Ꮡt.Fatalf("SignPKCS1v15 failed: %s"u8, err);
    }
    if (!bytes.Equal(sig, expectedSig)) {
        Ꮡt.Fatalf("signature is not expected value: got %x, want %x"u8, sig, expectedSig);
    }
    {
        var errΔ1 = VerifyPKCS1v15(test512Key.of(rsa.PrivateKey.ᏑPublicKey), ((crypto.Hash)0), msg, sig); if (errΔ1 != default!) {
            Ꮡt.Fatalf("signature failed to verify: %s"u8, errΔ1);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object keyWasModifiedWhenˢ = (@string)"key was modified when ciphertext was invalid"u8;

public static void TestShortSessionKey(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // This tests that attempting to decrypt a session key where the
    // ciphertext is too small doesn't run outside the array bounds.
    var (ciphertext, err) = EncryptPKCS1v15(rand.Reader, rsaPrivateKey.of(rsa.PrivateKey.ᏑPublicKey), new byte[]{1}.slice());
    if (err != default!) {
        Ꮡt.Fatalf("Failed to encrypt short message: %s"u8, err);
    }
    array<byte> key = new(32);
    {
        var errΔ1 = DecryptPKCS1v15SessionKey(default!, rsaPrivateKey, ciphertext, key[..]); if (errΔ1 != default!) {
            Ꮡt.Fatalf("Failed to decrypt short message: %s"u8, errΔ1);
        }
    }
    foreach (var (_, v) in key.ΔRangeSnapshot()) {
        if (v != 0) {
            Ꮡt.Fatal(keyWasModifiedWhenˢ);
        }
    }
}

internal static ж<rsa.PublicKey> parsePublicKey(@string s) {
    var (p, _) = pem.Decode(slice<byte>(s));
    var (k, err) = Δx509.ParsePKCS1PublicKey((~p).Bytes);
    if (err != default!) {
        throw panic(err);
    }
    return k;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string beginRsaPublicKeyˢ = """
-----BEGIN RSA PUBLIC KEY-----
MEgCQQCd9BVzo775lkohasxjnefF1nCMcNoibqIWEVDe/K7M2GSoO4zlSQB+gkix
O3AnTcdHB51iaZpWfxPSnew8yfulAgMBAAE=
-----END RSA PUBLIC KEY-----
"""u8;
internal static readonly object verifyPKCS1v15AcceptedAˢ = (@string)"VerifyPKCS1v15 accepted a truncated signature"u8;

public static void TestShortPKCS1v15Signature(ж<testing.T> Ꮡt) {
    var pub = parsePublicKey(beginRsaPublicKeyˢ);
    var (sig, err) = hex.DecodeString("193a310d0dcf64094c6e3a00c8219b80ded70535473acff72c08e1222974bb24a93a535b1dc4c59fc0e65775df7ba2007dd20e9193f4c4025a18a7070aee93"u8);
    if (err != default!) {
        Ꮡt.Fatalf("failed to decode signature: %s"u8, err);
    }
    var h = sha256.Sum256(slice<byte>("hello"u8));
    err = VerifyPKCS1v15(pub, crypto.SHA256, h[..], sig);
    if (err == default!) {
        Ꮡt.Fatal(verifyPKCS1v15AcceptedAˢ);
    }
}

} // end rsa_test_package
