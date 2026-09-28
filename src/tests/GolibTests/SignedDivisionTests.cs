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
}
