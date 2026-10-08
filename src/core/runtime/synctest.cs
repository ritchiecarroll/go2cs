// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// A synctestGroup is a group of goroutines started by synctest.Run.
partial struct synctestGroup {
    internal mutex mu;
    internal timers timers;
    internal int64 now; // current fake time
    internal ж<g> root; // caller of synctest.Run
    internal ж<g> waiter; // caller of synctest.Wait
    internal bool waiting;  // true if a goroutine is calling synctest.Wait
    // The group is active (not blocked) so long as running > 0 || active > 0.
    //
    // running is the number of goroutines which are not "durably blocked":
    // Goroutines which are either running, runnable, or non-durably blocked
    // (for example, blocked in a syscall).
    //
    // active is used to keep the group from becoming blocked,
    // even if all goroutines in the group are blocked.
    // For example, park_m can choose to immediately unpark a goroutine after parking it.
    // It increments the active count to keep the group active until it has determined
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
internal static void changegstatus(this ж<synctestGroup> Ꮡsg, ж<g> Ꮡgp, uint32 oldval, uint32 newval) {
    ref var sg = ref Ꮡsg.DerefOrNull();
    ref var gp = ref Ꮡgp.DerefOrNull();

    // Determine whether this change in status affects the idleness of the group.
    // If this isn't a goroutine starting, stopping, durably blocking,
    // or waking up after durably blocking, then return immediately without
    // locking sg.mu.
    //
    // For example, stack growth (newstack) will changegstatus
    // from _Grunning to _Gcopystack. This is uninteresting to synctest,
    // but if stack growth occurs while sg.mu is held, we must not recursively lock.
    nint totalDelta = 0;
    var wasRunning = true;
    var exprᴛ1 = oldval;
    if (exprᴛ1 == _Gdead) {
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
    if (exprᴛ2 == _Gdead) {
        isRunning = false;
        totalDelta--;
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
    @lock(Ꮡsg.of(synctestGroup.Ꮡmu));
    sg.total += totalDelta;
    if (wasRunning != isRunning) {
        if (isRunning){
            sg.running++;
        } else {
            sg.running--;
            if (raceenabled && newval != _Gdead) {
                racereleasemergeg(ref (Ꮡgp).DerefOrNull(), (uintptr)Ꮡsg.raceaddr());
            }
        }
    }
    if (sg.total < 0) {
        fatal(total0ˢ);
    }
    if (sg.running < 0) {
        fatal(running0ˢ);
    }
    var wake = sg.maybeWakeLocked();
    unlock(Ꮡsg.of(synctestGroup.Ꮡmu));
    if (wake != nil) {
        goready(wake, 0);
    }
}

// incActive increments the active-count for the group.
// A group does not become durably blocked while the active-count is non-zero.
internal static void incActive(this ж<synctestGroup> Ꮡsg) {
    ref var sg = ref Ꮡsg.DerefOrNull();

    @lock(Ꮡsg.of(synctestGroup.Ꮡmu));
    sg.active++;
    unlock(Ꮡsg.of(synctestGroup.Ꮡmu));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string active0ˢ = "active < 0"u8;

// decActive decrements the active-count for the group.
internal static void decActive(this ж<synctestGroup> Ꮡsg) {
    ref var sg = ref Ꮡsg.DerefOrNull();

    @lock(Ꮡsg.of(synctestGroup.Ꮡmu));
    sg.active--;
    if (sg.active < 0) {
        @throw(active0ˢ);
    }
    var wake = sg.maybeWakeLocked();
    unlock(Ꮡsg.of(synctestGroup.Ꮡmu));
    if (wake != nil) {
        goready(wake, 0);
    }
}

// maybeWakeLocked returns a g to wake if the group is durably blocked.
internal static ж<g> maybeWakeLocked(this ref synctestGroup sg) {
    if (sg.running > 0 || sg.active > 0) {
        return default!;
    }
    // Increment the group active count, since we've determined to wake something.
    // The woken goroutine will decrement the count.
    // We can't just call goready and let it increment sg.running,
    // since we can't call goready with sg.mu held.
    //
    // Incrementing the active count here is only necessary if something has gone wrong,
    // and a goroutine that we considered durably blocked wakes up unexpectedly.
    // Two wakes happening at the same time leads to very confusing failure modes,
    // so we take steps to avoid it happening.
    sg.active++;
    {
        var gp = sg.waiter; if (gp != nil) {
            // A goroutine is blocked in Wait. Wake it.
            return gp;
        }
    }
    // All goroutines in the group are durably blocked, and nothing has called Wait.
    // Wake the root goroutine.
    return sg.root;
}

internal static @unsafe.Pointer raceaddr(this ж<synctestGroup> Ꮡsg) {
    ref var sg = ref Ꮡsg.DerefOrNull();

    // Address used to record happens-before relationships created by the group.
    //
    // Wait creates a happens-before relationship between itself and
    // the blocking operations which caused other goroutines in the group to park.
    return (uintptr)@unsafe.Pointer.FromRef(ref sg);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string timeWentBackwardsˢ = "time went backwards"u8;
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
        if ((~gp).syncGroup != nil) {
            throw panic("synctest.Run called from within a synctest bubble");
        }
        gp.Value.syncGroup = Ꮡ(new synctestGroup(
            total: 1,
            running: 1,
            root: gp
        ));
        const int64 synctestBaseTime = 946684800000000000; // midnight UTC 2000-01-01
        gp.Value.syncGroup.Value.now = synctestBaseTime;
        gp.Value.syncGroup.Value.timers.syncGroup = gp.Value.syncGroup;
        lockInit((~gp).syncGroup.of(synctestGroup.Ꮡmu), lockRankSynctest);
        lockInit((~gp).syncGroup.of(synctestGroup.Ꮡtimers).of(timers.Ꮡmu), lockRankTimers);
        var gpʗ1 = gp;
        defer(() => {
            gpʗ1.Value.syncGroup = default!;
        }, ref ᒐ);
        var fv = ~Ꮡf.Reinterpret<Action, ж<funcval>>();
        newproc(fv);
        var sg = gp.Value.syncGroup;
        @lock(sg.of(synctestGroup.Ꮡmu));
        sg.Value.active++;
        while (ᐧ) {
            if (raceenabled) {
                raceacquireg(ref (gp).DerefOrNull(), (uintptr)(~gp).syncGroup.raceaddr());
            }
            unlock(sg.of(synctestGroup.Ꮡmu));
            var gpʗ2 = gp;
            systemstack(() => {
                (~gpʗ2).syncGroup.of(synctestGroup.Ꮡtimers).check((~(~gpʗ2).syncGroup).now);
            });
            gopark(synctestidle_c, nil, waitReasonSynctestRun, traceBlockSynctest, 0);
            @lock(sg.of(synctestGroup.Ꮡmu));
            if ((~sg).active < 0) {
                @throw(active0ˢ);
            }
            var next = sg.of(synctestGroup.Ꮡtimers).wakeTime();
            if (next == 0) {
                break;
            }
            if (next < (~sg).now) {
                @throw(timeWentBackwardsˢ);
            }
            sg.Value.now = next;
        }
        nint total = sg.Value.total;
        unlock(sg.of(synctestGroup.Ꮡmu));
        if (total != 1) {
            throw panic("deadlock: all goroutines in bubble are blocked");
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

internal static bool synctestidle_c(ж<g> Ꮡgp, @unsafe.Pointer _) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    @lock(gp.syncGroup.of(synctestGroup.Ꮡmu));
    var canIdle = true;
    if ((~gp.syncGroup).running == 0 && (~gp.syncGroup).active == 1){
        // All goroutines in the group have blocked or exited.
        canIdle = false;
    } else {
        gp.syncGroup.Value.active--;
    }
    unlock(gp.syncGroup.of(synctestGroup.Ꮡmu));
    return canIdle;
}

//go:linkname synctestWait internal/synctest.Wait
internal static void synctestWait() {
    var gp = getg();
    if ((~gp).syncGroup == nil) {
        throw panic("goroutine is not in a bubble");
    }
    @lock((~gp).syncGroup.of(synctestGroup.Ꮡmu));
    // We use a syncGroup.waiting bool to detect simultaneous calls to Wait rather than
    // checking to see if syncGroup.waiter is non-nil. This avoids a race between unlocking
    // syncGroup.mu and setting syncGroup.waiter while parking.
    if ((~(~gp).syncGroup).waiting) {
        unlock((~gp).syncGroup.of(synctestGroup.Ꮡmu));
        throw panic("wait already in progress");
    }
    gp.Value.syncGroup.Value.waiting = true;
    unlock((~gp).syncGroup.of(synctestGroup.Ꮡmu));
    gopark(synctestwait_c, nil, waitReasonSynctestWait, traceBlockSynctest, 0);
    @lock((~gp).syncGroup.of(synctestGroup.Ꮡmu));
    gp.Value.syncGroup.Value.active--;
    if ((~(~gp).syncGroup).active < 0) {
        @throw(active0ˢ);
    }
    gp.Value.syncGroup.Value.waiter = default!;
    gp.Value.syncGroup.Value.waiting = false;
    unlock((~gp).syncGroup.of(synctestGroup.Ꮡmu));
    // Establish a happens-before relationship on the activity of the now-blocked
    // goroutines in the group.
    if (raceenabled) {
        raceacquireg(ref (gp).DerefOrNull(), (uintptr)(~gp).syncGroup.raceaddr());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string running0Active0ˢ = "running == 0 && active == 0"u8;

internal static bool synctestwait_c(ж<g> Ꮡgp, @unsafe.Pointer _) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    @lock(gp.syncGroup.of(synctestGroup.Ꮡmu));
    if ((~gp.syncGroup).running == 0 && (~gp.syncGroup).active == 0) {
        // This shouldn't be possible, since gopark increments active during unlockf.
        @throw(running0Active0ˢ);
    }
    gp.syncGroup.Value.waiter = Ꮡgp;
    unlock(gp.syncGroup.of(synctestGroup.Ꮡmu));
    return true;
}

//go:linkname synctest_acquire internal/synctest.acquire
internal static any synctest_acquire() {
    {
        var sg = getg().Value.syncGroup; if (sg != nil) {
            sg.incActive();
            return sg.OrTypedNil();
        }
    }
    return default!;
}

//go:linkname synctest_release internal/synctest.release
internal static void synctest_release(any sg) {
    sg._<ж<synctestGroup>>().decActive();
}

//go:linkname synctest_inBubble internal/synctest.inBubble
internal static void synctest_inBubble(any sg, Action f) {
    GoFrame ᒐ = default;
    try {
        var gp = getg();
        if ((~gp).syncGroup != nil) {
            throw panic("goroutine is already bubbled");
        }
        gp.Value.syncGroup = sg._<ж<synctestGroup>>();
        var gpʗ1 = gp;
        defer(() => {
            gpʗ1.Value.syncGroup = default!;
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_package
