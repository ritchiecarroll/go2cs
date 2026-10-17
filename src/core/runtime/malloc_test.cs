// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using flag = flag_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using race = @internal.race_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = global::go.os.exec_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using static runtime_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.os;
using global::go.sync;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static nint testMemStatsCount;

public static void TestMemStats(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testMemStatsCount++;

    // Make sure there's at least one forced GC.
    GC();

    // Test that MemStats has sane values.
    var st = @new<Δruntime.MemStats>();
    ReadMemStats(st);

    var nz = error (any x) => {
        if (!AreEqual(x, reflect.Zero(reflect.TypeOf(x)).Interface())) {
            return default!;
        }
        return fmt.Errorf("zero value"u8);
    };
    Func<any, error> le(float64 thresh) => error (any x) => {
            // These sanity tests aren't necessarily valid
            // with high -test.count values, so only run
            // them once.
            if (testMemStatsCount > 1) {
                return default!;
            }

            if (reflect.ValueOf(x).Convert(reflect.TypeOf(thresh)).Float() < thresh) {
                return default!;
            }
            return fmt.Errorf("insanely high value (overflow?); want <= %v"u8, thresh);
        };
    Func<any, error> eq(any x) => error (any y) => {
            if (AreEqual(x, y)) {
                return default!;
            }
            return fmt.Errorf("want %v"u8, x);
        };
    // Of the uint fields, HeapReleased, HeapIdle can be 0.
    // PauseTotalNs can be 0 if timer resolution is poor.
    var fields = new map<@string, slice<Func<any, error>>>{
        ["Alloc"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["TotalAlloc"u8] = new Func<any, error>[]{nz, le(1e11D)}.slice(), ["Sys"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(),
        ["Lookups"u8] = new Func<any, error>[]{eq((uint64)0)}.slice(), ["Mallocs"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["Frees"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(),
        ["HeapAlloc"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["HeapSys"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["HeapIdle"u8] = new Func<any, error>[]{le(1e10D)}.slice(),
        ["HeapInuse"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["HeapReleased"u8] = new Func<any, error>[]{le(1e10D)}.slice(), ["HeapObjects"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(),
        ["StackInuse"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["StackSys"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(),
        ["MSpanInuse"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["MSpanSys"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(),
        ["MCacheInuse"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["MCacheSys"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(),
        ["BuckHashSys"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["GCSys"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["OtherSys"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(),
        ["NextGC"u8] = new Func<any, error>[]{nz, le(1e10D)}.slice(), ["LastGC"u8] = new Func<any, error>[]{nz}.slice(),
        ["PauseTotalNs"u8] = new Func<any, error>[]{le(1e11D)}.slice(), ["PauseNs"u8] = default!, ["PauseEnd"u8] = default!,
        ["NumGC"u8] = new Func<any, error>[]{nz, le(1e9D)}.slice(), ["NumForcedGC"u8] = new Func<any, error>[]{nz, le(1e9D)}.slice(),
        ["GCCPUFraction"u8] = new Func<any, error>[]{le(0.99D)}.slice(), ["EnableGC"u8] = new Func<any, error>[]{eq(true)}.slice(), ["DebugGC"u8] = new Func<any, error>[]{eq(false)}.slice(),
        ["BySize"u8] = default!
    };

    var rst = reflect.ValueOf(st.OrTypedNil()).Elem();
    for (nint i = 0; i < rst.Type().NumField(); i++) {
        @string name = rst.Type().Field(i).Name;
        var val = rst.Field(i).Interface();
        var (checks, ok) = fields[name, ꟷ];
        if (!ok) {
            Ꮡt.Errorf("unknown MemStats field %s"u8, name);
            continue;
        }
        foreach (var (_, check) in checks) {
            {
                var err = check(val); if (err != default!) {
                    Ꮡt.Errorf("%s = %v: %s"u8, name, val, err);
                }
            }
        }
    }

    if ((~st).Sys != (~st).HeapSys + (~st).StackSys + (~st).MSpanSys + (~st).MCacheSys + (~st).BuckHashSys + (~st).GCSys + (~st).OtherSys) {
        Ꮡt.Fatalf("Bad sys value: %+v"u8, st.Value);
    }

    if ((~st).HeapIdle + (~st).HeapInuse != (~st).HeapSys) {
        Ꮡt.Fatalf("HeapIdle(%d) + HeapInuse(%d) should be equal to HeapSys(%d), but isn't."u8, (~st).HeapIdle, (~st).HeapInuse, (~st).HeapSys);
    }

    {
        var lpe = (~st).PauseEnd[(nint)((~st).NumGC + 255) % len((~st).PauseEnd)]; if ((~st).LastGC != lpe) {
            Ꮡt.Fatalf("LastGC(%d) != last PauseEnd(%d)"u8, (~st).LastGC, lpe);
        }
    }

    uint64 pauseTotal = default!;
    foreach (var (_, pause) in (~st).PauseNs.ΔRangeSnapshot()) {
        pauseTotal += pause;
    }
    if ((nint)(~st).NumGC < len((~st).PauseNs)){
        // We have all pauses, so this should be exact.
        if ((~st).PauseTotalNs != pauseTotal) {
            Ꮡt.Fatalf("PauseTotalNs(%d) != sum PauseNs(%d)"u8, (~st).PauseTotalNs, pauseTotal);
        }
        for (nint i = (nint)(~st).NumGC; i < len((~st).PauseNs); i++) {
            if ((~st).PauseNs[i] != 0) {
                Ꮡt.Fatalf("Non-zero PauseNs[%d]: %+v"u8, i, st.OrTypedNil());
            }
            if ((~st).PauseEnd[i] != 0) {
                Ꮡt.Fatalf("Non-zero PauseEnd[%d]: %+v"u8, i, st.OrTypedNil());
            }
        }
    } else {
        if ((~st).PauseTotalNs < pauseTotal) {
            Ꮡt.Fatalf("PauseTotalNs(%d) < sum PauseNs(%d)"u8, (~st).PauseTotalNs, pauseTotal);
        }
    }

    if ((~st).NumForcedGC > (~st).NumGC) {
        Ꮡt.Fatalf("NumForcedGC(%d) > NumGC(%d)"u8, (~st).NumForcedGC, (~st).NumGC);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string foo0123456789ˢ = "foo0123456789"u8;

public static void TestStringConcatenationAllocs(ж<testing.T> Ꮡt) {
    var n = testing.AllocsPerRun(1000, () => {
        var b = new slice<byte>(10);
        for (nint i = 0; i < 10; i++) {
            b[i] = (byte)((byte)i + (rune)'0');
        }
        @string s = "foo"u8 + ((sstring)b);
        {
            @string want = foo0123456789ˢ; if (s != want) {
                Ꮡt.Fatalf("want %v, got %v"u8, want, s);
            }
        }
    });
    // Only string concatenation allocates.
    if (n != 1D) {
        Ꮡt.Fatalf("want 1 allocation, got %v"u8, n);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tinyallocSuppressedWhenˢ = (@string)"tinyalloc suppressed when running in race mode"u8;
internal static readonly object tinyallocSuppressedWhenˢ2 = (@string)"tinyalloc suppressed when running in asan mode due to redzone"u8;
internal static readonly object noBytesAllocatedWithinˢ = (@string)"no bytes allocated within the same 8-byte chunk"u8;

public static void TestTinyAlloc(ж<testing.T> Ꮡt) {
    if (runtime_internal_test_package.Raceenabled) {
        Ꮡt.Skip(tinyallocSuppressedWhenˢ);
    }
    if (asan.Enabled) {
        Ꮡt.Skip(tinyallocSuppressedWhenˢ2);
    }
    UntypedInt N = 16;
    array<@unsafe.Pointer> v = new(16); /* N */
    foreach (var (i, _) in v) {
        v[i] = @unsafe.Pointer.FromPinnedBox(@new<byte>());
    }

    var chunks = new map<uintptr, bool>(N);
    foreach (var (_, p) in v.ΔRangeSnapshot()) {
        chunks[(uintptr)((uintptr)p & ~(uintptr)7)] = true;
    }

    if (len(chunks) == N) {
        Ꮡt.Fatal(noBytesAllocatedWithinˢ);
    }
}

partial struct obj12 {
    internal uint64 a;
    internal uint32 b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unableToGetAFreshTinyˢ = (@string)"unable to get a fresh tiny slot"u8;

public static void TestTinyAllocIssue37262(ж<testing.T> Ꮡt) {
    if (runtime_internal_test_package.Raceenabled) {
        Ꮡt.Skip(tinyallocSuppressedWhenˢ);
    }
    if (asan.Enabled) {
        Ꮡt.Skip(tinyallocSuppressedWhenˢ2);
    }
    // Try to cause an alignment access fault
    // by atomically accessing the first 64-bit
    // value of a tiny-allocated object.
    // See issue 37262 for details.

    // GC twice, once to reach a stable heap state
    // and again to make sure we finish the sweep phase.
    Δruntime.GC();
    Δruntime.GC();

    // Disable preemption so we stay on one P's tiny allocator and
    // nothing else allocates from it.
    runtime_internal_test_package.Acquirem();

    // Make 1-byte allocations until we get a fresh tiny slot.
    var aligned = false;
    for (nint i = 0; i < 16; i++) {
        var x = runtime_internal_test_package.Escape(@new<byte>());
        if ((uintptr)((uintptr)x & 0xf) == 0xf) {
            aligned = true;
            break;
        }
    }
    if (!aligned) {
        runtime_internal_test_package.Releasem();
        Ꮡt.Fatal(unableToGetAFreshTinyˢ);
    }

    // Create a 4-byte object so that the current
    // tiny slot is partially filled.
    runtime_internal_test_package.Escape(@new<uint32>());

    // Create a 12-byte object, which fits into the
    // tiny slot. If it actually gets place there,
    // then the field "a" will be improperly aligned
    // for atomic access on 32-bit architectures.
    // This won't be true if issue 36606 gets resolved.
    var tinyObj12 = runtime_internal_test_package.Escape(@new<obj12>());

    // Try to atomically access "x.a".
    atomic.StoreUint64(tinyObj12.of(obj12.Ꮡa), 10);

    runtime_internal_test_package.Releasem();
}

internal partial struct TestFreegc_tests /*dyn*/ {
    internal @string size;
    internal Func<bool, Action<ж<testing.T>>> f;
    internal bool noscan;
}

// TestFreegc does basic testing of explicit frees.
public static void TestFreegc(ж<testing.T> Ꮡt) {
    var tests = new TestFreegc_tests[]{
        // Types without pointers.
        new("size=16"u8, testFreegc<array<byte>>, true), // smallest we support currently
        new("size=17"u8, testFreegc<array<byte>>, true),
        new("size=64"u8, testFreegc<array<byte>>, true),
        new("size=500"u8, testFreegc<array<byte>>, true),
        new("size=512"u8, testFreegc<array<byte>>, true),
        new("size=4096"u8, testFreegc<array<byte>>, true),
        new("size=20000"u8, testFreegc<array<byte>>, true), // not power of 2 or spc boundary
        new("size=32KiB-8"u8, testFreegc<array<byte>>, true) // max noscan small object for 64-bit
    }.slice();

    // Run the tests twice if not in -short mode or not otherwise saving test time.
    // First while manually calling runtime.GC to slightly increase isolation (perhaps making
    // problems more reproducible).
    foreach (var (_, tt) in tests) {
        Δruntime.GC();
        Ꮡt.Run(fmt.Sprintf("gc=yes/ptrs=%v/%s"u8, !tt.noscan, tt.size), tt.f(tt.noscan));
    }
    Δruntime.GC();

    if (testing.Short() || !runtime_internal_test_package.RuntimeFreegcEnabled || runtime_internal_test_package.Raceenabled) {
        return;
    }

    // Again, but without manually calling runtime.GC in the loop (perhaps less isolation might
    // trigger problems).
    foreach (var (_, tt) in tests) {
        Ꮡt.Run(fmt.Sprintf("gc=no/ptrs=%v/%s"u8, !tt.noscan, tt.size), tt.f(tt.noscan));
    }
    Δruntime.GC();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string basicFreeˢ = "basic-free"u8;
internal static readonly @string stackFreeˢ = "stack-free"u8;
internal static readonly @string allocsBaselineˢ = "allocs-baseline"u8;
internal static readonly @string allocsWithFreeˢ = "allocs-with-free"u8;
internal static readonly object temporarilySkippingAllocˢ = (@string)"temporarily skipping alloc tests for GOEXPERIMENT=sizespecializedmalloc for pointer types"u8;
internal static readonly object skippingAllocTestsWithˢ = (@string)"skipping alloc tests with runtime.freegc disabled"u8;
internal static readonly @string freeMultipleˢ = "free-multiple"u8;
internal static readonly object skippingRemainingFreegcˢ = (@string)"skipping remaining freegc tests, was timing out on wasm"u8;
internal static readonly @string freeManyˢ = "free-many"u8;
internal static readonly @string duplicateCheckˢ = "duplicate-check"u8;
internal static readonly @string freeOtherGoroutineˢ = "free-other-goroutine"u8;
internal static readonly @string manyGoroutinesˢ = "many-goroutines"u8;
internal static readonly @string assistCreditˢ = "assist-credit"u8;
internal static readonly object temporarilySkipAssistˢ = (@string)"temporarily skip assist credit tests for GOEXPERIMENT=sizespecializedmalloc for pointer types"u8;
internal static readonly object skippingAssistCreditTestˢ = (@string)"skipping assist credit test with runtime.freegc disabled"u8;

internal static Action<ж<testing.T>> testFreegc<T>(bool noscan) {
    // We use stressMultiple to influence the duration of the tests.
    // When testing freegc changes, stressMultiple can be increased locally
    // to test longer or in some cases with more goroutines.
    // It can also be helpful to test with GODEBUG=clobberfree=1 and
    // with and without doubleCheckMalloc and doubleCheckReusable enabled.
    nint stressMultiple = 10;
    if (testing.Short() || !runtime_internal_test_package.RuntimeFreegcEnabled || runtime_internal_test_package.Raceenabled) {
        stressMultiple = 1;
    }

    return (ж<testing.T> t) => {
        ж<T> alloc() {
            // Force heap alloc, plus some light validation of zeroed memory.
            t.Helper();
            var p = runtime_internal_test_package.Escape(@new<T>());
            T zero = GoZero<T>();
            if (!AreEqual(p.ValueSlot, zero)) {
                t.Fatalf("allocator returned non-zero memory: %v"u8, (p.ValueSlot).OrTypedNilOf());
            }
            return p;
        }

        void free(ж<T> p) {
            t.Helper();
            T zero = GoZero<T>();
            if (!AreEqual(p.ValueSlot, zero)) {
                t.Fatalf("found non-zero memory before freegc (tests do not modify memory): %v"u8, (p.ValueSlot).OrTypedNilOf());
            }
            runtime_internal_test_package.Freegc(@unsafe.Pointer.FromPinnedBox(p), @unsafe.Sizeof(p.ValueSlot), noscan);
        }

        var allocʗ1 = alloc;
        var freeʗ1 = free;
        t.Run(basicFreeˢ, (ж<testing.T> tΔ1) => {
            // Test that freeing a live heap object doesn't crash.
            foreach (var _ᴛ1 in range(100)) {
                var p = allocʗ1();
                freeʗ1(p);
            }
        });

        t.Run(stackFreeˢ, (ж<testing.T> tΔ2) => {
            // Test that freeing a stack object doesn't crash.
            foreach (var _ᴛ2 in range(100)) {
                ref var x = ref heap(new array<byte>(32), out var Ꮡx);
                ref var y = ref heap(new array<ж<nint>>(32), out var Ꮡy);
                runtime_internal_test_package.Freegc(@unsafe.Pointer.FromPinnedBox(Ꮡx), /* unsafe.Sizeof(x) */ (uintptr)32, true); // noscan
                runtime_internal_test_package.Freegc(@unsafe.Pointer.FromPinnedBox(Ꮡy), /* unsafe.Sizeof(y) */ (uintptr)256, false); // !noscan
            }
        });

        // Check our allocations. These tests rely on the
        // current implementation treating a re-used object
        // as not adding to the allocation counts seen
        // by testing.AllocsPerRun. (This is not the desired
        // long-term behavior, but it is the current behavior and
        // makes these tests convenient).

        var allocʗ2 = alloc;
        t.Run(allocsBaselineˢ, (ж<testing.T> tΔ3) => {
            // Baseline result without any explicit free.
            var allocʗ3 = allocʗ2;
            var allocs = testing.AllocsPerRun(100, () => {
                foreach (var _ᴛ3 in range(100)) {
                    var p = allocʗ3();
                    _ = p;
                }
            });
            if (allocs < 100D) {
                // TODO(thepudds): we get exactly 100 for almost all the tests, but investigate why
                // ~101 allocs for TestFreegc/ptrs=true/size=32KiB-8.
                tΔ3.Fatalf("expected >=100 allocations, got %v"u8, allocs);
            }
        });

        var allocʗ4 = alloc;
        var freeʗ2 = free;
        t.Run(allocsWithFreeˢ, (ж<testing.T> tΔ4) => {
            // Same allocations, but now using explicit free so that
            // no allocs get reported. (Again, not the desired long-term behavior).
            if (runtime_internal_test_package.SizeSpecializedMallocEnabled && !noscan) {
                // TODO(thepudds): skip at this point in the stack for size-specialized malloc
                // with !noscan. Additional integration with sizespecializedmalloc is in a later CL.
                tΔ4.Skip(temporarilySkippingAllocˢ);
            }
            if (!runtime_internal_test_package.RuntimeFreegcEnabled) {
                tΔ4.Skip(skippingAllocTestsWithˢ);
            }
            var allocʗ5 = allocʗ4;
            var freeʗ3 = freeʗ2;
            var allocs = testing.AllocsPerRun(100, () => {
                foreach (var _ᴛ4 in range(100)) {
                    var p = allocʗ5();
                    freeʗ3(p);
                }
            });
            if (allocs != 0D) {
                tΔ4.Fatalf("expected 0 allocations, got %v"u8, allocs);
            }
        });

        var allocʗ6 = alloc;
        var freeʗ4 = free;
        t.Run(freeMultipleˢ, (ж<testing.T> tΔ5) => {
            // Multiple allocations outstanding before explicitly freeing,
            // but still within the limit of our smallest free list size
            // so that no allocs are reported. (Again, not long-term behavior).
            if (runtime_internal_test_package.SizeSpecializedMallocEnabled && !noscan) {
                // TODO(thepudds): skip at this point in the stack for size-specialized malloc
                // with !noscan. Additional integration with sizespecializedmalloc is in a later CL.
                tΔ5.Skip(temporarilySkippingAllocˢ);
            }
            if (!runtime_internal_test_package.RuntimeFreegcEnabled) {
                tΔ5.Skip(skippingAllocTestsWithˢ);
            }
            const nint maxOutstanding = 20;
            ref var s = ref heap<slice<ж<T>>>(out var Ꮡs);
            Ꮡs.ValueSlot = new slice<ж<T>>(0, maxOutstanding);
            var allocʗ7 = allocʗ6;
            var freeʗ5 = freeʗ4;
            var allocs = testing.AllocsPerRun(100 * stressMultiple, () => {
                Ꮡs.ValueSlot = Ꮡs.ValueSlot[..0];
                foreach (var _ᴛ5 in range(maxOutstanding)) {
                    var p = allocʗ7();
                    Ꮡs.ValueSlot = append(Ꮡs.ValueSlot, p);
                }
                foreach (var (_, p) in Ꮡs.ValueSlot) {
                    freeʗ5(p);
                }
            });
            if (allocs != 0D) {
                tΔ5.Fatalf("expected 0 allocations, got %v"u8, allocs);
            }
        });

        if (Δruntime.GOARCH == "wasm"u8) {
            // TODO(thepudds): for wasm, double-check if just slow, vs. some test logic problem,
            // vs. something else. It might have been wasm was slowest with tests that spawn
            // many goroutines, which might be expected for wasm. This skip might no longer be
            // needed now that we have tuned test execution time more, or perhaps wasm should just
            // always run in short mode, which might also let us remove this skip.
            t.Skip(skippingRemainingFreegcˢ);
        }

        var allocʗ8 = alloc;
        var freeʗ6 = free;
        t.Run(freeManyˢ, (ж<testing.T> tΔ6) => {
            // Confirm we are graceful if we have more freed elements at once
            // than the max free list size.
            var s = new slice<ж<T>>(0, 1000);
            nint iterations = stressMultiple * stressMultiple; // currently 1 (-short) or 100
            foreach (var _ᴛ6 in range(iterations)) {
                s = s[..0];
                foreach (var _ᴛ7 in range(1000)) {
                    var p = allocʗ8();
                    s = append(s, p);
                }
                foreach (var (_, p) in s) {
                    freeʗ6(p);
                }
            }
        });

        var allocʗ9 = alloc;
        var freeʗ7 = free;
        t.Run(duplicateCheckˢ, (ж<testing.T> tΔ7) => {
            // A simple duplicate allocation test. We track what should be the set
            // of live pointers in a map across a series of allocs and frees,
            // and fail if a live pointer value is returned by an allocation.
            // TODO: maybe add randomness? allow more live pointers? do across goroutines?
            var live = new map<uintptr, bool>();
            foreach (var i in range(100 * stressMultiple)) {
                slice<ж<T>> s = default!;
                // Alloc 10 times, tracking the live pointer values.
                foreach (var j in range(10)) {
                    var p = allocʗ9();
                    var uptr = (uintptr)p;
                    if (live[uptr]) {
                        tΔ7.Fatalf("found duplicate pointer (0x%x). i: %d j: %d"u8, uptr, i, j);
                    }
                    live[uptr] = true;
                    s = append(s, p);
                }
                // Explicitly free those pointers, removing them from the live map.
                foreach (var (k, _) in s) {
                    var p = s[k];
                    s[k] = default!;
                    var uptr = (uintptr)p;
                    freeʗ7(p);
                    delete(live, uptr);
                }
            }
        });

        var allocʗ10 = alloc;
        var freeʗ8 = free;
        t.Run(freeOtherGoroutineˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ8) => {
            // Use explicit free, but the free happens on a different goroutine than the alloc.
            // This also lightly simulates how the free code sees P migration or flushing
            // the mcache, assuming we have > 1 P. (Not using testing.AllocsPerRun here).
            nint iterations = 10 * stressMultiple * stressMultiple; // currently 10 (-short) or 1000
            foreach (var (_, capacity) in new nint[]{2}.slice()) {
                foreach (var _ᴛ8 in range(iterations)) {
                    var ch = new channel<ж<T>>(capacity);
                    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                    foreach (var _ᴛ9 in range(2)) {
                        Ꮡwg.Add(1);
                        var chʗ1 = ch;
                        var freeʗ9 = freeʗ8;
                        goǃ(() => {
                            GoFrame ᒐ = default;
                            try {
                                defer(Ꮡwg.Done, ref ᒐ);
                                foreach (var p in chʗ1) {
                                    freeʗ9(p);
                                }
                            }
                            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                            finally { ᒐ.Run(); }
                        });
                    }
                    foreach (var _ᴛ10 in range(100)) {
                        var p = allocʗ10();
                        ch.ᐸꟷ(p);
                    }
                    close(ch);
                    Ꮡwg.Wait();
                }
            }
        });

        var allocʗ11 = alloc;
        var freeʗ10 = free;
        t.Run(manyGoroutinesˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ9) => {
            // Allocate across multiple goroutines, freeing on the same goroutine.
            // TODO: probably remove the duplicate checking here; not that useful.
            var counts = new nint[]{1, 2, 4, 8, 10 * stressMultiple}.slice();
            foreach (var (_, goroutines) in counts) {
                ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                foreach (var _ᴛ11 in range(goroutines)) {
                    Ꮡwg.Add(1);
                    var allocʗ12 = allocʗ11;
                    var freeʗ11 = freeʗ10;
                    goǃ(() => {
                        GoFrame ᒐ = default;
                        try {
                            defer(Ꮡwg.Done, ref ᒐ);
                            var live = new map<uintptr, bool>();
                            foreach (var _ᴛ12 in range(100 * stressMultiple)) {
                                var p = allocʗ12();
                                var uptr = (uintptr)p;
                                if (live[uptr]) {
                                    throw panic("TestFreeLive: found duplicate pointer");
                                }
                                live[uptr] = true;
                                freeʗ11(p);
                                delete(live, uptr);
                            }
                        }
                        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                        finally { ᒐ.Run(); }
                    });
                }
                Ꮡwg.Wait();
            }
        });

        var allocʗ13 = alloc;
        var freeʗ12 = free;
        t.Run(assistCreditˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.T> tΔ10) => {
            GoFrame ᒐ = default;
            try {
                // Allocate and free using the same span class repeatedly while
                // verifying it results in a net zero change in assist credit.
                // This helps double-check our manipulation of the assist credit
                // during mallocgc/freegc, including in cases when there is
                // internal fragmentation when the requested mallocgc size is
                // smaller than the size class.
                //
                // See https://go.dev/cl/717520 for some additional discussion,
                // including how we can deliberately cause the test to fail currently
                // if we purposefully introduce some assist credit bugs.
                if (runtime_internal_test_package.SizeSpecializedMallocEnabled && !noscan) {
                    // TODO(thepudds): skip this test at this point in the stack; later CL has
                    // integration with sizespecializedmalloc.
                    tΔ10.Skip(temporarilySkipAssistˢ);
                }
                if (!runtime_internal_test_package.RuntimeFreegcEnabled) {
                    tΔ10.Skip(skippingAssistCreditTestˢ);
                }

                // Use a background goroutine to continuously run the GC.
                var done = new channel<EmptyStruct>(0);
                defer(ᴛ1 => close(ᴛ1), done, ref ᒐ);
                var doneʗ1 = done;
                goǃ(() => {
                    while (ᐧ) {
                        var selᴛ70 = doneʗ1;
                        switch (trySelect(ᐸꟷ(selᴛ70, ꓸꓸꓸ))) {
                        case 0 when selᴛ70.ꟷᐳ(out _): {
                            return;
                        }
                        default: {
                            Δruntime.GC();
                            break;
                        }}
                    }
                });

                // If making changes related to this test, consider testing locally with
                // larger counts, like 100K or 1M.
                var counts = new nint[]{1, 2, 10, 100 * stressMultiple}.slice();
                // Dropping down to GOMAXPROCS=1 might help reduce noise.
                defer(GOMAXPROCS, GOMAXPROCS(1), ref ᒐ);
                var size = (int64)@unsafe.Sizeof(@new<T>().ValueSlot);
                foreach (var (_, count) in counts) {
                    // Start by forcing a GC to reset this g's assist credit
                    // and perhaps help us get a cleaner measurement of GC cycle count.
                    Δruntime.GC();
                    foreach (var i in range(count)) {
                        // We disable preemption to reduce other code's ability to adjust this g's
                        // assist credit or otherwise change things while we are measuring.
                        runtime_internal_test_package.Acquirem();

                        // We do two allocations per loop, with the second allocation being
                        // the one we measure. The first allocation tries to ensure at least one
                        // reusable object on the mspan's free list when we do our measured allocation.
                        var p = allocʗ13();
                        freeʗ12(p);

                        // Now do our primary allocation of interest, bracketed by measurements.
                        // We measure more than we strictly need (to log details in case of a failure).
                        var creditStart = runtime_internal_test_package.AssistCredit();
                        var blackenStart = runtime_internal_test_package.GcBlackenEnable();
                        p = allocʗ13();
                        var blackenAfterAlloc = runtime_internal_test_package.GcBlackenEnable();
                        var creditAfterAlloc = runtime_internal_test_package.AssistCredit();
                        freeʗ12(p);
                        var blackenEnd = runtime_internal_test_package.GcBlackenEnable();
                        var creditEnd = runtime_internal_test_package.AssistCredit();

                        runtime_internal_test_package.Releasem();
                        runtime_internal_test_package.GoschedIfBusy();

                        var delta = creditEnd - creditStart;
                        if (delta != 0) {
                            tΔ10.Logf("assist credit non-zero delta: %d"u8, delta);
                            tΔ10.Logf("\t| size: %d i: %d count: %d"u8, size, i, count);
                            tΔ10.Logf("\t| credit before: %d credit after: %d"u8, creditStart, creditEnd);
                            tΔ10.Logf("\t| alloc delta: %d free delta: %d"u8,
                                creditAfterAlloc - creditStart, creditEnd - creditAfterAlloc);
                            tΔ10.Logf("\t| gcBlackenEnable (start / after alloc / end): %v/%v/%v"u8,
                                blackenStart, blackenAfterAlloc, blackenEnd);
                            tΔ10.FailNow();
                        }
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    };
}

public static void TestPageCacheLeak(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(GOMAXPROCS, GOMAXPROCS(1), ref ᒐ);
        var leaked = runtime_internal_test_package.PageCachePagesLeaked();
        if (leaked != 0) {
            Ꮡt.Fatalf("found %d leaked pages in page caches"u8, leaked);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcPhysˢ = "GCPhys"u8;

public static void TestPhysicalMemoryUtilization(ж<testing.T> Ꮡt) {
    @string got = runTestProg(Ꮡt, testprogˢ, gcPhysˢ);
    @string want = "OK\n"u8;
    if (got != want) {
        Ꮡt.Fatalf("expected %q, but got %q"u8, want, got);
    }
}

public static void TestScavengedBitsCleared(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    array<global::go.runtime_internal_test_package.BitsMismatch> mismatches = new(128);
    {
        var (n, ok) = runtime_internal_test_package.CheckScavengedBitsCleared(mismatches[..]); if (!ok) {
            Ꮡt.Errorf("uncleared scavenged bits"u8);
            foreach (var (_, m) in mismatches.slice(0, n)) {
                Ꮡt.Logf("\t@ address 0x%x"u8, m.Base);
                Ꮡt.Logf("\t|  got: %064b"u8, m.Got);
                Ꮡt.Logf("\t| want: %064b"u8, m.Want);
            }
            Ꮡt.FailNow();
        }
    }
}

partial struct acLink {
    internal array<byte> x = new((1 << (int)(20)));
}

internal static slice<ж<acLink>> arenaCollisionSink;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testArenaCollisionˢ = "TEST_ARENA_COLLISION"u8;
internal static readonly @string testRunˢ5 = "-test.run=^TestArenaCollision$"u8;
internal static readonly @string tooManyAddressSpaceˢ = "too many address space collisions"u8;
internal static readonly object ranOutOfArenaHintsˢ = (@string)"ran out of arena hints"u8;

public static void TestArenaCollision(ж<testing.T> Ꮡt) {
    // Test that mheap.sysAlloc handles collisions with other
    // memory mappings.
    if (Δos.Getenv(testArenaCollisionˢ) != "1"u8) {
        var cmd = testenv.CleanCmdEnv(exec.Command(testenv.Executable(new runtime_test_package.testing_TжTB(Ꮡt)), testRunˢ5, testVˢ));
        cmd.Value.Env = append((~cmd).Env, "TEST_ARENA_COLLISION=1"u8);
        var (@out, err) = cmd.CombinedOutput();
        if (race.Enabled){
            // This test runs the runtime out of hint
            // addresses, so it will start mapping the
            // heap wherever it can. The race detector
            // doesn't support this, so look for the
            // expected failure.
            {
                @string want = tooManyAddressSpaceˢ; if (!strings.Contains(((@string)@out), want)) {
                    Ꮡt.Fatalf("want %q, got:\n%s"u8, want, ((@string)@out));
                }
            }
        } else 
        if (!strings.Contains(((@string)@out), passˢ) || err != default!) {
            Ꮡt.Fatalf("%s\n(exit status %v)"u8, ((@string)@out), err);
        }
        return;
    }
    var disallowed = GoReflect.WithElemDims(new array<uintptr>[]{}.slice(), 2);
    // Drop all but the next 3 hints. 64-bit has a lot of hints,
    // so it would take a lot of memory to go through all of them.
    runtime_internal_test_package.KeepNArenaHints(3);
    // Consume these 3 hints and force the runtime to find some
    // fallback hints.
    for (nint i = 0; i < 5; i++) {
        // Reserve memory at the next hint so it can't be used
        // for the heap.
        var (start, end, ok) = runtime_internal_test_package.MapNextArenaHint();
        if (!ok) {
            Ꮡt.Skipf("failed to reserve memory at next arena hint [%#x, %#x)"u8, start, end);
        }
        Ꮡt.Logf("reserved [%#x, %#x)"u8, start, end);
        disallowed = append(disallowed, new uintptr[]{start, end}.array());

        (var hint, ok) = runtime_internal_test_package.NextArenaHint();
        if (!ok) {
            // We're out of arena hints. There's not much we can do now except give up.
            // This might happen for a number of reasons, like if there's just something
            // else already mapped in the address space where we put our hints. This is
            // a bit more common than it used to be thanks to heap base randomization.
            Ꮡt.Skip(ranOutOfArenaHintsˢ);
        }

        // Allocate until the runtime tries to use the hint we
        // just mapped over.
        while (ᐧ) {
            {
                var (next, okΔ1) = runtime_internal_test_package.NextArenaHint(); if (!okΔ1){
                    Ꮡt.Skip(ranOutOfArenaHintsˢ);
                } else 
                if (next != hint) {
                    break;
                }
            }
            var ac = @new<acLink>();
            arenaCollisionSink = append(arenaCollisionSink, ac);
            // The allocation must not have fallen into
            // one of the reserved regions.
            var p = (uintptr)ac;
            foreach (var (_, vᴛ1) in disallowed) {
                var d = vᴛ1.Clone();

                if (d[0] <= p && p < d[1]) {
                    Ꮡt.Fatalf("allocation %#x in reserved region [%#x, %#x)"u8, p, d[0], d[1]);
                }
            }
        }
    }
}

public static void BenchmarkMalloc8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var p = @new<int64>();
        runtime_internal_test_package.Escape(p);
    }
}

public static void BenchmarkMalloc16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var p = Ꮡ(new array<int64>(2));
        runtime_internal_test_package.Escape(p);
    }
}

public static void BenchmarkMalloc32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var p = Ꮡ(new array<int64>(4));
        runtime_internal_test_package.Escape(p);
    }
}

internal partial struct BenchmarkMallocTypeInfo8_type /*dyn*/ {
    internal array<ж<nint>> p = new(1);
}

public static void BenchmarkMallocTypeInfo8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var p = @new<BenchmarkMallocTypeInfo8_type>();
        runtime_internal_test_package.Escape(p);
    }
}

internal partial struct BenchmarkMallocTypeInfo16_type /*dyn*/ {
    internal array<ж<nint>> p = new(2);
}

public static void BenchmarkMallocTypeInfo16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var p = @new<BenchmarkMallocTypeInfo16_type>();
        runtime_internal_test_package.Escape(p);
    }
}

internal partial struct BenchmarkMallocTypeInfo32_type /*dyn*/ {
    internal array<ж<nint>> p = new(4);
}

public static void BenchmarkMallocTypeInfo32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var p = @new<BenchmarkMallocTypeInfo32_type>();
        runtime_internal_test_package.Escape(p);
    }
}

partial struct LargeStruct {
    internal array<slice<byte>> x = new(16);
}

public static void BenchmarkMallocLargeStruct(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var p = new slice<LargeStruct>(2, () => new());
        runtime_internal_test_package.Escape(p);
    }
}

internal static ж<nint> n = flag.Int("n"u8, 1000, "number of goroutines"u8);

public static void BenchmarkGoroutineSelect(ж<testing.B> Ꮡb) {
    var quit = new channel<EmptyStruct>(0);
    var quitʗ1 = quit;
    var read = (channel<EmptyStruct> ch) => {
        while (ᐧ) {
            var selᴛ71 = ch;
            var selᴛ72 = quitʗ1;
            switch (select(ᐸꟷ(selᴛ71, ꓸꓸꓸ), ᐸꟷ(selᴛ72, ꓸꓸꓸ))) {
            case 0 when selᴛ71.ꟷᐳ(out var _, out var ok): {
                if (!ok) {
                    return;
                }
                break;
            }
            case 1 when selᴛ72.ꟷᐳ(out _): {
                return;
            }}
        }
    };
    benchHelper(Ꮡb, n.Value, read);
}

public static void BenchmarkGoroutineBlocking(ж<testing.B> Ꮡb) {
    var read = (channel<EmptyStruct> ch) => {
        while (ᐧ) {
            {
                var (_, ok) = ᐸꟷ(ch, ꟷ); if (!ok) {
                    return;
                }
            }
        }
    };
    benchHelper(Ꮡb, n.Value, read);
}

public static void BenchmarkGoroutineForRange(ж<testing.B> Ꮡb) {
    var read = (channel<EmptyStruct> ch) => {
        foreach (var _ᴛ1 in ch) {
        }
    };
    benchHelper(Ꮡb, n.Value, read);
}

internal static partial void benchHelper(ж<testing.B> Ꮡb, nint n, Action<channel<EmptyStruct>> read) {
    ref var b = ref Ꮡb.DerefOrNull();

    var m = new slice<channel<EmptyStruct>>(n);
    foreach (var (i, _) in m) {
        m[i] = new channel<EmptyStruct>(1);
        goǃ(read, m[i]);
    }
    b.StopTimer();
    b.ResetTimer();
    GC();

    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, ch) in m) {
            if (ch != default!) {
                ch.ᐸꟷ(new EmptyStruct());
            }
        }
        time.Sleep(10 * time.Millisecond);
        b.StartTimer();
        GC();
        b.StopTimer();
    }

    foreach (var (_, ch) in m) {
        close(ch);
    }
    time.Sleep(10 * time.Millisecond);
}

public static partial void BenchmarkGoroutineIdle(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var quit = new channel<EmptyStruct>(0);
    var quitʗ1 = quit;
    void fn() {
        ᐸꟷ(quitʗ1);
    }
    for (nint i = 0; i < n.Value; i++) {
        var fnʗ1 = fn;
        goǃ(fnʗ1);
    }

    GC();
    b.ResetTimer();

    for (nint i = 0; i < b.N; i++) {
        GC();
    }

    b.StopTimer();
    close(quit);
    time.Sleep(10 * time.Millisecond);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mkmallocˢ = "_mkmalloc"u8;
internal static readonly @string testˢ = "test"u8;

public static void TestMkmalloc(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoRun(new runtime_test_package.testing_TжTB(Ꮡt));
    testenv.MustHaveExternalNetwork(new runtime_test_package.testing_TжTB(Ꮡt)); // To download the golang.org/x/tools dependency.
    var (output, err) = exec.Command("go"u8, "-C"u8, mkmallocˢ, testˢ).CombinedOutput();
    Ꮡt.Logf("test output:\n%s"u8, output);
    if (err != default!) {
        Ꮡt.Errorf("_mkmalloc tests failed: %v"u8, err);
    }
}

} // end runtime_test_package
