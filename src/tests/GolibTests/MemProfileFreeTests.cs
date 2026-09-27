using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards class M's frees and cycles (M2, COORD ruling 2026-09-26): a sampled allocation that becomes
/// garbage is counted as FREED by the next GC cycle, and each cycle PUBLISHES the profile in Go's order
/// (mProf_NextCycle at mark termination, mProf_Flush, the sweep's mProf_Free calls, then mProf_PostSweep).
/// The sweep reads a weak reference per sampled allocation. It runs at the end of runtime.GC(), and after a
/// collection the program did not request, through a per-collection sentinel. Red against M1, which
/// records allocations but never frees one and never publishes a cycle.
/// </summary>
[TestClass]
public class MemProfileFreeTests
{
    // One read: (allocs, frees, alloc bytes, free bytes) of the records whose first frame is `function`.
    private static (int64 Allocs, int64 Frees, int64 AllocBytes, int64 FreeBytes) RecordOf(string function)
    {
        var (n, _) = MemProfile(default, true);
        var records = new slice<MemProfileRecord>((int)n + 64, static () => new MemProfileRecord());
        var (m, ok) = MemProfile(records, true);
        Assert.IsTrue(ok, "MemProfile must fit a buffer sized from its own count");

        int64 allocs = 0, frees = 0, allocBytes = 0, freeBytes = 0;

        for (int i = 0; i < (int)m; i++)
        {
            var (first, _) = CallersFrames(records[i].Stack()).Next();

            string name = first.Function;

            if (name.EndsWith("." + function, StringComparison.Ordinal))
            {
                allocs += records[i].AllocObjects;
                frees += records[i].FreeObjects;
                allocBytes += records[i].AllocBytes;
                freeBytes += records[i].FreeBytes;
            }
        }

        return (allocs, frees, allocBytes, freeBytes);
    }

    private static void AtRateOne(Action allocate)
    {
        nint previous = MemProfileRate;

        try
        {
            MemProfileRate = 1;
            allocate();
        }
        finally
        {
            MemProfileRate = previous;
        }
    }

    // Go's TestMemoryProfiler shape: allocate, runtime.GC(), read. Garbage is freed, what is kept is in use,
    // and a second cycle publishes again.
    [TestMethod]
    public void RuntimeGCFreesGarbageAndPublishesEveryCycle()
    {
        AtRateOne(() =>
        {
            memprofprobe_package.allocateRetained1K();
            memprofprobe_package.allocateDropped1K();
        });

        runtime_package.GC();

        var retained = RecordOf(nameof(memprofprobe_package.allocateRetained1K));
        var dropped = RecordOf(nameof(memprofprobe_package.allocateDropped1K));

        Console.WriteLine($"cycle 1: retained {retained}; dropped {dropped}");

        Assert.AreEqual((32L, 0L, 1024L, 0L), retained, "32 kept &Obj32{}: 32 allocated, none freed, 32 in use");
        Assert.AreEqual((1024L, 1024L, 65536L, 65536L), dropped, "1,024 dropped make([]byte, 64): each allocated and each freed");

        // A second round: the first read published cycle 1, and cycle 2 must publish on top of it.
        AtRateOne(memprofprobe_package.allocateDropped1K);

        runtime_package.GC();

        dropped = RecordOf(nameof(memprofprobe_package.allocateDropped1K));
        Console.WriteLine($"cycle 2: dropped {dropped}");

        Assert.AreEqual((2048L, 2048L, 131072L, 131072L), dropped, "the second cycle adds its allocations and frees to the published profile");
        System.GC.KeepAlive(memprofprobe_package.retainedSink);
    }

    // A collection the program did not ask for (Go's background GC) also ends a cycle: the sentinel runs it.
    [TestMethod]
    public void AnUnrequestedCollectionEndsACycle()
    {
        AtRateOne(memprofprobe_package.allocateDroppedUnrequested);

        System.GC.Collect(System.GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
        System.GC.WaitForPendingFinalizers();

        var dropped = RecordOf(nameof(memprofprobe_package.allocateDroppedUnrequested));
        Console.WriteLine($"unrequested: dropped {dropped}");

        Assert.AreEqual((256L, 256L, 16384L, 16384L), dropped, "256 dropped make([]byte, 64): allocated and freed by the unrequested cycle");
    }
}
