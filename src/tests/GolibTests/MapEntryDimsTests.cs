// MapEntryDimsTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using abi = go.@internal.abi_package;
using reflect = go.reflect_package;

namespace GolibTests;

/// <summary>
/// gojq's class G: TypeOf of a map measured its element (and key) array dims from an ENTRY, whatever the
/// map's static types were. Two defects rode on that read. An interface side took dims from the entry's
/// dynamic value, so the type of a map[string]any depended on its content. And reading the entry went
/// through Go's range contract, which snapshots the whole map, so every reflect.ValueOf of a map was
/// O(n): gojq's updateObject, which reflects on the object it fills once per key, was quadratic.
/// </summary>
/// <remarks>
/// The scaling rows count BYTES (GC.GetAllocatedBytesForCurrentThread is exact per thread), never wall
/// time: the range snapshot is an n-entry array, so a large map against a one-entry map is a difference of
/// hundreds of kilobytes before the fix and none after it.
/// </remarks>
[TestClass]
public class MapEntryDimsTests
{
    private const int Large = 50_000;

    private static map<@string, object> AnyMap(int entries, object first)
    {
        map<@string, object> m = new(entries);
        m["k"] = first;

        for (int i = 1; i < entries; i++)
            m[(@string)("k" + i)] = (nint)i;

        return m;
    }

    private static map<@string, array<nint>> ArrayMap(int entries)
    {
        map<@string, array<nint>> m = new(entries);

        for (int i = 0; i < entries; i++)
            m[(@string)("k" + i)] = new array<nint>(3);

        return m;
    }

    private static long BytesOf(Action action)
    {
        action(); // warm: the first call JITs and fills the per-type caches
        long before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [TestMethod]
    public void AnInterfaceElementCarriesNoDimsFromItsDynamicValue()
    {
        map<@string, object> holdsArray = AnyMap(1, new array<nint>(3));
        map<@string, object> holdsString = AnyMap(1, (@string)"x");

        Assert.IsNull(GoReflect.MapElemArrayDims(holdsArray),
            "map[string]any{\"k\": [3]int{}}: an interface element has no array length");
        Assert.AreSame(abi.TypeOf(holdsString), abi.TypeOf(holdsArray),
            "TypeOf(map[string]any) must not depend on what the map holds");
        Assert.AreEqual("map[string]interface {}", (string)reflect.TypeOf(holdsArray).String());
    }

    [TestMethod]
    public void AnInterfaceKeyCarriesNoDimsFromItsDynamicValue()
    {
        map<object, nint> m = new() { [new array<nint>(3)] = 1 };

        Assert.IsNull(GoReflect.MapKeyArrayDims(m), "map[any]int with a [3]int key: an interface key has no array length");
    }

    [TestMethod]
    public void AnArrayElementStillAnswersItsLength()
    {
        // The control: a side that CAN carry dims is still measured, through the direct store read.
        CollectionAssert.AreEqual(new nint[] { 3 }, GoReflect.MapElemArrayDims(ArrayMap(1)), "map[string][3]int");
        CollectionAssert.AreEqual(new nint[] { 3 }, GoReflect.MapElemArrayDims(ArrayMap(100)), "map[string][3]int, 100 entries");

        map<array<nint>, nint> keyed = new() { [new array<nint>(2)] = 1 };
        CollectionAssert.AreEqual(new nint[] { 2 }, GoReflect.MapKeyArrayDims(keyed), "map[[2]int]int");
    }

    [TestMethod]
    public void ValueOfAnInterfaceElementMapDoesNotScaleWithItsSize()
    {
        map<@string, object> small = AnyMap(1, (nint)0);
        map<@string, object> large = AnyMap(Large, (nint)0);

        long smallBytes = BytesOf(() => reflect.ValueOf(small));
        long largeBytes = BytesOf(() => reflect.ValueOf(large));

        Assert.AreEqual(smallBytes, largeBytes,
            $"reflect.ValueOf(map[string]any) allocated {largeBytes} B at {Large} entries against {smallBytes} B at 1");
    }

    [TestMethod]
    public void TypeOfAnArrayElementMapDoesNotScaleWithItsSize()
    {
        map<@string, array<nint>> small = ArrayMap(1);
        map<@string, array<nint>> large = ArrayMap(Large);

        long smallBytes = BytesOf(() => GoReflect.MapElemArrayDims(small));
        long largeBytes = BytesOf(() => GoReflect.MapElemArrayDims(large));

        Assert.AreEqual(smallBytes, largeBytes,
            $"MapElemArrayDims(map[string][3]int) allocated {largeBytes} B at {Large} entries against {smallBytes} B at 1");
    }
}
