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
// re-check `state` once, never acquire a held lock. Unlock sends none while it can see one kept (see
// Unlock); two Unlocks that both see none can leave two, and a second one costs one more re-check.
// Wakeups follow the semaphore's monitor order, and a woken waiter competes with newcomers for the free
// lock, as in Go's normal mode; Go's starvation mode is not reproduced. See runtime_impl.cs for the
// shared rationale.
//
// A COPY of a Mutex is a separate mutex, as in Go: it copies the state word, so a copy of a locked
// mutex is locked, and a copy of an idle mutex is a free mutex with no queue. Go's vet forbids the copy
// ("must not be copied after first use") but its bytes work, and so do these.
//
// WHAT A CONTENDED LOCK ALLOCATES: nothing, while its thread's stripe has a spare queue. Spare queues
// belong to the STRIPE, and threads share stripes (always, past QueueStripes threads). A Lock that has
// to install a queue allocates one (104 bytes: the semaphore and its lock object) when the stripe has
// no spare, which is when more of that stripe's queues are in use at once than ever before. So a
// thread's first install often allocates nothing, and one thread can pay for another's peak. A thread's
// first read of its stripe slot allocates 32 bytes (see t_stripe). Queues are kept for the life of the
// process (QueueStripe has the bound). Go's waiter takes its sudog from a per-P cache the same way, and
// allocates one when the cache is empty. The mutex this file replaced allocated one semaphore (88
// bytes) for every Mutex ever locked, contended or not, and kept it as long as the mutex lived.
//
// WHERE THIS DIFFERS FROM GO, each stated and none cured (BOARD, W9a):
// - A mutex copied WHILE a goroutine waits on it carries the queue's handle. Until that queue empties,
//   a waiter of the copy joins the original's queue and can take a wakeup meant for the original,
//   re-check its own state and wait again, which leaves the original's waiter asleep. In Go the two
//   wait on different addresses. That copy reads the two words while other goroutines write them.
// - A Mutex must start as ZEROED memory, as Go's must. A handle the mutex never wrote is harmless only
//   while it names no queue: gateOf replaces it. One that happens to name a LIVE queue (bytes left in
//   recycled C memory, or the stale handle of the copy above, once its stripe's 26-bit sequence number
//   has come round to it) joins an unrelated mutex's queue, with the copy's fault above. A state word
//   that is neither 0 nor a locked mutex somebody will unlock blocks its first Lock forever, as in Go.
// - Unlock reads the `sema` word after the swap that unlocks, so a mutex freed by another goroutine the
//   moment it is unlocked is read after the free. Go's own Unlock does as much: with a waiter recorded,
//   unlockSlow loads `state` again and semrelease adds to `sema`, both after the unlocking store.
// - A waiter blocks its OS THREAD in the semaphore, where Go parks a goroutine and frees its M, and an
//   Unlock that read 2 takes the stripe's monitor for its lookup. A scheduler that parks instead must
//   keep "a wakeup is waiting to be taken" readable by Unlock, which is the semaphore's CurrentCount
//   here. Under such a scheduler the population counted on queues is goroutines, not OS threads, so
//   the peak a stripe keeps queues for can be far larger: re-read the keep-every-queue rule
//   (QueueStripe) then.
// - Go 1.26 declares sync.Mutex as one field, an internal/sync.Mutex that holds the two words. Reflect
//   sees `state` and `sema` on sync.Mutex itself here: the same eight bytes, one level flatter.
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

using static sync_package;

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
[GoType] public partial struct Mutex {
    // Go's layout (internal/sync.Mutex): the lock word and the semaphore word, eight bytes, no managed
    // reference. `state`: 0 unlocked, 1 locked, 2 locked with a waiter possibly blocked. `sema`: the
    // handle of this mutex's wait queue (queueOf), 0 while no goroutine waits.
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
// it has the lock (gateOf, leave). The queue of a mutex nobody waits on is taken out of the table and
// kept for the next mutex that needs one, which gets it under a NEW handle: a handle read from a mutex
// before its queue was given up names nothing afterwards. So a contended Lock allocates a queue only
// while its thread's stripe has no spare one, which is when more of that stripe's queues are in use at
// once than ever before. `handle`, `waiters` and `next` are written under the stripe's lock.
internal sealed class WaitQueue : SemaphoreSlim {
    internal uint32 handle;
    internal int waiters;
    internal WaitQueue? next;   // the next queue of its bucket, or the next spare one

    internal WaitQueue() : base(0) { }
}

// The table of wait queues is QueueStripes stripes. A handle is its stripe's index in the low StripeBits
// bits and a sequence number above them, so a handle names its stripe; and a thread installs every queue
// it installs in ONE stripe (t_stripe), so threads that contend on different mutexes work in different
// stripes, and one thread meets the same lock and the same spare queue again and again. The sequence
// number is 26 bits: a stripe that installs a queue every microsecond wraps it in about a minute.
private const int StripeBits = 6, QueueStripes = 1 << StripeBits;

// One stripe: its queues, hashed by sequence number and chained through WaitQueue.next, and its spare
// queues. Every read and write of a stripe happens under `lock (stripe)`, and that lock is what makes
// the three compound steps of the protocol atomic: find a queue and count a waiter into it (gateOf),
// install a queue together with its handle (gateOf), and count the last waiter out and remove the queue
// (leave).
//
// A stripe keeps every queue it created, for the life of the process. It creates one only when all of
// its queues are in the table, and a queue in the table has a goroutine counted on it (from gateOf
// until leave: one that has not blocked yet, one that is blocked, or one that has the lock and has not
// left). So a stripe never holds more queues than the most goroutines that were ever inside a contended
// Lock at one moment on queues it installed. The worst case is QueueStripes stripes, each with as many
// queues as the most goroutines that were inside a contended Lock at once, at 104 bytes a queue; it is
// loose, because the stripes' peaks need not coincide. A thread that installed a queue moves on while
// others are still counted on it, so one thread can have several in the table at once; with a limit on
// the spares (four, at first) such a stripe allocated a queue and dropped one again and again.
private sealed class QueueStripe(uint32 index) {
    private WaitQueue?[] m_buckets = new WaitQueue?[16];
    private int m_queues;
    private uint32 m_sequence;
    private WaitQueue? m_spare;

    private static int bucketOf(uint32 handle, int buckets) => (int)(handle >> StripeBits) & (buckets - 1);

    internal WaitQueue? find(uint32 handle) {
        for (WaitQueue? queue = m_buckets[bucketOf(handle, m_buckets.Length)]; queue is not null; queue = queue.next) {
            if (queue.handle == handle) {
                return queue;
            }
        }

        return null;
    }

    // newHandle returns a handle of this stripe that no queue holds: never 0, and never a handle in use,
    // so the sequence number can wrap.
    internal uint32 newHandle() {
        while (true) {
            uint32 handle = ++m_sequence << StripeBits | index;

            if (handle != 0 && find(handle) is null) {
                return handle;
            }
        }
    }

    // install puts a queue with one waiter into the table under handle, a spare queue if there is one.
    internal WaitQueue install(uint32 handle) {
        WaitQueue? queue = m_spare;

        if (queue is null) {
            queue = new WaitQueue();
        } else {
            m_spare = queue.next;

            // A wakeup sent to the queue after its last waiter left belongs to nobody.
            while (queue.CurrentCount != 0 && queue.Wait(0)) { }
        }

        if (++m_queues > m_buckets.Length) {
            grow();
        }

        ref WaitQueue? bucket = ref m_buckets[bucketOf(handle, m_buckets.Length)];
        queue.handle = handle;
        queue.waiters = 1;
        queue.next = bucket;
        bucket = queue;
        return queue;
    }

    // retire takes a queue nobody waits on out of the table and keeps it as a spare. Every queue is
    // kept: see the bound above.
    internal void retire(WaitQueue queue) {
        ref WaitQueue? link = ref m_buckets[bucketOf(queue.handle, m_buckets.Length)];

        while (!ReferenceEquals(link, queue)) {
            link = ref link!.next;
        }

        link = queue.next;
        m_queues--;
        queue.handle = 0;
        queue.next = m_spare;
        m_spare = queue;
    }

    private void grow() {
        WaitQueue?[] buckets = new WaitQueue?[m_buckets.Length * 2];

        foreach (WaitQueue? head in m_buckets) {
            for (WaitQueue? queue = head, next; queue is not null; queue = next) {
                next = queue.next;
                ref WaitQueue? bucket = ref buckets[bucketOf(queue.handle, buckets.Length)];
                queue.next = bucket;
                bucket = queue;
            }
        }

        m_buckets = buckets;
    }
}

private static readonly QueueStripe[] s_stripes = newStripes();

private static QueueStripe[] newStripes() {
    var stripes = new QueueStripe[QueueStripes];

    for (uint32 i = 0; i < QueueStripes; i++) {
        stripes[i] = new QueueStripe(i);
    }

    return stripes;
}

private static QueueStripe stripeOf(uint32 handle) => s_stripes[(int)(handle & (QueueStripes - 1))];

// The stripe this thread installs its queues in, given out in turn the first time the thread needs one.
// The runtime allocates the thread's storage for the field at that first read (32 bytes on .NET 10).
[System.ThreadStatic] private static QueueStripe? t_stripe;

private static int s_stripeTurn;

// queueOf returns the wait queue a handle names, or null: a handle whose queue was given up, or bytes
// that were never a handle.
internal static WaitQueue? queueOf(uint32 handle) {
    QueueStripe stripe = stripeOf(handle);

    lock (stripe) {
        return stripe.find(handle);
    }
}

// gateOf returns the mutex's wait queue with the caller counted as one of its waiters, installing a
// queue if the mutex has none. The caller gives the queue back with leave. Reached only from a
// contended Lock: an uncontended Lock/Unlock pair never comes here.
//
// While a waiter holds the queue, the mutex's handle cannot change: only leave clears it, and only
// when it counts the last waiter out; and a handle whose queue is in the table is never replaced. So an
// Unlock that follows a waiter's swap of `state` reads that waiter's handle and finds that waiter's
// queue.
//
// A queue and its handle are installed together under the stripe's lock, and every reader of the table
// takes that lock, so a thread that reads a handle from a mutex finds its queue, or finds none because
// the queue was given up. A handle with no queue is replaced like a zero one: the handle of a queue
// given up since the read, or bytes the mutex never wrote. Takes the mutex BY REF: the methods that
// call it are `ref Mutex` primaries, and the mutex can live where no box reaches it.
internal static WaitQueue gateOf(ref Mutex m) {
    while (true) {
        uint32 seen = Volatile.Read(ref m.sema);
        QueueStripe stripe;

        if (seen != 0) {
            stripe = stripeOf(seen);

            lock (stripe) {
                if (stripe.find(seen) is { } queue) {
                    // The second read: the mutex still holds the handle now that the queue is known to
                    // be in the table, so the queue is this mutex's, and not another's under a handle
                    // the sequence came round to since the first read.
                    if (Volatile.Read(ref m.sema) != seen) {
                        continue;
                    }

                    queue.waiters++;
                    return queue;
                }
            }
        }

        stripe = t_stripe ??= s_stripes[(int)((uint)Interlocked.Increment(ref s_stripeTurn) % QueueStripes)];

        lock (stripe) {
            // A lost CompareAndSwap means another waiter installed a queue, or the queue behind `seen`
            // was given up, since the read: look again.
            uint32 handle = stripe.newHandle();

            if (Interlocked.CompareExchange(ref m.sema, handle, seen) == seen) {
                return stripe.install(handle);
            }
        }
    }
}

// leave gives back a queue gateOf returned. The last waiter out takes the queue out of the table and
// writes 0 back into the mutex, so an idle mutex holds no handle. Both happen under the lock gateOf
// counts under, so no waiter is counted into a queue that is being given up.
internal static void leave(ref Mutex m, WaitQueue queue) {
    uint32 handle = queue.handle;
    QueueStripe stripe = stripeOf(handle);

    lock (stripe) {
        if (--queue.waiters == 0) {
            stripe.retire(queue);
            Interlocked.CompareExchange(ref m.sema, 0, handle);
        }
    }
}

} partial class syncᴛMutex {

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
// - every Unlock that still finds a stamped waiter outstanding charges the head wait (see TryHandoff),
//   where Go charges only an Unlock that semreleases to a waiter, so event COUNTS differ and a woken
//   waiter's run gap is included in its next charge. "Still finds": the charge follows the swap that
//   unlocks, so a waiter the previous Unlock woke can take the lock and retire its stamp before the
//   charge, and that Unlock records nothing, as Go's does (unlockSlow returns on mutexWoken);
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

} partial class sync_package {

// The acquire times of a queue's stamped waiters, Go's per-sudog acquiretime reduced to the two its
// semrelease1 reads: the HEAD (the waiter a handoff dequeues) and the TAIL (the newest), plus the count
// between them. Kept beside the queue rather than in Mutex, so Mutex keeps Go's layout, and
// reached only while stamped waiters exist anywhere (s_outstanding), so an Unlock with profiling off
// pays one volatile read.
internal sealed class WaitStamps {
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

internal static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SemaphoreSlim, WaitStamps> s_waitStamps = new();

} partial class syncᴛMutex {

// TryLock tries to lock m and reports whether it succeeded.
[GoRecv] public static bool TryLock(this ref Mutex m) => Interlocked.CompareExchange(ref m.state, 1, 0) == 0;

// The M:N twin of Lock, which a twinned program's twin awaits (M:N scheduler, experimental, opt-in: GoTwins=true and
// GO2CS_MN=1; inert otherwise). FROZEN SURFACE from .5 (COORD's ruling, docs/phase4/inbox/G/20261122T114603Z-COORD.md;
// row 7 pieces 2+3, DESIGN-mn-stdlib-twins.md): generated code in user assemblies names it, so its signature holds.
//
// A contended Lock PARKS here instead of blocking its carrier. Before row 7 this twin was `internal Lockꞏtwin(this ref
// Mutex)` and forwarded to Lock, so every contended Lock in a twinned program blocked an OS thread: MnMixedStorm at
// 100,000 goroutines held 25,409 threads in Lock and hung about 1 run in 10 (the train-4 capture). The receiver is the
// BOX: the lock word is written after the await, which a `ref` cannot cross, and with no own-shape `this ref` twin
// beside it the generator routes a twinned caller's `s.mu.Lock()` here, naming `s.mu`'s box without a copy.
//
// The protocol is Lock's, on the same gate: the CAS fast path (an uncontended Lock allocates nothing and parks
// nowhere), then gateOf, the swap to 2, and leave. The only difference is the wait: the gate's WaitAsync completes a
// GoParkSlot, so the goroutine parks; Unlock's Release serves a parked waiter and a blocked one (plain Lock, profiling
// on) from the one SemaphoreSlim. Off a carrier (GoScheduler.OnCarrier: the scheduler off, or a thread it did not
// make, where a park would throw and abandon the queued waiter), and with the block or mutex profile on (so its stamps,
// charges and Lock frame stay exactly Lock's), it takes plain Lock: the latter is the stated cost of profiling a
// twinned program.
//
// THE WAKE COST. SemaphoreSlim completes an async waiter's task with RunContinuationsAsynchronously, so a parked waiter
// is woken through ONE ThreadPool hop: Unlock's Release queues the task's completion, a pool thread runs the
// continuation (Wake), and Wake queues the goroutine's step for a carrier. ExecuteSynchronously only keeps the
// continuation on that pool thread. MnLockWake reads the hand-off's latency tail (row 7's record, section 6).
//
// WEAK (N1, DESIGN-mn-nopark.md 3.2): a call to Lock never makes its caller a twin. A caller that is a twin for another
// reason awaits this twin as above; a caller with no other park stays plain, and a contended Lock there blocks its
// thread. sync is hand-owned and no generator runs over it, so the mark is written here by hand.
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[global::go.golib.GoParkWeak]
public static global::go.golib.GoTask Lockꞏtwin(this ж<Mutex> Ꮡm) {
    if (Interlocked.CompareExchange(ref Ꮡm.Value.state, 1, 0) == 0) {
        return default;
    }
    if (!global::go.golib.GoScheduler.OnCarrier || runtime_package.GoBlockProfileOn || runtime_package.GoMutexProfileOn) {
        Ꮡm.Value.Lock();
        return default;
    }
    return lockSlowAsync(Ꮡm);
}

private static async global::go.golib.GoTask lockSlowAsync(ж<Mutex> Ꮡm) {
    WaitQueue gate = gateOf(ref Ꮡm.Value);
    try {
        Goroutine.ParkScope scope = Goroutine.ParkTwin(WaitReason.SyncMutexLock);
        try {
            while (Interlocked.Exchange(ref Ꮡm.Value.state, 2) != 0) {
                System.Threading.Tasks.Task wait = gate.WaitAsync();
                if (wait.IsCompleted) {
                    continue;
                }
                global::go.golib.GoParkSlot slot = new();
                _ = wait.ContinueWith(static (_, state) => ((global::go.golib.GoParkSlot)state!).Wake(), slot,
                    CancellationToken.None, System.Threading.Tasks.TaskContinuationOptions.ExecuteSynchronously, System.Threading.Tasks.TaskScheduler.Default);
                await slot;
            }
        } finally {
            scope.Dispose();
        }
    } finally {
        leave(ref Ꮡm.Value, gate);
    }
}

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
// The wakeup is sent outside the stripe's lock, so the queue can be given up, and handed to another
// mutex, between the lookup and the Release. The wakeup then reaches a spare queue, which drops it when
// it is next installed, or a waiter of the other mutex, which re-checks its `state` and waits again. No
// waiter of THIS mutex misses it: a queue is given up only when every waiter that held it has the lock.
// With a profile rate on, the same late lookup can charge a stamped wait of the other mutex to this
// unlocker: one misplaced mutex event.
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
    bool stamped = Volatile.Read(ref WaitStamps.s_outstanding) != 0;
    uint32 handle;
    if ((old == 2 || stamped) && (handle = Volatile.Read(ref m.sema)) != 0 && queueOf(handle) is { } gate) {
        if (stamped && s_waitStamps.TryGetValue(gate, out WaitStamps? stamps) && stamps.TryHandoff(out int64 dt)) {
            runtime_package.GoSyncMutexEvent(dt);
        }
        if (old == 2 && gate.CurrentCount == 0) {
            gate.Release();
        }
    }
}

} partial class sync_package {

} // end sync_package
