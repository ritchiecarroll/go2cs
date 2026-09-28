using System.Threading;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static go.runtime_package;
using metrics = go.runtime.metrics_package;

namespace GolibTests;

/// <summary>
/// runtime's CPU accounting behind runtime/metrics' /cpu/classes and the ReadCPUStats export
/// (mstats.go's cpuStats). Go writes work.cpuStats only at gcMarkTermination, which the managed host
/// never runs, so every class read 0 and the runtime row's TestCPUStats, TestCPUMetricsSleep and
/// TestReadMetricsConsistency failed on the zeros. These arms mirror those tests' checks.
/// </summary>
[TestClass]
[DoNotParallelize]
public class RuntimeCPUStatsTests
{
    private const int GCAssist = 0, GCDedicated = 1, GCIdle = 2, GCPause = 3, GCTotal = 4;
    private const int ScavengeAssist = 5, ScavengeBg = 6, ScavengeTotal = 7;
    private const int Idle = 8, User = 9, Total = 10, GoMaxProcs = 11;

    [TestMethod]
    public void AGCCycleRecordsPauseTotalAndIdleTimeWhoseSumsAgree()
    {
        // TestCPUStats: three cycles, then GOMAXPROCS(10) across a short sleep for idle time.
        GC();
        GC();
        GC();

        nint old = GOMAXPROCS(10);
        Thread.Sleep(1);
        GOMAXPROCS(old);
        GC();

        long[] s = GoCPUStatsProbe();

        Assert.AreEqual(s[GCAssist] + s[GCDedicated] + s[GCIdle] + s[GCPause], s[GCTotal], "GC total");
        Assert.AreEqual(s[ScavengeAssist] + s[ScavengeBg], s[ScavengeTotal], "scavenge total");
        Assert.AreEqual(s[GCTotal] + s[ScavengeTotal] + s[Idle] + s[User], s[Total], "overall total");
        Assert.IsTrue(s[Total] > 0, "total time is zero");
        Assert.IsTrue(s[GCPause] > 0, "GC pause time is zero");
        Assert.IsTrue(s[Idle] > 0, "idle time is zero");
        Assert.IsTrue(s[User] > 0, "user time is zero");
    }

    [TestMethod]
    public void ASleepBetweenTwoCyclesIsCountedAsIdle()
    {
        // TestCPUMetricsSleep: a 100 ms sleep between two cycles adds at least
        // 0.1 s * (GOMAXPROCS - 0.5) of idle time, with Go's ten tries.
        const double dur = 0.1;
        double min = dur * (GOMAXPROCS(-1) - 0.5);
        double last = 0;

        for (int tries = 0; tries < 10; tries++)
        {
            GC();
            long before = GoCPUStatsProbe()[Idle];
            Thread.Sleep(100);
            GC();
            last = (GoCPUStatsProbe()[Idle] - before) / 1e9;

            if (last >= min)
                return;
        }

        Assert.Fail($"the sleep did not reach the idle class: {last:F5} s, want at least {min:F5} s");
    }

    [TestMethod]
    public void AReadAfterACollectionRuntimeGCDidNotRunCatchesUp()
    {
        // COORD's ruled arm: a gen2 collection that did not come through runtime.GC() still
        // reaches /cpu/classes, at the next read. The collection is forced through the CLR
        // directly because an allocation-driven gen2 cannot be scheduled by a test.
        double[] before = ReadTotalAndPause();
        System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced, blocking: true);
        double[] after = ReadTotalAndPause();

        Assert.IsTrue(after[0] > before[0], $"total did not advance: {before[0]} -> {after[0]}");
        Assert.IsTrue(after[1] > before[1], $"GC pause did not advance: {before[1]} -> {after[1]}");
    }

    // Reads /cpu/classes/total and /cpu/classes/gc/pause through runtime/metrics.Read, the read point
    // the catch-up runs at.
    private static double[] ReadTotalAndPause()
    {
        slice<metrics.Sample> samples = new(2);
        samples[0].Name = "/cpu/classes/total:cpu-seconds";
        samples[1].Name = "/cpu/classes/gc/pause:cpu-seconds";
        metrics.Read(samples);

        return [metrics.Float64(samples[0].Value), metrics.Float64(samples[1].Value)];
    }

    [TestMethod]
    public void GOMAXPROCSKeepsTheGoGlobalInStep()
    {
        nint old = GOMAXPROCS(3);

        try
        {
            Assert.AreEqual(3L, GoCPUStatsProbe()[GoMaxProcs]);
        }
        finally
        {
            GOMAXPROCS(old);
        }

        Assert.AreEqual((long)old, GoCPUStatsProbe()[GoMaxProcs]);
    }
}
