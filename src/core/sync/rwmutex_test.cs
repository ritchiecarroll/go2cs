// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// GOMAXPROCS=10 go test
namespace go;

using fmt = fmt_package;
using Δruntime = runtime_package;
using static sync_package;
using atomic = go.sync.atomic_package;
using Δtesting = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.sync;
using static go.sync_internal_test_package;
using Δsync = sync_package;

partial class sync_test_package {

// There is a modified copy of this file in runtime/rwmutex_test.go.
// If you make any changes here, see if you should make them there.
internal static void parallelReader(ж<Δsync.RWMutex> Ꮡm, channel<bool> clocked, channel<bool> cunlock, channel<bool> cdone) {
    Ꮡm.RLock();
    clocked.ᐸꟷ(true);
    ᐸꟷ(cunlock);
    Ꮡm.RUnlock();
    cdone.ᐸꟷ(true);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void doTestParallelReaders(nint numReaders, nint gomaxprocs) {
    Δruntime.GOMAXPROCS(gomaxprocs);
    ref var m = ref heap(new Δsync.RWMutex(), out var Ꮡm);
    var clocked = new channel<bool>(0);
    var cunlock = new channel<bool>(0);
    var cdone = new channel<bool>(0);
    for (nint i = 0; i < numReaders; i++) {
        goǃ(parallelReader, Ꮡm, clocked, cunlock, cdone);
    }
    // Wait for all parallel RLock()s to succeed.
    for (nint i = 0; i < numReaders; i++) {
        ᐸꟷ(clocked);
    }
    for (nint i = 0; i < numReaders; i++) {
        cunlock.ᐸꟷ(true);
    }
    // Wait for the goroutines to finish.
    for (nint i = 0; i < numReaders; i++) {
        ᐸꟷ(cdone);
    }
}

public static void TestParallelReaders(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(-1), ref ᒐ);
        doTestParallelReaders(1, 4);
        doTestParallelReaders(3, 4);
        doTestParallelReaders(4, 2);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void reader(ж<Δsync.RWMutex> Ꮡrwm, nint num_iterations, ж<int32> Ꮡactivity, channel<bool> cdone) {
    for (nint i = 0; i < num_iterations; i++) {
        Ꮡrwm.RLock();
        var n = atomic.AddInt32(Ꮡactivity, 1);
        if (n < 1 || n >= 10000) {
            Ꮡrwm.RUnlock();
            throw panic(fmt.Sprintf("wlock(%d)\n"u8, n));
        }
        for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
        }
        atomic.AddInt32(Ꮡactivity, -1);
        Ꮡrwm.RUnlock();
    }
    cdone.ᐸꟷ(true);
}

internal static void writer(ж<Δsync.RWMutex> Ꮡrwm, nint num_iterations, ж<int32> Ꮡactivity, channel<bool> cdone) {
    for (nint i = 0; i < num_iterations; i++) {
        Ꮡrwm.Lock();
        var n = atomic.AddInt32(Ꮡactivity, 10000);
        if (n != 10000) {
            Ꮡrwm.Unlock();
            throw panic(fmt.Sprintf("wlock(%d)\n"u8, n));
        }
        for (nint iΔ1 = 0; iΔ1 < 100; iΔ1++) {
        }
        atomic.AddInt32(Ꮡactivity, -10000);
        Ꮡrwm.Unlock();
    }
    cdone.ᐸꟷ(true);
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void HammerRWMutex(nint gomaxprocs, nint numReaders, nint num_iterations) {
    Δruntime.GOMAXPROCS(gomaxprocs);
    // Number of active readers + 10000 * number of active writers.
    ref var activity = ref heap(new int32(), out var Ꮡactivity);
    ref var rwm = ref heap(new Δsync.RWMutex(), out var Ꮡrwm);
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

public static void TestRWMutex(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var m = ref heap(new Δsync.RWMutex(), out var Ꮡm);
        Ꮡm.Lock();
        if (Ꮡm.TryLock()) {
            Ꮡt.Fatalf("TryLock succeeded with mutex locked"u8);
        }
        if (Ꮡm.TryRLock()) {
            Ꮡt.Fatalf("TryRLock succeeded with mutex locked"u8);
        }
        Ꮡm.Unlock();
        if (!Ꮡm.TryLock()) {
            Ꮡt.Fatalf("TryLock failed with mutex unlocked"u8);
        }
        Ꮡm.Unlock();
        if (!Ꮡm.TryRLock()) {
            Ꮡt.Fatalf("TryRLock failed with mutex unlocked"u8);
        }
        if (!Ꮡm.TryRLock()) {
            Ꮡt.Fatalf("TryRLock failed with mutex rlocked"u8);
        }
        if (Ꮡm.TryLock()) {
            Ꮡt.Fatalf("TryLock succeeded with mutex rlocked"u8);
        }
        Ꮡm.RUnlock();
        Ꮡm.RUnlock();
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(-1), ref ᒐ);
        nint n = 1000;
        if (Δtesting.Short()) {
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object rLockerDidnTReadLockItˢ = (@string)"RLocker() didn't read-lock it"u8;
internal static readonly object rLockerDidnTRespectTheˢ = (@string)"RLocker() didn't respect the write lock"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestRLocker(ж<Δtesting.T> Ꮡt) {
    ref var wl = ref heap(new Δsync.RWMutex(), out var Ꮡwl);
    Δsync.Locker rl = default!;
    var wlocked = new channel<bool>(1);
    var rlocked = new channel<bool>(1);
    rl = Ꮡwl.RLocker();
    nint n = 10;
    var rlʗ1 = rl;
    var rlockedʗ1 = rlocked;
    var wlockedʗ1 = wlocked;
    goǃ(() => {
        for (nint i = 0; i < n; i++) {
            rlʗ1.Lock();
            rlʗ1.Lock();
            rlockedʗ1.ᐸꟷ(true);
            Ꮡwl.Lock();
            wlockedʗ1.ᐸꟷ(true);
        }
    });
    for (nint i = 0; i < n; i++) {
        ᐸꟷ(rlocked);
        rl.Unlock();
        var selᴛ9 = wlocked;
        switch (trySelect(ᐸꟷ(selᴛ9, ꓸꓸꓸ))) {
        case 0 when selᴛ9.ꟷᐳ(out _): {
            Ꮡt.Fatal(rLockerDidnTReadLockItˢ);
            break;
        }
        default: {
            break;
        }}
        rl.Unlock();
        ᐸꟷ(wlocked);
        var selᴛ10 = rlocked;
        switch (trySelect(ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
        case 0 when selᴛ10.ꟷᐳ(out _): {
            Ꮡt.Fatal(rLockerDidnTRespectTheˢ);
            break;
        }
        default: {
            break;
        }}
        Ꮡwl.Unlock();
    }
}

[GoType("dyn")] internal partial struct BenchmarkRWMutexUncontended_PaddedRWMutex {
    public partial ref sync_package.RWMutex RWMutex { get; }
    internal array<uint32> pad = new(32);
}

public static void BenchmarkRWMutexUncontended(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        ref var rwm = ref heap(new BenchmarkRWMutexUncontended_PaddedRWMutex(), out var Ꮡrwm);
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

internal static void benchmarkRWMutex(ж<Δtesting.B> Ꮡb, nint localWork, nint writeRatio) {
    ref var rwm = ref heap(new Δsync.RWMutex(), out var Ꮡrwm);
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
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

public static void BenchmarkRWMutexWrite100(ж<Δtesting.B> Ꮡb) {
    benchmarkRWMutex(Ꮡb, 0, 100);
}

public static void BenchmarkRWMutexWrite10(ж<Δtesting.B> Ꮡb) {
    benchmarkRWMutex(Ꮡb, 0, 10);
}

public static void BenchmarkRWMutexWorkWrite100(ж<Δtesting.B> Ꮡb) {
    benchmarkRWMutex(Ꮡb, 100, 100);
}

public static void BenchmarkRWMutexWorkWrite10(ж<Δtesting.B> Ꮡb) {
    benchmarkRWMutex(Ꮡb, 100, 10);
}

} // end sync_test_package
