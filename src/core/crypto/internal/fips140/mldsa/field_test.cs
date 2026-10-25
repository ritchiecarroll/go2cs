// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140;

using big = math.big_package;
using testing = testing_package;
using math;
using static go.crypto.@internal.fips140.mldsa_package;

partial class mldsa_internal_test_package {

internal partial struct interestingValue {
    internal uint32 v;
    internal global::go.crypto.@internal.fips140.mldsa_package.fieldElement m;
}

// q is large enough that we can't exhaustively test all q × q inputs, so when
// we have two inputs  we test [0, q) on one side and a set of interesting
// values on the other side.
internal static slice<interestingValue> interestingValues() {
    if (testing.Short()) {
        return new interestingValue[]{new(v: q - 1, m: minusOne)}.slice();
    }
    slice<interestingValue> values = default!;
    foreach (var (_, v) in new uint32[]{
        0,
        1,
        2,
        3,
        q - 3,
        q - 2,
        q - 1,
        q / 2,
        (q + 1) / 2
    }.slice()) {
        var (m, _) = fieldToMontgomery(v);
        values = append(values, new interestingValue(v: v, m: m));
        // Also test values that have an interesting Montgomery representation.
        values = append(values, new interestingValue(
            v: fieldFromMontgomery((global::go.crypto.@internal.fips140.mldsa_package.fieldElement)v), m: ((global::go.crypto.@internal.fips140.mldsa_package.fieldElement)v)));
    }
    return values;
}

public static void TestToFromMontgomery(ж<testing.T> Ꮡt) {
    foreach (var a in range<uint32>((uint32)q)) {
        var (m, err) = fieldToMontgomery(a);
        if (err != default!) {
            Ꮡt.Fatalf("fieldToMontgomery(%d) returned error: %v"u8, a, err);
        }
        var exp = (global::go.crypto.@internal.fips140.mldsa_package.fieldElement)(uint32)(((uint64)a * (uint64)R) % (uint64)q);
        if (m != exp) {
            Ꮡt.Fatalf("fieldToMontgomery(%d) = %d, expected %d"u8, a, m, exp);
        }
        var got = fieldFromMontgomery(m);
        if (got != a) {
            Ꮡt.Fatalf("fieldFromMontgomery(fieldToMontgomery(%d)) = %d, expected %d"u8, a, got, a);
        }
    }
}

public static void TestFieldAdd(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, a) in interestingValues()) {
        foreach (var b in range<global::go.crypto.@internal.fips140.mldsa_package.fieldElement>(((global::go.crypto.@internal.fips140.mldsa_package.fieldElement)q))) {
            var got = fieldAdd(a.m, b);
            var exp = (a.m + b) % (uint32)q;
            if (got != exp) {
                Ꮡt.Fatalf("%d + %d = %d, expected %d"u8, a, b, got, exp);
            }
        }
    }
}

public static void TestFieldSub(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, a) in interestingValues()) {
        foreach (var b in range<global::go.crypto.@internal.fips140.mldsa_package.fieldElement>(((global::go.crypto.@internal.fips140.mldsa_package.fieldElement)q))) {
            var got = fieldSub(a.m, b);
            var exp = (a.m + (uint32)q - b) % (uint32)q;
            if (got != exp) {
                Ꮡt.Fatalf("%d - %d = %d, expected %d"u8, a, b, got, exp);
            }
        }
    }
}

public static void TestFieldSubToMontgomery(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, a) in interestingValues()) {
        foreach (var b in range<uint32>((uint32)q)) {
            var got = fieldSubToMontgomery(a.v, b);
            var diff = (a.v + (uint32)q - b) % (uint32)q;
            var exp = (global::go.crypto.@internal.fips140.mldsa_package.fieldElement)(uint32)(((uint64)diff * (uint64)R) % (uint64)q);
            if (got != exp) {
                Ꮡt.Fatalf("fieldSubToMontgomery(%d, %d) = %d, expected %d"u8, a.v, b, got, exp);
            }
        }
    }
}

public static void TestFieldReduceOnce(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var a in range<uint32>((uint32)(2 * q))) {
        var got = fieldReduceOnce(a);
        uint32 exp = default!;
        if (a < q){
            exp = a;
        } else {
            exp = a - (uint32)q;
        }
        if ((uint32)got != exp) {
            Ꮡt.Fatalf("fieldReduceOnce(%d) = %d, expected %d"u8, a, got, exp);
        }
    }
}

public static void TestFieldMul(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var (_, a) in interestingValues()) {
        foreach (var b in range<global::go.crypto.@internal.fips140.mldsa_package.fieldElement>(((global::go.crypto.@internal.fips140.mldsa_package.fieldElement)q))) {
            var got = fieldFromMontgomery(fieldMontgomeryMul(a.m, b));
            var exp = (uint32)(((uint64)a.v * (uint64)fieldFromMontgomery(b)) % (uint64)q);
            if (got != exp) {
                Ꮡt.Fatalf("%d * %d = %d, expected %d"u8, a, b, got, exp);
            }
        }
    }
}

public static void TestFieldToMontgomeryOverflow(ж<testing.T> Ꮡt) {
    // fieldToMontgomery should reject inputs ≥ q.
    var inputs = new uint32[]{
        q,
        q + 1,
        q + 2,
        (1 << 23) - 1,
        (uint32)1 << 23,
        q + (1 << 23),
        (uint32)(q + 2147483648L),
        ~(uint32)0
    }.slice();
    foreach (var (_, @in) in inputs) {
        {
            var (_, err) = fieldToMontgomery(@in); if (err == default!) {
                Ꮡt.Fatalf("fieldToMontgomery(%d) did not return an error"u8, @in);
            }
        }
    }
}

public static void TestFieldMulSub(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in interestingValues()) {
        foreach (var (_, b) in interestingValues()) {
            foreach (var (_, c) in interestingValues()) {
                var got = fieldFromMontgomery(fieldMontgomeryMulSub(a.m, b.m, c.m));
                var exp = (uint32)(((uint64)a.v * ((uint64)b.v + (uint64)q - (uint64)c.v)) % (uint64)q);
                if (got != exp) {
                    Ꮡt.Fatalf("%d * (%d - %d) = %d, expected %d"u8, a.v, b.v, c.v, got, exp);
                }
            }
        }
    }
}

public static void TestFieldAddMul(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in interestingValues()) {
        foreach (var (_, b) in interestingValues()) {
            foreach (var (_, c) in interestingValues()) {
                foreach (var (_, d) in interestingValues()) {
                    var got = fieldFromMontgomery(fieldMontgomeryAddMul(a.m, b.m, c.m, d.m));
                    var exp = (uint32)(((uint64)a.v * (uint64)b.v + (uint64)c.v * (uint64)d.v) % (uint64)q);
                    if (got != exp) {
                        Ꮡt.Fatalf("%d + %d * %d = %d, expected %d"u8, a.v, b.v, c.v, got, exp);
                    }
                }
            }
        }
    }
}

public static uint8 BitRev8(uint8 n) {
    uint8 r = default!;
    r |= (uint8)((uint8)(n >> 7 & 0b0000_0001));
    r |= (uint8)((uint8)(n >> 5 & 0b0000_0010));
    r |= (uint8)((uint8)(n >> 3 & 0b0000_0100));
    r |= (uint8)((uint8)(n >> 1 & 0b0000_1000));
    r |= (uint8)((uint8)((uint8)(n << 1) & 0b0001_0000));
    r |= (uint8)((uint8)((uint8)(n << 3) & 0b0010_0000));
    r |= (uint8)((uint8)((uint8)(n << 5) & 0b0100_0000));
    r |= (uint8)((uint8)((uint8)(n << 7) & 0b1000_0000));
    return r;
}

public static int32 CenteredMod(uint32 x, uint32 m) {
    x = x % m;
    if (x > m / 2) {
        return (int32)x - (int32)m;
    }
    return (int32)x;
}

internal static uint32 reduceModQ(int32 x) {
    x %= q;
    if (x < 0) {
        return (uint32)(x + (int32)q);
    }
    return (uint32)x;
}

public static void TestCenteredMod(ж<testing.T> Ꮡt) {
    foreach (var x in range<uint32>((uint32)(q * 2))) {
        var got = CenteredMod((uint32)x, q);
        if (reduceModQ(got) != (x % (uint32)q)) {
            Ꮡt.Fatalf("CenteredMod(%d) = %d, which is not congruent to %d mod %d"u8, x, got, x, (nint)(q));
        }
    }

    foreach (var x in range<uint32>((uint32)q)) {
        var (r, _) = fieldToMontgomery(x);
        var got = fieldCenteredMod(r);
        var exp = CenteredMod(x, q);
        if (got != exp) {
            Ꮡt.Fatalf("fieldCenteredMod(%d) = %d, expected %d"u8, x, got, exp);
        }
    }
}

public static void TestInfinityNorm(ж<testing.T> Ꮡt) {
    foreach (var x in range<uint32>((uint32)q)) {
        var (r, _) = fieldToMontgomery(x);
        var got = fieldInfinityNorm(r);
        var exp = CenteredMod(x, q);
        if (exp < 0) {
            exp = -exp;
        }
        if (got != (uint32)exp) {
            Ꮡt.Fatalf("fieldInfinityNorm(%d) = %d, expected %d"u8, x, got, exp);
        }
    }
}

public static void TestConstants(ж<testing.T> Ꮡt) {
    if (fieldFromMontgomery(one) != 1) {
        Ꮡt.Errorf("one constant incorrect"u8);
    }
    if (fieldFromMontgomery(minusOne) != (uint32)(q - 1)) {
        Ꮡt.Errorf("minusOne constant incorrect"u8);
    }
    if (fieldInfinityNorm(one) != 1) {
        Ꮡt.Errorf("one infinity norm incorrect"u8);
    }
    if (fieldInfinityNorm(minusOne) != 1) {
        Ꮡt.Errorf("minusOne infinity norm incorrect"u8);
    }

    if (PublicKeySize44 != pubKeySize(params44)) {
        Ꮡt.Errorf("PublicKeySize44 constant incorrect"u8);
    }
    if (PublicKeySize65 != pubKeySize(params65)) {
        Ꮡt.Errorf("PublicKeySize65 constant incorrect"u8);
    }
    if (PublicKeySize87 != pubKeySize(params87)) {
        Ꮡt.Errorf("PublicKeySize87 constant incorrect"u8);
    }
    if (SignatureSize44 != sigSize(params44)) {
        Ꮡt.Errorf("SignatureSize44 constant incorrect"u8);
    }
    if (SignatureSize65 != sigSize(params65)) {
        Ꮡt.Errorf("SignatureSize65 constant incorrect"u8);
    }
    if (SignatureSize87 != sigSize(params87)) {
        Ꮡt.Errorf("SignatureSize87 constant incorrect"u8);
    }
}

public static void TestPower2Round(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    foreach (var x in range<uint32>((uint32)q)) {
        var (rr, _) = fieldToMontgomery(x);
        var (t1, t0) = power2Round(rr);

        var (hi, err) = fieldToMontgomery((uint32)t1 << 13);
        if (err != default!) {
            Ꮡt.Fatalf("power2Round(%d): failed to convert high part to Montgomery: %v"u8, x, err);
        }
        {
            var r = fieldFromMontgomery(fieldAdd(hi, t0)); if (r != x) {
                Ꮡt.Fatalf("power2Round(%d) = (%d, %d), which reconstructs to %d, expected %d"u8, x, t1, t0, r, x);
            }
        }
    }
}

internal static (uint32 R1, int32 R0) SpecDecompose(global::go.crypto.@internal.fips140.mldsa_package.fieldElement rr, global::go.crypto.@internal.fips140.mldsa_package.parameters p) {
    var r = fieldFromMontgomery(rr);
    if ((nint)(q - 1) % p.γ2 != 0) {
        throw panic("mldsa: internal error: unsupported denγ2");
    }
    var γ2 = (uint32)(q - 1) / (uint32)p.γ2;
    var r0 = CenteredMod(r, 2 * γ2);
    var diff = (int32)r - r0;
    if (diff == (int32)(q - 1)){
        r0 = r0 - 1;
        return (0, r0);
    } else {
        if (diff < 0 || (uint32)diff % γ2 != 0) {
            throw panic("mldsa: internal error: invalid decomposition");
        }
        var r1 = (uint32)diff / (2 * γ2);
        return (r1, r0);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mlDsa6587ˢ = "ML-DSA-65,87"u8;

public static void TestDecompose(ж<testing.T> Ꮡt) {
    Ꮡt.Run(mlDsa44ˢ, (ж<testing.T> tΔ1) => {
        testDecompose(tΔ1, params44);
    });
    Ꮡt.Run(mlDsa6587ˢ, (ж<testing.T> tΔ2) => {
        testDecompose(tΔ2, params65);
    });
}

internal static void testDecompose(ж<testing.T> Ꮡt, global::go.crypto.@internal.fips140.mldsa_package.parameters p) {
    Ꮡt.Parallel();
    foreach (var x in range<uint32>((uint32)q)) {
        var (rr, _) = fieldToMontgomery(x);
        var (r1, r0) = SpecDecompose(rr, p);

        // Check that SpecDecompose is correct.
        // r ≡ r1 * (2 * γ2) + r0 mod q
        var γ2 = (uint32)(q - 1) / (uint32)p.γ2;
        var reconstructed = reduceModQ((int32)(r1 * 2 * γ2) + r0);
        if (reconstructed != x) {
            Ꮡt.Fatalf("SpecDecompose(%d) = (%d, %d), which reconstructs to %d, expected %d"u8, x, r1, r0, reconstructed, x);
        }

        byte gotR1 = default!;
        int32 gotR0 = default!;
        switch (p.γ2) {
        case 88: {
            (gotR1, gotR0) = decompose88(rr);
            if (gotR1 > 43) {
                Ꮡt.Fatalf("decompose88(%d) returned r1 = %d, which is out of range"u8, x, gotR1);
            }
            break;
        }
        case 32: {
            (gotR1, gotR0) = decompose32(rr);
            if (gotR1 > 15) {
                Ꮡt.Fatalf("decompose32(%d) returned r1 = %d, which is out of range"u8, x, gotR1);
            }
            break;
        }
        default: {
            Ꮡt.Fatalf("unsupported denγ2: %d"u8, p.γ2);
            break;
        }}
        if ((uint32)gotR1 != r1) {
            Ꮡt.Fatalf("highBits(%d) = %d, expected %d"u8, x, gotR1, r1);
        }
        if (gotR0 != r0) {
            Ꮡt.Fatalf("lowBits(%d) = %d, expected %d"u8, x, gotR0, r0);
        }
    }
}

public static void TestZetas(ж<testing.T> Ꮡt) {
    var ζ = big.NewInt(1753);
    var q = big.NewInt(mldsa_package.q);
    foreach (var (k, zeta) in zetas.ΔRangeSnapshot()) {
        // ζ^BitRev₈(k) mod q
        var exp = @new<bigꓸInt>().Exp(ζ, big.NewInt((int64)BitRev8((uint8)k)), q);
        var got = fieldFromMontgomery(zeta);
        if (big.NewInt((int64)got).Cmp(exp) != 0) {
            Ꮡt.Errorf("zetas[%d] = %v, expected %v"u8, k, got, exp.OrTypedNil());
        }
    }
}

} // end mldsa_internal_test_package
