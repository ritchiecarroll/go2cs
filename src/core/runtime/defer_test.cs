// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using slices = slices_package;
using testing = testing_package;
using System.Runtime.CompilerServices;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedUnconditionalˢ = (@string)"expected unconditional panic"u8;

// Make sure open-coded defer exit code is not lost, even when there is an
// unconditional panic (hence no return from the function)
public static void TestUnconditionalPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (!AreEqual(recover(), (@string)("testUnconditional"))) {
                Ꮡt.Fatal(expectedUnconditionalˢ);
            }
        }, ref ᒐ);
        throw panic("testUnconditional");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static nint glob = 3;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedTestNonOpenPanicˢ = (@string)"expected testNonOpen panic"u8;

// Test an open-coded defer and non-open-coded defer - make sure both defers run
// and call recover()
public static void TestOpenAndNonOpenDefers(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        while (ᐧ) {
            // Non-open defer because in a loop
            defer((nint n) => {
                if (!AreEqual(recover(), (@string)("testNonOpenDefer"))) {
                    Ꮡt.Fatal(expectedTestNonOpenPanicˢ);
                }
            }, (nint)(3), ref ᒐ);
            if (glob > 2) {
                break;
            }
        }
        testOpen(Ꮡt, 47);
        throw panic("testNonOpenDefer");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedTestOpenPanicˢ = (@string)"expected testOpen panic"u8;

//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static void testOpen(ж<testing.T> Ꮡt, nint arg) {
    GoFrame ᒐ = default;
    try {
        defer((nint n) => {
            if (!AreEqual(recover(), (@string)("testOpenDefer"))) {
                Ꮡt.Fatal(expectedTestOpenPanicˢ);
            }
        }, (nint)(4), ref ᒐ);
        if (arg > 2) {
            throw panic("testOpenDefer");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test a non-open-coded defer and an open-coded defer - make sure both defers run
// and call recover()
public static void TestNonOpenAndOpenDefers(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        testOpen(Ꮡt, 47);
        while (ᐧ) {
            // Non-open defer because in a loop
            defer((nint n) => {
                if (!AreEqual(recover(), (@string)("testNonOpenDefer"))) {
                    Ꮡt.Fatal(expectedTestNonOpenPanicˢ);
                }
            }, (nint)(3), ref ᒐ);
            if (glob > 2) {
                break;
            }
        }
        throw panic("testNonOpenDefer");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static slice<nint> list;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object expectedPanicˢ = (@string)"expected panic"u8;

// Make sure that conditional open-coded defers are activated correctly and run in
// the correct order.
public static void TestConditionalDefers(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        list = new slice<nint>(0, 10);
        defer(() => {
            if (!AreEqual(recover(), (@string)("testConditional"))) {
                Ꮡt.Fatal(expectedPanicˢ);
            }
            var want = new nint[]{4, 2, 1}.slice();
            if (!slices.Equal<slice<nint>, nint>(want, list)) {
                Ꮡt.Fatalf("wanted %v, got %v"u8, want, list);
            }
        }, ref ᒐ);
        testConditionalDefers(8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void testConditionalDefers(nint n) {
    GoFrame ᒐ = default;
    try {
        void doappend(nint i) {
            list = append(list, i);
        }
        var doappendʗ1 = doappend;
        defer(doappendʗ1, (nint)(1), ref ᒐ);
        if (n > 5) {
            var doappendʗ2 = doappend;
            defer(doappendʗ2, (nint)(2), ref ᒐ);
            if (n > 8){
                var doappendʗ3 = doappend;
                defer(doappendʗ3, (nint)(3), ref ᒐ);
            } else {
                var doappendʗ4 = doappend;
                defer(doappendʗ4, (nint)(4), ref ᒐ);
            }
        }
        throw panic("testConditional");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object deferShouldnTRunˢ = (@string)"Defer shouldn't run"u8;

// Test that there is no compile-time or run-time error if an open-coded defer
// call is removed by constant propagation and dead-code elimination.
public static void TestDisappearingDefer(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var exprᴛ1 = Δruntime.GOOS;
        if (exprᴛ1 == "invalidOS"u8) {
            defer(() => {
                Ꮡt.Fatal(deferShouldnTRunˢ);
            }, ref ᒐ);
        }

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// This tests an extra recursive panic behavior that is only specified in the
// code. Suppose a first panic P1 happens and starts processing defer calls. If a
// second panic P2 happens while processing defer call D in frame F, then defer
// call processing is restarted (with some potentially new defer calls created by
// D or its callees). If the defer processing reaches the started defer call D
// again in the defer stack, then the original panic P1 is aborted and cannot
// continue panic processing or be recovered. If the panic P2 does a recover at
// some point, it will naturally remove the original panic P1 from the stack
// (since the original panic had to be in frame F or a descendant of F).
public static void TestAbortedPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            if (r != default!) {
                Ꮡt.Fatalf("wanted nil recover, got %v"u8, r);
            }
        }, ref ᒐ);
        defer(() => {
            var r = recover();
            if (!AreEqual(r, (@string)("panic2"))) {
                Ꮡt.Fatalf("wanted %v, got %v"u8, panic2ˢ, r);
            }
        }, ref ᒐ);
        defer(() => {
            throw panic("panic2");
        }, ref ᒐ);
        throw panic("panic1");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object panic1ˢ = (@string)"panic1"u8;

// This tests that recover() does not succeed unless it is called directly from a
// defer function that is directly called by the panic.  Here, we first call it
// from a defer function that is created by the defer function called directly by
// the panic.  In
public static void TestRecoverMatching(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        defer(() => {
            var r = recover();
            if (!AreEqual(r, (@string)("panic1"))) {
                Ꮡt.Fatalf("wanted %v, got %v"u8, panic1ˢ, r);
            }
        }, ref ᒐ);
        defer(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    // Shouldn't succeed, even though it is called directly
                    // from a defer function, since this defer function was
                    // not directly called by the panic.
                    var r = recover();
                    if (r != default!) {
                        Ꮡt.Fatalf("wanted nil recover, got %v"u8, r);
                    }
                }, ref ᒐ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, ref ᒐ);
        throw panic("panic1");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType("[128]byte")] partial struct nonSSAable;

[GoType] partial struct bigStruct {
    internal int64 x, y, z, w, p, q;
}

[GoType] partial struct containsBigStruct {
    internal bigStruct element;
}

internal static nonSSAable mknonSSAable() {
    globint1++;
    return new nonSSAable(new byte[]{0, 0, 0, 0, 5}.array(128));
}

internal static nint globint1;
internal static nint globint2;
internal static nint globint3;

//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static int64 sideeffect(int64 n) {
    globint2++;
    return n;
}

internal static containsBigStruct sideeffect2(containsBigStruct @in) {
    globint3++;
    return @in;
}

// Test that nonSSAable arguments to defer are handled correctly and only evaluated once.
public static void TestNonSSAableArgs(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        globint1 = 0;
        globint2 = 0;
        globint3 = 0;
        byte save1 = default!;
        int64 save2 = default!;
        int64 save3 = default!;
        int64 save4 = default!;
        defer(() => {
            if (globint1 != 1) {
                Ꮡt.Fatalf("globint1:  wanted: 1, got %v"u8, globint1);
            }
            if (save1 != 5) {
                Ꮡt.Fatalf("save1:  wanted: 5, got %v"u8, save1);
            }
            if (globint2 != 1) {
                Ꮡt.Fatalf("globint2:  wanted: 1, got %v"u8, globint2);
            }
            if (save2 != 2) {
                Ꮡt.Fatalf("save2:  wanted: 2, got %v"u8, save2);
            }
            if (save3 != 4) {
                Ꮡt.Fatalf("save3:  wanted: 4, got %v"u8, save3);
            }
            if (globint3 != 1) {
                Ꮡt.Fatalf("globint3:  wanted: 1, got %v"u8, globint3);
            }
            if (save4 != 4) {
                Ꮡt.Fatalf("save1:  wanted: 4, got %v"u8, save4);
            }
        }, ref ᒐ);
        // Test function returning a non-SSAable arg
        defer((nonSSAable n) => {
            n = n.Clone();
            save1 = n[4];
        }, mknonSSAable(), ref ᒐ);
        // Test composite literal that is not SSAable
        defer((bigStruct b) => {
            save2 = b.y;
        }, new bigStruct(1, 2, 3, 4, 5, sideeffect(6)), ref ᒐ);
        // Test struct field reference that is non-SSAable
        ref var foo = ref heap<containsBigStruct>(out var Ꮡfoo);
        foo = new containsBigStruct(nil);
        foo.element.z = 4;
        defer((bigStruct element) => {
            save3 = element.z;
        }, foo.element, ref ᒐ);
        defer((bigStruct element) => {
            save4 = element.z;
        }, sideeffect2(foo).element, ref ᒐ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] internal static void doPanic() {
    throw panic("Test panic");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didnTFindExpectedPanicˢ = (@string)"Didn't find expected panic"u8;

public static void TestDeferForFuncWithNoExit(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        nint cond = 1;
        defer(() => {
            if (cond != 2) {
                Ꮡt.Fatalf("cond: wanted 2, got %v"u8, cond);
            }
            if (!AreEqual(recover(), (@string)("Test panic"))) {
                Ꮡt.Fatal(didnTFindExpectedPanicˢ);
            }
        }, ref ᒐ);
        ref var x = ref heap<nint>(out var Ꮡx);
        x = 0;
        // Force a stack copy, to make sure that the &cond pointer passed to defer
        // function is properly updated.
        growStackIter(Ꮡx, 1000);
        cond = 2;
        doPanic();
        // This function has no exit/return, since it ends with an infinite loop
        while (ᐧ) {
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Test case approximating issue #37664, where a recursive function (interpreter)
// may do repeated recovers/re-panics until it reaches the frame where the panic
// can actually be handled. The recurseFnPanicRec() function is testing that there
// are no stale defer structs on the defer chain after the interpreter() sequence,
// by writing a bunch of 0xffffffffs into several recursive stack frames, and then
// doing a single panic-recover which would invoke any such stale defer structs.
public static void TestDeferWithRepeatedRepanics(ж<testing.T> Ꮡt) {
    interpreter(0, 6, 2);
    recurseFnPanicRec(0, 10);
    interpreter(0, 5, 1);
    recurseFnPanicRec(0, 10);
    interpreter(0, 6, 3);
    recurseFnPanicRec(0, 10);
}

internal static void interpreter(nint level, nint maxlevel, nint rec) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var e = recover();
            if (e == default!) {
                return;
            }
            if (level != e._<nint>()) {
                //fmt.Fprintln(os.Stderr, "re-panicing, level", level)
                throw panic(e);
            }
        }, ref ᒐ);
        //fmt.Fprintln(os.Stderr, "Recovered, level", level)
        if (level + 1 < maxlevel){
            interpreter(level + 1, maxlevel, rec);
        } else {
            //fmt.Fprintln(os.Stderr, "Initiating panic")
            throw panic(rec);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void recurseFnPanicRec(nint level, nint maxlevel) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            recover();
        }, ref ᒐ);
        recurseFn(level, maxlevel);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static uint32 saveInt;

internal static void recurseFn(nint level, nint maxlevel) {
    var a = new uint32[]{0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU, 0xffffffffU}.array();
    if (level + 1 < maxlevel){
        // Make sure a array is referenced, so it is not optimized away
        saveInt = a[4];
        recurseFn(level + 1, maxlevel);
    } else {
        throw panic("recurseFn panic");
    }
}

// Try to reproduce issue #37688, where a pointer to an open-coded defer struct is
// mistakenly held, and that struct keeps a pointer to a stack-allocated defer
// struct, and that stack-allocated struct gets overwritten or the stack gets
// moved, so a memory error happens on GC.
public static void TestIssue37688(ж<testing.T> Ꮡt) {
    for (nint j = 0; j < 10; j++) {
        g2();
        g3();
    }
}

[GoType] partial struct foo {
}

//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] [GoRecv] internal static void method1(this ref foo f) {
}

//go:noinline
[MethodImpl(MethodImplOptions.NoInlining)] [GoRecv] internal static void method2(this ref foo f) {
}

internal static void g2() {
    GoFrame ᒐ = default;
    try {
        ref var a = ref heap(new foo(), out var Ꮡa);
        var ap = Ꮡa;
        // The loop forces this defer to be heap-allocated and the remaining two
        // to be stack-allocated.
        for (nint i = 0; i < 1; i++) {
            var apʗ1 = ap;
            defer(apʗ1.method1, ref ᒐ);
        }
        var apʗ2 = ap;
        defer(apʗ2.method2, ref ᒐ);
        var apʗ3 = ap;
        defer(apʗ3.method1, ref ᒐ);
        ff1(ap, 1, 2, 3, 4, 5, 6, 7, 8, 9);
        // Try to get the stack to be moved by growing it too large, so
        // existing stack-allocated defer becomes invalid.
        rec1(2000);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void g3() {
    // Mix up the stack layout by adding in an extra function frame
    g2();
}


[GoType("dyn")] partial struct globstructᴛ1 {
    internal nint a, b, c, d, e, f, g, h, i;
}
internal static globstructᴛ1 globstruct;

internal static void ff1(ж<foo> Ꮡap, nint a, nint b, nint c, nint d, nint e, nint f, nint g, nint h, nint i) {
    GoFrame ᒐ = default;
    try {
        defer(Ꮡap.method1, ref ᒐ);
        // Make a defer that has a very large set of args, hence big size for the
        // defer record for the open-coded frame (which means it won't use the
        // defer pool)
        defer((ж<foo> apΔ1, nint aΔ1, nint bΔ1, nint cΔ1, nint dΔ1, nint eΔ1, nint fΔ1, nint gΔ1, nint hΔ1, nint iΔ1) => {
            {
                var v = recover(); if (v != default!) {
                }
            }
            globstruct.a = aΔ1;
            globstruct.b = bΔ1;
            globstruct.c = cΔ1;
            globstruct.d = dΔ1;
            globstruct.e = eΔ1;
            globstruct.f = fΔ1;
            globstruct.g = gΔ1;
            globstruct.h = hΔ1;
        }, Ꮡap, a, b, c, d, e, f, g, h, i, ref ᒐ);
        throw panic("ff1 panic");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void rec1(nint max) {
    if (max > 0) {
        rec1(max - 1);
    }
}

public static void TestIssue43921(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            expect(Ꮡt, 1, recover());
        }, ref ᒐ);
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                // Prevent open-coded defers
                while (ᐧ) {
                    defer(() => {
                    }, ref ᒐ);
                    break;
                }
                defer(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(() => {
                            expect(Ꮡt, 4, recover());
                        }, ref ᒐ);
                        throw panic((nint)(4));
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, ref ᒐ);
                throw panic((nint)(1));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void expect(ж<testing.T> Ꮡt, nint n, any err) {
    if (!AreEqual(n, err)) {
        Ꮡt.Fatalf("have %v, want %v"u8, err, n);
    }
}

public static void TestIssue43920(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var steps = ref heap(new nint(), out var Ꮡsteps);
        defer(() => {
            expect(Ꮡt, 1, recover());
        }, ref ᒐ);
        defer(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(() => {
                            expect(Ꮡt, 5, recover());
                        }, ref ᒐ);
                        defer(ᴛ1 => throw panic(ᴛ1), (nint)(5), ref ᒐ);
                        ((Action)(() => {
                            throw panic((nint)(4));
                        }))();
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }, ref ᒐ);
                defer(() => {
                    expect(Ꮡt, 3, recover());
                }, ref ᒐ);
                defer(ᴛ1 => throw panic(ᴛ1), (nint)(3), ref ᒐ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, ref ᒐ);
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(step, Ꮡt, Ꮡsteps, (nint)(1), ref ᒐ);
                throw panic((nint)(1));
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void step(ж<testing.T> Ꮡt, ж<nint> Ꮡsteps, nint want) {
    ref var steps = ref Ꮡsteps.DerefOrNull();

    steps++;
    if (steps != want) {
        Ꮡt.Fatalf("have %v, want %v"u8, steps, want);
    }
}

public static void TestIssue43941(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        ref var steps = ref heap(new nint(), out var Ꮡsteps);
        steps = 7;
        defer(() => {
            step(Ꮡt, Ꮡsteps, 14);
            expect(Ꮡt, 4, recover());
        }, ref ᒐ);
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                ((Action)(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(() => {
                            GoFrame ᒐ = default;
                            try {
                                defer(() => {
                                    expect(Ꮡt, 3, recover());
                                }, ref ᒐ);
                                defer(ᴛ1 => throw panic(ᴛ1), (nint)(3), ref ᒐ);
                                throw panic((nint)(2));
                            }
                            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                            finally { ᒐ.Run(); }
                        }, ref ᒐ);
                        defer(() => {
                            expect(Ꮡt, 1, recover());
                        }, ref ᒐ);
                        defer(ᴛ1 => throw panic(ᴛ1), (nint)(1), ref ᒐ);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }))();
                defer(() => {
                }, ref ᒐ);
                defer(() => {
                }, ref ᒐ);
                defer(step, Ꮡt, Ꮡsteps, (nint)(10), ref ᒐ);
                defer(step, Ꮡt, Ꮡsteps, (nint)(9), ref ᒐ);
                step(Ꮡt, Ꮡsteps, 8);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(step, Ꮡt, Ꮡsteps, (nint)(13), ref ᒐ);
                defer(step, Ꮡt, Ꮡsteps, (nint)(12), ref ᒐ);
                ((Action)(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(step, Ꮡt, Ꮡsteps, (nint)(11), ref ᒐ);
                        throw panic((nint)(4));
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }))();
                // Code below isn't executed,
                // but removing it breaks the test case.
                defer(() => {
                }, ref ᒐ);
                defer(ᴛ1 => throw panic(ᴛ1), (nint)(-1), ref ᒐ);
                defer(step, Ꮡt, Ꮡsteps, (nint)(-1), ref ᒐ);
                defer(step, Ꮡt, Ꮡsteps, (nint)(-1), ref ᒐ);
                defer(() => {
                }, ref ᒐ);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package
