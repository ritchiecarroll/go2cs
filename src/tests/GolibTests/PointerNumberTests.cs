// PointerNumberTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// <see cref="ManagedPointerTokens.PointerNumber"/>: the one number reflect's <c>Pointer</c> and <c>UnsafePointer</c>
/// (and so fmt's <c>%p</c>) answer for a Go pointer, which is the number <c>unsafe.Pointer(p)</c> carries.
/// </summary>
/// <remarks>
/// COORD's %p ruling, 2026-11-22 (option A), on C2's BOARD entry of the same day: before it, <c>%p</c> printed the order
/// token while <c>unsafe.Pointer(&amp;i)</c> carried the pinned address, so one Go pointer had two numbers
/// (docs/phase4/DATA-pfmt-identity-2026-11-22.md; the red is the <c>PointerPrintIdentity</c> behavioral program).
/// Each arm asserts the answer of one storage kind against the number <c>unsafe.Pointer</c> carries for it.
/// </remarks>
[TestClass]
public class PointerNumberTests
{
    // A struct that holds a reference beside a reference-free scalar: the scalar's field reference is Unpinnable.
    private struct Holder
    {
        internal string name;
        internal nint scalar;

        internal static ref nint Ꮡscalar(ref Holder instance) => ref instance.scalar;
    }

    private static void Settle()
    {
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true, true);
    }

    [TestMethod]
    public void AReferenceFreeBoxAnswersThePinnedAddressUnsafePointerCarries()
    {
        ж<long> box = new StandardBox<long>(7L);
        nuint number = ManagedPointerTokens.PointerNumber(box);

        Assert.AreEqual(PointerStorage.Pinnable, box.StorageKind, "the premise: a reference-free pointee");
        Assert.AreEqual(ManagedPointerTokens.PointerNumber(@unsafe.Pointer.FromPinnedBox(box)), number, "%p agrees with unsafe.Pointer(p)");
        Assert.IsFalse(ManagedPointerTokens.IsTaggedToken(number), "a machine address, not a token");
        Assert.AreNotEqual(box.PointerOrderToken, number);

        Settle();
        Assert.AreEqual(number, ManagedPointerTokens.PointerNumber(box), "held still across a compacting collection");
        GC.KeepAlive(box);
    }

    [TestMethod]
    public void AReferenceBearingBoxAnswersItsOrderToken()
    {
        heap<ж<long>>(out ж<ж<long>> box);

        Assert.AreEqual(PointerStorage.None, box.StorageKind, "the premise: a reference-bearing pointee");
        Assert.AreEqual(box.PointerOrderToken, ManagedPointerTokens.PointerNumber(box));
        Assert.AreEqual(ManagedPointerTokens.PointerNumber(@unsafe.Pointer.FromPinnedBox(box)), ManagedPointerTokens.PointerNumber(box), "%p agrees with unsafe.Pointer(p)");
    }

    // An unsafe.Pointer value IS a number: its own conversion would pin and answer the slot holding that number.
    [TestMethod]
    public void AnUnsafePointerValueAnswersTheNumberItCarries()
    {
        var pointer = new @unsafe.Pointer((uintptr)0x1234_5670);

        Assert.AreEqual((nuint)0x1234_5670, ManagedPointerTokens.PointerNumber(pointer));
    }

    // The disclosed residual: its uintptr is an address nothing holds still, so the projection keeps the token, stable
    // for as long as the pointer lives, and %p of it differs from unsafe.Pointer of it.
    [TestMethod]
    public void AnUnpinnableFieldKeepsItsOrderToken()
    {
        ж<Holder> container = new StandardBox<Holder>(new Holder { name = "x", scalar = 3 });
        ж<nint> field = container.of(Holder.Ꮡscalar);

        Assert.AreEqual(PointerStorage.Unpinnable, field.StorageKind, "the premise: a field of a struct that holds a reference");

        nuint number = ManagedPointerTokens.PointerNumber(field);
        Assert.AreEqual(field.PointerOrderToken, number);
        Assert.IsTrue(ManagedPointerTokens.IsTaggedToken(number));

        Settle();
        Assert.AreEqual(number, ManagedPointerTokens.PointerNumber(field), "stable across a compacting collection");
        GC.KeepAlive(container);
    }
}
