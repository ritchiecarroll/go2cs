// time_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

namespace go;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
// Aliased rather than imported wholesale: this file needs exactly these golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using Goroutine = go.golib.Goroutine;
using GoTimer = go.golib.GoTimer;
using MonotonicClock = go.golib.MonotonicClock;
using WaitReason = go.golib.WaitReason;
using SyncTestBubble = go.golib.SyncTestBubble;
using @unsafe = unsafe_package;
// For godebug_package's extension methods on ж<Setting> — asyncTimerChan() reads the same
// asynctimerchan setting sleep.cs's syncTimer does.
using @internal;

partial class time_package
{
    // time's now(), runtimeNano() and — since Go 1.24 — runtimeNow() are //go:linkname'd into the
    // Go runtime (runtime.now / runtime.nanotime / runtime.time_runtimeNow), so the converter
    // emitted them as bodyless partials — throwing stubs. That made the package UNUSABLE at load:
    // the static initializer `startNano = runtimeNano() - 1`
    // (time.cs) runs runtimeNano() during the type's cctor, so merely IMPORTING time — or the first
    // touch of time.Now / Since / Sub — died with "runtimeNano: external (assembly or cgo) function
    // is not implemented" (surfaced by io/fs, crypto/subtle, and go/doc/comment, whose test-package
    // static init reaches time_package..cctor). These supply the equivalent managed bodies so the
    // clock RUNS. Living in an _impl.cs companion (never emitted by the converter) makes this durable
    // by construction: a -stdlib reconvert regenerates the pristine bodyless partials and these
    // bodies remain. The same applies to Go's runtime TIMERS (Sleep / newTimer / stopTimer /
    // resetTimer), realized in the region at the bottom of this file.

    // runtimeNano returns the current value of a monotonic clock in nanoseconds. Go's runtime.nanotime reads
    // a monotonic source (QueryPerformanceCounter on Windows); Stopwatch is exactly that source on .NET, read
    // through golib's MonotonicClock (realNano below), which scales the tick count to nanoseconds with the
    // seconds/remainder split so the result stays EXACT and monotonic without overflowing: `ticks * 1e9`
    // would overflow int64 for any real uptime, whereas `seconds * 1e9` (seconds = ticks/Frequency) and
    // `rem * 1e9 / Frequency` (rem < Frequency) each stay well inside int64 and together preserve full
    // sub-tick nanosecond resolution. The absolute epoch is arbitrary — only differences are observed
    // (startNano rebases it), and Now().mono / Sub / Since all read this same source, so the monotonic
    // component is coherent across the package.
    internal static partial int64 runtimeNano()
    {
        // Go's time_runtimeNano: a synctest bubble member reads its bubble's FAKE clock (S3).
        if (SyncTestBubble.Current is { } bubble)
            return bubble.Now;

        return realNano();
    }

    // runtimeIsBubbled reports whether the caller is in a synctest bubble. Go 1.26 pushes it from
    // runtime/time.go (`//go:linkname time_runtimeIsBubbled time.runtimeIsBubbled`, body
    // `getg().bubble != nil`), and Since and Until call it whenever t carries a monotonic reading, so
    // the stub it replaces threw on the first time.Since(start). Bubble membership is golib's, the same object
    // runtimeNano reads above. A companion, not a push forward: runtime's getg() is not where the
    // managed bubble lives, and time already answers its other runtime pulls here.
    internal static partial bool runtimeIsBubbled() => SyncTestBubble.Current is not null;

    // The REAL monotonic clock -- Go's nanotime(), which no bubble ever fakes. now() (Go's time_now),
    // the timer service thread and real-clock waits read this, never runtimeNano(): those are real in
    // Go even for a bubble member, and a member's runtimeNano() is its bubble's fake clock. It is golib's
    // MonotonicClock, the clock GoTimer waits on, so a Sleep's deadline and the timer that ends it share one
    // epoch by construction.
    private static int64 realNano() => MonotonicClock.Nanoseconds();

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
        int64 mono = realNano();
        return (sec, nsec, mono);
    }

    // runtimeNow returns the current time, and at Go 1.24 it is what time.Now() calls: Now()'s body
    // changed from `now()` to `runtimeNow()` at the hop, so EVERY converted program's first
    // time.Now() reached the throwing stub this body replaces (i9 measured it ending os/user and os
    // test hosts, c5f7b4b90d; COORD ruled it at 7fad751867).
    //
    // ⚠ WHY THIS COMPANION AND NOT A LINKNAME FORWARD. runtime.time_runtimeNow exists and carries a
    // body (runtime/time.cs:17), but its non-bubble path returns time_now() — and time_now is itself
    // a BODYLESS partial on windows (runtime/windows/timeasm.cs:13) and linux (linux/timeasm.cs:13),
    // where Go implements it in assembly. Forwarding would move the same throw one frame deeper on
    // exactly the two flavours that matter here. Read at the tree; darwin differs (timestub.cs:25
    // has a real body over walltime/nanotime), which is why the flavour is named rather than the
    // claim generalized.
    //
    // THE BUBBLE BRANCH, now owed and taken (S3 of DESIGN-gopark-goready-synctest.md). This comment
    // used to say "RE-CHECK THE DAY A SYNCTEST BRIDGE IS WIRED: this body then owes the syncGroup
    // branch" -- golib's SyncTestBubble is that bridge. runtime.time_runtimeNow returns the enclosing
    // bubble's fake clock when getg().bubble is set, and time_now() otherwise; so does this.
    //
    // Inside a bubble the monotonic reading is ZERO, as Go's (go1.26.9 runtime/time.go:17-30: "Don't
    // return a monotonic time inside a synctest bubble"), so Now() carries no monotonic component and
    // Since and Sub fall back to wall arithmetic. This body used to return mono = the fake nanoseconds,
    // and a Time taken OUTSIDE the bubble then subtracted a real uptime reading from fake epoch
    // nanoseconds: internal/synctest's TestMonotonicClock read time.Since(start) as about 56.8 years.
    // runtimeNano still returns the fake clock in a bubble (Go's time_runtimeNano does), and timers read
    // that.
    //
    // Delegating to now() rather than repeating its work keeps Now()'s wall clock and monotonic
    // component reading the same two sources as Since / Sub, which is the property startNano rebases.
    internal static partial (int64 sec, int32 nsec, int64 mono) runtimeNow()
    {
        if (SyncTestBubble.Current is { } bubble)
        {
            int64 fake = bubble.Now;
            return (fake / 1_000_000_000L, (int32)(fake % 1_000_000_000L), 0);
        }

        return now();
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
    // SERVICE MODEL. Go keeps every runtime timer in one kind of heap, whether time.Sleep, a Timer, a
    // Ticker or an I/O deadline armed it. The managed model has ONE heap too: golib's GoTimer, served
    // by one background thread, which internal/poll's deadlines already use. Each real-clock
    // runtimeTimer owns one GoTimer (`engine`), armed exactly while `when > 0`, and a Sleep arms its
    // thread's own. One thread suffices and is ordering-faithful because the only two callbacks
    // package time ever installs are `sendTime` (a NON-BLOCKING channel send) and `goFunc` (which only
    // starts a goroutine): neither can block the service thread and delay a later deadline, which is
    // GoTimer's contract. Callbacks run with s_timerLock released, in deadline order. A synctest
    // bubble's FAKE timers stay on the bubble's own heap, on the bubble's clock (below).
    //
    // WHY NOT System.Threading.Timer. Its resolution is the Windows timer tick — measured at ~15 ms
    // on this host, so a 1 ms Go timer would fire ~15x late and any two timers less than a tick
    // apart could fire OUT OF ORDER. GoTimer's service thread instead waits on a Windows
    // high-resolution waitable timer (golib's GoHighResolutionTimer), precisely the object the Go
    // runtime uses for the same job (runtime/os_windows.go createHighResTimer/usleep), and it reads
    // the same monotonic clock as runtimeNano(), so timer deadlines are coherent with Now()/Since/Sub.
    //
    // LOCKING DISCIPLINE. `s_timerLock` guards every runtimeTimer field and the bubbles' fake heaps.
    // Go's finer-grained scheme (a per-timer lock plus a per-P heap lock) exists to scale across Ps;
    // here one lock serves every timer. GoTimer's own heap lock is taken INSIDE it (engine.Reset and
    // engine.Stop) and never the reverse, because GoTimer runs a callback with its heap lock released
    // and the callback (fireReal) takes s_timerLock. No user code and no timer callback ever runs
    // under s_timerLock — the firing is decided inside it and delivered after releasing it — so a
    // callback cannot deadlock against a concurrent Stop/Reset. A Stop or Reset that races the
    // firing cannot double-fire or lose a re-arm: both the firing decision and the cancellation are
    // `when` transitions made under this one lock, and the loser is detected there (fireReal for a
    // real timer, the generation check for a fake one — see runtimeTimer.gen).
    //
    // ⚠ There IS a second lock now — a per-timer `sendLock`, for chan timers only (below) — and with
    // it one ordering rule, which holds today by construction and must keep holding:
    //
    //     sendLock is OUTERMOST. sendLock -> s_timerLock -> GoTimer's heap lock, and sendLock -> the
    //     channel's own lock.
    //
    // stop/modify take sendLock first and then s_timerLock; a firing (fireReal, or a bubble's
    // check) takes sendLock only AFTER releasing s_timerLock; the send itself and the drain take the channel lock under
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
    // ONE FIRING PER TIMER PER PASS (r39, 2026-08-03; restated for the GoTimer heap, D3). Go's
    // scheduler samples the clock once in timers.check and hands it down through timers.run(now) to
    // timer.unlockAndRun(now), and the consequence is a theorem rather than a heuristic:
    //
    //     Within one service pass, every timer fires AT MOST ONCE.
    //
    // Two facts carry it here. First, THE BATCH IS FIXED BEFORE ANY CALLBACK RUNS: GoTimer's pass
    // takes every due timer off its heap under its heap lock and only then runs the callbacks, and a
    // GoTimer is in that heap at most once. A periodic re-arm (engine.Reset in takeDueLocked) puts the
    // timer back on the HEAP, never into the batch being run, so it waits for the next pass.
    //
    // Second, the arithmetic. A firing reads the clock ONCE (fireReal's `now`, a bubble check's
    // bubble.Now) and a periodic timer is re-armed to next = when + period*(1 + delay/period) with
    // delay = now - when >= 0; writing delay = q*period + r for 0 <= r < period gives
    // next = when + period*(1 + q) = (when + delay) + (period - r) = now + (period - r), and
    // r < period makes that STRICTLY greater than `now`. It holds for every period, including the
    // 1 ns one testTimerChan resets to. Overflow cannot defeat it: the arithmetic is unchecked, the
    // true value is always in [1, 2^64-2), so a wrapped result is always NEGATIVE — never a small
    // positive that could land at or below `now` — and the `next < 0` clamp catches exactly that. A
    // bubble's fake heap keeps one entry per ARM, so there the first fact is the generation pairing:
    // every Enqueue in this file is immediately preceded by a `gen++` on the same timer and queues
    // the POST-increment value, so every older entry fails the `entry.timer.gen != entry.gen` test
    // (keep that pairing intact: moving the `gen++` in takeDueLocked into the periodic branch would
    // silently break this), and the strict inequality ends the bubble's drain.
    //
    // Re-reading the clock between firings of ONE drain broke that theorem and was the defect r39
    // replaced (r36 recorded it): the advanced `when` lands one nanosecond ahead, a freshly read
    // `now` has already passed it, and the same ticker fires again — for as long as consecutive reads
    // of the ~100 ns monotonic source keep advancing. The burst is invisible while nobody is
    // receiving (sendTime's non-blocking send onto the cap-1 channel drops all but one) but
    // testTimerChan's drainAsync IS receiving, so the two stale values an async ticker is allowed
    // became three or more and `noTick` reported "extra tick" — in ALL THREE asynctimerchan modes.
    //
    // What the invariant does NOT do is rate-limit, and it cannot delay anything: a high-rate ticker
    // is served every pass, as in Go, where the scheduler simply calls check again with a fresh
    // `now`. The bound is on re-firing WITHIN a pass, which is Go's "drop ticks to make up for slow
    // receivers". A timer coming due DURING a pass waits for the next one, and that is not a delay
    // either: GoTimer's next wait is for its heap head, already past, so the next pass starts at once.
    //
    // Each real firing is decided under s_timerLock in its OWN callback, as Go's check releases the
    // timer-set lock around each callback and re-validates between them, so a Stop landing mid-pass
    // cancels the firings after it (fireReal finds `when` cleared). A bubble's check still commits
    // its whole batch under one hold of s_timerLock and then runs it; in SYNC mode the seq check
    // restores the cancellation for its chan timers, and in async mode it does not, which is the
    // pre-1.23 behavior the mode selects.
    //
    // Finally, a CONSTRAINT this establishes rather than merely satisfies. At the instant a ticker is
    // stopped or reset, at most TWO of its ticks can exist: one in the cap-1 buffer and one decided
    // but not yet sent. In SYNC mode both are revoked outright (the drain takes the first, the seq
    // check the second), so the guarantee does not rest on the count. In ASYNC mode it does, and
    // there the two is exactly the allowance testTimerChan's drainAsync drains for a ticker — the
    // margin is zero. Any later change that lets two ticks for ONE timer be decided before either is
    // sent — batching two passes, a second service thread, a callback handed to the thread pool —
    // re-breaks `noTick` under asynctimerchan=1 and =2 without touching a line of this file.

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

        // Arm generation, bumped on EVERY change to `when` (arm, stop, reset, ticker re-arm). A
        // bubble's fake-heap entries carry the generation they were queued with, which is how a Stop
        // or Reset cancels an already-queued fake firing without removing it from the heap: the
        // bubble's check drops any entry whose generation no longer matches. This is the managed
        // stand-in for Go's timerModified/timerZombie bits, which likewise leave the stale heap entry
        // in place. A real timer needs none of it: its `engine` is moved or removed in place.
        internal int64 gen;

        // The real-clock timer on golib's one heap, created at this timer's first real arm (a fake
        // timer never has one). Armed exactly while `when > 0`, and at `when`: every change to `when`
        // under s_timerLock moves it to match (queueLocked, stopLocked, takeDueLocked). GoTimer can
        // still run a callback it took off its heap just before a Stop or Reset, so fireReal re-reads
        // `when` under s_timerLock and drops a firing whose arm is gone.
        internal GoTimer? engine;

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

        // The synctest bubble whose FAKE clock this timer runs on, or null -- Go's t.isFake, kept as
        // the bubble so arming finds its heap. Set when the timer is created by a bubble member
        // (runtime.newTimer: if getg().syncGroup != nil { t.isFake = true }) and immutable after.
        internal SyncTestBubble? fake;

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
        // ordered against both writers. At most one offer per timer can be outstanding: the timer
        // service thread is single and delivers a firing before it decides the next, and a timer is
        // on its heap at most once.
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
    // measured at ~670 ns. Three orders of magnitude more expensive, so it is read at Go's own
    // granularity and no finer: once per CHAN-timer firing (fireReal; a bubble's check reads it once
    // per batch), and per Stop/Reset and len()/cap() of a timer channel — all per user operation or
    // per tick. A func timer pays NOTHING, since fireReal and syncSendLock test IsChan first. Do not
    // "optimize" this by caching the result across calls: the live-mode contract is the thing it
    // exists to honor.
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
    // modify cannot drift apart. (A firing asks the same question once, before it is decided, and
    // carries the answer through its delivery — see `sync` in fireReal and checkBubbleTimers.)
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

    // The single lock (see LOCKING DISCIPLINE above). The heap is golib's GoTimer; golib's deadlock
    // report reads it directly (Go's checkdead scans the same heaps), so package time registers
    // nothing for it.
    private static readonly object s_timerLock = new();

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

        // Go's timeSleep in a synctest bubble: the deadline is on the bubble's FAKE clock, the wake is
        // a fake timer on the bubble's heap, and the park is durable -- the bubble counts the sleeper
        // idle and advances its clock to the deadline once everything is. Armed BEFORE the park, as Go
        // does ("time won't advance until we park"): d > 0, so the deadline is never due at the clock
        // this goroutine read, and only the bubble's Run moves that clock.
        if (SyncTestBubble.Current is { } bubble)
        {
            Goroutine sleeper = Goroutine.Current!;

            runtimeTimer wake = new()
            {
                f = (_, _, _) =>
                {
                    Goroutine.Ready(sleeper);
                    sleeper.ReleaseParkGate();
                },
                fake = bubble
            };

            lock (s_timerLock)
                armLocked(wake, deadline);

            using (Goroutine.Park(WaitReason.Sleep))
                Goroutine.WaitOnParkGate();

            return;
        }

        // Go's timeSleep: the sleeper's own timer (gp.timer, reused across sleeps; here one per thread)
        // goes on the one heap and its callback wakes the sleeper, so a sleep is exactly as precise as
        // every other timer. Parked ONCE for the whole sleep, as Go parks once with waitReasonSleep.
        // The loop is for a wake that is not this sleep's: the firing of an earlier sleep on this
        // thread that was interrupted while armed. It re-arms rather than trusting the wake, so the
        // sleep still lasts at least d.
        sleepTimer timer = t_sleepTimer ??= new sleepTimer();

        using (Goroutine.Park(WaitReason.Sleep))
        {
            while (true)
            {
                timer.woken.Reset();
                timer.engine.Reset(deadline);
                timer.woken.Wait();

                if (realNano() >= deadline)
                    return;
            }
        }
    }

    // A thread's sleep timer: a GoTimer whose callback sets the event the sleeper waits on.
    private sealed class sleepTimer
    {
        internal readonly ManualResetEventSlim woken = new(false);
        internal readonly GoTimer engine;

        internal sleepTimer()
        {
            engine = new GoTimer(static state => ((ManualResetEventSlim)state!).Set(), woken);
        }
    }

    [ThreadStatic]
    private static sleepTimer? t_sleepTimer;

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

        // Made by a bubble member: a fake timer, on the bubble's heap and clock (S3).
        timer.fake = SyncTestBubble.Current;

        s_timerState.Add(box, timer);
        modifyRuntimeTimer(timer, when, period);
    }

    // Stops the timer owned by the given Timer/Ticker box, reporting whether it was still pending
    // (armed and not yet fired). Shared with tick.cs — runtime.stopTimer serves both.
    internal static bool stopRuntimeTimer(object box)
    {
        runtimeTimer timer = runtimeTimerOf(box);

        // Go's stopTimer: a fake timer touched from outside any bubble is fatal.
        if (outsideBubbleTimerFault(timer, "stop") is { } fault)
            go.golib.FatalReport.Fatal(fault, userFault: true);

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
            // Cancels any firing already queued for this timer: a real timer leaves the heap, a fake
            // one's entry is invalidated (see runtimeTimer.gen). Doing it under the same lock a
            // firing is decided under is what makes the stop-vs-fire race safe in both directions:
            // either the firing was already decided (and `when` is 0, so `offered` reports it
            // instead), or the firing finds `when` cleared, or its entry stale, and never runs.
            timer.gen++;
            timer.engine?.Stop();

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
        runtimeTimer timer = runtimeTimerOf(box);

        // Go's resetTimer: a fake timer touched from outside any bubble is fatal.
        if (outsideBubbleTimerFault(timer, "reset") is { } fault)
            go.golib.FatalReport.Fatal(fault, userFault: true);

        return modifyRuntimeTimer(timer, when, period);
    }

    // Go's stopTimer / resetTimer decision: a fake timer touched from a goroutine in NO bubble is a
    // fault, and this is its text (null when the call may run). Go 1.26 calls fatal (go1.26.9
    // runtime/time.go:417-419, 432-434); Go 1.24 panicked. Unlike a channel's, the condition is
    // still "no bubble", not "another bubble". The fault ends the process, so a test reads this
    // (GolibTests, by reflection, with runtimeTimerOf).
    private static string? outsideBubbleTimerFault(runtimeTimer timer, string operation) =>
        timer.fake is not null && SyncTestBubble.Current is null
            ? $"{operation} of synctest timer from outside bubble"
            : null;

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
        bool wasPending;
        bool wasHeaped;

        if (sendLock is null)
        {
            wasHeaped = wasPending = modifyLocked(timer, when, period, bumpSeq: false);
        }
        else
        {
            // Synchronous timer channel (runtime.timer.modify's !async && isChan path) — the same one
            // critical section as stop above, and here the lock is load-bearing in a second way: the
            // re-arm can make the timer due IMMEDIATELY, so without holding sendLock across the drain a
            // fresh, legitimate tick could be produced and then discarded by this very drain.
            lock (sendLock)
            {
                bool pending = modifyLocked(timer, when, period, bumpSeq: true);
                bool revoked = timer.c.DrainBuffer();
                wasHeaped = pending;
                wasPending = pending || revoked;
            }
        }

        // A bubbled timer due as it is ADDED runs now, in this goroutine, once the send lock is free. Go's
        // condition is `add && t.isFake && t.state&timerHeaped == 0 && when <= bubble.now`: a Reset of a
        // timer still on the heap is left for the bubble's next check, as Go leaves it.
        if (timer.fake is { } bubble && !wasHeaped && when <= bubble.Now)
            runFakeTimerNow(timer, bubble);

        return wasPending;
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

    // Arms the timer for `when`. Caller holds s_timerLock.
    private static void armLocked(runtimeTimer timer, int64 when)
    {
        timer.when = when;
        timer.gen++;
        queueLocked(timer, when);
    }

    // Puts the timer on its heap for `when`, the post-increment `gen` already set. A fake timer goes
    // on its bubble's own heap (Go: ts = &sg.timers) and wakes nothing: only the bubble's clock can
    // make it due, and the clock moves only in the bubble's Run. A real timer's engine is moved to
    // `when` in place, and a new head wakes the service thread. Caller holds s_timerLock.
    private static void queueLocked(runtimeTimer timer, int64 when)
    {
        if (timer.fake is { } bubble)
        {
            fakeHeapOf(bubble).Enqueue((timer, timer.gen), heapPrio(timer, when));
            return;
        }

        (timer.engine ??= new GoTimer(s_fireReal, timer)).Reset(when);
    }

    private static readonly Action<object?> s_fireReal = fireReal;

    // A real timer's GoTimer callback, on the timer service thread: Go's unlockAndRun for one timer.
    // The clock is read ONCE, under s_timerLock, and the firing is decided there and delivered after
    // it is released (see ONE FIRING PER TIMER PER PASS). The asynctimerchan setting is read only for
    // a chan timer, where Go reads it per firing; a func timer pays nothing (see asyncTimerChan).
    private static void fireReal(object? state)
    {
        runtimeTimer timer = (runtimeTimer)state!;
        bool sync = timer.IsChan && !asyncTimerChan();
        List<(runtimeTimer timer, Action<any, uintptr, int64> f, any? arg, int64 delay, int64 seq)> due = s_realDue;

        lock (s_timerLock)
        {
            int64 now = realNano();

            // GoTimer took this firing off its heap before a Stop or a Reset to a later time reached
            // the timer: the arm it was for is gone (`when` 0) or queued again (`when` past `now`).
            if (timer.when == 0 || timer.when > now)
                return;

            takeDueLocked(timer, timer.when, now, sync, due);
        }

        fireDue(due, sync);
        due.Clear();
    }

    // fireReal's batch of one, reused: GoTimer runs every callback on its one service thread.
    private static readonly List<(runtimeTimer timer, Action<any, uintptr, int64> f, any? arg, int64 delay, int64 seq)> s_realDue = new(1);

    // ---- synctest bubbles: fake timers (S3; COORD's addition 5) ----
    //
    // A bubble's fake timers live on their own heap, kept in the bubble's opaque TimerState slot (golib
    // cannot name runtimeTimer). They run through the SAME drain and fire as the service thread --
    // collectDueLocked / fireDue -- on the bubble's clock instead of the real one, called from the
    // bubble's Run loop on its root's thread (a member: an AfterFunc's goroutine joins the bubble).
    // Lock order is bubble, then s_timerLock, never the reverse: NextTimerWake is called under the
    // bubble's lock, and CheckTimers reads the bubble's clock BEFORE taking s_timerLock.

    // The bubble's fake heap, created on first arm. Under s_timerLock.
    private static PriorityQueue<(runtimeTimer timer, int64 gen), (int64 when, uint rand)> fakeHeapOf(SyncTestBubble bubble) =>
        (PriorityQueue<(runtimeTimer timer, int64 gen), (int64 when, uint rand)>)(bubble.TimerState ??= new PriorityQueue<(runtimeTimer timer, int64 gen), (int64 when, uint rand)>());

    // A heap entry's order: its deadline, then a tiebreak. Go re-randomizes a FAKE timer's rand at every
    // insert (maybeAdd, go1.26.9 runtime/time.go:739-743) and orders the heap on (when, rand), so bubbled
    // timers due at one fake instant fire in no fixed order, which internal/synctest's
    // TestTimerNondeterminism checks across 1000 runs. Only a fake timer reaches here: a real timer is on GoTimer's heap.
    private static (int64 when, uint rand) heapPrio(runtimeTimer timer, int64 when) =>
        (when, timer.fake is null ? 0u : (uint)System.Random.Shared.Next());

    // Go's sg.timers.check(sg.now): every fake timer due at the bubble's clock fires.
    private static void checkBubbleTimers(SyncTestBubble bubble)
    {
        if (bubble.TimerState is null)
            return;

        List<(runtimeTimer timer, Action<any, uintptr, int64> f, any? arg, int64 delay, int64 seq)> due = new();
        bool sync = !asyncTimerChan();
        int64 now = bubble.Now;

        lock (s_timerLock)
            collectDueLocked(fakeHeapOf(bubble), now, sync, due);

        fireDue(due, sync);
    }

    // Go's sg.timers.wakeTime(): the earliest LIVE fake timer's deadline, or 0 when none is pending
    // (stale entries -- stopped or re-armed -- are dropped on the way, as the drain drops them).
    private static long nextBubbleTimerWake(SyncTestBubble bubble)
    {
        lock (s_timerLock)
        {
            if (bubble.TimerState is null)
                return 0;

            PriorityQueue<(runtimeTimer timer, int64 gen), (int64 when, uint rand)> heap = fakeHeapOf(bubble);

            while (heap.TryPeek(out (runtimeTimer timer, int64 gen) entry, out (int64 when, uint rand) prio))
            {
                if (entry.timer.gen == entry.gen)
                    return prio.when;

                heap.Dequeue();
            }

            return 0;
        }
    }

    [ModuleInitializer]
    internal static void ᴛInstallSyncTestTimers()
    {
        SyncTestBubble.CheckTimers = checkBubbleTimers;
        SyncTestBubble.NextTimerWake = nextBubbleTimerWake;
        SyncTestBubble.AsyncTimerChan = asyncTimerChan;
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

    // A synctest bubble's drain (checkBubbleTimers): run under s_timerLock, it moves every fake timer
    // due at `now` out of `heap` into `due` and returns the next deadline (0 when nothing is armed).
    // Extracted for S3 from the real-clock service pass, whose heap is GoTimer's since D3; the firing
    // itself (takeDueLocked, fireDue) is shared, so the real clock and a bubble's fake one are ONE
    // engine with two clocks.
    private static int64 collectDueLocked(PriorityQueue<(runtimeTimer timer, int64 gen), (int64 when, uint rand)> heap, int64 now, bool sync,
        List<(runtimeTimer timer, Action<any, uintptr, int64> f, any? arg, int64 delay, int64 seq)> due)
    {
        int64 deadline = 0;

        while (heap.TryPeek(out (runtimeTimer timer, int64 gen) entry, out (int64 when, uint rand) prio))
        {
            if (entry.timer.gen != entry.gen)
            {
                // Stopped, reset or re-armed after this entry was queued — drop it; the live
                // entry, if any, is elsewhere in the heap (see runtimeTimer.gen).
                heap.Dequeue();
                continue;
            }

            if (prio.when > now)
            {
                deadline = prio.when;
                break;
            }

            heap.Dequeue();
            takeDueLocked(entry.timer, prio.when, now, sync, due);
        }

        return deadline;
    }

    // One due timer's firing: queue it into `due` and advance or clear its `when`, re-arming a periodic
    // timer on its heap. Shared by a real timer's callback (fireReal), a bubble's drain above, and a
    // bubbled timer that is already due when it is armed (runFakeTimerNow). Caller holds s_timerLock and
    // holds no live heap entry for the timer: GoTimer or the drain has taken it off its heap, or (the
    // run-now path) there is none.
    private static void takeDueLocked(runtimeTimer timer, int64 when, int64 now, bool sync,
        List<(runtimeTimer timer, Action<any, uintptr, int64> f, any? arg, int64 delay, int64 seq)> due)
    {
        // runtime.timer.unlockAndRun: capture the callback, advance or clear `when`, and
        // run the callback only after the lock is released. `delay` is how LATE the
        // firing is (Go's `delay := now - t.when`, always >= 0); sendTime subtracts it
        // to send the time the tick was scheduled for rather than the time it ran.
        // `seq` is captured HERE, with the firing decision, exactly as Go's
        // unlockAndRun copies t.seq while still holding t.mu — it is what the send
        // below re-checks to discover a Stop/Reset that landed in between.
        int64 delay = now - when;
        due.Add((timer, timer.f, timer.arg, delay, timer.seq));
        timer.gen++;

        // From here until the send resolves, this timer has a tick that exists but is
        // in NEITHER place a Stop can look — `when` is about to be cleared and the
        // buffer is still empty. `offered` is that third state, and recording it is
        // what lets Stop/Reset answer `pending` correctly for a firing they are about
        // to revoke. See COMMITTED-BUT-UNSENT on runtimeTimer.offered.
        timer.offered = sync && timer.IsChan;

        if (timer.period > 0)
        {
            // Advance by WHOLE periods past a late firing, so the tick phase stays
            // aligned to the original schedule instead of drifting — Go's documented
            // "adjust the time interval or drop ticks to make up for slow receivers"
            // (unlockAndRun: next = when + period*(1 + delay/period)). Combined with
            // sendTime's non-blocking send onto the cap-1 channel, a receiver too slow
            // to keep up therefore LOSES ticks rather than seeing them queue up or the
            // ticker fall behind.
            int64 next = when + timer.period * (1 + delay / timer.period);

            if (next < 0)
            {
                next = int64.MaxValue;
            }

            timer.when = next;
            queueLocked(timer, next);
        }
        else
        {
            timer.when = 0;

            // A Reset to a time already past can have put the engine back on its heap after GoTimer
            // took this firing off it; this firing serves that arm, so the arm leaves the heap.
            timer.engine?.Stop();
        }
    }

    // Go's timer.modify for a bubbled timer that is due as it is armed (go1.26.9 runtime/time.go:622-645):
    // "run it now rather than waiting for the bubble's timer scheduler", in the arming goroutine, so a
    // 0-duration timer's channel is readable at once and an AfterFunc(0, f) starts f while its creator
    // runs on. Waiting for Run to fire it deferred it until the bubble was durably blocked, which never
    // comes for a creator that spins. Called with no lock held; the send lock is free, as Go unlocks it
    // before unlockAndRun.
    private static void runFakeTimerNow(runtimeTimer timer, SyncTestBubble bubble)
    {
        List<(runtimeTimer timer, Action<any, uintptr, int64> f, any? arg, int64 delay, int64 seq)> due = new();
        bool sync = !asyncTimerChan();
        int64 now = bubble.Now;

        lock (s_timerLock)
        {
            // Still armed and still due: a racing Stop or Reset since the arm has moved `when`.
            if (timer.when > 0 && timer.when <= now)
                takeDueLocked(timer, timer.when, now, sync, due);
        }

        fireDue(due, sync);
    }

    // The delivery of decided firings, outside s_timerLock -- shared the same way.
    private static void fireDue(List<(runtimeTimer timer, Action<any, uintptr, int64> f, any? arg, int64 delay, int64 seq)> due, bool sync)
    {
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
    }
}
