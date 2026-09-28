// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static context_package;
using errors = errors_package;
using fmt = fmt_package;
using rand = math.rand_package;
using Δruntime = runtime_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using context = context_package;
using math;
using static go.context_internal_test_package;
using ꓸꓸꓸany = Span<any>;

partial class context_test_package {

// Each XTestFoo in context_test.go must be called from a TestFoo here to run.
public static void TestParentFinishesChild(ж<testing.T> Ꮡt) {
    context_internal_test_package.XTestParentFinishesChild(new context_test_package.testing_TжtestingT(Ꮡt)); // uses unexported context types
}

public static void TestChildFinishesFirst(ж<testing.T> Ꮡt) {
    context_internal_test_package.XTestChildFinishesFirst(new context_test_package.testing_TжtestingT(Ꮡt)); // uses unexported context types
}

public static void TestCancelRemoves(ж<testing.T> Ꮡt) {
    context_internal_test_package.XTestCancelRemoves(new context_test_package.testing_TжtestingT(Ꮡt)); // uses unexported context types
}

public static void TestCustomContextGoroutines(ж<testing.T> Ꮡt) {
    context_internal_test_package.XTestCustomContextGoroutines(new context_test_package.testing_TжtestingT(Ꮡt)); // reads the context.goroutines counter
}

// The following are regular tests in package context_test.

// otherContext is a Context that's not one of the types defined in context.go.
// This lets us test code paths that differ based on the underlying type of the
// Context.
[GoType] partial struct otherContext {
    public context_package.Context Context;
}

internal static time.Duration shortDuration => /* 1 * time.Millisecond */ 1000000; // a reasonable duration to block in a test
internal static time.Duration veryLongDuration => /* 1000 * time.Hour */ 3600000000000000; // an arbitrary upper bound on the test's running time

// quiescent returns an arbitrary duration by which the program should have
// completed any remaining work and reached a steady (idle) state.
internal static time.Duration quiescent(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (deadline, ok) = t.Deadline();
    if (!ok) {
        return (time.Duration)(5000000000L);
    }
    time.Duration arbitraryCleanupMargin = /* 1 * time.Second */ 1000000000;
    return time.Until(deadline) - arbitraryCleanupMargin;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextBackgroundˢ = "context.Background"u8;

public static void TestBackground(ж<testing.T> Ꮡt) {
    var c = Background();
    if (c == default!) {
        Ꮡt.Fatalf("Background returned nil"u8);
    }
    var selᴛ9 = c.Done();
    switch (trySelect(ᐸꟷ(selᴛ9, ꓸꓸꓸ))) {
    case 0 when selᴛ9.ꟷᐳ(out var x): {
        Ꮡt.Errorf("<-c.Done() == %v want nothing (it should block)"u8, x);
        break;
    }
    default: {
        break;
    }}
    {
        @string got = fmt.Sprint(c);
        @string want = contextBackgroundˢ; if (got != want) {
            Ꮡt.Errorf("Background().String() = %q want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextTodoˢ = "context.TODO"u8;

public static void TestTODO(ж<testing.T> Ꮡt) {
    var c = TODO();
    if (c == default!) {
        Ꮡt.Fatalf("TODO returned nil"u8);
    }
    var selᴛ10 = c.Done();
    switch (trySelect(ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
    case 0 when selᴛ10.ꟷᐳ(out var x): {
        Ꮡt.Errorf("<-c.Done() == %v want nothing (it should block)"u8, x);
        break;
    }
    default: {
        break;
    }}
    {
        @string got = fmt.Sprint(c);
        @string want = contextTodoˢ; if (got != want) {
            Ꮡt.Errorf("TODO().String() = %q want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextBackgroundˢ2 = "context.Background.WithCancel"u8;

public static void TestWithCancel(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (c1, cancel) = WithCancel(Background());
    {
        @string got = fmt.Sprint(c1);
        @string want = contextBackgroundˢ2; if (got != want) {
            Ꮡt.Errorf("c1.String() = %q want %q"u8, got, want);
        }
    }
    var o = new otherContext(c1);
    var (c2, _) = WithCancel(o);
    var contexts = new context.Context[]{c1, o, c2}.slice();
    foreach (var (i, c) in contexts) {
        {
            var d = c.Done(); if (d == default!) {
                Ꮡt.Errorf("c[%d].Done() == %v want non-nil"u8, i, d);
            }
        }
        {
            var e = c.Err(); if (e != default!) {
                Ꮡt.Errorf("c[%d].Err() == %v want nil"u8, i, e);
            }
        }
        var selᴛ11 = c.Done();
        switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
        case 0 when selᴛ11.ꟷᐳ(out var x): {
            Ꮡt.Errorf("<-c.Done() == %v want nothing (it should block)"u8, x);
            break;
        }
        default: {
            break;
        }}
    }
    cancel(); // Should propagate synchronously.
    foreach (var (i, c) in contexts) {
        var selᴛ12 = c.Done();
        switch (trySelect(ᐸꟷ(selᴛ12, ꓸꓸꓸ))) {
        case 0 when selᴛ12.ꟷᐳ(out _): {
            break;
        }
        default: {
            Ꮡt.Errorf("<-c[%d].Done() blocked, but shouldn't have"u8, i);
            break;
        }}
        {
            var e = c.Err(); if (!AreEqual(e, Canceled)) {
                Ꮡt.Errorf("c[%d].Err() == %v want %v"u8, i, e, Canceled);
            }
        }
    }
}

internal static void testDeadline(context.Context c, @string name, ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Helper();
        var d = quiescent(Ꮡt);
        var timer = time.NewTimer(d);
        var timerʗ1 = timer;
        defer(() => timerʗ1.Stop(), ref ᒐ);
        var selᴛ13 = (~timer).C;
        var selᴛ14 = c.Done();
        switch (select(ᐸꟷ(selᴛ13, ꓸꓸꓸ), ᐸꟷ(selᴛ14, ꓸꓸꓸ))) {
        case 0 when selᴛ13.ꟷᐳ(out _): {
            Ꮡt.Fatalf("%s: context not timed out after %v"u8, name, d);
            break;
        }
        case 1 when selᴛ14.ꟷᐳ(out _): {
            break;
        }}
        {
            var e = c.Err(); if (!AreEqual(e, DeadlineExceeded)) {
                Ꮡt.Errorf("%s: c.Err() == %v; want %v"u8, name, e, DeadlineExceeded);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextBackgroundˢ3 = "context.Background.WithDeadline("u8;
internal static readonly @string withDeadlineˢ = "WithDeadline"u8;
internal static readonly @string withDeadlineOtherContextˢ = "WithDeadline+otherContext"u8;
internal static readonly @string withDeadlineOtherContextˢ2 = "WithDeadline+otherContext+WithDeadline"u8;
internal static readonly @string withDeadlineInthepastˢ = "WithDeadline+inthepast"u8;
internal static readonly @string withDeadlineNowˢ = "WithDeadline+now"u8;

public static void TestDeadline(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var (c, _) = WithDeadline(Background(), time.Now().Add(shortDuration));
    {
        @string got = fmt.Sprint(c);
        @string prefix = contextBackgroundˢ3; if (!strings.HasPrefix(got, prefix)) {
            Ꮡt.Errorf("c.String() = %q want prefix %q"u8, got, prefix);
        }
    }
    testDeadline(c, withDeadlineˢ, Ꮡt);
    (c, _) = WithDeadline(Background(), time.Now().Add(shortDuration));
    var o = new otherContext(c);
    testDeadline(o, withDeadlineOtherContextˢ, Ꮡt);
    (c, _) = WithDeadline(Background(), time.Now().Add(shortDuration));
    o = new otherContext(c);
    (c, _) = WithDeadline(o, time.Now().Add(veryLongDuration));
    testDeadline(c, withDeadlineOtherContextˢ2, Ꮡt);
    (c, _) = WithDeadline(Background(), time.Now().Add(-shortDuration));
    testDeadline(c, withDeadlineInthepastˢ, Ꮡt);
    (c, _) = WithDeadline(Background(), time.Now());
    testDeadline(c, withDeadlineNowˢ, Ꮡt);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string withTimeoutˢ = "WithTimeout"u8;
internal static readonly @string withTimeoutOtherContextˢ = "WithTimeout+otherContext"u8;
internal static readonly @string withTimeoutOtherContextˢ2 = "WithTimeout+otherContext+WithTimeout"u8;

public static void TestTimeout(ж<testing.T> Ꮡt) {
    Ꮡt.Parallel();
    var (c, _) = WithTimeout(Background(), shortDuration);
    {
        @string got = fmt.Sprint(c);
        @string prefix = contextBackgroundˢ3; if (!strings.HasPrefix(got, prefix)) {
            Ꮡt.Errorf("c.String() = %q want prefix %q"u8, got, prefix);
        }
    }
    testDeadline(c, withTimeoutˢ, Ꮡt);
    (c, _) = WithTimeout(Background(), shortDuration);
    var o = new otherContext(c);
    testDeadline(o, withTimeoutOtherContextˢ, Ꮡt);
    (c, _) = WithTimeout(Background(), shortDuration);
    o = new otherContext(c);
    (c, _) = WithTimeout(o, veryLongDuration);
    testDeadline(c, withTimeoutOtherContextˢ2, Ꮡt);
}

public static void TestCanceledTimeout(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (c, _) = WithTimeout(Background(), time.ΔSecond);
    var o = new otherContext(c);
    (c, var cancel) = WithTimeout(o, veryLongDuration);
    cancel(); // Should propagate synchronously.
    var selᴛ15 = c.Done();
    switch (trySelect(ᐸꟷ(selᴛ15, ꓸꓸꓸ))) {
    case 0 when selᴛ15.ꟷᐳ(out _): {
        break;
    }
    default: {
        Ꮡt.Errorf("<-c.Done() blocked, but shouldn't have"u8);
        break;
    }}
    {
        var e = c.Err(); if (!AreEqual(e, Canceled)) {
            Ꮡt.Errorf("c.Err() == %v want %v"u8, e, Canceled);
        }
    }
}

[GoType("num:nint")] partial struct key1;

[GoType("num:nint")] partial struct key2;

internal static @string String(this key2 k) {
    return fmt.Sprintf("%[1]T(%[1]d)"u8, k);
}

internal static key1 k1 = ((key1)1);

internal static key2 k2 = ((key2)1); // same int as k1, different type

internal static key2 k3 = ((key2)3); // same type as k2, different int

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string c1k1ˢ = "c1k1"u8;
internal static readonly @string contextBackgroundˢ4 = @"context.Background.WithValue(context_test.key1, c1k1)"u8;
internal static readonly @string c2k2ˢ = "c2k2"u8;
internal static readonly @string contextBackgroundˢ5 = @"context.Background.WithValue(context_test.key1, c1k1).WithValue(context_test.key2(1), c2k2)"u8;
internal static readonly @string c3k3ˢ = "c3k3"u8;
internal static readonly @string contextBackgroundˢ6 = @"context.Background.WithValue(context_test.key1, c1k1).WithValue(context_test.key2(1), c2k2).WithValue(context_test.key2(3), c3k3).WithValue(context_test.key1, <nil>)"u8;
internal static readonly @string o2k2ˢ = "o2k2"u8;

public static void TestValues(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    void check(context.Context c, @string nm, @string v1, @string v2, @string v3) {
        {
            var (v, ok) = c.Value(k1)._<@string>(ᐧ); if (ok == (len(v1) == 0) || v != v1) {
                Ꮡt.Errorf(@"%s.Value(k1).(string) = %q, %t want %q, %t"u8, nm, v, ok, v1, len(v1) != 0);
            }
        }
        {
            var (v, ok) = c.Value(k2)._<@string>(ᐧ); if (ok == (len(v2) == 0) || v != v2) {
                Ꮡt.Errorf(@"%s.Value(k2).(string) = %q, %t want %q, %t"u8, nm, v, ok, v2, len(v2) != 0);
            }
        }
        {
            var (v, ok) = c.Value(k3)._<@string>(ᐧ); if (ok == (len(v3) == 0) || v != v3) {
                Ꮡt.Errorf(@"%s.Value(k3).(string) = %q, %t want %q, %t"u8, nm, v, ok, v3, len(v3) != 0);
            }
        }
    }
    var c0 = Background();
    check(c0, "c0"u8, ""u8, ""u8, ""u8);
    var c1 = WithValue(Background(), k1, c1k1ˢ);
    check(c1, "c1"u8, c1k1ˢ, ""u8, ""u8);
    {
        @string got = fmt.Sprint(c1);
        @string want = contextBackgroundˢ4; if (got != want) {
            Ꮡt.Errorf("c.String() = %q want %q"u8, got, want);
        }
    }
    var c2 = WithValue(c1, k2, c2k2ˢ);
    check(c2, "c2"u8, c1k1ˢ, c2k2ˢ, ""u8);
    {
        @string got = fmt.Sprint(c2);
        @string want = contextBackgroundˢ5; if (got != want) {
            Ꮡt.Errorf("c.String() = %q want %q"u8, got, want);
        }
    }
    var c3 = WithValue(c2, k3, c3k3ˢ);
    check(c3, "c2"u8, c1k1ˢ, c2k2ˢ, c3k3ˢ);
    var c4 = WithValue(c3, k1, default!);
    check(c4, "c4"u8, ""u8, c2k2ˢ, c3k3ˢ);
    {
        @string got = fmt.Sprint(c4);
        @string want = contextBackgroundˢ6; if (got != want) {
            Ꮡt.Errorf("c.String() = %q want %q"u8, got, want);
        }
    }
    var o0 = new otherContext(Background());
    check(o0, "o0"u8, ""u8, ""u8, ""u8);
    var o1 = new otherContext(WithValue(Background(), k1, c1k1ˢ));
    check(o1, "o1"u8, c1k1ˢ, ""u8, ""u8);
    var o2 = WithValue(o1, k2, o2k2ˢ);
    check(o2, "o2"u8, c1k1ˢ, o2k2ˢ, ""u8);
    var o3 = new otherContext(c4);
    check(o3, "o3"u8, ""u8, c2k2ˢ, c3k3ˢ);
    var o4 = WithValue(o3, k3, default!);
    check(o4, "o4"u8, ""u8, c2k2ˢ, ""u8);
}

[GoType("dyn")] internal partial struct TestAllocs_type {
    internal @string desc;
    internal Action f;
    internal float64 limit;
    internal float64 gccgoLimit;
}

public static void TestAllocs(ж<testing.T> Ꮡt) {
    var bg = Background();
            var bgʗ1 = bg;

            var bgʗ2 = bg;

            var bgʗ3 = bg;

            var bgʗ4 = bg;
    foreach (var (_, test) in new TestAllocs_type[]{
        new(
            desc: "Background()"u8,
            f: () => {
                Background();
            },
            limit: 0D,
            gccgoLimit: 0D
        ),
        new(
            desc: fmt.Sprintf("WithValue(bg, %v, nil)"u8, k1),
            f: () => {
                var c = WithValue(bgʗ1, k1, default!);
                c.Value(k1);
            },
            limit: 3D,
            gccgoLimit: 3D
        ),
        new(
            desc: "WithTimeout(bg, 1*time.Nanosecond)"u8,
            f: () => {
                var (c, _) = WithTimeout(bgʗ2, 1 * time.ΔNanosecond);
                ᐸꟷ(c.Done());
            },
            limit: 12D,
            gccgoLimit: 15D
        ),
        new(
            desc: "WithCancel(bg)"u8,
            f: () => {
                var (c, cancel) = WithCancel(bgʗ3);
                cancel();
                ᐸꟷ(c.Done());
            },
            limit: 5D,
            gccgoLimit: 8D
        ),
        new(
            desc: "WithTimeout(bg, 5*time.Millisecond)"u8,
            f: () => {
                var (c, cancel) = WithTimeout(bgʗ4, 5 * time.Millisecond);
                cancel();
                ᐸꟷ(c.Done());
            },
            limit: 8D,
            gccgoLimit: 25D
        )
    }.slice()) {
        var limit = test.limit;
        if (Δruntime.Compiler == "gccgo") {
            // gccgo does not yet do escape analysis.
            // TODO(iant): Remove this when gccgo does do escape analysis.
            limit = test.gccgoLimit;
        }
        nint numRuns = 100;
        if (testing.Short()) {
            numRuns = 10;
        }
        {
            var n = testing.AllocsPerRun(numRuns, test.f); if (n > limit) {
                Ꮡt.Errorf("%s allocs = %f want %d"u8, test.desc, n, (nint)limit);
            }
        }
    }
}

public static void TestSimultaneousCancels(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (root, cancel) = WithCancel(Background());
        var m = new map<context.Context, Action>{[root] = cancel};
        var q = new context.Context[]{root}.slice();
        // Create a tree of contexts.
        while (len(q) != 0 && len(m) < 100) {
            var parent = q[0];
            q = q[1..];
            for (nint i = 0; i < 4; i++) {
                var (ctx, cancelΔ1) = WithCancel(parent);
                m[ctx] = cancelΔ1;
                q = append(q, ctx);
            }
        }
        // Start all the cancels in a random order.
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(len(m));
        foreach (var (_, cancelΔ2) in m) {
            goǃ((Action cancelΔ3) => {
                cancelΔ3();
                Ꮡwg.Done();
            }, cancelΔ2);
        }
        var d = quiescent(Ꮡt);
        var stuck = new channel<EmptyStruct>(0);
        var stuckʗ1 = stuck;
        var timer = time.AfterFunc(d, () => {
            close(stuckʗ1);
        });
        var timerʗ1 = timer;
        defer(() => timerʗ1.Stop(), ref ᒐ);
        // Wait on all the contexts in a random order.
        foreach (var (ctx, _) in m) {
            var selᴛ16 = ctx.Done();
            var selᴛ17 = stuck;
            switch (select(ᐸꟷ(selᴛ16, ꓸꓸꓸ), ᐸꟷ(selᴛ17, ꓸꓸꓸ))) {
            case 0 when selᴛ16.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ17.ꟷᐳ(out _): {
                var buf = new slice<byte>((10 << (int)(10)));
                nint n = Δruntime.Stack(buf, true);
                Ꮡt.Fatalf("timed out after %v waiting for <-ctx.Done(); stacks:\n%s"u8, d, buf[..(int)(n)]);
                break;
            }}
        }
        // Wait for all the cancel functions to return.
        var done = new channel<EmptyStruct>(0);
        var doneʗ1 = done;
        goǃ(() => {
            Ꮡwg.Wait();
            close(doneʗ1);
        });
        var selᴛ18 = done;
        var selᴛ19 = stuck;
        switch (select(ᐸꟷ(selᴛ18, ꓸꓸꓸ), ᐸꟷ(selᴛ19, ꓸꓸꓸ))) {
        case 0 when selᴛ18.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ19.ꟷᐳ(out _): {
            var buf = new slice<byte>((10 << (int)(10)));
            nint n = Δruntime.Stack(buf, true);
            Ꮡt.Fatalf("timed out after %v waiting for cancel functions; stacks:\n%s"u8, d, buf[..(int)(n)]);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestInterlockedCancels(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (parent, cancelParent) = WithCancel(Background());
        var (child, cancelChild) = WithCancel(parent);
        var cancelChildʗ1 = cancelChild;
        var parentʗ1 = parent;
        goǃ(() => {
            ᐸꟷ(parentʗ1.Done());
            cancelChildʗ1();
        });
        cancelParent();
        var d = quiescent(Ꮡt);
        var timer = time.NewTimer(d);
        var timerʗ1 = timer;
        defer(() => timerʗ1.Stop(), ref ᒐ);
        var selᴛ20 = child.Done();
        var selᴛ21 = (~timer).C;
        switch (select(ᐸꟷ(selᴛ20, ꓸꓸꓸ), ᐸꟷ(selᴛ21, ꓸꓸꓸ))) {
        case 0 when selᴛ20.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ21.ꟷᐳ(out _): {
            var buf = new slice<byte>((10 << (int)(10)));
            nint n = Δruntime.Stack(buf, true);
            Ꮡt.Fatalf("timed out after %v waiting for child.Done(); stacks:\n%s"u8, d, buf[..(int)(n)]);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestLayersCancel(ж<testing.T> Ꮡt) {
    testLayers(Ꮡt, time.Now().UnixNano(), false);
}

public static void TestLayersTimeout(ж<testing.T> Ꮡt) {
    testLayers(Ꮡt, time.Now().UnixNano(), true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string contextBackgroundˢ7 = "context.Background."u8;
internal static readonly @string beforeCancelˢ = "before cancel"u8;
internal static readonly @string afterTimeoutˢ = "after timeout"u8;
internal static readonly @string afterCancelˢ = "after cancel"u8;

[GoLocalName("value")] [GoType("num:nint")] internal partial struct testLayers_value;

internal static void testLayers(ж<testing.T> Ꮡt, int64 seed, bool testTimeout) {
    GoFrame ᒐ = default;
    try {
        Ꮡt.Parallel();
        var r = rand.New(rand.NewSource(seed));
        @string prefix = fmt.Sprintf("seed=%d"u8, seed);
        void errorf(@string format, params ꓸꓸꓸany aʗp) {
            var a = aʗp.sslice();
            Ꮡt.Errorf(prefix + format, a.ꓸꓸꓸ);
        }
        const nint minLayers = 30;
        slice<ж<testLayers_value>> vals = default!;
        slice<Action> cancels = default!;
        nint numTimers = default!;
        context.Context ctx = Background();
        for (nint i = 0; i < minLayers || numTimers == 0 || len(cancels) == 0 || len(vals) == 0; i++) {
            switch (r.Intn(3)) {
            case 0: {
                var v = @new<testLayers_value>();
                ctx = WithValue(ctx, v.OrTypedNil(), v.OrTypedNil());
                vals = append(vals, v);
                break;
            }
            case 1: {
                Action cancel = default!;
                (ctx, cancel) = WithCancel(ctx);
                cancels = append(cancels, cancel);
                break;
            }
            case 2: {
                Action cancel = default!;
                var d = veryLongDuration;
                if (testTimeout) {
                    d = shortDuration;
                }
                (ctx, cancel) = WithTimeout(ctx, d);
                cancels = append(cancels, cancel);
                numTimers++;
                break;
            }}

        }
        var ctxʗ1 = ctx;
        var errorfʗ1 = errorf;
        var valsʗ1 = vals;
        void checkValues(@string when) {
            foreach (var (_, key) in valsʗ1) {
                {
                    var val = ctxʗ1.Value(key.OrTypedNil())._<ж<testLayers_value>>(); if (key != val) {
                        errorfʗ1("%s: ctx.Value(%p) = %p want %p"u8, when, key.OrTypedNil(), val.OrTypedNil(), key.OrTypedNil());
                    }
                }
            }
        }
        if (!testTimeout) {
            var selᴛ22 = ctx.Done();
            switch (trySelect(ᐸꟷ(selᴛ22, ꓸꓸꓸ))) {
            case 0 when selᴛ22.ꟷᐳ(out _): {
                errorf("ctx should not be canceled yet"u8);
                break;
            }
            default: {
                break;
            }}
        }
        {
            @string s = fmt.Sprint(ctx);
            @string prefixΔ1 = contextBackgroundˢ7; if (!strings.HasPrefix(s, prefixΔ1)) {
                Ꮡt.Errorf("ctx.String() = %q want prefix %q"u8, s, prefixΔ1);
            }
        }
        Ꮡt.Log(ctx);
        checkValues(beforeCancelˢ);
        if (testTimeout){
            var d = quiescent(Ꮡt);
            var timer = time.NewTimer(d);
            var timerʗ1 = timer;
            defer(() => timerʗ1.Stop(), ref ᒐ);
            var selᴛ23 = ctx.Done();
            var selᴛ24 = (~timer).C;
            switch (select(ᐸꟷ(selᴛ23, ꓸꓸꓸ), ᐸꟷ(selᴛ24, ꓸꓸꓸ))) {
            case 0 when selᴛ23.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ24.ꟷᐳ(out _): {
                errorf("ctx should have timed out after %v"u8, d);
                break;
            }}
            checkValues(afterTimeoutˢ);
        } else {
            var cancel = cancels[r.Intn(len(cancels))];
            cancel();
            var selᴛ25 = ctx.Done();
            switch (trySelect(ᐸꟷ(selᴛ25, ꓸꓸꓸ))) {
            case 0 when selᴛ25.ꟷᐳ(out _): {
                break;
            }
            default: {
                errorf("ctx should be canceled"u8);
                break;
            }}
            checkValues(afterCancelˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestWithCancelCanceledParent(ж<testing.T> Ꮡt) {
    var (parent, pcancel) = WithCancelCause(Background());
    var cause = fmt.Errorf("Because!"u8);
    pcancel(cause);
    var (c, _) = WithCancel(parent);
    var selᴛ26 = c.Done();
    switch (trySelect(ᐸꟷ(selᴛ26, ꓸꓸꓸ))) {
    case 0 when selᴛ26.ꟷᐳ(out _): {
        break;
    }
    default: {
        Ꮡt.Errorf("child not done immediately upon construction"u8);
        break;
    }}
    {
        var (got, want) = (c.Err(), Canceled); if (!AreEqual(got, want)) {
            Ꮡt.Errorf("child not canceled; got = %v, want = %v"u8, got, want);
        }
    }
    {
        var (got, want) = (Cause(c), cause); if (!AreEqual(got, want)) {
            Ꮡt.Errorf("child has wrong cause; got = %v, want = %v"u8, got, want);
        }
    }
}

public static void TestWithCancelSimultaneouslyCanceledParent(ж<testing.T> Ꮡt) {
    // Cancel the parent goroutine concurrently with creating a child.
    for (nint i = 0; i < 100; i++) {
        var (parent, pcancel) = WithCancelCause(Background());
        var cause = fmt.Errorf("Because!"u8);
        var pcancelʗ1 = pcancel;
        goǃ(pcancelʗ1, cause);
        var (c, _) = WithCancel(parent);
        ᐸꟷ(c.Done());
        {
            var (got, want) = (c.Err(), Canceled); if (!AreEqual(got, want)) {
                Ꮡt.Errorf("child not canceled; got = %v, want = %v"u8, got, want);
            }
        }
        {
            var (got, want) = (Cause(c), cause); if (!AreEqual(got, want)) {
                Ꮡt.Errorf("child has wrong cause; got = %v, want = %v"u8, got, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object barˢ = (@string)"bar"u8;
internal static readonly object expectedPanicˢ = (@string)"expected panic"u8;
internal static readonly @string nilKeyˢ = "nil key"u8;

public static void TestWithValueChecksKey(ж<testing.T> Ꮡt) {
    var panicVal = recoveredValue(() => {
        _ = WithValue(Background(), slice<byte>("foo"u8), barˢ);
    });
    if (panicVal == default!) {
        Ꮡt.Error(expectedPanicˢ);
    }
    panicVal = recoveredValue(() => {
        _ = WithValue(Background(), default!, barˢ);
    });
    {
        @string got = fmt.Sprint(panicVal);
        @string want = nilKeyˢ; if (got != want) {
            Ꮡt.Errorf("panic = %q; want %q"u8, got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fooˢ = (@string)"foo"u8;

public static void TestInvalidDerivedFail(ж<testing.T> Ꮡt) {
    var panicVal = recoveredValue(() => {
        (_, _) = WithCancel(default!);
    });
    if (panicVal == default!) {
        Ꮡt.Error(expectedPanicˢ);
    }
    panicVal = recoveredValue(() => {
        (_, _) = WithDeadline(default!, time.Now().Add(shortDuration));
    });
    if (panicVal == default!) {
        Ꮡt.Error(expectedPanicˢ);
    }
    panicVal = recoveredValue(() => {
        _ = WithValue(default!, fooˢ, barˢ);
    });
    if (panicVal == default!) {
        Ꮡt.Error(expectedPanicˢ);
    }
}

internal static any /*v*/ recoveredValue(Action fn) {
    any v = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            v = recover();
        }, ref ᒐ);
        fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return v;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deadlineExceededDoesNotˢ2 = (@string)"DeadlineExceeded does not support Timeout interface"u8;
internal static readonly object wrongValueForTimeoutˢ = (@string)"wrong value for timeout"u8;

[GoType("dyn")] internal partial interface TestDeadlineExceededSupportsTimeout_type {
    bool Timeout();
}

public static void TestDeadlineExceededSupportsTimeout(ж<testing.T> Ꮡt) {
    var (i, ok) = DeadlineExceeded._<TestDeadlineExceededSupportsTimeout_type>(ᐧ);
    if (!ok) {
        Ꮡt.Fatal(deadlineExceededDoesNotˢ2);
    }
    if (!i.Timeout()) {
        Ꮡt.Fatal(wrongValueForTimeoutˢ);
    }
}

[GoType("dyn")] internal partial struct TestCause_type {
    internal @string name;
    internal Func<context.Context> ctx;
    internal error err;
    internal error cause;
}

public static void TestCause(ж<testing.T> Ꮡt) {
    time.Duration forever = (time.Duration)(1000000000000000L);
    error parentCause = fmt.Errorf("parentCause"u8);
    error childCause = fmt.Errorf("childCause"u8);
    error tooSlow = fmt.Errorf("tooSlow"u8);
    error finishedEarly = fmt.Errorf("finishedEarly"u8);
            var parentCauseʗ1 = parentCause;


            var childCauseʗ1 = childCause;
            var parentCauseʗ2 = parentCause;

            var childCauseʗ2 = childCause;
            var parentCauseʗ3 = parentCause;

            var parentCauseʗ4 = parentCause;

            var parentCauseʗ5 = parentCause;

            var childCauseʗ3 = childCause;

            var childCauseʗ4 = childCause;



            var tooSlowʗ1 = tooSlow;

            var tooSlowʗ2 = tooSlow;

            var finishedEarlyʗ1 = finishedEarly;
            var tooSlowʗ3 = tooSlow;

            var finishedEarlyʗ2 = finishedEarly;
            var tooSlowʗ4 = tooSlow;


            var finishedEarlyʗ3 = finishedEarly;

            var tooSlowʗ5 = tooSlow;
    foreach (var (_, test) in new TestCause_type[]{
        new(
            name: "Background"u8,
            ctx: Background,
            err: default!,
            cause: default!
        ),
        new(
            name: "TODO"u8,
            ctx: TODO,
            err: default!,
            cause: default!
        ),
        new(
            name: "WithCancel"u8,
            ctx: () => {
                var (ctx, cancel) = WithCancel(Background());
                cancel();
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "WithCancelCause"u8,
            ctx: () => {
                var (ctx, cancel) = WithCancelCause(Background());
                cancel(parentCauseʗ1);
                return ctx;
            },
            err: Canceled,
            cause: parentCause
        ),
        new(
            name: "WithCancelCause nil"u8,
            ctx: () => {
                var (ctx, cancel) = WithCancelCause(Background());
                cancel(default!);
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "WithCancelCause: parent cause before child"u8,
            ctx: () => {
                var (ctx, cancelParent) = WithCancelCause(Background());
                (ctx, var cancelChild) = WithCancelCause(ctx);
                cancelParent(parentCauseʗ2);
                cancelChild(childCauseʗ1);
                return ctx;
            },
            err: Canceled,
            cause: parentCause
        ),
        new(
            name: "WithCancelCause: parent cause after child"u8,
            ctx: () => {
                var (ctx, cancelParent) = WithCancelCause(Background());
                (ctx, var cancelChild) = WithCancelCause(ctx);
                cancelChild(childCauseʗ2);
                cancelParent(parentCauseʗ3);
                return ctx;
            },
            err: Canceled,
            cause: childCause
        ),
        new(
            name: "WithCancelCause: parent cause before nil"u8,
            ctx: () => {
                var (ctx, cancelParent) = WithCancelCause(Background());
                (ctx, var cancelChild) = WithCancel(ctx);
                cancelParent(parentCauseʗ4);
                cancelChild();
                return ctx;
            },
            err: Canceled,
            cause: parentCause
        ),
        new(
            name: "WithCancelCause: parent cause after nil"u8,
            ctx: () => {
                var (ctx, cancelParent) = WithCancelCause(Background());
                (ctx, var cancelChild) = WithCancel(ctx);
                cancelChild();
                cancelParent(parentCauseʗ5);
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "WithCancelCause: child cause after nil"u8,
            ctx: () => {
                var (ctx, cancelParent) = WithCancel(Background());
                (ctx, var cancelChild) = WithCancelCause(ctx);
                cancelParent();
                cancelChild(childCauseʗ3);
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "WithCancelCause: child cause before nil"u8,
            ctx: () => {
                var (ctx, cancelParent) = WithCancel(Background());
                (ctx, var cancelChild) = WithCancelCause(ctx);
                cancelChild(childCauseʗ4);
                cancelParent();
                return ctx;
            },
            err: Canceled,
            cause: childCause
        ),
        new(
            name: "WithTimeout"u8,
            ctx: () => {
                var (ctx, cancel) = WithTimeout(Background(), 0);
                cancel();
                return ctx;
            },
            err: DeadlineExceeded,
            cause: DeadlineExceeded
        ),
        new(
            name: "WithTimeout canceled"u8,
            ctx: () => {
                var (ctx, cancel) = WithTimeout(Background(), forever);
                cancel();
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "WithTimeoutCause"u8,
            ctx: () => {
                var (ctx, cancel) = WithTimeoutCause(Background(), 0, tooSlowʗ1);
                cancel();
                return ctx;
            },
            err: DeadlineExceeded,
            cause: tooSlow
        ),
        new(
            name: "WithTimeoutCause canceled"u8,
            ctx: () => {
                var (ctx, cancel) = WithTimeoutCause(Background(), forever, tooSlowʗ2);
                cancel();
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "WithTimeoutCause stacked"u8,
            ctx: () => {
                var (ctx, cancel) = WithCancelCause(Background());
                (ctx, _) = WithTimeoutCause(ctx, 0, tooSlowʗ3);
                cancel(finishedEarlyʗ1);
                return ctx;
            },
            err: DeadlineExceeded,
            cause: tooSlow
        ),
        new(
            name: "WithTimeoutCause stacked canceled"u8,
            ctx: () => {
                var (ctx, cancel) = WithCancelCause(Background());
                (ctx, _) = WithTimeoutCause(ctx, forever, tooSlowʗ4);
                cancel(finishedEarlyʗ2);
                return ctx;
            },
            err: Canceled,
            cause: finishedEarly
        ),
        new(
            name: "WithoutCancel"u8,
            ctx: () => WithoutCancel(Background()),
            err: default!,
            cause: default!
        ),
        new(
            name: "WithoutCancel canceled"u8,
            ctx: () => {
                var (ctx, cancel) = WithCancelCause(Background());
                ctx = WithoutCancel(ctx);
                cancel(finishedEarlyʗ3);
                return ctx;
            },
            err: default!,
            cause: default!
        ),
        new(
            name: "WithoutCancel timeout"u8,
            ctx: () => {
                var (ctx, cancel) = WithTimeoutCause(Background(), 0, tooSlowʗ5);
                ctx = WithoutCancel(ctx);
                cancel();
                return ctx;
            },
            err: default!,
            cause: default!
        )
    }.slice()) {
        ref var testΔ1 = ref heap<TestCause_type>(out var ᏑtestΔ1);
        testΔ1 = test;
        var testʗ1 = testΔ1;
        Ꮡt.Run(testΔ1.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var ctx = testʗ1.ctx();
            {
                var (got, want) = (ctx.Err(), testʗ1.err); if (!AreEqual(want, got)) {
                    tΔ1.Errorf("ctx.Err() = %v want %v"u8, got, want);
                }
            }
            {
                var (got, want) = (Cause(ctx), testʗ1.cause); if (!AreEqual(want, got)) {
                    tΔ1.Errorf("Cause(ctx) = %v want %v"u8, got, want);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testCauseRaceˢ = "TestCauseRace"u8;

public static void TestCauseRace(ж<testing.T> Ꮡt) {
    var cause = errors.New(testCauseRaceˢ);
    var (ctx, cancel) = WithCancelCause(Background());
    var cancelʗ1 = cancel;
    var causeʗ1 = cause;
    goǃ(() => {
        cancelʗ1(causeʗ1);
    });
    while (ᐧ) {
        // Poll Cause, rather than waiting for Done, to test that
        // access to the underlying cause is synchronized properly.
        {
            var err = Cause(ctx); if (err != default!) {
                if (!AreEqual(err, cause)) {
                    Ꮡt.Errorf("Cause returned %v, want %v"u8, err, cause);
                }
                break;
            }
        }
        Δruntime.Gosched();
    }
}

public static void TestWithoutCancel(ж<testing.T> Ꮡt) {
    @string key = keyˢ;
    @string value = valueˢ;
    var ctx = WithValue(Background(), key, value);
    ctx = WithoutCancel(ctx);
    {
        var (d, ok) = ctx.Deadline(); if (!d.IsZero() || ok != false) {
            Ꮡt.Errorf("ctx.Deadline() = %v, %v want zero, false"u8, d, ok);
        }
    }
    {
        var done = ctx.Done(); if (done != default!) {
            Ꮡt.Errorf("ctx.Deadline() = %v want nil"u8, done);
        }
    }
    {
        var err = ctx.Err(); if (err != default!) {
            Ꮡt.Errorf("ctx.Err() = %v want nil"u8, err);
        }
    }
    {
        var v = ctx.Value(key); if (!AreEqual(v, value)) {
            Ꮡt.Errorf("ctx.Value(%q) = %q want %q"u8, key, v, value);
        }
    }
}

[GoType] partial struct customDoneContext {
    public context_package.Context Context;
    internal channel<EmptyStruct> donec;
}

// Go method set entry for the promoted 'Context.Deadline()' - provided ONLY by the embedded
// interface field in *customDoneContext's method set; see the pointer-only satisfaction record.
internal static (time.Time, bool) Deadline(this customDoneContext recvᴛ) => recvᴛ.Context.Deadline();

// Go method set entry for the promoted 'Context.Err()' - provided ONLY by the embedded
// interface field in *customDoneContext's method set; see the pointer-only satisfaction record.
internal static error Err(this customDoneContext recvᴛ) => recvᴛ.Context.Err();

// Go method set entry for the promoted 'Context.Value()' - provided ONLY by the embedded
// interface field in *customDoneContext's method set; see the pointer-only satisfaction record.
internal static any Value(this customDoneContext recvᴛ, any key) => recvᴛ.Context.Value(key);

[GoRecv] internal static /*<-*/channel<EmptyStruct> Done(this ref customDoneContext c) {
    return c.donec.WithDirection(GoChanDir.Recv);
}

public static void TestCustomContextPropagation(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var cause = errors.New("TestCustomContextPropagation"u8);
        var donec = new channel<EmptyStruct>(0);
        var (ctx1, cancel1) = WithCancelCause(Background());
        var ctx2 = Ꮡ(new customDoneContext(
            Context: ctx1,
            donec: donec
        ));
        var (ctx3, cancel3) = WithCancel(new context_test_package.customDoneContextжContext(ctx2));
        var cancel3ʗ1 = cancel3;
        defer(() => cancel3ʗ1(), ref ᒐ);
        cancel1(cause);
        close(donec);
        ᐸꟷ(ctx3.Done());
        {
            var (got, want) = (ctx3.Err(), Canceled); if (!AreEqual(got, want)) {
                Ꮡt.Errorf("child not canceled; got = %v, want = %v"u8, got, want);
            }
        }
        {
            var (got, want) = (Cause(ctx3), cause); if (!AreEqual(got, want)) {
                Ꮡt.Errorf("child has wrong cause; got = %v, want = %v"u8, got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// customCauseContext is a custom Context used to test context.Cause.
[GoType] partial struct customCauseContext {
    internal Δsync.Mutex mu;
    internal channel<EmptyStruct> done;
    internal error err;
    internal Action cancelChild;
}

[GoRecv] internal static (time.Time deadline, bool ok) Deadline(this ref customCauseContext ccc) {
    time.Time deadline = default!;
    bool ok = default!;

    return (deadline, ok);
}

internal static /*<-*/channel<EmptyStruct> Done(this ж<customCauseContext> Ꮡccc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var ccc = ref Ꮡccc.DerefOrNull();

        ccc.mu.Lock();
        ᒐd1 = true;
        return ccc.done.WithDirection(GoChanDir.Recv);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡccc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static error Err(this ж<customCauseContext> Ꮡccc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var ccc = ref Ꮡccc.DerefOrNull();

        ccc.mu.Lock();
        ᒐd1 = true;
        return ccc.err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡccc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

[GoRecv] internal static any Value(this ref customCauseContext ccc, any key) {
    return default!;
}

internal static void cancel(this ж<customCauseContext> Ꮡccc) {
    ref var ccc = ref Ꮡccc.DerefOrNull();

    ccc.mu.Lock();
    ccc.err = Canceled;
    close(ccc.done);
    var cancelChild = ccc.cancelChild;
    ccc.mu.Unlock();
    if (cancelChild != default!) {
        cancelChild();
    }
}

[GoRecv] internal static void setCancelChild(this ref customCauseContext ccc, Action cancelChild) {
    ccc.cancelChild = cancelChild;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string testCustomContextCauseˢ = "TestCustomContextCause"u8;

public static void TestCustomContextCause(ж<testing.T> Ꮡt) {
    // Test if we cancel a custom context, Err and Cause return Canceled.
    var ccc = Ꮡ(new customCauseContext(
        done: new channel<EmptyStruct>(0)
    ));
    ccc.cancel();
    {
        var got = ccc.Err(); if (!AreEqual(got, Canceled)) {
            Ꮡt.Errorf("ccc.Err() = %v, want %v"u8, got, Canceled);
        }
    }
    {
        var got = Cause(new context_test_package.customCauseContextжContext(ccc)); if (!AreEqual(got, Canceled)) {
            Ꮡt.Errorf("Cause(ccc) = %v, want %v"u8, got, Canceled);
        }
    }
    // Test that if we pass a custom context to WithCancelCause,
    // and then cancel that child context with a cause,
    // that the cause of the child canceled context is correct
    // but that the parent custom context is not canceled.
    ccc = Ꮡ(new customCauseContext(
        done: new channel<EmptyStruct>(0)
    ));
    var (ctx, causeFunc) = WithCancelCause(new context_test_package.customCauseContextжContext(ccc));
    var cause = errors.New(testCustomContextCauseˢ);
    causeFunc(cause);
    {
        var got = ctx.Err(); if (!AreEqual(got, Canceled)) {
            Ꮡt.Errorf("after CancelCauseFunc ctx.Err() = %v, want %v"u8, got, Canceled);
        }
    }
    {
        var got = Cause(ctx); if (!AreEqual(got, cause)) {
            Ꮡt.Errorf("after CancelCauseFunc Cause(ctx) = %v, want %v"u8, got, cause);
        }
    }
    {
        var got = ccc.Err(); if (got != default!) {
            Ꮡt.Errorf("after CancelCauseFunc ccc.Err() = %v, want %v"u8, got, (any)(default!));
        }
    }
    {
        var got = Cause(new context_test_package.customCauseContextжContext(ccc)); if (got != default!) {
            Ꮡt.Errorf("after CancelCauseFunc Cause(ccc) = %v, want %v"u8, got, (any)(default!));
        }
    }
    // Test that if we now cancel the parent custom context,
    // the cause of the child canceled context is still correct,
    // and the parent custom context is canceled without a cause.
    ccc.cancel();
    {
        var got = ctx.Err(); if (!AreEqual(got, Canceled)) {
            Ꮡt.Errorf("after CancelCauseFunc ctx.Err() = %v, want %v"u8, got, Canceled);
        }
    }
    {
        var got = Cause(ctx); if (!AreEqual(got, cause)) {
            Ꮡt.Errorf("after CancelCauseFunc Cause(ctx) = %v, want %v"u8, got, cause);
        }
    }
    {
        var got = ccc.Err(); if (!AreEqual(got, Canceled)) {
            Ꮡt.Errorf("after CancelCauseFunc ccc.Err() = %v, want %v"u8, got, Canceled);
        }
    }
    {
        var got = Cause(new context_test_package.customCauseContextжContext(ccc)); if (!AreEqual(got, Canceled)) {
            Ꮡt.Errorf("after CancelCauseFunc Cause(ccc) = %v, want %v"u8, got, Canceled);
        }
    }
    // Test that if we associate a custom context with a child,
    // then canceling the custom context cancels the child.
    ccc = Ꮡ(new customCauseContext(
        done: new channel<EmptyStruct>(0)
    ));
    (ctx, var cancelFunc) = WithCancel(new context_test_package.customCauseContextжContext(ccc));
    ccc.setCancelChild(cancelFunc);
    ccc.cancel();
    {
        var got = ctx.Err(); if (!AreEqual(got, Canceled)) {
            Ꮡt.Errorf("after CancelCauseFunc ctx.Err() = %v, want %v"u8, got, Canceled);
        }
    }
    {
        var got = Cause(ctx); if (!AreEqual(got, Canceled)) {
            Ꮡt.Errorf("after CancelCauseFunc Cause(ctx) = %v, want %v"u8, got, Canceled);
        }
    }
    {
        var got = ccc.Err(); if (!AreEqual(got, Canceled)) {
            Ꮡt.Errorf("after CancelCauseFunc ccc.Err() = %v, want %v"u8, got, Canceled);
        }
    }
    {
        var got = Cause(new context_test_package.customCauseContextжContext(ccc)); if (!AreEqual(got, Canceled)) {
            Ꮡt.Errorf("after CancelCauseFunc Cause(ccc) = %v, want %v"u8, got, Canceled);
        }
    }
}

public static void TestAfterFuncCalledAfterCancel(ж<testing.T> Ꮡt) {
    var (ctx, cancel) = WithCancel(Background());
    var donec = new channel<EmptyStruct>(0);
    var donecʗ1 = donec;
    var stop = AfterFunc(ctx, () => {
        close(donecʗ1);
    });
    var selᴛ27 = donec;
    var selᴛ28 = time.After(shortDuration);
    switch (select(ᐸꟷ(selᴛ27, ꓸꓸꓸ), ᐸꟷ(selᴛ28, ꓸꓸꓸ))) {
    case 0 when selᴛ27.ꟷᐳ(out _): {
        Ꮡt.Fatalf("AfterFunc called before context is done"u8);
        break;
    }
    case 1 when selᴛ28.ꟷᐳ(out _): {
        break;
    }}
    cancel();
    var selᴛ29 = donec;
    var selᴛ30 = time.After(veryLongDuration);
    switch (select(ᐸꟷ(selᴛ29, ꓸꓸꓸ), ᐸꟷ(selᴛ30, ꓸꓸꓸ))) {
    case 0 when selᴛ29.ꟷᐳ(out _): {
        break;
    }
    case 1 when selᴛ30.ꟷᐳ(out _): {
        Ꮡt.Fatalf("AfterFunc not called after context is canceled"u8);
        break;
    }}
    if (stop()) {
        Ꮡt.Fatalf("stop() = true, want false"u8);
    }
}

public static void TestAfterFuncCalledAfterTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (ctx, cancel) = WithTimeout(Background(), shortDuration);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var donec = new channel<EmptyStruct>(0);
        var donecʗ1 = donec;
        AfterFunc(ctx, () => {
            close(donecʗ1);
        });
        var selᴛ31 = donec;
        var selᴛ32 = time.After(veryLongDuration);
        switch (select(ᐸꟷ(selᴛ31, ꓸꓸꓸ), ᐸꟷ(selᴛ32, ꓸꓸꓸ))) {
        case 0 when selᴛ31.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ32.ꟷᐳ(out _): {
            Ꮡt.Fatalf("AfterFunc not called after context is canceled"u8);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestAfterFuncCalledImmediately(ж<testing.T> Ꮡt) {
    var (ctx, cancel) = WithCancel(Background());
    cancel();
    var donec = new channel<EmptyStruct>(0);
    var donecʗ1 = donec;
    AfterFunc(ctx, () => {
        close(donecʗ1);
    });
    var selᴛ33 = donec;
    var selᴛ34 = time.After(veryLongDuration);
    switch (select(ᐸꟷ(selᴛ33, ꓸꓸꓸ), ᐸꟷ(selᴛ34, ꓸꓸꓸ))) {
    case 0 when selᴛ33.ꟷᐳ(out _): {
        break;
    }
    case 1 when selᴛ34.ꟷᐳ(out _): {
        Ꮡt.Fatalf("AfterFunc not called for already-canceled context"u8);
        break;
    }}
}

public static void TestAfterFuncNotCalledAfterStop(ж<testing.T> Ꮡt) {
    var (ctx, cancel) = WithCancel(Background());
    var donec = new channel<EmptyStruct>(0);
    var donecʗ1 = donec;
    var stop = AfterFunc(ctx, () => {
        close(donecʗ1);
    });
    if (!stop()) {
        Ꮡt.Fatalf("stop() = false, want true"u8);
    }
    cancel();
    var selᴛ35 = donec;
    var selᴛ36 = time.After(shortDuration);
    switch (select(ᐸꟷ(selᴛ35, ꓸꓸꓸ), ᐸꟷ(selᴛ36, ꓸꓸꓸ))) {
    case 0 when selᴛ35.ꟷᐳ(out _): {
        Ꮡt.Fatalf("AfterFunc called for already-canceled context"u8);
        break;
    }
    case 1 when selᴛ36.ꟷᐳ(out _): {
        break;
    }}
    if (stop()) {
        Ꮡt.Fatalf("stop() = true, want false"u8);
    }
}

// This test verifies that canceling a context does not block waiting for AfterFuncs to finish.
public static void TestAfterFuncCalledAsynchronously(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (ctx, cancel) = WithCancel(Background());
        var donec = new channel<EmptyStruct>(0);
        var donecʗ1 = donec;
        var stop = AfterFunc(ctx, () => {
            // The channel send blocks until donec is read from.
            donecʗ1.ᐸꟷ(new EmptyStruct());
        });
        var stopʗ1 = stop;
        defer(() => stopʗ1(), ref ᒐ);
        cancel();
        // After cancel returns, read from donec and unblock the AfterFunc.
        var selᴛ37 = donec;
        var selᴛ38 = time.After(veryLongDuration);
        switch (select(ᐸꟷ(selᴛ37, ꓸꓸꓸ), ᐸꟷ(selᴛ38, ꓸꓸꓸ))) {
        case 0 when selᴛ37.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ38.ꟷᐳ(out _): {
            Ꮡt.Fatalf("AfterFunc not called after context is canceled"u8);
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end context_test_package
