// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static context_package;
using fmt = fmt_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using context = context_package;
using static go.context_internal_test_package;

partial class context_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string keyˢ = "key"u8;
internal static readonly @string valueˢ = "value"u8;
internal static readonly object shouldNotBeReachedˢ = (@string)"should not be reached"u8;

public static void BenchmarkCommonParentCancel(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        ref var b = ref Ꮡb.DerefOrNull();

        var root = WithValue(Background(), keyˢ, valueˢ);
        var (shared, sharedcancel) = WithCancel(root);
        var sharedcancelʗ1 = sharedcancel;
        defer(() => sharedcancelʗ1(), ref ᒐ);
        b.ResetTimer();
        var sharedʗ1 = shared;
        Ꮡb.RunParallel((ж<testing.PB> pb) => {
            nint x = 0;
            while (pb.Next()) {
                var (ctx, cancel) = WithCancel(sharedʗ1);
                if (ctx.Value(keyˢ)._<@string>() != "value"u8) {
                    Ꮡb.Fatal(shouldNotBeReachedˢ);
                }
                for (nint i = 0; i < 100; i++) {
                    x = quo(x, x + 1);
                }
                cancel();
                for (nint i = 0; i < 100; i++) {
                    x = quo(x, x + 1);
                }
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkWithTimeout(ж<testing.B> Ꮡb) {
    for (nint concurrencyᴛ1 = 40; concurrencyᴛ1 <= 400000; concurrencyᴛ1 *= 100) {
        var concurrency = concurrencyᴛ1;
        @string name = fmt.Sprintf("concurrency=%d"u8, concurrency);
        Ꮡb.Run(name, [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ1) => {
            benchmarkWithTimeout(bΔ1, concurrency);
        });
    }
}

internal static partial void benchmarkWithTimeout(ж<testing.B> Ꮡb, nint concurrentContexts) {
    ref var b = ref Ꮡb.DerefOrNull();

    nint gomaxprocs = Δruntime.GOMAXPROCS(0);
    nint perPContexts = quo(concurrentContexts, gomaxprocs);
    var root = Background();
    // Generate concurrent contexts.
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    var ccf = new slice<slice<Action>>(gomaxprocs);
    foreach (var (i, _) in ccf) {
        Ꮡwg.Add(1);
        var ccfʗ1 = ccf;
        var rootʗ1 = root;
        goǃ((nint iΔ1) => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                var cf = new slice<Action>(perPContexts);
                foreach (var (j, _) in cf) {
                    (_, cf[j]) = WithTimeout(rootʗ1, time.ΔHour);
                }
                ccfʗ1[iΔ1] = cf;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, i);
    }
    Ꮡwg.Wait();
    b.ResetTimer();
    var rootʗ2 = root;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var wcf = new slice<Action>(10);
        while (pb.Next()) {
            foreach (var (i, _) in wcf) {
                (_, wcf[i]) = WithTimeout(rootʗ2, time.ΔHour);
            }
            foreach (var (_, f) in wcf) {
                f();
            }
        }
    });
    b.StopTimer();
    foreach (var (_, cf) in ccf) {
        foreach (var (_, f) in cf) {
            f();
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string rootBackgroundˢ = "Root=Background"u8;
internal static readonly @string rootOpenCancelerˢ = "Root=OpenCanceler"u8;
internal static readonly @string rootClosedCancelerˢ = "Root=ClosedCanceler"u8;

public static void BenchmarkCancelTree(ж<testing.B> Ꮡb) {
    var depths = new nint[]{1, 10, 100, 1000}.slice();
    foreach (var (_, d) in depths) {
        Ꮡb.Run(fmt.Sprintf("depth=%d"u8, d), (ж<testing.B> bΔ1) => {
            bΔ1.Run(rootBackgroundˢ, (ж<testing.B> bΔ2) => {
                for (nint i = 0; i < (~bΔ2).N; i++) {
                    buildContextTree(Background(), d);
                }
            });
            bΔ1.Run(rootOpenCancelerˢ, (ж<testing.B> bΔ3) => {
                for (nint i = 0; i < (~bΔ3).N; i++) {
                    var (ctx, cancel) = WithCancel(Background());
                    buildContextTree(ctx, d);
                    cancel();
                }
            });
            bΔ1.Run(rootClosedCancelerˢ, (ж<testing.B> bΔ4) => {
                for (nint i = 0; i < (~bΔ4).N; i++) {
                    var (ctx, cancel) = WithCancel(Background());
                    cancel();
                    buildContextTree(ctx, d);
                }
            });
        });
    }
}

internal static void buildContextTree(context.Context root, nint depth) {
    for (nint d = 0; d < depth; d++) {
        (root, _) = WithCancel(root);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string errˢ = "Err"u8;
internal static readonly @string doneˢ = "Done"u8;

public static void BenchmarkCheckCanceled(ж<testing.B> Ꮡb) {
    var (ctx, cancel) = WithCancel(Background());
    cancel();
    var ctxʗ1 = ctx;
    Ꮡb.Run(errˢ, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            ctxʗ1.Err();
        }
    });
    var ctxʗ2 = ctx;
    Ꮡb.Run(doneˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            var selᴛ1 = ctxʗ2.Done();
            switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
            case 0 when selᴛ1.ꟷᐳ(out _): {
                break;
            }
            default: {
                break;
            }}
        }
    });
}

public static void BenchmarkContextCancelDone(ж<testing.B> Ꮡb) {
    GoFrame ᒐ = default;
    try {
        var (ctx, cancel) = WithCancel(Background());
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        var ctxʗ1 = ctx;
        Ꮡb.RunParallel((ж<testing.PB> pb) => {
            while (pb.Next()) {
                var selᴛ2 = ctxʗ1.Done();
                switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
                case 0 when selᴛ2.ꟷᐳ(out _): {
                    break;
                }
                default: {
                    break;
                }}
            }
        });
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void BenchmarkDeepValueNewGoRoutine(ж<testing.B> Ꮡb) {
    foreach (var (_, depth) in new nint[]{10, 20, 30, 50, 100}.slice()) {
        ref var ctx = ref heap<context.Context>(out var Ꮡctx);
        ctx = Background();
        for (nint i = 0; i < depth; i++) {
            ctx = WithValue(ctx, i, i);
        }
        Ꮡb.Run(fmt.Sprintf("depth=%d"u8, depth), [MethodImpl(MethodImplOptions.NoInlining)] (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
                Ꮡwg.Add(1);
                goǃ(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(Ꮡwg.Done, ref ᒐ);
                        Ꮡctx.ValueSlot.Value((nint)(-1));
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                });
                Ꮡwg.Wait();
            }
        });
    }
}

public static void BenchmarkDeepValueSameGoRoutine(ж<testing.B> Ꮡb) {
    foreach (var (_, depth) in new nint[]{10, 20, 30, 50, 100}.slice()) {
        ref var ctx = ref heap<context.Context>(out var Ꮡctx);
        ctx = Background();
        for (nint i = 0; i < depth; i++) {
            ctx = WithValue(ctx, i, i);
        }
        Ꮡb.Run(fmt.Sprintf("depth=%d"u8, depth), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                Ꮡctx.ValueSlot.Value((nint)(-1));
            }
        });
    }
}

} // end context_test_package
