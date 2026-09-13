// fd_mutex_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-owned companions for internal/poll's fdMutex read/write lock — `rwlock` and `rwunlock`,
// displaced from the converted fd_mutex.cs through manualConversionFuncs. Nothing else in
// fd_mutex.go is hand-owned: incref, increfAndClose, decref and the six FD-level wrappers convert
// faithfully and stay auto.
//
// WHY these two, and only these two. Go's fdMutex carries two semaphore WORDS (`rsema`, `wsema`)
// and hands their ADDRESSES to the runtime's semaphore primitives. The converted port keeps the
// waiter count in a side table keyed by the address box, so every `&mu.rsema` had to mint a
// ж<uint32> — and, because a box is the only thing that can carry that identity, the two methods
// forming those addresses could never take a `ref fdMutex` receiver. That made the whole chain
// above them unpromotable: FD.readLock/writeLock, FD.Write, and os.File's own pfd access.
//
// The identity was a property of OUR representation, not of Go's semantics. Measured: Go's
// internal/poll forms `&mu.rsema` / `&mu.wsema` for the primitives and NEVER reads or writes either
// word as a value (the only occurrences in Go 1.23.12 are the two declarations and four
// address-takes), and this port's own count lives entirely in the side table — so the words are
// dead storage. The gate can therefore live where it belongs: INLINE in the struct, exactly as the
// hand-owned sync/mutex.cs already does for sync.Mutex. `rgate`/`wgate` below are that gate,
// CAS-installed on first use.
//
// What that buys: no side table for this type (the table in runtime_sema_impl.cs keys with
// GetOrAdd and has no removal path, so it accumulates one bucket per distinct semaphore for the
// life of the process — an fdMutex no longer contributes to it), the semaphore words left untouched
// as Go declares them, and — the point of the exercise — `rwlock`/`rwunlock` become genuine
// `ref fdMutex` primaries, so the boxes their callers mint can be removed by the call-site rule
// rather than by more hand-owns.
//
// DELIBERATELY NOT COVERED. FD.csema is the same FAMILY (a semaphore word in a struct) and a
// different ACCESS SHAPE: it is passed as a free-function ARGUMENT from converted Close/destroy
// bodies, which never hold the gate's containing struct, so an inline gate cannot be reached from
// there without hand-owning two functions of real logic per platform. csema keeps the side table
// and is unchanged by this cut.
//
// A COPY of an fdMutex shares the original's gates, where Go's copy would get fresh (zero)
// semaphores. That divergence is real and unreachable: like sync's types, an fdMutex must not be
// copied after first use — it is an unexported field of FD, which is itself always used through a
// pointer. The same reasoning already covers sync.Mutex's inline gate.

using System.Threading;
// Aliased rather than imported wholesale, matching runtime_sema_impl.cs beside it: this file needs
// exactly two golib types, and a blanket `using go.golib` would also pull that namespace's
// extension methods into a hand-owned file sitting next to converted code.
using Goroutine = go.golib.Goroutine;
using WaitReason = go.golib.WaitReason;

// Hand-owned companion (no fd_mutex_impl.go exists, so a reconvert never regenerates it); marked
// for consistency with the other hand-owned operational files in this package.
[module: go.GoManualConversion]

namespace go.@internal;

partial class poll_package {

// The inline gates backing fdMutex's two semaphore words. Lazily created, so a zero fdMutex is a
// valid unlocked fdMutex exactly as in Go. Declared here rather than in the converted fd_mutex.cs
// so a reconvert leaves them alone; fdMutex is a partial struct, so this needs no whole-file
// hand-own of the type's own file.
partial struct fdMutex {
    internal SemaphoreSlim? rgate;
    internal SemaphoreSlim? wgate;
}

// gateOf returns the read or write gate, creating it once on first use (race-safe: the loser of
// the install disposes its own candidate and takes the winner's, as sync/mutex.cs's gateOf does).
// A counting semaphore starting at zero — internal/poll's contract has no handoff or starvation
// mode, so a release simply increments and a waiter re-competes, which is what the side-table
// implementation it replaces did with a count plus a FIFO queue.
private static SemaphoreSlim gateOf(ref fdMutex mu, bool read) {
    ref SemaphoreSlim? slot = ref read ? ref mu.rgate : ref mu.wgate;

    SemaphoreSlim? g = Volatile.Read(ref slot);

    if (g is not null) {
        return g;
    }

    var created = new SemaphoreSlim(0);
    g = Interlocked.CompareExchange(ref slot, created, null);

    if (g is not null) {
        created.Dispose(); // lost the race; another thread installed the gate
        return g;
    }

    return created;
}

// increfAndClose sets the state of mu to closed.
// It returns false if the file was already closed.
//
// Hand-owned for one reason, and it is the reason the hand-own set is THREE and not two: this is
// the third member of the semaphore protocol. Go's own TestMutexCloseUnblock pins it —
// `mu.IncrefAndClose() // Must unblock the readers` — so the waiters this wakes are the ones
// rwlock parks. A gate-parked waiter cannot be woken by a table release, so leaving this function
// auto-converted split the protocol across two mechanisms and lost every close-time wakeup: the
// readers slept until the test's own 10-second deadline and Go's "broken" came back against a
// passing oracle. Same transcription as the pair below — the state word through ref, the wakeups
// through the inline gate.
[GoRecv] internal static bool increfAndClose(this ref fdMutex mu) {
    while (true) {
        uint64 old = Volatile.Read(ref mu.state);

        if ((uint64)(old & (uint64)mutexClosed) != 0) {
            return false;
        }

        // Mark as closed and acquire a reference.
        uint64 @new = ((uint64)(old | (uint64)mutexClosed)) + (uint64)mutexRef;

        if ((uint64)(@new & (uint64)mutexRefMask) == 0) {
            throw panic(overflowMsg);
        }

        // Remove all read and write waiters.
        @new &= unchecked((uint64)~(uint64)((uint64)((uint64)mutexRMask | (uint64)mutexWMask)));

        if (Interlocked.CompareExchange(ref mu.state, @new, old) == old) {
            // Wake all read and write waiters,
            // they will observe closed flag after wakeup.
            while ((uint64)(old & (uint64)mutexRMask) != 0) {
                old -= mutexRWait;
                gateOf(ref mu, true).Release();
            }

            while ((uint64)(old & (uint64)mutexWMask) != 0) {
                old -= mutexWWait;
                gateOf(ref mu, false).Release();
            }

            return true;
        }
    }
}

// rwlock adds a reference to mu and locks mu.
// It reports whether mu is available for reading or writing.
//
// Transcribed from fd_mutex.go's (*fdMutex).rwlock. The state machine is unchanged, statement for
// statement; only two things differ from the converted body, and both follow from the ref receiver.
// The state word is read and CAS'd through `ref mu.state` with Volatile/Interlocked — the same
// operations sync/atomic's Uint64 helpers perform, minus the address box a ref receiver cannot
// form — and the parked wait takes the inline gate instead of a table lookup on `&mu.rsema`.
[GoRecv] internal static bool rwlock(this ref fdMutex mu, bool read) {
    uint64 mutexBit = default!;
    uint64 mutexWait = default!;
    uint64 mutexMask = default!;

    if (read){
        mutexBit = mutexRLock;
        mutexWait = mutexRWait;
        mutexMask = mutexRMask;
    } else {
        mutexBit = mutexWLock;
        mutexWait = mutexWWait;
        mutexMask = mutexWMask;
    }

    while (true) {
        uint64 old = Volatile.Read(ref mu.state);

        if ((uint64)(old & (uint64)mutexClosed) != 0) {
            return false;
        }

        uint64 @new = default!;

        if ((uint64)(old & mutexBit) == 0){
            // Lock is free, acquire it.
            @new = ((uint64)(old | mutexBit)) + (uint64)mutexRef;

            if ((uint64)(@new & (uint64)mutexRefMask) == 0) {
                throw panic(overflowMsg);
            }
        } else {
            // Wait for lock.
            @new = old + mutexWait;

            if ((uint64)(@new & mutexMask) == 0) {
                throw panic(overflowMsg);
            }
        }

        if (Interlocked.CompareExchange(ref mu.state, @new, old) == old) {
            if ((uint64)(old & mutexBit) == 0) {
                return true;
            }

            // The signaller has subtracted mutexWait.
            using (Goroutine.Park(WaitReason.Semacquire)) {
                gateOf(ref mu, read).Wait();
            }
        }
    }
}

// rwunlock removes a reference from mu and unlocks mu.
// It reports whether there is no remaining reference.
//
// Transcribed from fd_mutex.go's (*fdMutex).rwunlock, with the same two ref-receiver consequences
// as rwlock above.
[GoRecv] internal static bool rwunlock(this ref fdMutex mu, bool read) {
    uint64 mutexBit = default!;
    uint64 mutexWait = default!;
    uint64 mutexMask = default!;

    if (read){
        mutexBit = mutexRLock;
        mutexWait = mutexRWait;
        mutexMask = mutexRMask;
    } else {
        mutexBit = mutexWLock;
        mutexWait = mutexWWait;
        mutexMask = mutexWMask;
    }

    while (true) {
        uint64 old = Volatile.Read(ref mu.state);

        if ((uint64)(old & mutexBit) == 0 || (uint64)(old & (uint64)mutexRefMask) == 0) {
            // Spelled as the displaced body spelled it: panic takes `object`, so a u8 literal
            // (which is a ReadOnlySpan<byte>) does not convert.
            throw panic("inconsistent poll.fdMutex");
        }

        // Drop lock, drop reference and wake read waiter if present.
        uint64 @new = ((uint64)(old & ~mutexBit)) - (uint64)mutexRef;

        if ((uint64)(old & mutexMask) != 0) {
            @new -= mutexWait;
        }

        if (Interlocked.CompareExchange(ref mu.state, @new, old) == old) {
            if ((uint64)(old & mutexMask) != 0) {
                gateOf(ref mu, read).Release();
            }

            return (uint64)(@new & ((uint64)((uint64)mutexClosed | (uint64)mutexRefMask))) == mutexClosed;
        }
    }
}

}
