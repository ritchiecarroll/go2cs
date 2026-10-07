// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.context_internal_test_package;

partial class context_test_package {

// afterFuncContext is a context that's not one of the types
// defined in context.go, that supports registering AfterFuncs.
[GoType] partial struct afterFuncContext {
    internal Δsync.Mutex mu;
    internal map<ж<byte>, Action> afterFuncs;
    internal channel<EmptyStruct> done;
    internal error err;
}

internal static context.Context newAfterFuncContext() {
    return new context_test_package.afterFuncContextжContext(Ꮡ(new afterFuncContext(nil)));
}

[GoRecv] internal static (time.Time, bool) Deadline(this ref afterFuncContext c) {
    return (new time.Time(nil), false);
}

internal static /*<-*/channel<EmptyStruct> Done(this ж<afterFuncContext> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        ᒐd1 = true;
        if (c.done == default!) {
            c.done = new channel<EmptyStruct>(0);
        }
        return c.done.WithDirection(GoChanDir.Recv);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

internal static error Err(this ж<afterFuncContext> Ꮡc) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        ᒐd1 = true;
        return c.err;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

[GoRecv] internal static any Value(this ref afterFuncContext c, any key) {
    return default!;
}

internal static Func<bool> ΔAfterFunc(this ж<afterFuncContext> Ꮡc, Action f) {
    GoFrame ᒐ = default;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        defer(Ꮡc.of(afterFuncContext.Ꮡmu).Unlock, ref ᒐ);
        var k = @new<byte>();
        if (c.afterFuncs == default!) {
            c.afterFuncs = new map<ж<byte>, Action>();
        }
        c.afterFuncs[k] = f;
        var kʗ1 = k;
        return () => {
            GoFrame ᒐ = default;
            try {
                Ꮡc.Value.mu.Lock();
                defer(Ꮡc.of(afterFuncContext.Ꮡmu).Unlock, ref ᒐ);
                var (_, ok) = Ꮡc.Value.afterFuncs[kʗ1, ꟷ];
                delete(Ꮡc.Value.afterFuncs, kʗ1);
                return ok;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
            finally { ᒐ.Run(); }
        };
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

internal static partial void cancel(this ж<afterFuncContext> Ꮡc, error err) {
    GoFrame ᒐ = default;
    bool ᒐd1 = false;
    try {
        ref var c = ref Ꮡc.DerefOrNull();

        c.mu.Lock();
        ᒐd1 = true;
        if (c.err != default!) {
            return;
        }
        c.err = err;
        foreach (var (_, f) in c.afterFuncs) {
            var fʗ1 = f;
            goǃ(fʗ1);
        }
        c.afterFuncs = default!;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { if (ᒐd1) Ꮡc.DerefOrNull().mu.Unlock(); ᒐ.Run(); }
}

public static void TestCustomContextAfterFuncCancel(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ctx0 = Ꮡ(new afterFuncContext(nil));
        var (ctx1, cancel) = context.WithCancel(new context_test_package.afterFuncContextжContext(ctx0));
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        ctx0.cancel(context.Canceled);
        ᐸꟷ(ctx1.Done());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestCustomContextAfterFuncTimeout(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ctx0 = Ꮡ(new afterFuncContext(nil));
        var (ctx1, cancel) = context.WithTimeout(new context_test_package.afterFuncContextжContext(ctx0), veryLongDuration);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        ctx0.cancel(context.Canceled);
        ᐸꟷ(ctx1.Done());
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestCustomContextAfterFuncAfterFunc(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        var ctx0 = Ꮡ(new afterFuncContext(nil));
        var donec = new channel<EmptyStruct>(0);
        var donecʗ1 = donec;
        var stop = context.AfterFunc(new context_test_package.afterFuncContextжContext(ctx0), () => {
            close(donecʗ1);
        });
        var stopʗ1 = stop;
        defer(() => stopʗ1(), ref ᒐ);
        ctx0.cancel(context.Canceled);
        ᐸꟷ(donec);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestCustomContextAfterFuncUnregisterCancel(ж<testing.T> Ꮡt) {
    var ctx0 = Ꮡ(new afterFuncContext(nil));
    var (_, cancel1) = context.WithCancel(new context_test_package.afterFuncContextжContext(ctx0));
    var (_, cancel2) = context.WithCancel(new context_test_package.afterFuncContextжContext(ctx0));
    {
        nint got = len((~ctx0).afterFuncs);
        nint want = 2; if (got != want) {
            Ꮡt.Errorf("after WithCancel(ctx0): ctx0 has %v afterFuncs, want %v"u8, got, want);
        }
    }
    cancel1();
    cancel2();
    {
        nint got = len((~ctx0).afterFuncs);
        nint want = 0; if (got != want) {
            Ꮡt.Errorf("after canceling WithCancel(ctx0): ctx0 has %v afterFuncs, want %v"u8, got, want);
        }
    }
}

public static void TestCustomContextAfterFuncUnregisterTimeout(ж<testing.T> Ꮡt) {
    var ctx0 = Ꮡ(new afterFuncContext(nil));
    var (_, cancel) = context.WithTimeout(new context_test_package.afterFuncContextжContext(ctx0), veryLongDuration);
    {
        nint got = len((~ctx0).afterFuncs);
        nint want = 1; if (got != want) {
            Ꮡt.Errorf("after WithTimeout(ctx0, d): ctx0 has %v afterFuncs, want %v"u8, got, want);
        }
    }
    cancel();
    {
        nint got = len((~ctx0).afterFuncs);
        nint want = 0; if (got != want) {
            Ꮡt.Errorf("after canceling WithTimeout(ctx0, d): ctx0 has %v afterFuncs, want %v"u8, got, want);
        }
    }
}

public static void TestCustomContextAfterFuncUnregisterAfterFunc(ж<testing.T> Ꮡt) {
    var ctx0 = Ꮡ(new afterFuncContext(nil));
    var stop = context.AfterFunc(new context_test_package.afterFuncContextжContext(ctx0), () => {
    });
    {
        nint got = len((~ctx0).afterFuncs);
        nint want = 1; if (got != want) {
            Ꮡt.Errorf("after AfterFunc(ctx0, f): ctx0 has %v afterFuncs, want %v"u8, got, want);
        }
    }
    stop();
    {
        nint got = len((~ctx0).afterFuncs);
        nint want = 0; if (got != want) {
            Ꮡt.Errorf("after stopping AfterFunc(ctx0, f): ctx0 has %v afterFuncs, want %v"u8, got, want);
        }
    }
}

} // end context_test_package
