// GoTimer.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace go.golib;

/// <summary>
/// A Go runtime timer: a callback that runs once at a <see cref="MonotonicClock"/> time, on the one timer
/// service thread, from one heap. Go's <c>runtime.timer</c> with <c>t.modify(when)</c> and <c>t.stop()</c>.
/// </summary>
/// <remarks>
/// <para>
/// WHY (G's design approval, 2026-10-19). Go has ONE timer mechanism: an I/O deadline (<c>pd.rt</c>,
/// <c>pd.wt</c>) is an ordinary runtime timer, armed by <c>modify</c> on the same heap as <c>time.Sleep</c>.
/// internal/poll's hand-owns armed each deadline as a <see cref="System.Threading.Timer"/> in whole
/// milliseconds, which windows fires on its 15.6 ms tick: a 50 ms read deadline returned 10-13 ms late, and
/// about 2 ms late on linux. This timer fires within the platform's high-resolution wait of its due time.
/// </para>
/// <para>
/// THE CONTRACT, Go's own: a callback must not block. It runs INLINE on the service thread, as Go runs a
/// timer function from <c>timers.run</c>, so a hand-off to the thread pool cannot put the latency back. A
/// callback that takes a lock briefly and wakes a waiter (netpoll's <c>netpolldeadlineimpl</c>) is the
/// intended use. A callback that runs longer than <see cref="SlowCallbackMilliseconds"/> is counted and
/// named (<see cref="SlowCallbacks"/>; a debug build also writes its name to the debug listeners). It is
/// reported rather than failed, because a garbage collection can stretch any callback past the bound.
/// </para>
/// <para>
/// THE HEAP is index-tracked: each armed timer knows its slot, so <see cref="Reset"/> moves it in place and
/// <see cref="Stop"/> removes it, both in O(log n). An armed timer is in the heap exactly once, whatever
/// number of resets it has seen (net/http resets a connection's deadlines on every request), and a stopped
/// timer is not in it at all, so the queue holds no reference to its callback or state and a closed
/// descriptor's timer is collectable.
/// </para>
/// <para>
/// A callback can still run once after <see cref="Stop"/> when the service thread had already taken it off
/// the heap, exactly as Go's <c>t.stop()</c> returns false for a timer that is running. A caller that must
/// tell a stale firing from a live one checks its own generation, as netpoll does with <c>rseq</c>/<c>wseq</c>.
/// </para>
/// <para>
/// THE WAIT. Windows waits on a high-resolution waitable timer (<see cref="GoHighResolutionTimer"/>, the
/// object Go's windows runtime uses), with no spin. Elsewhere the service thread waits on its wake event to
/// within a millisecond of the head timer's due time, then sleeps out the final sub-millisecond in 250 µs
/// libc <c>nanosleep</c> slices, spinning with <see cref="SpinWait"/> only where that call is missing. Only
/// the head timer is ever waited for that closely, so with one far-future timer armed the service thread is
/// blocked, not running.
/// </para>
/// <para>
/// ONE HEAP, as in Go: package time's timers, tickers and <c>time.Sleep</c> are armed here too (each real-clock
/// runtime timer owns one <see cref="GoTimer"/>), beside internal/poll's deadlines. golib's deadlock report
/// reads <see cref="AnyPending"/>, Go's <c>checkdead</c> timer scan.
/// </para>
/// </remarks>
public sealed partial class GoTimer
{
    private readonly Action<object?> m_callback;
    private readonly object? m_state;

    // Guarded by s_lock. m_index is the timer's slot in s_heap, or -1 when it is not armed.
    private long m_when;
    private long m_sequence;
    private int m_index = -1;

    /// <summary>
    /// Creates a stopped timer that will call <paramref name="callback"/> with <paramref name="state"/> when it
    /// fires. The callback runs on the timer service thread and must not block.
    /// </summary>
    public GoTimer(Action<object?> callback, object? state)
    {
        m_callback = callback ?? throw new ArgumentNullException(nameof(callback));
        m_state = state;
    }

    /// <summary>
    /// Arms the timer to fire at <paramref name="when"/>, a <see cref="MonotonicClock.Nanoseconds"/> reading,
    /// replacing any earlier arm. A time already past fires at once. Go's <c>t.modify(when, 0, …)</c>.
    /// </summary>
    /// <returns>Whether the timer was armed before this call.</returns>
    public bool Reset(long when)
    {
        bool wake;
        bool wasArmed;

        lock (s_lock)
        {
            wasArmed = m_index >= 0;
            m_when = when;
            m_sequence = ++s_sequence;

            if (wasArmed)
                fix(m_index);
            else
                push(this);

            // Only a new head moves the service thread's wait.
            wake = m_index == 0;

            ensureServiceLocked();
        }

        if (wake)
            wakeService();

        return wasArmed;
    }

    /// <summary>
    /// Stops the timer and releases it from the heap. Go's <c>t.stop()</c>.
    /// </summary>
    /// <returns>Whether the timer was armed: false when it had already fired or been stopped.</returns>
    public bool Stop()
    {
        lock (s_lock)
        {
            if (m_index < 0)
                return false;

            removeAt(m_index);
            return true;
        }
    }

    // ---- the queue -------------------------------------------------------------------------------------

    /// <summary>The bound past which a callback is reported as blocking the service thread.</summary>
    public const int SlowCallbackMilliseconds = 5;

    private static readonly object s_lock = new();
    private static GoTimer[] s_heap = new GoTimer[16];
    private static int s_count;
    private static long s_sequence;
    private static Thread? s_service;

    private static readonly AutoResetEvent s_wake = new(false);

    // Set by an arm that made a new head, read by the sub-millisecond spin, which then ends early.
    private static int s_wakeRequested;

    private static long s_slowCallbacks;
    private static string? s_lastSlowCallback;

    // The callbacks of the batch the service thread is running that have not yet returned; set with the
    // batch under s_lock, counted down as each returns.
    private static int s_running;

    /// <summary>The number of armed timers in the heap (a test-visible observation point).</summary>
    internal static int Count
    {
        get
        {
            lock (s_lock)
                return s_count;
        }
    }

    /// <summary>
    /// Whether any timer is armed or any callback is still running: Go's <c>checkdead</c> timer scan. A
    /// running callback counts because what it does (start a goroutine, ready a waiter) has not happened
    /// yet, as Go's checkdead sees the M running a timer as running.
    /// </summary>
    internal static bool AnyPending
    {
        get
        {
            lock (s_lock)
                return s_count > 0 || Volatile.Read(ref s_running) > 0;
        }
    }

    /// <summary>
    /// How many callbacks ran longer than <see cref="SlowCallbackMilliseconds"/>, and the last one's name
    /// (a test-visible observation point).
    /// </summary>
    internal static (long count, string? last) SlowCallbacks => (Interlocked.Read(ref s_slowCallbacks), Volatile.Read(ref s_lastSlowCallback));

    private static void wakeService()
    {
        Volatile.Write(ref s_wakeRequested, 1);
        s_wake.Set();
    }

    private static void ensureServiceLocked()
    {
        if (s_service is not null)
            return;

        s_service = new Thread(service)
        {
            IsBackground = true,
            Name = "go2cs timer service"
        };

        // Started WITHOUT the arming thread's ExecutionContext: the service thread lives for the process
        // and must not carry whichever goroutine armed the first timer, its AsyncLocals (profile labels)
        // or anything else it had flowing, into every callback after it.
        using (ExecutionContext.SuppressFlow())
            s_service.Start();
    }

    private static void service()
    {
        GoHighResolutionTimer? highResolution = GoHighResolutionTimer.TryCreate();
        GoTimer[] due = new GoTimer[16];

        while (true)
        {
            int dueCount = 0;
            long next;

            lock (s_lock)
            {
                long now = MonotonicClock.Nanoseconds();

                while (s_count > 0 && s_heap[0].m_when <= now)
                {
                    if (dueCount == due.Length)
                        Array.Resize(ref due, due.Length * 2);

                    due[dueCount++] = s_heap[0];
                    removeAt(0);
                }

                next = s_count > 0 ? s_heap[0].m_when : long.MaxValue;
                Volatile.Write(ref s_running, dueCount);
                Volatile.Write(ref s_wakeRequested, 0);
            }

            for (int i = 0; i < dueCount; i++)
            {
                try
                {
                    run(due[i]);
                }
                finally
                {
                    Interlocked.Decrement(ref s_running);
                }

                due[i] = null!;
            }

            if (next == long.MaxValue)
                s_wake.WaitOne();
            else
                waitUntil(next, highResolution);
        }
    }

    private static void run(GoTimer timer)
    {
        long started = Stopwatch.GetTimestamp();

        timer.m_callback(timer.m_state);

        TimeSpan elapsed = Stopwatch.GetElapsedTime(started);

        if (elapsed.TotalMilliseconds <= SlowCallbackMilliseconds)
            return;

        string name = $"{timer.m_callback.Method.DeclaringType?.FullName}.{timer.m_callback.Method.Name}";
        Interlocked.Increment(ref s_slowCallbacks);
        Volatile.Write(ref s_lastSlowCallback, name);
        Debug.WriteLine($"GoTimer: callback {name} ran {elapsed.TotalMilliseconds:F1} ms on the timer service thread; a timer callback must not block");
    }

    // Returns at the head timer's due time, or as soon as an arm makes a new head.
    private static void waitUntil(long when, GoHighResolutionTimer? highResolution)
    {
        long remaining = when - MonotonicClock.Nanoseconds();

        if (remaining <= 0)
            return;

        if (highResolution is not null && highResolution.Arm(remaining))
        {
            WaitHandle.WaitAny([highResolution, s_wake]);
            return;
        }

        // To within a millisecond of the due time on the wake event, which counts whole milliseconds (at least
        // one); then the final sub-millisecond in short sleeps, looking for a new head between them.
        const long finalNanos = 1_000_000;

        if (remaining > finalNanos)
        {
            long millis = Math.Max(1, (remaining - finalNanos) / 1_000_000);
            s_wake.WaitOne((int)Math.Min(millis, int.MaxValue));
            return;
        }

        SpinWait spin = default;

        while (Volatile.Read(ref s_wakeRequested) == 0)
        {
            remaining = when - MonotonicClock.Nanoseconds();

            if (remaining <= 0)
                return;

            if (!trySleep(Math.Min(remaining, SliceNanos)))
                spin.SpinOnce(sleep1Threshold: -1);
        }
    }

    // One slice of the final sub-millisecond: a quarter of it, so a new head waits at most that long.
    private const long SliceNanos = 250_000;

    private static bool s_noSleep;

    // libc nanosleep, which sleeps to the microsecond where the managed waits count milliseconds; false where the
    // call is missing (windows without a high-resolution timer), and the caller spins.
    private static bool trySleep(long nanos)
    {
        if (s_noSleep)
            return false;

        try
        {
            Timespec request = new() { Seconds = 0, Nanoseconds = (nint)nanos };
            sys_nanosleep(in request, 0);
            return true;
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            s_noSleep = true;
            return false;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Timespec
    {
        public nint Seconds;
        public nint Nanoseconds;
    }

    [LibraryImport("libc", EntryPoint = "nanosleep")]
    private static partial int sys_nanosleep(in Timespec request, nint remaining);

    // ---- the heap, a binary min-heap on (when, sequence) whose entries know their slot; s_lock held -----

    private static bool less(GoTimer a, GoTimer b) => a.m_when < b.m_when || a.m_when == b.m_when && a.m_sequence < b.m_sequence;

    private static void push(GoTimer timer)
    {
        if (s_count == s_heap.Length)
            Array.Resize(ref s_heap, s_heap.Length * 2);

        place(timer, s_count++);
        siftUp(timer.m_index);
    }

    private static void removeAt(int index)
    {
        GoTimer removed = s_heap[index];
        int last = --s_count;

        if (index != last)
        {
            place(s_heap[last], index);
            fix(index);
        }

        s_heap[last] = null!;
        removed.m_index = -1;
    }

    private static void fix(int index)
    {
        if (!siftUp(index))
            siftDown(index);
    }

    private static bool siftUp(int index)
    {
        GoTimer timer = s_heap[index];
        int start = index;

        while (index > 0)
        {
            int parent = (index - 1) / 2;

            if (!less(timer, s_heap[parent]))
                break;

            place(s_heap[parent], index);
            index = parent;
        }

        place(timer, index);
        return index != start;
    }

    private static void siftDown(int index)
    {
        GoTimer timer = s_heap[index];

        while (true)
        {
            int child = 2 * index + 1;

            if (child >= s_count)
                break;

            if (child + 1 < s_count && less(s_heap[child + 1], s_heap[child]))
                child++;

            if (!less(s_heap[child], timer))
                break;

            place(s_heap[child], index);
            index = child;
        }

        place(timer, index);
    }

    private static void place(GoTimer timer, int index)
    {
        s_heap[index] = timer;
        timer.m_index = index;
    }
}
