// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// GOMAXPROCS=10 go test
// This is a copy of sync/rwmutex_test.go rewritten to test the
// runtime rwmutex.
namespace go;

using fmt = fmt_package;
using static runtime_package;
using Δdebug = global::go.runtime.debug_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.runtime;
using global::go.sync;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

internal static void parallelReader(ж<global::go.runtime_internal_test_package.RWMutex> Ꮡm, channel<bool> clocked, ж<atomic.Bool> Ꮡcunlock, channel<bool> cdone) {
    Ꮡm.RLock();
    clocked.ᐸꟷ(true);
    while (!Ꮡcunlock.Load()) {
    }
    Ꮡm.RUnlock();
    cdone.ᐸꟷ(true);
}

internal static partial void doTestParallelReaders(nint numReaders) {
    GOMAXPROCS(numReaders + 1);
    ref var m = ref heap(new global::go.runtime_internal_test_package.RWMutex(), out var Ꮡm);
    Ꮡm.Init();
    var clocked = new channel<bool>(numReaders);
    ref var cunlock = ref heap(new atomic.Bool(), out var Ꮡcunlock);
    var cdone = new channel<bool>(0);
    for (nint i = 0; i < numReaders; i++) {
        goǃ(parallelReader, Ꮡm, clocked, Ꮡcunlock, cdone);
    }
    // Wait for all parallel RLock()s to succeed.
    for (nint i = 0; i < numReaders; i++) {
        ᐸꟷ(clocked);
    }
    Ꮡcunlock.Store(true);
    // Wait for the goroutines to finish.
    for (nint i = 0; i < numReaders; i++) {
        ᐸꟷ(cdone);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wasmHasNoThreadsYetˢ = (@string)"wasm has no threads yet"u8;

public static void TestParallelRWMutexReaders(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (GOARCH == "wasm"u8) {
            Ꮡt.Skip(wasmHasNoThreadsYetˢ);
        }
        defer(GOMAXPROCS, GOMAXPROCS(-1), ref ᒐ);
        // If runtime triggers a forced GC during this test then it will deadlock,
        // since the goroutines can't be stopped/preempted.
        // Disable GC for this test (see issue #10958).
        defer(Δdebug.SetGCPercent, Δdebug.SetGCPercent(-1), ref ᒐ);
        // SetGCPercent waits until the mark phase is over, but the runtime
        // also preempts at the start of the sweep phase, so make sure that's
        // done too.
        GC();
        doTestParallelReaders(1);
        doTestParallelReaders(3);
        doTestParallelReaders(4);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void reader(ж<global::go.runtime_internal_test_package.RWMutex> Ꮡrwm, nint num_iterations, ж<int32> Ꮡactivity, channel<bool> cdone) {
    for (nint i = 0; i < num_iterations; i++) {
        Ꮡrwm.RLock();
        var n = atomic.AddInt32(Ꮡactivity, 1);
        if (n < 1 || n >= 10000) {
            throw panic(fmt.Sprintf("wlock(%d)\n"u8, n));
        }
        for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
        }
        atomic.AddInt32(Ꮡactivity, -1);
        Ꮡrwm.RUnlock();
    }
    cdone.ᐸꟷ(true);
}

internal static void writer(ж<global::go.runtime_internal_test_package.RWMutex> Ꮡrwm, nint num_iterations, ж<int32> Ꮡactivity, channel<bool> cdone) {
    for (nint i = 0; i < num_iterations; i++) {
        Ꮡrwm.Lock();
        var n = atomic.AddInt32(Ꮡactivity, 10000);
        if (n != 10000) {
            throw panic(fmt.Sprintf("wlock(%d)\n"u8, n));
        }
        for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
        }
        atomic.AddInt32(Ꮡactivity, -10000);
        Ꮡrwm.Unlock();
    }
    cdone.ᐸꟷ(true);
}

public static partial void HammerRWMutex(nint gomaxprocs, nint numReaders, nint num_iterations) {
    GOMAXPROCS(gomaxprocs);
    // Number of active readers + 10000 * number of active writers.
    ref var activity = ref heap(new int32(), out var Ꮡactivity);
    ref var rwm = ref heap(new global::go.runtime_internal_test_package.RWMutex(), out var Ꮡrwm);
    Ꮡrwm.Init();
    var cdone = new channel<bool>(0);
    goǃ(writer, Ꮡrwm, num_iterations, Ꮡactivity, cdone);
    nint i = default!;
    for (i = 0; i < numReaders / 2; i++) {
        goǃ(reader, Ꮡrwm, num_iterations, Ꮡactivity, cdone);
    }
    goǃ(writer, Ꮡrwm, num_iterations, Ꮡactivity, cdone);
    for (; i < numReaders; i++) {
        goǃ(reader, Ꮡrwm, num_iterations, Ꮡactivity, cdone);
    }
    // Wait for the 2 writers and all readers to finish.
    for (nint iΔ1 = 0; iΔ1 < 2 + numReaders; iΔ1++) {
        ᐸꟷ(cdone);
    }
}

public static void TestRWMutex(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(GOMAXPROCS, GOMAXPROCS(-1), ref ᒐ);
        nint n = 1000;
        if (testing.Short()) {
            n = 5;
        }
        HammerRWMutex(1, 1, n);
        HammerRWMutex(1, 3, n);
        HammerRWMutex(1, 10, n);
        HammerRWMutex(4, 1, n);
        HammerRWMutex(4, 3, n);
        HammerRWMutex(4, 10, n);
        HammerRWMutex(10, 1, n);
        HammerRWMutex(10, 3, n);
        HammerRWMutex(10, 10, n);
        HammerRWMutex(10, 5, n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct BenchmarkRWMutexUncontended_PaddedRWMutex /*dyn*/ {
    public partial ref global::go.runtime_internal_test_package.RWMutex RWMutex { get; }
    internal array<uint32> pad = new(32);
}

public static void BenchmarkRWMutexUncontended(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        ref var rwm = ref heap(new BenchmarkRWMutexUncontended_PaddedRWMutex(), out var Ꮡrwm);
        Ꮡrwm.of(BenchmarkRWMutexUncontended_PaddedRWMutex.ᏑRWMutex).Init();
        while (pb.Next()) {
            Ꮡrwm.of(BenchmarkRWMutexUncontended_PaddedRWMutex.ᏑRWMutex).RLock();
            Ꮡrwm.of(BenchmarkRWMutexUncontended_PaddedRWMutex.ᏑRWMutex).RLock();
            Ꮡrwm.of(BenchmarkRWMutexUncontended_PaddedRWMutex.ᏑRWMutex).RUnlock();
            Ꮡrwm.of(BenchmarkRWMutexUncontended_PaddedRWMutex.ᏑRWMutex).RUnlock();
            Ꮡrwm.of(BenchmarkRWMutexUncontended_PaddedRWMutex.ᏑRWMutex).Lock();
            Ꮡrwm.of(BenchmarkRWMutexUncontended_PaddedRWMutex.ᏑRWMutex).Unlock();
        }
    });
}

internal static void benchmarkRWMutex(ж<testing.B> Ꮡb, nint localWork, nint writeRatio) {
    ref var rwm = ref heap(new global::go.runtime_internal_test_package.RWMutex(), out var Ꮡrwm);
    Ꮡrwm.Init();
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        nint foo = 0;
        while (pb.Next()) {
            foo++;
            if (rem(foo, writeRatio) == 0){
                Ꮡrwm.Lock();
                Ꮡrwm.Unlock();
            } else {
                Ꮡrwm.RLock();
                for (nint i = 0; i != localWork; i += 1) {
                    foo *= 2;
                    foo /= 2;
                }
                Ꮡrwm.RUnlock();
            }
        }
        _ = foo;
    });
}

public static void BenchmarkRWMutexWrite100(ж<testing.B> Ꮡb) {
    benchmarkRWMutex(Ꮡb, 0, 100);
}

public static void BenchmarkRWMutexWrite10(ж<testing.B> Ꮡb) {
    benchmarkRWMutex(Ꮡb, 0, 10);
}

public static void BenchmarkRWMutexWorkWrite100(ж<testing.B> Ꮡb) {
    benchmarkRWMutex(Ꮡb, 100, 100);
}

public static void BenchmarkRWMutexWorkWrite10(ж<testing.B> Ꮡb) {
    benchmarkRWMutex(Ꮡb, 100, 10);
}

} // end runtime_test_package
