// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using Δmath = math_package;
using rand = global::go.math.rand_package;
using static runtime_package;
using testing = testing_package;
using global::go.math;
using static global::go.runtime_internal_test_package;
using ꓸꓸꓸany = Span<any>;

partial class runtime_test_package {

// turn uint64 op into float64 op
internal static Func<float64, float64, float64> fop(Func<uint64, uint64, uint64> f) {
    return (float64 x, float64 y) => {
        var bx = Δmath.Float64bits(x);
        var by = Δmath.Float64bits(y);
        return Δmath.Float64frombits(f(bx, by));
    };
}

internal static float64 add(float64 x, float64 y) {
    return x + y;
}

internal static float64 sub(float64 x, float64 y) {
    return x - y;
}

internal static float64 mul(float64 x, float64 y) {
    return x * y;
}

internal static float64 div(float64 x, float64 y) {
    return x / y;
}

public static void TestFloat64(ж<testing.T> Ꮡt) {
    var @base = new float64[]{
        0D,
        Δmath.Copysign(0D, -1D),
        -1D,
        1D,
        Δmath.NaN(),
        Δmath.Inf(+1),
        Δmath.Inf(-1),
        0.1D,
        1.5D,
        1.9999999999999998D, // all 1s mantissa

        1.3333333333333333D, // 1.010101010101...

        1.1428571428571428D, // 1.001001001001...

        1.112536929253601e-308D, // first normal

        2D,
        4D,
        8D,
        16D,
        32D,
        64D,
        128D,
        256D,
        3D,
        12D,
        1234D,
        123456D,
        -0.1D,
        -1.5D,
        -1.9999999999999998D,
        -1.3333333333333333D,
        -1.1428571428571428D,
        -2D,
        -3D,
        1e-200D,
        1e-300D,
        1e-310D,
        5e-324D,
        1e-105D,
        1e-305D,
        1e+200D,
        1e+306D,
        1e+307D,
        1e+308D
    }.slice();
    var all = new slice<float64>(200);
    copy(all, @base);
    for (nint i = len(@base); i < len(all); i++) {
        all[i] = rand.NormFloat64();
    }
    test(Ꮡt, "+"u8, add, fop(runtime_internal_test_package.Fadd64), all);
    test(Ꮡt, "-"u8, sub, fop(runtime_internal_test_package.Fsub64), all);
    if (GOARCH != "386"u8) {
        // 386 is not precise!
        test(Ꮡt, "*"u8, mul, fop(runtime_internal_test_package.Fmul64), all);
        test(Ꮡt, "/"u8, div, fop(runtime_internal_test_package.Fdiv64), all);
    }
}

// 64 -hw-> 32 -hw-> 64
internal static float64 trunc32(float64 f) {
    return (float64)(float32)f;
}

// 64 -sw->32 -hw-> 64
internal static float64 to32sw(float64 f) {
    return (float64)Δmath.Float32frombits(runtime_internal_test_package.F64to32(Δmath.Float64bits(f)));
}

// 64 -hw->32 -sw-> 64
internal static float64 to64sw(float64 f) {
    return Δmath.Float64frombits(runtime_internal_test_package.F32to64(Δmath.Float32bits((float32)f)));
}

// float64 -hw-> int64 -hw-> float64
internal static float64 hwint64(float64 f) {
    return (float64)(int64)f;
}

// float64 -hw-> int32 -hw-> float64
internal static float64 hwint32(float64 f) {
    return (float64)(int32)f;
}

// float64 -sw-> int64 -hw-> float64
internal static float64 toint64sw(float64 f) {
    var (i, ok) = runtime_internal_test_package.F64toint(Δmath.Float64bits(f));
    if (!ok) {
        // There's no right answer for out of range.
        // Match the hardware to pass the test.
        i = (int64)f;
    }
    return (float64)i;
}

// float64 -hw-> int64 -sw-> float64
internal static float64 fromint64sw(float64 f) {
    return Δmath.Float64frombits(runtime_internal_test_package.Fintto64((int64)f));
}

internal static nint nerr;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object tooManyErrorsˢ = (@string)"too many errors"u8;

internal static void err(ж<testing.T> Ꮡt, @string format, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    Ꮡt.Errorf(format, args.ꓸꓸꓸ);
    // cut errors off after a while.
    // otherwise we spend all our time
    // allocating memory to hold the
    // formatted output.
    {
        runtime_test_package.nerr++; if (runtime_test_package.nerr >= 10) {
            Ꮡt.Fatal(tooManyErrorsˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string gSGSwGHwGˢ = "%g %s %g = sw %g, hw %g\n"u8;
internal static readonly @string to32ˢ = "to32"u8;
internal static readonly @string to64ˢ = "to64"u8;
internal static readonly @string toint64ˢ = "toint64"u8;
internal static readonly @string fromint64ˢ = "fromint64"u8;

internal static void test(ж<testing.T> Ꮡt, @string op, Func<float64, float64, float64> hw, Func<float64, float64, float64> sw, slice<float64> all) {
    foreach (var (_, f) in all) {
        foreach (var (_, g) in all) {
            var h = hw(f, g);
            var s = sw(f, g);
            if (!same(h, s)) {
                err(Ꮡt, gSGSwGHwGˢ, f, op, g, s, h);
            }
            testu(Ꮡt, to32ˢ, trunc32, to32sw, h);
            testu(Ꮡt, to64ˢ, trunc32, to64sw, h);
            testu(Ꮡt, toint64ˢ, hwint64, toint64sw, h);
            testu(Ꮡt, fromint64ˢ, hwint64, fromint64sw, h);
            testcmp(Ꮡt, f, h);
            testcmp(Ꮡt, h, f);
            testcmp(Ꮡt, g, h);
            testcmp(Ꮡt, h, g);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string sGSwGHwGˢ = "%s %g = sw %g, hw %g\n"u8;

internal static void testu(ж<testing.T> Ꮡt, @string op, Func<float64, float64> hw, Func<float64, float64> sw, float64 v) {
    var h = hw(v);
    var s = sw(v);
    if (!same(h, s)) {
        err(Ꮡt, sGSwGHwGˢ, op, v, s, h);
    }
}

internal static (nint cmp, bool isnan) hwcmp(float64 f, float64 g) {
    switch (ᐧ) {
    case {} when f < g: {
        return (-1, false);
    }
    case {} when f > g: {
        return (+1, false);
    }
    case {} when f == g: {
        return (0, false);
    }}

    return (0, true); // must be NaN
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cmpGGSwVVHwVVˢ = "cmp(%g, %g) = sw %v, %v, hw %v, %v\n"u8;

internal static void testcmp(ж<testing.T> Ꮡt, float64 f, float64 g) {
    var (hcmp, hisnan) = hwcmp(f, g);
    var (scmp, sisnan) = runtime_internal_test_package.Fcmp64(Δmath.Float64bits(f), Δmath.Float64bits(g));
    if ((int32)hcmp != scmp || hisnan != sisnan) {
        err(Ꮡt, cmpGGSwVVHwVVˢ, f, g, scmp, sisnan, hcmp, hisnan);
    }
}

internal static bool same(float64 f, float64 g) {
    if (Δmath.IsNaN(f) && Δmath.IsNaN(g)) {
        return true;
    }
    if (Δmath.Copysign(1D, f) != Δmath.Copysign(1D, g)) {
        return false;
    }
    return f == g;
}

} // end runtime_test_package
