// M:N PROTOTYPE (A3): the hand-owned side of the context fixture. This file has no `.go` counterpart.
//
// It holds what Go has no word for: the AsyncLocal the goroutines write and read, the counters of wrong reads, and
// the record of which thread ran which goroutine. Nothing here can park, so nothing here gets a twin.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using go.golib;

namespace go;

partial class main_package {

private static readonly string[] s_armNames =
    ["follow", "callee write", "bleed", "inherit", "injected", "hand-off", "sleep", "labels", "main"];

private static readonly AsyncLocal<nint> s_tag = new();
private static readonly long[] s_checks = new long[s_armNames.Length];
private static readonly long[] s_wrong = new long[s_armNames.Length];
private static readonly long[] s_firstWrong = new long[s_armNames.Length * 2];
private static readonly ConcurrentDictionary<(int thread, object goroutine), byte> s_pairs = new();
private static readonly ConcurrentDictionary<(int thread, nint arm), byte> s_armThreads = new();
private static readonly string s_arms = Environment.GetEnvironmentVariable("MN_CTX_ARMS") ?? "";
private static readonly Stopwatch s_clock = new();
private static int s_stop;

// Slot 0 is this fixture's AsyncLocal; slot 1 is golib's profile labels, with zero standing for "never labelled".
internal static void setTag(nint slot, nint v) {
    if (slot == 0) {
        s_tag.Value = v;
    } else {
        Goroutine.SetProfileLabels(v == 0 ? null : (object)v);
    }
}

internal static nint getTag(nint slot) {
    if (slot == 0) {
        return s_tag.Value;
    }

    return Goroutine.GetProfileLabels() is nint v ? v : 0;
}

// The arm's own slot: the labels arm reads golib's profile labels, every other arm the fixture's AsyncLocal.
private static nint slotOf(nint arm) => arm == armLabels ? slotLabels : slotLocal;

internal static void tag(nint arm, nint v) => setTag(slotOf(arm), v);

internal static void read(nint arm, nint want) => check(arm, slotOf(arm), want);

// One read of the calling goroutine's value against what it must be, counted under its arm.
internal static void check(nint arm, nint slot, nint want) {
    nint got = getTag(slot);
    int thread = Environment.CurrentManagedThreadId;

    Interlocked.Increment(ref s_checks[arm]);

    if (got != want && Interlocked.Increment(ref s_wrong[arm]) == 1) {
        Volatile.Write(ref s_firstWrong[arm * 2], want);
        Volatile.Write(ref s_firstWrong[arm * 2 + 1], got);
    }

    s_pairs.TryAdd((thread, (object?)Goroutine.Current ?? Thread.CurrentThread), 0);
    s_armThreads.TryAdd((thread, arm), 0);
}

internal static bool stopped() => Volatile.Read(ref s_stop) != 0;

internal static void stop() => Volatile.Write(ref s_stop, 1);

// MN_CTX_ARMS names the holder arms to run ("injected", "hand-off", "sleep", comma separated); unset runs all.
internal static bool enabled(nint arm) =>
    s_arms.Length == 0 || Array.IndexOf(s_arms.Split(','), s_armNames[arm]) >= 0;

// Holds the calling thread, running, for the given time: the goroutine does not park and does not yield.
internal static void spin(nint milliseconds) {
    long until = Stopwatch.GetTimestamp() + milliseconds * Stopwatch.Frequency / 1000;

    while (Stopwatch.GetTimestamp() < until) {
        Thread.SpinWait(64);
    }
}

// Blocks the calling thread inside a park scope, the way a blocking call in code with no twin does.
internal static void block(nint milliseconds) {
    using (Goroutine.Park(WaitReason.IOWait)) {
        Thread.Sleep((int)milliseconds);
    }
}

internal static bool costMode() => Environment.GetEnvironmentVariable("MN_CTX_MODE") == "cost";

internal static bool costTagged() => Environment.GetEnvironmentVariable("MN_CTX_TAGGED") == "1";

internal static nint costN() =>
    int.TryParse(Environment.GetEnvironmentVariable("MN_CTX_N"), out int n) && n > 0 ? n : 200000;

internal static void begin() => s_clock.Restart();

internal static void end(nint rep, nint n) {
    double nanoseconds = s_clock.Elapsed.TotalNanoseconds;

    Console.Error.WriteLine(
        $"COST mode=ping tagged={(costTagged() ? 1 : 0)} rep={rep} n={n} ns-per-trip={nanoseconds / n:F1}");
}

// Standard output: the wrong reads of every arm, the same text in every lane. Standard error: the proof that the
// arms ran where the rule could fail (how many reads, on how many threads, shared by how many goroutines).
internal static void report() {
    long total = 0;

    for (int arm = 0; arm < s_armNames.Length; arm++) {
        long wrong = Interlocked.Read(ref s_wrong[arm]);
        total += wrong;

        if (!costMode()) {
            Console.Out.WriteLine($"{s_armNames[arm]} wrong: {wrong}");
        }
    }

    Console.Out.WriteLine($"total wrong: {total}");
    Console.Out.Flush();

    Dictionary<int, int> goroutinesOfThread = [];
    Dictionary<object, int> threadsOfGoroutine = new(ReferenceEqualityComparer.Instance);

    foreach ((int thread, object goroutine) in s_pairs.Keys) {
        goroutinesOfThread[thread] = goroutinesOfThread.GetValueOrDefault(thread) + 1;
        threadsOfGoroutine[goroutine] = threadsOfGoroutine.GetValueOrDefault(goroutine) + 1;
    }

    int shared = 0;
    int migrated = 0;

    foreach (int count in goroutinesOfThread.Values) {
        if (count > 1) {
            shared++;
        }
    }

    foreach (int count in threadsOfGoroutine.Values) {
        if (count > 1) {
            migrated++;
        }
    }

    Console.Error.WriteLine(
        $"PROOF threads={goroutinesOfThread.Count} shared-threads={shared} goroutines={threadsOfGoroutine.Count}" +
        $" migrated-goroutines={migrated}");

    for (int arm = 0; arm < s_armNames.Length; arm++) {
        int threads = 0;

        foreach ((int _, nint of) in s_armThreads.Keys) {
            if (of == arm) {
                threads++;
            }
        }

        long want = Volatile.Read(ref s_firstWrong[arm * 2]);
        long got = Volatile.Read(ref s_firstWrong[arm * 2 + 1]);
        string first = Interlocked.Read(ref s_wrong[arm]) == 0 ? "" : $" first-wrong=want:{want},got:{got}";
        string name = s_armNames[arm].Replace(' ', '-');

        Console.Error.WriteLine(
            $"PROOF arm={name} checks={Interlocked.Read(ref s_checks[arm])} threads={threads}{first}");
    }
}

} // end main_package
