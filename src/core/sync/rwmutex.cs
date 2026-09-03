// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted rwmutex.go output). Go's RWMutex
// coordinates readers and a writer through two runtime sleeping semaphores plus an embedded Mutex —
// the same runtime layer that cannot be emulated faithfully (see mutex.cs / runtime_impl.cs).
// Reimplemented natively as a writer-preferring readers-writer lock over a single monitor: a pending
// writer blocks new readers (Go's documented guarantee that the lock eventually reaches the writer),
// and it is not goroutine-affine (any goroutine may release what another acquired). The RLocker/rlocker
// interface witness is unchanged — it simply forwards to RLock/RUnlock.
using System.Threading;
// Aliased rather than imported wholesale: this file needs exactly two golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using Goroutine = go.golib.Goroutine;
using WaitReason = go.golib.WaitReason;

// Hand-owned native replacement of the converted rwmutex.go output — the marker makes a -stdlib
// reconvert skip regenerating this file (see containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go;

partial class sync_package {

// A RWMutex is a reader/writer mutual exclusion lock.
// The lock can be held by an arbitrary number of readers or a single writer.
// The zero value for a RWMutex is an unlocked mutex. A RWMutex must not be copied after first use.
[GoType] partial struct RWMutex {
    // Lazily-created shared state; a RWMutex is always used through a pointer (ж<RWMutex>).
    internal RWState? st;
}

// RWState is the native backing: reader/writer bookkeeping guarded by one monitor (Gate). Writers are
// preferred — a waiting writer stops new readers — matching Go's eventual-writer-progress guarantee.
internal sealed class RWState {
    internal readonly object Gate = new();
    internal int Readers;        // active readers
    internal int ReadersWaiting; // readers currently blocked on a pending/active writer
    internal int WritersWaiting; // writers currently blocked
    internal bool Writer;        // a writer holds the lock
}

private static RWState rwStateOf(ж<RWMutex> Ꮡrw) {
    ref var rw = ref Ꮡrw.Value;

    RWState? s = Volatile.Read(ref rw.st);

    if (s is not null) {
        return s;
    }

    var created = new RWState();
    return Interlocked.CompareExchange(ref rw.st, created, null) ?? created;
}

// RLock locks rw for reading. It should not be used for recursive read locking; a blocked Lock call
// excludes new readers from acquiring the lock.
public static void RLock(this ж<RWMutex> Ꮡrw) {
    RWState s = rwStateOf(Ꮡrw);

    lock (s.Gate) {
        while (s.Writer || s.WritersWaiting > 0) {
            s.ReadersWaiting++;

            // Park accounting only — Monitor.Wait still owns the release-and-reacquire (Go's
            // sync_runtime_SemacquireRWMutexR carries the same reason to the same effect).
            using (Goroutine.Park(WaitReason.SyncRWMutexRLock)) {
                Monitor.Wait(s.Gate);
            }

            s.ReadersWaiting--;
        }

        s.Readers++;
    }
}

// TryRLock tries to lock rw for reading and reports whether it succeeded.
public static bool TryRLock(this ж<RWMutex> Ꮡrw) {
    RWState s = rwStateOf(Ꮡrw);

    lock (s.Gate) {
        if (s.Writer || s.WritersWaiting > 0) {
            return false;
        }

        s.Readers++;
        return true;
    }
}

// RUnlock undoes a single RLock call; it does not affect other simultaneous readers.
public static void RUnlock(this ж<RWMutex> Ꮡrw) {
    RWState s = rwStateOf(Ꮡrw);

    lock (s.Gate) {
        if (s.Readers == 0) {
            fatal("sync: RUnlock of unlocked RWMutex"u8);
        }

        s.Readers--;

        if (s.Readers == 0) {
            Monitor.PulseAll(s.Gate); // let a waiting writer proceed
        }
    }
}

// Lock locks rw for writing. If the lock is already locked for reading or writing, Lock blocks until
// the lock is available.
public static void Lock(this ж<RWMutex> Ꮡrw) {
    RWState s = rwStateOf(Ꮡrw);

    lock (s.Gate) {
        s.WritersWaiting++;

        while (s.Writer || s.Readers > 0) {
            using (Goroutine.Park(WaitReason.SyncRWMutexLock)) {
                Monitor.Wait(s.Gate);
            }
        }

        s.WritersWaiting--;
        s.Writer = true;
    }
}

// TryLock tries to lock rw for writing and reports whether it succeeded.
public static bool TryLock(this ж<RWMutex> Ꮡrw) {
    RWState s = rwStateOf(Ꮡrw);

    lock (s.Gate) {
        if (s.Writer || s.Readers > 0 || s.WritersWaiting > 0) {
            return false;
        }

        s.Writer = true;
        return true;
    }
}

// Unlock unlocks rw for writing. It is a run-time error if rw is not locked for writing on entry.
public static void Unlock(this ж<RWMutex> Ꮡrw) {
    RWState s = rwStateOf(Ꮡrw);

    lock (s.Gate) {
        if (!s.Writer) {
            fatal("sync: Unlock of unlocked RWMutex"u8);
        }

        s.Writer = false;
        Monitor.PulseAll(s.Gate); // release blocked readers and/or the next writer
    }
}

// syscall_hasWaitingReaders reports whether any goroutine is waiting to acquire a read lock on rw.
// (syscall.ForkLock is an RWMutex and relies on this private linkname.)
//
//go:linkname syscall_hasWaitingReaders syscall.hasWaitingReaders
// PUBLIC, not internal: this is a linkname PUSH source (linknamePushSources — the registry row
// syscall.hasWaitingReaders added 2026-08-22), and syscall's forkpipe2 forwarder calls it across
// the assembly boundary. The converter emits push sources public automatically; this file is
// hand-owned, so the access travels by hand — the regenerated form lives in rwmutex.cs.auto.
public static bool syscall_hasWaitingReaders(ж<RWMutex> Ꮡrw) {
    RWState s = rwStateOf(Ꮡrw);

    lock (s.Gate) {
        return s.ReadersWaiting > 0;
    }
}

// RLocker returns a Locker interface that implements Lock and Unlock by calling rw.RLock and rw.RUnlock.
//
// Go's RLocker is a pointer REINTERPRETATION — `(*rlocker)(rw)` — so the returned Locker shares rw's
// storage. The C# form copy-boxes the RWMutex value into an rlocker (identical layout), which shares
// rw's state only because `st` is a REFERENCE to the shared RWState. That reference has to exist
// first: on a never-used RWMutex it is still null, the copy's own rwStateOf then installs a SECOND
// RWState on the copy, and every RLock/RUnlock through the Locker operates on state the original
// never sees — TestRLocker (whose `var wl RWMutex; rl = wl.RLocker()` calls it on a fresh mutex)
// died with "sync: RUnlock of unlocked RWMutex". Forcing creation before the copy makes both sides
// hold the one RWState, which is exactly what Go's shared storage gives.
public static Locker RLocker(this ж<RWMutex> Ꮡrw) {
    rwStateOf(Ꮡrw);

    ref var rw = ref Ꮡrw.Value;

    return new rlockerжLocker(Ꮡ((rlocker)(rw)));
}

[GoType("RWMutex")] partial struct rlocker;

internal static void Lock(this ж<rlocker> Ꮡr) {
    ref var r = ref Ꮡr.Value;

    (Ꮡ((RWMutex)(r))).RLock();
}

internal static void Unlock(this ж<rlocker> Ꮡr) {
    ref var r = ref Ꮡr.Value;

    (Ꮡ((RWMutex)(r))).RUnlock();
}

} // end sync_package
