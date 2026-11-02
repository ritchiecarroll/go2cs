// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements a GCD benchmark.
// Usage: go test math/big -test.bench GCD
namespace go.math;

using Δrand = go.math.rand_package;
using testing = testing_package;
using go.math;
using static go.math.big_package;

partial class big_internal_test_package {

// randInt returns a pseudo-random Int in the range [1<<(size-1), (1<<size) - 1]
internal static ж<global::go.math.big_package.ΔInt> randInt(ж<Δrand.Rand> r, nuint size) {
    var n = @new<global::go.math.big_package.ΔInt>().Lsh(intOne, size - 1);
    var x = @new<global::go.math.big_package.ΔInt>().Rand(r, n);
    return x.Add(x, n); // make sure result > 1<<(size-1)
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object skippingOnRaceBuilderˢ = (@string)"skipping on race builder"u8;
internal static ref readonly @string withoutXYˢ => ref withoutXYˢᵇ;
private static readonly @string withoutXYˢᵇ = "WithoutXY"u8;
internal static ref readonly @string withXYˢ => ref withXYˢᵇ;
private static readonly @string withXYˢᵇ = "WithXY"u8;

internal static void runGCD(ж<testing.B> b, nuint aSize, nuint bSize) {
    if (isRaceBuilder && (aSize > 1000 || bSize > 1000)) {
        b.Skip(skippingOnRaceBuilderˢ);
    }
    b.Run(withoutXYˢ, (ж<testing.B> bΔ1) => {
        runGCDExt(bΔ1, aSize, bSize, false);
    });
    b.Run(withXYˢ, (ж<testing.B> bΔ2) => {
        runGCDExt(bΔ2, aSize, bSize, true);
    });
}

internal static void runGCDExt(ж<testing.B> Ꮡb, nuint aSize, nuint bSize, bool calcXY) {
    ref var b = ref Ꮡb.DerefOrNull();

    b.StopTimer();
    ж<Δrand.Rand> r = Δrand.New(Δrand.NewSource(1234));
    var aa = randInt(r, aSize);
    var bb = randInt(r, bSize);
    ж<global::go.math.big_package.ΔInt> x = default;
    ж<global::go.math.big_package.ΔInt> y = default;
    if (calcXY) {
        x = @new<global::go.math.big_package.ΔInt>();
        y = @new<global::go.math.big_package.ΔInt>();
    }
    b.StartTimer();
    for (nint i = 0; i < b.N; i++) {
        @new<global::go.math.big_package.ΔInt>().GCD(x, y, aa, bb);
    }
}

public static void BenchmarkGCD10x10(ж<testing.B> b) {
    runGCD(b, 10, 10);
}

public static void BenchmarkGCD10x100(ж<testing.B> b) {
    runGCD(b, 10, 100);
}

public static void BenchmarkGCD10x1000(ж<testing.B> b) {
    runGCD(b, 10, 1000);
}

public static void BenchmarkGCD10x10000(ж<testing.B> b) {
    runGCD(b, 10, 10000);
}

public static void BenchmarkGCD10x100000(ж<testing.B> b) {
    runGCD(b, 10, 100000);
}

public static void BenchmarkGCD100x100(ж<testing.B> b) {
    runGCD(b, 100, 100);
}

public static void BenchmarkGCD100x1000(ж<testing.B> b) {
    runGCD(b, 100, 1000);
}

public static void BenchmarkGCD100x10000(ж<testing.B> b) {
    runGCD(b, 100, 10000);
}

public static void BenchmarkGCD100x100000(ж<testing.B> b) {
    runGCD(b, 100, 100000);
}

public static void BenchmarkGCD1000x1000(ж<testing.B> b) {
    runGCD(b, 1000, 1000);
}

public static void BenchmarkGCD1000x10000(ж<testing.B> b) {
    runGCD(b, 1000, 10000);
}

public static void BenchmarkGCD1000x100000(ж<testing.B> b) {
    runGCD(b, 1000, 100000);
}

public static void BenchmarkGCD10000x10000(ж<testing.B> b) {
    runGCD(b, 10000, 10000);
}

public static void BenchmarkGCD10000x100000(ж<testing.B> b) {
    runGCD(b, 10000, 100000);
}

public static void BenchmarkGCD100000x100000(ж<testing.B> b) {
    runGCD(b, 100000, 100000);
}

} // end big_internal_test_package
