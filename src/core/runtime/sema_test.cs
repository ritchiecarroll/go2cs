// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using static runtime_package;
using Δsync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.sync;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object directHandoff23ˢ = (@string)"direct handoff < 2/3:"u8;

// TestSemaHandoff checks that when semrelease+handoff is
// requested, the G that releases the semaphore yields its
// P directly to the first waiter in line.
// See issue 33747 for discussion.
public static void TestSemaHandoff(ж<testing.T> Ꮡt) {
    UntypedInt iter = 10000;
    nint ok = 0;
    for (nint i = 0; i < iter; i++) {
        if (testSemaHandoff()) {
            ok++;
        }
    }
    // As long as two thirds of handoffs are direct, we
    // consider the test successful. The scheduler is
    // nondeterministic, so this test checks that we get the
    // desired outcome in a significant majority of cases.
    // The actual ratio of direct handoffs is much higher
    // (>90%) but we use a lower threshold to minimize the
    // chances that unrelated changes in the runtime will
    // cause the test to fail or become flaky.
    if (ok < (nint)(iter * 2 / 3)) {
        Ꮡt.Fatal(directHandoff23ˢ, ok, (nint)(iter));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gomaxprocs1ˢ2 = (@string)"GOMAXPROCS <= 1"u8;

public static void TestSemaHandoff1(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (GOMAXPROCS(-1) <= 1) {
            Ꮡt.Skip(gomaxprocs1ˢ2);
        }
        defer(GOMAXPROCS, GOMAXPROCS(-1), ref ᒐ);
        GOMAXPROCS(1);
        TestSemaHandoff(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object gomaxprocs2ˢ = (@string)"GOMAXPROCS <= 2"u8;

public static void TestSemaHandoff2(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (GOMAXPROCS(-1) <= 2) {
            Ꮡt.Skip(gomaxprocs2ˢ);
        }
        defer(GOMAXPROCS, GOMAXPROCS(-1), ref ᒐ);
        GOMAXPROCS(2);
        TestSemaHandoff(Ꮡt);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial bool testSemaHandoff() {
    ref var sema = ref heap(new uint32(), out var Ꮡsema);
    ref var res = ref heap(new uint32(), out var Ꮡres);
    var done = new channel<EmptyStruct>(0);
    // We're testing that the current goroutine is able to yield its time slice
    // to another goroutine. Stop the current goroutine from migrating to
    // another CPU where it can win the race (and appear to have not yielded) by
    // keeping the CPUs slightly busy.
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < GOMAXPROCS(-1); i++) {
        Ꮡwg.Add(1);
        var doneʗ1 = done;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                while (ᐧ) {
                    var selᴛ95 = doneʗ1;
                    switch (trySelect(ᐸꟷ(selᴛ95, ꓸꓸꓸ))) {
                    case 0 when selᴛ95.ꟷᐳ(out _): {
                        return;
                    }
                    default: {
                        break;
                    }}
                    Gosched();
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Add(1);
    var doneʗ2 = done;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            defer(Ꮡwg.Done, ref ᒐ);
            runtime_internal_test_package.Semacquire(Ꮡsema);
            atomic.CompareAndSwapUint32(Ꮡres, 0, 1);
            runtime_internal_test_package.Semrelease1(Ꮡsema, true, 0);
            close(doneʗ2);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    while (runtime_internal_test_package.SemNwait(Ꮡsema) == 0) {
        Gosched(); // wait for goroutine to block in Semacquire
    }
    // The crux of the test: we release the semaphore with handoff
    // and immediately perform a CAS both here and in the waiter; we
    // want the CAS in the waiter to execute first.
    runtime_internal_test_package.Semrelease1(Ꮡsema, true, 0);
    atomic.CompareAndSwapUint32(Ꮡres, 0, 2);
    Ꮡwg.Wait(); // wait for goroutines to finish to avoid data races
    return res == 1; // did the waiter run first?
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object failedToDequeueˢ = (@string)"failed to dequeue"u8;

public static void BenchmarkSemTable(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in new nint[]{1000, 2000, 4000, 8000}.slice()) {
        Ꮡb.Run(fmt.Sprintf("OneAddrCollision/n=%d"u8, n), (ж<testing.B> bΔ1) => {
            var tab = runtime_internal_test_package.Escape(@new<global::go.runtime_internal_test_package.SemTable>());
            var u = new slice<uint32>(runtime_internal_test_package.SemTableSize + 1);
            bΔ1.ResetTimer();
            for (nint j = 0; j < (~bΔ1).N; j++) {
                // Simulate two locks colliding on the same semaRoot.
                //
                // Specifically enqueue all the waiters for the first lock,
                // then all the waiters for the second lock.
                //
                // Then, dequeue all the waiters from the first lock, then
                // the second.
                //
                // Each enqueue/dequeue operation should be O(1), because
                // there are exactly 2 locks. This could be O(n) if all
                // the waiters for both locks are on the same list, as it
                // once was.
                for (nint i = 0; i < n; i++) {
                    if (i < n / 2){
                        tab.Enqueue(Ꮡ(u, 0));
                    } else {
                        tab.Enqueue(Ꮡ(u, runtime_internal_test_package.SemTableSize));
                    }
                }
                for (nint i = 0; i < n; i++) {
                    bool ok = default!;
                    if (i < n / 2){
                        ok = tab.Dequeue(Ꮡ(u, 0));
                    } else {
                        ok = tab.Dequeue(Ꮡ(u, runtime_internal_test_package.SemTableSize));
                    }
                    if (!ok) {
                        bΔ1.Fatal(failedToDequeueˢ);
                    }
                }
            }
        });
        Ꮡb.Run(fmt.Sprintf("ManyAddrCollision/n=%d"u8, n), (ж<testing.B> bΔ2) => {
            var tab = runtime_internal_test_package.Escape(@new<global::go.runtime_internal_test_package.SemTable>());
            var u = new slice<uint32>(n * (nint)runtime_internal_test_package.SemTableSize);
            bΔ2.ResetTimer();
            for (nint j = 0; j < (~bΔ2).N; j++) {
                // Simulate n locks colliding on the same semaRoot.
                //
                // Each enqueue/dequeue operation should be O(log n), because
                // each semaRoot is a tree. This could be O(n) if it was
                // some simpler data structure.
                for (nint i = 0; i < n; i++) {
                    tab.Enqueue(Ꮡ(u, i * (nint)runtime_internal_test_package.SemTableSize));
                }
                for (nint i = 0; i < n; i++) {
                    if (!tab.Dequeue(Ꮡ(u, i * (nint)runtime_internal_test_package.SemTableSize))) {
                        bΔ2.Fatal(failedToDequeueˢ);
                    }
                }
            }
        });
    }
}

} // end runtime_test_package
