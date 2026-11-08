// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using goarch = go.@internal.goarch_package;
using atomic = go.@internal.runtime.atomic_package;
using runtime = runtime_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using go.@internal;
using go.@internal.runtime;

partial class atomic_test_package {

internal static partial void runParallel(nint N, nint iter, Action f) {
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

public static void TestXadduintptr(ж<testing.T> t) {
    nint N = 20;
    nint iter = 100000;
    if (testing.Short()) {
        N = 10;
        iter = 10000;
    }
    var inc = (uintptr)100;
    ref var total = ref heap<uintptr>((uintptr)0, out var Ꮡtotal);
    runParallel(N, iter, () => {
        atomic.Xadduintptr(Ꮡtotal, inc);
    });
    var want = (uintptr)(N * iter) * inc; if (want != total) {
        t.Fatalf("xadduintpr error, want %d, got %d", want, total);
    }
    total = 0;
    runParallel(N, iter, () => {
        atomic.Xadduintptr(Ꮡtotal, inc);
        atomic.Xadduintptr(Ꮡtotal, (uintptr)(-(int64)inc));
    });
    if (total != 0) {
        t.Fatalf("xadduintpr total error, want %d, got %d", (nint)0, total);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object skipXadduintptrOnBigˢ = (@string)"skip xadduintptr on big endian architecture"u8;

// Tests that xadduintptr correctly updates 64-bit values. The place where
// we actually do so is mstats.go, functions mSysStat{Inc,Dec}.
public static void TestXadduintptrOnUint64(ж<testing.T> t) {
    if (goarch.BigEndian) {
        // On big endian architectures, we never use xadduintptr to update
        // 64-bit values and hence we skip the test.  (Note that functions
        // mSysStat{Inc,Dec} in mstats.go have explicit checks for
        // big-endianness.)
        t.Skip(skipXadduintptrOnBigˢ);
    }
    UntypedInt inc = 100;
    ref var val = ref heap<uint64>((uint64)0, out var Ꮡval);
    atomic.Xadduintptr(Ꮡval.Reinterpret<uint64, uintptr>(), inc);
    if (inc != val) {
        t.Fatalf("xadduintptr should increase lower-order bits, want %d, got %d", (nint)(inc), val);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static ref readonly @string unaligned64BitAtomicˢ => ref unaligned64BitAtomicˢᵇ;
private static readonly @string unaligned64BitAtomicˢᵇ = "unaligned 64-bit atomic operation"u8;

internal static void shouldPanic(ж<testing.T> t, @string name, Action f) {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            // Check that all GC maps are sane.
            runtime.GC();

            var err = recover();
            @string want = unaligned64BitAtomicˢ;
            if (err == nil) {
                t.Errorf("%s did not panic", name);
            } else if (err._<@string>(ᐧ) is var (s, _) && s != want) {
                t.Errorf("%s: wanted panic %q, got %q", name, want, err);
            }
        }, ref ᒐ);
        f();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object testOnlyRunsOn32Bitˢ = (@string)"test only runs on 32-bit systems"u8;
private static ref readonly @string load64ˢ => ref load64ˢᵇ;
private static readonly @string load64ˢᵇ = "Load64"u8;
private static ref readonly @string loadint64ˢ => ref loadint64ˢᵇ;
private static readonly @string loadint64ˢᵇ = "Loadint64"u8;
private static ref readonly @string store64ˢ => ref store64ˢᵇ;
private static readonly @string store64ˢᵇ = "Store64"u8;
private static ref readonly @string xadd64ˢ => ref xadd64ˢᵇ;
private static readonly @string xadd64ˢᵇ = "Xadd64"u8;
private static ref readonly @string xchg64ˢ => ref xchg64ˢᵇ;
private static readonly @string xchg64ˢᵇ = "Xchg64"u8;
private static ref readonly @string cas64ˢ => ref cas64ˢᵇ;
private static readonly @string cas64ˢᵇ = "Cas64"u8;

// Variant of sync/atomic's TestUnaligned64:
public static void TestUnaligned64(ж<testing.T> t) {
    // Unaligned 64-bit atomics on 32-bit systems are
    // a continual source of pain. Test that on 32-bit systems they crash
    // instead of failing silently.

    if (/* unsafe.Sizeof(int(0)) */ (uintptr)8 != 4) {
        t.Skip(testOnlyRunsOn32Bitˢ);
    }

    var x = new slice<uint32>(4);
    @unsafe.Pointer u = (@unsafe.Pointer)((uintptr)((uintptr)Ꮡ(x, 0) | 4)); // force alignment to 4

    var up64 = (ж<uint64>)(uintptr)(u); // misaligned
    var p64 = (ж<int64>)(uintptr)(u);   // misaligned

    var up64ʗ1 = up64;
    shouldPanic(t, load64ˢ, () => {
        atomic.Load64(up64ʗ1);
    });
    var p64ʗ1 = p64;
    shouldPanic(t, loadint64ˢ, () => {
        atomic.Loadint64(p64ʗ1);
    });
    var up64ʗ2 = up64;
    shouldPanic(t, store64ˢ, () => {
        atomic.Store64(up64ʗ2, 0);
    });
    var up64ʗ3 = up64;
    shouldPanic(t, xadd64ˢ, () => {
        atomic.Xadd64(up64ʗ3, 1);
    });
    var up64ʗ4 = up64;
    shouldPanic(t, xchg64ˢ, () => {
        atomic.Xchg64(up64ʗ4, 1);
    });
    var up64ʗ5 = up64;
    shouldPanic(t, cas64ˢ, () => {
        atomic.Cas64(up64ʗ5, 1, 2);
    });
}

public static partial void TestAnd8(ж<testing.T> t) {
    // Basic sanity check.
    ref var x = ref heap<uint8>((uint8)0xff, out var Ꮡx);
    for (var i = (uint8)0; i < 8; i++) {
        atomic.And8(Ꮡx, (uint8)((uint8)(~(((uint8)1).Lsh((uint64)(i))))));
        var r = (uint8)(((uint8)0xff).Lsh((uint64)((uint8)(i + 1)))); if (x != r) {
            t.Fatalf("clearing bit %#x: want %#x, got %#x", (uint8)(((uint8)1).Lsh((uint64)(i))), r, x);
        }
    }

    // Set every bit in array to 1.
    var a = new slice<uint8>((1 << 12));
    foreach (var i in range(len(a))) {
        a[i] = 0xff;
    }

    // Clear array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 8; i++) {
        var m = (uint8)(((uint8)(~(uint8)(((uint8)1).Lsh((int64)(i))))));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            foreach (var iΔ1 in range(len(aʗ1))) {
                atomic.And8(Ꮡ(aʗ1, iΔ1), m);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 8; i++) {
        ᐸꟷ(done);
    }

    // Check that the array has been totally cleared.
    foreach (var (i, v) in range(a)) {
        if (v != 0) {
            t.Fatalf("a[%v] not cleared: want %#x, got %#x", i, (uint8)0, v);
        }
    }
}

public static partial void TestAnd(ж<testing.T> t) {
    // Basic sanity check.
    ref var x = ref heap<uint32>((uint32)0xffffffffU, out var Ꮡx);
    for (var i = (uint32)0; i < 32; i++) {
        atomic.And(Ꮡx, ~(((uint32)1).Lsh((uint64)(i))));
        var r = ((uint32)0xffffffffU).Lsh((uint64)(i + 1)); if (x != r) {
            t.Fatalf("clearing bit %#x: want %#x, got %#x", (uint32)(((uint32)1).Lsh((uint64)(i))), r, x);
        }
    }

    // Set every bit in array to 1.
    var a = new slice<uint32>((1 << 12));
    foreach (var i in range(len(a))) {
        a[i] = 0xffffffffU;
    }

    // Clear array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 32; i++) {
        var m = ~(uint32)(((uint32)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            foreach (var iΔ1 in range(len(aʗ1))) {
                atomic.And(Ꮡ(aʗ1, iΔ1), m);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 32; i++) {
        ᐸꟷ(done);
    }

    // Check that the array has been totally cleared.
    foreach (var (i, v) in range(a)) {
        if (v != 0) {
            t.Fatalf("a[%v] not cleared: want %#x, got %#x", i, (uint32)0, v);
        }
    }
}

public static partial void TestOr8(ж<testing.T> t) {
    // Basic sanity check.
    ref var x = ref heap<uint8>((uint8)0, out var Ꮡx);
    for (var i = (uint8)0; i < 8; i++) {
        atomic.Or8(Ꮡx, (uint8)(((uint8)1).Lsh((uint64)(i))));
        var r = (uint8)((((uint8)1).Lsh((uint64)((uint8)(i + 1)))) - 1); if (x != r) {
            t.Fatalf("setting bit %#x: want %#x, got %#x", ((uint8)1).Lsh((uint64)(i)), r, x);
        }
    }

    // Start with every bit in array set to 0.
    var a = new slice<uint8>((1 << 12));

    // Set every bit in array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 8; i++) {
        var m = (uint8)(((uint8)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            foreach (var iΔ1 in range(len(aʗ1))) {
                atomic.Or8(Ꮡ(aʗ1, iΔ1), m);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 8; i++) {
        ᐸꟷ(done);
    }

    // Check that the array has been totally set.
    foreach (var (i, v) in range(a)) {
        if (v != 0xff) {
            t.Fatalf("a[%v] not fully set: want %#x, got %#x", i, (uint8)0xff, v);
        }
    }
}

public static partial void TestOr(ж<testing.T> t) {
    // Basic sanity check.
    ref var x = ref heap<uint32>((uint32)0, out var Ꮡx);
    for (var i = (uint32)0; i < 32; i++) {
        atomic.Or(Ꮡx, ((uint32)1).Lsh((uint64)(i)));
        var r = (((uint32)1).Lsh((uint64)(i + 1))) - 1; if (x != r) {
            t.Fatalf("setting bit %#x: want %#x, got %#x", ((uint32)1).Lsh((uint64)(i)), r, x);
        }
    }

    // Start with every bit in array set to 0.
    var a = new slice<uint32>((1 << 12));

    // Set every bit in array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 32; i++) {
        var m = (uint32)(((uint32)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            foreach (var iΔ1 in range(len(aʗ1))) {
                atomic.Or(Ꮡ(aʗ1, iΔ1), m);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 32; i++) {
        ᐸꟷ(done);
    }

    // Check that the array has been totally set.
    foreach (var (i, v) in range(a)) {
        if (v != 0xffffffffU) {
            t.Fatalf("a[%v] not fully set: want %#x, got %#x", i, (uint32)0xffffffffU, v);
        }
    }
}

public static partial void TestBitwiseContended8(ж<testing.T> t) {
    // Start with every bit in array set to 0.
    var a = new slice<uint8>(16);

    // Iterations to try.
    nint N = 1 << 16;
    if (testing.Short()) {
        N = 1 << 10;
    }

    // Set and then clear every bit in the array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 8; i++) {
        var m = (uint8)(((uint8)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            for (nint n = 0; n < N; n++) {
                foreach (var iΔ1 in range(len(aʗ1))) {
                    atomic.Or8(Ꮡ(aʗ1, iΔ1), m);
                    if ((uint8)(atomic.Load8(Ꮡ(aʗ1, iΔ1)) & m) != m) {
                        t.Errorf("a[%v] bit %#x not set", iΔ1, m);
                    }
                    atomic.And8(Ꮡ(aʗ1, iΔ1), (uint8)((uint8)(~m)));
                    if ((uint8)(atomic.Load8(Ꮡ(aʗ1, iΔ1)) & m) != 0) {
                        t.Errorf("a[%v] bit %#x not clear", iΔ1, m);
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
    foreach (var (i, v) in range(a)) {
        if (v != 0) {
            t.Fatalf("a[%v] not cleared: want %#x, got %#x", i, (uint8)0, v);
        }
    }
}

public static partial void TestBitwiseContended(ж<testing.T> t) {
    // Start with every bit in array set to 0.
    var a = new slice<uint32>(16);

    // Iterations to try.
    nint N = 1 << 16;
    if (testing.Short()) {
        N = 1 << 10;
    }

    // Set and then clear every bit in the array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 32; i++) {
        var m = (uint32)(((uint32)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            for (nint n = 0; n < N; n++) {
                foreach (var iΔ1 in range(len(aʗ1))) {
                    atomic.Or(Ꮡ(aʗ1, iΔ1), m);
                    if ((atomic.Load(Ꮡ(aʗ1, iΔ1)) & m) != m) {
                        t.Errorf("a[%v] bit %#x not set", iΔ1, m);
                    }
                    atomic.And(Ꮡ(aʗ1, iΔ1), ~m);
                    if ((atomic.Load(Ꮡ(aʗ1, iΔ1)) & m) != 0) {
                        t.Errorf("a[%v] bit %#x not clear", iΔ1, m);
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
    foreach (var (i, v) in range(a)) {
        if (v != 0) {
            t.Fatalf("a[%v] not cleared: want %#x, got %#x", i, (uint32)0, v);
        }
    }
}

internal partial struct TestCasRel_x /*dyn*/ {
    internal uint32 before;
    internal uint32 i;
    internal uint32 after;
    internal uint32 o;
    internal uint32 n;
}

public static void TestCasRel(ж<testing.T> t) {
    UntypedInt _magic = 0x5a5aa5a5;
    ref var x = ref heap(new TestCasRel_x(), out var Ꮡx);

    x.before = _magic;
    x.after = _magic;
    for (nint j = 0; j < 32; j += 1) {
        x.i = (((uint32)1).Lsh((int64)(j))) + 0;
        x.o = (((uint32)1).Lsh((int64)(j))) + 0;
        x.n = (((uint32)1).Lsh((int64)(j))) + 1;
        if (!atomic.CasRel(Ꮡx.of(TestCasRel_x.Ꮡi), x.o, x.n)) {
            t.Fatalf("should have swapped %#x %#x", x.o, x.n);
        }

        if (x.i != x.n) {
            t.Fatalf("wrong x.i after swap: x.i=%#x x.n=%#x", x.i, x.n);
        }

        if (x.before != _magic || x.after != _magic) {
            t.Fatalf("wrong magic: %#x _ %#x != %#x _ %#x", x.before, x.after, (nint)(_magic), (nint)(_magic));
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object badEscapeAnalysisOfˢ = (@string)"Bad escape analysis of StorepNoWB"u8;

public static void TestStorepNoWB(ж<testing.T> t) {
    ref var p = ref heap(new array<ж<nint>>(2), out var Ꮡp);
    foreach (var i in range(len(p))) {
        atomic.StorepNoWB(@unsafe.Pointer.FromBox(Ꮡp.at<ж<nint>>(i)), @unsafe.Pointer.FromPinnedBox(@new<nint>()));
    }
    if (p[0] == p[1]) {
        t.Error(badEscapeAnalysisOfˢ);
    }
}

} // end atomic_test_package
