// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using testenv = @internal.testenv_package;
using Δrand = go.math.rand_package;
using exec = go.os.exec_package;
using strings = strings_package;
using testing = testing_package;
using @internal;
using go.math;
using go.os;
using iter = iter_package;
using static go.math.big_package;

partial class big_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;
internal static ref readonly @string gcflagsMˢ => ref gcflagsMˢᵇ;
private static readonly @string gcflagsMˢᵇ = "-gcflags=-m"u8;
internal static ref readonly @string natmulGoˢ => ref natmulGoˢᵇ;
private static readonly @string natmulGoˢᵇ = "natmul.go"u8;
internal static ref readonly @string intˢ => ref intˢᵇ;
private static readonly @string intˢᵇ = "Int"u8;
internal static ref readonly @string escapesˢ => ref escapesˢᵇ;
private static readonly @string escapesˢᵇ = "escapes"u8;

public static void TestEscape(ж<testing.T> t) {
    testenv.MustHaveGoBuild(new big_test_package.testing_TжTB(t));
    // The multiplication routines create many temporary Int values,
    // expecting them to be stack-allocated. Make sure none escape to the heap.
    var (@out, err) = exec.Command("go", buildˢ, gcflagsMˢ).CombinedOutput();
    if (err != nil) {
        t.Fatalf("go build -gcflags=-m: %v\n%s", err, @out);
    }
    foreach (var line in range<@string>(strings.Lines((@string)@out).Invoke)) {
        if (strings.Contains(line, natmulGoˢ) && strings.Contains(line, intˢ) && strings.Contains(line, escapesˢ)) {
            t.Error(strings.TrimSpace(line));
        }
    }
}

public static void TestMulAlloc(ж<testing.T> t) {
    var r = Δrand.New(Δrand.NewSource(1234));
    var sizes = slice<nint>([karatsubaThreshold / 2, karatsubaThreshold]);
    foreach (var (_, size) in range(sizes)) {
        var x = randInt(r, (nuint)size);
        var y = randInt(r, (nuint)size);
        var z = Ꮡ(new ΔInt(abs: new global::go.math.big_package.Δnat((nint)(2 * (nuint)size))));
        {
            var xʗ1 = x;
            var yʗ1 = y;
            var zʗ1 = z;
            var n = testing.AllocsPerRun(10, () => {
                zʗ1.Mul(xʗ1, yʗ1);
            }); if (n >= 1D) {
                t.Errorf("Mul(len %d, len %d) allocates %.2f objects", size, size, n);
            }
        }
    }
}

public static void TestSqrAlloc(ж<testing.T> t) {
    var r = Δrand.New(Δrand.NewSource(1234));
    var sizes = slice<nint>([basicSqrThreshold / 2, basicSqrThreshold, karatsubaSqrThreshold]);
    foreach (var (_, size) in range(sizes)) {
        var x = randInt(r, (nuint)size);
        var z = Ꮡ(new ΔInt(abs: new global::go.math.big_package.Δnat((nint)(2 * (nuint)size))));
        {
            var xʗ1 = x;
            var zʗ1 = z;
            var n = testing.AllocsPerRun(10, () => {
                zʗ1.Mul(xʗ1, xʗ1);
            }); if (n >= 1D) {
                t.Errorf("Mul(len %d with itself) allocates %.2f objects", size, n);
            }
        }
    }
}

} // end big_internal_test_package
