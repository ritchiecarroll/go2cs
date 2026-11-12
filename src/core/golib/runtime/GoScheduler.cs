//******************************************************************************************************
//  GoScheduler.cs - M:N SPIKE (R5b, B1). Spike code: it stays on the spike branch and never lands.
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

    // null (not woken, not parked), s_woken, or the parked continuation (an Action).
    private object? m_state;
    private Goroutine? m_goroutine;

    /// <summary>Whether <see cref="Wake"/> has run.</summary>
    public bool IsWoken => ReferenceEquals(Volatile.Read(ref m_state), s_woken);

    /// <summary>Makes the goroutine parked here runnable, now or as soon as its park is committed.</summary>
    public void Wake()
    {
        if (Interlocked.Exchange(ref m_state, s_woken) is Action continuation)
            GoScheduler.Ready(m_goroutine!, continuation);
    }

    // Called by the carrier once the goroutine is unmounted.
    internal void Commit(Goroutine goroutine, Action continuation)
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

    private static readonly int s_procs = ReadProcs();

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

    private readonly struct Item(Goroutine goroutine, Action step)
    {
        internal readonly Goroutine Goroutine = goroutine;
        internal readonly Action Step = step;
    }

    private static readonly ConcurrentQueue<Item> s_runq = new();
    private static readonly SemaphoreSlim s_work = new(0);      // one permit per queued item
    private static readonly SemaphoreSlim s_p = new(s_procs);   // the P tokens
    private static readonly List<Carrier> s_carriers = [];
    private static readonly object s_carriersLock = new();

    private static int s_started;
    private static int s_idle;
    private static int s_overBudget;
    private static int s_blocked;
    private static int s_peakCarriers;
    private static int s_peakThreads;
    private static long s_steps;
    private static long s_parks;
    private static long s_handoffs;
    private static long s_injections;
    private static long s_carrierStartFailures;
    private static long s_carriersStarted;
    private static long s_longSteps;
    private static long s_maxStepTicks;

    [ThreadStatic] private static Carrier? t_carrier;

    private static int ReadProcs()
    {
        string? text = Environment.GetEnvironmentVariable("GO2CS_MN_PROCS");
        return int.TryParse(text, out int procs) && procs > 0 ? procs : Environment.ProcessorCount;
    }

    /// <summary>Whether the calling thread is a carrier with a goroutine mounted.</summary>
    internal static bool OnCarrier => t_carrier is { Mounted: not null };

    // M:N PROTOTYPE (A1). GO2CS_MN_CONTEXT=0 (a reading) turns the context rule off: a goroutine's
    // ExecutionContext is then not carried from step to step, and a carrier starts under its starter's.
    private static readonly bool s_contextSwap = Environment.GetEnvironmentVariable("GO2CS_MN_CONTEXT") != "0";

    /// <summary>Whether a goroutine's ExecutionContext follows it from carrier to carrier.</summary>
    internal static bool ContextFollowsGoroutine => s_contextSwap;

    // Readings: a twin's step that began on a thread with no goroutine mounted, and a twin's caller resumed
    // inside the frame that completed it (both zero unless something other than a twin or a park was awaited).
    private static long s_offCarrierResumes;
    private static long s_nestedResumes;

    // A twin's step is starting from a delegate: a wake, or an awaiter that is not a twin's.
    internal static void NoteStep()
    {
        if (t_carrier is not { Mounted: not null })
            Interlocked.Increment(ref s_offCarrierResumes);
    }

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
    internal static void Ready(Goroutine goroutine, Action step)
    {
        if (Volatile.Read(ref s_started) == 0)
            Start();

        if (s_runNext && t_carrier is { InStep: true, Chain: < MaxRunNextChain } carrier)
        {
            goroutine.m_readyStep = step;

            if (Interlocked.Exchange(ref carrier.Next, goroutine) is { } older)
                Enqueue(older, older.TakeReadyStep());

            return;
        }

        Enqueue(goroutine, step);
    }

    private static void Enqueue(Goroutine goroutine, Action step)
    {
        s_runq.Enqueue(new Item(goroutine, step));
        s_work.Release();

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
               $" handoffs={Interlocked.Read(ref s_handoffs)} injections={Interlocked.Read(ref s_injections)}" +
               $" run-next-steps={Interlocked.Read(ref s_runNextSteps)} run-next-steals={Interlocked.Read(ref s_runNextSteals)}" +
               $" steps-over-10ms={Interlocked.Read(ref s_longSteps)}" +
               $" max-step-ms={Interlocked.Read(ref s_maxStepTicks) * 1000 / Stopwatch.Frequency}" +
               $" carrier-start-failures={Interlocked.Read(ref s_carrierStartFailures)}" +
               $" retakes={Interlocked.Read(ref s_retakes)}" +
               $" off-carrier-resumes={Interlocked.Read(ref s_offCarrierResumes)}" +
               $" nested-resumes={Interlocked.Read(ref s_nestedResumes)}" +
               $" context={(s_contextSwap ? "goroutine" : "off")}" + Goroutine.MainReading() + Goroutine.MountReading();
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

    // Records the park of the goroutine mounted on this carrier. The carrier commits it after the unmount.
    internal static void ParkPending(GoParkSlot slot, Action continuation)
    {
        Carrier? carrier = t_carrier;

        if (carrier?.Mounted is null)
            throw new InvalidOperationException("golib: a goroutine twin parked off its carrier");

        if (carrier.PendingSlot is not null)
            throw new InvalidOperationException("golib: a goroutine twin parked twice in one step");

        carrier.PendingSlot = slot;
        carrier.PendingContinuation = continuation;
    }

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
            room = s_carriers.Count - Volatile.Read(ref s_blocked) < s_procs;

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

                lock (s_carriersLock)
                {
                    foreach (Carrier carrier in s_carriers)
                    {
                        bool native = Volatile.Read(ref carrier.RunningSince) != 0 &&
                                      carrier.Mounted is { } mounted && (Volatile.Read(ref mounted.m_nativeDepth) & 1) != 0;

                        if (!native)
                            carrier.NativeTicks = 0;
                        else if (++carrier.NativeTicks >= 2 && Interlocked.CompareExchange(ref carrier.Retaken, 1, 0) == 0)
                            retake++;
                    }
                }

                for (int i = 0; i < retake; i++)
                {
                    Interlocked.Increment(ref s_retakes);
                    Interlocked.Increment(ref s_blocked);
                    ReleaseP();
                    EnsureCarrier();
                }
            }

            if (s_preemptTicks == 0 || !queued)
                continue;

            long pauses = GC.GetTotalPauseDuration().Ticks;
            int inject = 0;

            lock (s_carriersLock)
            {
                foreach (Carrier carrier in s_carriers)
                {
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

                bool room;

                lock (s_carriersLock)
                    room = s_carriers.Count < s_procs + over;

                if (room)
                    AddCarrier();
            }
        }
    }

    private sealed class Carrier
    {
        internal Goroutine? Mounted;
        internal GoParkSlot? PendingSlot;
        internal Action? PendingContinuation;
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

        // The ExecutionContext this thread holds between steps: no goroutine's.
        private ExecutionContext? m_baseContext;

        internal void Loop()
        {
            t_carrier = this;
            m_baseContext = ExecutionContext.Capture();

            while (true)
            {
                Item item;

                if (Interlocked.Exchange(ref Next, null) is { } next)
                {
                    Chain++;
                    Interlocked.Increment(ref s_runNextSteps);
                    item = new Item(next, next.TakeReadyStep());
                }
                else
                {
                    Chain = 0;
                    Interlocked.Increment(ref s_idle);
                    bool signalled = s_work.Wait(100);
                    Interlocked.Decrement(ref s_idle);

                    if (!signalled)
                    {
                        // A surplus carrier (one added for a hand-off or an injection) retires when idle.
                        lock (s_carriersLock)
                        {
                            if (s_carriers.Count > s_procs)
                            {
                                s_carriers.Remove(this);
                                return;
                            }
                        }

                        continue;
                    }

                    if (!s_runq.TryDequeue(out item))
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
                        Interlocked.Decrement(ref s_blocked);
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

            // The goroutine's ExecutionContext comes with it: what it wrote to an AsyncLocal on its last
            // carrier is what it reads on this one.
            if (s_contextSwap && goroutine.m_context is { } context)
                ExecutionContext.Restore(context);

            item.Step();

            // Each twin that ended in the step left its awaiting caller here: run the callers one after
            // another, each from this frame, until one parks or the goroutine's root ends.
            while (Continuation is { } caller)
            {
                Continuation = null;
                caller.Resume();
            }

            // An unrecovered panic left the twin: raise it again HERE, with the goroutine still mounted, so
            // it is unhandled on this thread and golib's backstop reports it for the right goroutine.
            goroutine.RethrowTwinFatal();

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
                Action continuation = PendingContinuation!;
                PendingSlot = null;
                PendingContinuation = null;
                Interlocked.Increment(ref s_parks);
                slot.Commit(goroutine, continuation);
            }
        }
    }
}
