// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using aes = go.crypto.aes_package;
using cipher = go.crypto.cipher_package;
using des = go.crypto.des_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using fmt = fmt_package;
using io = io_package;
using rand = go.math.rand_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.crypto;
using go.crypto.@internal;
using go.math;

partial class cipher_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string desˢ => ref desˢᵇ;
private static readonly @string desˢᵇ = "DES"u8;

// Test CBC Blockmode against the general cipher.BlockMode interface tester
public static void TestCBCBlockMode(ж<testing.T> t) {
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

                cryptotest.TestBlockMode(tΔ2, block, new Func<cipher.Block, slice<byte>, cipher.BlockMode>(cipher.NewCBCEncrypter), new Func<cipher.Block, slice<byte>, cipher.BlockMode>(cipher.NewCBCDecrypter));
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

        cryptotest.TestBlockMode(tΔ3, block, new Func<cipher.Block, slice<byte>, cipher.BlockMode>(cipher.NewCBCEncrypter), new Func<cipher.Block, slice<byte>, cipher.BlockMode>(cipher.NewCBCDecrypter));
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string setIVˢ => ref setIVˢᵇ;
private static readonly @string setIVˢᵇ = "SetIV"u8;

public static void TestCBCExtraMethods(ж<testing.T> t) {
    var (block, _) = aes.NewCipher(new slice<byte>(16));
    var iv = new slice<byte>(block.BlockSize());
    ref var s = ref heap<cipher.BlockMode>(cipher.NewCBCEncrypter(block, iv), out var Ꮡs);
    cryptotest.NoExtraMethods(t, Ꮡs, setIVˢ);

    s = cipher.NewCBCDecrypter(block, iv);
    cryptotest.NoExtraMethods(t, Ꮡs, setIVˢ);
}

internal static partial io.Reader newRandReader(ж<testing.T> t) {
    var seed = time.Now().UnixNano();
    t.Logf("Deterministic RNG seed: 0x%x", seed);
    return new rand_RandжReader(rand.New(rand.NewSource(seed)));
}

} // end cipher_test_package
