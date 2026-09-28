// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using bytes = bytes_package;
using Δmath = math_package;
using Δruntime = runtime_package;
using debug = go.runtime.debug_package;
using Δsync = sync_package;
using atomic = go.sync.atomic_package;
using Δtesting = testing_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using go.runtime;
using go.sync;
using static go.sync_internal_test_package;

partial class sync_test_package {

// We assume that the Once.Do tests have already covered parallelism.
public static void TestOnceFunc(ж<Δtesting.T> Ꮡt) {
    nint calls = 0;
    var f = Δsync.OnceFunc(() => {
        calls++;
    });
    var allocs = Δtesting.AllocsPerRun(10, f);
    if (calls != 1) {
        Ꮡt.Errorf("want calls==1, got %d"u8, calls);
    }
    if (allocs != 0D) {
        Ꮡt.Errorf("want 0 allocations per call, got %v"u8, allocs);
    }
}

public static void TestOnceValue(ж<Δtesting.T> Ꮡt) {
    nint calls = 0;
    var f = Δsync.OnceValue(nint () => {
        calls++;
        return calls;
    });
    var fʗ1 = f;
    var allocs = Δtesting.AllocsPerRun(10, () => {
        fʗ1();
    });
    nint value = f();
    if (calls != 1) {
        Ꮡt.Errorf("want calls==1, got %d"u8, calls);
    }
    if (value != 1) {
        Ꮡt.Errorf("want value==1, got %d"u8, value);
    }
    if (allocs != 0D) {
        Ꮡt.Errorf("want 0 allocations per call, got %v"u8, allocs);
    }
}

public static void TestOnceValues(ж<Δtesting.T> Ꮡt) {
    nint calls = 0;
    var f = Δsync.OnceValues((nint, nint) () => {
        calls++;
        return (calls, calls + 1);
    });
    var fʗ1 = f;
    var allocs = Δtesting.AllocsPerRun(10, () => {
        fʗ1();
    });
    var (v1, v2) = f();
    if (calls != 1) {
        Ꮡt.Errorf("want calls==1, got %d"u8, calls);
    }
    if (v1 != 1 || v2 != 2) {
        Ꮡt.Errorf("want v1==1 and v2==2, got %d and %d"u8, v1, v2);
    }
    if (allocs != 0D) {
        Ꮡt.Errorf("want 0 allocations per call, got %v"u8, allocs);
    }
}

internal static void testOncePanicX(ж<Δtesting.T> Ꮡt, ж<nint> Ꮡcalls, Action f) {
    testOncePanicWith(Ꮡt, Ꮡcalls, f, (@string label, any p) => {
        if (!AreEqual(p, (@string)("x"))) {
            Ꮡt.Fatalf("%s: want panic %v, got %v"u8, label, (@string)"x"u8, p);
        }
    });
}

internal static void testOncePanicWith(ж<Δtesting.T> Ꮡt, ж<nint> Ꮡcalls, Action f, Action<@string, any> check) {
    ref var calls = ref Ꮡcalls.DerefOrNull();

    // Check that the each call to f panics with the same value, but the
    // underlying function is only called once.
    foreach (var (_, label) in new @string[]{"first time"u8, "second time"u8}.slice()) {
        ref var p = ref heap<any>(out var Ꮡp);
        var panicked = true;
        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    Ꮡp.ValueSlot = recover();
                }, ref ᒐ);
                f();
                panicked = false;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();
        if (!panicked) {
            Ꮡt.Fatalf("%s: f did not panic"u8, label);
        }
        check(label, p);
    }
    if (calls != 1) {
        Ꮡt.Errorf("want calls==1, got %d"u8, calls);
    }
}

public static void TestOnceFuncPanic(ж<Δtesting.T> Ꮡt) {
    ref var calls = ref heap<nint>(out var Ꮡcalls);
    calls = 0;
    var f = Δsync.OnceFunc(() => {
        Ꮡcalls.Value++;
        throw panic("x");
    });
    testOncePanicX(Ꮡt, Ꮡcalls, f);
}

public static void TestOnceValuePanic(ж<Δtesting.T> Ꮡt) {
    ref var calls = ref heap<nint>(out var Ꮡcalls);
    calls = 0;
    var f = Δsync.OnceValue(nint () => {
        Ꮡcalls.Value++;
        throw panic("x");
    });
    var fʗ1 = f;
    testOncePanicX(Ꮡt, Ꮡcalls, () => {
        fʗ1();
    });
}

public static void TestOnceValuesPanic(ж<Δtesting.T> Ꮡt) {
    ref var calls = ref heap<nint>(out var Ꮡcalls);
    calls = 0;
    var f = Δsync.OnceValues((nint, nint) () => {
        Ꮡcalls.Value++;
        throw panic("x");
    });
    var fʗ1 = f;
    testOncePanicX(Ꮡt, Ꮡcalls, () => {
        fʗ1();
    });
}

public static void TestOnceFuncPanicNil(ж<Δtesting.T> Ꮡt) {
    ref var calls = ref heap<nint>(out var Ꮡcalls);
    calls = 0;
    var f = Δsync.OnceFunc(() => {
        Ꮡcalls.Value++;
        throw panic(default!);
    });
    testOncePanicWith(Ꮡt, Ꮡcalls, f, (@string label, any p) => {
        switch (p.type()) {
        case null:
        case ж<Δruntime.PanicNilError> _: {
            return;
        }}

        Ꮡt.Fatalf("%s: want nil panic, got %v"u8, label, p);
    });
}

public static void TestOnceFuncGoexit(ж<Δtesting.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // If f calls Goexit, the results are unspecified. But check that f doesn't
    // get called twice.
    nint calls = 0;
    var f = Δsync.OnceFunc(() => {
        calls++;
        Δruntime.Goexit();
    });
    ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
    for (nint i = 0; i < 2; i++) {
        Ꮡwg.Add(1);
        var fʗ1 = f;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                defer(() => {
                    recover();
                }, ref ᒐ);
                fʗ1();
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        Ꮡwg.Wait();
    }
    if (calls != 1) {
        Ꮡt.Errorf("want calls==1, got %d"u8, calls);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string syncTestOnceFuncPanicˢ = "sync_test.onceFuncPanic"u8;

public static void TestOnceFuncPanicTraceback(ж<Δtesting.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // Test that on the first invocation of a OnceFunc, the stack trace goes all
        // the way to the origin of the panic.
        var f = Δsync.OnceFunc(onceFuncPanic);
        defer(() => {
            {
                var p = recover(); if (!AreEqual(p, (@string)("x"))) {
                    Ꮡt.Fatalf("want panic %v, got %v"u8, (@string)"x"u8, p);
                }
            }
            var stack = debug.Stack();
            @string want = syncTestOnceFuncPanicˢ;
            if (!bytes.Contains(stack, slice<byte>(want))) {
                Ꮡt.Fatalf("want stack containing %v, got:\n%s"u8, want, ((@string)stack));
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static void onceFuncPanic() {
    throw panic("x");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object wrappedFunctionGarbageˢ = (@string)"wrapped function garbage collected too early"u8;
internal static readonly object wrappedFunctionShouldBeˢ = (@string)"wrapped function should be garbage collected, but still live"u8;

public static void TestOnceXGC(ж<Δtesting.T> Ꮡt) {

    var fns = new map<@string, Func<slice<byte>, Action>>{
        ["OnceFunc"u8] = (slice<byte> buf) => {
            var bufʗ1 = buf;
            return Δsync.OnceFunc(() => {
                bufʗ1[0] = 1;
            });
        },
        ["OnceValue"u8] = (slice<byte> buf) => {
            var bufʗ2 = buf;
            var f = Δsync.OnceValue(any () => {
                bufʗ2[0] = 1;
                return default!;
            });
            var fʗ1 = f;
            return () => {
                fʗ1();
            };
        },
        ["OnceValues"u8] = (slice<byte> buf) => {
            var bufʗ3 = buf;
            var f = Δsync.OnceValues((any, any) () => {
                bufʗ3[0] = 1;
                return (default!, default!);
            });
            var fʗ2 = f;
            return () => {
                fʗ2();
            };
        }
    };
    foreach (var (n, fn) in fns) {
        var fnʗ1 = fn;
        Ꮡt.Run(n, (ж<Δtesting.T> tΔ1) => {
            var buf = new slice<byte>(1024);
            ref var gc = ref heap(new atomic.Bool(), out var Ꮡgc);
            Δruntime.SetFinalizer(Ꮡ(buf, 0), (ж<byte> _) => {
                Ꮡgc.Store(true);
            });
            var f = fnʗ1(buf);
            gcwaitfin();
            if (Ꮡgc.Load() != false) {
                tΔ1.Fatal(wrappedFunctionGarbageˢ);
            }
            f();
            gcwaitfin();
            if (Ꮡgc.Load() != true) {
                // Even if f is still alive, the function passed to Once(Func|Value|Values)
                // is not kept alive after the first call to f.
                tΔ1.Fatal(wrappedFunctionShouldBeˢ);
            }
            f();
        });
    }
}

// gcwaitfin performs garbage collection and waits for all finalizers to run.
internal static void gcwaitfin() {
    Δruntime.GC();
    runtime_blockUntilEmptyFinalizerQueue(Δmath.MaxInt64);
}

//go:linkname runtime_blockUntilEmptyFinalizerQueue runtime.blockUntilEmptyFinalizerQueue
[global::System.Diagnostics.StackTraceHidden] internal static bool runtime_blockUntilEmptyFinalizerQueue(int64 _) {
    return Δruntime.blockUntilEmptyFinalizerQueue(_);
}

internal static Action onceFunc = Δsync.OnceFunc(() => {
});
internal static ж<Δsync.Once> ᏑonceFuncOnce = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once onceFuncOnce => ref ᏑonceFuncOnce.Value;

internal static void doOnceFunc() {
    ᏑonceFuncOnce.Do(() => {
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string vOnceˢ = "v=Once"u8;
internal static readonly @string vGlobalˢ = "v=Global"u8;
internal static readonly @string vLocalˢ = "v=Local"u8;

public static void BenchmarkOnceFunc(ж<Δtesting.B> Ꮡb) {
    Ꮡb.Run(vOnceˢ, (ж<Δtesting.B> bΔ1) => {
        bΔ1.ReportAllocs();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            // The baseline is direct use of sync.Once.
            doOnceFunc();
        }
    });
    Ꮡb.Run(vGlobalˢ, (ж<Δtesting.B> bΔ2) => {
        bΔ2.ReportAllocs();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            // As of 3/2023, the compiler doesn't recognize that onceFunc is
            // never mutated and is a closure that could be inlined.
            // Too bad, because this is how OnceFunc will usually be used.
            onceFunc();
        }
    });
    Ꮡb.Run(vLocalˢ, (ж<Δtesting.B> bΔ3) => {
        bΔ3.ReportAllocs();
        // As of 3/2023, the compiler *does* recognize this local binding as an
        // inlinable closure. This is the best case for OnceFunc, but probably
        // not typical usage.
        var f = Δsync.OnceFunc(() => {
        });
        for (nint i = 0; i < (~bΔ3).N; i++) {
            f();
        }
    });
}

internal static Func<nint> onceValue = Δsync.OnceValue(nint () => 42);
internal static ж<Δsync.Once> ᏑonceValueOnce = new StandardBox<Δsync.Once>(default(Δsync.Once));
internal static ref Δsync.Once onceValueOnce => ref ᏑonceValueOnce.Value;
internal static nint onceValueValue;

internal static nint doOnceValue() {
    ᏑonceValueOnce.Do(() => {
        onceValueValue = 42;
    });
    return onceValueValue;
}

public static void BenchmarkOnceValue(ж<Δtesting.B> Ꮡb) {
    // See BenchmarkOnceFunc
    Ꮡb.Run(vOnceˢ, (ж<Δtesting.B> bΔ1) => {
        bΔ1.ReportAllocs();
        for (nint i = 0; i < (~bΔ1).N; i++) {
            {
                nint want = 42;
                nint got = doOnceValue(); if (want != got) {
                    bΔ1.Fatalf("want %d, got %d"u8, want, got);
                }
            }
        }
    });
    Ꮡb.Run(vGlobalˢ, (ж<Δtesting.B> bΔ2) => {
        bΔ2.ReportAllocs();
        for (nint i = 0; i < (~bΔ2).N; i++) {
            {
                nint want = 42;
                nint got = onceValue(); if (want != got) {
                    bΔ2.Fatalf("want %d, got %d"u8, want, got);
                }
            }
        }
    });
    Ꮡb.Run(vLocalˢ, (ж<Δtesting.B> bΔ3) => {
        bΔ3.ReportAllocs();
        var onceValueΔ1 = Δsync.OnceValue(nint () => 42);
        for (nint i = 0; i < (~bΔ3).N; i++) {
            {
                nint want = 42;
                nint got = onceValueΔ1(); if (want != got) {
                    bΔ3.Fatalf("want %d, got %d"u8, want, got);
                }
            }
        }
    });
}

} // end sync_test_package
