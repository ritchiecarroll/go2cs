// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// CBC AES test vectors.
// See U.S. National Institute of Standards and Technology (NIST)
// Special Publication 800-38A, ``Recommendation for Block Cipher
// Modes of Operation,'' 2001 Edition, pp. 24-29.
namespace go.crypto;

using bytes = bytes_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using testing = testing_package;
using go.crypto;
using go.crypto.@internal;

partial class cipher_test_package {

// NIST SP 800-38A pp 27-29

partial struct cbcAESTestsᴛ1 /*dyn*/ {
    internal @string name;
    internal slice<byte> key;
    internal slice<byte> iv;
    internal slice<byte> @in;
    internal slice<byte> @out;
}
internal static slice<cbcAESTestsᴛ1> cbcAESTests;
internal static void initᴛcbcAESTests() { cbcAESTests = new cbcAESTestsᴛ1[]{
    new(
        "CBC-AES128"u8,
        commonKey128,
        commonIV,
        commonInput,
        new byte[]{
            0x76, 0x49, 0xab, 0xac, 0x81, 0x19, 0xb2, 0x46, 0xce, 0xe9, 0x8e, 0x9b, 0x12, 0xe9, 0x19, 0x7d,
            0x50, 0x86, 0xcb, 0x9b, 0x50, 0x72, 0x19, 0xee, 0x95, 0xdb, 0x11, 0x3a, 0x91, 0x76, 0x78, 0xb2,
            0x73, 0xbe, 0xd6, 0xb8, 0xe3, 0xc1, 0x74, 0x3b, 0x71, 0x16, 0xe6, 0x9e, 0x22, 0x22, 0x95, 0x16,
            0x3f, 0xf1, 0xca, 0xa1, 0x68, 0x1f, 0xac, 0x09, 0x12, 0x0e, 0xca, 0x30, 0x75, 0x86, 0xe1, 0xa7
        }.slice()
    ),
    new(
        "CBC-AES192"u8,
        commonKey192,
        commonIV,
        commonInput,
        new byte[]{
            0x4f, 0x02, 0x1d, 0xb2, 0x43, 0xbc, 0x63, 0x3d, 0x71, 0x78, 0x18, 0x3a, 0x9f, 0xa0, 0x71, 0xe8,
            0xb4, 0xd9, 0xad, 0xa9, 0xad, 0x7d, 0xed, 0xf4, 0xe5, 0xe7, 0x38, 0x76, 0x3f, 0x69, 0x14, 0x5a,
            0x57, 0x1b, 0x24, 0x20, 0x12, 0xfb, 0x7a, 0xe0, 0x7f, 0xa9, 0xba, 0xac, 0x3d, 0xf1, 0x02, 0xe0,
            0x08, 0xb0, 0xe2, 0x79, 0x88, 0x59, 0x88, 0x81, 0xd9, 0x20, 0xa9, 0xe6, 0x4f, 0x56, 0x15, 0xcd
        }.slice()
    ),
    new(
        "CBC-AES256"u8,
        commonKey256,
        commonIV,
        commonInput,
        new byte[]{
            0xf5, 0x8c, 0x4c, 0x04, 0xd6, 0xe5, 0xf1, 0xba, 0x77, 0x9e, 0xab, 0xfb, 0x5f, 0x7b, 0xfb, 0xd6,
            0x9c, 0xfc, 0x4e, 0x96, 0x7e, 0xdb, 0x80, 0x8d, 0x67, 0x9f, 0x77, 0x7b, 0xc6, 0x70, 0x2c, 0x7d,
            0x39, 0xf2, 0x33, 0x69, 0xa9, 0xd9, 0xba, 0xcf, 0xa5, 0x30, 0xe2, 0x63, 0x04, 0x23, 0x14, 0x61,
            0xb2, 0xeb, 0x05, 0xe2, 0xc3, 0x9b, 0xe9, 0xfc, 0xda, 0x6c, 0x19, 0x07, 0x8c, 0x6a, 0x9d, 0x1b
        }.slice()
    )
}.slice(); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string aesˢ = "aes"u8;

public static void TestCBCEncrypterAES(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, aesˢ, testCBCEncrypterAES);
}

internal static void testCBCEncrypterAES(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in cbcAESTests) {
        var (c, err) = aes.NewCipher(test.key);
        if (err != default!) {
            Ꮡt.Errorf("%s: NewCipher(%d bytes) = %s"u8, test.name, len(test.key), err);
            continue;
        }
        var encrypter = cipher.NewCBCEncrypter(c, test.iv);
        var data = new slice<byte>(len(test.@in));
        copy(data, test.@in);
        encrypter.CryptBlocks(data, data);
        if (!bytes.Equal(test.@out, data)) {
            Ꮡt.Errorf("%s: CBCEncrypter\nhave %x\nwant %x"u8, test.name, data, test.@out);
        }
    }
}

public static void TestCBCDecrypterAES(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, aesˢ, testCBCDecrypterAES);
}

internal static void testCBCDecrypterAES(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in cbcAESTests) {
        var (c, err) = aes.NewCipher(test.key);
        if (err != default!) {
            Ꮡt.Errorf("%s: NewCipher(%d bytes) = %s"u8, test.name, len(test.key), err);
            continue;
        }
        var decrypter = cipher.NewCBCDecrypter(c, test.iv);
        var data = new slice<byte>(len(test.@out));
        copy(data, test.@out);
        decrypter.CryptBlocks(data, data);
        if (!bytes.Equal(test.@in, data)) {
            Ꮡt.Errorf("%s: CBCDecrypter\nhave %x\nwant %x"u8, test.name, data, test.@in);
        }
    }
}

} // end cipher_test_package
