// sema_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The runtime's own semaphore, runtime.semacquire1 / runtime.semrelease1 (sema.go).
//
// WHY. The converted semacquire1 parks the caller through Go's sudog machinery: acquireSudog reads the
// caller's P (`mp.p.ptr()`) for its per-P sudog cache. The managed runtime has no Ps -- every goroutine
// is its own thread and m.p is nil by construction (stubs_impl.cs, getg) -- so the first semacquire that
// has to WAIT dereferenced a nil P on a goroutine and the runtime row's test host died there
// (TestSemaHandoff, measured on linux 2026-09-24: 10,669 of 10,884 results, 124 top-level tests unrun).
//
// WHAT THESE BODIES ARE. The waiting half of both functions over golib's RuntimeSemaphore -- the SAME
// primitive sync's and internal/sync's semaphore pulls already stand on (sync/runtime_impl.cs,
// internal/sync/runtime_impl.cs), so runtime's own callers (metricsSema, worldsema, traceAdvanceSema,
// and export_test's Semacquire/Semrelease1) now park the way every other Go semaphore in the corpus
// parks: Go's commit order on the goroutine's own gate, the count kept in the uint32 the pointer
// addresses, and handoff giving the released permit directly to the first waiter.
//
// WHAT IS DROPPED, AND WHY. The sudog, the per-P sudog cache and the semaRoot treap exist to park a g on
// a P-less queue and to find it again by address; RuntimeSemaphore's pointer-keyed bucket is that queue.
// Named, not modelled -- the same three internal/sync's bodies name: `lifo` (the queue is FIFO; a
// fairness difference, not a correctness one), `skipframes`, and the block/mutex profile flags (the
// structural "no contention samples" class). Go's "semacquire not on the G stack" check has no subject:
// there is no g0 to be on.
//
// SemNwait. export_test.go's SemNwait reads semtable.rootFor(addr).nwait, the treap's waiter count, which
// nothing maintains once the treap is gone; export_impl_test.cs answers it from RuntimeSemaphore.Waiters,
// the count of goroutines queued on that word. Go's count is per semaRoot (every word hashing to the
// root); this one is per word, which is what TestSemaHandoff's `for SemNwait(&sema) == 0` asks.
//
// Hand-owned (no sema_impl.go exists, so a reconvert never regenerates this file).

using System;
using System.Linq;
using go.golib;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    // sema.go:142. Go's harder case (sudog, semaRoot queue, goparkunlock) is RuntimeSemaphore.Acquire;
    // the easy case, cansemacquire, is its first step under the bucket lock.
    internal static void semacquire1(ж<uint32> Ꮡaddr, bool lifo, semaProfileFlags profile, nint skipframes, waitReason reason) =>
        RuntimeSemaphore.Acquire(Ꮡaddr, golibWaitReasonOf(reason));

    // sema.go:203. Go's xadd, the waiter search and readyWithTime are RuntimeSemaphore.Release; with
    // handoff the released permit goes to the first waiter rather than back to the count.
    internal static void semrelease1(ж<uint32> Ꮡaddr, bool handoff, nint skipframes) =>
        RuntimeSemaphore.Release(Ꮡaddr, handoff);

    /// <summary>How many goroutines are queued on the semaphore word <paramref name="addr"/>.</summary>
    public static uint32 GoSemaWaiters(ж<uint32> addr) => (uint32)RuntimeSemaphore.Waiters(addr);

    // ---- the guard's view (RuntimeSemaphoreTests) ----

    /// <summary>
    /// A goroutine acquires a zero semaphore through runtime's own semacquire, and this thread releases
    /// it with handoff through runtime's semrelease1. Returns whether the acquirer came back holding the
    /// permit, and the acquirer's failure by name if it died instead.
    /// </summary>
    public static (bool acquired, string? acquirerFailure) GoSemacquireReleaseProbe(int timeoutMs)
    {
        using System.Threading.ManualResetEventSlim done = new(false);
        ж<uint32> Ꮡsema = new StandardBox<uint32>(default(uint32));
        bool acquired = false;
        string? acquirerFailure = null;

        Goroutine.Start(() =>
        {
            try
            {
                semacquire(Ꮡsema);
                acquired = true;
            }
            catch (Exception ex)
            {
                // Name the runtime frame it died in: a bare NullReferenceException does not say where.
                string? frame = new System.Diagnostics.StackTrace(ex).GetFrames()
                    .Select(f => f.GetMethod())
                    .FirstOrDefault(m => m?.DeclaringType == typeof(runtime_package))?.Name;

                acquirerFailure = $"{ex.GetType().Name} in runtime.{frame ?? "?"}: {ex.Message}";
            }

            done.Set();
        });

        // The acquirer either parks (the release then wakes it) or has not reached the semaphore yet
        // (the release then leaves a permit it takes without parking). Both end with it holding the
        // permit; only a failure on its way in ends earlier, and that is what this wait watches for.
        done.Wait(200);

        if (acquirerFailure is not null)
            return (false, acquirerFailure);

        semrelease1(Ꮡsema, true, 0);

        if (!done.Wait(timeoutMs))
            throw new TimeoutException("the acquirer never returned from semacquire after the release");

        return (acquired, acquirerFailure);
    }

    /// <summary>
    /// TestSemaHandoff's wait, `for SemNwait(&amp;sema) == 0 { Gosched() }`: a goroutine parks on a zero
    /// semaphore, and the count is read before it parks is seen, once it is, and after the release.
    /// </summary>
    public static (uint32 before, uint32 parked, uint32 after) GoSemaWaitersProbe(int timeoutMs)
    {
        using System.Threading.ManualResetEventSlim done = new(false);
        ж<uint32> Ꮡsema = new StandardBox<uint32>(default(uint32));
        uint32 before = GoSemaWaiters(Ꮡsema);

        Goroutine.Start(() =>
        {
            semacquire(Ꮡsema);
            done.Set();
        });

        System.Diagnostics.Stopwatch clock = System.Diagnostics.Stopwatch.StartNew();
        uint32 parked;

        while ((parked = GoSemaWaiters(Ꮡsema)) == 0 && clock.ElapsedMilliseconds < timeoutMs)
            System.Threading.Thread.Yield();

        semrelease1(Ꮡsema, false, 0);

        if (!done.Wait(timeoutMs))
            throw new TimeoutException("the waiter never returned from semacquire after the release");

        return (before, parked, GoSemaWaiters(Ꮡsema));
    }
}
