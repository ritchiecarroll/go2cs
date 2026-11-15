using System;
using System.Runtime.CompilerServices;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.builtin;

namespace GolibTests;

// The batch-308 merge of the pin-extent seat: a named array's FIRST pin goes through the element door's publish gate
// (master's lazy-named-array fix) and then through PinnedBuffer.Of (the seat), so under cgo the one pin is of the backing
// the variable reads AND is in the index. Before the merge each half held alone: master pinned the published backing
// with a constructor the index never sees, and the seat pinned through Of the copy's own backing. One arm per pin path
// (the raw pointer and the number), each read through the variable, never through the address the write used.
public unsafe partial class GoCgoArgsTests
{
    [TestMethod]
    public void ANamedArraysFirstRawPointerPublishesThenPinsIntoTheIndex() =>
        aNamedArraysFirstPinIsInTheIndexUntilItIsReleased(raw: true);

    [TestMethod]
    public void ANamedArraysFirstNumberPublishesThenPinsIntoTheIndex() =>
        aNamedArraysFirstPinIsInTheIndexUntilItIsReleased(raw: false);

    private static void aNamedArraysFirstPinIsInTheIndexUntilItIsReleased(bool raw)
    {
        Assert.IsNotNull(PinExtents.Current, "GolibTests is a cgo process: the index is on");

        (nuint start, byte[] backing) = PinANamedArrayNothingHasUsed(raw);

        // The box is unreachable once the helper has returned; its pin is released by the finalizer.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.AreEqual(0, PinExtents.EntriesOver(start, start + 64), "released by its finalizer: the pin left the index");

        // The backing is kept alive (not pinned), so no other object can be pinned at these numbers in between.
        GC.KeepAlive(backing);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (nuint start, byte[] backing) PinANamedArrayNothingHasUsed(bool raw)
    {
        ref FloorSmallMem local = ref heap(new FloorSmallMem(), out ж<FloorSmallMem> Ꮡlocal);

        nuint start = raw ? (nuint)(void*)Ꮡlocal : (nuint)((uintptr)Ꮡlocal).Value;

        Assert.AreEqual(1, PinExtents.EntriesOver(start, start + 64), "the first pin of a named array nothing has used, in the index");
        Assert.IsFalse(Crosses(start + 8), "live: its interior is Go memory, refused by name");

        ((byte*)start)[3] = 7;

        Assert.AreEqual((byte)7, local[3], "published: the pin is of the backing the variable reads");

        byte[] backing = local.Source;
        GC.KeepAlive(Ꮡlocal);

        return (start, backing);
    }
}
