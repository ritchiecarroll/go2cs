// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// go2cs NATIVE IMPLEMENTATION (hand-owned; replaces the converted waitgroup.go output). Go's WaitGroup
// packs a counter and a waiter count into one atomic uint64 and parks waiters on the runtime sleeping
// semaphore — the same primitive that cannot be faithfully emulated (see mutex.cs / runtime_impl.cs).
// Reimplemented natively: a guarded counter plus a latch event that is set whenever the counter is
// zero. Wait blocks on the latch; the Add that drives the counter to zero releases every waiter.
using System.Threading;
// Aliased rather than imported wholesale: this file needs exactly two golib types, and a blanket
// `using go.golib` would also pull that namespace's extension methods into a hand-owned file sitting
// beside converted code.
using Goroutine = go.golib.Goroutine;
using WaitReason = go.golib.WaitReason;

// Hand-owned native replacement of the converted waitgroup.go output — the marker makes a -stdlib
// reconvert skip regenerating this file (see containsManualConversionMarker).
[module: go.GoManualConversion]

namespace go;

partial class sync_package {

// A WaitGroup waits for a collection of goroutines to finish.
// A WaitGroup must not be copied after first use.
[GoType] partial struct WaitGroup {
    // Lazily-created shared state; a WaitGroup is always used through a pointer (ж<WaitGroup>).
    internal WaitGroupState? st;
}

// WaitGroupState is the native backing for a WaitGroup: a counter guarded by Gate, and an event that
// is signaled exactly while the counter is zero (so a fresh/zero WaitGroup lets Wait return at once).
internal sealed class WaitGroupState {
    internal int Counter;
    internal readonly ManualResetEventSlim Idle = new(true);
    internal readonly object Gate = new();
}

private static WaitGroupState wgStateOf(ж<WaitGroup> Ꮡwg) {
    ref var wg = ref Ꮡwg.Value;

    WaitGroupState? s = Volatile.Read(ref wg.st);

    if (s is not null) {
        return s;
    }

    var created = new WaitGroupState();
    return Interlocked.CompareExchange(ref wg.st, created, null) ?? created;
}

// Add adds delta, which may be negative, to the WaitGroup counter.
// If the counter becomes zero, all goroutines blocked on Wait are released.
// If the counter goes negative, Add panics.
public static void Add(this ж<WaitGroup> Ꮡwg, nint delta) {
    WaitGroupState s = wgStateOf(Ꮡwg);

    lock (s.Gate) {
        int c = s.Counter + ((int)delta);
        s.Counter = c;

        if (c < 0) {
            throw panic("sync: negative WaitGroup counter");
        }

        if (c == 0) {
            s.Idle.Set();
        } else {
            s.Idle.Reset();
        }
    }
}

// Done decrements the WaitGroup counter by one.
public static void Done(this ж<WaitGroup> Ꮡwg) => Ꮡwg.Add(-1);

// Wait blocks until the WaitGroup counter is zero.
//
// `semacquire`, not `sync.WaitGroup.Wait`: Go has no wait reason of the latter name. WaitGroup.Wait
// calls runtime_Semacquire, which is sema.go's sync_runtime_Semacquire, which parks with
// waitReasonSemacquire — so `semacquire` is what a Go traceback prints for a blocked Wait, and this
// reports the same word for the same wait.
public static void Wait(this ж<WaitGroup> Ꮡwg) {
    using (Goroutine.Park(WaitReason.Semacquire)) {
        wgStateOf(Ꮡwg).Idle.Wait();
    }
}

} // end sync_package
