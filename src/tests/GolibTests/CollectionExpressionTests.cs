// CollectionExpressionTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

/// <summary>
/// Guards B3 (docs/PLAN-collection-expressions.md, ruled 2026-11-05): a Go slice literal where C# knows the target type
/// is a C# collection expression, <c>slice&lt;nint&gt; s = [1, 2, 3];</c>, which needs a slice and an array to
/// enumerate their VALUES. Go's range pairs then have their own door, <c>foreach (var (i, v) in range(s))</c>, and the
/// index-only range is <c>foreach (var i in range(len(s)))</c>.
/// </summary>
[TestClass]
public class CollectionExpressionTests
{
    [TestMethod]
    public void ACollectionExpressionBuildsTheSliceGosLiteralDoes()
    {
        slice<nint> s = [1, 2, 3];
        slice<nint> wrapped = slice<nint>([1, 2, 3]);

        Assert.AreEqual((nint)3, len(s));
        Assert.AreEqual((nint)3, cap(s), "Go's literal has capacity equal to its length");
        CollectionAssert.AreEqual(wrapped.ToArray(), s.ToArray());

        s[0] = 42;
        Assert.AreEqual((nint)1, wrapped[0], "each literal owns its backing");
    }

    [TestMethod]
    public void AnEmptyCollectionExpressionIsGosEmptyLiteralNotNil()
    {
        slice<nint> empty = [];

        Assert.IsFalse(empty == nil, "[]int{} is not nil");
        Assert.AreEqual((nint)0, len(empty));
        Assert.IsTrue(default(slice<nint>) == nil, "control: the zero slice is nil");
    }

    [TestMethod]
    public void NestedLiteralsNestAsGosDo()
    {
        slice<slice<nint>> rows = [[1], [2, 3]];

        Assert.AreEqual((nint)2, len(rows));
        Assert.AreEqual((nint)2, len(rows[1]));
        Assert.AreEqual((nint)3, rows[1][1]);
    }

    [TestMethod]
    public void TheLiteralCountsWhatTheWrappedFormCounts()
    {
        AllocationCounter.Enable();

        long before = AllocationCounter.CurrentThreadCount;
        slice<nint> bare = [1, 2, 3];
        long bareCount = AllocationCounter.CurrentThreadCount - before;

        before = AllocationCounter.CurrentThreadCount;
        slice<nint> wrapped = slice<nint>([1, 2, 3]);
        long wrappedCount = AllocationCounter.CurrentThreadCount - before;

        Assert.AreEqual(wrappedCount, bareCount, "one Go literal, two spellings, one count");
        GC.KeepAlive(bare);
        GC.KeepAlive(wrapped);
    }

    [TestMethod]
    public void ASliceEnumeratesItsValuesAndRangeGivesThePairs()
    {
        slice<@string> s = ["a", "b", "c"];

        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, s.Select(static v => (string)v).ToArray());

        string values = "";

        foreach (var v in s)
            values += v;

        Assert.AreEqual("abc", values);

        string pairs = "";

        foreach (var (i, v) in range(s))
            pairs += $"{i}{v}";

        Assert.AreEqual("0a1b2c", pairs);

        nint count = 0;

        foreach (var i in range(len(s)))
            count += i;

        Assert.AreEqual((nint)3, count, "the index-only range is range(len(s)): 0 + 1 + 2");
    }

    [TestMethod]
    public void RangeOverANilSliceRunsNoIteration()
    {
        slice<nint> none = default;
        nint iterations = 0;

        foreach (var (_, _) in range(none))
            iterations++;

        foreach (var _ in none)
            iterations++;

        Assert.AreEqual((nint)0, iterations);
    }

    [TestMethod]
    public void AnArrayEnumeratesItsValuesAndRangeGivesThePairs()
    {
        array<nint> a = new(3);
        a[0] = 7;
        a[2] = 9;

        Assert.AreEqual(16L, a.Sum(static v => (long)v));

        nint weighted = 0;

        foreach (var (i, v) in range(a))
            weighted += i * v;

        Assert.AreEqual((nint)18, weighted, "0*7 + 1*0 + 2*9");

        ж<array<nint>> p = new StandardBox<array<nint>>(a);
        weighted = 0;

        foreach (var (i, v) in range(p))
            weighted += i * v;

        Assert.AreEqual((nint)18, weighted, "the same pairs through a pointer to the array");
    }

    private static nint WeightedSum<S>(S s) where S : IArray<nint>
    {
        nint sum = 0;

        foreach (var (i, v) in range(s))
            sum += i * v;

        return sum;
    }

    [TestMethod]
    public void ATypeParameterSequenceRangesItsPairs()
    {
        slice<nint> s = [5, 6, 7];

        Assert.AreEqual((nint)20, WeightedSum(s), "0*5 + 1*6 + 2*7");
    }

    private static nint WeightedStackSum(sslice<nint> s)
    {
        nint sum = 0;

        foreach (var (i, v) in range(s))
            sum += i * v;

        foreach (var v in s)
            sum += v;

        return sum;
    }

    [TestMethod]
    public void AVariadicStackSliceRangesItsPairsAndEnumeratesItsValues()
    {
        Span<nint> values = [5, 6, 7];

        Assert.AreEqual((nint)(20 + 18), WeightedStackSum(values));
    }
}
