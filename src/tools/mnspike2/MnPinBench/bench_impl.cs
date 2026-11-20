// M:N PROTOTYPE, THE PIN'S ARMS: the hand-owned side of the bench. This file has no `.go` counterpart.
//
// Clocks, the plain threads that drive wakes and end timed runs (outside the scheduler they measure), the CPU spin,
// and the MNBENCH lines on standard error.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using go.golib;

namespace go;

partial class main_package {

private static long s_clock;
private static long s_trips;
private static int s_stop;
private static readonly List<long> s_latencies = new(4096);
private static long s_firstSend;

private static string Mode => Environment.GetEnvironmentVariable("MN_BENCH") ?? "pp";

internal static @string benchMode() => Mode;

internal static nint param(@string name, nint fallback) =>
    nint.TryParse(Environment.GetEnvironmentVariable(name.ToString()), out nint value) ? value : fallback;

internal static void startClock() => s_clock = Stopwatch.GetTimestamp();

internal static void stopClock(nint n, @string label) {
    long ticks = Stopwatch.GetTimestamp() - s_clock;
    double ns = ticks * 1e9 / Stopwatch.Frequency / n;
    Console.Error.WriteLine($"MNBENCH mode={Mode} {label}={ns:F1} n={n}");
}

internal static bool stopped() => Volatile.Read(ref s_stop) != 0;

internal static void stop() => Volatile.Write(ref s_stop, 1);

internal static void countTrips(nint n) => Interlocked.Add(ref s_trips, n);

internal static void reportTrips(nint durationMs) =>
    Console.Error.WriteLine($"MNBENCH mode={Mode} trips={Interlocked.Read(ref s_trips)} trips-per-s={Interlocked.Read(ref s_trips) * 1000.0 / durationMs:F0}");

// A plain thread ends the timed run, so no carrier waits on the clock.
internal static void runFor(nint ms) =>
    new Thread(() => { Thread.Sleep((int)ms); stop(); }) { IsBackground = true, Name = "mnbench-timer" }.Start();

internal static void spin(nint ms) {
    long until = Stopwatch.GetTimestamp() + ms * Stopwatch.Frequency / 1000;

    while (Stopwatch.GetTimestamp() < until) {
        Thread.SpinWait(32);
    }
}

// A plain thread sends its send time every intervalMs for durationMs, then -1. It returns at once: the caller parks.
internal static void driveWakes(channel<long> wakes, nint intervalMs, nint durationMs) {
    new Thread(() => {
        long end = Stopwatch.GetTimestamp() + durationMs * Stopwatch.Frequency / 1000;
        long next = Stopwatch.GetTimestamp();
        s_firstSend = next;

        while (Stopwatch.GetTimestamp() < end) {
            next += intervalMs * Stopwatch.Frequency / 1000;

            while (Stopwatch.GetTimestamp() < next) {
                long left = (next - Stopwatch.GetTimestamp()) * 1000 / Stopwatch.Frequency;

                if (left > 2)
                    Thread.Sleep((int)(left - 1));
                else
                    Thread.SpinWait(64);
            }

            wakes.ᐸꟷ(Stopwatch.GetTimestamp());
        }

        wakes.ᐸꟷ(-1L);
    }) { IsBackground = true, Name = "mnbench-waker" }.Start();
}

internal static void noteWake(long sent) {
    long latency = Stopwatch.GetTimestamp() - sent;

    lock (s_latencies)
        s_latencies.Add(latency);
}

internal static void reportWakes() {
    long[] all;

    lock (s_latencies)
        all = s_latencies.ToArray();

    Array.Sort(all);
    double Ms(long ticks) => ticks * 1000.0 / Stopwatch.Frequency;
    double At(double q) => all.Length == 0 ? double.NaN : Ms(all[Math.Min(all.Length - 1, (int)(q * all.Length))]);

    Console.Error.WriteLine($"MNBENCH mode={Mode} wakes={all.Length} p50-ms={At(0.50):F3} p99-ms={At(0.99):F3} max-ms={At(1.0):F3}");
}

internal static void reportScheduler() {
    using Process self = Process.GetCurrentProcess();
    Console.Error.WriteLine($"MNBENCH mode={Mode} threads-now={self.Threads.Count}");
}

} // end main_package
