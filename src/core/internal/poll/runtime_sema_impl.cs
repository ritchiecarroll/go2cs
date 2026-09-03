// runtime_sema_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// Hand-written implementations of internal/poll's //go:linkname runtime semaphore primitives
// (fd_mutex.go). In Go these are provided by the runtime (sema.go); go2cs emits them as bodyless
// `partial` methods, and without a body here the PartialStubGenerator fills them with a throwing
// stub — so the first fdMutex teardown (every os.File Close, hence os.ReadFile/WriteFile) crashed
// with "runtime_Semrelease: external function is not implemented" (surfaced by the Phase-4
// ConvertedTestHarness fixture's testdata read).
//
// The sleeping semaphore mirrors the hand-owned sync/runtime_impl.cs port of Go's runtime
// semaphore, keyed by the address identity of the *uint32 — the ж<uint32> pointer, whose
// Equals/GetHashCode are identity-based (same &mu.rsema slot ⇒ same bucket). internal/poll's
// linknames have no handoff/starvation mode (unlike sync's Mutex), so a bucket is just a count
// plus a FIFO waiter queue: Semrelease wakes one parked waiter, wakers re-compete for the count.
// The uint32 the pointer addresses is never read outside these calls, so the count lives entirely
// in the bucket. Same known Phase-4 limitation as sync's: bucket entries persist for the process
// lifetime (a bounded leak for programs that churn many descriptors).

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
// Aliased rather than imported wholesale: this file needs exactly two golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using Goroutine = go.golib.Goroutine;
using WaitReason = go.golib.WaitReason;

// Hand-owned (no runtime_sema.go exists, so a reconvert never regenerates it); marked for
// consistency with the other hand-owned operational files.
[module: go.GoManualConversion]

namespace go.@internal;

partial class poll_package
{
    private sealed class SemaWaiter
    {
        internal readonly ManualResetEventSlim Signal = new(false);
    }

    private sealed class SemaBucket
    {
        internal uint Count;
        internal readonly Queue<SemaWaiter> Waiters = new();
    }

    private static readonly ConcurrentDictionary<ж<uint32>, SemaBucket> semaTable = new();

    private static SemaBucket bucketFor(ж<uint32> sema) => semaTable.GetOrAdd(sema, static _ => new SemaBucket());

    internal static partial void runtime_Semacquire(ж<uint32> sema)
    {
        SemaBucket bucket = bucketFor(sema);

        while (true)
        {
            SemaWaiter waiter;

            lock (bucket)
            {
                if (bucket.Count > 0)
                {
                    bucket.Count--; // acquired without parking
                    return;
                }

                waiter = new SemaWaiter();
                bucket.Waiters.Enqueue(waiter);
            }

            // `semacquire`, which is what Go's poll_runtime_Semacquire passes (sema.go:75-77). NOT
            // `IO wait` — that is netpollblock's reason and belongs to runtime_netpoll_impl.cs's
            // pollBlock, one layer up. Both live in internal/poll and they are different waits: this
            // one is the fdMutex's reference/lock counter, that one is the completion itself.
            using (Goroutine.Park(WaitReason.Semacquire))
                waiter.Signal.Wait();

            // Woken: re-compete for the count (no direct handoff in internal/poll's contract).
        }
    }

    internal static partial void runtime_Semrelease(ж<uint32> sema)
    {
        SemaBucket bucket = bucketFor(sema);
        SemaWaiter? waiter = null;

        lock (bucket)
        {
            bucket.Count++;

            if (bucket.Waiters.Count > 0)
                waiter = bucket.Waiters.Dequeue();
        }

        waiter?.Signal.Set();
    }
}
