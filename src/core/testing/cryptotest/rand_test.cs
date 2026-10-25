// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !fips140v1.0
namespace go.testing;

using bytes = bytes_package;
using ecdsa = crypto.ecdsa_package;
using elliptic = crypto.elliptic_package;
using mlkem = crypto.mlkem_package;
using rand = crypto.rand_package;
using hex = encoding.hex_package;
using testing = testing_package;
using big = math.big_package;
using crypto;
using crypto = crypto_package;
using encoding;
using io = io_package;
using math;
using static go.testing.cryptotest_package;

partial class cryptotest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string randReadˢ = "rand.Read"u8;
internal static readonly @string seed1ˢ = "seed 1"u8;
internal static readonly @string seed2ˢ = "seed 2"u8;
internal static readonly @string randReaderˢ = "rand.Reader"u8;
internal static readonly @string seed1Againˢ = "seed 1 again"u8;
internal static readonly @string mlkemGenerateKey768ˢ = "mlkem.GenerateKey768"u8;
internal static readonly @string ecdsaGenerateKeyˢ = "ecdsa.GenerateKey"u8;
internal static readonly @string cryptocustomrand1ˢ = "cryptocustomrand=1"u8;
internal static readonly @string godebugˢ = "GODEBUG"u8;

public static void TestSetGlobalRandom(ж<testing.T> Ꮡt) {
    var (seed1, _) = hex.DecodeString("6ae6783f4fbde91b6eb88b73a48ed247dbe5882e2579683432c1bfc525454add"u8 + "0cd87274d67084caaf0e0d36c8496db7fef55fe0e125750aa608d5e20ffc2d12"u8);

    var seed1ʗ1 = seed1;
    Ꮡt.Run(randReadˢ, (ж<testing.T> tΔ1) => {
        var buf = new slice<byte>(64);

        var bufʗ1 = buf;
        var seed1ʗ2 = seed1ʗ1;
        tΔ1.Run(seed1ˢ, (ж<testing.T> tΔ2) => {
            SetGlobalRandom(tΔ2, 1);
            rand.Read(bufʗ1);
            if (!bytes.Equal(bufʗ1, seed1ʗ2)) {
                tΔ2.Errorf("rand.Read with seed 1 = %x; want %x"u8, bufʗ1, seed1ʗ2);
            }

            rand.Read(bufʗ1);
            if (bytes.Equal(bufʗ1, seed1ʗ2)) {
                tΔ2.Errorf("rand.Read with seed 1 returned same output twice: %x"u8, bufʗ1);
            }

            SetGlobalRandom(tΔ2, 1);
            rand.Read(bufʗ1);
            if (!bytes.Equal(bufʗ1, seed1ʗ2)) {
                tΔ2.Errorf("rand.Read with seed 1 after reset = %x; want %x"u8, bufʗ1, seed1ʗ2);
            }

            SetGlobalRandom(tΔ2, 1);
        });

        rand.Read(buf);
        if (bytes.Equal(buf, seed1ʗ1)) {
            tΔ1.Errorf("rand.Read returned seeded output after test end"u8);
        }

        var bufʗ2 = buf;
        var seed1ʗ3 = seed1ʗ1;
        tΔ1.Run(seed2ˢ, (ж<testing.T> tΔ3) => {
            SetGlobalRandom(tΔ3, 2);
            rand.Read(bufʗ2);
            if (bytes.Equal(bufʗ2, seed1ʗ3)) {
                tΔ3.Errorf("rand.Read with seed 2 = %x; want different from %x"u8, bufʗ2, seed1ʗ3);
            }
        });
    });

    var seed1ʗ4 = seed1;
    Ꮡt.Run(randReaderˢ, (ж<testing.T> tΔ4) => {
        var buf = new slice<byte>(64);

        var bufʗ3 = buf;
        var seed1ʗ5 = seed1ʗ4;
        tΔ4.Run(seed1ˢ, (ж<testing.T> tΔ5) => {
            SetGlobalRandom(tΔ5, 1);
            rand.Reader.Read(bufʗ3);
            if (!bytes.Equal(bufʗ3, seed1ʗ5)) {
                tΔ5.Errorf("rand.Reader.Read with seed 1 = %x; want %x"u8, bufʗ3, seed1ʗ5);
            }

            SetGlobalRandom(tΔ5, 1);
        });

        rand.Reader.Read(buf);
        if (bytes.Equal(buf, seed1ʗ4)) {
            tΔ4.Errorf("rand.Reader.Read returned seeded output after test end"u8);
        }

        var oldReader = rand.Reader;
        var oldReaderʗ1 = oldReader;
        tΔ4.Cleanup(() => {
            rand.Reader = oldReaderʗ1;
        });
        rand.Reader = new cryptotest_internal_test_package.bytes_ReaderжReader(bytes.NewReader(bytes.Repeat(new byte[]{5}.slice(), 64)));

        var bufʗ4 = buf;
        var seed1ʗ6 = seed1ʗ4;
        tΔ4.Run(seed1Againˢ, (ж<testing.T> tΔ6) => {
            SetGlobalRandom(tΔ6, 1);
            rand.Reader.Read(bufʗ4);
            if (!bytes.Equal(bufʗ4, seed1ʗ6)) {
                tΔ6.Errorf("rand.Reader.Read with seed 1 = %x; want %x"u8, bufʗ4, seed1ʗ6);
            }
        });

        rand.Reader.Read(buf);
        if (!bytes.Equal(buf, bytes.Repeat(new byte[]{5}.slice(), 64))) {
            tΔ4.Errorf("rand.Reader not restored"u8);
        }
    });

    // A direct internal use of drbg.Read.
    var seed1ʗ7 = seed1;
    Ꮡt.Run(mlkemGenerateKey768ˢ, (ж<testing.T> tΔ7) => {
        var (exp, err) = mlkem.NewDecapsulationKey768(seed1ʗ7);
        if (err != default!) {
            tΔ7.Fatalf("mlkem.NewDecapsulationKey768: %v"u8, err);
        }

        SetGlobalRandom(tΔ7, 1);
        (var got, err) = mlkem.GenerateKey768();
        if (err != default!) {
            tΔ7.Fatalf("mlkem.GenerateKey768: %v"u8, err);
        }

        {
            var gotBytes = got.Bytes(); if (!bytes.Equal(gotBytes, exp.Bytes())) {
                tΔ7.Errorf("mlkem.GenerateKey768 with seed 1 = %x; want %x"u8, gotBytes, exp.Bytes());
            }
        }
    });

    // An ignored passed-in Reader.
    var seed1ʗ8 = seed1;
    Ꮡt.Run(ecdsaGenerateKeyˢ, (ж<testing.T> tΔ8) => {
        var (exp, err) = ecdsa.ParseRawPrivateKey(elliptic.P384(), seed1ʗ8[..48]);
        if (err != default!) {
            tΔ8.Fatalf("ecdsa.ParseRawPrivateKey: %v"u8, err);
        }

        SetGlobalRandom(tΔ8, 1);
        (var got, err) = ecdsa.GenerateKey(elliptic.P384(), new cryptotest_internal_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>("this reader is ignored"u8))));
        if (err != default!) {
            tΔ8.Fatalf("ecdsa.GenerateKey: %v"u8, err);
        }

        if (!got.Equal(exp.OrTypedNil())) {
            tΔ8.Errorf("ecdsa.GenerateKey with seed 1 = %x; want %x"u8, (~got).D.Bytes(), (~exp).D.Bytes());
        }
    });

    // The passed-in Reader is used if cryptocustomrand=1 is set,
    // and MaybeReadByte is called on it.
    Ꮡt.Run(cryptocustomrand1ˢ, (ж<testing.T> tΔ9) => {
        tΔ9.Setenv(godebugˢ, cryptocustomrand1ˢ);

        var buf = new slice<byte>(49);
        buf[0] = 42;
        for (nint i = 2; i < 49; i++) {
            buf[i] = 1;
        }

        var (exp1, err) = ecdsa.ParseRawPrivateKey(elliptic.P384(), buf[..48]);
        if (err != default!) {
            tΔ9.Fatalf("ecdsa.ParseRawPrivateKey: %v"u8, err);
        }
        (var exp2, err) = ecdsa.ParseRawPrivateKey(elliptic.P384(), buf[1..49]);
        if (err != default!) {
            tΔ9.Fatalf("ecdsa.ParseRawPrivateKey: %v"u8, err);
        }

        var seen = new bool[]{}.array(2);
        for (nint i = 0; i < 1000; i++) {
            var r = bytes.NewReader(buf);
            var (got, errΔ1) = ecdsa.GenerateKey(elliptic.P384(), new cryptotest_internal_test_package.bytes_ReaderжReader(r));
            if (errΔ1 != default!) {
                tΔ9.Fatalf("ecdsa.GenerateKey: %v"u8, errΔ1);
            }
            switch (ᐧ) {
            case {} when got.Equal(exp1.OrTypedNil()): {
                seen[0] = true;
                break;
            }
            case {} when got.Equal(exp2.OrTypedNil()): {
                seen[1] = true;
                break;
            }
            default: {
                tΔ9.Fatalf("ecdsa.GenerateKey with custom reader = %x; want %x or %x"u8, (~got).D.Bytes(), (~exp1).D.Bytes(), (~exp2).D.Bytes());
                break;
            }}
            if (seen[0] && seen[1]) {
                break;
            }
        }
        if (!seen[0] || !seen[1]) {
            tΔ9.Errorf("ecdsa.GenerateKey with custom reader did not produce both expected keys"u8);
        }

        // Again, with SetGlobalRandom.
        SetGlobalRandom(tΔ9, 1);

        seen = new bool[]{}.array(2);
        for (nint i = 0; i < 1000; i++) {
            var r = bytes.NewReader(buf);
            var (got, errΔ2) = ecdsa.GenerateKey(elliptic.P384(), new cryptotest_internal_test_package.bytes_ReaderжReader(r));
            if (errΔ2 != default!) {
                tΔ9.Fatalf("ecdsa.GenerateKey: %v"u8, errΔ2);
            }
            switch (ᐧ) {
            case {} when got.Equal(exp1.OrTypedNil()): {
                seen[0] = true;
                break;
            }
            case {} when got.Equal(exp2.OrTypedNil()): {
                seen[1] = true;
                break;
            }
            default: {
                tΔ9.Fatalf("ecdsa.GenerateKey with custom reader and SetGlobalRandom = %x; want %x or %x"u8, (~got).D.Bytes(), (~exp1).D.Bytes(), (~exp2).D.Bytes());
                break;
            }}
            if (seen[0] && seen[1]) {
                break;
            }
        }
        if (!seen[0] || !seen[1]) {
            tΔ9.Errorf("ecdsa.GenerateKey with custom reader and SetGlobalRandom did not produce both expected keys"u8);
        }
    });
}

} // end cryptotest_internal_test_package
