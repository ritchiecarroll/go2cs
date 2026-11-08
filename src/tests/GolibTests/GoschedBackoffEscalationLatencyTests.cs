// GoschedBackoffEscalationLatencyTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
// 275-317 measured on the mac runners). Linux keeps Sleep(1), ~1.1 ms there, and is not run.
[TestClass]
public class GoschedBackoffEscalationLatencyTests
{
    private const int Yields = 64 * 500;
    private const int MinimumEscalations = 200;
    private const int SleepSamples = 500;
    private const double BoundMeanMilliseconds = 5.0;
    private const double BoundDarwinP90Milliseconds = 3.0;

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
        double elapsedMs = 0;
        var perEscalationMs = new List<double>(Yields / 64 + 16);
        var sleepMs = new double[SleepSamples];

        // A fresh thread: its [ThreadStatic] inert streak starts at zero, as a new goroutine's does.
        var worker = new Thread(() =>
        {
            long before = GoschedBackoff.Escalations;
            long started = Stopwatch.GetTimestamp();

            for (int i = 0; i < Yields; i++)
            {
                long count = GoschedBackoff.Escalations;
                long callStarted = Stopwatch.GetTimestamp();

                GoschedBackoff.Yield();

                long callEnded = Stopwatch.GetTimestamp();

                if (GoschedBackoff.Escalations != count)
                    perEscalationMs.Add((callEnded - callStarted) * 1000.0 / Stopwatch.Frequency);
            }

            elapsedMs = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;
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
        Console.WriteLine($"READING {Yields} lone yields: {escalations} escalations in {elapsedMs:F0} ms, " +
            $"{(escalations > 0 ? elapsedMs / escalations : 0):F3} ms per escalation overall");
        Console.WriteLine($"READING one escalation's cost (ms): {Describe(perEscalationMs.ToArray())}");
        Console.WriteLine($"READING Thread.Sleep(1) (ms): {Describe(sleepMs)}");

        Assert.IsTrue(perEscalationMs.Count >= MinimumEscalations,
            $"only {perEscalationMs.Count} escalations in {Yields} lone yields -- the escalation path barely ran, " +
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
