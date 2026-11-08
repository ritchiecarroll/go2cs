// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using atomic = go.@internal.runtime.atomic_package;
using testing = testing_package;
using go.@internal.runtime;

partial class atomic_test_package {

internal static any sink;

public static void BenchmarkAtomicLoad64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new uint64(), out var Ꮡx);
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        _ = atomic.Load64(Ꮡx);
    }
}

public static void BenchmarkAtomicStore64(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new uint64(), out var Ꮡx);
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.Store64(Ꮡx, 0);
    }
}

public static void BenchmarkAtomicLoad(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new uint32(), out var Ꮡx);
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        _ = atomic.Load(Ꮡx);
    }
}

public static void BenchmarkAtomicStore(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new uint32(), out var Ꮡx);
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.Store(Ꮡx, 0);
    }
}

public static void BenchmarkAtomicLoad8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new uint8(), out var Ꮡx);
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.Load8(Ꮡx);
    }
}

public static void BenchmarkAtomicStore8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new uint8(), out var Ꮡx);
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.Store8(Ꮡx, 0);
    }
}

public static void BenchmarkAnd8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new array<uint8>(512), out var Ꮡx); // give byte its own cache line
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.And8(Ꮡx.at<uint8>(255), (uint8)i);
    }
}

public static void BenchmarkAnd(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new array<uint32>(128), out var Ꮡx); // give x its own cache line
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.And(Ꮡx.at<uint32>(63), (uint32)i);
    }
}

public static void BenchmarkAnd8Parallel(ж<testing.B> b) {
    ref var x = ref heap(new array<uint8>(512), out var Ꮡx); // give byte its own cache line
    sink = Ꮡx;
    b.RunParallel((ж<testing.PB> pb) => {
        var i = (uint8)0;
        while (pb.Next()) {
            atomic.And8(Ꮡx.at<uint8>(255), i);
            i++;
        }
    });
}

public static void BenchmarkAndParallel(ж<testing.B> b) {
    ref var x = ref heap(new array<uint32>(128), out var Ꮡx); // give x its own cache line
    sink = Ꮡx;
    b.RunParallel((ж<testing.PB> pb) => {
        var i = (uint32)0;
        while (pb.Next()) {
            atomic.And(Ꮡx.at<uint32>(63), i);
            i++;
        }
    });
}

public static void BenchmarkOr8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new array<uint8>(512), out var Ꮡx); // give byte its own cache line
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.Or8(Ꮡx.at<uint8>(255), (uint8)i);
    }
}

public static void BenchmarkOr(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new array<uint32>(128), out var Ꮡx); // give x its own cache line
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.Or(Ꮡx.at<uint32>(63), (uint32)i);
    }
}

public static void BenchmarkOr8Parallel(ж<testing.B> b) {
    ref var x = ref heap(new array<uint8>(512), out var Ꮡx); // give byte its own cache line
    sink = Ꮡx;
    b.RunParallel((ж<testing.PB> pb) => {
        var i = (uint8)0;
        while (pb.Next()) {
            atomic.Or8(Ꮡx.at<uint8>(255), i);
            i++;
        }
    });
}

public static void BenchmarkOrParallel(ж<testing.B> b) {
    ref var x = ref heap(new array<uint32>(128), out var Ꮡx); // give x its own cache line
    sink = Ꮡx;
    b.RunParallel((ж<testing.PB> pb) => {
        var i = (uint32)0;
        while (pb.Next()) {
            atomic.Or(Ꮡx.at<uint32>(63), i);
            i++;
        }
    });
}

public static void BenchmarkXadd(ж<testing.B> b) {
    ref var x = ref heap(new uint32(), out var Ꮡx);
    var ptr = Ꮡx;
    var ptrʗ1 = ptr;
    b.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            atomic.Xadd(ptrʗ1, 1);
        }
    });
}

public static void BenchmarkXadd64(ж<testing.B> b) {
    ref var x = ref heap(new uint64(), out var Ꮡx);
    var ptr = Ꮡx;
    var ptrʗ1 = ptr;
    b.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            atomic.Xadd64(ptrʗ1, 1);
        }
    });
}

public static void BenchmarkCas(ж<testing.B> b) {
    ref var x = ref heap(new uint32(), out var Ꮡx);
    x = 1;
    var ptr = Ꮡx;
    var ptrʗ1 = ptr;
    b.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            atomic.Cas(ptrʗ1, 1, 0);
            atomic.Cas(ptrʗ1, 0, 1);
        }
    });
}

public static void BenchmarkCas64(ж<testing.B> b) {
    ref var x = ref heap(new uint64(), out var Ꮡx);
    x = 1;
    var ptr = Ꮡx;
    var ptrʗ1 = ptr;
    b.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            atomic.Cas64(ptrʗ1, 1, 0);
            atomic.Cas64(ptrʗ1, 0, 1);
        }
    });
}
public static void BenchmarkXchg(ж<testing.B> b) {
    ref var x = ref heap(new uint32(), out var Ꮡx);
    x = 1;
    var ptr = Ꮡx;
    var ptrʗ1 = ptr;
    b.RunParallel((ж<testing.PB> pb) => {
        uint32 y = default;
        y = 1;
        while (pb.Next()) {
            y = atomic.Xchg(ptrʗ1, y);
            y += 1;
        }
    });
}

public static void BenchmarkXchg64(ж<testing.B> b) {
    ref var x = ref heap(new uint64(), out var Ꮡx);
    x = 1;
    var ptr = Ꮡx;
    var ptrʗ1 = ptr;
    b.RunParallel((ж<testing.PB> pb) => {
        uint64 y = default;
        y = 1;
        while (pb.Next()) {
            y = atomic.Xchg64(ptrʗ1, y);
            y += 1;
        }
    });
}

} // end atomic_test_package
