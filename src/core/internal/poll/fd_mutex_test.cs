// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

using static go.@internal.poll_package;
using rand = math.rand_package;
using Δruntime = runtime_package;
using strings = strings_package;
using testing = testing_package;
using time = time_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.@internal;
using math;
using poll = go.@internal.poll_package;
using static go.@internal.poll_internal_test_package;

partial class poll_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object brokenˢ = (@string)"broken"u8;

public static void TestMutexLock(ж<testing.T> Ꮡt) {
    ref var mu = ref heap(new global::go.@internal.poll_internal_test_package.XFDMutex(), out var Ꮡmu);
    if (!Ꮡmu.Incref()) {
        Ꮡt.Fatal(brokenˢ);
    }
    if (Ꮡmu.Decref()) {
        Ꮡt.Fatal(brokenˢ);
    }
    if (!Ꮡmu.RWLock(true)) {
        Ꮡt.Fatal(brokenˢ);
    }
    if (Ꮡmu.RWUnlock(true)) {
        Ꮡt.Fatal(brokenˢ);
    }
    if (!Ꮡmu.RWLock(false)) {
        Ꮡt.Fatal(brokenˢ);
    }
    if (Ꮡmu.RWUnlock(false)) {
        Ꮡt.Fatal(brokenˢ);
    }
}

public static void TestMutexClose(ж<testing.T> Ꮡt) {
    ref var mu = ref heap(new global::go.@internal.poll_internal_test_package.XFDMutex(), out var Ꮡmu);
    if (!Ꮡmu.IncrefAndClose()) {
        Ꮡt.Fatal(brokenˢ);
    }
    if (Ꮡmu.Incref()) {
        Ꮡt.Fatal(brokenˢ);
    }
    if (Ꮡmu.RWLock(true)) {
        Ꮡt.Fatal(brokenˢ);
    }
    if (Ꮡmu.RWLock(false)) {
        Ꮡt.Fatal(brokenˢ);
    }
    if (Ꮡmu.IncrefAndClose()) {
        Ꮡt.Fatal(brokenˢ);
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestMutexCloseUnblock(ж<testing.T> Ꮡt) {
    var c = new channel<bool>(4);
    ref var mu = ref heap(new global::go.@internal.poll_internal_test_package.XFDMutex(), out var Ꮡmu);
    Ꮡmu.RWLock(true);
    for (nint i = 0; i < 4; i++) {
        var cʗ1 = c;
        goǃ(() => {
            if (Ꮡmu.RWLock(true)) {
                Ꮡt.Error(brokenˢ);
                return;
            }
            cʗ1.ᐸꟷ(true);
        });
    }
    // Concurrent goroutines must not be able to read lock the mutex.
    time.Sleep(time.Millisecond);
    var selᴛ1 = c;
    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out _): {
        Ꮡt.Fatal(brokenˢ);
        break;
    }
    default: {
        break;
    }}
    Ꮡmu.IncrefAndClose(); // Must unblock the readers.
    for (nint i = 0; i < 4; i++) {
        var selᴛ2 = c;
        var selᴛ3 = time.After((time.Duration)(10000000000L));
        switch (select(ᐸꟷ(selᴛ2, ꓸꓸꓸ), ᐸꟷ(selᴛ3, ꓸꓸꓸ))) {
        case 0 when selᴛ2.ꟷᐳ(out _): {
            break;
        }
        case 1 when selᴛ3.ꟷᐳ(out _): {
            Ꮡt.Fatal(brokenˢ);
            break;
        }}
    }
    if (Ꮡmu.Decref()) {
        Ꮡt.Fatal(brokenˢ);
    }
    if (!Ꮡmu.RWUnlock(true)) {
        Ꮡt.Fatal(brokenˢ);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object doesNotPanicˢ = (@string)"does not panic"u8;

public static void TestMutexPanic(ж<testing.T> Ꮡt) {
    void ensurePanics(Action f) {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                if (recover() == default!) {
                    Ꮡt.Fatal(doesNotPanicˢ);
                }
            }, ref ᒐ);
            f();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }
    ref var mu = ref heap(new global::go.@internal.poll_internal_test_package.XFDMutex(), out var Ꮡmu);
    ensurePanics(() => {
        Ꮡmu.Decref();
    });
    ensurePanics(() => {
        Ꮡmu.RWUnlock(true);
    });
    ensurePanics(() => {
        Ꮡmu.RWUnlock(false);
    });
    ensurePanics(() => {
        Ꮡmu.Incref();
        Ꮡmu.Decref();
        Ꮡmu.Decref();
    });
    ensurePanics(() => {
        Ꮡmu.RWLock(true);
        Ꮡmu.RWUnlock(true);
        Ꮡmu.RWUnlock(true);
    });
    ensurePanics(() => {
        Ꮡmu.RWLock(false);
        Ꮡmu.RWUnlock(false);
        Ꮡmu.RWUnlock(false);
    });
    // ensure that it's still not broken
    Ꮡmu.Incref();
    Ꮡmu.Decref();
    Ꮡmu.RWLock(true);
    Ꮡmu.RWUnlock(true);
    Ꮡmu.RWLock(false);
    Ꮡmu.RWUnlock(false);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object didNotPanicˢ = (@string)"did not panic"u8;
internal static readonly @string tooManyˢ = "too many"u8;
internal static readonly @string inconsistentˢ = "inconsistent"u8;

public static void TestMutexOverflowPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            var r = recover();
            if (r == default!) {
                Ꮡt.Fatal(didNotPanicˢ);
            }
            var (msg, ok) = r._<@string>(ᐧ);
            if (!ok) {
                Ꮡt.Fatalf("unexpected panic type %T"u8, r);
            }
            if (!strings.Contains(msg, tooManyˢ) || strings.Contains(msg, inconsistentˢ)) {
                Ꮡt.Fatalf("wrong panic message %q"u8, msg);
            }
        }, ref ᒐ);
        ref var mu1 = ref heap(new global::go.@internal.poll_internal_test_package.XFDMutex(), out var Ꮡmu1);
        for (nint i = 0; i < (1 << (int)(21)); i++) {
            Ꮡmu1.Incref();
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestMutexStress(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        nint P = 8;
        nint N = (nint)1000000;
        if (testing.Short()) {
            P = 4;
            N = 10000;
        }
        defer(Δruntime.GOMAXPROCS, Δruntime.GOMAXPROCS(P), ref ᒐ);
        var done = new channel<bool>(P);
        ref var mu = ref heap(new global::go.@internal.poll_internal_test_package.XFDMutex(), out var Ꮡmu);
        ref var readState = ref heap(new array<uint64>(2), out var ᏑreadState);
        ref var writeState = ref heap(new array<uint64>(2), out var ᏑwriteState);
        for (nint p = 0; p < P; p++) {
            var doneʗ1 = done;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    var doneʗ2 = doneʗ1;
                    defer(() => {
                        doneʗ2.ᐸꟷ(!Ꮡt.Failed());
                    }, ref ᒐ);
                    var r = rand.New(rand.NewSource(rand.Int63()));
                    for (nint i = 0; i < N; i++) {
                        switch (r.Intn(3)) {
                        case 0: {
                            if (!Ꮡmu.Incref()) {
                                Ꮡt.Error(brokenˢ);
                                return;
                            }
                            if (Ꮡmu.Decref()) {
                                Ꮡt.Error(brokenˢ);
                                return;
                            }
                            break;
                        }
                        case 1: {
                            if (!Ꮡmu.RWLock(true)) {
                                Ꮡt.Error(brokenˢ);
                                return;
                            }
                            if (ᏑreadState.Value[0] != ᏑreadState.Value[1]) {
                                // Ensure that it provides mutual exclusion for readers.
                                Ꮡt.Error(brokenˢ);
                                return;
                            }
                            ᏑreadState.Value[0]++;
                            ᏑreadState.Value[1]++;
                            if (Ꮡmu.RWUnlock(true)) {
                                Ꮡt.Error(brokenˢ);
                                return;
                            }
                            break;
                        }
                        case 2: {
                            if (!Ꮡmu.RWLock(false)) {
                                Ꮡt.Error(brokenˢ);
                                return;
                            }
                            if (ᏑwriteState.Value[0] != ᏑwriteState.Value[1]) {
                                // Ensure that it provides mutual exclusion for writers.
                                Ꮡt.Error(brokenˢ);
                                return;
                            }
                            ᏑwriteState.Value[0]++;
                            ᏑwriteState.Value[1]++;
                            if (Ꮡmu.RWUnlock(false)) {
                                Ꮡt.Error(brokenˢ);
                                return;
                            }
                            break;
                        }}

                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        for (nint p = 0; p < P; p++) {
            if (!ᐸꟷ(done)) {
                Ꮡt.FailNow();
            }
        }
        if (!Ꮡmu.IncrefAndClose()) {
            Ꮡt.Fatal(brokenˢ);
        }
        if (!Ꮡmu.Decref()) {
            Ꮡt.Fatal(brokenˢ);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end poll_test_package
