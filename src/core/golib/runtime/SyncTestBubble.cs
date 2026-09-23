// SyncTestBubble.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Threading;

namespace go.golib;

/// <summary>
/// A <c>testing/synctest</c> bubble — Go's <c>synctestGroup</c> (runtime/synctest.go), field for
/// field, on golib's park seam (S1c of <c>docs/phase4/DESIGN-gopark-goready-synctest.md</c>).
/// </summary>
/// <remarks>
/// <para>
/// <b>What it counts.</b> <c>Total</c> members; <c>Running</c> members not durably blocked;
/// <c>Active</c> other sources of activity. Go updates the counts from <c>casgstatus</c>
/// (<c>changegstatus</c>) at four points, and golib has one seam for each:
/// </para>
/// <list type="bullet">
/// <item>creation — counted by the CREATOR, at the <c>go</c> statement (Go's <c>newproc</c>), not when
/// the child's thread first runs: a child not yet started is running, and <c>Wait</c> must see it
/// (COORD's creator-side addition, 2026-09-22);</item>
/// <item>park — a durable reason (<see cref="IsIdle"/>) takes the member out of <c>Running</c>;</item>
/// <item>ready — the WAKER puts it back (golib's <see cref="Goroutine.Ready"/>, S1a) before its signal,
/// so the window between a signal and the wakee's resumption can never read "all blocked";</item>
/// <item>exit — the member leaves <c>Total</c> (and <c>Running</c>).</item>
/// </list>
/// <para>
/// <b>Membership</b> is a field on the goroutine, set by its creator (Go's
/// <c>newg.syncGroup = callergp.syncGroup</c>), NOT an <c>AsyncLocal</c>: so a thread that is not a
/// goroutine — the timer service thread above all — can never inherit a bubble by flowing the
/// <c>ExecutionContext</c> of whoever armed the first timer (COORD's addition 4).
/// </para>
/// <para>
/// <b>What it cannot see</b>, and why that is safe: a wait that bypasses <see cref="Goroutine.Park"/>
/// (a BCL wait in hand-owned code, a blocking P/Invoke), or a park whose reason is not in
/// <see cref="IsIdle"/>, leaves its member counted RUNNING, so the bubble never idles and
/// <see cref="Run"/>/<see cref="Wait"/> HANG rather than return early.
/// </para>
/// <para>
/// <b>Fake time (S3).</b> A bubble has its own clock (<see cref="Now"/>, from <see cref="BaseTime"/>)
/// and its own timer heap, held opaquely in <see cref="TimerState"/> by package <c>time</c>, which owns
/// the timer type golib cannot name (COORD's addition 5). <c>time</c> installs
/// <see cref="CheckTimers"/> and <see cref="NextTimerWake"/>; <see cref="Run"/>'s loop fires the due
/// timers, parks, and when the bubble idles advances the clock to the next one -- Go's loop exactly.
/// A bubbled <c>time.Sleep</c> therefore waits on a FAKE timer, woken only by that advance, which is
/// why it is idle here as it is in Go.
/// </para>
/// <para>
/// <b>The pulls (S4).</b> <c>internal/synctest</c>'s five linkname pulls land here through its
/// hand-owned companion: <c>Run</c> → <see cref="Run"/>, <c>Wait</c> → <see cref="Wait"/>,
/// <c>acquire</c> → <see cref="Acquire"/>, <c>release</c> → <see cref="Release"/>, <c>inBubble</c> →
/// <see cref="InBubble"/>. Bubbled channels (S2) tag at make in <c>ChanCore.Bubble</c>.
/// </para>
/// <para>
/// The panics a Go program can <c>recover</c> are raised through <see cref="builtin.panic"/>, so the
/// recovered value is a Go <c>string</c> as it is in <c>runtime/synctest.go</c>; the runtime's own
/// <c>throw</c>s (<c>active &lt; 0</c> and the like) stay plain <see cref="PanicException"/>s.
/// </para>
/// </remarks>
public sealed class SyncTestBubble
{
    /// <summary>Go's <c>synctestBaseTime</c>: midnight UTC 2000-01-01, in nanoseconds.</summary>
    public const long BaseTime = 946684800000000000;

    private readonly object m_mu = new();
    private readonly Goroutine m_root;

    private long m_now = BaseTime;
    private Goroutine? m_waiter;
    private bool m_waiting;
    private int m_total = 1;
    private int m_running = 1;
    private int m_active;

    // Set when Run has decided -- returned or reported its deadlock. A closed bubble wakes no one: its
    // root has moved on, so a member left behind (blocked when Run panicked, and later woken from
    // OUTSIDE the bubble, e.g. by a non-member signalling a plain sync.Cond) must not ready a goroutine
    // that is no longer parked in Run. Go reaches no such state for a truly deadlocked bubble; this is
    // the guard for the one that is not, measured when S1c's red arms left false deadlocks behind and
    // their stray members readied the next test's goroutine.
    private bool m_closed;

    private SyncTestBubble(Goroutine root) => m_root = root;

    /// <summary>The calling goroutine's bubble, or <c>null</c>.</summary>
    public static SyncTestBubble? Current => Goroutine.Current?.Bubble;

    /// <summary>The goroutine that called <see cref="Run"/> (Go's <c>synctestGroup.root</c>).</summary>
    public Goroutine Root => m_root;

    /// <summary>The bubble's fake clock, in nanoseconds since the Unix epoch; <see cref="Run"/> advances it.</summary>
    public long Now
    {
        get { lock (m_mu) return m_now; }
    }

    /// <summary>
    /// Package <c>time</c>'s per-bubble timer state (its fake timer heap) -- opaque to golib, which cannot
    /// name time's types. Go keeps it as <c>synctestGroup.timers</c>.
    /// </summary>
    public object? TimerState { get; set; }

    /// <summary>
    /// Installed by package <c>time</c>: runs every fake timer of the bubble due at its <see cref="Now"/>
    /// (Go's <c>sg.timers.check(sg.now)</c>). Called by <see cref="Run"/> on the root's thread, which is
    /// a member, so a timer func that starts a goroutine (AfterFunc) starts it in the bubble.
    /// </summary>
    public static Action<SyncTestBubble>? CheckTimers { get; set; }

    /// <summary>
    /// Installed by package <c>time</c>: when the bubble's earliest pending fake timer fires, or 0 when
    /// none is pending (Go's <c>sg.timers.wakeTime()</c>). Called under the bubble's lock (the one lock
    /// order: bubble, then time's timer lock).
    /// </summary>
    public static Func<SyncTestBubble, long>? NextTimerWake { get; set; }

    /// <summary>
    /// Installed by package <c>time</c>: whether <c>GODEBUG=asynctimerchan</c> is nonzero, read live
    /// from the same setting time's timers read (Go's <c>debug.asynctimerchan.Load() != 0</c>). A bubble
    /// needs synchronous timer channels, so <see cref="Run"/> refuses under the asynchronous model.
    /// Unset (time never loaded) reads as the default, synchronous.
    /// </summary>
    public static Func<bool>? AsyncTimerChan { get; set; }

    // The counts, read under the bubble's lock: the guards' view.
    public int Total { get { lock (m_mu) return m_total; } }

    public int Running { get { lock (m_mu) return m_running; } }

    public int Active { get { lock (m_mu) return m_active; } }

    // isIdleInSynctest (runtime2.go), whole: Sleep included since S3 put a bubbled sleep on the fake
    // clock (see the class remarks).
    internal static bool IsIdle(WaitReason reason) => reason is
        WaitReason.ChanReceiveNilChan or
        WaitReason.ChanSendNilChan or
        WaitReason.SelectNoCases or
        WaitReason.Sleep or
        WaitReason.SyncCondWait or
        WaitReason.SyncWaitGroupWait or
        WaitReason.Coroutine or
        WaitReason.SynctestRun or
        WaitReason.SynctestWait or
        WaitReason.SynctestChanReceive or
        WaitReason.SynctestChanSend or
        WaitReason.SynctestSelect;

    // ---- the four accounting points (called by Goroutine and Coro) ----

    // A goroutine created by a member: counted runnable by its creator, before its thread starts.
    internal void Spawned()
    {
        lock (m_mu)
        {
            m_total++;
            m_running++;
        }
    }

    // The creator could not start the thread it counted: undo, and let the bubble reconsider.
    internal void SpawnFailed() => Leave();

    // A member's goroutine ended (it was running: a goroutine exits from its own thread).
    internal void Exited() => Leave();

    private void Leave()
    {
        Goroutine? wake;

        lock (m_mu)
        {
            m_total--;
            m_running--;
            CheckCounts();
            wake = MaybeWakeLocked();
        }

        WakeUp(wake);
    }

    // A member parked on a durable reason.
    internal void Idled()
    {
        Goroutine? wake;

        lock (m_mu)
        {
            m_running--;
            CheckCounts();
            wake = MaybeWakeLocked();
        }

        WakeUp(wake);
    }

    // A member that had idled is running again: readied by its waker, or back from a park that ended
    // without one.
    internal void Resumed()
    {
        lock (m_mu)
            m_running++;
    }

    // Go's incActive / decActive.
    internal void IncActive()
    {
        lock (m_mu)
            m_active++;
    }

    internal void DecActive()
    {
        Goroutine? wake;

        lock (m_mu)
        {
            m_active--;

            if (m_active < 0)
                throw new PanicException("synctest: active < 0");

            wake = MaybeWakeLocked();
        }

        WakeUp(wake);
    }

    private void CheckCounts()
    {
        if (m_total < 0)
            throw new PanicException("synctest: total < 0");

        if (m_running < 0)
            throw new PanicException("synctest: running < 0");
    }

    // Go's maybeWakeLocked: when nothing in the bubble can make progress, wake the Wait caller, or else
    // the root, counting the wake itself as activity.
    private Goroutine? MaybeWakeLocked()
    {
        if (m_closed || m_running > 0 || m_active > 0)
            return null;

        m_active++;

        return m_waiter ?? m_root;
    }

    // Go's goready(wake): the waiter and the root park only on their own gate (Run / Wait below).
    private static void WakeUp(Goroutine? gp)
    {
        if (gp is null)
            return;

        Goroutine.Ready(gp);
        gp.ReleaseParkGate();
    }

    // ---- synctest.Run / synctest.Wait (runtime/synctest.go) ----

    /// <summary>
    /// Go's <c>synctestRun</c>: runs <paramref name="f"/> on a new goroutine in a new bubble and returns
    /// once every goroutine in the bubble has exited; panics <c>deadlock: all goroutines in bubble are
    /// blocked</c> if the bubble idles with members left. (The fake-timer loop is S3's; with no timers
    /// the loop runs once.)
    /// </summary>
    public static void Run(Action f)
    {
        if (AsyncTimerChan?.Invoke() == true)
            throw builtin.panic("synctest.Run not supported with asynctimerchan!=0");

        Goroutine gp = Goroutine.Current ?? throw new PanicException("synctest.Run on a thread with no goroutine identity");

        if (gp.Bubble is not null)
            throw builtin.panic("synctest.Run called from within a synctest bubble");

        SyncTestBubble sg = new(gp);
        gp.Bubble = sg;

        try
        {
            Goroutine.Start(f);

            Monitor.Enter(sg.m_mu);
            sg.m_active++;

            while (true)
            {
                Monitor.Exit(sg.m_mu);

                // Go: systemstack(func() { gp.syncGroup.timers.check(gp.syncGroup.now) }) -- every
                // fake timer due at the bubble's clock fires, on this (the root's) thread.
                CheckTimers?.Invoke(sg);

                // gopark(synctestidle_c, nil, waitReasonSynctestRun, ...), with park_m's bracket.
                sg.IncActive();
                bool canIdle;

                using (Goroutine.Park(WaitReason.SynctestRun))
                {
                    lock (sg.m_mu)
                    {
                        if (sg.m_running == 0 && sg.m_active == 1)
                        {
                            canIdle = false;
                        }
                        else
                        {
                            sg.m_active--;
                            canIdle = true;
                        }
                    }

                    if (canIdle)
                    {
                        sg.DecActive();
                        Goroutine.WaitOnParkGate();
                    }
                }

                // park_m's refused-commit arm: the goroutine is running again (the scope's dispose
                // counted that), THEN the bracket closes.
                if (!canIdle)
                    sg.DecActive();

                Monitor.Enter(sg.m_mu);

                if (sg.m_active < 0)
                {
                    Monitor.Exit(sg.m_mu);
                    throw new PanicException("synctest: active < 0");
                }

                // Go: next := sg.timers.wakeTime(); if next == 0 { break }; sg.now = next. The bubble is
                // idle, so fake time jumps straight to the next timer; with none pending, Run is done.
                long next = NextTimerWake?.Invoke(sg) ?? 0;

                if (next == 0)
                    break;

                if (next < sg.m_now)
                {
                    Monitor.Exit(sg.m_mu);
                    throw new PanicException("synctest: time went backwards");
                }

                sg.m_now = next;
            }

            int total = sg.m_total;
            sg.m_closed = true;
            Monitor.Exit(sg.m_mu);

            if (total != 1)
                throw builtin.panic("deadlock: all goroutines in bubble are blocked");
        }
        finally
        {
            gp.Bubble = null;
        }
    }

    /// <summary>
    /// Go's <c>synctestWait</c>: blocks until every other goroutine in the caller's bubble is durably
    /// blocked.
    /// </summary>
    public static void Wait()
    {
        Goroutine? gp = Goroutine.Current;

        if (gp?.Bubble is not { } sg)
            throw builtin.panic("goroutine is not in a bubble");

        lock (sg.m_mu)
        {
            if (sg.m_waiting)
                throw builtin.panic("wait already in progress");

            sg.m_waiting = true;
        }

        // gopark(synctestwait_c, nil, waitReasonSynctestWait, ...), with park_m's bracket: the bubble
        // cannot wake anyone between this goroutine idling and it naming itself the waiter.
        sg.IncActive();

        using (Goroutine.Park(WaitReason.SynctestWait))
        {
            lock (sg.m_mu)
            {
                if (sg.m_running == 0 && sg.m_active == 0)
                    throw new PanicException("synctest: running == 0 && active == 0");

                sg.m_waiter = gp;
            }

            sg.DecActive();
            Goroutine.WaitOnParkGate();
        }

        lock (sg.m_mu)
        {
            sg.m_active--;

            if (sg.m_active < 0)
                throw new PanicException("synctest: active < 0");

            sg.m_waiter = null;
            sg.m_waiting = false;
        }
    }

    // ---- acquire / release / inBubble (runtime/synctest.go; internal/synctest's Bubble) ----

    /// <summary>
    /// Go's <c>synctest_acquire</c>: the caller's bubble with one more source of activity, so it cannot
    /// idle until <see cref="Release"/>; <c>null</c> outside a bubble.
    /// </summary>
    public static SyncTestBubble? Acquire()
    {
        if (Current is not { } sg)
            return null;

        sg.IncActive();
        return sg;
    }

    /// <summary>Go's <c>synctest_release</c>: gives back an <see cref="Acquire"/>d activity.</summary>
    public void Release() => DecActive();

    /// <summary>
    /// Go's <c>synctest_inBubble</c>: runs <paramref name="f"/> on the calling goroutine as a member of
    /// <paramref name="sg"/>, and leaves it again. As in Go, the goroutine joins without being counted
    /// into <c>Total</c> / <c>Running</c>: its caller holds an <see cref="Acquire"/>d activity for it.
    /// </summary>
    public static void InBubble(SyncTestBubble sg, Action f)
    {
        Goroutine gp = Goroutine.Current ?? throw new PanicException("synctest inBubble on a thread with no goroutine identity");

        if (gp.Bubble is not null)
            throw builtin.panic("goroutine is already bubbled");

        gp.Bubble = sg;

        try
        {
            f();
        }
        finally
        {
            gp.Bubble = null;
        }
    }
}
