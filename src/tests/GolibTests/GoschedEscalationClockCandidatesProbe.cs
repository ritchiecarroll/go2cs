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

// A MEASUREMENT, not a guard (C1, 2026-11-08, COORD's darwin clock seat). On the hosted macOS runners
// Thread.Sleep(1) coalesces (p50 9.0 ms on osx-arm64), and so do nanosleep and mach_wait_until (run 42445042481,
// docs/phase4/DESIGN-darwin-gosched-clock.md). The escalation's macOS clock is GoCriticalTimer, a one-shot kqueue
// EVFILT_TIMER with NOTE_CRITICAL. This probe keeps the record behind that choice: 500 waits of 1 ms apiece on a
// fresh thread, for Thread.Sleep(1), the same kqueue timer WITHOUT NOTE_CRITICAL, the timer WITH it, and
// GoCriticalTimer.TrySleep itself (the shipping code; any wait it answers false for is counted, because the
// escalation then falls back to Sleep(1)). It reports each latency distribution and the thread CPU per wait
// (thread_info, microsecond resolution) as READING lines. A candidate that cannot run on this host is reported as
// unavailable with its exception, and the others still run. It asserts only that the baseline ran.
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

        int criticalTimerRefusals = 0;

        var candidates = new List<(string Name, Action Wait)>
        {
            ("Thread.Sleep(1)", () => Thread.Sleep(1)),
            ("kevent EVFILT_TIMER 1 ms, no NOTE_CRITICAL", () => KeventTimer(NoteUseconds)),
            ("kevent EVFILT_TIMER 1 ms, NOTE_CRITICAL", () => KeventTimer(NoteUseconds | NoteCritical)),
            ("GoCriticalTimer.TrySleep(1 ms)", () =>
            {
                if (!GoCriticalTimer.TrySleep(OneMillisecondNanos))
                    criticalTimerRefusals++;
            })
        };

        var lines = new List<string>();
        bool baselineRan = false;

        var worker = new Thread(() =>
        {
            long token = ThreadCpuTime.CurrentThreadToken();
            using ThreadCpuReader cpu = token != 0 ? ThreadCpuTime.Open(token) : null;

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

        Console.WriteLine($"READING GoCriticalTimer.TrySleep answered false {criticalTimerRefusals} time(s) of {Samples + 1}");

        Assert.IsTrue(baselineRan, "the Thread.Sleep(1) baseline did not run, so no candidate has a comparison here");
    }

    // ---- the candidates --------------------------------------------------------------------------------------------

    [ThreadStatic]
    private static int t_kqueue;

    private static unsafe void KeventTimer(uint fflags)
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
            Fflags = fflags,
            Data = 1000
        };

        Kevent fired;
        int n = kevent(t_kqueue, &change, 1, &fired, 1, null);

        if (n != 1)
            throw new InvalidOperationException($"kevent returned {n}, errno {Marshal.GetLastPInvokeError()}");
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
    private static partial int kqueue();

    [LibraryImport("libc", SetLastError = true)]
    private static unsafe partial int kevent(int kq, Kevent* changelist, int nchanges, Kevent* eventlist, int nevents,
        Timespec* timeout);
}
