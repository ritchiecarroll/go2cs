using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using go.golib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using @unsafe = go.unsafe_package;

namespace GolibTests;

// The pin-extent index behind the void pointer's door (docs/phase4/DESIGN-cgo-pin-extents.md, section 7). GolibTests
// is a cgo process: its OS golden's module initializer registers C libraries at load, which is what switches the index
// on, so every arm here runs with it on.
public unsafe partial class GoCgoArgsTests
{
    private static bool Crosses(nuint number)
    {
        try
        {
            return GoCgoArgs.VoidPointer(new @unsafe.Pointer((uintptr)number), "fill") == number;
        }
        catch (PanicException refusal)
        {
            StringAssert.Contains(refusal.Message, GoMemoryOwner);
            return false;
        }
    }

    // R3, the stop line's own arm (SB-1): C memory crosses, at its exact number and inside, whatever golib has pinned.
    [TestMethod]
    public void CMemoryCrossesAtItsExactAndInteriorNumbersBesideLivePins()
    {
        byte[] bytes = new byte[64];
        PinnedBuffer pinned = PinnedBuffer.Of(bytes);
        void* memory = NativeMemory.AllocZeroed(16);

        try
        {
            nuint c = (nuint)memory;

            Assert.IsTrue(Crosses(c), "C memory's exact number");
            Assert.IsTrue(Crosses(c + 7), "C memory's interior number");
            Assert.IsTrue(Crosses(c + 15), "C memory's last byte");
            Assert.IsFalse(Crosses((nuint)pinned.Pointer + 9), "the live pin beside it is still refused");
        }
        finally
        {
            NativeMemory.Free(memory);
            pinned.Dispose();
        }

        GC.KeepAlive(bytes);
    }

    // R4: a released pin leaves no stale refusal. While the buffer is pinned its interior is Go memory; once it is
    // disposed, the extent has left the index, and the same number crosses as any number golib does not know does (row 7,
    // a released pin, is the disclosed residue this leaves: the memory may have moved, and the door cannot tell).
    [TestMethod]
    public void AReleasedPinLeavesNoStaleRefusal()
    {
        byte[] bytes = new byte[64];
        PinnedBuffer pinned = PinnedBuffer.Of(bytes);
        nuint inside = (nuint)pinned.Pointer + 8;

        Assert.IsFalse(Crosses(inside), "live: refused");

        pinned.Dispose();

        Assert.IsTrue(Crosses(inside), "released: no stale refusal");
        GC.KeepAlive(bytes);

        // A pin over several 4 KiB pages enters and leaves every page it covers.
        byte[] wide = new byte[12 * 1024];
        PinnedBuffer pages = PinnedBuffer.Of(wide);
        nuint start = (nuint)pages.Pointer;
        nuint[] across = [start, start + 4096, start + 6000, start + 8192, start + (nuint)wide.Length - 1];

        foreach (nuint number in across)
            Assert.IsFalse(Crosses(number), $"a multi-page pin, live: refused at +{number - start}");

        pages.Dispose();

        foreach (nuint number in across)
            Assert.IsTrue(Crosses(number), $"a multi-page pin, released: no stale refusal at +{number - start}");

        GC.KeepAlive(wide);
    }

    // R8, lane G's hazard (it bites at 3.2): a pinned buffer whose last user reference is gone while something that holds
    // its address is still running. The door's find answers a STRONG reference, and while that is held the buffer, its
    // pin and its entry survive a forced collection, so the bytes cannot move under a native call. Under 3.1 the door
    // only refuses, and does. Once the strong reference is dropped the finalizer releases the pin and takes the extent
    // out, and the number crosses as any number golib does not know does: row 7, the released pin, disclosed.
    [TestMethod]
    public void TheDoorsStrongFindKeepsAPinAcrossACollection()
    {
        nuint inside = PinAndDropEveryReference();

        HoldAcrossACollection(inside);

        // No frame holds the buffer now: the helper's returned, so no temporary of it outlives the hold.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsNull(PinExtents.Find(inside), "released by its finalizer: the extent left the index");
        Assert.IsTrue(Crosses(inside), "DISCLOSED (row 7): a released pin's number crosses");
    }

    // The strong reference the door's find answers, held across a forced collection, in its own frame so that nothing
    // of it outlives the call (a tier-0 frame keeps its temporaries alive to the method's end).
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void HoldAcrossACollection(nuint inside)
    {
        PinnedBuffer? held = PinExtents.Find(inside);

        Assert.IsNotNull(held, "the entry answers while the buffer is alive");

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.AreSame(held, PinExtents.Find(inside), "held across a collection: the entry still resolves");
        Assert.IsFalse(Crosses(inside), "held: refused");

        GC.KeepAlive(held);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static nuint PinAndDropEveryReference()
    {
        PinnedBuffer pinned = PinnedBuffer.Of(new byte[64]);
        return (nuint)pinned.Pointer + 8;
    }

    // R7: pins come and go on eight threads while two more ask the door about C memory. Every pinning thread asks about
    // its own LIVE pin's interior (always refused), and the C threads' numbers always cross. The door's read takes no
    // lock, so this is the arm that would show a torn list or a stale page.
    [TestMethod]
    public void PinsComingAndGoingOnManyThreadsNeverOpenALivePinOrCloseCMemory()
    {
        const int pinners = 8, askers = 2;
        TimeSpan window = TimeSpan.FromSeconds(2);

        long livePinsCrossed = 0, cNumbersRefused = 0, pins = 0, asks = 0;
        using var stop = new ManualResetEventSlim();
        var threads = new List<Thread>();

        for (int i = 0; i < pinners; i++)
        {
            // One pinner spans pages, so its chain is added and removed while the other threads read.
            int size = i == 0 ? 9000 : 8 + i * 24;

            threads.Add(new Thread(() =>
            {
                while (!stop.IsSet)
                {
                    byte[] bytes = new byte[size];
                    PinnedBuffer pinned = PinnedBuffer.Of(bytes);

                    if (Crosses((nuint)pinned.Pointer + (nuint)(size / 2)))
                        Interlocked.Increment(ref livePinsCrossed);

                    pinned.Dispose();
                    GC.KeepAlive(bytes);
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

        Assert.IsTrue(pins > 0 && asks > 0, $"the arm ran: {pins} pins, {asks} C asks");
        Assert.AreEqual(0L, livePinsCrossed, $"a live pin crossed ({pins} pins)");
        Assert.AreEqual(0L, cNumbersRefused, $"C memory was refused ({asks} asks)");
    }
}
