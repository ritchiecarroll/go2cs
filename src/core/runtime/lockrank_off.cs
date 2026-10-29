// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !goexperiment.staticlockranking
namespace go;

using System.Runtime.CompilerServices;

partial class runtime_package {

internal const bool staticLockRanking = false;

// // lockRankStruct is embedded in mutex, but is empty when staticklockranking is
// disabled (the default)
partial struct lockRankStruct {
}

internal static void lockInit(ref mutex l, lockRank rank) {
}

internal static lockRank getLockRank(ж<mutex> l) {
    return 0;
}

internal static void lockWithRank(ж<mutex> l, lockRank rank) {
    lock2(l);
}

// This function may be called in nosplit context and thus must be nosplit.
//
//go:nosplit
internal static void acquireLockRankAndM(lockRank rank) {
    acquirem();
}

internal static partial void unlockWithRank(ж<mutex> l) {
    unlock2(l);
}

// This function may be called in nosplit context and thus must be nosplit.
//
//go:nosplit
internal static void releaseLockRankAndM(lockRank rank) {
    releasem(ref ((~getg()).m).DerefOrNull());
}

// This function may be called in nosplit context and thus must be nosplit.
//
//go:nosplit
internal static void lockWithRankMayAcquire(ж<mutex> l, lockRank rank) {
}

//go:nosplit
internal static void assertLockHeld(ж<mutex> l) {
}

//go:nosplit
internal static void assertRankHeld(lockRank r) {
}

//go:nosplit
internal static void worldStopped() {
}

//go:nosplit
internal static void worldStarted() {
}

//go:nosplit
internal static void assertWorldStopped() {
}

//go:nosplit
internal static void assertWorldStoppedOrLockHeld(ref mutex l) {
}

} // end runtime_package
