// RuntimeGOMAXPROCSUpdateTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

// The automatic GOMAXPROCS update (runtime/managed_impl.cs, W-gmp-e). Go's sysmon recomputes the default at
// most once a second while the setting is not custom, and sets it when it changed, so a program follows a
// change of its CPU affinity (and of its cgroup CPU limit) without a restart. These rows change the affinity
// of the WHOLE process, every thread, as `taskset -a` does, so the real sysmon tick sees the change it would
// see in Go, and wait for it.
//
// Linux and Windows: the affinity is the input on both. Darwin has no process affinity and hw.ncpu does not
// change while a process runs, so there is nothing to follow there. The assembly runs serially; each row puts
// the affinity and the setting back.
[TestClass]
public class RuntimeGOMAXPROCSUpdateTests
{
    // Go updates within a second of the change; the tick adds at most 100 ms. Three seconds is the wait.
    private static readonly TimeSpan Follow = TimeSpan.FromSeconds(3);

    // Long enough for two sysmon checks, for the rows that must see no change.
    private static readonly TimeSpan Hold = TimeSpan.FromSeconds(2.5);

    [TestMethod]
    public void GOMAXPROCSFollowsAnAffinityChange()
    {
        WithOneCpuAvailable(environmentExtra: null, shrunk =>
        {
            nint before = runtime_package.GOMAXPROCS(0);

            shrunk.Apply();
            Assert.IsTrue(WaitFor(1, Follow, out nint seen), $"GOMAXPROCS stayed {seen} after the affinity went from {before} CPUs to 1");

            shrunk.Restore();
            Assert.IsTrue(WaitFor(before, Follow, out seen), $"GOMAXPROCS stayed {seen} after the affinity went back to {before} CPUs");
        });
    }

    [TestMethod]
    public void AnExplicitSettingDoesNotFollow()
    {
        WithOneCpuAvailable(environmentExtra: null, shrunk =>
        {
            nint before = runtime_package.GOMAXPROCS(0);

            runtime_package.GOMAXPROCS(before);   // the same value, but now set by the program
            shrunk.Apply();
            Thread.Sleep(Hold);

            Assert.AreEqual(before, runtime_package.GOMAXPROCS(0), "an explicit GOMAXPROCS followed the affinity");
        });
    }

    [TestMethod]
    public void UpdatemaxprocsZeroTurnsItOff()
    {
        WithOneCpuAvailable(environmentExtra: "GODEBUG=updatemaxprocs=0", shrunk =>
        {
            nint before = runtime_package.GOMAXPROCS(0);

            shrunk.Apply();
            Thread.Sleep(Hold);

            Assert.AreEqual(before, runtime_package.GOMAXPROCS(0), "GOMAXPROCS followed the affinity under GODEBUG=updatemaxprocs=0");
        });
    }

    // Starts the runtime's GOMAXPROCS over with no GOMAXPROCS variable (so the setting is not custom) and the
    // given extra variable, hands the body a way to narrow the process to one CPU, and puts everything back.
    private static void WithOneCpuAvailable(string? environmentExtra, Action<Affinity> body)
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsWindows())
            Assert.Inconclusive("no process affinity to change on this host; hw.ncpu does not change while a process runs");

        Affinity affinity = Affinity.Read();

        if (affinity.Count < 2)
            Assert.Inconclusive($"the process may run on {affinity.Count} CPU; narrowing it would change nothing");

        string[] original = runtime_package.GoStartEnvironment();
        string[] start = original
            .Where(e => !e.StartsWith("GOMAXPROCS=", StringComparison.Ordinal) && !e.StartsWith("GODEBUG=", StringComparison.Ordinal))
            .Concat(environmentExtra is null ? [] : [environmentExtra])
            .ToArray();

        try
        {
            runtime_package.GoStartGOMAXPROCS(start);
            Assert.AreEqual(runtime_package.GoDefaultGOMAXPROCSProbe(), runtime_package.GOMAXPROCS(0), "the start was not the default");

            body(affinity);
        }
        finally
        {
            affinity.Restore();
            runtime_package.GoStartGOMAXPROCS(original);
        }
    }

    private static bool WaitFor(nint want, TimeSpan within, out nint seen)
    {
        Stopwatch clock = Stopwatch.StartNew();

        while (true)
        {
            seen = runtime_package.GOMAXPROCS(0);

            if (seen == want)
            {
                Console.WriteLine($"GOMAXPROCS reached {want} after {clock.Elapsed.TotalMilliseconds:F0} ms");
                return true;
            }

            if (clock.Elapsed > within)
                return false;

            Thread.Sleep(50);
        }
    }

    // The process's CPU affinity: read once, narrowed to its first CPU for every thread, and put back.
    private sealed class Affinity
    {
        private readonly byte[] m_linuxMask = new byte[128];
        private nuint m_windowsMask;

        internal int Count { get; private set; }

        internal static Affinity Read()
        {
            Affinity a = new();

            if (OperatingSystem.IsLinux())
            {
                if (sched_getaffinity(0, (nuint)a.m_linuxMask.Length, a.m_linuxMask) != 0)
                    throw new InvalidOperationException($"sched_getaffinity failed: {Marshal.GetLastPInvokeError()}");

                a.Count = a.m_linuxMask.Sum(b => BitOperations.PopCount(b));
            }
            else
            {
                if (!GetProcessAffinityMask(GetCurrentProcess(), out a.m_windowsMask, out _))
                    throw new InvalidOperationException($"GetProcessAffinityMask failed: {Marshal.GetLastPInvokeError()}");

                a.Count = BitOperations.PopCount(a.m_windowsMask);
            }

            return a;
        }

        internal void Apply()
        {
            if (OperatingSystem.IsLinux())
            {
                byte[] one = new byte[m_linuxMask.Length];
                int first = Enumerable.Range(0, m_linuxMask.Length * 8).First(i => (m_linuxMask[i / 8] & (1 << (i % 8))) != 0);
                one[first / 8] = (byte)(1 << (first % 8));
                SetEveryThread(one);
            }
            else
            {
                nuint first = m_windowsMask & (nuint)(-(nint)m_windowsMask);
                if (!SetProcessAffinityMask(GetCurrentProcess(), first))
                    throw new InvalidOperationException($"SetProcessAffinityMask failed: {Marshal.GetLastPInvokeError()}");
            }
        }

        internal void Restore()
        {
            if (OperatingSystem.IsLinux())
                SetEveryThread(m_linuxMask);
            else
                SetProcessAffinityMask(GetCurrentProcess(), m_windowsMask);
        }

        // Every thread of the process, as `taskset -a` does; twice, so a thread started by a narrowed thread
        // between the two passes is caught by the second.
        private static void SetEveryThread(byte[] mask)
        {
            for (int pass = 0; pass < 2; pass++)
            {
                foreach (string task in Directory.GetDirectories("/proc/self/task"))
                {
                    if (int.TryParse(Path.GetFileName(task), out int tid))
                        sched_setaffinity(tid, (nuint)mask.Length, mask);   // a thread that has exited fails; that is fine
                }
            }
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int sched_getaffinity(int pid, nuint cpusetsize, byte[] mask);

        [DllImport("libc", SetLastError = true)]
        private static extern int sched_setaffinity(int pid, nuint cpusetsize, byte[] mask);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern nint GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool GetProcessAffinityMask(nint hProcess, out nuint lpProcessAffinityMask, out nuint lpSystemAffinityMask);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool SetProcessAffinityMask(nint hProcess, nuint dwProcessAffinityMask);
    }
}
