// MapKeyReplacementTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Guards the BCL fact golib's map key replacement stands on: a key REPLACED on overwrite (Go keeps
/// the newer of two equal keys, so <c>m[+0] = v; m[-0] = v</c> leaves -0) keeps its entry's position.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Dictionary{TKey, TValue}"/> cannot replace a stored key in place, so <c>map</c> removes
/// the entry and adds it back under the new key. That preserves the entry's position ONLY because
/// Remove puts the freed entry on the dictionary's free list and the next Add takes it -- an
/// implementation detail of the BCL, not a documented contract. If it ever changes, the replaced
/// entry moves to the end, the map's Keys and Values views reorder under an ordinary overwrite, and
/// nothing else fails. These rows fail LOUDLY instead.
/// </para>
/// <para>
/// The behavioral project MapKeyUpdateAndHashPanic is the Go-side proof of the semantics; this is the
/// guard on the mechanism.
/// </para>
/// </remarks>
[TestClass]
public class MapKeyReplacementTests
{
    private static readonly double NegZero = Math.CopySign(0.0, -1.0);

    // The STORAGE order, read through the map's IDictionary Keys view, which is the dictionary's own.
    // A range no longer shows it: each range starts at a random entry, as Go's does (map.cs,
    // enumerateStore), so these rows read the slots directly. The mechanism they guard is unchanged:
    // it is still what every Keys/Values view and IDictionary cast of a map exposes.
    private static IEnumerable<TKey> Slots<TKey, TValue>(map<TKey, TValue> m) where TKey : notnull =>
        ((IDictionary<TKey, TValue>)m).Keys;

    [TestMethod]
    public void Float64KeyReplacement_KeepsTheEntryPosition()
    {
        map<float64, nint> m = new();
        m[3.0] = 1;
        m[0.0] = 2;
        m[7.0] = 3;
        m[-1.0] = 4;

        double[] before = Slots(m).Select(static key => (double)key).ToArray();

        m[NegZero] = 20;

        double[] after = Slots(m).Select(static key => (double)key).ToArray();

        Assert.AreEqual(4, m.Count, "an overwrite adds no entry");
        CollectionAssert.AreEqual(before, after,
            "a key replaced on overwrite MOVED in the enumeration: Dictionary no longer reuses a freed entry for the next Add, and map<K,V>.setReplacingKey's Remove+Add now reorders the map");
        Assert.IsTrue(double.IsNegative(after[1]), "the replaced key is the newer one, -0");
        Assert.AreEqual((nint)20, m[0.0]);
    }

    [TestMethod]
    public void InterfaceKeyReplacement_KeepsTheEntryPosition()
    {
        map<object, nint> m = new();
        m[(@string)"a"] = 1;
        m[0.0] = 2;
        m[(nint)5] = 3;
        m[(@string)"b"] = 4;

        object[] before = Slots(m).ToArray();

        m[NegZero] = 20;

        object[] after = Slots(m).ToArray();

        Assert.AreEqual(4, m.Count, "an overwrite adds no entry");
        Assert.AreEqual(before.Length, after.Length);

        for (int i = 0; i < before.Length; i++)
            Assert.IsTrue(builtin.AreEqual(before[i], after[i]),
                $"entry {i} MOVED on a key replacement: Dictionary no longer reuses a freed entry for the next Add");

        Assert.IsTrue(after[1] is double replaced && double.IsNegative(replaced), "the replaced key is the newer one, -0");
    }

    [TestMethod]
    public void RepeatedReplacement_NeverMovesAnyEntry()
    {
        // Flip the zero key's sign many times among other entries; any drift of the free list would
        // show as the zero entry wandering toward the end.
        map<float64, nint> m = new();

        for (int i = 1; i <= 8; i++)
            m[(double)i] = i;

        m[0.0] = 0;

        for (int i = 9; i <= 16; i++)
            m[(double)i] = i;

        int zeroIndex = Slots(m).Select(static key => (double)key).ToList().IndexOf(0.0);

        for (int flip = 0; flip < 100; flip++)
            m[flip % 2 == 0 ? NegZero : 0.0] = flip;

        Assert.AreEqual(17, m.Count);
        Assert.AreEqual(zeroIndex, Slots(m).Select(static key => (double)key).ToList().IndexOf(0.0),
            "the zero entry moved after repeated key replacement");
    }
}
