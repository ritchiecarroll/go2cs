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
internal static error _ᴛ1ʗ = new big_test_package.big_ErrNaNᴠerror(new ErrNaN(nil));

internal static uint64 uint64(this ж<global::go.math.big_package.Float> Ꮡx) {
    var (u, acc) = Ꮡx.Uint64();
    if (acc != Exact) {
        throw panic(fmt.Sprintf("%s is not a uint64"u8, Ꮡx.Text((rune)'g', 10)));
    }
    return u;
}

internal static int64 int64(this ж<global::go.math.big_package.Float> Ꮡx) {
    var (i, acc) = Ꮡx.Int64();
    if (acc != Exact) {
        throw panic(fmt.Sprintf("%s is not an int64"u8, Ꮡx.Text((rune)'g', 10)));
    }
    return i;
}

internal partial struct TestFloatZeroValue_type /*dyn*/ {
    internal nint z, x, y, want;
    internal rune opname;
    internal Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>> op;
}

public static void TestFloatZeroValue(ж<testing.T> Ꮡt) {
    // zero (uninitialized) value is a ready-to-use 0.0
    ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
    {
        @string s = Ꮡx.Text((rune)'f', 1); if (s != "0.0"u8) {
            Ꮡt.Errorf("zero value = %s; want 0.0"u8, s);
        }
    }
    // zero value has precision 0
    {
        nuint prec = x.Prec(); if (prec != 0) {
            Ꮡt.Errorf("prec = %d; want 0"u8, prec);
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
    foreach (var (_, test) in new TestFloatZeroValue_type[]{
        new(0, 0, 0, 0, (rune)'+', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Add))),
        new(0, 1, 2, 3, (rune)'+', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Add))),
        new(1, 2, 0, 2, (rune)'+', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Add))),
        new(2, 0, 1, 1, (rune)'+', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Add))),
        new(0, 0, 0, 0, (rune)'-', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Sub))),
        new(0, 1, 2, -1, (rune)'-', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Sub))),
        new(1, 2, 0, 2, (rune)'-', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Sub))),
        new(2, 0, 1, -1, (rune)'-', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Sub))),
        new(0, 0, 0, 0, (rune)'*', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Mul))),
        new(0, 1, 2, 2, (rune)'*', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Mul))),
        new(1, 2, 0, 0, (rune)'*', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Mul))),
        new(2, 0, 1, 0, (rune)'*', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Mul))), // {0, 0, 0, 0, '/', (*Float).Quo}, // panics

        new(0, 2, 1, 2, (rune)'/', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Quo))),
        new(1, 2, 0, 0, (rune)'/', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Quo))), // = +Inf

        new(2, 0, 1, 0, (rune)'/', ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Quo)))
    }.slice()) {
        var z = make(test.z);
        test.op(z, make(test.x), make(test.y));
        nint got = 0;
        if (!z.IsInf()) {
            got = (nint)z.int64();
        }
        if (got != test.want) {
            Ꮡt.Errorf("%d %c %d = %d; want %d"u8, test.x, test.opname, test.y, got, test.want);
        }
    }
}

// TODO(gri) test how precision is set for zero value results
internal static ж<global::go.math.big_package.Float> makeFloat(@string s) {
    var (x, _, err) = ParseFloat(s, 0, 1000, ToNearestEven);
    if (err != default!) {
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

public static void TestFloatSetPrec(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatSetPrec_type[]{ // prec 0

        new("0"u8, 0, "0"u8, Exact),
        new("-0"u8, 0, "-0"u8, Exact),
        new("-Inf"u8, 0, "-Inf"u8, Exact),
        new("+Inf"u8, 0, "+Inf"u8, Exact),
        new("123"u8, 0, "0"u8, Below),
        new("-123"u8, 0, "-0"u8, Above), // prec at upper limit

        new("0"u8, MaxPrec, "0"u8, Exact),
        new("-0"u8, MaxPrec, "-0"u8, Exact),
        new("-Inf"u8, MaxPrec, "-Inf"u8, Exact),
        new("+Inf"u8, MaxPrec, "+Inf"u8, Exact), // just a few regular cases - general rounding is tested elsewhere

        new("1.5"u8, 1, "2"u8, Above),
        new("-1.5"u8, 1, "-2"u8, Below),
        new("123"u8, 1000000, "123"u8, Exact),
        new("-123"u8, 1000000, "-123"u8, Exact)
    }.slice()) {
        var x = makeFloat(test.x).SetPrec(test.prec);
        nuint prec = test.prec;
        if (prec > MaxPrec) {
            prec = MaxPrec;
        }
        {
            nuint got = x.Prec(); if (got != prec) {
                Ꮡt.Errorf("%s.SetPrec(%d).Prec() == %d; want %d"u8, test.x, test.prec, got, prec);
            }
        }
        {
            @string got = x.String();
            var acc = x.Acc(); if (got != test.want || acc != test.acc) {
                Ꮡt.Errorf("%s.SetPrec(%d) = %s (%s); want %s (%s)"u8, test.x, test.prec, got, acc, test.want, test.acc);
            }
        }
    }
}

internal partial struct TestFloatMinPrec_type /*dyn*/ {
    internal @string x;
    internal nuint want;
}

public static void TestFloatMinPrec(ж<testing.T> Ꮡt) {
    const nuint max = 100;
    foreach (var (_, test) in new TestFloatMinPrec_type[]{
        new("0"u8, 0),
        new("-0"u8, 0),
        new("+Inf"u8, 0),
        new("-Inf"u8, 0),
        new("1"u8, 1),
        new("2"u8, 1),
        new("3"u8, 2),
        new("0x8001"u8, 16),
        new("0x8001p-1000"u8, 16),
        new("0x8001p+1000"u8, 16),
        new("0.1"u8, max)
    }.slice()) {
        var x = makeFloat(test.x).SetPrec(max);
        {
            nuint got = x.MinPrec(); if (got != test.want) {
                Ꮡt.Errorf("%s.MinPrec() = %d; want %d"u8, test.x, got, test.want);
            }
        }
    }
}

internal partial struct TestFloatSign_type /*dyn*/ {
    internal @string x;
    internal nint s;
}

public static void TestFloatSign(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatSign_type[]{
        new("-Inf"u8, -1),
        new("-1"u8, -1),
        new("-0"u8, 0),
        new("+0"u8, 0),
        new("+1"u8, +1),
        new("+Inf"u8, +1)
    }.slice()) {
        var x = makeFloat(test.x);
        nint s = x.Sign();
        if (s != test.s) {
            Ꮡt.Errorf("%s.Sign() = %d; want %d"u8, test.x, s, test.s);
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

public static void TestFloatMantExp(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatMantExp_type[]{
        new("0"u8, "0"u8, 0),
        new("+0"u8, "0"u8, 0),
        new("-0"u8, "-0"u8, 0),
        new("Inf"u8, "+Inf"u8, 0),
        new("+Inf"u8, "+Inf"u8, 0),
        new("-Inf"u8, "-Inf"u8, 0),
        new("1.5"u8, "0.75"u8, 1),
        new("1.024e3"u8, "0.5"u8, 11),
        new("-0.125"u8, "-0.5"u8, -2)
    }.slice()) {
        var x = makeFloat(test.x);
        var mant = makeFloat(test.mant);
        var m = @new<global::go.math.big_package.Float>();
        nint e = x.MantExp(m);
        if (!alike(m, mant) || e != test.exp) {
            Ꮡt.Errorf("%s.MantExp() = %s, %d; want %s, %d"u8, test.x, m.Text((rune)'g', 10), e, test.mant, test.exp);
        }
    }
}

public static void TestFloatMantExpAliasing(ж<testing.T> Ꮡt) {
    var x = makeFloat("0.5p10"u8);
    {
        nint e = x.MantExp(x); if (e != 10) {
            Ꮡt.Fatalf("Float.MantExp aliasing error: got %d; want 10"u8, e);
        }
    }
    {
        var want = makeFloat("0.5"u8); if (!alike(x, want)) {
            Ꮡt.Fatalf("Float.MantExp aliasing error: got %s; want %s"u8, x.Text((rune)'g', 10), want.Text((rune)'g', 10));
        }
    }
}

internal partial struct TestFloatSetMantExp_type /*dyn*/ {
    internal @string frac;
    internal nint exp;
    internal @string z;
}

public static void TestFloatSetMantExp(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatSetMantExp_type[]{
        new("0"u8, 0, "0"u8),
        new("+0"u8, 0, "0"u8),
        new("-0"u8, 0, "-0"u8),
        new("Inf"u8, 1234, "+Inf"u8),
        new("+Inf"u8, -1234, "+Inf"u8),
        new("-Inf"u8, -1234, "-Inf"u8),
        new("0"u8, MinExp, "0"u8),
        new("0.25"u8, MinExp, "+0"u8), // exponent underflow

        new("-0.25"u8, MinExp, "-0"u8), // exponent underflow

        new("1"u8, MaxExp, "+Inf"u8), // exponent overflow

        new("2"u8, MaxExp - 1, "+Inf"u8), // exponent overflow

        new("0.75"u8, 1, "1.5"u8),
        new("0.5"u8, 11, "1024"u8),
        new("-0.5"u8, -2, "-0.125"u8),
        new("32"u8, 5, "1024"u8),
        new("1024"u8, -10, "1"u8)
    }.slice()) {
        var frac = makeFloat(test.frac);
        var want = makeFloat(test.z);
        ref var z = ref heap(new global::go.math.big_package.Float(), out var Ꮡz);
        Ꮡz.SetMantExp(frac, test.exp);
        if (!alike(Ꮡz, want)) {
            Ꮡt.Errorf("SetMantExp(%s, %d) = %s; want %s"u8, test.frac, test.exp, Ꮡz.Text((rune)'g', 10), test.z);
        }
        // test inverse property
        var mant = @new<global::go.math.big_package.Float>();
        if (Ꮡz.SetMantExp(mant, want.MantExp(mant)).Cmp(want) != 0) {
            Ꮡt.Errorf("Inverse property not satisfied: got %s; want %s"u8, Ꮡz.Text((rune)'g', 10), test.z);
        }
    }
}

internal partial struct TestFloatPredicates_type /*dyn*/ {
    internal @string x;
    internal nint sign;
    internal bool signbit, inf;
}

public static void TestFloatPredicates(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatPredicates_type[]{
        new(x: "-Inf"u8, sign: -1, signbit: true, inf: true),
        new(x: "-1"u8, sign: -1, signbit: true),
        new(x: "-0"u8, signbit: true),
        new(x: "0"u8),
        new(x: "1"u8, sign: 1),
        new(x: "+Inf"u8, sign: 1, inf: true)
    }.slice()) {
        var x = makeFloat(test.x);
        {
            var got = x.Signbit(); if (got != test.signbit) {
                Ꮡt.Errorf("(%s).Signbit() = %v; want %v"u8, test.x, got, test.signbit);
            }
        }
        {
            nint got = x.Sign(); if (got != test.sign) {
                Ꮡt.Errorf("(%s).Sign() = %d; want %d"u8, test.x, got, test.sign);
            }
        }
        {
            var got = x.IsInf(); if (got != test.inf) {
                Ꮡt.Errorf("(%s).IsInf() = %v; want %v"u8, test.x, got, test.inf);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string intˢ = " int"u8;

public static void TestFloatIsInt(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, test) in new @string[]{
        "0 int"u8,
        "-0 int"u8,
        "1 int"u8,
        "-1 int"u8,
        "0.5"u8,
        "1.23"u8,
        "1.23e1"u8,
        "1.23e2 int"u8,
        "0.000000001e+8"u8,
        "0.000000001e+9 int"u8,
        "1.2345e200 int"u8,
        "Inf"u8,
        "+Inf"u8,
        "-Inf"u8
    }.slice()) {
        @string s = strings.TrimSuffix(test, intˢ);
        var want = s != test;
        {
            var got = makeFloat(s).IsInt(); if (got != want) {
                Ꮡt.Errorf("%s.IsInt() == %t"u8, s, got);
            }
        }
    }
}

internal static int64 fromBinary(@string s) {
    var (x, err) = strconv.ParseInt(s, 2, 64);
    if (err != default!) {
        throw panic(err);
    }
    return x;
}

internal static @string toBinary(int64 x) {
    return strconv.FormatInt(x, 2);
}

internal static void testFloatRound(ж<testing.T> Ꮡt, int64 x, int64 r, nuint prec, global::go.math.big_package.RoundingMode mode) {
    // verify test data
    bool ok = default!;
    var exprᴛ1 = mode;
    if (exprᴛ1 == ToNearestEven || exprᴛ1 == ToNearestAway) {
        ok = true; // nothing to do for now
    }
    else if (exprᴛ1 == ToZero) {
        if (x < 0){
            ok = r >= x;
        } else {
            ok = r <= x;
        }
    }
    else if (exprᴛ1 == AwayFromZero) {
        if (x < 0){
            ok = r <= x;
        } else {
            ok = r >= x;
        }
    }
    else if (exprᴛ1 == ToNegativeInf) {
        ok = r <= x;
    }
    else if (exprᴛ1 == ToPositiveInf) {
        ok = r >= x;
    }
    else { /* default: */
        throw panic("unreachable");
    }

    if (!ok) {
        Ꮡt.Fatalf("incorrect test data for prec = %d, %s: x = %s, r = %s"u8, prec, mode, toBinary(x), toBinary(r));
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
        Ꮡt.Errorf("round %s (%d bits, %s) incorrect: got %s (%d bits, %s); want %s (%d bits, %s)"u8,
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
        Ꮡt.Errorf("round %s (%d bits, %s) not symmetric: got %s and %s; want %s"u8,
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
        Ꮡt.Errorf("round %s (%d bits, %s) not idempotent: got %s and %s; want %s"u8,
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
public static void TestFloatRound(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatRound_type[]{
        new(5, "1000"u8, "1000"u8, "1000"u8, "1000"u8, "1000"u8),
        new(5, "1001"u8, "1001"u8, "1001"u8, "1001"u8, "1001"u8),
        new(5, "1010"u8, "1010"u8, "1010"u8, "1010"u8, "1010"u8),
        new(5, "1011"u8, "1011"u8, "1011"u8, "1011"u8, "1011"u8),
        new(5, "1100"u8, "1100"u8, "1100"u8, "1100"u8, "1100"u8),
        new(5, "1101"u8, "1101"u8, "1101"u8, "1101"u8, "1101"u8),
        new(5, "1110"u8, "1110"u8, "1110"u8, "1110"u8, "1110"u8),
        new(5, "1111"u8, "1111"u8, "1111"u8, "1111"u8, "1111"u8),
        new(4, "1000"u8, "1000"u8, "1000"u8, "1000"u8, "1000"u8),
        new(4, "1001"u8, "1001"u8, "1001"u8, "1001"u8, "1001"u8),
        new(4, "1010"u8, "1010"u8, "1010"u8, "1010"u8, "1010"u8),
        new(4, "1011"u8, "1011"u8, "1011"u8, "1011"u8, "1011"u8),
        new(4, "1100"u8, "1100"u8, "1100"u8, "1100"u8, "1100"u8),
        new(4, "1101"u8, "1101"u8, "1101"u8, "1101"u8, "1101"u8),
        new(4, "1110"u8, "1110"u8, "1110"u8, "1110"u8, "1110"u8),
        new(4, "1111"u8, "1111"u8, "1111"u8, "1111"u8, "1111"u8),
        new(3, "1000"u8, "1000"u8, "1000"u8, "1000"u8, "1000"u8),
        new(3, "1001"u8, "1000"u8, "1000"u8, "1010"u8, "1010"u8),
        new(3, "1010"u8, "1010"u8, "1010"u8, "1010"u8, "1010"u8),
        new(3, "1011"u8, "1010"u8, "1100"u8, "1100"u8, "1100"u8),
        new(3, "1100"u8, "1100"u8, "1100"u8, "1100"u8, "1100"u8),
        new(3, "1101"u8, "1100"u8, "1100"u8, "1110"u8, "1110"u8),
        new(3, "1110"u8, "1110"u8, "1110"u8, "1110"u8, "1110"u8),
        new(3, "1111"u8, "1110"u8, "10000"u8, "10000"u8, "10000"u8),
        new(3, "1000001"u8, "1000000"u8, "1000000"u8, "1000000"u8, "1010000"u8),
        new(3, "1001001"u8, "1000000"u8, "1010000"u8, "1010000"u8, "1010000"u8),
        new(3, "1010001"u8, "1010000"u8, "1010000"u8, "1010000"u8, "1100000"u8),
        new(3, "1011001"u8, "1010000"u8, "1100000"u8, "1100000"u8, "1100000"u8),
        new(3, "1100001"u8, "1100000"u8, "1100000"u8, "1100000"u8, "1110000"u8),
        new(3, "1101001"u8, "1100000"u8, "1110000"u8, "1110000"u8, "1110000"u8),
        new(3, "1110001"u8, "1110000"u8, "1110000"u8, "1110000"u8, "10000000"u8),
        new(3, "1111001"u8, "1110000"u8, "10000000"u8, "10000000"u8, "10000000"u8),
        new(2, "1000"u8, "1000"u8, "1000"u8, "1000"u8, "1000"u8),
        new(2, "1001"u8, "1000"u8, "1000"u8, "1000"u8, "1100"u8),
        new(2, "1010"u8, "1000"u8, "1000"u8, "1100"u8, "1100"u8),
        new(2, "1011"u8, "1000"u8, "1100"u8, "1100"u8, "1100"u8),
        new(2, "1100"u8, "1100"u8, "1100"u8, "1100"u8, "1100"u8),
        new(2, "1101"u8, "1100"u8, "1100"u8, "1100"u8, "10000"u8),
        new(2, "1110"u8, "1100"u8, "10000"u8, "10000"u8, "10000"u8),
        new(2, "1111"u8, "1100"u8, "10000"u8, "10000"u8, "10000"u8),
        new(2, "1000001"u8, "1000000"u8, "1000000"u8, "1000000"u8, "1100000"u8),
        new(2, "1001001"u8, "1000000"u8, "1000000"u8, "1000000"u8, "1100000"u8),
        new(2, "1010001"u8, "1000000"u8, "1100000"u8, "1100000"u8, "1100000"u8),
        new(2, "1011001"u8, "1000000"u8, "1100000"u8, "1100000"u8, "1100000"u8),
        new(2, "1100001"u8, "1100000"u8, "1100000"u8, "1100000"u8, "10000000"u8),
        new(2, "1101001"u8, "1100000"u8, "1100000"u8, "1100000"u8, "10000000"u8),
        new(2, "1110001"u8, "1100000"u8, "10000000"u8, "10000000"u8, "10000000"u8),
        new(2, "1111001"u8, "1100000"u8, "10000000"u8, "10000000"u8, "10000000"u8),
        new(1, "1000"u8, "1000"u8, "1000"u8, "1000"u8, "1000"u8),
        new(1, "1001"u8, "1000"u8, "1000"u8, "1000"u8, "10000"u8),
        new(1, "1010"u8, "1000"u8, "1000"u8, "1000"u8, "10000"u8),
        new(1, "1011"u8, "1000"u8, "1000"u8, "1000"u8, "10000"u8),
        new(1, "1100"u8, "1000"u8, "10000"u8, "10000"u8, "10000"u8),
        new(1, "1101"u8, "1000"u8, "10000"u8, "10000"u8, "10000"u8),
        new(1, "1110"u8, "1000"u8, "10000"u8, "10000"u8, "10000"u8),
        new(1, "1111"u8, "1000"u8, "10000"u8, "10000"u8, "10000"u8),
        new(1, "1000001"u8, "1000000"u8, "1000000"u8, "1000000"u8, "10000000"u8),
        new(1, "1001001"u8, "1000000"u8, "1000000"u8, "1000000"u8, "10000000"u8),
        new(1, "1010001"u8, "1000000"u8, "1000000"u8, "1000000"u8, "10000000"u8),
        new(1, "1011001"u8, "1000000"u8, "1000000"u8, "1000000"u8, "10000000"u8),
        new(1, "1100001"u8, "1000000"u8, "10000000"u8, "10000000"u8, "10000000"u8),
        new(1, "1101001"u8, "1000000"u8, "10000000"u8, "10000000"u8, "10000000"u8),
        new(1, "1110001"u8, "1000000"u8, "10000000"u8, "10000000"u8, "10000000"u8),
        new(1, "1111001"u8, "1000000"u8, "10000000"u8, "10000000"u8, "10000000"u8)
    }.slice()) {
        var x = fromBinary(test.x);
        var z = fromBinary(test.zero);
        var e = fromBinary(test.neven);
        var n = fromBinary(test.naway);
        var a = fromBinary(test.away);
        nuint prec = test.prec;
        testFloatRound(Ꮡt, x, z, prec, ToZero);
        testFloatRound(Ꮡt, x, e, prec, ToNearestEven);
        testFloatRound(Ꮡt, x, n, prec, ToNearestAway);
        testFloatRound(Ꮡt, x, a, prec, AwayFromZero);
        testFloatRound(Ꮡt, x, z, prec, ToNegativeInf);
        testFloatRound(Ꮡt, x, a, prec, ToPositiveInf);
        testFloatRound(Ꮡt, -x, -a, prec, ToNegativeInf);
        testFloatRound(Ꮡt, -x, -z, prec, ToPositiveInf);
    }
}

// TestFloatRound24 tests that rounding a float64 to 24 bits
// matches IEEE 754 rounding to nearest when converting a
// float64 to a float32 (excluding denormal numbers).
public static void TestFloatRound24(ж<testing.T> Ꮡt) {
    const nint x0 = /* 1<<26 - 0x10 */ 67108848; // 11...110000 (26 bits)
    for (nint d = 0; d <= 0x10; d++) {
        var x = (float64)(x0 + d);
        var f = @new<global::go.math.big_package.Float>().SetPrec(24).SetFloat64(x);
        var (got, _) = f.Float32();
        var want = (float32)x;
        if (got != want) {
            Ꮡt.Errorf("Round(%g, 24) = %g; want %g"u8, x, got, want);
        }
    }
}

public static void TestFloatSetUint64(ж<testing.T> Ꮡt) {
    foreach (var (_, want) in new uint64[]{
        0,
        1,
        2,
        10,
        100,
        (uint64)(4294967296L - 1),
        ((uint64)1 << (int)(32)),
        18446744073709551615UL
    }.slice()) {
        ref var f = ref heap(new global::go.math.big_package.Float(), out var Ꮡf);
        Ꮡf.SetUint64(want);
        {
            var got = Ꮡf.uint64(); if (got != want) {
                Ꮡt.Errorf("got %#x (%s); want %#x"u8, got, Ꮡf.Text((rune)'p', 0), want);
            }
        }
    }
    // test basic rounding behavior (exhaustive rounding testing is done elsewhere)
    const uint64 x = 0x8765432187654321;              // 64 bits needed
    for (nuint prec = (nuint)1; prec <= 64; prec++) {
        var f = @new<global::go.math.big_package.Float>().SetPrec(prec).SetMode(ToZero).SetUint64(x);
        var got = f.uint64();
        var want = (uint64)(x & ~(((uint64)1).Lsh((64 - prec)) - 1)); // cut off (round to zero) low 64-prec bits
        if (got != want) {
            Ꮡt.Errorf("got %#x (%s); want %#x"u8, got, f.Text((rune)'p', 0), want);
        }
    }
}

public static void TestFloatSetInt64(ж<testing.T> Ꮡt) {
    foreach (var (_, vᴛ1) in new int64[]{
        0,
        1,
        2,
        10,
        100,
        4294967295L,
        4294967296L,
        9223372036854775807L
    }.slice()) {
        var want = vᴛ1;

        foreach (var (i, _) in new nint[]{}.array(2)) {
            if ((nint)(i & 1) != 0) {
                want = -want;
            }
            ref var f = ref heap(new global::go.math.big_package.Float(), out var Ꮡf);
            Ꮡf.SetInt64(want);
            {
                var got = Ꮡf.int64(); if (got != want) {
                    Ꮡt.Errorf("got %#x (%s); want %#x"u8, got, Ꮡf.Text((rune)'p', 0), want);
                }
            }
        }
    }
    // test basic rounding behavior (exhaustive rounding testing is done elsewhere)
    const int64 x = 0x7654321076543210;              // 63 bits needed
    for (nuint prec = (nuint)1; prec <= 63; prec++) {
        var f = @new<global::go.math.big_package.Float>().SetPrec(prec).SetMode(ToZero).SetInt64(x);
        var got = f.int64();
        var want = (int64)(x & ~(((int64)1).Lsh((63 - prec)) - 1)); // cut off (round to zero) low 63-prec bits
        if (got != want) {
            Ꮡt.Errorf("got %#x (%s); want %#x"u8, got, f.Text((rune)'p', 0), want);
        }
    }
}

public static void TestFloatSetFloat64(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        foreach (var (_, vᴛ1) in new float64[]{
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
        }.slice()) {
            var want = vᴛ1;

            foreach (var (i, _) in new nint[]{}.array(2)) {
                if ((nint)(i & 1) != 0) {
                    want = -want;
                }
                ref var fΔ1 = ref heap(new global::go.math.big_package.Float(), out var ᏑfΔ1);
                ᏑfΔ1.SetFloat64(want);
                {
                    var (got, acc) = ᏑfΔ1.Float64(); if (got != want || acc != Exact) {
                        Ꮡt.Errorf("got %g (%s, %s); want %g (Exact)"u8, got, ᏑfΔ1.Text((rune)'p', 0), acc, want);
                    }
                }
            }
        }
        // test basic rounding behavior (exhaustive rounding testing is done elsewhere)
        const uint64 x = 0x8765432143218;           // 53 bits needed
        for (nuint prec = (nuint)1; prec <= 52; prec++) {
            var fΔ2 = @new<global::go.math.big_package.Float>().SetPrec(prec).SetMode(ToZero).SetFloat64((float64)x);
            var (got, _) = fΔ2.Float64();
            var want = (float64)((uint64)(x & ~(((uint64)1).Lsh((52 - prec)) - 1))); // cut off (round to zero) low 53-prec bits
            if (got != want) {
                Ꮡt.Errorf("got %g (%s); want %g"u8, got, fΔ2.Text((rune)'p', 0), want);
            }
        }
        // test NaN
        defer(() => {
            {
                ref var p = ref heap<global::go.math.big_package.ErrNaN>(out var Ꮡp);
                (p, var ok) = recover()._<ErrNaN>(ᐧ); if (!ok) {
                    Ꮡt.Errorf("got %v; want ErrNaN panic"u8, p);
                }
            }
        }, ref ᒐ);
        ref var f = ref heap(new global::go.math.big_package.Float(), out var Ꮡf);
        Ꮡf.SetFloat64(math.NaN());
        // should not reach here
        Ꮡt.Errorf("got %s; want ErrNaN panic"u8, Ꮡf.Text((rune)'p', 0));
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestFloatSetInt(ж<testing.T> Ꮡt) {
    foreach (var (_, want) in new @string[]{
        "0"u8,
        "1"u8,
        "-1"u8,
        "1234567890"u8,
        "123456789012345678901234567890"u8,
        "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"u8
    }.slice()) {
        ref var x = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡx);
        var (_, ok) = Ꮡx.SetString(want, 0);
        if (!ok) {
            Ꮡt.Errorf("invalid integer %s"u8, want);
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
                Ꮡt.Errorf("got prec = %d; want %d"u8, prec, n);
            }
        }
        // check value
        @string got = Ꮡf.Text((rune)'g', 100);
        if (got != want) {
            Ꮡt.Errorf("got %s (%s); want %s"u8, got, Ꮡf.Text((rune)'p', 0), want);
        }
    }
}

// TODO(gri) test basic rounding behavior
public static void TestFloatSetRat(ж<testing.T> Ꮡt) {
    foreach (var (_, want) in new @string[]{
        "0"u8,
        "1"u8,
        "-1"u8,
        "1234567890"u8,
        "123456789012345678901234567890"u8,
        "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890"u8,
        "1.2"u8,
        "3.14159265"u8
    }.slice()) {
        // TODO(gri) expand
        ref var x = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡx);
        var (_, ok) = Ꮡx.SetString(want);
        if (!ok) {
            Ꮡt.Errorf("invalid fraction %s"u8, want);
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
                Ꮡt.Errorf("got prec = %d; want %d"u8, prec, n);
            }
        }
        @string got = Ꮡf2.Text((rune)'g', 100);
        if (got != want) {
            Ꮡt.Errorf("got %s (%s); want %s"u8, got, Ꮡf2.Text((rune)'p', 0), want);
        }
    }
}

internal partial struct TestFloatSetInf_type /*dyn*/ {
    internal bool signbit;
    internal nuint prec;
    internal @string want;
}

public static void TestFloatSetInf(ж<testing.T> Ꮡt) {
    ref var f = ref heap(new global::go.math.big_package.Float(), out var Ꮡf);
    foreach (var (_, test) in new TestFloatSetInf_type[]{
        new(false, 0, "+Inf"u8),
        new(true, 0, "-Inf"u8),
        new(false, 10, "+Inf"u8),
        new(true, 30, "-Inf"u8)
    }.slice()) {
        var x = Ꮡf.SetPrec(test.prec).SetInf(test.signbit);
        {
            @string got = x.String(); if (got != test.want || x.Prec() != test.prec) {
                Ꮡt.Errorf("SetInf(%v) = %s (prec = %d); want %s (prec = %d)"u8, test.signbit, got, x.Prec(), test.want, test.prec);
            }
        }
    }
}

internal partial struct TestFloatUint64_type /*dyn*/ {
    internal @string x;
    internal uint64 @out;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatUint64(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatUint64_type[]{
        new("-Inf"u8, 0, Above),
        new("-1"u8, 0, Above),
        new("-1e-1000"u8, 0, Above),
        new("-0"u8, 0, Exact),
        new("0"u8, 0, Exact),
        new("1e-1000"u8, 0, Below),
        new("1"u8, 1, Exact),
        new("1.000000000000000000001"u8, 1, Below),
        new("12345.0"u8, 12345, Exact),
        new("12345.000000000000000000001"u8, 12345, Below),
        new("18446744073709551615"u8, 18446744073709551615UL, Exact),
        new("18446744073709551615.000000000000000000001"u8, math.MaxUint64, Below),
        new("18446744073709551616"u8, math.MaxUint64, Below),
        new("1e10000"u8, math.MaxUint64, Below),
        new("+Inf"u8, math.MaxUint64, Below)
    }.slice()) {
        var x = makeFloat(test.x);
        var (@out, acc) = x.Uint64();
        if (@out != test.@out || acc != test.acc) {
            Ꮡt.Errorf("%s: got %d (%s); want %d (%s)"u8, test.x, @out, acc, test.@out, test.acc);
        }
    }
}

internal partial struct TestFloatInt64_type /*dyn*/ {
    internal @string x;
    internal int64 @out;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatInt64(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatInt64_type[]{
        new("-Inf"u8, math.MinInt64, Above),
        new("-1e10000"u8, math.MinInt64, Above),
        new("-9223372036854775809"u8, math.MinInt64, Above),
        new("-9223372036854775808.000000000000000000001"u8, math.MinInt64, Above),
        new("-9223372036854775808"u8, -9223372036854775808L, Exact),
        new("-9223372036854775807.000000000000000000001"u8, -9223372036854775807L, Above),
        new("-9223372036854775807"u8, -9223372036854775807L, Exact),
        new("-12345.000000000000000000001"u8, -12345, Above),
        new("-12345.0"u8, -12345, Exact),
        new("-1.000000000000000000001"u8, -1, Above),
        new("-1.5"u8, -1, Above),
        new("-1"u8, -1, Exact),
        new("-1e-1000"u8, 0, Above),
        new("0"u8, 0, Exact),
        new("1e-1000"u8, 0, Below),
        new("1"u8, 1, Exact),
        new("1.000000000000000000001"u8, 1, Below),
        new("1.5"u8, 1, Below),
        new("12345.0"u8, 12345, Exact),
        new("12345.000000000000000000001"u8, 12345, Below),
        new("9223372036854775807"u8, 9223372036854775807L, Exact),
        new("9223372036854775807.000000000000000000001"u8, math.MaxInt64, Below),
        new("9223372036854775808"u8, math.MaxInt64, Below),
        new("1e10000"u8, math.MaxInt64, Below),
        new("+Inf"u8, math.MaxInt64, Below)
    }.slice()) {
        var x = makeFloat(test.x);
        var (@out, acc) = x.Int64();
        if (@out != test.@out || acc != test.acc) {
            Ꮡt.Errorf("%s: got %d (%s); want %d (%s)"u8, test.x, @out, acc, test.@out, test.acc);
        }
    }
}

internal partial struct TestFloatFloat32_type /*dyn*/ {
    internal @string x;
    internal float32 @out;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatFloat32(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatFloat32_type[]{
        new("0"u8, 0F, Exact), // underflow to zero

        new("1e-1000"u8, 0F, Below),
        new("0x0.000002p-127"u8, 0F, Below),
        new("0x.0000010p-126"u8, 0F, Below), // denormals

        new("1.401298464e-45"u8, math.SmallestNonzeroFloat32, Above), // rounded up to smallest denormal

        new("0x.ffffff8p-149"u8, math.SmallestNonzeroFloat32, Above), // rounded up to smallest denormal

        new("0x.0000018p-126"u8, math.SmallestNonzeroFloat32, Above), // rounded up to smallest denormal

        new("0x.0000020p-126"u8, math.SmallestNonzeroFloat32, Exact),
        new("0x.8p-148"u8, math.SmallestNonzeroFloat32, Exact),
        new("1p-149"u8, math.SmallestNonzeroFloat32, Exact),
        new("0x.fffffep-126"u8, math.Float32frombits(0x7fffff), Exact), // largest denormal
 // special denormal cases (see issues 14553, 14651)

        new("0x0.0000001p-126"u8, math.Float32frombits(0x00000000), Below), // underflow to zero

        new("0x0.0000008p-126"u8, math.Float32frombits(0x00000000), Below), // underflow to zero

        new("0x0.0000010p-126"u8, math.Float32frombits(0x00000000), Below), // rounded down to even

        new("0x0.0000011p-126"u8, math.Float32frombits(0x00000001), Above), // rounded up to smallest denormal

        new("0x0.0000018p-126"u8, math.Float32frombits(0x00000001), Above), // rounded up to smallest denormal

        new("0x1.0000000p-149"u8, math.Float32frombits(0x00000001), Exact), // smallest denormal

        new("0x0.0000020p-126"u8, math.Float32frombits(0x00000001), Exact), // smallest denormal

        new("0x0.fffffe0p-126"u8, math.Float32frombits(0x007fffff), Exact), // largest denormal

        new("0x1.0000000p-126"u8, math.Float32frombits(0x00800000), Exact), // smallest normal

        new("0x0.8p-149"u8, math.Float32frombits(0x000000000), Below), // rounded down to even

        new("0x0.9p-149"u8, math.Float32frombits(0x000000001), Above), // rounded up to smallest denormal

        new("0x0.ap-149"u8, math.Float32frombits(0x000000001), Above), // rounded up to smallest denormal

        new("0x0.bp-149"u8, math.Float32frombits(0x000000001), Above), // rounded up to smallest denormal

        new("0x0.cp-149"u8, math.Float32frombits(0x000000001), Above), // rounded up to smallest denormal

        new("0x1.0p-149"u8, math.Float32frombits(0x000000001), Exact), // smallest denormal

        new("0x1.7p-149"u8, math.Float32frombits(0x000000001), Below),
        new("0x1.8p-149"u8, math.Float32frombits(0x000000002), Above),
        new("0x1.9p-149"u8, math.Float32frombits(0x000000002), Above),
        new("0x2.0p-149"u8, math.Float32frombits(0x000000002), Exact),
        new("0x2.8p-149"u8, math.Float32frombits(0x000000002), Below), // rounded down to even

        new("0x2.9p-149"u8, math.Float32frombits(0x000000003), Above),
        new("0x3.0p-149"u8, math.Float32frombits(0x000000003), Exact),
        new("0x3.7p-149"u8, math.Float32frombits(0x000000003), Below),
        new("0x3.8p-149"u8, math.Float32frombits(0x000000004), Above), // rounded up to even

        new("0x4.0p-149"u8, math.Float32frombits(0x000000004), Exact),
        new("0x4.8p-149"u8, math.Float32frombits(0x000000004), Below), // rounded down to even

        new("0x4.9p-149"u8, math.Float32frombits(0x000000005), Above), // specific case from issue 14553

        new("0x7.7p-149"u8, math.Float32frombits(0x000000007), Below),
        new("0x7.8p-149"u8, math.Float32frombits(0x000000008), Above),
        new("0x7.9p-149"u8, math.Float32frombits(0x000000008), Above), // normals

        new("0x.ffffffp-126"u8, math.Float32frombits(0x00800000), Above), // rounded up to smallest normal

        new("1p-126"u8, math.Float32frombits(0x00800000), Exact), // smallest normal

        new("0x1.fffffep-126"u8, math.Float32frombits(0x00ffffff), Exact),
        new("0x1.ffffffp-126"u8, math.Float32frombits(0x01000000), Above), // rounded up

        new("1"u8, 1F, Exact),
        new("1.000000000000000000001"u8, 1F, Below),
        new("12345.0"u8, 12345F, Exact),
        new("12345.000000000000000000001"u8, 12345F, Below),
        new("0x1.fffffe0p127"u8, math.MaxFloat32, Exact),
        new("0x1.fffffe8p127"u8, math.MaxFloat32, Below), // overflow

        new("0x1.ffffff0p127"u8, (float32)math.Inf(+1), Above),
        new("0x1p128"u8, (float32)math.Inf(+1), Above),
        new("1e10000"u8, (float32)math.Inf(+1), Above),
        new("0x1.ffffff0p2147483646"u8, (float32)math.Inf(+1), Above), // overflow in rounding
 // inf

        new("Inf"u8, (float32)math.Inf(+1), Exact)
    }.slice()) {
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
                var (f, err) = strconv.ParseFloat(tx, 32); if (err == default! && !alike32((float32)f, tout)) {
                    Ꮡt.Errorf("%s: got %g; want %g (incorrect test data)"u8, tx, f, tout);
                }
            }
            var x = makeFloat(tx);
            var (@out, acc) = x.Float32();
            if (!alike32(@out, tout) || acc != tacc) {
                Ꮡt.Errorf("%s: got %g (%#08x, %s); want %g (%#08x, %s)"u8, tx, @out, math.Float32bits(@out), acc, test.@out, math.Float32bits(test.@out), tacc);
            }
            // test that x.SetFloat64(float64(f)).Float32() == f
            ref var x2 = ref heap(new global::go.math.big_package.Float(), out var Ꮡx2);
            var (out2, acc2) = Ꮡx2.SetFloat64((float64)@out).Float32();
            if (!alike32(out2, @out) || acc2 != Exact) {
                Ꮡt.Errorf("idempotency test: got %g (%s); want %g (Exact)"u8, out2, acc2, @out);
            }
        }
    }
}

internal partial struct TestFloatFloat64_type /*dyn*/ {
    internal @string x;
    internal float64 @out;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatFloat64(ж<testing.T> Ꮡt) {
    UntypedFloat smallestNormalFloat64 = 2.2250738585072014e-308; // 1p-1022
    foreach (var (_, test) in new TestFloatFloat64_type[]{
        new("0"u8, 0D, Exact), // underflow to zero

        new("1e-1000"u8, 0D, Below),
        new("0x0.0000000000001p-1023"u8, 0D, Below),
        new("0x0.00000000000008p-1022"u8, 0D, Below), // denormals

        new("0x0.0000000000000cp-1022"u8, math.SmallestNonzeroFloat64, Above), // rounded up to smallest denormal

        new("0x0.00000000000010p-1022"u8, math.SmallestNonzeroFloat64, Exact), // smallest denormal

        new("0x.8p-1073"u8, math.SmallestNonzeroFloat64, Exact),
        new("1p-1074"u8, math.SmallestNonzeroFloat64, Exact),
        new("0x.fffffffffffffp-1022"u8, math.Float64frombits(0x000fffffffffffffUL), Exact), // largest denormal
 // special denormal cases (see issues 14553, 14651)

        new("0x0.00000000000001p-1022"u8, math.Float64frombits(0x00000000000000000), Below), // underflow to zero

        new("0x0.00000000000004p-1022"u8, math.Float64frombits(0x00000000000000000), Below), // underflow to zero

        new("0x0.00000000000008p-1022"u8, math.Float64frombits(0x00000000000000000), Below), // rounded down to even

        new("0x0.00000000000009p-1022"u8, math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal

        new("0x0.0000000000000ap-1022"u8, math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal

        new("0x0.8p-1074"u8, math.Float64frombits(0x00000000000000000), Below), // rounded down to even

        new("0x0.9p-1074"u8, math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal

        new("0x0.ap-1074"u8, math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal

        new("0x0.bp-1074"u8, math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal

        new("0x0.cp-1074"u8, math.Float64frombits(0x00000000000000001), Above), // rounded up to smallest denormal

        new("0x1.0p-1074"u8, math.Float64frombits(0x00000000000000001), Exact),
        new("0x1.7p-1074"u8, math.Float64frombits(0x00000000000000001), Below),
        new("0x1.8p-1074"u8, math.Float64frombits(0x00000000000000002), Above),
        new("0x1.9p-1074"u8, math.Float64frombits(0x00000000000000002), Above),
        new("0x2.0p-1074"u8, math.Float64frombits(0x00000000000000002), Exact),
        new("0x2.8p-1074"u8, math.Float64frombits(0x00000000000000002), Below), // rounded down to even

        new("0x2.9p-1074"u8, math.Float64frombits(0x00000000000000003), Above),
        new("0x3.0p-1074"u8, math.Float64frombits(0x00000000000000003), Exact),
        new("0x3.7p-1074"u8, math.Float64frombits(0x00000000000000003), Below),
        new("0x3.8p-1074"u8, math.Float64frombits(0x00000000000000004), Above), // rounded up to even

        new("0x4.0p-1074"u8, math.Float64frombits(0x00000000000000004), Exact),
        new("0x4.8p-1074"u8, math.Float64frombits(0x00000000000000004), Below), // rounded down to even

        new("0x4.9p-1074"u8, math.Float64frombits(0x00000000000000005), Above), // normals

        new("0x.fffffffffffff8p-1022"u8, math.Float64frombits(0x0010000000000000UL), Above), // rounded up to smallest normal

        new("1p-1022"u8, math.Float64frombits(0x0010000000000000UL), Exact), // smallest normal

        new("1"u8, 1D, Exact),
        new("1.000000000000000000001"u8, 1D, Below),
        new("12345.0"u8, 12345D, Exact),
        new("12345.000000000000000000001"u8, 12345D, Below),
        new("0x1.fffffffffffff0p1023"u8, math.MaxFloat64, Exact),
        new("0x1.fffffffffffff4p1023"u8, math.MaxFloat64, Below), // overflow

        new("0x1.fffffffffffff8p1023"u8, math.Inf(+1), Above),
        new("0x1p1024"u8, math.Inf(+1), Above),
        new("1e10000"u8, math.Inf(+1), Above),
        new("0x1.fffffffffffff8p2147483646"u8, math.Inf(+1), Above), // overflow in rounding

        new("Inf"u8, math.Inf(+1), Exact), // selected denormalized values that were handled incorrectly in the past

        new("0x.fffffffffffffp-1022"u8, smallestNormalFloat64 - math.SmallestNonzeroFloat64, Exact),
        new("4503599627370495p-1074"u8, smallestNormalFloat64 - math.SmallestNonzeroFloat64, Exact), // https://www.exploringbinary.com/php-hangs-on-numeric-value-2-2250738585072011e-308/

        new("2.2250738585072011e-308"u8, 2.225073858507201e-308D, Below), // https://www.exploringbinary.com/java-hangs-when-converting-2-2250738585072012e-308/

        new("2.2250738585072012e-308"u8, 2.2250738585072014e-308D, Above)
    }.slice()) {
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
                var (f, err) = strconv.ParseFloat(tx, 64); if (err == default! && !alike64(f, tout)) {
                    Ꮡt.Errorf("%s: got %g; want %g (incorrect test data)"u8, tx, f, tout);
                }
            }
            var x = makeFloat(tx);
            var (@out, acc) = x.Float64();
            if (!alike64(@out, tout) || acc != tacc) {
                Ꮡt.Errorf("%s: got %g (%#016x, %s); want %g (%#016x, %s)"u8, tx, @out, math.Float64bits(@out), acc, test.@out, math.Float64bits(test.@out), tacc);
            }
            // test that x.SetFloat64(f).Float64() == f
            ref var x2 = ref heap(new global::go.math.big_package.Float(), out var Ꮡx2);
            var (out2, acc2) = Ꮡx2.SetFloat64(@out).Float64();
            if (!alike64(out2, @out) || acc2 != Exact) {
                Ꮡt.Errorf("idempotency test: got %g (%s); want %g (Exact)"u8, out2, acc2, @out);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string nilˢ2 = "nil"u8;

internal partial struct TestFloatInt_type /*dyn*/ {
    internal @string x;
    internal @string want;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatInt(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatInt_type[]{
        new("0"u8, "0"u8, Exact),
        new("+0"u8, "0"u8, Exact),
        new("-0"u8, "0"u8, Exact),
        new("Inf"u8, "nil"u8, Below),
        new("+Inf"u8, "nil"u8, Below),
        new("-Inf"u8, "nil"u8, Above),
        new("1"u8, "1"u8, Exact),
        new("-1"u8, "-1"u8, Exact),
        new("1.23"u8, "1"u8, Below),
        new("-1.23"u8, "-1"u8, Above),
        new("123e-2"u8, "1"u8, Below),
        new("123e-3"u8, "0"u8, Below),
        new("123e-4"u8, "0"u8, Below),
        new("1e-1000"u8, "0"u8, Below),
        new("-1e-1000"u8, "0"u8, Above),
        new("1e+10"u8, "10000000000"u8, Exact),
        new("1e+100"u8, "10000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"u8, Exact)
    }.slice()) {
        var x = makeFloat(test.x);
        var (res, acc) = x.Int(nil);
        @string got = nilˢ2;
        if (res != nil) {
            got = res.String();
        }
        if (got != test.want || acc != test.acc) {
            Ꮡt.Errorf("%s: got %s (%s); want %s (%s)"u8, test.x, got, acc, test.want, test.acc);
        }
    }
    // check that supplied *Int is used
    foreach (var (_, f) in new @string[]{"0"u8, "1"u8, "-1"u8, "1234"u8}.slice()) {
        var x = makeFloat(f);
        var i = @new<global::go.math.big_package.ΔInt>();
        {
            var (res, _) = x.Int(i); if (res != i) {
                Ꮡt.Errorf("(%s).Int is not using supplied *Int"u8, f);
            }
        }
    }
}

internal partial struct TestFloatRat_type /*dyn*/ {
    internal @string x, want;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloatRat(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatRat_type[]{
        new("0"u8, "0/1"u8, Exact),
        new("+0"u8, "0/1"u8, Exact),
        new("-0"u8, "0/1"u8, Exact),
        new("Inf"u8, "nil"u8, Below),
        new("+Inf"u8, "nil"u8, Below),
        new("-Inf"u8, "nil"u8, Above),
        new("1"u8, "1/1"u8, Exact),
        new("-1"u8, "-1/1"u8, Exact),
        new("1.25"u8, "5/4"u8, Exact),
        new("-1.25"u8, "-5/4"u8, Exact),
        new("1e10"u8, "10000000000/1"u8, Exact),
        new("1p10"u8, "1024/1"u8, Exact),
        new("-1p-10"u8, "-1/1024"u8, Exact),
        new("3.14159265"u8, "7244019449799623199/2305843009213693952"u8, Exact)
    }.slice()) {
        var x = makeFloat(test.x).SetPrec(64);
        var (res, acc) = x.Rat(nil);
        @string got = nilˢ2;
        if (res != nil) {
            got = res.String();
        }
        if (got != test.want) {
            Ꮡt.Errorf("%s: got %s; want %s"u8, test.x, got, test.want);
            continue;
        }
        if (acc != test.acc) {
            Ꮡt.Errorf("%s: got %s; want %s"u8, test.x, acc, test.acc);
            continue;
        }
        // inverse conversion
        if (res != nil) {
            var gotΔ1 = @new<global::go.math.big_package.Float>().SetPrec(64).SetRat(res);
            if (gotΔ1.Cmp(x) != 0) {
                Ꮡt.Errorf("%s: got %s; want %s"u8, test.x, gotΔ1.OrTypedNil(), x.OrTypedNil());
            }
        }
    }
    // check that supplied *Rat is used
    foreach (var (_, f) in new @string[]{"0"u8, "1"u8, "-1"u8, "1234"u8}.slice()) {
        var x = makeFloat(f);
        var r = @new<global::go.math.big_package.ΔRat>();
        {
            var (res, _) = x.Rat(r); if (res != r) {
                Ꮡt.Errorf("(%s).Rat is not using supplied *Rat"u8, f);
            }
        }
    }
}

public static void TestFloatAbs(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new @string[]{
        "0"u8,
        "1"u8,
        "1234"u8,
        "1.23e-2"u8,
        "1e-1000"u8,
        "1e1000"u8,
        "Inf"u8
    }.slice()) {
        var p = makeFloat(test);
        var a = @new<global::go.math.big_package.Float>().Abs(p);
        if (!alike(a, p)) {
            Ꮡt.Errorf("%s: got %s; want %s"u8, test, a.Text((rune)'g', 10), test);
        }
        var n = makeFloat("-"u8 + test);
        a.Abs(n);
        if (!alike(a, p)) {
            Ꮡt.Errorf("-%s: got %s; want %s"u8, test, a.Text((rune)'g', 10), test);
        }
    }
}

public static void TestFloatNeg(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new @string[]{
        "0"u8,
        "1"u8,
        "1234"u8,
        "1.23e-2"u8,
        "1e-1000"u8,
        "1e1000"u8,
        "Inf"u8
    }.slice()) {
        var p1 = makeFloat(test);
        var n1 = makeFloat("-"u8 + test);
        var n2 = @new<global::go.math.big_package.Float>().Neg(p1);
        var p2 = @new<global::go.math.big_package.Float>().Neg(n2);
        if (!alike(n2, n1)) {
            Ꮡt.Errorf("%s: got %s; want %s"u8, test, n2.Text((rune)'g', 10), n1.Text((rune)'g', 10));
        }
        if (!alike(p2, p1)) {
            Ꮡt.Errorf("%s: got %s; want %s"u8, test, p2.Text((rune)'g', 10), p1.Text((rune)'g', 10));
        }
    }
}

public static void TestFloatInc(ж<testing.T> Ꮡt) {
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
            Ꮡt.Errorf("prec = %d: got %s; want %d"u8, prec, Ꮡx, (nint)(n));
        }
    }
}

// Selected precisions with which to run various tests.
internal static array<nuint> precList = new nuint[]{1, 2, 5, 8, 10, 16, 23, 24, 32, 50, 53, 64, 100, 128, 500, 511, 512, 513, 1000, 10000}.array();

// = 0
// = 1
// = 2
// = 1/2
// = 2**10 == 1024
// = 2**-10 == 1/1024
// = 2**100 + 2**10 + 2**1
// TODO(gri) add more test cases
// Selected bits with which to run various tests.
// Each entry is a list of bits representing a floating-point number (see fromBits).
internal static array<ΔBits> bitsList = new ΔBits[]{
    new nint[]{}.slice(),
    new nint[]{0}.slice(),
    new nint[]{1}.slice(),
    new nint[]{-1}.slice(),
    new nint[]{10}.slice(),
    new nint[]{-10}.slice(),
    new nint[]{100, 10, 1}.slice(),
    new nint[]{0, -1, -2, -10}.slice()
}.array();

// TestFloatAdd tests Float.Add/Sub by comparing the result of a "manual"
// addition/subtraction of arguments represented by Bits values with the
// respective Float addition/subtraction for a variety of precisions
// and rounding modes.
public static void TestFloatAdd(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

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
                        Ꮡt.Errorf("i = %d, prec = %d, %s:\n\t     %s %v\n\t+    %s %v\n\t=    %s\n\twant %s"u8,
                            i, prec, mode, x.OrTypedNil(), xbits, y.OrTypedNil(), ybits, got.OrTypedNil(), want.OrTypedNil());
                    }
                    got.Sub(z, x);
                    want = ybits.round(prec, mode);
                    if (got.Cmp(want) != 0) {
                        Ꮡt.Errorf("i = %d, prec = %d, %s:\n\t     %s %v\n\t-    %s %v\n\t=    %s\n\twant %s"u8,
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
public static void TestFloatAddRoundZero(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, mode) in new global::go.math.big_package.RoundingMode[]{ToNearestEven, ToNearestAway, ToZero, AwayFromZero, ToPositiveInf, ToNegativeInf}.array()) {
        var x = NewFloat(5.0D);
        var y = @new<global::go.math.big_package.Float>().Neg(x);
        var want = NewFloat(0.0D);
        if (mode == ToNegativeInf) {
            want.Neg(want);
        }
        var got = @new<global::go.math.big_package.Float>().SetMode(mode);
        got.Add(x, y);
        if (got.Cmp(want) != 0 || (~got).neg != (mode == ToNegativeInf)) {
            Ꮡt.Errorf("%s:\n\t     %v\n\t+    %v\n\t=    %v\n\twant %v"u8,
                mode, x.OrTypedNil(), y.OrTypedNil(), got.OrTypedNil(), want.OrTypedNil());
        }
        got.Sub(x, x);
        if (got.Cmp(want) != 0 || (~got).neg != (mode == ToNegativeInf)) {
            Ꮡt.Errorf("%v:\n\t     %v\n\t-    %v\n\t=    %v\n\twant %v"u8,
                mode, x.OrTypedNil(), x.OrTypedNil(), got.OrTypedNil(), want.OrTypedNil());
        }
    }
}

// TestFloatAdd32 tests that Float.Add/Sub of numbers with
// 24bit mantissa behaves like float32 addition/subtraction
// (excluding denormal numbers).
public static void TestFloatAdd32(ж<testing.T> Ꮡt) {
    // chose base such that we cross the mantissa precision limit
    UntypedInt @base = /* 1<<26 - 0x10 */ 67108848; // 11...110000 (26 bits)
    for (nint d = 0; d <= 0x10; d++) {
        foreach (var (i, _) in new nint[]{}.array(2)) {
            var (x0, y0) = ((float64)@base, (float64)d);
            if ((nint)(i & 1) != 0) {
                (x0, y0) = (y0, x0);
            }
            var x = NewFloat(x0);
            var y = NewFloat(y0);
            var z = @new<global::go.math.big_package.Float>().SetPrec(24);
            z.Add(x, y);
            var (got, acc) = z.Float32();
            var want = (float32)y0 + (float32)x0;
            if (got != want || acc != Exact) {
                Ꮡt.Errorf("d = %d: %g + %g = %g (%s); want %g (Exact)"u8, d, x0, y0, got, acc, want);
            }
            z.Sub(z, y);
            (got, acc) = z.Float32();
            want = (float32)want - (float32)y0;
            if (got != want || acc != Exact) {
                Ꮡt.Errorf("d = %d: %g - %g = %g (%s); want %g (Exact)"u8, d, x0 + y0, y0, got, acc, want);
            }
        }
    }
}

// TestFloatAdd64 tests that Float.Add/Sub of numbers with
// 53bit mantissa behaves like float64 addition/subtraction.
public static void TestFloatAdd64(ж<testing.T> Ꮡt) {
    // chose base such that we cross the mantissa precision limit
    UntypedInt @base = /* 1<<55 - 0x10 */ 36028797018963952; // 11...110000 (55 bits)
    for (nint d = 0; d <= 0x10; d++) {
        foreach (var (i, _) in new nint[]{}.array(2)) {
            var (x0, y0) = ((float64)@base, (float64)d);
            if ((nint)(i & 1) != 0) {
                (x0, y0) = (y0, x0);
            }
            var x = NewFloat(x0);
            var y = NewFloat(y0);
            var z = @new<global::go.math.big_package.Float>().SetPrec(53);
            z.Add(x, y);
            var (got, acc) = z.Float64();
            var want = x0 + y0;
            if (got != want || acc != Exact) {
                Ꮡt.Errorf("d = %d: %g + %g = %g (%s); want %g (Exact)"u8, d, x0, y0, got, acc, want);
            }
            z.Sub(z, y);
            (got, acc) = z.Float64();
            want -= y0;
            if (got != want || acc != Exact) {
                Ꮡt.Errorf("d = %d: %g - %g = %g (%s); want %g (Exact)"u8, d, x0 + y0, y0, got, acc, want);
            }
        }
    }
}

internal partial struct TestIssue20490_type /*dyn*/ {
    internal float64 a, b;
}

public static void TestIssue20490(ж<testing.T> Ꮡt) {
    slice<TestIssue20490_type> tests = new TestIssue20490_type[]{
        new(4D, 1D),
        new(-4D, 1D),
        new(4D, -1D),
        new(-4D, -1D)
    }.slice();
    foreach (var (_, test) in tests) {
        var (a, b) = (NewFloat(test.a), NewFloat(test.b));
        var diff = @new<global::go.math.big_package.Float>().Sub(a, b);
        b.Sub(a, b);
        if (b.Cmp(diff) != 0) {
            Ꮡt.Errorf("got %g - %g = %g; want %g\n"u8, a.OrTypedNil(), NewFloat(test.b).OrTypedNil(), b.OrTypedNil(), diff.OrTypedNil());
        }
        b = NewFloat(test.b);
        var sum = @new<global::go.math.big_package.Float>().Add(a, b);
        b.Add(a, b);
        if (b.Cmp(sum) != 0) {
            Ꮡt.Errorf("got %g + %g = %g; want %g\n"u8, a.OrTypedNil(), NewFloat(test.b).OrTypedNil(), b.OrTypedNil(), sum.OrTypedNil());
        }
    }
}

// TestFloatMul tests Float.Mul/Quo by comparing the result of a "manual"
// multiplication/division of arguments represented by Bits values with the
// respective Float multiplication/division for a variety of precisions
// and rounding modes.
public static void TestFloatMul(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

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
                        Ꮡt.Errorf("i = %d, prec = %d, %s:\n\t     %v %v\n\t*    %v %v\n\t=    %v\n\twant %v"u8,
                            i, prec, mode, x.OrTypedNil(), xbits, y.OrTypedNil(), ybits, got.OrTypedNil(), want.OrTypedNil());
                    }
                    if (x.Sign() == 0) {
                        continue; // ignore div-0 case (not invertable)
                    }
                    got.Quo(z, x);
                    want = ybits.round(prec, mode);
                    if (got.Cmp(want) != 0) {
                        Ꮡt.Errorf("i = %d, prec = %d, %s:\n\t     %v %v\n\t/    %v %v\n\t=    %v\n\twant %v"u8,
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
public static void TestFloatMul64(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatMul64_type[]{
        new(0D, 0D),
        new(0D, 1D),
        new(1D, 1D),
        new(1D, 1.5D),
        new(1.234D, 0.5678D),
        new(2.718281828D, 3.14159265358979D),
        new(2.718281828e10D, 3.14159265358979e-32D),
        new(1.0D / 3D, 1e200D)
    }.slice()) {
        foreach (var (i, _) in new nint[]{}.array(8)) {
            var (x0, y0) = (test.x, test.y);
            if ((nint)(i & 1) != 0) {
                x0 = -x0;
            }
            if ((nint)(i & 2) != 0) {
                y0 = -y0;
            }
            if ((nint)(i & 4) != 0) {
                (x0, y0) = (y0, x0);
            }
            var x = NewFloat(x0);
            var y = NewFloat(y0);
            var z = @new<global::go.math.big_package.Float>().SetPrec(53);
            z.Mul(x, y);
            var (got, _) = z.Float64();
            var want = x0 * y0;
            if (got != want) {
                Ꮡt.Errorf("%g * %g = %g; want %g"u8, x0, y0, got, want);
            }
            if (y0 == 0D) {
                continue; // avoid division-by-zero
            }
            z.Quo(z, y);
            (got, _) = z.Float64();
            want /= y0;
            if (got != want) {
                Ꮡt.Errorf("%g / %g = %g; want %g"u8, x0 * y0, y0, got, want);
            }
        }
    }
}

public static void TestIssue6866(ж<testing.T> Ꮡt) {
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
            Ꮡt.Fatalf("prec %d: got z1 = %v != z2 = %v; want z1 == z2\n"u8, prec, z1.OrTypedNil(), z2.OrTypedNil());
        }
        if (z1.Sign() != 0) {
            Ꮡt.Errorf("prec %d: got z1 = %v; want 0"u8, prec, z1.OrTypedNil());
        }
        if (z2.Sign() != 0) {
            Ꮡt.Errorf("prec %d: got z2 = %v; want 0"u8, prec, z2.OrTypedNil());
        }
    }
}

public static void TestFloatQuo(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    // TODO(gri) make the test vary these precisions
    nint preci = 200; // precision of integer part
    nint precf = 20; // precision of fractional part
    for (nint i = 0; i < 8; i++) {
        // compute accurate (not rounded) result z
        var bits = new ΔBits(new nint[]{preci - 1}.slice());
        if ((nint)(i & 3) != 0) {
            bits = append(bits, (nint)(0));
        }
        if ((nint)(i & 2) != 0) {
            bits = append(bits, (nint)(-1));
        }
        if ((nint)(i & 1) != 0) {
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
                Ꮡt.Errorf("got acc = %s; want exact"u8, got);
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
                    Ꮡt.Errorf("i = %d, prec = %d, %s:\n\t     %s\n\t/    %s\n\t=    %s\n\twant %s"u8,
                        i, prec, mode, x.OrTypedNil(), y.OrTypedNil(), got.OrTypedNil(), want.OrTypedNil());
                }
            }
        }
    }
}

internal static ж<bool> @long = flag.Bool("long"u8, false, "run very long tests"u8);

// TestFloatQuoSmoke tests all divisions x/y for values x, y in the range [-n, +n];
// it serves as a smoke test for basic correctness of division.
public static void TestFloatQuoSmoke(ж<testing.T> Ꮡt) {
    nint n = 10;
    if (@long.Value) {
        n = 1000;
    }
    UntypedInt dprec = 3; // max. precision variation
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
                        Ꮡt.Errorf("%g/%g = %s; want %.5g\n"u8, a, b, C.Text((rune)'g', 5), c);
                        continue;
                    }
                    if (acc != Exact) {
                        Ꮡt.Errorf("%g/%g got %s result; want exact result"u8, a, b, acc);
                    }
                }
            }
        }
    }
}

// TestFloatArithmeticSpecialValues tests that Float operations produce the
// correct results for combinations of zero (±0), finite (±1 and ±2.71828),
// and infinite (±Inf) operands.
public static void TestFloatArithmeticSpecialValues(ж<testing.T> Ꮡt) {
    var zero = 0.0D;
    var args = new float64[]{math.Inf(-1), -2.71828D, -1D, -zero, zero, 1D, 2.71828D, math.Inf(1)}.slice();
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
                    Ꮡt.Errorf("Float(%g) == %g (%s)"u8, x, gotΔ1, acc);
                }
            }
            foreach (var (_, y) in args) {
                yy.SetFloat64(y);
                @string op = default!;
                float64 z = default!;
                ref var f = ref heap<Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>>(out var Ꮡf);
                switch (i) {
                case 0: {
                    op = "+"u8;
                    z = x + y;
                    f = ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Add));
                    break;
                }
                case 1: {
                    op = "-"u8;
                    z = x - y;
                    f = ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Sub));
                    break;
                }
                case 2: {
                    op = "*"u8;
                    z = x * y;
                    f = ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Mul));
                    break;
                }
                case 3: {
                    op = "/"u8;
                    z = x / y;
                    f = ((Func<ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>, ж<global::go.math.big_package.Float>>)(global::go.math.big_package.Quo));
                    break;
                }
                default: {
                    throw panic("unreachable");
                    break;
                }}

                bool errnan = default!;    // set if execution of f panicked with ErrNaN
                // protect execution of f
                var gotʗ1 = got;
                var xxʗ1 = xx;
                var yyʗ1 = yy;
                ((Action)(() => {
                    GoFrame ᒐ = default;
                    try {
                        defer(() => {
                            {
                                var p = recover(); if (p != default!) {
                                    _ = p._<ErrNaN>(); // re-panic if not ErrNaN
                                    errnan = true;
                                }
                            }
                        }, ref ᒐ);
                        Ꮡf.ValueSlot(gotʗ1, xxʗ1, yyʗ1);
                    }
                    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
                    finally { ᒐ.Run(); }
                }))();
                if (math.IsNaN(z)) {
                    if (!errnan) {
                        Ꮡt.Errorf("%5g %s %5g = %5s; want ErrNaN panic"u8, x, op, y, got.OrTypedNil());
                    }
                    continue;
                }
                if (errnan) {
                    Ꮡt.Errorf("%5g %s %5g panicked with ErrNan; want %5s"u8, x, op, y, want.OrTypedNil());
                    continue;
                }
                want.SetFloat64(z);
                if (!alike(got, want)) {
                    Ꮡt.Errorf("%5g %s %5g = %5s; want %5s"u8, x, op, y, got.OrTypedNil(), want.OrTypedNil());
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

public static void TestFloatArithmeticOverflow(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatArithmeticOverflow_type[]{
        new(4, ToNearestEven, (rune)'+', "0"u8, "0"u8, "0"u8, Exact), // smoke test

        new(4, ToNearestEven, (rune)'+', "0x.8p+0"u8, "0x.8p+0"u8, "0x.8p+1"u8, Exact), // smoke test

        new(4, ToNearestEven, (rune)'+', "0"u8, "0x.8p2147483647"u8, "0x.8p+2147483647"u8, Exact),
        new(4, ToNearestEven, (rune)'+', "0x.8p2147483500"u8, "0x.8p2147483647"u8, "0x.8p+2147483647"u8, Below), // rounded to zero

        new(4, ToNearestEven, (rune)'+', "0x.8p2147483647"u8, "0x.8p2147483647"u8, "+Inf"u8, Above), // exponent overflow in +

        new(4, ToNearestEven, (rune)'+', "-0x.8p2147483647"u8, "-0x.8p2147483647"u8, "-Inf"u8, Below), // exponent overflow in +

        new(4, ToNearestEven, (rune)'-', "-0x.8p2147483647"u8, "0x.8p2147483647"u8, "-Inf"u8, Below), // exponent overflow in -

        new(4, ToZero, (rune)'+', "0x.fp2147483647"u8, "0x.8p2147483643"u8, "0x.fp+2147483647"u8, Below), // rounded to zero

        new(4, ToNearestEven, (rune)'+', "0x.fp2147483647"u8, "0x.8p2147483643"u8, "+Inf"u8, Above), // exponent overflow in rounding

        new(4, AwayFromZero, (rune)'+', "0x.fp2147483647"u8, "0x.8p2147483643"u8, "+Inf"u8, Above), // exponent overflow in rounding

        new(4, AwayFromZero, (rune)'-', "-0x.fp2147483647"u8, "0x.8p2147483644"u8, "-Inf"u8, Below), // exponent overflow in rounding

        new(4, ToNearestEven, (rune)'-', "-0x.fp2147483647"u8, "0x.8p2147483643"u8, "-Inf"u8, Below), // exponent overflow in rounding

        new(4, ToZero, (rune)'-', "-0x.fp2147483647"u8, "0x.8p2147483643"u8, "-0x.fp+2147483647"u8, Above), // rounded to zero

        new(4, ToNearestEven, (rune)'+', "0"u8, "0x.8p-2147483648"u8, "0x.8p-2147483648"u8, Exact),
        new(4, ToNearestEven, (rune)'+', "0x.8p-2147483648"u8, "0x.8p-2147483648"u8, "0x.8p-2147483647"u8, Exact),
        new(4, ToNearestEven, (rune)'*', "1"u8, "0x.8p2147483647"u8, "0x.8p+2147483647"u8, Exact),
        new(4, ToNearestEven, (rune)'*', "2"u8, "0x.8p2147483647"u8, "+Inf"u8, Above), // exponent overflow in *

        new(4, ToNearestEven, (rune)'*', "-2"u8, "0x.8p2147483647"u8, "-Inf"u8, Below), // exponent overflow in *

        new(4, ToNearestEven, (rune)'/', "0.5"u8, "0x.8p2147483647"u8, "0x.8p-2147483646"u8, Exact),
        new(4, ToNearestEven, (rune)'/', "0x.8p+0"u8, "0x.8p2147483647"u8, "0x.8p-2147483646"u8, Exact),
        new(4, ToNearestEven, (rune)'/', "0x.8p-1"u8, "0x.8p2147483647"u8, "0x.8p-2147483647"u8, Exact),
        new(4, ToNearestEven, (rune)'/', "0x.8p-2"u8, "0x.8p2147483647"u8, "0x.8p-2147483648"u8, Exact),
        new(4, ToNearestEven, (rune)'/', "0x.8p-3"u8, "0x.8p2147483647"u8, "0"u8, Below)
    }.slice()) {
        // exponent underflow in /
        var x = makeFloat(test.x);
        var y = makeFloat(test.y);
        var z = @new<global::go.math.big_package.Float>().SetPrec(test.prec).SetMode(test.mode);
        switch (test.op) {
        case (rune)'+': {
            z.Add(x, y);
            break;
        }
        case (rune)'-': {
            z.Sub(x, y);
            break;
        }
        case (rune)'*': {
            z.Mul(x, y);
            break;
        }
        case (rune)'/': {
            z.Quo(x, y);
            break;
        }
        default: {
            throw panic("unreachable");
            break;
        }}

        {
            @string got = z.Text((rune)'p', 0); if (got != test.want || z.Acc() != test.acc) {
                Ꮡt.Errorf(
                    "prec = %d (%s): %s %c %s = %s (%s); want %s (%s)"u8,
                    test.prec, test.mode, x.Text((rune)'p', 0), test.op, y.Text((rune)'p', 0), got, z.Acc(), test.want, test.acc);
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
public static void TestFloatArithmeticRounding(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloatArithmeticRounding_type[]{
        new(ToZero, 3, -0x8, -0x1, -0x8, (rune)'+'),
        new(AwayFromZero, 3, -0x8, -0x1, -0xa, (rune)'+'),
        new(ToNegativeInf, 3, -0x8, -0x1, -0xa, (rune)'+'),
        new(ToZero, 3, -0x8, 0x1, -0x8, (rune)'-'),
        new(AwayFromZero, 3, -0x8, 0x1, -0xa, (rune)'-'),
        new(ToNegativeInf, 3, -0x8, 0x1, -0xa, (rune)'-'),
        new(ToZero, 3, -0x9, 0x1, -0x8, (rune)'*'),
        new(AwayFromZero, 3, -0x9, 0x1, -0xa, (rune)'*'),
        new(ToNegativeInf, 3, -0x9, 0x1, -0xa, (rune)'*'),
        new(ToZero, 3, -0x9, 0x1, -0x8, (rune)'/'),
        new(AwayFromZero, 3, -0x9, 0x1, -0xa, (rune)'/'),
        new(ToNegativeInf, 3, -0x9, 0x1, -0xa, (rune)'/')
    }.slice()) {
        ref var x = ref heap(new global::go.math.big_package.Float(), out var Ꮡx);
        ref var y = ref heap(new global::go.math.big_package.Float(), out var Ꮡy);
        ref var z = ref heap(new global::go.math.big_package.Float(), out var Ꮡz);
        Ꮡx.SetInt64(test.x);
        Ꮡy.SetInt64(test.y);
        Ꮡz.SetPrec(test.prec).SetMode(test.mode);
        switch (test.op) {
        case (rune)'+': {
            Ꮡz.Add(Ꮡx, Ꮡy);
            break;
        }
        case (rune)'-': {
            Ꮡz.Sub(Ꮡx, Ꮡy);
            break;
        }
        case (rune)'*': {
            Ꮡz.Mul(Ꮡx, Ꮡy);
            break;
        }
        case (rune)'/': {
            Ꮡz.Quo(Ꮡx, Ꮡy);
            break;
        }
        default: {
            throw panic("unreachable");
            break;
        }}

        {
            var (got, acc) = Ꮡz.Int64(); if (got != test.want || acc != Exact) {
                Ꮡt.Errorf("%s, %d bits: %d %c %d = %d (%s); want %d (Exact)"u8,
                    test.mode, test.prec, test.x, test.op, test.y, got, acc, test.want);
            }
        }
    }
}

// TestFloatCmpSpecialValues tests that Cmp produces the correct results for
// combinations of zero (±0), finite (±1 and ±2.71828), and infinite (±Inf)
// operands.
public static void TestFloatCmpSpecialValues(ж<testing.T> Ꮡt) {
    var zero = 0.0D;
    var args = new float64[]{math.Inf(-1), -2.71828D, -1D, -zero, zero, 1D, 2.71828D, math.Inf(1)}.slice();
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
                    Ꮡt.Errorf("Float(%g) == %g (%s)"u8, x, got, acc);
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
                    Ꮡt.Errorf("(%g).Cmp(%g) = %v; want %v"u8, x, y, got, want);
                }
            }
        }
    }
}

public static void BenchmarkFloatAdd(ж<testing.B> Ꮡb) {
    var x = @new<global::go.math.big_package.Float>();
    var y = @new<global::go.math.big_package.Float>();
    var z = @new<global::go.math.big_package.Float>();
    foreach (var (_, prec) in new nuint[]{10, 100, 1000, 10000, 100000}.slice()) {
        x.SetPrec(prec).SetRat(NewRat(1, 3));
        y.SetPrec(prec).SetRat(NewRat(1, 6));
        z.SetPrec(prec);
        var xʗ1 = x;
        var yʗ1 = y;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprintf("%v"u8, prec), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                zʗ1.Add(xʗ1, yʗ1);
            }
        });
    }
}

public static void BenchmarkFloatSub(ж<testing.B> Ꮡb) {
    var x = @new<global::go.math.big_package.Float>();
    var y = @new<global::go.math.big_package.Float>();
    var z = @new<global::go.math.big_package.Float>();
    foreach (var (_, prec) in new nuint[]{10, 100, 1000, 10000, 100000}.slice()) {
        x.SetPrec(prec).SetRat(NewRat(1, 3));
        y.SetPrec(prec).SetRat(NewRat(1, 6));
        z.SetPrec(prec);
        var xʗ1 = x;
        var yʗ1 = y;
        var zʗ1 = z;
        Ꮡb.Run(fmt.Sprintf("%v"u8, prec), (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                zʗ1.Sub(xʗ1, yʗ1);
            }
        });
    }
}

} // end big_internal_test_package
