using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.crypto.@internal.fips140.nistec.fiat_package;
using fiat = go.crypto.@internal.fips140.nistec.fiat_package;

namespace GolibTests;

[TestClass]
public class P521FieldDifferentialTests
{
    // WHY THIS LIVES IN GolibTests, and why it exists at all.
    //
    // crypto/internal/fips140/nistec/fiat's p521Mul and p521Square are hand-owned
    // (manualConversionFuncs, fiat/p521_impl.cs): the converted fiat-crypto bodies declare 1,006 locals,
    // past the JIT's 512-local inlining cut-off, so every 64-bit multiply, carry add and limb read stayed
    // a call (1,250 per field multiply) and the P-521 generator-table build cost 523 ms under ReadyToRun
    // against Go's 6 ms (docs/phase4/DATA-p521-table.md). The replacement is hand-written arithmetic in
    // place of fiat-crypto's formally verified output, so it is checked here against an oracle with no
    // dependency on this repository: System.Numerics.BigInteger.
    //
    // fiat's contract for both functions: inputs are Montgomery-domain elements strictly below
    // p = 2^521 - 1, and the output is (arg1 * arg2 * R^-1) mod p, fully reduced, with R = 2^576. The
    // public element API reaches it: SetBytes converts a canonical value c into the Montgomery limbs
    // c * R mod p, Mul calls p521Mul, and Bytes converts back. So Mul(x, y).Bytes() == x * y mod p is the
    // contract read through a bijection, and an input whose Montgomery LIMBS are an edge value L is the
    // canonical value L * R^-1 mod p.
    //
    // The edges are every boundary fiat's input contract admits, stated as Montgomery limbs: 0, 1, 2,
    // p - 1, p - 2, R mod p, R^2 mod p, a single all-ones limb, all-ones limbs below the top limb, the
    // all-ones pattern with top limb 0x1fe, and 2^520. Every ordered pair of them is multiplied, every one
    // is squared, and 20,000 seeded random pairs follow. The aliased form the point formulas use,
    // `t.Mul(t, u)`, is checked as well, since the hand-owned body must read its inputs before it writes.

    private static readonly BigInteger P = (BigInteger.One << 521) - 1;
    private static readonly BigInteger R = BigInteger.One << 576;
    private static readonly BigInteger RInv = BigInteger.ModPow(R % P, P - 2, P);

    private static slice<byte> encode(BigInteger v)
    {
        byte[] le = v.ToByteArray(isUnsigned: true, isBigEndian: false);
        byte[] be = new byte[66];

        for (int i = 0; i < le.Length && i < 66; i++)
            be[65 - i] = le[i];

        return new slice<byte>(be);
    }

    private static BigInteger decode(slice<byte> b)
    {
        byte[] be = new byte[b.Length];

        for (nint i = 0; i < b.Length; i++)
            be[i] = b[i];

        return new BigInteger(be, isUnsigned: true, isBigEndian: true);
    }

    private static ж<fiat.P521Element> element(BigInteger canonical)
    {
        var (e, err) = builtin.@new<fiat.P521Element>().SetBytes(encode(canonical));
        Assert.IsNull(err, $"SetBytes refused {canonical:X}");
        return e;
    }

    // The canonical value whose Montgomery limbs are `limbs`.
    private static BigInteger withLimbs(BigInteger limbs) => limbs * RInv % P;

    private static IEnumerable<(string name, BigInteger canonical)> edges()
    {
        BigInteger allOnesBelowTop = (BigInteger.One << 512) - 1;

        yield return ("limbs 0", withLimbs(0));
        yield return ("limbs 1", withLimbs(1));
        yield return ("limbs 2", withLimbs(2));
        yield return ("limbs p-1", withLimbs(P - 1));
        yield return ("limbs p-2", withLimbs(P - 2));
        yield return ("limbs R mod p", withLimbs(R % P));
        yield return ("limbs R^2 mod p", withLimbs(R * R % P));
        yield return ("limbs 2^64-1", withLimbs(ulong.MaxValue));
        yield return ("limbs 2^512-1", withLimbs(allOnesBelowTop));
        yield return ("limbs 0x1fe ++ ones", withLimbs((new BigInteger(0x1fe) << 512) | allOnesBelowTop));
        yield return ("limbs 2^520", withLimbs(BigInteger.One << 520));
    }

    private static void check(string what, BigInteger x, BigInteger y)
    {
        var product = builtin.@new<fiat.P521Element>().Mul(element(x), element(y));
        Assert.AreEqual(x * y % P, decode(product.Bytes()), $"Mul {what}");
    }

    private static void checkSquare(string what, BigInteger x)
    {
        var square = builtin.@new<fiat.P521Element>().Square(element(x));
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
        Random rng = new(521);
        byte[] buf = new byte[72];

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
        Random rng = new(1521);
        byte[] buf = new byte[72];

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
}
