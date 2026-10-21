// ReflectInterfaceCopyChargeTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ΔValue = go.reflect_package.ΔValue;

namespace GolibTests;

/// <summary>
/// Go's Interface() of an ADDRESSABLE Value copies the datum into a fresh allocation (packEface), so a later Set
/// cannot reach the returned interface value: one malloc, which reflect's TestTypeAssertAllocs asserts for
/// TypeAssert[any](ValueOf(&amp;val).Elem()). The managed read makes the same one copy (a fresh box of the slot's
/// value), and golib's AllocationCounter must charge it, or testing.AllocsPerRun falls back to bytes (24) against
/// Go's count of 1. Every path Go does NOT allocate on must charge nothing: a non-addressable Value, a
/// pointer-shaped kind, an Interface-kind slot, a concrete TypeAssert, and a plain Int().
/// </summary>
[TestClass]
public class ReflectInterfaceCopyChargeTests
{
    [ClassInitialize]
    public static void EnableCounting(TestContext _) => AllocationCounter.Enable();

    private static long Charged(Action action)
    {
        action(); // warm: first-use initialization is not the path being measured
        long before = AllocationCounter.CurrentThreadCount;
        action();
        return AllocationCounter.CurrentThreadCount - before;
    }

    [TestMethod]
    public void AnAddressableCopyIsChargedAsOneAllocation()
    {
        ΔValue v = reflect_package.ValueOf(new StandardBox<nint>(123)).Elem();

        Assert.AreEqual(1L, Charged(() => _ = v.Interface()), "Interface() of an addressable int");
        Assert.AreEqual(1L, Charged(() => _ = reflect_package.TypeAssert<object>(v)), "TypeAssert[any] of an addressable int");
    }

    [TestMethod]
    public void ThePathsGoDoesNotAllocateOnChargeNothing()
    {
        ΔValue addressable = reflect_package.ValueOf(new StandardBox<nint>(123)).Elem();
        Assert.AreEqual(0L, Charged(() => _ = reflect_package.TypeAssert<nint>(addressable)), "TypeAssert[int] reads the slot");
        Assert.AreEqual(0L, Charged(() => _ = addressable.Int()), "Int() reads the slot");

        ΔValue plain = reflect_package.ValueOf((nint)123);
        Assert.AreEqual(0L, Charged(() => _ = plain.Interface()), "Interface() of a non-addressable int");

        ж<nint> target = new StandardBox<nint>(5);
        ΔValue pointerSlot = reflect_package.ValueOf(new StandardBox<ж<nint>>(target)).Elem();
        Assert.AreEqual(0L, Charged(() => _ = pointerSlot.Interface()), "Interface() of an addressable *int (pointer-shaped)");

        ΔValue interfaceSlot = reflect_package.ValueOf(new StandardBox<object>((nint)7)).Elem();
        Assert.AreEqual(0L, Charged(() => _ = interfaceSlot.Interface()), "Interface() of an addressable interface slot");
    }
}
