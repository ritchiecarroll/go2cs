// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using cryptotest = go.crypto.@internal.cryptotest_package;
using fmt = fmt_package;
using testing = testing_package;
using cipher = go.crypto.cipher_package;
using go.crypto;
using go.crypto.@internal;
using static go.crypto.aes_package;

partial class aes_internal_test_package {

// Test vectors are from FIPS 197:
//	https://csrc.nist.gov/publications/fips/fips197/fips-197.pdf

// Appendix B, C of FIPS 197: Cipher examples, Example vectors.
public partial struct CryptTest {
    internal slice<byte> key;
    internal slice<byte> @in;
    internal slice<byte> @out;
}

// Appendix B.
// Appendix C.1.  AES-128
// Appendix C.2.  AES-192
// Appendix C.3.  AES-256
internal static slice<CryptTest> encryptTests = new CryptTest[]{
    new(
        new byte[]{0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6, 0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c}.slice(),
        new byte[]{0x32, 0x43, 0xf6, 0xa8, 0x88, 0x5a, 0x30, 0x8d, 0x31, 0x31, 0x98, 0xa2, 0xe0, 0x37, 0x07, 0x34}.slice(),
        new byte[]{0x39, 0x25, 0x84, 0x1d, 0x02, 0xdc, 0x09, 0xfb, 0xdc, 0x11, 0x85, 0x97, 0x19, 0x6a, 0x0b, 0x32}.slice()
    ),
    new(
        new byte[]{0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f}.slice(),
        new byte[]{0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff}.slice(),
        new byte[]{0x69, 0xc4, 0xe0, 0xd8, 0x6a, 0x7b, 0x04, 0x30, 0xd8, 0xcd, 0xb7, 0x80, 0x70, 0xb4, 0xc5, 0x5a}.slice()
    ),
    new(
        new byte[]{0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17
        }.slice(),
        new byte[]{0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff}.slice(),
        new byte[]{0xdd, 0xa9, 0x7c, 0xa4, 0x86, 0x4c, 0xdf, 0xe0, 0x6e, 0xaf, 0x70, 0xa0, 0xec, 0x0d, 0x71, 0x91}.slice()
    ),
    new(
        new byte[]{0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f
        }.slice(),
        new byte[]{0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff}.slice(),
        new byte[]{0x8e, 0xa2, 0xb7, 0xca, 0x51, 0x67, 0x45, 0xbf, 0xea, 0xfc, 0x49, 0x90, 0x4b, 0x49, 0x60, 0x89}.slice()
    )
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aesˢ = "aes"u8;

// Test Cipher Encrypt method against FIPS 197 examples.
public static void TestCipherEncrypt(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, aesˢ, testCipherEncrypt);
}

internal static void testCipherEncrypt(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in encryptTests) {
        var (c, err) = NewCipher(tt.key);
        if (err != default!) {
            Ꮡt.Errorf("NewCipher(%d bytes) = %s"u8, len(tt.key), err);
            continue;
        }
        var @out = new slice<byte>(len(tt.@in));
        c.Encrypt(@out, tt.@in);
        foreach (var (j, v) in @out) {
            if (v != tt.@out[j]) {
                Ꮡt.Errorf("Cipher.Encrypt %d: out[%d] = %#x, want %#x"u8, i, j, v, tt.@out[j]);
                break;
            }
        }
    }
}

// Test Cipher Decrypt against FIPS 197 examples.
public static void TestCipherDecrypt(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, aesˢ, testCipherDecrypt);
}

internal static void testCipherDecrypt(ж<testing.T> Ꮡt) {
    foreach (var (i, tt) in encryptTests) {
        var (c, err) = NewCipher(tt.key);
        if (err != default!) {
            Ꮡt.Errorf("NewCipher(%d bytes) = %s"u8, len(tt.key), err);
            continue;
        }
        var plain = new slice<byte>(len(tt.@in));
        c.Decrypt(plain, tt.@out);
        foreach (var (j, v) in plain) {
            if (v != tt.@in[j]) {
                Ꮡt.Errorf("decryptBlock %d: plain[%d] = %#x, want %#x"u8, i, j, v, tt.@in[j]);
                break;
            }
        }
    }
}

// Test AES against the general cipher.Block interface tester
public static void TestAESBlock(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, aesˢ, testAESBlock);
}

internal static void testAESBlock(ж<testing.T> Ꮡt) {
    foreach (var (_, keylen) in new nint[]{128, 192, 256}.slice()) {
        Ꮡt.Run(fmt.Sprintf("AES-%d"u8, keylen), (ж<testing.T> tΔ1) => {
            cryptotest.TestBlock(tΔ1, keylen / 8, new Func<slice<byte>, (cipher.Block, error)>(NewCipher));
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aes128ˢ = "AES-128"u8;
internal static readonly @string aes192ˢ = "AES-192"u8;
internal static readonly @string aes256ˢ = "AES-256"u8;

public static void BenchmarkEncrypt(ж<testing.B> Ꮡb) {
    Ꮡb.Run(aes128ˢ, (ж<testing.B> bΔ1) => {
        benchmarkEncrypt(bΔ1, encryptTests[1]);
    });
    Ꮡb.Run(aes192ˢ, (ж<testing.B> bΔ2) => {
        benchmarkEncrypt(bΔ2, encryptTests[2]);
    });
    Ꮡb.Run(aes256ˢ, (ж<testing.B> bΔ3) => {
        benchmarkEncrypt(bΔ3, encryptTests[3]);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object newCipherˢ = (@string)"NewCipher:"u8;

internal static void benchmarkEncrypt(ж<testing.B> Ꮡb, CryptTest tt) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (c, err) = NewCipher(tt.key);
    if (err != default!) {
        Ꮡb.Fatal(newCipherˢ, err);
    }
    var @out = new slice<byte>(len(tt.@in));
    b.SetBytes((int64)len(@out));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        c.Encrypt(@out, tt.@in);
    }
}

public static void BenchmarkDecrypt(ж<testing.B> Ꮡb) {
    Ꮡb.Run(aes128ˢ, (ж<testing.B> bΔ1) => {
        benchmarkDecrypt(bΔ1, encryptTests[1]);
    });
    Ꮡb.Run(aes192ˢ, (ж<testing.B> bΔ2) => {
        benchmarkDecrypt(bΔ2, encryptTests[2]);
    });
    Ꮡb.Run(aes256ˢ, (ж<testing.B> bΔ3) => {
        benchmarkDecrypt(bΔ3, encryptTests[3]);
    });
}

internal static void benchmarkDecrypt(ж<testing.B> Ꮡb, CryptTest tt) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (c, err) = NewCipher(tt.key);
    if (err != default!) {
        Ꮡb.Fatal(newCipherˢ, err);
    }
    var @out = new slice<byte>(len(tt.@out));
    b.SetBytes((int64)len(@out));
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        c.Decrypt(@out, tt.@out);
    }
}

public static void BenchmarkCreateCipher(ж<testing.B> Ꮡb) {
    Ꮡb.Run(aes128ˢ, (ж<testing.B> bΔ1) => {
        benchmarkCreateCipher(bΔ1, encryptTests[1]);
    });
    Ꮡb.Run(aes192ˢ, (ж<testing.B> bΔ2) => {
        benchmarkCreateCipher(bΔ2, encryptTests[2]);
    });
    Ꮡb.Run(aes256ˢ, (ж<testing.B> bΔ3) => {
        benchmarkCreateCipher(bΔ3, encryptTests[3]);
    });
}

internal static void benchmarkCreateCipher(ж<testing.B> Ꮡb, CryptTest tt) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        {
            var (_, err) = NewCipher(tt.key); if (err != default!) {
                Ꮡb.Fatal(err);
            }
        }
    }
}

} // end aes_internal_test_package
