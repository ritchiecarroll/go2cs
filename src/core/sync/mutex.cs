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
// SemaphoreSlim, which the CLR implements correctly — FIFO wakeups, release permitted from any thread
// (Go allows unlock on a different goroutine), and non-reentrant (a second Lock on the same thread
// blocks, matching Go's self-deadlock). See runtime_impl.cs for the shared rationale.
using System.Threading;
// Aliased rather than imported wholesale: this file needs exactly two golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using Goroutine = go.golib.Goroutine;
using WaitReason = go.golib.WaitReason;

// Hand-owned native replacement of the converted mutex.go output — the converter skips regenerating a
// file that carries this marker, so a -stdlib reconvert preserves it (see containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go;

partial class sync_package {

// Fatal-error hooks (Go provides these via runtime linkname). Surfaced as a non-panic exception so
// recover() cannot swallow them and the program terminates loudly, as Go's runtime.throw/fatal do.
internal static void @throw(@string s) => throw new global::System.InvalidOperationException($"fatal error: {s}");

internal static void fatal(@string s) => throw new global::System.InvalidOperationException($"fatal error: {s}");

// A Mutex is a mutual exclusion lock.
// The zero value for a Mutex is an unlocked mutex.
//
// A Mutex must not be copied after first use.
[GoType] partial struct Mutex {
    // Lazily-created binary semaphore backing this mutex. A Mutex is always used through a pointer
    // (ж<Mutex>), never copied after first use, so the box holds the single shared gate.
    internal SemaphoreSlim? gate;
}

// A Locker represents an object that can be locked and unlocked.
[GoType] public partial interface Locker {
    void Lock();
    void Unlock();
}

// gateOf returns the mutex's backing semaphore, creating it once on first use (race-safe).
private static SemaphoreSlim gateOf(ж<Mutex> Ꮡm) {
    ref var m = ref Ꮡm.Value;

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
// traceback and touches neither the gate nor the order in which waiters reach it. It wraps the whole
// Wait rather than only a contended one BECAUSE the fast path would have to be a Wait(0), and that
// is a protocol change — a newcomer barging ahead of the queue — where this cut is required to
// change no protocol at all. The cost is therefore two volatile stores on every Lock, contended or
// not, measured by the cost canary named in this arc's commit.
public static void Lock(this ж<Mutex> Ꮡm) {
    using (Goroutine.Park(WaitReason.SyncMutexLock)) {
        gateOf(Ꮡm).Wait();
    }
}

// TryLock tries to lock m and reports whether it succeeded.
public static bool TryLock(this ж<Mutex> Ꮡm) => gateOf(Ꮡm).Wait(0);

// Unlock unlocks m.
// It is a run-time error if m is not locked on entry to Unlock.
// A locked Mutex is not associated with a particular goroutine; one goroutine may lock a Mutex and
// then arrange for another goroutine to unlock it.
public static void Unlock(this ж<Mutex> Ꮡm) {
    try {
        gateOf(Ꮡm).Release();
    } catch (SemaphoreFullException) {
        fatal("sync: unlock of unlocked mutex"u8);
    }
}

} // end sync_package
