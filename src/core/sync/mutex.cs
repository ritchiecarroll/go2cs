// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sync provides basic synchronization primitives such as mutual
// exclusion locks. Other than the [Once] and [WaitGroup] types, most are intended
// for use by low-level library routines. Higher-level synchronization is
// better done via channels and communication.
//
// Values containing the types defined in this package should not be copied.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted mutex.go output). Go's Mutex is a
// state machine over an int32 driven by a runtime sleeping semaphore (sema.go). That semaphore is
// co-designed with the state machine — starvation-mode ownership is handed off to one specific waiter
// with exact ticket semantics — and cannot be reproduced faithfully on top of any .NET primitive
// (an emulated semaphore trips "inconsistent mutex state" / "unlock of unlocked mutex" under sustained
// contention). So the whole type is reimplemented natively: a Mutex is a lazily-created binary
// SemaphoreSlim, which the CLR implements correctly — wakeups in its monitor's pulse order (not a
// FIFO handoff: a free count goes to whichever thread takes it first, queued or not), release
// permitted from any thread (Go allows unlock on a different goroutine), and non-reentrant (a second
// Lock on the same thread blocks, matching Go's self-deadlock). See runtime_impl.cs for the shared
// rationale.
using System.Runtime.CompilerServices;
using System.Threading;
// Aliased rather than imported wholesale: this file needs exactly three golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using FatalReport = go.golib.FatalReport;
using Goroutine = go.golib.Goroutine;
using WaitReason = go.golib.WaitReason;

// Hand-owned native replacement of the converted mutex.go output — the converter skips regenerating a
// file that carries this marker, so a -stdlib reconvert preserves it (see containsManualConversionMarker).
[module: go.GoManualConversion]

#pragma warning disable CS8826 // the converted partial names the blank Go parameter `_`; this hand-owned body keeps its readable name

namespace go;

partial class sync_package {

// Fatal-error hooks (Go provides these via runtime linkname): one-line forwards to golib's fatal
// primitive, which owns the report and the exit for every site that has one — runtime's own throw
// and fatal, these two, and at Go 1.24 the new internal/sync's pair. golib is the only assembly
// below all of them, and internal/sync referencing sync would be the project-reference cycle
// check-solution-integrity's per-GOOS assertion exists to catch. COORD ruling, mailbox 4e9b115;
// docs/phase4/DESIGN-fatal-path.md.
//
// The unrecoverability these lines used to get from a non-panic exception is now structural: the
// primitive writes the report and calls Environment.Exit(2), so no recover(), no deferred function
// and no catch runs — which is what Go's throw/fatal do. What changes is the TEXT: Go's
// `fatal error: <text>`, a blank line and a Go-spelled traceback, in place of a .NET exception dump
// that merely happened to contain the first line.
internal static partial void @throw(@string s) => FatalReport.Fatal(s, userFault: false);

internal static partial void fatal(@string s) => FatalReport.Fatal(s, userFault: true);

// A Mutex is a mutual exclusion lock.
// The zero value for a Mutex is an unlocked mutex.
//
// A Mutex must not be copied after first use.
[GoType] partial struct Mutex {
    // Lazily-created binary semaphore backing this mutex. The gate is a FIELD of the mutex, so what
    // makes it shared is Go's own rule that a Mutex must not be copied after first use — every
    // reference to the same mutex reaches the same field. The box is NOT what holds it: this file's
    // earlier wording said "the box holds the single shared gate", which read as though the identity
    // lived in the ж<Mutex>, and it never did. That mattered once the methods below became `ref
    // Mutex` primaries and there is no box in the picture at all; the gate is reached the same way
    // and is the same gate.
    internal SemaphoreSlim? gate;
}

// A Locker represents an object that can be locked and unlocked.
[GoType] public partial interface Locker {
    void Lock();
    void Unlock();
}

// gateOf returns the mutex's backing semaphore, creating it once on first use (race-safe).
//
// Takes the mutex BY REF rather than by box. Every use below was already `ref m` — the box existed
// only to produce that ref — so this is the same code reached one indirection earlier, and it is
// what lets the three methods that call it be `ref Mutex` primaries.
private static SemaphoreSlim gateOf(ref Mutex m) {
    SemaphoreSlim? g = Volatile.Read(ref m.gate);

    if (g is not null) {
        return g;
    }

    var created = new SemaphoreSlim(1, 1);
    g = Interlocked.CompareExchange(ref m.gate, created, null);

    if (g is not null) {
        created.Dispose(); // lost the race; another thread installed the gate
        return g;
    }

    return created;
}

// Lock locks m.
// If the lock is already in use, the calling goroutine blocks until the mutex is available.
//
// The park scope is ACCOUNTING ONLY (DESIGN-cooperative-scheduler.md §5.3): it names the wait for a
// traceback and touches neither the gate nor the order in which waiters reach it. With every profile
// rate at zero it wraps the whole Wait, contended or not: two volatile stores on every Lock, measured
// by the cost canary named in that arc's commit. (That arc declined a Wait(0) fast path as "a newcomer
// barging ahead of the queue". The premise was wrong: SemaphoreSlim.Wait() already takes any free count
// ahead of queued or pulsed waiters, and a failed Wait(0) has no side effect, so a try-then-Wait grants
// in exactly the cases Wait does. The profiling path below uses one; the rate-off path is unchanged.)
//
// PROFILING (class F; DESIGN-managed-profiling I4 and the Mutex slice of I3). Go records a contended
// acquire in semacquire1 -- a block event on the waiter, and the waiter's acquire time for the mutex
// event its unlocker records -- and an uncontended one not at all, because cansemacquire returns first.
// This gate never reaches semacquire1, so Lock does the same accounting itself, and only while a
// profile rate is on (Go stamps t0 only then): the rate-off path adds two static reads. "Contended"
// is Go's own test: the fast-path try (Wait(0), cansemacquire's counterpart) FAILED, so a Lock that
// blocks has always stamped. The stamp precedes the park, and a successful try returns with no stamp,
// no park and no event. STATED DEVIATIONS from Go's accounting (dated amendment 2026-09-28 in
// DESIGN-managed-profiling.md), each accepted as such:
// - a failed try that then acquires inside Wait()'s own spin records a short block event (and can
//   draw a short mutex charge) where Go's lockSlow spin records none;
// - the block event's cycles end when this waiter wakes, where Go's end at the releaser's readyWithTime;
// - one block event per Lock, where Go records one per semacquire;
// - every Unlock while a stamped waiter is outstanding charges the head wait (see TryHandoff), where
//   Go charges only an Unlock that semreleases to a waiter, so event COUNTS differ and a woken waiter's
//   run gap is included in its next charge;
// - a handoff charges the OLDEST stamped wait (Go's dequeued head), not necessarily the waiter the gate
//   actually wakes;
// - a blocked waiter that stamped nothing is invisible to both profiles; with the try that narrows to
//   a rate turned on mid-wait;
// - an uncontended Lock with a rate on skips the park scope, so tracer and traceback output differ by
//   rate (closer to Go, which parks only a contended waiter);
// - the try's equivalence to Wait() rests on SemaphoreSlim internals, re-verified at each .NET major hop.
//
// NoInlining: GoSyncBlockEvent's skip counts this method's frame as the block event's top frame (Go's
// sync.(*Mutex).Lock), and .NET 10 can inline a method with try/finally.
[GoRecv, MethodImpl(MethodImplOptions.NoInlining)] public static void Lock(this ref Mutex m) {
    SemaphoreSlim gate = gateOf(ref m);
    bool block = runtime_package.GoBlockProfileOn;
    bool mutex = runtime_package.GoMutexProfileOn;
    if (!(block || mutex)) {
        using (Goroutine.Park(WaitReason.SyncMutexLock)) {
            gate.Wait();
        }
        return;
    }
    if (gate.Wait(0)) {
        return;
    }
    int64 t0 = runtime_package.GoCputicks();
    WaitStamps? stamps = null;
    if (mutex) {
        stamps = s_waitStamps.GetValue(gate, static _ => new WaitStamps());
        stamps.Enqueue(t0);
    }
    // try/finally: a Wait that throws must still retire its stamp, or s_outstanding stays above zero
    // and every later Unlock pays the side-table lookup.
    try {
        using (Goroutine.Park(WaitReason.SyncMutexLock)) {
            gate.Wait();
        }
    } finally {
        stamps?.Acquired();
    }
    if (block) {
        runtime_package.GoSyncBlockEvent(runtime_package.GoCputicks() - t0);
    }
}

// The acquire times of a gate's stamped waiters, Go's per-sudog acquiretime reduced to the two its
// semrelease1 reads: the HEAD (the waiter a handoff dequeues) and the TAIL (the newest), plus the count
// between them. Kept beside the gate rather than in Mutex, so Mutex's layout does not change, and
// reached only while stamped waiters exist anywhere (s_outstanding), so an Unlock with profiling off
// pays one volatile read.
private sealed class WaitStamps {
    internal static int s_outstanding;

    private int64 m_head;
    private int64 m_tail;
    private int m_waiters;

    internal void Enqueue(int64 t0) {
        lock (this) {
            if (m_head == 0) {
                m_head = t0;
            }
            m_tail = t0;
            m_waiters++;
        }
        Interlocked.Increment(ref s_outstanding);
    }

    // The last stamped waiter leaving clears the stamps, handoff or not: a stamp that acquired without a
    // handoff (a failed try that took the gate inside Wait()'s own spin) must not survive into the next
    // contention episode, whose handoff would otherwise charge the whole idle gap since it.
    internal void Acquired() {
        lock (this) {
            if (--m_waiters == 0) {
                m_head = 0;
                m_tail = 0;
            }
        }
        Interlocked.Decrement(ref s_outstanding);
    }

    // Go's semrelease1 over dequeue: the dequeued waiter's wait (dt0) plus the tail-average estimate
    // for the waiters still queued, (dtail + dt0) / 2 each. Then the acquire times restart at now, so
    // every release that finds a stamped waiter is charged from the previous charge and the charges
    // telescope with no wait counted twice. For the waiters still queued that is go1.24.13 sema.go's
    // dequeue, which resets the remaining list's head and tail to now (L438-440; a waiter queued behind
    // them inherits that clock, L311). For a sole waiter it is lockSlow's re-call of
    // runtime_SemacquireMutex on every failed wake (internal/sync/mutex.go:149), a fresh semacquire1
    // whose own t0 (sema.go:169-173) is about the wake time: `now` approximates that t0, early by the
    // wake latency (R's review). The stamp is NOT cleared here: SemaphoreSlim lets the releasing thread take the
    // gate straight back, and the woken waiter then stays blocked in the same Wait() with its stamp,
    // exactly the waiter Go re-queues. Only Acquired, when the last stamped waiter leaves, clears it.
    internal bool TryHandoff(out int64 dt) {
        lock (this) {
            if (m_waiters == 0 || m_head == 0) {
                dt = 0;
                return false;
            }
            int64 now = runtime_package.GoCputicks();
            int64 dt0 = now - m_head;
            dt = dt0;
            int remaining = m_waiters - 1;
            if (remaining > 0) {
                dt += (now - m_tail + dt0) / 2 * remaining;
            }
            m_head = now;
            m_tail = now;
            return true;
        }
    }
}

private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SemaphoreSlim, WaitStamps> s_waitStamps = new();

// TryLock tries to lock m and reports whether it succeeded.
[GoRecv] public static bool TryLock(this ref Mutex m) => gateOf(ref m).Wait(0);

// Unlock unlocks m.
// It is a run-time error if m is not locked on entry to Unlock.
// A locked Mutex is not associated with a particular goroutine; one goroutine may lock a Mutex and
// then arrange for another goroutine to unlock it.
//
// A handoff to a stamped waiter records Go's mutex event first, on this unlocker's stack, as
// semrelease1 does before it readies the waiter. NoInlining for the same reason as Lock: this frame is
// the mutex event's top frame (Go's sync.(*Mutex).Unlock).
[GoRecv, MethodImpl(MethodImplOptions.NoInlining)] public static void Unlock(this ref Mutex m) {
    SemaphoreSlim gate = gateOf(ref m);
    if (Volatile.Read(ref WaitStamps.s_outstanding) != 0 &&
        s_waitStamps.TryGetValue(gate, out WaitStamps? stamps) && stamps.TryHandoff(out int64 dt)) {
        runtime_package.GoSyncMutexEvent(dt);
    }
    try {
        gate.Release();
    } catch (SemaphoreFullException) {
        fatal("sync: unlock of unlocked mutex"u8);
    }
}

} // end sync_package
