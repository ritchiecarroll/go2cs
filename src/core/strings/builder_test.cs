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

internal static void check(ж<testing.T> Ꮡt, ж<strings.Builder> Ꮡb, @string want) {
    ref var b = ref Ꮡb.DerefOrNull();

    Ꮡt.Helper();
    @string got = b.String();
    if (got != want) {
        Ꮡt.Errorf("String: got %#q; want %#q"u8, got, want);
        return;
    }
    {
        nint n = b.Len(); if (n != len(got)) {
            Ꮡt.Errorf("Len: got %d; but len(String()) is %d"u8, n, len(got));
        }
    }
    {
        nint n = b.Cap(); if (n < len(got)) {
            Ꮡt.Errorf("Cap: got %d; but len(String()) is %d"u8, n, len(got));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string helloˢ = "hello"u8;
internal static readonly @string helloˢ2 = "hello "u8;
internal static readonly @string worldˢ = "world"u8;
internal static readonly @string helloWorldˢ = "hello world"u8;

public static void TestBuilder(ж<testing.T> Ꮡt) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    check(Ꮡt, Ꮡb, ""u8);
    var (n, err) = Ꮡb.WriteString(helloˢ);
    if (err != default! || n != 5) {
        Ꮡt.Errorf("WriteString: got %d,%s; want 5,nil"u8, n, err);
    }
    check(Ꮡt, Ꮡb, helloˢ);
    {
        err = Ꮡb.WriteByte((rune)' '); if (err != default!) {
            Ꮡt.Errorf("WriteByte: %s"u8, err);
        }
    }
    check(Ꮡt, Ꮡb, helloˢ2);
    (n, err) = Ꮡb.WriteString(worldˢ);
    if (err != default! || n != 5) {
        Ꮡt.Errorf("WriteString: got %d,%s; want 5,nil"u8, n, err);
    }
    check(Ꮡt, Ꮡb, helloWorldˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string alphaˢ = "alpha"u8;
internal static readonly @string betaˢ = "beta"u8;
internal static readonly @string alphabetaˢ = "alphabeta"u8;
internal static readonly @string gammaˢ = "gamma"u8;
internal static readonly @string alphabetagammaˢ = "alphabetagamma"u8;

public static void TestBuilderString(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    Ꮡb.WriteString(alphaˢ);
    check(Ꮡt, Ꮡb, alphaˢ);
    @string s1 = b.String();
    Ꮡb.WriteString(betaˢ);
    check(Ꮡt, Ꮡb, alphabetaˢ);
    @string s2 = b.String();
    Ꮡb.WriteString(gammaˢ);
    check(Ꮡt, Ꮡb, alphabetagammaˢ);
    @string s3 = b.String();
    // Check that subsequent operations didn't change the returned strings.
    {
        @string want = alphaˢ; if (s1 != want) {
            Ꮡt.Errorf("first String result is now %q; want %q"u8, s1, want);
        }
    }
    {
        @string want = alphabetaˢ; if (s2 != want) {
            Ꮡt.Errorf("second String result is now %q; want %q"u8, s2, want);
        }
    }
    {
        @string want = alphabetagammaˢ; if (s3 != want) {
            Ꮡt.Errorf("third String result is now %q; want %q"u8, s3, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string aaaˢ = "aaa"u8;
internal static readonly @string bbbˢ = "bbb"u8;

public static void TestBuilderReset(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    check(Ꮡt, Ꮡb, ""u8);
    Ꮡb.WriteString(aaaˢ);
    @string s = b.String();
    check(Ꮡt, Ꮡb, aaaˢ);
    b.Reset();
    check(Ꮡt, Ꮡb, ""u8);
    // Ensure that writing after Reset doesn't alter
    // previously returned strings.
    Ꮡb.WriteString(bbbˢ);
    check(Ꮡt, Ꮡb, bbbˢ);
    {
        @string want = aaaˢ; if (s != want) {
            Ꮡt.Errorf("previous String result changed after Reset: got %q; want %q"u8, s, want);
        }
    }
}

public static void TestBuilderGrow(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, growLen) in new nint[]{0, 100, 1000, 10000, 100000}.slice()) {
            if (asan.Enabled) {
                Ꮡt.Logf("skipping allocs check for growLen %d: extra allocs with -asan; see #70079"u8, growLen);
                continue;
            }
            var p = bytes.Repeat(new byte[]{(rune)'a'}.slice(), growLen);
            var pʗ1 = p;
            var allocs = testing.AllocsPerRun(100, () => {
                ref var b = ref heap(new strings.Builder(), out var Ꮡb);
                Ꮡb.Grow(growLen); // should be only alloc, when growLen > 0
                if (b.Cap() < growLen) {
                    Ꮡt.Fatalf("growLen=%d: Cap() is lower than growLen"u8, growLen);
                }
                Ꮡb.Write(pʗ1);
                if (b.String() != ((@string)pʗ1)) {
                    Ꮡt.Fatalf("growLen=%d: bad data written after Grow"u8, growLen);
                }
            });
            nint wantAllocs = 1;
            if (growLen == 0) {
                wantAllocs = 0;
            }
            {
                nint g = (nint)allocs;
                nint w = wantAllocs; if (g != w) {
                    Ꮡt.Errorf("growLen=%d: got %d allocs during Write; want %v"u8, growLen, g, w);
                }
            }
        }
        // when growLen < 0, should panic
        ref var a = ref heap(new strings.Builder(), out var Ꮡa);
        nint n = -1;
        defer(() => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Errorf("a.Grow(%d) should panic()"u8, n);
                }
            }
        }, ref ᒐ);
        Ꮡa.Grow(n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string s0ᶜ = "hello 世界"u8;

[GoType("dyn")] internal partial struct TestBuilderWrite2_type {
    internal @string name;
    internal Func<ж<strings.Builder>, (nint, error)> fn;
    internal nint n;
    internal @string want;
}

public static void TestBuilderWrite2(ж<testing.T> Ꮡt) {
    @string s0 = s0ᶜ;
    foreach (var (_, vᴛ1) in new TestBuilderWrite2_type[]{
        new(
            "Write"u8,
            (ж<strings.Builder> b) => b.Write(slice<byte>(s0)),
            len(s0),
            s0
        ),
        new(
            "WriteRune"u8,
            (ж<strings.Builder> b) => b.WriteRune((rune)'a'),
            1,
            "a"u8
        ),
        new(
            "WriteRuneWide"u8,
            (ж<strings.Builder> b) => b.WriteRune((rune)'世'),
            3,
            "世"u8
        ),
        new(
            "WriteString"u8,
            (ж<strings.Builder> b) => b.WriteString(s0),
            len(s0),
            s0
        )
    }.slice()) {
        ref var tt = ref heap(new TestBuilderWrite2_type(), out var Ꮡtt);
        tt = vᴛ1;

        var ttʗ1 = tt;
        Ꮡt.Run(tt.name, (ж<testing.T> tΔ1) => {
            ref var b = ref heap(new strings.Builder(), out var Ꮡb);
            var (n, err) = ttʗ1.fn(Ꮡb);
            if (err != default!) {
                tΔ1.Fatalf("first call: got %s"u8, err);
            }
            if (n != ttʗ1.n) {
                tΔ1.Errorf("first call: got n=%d; want %d"u8, n, ttʗ1.n);
            }
            check(tΔ1, Ꮡb, ttʗ1.want);
            (n, err) = ttʗ1.fn(Ꮡb);
            if (err != default!) {
                tΔ1.Fatalf("second call: got %s"u8, err);
            }
            if (n != ttʗ1.n) {
                tΔ1.Errorf("second call: got n=%d; want %d"u8, n, ttʗ1.n);
            }
            check(tΔ1, Ꮡb, ttʗ1.want + ttʗ1.want);
        });
    }
}

public static void TestBuilderWriteByte(ж<testing.T> Ꮡt) {
    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    {
        var err = Ꮡb.WriteByte((rune)'a'); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    {
        var err = Ꮡb.WriteByte(0); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    check(Ꮡt, Ꮡb, "a\x00"u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan; see #70079"u8;
internal static readonly @string abcdeˢ = "abcde"u8;

public static void TestBuilderAllocs(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
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
        Ꮡt.Errorf("Builder allocs = %v; want 1"u8, n);
    }
}

[GoType("dyn")] internal partial struct TestBuilderCopyPanic_tests {
    internal @string name;
    internal Action fn;
    internal bool wantPanic;
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestBuilderCopyPanic(ж<testing.T> Ꮡt) {
    var tests = new TestBuilderCopyPanic_tests[]{
        new(
            name: "String"u8,
            wantPanic: false,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteByte((rune)'x');
                var b = a;
                _ = b.String(); // appease vet
            }
        ),
        new(
            name: "Len"u8,
            wantPanic: false,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteByte((rune)'x');
                var b = a;
                b.Len();
            }
        ),
        new(
            name: "Cap"u8,
            wantPanic: false,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteByte((rune)'x');
                var b = a;
                b.Cap();
            }
        ),
        new(
            name: "Reset"u8,
            wantPanic: false,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteByte((rune)'x');
                ref var b = ref heap<strings.Builder>(out var Ꮡb);
                b = a;
                b.Reset();
                Ꮡb.WriteByte((rune)'y');
            }
        ),
        new(
            name: "Write"u8,
            wantPanic: true,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.Write(slice<byte>("x"u8));
                ref var b = ref heap<strings.Builder>(out var Ꮡb);
                b = a;
                Ꮡb.Write(slice<byte>("y"u8));
            }
        ),
        new(
            name: "WriteByte"u8,
            wantPanic: true,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteByte((rune)'x');
                ref var b = ref heap<strings.Builder>(out var Ꮡb);
                b = a;
                Ꮡb.WriteByte((rune)'y');
            }
        ),
        new(
            name: "WriteString"u8,
            wantPanic: true,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteString("x"u8);
                ref var b = ref heap<strings.Builder>(out var Ꮡb);
                b = a;
                Ꮡb.WriteString("y"u8);
            }
        ),
        new(
            name: "WriteRune"u8,
            wantPanic: true,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.WriteRune((rune)'x');
                ref var b = ref heap<strings.Builder>(out var Ꮡb);
                b = a;
                Ꮡb.WriteRune((rune)'y');
            }
        ),
        new(
            name: "Grow"u8,
            wantPanic: true,
            fn: () => {
                ref var a = ref heap(new strings.Builder(), out var Ꮡa);
                Ꮡa.Grow(1);
                ref var b = ref heap<strings.Builder>(out var Ꮡb);
                b = a;
                Ꮡb.Grow(2);
            }
        )
    }.slice();
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
                    didPanicʗ2.ᐸꟷ(recover() != default!);
                }, ref ᒐ);
                ttʗ1.fn();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        {
            var got = ᐸꟷ(didPanic); if (got != tt.wantPanic) {
                Ꮡt.Errorf("%s: panicked = %v; want %v"u8, tt.name, got, tt.wantPanic);
            }
        }
    }
}

public static void TestBuilderWriteInvalidRune(ж<testing.T> Ꮡt) {
    // Invalid runes, including negative ones, should be written as
    // utf8.RuneError.
    foreach (var (_, r) in new rune[]{-1, utf8.MaxRune + 1}.slice()) {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        Ꮡb.WriteRune(r);
        check(Ꮡt, Ꮡb, "\uFFFD"u8);
    }
}

internal static slice<byte> someBytes = slice<byte>("some bytes sdljlk jsklj3lkjlk djlkjw"u8);

internal static @string sinkS;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string noGrowˢ = "1Write_NoGrow"u8;
internal static readonly @string noGrowˢ2 = "3Write_NoGrow"u8;
internal static readonly @string growˢ = "3Write_Grow"u8;

internal static void benchmarkBuilder(ж<testing.B> Ꮡb, Action<ж<testing.B>, nint, bool> f) {
    Ꮡb.Run(noGrowˢ, (ж<testing.B> bΔ1) => {
        bΔ1.ReportAllocs();
        f(bΔ1, 1, false);
    });
    Ꮡb.Run(noGrowˢ2, (ж<testing.B> bΔ2) => {
        bΔ2.ReportAllocs();
        f(bΔ2, 3, false);
    });
    Ꮡb.Run(growˢ, (ж<testing.B> bΔ3) => {
        bΔ3.ReportAllocs();
        f(bΔ3, 3, true);
    });
}

public static void BenchmarkBuildString_Builder(ж<testing.B> Ꮡb) {
    benchmarkBuilder(Ꮡb, (ж<testing.B> bΔ1, nint numWrite, bool grow) => {
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

public static void BenchmarkBuildString_WriteString(ж<testing.B> Ꮡb) {
    @string someString = ((@string)someBytes);
    benchmarkBuilder(Ꮡb, (ж<testing.B> bΔ1, nint numWrite, bool grow) => {
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

public static void BenchmarkBuildString_ByteBuffer(ж<testing.B> Ꮡb) {
    benchmarkBuilder(Ꮡb, (ж<testing.B> bΔ1, nint numWrite, bool grow) => {
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

public static void TestBuilderGrowSizeclasses(ж<testing.T> Ꮡt) {
    if (asan.Enabled) {
        Ꮡt.Skip(testAllocatesMoreWithˢ);
    }
    @string s = Repeat("a"u8, 19);
    var allocs = testing.AllocsPerRun(100, () => {
        ref var b = ref heap(new strings.Builder(), out var Ꮡb);
        Ꮡb.Grow(18);
        Ꮡb.WriteString(s);
        _ = b.String();
    });
    if (allocs > 1D) {
        Ꮡt.Fatalf("unexpected amount of allocations: %v, want: 1"u8, allocs);
    }
}

} // end strings_test_package
