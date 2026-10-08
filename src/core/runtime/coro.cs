// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using sys = @internal.runtime.sys_package;
using @unsafe = unsafe_package;
using @internal.runtime;

partial class runtime_package {

// A coro represents extra concurrency without extra parallelism,
// as would be needed for a coroutine implementation.
// The coro does not represent a specific coroutine, only the ability
// to do coroutine-style control transfers.
// It can be thought of as like a special channel that always has
// a goroutine blocked on it. If another goroutine calls coroswitch(c),
// the caller becomes the goroutine blocked in c, and the goroutine
// formerly blocked in c starts running.
// These switches continue until a call to coroexit(c),
// which ends the use of the coro by releasing the blocked
// goroutine in c and exiting the current goroutine.
//
// Coros are heap allocated and garbage collected, so that user code
// can hold a pointer to a coro without causing potential dangling
// pointer errors.
partial struct coro {
    internal Δguintptr gp;
    internal Action<ж<coro>> f;
    // State for validating thread-lock interactions.
    internal ж<m> mp;
    internal uint32 lockedExt; // mp's external LockOSThread counter at coro creation time.
    internal uint32 lockedInt; // mp's internal lockOSThread counter at coro creation time.
}

//go:linkname newcoro

// newcoro creates a new coro containing a
// goroutine blocked waiting to run f
// and returns that coro.
internal static ж<coro> newcoro(Action<ж<coro>> f) {
    var c = @new<coro>();
    c.Value.f = f;
    var pc = sys.GetCallerPC();
    ref var gp = ref heap<ж<g>>(out var Ꮡgp);
    gp = getg();
    var cʗ1 = c;
    systemstack(() => {
        var mp = Ꮡgp.ValueSlot.Value.m;
        ref var start = ref heap<Action>(out var Ꮡstart);
        start = corostart;
        var startfv = ~Ꮡstart.Reinterpret<Action, ж<funcval>>();
        Ꮡgp.ValueSlot = newproc1(startfv, Ꮡgp.ValueSlot, pc, true, waitReasonCoroutine);
        // Scribble down locked thread state if needed and/or donate
        // thread-lock state to the new goroutine.
        if ((~mp).lockedExt + (~mp).lockedInt != 0) {
            cʗ1.Value.mp = mp;
            cʗ1.Value.lockedExt = mp.Value.lockedExt;
            cʗ1.Value.lockedInt = mp.Value.lockedInt;
        }
    });
    gp.Value.coroarg = c;
    c.of(coro.Ꮡgp).set(gp);
    return c;
}

// corostart is the entry func for a new coroutine.
// It runs the coroutine user function f passed to corostart
// and then calls coroexit to remove the extra concurrency.
internal static void corostart() {
    GoFrame ᒐ = default;
    try {
        var gp = getg();
        var c = gp.Value.coroarg;
        gp.Value.coroarg = default!;
        defer(coroexit, c, ref ᒐ);
        (~c).f(c);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// coroexit is like coroswitch but closes the coro
// and exits the current goroutine
internal static void coroexit(ж<coro> Ꮡc) {
    var gp = getg();
    gp.Value.coroarg = Ꮡc;
    gp.Value.coroexit = true;
    mcall(coroswitch_m);
}

//go:linkname coroswitch

// coroswitch switches to the goroutine blocked on c
// and then blocks the current goroutine on c.
internal static void coroswitch(ж<coro> Ꮡc) {
    var gp = getg();
    gp.Value.coroarg = Ꮡc;
    mcall(coroswitch_m);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string coroOsThreadLockingMustˢ = "coro: OS thread locking must match locking at coroutine creation"u8;
internal static readonly @string coroswitchOnExitedCoroˢ = "coroswitch on exited coro"u8;
internal static readonly @string coroswitchOfAGoroutineToˢ = "coroswitch of a goroutine to itself"u8;

// coroswitch_m is the implementation of coroswitch
// that runs on the m stack.
//
// Note: Coroutine switches are expected to happen at
// an order of magnitude (or more) higher frequency
// than regular goroutine switches, so this path is heavily
// optimized to remove unnecessary work.
// The fast path here is three CAS: the one at the top on gp.atomicstatus,
// the one in the middle to choose the next g,
// and the one at the bottom on gnext.atomicstatus.
// It is important not to add more atomic operations or other
// expensive operations to the fast path.
internal static void coroswitch_m(ж<g> Ꮡgp) {
    ref var gp = ref Ꮡgp.DerefOrNull();

    var c = gp.coroarg;
    gp.coroarg = default!;
    var exit = gp.coroexit;
    gp.coroexit = false;
    var mp = gp.m;
    // Track and validate thread-lock interactions.
    //
    // The rules with thread-lock interactions are simple. When a coro goroutine is switched to,
    // the same thread must be used, and the locked state must match with the thread-lock state of
    // the goroutine which called newcoro. Thread-lock state consists of the thread and the number
    // of internal (cgo callback, etc.) and external (LockOSThread) thread locks.
    var locked = gp.lockedm != 0;
    if ((~c).mp != nil || locked) {
        if (mp != (~c).mp || (~mp).lockedInt != (~c).lockedInt || (~mp).lockedExt != (~c).lockedExt) {
            print((@string)"coro: got thread "u8, @unsafe.Pointer.FromPinnedBox(mp), (@string)", want "u8, @unsafe.Pointer.FromPinnedBox((~c).mp), (@string)"\n"u8);
            print((@string)"coro: got lock internal "u8, (~mp).lockedInt, (@string)", want "u8, (~c).lockedInt, (@string)"\n"u8);
            print((@string)"coro: got lock external "u8, (~mp).lockedExt, (@string)", want "u8, (~c).lockedExt, (@string)"\n"u8);
            @throw(coroOsThreadLockingMustˢ);
        }
    }
    // Acquire tracer for writing for the duration of this call.
    //
    // There's a lot of state manipulation performed with shortcuts
    // but we need to make sure the tracer can only observe the
    // start and end states to maintain a coherent model and avoid
    // emitting an event for every single transition.
    var Δtrace = traceAcquire();
    var canCAS = true;
    var sg = gp.syncGroup;
    if (sg != nil) {
        // If we're in a synctest group, always use casgstatus (which tracks
        // group idleness) rather than directly CASing. Mark the group as active
        // while we're in the process of transferring control.
        canCAS = false;
        sg.incActive();
    }
    if (locked) {
        // Detach the goroutine from the thread; we'll attach to the goroutine we're
        // switching to before returning.
        gp.lockedm.set(nil);
    }
    if (exit){
        // The M might have a non-zero OS thread lock count when we get here, gdestroy
        // will avoid destroying the M if the G isn't explicitly locked to it via lockedm,
        // which we cleared above. It's fine to gdestroy here also, even when locked to
        // the thread, because we'll be switching back to another goroutine anyway, which
        // will take back its thread-lock state before returning.
        gdestroy(Ꮡgp);
        Ꮡgp = default!; gp = ref Ꮡgp.DerefOrNull();
    } else {
        // If we can CAS ourselves directly from running to waiting, so do,
        // keeping the control transfer as lightweight as possible.
        gp.waitreason = waitReasonCoroutine;
        if (!canCAS || !Ꮡgp.of(g.Ꮡatomicstatus).CompareAndSwap(_Grunning, _Gwaiting)) {
            // The CAS failed: use casgstatus, which will take care of
            // coordinating with the garbage collector about the state change.
            casgstatus(Ꮡgp, _Grunning, _Gwaiting);
        }
        // Clear gp.m.
        setMNoWB(Ꮡgp.of(g.Ꮡm), nil);
    }
    // The goroutine stored in c is the one to run next.
    // Swap it with ourselves.
    ж<g> gnext = default!;
    while (ᐧ) {
        // Note: this is a racy load, but it will eventually
        // get the right value, and if it gets the wrong value,
        // the c.gp.cas will fail, so no harm done other than
        // a wasted loop iteration.
        // The cas will also sync c.gp's
        // memory enough that the next iteration of the racy load
        // should see the correct value.
        // We are avoiding the atomic load to keep this path
        // as lightweight as absolutely possible.
        // (The atomic load is free on x86 but not free elsewhere.)
        var next = c.Value.gp;
        if (next.ptr() == nil) {
            @throw(coroswitchOnExitedCoroˢ);
        }
        Δguintptr self = default!;
        self.set(Ꮡgp);
        if (c.of(coro.Ꮡgp).cas(next, self)) {
            gnext = next.ptr();
            break;
        }
    }
    // Check if we're switching to ourselves. This case is able to break our
    // thread-lock invariants and an unbuffered channel implementation of
    // coroswitch would deadlock. It's clear that this case should just not
    // work.
    if (gnext == Ꮡgp) {
        @throw(coroswitchOfAGoroutineToˢ);
    }
    // Emit the trace event after getting gnext but before changing curg.
    // GoSwitch expects that the current G is running and that we haven't
    // switched yet for correct status emission.
    if (Δtrace.ok()) {
        Δtrace.GoSwitch(gnext, exit);
    }
    // Start running next, without heavy scheduling machinery.
    // Set mp.curg and gnext.m and then update scheduling state
    // directly if possible.
    setGNoWB(mp.of(m.Ꮡcurg), gnext);
    setMNoWB(gnext.of(g.Ꮡm), mp);
    // Synchronize with any out-standing goroutine profile. We're about to start
    // executing, and an invariant of the profiler is that we tryRecordGoroutineProfile
    // whenever a goroutine is about to start running.
    //
    // N.B. We must do this before transitioning to _Grunning but after installing gnext
    // in curg, so that we have a valid curg for allocation (tryRecordGoroutineProfile
    // may allocate).
    if (goroutineProfile.active) {
        tryRecordGoroutineProfile(gnext, default!, osyield);
    }
    if (!canCAS || !gnext.of(g.Ꮡatomicstatus).CompareAndSwap(_Gwaiting, _Grunning)) {
        // The CAS failed: use casgstatus, which will take care of
        // coordinating with the garbage collector about the state change.
        casgstatus(gnext, _Gwaiting, _Grunnable);
        casgstatus(gnext, _Grunnable, _Grunning);
    }
    // Donate locked state.
    if (locked) {
        mp.of(m.Ꮡlockedg).set(gnext);
        gnext.of(g.Ꮡlockedm).set(mp);
    }
    // Release the trace locker. We've completed all the necessary transitions..
    if (Δtrace.ok()) {
        traceRelease(Δtrace);
    }
    if (sg != nil) {
        sg.decActive();
    }
    // Switch to gnext. Does not return.
    gogo(gnext.of(g.Ꮡsched));
}

} // end runtime_package
