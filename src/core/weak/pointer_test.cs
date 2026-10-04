// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using context = context_package;
using goarch = @internal.goarch_package;
using Δruntime = runtime_package;
using Δsync = sync_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using weak = weak_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;

partial class weak_test_package {

[GoType] partial struct T {
    // N.B. This must contain a pointer, otherwise the weak handle might get placed
    // in a tiny block making the tests in this package flaky.
    internal ж<T> t;
    internal nint a;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object valueOfZeroValueOfWeakˢ = (@string)"Value of zero value of weak.Pointer is not nil"u8;
private static readonly object valueOfWeakMakeTNilIsNotˢ = (@string)"Value of weak.Make[T](nil) is not nil"u8;

public static void TestPointer(ж<testing.T> Ꮡt) {
    weak.Pointer<T> zero = default!;
    if (zero.Value() != nil) {
        Ꮡt.Error(valueOfZeroValueOfWeakˢ);
    }
    var zeroNil = weak.Make<T>(nil);
    if (zeroNil.Value() != nil) {
        Ꮡt.Error(valueOfWeakMakeTNilIsNotˢ);
    }
    var bt = @new<T>();
    var wt = weak.Make<T>(bt);
    {
        var st = wt.Value(); if (st != bt) {
            Ꮡt.Fatalf("weak pointer is not the same as strong pointer: %p vs. %p"u8, st.OrTypedNil(), bt.OrTypedNil());
        }
    }
    // bt is still referenced.
    Δruntime.GC();
    {
        var st = wt.Value(); if (st != bt) {
            Ꮡt.Fatalf("weak pointer is not the same as strong pointer after GC: %p vs. %p"u8, st.OrTypedNil(), bt.OrTypedNil());
        }
    }
    // bt is no longer referenced.
    Δruntime.GC();
    {
        var st = wt.Value(); if (st != nil) {
            Ꮡt.Fatalf("expected weak pointer to be nil, got %p"u8, st.OrTypedNil());
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object weakMakeTNilZeroValueOfˢ = (@string)"weak.Make[T](nil) != zero value of weak.Pointer[T]"u8;

public static void TestPointerEquality(ж<testing.T> Ꮡt) {
    weak.Pointer<T> zero = default!;
    var zeroNil = weak.Make<T>(nil);
    if (zero != zeroNil) {
        Ꮡt.Error(weakMakeTNilZeroValueOfˢ);
    }
    var bt = new slice<ж<T>>(10);
    var wt = new slice<weak.Pointer<T>>(10);
    var wo = new slice<weak.Pointer<nint>>(10);
    foreach (var (i, _) in bt) {
        bt[i] = @new<T>();
        wt[i] = weak.Make<T>(bt[i]);
        wo[i] = weak.Make<nint>(bt[i].of(T.Ꮡa));
    }
    foreach (var (i, _) in bt) {
        var st = wt[i].Value();
        if (st != bt[i]) {
            Ꮡt.Fatalf("weak pointer is not the same as strong pointer: %p vs. %p"u8, st.OrTypedNil(), bt[i].OrTypedNil());
        }
        {
            var wp = weak.Make<T>(st); if (wp != wt[i]) {
                Ꮡt.Fatalf("new weak pointer not equal to existing weak pointer: %v vs. %v"u8, wp, wt[i]);
            }
        }
        {
            var wp = weak.Make<nint>(st.of(T.Ꮡa)); if (wp != wo[i]) {
                Ꮡt.Fatalf("new weak pointer not equal to existing weak pointer: %v vs. %v"u8, wp, wo[i]);
            }
        }
        if (i == 0) {
            continue;
        }
        if (wt[i] == wt[i - 1]) {
            Ꮡt.Fatalf("expected weak pointers to not be equal to each other, but got %v"u8, wt[i]);
        }
    }
    // bt is still referenced.
    Δruntime.GC();
    foreach (var (i, _) in bt) {
        var st = wt[i].Value();
        if (st != bt[i]) {
            Ꮡt.Fatalf("weak pointer is not the same as strong pointer: %p vs. %p"u8, st.OrTypedNil(), bt[i].OrTypedNil());
        }
        {
            var wp = weak.Make<T>(st); if (wp != wt[i]) {
                Ꮡt.Fatalf("new weak pointer not equal to existing weak pointer: %v vs. %v"u8, wp, wt[i]);
            }
        }
        {
            var wp = weak.Make<nint>(st.of(T.Ꮡa)); if (wp != wo[i]) {
                Ꮡt.Fatalf("new weak pointer not equal to existing weak pointer: %v vs. %v"u8, wp, wo[i]);
            }
        }
        if (i == 0) {
            continue;
        }
        if (wt[i] == wt[i - 1]) {
            Ꮡt.Fatalf("expected weak pointers to not be equal to each other, but got %v"u8, wt[i]);
        }
    }
    bt = default!;
    // bt is no longer referenced.
    Δruntime.GC();
    foreach (var (i, _) in bt) {
        var st = wt[i].Value();
        if (st != nil) {
            Ꮡt.Fatalf("expected weak pointer to be nil, got %p"u8, st.OrTypedNil());
        }
        if (i == 0) {
            continue;
        }
        if (wt[i] == wt[i - 1]) {
            Ꮡt.Fatalf("expected weak pointers to not be equal to each other, but got %v"u8, wt[i]);
        }
    }
}

public static void TestPointerFinalizer(ж<testing.T> Ꮡt) {
    var bt = @new<T>();
    ref var wt = ref heap<weak.Pointer<T>>(out var Ꮡwt);
    wt = weak.Make<T>(bt);
    var done = new channel<EmptyStruct>(1);
    var doneʗ1 = done;
    var wtʗ1 = wt;
    Δruntime.SetFinalizer(bt.OrTypedNil(), (ж<T> btΔ1) => {
        if (wtʗ1.Value() != nil) {
            Ꮡt.Errorf("weak pointer did not go nil before finalizer ran"u8);
        }
        doneʗ1.ᐸꟷ(new EmptyStruct());
    });
    // Make sure the weak pointer stays around while bt is live.
    Δruntime.GC();
    if (wt.Value() == nil) {
        Ꮡt.Errorf("weak pointer went nil too soon"u8);
    }
    Δruntime.KeepAlive(bt.OrTypedNil());
    // bt is no longer referenced.
    //
    // Run one cycle to queue the finalizer.
    Δruntime.GC();
    if (wt.Value() != nil) {
        Ꮡt.Errorf("weak pointer did not go nil when finalizer was enqueued"u8);
    }
    // Wait for the finalizer to run.
    ᐸꟷ(done);
    // The weak pointer should still be nil after the finalizer runs.
    Δruntime.GC();
    if (wt.Value() != nil) {
        Ꮡt.Errorf("weak pointer is non-nil even after finalization: %v"u8, wt);
    }
}

public static void TestPointerSize(ж<testing.T> Ꮡt) {
    weak.Pointer<T> p = default!;
    var size = /* unsafe.Sizeof(p) */ (uintptr)8;
    if (size != goarch.PtrSize) {
        Ꮡt.Errorf("weak.Pointer[T] size = %d, want %d"u8, size, (nint)(goarch.PtrSize));
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object thisIsAStressTestThatˢ = (@string)"this is a stress test that takes seconds to run on its own"u8;

// Regression test for issue 69210.
//
// Weak-to-strong conversions must shade the new strong pointer, otherwise
// that might be creating the only strong pointer to a white object which
// is hidden in a blackened stack.
//
// Never fails if correct, fails with some high probability if incorrect.
[MethodImpl(MethodImplOptions.NoInlining)] public static void TestIssue69210(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (testing.Short()) {
            Ꮡt.Skip(thisIsAStressTestThatˢ);
        }
        var (ctx, cancel) = context.WithTimeout(context.Background(), 1 * time.ΔSecond);
        var cancelʗ1 = cancel;
        defer(() => cancelʗ1(), ref ᒐ);
        // What we're trying to do is manufacture the conditions under which this
        // bug happens. Specifically, we want:
        //
        // 1. To create a whole bunch of objects that are only weakly-pointed-to,
        // 2. To call Value while the GC is in the mark phase,
        // 3. The new strong pointer to be missed by the GC,
        // 4. The following GC cycle to mark a free object.
        //
        // Unfortunately, (2) and (3) are hard to control, but we can increase
        // the likelihood by having several goroutines do (1) at once while
        // another goroutine constantly keeps us in the GC with runtime.GC.
        // Like throwing darts at a dart board until they land just right.
        // We can increase the likelihood of (4) by adding some delay after
        // creating the strong pointer, but only if it's non-nil. If it's nil,
        // that means it was already collected in which case there's no chance
        // of triggering the bug, so we want to retry as fast as possible.
        // Our heap here is tiny, so the GCs will go by fast.
        //
        // As of 2024-09-03, removing the line that shades pointers during
        // the weak-to-strong conversion causes this test to fail about 50%
        // of the time.
        ref var wg = ref heap(new Δsync.WaitGroup(), out var Ꮡwg);
        Ꮡwg.Add(1);
        var ctxʗ1 = ctx;
        goǃ(() => {
            GoFrame ᒐ = default;
            try {
                defer(Ꮡwg.Done, ref ᒐ);
                while (ᐧ) {
                    Δruntime.GC();
                    var selᴛ1 = ctxʗ1.Done();
                    switch (trySelect(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
                    case 0 when selᴛ1.ꟷᐳ(out _): {
                        return;
                    }
                    default: {
                        break;
                    }}
                }
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        });
        foreach (var _ᴛ1 in range(max(Δruntime.GOMAXPROCS(-1) - 1, 1))) {
            Ꮡwg.Add(1);
            var ctxʗ2 = ctx;
            goǃ(() => {
                GoFrame ᒐ = default;
                try {
                    defer(Ꮡwg.Done, ref ᒐ);
                    while (ᐧ) {
                        foreach (var _ᴛ2 in range(5)) {
                            var bt = @new<T>();
                            ref var wt = ref heap<weak.Pointer<T>>(out var Ꮡwt);
                            wt = weak.Make<T>(bt);
                            bt = default!;
                            time.Sleep(1 * time.Millisecond);
                            bt = wt.Value();
                            if (bt != nil) {
                                time.Sleep(4 * time.Millisecond);
                                bt.Value.t = bt;
                                bt.Value.a = 12;
                            }
                            Δruntime.KeepAlive(bt.OrTypedNil());
                        }
                        var selᴛ2 = ctxʗ2.Done();
                        switch (trySelect(ᐸꟷ(selᴛ2, ꓸꓸꓸ))) {
                        case 0 when selᴛ2.ꟷᐳ(out _): {
                            return;
                        }
                        default: {
                            break;
                        }}
                    }
                }
                catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                finally { ᒐ.Run(); }
            });
        }
        Ꮡwg.Wait();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object failedToLookUpSpecialAndˢ = (@string)"failed to look up special and made duplicate weak handle; see issue #70739"u8;

public static void TestIssue70739(ж<testing.T> Ꮡt) {
    var x = new slice<ж<nint>>((4 << (int)(16)));
    var wx1 = weak.Make<ж<nint>>(Ꮡ(x, (1 << (int)(16))));
    var wx2 = weak.Make<ж<nint>>(Ꮡ(x, (1 << (int)(16))));
    if (wx1 != wx2) {
        Ꮡt.Fatal(failedToLookUpSpecialAndˢ);
    }
}

} // end weak_test_package
