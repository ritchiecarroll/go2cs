// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using flag = flag_package;
using fmt = fmt_package;
using math = math_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using static go.math.big_package;

partial class big_internal_test_package {

// Verify that ErrNaN implements the error interface.
internal static error _ᴛ1ʗ = new big_test_package.big_ErrNaNᴠerror(new ErrNaN());

internal static uint64 uint64(this ж<global::go.math.big_package.Float> Ꮡx) {
    var (u, acc) = Ꮡx.Uint64();
    if (acc != Exact) {
        throw panic(fmt.Sprintf("%s is not a uint64"u8, Ꮡx.Text((byte)'g', 10)));
    }
    return u;
}

internal static int64 int64(this ж<global::go.math.big_package.Float> Ꮡx) {
    var (i, acc) = Ꮡx.Int64();
    if (acc != Exact) {
        throw panic(fmt.Sprintf("%s is not an int64"u8, Ꮡx.Text((byte)'g', 10)));
    }
    return i;
}

internal partial struct TestFloatZeroValue_type /*dyn*/ {
    internal nint z, x, y, want;
    internal rune opname;
    internal Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>> op;
}

public static void TestFloatZeroValue(ж<testing.T> t) {
    // zero (uninitialized) value is a ready-to-use 0.0
    ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
    {
        @string s = Ꮡx.Text((byte)'f', 1); if (s != "0.0"u8) {
            t.Errorf("zero value = %s; want 0.0", s);
        }
    }

    // zero value has precision 0
    {
        nuint prec = x.Prec(); if (prec != 0) {
            t.Errorf("prec = %d; want 0", prec);
        }
    }

    // zero value can be used in any and all positions of binary operations
    ж<global::go.math.big_package.Float> make(nint xΔ1) {
        ref var f = ref heap(new global::go.math.big_package.Float(), out var Ꮡf);
        if (xΔ1 != 0) {
            Ꮡf.SetInt64((int64)xΔ1);
        }
        // x == 0 translates into the zero value
        return Ꮡf;
    }
    foreach (var (_, test) in slice<TestFloatZeroValue_type>([
        new(0, 0, 0, 0, '+', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Add))),
        new(0, 1, 2, 3, '+', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Add))),
        new(1, 2, 0, 2, '+', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Add))),
        new(2, 0, 1, 1, '+', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Add))),

        new(0, 0, 0, 0, '-', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Sub))),
        new(0, 1, 2, -1, '-', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Sub))),
        new(1, 2, 0, 2, '-', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Sub))),
        new(2, 0, 1, -1, '-', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Sub))),

        new(0, 0, 0, 0, '*', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Mul))),
        new(0, 1, 2, 2, '*', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Mul))),
        new(1, 2, 0, 0, '*', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Mul))),
        new(2, 0, 1, 0, '*', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Mul))),

        // {0, 0, 0, 0, '/', (*Float).Quo}, // panics
        new(0, 2, 1, 2, '/', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Quo))),
        new(1, 2, 0, 0, '/', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Quo))), // = +Inf
        new(2, 0, 1, 0, '/', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Quo)))
    ])) {
        var z = make(test.z);
        test.op(z, make(test.x), make(test.y));
        nint got = 0;
        if (!z.IsInf()) {
            got = (nint)z.int64();
        }
        if (got != test.want) {
            t.Errorf("%d %c %d = %d; want %d", test.x, test.opname, test.y, got, test.want);
        }
    }

    // TODO(gri) test how precision is set for zero value results
}

internal static ж<global::go.math.big_package.Float> makeFloat(@string s) {
    var (x, _, err) = ParseFloat(s, 0, 1000, ToNearestEven);
    if (err != nil) {
        throw panic(err);
    }
    return x;
}

internal partial struct TestFloatSetPrec_type /*dyn*/ {
    internal @string x;
    internal nuint prec;
    internal @string want;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatSetPrec(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatSetPrec_type>([
        // prec 0
        new("0", 0, "0", Exact),
        new("-0", 0, "-0", Exact),
        new("-Inf", 0, "-Inf", Exact),
        new("+Inf", 0, "+Inf", Exact),
        new("123", 0, "0", Below),
        new("-123", 0, "-0", Above),

        // prec at upper limit
        new("0", MaxPrec, "0", Exact),
        new("-0", MaxPrec, "-0", Exact),
        new("-Inf", MaxPrec, "-Inf", Exact),
        new("+Inf", MaxPrec, "+Inf", Exact),

        // just a few regular cases - general rounding is tested elsewhere
        new("1.5", 1, "2", Above),
        new("-1.5", 1, "-2", Below),
        new("123", 1000000, "123", Exact),
        new("-123", 1000000, "-123", Exact)
    ])) {
        var x = makeFloat(test.x).SetPrec(test.prec);
        nuint prec = test.prec;
        if (prec > MaxPrec) {
            prec = MaxPrec;
        }
        {
            nuint got = x.Prec(); if (got != prec) {
                t.Errorf("%s.SetPrec(%d).Prec() == %d; want %d", test.x, test.prec, got, prec);
            }
        }
        {
            @string got = x.String();
            var acc = x.Acc(); if (got != test.want || acc != test.acc) {
                t.Errorf("%s.SetPrec(%d) = %s (%s); want %s (%s)", test.x, test.prec, got, acc, test.want, test.acc);
            }
        }
    }
}

internal partial struct TestFloatMinPrec_type /*dyn*/ {
    internal @string x;
    internal nuint want;
}

public static void TestFloatMinPrec(ж<testing.T> t) {
    const nuint max = 100;
    foreach (var (_, test) in slice<TestFloatMinPrec_type>([
        new("0", 0),
        new("-0", 0),
        new("+Inf", 0),
        new("-Inf", 0),
        new("1", 1),
        new("2", 1),
        new("3", 2),
        new("0x8001", 16),
        new("0x8001p-1000", 16),
        new("0x8001p+1000", 16),
        new("0.1", max)
    ])) {
        var x = makeFloat(test.x).SetPrec(max);
        {
            nuint got = x.MinPrec(); if (got != test.want) {
                t.Errorf("%s.MinPrec() = %d; want %d", test.x, got, test.want);
            }
        }
    }
}

internal partial struct TestFloatSign_type /*dyn*/ {
    internal @string x;
    internal nint s;
}

public static void TestFloatSign(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatSign_type>([
        new("-Inf", -1),
        new("-1", -1),
        new("-0", 0),
        new("+0", 0),
        new("+1", +1),
        new("+Inf", +1)
    ])) {
        var x = makeFloat(test.x);
        nint s = x.Sign();
        if (s != test.s) {
            t.Errorf("%s.Sign() = %d; want %d", test.x, s, test.s);
        }
    }
}

// alike(x, y) is like x.Cmp(y) == 0 but also considers the sign of 0 (0 != -0).
internal static bool alike(ж<global::go.math.big_package.Float> Ꮡx, ж<global::go.math.big_package.Float> Ꮡy) {
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();

    return Ꮡx.Cmp(Ꮡy) == 0 && x.Signbit() == y.Signbit();
}

internal static bool alike32(float32 x, float32 y) {
    // we can ignore NaNs
    return x == y && math.Signbit((float64)x) == math.Signbit((float64)y);
}

internal static bool alike64(float64 x, float64 y) {
    // we can ignore NaNs
    return x == y && math.Signbit(x) == math.Signbit(y);
}

internal partial struct TestFloatMantExp_type /*dyn*/ {
    internal @string x;
    internal @string mant;
    internal nint exp;
}

public static void TestFloatMantExp(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatMantExp_type>([
        new("0", "0", 0),
        new("+0", "0", 0),
        new("-0", "-0", 0),
        new("Inf", "+Inf", 0),
        new("+Inf", "+Inf", 0),
        new("-Inf", "-Inf", 0),
        new("1.5", "0.75", 1),
        new("1.024e3", "0.5", 11),
        new("-0.125", "-0.5", -2)
    ])) {
        var x = makeFloat(test.x);
        var mant = makeFloat(test.mant);
        var m = @new<global::go.math.big_package.Float>();
        nint e = x.MantExp(m);
        if (!alike(m, mant) || e != test.exp) {
            t.Errorf("%s.MantExp() = %s, %d; want %s, %d", test.x, m.Text((byte)'g', 10), e, test.mant, test.exp);
        }
    }
}

public static void TestFloatMantExpAliasing(ж<testing.T> t) {
    var x = makeFloat("0.5p10");
    {
        nint e = x.MantExp(x); if (e != 10) {
            t.Fatalf("Float.MantExp aliasing error: got %d; want 10", e);
        }
    }
    {
        var want = makeFloat("0.5"); if (!alike(x, want)) {
            t.Fatalf("Float.MantExp aliasing error: got %s; want %s", x.Text((byte)'g', 10), want.Text((byte)'g', 10));
        }
    }
}

internal partial struct TestFloatSetMantExp_type /*dyn*/ {
    internal @string frac;
    internal nint exp;
    internal @string z;
}

public static void TestFloatSetMantExp(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatSetMantExp_type>([
        new("0", 0, "0"),
        new("+0", 0, "0"),
        new("-0", 0, "-0"),
        new("Inf", 1234, "+Inf"),
        new("+Inf", -1234, "+Inf"),
        new("-Inf", -1234, "-Inf"),
        new("0", MinExp, "0"),
        new("0.25", MinExp, "+0"),    // exponent underflow
        new("-0.25", MinExp, "-0"),   // exponent underflow
        new("1", MaxExp, "+Inf"),     // exponent overflow
        new("2", MaxExp - 1, "+Inf"), // exponent overflow
        new("0.75", 1, "1.5"),
        new("0.5", 11, "1024"),
        new("-0.5", -2, "-0.125"),
        new("32", 5, "1024"),
        new("1024", -10, "1")
    ])) {
        var frac = makeFloat(test.frac);
        var want = makeFloat(test.z);
        ref var z = ref heap(new global::go.math.big_package.Float(), out var Ꮡz);
        Ꮡz.SetMantExp(frac, test.exp);
        if (!alike(Ꮡz, want)) {
            t.Errorf("SetMantExp(%s, %d) = %s; want %s", test.frac, test.exp, Ꮡz.Text((byte)'g', 10), test.z);
        }
        // test inverse property
        var mant = @new<global::go.math.big_package.Float>();
        if (Ꮡz.SetMantExp(mant, want.MantExp(mant)).Cmp(want) != 0) {
            t.Errorf("Inverse property not satisfied: got %s; want %s", Ꮡz.Text((byte)'g', 10), test.z);
        }
    }
}

internal partial struct TestFloatPredicates_type /*dyn*/ {
    internal @string x;
    internal nint sign;
    internal bool signbit, inf;
}

public static void TestFloatPredicates(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatPredicates_type>([
        new(x: "-Inf", sign: -1, signbit: true, inf: true),
        new(x: "-1", sign: -1, signbit: true),
        new(x: "-0", signbit: true),
        new(x: "0"),
        new(x: "1", sign: 1),
        new(x: "+Inf", sign: 1, inf: true)
    ])) {
        var x = makeFloat(test.x);
        {
            var got = x.Signbit(); if (got != test.signbit) {
                t.Errorf("(%s).Signbit() = %v; want %v", test.x, got, test.signbit);
            }
        }
        {
            nint got = x.Sign(); if (got != test.sign) {
                t.Errorf("(%s).Sign() = %d; want %d", test.x, got, test.sign);
            }
        }
        {
            var got = x.IsInf(); if (got != test.inf) {
                t.Errorf("(%s).IsInf() = %v; want %v", test.x, got, test.inf);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string intˢ2 => ref intˢ2ᵇ;
private static readonly @string intˢ2ᵇ = " int"u8;

public static void TestFloatIsInt(ж<testing.T> t) {
    foreach (var (_, test) in slice<@string>([
        "0 int",
        "-0 int",
        "1 int",
        "-1 int",
        "0.5",
        "1.23",
        "1.23e1",
        "1.23e2 int",
        "0.000000001e+8",
        "0.000000001e+9 int",
        "1.2345e200 int",
        "Inf",
        "+Inf",
        "-Inf"
    ])) {
        @string s = strings.TrimSuffix(test, intˢ2);
        var want = s != test;
        {
            var got = makeFloat(s).IsInt(); if (got != want) {
                t.Errorf("%s.IsInt() == %t", s, got);
            }
        }
    }
}

internal static int64 fromBinary(@string s) {
    var (x, err) = strconv.ParseInt(s, 2, 64);
    if (err != nil) {
        throw panic(err);
    }
    return x;
}

internal static @string toBinary(int64 x) {
    return strconv.FormatInt(x, 2);
}

internal static void testFloatRound(ж<testing.T> t, int64 x, int64 r, nuint prec, global::go.math.big_package.RoundingMode mode) {
    // verify test data
    bool ok = default;
    switch (mode) {
    case var ᴛ when ᴛ == ToNearestEven || ᴛ == ToNearestAway: {
        ok = true; // nothing to do for now
        break;
    }
    case var ᴛ when ᴛ == ToZero: {
        if (x < 0) {
            ok = r >= x;
        } else {
            ok = r <= x;
        }
        break;
    }
    case var ᴛ when ᴛ == AwayFromZero: {
        if (x < 0) {
            ok = r <= x;
        } else {
            ok = r >= x;
        }
        break;
    }
    case var ᴛ when ᴛ == ToNegativeInf: {
        ok = r <= x;
        break;
    }
    case var ᴛ when ᴛ == ToPositiveInf: {
        ok = r >= x;
        break;
    }
    default: {
        throw panic("unreachable");
        break;
    }}
    if (!ok) {
        t.Fatalf("incorrect test data for prec = %d, %s: x = %s, r = %s", prec, mode, toBinary(x), toBinary(r));
    }

    // compute expected accuracy
    var a = Exact;
    switch (ᐧ) {
    case {} when r < x: {
        a = Below;
        break;
    }
    case {} when r > x: {
        a = Above;
        break;
    }}

    // round
    var f = @new<global::go.math.big_package.Float>().SetMode(mode).SetInt64(x).SetPrec(prec);

    // check result
    var r1 = f.int64();
    nuint p1 = f.Prec();
    var a1 = f.Acc();
    if (r1 != r || p1 != prec || a1 != a) {
        t.Errorf("round %s (%d bits, %s) incorrect: got %s (%d bits, %s); want %s (%d bits, %s)",
            toBinary(x), prec, mode,
            toBinary(r1), p1, a1,
            toBinary(r), prec, a);
        return;
    }

    // g and f should be the same
    // (rounding by SetPrec after SetInt64 using default precision
    // should be the same as rounding by SetInt64 after setting the
    // precision)
    var g = @new<global::go.math.big_package.Float>().SetMode(mode).SetPrec(prec).SetInt64(x);
    if (!alike(g, f)) {
        t.Errorf("round %s (%d bits, %s) not symmetric: got %s and %s; want %s",
            toBinary(x), prec, mode,
            toBinary(g.int64()),
            toBinary(r1),
            toBinary(r));
        return;
    }

    // h and f should be the same
    // (repeated rounding should be idempotent)
    var h = @new<global::go.math.big_package.Float>().SetMode(mode).SetPrec(prec).Set(f);
    if (!alike(h, f)) {
        t.Errorf("round %s (%d bits, %s) not idempotent: got %s and %s; want %s",
            toBinary(x), prec, mode,
            toBinary(h.int64()),
            toBinary(r1),
            toBinary(r));
        return;
    }
}

internal partial struct TestFloatRound_type /*dyn*/ {
    internal nuint prec;
    internal @string x, zero, neven, naway, away; // input, results rounded to prec bits
}

// TestFloatRound tests basic rounding.
public static void TestFloatRound(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatRound_type>([
        new(5, "1000", "1000", "1000", "1000", "1000"),
        new(5, "1001", "1001", "1001", "1001", "1001"),
        new(5, "1010", "1010", "1010", "1010", "1010"),
        new(5, "1011", "1011", "1011", "1011", "1011"),
        new(5, "1100", "1100", "1100", "1100", "1100"),
        new(5, "1101", "1101", "1101", "1101", "1101"),
        new(5, "1110", "1110", "1110", "1110", "1110"),
        new(5, "1111", "1111", "1111", "1111", "1111"),

        new(4, "1000", "1000", "1000", "1000", "1000"),
        new(4, "1001", "1001", "1001", "1001", "1001"),
        new(4, "1010", "1010", "1010", "1010", "1010"),
        new(4, "1011", "1011", "1011", "1011", "1011"),
        new(4, "1100", "1100", "1100", "1100", "1100"),
        new(4, "1101", "1101", "1101", "1101", "1101"),
        new(4, "1110", "1110", "1110", "1110", "1110"),
        new(4, "1111", "1111", "1111", "1111", "1111"),

        new(3, "1000", "1000", "1000", "1000", "1000"),
        new(3, "1001", "1000", "1000", "1010", "1010"),
        new(3, "1010", "1010", "1010", "1010", "1010"),
        new(3, "1011", "1010", "1100", "1100", "1100"),
        new(3, "1100", "1100", "1100", "1100", "1100"),
        new(3, "1101", "1100", "1100", "1110", "1110"),
        new(3, "1110", "1110", "1110", "1110", "1110"),
        new(3, "1111", "1110", "10000", "10000", "10000"),

        new(3, "1000001", "1000000", "1000000", "1000000", "1010000"),
        new(3, "1001001", "1000000", "1010000", "1010000", "1010000"),
        new(3, "1010001", "1010000", "1010000", "1010000", "1100000"),
        new(3, "1011001", "1010000", "1100000", "1100000", "1100000"),
        new(3, "1100001", "1100000", "1100000", "1100000", "1110000"),
        new(3, "1101001", "1100000", "1110000", "1110000", "1110000"),
        new(3, "1110001", "1110000", "1110000", "1110000", "10000000"),
        new(3, "1111001", "1110000", "10000000", "10000000", "10000000"),

        new(2, "1000", "1000", "1000", "1000", "1000"),
        new(2, "1001", "1000", "1000", "1000", "1100"),
        new(2, "1010", "1000", "1000", "1100", "1100"),
        new(2, "1011", "1000", "1100", "1100", "1100"),
        new(2, "1100", "1100", "1100", "1100", "1100"),
        new(2, "1101", "1100", "1100", "1100", "10000"),
        new(2, "1110", "1100", "10000", "10000", "10000"),
        new(2, "1111", "1100", "10000", "10000", "10000"),

        new(2, "1000001", "1000000", "1000000", "1000000", "1100000"),
        new(2, "1001001", "1000000", "1000000", "1000000", "1100000"),
        new(2, "1010001", "1000000", "1100000", "1100000", "1100000"),
        new(2, "1011001", "1000000", "1100000", "1100000", "1100000"),
        new(2, "1100001", "1100000", "1100000", "1100000", "10000000"),
        new(2, "1101001", "1100000", "1100000", "1100000", "10000000"),
        new(2, "1110001", "1100000", "10000000", "10000000", "10000000"),
        new(2, "1111001", "1100000", "10000000", "10000000", "10000000"),

        new(1, "1000", "1000", "1000", "1000", "1000"),
        new(1, "1001", "1000", "1000", "1000", "10000"),
        new(1, "1010", "1000", "1000", "1000", "10000"),
        new(1, "1011", "1000", "1000", "1000", "10000"),
        new(1, "1100", "1000", "10000", "10000", "10000"),
        new(1, "1101", "1000", "10000", "10000", "10000"),
        new(1, "1110", "1000", "10000", "10000", "10000"),
        new(1, "1111", "1000", "10000", "10000", "10000"),

        new(1, "1000001", "1000000", "1000000", "1000000", "10000000"),
        new(1, "1001001", "1000000", "1000000", "1000000", "10000000"),
        new(1, "1010001", "1000000", "1000000", "1000000", "10000000"),
        new(1, "1011001", "1000000", "1000000", "1000000", "10000000"),
        new(1, "1100001", "1000000", "10000000", "10000000", "10000000"),
        new(1, "1101001", "1000000", "10000000", "10000000", "10000000"),
        new(1, "1110001", "1000000", "10000000", "10000000", "10000000"),
        new(1, "1111001", "1000000", "10000000", "10000000", "10000000")
    ])) {
        var x = fromBinary(test.x);
        var z = fromBinary(test.zero);
        var e = fromBinary(test.neven);
        var n = fromBinary(test.naway);
        var a = fromBinary(test.away);
        nuint prec = test.prec;

        testFloatRound(t, x, z, prec, ToZero);
        testFloatRound(t, x, e, prec, ToNearestEven);
        testFloatRound(t, x, n, prec, ToNearestAway);
        testFloatRound(t, x, a, prec, AwayFromZero);

        testFloatRound(t, x, z, prec, ToNegativeInf);
        testFloatRound(t, x, a, prec, ToPositiveInf);

        testFloatRound(t, -x, -a, prec, ToNegativeInf);
        testFloatRound(t, -x, -z, prec, ToPositiveInf);
    }
}

// TestFloatRound24 tests that rounding a float64 to 24 bits
// matches IEEE 754 rounding to nearest when converting a
// float64 to a float32 (excluding denormal numbers).
public static void TestFloatRound24(ж<testing.T> t) {
    const nint x0 = /* 1<<26 - 0x10 */ 67108848; // 11...110000 (26 bits)
    for (nint d = 0; d <= 0x10; d++) {
        var x = (float64)(x0 + d);
        var f = @new<global::go.math.big_package.Float>().SetPrec(24).SetFloat64(x);
        var (got, _) = f.Float32();
        var want = (float32)x;
        if (got != want) {
            t.Errorf("Round(%g, 24) = %g; want %g", x, got, want);
        }
    }
}

public static void TestFloatSetUint64(ж<testing.T> t) {
    foreach (var (_, want) in slice<uint64>([
        0,
        1,
        2,
        10,
        100,
        (uint64)(4294967296L - 1),
        (uint64)1 << 32,
        18446744073709551615UL
    ])) {
        ref var f = ref heap(new global::go.math.big_package.Float(), out var Ꮡf);
        Ꮡf.SetUint64(want);
        {
            var got = Ꮡf.uint64(); if (got != want) {
                t.Errorf("got %#x (%s); want %#x", got, Ꮡf.Text((byte)'p', 0), want);
            }
        }
    }

    // test basic rounding behavior (exhaustive rounding testing is done elsewhere)
    const uint64 x = 0x8765432187654321; // 64 bits needed
    for (nuint prec = (nuint)1; prec <= 64; prec++) {
        var f = @new<global::go.math.big_package.Float>().SetPrec(prec).SetMode(ToZero).SetUint64(x);
        var got = f.uint64();
        var want = x & ~(((uint64)1).Lsh((64 - prec)) - 1); // cut off (round to zero) low 64-prec bits
        if (got != want) {
            t.Errorf("got %#x (%s); want %#x", got, f.Text((byte)'p', 0), want);
        }
    }
}

public static void TestFloatSetInt64(ж<testing.T> t) {
    foreach (var (_, vᴛ1) in slice<int64>([
        0,
        1,
        2,
        10,
        100,
        4294967295L,
        4294967296L,
        9223372036854775807L
    ])) {
        var want = vᴛ1;

        foreach (var i in range(new nint[]{}.array(2))) {
            if ((i & 1) != 0) {
                want = -want;
            }
            ref var f = ref heap(new global::go.math.big_package.Float(), out var Ꮡf);
            Ꮡf.SetInt64(want);
            {
                var got = Ꮡf.int64(); if (got != want) {
                    t.Errorf("got %#x (%s); want %#x", got, Ꮡf.Text((byte)'p', 0), want);
                }
            }
        }
    }

    // test basic rounding behavior (exhaustive rounding testing is done elsewhere)
    const int64 x = 0x7654321076543210; // 63 bits needed
    for (nuint prec = (nuint)1; prec <= 63; prec++) {
        var f = @new<global::go.math.big_package.Float>().SetPrec(prec).SetMode(ToZero).SetInt64(x);
        var got = f.int64();
        var want = x & ~(((int64)1).Lsh((63 - prec)) - 1); // cut off (round to zero) low 63-prec bits
        if (got != want) {
            t.Errorf("got %#x (%s); want %#x", got, f.Text((byte)'p', 0), want);
        }
    }
}

public static void TestFloatSetFloat64(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, vᴛ1) in slice<float64>([
            0D,
            1D,
            2D,
            12345D,
            1e10D,
            1e100D,
            3.14159265e10D,
            2.718281828e-123D,
            1.0D / 3D,
            math.MaxFloat32,
            math.MaxFloat64,
            math.SmallestNonzeroFloat32,
            math.SmallestNonzeroFloat64,
            math.Inf(-1),
            math.Inf(0),
            -math.Inf(1)
        ])) {
            var want = vᴛ1;

            foreach (var i in range(new nint[]{}.array(2))) {
                if ((i & 1) != 0) {
                    want = -want;
                }
                ref var fΔ1 = ref heap(new global::go.math.big_package.Float(), out var ᏑfΔ1);
                ᏑfΔ1.SetFloat64(want);
                {
                    var (got, acc) = ᏑfΔ1.Float64(); if (got != want || acc != Exact) {
                        t.Errorf("got %g (%s, %s); want %g (Exact)", got, ᏑfΔ1.Text((byte)'p', 0), acc, want);
                    }
                }
            }
        }

        // test basic rounding behavior (exhaustive rounding testing is done elsewhere)
        const uint64 x = 0x8765432143218; // 53 bits needed
        for (nuint prec = (nuint)1; prec <= 52; prec++) {
            var fΔ2 = @new<global::go.math.big_package.Float>().SetPrec(prec).SetMode(ToZero).SetFloat64((float64)x);
            var (got, _) = fΔ2.Float64();
            var want = (float64)(x & ~(((uint64)1).Lsh((52 - prec)) - 1)); // cut off (round to zero) low 53-prec bits
            if (got != want) {
                t.Errorf("got %g (%s); want %g", got, fΔ2.Text((byte)'p', 0), want);
            }
        }

        // test NaN
        defer(() => {
            {
                ref var p = ref heap<global::go.math.big_package.ErrNaN>(out var Ꮡp);
                (p, var ok) = recover()._<ErrNaN>(ᐧ); if (!ok) {
                    t.Errorf("got %v; want ErrNaN panic", p);
                }
            }
        }, ref ᒐ);
        ref var f = ref heap(new global::go.math.big_package.Float(), out var Ꮡf);
        Ꮡf.SetFloat64(math.NaN());
        // should not reach here
        t.Errorf("got %s; want ErrNaN panic", Ꮡf.Text((byte)'p', 0));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFloatSetInt(ж<testing.T> t) {
    foreach (var (_, want) in slice<@string>([
        "0",
        "1",
        "-1",
        "1234567890",
        "123456789012345678901234567890",
        "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"
    ])) {
        ref var x = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡx);
        var (_, ok) = Ꮡx.SetString(want, 0);
        if (!ok) {
            t.Errorf("invalid integer %s", want);
            continue;
        }
        nint n = x.BitLen();

        ref var f = ref heap(new global::go.math.big_package.Float(), out var Ꮡf);
        Ꮡf.SetInt(Ꮡx);

        // check precision
        if (n < 64) {
            n = 64;
        }
        {
            nuint prec = f.Prec(); if (prec != (nuint)n) {
                t.Errorf("got prec = %d; want %d", prec, n);
            }
        }

        // check value
        @string got = Ꮡf.Text((byte)'g', 100);
        if (got != want) {
            t.Errorf("got %s (%s); want %s", got, Ꮡf.Text((byte)'p', 0), want);
        }
    }

    // TODO(gri) test basic rounding behavior
}

public static void TestFloatSetRat(ж<testing.T> t) {
    foreach (var (_, want) in slice<@string>([
        "0",
        "1",
        "-1",
        "1234567890",
        "123456789012345678901234567890",
        "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890",
        "1.2",
        "3.14159265"
        // TODO(gri) expand
    ])) {
        ref var x = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡx);
        var (_, ok) = Ꮡx.SetString(want);
        if (!ok) {
            t.Errorf("invalid fraction %s", want);
            continue;
        }
        nint n = max(Ꮡx.Num().BitLen(), Ꮡx.Denom().BitLen());

        ref var f1 = ref heap(new global::go.math.big_package.Float(), out var Ꮡf1);
        ref var f2 = ref heap(new global::go.math.big_package.Float(), out var Ꮡf2);
        Ꮡf2.SetPrec(1000);
        Ꮡf1.SetRat(Ꮡx);
        Ꮡf2.SetRat(Ꮡx);

        // check precision when set automatically
        if (n < 64) {
            n = 64;
        }
        {
            nuint prec = f1.Prec(); if (prec != (nuint)n) {
                t.Errorf("got prec = %d; want %d", prec, n);
            }
        }

        @string got = Ꮡf2.Text((byte)'g', 100);
        if (got != want) {
            t.Errorf("got %s (%s); want %s", got, Ꮡf2.Text((byte)'p', 0), want);
        }
    }
}

internal partial struct TestFloatSetInf_type /*dyn*/ {
    internal bool signbit;
    internal nuint prec;
    internal @string want;
}

public static void TestFloatSetInf(ж<testing.T> t) {
    ref var f = ref heap(new global::go.math.big_package.Float(), out var Ꮡf);
    foreach (var (_, test) in slice<TestFloatSetInf_type>([
        new(false, 0, "+Inf"),
        new(true, 0, "-Inf"),
        new(false, 10, "+Inf"),
        new(true, 30, "-Inf")
    ])) {
        var x = Ꮡf.SetPrec(test.prec).SetInf(test.signbit);
        {
            @string got = x.String(); if (got != test.want || x.Prec() != test.prec) {
                t.Errorf("SetInf(%v) = %s (prec = %d); want %s (prec = %d)", test.signbit, got, x.Prec(), test.want, test.prec);
            }
        }
    }
}

internal partial struct TestFloatUint64_type /*dyn*/ {
    internal @string x;
    internal uint64 @out;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatUint64(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatUint64_type>([
        new("-Inf", 0, Above),
        new("-1", 0, Above),
        new("-1e-1000", 0, Above),
        new("-0", 0, Exact),
        new("0", 0, Exact),
        new("1e-1000", 0, Below),
        new("1", 1, Exact),
        new("1.000000000000000000001", 1, Below),
        new("12345.0", 12345, Exact),
        new("12345.000000000000000000001", 12345, Below),
        new("18446744073709551615", 18446744073709551615UL, Exact),
        new("18446744073709551615.000000000000000000001", math.MaxUint64, Below),
        new("18446744073709551616", math.MaxUint64, Below),
        new("1e10000", math.MaxUint64, Below),
        new("+Inf", math.MaxUint64, Below)
    ])) {
        var x = makeFloat(test.x);
        var (@out, acc) = x.Uint64();
        if (@out != test.@out || acc != test.acc) {
            t.Errorf("%s: got %d (%s); want %d (%s)", test.x, @out, acc, test.@out, test.acc);
        }
    }
}

internal partial struct TestFloatInt64_type /*dyn*/ {
    internal @string x;
    internal int64 @out;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatInt64(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatInt64_type>([
        new("-Inf", math.MinInt64, Above),
        new("-1e10000", math.MinInt64, Above),
        new("-9223372036854775809", math.MinInt64, Above),
        new("-9223372036854775808.000000000000000000001", math.MinInt64, Above),
        new("-9223372036854775808", -9223372036854775808L, Exact),
        new("-9223372036854775807.000000000000000000001", -9223372036854775807L, Above),
        new("-9223372036854775807", -9223372036854775807L, Exact),
        new("-12345.000000000000000000001", -12345, Above),
        new("-12345.0", -12345, Exact),
        new("-1.000000000000000000001", -1, Above),
        new("-1.5", -1, Above),
        new("-1", -1, Exact),
        new("-1e-1000", 0, Above),
        new("0", 0, Exact),
        new("1e-1000", 0, Below),
        new("1", 1, Exact),
        new("1.000000000000000000001", 1, Below),
        new("1.5", 1, Below),
        new("12345.0", 12345, Exact),
        new("12345.000000000000000000001", 12345, Below),
        new("9223372036854775807", 9223372036854775807L, Exact),
        new("9223372036854775807.000000000000000000001", math.MaxInt64, Below),
        new("9223372036854775808", math.MaxInt64, Below),
        new("1e10000", math.MaxInt64, Below),
        new("+Inf", math.MaxInt64, Below)
    ])) {
        var x = makeFloat(test.x);
        var (@out, acc) = x.Int64();
        if (@out != test.@out || acc != test.acc) {
            t.Errorf("%s: got %d (%s); want %d (%s)", test.x, @out, acc, test.@out, test.acc);
        }
    }
}

internal partial struct TestFloatFloat32_type /*dyn*/ {
    internal @string x;
    internal float32 @out;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatFloat32(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatFloat32_type>([
        new("0", 0F, Exact),

        // underflow to zero
        new("1e-1000", 0F, Below),
        new("0x0.000002p-127", 0F, Below),
        new("0x.0000010p-126", 0F, Below),

        // denormals
        new("1.401298464e-45", math.SmallestNonzeroFloat32, Above), // rounded up to smallest denormal
        new("0x.ffffff8p-149", math.SmallestNonzeroFloat32, Above), // rounded up to smallest denormal
        new("0x.0000018p-126", math.SmallestNonzeroFloat32, Above), // rounded up to smallest denormal
        new("0x.0000020p-126", math.SmallestNonzeroFloat32, Exact),
        new("0x.8p-148", math.SmallestNonzeroFloat32, Exact),
        new("1p-149", math.SmallestNonzeroFloat32, Exact),
        new("0x.fffffep-126", math.Float32frombits(0x7fffff), Exact), // largest denormal

        // special denormal cases (see issues 14553, 14651)
        new("0x0.0000001p-126", math.Float32frombits(0x00000000), Below), // underflow to zero
        new("0x0.0000008p-126", math.Float32frombits(0x00000000), Below), // underflow to zero
        new("0x0.0000010p-126", math.Float32frombits(0x00000000), Below), // rounded down to even
        new("0x0.0000011p-126", math.Float32frombits(0x00000001), Above), // rounded up to smallest denormal
        new("0x0.0000018p-126", math.Float32frombits(0x00000001), Above), // rounded up to smallest denormal

        new("0x1.0000000p-149", math.Float32frombits(0x00000001), Exact), // smallest denormal
        new("0x0.0000020p-126", math.Float32frombits(0x00000001), Exact), // smallest denormal
        new("0x0.fffffe0p-126", math.Float32frombits(0x007fffff), Exact), // largest denormal
        new("0x1.0000000p-126", math.Float32frombits(0x00800000), Exact), // smallest normal

        new("0x0.8p-149", math.Float32frombits(0x000000000), Below), // rounded down to even
        new("0x0.9p-149", math.Float32frombits(0x000000001), Above), // rounded up to smallest denormal
        new("0x0.ap-149", math.Float32frombits(0x000000001), Above), // rounded up to smallest denormal
        new("0x0.bp-149", math.Float32frombits(0x000000001), Above), // rounded up to smallest denormal
        new("0x0.cp-149", math.Float32frombits(0x000000001), Above), // rounded up to smallest denormal

        new("0x1.0p-149", math.Float32frombits(0x000000001), Exact), // smallest denormal
        new("0x1.7p-149", math.Float32frombits(0x000000001), Below),
        new("0x1.8p-149", math.Float32frombits(0x000000002), Above),
        new("0x1.9p-149", math.Float32frombits(0x000000002), Above),

        new("0x2.0p-149", math.Float32frombits(0x000000002), Exact),
        new("0x2.8p-149", math.Float32frombits(0x000000002), Below), // rounded down to even
        new("0x2.9p-149", math.Float32frombits(0x000000003), Above),

        new("0x3.0p-149", math.Float32frombits(0x000000003), Exact),
        new("0x3.7p-149", math.Float32frombits(0x000000003), Below),
        new("0x3.8p-149", math.Float32frombits(0x000000004), Above), // rounded up to even

        new("0x4.0p-149", math.Float32frombits(0x000000004), Exact),
        new("0x4.8p-149", math.Float32frombits(0x000000004), Below), // rounded down to even
        new("0x4.9p-149", math.Float32frombits(0x000000005), Above),

        // specific case from issue 14553
        new("0x7.7p-149", math.Float32frombits(0x000000007), Below),
        new("0x7.8p-149", math.Float32frombits(0x000000008), Above),
        new("0x7.9p-149", math.Float32frombits(0x000000008), Above),

        // normals
        new("0x.ffffffp-126", math.Float32frombits(0x00800000), Above), // rounded up to smallest normal
        new("1p-126", math.Float32frombits(0x00800000), Exact),         // smallest normal
        new("0x1.fffffep-126", math.Float32frombits(0x00ffffff), Exact),
        new("0x1.ffffffp-126", math.Float32frombits(0x01000000), Above), // rounded up
        new("1", 1F, Exact),
        new("1.000000000000000000001", 1F, Below),
        new("12345.0", 12345F, Exact),
        new("12345.000000000000000000001", 12345F, Below),
        new("0x1.fffffe0p127", math.MaxFloat32, Exact),
        new("0x1.fffffe8p127", math.MaxFloat32, Below),

        // overflow
        new("0x1.ffffff0p127", (float32)math.Inf(+1), Above),
        new("0x1p128", (float32)math.Inf(+1), Above),
        new("1e10000", (float32)math.Inf(+1), Above),
        new("0x1.ffffff0p2147483646", (float32)math.Inf(+1), Above), // overflow in rounding

        // inf
        new("Inf", (float32)math.Inf(+1), Exact)
    ])) {
        for (nint i = 0; i < 2; i++) {
            // test both signs
            @string tx = test.x;
            var tout = test.@out;
            var tacc = test.acc;
            if (i != 0) {
                tx = "-"u8 + tx;
                tout = -tout;
                tacc = (global::go.math.big_package.Accuracy)(-tacc);
            }

            // conversion should match strconv where syntax is agreeable
            {
                var (f, err) = strconv.ParseFloat(tx, 32); if (err == nil && !alike32((float32)f, tout)) {
                    t.Errorf("%s: got %g; want %g (incorrect test data)", tx, f, tout);
                }
            }

            var x = makeFloat(tx);
            var (@out, acc) = x.Float32();
            if (!alike32(@out, tout) || acc != tacc) {
                t.Errorf("%s: got %g (%#08x, %s); want %g (%#08x, %s)", tx, @out, math.Float32bits(@out), acc, test.@out, math.Float32bits(test.@out), tacc);
            }

            // test that x.SetFloat64(float64(f)).Float32() == f
            ref var x2 = ref heap(new global::go.math.big_package.Float(), out var Ꮡx2);
            var (out2, acc2) = Ꮡx2.SetFloat64((float64)@out).Float32();
            if (!alike32(out2, @out) || acc2 != Exact) {
                t.Errorf("idempotency test: got %g (%s); want %g (Exact)", out2, acc2, @out);
            }
        }
    }
}

internal partial struct TestFloatFloat64_type /*dyn*/ {
    internal @string x;
    internal float64 @out;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatFloat64(ж<testing.T> t) {
    UntypedFloat smallestNormalFloat64 = 2.2250738585072014e-308; // 1p-1022
    foreach (var (_, test) in slice<TestFloatFloat64_type>([
        new("0", 0D, Exact),

        // underflow to zero
        new("1e-1000", 0D, Below),
        new("0x0.0000000000001p-1023", 0D, Below),
        new("0x0.00000000000008p-1022", 0D, Below),

        // denormals
        new("0x0.0000000000000cp-1022", math.SmallestNonzeroFloat64, Above), // rounded up to smallest denormal
        new("0x0.00000000000010p-1022", math.SmallestNonzeroFloat64, Exact), // smallest denormal
        new("0x.8p-1073", math.SmallestNonzeroFloat64, Exact),
        new("1p-1074", math.SmallestNonzeroFloat64, Exact),
        new("0x.fffffffffffffp-1022", math.Float64frombits(0x000fffffffffffffUL), Exact), // largest denormal

        // special denormal cases (see issues 14553, 14651)
        new("0x0.00000000000001p-1022", math.Float64frombits(0x00000000000000000), Below), // underflow to zero
        new("0x0.00000000000004p-1022", math.Float64frombits(0x00000000000000000), Below), // underflow to zero
        new("0x0.00000000000008p-1022", math.Float64frombits(0x00000000000000000), Below), // rounded down to even
        new("0x0.00000000000009p-1022", math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal
        new("0x0.0000000000000ap-1022", math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal

        new("0x0.8p-1074", math.Float64frombits(0x00000000000000000), Below), // rounded down to even
        new("0x0.9p-1074", math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal
        new("0x0.ap-1074", math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal
        new("0x0.bp-1074", math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal
        new("0x0.cp-1074", math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal

        new("0x1.0p-1074", math.Float64frombits(0x00000000000000001), Exact),
        new("0x1.7p-1074", math.Float64frombits(0x00000000000000001), Below),
        new("0x1.8p-1074", math.Float64frombits(0x00000000000000002), Above),
        new("0x1.9p-1074", math.Float64frombits(0x00000000000000002), Above),

        new("0x2.0p-1074", math.Float64frombits(0x00000000000000002), Exact),
        new("0x2.8p-1074", math.Float64frombits(0x00000000000000002), Below), // rounded down to even
        new("0x2.9p-1074", math.Float64frombits(0x00000000000000003), Above),

        new("0x3.0p-1074", math.Float64frombits(0x00000000000000003), Exact),
        new("0x3.7p-1074", math.Float64frombits(0x00000000000000003), Below),
        new("0x3.8p-1074", math.Float64frombits(0x00000000000000004), Above), // rounded up to even

        new("0x4.0p-1074", math.Float64frombits(0x00000000000000004), Exact),
        new("0x4.8p-1074", math.Float64frombits(0x00000000000000004), Below), // rounded down to even
        new("0x4.9p-1074", math.Float64frombits(0x00000000000000005), Above),

        // normals
        new("0x.fffffffffffff8p-1022", math.Float64frombits(0x0010000000000000UL), Above), // rounded up to smallest normal
        new("1p-1022", math.Float64frombits(0x0010000000000000UL), Exact),                 // smallest normal
        new("1", 1D, Exact),
        new("1.000000000000000000001", 1D, Below),
        new("12345.0", 12345D, Exact),
        new("12345.000000000000000000001", 12345D, Below),
        new("0x1.fffffffffffff0p1023", math.MaxFloat64, Exact),
        new("0x1.fffffffffffff4p1023", math.MaxFloat64, Below),

        // overflow
        new("0x1.fffffffffffff8p1023", math.Inf(+1), Above),
        new("0x1p1024", math.Inf(+1), Above),
        new("1e10000", math.Inf(+1), Above),
        new("0x1.fffffffffffff8p2147483646", math.Inf(+1), Above), // overflow in rounding
        new("Inf", math.Inf(+1), Exact),

        // selected denormalized values that were handled incorrectly in the past
        new("0x.fffffffffffffp-1022", smallestNormalFloat64 - math.SmallestNonzeroFloat64, Exact),
        new("4503599627370495p-1074", smallestNormalFloat64 - math.SmallestNonzeroFloat64, Exact),

        // https://www.exploringbinary.com/php-hangs-on-numeric-value-2-2250738585072011e-308/
        new("2.2250738585072011e-308", 2.225073858507201e-308D, Below),
        // https://www.exploringbinary.com/java-hangs-when-converting-2-2250738585072012e-308/
        new("2.2250738585072012e-308", 2.2250738585072014e-308D, Above)
    ])) {
        for (nint i = 0; i < 2; i++) {
            // test both signs
            @string tx = test.x;
            var tout = test.@out;
            var tacc = test.acc;
            if (i != 0) {
                tx = "-"u8 + tx;
                tout = -tout;
                tacc = (global::go.math.big_package.Accuracy)(-tacc);
            }

            // conversion should match strconv where syntax is agreeable
            {
                var (f, err) = strconv.ParseFloat(tx, 64); if (err == nil && !alike64(f, tout)) {
                    t.Errorf("%s: got %g; want %g (incorrect test data)", tx, f, tout);
                }
            }

            var x = makeFloat(tx);
            var (@out, acc) = x.Float64();
            if (!alike64(@out, tout) || acc != tacc) {
                t.Errorf("%s: got %g (%#016x, %s); want %g (%#016x, %s)", tx, @out, math.Float64bits(@out), acc, test.@out, math.Float64bits(test.@out), tacc);
            }

            // test that x.SetFloat64(f).Float64() == f
            ref var x2 = ref heap(new global::go.math.big_package.Float(), out var Ꮡx2);
            var (out2, acc2) = Ꮡx2.SetFloat64(@out).Float64();
            if (!alike64(out2, @out) || acc2 != Exact) {
                t.Errorf("idempotency test: got %g (%s); want %g (Exact)", out2, acc2, @out);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string nilˢ2 => ref nilˢ2ᵇ;
private static readonly @string nilˢ2ᵇ = "nil"u8;

internal partial struct TestFloatInt_type /*dyn*/ {
    internal @string x;
    internal @string want;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatInt(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatInt_type>([
        new("0", "0", Exact),
        new("+0", "0", Exact),
        new("-0", "0", Exact),
        new("Inf", "nil", Below),
        new("+Inf", "nil", Below),
        new("-Inf", "nil", Above),
        new("1", "1", Exact),
        new("-1", "-1", Exact),
        new("1.23", "1", Below),
        new("-1.23", "-1", Above),
        new("123e-2", "1", Below),
        new("123e-3", "0", Below),
        new("123e-4", "0", Below),
        new("1e-1000", "0", Below),
        new("-1e-1000", "0", Above),
        new("1e+10", "10000000000", Exact),
        new("1e+100", "10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Exact)
    ])) {
        var x = makeFloat(test.x);
        var (res, acc) = x.Int(nil);
        @string got = nilˢ2;
        if (res != nil) {
            got = res.String();
        }
        if (got != test.want || acc != test.acc) {
            t.Errorf("%s: got %s (%s); want %s (%s)", test.x, got, acc, test.want, test.acc);
        }
    }

    // check that supplied *Int is used
    foreach (var (_, f) in slice<@string>(["0", "1", "-1", "1234"])) {
        var x = makeFloat(f);
        var i = @new<global::go.math.big_package.ΔInt>();
        {
            var (res, _) = x.Int(i); if (res != i) {
                t.Errorf("(%s).Int is not using supplied *Int", f);
            }
        }
    }
}

internal partial struct TestFloatRat_type /*dyn*/ {
    internal @string x, want;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatRat(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatRat_type>([
        new("0", "0/1", Exact),
        new("+0", "0/1", Exact),
        new("-0", "0/1", Exact),
        new("Inf", "nil", Below),
        new("+Inf", "nil", Below),
        new("-Inf", "nil", Above),
        new("1", "1/1", Exact),
        new("-1", "-1/1", Exact),
        new("1.25", "5/4", Exact),
        new("-1.25", "-5/4", Exact),
        new("1e10", "10000000000/1", Exact),
        new("1p10", "1024/1", Exact),
        new("-1p-10", "-1/1024", Exact),
        new("3.14159265", "7244019449799623199/2305843009213693952", Exact)
    ])) {
        var x = makeFloat(test.x).SetPrec(64);
        var (res, acc) = x.Rat(nil);
        @string got = nilˢ2;
        if (res != nil) {
            got = res.String();
        }
        if (got != test.want) {
            t.Errorf("%s: got %s; want %s", test.x, got, test.want);
            continue;
        }
        if (acc != test.acc) {
            t.Errorf("%s: got %s; want %s", test.x, acc, test.acc);
            continue;
        }

        // inverse conversion
        if (res != nil) {
            var gotΔ1 = @new<global::go.math.big_package.Float>().SetPrec(64).SetRat(res);
            if (gotΔ1.Cmp(x) != 0) {
                t.Errorf("%s: got %s; want %s", test.x, gotΔ1.OrTypedNil(), x.OrTypedNil());
            }
        }
    }

    // check that supplied *Rat is used
    foreach (var (_, f) in slice<@string>(["0", "1", "-1", "1234"])) {
        var x = makeFloat(f);
        var r = @new<global::go.math.big_package.ΔRat>();
        {
            var (res, _) = x.Rat(r); if (res != r) {
                t.Errorf("(%s).Rat is not using supplied *Rat", f);
            }
        }
    }
}

public static void TestFloatAbs(ж<testing.T> t) {
    foreach (var (_, test) in slice<@string>([
        "0",
        "1",
        "1234",
        "1.23e-2",
        "1e-1000",
        "1e1000",
        "Inf"
    ])) {
        var p = makeFloat(test);
        var a = @new<global::go.math.big_package.Float>().Abs(p);
        if (!alike(a, p)) {
            t.Errorf("%s: got %s; want %s", test, a.Text((byte)'g', 10), test);
        }

        var n = makeFloat("-"u8 + test);
        a.Abs(n);
        if (!alike(a, p)) {
            t.Errorf("-%s: got %s; want %s", test, a.Text((byte)'g', 10), test);
        }
    }
}

public static void TestFloatNeg(ж<testing.T> t) {
    foreach (var (_, test) in slice<@string>([
        "0",
        "1",
        "1234",
        "1.23e-2",
        "1e-1000",
        "1e1000",
        "Inf"
    ])) {
        var p1 = makeFloat(test);
        var n1 = makeFloat("-"u8 + test);
        var n2 = @new<global::go.math.big_package.Float>().Neg(p1);
        var p2 = @new<global::go.math.big_package.Float>().Neg(n2);
        if (!alike(n2, n1)) {
            t.Errorf("%s: got %s; want %s", test, n2.Text((byte)'g', 10), n1.Text((byte)'g', 10));
        }
        if (!alike(p2, p1)) {
            t.Errorf("%s: got %s; want %s", test, p2.Text((byte)'g', 10), p1.Text((byte)'g', 10));
        }
    }
}

public static void TestFloatInc(ж<testing.T> t) {
    UntypedInt n = 10;
    foreach (var (_, prec) in precList.ΔRangeSnapshot()) {
        if (((nint)1).Lsh(prec) < n) {
            continue; // prec must be large enough to hold all numbers from 0 to n
        }
        ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
        ref var one = ref heap(new global::go.math.big_package.Float(), out var Ꮡone);
        Ꮡx.SetPrec(prec);
        Ꮡone.SetInt64(1);
        for (nint i = 0; i < n; i++) {
            Ꮡx.Add(Ꮡx, Ꮡone);
        }
        if (Ꮡx.Cmp(@new<global::go.math.big_package.Float>().SetInt64(n)) != 0) {
            t.Errorf("prec = %d: got %s; want %d", prec, Ꮡx, (nint)(n));
        }
    }
}

// Selected precisions with which to run various tests.
internal static array<nuint> precList = new nuint[]{1, 2, 5, 8, 10, 16, 23, 24, 32, 50, 53, 64, 100, 128, 500, 511, 512, 513, 1000, 10000}.array();

// Selected bits with which to run various tests.
// Each entry is a list of bits representing a floating-point number (see fromBits).
internal static array<ΔBits> bitsList = new ΔBits[]{
    slice<nint>([]),           // = 0
    slice<nint>([0]),          // = 1
    slice<nint>([1]),          // = 2
    slice<nint>([-1]),         // = 1/2
    slice<nint>([10]),         // = 2**10 == 1024
    slice<nint>([-10]),        // = 2**-10 == 1/1024
    slice<nint>([100, 10, 1]), // = 2**100 + 2**10 + 2**1
    slice<nint>([0, -1, -2, -10])
    // TODO(gri) add more test cases
}.array();

// TestFloatAdd tests Float.Add/Sub by comparing the result of a "manual"
// addition/subtraction of arguments represented by Bits values with the
// respective Float addition/subtraction for a variety of precisions
// and rounding modes.
public static void TestFloatAdd(ж<testing.T> t) {
    foreach (var (_, xbits) in bitsList.ΔRangeSnapshot()) {
        foreach (var (_, ybits) in bitsList.ΔRangeSnapshot()) {
            // exact values
            var x = xbits.ΔFloat();
            var y = ybits.ΔFloat();
            var zbits = xbits.add(ybits);
            var z = zbits.ΔFloat();

            foreach (var (i, mode) in new global::go.math.big_package.RoundingMode[]{ToZero, ToNearestEven, AwayFromZero}.array()) {
                foreach (var (_, prec) in precList.ΔRangeSnapshot()) {
                    var got = @new<global::go.math.big_package.Float>().SetPrec(prec).SetMode(mode);
                    got.Add(x, y);
                    var want = zbits.round(prec, mode);
                    if (got.Cmp(want) != 0) {
                        t.Errorf("i = %d, prec = %d, %s:\n\t     %s %v\n\t+    %s %v\n\t=    %s\n\twant %s",
                            i, prec, mode, x.OrTypedNil(), xbits, y.OrTypedNil(), ybits, got.OrTypedNil(), want.OrTypedNil());
                    }

                    got.Sub(z, x);
                    want = ybits.round(prec, mode);
                    if (got.Cmp(want) != 0) {
                        t.Errorf("i = %d, prec = %d, %s:\n\t     %s %v\n\t-    %s %v\n\t=    %s\n\twant %s",
                            i, prec, mode, z.OrTypedNil(), zbits, x.OrTypedNil(), xbits, got.OrTypedNil(), want.OrTypedNil());
                    }
                }
            }
        }
    }
}

// TestFloatAddRoundZero tests Float.Add/Sub rounding when the result is exactly zero.
// x + (-x) or x - x for non-zero x should be +0 in all cases except when
// the rounding mode is ToNegativeInf in which case it should be -0.
public static void TestFloatAddRoundZero(ж<testing.T> t) {
    foreach (var (_, mode) in new global::go.math.big_package.RoundingMode[]{ToNearestEven, ToNearestAway, ToZero, AwayFromZero, ToPositiveInf, ToNegativeInf}.array()) {
        var x = NewFloat(5.0D);
        var y = @new<global::go.math.big_package.Float>().Neg(x);
        var want = NewFloat(0.0D);
        if (mode == ToNegativeInf) {
            want.Neg(want);
        }
        var got = @new<global::go.math.big_package.Float>().SetMode(mode);
        got.Add(x, y);
        if (got.Cmp(want) != 0 || got.neg != (mode == ToNegativeInf)) {
            t.Errorf("%s:\n\t     %v\n\t+    %v\n\t=    %v\n\twant %v",
                mode, x.OrTypedNil(), y.OrTypedNil(), got.OrTypedNil(), want.OrTypedNil());
        }
        got.Sub(x, x);
        if (got.Cmp(want) != 0 || got.neg != (mode == ToNegativeInf)) {
            t.Errorf("%v:\n\t     %v\n\t-    %v\n\t=    %v\n\twant %v",
                mode, x.OrTypedNil(), x.OrTypedNil(), got.OrTypedNil(), want.OrTypedNil());
        }
    }
}

// TestFloatAdd32 tests that Float.Add/Sub of numbers with
// 24bit mantissa behaves like float32 addition/subtraction
// (excluding denormal numbers).
public static void TestFloatAdd32(ж<testing.T> t) {
    // chose base such that we cross the mantissa precision limit
    UntypedInt @base = /* 1<<26 - 0x10 */ 67108848; // 11...110000 (26 bits)
    for (nint d = 0; d <= 0x10; d++) {
        foreach (var i in range(new nint[]{}.array(2))) {
            var (x0, y0) = ((float64)@base, (float64)d);
            if ((i & 1) != 0) {
                (x0, y0) = (y0, x0);
            }

            var x = NewFloat(x0);
            var y = NewFloat(y0);
            var z = @new<global::go.math.big_package.Float>().SetPrec(24);

            z.Add(x, y);
            var (got, acc) = z.Float32();
            var want = (float32)y0 + (float32)x0;
            if (got != want || acc != Exact) {
                t.Errorf("d = %d: %g + %g = %g (%s); want %g (Exact)", d, x0, y0, got, acc, want);
            }

            z.Sub(z, y);
            (got, acc) = z.Float32();
            want = (float32)want - (float32)y0;
            if (got != want || acc != Exact) {
                t.Errorf("d = %d: %g - %g = %g (%s); want %g (Exact)", d, x0 + y0, y0, got, acc, want);
            }
        }
    }
}

// TestFloatAdd64 tests that Float.Add/Sub of numbers with
// 53bit mantissa behaves like float64 addition/subtraction.
public static void TestFloatAdd64(ж<testing.T> t) {
    // chose base such that we cross the mantissa precision limit
    UntypedInt @base = /* 1<<55 - 0x10 */ 36028797018963952; // 11...110000 (55 bits)
    for (nint d = 0; d <= 0x10; d++) {
        foreach (var i in range(new nint[]{}.array(2))) {
            var (x0, y0) = ((float64)@base, (float64)d);
            if ((i & 1) != 0) {
                (x0, y0) = (y0, x0);
            }

            var x = NewFloat(x0);
            var y = NewFloat(y0);
            var z = @new<global::go.math.big_package.Float>().SetPrec(53);

            z.Add(x, y);
            var (got, acc) = z.Float64();
            var want = x0 + y0;
            if (got != want || acc != Exact) {
                t.Errorf("d = %d: %g + %g = %g (%s); want %g (Exact)", d, x0, y0, got, acc, want);
            }

            z.Sub(z, y);
            (got, acc) = z.Float64();
            want -= y0;
            if (got != want || acc != Exact) {
                t.Errorf("d = %d: %g - %g = %g (%s); want %g (Exact)", d, x0 + y0, y0, got, acc, want);
            }
        }
    }
}

internal partial struct TestIssue20490_type /*dyn*/ {
    internal float64 a, b;
}

public static void TestIssue20490(ж<testing.T> t) {
    slice<TestIssue20490_type> tests = slice<TestIssue20490_type>([
        new(4D, 1D),
        new(-4D, 1D),
        new(4D, -1D),
        new(-4D, -1D)
    ]);

    foreach (var (_, test) in tests) {
        var (a, b) = (NewFloat(test.a), NewFloat(test.b));
        var diff = @new<global::go.math.big_package.Float>().Sub(a, b);
        b.Sub(a, b);
        if (b.Cmp(diff) != 0) {
            t.Errorf("got %g - %g = %g; want %g\n", a.OrTypedNil(), NewFloat(test.b).OrTypedNil(), b.OrTypedNil(), diff.OrTypedNil());
        }

        b = NewFloat(test.b);
        var sum = @new<global::go.math.big_package.Float>().Add(a, b);
        b.Add(a, b);
        if (b.Cmp(sum) != 0) {
            t.Errorf("got %g + %g = %g; want %g\n", a.OrTypedNil(), NewFloat(test.b).OrTypedNil(), b.OrTypedNil(), sum.OrTypedNil());
        }
    }
}

// TestFloatMul tests Float.Mul/Quo by comparing the result of a "manual"
// multiplication/division of arguments represented by Bits values with the
// respective Float multiplication/division for a variety of precisions
// and rounding modes.
public static void TestFloatMul(ж<testing.T> t) {
    foreach (var (_, xbits) in bitsList.ΔRangeSnapshot()) {
        foreach (var (_, ybits) in bitsList.ΔRangeSnapshot()) {
            // exact values
            var x = xbits.ΔFloat();
            var y = ybits.ΔFloat();
            var zbits = xbits.mul(ybits);
            var z = zbits.ΔFloat();

            foreach (var (i, mode) in new global::go.math.big_package.RoundingMode[]{ToZero, ToNearestEven, AwayFromZero}.array()) {
                foreach (var (_, prec) in precList.ΔRangeSnapshot()) {
                    var got = @new<global::go.math.big_package.Float>().SetPrec(prec).SetMode(mode);
                    got.Mul(x, y);
                    var want = zbits.round(prec, mode);
                    if (got.Cmp(want) != 0) {
                        t.Errorf("i = %d, prec = %d, %s:\n\t     %v %v\n\t*    %v %v\n\t=    %v\n\twant %v",
                            i, prec, mode, x.OrTypedNil(), xbits, y.OrTypedNil(), ybits, got.OrTypedNil(), want.OrTypedNil());
                    }

                    if (x.Sign() == 0) {
                        continue; // ignore div-0 case (not invertable)
                    }
                    got.Quo(z, x);
                    want = ybits.round(prec, mode);
                    if (got.Cmp(want) != 0) {
                        t.Errorf("i = %d, prec = %d, %s:\n\t     %v %v\n\t/    %v %v\n\t=    %v\n\twant %v",
                            i, prec, mode, z.OrTypedNil(), zbits, x.OrTypedNil(), xbits, got.OrTypedNil(), want.OrTypedNil());
                    }
                }
            }
        }
    }
}

internal partial struct TestFloatMul64_type /*dyn*/ {
    internal float64 x, y;
}

// TestFloatMul64 tests that Float.Mul/Quo of numbers with
// 53bit mantissa behaves like float64 multiplication/division.
public static void TestFloatMul64(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatMul64_type>([
        new(0D, 0D),
        new(0D, 1D),
        new(1D, 1D),
        new(1D, 1.5D),
        new(1.234D, 0.5678D),
        new(2.718281828D, 3.14159265358979D),
        new(2.718281828e10D, 3.14159265358979e-32D),
        new(1.0D / 3D, 1e200D)
    ])) {
        foreach (var i in range(new nint[]{}.array(8))) {
            var (x0, y0) = (test.x, test.y);
            if ((i & 1) != 0) {
                x0 = -x0;
            }
            if ((i & 2) != 0) {
                y0 = -y0;
            }
            if ((i & 4) != 0) {
                (x0, y0) = (y0, x0);
            }

            var x = NewFloat(x0);
            var y = NewFloat(y0);
            var z = @new<global::go.math.big_package.Float>().SetPrec(53);

            z.Mul(x, y);
            var (got, _) = z.Float64();
            var want = x0 * y0;
            if (got != want) {
                t.Errorf("%g * %g = %g; want %g", x0, y0, got, want);
            }

            if (y0 == 0D) {
                continue; // avoid division-by-zero
            }
            z.Quo(z, y);
            (got, _) = z.Float64();
            want /= y0;
            if (got != want) {
                t.Errorf("%g / %g = %g; want %g", x0 * y0, y0, got, want);
            }
        }
    }
}

public static void TestIssue6866(ж<testing.T> t) {
    foreach (var (_, prec) in precList.ΔRangeSnapshot()) {
        var two = @new<global::go.math.big_package.Float>().SetPrec(prec).SetInt64(2);
        var one = @new<global::go.math.big_package.Float>().SetPrec(prec).SetInt64(1);
        var three = @new<global::go.math.big_package.Float>().SetPrec(prec).SetInt64(3);
        var msix = @new<global::go.math.big_package.Float>().SetPrec(prec).SetInt64(-6);
        var psix = @new<global::go.math.big_package.Float>().SetPrec(prec).SetInt64(+6);

        var p = @new<global::go.math.big_package.Float>().SetPrec(prec);
        var z1 = @new<global::go.math.big_package.Float>().SetPrec(prec);
        var z2 = @new<global::go.math.big_package.Float>().SetPrec(prec);

        // z1 = 2 + 1.0/3*-6
        p.Quo(one, three);
        p.Mul(p, msix);
        z1.Add(two, p);

        // z2 = 2 - 1.0/3*+6
        p.Quo(one, three);
        p.Mul(p, psix);
        z2.Sub(two, p);

        if (z1.Cmp(z2) != 0) {
            t.Fatalf("prec %d: got z1 = %v != z2 = %v; want z1 == z2\n", prec, z1.OrTypedNil(), z2.OrTypedNil());
        }
        if (z1.Sign() != 0) {
            t.Errorf("prec %d: got z1 = %v; want 0", prec, z1.OrTypedNil());
        }
        if (z2.Sign() != 0) {
            t.Errorf("prec %d: got z2 = %v; want 0", prec, z2.OrTypedNil());
        }
    }
}

public static void TestFloatQuo(ж<testing.T> t) {
    var bitsᴷ = new stackAppend<nint>();

    // TODO(gri) make the test vary these precisions
    nint preci = 200; // precision of integer part
    nint precf = 20;  // precision of fractional part

    for (nint i = 0; i < 8; i++) {
        // compute accurate (not rounded) result z
        var bits = new ΔBits(new nint[]{preci - 1}.slice());
        if ((i & 3) != 0) {
            bits = append(bitsᴷ, bits, (nint)0);
        }
        if ((i & 2) != 0) {
            bits = append(bits, (nint)(-1));
        }
        if ((i & 1) != 0) {
            bits = append(bits, -precf);
        }
        var z = bits.ΔFloat();

        // compute accurate x as z*y
        var y = NewFloat(3.14159265358979323e123D);

        var x = @new<global::go.math.big_package.Float>().SetPrec(z.Prec() + y.Prec()).SetMode(ToZero);
        x.Mul(z, y);

        // leave for debugging
        // fmt.Printf("x = %s\ny = %s\nz = %s\n", x, y, z)

        {
            var got = x.Acc(); if (got != Exact) {
                t.Errorf("got acc = %s; want exact", got);
            }
        }

        // round accurate z for a variety of precisions and
        // modes and compare against result of x / y.
        foreach (var (_, mode) in new global::go.math.big_package.RoundingMode[]{ToZero, ToNearestEven, AwayFromZero}.array()) {
            for (nint d = -5; d < 5; d++) {
                nuint prec = (nuint)(preci + d);
                var got = @new<global::go.math.big_package.Float>().SetPrec(prec).SetMode(mode).Quo(x, y);
                var want = bits.round(prec, mode);
                if (got.Cmp(want) != 0) {
                    t.Errorf("i = %d, prec = %d, %s:\n\t     %s\n\t/    %s\n\t=    %s\n\twant %s",
                        i, prec, mode, x.OrTypedNil(), y.OrTypedNil(), got.OrTypedNil(), want.OrTypedNil());
                }
            }
        }
    }
}

internal static ж<bool> @long = flag.Bool("long", false, "run very long tests");

// TestFloatQuoSmoke tests all divisions x/y for values x, y in the range [-n, +n];
// it serves as a smoke test for basic correctness of division.
public static void TestFloatQuoSmoke(ж<testing.T> t) {
    nint n = 10;
    if (@long.Value) {
        n = 1000;
    }

    UntypedInt dprec = 3;                  // max. precision variation
    const nint prec = /* 10 + dprec */ 13; // enough bits to hold n precisely
    for (nint x = -n; x <= n; x++) {
        for (nint y = -n; y < n; y++) {
            if (y == 0) {
                continue;
            }

            var a = (float64)x;
            var b = (float64)y;
            var c = a / b;

            // vary operand precision (only ok as long as a, b can be represented correctly)
            for (nint ad = -dprec; ad <= dprec; ad++) {
                for (nint bd = -dprec; bd <= dprec; bd++) {
                    var A = @new<global::go.math.big_package.Float>().SetPrec((nuint)(prec + ad)).SetFloat64(a);
                    var B = @new<global::go.math.big_package.Float>().SetPrec((nuint)(prec + bd)).SetFloat64(b);
                    var C = @new<global::go.math.big_package.Float>().SetPrec(53).Quo(A, B); // C has float64 mantissa width

                    var (cc, acc) = C.Float64();
                    if (cc != c) {
                        t.Errorf("%g/%g = %s; want %.5g\n", a, b, C.Text((byte)'g', 5), c);
                        continue;
                    }
                    if (acc != Exact) {
                        t.Errorf("%g/%g got %s result; want exact result", a, b, acc);
                    }
                }
            }
        }
    }
}

// TestFloatArithmeticSpecialValues tests that Float operations produce the
// correct results for combinations of zero (±0), finite (±1 and ±2.71828),
// and infinite (±Inf) operands.
public static void TestFloatArithmeticSpecialValues(ж<testing.T> t) {
    var zero = 0.0D;
    var args = slice<float64>([math.Inf(-1), -2.71828D, -1D, -zero, zero, 1D, 2.71828D, math.Inf(1)]);
    var xx = @new<global::go.math.big_package.Float>();
    var yy = @new<global::go.math.big_package.Float>();
    var got = @new<global::go.math.big_package.Float>();
    var want = @new<global::go.math.big_package.Float>();
    for (nint i = 0; i < 4; i++) {
        foreach (var (_, x) in args) {
            xx.SetFloat64(x);
            // check conversion is correct
            // (no need to do this for y, since we see exactly the
            // same values there)
            {
                var (gotΔ1, acc) = xx.Float64(); if (gotΔ1 != x || acc != Exact) {
                    t.Errorf("Float(%g) == %g (%s)", x, gotΔ1, acc);
                }
            }
            foreach (var (_, y) in args) {
                yy.SetFloat64(y);
                @string op = default;
                float64 z = default;
                ref var f = ref heap<Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>>(out var Ꮡf);
                switch (i) {
                case 0: {
                    op = "+";
                    z = x + y;
                    f = ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Add));
                    break;
                }
                case 1: {
                    op = "-";
                    z = x - y;
                    f = ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Sub));
                    break;
                }
                case 2: {
                    op = "*";
                    z = x * y;
                    f = ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Mul));
                    break;
                }
                case 3: {
                    op = "/";
                    z = x / y;
                    f = ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Quo));
                    break;
                }
                default: {
                    throw panic("unreachable");
                    break;
                }}
                bool errnan = default; // set if execution of f panicked with ErrNaN
                // protect execution of f
                var gotʗ1 = got;
                var xxʗ1 = xx;
                var yyʗ1 = yy;
                func(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(() => {
                            {
                                var p = recover(); if (p != nil) {
                                    _ = p._<ErrNaN>(); // re-panic if not ErrNaN
                                    errnan = true;
                                }
                            }
                        }, ref ᒐ);
                        Ꮡf.ValueSlot(gotʗ1, xxʗ1, yyʗ1);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                })();
                if (math.IsNaN(z)) {
                    if (!errnan) {
                        t.Errorf("%5g %s %5g = %5s; want ErrNaN panic", x, op, y, got.OrTypedNil());
                    }
                    continue;
                }
                if (errnan) {
                    t.Errorf("%5g %s %5g panicked with ErrNan; want %5s", x, op, y, want.OrTypedNil());
                    continue;
                }
                want.SetFloat64(z);
                if (!alike(got, want)) {
                    t.Errorf("%5g %s %5g = %5s; want %5s", x, op, y, got.OrTypedNil(), want.OrTypedNil());
                }
            }
        }
    }
}

internal partial struct TestFloatArithmeticOverflow_type /*dyn*/ {
    internal nuint prec;
    internal global::go.math.big_package.RoundingMode mode;
    internal byte op;
    internal @string x, y, want;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatArithmeticOverflow(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatArithmeticOverflow_type>([
        new(4, ToNearestEven, (byte)'+', "0", "0", "0", Exact),                   // smoke test
        new(4, ToNearestEven, (byte)'+', "0x.8p+0", "0x.8p+0", "0x.8p+1", Exact), // smoke test

        new(4, ToNearestEven, (byte)'+', "0", "0x.8p2147483647", "0x.8p+2147483647", Exact),
        new(4, ToNearestEven, (byte)'+', "0x.8p2147483500", "0x.8p2147483647", "0x.8p+2147483647", Below), // rounded to zero
        new(4, ToNearestEven, (byte)'+', "0x.8p2147483647", "0x.8p2147483647", "+Inf", Above),             // exponent overflow in +
        new(4, ToNearestEven, (byte)'+', "-0x.8p2147483647", "-0x.8p2147483647", "-Inf", Below),           // exponent overflow in +
        new(4, ToNearestEven, (byte)'-', "-0x.8p2147483647", "0x.8p2147483647", "-Inf", Below),            // exponent overflow in -

        new(4, ToZero, (byte)'+', "0x.fp2147483647", "0x.8p2147483643", "0x.fp+2147483647", Below), // rounded to zero
        new(4, ToNearestEven, (byte)'+', "0x.fp2147483647", "0x.8p2147483643", "+Inf", Above),      // exponent overflow in rounding
        new(4, AwayFromZero, (byte)'+', "0x.fp2147483647", "0x.8p2147483643", "+Inf", Above),       // exponent overflow in rounding

        new(4, AwayFromZero, (byte)'-', "-0x.fp2147483647", "0x.8p2147483644", "-Inf", Below),        // exponent overflow in rounding
        new(4, ToNearestEven, (byte)'-', "-0x.fp2147483647", "0x.8p2147483643", "-Inf", Below),       // exponent overflow in rounding
        new(4, ToZero, (byte)'-', "-0x.fp2147483647", "0x.8p2147483643", "-0x.fp+2147483647", Above), // rounded to zero

        new(4, ToNearestEven, (byte)'+', "0", "0x.8p-2147483648", "0x.8p-2147483648", Exact),
        new(4, ToNearestEven, (byte)'+', "0x.8p-2147483648", "0x.8p-2147483648", "0x.8p-2147483647", Exact),

        new(4, ToNearestEven, (byte)'*', "1", "0x.8p2147483647", "0x.8p+2147483647", Exact),
        new(4, ToNearestEven, (byte)'*', "2", "0x.8p2147483647", "+Inf", Above),  // exponent overflow in *
        new(4, ToNearestEven, (byte)'*', "-2", "0x.8p2147483647", "-Inf", Below), // exponent overflow in *

        new(4, ToNearestEven, (byte)'/', "0.5", "0x.8p2147483647", "0x.8p-2147483646", Exact),
        new(4, ToNearestEven, (byte)'/', "0x.8p+0", "0x.8p2147483647", "0x.8p-2147483646", Exact),
        new(4, ToNearestEven, (byte)'/', "0x.8p-1", "0x.8p2147483647", "0x.8p-2147483647", Exact),
        new(4, ToNearestEven, (byte)'/', "0x.8p-2", "0x.8p2147483647", "0x.8p-2147483648", Exact),
        new(4, ToNearestEven, (byte)'/', "0x.8p-3", "0x.8p2147483647", "0", Below) // exponent underflow in /
    ])) {
        var x = makeFloat(test.x);
        var y = makeFloat(test.y);
        var z = @new<global::go.math.big_package.Float>().SetPrec(test.prec).SetMode(test.mode);
        switch (test.op) {
        case (byte)'+': {
            z.Add(x, y);
            break;
        }
        case (byte)'-': {
            z.Sub(x, y);
            break;
        }
        case (byte)'*': {
            z.Mul(x, y);
            break;
        }
        case (byte)'/': {
            z.Quo(x, y);
            break;
        }
        default: {
            throw panic("unreachable");
            break;
        }}
        {
            @string got = z.Text((byte)'p', 0); if (got != test.want || z.Acc() != test.acc) {
                t.Errorf(
                    "prec = %d (%s): %s %c %s = %s (%s); want %s (%s)",
                    test.prec, test.mode, x.Text((byte)'p', 0), test.op, y.Text((byte)'p', 0), got, z.Acc(), test.want, test.acc);
            }
        }
    }
}

internal partial struct TestFloatArithmeticRounding_type /*dyn*/ {
    internal global::go.math.big_package.RoundingMode mode;
    internal nuint prec;
    internal int64 x, y, want;
    internal byte op;
}

// TODO(gri) Add tests that check correctness in the presence of aliasing.

// For rounding modes ToNegativeInf and ToPositiveInf, rounding is affected
// by the sign of the value to be rounded. Test that rounding happens after
// the sign of a result has been set.
// This test uses specific values that are known to fail if rounding is
// "factored" out before setting the result sign.
public static void TestFloatArithmeticRounding(ж<testing.T> t) {
    foreach (var (_, test) in slice<TestFloatArithmeticRounding_type>([
        new(ToZero, 3, -0x8, -0x1, -0x8, (byte)'+'),
        new(AwayFromZero, 3, -0x8, -0x1, -0xa, (byte)'+'),
        new(ToNegativeInf, 3, -0x8, -0x1, -0xa, (byte)'+'),

        new(ToZero, 3, -0x8, 0x1, -0x8, (byte)'-'),
        new(AwayFromZero, 3, -0x8, 0x1, -0xa, (byte)'-'),
        new(ToNegativeInf, 3, -0x8, 0x1, -0xa, (byte)'-'),

        new(ToZero, 3, -0x9, 0x1, -0x8, (byte)'*'),
        new(AwayFromZero, 3, -0x9, 0x1, -0xa, (byte)'*'),
        new(ToNegativeInf, 3, -0x9, 0x1, -0xa, (byte)'*'),

        new(ToZero, 3, -0x9, 0x1, -0x8, (byte)'/'),
        new(AwayFromZero, 3, -0x9, 0x1, -0xa, (byte)'/'),
        new(ToNegativeInf, 3, -0x9, 0x1, -0xa, (byte)'/')
    ])) {
        ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
        ref var y = ref heap(new global::go.math.big_package.Float(), out var Ꮡy);
        ref var z = ref heap(new global::go.math.big_package.Float(), out var Ꮡz);
        Ꮡx.SetInt64(test.x);
        Ꮡy.SetInt64(test.y);
        Ꮡz.SetPrec(test.prec).SetMode(test.mode);
        switch (test.op) {
        case (byte)'+': {
            Ꮡz.Add(Ꮡx, Ꮡy);
            break;
        }
        case (byte)'-': {
            Ꮡz.Sub(Ꮡx, Ꮡy);
            break;
        }
        case (byte)'*': {
            Ꮡz.Mul(Ꮡx, Ꮡy);
            break;
        }
        case (byte)'/': {
            Ꮡz.Quo(Ꮡx, Ꮡy);
            break;
        }
        default: {
            throw panic("unreachable");
            break;
        }}
        {
            var (got, acc) = Ꮡz.Int64(); if (got != test.want || acc != Exact) {
                t.Errorf("%s, %d bits: %d %c %d = %d (%s); want %d (Exact)",
                    test.mode, test.prec, test.x, test.op, test.y, got, acc, test.want);
            }
        }
    }
}

// TestFloatCmpSpecialValues tests that Cmp produces the correct results for
// combinations of zero (±0), finite (±1 and ±2.71828), and infinite (±Inf)
// operands.
public static void TestFloatCmpSpecialValues(ж<testing.T> t) {
    var zero = 0.0D;
    var args = slice<float64>([math.Inf(-1), -2.71828D, -1D, -zero, zero, 1D, 2.71828D, math.Inf(1)]);
    var xx = @new<global::go.math.big_package.Float>();
    var yy = @new<global::go.math.big_package.Float>();
    for (nint i = 0; i < 4; i++) {
        foreach (var (_, x) in args) {
            xx.SetFloat64(x);
            // check conversion is correct
            // (no need to do this for y, since we see exactly the
            // same values there)
            {
                var (got, acc) = xx.Float64(); if (got != x || acc != Exact) {
                    t.Errorf("Float(%g) == %g (%s)", x, got, acc);
                }
            }
            foreach (var (_, y) in args) {
                yy.SetFloat64(y);
                nint got = xx.Cmp(yy);
                nint want = 0;
                switch (ᐧ) {
                case {} when x < y: {
                    want = -1;
                    break;
                }
                case {} when x > y: {
                    want = +1;
                    break;
                }}
                if (got != want) {
                    t.Errorf("(%g).Cmp(%g) = %v; want %v", x, y, got, want);
                }
            }
        }
    }
}

public static void BenchmarkFloatAdd(ж<testing.B> b) {
    var x = @new<global::go.math.big_package.Float>();
    var y = @new<global::go.math.big_package.Float>();
    var z = @new<global::go.math.big_package.Float>();

    foreach (var (_, prec) in slice<nuint>([10, 100, 1000, 10000, 100000])) {
        x.SetPrec(prec).SetRat(NewRat(1, 3));
        y.SetPrec(prec).SetRat(NewRat(1, 6));
        z.SetPrec(prec);

        var xʗ1 = x;
        var yʗ1 = y;
        var zʗ1 = z;
        b.Run(fmt.Sprintf("%v"u8, prec), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                zʗ1.Add(xʗ1, yʗ1);
            }
        });
    }
}

public static void BenchmarkFloatSub(ж<testing.B> b) {
    var x = @new<global::go.math.big_package.Float>();
    var y = @new<global::go.math.big_package.Float>();
    var z = @new<global::go.math.big_package.Float>();

    foreach (var (_, prec) in slice<nuint>([10, 100, 1000, 10000, 100000])) {
        x.SetPrec(prec).SetRat(NewRat(1, 3));
        y.SetPrec(prec).SetRat(NewRat(1, 6));
        z.SetPrec(prec);

        var xʗ1 = x;
        var yʗ1 = y;
        var zʗ1 = z;
        b.Run(fmt.Sprintf("%v"u8, prec), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                zʗ1.Sub(xʗ1, yʗ1);
            }
        });
    }
}

} // end big_internal_test_package
