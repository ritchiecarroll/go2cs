// time_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// [LibraryImport] requires /unsafe unconditionally (SYSLIB1062) — the generated stub is written in
// terms of pointers even for a signature that has none. `time`'s converted emission needs nothing
// unsafe, so this package is the one where the declaration is genuinely load-bearing rather than
// merely honest: without it the file below does not compile, and the .csproj that would have said
// otherwise is regenerated on every transpile. Placed above the file-scoped namespace because module
// attributes must precede every other element (CS1730).
[module: go.GoRequiresUnsafe]

namespace go;

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
// Aliased rather than imported wholesale: this file needs exactly two golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using Goroutine = go.golib.Goroutine;
using WaitReason = go.golib.WaitReason;
using @unsafe = unsafe_package;
// For godebug_package's extension methods on ж<Setting> — asyncTimerChan() reads the same
// asynctimerchan setting sleep.cs's syncTimer does.
using @internal;

partial class time_package
{
    // time's now() and runtimeNano() are //go:linkname'd into the Go runtime (runtime.now /
    // runtime.nanotime), so the converter emitted them as bodyless partials — throwing stubs. That
    // made the package UNUSABLE at load: the static initializer `startNano = runtimeNano() - 1`
    // (time.cs) runs runtimeNano() during the type's cctor, so merely IMPORTING time — or the first
    // touch of time.Now / Since / Sub — died with "runtimeNano: external (assembly or cgo) function
    // is not implemented" (surfaced by io/fs, crypto/subtle, and go/doc/comment, whose test-package
    // static init reaches time_package..cctor). These supply the equivalent managed bodies so the
    // clock RUNS. Living in an _impl.cs companion (never emitted by the converter) makes this durable
    // by construction: a -stdlib reconvert regenerates the pristine bodyless partials and these
    // bodies remain. The same applies to Go's runtime TIMERS (Sleep / newTimer / stopTimer /
    // resetTimer), realized in the region at the bottom of this file.

    // runtimeNano returns the current value of a monotonic clock in nanoseconds. Go's
    // runtime.nanotime reads a monotonic source (QueryPerformanceCounter on Windows); Stopwatch is
    // exactly that source on .NET. The tick count is scaled to nanoseconds with the seconds/remainder
    // split so the result stays EXACT and monotonic without overflowing: `ticks * 1e9` would overflow
    // int64 for any real uptime, whereas `seconds * 1e9` (seconds = ticks/Frequency) and
    // `rem * 1e9 / Frequency` (rem < Frequency) each stay well inside int64 and together preserve full
    // sub-tick nanosecond resolution. The absolute epoch is arbitrary — only differences are observed
    // (startNano rebases it), and Now().mono / Sub / Since all read this same source, so the monotonic
    // component is coherent across the package.
    internal static partial int64 runtimeNano()
    {
        int64 ticks = Stopwatch.GetTimestamp();
        int64 freq = Stopwatch.Frequency;
        int64 seconds = ticks / freq;
        int64 rem = ticks % freq;
        return seconds * 1_000_000_000L + rem * 1_000_000_000L / freq;
    }

    // now returns the current wall-clock time as (Unix seconds, sub-second nanoseconds) plus a
    // monotonic reading. Go's runtime.now returns seconds since the Unix epoch (Now() then rebases
    // them by unixToInternal - minWall = 2682288000); DateTime.UtcNow is the managed wall clock
    // (.NET uses GetSystemTimePreciseAsFileTime on modern Windows, ~100 ns granularity). nsec is the
    // sub-second remainder converted from 100 ns Ticks to nanoseconds. mono uses the same monotonic
    // source as runtimeNano() so Now()'s monotonic component agrees with Since / Sub.
    internal static partial (int64 sec, int32 nsec, int64 mono) now()
    {
        // `DateTime` (unqualified) binds to time's own layout constant here, so the wall-clock type
        // is spelled out fully.
        int64 unixTicks = System.DateTime.UtcNow.Ticks - System.DateTime.UnixEpoch.Ticks;
        int64 sec = unixTicks / System.TimeSpan.TicksPerSecond;
        int32 nsec = ((int32)(unixTicks % System.TimeSpan.TicksPerSecond)) * 100;
        int64 mono = runtimeNano();
        return (sec, nsec, mono);
    }

    // ---------------------------------------------------------------------------------------------
    //  Runtime timers — Sleep, newTimer, stopTimer, resetTimer
    // ---------------------------------------------------------------------------------------------
    //
    // Sleep and the newTimer/stopTimer/resetTimer trio are //go:linkname'd into the runtime's timer
    // machinery (runtime/time.go), so the converter emitted them as bodyless partials — throwing
    // stubs. A stub throw on a timer path is uniquely destructive: it lands on whichever goroutine
    // touched the timer, and a goroutine that dies takes the whole host down, so every package that
    // so much as calls time.Sleep once was unreachable. These bodies realize the runtime's timer
    // contract on .NET.
    //
    // SERVICE MODEL. Go keeps timers in per-P heaps, run by whichever P first notices a deadline.
    // The managed model is ONE global heap serviced by one dedicated background thread — the shape
    // Go itself used before per-P timers (the old runtime `timerproc`). One thread suffices and is
    // ordering-faithful because the only two callbacks package time ever installs are `sendTime` (a
    // NON-BLOCKING channel send) and `goFunc` (which only starts a goroutine): neither can block the
    // service thread and delay a later deadline. Callbacks run with the lock released, in deadline
    // order.
    //
    // WHY NOT System.Threading.Timer. Its resolution is the Windows timer tick — measured at ~15 ms
    // on this host, so a 1 ms Go timer would fire ~15x late and any two timers less than a tick
    // apart could fire OUT OF ORDER. The service thread instead waits on a Windows high-resolution
    // waitable timer, precisely the object the Go runtime uses for the same job
    // (runtime/os_windows.go createHighResTimer/usleep), and it reads the same monotonic clock as
    // runtimeNano(), so timer deadlines are coherent with Now()/Since/Sub.
    //
    // LOCKING DISCIPLINE. `s_timerLock` guards the heap AND every runtimeTimer field; nothing else
    // does. Go's finer-grained scheme (a per-timer lock plus a per-P heap lock) exists to scale
    // across Ps and to order timer-vs-heap acquisition; with a single heap there is exactly one
    // lock and therefore no lock-ordering hazard at all. No user code and no timer callback ever
    // runs under it — the service thread collects the due callbacks inside the lock and invokes
    // them after releasing it — so a callback cannot deadlock against a concurrent Stop/Reset.
    // A Stop or Reset that races the firing callback cannot double-fire or lose a re-arm: both the
    // firing decision and the cancellation are `when`/`gen` transitions made under this one lock,
    // and the loser of the race is detected by the generation check (see runtimeTimer.gen).
    //
    // ⚠ There IS a second lock now — a per-timer `sendLock`, for chan timers only (below) — and with
    // it one ordering rule, which holds today by construction and must keep holding:
    //
    //     sendLock is OUTERMOST. sendLock -> s_timerLock, and sendLock -> the channel's own lock.
    //
    // stop/modify take sendLock first and then s_timerLock; the service thread takes sendLock only
    // AFTER releasing s_timerLock; the send itself and the drain take the channel lock under
    // sendLock; and nothing in golib's channel calls out while holding the channel lock. So no cycle
    // exists. The way to introduce one is to call Timer.Stop/Reset from inside something that
    // already holds a channel lock — a callback invoked under it — which nothing does and nothing
    // should.
    //
    // SYNCHRONOUS TIMER CHANNELS (Go 1.23, proposal #37196) — the property, and the two mechanisms
    // that keep it. The name describes a guarantee, not a plumbing change: the channel is STILL
    // created buffered (`make(chan Time, 1)`, so a tick can be produced with nobody receiving), and
    // what Go 1.23 added is that its owner may take a tick BACK. The whole of it is one sentence:
    //
    //     A Stop or Reset prevents any tick generated before the call from being received after it.
    //
    // Equivalently: every value a receive on t.C observes was committed AFTER the most recent
    // Stop/Reset — there are no stale values, and the pre-1.23 "drain t.C when Stop returns false"
    // idiom is unnecessary. Two consequences the tests read directly: Stop/Reset report `pending`
    // true when they REVOKE an unreceived tick, not only when the timer was still armed; and
    // len(t.C)/cap(t.C) are 0, because a buffered value the next Stop may confiscate must not be
    // visible (golib's IChannelTimer.HidesBuffer masks both — the buffer itself is untouched).
    //
    // The guarantee needs three mechanisms. Two are Go's, at the same two layers; the third exists
    // because this model fires eagerly where Go fires lazily, and so reaches a state Go never does.
    //
    //  1. SEND LOCK + SEQUENCE (this file — Go's timer.sendLock / timer.seq). A firing is decided
    //     under s_timerLock but must be delivered with it released, so the decision and the send are
    //     separated in time and a Stop/Reset can land between them. A service pass therefore only
    //     OFFERS a tick: it captures `seq` when it commits the firing, then takes that timer's
    //     sendLock and re-checks it — a mismatch means a Stop/Reset intervened and the offer is
    //     ABANDONED. Stop/Reset bump `seq` while holding sendLock, so every offer is either wholly
    //     before them (its send completed — see the direct-hand-off note below) or wholly after
    //     (abandoned); it cannot straddle. `seq` is deliberately NOT `gen`: `gen` is bumped by a
    //     firing as well, which is what makes it the heap's liveness token, and a delivery check
    //     has to survive its own firing.
    //  2. BUFFER DRAIN (golib channel<T>.DrainBuffer — Go's runtime.timerchandrain). The seq check
    //     stops offers not yet sent; a tick ALREADY in the buffer is revoked by emptying it. Both
    //     Stop and Reset drain, and either reports `pending` true when it discarded something.
    //  3. THE OFFERED FLAG (runtimeTimer.offered — no Go counterpart, and the reason is below).
    //     Mechanisms 1 and 2 revoke correctly but cannot, between them, ANSWER correctly: in the
    //     window mechanism 1 exists to cover, the tick is in neither place a Stop looks — `when` is
    //     already 0 and the buffer is still empty — so Stop would revoke the tick and then report
    //     that there had been none. `offered` is that third state, made explicit.
    //
    // Together those cover every place a tick can be: scheduled, committed-but-unsent, or buffered.
    // A tick handed DIRECTLY to a parked receiver is none of them — but that receive already
    // committed, and it committed while the sender held sendLock, hence strictly before the Stop
    // waiting on that lock could return. The guarantee is about ticks received AFTER the call, so a
    // direct hand-off falls inside it rather than being an exception to it.
    //
    // WHERE THIS DELIBERATELY DIFFERS FROM GO, and why each difference is sound here:
    //  • Go drains in Stop AFTER releasing sendLock and in Reset BEFORE releasing it; this holds
    //    sendLock across the drain in BOTH. Go can afford the looser order because a sync-mode chan
    //    timer sits in a heap only while a receiver is blocked on it (timer.needsAdd requires
    //    t.blocked > 0), so between Stop's unlock and its drain there is no machinery that could
    //    produce a tick. This model's service thread is always live and always eager, so that
    //    protection does not exist, and a Reset racing another goroutine's Stop could have its fresh
    //    tick drained by the Stop. Under one lock the seq bump and the drain are a single step, which
    //    makes the guarantee a property of one critical section rather than an argument about a
    //    window.
    //  • Go fires a sync-mode chan timer LAZILY (runtime.maybeRunChan, at receive time); this fires
    //    eagerly from the service heap, as in async mode. Eager firing is NOT invisible through the
    //    guarantee, and an earlier draft of this comment claimed it was — an adversarial review
    //    measured the claim false. Go's `pending` can rest on `when > 0` alone because a sync-mode
    //    chan timer nobody is receiving from is never in a heap (timer.needsAdd requires
    //    t.blocked > 0) and therefore never fires at all; ours fires, clears `when` at commit, and
    //    fills the buffer only later, so between those two points BOTH of Go's indicators read
    //    "nothing pending" while a tick is very much in flight. Measured on the batch shape the
    //    SyncTimerChannel guard now runs: hundreds of one-shots per run answered `false` where Go
    //    answers `true` for every one — and each of those also destroyed its tick, the seq bump
    //    doing its job. Mechanism 3 is what closes it; WITH it, Stop's answer is the same either
    //    way. What eager firing still costs is Go 1.23's early GC of unreferenced timers: an armed
    //    timer stays reachable from the service heap. That is pre-1.23 behavior, unchanged by this
    //    work and unrelated to #37196.
    //
    // GODEBUG=asynctimerchan selects the model, exactly as upstream: 0 (the default) synchronous;
    // 1 pre-1.23 asynchronous, where package time's own syncTimer withholds the channel so no timer
    // channel is ever registered; 2 asynchronous semantics over a registered channel. Modes 1 and 2
    // run the model that predates this work, unchanged — every branch above is gated on
    // asyncTimerChan(), which reads the setting LIVE at each decision point exactly as the runtime
    // reads debug.asynctimerchan.
    //
    // ONE FIRING PER TIMER PER PASS — why the pass reads the clock exactly once (r39, 2026-08-03).
    // The service pass samples `now` ONCE and threads that one value through the whole drain. That
    // is not an optimization: it is the invariant. Go does the same and for the same reason — the
    // scheduler samples the clock in timers.check and hands that value down through timers.run(now)
    // to timer.unlockAndRun(now), never re-reading it inside a pass — and the consequence is a
    // theorem rather than a heuristic:
    //
    //     Within one service pass, every timer fires AT MOST ONCE.
    //
    // Two facts carry it. First, AT MOST ONE HEAP ENTRY PER TIMER IS EVER LIVE: every Enqueue in
    // this file is immediately preceded by a `gen++` on the same timer and queues the POST-increment
    // value, and `gen` only ever increases — so each generation is consumed by at most one entry and
    // every other entry for that timer fails the `entry.timer.gen != entry.gen` test and is dropped.
    // (Keep that pairing intact: moving the `gen++` in the drain below into the periodic branch
    // would silently break this.) A one-shot therefore cannot be re-peeked because it is dequeued
    // and NOT re-enqueued, not because `when` is cleared — the drain reads the heap's priority, not
    // `timer.when`.
    //
    // Second, the arithmetic. A periodic timer is re-armed to next = when + period*(1 + delay/period)
    // with delay = now - when >= 0; writing delay = q*period + r for 0 <= r < period gives
    // next = when + period*(1 + q) = (when + delay) + (period - r) = now + (period - r), and
    // r < period makes that STRICTLY greater than `now`. The re-peek therefore always takes the
    // `when > now` branch and the drain ends. It holds for every period, including the 1 ns one
    // testTimerChan resets to. Overflow cannot defeat it: the arithmetic is unchecked, the true
    // value is always in [1, 2^64-2), so a wrapped result is always NEGATIVE — never a small
    // positive that could land at or below `now` — and the `next < 0` clamp catches exactly that.
    //
    // Re-reading the clock per iteration broke that theorem and was the defect this replaced (r36
    // recorded it): the advanced `when` lands one nanosecond ahead, a freshly read `now` has already
    // passed it, and the same ticker fires again — for as long as consecutive reads of the ~100 ns
    // monotonic source keep advancing. The burst is invisible while nobody is receiving (sendTime's
    // non-blocking send onto the cap-1 channel drops all but one) but testTimerChan's drainAsync IS
    // receiving, so the two stale values an async ticker is allowed became three or more and `noTick`
    // reported "extra tick" — in ALL THREE asynctimerchan modes, which the divergence above (scoped
    // to the sync mode) never explained.
    //
    // What the invariant does NOT do is rate-limit, and it cannot delay anything. `next` depends on
    // `now` only through the floor delay/period, which is non-decreasing in `now`, so hoisting the
    // read can only make `next` — and therefore the pass's `deadline` — SMALLER or equal. waitUntil
    // recomputes `remaining` from a FRESH clock, and the OS wake is monotone in the deadline, so no
    // timer can wake later than the per-iteration version would have woken it; usually the deadline
    // is already past and waitUntil returns at once. (When delay < period, the common case, the floor
    // is 0 and the deadline is bit-for-bit identical either way.) A high-rate ticker is thus served
    // every pass, as in Go, where the scheduler simply calls check again with a fresh `now`. The
    // bound is on re-firing WITHIN a pass, which is Go's "drop ticks to make up for slow receivers".
    //
    // A timer coming due DURING a pass waits for the next one, and that is not a delay either: the
    // head of the heap is the minimum `when` among live entries, so `deadline` is <= that timer's
    // `when` <= the clock at the end of the drain, hence `remaining <= 0` and the next pass starts
    // immediately. Go behaves the same way.
    //
    // Where this model is NOT Go, so the fidelity claim above is not over-read: Go's `check` releases
    // the timer-set lock around EACH callback and re-validates the head between them, so a Stop
    // landing mid-pass cancels the callbacks after it; this drain commits the whole batch under one
    // lock hold and then runs it. (In SYNC mode the seq check restores that cancellation for chan
    // timers: a Stop or Reset landing after the batch was committed abandons every offer in it. In
    // async mode it does not, which is the pre-1.23 behavior the mode selects.) Go also keeps one
    // heap entry per TIMER (repositioned in place, zombies swept), where this keeps one per ARM and
    // reclaims a dead entry only when it reaches the head. Both predate the single clock sample and
    // are narrowed, not widened, by it — a frozen `now` commits a SMALLER batch. One consequence to
    // know: `delay` is now measured from the pass's clock, so sendTime's `Now().Add(-delay)` lands
    // slightly later than a per-iteration reading would place it — the same form Go uses, with a
    // larger magnitude because callbacks here are deferred to the end of the batch.
    //
    // Finally, a CONSTRAINT this establishes rather than merely satisfies. At the instant a ticker is
    // stopped or reset, at most TWO of its ticks can exist: one in the cap-1 buffer and one committed
    // to `due` but not yet sent. In SYNC mode both are now revoked outright (the drain takes the
    // first, the seq check the second), so the guarantee no longer rests on the count. In ASYNC mode
    // it still does, and there the two is exactly the allowance testTimerChan's drainAsync drains for
    // a ticker — the margin is zero. Any later change that lets two ticks for ONE timer be committed
    // before their callbacks run — batching two passes, a second service thread, moving the `due`
    // flush inside the lock-held drain — re-breaks `noTick` under asynctimerchan=1 and =2 without
    // touching a line of this file.

    // Hidden runtime state for one Timer or Ticker. Go declares these fields on runtime.timeTimer,
    // AFTER the two fields package time can see (`C` and `initTimer`/`initTicker`); the comment in
    // sleep.go — "there are extra fields after the channel, reserved for the runtime and
    // inaccessible to users" — is describing exactly this record.
    private sealed class runtimeTimer : IChannelTimer
    {
        // Absolute deadline on the runtimeNano() clock, or 0 for NOT ARMED — stopped, or a one-shot
        // that already fired. Go marks that state the same way (`t.when = 0` in timer.stop and in
        // unlockAndRun's one-shot path), and it is the value both stopTimer and resetTimer report as
        // their "was still pending" answer.
        internal int64 when;

        // Repeat interval: 0 for a one-shot Timer, > 0 for a Ticker.
        internal int64 period;

        // The callback package time installed — `sendTime` for a chan-based Timer/Ticker, `goFunc`
        // for AfterFunc.
        internal Action<any, uintptr, int64> f = null!;

        // Callback argument — the channel<Time> for sendTime, the Action for goFunc.
        internal any? arg;

        // Arm generation, bumped on EVERY change to `when` (arm, stop, reset, ticker re-arm). Heap
        // entries carry the generation they were queued with, which is how a Stop or Reset cancels
        // an already-queued firing without removing it from the heap: the service thread drops any
        // entry whose generation no longer matches. This is the managed stand-in for Go's
        // timerModified/timerZombie bits, which likewise leave the stale heap entry in place.
        internal int64 gen;

        // The timer's channel when this is a CHANNEL timer whose channel was handed to the runtime
        // — Go's t.isChan plus t.hchan(). The NIL channel otherwise: for AfterFunc's func timer,
        // which has no channel at all, and for a Timer/Ticker created under
        // GODEBUG=asynctimerchan=1, where package time's syncTimer deliberately withholds it.
        internal channel<Time> c;

        // Go's timer.sendLock, held across the actual send AND across the seq bump in stop/modify,
        // so a firing decided before a Stop/Reset cannot complete after it. Non-null exactly when
        // `c` is non-nil, which is also what makes it the isChan flag (see IsChan).
        internal object? sendLock;

        // Go's timer.seq — the stale-send sequence. Bumped by stop and modify only, NEVER by a
        // firing (that is `gen`'s job), and captured when a firing is committed to the due batch; a
        // mismatch when the send is about to happen means a Stop or Reset landed in between, and
        // the offer is abandoned. See SYNCHRONOUS TIMER CHANNELS above.
        internal int64 seq;

        // COMMITTED-BUT-UNSENT — the third place a tick can be, and the one Go never has.
        //
        // A synchronous Stop/Reset has to answer "was a tick pending?" by looking where ticks live:
        // `when > 0` says one is still scheduled, and a non-empty buffer says one was delivered but
        // not received. Between the service pass committing a firing and the send completing there
        // is a moment when NEITHER is true — the pass has already cleared `when` (a one-shot) and
        // the send has not happened — and a Stop landing there would revoke the tick (its seq bump
        // makes the send abandon) while reporting `pending == false`. Measured before this flag
        // existed: 26 to 132 of 500 one-shots per run, where Go reports 500 of 500.
        //
        // Go has no such state because a sync-mode chan timer is only in a heap while a receiver is
        // blocked on it (timer.needsAdd requires t.blocked > 0), so an unreceived timer never fires
        // at all and `when > 0` still answers. Eager firing is what opens the window, so eager
        // firing is what has to close it: `offered` records that a firing is in flight, and
        // stop/modify count it as pending exactly as they count a drained buffer.
        //
        // Set under s_timerLock where the firing is committed, cleared under sendLock where it
        // resolves, and read-and-cleared by stop/modify, which hold BOTH — so every access is
        // ordered against both writers. At most one offer per timer can be outstanding: the service
        // thread is single and drains the whole batch before the next pass, and at most one heap
        // entry per timer is ever live.
        internal bool offered;

        // Go's t.isChan. A channel timer is one whose channel package time handed to the runtime;
        // sendLock is created with it and nowhere else, so the two are the same question.
        internal bool IsChan => sendLock is not null;

        // golib asks this on every cap()/len() of the channel: a SYNCHRONOUS timer channel presents
        // as unbuffered, an asynchronous one (asynctimerchan=2 — mode 1 never registers a channel
        // at all) reports its real buffer. Read live, exactly as runtime.chanlen/chancap do.
        bool IChannelTimer.HidesBuffer => !asyncTimerChan();
    }

    // Go's `async := debug.asynctimerchan.Load() != 0` — the live GODEBUG selector between Go
    // 1.23's SYNCHRONOUS timer channel (0, the default) and the pre-1.23 asynchronous one (1, and 2
    // which keeps the channel registered but takes the async paths). Package time's own syncTimer
    // has already resolved mode 1 at construction by withholding the channel; this is what
    // distinguishes 0 from 2 at every decision point thereafter. The value is PARSED, not compared
    // as a string, because that is what the runtime does — see godebugInt.
    //
    // COST, stated honestly: Go's counterpart is one atomic int load, and this is a $GODEBUG
    // environment read plus two lookups (godebug.Value re-reads the variable every call BY DESIGN —
    // the raw text is its cache-invalidation token, since there is no Setenv notification hook),
    // measured at ~670 ns. Three orders of magnitude more expensive, so it is kept OFF the per-timer
    // path: the service pass reads it once and threads the answer through the whole batch, and the
    // remaining callers are Stop/Reset and len()/cap() of a timer channel — all per user operation.
    // A func timer pays NOTHING, since syncSendLock tests IsChan first. Do not "optimize" this by
    // caching the result across calls: the live-mode contract is the thing it exists to honor.
    private static bool asyncTimerChan()
    {
        return godebugInt(asynctimerchan.Value()) != 0;
    }

    // Package time's own syncTimer test, hoisted so BOTH constructors ask it the same way: it
    // withholds the channel from the runtime — selecting the pre-1.23 asynchronous model outright —
    // only for the exact value "1", which is how Go spells it (`asynctimerchan.Value() == "1"`).
    // Modes 0 and 2 both register the channel; asyncTimerChan then separates them.
    private static bool syncTimerChanEnabled()
    {
        return asynctimerchan.Value() != "1"u8;
    }

    // Go's runtime.atoi32, which is how a GODEBUG value becomes the int the runtime compares
    // against 0: an optional leading '-' followed by at least one decimal digit, and NOTHING else.
    // parsegodebug stores the result only when that parse SUCCEEDS, so every unparseable value
    // leaves the setting at its default 0 — "00" is 0, and so are "x", "0 " and "+0". A string
    // compare against "0" would call all four of those asynchronous where Go calls them
    // synchronous (measured against `go run`; garbage input only, but free to get right).
    private static int32 godebugInt(@string value)
    {
        int start = value.Length > 0 && value[0] == (byte)'-' ? 1 : 0;

        if (value.Length == start)
        {
            return 0;
        }

        // atoi32 keeps only what round-trips through int32, so the negative side reaches one
        // further than the positive one. Spelling the bound rather than reusing int32.MaxValue is
        // what makes "-2147483648" async here as it is in Go.
        int64 limit = start == 1 ? -(int64)int32.MinValue : int32.MaxValue;
        int64 magnitude = 0;

        for (int i = start; i < value.Length; i++)
        {
            byte digit = value[i];

            if (digit < (byte)'0' || digit > (byte)'9')
            {
                return 0;
            }

            magnitude = magnitude * 10 + (digit - (byte)'0');

            if (magnitude > limit)
            {
                // atoi rejects an overflowing magnitude and atoi32 rejects anything outside int32 —
                // either way parsegodebug never stores it and the setting keeps its default.
                return 0;
            }
        }

        return (int32)(start == 1 ? -magnitude : magnitude);
    }

    // The timer's sendLock when the SYNCHRONOUS timer-channel protocol applies right now — a
    // channel timer, in a mode that wants the Go 1.23 semantics — and null when it does not, which
    // is every func timer and every timer under asynctimerchan=1 or =2. One helper so stop and
    // modify cannot drift apart. (The service pass asks the same question, but resolves the mode
    // ONCE per pass rather than per timer — see `sync` in serviceTimers.)
    private static object? syncSendLock(runtimeTimer timer)
    {
        return timer.IsChan && !asyncTimerChan() ? timer.sendLock : null;
    }

    // Timer/Ticker box → hidden runtime state. Go's runtime allocates the timeTimer and thus owns
    // the storage for both halves; here the visible half is the ж<T> box the converted code holds
    // and the hidden half is looked up by that box's REFERENCE IDENTITY — stopTimer/resetTimer are
    // always handed the very box newTimer returned. Weak-keyed so an unreferenced Timer stays
    // collectible (Go 1.23 recovers unreferenced timers); an ARMED timer's state is independently
    // kept alive by the service heap, which is what lets a bare `time.After(d)` — whose Timer is
    // dropped on the spot, only the channel kept — still fire.
    private static readonly ConditionalWeakTable<object, runtimeTimer> s_timerState = new();

    // The single lock (see LOCKING DISCIPLINE above), the deadline-ordered heap, and the handle the
    // service thread waits on alongside its high-resolution timer so a newly-armed earlier deadline
    // interrupts an in-progress wait.
    private static readonly object s_timerLock = new();
    private static readonly PriorityQueue<(runtimeTimer timer, int64 gen), int64> s_timerHeap = new();
    private static readonly AutoResetEvent s_timerWake = new(false);
    private static Thread? s_timerThread;

    // Sleep pauses the calling goroutine for at least the duration d. Mirrors runtime.timeSleep:
    // a non-positive duration returns immediately, and the deadline is computed on the monotonic
    // clock with Go's overflow guard.
    public static partial void Sleep(Duration d)
    {
        if (d <= 0)
        {
            return;
        }

        int64 deadline = runtimeNano() + (int64)d;

        if (deadline < 0)
        {
            // N.B. runtimeNano() and d are both positive, so this is overflow — runtime.timeSleep
            // clamps to maxWhen identically.
            deadline = int64.MaxValue;
        }

        // Parked ONCE for the whole sleep, not once per waitUntil retry — Go's timeSleep parks once
        // with waitReasonSleep and the retries are below the park, not around it. Deliberately here
        // rather than inside waitUntil, whose OTHER caller is the timer service thread: that thread
        // is not a goroutine (Park would be inert there anyway), and a sleep is a goroutine-level
        // wait while the service loop's wait is the engine's own.
        using (Goroutine.Park(WaitReason.Sleep))
            waitUntil(deadline, null);
    }

    // newTimer allocates a Timer (or, through the hand-owned tick.cs, a Ticker — same runtime
    // state) and arms it. Mirrors runtime.newTimer: build the state, modify(when, period, f, arg),
    // then mark it initialized.
    internal static partial ж<Timer> newTimer(int64 when, int64 period, Action<any, uintptr, int64> f, any arg, @unsafe.Pointer cp)
    {
        // cp is Go's marker for a SYNCHRONOUS timer channel: package time's syncTimer hands the
        // runtime the channel for asynctimerchan=0 and =2 and nil for =1, so its NIL-NESS — one
        // bit — is the entire signal, and its value is never dereferenced even in Go.
        //
        // ⚠ That bit is NOT read from `cp` here, and the reason is worth stating rather than
        // discovering. `cp` arrives from converted sleep.cs as
        // `(uintptr)(~Ꮡc.Reinterpret<channel<Time>, unsafe.Pointer>())`, and `unsafe.Pointer` is a
        // CLASS, so the reinterpret cannot alias storage: it produces a box holding a punned read
        // OF the channel struct's contents (measured: the same value for three distinct channels,
        // a module-range address unrelated to any of them), and the uintptr bridge then reads
        // ж<uintptr>'s fields at offsets that belong to ChanCore<Time>. It happens to be non-zero
        // today. That is a property of two type layouts, not a contract, and THIS change added a
        // field to ChanCore — so a future field add or reorder could flip the bit silently, with no
        // compile error and no crash, dropping NewTimer to the asynchronous model while NewTicker,
        // which never goes through a pointer, stayed synchronous.
        //
        // So the bit is recomputed from where syncTimer itself computes it — the setting — and the
        // CHANNEL comes from `arg`, which is the same channel Go's t.hchan() recovers from t.arg.
        // Both constructors then agree by construction. A func timer (AfterFunc) passes an Action
        // for arg and is never a channel timer.
        _ = cp;

        channel<Time> syncChan = syncTimerChanEnabled() && arg is channel<Time> c ? c : default;

        ж<Timer> box = new StandardBox<Timer>(new Timer{ initTimer = true });
        armNewTimer(box, when, period, f, arg, syncChan);
        return box;
    }

    // stopTimer stops a timer, reporting whether it was still pending. Mirrors runtime.timer.stop:
    // `pending = t.when > 0`, then clear `when`.
    internal static partial bool stopTimer(ж<Timer> _)
    {
        return stopRuntimeTimer(_);
    }

    // resetTimer re-arms a timer, reporting whether it was still pending beforehand. Mirrors
    // runtime.timer.reset → timer.modify with a nil f (which leaves f/arg untouched).
    internal static partial bool resetTimer(ж<Timer> t, int64 when, int64 period)
    {
        return resetRuntimeTimer(t, when, period);
    }

    // Installs the hidden state for a freshly allocated Timer/Ticker box and arms it. Shared with
    // the hand-owned Ticker constructor in tick.cs (Go's runtime.newTimer serves both, since
    // "Ticker and Timer have the same layout").
    /// <param name="syncChan">
    /// The timer's channel when package time handed it to the runtime (Go's <c>syncTimer(c)</c>
    /// result being non-nil); the NIL channel for a func timer and under
    /// <c>GODEBUG=asynctimerchan=1</c>.
    /// </param>
    internal static void armNewTimer(object box, int64 when, int64 period, Action<any, uintptr, int64> f, any? arg, channel<Time> syncChan = default)
    {
        runtimeTimer timer = new()
        {
            f = f,
            arg = arg
        };

        if (syncChan != nil)
        {
            // runtime.newTimer's order, and it matters: the channel learns its owner BEFORE the
            // timer is armed, so no firing can reach a channel that cannot yet mask its buffer or
            // be drained. (Go likewise sets c.timer and t.isChan before calling t.modify.)
            timer.c = syncChan;
            timer.sendLock = new object();
            syncChan.AttachTimer(timer);
        }

        s_timerState.Add(box, timer);
        modifyRuntimeTimer(timer, when, period);
    }

    // Stops the timer owned by the given Timer/Ticker box, reporting whether it was still pending
    // (armed and not yet fired). Shared with tick.cs — runtime.stopTimer serves both.
    internal static bool stopRuntimeTimer(object box)
    {
        runtimeTimer timer = runtimeTimerOf(box);
        object? sendLock = syncSendLock(timer);

        if (sendLock is null)
        {
            return stopLocked(timer, bumpSeq: false);
        }

        // Synchronous timer channel (runtime.timer.stop's !async && isChan path). Everything that
        // revokes a tick happens inside ONE hold of sendLock: the seq bump abandons any offer the
        // service pass has committed but not yet sent, and the drain discards one already in the
        // buffer. Stop reports `pending` if EITHER the timer was still armed or a tick was revoked
        // — which is what makes "if the program has not received from t.C already and the timer is
        // running, Stop is guaranteed to return true" hold once the timer has fired.
        lock (sendLock)
        {
            bool pending = stopLocked(timer, bumpSeq: true);

            // Evaluated into its own local: `pending || Drain()` would SKIP the drain whenever the
            // timer was still armed, leaving exactly the stale tick this exists to remove.
            bool revoked = timer.c.DrainBuffer();
            return pending || revoked;
        }
    }

    // The heap-side half of a stop: clear `when` and cancel any queued firing. Caller holds
    // sendLock when `bumpSeq` is set (Go bumps t.seq under both t.mu and t.sendLock).
    private static bool stopLocked(runtimeTimer timer, bool bumpSeq)
    {
        lock (s_timerLock)
        {
            // `offered` is the committed-but-unsent tick this Stop is about to revoke by bumping
            // seq; counting it is what keeps `pending` truthful in that window (see
            // runtimeTimer.offered). Only the synchronous protocol sets it, so the async path
            // reads a field that is always false.
            bool pending = timer.when > 0 || timer.offered;
            timer.offered = false;
            timer.when = 0;
            // Cancels any firing already queued for this timer (see runtimeTimer.gen). Doing it
            // under the same lock the service thread validates entries under is what makes the
            // stop-vs-fire race safe in both directions: either the service thread has already
            // taken the callback (and `when` is 0, so `offered` reports it instead), or this
            // generation bump invalidates its queued entry and the callback never runs.
            timer.gen++;

            if (bumpSeq)
            {
                timer.seq++;
            }

            return pending;
        }
    }

    // Re-arms the timer owned by the given box, reporting whether it was pending beforehand.
    // Shared with tick.cs — runtime.resetTimer serves both.
    internal static bool resetRuntimeTimer(object box, int64 when, int64 period)
    {
        return modifyRuntimeTimer(runtimeTimerOf(box), when, period);
    }

    // runtime.timer.modify: set the period and the new deadline, reporting whether the timer was
    // still pending. A Reset of an already-fired or stopped timer therefore returns false while
    // re-arming it, which is precisely what Timer.Reset documents.
    private static bool modifyRuntimeTimer(runtimeTimer timer, int64 when, int64 period)
    {
        if (when <= 0)
        {
            throw panic("timer when must be positive");
        }

        if (period < 0)
        {
            throw panic("timer period must be non-negative");
        }

        object? sendLock = syncSendLock(timer);

        if (sendLock is null)
        {
            return modifyLocked(timer, when, period, bumpSeq: false);
        }

        // Synchronous timer channel (runtime.timer.modify's !async && isChan path) — the same one
        // critical section as stop above, and here the lock is load-bearing in a second way: the
        // re-arm can make the timer due IMMEDIATELY, so without holding sendLock across the drain a
        // fresh, legitimate tick could be produced and then discarded by this very drain.
        lock (sendLock)
        {
            bool pending = modifyLocked(timer, when, period, bumpSeq: true);
            bool revoked = timer.c.DrainBuffer();
            return pending || revoked;
        }
    }

    // The heap-side half of a modify: set the period and re-arm. Caller holds sendLock when
    // `bumpSeq` is set (Go bumps t.seq under both t.mu and t.sendLock).
    private static bool modifyLocked(runtimeTimer timer, int64 when, int64 period, bool bumpSeq)
    {
        lock (s_timerLock)
        {
            // Same third state as stop's — a firing committed but not yet sent, which this Reset
            // revokes via the seq bump below and must therefore report as pending.
            bool pending = timer.when > 0 || timer.offered;
            timer.offered = false;
            timer.period = period;
            armLocked(timer, when);

            if (bumpSeq)
            {
                timer.seq++;
            }

            return pending;
        }
    }

    // Queues the timer for `when` and wakes the service thread. Caller holds s_timerLock.
    private static void armLocked(runtimeTimer timer, int64 when)
    {
        timer.when = when;
        timer.gen++;
        s_timerHeap.Enqueue((timer, timer.gen), when);

        if (s_timerThread is null)
        {
            // Started on first use, not at package init: a program that never touches a timer never
            // pays for the thread. Background, so it never keeps the process alive — Go's timer
            // service does not either.
            s_timerThread = new Thread(serviceTimers)
            {
                IsBackground = true,
                Name = "go2cs.time.timers"
            };

            s_timerThread.Start();
        }

        // An AutoResetEvent latches, so this cannot be a lost wakeup even though the service thread
        // computes its deadline and starts waiting outside the lock: a Set that lands in that window
        // makes its next wait return at once and re-derive the head of the heap.
        s_timerWake.Set();
    }

    // Resolves a Timer/Ticker box to its hidden runtime state.
    private static runtimeTimer runtimeTimerOf(object box)
    {
        if (box is not null && s_timerState.TryGetValue(box, out runtimeTimer? timer))
        {
            return timer;
        }

        // Where Go reaches its own "timer used without initialization" throw. Package time already
        // guards ordinary misuse ahead of this (Timer.Stop/Reset panic on !initTimer), so this is
        // reachable only by operating on a COPY of a Timer/Ticker value — which does not carry the
        // hidden runtime state, exactly as in Go, where the copy loses the runtime fields that
        // follow the two visible ones.
        throw panic("time: timer used without initialization");
    }

    // The service thread: run every due timer in deadline order, then wait for the next deadline
    // (or for an arm to interrupt the wait). Mirrors the runtime's timers.run/timer.unlockAndRun.
    private static void serviceTimers()
    {
        List<(runtimeTimer timer, Action<any, uintptr, int64> f, any? arg, int64 delay, int64 seq)> due = new();

        while (true)
        {
            // 0 == nothing armed; wait for an arm instead of a deadline.
            int64 deadline = 0;
            due.Clear();

            // Sampled ONCE for the whole pass, for the same reason `now` is: the commit below and
            // the sends after it are two halves of ONE batched unlockAndRun, and they must agree
            // about which model is in force or a firing could be marked as an offer and then
            // delivered without the protocol that makes an offer revocable (or the reverse). Go
            // reads the setting per unlockAndRun, i.e. per firing, which is the same granularity
            // when a pass carries one timer and a strictly safer one when it carries several. It
            // also keeps the read off the per-timer path — see the cost note on asyncTimerChan.
            bool sync = !asyncTimerChan();

            lock (s_timerLock)
            {
                // Sampled ONCE for the whole drain and never re-read inside it — the pass's clock,
                // exactly as Go's timers.check samples `now` and passes it down through run and
                // unlockAndRun. This is what bounds a periodic timer to one firing per pass; see
                // ONE FIRING PER TIMER PER PASS above for the proof and for why re-reading it here
                // let a 1 ns ticker burst.
                int64 now = runtimeNano();

                while (s_timerHeap.TryPeek(out (runtimeTimer timer, int64 gen) entry, out int64 when))
                {
                    if (entry.timer.gen != entry.gen)
                    {
                        // Stopped, reset or re-armed after this entry was queued — drop it; the live
                        // entry, if any, is elsewhere in the heap (see runtimeTimer.gen).
                        s_timerHeap.Dequeue();
                        continue;
                    }

                    if (when > now)
                    {
                        deadline = when;
                        break;
                    }

                    s_timerHeap.Dequeue();

                    // runtime.timer.unlockAndRun: capture the callback, advance or clear `when`, and
                    // run the callback only after the lock is released. `delay` is how LATE the
                    // firing is (Go's `delay := now - t.when`, always >= 0); sendTime subtracts it
                    // to send the time the tick was scheduled for rather than the time it ran.
                    // `seq` is captured HERE, with the firing decision, exactly as Go's
                    // unlockAndRun copies t.seq while still holding t.mu — it is what the send
                    // below re-checks to discover a Stop/Reset that landed in between.
                    int64 delay = now - when;
                    due.Add((entry.timer, entry.timer.f, entry.timer.arg, delay, entry.timer.seq));
                    entry.timer.gen++;

                    // From here until the send resolves, this timer has a tick that exists but is
                    // in NEITHER place a Stop can look — `when` is about to be cleared and the
                    // buffer is still empty. `offered` is that third state, and recording it is
                    // what lets Stop/Reset answer `pending` correctly for a firing they are about
                    // to revoke. See COMMITTED-BUT-UNSENT on runtimeTimer.offered.
                    entry.timer.offered = sync && entry.timer.IsChan;

                    if (entry.timer.period > 0)
                    {
                        // Advance by WHOLE periods past a late firing, so the tick phase stays
                        // aligned to the original schedule instead of drifting — Go's documented
                        // "adjust the time interval or drop ticks to make up for slow receivers"
                        // (unlockAndRun: next = when + period*(1 + delay/period)). Combined with
                        // sendTime's non-blocking send onto the cap-1 channel, a receiver too slow
                        // to keep up therefore LOSES ticks rather than seeing them queue up or the
                        // ticker fall behind.
                        int64 next = when + entry.timer.period * (1 + delay / entry.timer.period);

                        if (next < 0)
                        {
                            next = int64.MaxValue;
                        }

                        entry.timer.when = next;
                        s_timerHeap.Enqueue((entry.timer, entry.timer.gen), next);
                    }
                    else
                    {
                        entry.timer.when = 0;
                    }
                }
            }

            foreach ((runtimeTimer timer, Action<any, uintptr, int64> f, any? arg, int64 delay, int64 seq) in due)
            {
                if (!sync || !timer.IsChan)
                {
                    // A func timer, or an asynchronous timer channel: the firing decision IS the
                    // delivery, with nothing able to take it back. (Neither sendTime nor goFunc
                    // reads the seq parameter; it is passed for signature fidelity.)
                    f(arg!, (uintptr)seq, delay);
                    continue;
                }

                // runtime.timer.unlockAndRun's send-lock protocol: this firing was decided under
                // s_timerLock, which is long since released, so it is only an OFFER. Take the send
                // lock — which also blocks a Stop/Reset from starting mid-send — and re-check the
                // sequence. A mismatch means one landed since the decision, and the offer is
                // abandoned; Go expresses the same thing by replacing f with a no-op.
                lock (timer.sendLock!)
                {
                    // Resolved either way, and cleared BEFORE the send so no window exists in which
                    // the value is both in the buffer and still claimed as an outstanding offer —
                    // that would make a Stop report `pending` for one tick twice over. Both writers
                    // of this field hold a lock stop/modify hold: the set above is under
                    // s_timerLock, this clear is under sendLock, and stop/modify hold both.
                    bool live = timer.seq == seq;
                    timer.offered = false;

                    if (live)
                    {
                        f(arg!, (uintptr)seq, delay);
                    }
                }
            }

            if (deadline == 0)
            {
                s_timerWake.WaitOne();
            }
            else
            {
                waitUntil(deadline, s_timerWake);
            }
        }
    }

    // Blocks until the monotonic deadline, or until `interrupt` is signaled; returns true when
    // interrupted. Never returns early on the deadline path: Go guarantees a sleep of AT LEAST the
    // requested duration, so a short OS wake re-waits the remainder.
    private static bool waitUntil(int64 deadline, WaitHandle? interrupt)
    {
        while (true)
        {
            int64 remaining = deadline - runtimeNano();

            if (remaining <= 0)
            {
                return false;
            }

            if (!t_highResTimerProbed)
            {
                // Cached PER THREAD, matching the Go runtime's per-M highResTimer. The handle is
                // released by its SafeWaitHandle finalizer once the thread — and with it this
                // thread-static — becomes unreachable.
                t_highResTimerProbed = true;
                t_highResTimer = highResTimer.TryCreate();
            }

            highResTimer? timer = t_highResTimer;

            if (timer is not null && timer.Arm(remaining))
            {
                if (interrupt is null)
                {
                    timer.WaitOne();
                }
                else if (WaitHandle.WaitAny([timer, interrupt]) == 1)
                {
                    return true;
                }

                continue;
            }

            // Fallback where no high-resolution timer exists: wait coarsely to within a millisecond
            // of the deadline, then spin out the remainder. Coarse waits are quantized to the OS
            // timer tick, so this path can OVERSHOOT a short deadline — which is exactly why the
            // high-resolution timer above is preferred.
            int64 millis = remaining / 1_000_000;

            if (millis > 1)
            {
                int coarse = (int)Math.Min(millis - 1, int.MaxValue);

                if (interrupt is null)
                {
                    Thread.Sleep(coarse);
                }
                else if (interrupt.WaitOne(coarse))
                {
                    return true;
                }
            }
            else
            {
                Thread.SpinWait(1000);
            }
        }
    }

    [ThreadStatic]
    private static highResTimer? t_highResTimer;

    [ThreadStatic]
    private static bool t_highResTimerProbed;

    // A Windows high-resolution waitable timer, wrapped as a WaitHandle so the managed wait
    // primitives block on it directly. This is the same OS object the Go runtime creates for its own
    // sub-tick sleeps (runtime/os_windows.go: CreateWaitableTimerExW with
    // CREATE_WAITABLE_TIMER_HIGH_RESOLUTION, Windows 10 1803+) and it is what keeps a 1 ms Go timer
    // from becoming a ~15 ms one.
    private sealed partial class highResTimer : WaitHandle
    {
        private const uint CreateHighResolution = 0x2;
        private const uint TimerAllAccess = 0x1F0003;

        private highResTimer(nint handle)
        {
            SafeWaitHandle = new SafeWaitHandle(handle, ownsHandle: true);
        }

        internal static highResTimer? TryCreate()
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            nint handle;

            try
            {
                handle = CreateWaitableTimerExW(0, null, CreateHighResolution, TimerAllAccess);
            }
            catch (Exception)
            {
                // No such export at all (a non-Windows host reached through some other runtime) —
                // fall back to the coarse wait rather than failing the sleep.
                return null;
            }

            // Pre-1803 Windows rejects the high-resolution flag (ERROR_INVALID_PARAMETER). A plain
            // waitable timer would be no more precise than the coarse fallback, so there is nothing
            // to gain by retrying without the flag.
            return handle == 0 ? null : new highResTimer(handle);
        }

        // Arms the timer to signal `nanos` from now. A NEGATIVE due time is a relative interval in
        // 100 ns units; the truncation can only wake the waiter early, by under 100 ns, which the
        // caller's deadline loop absorbs.
        internal bool Arm(int64 nanos)
        {
            long dueTime = -(nanos / 100);

            if (dueTime == 0)
            {
                dueTime = -1;
            }

            // The reference count the SafeWaitHandle parameter used to take implicitly — see the
            // declaration below for why it is explicit now. Without it a concurrent Dispose could
            // close the handle between the read and the kernel's use of it, and the timer would be
            // armed on whatever object Windows had since given the number to.
            bool addedRef = false;

            try
            {
                SafeWaitHandle.DangerousAddRef(ref addedRef);

                return SetWaitableTimer(SafeWaitHandle.DangerousGetHandle(), ref dueTime, 0, 0, 0, resume: false);
            }
            finally
            {
                if (addedRef)
                {
                    SafeWaitHandle.DangerousRelease();
                }
            }
        }

        // CharSet.Unicode becomes StringMarshalling.Utf16. `timerName` is always null here (Go's
        // runtime creates an unnamed timer), but the parameter stays a string because that is what
        // the API takes and the marshalling for it is a pin, not a copy.
        [LibraryImport("kernel32.dll", EntryPoint = "CreateWaitableTimerExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        private static partial nint CreateWaitableTimerExW(nint timerAttributes, string? timerName, uint flags, uint desiredAccess);

        // TWO rejections, and they are the two the migration was worth doing for.
        //
        //  1. SafeWaitHandle. The source generator does not marshal SafeHandles at all, because the
        //     reference counting that makes them safe is a RUNTIME marshalling service. [DllImport]
        //     supplied it invisibly, which reads as "SafeHandle, therefore safe" — but nothing in
        //     the source said where the ref count was taken, so nothing would have said it if the
        //     parameter had ever been changed to a raw handle. The declaration now takes the `nint`
        //     it always passed, and Arm above takes the ref count where a reader can see it.
        //
        //  2. `bool`, in both directions. Its native width is a marshalling decision, not a fact
        //     about the type: Win32 BOOL is a 4-byte int, C++ `bool` is one byte, and the runtime
        //     marshaller's silent default of 4 bytes happened to be right here. The generator
        //     refuses to guess. Both are now stated — UnmanagedType.Bool IS the 4-byte Win32 BOOL,
        //     so the ABI is unchanged and the choice is written down.
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetWaitableTimer(nint timer, ref long dueTime, int period, nint completionRoutine, nint completionArg, [MarshalAs(UnmanagedType.Bool)] bool resume);
    }
}
