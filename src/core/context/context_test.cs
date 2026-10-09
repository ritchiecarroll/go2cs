// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

// Tests in package context cannot depend directly on package testing due to an import cycle.
// If your test does requires access to unexported members of the context package,
// add your test below as `func XTestFoo(t testingT)` and add a `TestFoo` to x_test.go
// that calls it. Otherwise, write a regular test in a test.go file in package context_test.
using time = time_package;
using go.sync;
using static go.context_package;
using ꓸꓸꓸany = Span<any>;

partial class context_internal_test_package {

public partial interface testingT {
    (time.Time, bool) Deadline();
    void Error(params ꓸꓸꓸany argsʗp);
    void Errorf(@string format, params ꓸꓸꓸany argsʗp);
    void Fail();
    void FailNow();
    bool Failed();
    void Fatal(params ꓸꓸꓸany argsʗp);
    void Fatalf(@string format, params ꓸꓸꓸany argsʗp);
    void Helper();
    void Log(params ꓸꓸꓸany argsʗp);
    void Logf(@string format, params ꓸꓸꓸany argsʗp);
    @string Name();
    void Parallel();
    void Skip(params ꓸꓸꓸany argsʗp);
    void SkipNow();
    void Skipf(@string format, params ꓸꓸꓸany argsʗp);
    bool Skipped();
}

internal static time.Duration veryLongDuration => /* 1000 * time.Hour */ 3600000000000000; // an arbitrary upper bound on the test's running time

internal static bool contains(map<global::go.context_package.canceler, EmptyStruct> m, global::go.context_package.canceler key) {
    var (_, ret) = m[key, ꟷ];
    return ret;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object keyˢ = (@string)"key"u8;
internal static readonly object valueˢ = (@string)"value"u8;
internal static readonly @string parentˢ = "parent"u8;
internal static readonly @string cancelChildˢ = "cancelChild"u8;
internal static readonly @string valueChildˢ = "valueChild"u8;
internal static readonly @string timerChildˢ = "timerChild"u8;

public static void XTestParentFinishesChild(testingT t) {
    GoFrame ᒐ = default;
    try {
        // Context tree:
        // parent -> cancelChild
        // parent -> valueChild -> timerChild
        // parent -> afterChild
        var (parent, cancel) = WithCancel(Background());
        var (cancelChild, stop) = WithCancel(parent);
        var stopʗ1 = stop;
        defer(() => stopʗ1(), ref ᒐ);
        var valueChild = WithValue(parent, keyˢ, valueˢ);
        (var timerChild, stop) = WithTimeout(valueChild, veryLongDuration);
        var stopʗ2 = stop;
        defer(() => stopʗ2(), ref ᒐ);
        var afterStop = AfterFunc(parent, () => {
        });
        var afterStopʗ1 = afterStop;
        defer(() => afterStopʗ1(), ref ᒐ);
        var selᴛ1 = parent.Done();
        var selᴛ2 = cancelChild.Done();
        var selᴛ3 = timerChild.Done();
        var selᴛ4 = valueChild.Done();
        switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ), ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ), ᐸꟷ(selᴛ4, ꓸꓸꓸ))) {
        case 0 when selᴛ1.ꟷᐳ(out var x): {
            t.Errorf("<-parent.Done() == %v want nothing (it should block)"u8, x);
            break;
        }
        case 1 when selᴛ2.ꟷᐳ(out var x): {
            t.Errorf("<-cancelChild.Done() == %v want nothing (it should block)"u8, x);
            break;
        }
        case 2 when selᴛ3.ꟷᐳ(out var x): {
            t.Errorf("<-timerChild.Done() == %v want nothing (it should block)"u8, x);
            break;
        }
        case 3 when selᴛ4.ꟷᐳ(out var x): {
            t.Errorf("<-valueChild.Done() == %v want nothing (it should block)"u8, x);
            break;
        }
        default: {
            break;
        }}
        // The parent's children should contain the three cancelable children.
        var pc = parent._<ж<global::go.context_package.cancelCtx>>();
        var cc = cancelChild._<ж<global::go.context_package.cancelCtx>>();
        var tc = timerChild._<ж<global::go.context_package.timerCtx>>();
        pc.of(global::go.context_package.cancelCtx.Ꮡmu).Lock();
        ж<global::go.context_package.afterFuncCtx> ac = default!;
        foreach (var (c, _) in (~pc).children) {
            {
                var (a, ok) = c._<ж<global::go.context_package.afterFuncCtx>>(ᐧ); if (ok) {
                    ac = a;
                    break;
                }
            }
        }
        if (len((~pc).children) != 3 || !contains((~pc).children, new global::go.context_package.cancelCtxжcanceler(cc)) || !contains((~pc).children, new global::go.context_package.timerCtxжcanceler(tc)) || ac == nil) {
            t.Errorf("bad linkage: pc.children = %v, want %v, %v, and an afterFunc"u8,
                (~pc).children, cc.OrTypedNil(), tc.OrTypedNil());
        }
        pc.of(global::go.context_package.cancelCtx.Ꮡmu).Unlock();
        {
            var (p, ok) = parentCancelCtx((~cc).Context); if (!ok || p != pc) {
                t.Errorf("bad linkage: parentCancelCtx(cancelChild.Context) = %v, %v want %v, true"u8, p.OrTypedNil(), ok, pc.OrTypedNil());
            }
        }
        {
            var (p, ok) = parentCancelCtx((~tc).Context); if (!ok || p != pc) {
                t.Errorf("bad linkage: parentCancelCtx(timerChild.Context) = %v, %v want %v, true"u8, p.OrTypedNil(), ok, pc.OrTypedNil());
            }
        }
        {
            var (p, ok) = parentCancelCtx((~ac).Context); if (!ok || p != pc) {
                t.Errorf("bad linkage: parentCancelCtx(afterChild.Context) = %v, %v want %v, true"u8, p.OrTypedNil(), ok, pc.OrTypedNil());
            }
        }
        cancel();
        pc.of(global::go.context_package.cancelCtx.Ꮡmu).Lock();
        if (len((~pc).children) != 0) {
            t.Errorf("pc.cancel didn't clear pc.children = %v"u8, (~pc).children);
        }
        pc.of(global::go.context_package.cancelCtx.Ꮡmu).Unlock();
        // parent and children should all be finished.
        void check(global::go.context_package.Context ctx, @string name) {
            var selᴛ5 = ctx.Done();
            switch (trySelect(ᐸꟷ(selᴛ5, ꓸꓸꓸ))) {
            case 0 when selᴛ5.ꟷᐳ(out _): {
                break;
            }
            default: {
                t.Errorf("<-%s.Done() blocked, but shouldn't have"u8, name);
                break;
            }}
            {
                var e = ctx.Err(); if (!AreEqual(e, Canceled)) {
                    t.Errorf("%s.Err() == %v want %v"u8, name, e, Canceled);
                }
            }
        }
        check(parent, parentˢ);
        check(cancelChild, cancelChildˢ);
        check(valueChild, valueChildˢ);
        check(timerChild, timerChildˢ);
        // WithCancel should return a canceled context on a canceled parent.
        var precanceledChild = WithValue(parent, keyˢ, valueˢ);
        var selᴛ6 = precanceledChild.Done();
        switch (trySelect(ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
        case 0 when selᴛ6.ꟷᐳ(out _): {
            break;
        }
        default: {
            t.Errorf("<-precanceledChild.Done() blocked, but shouldn't have"u8);
            break;
        }}
        {
            var e = precanceledChild.Err(); if (!AreEqual(e, Canceled)) {
                t.Errorf("precanceledChild.Err() == %v want %v"u8, e, Canceled);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void XTestChildFinishesFirst(testingT t) {
    GoFrame ᒐ = default;
    try {
        var (cancelable, stop) = WithCancel(Background());
        var stopʗ1 = stop;
        defer(() => stopʗ1(), ref ᒐ);
        foreach (var (_, parent) in new global::go.context_package.Context[]{Background(), cancelable}.slice()) {
            var (child, cancel) = WithCancel(parent);
            var selᴛ7 = parent.Done();
            var selᴛ8 = child.Done();
            switch (trySelect(ᐸꟷ(selᴛ7, ꓸꓸꓸ), ᐸꟷ(selᴛ8, ꓸꓸꓸ))) {
            case 0 when selᴛ7.ꟷᐳ(out var x): {
                t.Errorf("<-parent.Done() == %v want nothing (it should block)"u8, x);
                break;
            }
            case 1 when selᴛ8.ꟷᐳ(out var x): {
                t.Errorf("<-child.Done() == %v want nothing (it should block)"u8, x);
                break;
            }
            default: {
                break;
            }}
            var cc = child._<ж<global::go.context_package.cancelCtx>>();
            var (pc, pcok) = parent._<ж<global::go.context_package.cancelCtx>>(ᐧ); // pcok == false when parent == Background()
            {
                var (p, ok) = parentCancelCtx((~cc).Context); if (ok != pcok || (ok && pc != p)) {
                    t.Errorf("bad linkage: parentCancelCtx(cc.Context) = %v, %v want %v, %v"u8, p.OrTypedNil(), ok, pc.OrTypedNil(), pcok);
                }
            }
            if (pcok) {
                pc.of(global::go.context_package.cancelCtx.Ꮡmu).Lock();
                if (len((~pc).children) != 1 || !contains((~pc).children, new global::go.context_package.cancelCtxжcanceler(cc))) {
                    t.Errorf("bad linkage: pc.children = %v, cc = %v"u8, (~pc).children, cc.OrTypedNil());
                }
                pc.of(global::go.context_package.cancelCtx.Ꮡmu).Unlock();
            }
            cancel();
            if (pcok) {
                pc.of(global::go.context_package.cancelCtx.Ꮡmu).Lock();
                if (len((~pc).children) != 0) {
                    t.Errorf("child's cancel didn't remove self from pc.children = %v"u8, (~pc).children);
                }
                pc.of(global::go.context_package.cancelCtx.Ꮡmu).Unlock();
            }
            // child should be finished.
            var selᴛ9 = child.Done();
            switch (trySelect(ᐸꟷ(selᴛ9, ꓸꓸꓸ))) {
            case 0 when selᴛ9.ꟷᐳ(out _): {
                break;
            }
            default: {
                t.Errorf("<-child.Done() blocked, but shouldn't have"u8);
                break;
            }}
            {
                var e = child.Err(); if (!AreEqual(e, Canceled)) {
                    t.Errorf("child.Err() == %v want %v"u8, e, Canceled);
                }
            }
            // parent should not be finished.
            var selᴛ10 = parent.Done();
            switch (trySelect(ᐸꟷ(selᴛ10, ꓸꓸꓸ))) {
            case 0 when selᴛ10.ꟷᐳ(out var x): {
                t.Errorf("<-parent.Done() == %v want nothing (it should block)"u8, x);
                break;
            }
            default: {
                break;
            }}
            {
                var e = parent.Err(); if (e != default!) {
                    t.Errorf("parent.Err() == %v want nil"u8, e);
                }
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string afterCreationˢ = "after creation"u8;
internal static readonly @string withWithCancelChildˢ = "with WithCancel child "u8;
internal static readonly @string afterCancelingWithCancelˢ = "after canceling WithCancel child"u8;
internal static readonly @string withWithTimeoutChildˢ = "with WithTimeout child "u8;
internal static readonly @string afterCancelingˢ = "after canceling WithTimeout child"u8;
internal static readonly @string withAfterFuncChildˢ = "with AfterFunc child "u8;
internal static readonly @string afterStoppingAfterFuncˢ = "after stopping AfterFunc child "u8;

public static void XTestCancelRemoves(testingT t) {
    void checkChildren(@string when, global::go.context_package.Context ctxΔ1, nint want) {
        {
            nint got = len((~ctxΔ1._<ж<global::go.context_package.cancelCtx>>()).children); if (got != want) {
                t.Errorf("%s: context has %d children, want %d"u8, when, got, want);
            }
        }
    }
    var (ctx, _) = WithCancel(Background());
    checkChildren(afterCreationˢ, ctx, 0);
    var (_, cancel) = WithCancel(ctx);
    checkChildren(withWithCancelChildˢ, ctx, 1);
    cancel();
    checkChildren(afterCancelingWithCancelˢ, ctx, 0);
    (ctx, _) = WithCancel(Background());
    checkChildren(afterCreationˢ, ctx, 0);
    (_, cancel) = WithTimeout(ctx, (time.Duration)(3600000000000L));
    checkChildren(withWithTimeoutChildˢ, ctx, 1);
    cancel();
    checkChildren(afterCancelingˢ, ctx, 0);
    (ctx, _) = WithCancel(Background());
    checkChildren(afterCreationˢ, ctx, 0);
    var stop = AfterFunc(ctx, () => {
    });
    checkChildren(withAfterFuncChildˢ, ctx, 1);
    stop();
    checkChildren(afterStoppingAfterFuncˢ, ctx, 0);
}

internal partial struct myCtx {
    /*embed*/ public global::go.context_package.Context Context;
}

internal partial struct myDoneCtx {
    /*embed*/ public global::go.context_package.Context Context;
}

// Go method set entry for the promoted 'Context.Deadline()' - provided ONLY by the embedded
// interface field in *myDoneCtx's method set; see the pointer-only satisfaction record.
internal static (time.Time, bool) Deadline(this myDoneCtx recvᴛ) => recvᴛ.Context.Deadline();

// Go method set entry for the promoted 'Context.Err()' - provided ONLY by the embedded
// interface field in *myDoneCtx's method set; see the pointer-only satisfaction record.
internal static error Err(this myDoneCtx recvᴛ) => recvᴛ.Context.Err();

// Go method set entry for the promoted 'Context.Value()' - provided ONLY by the embedded
// interface field in *myDoneCtx's method set; see the pointer-only satisfaction record.
internal static any Value(this myDoneCtx recvᴛ, any key) => recvᴛ.Context.Value(key);

internal static /*<-*/channel<EmptyStruct> Done(this ref myDoneCtx d) {
    var c = new channel<EmptyStruct>(0);
    return c.WithDirection(GoChanDir.Recv);
}

public static void XTestCustomContextGoroutines(testingT t) {
    GoFrame ᒐ = default;
    try {
        var g = Ꮡgoroutines.Load();
        void checkNoGoroutine() {
            t.Helper();
            var now = Ꮡgoroutines.Load();
            if (now != g) {
                t.Fatalf("%d goroutines created"u8, now - g);
            }
        }
        void checkCreatedGoroutine() {
            t.Helper();
            var now = Ꮡgoroutines.Load();
            if (now != g + 1) {
                t.Fatalf("%d goroutines created, want 1"u8, now - g);
            }
            g = now;
        }
        var (_, cancel0) = WithCancel(new context_internal_test_package.myDoneCtxжContext(Ꮡ(new myDoneCtx(Background()))));
        cancel0();
        checkCreatedGoroutine();
        (_, cancel0) = WithTimeout(new context_internal_test_package.myDoneCtxжContext(Ꮡ(new myDoneCtx(Background()))), veryLongDuration);
        cancel0();
        checkCreatedGoroutine();
        checkNoGoroutine();
        var checkNoGoroutineʗ1 = checkNoGoroutine;
        defer(checkNoGoroutineʗ1, ref ᒐ);
        var (ctx1, cancel1) = WithCancel(Background());
        var cancel1ʗ1 = cancel1;
        defer(() => cancel1ʗ1(), ref ᒐ);
        checkNoGoroutine();
        var ctx2 = Ꮡ(new myCtx(ctx1));
        var (ctx3, cancel3) = WithCancel(new context_internal_test_package.myCtxжContext(ctx2));
        var cancel3ʗ1 = cancel3;
        defer(() => cancel3ʗ1(), ref ᒐ);
        checkNoGoroutine();
        var (_, cancel3b) = WithCancel(new context_internal_test_package.myDoneCtxжContext(Ꮡ(new myDoneCtx(new context_internal_test_package.myCtxжContext(ctx2)))));
        var cancel3bʗ1 = cancel3b;
        defer(() => cancel3bʗ1(), ref ᒐ);
        checkCreatedGoroutine(); // ctx1 is not providing Done, must not be used
        var (ctx4, cancel4) = WithTimeout(ctx3, veryLongDuration);
        var cancel4ʗ1 = cancel4;
        defer(() => cancel4ʗ1(), ref ᒐ);
        checkNoGoroutine();
        var (ctx5, cancel5) = WithCancel(ctx4);
        var cancel5ʗ1 = cancel5;
        defer(() => cancel5ʗ1(), ref ᒐ);
        checkNoGoroutine();
        cancel5();
        checkNoGoroutine();
        var (_, cancel6) = WithTimeout(ctx5, veryLongDuration);
        var cancel6ʗ1 = cancel6;
        defer(() => cancel6ʗ1(), ref ᒐ);
        checkNoGoroutine();
        // Check applied to canceled context.
        cancel6();
        cancel1();
        var (_, cancel7) = WithCancel(ctx5);
        var cancel7ʗ1 = cancel7;
        defer(() => cancel7ʗ1(), ref ᒐ);
        checkNoGoroutine();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end context_internal_test_package
