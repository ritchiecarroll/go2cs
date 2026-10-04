// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package sync provides basic synchronization primitives such as mutual
// exclusion locks to internal packages (including ones that depend on sync).
//
// Tests are defined in package [sync].
namespace go.@internal;

using race = go.@internal.race_package;
using atomic = sync.atomic_package;
using @unsafe = unsafe_package;
using go.@internal;
using sync;

partial class sync_package {

// A Mutex is a mutual exclusion lock.
//
// See package [sync.Mutex] documentation.
[GoType] partial struct Mutex {
    internal int32 state;
    internal uint32 sema;
}

internal static UntypedInt mutexLocked => /* 1 << iota */ 1; // mutex is locked
internal static UntypedInt mutexWoken => 2;
internal static UntypedInt mutexStarving => 4;
internal static UntypedInt mutexWaiterShift => /* iota */ 3;
internal static UntypedFloat starvationThresholdNs => 1e6;

// Lock locks m.
//
// See package [sync.Mutex] documentation.
public static void Lock(this ж<Mutex> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    // Fast path: grab unlocked mutex.
    if (atomic.CompareAndSwapInt32(Ꮡm.of(Mutex.Ꮡstate), 0, mutexLocked)) {
        if (race.Enabled) {
            race.Acquire((uintptr)@unsafe.Pointer.FromRef(ref m));
        }
        return;
    }
    // Slow path (outlined so that the fast path can be inlined)
    Ꮡm.lockSlow();
}

// TryLock tries to lock m and reports whether it succeeded.
//
// See package [sync.Mutex] documentation.
public static bool TryLock(this ж<Mutex> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    var old = m.state;
    if ((int32)(old & ((int32)((int32)mutexLocked | (int32)mutexStarving))) != 0) {
        return false;
    }
    // There may be a goroutine waiting for the mutex, but we are
    // running now and can try to grab the mutex before that
    // goroutine wakes up.
    if (!atomic.CompareAndSwapInt32(Ꮡm.of(Mutex.Ꮡstate), old, (int32)(old | (int32)mutexLocked))) {
        return false;
    }
    if (race.Enabled) {
        race.Acquire((uintptr)@unsafe.Pointer.FromRef(ref m));
    }
    return true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syncInconsistentMutexˢ = "sync: inconsistent mutex state"u8;

internal static void lockSlow(this ж<Mutex> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    int64 waitStartTime = default!;
    var starving = false;
    var awoke = false;
    nint iter = 0;
    var old = m.state;
    while (ᐧ) {
        // Don't spin in starvation mode, ownership is handed off to waiters
        // so we won't be able to acquire the mutex anyway.
        if ((int32)(old & ((int32)((int32)mutexLocked | (int32)mutexStarving))) == mutexLocked && runtime_canSpin(iter)) {
            // Active spinning makes sense.
            // Try to set mutexWoken flag to inform Unlock
            // to not wake other blocked goroutines.
            if (!awoke && (int32)(old & (int32)mutexWoken) == 0 && (old >> (int)(mutexWaiterShift)) != 0 && atomic.CompareAndSwapInt32(Ꮡm.of(Mutex.Ꮡstate), old, (int32)(old | (int32)mutexWoken))) {
                awoke = true;
            }
            runtime_doSpin();
            iter++;
            old = m.state;
            continue;
        }
        var @new = old;
        // Don't try to acquire starving mutex, new arriving goroutines must queue.
        if ((int32)(old & (int32)mutexStarving) == 0) {
            @new |= (int32)(mutexLocked);
        }
        if ((int32)(old & ((int32)((int32)mutexLocked | (int32)mutexStarving))) != 0) {
            @new += (int32)(1 << (int)(mutexWaiterShift));
        }
        // The current goroutine switches mutex to starvation mode.
        // But if the mutex is currently unlocked, don't do the switch.
        // Unlock expects that starving mutex has waiters, which will not
        // be true in this case.
        if (starving && (int32)(old & (int32)mutexLocked) != 0) {
            @new |= (int32)(mutexStarving);
        }
        if (awoke) {
            // The goroutine has been woken from sleep,
            // so we need to reset the flag in either case.
            if ((int32)(@new & (int32)mutexWoken) == 0) {
                @throw(syncInconsistentMutexˢ);
            }
            @new &= ~(int32)(mutexWoken);
        }
        if (atomic.CompareAndSwapInt32(Ꮡm.of(Mutex.Ꮡstate), old, @new)){
            if ((int32)(old & ((int32)((int32)mutexLocked | (int32)mutexStarving))) == 0) {
                break; // locked the mutex with CAS
            }
            // If we were already waiting before, queue at the front of the queue.
            var queueLifo = waitStartTime != 0;
            if (waitStartTime == 0) {
                waitStartTime = runtime_nanotime();
            }
            runtime_SemacquireMutex(Ꮡm.of(Mutex.Ꮡsema), queueLifo, 2);
            starving = starving || runtime_nanotime() - waitStartTime > (int64)starvationThresholdNs;
            old = m.state;
            if ((int32)(old & (int32)mutexStarving) != 0) {
                // If this goroutine was woken and mutex is in starvation mode,
                // ownership was handed off to us but mutex is in somewhat
                // inconsistent state: mutexLocked is not set and we are still
                // accounted as waiter. Fix that.
                if ((int32)(old & ((int32)((int32)mutexLocked | (int32)mutexWoken))) != 0 || (old >> (int)(mutexWaiterShift)) == 0) {
                    @throw(syncInconsistentMutexˢ);
                }
                var delta = (int32)(mutexLocked - (1 << (int)(mutexWaiterShift)));
                if (!starving || (old >> (int)(mutexWaiterShift)) == 1) {
                    // Exit starvation mode.
                    // Critical to do it here and consider wait time.
                    // Starvation mode is so inefficient, that two goroutines
                    // can go lock-step infinitely once they switch mutex
                    // to starvation mode.
                    delta -= mutexStarving;
                }
                atomic.AddInt32(Ꮡm.of(Mutex.Ꮡstate), delta);
                break;
            }
            awoke = true;
            iter = 0;
        } else {
            old = m.state;
        }
    }
    if (race.Enabled) {
        race.Acquire((uintptr)@unsafe.Pointer.FromRef(ref m));
    }
}

// Unlock unlocks m.
//
// See package [sync.Mutex] documentation.
public static void Unlock(this ж<Mutex> Ꮡm) {
    ref var m = ref Ꮡm.DerefOrNull();

    if (race.Enabled) {
        _ = m.state;
        race.Release((uintptr)@unsafe.Pointer.FromRef(ref m));
    }
    // Fast path: drop lock bit.
    var @new = atomic.AddInt32(Ꮡm.of(Mutex.Ꮡstate), -mutexLocked);
    if (@new != 0) {
        // Outlined slow path to allow inlining the fast path.
        // To hide unlockSlow during tracing we skip one extra frame when tracing GoUnblock.
        Ꮡm.unlockSlow(@new);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syncUnlockOfUnlockedˢ = "sync: unlock of unlocked mutex"u8;

internal static void unlockSlow(this ж<Mutex> Ꮡm, int32 @new) {
    ref var m = ref Ꮡm.DerefOrNull();

    if ((int32)((@new + (int32)mutexLocked) & (int32)mutexLocked) == 0) {
        fatal(syncUnlockOfUnlockedˢ);
    }
    if ((int32)(@new & (int32)mutexStarving) == 0){
        var old = @new;
        while (ᐧ) {
            // If there are no waiters or a goroutine has already
            // been woken or grabbed the lock, no need to wake anyone.
            // In starvation mode ownership is directly handed off from unlocking
            // goroutine to the next waiter. We are not part of this chain,
            // since we did not observe mutexStarving when we unlocked the mutex above.
            // So get off the way.
            if ((old >> (int)(mutexWaiterShift)) == 0 || (int32)(old & ((int32)((UntypedInt)(mutexLocked | mutexWoken) | (int32)mutexStarving))) != 0) {
                return;
            }
            // Grab the right to wake someone.
            @new = (int32)((old - (int32)(1 << (int)(mutexWaiterShift))) | (int32)mutexWoken);
            if (atomic.CompareAndSwapInt32(Ꮡm.of(Mutex.Ꮡstate), old, @new)) {
                runtime_Semrelease(Ꮡm.of(Mutex.Ꮡsema), false, 2);
                return;
            }
            old = m.state;
        }
    } else {
        // Starving mode: handoff mutex ownership to the next waiter, and yield
        // our time slice so that the next waiter can start to run immediately.
        // Note: mutexLocked is not set, the waiter will set it after wakeup.
        // But mutex is still considered locked if mutexStarving is set,
        // so new coming goroutines won't acquire it.
        runtime_Semrelease(Ꮡm.of(Mutex.Ꮡsema), true, 2);
    }
}

} // end sync_package
