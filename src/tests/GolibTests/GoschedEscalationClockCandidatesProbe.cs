// GoschedEscalationClockCandidatesProbe.cs - Gbtc
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

// A MEASUREMENT, not a guard (C1, 2026-11-08, COORD's darwin clock seat, design note first). GoschedBackoff's 1 ms
// escalation on macOS is Thread.Sleep(1), measured on the hosted runners at p50 9.0 ms (osx-arm64) and 1.4 ms
// (osx-x64) by GoschedBackoffEscalationLatencyTests (run 42443148330). This probe times each candidate 1 ms wait on
// the same runners, 500 waits apiece on a fresh thread, and reports the latency distribution and the thread CPU each
// wait burned (thread_info, microsecond resolution) as READING lines. A candidate that cannot run on this host is
// reported as unavailable with its exception, and the others still run. It asserts only that the baseline ran.
[TestClass]
public partial class GoschedEscalationClockCandidatesProbe
{
    private const int Samples = 500;
    private const long OneMillisecondNanos = 1_000_000;

    [TestMethod]
    public void EachCandidateOneMillisecondWaitIsMeasured()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive("the candidates are darwin's; this probe measures them on macOS only");
            return;
        }

        var candidates = new List<(string Name, Action Wait)>
        {
            ("Thread.Sleep(1)", () => Thread.Sleep(1)),
            ("nanosleep(1 ms)", NanoSleep),
            ("mach_wait_until(+1 ms)", MachWaitUntil),
            ("kevent EVFILT_TIMER 1 ms NOTE_CRITICAL", KeventTimer),
            ("Thread.Yield spin to 1 ms", YieldSpin)
        };

        var lines = new List<string>();
        bool baselineRan = false;

        var worker = new Thread(() =>
        {
            long token = ThreadCpuTime.CurrentThreadToken();
            using ThreadCpuReader? cpu = token != 0 ? ThreadCpuTime.Open(token) : null;

            foreach ((string name, Action wait) in candidates)
            {
                try
                {
                    wait(); // a warm call: the first P/Invoke binds its export

                    var samples = new double[Samples];
                    TimeSpan cpuBefore = TimeSpan.Zero, cpuAfter = TimeSpan.Zero;
                    bool cpuRead = cpu is not null && cpu.TryRead(out cpuBefore);

                    for (int i = 0; i < Samples; i++)
                    {
                        long started = Stopwatch.GetTimestamp();
                        wait();
                        samples[i] = (Stopwatch.GetTimestamp() - started) * 1000.0 / Stopwatch.Frequency;
                    }

                    cpuRead = cpuRead && cpu!.TryRead(out cpuAfter);
                    string cpuText = cpuRead
                        ? $"{(cpuAfter - cpuBefore).TotalMilliseconds * 1000.0 / Samples:F1} us CPU per wait"
                        : "CPU unread";

                    lines.Add($"READING {name} (ms): {Describe(samples)}; {cpuText}");

                    if (name == "Thread.Sleep(1)")
                        baselineRan = true;
                }
                catch (Exception ex)
                {
                    lines.Add($"READING {name}: unavailable -- {ex.GetType().Name}: {ex.Message}");
                }
            }
        });

        worker.Start();
        worker.Join();

        Console.WriteLine($"READING host: {RuntimeInformation.OSDescription.Trim()}, " +
            $"{RuntimeInformation.ProcessArchitecture}, {Environment.ProcessorCount} processors");

        foreach (string line in lines)
            Console.WriteLine(line);

        Assert.IsTrue(baselineRan, "the Thread.Sleep(1) baseline did not run, so no candidate has a comparison here");
    }

    // ---- the candidates --------------------------------------------------------------------------------------------

    private static void NanoSleep()
    {
        Timespec request = new() { Seconds = 0, Nanoseconds = (nint)OneMillisecondNanos };

        if (nanosleep(in request, 0) != 0)
            throw new InvalidOperationException($"nanosleep failed, errno {Marshal.GetLastPInvokeError()}");
    }

    private static MachTimebase s_timebase;

    private static void MachWaitUntil()
    {
        if (s_timebase.Denominator == 0 && mach_timebase_info(out s_timebase) != 0)
            throw new InvalidOperationException("mach_timebase_info failed");

        // nanoseconds -> mach absolute time units: ns * denom / numer
        ulong delta = (ulong)OneMillisecondNanos * s_timebase.Denominator / s_timebase.Numerator;
        int kr = mach_wait_until(mach_absolute_time() + delta);

        if (kr != 0)
            throw new InvalidOperationException($"mach_wait_until returned {kr}");
    }

    [ThreadStatic]
    private static int t_kqueue;

    private static unsafe void KeventTimer()
    {
        if (t_kqueue == 0)
        {
            int kq = kqueue();

            if (kq < 0)
                throw new InvalidOperationException($"kqueue failed, errno {Marshal.GetLastPInvokeError()}");

            t_kqueue = kq;
        }

        Kevent change = new()
        {
            Ident = 1,
            Filter = EvfiltTimer,
            Flags = EvAdd | EvOneshot,
            Fflags = NoteUseconds | NoteCritical,
            Data = 1000
        };

        Kevent fired;
        int n = kevent(t_kqueue, &change, 1, &fired, 1, null);

        if (n != 1)
            throw new InvalidOperationException($"kevent returned {n}, errno {Marshal.GetLastPInvokeError()}");
    }

    private static void YieldSpin()
    {
        long until = Stopwatch.GetTimestamp() + Stopwatch.Frequency / 1000;

        while (Stopwatch.GetTimestamp() < until)
            Thread.Yield();
    }

    // n, mean, p50, p90, p99 and max of the samples, in milliseconds.
    private static string Describe(double[] samples)
    {
        var sorted = (double[])samples.Clone();
        Array.Sort(sorted);

        double Percentile(double p) => sorted[Math.Min(sorted.Length - 1, (int)Math.Ceiling(p * sorted.Length) - 1)];

        double sum = 0;

        foreach (double sample in sorted)
            sum += sample;

        return $"n={sorted.Length} mean={sum / sorted.Length:F3} p50={Percentile(0.50):F3} p90={Percentile(0.90):F3} " +
            $"p99={Percentile(0.99):F3} max={sorted[^1]:F3}";
    }

    // ---- darwin libc (libSystem) -----------------------------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    private struct Timespec
    {
        public nint Seconds;
        public nint Nanoseconds;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MachTimebase
    {
        public uint Numerator;
        public uint Denominator;
    }

    // struct kevent (sys/event.h), 64-bit: uintptr_t ident; int16_t filter; uint16_t flags; uint32_t fflags;
    // intptr_t data; void *udata -- 32 bytes.
    [StructLayout(LayoutKind.Sequential)]
    private struct Kevent
    {
        public nuint Ident;
        public short Filter;
        public ushort Flags;
        public uint Fflags;
        public nint Data;
        public nint Udata;
    }

    private const short EvfiltTimer = -7;
    private const ushort EvAdd = 0x0001;
    private const ushort EvOneshot = 0x0010;
    private const uint NoteUseconds = 0x00000002;
    private const uint NoteCritical = 0x00000020;

    [LibraryImport("libc", SetLastError = true)]
    private static partial int nanosleep(in Timespec request, nint remaining);

    [LibraryImport("libc")]
    private static partial int mach_timebase_info(out MachTimebase info);

    [LibraryImport("libc")]
    private static partial ulong mach_absolute_time();

    [LibraryImport("libc")]
    private static partial int mach_wait_until(ulong deadline);

    [LibraryImport("libc", SetLastError = true)]
    private static partial int kqueue();

    [LibraryImport("libc", SetLastError = true)]
    private static unsafe partial int kevent(int kq, Kevent* changelist, int nchanges, Kevent* eventlist, int nevents,
        Timespec* timeout);
}
