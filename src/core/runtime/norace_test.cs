// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// The file contains tests that cannot run under race detector for some reason.
//
//go:build !race
namespace go;

using Δruntime = runtime_package;
using testing = testing_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

// Syscall tests split stack between Entersyscall and Exitsyscall under race detector.
public static void BenchmarkSyscall(ж<testing.B> b) {
    benchmarkSyscall(b, 0, 1);
}

public static void BenchmarkSyscallWork(ж<testing.B> b) {
    benchmarkSyscall(b, 100, 1);
}

public static void BenchmarkSyscallExcess(ж<testing.B> b) {
    benchmarkSyscall(b, 0, 4);
}

public static void BenchmarkSyscallExcessWork(ж<testing.B> b) {
    benchmarkSyscall(b, 100, 4);
}

internal static void benchmarkSyscall(ж<testing.B> Ꮡb, nint work, nint excess) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.SetParallelism(excess);
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        nint foo = 42;
        while (pb.Next()) {
            runtime_internal_test_package.Entersyscall();
            for (nint i = 0; i < work; i++) {
                foo *= 2;
                foo /= 2;
            }
            runtime_internal_test_package.Exitsyscall();
        }
        _ = foo;
    });
}

} // end runtime_test_package
