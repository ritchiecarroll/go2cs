// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using flag = flag_package;
using fmt = fmt_package;
using cpu = @internal.cpu_package;
using atomic = @internal.runtime.atomic_package;
using Δio = io_package;
using bits = global::go.math.bits_package;
using static runtime_package;
using Δdebug = global::go.runtime.debug_package;
using slices = slices_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using @internal.runtime;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using FieldOffsetAttribute = global::System.Runtime.InteropServices.FieldOffsetAttribute;
using LayoutKind = global::System.Runtime.InteropServices.LayoutKind;
using StructLayoutAttribute = global::System.Runtime.InteropServices.StructLayoutAttribute;
using global::go.math;
using global::go.runtime;
using static global::go.runtime_internal_test_package;
using Δruntime = runtime_package;

partial class runtime_test_package {

// flagQuick is set by the -quick option to skip some relatively slow tests.
// This is used by the cmd/dist test runtime:cpu124.
// The cmd/dist test passes both -test.short and -quick;
// there are tests that only check testing.Short, and those tests will
// not be skipped if only -quick is used.
internal static ж<bool> flagQuick = flag.Bool("quick"u8, false, "skip slow tests, for cmd/dist test runtime:cpu124"u8);

[GoInit] internal static void initΔ5() {
    // We're testing the runtime, so make tracebacks show things
    // in the runtime. This only raises the level, so it won't
    // override GOTRACEBACK=crash from the user.
    runtime_internal_test_package.SetTracebackEnv("system"u8);
}

internal static error errf;

internal static error errfn() {
    return errf;
}

internal static error errfn1() {
    return Δio.EOF;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object badComparisonˢ = (@string)"bad comparison"u8;

public static void BenchmarkIfaceCmp100(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        for (nint j = 0; j < 100; j++) {
            if (AreEqual(errfn(), Δio.EOF)) {
                Ꮡb.Fatal(badComparisonˢ);
            }
        }
    }
}

public static void BenchmarkIfaceCmpNil100(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        for (nint j = 0; j < 100; j++) {
            if (errfn1() == default!) {
                Ꮡb.Fatal(badComparisonˢ);
            }
        }
    }
}

internal static any efaceCmp1;

internal static any efaceCmp2;

public static void BenchmarkEfaceCmpDiff(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap<nint>(out var Ꮡx);
    x = 5;
    efaceCmp1 = Ꮡx;
    ref var y = ref heap<nint>(out var Ꮡy);
    y = 6;
    efaceCmp2 = Ꮡy;
    for (nint i = 0; i < b.N; i++) {
        for (nint j = 0; j < 100; j++) {
            if (AreEqual(efaceCmp1, efaceCmp2)) {
                Ꮡb.Fatal(badComparisonˢ);
            }
        }
    }
}

public static void BenchmarkEfaceCmpDiffIndirect(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    efaceCmp1 = new nint[]{1, 2}.array();
    efaceCmp2 = new nint[]{1, 2}.array();
    for (nint i = 0; i < b.N; i++) {
        for (nint j = 0; j < 100; j++) {
            if (!AreEqual(efaceCmp1, efaceCmp2)) {
                Ꮡb.Fatal(badComparisonˢ);
            }
        }
    }
}

public static void BenchmarkDefer(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        defer1();
    }
}

internal static void defer1() {
    GoFrame ᒐ = default;
    try {
        defer((nint x, nint y, nint z) => {
            if (recover() != default! || x != 1 || y != 2 || z != 3) {
                throw panic("bad recover");
            }
        }, (nint)(1), (nint)(2), (nint)(3), ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkDefer10(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N / 10; i++) {
        defer2();
    }
}

internal static void defer2() {
    GoFrame ᒐ = default;
    try {
        for (nint i = 0; i < 10; i++) {
            defer((nint x, nint y, nint z) => {
                if (recover() != default! || x != 1 || y != 2 || z != 3) {
                    throw panic("bad recover");
                }
            }, (nint)(1), (nint)(2), (nint)(3), ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkDeferMany(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        for (nint i = 0; i < b.N; i++) {
            defer((nint x, nint y, nint z) => {
                if (recover() != default! || x != 1 || y != 2 || z != 3) {
                    throw panic("bad recover");
                }
            }, (nint)(1), (nint)(2), (nint)(3), ref ᒐ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkPanicRecover(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        defer3();
    }
}

internal static void defer3() {
    GoFrame ᒐ = default;
    try {
        defer((nint x, nint y, nint z) => {
            if (recover() == default!) {
                throw panic("failed recover");
            }
        }, (nint)(1), (nint)(2), (nint)(3), ref ᒐ);
        throw panic("hi");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// golang.org/issue/7063
public static void TestStopCPUProfilingWithProfilerOff(ж<testing.T> Ꮡt) {
    SetCPUProfileRate(0);
}

// Addresses to test for faulting behavior.
// This is less a test of SetPanicOnFault and more a check that
// the operating system and the runtime can process these faults
// correctly. That is, we're indirectly testing that without SetPanicOnFault
// these would manage to turn into ordinary crashes.
// Note that these are truncated on 32-bit systems, so the bottom 32 bits
// of the larger addresses must themselves be invalid addresses.
// We might get unlucky and the OS might have mapped one of these
// addresses, but probably not: they're all in the first page, very high
// addresses that normally an OS would reserve for itself, or malformed
// addresses. Even so, we might have to remove one or two on different
// systems. We will see.
// low addresses
// high (kernel) addresses
// or else malformed.
internal static slice<uint64> faultAddrs = new uint64[]{
    0,
    1,
    0xfff,
    0xffffffffffffffffUL,
    0xfffffffffffff001UL,
    0xffffffffffff0001UL,
    0xfffffffffff00001UL,
    0xffffffffff000001UL,
    0xfffffffff0000001UL,
    0xffffffff00000001UL,
    0xfffffff000000001UL,
    0xffffff0000000001UL,
    0xfffff00000000001UL,
    0xffff000000000001UL,
    0xfff0000000000001UL,
    0xff00000000000001UL,
    0xf000000000000001UL,
    0x8000000000000001UL
}.slice();

public static void TestSetPanicOnFault(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var old = Δdebug.SetPanicOnFault(true);
        defer(Δdebug.SetPanicOnFault, old, ref ᒐ);
        ref var nfault = ref heap<nint>(out var Ꮡnfault);
        nfault = 0;
        foreach (var (_, addr) in faultAddrs) {
            testSetPanicOnFault(Ꮡt, (uintptr)addr, Ꮡnfault);
        }
        if (nfault == 0) {
            Ꮡt.Fatalf("none of the addresses faulted"u8);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// testSetPanicOnFault tests one potentially faulting address.
// It deliberately constructs and uses an invalid pointer,
// so mark it as nocheckptr.
//
//go:nocheckptr
internal static void testSetPanicOnFault(ж<testing.T> Ꮡt, uintptr addr, ж<nint> Ꮡnfault) {
    GoFrame ᒐ = default;
    try {
        if (GOOS == "js"u8 || GOOS == "wasip1"u8) {
            Ꮡt.Skip(GOOS + " does not support catching faults");
        }
        defer(() => {
            {
                var err = recover(); if (err != default!) {
                    Ꮡnfault.Value++;
                }
            }
        }, ref ᒐ);
        // The read should fault, except that sometimes we hit
        // addresses that have had C or kernel pages mapped there
        // readable by user code. So just log the content.
        // If no addresses fault, we'll fail the test.
        var v = ~(ж<byte>)(uintptr)((@unsafe.Pointer)addr);
        Ꮡt.Logf("addr %#x: %#x\n"u8, addr, v);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static bool eqstring_generic(@string s1, @string s2) {
    if (len(s1) != len(s2)) {
        return false;
    }
    // optimization in assembly versions:
    // if s1.str == s2.str { return true }
    for (nint i = 0; i < len(s1); i++) {
        if (s1[i] != s2[i]) {
            return false;
        }
    }
    return true;
}

public static void TestEqString(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // This isn't really an exhaustive test of == on strings, it's
    // just a convenient way of documenting (via eqstring_generic)
    // what == does.
    var s = new @string[]{
        ""u8,
        "a"u8,
        "c"u8,
        "aaa"u8,
        "ccc"u8,
        "cccc"u8[..3], // same contents, different string

        "1234567890"u8
    }.slice();
    foreach (var (_, s1) in s) {
        foreach (var (_, s2) in s) {
            var x = s1 == s2;
            var y = eqstring_generic(s1, s2);
            if (x != y) {
                Ꮡt.Errorf(@"(""%s"" == ""%s"") = %t, want %t"u8, s1, s2, x, y);
            }
        }
    }
}

// make sure we add padding for structs with trailing zero-sized fields
internal partial struct TestTrailingZero_T1 /*dyn*/ {
    internal int32 n;
    internal array<byte> z = new(0);
}

[StructLayout(LayoutKind.Explicit, Size = 16)] internal partial struct TestTrailingZero_T2 /*dyn*/ {
    [FieldOffset(0)] internal int64 n;
    [FieldOffset(8)] internal readonly EmptyStruct z;
}

internal partial struct TestTrailingZero_T3 /*dyn*/ {
    internal byte n;
    internal array<EmptyStruct> z = new(4);
}

// make sure padding can double for both zerosize and alignment
[StructLayout(LayoutKind.Explicit, Size = 8)] internal partial struct TestTrailingZero_T4 /*dyn*/ {
    [FieldOffset(0)] internal int32 a;
    [FieldOffset(4)] internal int16 b;
    [FieldOffset(6)] internal int8 c;
    [FieldOffset(7)] internal readonly EmptyStruct z;
}

// make sure we don't pad a zero-sized thing
internal partial struct TestTrailingZero_T5 /*dyn*/ {
}

public static void TestTrailingZero(ж<testing.T> Ꮡt) {
    if (/* unsafe.Sizeof(T1{}) */ (uintptr)8 != 8) {
        Ꮡt.Errorf("sizeof(%#v)==%d, want 8"u8, new TestTrailingZero_T1(nil), /* unsafe.Sizeof(T1{}) */ (uintptr)8);
    }
    if (/* unsafe.Sizeof(T2{}) */ (uintptr)16 != 8 + /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8) {
        Ꮡt.Errorf("sizeof(%#v)==%d, want %d"u8, new TestTrailingZero_T2(nil), /* unsafe.Sizeof(T2{}) */ (uintptr)16, 8 + /* unsafe.Sizeof(uintptr(0)) */ (uintptr)8);
    }
    if (/* unsafe.Sizeof(T3{}) */ (uintptr)2 != 2) {
        Ꮡt.Errorf("sizeof(%#v)==%d, want 2"u8, new TestTrailingZero_T3(nil), /* unsafe.Sizeof(T3{}) */ (uintptr)2);
    }
    if (/* unsafe.Sizeof(T4{}) */ (uintptr)8 != 8) {
        Ꮡt.Errorf("sizeof(%#v)==%d, want 8"u8, new TestTrailingZero_T4(nil), /* unsafe.Sizeof(T4{}) */ (uintptr)8);
    }
    if (/* unsafe.Sizeof(T5{}) */ (uintptr)0 != 0) {
        Ꮡt.Errorf("sizeof(%#v)==%d, want 0"u8, new TestTrailingZero_T5(nil), /* unsafe.Sizeof(T5{}) */ (uintptr)0);
    }
}

public static void TestAppendGrowth(ж<testing.T> Ꮡt) {
    ref var x = ref heap<slice<int64>>(out var Ꮡx);
    void check(nint wantΔ1) {
        if (cap(Ꮡx.ValueSlot) != wantΔ1) {
            Ꮡt.Errorf("len=%d, cap=%d, want cap=%d"u8, len(Ꮡx.ValueSlot), cap(Ꮡx.ValueSlot), wantΔ1);
        }
    }
    check(0);
    nint want = 1;
    for (nint i = 1; i <= 100; i++) {
        x = append(x, (int64)(1));
        check(want);
        if ((nint)(i & (i - 1)) == 0) {
            want = 2 * i;
        }
    }
}

public static slice<int64> One = new int64[]{1}.slice();

public static void TestAppendSliceGrowth(ж<testing.T> Ꮡt) {
    ref var x = ref heap<slice<int64>>(out var Ꮡx);
    void check(nint wantΔ1) {
        if (cap(Ꮡx.ValueSlot) != wantΔ1) {
            Ꮡt.Errorf("len=%d, cap=%d, want cap=%d"u8, len(Ꮡx.ValueSlot), cap(Ꮡx.ValueSlot), wantΔ1);
        }
    }
    check(0);
    nint want = 1;
    for (nint i = 1; i <= 100; i++) {
        x = appendꓸꓸꓸ(x, One);
        check(want);
        if ((nint)(i & (i - 1)) == 0) {
            want = 2 * i;
        }
    }
}

public static void TestGoroutineProfileTrivial(ж<testing.T> Ꮡt) {
    // Calling GoroutineProfile twice in a row should find the same number of goroutines,
    // but it's possible there are goroutines just about to exit, so we might end up
    // with fewer in the second call. Try a few times; it should converge once those
    // zombies are gone.
    for (nint i = 0; ᐧ ; i++) {
        var (n1, ok) = GoroutineProfile(default!); // should fail, there's at least 1 goroutine
        if (n1 < 1 || ok) {
            Ꮡt.Fatalf("GoroutineProfile(nil) = %d, %v, want >0, false"u8, n1, ok);
        }
        (var n2, ok) = GoroutineProfile(new slice<Δruntime.StackRecord>(n1, () => new()));
        if (n2 == n1 && ok) {
            break;
        }
        Ꮡt.Logf("GoroutineProfile(%d) = %d, %v, want %d, true"u8, n1, n2, ok, n1);
        if (i >= 10) {
            Ꮡt.Fatalf("GoroutineProfile not converging"u8);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object goroutineProfileFailedˢ = (@string)"goroutine profile failed"u8;
internal static readonly @string idleˢ = "idle"u8;
internal static readonly @string loadedˢ = "loaded"u8;
internal static readonly @string smallNilˢ = "small-nil"u8;
internal static readonly @string smallˢ = "small"u8;
internal static readonly @string largeNilˢ = "large-nil"u8;
internal static readonly @string largeˢ = "large"u8;
internal static readonly @string sparseNilˢ = "sparse-nil"u8;
internal static readonly @string sparseˢ = "sparse"u8;

public static partial void BenchmarkGoroutineProfile(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    Action<ж<testing.B>> run(Func<bool> fn) {
        var runOne = (ж<testing.B> bΔ1) => {
            var latencies = new slice<time.Duration>(0, (~bΔ1).N);
            bΔ1.ResetTimer();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                var start = time.Now();
                var ok = fn();
                if (!ok) {
                    bΔ1.Fatal(goroutineProfileFailedˢ);
                }
                latencies = append(latencies, time.Since(start));
            }
            bΔ1.StopTimer();
            // Sort latencies then report percentiles.
            slices.Sort<slice<time.Duration>, time.Duration>(latencies);
            bΔ1.ReportMetric((float64)(int64)latencies[len(latencies) * 50 / 100], p50Nsˢ);
            bΔ1.ReportMetric((float64)(int64)latencies[len(latencies) * 90 / 100], p90Nsˢ);
            bΔ1.ReportMetric((float64)(int64)latencies[len(latencies) * 99 / 100], p99Nsˢ);
        };
        var runOneʗ1 = runOne;
        return (ж<testing.B> bΔ2) => {
            bΔ2.Run(idleˢ, runOneʗ1);
            var runOneʗ2 = runOneʗ1;
            bΔ2.Run(loadedˢ, (ж<testing.B> bΔ3) => {
                var stop = applyGCLoad(bΔ3);
                runOneʗ2(bΔ3);
                // Make sure to stop the timer before we wait! The load created above
                // is very heavy-weight and not easy to stop, so we could end up
                // confusing the benchmarking framework for small b.N.
                bΔ3.StopTimer();
                stop();
            });
        };
    }
    // Measure the cost of counting goroutines
    Ꮡb.Run(smallNilˢ, run(() => {
        GoroutineProfile(default!);
        return true;
    }));
    // Measure the cost with a small set of goroutines
    nint n = NumGoroutine();
    ref var p = ref heap<slice<Δruntime.StackRecord>>(out var Ꮡp);
    p = new slice<Δruntime.StackRecord>(2 * n + 2 * GOMAXPROCS(0), () => new());
    Ꮡb.Run(smallˢ, run(() => {
        var (_, ok) = GoroutineProfile(Ꮡp.ValueSlot);
        return ok;
    }));
    // Measure the cost with a large set of goroutines
    var ch = new channel<nint>(0);
    ref var ready = ref heap(new Δsync.WaitGroup(), out var Ꮡready);
    ref var done = ref heap(new Δsync.WaitGroup(), out var Ꮡdone);
    for (nint i = 0; i < 5000; i++) {
        Ꮡready.Add(1);
        Ꮡdone.Add(1);
        var chʗ1 = ch;
        goǃ(() => {
            Ꮡready.Done();
            ᐸꟷ(chʗ1);
            Ꮡdone.Done();
        });
    }
    Ꮡready.Wait();
    // Count goroutines with a large allgs list
    Ꮡb.Run(largeNilˢ, run(() => {
        GoroutineProfile(default!);
        return true;
    }));
    n = NumGoroutine();
    p = new slice<Δruntime.StackRecord>(2 * n + 2 * GOMAXPROCS(0), () => new());
    Ꮡb.Run(largeˢ, run(() => {
        var (_, ok) = GoroutineProfile(Ꮡp.ValueSlot);
        return ok;
    }));
    close(ch);
    Ꮡdone.Wait();
    // Count goroutines with a large (but unused) allgs list
    Ꮡb.Run(sparseNilˢ, run(() => {
        GoroutineProfile(default!);
        return true;
    }));
    // Measure the cost of a large (but unused) allgs list
    n = NumGoroutine();
    p = new slice<Δruntime.StackRecord>(2 * n + 2 * GOMAXPROCS(0), () => new());
    Ꮡb.Run(sparseˢ, run(() => {
        var (_, ok) = GoroutineProfile(Ꮡp.ValueSlot);
        return ok;
    }));
}

public static void TestVersion(ж<testing.T> Ꮡt) {
    // Test that version does not contain \r or \n.
    @string vers = Version();
    if (strings.Contains(vers, "\r"u8) || strings.Contains(vers, "\n"u8)) {
        Ꮡt.Fatalf("cr/nl in version: %q"u8, vers);
    }
}

internal partial struct TestTimediv_type /*dyn*/ {
    internal int64 num;
    internal int32 div;
    internal int32 ret;
    internal int32 rem;
}

public static void TestTimediv(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new TestTimediv_type[]{
        new(
            num: 8,
            div: 2,
            ret: 4,
            rem: 0
        ),
        new(
            num: 9,
            div: 2,
            ret: 4,
            rem: 1
        ),
        new(
            num: 12345000054321L, // Used by runtime.check.

            div: 1000000000,
            ret: 12345,
            rem: 54321
        ),
        new(
            num: 4294967295L,
            div: 2,
            ret: (int32)(2147483648L - 1), // no overflow.

            rem: 1
        ),
        new(
            num: 4294967296L,
            div: 2,
            ret: (int32)(2147483648L - 1), // overflow.

            rem: 0
        ),
        new(
            num: 1099511627776L,
            div: 2,
            ret: (int32)(2147483648L - 1), // overflow.

            rem: 0
        ),
        new(
            num: 1099511627777L,
            div: (int32)(1 << (int)(10)),
            ret: (int32)(1 << (int)(30)),
            rem: 1
        )
    }.slice()) {
        ref var tc = ref heap(new TestTimediv_type(), out var Ꮡtc);
        tc = vᴛ1;

        @string name = fmt.Sprintf("%d div %d"u8, tc.num, tc.div);
        var tcʗ1 = tc;
        Ꮡt.Run(name, (ж<testing.T> tΔ1) => {
            // Double check that the inputs make sense using
            // standard 64-bit division.
            var ret64 = quo(tcʗ1.num, (int64)tcʗ1.div);
            var rem64 = builtin.rem(tcʗ1.num, (int64)tcʗ1.div);
            if (ret64 != (int64)(int32)ret64) {
                // Simulate timediv overflow value.
                ret64 = 2147483648L - 1;
                rem64 = 0;
            }
            if (ret64 != (int64)tcʗ1.ret) {
                tΔ1.Errorf("%d / %d got ret %d rem %d want ret %d rem %d"u8, tcʗ1.num, tcʗ1.div, ret64, rem64, tcʗ1.ret, tcʗ1.rem);
            }
            ref var rem = ref heap(new int32(), out var Ꮡrem);
            var ret = runtime_internal_test_package.Timediv(tcʗ1.num, tcʗ1.div, Ꮡrem);
            if (ret != tcʗ1.ret || rem != tcʗ1.rem) {
                tΔ1.Errorf("timediv %d / %d got ret %d rem %d want ret %d rem %d"u8, tcʗ1.num, tcʗ1.div, ret, rem, tcʗ1.ret, tcʗ1.rem);
            }
        });
    }
}

public static void BenchmarkProcYield(ж<testing.B> Ꮡb) {
    Action<ж<testing.B>> benchN(uint32 n) => (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                runtime_internal_test_package.ProcYield(n);
            }
        };
    Ꮡb.Run("1"u8, benchN(1));
    Ꮡb.Run("10"u8, benchN(10));
    Ꮡb.Run("30"u8, benchN(30)); // active_spin_cnt in lock_sema.go and lock_futex.go
    Ꮡb.Run("100"u8, benchN(100));
    Ꮡb.Run("1000"u8, benchN(1000));
}

public static void BenchmarkOSYield(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    for (nint i = 0; i < b.N; i++) {
        runtime_internal_test_package.OSYield();
    }
}

internal partial struct BenchmarkMutexContention_state /*dyn*/ {
    internal cpu.CacheLinePad _;
    internal Mutex @lock;
    internal cpu.CacheLinePad __;
    internal atomic.Int64 count;
    internal cpu.CacheLinePad ___;
}

public static partial void BenchmarkMutexContention(ж<testing.B> Ꮡb) {
    // Measure throughput of a single mutex with all threads contending
    //
    // Share a single counter across all threads. Progress from any thread is
    // progress for the benchmark as a whole. We don't measure or give points
    // for fairness here, arbitrary delay to any given thread's progress is
    // invisible and allowed.
    //
    // The cache line that holds the count value will need to move between
    // processors, but not as often as the cache line that holds the mutex. The
    // mutex protects access to the count value, which limits contention on that
    // cache line. This is a simple design, but it helps to make the behavior of
    // the benchmark clear. Most real uses of mutex will protect some number of
    // cache lines anyway.
    ref var state = ref heap(new BenchmarkMutexContention_state(), out var Ꮡstate);
    nint procs = GOMAXPROCS(0);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    foreach (var _ᴛ1 in range(procs)) {
        Ꮡwg.Add(1);
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                while (ᐧ) {
                    runtime_internal_test_package.ΔLock(Ꮡstate.of(BenchmarkMutexContention_state.Ꮡlock));
                    var ours = Ꮡstate.of(BenchmarkMutexContention_state.Ꮡcount).Add(1);
                    runtime_internal_test_package.ΔUnlock(Ꮡstate.of(BenchmarkMutexContention_state.Ꮡlock));
                    if (ours >= (int64)Ꮡb.Value.N) {
                        return;
                    }
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nsStreakP100ˢ = "ns/streak-p100"u8;
internal static readonly @string nsStreakP90ˢ = "ns/streak-p90"u8;
internal static readonly @string nsStarveP100ˢ = "ns/starve-p100"u8;
internal static readonly @string nsStarveP90ˢ = "ns/starve-p90"u8;

internal partial struct BenchmarkMutexCapture_state /*dyn*/ {
    internal cpu.CacheLinePad _;
    internal Mutex @lock;
    internal cpu.CacheLinePad __;
    internal atomic.Int64 count;
    internal cpu.CacheLinePad ___;
}

public static partial void BenchmarkMutexCapture(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // Measure mutex fairness.
    //
    // Have several threads contend for a single mutex value. Measure how
    // effectively a single thread is able to capture the lock and report the
    // duration of those "streak" events. Measure how long other individual
    // threads need to wait between their turns with the lock. Report the
    // duration of those "starve" events.
    //
    // Report in terms of wall clock time (assuming a constant time per
    // lock/unlock pair) rather than number of locks/unlocks. This keeps
    // timekeeping overhead out of the critical path, and avoids giving an
    // advantage to lock/unlock implementations that take less time per
    // operation.
    ref var state = ref heap(new BenchmarkMutexCapture_state(), out var Ꮡstate);
    nint procs = GOMAXPROCS(0);
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var histograms = new channel<array<array<nint>>>(0, ChanCargo.Of(null, new nint[] { 2, 65 }));
    foreach (var _ᴛ1 in range(procs)) {
        Ꮡwg.Add(1);
        var histogramsʗ1 = histograms;
        goǃ(() => {
            int64 prev = default!;
            int64 streak = default!;
            ref var histogramΔ1 = ref heap(new array<array<nint>>(2, () => new(65)), out var ᏑhistogramΔ1);
            while (ᐧ) {
                runtime_internal_test_package.ΔLock(Ꮡstate.of(BenchmarkMutexCapture_state.Ꮡlock));
                var ours = Ꮡstate.of(BenchmarkMutexCapture_state.Ꮡcount).Add(1);
                runtime_internal_test_package.ΔUnlock(Ꮡstate.of(BenchmarkMutexCapture_state.Ꮡlock));
                var delta = ours - prev - 1;
                prev = ours;
                if (delta == 0){
                    streak++;
                } else {
                    histogramΔ1[0][global::go.math.bits_package.LeadingZeros64((uint64)streak)]++;
                    histogramΔ1[1][global::go.math.bits_package.LeadingZeros64((uint64)delta)]++;
                    streak = 1;
                }
                if (ours >= (int64)Ꮡb.Value.N) {
                    Ꮡwg.Done();
                    if (delta == 0) {
                        histogramΔ1[0][global::go.math.bits_package.LeadingZeros64((uint64)streak)]++;
                        histogramΔ1[1][global::go.math.bits_package.LeadingZeros64((uint64)delta)]++;
                    }
                    histogramsʗ1.ᐸꟷ(histogramΔ1.Clone());
                    return;
                }
            }
        });
    }
    Ꮡwg.Wait();
    b.StopTimer();
    array<array<nint>> histogram = new(2, () => new(65));
    foreach (var _ᴛ2 in range(procs)) {
        var h = ᐸꟷ(histograms);
        foreach (var (i, _) in h) {
            foreach (var (j, _) in h[i]) {
                histogram[i][j] += h[i][j];
            }
        }
    }
    nint percentile([GoArrayDims(65)] array<nint> h, float64 p) {
        h = h.Clone();
        nint sum = 0;
        foreach (var (i, v) in h.ΔRangeSnapshot()) {
            var bound = ((uint64)(((uint64)1 << (int)(63)))).Rsh((int64)(i));
            sum += (nint)bound * v;
        }
        // Imagine that the longest streak / starvation events were instead half
        // as long but twice in number. (Note that we've pre-multiplied by the
        // [lower] "bound" value.) Continue those splits until we meet the
        // percentile target.
        nint part = 0;
        foreach (var (i, v) in h.ΔRangeSnapshot()) {
            var bound = ((uint64)(((uint64)1 << (int)(63)))).Rsh((int64)(i));
            part += (nint)bound * v;
            // have we trimmed off enough at the head to dip below the percentile goal
            if ((float64)(sum - part) < (float64)sum * p) {
                return (nint)bound;
            }
        }
        return 0;
    }
    var perOp = (float64)b.Elapsed().Nanoseconds() / (float64)b.N;
    b.ReportMetric(perOp * (float64)percentile(histogram[0], 1.0D), nsStreakP100ˢ);
    b.ReportMetric(perOp * (float64)percentile(histogram[0], 0.9D), nsStreakP90ˢ);
    b.ReportMetric(perOp * (float64)percentile(histogram[1], 1.0D), nsStarveP100ˢ);
    b.ReportMetric(perOp * (float64)percentile(histogram[1], 0.9D), nsStarveP90ˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string soloˢ = "Solo"u8;
internal static readonly @string fastPingPongˢ = "FastPingPong"u8;
internal static readonly @string slowPingPongˢ = "SlowPingPong"u8;

internal partial struct BenchmarkMutexHandoff_state /*dyn*/ {
    internal cpu.CacheLinePad _;
    internal Mutex @lock;
    internal cpu.CacheLinePad __;
    internal atomic.Int64 turn;
    internal cpu.CacheLinePad ___;
}

public static void BenchmarkMutexHandoff(ж<testing.B> Ꮡb) {
    Action<ж<testing.B>> testcase(Action<ж<Mutex>> delay) => [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ1) => {
            {
                nint workers = 2; if (GOMAXPROCS(0) < workers) {
                    bΔ1.Skipf("requires GOMAXPROCS >= %d"u8, workers);
                }
            }
            // Measure latency of mutex handoff between threads.
            //
            // Hand off a runtime.mutex between two threads, one running a
            // "coordinator" goroutine and the other running a "worker"
            // goroutine. We don't override the runtime's typical
            // goroutine/thread mapping behavior.
            //
            // Measure the latency, starting when the coordinator enters a call
            // to runtime.unlock and ending when the worker's call to
            // runtime.lock returns. The benchmark can specify a "delay"
            // function to simulate the length of the mutex-holder's critical
            // section, including to arrange for the worker's thread to be in
            // either the "spinning" or "sleeping" portions of the runtime.lock2
            // implementation. Measurement starts after any such "delay".
            //
            // The two threads' goroutines communicate their current position to
            // each other in a non-blocking way via the "turn" state.
            ref var state = ref heap(new BenchmarkMutexHandoff_state(), out var Ꮡstate);
            ref var delta = ref heap(new atomic.Int64(), out var Ꮡdelta);
            ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
            // coordinator:
            //  - acquire the mutex
            //  - set the turn to 2 mod 4, instructing the worker to begin its Lock call
            //  - wait until the mutex is contended
            //  - wait a bit more so the worker can commit to its sleep
            //  - release the mutex and wait for it to be our turn (0 mod 4) again
            Ꮡwg.Add(1);
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    int64 t = default!;
                    foreach (var _ᴛ1 in range((~bΔ1).N)) {
                        runtime_internal_test_package.ΔLock(Ꮡstate.of(BenchmarkMutexHandoff_state.Ꮡlock));
                        Ꮡstate.of(BenchmarkMutexHandoff_state.Ꮡturn).Add(2);
                        delay(Ꮡstate.of(BenchmarkMutexHandoff_state.Ꮡlock));
                        t -= runtime_internal_test_package.Nanotime(); // start the timer
                        runtime_internal_test_package.ΔUnlock(Ꮡstate.of(BenchmarkMutexHandoff_state.Ꮡlock));
                        while ((int64)(Ꮡstate.of(BenchmarkMutexHandoff_state.Ꮡturn).Load() & 0x2) != 0) {
                        }
                    }
                    Ꮡstate.of(BenchmarkMutexHandoff_state.Ꮡturn).Add(1);
                    Ꮡdelta.Add(t);
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            // worker:
            //  - wait until its our turn (2 mod 4)
            //  - acquire and release the mutex
            //  - switch the turn counter back to the coordinator (0 mod 4)
            Ꮡwg.Add(1);
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    int64 t = default!;
                    while (ᐧ) {
                        switch ((int64)(Ꮡstate.of(BenchmarkMutexHandoff_state.Ꮡturn).Load() & 0x3)) {
                        case 0: {
                            break;
                        }
                        case 1 or 3: {
                            Ꮡdelta.Add(t);
                            return;
                        }
                        case 2: {
                            runtime_internal_test_package.ΔLock(Ꮡstate.of(BenchmarkMutexHandoff_state.Ꮡlock));
                            t += runtime_internal_test_package.Nanotime(); // stop the timer
                            runtime_internal_test_package.ΔUnlock(Ꮡstate.of(BenchmarkMutexHandoff_state.Ꮡlock));
                            Ꮡstate.of(BenchmarkMutexHandoff_state.Ꮡturn).Add(2);
                            break;
                        }}

                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            Ꮡwg.Wait();
            bΔ1.ReportMetric((float64)Ꮡdelta.Load() / (float64)(~bΔ1).N, nsOpˢ);
        };
    Ꮡb.Run(soloˢ, (ж<testing.B> bΔ2) => {
        ref var @lock = ref heap(new Mutex(), out var Ꮡlock);
        foreach (var _ᴛ2 in range((~bΔ2).N)) {
            runtime_internal_test_package.ΔLock(Ꮡlock);
            runtime_internal_test_package.ΔUnlock(Ꮡlock);
        }
    });
    Ꮡb.Run(fastPingPongˢ, testcase((ж<Mutex> l) => {
    }));
    Ꮡb.Run(slowPingPongˢ, testcase((ж<Mutex> l) => {
        // Wait for the worker to stop spinning and prepare to sleep
        while (!runtime_internal_test_package.MutexContended(l)) {
        }
        // Wait a bit longer so the OS can finish committing the worker to its
        // sleep. Balance consistency against getting enough iterations.
        const int64 extraNs = 10000;
        for (var t0 = runtime_internal_test_package.Nanotime(); runtime_internal_test_package.Nanotime() - t0 < extraNs; ) {
        }
    }));
}

} // end runtime_test_package
