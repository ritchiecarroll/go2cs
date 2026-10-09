// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static runtime_package;
using strconv = strconv_package;
using testing = testing_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

public static void TestReadRandom(ж<testing.T> Ꮡt) {
    if (runtime_internal_test_package.ReadRandomFailed.Value) {
        var exprᴛ1 = GOOS;
        if (exprᴛ1 == "plan9"u8) {
        }
        else { /* default: */
            Ꮡt.Fatalf("readRandom failed at startup"u8);
        }

    }
}

// ok
public static void BenchmarkFastrand(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            runtime_internal_test_package.Fastrand();
        }
    });
}

public static void BenchmarkFastrand64(ж<testing.B> Ꮡb) {
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            runtime_internal_test_package.Fastrand64();
        }
    });
}

public static void BenchmarkFastrandHashiter(ж<testing.B> Ꮡb) {
    map<nint, nint> m = new map<nint, nint>(10);
    for (nint i = 0; i < 10; i++) {
        m[i] = i;
    }
    var mʗ1 = m;
    Ꮡb.RunParallel((ж<testing.PB> pb) => {
        while (pb.Next()) {
            foreach ((_, _) in mʗ1) {
                break;
            }
        }
    });
}

internal static uint32 sink32;

public static void BenchmarkFastrandn(ж<testing.B> Ꮡb) {
    for (var nᴛ1 = (uint32)2; nᴛ1 <= 5; nᴛ1++) {
        var n = nᴛ1;
        Ꮡb.Run(strconv.Itoa((nint)n), (ж<testing.B> bΔ1) => {
            for (nint i = 0; i < (~bΔ1).N; i++) {
                sink32 = runtime_internal_test_package.Fastrandn(n);
            }
        });
    }
}

//go:linkname fastrand runtime.fastrand
/*linkname*/ internal static partial uint32 fastrand() {
    return global::go.runtime_package.legacy_fastrand();
}

//go:linkname fastrandn runtime.fastrandn
/*linkname*/ internal static partial uint32 fastrandn(uint32 _) {
    return global::go.runtime_package.legacy_fastrandn(_);
}

//go:linkname fastrand64 runtime.fastrand64
/*linkname*/ internal static partial uint64 fastrand64() {
    return global::go.runtime_package.legacy_fastrand64();
}

public static void TestLegacyFastrand(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // Testing mainly that the calls work at all,
    // but check that all three don't return the same number (1 in 2^64 chance)
    {
        var (x, y, z) = (fastrand(), fastrand(), fastrand());
        if (x == y && y == z) {
            Ꮡt.Fatalf("fastrand three times = %#x, %#x, %#x, want different numbers"u8, x, y, z);
        }
    }
    {
        var (x, y, z) = (fastrandn(1000000000), fastrandn(1000000000), fastrandn(1000000000));
        if (x == y && y == z) {
            Ꮡt.Fatalf("fastrandn three times = %#x, %#x, %#x, want different numbers"u8, x, y, z);
        }
    }
    {
        var (x, y, z) = (fastrand64(), fastrand64(), fastrand64());
        if (x == y && y == z) {
            Ꮡt.Fatalf("fastrand64 three times = %#x, %#x, %#x, want different numbers"u8, x, y, z);
        }
    }
}

} // end runtime_test_package
