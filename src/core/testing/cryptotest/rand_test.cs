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
internal static ref readonly @string randReadˢ => ref randReadˢᵇ;
private static readonly @string randReadˢᵇ = "rand.Read"u8;
internal static ref readonly @string seed1ˢ => ref seed1ˢᵇ;
private static readonly @string seed1ˢᵇ = "seed 1"u8;
internal static ref readonly @string seed2ˢ => ref seed2ˢᵇ;
private static readonly @string seed2ˢᵇ = "seed 2"u8;
internal static ref readonly @string randReaderˢ => ref randReaderˢᵇ;
private static readonly @string randReaderˢᵇ = "rand.Reader"u8;
internal static ref readonly @string seed1Againˢ => ref seed1Againˢᵇ;
private static readonly @string seed1Againˢᵇ = "seed 1 again"u8;
internal static ref readonly @string mlkemGenerateKey768ˢ => ref mlkemGenerateKey768ˢᵇ;
private static readonly @string mlkemGenerateKey768ˢᵇ = "mlkem.GenerateKey768"u8;
internal static ref readonly @string ecdsaGenerateKeyˢ => ref ecdsaGenerateKeyˢᵇ;
private static readonly @string ecdsaGenerateKeyˢᵇ = "ecdsa.GenerateKey"u8;
internal static ref readonly @string cryptocustomrand1ˢ => ref cryptocustomrand1ˢᵇ;
private static readonly @string cryptocustomrand1ˢᵇ = "cryptocustomrand=1"u8;
internal static ref readonly @string godebugˢ => ref godebugˢᵇ;
private static readonly @string godebugˢᵇ = "GODEBUG"u8;

public static void TestSetGlobalRandom(ж<testing.T> t) {
    var (seed1, _) = hex.DecodeString("6ae6783f4fbde91b6eb88b73a48ed247dbe5882e2579683432c1bfc525454add"u8 + "0cd87274d67084caaf0e0d36c8496db7fef55fe0e125750aa608d5e20ffc2d12"u8);

    var seed1ʗ1 = seed1;
    t.Run(randReadˢ, (ж<testing.T> tΔ1) => {
        var buf = new slice<byte>(64);

        var bufʗ1 = buf;
        var seed1ʗ2 = seed1ʗ1;
        tΔ1.Run(seed1ˢ, (ж<testing.T> tΔ2) => {
            SetGlobalRandom(tΔ2, 1);
            rand.Read(bufʗ1);
            if (!bytes.Equal(bufʗ1, seed1ʗ2)) {
                tΔ2.Errorf("rand.Read with seed 1 = %x; want %x", bufʗ1, seed1ʗ2);
            }

            rand.Read(bufʗ1);
            if (bytes.Equal(bufʗ1, seed1ʗ2)) {
                tΔ2.Errorf("rand.Read with seed 1 returned same output twice: %x", bufʗ1);
            }

            SetGlobalRandom(tΔ2, 1);
            rand.Read(bufʗ1);
            if (!bytes.Equal(bufʗ1, seed1ʗ2)) {
                tΔ2.Errorf("rand.Read with seed 1 after reset = %x; want %x", bufʗ1, seed1ʗ2);
            }

            SetGlobalRandom(tΔ2, 1);
        });

        rand.Read(buf);
        if (bytes.Equal(buf, seed1ʗ1)) {
            tΔ1.Errorf("rand.Read returned seeded output after test end");
        }

        var bufʗ2 = buf;
        var seed1ʗ3 = seed1ʗ1;
        tΔ1.Run(seed2ˢ, (ж<testing.T> tΔ3) => {
            SetGlobalRandom(tΔ3, 2);
            rand.Read(bufʗ2);
            if (bytes.Equal(bufʗ2, seed1ʗ3)) {
                tΔ3.Errorf("rand.Read with seed 2 = %x; want different from %x", bufʗ2, seed1ʗ3);
            }
        });
    });

    var seed1ʗ4 = seed1;
    t.Run(randReaderˢ, (ж<testing.T> tΔ4) => {
        var buf = new slice<byte>(64);

        var bufʗ3 = buf;
        var seed1ʗ5 = seed1ʗ4;
        tΔ4.Run(seed1ˢ, (ж<testing.T> tΔ5) => {
            SetGlobalRandom(tΔ5, 1);
            rand.Reader.Read(bufʗ3);
            if (!bytes.Equal(bufʗ3, seed1ʗ5)) {
                tΔ5.Errorf("rand.Reader.Read with seed 1 = %x; want %x", bufʗ3, seed1ʗ5);
            }

            SetGlobalRandom(tΔ5, 1);
        });

        rand.Reader.Read(buf);
        if (bytes.Equal(buf, seed1ʗ4)) {
            tΔ4.Errorf("rand.Reader.Read returned seeded output after test end");
        }

        var oldReader = rand.Reader;
        var oldReaderʗ1 = oldReader;
        tΔ4.Cleanup(() => {
            rand.Reader = oldReaderʗ1;
        });
        rand.Reader = new cryptotest_internal_test_package.bytes_ReaderжReader(bytes.NewReader(bytes.Repeat([5], 64)));

        var bufʗ4 = buf;
        var seed1ʗ6 = seed1ʗ4;
        tΔ4.Run(seed1Againˢ, (ж<testing.T> tΔ6) => {
            SetGlobalRandom(tΔ6, 1);
            rand.Reader.Read(bufʗ4);
            if (!bytes.Equal(bufʗ4, seed1ʗ6)) {
                tΔ6.Errorf("rand.Reader.Read with seed 1 = %x; want %x", bufʗ4, seed1ʗ6);
            }
        });

        rand.Reader.Read(buf);
        if (!bytes.Equal(buf, bytes.Repeat([5], 64))) {
            tΔ4.Errorf("rand.Reader not restored");
        }
    });

    // A direct internal use of drbg.Read.
    var seed1ʗ7 = seed1;
    t.Run(mlkemGenerateKey768ˢ, (ж<testing.T> tΔ7) => {
        var (exp, err) = mlkem.NewDecapsulationKey768(seed1ʗ7);
        if (err != nil) {
            tΔ7.Fatalf("mlkem.NewDecapsulationKey768: %v", err);
        }

        SetGlobalRandom(tΔ7, 1);
        (var got, err) = mlkem.GenerateKey768();
        if (err != nil) {
            tΔ7.Fatalf("mlkem.GenerateKey768: %v", err);
        }

        var gotBytes = got.Bytes(); if (!bytes.Equal(gotBytes, exp.Bytes())) {
            tΔ7.Errorf("mlkem.GenerateKey768 with seed 1 = %x; want %x", gotBytes, exp.Bytes());
        }
    });

    // An ignored passed-in Reader.
    var seed1ʗ8 = seed1;
    t.Run(ecdsaGenerateKeyˢ, (ж<testing.T> tΔ8) => {
        var (exp, err) = ecdsa.ParseRawPrivateKey(elliptic.P384(), seed1ʗ8[..48]);
        if (err != nil) {
            tΔ8.Fatalf("ecdsa.ParseRawPrivateKey: %v", err);
        }

        SetGlobalRandom(tΔ8, 1);
        (var got, err) = ecdsa.GenerateKey(elliptic.P384(), new cryptotest_internal_test_package.bytes_ReaderжReader(bytes.NewReader(slice<byte>("this reader is ignored"u8))));
        if (err != nil) {
            tΔ8.Fatalf("ecdsa.GenerateKey: %v", err);
        }

        if (!got.Equal(exp.OrTypedNil())) {
            tΔ8.Errorf("ecdsa.GenerateKey with seed 1 = %x; want %x", got.D.Bytes(), exp.D.Bytes());
        }
    });

    // The passed-in Reader is used if cryptocustomrand=1 is set,
    // and MaybeReadByte is called on it.
    t.Run(cryptocustomrand1ˢ, (ж<testing.T> tΔ9) => {
        tΔ9.Setenv(godebugˢ, cryptocustomrand1ˢ);

        var buf = new slice<byte>(49);
        buf[0] = 42;
        for (nint i = 2; i < 49; i++) {
            buf[i] = 1;
        }

        var (exp1, err) = ecdsa.ParseRawPrivateKey(elliptic.P384(), buf[..48]);
        if (err != nil) {
            tΔ9.Fatalf("ecdsa.ParseRawPrivateKey: %v", err);
        }
        (var exp2, err) = ecdsa.ParseRawPrivateKey(elliptic.P384(), buf[1..49]);
        if (err != nil) {
            tΔ9.Fatalf("ecdsa.ParseRawPrivateKey: %v", err);
        }

        var seen = new bool[]{}.array(2);
        for (nint i = 0; i < 1000; i++) {
            var r = bytes.NewReader(buf);
            var (got, errΔ1) = ecdsa.GenerateKey(elliptic.P384(), new cryptotest_internal_test_package.bytes_ReaderжReader(r));
            if (errΔ1 != nil) {
                tΔ9.Fatalf("ecdsa.GenerateKey: %v", errΔ1);
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
                tΔ9.Fatalf("ecdsa.GenerateKey with custom reader = %x; want %x or %x", got.D.Bytes(), exp1.D.Bytes(), exp2.D.Bytes());
                break;
            }}
            if (seen[0] && seen[1]) {
                break;
            }
        }
        if (!seen[0] || !seen[1]) {
            tΔ9.Errorf("ecdsa.GenerateKey with custom reader did not produce both expected keys");
        }

        // Again, with SetGlobalRandom.
        SetGlobalRandom(tΔ9, 1);

        seen = new bool[]{}.array(2);
        for (nint i = 0; i < 1000; i++) {
            var r = bytes.NewReader(buf);
            var (got, errΔ2) = ecdsa.GenerateKey(elliptic.P384(), new cryptotest_internal_test_package.bytes_ReaderжReader(r));
            if (errΔ2 != nil) {
                tΔ9.Fatalf("ecdsa.GenerateKey: %v", errΔ2);
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
                tΔ9.Fatalf("ecdsa.GenerateKey with custom reader and SetGlobalRandom = %x; want %x or %x", got.D.Bytes(), exp1.D.Bytes(), exp2.D.Bytes());
                break;
            }}
            if (seen[0] && seen[1]) {
                break;
            }
        }
        if (!seen[0] || !seen[1]) {
            tΔ9.Errorf("ecdsa.GenerateKey with custom reader and SetGlobalRandom did not produce both expected keys");
        }
    });
}

} // end cryptotest_internal_test_package
