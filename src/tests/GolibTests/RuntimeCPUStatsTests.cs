using System.Threading;
using go;
using go.runtime;
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
        //
        // Go's half-P slack assumes nothing else in the process runs across the sleep. Here idle is the
        // CPU the process did NOT use (catchUpCPUStats), so a thread another test left running eats it:
        // one busy thread reads about 0.30 against 0.35 at GOMAXPROCS 4, the full-suite reading. So each
        // try measures the process's own CPU across the interval, outside GC pauses, and only a QUIET try
        // (at most 0.2 * dur busy) must reach the floor; ten tries with none quiet are Inconclusive with
        // the readings, never red. 0.2, not Go's 0.5: this read and catchUpCPUStats' read the same clock
        // a few instructions apart, which can differ by one Windows tick (15.6 ms), and a quiet try must
        // still guarantee GOMAXPROCS * dur - 0.02 - 0.0156 >= the floor.
        const double dur = 0.1, quiet = 0.2 * dur;
        double min = dur * (GOMAXPROCS(-1) - 0.5);
        var readings = new System.Collections.Generic.List<string>();

        for (int tries = 0; tries < 10; tries++)
        {
            GC();
            long before = GoCPUStatsProbe()[Idle];
            long cpu0 = ProcessCpuNs(), pause0 = PauseNs();
            Thread.Sleep(100);
            GC();
            long cpu1 = ProcessCpuNs(), pause1 = PauseNs();
            double idle = (GoCPUStatsProbe()[Idle] - before) / 1e9;
            double busy = (cpu1 - cpu0 - (pause1 - pause0)) / 1e9;

            readings.Add($"{idle:F4} idle / {busy:F4} busy");

            if (busy > quiet)
                continue;

            Assert.IsTrue(idle >= min, $"a quiet sleep (the process busy {busy:F5} s of it, at most {quiet:F2}) did not reach the idle class: {idle:F5} s, want at least {min:F5} s");
            System.Console.WriteLine($"CPUSLEEP floor {min:F4}: {string.Join("; ", readings)}");
            return;
        }

        Assert.Inconclusive($"no try was quiet (busy at most {quiet:F2} s): {string.Join("; ", readings)}; floor {min:F5} s");
    }

    // The two clocks catchUpCPUStats reads, in its units (ns).
    private static long ProcessCpuNs() => System.Environment.CpuUsage.TotalTime.Ticks * 100;

    private static long PauseNs() => System.GC.GetTotalPauseDuration().Ticks * 100;

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

        return [samples[0].Value.Float64(), samples[1].Value.Float64()];
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
