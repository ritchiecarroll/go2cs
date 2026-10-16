// LinqOnSliceTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// LINQ binds a Go slice or array as its sequence of VALUES, and <c>foreach</c> still yields Go's
/// <c>(index, value)</c> pairs. Before, <c>slice&lt;T&gt;</c> implemented both <c>IEnumerable&lt;T&gt;</c> (through
/// <c>IList&lt;T&gt;</c>) and <c>IEnumerable&lt;(nint, T)&gt;</c> (through <c>IArray&lt;T&gt;</c>), so every LINQ call on
/// a slice failed type inference (CS0411) and a consumer had to name the element type first. The pairs now come
/// from <c>IArray&lt;T&gt;</c>'s own <c>GetEnumerator</c>, which <c>foreach</c> binds by pattern, and the only
/// generic sequence interface left is <c>IEnumerable&lt;T&gt;</c>.
/// </summary>
[TestClass]
public class LinqOnSliceTests
{
    private static long SumIndices<S, E>(S s) where S : ISlice<E>
    {
        long n = 0;

        foreach (var (i, _) in s)
            n += i;

        return n;
    }

    [TestMethod]
    public void LinqBindsASliceAsItsValues()
    {
        slice<nint> s = new nint[] { 7, 8, 9 };

        Assert.AreEqual("7,8,9", string.Join(",", s.ToList()));
        Assert.AreEqual(2, s.Count(x => x > 7));
        Assert.AreEqual("7,9", string.Join(",", s.Where(x => x != 8)));
        Assert.AreEqual(24, (int)s.Sum(x => (int)x));
    }

    [TestMethod]
    public void LinqBindsAnArrayAsItsValues()
    {
        array<nint> a = new(new nint[] { 1, 2, 3 });

        Assert.AreEqual(6, (int)a.Sum(x => (int)x));
        Assert.AreEqual(3, a.Max());
    }

    [TestMethod]
    public void AnInterfaceTypedSliceDoesBoth()
    {
        slice<nint> s = new nint[] { 7, 8, 9 };
        IArray<nint> ia = s;
        long pairs = 0;

        Assert.AreEqual(24, (int)ia.Sum(x => (int)x), "LINQ over the interface sees values");

        foreach (var (i, v) in ia)
            pairs += i * 100 + v;

        Assert.AreEqual(324, pairs, "foreach over the interface still yields (index, value) pairs");
    }

    [TestMethod]
    public void ForeachStillYieldsPairs()
    {
        slice<nint> s = new nint[] { 7, 8, 9 };
        long indices = 0, values = 0;

        foreach (var (i, v) in s)
        {
            indices += i;
            values += v;
        }

        Assert.AreEqual(3, indices);
        Assert.AreEqual(24, values);
        Assert.AreEqual(3, SumIndices<slice<nint>, nint>(s), "a generic ISlice<E> constraint ranges pairs too");
    }

    [TestMethod]
    public void ASubSliceEnumeratesItsOwnWindow()
    {
        slice<nint> s = new nint[] { 1, 2, 3, 4, 5 };
        slice<nint> window = s[1..4];

        CollectionAssert.AreEqual(new nint[] { 2, 3, 4 }, window.ToArray());
        CollectionAssert.AreEqual(new nint[] { 2, 3, 4 }, ((IEnumerable<nint>)window).ToList());
    }
}
