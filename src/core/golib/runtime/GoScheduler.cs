//******************************************************************************************************
//  GoScheduler.cs - M:N scheduler, experimental and opt-in (GoTwins=true + GO2CS_MN=1); inert otherwise.
//
//  A goroutine started with a hidden async twin (Goroutine.StartTwin) runs here, multiplexed over a
//  small set of carrier threads, when GO2CS_MN=1. With the switch off nothing in this file runs.
//  The design is docs/phase4/DESIGN-mn-scheduler.md (O6 + O7); the measurements are in
//  docs/phase4/DATA-mn-spike-2.md.
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace go.golib;

/// <summary>
/// Where a goroutine twin waits. One-shot: <see cref="Wake"/> makes the parked goroutine runnable, and a
/// wake that arrives before the park is committed is remembered, so no wakeup is lost.
/// </summary>
/// <remarks>
/// The continuation is published by the CARRIER, after it has unmounted the goroutine
/// (<see cref="Commit"/>), never by the awaiter. A waker therefore cannot make the goroutine runnable
/// while it is still mounted on the carrier it parked on.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class GoParkSlot
{
    private static readonly object s_woken = new();

    // null (not woken, not parked), s_woken, or the parked continuation: an Action, or the parked twin's box (row 6).
    private object? m_state;

    // A Gosched yield park (GoScheduler.GoschedTwin): the carrier requeues the goroutine at the global queue's tail instead
    // of committing the park. Carried on the SLOT, never on the carrier, so a park attempt that fails (ParkPending throws)
    // cannot turn the step's earlier, real park into a requeue (P3's read of the Gosched seat).
    internal bool IsYield { get; init; }
    private Goroutine? m_goroutine;

    /// <summary>Whether <see cref="Wake"/> has run.</summary>
    public bool IsWoken => ReferenceEquals(Volatile.Read(ref m_state), s_woken);

    /// <summary>Makes the goroutine parked here runnable, now or as soon as its park is committed.</summary>
    public void Wake()
    {
        if (Interlocked.Exchange(ref m_state, s_woken) is { } continuation && !ReferenceEquals(continuation, s_woken))
            GoScheduler.Ready(m_goroutine!, continuation);
    }

    // Called by the carrier once the goroutine is unmounted.
    internal void Commit(Goroutine goroutine, object continuation)
    {
        m_goroutine = goroutine;

        if (Interlocked.CompareExchange(ref m_state, continuation, null) is not null)
            GoScheduler.Ready(goroutine, continuation);
    }

    /// <summary>Awaits the wake.</summary>
    public GoParkAwaiter GetAwaiter() => new(this);
}

/// <summary>The awaiter of a <see cref="GoParkSlot"/>.</summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly struct GoParkAwaiter : ICriticalNotifyCompletion
{
    private readonly GoParkSlot m_slot;

    internal GoParkAwaiter(GoParkSlot slot) => m_slot = slot;

    // The slot, for a twin's builder to park on it with the twin's box instead of a delegate (row 6).
    internal GoParkSlot Slot => m_slot;

    /// <summary>Whether the slot was woken before the await: the twin then runs on without parking.</summary>
    public bool IsCompleted => m_slot.IsWoken;

    /// <summary>Nothing to return.</summary>
    public void GetResult() { }

    /// <inheritdoc/>
    public void OnCompleted(Action continuation) => GoScheduler.ParkPending(m_slot, continuation);

    /// <inheritdoc/>
    public void UnsafeOnCompleted(Action continuation) => GoScheduler.ParkPending(m_slot, continuation);
}

/// <summary>
/// The spike's M:N scheduler: a global run queue of goroutine steps, P tokens (one per processor), carrier
/// threads that take a P to run one step, hand-off of the P when a mounted goroutine blocks its thread,
/// and a monitor thread that injects a P when a step runs long.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class GoScheduler
{
    /// <summary>GO2CS_MN=1: goroutines started with a twin are multiplexed.</summary>
    public static readonly bool Enabled = Environment.GetEnvironmentVariable("GO2CS_MN") == "1";

    private static readonly bool s_reading = Environment.GetEnvironmentVariable("GO2CS_MN_READING") == "1";

    // The P count: how many goroutine steps run at once. It follows GOMAXPROCS (SetProcs, below).
    private static int s_procs = ReadProcs();

    // A carrier runs inline continuations of any depth the twin chain reaches, so it gets a goroutine-sized
    // stack rather than the prototype's 256 KB.
    private const int CarrierStackSize = 1024 * 1024;

    // GO2CS_MN_PREEMPT_MS: how long a step may run before the monitor injects a P beside it (default 10 ms,
    // Go's forcePreemptNS). 0 turns injection off: a measurement arm, not a mode.
    private static readonly long s_preemptTicks = ReadPreemptTicks();

    // GO2CS_MN_MAXINJECT: how many injected Ps may be live at once (default: the P count, so a storm of long
    // steps at most doubles the running carriers). Injection adds parallelism where Go would preempt, and
    // each added carrier past the core count makes the next step likelier to be descheduled and read as long.
    private static readonly int s_maxInject = ReadMaxInject();

    // GO2CS_MN_RUNNEXT=0 turns the run-next slot off (a measurement arm). With it on, a goroutine made
    // runnable by a running step goes to that carrier's slot and runs next on the same thread, as Go's
    // runnext does: the newest wins, an older one moves to the global queue, and the monitor steals the
    // slot of a step that runs past a millisecond. A chain of slot runs is bounded so the global queue is served.
    private static readonly bool s_runNext = Environment.GetEnvironmentVariable("GO2CS_MN_RUNNEXT") != "0";
    private const int MaxRunNextChain = 128;
    private static long s_runNextSteps;
    private static long s_runNextSteals;

    // GO2CS_MN_RETAKE=0 turns the native-call retake off (a measurement arm). With it on, the monitor does
    // Go's sysmon retake: a goroutine it finds inside a native call (the native-call seam's depth is odd) on
    // two ticks in a row, while work waits, has its P taken back, outside the injection budget. The seam
    // itself is not changed: the carrier settles the account when the step ends or parks.
    private static readonly bool s_retake = Environment.GetEnvironmentVariable("GO2CS_MN_RETAKE") != "0";

    // The pin's made-to-fail switches (DESIGN-mn-pin.md 10.1, 10.5, 11.1), each the line as it was before the pin:
    // GO2CS_MN_INJECT_WAITERS=0 injects for the run queue only, GO2CS_MN_RETIRE_RAW=1 retires on the raw carrier count,
    // GO2CS_MN_ROOM_RAW=1 reads the injection room on the raw count. Arms only; never set in a build anyone ships.
    private static readonly bool s_injectWaiters = Environment.GetEnvironmentVariable("GO2CS_MN_INJECT_WAITERS") != "0";
    private static readonly bool s_retireRaw = Environment.GetEnvironmentVariable("GO2CS_MN_RETIRE_RAW") == "1";
    private static readonly bool s_roomRaw = Environment.GetEnvironmentVariable("GO2CS_MN_ROOM_RAW") == "1";

    // DESIGN-mn-pin.md 15.3 (arms only): GO2CS_MN_RETAKE_UNPINNED=1 counts a retake as shared, as before 15.2.
    private static readonly bool s_retakeUnpinned = Environment.GetEnvironmentVariable("GO2CS_MN_RETAKE_UNPINNED") == "1";
    private static long s_retakes;
    private static int s_pWaiters;

    private static int ReadMaxInject()
    {
        string? text = Environment.GetEnvironmentVariable("GO2CS_MN_MAXINJECT");
        return int.TryParse(text, out int value) && value >= 0 ? value : s_procs;
    }

    private static long ReadPreemptTicks()
    {
        string? text = Environment.GetEnvironmentVariable("GO2CS_MN_PREEMPT_MS");
        int ms = int.TryParse(text, out int value) && value >= 0 ? value : 10;
        return ms * (Stopwatch.Frequency / 1000);
    }

    // A step is an Action, or the box of a twin that parked (row 6 of the .5 plan): a park carries the box itself,
    // so a leaf park allocates no delegate. RunStep resumes a box as GoTaskCore.ResumeAsStep would.
    private readonly struct Item(Goroutine goroutine, object step)
    {
        internal readonly Goroutine Goroutine = goroutine;
        internal readonly object Step = step;
    }

    private static readonly ConcurrentQueue<Item> s_runq = new();
    // THE CURE (DESIGN-mn-pin.md 14): the idle carriers, each waiting on its own semaphore, and how many there are
    // (s_idle, always the set's size). A pinned carrier never enters it (12.1). Replaces the shared work semaphore.
    private static readonly object s_idleLock = new();
    private static readonly List<Carrier> s_idleList = [];
    private static long s_idleHandoffs;

    // DESIGN-mn-pin.md 12.1, as the cure keeps it (a reading, expected 0): no carrier in the idle set is pinned, and the
    // set's size is s_idle. Checked by the monitor's reading tick only.
    private static long s_idleSetViolations;

    private static void CheckIdleSet()
    {
        lock (s_idleLock)
        {
            if (s_idle != s_idleList.Count)
                Interlocked.Increment(ref s_idleSetViolations);

            foreach (Carrier carrier in s_idleList)
            {
                if (carrier.IsPinned || !carrier.m_idle)
                    Interlocked.Increment(ref s_idleSetViolations);
            }
        }

        // DESIGN-mn-pin.md 15 (P3's SHOULD 3): the shared-pool floor, a standing reading: at least P shared carriers.
        lock (s_carriersLock)
        {
            if (s_carriers.Count - s_pinnedCount < s_procs)
                Interlocked.Increment(ref s_sharedFloorViolations);

            // 15.3 (P3's NIT): the pinned-blocked count stays within 0 and the pinned count, a standing reading.
            int pinnedBlocked = Volatile.Read(ref s_pinnedBlocked);

            if (pinnedBlocked < 0 || pinnedBlocked > s_pinnedCount)
                Interlocked.Increment(ref s_pinnedBlockedViolations);
        }
    }

    // Takes one carrier out of the idle set: `prefer` when it is idle, else the one that went idle last. The caller
    // must release the carrier's m_idleWake; the carrier consumes that permit even if its wait has timed out.
    private static Carrier? ClaimIdle(Carrier? prefer)
    {
        lock (s_idleLock)
        {
            Carrier? claimed = null;

            if (prefer is { m_idle: true })
            {
                s_idleList.Remove(prefer);
                claimed = prefer;
            }
            else if (s_idleList.Count > 0)
            {
                claimed = s_idleList[^1];
                s_idleList.RemoveAt(s_idleList.Count - 1);
            }

            if (claimed is not null)
            {
                claimed.m_idle = false;
                s_idle = s_idleList.Count;
            }

            return claimed;
        }
    }
    private static readonly PTokens s_p = new(s_procs);   // the P tokens (lock-free hand-back: GoScheduler.PTokens.cs)
    private static readonly List<Carrier> s_carriers = [];
    private static readonly object s_carriersLock = new();

    private static int s_started;
    private static int s_idle;
    private static int s_overBudget;
    private static int s_blocked;

    // DESIGN-mn-pin.md 15 (P3's MUST on the pin build): carriers pinned to a goroutine, running or waiting, kept under
    // s_carriersLock at pin, unpin and retire. A pinned carrier serves one goroutine, so it is never shared capacity.
    private static int s_pinnedCount;

    // DESIGN-mn-pin.md 15.2 (P3's SHOULD on the floor fix): the pinned carriers counted in s_blocked, waiting pinned, in a
    // plain park (EnterBlocked) or retaken in a native call; raised and lowered with s_blocked through each carrier's flag.
    private static int s_pinnedBlocked;
    private static long s_sharedFloorViolations;
    private static long s_pinnedBlockedViolations;

    // The shared carriers that are not blocked: what the idle retire, EnsureCarrier and the injection read (under
    // s_carriersLock). s_blocked holds blocked pinned carriers (s_pinnedBlocked) as well as the shared ones.
    private static int SharedUnblocked() =>
        s_carriers.Count - s_pinnedCount - (Volatile.Read(ref s_blocked) - Volatile.Read(ref s_pinnedBlocked));

    // Keeps at least P shared carriers (under s_carriersLock; AddCarrier takes it again, reentrantly), so Enqueue's
    // assumption that a shared carrier exists always holds.
    private static void RefillShared()
    {
        while (s_carriers.Count - s_pinnedCount < s_procs)
        {
            int before = s_carriers.Count;
            AddCarrier();

            if (s_carriers.Count == before)
                break;
        }
    }

    private static int s_peakCarriers;
    private static int s_peakThreads;
    private static long s_steps;
    private static long s_parks;
    private static long s_goschedYields;
    private static long s_migrations;
    private static long s_migrated;
    private static long s_handoffs;

    // The other half of the hand-off counter (COORD's split for byref-param, 2026-11-16): an outermost PLAIN park of a
    // goroutine with no carrier to hand off (a thread goroutine, or one off its carrier), so it blocks its own thread.
    // `handoffs` alone rose under N2 as more goroutines came onto carriers; the two together show where blocks went.
    private static long s_threadBlocks;
    private static long s_injections;

    // The pin's readings (DESIGN-mn-pin.md): carriers that pinned a goroutine, wakes delivered to a pinned carrier,
    // pinned carriers ended with their goroutine, the most waiting at once, and LockOSThread calls off any carrier.
    private static long s_pins;
    private static long s_pinnedWakes;
    private static long s_pinRetired;
    private static int s_pinnedWaiting;
    private static int s_peakPinnedWaiting;
    private static long s_offCarrierLocks;

    internal static void NoteOffCarrierLock() => Interlocked.Increment(ref s_offCarrierLocks);

    private static long s_carrierStartFailures;
    private static long s_carriersStarted;
    private static long s_longSteps;
    private static long s_maxStepTicks;

    [ThreadStatic] private static Carrier? t_carrier;

    // The count until the runtime hands its GOMAXPROCS over (SetProcs), which it does at start, before main. GO2CS_MN_PROCS,
    // a measurement knob until it retires after .5, pins the count: the setting is then not followed.
    // ReadProcs runs in s_procs' initializer, ahead of every field declared below it, so it reads the variable itself.
    private static int ReadProcs() => PinnedProcs() is > 0 and int pin ? pin : Environment.ProcessorCount;

    private static int PinnedProcs() => int.TryParse(Environment.GetEnvironmentVariable("GO2CS_MN_PROCS"), out int pin) ? pin : 0;

    private static readonly bool s_procsPinned = PinnedProcs() > 0;
    private static readonly object s_procsLock = new();

    /// <summary>The P count: how many goroutine steps the scheduler runs at once.</summary>
    internal static int Procs => Volatile.Read(ref s_procs);

    /// <summary>
    /// The P count follows GOMAXPROCS, as Go's procresize does (COORD's ruling for .5, C1/20261122T141803Z): runtime's one
    /// writer of the setting hands every new value here, the GOMAXPROCS variable at start and runtime.GOMAXPROCS(n) live.
    /// With the scheduler off the count is only kept, so a later start uses it. A P added is a token released and, once
    /// started, a shared carrier; a P removed takes a token out of circulation, now if one is free and otherwise once a
    /// running step releases one, and the monitor retires the carrier it leaves idle.
    /// </summary>
    internal static void SetProcs(int procs)
    {
        if (procs < 1 || s_procsPinned)
            return;

        lock (s_procsLock)
        {
            int previous = s_procs;

            if (procs == previous)
                return;

            Volatile.Write(ref s_procs, procs);

            if (procs > previous)
            {
                s_p.Release(procs - previous);
            }
            else
            {
                int owed = previous - procs;

                while (owed > 0 && s_p.Wait(0))
                    owed--;

                // Tokens held by running steps leave as those steps release them. The wait is a plain one, beside the
                // carriers': SemaphoreSlim serves a pending async wait only once no plain waiter wants the token, so
                // under load an async wait would never take one and the P would never leave.
                if (owed > 0)
                {
                    Thread retire = new(() =>
                    {
                        for (int i = 0; i < owed; i++)
                            s_p.Wait();
                    })
                    { IsBackground = true, Name = "go-procresize" };

                    retire.Start();
                }
            }
        }

        if (Volatile.Read(ref s_started) != 0)
        {
            lock (s_carriersLock)
                RefillShared();
        }
    }

    /// <summary>The P tokens free now: the P count less the steps running, when no P is waiting to be removed.</summary>
    internal static int FreePs => s_p.CurrentCount;

    /// <summary>Whether the calling thread is a carrier with a goroutine mounted: where a twin may park.</summary>
    public static bool OnCarrier => t_carrier is { Mounted: not null };

    // M:N (A1). GO2CS_MN_CONTEXT=0 (a reading) turns the context rule off: a goroutine's
    // ExecutionContext is then not carried from step to step, and a carrier starts under its starter's.
    private static readonly bool s_contextSwap = Environment.GetEnvironmentVariable("GO2CS_MN_CONTEXT") != "0";

    /// <summary>Whether a goroutine's ExecutionContext follows it from carrier to carrier.</summary>
    internal static bool ContextFollowsGoroutine => s_contextSwap;

    // Readings: a twin's step that began on a thread with no goroutine mounted, and a twin's caller resumed
    // inside the frame that completed it (both zero unless something other than a twin or a park was awaited).
    private static long s_offCarrierResumes;
    private static long s_nestedResumes;

    // A twin's step is starting from its ResumeAction (a foreign awaiter) or from a parked box (RunStep).
    internal static void NoteStep(GoTaskCore twin)
    {
        if (t_carrier is { Mounted: not null } carrier)
            carrier.Resuming = twin;
        else
            Interlocked.Increment(ref s_offCarrierResumes);
    }

    // M:N (A2). The twin the calling carrier's step resumed, or null: no goroutine is mounted, or
    // the step is a goroutine's first, whose calls are all frames on this thread. The resumed twin is the
    // lowest frame of the goroutine that the thread holds; its callers are the suspended twins up its
    // parent link (GoTaskCore.Parent), which a stack walk reads after the thread's own frames.
    internal static GoTaskCore? ResumedTwin => t_carrier is { Mounted: not null } carrier ? carrier.Resuming : null;

    // A twin ended, and `continuation` awaits it: its caller's box, as a rule. The caller does not resume
    // inside the frame that ended. The carrier runs it when that frame has returned (RunStep), so a return
    // through any number of suspended callers stays on this carrier and uses no stack.
    internal static void Continue(object continuation)
    {
        if (continuation is not GoTaskCore core)
        {
            ((Action)continuation)();
            return;
        }

        if (t_carrier is { Mounted: not null } carrier)
        {
            if (carrier.Continuation is null)
            {
                carrier.Continuation = core;
                return;
            }

            Interlocked.Increment(ref s_nestedResumes);
        }
        else
        {
            Interlocked.Increment(ref s_offCarrierResumes);
        }

        core.Resume();
    }

    /// <summary>Queues one step of <paramref name="goroutine"/>.</summary>
    internal static void Ready(Goroutine goroutine, object step)
    {
        if (Volatile.Read(ref s_started) == 0)
            Start();

        // The pin (DESIGN-mn-pin.md 3.3): a goroutine parked locked to its thread runs again only on its own carrier,
        // which takes a P from the pool; the waker runs on.
        if (goroutine.m_pin is Carrier pin)
        {
            pin.Deliver(step);
            return;
        }

        // A waker locked to its carrier does not use the run-next slot: that carrier serves one goroutine.
        if (s_runNext && t_carrier is { InStep: true, Chain: < MaxRunNextChain } carrier && carrier.Mounted is not { LockedToThread: true })
        {
            goroutine.m_readyStep = step;

            if (Interlocked.Exchange(ref carrier.Next, goroutine) is { } older)
                Enqueue(older, older.TakeReadyStep());

            return;
        }

        // THE CURE (DESIGN-mn-pin.md 14): a waker locked to its carrier hands the goroutine it readies straight to an idle
        // carrier (the one that ran it last, when idle), through that carrier's run-next slot and its own wake: no queue.
        if (t_carrier is { Mounted: { LockedToThread: true } } && HandToIdle(goroutine, step))
            return;

        Enqueue(goroutine, step);
    }

    // DESIGN-mn-pin.md 14.2 (COORD's arm (c)): the locked waker's hand-off, kept out of Ready's hot path as a cold static
    // helper, so the lock-taking call cannot change how Ready is compiled for the unlocked path.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool HandToIdle(Goroutine goroutine, object step)
    {
        if (ClaimIdle(goroutine.m_lastCarrier as Carrier) is not { } target)
            return false;

        goroutine.m_readyStep = step;

        if (Interlocked.Exchange(ref target.Next, goroutine) is { } stale)
            Enqueue(stale, stale.TakeReadyStep());

        Interlocked.Increment(ref s_idleHandoffs);
        target.m_idleWake.Release();
        return true;
    }

    private static void Enqueue(Goroutine goroutine, object step)
    {
        s_runq.Enqueue(new Item(goroutine, step));

        // One idle carrier is woken for it (the shared work semaphore this replaced held one permit per item).
        if (ClaimIdle(null) is { } idle)
            idle.m_idleWake.Release();

        // Work queued while carriers sit blocked under goroutines that had no twin: make sure one can take it.
        if (Volatile.Read(ref s_blocked) > 0)
            EnsureCarrier();
    }

    private static void Start()
    {
        if (Interlocked.Exchange(ref s_started, 1) != 0)
            return;

        for (int i = 0; i < s_procs; i++)
            AddCarrier();

        Thread monitor = new(Monitor) { IsBackground = true, Name = "go-sysmon" };
        monitor.Start();

        if (s_reading)
            AppDomain.CurrentDomain.ProcessExit += static (_, _) => Console.Error.WriteLine(Reading());
    }

    /// <summary>The scheduler's counters, one line.</summary>
    public static string Reading()
    {
        SampleThreads();

        return $"READING go-scheduler procs={s_procs} peak-carriers={Volatile.Read(ref s_peakCarriers)}" +
               $" carriers-started={Interlocked.Read(ref s_carriersStarted)} peak-os-threads={Volatile.Read(ref s_peakThreads)}" +
               $" steps={Interlocked.Read(ref s_steps)} parks={Interlocked.Read(ref s_parks)}" +
               $" migrations={Interlocked.Read(ref s_migrations)} migrated-goroutines={Interlocked.Read(ref s_migrated)}" +
               $" handoffs={Interlocked.Read(ref s_handoffs)} thread-blocks={Interlocked.Read(ref s_threadBlocks)}" +
               $" injections={Interlocked.Read(ref s_injections)}" +
               $" run-next-steps={Interlocked.Read(ref s_runNextSteps)} run-next-steals={Interlocked.Read(ref s_runNextSteals)}" +
               $" steps-over-10ms={Interlocked.Read(ref s_longSteps)}" +
               $" max-step-ms={Interlocked.Read(ref s_maxStepTicks) * 1000 / Stopwatch.Frequency}" +
               $" carrier-start-failures={Interlocked.Read(ref s_carrierStartFailures)}" +
               $" retakes={Interlocked.Read(ref s_retakes)}" +
               $" off-carrier-resumes={Interlocked.Read(ref s_offCarrierResumes)}" +
               $" nested-resumes={Interlocked.Read(ref s_nestedResumes)}" +
               $" pins={Interlocked.Read(ref s_pins)} pinned-wakes={Interlocked.Read(ref s_pinnedWakes)}" +
               $" pin-retired={Interlocked.Read(ref s_pinRetired)} peak-pinned-waiting={Volatile.Read(ref s_peakPinnedWaiting)}" +
               $" off-carrier-locks={Interlocked.Read(ref s_offCarrierLocks)} idle-handoffs={Interlocked.Read(ref s_idleHandoffs)} idle-set-violations={Interlocked.Read(ref s_idleSetViolations)} shared-floor-violations={Interlocked.Read(ref s_sharedFloorViolations)} pinned-blocked-violations={Interlocked.Read(ref s_pinnedBlockedViolations)}" +
               $" context={(s_contextSwap ? "goroutine" : "off")}" + Goroutine.MainReading() + Goroutine.MountReading() +
               $" gosched-yields={Interlocked.Read(ref s_goschedYields)}";
    }

    private static void SampleThreads()
    {
        try
        {
            using Process self = Process.GetCurrentProcess();
            int threads = self.Threads.Count;
            int peak;

            while (threads > (peak = Volatile.Read(ref s_peakThreads)))
            {
                if (Interlocked.CompareExchange(ref s_peakThreads, threads, peak) == peak)
                    break;
            }
        }
        catch (Exception)
        {
            // A reading, never a failure.
        }
    }

    // The Gosched-park seat (COORD's ruling of 2026-11-21, docs/phase4/inbox/G/20261121T231225Z-COORD.md): a twin's
    // runtime.Gosched is a park point that requeues the goroutine at the GLOBAL run queue's tail, as Go's goschedImpl
    // puts it on the global queue, so a goroutine that spins on Gosched ends its step and every goroutine queued
    // before it runs first (S2 arm 1, MnSpinCap: spinners beyond the P count starved parked goroutines while Gosched
    // was a thread yield). Off a carrier (the scheduler off, a plain thread) it is runtime.Gosched's own thread yield.
    internal static GoTask GoschedTwin()
    {
        if (t_carrier is not { Mounted: not null })
        {
            GoschedBackoff.Yield();
            return default;
        }

        return YieldPark();
    }

    // A park on a slot nobody wakes, marked a yield: the carrier requeues the goroutine itself (RunStep).
    private static async GoTask YieldPark() => await YieldSlot();

    // The slot a Gosched yield parks on (its own factory, so a row can read the mark the carrier reads).
    internal static GoParkSlot YieldSlot() => new() { IsYield = true };

    // What the carrier does with a committed park after the unmount (P3's read): a yield slot's goroutine is requeued, any
    // other slot is committed. The decision reads the PENDING slot alone, so nothing a failed park attempt left behind can
    // change it.
    internal static bool RequeuesOnCommit(GoParkSlot slot) => slot.IsYield;

    // Records the park of the goroutine mounted on this carrier. The carrier commits it after the unmount.
    internal static void ParkPending(GoParkSlot slot, object continuation)
    {
        Carrier? carrier = t_carrier;

        if (carrier?.Mounted is null)
            throw new InvalidOperationException("golib: a goroutine twin parked off its carrier");

        if (carrier.PendingSlot is not null)
            throw new InvalidOperationException("golib: a goroutine twin parked twice in one step");

        carrier.PendingSlot = slot;
        carrier.PendingContinuation = continuation;
    }

    /// <summary>A plain park's block with no carrier to hand off: counted only (the `thread-blocks` reading).</summary>
    internal static void NoteThreadBlock() => Interlocked.Increment(ref s_threadBlocks);

    /// <summary>
    /// The mounted goroutine is about to block its thread (a park with no twin): give the P away, and make
    /// sure a carrier exists to take it. Returns false off a carrier.
    /// </summary>
    internal static bool EnterBlocked()
    {
        Carrier? carrier = t_carrier;

        if (carrier?.Mounted is null)
            return false;

        Interlocked.Increment(ref s_handoffs);
        Volatile.Write(ref carrier.RunningSince, 0);

        // This thread is about to block: whatever waits in its slot must not wait with it.
        if (Interlocked.Exchange(ref carrier.Next, null) is { } next)
            Enqueue(next, next.TakeReadyStep());

        // A step the monitor already retook the P from is counted blocked and holds no P.
        if (Interlocked.Exchange(ref carrier.Retaken, 0) == 0)
        {
            Interlocked.Increment(ref s_blocked);
            carrier.BlockCounted();
            ReleaseP();
        }

        if (!s_runq.IsEmpty)
            EnsureCarrier();

        return true;
    }

    // Go's handoffp/wakep rule: a carrier is added only when none is idle and fewer than P are unblocked.
    private static void EnsureCarrier()
    {
        if (Volatile.Read(ref s_idle) > 0)
            return;

        bool room;

        lock (s_carriersLock)
            room = SharedUnblocked() < s_procs;

        if (room)
            AddCarrier();
    }

    /// <summary>The blocked goroutine's thread is running again: take a P back.</summary>
    internal static void ExitBlocked()
    {
        Interlocked.Increment(ref s_pWaiters);
        s_p.Wait();
        Interlocked.Decrement(ref s_pWaiters);
        Interlocked.Decrement(ref s_blocked);
        t_carrier?.UnblockCounted();

        if (t_carrier is { } carrier)
        {
            carrier.Injected = false;
            carrier.PauseAtStart = GC.GetTotalPauseDuration().Ticks;
            Volatile.Write(ref carrier.RunningSince, Stopwatch.GetTimestamp());
        }
    }

    private static void ReleaseP()
    {
        // A P the monitor injected is retired by the first release that finds the budget exceeded.
        while (true)
        {
            int over = Volatile.Read(ref s_overBudget);

            if (over <= 0)
                break;

            if (Interlocked.CompareExchange(ref s_overBudget, over - 1, over) == over)
                return;
        }

        s_p.Release();
    }

    private static void AddCarrier()
    {
        Carrier carrier = new();

        lock (s_carriersLock)
        {
            s_carriers.Add(carrier);

            if (s_carriers.Count > s_peakCarriers)
                s_peakCarriers = s_carriers.Count;
        }

        try
        {
            Thread thread = new(carrier.Loop, CarrierStackSize) { IsBackground = true, Name = "go-carrier" };

            // A carrier is often started from inside a goroutine's step. It must not start under that
            // goroutine's ExecutionContext, or every goroutine it mounts afterwards could read it.
            if (s_contextSwap)
                thread.UnsafeStart();
            else
                thread.Start();

            Interlocked.Increment(ref s_carriersStarted);
        }
        catch (OutOfMemoryException)
        {
            lock (s_carriersLock)
                s_carriers.Remove(carrier);

            Interlocked.Increment(ref s_carrierStartFailures);
        }
    }

    // Go's sysmon, reduced to the one job the spike needs: a step that has run past the threshold (net of
    // GC pauses) while work is queued and no carrier is idle gets a P injected beside it.
    private static void Monitor()
    {
        long lastSample = 0;
        List<Goroutine> stolen = [];

        while (true)
        {
            Thread.Sleep(2);
            long now = Stopwatch.GetTimestamp();

            if (s_reading && now - lastSample > Stopwatch.Frequency / 4)
            {
                lastSample = now;
                SampleThreads();
                CheckIdleSet();
            }

            if (s_runNext)
            {
                // The steals are queued after the walk: Enqueue may add a carrier, and the lock is reentrant,
                // so a queue inside the walk changes the list under its own enumerator.
                stolen.Clear();

                lock (s_carriersLock)
                {
                    foreach (Carrier carrier in s_carriers)
                    {
                        long since = Volatile.Read(ref carrier.RunningSince);

                        if (since == 0 || now - since < Stopwatch.Frequency / 1000 || Volatile.Read(ref carrier.Next) is null)
                            continue;

                        if (Interlocked.Exchange(ref carrier.Next, null) is { } next)
                            stolen.Add(next);
                    }
                }

                foreach (Goroutine next in stolen)
                {
                    Interlocked.Increment(ref s_runNextSteals);
                    Enqueue(next, next.TakeReadyStep());
                }
            }

            bool queued = !s_runq.IsEmpty && Volatile.Read(ref s_idle) == 0;

            // A carrier holding a dequeued step waits for a P with the queue empty: that is work waiting too.
            if (s_retake && (queued || Volatile.Read(ref s_pWaiters) > 0))
            {
                int retake = 0;
                int retakePinned = 0;

                lock (s_carriersLock)
                {
                    foreach (Carrier carrier in s_carriers)
                    {
                        bool native = Volatile.Read(ref carrier.RunningSince) != 0 &&
                                      carrier.Mounted is { } mounted && (Volatile.Read(ref mounted.m_nativeDepth) & 1) != 0;

                        if (!native)
                            carrier.NativeTicks = 0;
                        else if (++carrier.NativeTicks >= 2)
                        {
                            // DESIGN-mn-pin.md 15.2: the flag is set BEFORE the retake is published, so a step that releases
                            // the retake at once still lowers the pinned count it raises.
                            bool pinned = carrier.IsPinned && !s_retakeUnpinned;
                            carrier.BlockedPinned = pinned;

                            if (Interlocked.CompareExchange(ref carrier.Retaken, 1, 0) == 0)
                            {
                                retake++;

                                if (pinned)
                                    retakePinned++;
                            }
                        }
                    }
                }

                for (int i = 0; i < retake; i++)
                {
                    Interlocked.Increment(ref s_retakes);
                    Interlocked.Increment(ref s_blocked);

                    if (i < retakePinned)
                        Interlocked.Increment(ref s_pinnedBlocked);

                    ReleaseP();
                    EnsureCarrier();
                }
            }

            // DESIGN-mn-pin.md 10.5 and 11.2: a carrier waiting for a P (a pinned wake, or a thread back from a block) is
            // waiting work too, as the retake already reads it. With the run queue empty a tick injects at most one P per
            // such waiter, net of the permits already free, and adds no carrier: the waiters are threads already.
            int waiters = s_injectWaiters ? Volatile.Read(ref s_pWaiters) - s_p.CurrentCount : 0;

            if (s_preemptTicks == 0 || (!queued && waiters <= 0))
                continue;

            long pauses = GC.GetTotalPauseDuration().Ticks;
            int inject = 0;

            lock (s_carriersLock)
            {
                foreach (Carrier carrier in s_carriers)
                {
                    // Only as many carriers spend their once-per-step injection as there are waiters to serve.
                    if (!queued && inject >= waiters)
                        break;

                    long since = Volatile.Read(ref carrier.RunningSince);

                    if (since == 0 || carrier.Injected || Volatile.Read(ref carrier.Retaken) != 0)
                        continue;

                    long paused = (pauses - carrier.PauseAtStart) * Stopwatch.Frequency / TimeSpan.TicksPerSecond;

                    if (now - since - paused > s_preemptTicks)
                    {
                        carrier.Injected = true;
                        inject++;
                    }
                }
            }

            for (int i = 0; i < inject; i++)
            {
                if (Volatile.Read(ref s_overBudget) >= s_maxInject)
                    break;

                Interlocked.Increment(ref s_injections);
                int over = Interlocked.Increment(ref s_overBudget);
                s_p.Release();

                if (!queued)
                    continue;

                bool room;

                // DESIGN-mn-pin.md 11.1: the UNBLOCKED count, as the retire and EnsureCarrier read it; a pinned carrier
                // waiting for its goroutine is blocked and runs nothing queued.
                lock (s_carriersLock)
                    room = (s_roomRaw ? s_carriers.Count : SharedUnblocked()) < s_procs + over;

                if (room)
                    AddCarrier();
            }
        }
    }

    private sealed class Carrier
    {
        internal Goroutine? Mounted;
        internal GoParkSlot? PendingSlot;
        internal object? PendingContinuation;
        internal long RunningSince;
        internal long PauseAtStart;
        internal volatile bool Injected;

        // 1 while the monitor holds this step's P retaken (the mounted goroutine was inside a native call),
        // and how many ticks in a row the monitor has found it there.
        internal int Retaken;
        internal int NativeTicks;

        // The run-next slot (the goroutine carries its step), whether this carrier is inside a step, and
        // how many slot runs in a row it has made.
        internal Goroutine? Next;
        internal bool InStep;
        internal int Chain;

        // The caller of a twin that ended in this step, to resume when the ending frame has returned.
        internal GoTaskCore? Continuation;

        // The twin this step resumed (see ResumedTwin). Null from the end of one step to a resume in another.
        internal GoTaskCore? Resuming;

        // The ExecutionContext this thread holds between steps: no goroutine's.
        private ExecutionContext? m_baseContext;

        // THE PIN (DESIGN-mn-pin.md 3). The goroutine this carrier serves alone while it parks locked to this thread;
        // the step its Ready delivers, and the one-permit wake that carries it (a permit, not a pulse: a wake that
        // races the commit is kept); and whether the carrier ends its thread after this step (its goroutine ended
        // locked, Go's rule).
        private Goroutine? m_pinned;

        internal bool IsPinned => m_pinned is not null;

        // DESIGN-mn-pin.md 15.2: whether this carrier's share of s_blocked is a pinned one. Set where its s_blocked rises (by
        // the monitor before a retake is published), read and cleared where it falls.
        internal bool BlockedPinned;

        internal void BlockCounted()
        {
            if (IsPinned)
            {
                BlockedPinned = true;
                Interlocked.Increment(ref s_pinnedBlocked);
            }
        }

        internal void UnblockCounted()
        {
            if (BlockedPinned)
            {
                BlockedPinned = false;
                Interlocked.Decrement(ref s_pinnedBlocked);
            }
        }
        private object? m_pinnedStep;
        private SemaphoreSlim? m_pinWake;
        private bool m_retiring;

        // THE CURE (DESIGN-mn-pin.md 14): this carrier's own idle wake, and whether it sits in the idle set. Both are
        // read and written under s_idleLock only.
        internal readonly SemaphoreSlim m_idleWake = new(0);
        internal bool m_idle;

        private void RegisterIdle()
        {
            lock (s_idleLock)
            {
                m_idle = true;
                s_idleList.Add(this);
                s_idle = s_idleList.Count;
            }
        }

        // False when a waker has already claimed this carrier: its permit is then on the way.
        private bool TryUnregisterIdle()
        {
            lock (s_idleLock)
            {
                if (!m_idle)
                    return false;

                m_idle = false;
                s_idleList.Remove(this);
                s_idle = s_idleList.Count;
                return true;
            }
        }

        internal void Deliver(object step)
        {
            Volatile.Write(ref m_pinnedStep, step);
            m_pinWake!.Release();
        }

        // 3.2: the pinned goroutine is parked. Hand on whatever waits in the run-next slot, count this carrier BLOCKED
        // (EnsureCarrier backfills the pool, and the idle retire and the injection room read the unblocked count), and
        // wait, holding no P, on the private wake. A pinned carrier never registers in the idle set, so it never counts in
        // s_idle (12.1: EnsureCarrier's idle return relies on it).
        private Item WaitPinned(Goroutine goroutine)
        {
            if (Interlocked.Exchange(ref Next, null) is { } next)
                Enqueue(next, next.TakeReadyStep());

            Interlocked.Increment(ref s_blocked);
            int waiting = Interlocked.Increment(ref s_pinnedWaiting);
            BlockCounted();
            int peak;

            while (waiting > (peak = Volatile.Read(ref s_peakPinnedWaiting)))
            {
                if (Interlocked.CompareExchange(ref s_peakPinnedWaiting, waiting, peak) == peak)
                    break;
            }

            if (!s_runq.IsEmpty)
                EnsureCarrier();

            m_pinWake!.Wait();

            Interlocked.Decrement(ref s_pinnedWaiting);
            Interlocked.Decrement(ref s_blocked);
            UnblockCounted();
            Interlocked.Increment(ref s_pinnedWakes);

            return new Item(goroutine, Interlocked.Exchange(ref m_pinnedStep, null)!);
        }

        internal void Loop()
        {
            t_carrier = this;
            m_baseContext = ExecutionContext.Capture();

            while (true)
            {
                Item item;

                // 3.4: the goroutine ended locked to this thread, so the thread ends with it, as Go terminates it.
                if (m_retiring)
                {
                    // DESIGN-mn-pin.md 15 (P3's MUST, (i)): the retiring thread starts its replacement whenever fewer
                    // than P shared carriers remain, whatever the queue holds: a later Enqueue would strand too.
                    lock (s_carriersLock)
                    {
                        s_carriers.Remove(this);
                        RefillShared();
                    }

                    Interlocked.Increment(ref s_pinRetired);
                    return;
                }

                if (m_pinned is { } pinned)
                {
                    item = WaitPinned(pinned);
                }
                else if (Interlocked.Exchange(ref Next, null) is { } next)
                {
                    Chain++;
                    Interlocked.Increment(ref s_runNextSteps);
                    item = new Item(next, next.TakeReadyStep());
                }
                else if (s_runq.TryDequeue(out item))
                {
                    Chain = 0;
                }
                else
                {
                    Chain = 0;

                    // THE CURE (DESIGN-mn-pin.md 14): an idle carrier waits on its OWN semaphore, registered in the idle
                    // set, so a waker can hand it one goroutine directly. Registered first, then the run queue read
                    // again, so an item queued in between is never left behind.
                    RegisterIdle();

                    // Work queued between the read above and the registration: take it at the top of the loop. If a
                    // waker claimed this carrier meanwhile, its permit is coming, and the wait below returns at once.
                    if (!s_runq.IsEmpty && TryUnregisterIdle())
                        continue;

                    bool woken = m_idleWake.Wait(100);

                    // A waker claimed this carrier as the wait timed out: its permit is on the way.
                    if (!woken && !TryUnregisterIdle())
                    {
                        m_idleWake.Wait();
                        woken = true;
                    }

                    // Woken: a hand-off in the run-next slot, or a queued item; both are read at the top of the loop.
                    if (woken || !s_runq.IsEmpty)
                        continue;

                    // A surplus carrier (one added for a hand-off or an injection) retires when idle. Surplus is read
                    // on the UNBLOCKED count (DESIGN-mn-pin.md 10.1): pinned carriers stay in s_carriers, and counting
                    // them would let every shared carrier retire under K >= P pinned and parked.
                    lock (s_carriersLock)
                    {
                        if ((s_retireRaw ? s_carriers.Count : SharedUnblocked()) > s_procs)
                        {
                            s_carriers.Remove(this);
                            return;
                        }
                    }

                    continue;
                }

                Interlocked.Increment(ref s_pWaiters);
                s_p.Wait();
                Interlocked.Decrement(ref s_pWaiters);
                InStep = true;
                Injected = false;
                PauseAtStart = GC.GetTotalPauseDuration().Ticks;
                Volatile.Write(ref RunningSince, Stopwatch.GetTimestamp());

                try
                {
                    RunStep(item);
                }
                finally
                {
                    if (s_reading && Volatile.Read(ref RunningSince) is var since and not 0)
                    {
                        long ticks = Stopwatch.GetTimestamp() - since;

                        if (ticks > Stopwatch.Frequency / 100)
                            Interlocked.Increment(ref s_longSteps);

                        long max;

                        while (ticks > (max = Interlocked.Read(ref s_maxStepTicks)))
                        {
                            if (Interlocked.CompareExchange(ref s_maxStepTicks, ticks, max) == max)
                                break;
                        }
                    }

                    InStep = false;
                    Volatile.Write(ref RunningSince, 0);

                    // A step whose P the monitor retook has none to release: it stops counting as blocked.
                    if (Interlocked.Exchange(ref Retaken, 0) == 1)
                    {
                        Interlocked.Decrement(ref s_blocked);
                        UnblockCounted();
                    }
                    else
                        ReleaseP();
                }
            }
        }

        private void RunStep(Item item)
        {
            Goroutine goroutine = item.Goroutine;

            goroutine.Mount();
            Mounted = goroutine;
            Interlocked.Increment(ref s_steps);

            // M:N (A4). The reading's proof that goroutines moved: a step on a carrier other than the
            // one that ran the goroutine's last step is a migration.
            if (goroutine.m_lastCarrier is { } last && !ReferenceEquals(last, this))
            {
                Interlocked.Increment(ref s_migrations);

                if (!goroutine.m_migrated)
                {
                    goroutine.m_migrated = true;
                    Interlocked.Increment(ref s_migrated);
                }
            }

            goroutine.m_lastCarrier = this;

            // The goroutine's ExecutionContext comes with it: what it wrote to an AsyncLocal on its last
            // carrier is what it reads on this one.
            if (s_contextSwap && goroutine.m_context is { } context)
                ExecutionContext.Restore(context);

            // A parked twin's box resumes as its ResumeAction would: the step is noted for the stack walk, then the box
            // runs. Any other step is an Action (a goroutine's first step, a foreign awaiter's continuation).
            if (item.Step is GoTaskCore box)
            {
                NoteStep(box);
                box.Resume();
            }
            else
            {
                ((Action)item.Step)();
            }

            // Each twin that ended in the step left its awaiting caller here: run the callers one after
            // another, each from this frame, until one parks or the goroutine's root ends.
            while (Continuation is { } caller)
            {
                Continuation = null;
                Resuming = caller;
                caller.Resume();
            }

            Resuming = null;

            // An unrecovered panic left the twin: raise it again HERE, with the goroutine still mounted, so
            // it is unhandled on this thread and golib's backstop reports it for the right goroutine.
            goroutine.RethrowTwinFatal();

            // The pin's fourth step-end case (DESIGN-mn-pin.md 10.2, 11.3): locked, no park pending and the goroutine
            // not ended. GoTaskCore routes a locked goroutine's foreign await into a park, so this is a golib defect:
            // raised here, still mounted, so the report names the goroutine.
            bool locked = goroutine.LockedToThread;

            if (locked && PendingSlot is null && !goroutine.m_twinEnded)
                throw new InvalidOperationException("golib: a locked goroutine's step ended with no park pending");

            // The goroutine keeps the context it ends the step with, and the carrier takes its own back
            // before another goroutine mounts.
            if (s_contextSwap)
            {
                if (ExecutionContext.Capture() is { } current)
                    goroutine.m_context = current;

                if (m_baseContext is { } baseContext)
                    ExecutionContext.Restore(baseContext);
            }

            Mounted = null;
            goroutine.Unmount();

            // Only now may a waker make the goroutine runnable: it is on no carrier.
            if (PendingSlot is { } slot)
            {
                object continuation = PendingContinuation!;
                PendingSlot = null;
                PendingContinuation = null;
                Interlocked.Increment(ref s_parks);

                // THE PIN'S INVARIANT (DESIGN-mn-pin.md 10.3): m_pin is written BEFORE the commit, on pin and on unpin,
                // so a wake that races the commit (Commit's own "already woken" branch calls Ready at once) takes the
                // route the goroutine's next step must run on.
                if (locked)
                {
                    m_pinWake ??= new SemaphoreSlim(0);

                    if (m_pinned is null)
                    {
                        m_pinned = goroutine;
                        Interlocked.Increment(ref s_pins);

                        // DESIGN-mn-pin.md 15: this carrier leaves the shared pool, which is refilled to P at once.
                        lock (s_carriersLock)
                        {
                            s_pinnedCount++;
                            RefillShared();
                        }
                    }

                    goroutine.m_pin = this;
                }
                else if (m_pinned is not null)
                {
                    goroutine.m_pin = null;
                    m_pinned = null;

                    lock (s_carriersLock)
                        s_pinnedCount--;
                }

                // A Gosched yield goes to the GLOBAL queue's tail (Go's goschedImpl -> globrunqput), never the run-next
                // slot, so every goroutine queued before it runs first. A pinned goroutine goes back to its own carrier, after a
                // Thread.Yield, so its thread still gives the processor up as runtime.Gosched did (P3's nit).
                if (RequeuesOnCommit(slot))
                {
                    Interlocked.Increment(ref s_goschedYields);

                    if (goroutine.m_pin is Carrier pin)
                    {
                        Thread.Yield();
                        pin.Deliver(continuation);
                    }
                    else
                    {
                        Enqueue(goroutine, continuation);
                    }
                }
                else
                {
                    slot.Commit(goroutine, continuation);
                }
            }
            else if (m_pinned is not null || locked)
            {
                // The goroutine ended (the fourth case was refused above). Locked: this thread ends with it (3.4).
                goroutine.m_pin = null;

                if (m_pinned is not null)
                {
                    m_pinned = null;

                    lock (s_carriersLock)
                        s_pinnedCount--;
                }

                m_retiring = locked;
            }
        }
    }
}
