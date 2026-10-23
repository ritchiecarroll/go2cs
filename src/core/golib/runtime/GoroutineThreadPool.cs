// GoroutineThreadPool.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace go.golib;

/// <summary>
/// Reuses goroutine-sized threads for goroutines (<see cref="Goroutine.Start(Action)"/>) and
/// <see cref="Coro"/> bodies, so starting one costs a hand-off rather than a thread creation.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why.</b> A goroutine needs a stack of its own, and a fresh thread with the goroutine stack reserve
/// (<see cref="Goroutine.StackReserve"/>) costs on the order of 100 µs to create and hand-shake -- for a
/// coro, once per <c>iter.Pull</c> and once per range-over-func loop, measured at ~110 µs a loop against
/// ~1 µs for the thread-pool hand-off it replaced. Thread creation also does not scale: 6-9.5k threads a
/// second on a 16-thread laptop whether 1 creator asks or 100, so a program starting goroutines by the
/// million (runtime's TestChanSendBarrier, 10M) spent its life there and starved every other test beside
/// it (2026-10-23). An idle worker waits for its next body instead.
/// </para>
/// <para>
/// <b>What a worker carries between bodies: NOTHING.</b> Each body mints its own goroutine identity
/// (<see cref="Goroutine.Run(Action, SyncTestBubble?)"/>), runs under the ExecutionContext its creator
/// captured -- exactly what a new thread's start would have flowed, so AsyncLocals (pprof labels, the
/// test host's current test) are the creator's -- and after it the worker resets every thread-static
/// the tree declares (<see cref="GoroutineThreadState"/>), whose census a GolibTests guard keeps
/// complete.
/// </para>
/// <para>
/// <b>Bounded.</b> At most <see cref="MaxIdle"/> workers wait at once, and an idle worker exits after
/// <see cref="IdleTimeout"/>, so an idle program holds no extra threads. Demand beyond the idle set
/// creates threads exactly as before; nothing ever waits for a worker.
/// </para>
/// </remarks>
internal static class GoroutineThreadPool
{
    /// <summary>The most workers kept waiting for a body at once.</summary>
    internal const int MaxIdle = 64;

    /// <summary>How long an idle worker waits for its next body before its thread exits.</summary>
    internal static readonly TimeSpan IdleTimeout = TimeSpan.FromSeconds(10);

    private static readonly ConcurrentStack<Worker> s_idle = new();
    private static int s_idleCount;
    private static long s_created;

    /// <summary>Threads this pool has created, for the guard's reuse assertion.</summary>
    internal static long Created => Interlocked.Read(ref s_created);

    /// <summary>Workers currently waiting for a body.</summary>
    internal static int IdleCount => Volatile.Read(ref s_idleCount);

    /// <summary>
    /// GolibTests' seam: runs on a worker after a body's goroutine has ended and before its thread is
    /// reset (ThreadStateCensusTests plants a finished goroutine's leftovers through it). Null otherwise.
    /// </summary>
    internal static volatile Action? AfterBodyForTest;

    /// <summary>
    /// Runs <paramref name="work"/> on a pooled worker (or a new one), under the caller's
    /// ExecutionContext. Returns once the work is handed over, not when it finishes.
    /// </summary>
    internal static void Run(Action work)
    {
        ExecutionContext? context = ExecutionContext.Capture();

        // LIFO: the most recently idled worker is the warmest. The count is given back only for a worker
        // actually claimed: a worker that idled out left its entry here and gave its own count back as it
        // retired, so decrementing for that entry too drove the count below zero and let the cap admit twice
        // MaxIdle (G's review, 2026-10-23).
        while (s_idle.TryPop(out Worker? worker))
        {
            if (worker.TryAssign(work, context))
            {
                Interlocked.Decrement(ref s_idleCount);
                return;
            }
        }

        Worker created = new();
        Interlocked.Increment(ref s_created);
        created.Start(work, context);
    }

    private sealed class Worker
    {
        private const int Idle = 0, Assigned = 1, Retired = 2;

        private readonly SemaphoreSlim m_signal = new(0, 1);
        private Action? m_work;
        private ExecutionContext? m_context;
        private int m_state = Assigned;

        internal void Start(Action work, ExecutionContext? context)
        {
            m_work = work;
            m_context = context;

            Thread thread = new(loop, Goroutine.StackReserve)
            {
                IsBackground = true
            };

            // UnsafeStart: the worker's own loop takes no ExecutionContext. Start would capture the context of
            // whichever goroutine happened to need a new worker and keep it for the worker's whole life -- its
            // AsyncLocals (pprof labels, the test host's current test) on every later body, and the context a
            // flow-suppressed body would run under instead of an empty one. Each body gets its creator's context
            // from Run (G's review, 2026-10-23).
            thread.UnsafeStart();
        }

        // Claims an idle worker. Fails when the worker retired on its idle timeout in the meantime.
        internal bool TryAssign(Action work, ExecutionContext? context)
        {
            if (Interlocked.CompareExchange(ref m_state, Assigned, Idle) != Idle)
                return false;

            m_work = work;
            m_context = context;
            m_signal.Release();

            return true;
        }

        private void loop()
        {
            while (true)
            {
                Action work = m_work!;
                ExecutionContext? context = m_context;
                m_work = null;
                m_context = null;

                if (context is null)
                    work();
                else
                    ExecutionContext.Run(context, static state => ((Action)state!)(), work);

                // The guard's seam: what a finished goroutine can leave on its thread is planted HERE,
                // after its goroutine scope has closed and before the reset -- planting it inside a live
                // goroutine would hand it to golib's own goroutine-exit machinery instead.
                AfterBodyForTest?.Invoke();

                // A goroutine that exited locked to its thread takes the thread with it (Go: "If the calling
                // goroutine exits without unlocking the thread, the thread will be terminated"). Asked before
                // the reset, which clears what the checks read.
                if (GoroutineThreadState.MustRetire())
                    return;

                // Nothing of that goroutine survives into the next one -- its name included, so the next
                // goroutine's Adopt names the thread for itself.
                GoroutineThreadState.ResetForReuse();
                Thread.CurrentThread.Name = null;

                if (Interlocked.Increment(ref s_idleCount) > MaxIdle)
                {
                    Interlocked.Decrement(ref s_idleCount);
                    return;
                }

                Volatile.Write(ref m_state, Idle);
                s_idle.Push(this);

                if (m_signal.Wait(IdleTimeout))
                    continue;

                // Timed out. Retire -- unless a caller claimed this worker just now, in which case its
                // release is on the way.
                if (Interlocked.CompareExchange(ref m_state, Retired, Idle) == Idle)
                {
                    // The stack entry stays behind and is skipped by TryAssign; the count it held is
                    // given back here, where the worker stops being available.
                    Interlocked.Decrement(ref s_idleCount);
                    return;
                }

                m_signal.Wait();
            }
        }
    }
}

/// <summary>
/// Resets the per-thread state a goroutine leaves behind, so a reused thread starts the next goroutine
/// clean.
/// </summary>
/// <remarks>
/// golib's own thread-statics are reset directly below. Packages above golib cannot be named from here,
/// so each registers its reset from its own static initialization with <see cref="Register"/>: a
/// thread-static is only ever non-default after its declaring type initialized, and type initialization
/// runs the registration first. The census of every <c>[ThreadStatic]</c>, <c>ThreadLocal</c> and
/// <c>AsyncLocal</c> in the tree, and each one's disposition, is GolibTests' ThreadStateCensusTests.
/// </remarks>
public static class GoroutineThreadState
{
    private static readonly List<Action> s_resets = [];
    private static Action[] s_snapshot = [];
    private static readonly Lock s_lock = new();

    /// <summary>
    /// Registers a reset for a package's per-goroutine thread-static state; returns true so it can
    /// initialize a static field.
    /// </summary>
    /// <param name="reset">Restores this thread's slots to their defaults.</param>
    public static bool Register(Action reset)
    {
        lock (s_lock)
        {
            s_resets.Add(reset);
            s_snapshot = [.. s_resets];
        }

        return true;
    }

    private static readonly List<Func<bool>> s_retires = [];
    private static Func<bool>[] s_retireSnapshot = [];

    /// <summary>
    /// Registers a check, asked on a pooled thread after each goroutine ends and before its state is reset,
    /// that the thread must not be reused; returns true so it can initialize a static field.
    /// </summary>
    /// <param name="mustRetire">True when the goroutine that just ended leaves its thread unfit for another.</param>
    /// <remarks>
    /// <c>runtime</c> registers Go's one such rule: a goroutine that exits locked to its thread
    /// (<c>LockOSThread</c> without its <c>UnlockOSThread</c>) takes the thread with it.
    /// </remarks>
    public static bool RegisterRetire(Func<bool> mustRetire)
    {
        lock (s_lock)
        {
            s_retires.Add(mustRetire);
            s_retireSnapshot = [.. s_retires];
        }

        return true;
    }

    /// <summary>
    /// True when a registered check says the goroutine that just ended on this thread leaves it unfit for reuse.
    /// </summary>
    internal static bool MustRetire()
    {
        foreach (Func<bool> mustRetire in Volatile.Read(ref s_retireSnapshot))
        {
            if (mustRetire())
                return true;
        }

        return false;
    }

    /// <summary>
    /// Restores every registered and golib-owned thread-static slot on this thread to its default.
    /// </summary>
    internal static void ResetForReuse()
    {
        AllocationCounter.ResetThread();
        GoschedBackoff.ResetThread();
        SelectPending.ResetThread();
        GoFuncRoot.ResetThread();
        builtin.ResetFallthrough();

        foreach (Action reset in Volatile.Read(ref s_snapshot))
            reset();
    }
}
