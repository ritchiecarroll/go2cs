// SliceArrayElementCopyTests.cs - Gbtc
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
/// Every golib path that moves the elements of a slice copies an array element's VALUE, as Go's copy and append do:
/// after the move the two sides share no inner array, so a write to one is never seen by the other.
/// </summary>
/// <remarks>
/// <para>
/// An array element is a struct over its own storage, and the moves were span copies, which carried that storage
/// along: <c>copy(dst, src)</c> then <c>src[0][1] = 9</c> changed <c>dst</c> too. The behavioral fixture
/// SliceArrayElementCopies holds the paths converted Go reaches by name; these arms hold the paths it reaches only
/// through an interface, a variadic pack, reflect or a stack buffer, and the cost of the copies.
/// </para>
/// </remarks>
[TestClass]
public class SliceArrayElementCopyTests
{
    [ClassInitialize]
    public static void EnableCounting(TestContext _) => AllocationCounter.Enable();

    // [][2]int{{1, 2}, {3, 4}}, cap = len.
    private static slice<array<nint>> Pairs() =>
        new array<nint>[] { new nint[] { 1, 2 }.array(), new nint[] { 3, 4 }.array() }.slice();

    private static slice<array<nint>> ZeroPairs(int length)
    {
        slice<array<nint>> s = new(length);

        for (int i = 0; i < length; i++)
            s[i] = new array<nint>(2);

        return s;
    }

    private static void AssertPairs(slice<array<nint>> s, string what)
    {
        Assert.AreEqual((nint)2, s[0][1], $"{what}: element 0 kept its own value");
        Assert.AreEqual((nint)3, s[1][0], $"{what}: element 1 kept its own value");
    }

    // Writes through both of the source's elements, in place as converted Go writes them.
    private static void Scribble(slice<array<nint>> s)
    {
        s[0][1] = 9;
        s[1][0] = 9;
    }

    [TestMethod]
    public void CopyBetweenSliceInterfacesCopiesEachElement()
    {
        slice<array<nint>> src = Pairs();
        slice<array<nint>> dst = ZeroPairs(2);

        Assert.AreEqual((nint)2, copy((ISlice<array<nint>>)dst, (ISlice<array<nint>>)src));
        Scribble(src);
        AssertPairs(dst, "copy(ISlice, ISlice)");
    }

    [TestMethod]
    public void CopyFromASliceInterfaceCopiesEachElement()
    {
        slice<array<nint>> src = Pairs();
        slice<array<nint>> dst = ZeroPairs(2);

        copy(dst, (ISlice<array<nint>>)src);
        Scribble(src);
        AssertPairs(dst, "copy(slice, ISlice)");
    }

    // copy writes INTO the destination's elements: storage taken from one beforehand sees the copied values.
    [TestMethod]
    public void CopyWritesIntoTheDestinationElementsOwnStorage()
    {
        slice<array<nint>> dst = ZeroPairs(2);
        slice<nint> view = dst[0][..];

        copy(dst, Pairs());

        Assert.AreEqual((nint)1, view[0], "a slice of a destination element, taken before the copy");
        Assert.AreEqual((nint)2, view[1], "a slice of a destination element, taken before the copy");
    }

    [TestMethod]
    public void CopyFromAVariadicPackCopiesEachElement()
    {
        slice<array<nint>> src = Pairs();
        array<nint>[] pack = [src[0], src[1]];
        slice<array<nint>> dst = ZeroPairs(2);

        copy(dst, pack.AsSpan().sslice());
        pack[0][1] = 9;
        pack[1][0] = 9;
        AssertPairs(dst, "copy(slice, sslice)");
    }

    [TestMethod]
    public void AppendOfAVariadicPackCopiesEachElement()
    {
        slice<array<nint>> src = Pairs();
        array<nint>[] pack = [src[0], src[1]];

        slice<array<nint>> appended = appendꓸꓸꓸ(default(slice<array<nint>>), pack.AsSpan().sslice());
        pack[0][1] = 9;
        pack[1][0] = 9;
        AssertPairs(appended, "append(nil, pack...)");
    }

    [TestMethod]
    public void ReflectGrowCopiesTheMovedElements()
    {
        slice<array<nint>> old = Pairs();
        slice<array<nint>> grown = (slice<array<nint>>)GoReflect.GrowSlice(old, typeof(array<nint>), 1)!;

        Assert.IsTrue(cap(grown) > cap(old), "the grow reallocated");
        Scribble(old);
        AssertPairs(grown, "reflect.Value.Grow");
    }

    [TestMethod]
    public void AppendOfZeroesCopiesTheMovedElements()
    {
        slice<array<nint>> old = Pairs();
        slice<array<nint>> grown = appendꓸꓸꓸ(old, makeꓸꓸꓸ<array<nint>>(1));

        Scribble(old);
        AssertPairs(grown, "append(s, make([][2]int, 1)...)");
    }

    // runtime.growsliceBuf: a growth that fits the stack buffer moves the old elements into it.
    [TestMethod]
    public void AStackBufferGrowthCopiesTheOldElements()
    {
        slice<array<nint>> old = new array<nint>[] { new nint[] { 1, 2 }.array() }.slice();
        var bufᴷ = new stackAppend<array<nint>>(moves: true, capacityUsed: true);

        slice<array<nint>> grown = append(bufᴷ, old, new nint[] { 3, 4 }.array());

        Assert.AreEqual((nint)2, len(grown));
        old[0][1] = 9;
        Assert.AreEqual((nint)2, grown[0][1], "the buffer's element kept its own value");
    }

    [TestMethod]
    public void SliceToArrayConversionCopiesEachElement()
    {
        slice<array<nint>> src = Pairs();
        array<array<nint>> converted = new(src, 2);
        array<array<nint>> implicitly = src;

        Scribble(src);
        Assert.AreEqual((nint)2, converted[0][1], "[2][2]int(s)");
        Assert.AreEqual((nint)3, converted[1][0], "[2][2]int(s)");
        Assert.AreEqual((nint)2, implicitly[0][1], "the implicit slice-to-array conversion");
    }

    // The cost, stated: a growth copies each moved element that owns an array (one counted malloc each, where Go's
    // memmove has none); copy into existing elements and every other element type cost what they did.
    [TestMethod]
    public void TheCopiesCostOneMallocPerMovedArrayElementAndNothingElse()
    {
        slice<array<nint>> full = Pairs();
        array<nint> extra = new nint[] { 5, 6 }.array();
        Assert.AreEqual(1L + 2L, Charge(() => append(full, extra)),
            "growth of [][2]int by one element: the new backing and one copy per moved element");

        slice<array<nint>> dst = ZeroPairs(2);
        slice<array<nint>> src = Pairs();
        Assert.AreEqual(0L, Charge(() => copy(dst, src)), "copy into existing [2]int elements");

        slice<nint> ints = new nint[] { 1, 2 }.slice();
        Assert.AreEqual(1L, Charge(() => append(ints, 3)), "growth of []int: the new backing alone");
    }

    private static long Charge(Action action)
    {
        action();

        long before = AllocationCounter.CurrentThreadCount;
        action();

        return AllocationCounter.CurrentThreadCount - before;
    }
}
