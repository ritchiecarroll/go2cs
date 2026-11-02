// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140test;

using bytes = bytes_package;
using elliptic = go.crypto.elliptic_package;
using cryptotest = go.crypto.@internal.cryptotest_package;
using nistec = go.crypto.@internal.fips140.nistec_package;
using fmt = fmt_package;
using big = math.big_package;
using rand = math.rand_package;
using testing = testing_package;
using go.crypto;
using go.crypto.@internal;
using go.crypto.@internal.fips140;
using math;

partial class fipstest_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string p224ˢ => ref p224ˢᵇ;
private static readonly @string p224ˢᵇ = "P224"u8;
internal static ref readonly @string p256ˢ => ref p256ˢᵇ;
private static readonly @string p256ˢᵇ = "P256"u8;
internal static ref readonly @string p384ˢ => ref p384ˢᵇ;
private static readonly @string p384ˢᵇ = "P384"u8;
internal static ref readonly @string p521ˢ => ref p521ˢᵇ;
private static readonly @string p521ˢᵇ = "P521"u8;

public static void TestNISTECAllocations(ж<testing.T> t) {
    cryptotest.SkipTestAllocations(t);
    t.Run(p224ˢ, (ж<testing.T> tΔ1) => {
        {
            var allocs = testing.AllocsPerRun(100, () => {
                var p = nistec.NewP224Point().SetGenerator();
                var scalar = new slice<byte>(28);
                rand.Read(scalar);
                p.ScalarBaseMult(scalar);
                p.ScalarMult(p, scalar);
                var @out = p.Bytes();
                {
                    var (_, err) = nistec.NewP224Point().SetBytes(@out); if (err != nil) {
                        tΔ1.Fatal(err);
                    }
                }
                @out = p.BytesCompressed();
                {
                    var (_, err) = p.SetBytes(@out); if (err != nil) {
                        tΔ1.Fatal(err);
                    }
                }
            }); if (allocs > 0D) {
                tΔ1.Errorf("expected zero allocations, got %0.1f", allocs);
            }
        }
    });
    t.Run(p256ˢ, (ж<testing.T> tΔ2) => {
        {
            var allocs = testing.AllocsPerRun(100, () => {
                var p = nistec.NewP256Point().SetGenerator();
                var scalar = new slice<byte>(32);
                rand.Read(scalar);
                p.ScalarBaseMult(scalar);
                p.ScalarMult(p, scalar);
                var @out = p.Bytes();
                {
                    var (_, err) = nistec.NewP256Point().SetBytes(@out); if (err != nil) {
                        tΔ2.Fatal(err);
                    }
                }
                @out = p.BytesCompressed();
                {
                    var (_, err) = p.SetBytes(@out); if (err != nil) {
                        tΔ2.Fatal(err);
                    }
                }
            }); if (allocs > 0D) {
                tΔ2.Errorf("expected zero allocations, got %0.1f", allocs);
            }
        }
    });
    t.Run(p384ˢ, (ж<testing.T> tΔ3) => {
        {
            var allocs = testing.AllocsPerRun(100, () => {
                var p = nistec.NewP384Point().SetGenerator();
                var scalar = new slice<byte>(48);
                rand.Read(scalar);
                p.ScalarBaseMult(scalar);
                p.ScalarMult(p, scalar);
                var @out = p.Bytes();
                {
                    var (_, err) = nistec.NewP384Point().SetBytes(@out); if (err != nil) {
                        tΔ3.Fatal(err);
                    }
                }
                @out = p.BytesCompressed();
                {
                    var (_, err) = p.SetBytes(@out); if (err != nil) {
                        tΔ3.Fatal(err);
                    }
                }
            }); if (allocs > 0D) {
                tΔ3.Errorf("expected zero allocations, got %0.1f", allocs);
            }
        }
    });
    t.Run(p521ˢ, (ж<testing.T> tΔ4) => {
        {
            var allocs = testing.AllocsPerRun(100, () => {
                var p = nistec.NewP521Point().SetGenerator();
                var scalar = new slice<byte>(66);
                rand.Read(scalar);
                p.ScalarBaseMult(scalar);
                p.ScalarMult(p, scalar);
                var @out = p.Bytes();
                {
                    var (_, err) = nistec.NewP521Point().SetBytes(@out); if (err != nil) {
                        tΔ4.Fatal(err);
                    }
                }
                @out = p.BytesCompressed();
                {
                    var (_, err) = p.SetBytes(@out); if (err != nil) {
                        tΔ4.Fatal(err);
                    }
                }
            }); if (allocs > 0D) {
                tΔ4.Errorf("expected zero allocations, got %0.1f", allocs);
            }
        }
    });
}

internal partial interface nistPoint<T> {
    slice<byte> Bytes();
    T SetGenerator();
    (T, error) SetBytes(slice<byte> _Δp0);
    T Add(T _Δp0, T _Δp1);
    T Double(T _Δp0);
    (T, error) ScalarMult(T _Δp0, slice<byte> _Δp1);
    (T, error) ScalarBaseMult(slice<byte> _Δp0);
}

public static void TestEquivalents(ж<testing.T> t) {
    t.Run(p224ˢ, (ж<testing.T> tΔ1) => {
        testEquivalents<P224PointжnistPoint>(tΔ1, () => nistec.NewP224Point(), elliptic.P224());
    });
    t.Run(p256ˢ, (ж<testing.T> tΔ2) => {
        testEquivalents<P256PointжnistPoint>(tΔ2, () => nistec.NewP256Point(), elliptic.P256());
    });
    t.Run(p384ˢ, (ж<testing.T> tΔ3) => {
        testEquivalents<P384PointжnistPoint>(tΔ3, () => nistec.NewP384Point(), elliptic.P384());
    });
    t.Run(p521ˢ, (ж<testing.T> tΔ4) => {
        testEquivalents<P521PointжnistPoint>(tΔ4, () => nistec.NewP521Point(), elliptic.P521());
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object pP2Pˢ = (@string)"P+P != 2*P"u8;
internal static readonly object pP2Pˢ2 = (@string)"P+P != [2]P"u8;
internal static readonly object gG2Gˢ = (@string)"G+G != [2]G"u8;
internal static readonly object pPN2Pˢ = (@string)"P+P != [N+2]P"u8;
internal static readonly object gGN2Gˢ = (@string)"G+G != [N+2]G"u8;

internal static void testEquivalents<P>(ж<testing.T> t, Func<P> newPoint, elliptic.Curve c)
    where P : nistPoint<P>
{
    var p = newPoint().SetGenerator();

    nint elementSize = (c.Params().BitSize + 7) / 8;
    var two = new slice<byte>(elementSize);
    two[len(two) - 1] = 2;
    var nPlusTwo = new slice<byte>(elementSize);
    @new<bigꓸInt>().Add(c.Params().N, big.NewInt(2)).FillBytes(nPlusTwo);

    var p1 = newPoint().Double(p);
    var p2 = newPoint().Add(p, p);
    var (p3, err) = newPoint().ScalarMult(p, two);
    fatalIfErr(t, err);
    (var p4, err) = newPoint().ScalarBaseMult(two);
    fatalIfErr(t, err);
    (var p5, err) = newPoint().ScalarMult(p, nPlusTwo);
    fatalIfErr(t, err);
    (var p6, err) = newPoint().ScalarBaseMult(nPlusTwo);
    fatalIfErr(t, err);

    if (!bytes.Equal(p1.Bytes(), p2.Bytes())) {
        t.Error(pP2Pˢ);
    }
    if (!bytes.Equal(p1.Bytes(), p3.Bytes())) {
        t.Error(pP2Pˢ2);
    }
    if (!bytes.Equal(p1.Bytes(), p4.Bytes())) {
        t.Error(gG2Gˢ);
    }
    if (!bytes.Equal(p1.Bytes(), p5.Bytes())) {
        t.Error(pPN2Pˢ);
    }
    if (!bytes.Equal(p1.Bytes(), p6.Bytes())) {
        t.Error(gGN2Gˢ);
    }
}

public static void TestScalarMult(ж<testing.T> t) {
    t.Run(p224ˢ, (ж<testing.T> tΔ1) => {
        testScalarMult<P224PointжnistPoint>(tΔ1, () => nistec.NewP224Point(), elliptic.P224());
    });
    t.Run(p256ˢ, (ж<testing.T> tΔ2) => {
        testScalarMult<P256PointжnistPoint>(tΔ2, () => nistec.NewP256Point(), elliptic.P256());
    });
    t.Run(p384ˢ, (ж<testing.T> tΔ3) => {
        testScalarMult<P384PointжnistPoint>(tΔ3, () => nistec.NewP384Point(), elliptic.P384());
    });
    t.Run(p521ˢ, (ж<testing.T> tΔ4) => {
        testScalarMult<P521PointжnistPoint>(tΔ4, () => nistec.NewP521Point(), elliptic.P521());
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object kGScalarBaseMultKˢ = (@string)"[k]G != ScalarBaseMult(k)"u8;
internal static readonly object scalarBaseMultKˢ = (@string)"ScalarBaseMult(k) != ∞"u8;
internal static readonly object scalarBaseMultKˢ2 = (@string)"ScalarBaseMult(k) == ∞"u8;
internal static readonly object nKGKGˢ = (@string)"[N - k]G + [k]G != ∞"u8;
internal static ref readonly @string all1sˢ => ref all1sˢᵇ;
private static readonly @string all1sˢᵇ = "all1s"u8;

internal static void testScalarMult<P>(ж<testing.T> t, Func<P> newPoint, elliptic.Curve c)
    where P : nistPoint<P>
{
    var G = newPoint().SetGenerator();
    var Gʗ1 = G;
    void checkScalar(ж<testing.T> tΔ1, slice<byte> scalar) {
        var (p1, err) = newPoint().ScalarBaseMult(scalar);
        fatalIfErr(tΔ1, err);
        (var p2, err) = newPoint().ScalarMult(Gʗ1, scalar);
        fatalIfErr(tΔ1, err);
        if (!bytes.Equal(p1.Bytes(), p2.Bytes())) {
            tΔ1.Error(kGScalarBaseMultKˢ);
        }

        var expectInfinity = @new<bigꓸInt>().Mod(@new<bigꓸInt>().SetBytes(scalar), c.Params().N).Sign() == 0;
        if (expectInfinity) {
            if (!bytes.Equal(p1.Bytes(), newPoint().Bytes())) {
                tΔ1.Error(scalarBaseMultKˢ);
            }
            if (!bytes.Equal(p2.Bytes(), newPoint().Bytes())) {
                tΔ1.Error((@string)"[k]G != ∞");
            }
        } else {
            if (bytes.Equal(p1.Bytes(), newPoint().Bytes())) {
                tΔ1.Error(scalarBaseMultKˢ2);
            }
            if (bytes.Equal(p2.Bytes(), newPoint().Bytes())) {
                tΔ1.Error((@string)"[k]G == ∞");
            }
        }

        var d = @new<bigꓸInt>().SetBytes(scalar);
        d.Sub(c.Params().N, d);
        d.Mod(d, c.Params().N);
        (var g1, err) = newPoint().ScalarBaseMult(d.FillBytes(new slice<byte>(len(scalar))));
        fatalIfErr(tΔ1, err);
        g1.Add(g1, p1);
        if (!bytes.Equal(g1.Bytes(), newPoint().Bytes())) {
            tΔ1.Error(nKGKGˢ);
        }
    }

    nint byteLen = len(c.Params().N.Bytes());
    nint bitLen = c.Params().N.BitLen();
    var checkScalarʗ1 = checkScalar;
    t.Run("0", (ж<testing.T> tΔ2) => {
        checkScalarʗ1(tΔ2, new slice<byte>(byteLen));
    });
    var checkScalarʗ2 = checkScalar;
    t.Run("1", (ж<testing.T> tΔ3) => {
        checkScalarʗ2(tΔ3, big.NewInt(1).FillBytes(new slice<byte>(byteLen)));
    });
    var checkScalarʗ3 = checkScalar;
    t.Run("N-1", (ж<testing.T> tΔ4) => {
        checkScalarʗ3(tΔ4, @new<bigꓸInt>().Sub(c.Params().N, big.NewInt(1)).Bytes());
    });
    var checkScalarʗ4 = checkScalar;
    t.Run("N", (ж<testing.T> tΔ5) => {
        checkScalarʗ4(tΔ5, c.Params().N.Bytes());
    });
    var checkScalarʗ5 = checkScalar;
    t.Run("N+1", (ж<testing.T> tΔ6) => {
        checkScalarʗ5(tΔ6, @new<bigꓸInt>().Add(c.Params().N, big.NewInt(1)).Bytes());
    });
    var checkScalarʗ6 = checkScalar;
    t.Run(all1sˢ, (ж<testing.T> tΔ7) => {
        var s = @new<bigꓸInt>().Lsh(big.NewInt(1), (nuint)bitLen);
        s.Sub(s, big.NewInt(1));
        checkScalarʗ6(tΔ7, s.Bytes());
    });
    if (testing.Short()) {
        return;
    }
    for (nint iᴛ1 = 0; iᴛ1 < bitLen; iᴛ1++) {
        var i = iᴛ1;
        var checkScalarʗ7 = checkScalar;
        t.Run(fmt.Sprintf("1<<%d"u8, i), (ж<testing.T> tΔ8) => {
            var s = @new<bigꓸInt>().Lsh(big.NewInt(1), (nuint)i);
            checkScalarʗ7(tΔ8, s.FillBytes(new slice<byte>(byteLen)));
        });
    }
    for (nint iᴛ2 = 0; iᴛ2 <= 64; iᴛ2++) {
        var i = iᴛ2;
        var checkScalarʗ8 = checkScalar;
        t.Run(fmt.Sprintf("%d"u8, i), (ж<testing.T> tΔ9) => {
            checkScalarʗ8(tΔ9, big.NewInt((int64)i).FillBytes(new slice<byte>(byteLen)));
        });
    }
    // Test N-64...N+64 since they risk overlapping with precomputed table values
    // in the final additions.
    for (var iᴛ3 = (int64)(-64); iᴛ3 <= 64; iᴛ3++) {
        var i = iᴛ3;
        var checkScalarʗ9 = checkScalar;
        t.Run(fmt.Sprintf("N%+d"u8, i), (ж<testing.T> tΔ10) => {
            checkScalarʗ9(tΔ10, @new<bigꓸInt>().Add(c.Params().N, big.NewInt(i)).Bytes());
        });
    }
}

} // end fipstest_internal_test_package
