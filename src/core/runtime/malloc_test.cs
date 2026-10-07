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

[GoType] partial struct obj12 {
    internal uint64 a;
    internal uint32 b;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object unableToGetAFreshTinyˢ = (@string)"unable to get a fresh tiny slot"u8;

public static void TestTinyAllocIssue37262(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

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

[GoType] partial struct acLink {
    internal array<byte> x = new((1 << (int)(20)));
}

internal static slice<ж<acLink>> arenaCollisionSink;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testArenaCollisionˢ = "TEST_ARENA_COLLISION"u8;
internal static readonly @string testRunˢ5 = "-test.run=^TestArenaCollision$"u8;
internal static readonly @string tooManyAddressSpaceˢ = "too many address space collisions"u8;

public static void TestArenaCollision(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    testenv.MustHaveExec(new runtime_test_package.testing_TжTB(Ꮡt));
    // Test that mheap.sysAlloc handles collisions with other
    // memory mappings.
    if (Δos.Getenv(testArenaCollisionˢ) != "1"u8) {
        var cmd = testenv.CleanCmdEnv(exec.Command(Δos.Args[0], testRunˢ5, testVˢ));
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
        // Allocate until the runtime tries to use the hint we
        // just mapped over.
        var hint = runtime_internal_test_package.GetNextArenaHint();
        while (runtime_internal_test_package.GetNextArenaHint() == hint) {
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

[GoType("dyn")] internal partial struct BenchmarkMallocTypeInfo8_type {
    internal array<ж<nint>> p = new(1);
}

public static void BenchmarkMallocTypeInfo8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var p = @new<BenchmarkMallocTypeInfo8_type>();
        runtime_internal_test_package.Escape(p);
    }
}

[GoType("dyn")] internal partial struct BenchmarkMallocTypeInfo16_type {
    internal array<ж<nint>> p = new(2);
}

public static void BenchmarkMallocTypeInfo16(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        var p = @new<BenchmarkMallocTypeInfo16_type>();
        runtime_internal_test_package.Escape(p);
    }
}

[GoType] partial struct LargeStruct {
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
            var selᴛ70 = ch;
            var selᴛ71 = quitʗ1;
            switch (select(ᐸꟷ(selᴛ70, ꓸꓸꓸ), ᐸꟷ(selᴛ71, ꓸꓸꓸ))) {
            case 0 when selᴛ70.ꟷᐳ(out var _, out var ok): {
                if (!ok) {
                    return;
                }
                break;
            }
            case 1 when selᴛ71.ꟷᐳ(out _): {
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

} // end runtime_test_package
