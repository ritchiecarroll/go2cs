// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using Δmath = math_package;
using Δnet = net_package;
using Δruntime = runtime_package;
using Δdebug = global::go.runtime.debug_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = global::go.sync.atomic_package;
using syscall = syscall_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.runtime;
using global::go.sync;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static channel<bool> stop = new channel<bool>(1);

internal static partial void perpetuumMobile() {
    var selᴛ80 = stop;
    switch (trySelect(ᐸꟷ(selᴛ80, ꓸꓸꓸ))) {
    case 0 when selᴛ80.ꟷᐳ(out _): {
        break;
    }
    default: {
        goǃ(perpetuumMobile);
        break;
    }}
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noPreemptionOnWasmYetˢ = (@string)"no preemption on wasm yet"u8;
internal static readonly object skippingDuringShortTestˢ = (@string)"skipping during short test"u8;

public static partial void TestStopTheWorldDeadlock(ж<testing.T> Ꮡt) {
    if (Δruntime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(noPreemptionOnWasmYetˢ);
    }
    if (testing.Short()) {
        Ꮡt.Skip(skippingDuringShortTestˢ);
    }
    nint maxprocs = Δruntime.GOMAXPROCS(3);
    var compl = new channel<bool>(2);
    var complʗ1 = compl;
    goǃ(() => {
        for (nint i = 0; i != 1000; i += 1) {
            Δruntime.GC();
        }
        complʗ1.ᐸꟷ(true);
    });
    var complʗ2 = compl;
    goǃ(() => {
        for (nint i = 0; i != 1000; i += 1) {
            Δruntime.GOMAXPROCS(3);
        }
        complʗ2.ᐸꟷ(true);
    });
    goǃ(perpetuumMobile);
    ᐸꟷ(compl);
    ᐸꟷ(compl);
    stop.ᐸꟷ(true);
    Δruntime.GOMAXPROCS(maxprocs);
}

public static void TestYieldProgress(ж<testing.T> Ꮡt) {
    testYieldProgress(false);
}

public static void TestYieldLockedProgress(ж<testing.T> Ꮡt) {
    testYieldProgress(true);
}

internal static partial void testYieldProgress(bool locked) {
    var c = new channel<bool>(0);
    var cack = new channel<bool>(0);
    var cʗ1 = c;
    var cackʗ1 = cack;
    goǃ(() => {
        if (locked) {
            Δruntime.LockOSThread();
        }
        while (ᐧ) {
            var selᴛ81 = cʗ1;
            switch (trySelect(ᐸꟷ(selᴛ81, ꓸꓸꓸ))) {
            case 0 when selᴛ81.ꟷᐳ(out _): {
                cackʗ1.ᐸꟷ(true);
                return;
            }
            default: {
                Δruntime.Gosched();
                break;
            }}
        }
    });
    time.Sleep(10 * time.Millisecond);
    c.ᐸꟷ(true);
    ᐸꟷ(cack);
}

public static partial void TestYieldLocked(ж<testing.T> Ꮡt) {
    const nint N = 10;
    var c = new channel<bool>(0);
    var cʗ1 = c;
    goǃ(() => {
        Δruntime.LockOSThread();
        for (nint i = 0; i < N; i++) {
            Δruntime.Gosched();
            time.Sleep(time.Millisecond);
        }
        cʗ1.ᐸꟷ(true);
    });
    // runtime.UnlockOSThread() is deliberately omitted
    ᐸꟷ(c);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnUniprocessorˢ = (@string)"skipping on uniprocessor"u8;

public static partial void TestGoroutineParallelism(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (Δruntime.NumCPU() == 1) {
            // Takes too long, too easy to deadlock, etc.
            Ꮡt.Skip(skippingOnUniprocessorˢ);
        }
        nint P = 4;
        nint N = 10;
        if (testing.Short()) {
            P = 3;
            N = 3;
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(P), ref ᒐ);
        // If runtime triggers a forced GC during this test then it will deadlock,
        // since the goroutines can't be stopped/preempted.
        // Disable GC for this test (see issue #10958).
        defer(Δdebug.SetGCPercent, Δdebug.SetGCPercent(-1), ref ᒐ);
        // SetGCPercent waits until the mark phase is over, but the runtime
        // also preempts at the start of the sweep phase, so make sure that's
        // done too. See #45867.
        Δruntime.GC();
        for (nint @try = 0; @try < N; @try++) {
            var done = new channel<bool>(0);
            ref var x = ref heap<uint32>(out var Ꮡx);
            x = (uint32)0;
            for (nint p = 0; p < P; p++) {
                // Test that all P goroutines are scheduled at the same time
                var doneʗ1 = done;
                goǃ((nint pΔ1) => {
                    for (nint i = 0; i < 3; i++) {
                        var expected = (uint32)(P * i + pΔ1);
                        while (atomic.LoadUint32(Ꮡx) != expected) {
                        }
                        atomic.StoreUint32(Ꮡx, expected + 1);
                    }
                    doneʗ1.ᐸꟷ(true);
                }, p);
            }
            for (nint p = 0; p < P; p++) {
                ᐸꟷ(done);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test that all runnable goroutines are scheduled at the same time.
public static void TestGoroutineParallelism2(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    //testGoroutineParallelism2(t, false, false)
    testGoroutineParallelism2(Ꮡt, true, false);
    testGoroutineParallelism2(Ꮡt, false, true);
    testGoroutineParallelism2(Ꮡt, true, true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string localhost0ˢ = "localhost:0"u8;
internal static readonly @string tcpˢ = "tcp"u8;

internal static partial void testGoroutineParallelism2(ж<testing.T> Ꮡt, bool load, bool netpoll) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (Δruntime.NumCPU() == 1) {
            // Takes too long, too easy to deadlock, etc.
            Ꮡt.Skip(skippingOnUniprocessorˢ);
        }
        nint P = 4;
        nint N = 10;
        if (testing.Short()) {
            N = 3;
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(P), ref ᒐ);
        // If runtime triggers a forced GC during this test then it will deadlock,
        // since the goroutines can't be stopped/preempted.
        // Disable GC for this test (see issue #10958).
        defer(Δdebug.SetGCPercent, Δdebug.SetGCPercent(-1), ref ᒐ);
        // SetGCPercent waits until the mark phase is over, but the runtime
        // also preempts at the start of the sweep phase, so make sure that's
        // done too. See #45867.
        Δruntime.GC();
        for (nint @try = 0; @try < N; @try++) {
            if (load) {
                // Create P goroutines and wait until they all run.
                // When we run the actual test below, worker threads
                // running the goroutines will start parking.
                var doneΔ1 = new channel<bool>(0);
                ref var xΔ1 = ref heap<uint32>(out var ᏑxΔ1);
                xΔ1 = (uint32)0;
                for (nint p = 0; p < P; p++) {
                    var doneʗ1 = doneΔ1;
                    goǃ(() => {
                        if (atomic.AddUint32(ᏑxΔ1, 1) == (uint32)P) {
                            doneʗ1.ᐸꟷ(true);
                            return;
                        }
                        while (atomic.LoadUint32(ᏑxΔ1) != (uint32)P) {
                        }
                    });
                }
                ᐸꟷ(doneΔ1);
            }
            if (netpoll) {
                // Enable netpoller, affects schedler behavior.
                @string laddr = localhost0ˢ;
                if (Δruntime.GOOS == "android"u8) {
                    // On some Android devices, there are no records for localhost,
                    // see https://golang.org/issues/14486.
                    // Don't use 127.0.0.1 for every case, it won't work on IPv6-only systems.
                    laddr = "127.0.0.1:0"u8;
                }
                var (ln, err) = Δnet.Listen(tcpˢ, laddr);
                if (err == default!) {
                    var lnʗ1 = ln;
                    defer(() => lnʗ1.Close(), ref ᒐ); // yup, defer in a loop
                }
            }
            var done = new channel<bool>(0);
            ref var x = ref heap<uint32>(out var Ꮡx);
            x = (uint32)0;
            // Spawn P goroutines in a nested fashion just to differ from TestGoroutineParallelism.
            for (nint p = 0; p < P / 2; p++) {
                var doneʗ2 = done;
                goǃ([MethodImpl(MethodImplOptions.NoInlining)] (nint pΔ1) => {
                    for (nint p2 = 0; p2 < 2; p2++) {
                        var doneʗ3 = doneʗ2;
                        goǃ((nint p2Δ1) => {
                            for (nint i = 0; i < 3; i++) {
                                var expected = (uint32)(P * i + pΔ1 * 2 + p2Δ1);
                                while (atomic.LoadUint32(Ꮡx) != expected) {
                                }
                                atomic.StoreUint32(Ꮡx, expected + 1);
                            }
                            doneʗ3.ᐸꟷ(true);
                        }, p2);
                    }
                }, p);
            }
            for (nint p = 0; p < P; p++) {
                ᐸꟷ(done);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestBlockLocked(ж<testing.T> Ꮡt) {
    const nint N = 10;
    var c = new channel<bool>(0);
    var cʗ1 = c;
    goǃ(() => {
        Δruntime.LockOSThread();
        for (nint i = 0; i < N; i++) {
            cʗ1.ᐸꟷ(true);
        }
        Δruntime.UnlockOSThread();
    });
    for (nint i = 0; i < N; i++) {
        ᐸꟷ(c);
    }
}

public static partial void TestTimerFairness(ж<testing.T> Ꮡt) {
    if (Δruntime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(noPreemptionOnWasmYetˢ);
    }
    var done = new channel<bool>(0);
    var c = new channel<bool>(0);
    for (nint i = 0; i < 2; i++) {
        var cʗ1 = c;
        var doneʗ1 = done;
        goǃ(() => {
            while (ᐧ) {
                var selᴛ82 = cʗ1.ᐸꟷ(true, ꓸꓸꓸ);
                var selᴛ83 = doneʗ1;
                switch (select(selᴛ82, ᐸꟷ(selᴛ83, ꓸꓸꓸ))) {
                case 0: {
                    break;
                }
                case 1 when selᴛ83.ꟷᐳ(out _): {
                    return;
                }}
            }
        });
    }
    var timer = time.After(20 * time.Millisecond);
    while (ᐧ) {
        var selᴛ84 = c;
        var selᴛ85 = timer;
        switch (select(ᐸꟷ(selᴛ84, ꓸꓸꓸ), ᐸꟷ(selᴛ85, ꓸꓸꓸ))) {
        case 0 when selᴛ84.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ85.ꟷᐳ(out _): {
            close(done);
            return;
        }}
    }
}

public static partial void TestTimerFairness2(ж<testing.T> Ꮡt) {
    if (Δruntime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(noPreemptionOnWasmYetˢ);
    }
    var done = new channel<bool>(0);
    var c = new channel<bool>(0);
    for (nint i = 0; i < 2; i++) {
        var cʗ1 = c;
        var doneʗ1 = done;
        goǃ(() => {
            var timer = time.After(20 * time.Millisecond);
            ref var buf = ref heap(new array<byte>(1), out var Ꮡbuf);
            while (ᐧ) {
                syscall.Read(0, buf[0..0]);
                var selᴛ86 = cʗ1.ᐸꟷ(true, ꓸꓸꓸ);
                var selᴛ87 = cʗ1;
                var selᴛ88 = timer;
                switch (select(selᴛ86, ᐸꟷ(selᴛ87, ꓸꓸꓸ), ᐸꟷ(selᴛ88, ꓸꓸꓸ))) {
                case 0: {
                    break;
                }
                case 1 when selᴛ87.ꟷᐳ(out _): {
                    break;
                }
                case 2 when selᴛ88.ꟷᐳ(out _): {
                    doneʗ1.ᐸꟷ(true);
                    return;
                }}
            }
        });
    }
    ᐸꟷ(done);
    ᐸꟷ(done);
}

// The function is used to test preemption at split stack checks.
// Declaring a var avoids inlining at the call site.
internal static Func<nint> preempt = () => {
    array<nint> a = new(128);
    nint sum = 0;
    foreach (var (_, v) in a.ΔRangeSnapshot()) {
        sum += v;
    }
    return sum;
};

public static partial void TestPreemption(ж<testing.T> Ꮡt) {
    if (Δruntime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(noPreemptionOnWasmYetˢ);
    }
    // Test that goroutines are preempted at function calls.
    nint N = 5;
    if (testing.Short()) {
        N = 2;
    }
    var c = new channel<bool>(0);
    ref var x = ref heap(new uint32(), out var Ꮡx);
    for (nint g = 0; g < 2; g++) {
        var cʗ1 = c;
        goǃ((nint gΔ1) => {
            for (nint i = 0; i < N; i++) {
                while (atomic.LoadUint32(Ꮡx) != (uint32)gΔ1) {
                    preempt();
                }
                atomic.StoreUint32(Ꮡx, (uint32)(1 - gΔ1));
            }
            cʗ1.ᐸꟷ(true);
        }, g);
    }
    ᐸꟷ(c);
    ᐸꟷ(c);
}

public static partial void TestPreemptionGC(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOARCH == "wasm"u8) {
            Ꮡt.Skip(noPreemptionOnWasmYetˢ);
        }
        // Test that pending GC preempts running goroutines.
        nint P = 5;
        nint N = 10;
        if (testing.Short()) {
            P = 3;
            N = 2;
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(P + 1), ref ᒐ);
        ref var stop = ref heap(new uint32(), out var Ꮡstop);
        for (nint i = 0; i < P; i++) {
            goǃ(() => {
                while (atomic.LoadUint32(Ꮡstop) == 0) {
                    preempt();
                }
            });
        }
        for (nint i = 0; i < N; i++) {
            Δruntime.Gosched();
            Δruntime.GC();
        }
        atomic.StoreUint32(Ꮡstop, 1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object asynchronousPreemptionˢ = (@string)"asynchronous preemption not supported on this platform"u8;
internal static readonly @string asyncPreemptˢ = "AsyncPreempt"u8;

public static void TestAsyncPreempt(ж<testing.T> Ꮡt) {
    if (!runtime_internal_test_package.PreemptMSupported) {
        Ꮡt.Skip(asynchronousPreemptionˢ);
    }
    @string output = runTestProg(Ꮡt, testprogˢ, asyncPreemptˢ);
    @string want = "OK\n"u8;
    if (output != want) {
        Ꮡt.Fatalf("want %s, got %s\n"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcFairnessˢ = "GCFairness"u8;

public static void TestGCFairness(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, gcFairnessˢ);
    @string want = "OK\n"u8;
    if (output != want) {
        Ꮡt.Fatalf("want %s, got %s\n"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcFairness2ˢ = "GCFairness2"u8;

public static void TestGCFairness2(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, gcFairness2ˢ);
    @string want = "OK\n"u8;
    if (output != want) {
        Ꮡt.Fatalf("want %s, got %s\n"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string numGoroutineˢ = "NumGoroutine"u8;
internal static readonly @string inGoroutineˢ = "in goroutine"u8;
internal static readonly @string goroutineˢ = "goroutine "u8;

public static partial void TestNumGoroutine(ж<testing.T> Ꮡt) {
    @string output = runTestProg(Ꮡt, testprogˢ, numGoroutineˢ);
    @string want = "1\n"u8;
    if (output != want) {
        Ꮡt.Fatalf("want %q, got %q"u8, want, output);
    }
    var buf = new slice<byte>((1 << (int)(20)));
    // Try up to 10 times for a match before giving up.
    // This is a fundamentally racy check but it's important
    // to notice if NumGoroutine and Stack are _always_ out of sync.
    for (nint i = 0; ᐧ ; i++) {
        // Give goroutines about to exit a chance to exit.
        // The NumGoroutine and Stack below need to see
        // the same state of the world, so anything we can do
        // to keep it quiet is good.
        Δruntime.Gosched();
        nint n = Δruntime.NumGoroutine();
        buf = buf.slice(0, Δruntime.Stack(buf, true));
        // To avoid double-counting "goroutine" in "goroutine $m [running]:"
        // and "created by $func in goroutine $n", remove the latter
        @string outputΔ1 = strings.ReplaceAll(((@string)buf), inGoroutineˢ, ""u8);
        nint nstk = strings.Count(outputΔ1, goroutineˢ);
        if (n == nstk) {
            break;
        }
        if (i >= 10) {
            Ꮡt.Fatalf("NumGoroutine=%d, but found %d goroutines in stack dump: %s"u8, n, nstk, buf);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ2 = (@string)"skipping in -short mode"u8;
internal static readonly object skippingInRaceModeˢ = (@string)"skipping in -race mode"u8;

public static partial void TestPingPongHog(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (Δruntime.GOARCH == "wasm"u8) {
            Ꮡt.Skip(noPreemptionOnWasmYetˢ);
        }
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ2);
        }
        if (race.Enabled) {
            // The race detector randomizes the scheduler,
            // which causes this test to fail (#38266).
            Ꮡt.Skip(skippingInRaceModeˢ);
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(1), ref ᒐ);
        var done = new channel<bool>(0);
        var (hogChan, lightChan) = (new channel<bool>(0), new channel<bool>(0));
        ref var hogCount = ref heap<nint>(out var ᏑhogCount);
        hogCount = 0;
        ref var lightCount = ref heap<nint>(out var ᏑlightCount);
        lightCount = 0;
        var doneʗ1 = done;
        void run(nint limit, ж<nint> counter, channel<bool> wake) {
            while (ᐧ) {
                var selᴛ89 = doneʗ1;
                var selᴛ90 = wake;
                switch (select(ᐸꟷ(selᴛ89, ꓸꓸꓸ), ᐸꟷ(selᴛ90, ꓸꓸꓸ))) {
                case 0 when selᴛ89.ꟷᐳ(out _): {
                    return;
                }
                case 1 when selᴛ90.ꟷᐳ(out _): {
                    for (nint i = 0; i < limit; i++) {
                        counter.Value++;
                    }
                    wake.ᐸꟷ(true);
                    break;
                }}
            }
        }
        // Start two co-scheduled hog goroutines.
        for (nint i = 0; i < 2; i++) {
            var runʗ1 = run;
            goǃ(runʗ1, (nint)(1000000), ᏑhogCount, hogChan);
        }
        // Start two co-scheduled light goroutines.
        for (nint i = 0; i < 2; i++) {
            var runʗ2 = run;
            goǃ(runʗ2, (nint)(1000), ᏑlightCount, lightChan);
        }
        // Start goroutine pairs and wait for a few preemption rounds.
        hogChan.ᐸꟷ(true);
        lightChan.ᐸꟷ(true);
        time.Sleep(100 * time.Millisecond);
        close(done);
        ᐸꟷ(hogChan);
        ᐸꟷ(lightChan);
        // Check that hogCount and lightCount are within a factor of
        // 20, which indicates that both pairs of goroutines handed off
        // the P within a time-slice to their buddy. We can use a
        // fairly large factor here to make this robust: if the
        // scheduler isn't working right, the gap should be ~1000X
        // (was 5, increased to 20, see issue 52207).
        UntypedInt factor = 20;
        if (hogCount / (nint)factor > lightCount || lightCount / (nint)factor > hogCount) {
            Ꮡt.Fatalf("want hogCount/lightCount in [%v, %v]; got %d/%d = %g"u8, (float64)(/* 1.0 / factor */ 0.05D), (nint)(factor), hogCount, lightCount, (float64)hogCount / (float64)lightCount);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void BenchmarkPingPongHog(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        if (b.N == 0) {
            return;
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(1), ref ᒐ);
        // Create a CPU hog
        var (stop, done) = (new channel<bool>(0), new channel<bool>(0));
        var doneʗ1 = done;
        var stopʗ1 = stop;
        goǃ(() => {
            while (ᐧ) {
                var selᴛ91 = stopʗ1;
                switch (trySelect(ᐸꟷ(selᴛ91, ꓸꓸꓸ))) {
                case 0 when selᴛ91.ꟷᐳ(out _): {
                    doneʗ1.ᐸꟷ(true);
                    return;
                }
                default: {
                    break;
                }}
            }
        });
        // Ping-pong b.N times
        var (ping, pong) = (new channel<bool>(0), new channel<bool>(0));
        var doneʗ2 = done;
        var pingʗ1 = ping;
        var pongʗ1 = pong;
        var stopʗ2 = stop;
        goǃ(() => {
            for (nint j = 0; j < Ꮡb.Value.N; j++) {
                pongʗ1.ᐸꟷ(ᐸꟷ(pingʗ1));
            }
            close(stopʗ2);
            doneʗ2.ᐸꟷ(true);
        });
        var doneʗ3 = done;
        var pingʗ2 = ping;
        var pongʗ2 = pong;
        goǃ(() => {
            for (nint i = 0; i < Ꮡb.Value.N; i++) {
                pingʗ2.ᐸꟷ(ᐸꟷ(pongʗ2));
            }
            doneʗ3.ᐸꟷ(true);
        });
        b.ResetTimer();
        ping.ᐸꟷ(true); // Start ping-pong
        ᐸꟷ(stop);
        b.StopTimer();
        ᐸꟷ(ping); // Let last ponger exit
        ᐸꟷ(done); // Make sure goroutines exit
        ᐸꟷ(done);
        ᐸꟷ(done);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static array<uint64> padData = new(128);

internal static void stackGrowthRecursive(nint i) {
    array<uint64> pad = new(128);
    pad = padData.Clone();
    foreach (var (j, _) in pad) {
        if (pad[j] != 0) {
            return;
        }
    }
    if (i != 0) {
        stackGrowthRecursive(i - 1);
    }
}

public static partial void TestPreemptSplitBig(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ2);
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(2), ref ᒐ);
        var stop = new channel<nint>(0);
        goǃ(ᴛ1 => big(ᴛ1), stop);
        for (nint i = 0; i < 3; i++) {
            time.Sleep(10 * time.Microsecond); // let big start running
            Δruntime.GC();
        }
        close(stop);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint big(channel<nint> stop) {
    nint n = 0;
    while (ᐧ) {
        // delay so that gc is sure to have asked for a preemption
        for (nint i = 0; i < 1000000000; i++) {
            n++;
        }
        // call bigframe, which used to miss the preemption in its prologue.
        bigframe(stop);
        // check if we've been asked to stop.
        var selᴛ92 = stop;
        switch (select(ᐸꟷ(selᴛ92, ꓸꓸꓸ))) {
        case 0 when selᴛ92.ꟷᐳ(out _): {
            return n;
        }}
        return default!;
    }
}

internal static nint bigframe(channel<nint> stop) {
    // not splitting the stack will overflow.
    // small will notice that it needs a stack split and will
    // catch the overflow.
    ref var x = ref heap(new array<byte>(8192), out var Ꮡx);
    return small(stop, Ꮡx);
}

internal static nint small(channel<nint> stop, /*[8192]*/ ж<array<byte>> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    foreach (var (i, _) in x) {
        x[i] = (byte)i;
    }
    nint sum = 0;
    foreach (var (i, _) in x) {
        sum += (nint)x[i];
    }
    // keep small from being a leaf function, which might
    // make it not do any stack check at all.
    nonleaf(stop);
    return sum;
}

internal static bool nonleaf(channel<nint> stop) {
    // do something that won't be inlined:
    var selᴛ93 = stop;
    switch (trySelect(ᐸꟷ(selᴛ93, ꓸꓸꓸ))) {
    case 0 when selᴛ93.ꟷᐳ(out _): {
        return true;
    }
    default: {
        return false;
    }}
}

public static void TestSchedLocalQueue(ж<testing.T> Ꮡt) {
    runtime_internal_test_package.RunSchedLocalQueueTest();
}

public static void TestSchedLocalQueueSteal(ж<testing.T> Ꮡt) {
    runtime_internal_test_package.RunSchedLocalQueueStealTest();
}

public static void TestSchedLocalQueueEmpty(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (Δruntime.NumCPU() == 1) {
            // Takes too long and does not trigger the race.
            Ꮡt.Skip(skippingOnUniprocessorˢ);
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(4), ref ᒐ);
        // If runtime triggers a forced GC during this test then it will deadlock,
        // since the goroutines can't be stopped/preempted during spin wait.
        defer(Δdebug.SetGCPercent, Δdebug.SetGCPercent(-1), ref ᒐ);
        // SetGCPercent waits until the mark phase is over, but the runtime
        // also preempts at the start of the sweep phase, so make sure that's
        // done too. See #45867.
        Δruntime.GC();
        nint iters = (nint)100000;
        if (testing.Short()) {
            iters = 100;
        }
        runtime_internal_test_package.RunSchedLocalQueueEmptyTest(iters);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void benchmarkStackGrowth(ж<testing.B> Ꮡb, nint rec) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            stackGrowthRecursive(rec);
        }
    });
}

public static void BenchmarkStackGrowth(ж<testing.B> Ꮡb) {
    benchmarkStackGrowth(Ꮡb, 10);
}

public static void BenchmarkStackGrowthDeep(ж<testing.B> Ꮡb) {
    benchmarkStackGrowth(Ꮡb, 1024);
}

public static void BenchmarkCreateGoroutines(ж<testing.B> Ꮡb) {
    benchmarkCreateGoroutines(Ꮡb, 1);
}

public static void BenchmarkCreateGoroutinesParallel(ж<testing.B> Ꮡb) {
    benchmarkCreateGoroutines(Ꮡb, Δruntime.GOMAXPROCS(-1));
}

internal static partial void benchmarkCreateGoroutines(ж<testing.B> Ꮡb, nint procs) {
    ref var b = ref Ꮡb.DerefOrNull();

    var c = new channel<bool>(0);
    ref var f = ref heap<Action<nint>>(out var Ꮡf);
    var cʗ1 = c;
    f = [MethodImpl(MethodImplOptions.NoInlining)] (nint n) => {
        if (n == 0) {
            cʗ1.ᐸꟷ(true);
            return;
        }
        goǃ(Ꮡf.ValueSlot, n - 1);
    };
    for (nint i = 0; i < procs; i++) {
        goǃ(Ꮡf.ValueSlot, quo(b.N, procs));
    }
    for (nint i = 0; i < procs; i++) {
        ᐸꟷ(c);
    }
}

public static partial void BenchmarkCreateGoroutinesCapture(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    for (nint i = 0; i < b.N; i++) {
        const nint N = 4;
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(N);
        for (nint iΔ1 = 0; iΔ1 < N; iΔ1++) {
            nint iΔ2 = iΔ1;
            goǃ(() => {
                if (iΔ2 >= N) {
                    Ꮡb.Logf("bad"u8); // just to capture b
                }
                Ꮡwg.Done();
            });
        }
        Ꮡwg.Wait();
    }
}

// warmupScheduler ensures the scheduler has at least targetThreadCount threads
// in its thread pool.
internal static partial void warmupScheduler(nint targetThreadCount) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    ref var count = ref heap(new int32(), out var Ꮡcount);
    for (nint i = 0; i < targetThreadCount; i++) {
        Ꮡwg.Add(1);
        goǃ(() => {
            atomic.AddInt32(Ꮡcount, 1);
            while (atomic.LoadInt32(Ꮡcount) < (int32)targetThreadCount) {
            }
            // spin until all threads started
            // spin a bit more to ensure they are all running on separate CPUs.
            doWork(time.Millisecond);
            Ꮡwg.Done();
        });
    }
    Ꮡwg.Wait();
}

internal static void doWork(time.Duration dur) {
    var start = time.Now();
    while (time.Since(start) < dur) {
    }
}

// BenchmarkCreateGoroutinesSingle creates many goroutines, all from a single
// producer (the main benchmark goroutine).
//
// Compared to BenchmarkCreateGoroutines, this causes different behavior in the
// scheduler because Ms are much more likely to need to steal work from the
// main P rather than having work in the local run queue.
public static partial void BenchmarkCreateGoroutinesSingle(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Since we are interested in stealing behavior, warm the scheduler to
    // get all the Ps running first.
    warmupScheduler(Δruntime.GOMAXPROCS(0));
    b.ResetTimer();
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Add(b.N);
    for (nint i = 0; i < b.N; i++) {
        goǃ(() => {
            Ꮡwg.Done();
        });
    }
    Ꮡwg.Wait();
}

public static void BenchmarkClosureCall(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint sum = 0;
    nint off1 = 1;
    for (nint iᴛ1 = 0; iᴛ1 < b.N; iᴛ1++) {
        var i = iᴛ1;
        nint off2 = 2;
        ((Action)(() => {
            sum += i + off1 + off2;
        }))();
    }
    _ = sum;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingGomaxprocs1ˢ = (@string)"skipping: GOMAXPROCS=1"u8;

internal static void benchmarkWakeupParallel(ж<testing.B> Ꮡb, Action<time.Duration> spin) {
    if (Δruntime.GOMAXPROCS(0) == 1) {
        Ꮡb.Skip(skippingGomaxprocs1ˢ);
    }
    var wakeDelay = 5 * time.Microsecond;
    foreach (var (_, delay) in new time.Duration[]{
        0,
        1 * time.Microsecond,
        2 * time.Microsecond,
        5 * time.Microsecond,
        10 * time.Microsecond,
        20 * time.Microsecond,
        50 * time.Microsecond,
        100 * time.Microsecond
    }.slice()) {
        Ꮡb.Run(delay.String(), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ1) => {
            if ((~bΔ1).N == 0) {
                return;
            }
            // Start two goroutines, which alternate between being
            // sender and receiver in the following protocol:
            //
            // - The receiver spins for `delay` and then does a
            // blocking receive on a channel.
            //
            // - The sender spins for `delay+wakeDelay` and then
            // sends to the same channel. (The addition of
            // `wakeDelay` improves the probability that the
            // receiver will be blocking when the send occurs when
            // the goroutines execute in parallel.)
            //
            // In each iteration of the benchmark, each goroutine
            // acts once as sender and once as receiver, so each
            // goroutine spins for delay twice.
            //
            // BenchmarkWakeupParallel is used to estimate how
            // efficiently the scheduler parallelizes goroutines in
            // the presence of blocking:
            //
            // - If both goroutines are executed on the same core,
            // an increase in delay by N will increase the time per
            // iteration by 4*N, because all 4 delays are
            // serialized.
            //
            // - Otherwise, an increase in delay by N will increase
            // the time per iteration by 2*N, and the time per
            // iteration is 2 * (runtime overhead + chan
            // send/receive pair + delay + wakeDelay). This allows
            // the runtime overhead, including the time it takes
            // for the unblocked goroutine to be scheduled, to be
            // estimated.
            var (ping, pong) = (new channel<EmptyStruct>(0), new channel<EmptyStruct>(0));
            var start = new channel<EmptyStruct>(0);
            var done = new channel<EmptyStruct>(0);
            var doneʗ1 = done;
            var pingʗ1 = ping;
            var pongʗ1 = pong;
            var startʗ1 = start;
            goǃ(() => {
                ᐸꟷ(startʗ1);
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    // sender
                    spin(delay + wakeDelay);
                    pingʗ1.ᐸꟷ(new EmptyStruct());
                    // receiver
                    spin(delay);
                    ᐸꟷ(pongʗ1);
                }
                doneʗ1.ᐸꟷ(new EmptyStruct());
            });
            var doneʗ2 = done;
            var pingʗ2 = ping;
            var pongʗ2 = pong;
            goǃ(() => {
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    // receiver
                    spin(delay);
                    ᐸꟷ(pingʗ2);
                    // sender
                    spin(delay + wakeDelay);
                    pongʗ2.ᐸꟷ(new EmptyStruct());
                }
                doneʗ2.ᐸꟷ(new EmptyStruct());
            });
            bΔ1.ResetTimer();
            start.ᐸꟷ(new EmptyStruct());
            ᐸꟷ(done);
            ᐸꟷ(done);
        });
    }
}

public static void BenchmarkWakeupParallelSpinning(ж<testing.B> Ꮡb) {
    benchmarkWakeupParallel(Ꮡb, (time.Duration d) => {
        var end = time.Now().Add(d);
        while (time.Now().Before(end)) {
        }
    });
}

// do nothing

// sysNanosleep is defined by OS-specific files (such as runtime_linux_test.go)
// to sleep for the given duration. If nil, dependent tests are skipped.
// The implementation should invoke a blocking system call and not
// call time.Sleep, which would deschedule the goroutine.
internal static Action<time.Duration> sysNanosleep;

public static void BenchmarkWakeupParallelSyscall(ж<testing.B> Ꮡb) {
    if (sysNanosleep == default!) {
        Ꮡb.Skipf("skipping on %v; sysNanosleep not defined"u8, Δruntime.GOOS);
    }
    benchmarkWakeupParallel(Ꮡb, (time.Duration d) => {
        sysNanosleep(d);
    });
}

partial struct Matrix /*[]slice<float64>*/;

public static void BenchmarkMatmult(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    // matmult is O(N**3) but testing expects O(b.N),
    // so we need to take cube root of b.N
    nint n = (nint)Δmath.Cbrt((float64)b.N) + 1;
    var A = makeMatrix(n);
    var B = makeMatrix(n);
    var C = makeMatrix(n);
    b.StartTimer();
    matmult(default!, A, B, C, 0, n, 0, n, 0, n, 8);
}

internal static Matrix makeMatrix(nint n) {
    var m = new Matrix(n);
    for (nint i = 0; i < n; i++) {
        m[i] = new slice<float64>(n);
        for (nint j = 0; j < n; j++) {
            m[i][j] = (float64)(i * n + j);
        }
    }
    return m;
}

internal static partial void matmult(channel/*<-*/<EmptyStruct> done, Matrix A, Matrix B, Matrix C, nint i0, nint i1, nint j0, nint j1, nint k0, nint k1, nint threshold) {
    nint di = i1 - i0;
    nint dj = j1 - j0;
    nint dk = k1 - k0;
    if (di >= dj && di >= dk && di >= threshold){
        // divide in two by y axis
        nint mi = i0 + di / 2;
        var done1 = new channel<EmptyStruct>(1);
        goǃ(matmult, done1.WithDirection(GoChanDir.Send), A, B, C, i0, mi, j0, j1, k0, k1, threshold);
        matmult(default!, A, B, C, mi, i1, j0, j1, k0, k1, threshold);
        ᐸꟷ(done1);
    } else 
    if (dj >= dk && dj >= threshold){
        // divide in two by x axis
        nint mj = j0 + dj / 2;
        var done1 = new channel<EmptyStruct>(1);
        goǃ(matmult, done1.WithDirection(GoChanDir.Send), A, B, C, i0, i1, j0, mj, k0, k1, threshold);
        matmult(default!, A, B, C, i0, i1, mj, j1, k0, k1, threshold);
        ᐸꟷ(done1);
    } else 
    if (dk >= threshold){
        // divide in two by "k" axis
        // deliberately not parallel because of data races
        nint mk = k0 + dk / 2;
        matmult(default!, A, B, C, i0, i1, j0, j1, k0, mk, threshold);
        matmult(default!, A, B, C, i0, i1, j0, j1, mk, k1, threshold);
    } else {
        // the matrices are small enough, compute directly
        for (nint i = i0; i < i1; i++) {
            for (nint j = j0; j < j1; j++) {
                for (nint k = k0; k < k1; k++) {
                    C[i][j] += A[i][k] * B[k][j];
                }
            }
        }
    }
    if (done != default!) {
        done.ᐸꟷ(new EmptyStruct());
    }
}

public static void TestStealOrder(ж<testing.T> Ꮡt) {
    runtime_internal_test_package.RunStealOrderTest();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noThreadsOnWasmYetˢ = (@string)"no threads on wasm yet"u8;

public static partial void TestLockOSThreadNesting(ж<testing.T> Ꮡt) {
    if (Δruntime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(noThreadsOnWasmYetˢ);
    }
    goǃ(() => {
        var (e, i) = runtime_internal_test_package.LockOSCounts();
        if (e != 0 || i != 0) {
            Ꮡt.Errorf("want locked counts 0, 0; got %d, %d"u8, e, i);
            return;
        }
        Δruntime.LockOSThread();
        Δruntime.LockOSThread();
        Δruntime.UnlockOSThread();
        (e, i) = runtime_internal_test_package.LockOSCounts();
        if (e != 1 || i != 0) {
            Ꮡt.Errorf("want locked counts 1, 0; got %d, %d"u8, e, i);
            return;
        }
        Δruntime.UnlockOSThread();
        (e, i) = runtime_internal_test_package.LockOSCounts();
        if (e != 0 || i != 0) {
            Ꮡt.Errorf("want locked counts 0, 0; got %d, %d"u8, e, i);
            return;
        }
    });
}

public static void TestLockOSThreadExit(ж<testing.T> Ꮡt) {
    testLockOSThreadExit(Ꮡt, testprogˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string lockOSThreadMainˢ = "LockOSThreadMain"u8;
internal static readonly @string gomaxprocs1ˢ = "GOMAXPROCS=1"u8;
internal static readonly @string lockOSThreadAltˢ = "LockOSThreadAlt"u8;

internal static void testLockOSThreadExit(ж<testing.T> Ꮡt, @string prog) {
    @string output = runTestProg(Ꮡt, prog, lockOSThreadMainˢ, gomaxprocs1ˢ);
    @string want = "OK\n"u8;
    if (output != want) {
        Ꮡt.Errorf("want %q, got %q"u8, want, output);
    }
    output = runTestProg(Ꮡt, prog, lockOSThreadAltˢ);
    if (output != want) {
        Ꮡt.Errorf("want %q, got %q"u8, want, output);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string unshareNotPermittedˢ = "unshare not permitted\n"u8;
internal static readonly object unshareSyscallNotˢ = (@string)"unshare syscall not permitted on this system"u8;

public static void TestLockOSThreadAvoidsStatePropagation(ж<testing.T> Ꮡt) {
    @string want = "OK\n"u8;
    @string skip = unshareNotPermittedˢ;
    @string output = runTestProg(Ꮡt, testprogˢ, "LockOSThreadAvoidsStatePropagation"u8, gomaxprocs1ˢ);
    if (output == skip){
        Ꮡt.Skip(unshareSyscallNotˢ);
    } else 
    if (output != want) {
        Ꮡt.Errorf("want %q, got %q"u8, want, output);
    }
}

public static void TestLockOSThreadTemplateThreadRace(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(Ꮡt));
    var (exe, err) = buildTestProg(Ꮡt, testprogˢ);
    if (err != default!) {
        Ꮡt.Fatal(err);
    }
    nint iterations = 100;
    if (testing.Short()) {
        // Reduce run time to ~100ms, with much lower probability of
        // catching issues.
        iterations = 5;
    }
    for (nint i = 0; i < iterations; i++) {
        @string want = "OK\n"u8;
        @string output = runBuiltTestProg(Ꮡt, exe, "LockOSThreadTemplateThreadRace"u8);
        if (output != want) {
            Ꮡt.Fatalf("run %d: want %q, got %q"u8, i, want, output);
        }
    }
}

// fakeSyscall emulates a system call.
//
//go:nosplit
internal static void fakeSyscall(time.Duration duration) {
    runtime_internal_test_package.Entersyscall();
    for (var start = runtime_internal_test_package.Nanotime(); runtime_internal_test_package.Nanotime() - start < (int64)duration; ) {
    }
    runtime_internal_test_package.Exitsyscall();
}

// Check that a goroutine will be preempted if it is calling short system calls.
internal static partial void testPreemptionAfterSyscall(ж<testing.T> Ꮡt, time.Duration syscallDuration) {
    GoFrame ᒐ = default;
    try {
        if (Δruntime.GOARCH == "wasm"u8) {
            Ꮡt.Skip(noPreemptionOnWasmYetˢ);
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(2), ref ᒐ);
        nint iterations = 10;
        if (testing.Short()) {
            iterations = 1;
        }
        time.Duration maxDuration = /* 5 * time.Second */ 5000000000;
        const nint nroutines = 8;
        for (nint i = 0; i < iterations; i++) {
            var c = new channel<bool>(nroutines);
            ref var stop = ref heap<uint32>(out var Ꮡstop);
            stop = (uint32)0;
            var start = time.Now();
            for (nint g = 0; g < nroutines; g++) {
                var cʗ1 = c;
                goǃ((ж<uint32> stopΔ1) => {
                    cʗ1.ᐸꟷ(true);
                    while (atomic.LoadUint32(stopΔ1) == 0) {
                        fakeSyscall(syscallDuration);
                    }
                    cʗ1.ᐸꟷ(true);
                }, Ꮡstop);
            }
            // wait until all goroutines have started.
            for (nint g = 0; g < nroutines; g++) {
                ᐸꟷ(c);
            }
            atomic.StoreUint32(Ꮡstop, 1);
            // wait until all goroutines have finished.
            for (nint g = 0; g < nroutines; g++) {
                ᐸꟷ(c);
            }
            var duration = time.Since(start);
            if (duration > maxDuration) {
                Ꮡt.Errorf("timeout exceeded: %v (%v)"u8, duration, maxDuration);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPreemptionAfterSyscall(ж<testing.T> Ꮡt) {
    if (Δruntime.GOOS == "plan9"u8) {
        testenv.SkipFlaky(new runtime_test_package.testing_TжTB(Ꮡt), 41015);
    }
    foreach (var (_, i) in new time.Duration[]{10, 100, 1000}.slice()) {
        var d = i * time.Microsecond;
        Ꮡt.Run(fmt.Sprint(d), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ1) => {
            testPreemptionAfterSyscall(tΔ1, d);
        });
    }
}

public static void TestGetgThreadSwitch(ж<testing.T> Ꮡt) {
    runtime_internal_test_package.RunGetgThreadSwitchTest();
}

// TestNetpollBreak tests that netpollBreak can break a netpoll.
// This test is not particularly safe since the call to netpoll
// will pick up any stray files that are ready, but it should work
// OK as long it is not run in parallel.
public static partial void TestNetpollBreak(ж<testing.T> Ꮡt) {
    if (Δruntime.GOMAXPROCS(0) == 1) {
        Ꮡt.Skip(skippingGomaxprocs1ˢ);
    }
    // Make sure that netpoll is initialized.
    runtime_internal_test_package.NetpollGenericInit();
    var start = time.Now();
    var c = new channel<bool>(2);
    var cʗ1 = c;
    goǃ(() => {
        cʗ1.ᐸꟷ(true);
        runtime_internal_test_package.Netpoll(10 * time.ΔSecond.Nanoseconds());
        cʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(c);
    // Loop because the break might get eaten by the scheduler.
    // Break twice to break both the netpoll we started and the
    // scheduler netpoll.
loop:
    while (ᐧ) {
        runtime_internal_test_package.Usleep(100);
        runtime_internal_test_package.NetpollBreak();
        runtime_internal_test_package.NetpollBreak();
        var selᴛ94 = c;
        switch (trySelect(ᐸꟷ(selᴛ94, ꓸꓸꓸ))) {
        case 0 when selᴛ94.ꟷᐳ(out _): {
            goto break_loop;
            break;
        }
        default: {
            break;
        }}
continue_loop:;
    }
break_loop:;
    {
        var dur = time.Since(start); if (dur > (time.Duration)(5000000000L)) {
            Ꮡt.Errorf("netpollBreak did not interrupt netpoll: slept for: %v"u8, dur);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nonexistentTestˢ = "NonexistentTest"u8;
internal static readonly @string gomaxprocs1024ˢ = "GOMAXPROCS=1024"u8;
internal static readonly @string unknownFunctionˢ = "unknown function: NonexistentTest"u8;

// TestBigGOMAXPROCS tests that setting GOMAXPROCS to a large value
// doesn't cause a crash at startup. See issue 38474.
public static void TestBigGOMAXPROCS(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    @string output = runTestProg(Ꮡt, testprogˢ, nonexistentTestˢ, gomaxprocs1024ˢ);
    // Ignore error conditions on small machines.
    foreach (var (_, errstr) in new @string[]{
        "failed to create new OS thread"u8,
        "cannot allocate memory"u8
    }.slice()) {
        if (strings.Contains(output, errstr)) {
            Ꮡt.Skipf("failed to create 1024 threads"u8);
        }
    }
    if (!strings.Contains(output, unknownFunctionˢ)) {
        Ꮡt.Errorf("output:\n%s\nwanted:\nunknown function: NonexistentTest"u8, output);
    }
}

} // end runtime_test_package
