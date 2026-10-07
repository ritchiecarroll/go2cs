// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;
using Δnet = net_package;
using Δsync = sync_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static go.context_internal_test_package;

partial class context_test_package {

internal static channel<EmptyStruct> neverReady = new channel<EmptyStruct>(0);    // never closed

// This example demonstrates the use of a cancelable context to prevent a
// goroutine leak. By the end of the example function, the goroutine started
// by gen will return without leaking.
public static void ExampleWithCancel() {
    GoFrame ᒐ = default;
    try {
        // gen generates integers in a separate goroutine and
        // sends them to the returned channel.
        // The callers of gen need to cancel the context once
        // they are done consuming generated integers not to leak
        // the internal goroutine started by gen.
        [MethodImpl(MethodImplOptions.NoInlining)] /*<-*/channel<nint> gen(context.Context ctxΔ1) {
            var dst = new channel<nint>(0);
            nint n = 1;
            var dstʗ1 = dst;
            goǃ(() => {
                while (ᐧ) {
                    var selᴛ3 = ctxΔ1.Done();
                    var selᴛ4 = dstʗ1.ᐸꟷ(n, ꓸꓸꓸ);
                    switch (select(ᐸꟷ(selᴛ3, ꓸꓸꓸ), selᴛ4)) {
                    case 0 when selᴛ3.ꟷᐳ(out _): {
                        return; // returning not to leak the goroutine
                    }
                    case 1: {
                        n++;
                        break;
                    }}
                }
            });
            return dst.WithDirection(GoChanDir.Recv);
        }
        var (ctx, cancel) = context.WithCancel(context.Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ); // cancel when we are finished consuming integers
        foreach (var n in gen(ctx)) {
            fmt.Println(n);
            if (n == 5) {
                break;
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object readyˢ = (@string)"ready"u8;

// Output:
// 1
// 2
// 3
// 4
// 5

// This example passes a context with an arbitrary deadline to tell a blocking
// function that it should abandon its work as soon as it gets to it.
public static void ExampleWithDeadline() {
    GoFrame ᒐ = default;
    try {
        var d = time.Now().Add(shortDuration);
        var (ctx, cancel) = context.WithDeadline(context.Background(), d);
        // Even though ctx will be expired, it is good practice to call its
        // cancellation function in any case. Failure to do so may keep the
        // context and its parent alive longer than necessary.
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var selᴛ5 = neverReady;
        var selᴛ6 = ctx.Done();
        switch (select(ᐸꟷ(selᴛ5, ꓸꓸꓸ), ᐸꟷ(selᴛ6, ꓸꓸꓸ))) {
        case 0 when selᴛ5.ꟷᐳ(out _): {
            fmt.Println(readyˢ);
            break;
        }
        case 1 when selᴛ6.ꟷᐳ(out _): {
            fmt.Println(ctx.Err());
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Output:
// context deadline exceeded

// This example passes a context with a timeout to tell a blocking function that
// it should abandon its work after the timeout elapses.
public static void ExampleWithTimeout() {
    GoFrame ᒐ = default;
    try {
        // Pass a context with a timeout to tell a blocking function that it
        // should abandon its work after the timeout elapses.
        var (ctx, cancel) = context.WithTimeout(context.Background(), shortDuration);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var selᴛ7 = neverReady;
        var selᴛ8 = ctx.Done();
        switch (select(ᐸꟷ(selᴛ7, ꓸꓸꓸ), ᐸꟷ(selᴛ8, ꓸꓸꓸ))) {
        case 0 when selᴛ7.ꟷᐳ(out _): {
            fmt.Println(readyˢ);
            break;
        }
        case 1 when selᴛ8.ꟷᐳ(out _): {
            fmt.Println(ctx.Err()); // prints "context deadline exceeded"
            break;
        }}
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object foundValueˢ = (@string)"found value:"u8;
internal static readonly object keyNotFoundˢ = (@string)"key not found:"u8;
internal static readonly @string languageˢ = "language"u8;
internal static readonly @string colorˢ = "color"u8;

[GoLocalName("favContextKey")] [GoType("@string")] internal partial struct ExampleWithValue_favContextKey;

// Output:
// context deadline exceeded

// This example demonstrates how a value can be passed to the context
// and also how to retrieve it if it exists.
public static void ExampleWithValue() {
    void f(context.Context ctxΔ1, ExampleWithValue_favContextKey kΔ1) {
        {
            var v = ctxΔ1.Value(kΔ1); if (v != default!) {
                fmt.Println(foundValueˢ, v);
                return;
            }
        }
        fmt.Println(keyNotFoundˢ, kΔ1);
    }
    ExampleWithValue_favContextKey k = ((ExampleWithValue_favContextKey)(@string)languageˢ);
    var ctx = context.WithValue(context.Background(), k, (@string)"Go"u8);
    f(ctx, k);
    f(ctx, ((ExampleWithValue_favContextKey)(@string)colorˢ));
}

// Output:
// found value: Go
// key not found: color

// This example uses AfterFunc to define a function which waits on a sync.Cond,
// stopping the wait when a context is canceled.
public static partial void ExampleAfterFunc_cond() {
    error waitOnCond(context.Context ctx, ж<Δsync.Cond> condΔ1, Func<bool> conditionMet) {
        GoFrame ᒐ = default;
        try {
            var stopf = context.AfterFunc(ctx, () => {
                GoFrame ᒐ = default;
                try {
                    // We need to acquire cond.L here to be sure that the Broadcast
                    // below won't occur before the call to Wait, which would result
                    // in a missed signal (and deadlock).
                    (~condΔ1).L.Lock();
                    defer((~condΔ1).L.Unlock, ref ᒐ);
                    // If multiple goroutines are waiting on cond simultaneously,
                    // we need to make sure we wake up exactly this one.
                    // That means that we need to Broadcast to all of the goroutines,
                    // which will wake them all up.
                    //
                    // If there are N concurrent calls to waitOnCond, each of the goroutines
                    // will spuriously wake up O(N) other goroutines that aren't ready yet,
                    // so this will cause the overall CPU cost to be O(N²).
                    condΔ1.Broadcast();
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
            var stopfʗ1 = stopf;
            defer(() => stopfʗ1(), ref ᒐ);
            // Since the wakeups are using Broadcast instead of Signal, this call to
            // Wait may unblock due to some other goroutine's context being canceled,
            // so to be sure that ctx is actually canceled we need to check it in a loop.
            while (!conditionMet()) {
                condΔ1.Wait();
                if (ctx.Err() != default!) {
                    return ctx.Err();
                }
            }
            return default!;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
        finally { ᒐ.Run(); }
    }
    var cond = Δsync.NewCond(new Δsync.MutexжLocker(@new<Δsync.Mutex>()));
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 4; i++) {
        Ꮡwg.Add(1);
        var condʗ1 = cond;
        var waitOnCondʗ1 = waitOnCond;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var (ctx, cancel) = context.WithTimeout(context.Background(), 1 * time.Millisecond);
                var cancelʗ1 = cancel;
                defer(() => cancelʗ1(), ref ᒐ);
                (~condʗ1).L.Lock();
                var condʗ2 = condʗ1;
                defer((~condʗ2).L.Unlock, ref ᒐ);
                var err = waitOnCondʗ1(ctx, condʗ1, () => false);
                fmt.Println(err);
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
    }
    Ꮡwg.Wait();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string tcpˢ = "tcp"u8;
internal static readonly @string localhost0ˢ = "localhost:0"u8;

// Output:
// context deadline exceeded
// context deadline exceeded
// context deadline exceeded
// context deadline exceeded

// This example uses AfterFunc to define a function which reads from a net.Conn,
// stopping the read when a context is canceled.
public static void ExampleAfterFunc_connection() {
    GoFrame ᒐ = default;
    try {
        (nint n, error err) readFromConn(context.Context ctxΔ1, Δnet.Conn connΔ1, slice<byte> bΔ1) {
            nint n = default!;
            error errΔ1 = default!;
            var stopc = new channel<EmptyStruct>(0);
            var stopcʗ1 = stopc;
            var stop = context.AfterFunc(ctxΔ1, () => {
                connΔ1.SetReadDeadline(time.Now());
                close(stopcʗ1);
            });
            (n, errΔ1) = connΔ1.Read(bΔ1);
            if (!stop()) {
                // The AfterFunc was started.
                // Wait for it to complete, and reset the Conn's deadline.
                ᐸꟷ(stopc);
                connΔ1.SetReadDeadline(new time.Time(nil));
                return (n, ctxΔ1.Err());
            }
            return (n, errΔ1);
        }
        var (listener, err) = Δnet.Listen(tcpˢ, localhost0ˢ);
        if (err != default!) {
            fmt.Println(err);
            return;
        }
        var listenerʗ1 = listener;
        defer(() => listenerʗ1.Close(), ref ᒐ);
        (var conn, err) = Δnet.Dial(listener.Addr().Network(), listener.Addr().String());
        if (err != default!) {
            fmt.Println(err);
            return;
        }
        var connʗ1 = conn;
        defer(() => connʗ1.Close(), ref ᒐ);
        var (ctx, cancel) = context.WithTimeout(context.Background(), 1 * time.Millisecond);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var b = new slice<byte>(1024);
        (_, err) = readFromConn(ctx, conn, b);
        fmt.Println(err);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ctx1Canceledˢ = "ctx1 canceled"u8;
internal static readonly @string ctx2Canceledˢ = "ctx2 canceled"u8;

// Output:
// context deadline exceeded

// This example uses AfterFunc to define a function which combines
// the cancellation signals of two Contexts.
public static void ExampleAfterFunc_merge() {
    GoFrame ᒐ = default;
    try {
        // mergeCancel returns a context that contains the values of ctx,
        // and which is canceled when either ctx or cancelCtx is canceled.
        (context.Context, Action) mergeCancel(context.Context ctx, context.Context cancelCtx) {
            (ctx, var cancel) = context.WithCancelCause(ctx);
            var cancelʗ1 = cancel;
            var stop = context.AfterFunc(cancelCtx, () => {
                cancelʗ1(context.Cause(cancelCtx));
            });
            var cancelʗ2 = cancel;
            var stopʗ1 = stop;
            return (ctx, (Action)(() => {
                stopʗ1();
                cancelʗ2(context.Canceled);
            }));
        }
        var (ctx1, cancel1) = context.WithCancelCause(context.Background());
        var cancel1ʗ1 = cancel1;
        defer(cancel1ʗ1, errors.New(ctx1Canceledˢ), ref ᒐ);
        var (ctx2, cancel2) = context.WithCancelCause(context.Background());
        var (mergedCtx, mergedCancel) = mergeCancel(ctx1, ctx2);
        var mergedCancelʗ1 = mergedCancel;
        defer(() => mergedCancelʗ1(), ref ᒐ);
        cancel2(errors.New(ctx2Canceledˢ));
        ᐸꟷ(mergedCtx.Done());
        fmt.Println(context.Cause(mergedCtx));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Output:
// ctx2 canceled

} // end context_test_package
