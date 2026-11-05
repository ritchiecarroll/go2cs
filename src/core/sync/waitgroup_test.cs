// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = go.os.exec_package;
using strings = strings_package;
using static sync_package;
using atomic = go.sync.atomic_package;
using Δtesting = testing_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.os;
using go.sync;
using static go.sync_internal_test_package;
using Δsync = sync_package;

partial class sync_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object waitGroupReleasedGroupˢ = (@string)"WaitGroup released group too soon"u8;

internal static partial void testWaitGroup(ж<Δtesting.T> t, ж<Δsync.WaitGroup> wg1, ж<Δsync.WaitGroup> wg2) {
    nint n = 16;
    wg1.Add(n);
    wg2.Add(n);
    var exited = new channel<bool>(n);
    for (nint i = 0; i != n; i++) {
        var exitedʗ1 = exited;
        goǃ(() => {
            wg1.Done();
            wg2.Wait();
            exitedʗ1.ᐸꟷ(true);
        });
    }
    wg1.Wait();
    for (nint i = 0; i != n; i++) {
        var selᴛ11 = exited;
        switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
        case 0 when selᴛ11.ꟷᐳ(out _): {
            t.Fatal(waitGroupReleasedGroupˢ);
            break;
        }
        default: {
            break;
        }}
        wg2.Done();
    }
    for (nint i = 0; i != n; i++) {
        ᐸꟷ(exited); // Will block if barrier fails to unlock someone.
    }
}

public static void TestWaitGroup(ж<Δtesting.T> t) {
    var wg1 = Ꮡ(new WaitGroup());
    var wg2 = Ꮡ(new WaitGroup());

    // Run the same test a few times to ensure barrier is in a proper state.
    for (nint i = 0; i != 8; i++) {
        testWaitGroup(t, wg1, wg2);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object shouldPanicˢ = (@string)"Should panic"u8;

public static void TestWaitGroupMisuse(ж<Δtesting.T> t) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var err = recover();
            if (!AreEqual(err, (@string)("sync: negative WaitGroup counter"))) {
                t.Fatalf("Unexpected panic: %#v", err);
            }
        }, ref ᒐ);
        var wg = Ꮡ(new WaitGroup());
        wg.Add(1);
        wg.Done();
        wg.Done();
        t.Fatal(shouldPanicˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object spuriousWakeupFromWaitˢ = (@string)"Spurious wakeup from Wait"u8;

public static partial void TestWaitGroupRace(ж<Δtesting.T> t) {
    // Run this test for about 1ms.
    for (nint i = 0; i < 1000; i++) {
        var wg = Ꮡ(new WaitGroup());
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
            t.Fatal(spuriousWakeupFromWaitˢ);
        }
    }
}

internal partial struct TestWaitGroupAlign_X /*dyn*/ {
    internal byte x;
    internal Δsync.WaitGroup wg;
}

public static partial void TestWaitGroupAlign(ж<Δtesting.T> t) {
    ref var x = ref heap(new TestWaitGroupAlign_X(), out var Ꮡx);
    Ꮡx.Ꮡwg.Add(1);
    goǃ((ж<TestWaitGroupAlign_X> xΔ1) => {
        xΔ1.Ꮡwg.Done();
    }, Ꮡx);
    Ꮡx.Ꮡwg.Wait();
}

public static void TestWaitGroupGo(ж<Δtesting.T> t) {
    var wg = Ꮡ(new WaitGroup());
    nint i = default;
    wg.Go(() => {
        i++;
    });
    wg.Wait();
    if (i != 1) {
        t.Fatalf("got %d, want 1", i);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string syncTestChildˢ => ref syncTestChildˢᵇ;
private static readonly @string syncTestChildˢᵇ = "SYNC_TEST_CHILD"u8;
internal static ref readonly @string testRunTestIssue76126ˢ => ref testRunTestIssue76126ˢᵇ;
private static readonly @string testRunTestIssue76126ˢᵇ = "-test.run=^TestIssue76126$"u8;
internal static ref readonly @string panicTestˢ => ref panicTestˢᵇ;
private static readonly @string panicTestˢᵇ = "panic: test"u8;

// This test ensures that an unhandled panic in a Go goroutine terminates
// the process without causing Wait to unblock; previously there was a race.
public static void TestIssue76126(ж<Δtesting.T> t) {
    testenv.MustHaveExec(new sync_test_package.testing_TжTB(t));
    if (Δos.Getenv(syncTestChildˢ) != "1"u8) {
        // Call child in a child process
        // and inspect its failure message.
        var cmd = exec.Command(Δos.Args[0], testRunTestIssue76126ˢ);
        cmd.Env = append(Δos.Environ(), (@string)"SYNC_TEST_CHILD=1");
        var buf = @new<bytes.Buffer>();
        cmd.Stderr = new sync_test_package.bytes_BufferжWriter(buf);
        cmd.Run(); // ignore error
        @string got = buf.String();
        if (!strings.Contains(got, panicTestˢ)) {
            t.Errorf("missing panic: test\n%s", got);
        }
        return;
    }
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    Ꮡwg.Go(() => {
        throw panic("test");
    });
    Ꮡwg.Wait();                   // process should terminate here
    throw panic("Wait returned"); // must not be reached
}

internal partial struct BenchmarkWaitGroupUncontended_PaddedWaitGroup /*dyn*/ {
    /*embed*/ public sync_package.WaitGroup WaitGroup;
    internal array<uint8> pad = new(128);
}

public static void BenchmarkWaitGroupUncontended(ж<Δtesting.B> b) {
    b.RunParallel((ж<Δtesting.PB> pb) => {
        ref var wg = ref heap(new BenchmarkWaitGroupUncontended_PaddedWaitGroup(), out var Ꮡwg);
        while (pb.Next()) {
            Ꮡwg.of(BenchmarkWaitGroupUncontended_PaddedWaitGroup.ᏑWaitGroup).Add(1);
            Ꮡwg.of(BenchmarkWaitGroupUncontended_PaddedWaitGroup.ᏑWaitGroup).Done();
            Ꮡwg.of(BenchmarkWaitGroupUncontended_PaddedWaitGroup.ᏑWaitGroup).Wait();
        }
    });
}

internal static void benchmarkWaitGroupAddDone(ж<Δtesting.B> b, nint localWork) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    b.RunParallel((ж<Δtesting.PB> pb) => {
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

public static void BenchmarkWaitGroupAddDone(ж<Δtesting.B> b) {
    benchmarkWaitGroupAddDone(b, 0);
}

public static void BenchmarkWaitGroupAddDoneWork(ж<Δtesting.B> b) {
    benchmarkWaitGroupAddDone(b, 100);
}

internal static void benchmarkWaitGroupWait(ж<Δtesting.B> b, nint localWork) {
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    b.RunParallel((ж<Δtesting.PB> pb) => {
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

public static void BenchmarkWaitGroupWait(ж<Δtesting.B> b) {
    benchmarkWaitGroupWait(b, 0);
}

public static void BenchmarkWaitGroupWaitWork(ж<Δtesting.B> b) {
    benchmarkWaitGroupWait(b, 100);
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
