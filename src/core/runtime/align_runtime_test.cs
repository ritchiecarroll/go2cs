// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file lives in the runtime package
// so we can get access to the runtime guts.
// The rest of the implementation of this test is in align_test.go.
namespace go;

using @unsafe = unsafe_package;
using static global::go.runtime_package;

partial class runtime_internal_test_package {

// AtomicFields is the set of fields on which we perform 64-bit atomic
// operations (all the *64 operations in internal/runtime/atomic).
public static slice<uintptr> AtomicFields = new uintptr[]{
    /* unsafe.Offsetof(m{}.procid) */ (uintptr)64,
    /* unsafe.Offsetof(profBuf{}.overflow) */ (uintptr)16,
    /* unsafe.Offsetof(profBuf{}.overflowTime) */ (uintptr)24,
    /* unsafe.Offsetof(heapStatsDelta{}.tinyAllocCount) */ (uintptr)40,
    /* unsafe.Offsetof(heapStatsDelta{}.smallAllocCount) */ (uintptr)64,
    /* unsafe.Offsetof(heapStatsDelta{}.smallFreeCount) */ (uintptr)624,
    /* unsafe.Offsetof(heapStatsDelta{}.largeAlloc) */ (uintptr)48,
    /* unsafe.Offsetof(heapStatsDelta{}.largeAllocCount) */ (uintptr)56,
    /* unsafe.Offsetof(heapStatsDelta{}.largeFree) */ (uintptr)608,
    /* unsafe.Offsetof(heapStatsDelta{}.largeFreeCount) */ (uintptr)616,
    /* unsafe.Offsetof(heapStatsDelta{}.committed) */ (uintptr)0,
    /* unsafe.Offsetof(heapStatsDelta{}.released) */ (uintptr)8,
    /* unsafe.Offsetof(heapStatsDelta{}.inHeap) */ (uintptr)16,
    /* unsafe.Offsetof(heapStatsDelta{}.inStacks) */ (uintptr)24,
    /* unsafe.Offsetof(heapStatsDelta{}.inWorkBufs) */ (uintptr)32,
    /* unsafe.Offsetof(lfnode{}.next) */ (uintptr)0,
    /* unsafe.Offsetof(mstats{}.last_gc_nanotime) */ (uintptr)7696,
    /* unsafe.Offsetof(mstats{}.last_gc_unix) */ (uintptr)3568,
    /* unsafe.Offsetof(workType{}.bytesMarked) */ (uintptr)368
}.slice();

// AtomicVariables is the set of global variables on which we perform
// 64-bit atomic operations.
public static slice<@unsafe.Pointer> AtomicVariables;
internal static void initᴛAtomicVariables() { AtomicVariables = new @unsafe.Pointer[]{
    @unsafe.Pointer.FromPinnedBox(Ꮡncgocall),
    @unsafe.Pointer.FromPinnedBox(Ꮡtest_z64),
    @unsafe.Pointer.FromPinnedBox(Ꮡblockprofilerate),
    @unsafe.Pointer.FromPinnedBox(Ꮡmutexprofilerate),
    @unsafe.Pointer.FromPinnedBox(ᏑgcController),
    @unsafe.Pointer.FromPinnedBox(Ꮡmemstats),
    @unsafe.Pointer.FromPinnedBox(Ꮡsched),
    @unsafe.Pointer.FromPinnedBox(Ꮡticks),
    @unsafe.Pointer.FromPinnedBox(Ꮡwork)
}.slice(); }

} // end runtime_internal_test_package
