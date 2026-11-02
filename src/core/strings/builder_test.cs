// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using asan = @internal.asan_package;
using static strings_package;
using testing = testing_package;
using utf8 = go.unicode.utf8_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.unicode;
using static go.strings_internal_test_package;
using strings = strings_package;

partial class strings_test_package {

internal static void check(ж<testing.T> t, ж<strings.Builder> Ꮡb, @string want) {
    ref var b = ref Ꮡb.DerefOrNull();

    t.Helper();
    @string got = b.String();
    if (got != want) {
        t.Errorf("String: got %#q; want %#q", got, want);
        return;
    }
    {
        nint n = b.Len(); if (n != len(got)) {
            t.Errorf("Len: got %d; but len(String()) is %d", n, len(got));
        }
    }
    {
        nint n = b.Cap(); if (n < len(got)) {
            t.Errorf("Cap: got %d; but len(String()) is %d", n, len(got));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string helloˢ => ref helloˢᵇ;
private static readonly @string helloˢᵇ = "hello"u8;
internal static ref readonly @string helloˢ2 => ref helloˢ2ᵇ;
private static readonly @string helloˢ2ᵇ = "hello "u8;
internal static ref readonly @string worldˢ => ref worldˢᵇ;
private static readonly @string worldˢᵇ = "world"u8;
internal static ref readonly @string helloWorldˢ => ref helloWorldˢᵇ;
private static readonly @string helloWorldˢᵇ = "hello world"u8;

public static void TestBuilder(ж<testing.T> t) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    check(t, Ꮡb, ""u8);
    var (n, err) = Ꮡb.WriteString(helloˢ);
    if (err != nil || n != 5) {
        t.Errorf("WriteString: got %d,%s; want 5,nil", n, err);
    }
    check(t, Ꮡb, helloˢ);
    {
        err = Ꮡb.WriteByte((byte)' '); if (err != nil) {
            t.Errorf("WriteByte: %s", err);
        }
    }
    check(t, Ꮡb, helloˢ2);
    (n, err) = Ꮡb.WriteString(worldˢ);
    if (err != nil || n != 5) {
        t.Errorf("WriteString: got %d,%s; want 5,nil", n, err);
    }
    check(t, Ꮡb, helloWorldˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string alphaˢ => ref alphaˢᵇ;
private static readonly @string alphaˢᵇ = "alpha"u8;
internal static ref readonly @string betaˢ => ref betaˢᵇ;
private static readonly @string betaˢᵇ = "beta"u8;
internal static ref readonly @string alphabetaˢ => ref alphabetaˢᵇ;
private static readonly @string alphabetaˢᵇ = "alphabeta"u8;
internal static ref readonly @string gammaˢ => ref gammaˢᵇ;
private static readonly @string gammaˢᵇ = "gamma"u8;
internal static ref readonly @string alphabetagammaˢ => ref alphabetagammaˢᵇ;
private static readonly @string alphabetagammaˢᵇ = "alphabetagamma"u8;

public static void TestBuilderString(ж<testing.T> t) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    Ꮡb.WriteString(alphaˢ);
    check(t, Ꮡb, alphaˢ);
    @string s1 = b.String();
    Ꮡb.WriteString(betaˢ);
    check(t, Ꮡb, alphabetaˢ);
    @string s2 = b.String();
    Ꮡb.WriteString(gammaˢ);
    check(t, Ꮡb, alphabetagammaˢ);
    @string s3 = b.String();

    // Check that subsequent operations didn't change the returned strings.
    {
        @string want = alphaˢ; if (s1 != want) {
            t.Errorf("first String result is now %q; want %q", s1, want);
        }
    }
    {
        @string want = alphabetaˢ; if (s2 != want) {
            t.Errorf("second String result is now %q; want %q", s2, want);
        }
    }
    {
        @string want = alphabetagammaˢ; if (s3 != want) {
            t.Errorf("third String result is now %q; want %q", s3, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string aaaˢ => ref aaaˢᵇ;
private static readonly @string aaaˢᵇ = "aaa"u8;
internal static ref readonly @string bbbˢ => ref bbbˢᵇ;
private static readonly @string bbbˢᵇ = "bbb"u8;

public static void TestBuilderReset(ж<testing.T> t) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    check(t, Ꮡb, ""u8);
    Ꮡb.WriteString(aaaˢ);
    @string s = b.String();
    check(t, Ꮡb, aaaˢ);
    b.Reset();
    check(t, Ꮡb, ""u8);

    // Ensure that writing after Reset doesn't alter
    // previously returned strings.
    Ꮡb.WriteString(bbbˢ);
    check(t, Ꮡb, bbbˢ);
    {
        @string want = aaaˢ; if (s != want) {
            t.Errorf("previous String result changed after Reset: got %q; want %q", s, want);
        }
    }
}

public static void TestBuilderGrow(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, growLen) in slice<nint>([0, 100, 1000, 10000, 100000])) {
            if (asan.Enabled) {
                t.Logf("skipping allocs check for growLen %d: extra allocs with -asan; see #70079", growLen);
                continue;
            }
            var p = bytes.Repeat(slice<byte>([(byte)'a']), growLen);
            var pʗ1 = p;
            var allocs = testing.AllocsPerRun(100, () => {
                ref var b = ref heap(new strings.Builder(), out var Ꮡb);
                Ꮡb.Grow(growLen); // should be only alloc, when growLen > 0
                if (b.Cap() < growLen) {
                    t.Fatalf("growLen=%d: Cap() is lower than growLen", growLen);
                }
                Ꮡb.Write(pʗ1);
                if (b.String() != ((@string)pʗ1)) {
                    t.Fatalf("growLen=%d: bad data written after Grow", growLen);
                }
            });
            nint wantAllocs = 1;
            if (growLen == 0) {
                wantAllocs = 0;
            }
            {
                nint g = (nint)allocs;
                nint w = wantAllocs; if (g != w) {
                    t.Errorf("growLen=%d: got %d allocs during Write; want %v", growLen, g, w);
                }
            }
        }
        // when growLen < 0, should panic
        ref var a = ref heap(new strings.Builder(), out var Ꮡa);
        nint n = -1;
        defer(() => {
            {
                var r = recover(); if (r == nil) {
                    t.Errorf("a.Grow(%d) should panic()", n);
                }
            }
        }, ref ᒐ);
        Ꮡa.Grow(n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string s0ᶜ => ref s0ᶜᵇ;
private static readonly @string s0ᶜᵇ = "hello 世界"u8;

internal partial struct TestBuilderWrite2_type /*dyn*/ {
    internal @string name;
    internal Func<ж<strings.Builder>, (nint, error)> fn;
    internal nint n;
    internal @string want;
}

public static void TestBuilderWrite2(ж<testing.T> t) {
    @string s0 = s0ᶜ;
    foreach (var (_, vᴛ1) in slice<TestBuilderWrite2_type>([
        new(
            "Write",
            (ж<strings.Builder> b) => b.Write(slice<byte>(s0)),
            len(s0),
            s0
        ),
        new(
            "WriteRune",
            (ж<strings.Builder> b) => b.WriteRune('a'),
            1,
            "a"
        ),
        new(
            "WriteRuneWide",
            (ж<strings.Builder> b) => b.WriteRune('世'),
            3,
            "世"
        ),
        new(
            "WriteString",
            (ж<strings.Builder> b) => b.WriteString(s0),
            len(s0),
            s0
        )
    ])) {
        ref var tt = ref heap(new TestBuilderWrite2_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        t.Run(tt.name, (ж<testing.T> tΔ1) => {
            ref var b = ref heap(new strings.Builder(), out var Ꮡb);
            var (n, err) = ttʗ1.fn(Ꮡb);
            if (err != nil) {
                tΔ1.Fatalf("first call: got %s", err);
            }
            if (n != ttʗ1.n) {
                tΔ1.Errorf("first call: got n=%d; want %d", n, ttʗ1.n);
            }
            check(tΔ1, Ꮡb, ttʗ1.want);

            (n, err) = ttʗ1.fn(Ꮡb);
            if (err != nil) {
                tΔ1.Fatalf("second call: got %s", err);
            }
            if (n != ttʗ1.n) {
                tΔ1.Errorf("second call: got n=%d; want %d", n, ttʗ1.n);
            }
            check(tΔ1, Ꮡb, ttʗ1.want + ttʗ1.want);
        });
    }
}

public static void TestBuilderWriteByte(ж<testing.T> t) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    {
        var err = Ꮡb.WriteByte((byte)'a'); if (err != nil) {
            t.Error(err);
        }
    }
    {
        var err = Ꮡb.WriteByte(0); if (err != nil) {
            t.Error(err);
        }
    }
    check(t, Ꮡb, "a\x00");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;
internal static ref readonly @string abcdeˢ => ref abcdeˢᵇ;
private static readonly @string abcdeˢᵇ = "abcde"u8;

public static void TestBuilderAllocs(ж<testing.T> t) {
    if (asan.Enabled) {
        t.Skip(testAllocatesMoreWithˢ);
    }
    // Issue 23382; verify that copyCheck doesn't force the
    // Builder to escape and be heap allocated.
    var n = testing.AllocsPerRun(10000, () => {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        Ꮡb.Grow(5);
        Ꮡb.WriteString(abcdeˢ);
        _ = b.String();
    });
    if (n != 1D) {
        t.Errorf("Builder allocs = %v; want 1", n);
    }
}

internal partial struct TestBuilderCopyPanic_tests /*dyn*/ {
    internal @string name;
    internal Action fn;
    internal bool wantPanic;
}

public static partial void TestBuilderCopyPanic(ж<testing.T> t) {
    var tests = slice<TestBuilderCopyPanic_tests>([
        new(
            name: "String",
            wantPanic: false,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteByte((byte)'x');
                var b = a;
                _ = b.String(); // appease vet
            }
        ),
        new(
            name: "Len",
            wantPanic: false,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteByte((byte)'x');
                var b = a;
                b.Len();
            }
        ),
        new(
            name: "Cap",
            wantPanic: false,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteByte((byte)'x');
                var b = a;
                b.Cap();
            }
        ),
        new(
            name: "Reset",
            wantPanic: false,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteByte((byte)'x');
                ref var b = ref heap<strings.Builder>(a, out var Ꮡb);
                b.Reset();
                Ꮡb.WriteByte((byte)'y');
            }
        ),
        new(
            name: "Write",
            wantPanic: true,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.Write(slice<byte>("x"u8));
                ref var b = ref heap<strings.Builder>(a, out var Ꮡb);
                Ꮡb.Write(slice<byte>("y"u8));
            }
        ),
        new(
            name: "WriteByte",
            wantPanic: true,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteByte((byte)'x');
                ref var b = ref heap<strings.Builder>(a, out var Ꮡb);
                Ꮡb.WriteByte((byte)'y');
            }
        ),
        new(
            name: "WriteString",
            wantPanic: true,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteString("x");
                ref var b = ref heap<strings.Builder>(a, out var Ꮡb);
                Ꮡb.WriteString("y");
            }
        ),
        new(
            name: "WriteRune",
            wantPanic: true,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteRune('x');
                ref var b = ref heap<strings.Builder>(a, out var Ꮡb);
                Ꮡb.WriteRune('y');
            }
        ),
        new(
            name: "Grow",
            wantPanic: true,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.Grow(1);
                ref var b = ref heap<strings.Builder>(a, out var Ꮡb);
                Ꮡb.Grow(2);
            }
        )
    ]);
    foreach (var (_, vᴛ1) in tests) {
        ref var tt = ref heap(new TestBuilderCopyPanic_tests(), out var Ꮡtt);
        tt = vᴛ1;

        var didPanic = new channel<bool>(0);
        var didPanicʗ1 = didPanic;
        var ttʗ1 = tt;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                var didPanicʗ2 = didPanicʗ1;
                defer(() => {
                    didPanicʗ2.ᐸꟷ(recover() != nil);
                }, ref ᒐ);
                ttʗ1.fn();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        {
            var got = ᐸꟷ(didPanic); if (got != tt.wantPanic) {
                t.Errorf("%s: panicked = %v; want %v", tt.name, got, tt.wantPanic);
            }
        }
    }
}

public static void TestBuilderWriteInvalidRune(ж<testing.T> t) {
    // Invalid runes, including negative ones, should be written as
    // utf8.RuneError.
    foreach (var (_, r) in slice<rune>([-1, utf8.MaxRune + 1])) {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        Ꮡb.WriteRune(r);
        check(t, Ꮡb, "\uFFFD");
    }
}

internal static slice<byte> someBytes = slice<byte>("some bytes sdljlk jsklj3lkjlk djlkjw"u8);

internal static @string sinkS;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string noGrowˢ => ref noGrowˢᵇ;
private static readonly @string noGrowˢᵇ = "1Write_NoGrow"u8;
internal static ref readonly @string noGrowˢ2 => ref noGrowˢ2ᵇ;
private static readonly @string noGrowˢ2ᵇ = "3Write_NoGrow"u8;
internal static ref readonly @string growˢ => ref growˢᵇ;
private static readonly @string growˢᵇ = "3Write_Grow"u8;

internal static void benchmarkBuilder(ж<testing.B> b, Action<ж<testing.B>, nint, bool> f) {
    b.Run(noGrowˢ, (ж<testing.B> bΔ1) => {
        bΔ1.ReportAllocs();
        f(bΔ1, 1, false);
    });
    b.Run(noGrowˢ2, (ж<testing.B> bΔ2) => {
        bΔ2.ReportAllocs();
        f(bΔ2, 3, false);
    });
    b.Run(growˢ, (ж<testing.B> bΔ3) => {
        bΔ3.ReportAllocs();
        f(bΔ3, 3, true);
    });
}

public static void BenchmarkBuildString_Builder(ж<testing.B> b) {
    benchmarkBuilder(b, (ж<testing.B> bΔ1, nint numWrite, bool grow) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
            if (grow) {
                Ꮡbuf.Grow(len(someBytes) * numWrite);
            }
            for (nint iΔ1 = 0; iΔ1 < numWrite; iΔ1++) {
                Ꮡbuf.Write(someBytes);
            }
            sinkS = buf.String();
        }
    });
}

public static void BenchmarkBuildString_WriteString(ж<testing.B> b) {
    @string someString = (@string)someBytes;
    benchmarkBuilder(b, (ж<testing.B> bΔ1, nint numWrite, bool grow) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
            if (grow) {
                Ꮡbuf.Grow(len(someString) * numWrite);
            }
            for (nint iΔ1 = 0; iΔ1 < numWrite; iΔ1++) {
                Ꮡbuf.WriteString(someString);
            }
            sinkS = buf.String();
        }
    });
}

public static void BenchmarkBuildString_ByteBuffer(ж<testing.B> b) {
    benchmarkBuilder(b, (ж<testing.B> bΔ1, nint numWrite, bool grow) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            ref var buf = ref heap(new bytes.Buffer(), out var Ꮡbuf);
            if (grow) {
                buf.Grow(len(someBytes) * numWrite);
            }
            for (nint iΔ1 = 0; iΔ1 < numWrite; iΔ1++) {
                buf.Write(someBytes);
            }
            sinkS = Ꮡbuf.String();
        }
    });
}

public static void TestBuilderGrowSizeclasses(ж<testing.T> t) {
    if (asan.Enabled) {
        t.Skip(testAllocatesMoreWithˢ);
    }
    @string s = Repeat("a", 19);
    var allocs = testing.AllocsPerRun(100, () => {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        Ꮡb.Grow(18);
        Ꮡb.WriteString(s);
        _ = b.String();
    });
    if (allocs > 1D) {
        t.Fatalf("unexpected amount of allocations: %v, want: 1", allocs);
    }
}

} // end strings_test_package
