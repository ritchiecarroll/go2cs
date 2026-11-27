// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

// import _ "unsafe" (nothing to initialize)

partial class sync_package {

// defined in package runtime

// SemacquireMutex is like Semacquire, but for profiling contended
// Mutexes and RWMutexes.
// If lifo is true, queue waiter at the head of wait queue.
// skipframes is the number of frames to omit during tracing, counting from
// runtime_SemacquireMutex's caller.
// The different forms of this function just tell the runtime how to present
// the reason for waiting in a backtrace, and is used to compute some metrics.
// Otherwise they're functionally identical.
//
//go:linkname runtime_SemacquireMutex
internal static partial void runtime_SemacquireMutex(ж<uint32> s, bool lifo, nint skipframes);

// Semrelease atomically increments *s and notifies a waiting goroutine
// if one is blocked in Semacquire.
// It is intended as a simple wakeup primitive for use by the synchronization
// library and should not be used directly.
// If handoff is true, pass count directly to the first waiter.
// skipframes is the number of frames to omit during tracing, counting from
// runtime_Semrelease's caller.
//
//go:linkname runtime_Semrelease
internal static partial void runtime_Semrelease(ж<uint32> s, bool handoff, nint skipframes);

// Active spinning runtime support.
// runtime_canSpin reports whether spinning makes sense at the moment.
//
//go:linkname runtime_canSpin
internal static partial bool runtime_canSpin(nint i);

// runtime_doSpin does active spinning.
//
//go:linkname runtime_doSpin
internal static partial void runtime_doSpin();

//go:linkname runtime_nanotime
internal static partial int64 runtime_nanotime();

//go:linkname throw
internal static partial void @throw(@string _);

//go:linkname fatal
internal static partial void fatal(@string _);

} // end sync_package
