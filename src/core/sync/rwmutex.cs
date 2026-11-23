// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using race = @internal.race_package;
using atomic = go.sync.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using go.sync;

partial class sync_package {

// There is a modified copy of this file in runtime/rwmutex.go.
// If you make any changes here, see if you should make them there.

// A RWMutex is a reader/writer mutual exclusion lock.
// The lock can be held by an arbitrary number of readers or a single writer.
// The zero value for a RWMutex is an unlocked mutex.
//
// A RWMutex must not be copied after first use.
//
// If any goroutine calls [RWMutex.Lock] while the lock is already held by
// one or more readers, concurrent calls to [RWMutex.RLock] will block until
// the writer has acquired (and released) the lock, to ensure that
// the lock eventually becomes available to the writer.
// Note that this prohibits recursive read-locking.
// A [RWMutex.RLock] cannot be upgraded into a [RWMutex.Lock],
// nor can a [RWMutex.Lock] be downgraded into a [RWMutex.RLock].
//
// In the terminology of [the Go memory model],
// the n'th call to [RWMutex.Unlock] “synchronizes before” the m'th call to Lock
// for any n < m, just as for [Mutex].
// For any call to RLock, there exists an n such that
// the n'th call to Unlock “synchronizes before” that call to RLock,
// and the corresponding call to [RWMutex.RUnlock] “synchronizes before”
// the n+1'th call to Lock.
//
// [the Go memory model]: https://go.dev/ref/mem
partial struct RWMutex {
    internal Mutex w;                  // held if there are pending writers
    internal uint32 writerSem;         // semaphore for writers to wait for completing readers
    internal uint32 readerSem;         // semaphore for readers to wait for completing writers
    internal atomic.Int32 readerCount; // number of pending readers
    internal atomic.Int32 readerWait;  // number of departing readers
}

internal static UntypedInt rwmutexMaxReaders => /* 1 << 30 */ 1073741824;

// Happens-before relationships are indicated to the race detector via:
// - Unlock  -> Lock:  readerSem
// - Unlock  -> RLock: readerSem
// - RUnlock -> Lock:  writerSem
//
// The methods below temporarily disable handling of race synchronization
// events in order to provide the more precise model above to the race
// detector.
//
// For example, atomic.AddInt32 in RLock should not appear to provide
// acquire-release semantics, which would incorrectly synchronize racing
// readers, thus potentially missing races.

// RLock locks rw for reading.
//
// It should not be used for recursive read locking; a blocked Lock
// call excludes new readers from acquiring the lock. See the
// documentation on the [RWMutex] type.
internal static void RLock⁔(/*this*/ ж<RWMutex> Ꮡrw) {
    if (race.Enabled) {
        race.Read(@unsafe.Pointer.FromPinnedBox(Ꮡrw.Ꮡw));
        race.Disable();
    }
    if (Ꮡrw.ᏑreaderCount.Add(1) < 0) {
        // A writer is pending, wait for it.
        runtime_SemacquireRWMutexR(Ꮡrw.ᏑreaderSem, false, 0);
    }
    if (race.Enabled) {
        race.Enable();
        race.Acquire(@unsafe.Pointer.FromPinnedBox(Ꮡrw.ᏑreaderSem));
    }
}

// TryRLock tries to lock rw for reading and reports whether it succeeded.
//
// Note that while correct uses of TryRLock do exist, they are rare,
// and use of TryRLock is often a sign of a deeper problem
// in a particular use of mutexes.
internal static bool TryRLock⁔(/*this*/ ж<RWMutex> Ꮡrw) {
    if (race.Enabled) {
        race.Read(@unsafe.Pointer.FromPinnedBox(Ꮡrw.Ꮡw));
        race.Disable();
    }
    for (;;) {
        var c = Ꮡrw.ᏑreaderCount.Load();
        if (c < 0) {
            if (race.Enabled) {
                race.Enable();
            }
            return false;
        }
        if (Ꮡrw.ᏑreaderCount.CompareAndSwap(c, c + 1)) {
            if (race.Enabled) {
                race.Enable();
                race.Acquire(@unsafe.Pointer.FromPinnedBox(Ꮡrw.ᏑreaderSem));
            }
            return true;
        }
    }
}

// RUnlock undoes a single [RWMutex.RLock] call;
// it does not affect other simultaneous readers.
// It is a run-time error if rw is not locked for reading
// on entry to RUnlock.
internal static void RUnlock⁔(/*this*/ ж<RWMutex> Ꮡrw) {
    if (race.Enabled) {
        race.Read(@unsafe.Pointer.FromPinnedBox(Ꮡrw.Ꮡw));
        race.ReleaseMerge(@unsafe.Pointer.FromPinnedBox(Ꮡrw.ᏑwriterSem));
        race.Disable();
    }
    var r = Ꮡrw.ᏑreaderCount.Add(-1); if (r < 0) {
        // Outlined slow-path to allow the fast-path to be inlined
        Ꮡrw.rUnlockSlow(r);
    }
    if (race.Enabled) {
        race.Enable();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syncRUnlockOfUnlockedˢ => ref syncRUnlockOfUnlockedˢᵇ;
private static readonly @string syncRUnlockOfUnlockedˢᵇ = "sync: RUnlock of unlocked RWMutex"u8;

internal static void rUnlockSlow⁔(/*this*/ ж<RWMutex> Ꮡrw, int32 r) {
    if (r + 1 == 0 || r + 1 == (int32)(-rwmutexMaxReaders)) {
        race.Enable();
        fatal(syncRUnlockOfUnlockedˢ);
    }
    // A writer is pending.
    if (Ꮡrw.ᏑreaderWait.Add(-1) == 0) {
        // The last reader unblocks the writer.
        runtime_Semrelease(Ꮡrw.ᏑwriterSem, false, 1);
    }
}

// Lock locks rw for writing.
// If the lock is already locked for reading or writing,
// Lock blocks until the lock is available.
internal static void Lock⁔(/*this*/ ж<RWMutex> Ꮡrw) {
    ref var rw = ref Ꮡrw.DerefOrNull();

    if (race.Enabled) {
        race.Read(@unsafe.Pointer.FromPinnedBox(Ꮡrw.Ꮡw));
        race.Disable();
    }
    // First, resolve competition with other writers.
    rw.w.Lock();
    // Announce to readers there is a pending writer.
    var r = Ꮡrw.ᏑreaderCount.Add(-rwmutexMaxReaders) + (int32)rwmutexMaxReaders;
    // Wait for active readers.
    if (r != 0 && Ꮡrw.ᏑreaderWait.Add(r) != 0) {
        runtime_SemacquireRWMutex(Ꮡrw.ᏑwriterSem, false, 0);
    }
    if (race.Enabled) {
        race.Enable();
        race.Acquire(@unsafe.Pointer.FromPinnedBox(Ꮡrw.ᏑreaderSem));
        race.Acquire(@unsafe.Pointer.FromPinnedBox(Ꮡrw.ᏑwriterSem));
    }
}

// TryLock tries to lock rw for writing and reports whether it succeeded.
//
// Note that while correct uses of TryLock do exist, they are rare,
// and use of TryLock is often a sign of a deeper problem
// in a particular use of mutexes.
internal static bool TryLock⁔(/*this*/ ж<RWMutex> Ꮡrw) {
    ref var rw = ref Ꮡrw.DerefOrNull();

    if (race.Enabled) {
        race.Read(@unsafe.Pointer.FromPinnedBox(Ꮡrw.Ꮡw));
        race.Disable();
    }
    if (!rw.w.TryLock()) {
        if (race.Enabled) {
            race.Enable();
        }
        return false;
    }
    if (!Ꮡrw.ᏑreaderCount.CompareAndSwap(0, -rwmutexMaxReaders)) {
        rw.w.Unlock();
        if (race.Enabled) {
            race.Enable();
        }
        return false;
    }
    if (race.Enabled) {
        race.Enable();
        race.Acquire(@unsafe.Pointer.FromPinnedBox(Ꮡrw.ᏑreaderSem));
        race.Acquire(@unsafe.Pointer.FromPinnedBox(Ꮡrw.ᏑwriterSem));
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syncUnlockOfUnlockedˢ => ref syncUnlockOfUnlockedˢᵇ;
private static readonly @string syncUnlockOfUnlockedˢᵇ = "sync: Unlock of unlocked RWMutex"u8;

// Unlock unlocks rw for writing. It is a run-time error if rw is
// not locked for writing on entry to Unlock.
//
// As with Mutexes, a locked [RWMutex] is not associated with a particular
// goroutine. One goroutine may [RWMutex.RLock] ([RWMutex.Lock]) a RWMutex and then
// arrange for another goroutine to [RWMutex.RUnlock] ([RWMutex.Unlock]) it.
internal static void Unlock⁔(/*this*/ ж<RWMutex> Ꮡrw) {
    ref var rw = ref Ꮡrw.DerefOrNull();

    if (race.Enabled) {
        race.Read(@unsafe.Pointer.FromPinnedBox(Ꮡrw.Ꮡw));
        race.Release(@unsafe.Pointer.FromPinnedBox(Ꮡrw.ᏑreaderSem));
        race.Disable();
    }

    // Announce to readers there is no active writer.
    var r = Ꮡrw.ᏑreaderCount.Add(rwmutexMaxReaders);
    if (r >= rwmutexMaxReaders) {
        race.Enable();
        fatal(syncUnlockOfUnlockedˢ);
    }
    // Unblock blocked readers, if any.
    for (nint i = 0; i < (nint)r; i++) {
        runtime_Semrelease(Ꮡrw.ᏑreaderSem, false, 0);
    }
    // Allow other writers to proceed.
    rw.w.Unlock();
    if (race.Enabled) {
        race.Enable();
    }
}

// syscall_hasWaitingReaders reports whether any goroutine is waiting
// to acquire a read lock on rw. This exists because syscall.ForkLock
// is an RWMutex, and we can't change that without breaking compatibility.
// We don't need or want RWMutex semantics for ForkLock, and we use
// this private API to avoid having to change the type of ForkLock.
// For more details see the syscall package.
//
//go:linkname syscall_hasWaitingReaders syscall.hasWaitingReaders
public static bool syscall_hasWaitingReaders(ж<RWMutex> rw) {
    var r = rw.ᏑreaderCount.Load();
    return r < 0 && r + (int32)rwmutexMaxReaders > 0;
}

// RLocker returns a [Locker] interface that implements
// the [Locker.Lock] and [Locker.Unlock] methods by calling rw.RLock and rw.RUnlock.
internal static Locker RLocker⁔(/*this*/ ж<RWMutex> Ꮡrw) {
    return new rlockerжLocker(Ꮡrw.Reinterpret<RWMutex, rlocker>());
}

partial struct rlocker /*RWMutex*/;

internal static void Lock⁔(/*this*/ ж<rlocker> Ꮡr) { (Ꮡr.Reinterpret<rlocker, RWMutex>()).RLock(); }
internal static void Unlock⁔(/*this*/ ж<rlocker> Ꮡr) { (Ꮡr.Reinterpret<rlocker, RWMutex>()).RUnlock(); }

} // end sync_package
