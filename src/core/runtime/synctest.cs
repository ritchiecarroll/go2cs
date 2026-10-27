// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;

[GoFieldAccess(typeof(specialBubble))]
partial class runtime_package {

// A synctestBubble is a set of goroutines started by synctest.Run.
partial struct synctestBubble {
    internal mutex mu;
    internal timers timers;
    internal uint64 id; // unique id
    internal int64 now;  // current fake time
    internal ж<g> root;  // caller of synctest.Run
    internal ж<g> waiter;  // caller of synctest.Wait
    internal ж<g> main;  // goroutine started by synctest.Run
    internal bool waiting;   // true if a goroutine is calling synctest.Wait
    internal bool done;   // true if main has exited
    // The bubble is active (not blocked) so long as running > 0 || active > 0.
    //
    // running is the number of goroutines which are not "durably blocked":
    // Goroutines which are either running, runnable, or non-durably blocked
    // (for example, blocked in a syscall).
    //
    // active is used to keep the bubble from becoming blocked,
    // even if all goroutines in the bubble are blocked.
    // For example, park_m can choose to immediately unpark a goroutine after parking it.
    // It increments the active count to keep the bubble active until it has determined
    // that the park operation has completed.
    internal nint total; // total goroutines
    internal nint running; // non-blocked goroutines
    internal nint active; // other sources of activity
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string total0ˢ = "total < 0"u8;
internal static readonly @string running0ˢ = "running < 0"u8;

// changegstatus is called when the non-lock status of a g changes.
// It is never called with a Gscanstatus.
internal static void changegstatus(this ж<synctestBubble> Ꮡbubble, ж<g> Ꮡgp, uint32 oldval, uint32 newval) {
    ref var bubble = ref Ꮡbubble.DerefOrNull();
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Determine whether this change in status affects the idleness of the bubble.
    // If this isn't a goroutine starting, stopping, durably blocking,
    // or waking up after durably blocking, then return immediately without
    // locking bubble.mu.
    //
    // For example, stack growth (newstack) will changegstatus
    // from _Grunning to _Gcopystack. This is uninteresting to synctest,
    // but if stack growth occurs while bubble.mu is held, we must not recursively lock.
    nint totalDelta = 0;
    var wasRunning = true;
    var exprᴛ1 = oldval;
    if (exprᴛ1 == _Gdead || exprᴛ1 == _Gdeadextra) {
        wasRunning = false;
        totalDelta++;
    }
    else if (exprᴛ1 == _Gwaiting) {
        if (gp.waitreason.isIdleInSynctest()) {
            wasRunning = false;
        }
    }
    var isRunning = true;
    var exprᴛ2 = newval;
    if (exprᴛ2 == _Gdead || exprᴛ2 == _Gdeadextra) {
        isRunning = false;
        totalDelta--;
        if (Ꮡgp == bubble.main) {
            bubble.done = true;
        }
    }
    else if (exprᴛ2 == _Gwaiting) {
        if (gp.waitreason.isIdleInSynctest()) {
            isRunning = false;
        }
    }
    // It's possible for wasRunning == isRunning while totalDelta != 0;
    // for example, if a new goroutine is created in a non-running state.
    if (wasRunning == isRunning && totalDelta == 0) {
        return;
    }

    @lock(Ꮡbubble.of(synctestBubble.Ꮡmu));
    bubble.total += totalDelta;
    if (wasRunning != isRunning) {
        if (isRunning){
            bubble.running++;
        } else {
            bubble.running--;
            if (raceenabled && newval != _Gdead && newval != _Gdeadextra) {
                // Record that this goroutine parking happens before
                // any subsequent Wait.
                racereleasemergeg(ref (Ꮡgp).DerefOrNull(), (uintptr)Ꮡbubble.raceaddr());
            }
        }
    }
    if (bubble.total < 0) {
        fatal(total0ˢ);
    }
    if (bubble.running < 0) {
        fatal(running0ˢ);
    }
    var wake = Ꮡbubble.maybeWakeLocked();
    unlock(Ꮡbubble.of(synctestBubble.Ꮡmu));
    if (wake != nil) {
        goready(wake, 0);
    }
}

// incActive increments the active-count for the bubble.
// A bubble does not become durably blocked while the active-count is non-zero.
internal static void incActive(this ж<synctestBubble> Ꮡbubble) {
    ref var bubble = ref Ꮡbubble.DerefOrNull();

    @lock(Ꮡbubble.of(synctestBubble.Ꮡmu));
    bubble.active++;
    unlock(Ꮡbubble.of(synctestBubble.Ꮡmu));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string active0ˢ = "active < 0"u8;

// decActive decrements the active-count for the bubble.
internal static void decActive(this ж<synctestBubble> Ꮡbubble) {
    ref var bubble = ref Ꮡbubble.DerefOrNull();

    @lock(Ꮡbubble.of(synctestBubble.Ꮡmu));
    bubble.active--;
    if (bubble.active < 0) {
        @throw(active0ˢ);
    }
    var wake = Ꮡbubble.maybeWakeLocked();
    unlock(Ꮡbubble.of(synctestBubble.Ꮡmu));
    if (wake != nil) {
        goready(wake, 0);
    }
}

// maybeWakeLocked returns a g to wake if the bubble is durably blocked.
internal static ж<g> maybeWakeLocked(this ж<synctestBubble> Ꮡbubble) {
    ref var bubble = ref Ꮡbubble.DerefOrNull();

    if (bubble.running > 0 || bubble.active > 0) {
        return default!;
    }
    // Increment the bubble active count, since we've determined to wake something.
    // The woken goroutine will decrement the count.
    // We can't just call goready and let it increment bubble.running,
    // since we can't call goready with bubble.mu held.
    //
    // Incrementing the active count here is only necessary if something has gone wrong,
    // and a goroutine that we considered durably blocked wakes up unexpectedly.
    // Two wakes happening at the same time leads to very confusing failure modes,
    // so we take steps to avoid it happening.
    bubble.active++;
    var next = Ꮡbubble.of(synctestBubble.Ꮡtimers).wakeTime();
    if (next > 0 && next <= bubble.now) {
        // A timer is scheduled to fire. Wake the root goroutine to handle it.
        return bubble.root;
    }
    {
        var gp = bubble.waiter; if (gp != nil) {
            // A goroutine is blocked in Wait. Wake it.
            return gp;
        }
    }
    // All goroutines in the bubble are durably blocked, and nothing has called Wait.
    // Wake the root goroutine.
    return bubble.root;
}

internal static @unsafe.Pointer raceaddr(this ж<synctestBubble> Ꮡbubble) {
    ref var bubble = ref Ꮡbubble.DerefOrNull();

    // Address used to record happens-before relationships created by the bubble.
    //
    // Wait creates a happens-before relationship between itself and
    // the blocking operations which caused other goroutines in the bubble to park.
    return (uintptr)@unsafe.Pointer.FromRef(ref bubble);
}

internal static readonly ж<atomic.Uint64> ᏑbubbleGen = new StaticBox<atomic.Uint64>(default(atomic.Uint64));
internal static ref atomic.Uint64 bubbleGen => ref ((StaticBox<atomic.Uint64>)ᏑbubbleGen).ValueSlot;     // bubble ID counter

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timeWentBackwardsˢ = "time went backwards"u8;
internal static readonly @string deadlockMainBubbleˢ = "deadlock: main bubble goroutine has exited but blocked goroutines remain"u8;
internal static readonly @string deadlockAllGoroutinesInˢ = "deadlock: all goroutines in bubble are blocked"u8;
internal static readonly @string synctestRootGoroutineHasˢ = "synctest root goroutine has a fake timer"u8;

//go:linkname synctestRun internal/synctest.Run
internal static void synctestRun(Action fʗp) {
    GoFrame ᒐ = default;
    try {
        ref var f = ref heap(fʗp, out var Ꮡf);

        if (Ꮡdebug.of(debugᴛ1.Ꮡasynctimerchan).Load() != 0) {
            throw panic("synctest.Run not supported with asynctimerchan!=0");
        }

        var gp = getg();
        if ((~gp).bubble != nil) {
            throw panic("synctest.Run called from within a synctest bubble");
        }
        var bubble = Ꮡ(new synctestBubble(
            id: ᏑbubbleGen.Add(1),
            total: 1,
            running: 1,
            root: gp
        ));
        const int64 synctestBaseTime = 946684800000000000; // midnight UTC 2000-01-01
        bubble.Value.now = synctestBaseTime;
        lockInit(ref (bubble.of(synctestBubble.Ꮡmu)).DerefOrNull(), lockRankSynctest);
        lockInit(ref (bubble.of(synctestBubble.Ꮡtimers).of(timers.Ꮡmu)).DerefOrNull(), lockRankTimers);

        gp.Value.bubble = bubble;
        var gpʗ1 = gp;
        defer(() => {
            gpʗ1.Value.bubble = default!;
        }, ref ᒐ);

        // This is newproc, but also records the new g in bubble.main.
        var pc = sys.GetCallerPC();
        var bubbleʗ1 = bubble;
        var gpʗ2 = gp;
        systemstack(() => {
            var fv = ~Ꮡf.Reinterpret<Action, ж<funcval>>();
            bubbleʗ1.Value.main = newproc1(fv, gpʗ2, pc, false, waitReasonZero);
            var pp = (~(~getg()).m).p.ptr();
            runqput(pp, (~bubbleʗ1).main, true);
            wakep();
        });

        @lock(bubble.of(synctestBubble.Ꮡmu));
        bubble.Value.active++;
        while (ᐧ) {
            unlock(bubble.of(synctestBubble.Ꮡmu));
            var bubbleʗ2 = bubble;
            var gpʗ3 = gp;
            systemstack(() => {
                // Clear gp.m.curg while running timers,
                // so timer goroutines inherit their child race context from g0.
                var curg = gpʗ3.Value.m.Value.curg;
                gpʗ3.Value.m.Value.curg = default!;
                (~gpʗ3).bubble.of(synctestBubble.Ꮡtimers).check((~bubbleʗ2).now, bubbleʗ2);
                gpʗ3.Value.m.Value.curg = curg;
            });
            gopark(synctestidle_c, nil, waitReasonSynctestRun, traceBlockSynctest, 0);
            @lock(bubble.of(synctestBubble.Ꮡmu));
            if ((~bubble).active < 0) {
                @throw(active0ˢ);
            }
            var next = bubble.of(synctestBubble.Ꮡtimers).wakeTime();
            if (next == 0) {
                break;
            }
            if (next < (~bubble).now) {
                @throw(timeWentBackwardsˢ);
            }
            if ((~bubble).done) {
                // Time stops once the bubble's main goroutine has exited.
                break;
            }
            bubble.Value.now = next;
        }

        nint total = bubble.Value.total;
        unlock(bubble.of(synctestBubble.Ꮡmu));
        if (raceenabled) {
            // Establish a happens-before relationship between bubbled goroutines exiting
            // and Run returning.
            raceacquireg(ref (gp).DerefOrNull(), (uintptr)(~gp).bubble.raceaddr());
        }
        if (total != 1) {
            @string reason = default!;
            if ((~bubble).done){
                reason = deadlockMainBubbleˢ;
            } else {
                reason = deadlockAllGoroutinesInˢ;
            }
            throw panic(new synctestDeadlockError(reason: reason, bubble: bubble));
        }
        if ((~gp).timer != nil && (~(~gp).timer).isFake) {
            // Verify that we haven't marked this goroutine's sleep timer as fake.
            // This could happen if something in Run were to call timeSleep.
            @throw(synctestRootGoroutineHasˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct synctestDeadlockError {
    internal @string reason;
    internal ж<synctestBubble> bubble;
}

internal static @string Error(this synctestDeadlockError e) {
    return e.reason;
}

internal static bool synctestidle_c(ж<g> Ꮡgp, @unsafe.Pointer _) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    @lock(gp.bubble.of(synctestBubble.Ꮡmu));
    var canIdle = true;
    if ((~gp.bubble).running == 0 && (~gp.bubble).active == 1){
        // All goroutines in the bubble have blocked or exited.
        canIdle = false;
    } else {
        gp.bubble.Value.active--;
    }
    unlock(gp.bubble.of(synctestBubble.Ꮡmu));
    return canIdle;
}

//go:linkname synctestWait internal/synctest.Wait
internal static void synctestWait() {
    var gp = getg();
    if ((~gp).bubble == nil) {
        throw panic("goroutine is not in a bubble");
    }
    @lock((~gp).bubble.of(synctestBubble.Ꮡmu));
    // We use a bubble.waiting bool to detect simultaneous calls to Wait rather than
    // checking to see if bubble.waiter is non-nil. This avoids a race between unlocking
    // bubble.mu and setting bubble.waiter while parking.
    if ((~(~gp).bubble).waiting) {
        unlock((~gp).bubble.of(synctestBubble.Ꮡmu));
        throw panic("wait already in progress");
    }
    gp.Value.bubble.Value.waiting = true;
    unlock((~gp).bubble.of(synctestBubble.Ꮡmu));
    gopark(synctestwait_c, nil, waitReasonSynctestWait, traceBlockSynctest, 0);

    @lock((~gp).bubble.of(synctestBubble.Ꮡmu));
    gp.Value.bubble.Value.active--;
    if ((~(~gp).bubble).active < 0) {
        @throw(active0ˢ);
    }
    gp.Value.bubble.Value.waiter = default!;
    gp.Value.bubble.Value.waiting = false;
    unlock((~gp).bubble.of(synctestBubble.Ꮡmu));

    // Establish a happens-before relationship on the activity of the now-blocked
    // goroutines in the bubble.
    if (raceenabled) {
        raceacquireg(ref (gp).DerefOrNull(), (uintptr)(~gp).bubble.raceaddr());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string running0Active0ˢ = "running == 0 && active == 0"u8;

internal static bool synctestwait_c(ж<g> Ꮡgp, @unsafe.Pointer _) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    @lock(gp.bubble.of(synctestBubble.Ꮡmu));
    if ((~gp.bubble).running == 0 && (~gp.bubble).active == 0) {
        // This shouldn't be possible, since gopark increments active during unlockf.
        @throw(running0Active0ˢ);
    }
    gp.bubble.Value.waiter = Ꮡgp;
    unlock(gp.bubble.of(synctestBubble.Ꮡmu));
    return true;
}

//go:linkname synctest_isInBubble internal/synctest.IsInBubble
internal static bool synctest_isInBubble() {
    return (~getg()).bubble != nil;
}

//go:linkname synctest_acquire internal/synctest.acquire
internal static any synctest_acquire() {
    {
        var bubble = getg().Value.bubble; if (bubble != nil) {
            bubble.incActive();
            return bubble.OrTypedNil();
        }
    }
    return default!;
}

//go:linkname synctest_release internal/synctest.release
internal static void synctest_release(any bubble) {
    bubble._<ж<synctestBubble>>().decActive();
}

//go:linkname synctest_inBubble internal/synctest.inBubble
internal static void synctest_inBubble(any bubble, Action f) {
    GoFrame ᒐ = default;
    try {
        var gp = getg();
        if ((~gp).bubble != nil) {
            throw panic("goroutine is already bubbled");
        }
        gp.Value.bubble = bubble._<ж<synctestBubble>>();
        var gpʗ1 = gp;
        defer(() => {
            gpʗ1.Value.bubble = default!;
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// specialBubble is a special used to associate objects with bubbles.
partial struct specialBubble {
    internal sys.NotInHeap _;
    internal special special;
    internal uint64 bubbleid;
}

// Keep these in sync with internal/synctest.
internal static UntypedInt bubbleAssocUnbubbled => iota; // not associated with any bubble
internal static UntypedInt bubbleAssocCurrentBubble => 1; // associated with the current bubble
internal static UntypedInt bubbleAssocOtherBubble => 2; // associated with a different bubble

// getOrSetBubbleSpecial checks the special record for p's bubble membership.
//
// If add is true and p is not associated with any bubble,
// it adds a special record for p associating it with bubbleid.
//
// It returns ok==true if p is associated with bubbleid
// (including if a new association was added),
// and ok==false if not.
internal static nint /*assoc*/ getOrSetBubbleSpecial(@unsafe.Pointer Δp, uint64 bubbleid, bool add) {
    nint assoc = default!;

    var span = spanOfHeap((uintptr)Δp);
    if (span == nil) {
        // This is probably a package var.
        // We can't attach a special to it, so always consider it unbubbled.
        return bubbleAssocUnbubbled;
    }

    // Ensure that the span is swept.
    // Sweeping accesses the specials list w/o locks, so we have
    // to synchronize with it. And it's just much safer.
    var mp = acquirem();
    span.ensureSwept();

    var offset = (uintptr)Δp - span.@base();

    @lock(span.of(mspan.Ꮡspeciallock));

    // Find splice point, check for existing record.
    var (iter, exists) = span.specialFindSplicePoint(offset, _KindSpecialBubble);
    if (exists){
        // p is already associated with a bubble.
        // Return true iff it's the same bubble.
        var s = iter.ValueSlot.Reinterpret<special, specialBubble>();
        if (s.bubbleid == bubbleid){
            assoc = bubbleAssocCurrentBubble;
        } else {
            assoc = bubbleAssocOtherBubble;
        }
    } else 
    if (add){
        // p is not associated with a bubble,
        // and we've been asked to add an association.
        @lock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        var s = (ж<specialBubble>)(uintptr)(Ꮡmheap_.of(mheap.ᏑspecialBubbleAlloc).alloc());
        unlock(Ꮡmheap_.of(mheap.Ꮡspeciallock));
        s.bubbleid = bubbleid;
        s.special.kind = _KindSpecialBubble;
        s.special.offset = offset;
        s.special.next = iter.ValueSlot;
        iter.ValueSlot = s.Reinterpret<specialBubble, special>();
        spanHasSpecials(span);
        assoc = bubbleAssocCurrentBubble;
    } else {
        // p is not associated with a bubble.
        assoc = bubbleAssocUnbubbled;
    }

    unlock(span.of(mspan.Ꮡspeciallock));
    releasem(ref (mp).DerefOrNull());

    return assoc;
}

// synctest_associate associates p with the current bubble.
// It returns false if p is already associated with a different bubble.
//
//go:linkname synctest_associate internal/synctest.associate
internal static nint synctest_associate(@unsafe.Pointer Δp) {
    return getOrSetBubbleSpecial(Δp, (~(~getg()).bubble).id, true);
}

// synctest_disassociate disassociates p from its bubble.
//
//go:linkname synctest_disassociate internal/synctest.disassociate
internal static void synctest_disassociate(@unsafe.Pointer Δp) {
    removespecial(Δp, _KindSpecialBubble);
}

// synctest_isAssociated reports whether p is associated with the current bubble.
//
//go:linkname synctest_isAssociated internal/synctest.isAssociated
internal static bool synctest_isAssociated(@unsafe.Pointer Δp) {
    return getOrSetBubbleSpecial(Δp, (~(~getg()).bubble).id, false) == bubbleAssocCurrentBubble;
}

} // end runtime_package
