// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// TODO(61395): move these tests to atomic_test.go once And/Or have
// implementations for all architectures.
namespace go.@internal.runtime;

using atomic = go.@internal.runtime.atomic_package;
using testing = testing_package;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using go.@internal.runtime;

partial class atomic_test_package {

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestAnd32(ж<testing.T> Ꮡt) {
    // Basic sanity check.
    ref var x = ref heap<uint32>(out var Ꮡx);
    x = (uint32)0xffffffffU;
    for (var i = (uint32)0; i < 32; i++) {
        var old = x;
        var v = atomic.And32(Ꮡx, ~(((uint32)1).Lsh((uint64)(i))));
        {
            var r = ((uint32)0xffffffffU).Lsh((uint64)((i + 1))); if (x != r || v != old) {
                Ꮡt.Fatalf("clearing bit %#x: want %#x, got new %#x and old %#v"u8, (uint32)(((uint32)1).Lsh((uint64)(i))), r, x, v);
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

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestAnd64(ж<testing.T> Ꮡt) {
    // Basic sanity check.
    ref var x = ref heap<uint64>(out var Ꮡx);
    x = (uint64)0xffffffffffffffffUL;
    sink = Ꮡx;
    for (var i = (uint64)0; i < 64; i++) {
        var old = x;
        var v = atomic.And64(Ꮡx, ~(((uint64)1).Lsh(i)));
        {
            var r = ((uint64)0xffffffffffffffffUL).Lsh((i + 1)); if (x != r || v != old) {
                Ꮡt.Fatalf("clearing bit %#x: want %#x, got new %#x and old %#v"u8, (uint64)(((uint64)1).Lsh(i)), r, x, v);
            }
        }
    }
    // Set every bit in array to 1.
    var a = new slice<uint64>((1 << (int)(12)));
    foreach (var (i, _) in a) {
        a[i] = 0xffffffffffffffffUL;
    }
    // Clear array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 64; i++) {
        var m = ~(uint64)(((uint64)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            foreach (var (iΔ1, _) in aʗ1) {
                atomic.And64(Ꮡ(aʗ1, iΔ1), m);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 64; i++) {
        ᐸꟷ(done);
    }
    // Check that the array has been totally cleared.
    foreach (var (i, v) in a) {
        if (v != 0) {
            Ꮡt.Fatalf("a[%v] not cleared: want %#x, got %#x"u8, i, (uint64)0, v);
        }
    }
}

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestOr32(ж<testing.T> Ꮡt) {
    // Basic sanity check.
    ref var x = ref heap<uint32>(out var Ꮡx);
    x = (uint32)0;
    for (var i = (uint32)0; i < 32; i++) {
        var old = x;
        var v = atomic.Or32(Ꮡx, ((uint32)1).Lsh((uint64)(i)));
        {
            var r = (((uint32)1).Lsh((uint64)((i + 1)))) - 1; if (x != r || v != old) {
                Ꮡt.Fatalf("setting bit %#x: want %#x, got new %#x and old %#v"u8, (uint32)(((uint32)1).Lsh((uint64)(i))), r, x, v);
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
                atomic.Or32(Ꮡ(aʗ1, iΔ1), m);
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

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestOr64(ж<testing.T> Ꮡt) {
    // Basic sanity check.
    ref var x = ref heap<uint64>(out var Ꮡx);
    x = (uint64)0;
    sink = Ꮡx;
    for (var i = (uint64)0; i < 64; i++) {
        var old = x;
        var v = atomic.Or64(Ꮡx, ((uint64)1).Lsh(i));
        {
            var r = (((uint64)1).Lsh((i + 1))) - 1; if (x != r || v != old) {
                Ꮡt.Fatalf("setting bit %#x: want %#x, got new %#x and old %#v"u8, (uint64)(((uint64)1).Lsh(i)), r, x, v);
            }
        }
    }
    // Start with every bit in array set to 0.
    var a = new slice<uint64>((1 << (int)(12)));
    // Set every bit in array bit-by-bit in different goroutines.
    var done = new channel<bool>(0);
    for (nint i = 0; i < 64; i++) {
        var m = (uint64)(((uint64)1).Lsh((int64)(i)));
        var aʗ1 = a;
        var doneʗ1 = done;
        goǃ(() => {
            foreach (var (iΔ1, _) in aʗ1) {
                atomic.Or64(Ꮡ(aʗ1, iΔ1), m);
            }
            doneʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < 64; i++) {
        ᐸꟷ(done);
    }
    // Check that the array has been totally set.
    foreach (var (i, v) in a) {
        if (v != 0xffffffffffffffffUL) {
            Ꮡt.Fatalf("a[%v] not fully set: want %#x, got %#x"u8, i, (uint64)0xffffffffffffffffUL, v);
        }
    }
}

public static void BenchmarkAnd32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new array<uint32>(128), out var Ꮡx);                       // give x its own cache line
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.And32(Ꮡx.at<uint32>(63), (uint32)i);
    }
}

public static void BenchmarkAnd32Parallel(ж<testing.B> Ꮡb) {
    ref var x = ref heap(new array<uint32>(128), out var Ꮡx);                       // give x its own cache line
    sink = Ꮡx;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var i = (uint32)0;
        while (pb.Next()) {
            atomic.And32(Ꮡx.at<uint32>(63), i);
            i++;
        }
    });
}

public static void BenchmarkAnd64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new array<uint64>(128), out var Ꮡx);                       // give x its own cache line
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.And64(Ꮡx.at<uint64>(63), (uint64)i);
    }
}

public static void BenchmarkAnd64Parallel(ж<testing.B> Ꮡb) {
    ref var x = ref heap(new array<uint64>(128), out var Ꮡx);                       // give x its own cache line
    sink = Ꮡx;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var i = (uint64)0;
        while (pb.Next()) {
            atomic.And64(Ꮡx.at<uint64>(63), i);
            i++;
        }
    });
}

public static void BenchmarkOr32(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new array<uint32>(128), out var Ꮡx);                       // give x its own cache line
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.Or32(Ꮡx.at<uint32>(63), (uint32)i);
    }
}

public static void BenchmarkOr32Parallel(ж<testing.B> Ꮡb) {
    ref var x = ref heap(new array<uint32>(128), out var Ꮡx);                       // give x its own cache line
    sink = Ꮡx;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var i = (uint32)0;
        while (pb.Next()) {
            atomic.Or32(Ꮡx.at<uint32>(63), i);
            i++;
        }
    });
}

public static void BenchmarkOr64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new array<uint64>(128), out var Ꮡx);                       // give x its own cache line
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.Or64(Ꮡx.at<uint64>(63), (uint64)i);
    }
}

public static void BenchmarkOr64Parallel(ж<testing.B> Ꮡb) {
    ref var x = ref heap(new array<uint64>(128), out var Ꮡx);                       // give x its own cache line
    sink = Ꮡx;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var i = (uint64)0;
        while (pb.Next()) {
            atomic.Or64(Ꮡx.at<uint64>(63), i);
            i++;
        }
    });
}

} // end atomic_test_package
