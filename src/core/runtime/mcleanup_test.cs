// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using atomic = @internal.runtime.atomic_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal.runtime;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
internal partial struct TestCleanup_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanup(ж<testing.T> Ꮡt) {
    var ch = new channel<bool>(1);
    var done = new channel<bool>(1);
    nint want = 97531;
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanup_T>().of(TestCleanup_T.Ꮡv);
        v.Value = 97531;
        var chʗ2 = chʗ1;
        var cleanup = (nint x) => {
            if (x != want) {
                Ꮡt.Errorf("cleanup %d, want %d"u8, x, want);
            }
            chʗ2.ᐸꟷ(true);
        };
        Δruntime.AddCleanup(v, cleanup, 97531, new object?[] { Ꮡt.Value });
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
}

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
internal partial struct TestCleanupMultiple_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupMultiple(ж<testing.T> Ꮡt) {
    var ch = new channel<bool>(3);
    var done = new channel<bool>(1);
    nint want = 97531;
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupMultiple_T>().of(TestCleanupMultiple_T.Ꮡv);
        v.Value = 97531;
        var chʗ2 = chʗ1;
        var cleanup = (nint x) => {
            if (x != want) {
                Ꮡt.Errorf("cleanup %d, want %d"u8, x, want);
            }
            chʗ2.ᐸꟷ(true);
        };
        Δruntime.AddCleanup(v, cleanup, 97531, new object?[] { Ꮡt.Value });
        Δruntime.AddCleanup(v, cleanup, 97531, new object?[] { Ꮡt.Value });
        Δruntime.AddCleanup(v, cleanup, 97531, new object?[] { Ꮡt.Value });
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
    ᐸꟷ(ch);
    ᐸꟷ(ch);
}

internal partial struct TestCleanupZeroSizedStruct_Z /*dyn*/ {
}

public static void TestCleanupZeroSizedStruct(ж<testing.T> Ꮡt) {
    var z = @new<TestCleanupZeroSizedStruct_Z>();
    Δruntime.AddCleanup(z, (@string s) => {
    }, fooˢ2);
}

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
internal partial struct TestCleanupAfterFinalizer_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupAfterFinalizer(ж<testing.T> Ꮡt) {
    var ch = new channel<nint>(2);
    var done = new channel<bool>(1);
    nint want = 97531;
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupAfterFinalizer_T>().of(TestCleanupAfterFinalizer_T.Ꮡv);
        v.Value = 97531;
        var chʗ2 = chʗ1;
        var finalizer = (ж<nint> x) => {
            chʗ2.ᐸꟷ(1);
        };
        var chʗ3 = chʗ1;
        var cleanup = (nint x) => {
            if (x != want) {
                Ꮡt.Errorf("cleanup %d, want %d"u8, x, want);
            }
            chʗ3.ᐸꟷ(2);
        };
        Δruntime.AddCleanup(v, cleanup, 97531, new object?[] { Ꮡt.Value });
        Δruntime.SetFinalizer(v.OrTypedNil(), (finalizer).OrTypedNilFunc());
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    nint result = default!;
    result = ᐸꟷ(ch);
    if (result != 1) {
        Ꮡt.Errorf("result %d, want 1"u8, result);
    }
    Δruntime.GC();
    result = ᐸꟷ(ch);
    if (result != 2) {
        Ꮡt.Errorf("result %d, want 2"u8, result);
    }
}

// Allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
internal partial struct TestCleanupInteriorPointer_T /*dyn*/ {
    internal @unsafe.Pointer p;
    internal nint i;
    internal nint a;
    internal nint b;
    internal nint c;
}

public static partial void TestCleanupInteriorPointer(ж<testing.T> Ꮡt) {
    var ch = new channel<bool>(3);
    var done = new channel<bool>(1);
    nint want = 97531;
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var ts = @new<TestCleanupInteriorPointer_T>();
        ts.Value.a = 97531;
        ts.Value.b = 97531;
        ts.Value.c = 97531;
        var chʗ2 = chʗ1;
        var cleanup = (nint x) => {
            if (x != want) {
                Ꮡt.Errorf("cleanup %d, want %d"u8, x, want);
            }
            chʗ2.ᐸꟷ(true);
        };
        Δruntime.AddCleanup(ts.of(TestCleanupInteriorPointer_T.Ꮡa), cleanup, 97531, new object?[] { Ꮡt.Value });
        Δruntime.AddCleanup(ts.of(TestCleanupInteriorPointer_T.Ꮡb), cleanup, 97531, new object?[] { Ꮡt.Value });
        Δruntime.AddCleanup(ts.of(TestCleanupInteriorPointer_T.Ꮡc), cleanup, 97531, new object?[] { Ꮡt.Value });
        ts = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
    ᐸꟷ(ch);
    ᐸꟷ(ch);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cleanupCalledWantNoˢ = (@string)"cleanup called, want no cleanup called"u8;

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
internal partial struct TestCleanupStop_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupStop(ж<testing.T> Ꮡt) {
    var done = new channel<bool>(1);
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupStop_T>().of(TestCleanupStop_T.Ꮡv);
        v.Value = 97531;
        var cleanup = (nint x) => {
            Ꮡt.Error(cleanupCalledWantNoˢ);
        };
        ref var c = ref heap<Δruntime.Cleanup>(out var Ꮡc);
        c = Δruntime.AddCleanup(v, cleanup, 97531, new object?[] { Ꮡt.Value });
        c.Stop();
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
}

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
internal partial struct TestCleanupStopMultiple_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupStopMultiple(ж<testing.T> Ꮡt) {
    var done = new channel<bool>(1);
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupStopMultiple_T>().of(TestCleanupStopMultiple_T.Ꮡv);
        v.Value = 97531;
        var cleanup = (nint x) => {
            Ꮡt.Error(cleanupCalledWantNoˢ);
        };
        ref var c = ref heap<Δruntime.Cleanup>(out var Ꮡc);
        c = Δruntime.AddCleanup(v, cleanup, 97531, new object?[] { Ꮡt.Value });
        c.Stop();
        c.Stop();
        c.Stop();
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
}

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
internal partial struct TestCleanupStopinterleavedMultiple_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupStopinterleavedMultiple(ж<testing.T> Ꮡt) {
    var ch = new channel<bool>(3);
    var done = new channel<bool>(1);
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupStopinterleavedMultiple_T>().of(TestCleanupStopinterleavedMultiple_T.Ꮡv);
        v.Value = 97531;
        var chʗ2 = chʗ1;
        var cleanup = (nint x) => {
            if (x != 1) {
                Ꮡt.Error(cleanupCalledWantNoˢ);
            }
            chʗ2.ᐸꟷ(true);
        };
        Δruntime.AddCleanup(v, cleanup, 1, new object?[] { Ꮡt.Value });
        Δruntime.AddCleanup(v, cleanup, 2, new object?[] { Ꮡt.Value }).Stop();
        Δruntime.AddCleanup(v, cleanup, 1, new object?[] { Ꮡt.Value });
        Δruntime.AddCleanup(v, cleanup, 2, new object?[] { Ꮡt.Value }).Stop();
        Δruntime.AddCleanup(v, cleanup, 1, new object?[] { Ꮡt.Value });
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
    ᐸꟷ(ch);
    ᐸꟷ(ch);
}

// Allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
internal partial struct TestCleanupStopAfterCleanupRuns_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupStopAfterCleanupRuns(ж<testing.T> Ꮡt) {
    var ch = new channel<bool>(1);
    var done = new channel<bool>(1);
    ref var stop = ref heap<Action>(out var Ꮡstop);
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupStopAfterCleanupRuns_T>().of(TestCleanupStopAfterCleanupRuns_T.Ꮡv);
        v.Value = 97531;
        var chʗ2 = chʗ1;
        var cleanup = (nint x) => {
            chʗ2.ᐸꟷ(true);
        };
        ref var cl = ref heap<Δruntime.Cleanup>(out var Ꮡcl);
        cl = Δruntime.AddCleanup(v, cleanup, 97531);
        v = default!;
        var clʗ1 = cl;
                Ꮡstop.ValueSlot = () => clʗ1.Stop();
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
    stop();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeAddCleanupPtrIsˢ = "runtime.AddCleanup: ptr is equal to arg, cleanup will never run"u8;
internal static readonly object wantPanicTestDidNotPanicˢ = (@string)"want panic, test did not panic"u8;

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
internal partial struct TestCleanupPointerEqualsArg_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static void TestCleanupPointerEqualsArg(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // See go.dev/issue/71316
        defer(() => {
            @string want = runtimeAddCleanupPtrIsˢ;
            {
                var r = recover(); if (r == default!){
                    Ꮡt.Error(wantPanicTestDidNotPanicˢ);
                } else 
                if (AreEqual(r, want)){
                    // do nothing
                } else {
                    Ꮡt.Errorf("wrong panic: want=%q, got=%q"u8, want, r);
                }
            }
        }, ref ᒐ);
        var v = @new<TestCleanupPointerEqualsArg_T>().of(TestCleanupPointerEqualsArg_T.Ꮡv);
        v.Value = 97531;
        Δruntime.AddCleanup(v, (ж<nint> x) => {
        }, v);
        v = default!;
        Δruntime.GC();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct TestCleanupLost_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

// Checks to make sure cleanups aren't lost when there are a lot of them.
public static partial void TestCleanupLost(ж<testing.T> Ꮡt) {
    nint cleanups = 10_000;
    if (testing.Short()) {
        cleanups = 100;
    }
    nint n = Δruntime.GOMAXPROCS(-1);
    nint want = n * cleanups;
    ref var got = ref heap(new atomic.Uint64(), out var Ꮡgot);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    foreach (var i in range(n)) {
        Ꮡwg.Add(1);
        goǃ((nint iΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);

                foreach (var _ᴛ1 in range(cleanups)) {
                    var v = @new<TestCleanupLost_T>().of(TestCleanupLost_T.Ꮡv);
                    v.Value = 97531;
                    Δruntime.AddCleanup(v, (nint _) => {
                        Ꮡgot.Add(1);
                    }, 97531);
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }
    Ꮡwg.Wait();
    Δruntime.GC();
    var timeout = (time.Duration)(10000000000L);
    var empty = runtime_internal_test_package.BlockUntilEmptyCleanupQueue((int64)timeout);
    if (!empty) {
        Ꮡt.Errorf("failed to drain cleanup queue within %s"u8, timeout);
    }

    {
        nint gotΔ1 = (nint)Ꮡgot.Load(); if (gotΔ1 != want) {
            Ꮡt.Errorf("%d cleanups executed, expected %d"u8, gotΔ1, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object addCleanupFailedToDetectˢ = (@string)"AddCleanup failed to detect self-pointer"u8;
internal static readonly object cleanupRanUnexpectedlyˢ = (@string)"cleanup ran unexpectedly"u8;

internal partial struct TestCleanupUnreachablePointer_T /*dyn*/ {
    internal ж<byte> p; // use *byte to avoid tiny allocator
    internal nint f;
}

public static void TestCleanupUnreachablePointer(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(() => {
            if (recover() == default!) {
                Ꮡt.Error(addCleanupFailedToDetectˢ);
            }
        }, ref ᒐ);
        var v = Ꮡ(new TestCleanupUnreachablePointer_T(nil));
        Δruntime.AddCleanup(v, (ж<nint> _) => {
            Ꮡt.Error(cleanupRanUnexpectedlyˢ);
        }, v.of(TestCleanupUnreachablePointer_T.Ꮡf), new object?[] { t });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object addCleanupFailedToDetectˢ2 = (@string)"AddCleanup failed to detect closure pointer"u8;

internal partial struct TestCleanupUnreachableClosure_T /*dyn*/ {
    internal ж<byte> p; // use *byte to avoid tiny allocator
    internal nint f;
}

public static void TestCleanupUnreachableClosure(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(() => {
            if (recover() == default!) {
                Ꮡt.Error(addCleanupFailedToDetectˢ2);
            }
        }, ref ᒐ);
        var v = Ꮡ(new TestCleanupUnreachableClosure_T(nil));
        var vʗ1 = v;
        Δruntime.AddCleanup(v, (nint _) => {
            Ꮡt.Log((~vʗ1).f);
            Ꮡt.Error(cleanupRanUnexpectedlyˢ);
        }, 0, new object?[] { t, v });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal partial struct BenchmarkAddCleanupAndStop_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

// BenchmarkAddCleanupAndStop benchmarks adding and removing a cleanup
// from the same allocation.
//
// At face value, this benchmark is unrealistic, since no program would
// do this in practice. However, adding cleanups to new allocations in a
// loop is also unrealistic. It adds additional unused allocations,
// exercises uncommon performance pitfalls in AddCleanup (traversing the
// specials list, which should just be its own benchmark), and executing
// cleanups at a frequency that is unlikely to appear in real programs.
//
// This benchmark is still useful however, since we can get a low-noise
// measurement of the cost of AddCleanup and Stop all in one without the
// above pitfalls: we can measure the pure overhead. We can then separate
// out the cost of each in CPU profiles if we so choose (they're not so
// inexpensive as to make this infeasible).
public static void BenchmarkAddCleanupAndStop(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.ReportAllocs();
    var x = @new<BenchmarkAddCleanupAndStop_T>();
    while (Ꮡb.Loop()) {
        Δruntime.AddCleanup(x, (nint _) => {
        }, 14).Stop();
    }
}

} // end runtime_test_package
