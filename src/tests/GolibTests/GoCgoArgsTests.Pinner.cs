using System;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// cgo S6 (docs/phase4/DESIGN-cgo-phase3-marshaling.md 21, with P3's read folded in 21.7): runtime.Pinner holds a REAL pin.
// cgo lets C keep a Go pointer past the call only if a Pinner pins its memory; before S6 go2cs's Pinner recorded the pin
// and held nothing still, and the doors handed C a stack copy of a whole value. Each row compacts the heap and reads the
// address C kept.
public unsafe partial class GoCgoArgsTests
{
    private struct pinHolder
    {
        public uint16 Family;
        public array<byte> Hdr = new(16);

        public pinHolder() { }
    }

    private static void Compact()
    {
        for (int round = 0; round < 3; round++)
        {
            // Garbage between the pinned object and the heap's end, so a compaction has something to slide over.
            object[] churn = new object[4096];

            for (int i = 0; i < churn.Length; i++)
                churn[i] = new byte[64];

            churn = null!;
            System.GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            System.GC.WaitForPendingFinalizers();
        }
    }

    // A pinned whole scalar crosses IN PLACE (not as a stack mirror), and the address C kept still names the box's bytes
    // after compactions. Near-miss: the same box unpinned keeps the mirror.
    [TestMethod]
    public void S6_APinnedScalarCrossesInPlaceAndTheKeptAddressSurvivesCompaction()
    {
        ж<int32> unpinned = new StandardBox<int32>(1);
        Assert.AreEqual(GoCgoStructPointer.Mirror, GoCgoArgs.ScalarPointer(unpinned, "keep", out _, out _), "unpinned: the mirror, as before");

        ж<int32> box = new StandardBox<int32>(7);
        runtime_package.Pinner pinner = default;
        pinner.Pin(box);

        try
        {
            Assert.AreEqual(GoCgoStructPointer.Pinned, GoCgoArgs.ScalarPointer(box, "keep", out _, out GoCgoPin pin), "pinned: in place");

            nint kept;

            fixed (byte* f = pin)
                kept = pin.Address(f);

            Compact();

            *(int*)kept = 42; // C writes through the address it kept, after the call
            Assert.AreEqual((int32)42, box.Value, "the kept address still names the box's own bytes");
        }
        finally
        {
            pinner.Unpin();
        }

        Assert.IsFalse(GoPinTable.IsPinned(box));
    }

    // 21.7 MUST 1: pinning a value pins every allocation inside it. An element pointer into the value's inline array is
    // then held by the value's pin, and its address survives compaction.
    [TestMethod]
    public void S6_PinningAValuePinsTheInlineArraysInsideIt()
    {
        ж<pinHolder> holder = new StandardBox<pinHolder>(new pinHolder { Family = 2 });
        runtime_package.Pinner pinner = default;
        pinner.Pin(holder);

        try
        {
            array<byte> hdr = holder.Value.Hdr;
            ж<byte> first = new ElemRefBox<byte>(hdr, 0);

            GoCgoPin pin = GoCgoArgs.PinVoidPointer(@unsafe.Pointer.FromBox(first), "keep");
            Assert.IsTrue(pin.IsGoMemory);

            nint kept;

            fixed (byte* f = pin)
                kept = pin.Address(f);

            Compact();

            *(byte*)kept = 9;
            Assert.AreEqual((byte)9, holder.Value.Hdr[0], "the inline array's backing is held by the value's pin");
        }
        finally
        {
            pinner.Unpin();
        }
    }

    // 21.7 MUST 2: the Pinner owns its handle. Another owner's pin over the same storage released first does not release
    // the Pinner's: the address still holds across compaction.
    [TestMethod]
    public void S6_AnotherOwnersReleaseDoesNotReleaseThePinnersPin()
    {
        ж<int32> box = new StandardBox<int32>(5);
        PinnedBuffer other = PinnedBuffer.PinOnly(box.PinnableStorage!)!;
        runtime_package.Pinner pinner = default;
        pinner.Pin(box);

        try
        {
            GoCgoArgs.ScalarPointer(box, "keep", out _, out GoCgoPin pin);

            nint kept;

            fixed (byte* f = pin)
                kept = pin.Address(f);

            other.Dispose(); // the first owner releases
            Compact();

            *(int*)kept = 11;
            Assert.AreEqual((int32)11, box.Value, "the Pinner's own handle still holds the box");
        }
        finally
        {
            pinner.Unpin();
        }
    }

    // 21.7 SHOULD 3: a pinned struct whose Go layout is not its C layout (an inline array is a reference here) cannot be
    // handed over in place, and a mirror or an image would be a copy C keeps. Refused by name.
    [TestMethod]
    public void S6_APinnedStructWhoseLayoutIsNotCsIsRefusedByName()
    {
        ж<pinHolder> holder = new StandardBox<pinHolder>(new pinHolder());
        runtime_package.Pinner pinner = default;
        pinner.Pin(holder);

        try
        {
            GoCgoImages images = default;
            StringAssert.Contains(RefusalOf(() => GoCgoArgs.StructPointer(holder, "keep", ref images, out _, out _)), "pinned with runtime.Pinner");
        }
        finally
        {
            pinner.Unpin();
        }
    }

    // A repeat pin counts and takes no handle; the last unpin frees; one more is Go's "object already unpinned".
    [TestMethod]
    public void S6_ARepeatPinCountsAndTakesNoHandle()
    {
        ж<int32> box = new StandardBox<int32>(3);

        long before = GoPinTable.HandlesTaken;
        GoPinTable.Pin(box);
        long first = GoPinTable.HandlesTaken - before;
        GoPinTable.Pin(box);

        Assert.AreEqual(1L, first, "a first pin of a scalar box holds its one slot");
        Assert.AreEqual(before + first, GoPinTable.HandlesTaken, "a repeat pin takes no handle");
        Assert.AreEqual(2, GoPinTable.Count(box));

        Assert.IsTrue(GoPinTable.Unpin(box));
        Assert.IsTrue(GoPinTable.Unpin(box));
        Assert.IsFalse(GoPinTable.IsPinned(box));
        Assert.IsFalse(GoPinTable.Unpin(box), "a third unpin is Go's object-already-unpinned");
    }

    // S4's number into a Pinner-pinned box resolves to the same storage, so C keeping it keeps the pinned bytes.
    [TestMethod]
    public void S6_ANumberIntoAPinnedBoxCrossesAtThePinnedBytes()
    {
        ж<int32> box = new StandardBox<int32>(8);
        runtime_package.Pinner pinner = default;
        pinner.Pin(box);

        try
        {
            uintptr number = (uintptr)box;
            GoCgoPin pin = GoCgoArgs.PinVoidPointer(new @unsafe.Pointer(number), "keep");
            Assert.IsTrue(pin.IsGoMemory);

            nint kept;

            fixed (byte* f = pin)
                kept = pin.Address(f);

            Compact();

            Assert.AreEqual(8, *(int*)kept, "the number's bytes are the pinned box's");
        }
        finally
        {
            pinner.Unpin();
        }
    }
}
