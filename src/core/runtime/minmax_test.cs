// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δmath = math_package;
using strings = strings_package;
using testing = testing_package;
using @unsafe = unsafe_package;
using static global::go.runtime_internal_test_package;

partial class runtime_test_package {

internal static float64 zero = Δmath.Copysign(0D, +1D);
internal static float64 negZero = Δmath.Copysign(0D, -1D);
internal static float64 inf = Δmath.Inf(+1);
internal static float64 negInf = Δmath.Inf(-1);
internal static float64 nan = Δmath.NaN();

partial struct testsᴛ1 /*dyn*/ {
    internal float64 min, max;
}
internal static slice<testsᴛ1> tests = [
    new(1D, 2D),
    new(-2D, 1D),
    new(negZero, zero),
    new(zero, inf),
    new(negInf, zero),
    new(negInf, inf),
    new(1D, inf),
    new(negInf, 1D)
];

internal static slice<float64> all = [1D, 2D, -1D, -2D, zero, negZero, inf, negInf, nan];

internal static bool eq(float64 x, float64 y) {
    return x == y && Δmath.Signbit(x) == Δmath.Signbit(y);
}

public static void TestMinFloat(ж<testing.T> t) {
    foreach (var (_, tt) in range(tests)) {
        {
            var z = builtin.min(tt.min, tt.max); if (!eq(z, tt.min)) {
                t.Errorf("min(%v, %v) = %v, want %v", tt.min, tt.max, z, tt.min);
            }
        }
        {
            var z = builtin.min(tt.max, tt.min); if (!eq(z, tt.min)) {
                t.Errorf("min(%v, %v) = %v, want %v", tt.max, tt.min, z, tt.min);
            }
        }
    }
    foreach (var (_, x) in range(all)) {
        {
            var z = builtin.min(nan, x); if (!Δmath.IsNaN(z)) {
                t.Errorf("min(%v, %v) = %v, want %v", nan, x, z, nan);
            }
        }
        {
            var z = builtin.min(x, nan); if (!Δmath.IsNaN(z)) {
                t.Errorf("min(%v, %v) = %v, want %v", nan, x, z, nan);
            }
        }
    }
}

public static void TestMaxFloat(ж<testing.T> t) {
    foreach (var (_, tt) in range(tests)) {
        {
            var z = builtin.max(tt.min, tt.max); if (!eq(z, tt.max)) {
                t.Errorf("max(%v, %v) = %v, want %v", tt.min, tt.max, z, tt.max);
            }
        }
        {
            var z = builtin.max(tt.max, tt.min); if (!eq(z, tt.max)) {
                t.Errorf("max(%v, %v) = %v, want %v", tt.max, tt.min, z, tt.max);
            }
        }
    }
    foreach (var (_, x) in range(all)) {
        {
            var z = builtin.max(nan, x); if (!Δmath.IsNaN(z)) {
                t.Errorf("max(%v, %v) = %v, want %v", nan, x, z, nan);
            }
        }
        {
            var z = builtin.max(x, nan); if (!Δmath.IsNaN(z)) {
                t.Errorf("max(%v, %v) = %v, want %v", nan, x, z, nan);
            }
        }
    }
}

// testMinMax tests that min/max behave correctly on every pair of
// values in vals.
//
// vals should be a sequence of values in strictly ascending order.
internal static void testMinMax<T>(ж<testing.T> t, params Span<T> valsʗp)
    where T : /* int | uint8 | string */ IAdditionOperators<T, T, T>, IEqualityOperators<T, T, bool>, IComparisonOperators<T, T, bool>, new()
{
    var vals = valsʗp.slice();

    foreach (var (i, x) in range(vals)) {
        foreach (var (_, y) in range(vals.slice(i + 1))) {
            if (!(x < y)) {
                t.Fatalf("values out of order: !(%v < %v)", x, y);
            }

            {
                var z = builtin.min(x, y); if (!AreEqual(z, x)) {
                    t.Errorf("min(%v, %v) = %v, want %v", x, y, z, x);
                }
            }
            {
                var z = builtin.min(y, x); if (!AreEqual(z, x)) {
                    t.Errorf("min(%v, %v) = %v, want %v", y, x, z, x);
                }
            }

            {
                var z = builtin.max(x, y); if (!AreEqual(z, y)) {
                    t.Errorf("max(%v, %v) = %v, want %v", x, y, z, y);
                }
            }
            {
                var z = builtin.max(y, x); if (!AreEqual(z, y)) {
                    t.Errorf("max(%v, %v) = %v, want %v", y, x, z, y);
                }
            }
        }
    }
}

public static void TestMinMaxInt(ж<testing.T> t) { testMinMax<nint>(t, -7, 0, 9); }
public static void TestMinMaxUint8(ж<testing.T> t) { testMinMax<uint8>(t, 0, 1, 2, 4, 7); }
public static void TestMinMaxString(ж<testing.T> t) { testMinMax<@string>(t, (@string)"a", (@string)"b", (@string)"c"); }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string xxxˢ => ref xxxˢᵇ;
private static readonly @string xxxˢᵇ = "xxx"u8;

// TestMinMaxStringTies ensures that min(a, b) returns a when a == b.
public static void TestMinMaxStringTies(ж<testing.T> t) {
    @string s = xxxˢ;
    var x = strings.Split(s, ""u8);

    var xʗ1 = x;
    void test(nint i, nint j, nint k) {
        {
            @string z = builtin.min(xʗ1[i], xʗ1[j], xʗ1[k]); if (@unsafe.StringData(z) != @unsafe.StringData(xʗ1[i])) {
                t.Errorf("min(x[%v], x[%v], x[%v]) = %p, want %p", i, j, k, @unsafe.StringData(z).OrTypedNil(), @unsafe.StringData(xʗ1[i]).OrTypedNil());
            }
        }
        {
            @string z = builtin.max(xʗ1[i], xʗ1[j], xʗ1[k]); if (@unsafe.StringData(z) != @unsafe.StringData(xʗ1[i])) {
                t.Errorf("max(x[%v], x[%v], x[%v]) = %p, want %p", i, j, k, @unsafe.StringData(z).OrTypedNil(), @unsafe.StringData(xʗ1[i]).OrTypedNil());
            }
        }
    }

    test(0, 1, 2);
    test(0, 2, 1);
    test(1, 0, 2);
    test(1, 2, 0);
    test(2, 0, 1);
    test(2, 1, 0);
}

public static void BenchmarkMinFloat(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    float64 m = 0D;
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, f) in range(all)) {
            m = builtin.min(m, f);
        }
    }
}

public static void BenchmarkMaxFloat(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    float64 m = 0D;
    for (nint i = 0; i < b.N; i++) {
        foreach (var (_, f) in range(all)) {
            m = builtin.max(m, f);
        }
    }
}

} // end runtime_test_package
