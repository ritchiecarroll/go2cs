// CgoPointerFieldTests.cs - Gbtc
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
/// The pointer field of a C struct (cgo Phase 3, increment 3.2, seat 1a: docs/phase4/DESIGN-cgo-phase3-marshaling.md
/// 18.9): <see cref="ж{T}.cgoPointerField{U}(FieldRefFunc{T, ж{U}})"/>, its store overload, and
/// <see cref="ж{T}.cgoWordField{TF}"/>, over each receiver 18.9 names: managed, native over C memory alone, native over
/// Go memory a pin holds still, native over Go storage nothing holds still, and a native receiver whose field has no
/// known Go offset.
/// </summary>
/// <remarks>
/// Before the seat these are the reads s6nextnil, s6nextdirect and s6name refuse on every hosted target ("cannot read
/// native memory as ...: its CLR layout is not its Go layout"): the field is a managed reference in C#, where C stores a
/// word.
/// </remarks>
[TestClass]
public unsafe class CgoPointerFieldTests
{
    // Go layout of a C struct { int family; struct entry *next; void *data; }: family at 0, next at 8, data at 16;
    // size 24.
    public struct Entry
    {
        public int Family;
        public ж<Entry> Next;
        public @unsafe.Pointer Data;

        public static ref int ᏑFamily(ref Entry s) => ref s.Family;
        public static ref ж<Entry> ᏑNext(ref Entry s) => ref s.Next;
        public static ref @unsafe.Pointer ᏑData(ref Entry s) => ref s.Data;

        // Not converter-named, so no Go name and no Go offset can be read off it (FieldRefBox.FieldNameOf).
        public static ref ж<Entry> UnnamedNext(ref Entry s) => ref s.Next;
        public static ref @unsafe.Pointer UnnamedData(ref Entry s) => ref s.Data;
    }

    private const nuint NextOffset = 8;

    private static nuint Zeroed(nuint bytes = 24) => (nuint)NativeMemory.AllocZeroed(bytes);

    private static void AssertRefused(Action action, string text)
    {
        PanicException panic = Assert.ThrowsException<PanicException>(action);
        StringAssert.Contains(panic.Message, text);
    }

    // A Go-memory number: the address of a pinned Go buffer, which the pin index knows.
    private static nuint GoMemoryNumber(slice<byte> buffer) => (nuint)(uintptr)@unsafe.Pointer.FromPinnedBox(Ꮡ(buffer, 0));

    [TestMethod]
    public void AFieldOverCMemoryReadsNilAndACMemoryWordAsANativeBoxAndWalksOn()
    {
        nuint first = Zeroed();
        nuint second = Zeroed();

        try
        {
            *(int*)second = 7;
            ж<Entry> entry = new NativeBox<Entry>(first);

            Assert.IsNull(entry.cgoPointerField(Entry.ᏑNext), "a zero word reads as nil (s6nextnil)");

            *(nuint*)(first + NextOffset) = second;
            ж<Entry> next = entry.cgoPointerField(Entry.ᏑNext);

            Assert.IsInstanceOfType(next, typeof(NativeBox<Entry>), "a word of C memory is a native box over it");
            Assert.AreEqual(second, ((NativeBox<Entry>)next).NativeAddress);
            Assert.AreEqual(7, next.field(Entry.ᏑFamily), "and its scalar field reads at its Go offset (s6nextdirect)");

            // P3's minted root (18.9 arm 5): a field of the box the read minted is a C-memory word too, so the walk goes on.
            Assert.AreEqual(GoPointerWords.SlotKind.CMemory, GoPointerWords.Classify(next.of(Entry.ᏑNext)));
            Assert.IsNull(next.cgoPointerField(Entry.ᏑNext), "l.next.next reads the second node's zero word as nil");
        }
        finally
        {
            NativeMemory.Free((void*)first);
            NativeMemory.Free((void*)second);
        }
    }

    [TestMethod]
    public void AFieldOverCMemoryHoldingAGoPointerIsRefusedOnRead()
    {
        nuint memory = Zeroed();
        slice<byte> buffer = new(16);

        try
        {
            *(nuint*)(memory + NextOffset) = GoMemoryNumber(buffer);
            ж<Entry> entry = new NativeBox<Entry>(memory);

            AssertRefused(() => entry.cgoPointerField(Entry.ᏑNext), "cgo struct field load: a Go pointer read out of C memory is refused");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
            GC.KeepAlive(buffer);
        }
    }

    [TestMethod]
    public void AFieldOverCMemoryStoresNilAndACPointerAndRefusesAGoPointer()
    {
        nuint memory = Zeroed();
        nuint target = Zeroed();

        try
        {
            ж<Entry> entry = new NativeBox<Entry>(memory);

            entry.cgoPointerField(Entry.ᏑNext, new NativeBox<Entry>(target));
            Assert.AreEqual(target, *(nuint*)(memory + NextOffset), "a C pointer's store writes its address");

            entry.cgoPointerField(Entry.ᏑNext, null!);
            Assert.AreEqual((nuint)0, *(nuint*)(memory + NextOffset), "nil writes a zero word");

            *(nuint*)(memory + NextOffset) = target;
            AssertRefused(() => entry.cgoPointerField(Entry.ᏑNext, new StandardBox<Entry>(new Entry())), "cgo struct field store: storing a Go pointer into C memory is refused");
            Assert.AreEqual(target, *(nuint*)(memory + NextOffset), "the refused store left C memory as it was");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
            NativeMemory.Free((void*)target);
        }
    }

    // P3's third receiver (M3): a C struct laid over a Go buffer through the converter's own mint.
    [TestMethod]
    public void AFieldOverPinnedGoMemoryReadsCWordsAndRefusesAGoPointerWithItsOwnText()
    {
        slice<byte> blob = new(24);
        slice<byte> other = new(16);
        nuint target = Zeroed();

        try
        {
            ж<Entry> entry = @unsafe.Pointer.To<Entry>(@unsafe.Pointer.FromPinnedBox(Ꮡ(blob, 0)));

            Assert.IsNull(entry.cgoPointerField(Entry.ᏑNext), "a zero word reads as nil");

            entry.cgoPointerField(Entry.ᏑNext, new NativeBox<Entry>(target));
            Assert.AreEqual(target, ((NativeBox<Entry>)entry.cgoPointerField(Entry.ᏑNext)).NativeAddress, "a C pointer stores and reads back");

            AssertRefused(() => entry.cgoPointerField(Entry.ᏑNext, new StandardBox<Entry>(new Entry())), "cgo struct field store: a Go pointer stored into a C struct laid over Go memory is not supported in 3.2");

            ж<uintptr> word = @unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(blob, 0)), (uintptr)NextOffset));
            word.Value = (uintptr)GoMemoryNumber(other);
            AssertRefused(() => entry.cgoPointerField(Entry.ᏑNext), "cgo struct field load: a Go pointer in a C struct laid over Go memory is not read in 3.2");
        }
        finally
        {
            NativeMemory.Free((void*)target);
            GC.KeepAlive(blob);
            GC.KeepAlive(other);
        }
    }

    // The fourth slot kind (the settling arm, 2026-11-21): a struct over Go storage that no pin holds still.
    [TestMethod]
    public void AFieldOverUnheldGoStorageIsRefusedByS_B1sText()
    {
        slice<byte> blob = new(24);
        ж<Entry> entry = Ꮡ(blob, 0).Reinterpret<byte, Entry>();

        AssertRefused(() => entry.cgoPointerField(Entry.ᏑNext), "the slot is a native box over Go storage that nothing holds still");
        AssertRefused(() => entry.cgoPointerField(Entry.ᏑNext, null!), "the slot is a native box over Go storage that nothing holds still");

        GC.KeepAlive(blob);
    }

    // P3's M2 (18.9 arm 6): a native receiver whose field has no known Go offset is refused by name, for both field kinds,
    // and C memory is left as it was.
    [TestMethod]
    public void AFieldWithNoKnownGoOffsetIsRefusedByNameAndWritesNothing()
    {
        nuint memory = Zeroed();

        try
        {
            ж<Entry> entry = new NativeBox<Entry>(memory);

            AssertRefused(() => entry.cgoPointerField(Entry.UnnamedNext), "has no known Go offset");
            AssertRefused(() => entry.cgoPointerField(Entry.UnnamedNext, null!), "has no known Go offset");
            AssertRefused(() => entry.cgoWordField(Entry.UnnamedData), "has no known Go offset");

            for (nuint i = 0; i < 24; i++)
                Assert.AreEqual((byte)0, *(byte*)(memory + i), $"byte {i} was written");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }

    // 18.9 arm (8), a stop line: a MANAGED receiver (`var x C.struct_s; x.p = q; r := x.p`) round-trips both field kinds,
    // a Go pointer included, exactly as its own slot does.
    [TestMethod]
    public void AManagedReceiverRoundTripsBothFieldKindsAGoPointerIncluded()
    {
        ж<Entry> x = Ꮡ(new Entry());
        ж<Entry> goPointer = Ꮡ(new Entry { Family = 3 });

        x.cgoPointerField(Entry.ᏑNext, goPointer);
        Assert.AreSame(goPointer, x.cgoPointerField(Entry.ᏑNext), "a Go pointer round-trips");
        Assert.AreSame(goPointer, x.Value.Next, "through the struct's own slot");

        x.cgoPointerField(Entry.ᏑNext, null!);
        Assert.IsNull(x.cgoPointerField(Entry.ᏑNext));

        ж<uintptr> storage = new StandardBox<uintptr>(0);
        @unsafe.Pointer data = @unsafe.Pointer.FromBox(storage);
        @unsafe.Pointer.StoreWord(x.cgoWordField(Entry.ᏑData), data);
        Assert.AreSame(data, @unsafe.Pointer.LoadWord(x.cgoWordField(Entry.ᏑData)), "an unsafe.Pointer round-trips through the word doors");
        Assert.AreSame(data, x.Value.Data, "through the struct's own slot");
    }
}
