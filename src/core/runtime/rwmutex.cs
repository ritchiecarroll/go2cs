// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using @internal.runtime;

partial class runtime_package {

// This is a copy of sync/rwmutex.go rewritten to work in the runtime.

// A rwmutex is a reader/writer mutual exclusion lock.
// The lock can be held by an arbitrary number of readers or a single writer.
// This is a variant of sync.RWMutex, for the runtime package.
// Like mutex, rwmutex blocks the calling M.
// It does not interact with the goroutine scheduler.
[GoType] partial struct rwmutex {
    internal mutex rLock;    // protects readers, readerPass, writer
    internal muintptr readers; // list of pending readers
    internal uint32 readerPass;   // number of pending readers to skip readers list
    internal mutex wLock;    // serializes writers
    internal muintptr writer; // pending writer waiting for completing readers
    internal atomic.Int32 readerCount; // number of pending readers
    internal atomic.Int32 readerWait; // number of departing readers
    internal lockRank readRank; // semantic lock rank for read locking
}

// Lock ranking an rwmutex has two aspects:
//
// Semantic ranking: this rwmutex represents some higher level lock that
// protects some resource (e.g., allocmLock protects creation of new Ms). The
// read and write locks of that resource need to be represented in the lock
// rank.
//
// Internal ranking: as an implementation detail, rwmutex uses two mutexes:
// rLock and wLock. These have lock order requirements: wLock must be locked
// before rLock. This also needs to be represented in the lock rank.
//
// Semantic ranking is represented by acquiring readRank during read lock and
// writeRank during write lock.
//
// wLock is held for the duration of a write lock, so it uses writeRank
// directly, both for semantic and internal ranking. rLock is only held
// temporarily inside the rlock/lock methods, so it uses readRankInternal to
// represent internal ranking. Semantic ranking is represented by a separate
// acquire of readRank for the duration of a read lock.
//
// The lock ranking must document this ordering:
//   - readRankInternal is a leaf lock.
//   - readRank is taken before readRankInternal.
//   - writeRank is taken before readRankInternal.
//   - readRank is placed in the lock order wherever a read lock of this rwmutex
//     belongs.
//   - writeRank is placed in the lock order wherever a write lock of this
//     rwmutex belongs.
internal static void init(this ж<rwmutex> Ꮡrw, lockRank readRank, lockRank readRankInternal, lockRank writeRank) {
    ref var rw = ref Ꮡrw.DerefOrNull();

    rw.readRank = readRank;
    lockInit(ref nonnil(ref rw).rLock, readRankInternal);
    lockInit(ref nonnil(ref rw).wLock, writeRank);
}

internal static UntypedInt rwmutexMaxReaders => /* 1 << 30 */ 1073741824;

// rlock locks rw for reading.
internal static void rlock(this ж<rwmutex> Ꮡrw) {
    ref var rw = ref Ꮡrw.DerefOrNull();

    // The reader must not be allowed to lose its P or else other
    // things blocking on the lock may consume all of the Ps and
    // deadlock (issue #20903). Alternatively, we could drop the P
    // while sleeping.
    acquireLockRankAndM(rw.readRank);
    lockWithRankMayAcquire(Ꮡrw.of(rwmutex.ᏑrLock), getLockRank(Ꮡrw.of(rwmutex.ᏑrLock)));
    if (Ꮡrw.of(rwmutex.ᏑreaderCount).Add(1) < 0) {
        // A writer is pending. Park on the reader queue.
        systemstack(() => {
            @lock(Ꮡrw.of(rwmutex.ᏑrLock));
            if (Ꮡrw.Value.readerPass > 0){
                // Writer finished.
                Ꮡrw.Value.readerPass -= 1;
                unlock(Ꮡrw.of(rwmutex.ᏑrLock));
            } else {
                // Queue this reader to be woken by
                // the writer.
                var m = getg().Value.m;
                m.Value.schedlink = Ꮡrw.Value.readers;
                Ꮡrw.Value.readers.set(m);
                unlock(Ꮡrw.of(rwmutex.ᏑrLock));
                notesleep(m.of(runtime_package.m.Ꮡpark));
                noteclear(m.of(runtime_package.m.Ꮡpark));
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runlockOfUnlockedRwmutexˢ = "runlock of unlocked rwmutex"u8;

// runlock undoes a single rlock call on rw.
internal static void runlock(this ж<rwmutex> Ꮡrw) {
    ref var rw = ref Ꮡrw.DerefOrNull();

    {
        var r = Ꮡrw.of(rwmutex.ᏑreaderCount).Add(-1); if (r < 0) {
            if (r + 1 == 0 || r + 1 == (int32)(-rwmutexMaxReaders)) {
                @throw(runlockOfUnlockedRwmutexˢ);
            }
            // A writer is pending.
            if (Ꮡrw.of(rwmutex.ᏑreaderWait).Add(-1) == 0) {
                // The last reader unblocks the writer.
                @lock(Ꮡrw.of(rwmutex.ᏑrLock));
                var w = rw.writer.ptr();
                if (w != nil) {
                    notewakeup(w.of(m.Ꮡpark));
                }
                unlock(Ꮡrw.of(rwmutex.ᏑrLock));
            }
        }
    }
    releaseLockRankAndM(rw.readRank);
}

// lock locks rw for writing.
internal static void @lock(this ж<rwmutex> Ꮡrw) {
    // Resolve competition with other writers and stick to our P.
    @lock(Ꮡrw.of(rwmutex.ᏑwLock));
    var m = getg().Value.m;
    // Announce that there is a pending writer.
    var r = Ꮡrw.of(rwmutex.ᏑreaderCount).Add(-rwmutexMaxReaders) + (int32)rwmutexMaxReaders;
    // Wait for any active readers to complete.
    @lock(Ꮡrw.of(rwmutex.ᏑrLock));
    if (r != 0 && Ꮡrw.of(rwmutex.ᏑreaderWait).Add(r) != 0){
        // Wait for reader to wake us up.
        var mʗ1 = m;
        systemstack(() => {
            Ꮡrw.Value.writer.set(mʗ1);
            unlock(Ꮡrw.of(rwmutex.ᏑrLock));
            notesleep(mʗ1.of(runtime_package.m.Ꮡpark));
            noteclear(mʗ1.of(runtime_package.m.Ꮡpark));
        });
    } else {
        unlock(Ꮡrw.of(rwmutex.ᏑrLock));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unlockOfUnlockedRwmutexˢ = "unlock of unlocked rwmutex"u8;

// unlock unlocks rw for writing.
internal static void unlock(this ж<rwmutex> Ꮡrw) {
    ref var rw = ref Ꮡrw.DerefOrNull();

    // Announce to readers that there is no active writer.
    var r = Ꮡrw.of(rwmutex.ᏑreaderCount).Add(rwmutexMaxReaders);
    if (r >= rwmutexMaxReaders) {
        @throw(unlockOfUnlockedRwmutexˢ);
    }
    // Unblock blocked readers.
    @lock(Ꮡrw.of(rwmutex.ᏑrLock));
    while (rw.readers.ptr() != nil) {
        var reader = rw.readers.ptr();
        rw.readers = reader.Value.schedlink;
        reader.of(m.Ꮡschedlink).set(nil);
        notewakeup(reader.of(m.Ꮡpark));
        r -= 1;
    }
    // If r > 0, there are pending readers that aren't on the
    // queue. Tell them to skip waiting.
    rw.readerPass += (uint32)r;
    unlock(Ꮡrw.of(rwmutex.ᏑrLock));
    // Allow other writers to proceed.
    unlock(Ꮡrw.of(rwmutex.ᏑwLock));
}

} // end runtime_package
