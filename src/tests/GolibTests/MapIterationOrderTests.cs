// MapIterationOrderTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Go randomizes where each range over a map starts (the swiss iterator's random group and slot
/// offset), so two ranges over one unchanged map can visit its entries in different orders, and Go's
/// runtime tests assert exactly that (TestMapIterOrder, TestMapSparseIterOrder). golib walked the same
/// snapshot order on every range. The rest of the range contract must hold whatever the start: every
/// entry exactly once, and an entry removed before it is reached never produced.
/// </summary>
[TestClass]
public class MapIterationOrderTests
{
    private static map<nint, bool> Dense(int n)
    {
        map<nint, bool> m = new();

        for (int i = 0; i < n; i++)
            m[i] = true;

        return m;
    }

    private static List<nint> Order(map<nint, bool> m)
    {
        List<nint> keys = [];

        foreach ((nint key, bool _) in m)
            keys.Add(key);

        return keys;
    }

    [TestMethod]
    public void RangesOverOneMapVisitItInMoreThanOneOrder()
    {
        // runtime's TestMapIterOrder: n = 3, 7, 9, 15, and at least two orders within 100 ranges.
        foreach (int n in new[] { 3, 7, 9, 15 })
        {
            map<nint, bool> m = Dense(n);
            List<nint> first = Order(m);
            bool varied = false;

            for (int attempt = 0; attempt < 100 && !varied; attempt++)
                varied = !first.SequenceEqual(Order(m));

            Assert.IsTrue(varied, $"a map of {n} entries ranged in one order 101 times: [{string.Join(" ", first)}]");
        }
    }

    [TestMethod]
    public void ASparseMapsRangeOrderVaries()
    {
        // runtime's TestMapSparseIterOrder (issue 8410): 1000 inserted, 980 deleted.
        map<nint, bool> m = Dense(1000);

        for (int i = 20; i < 1000; i++)
            m.Remove(i);

        List<nint> first = Order(m);
        bool varied = false;

        for (int attempt = 0; attempt < 800 && !varied; attempt++)
            varied = !first.SequenceEqual(Order(m));

        Assert.IsTrue(varied, "a sparse map of 20 entries ranged in one order 801 times");
    }

    [TestMethod]
    public void EveryRangeProducesEveryEntryExactlyOnce()
    {
        foreach (int n in new[] { 0, 1, 2, 3, 8, 64 })
        {
            map<nint, bool> m = Dense(n);

            for (int attempt = 0; attempt < 50; attempt++)
            {
                List<nint> order = Order(m);

                Assert.AreEqual(n, order.Count, $"n={n}: a range produces len(m) entries");
                CollectionAssert.AreEquivalent(Enumerable.Range(0, n).Select(i => (nint)i).ToList(), order, $"n={n}: each key once");
            }
        }
    }

    [TestMethod]
    public void AnEntryRemovedBeforeItIsReachedIsNeverProducedWhateverTheStart()
    {
        for (int attempt = 0; attempt < 200; attempt++)
        {
            map<nint, bool> m = Dense(8);
            List<nint> produced = [];

            foreach ((nint key, bool _) in m)
            {
                produced.Add(key);

                // The first entry reached removes every other: none of them may be produced.
                if (produced.Count == 1)
                {
                    for (nint other = 0; other < 8; other++)
                    {
                        if (other != key)
                            m.Remove(other);
                    }
                }
            }

            Assert.AreEqual(1, produced.Count, $"entries removed before they were reached were produced: [{string.Join(" ", produced)}]");
        }
    }

    [TestMethod]
    public void AnEntryCreatedDuringTheRangeIsNotProduced()
    {
        for (int attempt = 0; attempt < 50; attempt++)
        {
            map<nint, bool> m = Dense(5);
            int visited = 0;

            foreach ((nint key, bool _) in m)
            {
                visited++;
                m[100 + key] = true;
            }

            Assert.AreEqual(5, visited, "a range walks the entries present when it began");
            Assert.AreEqual(10, m.Count);
        }
    }
}
