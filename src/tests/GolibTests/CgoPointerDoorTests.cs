// CgoPointerDoorTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using go.golib;
using static go.builtin;
using @unsafe = go.unsafe_package;

namespace GolibTests;

/// <summary>
/// The <c>**C.T</c> door (cgo Phase 3, increment 3.2, seat 1c: docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.11, arm
/// E-2): <see cref="GoCgoArgs.LoadPointer{U}"/> and <see cref="GoCgoArgs.StorePointer{U}"/> over each receiver the
/// design names.
/// </summary>
/// <remarks>
/// Before the seat, Go's <c>*pp</c> for <c>pp := &amp;l.next</c> over a C struct in C memory is <c>pp.Value</c>, which
/// the reference floor refuses ("a managed reference where Go stores words"): s6next's pin.
/// </remarks>
[TestClass]
public unsafe class CgoPointerDoorTests
{
    // Go layout of a C struct { int family; struct entry *next; }: family at 0, next at 8; size 16.
    public struct Entry
    {
        public int Family;
        public ж<Entry> Next;

        public static ref int ᏑFamily(ref Entry s) => ref s.Family;
        public static ref ж<Entry> ᏑNext(ref Entry s) => ref s.Next;

        // Not converter-named, so no Go name and no Go offset can be read off it.
        public static ref ж<Entry> UnnamedNext(ref Entry s) => ref s.Next;
    }

    // Go layout of a C struct { int tag; struct entry **pp; }: tag at 0, pp at 8; size 16.
    public struct Holder
    {
        public int Tag;
        public ж<ж<Entry>> Pp;

        public static ref int ᏑTag(ref Holder s) => ref s.Tag;
        public static ref ж<ж<Entry>> ᏑPp(ref Holder s) => ref s.Pp;
    }

    // Go layout of a C struct { int tag; struct entry in; }: tag at 0, in at 8 (family at 8, next at 16); size 24.
    public struct Outer
    {
        public int Tag;
        public Entry In;

        public static ref int ᏑTag(ref Outer s) => ref s.Tag;
        public static ref Entry ᏑIn(ref Outer s) => ref s.In;
    }

    private const nuint NextOffset = 8;

    private const string NoKnownGoOffset = "has no known Go offset";

    private const string ReferenceFloor = "a managed reference where Go stores words";

    private static nuint Zeroed(nuint bytes = 16) => (nuint)NativeMemory.AllocZeroed(bytes);

    private static void AssertRefused(Action action, string text)
    {
        PanicException panic = Assert.ThrowsException<PanicException>(action);
        StringAssert.Contains(panic.Message, text);
    }

    // A Go-memory number: the address of a pinned Go buffer, which the pin index knows.
    private static nuint GoMemoryNumber(slice<byte> buffer) => (nuint)(uintptr)@unsafe.Pointer.FromPinnedBox(Ꮡ(buffer, 0));

    [TestMethod]
    public void OverCMemoryTheDoorLoadsAndStoresTheWordAndRefusesAGoPointer()
    {
        nuint first = Zeroed();
        nuint second = Zeroed();

        try
        {
            *(int*)second = 10;
            ж<Entry> l = new NativeBox<Entry>(first);
            ж<ж<Entry>> pp = l.of(Entry.ᏑNext);

            Assert.IsInstanceOfType(pp, typeof(NativeBox<ж<Entry>>), "&l.next over C memory is a native box at the field's Go offset");
            Assert.IsNull(GoCgoArgs.LoadPointer(pp), "a zero word loads as nil");

            GoCgoArgs.StorePointer(pp, new NativeBox<Entry>(second));
            Assert.AreEqual(second, *(nuint*)(first + NextOffset), "a C pointer's store writes its address");

            ж<Entry> next = GoCgoArgs.LoadPointer(pp);
            Assert.AreEqual(second, ((NativeBox<Entry>)next).NativeAddress, "and loads back as a native box over it");
            Assert.AreEqual(10, next.field(Entry.ᏑFamily), "whose scalar field reads at its Go offset (s6next)");
            Assert.IsTrue(next == GoCgoArgs.LoadPointer(pp), "two loads are one Go pointer (by address)");

            // The minted root (18.9 arm 5): a field address of the box the load minted is a C-memory word too.
            ж<ж<Entry>> pp2 = next.of(Entry.ᏑNext);
            Assert.AreEqual(GoPointerWords.SlotKind.CMemory, GoPointerWords.Classify(pp2));
            Assert.IsNull(GoCgoArgs.LoadPointer(pp2), "the walk goes on: the second node's zero word is nil");

            AssertRefused(() => GoCgoArgs.StorePointer(pp, new StandardBox<Entry>(new Entry())), "cgo struct field store: storing a Go pointer into C memory is refused");
            Assert.AreEqual(second, *(nuint*)(first + NextOffset), "the refused store left C memory as it was");

            GoCgoArgs.StorePointer(pp, null!);
            Assert.AreEqual((nuint)0, *(nuint*)(first + NextOffset), "nil writes a zero word");
        }
        finally
        {
            NativeMemory.Free((void*)first);
            NativeMemory.Free((void*)second);
        }
    }

    [TestMethod]
    public void OverCMemoryAWordNamingGoMemoryIsRefusedOnLoad()
    {
        nuint memory = Zeroed();
        slice<byte> buffer = new(16);

        try
        {
            *(nuint*)(memory + NextOffset) = GoMemoryNumber(buffer);
            ж<ж<Entry>> pp = new NativeBox<Entry>(memory).of(Entry.ᏑNext);

            AssertRefused(() => GoCgoArgs.LoadPointer(pp), "cgo struct field load: a Go pointer read out of C memory is refused");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
            GC.KeepAlive(buffer);
        }
    }

    // A C struct laid over pinned Go memory through the converter's own mint: C words round-trip, and a Go pointer is
    // refused both ways with the over-Go texts.
    [TestMethod]
    public void OverPinnedGoMemoryTheDoorRoundTripsACWordAndRefusesAGoPointerBothWays()
    {
        slice<byte> blob = new(16);
        slice<byte> other = new(16);
        nuint target = Zeroed();

        try
        {
            ж<Entry> l = @unsafe.Pointer.To<Entry>(@unsafe.Pointer.FromPinnedBox(Ꮡ(blob, 0)));
            ж<ж<Entry>> pp = l.of(Entry.ᏑNext);

            Assert.AreEqual(GoPointerWords.SlotKind.GoMemory, GoPointerWords.Classify(pp), "the receiver under test");
            Assert.IsNull(GoCgoArgs.LoadPointer(pp), "a zero word loads as nil");

            GoCgoArgs.StorePointer(pp, new NativeBox<Entry>(target));
            Assert.AreEqual(target, ((NativeBox<Entry>)GoCgoArgs.LoadPointer(pp)).NativeAddress, "a C pointer stores and loads back");

            AssertRefused(() => GoCgoArgs.StorePointer(pp, new StandardBox<Entry>(new Entry())), "cgo struct field store: a Go pointer stored into a C struct laid over Go memory is not supported in 3.2");

            ж<uintptr> word = @unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(blob, 0)), (uintptr)NextOffset));
            word.Value = (uintptr)GoMemoryNumber(other);
            AssertRefused(() => GoCgoArgs.LoadPointer(pp), "cgo struct field load: a Go pointer in a C struct laid over Go memory is not read in 3.2");
        }
        finally
        {
            NativeMemory.Free((void*)target);
            GC.KeepAlive(blob);
            GC.KeepAlive(other);
        }
    }

    [TestMethod]
    public void OverUnheldGoStorageTheDoorIsRefusedByS_B1sText()
    {
        slice<byte> blob = new(16);
        ж<ж<Entry>> pp = Ꮡ(blob, 0).Reinterpret<byte, Entry>().of(Entry.ᏑNext);

        AssertRefused(() => GoCgoArgs.LoadPointer(pp), "the slot is a native box over Go storage that nothing holds still");
        AssertRefused(() => GoCgoArgs.StorePointer(pp, null!), "the slot is a native box over Go storage that nothing holds still");

        GC.KeepAlive(blob);
    }

    // The managed arm is the slot read the converter emits today (ValueSlot): a slot Go allocated round-trips a Go pointer, and nil, exactly as before the seat (18.9 arm 8's
    // twin), through a field view and through a plain box.
    [TestMethod]
    public void OverAManagedSlotTheDoorIsValueAGoPointerIncluded()
    {
        ж<Entry> x = Ꮡ(new Entry());
        ж<Entry> goPointer = Ꮡ(new Entry { Family = 3 });
        ж<ж<Entry>> field = x.of(Entry.ᏑNext);

        GoCgoArgs.StorePointer(field, goPointer);
        Assert.AreSame(goPointer, GoCgoArgs.LoadPointer(field));
        Assert.AreSame(goPointer, x.Value.Next, "through the struct's own slot");

        ж<ж<Entry>> local = new StandardBox<ж<Entry>>((ж<Entry>)null!); // a box HOLDING nil, not the nil box (the NilType overload)
        Assert.IsNull(GoCgoArgs.LoadPointer(local));

        GoCgoArgs.StorePointer(local, goPointer);
        Assert.AreSame(goPointer, local.Value, "the store is `.Value = v`");
        Assert.AreSame(goPointer, GoCgoArgs.LoadPointer(local), "the load is `.Value`");
    }

    [TestMethod]
    public void ANilPointerToAPointerIsGosNilDereference()
    {
        AssertRefused(() => GoCgoArgs.LoadPointer<Entry>(null!), "nil pointer dereference");
        AssertRefused(() => GoCgoArgs.StorePointer<Entry>(null!, null!), "nil pointer dereference");
        AssertRefused(() => GoCgoArgs.LoadPointer(new NativeBox<ж<Entry>>(0)), "nil pointer dereference");
        AssertRefused(() => GoCgoArgs.StorePointer(new NativeBox<ж<Entry>>(0), null!), "nil pointer dereference");
    }

    // [P3 M1]: a native receiver whose field has no known Go offset keeps a field VIEW (ж.cs nativeFieldView), whose CLR
    // slot is not where the field's bytes are. The door refuses it by name before any read or write.
    [TestMethod]
    public void AFieldViewOverANativeReceiverWithNoKnownGoOffsetIsRefusedByNameAndWritesNothing()
    {
        nuint memory = Zeroed();

        try
        {
            ж<ж<Entry>> pp = new NativeBox<Entry>(memory).of(Entry.UnnamedNext);

            Assert.IsNotInstanceOfType(pp, typeof(NativeBox<ж<Entry>>), "the receiver under test: a view, not a native box");
            AssertRefused(() => GoCgoArgs.LoadPointer(pp), NoKnownGoOffset);
            AssertRefused(() => GoCgoArgs.StorePointer(pp, null!), NoKnownGoOffset);
            AssertRefused(() => GoCgoArgs.StorePointer(pp, new StandardBox<Entry>(new Entry())), NoKnownGoOffset);

            for (nuint i = 0; i < 16; i++)
                Assert.AreEqual((byte)0, *(byte*)(memory + i), $"byte {i} was written");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    // [P3 S3]: the tuple target keeps `.val`, and over a native slot the reference floor refuses it by name. This is
    // the emitted shape of `*pp, n = v, 1`.
    [TestMethod]
    public void ATupleTargetsValueOverANativeSlotMeetsTheReferenceFloor()
    {
        nuint memory = Zeroed();

        try
        {
            ж<ж<Entry>> pp = new NativeBox<Entry>(memory).of(Entry.ᏑNext);

            AssertRefused(() => pp.Value = null!, ReferenceFloor);
            Assert.AreEqual((nuint)0, *(nuint*)(memory + NextOffset), "nothing was written");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    // [P3 S5]: one arm across the two seats. A C struct field of type **C.T, read through 1a's accessor, mints a native
    // box over the word it holds; that box classifies as C memory, and `*h.pp` loads and stores through the door.
    [TestMethod]
    public void APointerToAPointerFieldReadByTheAccessorGoesThroughTheDoor()
    {
        nuint holder = Zeroed();
        nuint slot = Zeroed(8);
        nuint target = Zeroed();

        try
        {
            *(nuint*)(holder + 8) = slot;
            ж<Holder> h = new NativeBox<Holder>(holder);
            ж<ж<Entry>> pp = h.cgoPointerField(Holder.ᏑPp);

            Assert.IsInstanceOfType(pp, typeof(NativeBox<ж<Entry>>), "1a's accessor mints a native box over the word");
            Assert.AreEqual(GoPointerWords.SlotKind.CMemory, GoPointerWords.Classify(pp), "which is C memory to the door");
            Assert.IsNull(GoCgoArgs.LoadPointer(pp));

            GoCgoArgs.StorePointer(pp, new NativeBox<Entry>(target));
            Assert.AreEqual(target, *(nuint*)slot, "*h.pp = q writes the C word h.pp names");
            Assert.AreEqual(target, ((NativeBox<Entry>)GoCgoArgs.LoadPointer(h.cgoPointerField(Holder.ᏑPp))).NativeAddress);
        }
        finally
        {
            NativeMemory.Free((void*)holder);
            NativeMemory.Free((void*)slot);
            NativeMemory.Free((void*)target);
        }
    }

    // P3's code-read note on M1: a field address taken in TWO hops over a native receiver (`&o.in.next`), every offset
    // known, is a native box at the Go offset, so the door reads it rather than refusing it under a text that is false
    // for it.
    [TestMethod]
    public void ATwoHopFieldAddressOverANativeReceiverIsANativeBoxTheDoorReads()
    {
        nuint outer = Zeroed(24);
        nuint target = Zeroed();

        try
        {
            *(nuint*)(outer + 16) = target;
            ж<ж<Entry>> pp = new NativeBox<Outer>(outer).of(Outer.ᏑIn).of(Entry.ᏑNext);

            Assert.IsInstanceOfType(pp, typeof(NativeBox<ж<Entry>>), "both hops are native boxes at Go offsets");
            Assert.AreEqual(outer + 16, ((NativeBox<ж<Entry>>)pp).NativeAddress, "at in's offset plus next's");
            Assert.AreEqual(target, ((NativeBox<Entry>)GoCgoArgs.LoadPointer(pp)).NativeAddress);
        }
        finally
        {
            NativeMemory.Free((void*)outer);
            NativeMemory.Free((void*)target);
        }
    }
}
