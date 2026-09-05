using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards the hand-owned addrRanges writers (increment 7 of the runtime row, W2a): init, add and
/// cloneInto re-base the managed a.ranges over the persistentalloc'd block AT the write, so a set
/// grows past its initial 16 slots by doubling, keeps its ranges sorted and disjoint, coalesces
/// adjacent ranges into one, and clones elementwise. Every arm reaches runtime through the
/// Go-prefixed public probe. Linux only: persistentalloc's primitives are bodied in the linux flavour.
/// </summary>
[TestClass]
public class RuntimeAddrRangesTests
{
    private static bool OnLinux => OperatingSystem.IsLinux();

    [TestMethod]
    public void TwentyDisjointRangesGrowTheSetPastItsInitialSixteenSlots()
    {
        if (!OnLinux) Assert.Inconclusive("the memory primitives are the linux flavour's");
        (nint len, nint cap, nuint total, bool sortedDisjoint, nint cloneLen, bool cloneEqual) = GoAddrRangesProbe(20, 8192);
        Assert.AreEqual((nint)20, len, "every range landed");
        Assert.AreEqual((nint)32, cap, "16 doubled once");
        Assert.AreEqual((nuint)(20 * 4096), total, "totalBytes is the sum of the sizes");
        Assert.IsTrue(sortedDisjoint, "ranges stay sorted and disjoint through the growth");
        Assert.AreEqual((nint)20, cloneLen, "the clone grew to fit");
        Assert.IsTrue(cloneEqual, "and holds the same ranges");
    }

    [TestMethod]
    public void AdjacentRangesCoalesceIntoOne()
    {
        if (!OnLinux) Assert.Inconclusive("the memory primitives are the linux flavour's");
        (nint len, nint cap, nuint total, bool sortedDisjoint, nint cloneLen, bool cloneEqual) = GoAddrRangesProbe(5, 4096);
        Assert.AreEqual((nint)1, len, "five adjacent ranges are one");
        Assert.AreEqual((nint)16, cap, "no growth");
        Assert.AreEqual((nuint)(5 * 4096), total);
        Assert.IsTrue(sortedDisjoint);
        Assert.AreEqual((nint)1, cloneLen);
        Assert.IsTrue(cloneEqual);
    }

    [TestMethod]
    public void AnEmptySetClonesEmpty()
    {
        if (!OnLinux) Assert.Inconclusive("the memory primitives are the linux flavour's");
        (nint len, nint cap, nuint total, bool sortedDisjoint, nint cloneLen, bool cloneEqual) = GoAddrRangesProbe(0, 8192);
        Assert.AreEqual((nint)0, len);
        Assert.AreEqual((nint)16, cap, "init's sixteen slots");
        Assert.AreEqual((nuint)0, total);
        Assert.IsTrue(sortedDisjoint);
        Assert.AreEqual((nint)0, cloneLen);
        Assert.IsTrue(cloneEqual);
    }
}
