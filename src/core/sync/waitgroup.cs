// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted waitgroup.go output). Go's WaitGroup
// packs a counter and a waiter count into one atomic uint64 and parks waiters on the runtime sleeping
// semaphore — the same primitive that cannot be faithfully emulated (see mutex.cs / runtime_impl.cs).
// Reimplemented natively: a guarded counter plus a latch event that is set whenever the counter is
// zero. Wait blocks on the latch; the Add that drives the counter to zero releases every waiter.
using System.Collections.Generic;
using System.Threading;
// Aliased rather than imported wholesale: this file needs exactly three golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using Goroutine = go.golib.Goroutine;
using SyncTestBubble = go.golib.SyncTestBubble;
using WaitReason = go.golib.WaitReason;

// Hand-owned native replacement of the converted waitgroup.go output — the marker makes a -stdlib
// reconvert skip regenerating this file (see containsManualConversionMarker).
[module: go.GoManualConversion]

// The Go lines of this file's `go` statements, so a goroutine it starts names its creator's Go
// position in a traceback's "created by" line. Hand-kept: the converter emits no record for a
// hand-owned file. One table entry per statement, each followed by a zero entry, so every other
// line of the file still reports its own C# position. repoguard's TestHandOwnedGoStatementsMapToGo
// checks it.
[assembly: go.GoPositionMap("sync/waitgroup.go", "waitgroup.cs", "AOwB3AMAANsD")]

namespace go;

partial class sync_package {

// A WaitGroup waits for a collection of goroutines to finish.
// A WaitGroup must not be copied after first use.
[GoType] partial struct WaitGroup {
    // Lazily-created shared state; a WaitGroup is always used through a pointer (ж<WaitGroup>).
    internal WaitGroupState? st;
}

// WaitGroupState is the native backing for a WaitGroup: a counter guarded by Gate, and an event that
// is signaled exactly while the counter is zero (so a fresh/zero WaitGroup lets Wait return at once).
//
// Parked lists the goroutines blocked in Wait for the current count to drain -- Go's semaphore
// queue for wg.sema, reduced to what the event cannot say: WHOM the Add that reaches zero wakes. The
// event releases every waiter at once and cannot name them, so without this the zero-reaching Add
// could not ready them (golib's Goroutine.Ready, on the waker's side), and a waiter would read parked
// until its own thread resumed. Guarded by Gate.
//
// Bubble and Bubbled are Go 1.26's synctest association (waitgroup.go:86-105): Bubble is the
// _KindSpecialBubble record runtime/synctest.go keeps on the WaitGroup's heap object, and Bubbled is
// waitGroupBubbleFlag in wg.state. They live here, on the WaitGroup's own state, because this state is
// the one identity every pointer to the WaitGroup reaches -- what Go's special is keyed on. (The
// internal/synctest pulls cannot keep it: a pointer there is a box, and two pointers to one field are
// two boxes; see synctest_impl.cs.) Guarded by Gate.
//
// Go answers Unbubbled for a WaitGroup that is not in a heap span, such as a package-level variable
// (runtime/synctest.go:383-386, "This is probably a package var"): no association, no flag, and a Wait
// that is never durable. A converted global's box is a StaticBox (the converter's package-level
// declaration), so wgBubbleOf answers the same; a field of a global reads through its view's source.
internal sealed class WaitGroupState {
    internal int Counter;
    internal readonly ManualResetEventSlim Idle = new(true);
    internal readonly object Gate = new();
    internal readonly List<Goroutine?> Parked = new();
    internal SyncTestBubble? Bubble;
    internal bool Bubbled;
}

private static WaitGroupState wgStateOf(ж<WaitGroup> Ꮡwg) {
    ref var wg = ref Ꮡwg.Value;

    WaitGroupState? s = Volatile.Read(ref wg.st);

    if (s is not null) {
        return s;
    }

    var created = new WaitGroupState();
    return Interlocked.CompareExchange(ref wg.st, created, null) ?? created;
}

// wgBubbleOf is the calling goroutine's bubble as synctest.Associate and synctest.IsAssociated see it for
// this WaitGroup: none for a package-level WaitGroup, which Go leaves Unbubbled, so Add records no
// association and sets no flag, and Wait is never durable -- exactly the path of a caller outside every
// bubble (waitgroup.go: `case synctest.Unbubbled:` does nothing).
private static SyncTestBubble? wgBubbleOf(ж<WaitGroup> Ꮡwg) =>
    PackageLevelStorage.Holds(Ꮡwg) ? null : SyncTestBubble.Current;

// Add adds delta, which may be negative, to the WaitGroup counter.
// If the counter becomes zero, all goroutines blocked on Wait are released.
// If the counter goes negative, Add panics.
//
// Go's two bubble fatals are raised after Gate is released: the process exits on them either way, and
// not under a lock another goroutine's exit path could want.
public static void Add(this ж<WaitGroup> Ꮡwg, nint delta) {
    WaitGroupState s = wgStateOf(Ꮡwg);
    string? fault;

    lock (s.Gate) {
        fault = wgAddBubble(wgBubbleOf(Ꮡwg), ref s.Bubble, ref s.Bubbled, s.Counter != 0 || s.Parked.Count != 0, out bool bubbled);

        if (fault is null) {
            int c = s.Counter + ((int)delta);
            s.Counter = c;

            if (c < 0) {
                throw panic("sync: negative WaitGroup counter");
            }

            if (c == 0) {
                if (s.Parked.Count != 0) {
                    // Go resets wg.state to 0 here, the bubble flag with it, and a bubbled Add
                    // disassociates: no Add may run concurrently with Wait at zero.
                    s.Bubbled = false;

                    if (bubbled) {
                        s.Bubble = null;
                    }
                }

                // Go's Add at zero: semrelease for every waiter, each readied (goready) before it is
                // signalled -- here, readied, then the one event that releases them all.
                foreach (Goroutine? parked in s.Parked) {
                    Goroutine.Ready(parked);
                }

                s.Parked.Clear();
                s.Idle.Set();
            } else {
                s.Idle.Reset();
            }
        }
    }

    if (fault is not null) {
        fatal(fault);
    }
}

// wgAddBubble is the bubble half of Go 1.26's Add (waitgroup.go:86-108), over the association it
// updates in place: caller is the calling goroutine's bubble, and busy is Go's `state != 0` with the
// flag bit set aside (a non-zero counter or a parked waiter). It returns Go's fatal text, or null, and
// bubbled is Go's local of that name. Kept apart from the WaitGroup so a test can reach both fatal
// arms without the process exit they end in.
internal static string? wgAddBubble(SyncTestBubble? caller, ref SyncTestBubble? bubble, ref bool flag, bool busy, out bool bubbled) {
    bubbled = false;

    if (caller is not null) {
        // synctest.Associate(wg): the first association records the caller's bubble; after that the
        // record answers CurrentBubble or OtherBubble.
        bubble ??= caller;

        if (!ReferenceEquals(bubble, caller)) {
            return "sync: WaitGroup.Add called from multiple synctest bubbles";
        }

        bubbled = true;
        bool wasFlagged = flag;
        flag = true;

        if (busy && !wasFlagged) {
            // Add has been called from outside this bubble.
            return "sync: WaitGroup.Add called from inside and outside synctest bubble";
        }
    }

    if (flag && !bubbled) {
        // Add has been called from within a synctest bubble (and we aren't in one).
        return "sync: WaitGroup.Add called from inside and outside synctest bubble";
    }

    return null;
}

// Done decrements the WaitGroup counter by one.
public static void Done(this ж<WaitGroup> Ꮡwg) => Ꮡwg.Add(-1);

// Wait blocks until the WaitGroup counter is zero.
//
// `sync.WaitGroup.Wait`, Go 1.24's own reason: WaitGroup.Wait calls runtime_SemacquireWaitGroup, which
// is sema.go's sync_runtime_SemacquireWaitGroup, which parks with waitReasonSyncWaitGroupWait -- so
// that is what a Go 1.24 traceback prints for a blocked Wait. (At Go 1.23 it called
// runtime_Semacquire and printed `semacquire`; this file said so until the corpus moved to 1.24, and
// the GoroutineWaitState behavioral guard read the stale word against go1.24.13.)
//
// Go 1.25 made the wait durable inside a bubble: when Add was called from the waiter's own bubble (the
// flag is set and the WaitGroup is associated with the caller's bubble), Wait passes synctestDurable
// and sema.go parks with waitReasonSynctestWaitGroupWait, the reason SyncTestBubble.IsIdle counts as
// durably blocked. Any other wait parks with the plain reason.
//
// A zero counter returns at once without parking, as Go's does (`if v == 0 { return }`); since Go 1.25
// a zero-count Wait with no waiter left also disassociates a WaitGroup bubbled to the caller's bubble,
// so it can be reused from another. Otherwise the goroutine registers itself and parks in Go's commit
// order -- under Gate, the lock that publishes it to Add, then unlocks, then waits -- so the Add that
// reaches zero can only ever find it parked.
public static void Wait(this ж<WaitGroup> Ꮡwg) {
    WaitGroupState s = wgStateOf(Ꮡwg);
    bool locked = false;

    try {
        Monitor.Enter(s.Gate, ref locked);

        // synctest.IsAssociated(wg): associated with the caller's bubble, so never outside one, and never
        // for a package-level WaitGroup.
        SyncTestBubble? caller = wgBubbleOf(Ꮡwg);
        bool associated = caller is not null && ReferenceEquals(s.Bubble, caller);

        if (s.Counter == 0) {
            if (s.Parked.Count == 0 && s.Bubbled && associated) {
                s.Bubbled = false;
                s.Bubble = null;
            }

            return;
        }

        bool synctestDurable = s.Bubbled && associated;
        s.Parked.Add(Goroutine.Current);

        using (Goroutine.Park(synctestDurable ? WaitReason.SynctestWaitGroupWait : WaitReason.SyncWaitGroupWait)) {
            Monitor.Exit(s.Gate);
            locked = false;
            s.Idle.Wait();
        }
    }
    finally {
        if (locked) {
            Monitor.Exit(s.Gate);
        }
    }
}

// Go calls f in a new goroutine and adds that task to the WaitGroup.
// When f returns, the task is removed from the WaitGroup.
//
// The function f must not panic.
//
// Go 1.26's body: a panicking f does not call Done -- Done would release Wait to race the fatal panic,
// possibly to os.Exit(0) first -- so the panic propagates and ends the process as any unrecovered
// goroutine panic does. An f that ends in runtime.Goexit still calls Done, as Go's deferred Done runs
// on Goexit; the Goexit then goes on to end the goroutine.
public static void Go(this ж<WaitGroup> Ꮡwg, Action f) {
    Ꮡwg.Add(1);

    goǃ(() => {
        try {
            f();
        }
        catch (GoexitException) {
            Ꮡwg.Done();
            throw;
        }

        Ꮡwg.Done();
    });
}

} // end sync_package
