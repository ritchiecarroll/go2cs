// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build 386 || amd64 || arm || arm64 || loong64 || ppc64 || ppc64le
namespace go.@internal.runtime;

using atomic = go.@internal.runtime.atomic_package;
using testing = testing_package;
using go.@internal.runtime;

partial class atomic_test_package {

public static void TestXchg8(ж<testing.T> Ꮡt) {
    ref var a = ref heap(new array<uint8>(16), out var Ꮡa);
    foreach (var (i, _) in a) {
        var next = (uint8)(i + 50);
        a[i] = next;
    }
    var b = a.Clone();

    // Compare behavior against non-atomic implementation. Expect the operation
    // to work at any byte offset and to not clobber neighboring values.
    foreach (var (i, _) in a) {
        var next = (uint8)(i + 100);
        var pa = atomic.Xchg8(Ꮡa.at<uint8>(i), next);
        var pb = b[i];
        b[i] = next;
        if (pa != pb) {
            Ꮡt.Errorf("atomic.Xchg8(a[%d]); %d != %d"u8, i, pa, pb);
        }
        if (a != b) {
            Ꮡt.Errorf("after atomic.Xchg8(a[%d]); %d != %d"u8, i, a, b);
        }
        if (Ꮡt.Failed()) {
            break;
        }
    }
}

public static void BenchmarkXchg8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var x = ref heap(new array<uint8>(512), out var Ꮡx);                     // give byte its own cache line
    sink = Ꮡx;
    for (nint i = 0; i < b.N; i++) {
        atomic.Xchg8(Ꮡx.at<uint8>(255), (uint8)i);
    }
}

public static void BenchmarkXchg8Parallel(ж<testing.B> Ꮡb) {
    ref var x = ref heap(new array<uint8>(512), out var Ꮡx);                     // give byte its own cache line
    sink = Ꮡx;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        var i = (uint8)0;
        while (pb.Next()) {
            atomic.Xchg8(Ꮡx.at<uint8>(255), i);
            i++;
        }
    });
}

} // end atomic_test_package
