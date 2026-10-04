// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto;

using bytes = bytes_package;
using mlkem = go.crypto.@internal.fips140.mlkem_package;
using sha3 = go.crypto.@internal.fips140.sha3_package;
using rand = go.crypto.rand_package;
using hex = encoding.hex_package;
using flag = flag_package;
using testing = testing_package;
using encoding;
using go.crypto;
using go.crypto.@internal.fips140;
using static go.crypto.mlkem_package;

partial class mlkem_internal_test_package {

[GoType] internal partial interface encapsulationKey {
    slice<byte> Bytes();
    (slice<byte>, slice<byte>) Encapsulate();
}

[GoType] internal partial interface decapsulationKey<E> 
    where E : encapsulationKey{
    slice<byte> Bytes();
    (slice<byte>, error) Decapsulate(slice<byte> _);
    E EncapsulationKey();
}

public static void TestRoundTrip(ж<testing.T> Ꮡt) {
    Ꮡt.Run("768"u8, (ж<testing.T> tΔ1) => {
        testRoundTrip<encapsulationKey, decapsulationKey<encapsulationKey>>(tΔ1, widenResult<ж<global::go.crypto.mlkem_package.DecapsulationKey768>, decapsulationKey<encapsulationKey>>(GenerateKey768, elemᴛ1 => new mlkem_test_package.mlkem_DecapsulationKey768жdecapsulationKey(elemᴛ1)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.EncapsulationKey768>, encapsulationKey>(NewEncapsulationKey768, elemᴛ2 => new mlkem_internal_test_package.mlkem_EncapsulationKey768жencapsulationKey(elemᴛ2)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.DecapsulationKey768>, decapsulationKey<encapsulationKey>>(NewDecapsulationKey768, elemᴛ3 => new mlkem_test_package.mlkem_DecapsulationKey768жdecapsulationKey(elemᴛ3)));
    });
    Ꮡt.Run("1024"u8, (ж<testing.T> tΔ2) => {
        testRoundTrip<encapsulationKey, decapsulationKey<encapsulationKey>>(tΔ2, widenResult<ж<global::go.crypto.mlkem_package.DecapsulationKey1024>, decapsulationKey<encapsulationKey>>(GenerateKey1024, elemᴛ1 => new mlkem_test_package.mlkem_DecapsulationKey1024жdecapsulationKey(elemᴛ1)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.EncapsulationKey1024>, encapsulationKey>(NewEncapsulationKey1024, elemᴛ2 => new mlkem_internal_test_package.mlkem_EncapsulationKey1024жencapsulationKey(elemᴛ2)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.DecapsulationKey1024>, decapsulationKey<encapsulationKey>>(NewDecapsulationKey1024, elemᴛ3 => new mlkem_test_package.mlkem_DecapsulationKey1024жdecapsulationKey(elemᴛ3)));
    });
}

internal static void testRoundTrip<E, D>(ж<testing.T> Ꮡt, Func<(D, error)> generateKey, Func<slice<byte>, (E, error)> newEncapsulationKey, Func<slice<byte>, (D, error)> newDecapsulationKey)
    where E : encapsulationKey
    where D : decapsulationKey<E>
{
    var (dk, err) = generateKey();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var ek = dk.EncapsulationKey();
    var (Ke, c) = ek.Encapsulate();
    (var Kd, err) = dk.Decapsulate(c);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(Ke, Kd)) {
        Ꮡt.Fail();
    }
    (var ek1, err) = newEncapsulationKey(ek.Bytes());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(ek.Bytes(), ek1.Bytes())) {
        Ꮡt.Fail();
    }
    (var dk1, err) = newDecapsulationKey(dk.Bytes());
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(dk.Bytes(), dk1.Bytes())) {
        Ꮡt.Fail();
    }
    var (Ke1, c1) = ek1.Encapsulate();
    (var Kd1, err) = dk1.Decapsulate(c1);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (!bytes.Equal(Ke1, Kd1)) {
        Ꮡt.Fail();
    }
    (var dk2, err) = generateKey();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    if (bytes.Equal(dk.EncapsulationKey().Bytes(), dk2.EncapsulationKey().Bytes())) {
        Ꮡt.Fail();
    }
    if (bytes.Equal(dk.Bytes(), dk2.Bytes())) {
        Ꮡt.Fail();
    }
    var (Ke2, c2) = dk.EncapsulationKey().Encapsulate();
    if (bytes.Equal(c, c2)) {
        Ꮡt.Fail();
    }
    if (bytes.Equal(Ke, Ke2)) {
        Ꮡt.Fail();
    }
}

public static void TestBadLengths(ж<testing.T> Ꮡt) {
    Ꮡt.Run("768"u8, (ж<testing.T> tΔ1) => {
        testBadLengths<encapsulationKey, decapsulationKey<encapsulationKey>>(tΔ1, widenResult<ж<global::go.crypto.mlkem_package.DecapsulationKey768>, decapsulationKey<encapsulationKey>>(GenerateKey768, elemᴛ1 => new mlkem_test_package.mlkem_DecapsulationKey768жdecapsulationKey(elemᴛ1)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.EncapsulationKey768>, encapsulationKey>(NewEncapsulationKey768, elemᴛ2 => new mlkem_internal_test_package.mlkem_EncapsulationKey768жencapsulationKey(elemᴛ2)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.DecapsulationKey768>, decapsulationKey<encapsulationKey>>(NewDecapsulationKey768, elemᴛ3 => new mlkem_test_package.mlkem_DecapsulationKey768жdecapsulationKey(elemᴛ3)));
    });
    Ꮡt.Run("1024"u8, (ж<testing.T> tΔ2) => {
        testBadLengths<encapsulationKey, decapsulationKey<encapsulationKey>>(tΔ2, widenResult<ж<global::go.crypto.mlkem_package.DecapsulationKey1024>, decapsulationKey<encapsulationKey>>(GenerateKey1024, elemᴛ1 => new mlkem_test_package.mlkem_DecapsulationKey1024жdecapsulationKey(elemᴛ1)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.EncapsulationKey1024>, encapsulationKey>(NewEncapsulationKey1024, elemᴛ2 => new mlkem_internal_test_package.mlkem_EncapsulationKey1024жencapsulationKey(elemᴛ2)), widen<slice<byte>, ж<global::go.crypto.mlkem_package.DecapsulationKey1024>, decapsulationKey<encapsulationKey>>(NewDecapsulationKey1024, elemᴛ3 => new mlkem_test_package.mlkem_DecapsulationKey1024жdecapsulationKey(elemᴛ3)));
    });
}

internal static void testBadLengths<E, D>(ж<testing.T> Ꮡt, Func<(D, error)> generateKey, Func<slice<byte>, (E, error)> newEncapsulationKey, Func<slice<byte>, (D, error)> newDecapsulationKey)
    where E : encapsulationKey
    where D : decapsulationKey<E>
{
    var (dk, err) = generateKey();
    var dkBytes = dk.Bytes();
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    var ek = dk.EncapsulationKey();
    var ekBytes = dk.EncapsulationKey().Bytes();
    var (_, c) = ek.Encapsulate();
    for (nint i = 0; i < len(dkBytes) - 1; i++) {
        {
            var (_, errΔ1) = newDecapsulationKey(dkBytes.slice(0, i)); if (errΔ1 == default!) {
                Ꮡt.Errorf("expected error for dk length %d"u8, i);
            }
        }
    }
    var dkLong = dkBytes;
    for (nint i = 0; i < 100; i++) {
        dkLong = append(dkLong, (byte)(0));
        {
            var (_, errΔ2) = newDecapsulationKey(dkLong); if (errΔ2 == default!) {
                Ꮡt.Errorf("expected error for dk length %d"u8, len(dkLong));
            }
        }
    }
    for (nint i = 0; i < len(ekBytes) - 1; i++) {
        {
            var (_, errΔ3) = newEncapsulationKey(ekBytes.slice(0, i)); if (errΔ3 == default!) {
                Ꮡt.Errorf("expected error for ek length %d"u8, i);
            }
        }
    }
    var ekLong = ekBytes;
    for (nint i = 0; i < 100; i++) {
        ekLong = append(ekLong, (byte)(0));
        {
            var (_, errΔ4) = newEncapsulationKey(ekLong); if (errΔ4 == default!) {
                Ꮡt.Errorf("expected error for ek length %d"u8, len(ekLong));
            }
        }
    }
    for (nint i = 0; i < len(c) - 1; i++) {
        {
            var (_, errΔ5) = dk.Decapsulate(c.slice(0, i)); if (errΔ5 == default!) {
                Ꮡt.Errorf("expected error for c length %d"u8, i);
            }
        }
    }
    var cLong = c;
    for (nint i = 0; i < 100; i++) {
        cLong = append(cLong, (byte)(0));
        {
            var (_, errΔ6) = dk.Decapsulate(cLong); if (errΔ6 == default!) {
                Ꮡt.Errorf("expected error for c length %d"u8, len(cLong));
            }
        }
    }
}

internal static ж<bool> millionFlag = flag.Bool("million"u8, false, "run the million vector test"u8);

// TestAccumulated accumulates 10k (or 100, or 1M) random vectors and checks the
// hash of the result, to avoid checking in 150MB of test vectors.
public static void TestAccumulated(ж<testing.T> Ꮡt) {
    nint n = 10000;
    @string expected = "8a518cc63da366322a8e7a818c7a0d63483cb3528d34a4cf42f35d5ad73f22fc"u8;
    if (testing.Short()) {
        n = 100;
        expected = "1114b1b6699ed191734fa339376afa7e285c9e6acf6ff0177d346696ce564415"u8;
    }
    if (millionFlag.Value) {
        n = 1000000;
        expected = "424bf8f0e8ae99b78d788a6e2e8e9cdaf9773fc0c08a6f433507cb559edfd0f0"u8;
    }
    var s = sha3.NewShake128();
    var o = sha3.NewShake128();
    var seed = new slice<byte>(SeedSize);
    ref var msg = ref heap(new array<byte>(32), out var Ꮡmsg);
    var ct1 = new slice<byte>(CiphertextSize768);
    for (nint i = 0; i < n; i++) {
        s.Read(seed);
        var (dk, err) = NewDecapsulationKey768(seed);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var ek = dk.EncapsulationKey();
        o.Write(ek.Bytes());
        s.Read(msg[..]);
        var (k, ct) = (~ek).key.EncapsulateInternal(Ꮡmsg);
        o.Write(ct);
        o.Write(k);
        (var kk, err) = dk.Decapsulate(ct);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        if (!bytes.Equal(kk, k)) {
            Ꮡt.Errorf("k: got %x, expected %x"u8, kk, k);
        }
        s.Read(ct1);
        (var k1, err) = dk.Decapsulate(ct1);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        o.Write(k1);
    }
    @string got = hex.EncodeToString(o.Sum(default!));
    if (got != expected) {
        Ꮡt.Errorf("got %s, expected %s"u8, got, expected);
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
        var dk = mlkem.GenerateKeyInternal768(Ꮡd, Ꮡz);
        sink ^= (byte)(dk.EncapsulationKey().Bytes()[0]);
    }
}

public static void BenchmarkEncaps(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var seed = new slice<byte>(SeedSize);
    rand.Read(seed);
    ref var m = ref heap(new array<byte>(32), out var Ꮡm);
    rand.Read(m[..]);
    var (dk, err) = NewDecapsulationKey768(seed);
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    var ekBytes = dk.EncapsulationKey().Bytes();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        var (ek, errΔ1) = NewEncapsulationKey768(ekBytes);
        if (errΔ1 != default!) {
            Ꮡb.Fatal(errΔ1);
        }
        var (K, c) = (~ek).key.EncapsulateInternal(Ꮡm);
        sink ^= (byte)((byte)(c[0] ^ K[0]));
    }
}

public static void BenchmarkDecaps(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (dk, err) = GenerateKey768();
    if (err != default!) {
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
internal static readonly @string aliceˢ = "Alice"u8;
internal static readonly @string bobˢ = "Bob"u8;

public static void BenchmarkRoundTrip(ж<testing.B> Ꮡb) {
    var (dk, err) = GenerateKey768();
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    var ek = dk.EncapsulationKey();
    var ekBytes = ek.Bytes();
    var (_, c) = ek.Encapsulate();
    if (err != default!) {
        Ꮡb.Fatal(err);
    }
    var cʗ1 = c;
    var dkʗ1 = dk;
    Ꮡb.Run(aliceˢ, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            var (dkS, errΔ1) = GenerateKey768();
            if (errΔ1 != default!) {
                bΔ1.Fatal(errΔ1);
            }
            var ekS = dkS.EncapsulationKey().Bytes();
            sink ^= (byte)(ekS[0]);
            (var Ks, errΔ1) = dkʗ1.Decapsulate(cʗ1);
            if (errΔ1 != default!) {
                bΔ1.Fatal(errΔ1);
            }
            sink ^= (byte)(Ks[0]);
        }
    });
    var ekBytesʗ1 = ekBytes;
    Ꮡb.Run(bobˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            var (ekΔ1, errΔ2) = NewEncapsulationKey768(ekBytesʗ1);
            if (errΔ2 != default!) {
                bΔ2.Fatal(errΔ2);
            }
            var (Ks, cS) = ekΔ1.Encapsulate();
            if (errΔ2 != default!) {
                bΔ2.Fatal(errΔ2);
            }
            sink ^= (byte)((byte)(cS[0] ^ Ks[0]));
        }
    });
}

// Test that the constants from the public API match the corresponding values from the internal API.
public static void TestConstantSizes(ж<testing.T> Ꮡt) {
    if (SharedKeySize != mlkem.SharedKeySize) {
        Ꮡt.Errorf("SharedKeySize mismatch: got %d, want %d"u8, (nint)(SharedKeySize), (nint)(mlkem.SharedKeySize));
    }
    if (SeedSize != mlkem.SeedSize) {
        Ꮡt.Errorf("SeedSize mismatch: got %d, want %d"u8, (nint)(SeedSize), (nint)(mlkem.SeedSize));
    }
    if (CiphertextSize768 != mlkem.CiphertextSize768) {
        Ꮡt.Errorf("CiphertextSize768 mismatch: got %d, want %d"u8, (nint)(CiphertextSize768), (nint)(mlkem.CiphertextSize768));
    }
    if (EncapsulationKeySize768 != mlkem.EncapsulationKeySize768) {
        Ꮡt.Errorf("EncapsulationKeySize768 mismatch: got %d, want %d"u8, (nint)(EncapsulationKeySize768), (nint)(mlkem.EncapsulationKeySize768));
    }
    if (CiphertextSize1024 != mlkem.CiphertextSize1024) {
        Ꮡt.Errorf("CiphertextSize1024 mismatch: got %d, want %d"u8, (nint)(CiphertextSize1024), (nint)(mlkem.CiphertextSize1024));
    }
    if (EncapsulationKeySize1024 != mlkem.EncapsulationKeySize1024) {
        Ꮡt.Errorf("EncapsulationKeySize1024 mismatch: got %d, want %d"u8, (nint)(EncapsulationKeySize1024), (nint)(mlkem.EncapsulationKeySize1024));
    }
}

} // end mlkem_internal_test_package
