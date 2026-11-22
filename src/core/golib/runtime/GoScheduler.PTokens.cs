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
/// The s_p convoy row (COORD's ruling, docs/phase4/inbox/G/20261122T110220Z-COORD.md item 2; docs/phase4/DESIGN-mn-convoy.md):
/// s_p was a SemaphoreSlim, and every wait and every release went through its one monitor. The train-4 storm capture
/// read 19,228 threads back from a blocking call queued on it, 7,201 of them spinning on that monitor. Here no lock is
/// shared: the free tokens are one int taken by compare-and-swap; a waiter first SPINS on that count (the spin tier,
/// capped), then sleeps on its OWN event in a lock-free FIFO queue, so a release hands a token to a spinner hot or wakes
/// exactly one sleeper, and no thread spins on another's lock.
/// </para>
/// <para>
/// No lost wakeup, no lost or duplicated token. A sleeper is COUNTED (m_waiting: the nodes still Waiting) before it
/// queues and tries a free token again AFTER it queues, withdrawing its node (compare-and-swap, Waiting to Withdrawn)
/// when it wins one; a grant is the same compare-and-swap from Waiting, so a node is granted or withdrawn, never both,
/// and whoever moves it out of Waiting uncounts it. A release that finds no live sleeper queued frees its token and,
/// while a counted sleeper is not yet queued, takes it back and tries again (with backoff). A sleeper granted while it
/// withdrew holds two tokens and gives one back. Every node is used once, so a late grant can never meet a later wait.
/// </para>
/// <para>
/// Ordering. Each pair "interlocked write, then read of the other side's variable" (Release: the free count, then
/// m_spinning; a spinner leaving: m_spinning, then the free count and m_waiting; a sleeper: m_waiting and its enqueue,
/// then the free count; the hand-off: the free count, then m_waiting) is a Dekker pair, and a full fence sits between
/// the two halves: on arm64 an acquire load may otherwise pass the earlier store (PTokens review, 2026-11-22).
/// </para>
/// </remarks>
internal sealed class PTokens
{
    private sealed class Waiter
    {
        internal const int Waiting = 0, Granted = 1, Withdrawn = 2;

        internal int State;
        private ManualResetEventSlim? m_event;

        // Created only when the waiter sleeps (or is granted first), so a node that withdraws without sleeping, the
        // common case under the spin tier, never allocates one. EnsureInitialized publishes one instance to both sides.
        internal ManualResetEventSlim Event => LazyInitializer.EnsureInitialized(ref m_event, static () => new ManualResetEventSlim(false));
    }

    private readonly ConcurrentQueue<Waiter> m_waiters = new();
    private readonly int m_spinCap;
    private int m_free;
    private int m_waiting;   // nodes still Waiting (counted before they queue)
    private int m_spinning;  // waiters in the spin tier

    internal PTokens(int count)
    {
        m_free = count;

        // Go keeps at most GOMAXPROCS/2 Ms spinning. Uncapped, overlapping spinners kept m_spinning above zero and so
        // left every release to the spinners while FIFO sleepers waited, and thousands of them compared-and-swapped one
        // line. The cap follows the P count the scheduler starts with.
        m_spinCap = Math.Max(1, count / 2);
    }

    /// <summary>
    /// The free tokens now (the monitor's injection count and SetProcs read it). It can dip by one while a hand-off holds
    /// a token on its way to a sleeper, so a reader that injects on it may inject one P early; the monitor's injection is
    /// bounded (MAXINJECT) and retired by ReleaseP, so that is a transient, not a leak.
    /// </summary>
    internal int CurrentCount => Volatile.Read(ref m_free);

    /// <summary>The live waiters counted (GolibTests' conservation rows read it at quiescence).</summary>
    internal int WaitingForTest => Volatile.Read(ref m_waiting);

    // TEST SEAMS, GolibTests only; null in every program. BeforeCount: a sleeper between its failed take (or spin) and
    // counting itself, the window the post-queue retry closes. InSpin: a spinner at each step, before it tries the free
    // count. BeforeRetry: a sleeper after it queued, before its retry, the window where a grant and a withdraw race.
    // BeforeWithdraw: a timed wait whose event timed out, before its withdraw, where a grant and the timeout race.
    internal static Action? BeforeCountForTest;
    internal static Action? InSpinForTest;
    internal static Action? BeforeRetryForTest;
    internal static Action? BeforeWithdrawForTest;

    // The spin tier's length, in SpinWait steps that never sleep: about SemaphoreSlim's own spin before it blocks.
    private const int SpinSteps = 35;

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

    /// <summary>Takes a token: hot from the free count while spinning, else on this thread's own event.</summary>
    internal void Wait() => Wait(Timeout.Infinite);

    /// <summary>
    /// Takes a token within <paramref name="millisecondsTimeout"/> (SemaphoreSlim's contract): 0 only tries the free count
    /// (no spin, no queue); <see cref="Timeout.Infinite"/> waits until one is granted.
    /// </summary>
    /// <returns><c>true</c> when the caller holds a token.</returns>
    internal bool Wait(int millisecondsTimeout)
    {
        if (TryTake())
            return true;

        if (millisecondsTimeout == 0)
            return false;

        // THE SPIN TIER (design (A)): a waiter first spins on the free count, counted in m_spinning, so a release that
        // sees a spinner frees its token to the count and a spinner takes it hot, with no kernel wake. Over the cap, an
        // arrival goes straight to the queue (never counted, so a release never leaves a token for it).
        if (TryEnterSpin())
        {
            bool taken = false;

            // SpinOnce may Sleep(0), where an interrupt lands: the spinner is uncounted on every way out.
            try
            {
                SpinWait spin = default;

                for (int step = 0; step < SpinSteps && !taken; step++)
                {
                    InSpinForTest?.Invoke();
                    spin.SpinOnce(sleep1Threshold: -1);
                    taken = TryTake();
                }
            }
            finally
            {
                LeaveSpin();
            }

            if (taken)
                return true;
        }

        BeforeCountForTest?.Invoke();

        Waiter waiter = new();
        Interlocked.Increment(ref m_waiting);
        ReapWithdrawnHead();
        m_waiters.Enqueue(waiter);
        BeforeRetryForTest?.Invoke();

        // A release that found no waiter queued may have freed its token after the takes above failed: win it here and
        // withdraw the node, or the token stays free while this thread sleeps.
        Interlocked.MemoryBarrier();

        if (TryTake())
        {
            if (Withdraw(waiter))
                return true;

            // Granted meanwhile: this thread holds two tokens; one goes back.
            Release();
            return true;
        }

        bool granted;

        try
        {
            granted = waiter.Event.Wait(millisecondsTimeout);
        }
        catch (ThreadInterruptedException)
        {
            // Interrupted while asleep: leave nothing behind. Withdrawn, the node can no longer be granted; granted
            // already, its token goes back.
            if (!Withdraw(waiter))
                Release();

            throw;
        }

        if (!granted)
            BeforeWithdrawForTest?.Invoke();

        // Timed out: withdraw; a failed withdraw means the grant won the race, and the token is ours.
        return granted || !Withdraw(waiter);
    }

    // Moves a node from Waiting to Withdrawn and uncounts it; false when it was granted first.
    private bool Withdraw(Waiter waiter)
    {
        if (Interlocked.CompareExchange(ref waiter.State, Waiter.Withdrawn, Waiter.Waiting) != Waiter.Waiting)
            return false;

        Interlocked.Decrement(ref m_waiting);
        return true;
    }

    // Withdrawn nodes are left in the queue (a lock-free queue cannot remove from its middle), and a queue no release
    // dequeues while spinners are present would grow by one per withdraw. Each arrival reaps the withdrawn nodes at the
    // head. A node dequeued here that turns out live (the head moved between the peek and the dequeue) goes back to the
    // tail, still counted, so no waiter is lost; a release that meets the queue empty meanwhile backs off and retries.
    private void ReapWithdrawnHead()
    {
        while (m_waiters.TryPeek(out Waiter? head) && Volatile.Read(ref head.State) == Waiter.Withdrawn)
        {
            if (!m_waiters.TryDequeue(out Waiter? taken))
                return;

            if (Volatile.Read(ref taken.State) != Waiter.Withdrawn)
            {
                m_waiters.Enqueue(taken);
                return;
            }
        }
    }

    /// <summary>Gives a token back: to the free count while a spinner is counted, else to the oldest queued sleeper.</summary>
    internal void Release() => Release(1);

    /// <summary>
    /// Gives <paramref name="count"/> tokens back (SetProcs grows the P count this way). Never a bare addition to the free
    /// count: with no spinner counted the queued sleepers are served, min(count, sleepers) of them.
    /// </summary>
    internal void Release(int count)
    {
        if (count <= 0)
            return;

        Interlocked.Add(ref m_free, count);
        Interlocked.MemoryBarrier();

        if (Volatile.Read(ref m_spinning) > 0)
            return;

        HandToSleepers();
    }

    // Counts a spinner while fewer than the cap are counted.
    private bool TryEnterSpin()
    {
        while (true)
        {
            int spinning = Volatile.Read(ref m_spinning);

            if (spinning >= m_spinCap)
                return false;

            if (Interlocked.CompareExchange(ref m_spinning, spinning + 1, spinning) == spinning)
                return true;
        }
    }

    // A spinner stops counting itself. The LAST one to leave hands any token still free to the queue: releases that left
    // their tokens for the spinners (Release above) may leave more than the spinners take, and without this a sleeper
    // would sleep beside a free token until some later release. (A spinner that gives up takes the free token itself, by
    // its post-queue retry; the case this hand-off alone serves is the spinners taking FEWER tokens than were left.)
    private void LeaveSpin()
    {
        if (Interlocked.Decrement(ref m_spinning) == 0)
        {
            Interlocked.MemoryBarrier();
            HandToSleepers();
        }
    }

    // Grants free tokens to queued sleepers, oldest first, until none is free or none is counted. A counted sleeper not
    // yet queued (preempted between counting and queuing) would sleep past a token if its own retry already ran: the
    // token goes back and the queue is served again, with backoff, until that sleeper is queued (and granted) or has won
    // the token itself. Withdrawn nodes met on the way are reaped.
    private void HandToSleepers()
    {
        SpinWait backoff = default;

        while (true)
        {
            Interlocked.MemoryBarrier();

            if (Volatile.Read(ref m_waiting) == 0 || !TryTake())
                return;

            Waiter? granted = null;

            while (m_waiters.TryDequeue(out Waiter? waiter))
            {
                if (Interlocked.CompareExchange(ref waiter.State, Waiter.Granted, Waiter.Waiting) == Waiter.Waiting)
                {
                    Interlocked.Decrement(ref m_waiting);
                    granted = waiter;
                    break;
                }
            }

            if (granted is not null)
            {
                granted.Event.Set();
                backoff = default;
                continue;
            }

            Interlocked.Increment(ref m_free);
            backoff.SpinOnce();
        }
    }
}
