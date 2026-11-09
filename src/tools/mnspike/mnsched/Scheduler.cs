// The M:N spike's P2 scheduler prototype (docs/phase4/DESIGN-mn-scheduler.md §5 O6/O7, §8 items 3 and 5). It is a
// spike: standalone, measured, never shipped. It schedules goroutines whose code is an async TWIN onto GOMAXPROCS
// "P" tokens held by carrier threads, and keeps the two properties the design rests on:
//
//   THE BLOCKED-WORKER HANDOFF (O6's fallback, shaped like the native-call seam's Enter/Exit): a goroutine that blocks
//   SYNCHRONOUSLY (untwinned code, or a native call) on a carrier releases its P first, and the scheduler makes sure an
//   idle carrier exists to run queued work; on wake it reacquires a P before running Go code again. A missing twin
//   therefore costs a thread, never progress (the §4.E deadlock of a capped pool cannot happen).
//
//   CARRIER INJECTION (R4): a monitor (Go's sysmon role) that sees a carrier run one work item past a threshold while
//   work is queued and no carrier is idle adds a carrier AND a temporary extra P. A spinning goroutine then costs an
//   extra thread, as it does under today's 1:1 model, and never starves a runnable one. The extra P retires on the
//   next release.
//
// Each mechanism can be switched off (Options), so each arm can be shown failing without it.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MnSched;

public sealed class Options
{
    public int Procs { get; init; } = Environment.ProcessorCount;
    public bool Handoff { get; init; } = true;
    public bool Injection { get; init; } = true;
    public TimeSpan InjectionThreshold { get; init; } = TimeSpan.FromMilliseconds(10);
}

public sealed class Scheduler
{
    [ThreadStatic] private static Carrier? t_carrier;

    private readonly ConcurrentQueue<Action> m_runq = new();
    private readonly SemaphoreSlim m_work = new(0);      // one permit per queued item
    private readonly SemaphoreSlim m_p;                   // the P tokens
    private readonly List<Carrier> m_carriers = new();
    private readonly object m_carriersLock = new();
    private readonly Thread m_monitor;
    private int m_idle;                                   // carriers waiting for work
    private int m_overBudget;                             // temporary Ps added by injection, not yet retired
    private volatile bool m_stop;

    public Options Options { get; }
    public int CarrierCount { get { lock (m_carriersLock) return m_carriers.Count; } }
    public int Injections;
    public int Handoffs;

    public Scheduler(Options options)
    {
        Options = options;
        m_p = new SemaphoreSlim(options.Procs);
        for (int i = 0; i < options.Procs; i++) AddCarrier();
        m_monitor = new Thread(Monitor) { IsBackground = true, Name = "mnsched-sysmon" };
        m_monitor.Start();
    }

    public static bool OnCarrier => t_carrier is not null;

    // `go f()`: f is an async twin. It starts on a carrier; it parks by awaiting a park awaitable.
    public void Go(Func<ValueTask> twin) => Ready(() =>
    {
        ValueTask task;
        try { task = twin(); }
        catch (Exception ex) { Fatal(ex); return; }
        if (!task.IsCompleted) task.AsTask().ContinueWith(t => { if (t.IsFaulted) Fatal(t.Exception!); }, TaskContinuationOptions.ExecuteSynchronously);
        else if (task.IsFaulted) Fatal(task.AsTask().Exception!);
    });

    // goready: queue a parked goroutine's continuation (or a new goroutine's start).
    public void Ready(Action item)
    {
        m_runq.Enqueue(item);
        m_work.Release();
    }

    // The handoff's Enter: called by a SYNCHRONOUS park on a carrier, before the thread blocks.
    public void EnterBlocked()
    {
        if (!Options.Handoff || t_carrier is null) return;
        Interlocked.Increment(ref Handoffs);
        ReleaseP();
        // Some carrier must be free to run whatever is queued now or readied later; Go's handoffp starts an M.
        if (Volatile.Read(ref m_idle) == 0) AddCarrier();
    }

    // The handoff's Exit: called after the thread wakes, before it runs Go code again.
    public void ExitBlocked()
    {
        if (!Options.Handoff || t_carrier is null) return;
        m_p.Wait();
    }

    public void Stop() => m_stop = true;

    private static void Fatal(Exception ex)
    {
        Console.Error.WriteLine($"FATAL goroutine panic: {ex}");
        Environment.Exit(3);
    }

    private void ReleaseP()
    {
        // A P added by injection retires on its first release instead of returning to the pool.
        while (true)
        {
            int over = Volatile.Read(ref m_overBudget);
            if (over == 0) { m_p.Release(); return; }
            if (Interlocked.CompareExchange(ref m_overBudget, over - 1, over) == over) return;
        }
    }

    private void AddCarrier()
    {
        Carrier carrier = new(this);
        lock (m_carriersLock) m_carriers.Add(carrier);
        carrier.Start();
    }

    private void Monitor()
    {
        while (!m_stop)
        {
            Thread.Sleep(2);
            if (!Options.Injection || m_runq.IsEmpty || Volatile.Read(ref m_idle) > 0) continue;

            long now = Stopwatch.GetTimestamp();
            long threshold = (long)(Options.InjectionThreshold.TotalSeconds * Stopwatch.Frequency);
            long pauseNow = PauseStopwatchTicks();
            bool overrun = false;
            // An item's run time is measured NET of the GC pause accrued since it started: a stop-the-world collection
            // is not the goroutine running. Measured gross, a3 at 10^6 injected 28-31 carriers with no goroutine
            // running long (560-620 ms of total GC pause; 0 injections with injection off, 2026-11-09).
            lock (m_carriersLock)
                foreach (Carrier c in m_carriers)
                    if (c.RunningSince is (long since, long pauseAtStart) && now - since - (pauseNow - pauseAtStart) > threshold) { overrun = true; break; }

            if (overrun)
            {
                Interlocked.Increment(ref Injections);
                Interlocked.Increment(ref m_overBudget);
                m_p.Release();     // the temporary extra P
                AddCarrier();
            }
        }
    }

    // The process's cumulative GC pause, in Stopwatch ticks.
    private static long PauseStopwatchTicks() => (long)(GC.GetTotalPauseDuration().TotalSeconds * Stopwatch.Frequency);

    private sealed class Carrier
    {
        private static readonly TimeSpan IdleRetirement = TimeSpan.FromMilliseconds(100);

        private readonly Scheduler m_s;
        private readonly Thread m_thread;
        private long m_runningSince;   // 0 when not running an item
        private long m_pauseAtStart;   // the cumulative GC pause when the item started

        public (long since, long pauseAtStart)? RunningSince
        {
            get
            {
                long v = Volatile.Read(ref m_runningSince);
                return v == 0 ? null : (v, Volatile.Read(ref m_pauseAtStart));
            }
        }

        public Carrier(Scheduler s)
        {
            m_s = s;
            m_thread = new Thread(Loop, 256 * 1024) { IsBackground = true, Name = "mnsched-carrier" };
        }

        public void Start() => m_thread.Start();

        private void Loop()
        {
            t_carrier = this;
            while (!m_s.m_stop)
            {
                Interlocked.Increment(ref m_s.m_idle);
                bool got = m_s.m_work.Wait(IdleRetirement);
                Interlocked.Decrement(ref m_s.m_idle);

                // A SURPLUS carrier (one injection or the handoff added) retires once it has idled past the timeout,
                // so the thread count returns to GOMAXPROCS. On macOS every thread counts against
                // kern.num_taskthreads (2,048 on osx-arm64), so the budget has to come back.
                if (!got)
                {
                    lock (m_s.m_carriersLock)
                        if (m_s.m_carriers.Count > m_s.Options.Procs) { m_s.m_carriers.Remove(this); return; }
                    continue;
                }

                if (!m_s.m_runq.TryDequeue(out Action? item)) continue;

                m_s.m_p.Wait();                                      // hold a P while running Go code
                Volatile.Write(ref m_pauseAtStart, PauseStopwatchTicks());
                Volatile.Write(ref m_runningSince, Stopwatch.GetTimestamp());
                try { item(); }
                catch (Exception ex) { Fatal(ex); }
                finally
                {
                    Volatile.Write(ref m_runningSince, 0);
                    m_s.ReleaseP();
                }
            }
        }
    }
}
