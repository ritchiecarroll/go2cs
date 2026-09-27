using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.reflect_package;

namespace GolibTests;

/// <summary>
/// A Go map KEY is a value: storing one copies it, and reading one out hands back a copy. golib's
/// <c>array&lt;T&gt;</c> is a struct over a shared T[] backing, so a stored key that is the caller's array
/// ALIASES it -- runtime's TestBigItems mutates <c>key[37]</c> between inserts of <c>m[key] = key</c> and
/// every stored key followed the buffer ("#0: missing key"). The map clones a clone-needing key on
/// INSERT of a new key, and reflect's MapIter.Key hands out a clone.
/// </summary>
[TestClass]
public class MapKeyCloneTests
{
    private static array<long> Key(long tag)
    {
        array<long> key = new(4);
        key[0] = 1;
        key[2] = tag;
        return key;
    }

    [TestMethod]
    public void AnArrayKeyInsertedThroughTheIndexerIsACopy_TestBigItemsShape()
    {
        map<array<long>, long> m = new(4);
        array<long> key = Key(0);

        for (long i = 0; i < 10; i++)
        {
            key[2] = i;
            m[key] = i;
        }

        Assert.AreEqual((nint)10, m.Count, "ten distinct keys were stored");

        for (long i = 0; i < 10; i++)
            Assert.AreEqual(i, m[Key(i)], $"key #{i} is still present at its own value -- it was not mutated by later inserts");
    }

    [TestMethod]
    public void TheAddAndSetPathsCopyTheKeyToo()
    {
        map<array<long>, long> viaAdd = new(4);
        map<array<long>, long> viaSet = new(4);
        array<long> key = Key(1);

        viaAdd.Add(key, 1);
        viaSet.Set(key, 1);
        key[2] = 99;

        Assert.IsTrue(viaAdd.ContainsKey(Key(1)), "Add stored a copy, so mutating the caller's array did not move the key");
        Assert.IsTrue(viaSet.ContainsKey(Key(1)), "Set stored a copy");
    }

    [TestMethod]
    public void AnArrayHeldInAnInterfaceKeyIsACopy()
    {
        map<object, long> m = new(4);
        array<long> key = Key(5);

        m[key] = 5;
        key[2] = 6;

        Assert.IsTrue(m.ContainsKey(Key(5)), "an array boxed into an any-typed key is still a Go value");
        Assert.IsFalse(m.ContainsKey(Key(6)), "and the caller's later write did not reach it");
    }

    [TestMethod]
    public void OverwritingAnExistingKeyDoesNotReplaceTheStoredKey()
    {
        map<array<long>, long> m = new(4);
        m[Key(3)] = 1;
        m[Key(3)] = 2;

        Assert.AreEqual((nint)1, m.Count);
        Assert.AreEqual(2L, m[Key(3)]);
    }

    [TestMethod]
    public void AKeyReadThroughReflectMapIterIsACopy()
    {
        map<array<long>, long> m = new(4);
        m[Key(7)] = 7;

        ж<MapIter> iter = ValueOf(m).MapRange();
        Assert.IsTrue(iter.Next(), "the map has one entry");

        array<long> handedOut = (array<long>)iter.Key().Interface();
        handedOut[2] = 70;

        Assert.IsTrue(m.ContainsKey(Key(7)), "writing the key reflect handed out did not reach the stored key");
    }
}
