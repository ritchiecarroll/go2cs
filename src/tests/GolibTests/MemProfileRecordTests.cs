using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.runtime_package;

namespace GolibTests;

/// <summary>
/// Guards class M's allocation records (M1, docs/phase4/DESIGN-managed-profiling.md I6, COORD ruling
/// 2026-09-27): an allocation Go code makes explicitly reaches a golib constructor, and golib charges it
/// to the memory profile under Go's own rule. MemProfileRate is read on every allocation; at 1 every
/// allocation is recorded, and above 1 a per-thread countdown drawn from an exponential distribution with
/// mean MemProfileRate picks them. The record carries the Go size (roundupsize of the Go type's size) and
/// the stack from callers(), starting at the Go function that allocated. Red against master, where nothing
/// ever reaches mProf_Malloc and the heap profile has no records at all.
/// </summary>
[TestClass]
public class MemProfileRecordTests
{
    // One read of the profile: (allocs, alloc bytes) of the records whose first frame is each probe.
    private static (int64, int64, int64, int64, int64) RecordsOf(string persistent, string transient, string atZero)
    {
        var (n, _) = MemProfile(default, true);

        // Go's make([]runtime.MemProfileRecord, n): each element's Stack0 array is constructed.
        var records = new slice<MemProfileRecord>((int)n + 64, static () => new MemProfileRecord());
        var (m, ok) = MemProfile(records, true);
        Assert.IsTrue(ok, "MemProfile must fit a buffer sized from its own count");

        int64 p = 0, pb = 0, t = 0, tb = 0, z = 0;

        for (int i = 0; i < (int)m; i++)
        {
            var (first, _) = CallersFrames(records[i].Stack()).Next();
            string function = first.Function;

            if (function.EndsWith("." + persistent, StringComparison.Ordinal))
            {
                p += records[i].AllocObjects;
                pb += records[i].AllocBytes;
            }
            else if (function.EndsWith("." + transient, StringComparison.Ordinal))
            {
                t += records[i].AllocObjects;
                tb += records[i].AllocBytes;
            }
            else if (function.EndsWith("." + atZero, StringComparison.Ordinal))
            {
                z++;
            }
        }

        return (p, pb, t, tb, z);
    }

    // ONE test, and the order inside it matters: until M2 runs Go's cycle publication in runtime.GC(), a
    // record reaches the reader only through memProfileInternal's "no GC has happened yet" path, which
    // folds every pending cycle into the published one the FIRST time the profile is read and never
    // again. So every allocation is made before the one read. Go's own tests read after runtime.GC().
    [TestMethod]
    public void AllocationsAreRecordedUnderGosRateRule()
    {
        nint previous = MemProfileRate;
        Assert.AreEqual((nint)(512 * 1024), previous, "the test starts at Go's default MemProfileRate");

        try
        {
            // At MemProfileRate = 1 every allocation is recorded.
            MemProfileRate = 1;
            memprofprobe_package.allocatePersistent1K();

            // At 0 nothing is.
            MemProfileRate = 0;
            memprofprobe_package.allocateAtRateZero();

            // At Go's default of 512 KiB, 64 MiB of 1 KiB make([]byte, 1024) gives about 128 samples
            // (a Poisson process; 64-256 is well outside four standard deviations either way).
            MemProfileRate = previous;
            memprofprobe_package.allocateTransient64M();
        }
        finally
        {
            MemProfileRate = previous;
        }

        var (persistent, persistentBytes, transient, transientBytes, atZero) = RecordsOf(
            nameof(memprofprobe_package.allocatePersistent1K),
            nameof(memprofprobe_package.allocateTransient64M),
            nameof(memprofprobe_package.allocateAtRateZero));

        Console.WriteLine($"allocatePersistent1K: {persistent} objects, {persistentBytes} bytes; allocateTransient64M: {transient} samples, {transientBytes} bytes; allocateAtRateZero: {atZero} records");

        // Go's TestMemoryProfiler: 32 &Obj32{} allocations of 32 bytes each.
        Assert.AreEqual(32L, persistent, "each of the 32 allocations is recorded at MemProfileRate = 1");
        Assert.AreEqual(1024L, persistentBytes, "each record carries the Go size of Obj32, 32 bytes");
        Assert.IsTrue(transient is >= 64 and <= 256, $"about 64 MiB / 512 KiB = 128 samples are recorded at the default rate, not every allocation and not none; got {transient}");
        Assert.AreEqual(transient * 1024, transientBytes, "each sample records one 1 KiB allocation");
        Assert.AreEqual(0L, atZero, "MemProfileRate = 0 turns profiling off");
    }
}
