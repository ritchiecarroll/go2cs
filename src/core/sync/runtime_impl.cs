// runtime_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of sync's //go:linkname runtime primitives. In Go these are provided by
// the runtime (sema.go / proc.go); go2cs emits them as bodyless `partial` methods, and without a body
// here the PartialStubGenerator would fill them with a throwing stub — so sync.init() (which registers a
// Pool cleanup) and every Mutex/RWMutex/WaitGroup/Once/Cond operation would crash at first use. The
// converted Mutex/RWMutex/WaitGroup/Cond state-machine logic is faithful to Go; only this runtime layer
// beneath it needs a real body.
//
// The sleeping semaphore itself is NO LONGER HERE: it is go.golib.RuntimeSemaphore, hoisted verbatim so
// that this companion and internal/sync's (new at Go 1.24, where sync.Mutex became a wrapper holding an
// isync.Mutex) call ONE primitive instead of keeping two copies with two waiter tables. golib is the
// only home both can reach: sync → internal/sync is a real import, so the primitive cannot live in sync,
// and internal/sync may reference nothing but sync/atomic — a back-reference would be the W1 project-
// graph cycle class. The pointer-identity keying, the handoff contract that Go's starvation mode relies
// on, and the seeded-count rule are documented at RuntimeSemaphore.cs; this file keeps only the linkname
// bodies that forward to it.
//
// Known Phase-4 limitation: the notify-list entries BELOW persist for the process lifetime (a bounded
// leak for programs that churn many short-lived Conds). The semaphore's own bucket accumulation moved
// with the machinery and is recorded, unchanged, in RuntimeSemaphore.cs.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
// Aliased rather than imported wholesale: this file needs exactly three golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using Goroutine = go.golib.Goroutine;
using WaitReason = go.golib.WaitReason;
using RuntimeSemaphore = go.golib.RuntimeSemaphore;
using Stopwatch = System.Diagnostics.Stopwatch;

// Hand-owned (no runtime_impl.go exists, so a reconvert never regenerates it); marked for consistency
// with the other hand-owned sync files.
[module: go.GoManualConversion]

namespace go;

partial class sync_package
{

    internal static partial void runtime_Semacquire(ж<uint32> s) => RuntimeSemaphore.Acquire(s, WaitReason.Semacquire);

    internal static partial void runtime_SemacquireMutex(ж<uint32> s, bool lifo, nint skipframes) => RuntimeSemaphore.Acquire(s, WaitReason.SyncMutexLock);

    internal static partial void runtime_SemacquireRWMutex(ж<uint32> s, bool lifo, nint skipframes) => RuntimeSemaphore.Acquire(s, WaitReason.SyncRWMutexLock);

    internal static partial void runtime_SemacquireRWMutexR(ж<uint32> s, bool lifo, nint skipframes) => RuntimeSemaphore.Acquire(s, WaitReason.SyncRWMutexRLock);

    internal static partial void runtime_Semrelease(ж<uint32> s, bool handoff, nint skipframes) => RuntimeSemaphore.Release(s, handoff);

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
