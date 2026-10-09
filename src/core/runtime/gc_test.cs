// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using asan = @internal.asan_package;
using testenv = @internal.testenv_package;
using bits = global::go.math.bits_package;
using rand = global::go.math.rand_package;
using Δos = os_package;
using reflect = reflect_package;
using Δruntime = runtime_package;
using Δdebug = global::go.runtime.debug_package;
using slices = slices_package;
using strings = strings_package;
using Δsync = sync_package;
using atomic = global::go.sync.atomic_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using weak = weak_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using global::go.math;
using global::go.runtime;
using global::go.sync;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingKnownFlakyTestˢ = (@string)"skipping known-flaky test; golang.org/issue/37331"u8;
internal static readonly @string gogcˢ = "GOGC"u8;
internal static readonly object skippingTestGogcOffInˢ = (@string)"skipping test; GOGC=off in environment"u8;
internal static readonly @string gcSysˢ = "GCSys"u8;

public static void TestGcSys(ж<testing.T> Ꮡt) {
    Ꮡt.Skip(skippingKnownFlakyTestˢ);
    if (Δos.Getenv(gogcˢ) == "off"u8) {
        Ꮡt.Skip(skippingTestGogcOffInˢ);
    }
    @string got = runTestProg(Ꮡt, testprogˢ, gcSysˢ);
    @string want = "OK\n"u8;
    if (got != want) {
        Ꮡt.Fatalf("expected %q, but got %q"u8, want, got);
    }
}

/*[2][2][2][2][2][2][2][2][2][2]*/ internal partial struct TestGcDeepNesting_T /*[2]array<array<array<array<array<array<array<array<array<ж<nint>>>>>>>>>>*/;

public static void TestGcDeepNesting(ж<testing.T> Ꮡt) {
    var a = @new<TestGcDeepNesting_T>();
    // Prevent the compiler from applying escape analysis.
    // This makes sure new(T) is allocated on heap, not on the stack.
    Ꮡt.Logf("%p"u8, a.OrTypedNil());
    a.Value[0][0][0][0][0][0][0][0][0][0] = @new<nint>();
    a.Value[0][0][0][0][0][0][0][0][0][0].Value = 13;
    Δruntime.GC();
    if (a.Value[0][0][0][0][0][0][0][0][0][0].Value != 13) {
        Ꮡt.Fail();
    }
}

internal partial struct TestGcMapIndirection_T /*dyn*/ {
    internal array<nint> a = new(256);
}

public static void TestGcMapIndirection(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(Δdebug.SetGCPercent, Δdebug.SetGCPercent(1), ref ᒐ);
        Δruntime.GC();
        var m = new map<TestGcMapIndirection_T, TestGcMapIndirection_T>();
        for (nint i = 0; i < 2000; i++) {
            TestGcMapIndirection_T a = new();
            a.a[0] = i;
            m[a] = new TestGcMapIndirection_T(nil);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object corruptedHeapˢ = (@string)"corrupted heap"u8;

internal partial struct TestGcArraySlice_X /*dyn*/ {
    internal array<byte> buf = new(1);
    internal slice<byte> nextbuf;
    internal ж<TestGcArraySlice_X> next;
}

public static void TestGcArraySlice(ж<testing.T> Ꮡt) {
    ж<TestGcArraySlice_X> head = default!;
    for (nint i = 0; i < 10; i++) {
        var p = Ꮡ(new TestGcArraySlice_X(nil));
        p.Value.buf[0] = 42;
        p.Value.next = head;
        if (head != nil) {
            p.Value.nextbuf = (~head).buf[..];
        }
        head = p;
        Δruntime.GC();
    }
    for (var p = head; p != nil; p = p.Value.next) {
        if ((~p).buf[0] != 42) {
            Ꮡt.Fatal(corruptedHeapˢ);
        }
    }
}

internal partial struct TestGcRescan_X /*dyn*/ {
    internal channel<error> c;
    internal ж<TestGcRescan_X> nextx;
}

internal partial struct TestGcRescan_Y /*dyn*/ {
    public partial ref TestGcRescan_X X { get; }
    internal ж<TestGcRescan_Y> nexty;
    internal ж<nint> p;
}

public static void TestGcRescan(ж<testing.T> Ꮡt) {
    ж<TestGcRescan_Y> head = default!;
    for (nint i = 0; i < 10; i++) {
        var p = Ꮡ(new TestGcRescan_Y(nil));
        p.Value.c = new channel<error>(0);
        if (head != nil) {
            p.Value.nextx = head.of(TestGcRescan_Y.ᏑX);
        }
        p.Value.nexty = head;
        p.Value.p = @new<nint>();
        (~p).p.Value = 42;
        head = p;
        Δruntime.GC();
    }
    for (var p = head; p != nil; p = p.Value.nexty) {
        if ((~p).p.Value != 42) {
            Ꮡt.Fatal(corruptedHeapˢ);
        }
    }
}

public static void TestGcLastTime(ж<testing.T> Ꮡt) {
    var ms = @new<Δruntime.MemStats>();
    var t0 = time.Now().UnixNano();
    Δruntime.GC();
    var t1 = time.Now().UnixNano();
    Δruntime.ReadMemStats(ms);
    var last = (int64)(~ms).LastGC;
    if (t0 > last || last > t1) {
        Ꮡt.Fatalf("bad last GC time: got %v, want [%v, %v]"u8, last, t0, t1);
    }
    var pause = (~ms).PauseNs[((~ms).NumGC + 255) % 256];
    // Due to timer granularity, pause can actually be 0 on windows
    // or on virtualized environments.
    if (pause == 0){
        Ꮡt.Logf("last GC pause was 0"u8);
    } else 
    if (pause > 10000000000) {
        Ꮡt.Logf("bad last GC pause: got %v, want [0, 10e9]"u8, pause);
    }
}

internal static any hugeSink;

internal partial struct TestHugeGCInfo_type /*dyn*/ {
    internal float64 x;
    internal array<ж<byte>> y = new(4398465941504);
    internal slice<@string> z;
}

internal partial struct TestHugeGCInfo_typeᴛ1 /*dyn*/ {
    internal float64 x;
    internal array<uintptr> y = new(4398465941504);
    internal slice<@string> z;
}

public static void TestHugeGCInfo(ж<testing.T> Ꮡt) {
    // The test ensures that compiler can chew these huge types even on weakest machines.
    // The types are not allocated at runtime.
    if (hugeSink != default!) {
        // 400MB on 32 bots, 4TB on 64-bits.
        uintptr n = /* (400 << 20) + (unsafe.Sizeof(uintptr(0))-4)<<40 */ unchecked((uintptr)4398465941504);
        hugeSink = Ꮡ(new array<ж<byte>>(4398465941504));
        hugeSink = Ꮡ(new array<uintptr>(4398465941504));
        hugeSink = @new<TestHugeGCInfo_type>();
        hugeSink = @new<TestHugeGCInfo_typeᴛ1>();
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object noSysmonOnWasmYetˢ = (@string)"no sysmon on wasm yet"u8;

public static void TestPeriodicGC(ж<testing.T> Ꮡt) {
    if (Δruntime.GOARCH == "wasm"u8) {
        Ꮡt.Skip(noSysmonOnWasmYetˢ);
    }
    // Make sure we're not in the middle of a GC.
    Δruntime.GC();
    ref var ms1 = ref heap(new Δruntime.MemStats(), out var Ꮡms1);
    ref var ms2 = ref heap(new Δruntime.MemStats(), out var Ꮡms2);
    Δruntime.ReadMemStats(Ꮡms1);
    // Make periodic GC run continuously.
    var orig = runtime_internal_test_package.ForceGCPeriod.Value;
    runtime_internal_test_package.ForceGCPeriod.Value = 0;
    // Let some periodic GCs happen. In a heavily loaded system,
    // it's possible these will be delayed, so this is designed to
    // succeed quickly if things are working, but to give it some
    // slack if things are slow.
    uint32 numGCs = default!;
    const uint32 want = 2;
    for (nint i = 0; i < 200 && numGCs < want; i++) {
        time.Sleep(5 * time.Millisecond);
        // Test that periodic GC actually happened.
        Δruntime.ReadMemStats(Ꮡms2);
        numGCs = ms2.NumGC - ms1.NumGC;
    }
    runtime_internal_test_package.ForceGCPeriod.Value = orig;
    if (numGCs < want) {
        Ꮡt.Fatalf("no periodic GC: got %v GCs, want >= 2"u8, numGCs);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcZombieˢ = "GCZombie"u8;
internal static readonly @string godebugInvalidptr0ˢ = "GODEBUG=invalidptr=0"u8;
internal static readonly @string foundPointerToFreeObjectˢ = "found pointer to free object"u8;

public static void TestGcZombieReporting(ж<testing.T> Ꮡt) {
    // This test is somewhat sensitive to how the allocator works.
    // Pointers in zombies slice may cross-span, thus we
    // add invalidptr=0 for avoiding the badPointer check.
    // See issue https://golang.org/issues/49613/
    @string got = runTestProg(Ꮡt, testprogˢ, gcZombieˢ, godebugInvalidptr0ˢ);
    @string want = foundPointerToFreeObjectˢ;
    if (!strings.Contains(got, want)) {
        Ꮡt.Fatalf("expected %q in output, but got %q"u8, want, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object extraAllocationsWithAsanˢ = (@string)"extra allocations with -asan causes this to fail; see #70079"u8;
internal static readonly object stackDidNotMoveˢ = (@string)"stack did not move"u8;

public static void TestGCTestMoveStackOnNextCall(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (asan.Enabled) {
        Ꮡt.Skip(extraAllocationsWithAsanˢ);
    }
    Ꮡt.Parallel();
    ref var onStack = ref heap(new nint(), out var ᏑonStack);
    // GCTestMoveStackOnNextCall can fail in rare cases if there's
    // a preemption. This won't happen many times in quick
    // succession, so just retry a few times.
    for (nint retry = 0; retry < 5; retry++) {
        runtime_internal_test_package.GCTestMoveStackOnNextCall();
        if (moveStackCheck(Ꮡt, ᏑonStack, (uintptr)ᏑonStack)) {
            // Passed.
            return;
        }
    }
    Ꮡt.Fatal(stackDidNotMoveˢ);
}

// This must not be inlined because the point is to force a stack
// growth check and move the stack.
//
//go:noinline
internal static partial bool moveStackCheck(ж<testing.T> Ꮡt, ж<nint> Ꮡnew, uintptr old) {
    ref var t = ref Ꮡt.DerefOrNull();

    // new should have been updated by the stack move;
    // old should not have.
    // Capture new's value before doing anything that could
    // further move the stack.
    var new2 = (uintptr)Ꮡnew;
    Ꮡt.Logf("old stack pointer %x, new stack pointer %x"u8, old, new2);
    if (new2 == old) {
        // Check that we didn't screw up the test's escape analysis.
        {
            @string cls = runtime_internal_test_package.GCTestPointerClass(@unsafe.Pointer.FromPinnedBox(Ꮡnew)); if (cls != "stack"u8) {
                Ꮡt.Fatalf("test bug: new (%#x) should be a stack pointer, not %s"u8, new2, cls);
            }
        }
        // This was a real failure.
        return false;
    }
    return true;
}

public static void TestGCTestMoveStackRepeatedly(ж<testing.T> Ꮡt) {
    // Move the stack repeatedly to make sure we're not doubling
    // it each time.
    for (nint i = 0; i < 100; i++) {
        runtime_internal_test_package.GCTestMoveStackOnNextCall();
        moveStack1(false);
    }
}

//go:noinline
internal static partial void moveStack1(bool x) {
    // Make sure this function doesn't get auto-nosplit.
    if (x) {
        println((@string)"x"u8);
    }
}

public static void TestGCTestIsReachable(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    slice<@unsafe.Pointer> all = default!;
    slice<@unsafe.Pointer> half = default!;
    uint64 want = default!;
    for (nint i = 0; i < 16; i++) {
        // The tiny allocator muddies things, so we use a
        // scannable type.
        @unsafe.Pointer p = @unsafe.Pointer.FromBox(@new<ж<nint>>());
        all = append(all, p);
        if (i % 2 == 0) {
            half = append(half, p);
            want |= (uint64)(((uint64)1).Lsh((int64)(i)));
        }
    }
    var got = runtime_internal_test_package.GCTestIsReachable(all.ꓸꓸꓸ);
    if ((uint64)(got & want) != want) {
        // This is a serious bug - an object is live (due to the KeepAlive
        // call below), but isn't reported as such.
        Ꮡt.Fatalf("live object not in reachable set; want %b, got %b"u8, want, got);
    }
    if (global::go.math.bits_package.OnesCount64((uint64)(got & ~want)) > 1) {
        // Note: we can occasionally have a value that is retained even though
        // it isn't live, due to conservative scanning of stack frames.
        // See issue 67204. For now, we allow a "slop" of 1 unintentionally
        // retained object.
        Ꮡt.Fatalf("dead object in reachable set; want %b, got %b"u8, want, got);
    }
    Δruntime.KeepAlive(half);
}

internal static ж<ж<nint>> ᏑpointerClassBSS = new StandardBox<ж<nint>>(default(ж<nint>));
internal static ref ж<nint> pointerClassBSS => ref ᏑpointerClassBSS.ValueSlot;

internal static ж<nint> ᏑpointerClassData = new StandardBox<nint>(42);
internal static ref nint pointerClassData => ref ᏑpointerClassData.Value;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object extraAllocationsCauseˢ = (@string)"extra allocations cause this test to fail; see #70079"u8;
internal static readonly @string stackˢ = "stack"u8;
internal static readonly @string heapˢ = "heap"u8;
internal static readonly @string bssˢ = "bss"u8;
internal static readonly @string dataˢ = "data"u8;
internal static readonly @string otherˢ = "other"u8;

public static void TestGCTestPointerClass(ж<testing.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(extraAllocationsCauseˢ);
    }
    Ꮡt.Parallel();
    void check(@unsafe.Pointer p, @string want) {
        Ꮡt.Helper();
        @string got = runtime_internal_test_package.GCTestPointerClass(p);
        if (got != want) {
            // Convert the pointer to a uintptr to avoid
            // escaping it.
            Ꮡt.Errorf("for %#x, want class %s, got %s"u8, (uintptr)p, want, got);
        }
    }
    ref var onStack = ref heap(new nint(), out var ᏑonStack);
    ref var notOnStack = ref heap(new nint(), out var ᏑnotOnStack);
    check(@unsafe.Pointer.FromPinnedBox(ᏑonStack), stackˢ);
    check(@unsafe.Pointer.FromPinnedBox(runtime_internal_test_package.Escape(ᏑnotOnStack)), heapˢ);
    check(@unsafe.Pointer.FromBox(ᏑpointerClassBSS), bssˢ);
    check(@unsafe.Pointer.FromPinnedBox(ᏑpointerClassData), dataˢ);
    check(nil, otherˢ);
}

internal partial struct BenchmarkAllocation_T /*dyn*/ {
    internal ж<byte> x, y;
}

public static partial void BenchmarkAllocation(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint ngo = Δruntime.GOMAXPROCS(0);
    var work = new channel<bool>(b.N + ngo);
    var result = new channel<ж<BenchmarkAllocation_T>>(0);
    for (nint i = 0; i < b.N; i++) {
        work.ᐸꟷ(true);
    }
    for (nint i = 0; i < ngo; i++) {
        work.ᐸꟷ(false);
    }
    for (nint i = 0; i < ngo; i++) {
        var resultʗ1 = result;
        var workʗ1 = work;
        goǃ(() => {
            ж<BenchmarkAllocation_T> x = default!;
            while (ᐸꟷ(workʗ1)) {
                for (nint iΔ1 = 0; iΔ1 < 1000; iΔ1++) {
                    x = Ꮡ(new BenchmarkAllocation_T(nil));
                }
            }
            resultʗ1.ᐸꟷ(x);
        });
    }
    for (nint i = 0; i < ngo; i++) {
        ᐸꟷ(result);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingInShortModeˢ = (@string)"Skipping in short mode"u8;

public static partial void TestPrintGC(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (testing.Short()) {
            Ꮡt.Skip(skippingInShortModeˢ);
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(2), ref ᒐ);
        var done = new channel<bool>(0);
        var doneʗ1 = done;
        goǃ(() => {
            while (ᐧ) {
                var selᴛ68 = doneʗ1;
                switch (trySelect(ᐸꟷ(selᴛ68, ꓸꓸꓸ))) {
                case 0 when selᴛ68.ꟷᐳ(out _): {
                    return;
                }
                default: {
                    Δruntime.GC();
                    break;
                }}
            }
        });
        for (nint i = 0; i < 10000; i++) {
            ((Action)(() => {
                GoFrame ᒐ = default;
                try {
                    defer(ᴛ1 => builtin.print(ᴛ1), (@string)"", ref ᒐ);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            }))();
        }
        close(done);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static error testTypeSwitch(any x) {
    switch (x.type()) {
    case null: {
        break;
    }
    case {} Δy when Δy._<error>(out var y): {
        return y;
    }}
    // ok
    return default!;
}

internal static error testAssert(any x) {
    {
        var (y, ok) = x._<error>(ᐧ); if (ok) {
            return y;
        }
    }
    return default!;
}

internal static error testAssertVar(any x) {

    var (y, ok) = x._<error>(ᐧ);
    if (ok) {
        return y;
    }
    return default!;
}

internal static bool a;

//go:noinline
internal static partial void testIfaceEqual(any x) {
    if (AreEqual(x, (@string)("abc"))) {
        a = true;
    }
}

public static void TestPageAccounting(ж<testing.T> Ꮡt) {
    // Grow the heap in small increments. This used to drop the
    // pages-in-use count below zero because of a rounding
    // mismatch (golang.org/issue/15022).
    UntypedInt blockSize = /* 64 << 10 */ 65536;
    var blocks = new slice<ж<array<byte>>>(((64 << (int)(20))) / blockSize);
    foreach (var (i, _) in blocks) {
        blocks[i] = Ꮡ(new array<byte>(65536));
    }
    // Check that the running page count matches reality.
    var (pagesInUse, counted) = runtime_internal_test_package.CountPagesInUse();
    if (pagesInUse != counted) {
        Ꮡt.Fatalf("mheap_.pagesInUse is %d, but direct count is %d"u8, pagesInUse, counted);
    }
}

[GoInit] internal static void initΔ2() {
    // Enable ReadMemStats' double-check mode.
    runtime_internal_test_package.DoubleCheckReadMemStats.Value = true;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string memStatsˢ = "MemStats"u8;
internal static readonly object memstatsMismatchˢ = (@string)"memstats mismatch"u8;

public static void TestReadMemStats(ж<testing.T> Ꮡt) {
    var (@base, slow) = runtime_internal_test_package.ReadMemStatsSlow();
    if (@base != slow) {
        logDiff(Ꮡt, memStatsˢ, reflect.ValueOf(@base), reflect.ValueOf(slow));
        Ꮡt.Fatal(memstatsMismatchˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object notImplementedLogDiffForˢ = (@string)"not implemented: logDiff for map"u8;

internal static void logDiff(ж<testing.T> Ꮡt, @string prefix, reflectꓸValue got, reflectꓸValue want) {
    var typ = got.Type();
    var exprᴛ1 = typ.Kind();
    if (exprᴛ1 == reflect.Array || exprᴛ1 == reflect.ΔSlice) {
        if (got.Len() != want.Len()) {
            Ꮡt.Logf("len(%s): got %v, want %v"u8, prefix, got, want);
            return;
        }
        for (nint i = 0; i < got.Len(); i++) {
            logDiff(Ꮡt, fmt.Sprintf("%s[%d]"u8, prefix, i), got.Index(i), want.Index(i));
        }
    }
    else if (exprᴛ1 == reflect.Struct) {
        for (nint i = 0; i < typ.NumField(); i++) {
            var (gf, wf) = (got.Field(i), want.Field(i));
            logDiff(Ꮡt, prefix + "."u8 + typ.Field(i).Name, gf, wf);
        }
    }
    else if (exprᴛ1 == reflect.Map) {
        Ꮡt.Fatal(notImplementedLogDiffForˢ);
    }
    else { /* default: */
        if (!AreEqual(got.Interface(), want.Interface())) {
            Ꮡt.Logf("%s: got %v, want %v"u8, prefix, got, want);
        }
    }

}

public static void BenchmarkReadMemStats(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var ms = ref heap(new Δruntime.MemStats(), out var Ꮡms);
    UntypedInt heapSize = /* 100 << 20 */ 104857600;
    var x = new slice<ж<array<byte>>>(heapSize / 1024);
    foreach (var (i, _) in x) {
        x[i] = Ꮡ(new array<byte>(1024));
    }
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        Δruntime.ReadMemStats(Ꮡms);
    }
    Δruntime.KeepAlive(x);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object thisBenchmarkCanOnlyBeˢ = (@string)"This benchmark can only be run with GOMAXPROCS > 1"u8;

// Code to build a big tree with lots of pointers.
internal partial struct applyGCLoad_node /*dyn*/ {
    internal array<ж<applyGCLoad_node>> children = new(16);
}

internal static partial Action applyGCLoad(ж<testing.B> Ꮡb) {
    // We’ll apply load to the runtime with maxProcs-1 goroutines
    // and use one more to actually benchmark. It doesn't make sense
    // to try to run this test with only 1 P (that's what
    // BenchmarkReadMemStats is for).
    nint maxProcs = Δruntime.GOMAXPROCS(-1);
    if (maxProcs == 1) {
        Ꮡb.Skip(thisBenchmarkCanOnlyBeˢ);
    }
    ref var buildTree = ref heap<Func<nint, ж<applyGCLoad_node>>>(out var ᏑbuildTree);
    buildTree = (nint depth) => {
        var tree = @new<applyGCLoad_node>();
        if (depth != 0) {
            foreach (var (i, _) in (~tree).children) {
                tree.Value.children[i] = ᏑbuildTree.ValueSlot(depth - 1);
            }
        }
        return tree;
    };
    // Keep the GC busy by continuously generating large trees.
    var done = new channel<EmptyStruct>(0);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < maxProcs - 1; i++) {
        Ꮡwg.Add(1);
        var doneʗ1 = done;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                ж<applyGCLoad_node> hold = default!;
loop:
                while (ᐧ) {
                    hold = ᏑbuildTree.ValueSlot(5);
                    var selᴛ69 = doneʗ1;
                    switch (trySelect(ᐸꟷ(selᴛ69, ꓸꓸꓸ))) {
                    case 0 when selᴛ69.ꟷᐳ(out _): {
                        goto break_loop;
                        break;
                    }
                    default: {
                        break;
                    }}
continue_loop:;
                }
break_loop:;
                Δruntime.KeepAlive(hold.OrTypedNil());
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    var doneʗ2 = done;
    return () => {
        close(doneʗ2);
        Ꮡwg.Wait();
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nsOpˢ = "ns/op"u8;
internal static readonly @string bOpˢ = "B/op"u8;
internal static readonly @string allocsOpˢ = "allocs/op"u8;
internal static readonly @string p50Nsˢ = "p50-ns"u8;
internal static readonly @string p90Nsˢ = "p90-ns"u8;
internal static readonly @string p99Nsˢ = "p99-ns"u8;

public static void BenchmarkReadMemStatsLatency(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var stop = applyGCLoad(Ꮡb);
    // Spend this much time measuring latencies.
    var latencies = new slice<time.Duration>(0, 1024);
    // Run for timeToBench hitting ReadMemStats continuously
    // and measuring the latency.
    b.ResetTimer();
    ref var ms = ref heap(new Δruntime.MemStats(), out var Ꮡms);
    for (nint i = 0; i < b.N; i++) {
        // Sleep for a bit, otherwise we're just going to keep
        // stopping the world and no one will get to do anything.
        time.Sleep(100 * time.Millisecond);
        var start = time.Now();
        Δruntime.ReadMemStats(Ꮡms);
        latencies = append(latencies, time.Since(start));
    }
    // Make sure to stop the timer before we wait! The load created above
    // is very heavy-weight and not easy to stop, so we could end up
    // confusing the benchmarking framework for small b.N.
    b.StopTimer();
    stop();
    // Disable the default */op metrics.
    // ns/op doesn't mean anything because it's an average, but we
    // have a sleep in our b.N loop above which skews this significantly.
    b.ReportMetric(0D, nsOpˢ);
    b.ReportMetric(0D, bOpˢ);
    b.ReportMetric(0D, allocsOpˢ);
    // Sort latencies then report percentiles.
    slices.Sort<slice<time.Duration>, time.Duration>(latencies);
    b.ReportMetric((float64)(int64)latencies[len(latencies) * 50 / 100], p50Nsˢ);
    b.ReportMetric((float64)(int64)latencies[len(latencies) * 90 / 100], p90Nsˢ);
    b.ReportMetric((float64)(int64)latencies[len(latencies) * 99 / 100], p99Nsˢ);
}

public static void TestUserForcedGC(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Test that runtime.GC() triggers a GC even if GOGC=off.
        defer(Δdebug.SetGCPercent, Δdebug.SetGCPercent(-1), ref ᒐ);
        ref var ms1 = ref heap(new Δruntime.MemStats(), out var Ꮡms1);
        ref var ms2 = ref heap(new Δruntime.MemStats(), out var Ꮡms2);
        Δruntime.ReadMemStats(Ꮡms1);
        Δruntime.GC();
        Δruntime.ReadMemStats(Ꮡms2);
        if (ms1.NumGC == ms2.NumGC) {
            Ꮡt.Fatalf("runtime.GC() did not trigger GC"u8);
        }
        if (ms1.NumForcedGC == ms2.NumForcedGC) {
            Ꮡt.Fatalf("runtime.GC() was not accounted in NumForcedGC"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static partial void writeBarrierBenchmark(ж<testing.B> Ꮡb, Action f) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        Δruntime.GC();
        ref var ms = ref heap(new Δruntime.MemStats(), out var Ꮡms);
        Δruntime.ReadMemStats(Ꮡms);
        //b.Logf("heap size: %d MB", ms.HeapAlloc>>20)
        // Keep GC running continuously during the benchmark, which in
        // turn keeps the write barrier on continuously.
        ref var stop = ref heap(new uint32(), out var Ꮡstop);
        var done = new channel<bool>(0);
        var doneʗ1 = done;
        goǃ(() => {
            while (atomic.LoadUint32(Ꮡstop) == 0) {
                Δruntime.GC();
            }
            close(doneʗ1);
        });
        var doneʗ2 = done;
        defer(() => {
            atomic.StoreUint32(Ꮡstop, 1);
            ᐸꟷ(doneʗ2);
        }, ref ᒐ);
        b.ResetTimer();
        f();
        b.StopTimer();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object needGomaxprocs2ˢ = (@string)"need GOMAXPROCS >= 2"u8;

// Construct a large tree both so the GC runs for a while and
// so we have a data structure to manipulate the pointers of.
internal partial struct BenchmarkWriteBarrier_node /*dyn*/ {
    internal ж<BenchmarkWriteBarrier_node> l, r;
}

public static void BenchmarkWriteBarrier(ж<testing.B> Ꮡb) {
    if (Δruntime.GOMAXPROCS(-1) < 2) {
        // We don't want GC to take our time.
        Ꮡb.Skip(needGomaxprocs2ˢ);
    }
    ref var wbRoots = ref heap<slice<ж<BenchmarkWriteBarrier_node>>>(out var ᏑwbRoots);
    ref var mkTree = ref heap<Func<nint, ж<BenchmarkWriteBarrier_node>>>(out var ᏑmkTree);
    mkTree = (nint level) => {
        if (level == 0) {
            return default!;
        }
        var n = Ꮡ(new BenchmarkWriteBarrier_node(ᏑmkTree.ValueSlot(level - 1), ᏑmkTree.ValueSlot(level - 1)));
        if (level == 10) {
            // Seed GC with enough early pointers so it
            // doesn't start termination barriers when it
            // only has the top of the tree.
            ᏑwbRoots.ValueSlot = append(ᏑwbRoots.ValueSlot, n);
        }
        return n;
    };
    UntypedInt depth = 22; // 64 MB
    var root = mkTree(22);
    var rootʗ1 = root;
    writeBarrierBenchmark(Ꮡb, () => {
        array<ж<BenchmarkWriteBarrier_node>> stack = new(22); /* depth */
        nint tos = -1;
        // There are two write barriers per iteration, so i+=2.
        for (nint i = 0; i < Ꮡb.Value.N; i += 2) {
            if (tos == -1) {
                stack[0] = rootʗ1;
                tos = 0;
            }
            // Perform one step of reversing the tree.
            var n = stack[tos];
            if ((~n).l == nil){
                tos--;
            } else {
                (n.Value.l, n.Value.r) = (n.Value.r, n.Value.l);
                stack[tos] = n.Value.l;
                stack[tos + 1] = n.Value.r;
                tos++;
            }
            if (i % ((1 << (int)(12))) == 0) {
                // Avoid non-preemptible loops (see issue #10958).
                Δruntime.Gosched();
            }
        }
    });
    Δruntime.KeepAlive(wbRoots);
}

internal partial struct BenchmarkBulkWriteBarrier_obj /*[16]ж<byte>*/;

public static void BenchmarkBulkWriteBarrier(ж<testing.B> Ꮡb) {
    if (Δruntime.GOMAXPROCS(-1) < 2) {
        // We don't want GC to take our time.
        Ꮡb.Skip(needGomaxprocs2ˢ);
    }
    // Construct a large set of objects we can copy around.
    UntypedInt heapSize = /* 64 << 20 */ 67108864;
    var ptrs = new slice<ж<BenchmarkBulkWriteBarrier_obj>>((nint)((uintptr)heapSize / /* unsafe.Sizeof(obj{}) */ (uintptr)128));
    foreach (var (i, _) in ptrs) {
        ptrs[i] = @new<BenchmarkBulkWriteBarrier_obj>();
    }
    var ptrsʗ1 = ptrs;
    writeBarrierBenchmark(Ꮡb, () => {
        UntypedInt blockSize = 1024;
        nint pos = default!;
        for (nint i = 0; i < Ꮡb.Value.N; i += blockSize) {
            // Rotate block.
            var block = ptrsʗ1.slice(pos, pos + (nint)blockSize);
            var first = block[0];
            copy(block, block[1..]);
            block[blockSize - 1] = first;
            pos += blockSize;
            if (pos + (nint)blockSize > len(ptrsʗ1)) {
                pos = 0;
            }
            Δruntime.Gosched();
        }
    });
    Δruntime.KeepAlive(ptrs);
}

public static partial void BenchmarkScanStackNoLocals(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var ready = ref heap(new Δsync.WaitGroup(), out var Ꮡready);
    var teardown = new channel<bool>(0);
    for (nint j = 0; j < 10; j++) {
        Ꮡready.Add(1);
        var teardownʗ1 = teardown;
        goǃ(() => {
            ref var x = ref heap<nint>(out var Ꮡx);
            x = 100000;
            countpwg(Ꮡx, Ꮡready, teardownʗ1);
        });
    }
    Ꮡready.Wait();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        b.StartTimer();
        Δruntime.GC();
        Δruntime.GC();
        b.StopTimer();
    }
    close(teardown);
}

public static void BenchmarkMSpanCountAlloc(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        // Allocate one dummy mspan for the whole benchmark.
        var s = runtime_internal_test_package.AllocMSpan();
        defer(runtime_internal_test_package.FreeMSpan, s, ref ᒐ);
        // n is the number of bytes to benchmark against.
        // n must always be a multiple of 8, since gcBits is
        // always rounded up 8 bytes.
        foreach (var (_, n) in new nint[]{8, 16, 32, 64, 128}.slice()) {
            var sʗ1 = s;
            Ꮡb.Run(fmt.Sprintf("bits=%d"u8, n * 8), (ж<testing.B> bΔ1) => {
                // Initialize a new byte slice with pseudo-random data.
                var bits = new slice<byte>(n);
                rand.Read(bits);
                bΔ1.ResetTimer();
                for (nint i = 0; i < (~bΔ1).N; i++) {
                    runtime_internal_test_package.MSpanCountAlloc(sʗ1, bits);
                }
            });
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void countpwg(ж<nint> Ꮡn, ж<Δsync.WaitGroup> Ꮡready, channel<bool> teardown) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (n == 0) {
        Ꮡready.Done();
        ᐸꟷ(teardown);
        return;
    }
    n--;
    countpwg(Ꮡn, Ꮡready, teardown);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object stressTestThatTakesTimeˢ = (@string)"stress test that takes time to run"u8;
internal static readonly object wantAtLeast4CPUsForThisˢ = (@string)"want at least 4 CPUs for this test"u8;
internal static readonly @string gcMemoryLimitˢ = "GCMemoryLimit"u8;

public static void TestMemoryLimit(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(stressTestThatTakesTimeˢ);
    }
    if (Δruntime.NumCPU() < 4) {
        Ꮡt.Skip(wantAtLeast4CPUsForThisˢ);
    }
    @string got = runTestProg(Ꮡt, testprogˢ, gcMemoryLimitˢ);
    @string want = "OK\n"u8;
    if (got != want) {
        Ꮡt.Fatalf("expected %q, but got %q"u8, want, got);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gcMemoryLimitNoGCPercentˢ = "GCMemoryLimitNoGCPercent"u8;

public static void TestMemoryLimitNoGCPercent(ж<testing.T> Ꮡt) {
    if (testing.Short()) {
        Ꮡt.Skip(stressTestThatTakesTimeˢ);
    }
    if (Δruntime.NumCPU() < 4) {
        Ꮡt.Skip(wantAtLeast4CPUsForThisˢ);
    }
    @string got = runTestProg(Ꮡt, testprogˢ, gcMemoryLimitNoGCPercentˢ);
    @string want = "OK\n"u8;
    if (got != want) {
        Ꮡt.Fatalf("expected %q, but got %q"u8, want, got);
    }
}

public static void TestMyGenericFunc(ж<testing.T> Ꮡt) {
    runtime_internal_test_package.MyGenericFunc<nint>();
}

internal partial struct TestWeakToStrongMarkTermination_T /*dyn*/ {
    internal ж<nint> a;
    internal nint b;
}

public static partial void TestWeakToStrongMarkTermination(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        testenv.MustHaveParallelism(new runtime_test_package.testing_TжTB(Ꮡt));
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(2), ref ᒐ);
        defer(Δdebug.SetGCPercent, Δdebug.SetGCPercent(-1), ref ᒐ);
        var w = new slice<weak.Pointer<TestWeakToStrongMarkTermination_T>>(2048);
        // Make sure there's no out-standing GC from a previous test.
        Δruntime.GC();
        // Create many objects with a weak pointers to them.
        foreach (var (i, _) in w) {
            var x = @new<TestWeakToStrongMarkTermination_T>();
            x.Value.a = @new<nint>();
            w[i] = weak.Make<TestWeakToStrongMarkTermination_T>(x);
        }
        // Reset the restart flag.
        runtime_internal_test_package.GCMarkDoneResetRestartFlag();
        // Prevent mark termination from completing.
        runtime_internal_test_package.SetSpinInGCMarkDone(true);
        // Start a GC, and wait a little bit to get something spinning in mark termination.
        // Simultaneously, fire off another goroutine to disable spinning. If everything's
        // working correctly, then weak.Value will block, so we need to make sure something
        // prevents the GC from continuing to spin.
        var done = new channel<EmptyStruct>(0);
        var doneʗ1 = done;
        goǃ(() => {
            Δruntime.GC();
            doneʗ1.ᐸꟷ(new EmptyStruct());
        });
        goǃ(() => {
            // Usleep here instead of time.Sleep. time.Sleep
            // can allocate, and if we get unlucky, then it
            // can end up stuck in gcMarkDone with nothing to
            // wake it.
            runtime_internal_test_package.Usleep(100000); // 100ms
            // Let mark termination continue.
            runtime_internal_test_package.SetSpinInGCMarkDone(false);
        });
        time.Sleep(10 * time.Millisecond);
        // Perform many weak->strong conversions in the critical window.
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        foreach (var (_, vᴛ1) in w) {
            ref var wp = ref heap(new weak.Pointer<TestWeakToStrongMarkTermination_T>(), out var Ꮡwp);
            wp = vᴛ1;

            Ꮡwg.Add(1);
            var wpʗ1 = wp;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    wpʗ1.Value();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        // Make sure the GC completes.
        ᐸꟷ(done);
        // Make sure all the weak->strong conversions finish.
        Ꮡwg.Wait();
        // The bug is triggered if there's still mark work after gcMarkDone stops the world.
        //
        // This can manifest in one of two ways today:
        // - An exceedingly rare crash in mark termination.
        // - gcMarkDone restarts, as if issue #27993 is at play.
        //
        // Check for the latter. This is a fairly controlled environment, so #27993 is very
        // unlikely to happen (it's already rare to begin with) but we'll always _appear_ to
        // trigger the same bug if weak->strong conversions aren't properly coordinated with
        // mark termination.
        if (runtime_internal_test_package.GCMarkDoneRestarted()) {
            Ꮡt.Errorf("gcMarkDone restarted"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package
