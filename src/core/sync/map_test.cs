// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using isync = @internal.sync_package;
using testenv = @internal.testenv_package;
using rand = go.math.rand_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using Δtesting = testing_package;
using quick = go.testing.quick_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.math;
using go.sync;
using go.testing;
using static go.sync_internal_test_package;

partial class sync_test_package {

partial struct mapOp /*@string*/;

internal static readonly mapOp opLoad = "Load"u8;
internal static readonly mapOp opStore = "Store"u8;
internal static readonly mapOp opLoadOrStore = "LoadOrStore"u8;
internal static readonly mapOp opLoadAndDelete = "LoadAndDelete"u8;
internal static readonly mapOp opDelete = "Delete"u8;
internal static readonly mapOp opSwap = "Swap"u8;
internal static readonly mapOp opCompareAndSwap = "CompareAndSwap"u8;
internal static readonly mapOp opCompareAndDelete = "CompareAndDelete"u8;
internal static readonly mapOp opClear = "Clear"u8;

internal static array<mapOp> mapOps = new mapOp[]{
    opLoad,
    opStore,
    opLoadOrStore,
    opLoadAndDelete,
    opDelete,
    opSwap,
    opCompareAndSwap,
    opCompareAndDelete,
    opClear
}.array();

// mapCall is a quick.Generator for calls on mapInterface.
partial struct mapCall {
    internal mapOp op;
    internal any k, v;
}

internal static (any, bool) apply(this mapCall c, mapInterface m) {
    var exprᴛ1 = c.op;
    if (exprᴛ1 == opLoad) {
        return m.Load(c.k);
    }
    if (exprᴛ1 == opStore) {
        m.Store(c.k, c.v);
        return (default!, false);
    }
    if (exprᴛ1 == opLoadOrStore) {
        return m.LoadOrStore(c.k, c.v);
    }
    if (exprᴛ1 == opLoadAndDelete) {
        return m.LoadAndDelete(c.k);
    }
    if (exprᴛ1 == opDelete) {
        m.Delete(c.k);
        return (default!, false);
    }
    if (exprᴛ1 == opSwap) {
        return m.Swap(c.k, c.v);
    }
    if (exprᴛ1 == opCompareAndSwap) {
        if (m.CompareAndSwap(c.k, c.v, rand.Int())) {
            m.Delete(c.k);
            return (c.v, true);
        }
        return (default!, false);
    }
    if (exprᴛ1 == opCompareAndDelete) {
        if (m.CompareAndDelete(c.k, c.v)) {
            {
                var (_, ok) = m.Load(c.k); if (!ok) {
                    return (default!, true);
                }
            }
        }
        return (default!, false);
    }
    if (exprᴛ1 == opClear) {
        m.Clear();
        return (default!, false);
    }
    { /* default: */
        throw panic("invalid mapOp");
    }

}

partial struct mapResult {
    internal any value;
    internal bool ok;
}

internal static any randValue(ж<rand.Rand> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    var b = new slice<byte>(r.Intn(4));
    foreach (var (i, _) in b) {
        b[i] = (byte)((rune)'a' + (byte)rand.Intn(26));
    }
    return ((@string)b);
}

internal static reflectꓸValue Generate(this mapCall _, ж<rand.Rand> Ꮡr, nint size) {
    var c = new mapCall(op: mapOps[rand.Intn(len(mapOps))], k: randValue(Ꮡr));
    var exprᴛ1 = c.op;
    if (exprᴛ1 == opStore || exprᴛ1 == opLoadOrStore) {
        c.v = randValue(Ꮡr);
    }

    return reflect.ValueOf(c);
}

internal static (slice<mapResult> results, map<any, any> final) applyCalls(mapInterface m, slice<mapCall> calls) {
    slice<mapResult> results = default!;
    map<any, any> final = default!;

    foreach (var (_, c) in calls) {
        var (v, ok) = c.apply(m);
        results = append(results, new mapResult(v, ok));
    }
    final = new map<any, any>();
    var finalʗ1 = final;
    m.Range((any k, any v) => {
        finalʗ1[k] = v;
        return true;
    });
    return (results, final);
}

internal static (slice<mapResult>, map<any, any>) applyMap(slice<mapCall> calls) {
    return applyCalls(new sync_test_package.sync_MapжmapInterface(@new<Δsync.Map>()), calls);
}

internal static (slice<mapResult>, map<any, any>) applyRWMutexMap(slice<mapCall> calls) {
    return applyCalls(new sync_test_package.RWMutexMapжmapInterface(@new<RWMutexMap>()), calls);
}

internal static (slice<mapResult>, map<any, any>) applyDeepCopyMap(slice<mapCall> calls) {
    return applyCalls(new sync_test_package.DeepCopyMapжmapInterface(@new<DeepCopyMap>()), calls);
}

internal static (slice<mapResult>, map<any, any>) applyHashTrieMap(slice<mapCall> calls) {
    return applyCalls(new sync_test_package.sync_HashTrieMapжmapInterface(@new<isync.HashTrieMap<any, any>>()), calls);
}

public static void TestMapMatchesRWMutex(ж<Δtesting.T> Ꮡt) {
    {
        var err = quick.CheckEqual(applyMap, applyRWMutexMap, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestMapMatchesDeepCopy(ж<Δtesting.T> Ꮡt) {
    {
        var err = quick.CheckEqual(applyMap, applyDeepCopyMap, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static void TestMapMatchesHashTrieMap(ж<Δtesting.T> Ꮡt) {
    {
        var err = quick.CheckEqual(applyMap, applyHashTrieMap, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

public static partial void TestConcurrentRange(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        UntypedInt mapSize = /* 1 << 10 */ 1024;
        var m = @new<Δsync.Map>();
        for (var n = (int64)1; n <= mapSize; n++) {
            m.Store(n, (int64)n);
        }
        var done = new channel<EmptyStruct>(0);
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        var doneʗ1 = done;
        defer(() => {
            close(doneʗ1);
            Ꮡwg.Wait();
        }, ref ᒐ);
        for (var g = (int64)Δruntime.GOMAXPROCS(0); g > 0; g--) {
            var r = rand.New(rand.NewSource(g));
            Ꮡwg.Add(1);
            var doneʗ2 = done;
            var mʗ1 = m;
            var rʗ1 = r;
            goǃ((int64 gΔ1) => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    for (var i = (int64)0; ᐧ ; i++) {
                        var selᴛ5 = doneʗ2;
                        switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
                        case 0 when selᴛ5.ꟷᐳ(out _): {
                            return;
                        }
                        default: {
                            break;
                        }}
                        for (var n = (int64)1; n < mapSize; n++) {
                            if (rʗ1.Int63n(mapSize) == 0){
                                mʗ1.Store(n, n * i * gΔ1);
                            } else {
                                mʗ1.Load(n);
                            }
                        }
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }, g);
        }
        nint iters = (1 << (int)(10));
        if (Δtesting.Short()) {
            iters = 16;
        }
        for (nint n = iters; n > 0; n--) {
            var seen = new map<int64, bool>(mapSize);
            var seenʗ1 = seen;
            m.Range((any ki, any vi) => {
                var (k, v) = (ki._<int64>(), vi._<int64>());
                if (rem(v, k) != 0) {
                    Ꮡt.Fatalf("while Storing multiples of %v, Range saw value %v"u8, k, v);
                }
                if (seenʗ1[k]) {
                    Ꮡt.Fatalf("Range visited key %v twice"u8, k);
                }
                seenʗ1[k] = true;
                return true;
            });
            if (len(seen) != mapSize) {
                Ꮡt.Fatalf("Range visited %v elements of %v-element Map"u8, len(seen), (nint)(mapSize));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue40999(ж<Δtesting.T> Ꮡt) {
    ref var m = ref heap(new Δsync.Map(), out var Ꮡm);
    // Since the miss-counting in missLocked (via Delete)
    // compares the miss count with len(m.dirty),
    // add an initial entry to bias len(m.dirty) above the miss count.
    Ꮡm.Store(default!, new EmptyStruct());
    ref var finalized = ref heap(new uint32(), out var Ꮡfinalized);
    // Set finalizers that count for collected keys. A non-zero count
    // indicates that keys have not been leaked.
    while (atomic.LoadUint32(Ꮡfinalized) == 0) {
        var p = @new<nint>();
        Δruntime.SetFinalizer(p.OrTypedNil(), (ж<nint> _) => {
            atomic.AddUint32(Ꮡfinalized, 1);
        });
        Ꮡm.Store(p.OrTypedNil(), new EmptyStruct());
        Ꮡm.Delete(p.OrTypedNil());
        Δruntime.GC();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object dummyˢ = (@string)"dummy"u8;
internal static readonly @string syncMapˢ = "sync.Map"u8;

public static void TestMapRangeNestedCall(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Issue 46399
    ref var m = ref heap(new Δsync.Map(), out var Ꮡm);
    foreach (var (i, v) in new @string[]{"hello"u8, "world"u8, "Go"u8}.array()) {
        Ꮡm.Store(i, v);
    }
    Ꮡm.Range((any key, any value) => {
        Ꮡm.Range((any keyΔ1, any valueΔ1) => {
            // We should be able to load the key offered in the Range callback,
            // because there are no concurrent Delete involved in this tested map.
            {
                var (v, ok) = Ꮡm.Load(keyΔ1); if (!ok || !reflect.DeepEqual(v, valueΔ1)) {
                    Ꮡt.Fatalf("Nested Range loads unexpected value, got %+v want %+v"u8, v, valueΔ1);
                }
            }
            // We didn't keep 42 and a value into the map before, if somehow we loaded
            // a value from such a key, meaning there must be an internal bug regarding
            // nested range in the Map.
            {
                var (_, loaded) = Ꮡm.LoadOrStore((nint)(42), dummyˢ); if (loaded) {
                    Ꮡt.Fatalf("Nested Range loads unexpected value, want store a new value"u8);
                }
            }
            // Try to Store then LoadAndDelete the corresponding value with the key
            // 42 to the Map. In this case, the key 42 and associated value should be
            // removed from the Map. Therefore any future range won't observe key 42
            // as we checked in above.
            @string val = syncMapˢ;
            Ꮡm.Store((nint)(42), val);
            {
                var (v, loaded) = Ꮡm.LoadAndDelete((nint)(42)); if (!loaded || !reflect.DeepEqual(v, val)) {
                    Ꮡt.Fatalf("Nested Range loads unexpected value, got %v, want %v"u8, v, val);
                }
            }
            return true;
        });
        // Remove key from Map on-the-fly.
        Ꮡm.Delete(key);
        return true;
    });
    // After a Range of Delete, all keys should be removed and any
    // further Range won't invoke the callback. Hence length remains 0.
    nint length = 0;
    Ꮡm.Range((any key, any value) => {
        length++;
        return true;
    });
    if (length != 0) {
        Ꮡt.Fatalf("Unexpected sync.Map size, got %v want %v"u8, length, (nint)(0));
    }
}

public static void TestCompareAndSwap_NonExistingKey(ж<Δtesting.T> Ꮡt) {
    var m = Ꮡ(new Δsync.Map(nil));
    if (m.CompareAndSwap(m.OrTypedNil(), default!, (nint)(42))) {
        // See https://go.dev/issue/51972#issuecomment-1126408637.
        Ꮡt.Fatalf("CompareAndSwap on a non-existing key succeeded"u8);
    }
}

public static void TestMapRangeNoAllocations(ж<Δtesting.T> Ꮡt) {
    // Issue 62404
    testenv.SkipIfOptimizationOff(new sync_test_package.testing_TжTB(Ꮡt));
    ref var m = ref heap(new Δsync.Map(), out var Ꮡm);
    var allocs = Δtesting.AllocsPerRun(10, () => {
        Ꮡm.Range((any key, any value) => true);
    });
    if (allocs > 0D) {
        Ꮡt.Errorf("AllocsPerRun of m.Range = %v; want 0"u8, allocs);
    }
}

// TestConcurrentClear tests concurrent behavior of sync.Map properties to ensure no data races.
// Checks for proper synchronization between Clear, Store, Load operations.
public static partial void TestConcurrentClear(ж<Δtesting.T> Ꮡt) {
    ref var m = ref heap(new Δsync.Map(), out var Ꮡm);
    ref var wg = ref heap<Δsync.WaitGroup>(out var Ꮡwg);
    wg = new Δsync.WaitGroup(nil);
    Ꮡwg.Add(30); // 10 goroutines for writing, 10 goroutines for reading, 10 goroutines for waiting
    // Writing data to the map concurrently
    for (nint i = 0; i < 10; i++) {
        goǃ((nint k, nint v) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                Ꮡm.Store(k, v);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i, i * 10);
    }
    // Reading data from the map concurrently
    for (nint i = 0; i < 10; i++) {
        goǃ((nint k) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                {
                    var (value, ok) = Ꮡm.Load(k); if (ok){
                        Ꮡt.Logf("Key: %v, Value: %v\n"u8, k, value);
                    } else {
                        Ꮡt.Logf("Key: %v not found\n"u8, k);
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }
    // Clearing data from the map concurrently
    for (nint i = 0; i < 10; i++) {
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                Ꮡm.Clear();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
    Ꮡm.Clear();
    Ꮡm.Range((any k, any v) => {
        Ꮡt.Errorf("after Clear, Map contains (%v, %v); expected to be empty"u8, k, v);
        return true;
    });
}

public static void TestMapClearOneAllocation(ж<Δtesting.T> Ꮡt) {
    testenv.SkipIfOptimizationOff(new sync_test_package.testing_TжTB(Ꮡt));
    ref var m = ref heap(new Δsync.Map(), out var Ꮡm);
    var allocs = Δtesting.AllocsPerRun(10, () => {
        Ꮡm.Clear();
    });
    if (allocs > 1D) {
        Ꮡt.Errorf("AllocsPerRun of m.Clear = %v; want 1"u8, allocs);
    }
}

} // end sync_test_package
