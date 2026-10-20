using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// <c>AsReadOnlySet()</c>: a read-only <see cref="IReadOnlySet{T}"/> view of a Go map's keys, the C# face of
/// Go's set idiom <c>map[K]struct{}</c>. Every answer must be the one Go's map gives: a nil map is the empty
/// set, the view is live, iteration is Go's <c>range</c> (every key once, the nil key included), and
/// membership is Go's key equality (<c>+0 == -0</c>; NaN is never found).
/// </summary>
[TestClass]
public class MapReadOnlySetTests
{
    private static map<int, EmptyStruct> SetOf(params int[] keys)
    {
        map<int, EmptyStruct> set = new();

        foreach (int key in keys)
            set[key] = default;

        return set;
    }

    [TestMethod]
    public void TheViewAnswersMembershipAndCountFromTheMap()
    {
        IReadOnlySet<int> set = SetOf(1, 2, 3).AsReadOnlySet();

        Assert.AreEqual(3, set.Count);
        Assert.IsTrue(set.Contains(2));
        Assert.IsFalse(set.Contains(4));
        CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, set.ToArray());
    }

    [TestMethod]
    public void ANilMapIsTheEmptySet()
    {
        map<int, EmptyStruct> nilMap = default;
        IReadOnlySet<int> set = nilMap.AsReadOnlySet();

        Assert.AreEqual(0, set.Count);
        Assert.IsFalse(set.Contains(1));
        Assert.IsTrue(set.IsSubsetOf(Array.Empty<int>()));
        Assert.IsTrue(set.SetEquals(Array.Empty<int>()));
        Assert.IsFalse(set.Overlaps(new[] { 1 }));
        Assert.AreEqual(0, set.Count());
    }

    [TestMethod]
    public void TheViewIsLive()
    {
        map<int, EmptyStruct> map = SetOf(1);
        IReadOnlySet<int> set = map.AsReadOnlySet();

        map[2] = default;
        delete(map, 1);

        Assert.IsTrue(set.Contains(2) && !set.Contains(1) && set.Count == 1);
    }

    [TestMethod]
    public void TheSetRelationsFollowTheBclContract()
    {
        IReadOnlySet<int> set = SetOf(1, 2, 3).AsReadOnlySet();

        Assert.IsTrue(set.IsSubsetOf(new[] { 1, 2, 3, 4 }));
        Assert.IsTrue(set.IsSubsetOf(new[] { 3, 2, 1, 1 }), "duplicates in the other sequence are ignored");
        Assert.IsFalse(set.IsSubsetOf(new[] { 1, 2 }));
        Assert.IsTrue(set.IsProperSubsetOf(new[] { 1, 2, 3, 4 }));
        Assert.IsFalse(set.IsProperSubsetOf(new[] { 1, 2, 3, 3 }));
        Assert.IsTrue(set.IsSupersetOf(new[] { 1, 3, 3 }));
        Assert.IsFalse(set.IsSupersetOf(new[] { 1, 4 }));
        Assert.IsTrue(set.IsProperSupersetOf(new[] { 1, 1 }));
        Assert.IsFalse(set.IsProperSupersetOf(new[] { 1, 2, 3 }));
        Assert.IsTrue(set.Overlaps(new[] { 9, 3 }));
        Assert.IsFalse(set.Overlaps(new[] { 9, 8 }));
        Assert.IsTrue(set.SetEquals(new[] { 3, 1, 2, 2 }));
        Assert.IsFalse(set.SetEquals(new[] { 1, 2 }));
        Assert.IsFalse(set.SetEquals(new[] { 1, 2, 3, 4 }));
        Assert.ThrowsException<ArgumentNullException>(() => set.IsSubsetOf(null!));
    }

    // Go's map equality, not .NET's: +0 and -0 are ONE key, and a NaN key is never found -- so a set
    // holding NaN is not even equal to itself, exactly as Go's `_, ok := m[k]` says.
    [TestMethod]
    public void MembershipIsGoKeyEquality()
    {
        map<float64, EmptyStruct> map = new();
        map[0.0] = default;
        map[double.NaN] = default;
        IReadOnlySet<float64> set = map.AsReadOnlySet();

        Assert.IsTrue(set.Contains(-0.0), "-0 is the key +0");
        Assert.IsFalse(set.Contains(double.NaN), "a NaN key is never found");
        Assert.AreEqual(2, set.Count);
        Assert.IsFalse(set.SetEquals(set), "a set holding NaN is not equal to itself under Go's equality");
    }

    // Go's range visits a nil key like any other, and so does the view.
    [TestMethod]
    public void IterationIsGoRangeIncludingTheNilKey()
    {
        map<object, EmptyStruct> map = new();
        map[null!] = default;
        map["a"] = default;
        IReadOnlySet<object> set = map.AsReadOnlySet();

        object[] keys = set.ToArray();

        Assert.AreEqual(2, keys.Length);
        CollectionAssert.Contains(keys, null);
        Assert.IsTrue(set.Contains(null!));
    }

    // Any map, not only a set: the view reads the keys and ignores the values.
    [TestMethod]
    public void AnyMapsKeysFormASet()
    {
        map<@string, nint> ages = new() { ["ann"] = 31, ["bob"] = 42 };
        IReadOnlySet<@string> names = ages.AsReadOnlySet();

        Assert.IsTrue(names.SetEquals(new @string[] { "bob", "ann" }));
    }
}
