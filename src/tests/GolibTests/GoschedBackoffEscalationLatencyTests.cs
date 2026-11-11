// GoschedBackoffEscalationLatencyTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

// GOSCHED'S "1 ms" ESCALATION MUST BE 1 ms ON WINDOWS (i9, 2026-09-26, COORD ruling after 03df2f43aa).
// GoschedBackoff escalates the 64th consecutive inert yield to a 1 ms sleep (board finding 2026-08-21,
// ratified). On Windows 11, Thread.Sleep(1) runs at the PROCESS's timer resolution, and a process that
// never asked for a finer one sleeps a whole ~15.6 ms tick: measured in-process on the i9 at p50
// 15.75 ms / p99 16.49 ms, while NtQueryTimerResolution reported 1 ms machine-wide. A lone yielder is
// inert on every call, so it paid ~15.7 ms per 64 yields: 237 s per million, against ~0.1 s in Go.
// runtime/pprof's TestGoroutineSwitch (10 x 1M Gosched) overran its 30 m package deadline, and 32 later
// leaves had no verdict. The ruled remedy sleeps on a high-resolution waitable timer (the one time_impl
// and Go's own Windows runtime use): 1 ms is then ~1.1-1.5 ms.
//
// MACOS TOO (C1, 2026-11-08, COORD ruling): the escalation's high-resolution clock is Windows-only, so darwin
// sleeps with Thread.Sleep(1), and runtime/pprof's TestGoroutineSwitch overran its 900 s per-test deadline on the
// hosted osx-arm64 runner with the test thread in that sleep at both 120 s and 870 s (stack-probe run 42438677029):
// 10 x 1M lone Gosched calls make 156,250 escalations, so each cost more than 5.76 ms there. osx-x64 finished
// inside the deadline. Measured on the hosted runners (runs 42443148330, 42445042481), one escalation over
// Thread.Sleep(1): osx-arm64 mean 6.9-8.3 ms, p50 9.0, p90 9.1-10.6; osx-x64 mean 2.7-3.0, p50 1.5-1.6, p90 6.9-7.1. The
// remedy waits on a one-shot kqueue EVFILT_TIMER with NOTE_CRITICAL (GoCriticalTimer), measured at p50 1.02 / p90
// 1.03 ms (arm64) and 1.11 / 1.16 ms (x64); docs/phase4/DESIGN-darwin-gosched-clock.md.
//
// The arm drives the REAL GoschedBackoff.Yield on a fresh thread (a clean per-thread streak) for 64 x 500 lone yields
// and TIMES EACH CALL THAT ESCALATED (the escalation counter moved during it), so it bounds the DISTRIBUTION of one
// escalation's cost; beside it, the same statistics for a bare Thread.Sleep(1). Both go to standard output as
// READING lines (the os-matrix golib-tests stage prints them from the trx). A loaded box turns some yields effective
// and so changes how many escalations there are, but not what each one costs. It bounds the TIMED escalations, not the
// loop's total over the count: on the mac runners the calls that only yield took a derived 41-391 us apiece, a
// reading that is NOT ROOTED and that the total would charge to the escalation (on Windows a yield is ~300 ns, and the
// two agree). Bounds:
// - mean under 5 ms, everywhere: Windows red ~15.7 ms, green ~1.1-1.5 ms; osx-arm64 red 6.9-8.3 ms.
// - p90 under 3 ms, on macOS only: osx-x64's Sleep(1) has a mean near 3 ms but a p90 near 7 ms, which the mean alone
//   would pass; green 1.03-1.16 ms. Windows has no p90 reading, so none is bounded there.
// It also requires at least 200 escalations, so the path it bounds actually ran (500 expected when every yield is inert;
// 275-396 measured on the mac runners, and 81 on one loaded osx-x64 run). The loop therefore takes the 64 x 500 yields
// and then continues until 200 escalations are timed or two minutes have passed (TimeEscalations): a loaded host takes
// longer to reach the floor instead of failing it, and the bounds are unchanged. Linux keeps Sleep(1), ~1.1 ms there,
// and is not run.
//
// THE PAIRED READING (COORD, 2026-11-11; a measurement, the bounds above are unchanged). The macOS bounds are absolute,
// and the hosted mac runners have a slow mode that makes an absolute wall-clock bar read the host; COORD's rule wants
// 10x over the typical or a paired host reference, and red and green here are only ~5x apart (p90 green 1.08-1.55 ms,
// a coalesced Sleep(1) 7.2-10.2 ms), so no widened bound can do both. So on macOS each timed escalation is followed at
// once, on the same thread, by the host's own one-shot NOTE_CRITICAL 1 ms kqueue wait (GoTimerTests.kernelWait, the
// test's P/Invoke and not GoCriticalTimer's), and the pairs print as READING lines: the host wait's distribution and
// the escalation minus the host, per pair. The paired bar follows from those readings, as GoTimer's did (5861ecd1d4,
// then 375a6617de). Windows is unpaired and keeps its mean bound.
[TestClass]
public class GoschedBackoffEscalationLatencyTests
{
    private const int Yields = 64 * 500;
    private const int MinimumEscalations = 200;
    private const int SleepSamples = 500;
    private const double BoundMeanMilliseconds = 5.0;
    private const double BoundDarwinP90Milliseconds = 3.0;
    private static readonly TimeSpan Cap = TimeSpan.FromSeconds(120);

    [TestMethod]
    public void AnEscalationSleepsAboutAMillisecondNotATimerTick()
    {
        if (!OperatingSystem.IsWindows() && !OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive("the escalation's sleep is measured on Windows and macOS; " +
                "linux keeps Sleep(1), ~1 ms there");
            return;
        }

        long escalations = 0;
        long yields = 0;
        double elapsedMs = 0;
        var perEscalationMs = new List<double>(Yields / 64 + 16);
        var sleepMs = new double[SleepSamples];
        var hostMs = new List<double>(Yields / 64 + 16);

        // On macOS each timed escalation is paired with the host's own 1 ms wait on the same clock class.
        Func<double> hostWait = OperatingSystem.IsMacOS() ? HostCriticalWaitMilliseconds : null;

        // A fresh thread: its [ThreadStatic] inert streak starts at zero, as a new goroutine's does.
        var worker = new Thread(() =>
        {
            long before = GoschedBackoff.Escalations;

            (yields, elapsedMs) = TimeEscalations(() => GoschedBackoff.Escalations, GoschedBackoff.Yield, perEscalationMs, Cap, hostWait, hostMs);
            escalations = GoschedBackoff.Escalations - before;

            for (int i = 0; i < SleepSamples; i++)
            {
                long sleepStarted = Stopwatch.GetTimestamp();
                Thread.Sleep(1);
                sleepMs[i] = (Stopwatch.GetTimestamp() - sleepStarted) * 1000.0 / Stopwatch.Frequency;
            }
        });

        worker.Start();
        worker.Join();

        string host = $"{RuntimeInformation.OSDescription.Trim()}, {RuntimeInformation.ProcessArchitecture}, " +
            $"{Environment.ProcessorCount} processors";

        Console.WriteLine($"READING host: {host}");
        Console.WriteLine($"READING {yields} lone yields: {escalations} escalations in {elapsedMs:F0} ms, " +
            $"{(escalations > 0 ? elapsedMs / escalations : 0):F3} ms per escalation overall");
        Console.WriteLine($"READING one escalation's cost (ms): {Describe(perEscalationMs.ToArray())}");
        Console.WriteLine($"READING Thread.Sleep(1) (ms): {Describe(sleepMs)}");

        if (hostWait is not null)
        {
            int answered = hostMs.Count(ms => !double.IsNaN(ms));

            if (answered == perEscalationMs.Count && answered > 0)
            {
                double[] excess = perEscalationMs.Select((ms, i) => ms - hostMs[i]).ToArray();
                Console.WriteLine($"READING host NOTE_CRITICAL 1 ms wait, paired (ms): {Describe(hostMs.ToArray())}");
                Console.WriteLine($"READING escalation minus host, per pair (ms): {Describe(excess)}");
            }
            else
            {
                Console.WriteLine($"READING host NOTE_CRITICAL 1 ms wait: {answered} of {perEscalationMs.Count} answered, no pairs");
            }
        }

        Assert.IsTrue(perEscalationMs.Count >= MinimumEscalations,
            $"only {perEscalationMs.Count} escalations in {yields} lone yields -- the escalation path barely ran, " +
            "so its cost is unmeasured here");

        double[] sorted = perEscalationMs.ToArray();
        Array.Sort(sorted);
        double mean = Mean(sorted), p90 = Percentile(sorted, 0.90);

        Assert.IsTrue(mean < BoundMeanMilliseconds,
            $"one escalation costs {mean:F2} ms on average (bound {BoundMeanMilliseconds} ms; {Describe(sorted)}): " +
            "the 1 ms escalation is sleeping far longer than 1 ms " +
            "(a whole timer tick on Windows, ~15.7 ms; ~9 ms on osx-arm64)");

        if (OperatingSystem.IsMacOS())
        {
            Assert.IsTrue(p90 < BoundDarwinP90Milliseconds,
                $"one escalation's p90 is {p90:F2} ms " +
                $"(bound {BoundDarwinP90Milliseconds} ms on macOS; {Describe(sorted)}): " +
                "the escalation is waiting on a coalesced timer rather than about 1 ms");
        }
    }

    // The yield loop: at least Yields lone yields, timing each call during which the escalation counter moved, and
    // adding to `timed`; then on until MinimumEscalations are timed or `cap` of wall time has passed. A host whose load
    // turns most yields effective takes longer to reach the floor rather than failing it, and the bounds read the
    // same distribution. Returns the yields taken and the wall time they took.
    // With `pair`, each timed escalation is followed at once, on the same thread, by pair(), whose milliseconds are
    // added to `paired` (NaN where it took no reading), so timed[i] and paired[i] are a pair.
    internal static (long Yields, double ElapsedMs) TimeEscalations(Func<long> escalations, Action yield, List<double> timed, TimeSpan cap,
        Func<double> pair = null, List<double> paired = null)
    {
        long started = Stopwatch.GetTimestamp();
        long capTicks = (long)(cap.TotalSeconds * Stopwatch.Frequency);
        long taken = 0;

        for (; taken < Yields || (timed.Count < MinimumEscalations && Stopwatch.GetTimestamp() - started < capTicks); taken++)
        {
            long count = escalations();
            long callStarted = Stopwatch.GetTimestamp();

            yield();

            long callEnded = Stopwatch.GetTimestamp();

            if (escalations() != count)
            {
                timed.Add((callEnded - callStarted) * 1000.0 / Stopwatch.Frequency);

                if (pair is not null)
                    paired.Add(pair());
            }
        }

        return (taken, (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency);
    }

    // THE FLOOR UNDER LOAD (item 3 of the post-.3 queue). The inert-yield classifier decides how many escalations a
    // fixed number of yields makes, and a loaded host makes fewer: 275-396 per 32,000 yields on the mac runners, and 81
    // on one osx-x64 run, which failed the 200 floor with the path's cost never in question. A yield that escalates
    // once per 395 calls (that run's rate) must still reach the floor, and the bounds are unchanged; the loop is what
    // adapts. Deterministic, so it runs on every OS.
    [TestMethod]
    public void ASparseEscalationRateStillReachesTheFloor()
    {
        long fake = 0, calls = 0;
        var timed = new List<double>();

        (long yields, _) = TimeEscalations(() => fake, () => { if (++calls % 395 == 0) fake++; }, timed, Cap);

        Assert.IsTrue(timed.Count >= MinimumEscalations,
            $"a yield escalating once per 395 calls gave {timed.Count} timed escalations in {yields} yields, under the floor of {MinimumEscalations}");
        Assert.IsTrue(yields >= Yields, $"the loop took {yields} yields, fewer than the {Yields} every reading has taken");
    }

    // The cap: a yield that never escalates ends at the cap rather than looping forever, and reports what it took.
    [TestMethod]
    public void AYieldThatNeverEscalatesStopsAtTheCap()
    {
        var timed = new List<double>();
        Stopwatch wall = Stopwatch.StartNew();

        (long yields, _) = TimeEscalations(() => 0, () => { }, timed, TimeSpan.FromMilliseconds(200));

        Assert.AreEqual(0, timed.Count);
        Assert.IsTrue(yields >= Yields, $"the loop took {yields} yields, fewer than {Yields}");
        Assert.IsTrue(wall.Elapsed < TimeSpan.FromSeconds(30), $"the loop ran {wall.Elapsed.TotalSeconds:F1} s against a 200 ms cap");
    }

    // The host's own 1 ms NOTE_CRITICAL wait (GoTimerTests' kernelWait: the test's P/Invoke, not GoCriticalTimer's, so a
    // change to golib's wait cannot move both sides of a pair), in milliseconds; NaN where it took no reading.
    private static double HostCriticalWaitMilliseconds()
    {
        long started = Stopwatch.GetTimestamp();
        bool answered = GoTimerTests.kernelWait(1_000_000);
        double ms = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;

        return answered ? ms : double.NaN;
    }

    private static double Mean(double[] samples)
    {
        double sum = 0;

        foreach (double sample in samples)
            sum += sample;

        return sum / samples.Length;
    }

    // The p-th percentile of SORTED samples, by the nearest-rank rule.
    private static double Percentile(double[] sorted, double p) =>
        sorted[Math.Min(sorted.Length - 1, (int)Math.Ceiling(p * sorted.Length) - 1)];

    // n, mean, p50, p90, p99 and max of the samples, in milliseconds.
    private static string Describe(double[] samples)
    {
        if (samples.Length == 0)
            return "n=0";

        var sorted = (double[])samples.Clone();
        Array.Sort(sorted);

        return $"n={sorted.Length} mean={Mean(sorted):F3} p50={Percentile(sorted, 0.50):F3} " +
            $"p90={Percentile(sorted, 0.90):F3} " +
            $"p99={Percentile(sorted, 0.99):F3} max={sorted[^1]:F3}";
    }
}
