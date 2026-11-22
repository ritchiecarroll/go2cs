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

    /// <summary>Takes a token, waiting on this thread's own event while none is free.</summary>
    internal void Wait()
    {
        if (TryTake())
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

    /// <summary>Gives a token back: to the oldest live waiter, else to the free count.</summary>
    internal void Release()
    {
        while (true)
        {
            while (m_waiters.TryDequeue(out Waiter? waiter))
            {
                Interlocked.Decrement(ref m_waiting);

                if (Interlocked.CompareExchange(ref waiter.State, Waiter.Granted, Waiter.Waiting) == Waiter.Waiting)
                {
                    waiter.Event.Set();
                    return;
                }
            }

            Interlocked.Increment(ref m_free);

            // A waiter counted but not yet queued would sleep past this token if its retry already ran: take the token
            // back and serve the queue again until no counted waiter is left unserved.
            if (Volatile.Read(ref m_waiting) == 0 || !TryTake())
                return;
        }
    }
}
