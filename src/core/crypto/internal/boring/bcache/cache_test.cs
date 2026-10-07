// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.boring;

using fmt = fmt_package;
using runtime = runtime_package;
using sync = sync_package;
using atomic = go.sync.atomic_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.sync;
using static go.crypto.@internal.boring.bcache_package;

partial class bcache_internal_test_package {

internal static ж<global::go.crypto.@internal.boring.bcache_package.Cache<nint, int32>> ᏑregisteredCache = new StandardBox<global::go.crypto.@internal.boring.bcache_package.Cache<nint, int32>>(default(global::go.crypto.@internal.boring.bcache_package.Cache<nint, int32>));
internal static ref global::go.crypto.@internal.boring.bcache_package.Cache<nint, int32> registeredCache => ref ᏑregisteredCache.Value;

[GoInit] internal static void init() {
    ᏑregisteredCache.Register();
}

internal static ж<atomic.Uint32> Ꮡseq = new StandardBox<atomic.Uint32>(default(atomic.Uint32));
internal static ref atomic.Uint32 seq => ref Ꮡseq.Value;

internal static ж<T> next<T>()
    where T : /* int | int32 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    var x = @new<T>();
    x.ValueSlot = ConvertToType<T>(Ꮡseq.Add(1));
    return x;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ = "nil"u8;

internal static @string str<T>(ж<T> Ꮡx)
    where T : /* int | int32 */ IAdditionOperators<T, T, T>, ISubtractionOperators<T, T, T>, IMultiplyOperators<T, T, T>, IDivisionOperators<T, T, T>, IIncrementOperators<T>, IDecrementOperators<T>, IUnaryNegationOperators<T, T>, IModulusOperators<T, T, T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    ref var x = ref Ꮡx.DerefOrNull();

    if (Ꮡx == nil) {
        return nilˢ;
    }
    return fmt.Sprint(x);
}

public static partial void TestCache(ж<testing.T> Ꮡt) {
    // Use unregistered cache for functionality tests,
    // to keep the runtime from clearing behind our backs.
    var c = @new<global::go.crypto.@internal.boring.bcache_package.Cache<nint, int32>>();
    // Create many entries.
    var m = new map<ж<nint>, ж<int32>>();
    for (nint i = 0; i < 10000; i++) {
        var k = next<nint>();
        var v = next<int32>();
        m[k] = v;
        c.Put(k, v);
    }
    // Overwrite a random 20% of those.
    nint n = 0;
    foreach (var (k, _) in m) {
        var v = next<int32>();
        m[k] = v;
        c.Put(k, v);
        {
            n++; if (n >= 2000) {
                break;
            }
        }
    }
    // Check results.
    foreach (var (k, v) in m) {
        {
            var cv = c.Get(k); if (cv != v) {
                Ꮡt.Fatalf("c.Get(%v) = %v, want %v"u8, str(k), str(cv), str(v));
            }
        }
    }
    c.Clear();
    foreach (var (k, _) in m) {
        {
            var cv = c.Get(k); if (cv != nil) {
                Ꮡt.Fatalf("after GC, c.Get(%v) = %v, want nil"u8, str(k), str(cv));
            }
        }
    }
    // Check that registered cache is cleared at GC.
    c = ᏑregisteredCache;
    foreach (var (k, v) in m) {
        c.Put(k, v);
    }
    runtime.GC();
    foreach (var (k, _) in m) {
        {
            var cv = c.Get(k); if (cv != nil) {
                Ꮡt.Fatalf("after Clear, c.Get(%v) = %v, want nil"u8, str(k), str(cv));
            }
        }
    }
    // Check that cache works for concurrent access.
    // Lists are discarded if they reach 1000 entries,
    // and there are cacheSize list heads, so we should be
    // able to do 100 * cacheSize entries with no problem at all.
    c = @new<global::go.crypto.@internal.boring.bcache_package.Cache<nint, int32>>();
    ref var barrier = ref heap(new sync.WaitGroup(), out var Ꮡbarrier);
    ref var wg = ref heap(new sync.WaitGroup(), out var Ꮡwg);
    const nint N = 100;
    Ꮡbarrier.Add(N);
    Ꮡwg.Add(N);
    ref var lost = ref heap(new int32(), out var Ꮡlost);
    for (nint i = 0; i < N; i++) {
        var cʗ1 = c;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var mΔ1 = new map<ж<nint>, ж<int32>>();
                for (nint j = 0; j < cacheSize; j++) {
                    var (k, v) = (next<nint>(), next<int32>());
                    mΔ1[k] = v;
                    cʗ1.Put(k, v);
                }
                Ꮡbarrier.Done();
                Ꮡbarrier.Wait();
                foreach (var (k, v) in mΔ1) {
                    {
                        var cv = cʗ1.Get(k); if (cv != v) {
                            Ꮡt.Errorf("c.Get(%v) = %v, want %v"u8, str(k), str(cv), str(v));
                            atomic.AddInt32(Ꮡlost, +1);
                        }
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
    if (lost != 0) {
        Ꮡt.Errorf("lost %d entries"u8, lost);
    }
}

} // end bcache_internal_test_package
