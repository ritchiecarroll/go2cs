// SignedDivisionTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Guards golib's <c>quo</c> and <c>rem</c>, the signed division and remainder the converter emits
/// where Go must check a variable divisor for -1 at run time.
/// </summary>
/// <remarks>
/// Go wraps <c>MinInt / -1</c> to <c>MinInt</c> and makes <c>MinInt % -1</c> zero, with no panic; the
/// plain C# operators throw <see cref="OverflowException"/> there, which each control arm proves.
/// Division by zero must still throw <see cref="DivideByZeroException"/>, which golib reports as Go's
/// integer divide-by-zero panic.
/// </remarks>
[TestClass]
public class SignedDivisionTests
{
    // Read through a field so the compiler cannot fold the -1 into a constant expression
    private static int s_minusOne = -1;
    private static int s_zero = 0;

    [TestMethod]
    public void QuoWrapsTheMinimumDividedByMinusOne()
    {
        Assert.AreEqual(nint.MinValue, quo(nint.MinValue, (nint)s_minusOne));
        Assert.AreEqual(int32.MinValue, quo(int32.MinValue, (int32)s_minusOne));
        Assert.AreEqual(int64.MinValue, quo(int64.MinValue, (int64)s_minusOne));
    }

    [TestMethod]
    public void RemOfTheMinimumByMinusOneIsZero()
    {
        Assert.AreEqual((nint)0, rem(nint.MinValue, (nint)s_minusOne));
        Assert.AreEqual(0, rem(int32.MinValue, (int32)s_minusOne));
        Assert.AreEqual(0L, rem(int64.MinValue, (int64)s_minusOne));
    }

    [TestMethod]
    public void ThePlainOperatorsThrowWhereGoWraps()
    {
        int64 minusOne = s_minusOne;
        Assert.ThrowsException<OverflowException>(() => int64.MinValue / minusOne);
        Assert.ThrowsException<OverflowException>(() => int64.MinValue % minusOne);
    }

    [TestMethod]
    public void OrdinaryDivisorsTruncateTowardZeroLikeGo()
    {
        Assert.AreEqual((nint)(-3), quo((nint)(-7), (nint)2));
        Assert.AreEqual((nint)(-1), rem((nint)(-7), (nint)2));
        Assert.AreEqual(3, quo(7, 2));
        Assert.AreEqual(-1, rem(-7, 2));
        Assert.AreEqual(7L, quo(-7L, (int64)s_minusOne));
        Assert.AreEqual(0L, rem(-7L, (int64)s_minusOne));
    }

    [TestMethod]
    public void DivisionByZeroStillThrows()
    {
        Assert.ThrowsException<DivideByZeroException>(() => quo((nint)1, (nint)s_zero));
        Assert.ThrowsException<DivideByZeroException>(() => rem(1L, (int64)s_zero));
    }

    [TestMethod]
    public void TheGenericFormsCarryTheMinusOneArmForTheGovernedTypes()
    {
        // A type-parameter division (`func div[T ~int64](a, b T) T { return a / b }`) instantiated
        // with an unnamed governed type.
        Assert.AreEqual(int64.MinValue, quo<int64>(int64.MinValue, s_minusOne));
        Assert.AreEqual(int32.MinValue, quo<int32>(int32.MinValue, s_minusOne));
        Assert.AreEqual(nint.MinValue, quo<nint>(nint.MinValue, s_minusOne));
        Assert.AreEqual(0L, rem<int64>(int64.MinValue, s_minusOne));
        Assert.AreEqual(0, rem<int32>(int32.MinValue, s_minusOne));
        Assert.AreEqual((nint)0, rem<nint>(nint.MinValue, s_minusOne));

        // Any other type divides with its own operator: a narrower signed type cannot overflow here,
        // and an unsigned one has no -1.
        Assert.AreEqual(int16.MinValue, quo<int16>(int16.MinValue, (int16)s_minusOne));
        Assert.AreEqual((uint8)66, quo<uint8>(200, 3));
        Assert.AreEqual((uint8)2, rem<uint8>(200, 3));
        Assert.ThrowsException<DivideByZeroException>(() => quo<int64>(1, s_zero));
    }

    [TestMethod]
    public void TheCompoundTwinsEvaluateTheTargetOnceAndWrap()
    {
        // `a[f()] /= b` and `*p %= b`: the `ref this` twin binds the element itself.
        int64[] longs = [int64.MinValue, int64.MinValue];
        int reads = 0;
        longs[Index(ref reads, 0)].QuoAssign(s_minusOne);
        longs[1].RemAssign(s_minusOne);
        Assert.AreEqual(int64.MinValue, longs[0]);
        Assert.AreEqual(0L, longs[1]);
        Assert.AreEqual(1, reads, "the index is evaluated once");

        int32[] ints = [int32.MinValue, int32.MinValue];
        ints[0].QuoAssign(s_minusOne);
        ints[1].RemAssign(s_minusOne);
        Assert.AreEqual(int32.MinValue, ints[0]);
        Assert.AreEqual(0, ints[1]);

        nint[] nints = [nint.MinValue, 7];
        nints[0].QuoAssign(s_minusOne);
        nints[1].RemAssign(2);
        Assert.AreEqual(nint.MinValue, nints[0]);
        Assert.AreEqual((nint)1, nints[1]);
    }

    [TestMethod]
    public void TheMapTwinsEvaluateTheKeyOnceAndWrap()
    {
        // `m[key()] /= b`: a map element has no ref, so the twin takes the map and the key.
        map<@string, int64> m = new() { ["k"u8] = int64.MinValue, ["r"u8] = int64.MinValue };
        int reads = 0;
        m.QuoAssign(Key(ref reads, "k"u8), s_minusOne);
        m.RemAssign(Key(ref reads, "r"u8), s_minusOne);
        Assert.AreEqual(int64.MinValue, m["k"u8]);
        Assert.AreEqual(0L, m["r"u8]);
        Assert.AreEqual(2, reads, "each key is evaluated once");

        // A missing key reads as zero and is then written, as Go's `m[k] /= b` does.
        map<@string, nint> n = new() { ["present"u8] = 7 };
        n.QuoAssign("absent"u8, 3);
        Assert.AreEqual((nint)0, n["absent"u8]);
        Assert.AreEqual((nint)2, (nint)len(n));
    }

    private static int Index(ref int reads, int index)
    {
        reads++;
        return index;
    }

    private static @string Key(ref int reads, @string key)
    {
        reads++;
        return key;
    }
}
