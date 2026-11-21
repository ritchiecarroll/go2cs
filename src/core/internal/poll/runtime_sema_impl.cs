// runtime_sema_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// Hand-written implementations of internal/poll's //go:linkname runtime semaphore primitives, declared in
// fd_mutex.go. In Go these are runtime/sema.go's poll_runtime_Semacquire and poll_runtime_Semrelease;
// go2cs emits them as bodyless `partial` methods, and without a body here the PartialStubGenerator fills
// them with a throwing stub -- so the first FD teardown (every os.File Close, hence
// os.ReadFile/WriteFile) crashed with "runtime_Semrelease: external function is not implemented"
// (surfaced by the Phase-4 ConvertedTestHarness fixture's testdata read).
//
// The one word they serve is FD.csema: FD.destroy releases it (linux/fd_unix.cs and darwin/fd_unix.cs
// destroy, windows/fd_windows.cs destroy) and FD.Close acquires it, waiting for the last reference to go.
// fdMutex's own rsema and wsema never reach these primitives: rwlock, rwunlock and increfAndClose are
// hand-owned in fd_mutex_impl.cs and wait on inline gates there.
//
// Both forward to golib's RuntimeSemaphore, the port of sema.go that sync, internal/sync and runtime
// already forward to, with Go's own arguments: poll_runtime_Semacquire is
// `semacquire1(addr, false, semaBlockProfile, 0, waitReasonSemacquire)` and poll_runtime_Semrelease is
// `semrelease1(addr, false, 0)`, no lifo and no handoff. So the count lives in csema itself, as in Go,
// and a word has a bucket only while a waiter is queued on it: in the common order, destroy before Close,
// neither call makes one.
//
// Until 2026-11-21 this file kept a private table whose bucket held the count, and both calls fetched the
// bucket, so no bucket could ever be removed: every FD that was ever closed kept its bucket, and the key
// (the pointer box over csema) kept the FD, and its os.File or netFD, alive for the life of the process.
// Guarded by GolibTests' PollSemaphoreTests.
//
// The wait reason is `semacquire`, which is what Go's poll_runtime_Semacquire passes (sema.go). NOT
// `IO wait`: that is netpollblock's reason and belongs to runtime_netpoll_impl.cs's pollBlock, one layer
// up. Both live in internal/poll and they are different waits: this one is Close waiting for the FD's
// last reference, that one is the completion itself.

// Aliased rather than imported wholesale: this file needs exactly two golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using RuntimeSemaphore = go.golib.RuntimeSemaphore;
using WaitReason = go.golib.WaitReason;

// Hand-owned (no runtime_sema.go exists, so a reconvert never regenerates it); marked for
// consistency with the other hand-owned operational files.
[module: go.GoManualConversion]

namespace go.@internal;

partial class poll_package
{
    internal static partial void runtime_Semacquire(ж<uint32> sema) => RuntimeSemaphore.Acquire(sema, WaitReason.Semacquire);

    internal static partial void runtime_Semrelease(ж<uint32> sema) => RuntimeSemaphore.Release(sema, handoff: false);
}
