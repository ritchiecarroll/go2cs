// RuntimeSemaphore.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

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
/// ⚠ <b>Known, pre-existing, and deliberately NOT changed here:</b> the table has a
/// <c>GetOrAdd</c> and no removal path, so a semaphore word's bucket outlives its owner for the
/// life of the process. That accumulation came with this code and is carried across unchanged,
/// because this move is required to be zero-behaviour-change; retiring it is its own increment with
/// its own measurement.
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
    }

    // The bucket carries the LOCK and the waiter queue only. The COUNT lives where Go keeps it —
    // in the uint32 the pointer addresses — because callers may seed it: sync's TestSemaphore does
    // `s := new(uint32); *s = 1` and then expects the first Semacquire to succeed without parking.
    // A bucket-private counter starting at zero parked that first acquirer forever with no one left
    // to release it, and the test deadlocked. Mutex/RWMutex/WaitGroup seed nothing, so their `sema`
    // field is zero and behaves exactly as before.
    private sealed class SemaBucket
    {
        internal readonly Queue<SemaWaiter> Waiters = new();
    }

    private static readonly ConcurrentDictionary<ж<uint32>, SemaBucket> semaTable = new();

    // How many times a releaser yielded to a waiter it handed a permit to (Go's goyield after
    // `s.ticket == 1`), and how many of those waits hit the bound. Read by RuntimeSemaphoreHandoffTests.
    internal static long s_handoffYields;
    internal static long s_handoffYieldTimeouts;

    private static SemaBucket bucketFor(ж<uint32> s) => semaTable.GetOrAdd(s, static _ => new SemaBucket());

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
    public static void Acquire(ж<uint32> s, WaitReason reason)
    {
        SemaBucket b = bucketFor(s);

        while (true)
        {
            SemaWaiter w;
            bool locked = false;

            try
            {
                Monitor.Enter(b, ref locked);

                if (s.Value > 0)
                {
                    s.Value--; // acquired without parking
                    return;
                }

                w = new SemaWaiter();
                b.Waiters.Enqueue(w);

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
            return b.Waiters.Count;
    }

    /// <summary>
    /// Releases the semaphore addressed by <paramref name="s"/>, optionally handing ownership
    /// directly to the next waiter (Go's starvation mode).
    /// </summary>
    public static void Release(ж<uint32> s, bool handoff)
    {
        SemaBucket b = bucketFor(s);
        SemaWaiter? w = null;

        lock (b)
        {
            s.Value++;

            if (b.Waiters.Count > 0)
            {
                w = b.Waiters.Dequeue();

                if (handoff)
                {
                    s.Value--;        // hand the just-added permit directly to w
                    w.HandedOff = true;
                    w.Resumed = new ManualResetEventSlim(false, spinCount: 0);
                }
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
