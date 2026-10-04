// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Pool is no-op under race detector, so all these tests do not work.
//
//go:build !race
namespace go;

using Δruntime = runtime_package;
using debug = go.runtime.debug_package;
using slices = slices_package;
using static sync_package;
using atomic = go.sync.atomic_package;
using Δtesting = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.runtime;
using go.sync;
using static go.sync_internal_test_package;
using Δsync = sync_package;

partial class sync_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedEmptyˢ = (@string)"expected empty"u8;

public static void TestPool(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // disable GC so we can control when it happens.
        defer(debug.SetGCPercent, debug.SetGCPercent(-1), ref ᒐ);
        ref var p = ref heap(new Δsync.Pool(), out var Ꮡp);
        if (Ꮡp.Get() != default!) {
            Ꮡt.Fatal(expectedEmptyˢ);
        }
        // Make sure that the goroutine doesn't migrate to another P
        // between Put and Get calls.
        sync_internal_test_package.Runtime_procPin();
        Ꮡp.Put((@string)"a"u8);
        Ꮡp.Put((@string)"b"u8);
        {
            var g = Ꮡp.Get(); if (!AreEqual(g, (@string)("a"))) {
                Ꮡt.Fatalf("got %#v; want a"u8, g);
            }
        }
        {
            var g = Ꮡp.Get(); if (!AreEqual(g, (@string)("b"))) {
                Ꮡt.Fatalf("got %#v; want b"u8, g);
            }
        }
        {
            var g = Ꮡp.Get(); if (g != default!) {
                Ꮡt.Fatalf("got %#v; want nil"u8, g);
            }
        }
        sync_internal_test_package.Runtime_procUnpin();
        // Put in a large number of objects so they spill into
        // stealable space.
        for (nint i = 0; i < 100; i++) {
            Ꮡp.Put((@string)"c"u8);
        }
        // After one GC, the victim cache should keep them alive.
        Δruntime.GC();
        {
            var g = Ꮡp.Get(); if (!AreEqual(g, (@string)("c"))) {
                Ꮡt.Fatalf("got %#v; want c after GC"u8, g);
            }
        }
        // A second GC should drop the victim cache.
        Δruntime.GC();
        {
            var g = Ꮡp.Get(); if (g != default!) {
                Ꮡt.Fatalf("got %#v; want nil after second GC"u8, g);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestPoolNew(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // disable GC so we can control when it happens.
        defer(debug.SetGCPercent, debug.SetGCPercent(-1), ref ᒐ);
        nint i = 0;
        ref var p = ref heap<Δsync.Pool>(out var Ꮡp);
        p = new Pool(
            New: () => {
                i++;
                return i;
            }
        );
        {
            var v = Ꮡp.Get(); if (!AreEqual(v, (nint)(1))) {
                Ꮡt.Fatalf("got %v; want 1"u8, v);
            }
        }
        {
            var v = Ꮡp.Get(); if (!AreEqual(v, (nint)(2))) {
                Ꮡt.Fatalf("got %v; want 2"u8, v);
            }
        }
        // Make sure that the goroutine doesn't migrate to another P
        // between Put and Get calls.
        sync_internal_test_package.Runtime_procPin();
        Ꮡp.Put((nint)(42));
        {
            var v = Ꮡp.Get(); if (!AreEqual(v, (nint)(42))) {
                Ꮡt.Fatalf("got %v; want 42"u8, v);
            }
        }
        sync_internal_test_package.Runtime_procUnpin();
        {
            var v = Ꮡp.Get(); if (!AreEqual(v, (nint)(3))) {
                Ꮡt.Fatalf("got %v; want 3"u8, v);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test that Pool does not hold pointers to previously cached resources.
public static void TestPoolGC(ж<Δtesting.T> Ꮡt) {
    testPool(Ꮡt, true);
}

// Test that Pool releases resources on GC.
public static void TestPoolRelease(ж<Δtesting.T> Ꮡt) {
    testPool(Ꮡt, false);
}

internal static void testPool(ж<Δtesting.T> Ꮡt, bool drain) {
    ref var p = ref heap(new Δsync.Pool(), out var Ꮡp);
    UntypedInt N = 100;
loop:
    for (nint @try = 0; @try < 3; @try++) {
        if (@try == 1 && Δtesting.Short()) {
            break;
        }
        ref var fin = ref heap(new uint32(), out var Ꮡfin);
        uint32 fin1 = default!;
        for (nint i = 0; i < N; i++) {
            var v = @new<@string>();
            Δruntime.SetFinalizer(v.OrTypedNil(), (ж<@string> vv) => {
                atomic.AddUint32(Ꮡfin, 1);
            });
            Ꮡp.Put(v.OrTypedNil());
        }
        if (drain) {
            for (nint i = 0; i < N; i++) {
                Ꮡp.Get();
            }
        }
        for (nint i = 0; i < 5; i++) {
            Δruntime.GC();
            time.Sleep(((time.Duration)(int64)(i * 100 + 10)) * time.Millisecond);
            // 1 pointer can remain on stack or elsewhere
            {
                fin1 = atomic.LoadUint32(Ꮡfin); if (fin1 >= (uint32)(N - 1)) {
                    goto continue_loop;
                }
            }
        }
        Ꮡt.Fatalf("only %v out of %v resources are finalized on try %v"u8, fin1, (nint)(N), @try);
continue_loop:;
    }
break_loop:;
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestPoolStress(ж<Δtesting.T> Ꮡt) {
    const nint P = 10;
    nint N = (nint)1000000;
    if (Δtesting.Short()) {
        N /= 100;
    }
    ref var p = ref heap(new Δsync.Pool(), out var Ꮡp);
    var done = new channel<bool>(0);
    for (nint i = 0; i < P; i++) {
        var doneʗ1 = done;
        goǃ(() => {
            any v = (nint)(0);
            for (nint j = 0; j < N; j++) {
                if (v == default!) {
                    v = (nint)(0);
                }
                Ꮡp.Put(v);
                v = Ꮡp.Get();
                if (v != default! && v._<nint>() != 0) {
                    Ꮡt.Errorf("expect 0, got %v"u8, v);
                    break;
                }
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < P; i++) {
        ᐸꟷ(done);
    }
}

public static void TestPoolDequeue(ж<Δtesting.T> Ꮡt) {
    testPoolDequeue(Ꮡt, sync_internal_test_package.NewPoolDequeue(16));
}

public static void TestPoolChain(ж<Δtesting.T> Ꮡt) {
    testPoolDequeue(Ꮡt, sync_internal_test_package.NewPoolChain());
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static void testPoolDequeue(ж<Δtesting.T> Ꮡt, global::go.sync_internal_test_package.PoolDequeue d) {
    const nint P = 10;
    nint N = 2000000;
    if (Δtesting.Short()) {
        N = 1000;
    }
    var have = new slice<int32>(N);
    ref var stop = ref heap(new int32(), out var Ꮡstop);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var haveʗ1 = have;
    void record(nint val) {
        atomic.AddInt32(Ꮡ(haveʗ1, val), 1);
        if (val == N - 1) {
            atomic.StoreInt32(Ꮡstop, 1);
        }
    }
    // Start P-1 consumers.
    for (nint i = 1; i < P; i++) {
        Ꮡwg.Add(1);
        var recordʗ1 = record;
        goǃ(() => {
            nint fail = 0;
            while (atomic.LoadInt32(Ꮡstop) == 0) {
                var (val, ok) = d.PopTail();
                if (ok){
                    fail = 0;
                    recordʗ1(val._<nint>());
                } else {
                    // Speed up the test by
                    // allowing the pusher to run.
                    {
                        fail++; if (fail % 100 == 0) {
                            Δruntime.Gosched();
                        }
                    }
                }
            }
            Ꮡwg.Done();
        });
    }
    // Start 1 producer.
    nint nPopHead = 0;
    Ꮡwg.Add(1);
    var recordʗ2 = record;
    goǃ(() => {
        for (nint j = 0; j < N; j++) {
            while (!d.PushHead(j)) {
                // Allow a popper to run.
                Δruntime.Gosched();
            }
            if (j % 10 == 0) {
                var (val, ok) = d.PopHead();
                if (ok) {
                    nPopHead++;
                    recordʗ2(val._<nint>());
                }
            }
        }
        Ꮡwg.Done();
    });
    Ꮡwg.Wait();
    // Check results.
    foreach (var (i, count) in have) {
        if (count != 1) {
            Ꮡt.Errorf("expected have[%d] = 1, got %d"u8, i, count);
        }
    }
    // Check that at least some PopHeads succeeded. We skip this
    // check in short mode because it's common enough that the
    // queue will stay nearly empty all the time and a PopTail
    // will happen during the window between every PushHead and
    // PopHead.
    if (!Δtesting.Short() && nPopHead == 0) {
        Ꮡt.Errorf("popHead never succeeded"u8);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedPanicˢ = (@string)"expected panic"u8;
internal static readonly @string getˢ = "Get"u8;
internal static readonly object shouldHavePanickedˢ = (@string)"should have panicked already"u8;
internal static readonly @string putˢ = "Put"u8;

public static void TestNilPool(ж<Δtesting.T> Ꮡt) {
    void @catch() {
        GoFrame ᒐ = default;
        try {
            if (recover() == default!) {
                Ꮡt.Error(expectedPanicˢ);
            }
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    ж<Δsync.Pool> p = default!;
    var catchʗ1 = @catch;
    var pʗ1 = p;
    Ꮡt.Run(getˢ, (ж<Δtesting.T> tΔ1) => {
        GoFrame ᒐ = default;
        try {
            var catchʗ2 = catchʗ1;
            defer(catchʗ2, ref ᒐ);
            if (pʗ1.Get() != default!) {
                tΔ1.Error(expectedEmptyˢ);
            }
            tΔ1.Error(shouldHavePanickedˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    var catchʗ3 = @catch;
    var pʗ2 = p;
    Ꮡt.Run(putˢ, (ж<Δtesting.T> tΔ2) => {
        GoFrame ᒐ = default;
        try {
            var catchʗ4 = catchʗ3;
            defer(catchʗ4, ref ᒐ);
            pʗ2.Put((@string)"a"u8);
            tΔ2.Error(shouldHavePanickedˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
}

public static void BenchmarkPool(ж<Δtesting.B> Ꮡb) {
    ref var p = ref heap(new Δsync.Pool(), out var Ꮡp);
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            Ꮡp.Put((nint)(1));
            Ꮡp.Get();
        }
    });
}

public static void BenchmarkPoolOverflow(ж<Δtesting.B> Ꮡb) {
    ref var p = ref heap(new Δsync.Pool(), out var Ꮡp);
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            for (nint bΔ1 = 0; bΔ1 < 100; bΔ1++) {
                Ꮡp.Put((nint)(1));
            }
            for (nint bΔ2 = 0; bΔ2 < 100; bΔ2++) {
                Ꮡp.Get();
            }
        }
    });
}

// Simulate object starvation in order to force Ps to steal objects
// from other Ps.
public static void BenchmarkPoolStarvation(ж<Δtesting.B> Ꮡb) {
    ref var p = ref heap(new Δsync.Pool(), out var Ꮡp);
    nint count = 100;
    // Reduce number of putted objects by 33 %. It creates objects starvation
    // that force P-local storage to steal objects from other Ps.
    nint countStarved = count - (nint)((float32)count * 0.33F);
    Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
        while (pb.Next()) {
            for (nint bΔ1 = 0; bΔ1 < countStarved; bΔ1++) {
                Ꮡp.Put((nint)(1));
            }
            for (nint bΔ2 = 0; bΔ2 < count; bΔ2++) {
                Ꮡp.Get();
            }
        }
    });
}

internal static any globalSink;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nsOpˢ = "ns/op"u8;
internal static readonly @string p95NsStwˢ = "p95-ns/STW"u8;
internal static readonly @string p50NsStwˢ = "p50-ns/STW"u8;

public static void BenchmarkPoolSTW(ж<Δtesting.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        // Take control of GC.
        defer(debug.SetGCPercent, debug.SetGCPercent(-1), ref ᒐ);
        ref var mstats = ref heap(new Δruntime.MemStats(), out var Ꮡmstats);
        slice<uint64> pauses = default!;
        ref var p = ref heap(new Δsync.Pool(), out var Ꮡp);
        for (nint i = 0; i < b.N; i++) {
            // Put a large number of items into a pool.
            const nint N = 100000;
            any item = (nint)(42);
            for (nint iΔ1 = 0; iΔ1 < N; iΔ1++) {
                Ꮡp.Put(item);
            }
            // Do a GC.
            Δruntime.GC();
            // Record pause time.
            Δruntime.ReadMemStats(Ꮡmstats);
            pauses = append(pauses, mstats.PauseNs[(mstats.NumGC + 255) % 256]);
        }
        // Get pause time stats.
        slices.Sort<slice<uint64>, uint64>(pauses);
        uint64 total = default!;
        foreach (var (_, ns) in pauses) {
            total += ns;
        }
        // ns/op for this benchmark is average STW time.
        b.ReportMetric((float64)total / (float64)b.N, nsOpˢ);
        b.ReportMetric((float64)pauses[len(pauses) * 95 / 100], p95NsStwˢ);
        b.ReportMetric((float64)pauses[len(pauses) * 50 / 100], p50NsStwˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gCsOpˢ = "GCs/op"u8;
internal static readonly @string newOpˢ = "New/op"u8;

public static void BenchmarkPoolExpensiveNew(ж<Δtesting.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        // Populate a pool with items that are expensive to construct
        // to stress pool cleanup and subsequent reconstruction.
        // Create a ballast so the GC has a non-zero heap size and
        // runs at reasonable times.
        globalSink = new slice<byte>((8 << (int)(20)));
        defer(() => {
            globalSink = default!;
        }, ref ᒐ);
        // Create a pool that's "expensive" to fill.
        ref var p = ref heap(new Δsync.Pool(), out var Ꮡp);
        ref var nNew = ref heap(new uint64(), out var ᏑnNew);
        p.New = () => {
            atomic.AddUint64(ᏑnNew, 1);
            time.Sleep(time.Millisecond);
            return (nint)(42);
        };
        ref var mstats1 = ref heap(new Δruntime.MemStats(), out var Ꮡmstats1);
        ref var mstats2 = ref heap(new Δruntime.MemStats(), out var Ꮡmstats2);
        Δruntime.ReadMemStats(Ꮡmstats1);
        Ꮡb.RunParallel((ж<Δtesting.PB> pb) => {
            // Simulate 100X the number of goroutines having items
            // checked out from the Pool simultaneously.
            var items = new slice<any>(100);
            slice<byte> sink = default!;
            while (pb.Next()) {
                // Stress the pool.
                foreach (var (i, _) in items) {
                    items[i] = Ꮡp.Get();
                    // Simulate doing some work with this
                    // item checked out.
                    sink = new slice<byte>((32 << (int)(10)));
                }
                foreach (var (i, v) in items) {
                    Ꮡp.Put(v);
                    items[i] = default!;
                }
            }
            _ = sink;
        });
        Δruntime.ReadMemStats(Ꮡmstats2);
        b.ReportMetric((float64)(mstats2.NumGC - mstats1.NumGC) / (float64)b.N, gCsOpˢ);
        b.ReportMetric((float64)nNew / (float64)b.N, newOpˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end sync_test_package
