// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using strconv = strconv_package;
using testing = testing_package;
using go.crypto;

partial class cipher_test_package {

internal static void benchmarkAESGCMSeal(ж<testing.B> Ꮡb, slice<byte> buf, nint keySize) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    b.SetBytes((int64)len(buf));

    slice<byte> key = new slice<byte>(keySize);
    array<byte> nonce = new(12);
    array<byte> ad = new(13);
    var (aesΔ1, _) = aes.NewCipher(key[..]);
    var (aesgcm, _) = cipher.NewGCM(aesΔ1);
    slice<byte> @out = default!;

    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        @out = aesgcm.Seal(@out[..0], nonce[..], buf, ad[..]);
    }
}

internal static void benchmarkAESGCMOpen(ж<testing.B> Ꮡb, slice<byte> buf, nint keySize) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    b.SetBytes((int64)len(buf));

    slice<byte> key = new slice<byte>(keySize);
    array<byte> nonce = new(12);
    array<byte> ad = new(13);
    var (aesΔ1, _) = aes.NewCipher(key[..]);
    var (aesgcm, _) = cipher.NewGCM(aesΔ1);
    slice<byte> @out = default!;

    var ct = aesgcm.Seal(default!, nonce[..], buf[..], ad[..]);

    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        (@out, _) = aesgcm.Open(@out[..0], nonce[..], ct, ad[..]);
    }
}

public static void BenchmarkAESGCM(ж<testing.B> Ꮡb) {
    foreach (var (_, length) in new nint[]{64, 1350, 8 * 1024}.slice()) {
        Ꮡb.Run("Open-128-"u8 + strconv.Itoa(length), (ж<testing.B> bΔ1) => {
            benchmarkAESGCMOpen(bΔ1, new slice<byte>(length), 128 / 8);
        });
        Ꮡb.Run("Seal-128-"u8 + strconv.Itoa(length), (ж<testing.B> bΔ2) => {
            benchmarkAESGCMSeal(bΔ2, new slice<byte>(length), 128 / 8);
        });

        Ꮡb.Run("Open-256-"u8 + strconv.Itoa(length), (ж<testing.B> bΔ3) => {
            benchmarkAESGCMOpen(bΔ3, new slice<byte>(length), 256 / 8);
        });
        Ꮡb.Run("Seal-256-"u8 + strconv.Itoa(length), (ж<testing.B> bΔ4) => {
            benchmarkAESGCMSeal(bΔ4, new slice<byte>(length), 256 / 8);
        });
    }
}

internal static void benchmarkAESStream(ж<testing.B> Ꮡb, Func<cipher.Block, slice<byte>, cipher.Stream> mode, slice<byte> buf, nint keySize) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetBytes((int64)len(buf));

    var key = new slice<byte>(keySize);
    array<byte> iv = new(16);
    var (aesΔ1, _) = aes.NewCipher(key);
    var stream = mode(aesΔ1, iv[..]);

    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        stream.XORKeyStream(buf, buf);
    }
}

// If we test exactly 1K blocks, we would generate exact multiples of
// the cipher's block size, and the cipher stream fragments would
// always be wordsize aligned, whereas non-aligned is a more typical
// use-case.
internal static UntypedInt almost1K => /* 1024 - 5 */ 1019;

internal static UntypedInt almost8K => /* 8*1024 - 5 */ 8187;

public static void BenchmarkAESCTR(ж<testing.B> Ꮡb) {
    foreach (var (_, keyBits) in new nint[]{128, 192, 256}.slice()) {
        nint keySize = keyBits / 8;
        Ꮡb.Run(strconv.Itoa(keyBits), (ж<testing.B> bΔ1) => {
            bΔ1.Run("50"u8, (ж<testing.B> bΔ2) => {
                benchmarkAESStream(bΔ2, cipher.NewCTR, new slice<byte>(50), keySize);
            });
            bΔ1.Run("1K"u8, (ж<testing.B> bΔ3) => {
                benchmarkAESStream(bΔ3, cipher.NewCTR, new slice<byte>(almost1K), keySize);
            });
            bΔ1.Run("8K"u8, (ж<testing.B> bΔ4) => {
                benchmarkAESStream(bΔ4, cipher.NewCTR, new slice<byte>(almost8K), keySize);
            });
        });
    }
}

public static void BenchmarkAESCBCEncrypt1K(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(1024);
    b.SetBytes((int64)len(buf));

    array<byte> key = new(16);
    array<byte> iv = new(16);
    var (aesΔ1, _) = aes.NewCipher(key[..]);
    var cbc = cipher.NewCBCEncrypter(aesΔ1, iv[..]);
    for (nint i = 0; i < b.N; i++) {
        cbc.CryptBlocks(buf, buf);
    }
}

public static void BenchmarkAESCBCDecrypt1K(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var buf = new slice<byte>(1024);
    b.SetBytes((int64)len(buf));

    array<byte> key = new(16);
    array<byte> iv = new(16);
    var (aesΔ1, _) = aes.NewCipher(key[..]);
    var cbc = cipher.NewCBCDecrypter(aesΔ1, iv[..]);
    for (nint i = 0; i < b.N; i++) {
        cbc.CryptBlocks(buf, buf);
    }
}

} // end cipher_test_package
