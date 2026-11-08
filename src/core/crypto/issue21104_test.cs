// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using aes = global::go.crypto.aes_package;
using cipher = global::go.crypto.cipher_package;
using rc4 = global::go.crypto.rc4_package;
using testing = testing_package;
using global::go.crypto;

partial class crypto_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string rc4ˢ => ref rc4ˢᵇ;
private static readonly @string rc4ˢᵇ = "RC4"u8;

public static void TestRC4OutOfBoundsWrite(ж<testing.T> t) {
    // This cipherText is encrypted "0123456789"
    var cipherText = slice<byte>([238, 41, 187, 114, 151, 2, 107, 13, 178, 63]);
    var (cipher, err) = rc4.NewCipher([0]);
    if (err != nil) {
        throw panic(err);
    }
    test(t, rc4ˢ, cipherText, cipher.XORKeyStream);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string ctrˢ => ref ctrˢᵇ;
private static readonly @string ctrˢᵇ = "CTR"u8;

public static void TestCTROutOfBoundsWrite(ж<testing.T> t) {
    testBlock(t, ctrˢ, cipher.NewCTR);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string ofbˢ => ref ofbˢᵇ;
private static readonly @string ofbˢᵇ = "OFB"u8;

public static void TestOFBOutOfBoundsWrite(ж<testing.T> t) {
    testBlock(t, ofbˢ, cipher.NewOFB);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string cfbEncryptˢ => ref cfbEncryptˢᵇ;
private static readonly @string cfbEncryptˢᵇ = "CFB Encrypt"u8;

public static void TestCFBEncryptOutOfBoundsWrite(ж<testing.T> t) {
    testBlock(t, cfbEncryptˢ, cipher.NewCFBEncrypter);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string cfbDecryptˢ => ref cfbDecryptˢᵇ;
private static readonly @string cfbDecryptˢᵇ = "CFB Decrypt"u8;

public static void TestCFBDecryptOutOfBoundsWrite(ж<testing.T> t) {
    testBlock(t, cfbDecryptˢ, cipher.NewCFBDecrypter);
}
internal static void testBlock(ж<testing.T> t, @string name, Func<cipher.Block, slice<byte>, cipher.Stream> newCipher) {
    // This cipherText is encrypted "0123456789"
    var cipherText = slice<byte>([86, 216, 121, 231, 219, 191, 26, 12, 176, 117]);
    array<byte> iv = new(16);
    array<byte> key = new(16);
    var (block, err) = aes.NewCipher(key[..]);
    if (err != nil) {
        throw panic(err);
    }
    var stream = newCipher(block, iv[..]);
    test(t, name, cipherText, stream.XORKeyStream);
}
// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string abcdefghijˢ => ref abcdefghijˢᵇ;
private static readonly @string abcdefghijˢᵇ = "abcdefghij"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static ref readonly @string plainᶜ => ref plainᶜᵇ;
private static readonly @string plainᶜᵇ = "0123456789"u8;

internal static void test(ж<testing.T> t, @string name, slice<byte> cipherText, Action<slice<byte>, slice<byte>> xor) {
    GoFrame ᒐ = default;
    try {
        @string want = abcdefghijˢ;
        var plainText = slice<byte>(want);
        nint shorterLen = len(cipherText) / 2;
        var plainTextʗ1 = plainText;
        defer(() => {
            var err = recover();
            if (err == nil) {
                t.Errorf("%v XORKeyStream expected to panic on len(dst) < len(src), but didn't", name);
            }
            @string plain = plainᶜ;
            if (plainTextʗ1[shorterLen] == plain[shorterLen]) {
                t.Errorf("%v XORKeyStream did out of bounds write, want %v, got %v", name, want, (@string)plainTextʗ1);
            }
        }, ref ᒐ);
        xor(plainText.slice(0, shorterLen), cipherText);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end crypto_test_package
