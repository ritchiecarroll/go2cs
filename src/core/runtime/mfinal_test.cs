// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using asan = @internal.asan_package;
using Δruntime = runtime_package;
using testing = testing_package;
using time = time_package;
using @unsafe = unsafe_package;
using @internal;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

partial class Tintptr /*ж<nint>*/; // assignable to *int
partial struct Tint /*num:nint*/;  // *Tint implements Tinter, interface{}

internal static void m(this ref Tint t) { }

partial interface Tinter {
    void m();
}

internal partial struct TestFinalizerType_type /*dyn*/ {
    internal Func<ж<nint>, any> convert;
    internal any finalizer;
}

// allocate struct with pointer to avoid hitting tinyalloc.
// Otherwise we can't be sure when the allocation will
// be freed.
internal partial struct TestFinalizerType_T /*dyn*/ {
    internal nint v;
    internal @unsafe.Pointer p;
}

public static partial void TestFinalizerType(ж<testing.T> t) {
    var ch = new channel<bool>(10);
    var chʗ1 = ch;
    void finalize(ж<nint> x) {
        if (x.Value != 97531) {
            t.Errorf("finalizer %d, want %d", x.Value, (nint)97531);
        }
        chʗ1.ᐸꟷ(true);
    }



        var finalizeʗ1 = finalize;


        var finalizeʗ2 = finalize;


        var finalizeʗ3 = finalize;


        var finalizeʗ4 = finalize;


        var finalizeʗ5 = finalize;


        var finalizeʗ6 = finalize;
    slice<TestFinalizerType_type> finalizerTests = slice<TestFinalizerType_type>([
        new((ж<nint> x) => x.OrTypedNil(), (ж<nint> v) => {
            finalizeʗ1(v);
        }),
        new((ж<nint> x) => new Tintptr(x), (Tintptr v) => {
            finalizeʗ2(v);
        }),
        new((ж<nint> x) => new Tintptr(x), (ж<nint> v) => {
            finalizeʗ3(v);
        }),
        new((ж<nint> x) => x.Reinterpret<nint, Tint>().OrTypedNil(), (ж<Tint> v) => {
            finalizeʗ4(v.Reinterpret<Tint, nint>());
        }),
        new((ж<nint> x) => x.Reinterpret<nint, Tint>().OrTypedNil(), (Tinter v) => {
            finalizeʗ5(v._<ж<Tint>>().Reinterpret<Tint, nint>());
        }),
        // Test case for argument spill slot.
        // If the spill slot was not counted for the frame size, it will (incorrectly) choose
        // call32 as the result has (exactly) 32 bytes. When the argument actually spills,
        // it clobbers the caller's frame (likely the return PC).
        new((ж<nint> x) => x.OrTypedNil(), array<int64> (any v) => {
            builtin.print(); // force spill
            finalizeʗ6(v._<ж<nint>>());
            return new int64[]{}.array(4);
        })
    ]);

    foreach (var (_, vᴛ1) in range(finalizerTests)) {
        ref var tt = ref heap(new TestFinalizerType_type(), out var Ꮡtt);
        tt = vᴛ1;

        var done = new channel<bool>(1);
        var doneʗ1 = done;
        var ttʗ1 = tt;
        goǃ(() => {
            var v = @new<TestFinalizerType_T>().Ꮡv;
            v.Value = 97531;
            Δruntime.SetFinalizer(ttʗ1.convert(v), ttʗ1.finalizer);
            v = default;
            doneʗ1.ᐸꟷ(true);
        });
        ᐸꟷ(done);
        Δruntime.GC();
        ᐸꟷ(ch);
    }
}

partial struct bigValue {
    internal uint64 fill;
    internal bool it;
    internal @string up;
}

public static partial void TestFinalizerInterfaceBig(ж<testing.T> t) {
    var ch = new channel<bool>(0);
    var done = new channel<bool>(1);
    var chʗ1 = ch;
    var doneʗ1 = done;
    goǃ(() => {
        var v = Ꮡ(new bigValue(0xDEADBEEFDEADBEEFUL, true, "It matters not how strait the gate"));
        ref var old = ref heap<bigValue>(v.Value, out var Ꮡold);
        var chʗ2 = chʗ1;
        var oldʗ1 = old;
        Δruntime.SetFinalizer(v.OrTypedNil(), (any vΔ1) => {
            var (i, ok) = vΔ1._<ж<bigValue>>(ᐧ);
            if (!ok) {
                t.Errorf("finalizer called with type %T, want *bigValue", vΔ1);
            }
            if (i.Value != oldʗ1) {
                t.Errorf("finalizer called with %+v, want %+v", i.Value, oldʗ1);
            }
            close(chʗ2);
        });
        v = default;
        doneʗ1.ᐸꟷ(true);
    });
    ᐸꟷ(done);
    Δruntime.GC();
    ᐸꟷ(ch);
}

internal static void fin(ж<nint> v) {
}

internal partial struct TestFinalizerZeroSizedStruct_Z /*dyn*/ {
}

// Verify we don't crash at least. golang.org/issue/6857
public static void TestFinalizerZeroSizedStruct(ж<testing.T> t) {
    var z = @new<TestFinalizerZeroSizedStruct_Z>();
    Δruntime.SetFinalizer(z.OrTypedNil(), (ж<TestFinalizerZeroSizedStruct_Z> _) => {
    });
}

public static void BenchmarkFinalizer(ж<testing.B> b) {
    UntypedInt Batch = 1000;
    b.RunParallel((ж<testing.PB> pb) => {
        array<ж<nint>> data = new(1000); /* Batch */
        for (nint i = 0; i < Batch; i++) {
            data[i] = @new<nint>();
        }
        while (pb.Next()) {
            for (nint i = 0; i < Batch; i++) {
                Δruntime.SetFinalizer(data[i].OrTypedNil(), fin);
            }
            for (nint i = 0; i < Batch; i++) {
                Δruntime.SetFinalizer(data[i].OrTypedNil(), default);
            }
        }
    });
}

public static void BenchmarkFinalizerRun(ж<testing.B> b) {
    b.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            var v = @new<nint>();
            Δruntime.SetFinalizer(v.OrTypedNil(), fin);
        }
    });
}

// One chunk must be exactly one sizeclass in size.
// It should be a sizeclass not used much by others, so we
// have a greater chance of finding adjacent ones.
// size class 19: 320 byte objects, 25 per page, 1 page alloc at a time
internal static UntypedInt objsize => 320;

/* [objsize]byte */
partial struct objtype /*[320]byte*/;

internal static (ж<objtype>, ж<objtype>) adjChunks() {
    var sᴷ = new stackAppend<ж<objtype>>();

    slice<ж<objtype>> s = default;

    for (;;) {
        var c = @new<objtype>();
        foreach (var (_, d) in range(s)) {
            if ((uintptr)c + /* unsafe.Sizeof(*c) */ (uintptr)320 == (uintptr)d) {
                return (c, d);
            }
            if ((uintptr)d + /* unsafe.Sizeof(*c) */ (uintptr)320 == (uintptr)c) {
                return (d, c);
            }
        }
        s = append(sᴷ, s, c);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingWithAsanTestˢ = (@string)"skipping with -asan: test assumes exact size class alignment, but asan redzone breaks that assumption"u8;

// Make sure an empty slice on the stack doesn't pin the next object in memory.
public static void TestEmptySlice(ж<testing.T> t) {
    if (asan.Enabled) {
        t.Skip(skippingWithAsanTestˢ);
    }
    var (x, y) = adjChunks();

    // the pointer inside xs points to y.
    var xs = x.Value[(int)(objsize)..]; // change objsize to objsize-1 and the test passes

    var fin = new channel<bool>(1);
    var finʗ1 = fin;
    Δruntime.SetFinalizer(y.OrTypedNil(), (ж<objtype> z) => {
        finʗ1.ᐸꟷ(true);
    });
    Δruntime.GC();
    ᐸꟷ(fin);
    xsglobal = xs; // keep empty slice alive until here
}

internal static slice<byte> xsglobal;

internal static (@string, ж<objtype>) adjStringChunk() {
    var b = new slice<byte>(objsize);
    for (;;) {
        ref var s = ref heap<@string>((@string)b, out var Ꮡs);
        var t = @new<objtype>();
        var p = ~Ꮡs.Reinterpret<@string, uintptr>();
        var q = (uintptr)t;
        if (p + (uintptr)objsize == q) {
            return (s, t);
        }
    }
}

// Make sure an empty string on the stack doesn't pin the next object in memory.
public static void TestEmptyString(ж<testing.T> t) {
    if (asan.Enabled) {
        t.Skip(skippingWithAsanTestˢ);
    }
    var (x, y) = adjStringChunk();

    @string ss = x[(int)(objsize)..]; // change objsize to objsize-1 and the test passes
    var fin = new channel<bool>(1);
    // set finalizer on string contents of y
    var finʗ1 = fin;
    Δruntime.SetFinalizer(y.OrTypedNil(), (ж<objtype> z) => {
        finʗ1.ᐸꟷ(true);
    });
    Δruntime.GC();
    ᐸꟷ(fin);
    ssglobal = ss; // keep 0-length string live until here
}

internal static @string ssglobal;

// Test for issue 7656.
public static void TestFinalizerOnGlobal(ж<testing.T> t) {
    Δruntime.SetFinalizer(Foo1.OrTypedNil(), (ж<Object1> p) => {
    });
    Δruntime.SetFinalizer(Foo2.OrTypedNil(), (ж<Object2> p) => {
    });
    Δruntime.SetFinalizer(Foo1.OrTypedNil(), default);
    Δruntime.SetFinalizer(Foo2.OrTypedNil(), default);
}

partial struct Object1 {
    public slice<byte> Something;
}

partial struct Object2 {
    public byte Something;
}

public static ж<Object2> Foo2 = Ꮡ(new Object2());
public static ж<Object1> Foo1 = Ꮡ(new Object1());

internal partial class TestDeferKeepAlive_T /*ж<nint>*/; // needs to be a pointer base type to avoid tinyalloc and its never-finalized behavior.

public static void TestDeferKeepAlive(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        if (flagQuick.Value) {
            t.Skip(quickˢ);
        }

        // See issue 21402.
        t.Parallel();
        var x = @new<TestDeferKeepAlive_T>();
        var finRun = false;
        Δruntime.SetFinalizer(x.OrTypedNil(), (ж<TestDeferKeepAlive_T> xΔ1) => {
            finRun = true;
        });
        defer(Δruntime.KeepAlive, x.OrTypedNil(), ref ᒐ);
        Δruntime.GC();
        time.Sleep(time.ΔSecond);
        if (finRun) {
            t.Errorf("finalizer ran prematurely");
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end runtime_test_package
