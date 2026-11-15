using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// The pin-extent index's cost and correctness for LARGE pinned objects (P3's review of the seat, item 1): a pin's cost to
// the index must not grow with the size of the object it pins, nor with how many pins of that object are live at once,
// and every live pin of an object keeps all of it refused.
public unsafe partial class GoCgoArgsTests
{
    private const int SixteenMiB = 16 << 20;

    // R9: a hundred live pins of one 16 MiB object -- golib's box pins, released by finalizer, overlap exactly like this
    // in a loop over one large buffer -- cost the index O(1) entries each, never one per page each.
    [TestMethod]
    public void ManyPinsOfOneLargeObjectCostTheIndexOneEntryEach()
    {
        const int pins = 100;
        byte[] big = new byte[SixteenMiB];
        var held = new List<PinnedBuffer>(pins);

        try
        {
            for (int i = 0; i < pins; i++)
                held.Add(PinnedBuffer.PinOnly(big)!);

            nuint start = (nuint)held[0].Pointer;
            int entries = PinExtents.EntriesOver(start, start + (nuint)big.Length);

            Assert.IsTrue(entries is > 0 and <= pins, $"{pins} pins of one 16 MiB object hold {entries} index entries");

            // ...and they share ONE entry for the object, so the list the door walks holds one row per distinct object.
            Assert.AreEqual(1, PinExtents.LargeEntriesOver(start, start + (nuint)big.Length), $"{pins} pins of one object, as distinct large entries");
        }
        finally
        {
            foreach (PinnedBuffer pin in held)
                pin.Dispose();
        }

        GC.KeepAlive(big);
    }

    // R10: a LONE pin of a 16 MiB object, taken and released, allocates what a pin of a small one does -- the index's
    // share of it does not grow with the pages the object covers.
    [TestMethod]
    public void ALonePinOfALargeObjectAllocatesNoMoreThanASmallOne()
    {
        byte[] big = new byte[SixteenMiB];

        for (int i = 0; i < 16; i++)
            PinnedBuffer.PinOnly(big)!.Dispose();

        long before = GC.GetAllocatedBytesForCurrentThread();
        PinnedBuffer.PinOnly(big)!.Dispose();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.IsTrue(allocated <= 400, $"a lone pin+release of a 16 MiB object allocated {allocated} B");
        GC.KeepAlive(big);
    }

    // R11: an object with two live pins stays refused end to end when the FIRST is released, and opens only when the last
    // one goes.
    [TestMethod]
    public void ALargeObjectStaysRefusedWhileAnyOfItsPinsLives()
    {
        byte[] big = new byte[256 * 1024];
        PinnedBuffer first = PinnedBuffer.PinOnly(big)!;
        PinnedBuffer second = PinnedBuffer.PinOnly(big)!;
        nuint start = (nuint)first.Pointer;
        nuint[] across = [start, start + (nuint)big.Length / 2, start + (nuint)big.Length - 1];

        foreach (nuint number in across)
            Assert.IsFalse(Crosses(number), $"two pins live: refused at +{number - start}");

        first.Dispose();

        foreach (nuint number in across)
            Assert.IsFalse(Crosses(number), $"the first pin released, the second live: refused at +{number - start}");

        second.Dispose();

        foreach (nuint number in across)
            Assert.IsTrue(Crosses(number), $"both released: no stale refusal at +{number - start}");

        GC.KeepAlive(big);
    }

    // R3, with sixty-four large objects pinned: C memory still crosses at its exact and interior numbers, and a large
    // pin's interior is still refused.
    [TestMethod]
    public void CMemoryCrossesBesideSixtyFourLivePinsOfLargeObjects()
    {
        var objects = new List<byte[]>();
        var held = new List<PinnedBuffer>();
        void* memory = NativeMemory.AllocZeroed(16);

        try
        {
            for (int i = 0; i < 64; i++)
            {
                byte[] big = new byte[128 * 1024];
                objects.Add(big);
                held.Add(PinnedBuffer.PinOnly(big)!);
            }

            nuint c = (nuint)memory;

            Assert.IsTrue(Crosses(c), "C memory's exact number");
            Assert.IsTrue(Crosses(c + 7), "C memory's interior number");
            Assert.IsTrue(Crosses(c + 15), "C memory's last byte");

            foreach (PinnedBuffer pin in held)
                Assert.IsFalse(Crosses((nuint)pin.Pointer + 70_000), "a live large pin's interior is still refused");
        }
        finally
        {
            NativeMemory.Free(memory);

            foreach (PinnedBuffer pin in held)
                pin.Dispose();
        }

        GC.KeepAlive(objects);
    }

    // R12: eight threads take and release pins of ONE shared large object while a sentinel pin of it is held throughout,
    // and two more ask the door about C memory. The object's interior is never crossed while any pin is live, and C memory
    // always crosses. This is the arm that would show a joiner's release closing an extent other pins still hold.
    [TestMethod]
    public void PinsOfOneSharedLargeObjectOnManyThreadsNeverOpenItOrCloseCMemory()
    {
        const int pinners = 8, askers = 2;
        TimeSpan window = TimeSpan.FromSeconds(2);
        byte[] shared = new byte[128 * 1024];
        PinnedBuffer sentinel = PinnedBuffer.PinOnly(shared)!;
        nuint start = (nuint)sentinel.Pointer;

        long liveCrossed = 0, cNumbersRefused = 0, pins = 0, asks = 0;
        using var stop = new ManualResetEventSlim();
        var threads = new List<Thread>();

        for (int i = 0; i < pinners; i++)
        {
            nuint probe = start + (nuint)(i * 16_000 + 7);

            threads.Add(new Thread(() =>
            {
                while (!stop.IsSet)
                {
                    PinnedBuffer pinned = PinnedBuffer.PinOnly(shared)!;

                    if (Crosses(probe))
                        Interlocked.Increment(ref liveCrossed);

                    pinned.Dispose();
                    Interlocked.Increment(ref pins);
                }
            }));
        }

        for (int i = 0; i < askers; i++)
        {
            threads.Add(new Thread(() =>
            {
                void* memory = NativeMemory.AllocZeroed(256);

                try
                {
                    while (!stop.IsSet)
                    {
                        for (nuint offset = 0; offset < 256; offset += 32)
                        {
                            if (!Crosses((nuint)memory + offset))
                                Interlocked.Increment(ref cNumbersRefused);

                            if (Crosses(start + (nuint)shared.Length - 1 - offset))
                                Interlocked.Increment(ref liveCrossed);

                            Interlocked.Increment(ref asks);
                        }
                    }
                }
                finally
                {
                    NativeMemory.Free(memory);
                }
            }));
        }

        foreach (Thread thread in threads)
            thread.Start();

        Thread.Sleep(window);
        stop.Set();

        foreach (Thread thread in threads)
            thread.Join();

        sentinel.Dispose();

        Assert.IsTrue(pins > 0 && asks > 0, $"the arm ran: {pins} pins, {asks} asks");
        Assert.AreEqual(0L, liveCrossed, $"the shared object's interior crossed while pinned ({pins} pins)");
        Assert.AreEqual(0L, cNumbersRefused, $"C memory was refused ({asks} asks)");
        GC.KeepAlive(shared);
    }
}
