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
using go.sync;
using math;

partial class atomic_test_package {

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸmathꓸrand() {
    builtin.initPackage(typeof(math.rand_package));
}

// Go runs an imported package's `init` before this package's own; .NET would never load
// an assembly nothing has touched yet, so that initialization is forced here.
[GoInit] internal static void initᴛᴛimportꓸstrconv() {
    builtin.initPackage(typeof(strconv_package));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object initialValueIsNotNilˢ = (@string)"initial Value is not nil"u8;

public static void TestValue(ж<testing.T> Ꮡt) {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    if (Ꮡv.Load() != default!) {
        Ꮡt.Fatal(initialValueIsNotNilˢ);
    }
    Ꮡv.Store((nint)(42));
    var x = Ꮡv.Load();
    {
        var (xx, ok) = x._<nint>(ᐧ); if (!ok || xx != 42) {
            Ꮡt.Fatalf("wrong value: got %+v, want 42"u8, x);
        }
    }
    Ꮡv.Store((nint)(84));
    x = Ꮡv.Load();
    {
        var (xx, ok) = x._<nint>(ᐧ); if (!ok || xx != 84) {
            Ꮡt.Fatalf("wrong value: got %+v, want 84"u8, x);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fooˢ = (@string)"foo"u8;
private static readonly object barbazˢ = (@string)"barbaz"u8;

public static void TestValueLarge(ж<testing.T> Ꮡt) {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    Ꮡv.Store(fooˢ);
    var x = Ꮡv.Load();
    {
        var (xx, ok) = x._<@string>(ᐧ); if (!ok || xx != "foo"u8) {
            Ꮡt.Fatalf("wrong value: got %+v, want foo"u8, x);
        }
    }
    Ꮡv.Store(barbazˢ);
    x = Ꮡv.Load();
    {
        var (xx, ok) = x._<@string>(ᐧ); if (!ok || xx != "barbaz"u8) {
            Ꮡt.Fatalf("wrong value: got %+v, want barbaz"u8, x);
        }
    }
}

public static void TestValuePanic(ж<testing.T> Ꮡt) {
    @string nilErr = "sync/atomic: store of nil value into Value"u8;
    @string badErr = "sync/atomic: store of inconsistently typed value into Value"u8;
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var err = recover();
                if (!AreEqual(err, nilErr)) {
                    Ꮡt.Fatalf("inconsistent store panic: got '%v', want '%v'"u8, err, nilErr);
                }
            }, ref ᒐ);
            Ꮡv.Store(default!);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    Ꮡv.Store((nint)(42));
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var err = recover();
                if (!AreEqual(err, badErr)) {
                    Ꮡt.Fatalf("inconsistent store panic: got '%v', want '%v'"u8, err, badErr);
                }
            }, ref ᒐ);
            Ꮡv.Store(fooˢ);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var err = recover();
                if (!AreEqual(err, nilErr)) {
                    Ꮡt.Fatalf("inconsistent store panic: got '%v', want '%v'"u8, err, nilErr);
                }
            }, ref ᒐ);
            Ꮡv.Store(default!);
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
}

public static void TestValueConcurrent(ж<testing.T> Ꮡt) {
    var tests = new slice<any>[]{
        new any[]{(uint16)0, unchecked((uint16)(~(uint16)0)), (uint16)(1 + (2 << (int)(8))), (uint16)(3 + (4 << (int)(8)))}.slice(),
        new any[]{(uint32)0, ~(uint32)0, (uint32)(1 + (2 << (int)(16))), (uint32)(3 + (4 << (int)(16)))}.slice(),
        new any[]{(uint64)0, ~(uint64)0, (uint64)(1 + 8589934592L), (uint64)(3 + 17179869184L)}.slice(),
        new any[]{complex(0D, 0D), complex(1D, 2D), complex(3D, 4D), complex(5D, 6D)}.slice()
    }.slice();
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
                    Ꮡt.Logf("loaded unexpected value %+v, want %+v"u8, x, testʗ1);
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
                Ꮡt.FailNow();
            }
        }
    }
}

public static void BenchmarkValueRead(ж<testing.B> Ꮡb) {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    Ꮡv.Store(@new<nint>());
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            var x = Ꮡv.Load()._<ж<nint>>();
            if (x.Value != 0) {
                Ꮡb.Fatalf("wrong value: got %v, want 0"u8, x.Value);
            }
        }
    });
}


[GoType("dyn")] partial struct Value_SwapTestsᴛ1 {
    internal any init;
    internal any @new;
    internal any want;
    internal any err;
}
public static slice<Value_SwapTestsᴛ1> Value_SwapTests = new Value_SwapTestsᴛ1[]{
    new(init: default!, @new: default!, err: (@string)"sync/atomic: swap of nil value into Value"u8),
    new(init: default!, @new: true, want: default!, err: default!),
    new(init: true, @new: (@string)""u8, err: (@string)"sync/atomic: swap of inconsistently typed value into Value"u8),
    new(init: true, @new: false, want: true, err: default!)
}.slice();

public static void TestValue_Swap(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in Value_SwapTests) {
        ref var tt = ref heap(new Value_SwapTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(strconv.Itoa(i), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                ref var v = ref heap(new atomic.Value(), out var Ꮡv);
                if (ttʗ1.init != default!) {
                    Ꮡv.Store(ttʗ1.init);
                }
                var ttʗ2 = ttʗ1;
                defer(() => {
                    var err = recover();
                    switch (ᐧ) {
                    case {} when ttʗ2.err == default! && err != default!: {
                        tΔ1.Errorf("should not panic, got %v"u8, err);
                        break;
                    }
                    case {} when ttʗ2.err != default! && err == default!: {
                        tΔ1.Errorf("should panic %v, got <nil>"u8, ttʗ2.err);
                        break;
                    }}

                }, ref ᒐ);
                {
                    var got = Ꮡv.Swap(ttʗ1.@new); if (!AreEqual(got, ttʗ1.want)) {
                        tΔ1.Errorf("got %v, want %v"u8, got, ttʗ1.want);
                    }
                }
                {
                    var got = Ꮡv.Load(); if (!AreEqual(got, ttʗ1.@new)) {
                        tΔ1.Errorf("got %v, want %v"u8, got, ttʗ1.@new);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestValueSwapConcurrent(ж<testing.T> Ꮡt) {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    ref var count = ref heap(new uint64(), out var Ꮡcount);
    ref var g = ref heap(new sync.WaitGroup(), out var Ꮡg);
    uint64 m = 10000;
    uint64 n = 10000;
    if (testing.Short()) {
        m = 1000;
        n = 1000;
    }
    for (var i = (uint64)0; i < m * n; i += n) {
        var iΔ1 = i;
        Ꮡg.Add(1);
        goǃ(() => {
            uint64 c = default!;
            for (var @new = iΔ1; @new < iΔ1 + n; @new++) {
                {
                    var old = Ꮡv.Swap(@new); if (old != default!) {
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
            Ꮡt.Errorf("sum from 0 to %d was %d, want %v"u8, m * n - 1, got, want);
        }
    }
}


[GoType("dyn")] partial struct heapAᴛ1 {
    [GoEmbedded] internal nuint @uint;
}
internal static heapAᴛ1 heapA = new heapAᴛ1(0);
internal static heapAᴛ1 heapB = new heapAᴛ1(0);


[GoType("dyn")] partial struct Value_CompareAndSwapTestsᴛ1 {
    internal any init;
    internal any @new;
    internal any old;
    internal bool want;
    internal any err;
}
public static slice<Value_CompareAndSwapTestsᴛ1> Value_CompareAndSwapTests = new Value_CompareAndSwapTestsᴛ1[]{
    new(init: default!, @new: default!, old: default!, err: (@string)"sync/atomic: compare and swap of nil value into Value"u8),
    new(init: default!, @new: true, old: (@string)""u8, err: (@string)"sync/atomic: compare and swap of inconsistently typed values into Value"u8),
    new(init: default!, @new: true, old: true, want: false, err: default!),
    new(init: default!, @new: true, old: default!, want: true, err: default!),
    new(init: true, @new: (@string)""u8, err: (@string)"sync/atomic: compare and swap of inconsistently typed value into Value"u8),
    new(init: true, @new: true, old: false, want: false, err: default!),
    new(init: true, @new: true, old: true, want: true, err: default!),
    new(init: heapA, @new: new heapAᴛ1(1), old: heapB, want: true, err: default!)
}.slice();

public static void TestValue_CompareAndSwap(ж<testing.T> Ꮡt) {
    foreach (var (i, vᴛ1) in Value_CompareAndSwapTests) {
        ref var tt = ref heap(new Value_CompareAndSwapTestsᴛ1(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(strconv.Itoa(i), (ж<testing.T> tΔ1) => {
            GoFrame ᒐ = default;
            try {
                ref var v = ref heap(new atomic.Value(), out var Ꮡv);
                if (ttʗ1.init != default!) {
                    Ꮡv.Store(ttʗ1.init);
                }
                var ttʗ2 = ttʗ1;
                defer(() => {
                    var err = recover();
                    switch (ᐧ) {
                    case {} when ttʗ2.err == default! && err != default!: {
                        tΔ1.Errorf("got %v, wanted no panic"u8, err);
                        break;
                    }
                    case {} when ttʗ2.err != default! && err == default!: {
                        tΔ1.Errorf("did not panic, want %v"u8, ttʗ2.err);
                        break;
                    }}

                }, ref ᒐ);
                {
                    var got = Ꮡv.CompareAndSwap(ttʗ1.old, ttʗ1.@new); if (got != ttʗ1.want) {
                        tΔ1.Errorf("got %v, want %v"u8, got, ttʗ1.want);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
}

public static void TestValueCompareAndSwapConcurrent(ж<testing.T> Ꮡt) {
    ref var v = ref heap(new atomic.Value(), out var Ꮡv);
    ref var w = ref heap(new sync.WaitGroup(), out var Ꮡw);
    Ꮡv.Store((nint)(0));
    nint m = 1000;
    nint n = 100;
    if (testing.Short()) {
        m = 100;
        n = 100;
    }
    for (nint i = 0; i < m; i++) {
        nint iΔ1 = i;
        Ꮡw.Add(1);
        goǃ(() => {
            for (nint j = iΔ1; j < m * n; runtime.Gosched()) {
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
            Ꮡt.Errorf("did not get to %v, stopped at %v"u8, m * n, stop);
        }
    }
}

} // end atomic_test_package
