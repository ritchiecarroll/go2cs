// runtime_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Hand-written implementations of sync's //go:linkname runtime primitives. In Go these are provided by
// the runtime (sema.go / proc.go); go2cs emits them as bodyless `partial` methods, and without a body
// here the PartialStubGenerator would fill them with a throwing stub — so sync.init() (which registers a
// Pool cleanup) and every Mutex/RWMutex/WaitGroup/Once/Cond operation would crash at first use. The
// converted Mutex/RWMutex/WaitGroup/Cond state-machine logic is faithful to Go; only this runtime layer
// beneath it needs a real body.
//
// The sleeping semaphore is a faithful port of Go's runtime semaphore (sema.go), keyed by the address of
// the *uint32 — here the ж<uint32> pointer, whose Equals/GetHashCode are IDENTITY-based (same &m.sema
// slot ⇒ same bucket; distinct fields ⇒ distinct buckets). Each bucket is a counter plus a FIFO waiter
// queue; crucially, Semrelease with handoff=true hands ownership DIRECTLY to the dequeued waiter (it
// returns already-acquired, without re-competing) — the exact contract Go's Mutex/RWMutex starvation
// mode relies on, and the reason a plain SemaphoreSlim (which cannot hand off to a specific waiter)
// trips "sync: inconsistent mutex state" / "unlock of unlocked mutex" under contention. The COUNT is
// the uint32 the pointer addresses, exactly as in Go — the bucket carries only the lock and the queue —
// because a caller may SEED it (sync's TestSemaphore starts at 1); see SemaBucket.
//
// Known Phase-4 limitation: bucket/notify-list entries persist for the process lifetime (a bounded leak
// for programs that churn many short-lived locks).

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
// Aliased rather than imported wholesale: this file needs exactly two golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using Goroutine = go.golib.Goroutine;
using WaitReason = go.golib.WaitReason;
using Stopwatch = System.Diagnostics.Stopwatch;

// Hand-owned (no runtime_impl.go exists, so a reconvert never regenerates it); marked for consistency
// with the other hand-owned sync files.
[module: go.GoManualConversion]

namespace go;

partial class sync_package
{
    // ---- Sleeping semaphore (Mutex, RWMutex, WaitGroup) -------------------------------------------

    private sealed class SemaWaiter
    {
        internal readonly ManualResetEventSlim Signal = new(false);
        internal bool HandedOff;
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

    private static SemaBucket bucketFor(ж<uint32> s) => semaTable.GetOrAdd(s, static _ => new SemaBucket());

    // `reason` is park ACCOUNTING only — it never reaches the protocol, exactly as in Go, where
    // semacquire1's own `reason waitReason` parameter is handed straight to goparkunlock and
    // nothing else reads it. It is a PARAMETER rather than a constant for the same reason Go makes
    // it one: ONE semaphore serves four different Go-level waits, and a traceback has to name which
    // (sema.go's sync_runtime_Semacquire / SemacquireMutex / SemacquireRWMutexR / SemacquireRWMutex
    // pass waitReasonSemacquire, SyncMutexLock, SyncRWMutexRLock and SyncRWMutexLock respectively).
    private static void semacquire(ж<uint32> s, WaitReason reason)
    {
        SemaBucket b = bucketFor(s);

        while (true)
        {
            SemaWaiter w;

            lock (b)
            {
                if (s.Value > 0)
                {
                    s.Value--; // acquired without parking
                    return;
                }

                w = new SemaWaiter();
                b.Waiters.Enqueue(w);
            }

            using (Goroutine.Park(reason))
                w.Signal.Wait();

            if (w.HandedOff)
                return; // ownership was handed to us directly (starvation mode)

            // Normal wake: we were merely readied — re-compete for the count.
        }
    }

    private static void semrelease(ж<uint32> s, bool handoff)
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
                }
            }
        }

        w?.Signal.Set();
    }

    internal static partial void runtime_Semacquire(ж<uint32> s) => semacquire(s, WaitReason.Semacquire);

    internal static partial void runtime_SemacquireMutex(ж<uint32> s, bool lifo, nint skipframes) => semacquire(s, WaitReason.SyncMutexLock);

    internal static partial void runtime_SemacquireRWMutex(ж<uint32> s, bool lifo, nint skipframes) => semacquire(s, WaitReason.SyncRWMutexLock);

    internal static partial void runtime_SemacquireRWMutexR(ж<uint32> s, bool lifo, nint skipframes) => semacquire(s, WaitReason.SyncRWMutexRLock);

    internal static partial void runtime_Semrelease(ж<uint32> s, bool handoff, nint skipframes) => semrelease(s, handoff);

    // ---- Cond notify-list -------------------------------------------------------------------------
    //
    // A faithful port of the runtime's TICKETED notify list (runtime/sema.go notifyListAdd/Wait/
    // NotifyOne/NotifyAll). Each waiter draws a monotonically increasing ticket from `Wait`; `Notify`
    // is the ticket of the next waiter to release. NotifyOne releases ticket `Notify` SPECIFICALLY —
    // never "whichever waiter the OS picks".
    //
    // A plain counting semaphore is NOT faithful here, and the divergence is exactly what
    // TestCondSignalStealing exercises: with banked permits, a waiter that arrives AFTER a
    // Signal/Broadcast can consume the permit that the ALREADY-parked waiter was owed, leaving the
    // first waiter parked forever (the test's first waiter never reaches its `ch <- struct{}{}` and
    // the whole suite wedges). Go forbids that by ticket: the stealer's own ticket is higher, so the
    // release for ticket t reaches only the waiter holding t.
    //
    // The unparked-yet race is covered the same way Go covers it: NotifyOne/NotifyAll advance
    // `Notify` even when the target has not enqueued, and a waiter entering Wait with an already-
    // notified ticket (`less(t, Notify)`) returns immediately rather than parking. Ticket comparison
    // is wraparound-safe (Go's `less`: signed difference), so the uint32 counters may wrap freely.

    private sealed class NotifyWaiter
    {
        internal readonly ManualResetEventSlim Signal = new(false);
        internal uint32 Ticket;
    }

    private sealed class NotifyState
    {
        internal readonly LinkedList<NotifyWaiter> Waiters = new();
        internal uint32 Wait;
        internal uint32 Notify;
    }

    // less reports whether ticket a precedes ticket b, tolerating uint32 wraparound (runtime's `less`).
    private static bool less(uint32 a, uint32 b) => unchecked((int32)(a - b)) < 0;

    private static readonly ConcurrentDictionary<ж<notifyList>, NotifyState> notifyTable = new();

    private static NotifyState notifyFor(ж<notifyList> l) => notifyTable.GetOrAdd(l, static _ => new NotifyState());

    internal static partial uint32 runtime_notifyListAdd(ж<notifyList> l)
    {
        NotifyState n = notifyFor(l);

        lock (n)
            return unchecked(n.Wait++);
    }

    internal static partial void runtime_notifyListWait(ж<notifyList> l, uint32 t)
    {
        NotifyState n = notifyFor(l);
        NotifyWaiter w;

        lock (n)
        {
            // Already notified before this waiter got a chance to park — nothing to wait for.
            if (less(t, n.Notify))
                return;

            w = new NotifyWaiter { Ticket = t };
            n.Waiters.AddLast(w);
        }

        // Go's notifyListWait parks with waitReasonSyncCondWait (sema.go:587), which is what makes a
        // traceback distinguish a Cond waiter from a mutex waiter on the same lock.
        using (Goroutine.Park(WaitReason.SyncCondWait))
            w.Signal.Wait();
    }

    internal static partial void runtime_notifyListNotifyOne(ж<notifyList> l)
    {
        NotifyState n = notifyFor(l);
        NotifyWaiter? target = null;

        lock (n)
        {
            if (n.Wait == n.Notify)
                return; // no outstanding tickets

            uint32 t = n.Notify;
            n.Notify = unchecked(t + 1);

            // Release the waiter holding EXACTLY ticket t. If it has not parked yet, advancing
            // Notify above is enough: its Wait call will see less(t, Notify) and return at once.
            for (LinkedListNode<NotifyWaiter>? node = n.Waiters.First; node is not null; node = node.Next)
            {
                if (node.Value.Ticket != t)
                    continue;

                target = node.Value;
                n.Waiters.Remove(node);
                break;
            }
        }

        target?.Signal.Set();
    }

    internal static partial void runtime_notifyListNotifyAll(ж<notifyList> l)
    {
        NotifyState n = notifyFor(l);
        NotifyWaiter[] targets;

        lock (n)
        {
            targets = [.. n.Waiters];
            n.Waiters.Clear();
            n.Notify = n.Wait;
        }

        foreach (NotifyWaiter w in targets)
            w.Signal.Set();
    }

    // Size-agreement sanity check between sync.notifyList and runtime's — irrelevant here.
    internal static partial void runtime_notifyListCheck(uintptr size) { }

    // (runtime.throw / runtime.fatal are defined natively in mutex.cs — used by the still-converted
    // rwmutex/cond as well as the native types.)

    // ---- Spin / timing ----------------------------------------------------------------------------

    internal static partial bool runtime_canSpin(nint i) => false;

    internal static partial void runtime_doSpin() => Thread.SpinWait(30);

    private static readonly long nanotimeBase = Stopwatch.GetTimestamp();

    internal static partial int64 runtime_nanotime() =>
        unchecked((long)((Stopwatch.GetTimestamp() - nanotimeBase) * (1_000_000_000.0 / Stopwatch.Frequency)));

    internal static partial uint32 runtime_randn(uint32 n) =>
        n == 0 ? 0u : unchecked((uint32)((ulong)System.Random.Shared.NextInt64() % n));

    // ---- Pool sharding and GC-time cleanup --------------------------------------------------------
    //
    // Go's procPin pins the goroutine to its P and returns the P's id; sync.Pool uses that id to index
    // a per-P shard, and the pin is what gives each shard's dequeue its SINGLE producer. There is no P
    // here — a goroutine IS a managed thread and the CLR schedules it — so the shard index is
    // THREAD-affine instead: a thread draws a sticky id once, in arrival order, and folds it into the
    // GOMAXPROCS in force. That delivers what the pin exists to deliver (a stable shard per concurrent
    // worker, so a Put and the Get after it reach the same private slot) without pretending to control
    // scheduling, and procUnpin has nothing to undo.
    //
    // Threads outnumber shards, so two of them CAN land on one shard — the one thing a real pin rules
    // out. This function only hands out the index; sync.Pool closes that gap on its own side with
    // interlocked private slots and a per-shard producer gate (see pool.cs).

    private static long s_nextProcId;

    [ThreadStatic]
    private static nint t_procId;

    [ThreadStatic]
    private static bool t_procIdAssigned;

    internal static partial nint runtime_procPin()
    {
        if (!t_procIdAssigned)
        {
            t_procId = (nint)(Interlocked.Increment(ref s_nextProcId) - 1);
            t_procIdAssigned = true;
        }

        nint procs = runtime_package.GOMAXPROCS(0);

        return procs > 1 ? t_procId % procs : 0;
    }

    internal static partial void runtime_procUnpin() { }

    // Hands sync's poolCleanup to the runtime, which runs it when a garbage collection is requested —
    // Go registers it the same way and the runtime calls it from gcStart → clearpools.
    internal static partial void runtime_registerPoolCleanup(Action cleanup) =>
        runtime_package.registerPoolCleanup(cleanup);

    internal static partial uintptr runtime_LoadAcquintptr(ж<uintptr> ptr) => ptr.Value;

    internal static partial uintptr runtime_StoreReluintptr(ж<uintptr> ptr, uintptr val)
    {
        ptr.Value = val;
        return val;
    }
}
