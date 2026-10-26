using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;
using static go.runtime_package;
using debug = go.runtime.debug_package;
using metrics = go.runtime.metrics_package;
using @unsafe = go.unsafe_package;
using Δruntime = go.runtime_package;
using Goroutine = go.golib.Goroutine;

namespace GolibTests;

// The stop-the-world CONTRACT (owner ruling, ledger 2026-09-28 00:40): stopTheWorld takes worldsema and
// records a /sched/pauses stopping sample; startTheWorld records the total sample and releases worldsema
// with handoff. Other goroutines are NOT suspended (the managed model). Every stopped-world region the
// runtime row reaches works without Go Ps, and a panic that leaves a region releases worldsema, so the
// 2026-09-26 leak class (a region dying with worldsema held, the next stop parked for ever) cannot recur.
//
// One arm per mechanism, read through runtime's Go-prefixed probes (managed_impl.cs), plus one arm per
// stop the runtime row reaches (runtime.GC, GOMAXPROCS, ReadMemStats, Stack, GoroutineProfile,
// debug.WriteHeapDump, trace.Start): the shape of TestSchedPauseMetrics' subtests, which count the samples each
// call adds in each class. The leak arms cover worldsema and metricsSema (ruling 2026-09-28 02:10,
// Q1); the crossing arm is TestReadMetrics' raw []Sample address (Q2); GoroutineProfile's count path is
// Q3 and its fill path A9 (the body runtime/pprof's profile now forwards to); the minimal heap dump is Q4 (a).
[TestClass]
public class RuntimeStopTheWorldContractTests
{
    private const int TimeoutMs = 30000;

    // The holder is a GOROUTINE, not the thread it ran on: goroutine threads are reused (GoroutineThreadPool), so
    // a holder keyed by managed thread id would let the thread's NEXT goroutine release a lock it never took.
    [TestMethod]
    public void AHolderIsItsGoroutineNotTheThreadItRanOn()
    {
        (bool sameThread, bool worldsemaStillHeld, bool metricsSemaStillHeld) = GoSemaHolderReuseProbe(TimeoutMs);

        if (!sameThread)
            Assert.Inconclusive("goroutine B never landed on goroutine A's reused thread; nothing to assert");

        Assert.IsTrue(worldsemaStillHeld, "the next goroutine on the holder's thread released worldsema, which it never took");
        Assert.IsTrue(metricsSemaStillHeld, "the next goroutine on the holder's thread released metricsSema, which it never took");
    }

    [TestMethod]
    public void AStopTheWorldPairReleasesWorldsemaForTheNextCaller()
    {
        (string? firstFailure, bool secondCompleted) = GoStopTheWorldTwiceProbe(TimeoutMs);

        string reading = $"first pair: {firstFailure ?? "ok"}; second pair acquired worldsema: {secondCompleted}";

        Assert.IsNull(firstFailure, reading);
        Assert.IsTrue(secondCompleted, $"the first pair leaked worldsema -- {reading}");
    }

    [TestMethod]
    public void AnOtherPairRecordsOneSampleInEachOtherHistogram()
    {
        var before = GoStwPauseSampleCounts();
        GoStopTheWorldPair(gcReason: false);
        var after = GoStwPauseSampleCounts();

        AssertMoved(before, after, gc: false, "a stwUnknown pair");
    }

    [TestMethod]
    public void AGCPairRecordsOneSampleInEachGCHistogram()
    {
        var before = GoStwPauseSampleCounts();
        GoStopTheWorldPair(gcReason: true);
        var after = GoStwPauseSampleCounts();

        AssertMoved(before, after, gc: true, "a stwGCMarkTerm pair");
    }

    // ---- the hand-owned stops: TestSchedPauseMetrics' subtests, one arm each ----------------------

    [TestMethod]
    public void RuntimeGCRecordsAGCPause()
    {
        var before = GoStwPauseSampleCounts();
        Δruntime.GC();
        var after = GoStwPauseSampleCounts();

        AssertMoved(before, after, gc: true, "runtime.GC");
    }

    [TestMethod]
    public void GOMAXPROCSRecordsAnOtherPause()
    {
        nint n = Δruntime.GOMAXPROCS(0);

        try
        {
            var before = GoStwPauseSampleCounts();
            Δruntime.GOMAXPROCS(n + 1);
            var after = GoStwPauseSampleCounts();

            AssertMoved(before, after, gc: false, "runtime.GOMAXPROCS");
        }
        finally
        {
            Δruntime.GOMAXPROCS(n);
        }
    }

    [TestMethod]
    public void ReadMemStatsRecordsAnOtherPause()
    {
        ж<Δruntime.MemStats> stats = @new<Δruntime.MemStats>();

        var before = GoStwPauseSampleCounts();
        Δruntime.ReadMemStats(stats);
        var after = GoStwPauseSampleCounts();

        AssertMoved(before, after, gc: false, "runtime.ReadMemStats");
    }

    [TestMethod]
    public void StackOfAllGoroutinesRecordsAnOtherPause()
    {
        slice<byte> buf = new(64);

        var before = GoStwPauseSampleCounts();
        Δruntime.Stack(buf, true);
        var after = GoStwPauseSampleCounts();

        AssertMoved(before, after, gc: false, "runtime.Stack(all)");
    }

    // TestSchedPauseMetrics' runtime/trace.Start subtest (Q5): Go's StartTrace enables the tracer inside
    // stopTheWorld(stwStartTrace), and StopTrace stops no world. The trace is drained on a reader thread,
    // as runtime/trace.Start's goroutine does, since StopTrace returns only after the last byte is read.
    [TestMethod]
    public void StartTraceRecordsAnOtherPause()
    {
        var before = GoStwPauseSampleCounts();

        TraceWindow(() => { });

        var after = GoStwPauseSampleCounts();

        AssertMoved(before, after, gc: false, "runtime.StartTrace");
    }

    [TestMethod]
    public void ARefusedStartTraceStopsNoWorld()
    {
        // A GUARD, green before and after: Go answers "tracing is already enabled" BEFORE the stop.
        (ulong, ulong, ulong, ulong) before = default, after = default;
        error? refusal = null;

        TraceWindow(() =>
        {
            before = GoStwPauseSampleCounts();
            refusal = Δruntime.StartTrace();
            after = GoStwPauseSampleCounts();
        });

        // runtime.errorString's Error() prefixes "runtime error: ", as Go's does.
        Assert.AreEqual("runtime error: tracing is already enabled", refusal?.Error().ToString());
        Assert.AreEqual(before, after, "a refused StartTrace recorded a pause");
    }

    // Runs `window` between runtime.StartTrace and runtime.StopTrace with a reader draining ReadTrace.
    private static void TraceWindow(Action window)
    {
        Thread reader = new(() =>
        {
            while (Δruntime.ReadTrace() != nil) { }
        });

        Assert.IsNull(Δruntime.StartTrace(), "no trace may be running when an arm starts one");
        reader.Start();

        try
        {
            window();
        }
        finally
        {
            Δruntime.StopTrace();
            Assert.IsTrue(reader.Join(TimeoutMs), "the trace reader did not drain");
        }
    }

    // ---- the regions --------------------------------------------------------------------------

    [TestMethod]
    public void FlushAllMcachesWithNoPsReturns()
    {
        string? failure = GoFlushAllMcachesProbe();

        Assert.IsNull(failure, $"flushallmcaches raised {failure}");
    }

    [TestMethod]
    public void ADebugLogRecordRoundTrips()
    {
        string dump = GoDebugLogRoundTripProbe("testing");

        // TestDebugLog's dlogCanonicalize: drop the per-logger header, blank each record's prefix.
        string got = Regex.Replace(dump, @"(?m)^>> begin log \d+ <<\n", "");
        got = Regex.Replace(got, @"(?m)^\[[^]]+\]", "[]");

        Assert.IsTrue(got.EndsWith("[] testing\n", StringComparison.Ordinal), $"dump {dump}");
    }

    // ---- the leak arm -------------------------------------------------------------------------

    [TestMethod]
    public void APanicInsideAStoppedWorldReleasesWorldsema()
    {
        (bool regionEntered, string? recovered, bool secondCompleted) = GoStopTheWorldRegionPanicProbe(TimeoutMs);

        string reading = $"region entered: {regionEntered}; recovered: {recovered ?? "nothing"}; the next stop got worldsema: {secondCompleted}";

        Assert.IsTrue(regionEntered, reading);
        Assert.AreEqual("a panic inside a stop-the-world region", recovered, reading);
        Assert.IsTrue(secondCompleted, $"the panic leaked worldsema -- {reading}");
    }

    [TestMethod]
    public void APanicInsideAMetricsRegionReleasesMetricsSema()
    {
        (bool regionEntered, string? recovered, bool secondCompleted) = GoMetricsRegionPanicProbe(TimeoutMs);

        string reading = $"region entered: {regionEntered}; recovered: {recovered ?? "nothing"}; the next metricsLock got it: {secondCompleted}";

        Assert.IsTrue(regionEntered, reading);
        Assert.AreEqual("a panic inside a metricsSema region", recovered, reading);
        Assert.IsTrue(secondCompleted, $"the panic leaked metricsSema -- {reading}");
    }

    // ---- the profile, the dump and the metrics crossing ----------------------------------------

    // Go's goroutineProfileWithLabelsConcurrent answers an EMPTY slice with (gcount(), false) and "without
    // bothering to STW"; every other call stops the world, so a too-short slice records a pause too.
    [TestMethod]
    public void AnEmptyGoroutineProfileCountsWithoutStoppingTheWorld()
    {
        var before = GoStwPauseSampleCounts();
        (nint n, bool ok) = Δruntime.GoroutineProfile(new slice<Δruntime.StackRecord>(0));
        var after = GoStwPauseSampleCounts();

        Assert.IsFalse(ok, $"n {n}: a zero-length record slice cannot hold the profile");
        Assert.IsTrue(n >= 1, $"n {n}: the calling goroutine exists");
        Assert.AreEqual(before, after, "an empty GoroutineProfile slice stopped the world; Go's does not");
    }

    // TestSchedPauseMetrics/runtime.GoroutineProfile's own call: a one-record slice.
    [TestMethod]
    public void AOneRecordGoroutineProfileRecordsAnOtherPause()
    {
        channel<int> c = new(0);

        try
        {
            using (Goroutine.Enter())
            {
                // A second goroutine, so the one record cannot hold the profile whatever else is live.
                builtin.goǃ(ProfileParked, c);
                AwaitParked(1);

                var before = GoStwPauseSampleCounts();
                (nint n, bool ok) = Δruntime.GoroutineProfile(new slice<Δruntime.StackRecord>(1, () => new()));
                var after = GoStwPauseSampleCounts();

                Assert.IsFalse(ok, $"n {n}: one record cannot hold two goroutines");
                Assert.IsTrue(n >= 2, $"n {n}: the caller and its parked child exist");
                AssertMoved(before, after, gc: false, "runtime.GoroutineProfile");
            }
        }
        finally
        {
            c.Close();
        }
    }

    // The FILL path, over golib's goroutine registry: every user goroutine is one record whose stack
    // is its start function's synthetic PC (the bottom frame of the traceback Go's saveg records), the
    // world is stopped once, and worldsema is free afterwards.
    [TestMethod]
    public void AGoroutineProfileFillRecordsEveryGoroutineByItsStartFunction()
    {
        channel<int> c = new(0);

        try
        {
            using (Goroutine.Enter())
            {
                for (int i = 0; i < 3; i++)
                    builtin.goǃ(ProfileParked, c);

                AwaitParked(3);

                slice<Δruntime.StackRecord> records = new(1 << 16, () => new());

                var before = GoStwPauseSampleCounts();
                (nint n, bool ok) = Δruntime.GoroutineProfile(records);
                var after = GoStwPauseSampleCounts();

                Assert.IsTrue(ok, $"n {n}: {len(records)} records hold the whole profile");
                Assert.IsTrue(n >= 4, $"n {n}: the caller and its three parked children exist");
                AssertMoved(before, after, gc: false, "runtime.GoroutineProfile (fill)");

                uintptr parked = (uintptr)GoSyntheticPC.Of(ProfileParkedMethod);
                int atParked = 0;

                for (nint i = 0; i < n; i++)
                {
                    if (records[i].Stack0[0] == parked)
                        atParked++;
                }

                Assert.AreEqual(3, atParked, $"records whose stack is ProfileParked's synthetic PC, of n {n}");

                (string? firstFailure, bool secondCompleted) = GoStopTheWorldTwiceProbe(TimeoutMs);
                Assert.IsTrue(secondCompleted, $"worldsema was left held; a stop after the fill: {firstFailure ?? "ok"}");
            }
        }
        finally
        {
            c.Close();
        }
    }

    // NoInlining: this frame IS the start function the fill arm identifies its records by.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ProfileParked(channel<int> c) => c.Receive();

    private static MethodBase ProfileParkedMethod =>
        typeof(RuntimeStopTheWorldContractTests).GetMethod(nameof(ProfileParked), BindingFlags.NonPublic | BindingFlags.Static)!;

    // Waits until golib's registry holds `count` goroutines started at ProfileParked, so the profile
    // taken next cannot race their registration.
    private static void AwaitParked(int count)
    {
        RuntimeMethodHandle parked = ProfileParkedMethod.MethodHandle;
        int Live() => Goroutine.ProfileSnapshot().Count(e => e.Function is not null && e.Function.MethodHandle == parked);

        for (int i = 0; i < 600 && Live() < count; i++)
            Thread.Sleep(5);

        Assert.AreEqual(count, Live(), "goroutines started at ProfileParked");
    }
    [TestMethod]
    public void WriteHeapDumpWritesAMinimalDumpAndRecordsAnOtherPause()
    {
        string path = Path.Combine(Path.GetTempPath(), $"go2cs-heapdump-{Guid.NewGuid():N}");

        try
        {
            var before = GoStwPauseSampleCounts();

            using (FileStream file = new(path, FileMode.CreateNew, FileAccess.ReadWrite))
                debug.WriteHeapDump((uintptr)(nuint)file.SafeFileHandle.DangerousGetHandle());

            var after = GoStwPauseSampleCounts();

            AssertMoved(before, after, gc: false, "debug.WriteHeapDump");

            // The dump's bytes are read where the runtime's write is a file-descriptor write.
            if (OperatingSystem.IsLinux())
            {
                byte[] dump = File.ReadAllBytes(path);
                byte[] header = Encoding.ASCII.GetBytes("go1.7 heap dump\n");

                Assert.IsTrue(dump.Length > header.Length + 1, $"{dump.Length} bytes");
                CollectionAssert.AreEqual(header, dump[..header.Length], "the dump header");
                Assert.AreEqual((byte)6, dump[header.Length], "tagParams follows the header");
                Assert.AreEqual((byte)0, dump[^1], "the dump ends with tagEOF");
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void ReadMetricsLockedCrossesTheRawSampleAddress()
    {
        // TestReadMetrics' ReadMetricsSlow hands readMetricsLocked the raw address of the caller's
        // []runtime/metrics.Sample backing store, which the runtime cannot name.
        slice<metrics.Sample> samples = new(2);
        samples[0].Name = "/sched/gomaxprocs:threads";
        samples[1].Name = "/sched/pauses/total/other:seconds";

        GoReadMetricsLockedProbe(@unsafe.Pointer.FromPinnedBox(Ꮡ(samples, 0)), len(samples), cap(samples));

        Assert.AreEqual(metrics.KindUint64, metrics.Kind(samples[0].Value), "/sched/gomaxprocs:threads");
        Assert.AreEqual((ulong)Δruntime.GOMAXPROCS(0), metrics.Uint64(samples[0].Value), "/sched/gomaxprocs:threads");
        Assert.AreEqual(metrics.KindFloat64Histogram, metrics.Kind(samples[1].Value), "/sched/pauses/total/other:seconds");
    }

    // ---- helpers ------------------------------------------------------------------------------

    private static void AssertMoved(
        (ulong stoppingGC, ulong stoppingOther, ulong totalGC, ulong totalOther) before,
        (ulong stoppingGC, ulong stoppingOther, ulong totalGC, ulong totalOther) after,
        bool gc, string what)
    {
        string reading = $"{what}: stopping gc {before.stoppingGC}->{after.stoppingGC}, stopping other {before.stoppingOther}->{after.stoppingOther}, " +
                         $"total gc {before.totalGC}->{after.totalGC}, total other {before.totalOther}->{after.totalOther}";

        if (gc)
        {
            Assert.IsTrue(after.stoppingGC > before.stoppingGC, reading);
            Assert.IsTrue(after.totalGC > before.totalGC, reading);
            Assert.AreEqual(before.stoppingOther, after.stoppingOther, reading);
            Assert.AreEqual(before.totalOther, after.totalOther, reading);
        }
        else
        {
            Assert.IsTrue(after.stoppingOther > before.stoppingOther, reading);
            Assert.IsTrue(after.totalOther > before.totalOther, reading);
            Assert.AreEqual(before.stoppingGC, after.stoppingGC, reading);
            Assert.AreEqual(before.totalGC, after.totalGC, reading);
        }
    }
}
