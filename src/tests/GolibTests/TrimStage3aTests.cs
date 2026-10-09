// TrimStage3aTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

// Trim stage 3a (docs/PLAN-golib-full-trim.md, section 9): the reflect bridge's value operations no longer close a
// generic helper over a run-time type with MakeGenericMethod (Native AOT cannot), but dispatch through non-generic faces
// the values implement: a box's slot pair, a sequence's window, element alias and grow, a map's set, get and delete.
// These pin each entry point's answers, including the edges the helpers had (a nil box, the nil map key, growth that
// must not reallocate), and the refusal of a container that does not match the type the caller names.
[TestClass]
public class TrimStage3aTests
{
    [TestMethod]
    public void APlainBoxReadsAndWritesItsSlot()
    {
        heap(41, out ж<int> box);

        Assert.AreEqual(41, (int)GoReflect.ReadPointerSlot(box)!);

        GoReflect.WritePointerSlot(box, 42);

        Assert.AreEqual(42, box.Value, "the write lands in the box's own storage");
    }

    [TestMethod]
    public void ANilBoxReadsZeroAndRefusesAStore()
    {
        ж<int> nil = global::go.ж<int>.NilBox;

        Assert.AreEqual(0, (int)GoReflect.ReadPointerSlot(nil)!, "a nil pointer's pointee reads as the zero value");
        Assert.ThrowsException<PanicException>(() => GoReflect.WritePointerSlot(nil, 1), "a store through nil panics as Go's does");
    }

    [TestMethod]
    public void ASliceAndAnArrayWindowShareTheirBacking()
    {
        slice<int> source = new int[] { 1, 2, 3, 4, 5 }.slice();
        slice<int> window = (slice<int>)GoReflect.SliceWindow(source, typeof(int), 1, 3);

        Assert.AreEqual(2, (int)window.Length);
        window[0] = 20;
        Assert.AreEqual(20, source[1], "the window aliases the source's backing store");

        slice<int> capped = (slice<int>)GoReflect.SliceWindow(source, typeof(int), 1, 2, 3);
        Assert.AreEqual(2, (int)capped.Capacity, "the three-index form bounds the capacity");

        array<int> fixedArray = new(new int[] { 7, 8, 9 });
        slice<int> overArray = (slice<int>)GoReflect.SliceWindow(fixedArray, typeof(int), 0, 2);
        Assert.AreEqual(8, overArray[1]);
    }

    [TestMethod]
    public void GrowKeepsTheSameValueWhenThereIsRoomAndCopiesWhenThereIsNot()
    {
        slice<int> roomy = new slice<int>(new int[8], 0, 2);
        object boxed = roomy;

        Assert.AreSame(boxed, GoReflect.GrowSlice(boxed, typeof(int), 3), "growth within capacity returns the source itself");

        slice<int> full = new int[] { 1, 2 }.slice();
        slice<int> grown = (slice<int>)GoReflect.GrowSlice(full, typeof(int), 5)!;

        Assert.AreEqual(2, (int)grown.Length, "the length is preserved");
        Assert.IsTrue(grown.Capacity >= 7, "the capacity covers the requested extra");
        Assert.AreEqual(2, grown[1], "the contents are copied");
        Assert.IsNull(GoReflect.GrowSlice(null, typeof(int), 0), "a null container with nothing to add answers null, as before");
    }

    [TestMethod]
    public void AMapSetsReadsAndDeletesIncludingTheNilKey()
    {
        map<object, nint> m = new() { [(object)"a"] = 1 };

        GoReflect.SetMapEntry(m, typeof(object), typeof(nint), "b", (nint)2);
        Assert.IsTrue(GoReflect.TryGetMapEntry(m, typeof(object), typeof(nint), "b", out object? b));
        Assert.AreEqual((nint)2, b);

        GoReflect.SetMapEntry(m, typeof(object), typeof(nint), null, (nint)9);
        Assert.IsTrue(GoReflect.TryGetMapEntry(m, typeof(object), typeof(nint), null, out object? nilEntry), "the nil key is found like any other");
        Assert.AreEqual((nint)9, nilEntry);

        GoReflect.DeleteMapEntry(m, typeof(object), typeof(nint), null);
        Assert.IsFalse(GoReflect.TryGetMapEntry(m, typeof(object), typeof(nint), null, out _), "and deleted like any other");
        Assert.IsFalse(GoReflect.TryGetMapEntry(m, typeof(object), typeof(nint), "absent", out object? absent));
        Assert.IsNull(absent, "a missing key reads as no value");
    }

    [TestMethod]
    public void AnElementAliasOverAValueWritesThroughToItsBacking()
    {
        slice<int> source = new int[] { 1, 2, 3 }.slice();
        ж<int> alias = (ж<int>)GoReflect.ElementAliasBoxOfValue(source, typeof(int), 2);

        alias.Value = 30;

        Assert.AreEqual(30, source[2]);
    }

    [TestMethod]
    public void AContainerThatDoesNotMatchTheNamedTypeIsRefusedByName()
    {
        slice<int> ints = new int[] { 1 }.slice();
        map<@string, nint> m = new();

        Assert.ThrowsException<InvalidOperationException>(() => GoReflect.SliceWindow(ints, typeof(long), 0, 1));
        Assert.ThrowsException<InvalidOperationException>(() => GoReflect.GrowSlice(ints, typeof(long), 4));
        Assert.ThrowsException<InvalidOperationException>(() => GoReflect.SetMapEntry(m, typeof(int), typeof(nint), 1, (nint)1));
    }
}
