// Coro.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Threading;

namespace go.golib;

/// <summary>
/// Go's <c>runtime.newcoro</c>/<c>coroswitch</c> control transfer, on managed threads — extra
/// CONCURRENCY without extra PARALLELISM between exactly two contexts.
/// </summary>
/// <remarks>
/// <para>
/// A coro is not a goroutine and not a coroutine object. Go describes it as "a special channel that
/// always has a goroutine blocked on it": <see cref="Switch"/> makes the caller the blocked party
/// and starts the party that was blocked, so control alternates and the two contexts are never
/// runnable at the same instant. That is the whole primitive, and it is what <c>iter.Pull</c> is
/// built on — the pull iterator's <c>next</c> resumes the sequence function, the sequence's
/// <c>yield</c> resumes the puller, and neither runs while the other does.
/// </para>
/// <para>
/// <b>Why a rendezvous and not a state machine.</b> Go's <c>coroswitch_m</c> swaps stacks. The CLR
/// has no stack switching, so the sequence function has to keep a real stack of its own, which means
/// a real thread — the same conclusion <see cref="Goroutine"/> reaches for goroutines and for the
/// same reason. Two <see cref="SemaphoreSlim"/>s of capacity one make the handoff strict: a switch
/// releases the peer's permit and then blocks on its own, so exactly one side is runnable at any
/// point and neither can run ahead. The alternative shapes — rewriting the sequence function as an
/// iterator state machine, or driving it from a <c>Task</c> — both require the CALLEE to be written
/// for them, and the callee here is arbitrary converted Go.
/// </para>
/// <para>
/// <b>The coro goroutine is a real goroutine, registered before <see cref="Start"/> returns.</b>
/// Go's <c>newcoro</c> creates the g synchronously and <c>runtime.NumGoroutine</c> counts it from
/// that moment — <c>iter</c>'s own tests assert exactly this, checking for one extra goroutine on
/// the statement after <c>Pull</c> returns and for none after <c>stop</c>. A thread started and left
/// to register on its own time would make that count race, so <see cref="Start"/> waits for the
/// handshake. The exit side is the mirror and matters just as much: the goroutine identity is
/// retired BEFORE the caller is released, so the caller can never observe a count that still
/// includes a coro which has finished.
/// </para>
/// <para>
/// <b>Panics and Goexit cross the boundary by not being special.</b> The body runs under
/// <see cref="Goroutine.Run"/>, the same root every <c>go</c> statement uses, so a
/// <see cref="GoexitException"/> ends the coro goroutine after its defers have run, a host
/// containment policy still contains an infrastructure failure to one test, and an unrecovered
/// panic still takes the process down as Go's does. Nothing here re-implements any of that. What a
/// caller sees is whatever the body recorded before it stopped running — <c>iter.Pull</c> keeps a
/// <c>panicValue</c> in the closure both sides share and re-panics with it on the pulling side,
/// which is Go's own mechanism, not an emulation of one.
/// </para>
/// </remarks>
public sealed class Coro
{
    // The two halves of the handoff. Capacity one because a permit is a TURN, not a count: a second
    // release before the peer consumes the first would mean both sides were runnable, which is the
    // one state this primitive exists to make impossible, and SemaphoreSlim throws on it rather than
    // letting it pass silently.
    private readonly SemaphoreSlim m_resume = new(0, 1);
    private readonly SemaphoreSlim m_yield = new(0, 1);

    // The creation handshake — see the "registered before Start returns" note in the class remarks.
    private readonly ManualResetEventSlim m_started = new(false);

    private readonly Action m_body;

    // The coro thread's identity, written by that thread before it signals m_started and read by
    // every other thread after waiting on it, so the publication is ordered by the handshake rather
    // than by luck. Keyed on the managed thread id rather than the Thread object because the id is
    // written from INSIDE the thread: assigning the Thread object would have to happen in Start,
    // racing the body that reads it.
    private volatile int m_threadId;

    private volatile bool m_exited;

    // The two goroutines of the handoff, for the park accounting (golib's Goroutine.Park / Ready):
    // the coro goroutine, published before the creation handshake, and the goroutine that most
    // recently resumed the coro, published before it releases the coro. Either is null on a thread
    // with no goroutine identity, where Park is inert and Ready a no-op.
    private volatile Goroutine? m_coroGoroutine;
    private volatile Goroutine? m_resumer;

    // The creator's synctest bubble, which the coro goroutine joins (see Start), and whether its exit
    // holds the bubble active until the resumer is readied (see Run).
    private SyncTestBubble? m_bubble;
    private bool m_exitBracketOpen;

    private Coro(Action body) => m_body = body;

    /// <summary>
    /// Whether the coro's body has finished and its goroutine has been retired.
    /// </summary>
    public bool Exited => m_exited;

    /// <summary>
    /// Creates a coro whose goroutine is blocked waiting to run <paramref name="body"/>, and returns
    /// once that goroutine EXISTS — registered, counted, and parked.
    /// </summary>
    /// <param name="body">The coro body. It first runs on the switch that follows.</param>
    /// <remarks>
    /// <paramref name="body"/> has not started when this returns, and must not: Go's <c>newcoro</c>
    /// only creates the blocked goroutine, and <c>iter.Pull</c> depends on the distinction — a
    /// <c>stop</c> before any <c>next</c> is required to run the body's early-out arm, and a
    /// sequence function that panics on entry must not panic until something pulls from it.
    /// </remarks>
    public static Coro Start(Action body)
    {
        ArgumentNullException.ThrowIfNull(body);

        Coro coro = new(body);

        // A coro started by a synctest bubble member joins the bubble, counted by the creator (Go's
        // newcoro -> newproc1 inherits the caller's syncGroup). Counted runnable here; its first park,
        // as "coroutine", takes it out of the running set -- the end state Go reaches by creating it
        // parked.
        coro.m_bubble = Goroutine.Current?.Bubble;
        coro.m_bubble?.Spawned();

        // On a pooled goroutine-sized thread (GoroutineThreadPool): a coro's body mints its own
        // goroutine identity (Run, below), runs under this caller's ExecutionContext as a new thread's
        // would, and leaves the thread reset for the next one -- so reuse is invisible to Go code,
        // while the ~100 µs of creating a 256 MB-reserve thread per coro is not paid again.
        try
        {
            GoroutineThreadPool.Run(coro.Run);
        }
        catch
        {
            coro.m_bubble?.SpawnFailed();
            throw;
        }

        // Go's newcoro returns a coro whose goroutine is already accounted for. Waiting here is what
        // makes that true of this one; see the class remarks.
        coro.m_started.Wait();

        return coro;
    }

    /// <summary>
    /// Transfers control to the other side and blocks until control comes back — Go's
    /// <c>coroswitch</c>.
    /// </summary>
    /// <remarks>
    /// Symmetric, as Go's is: the direction is decided by which side is calling, not by the caller
    /// saying. Called from the coro's own goroutine it yields to whoever resumed it; called from
    /// anywhere else it resumes the coro. The resuming side need not be the same thread every time —
    /// <c>iter</c>'s Goexit tests create a pull iterator on one goroutine and finish draining it on
    /// another — so nothing is pinned to a "creator".
    /// </remarks>
    public void Switch()
    {
        // Tested BEFORE the side question, and that order is load-bearing rather than tidy. Go
        // throws here instead of blocking, because a switch onto a coro that has finished would
        // park the caller on a permit nothing can ever release — a caller's bookkeeping bug
        // becoming a silent hang. Asking it first also closes the one way the side test can lie:
        // a managed thread id is only unique among LIVE threads, so once the coro's thread has
        // exited its id can be reissued to an unrelated thread, and that thread would otherwise
        // match m_threadId and take the coro branch into a wait with no peer. Nothing on the coro
        // side can observe this flag — it is set after the body has finished running — so moving
        // the check up costs that side nothing.
        //
        // iter.Pull cannot reach this at all: its `done` latch is set by the body's own deferred
        // call before the body returns, and both next and stop test it before switching. This
        // guards a FUTURE consumer, stated the way Go states it.
        if (m_exited)
            throw new InvalidOperationException("coro: coroswitch on exited coro");

        // Go's coroswitch parks the caller with waitReasonCoroutine and makes the peer runnable in one
        // step. Here each side parks FIRST (Go's commit order: the peer cannot run, and so cannot
        // switch back and ready this side, until the release below), readies the peer on the waker's
        // side, and only then hands it the permit.
        //
        // In a synctest bubble the swap is also held open as ACTIVITY, as coroswitch_m does
        // (sg.incActive before the caller waits, decActive once the peer is runnable): between this
        // side idling and its peer being readied, every member can read idle for an instant, and the
        // bubble would wake its root into a false "deadlock" (measured, S1c's coroutine arm).
        SyncTestBubble? bubble = Goroutine.Current?.Bubble;

        if (Environment.CurrentManagedThreadId == m_threadId)
        {
            // The coro side: hand control back, then park until resumed.
            bubble?.IncActive();

            using (Goroutine.Park(WaitReason.Coroutine))
            {
                Goroutine.Ready(m_resumer);
                bubble?.DecActive();
                m_yield.Release();
                m_resume.Wait();
            }

            return;
        }

        m_resumer = Goroutine.Current;
        bubble?.IncActive();

        using (Goroutine.Park(WaitReason.Coroutine))
        {
            Goroutine.Ready(m_coroGoroutine);
            bubble?.DecActive();
            m_resume.Release();
            m_yield.Wait();
        }
    }

    // The coro goroutine, start to finish.
    private void Run()
    {
        try
        {
            // Goroutine.Run is the root every `go` statement uses: it mints the goroutine identity
            // (which is what makes this thread countable), swallows a Goexit after the body's
            // defers have run, offers a non-panic failure to a host containment policy, and lets a
            // panic reach the fatal path Go gives it. A coro goroutine gets all of that by being one
            // rather than by reproducing any of it.
            Goroutine.Run(() =>
            {
                // Created blocked, per Go's newcoro (newproc1(..., waitReasonCoroutine)) — the body
                // runs on the first switch in. Parked BEFORE the handshake publishes this goroutine,
                // so the first Switch can only ever find it parked when it readies it.
                using (Goroutine.Park(WaitReason.Coroutine))
                {
                    // Published before the handshake, so Start's wait orders both the id and the
                    // goroutine registration ahead of anything the creator does next.
                    m_coroGoroutine = Goroutine.Current;
                    m_threadId = Environment.CurrentManagedThreadId;
                    m_started.Set();

                    m_resume.Wait();
                }

                try
                {
                    m_body();
                }
                finally
                {
                    // The exit is a switch too (Go's coroexit runs coroswitch_m with exit set): hold
                    // the bubble active from before this goroutine leaves it (Goroutine.Run's exit
                    // accounting, next) until its resumer is readied (the outer finally), or the bubble
                    // reads every member idle in between.
                    if (m_bubble is { } bubble)
                    {
                        bubble.IncActive();
                        m_exitBracketOpen = true;
                    }
                }
            }, m_bubble);
        }
        finally
        {
            // Goroutine.Run has returned, so the goroutine identity is already retired and the live
            // count no longer includes this coro. Only now is the resuming side released: the
            // ORDER is the contract, because a puller that observes the count between the body
            // finishing and the identity being retired would see a goroutine Go says is gone.
            //
            // In a finally so that an escaping panic crashes rather than hangs. Go's outcome for an
            // unrecovered panic in a coro is process death either way; the difference is that a
            // released peer dies reporting the panic, while a peer still parked on a permit nobody
            // will release wedges the run until something outside it times out.
            m_exited = true;

            // The resumer is parked in its Switch (the body only ever runs while it is), so it is
            // readied before its permit, as every other switch does.
            Goroutine.Ready(m_resumer);

            if (m_exitBracketOpen)
                m_bubble!.DecActive();

            m_yield.Release();
        }
    }
}
