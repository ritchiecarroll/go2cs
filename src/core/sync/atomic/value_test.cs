// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.sync;

using rand = math.rand_package;
using runtime = runtime_package;
using strconv = strconv_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using static go.sync.atomic_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.sync;
using math;

partial class atomic_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object initialValueIsNotNilˢ = (@string)"initial Value is not nil"u8;

public static void TestValue(ж<testing.T> t) {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    if (Ꮡv.Load() != nil) {
        t.Fatal(initialValueIsNotNilˢ);
    }
    Ꮡv.Store((nint)42);
    var x = Ꮡv.Load();
    {
        var (xx, ok) = x._<nint>(ᐧ); if (!ok || xx != 42) {
            t.Fatalf("wrong value: got %+v, want 42", x);
        }
    }
    Ꮡv.Store((nint)84);
    x = Ꮡv.Load();
    {
        var (xx, ok) = x._<nint>(ᐧ); if (!ok || xx != 84) {
            t.Fatalf("wrong value: got %+v, want 84", x);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fooˢ = (@string)"foo"u8;
private static readonly object barbazˢ = (@string)"barbaz"u8;

public static void TestValueLarge(ж<testing.T> t) {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    Ꮡv.Store(fooˢ);
    var x = Ꮡv.Load();
    {
        var (xx, ok) = x._<@string>(ᐧ); if (!ok || xx != "foo"u8) {
            t.Fatalf("wrong value: got %+v, want foo", x);
        }
    }
    Ꮡv.Store(barbazˢ);
    x = Ꮡv.Load();
    {
        var (xx, ok) = x._<@string>(ᐧ); if (!ok || xx != "barbaz"u8) {
            t.Fatalf("wrong value: got %+v, want barbaz", x);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static ref readonly @string nilErrᶜ => ref nilErrᶜᵇ;
private static readonly @string nilErrᶜᵇ = "sync/atomic: store of nil value into Value"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static ref readonly @string badErrᶜ => ref badErrᶜᵇ;
private static readonly @string badErrᶜᵇ = "sync/atomic: store of inconsistently typed value into Value"u8;

public static void TestValuePanic(ж<testing.T> t) {
    @string nilErr = nilErrᶜ;
    @string badErr = badErrᶜ;
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var err = recover();
                if (!AreEqual(err, nilErr)) {
                    t.Fatalf("inconsistent store panic: got '%v', want '%v'", err, nilErr);
                }
            }, ref ᒐ);
            Ꮡv.Store(default);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    Ꮡv.Store((nint)42);
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var err = recover();
                if (!AreEqual(err, badErr)) {
                    t.Fatalf("inconsistent store panic: got '%v', want '%v'", err, badErr);
                }
            }, ref ᒐ);
            Ꮡv.Store(fooˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
    func(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var err = recover();
                if (!AreEqual(err, nilErr)) {
                    t.Fatalf("inconsistent store panic: got '%v', want '%v'", err, nilErr);
                }
            }, ref ᒐ);
            Ꮡv.Store(default);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    })();
}

public static partial void TestValueConcurrent(ж<testing.T> t) {
    var tests = slice<slice<any>>([
        slice<any>([(uint16)0, unchecked((uint16)(~(uint16)0)), (uint16)(1 + (2 << 8)), (uint16)(3 + (4 << 8))]),
        slice<any>([(uint32)0, ~(uint32)0, (uint32)(1 + (2 << 16)), (uint32)(3 + (4 << 16))]),
        slice<any>([(uint64)0, ~(uint64)0, (uint64)(1 + 8589934592L), (uint64)(3 + 17179869184L)]),
        slice<any>([complex(0D, 0D), complex(1D, 2D), complex(3D, 4D), complex(5D, 6D)])
    ]);
    nint p = 4 * runtime.GOMAXPROCS(0);
    nint N = (nint)100000;
    if (testing.Short()) {
        p /= 2;
        N = 1000;
    }
    foreach (var (_, test) in tests) {
        ref var v = ref heap(new atomic.Value(), out var Ꮡv);
        var done = new channel<bool>(p);
        for (nint i = 0; i < p; i++) {
            var doneʗ1 = done;
            var testʗ1 = test;
            goǃ(() => {
                var r = rand.New(rand.NewSource(rand.Int63()));
                var expected = true;
loop:
                for (nint j = 0; j < N; j++) {
                    var x = testʗ1[r.Intn(len(testʗ1))];
                    Ꮡv.Store(x);
                    x = Ꮡv.Load();
                    foreach (var (_, x1) in testʗ1) {
                        if (AreEqual(x, x1)) {
                            goto continue_loop;
                        }
                    }
                    t.Logf("loaded unexpected value %+v, want %+v", x, testʗ1);
                    expected = false;
                    break;
continue_loop:;
                }
break_loop:;
                doneʗ1.ᐸꟷ(expected);
            });
        }
        for (nint i = 0; i < p; i++) {
            if (!ᐸꟷ(done)) {
                t.FailNow();
            }
        }
    }
}

public static void BenchmarkValueRead(ж<testing.B> b) {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    Ꮡv.Store(@new<nint>());
    b.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            var x = Ꮡv.Load()._<ж<nint>>();
            if (x.Value != 0) {
                b.Fatalf("wrong value: got %v, want 0", x.Value);
            }
        }
    });
}

partial struct Value_SwapTestsᴛ1 /*dyn*/ {
    internal any init;
    internal any @new;
    internal any want;
    internal any err;
}
public static readonly ж<slice<Value_SwapTestsᴛ1>> ᏑValue_SwapTests = new StaticBox<slice<Value_SwapTestsᴛ1>>(slice<Value_SwapTestsᴛ1>([
    new(init: default, @new: default, err: (@string)"sync/atomic: swap of nil value into Value"),
    new(init: default, @new: true, want: default, err: default),
    new(init: true, @new: (@string)""u8, err: (@string)"sync/atomic: swap of inconsistently typed value into Value"),
    new(init: true, @new: false, want: true, err: default)
]));
public static ref slice<Value_SwapTestsᴛ1> Value_SwapTests => ref ((StaticBox<slice<Value_SwapTestsᴛ1>>)ᏑValue_SwapTests).ValueSlot;

public static void TestValue_Swap(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in Value_SwapTests) {
        ref var tt = ref heap(new Value_SwapTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(strconv.Itoa(i), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                ref var v = ref heap(new atomic.Value(), out var Ꮡv);
                if (ttʗ1.init != nil) {
                    Ꮡv.Store(ttʗ1.init);
                }
                var ttʗ2 = ttʗ1;
                defer(() => {
                    var err = recover();
                    switch (ᐧ) {
                    case {} when ttʗ2.err == nil && err != nil: {
                        tΔ1.Errorf("should not panic, got %v", err);
                        break;
                    }
                    case {} when ttʗ2.err != nil && err == nil: {
                        tΔ1.Errorf("should panic %v, got <nil>", ttʗ2.err);
                        break;
                    }}
                }, ref ᒐ);
                {
                    var got = Ꮡv.Swap(ttʗ1.@new); if (!AreEqual(got, ttʗ1.want)) {
                        tΔ1.Errorf("got %v, want %v", got, ttʗ1.want);
                    }
                }
                {
                    var got = Ꮡv.Load(); if (!AreEqual(got, ttʗ1.@new)) {
                        tΔ1.Errorf("got %v, want %v", got, ttʗ1.@new);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static partial void TestValueSwapConcurrent(ж<testing.T> t) {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    ref var count = ref heap(new uint64(), out var Ꮡcount);
    ref var g = ref heap(new sync.WaitGroup(), out var Ꮡg);
    uint64 m = 10000;
    uint64 n = 10000;
    if (testing.Short()) {
        m = 1000;
        n = 1000;
    }
    for (var iᴛ1 = (uint64)0; iᴛ1 < m * n; iᴛ1 += n) {
        var i = iᴛ1;
        Ꮡg.Add(1);
        goǃ(() => {
            uint64 c = default;
            for (var @new = i; @new < i + n; @new++) {
                {
                    var old = Ꮡv.Swap(@new); if (old != nil) {
                        c += old._<uint64>();
                    }
                }
            }
            atomic.AddUint64(Ꮡcount, c);
            Ꮡg.Done();
        });
    }
    Ꮡg.Wait();
    {
        var (want, got) = ((m * n - 1) * (m * n) / 2, count + Ꮡv.Load()._<uint64>()); if (got != want) {
            t.Errorf("sum from 0 to %d was %d, want %v", m * n - 1, got, want);
        }
    }
}

partial struct heapAᴛ1 /*dyn*/ {
    /*embed*/ internal nuint @uint;
}
internal static heapAᴛ1 heapA = new heapAᴛ1(0);
internal static heapAᴛ1 heapB = new heapAᴛ1(0);

partial struct Value_CompareAndSwapTestsᴛ1 /*dyn*/ {
    internal any init;
    internal any @new;
    internal any old;
    internal bool want;
    internal any err;
}
public static readonly ж<slice<Value_CompareAndSwapTestsᴛ1>> ᏑValue_CompareAndSwapTests = new StaticBox<slice<Value_CompareAndSwapTestsᴛ1>>(slice<Value_CompareAndSwapTestsᴛ1>([
    new(init: default, @new: default, old: default, err: (@string)"sync/atomic: compare and swap of nil value into Value"),
    new(init: default, @new: true, old: (@string)""u8, err: (@string)"sync/atomic: compare and swap of inconsistently typed values into Value"),
    new(init: default, @new: true, old: true, want: false, err: default),
    new(init: default, @new: true, old: default, want: true, err: default),
    new(init: true, @new: (@string)""u8, err: (@string)"sync/atomic: compare and swap of inconsistently typed value into Value"),
    new(init: true, @new: true, old: false, want: false, err: default),
    new(init: true, @new: true, old: true, want: true, err: default),
    new(init: heapA, @new: new heapAᴛ1(1), old: heapB, want: true, err: default)
]));
public static ref slice<Value_CompareAndSwapTestsᴛ1> Value_CompareAndSwapTests => ref ((StaticBox<slice<Value_CompareAndSwapTestsᴛ1>>)ᏑValue_CompareAndSwapTests).ValueSlot;

public static void TestValue_CompareAndSwap(ж<testing.T> t) {
    foreach (var (i, vᴛ1) in Value_CompareAndSwapTests) {
        ref var tt = ref heap(new Value_CompareAndSwapTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(strconv.Itoa(i), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                ref var v = ref heap(new atomic.Value(), out var Ꮡv);
                if (ttʗ1.init != nil) {
                    Ꮡv.Store(ttʗ1.init);
                }
                var ttʗ2 = ttʗ1;
                defer(() => {
                    var err = recover();
                    switch (ᐧ) {
                    case {} when ttʗ2.err == nil && err != nil: {
                        tΔ1.Errorf("got %v, wanted no panic", err);
                        break;
                    }
                    case {} when ttʗ2.err != nil && err == nil: {
                        tΔ1.Errorf("did not panic, want %v", ttʗ2.err);
                        break;
                    }}
                }, ref ᒐ);
                {
                    var got = Ꮡv.CompareAndSwap(ttʗ1.old, ttʗ1.@new); if (got != ttʗ1.want) {
                        tΔ1.Errorf("got %v, want %v", got, ttʗ1.want);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static partial void TestValueCompareAndSwapConcurrent(ж<testing.T> t) {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    ref var w = ref heap(new sync.WaitGroup(), out var Ꮡw);
    Ꮡv.Store((nint)0);
    nint m = 1000;
    nint n = 100;
    if (testing.Short()) {
        m = 100;
        n = 100;
    }
    for (nint iᴛ1 = 0; iᴛ1 < m; iᴛ1++) {
        var i = iᴛ1;
        Ꮡw.Add(1);
        goǃ(() => {
            for (nint j = i; j < m * n; runtime.Gosched()) {
                if (Ꮡv.CompareAndSwap(j, j + 1)) {
                    j += m;
                }
            }
            Ꮡw.Done();
        });
    }
    Ꮡw.Wait();
    {
        nint stop = Ꮡv.Load()._<nint>(); if (stop != m * n) {
            t.Errorf("did not get to %v, stopped at %v", m * n, stop);
        }
    }
}

} // end atomic_test_package
