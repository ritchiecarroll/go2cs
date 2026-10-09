// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using des = go.crypto.des_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using rand = go.crypto.rand_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using testing = testing_package;
using encoding;
using go.crypto;
using go.crypto.@internal;
using io = io_package;

partial class cipher_test_package {

// cfbTests contains the test vectors from
// https://csrc.nist.gov/publications/nistpubs/800-38a/sp800-38a.pdf, section
// F.3.13.

partial struct cfbTestsᴛ1 /*dyn*/ {
    internal @string key, iv, plaintext, ciphertext;
}
internal static slice<cfbTestsᴛ1> cfbTests = new cfbTestsᴛ1[]{
    new(
        "2b7e151628aed2a6abf7158809cf4f3c"u8,
        "000102030405060708090a0b0c0d0e0f"u8,
        "6bc1bee22e409f96e93d7e117393172a"u8,
        "3b3fd92eb72dad20333449f8e83cfb4a"u8
    ),
    new(
        "2b7e151628aed2a6abf7158809cf4f3c"u8,
        "3B3FD92EB72DAD20333449F8E83CFB4A"u8,
        "ae2d8a571e03ac9c9eb76fac45af8e51"u8,
        "c8a64537a0b3a93fcde3cdad9f1ce58b"u8
    ),
    new(
        "2b7e151628aed2a6abf7158809cf4f3c"u8,
        "C8A64537A0B3A93FCDE3CDAD9F1CE58B"u8,
        "30c81c46a35ce411e5fbc1191a0a52ef"u8,
        "26751f67a3cbb140b1808cf187a4f4df"u8
    ),
    new(
        "2b7e151628aed2a6abf7158809cf4f3c"u8,
        "26751F67A3CBB140B1808CF187A4F4DF"u8,
        "f69f2445df4f9b17ad2b417be66c3710"u8,
        "c04b05357c5d1c0eeac4c66f9ff7f2e6"u8
    )
}.slice();

public static void TestCFBVectors(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in cfbTests) {
        var (key, err) = hex.DecodeString(test.key);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var iv, err) = hex.DecodeString(test.iv);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var plaintext, err) = hex.DecodeString(test.plaintext);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var expected, err) = hex.DecodeString(test.ciphertext);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        (var block, err) = aes.NewCipher(key);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var ciphertext = new slice<byte>(len(plaintext));
        var cfb = cipher.NewCFBEncrypter(block, iv);
        cfb.XORKeyStream(ciphertext, plaintext);
        if (!bytes.Equal(ciphertext, expected)) {
            Ꮡt.Errorf("#%d: wrong output: got %x, expected %x"u8, i, ciphertext, expected);
        }
        var cfbdec = cipher.NewCFBDecrypter(block, iv);
        var plaintextCopy = new slice<byte>(len(ciphertext));
        cfbdec.XORKeyStream(plaintextCopy, ciphertext);
        if (!bytes.Equal(plaintextCopy, plaintext)) {
            Ꮡt.Errorf("#%d: wrong plaintext: got %x, expected %x"u8, i, plaintextCopy, plaintext);
        }
    }
}

public static void TestCFBInverse(ж<testing.T> Ꮡt) {
    var (block, err) = aes.NewCipher(commonKey128);
    if (err != default!) {
        Ꮡt.Error(err);
        return;
    }
    var plaintext = slice<byte>("this is the plaintext. this is the plaintext."u8);
    var iv = new slice<byte>(block.BlockSize());
    rand.Reader.Read(iv);
    var cfb = cipher.NewCFBEncrypter(block, iv);
    var ciphertext = new slice<byte>(len(plaintext));
    copy(ciphertext, plaintext);
    cfb.XORKeyStream(ciphertext, ciphertext);
    var cfbdec = cipher.NewCFBDecrypter(block, iv);
    var plaintextCopy = new slice<byte>(len(plaintext));
    copy(plaintextCopy, ciphertext);
    cfbdec.XORKeyStream(plaintextCopy, plaintextCopy);
    if (!bytes.Equal(plaintextCopy, plaintext)) {
        Ꮡt.Errorf("got: %x, want: %x"u8, plaintextCopy, plaintext);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string encrypterˢ = "Encrypter"u8;
private static readonly @string decrypterˢ = "Decrypter"u8;

public static void TestCFBStream(ж<testing.T> Ꮡt) {
    foreach (var (_, keylen) in new nint[]{128, 192, 256}.slice()) {
        Ꮡt.Run(fmt.Sprintf("AES-%d"u8, keylen), (ж<testing.T> tΔ1) => {
            var rng = newRandReader(tΔ1);
            var key = new slice<byte>(keylen / 8);
            rng.Read(key);
            var (block, err) = aes.NewCipher(key);
            if (err != default!) {
                throw panic(err);
            }
            var blockʗ1 = block;
            tΔ1.Run(encrypterˢ, (ж<testing.T> tΔ2) => {
                cryptotest.TestStreamFromBlock(tΔ2, blockʗ1, cipher.NewCFBEncrypter);
            });
            var blockʗ2 = block;
            tΔ1.Run(decrypterˢ, (ж<testing.T> tΔ3) => {
                cryptotest.TestStreamFromBlock(tΔ3, blockʗ2, cipher.NewCFBDecrypter);
            });
        });
    }
    Ꮡt.Run(desˢ, (ж<testing.T> tΔ4) => {
        var rng = newRandReader(tΔ4);
        var key = new slice<byte>(8);
        rng.Read(key);
        var (block, err) = des.NewCipher(key);
        if (err != default!) {
            throw panic(err);
        }
        var blockʗ3 = block;
        tΔ4.Run(encrypterˢ, (ж<testing.T> tΔ5) => {
            cryptotest.TestStreamFromBlock(tΔ5, blockʗ3, cipher.NewCFBEncrypter);
        });
        var blockʗ4 = block;
        tΔ4.Run(decrypterˢ, (ж<testing.T> tΔ6) => {
            cryptotest.TestStreamFromBlock(tΔ6, blockʗ4, cipher.NewCFBDecrypter);
        });
    });
}

} // end cipher_test_package
