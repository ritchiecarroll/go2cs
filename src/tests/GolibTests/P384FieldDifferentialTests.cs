using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.crypto.@internal.fips140.nistec;
using static go.crypto.@internal.fips140.nistec.fiat_package;
using fiat = go.crypto.@internal.fips140.nistec.fiat_package;

namespace GolibTests;

[TestClass]
public class P384FieldDifferentialTests
{
    // WHY THIS LIVES IN GolibTests, and why it exists at all.
    //
    // crypto/internal/fips140/nistec/fiat's p384Mul and p384Square are hand-owned
    // (manualConversionFuncs, fiat/p384_impl.cs): the converted fiat-crypto bodies start at about 460
    // locals, so the JIT stops inlining into them after about 74 callees, and every other 64-bit
    // multiply, carry add, p384Uint1 conversion and limb read stays a call (590 per field multiply);
    // under ReadyToRun no math/bits call inlines at all. A P-384 field multiply took 2.07 us under the JIT
    // and 5.06 us under ReadyToRun against Go's 77 ns (docs/phase4/DATA-p384-field.md). The replacement is
    // hand-written arithmetic in place of fiat-crypto's formally verified output, so it is checked here
    // against an oracle with no dependency on this repository: System.Numerics.BigInteger.
    //
    // fiat's contract for both functions: inputs are Montgomery-domain elements strictly below
    // p = 2^384 - 2^128 - 2^96 + 2^32 - 1, and the output is (arg1 * arg2 * R^-1) mod p, fully reduced,
    // with R = 2^384. The public element API reaches it: SetBytes converts a canonical value c into the
    // Montgomery limbs c * R mod p, Mul calls p384Mul, and Bytes converts back. So
    // Mul(x, y).Bytes() == x * y mod p is the contract read through a bijection, and an input whose
    // Montgomery LIMBS are an edge value L is the canonical value L * R^-1 mod p.
    //
    // The edges are every boundary fiat's input contract admits, stated as Montgomery limbs: 0, 1, 2,
    // p - 1, p - 2, R mod p, R^2 mod p, a single all-ones limb, all-ones limbs below the top limb, p's own
    // low-limb pattern with the top limb cleared, and 2^383. Every ordered pair of them is multiplied,
    // every one is squared, and 20,000 seeded random pairs follow. The aliased form the point formulas use,
    // `t.Mul(t, u)`, is checked as well, since the hand-owned body must read its inputs before it writes.

    private static readonly BigInteger P = (BigInteger.One << 384) - (BigInteger.One << 128) - (BigInteger.One << 96) + (BigInteger.One << 32) - 1;
    private static readonly BigInteger R = BigInteger.One << 384;
    private static readonly BigInteger RInv = BigInteger.ModPow(R % P, P - 2, P);

    private static slice<byte> encode(BigInteger v)
    {
        byte[] le = v.ToByteArray(isUnsigned: true, isBigEndian: false);
        byte[] be = new byte[48];

        for (int i = 0; i < le.Length && i < 48; i++)
            be[47 - i] = le[i];

        return new slice<byte>(be);
    }

    private static BigInteger decode(slice<byte> b)
    {
        byte[] be = new byte[b.Length];

        for (nint i = 0; i < b.Length; i++)
            be[i] = b[i];

        return new BigInteger(be, isUnsigned: true, isBigEndian: true);
    }

    private static ж<fiat.P384Element> element(BigInteger canonical)
    {
        var (e, err) = builtin.@new<fiat.P384Element>().SetBytes(encode(canonical));
        Assert.IsNull(err, $"SetBytes refused {canonical:X}");
        return e;
    }

    // The canonical value whose Montgomery limbs are `limbs`.
    private static BigInteger withLimbs(BigInteger limbs) => limbs * RInv % P;

    private static IEnumerable<(string name, BigInteger canonical)> edges()
    {
        BigInteger allOnesBelowTop = (BigInteger.One << 320) - 1;

        yield return ("limbs 0", withLimbs(0));
        yield return ("limbs 1", withLimbs(1));
        yield return ("limbs 2", withLimbs(2));
        yield return ("limbs p-1", withLimbs(P - 1));
        yield return ("limbs p-2", withLimbs(P - 2));
        yield return ("limbs R mod p", withLimbs(R % P));
        yield return ("limbs R^2 mod p", withLimbs(R * R % P));
        yield return ("limbs 2^64-1", withLimbs(ulong.MaxValue));
        yield return ("limbs 2^320-1", withLimbs(allOnesBelowTop));
        yield return ("limbs p mod 2^320", withLimbs(P & allOnesBelowTop));
        yield return ("limbs 2^383", withLimbs(BigInteger.One << 383));
    }

    private static void check(string what, BigInteger x, BigInteger y)
    {
        var product = builtin.@new<fiat.P384Element>().Mul(element(x), element(y));
        Assert.AreEqual(x * y % P, decode(product.Bytes()), $"Mul {what}");
    }

    private static void checkSquare(string what, BigInteger x)
    {
        var square = builtin.@new<fiat.P384Element>().Square(element(x));
        Assert.AreEqual(x * x % P, decode(square.Bytes()), $"Square {what}");
    }

    [TestMethod]
    public void EdgePairsMatchBigInteger()
    {
        int checkedPairs = 0;

        foreach (var (xn, x) in edges())
        {
            checkSquare(xn, x);

            foreach (var (yn, y) in edges())
            {
                check($"{xn} * {yn}", x, y);
                checkedPairs++;
            }
        }

        Assert.AreEqual(121, checkedPairs, "VACUOUS: the edge set did not enumerate");
    }

    [TestMethod]
    public void RandomPairsMatchBigInteger()
    {
        Random rng = new(384);
        byte[] buf = new byte[56];

        for (int n = 0; n < 20_000; n++)
        {
            rng.NextBytes(buf);
            BigInteger x = new BigInteger(buf, isUnsigned: true) % P;
            rng.NextBytes(buf);
            BigInteger y = new BigInteger(buf, isUnsigned: true) % P;

            check($"random pair {n}", x, y);

            if (n % 16 == 0)
                checkSquare($"random {n}", x);
        }
    }

    [TestMethod]
    public void AliasedOperandsMatchBigInteger()
    {
        Random rng = new(1384);
        byte[] buf = new byte[56];

        for (int n = 0; n < 500; n++)
        {
            rng.NextBytes(buf);
            BigInteger x = new BigInteger(buf, isUnsigned: true) % P;
            rng.NextBytes(buf);
            BigInteger y = new BigInteger(buf, isUnsigned: true) % P;

            // out1 aliases arg1, as the point formulas' `t3.Mul(t3, t4)` does.
            var t = element(x);
            t.Mul(t, element(y));
            Assert.AreEqual(x * y % P, decode(t.Bytes()), $"aliased Mul {n}");

            // out1 aliases both arguments.
            var u = element(x);
            u.Mul(u, u);
            Assert.AreEqual(x * x % P, decode(u.Bytes()), $"fully aliased Mul {n}");

            var v = element(y);
            v.Square(v);
            Assert.AreEqual(y * y % P, decode(v.Bytes()), $"aliased Square {n}");
        }
    }

    // The tests above read VALUES through Bytes(), so a result that is congruent but NOT fully reduced
    // would still pass them. This one reads the RAW Montgomery limbs and requires exactly
    // (a * b * R^-1) mod p, which is below p.
    //
    // It needs inputs whose pre-reduction t lands in [p, 2p), where only the final select brings the
    // result below p. With y = (a * b * R^-1) mod p, the CIOS loop ends at t = (a * b + m * p) / R for
    // the unique m in [0, R). t = y + p exactly when y * R < a * b, so the result must be tiny against
    // the product. Limbs p - 1 and p - (R mod p) multiply to y = 1, since (-1) * (-R) = R; and
    // (2^192)^2 = R, so limbs p - 2^192 square to y = 1 as well. The test counts the cases it reaches and
    // refuses to pass vacuously.
    private static readonly FieldInfo limbsField =
        typeof(fiat.P384Element).GetField("x", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static BigInteger rawLimbs(ж<fiat.P384Element> e)
    {
        var limbs = (IArray<ulong>)limbsField.GetValue(e.Value)!;
        BigInteger v = BigInteger.Zero;

        for (int i = 5; i >= 0; i--)
            v = (v << 64) | limbs.Source[i];

        return v;
    }

    [TestMethod]
    public void RawLimbsAreFullyReduced()
    {
        BigInteger[] limbEdges =
        [
            0, 1, 2, P - 1, P - 2, R % P, R * R % P, P - (R % P), P - 2 * (R % P), P - (BigInteger.One << 192),
            (BigInteger.One << 192), (BigInteger.One << 383), (BigInteger.One << 320) - 1,
        ];

        int upperHalf = 0, squaresInUpperHalf = 0;

        foreach (BigInteger a in limbEdges)
        {
            var square = builtin.@new<fiat.P384Element>().Square(element(withLimbs(a)));
            BigInteger ySquare = a * a * RInv % P;
            Assert.AreEqual(ySquare, rawLimbs(square), $"Square raw limbs, limbs {a:X}");

            if (ySquare * R < a * a)
                squaresInUpperHalf++;

            foreach (BigInteger b in limbEdges)
            {
                var product = builtin.@new<fiat.P384Element>().Mul(element(withLimbs(a)), element(withLimbs(b)));
                BigInteger y = a * b * RInv % P;
                Assert.AreEqual(y, rawLimbs(product), $"Mul raw limbs, limbs {a:X} * {b:X}");

                if (y * R < a * b)
                    upperHalf++;
            }
        }

        Assert.IsTrue(upperHalf >= 4, $"VACUOUS: only {upperHalf} Mul pairs reach t in [p, 2p)");
        Assert.IsTrue(squaresInUpperHalf >= 1, $"VACUOUS: only {squaresInUpperHalf} squares reach t in [p, 2p)");
    }
}
