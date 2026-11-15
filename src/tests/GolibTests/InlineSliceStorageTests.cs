// InlineSliceStorageTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using go;
using go.arrays;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// The storage <see cref="ж{T}.ΔInlineSlice{TElem}"/> slices (go-winio Part 2, P3's review 2026-11-15, items 3a and 3b).
/// A slice of an inline array aliases the array's own bytes, so the storage must hold still for as long as the slice
/// lives: managed storage that can be pinned is pinned through its box; NATIVE storage already holds still and needs no
/// pin; managed storage that cannot be pinned (a root that holds a managed reference) is refused by name.
/// </summary>
[TestClass]
public class InlineSliceStorageTests
{
    // win.GUID as an admitting package emits it: Data4 is an inline array (a go.arrays member), so the struct holds no
    // managed reference and can sit in C memory.
    private struct Guid
    {
        public uint Data1;
        public ushort Data2;
        public ushort Data3;
        public array8<byte> Data4;

        internal static ref array8<byte> ᏑData4(ref Guid g) => ref g.Data4;
    }

    // P3's 3b shape: `type P struct { ID guid.GUID; name string }`. The string makes the root reference-bearing.
    private struct Holder
    {
        public Guid ID;
        public @string name;

        internal static ref Guid ᏑID(ref Holder h) => ref h.ID;
    }

    // 3a: a field of a struct in C memory needs no pin, and the slice is a window over that memory: a write through the
    // slice is read through the memory, and back. READ GREEN BEFORE ANY CHANGE (2026-11-15): ж.of over a native root
    // hands out a NativeBox at the field's Go address (FieldRefBox.NativeFieldBox), whose NativeAddress is that
    // address, so ΔInlineSlice's refusal does not fire. The native-rooted FieldRefBox P3 read survives only where the
    // field's Go offset cannot be resolved, which an emitted accessor (Ꮡ<field>) always resolves. Kept as the guard.
    [TestMethod]
    public unsafe void AFieldOfAStructInNativeMemorySlicesOverThatMemory()
    {
        nint block = Marshal.AllocHGlobal(sizeof(Guid));

        try
        {
            NativeMemory.Clear((void*)block, (nuint)sizeof(Guid));

            var native = (ж<Guid>)(uintptr)(nuint)block;
            ж<array8<byte>> field = native.of((FieldRefFunc<Guid, array8<byte>>)Guid.ᏑData4);

            slice<byte> s = field.ΔInlineSlice<byte>();

            Assert.AreEqual((nint)8, s.Length);
            Assert.AreEqual((nint)8, s.Capacity);

            s[0] = 0xAB;
            Assert.AreEqual((byte)0xAB, ((byte*)block)[8], "a write through the slice reaches the native struct's Data4[0]");

            ((byte*)block)[15] = 0xCD;
            Assert.AreEqual((byte)0xCD, s[7], "a write to the native struct's Data4[7] is read through the slice");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    // The control for 3a: the managed, reference-free root slices as it did (the box is pinned and the slice aliases it).
    [TestMethod]
    public void AFieldOfAReferenceFreeManagedStructSlicesItsOwnBytes()
    {
        var box = new StandardBox<Guid>(new Guid());
        ж<array8<byte>> field = box.of((FieldRefFunc<Guid, array8<byte>>)Guid.ᏑData4);

        slice<byte> s = field.ΔInlineSlice<byte>();
        s[1] = 7;
        Assert.AreEqual((byte)7, box.Value.Data4[1]);

        box.Value.Data4[2] = 9;
        Assert.AreEqual((byte)9, s[2]);
    }

    // 3b, DISCLOSED: a root holding a managed reference cannot be pinned, so a slice of an inline array inside it is
    // refused at run time, by name. Plain Go slices it; the cure (a slice backing of object plus offset, no pin) is a
    // .5 BOARD row. This arm keeps the refusal loud and named until then.
    [TestMethod]
    public void AFieldOfAReferenceBearingRootIsRefusedByName()
    {
        var box = new StandardBox<Holder>(new Holder { name = "beside the GUID" });
        ж<array8<byte>> field = box.of((FieldRefFunc<Holder, Guid>)Holder.ᏑID).of((FieldRefFunc<Guid, array8<byte>>)Guid.ᏑData4);

        PanicException panic = Assert.ThrowsException<PanicException>(() => field.ΔInlineSlice<byte>());
        StringAssert.Contains(panic.Message, "cannot be held still");
    }
}
