// GoTimerLatenessAgainstGoProbe.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go.golib;

namespace GolibTests;

// A MEASUREMENT, not a guard (C1, 2026-11-09, the post-.3 queue's item 2). GoTimerTests.AFiringIsCloseToItsDueTime
// reads a median 5 ms lateness of 12.9 ms on osx-arm64 and 8.9 ms on osx-x64 (master baseline run 42456242089), where
// its bar is 1.5 ms. Whether that is a gap against Go or the host's timer coalescing is decided by what Go's own
// timers read on the same host. This probe reads, in one process: Go's time.AfterFunc and time.Sleep at 5 ms
// (`go run` of a 20-firing program, the pinned oracle); golib's GoTimer at 5 ms, the same 20 firings
// AFiringIsCloseToItsDueTime takes, read before and after the Go arm so the C# arm brackets it; and, on macOS,
// GoCriticalTimer.TrySleep at 5 ms (a one-shot kqueue EVFILT_TIMER with NOTE_CRITICAL). Each arm prints a READING
// line, min / median / max in ms. It asserts only that the GoTimer arm ran.
[TestClass]
[DoNotParallelize]
public class GoTimerLatenessAgainstGoProbe
{
    private const int Firings = 20;
    private const long FiveMillisecondsNanos = 5_000_000;

    private const string GoProgram = """
        package main

        import (
        	"fmt"
        	"sort"
        	"time"
        )

        func report(name string, late []float64) {
        	sort.Float64s(late)
        	fmt.Printf("%s min %.3f median %.3f max %.3f\n", name, late[0], late[len(late)/2], late[len(late)-1])
        }

        func main() {
        	var afterFunc, sleep []float64

        	for i := 0; i < 20; i++ {
        		fired := make(chan time.Time, 1)
        		due := time.Now().Add(5 * time.Millisecond)
        		time.AfterFunc(5*time.Millisecond, func() { fired <- time.Now() })
        		afterFunc = append(afterFunc, float64((<-fired).Sub(due))/1e6)
        	}

        	for i := 0; i < 20; i++ {
        		due := time.Now().Add(5 * time.Millisecond)
        		time.Sleep(5 * time.Millisecond)
        		sleep = append(sleep, float64(time.Since(due))/1e6)
        	}

        	report("time.AfterFunc", afterFunc)
        	report("time.Sleep", sleep)
        }
        """;

    [TestMethod]
    public void TimerLatenessIsMeasuredAgainstGosOwn()
    {
        string host = $"{System.Runtime.InteropServices.RuntimeInformation.OSDescription.Trim()}, " +
            $"{System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}";

        List<string> lines = [];

        double[] before = GoTimerLateness();
        lines.Add(Line("GoTimer 5 ms (before the Go arm)", before));

        (string? go, string? skip) = ExecutionTracerParserTests.Oracle();

        if (go is null)
            lines.Add($"Go 5 ms: unavailable -- {skip}");
        else
            lines.AddRange(GoLateness(go));

        lines.Add(Line("GoTimer 5 ms (after the Go arm)", GoTimerLateness()));

        if (OperatingSystem.IsMacOS())
        {
            int refusals = 0;
            double[] critical = new double[Firings];

            for (int i = 0; i < Firings; i++)
            {
                long start = MonotonicClock.Nanoseconds();

                if (!GoCriticalTimer.TrySleep(FiveMillisecondsNanos))
                    refusals++;

                critical[i] = (MonotonicClock.Nanoseconds() - start - FiveMillisecondsNanos) / 1e6;
            }

            lines.Add(Line($"GoCriticalTimer.TrySleep 5 ms (refused {refusals})", critical));
        }

        foreach (string line in lines)
            Console.WriteLine($"READING timer lateness, ms: {line} ({host})");

        Assert.AreEqual(Firings, before.Length, "the GoTimer arm did not run");
    }

    // GoCriticalTimer.Waiter's paths, on a background thread made as GoTimer's service thread is (C1, 2026-11-09): at
    // 5c37f9745 AFiringIsCloseToItsDueTime read a tight median of 2.5 ms on osx-arm64 (max 2.6), where TrySleep reads
    // 0.1 ms. The arms separate the wait's own paths: a fresh waiter per wait; one waiter reused; and one reused
    // waiter after a Wake that came first, so that wait returns at once and the measured one RE-ARMS the timer it
    // left registered (EV_ADD on a live knote), which is GoTimer's path when an arm wakes the service thread.
    [TestMethod]
    public void TheCriticalWaitersPathsAreMeasured()
    {
        if (!OperatingSystem.IsMacOS())
        {
            Assert.Inconclusive("GoCriticalTimer.Waiter is macOS's");
            return;
        }

        List<string> lines = [];

        var worker = new Thread(() =>
        {
            lines.Add(Line("TrySleep 5 ms", Measure(() => GoCriticalTimer.TrySleep(FiveMillisecondsNanos))));
            lines.Add(Line("fresh Waiter per wait, Wait 5 ms", Measure(() => GoCriticalTimer.Waiter.TryCreate()!.Wait(FiveMillisecondsNanos))));

            GoCriticalTimer.Waiter reused = GoCriticalTimer.Waiter.TryCreate()!;
            lines.Add(Line("one Waiter reused, Wait 5 ms", Measure(() => reused.Wait(FiveMillisecondsNanos))));

            GoCriticalTimer.Waiter woken = GoCriticalTimer.Waiter.TryCreate()!;
            double[] afterWake = new double[Firings];

            for (int i = 0; i < Firings; i++)
            {
                woken.Wake();
                woken.Wait(FiveMillisecondsNanos);       // returns at once, leaving the timer armed
                long start = MonotonicClock.Nanoseconds();
                woken.Wait(FiveMillisecondsNanos);       // re-arms the live timer
                afterWake[i] = (MonotonicClock.Nanoseconds() - start - FiveMillisecondsNanos) / 1e6;
            }

            lines.Add(Line("reused Waiter after a Wake, the re-arming Wait 5 ms", afterWake));
        })
        { IsBackground = true, Name = "critical waiter probe" };

        worker.Start();
        worker.Join();
        lines.Add(Line("GoTimer 5 ms", GoTimerLateness()));

        string host = $"{System.Runtime.InteropServices.RuntimeInformation.OSDescription.Trim()}, " +
            $"{System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}";

        foreach (string line in lines)
            Console.WriteLine($"READING critical waiter, lateness ms: {line} ({host})");

        Assert.AreEqual(5, lines.Count, "an arm did not run");

        static double[] Measure(Func<bool> wait)
        {
            double[] late = new double[Firings];

            for (int i = 0; i < Firings; i++)
            {
                long start = MonotonicClock.Nanoseconds();
                wait();
                late[i] = (MonotonicClock.Nanoseconds() - start - FiveMillisecondsNanos) / 1e6;
            }

            return late;
        }
    }

    // The firings AFiringIsCloseToItsDueTime takes: a fresh GoTimer per firing, due 5 ms out.
    private static double[] GoTimerLateness()
    {
        double[] late = new double[Firings];

        for (int i = 0; i < Firings; i++)
        {
            long firedAt = 0;
            using ManualResetEventSlim done = new();
            long due = MonotonicClock.Nanoseconds() + FiveMillisecondsNanos;

            new GoTimer(_ => { firedAt = MonotonicClock.Nanoseconds(); done.Set(); }, null).Reset(due);

            Assert.IsTrue(done.Wait(5_000), "a 5 ms GoTimer did not fire within 5 s");
            late[i] = (firedAt - due) / 1e6;
        }

        return late;
    }

    private static IEnumerable<string> GoLateness(string go)
    {
        string module = Path.Combine(Path.GetTempPath(), "g2cs-timerprobe-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(module);

        try
        {
            File.WriteAllText(Path.Combine(module, "go.mod"), "module timerprobe\n\ngo 1.24\n");
            File.WriteAllText(Path.Combine(module, "main.go"), GoProgram.Replace("\r", ""));

            ProcessStartInfo start = new(go, ["run", "."])
            {
                WorkingDirectory = module,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            using Process process = Process.Start(start)!;
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (!process.WaitForExit(600_000))
            {
                process.Kill(entireProcessTree: true);
                return ["Go 5 ms: `go run` did not finish in 600 s"];
            }

            if (process.ExitCode != 0)
                return [$"Go 5 ms: `go run` exited {process.ExitCode}: {error.Trim()}"];

            return output.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => "Go " + line);
        }
        finally
        {
            try
            {
                Directory.Delete(module, true);
            }
            catch (Exception)
            {
            }
        }
    }

    private static string Line(string name, double[] late)
    {
        double[] sorted = late.OrderBy(value => value).ToArray();
        return $"{name} min {sorted[0]:F3} median {sorted[sorted.Length / 2]:F3} max {sorted[^1]:F3}";
    }
}
