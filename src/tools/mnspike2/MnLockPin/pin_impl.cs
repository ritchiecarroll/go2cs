// M:N PROTOTYPE, THE PIN: the hand-owned side of the pin fixture. This file has no `.go` counterpart.
//
// It holds what Go has no word for: which thread a locked goroutine was pinned to, the counts of thread changes and
// of goroutines seen on a pinned thread, a foreign await (a .NET Task, neither a twin nor a park slot), and an
// awaiter whose wake comes before its park commits.

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using go.golib;

namespace go;

partial class main_package {

private static readonly string[] s_shapeNames =
    ["chan", "select", "nested", "foreign", "sleep", "mutex", "wait", "nest", "exit", "unpin", "scenA", "scenB", "gosched"];

private static readonly Thread?[] s_pinned = new Thread?[s_shapeNames.Length];
private static readonly long[] s_changes = new long[s_shapeNames.Length];
private static readonly long[] s_checks = new long[s_shapeNames.Length];
private static readonly ConcurrentDictionary<Thread, nint> s_owners = new();
private static long s_foreign;
private static long s_foreignChecks;
private static Thread? s_exitThread;
private static int s_stop;

internal static nint roundsOf(nint fallback) =>
    nint.TryParse(Environment.GetEnvironmentVariable("MN_PIN_ROUNDS_SCALE"), out nint scale) && scale > 0 ? fallback * scale / 100 : fallback;

// lockMe: runtime.LockOSThread, then the thread this goroutine is on now is the one it must stay on.
internal static void lockMe(nint shape) {
    runtime_package.LockOSThread();
    Thread thread = Thread.CurrentThread;
    s_pinned[shape] = thread;
    s_owners[thread] = shape;
}

internal static void unlockMe(nint shape) {
    if (s_pinned[shape] is { } thread)
        s_owners.TryRemove(thread, out _);

    runtime_package.UnlockOSThread();
}

internal static void checkPinned(nint shape) {
    Interlocked.Increment(ref s_checks[shape]);

    if (!ReferenceEquals(Thread.CurrentThread, s_pinned[shape]))
        Interlocked.Increment(ref s_changes[shape]);
}

// An unlocked goroutine is never on a thread that a locked one holds.
internal static void checkForeign() {
    Interlocked.Increment(ref s_foreignChecks);

    if (s_owners.ContainsKey(Thread.CurrentThread))
        Interlocked.Increment(ref s_foreign);
}

// The per-arm watchdog: an arm that has not finished by its deadline is named and the program exits 3, so a hang reds
// by NAME (a hang that names nothing is a weak red, and it is the shape a future regression would take). The watchdog
// is a plain thread, outside the scheduler it watches.
private static readonly long[] s_armStart = new long[s_shapeNames.Length];
private static readonly int[] s_armDone = new int[s_shapeNames.Length];
private static int s_watching;

private static readonly long s_deadlineTicks =
    (long.TryParse(Environment.GetEnvironmentVariable("MN_PIN_ARM_DEADLINE_MS"), out long ms) && ms > 0 ? ms : 20000) *
    (System.Diagnostics.Stopwatch.Frequency / 1000);

internal static void armStart(nint shape) {
    Volatile.Write(ref s_armStart[shape], System.Diagnostics.Stopwatch.GetTimestamp());

    if (Interlocked.Exchange(ref s_watching, 1) == 0)
        new Thread(Watch) { IsBackground = true, Name = "mnpin-watchdog" }.Start();
}

internal static void armDone(nint shape) => Volatile.Write(ref s_armDone[shape], 1);

private static void Watch() {
    while (true) {
        Thread.Sleep(250);
        long now = System.Diagnostics.Stopwatch.GetTimestamp();

        for (int i = 0; i < s_shapeNames.Length; i++) {
            long start = Volatile.Read(ref s_armStart[i]);

            if (start != 0 && Volatile.Read(ref s_armDone[i]) == 0 && now - start > s_deadlineTicks) {
                string line = $"MNPIN-WATCHDOG arm={s_shapeNames[i]} did not finish within {s_deadlineTicks / (System.Diagnostics.Stopwatch.Frequency / 1000)} ms";
                Console.Out.WriteLine(line);
                Console.Error.WriteLine(line);
                Console.Out.Flush();
                Environment.Exit(3);
            }
        }
    }
}

// The foreign check's positive control: a thread registered as pinned must count. Run once before any goroutine
// starts, and taken back, so it leaves the count at zero.
internal static void controlForeign() {
    s_owners[Thread.CurrentThread] = -1;
    checkForeign();
    bool fired = Interlocked.Read(ref s_foreign) == 1;
    s_owners.TryRemove(Thread.CurrentThread, out _);
    Interlocked.Exchange(ref s_foreign, 0);
    Interlocked.Exchange(ref s_foreignChecks, 0);
    Console.Error.WriteLine($"MNPIN-CONTROL foreign-check-fires={fired}");
}

internal static void noteExitThread() => s_exitThread = Thread.CurrentThread;

internal static bool stopped() => Volatile.Read(ref s_stop) != 0;

internal static void stop() => Volatile.Write(ref s_stop, 1);

// hop: a foreign await. The plain body blocks on a .NET task; its hand-written twin awaits the task, which completes
// on the thread pool, so its continuation would run off the carrier unless golib routes it (DESIGN-mn-pin.md 10.2).
internal static void hop() => Task.Run(static () => { }).Wait();

internal static GoTask hopꞏtwin() => Hop();

private static async GoTask Hop() => await Task.Run(static () => { });

// prewoke: a park whose slot is woken BEFORE the park commits, which sends the wake through GoScheduler.Ready at the
// commit itself. After an unlock, that wake must find the pin already cleared (DESIGN-mn-pin.md 10.3).
internal static void prewoke() { }

internal static GoTask prewokeꞏtwin() => Prewoke();

private static async GoTask Prewoke() => await new PreWoken();

private readonly struct PreWoken : ICriticalNotifyCompletion {
    public PreWoken GetAwaiter() => this;

    public bool IsCompleted => false;

    public void GetResult() { }

    public void OnCompleted(Action continuation) => UnsafeOnCompleted(continuation);

    public void UnsafeOnCompleted(Action continuation) {
        GoParkSlot slot = new();
        slot.Wake();
        slot.GetAwaiter().UnsafeOnCompleted(continuation);
    }
}

internal static void report() {
    for (int i = 0; i < s_shapeNames.Length; i++) {
        if (i == shapeExit || i == shapeScenA || i == shapeScenB)
            continue;

        fmt_package.Println("shape", s_shapeNames[i], "thread-changes:", s_changes[i]);
        Console.Error.WriteLine($"MNPIN-CHECKS shape={s_shapeNames[i]} checks={s_checks[i]} changes={s_changes[i]}");
    }

    fmt_package.Println("goroutines on a pinned thread:", s_foreign);
    fmt_package.Println("ended-locked thread gone:", s_exitThread is { IsAlive: false });
    fmt_package.Println("unpin arm finished: true");
    Console.Error.WriteLine($"MNPIN-FOREIGN checks={s_foreignChecks} foreign={s_foreign} exit-thread-alive={s_exitThread?.IsAlive}");
}

} // end main_package

partial class main_package {

// DESIGN-mn-pin.md 15: the scenario this run is (MN_PIN_SCENARIO=A or B; empty runs the identity arms), the P the
// scheduler reads (GO2CS_MN_PROCS, else the processor count), and a CPU spin outside any park.
internal static @string scenario() => Environment.GetEnvironmentVariable("MN_PIN_SCENARIO") ?? "";

internal static nint procs() =>
    nint.TryParse(Environment.GetEnvironmentVariable("GO2CS_MN_PROCS"), out nint p) && p > 0 ? p : Environment.ProcessorCount;

internal static void spinMs(nint ms) {
    long until = System.Diagnostics.Stopwatch.GetTimestamp() + ms * System.Diagnostics.Stopwatch.Frequency / 1000;

    while (System.Diagnostics.Stopwatch.GetTimestamp() < until)
        Thread.SpinWait(32);
}

} // end main_package (scenarios)
