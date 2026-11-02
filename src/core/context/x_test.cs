// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static context_package;
using errors = errors_package;
using fmt = fmt_package;
using asan = @internal.asan_package;
using rand = math.rand_package;
using Δruntime = runtime_package;
using strings = strings_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using context = context_package;
using math;
using static go.context_internal_test_package;
using ꓸꓸꓸany = Span<any>;

partial class context_test_package {

// Each XTestFoo in context_test.go must be called from a TestFoo here to run.
public static void TestParentFinishesChild(ж<testing.T> t) {
    context_internal_test_package.XTestParentFinishesChild(new context_test_package.testing_TжtestingT(t)); // uses unexported context types
}

public static void TestChildFinishesFirst(ж<testing.T> t) {
    context_internal_test_package.XTestChildFinishesFirst(new context_test_package.testing_TжtestingT(t)); // uses unexported context types
}

public static void TestCancelRemoves(ж<testing.T> t) {
    context_internal_test_package.XTestCancelRemoves(new context_test_package.testing_TжtestingT(t)); // uses unexported context types
}

public static void TestCustomContextGoroutines(ж<testing.T> t) {
    context_internal_test_package.XTestCustomContextGoroutines(new context_test_package.testing_TжtestingT(t)); // reads the context.goroutines counter
}

// The following are regular tests in package context_test.

// otherContext is a Context that's not one of the types defined in context.go.
// This lets us test code paths that differ based on the underlying type of the
// Context.
partial struct otherContext {
    /*embed*/ public context_package.Context Context;
}

internal static time.Duration shortDuration => /* 1 * time.Millisecond */ 1000000;         // a reasonable duration to block in a test
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
internal static ref readonly @string contextBackgroundˢ => ref contextBackgroundˢᵇ;
private static readonly @string contextBackgroundˢᵇ = "context.Background"u8;

public static void TestBackground(ж<testing.T> t) {
    var c = Background();
    if (c == nil) {
        t.Fatalf("Background returned nil");
    }
    var selᴛ9 = c.Done();
    switch (trySelect(ᐸꟷ(selᴛ9, ꓸꓸꓸ))) {
    case 0 when selᴛ9.ꟷᐳ(out var x): {
        t.Errorf("<-c.Done() == %v want nothing (it should block)", x);
        break;
    }
    default: {
        break;
    }}
    {
        @string got = fmt.Sprint(c);
        @string want = contextBackgroundˢ; if (got != want) {
            t.Errorf("Background().String() = %q want %q", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contextTodoˢ => ref contextTodoˢᵇ;
private static readonly @string contextTodoˢᵇ = "context.TODO"u8;

public static void TestTODO(ж<testing.T> t) {
    var c = TODO();
    if (c == nil) {
        t.Fatalf("TODO returned nil");
    }
    var selᴛ10 = c.Done();
    switch (trySelect(ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
    case 0 when selᴛ10.ꟷᐳ(out var x): {
        t.Errorf("<-c.Done() == %v want nothing (it should block)", x);
        break;
    }
    default: {
        break;
    }}
    {
        @string got = fmt.Sprint(c);
        @string want = contextTodoˢ; if (got != want) {
            t.Errorf("TODO().String() = %q want %q", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contextBackgroundˢ2 => ref contextBackgroundˢ2ᵇ;
private static readonly @string contextBackgroundˢ2ᵇ = "context.Background.WithCancel"u8;

public static void TestWithCancel(ж<testing.T> t) {
    var (c1, cancel) = WithCancel(Background());

    {
        @string got = fmt.Sprint(c1);
        @string want = contextBackgroundˢ2; if (got != want) {
            t.Errorf("c1.String() = %q want %q", got, want);
        }
    }

    var o = new otherContext(c1);
    var (c2, _) = WithCancel(o);
    var contexts = slice<context.Context>([c1, o, c2]);

    foreach (var (i, c) in contexts) {
        {
            var d = c.Done(); if (d == nil) {
                t.Errorf("c[%d].Done() == %v want non-nil", i, d);
            }
        }
        {
            var e = c.Err(); if (e != nil) {
                t.Errorf("c[%d].Err() == %v want nil", i, e);
            }
        }

        var selᴛ11 = c.Done();
        switch (trySelect(ᐸꟷ(selᴛ11, ꓸꓸꓸ))) {
        case 0 when selᴛ11.ꟷᐳ(out var x): {
            t.Errorf("<-c.Done() == %v want nothing (it should block)", x);
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
            t.Errorf("<-c[%d].Done() blocked, but shouldn't have", i);
            break;
        }}
        {
            var e = c.Err(); if (!AreEqual(e, Canceled)) {
                t.Errorf("c[%d].Err() == %v want %v", i, e, Canceled);
            }
        }
    }
}

internal static void testDeadline(context.Context c, @string name, ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        t.Helper();
        var d = quiescent(t);
        var timer = time.NewTimer(d);
        var timerʗ1 = timer;
        defer(() => timerʗ1.Stop(), ref ᒐ);
        var selᴛ13 = timer.C;
        var selᴛ14 = c.Done();
        switch (select(ᐸꟷ(selᴛ13, ꓸꓸꓸ), ᐸꟷ(selᴛ14, ꓸꓸꓸ))) {
        case 0 when selᴛ13.ꟷᐳ(out _): {
            t.Fatalf("%s: context not timed out after %v", name, d);
            break;
        }
        case 1 when selᴛ14.ꟷᐳ(out _): {
            break;
        }}
        {
            var e = c.Err(); if (!AreEqual(e, DeadlineExceeded)) {
                t.Errorf("%s: c.Err() == %v; want %v", name, e, DeadlineExceeded);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contextBackgroundˢ3 => ref contextBackgroundˢ3ᵇ;
private static readonly @string contextBackgroundˢ3ᵇ = "context.Background.WithDeadline("u8;
internal static ref readonly @string withDeadlineˢ => ref withDeadlineˢᵇ;
private static readonly @string withDeadlineˢᵇ = "WithDeadline"u8;
internal static ref readonly @string withDeadlineOtherContextˢ => ref withDeadlineOtherContextˢᵇ;
private static readonly @string withDeadlineOtherContextˢᵇ = "WithDeadline+otherContext"u8;
internal static ref readonly @string withDeadlineOtherContextˢ2 => ref withDeadlineOtherContextˢ2ᵇ;
private static readonly @string withDeadlineOtherContextˢ2ᵇ = "WithDeadline+otherContext+WithDeadline"u8;
internal static ref readonly @string withDeadlineInthepastˢ => ref withDeadlineInthepastˢᵇ;
private static readonly @string withDeadlineInthepastˢᵇ = "WithDeadline+inthepast"u8;
internal static ref readonly @string withDeadlineNowˢ => ref withDeadlineNowˢᵇ;
private static readonly @string withDeadlineNowˢᵇ = "WithDeadline+now"u8;

public static void TestDeadline(ж<testing.T> t) {
    t.Parallel();

    var (c, _) = WithDeadline(Background(), time.Now().Add(shortDuration));
    {
        @string got = fmt.Sprint(c);
        @string prefix = contextBackgroundˢ3; if (!strings.HasPrefix(got, prefix)) {
            t.Errorf("c.String() = %q want prefix %q", got, prefix);
        }
    }
    testDeadline(c, withDeadlineˢ, t);

    (c, _) = WithDeadline(Background(), time.Now().Add(shortDuration));
    var o = new otherContext(c);
    testDeadline(o, withDeadlineOtherContextˢ, t);

    (c, _) = WithDeadline(Background(), time.Now().Add(shortDuration));
    o = new otherContext(c);
    (c, _) = WithDeadline(o, time.Now().Add(veryLongDuration));
    testDeadline(c, withDeadlineOtherContextˢ2, t);

    (c, _) = WithDeadline(Background(), time.Now().Add(-shortDuration));
    testDeadline(c, withDeadlineInthepastˢ, t);

    (c, _) = WithDeadline(Background(), time.Now());
    testDeadline(c, withDeadlineNowˢ, t);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string withTimeoutˢ => ref withTimeoutˢᵇ;
private static readonly @string withTimeoutˢᵇ = "WithTimeout"u8;
internal static ref readonly @string withTimeoutOtherContextˢ => ref withTimeoutOtherContextˢᵇ;
private static readonly @string withTimeoutOtherContextˢᵇ = "WithTimeout+otherContext"u8;
internal static ref readonly @string withTimeoutOtherContextˢ2 => ref withTimeoutOtherContextˢ2ᵇ;
private static readonly @string withTimeoutOtherContextˢ2ᵇ = "WithTimeout+otherContext+WithTimeout"u8;

public static void TestTimeout(ж<testing.T> t) {
    t.Parallel();

    var (c, _) = WithTimeout(Background(), shortDuration);
    {
        @string got = fmt.Sprint(c);
        @string prefix = contextBackgroundˢ3; if (!strings.HasPrefix(got, prefix)) {
            t.Errorf("c.String() = %q want prefix %q", got, prefix);
        }
    }
    testDeadline(c, withTimeoutˢ, t);

    (c, _) = WithTimeout(Background(), shortDuration);
    var o = new otherContext(c);
    testDeadline(o, withTimeoutOtherContextˢ, t);

    (c, _) = WithTimeout(Background(), shortDuration);
    o = new otherContext(c);
    (c, _) = WithTimeout(o, veryLongDuration);
    testDeadline(c, withTimeoutOtherContextˢ2, t);
}

public static void TestCanceledTimeout(ж<testing.T> t) {
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
        t.Errorf("<-c.Done() blocked, but shouldn't have");
        break;
    }}
    {
        var e = c.Err(); if (!AreEqual(e, Canceled)) {
            t.Errorf("c.Err() == %v want %v", e, Canceled);
        }
    }
}

partial struct key1 /*num:nint*/;

partial struct key2 /*num:nint*/;

internal static @string String(this key2 k) {
    return fmt.Sprintf("%[1]T(%[1]d)"u8, k);
}

internal static key1 k1 = ((key1)1);

internal static key2 k2 = ((key2)1); // same int as k1, different type

internal static key2 k3 = ((key2)3); // same type as k2, different int

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string c1k1ˢ => ref c1k1ˢᵇ;
private static readonly @string c1k1ˢᵇ = "c1k1"u8;
internal static ref readonly @string contextBackgroundˢ4 => ref contextBackgroundˢ4ᵇ;
private static readonly @string contextBackgroundˢ4ᵇ = @"context.Background.WithValue(context_test.key1, c1k1)"u8;
internal static ref readonly @string c2k2ˢ => ref c2k2ˢᵇ;
private static readonly @string c2k2ˢᵇ = "c2k2"u8;
internal static ref readonly @string contextBackgroundˢ5 => ref contextBackgroundˢ5ᵇ;
private static readonly @string contextBackgroundˢ5ᵇ = @"context.Background.WithValue(context_test.key1, c1k1).WithValue(context_test.key2(1), c2k2)"u8;
internal static ref readonly @string c3k3ˢ => ref c3k3ˢᵇ;
private static readonly @string c3k3ˢᵇ = "c3k3"u8;
internal static ref readonly @string contextBackgroundˢ6 => ref contextBackgroundˢ6ᵇ;
private static readonly @string contextBackgroundˢ6ᵇ = @"context.Background.WithValue(context_test.key1, c1k1).WithValue(context_test.key2(1), c2k2).WithValue(context_test.key2(3), c3k3).WithValue(context_test.key1, <nil>)"u8;
internal static ref readonly @string o2k2ˢ => ref o2k2ˢᵇ;
private static readonly @string o2k2ˢᵇ = "o2k2"u8;

public static void TestValues(ж<testing.T> t) {
    void check(context.Context c, @string nm, @string v1, @string v2, @string v3) {
        {
            var (v, ok) = c.Value(k1)._<@string>(ᐧ); if (ok == (len(v1) == 0) || v != v1) {
                t.Errorf(@"%s.Value(k1).(string) = %q, %t want %q, %t", nm, v, ok, v1, len(v1) != 0);
            }
        }
        {
            var (v, ok) = c.Value(k2)._<@string>(ᐧ); if (ok == (len(v2) == 0) || v != v2) {
                t.Errorf(@"%s.Value(k2).(string) = %q, %t want %q, %t", nm, v, ok, v2, len(v2) != 0);
            }
        }
        {
            var (v, ok) = c.Value(k3)._<@string>(ᐧ); if (ok == (len(v3) == 0) || v != v3) {
                t.Errorf(@"%s.Value(k3).(string) = %q, %t want %q, %t", nm, v, ok, v3, len(v3) != 0);
            }
        }
    }

    var c0 = Background();
    check(c0, "c0", ""u8, ""u8, ""u8);

    var c1 = WithValue(Background(), k1, c1k1ˢ);
    check(c1, "c1", c1k1ˢ, ""u8, ""u8);

    {
        @string got = fmt.Sprint(c1);
        @string want = contextBackgroundˢ4; if (got != want) {
            t.Errorf("c.String() = %q want %q", got, want);
        }
    }

    var c2 = WithValue(c1, k2, c2k2ˢ);
    check(c2, "c2", c1k1ˢ, c2k2ˢ, ""u8);

    {
        @string got = fmt.Sprint(c2);
        @string want = contextBackgroundˢ5; if (got != want) {
            t.Errorf("c.String() = %q want %q", got, want);
        }
    }

    var c3 = WithValue(c2, k3, c3k3ˢ);
    check(c3, "c2", c1k1ˢ, c2k2ˢ, c3k3ˢ);

    var c4 = WithValue(c3, k1, default);
    check(c4, "c4", ""u8, c2k2ˢ, c3k3ˢ);

    {
        @string got = fmt.Sprint(c4);
        @string want = contextBackgroundˢ6; if (got != want) {
            t.Errorf("c.String() = %q want %q", got, want);
        }
    }

    var o0 = new otherContext(Background());
    check(o0, "o0", ""u8, ""u8, ""u8);

    var o1 = new otherContext(WithValue(Background(), k1, c1k1ˢ));
    check(o1, "o1", c1k1ˢ, ""u8, ""u8);

    var o2 = WithValue(o1, k2, o2k2ˢ);
    check(o2, "o2", c1k1ˢ, o2k2ˢ, ""u8);

    var o3 = new otherContext(c4);
    check(o3, "o3", ""u8, c2k2ˢ, c3k3ˢ);

    var o4 = WithValue(o3, k3, default);
    check(o4, "o4", ""u8, c2k2ˢ, ""u8);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object testAllocatesMoreWithˢ = (@string)"test allocates more with -asan"u8;

internal partial struct TestAllocs_type /*dyn*/ {
    internal @string desc;
    internal Action f;
    internal float64 limit;
    internal float64 gccgoLimit;
}

public static void TestAllocs(ж<testing.T> t) {
    if (asan.Enabled) {
        t.Skip(testAllocatesMoreWithˢ);
    }
    var bg = Background();
            var bgʗ1 = bg;

            var bgʗ2 = bg;

            var bgʗ3 = bg;

            var bgʗ4 = bg;
    foreach (var (_, test) in slice<TestAllocs_type>([
        new(
            desc: "Background()",
            f: () => {
                Background();
            },
            limit: 0D,
            gccgoLimit: 0D
        ),
        new(
            desc: fmt.Sprintf("WithValue(bg, %v, nil)"u8, k1),
            f: () => {
                var c = WithValue(bgʗ1, k1, default);
                c.Value(k1);
            },
            limit: 3D,
            gccgoLimit: 3D
        ),
        new(
            desc: "WithTimeout(bg, 1*time.Nanosecond)",
            f: () => {
                var (c, _) = WithTimeout(bgʗ2, 1 * time.ΔNanosecond);
                ᐸꟷ(c.Done());
            },
            limit: 12D,
            gccgoLimit: 15D
        ),
        new(
            desc: "WithCancel(bg)",
            f: () => {
                var (c, cancel) = WithCancel(bgʗ3);
                cancel();
                ᐸꟷ(c.Done());
            },
            limit: 5D,
            gccgoLimit: 8D
        ),
        new(
            desc: "WithTimeout(bg, 5*time.Millisecond)",
            f: () => {
                var (c, cancel) = WithTimeout(bgʗ4, 5 * time.Millisecond);
                cancel();
                ᐸꟷ(c.Done());
            },
            limit: 8D,
            gccgoLimit: 25D
        )
    ])) {
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
                t.Errorf("%s allocs = %f want %d", test.desc, n, (nint)limit);
            }
        }
    }
}

public static partial void TestSimultaneousCancels(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var qᴷ = new stackAppend<context.Context>();

        var (root, cancel) = WithCancel(Background());
        var m = new map<context.Context, Action>{[root] = cancel};
        var q = slice<context.Context>([root]);
        // Create a tree of contexts.
        while (len(q) != 0 && len(m) < 100) {
            var parent = q[0];
            q = q[1..];
            for (nint i = 0; i < 4; i++) {
                var (ctx, cancelΔ1) = WithCancel(parent);
                m[ctx] = cancelΔ1;
                q = append(qᴷ, q, ctx);
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

        var d = quiescent(t);
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
                var buf = new slice<byte>((10 << 10));
                nint n = Δruntime.Stack(buf, true);
                t.Fatalf("timed out after %v waiting for <-ctx.Done(); stacks:\n%s", d, buf.slice(0, n));
                break;
            }}
        }
        // Wait for all the cancel functions to return.
        var done = new channel<EmptyStruct>(0);
        var doneʗ1 = done;
        goǃ(() => {
            Ꮡwg.Wait();
            close(doneʗ1);
        }, GoStack.Bounded);
        var selᴛ18 = done;
        var selᴛ19 = stuck;
        switch (select(ᐸꟷ(selᴛ18, ꓸꓸꓸ), ᐸꟷ(selᴛ19, ꓸꓸꓸ))) {
        case 0 when selᴛ18.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ19.ꟷᐳ(out _): {
            var buf = new slice<byte>((10 << 10));
            nint n = Δruntime.Stack(buf, true);
            t.Fatalf("timed out after %v waiting for cancel functions; stacks:\n%s", d, buf.slice(0, n));
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static partial void TestInterlockedCancels(ж<testing.T> t) {
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
        var d = quiescent(t);
        var timer = time.NewTimer(d);
        var timerʗ1 = timer;
        defer(() => timerʗ1.Stop(), ref ᒐ);
        var selᴛ20 = child.Done();
        var selᴛ21 = timer.C;
        switch (select(ᐸꟷ(selᴛ20, ꓸꓸꓸ), ᐸꟷ(selᴛ21, ꓸꓸꓸ))) {
        case 0 when selᴛ20.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ21.ꟷᐳ(out _): {
            var buf = new slice<byte>((10 << 10));
            nint n = Δruntime.Stack(buf, true);
            t.Fatalf("timed out after %v waiting for child.Done(); stacks:\n%s", d, buf.slice(0, n));
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestLayersCancel(ж<testing.T> t) {
    testLayers(t, time.Now().UnixNano(), false);
}

public static void TestLayersTimeout(ж<testing.T> t) {
    testLayers(t, time.Now().UnixNano(), true);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string contextBackgroundˢ7 => ref contextBackgroundˢ7ᵇ;
private static readonly @string contextBackgroundˢ7ᵇ = "context.Background."u8;
internal static ref readonly @string beforeCancelˢ => ref beforeCancelˢᵇ;
private static readonly @string beforeCancelˢᵇ = "before cancel"u8;
internal static ref readonly @string afterTimeoutˢ => ref afterTimeoutˢᵇ;
private static readonly @string afterTimeoutˢᵇ = "after timeout"u8;
internal static ref readonly @string afterCancelˢ => ref afterCancelˢᵇ;
private static readonly @string afterCancelˢᵇ = "after cancel"u8;

internal partial struct testLayers_value /*num:nint*/;

internal static void testLayers(ж<testing.T> t, int64 seed, bool testTimeout) {
    GoFrame ᒐ = default;
    try {
        var valsᴷ = new stackAppend<ж<testLayers_value>>();
        var cancelsᴷ = new stackAppend<Action>();

        t.Parallel();

        var r = rand.New(rand.NewSource(seed));
        @string prefix = fmt.Sprintf("seed=%d"u8, seed);
        void errorf(@string format, params ꓸꓸꓸany aʗp) {
            var a = aʗp.sslice();
            t.Errorf(prefix + format, a.ꓸꓸꓸ);
        }
        const nint minLayers = 30;
        slice<ж<testLayers_value>> vals = default;
        slice<Action> cancels = default;
        nint numTimers = default;
        context.Context ctx = Background();
        for (nint i = 0; i < minLayers || numTimers == 0 || len(cancels) == 0 || len(vals) == 0; i++) {
            switch (r.Intn(3)) {
            case 0: {
                var v = @new<testLayers_value>();
                ctx = WithValue(ctx, v.OrTypedNil(), v.OrTypedNil());
                vals = append(valsᴷ, vals, v);
                break;
            }
            case 1: {
                Action cancel = default;
                (ctx, cancel) = WithCancel(ctx);
                cancels = append(cancelsᴷ, cancels, cancel);
                break;
            }
            case 2: {
                Action cancel = default;
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
                        errorfʗ1("%s: ctx.Value(%p) = %p want %p", when, key.OrTypedNil(), val.OrTypedNil(), key.OrTypedNil());
                    }
                }
            }
        }
        if (!testTimeout) {
            var selᴛ22 = ctx.Done();
            switch (trySelect(ᐸꟷ(selᴛ22, ꓸꓸꓸ))) {
            case 0 when selᴛ22.ꟷᐳ(out _): {
                errorf("ctx should not be canceled yet");
                break;
            }
            default: {
                break;
            }}
        }
        {
            @string s = fmt.Sprint(ctx);
            @string prefixΔ1 = contextBackgroundˢ7; if (!strings.HasPrefix(s, prefixΔ1)) {
                t.Errorf("ctx.String() = %q want prefix %q", s, prefixΔ1);
            }
        }
        t.Log(ctx);
        checkValues(beforeCancelˢ);
        if (testTimeout) {
            var d = quiescent(t);
            var timer = time.NewTimer(d);
            var timerʗ1 = timer;
            defer(() => timerʗ1.Stop(), ref ᒐ);
            var selᴛ23 = ctx.Done();
            var selᴛ24 = timer.C;
            switch (select(ᐸꟷ(selᴛ23, ꓸꓸꓸ), ᐸꟷ(selᴛ24, ꓸꓸꓸ))) {
            case 0 when selᴛ23.ꟷᐳ(out _): {
                break;
            }
            case 1 when selᴛ24.ꟷᐳ(out _): {
                errorf("ctx should have timed out after %v", d);
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
                errorf("ctx should be canceled");
                break;
            }}
            checkValues(afterCancelˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestWithCancelCanceledParent(ж<testing.T> t) {
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
        t.Errorf("child not done immediately upon construction");
        break;
    }}
    {
        var (got, want) = (c.Err(), Canceled); if (!AreEqual(got, want)) {
            t.Errorf("child not canceled; got = %v, want = %v", got, want);
        }
    }
    {
        var (got, want) = (Cause(c), cause); if (!AreEqual(got, want)) {
            t.Errorf("child has wrong cause; got = %v, want = %v", got, want);
        }
    }
}

public static partial void TestWithCancelSimultaneouslyCanceledParent(ж<testing.T> t) {
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
                t.Errorf("child not canceled; got = %v, want = %v", got, want);
            }
        }
        {
            var (got, want) = (Cause(c), cause); if (!AreEqual(got, want)) {
                t.Errorf("child has wrong cause; got = %v, want = %v", got, want);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object barˢ = (@string)"bar"u8;
internal static readonly object expectedPanicˢ = (@string)"expected panic"u8;
internal static ref readonly @string nilKeyˢ => ref nilKeyˢᵇ;
private static readonly @string nilKeyˢᵇ = "nil key"u8;

public static void TestWithValueChecksKey(ж<testing.T> t) {
    var panicVal = recoveredValue(() => {
        _ = WithValue(Background(), slice<byte>("foo"u8), barˢ);
    });
    if (panicVal == nil) {
        t.Error(expectedPanicˢ);
    }
    panicVal = recoveredValue(() => {
        _ = WithValue(Background(), default, barˢ);
    });
    {
        @string got = fmt.Sprint(panicVal);
        @string want = nilKeyˢ; if (got != want) {
            t.Errorf("panic = %q; want %q", got, want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object fooˢ = (@string)"foo"u8;

public static void TestInvalidDerivedFail(ж<testing.T> t) {
    var panicVal = recoveredValue(() => {
        (_, _) = WithCancel(default);
    });
    if (panicVal == nil) {
        t.Error(expectedPanicˢ);
    }
    panicVal = recoveredValue(() => {
        (_, _) = WithDeadline(default, time.Now().Add(shortDuration));
    });
    if (panicVal == nil) {
        t.Error(expectedPanicˢ);
    }
    panicVal = recoveredValue(() => {
        _ = WithValue(default, fooˢ, barˢ);
    });
    if (panicVal == nil) {
        t.Error(expectedPanicˢ);
    }
}

internal static any /*v*/ recoveredValue(Action fn) {
    any v = default;
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

internal partial interface TestDeadlineExceededSupportsTimeout_type /*dyn*/ {
    bool Timeout();
}

public static void TestDeadlineExceededSupportsTimeout(ж<testing.T> t) {
    var (i, ok) = DeadlineExceeded._<TestDeadlineExceededSupportsTimeout_type>(ᐧ);
    if (!ok) {
        t.Fatal(deadlineExceededDoesNotˢ2);
    }
    if (!i.Timeout()) {
        t.Fatal(wrongValueForTimeoutˢ);
    }
}

internal partial struct TestCause_type /*dyn*/ {
    internal @string name;
    internal Func<context.Context> ctx;
    internal error err;
    internal error cause;
}

public static void TestCause(ж<testing.T> t) {
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


            var parentCauseʗ6 = parentCause;

            var parentCauseʗ7 = parentCause;
    foreach (var (_, vᴛ1) in slice<TestCause_type>([
        new(
            name: "Background",
            ctx: Background,
            err: default,
            cause: default
        ),
        new(
            name: "TODO",
            ctx: TODO,
            err: default,
            cause: default
        ),
        new(
            name: "WithCancel",
            ctx: () => {
                var (ctx, cancel) = WithCancel(Background());
                cancel();
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "WithCancelCause",
            ctx: () => {
                var (ctx, cancel) = WithCancelCause(Background());
                cancel(parentCauseʗ1);
                return ctx;
            },
            err: Canceled,
            cause: parentCause
        ),
        new(
            name: "WithCancelCause nil",
            ctx: () => {
                var (ctx, cancel) = WithCancelCause(Background());
                cancel(default);
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "WithCancelCause: parent cause before child",
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
            name: "WithCancelCause: parent cause after child",
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
            name: "WithCancelCause: parent cause before nil",
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
            name: "WithCancelCause: parent cause after nil",
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
            name: "WithCancelCause: child cause after nil",
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
            name: "WithCancelCause: child cause before nil",
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
            name: "WithTimeout",
            ctx: () => {
                var (ctx, cancel) = WithTimeout(Background(), 0);
                cancel();
                return ctx;
            },
            err: DeadlineExceeded,
            cause: DeadlineExceeded
        ),
        new(
            name: "WithTimeout canceled",
            ctx: () => {
                var (ctx, cancel) = WithTimeout(Background(), forever);
                cancel();
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "WithTimeoutCause",
            ctx: () => {
                var (ctx, cancel) = WithTimeoutCause(Background(), 0, tooSlowʗ1);
                cancel();
                return ctx;
            },
            err: DeadlineExceeded,
            cause: tooSlow
        ),
        new(
            name: "WithTimeoutCause canceled",
            ctx: () => {
                var (ctx, cancel) = WithTimeoutCause(Background(), forever, tooSlowʗ2);
                cancel();
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "WithTimeoutCause stacked",
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
            name: "WithTimeoutCause stacked canceled",
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
            name: "WithoutCancel",
            ctx: () => WithoutCancel(Background()),
            err: default,
            cause: default
        ),
        new(
            name: "WithoutCancel canceled",
            ctx: () => {
                var (ctx, cancel) = WithCancelCause(Background());
                ctx = WithoutCancel(ctx);
                cancel(finishedEarlyʗ3);
                return ctx;
            },
            err: default,
            cause: default
        ),
        new(
            name: "WithoutCancel timeout",
            ctx: () => {
                var (ctx, cancel) = WithTimeoutCause(Background(), 0, tooSlowʗ5);
                ctx = WithoutCancel(ctx);
                cancel();
                return ctx;
            },
            err: default,
            cause: default
        ),
        new(
            name: "parent of custom context not canceled",
            ctx: () => {
                var (ctx, _) = WithCancelCause(Background());
                (ctx, var cancel2) = newCustomContext(ctx);
                cancel2();
                return ctx;
            },
            err: Canceled,
            cause: Canceled
        ),
        new(
            name: "parent of custom context is canceled before",
            ctx: () => {
                var (ctx, cancel1) = WithCancelCause(Background());
                (ctx, var cancel2) = newCustomContext(ctx);
                cancel1(parentCauseʗ6);
                cancel2();
                return ctx;
            },
            err: Canceled,
            cause: parentCause
        ),
        new(
            name: "parent of custom context is canceled after",
            ctx: () => {
                var (ctx, cancel1) = WithCancelCause(Background());
                (ctx, var cancel2) = newCustomContext(ctx);
                cancel2();
                cancel1(parentCauseʗ7);
                return ctx;
            },
            err: Canceled,
            // This isn't really right: the child context was canceled before
            // the parent context, and shouldn't inherit the parent's cause.
            // However, since the child is a custom context, Cause has no way
            // to tell which was canceled first and returns the parent's cause.
            cause: parentCause
        )
    ])) {
        ref var test = ref heap(new TestCause_type(), out var Ꮡtest);
        test = vᴛ1;

        var testʗ1 = test;
        t.Run(test.name, (ж<testing.T> tΔ1) => {
            tΔ1.Parallel();
            var ctx = testʗ1.ctx();
            {
                var (got, want) = (ctx.Err(), testʗ1.err); if (!AreEqual(want, got)) {
                    tΔ1.Errorf("ctx.Err() = %v want %v", got, want);
                }
            }
            {
                var (got, want) = (Cause(ctx), testʗ1.cause); if (!AreEqual(want, got)) {
                    tΔ1.Errorf("Cause(ctx) = %v want %v", got, want);
                }
            }
        });
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testCauseRaceˢ => ref testCauseRaceˢᵇ;
private static readonly @string testCauseRaceˢᵇ = "TestCauseRace"u8;

public static partial void TestCauseRace(ж<testing.T> t) {
    var cause = errors.New(testCauseRaceˢ);
    var (ctx, cancel) = WithCancelCause(Background());
    var cancelʗ1 = cancel;
    var causeʗ1 = cause;
    goǃ(() => {
        cancelʗ1(causeʗ1);
    });
    for (;;) {
        // Poll Cause, rather than waiting for Done, to test that
        // access to the underlying cause is synchronized properly.
        {
            var err = Cause(ctx); if (err != nil) {
                if (!AreEqual(err, cause)) {
                    t.Errorf("Cause returned %v, want %v", err, cause);
                }
                break;
            }
        }
        Δruntime.Gosched();
    }
}

public static void TestWithoutCancel(ж<testing.T> t) {
    @string key = keyˢ;
    @string value = valueˢ;
    var ctx = WithValue(Background(), key, value);
    ctx = WithoutCancel(ctx);
    {
        var (d, ok) = ctx.Deadline(); if (!d.IsZero() || ok != false) {
            t.Errorf("ctx.Deadline() = %v, %v want zero, false", d, ok);
        }
    }
    {
        var done = ctx.Done(); if (done != nil) {
            t.Errorf("ctx.Deadline() = %v want nil", done);
        }
    }
    {
        var err = ctx.Err(); if (err != nil) {
            t.Errorf("ctx.Err() = %v want nil", err);
        }
    }
    {
        var v = ctx.Value(key); if (!AreEqual(v, value)) {
            t.Errorf("ctx.Value(%q) = %q want %q", key, v, value);
        }
    }
}

partial struct customDoneContext {
    /*embed*/ public context_package.Context Context;
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

internal static /*<-*/channel<EmptyStruct> Done(this ref customDoneContext c) {
    return c.donec.AsRecvOnly();
}

public static void TestCustomContextPropagation(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        var cause = errors.New("TestCustomContextPropagation");
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
                t.Errorf("child not canceled; got = %v, want = %v", got, want);
            }
        }
        {
            var (got, want) = (Cause(ctx3), cause); if (!AreEqual(got, want)) {
                t.Errorf("child has wrong cause; got = %v, want = %v", got, want);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// customCauseContext is a custom Context used to test context.Cause.
partial struct customCauseContext {
    internal Δsync.Mutex mu;
    internal channel<EmptyStruct> done;
    internal error err;
    internal Action cancelChild;
}

internal static (time.Time deadline, bool ok) Deadline(this ref customCauseContext ccc) {
    time.Time deadline = default;
    bool ok = default;

    return (deadline, ok);
}

internal static /*<-*/channel<EmptyStruct> Done(this ж<customCauseContext> Ꮡccc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var ccc = ref Ꮡccc.DerefOrNull();

        ccc.mu.Lock();
        ᒐd1 = true;
        return ccc.done.AsRecvOnly();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
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
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default; }
    finally { if (ᒐd1) Ꮡccc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static any Value(this ref customCauseContext ccc, any key) {
    return default;
}

internal static void cancel(this ж<customCauseContext> Ꮡccc) {
    ref var ccc = ref Ꮡccc.DerefOrNull();

    ccc.mu.Lock();
    ccc.err = Canceled;
    close(ccc.done);
    var cancelChild = ccc.cancelChild;
    ccc.mu.Unlock();

    if (cancelChild != nil) {
        cancelChild();
    }
}

internal static void setCancelChild(this ref customCauseContext ccc, Action cancelChild) {
    ccc.cancelChild = cancelChild;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string testCustomContextCauseˢ => ref testCustomContextCauseˢᵇ;
private static readonly @string testCustomContextCauseˢᵇ = "TestCustomContextCause"u8;

public static void TestCustomContextCause(ж<testing.T> t) {
    // Test if we cancel a custom context, Err and Cause return Canceled.
    var ccc = Ꮡ(new customCauseContext(
        done: new channel<EmptyStruct>(0)
    ));
    ccc.cancel();
    {
        var got = ccc.Err(); if (!AreEqual(got, Canceled)) {
            t.Errorf("ccc.Err() = %v, want %v", got, Canceled);
        }
    }
    {
        var got = Cause(new context_test_package.customCauseContextжContext(ccc)); if (!AreEqual(got, Canceled)) {
            t.Errorf("Cause(ccc) = %v, want %v", got, Canceled);
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
            t.Errorf("after CancelCauseFunc ctx.Err() = %v, want %v", got, Canceled);
        }
    }
    {
        var got = Cause(ctx); if (!AreEqual(got, cause)) {
            t.Errorf("after CancelCauseFunc Cause(ctx) = %v, want %v", got, cause);
        }
    }
    {
        var got = ccc.Err(); if (got != nil) {
            t.Errorf("after CancelCauseFunc ccc.Err() = %v, want %v", got, (any)(default));
        }
    }
    {
        var got = Cause(new context_test_package.customCauseContextжContext(ccc)); if (got != nil) {
            t.Errorf("after CancelCauseFunc Cause(ccc) = %v, want %v", got, (any)(default));
        }
    }

    // Test that if we now cancel the parent custom context,
    // the cause of the child canceled context is still correct,
    // and the parent custom context is canceled without a cause.
    ccc.cancel();
    {
        var got = ctx.Err(); if (!AreEqual(got, Canceled)) {
            t.Errorf("after CancelCauseFunc ctx.Err() = %v, want %v", got, Canceled);
        }
    }
    {
        var got = Cause(ctx); if (!AreEqual(got, cause)) {
            t.Errorf("after CancelCauseFunc Cause(ctx) = %v, want %v", got, cause);
        }
    }
    {
        var got = ccc.Err(); if (!AreEqual(got, Canceled)) {
            t.Errorf("after CancelCauseFunc ccc.Err() = %v, want %v", got, Canceled);
        }
    }
    {
        var got = Cause(new context_test_package.customCauseContextжContext(ccc)); if (!AreEqual(got, Canceled)) {
            t.Errorf("after CancelCauseFunc Cause(ccc) = %v, want %v", got, Canceled);
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
            t.Errorf("after CancelCauseFunc ctx.Err() = %v, want %v", got, Canceled);
        }
    }
    {
        var got = Cause(ctx); if (!AreEqual(got, Canceled)) {
            t.Errorf("after CancelCauseFunc Cause(ctx) = %v, want %v", got, Canceled);
        }
    }
    {
        var got = ccc.Err(); if (!AreEqual(got, Canceled)) {
            t.Errorf("after CancelCauseFunc ccc.Err() = %v, want %v", got, Canceled);
        }
    }
    {
        var got = Cause(new context_test_package.customCauseContextжContext(ccc)); if (!AreEqual(got, Canceled)) {
            t.Errorf("after CancelCauseFunc Cause(ccc) = %v, want %v", got, Canceled);
        }
    }
}

public static void TestAfterFuncCalledAfterCancel(ж<testing.T> t) {
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
        t.Fatalf("AfterFunc called before context is done");
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
        t.Fatalf("AfterFunc not called after context is canceled");
        break;
    }}
    if (stop()) {
        t.Fatalf("stop() = true, want false");
    }
}

public static void TestAfterFuncCalledAfterTimeout(ж<testing.T> t) {
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
            t.Fatalf("AfterFunc not called after context is canceled");
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestAfterFuncCalledImmediately(ж<testing.T> t) {
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
        t.Fatalf("AfterFunc not called for already-canceled context");
        break;
    }}
}

public static void TestAfterFuncNotCalledAfterStop(ж<testing.T> t) {
    var (ctx, cancel) = WithCancel(Background());
    var donec = new channel<EmptyStruct>(0);
    var donecʗ1 = donec;
    var stop = AfterFunc(ctx, () => {
        close(donecʗ1);
    });
    if (!stop()) {
        t.Fatalf("stop() = false, want true");
    }
    cancel();
    var selᴛ35 = donec;
    var selᴛ36 = time.After(shortDuration);
    switch (select(ᐸꟷ(selᴛ35, ꓸꓸꓸ), ᐸꟷ(selᴛ36, ꓸꓸꓸ))) {
    case 0 when selᴛ35.ꟷᐳ(out _): {
        t.Fatalf("AfterFunc called for already-canceled context");
        break;
    }
    case 1 when selᴛ36.ꟷᐳ(out _): {
        break;
    }}
    if (stop()) {
        t.Fatalf("stop() = true, want false");
    }
}

// This test verifies that canceling a context does not block waiting for AfterFuncs to finish.
public static void TestAfterFuncCalledAsynchronously(ж<testing.T> t) {
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
            t.Fatalf("AfterFunc not called after context is canceled");
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// customContext is a custom Context implementation.
partial struct customContext {
    internal context.Context parent;
    internal Δsync.Once doneOnce;
    internal channel<EmptyStruct> donec;
    internal error err;
}

internal static (context.Context, Action) newCustomContext(context.Context parent) {
    var c = Ꮡ(new customContext(
        parent: parent,
        donec: new channel<EmptyStruct>(0)
    ));
    var cʗ1 = c;
    AfterFunc(parent, () => {
        var cʗ2 = cʗ1;
        cʗ1.ᏑdoneOnce.Do(() => {
            cʗ2.err = parent.Err();
            close(cʗ2.donec);
        });
    });
    var cʗ3 = c;
    return (new context_test_package.customContextжContext(c), () => {
        var cʗ4 = cʗ3;
        cʗ3.ᏑdoneOnce.Do(() => {
            cʗ4.err = Canceled;
            close(cʗ4.donec);
        });
    });
}

internal static (time.Time, bool) Deadline(this ref customContext c) {
    return c.parent.Deadline();
}

internal static /*<-*/channel<EmptyStruct> Done(this ref customContext c) {
    return c.donec.AsRecvOnly();
}

internal static error Err(this ref customContext c) {
    var selᴛ39 = c.donec;
    switch (trySelect(ᐸꟷ(selᴛ39, ꓸꓸꓸ))) {
    case 0 when selᴛ39.ꟷᐳ(out _): {
        return c.err;
    }
    default: {
        return default;
    }}
}

internal static any Value(this ref customContext c, any key) {
    return c.parent.Value(key);
}

// Issue #75533.
public static partial void TestContextErrDoneRace(ж<testing.T> t) {
    // 4 iterations reliably reproduced #75533.
    foreach (var _ᴛ1 in range(10)) {
        var (ctx, cancel) = WithCancel(Background());
        var donec = ctx.Done();
        var cancelʗ1 = cancel;
        goǃ(() => cancelʗ1());
        while (ctx.Err() == nil) {
            if (Δruntime.GOARCH == "wasm"u8) {
                Δruntime.Gosched(); // need to explicitly yield
            }
        }
        var selᴛ40 = donec;
        switch (trySelect(ᐸꟷ(selᴛ40, ꓸꓸꓸ))) {
        case 0 when selᴛ40.ꟷᐳ(out _): {
            break;
        }
        default: {
            t.Fatalf("ctx.Err is non-nil, but ctx.Done is not closed");
            break;
        }}
    }
}

} // end context_test_package
