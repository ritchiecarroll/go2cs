// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (aix || darwin || dragonfly || freebsd || linux || netbsd || openbsd || plan9 || solaris || windows) && goexperiment.spinbitmutex
namespace go;

using goarch = @internal.goarch_package;
using atomic = @internal.runtime.atomic_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using System.Runtime.CompilerServices;

partial class runtime_package {

// This implementation depends on OS-specific implementations of
//
//	func semacreate(mp *m)
//		Create a semaphore for mp, if it does not already have one.
//
//	func semasleep(ns int64) int32
//		If ns < 0, acquire m's semaphore and return 0.
//		If ns >= 0, try to acquire m's semaphore for at most ns nanoseconds.
//		Return 0 if the semaphore was acquired, -1 if interrupted or timed out.
//
//	func semawakeup(mp *m)
//		Wake up mp, which is or will soon be sleeping on its semaphore.
// The mutex state consists of four flags and a pointer. The flag at bit 0,
// mutexLocked, represents the lock itself. Bit 1, mutexSleeping, is a hint that
// the pointer is non-nil. The fast paths for locking and unlocking the mutex
// are based on atomic 8-bit swap operations on the low byte; bits 2 through 7
// are unused.
//
// Bit 8, mutexSpinning, is a try-lock that grants a waiting M permission to
// spin on the state word. Most other Ms must attempt to spend their time
// sleeping to reduce traffic on the cache line. This is the "spin bit" for
// which the implementation is named. (The anti-starvation mechanism also grants
// temporary permission for an M to spin.)
//
// Bit 9, mutexStackLocked, is a try-lock that grants an unlocking M permission
// to inspect the list of waiting Ms and to pop an M off of that stack.
//
// The upper bits hold a (partial) pointer to the M that most recently went to
// sleep. The sleeping Ms form a stack linked by their mWaitList.next fields.
// Because the fast paths use an 8-bit swap on the low byte of the state word,
// we'll need to reconstruct the full M pointer from the bits we have. Most Ms
// are allocated on the heap, and have a known alignment and base offset. (The
// offset is due to mallocgc's allocation headers.) The main program thread uses
// a static M value, m0. We check for m0 specifically and add a known offset
// otherwise.
internal static UntypedInt active_spin => 4; // referenced in proc.go for sync.Mutex implementation
internal static UntypedInt active_spin_cnt => 30; // referenced in proc.go for sync.Mutex implementation

internal static UntypedInt mutexLocked => 0x001;
internal static UntypedInt mutexSleeping => 0x002;
internal static UntypedInt mutexSpinning => 0x100;
internal static UntypedInt mutexStackLocked => 0x200;
internal static UntypedInt mutexMMask => 0x3FF;
internal static UntypedInt mutexMOffset => /* mallocHeaderSize */ 8; // alignment of heap-allocated Ms (those other than m0)
internal static UntypedInt mutexActiveSpinCount => 4;
internal static UntypedInt mutexActiveSpinSize => 30;
internal static UntypedInt mutexPassiveSpinCount => 1;
internal static UntypedInt mutexTailWakePeriod => 16;

//go:nosplit
internal static ж<uint8> key8(ж<uintptr> Ꮡp) {
    if (goarch.BigEndian) {
        return ((ж<array<uint8>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡp))).at<uint8>(goarch.PtrSize / 1 - 1);
    }
    return ((ж<array<uint8>>)(uintptr)(@unsafe.Pointer.FromPinnedBox(Ꮡp))).at<uint8>(0);
}

// mWaitList is part of the M struct, and holds the list of Ms that are waiting
// for a particular runtime.mutex.
//
// When an M is unable to immediately obtain a lock, it adds itself to the list
// of Ms waiting for the lock. It does that via this struct's next field,
// forming a singly-linked list with the mutex's key field pointing to the head
// of the list.
[GoType] partial struct mWaitList {
    internal muintptr next; // next m waiting for lock
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeMMemoryAlignmentˢ = "runtime.m memory alignment too small for spinbit mutex"u8;

// lockVerifyMSize confirms that we can recreate the low bits of the M pointer.
internal static void lockVerifyMSize() {
    var size = roundupsize(/* unsafe.Sizeof(m{}) */ (uintptr)1832, false) + (uintptr)mallocHeaderSize;
    if ((uintptr)(size & (uintptr)mutexMMask) != 0) {
        print((@string)"M structure uses sizeclass "u8, size, (@string)"/"u8, ((Δhex)(uint64)size), (@string)" bytes; "u8,
            (@string)"incompatible with mutex flag mask "u8, ((Δhex)mutexMMask), (@string)"\n"u8);
        @throw(runtimeMMemoryAlignmentˢ);
    }
}

// mutexWaitListHead recovers a full muintptr that was missing its low bits.
// With the exception of the static m0 value, it requires allocating runtime.m
// values in a size class with a particular minimum alignment. The 2048-byte
// size class allows recovering the full muintptr value even after overwriting
// the low 11 bits with flags. We can use those 11 bits as 3 flags and an
// atomically-swapped byte.
//
//go:nosplit
internal static muintptr mutexWaitListHead(uintptr v) {
    {
        var highBits = (uintptr)(v & ~(uintptr)(uintptr)mutexMMask); if (highBits == 0){
            return 0;
        } else 
        {
            var m0bits = new muintptr(Ꮡm0); if (highBits == (uintptr)((uintptr)m0bits & ~(uintptr)(uintptr)mutexMMask)){
                return m0bits;
            } else {
                return ((muintptr)(highBits + (uintptr)mutexMOffset));
            }
        }
    }
}

// mutexPreferLowLatency reports if this mutex prefers low latency at the risk
// of performance collapse. If so, we can allow all waiting threads to spin on
// the state word rather than go to sleep.
//
// TODO: We could have the waiting Ms each spin on their own private cache line,
// especially if we can put a bound on the on-CPU time that would consume.
//
// TODO: If there's a small set of mutex values with special requirements, they
// could make use of a more specialized lock2/unlock2 implementation. Otherwise,
// we're constrained to what we can fit within a single uintptr with no
// additional storage on the M for each lock held.
//
//go:nosplit
internal static bool mutexPreferLowLatency(ж<mutex> Ꮡl) {
    var exprᴛ1 = Ꮡl;
    if (exprᴛ1 == Ꮡsched.of(schedt.Ꮡlock)) {
        return true;
    }
    { /* default: */
        return false;
    }

}

// We often expect sched.lock to pass quickly between Ms in a way that
// each M has unique work to do: for instance when we stop-the-world
// (bringing each P to idle) or add new netpoller-triggered work to the
// global run queue.
// go2cs generated this placeholder — func mutexContended is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static void @lock(ж<mutex> Ꮡl) {
    lockWithRank(Ꮡl, getLockRank(Ꮡl));
}

// go2cs generated this placeholder — func lock2 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

internal static partial void unlock(ж<mutex> Ꮡl) {
    unlockWithRank(Ꮡl);
}

// go2cs generated this placeholder — func unlock2 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func unlock2Wake is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

} // end runtime_package
