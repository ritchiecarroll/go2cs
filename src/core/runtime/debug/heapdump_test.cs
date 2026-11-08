// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using os = os_package;
using runtime = runtime_package;
using static go.runtime.debug_package;
using testing = testing_package;
using fs = go.io.fs_package;
using go.io;

partial class debug_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string heapdumptestˢ => ref heapdumptestˢᵇ;
private static readonly @string heapdumptestˢᵇ = "heapdumptest"u8;

public static void TestWriteHeapDumpNonempty(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "js"u8) {
            t.Skipf("WriteHeapDump is not available on %s.", runtime.GOOS);
        }
        var (f, err) = os.CreateTemp(""u8, heapdumptestˢ);
        if (err != nil) {
            t.Fatalf("TempFile failed: %v", err);
        }
        defer(os.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        WriteHeapDump(f.Fd());
        (var fi, err) = f.Stat();
        if (err != nil) {
            t.Fatalf("Stat failed: %v", err);
        }
        UntypedInt minSize = 1;
        var size = fi.Size(); if (size < minSize) {
            t.Fatalf("Heap dump size %d bytes, expected at least %d bytes", size, (nint)(minSize));
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct Obj {
    internal nint x, y;
}

internal static void objfin(ж<Obj> x) {
    //println("finalized", x)
}

public static void TestWriteHeapDumpFinalizers(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "js"u8) {
            t.Skipf("WriteHeapDump is not available on %s.", runtime.GOOS);
        }
        var (f, err) = os.CreateTemp(""u8, heapdumptestˢ);
        if (err != nil) {
            t.Fatalf("TempFile failed: %v", err);
        }
        defer(os.Remove, f.Name(), ref ᒐ);
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);

        // bug 9172: WriteHeapDump couldn't handle more than one finalizer
        println((@string)"allocating objects");
        var x = Ꮡ(new Obj());
        runtime.SetFinalizer(x.OrTypedNil(), objfin);
        var y = Ꮡ(new Obj());
        runtime.SetFinalizer(y.OrTypedNil(), objfin);

        // Trigger collection of x and y, queueing of their finalizers.
        println((@string)"starting gc");
        runtime.GC();

        // Make sure WriteHeapDump doesn't fail with multiple queued finalizers.
        println((@string)"starting dump");
        WriteHeapDump(f.Fd());
        println((@string)"done dump");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

partial struct G<T> {
}
partial interface I {
    void M();
}

//go:noinline
public static partial void M<T>(this G<T> g) { }

internal static I dummy = new G<nint>();
internal static I dummy2 = new G<G<nint>>();

public static void TestWriteHeapDumpTypeName(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (runtime.GOOS == "js"u8) {
            t.Skipf("WriteHeapDump is not available on %s.", runtime.GOOS);
        }
        var (f, err) = os.CreateTemp(""u8, heapdumptestˢ);
        if (err != nil) {
            t.Fatalf("TempFile failed: %v", err);
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
