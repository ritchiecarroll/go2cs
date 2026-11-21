// NativeRootFieldDirectTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

namespace GolibTests;

/// <summary>
/// Change A (C1, COORD 20261121 rulings; P3's MUSTs): a NATIVE root answers <c>&amp;p.f</c> with a native box at the
/// field's Go offset WITHOUT the field-view cache. Before, <c>of()</c> on a <see cref="NativeBox{T}"/> looked the field up
/// in the box's weak-table entry, minted and published a <c>FieldRefBox</c> there on a miss, and then discarded that
/// view for the native box <c>FieldRefBox.NativeFieldBox</c> mints: a weak-table entry per native root and a view nothing
/// returns. The answer itself is unchanged, so every arm but the first holds before and after.
/// </summary>
[TestClass]
public class NativeRootFieldDirectTests
{
    // A reference-bearing struct, so the CLR lays it out automatically and the field's CLR slot is not Go's offset: the
    // native answer must come from the Go layout (Port at 2, Addr at 4), never from the slot.
    private struct NativeRootProbe
    {
        public uint8 Len;
        public uint8 Family;
        public uint16 Port;
        public array<byte> Addr = new(4);
        public array<int8> Zero = new(8);

        public NativeRootProbe() { }
    }

    private const int ProbeBytes = 16;
    private const int GoOffsetOfPort = 2;
    private const int GoOffsetOfAddr = 4;

    // Spelled as go2cs-gen spells a field accessor (`Ꮡ` + the Go field name): golib reads the field's name off it.
    private static ref uint16 ᏑPort(ref NativeRootProbe value) => ref value.Port;

    private static ref array<byte> ᏑAddr(ref NativeRootProbe value) => ref value.Addr;

    // The untyped overload's accessor, in a class of its own so `root.of(ᏑPort)` above names one method group. Its name
    // is the field's, as golib reads it; a native root's field is answered from the Go layout, so it is never invoked.
    private static class Untyped
    {
        internal static ref uint16 ᏑPort(object structPtr) => throw new InvalidOperationException("a native root's field was read through its CLR slot");
    }

    // An accessor whose name spells no Go field: the layout cannot be known, so the root keeps today's view route.
    private static ref uint16 PortWithoutAGoName(ref NativeRootProbe value) => ref value.Port;

    private static nint NativeProbe()
    {
        nint block = Marshal.AllocHGlobal(ProbeBytes);

        for (int i = 0; i < ProbeBytes; i++)
            Marshal.WriteByte(block, i, (byte)(0xA0 + i));

        return block;
    }

    private static bool HasViewEntry(object box) => GoBoxTables.Views.TryGetValue(box, out _);

    // ---- the red ----

    [TestMethod]
    public void ANativeRootPublishesNoViewForItsFieldOnEitherOverload()
    {
        nint block = NativeProbe();

        try
        {
            ж<NativeRootProbe> typedRoot = new NativeBox<NativeRootProbe>((nuint)block);
            ж<NativeRootProbe> untypedRoot = new NativeBox<NativeRootProbe>((nuint)block);

            _ = typedRoot.of(ᏑPort);
            _ = typedRoot.of(ᏑPort);
            _ = untypedRoot.of((FieldRefFunc<uint16>)Untyped.ᏑPort);

            Assert.IsFalse(HasViewEntry(typedRoot), "the typed overload created a weak-table view entry for a native root");
            Assert.IsFalse(HasViewEntry(untypedRoot), "the untyped overload created a weak-table view entry for a native root");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    // ---- the answer, unchanged ----

    [TestMethod]
    public void ANativeRootsFieldIsANativeBoxAtTheFieldsGoOffset()
    {
        nint block = NativeProbe();

        try
        {
            ж<NativeRootProbe> root = new NativeBox<NativeRootProbe>((nuint)block);
            ж<uint16> typed = root.of(ᏑPort);
            ж<uint16> untyped = root.of((FieldRefFunc<uint16>)Untyped.ᏑPort);
            ushort expected = (ushort)Marshal.ReadInt16(block, GoOffsetOfPort);

            foreach (ж<uint16> field in new[] { typed, untyped })
            {
                Assert.IsInstanceOfType(field, typeof(NativeBox<uint16>), "a native root's field is a native box");
                Assert.AreEqual((nuint)block + GoOffsetOfPort, field.NativeAddress, "at the field's GO offset, not its CLR slot");
                Assert.AreEqual(expected, (ushort)field.Value, "reading the bytes at that offset");
            }
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    [TestMethod]
    public void ANativeRootsArrayFieldIsANativeWindowAtItsGoOffset()
    {
        nint block = NativeProbe();

        try
        {
            ж<NativeRootProbe> root = new NativeBox<NativeRootProbe>((nuint)block);
            ж<array<byte>> addr = root.of(ᏑAddr);

            Assert.IsNotInstanceOfType(addr, typeof(FieldRefBox<array<byte>>), "an array field of a native root is a native window, not a view");
            Assert.AreEqual((nuint)block + GoOffsetOfAddr, addr.NativeAddress);
            Assert.IsNotNull(addr.TryGetNativeArrayView(typeof(byte)), "the window answers its elements natively");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    // ---- identity arms: each keeps today's route ----

    [TestMethod]
    public void ANilNativeRootKeepsTodaysView()
    {
        ж<NativeRootProbe> nilRoot = new NativeBox<NativeRootProbe>(0);

        ж<uint16> field = nilRoot.of(ᏑPort);

        Assert.IsInstanceOfType(field, typeof(FieldRefBox<uint16>), "a nil root has no address to answer from, so it keeps the view");
    }

    [TestMethod]
    public void AFieldWithoutAKnownGoLayoutKeepsTodaysCachedView()
    {
        nint block = NativeProbe();

        try
        {
            ж<NativeRootProbe> root = new NativeBox<NativeRootProbe>((nuint)block);
            ж<uint16> first = root.of(PortWithoutAGoName);
            ж<uint16> second = root.of(PortWithoutAGoName);

            Assert.IsInstanceOfType(first, typeof(FieldRefBox<uint16>), "no Go layout: nothing is viewed at a guess");
            Assert.AreSame(first, second, "and the view is the cached one");
            Assert.IsTrue(HasViewEntry(root), "published where today publishes it");
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }
    }

    [TestMethod]
    public void WithTheCacheDisabledANativeRootsFieldIsStillTheNativeBox()
    {
        nint block = NativeProbe();
        FieldViewCache.Disabled = true;

        try
        {
            ж<NativeRootProbe> root = new NativeBox<NativeRootProbe>((nuint)block);
            ж<uint16> field = root.of(ᏑPort);

            Assert.IsInstanceOfType(field, typeof(NativeBox<uint16>));
            Assert.AreEqual((nuint)block + GoOffsetOfPort, field.NativeAddress);
        }
        finally
        {
            FieldViewCache.Disabled = false;
            Marshal.FreeHGlobal(block);
        }
    }

    // ---- retention: a non-bare root's field keeps the root's storage (NativeDerivation.Kept) ----

    [TestMethod]
    public void ANonBareRootsFieldKeepsTheRootsStorageAliveAcrossACollection()
    {
        (ж<uint16> field, WeakReference storage, ushort expected) = FieldOverPinnedStorage();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsTrue(storage.IsAlive, "the field box must keep the root's retained storage alive");
        Assert.AreEqual(expected, (ushort)field.Value, "and read the bytes the root addressed");
        GC.KeepAlive(field);
    }

    // Allocates the storage and the root in a frame of their own, so nothing but the field box can hold them afterwards.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static unsafe (ж<uint16>, WeakReference, ushort) FieldOverPinnedStorage()
    {
        byte[] storage = GC.AllocateArray<byte>(ProbeBytes, pinned: true);

        for (int i = 0; i < ProbeBytes; i++)
            storage[i] = (byte)(0x30 + i);

        nuint address = (nuint)Unsafe.AsPointer(ref storage[0]);
        ж<NativeRootProbe> root = new NativeBox<NativeRootProbe>(address, retainedSource: storage);
        ж<uint16> field = root.of(ᏑPort);
        ushort expected = (ushort)(storage[GoOffsetOfPort] | storage[GoOffsetOfPort + 1] << 8);

        return (field, new WeakReference(storage), expected);
    }
}
