// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Time-related runtime and pieces of package time.
namespace go;

using abi = @internal.abi_package;
using atomic = @internal.runtime.atomic_package;
using sys = runtime.@internal.sys_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using runtime.@internal;

partial class runtime_package {

// A timer is a potentially repeating trigger for calling t.f(t.arg, t.seq).
// Timers are allocated by client code, often as part of other data structures.
// Each P has a heap of pointers to timers that it manages.
//
// A timer is expected to be used by only one client goroutine at a time,
// but there will be concurrent access by the P managing that timer.
// Timer accesses are protected by the lock t.mu, with a snapshot of
// t's state bits published in t.astate to enable certain fast paths to make
// decisions about a timer without acquiring the lock.
[GoType] partial struct timer {
    // mu protects reads and writes to all fields, with exceptions noted below.
    internal mutex mu;
    internal atomic.Uint8 astate; // atomic copy of state bits at last unlock
    internal uint8 state;        // state bits
    internal bool isChan;         // timer has a channel; immutable; can be read without lock
    internal uint32 blocked; // number of goroutines blocked on timer's channel
    // Timer wakes up at when, and then at when+period, ... (period > 0 only)
    // each time calling f(arg, seq, delay) in the timer goroutine, so f must be
    // a well-behaved function and not block.
    //
    // The arg and seq are client-specified opaque arguments passed back to f.
    // When used from netpoll, arg and seq have meanings defined by netpoll
    // and are completely opaque to this code; in that context, seq is a sequence
    // number to recognize and squech stale function invocations.
    // When used from package time, arg is a channel (for After, NewTicker)
    // or the function to call (for AfterFunc) and seq is unused (0).
    //
    // Package time does not know about seq, but if this is a channel timer (t.isChan == true),
    // this file uses t.seq as a sequence number to recognize and squelch
    // sends that correspond to an earlier (stale) timer configuration,
    // similar to its use in netpoll. In this usage (that is, when t.isChan == true),
    // writes to seq are protected by both t.mu and t.sendLock,
    // so reads are allowed when holding either of the two mutexes.
    //
    // The delay argument is nanotime() - t.when, meaning the delay in ns between
    // when the timer should have gone off and now. Normally that amount is
    // small enough not to matter, but for channel timers that are fed lazily,
    // the delay can be arbitrarily long; package time subtracts it out to make
    // it look like the send happened earlier than it actually did.
    // (No one looked at the channel since then, or the send would have
    // not happened so late, so no one can tell the difference.)
    internal int64 when;
    internal int64 period;
    internal Action<any, uintptr, int64> f;
    internal any arg;
    internal uintptr seq;
    // If non-nil, the timers containing t.
    internal ж<timers> ts;
    // sendLock protects sends on the timer's channel.
    // Not used for async (pre-Go 1.23) behavior when debug.asynctimerchan.Load() != 0.
    internal mutex sendLock;
    // isSending is used to handle races between running a
    // channel timer and stopping or resetting the timer.
    // It is used only for channel timers (t.isChan == true).
    // It is not used for tickers.
    // The value is incremented when about to send a value on the channel,
    // and decremented after sending the value.
    // The stop/reset code uses this to detect whether it
    // stopped the channel send.
    //
    // isSending is incremented only when t.mu is held.
    // isSending is decremented only when t.sendLock is held.
    // isSending is read only when both t.mu and t.sendLock are held.
    internal atomic.Int32 isSending;
}

// init initializes a newly allocated timer t.
// Any code that allocates a timer must call t.init before using it.
// The arg and f can be set during init, or they can be nil in init
// and set by a future call to t.modify.
internal static void init(this ж<timer> Ꮡt, Action<any, uintptr, int64> f, any arg) {
    ref var t = ref Ꮡt.DerefOrNull();

    lockInit(ref nonnil(ref t).mu, lockRankTimer);
    t.f = f;
    t.arg = arg;
}

// A timers is a per-P set of timers.
[GoType] partial struct timers {
    // mu protects timers; timers are per-P, but the scheduler can
    // access the timers of another P, so we have to lock.
    internal mutex mu;
    // heap is the set of timers, ordered by heap[i].when.
    // Must hold lock to access.
    internal slice<timerWhen> heap;
    // len is an atomic copy of len(heap).
    internal atomic.Uint32 len;
    // zombies is the number of timers in the heap
    // that are marked for removal.
    internal atomic.Int32 zombies;
    // raceCtx is the race context used while executing timer functions.
    internal uintptr raceCtx;
    // minWhenHeap is the minimum heap[i].when value (= heap[0].when).
    // The wakeTime method uses minWhenHeap and minWhenModified
    // to determine the next wake time.
    // If minWhenHeap = 0, it means there are no timers in the heap.
    internal atomic.Int64 minWhenHeap;
    // minWhenModified is a lower bound on the minimum
    // heap[i].when over timers with the timerModified bit set.
    // If minWhenModified = 0, it means there are no timerModified timers in the heap.
    internal atomic.Int64 minWhenModified;
}

[GoType] partial struct timerWhen {
    internal ж<timer> timer;
    internal int64 when;
}

internal static void @lock(this ж<timers> Ꮡts) {
    @lock(Ꮡts.of(timers.Ꮡmu));
}

internal static void unlock(this ж<timers> Ꮡts) {
    ref var ts = ref Ꮡts.DerefOrNull();

    // Update atomic copy of len(ts.heap).
    // We only update at unlock so that the len is always
    // the most recent unlocked length, not an ephemeral length.
    // This matters if we lock ts, delete the only timer from the heap,
    // add it back, and unlock. We want ts.len.Load to return 1 the
    // entire time, never 0. This is important for pidleput deciding
    // whether ts is empty.
    Ꮡts.of(timers.Ꮡlen).Store((uint32)len(ts.heap));
    unlock(Ꮡts.of(timers.Ꮡmu));
}

// Timer state field.
internal const uint8 timerHeaped = /* 1 << iota */ 1;

internal const uint8 timerModified = 2;

internal const uint8 timerZombie = 4;

// timerDebug enables printing a textual debug trace of all timer operations to stderr.
internal const bool timerDebug = false;

internal static void trace(this ж<timer> Ꮡt, @string op) {
    if (timerDebug) {
        Ꮡt.trace1(op);
    }
}

internal static void trace1(this ж<timer> Ꮡt, @string op) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (!timerDebug) {
        return;
    }
    var bits = new @string[]{"h"u8, "m"u8, "z"u8, "c"u8}.array();
    foreach (var i in range(3)) {
        if ((uint8)(t.state & (((uint8)1).Lsh((uint64)(i)))) == 0) {
            bits[i] = "-"u8;
        }
    }
    if (!t.isChan) {
        bits[3] = "-"u8;
    }
    print((@string)"T "u8, Ꮡt.OrTypedNil(), (@string)" "u8, bits[0], bits[1], bits[2], bits[3], (@string)" b="u8, t.blocked, (@string)" "u8, op, (@string)"\n"u8);
}

internal static void trace(this ж<timers> Ꮡts, @string op) {
    if (timerDebug) {
        println((@string)"TS"u8, Ꮡts.OrTypedNil(), op);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lockˢ = "lock"u8;

// lock locks the timer, allowing reading or writing any of the timer fields.
internal static void @lock(this ж<timer> Ꮡt) {
    @lock(Ꮡt.of(timer.Ꮡmu));
    Ꮡt.trace(lockˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unlockˢ = "unlock"u8;

// unlock updates t.astate and unlocks the timer.
internal static void unlock(this ж<timer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.trace(unlockˢ);
    // Let heap fast paths know whether heap[i].when is accurate.
    // Also let maybeRunChan know whether channel is in heap.
    Ꮡt.of(timer.Ꮡastate).Store(t.state);
    unlock(Ꮡt.of(timer.Ꮡmu));
}

// hchan returns the channel in t.arg.
// t must be a timer with a channel.
internal static ж<Δhchan> hchan(this ж<timer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (!t.isChan) {
        badTimer();
    }
    // Note: t.arg is a chan time.Time,
    // and runtime cannot refer to that type,
    // so we cannot use a type assertion.
    return (ж<Δhchan>)(uintptr)((~efaceOf(Ꮡt.of(timer.Ꮡarg))).data);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string updateHeapˢ = "updateHeap"u8;

// updateHeap updates t as directed by t.state, updating t.state
// and returning a bool indicating whether the state (and ts.heap[0].when) changed.
// The caller must hold t's lock, or the world can be stopped instead.
// The timer set t.ts must be non-nil and locked, t must be t.ts.heap[0], and updateHeap
// takes care of moving t within the timers heap to preserve the heap invariants.
// If ts == nil, then t must not be in a heap (or is in a heap that is
// temporarily not maintaining its invariant, such as during timers.adjust).
internal static bool /*updated*/ updateHeap(this ж<timer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    assertWorldStoppedOrLockHeld(ref nonnil(ref t).mu);
    Ꮡt.trace(updateHeapˢ);
    var ts = t.ts;
    if (ts == nil || Ꮡt != (~ts).heap[0].timer) {
        badTimer();
    }
    assertLockHeld(ts.of(timers.Ꮡmu));
    if ((uint8)(t.state & timerZombie) != 0) {
        // Take timer out of heap.
        t.state &= unchecked((uint8)~(uint8)((uint8)((uint8)(timerHeaped | timerZombie) | timerModified)));
        ts.of(timers.Ꮡzombies).Add(-1);
        ts.deleteMin();
        return true;
    }
    if ((uint8)(t.state & timerModified) != 0) {
        // Update ts.heap[0].when and move within heap.
        t.state &= unchecked((uint8)~(uint8)(timerModified));
        (~ts).heap[0].when = t.when;
        ts.siftDown(0);
        ts.updateMinWhenHeap();
        return true;
    }
    return false;
}

// maxWhen is the maximum value for timer's when field.
internal static UntypedInt maxWhen => /* 1<<63 - 1 */ 9223372036854775807;

// verifyTimers can be set to true to add debugging checks that the
// timer heaps are valid.
internal const bool verifyTimers = false;

// Package time APIs.
// Godoc uses the comments in package time, not these.
// time.now is implemented in assembly.

// timeSleep puts the current goroutine to sleep for at least ns nanoseconds.
//
//go:linkname timeSleep time.Sleep
internal static void timeSleep(int64 ns) {
    if (ns <= 0) {
        return;
    }
    var gp = getg();
    var t = gp.Value.timer;
    if (t == nil) {
        t = @new<timer>();
        t.init(goroutineReady, gp.OrTypedNil());
        gp.Value.timer = t;
    }
    var when = nanotime() + ns;
    if (when < 0) {
        // check for overflow.
        when = maxWhen;
    }
    gp.Value.sleepWhen = when;
    gopark(resetForSleep, nil, waitReasonSleep, traceBlockSleep, 1);
}

// resetForSleep is called after the goroutine is parked for timeSleep.
// We can't call timer.reset in timeSleep itself because if this is a short
// sleep and there are many goroutines then the P can wind up running the
// timer function, goroutineReady, before the goroutine has been parked.
internal static bool resetForSleep(ж<g> Ꮡgp, @unsafe.Pointer _) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    gp.timer.reset(gp.sleepWhen, 0);
    return true;
}

// A timeTimer is a runtime-allocated time.Timer or time.Ticker
// with the additional runtime state following it.
// The runtime state is inaccessible to package time.
[GoType] partial struct timeTimer {
    internal @unsafe.Pointer c; // <-chan time.Time
    internal bool init;
    internal partial ref timer timer { get; }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string newˢ = "new"u8;
internal static readonly @string invalidTimerChannelNoˢ = "invalid timer channel: no capacity"u8;

// newTimer allocates and returns a new time.Timer or time.Ticker (same layout)
// with the given parameters.
//
//go:linkname newTimer time.newTimer
internal static ж<timeTimer> newTimer(int64 when, int64 period, Action<any, uintptr, int64> f, any arg, ж<Δhchan> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    var t = @new<timeTimer>();
    t.of(timeTimer.Ꮡtimer).init(default!, default!);
    t.of(timeTimer.Ꮡtimer).trace(newˢ);
    if (raceenabled) {
        racerelease(new @unsafe.Pointer(t.of(timeTimer.Ꮡtimer)));
    }
    if (Ꮡc != nil) {
        lockInit(ref (t.of(timeTimer.ᏑsendLock)).DerefOrNull(), lockRankTimerSend);
        t.Value.isChan = true;
        c.timer = t.of(timeTimer.Ꮡtimer);
        if (c.dataqsiz == 0) {
            @throw(invalidTimerChannelNoˢ);
        }
    }
    t.of(timeTimer.Ꮡtimer).modify(when, period, f, arg, 0);
    t.Value.init = true;
    return t;
}

// stopTimer stops a timer.
// It reports whether t was stopped before being run.
//
//go:linkname stopTimer time.stopTimer
internal static bool stopTimer(ж<timeTimer> Ꮡt) {
    return Ꮡt.of(timeTimer.Ꮡtimer).stop();
}

// resetTimer resets an inactive timer, adding it to the timer heap.
//
// Reports whether the timer was modified before it was run.
//
//go:linkname resetTimer time.resetTimer
internal static bool resetTimer(ж<timeTimer> Ꮡt, int64 when, int64 period) {
    if (raceenabled) {
        racerelease(new @unsafe.Pointer(Ꮡt.of(timeTimer.Ꮡtimer)));
    }
    return Ꮡt.of(timeTimer.Ꮡtimer).reset(when, period);
}

// Go runtime.

// Ready the goroutine arg.
internal static void goroutineReady(any arg, uintptr _Δp1, int64 _Δp2) {
    goready(arg._<ж<g>>(), 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tsSetInTimerˢ = "ts set in timer"u8;

// addHeap adds t to the timers heap.
// The caller must hold ts.lock or the world must be stopped.
// The caller must also have checked that t belongs in the heap.
// Callers that are not sure can call t.maybeAdd instead,
// but note that maybeAdd has different locking requirements.
internal static void addHeap(this ж<timers> Ꮡts, ж<timer> Ꮡt) {
    ref var ts = ref Ꮡts.DerefOrNull();
    ref var t = ref Ꮡt.DerefOrNull();

    assertWorldStoppedOrLockHeld(ref nonnil(ref ts).mu);
    // Timers rely on the network poller, so make sure the poller
    // has started.
    if (ᏑnetpollInited.Load() == 0) {
        netpollGenericInit();
    }
    if (t.ts != nil) {
        @throw(tsSetInTimerˢ);
    }
    t.ts = Ꮡts;
    ts.heap = append(ts.heap, new timerWhen(Ꮡt, t.when));
    ts.siftUp(len(ts.heap) - 1);
    if (Ꮡt == ts.heap[0].timer) {
        Ꮡts.updateMinWhenHeap();
    }
}

// maybeRunAsync checks whether t needs to be triggered and runs it if so.
// The caller is responsible for locking the timer and for checking that we
// are running timers in async mode. If the timer needs to be run,
// maybeRunAsync will unlock and re-lock it.
// The timer is always locked on return.
internal static void maybeRunAsync(this ж<timer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    assertLockHeld(Ꮡt.of(timer.Ꮡmu));
    if ((uint8)(t.state & timerHeaped) == 0 && t.isChan && t.when > 0) {
        // If timer should have triggered already (but nothing looked at it yet),
        // trigger now, so that a receive after the stop sees the "old" value
        // that should be there.
        // (It is possible to have t.blocked > 0 if there is a racing receive
        // in blockTimerChan, but timerHeaped not being set means
        // it hasn't run t.maybeAdd yet; in that case, running the
        // timer ourselves now is fine.)
        {
            var now = nanotime(); if (t.when <= now) {
                systemstack(() => {
                    Ꮡt.unlockAndRun(now); // resets t.when
                });
                Ꮡt.@lock();
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string stopˢ = "stop"u8;

// stop stops the timer t. It may be on some other P, so we can't
// actually remove it from the timers heap. We can only mark it as stopped.
// It will be removed in due course by the P whose heap it is on.
// Reports whether the timer was stopped before it was run.
internal static bool stop(this ж<timer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var async = Ꮡdebug.of(debugᴛ1.Ꮡasynctimerchan).Load() != 0;
    if (!async && t.isChan) {
        @lock(Ꮡt.of(timer.ᏑsendLock));
    }
    Ꮡt.@lock();
    Ꮡt.trace(stopˢ);
    if (async) {
        Ꮡt.maybeRunAsync();
    }
    if ((uint8)(t.state & timerHeaped) != 0) {
        t.state |= (uint8)(timerModified);
        if ((uint8)(t.state & timerZombie) == 0) {
            t.state |= (uint8)(timerZombie);
            t.ts.of(timers.Ꮡzombies).Add(1);
        }
    }
    var pending = t.when > 0;
    t.when = 0;
    if (!async && t.isChan) {
        // Stop any future sends with stale values.
        // See timer.unlockAndRun.
        t.seq++;
        // If there is currently a send in progress,
        // incrementing seq is going to prevent that
        // send from actually happening. That means
        // that we should return true: the timer was
        // stopped, even though t.when may be zero.
        if (t.period == 0 && Ꮡt.of(timer.ᏑisSending).Load() > 0) {
            pending = true;
        }
    }
    Ꮡt.unlock();
    if (!async && t.isChan) {
        unlock(Ꮡt.of(timer.ᏑsendLock));
        if (timerchandrain(Ꮡt.hchan())) {
            pending = true;
        }
    }
    return pending;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string wrongTimersˢ = "wrong timers"u8;

// deleteMin removes timer 0 from ts.
// ts must be locked.
internal static void deleteMin(this ж<timers> Ꮡts) {
    ref var ts = ref Ꮡts.DerefOrNull();

    assertLockHeld(Ꮡts.of(timers.Ꮡmu));
    var t = ts.heap[0].timer;
    if ((~t).ts != Ꮡts) {
        @throw(wrongTimersˢ);
    }
    t.Value.ts = default!;
    nint last = len(ts.heap) - 1;
    if (last > 0) {
        ts.heap[0] = ts.heap[last];
    }
    ts.heap[last] = new timerWhen(nil);
    ts.heap = ts.heap[..(int)(last)];
    if (last > 0) {
        ts.siftDown(0);
    }
    Ꮡts.updateMinWhenHeap();
    if (last == 0) {
        // If there are no timers, then clearly there are no timerModified timers.
        Ꮡts.of(timers.ᏑminWhenModified).Store(0);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timerWhenMustBePositiveˢ = "timer when must be positive"u8;
internal static readonly @string timerPeriodMustBeNonˢ = "timer period must be non-negative"u8;
internal static readonly @string modifyˢ = "modify"u8;

// modify modifies an existing timer.
// This is called by the netpoll code or time.Ticker.Reset or time.Timer.Reset.
// Reports whether the timer was modified before it was run.
// If f == nil, then t.f, t.arg, and t.seq are not modified.
internal static bool modify(this ж<timer> Ꮡt, int64 when, int64 period, Action<any, uintptr, int64> f, any arg, uintptr seq) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (when <= 0) {
        @throw(timerWhenMustBePositiveˢ);
    }
    if (period < 0) {
        @throw(timerPeriodMustBeNonˢ);
    }
    var async = Ꮡdebug.of(debugᴛ1.Ꮡasynctimerchan).Load() != 0;
    if (!async && t.isChan) {
        @lock(Ꮡt.of(timer.ᏑsendLock));
    }
    Ꮡt.@lock();
    if (async) {
        Ꮡt.maybeRunAsync();
    }
    Ꮡt.trace(modifyˢ);
    var oldPeriod = t.period;
    t.period = period;
    if (f != default!) {
        t.f = f;
        t.arg = arg;
        t.seq = seq;
    }
    var wake = false;
    var pending = t.when > 0;
    t.when = when;
    if ((uint8)(t.state & timerHeaped) != 0) {
        t.state |= (uint8)(timerModified);
        if ((uint8)(t.state & timerZombie) != 0) {
            // In the heap but marked for removal (by a Stop).
            // Unmark it, since it has been Reset and will be running again.
            t.ts.of(timers.Ꮡzombies).Add(-1);
            t.state &= unchecked((uint8)~(uint8)(timerZombie));
        }
        // The corresponding heap[i].when is updated later.
        // See comment in type timer above and in timers.adjust below.
        {
            var min = t.ts.of(timers.ᏑminWhenModified).Load(); if (min == 0 || when < min) {
                wake = true;
                // Force timerModified bit out to t.astate before updating t.minWhenModified,
                // to synchronize with t.ts.adjust. See comment in adjust.
                Ꮡt.of(timer.Ꮡastate).Store(t.state);
                t.ts.updateMinWhenModified(when);
            }
        }
    }
    var add = Ꮡt.needsAdd();
    if (!async && t.isChan) {
        // Stop any future sends with stale values.
        // See timer.unlockAndRun.
        t.seq++;
        // If there is currently a send in progress,
        // incrementing seq is going to prevent that
        // send from actually happening. That means
        // that we should return true: the timer was
        // stopped, even though t.when may be zero.
        if (oldPeriod == 0 && Ꮡt.of(timer.ᏑisSending).Load() > 0) {
            pending = true;
        }
    }
    Ꮡt.unlock();
    if (!async && t.isChan) {
        if (timerchandrain(Ꮡt.hchan())) {
            pending = true;
        }
        unlock(Ꮡt.of(timer.ᏑsendLock));
    }
    if (add) {
        Ꮡt.maybeAdd();
    }
    if (wake) {
        wakeNetPoller(when);
    }
    return pending;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string needsAddˢ = "needsAdd+"u8;
internal static readonly @string needsAddˢ2 = "needsAdd-"u8;

// needsAdd reports whether t needs to be added to a timers heap.
// t must be locked.
internal static bool needsAdd(this ж<timer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    assertLockHeld(Ꮡt.of(timer.Ꮡmu));
    var need = (uint8)(t.state & timerHeaped) == 0 && t.when > 0 && (!t.isChan || t.blocked > 0);
    if (need){
        Ꮡt.trace(needsAddˢ);
    } else {
        Ꮡt.trace(needsAddˢ2);
    }
    return need;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string maybeAddˢ = "maybeAdd"u8;

// maybeAdd adds t to the local timers heap if it needs to be in a heap.
// The caller must not hold t's lock nor any timers heap lock.
// The caller probably just unlocked t, but that lock must be dropped
// in order to acquire a ts.lock, to avoid lock inversions.
// (timers.adjust holds ts.lock while acquiring each t's lock,
// so we cannot hold any t's lock while acquiring ts.lock).
//
// Strictly speaking it *might* be okay to hold t.lock and
// acquire ts.lock at the same time, because we know that
// t is not in any ts.heap, so nothing holding a ts.lock would
// be acquiring the t.lock at the same time, meaning there
// isn't a possible deadlock. But it is easier and safer not to be
// too clever and respect the static ordering.
// (If we don't, we have to change the static lock checking of t and ts.)
//
// Concurrent calls to time.Timer.Reset or blockTimerChan
// may result in concurrent calls to t.maybeAdd,
// so we cannot assume that t is not in a heap on entry to t.maybeAdd.
internal static void maybeAdd(this ж<timer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Note: Not holding any locks on entry to t.maybeAdd,
    // so the current g can be rescheduled to a different M and P
    // at any time, including between the ts := assignment and the
    // call to ts.lock. If a reschedule happened then, we would be
    // adding t to some other P's timers, perhaps even a P that the scheduler
    // has marked as idle with no timers, in which case the timer could
    // go unnoticed until long after t.when.
    // Calling acquirem instead of using getg().m makes sure that
    // we end up locking and inserting into the current P's timers.
    var mp = acquirem();
    var ts = (~mp).p.ptr().of(runtime_package.Δp.Ꮡtimers);
    ts.@lock();
    ts.cleanHead();
    Ꮡt.@lock();
    Ꮡt.trace(maybeAddˢ);
    var when = (int64)0;
    var wake = false;
    if (Ꮡt.needsAdd()) {
        t.state |= (uint8)(timerHeaped);
        when = t.when;
        var wakeTime = ts.wakeTime();
        wake = wakeTime == 0 || when < wakeTime;
        ts.addHeap(Ꮡt);
    }
    Ꮡt.unlock();
    ts.unlock();
    releasem(ref (mp).DerefOrNull());
    if (wake) {
        wakeNetPoller(when);
    }
}

// reset resets the time when a timer should fire.
// If used for an inactive timer, the timer will become active.
// Reports whether the timer was active and was stopped.
internal static bool reset(this ж<timer> Ꮡt, int64 when, int64 period) {
    return Ꮡt.modify(when, period, default!, default!, 0);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cleanHeadˢ = "cleanHead"u8;
internal static readonly @string badTsˢ = "bad ts"u8;

// cleanHead cleans up the head of the timer queue. This speeds up
// programs that create and delete timers; leaving them in the heap
// slows down heap operations.
// The caller must have locked ts.
internal static void cleanHead(this ж<timers> Ꮡts) {
    ref var ts = ref Ꮡts.DerefOrNull();

    Ꮡts.trace(cleanHeadˢ);
    assertLockHeld(Ꮡts.of(timers.Ꮡmu));
    var gp = getg();
    while (ᐧ) {
        if (len(ts.heap) == 0) {
            return;
        }
        // This loop can theoretically run for a while, and because
        // it is holding timersLock it cannot be preempted.
        // If someone is trying to preempt us, just return.
        // We can clean the timers later.
        if ((~gp).preemptStop) {
            return;
        }
        // Delete zombies from tail of heap. It requires no heap adjustments at all,
        // and doing so increases the chances that when we swap out a zombie
        // in heap[0] for the tail of the heap, we'll get a non-zombie timer,
        // shortening this loop.
        nint n = len(ts.heap);
        {
            var tΔ1 = ts.heap[n - 1].timer; if ((uint8)(tΔ1.of(timer.Ꮡastate).Load() & timerZombie) != 0) {
                tΔ1.@lock();
                if ((uint8)((~tΔ1).state & timerZombie) != 0) {
                    tΔ1.Value.state &= unchecked((uint8)~(uint8)((uint8)((uint8)(timerHeaped | timerZombie) | timerModified)));
                    tΔ1.Value.ts = default!;
                    Ꮡts.of(timers.Ꮡzombies).Add(-1);
                    ts.heap[n - 1] = new timerWhen(nil);
                    ts.heap = ts.heap[..(int)(n - 1)];
                }
                tΔ1.unlock();
                continue;
            }
        }
        var t = ts.heap[0].timer;
        if ((~t).ts != Ꮡts) {
            @throw(badTsˢ);
        }
        if ((uint8)(t.of(timer.Ꮡastate).Load() & ((uint8)(timerModified | timerZombie))) == 0) {
            // Fast path: head of timers does not need adjustment.
            return;
        }
        t.@lock();
        var updated = t.updateHeap();
        t.unlock();
        if (!updated) {
            // Head of timers does not need adjustment.
            return;
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string takeˢ = "take"u8;

// take moves any timers from src into ts
// and then clears the timer state from src,
// because src is being destroyed.
// The caller must not have locked either timers.
// For now this is only called when the world is stopped.
internal static void take(this ж<timers> Ꮡts, ж<timers> Ꮡsrc) {
    ref var ts = ref Ꮡts.DerefOrNull();
    ref var src = ref Ꮡsrc.DerefOrNull();

    Ꮡts.trace(takeˢ);
    assertWorldStopped();
    if (len(src.heap) > 0) {
        // The world is stopped, so we ignore the locking of ts and src here.
        // That would introduce a sched < timers lock ordering,
        // which we'd rather avoid in the static ranking.
        foreach (var (_, tw) in src.heap) {
            var t = tw.timer;
            t.Value.ts = default!;
            if ((uint8)((~t).state & timerZombie) != 0){
                t.Value.state &= unchecked((uint8)~(uint8)((uint8)((uint8)(timerHeaped | timerZombie) | timerModified)));
            } else {
                t.Value.state &= unchecked((uint8)~(uint8)(timerModified));
                Ꮡts.addHeap(t);
            }
        }
        src.heap = default!;
        Ꮡsrc.of(timers.Ꮡzombies).Store(0);
        Ꮡsrc.of(timers.ᏑminWhenHeap).Store(0);
        Ꮡsrc.of(timers.ᏑminWhenModified).Store(0);
        Ꮡsrc.of(timers.Ꮡlen).Store(0);
        Ꮡts.of(timers.Ꮡlen).Store((uint32)len(ts.heap));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string adjustˢ = "adjust"u8;

// adjust looks through the timers in ts.heap for
// any timers that have been modified to run earlier, and puts them in
// the correct place in the heap. While looking for those timers,
// it also moves timers that have been modified to run later,
// and removes deleted timers. The caller must have locked ts.
internal static void adjust(this ж<timers> Ꮡts, int64 now, bool force) {
    ref var ts = ref Ꮡts.DerefOrNull();

    Ꮡts.trace(adjustˢ);
    assertLockHeld(Ꮡts.of(timers.Ꮡmu));
    // If we haven't yet reached the time of the earliest modified
    // timer, don't do anything. This speeds up programs that adjust
    // a lot of timers back and forth if the timers rarely expire.
    // We'll postpone looking through all the adjusted timers until
    // one would actually expire.
    if (!force) {
        var first = Ꮡts.of(timers.ᏑminWhenModified).Load();
        if (first == 0 || first > now) {
            if (verifyTimers) {
                Ꮡts.verify();
            }
            return;
        }
    }
    // minWhenModified is a lower bound on the earliest t.when
    // among the timerModified timers. We want to make it more precise:
    // we are going to scan the heap and clean out all the timerModified bits,
    // at which point minWhenModified can be set to 0 (indicating none at all).
    //
    // Other P's can be calling ts.wakeTime concurrently, and we'd like to
    // keep ts.wakeTime returning an accurate value throughout this entire process.
    //
    // Setting minWhenModified = 0 *before* the scan could make wakeTime
    // return an incorrect value: if minWhenModified < minWhenHeap, then clearing
    // it to 0 will make wakeTime return minWhenHeap (too late) until the scan finishes.
    // To avoid that, we want to set minWhenModified to 0 *after* the scan.
    //
    // Setting minWhenModified = 0 *after* the scan could result in missing
    // concurrent timer modifications in other goroutines; those will lock
    // the specific timer, set the timerModified bit, and set t.when.
    // To avoid that, we want to set minWhenModified to 0 *before* the scan.
    //
    // The way out of this dilemma is to preserve wakeTime a different way.
    // wakeTime is min(minWhenHeap, minWhenModified), and minWhenHeap
    // is protected by ts.lock, which we hold, so we can modify it however we like
    // in service of keeping wakeTime accurate.
    //
    // So we can:
    //
    //	1. Set minWhenHeap = min(minWhenHeap, minWhenModified)
    //	2. Set minWhenModified = 0
    //	   (Other goroutines may modify timers and update minWhenModified now.)
    //	3. Scan timers
    //	4. Set minWhenHeap = heap[0].when
    //
    // That order preserves a correct value of wakeTime throughout the entire
    // operation:
    // Step 1 “locks in” an accurate wakeTime even with minWhenModified cleared.
    // Step 2 makes sure concurrent t.when updates are not lost during the scan.
    // Step 3 processes all modified timer values, justifying minWhenModified = 0.
    // Step 4 corrects minWhenHeap to a precise value.
    //
    // The wakeTime method implementation reads minWhenModified *before* minWhenHeap,
    // so that if the minWhenModified is observed to be 0, that means the minWhenHeap that
    // follows will include the information that was zeroed out of it.
    //
    // Originally Step 3 locked every timer, which made sure any timer update that was
    // already in progress during Steps 1+2 completed and was observed by Step 3.
    // All that locking was too expensive, so now we do an atomic load of t.astate to
    // decide whether we need to do a full lock. To make sure that we still observe any
    // timer update already in progress during Steps 1+2, t.modify sets timerModified
    // in t.astate *before* calling t.updateMinWhenModified. That ensures that the
    // overwrite in Step 2 cannot lose an update: if it does overwrite an update, Step 3
    // will see the timerModified and do a full lock.
    Ꮡts.of(timers.ᏑminWhenHeap).Store(Ꮡts.wakeTime());
    Ꮡts.of(timers.ᏑminWhenModified).Store(0);
    var changed = false;
    for (nint i = 0; i < len(ts.heap); i++) {
        var tw = Ꮡ(ts.heap, i);
        var t = tw.Value.timer;
        if ((~t).ts != Ꮡts) {
            @throw(badTsˢ);
        }
        if ((uint8)(t.of(timer.Ꮡastate).Load() & ((uint8)(timerModified | timerZombie))) == 0) {
            // Does not need adjustment.
            continue;
        }
        t.@lock();
        switch (ᐧ) {
        case {} when (uint8)((~t).state & timerHeaped) == 0: {
            badTimer();
            break;
        }
        case {} when (uint8)((~t).state & timerZombie) != 0: {
            Ꮡts.of(timers.Ꮡzombies).Add(-1);
            t.Value.state &= unchecked((uint8)~(uint8)((uint8)((uint8)(timerHeaped | timerZombie) | timerModified)));
            nint n = len(ts.heap);
            ts.heap[i] = ts.heap[n - 1];
            ts.heap[n - 1] = new timerWhen(nil);
            ts.heap = ts.heap[..(int)(n - 1)];
            t.Value.ts = default!;
            i--;
            changed = true;
            break;
        }
        case {} when (uint8)((~t).state & timerModified) != 0: {
            tw.Value.when = t.Value.when;
            t.Value.state &= unchecked((uint8)~(uint8)(timerModified));
            changed = true;
            break;
        }}

        t.unlock();
    }
    if (changed) {
        ts.initHeap();
    }
    Ꮡts.updateMinWhenHeap();
    if (verifyTimers) {
        Ꮡts.verify();
    }
}

// wakeTime looks at ts's timers and returns the time when we
// should wake up the netpoller. It returns 0 if there are no timers.
// This function is invoked when dropping a P, so it must run without
// any write barriers.
//
//go:nowritebarrierrec
internal static int64 wakeTime(this ж<timers> Ꮡts) {
    // Note that the order of these two loads matters:
    // adjust updates minWhen to make it safe to clear minNextWhen.
    // We read minWhen after reading minNextWhen so that
    // if we see a cleared minNextWhen, we are guaranteed to see
    // the updated minWhen.
    var nextWhen = Ꮡts.of(timers.ᏑminWhenModified).Load();
    var when = Ꮡts.of(timers.ᏑminWhenHeap).Load();
    if (when == 0 || (nextWhen != 0 && nextWhen < when)) {
        when = nextWhen;
    }
    return when;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string checkˢ = "check"u8;

// check runs any timers in ts that are ready.
// If now is not 0 it is the current time.
// It returns the passed time or the current time if now was passed as 0.
// and the time when the next timer should run or 0 if there is no next timer,
// and reports whether it ran any timers.
// If the time when the next timer should run is not 0,
// it is always larger than the returned time.
// We pass now in and out to avoid extra calls of nanotime.
//
//go:yeswritebarrierrec
internal static (int64 rnow, int64 pollUntil, bool ran) check(this ж<timers> Ꮡts, int64 now) {
    int64 pollUntil = default!;
    bool ran = default!;

    ref var ts = ref Ꮡts.DerefOrNull();
    Ꮡts.trace(checkˢ);
    // If it's not yet time for the first timer, or the first adjusted
    // timer, then there is nothing to do.
    var next = Ꮡts.wakeTime();
    if (next == 0) {
        // No timers to run or adjust.
        return (now, 0, false);
    }
    if (now == 0) {
        now = nanotime();
    }
    // If this is the local P, and there are a lot of deleted timers,
    // clear them out. We only do this for the local P to reduce
    // lock contention on timersLock.
    var zombies = Ꮡts.of(timers.Ꮡzombies).Load();
    if (zombies < 0) {
        badTimer();
    }
    var force = Ꮡts == (~(~getg()).m).p.ptr().of(runtime_package.Δp.Ꮡtimers) && (nint)zombies > (nint)Ꮡts.of(timers.Ꮡlen).Load() / 4;
    if (now < next && !force) {
        // Next timer is not ready to run, and we don't need to clear deleted timers.
        return (now, next, false);
    }
    Ꮡts.@lock();
    if (len(ts.heap) > 0) {
        Ꮡts.adjust(now, false);
        while (len(ts.heap) > 0) {
            // Note that runtimer may temporarily unlock ts.
            {
                var tw = Ꮡts.run(now); if (tw != 0) {
                    if (tw > 0) {
                        pollUntil = tw;
                    }
                    break;
                }
            }
            ran = true;
        }
        // Note: Delaying the forced adjustment until after the ts.run
        // (as opposed to calling ts.adjust(now, force) above)
        // is significantly faster under contention, such as in
        // package time's BenchmarkTimerAdjust10000,
        // though we do not fully understand why.
        force = Ꮡts == (~(~getg()).m).p.ptr().of(runtime_package.Δp.Ꮡtimers) && (nint)Ꮡts.of(timers.Ꮡzombies).Load() > (nint)Ꮡts.of(timers.Ꮡlen).Load() / 4;
        if (force) {
            Ꮡts.adjust(now, true);
        }
    }
    Ꮡts.unlock();
    return (now, pollUntil, ran);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runˢ = "run"u8;

// run examines the first timer in ts. If it is ready based on now,
// it runs the timer and removes or updates it.
// Returns 0 if it ran a timer, -1 if there are no more timers, or the time
// when the first timer should run.
// The caller must have locked ts.
// If a timer is run, this will temporarily unlock ts.
//
//go:systemstack
internal static int64 run(this ж<timers> Ꮡts, int64 now) {
    ref var ts = ref Ꮡts.DerefOrNull();

    Ꮡts.trace(runˢ);
    assertLockHeld(Ꮡts.of(timers.Ꮡmu));
Redo:
    if (len(ts.heap) == 0) {
        return -1;
    }
    var tw = ts.heap[0];
    var t = tw.timer;
    if ((~t).ts != Ꮡts) {
        @throw(badTsˢ);
    }
    if ((uint8)(t.of(timer.Ꮡastate).Load() & ((uint8)(timerModified | timerZombie))) == 0 && tw.when > now) {
        // Fast path: not ready to run.
        return tw.when;
    }
    t.@lock();
    if (t.updateHeap()) {
        t.unlock();
        goto Redo;
    }
    if ((uint8)((~t).state & timerHeaped) == 0 || (uint8)((~t).state & timerModified) != 0) {
        badTimer();
    }
    if ((~t).when > now) {
        // Not ready to run.
        t.unlock();
        return (~t).when;
    }
    t.unlockAndRun(now);
    assertLockHeld(Ꮡts.of(timers.Ꮡmu)); // t is unlocked now, but not ts
    return 0;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unlockAndRunˢ = "unlockAndRun"u8;
internal static readonly @string tooManyConcurrentTimerˢ = "too many concurrent timer firings"u8;
internal static readonly @string unexpectedRacectxˢ = "unexpected racectx"u8;
internal static readonly @string mismatchedIsSendingˢ = "mismatched isSending updates"u8;

// unlockAndRun unlocks and runs the timer t (which must be locked).
// If t is in a timer set (t.ts != nil), the caller must also have locked the timer set,
// and this call will temporarily unlock the timer set while running the timer function.
// unlockAndRun returns with t unlocked and t.ts (re-)locked.
//
//go:systemstack
internal static void unlockAndRun(this ж<timer> Ꮡt, int64 now) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.trace(unlockAndRunˢ);
    assertLockHeld(Ꮡt.of(timer.Ꮡmu));
    if (t.ts != nil) {
        assertLockHeld(t.ts.of(timers.Ꮡmu));
    }
    if (raceenabled) {
        // Note that we are running on a system stack,
        // so there is no chance of getg().m being reassigned
        // out from under us while this function executes.
        var tsLocal = (~(~getg()).m).p.ptr().of(runtime_package.Δp.Ꮡtimers);
        if ((~tsLocal).raceCtx == 0) {
            tsLocal.Value.raceCtx = racegostart(abi.FuncPCABIInternal(((Func<ж<timers>, int64, int64>)(run))) + (uintptr)sys.PCQuantum);
        }
        raceacquirectx((~tsLocal).raceCtx, (uintptr)@unsafe.Pointer.FromRef(ref t));
    }
    if ((uint8)(t.state & ((uint8)(timerModified | timerZombie))) != 0) {
        badTimer();
    }
    var f = t.f;
    var arg = t.arg;
    var seq = t.seq;
    int64 next = default!;
    var delay = now - t.when;
    if (t.period > 0){
        // Leave in heap but adjust next time to fire.
        next = t.when + t.period * (1 + delay / t.period);
        if (next < 0) {
            // check for overflow.
            next = maxWhen;
        }
    } else {
        next = 0;
    }
    var ts = t.ts;
    t.when = next;
    if ((uint8)(t.state & timerHeaped) != 0) {
        t.state |= (uint8)(timerModified);
        if (next == 0) {
            t.state |= (uint8)(timerZombie);
            t.ts.of(timers.Ꮡzombies).Add(1);
        }
        Ꮡt.updateHeap();
    }
    var async = Ꮡdebug.of(debugᴛ1.Ꮡasynctimerchan).Load() != 0;
    if (!async && t.isChan && t.period == 0) {
        // Tell Stop/Reset that we are sending a value.
        if (Ꮡt.of(timer.ᏑisSending).Add(1) < 0) {
            @throw(tooManyConcurrentTimerˢ);
        }
    }
    Ꮡt.unlock();
    if (raceenabled) {
        // Temporarily use the current P's racectx for g0.
        var gp = getg();
        if ((~gp).racectx != 0) {
            @throw(unexpectedRacectxˢ);
        }
        gp.Value.racectx = (~(~gp).m).p.ptr().Value.timers.raceCtx;
    }
    if (ts != nil) {
        ts.unlock();
    }
    if (!async && t.isChan) {
        // For a timer channel, we want to make sure that no stale sends
        // happen after a t.stop or t.modify, but we cannot hold t.mu
        // during the actual send (which f does) due to lock ordering.
        // It can happen that we are holding t's lock above, we decide
        // it's time to send a time value (by calling f), grab the parameters,
        // unlock above, and then a t.stop or t.modify changes the timer
        // and returns. At that point, the send needs not to happen after all.
        // The way we arrange for it not to happen is that t.stop and t.modify
        // both increment t.seq while holding both t.mu and t.sendLock.
        // We copied the seq value above while holding t.mu.
        // Now we can acquire t.sendLock (which will be held across the send)
        // and double-check that t.seq is still the seq value we saw above.
        // If not, the timer has been updated and we should skip the send.
        // We skip the send by reassigning f to a no-op function.
        //
        // The isSending field tells t.stop or t.modify that we have
        // started to send the value. That lets them correctly return
        // true meaning that no value was sent.
        @lock(Ꮡt.of(timer.ᏑsendLock));
        if (t.period == 0) {
            // We are committed to possibly sending a value
            // based on seq, so no need to keep telling
            // stop/modify that we are sending.
            if (Ꮡt.of(timer.ᏑisSending).Add(-1) < 0) {
                @throw(mismatchedIsSendingˢ);
            }
        }
        if (t.seq != seq) {
            f = (any _Δp0, uintptr _Δp1, int64 _Δp2) => {
            };
        }
    }
    f(arg, seq, delay);
    if (!async && t.isChan) {
        unlock(Ꮡt.of(timer.ᏑsendLock));
    }
    if (ts != nil) {
        ts.@lock();
    }
    if (raceenabled) {
        var gp = getg();
        gp.Value.racectx = 0;
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string badTimerHeapˢ = "bad timer heap"u8;
internal static readonly @string badTimerHeapLenˢ = "bad timer heap len"u8;

// verifyTimerHeap verifies that the timers is in a valid state.
// This is only for debugging, and is only called if verifyTimers is true.
// The caller must have locked ts.
internal static void verify(this ж<timers> Ꮡts) {
    ref var ts = ref Ꮡts.DerefOrNull();

    assertLockHeld(Ꮡts.of(timers.Ꮡmu));
    foreach (var (i, tw) in ts.heap) {
        if (i == 0) {
            // First timer has no parent.
            continue;
        }
        // The heap is timerHeapN-ary. See siftupTimer and siftdownTimer.
        nint Δp = (nint)((nuint)(i - 1) / (nuint)timerHeapN);
        if (tw.when < ts.heap[Δp].when) {
            print((@string)"bad timer heap at "u8, i, (@string)": "u8, Δp, (@string)": "u8, ts.heap[Δp].when, (@string)", "u8, i, (@string)": "u8, tw.when, (@string)"\n"u8);
            @throw(badTimerHeapˢ);
        }
    }
    {
        nint n = (nint)Ꮡts.of(timers.Ꮡlen).Load(); if (len(ts.heap) != n) {
            println((@string)"timer heap len"u8, len(ts.heap), (@string)"!= atomic len"u8, n);
            @throw(badTimerHeapLenˢ);
        }
    }
}

// updateMinWhenHeap sets ts.minWhenHeap to ts.heap[0].when.
// The caller must have locked ts or the world must be stopped.
internal static void updateMinWhenHeap(this ж<timers> Ꮡts) {
    ref var ts = ref Ꮡts.DerefOrNull();

    assertWorldStoppedOrLockHeld(ref nonnil(ref ts).mu);
    if (len(ts.heap) == 0){
        Ꮡts.of(timers.ᏑminWhenHeap).Store(0);
    } else {
        Ꮡts.of(timers.ᏑminWhenHeap).Store(ts.heap[0].when);
    }
}

// updateMinWhenModified updates ts.minWhenModified to be <= when.
// ts need not be (and usually is not) locked.
internal static void updateMinWhenModified(this ж<timers> Ꮡts, int64 when) {
    while (ᐧ) {
        var old = Ꮡts.of(timers.ᏑminWhenModified).Load();
        if (old != 0 && old < when) {
            return;
        }
        if (Ꮡts.of(timers.ᏑminWhenModified).CompareAndSwap(old, when)) {
            return;
        }
    }
}

// timeSleepUntil returns the time when the next timer should fire. Returns
// maxWhen if there are no timers.
// This is only called by sysmon and checkdead.
internal static int64 timeSleepUntil() {
    var next = (int64)maxWhen;
    // Prevent allp slice changes. This is like retake.
    @lock(ᏑallpLock);
    foreach (var (_, pp) in allp) {
        if (pp == nil) {
            // This can happen if procresize has grown
            // allp but not yet created new Ps.
            continue;
        }
        {
            var w = pp.of(runtime_package.Δp.Ꮡtimers).wakeTime(); if (w != 0) {
                next = min(next, w);
            }
        }
    }
    unlock(ᏑallpLock);
    return next;
}

internal static UntypedInt timerHeapN => 4;

// Heap maintenance algorithms.
// These algorithms check for slice index errors manually.
// Slice index error can happen if the program is using racy
// access to timers. We don't want to panic here, because
// it will cause the program to crash with a mysterious
// "panic holding locks" message. Instead, we panic while not
// holding a lock.

// siftUp puts the timer at position i in the right place
// in the heap by moving it up toward the top of the heap.
[GoRecv] internal static void siftUp(this ref timers ts, nint i) {
    var heap = ts.heap;
    if (i >= len(heap)) {
        badTimer();
    }
    var tw = heap[i];
    var when = tw.when;
    if (when <= 0) {
        badTimer();
    }
    while (i > 0) {
        nint Δp = (nint)((nuint)(i - 1) / (nuint)timerHeapN); // parent
        if (when >= heap[Δp].when) {
            break;
        }
        heap[i] = heap[Δp];
        i = Δp;
    }
    if (heap[i].timer != tw.timer) {
        heap[i] = tw;
    }
}

// siftDown puts the timer at position i in the right place
// in the heap by moving it down toward the bottom of the heap.
[GoRecv] internal static void siftDown(this ref timers ts, nint i) {
    var heap = ts.heap;
    nint n = len(heap);
    if (i >= n) {
        badTimer();
    }
    if (i * (nint)timerHeapN + 1 >= n) {
        return;
    }
    var tw = heap[i];
    var when = tw.when;
    if (when <= 0) {
        badTimer();
    }
    while (ᐧ) {
        nint leftChild = i * (nint)timerHeapN + 1;
        if (leftChild >= n) {
            break;
        }
        var w = when;
        nint c = -1;
        foreach (var (j, twΔ1) in heap[(int)(leftChild)..(int)(min(leftChild + (nint)timerHeapN, n))]) {
            if (twΔ1.when < w) {
                w = twΔ1.when;
                c = leftChild + j;
            }
        }
        if (c < 0) {
            break;
        }
        heap[i] = heap[c];
        i = c;
    }
    if (heap[i].timer != tw.timer) {
        heap[i] = tw;
    }
}

// initHeap reestablishes the heap order in the slice ts.heap.
// It takes O(n) time for n=len(ts.heap), not the O(n log n) of n repeated add operations.
[GoRecv] internal static void initHeap(this ref timers ts) {
    // Last possible element that needs sifting down is parent of last element;
    // last element is len(t)-1; parent of last element is (len(t)-1-1)/timerHeapN.
    if (len(ts.heap) <= 1) {
        return;
    }
    for (nint i = (nint)((nuint)(len(ts.heap) - 1 - 1) / (nuint)timerHeapN); i >= 0; i--) {
        ts.siftDown(i);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timerDataCorruptionˢ = "timer data corruption"u8;

// badTimer is called if the timer data structures have been corrupted,
// presumably due to racy use by the program. We panic here rather than
// panicking due to invalid slice access while holding locks.
// See issue #25686.
internal static void badTimer() {
    @throw(timerDataCorruptionˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string maybeRunChanˢ = "maybeRunChan-"u8;
internal static readonly @string maybeRunChanˢ2 = "maybeRunChan+"u8;

// Timer channels.

// maybeRunChan checks whether the timer needs to run
// to send a value to its associated channel. If so, it does.
// The timer must not be locked.
internal static void maybeRunChan(this ж<timer> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if ((uint8)(Ꮡt.of(timer.Ꮡastate).Load() & timerHeaped) != 0) {
        // If the timer is in the heap, the ordinary timer code
        // is in charge of sending when appropriate.
        return;
    }
    Ꮡt.@lock();
    var now = nanotime();
    if ((uint8)(t.state & timerHeaped) != 0 || t.when == 0 || t.when > now) {
        Ꮡt.trace(maybeRunChanˢ);
        // Timer in the heap, or not running at all, or not triggered.
        Ꮡt.unlock();
        return;
    }
    Ꮡt.trace(maybeRunChanˢ2);
    systemstack(() => {
        Ꮡt.unlockAndRun(now);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string blockTimerChanˢ = "blockTimerChan"u8;

// blockTimerChan is called when a channel op has decided to block on c.
// The caller holds the channel lock for c and possibly other channels.
// blockTimerChan makes sure that c is in a timer heap,
// adding it if needed.
internal static void blockTimerChan(ref Δhchan c) {
    var t = c.timer;
    t.@lock();
    t.trace(blockTimerChanˢ);
    if (!(~t).isChan) {
        badTimer();
    }
    t.Value.blocked++;
    // If this is the first enqueue after a recent dequeue,
    // the timer may still be in the heap but marked as a zombie.
    // Unmark it in this case, if the timer is still pending.
    if ((uint8)((~t).state & timerHeaped) != 0 && (uint8)((~t).state & timerZombie) != 0 && (~t).when > 0) {
        t.Value.state &= unchecked((uint8)~(uint8)(timerZombie));
        (~t).ts.of(timers.Ꮡzombies).Add(-1);
    }
    // t.maybeAdd must be called with t unlocked,
    // because it needs to lock t.ts before t.
    // Then it will do nothing if t.needsAdd(state) is false.
    // Check that now before the unlock,
    // avoiding the extra lock-lock-unlock-unlock
    // inside maybeAdd when t does not need to be added.
    var add = t.needsAdd();
    t.unlock();
    if (add) {
        t.maybeAdd();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unblockTimerChanˢ = "unblockTimerChan"u8;

// unblockTimerChan is called when a channel op that was blocked on c
// is no longer blocked. Every call to blockTimerChan must be paired with
// a call to unblockTimerChan.
// The caller holds the channel lock for c and possibly other channels.
// unblockTimerChan removes c from the timer heap when nothing is
// blocked on it anymore.
internal static void unblockTimerChan(ref Δhchan c) {
    var t = c.timer;
    t.@lock();
    t.trace(unblockTimerChanˢ);
    if (!(~t).isChan || (~t).blocked == 0) {
        badTimer();
    }
    t.Value.blocked--;
    if ((~t).blocked == 0 && (uint8)((~t).state & timerHeaped) != 0 && (uint8)((~t).state & timerZombie) == 0) {
        // Last goroutine that was blocked on this timer.
        // Mark for removal from heap but do not clear t.when,
        // so that we know what time it is still meant to trigger.
        t.Value.state |= (uint8)(timerZombie);
        (~t).ts.of(timers.Ꮡzombies).Add(1);
    }
    t.unlock();
}

} // end runtime_package
