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
// inside the deadline. The arm runs on macOS so the cost is measured on both mac runners rather than inferred.
//
// The arm drives the REAL GoschedBackoff.Yield on a fresh thread (a clean per-thread streak) for
// 64 x 500 lone yields and times each call that escalated (the escalation counter moved during it), so it reports
// the DISTRIBUTION of one escalation's cost; beside it, the same statistics for a bare Thread.Sleep(1), the
// primitive darwin's escalation uses. Both go to standard output as READING lines (the os-matrix golib-tests
// stage prints them from the trx). It bounds the MEAN COST PER ESCALATION, not the total. A loaded box turns some
// yields effective and so changes how many escalations there are, but not what each one costs. Margins on
// Windows: red ~15.7 ms per escalation, green ~1.1-1.5 ms, bound 5 ms -- over 3x clear on each side. It also
// requires at least 250 escalations, so the path it bounds actually ran (500 expected when every yield is
// inert). Linux keeps Sleep(1), which is ~1 ms there, and is not run.
[TestClass]
public class GoschedBackoffEscalationLatencyTests
{
    private const int Yields = 64 * 500;
    private const int MinimumEscalations = 250;
    private const int SleepSamples = 500;
    private const double BoundMillisecondsPerEscalation = 5.0;

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

        Assert.IsTrue(escalations >= MinimumEscalations,
            $"only {escalations} escalations in {Yields} lone yields -- the escalation path barely ran, so its cost is unmeasured here");

        double perEscalation = elapsedMs / escalations;

        Assert.IsTrue(perEscalation < BoundMillisecondsPerEscalation,
            $"{escalations} escalations took {elapsedMs:F0} ms, {perEscalation:F2} ms each (bound {BoundMillisecondsPerEscalation} ms): " +
            "the 1 ms escalation is sleeping far longer than 1 ms (a whole timer tick on Windows, ~15.7 ms)");
    }

    // n, mean, p50, p90, p99 and max of the samples, in milliseconds.
    private static string Describe(double[] samples)
    {
        if (samples.Length == 0)
            return "n=0";

        var sorted = (double[])samples.Clone();
        Array.Sort(sorted);

        double Percentile(double p) => sorted[Math.Min(sorted.Length - 1, (int)Math.Ceiling(p * sorted.Length) - 1)];

        double sum = 0;

        foreach (double sample in sorted)
            sum += sample;

        return $"n={sorted.Length} mean={sum / sorted.Length:F3} p50={Percentile(0.50):F3} p90={Percentile(0.90):F3} " +
            $"p99={Percentile(0.99):F3} max={sorted[^1]:F3}";
    }
}
