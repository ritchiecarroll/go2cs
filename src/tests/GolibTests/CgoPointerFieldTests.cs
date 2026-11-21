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

    // Go layout of a C struct { int family; int *ip; }: family at 0, ip at 8; size 16.
    public struct IntEntry
    {
        public int Family;
        public ж<int> Ip;

        public static ref int ᏑFamily(ref IntEntry s) => ref s.Family;
        public static ref ж<int> ᏑIp(ref IntEntry s) => ref s.Ip;
    }

    private const nuint NextOffset = 8;

    private const nuint DataOffset = 16;

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

    // P3's SHOULD 1 on 1a: an element of a native-backed slice that keeps no anchor is C memory alone (ж.ElemRefBox.cs,
    // 3.1's C2), so a store of its address is a C pointer stored into C memory. Go:
    // s := unsafe.Slice((*C.int)(C.malloc(16)), 4); e.ip = &s[1].
    [TestMethod]
    public void AFieldOverCMemoryStoresAnElementOfCMemoryByItsAddress()
    {
        nuint memory = Zeroed(16);
        nuint array = Zeroed(16);

        try
        {
            slice<int> s = @unsafe.Slice(new NativeBox<int>(array), 4);
            ж<int> element = new ElemRefBox<int>(s, 1);
            Assert.IsTrue(GoPointerWords.IsCMemoryAlone(element), "the shape under test: an element of C memory alone");

            ж<IntEntry> entry = new NativeBox<IntEntry>(memory);
            entry.cgoPointerField(IntEntry.ᏑIp, element);
            Assert.AreEqual(array + 4, *(nuint*)(memory + 8), "the store wrote the element's address");

            *(int*)(array + 4) = 5;
            Assert.AreEqual(5, entry.cgoPointerField(IntEntry.ᏑIp).Value, "and the field reads it back as C memory");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
            NativeMemory.Free((void*)array);
        }
    }

    // P3's SHOULD 2 on 1a: M3's `void *` half. An unsafe.Pointer field of a C struct laid over pinned Go memory, through
    // cgoWordField and the word doors: a C word round-trips as a number, and a Go pointer is stored and read back as its
    // box (18.9's limitations: the field does both, through the word doors).
    [TestMethod]
    public void AnUnsafePointerFieldOverPinnedGoMemoryRoundTripsACWordAndAGoPointer()
    {
        slice<byte> blob = new(24);
        nuint target = Zeroed();
        ж<uintptr> storage = new StandardBox<uintptr>(0);

        try
        {
            ж<Entry> entry = @unsafe.Pointer.To<Entry>(@unsafe.Pointer.FromPinnedBox(Ꮡ(blob, 0)));
            Assert.AreEqual(GoPointerWords.SlotKind.GoMemory, GoPointerWords.Classify(entry.cgoWordField(Entry.ᏑData)), "the receiver under test");

            @unsafe.Pointer.StoreWord(entry.cgoWordField(Entry.ᏑData), new @unsafe.Pointer((uintptr)target));
            @unsafe.Pointer loaded = @unsafe.Pointer.LoadWord(entry.cgoWordField(Entry.ᏑData));
            Assert.AreEqual(target, (nuint)(uintptr)loaded, "a C word round-trips");
            Assert.IsNull(loaded.RetainedSource, "as a number");

            ж<uintptr> word = @unsafe.Pointer.To<uintptr>(@unsafe.Add(@unsafe.Pointer.FromPinnedBox(Ꮡ(blob, 0)), (uintptr)DataOffset));
            Assert.AreEqual(target, (nuint)word.Value, "at the field's Go offset in the Go buffer");

            @unsafe.Pointer.StoreWord(entry.cgoWordField(Entry.ᏑData), @unsafe.Pointer.FromBox(storage));
            Assert.AreSame(storage, @unsafe.Pointer.LoadWord(entry.cgoWordField(Entry.ᏑData)).Referent, "a Go pointer is stored and read back as its box");
        }
        finally
        {
            NativeMemory.Free((void*)target);
            GC.KeepAlive(blob);
            GC.KeepAlive(storage);
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
