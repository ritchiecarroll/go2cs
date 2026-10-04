// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using goarch = go.@internal.goarch_package;
using atomic = go.@internal.runtime.atomic_package;
using runtime = runtime_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.@internal;
using go.@internal.runtime;

partial class atomic_test_package {

[MethodImpl(MethodImplOptions.NoInlining)] internal static void runParallel(nint N, nint iter, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(runtime.GOMAXPROCS, runtime.GOMAXPROCS((nint)N), ref ᒐ);
        var done = new channel<bool>(0);
        for (nint i = 0; i < N; i++) {
            var doneʗ1 = done;
            goǃ(() => {
                for (nint j = 0; j < iter; j++) {
                    f();
                }
                doneʗ1.ᐸꟷ(true);
            });
        }
        for (nint i = 0; i < N; i++) {
            ᐸꟷ(done);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestXadduintptr(ж<testing.T> Ꮡt) {
    nint N = 20;
    nint iter = 100000;
    if (testing.Short()) {
        N = 10;
        iter = 10000;
    }
    var inc = (uintptr)100;
    ref var total = ref heap<uintptr>(out var Ꮡtotal);
    total = (uintptr)0;
    runParallel(N, iter, () => {
        atomic.Xadduintptr(Ꮡtotal, inc);
    });
    {
        var want = (uintptr)(N * iter) * inc; if (want != total) {
            Ꮡt.Fatalf("xadduintpr error, want %d, got %d"u8, want, total);
        }
    }
    total = 0;
    runParallel(N, iter, () => {
        atomic.Xadduintptr(Ꮡtotal, inc);
        atomic.Xadduintptr(Ꮡtotal, (uintptr)(-(int64)inc));
    });
    if (total != 0) {
        Ꮡt.Fatalf("xadduintpr total error, want %d, got %d"u8, (nint)(0), total);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skipXadduintptrOnBigˢ = (@string)"skip xadduintptr on big endian architecture"u8;

// Tests that xadduintptr correctly updates 64-bit values. The place where
// we actually do so is mstats.go, functions mSysStat{Inc,Dec}.
public static void TestXadduintptrOnUint64(ж<testing.T> Ꮡt) {
    if (goarch.BigEndian) {
        // On big endian architectures, we never use xadduintptr to update
        // 64-bit values and hence we skip the test.  (Note that functions
        // mSysStat{Inc,Dec} in mstats.go have explicit checks for
        // big-endianness.)
        Ꮡt.Skip(skipXadduintptrOnBigˢ);
    }
    UntypedInt inc = 100;
    ref var val = ref heap<uint64>(out var Ꮡval);
    val = (uint64)0;
    atomic.Xadduintptr(Ꮡval.Reinterpret<uint64, uintptr>(), inc);
    if (inc != val) {
        Ꮡt.Fatalf("xadduintptr should increase lower-order bits, want %d, got %d"u8, (nint)(inc), val);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string unaligned64BitAtomicˢ = "unaligned 64-bit atomic operation"u8;

internal static void shouldPanic(ж<testing.T> Ꮡt, @string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            // Check that all GC maps are sane.
            runtime.GC();
            var err = recover();
            @string want = unaligned64BitAtomicˢ;
            if (err == default!){
                Ꮡt.Errorf("%s did not panic"u8, name);
            } else 
            {
                var (s, _) = err._<@string>(ᐧ); if (s != want) {
                    Ꮡt.Errorf("%s: wanted panic %q, got %q"u8, name, want, err);
                }
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object testOnlyRunsOn32Bitˢ = (@string)"test only runs on 32-bit systems"u8;
private static readonly @string load64ˢ = "Load64"u8;
private static readonly @string loadint64ˢ = "Loadint64"u8;
private static readonly @string store64ˢ = "Store64"u8;
private static readonly @string xadd64ˢ = "Xadd64"u8;
private static readonly @string xchg64ˢ = "Xchg64"u8;
private static readonly @string cas64ˢ = "Cas64"u8;

// Variant of sync/atomic's TestUnaligned64:
public static void TestUnaligned64(ж<testing.T> Ꮡt) {
    // Unaligned 64-bit atomics on 32-bit systems are
    // a continual source of pain. Test that on 32-bit systems they crash
    // instead of failing silently.
    if (/* unsafe.Sizeof(int(0)) */ (uintptr)8 != 4) {
        Ꮡt.Skip(testOnlyRunsOn32Bitˢ);
    }
    var x = new slice<uint32>(4);
    @unsafe.Pointer u = (@unsafe.Pointer)((uintptr)((uintptr)Ꮡ(x, 0) | 4)); // force alignment to 4
    var up64 = (ж<uint64>)(uintptr)(u); // misaligned
    var p64 = (ж<int64>)(uintptr)(u); // misaligned
    var up64ʗ1 = up64;
    shouldPanic(Ꮡt, load64ˢ, () => {
        atomic.Load64(up64ʗ1);
    });
    var p64ʗ1 = p64;
    shouldPanic(Ꮡt, loadint64ˢ, () => {
        atomic.Loadint64(p64ʗ1);
    });
    var up64ʗ2 = up64;
    shouldPanic(Ꮡt, store64ˢ, () => {
        atomic.Store64(up64ʗ2, 0);
    });
    var up64ʗ3 = up64;
    shouldPanic(Ꮡt, xadd64ˢ, () => {
        atomic.Xadd64(up64ʗ3, 1);
    });
    var up64ʗ4 = up64;
    shouldPanic(Ꮡt, xchg64ˢ, () => {
        atomic.Xchg64(up64ʗ4, 1);
    });
    var up64ʗ5 = up64;
    shouldPanic(Ꮡt, cas64ˢ, () => {
        atomic.Cas64(up64ʗ5, 1, 2);
    });
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestAnd8(ж<testing.T> Ꮡt) {
    // Basic sanity check.
    ref var x = ref heap<uint8>(out var Ꮡx);
    x = (uint8)0xff;
    for (var i = (uint8)0; i < 8; i++) {
        atomic.And8(Ꮡx, (uint8)(((uint8)(~(((uint8)1).Lsh((uint64)(i)))))));
        {
            var r = (uint8)(((uint8)0xff).Lsh((uint64)((uint8)(i + 1)))); if (x != r) {
                Ꮡt.Fatalf("clearing bit %#x: want %#x, got %#x"u8, (uint8)(((uint8)1).Lsh((uint64)(i))), r, x);
            }
        }
    }
    // Set every bit in array to 1.
    var a = new slice<uint8>((1 << (int)(12)));
    foreach (var (i, _) in a) {
        a[i] = 0xff;
    }
    // Clear array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 8; i++) {
        var m = (uint8)(((uint8)(~(uint8)(((uint8)1).Lsh((int64)(i))))));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            foreach (var (iΔ1, _) in aʗ1) {
                atomic.And8(Ꮡ(aʗ1, iΔ1), m);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 8; i++) {
        ᐸꟷ(done);
    }
    // Check that the array has been totally cleared.
    foreach (var (i, v) in a) {
        if (v != 0) {
            Ꮡt.Fatalf("a[%v] not cleared: want %#x, got %#x"u8, i, (uint8)0, v);
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestAnd(ж<testing.T> Ꮡt) {
    // Basic sanity check.
    ref var x = ref heap<uint32>(out var Ꮡx);
    x = (uint32)0xffffffffU;
    for (var i = (uint32)0; i < 32; i++) {
        atomic.And(Ꮡx, ~(((uint32)1).Lsh((uint64)(i))));
        {
            var r = ((uint32)0xffffffffU).Lsh((uint64)((i + 1))); if (x != r) {
                Ꮡt.Fatalf("clearing bit %#x: want %#x, got %#x"u8, (uint32)(((uint32)1).Lsh((uint64)(i))), r, x);
            }
        }
    }
    // Set every bit in array to 1.
    var a = new slice<uint32>((1 << (int)(12)));
    foreach (var (i, _) in a) {
        a[i] = 0xffffffffU;
    }
    // Clear array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 32; i++) {
        var m = ~(uint32)(((uint32)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            foreach (var (iΔ1, _) in aʗ1) {
                atomic.And(Ꮡ(aʗ1, iΔ1), m);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 32; i++) {
        ᐸꟷ(done);
    }
    // Check that the array has been totally cleared.
    foreach (var (i, v) in a) {
        if (v != 0) {
            Ꮡt.Fatalf("a[%v] not cleared: want %#x, got %#x"u8, i, (uint32)0, v);
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestOr8(ж<testing.T> Ꮡt) {
    // Basic sanity check.
    ref var x = ref heap<uint8>(out var Ꮡx);
    x = (uint8)0;
    for (var i = (uint8)0; i < 8; i++) {
        atomic.Or8(Ꮡx, (uint8)(((uint8)1).Lsh((uint64)(i))));
        {
            var r = (uint8)((((uint8)1).Lsh((uint64)((uint8)(i + 1)))) - 1); if (x != r) {
                Ꮡt.Fatalf("setting bit %#x: want %#x, got %#x"u8, ((uint8)1).Lsh((uint64)(i)), r, x);
            }
        }
    }
    // Start with every bit in array set to 0.
    var a = new slice<uint8>((1 << (int)(12)));
    // Set every bit in array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 8; i++) {
        var m = (uint8)(((uint8)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            foreach (var (iΔ1, _) in aʗ1) {
                atomic.Or8(Ꮡ(aʗ1, iΔ1), m);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 8; i++) {
        ᐸꟷ(done);
    }
    // Check that the array has been totally set.
    foreach (var (i, v) in a) {
        if (v != 0xff) {
            Ꮡt.Fatalf("a[%v] not fully set: want %#x, got %#x"u8, i, (uint8)0xff, v);
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestOr(ж<testing.T> Ꮡt) {
    // Basic sanity check.
    ref var x = ref heap<uint32>(out var Ꮡx);
    x = (uint32)0;
    for (var i = (uint32)0; i < 32; i++) {
        atomic.Or(Ꮡx, ((uint32)1).Lsh((uint64)(i)));
        {
            var r = (((uint32)1).Lsh((uint64)((i + 1)))) - 1; if (x != r) {
                Ꮡt.Fatalf("setting bit %#x: want %#x, got %#x"u8, ((uint32)1).Lsh((uint64)(i)), r, x);
            }
        }
    }
    // Start with every bit in array set to 0.
    var a = new slice<uint32>((1 << (int)(12)));
    // Set every bit in array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 32; i++) {
        var m = (uint32)(((uint32)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            foreach (var (iΔ1, _) in aʗ1) {
                atomic.Or(Ꮡ(aʗ1, iΔ1), m);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 32; i++) {
        ᐸꟷ(done);
    }
    // Check that the array has been totally set.
    foreach (var (i, v) in a) {
        if (v != 0xffffffffU) {
            Ꮡt.Fatalf("a[%v] not fully set: want %#x, got %#x"u8, i, (uint32)0xffffffffU, v);
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestBitwiseContended8(ж<testing.T> Ꮡt) {
    // Start with every bit in array set to 0.
    var a = new slice<uint8>(16);
    // Iterations to try.
    nint N = (1 << (int)(16));
    if (testing.Short()) {
        N = (1 << (int)(10));
    }
    // Set and then clear every bit in the array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 8; i++) {
        var m = (uint8)(((uint8)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            for (nint n = 0; n < N; n++) {
                foreach (var (iΔ1, _) in aʗ1) {
                    atomic.Or8(Ꮡ(aʗ1, iΔ1), m);
                    if ((uint8)(atomic.Load8(Ꮡ(aʗ1, iΔ1)) & m) != m) {
                        Ꮡt.Errorf("a[%v] bit %#x not set"u8, iΔ1, m);
                    }
                    atomic.And8(Ꮡ(aʗ1, iΔ1), (uint8)(((uint8)(~m))));
                    if ((uint8)(atomic.Load8(Ꮡ(aʗ1, iΔ1)) & m) != 0) {
                        Ꮡt.Errorf("a[%v] bit %#x not clear"u8, iΔ1, m);
                    }
                }
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 8; i++) {
        ᐸꟷ(done);
    }
    // Check that the array has been totally cleared.
    foreach (var (i, v) in a) {
        if (v != 0) {
            Ꮡt.Fatalf("a[%v] not cleared: want %#x, got %#x"u8, i, (uint8)0, v);
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestBitwiseContended(ж<testing.T> Ꮡt) {
    // Start with every bit in array set to 0.
    var a = new slice<uint32>(16);
    // Iterations to try.
    nint N = (1 << (int)(16));
    if (testing.Short()) {
        N = (1 << (int)(10));
    }
    // Set and then clear every bit in the array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 32; i++) {
        var m = (uint32)(((uint32)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            for (nint n = 0; n < N; n++) {
                foreach (var (iΔ1, _) in aʗ1) {
                    atomic.Or(Ꮡ(aʗ1, iΔ1), m);
                    if ((uint32)(atomic.Load(Ꮡ(aʗ1, iΔ1)) & m) != m) {
                        Ꮡt.Errorf("a[%v] bit %#x not set"u8, iΔ1, m);
                    }
                    atomic.And(Ꮡ(aʗ1, iΔ1), ~m);
                    if ((uint32)(atomic.Load(Ꮡ(aʗ1, iΔ1)) & m) != 0) {
                        Ꮡt.Errorf("a[%v] bit %#x not clear"u8, iΔ1, m);
                    }
                }
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 32; i++) {
        ᐸꟷ(done);
    }
    // Check that the array has been totally cleared.
    foreach (var (i, v) in a) {
        if (v != 0) {
            Ꮡt.Fatalf("a[%v] not cleared: want %#x, got %#x"u8, i, (uint32)0, v);
        }
    }
}

[GoType("dyn")] internal partial struct TestCasRel_x {
    internal uint32 before;
    internal uint32 i;
    internal uint32 after;
    internal uint32 o;
    internal uint32 n;
}

public static void TestCasRel(ж<testing.T> Ꮡt) {
    UntypedInt _magic = 0x5a5aa5a5;
    ref var x = ref heap(new TestCasRel_x(), out var Ꮡx);
    x.before = _magic;
    x.after = _magic;
    for (nint j = 0; j < 32; j += 1) {
        x.i = (((uint32)1).Lsh((int64)(j))) + 0;
        x.o = (((uint32)1).Lsh((int64)(j))) + 0;
        x.n = (((uint32)1).Lsh((int64)(j))) + 1;
        if (!atomic.CasRel(Ꮡx.of(TestCasRel_x.Ꮡi), x.o, x.n)) {
            Ꮡt.Fatalf("should have swapped %#x %#x"u8, x.o, x.n);
        }
        if (x.i != x.n) {
            Ꮡt.Fatalf("wrong x.i after swap: x.i=%#x x.n=%#x"u8, x.i, x.n);
        }
        if (x.before != _magic || x.after != _magic) {
            Ꮡt.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x"u8, x.before, x.after, (nint)(_magic), (nint)(_magic));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object badEscapeAnalysisOfˢ = (@string)"Bad escape analysis of StorepNoWB"u8;

public static void TestStorepNoWB(ж<testing.T> Ꮡt) {
    ref var p = ref heap(new array<ж<nint>>(2), out var Ꮡp);
    foreach (var (i, _) in p) {
        atomic.StorepNoWB(@unsafe.Pointer.FromBox(Ꮡp.at<ж<nint>>(i)), @unsafe.Pointer.FromPinnedBox(@new<nint>()));
    }
    if (p[0] == p[1]) {
        Ꮡt.Error(badEscapeAnalysisOfˢ);
    }
}

} // end atomic_test_package
