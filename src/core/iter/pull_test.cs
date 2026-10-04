// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using static iter_package;
using Δruntime = runtime_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using iter = iter_package;

partial class iter_test_package {

internal static iter.Seq<nint> count(nint n) {
    return (Func<nint, bool> yield) => {
        foreach (var i in range(n)) {
            if (!yield(i)) {
                break;
            }
        }
    };
}

internal static iter.Seq2<nint, int64> squares(nint n) {
    return (Func<nint, int64, bool> yield) => {
        foreach (var i in range(n)) {
            if (!yield(i, (int64)i * (int64)i)) {
                break;
            }
        }
    };
}

public static void TestPull(ж<testing.T> Ꮡt) {
    for (nint endᴛ1 = 0; endᴛ1 <= 3; endᴛ1++) {
        var end = endᴛ1;
        Ꮡt.Run(fmt.Sprint(end), (ж<testing.T> tΔ1) => {
            nint ng = stableNumGoroutine();
            void wantNG(nint want) {
                {
                    nint xg = Δruntime.NumGoroutine() - ng; if (xg != want) {
                        tΔ1.Helper();
                        tΔ1.Errorf("have %d extra goroutines, want %d"u8, xg, want);
                    }
                }
            }
            wantNG(0);
            var (next, stop) = Pull(count(3));
            wantNG(1);
            foreach (var i in range(end)) {
                var (v, ok) = next();
                if (v != i || ok != true) {
                    tΔ1.Fatalf("next() = %d, %v, want %d, %v"u8, v, ok, i, true);
                }
                wantNG(1);
            }
            wantNG(1);
            if (end < 3) {
                stop();
                wantNG(0);
            }
            foreach (var _ᴛ1 in range(2)) {
                var (v, ok) = next();
                if (v != 0 || ok != false) {
                    tΔ1.Fatalf("next() = %d, %v, want %d, %v"u8, v, ok, (nint)(0), false);
                }
                wantNG(0);
            }
            wantNG(0);
            stop();
            stop();
            stop();
            wantNG(0);
        });
    }
}

public static void TestPull2(ж<testing.T> Ꮡt) {
    for (nint endᴛ1 = 0; endᴛ1 <= 3; endᴛ1++) {
        var end = endᴛ1;
        Ꮡt.Run(fmt.Sprint(end), (ж<testing.T> tΔ1) => {
            nint ng = stableNumGoroutine();
            void wantNG(nint want) {
                {
                    nint xg = Δruntime.NumGoroutine() - ng; if (xg != want) {
                        tΔ1.Helper();
                        tΔ1.Errorf("have %d extra goroutines, want %d"u8, xg, want);
                    }
                }
            }
            wantNG(0);
            var (next, stop) = Pull2(squares(3));
            wantNG(1);
            foreach (var i in range(end)) {
                var (k, v, ok) = next();
                if (k != i || v != (int64)(i * i) || ok != true) {
                    tΔ1.Fatalf("next() = %d, %d, %v, want %d, %d, %v"u8, k, v, ok, i, i * i, true);
                }
                wantNG(1);
            }
            wantNG(1);
            if (end < 3) {
                stop();
                wantNG(0);
            }
            foreach (var _ᴛ1 in range(2)) {
                var (k, v, ok) = next();
                if (v != 0 || ok != false) {
                    tΔ1.Fatalf("next() = %d, %d, %v, want %d, %d, %v"u8, k, v, ok, (nint)(0), (nint)(0), false);
                }
                wantNG(0);
            }
            wantNG(0);
            stop();
            stop();
            stop();
            wantNG(0);
        });
    }
}

// stableNumGoroutine is like NumGoroutine but tries to ensure stability of
// the value by letting any exiting goroutines finish exiting.
internal static nint stableNumGoroutine() {
    GoFrame ᒐ = default;
    try {
        // The idea behind stablizing the value of NumGoroutine is to
        // see the same value enough times in a row in between calls to
        // runtime.Gosched. With GOMAXPROCS=1, we're trying to make sure
        // that other goroutines run, so that they reach a stable point.
        // It's not guaranteed, because it is still possible for a goroutine
        // to Gosched back into itself, so we require NumGoroutine to be
        // the same 100 times in a row. This should be more than enough to
        // ensure all goroutines get a chance to run to completion (or to
        // some block point) for a small group of test goroutines.
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(1), ref ᒐ);
        nint c = 0;
        nint ng = Δruntime.NumGoroutine();
        for (nint i = 0; i < 1000; i++) {
            nint nng = Δruntime.NumGoroutine();
            if (nng == ng){
                c++;
            } else {
                c = 0;
                ng = nng;
            }
            if (c >= 100) {
                // The same value 100 times in a row is good enough.
                return ng;
            }
            Δruntime.Gosched();
        }
        throw panic("failed to stabilize NumGoroutine after 1000 iterations");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object doubleNextDidNotFailˢ = (@string)"double next did not fail"u8;

public static void TestPullDoubleNext(ж<testing.T> Ꮡt) {
    var (next, _) = Pull(doDoubleNext());
    nextSlot = next;
    next();
    if (nextSlot != default!) {
        Ꮡt.Fatal(doubleNextDidNotFailˢ);
    }
}

internal static Func<(nint, bool)> nextSlot;

internal static iter.Seq<nint> doDoubleNext() {
    return (Func<nint, bool> _) => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                if (recover() != default!) {
                    nextSlot = default!;
                }
            }, ref ᒐ);
            nextSlot();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

public static void TestPullDoubleNext2(ж<testing.T> Ꮡt) {
    var (next, _) = Pull2(doDoubleNext2());
    nextSlot2 = next;
    next();
    if (nextSlot2 != default!) {
        Ꮡt.Fatal(doubleNextDidNotFailˢ);
    }
}

internal static Func<(nint, nint, bool)> nextSlot2;

internal static iter.Seq2<nint, nint> doDoubleNext2() {
    return (Func<nint, nint, bool> _) => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                if (recover() != default!) {
                    nextSlot2 = default!;
                }
            }, ref ᒐ);
            nextSlot2();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object yieldFailedˢ = (@string)"yield failed"u8;
private static readonly object doubleYieldDidNotFailˢ = (@string)"double yield did not fail"u8;

public static void TestPullDoubleYield(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (next, stop) = Pull(storeYield());
        next();
        if (yieldSlot == default!) {
            Ꮡt.Fatal(yieldFailedˢ);
        }
        var stopʗ1 = stop;
        defer(() => {
            if (recover() != default!) {
                yieldSlot = default!;
            }
            stopʗ1();
        }, ref ᒐ);
        yieldSlot(5);
        if (yieldSlot != default!) {
            Ꮡt.Fatal(doubleYieldDidNotFailˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static iter.Seq<nint> storeYield() {
    return (Func<nint, bool> yield) => {
        yieldSlot = yield;
        if (!yield(5)) {
            return;
        }
    };
}

internal static Func<nint, bool> yieldSlot;

public static void TestPullDoubleYield2(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var (next, stop) = Pull2(storeYield2());
        next();
        if (yieldSlot2 == default!) {
            Ꮡt.Fatal(yieldFailedˢ);
        }
        var stopʗ1 = stop;
        defer(() => {
            if (recover() != default!) {
                yieldSlot2 = default!;
            }
            stopʗ1();
        }, ref ᒐ);
        yieldSlot2(23, 77);
        if (yieldSlot2 != default!) {
            Ꮡt.Fatal(doubleYieldDidNotFailˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static iter.Seq2<nint, nint> storeYield2() {
    return (Func<nint, nint, bool> yield) => {
        yieldSlot2 = yield;
        if (!yield(23, 77)) {
            return;
        }
    };
}

internal static Func<nint, nint, bool> yieldSlot2;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nextˢ = "next"u8;
private static readonly object boomˢ = (@string)"boom"u8;
private static readonly object failedToPropagatePanicOnˢ = (@string)"failed to propagate panic on first next"u8;
private static readonly object nextReturnedTrueAfterˢ = (@string)"next returned true after iterator panicked"u8;
private static readonly @string stopˢ = "stop"u8;
private static readonly object failedToPropagatePanicOnˢ2 = (@string)"failed to propagate panic on stop"u8;

public static void TestPullPanic(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Run(nextˢ, (ж<testing.T> tΔ1) => {
        var (next, stop) = Pull(panicSeq());
        var nextʗ1 = next;
        if (!panicsWith(boomˢ, () => {
            nextʗ1();
        })) {
            tΔ1.Fatal(failedToPropagatePanicOnˢ);
        }
        // Make sure we don't panic again if we try to call next or stop.
        {
            var (_, ok) = next(); if (ok) {
                tΔ1.Fatal(nextReturnedTrueAfterˢ);
            }
        }
        // Calling stop again should be a no-op.
        stop();
    });
    Ꮡt.Run(stopˢ, (ж<testing.T> tΔ2) => {
        var (next, stop) = Pull(panicCleanupSeq());
        var (x, ok) = next();
        if (!ok || x != 55) {
            tΔ2.Fatalf("expected (55, true) from next, got (%d, %t)"u8, x, ok);
        }
        var stopʗ1 = stop;
        if (!panicsWith(boomˢ, () => {
            stopʗ1();
        })) {
            tΔ2.Fatal(failedToPropagatePanicOnˢ2);
        }
        // Make sure we don't panic again if we try to call next or stop.
        {
            var (_, okΔ1) = next(); if (okΔ1) {
                tΔ2.Fatal(nextReturnedTrueAfterˢ);
            }
        }
        // Calling stop again should be a no-op.
        stop();
    });
}

internal static iter.Seq<nint> panicSeq() {
    return (Func<nint, bool> yield) => {
        throw panic("boom");
    };
}

internal static iter.Seq<nint> panicCleanupSeq() {
    return (Func<nint, bool> yield) => {
        while (ᐧ) {
            if (!yield(55)) {
                throw panic("boom");
            }
        }
    };
}

public static void TestPull2Panic(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Run(nextˢ, (ж<testing.T> tΔ1) => {
        var (next, stop) = Pull2(panicSeq2());
        var nextʗ1 = next;
        if (!panicsWith(boomˢ, () => {
            nextʗ1();
        })) {
            tΔ1.Fatal(failedToPropagatePanicOnˢ);
        }
        // Make sure we don't panic again if we try to call next or stop.
        {
            var (_, _, ok) = next(); if (ok) {
                tΔ1.Fatal(nextReturnedTrueAfterˢ);
            }
        }
        // Calling stop again should be a no-op.
        stop();
    });
    Ꮡt.Run(stopˢ, (ж<testing.T> tΔ2) => {
        var (next, stop) = Pull2(panicCleanupSeq2());
        var (x, y, ok) = next();
        if (!ok || x != 55 || y != 100) {
            tΔ2.Fatalf("expected (55, 100, true) from next, got (%d, %d, %t)"u8, x, y, ok);
        }
        var stopʗ1 = stop;
        if (!panicsWith(boomˢ, () => {
            stopʗ1();
        })) {
            tΔ2.Fatal(failedToPropagatePanicOnˢ2);
        }
        // Make sure we don't panic again if we try to call next or stop.
        {
            var (_, _, okΔ1) = next(); if (okΔ1) {
                tΔ2.Fatal(nextReturnedTrueAfterˢ);
            }
        }
        // Calling stop again should be a no-op.
        stop();
    });
}

internal static iter.Seq2<nint, nint> panicSeq2() {
    return (Func<nint, nint, bool> yield) => {
        throw panic("boom");
    };
}

internal static iter.Seq2<nint, nint> panicCleanupSeq2() {
    return (Func<nint, nint, bool> yield) => {
        while (ᐧ) {
            if (!yield(55, 100)) {
                throw panic("boom");
            }
        }
    };
}

internal static bool /*panicked*/ panicsWith(any v, Action f) {
    bool panicked = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    if (!AreEqual(r, v)) {
                        throw panic(r);
                    }
                    panicked = true;
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return panicked;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object failedToGoexitFromNextˢ = (@string)"failed to Goexit from next"u8;
private static readonly object iteratorReturnedValidˢ = (@string)"iterator returned valid value after iterator Goexited"u8;
private static readonly object failedToGoexitFromStopˢ = (@string)"failed to Goexit from stop"u8;
private static readonly object nextReturnedTrueOrNonˢ = (@string)"next returned true or non-zero value after iterator Goexited"u8;

public static void TestPullGoexit(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Run(nextˢ, (ж<testing.T> tΔ1) => {
        ref var next = ref heap<Func<(nint, bool)>>(out var Ꮡnext);
        ref var stop = ref heap<Action>(out var Ꮡstop);
        if (!goexits(tΔ1, () => {
            (Ꮡnext.ValueSlot, Ꮡstop.ValueSlot) = Pull(goexitSeq());
            Ꮡnext.ValueSlot();
        })) {
            tΔ1.Fatal(failedToGoexitFromNextˢ);
        }
        {
            var (x, ok) = Ꮡnext.ValueSlot(); if (x != 0 || ok) {
                tΔ1.Fatal(iteratorReturnedValidˢ);
            }
        }
        Ꮡstop.ValueSlot();
    });
    Ꮡt.Run(stopˢ, (ж<testing.T> tΔ2) => {
        var (next, stop) = Pull(goexitCleanupSeq());
        var (x, ok) = next();
        if (!ok || x != 55) {
            tΔ2.Fatalf("expected (55, true) from next, got (%d, %t)"u8, x, ok);
        }
        var stopʗ1 = stop;
        if (!goexits(tΔ2, () => {
            stopʗ1();
        })) {
            tΔ2.Fatal(failedToGoexitFromStopˢ);
        }
        // Make sure we don't panic again if we try to call next or stop.
        {
            var (xΔ1, okΔ1) = next(); if (xΔ1 != 0 || okΔ1) {
                tΔ2.Fatal(nextReturnedTrueOrNonˢ);
            }
        }
        // Calling stop again should be a no-op.
        stop();
    });
}

internal static iter.Seq<nint> goexitSeq() {
    return (Func<nint, bool> yield) => {
        Δruntime.Goexit();
    };
}

internal static iter.Seq<nint> goexitCleanupSeq() {
    return (Func<nint, bool> yield) => {
        while (ᐧ) {
            if (!yield(55)) {
                Δruntime.Goexit();
            }
        }
    };
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nextReturnedTrueOrNonˢ2 = (@string)"next returned true or non-zero after iterator Goexited"u8;

public static void TestPull2Goexit(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    Ꮡt.Run(nextˢ, (ж<testing.T> tΔ1) => {
        ref var next = ref heap<Func<(nint, nint, bool)>>(out var Ꮡnext);
        ref var stop = ref heap<Action>(out var Ꮡstop);
        if (!goexits(tΔ1, () => {
            (Ꮡnext.ValueSlot, Ꮡstop.ValueSlot) = Pull2(goexitSeq2());
            Ꮡnext.ValueSlot();
        })) {
            tΔ1.Fatal(failedToGoexitFromNextˢ);
        }
        {
            var (x, y, ok) = Ꮡnext.ValueSlot(); if (x != 0 || y != 0 || ok) {
                tΔ1.Fatal(iteratorReturnedValidˢ);
            }
        }
        Ꮡstop.ValueSlot();
    });
    Ꮡt.Run(stopˢ, (ж<testing.T> tΔ2) => {
        var (next, stop) = Pull2(goexitCleanupSeq2());
        var (x, y, ok) = next();
        if (!ok || x != 55 || y != 100) {
            tΔ2.Fatalf("expected (55, 100, true) from next, got (%d, %d, %t)"u8, x, y, ok);
        }
        var stopʗ1 = stop;
        if (!goexits(tΔ2, () => {
            stopʗ1();
        })) {
            tΔ2.Fatal(failedToGoexitFromStopˢ);
        }
        // Make sure we don't panic again if we try to call next or stop.
        {
            var (xΔ1, yΔ1, okΔ1) = next(); if (xΔ1 != 0 || yΔ1 != 0 || okΔ1) {
                tΔ2.Fatal(nextReturnedTrueOrNonˢ2);
            }
        }
        // Calling stop again should be a no-op.
        stop();
    });
}

internal static iter.Seq2<nint, nint> goexitSeq2() {
    return (Func<nint, nint, bool> yield) => {
        Δruntime.Goexit();
    };
}

internal static iter.Seq2<nint, nint> goexitCleanupSeq2() {
    return (Func<nint, nint, bool> yield) => {
        while (ᐧ) {
            if (!yield(55, 100)) {
                Δruntime.Goexit();
            }
        }
    };
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static bool goexits(ж<testing.T> Ꮡt, Action f) {
    Ꮡt.Helper();
    var exit = new channel<bool>(0);
    var exitʗ1 = exit;
    goǃ(() => {
        GoFrame ᒐ = default;
        try {
            var cleanExit = false;
            var exitʗ2 = exitʗ1;
            defer(() => {
                exitʗ2.ᐸꟷ(recover() == default! && !cleanExit);
            }, ref ᒐ);
            f();
            cleanExit = true;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    });
    return ᐸꟷ(exit);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object nextReturnedTrueAfterˢ2 = (@string)"next returned true after iterator was stopped"u8;

public static void TestPullImmediateStop(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (next, stop) = Pull(panicSeq());
    stop();
    // Make sure we don't panic if we try to call next or stop.
    {
        var (_, ok) = next(); if (ok) {
            Ꮡt.Fatal(nextReturnedTrueAfterˢ2);
        }
    }
}

public static void TestPull2ImmediateStop(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    var (next, stop) = Pull2(panicSeq2());
    stop();
    // Make sure we don't panic if we try to call next or stop.
    {
        var (_, _, ok) = next(); if (ok) {
            Ꮡt.Fatal(nextReturnedTrueAfterˢ2);
        }
    }
}

} // end iter_test_package
