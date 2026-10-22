// MathBitsLeafTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using bits = go.math.bits_package;

namespace GolibTests;

/// <summary>
/// math/bits' 64-bit leaves, <c>Add64</c>, <c>Sub64</c> and <c>Mul64</c>, hand-owned in
/// <c>core/math/bits/bits_impl.cs</c>: <c>Mul64</c> is one <see cref="Math.BigMul(ulong, ulong, out ulong)"/> and all
/// three carry <see cref="MethodImplOptions.AggressiveInlining"/>, the C# form of the intrinsics Go's compiler
/// substitutes for them.
/// </summary>
/// <remarks>
/// <para>
/// Fiat field arithmetic calls these leaves dozens of times per method (84 calls in one P-256 field multiply). A
/// caller that large exhausts the JIT's inlining budget, and only the attribute keeps the leaves inlined there, so
/// <see cref="TheLeavesCarryAggressiveInlining"/> guards it: the converted bodies carried none.
/// </para>
/// <para>
/// The value arms hold every leaf to Go's definition, the exact 128-bit sum, difference and product, computed here
/// independently with <see cref="UInt128"/>, over the edges (zero, one, all ones, the top bit, a carry or borrow in
/// and out) and a fixed pseudo-random sweep. The MathBitsLeaves behavioral fixture prints the same edges beside Go.
/// </para>
/// </remarks>
[TestClass]
public class MathBitsLeafTests
{
    private static readonly ulong[] s_edges =
    [
        0UL, 1UL, 2UL, 0x7FFF_FFFF_FFFF_FFFFUL, 0x8000_0000_0000_0000UL, 0x8000_0000_0000_0001UL,
        0xFFFF_FFFF_FFFF_FFFEUL, ulong.MaxValue, 0x0000_0000_FFFF_FFFFUL, 0xFFFF_FFFF_0000_0000UL,
        0x5555_5555_5555_5555UL, 0xAAAA_AAAA_AAAA_AAAAUL, 0xFFFF_FFFF_0000_0001UL
    ];

    [TestMethod]
    public void TheLeavesCarryAggressiveInlining()
    {
        foreach (string name in new[] { "Add64", "Sub64", "Mul64" })
        {
            MethodInfo method = typeof(bits).GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, $"bits.{name} not found");
            Assert.IsTrue(method.MethodImplementationFlags.HasFlag(MethodImplAttributes.AggressiveInlining),
                $"bits.{name} must carry AggressiveInlining: an over-budget caller (a fiat field multiply) leaves it a call otherwise");
        }
    }

    [TestMethod]
    public void Add64MatchesTheExactSumAtTheEdges()
    {
        foreach (ulong x in s_edges)
        foreach (ulong y in s_edges)
        foreach (ulong carry in new[] { 0UL, 1UL })
            CheckAdd(x, y, carry);
    }

    [TestMethod]
    public void Sub64MatchesTheExactDifferenceAtTheEdges()
    {
        foreach (ulong x in s_edges)
        foreach (ulong y in s_edges)
        foreach (ulong borrow in new[] { 0UL, 1UL })
            CheckSub(x, y, borrow);
    }

    [TestMethod]
    public void Mul64MatchesTheExactProductAtTheEdges()
    {
        foreach (ulong x in s_edges)
        foreach (ulong y in s_edges)
            CheckMul(x, y);
    }

    [TestMethod]
    public void TheLeavesMatchTheExactResultsOverASweep()
    {
        Random random = new(20261022);

        for (int i = 0; i < 100_000; i++)
        {
            ulong x = (ulong)random.NextInt64() ^ ((ulong)random.Next() << 63);
            ulong y = (ulong)random.NextInt64() ^ ((ulong)random.Next() << 63);
            ulong bit = (ulong)(i & 1);

            CheckAdd(x, y, bit);
            CheckSub(x, y, bit);
            CheckMul(x, y);
        }
    }

    private static void CheckAdd(ulong x, ulong y, ulong carry)
    {
        UInt128 exact = (UInt128)x + y + carry;
        (ulong sum, ulong carryOut) = bits.Add64(x, y, carry);
        Assert.AreEqual((ulong)exact, sum, $"Add64({x:X}, {y:X}, {carry}) sum");
        Assert.AreEqual((ulong)(exact >> 64), carryOut, $"Add64({x:X}, {y:X}, {carry}) carryOut");
    }

    private static void CheckSub(ulong x, ulong y, ulong borrow)
    {
        UInt128 exact = (UInt128)x - y - borrow;
        (ulong diff, ulong borrowOut) = bits.Sub64(x, y, borrow);
        Assert.AreEqual((ulong)exact, diff, $"Sub64({x:X}, {y:X}, {borrow}) diff");
        Assert.AreEqual((ulong)(exact >> 64) & 1, borrowOut, $"Sub64({x:X}, {y:X}, {borrow}) borrowOut");
    }

    private static void CheckMul(ulong x, ulong y)
    {
        UInt128 exact = (UInt128)x * y;
        (ulong hi, ulong lo) = bits.Mul64(x, y);
        Assert.AreEqual((ulong)(exact >> 64), hi, $"Mul64({x:X}, {y:X}) hi");
        Assert.AreEqual((ulong)exact, lo, $"Mul64({x:X}, {y:X}) lo");
    }
}
