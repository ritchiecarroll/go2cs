// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static sync_package;
using atomic = go.sync.atomic_package;
using Δtesting = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.sync;
using static go.sync_internal_test_package;
using Δsync = sync_package;

partial class sync_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object waitGroupReleasedGroupˢ = (@string)"WaitGroup released group too soon"u8;

[MethodImpl(MethodImplOptions.NoInlining)] internal static void testWaitGroup(ж<Δtesting.T> Ꮡt, ж<Δsync.WaitGroup> Ꮡwg1, ж<Δsync.WaitGroup> Ꮡwg2) {
    nint n = 16;
    Ꮡwg1.Add(n);
    Ꮡwg2.Add(n);
    var exited = new channel<bool>(n);
    for (nint i = 0; i != n; i++) {
        var exitedʗ1 = exited;
        goǃ(() => {
            Ꮡwg1.Done();
            Ꮡwg2.Wait();
            exitedʗ1.ᐸꟷ(true);
        });
    }
    Ꮡwg1.Wait();
    for (nint i = 0; i != n; i++) {
        var selᴛ11 = exited;
        switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
        case 0 when selᴛ11.ꟷᐳ(out _): {
            Ꮡt.Fatal(waitGroupReleasedGroupˢ);
            break;
        }
        default: {
            break;
        }}
        Ꮡwg2.Done();
    }
    for (nint i = 0; i != n; i++) {
        ᐸꟷ(exited); // Will block if barrier fails to unlock someone.
    }
}

public static void TestWaitGroup(ж<Δtesting.T> Ꮡt) {
    var wg1 = Ꮡ(new WaitGroup(nil));
    var wg2 = Ꮡ(new WaitGroup(nil));
    // Run the same test a few times to ensure barrier is in a proper state.
    for (nint i = 0; i != 8; i++) {
        testWaitGroup(Ꮡt, wg1, wg2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shouldPanicˢ = (@string)"Should panic"u8;

public static void TestWaitGroupMisuse(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var err = recover();
            if (!AreEqual(err, (@string)("sync: negative WaitGroup counter"))) {
                Ꮡt.Fatalf("Unexpected panic: %#v"u8, err);
            }
        }, ref ᒐ);
        var wg = Ꮡ(new WaitGroup(nil));
        wg.Add(1);
        wg.Done();
        wg.Done();
        Ꮡt.Fatal(shouldPanicˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object spuriousWakeupFromWaitˢ = (@string)"Spurious wakeup from Wait"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestWaitGroupRace(ж<Δtesting.T> Ꮡt) {
    // Run this test for about 1ms.
    for (nint i = 0; i < 1000; i++) {
        var wg = Ꮡ(new WaitGroup(nil));
        var n = @new<int32>();
        // spawn goroutine 1
        wg.Add(1);
        var nʗ1 = n;
        var wgʗ1 = wg;
        goǃ(() => {
            atomic.AddInt32(nʗ1, 1);
            wgʗ1.Done();
        });
        // spawn goroutine 2
        wg.Add(1);
        var nʗ2 = n;
        var wgʗ2 = wg;
        goǃ(() => {
            atomic.AddInt32(nʗ2, 1);
            wgʗ2.Done();
        });
        // Wait for goroutine 1 and 2
        wg.Wait();
        if (atomic.LoadInt32(n) != 2) {
            Ꮡt.Fatal(spuriousWakeupFromWaitˢ);
        }
    }
}

[GoType("dyn")] internal partial struct TestWaitGroupAlign_X {
    internal byte x;
    internal Δsync.WaitGroup wg;
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestWaitGroupAlign(ж<Δtesting.T> Ꮡt) {
    ref var x = ref heap(new TestWaitGroupAlign_X(), out var Ꮡx);
    Ꮡx.of(TestWaitGroupAlign_X.Ꮡwg).Add(1);
    goǃ((ж<TestWaitGroupAlign_X> xΔ1) => {
        xΔ1.of(TestWaitGroupAlign_X.Ꮡwg).Done();
    }, Ꮡx);
    Ꮡx.of(TestWaitGroupAlign_X.Ꮡwg).Wait();
}

[GoType("dyn")] internal partial struct BenchmarkWaitGroupUncontended_PaddedWaitGroup {
    public partial ref sync_package.WaitGroup WaitGroup { get; }
    internal array<uint8> pad = new(128);
}

public static void BenchmarkWaitGroupUncontended(ж<Δtesting.B> Ꮡb) {
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        ref var wg = ref heap(new BenchmarkWaitGroupUncontended_PaddedWaitGroup(), out var Ꮡwg);
        while (pb.Next()) {
            Ꮡwg.of(BenchmarkWaitGroupUncontended_PaddedWaitGroup.ᏑWaitGroup).Add(1);
            Ꮡwg.of(BenchmarkWaitGroupUncontended_PaddedWaitGroup.ᏑWaitGroup).Done();
            Ꮡwg.of(BenchmarkWaitGroupUncontended_PaddedWaitGroup.ᏑWaitGroup).Wait();
        }
    });
}

internal static void benchmarkWaitGroupAddDone(ж<Δtesting.B> Ꮡb, nint localWork) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        nint foo = 0;
        while (pb.Next()) {
            Ꮡwg.Add(1);
            for (nint i = 0; i < localWork; i++) {
                foo *= 2;
                foo /= 2;
            }
            Ꮡwg.Done();
        }
        _ = foo;
    });
}

public static void BenchmarkWaitGroupAddDone(ж<Δtesting.B> Ꮡb) {
    benchmarkWaitGroupAddDone(Ꮡb, 0);
}

public static void BenchmarkWaitGroupAddDoneWork(ж<Δtesting.B> Ꮡb) {
    benchmarkWaitGroupAddDone(Ꮡb, 100);
}

internal static void benchmarkWaitGroupWait(ж<Δtesting.B> Ꮡb, nint localWork) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        nint foo = 0;
        while (pb.Next()) {
            Ꮡwg.Wait();
            for (nint i = 0; i < localWork; i++) {
                foo *= 2;
                foo /= 2;
            }
        }
        _ = foo;
    });
}

public static void BenchmarkWaitGroupWait(ж<Δtesting.B> Ꮡb) {
    benchmarkWaitGroupWait(Ꮡb, 0);
}

public static void BenchmarkWaitGroupWaitWork(ж<Δtesting.B> Ꮡb) {
    benchmarkWaitGroupWait(Ꮡb, 100);
}

public static void BenchmarkWaitGroupActuallyWait(ж<Δtesting.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    Ꮡb.RunParallel([MethodImpl(MethodImplOptions.NoInlining)] (ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
            Ꮡwg.Add(1);
            goǃ(() => {
                Ꮡwg.Done();
            });
            Ꮡwg.Wait();
        }
    });
}

} // end sync_test_package
