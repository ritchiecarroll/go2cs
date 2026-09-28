// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using aes = crypto.aes_package;
using cipher = crypto.cipher_package;
using rc4 = crypto.rc4_package;
using testing = testing_package;
using crypto;

partial class crypto_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string rc4ˢ = "RC4"u8;

public static void TestRC4OutOfBoundsWrite(ж<testing.T> Ꮡt) {
    // This cipherText is encrypted "0123456789"
    var cipherText = new byte[]{238, 41, 187, 114, 151, 2, 107, 13, 178, 63}.slice();
    var (cipher, err) = rc4.NewCipher(new byte[]{0}.slice());
    if (err != default!) {
        throw panic(err);
    }
    test(Ꮡt, rc4ˢ, cipherText, cipher.XORKeyStream);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string ctrˢ = "CTR"u8;

public static void TestCTROutOfBoundsWrite(ж<testing.T> Ꮡt) {
    testBlock(Ꮡt, ctrˢ, cipher.NewCTR);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string ofbˢ = "OFB"u8;

public static void TestOFBOutOfBoundsWrite(ж<testing.T> Ꮡt) {
    testBlock(Ꮡt, ofbˢ, cipher.NewOFB);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cfbEncryptˢ = "CFB Encrypt"u8;

public static void TestCFBEncryptOutOfBoundsWrite(ж<testing.T> Ꮡt) {
    testBlock(Ꮡt, cfbEncryptˢ, cipher.NewCFBEncrypter);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string cfbDecryptˢ = "CFB Decrypt"u8;

public static void TestCFBDecryptOutOfBoundsWrite(ж<testing.T> Ꮡt) {
    testBlock(Ꮡt, cfbDecryptˢ, cipher.NewCFBDecrypter);
}

internal static void testBlock(ж<testing.T> Ꮡt, @string name, Func<cipher.Block, slice<byte>, cipher.Stream> newCipher) {
    // This cipherText is encrypted "0123456789"
    var cipherText = new byte[]{86, 216, 121, 231, 219, 191, 26, 12, 176, 117}.slice();
    array<byte> iv = new(16);
    array<byte> key = new(16);
    var (block, err) = aes.NewCipher(key[..]);
    if (err != default!) {
        throw panic(err);
    }
    var stream = newCipher(block, iv[..]);
    test(Ꮡt, name, cipherText, stream.XORKeyStream);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcdefghijˢ = "abcdefghij"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string plainᶜ = "0123456789"u8;

internal static void test(ж<testing.T> Ꮡt, @string name, slice<byte> cipherText, Action<slice<byte>, slice<byte>> xor) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        @string want = abcdefghijˢ;
        var plainText = slice<byte>(want);
        nint shorterLen = len(cipherText) / 2;
        var plainTextʗ1 = plainText;
        defer(() => {
            var err = recover();
            if (err == default!) {
                Ꮡt.Errorf("%v XORKeyStream expected to panic on len(dst) < len(src), but didn't"u8, name);
            }
            @string plain = plainᶜ;
            if (plainTextʗ1[shorterLen] == plain[shorterLen]) {
                Ꮡt.Errorf("%v XORKeyStream did out of bounds write, want %v, got %v"u8, name, want, ((@string)plainTextʗ1));
            }
        }, ref ᒐ);
        xor(plainText[..(int)(shorterLen)], cipherText);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end crypto_test_package
