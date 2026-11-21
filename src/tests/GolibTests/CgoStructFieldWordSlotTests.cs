// CgoStructFieldWordSlotTests.cs - Gbtc
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
/// The premise seat 1a (docs/phase4/DESIGN-cgo-phase3-marshaling.md 18.9, item 2) rests on: the ADDRESS of a pointer
/// field of a C struct in C memory, <c>ʗp.of(T.Ꮡf)</c> over a native receiver, is a native box over C memory ALONE, so
/// S-B1's word doors classify it as a C-memory word (<see cref="GoPointerWords.SlotKind.CMemory"/>). An
/// <c>unsafe.Pointer</c> field's GET/SET property then needs no door of its own: it is <c>LoadWord</c> and
/// <c>StoreWord</c> over that box.
/// </summary>
[TestClass]
public unsafe class CgoStructFieldWordSlotTests
{
    // Go layout of a C struct { int family; struct entry *next; void *data; }: family int32 at 0, next at 8, data at 16;
    // size 24. The CLR layout is not Go's (Next and Data are managed references), which is why the field's address is
    // taken at its Go offset.
    public struct Entry
    {
        public int Family;
        public ж<Entry> Next;
        public @unsafe.Pointer Data;

        public static ref int ᏑFamily(ref Entry s) => ref s.Family;
        public static ref ж<Entry> ᏑNext(ref Entry s) => ref s.Next;
        public static ref @unsafe.Pointer ᏑData(ref Entry s) => ref s.Data;
    }

    private static nuint Zeroed(nuint bytes = 24) => (nuint)NativeMemory.AllocZeroed(bytes);

    [TestMethod]
    public void AnUnsafePointerFieldsAddressOverCMemoryIsACMemoryWordTheDoorsReadAndWrite()
    {
        nuint memory = Zeroed();
        nuint target = Zeroed(8);

        try
        {
            ж<Entry> entry = new NativeBox<Entry>(memory);
            ж<@unsafe.Pointer> slot = entry.of(Entry.ᏑData);

            Assert.IsInstanceOfType(slot, typeof(NativeBox<@unsafe.Pointer>), "the field's address over C memory is a native box");
            Assert.AreEqual(memory + 16, ((NativeBox<@unsafe.Pointer>)slot).NativeAddress, "at the field's Go offset");
            Assert.AreEqual(GoPointerWords.SlotKind.CMemory, GoPointerWords.Classify(slot), "and a word of C memory alone");

            Assert.IsTrue(@unsafe.Pointer.LoadWord(slot) == nil, "a zero word reads as a nil unsafe.Pointer");

            @unsafe.Pointer.StoreWord(slot, (@unsafe.Pointer)(uintptr)target);
            Assert.AreEqual((ulong)target, *(ulong*)(memory + 16), "a C pointer's store writes its address at the Go offset");
            Assert.AreEqual((uintptr)target, (uintptr)@unsafe.Pointer.LoadWord(slot), "and reads back as the same number");

            ж<uintptr> goStorage = new StandardBox<uintptr>(0);
            PanicException panic = Assert.ThrowsException<PanicException>(() => @unsafe.Pointer.StoreWord(slot, @unsafe.Pointer.FromBox(goStorage)));
            StringAssert.Contains(panic.Message, "storing a Go pointer into C memory is refused");
            Assert.AreEqual((ulong)target, *(ulong*)(memory + 16), "the refused store left C memory as it was");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
            NativeMemory.Free((void*)target);
        }
    }

    // P3's third receiver: a C struct laid over PINNED GO memory (s6slice's shape). The field's address is a word of Go
    // memory a pin holds still, never a C-memory word, so the C-memory rules (and their refusal texts) do not apply.
    // The converter's own mint for `(*C.struct_entry)(unsafe.Pointer(&buf[0]))`: To<T> over FromPinnedBox of the element.
    [TestMethod]
    public void AFieldsAddressOverAGoBufferThroughTheConvertersMintIsGoMemory()
    {
        slice<byte> blob = new(24);
        ж<Entry> entry = @unsafe.Pointer.To<Entry>(@unsafe.Pointer.FromPinnedBox(Ꮡ(blob, 0)));
        ж<@unsafe.Pointer> slot = entry.of(Entry.ᏑData);

        Assert.IsInstanceOfType(slot, typeof(NativeBox<@unsafe.Pointer>), "the field's address over the Go buffer is a native box");
        Assert.AreEqual(GoPointerWords.SlotKind.GoMemory, GoPointerWords.Classify(slot), "a word of Go memory a pin holds still");

        GC.KeepAlive(blob);
    }

    // The same buffer through Reinterpret, which holds no pin: Unheld (the first reading, 2026-11-21). Not C memory either.
    [TestMethod]
    public void AFieldsAddressOverAGoBufferThroughReinterpretIsUnheld()
    {
        slice<byte> blob = new(24);
        ж<Entry> entry = Ꮡ(blob, 0).Reinterpret<byte, Entry>();
        ж<@unsafe.Pointer> slot = entry.of(Entry.ᏑData);

        Assert.IsInstanceOfType(slot, typeof(NativeBox<@unsafe.Pointer>), "the field's address over the Go buffer is a native box");
        Assert.AreEqual(GoPointerWords.SlotKind.Unheld, GoPointerWords.Classify(slot), "derived from Go storage that no pin holds still");

        GC.KeepAlive(blob);
    }

    // A MANAGED receiver (a C struct value Go allocated): the field's address is a managed view, which the doors leave to
    // its own semantics.
    [TestMethod]
    public void AFieldsAddressOverAManagedReceiverIsNotAWord()
    {
        ж<Entry> entry = Ꮡ(new Entry());
        ж<@unsafe.Pointer> slot = entry.of(Entry.ᏑData);

        Assert.AreEqual(GoPointerWords.SlotKind.NotAWord, GoPointerWords.Classify(slot));
    }

    // The same premise for a typed pointer field (1c's slot, s6next's `&l.next`): its address is a C-memory word too.
    [TestMethod]
    public void ATypedPointerFieldsAddressOverCMemoryIsACMemoryWord()
    {
        nuint memory = Zeroed();

        try
        {
            ж<Entry> entry = new NativeBox<Entry>(memory);
            ж<ж<Entry>> slot = entry.of(Entry.ᏑNext);

            Assert.IsInstanceOfType(slot, typeof(NativeBox<ж<Entry>>), "the field's address over C memory is a native box");
            Assert.AreEqual(memory + 8, ((NativeBox<ж<Entry>>)slot).NativeAddress, "at the field's Go offset");
            Assert.AreEqual(GoPointerWords.SlotKind.CMemory, GoPointerWords.Classify(slot), "and a word of C memory alone");
        }
        finally
        {
            NativeMemory.Free((void*)memory);
        }
    }
}
