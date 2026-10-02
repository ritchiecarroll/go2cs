// runtime_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written bodies for ALL SEVEN of internal/sync's runtime pulls. Go 1.24 moved Mutex into this
// new package, and internal/sync/runtime.go declares seven //go:linkname hooks the runtime pushes:
// throw, fatal, runtime_canSpin, runtime_doSpin, runtime_nanotime, runtime_SemacquireMutex and
// runtime_Semrelease. The push does not arrive in this corpus, so PartialStubGenerator fills every
// one this file does not body with a throwing NotImplementedException.
//
// Until 2026-09-22 this file bodied only throw and fatal, and the other five were throwing stubs on
// Mutex.lockSlow's CONTENDED path: runtime_canSpin is its first call (mutex.cs:86), then
// runtime_nanotime (:128) and runtime_SemacquireMutex (:130), with runtime_Semrelease in unlockSlow
// (:207, :218). An uncontended Lock takes the CAS fast path and never gets there, which is why
// internal/sync's own suite validated. The first converted caller to CONTEND an internal/sync.Mutex
// died on the canSpin stub: i9 reached it through runtime/pprof. sync.Mutex is hand-owned over
// SemaphoreSlim and never reaches this file. The five bodies are in the second half of the class
// below; each cites its push line at the pin.
//
// REACH, measured at this tree rather than assumed: `fatal` has ONE caller, mutex.cs:190 in
// unlockSlow — an unlock of an unlocked Mutex, which is a program bug Go reports and exits on.
// `@throw` has NONE in this package today. That is not a reason to leave it throwing: an
// unreached stub is precisely the member that reaches a user as a dead host rather than as a
// failing gate, and the first caller to arrive would meet the wrong failure with no warning.

// WHY A COMPANION AND NOT A //go:linkname REGISTRY ROW. The converter CAN forward a push into the
// consumer package, and five of RED 7's members are being wired that way (crypto/internal/fips140's
// three, crypto/internal/sysrand's fatal and crypto/internal/fips140hash's sha3Unwrap — G's seat).
// A row is admissible only where the consumer's project ALREADY references the pushing package.
// internal.sync.csproj carries NO reference to runtime, and adding one would be the W1 project-graph
// cycle class that check-solution-integrity's per-GOOS assertion exists to catch — runtime sits above
// internal/sync. So these two take the companion, not the row.
// COORD's split at 1fa7940a0, on G's reading of the project files at 5bb307d57e.
//
// WHY golib HOLDS THE PRIMITIVE, and not `runtime`. golib is the only assembly BELOW every converted
// package. FatalReport.cs argues this for itself and names this very set of consumers — it was
// written for the fatal shims already known, and these are the members the 1.24 hop added to the
// same class. One primitive there, a one-line forward here.
//
// WHAT THE FORWARD PRESERVES. Go's fatal is UNRECOVERABLE from its first instruction: no recover(),
// no deferred function, no catch. FatalReport.Fatal writes `fatal error: <text>`, a blank line and a
// Go-spelled traceback, then calls Environment.Exit(2), so the unrecoverability is structural rather
// than a property of the exception type — which is exactly what a throwing stub does NOT give: a
// NotImplementedException is an ordinary managed exception any frame above can catch.
//
// userFault is Go's throwType axis, read from Go's OWN push at the pin rather than inferred:
// runtime/panic.go:1061 pushes internal_sync_throw, whose body is `throw(s)` — throwTypeRuntime,
// userFault FALSE. :1066 pushes internal_sync_fatal, whose body is `fatal(s)` — throwTypeUser,
// userFault TRUE. The two differ, which is why this is two forwards and not one with a flag.

// Aliased rather than imported wholesale: this file needs exactly these golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using FatalReport = go.golib.FatalReport;
using MonotonicClock = go.golib.MonotonicClock;
using RuntimeSemaphore = go.golib.RuntimeSemaphore;
using WaitReason = go.golib.WaitReason;
using Thread = System.Threading.Thread;

// Hand-owned (no *_impl.go exists, so a reconvert never regenerates it); marked so the marker-based
// readers see it as well as the suffix-based ones.
[module: go.GoManualConversion]

#pragma warning disable CS8826 // the converted partial names the blank Go parameter `_`; this hand-owned body keeps its readable name

namespace go.@internal;

partial class sync_package
{
    internal static partial void @throw(@string s) => FatalReport.Fatal(s, userFault: false);

    internal static partial void fatal(@string s) => FatalReport.Fatal(s, userFault: true);

    // ---- The five Mutex hooks ----------------------------------------------------------------------
    //
    // They are companions rather than registry pushes for the same reason as the two above: this
    // project may not reference runtime. There is also a second reason, specific to canSpin. Go's
    // pushed body reads getg().m.p, runqempty(p) and the sched.npidle / sched.nmspinning counters,
    // and the managed model keeps none of them, so a verbatim push would either nil-dereference or
    // return an answer based on counters that are never updated.

    // runtime/proc.go:7243 pushes internal_sync_runtime_canSpin. Go answers FALSE whenever spinning
    // makes no sense (`i >= active_spin || ncpu <= 1 || gomaxprocs <= npidle+nmspinning+1`, or the
    // local run queue is non-empty), and spinning only makes sense when this goroutine owns a P while
    // another P is running the lock holder. The managed runtime has no Ps: every goroutine is an OS
    // thread the CLR schedules. So false is Go's own answer for this runtime, not a stub. lockSlow
    // then does what it already does whenever spinning is refused: queue on the semaphore.
    // Spinning never affects correctness. It is only a latency optimisation in Go.
    internal static partial bool runtime_canSpin(nint i) => false;

    // runtime/proc.go:7260 pushes internal_sync_runtime_doSpin, whose body is procyield(active_spin_cnt)
    // with active_spin_cnt = 30 (lock_spinbit.go:54): thirty PAUSE instructions without yielding the
    // thread. Thread.SpinWait(30) is the CLR's version of the same thing: a bounded busy wait on the
    // processor's pause hint, with no OS yield. It is UNREACHED while canSpin answers false, because
    // lockSlow calls doSpin only after canSpin says true. It still gets a faithful body: an unreached
    // throwing stub is the member that reaches a user as a dead host with no warning.
    internal static partial void runtime_doSpin() => Thread.SpinWait(30);

    // runtime/sema.go:708 pushes internal_sync_nanotime, whose body is `return nanotime()`. The
    // runtime's own nanotime1 is MonotonicClock.Nanoseconds() on every platform this corpus builds
    // (runtime/linux/nanotime_impl.cs and runtime/windows/nanotime_impl.cs), so this reads the same
    // clock with the same epoch. That matters: lockSlow compares two readings against
    // starvationThresholdNs (1 ms), and a clock that overflowed or jumped would switch the mutex
    // into or out of starvation mode for the wrong reason.
    internal static partial int64 runtime_nanotime() => MonotonicClock.Nanoseconds();

    // runtime/sema.go:93 pushes internal_sync_runtime_SemacquireMutex, whose body is
    // semacquire1(addr, lifo, semaBlockProfile|semaMutexProfile, skipframes, waitReasonSyncMutexLock).
    // RuntimeSemaphore is the managed semacquire1, and its own remarks name THIS package as its
    // second caller. The park reason is Go's (a traceback reads [sync.Mutex.Lock]). Three arguments
    // do not reach it, and each is named here so that nobody has to rediscover it:
    //   lifo        Go queues a waiter that has ALREADY waited at the head of the queue, which bounds
    //               a re-waiter's delay. RuntimeSemaphore's queue is FIFO only. That is a fairness
    //               difference, not a correctness one: starvation mode, which is what actually bounds
    //               waiting in Go (the 1 ms threshold, then direct handoff), still works, because
    //               Semrelease's handoff below does reach the primitive. sync's RWMutex passes lifo
    //               to the same primitive under the same limitation.
    //   skipframes  traceback trimming for profiles only.
    //   profile     semaBlockProfile|semaMutexProfile: mutex and block profile SAMPLES. That is the
    //               structural class already ruled for runtime's sample-1 and runtime/pprof's
    //               TestMutexBlockFullAggregation (no mutex-profile stack samples in the managed model).
    internal static partial void runtime_SemacquireMutex(ж<uint32> s, bool lifo, nint skipframes) =>
        RuntimeSemaphore.Acquire(s, WaitReason.SyncMutexLock);

    // runtime/sema.go:118 pushes internal_sync_runtime_Semrelease, whose body is
    // semrelease1(addr, handoff, skipframes). handoff is Go's starvation-mode direct transfer and it
    // DOES reach the primitive: unlockSlow's starving branch (mutex.cs:218) passes true, and
    // RuntimeSemaphore.Release gives the permit to the first waiter rather than re-offering it.
    internal static partial void runtime_Semrelease(ж<uint32> s, bool handoff, nint skipframes) =>
        RuntimeSemaphore.Release(s, handoff);
}
