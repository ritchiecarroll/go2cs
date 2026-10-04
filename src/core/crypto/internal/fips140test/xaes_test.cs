// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140test;

using bytes = bytes_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using aes = go.crypto.@internal.fips140.aes_package;
using gcm = go.crypto.@internal.fips140.aes.gcm_package;
using drbg = go.crypto.@internal.fips140.drbg_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using hex = encoding.hex_package;
using runtime = runtime_package;
using testing = testing_package;
using encoding;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using go.crypto.@internal.fips140.aes;

partial class fipstest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testReportsNonZeroˢ = (@string)"Test reports non-zero allocation count. See issue #70448"u8;

public static void TestXAESAllocations(ж<testing.T> Ꮡt) {
    if (runtime.GOARCH == "ppc64"u8 || runtime.GOARCH == "ppc64le"u8) {
        Ꮡt.Skip(testReportsNonZeroˢ);
    }
    cryptotest.SkipTestAllocations(Ꮡt);
    {
        var allocs = testing.AllocsPerRun(10, () => {
            var key = new slice<byte>(32);
            var nonce = new slice<byte>(24);
            var plaintext = new slice<byte>(16);
            var aad = new slice<byte>(16);
            var ciphertext = new slice<byte>(0, 16 + 16);
            ciphertext = xaesSeal(ciphertext, key, nonce, plaintext, aad);
            {
                var (_, err) = xaesOpen(plaintext[..0], key, nonce, ciphertext, aad); if (err != default!) {
                    Ꮡt.Fatal(err);
                }
            }
        }); if (allocs > 0D) {
            Ꮡt.Errorf("expected zero allocations, got %0.1f"u8, allocs);
        }
    }
}

public static void TestXAES(ж<testing.T> Ꮡt) {
    var key = bytes.Repeat(new byte[]{0x01}.slice(), 32);
    var plaintext = slice<byte>("XAES-256-GCM"u8);
    var additionalData = slice<byte>("c2sp.org/XAES-256-GCM"u8);
    var nonce = new slice<byte>(24);
    var ciphertext = new slice<byte>(len(plaintext) + 16);
    drbg.Read(nonce[..12]);
    var (c, _) = aes.New(key);
    var k = gcm.NewCounterKDF(c).DeriveKey(0x58, new array<byte>(nonce, 12));
    var (a, _) = aes.New(k[..]);
    var (g, _) = gcm.New(a, 12, 16);
    gcm.SealWithRandomNonce(g, nonce[12..], ciphertext, plaintext, additionalData);
    var (got, err) = xaesOpen(default!, key, nonce, ciphertext, additionalData);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(plaintext, got)) {
        Ꮡt.Errorf("plaintext and got are not equal"u8);
    }
}

// ACVP tests consider fixed data part of the output, not part of the input, and
// all the pre-generated vectors at
// https://github.com/usnistgov/ACVP-Server/blob/3a7333f6/gen-val/json-files/KDF-1.0/expectedResults.json
// have a 32-byte fixed data, while ours is always 14 bytes. Instead, test
// against the XAES-256-GCM vectors, which were tested against OpenSSL's Counter
// KDF. This also ensures the KDF will work for XAES-256-GCM.
internal static slice<byte> xaesSeal(slice<byte> dst, slice<byte> key, slice<byte> nonce, slice<byte> plaintext, slice<byte> additionalData) {
    var (c, _) = aes.New(key);
    var k = gcm.NewCounterKDF(c).DeriveKey(0x58, new array<byte>(nonce, 12));
    var n = nonce[12..];
    var (a, _) = aes.New(k[..]);
    var (g, _) = gcm.New(a, 12, 16);
    return g.Seal(dst, n, plaintext, additionalData);
}

internal static (slice<byte>, error) xaesOpen(slice<byte> dst, slice<byte> key, slice<byte> nonce, slice<byte> ciphertext, slice<byte> additionalData) {
    var (c, _) = aes.New(key);
    var k = gcm.NewCounterKDF(c).DeriveKey(0x58, new array<byte>(nonce, 12));
    var n = nonce[12..];
    var (a, _) = aes.New(k[..]);
    var (g, _) = gcm.New(a, 12, 16);
    return g.Open(dst, n, ciphertext, additionalData);
}

public static void TestXAESVectors(ж<testing.T> Ꮡt) {
    var key = bytes.Repeat(new byte[]{0x01}.slice(), 32);
    var nonce = slice<byte>("ABCDEFGHIJKLMNOPQRSTUVWX"u8);
    var plaintext = slice<byte>("XAES-256-GCM"u8);
    var ciphertext = xaesSeal(default!, key, nonce, plaintext, default!);
    @string expected = "ce546ef63c9cc60765923609b33a9a1974e96e52daf2fcf7075e2271"u8;
    {
        @string got = hex.EncodeToString(ciphertext); if (got != expected) {
            Ꮡt.Errorf("got: %s"u8, got);
        }
    }
    {
        var (decrypted, err) = xaesOpen(default!, key, nonce, ciphertext, default!); if (err != default!){
            Ꮡt.Fatal(err);
        } else 
        if (!bytes.Equal(plaintext, decrypted)) {
            Ꮡt.Errorf("plaintext and decrypted are not equal"u8);
        }
    }
    key = bytes.Repeat(new byte[]{0x03}.slice(), 32);
    var aad = slice<byte>("c2sp.org/XAES-256-GCM"u8);
    ciphertext = xaesSeal(default!, key, nonce, plaintext, aad);
    expected = "986ec1832593df5443a179437fd083bf3fdb41abd740a21f71eb769d"u8;
    {
        @string got = hex.EncodeToString(ciphertext); if (got != expected) {
            Ꮡt.Errorf("got: %s"u8, got);
        }
    }
    {
        var (decrypted, err) = xaesOpen(default!, key, nonce, ciphertext, aad); if (err != default!){
            Ꮡt.Fatal(err);
        } else 
        if (!bytes.Equal(plaintext, decrypted)) {
            Ꮡt.Errorf("plaintext and decrypted are not equal"u8);
        }
    }
}

public static void TestXAESAccumulated(ж<testing.T> Ꮡt) {
    nint iterations = 10_000;
    @string expected = "e6b9edf2df6cec60c8cbd864e2211b597fb69a529160cd040d56c0c210081939"u8;
    var (s, d) = (sha3.NewShake128(), sha3.NewShake128());
    for (nint i = 0; i < iterations; i++) {
        var key = new slice<byte>(32);
        s.Read(key);
        var nonce = new slice<byte>(24);
        s.Read(nonce);
        var lenByte = new slice<byte>(1);
        s.Read(lenByte);
        var plaintext = new slice<byte>((nint)lenByte[0]);
        s.Read(plaintext);
        s.Read(lenByte);
        var aad = new slice<byte>((nint)lenByte[0]);
        s.Read(aad);
        var ciphertext = xaesSeal(default!, key, nonce, plaintext, aad);
        var (decrypted, err) = xaesOpen(default!, key, nonce, ciphertext, aad);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!bytes.Equal(plaintext, decrypted)) {
            Ꮡt.Errorf("plaintext and decrypted are not equal"u8);
        }
        d.Write(ciphertext);
    }
    {
        @string got = hex.EncodeToString(d.Sum(default!)); if (got != expected) {
            Ꮡt.Errorf("got: %s"u8, got);
        }
    }
}

} // end fipstest_internal_test_package
