// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using @unsafe = unsafe_package;

partial class sync_package {

// defined in package runtime

// Semacquire waits until *s > 0 and then atomically decrements it.
// It is intended as a simple sleep primitive for use by the synchronization
// library and should not be used directly.
internal static partial void runtime_Semacquire(ж<uint32> s);

// SemacquireWaitGroup is like Semacquire, but for WaitGroup.Wait.
internal static partial void runtime_SemacquireWaitGroup(ж<uint32> s, bool synctestDurable);

// Semacquire(RW)Mutex(R) is like Semacquire, but for profiling contended
// Mutexes and RWMutexes.
// If lifo is true, queue waiter at the head of wait queue.
// skipframes is the number of frames to omit during tracing, counting from
// runtime_SemacquireMutex's caller.
// The different forms of this function just tell the runtime how to present
// the reason for waiting in a backtrace, and is used to compute some metrics.
// Otherwise they're functionally identical.
internal static partial void runtime_SemacquireRWMutexR(ж<uint32> s, bool lifo, nint skipframes);

internal static partial void runtime_SemacquireRWMutex(ж<uint32> s, bool lifo, nint skipframes);

// Semrelease atomically increments *s and notifies a waiting goroutine
// if one is blocked in Semacquire.
// It is intended as a simple wakeup primitive for use by the synchronization
// library and should not be used directly.
// If handoff is true, pass count directly to the first waiter.
// skipframes is the number of frames to omit during tracing, counting from
// runtime_Semrelease's caller.
internal static partial void runtime_Semrelease(ж<uint32> s, bool handoff, nint skipframes);

// See runtime/sema.go for documentation.
internal static partial uint32 runtime_notifyListAdd(ж<notifyList> l);

// See runtime/sema.go for documentation.
internal static partial void runtime_notifyListWait(ж<notifyList> l, uint32 t);

// See runtime/sema.go for documentation.
internal static partial void runtime_notifyListNotifyAll(ж<notifyList> l);

// See runtime/sema.go for documentation.
internal static partial void runtime_notifyListNotifyOne(ж<notifyList> l);

// Ensure that sync and runtime agree on size of notifyList.
internal static partial void runtime_notifyListCheck(uintptr size);

[GoInit] internal static void initΔ1() {
    notifyList n = default;
    runtime_notifyListCheck(/* unsafe.Sizeof(n) */ (uintptr)32);
}

internal static partial void @throw(@string _);

internal static partial void fatal(@string _);

} // end sync_package
