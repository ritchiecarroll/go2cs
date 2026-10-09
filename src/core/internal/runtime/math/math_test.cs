// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.runtime;

using static go.@internal.runtime.math_package;
using testing = testing_package;

partial class math_test_package {

public static UntypedInt UintptrSize => /* 32 << (^uintptr(0) >> 63) */ 64;

partial struct mulUintptrTest {
    internal uintptr a;
    internal uintptr b;
    internal bool overflow;
}

internal static slice<mulUintptrTest> mulUintptrTests = new mulUintptrTest[]{
    new(0, 0, false),
    new(1000, 1000, false),
    new(MaxUintptr, 0, false),
    new(MaxUintptr, 1, false),
    new(MaxUintptr / 2, 2, false),
    new(MaxUintptr / 2, 3, true),
    new(MaxUintptr, 10, true),
    new(MaxUintptr, 100, true),
    new(MaxUintptr / 100, 100, false),
    new(MaxUintptr / 1000, 1001, true),
    new((uintptr)(4294967296L - 1), (uintptr)(4294967296L - 1), false),
    new(((uintptr)1 << (int)((UintptrSize / 2))), ((uintptr)1 << (int)((UintptrSize / 2))), true),
    new((MaxUintptr >> (int)(32)), (MaxUintptr >> (int)(32)), false),
    new(MaxUintptr, MaxUintptr, true)
}.slice();

public static void TestMulUintptr(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in mulUintptrTests) {
        var (a, b) = (test.a, test.b);
        for (nint i = 0; i < 2; i++) {
            var (mul, overflow) = MulUintptr(a, b);
            if (mul != a * b || overflow != test.overflow) {
                Ꮡt.Errorf("MulUintptr(%v, %v) = %v, %v want %v, %v"u8,
                    a, b, mul, overflow, a * b, test.overflow);
            }
            (a, b) = (b, a);
        }
    }
}

public static uintptr SinkUintptr;

public static bool SinkBool;

internal static uintptr x;
internal static uintptr y;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string smallˢ = "small"u8;
private static readonly @string largeˢ = "large"u8;

public static void BenchmarkMulUintptr(ж<testing.B> Ꮡb) {
    (x, y) = (1, 2);
    Ꮡb.Run(smallˢ, (ж<testing.B> bΔ1) => {
        for (nint i = 0; i < (~bΔ1).N; i++) {
            bool overflow = default!;
            (SinkUintptr, overflow) = MulUintptr(x, y);
            if (overflow) {
                SinkUintptr = 0;
            }
        }
    });
    (x, y) = (MaxUintptr, MaxUintptr - 1);
    Ꮡb.Run(largeˢ, (ж<testing.B> bΔ2) => {
        for (nint i = 0; i < (~bΔ2).N; i++) {
            bool overflow = default!;
            (SinkUintptr, overflow) = MulUintptr(x, y);
            if (overflow) {
                SinkUintptr = 0;
            }
        }
    });
}

} // end math_test_package
