// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using slices = slices_package;
using strings = strings_package;
using testing = testing_package;
using System.Runtime.CompilerServices;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

[MethodImpl(MethodImplOptions.NoInlining)] internal static slice<uintptr> f1(bool pan) {
    return f2(pan); // line 15
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static slice<uintptr> f2(bool pan) {
    return f3(pan); // line 19
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static slice<uintptr> f3(bool pan) {
    if (pan) {
        throw panic("f3"); // line 24
    }
    var ret = new slice<uintptr>(20);
    return ret.slice(0, Δruntime.Callers(0, ret)); // line 27
}

[GoType("dyn")] internal partial struct testCallers_want {
    internal @string name;
    internal nint line;
}

internal static void testCallers(ж<testing.T> Ꮡt, slice<uintptr> pcs, bool pan) {
    var m = new map<@string, nint>(len(pcs));
    var frames = Δruntime.CallersFrames(pcs);
    while (ᐧ) {
        var (frame, more) = frames.Next();
        if (frame.Function != ""u8) {
            m[frame.Function] = frame.Line;
        }
        if (!more) {
            break;
        }
    }
    slice<@string> seen = default!;
    foreach (var (k, _) in m) {
        seen = append(seen, k);
    }
    Ꮡt.Logf("functions seen: %s"u8, strings.Join(seen, " "u8));
    nint f3Line = default!;
    if (pan){
        f3Line = 24;
    } else {
        f3Line = 27;
    }
    var want = new testCallers_want[]{
        new("f1"u8, 15),
        new("f2"u8, 19),
        new("f3"u8, f3Line)
    }.slice();
    foreach (var (_, w) in want) {
        {
            nint got = m["runtime_test."u8 + w.name]; if (got != w.line) {
                Ꮡt.Errorf("%s is line %d, want %d"u8, w.name, got, w.line);
            }
        }
    }
}

internal static void testCallersEqual(ж<testing.T> Ꮡt, slice<uintptr> pcs, slice<@string> want) {
    Ꮡt.Helper();
    var got = new slice<@string>(0, len(want));
    var frames = Δruntime.CallersFrames(pcs);
    while (ᐧ) {
        var (frame, more) = frames.Next();
        if (!more || len(got) >= len(want)) {
            break;
        }
        got = append(got, frame.Function);
    }
    if (!slices.Equal<slice<@string>, @string>(want, got)) {
        Ꮡt.Fatalf("wanted %v, got %v"u8, want, got);
    }
}

public static void TestCallers(ж<testing.T> Ꮡt) {
    testCallers(Ꮡt, f1(false), false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didNotPanicˢ = (@string)"did not panic"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCallersPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Make sure we don't have any extra frames on the stack (due to
        // open-coded defer processing)
        var want = new @string[]{"runtime.Callers"u8, "runtime_test.TestCallersPanic.func1"u8,
            "runtime.gopanic"u8, "runtime_test.f3"u8, "runtime_test.f2"u8, "runtime_test.f1"u8,
            "runtime_test.TestCallersPanic"u8}.slice();
        var wantʗ1 = want;
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatal(didNotPanicˢ);
                }
            }
            var pcs = new slice<uintptr>(20);
            pcs = pcs.slice(0, Δruntime.Callers(0, pcs));
            testCallers(Ꮡt, pcs, true);
            testCallersEqual(Ꮡt, pcs, wantʗ1);
        }, ref ᒐ);
        f1(true);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCallersDoublePanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Make sure we don't have any extra frames on the stack (due to
        // open-coded defer processing)
        var want = new @string[]{"runtime.Callers"u8, "runtime_test.TestCallersDoublePanic.func1.1"u8,
            "runtime.gopanic"u8, "runtime_test.TestCallersDoublePanic.func1"u8, "runtime.gopanic"u8, "runtime_test.TestCallersDoublePanic"u8}.slice();
        var wantʗ1 = want;
        defer(() => {
            GoFrame ᒐ = default;
            try {
                var wantʗ2 = wantʗ1;
                defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
                    var pcs = new slice<uintptr>(20);
                    pcs = pcs.slice(0, Δruntime.Callers(0, pcs));
                    if (recover() == default!) {
                        Ꮡt.Fatal(didNotPanicˢ);
                    }
                    testCallersEqual(Ꮡt, pcs, wantʗ2);
                }, ref ᒐ);
                if (recover() == default!) {
                    Ꮡt.Fatal(didNotPanicˢ);
                }
                throw panic((nint)(2));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, ref ᒐ);
        throw panic((nint)(1));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didNotRecoverFromPanicˢ = (@string)"did not recover from panic"u8;

// Test that a defer after a successful recovery looks like it is called directly
// from the function with the defers.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCallersAfterRecovery(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var want = new @string[]{"runtime.Callers"u8, "runtime_test.TestCallersAfterRecovery.func1"u8, "runtime_test.TestCallersAfterRecovery"u8}.slice();
        var wantʗ1 = want;
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var pcs = new slice<uintptr>(20);
            pcs = pcs.slice(0, Δruntime.Callers(0, pcs));
            testCallersEqual(Ꮡt, pcs, wantʗ1);
        }, ref ᒐ);
        defer(() => {
            if (recover() == default!) {
                Ꮡt.Fatal(didNotRecoverFromPanicˢ);
            }
        }, ref ᒐ);
        throw panic((nint)(1));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object panic2ˢ = (@string)"panic2"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCallersAbortedPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var want = new @string[]{"runtime.Callers"u8, "runtime_test.TestCallersAbortedPanic.func2"u8, "runtime_test.TestCallersAbortedPanic"u8}.slice();
        defer(() => {
            var r = recover();
            if (r != default!) {
                Ꮡt.Fatalf("should be no panic remaining to recover"u8);
            }
        }, ref ᒐ);
        var wantʗ1 = want;
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            // panic2 was aborted/replaced by panic1, so when panic2 was
            // recovered, there is no remaining panic on the stack.
            var pcs = new slice<uintptr>(20);
            pcs = pcs.slice(0, Δruntime.Callers(0, pcs));
            testCallersEqual(Ꮡt, pcs, wantʗ1);
        }, ref ᒐ);
        defer(() => {
            var r = recover();
            if (!AreEqual(r, (@string)("panic2"))) {
                Ꮡt.Fatalf("got %v, wanted %v"u8, r, panic2ˢ);
            }
        }, ref ᒐ);
        defer(() => {
            // panic2 aborts/replaces panic1, because it is a recursive panic
            // that is not recovered within the defer function called by
            // panic1 panicking sequence
            throw panic("panic2");
        }, ref ᒐ);
        throw panic("panic1");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCallersAbortedPanic2(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var want = new @string[]{"runtime.Callers"u8, "runtime_test.TestCallersAbortedPanic2.func2"u8, "runtime_test.TestCallersAbortedPanic2"u8}.slice();
        defer(() => {
            var r = recover();
            if (r != default!) {
                Ꮡt.Fatalf("should be no panic remaining to recover"u8);
            }
        }, ref ᒐ);
        var wantʗ1 = want;
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            var pcs = new slice<uintptr>(20);
            pcs = pcs.slice(0, Δruntime.Callers(0, pcs));
            testCallersEqual(Ꮡt, pcs, wantʗ1);
        }, ref ᒐ);
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    var r = recover();
                    if (!AreEqual(r, (@string)("panic2"))) {
                        Ꮡt.Fatalf("got %v, wanted %v"u8, r, panic2ˢ);
                    }
                }, ref ᒐ);
                ((Action)(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(() => {
                            // Again, panic2 aborts/replaces panic1
                            throw panic("panic2");
                        }, ref ᒐ);
                        throw panic("panic1");
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }))();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didNotSeeNilPointerPanicˢ = (@string)"did not see nil pointer panic"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCallersNilPointerPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Make sure we don't have any extra frames on the stack (due to
        // open-coded defer processing)
        var want = new @string[]{"runtime.Callers"u8, "runtime_test.TestCallersNilPointerPanic.func1"u8,
            "runtime.gopanic"u8, "runtime.panicmem"u8, "runtime.sigpanic"u8,
            "runtime_test.TestCallersNilPointerPanic"u8}.slice();
        var wantʗ1 = want;
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatal(didNotPanicˢ);
                }
            }
            var pcs = new slice<uintptr>(20);
            pcs = pcs.slice(0, Δruntime.Callers(0, pcs));
            testCallersEqual(Ꮡt, pcs, wantʗ1);
        }, ref ᒐ);
        ж<nint> p = default!;
        if (p.Value == 3) {
            Ꮡt.Fatal(didNotSeeNilPointerPanicˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didNotSeeDivideBySizerˢ = (@string)"did not see divide-by-sizer panic"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCallersDivZeroPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Make sure we don't have any extra frames on the stack (due to
        // open-coded defer processing)
        var want = new @string[]{"runtime.Callers"u8, "runtime_test.TestCallersDivZeroPanic.func1"u8,
            "runtime.gopanic"u8, "runtime.panicdivide"u8,
            "runtime_test.TestCallersDivZeroPanic"u8}.slice();
        var wantʗ1 = want;
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatal(didNotPanicˢ);
                }
            }
            var pcs = new slice<uintptr>(20);
            pcs = pcs.slice(0, Δruntime.Callers(0, pcs));
            testCallersEqual(Ꮡt, pcs, wantʗ1);
        }, ref ᒐ);
        nint n = default!;
        if (5 / n == 1) {
            Ꮡt.Fatal(didNotSeeDivideBySizerˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object nilDeferFuncPanickedAtˢ = (@string)"nil defer func panicked at defer time rather than function exit time"u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCallersDeferNilFuncPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        // Make sure we don't have any extra frames on the stack. We cut off the check
        // at runtime.sigpanic, because non-open-coded defers (which may be used in
        // non-opt or race checker mode) include an extra 'deferreturn' frame (which is
        // where the nil pointer deref happens).
        nint state = 1;
        var want = new @string[]{"runtime.Callers"u8, "runtime_test.TestCallersDeferNilFuncPanic.func1"u8,
            "runtime.gopanic"u8, "runtime.panicmem"u8, "runtime.sigpanic"u8}.slice();
        var wantʗ1 = want;
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatal(didNotPanicˢ);
                }
            }
            var pcs = new slice<uintptr>(20);
            pcs = pcs.slice(0, Δruntime.Callers(0, pcs));
            testCallersEqual(Ꮡt, pcs, wantʗ1);
            if (state == 1) {
                Ꮡt.Fatal(nilDeferFuncPanickedAtˢ);
            }
        }, ref ᒐ);
        Action f = default!;
        var fʗ1 = f;
        defer(fʗ1, ref ᒐ);
        // Use the value of 'state' to make sure nil defer func f causes panic at
        // function exit, rather than at the defer statement.
        state = 2;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Same test, but forcing non-open-coded defer by putting the defer in a loop.  See
// issue #36050
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCallersDeferNilFuncPanicWithLoop(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        nint state = 1;
        var want = new @string[]{"runtime.Callers"u8, "runtime_test.TestCallersDeferNilFuncPanicWithLoop.func1"u8,
            "runtime.gopanic"u8, "runtime.panicmem"u8, "runtime.sigpanic"u8, "runtime.deferreturn"u8, "runtime_test.TestCallersDeferNilFuncPanicWithLoop"u8}.slice();
        var wantʗ1 = want;
        defer([MethodImpl(MethodImplOptions.NoInlining)] () => {
            {
                var r = recover(); if (r == default!) {
                    Ꮡt.Fatal(didNotPanicˢ);
                }
            }
            var pcs = new slice<uintptr>(20);
            pcs = pcs.slice(0, Δruntime.Callers(0, pcs));
            testCallersEqual(Ꮡt, pcs, wantʗ1);
            if (state == 1) {
                Ꮡt.Fatal(nilDeferFuncPanickedAtˢ);
            }
        }, ref ᒐ);
        for (nint i = 0; i < 1; i++) {
            Action f = default!;
            var fʗ1 = f;
            defer(fʗ1, ref ᒐ);
        }
        // Use the value of 'state' to make sure nil defer func f causes panic at
        // function exit, rather than at the defer statement.
        state = 2;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// issue #51988
// Func.Endlineno was lost when instantiating generic functions, leading to incorrect
// stack trace positions.
public static void TestCallersEndlineno(ж<testing.T> Ꮡt) {
    testNormalEndlineno(Ꮡt);
    testGenericEndlineno<nint>(Ꮡt);
}

internal static void testNormalEndlineno(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(testCallerLine, Ꮡt, callerLine(Ꮡt, 0) + 1, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testGenericEndlineno<_>(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(testCallerLine, Ꮡt, callerLine(Ꮡt, 0) + 1, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testCallerLine(ж<testing.T> Ꮡt, nint want) {
    {
        nint have = callerLine(Ꮡt, 1); if (have != want) {
            Ꮡt.Errorf("callerLine(1) returned %d, but want %d\n"u8, have, want);
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint callerLine(ж<testing.T> Ꮡt, nint skip) {
    var (_, _, line, ok) = Δruntime.Caller(skip + 1);
    if (!ok) {
        Ꮡt.Fatalf("runtime.Caller(%d) failed"u8, skip + 1);
    }
    return line;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cachedˢ = "cached"u8;
internal static readonly @string inlinedˢ = "inlined"u8;
internal static readonly @string noCacheˢ = "no-cache"u8;

public static void BenchmarkCallers(ж<testing.B> Ꮡb) {
    Ꮡb.Run(cachedˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ1) => {
        // Very pcvalueCache-friendly, no inlining.
        callersCached(bΔ1, 100);
    });
    Ꮡb.Run(inlinedˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ2) => {
        // Some inlining, still pretty cache-friendly.
        callersInlined(bΔ2, 100);
    });
    Ꮡb.Run(noCacheˢ, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ3) => {
        // Cache-hostile
        callersNoCache(bΔ3, 100);
    });
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint callersCached(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n <= 0) {
        var pcs = new slice<uintptr>(32);
        b.ResetTimer();
        for (nint i = 0; i < b.N; i++) {
            Δruntime.Callers(0, pcs);
        }
        b.StopTimer();
        return 0;
    }
    return 1 + callersCached(Ꮡb, n - 1);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint callersInlined(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n <= 0) {
        var pcs = new slice<uintptr>(32);
        b.ResetTimer();
        for (nint i = 0; i < b.N; i++) {
            Δruntime.Callers(0, pcs);
        }
        b.StopTimer();
        return 0;
    }
    return 1 + callersInlined1(Ꮡb, n - 1);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint callersInlined1(ж<testing.B> Ꮡb, nint n) {
    return callersInlined2(Ꮡb, n);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint callersInlined2(ж<testing.B> Ꮡb, nint n) {
    return callersInlined3(Ꮡb, n);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint callersInlined3(ж<testing.B> Ꮡb, nint n) {
    return callersInlined4(Ꮡb, n);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint callersInlined4(ж<testing.B> Ꮡb, nint n) {
    return callersInlined(Ꮡb, n);
}

[MethodImpl(MethodImplOptions.NoInlining)] internal static nint callersNoCache(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n <= 0) {
        var pcs = new slice<uintptr>(32);
        b.ResetTimer();
        for (nint i = 0; i < b.N; i++) {
            Δruntime.Callers(0, pcs);
        }
        b.StopTimer();
        return 0;
    }
    switch (n % 16) {
    case 0: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 1: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 2: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 3: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 4: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 5: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 6: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 7: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 8: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 9: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 10: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 11: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 12: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 13: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    case 14: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }
    default: {
        return 1 + callersNoCache(Ꮡb, n - 1);
    }}

}

public static void BenchmarkFPCallers(ж<testing.B> Ꮡb) {
    Ꮡb.Run(cachedˢ, (ж<testing.B> bΔ1) => {
        // Very pcvalueCache-friendly, no inlining.
        fpCallersCached(bΔ1, 100);
    });
}

internal static nint fpCallersCached(ж<testing.B> Ꮡb, nint n) {
    ref var b = ref Ꮡb.DerefOrNull();

    if (n <= 0) {
        var pcs = new slice<uintptr>(32);
        b.ResetTimer();
        for (nint i = 0; i < b.N; i++) {
            runtime_internal_test_package.FPCallers(pcs);
        }
        b.StopTimer();
        return 0;
    }
    return 1 + fpCallersCached(Ꮡb, n - 1);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object framePointersNotˢ = (@string)"frame pointers not supported for this architecture"u8;

public static void TestFPUnwindAfterRecovery(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (!runtime_internal_test_package.FramePointerEnabled) {
            Ꮡt.Skip(framePointersNotˢ);
        }
        // Make sure that frame pointer unwinding succeeds from a deferred
        // function run after recovering from a panic. It can fail if the
        // recovery does not properly restore the caller's frame pointer before
        // running the remaining deferred functions.
        //
        // This test does not verify the accuracy of the call stack (it
        // currently includes a frame from runtime.deferreturn which would
        // normally be omitted). It is only intended to check that producing the
        // call stack won't crash.
        defer(() => {
            var pcs = new slice<uintptr>(32);
            foreach (var (i, _) in pcs) {
                // If runtime.recovery doesn't properly restore the
                // frame pointer before returning control to this
                // function, it will point somewhere lower in the stack
                // from one of the frames of runtime.gopanic() or one of
                // it's callees prior to recovery.  So, we put some
                // non-zero values on the stack to ensure that frame
                // pointer unwinding will crash if it sees the old,
                // invalid frame pointer.
                pcs[i] = 10;
            }
            runtime_internal_test_package.FPCallers(pcs);
            Ꮡt.Logf("%v"u8, pcs);
        }, ref ᒐ);
        defer(() => {
            if (recover() == default!) {
                Ꮡt.Fatal(didNotRecoverFromPanicˢ);
            }
        }, ref ᒐ);
        throw panic((nint)(1));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package
