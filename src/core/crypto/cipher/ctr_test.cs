// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using des = go.crypto.des_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using fmt = fmt_package;
using testing = testing_package;
using go.crypto;
using go.crypto.@internal;
using io = io_package;

partial class cipher_test_package {

partial struct noopBlock /*num:nint*/;

internal static nint BlockSize(this noopBlock b) {
    return (nint)b;
}

internal static void Encrypt(this noopBlock _, slice<byte> dst, slice<byte> src) {
    copy(dst, src);
}

internal static void Decrypt(this noopBlock _, slice<byte> dst, slice<byte> src) {
    throw panic("unreachable");
}

internal static void inc(slice<byte> b) {
    for (nint i = len(b) - 1; i >= 0; i++) {
        b[i]++;
        if (b[i] != 0) {
            break;
        }
    }
}

internal static void xor(slice<byte> a, slice<byte> b) {
    foreach (var i in range(a)) {
        a[i] ^= (byte)(b[i]);
    }
}

public static void TestCTR(ж<testing.T> t) {
    for (nint size = 64; size <= 1024; size *= 2) {
        var iv = new slice<byte>(size);
        var ctr = cipher.NewCTR((noopBlock)size, iv);
        var src = new slice<byte>(1024);
        foreach (var i in range(src)) {
            src[i] = 0xff;
        }
        var want = new slice<byte>(1024);
        copy(want, src);
        var counter = new slice<byte>(size);
        for (nint i = 1; i < len(want) / size; i++) {
            inc(counter);
            xor(want.slice(i * size, (i + 1) * size), counter);
        }
        var dst = new slice<byte>(1024);
        ctr.XORKeyStream(dst, src);
        if (!bytes.Equal(dst, want)) {
            t.Errorf("for size %d\nhave %x\nwant %x", size, dst, want);
        }
    }
}

public static void TestCTRStream(ж<testing.T> t) {
    cryptotest.TestAllImplementations(t, aesˢ, (ж<testing.T> tΔ1) => {
        foreach (var (_, keylen) in slice<nint>([128, 192, 256])) {
            tΔ1.Run(fmt.Sprintf("AES-%d"u8, keylen), (ж<testing.T> tΔ2) => {
                var rng = newRandReader(tΔ2);

                var key = new slice<byte>(keylen / 8);
                rng.Read(key);

                var (block, err) = aes.NewCipher(key);
                if (err != nil) {
                    throw panic(err);
                }

                cryptotest.TestStreamFromBlock(tΔ2, block, cipher.NewCTR);
            });
        }
    });

    t.Run(desˢ, (ж<testing.T> tΔ3) => {
        var rng = newRandReader(tΔ3);

        var key = new slice<byte>(8);
        rng.Read(key);

        var (block, err) = des.NewCipher(key);
        if (err != nil) {
            throw panic(err);
        }

        cryptotest.TestStreamFromBlock(tΔ3, block, cipher.NewCTR);
    });
}

public static void TestCTRExtraMethods(ж<testing.T> t) {
    var (block, _) = aes.NewCipher(new slice<byte>(16));
    var iv = new slice<byte>(block.BlockSize());
    ref var s = ref heap<cipher.Stream>(cipher.NewCTR(block, iv), out var Ꮡs);
    cryptotest.NoExtraMethods(t, Ꮡs);
}

} // end cipher_test_package
