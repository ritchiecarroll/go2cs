// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using os = os_package;
using runtime = runtime_package;
using static go.runtime.debug_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using fs = go.io.fs_package;
using go.io;

partial class debug_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string heapdumptestˢ = "heapdumptest"u8;

public static void TestWriteHeapDumpNonempty(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "js"u8) {
            Ꮡt.Skipf("WriteHeapDump is not available on %s."u8, runtime.GOOS);
        }
        var (f, err) = os.CreateTemp(""u8, heapdumptestˢ);
        if (err != default!) {
            Ꮡt.Fatalf("TempFile failed: %v"u8, err);
        }
        defer(os.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        WriteHeapDump(f.Fd());
        (var fi, err) = f.Stat();
        if (err != default!) {
            Ꮡt.Fatalf("Stat failed: %v"u8, err);
        }
        UntypedInt minSize = 1;
        {
            var size = fi.Size(); if (size < minSize) {
                Ꮡt.Fatalf("Heap dump size %d bytes, expected at least %d bytes"u8, size, (nint)(minSize));
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct Obj {
    internal nint x, y;
}

internal static void objfin(ж<Obj> Ꮡx) {
}

//println("finalized", x)
public static void TestWriteHeapDumpFinalizers(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        ref var t = ref Ꮡt.DerefOrNull();

        if (runtime.GOOS == "js"u8) {
            Ꮡt.Skipf("WriteHeapDump is not available on %s."u8, runtime.GOOS);
        }
        var (f, err) = os.CreateTemp(""u8, heapdumptestˢ);
        if (err != default!) {
            Ꮡt.Fatalf("TempFile failed: %v"u8, err);
        }
        defer(os.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        // bug 9172: WriteHeapDump couldn't handle more than one finalizer
        println((@string)"allocating objects"u8);
        var x = Ꮡ(new Obj(nil));
        runtime.SetFinalizer(x.OrTypedNil(), objfin);
        var y = Ꮡ(new Obj(nil));
        runtime.SetFinalizer(y.OrTypedNil(), objfin);
        // Trigger collection of x and y, queueing of their finalizers.
        println((@string)"starting gc"u8);
        runtime.GC();
        // Make sure WriteHeapDump doesn't fail with multiple queued finalizers.
        println((@string)"starting dump"u8);
        WriteHeapDump(f.Fd());
        println((@string)"done dump"u8);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

[GoType] partial struct G<T> {
}

[GoType] partial interface I {
    void M();
}

//go:noinline
public static partial void M<T>(this G<T> g) {
}

internal static I dummy = new G<nint>(nil);

internal static I dummy2 = new G<G<nint>>(nil);

public static void TestWriteHeapDumpTypeName(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "js"u8) {
            Ꮡt.Skipf("WriteHeapDump is not available on %s."u8, runtime.GOOS);
        }
        var (f, err) = os.CreateTemp(""u8, heapdumptestˢ);
        if (err != default!) {
            Ꮡt.Fatalf("TempFile failed: %v"u8, err);
        }
        defer(os.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        WriteHeapDump(f.Fd());
        dummy.M();
        dummy2.M();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end debug_test_package
