//******************************************************************************************************
//  GoScheduler.PTokens.cs - the M:N scheduler's P tokens (experimental, opt-in: GoTwins=true and GO2CS_MN=1; inert
//  otherwise): a lock-free hand-back of an idle P to the thread that waits for one.
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Threading;

namespace go.golib;

/// <summary>
/// The scheduler's P tokens (GoScheduler.s_p): one per processor, taken by a carrier to run a step and by a thread that
/// returns from a blocking call (GoScheduler.ExitBlocked), and given back when that P goes idle or blocks.
/// </summary>
/// <remarks>
/// <para>
/// The s_p convoy row (COORD's ruling, docs/phase4/inbox/G/20261122T110220Z-COORD.md item 2): s_p was a SemaphoreSlim,
/// and every wait and every release went through its one monitor. The train-4 storm capture read 19,228 threads back
/// from a blocking call queued on it, 7,201 of them spinning on that monitor, and a run that took 76 s became a hang.
/// Here no lock is shared: the free tokens are one int taken by compare-and-swap, and each waiter waits on its OWN
/// event, in a lock-free FIFO queue, so a release wakes exactly one thread and no thread spins on another's lock. Go's
/// exitsyscall takes an idle P off sched.pidle the same way, under a lock it holds for a few instructions.
/// </para>
/// <para>
/// No lost wakeup. A waiter is COUNTED (m_waiting) before it queues and tries a free token again AFTER it queues,
/// withdrawing its node (compare-and-swap, 0 to withdrawn) when it wins one. A release that finds no live waiter frees
/// its token and, while a counted waiter is not yet queued, takes it back and tries again, so a token is never left
/// free while a waiter sleeps. A waiter granted while it withdrew holds two tokens and gives one back. Every node is
/// used once, so a late grant can never meet a later wait.
/// </para>
/// </remarks>
internal sealed class PTokens
{
    private sealed class Waiter
    {
        internal const int Waiting = 0, Granted = 1, Withdrawn = 2;

        internal int State;
        internal readonly ManualResetEventSlim Event = new(false);
    }

    private readonly ConcurrentQueue<Waiter> m_waiters = new();
    private int m_free;
    private int m_waiting;   // waiters counted and not yet dequeued by a release
    private int m_spinning;  // waiters in the spin tier

    internal PTokens(int count) => m_free = count;

    /// <summary>The free tokens now (the monitor's injection count reads it).</summary>
    internal int CurrentCount => Volatile.Read(ref m_free);

    /// <summary>The waiters counted and not yet served (GolibTests' conservation rows read it at quiescence).</summary>
    internal int WaitingForTest => Volatile.Read(ref m_waiting);

    // TEST SEAM, GolibTests only: called by a waiter between its first failed take and counting itself, the window the
    // post-queue retry closes; a test widens it to make the lost-wakeup arm deterministic. Null in every program.
    internal static Action? BeforeCountForTest;

    private bool TryTake()
    {
        while (true)
        {
            int free = Volatile.Read(ref m_free);

            if (free <= 0)
                return false;

            if (Interlocked.CompareExchange(ref m_free, free - 1, free) == free)
                return true;
        }
    }

    // TEST SEAM, GolibTests only: called by a spinner at each step of its spin, before it tries the free count, so a test
    // can release tokens while it is counted as a spinner (the window LeaveSpin's hand-off closes). Null in every program.
    internal static Action? InSpinForTest;

    // The spin tier's length, in SpinWait steps that never sleep: about SemaphoreSlim's own spin before it blocks.
    private const int SpinSteps = 35;

    /// <summary>Takes a token: hot from the free count while spinning, else on this thread's own event.</summary>
    internal void Wait()
    {
        if (TryTake())
            return;

        // THE SPIN TIER (design (A), COORD's ruling of 2026-11-22). A waiter first spins on the free count, counted in
        // m_spinning, so a release that sees a spinner frees its token to the count and a spinner takes it hot, with no
        // kernel wake. That is what SemaphoreSlim's own spinners buy, without its monitor: the FIFO arm, which woke a
        // kernel sleeper for every grant, measured about 1.7x slower at the median (DESIGN-mn-convoy.md).
        Interlocked.Increment(ref m_spinning);
        bool taken = false;
        SpinWait spin = default;

        for (int step = 0; step < SpinSteps && !taken; step++)
        {
            InSpinForTest?.Invoke();
            spin.SpinOnce(sleep1Threshold: -1);
            taken = TryTake();
        }

        LeaveSpin();

        if (taken)
            return;

        BeforeCountForTest?.Invoke();

        Waiter waiter = new();
        Interlocked.Increment(ref m_waiting);
        m_waiters.Enqueue(waiter);

        // A release that found no waiter queued may have freed its token after the take above failed: win it here and
        // withdraw the node, or the token stays free while this thread sleeps.
        if (TryTake())
        {
            if (Interlocked.CompareExchange(ref waiter.State, Waiter.Withdrawn, Waiter.Waiting) == Waiter.Waiting)
                return;

            // Granted meanwhile: this thread holds two tokens; one goes back.
            Release();
            return;
        }

        waiter.Event.Wait();
    }

    /// <summary>Gives a token back: to the free count while a spinner is counted, else to the oldest queued sleeper.</summary>
    internal void Release()
    {
        // The increment is a full fence, so a spinner that stops counting itself after this read of m_spinning sees the
        // freed token in LeaveSpin, and one that stopped before it is not counted here: no token is stranded either way.
        Interlocked.Increment(ref m_free);

        if (Volatile.Read(ref m_spinning) > 0)
            return;

        HandToSleepers();
    }

    // A spinner stops counting itself. The LAST one to leave hands any token still free to the queue: releases that left
    // their tokens for the spinners (Release above) may leave more than the spinners take, and without this a sleeper
    // would sleep beside a free token until some later release. (A spinner that gives up takes the free token itself, by
    // its post-queue retry; the case this hand-off alone serves is the spinners taking FEWER tokens than were left.)
    private void LeaveSpin()
    {
        if (Interlocked.Decrement(ref m_spinning) == 0)
            HandToSleepers();
    }

    // Grants free tokens to queued sleepers, oldest first, until none is free or none is counted. A counted waiter not
    // yet queued would sleep past a token if its own retry already ran: the token goes back and the queue is served
    // again until that waiter is queued (and granted) or has won the token itself.
    private void HandToSleepers()
    {
        while (Volatile.Read(ref m_waiting) > 0 && TryTake())
        {
            Waiter? granted = null;

            while (m_waiters.TryDequeue(out Waiter? waiter))
            {
                Interlocked.Decrement(ref m_waiting);

                if (Interlocked.CompareExchange(ref waiter.State, Waiter.Granted, Waiter.Waiting) == Waiter.Waiting)
                {
                    granted = waiter;
                    break;
                }
            }

            if (granted is not null)
                granted.Event.Set();
            else
                Interlocked.Increment(ref m_free);
        }
    }
}
