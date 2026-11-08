// DarwinGoroutineThreadCapProbe.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;
using static go.builtin;

namespace GolibTests;

// A MEASUREMENT, not a guard (C1, 2026-11-08, COORD's ruling on internal/synctest at the 1.26.10.3 re-bank). Each
// converted goroutine is an OS thread, and on both hosted mac runners internal/synctest's TestWaitGroupManyBubbles
// (about 10,100 live goroutines) died with System.OutOfMemoryException from Thread.StartCore. This probe reads, on
// macOS, the three numbers the user-facing limitation needs: the kernel's per-process thread cap
// (kern.num_taskthreads, with kern.maxthreads and kern.maxthreadsperproc beside it); the number of live PARKED
// goroutines (each blocked in a channel receive, as the synctest bubbles park) at which a `go` statement (goǃ) first
// fails, with the process's OS thread count at that moment; and the exception the program sees there. Every parked
// goroutine is released before the test returns. It stops at 20,000 goroutines if nothing fails first. READING lines
// carry the numbers; it asserts only that it measured. Inconclusive off macOS.
[TestClass]
public partial class DarwinGoroutineThreadCapProbe
{
    private const int Ceiling = 20_000;

    [TestMethod]
    public void TheLiveGoroutineCountAtWhichAGoStatementFailsIsMeasured()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive("the thread cap measured here is darwin's; this probe runs on macOS only");
            return;
        }

        Console.WriteLine($"READING host: {RuntimeInformation.OSDescription.Trim()}, {RuntimeInformation.ProcessArchitecture}, " +
            $"{Environment.ProcessorCount} processors");
        Console.WriteLine($"READING kern.num_taskthreads={Sysctl("kern.num_taskthreads")} kern.maxthreads={Sysctl("kern.maxthreads")} " +
            $"kern.maxthreadsperproc={Sysctl("kern.maxthreadsperproc")}");

        int baselineThreads = Process.GetCurrentProcess().Threads.Count;
        channel<int> release = new(0);
        int live = 0;
        int launched = 0;
        Exception failure = null;
        int threadsAtFailure = 0;
        var clock = Stopwatch.StartNew();

        try
        {
            for (int i = 0; i < Ceiling; i++)
            {
                try
                {
                    goǃ(() =>
                    {
                        Interlocked.Increment(ref live);
                        release.Receive(true);
                        Interlocked.Decrement(ref live);
                    });
                    launched++;
                }
                catch (Exception ex)
                {
                    failure = ex;
                    threadsAtFailure = Process.GetCurrentProcess().Threads.Count;
                    break;
                }
            }

            // Let the launched goroutines reach their receive, so "live" counts parked goroutines.
            SpinWait.SpinUntil(() => Volatile.Read(ref live) >= launched, TimeSpan.FromSeconds(30));
            Console.WriteLine($"READING baseline OS threads before the probe: {baselineThreads}");
            Console.WriteLine($"READING goroutines launched before the first failure: {launched}; parked and live: {Volatile.Read(ref live)}; " +
                $"OS threads at the failure: {(failure is null ? Process.GetCurrentProcess().Threads.Count : threadsAtFailure)}; " +
                $"{clock.Elapsed.TotalSeconds:F1} s");

            if (failure is null)
                Console.WriteLine($"READING no go statement failed up to the ceiling of {Ceiling} live goroutines");
            else
            {
                Console.WriteLine($"READING the exception: {failure.GetType().FullName}: {failure.Message}");
                string[] frames = (failure.StackTrace ?? "").Split('\n');
                for (int f = 0; f < Math.Min(6, frames.Length); f++)
                    Console.WriteLine($"READING   frame {f}: {frames[f].Trim()}");
                if (failure.InnerException is not null)
                    Console.WriteLine($"READING   inner: {failure.InnerException.GetType().FullName}: {failure.InnerException.Message}");
            }
        }
        finally
        {
            release.Close();
            SpinWait.SpinUntil(() => Volatile.Read(ref live) == 0, TimeSpan.FromSeconds(60));
            Console.WriteLine($"READING released: {Volatile.Read(ref live)} goroutine(s) still live after the release");
        }

        Assert.IsTrue(launched > 0, "no goroutine launched, so nothing was measured");
    }

    private static string Sysctl(string name)
    {
        try
        {
            int value = 0;
            nint size = sizeof(int);

            return sysctlbyname(name, ref value, ref size, 0, 0) == 0 ? value.ToString() : $"(errno {Marshal.GetLastPInvokeError()})";
        }
        catch (Exception ex)
        {
            return $"(unavailable: {ex.GetType().Name})";
        }
    }

    [LibraryImport("libc", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    private static partial int sysctlbyname(string name, ref int oldp, ref nint oldlenp, nint newp, nint newlen);
}
