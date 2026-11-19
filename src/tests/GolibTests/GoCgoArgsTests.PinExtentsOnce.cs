using System;
using System.Collections.Generic;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// The flake at GoCgoArgsTests.NamedArrayPin.cs (EntriesOver read 2 for one pin, Debug, host-dependent): the index gives a
// pin one node per 4 KiB page its bytes cover, every node carrying the pin's whole extent, so a test diagnostic that
// counted the nodes over a window counted a pin whose bytes cross a page boundary once per page. The index's answers
// (Find, the door) were never wrong: Find asks one page. These arms place the crossing deliberately, so the count is
// read on every run rather than whenever the allocator happens to put a 64-byte array across a page.
public unsafe partial class GoCgoArgsTests
{
    private static bool CrossesAPage(nuint start, nuint length) => start >> 12 != (start + length - 1) >> 12;

    [TestMethod]
    public void OnePinWhoseBytesCrossAPageIsOneIndexEntry()
    {
        Assert.IsNotNull(PinExtents.Current, "GolibTests is a cgo process: the index is on");

        // Kept alive, so a released array's bytes are not handed to the next one and the search moves forward.
        List<byte[]> kept = new();
        PinnedBuffer? crossing = null, inside = null;

        try
        {
            for (int i = 0; i < 100_000 && (crossing is null || inside is null); i++)
            {
                byte[] bytes = new byte[64];
                kept.Add(bytes);

                PinnedBuffer pin = PinnedBuffer.Of(bytes);
                bool crosses = CrossesAPage((nuint)pin.Pointer, 64);

                if (crosses && crossing is null)
                    crossing = pin;
                else if (!crosses && inside is null)
                    inside = pin;
                else
                    pin.Dispose();
            }

            Assert.IsNotNull(crossing, "the premise: a 64-byte array whose bytes cross a 4 KiB page");
            Assert.IsNotNull(inside, "the control's premise: a 64-byte array inside one page");

            nuint across = (nuint)crossing.Pointer, within = (nuint)inside.Pointer;

            Assert.AreEqual(1, PinExtents.EntriesOver(within, within + 64), "control: a pin inside one page is one entry");
            Assert.AreEqual(1, PinExtents.EntriesOver(across, across + 64), "a pin whose bytes cross a page is still ONE entry");
            Assert.AreEqual(1, PinExtents.EntriesOver(across + 63, across + 64), "its last byte, on the second page, is that one entry");
        }
        finally
        {
            crossing?.Dispose();
            inside?.Dispose();
        }

        GC.KeepAlive(kept);
    }
}
