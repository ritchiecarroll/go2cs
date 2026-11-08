// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using Δmlkem = go.crypto.@internal.fips140.mlkem_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using static go.crypto.mlkem_package;
using mlkemtest = go.crypto.mlkem.mlkemtest_package;
using rand = go.crypto.rand_package;
using hex = encoding.hex_package;
using flag = flag_package;
using testing = testing_package;
using encoding;
using go.crypto;
using go.crypto.@internal.fips140;
using go.crypto.mlkem;

partial class mlkem_test_package {

partial interface encapsulationKey {
    slice<byte> Bytes();
    (slice<byte>, slice<byte>) Encapsulate();
}

partial interface decapsulationKey<E> 
    where E : encapsulationKey{
    slice<byte> Bytes();
    (slice<byte>, error) Decapsulate(slice<byte> _);
    E EncapsulationKey();
}

public static void TestRoundTrip(ж<testing.T> t) {
    t.Run("768", (ж<testing.T> tΔ1) => {
        testRoundTrip<encapsulationKey, decapsulationKey<encapsulationKey>>(tΔ1, widenResult<ж<global::go.crypto.mlkem_package.DecapsulationKey768>, decapsulationKey<encapsulationKey>>(GenerateKey768, elemᴛ1 => new mlkem_DecapsulationKey768жdecapsulationKey(elemᴛ1)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.EncapsulationKey768>, encapsulationKey>(NewEncapsulationKey768, elemᴛ2 => new mlkem_EncapsulationKey768жencapsulationKey(elemᴛ2)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.DecapsulationKey768>, decapsulationKey<encapsulationKey>>(NewDecapsulationKey768, elemᴛ3 => new mlkem_DecapsulationKey768жdecapsulationKey(elemᴛ3)));
    });
    t.Run("1024", (ж<testing.T> tΔ2) => {
        testRoundTrip<encapsulationKey, decapsulationKey<encapsulationKey>>(tΔ2, widenResult<ж<global::go.crypto.mlkem_package.DecapsulationKey1024>, decapsulationKey<encapsulationKey>>(GenerateKey1024, elemᴛ1 => new mlkem_DecapsulationKey1024жdecapsulationKey(elemᴛ1)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.EncapsulationKey1024>, encapsulationKey>(NewEncapsulationKey1024, elemᴛ2 => new mlkem_EncapsulationKey1024жencapsulationKey(elemᴛ2)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.DecapsulationKey1024>, decapsulationKey<encapsulationKey>>(NewDecapsulationKey1024, elemᴛ3 => new mlkem_DecapsulationKey1024жdecapsulationKey(elemᴛ3)));
    });
}

internal static void testRoundTrip<E, D>(ж<testing.T> t, Func<(D, error)> generateKey, Func<slice<byte>, (E, error)> newEncapsulationKey, Func<slice<byte>, (D, error)> newDecapsulationKey)
    where E : encapsulationKey
    where D : decapsulationKey<E>
{
    var (dk, err) = generateKey();
    if (err != nil) {
        t.Fatal(err);
    }
    var ek = dk.EncapsulationKey();
    var (Ke, c) = ek.Encapsulate();
    (var Kd, err) = dk.Decapsulate(c);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(Ke, Kd)) {
        t.Fail();
    }

    (var ek1, err) = newEncapsulationKey(ek.Bytes());
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(ek.Bytes(), ek1.Bytes())) {
        t.Fail();
    }
    (var dk1, err) = newDecapsulationKey(dk.Bytes());
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(dk.Bytes(), dk1.Bytes())) {
        t.Fail();
    }
    var (Ke1, c1) = ek1.Encapsulate();
    (var Kd1, err) = dk1.Decapsulate(c1);
    if (err != nil) {
        t.Fatal(err);
    }
    if (!bytes.Equal(Ke1, Kd1)) {
        t.Fail();
    }

    (var dk2, err) = generateKey();
    if (err != nil) {
        t.Fatal(err);
    }
    if (bytes.Equal(dk.EncapsulationKey().Bytes(), dk2.EncapsulationKey().Bytes())) {
        t.Fail();
    }
    if (bytes.Equal(dk.Bytes(), dk2.Bytes())) {
        t.Fail();
    }

    var (Ke2, c2) = dk.EncapsulationKey().Encapsulate();
    if (bytes.Equal(c, c2)) {
        t.Fail();
    }
    if (bytes.Equal(Ke, Ke2)) {
        t.Fail();
    }
}

public static void TestBadLengths(ж<testing.T> t) {
    t.Run("768", (ж<testing.T> tΔ1) => {
        testBadLengths<encapsulationKey, decapsulationKey<encapsulationKey>>(tΔ1, widenResult<ж<global::go.crypto.mlkem_package.DecapsulationKey768>, decapsulationKey<encapsulationKey>>(GenerateKey768, elemᴛ1 => new mlkem_DecapsulationKey768жdecapsulationKey(elemᴛ1)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.EncapsulationKey768>, encapsulationKey>(NewEncapsulationKey768, elemᴛ2 => new mlkem_EncapsulationKey768жencapsulationKey(elemᴛ2)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.DecapsulationKey768>, decapsulationKey<encapsulationKey>>(NewDecapsulationKey768, elemᴛ3 => new mlkem_DecapsulationKey768жdecapsulationKey(elemᴛ3)));
    });
    t.Run("1024", (ж<testing.T> tΔ2) => {
        testBadLengths<encapsulationKey, decapsulationKey<encapsulationKey>>(tΔ2, widenResult<ж<global::go.crypto.mlkem_package.DecapsulationKey1024>, decapsulationKey<encapsulationKey>>(GenerateKey1024, elemᴛ1 => new mlkem_DecapsulationKey1024жdecapsulationKey(elemᴛ1)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.EncapsulationKey1024>, encapsulationKey>(NewEncapsulationKey1024, elemᴛ2 => new mlkem_EncapsulationKey1024жencapsulationKey(elemᴛ2)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.DecapsulationKey1024>, decapsulationKey<encapsulationKey>>(NewDecapsulationKey1024, elemᴛ3 => new mlkem_DecapsulationKey1024жdecapsulationKey(elemᴛ3)));
    });
}

internal static void testBadLengths<E, D>(ж<testing.T> t, Func<(D, error)> generateKey, Func<slice<byte>, (E, error)> newEncapsulationKey, Func<slice<byte>, (D, error)> newDecapsulationKey)
    where E : encapsulationKey
    where D : decapsulationKey<E>
{
    var (dk, err) = generateKey();
    var dkBytes = dk.Bytes();
    if (err != nil) {
        t.Fatal(err);
    }
    var ek = dk.EncapsulationKey();
    var ekBytes = dk.EncapsulationKey().Bytes();
    var (_, c) = ek.Encapsulate();

    for (nint i = 0; i < len(dkBytes) - 1; i++) {
        var (_, errΔ1) = newDecapsulationKey(dkBytes.slice(0, i)); if (errΔ1 == nil) {
            t.Errorf("expected error for dk length %d", i);
        }
    }
    var dkLong = dkBytes;
    for (nint i = 0; i < 100; i++) {
        dkLong = append(dkLong, (byte)0);
        var (_, errΔ2) = newDecapsulationKey(dkLong); if (errΔ2 == nil) {
            t.Errorf("expected error for dk length %d", len(dkLong));
        }
    }

    for (nint i = 0; i < len(ekBytes) - 1; i++) {
        var (_, errΔ3) = newEncapsulationKey(ekBytes.slice(0, i)); if (errΔ3 == nil) {
            t.Errorf("expected error for ek length %d", i);
        }
    }
    var ekLong = ekBytes;
    for (nint i = 0; i < 100; i++) {
        ekLong = append(ekLong, (byte)0);
        var (_, errΔ4) = newEncapsulationKey(ekLong); if (errΔ4 == nil) {
            t.Errorf("expected error for ek length %d", len(ekLong));
        }
    }

    for (nint i = 0; i < len(c) - 1; i++) {
        var (_, errΔ5) = dk.Decapsulate(c.slice(0, i)); if (errΔ5 == nil) {
            t.Errorf("expected error for c length %d", i);
        }
    }
    var cLong = c;
    for (nint i = 0; i < 100; i++) {
        cLong = append(cLong, (byte)0);
        var (_, errΔ6) = dk.Decapsulate(cLong); if (errΔ6 == nil) {
            t.Errorf("expected error for c length %d", len(cLong));
        }
    }
}

internal static ж<bool> millionFlag = flag.Bool("million", false, "run the million vector test");

// TestAccumulated accumulates 10k (or 100, or 1M) random vectors and checks the
// hash of the result, to avoid checking in 150MB of test vectors.
public static void TestAccumulated(ж<testing.T> t) {
    nint n = 10000;
    @string expected = "8a518cc63da366322a8e7a818c7a0d63483cb3528d34a4cf42f35d5ad73f22fc";
    if (testing.Short()) {
        n = 100;
        expected = "1114b1b6699ed191734fa339376afa7e285c9e6acf6ff0177d346696ce564415";
    }
    if (millionFlag.Value) {
        n = 1000000;
        expected = "424bf8f0e8ae99b78d788a6e2e8e9cdaf9773fc0c08a6f433507cb559edfd0f0";
    }

    var s = sha3.NewShake128();
    var o = sha3.NewShake128();
    var seed = new slice<byte>(SeedSize);
    var msg = new slice<byte>(32);
    var ct1 = new slice<byte>(CiphertextSize768);

    for (nint i = 0; i < n; i++) {
        s.Read(seed);
        var (dk, err) = NewDecapsulationKey768(seed);
        if (err != nil) {
            t.Fatal(err);
        }
        var ek = dk.EncapsulationKey();
        o.Write(ek.Bytes());

        s.Read(msg);
        (var k, var ct, err) = mlkemtest.Encapsulate768(ek, msg);
        if (err != nil) {
            t.Fatal(err);
        }
        o.Write(ct);
        o.Write(k);

        (var kk, err) = dk.Decapsulate(ct);
        if (err != nil) {
            t.Fatal(err);
        }
        if (!bytes.Equal(kk, k)) {
            t.Errorf("k: got %x, expected %x", kk, k);
        }

        s.Read(ct1);
        (var k1, err) = dk.Decapsulate(ct1);
        if (err != nil) {
            t.Fatal(err);
        }
        o.Write(k1);
    }

    @string got = hex.EncodeToString(o.Sum(default));
    if (got != expected) {
        t.Errorf("got %s, expected %s", got, expected);
    }
}

internal static byte sink;

public static void BenchmarkKeyGen(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var d = ref heap(new array<byte>(32), out var Ꮡd);
    ref var z = ref heap(new array<byte>(32), out var Ꮡz);
    rand.Read(d[..]);
    rand.Read(z[..]);
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var dk = Δmlkem.GenerateKeyInternal768(Ꮡd, Ꮡz);
        sink ^= (byte)(dk.EncapsulationKey().Bytes()[0]);
    }
}

public static void BenchmarkEncaps(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var seed = new slice<byte>(SeedSize);
    rand.Read(seed);
    var (dk, err) = NewDecapsulationKey768(seed);
    if (err != nil) {
        Ꮡb.Fatal(err);
    }
    var ekBytes = dk.EncapsulationKey().Bytes();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (ek, errΔ1) = NewEncapsulationKey768(ekBytes);
        if (errΔ1 != nil) {
            Ꮡb.Fatal(errΔ1);
        }
        var (K, c) = ek.Encapsulate();
        sink ^= (byte)((byte)(c[0] ^ K[0]));
    }
}

public static void BenchmarkDecaps(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (dk, err) = GenerateKey768();
    if (err != nil) {
        Ꮡb.Fatal(err);
    }
    var ek = dk.EncapsulationKey();
    var (_, c) = ek.Encapsulate();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (K, _) = dk.Decapsulate(c);
        sink ^= (byte)(K[0]);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string aliceˢ => ref aliceˢᵇ;
private static readonly @string aliceˢᵇ = "Alice"u8;
private static ref readonly @string bobˢ => ref bobˢᵇ;
private static readonly @string bobˢᵇ = "Bob"u8;

public static void BenchmarkRoundTrip(ж<testing.B> b) {
    var (dk, err) = GenerateKey768();
    if (err != nil) {
        b.Fatal(err);
    }
    var ek = dk.EncapsulationKey();
    var ekBytes = ek.Bytes();
    var (_, c) = ek.Encapsulate();
    if (err != nil) {
        b.Fatal(err);
    }
    var cʗ1 = c;
    var dkʗ1 = dk;
    b.Run(aliceˢ, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var (dkS, errΔ1) = GenerateKey768();
            if (errΔ1 != nil) {
                bΔ1.Fatal(errΔ1);
            }
            var ekS = dkS.EncapsulationKey().Bytes();
            sink ^= (byte)(ekS[0]);

            (var Ks, errΔ1) = dkʗ1.Decapsulate(cʗ1);
            if (errΔ1 != nil) {
                bΔ1.Fatal(errΔ1);
            }
            sink ^= (byte)(Ks[0]);
        }
    });
    var ekBytesʗ1 = ekBytes;
    b.Run(bobˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            var (ekΔ1, errΔ2) = NewEncapsulationKey768(ekBytesʗ1);
            if (errΔ2 != nil) {
                bΔ2.Fatal(errΔ2);
            }
            var (Ks, cS) = ekΔ1.Encapsulate();
            if (errΔ2 != nil) {
                bΔ2.Fatal(errΔ2);
            }
            sink ^= (byte)((byte)(cS[0] ^ Ks[0]));
        }
    });
}

// Test that the constants from the public API match the corresponding values from the internal API.
public static void TestConstantSizes(ж<testing.T> t) {
    if (SharedKeySize != Δmlkem.SharedKeySize) {
        t.Errorf("SharedKeySize mismatch: got %d, want %d", (nint)(SharedKeySize), (nint)(Δmlkem.SharedKeySize));
    }

    if (SeedSize != Δmlkem.SeedSize) {
        t.Errorf("SeedSize mismatch: got %d, want %d", (nint)(SeedSize), (nint)(Δmlkem.SeedSize));
    }

    if (CiphertextSize768 != Δmlkem.CiphertextSize768) {
        t.Errorf("CiphertextSize768 mismatch: got %d, want %d", (nint)(CiphertextSize768), (nint)(Δmlkem.CiphertextSize768));
    }

    if (EncapsulationKeySize768 != Δmlkem.EncapsulationKeySize768) {
        t.Errorf("EncapsulationKeySize768 mismatch: got %d, want %d", (nint)(EncapsulationKeySize768), (nint)(Δmlkem.EncapsulationKeySize768));
    }

    if (CiphertextSize1024 != Δmlkem.CiphertextSize1024) {
        t.Errorf("CiphertextSize1024 mismatch: got %d, want %d", (nint)(CiphertextSize1024), (nint)(Δmlkem.CiphertextSize1024));
    }

    if (EncapsulationKeySize1024 != Δmlkem.EncapsulationKeySize1024) {
        t.Errorf("EncapsulationKeySize1024 mismatch: got %d, want %d", (nint)(EncapsulationKeySize1024), (nint)(Δmlkem.EncapsulationKeySize1024));
    }
}

} // end mlkem_test_package
