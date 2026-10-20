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
using rand = math.rand_package;
using testing = testing_package;
using time = time_package;
using go.crypto;
using go.crypto.@internal;
using math;

partial class cipher_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string desˢ = "DES"u8;

// Test CBC Blockmode against the general cipher.BlockMode interface tester
public static void TestCBCBlockMode(ж<testing.T> Ꮡt) {
    cryptotest.TestAllImplementations(Ꮡt, aesˢ, (ж<testing.T> tΔ1) => {
        foreach (var (_, keylen) in new nint[]{128, 192, 256}.slice()) {
            tΔ1.Run(fmt.Sprintf("AES-%d"u8, keylen), (ж<testing.T> tΔ2) => {
                var rng = newRandReader(tΔ2);

                var key = new slice<byte>(keylen / 8);
                rng.Read(key);

                var (block, err) = aes.NewCipher(key);
                if (err != default!) {
                    throw panic(err);
                }

                cryptotest.TestBlockMode(tΔ2, block, new Func<cipher.Block, slice<byte>, cipher.BlockMode>(cipher.NewCBCEncrypter), new Func<cipher.Block, slice<byte>, cipher.BlockMode>(cipher.NewCBCDecrypter));
            });
        }
    });

    Ꮡt.Run(desˢ, (ж<testing.T> tΔ3) => {
        var rng = newRandReader(tΔ3);

        var key = new slice<byte>(8);
        rng.Read(key);

        var (block, err) = des.NewCipher(key);
        if (err != default!) {
            throw panic(err);
        }

        cryptotest.TestBlockMode(tΔ3, block, new Func<cipher.Block, slice<byte>, cipher.BlockMode>(cipher.NewCBCEncrypter), new Func<cipher.Block, slice<byte>, cipher.BlockMode>(cipher.NewCBCDecrypter));
    });
}

internal static io.Reader newRandReader(ж<testing.T> Ꮡt) {
    var seed = time.Now().UnixNano();
    Ꮡt.Logf("Deterministic RNG seed: 0x%x"u8, seed);
    return new rand_RandжReader(rand.New(rand.NewSource(seed)));
}

} // end cipher_test_package
