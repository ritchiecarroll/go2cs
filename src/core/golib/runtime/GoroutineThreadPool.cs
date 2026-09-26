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
/// Reuses goroutine-sized threads for <see cref="Coro"/> bodies, so a coro costs a hand-off rather
/// than a thread creation.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why.</b> A coro needs a stack of its own, and a fresh thread with the goroutine stack reserve
/// (<see cref="Goroutine.StackReserve"/>) costs on the order of 100 µs to create and hand-shake -- once
/// per <c>iter.Pull</c> and once per range-over-func loop, measured at ~110 µs a loop against ~1 µs for
/// the thread-pool hand-off it replaced. An idle worker waits for its next body instead.
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
    /// Runs <paramref name="work"/> on a pooled worker (or a new one), under the caller's
    /// ExecutionContext. Returns once the work is handed over, not when it finishes.
    /// </summary>
    internal static void Run(Action work)
    {
        ExecutionContext? context = ExecutionContext.Capture();

        // LIFO: the most recently idled worker is the warmest.
        while (s_idle.TryPop(out Worker? worker))
        {
            Interlocked.Decrement(ref s_idleCount);

            if (worker.TryAssign(work, context))
                return;
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

            thread.Start();
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

                // Nothing of that goroutine survives into the next one.
                GoroutineThreadState.ResetForReuse();

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
