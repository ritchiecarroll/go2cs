// ElementTakeOverloadTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// The CONCRETE-header element takes (the os want-zero residue's candidate A, coordinator ruling
/// d0d9f36c6, 2026-09-05): <c>Ꮡ(slice, i)</c> / <c>Ꮡ(array, i)</c> bind overloads that take the
/// header BY VALUE, so the caller's boxing temp the <c>IArray&lt;T&gt;</c> overloads cost (56 B, one
/// counted object beside the 64 B element box) is gone, with NO emission change: overload resolution
/// prefers the identity conversion over the boxing one.
/// </summary>
/// <remarks>
/// The load-bearing arm is the ALIASING one the ruling conditioned the cut on: a reference minted
/// through the concrete overload and one minted through the interface overload over the SAME backing
/// must alias the same element — the by-value header copy is a VIEW, never a copy of the storage.
/// The charge arms pin the counting record's rows: concrete take 1 object (the box), interface take
/// 2 (the box plus the temp), and the bytes of the concrete take are the box alone.
/// </remarks>
[TestClass]
public class ElementTakeOverloadTests
{
    [ClassInitialize]
    public static void EnableCounting(TestContext _) => AllocationCounter.Enable();

    // Objects golib charged and bytes the heap saw for one invocation, measured after a warm-up
    // call so JIT and first-call initialization land outside the window (the counting suite's shape).
    private static (long objects, long bytes) Charge(Action action)
    {
        action();

        long beforeCount = AllocationCounter.CurrentThreadCount;
        long beforeBytes = GC.GetAllocatedBytesForCurrentThread();

        action();

        long bytes = GC.GetAllocatedBytesForCurrentThread() - beforeBytes;

        return (AllocationCounter.CurrentThreadCount - beforeCount, bytes);
    }

    [TestMethod]
    public void AConcreteSliceTakeAndAnInterfaceTakeOverOneBackingAliasTheSameElement()
    {
        slice<int> s = new(new int[] { 10, 20, 30, 40 });

        ж<int> concrete = Ꮡ(s, 2);                       // binds Ꮡ<T>(slice<T>, int)
        ж<int> viaInterface = Ꮡ((IArray<int>)s, 2);      // binds Ꮡ<T>(IArray<T>, int)

        concrete.Value = 77;
        Assert.AreEqual(77, viaInterface.Value, "a write through the concrete take must be read through the interface take: the header copy is a VIEW of one backing");
        Assert.AreEqual(77, s[2], "and through the slice itself");

        viaInterface.Value = 91;
        Assert.AreEqual(91, concrete.Value, "and the other direction");
        Assert.AreEqual(91, s[2]);

        Assert.IsTrue(concrete.Equals(viaInterface), "the two takes are EQUAL as pointers (same backing, same absolute index)");
        Assert.AreEqual(concrete.PointerOrderToken, viaInterface.PointerOrderToken, "and order the same");
    }

    [TestMethod]
    public void AConcreteArrayTakeAndAnInterfaceTakeOverOneBackingAliasTheSameElement()
    {
        array<long> a = new(new long[] { 1, 2, 3 });

        ж<long> concrete = Ꮡ(a, 1);                      // binds Ꮡ<T>(array<T>, int)
        ж<long> viaInterface = Ꮡ((IArray<long>)a, 1);

        concrete.Value = 55;
        Assert.AreEqual(55L, viaInterface.Value, "array: the concrete take aliases the interface take's element");
        Assert.AreEqual(55L, a[1]);

        viaInterface.Value = 66;
        Assert.AreEqual(66L, concrete.Value);
        Assert.IsTrue(concrete.Equals(viaInterface));
    }

    [TestMethod]
    public void AConcreteSliceTakeChargesTheBoxAlone()
    {
        slice<byte> s = new(new byte[16]);

        (long objects, long bytes) = Charge(() => { ж<byte> _ = Ꮡ(s, 3); });

        Assert.AreEqual(1L, objects, $"Ꮡ(slice, i) must charge ONE object (the element box) — it charged {objects} ({bytes} B measured)");
        Assert.IsTrue(bytes <= 64, $"Ꮡ(slice, i) allocated {bytes} B; the element box alone is 64 B, and a header temp would add 56");
    }

    [TestMethod]
    public void AConcreteArrayTakeChargesTheBoxAlone()
    {
        array<byte> a = new(16);

        (long objects, long bytes) = Charge(() => { ж<byte> _ = Ꮡ(a, 3); });

        Assert.AreEqual(1L, objects, $"Ꮡ(array, i) must charge ONE object — it charged {objects} ({bytes} B measured)");
        Assert.IsTrue(bytes <= 64, $"Ꮡ(array, i) allocated {bytes} B; the element box alone is 64 B");
    }

    [TestMethod]
    public void TheInterfaceTakeStillChargesTheBoxAndTheHeaderTemp()
    {
        // The IArray<T> overload is kept for foreign callers at its recorded charge: the box plus the
        // caller's boxing temp (the counting record's "+1" row). This arm is what makes the concrete
        // arm's 1 a measurement rather than a definition — the same site, one overload apart.
        slice<byte> s = new(new byte[16]);

        (long objects, long bytes) = Charge(() => { ж<byte> _ = Ꮡ((IArray<byte>)s, 3); });

        Assert.AreEqual(2L, objects, $"Ꮡ((IArray<T>)slice, i) must still charge TWO objects — it charged {objects} ({bytes} B measured)");
        Assert.IsTrue(bytes > 64, $"the interface take allocated {bytes} B; the header temp must show beside the 64 B box");
    }

    [TestMethod]
    public void TheNativeIndexTwinsBindTheConcreteOverloadsToo()
    {
        slice<int> s = new(new int[] { 5, 6, 7 });
        array<int> a = new(new int[] { 8, 9 });

        nint i = 1;
        ж<int> fromSlice = Ꮡ(s, i);                      // Ꮡ<T>(slice<T>, nint)
        ж<int> fromArray = Ꮡ(a, i);                      // Ꮡ<T>(array<T>, nint)

        Assert.AreEqual(6, fromSlice.Value);
        Assert.AreEqual(9, fromArray.Value);

        (long objects, long _) = Charge(() => { ж<int> _ = Ꮡ(s, i); });
        Assert.AreEqual(1L, objects, "the nint twin charges the box alone as well");
    }

    [TestMethod]
    public void ANativeBackedSliceStillYieldsTheAddressBox()
    {
        // The native-backed arm carried over from the interface overload: a slice over native memory
        // hands back the address-model box, exactly as before.
        unsafe
        {
            int* native = stackalloc int[4];
            native[2] = 42;
            slice<int> view = go.slice<int>.OverNativeMemory((nuint)native, 4);

            ж<int> p = Ꮡ(view, 2);

            Assert.IsInstanceOfType(p, typeof(NativeBox<int>), "a native-backed slice's element take is the address box");
            Assert.AreEqual(42, p.Value);
        }
    }
}
