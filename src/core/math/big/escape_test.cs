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
internal static readonly @string buildˢ = "build"u8;
internal static readonly @string gcflagsMˢ = "-gcflags=-m"u8;
internal static readonly @string natmulGoˢ = "natmul.go"u8;
internal static readonly @string intˢ = "Int"u8;
internal static readonly @string escapesˢ = "escapes"u8;

public static void TestEscape(ж<testing.T> Ꮡt) {
    testenv.MustHaveGoBuild(new big_test_package.testing_TжTB(Ꮡt));
    // The multiplication routines create many temporary Int values,
    // expecting them to be stack-allocated. Make sure none escape to the heap.
    var (@out, err) = exec.Command("go"u8, buildˢ, gcflagsMˢ).CombinedOutput();
    if (err != default!) {
        Ꮡt.Fatalf("go build -gcflags=-m: %v\n%s"u8, err, @out);
    }
    foreach (var line in range<@string>(strings.Lines((@string)@out).Invoke)) {
        if (strings.Contains(line, natmulGoˢ) && strings.Contains(line, intˢ) && strings.Contains(line, escapesˢ)) {
            Ꮡt.Error(strings.TrimSpace(line));
        }
    }
}

public static void TestMulAlloc(ж<testing.T> Ꮡt) {
    var r = Δrand.New(Δrand.NewSource(1234));
    var sizes = new nint[]{karatsubaThreshold / 2, karatsubaThreshold}.slice();
    foreach (var (_, size) in sizes) {
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
                Ꮡt.Errorf("Mul(len %d, len %d) allocates %.2f objects"u8, size, size, n);
            }
        }
    }
}

public static void TestSqrAlloc(ж<testing.T> Ꮡt) {
    var r = Δrand.New(Δrand.NewSource(1234));
    var sizes = new nint[]{basicSqrThreshold / 2, basicSqrThreshold, karatsubaSqrThreshold}.slice();
    foreach (var (_, size) in sizes) {
        var x = randInt(r, (nuint)size);
        var z = Ꮡ(new ΔInt(abs: new global::go.math.big_package.Δnat((nint)(2 * (nuint)size))));
        {
            var xʗ1 = x;
            var zʗ1 = z;
            var n = testing.AllocsPerRun(10, () => {
                zʗ1.Mul(xʗ1, xʗ1);
            }); if (n >= 1D) {
                Ꮡt.Errorf("Mul(len %d with itself) allocates %.2f objects"u8, size, n);
            }
        }
    }
}

} // end big_internal_test_package
