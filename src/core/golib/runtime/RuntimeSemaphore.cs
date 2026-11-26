// RuntimeSemaphore.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.
//
// Parts of this file are inspired by Go, whose notice they carry (license of record: LICENSES/BSD-3-Clause.txt):
//
// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// PROVENANCE, part by part:
// - cansemacquire, SemaBucket.Queue, SemaBucket.Dequeue, Acquire: INSPIRED BY src/runtime/sema.go, Go 1.26.11, funcs
//   cansemacquire, semacquire1; methods semaRoot.queue, semaRoot.dequeue. The same atomic count, FIFO or LIFO queue
//   and acquire order, per address in a managed table.
// - Release's handoff over golib's goroutines and the bucket table: original go2cs code.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace go.golib;

/// <summary>
/// Go's sleeping semaphore — the primitive behind <c>Mutex</c>, <c>RWMutex</c> and
/// <c>WaitGroup</c>.
/// </summary>
/// <remarks>
/// <para>
/// This is the machinery Go keeps in <c>runtime/sema.go</c> and reaches through the
/// <c>sync_runtime_Semacquire*</c> / <c>sync_runtime_Semrelease</c> linkname family. It lives in
/// golib rather than in a package companion because at Go 1.24 <b>TWO</b> packages declare those
/// stubs: <c>sync</c> keeps <c>Semacquire</c>, <c>SemacquireRWMutex</c>, <c>SemacquireWaitGroup</c>
/// and <c>Semrelease</c>, while the new <c>internal/sync</c> — which <c>sync.Mutex</c> now wraps
/// (<c>type Mutex struct { mu isync.Mutex }</c>) — declares <c>SemacquireMutex</c> and its own
/// <c>Semrelease</c>.
/// </para>
/// <para>
/// <c>internal/sync</c> cannot call into <c>sync</c>: the corpus reference runs
/// <c>sync → internal/sync</c>, so a back-reference is a project-graph CYCLE, which
/// <c>check-solution-integrity.ps1</c> refuses per GOOS. The two companions could each hold a copy —
/// their semaphore populations are provably disjoint, since <c>internal/sync</c> declares BOTH the
/// acquire and the release half and the word lives in <c>isync.Mutex</c>, which only that package
/// touches — but a duplicate is two branches of one rule waiting to drift, which is precisely the
/// defect shape paid for elsewhere in this tree. ONE primitive, two callers.
/// </para>
/// <para>
/// <b>The table is keyed by POINTER IDENTITY.</b> <see cref="ж{T}"/>'s equality is
/// <c>ReferenceEquals</c> or equal order tokens and its hash is the order token, so two boxes over
/// one semaphore word resolve to one bucket — which is the property the whole protocol rests on.
/// </para>
/// <para>
/// <b>A bucket lives only while someone needs it.</b> Whoever leaves a bucket with an empty queue, under
/// its lock, retires it: marks it and removes it from the table. A thread that fetched it before then
/// finds the mark under the same lock and fetches again, so every live bucket is the one the table holds.
/// A retired bucket is never reused (a holder of the old reference would take it for another word's). So a
/// word that blocked once keeps nothing, and its key (the pointer box) no longer keeps the word's owner
/// alive. In Go an address is in its semaRoot's tree only while a goroutine waits on it (sema.go,
/// semaRoot.queue and dequeue).
/// </para>
/// <para>
/// <b>The count is changed atomically, as Go changes it</b> (<c>cansemacquire</c>'s compare-and-swap,
/// <c>semrelease1</c>'s <c>xadd</c>), so the uncontended acquire and release touch no bucket: neither makes
/// one, and neither allocates. Only an acquirer that finds the count at zero takes a bucket, and only a
/// releaser that finds a bucket in the table looks for a waiter.
/// The table had a <c>GetOrAdd</c> and no removal until sync.RWMutex took Go's layout and began to block
/// here; each RWMutex that ever blocked would then have kept a bucket and its owner for the process.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public static class RuntimeSemaphore
{
    private sealed class SemaWaiter
    {
        internal readonly ManualResetEventSlim Signal = new(false);
        internal bool HandedOff;

        // Set by the WAITER as the last act of Acquire after a handoff, and waited on by the releaser:
        // Go's goyield (see Release). Created only on a handoff, under the bucket lock, so the ordinary
        // park pays nothing for it. Spin count 0: a releaser that spun would see the signal and race
        // the waiter's very next instructions, which is the ordering the yield exists to give away.
        internal ManualResetEventSlim? Resumed;

        // The goroutine parked on this waiter (Go's sudog.g): the acquirer constructs it on its own
        // thread, just before parking. Release readies it before signalling.
        internal readonly Goroutine? Parker = Goroutine.Current;

        // The next waiter in this word's queue (Go's sudog.waitlink). Written only under the bucket lock.
        internal SemaWaiter? Next;
    }

    // The bucket carries the LOCK and the waiter queue only. The COUNT lives where Go keeps it —
    // in the uint32 the pointer addresses — because callers may seed it: sync's TestSemaphore does
    // `s := new(uint32); *s = 1` and then expects the first Semacquire to succeed without parking.
    // A bucket-private counter starting at zero parked that first acquirer forever with no one left
    // to release it, and the test deadlocked. Mutex/RWMutex/WaitGroup seed nothing, so their `sema`
    // field is zero and behaves exactly as before.
    private sealed class SemaBucket
    {
        // The word's waiters, first to be woken at the head (Go's per-address list in semaRoot: a sudog chain
        // through waitlink, with waittail). Every member is read and written only under this bucket's lock.
        private SemaWaiter? head, tail;
        internal int Count;

        // Go's semaRoot.queue(addr, s, lifo): a lifo waiter takes the head ("Substitute s in t's place"),
        // any other waiter the tail.
        internal void Queue(SemaWaiter w, bool lifo)
        {
            if (head is null)
                head = tail = w;
            else if (lifo)
            {
                w.Next = head;
                head = w;
            }
            else
            {
                tail!.Next = w;
                tail = w;
            }

            Count++;
        }

        // Go's semaRoot.dequeue: the head leaves first.
        internal SemaWaiter Dequeue()
        {
            SemaWaiter w = head!;
            head = w.Next;
            w.Next = null;

            if (head is null)
                tail = null;

            Count--;
            return w;
        }

        // Set, under this bucket's lock, by whoever removed it from the table. A thread that fetched the
        // bucket before then reads it under the same lock and fetches the word's current bucket instead.
        internal bool Retired;
    }

    private static readonly ConcurrentDictionary<ж<uint32>, SemaBucket> semaTable = new();

    // How many times a releaser yielded to a waiter it handed a permit to (Go's goyield after
    // `s.ticket == 1`), and how many of those waits hit the bound. Read by RuntimeSemaphoreHandoffTests.
    internal static long s_handoffYields;
    internal static long s_handoffYieldTimeouts;

    private static SemaBucket bucketFor(ж<uint32> s) => semaTable.GetOrAdd(s, static _ => new SemaBucket());

    // Go's cansemacquire (sema.go): take one permit if the count is above zero, by compare-and-swap, with
    // no bucket and no lock. A permit that is there can always be taken without losing a wakeup; a zero
    // count sends the acquirer to its bucket.
    private static bool cansemacquire(ж<uint32> s)
    {
        ref uint32 count = ref s.Value;

        while (true)
        {
            uint32 v = Volatile.Read(ref count);

            if (v == 0)
                return false;

            if (Interlocked.CompareExchange(ref count, v - 1, v) == v)
                return true;
        }
    }

    // Called under b's lock by whoever may have left its queue empty. Removes exactly this bucket: the key
    // and value pair, so a newer bucket for the same word is never touched.
    private static void retireIfIdle(ж<uint32> s, SemaBucket b)
    {
        if (b.Count != 0)
            return;

        b.Retired = true;
        semaTable.TryRemove(new KeyValuePair<ж<uint32>, SemaBucket>(s, b));
    }

    /// <summary>
    /// Acquires the semaphore addressed by <paramref name="s"/>, parking until a permit is
    /// available.
    /// </summary>
    // `reason` is park ACCOUNTING only — it never reaches the protocol, exactly as in Go, where
    // semacquire1's own `reason waitReason` parameter is handed straight to goparkunlock and
    // nothing else reads it. It is a PARAMETER rather than a constant for the same reason Go makes
    // it one: ONE semaphore serves four different Go-level waits, and a traceback has to name which
    // (sema.go's sync_runtime_Semacquire / SemacquireMutex / SemacquireRWMutexR / SemacquireRWMutex
    // pass waitReasonSemacquire, SyncMutexLock, SyncRWMutexRLock and SyncRWMutexLock respectively).
    // This form queues at the tail: Go's callers that pass lifo = false (Semacquire, WaitGroup, RWMutex).
    public static void Acquire(ж<uint32> s, WaitReason reason) => Acquire(s, lifo: false, reason);

    /// <summary>
    /// Acquires the semaphore addressed by <paramref name="s"/>, parking until a permit is available; a
    /// waiter that parks goes to the head of the word's queue when <paramref name="lifo"/> is set.
    /// </summary>
    // Go's semacquire1(addr, lifo, ...). sync.Mutex passes lifo for a waiter that has already waited
    // (lockSlow's queueLifo, waitStartTime != 0), so that waiter is woken before every waiter queued after
    // it. That half belongs with the conditional handoff in Release: a starving Mutex's longest waiter that
    // is readied but loses the permit to a newcomer's easy case re-queues at the head, and wins the next
    // round, as in Go.
    public static void Acquire(ж<uint32> s, bool lifo, WaitReason reason)
    {
        while (true)
        {
            // Go's easy case (semacquire1: `if cansemacquire(addr) { return }`): an available permit is
            // taken without a bucket, so an uncontended acquire allocates nothing and leaves nothing behind.
            if (cansemacquire(s))
                return;

            // Fetched on every pass: a woken waiter's bucket may have been retired by the Release that woke it.
            SemaBucket b = bucketFor(s);
            SemaWaiter w;
            bool locked = false;

            try
            {
                Monitor.Enter(b, ref locked);

                if (b.Retired)
                    continue; // removed after we fetched it; the finally releases its lock

                // Re-checked under the lock, which is a full fence after the bucket was published: a Release
                // whose add came later finds this bucket (Release's TryGetValue) and the waiter queued below.
                if (cansemacquire(s))
                {
                    retireIfIdle(s, b); // acquired without parking
                    return;
                }

                // Every pass queues with the caller's lifo, as Go's semacquire1 loop does.
                w = new SemaWaiter();
                b.Queue(w, lifo);

                // Go's commit order (semacquire1 -> goparkunlock(&root.lock)): the park is entered
                // while the bucket lock that publishes the waiter is still held, so Release can only
                // ever find a parked goroutine to ready. The WAIT is outside the lock, as before.
                using (Goroutine.Park(reason))
                {
                    Monitor.Exit(b);
                    locked = false;
                    w.Signal.Wait();
                }
            }
            finally
            {
                if (locked)
                    Monitor.Exit(b);
            }

            if (w.HandedOff)
            {
                // Ownership was handed to us directly (starvation mode). Tell the releaser we are
                // running -- after the park scope has returned us to _Grunning -- so it can stop
                // yielding to us (Release, Go's goyield).
                w.Resumed?.Set();
                return;
            }

            // Normal wake: we were merely readied — re-compete for the count.
        }
    }

    /// <summary>
    /// The number of goroutines queued on the semaphore addressed by <paramref name="s"/> -- Go's
    /// <c>semaRoot.nwait</c>, read per word rather than per root, which is what runtime's export_test
    /// <c>SemNwait</c> asks. A word nobody has waited on has no bucket and reads zero without making one.
    /// </summary>
    public static int Waiters(ж<uint32> s)
    {
        if (!semaTable.TryGetValue(s, out SemaBucket? b))
            return 0;

        lock (b)
            return b.Count;
    }

    /// <summary>
    /// Releases the semaphore addressed by <paramref name="s"/>, optionally handing ownership
    /// directly to the next waiter (Go's starvation mode).
    /// </summary>
    public static void Release(ж<uint32> s, bool handoff)
    {
        SemaWaiter? w = null;

        // Go's order (semrelease1): add the permit, then look for waiters (`atomic.Load(&root.nwait) == 0`
        // returns at once). Here a word with waiters has a bucket in the table, and an Acquire publishes its
        // bucket before it reads the count under the bucket's lock; the add below is a full fence, so either
        // that read sees the permit or this lookup sees the bucket. A word nobody waits on is looked up and
        // left alone: no bucket is made, so an uncontended release allocates nothing.
        Interlocked.Increment(ref s.Value);

        while (semaTable.TryGetValue(s, out SemaBucket? b))
        {
            lock (b)
            {
                if (b.Retired)
                    continue; // removed after we fetched it; look again

                if (b.Count > 0)
                {
                    w = b.Dequeue();

                    // Go: `if handoff && cansemacquire(addr) { s.ticket = 1 }`. The permit just added may
                    // already have been taken by an acquirer's easy case; then w is only readied and
                    // re-competes, as in Go.
                    if (handoff && cansemacquire(s))
                    {
                        w.HandedOff = true;
                        w.Resumed = new ManualResetEventSlim(false, spinCount: 0);
                    }
                }

                retireIfIdle(s, b);
                break;
            }
        }

        if (w is null)
            return;

        // Go's readyWithTime -> goready, on the releaser's side, before the signal.
        Goroutine.Ready(w.Parker);
        w.Signal.Set();

        // GO'S DIRECT HANDOFF YIELD (sema.go semrelease1: `if s.ticket == 1 && getg().m.locks == 0 {
        // goyield() }`). Having handed the permit over, Go queues the RELEASER behind the readied waiter on
        // the same P, so the waiter -- which "inherits our time slice" -- runs first; runtime's
        // TestSemaHandoff asserts that ordering. Here every goroutine is its own thread, so returning at
        // once let the releaser win in nanoseconds while the waiter was still waking (1-8 per 10,000 on
        // P1's linux readings, 0 of 1,000 in RuntimeSemaphoreHandoffTests). The releaser instead waits
        // until the waiter has resumed.
        //
        // BOUNDED, by Go's forcePreemptNS (10 ms): the time slice after which Go's own sysmon would
        // preempt the waiter and let the releaser run. Go's `m.locks == 0` has no managed subject -- a
        // releaser holding something the waiter's resume path needed would otherwise wait forever -- and
        // the bound is what stands in for it: such a releaser loses 10 ms, never deadlocks. (A timed
        // kernel wait rounds up to the timer resolution; the measured worst case is recorded with this
        // seat.) Only a handoff that actually handed a permit yields, as only `ticket == 1` does in Go.
        if (w.Resumed is { } resumed)
        {
            Interlocked.Increment(ref s_handoffYields);

            if (!resumed.Wait(HandoffYieldBoundMs))
                Interlocked.Increment(ref s_handoffYieldTimeouts);
        }
    }

    // Go's forcePreemptNS (proc.go: `forcePreemptNS = 10 * 1000 * 1000 // 10ms`).
    private const int HandoffYieldBoundMs = 10;
}
