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
// contention). So the LOCK PROTOCOL is reimplemented natively, inside Go's own layout: a Mutex is Go's
// eight bytes, {state int32; sema uint32}, and nothing else.
//
// - `state` is the classic three-state futex lock: 0 unlocked, 1 locked, 2 locked and a waiter may be
//   blocked. Lock takes 0 -> 1 with one CompareAndSwap (Go's fast path); a contended Lock swaps in 2 and
//   blocks until its swap reads 0; Unlock swaps in 0 and wakes one waiter only when it read 2.
// - `sema` holds the HANDLE of the mutex's wait queue, a semaphore in a process-wide table, and holds
//   it only while a goroutine waits: the first waiter installs it by CompareAndSwap and the last waiter
//   to take the lock removes the queue and writes 0 back, so an idle mutex is eight zero bytes, as Go's
//   is. The handle is a number IN the mutex's bytes, never a managed reference and never keyed by an
//   address, so a Mutex works the same in the managed heap, on the stack and in C memory (sqlite's
//   lib/mutex.go callocs one; libc's pthread mutex overlay holds one), and a moving collector does not
//   matter.
//
// The CLR's semaphore keeps what the protocol needs: release from any thread (Go allows unlock on a
// different goroutine), a release that finds no waiter is kept rather than lost, and a second Lock on
// the same goroutine blocks, matching Go's self-deadlock. A kept wakeup only ever makes a later waiter
// re-check `state` once, never acquire a held lock, and Unlock keeps at most one (see Unlock). Wakeups
// follow the semaphore's monitor order, and a woken waiter competes with newcomers for the free lock,
// as in Go's normal mode; Go's starvation mode is not reproduced. See runtime_impl.cs for the shared
// rationale.
//
// A COPY of a Mutex is a separate mutex, as in Go: it copies the state word, so a copy of a locked
// mutex is locked, and a copy of an idle mutex is a free mutex with no queue. Go's vet forbids the copy
// ("must not be copied after first use") but its bytes work, and so do these. One copy differs: a
// mutex copied WHILE a goroutine waits on it carries the queue's handle, and until that queue empties
// a waiter of the copy can take a wakeup meant for the original, re-check its own state and wait
// again, which leaves the original's waiter asleep. In Go the two wait on different addresses. That
// copy reads the two words while other goroutines write them. BOARD, W9a.
using System.Collections.Concurrent;
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
    // Go's layout (internal/sync.Mutex): the lock word and the semaphore word, eight bytes, no managed
    // reference. `state`: 0 unlocked, 1 locked, 2 locked with a waiter possibly blocked. `sema`: the
    // handle of this mutex's wait queue in s_queues, 0 while no goroutine waits.
    internal int32 state;
    internal uint32 sema;
}

// A Locker represents an object that can be locked and unlocked.
[GoType] public partial interface Locker {
    void Lock();
    void Unlock();
}

// A mutex's wait queue: the semaphore its waiters block on, the handle the mutex holds for it, and the
// number of waiters that hold it. A waiter holds the queue from before its first swap of `state` until
// it has the lock (gateOf, leave), and the queue of a mutex nobody waits on is retired: `waiters` goes
// from 0 to -1 once and for all, and a retired queue is never handed out again. It is never disposed
// either, so an Unlock that read the handle just before the retirement releases a semaphore nobody
// waits on, which is harmless.
private sealed class WaitQueue : SemaphoreSlim {
    internal uint32 handle;
    internal int waiters;

    internal WaitQueue() : base(0) { }
}

// The wait queues, by handle. A queue is published here BEFORE its handle is written into a mutex, so
// any thread that reads a handle from a mutex finds its queue, or finds none because it was retired.
private static readonly ConcurrentDictionary<uint32, WaitQueue> s_queues = new();

private static int s_lastQueue;

// gateOf returns the mutex's wait queue with the caller counted as one of its waiters, installing a
// queue if the mutex has none (race-safe). The caller gives the queue back with leave. Reached only from
// a contended Lock: an uncontended Lock/Unlock pair never comes here.
//
// While a waiter holds the queue, the mutex's handle cannot change: only a retirement clears it, a
// retirement needs the count at 0, and a handle whose queue is in the table is never replaced. So an
// Unlock that follows a waiter's swap of `state` reads that waiter's handle and finds that waiter's queue.
//
// A handle with no queue is replaced like a zero one: bytes the mutex never wrote (recycled C memory
// that was not zeroed), or the handle of a retired queue in a mutex copied while it had waiters. A new
// handle skips 0 and every handle in use, so the 32-bit counter can wrap. Takes the mutex BY REF: the
// methods that call it are `ref Mutex` primaries, and the mutex can live where no box reaches it.
private static WaitQueue gateOf(ref Mutex m) {
    while (true) {
        uint32 seen = Volatile.Read(ref m.sema);

        if (seen != 0 && s_queues.TryGetValue(seen, out WaitQueue? queue)) {
            for (int waiters = Volatile.Read(ref queue.waiters); waiters >= 0;) {
                int found = Interlocked.CompareExchange(ref queue.waiters, waiters + 1, waiters);

                if (found == waiters) {
                    return queue;
                }

                waiters = found;
            }

            // Retired between the two reads. Its last waiter is clearing the handle; do the same and
            // install a new queue.
            Interlocked.CompareExchange(ref m.sema, 0, seen);
            continue;
        }

        var created = new WaitQueue { waiters = 1 };

        do {
            created.handle = unchecked((uint32)Interlocked.Increment(ref s_lastQueue));
        } while (created.handle == 0 || !s_queues.TryAdd(created.handle, created));

        if (Interlocked.CompareExchange(ref m.sema, created.handle, seen) == seen) {
            return created;
        }

        // Another thread installed a queue first: give ours back, which retires it, and take theirs.
        leave(ref m, created);
    }
}

// leave gives back a queue gateOf returned. The last waiter out retires the queue, removes it from the
// table and writes 0 back into the mutex, so an idle mutex holds no handle. A waiter that arrives
// between the count reaching 0 and the retirement keeps the queue alive, and retires it when it leaves.
private static void leave(ref Mutex m, WaitQueue queue) {
    if (Interlocked.Decrement(ref queue.waiters) != 0 || Interlocked.CompareExchange(ref queue.waiters, -1, 0) != 0) {
        return;
    }

    s_queues.TryRemove(new System.Collections.Generic.KeyValuePair<uint32, WaitQueue>(queue.handle, queue));
    Interlocked.CompareExchange(ref m.sema, 0, queue.handle);
}

// Lock locks m.
// If the lock is already in use, the calling goroutine blocks until the mutex is available.
//
// FAST PATH: Lock first takes the free lock with Go's own CompareAndSwap(0, mutexLocked) ahead of
// lockSlow, and returns on success with no park, no profile read, no queue and no allocation. An
// uncontended pair parked anyway before C2's fast path: 230 ns and 64 B on Windows x64 and 322 ns and
// 64 B on linux-x64, where Go reads 3.8 ns and 0 B (BOARD 2026-11-11, G). With the Go layout (W9a,
// 2026-11-15, linux/amd64, 4 cpus, .NET 10.0.13) the SemaphoreSlim try's 49 ns and 0 B a pair became
// 17 ns and 0 B, the same in C memory.
//
// SLOW PATH: take the wait queue, swap in 2 ("locked, a waiter may be blocked") and block on the queue
// until a swap reads 0, then give the queue back. The waiter that wins therefore leaves 2 behind, so its
// Unlock wakes the queue even when no one waits: that kept wakeup costs the next waiter one extra check,
// never a held lock.
//
// The park scope is ACCOUNTING ONLY (DESIGN-cooperative-scheduler.md §5.3): it names the wait for a
// traceback and touches neither the queue nor the order in which waiters reach it. It wraps only the
// slow path, as Go parks only a contended Lock.
//
// PROFILING (class F; DESIGN-managed-profiling I4 and the Mutex slice of I3). Go records a contended
// acquire in semacquire1 -- a block event on the waiter, and the waiter's acquire time for the mutex
// event its unlocker records -- and an uncontended one not at all, because cansemacquire returns first.
// This lock never reaches semacquire1, so Lock does the same accounting itself, and only while a
// profile rate is on (Go stamps t0 only then): a failed try with rates off adds two static reads. "Contended"
// is Go's own test: the fast-path CompareAndSwap (cansemacquire's counterpart) FAILED, so a Lock that
// blocks has always stamped. The stamp precedes the park, and a successful try returns with no stamp,
// no park and no event. STATED DEVIATIONS from Go's accounting (dated amendment 2026-09-28 in
// DESIGN-managed-profiling.md), each accepted as such:
// - a failed try that then acquires on its first swap, or inside Wait()'s own spin, records a short
//   block event (and can draw a short mutex charge) where Go's lockSlow spin records none;
// - the block event's cycles end when this waiter wakes, where Go's end at the releaser's readyWithTime;
// - one block event per Lock, where Go records one per semacquire;
// - every Unlock while a stamped waiter is outstanding charges the head wait (see TryHandoff), where
//   Go charges only an Unlock that semreleases to a waiter, so event COUNTS differ and a woken waiter's
//   run gap is included in its next charge;
// - a handoff charges the OLDEST stamped wait (Go's dequeued head), not necessarily the waiter the queue
//   actually wakes;
// - a blocked waiter that stamped nothing is invisible to both profiles; with the try that narrows to
//   a rate turned on mid-wait;
//
// NoInlining: GoSyncBlockEvent's skip counts this method's frame as the block event's top frame (Go's
// sync.(*Mutex).Lock), and .NET 10 can inline a method with try/finally.
[GoRecv, MethodImpl(MethodImplOptions.NoInlining)] public static void Lock(this ref Mutex m) {
    // The fast path, Go's CompareAndSwap(0, mutexLocked) before lockSlow: a free lock is taken before
    // any wait accounting, so an uncontended Lock parks nowhere and allocates nothing.
    if (Interlocked.CompareExchange(ref m.state, 1, 0) == 0) {
        return;
    }
    WaitQueue gate = gateOf(ref m);
    bool block = runtime_package.GoBlockProfileOn;
    bool mutex = runtime_package.GoMutexProfileOn;
    if (!(block || mutex)) {
        try {
            using (Goroutine.Park(WaitReason.SyncMutexLock)) {
                while (Interlocked.Exchange(ref m.state, 2) != 0) {
                    gate.Wait();
                }
            }
        } finally {
            leave(ref m, gate);
        }
        return;
    }
    int64 t0 = runtime_package.GoCputicks();
    WaitStamps? stamps = null;
    if (mutex) {
        stamps = s_waitStamps.GetValue(gate, static _ => new WaitStamps());
        stamps.Enqueue(t0);
    }
    // try/finally: a Wait that throws must still retire its stamp, or s_outstanding stays above zero
    // and every later Unlock pays the side-table lookup; and it must still give the queue back.
    try {
        using (Goroutine.Park(WaitReason.SyncMutexLock)) {
            while (Interlocked.Exchange(ref m.state, 2) != 0) {
                gate.Wait();
            }
        }
    } finally {
        stamps?.Acquired();
        leave(ref m, gate);
    }
    if (block) {
        runtime_package.GoSyncBlockEvent(runtime_package.GoCputicks() - t0);
    }
}

// The acquire times of a queue's stamped waiters, Go's per-sudog acquiretime reduced to the two its
// semrelease1 reads: the HEAD (the waiter a handoff dequeues) and the TAIL (the newest), plus the count
// between them. Kept beside the queue rather than in Mutex, so Mutex keeps Go's layout, and
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
    // handoff (a failed try that took the lock on its first swap) must not survive into the next
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
    // wake latency (R's review). The stamp is NOT cleared here: the releasing thread can take the lock
    // straight back, and the woken waiter then swaps in 2 and blocks again with its stamp, exactly the
    // waiter Go re-queues. Only Acquired, when the last stamped waiter leaves, clears it.
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
[GoRecv] public static bool TryLock(this ref Mutex m) => Interlocked.CompareExchange(ref m.state, 1, 0) == 0;

// Unlock unlocks m.
// It is a run-time error if m is not locked on entry to Unlock.
// A locked Mutex is not associated with a particular goroutine; one goroutine may lock a Mutex and
// then arrange for another goroutine to unlock it.
//
// The swap to 0 is the unlock; only a swap that read 2 (a waiter may be blocked) wakes the queue, so an
// uncontended Unlock with profiling off is one interlocked instruction and one volatile read. A swap that
// read 0 is Go's fatal error, the same text as internal/sync's unlockSlow; the 0 it wrote is never
// observed, because fatal does not return.
//
// THE WAKEUP. A waiter holds the queue from before its swap to 2, so an Unlock that read 2 because a
// waiter is blocked finds that waiter's handle in the mutex. No handle, or a handle with no queue, means
// the 2 was left by a waiter that has the lock or had it, and nobody is blocked. The queue then needs ONE
// wakeup available, not one per Unlock: whoever takes it swaps `state` again, and either gets the lock
// and leaves 2 behind, so its own Unlock comes back here, or finds a new holder and leaves 2 for that
// holder's Unlock. So a wakeup is sent only when none is waiting to be taken. This read comes after the
// swap to 0, so a wakeup it sees is taken by a thread that will see this Unlock. Sending one per Unlock
// would count up without bound while waiters take the lock on their first swap and nobody blocks.
//
// A handoff to a stamped waiter records Go's mutex event first, on this unlocker's stack, as
// semrelease1 does before it readies the waiter. NoInlining for the same reason as Lock: this frame is
// the mutex event's top frame (Go's sync.(*Mutex).Unlock).
[GoRecv, MethodImpl(MethodImplOptions.NoInlining)] public static void Unlock(this ref Mutex m) {
    int32 old = Interlocked.Exchange(ref m.state, 0);
    if (old != 1 && old != 2) {
        fatal("sync: unlock of unlocked mutex"u8);
    }
    // A stamped waiter exists only on a mutex whose queue is installed (Lock stamps after gateOf), so a
    // zero handle needs no lookup. The charge does not wait for a swap that read 2: a woken waiter that
    // has not yet swapped 2 back in is still Go's dequeued waiter, as the stamp rule above states.
    uint32 handle;
    if (Volatile.Read(ref WaitStamps.s_outstanding) != 0 && (handle = Volatile.Read(ref m.sema)) != 0 &&
        s_queues.TryGetValue(handle, out WaitQueue? stamped) &&
        s_waitStamps.TryGetValue(stamped, out WaitStamps? stamps) && stamps.TryHandoff(out int64 dt)) {
        runtime_package.GoSyncMutexEvent(dt);
    }
    if (old == 2 && (handle = Volatile.Read(ref m.sema)) != 0 &&
        s_queues.TryGetValue(handle, out WaitQueue? gate) && gate.CurrentCount == 0) {
        gate.Release();
    }
}

} // end sync_package
