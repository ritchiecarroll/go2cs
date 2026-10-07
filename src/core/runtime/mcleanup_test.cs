// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δruntime = runtime_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
[GoType("dyn")] internal partial struct TestCleanup_T {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanup(ж<testing.T> Ꮡt) {
    var ch = new channel<bool>(1);
    var done = new channel<bool>(1);
    nint want = 97531;
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanup_T>().of(TestCleanup_T.Ꮡv);
        v.Value = 97531;
        var chʗ2 = chʗ1;
        var cleanup = (nint x) => {
            if (x != want) {
                Ꮡt.Errorf("cleanup %d, want %d"u8, x, want);
            }
            chʗ2.ᐸꟷ(true);
        };
        Δruntime.AddCleanup(v, cleanup, 97531);
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
}

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
[GoType("dyn")] internal partial struct TestCleanupMultiple_T {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupMultiple(ж<testing.T> Ꮡt) {
    var ch = new channel<bool>(3);
    var done = new channel<bool>(1);
    nint want = 97531;
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupMultiple_T>().of(TestCleanupMultiple_T.Ꮡv);
        v.Value = 97531;
        var chʗ2 = chʗ1;
        var cleanup = (nint x) => {
            if (x != want) {
                Ꮡt.Errorf("cleanup %d, want %d"u8, x, want);
            }
            chʗ2.ᐸꟷ(true);
        };
        Δruntime.AddCleanup(v, cleanup, 97531);
        Δruntime.AddCleanup(v, cleanup, 97531);
        Δruntime.AddCleanup(v, cleanup, 97531);
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
    ᐸꟷ(ch);
    ᐸꟷ(ch);
}

[GoType("dyn")] internal partial struct TestCleanupZeroSizedStruct_Z {
}

public static void TestCleanupZeroSizedStruct(ж<testing.T> Ꮡt) {
    var z = @new<TestCleanupZeroSizedStruct_Z>();
    Δruntime.AddCleanup(z, (@string s) => {
    }, fooˢ2);
}

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
[GoType("dyn")] internal partial struct TestCleanupAfterFinalizer_T {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupAfterFinalizer(ж<testing.T> Ꮡt) {
    var ch = new channel<nint>(2);
    var done = new channel<bool>(1);
    nint want = 97531;
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupAfterFinalizer_T>().of(TestCleanupAfterFinalizer_T.Ꮡv);
        v.Value = 97531;
        var chʗ2 = chʗ1;
        var finalizer = (ж<nint> x) => {
            chʗ2.ᐸꟷ(1);
        };
        var chʗ3 = chʗ1;
        var cleanup = (nint x) => {
            if (x != want) {
                Ꮡt.Errorf("cleanup %d, want %d"u8, x, want);
            }
            chʗ3.ᐸꟷ(2);
        };
        Δruntime.AddCleanup(v, cleanup, 97531);
        Δruntime.SetFinalizer(v.OrTypedNil(), (finalizer).OrTypedNilFunc());
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    nint result = default!;
    result = ᐸꟷ(ch);
    if (result != 1) {
        Ꮡt.Errorf("result %d, want 1"u8, result);
    }
    Δruntime.GC();
    result = ᐸꟷ(ch);
    if (result != 2) {
        Ꮡt.Errorf("result %d, want 2"u8, result);
    }
}

// Allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
[GoType("dyn")] internal partial struct TestCleanupInteriorPointer_T {
    internal @unsafe.Pointer p;
    internal nint i;
    internal nint a;
    internal nint b;
    internal nint c;
}

public static partial void TestCleanupInteriorPointer(ж<testing.T> Ꮡt) {
    var ch = new channel<bool>(3);
    var done = new channel<bool>(1);
    nint want = 97531;
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var ts = @new<TestCleanupInteriorPointer_T>();
        ts.Value.a = 97531;
        ts.Value.b = 97531;
        ts.Value.c = 97531;
        var chʗ2 = chʗ1;
        var cleanup = (nint x) => {
            if (x != want) {
                Ꮡt.Errorf("cleanup %d, want %d"u8, x, want);
            }
            chʗ2.ᐸꟷ(true);
        };
        Δruntime.AddCleanup(ts.of(TestCleanupInteriorPointer_T.Ꮡa), cleanup, 97531);
        Δruntime.AddCleanup(ts.of(TestCleanupInteriorPointer_T.Ꮡb), cleanup, 97531);
        Δruntime.AddCleanup(ts.of(TestCleanupInteriorPointer_T.Ꮡc), cleanup, 97531);
        ts = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
    ᐸꟷ(ch);
    ᐸꟷ(ch);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cleanupCalledWantNoˢ = (@string)"cleanup called, want no cleanup called"u8;

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
[GoType("dyn")] internal partial struct TestCleanupStop_T {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupStop(ж<testing.T> Ꮡt) {
    var done = new channel<bool>(1);
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupStop_T>().of(TestCleanupStop_T.Ꮡv);
        v.Value = 97531;
        var cleanup = (nint x) => {
            Ꮡt.Error(cleanupCalledWantNoˢ);
        };
        ref var c = ref heap<Δruntime.Cleanup>(out var Ꮡc);
        c = Δruntime.AddCleanup(v, cleanup, 97531);
        c.Stop();
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
}

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
[GoType("dyn")] internal partial struct TestCleanupStopMultiple_T {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupStopMultiple(ж<testing.T> Ꮡt) {
    var done = new channel<bool>(1);
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupStopMultiple_T>().of(TestCleanupStopMultiple_T.Ꮡv);
        v.Value = 97531;
        var cleanup = (nint x) => {
            Ꮡt.Error(cleanupCalledWantNoˢ);
        };
        ref var c = ref heap<Δruntime.Cleanup>(out var Ꮡc);
        c = Δruntime.AddCleanup(v, cleanup, 97531);
        c.Stop();
        c.Stop();
        c.Stop();
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
}

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
[GoType("dyn")] internal partial struct TestCleanupStopinterleavedMultiple_T {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupStopinterleavedMultiple(ж<testing.T> Ꮡt) {
    var ch = new channel<bool>(3);
    var done = new channel<bool>(1);
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupStopinterleavedMultiple_T>().of(TestCleanupStopinterleavedMultiple_T.Ꮡv);
        v.Value = 97531;
        var chʗ2 = chʗ1;
        var cleanup = (nint x) => {
            if (x != 1) {
                Ꮡt.Error(cleanupCalledWantNoˢ);
            }
            chʗ2.ᐸꟷ(true);
        };
        Δruntime.AddCleanup(v, cleanup, 1);
        Δruntime.AddCleanup(v, cleanup, 2).Stop();
        Δruntime.AddCleanup(v, cleanup, 1);
        Δruntime.AddCleanup(v, cleanup, 2).Stop();
        Δruntime.AddCleanup(v, cleanup, 1);
        v = default!;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
    ᐸꟷ(ch);
    ᐸꟷ(ch);
}

// Allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
[GoType("dyn")] internal partial struct TestCleanupStopAfterCleanupRuns_T {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestCleanupStopAfterCleanupRuns(ж<testing.T> Ꮡt) {
    var ch = new channel<bool>(1);
    var done = new channel<bool>(1);
    ref var stop = ref heap<Action>(out var Ꮡstop);
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = @new<TestCleanupStopAfterCleanupRuns_T>().of(TestCleanupStopAfterCleanupRuns_T.Ꮡv);
        v.Value = 97531;
        var chʗ2 = chʗ1;
        var cleanup = (nint x) => {
            chʗ2.ᐸꟷ(true);
        };
        ref var cl = ref heap<Δruntime.Cleanup>(out var Ꮡcl);
        cl = Δruntime.AddCleanup(v, cleanup, 97531);
        v = default!;
        var clʗ1 = cl;
                Ꮡstop.ValueSlot = () => clʗ1.Stop();
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
    stop();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string runtimeAddCleanupPtrIsˢ = "runtime.AddCleanup: ptr is equal to arg, cleanup will never run"u8;
internal static readonly object wantPanicTestDidNotPanicˢ = (@string)"want panic, test did not panic"u8;

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
[GoType("dyn")] internal partial struct TestCleanupPointerEqualsArg_T {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static void TestCleanupPointerEqualsArg(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // See go.dev/issue/71316
        defer(() => {
            @string want = runtimeAddCleanupPtrIsˢ;
            {
                var r = recover(); if (r == default!){
                    Ꮡt.Error(wantPanicTestDidNotPanicˢ);
                } else 
                if (AreEqual(r, want)){
                } else {
                    // do nothing
                    Ꮡt.Errorf("wrong panic: want=%q, got=%q"u8, want, r);
                }
            }
        }, ref ᒐ);
        var v = @new<TestCleanupPointerEqualsArg_T>().of(TestCleanupPointerEqualsArg_T.Ꮡv);
        v.Value = 97531;
        Δruntime.AddCleanup(v, (ж<nint> x) => {
        }, v);
        v = default!;
        Δruntime.GC();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package
