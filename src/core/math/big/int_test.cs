// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using bytes = bytes_package;
using hex = encoding.hex_package;
using fmt = fmt_package;
using testenv = @internal.testenv_package;
using math = math_package;
using Δrand = go.math.rand_package;
using strconv = strconv_package;
using strings = strings_package;
using testing = testing_package;
using quick = go.testing.quick_package;
using @internal;
using encoding;
using go.math;
using go.testing;
using static go.math.big_package;

partial class big_internal_test_package {

internal static bool isNormalized(ж<global::go.math.big_package.ΔInt> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    if (len(x.abs) == 0) {
        return !x.neg;
    }
    // len(x.abs) > 0
    return x.abs[len(x.abs) - 1] != 0;
}

// type funZZ is a methodless func type — rendered inline as its base delegate

[GoType] internal partial struct argZZ {
    internal ж<global::go.math.big_package.ΔInt> z, x, y;
}

internal static slice<argZZ> sumZZ = new argZZ[]{
    new(NewInt(0), NewInt(0), NewInt(0)),
    new(NewInt(1), NewInt(1), NewInt(0)),
    new(NewInt(1111111110), NewInt(123456789), NewInt(987654321)),
    new(NewInt(-1), NewInt(-1), NewInt(0)),
    new(NewInt(864197532), NewInt(-123456789), NewInt(987654321)),
    new(NewInt(-1111111110), NewInt(-123456789), NewInt(-987654321))
}.slice();

// TODO(gri) add larger products
internal static slice<argZZ> prodZZ = new argZZ[]{
    new(NewInt(0), NewInt(0), NewInt(0)),
    new(NewInt(0), NewInt(1), NewInt(0)),
    new(NewInt(1), NewInt(1), NewInt(1)),
    new(NewInt(-991 * 991), NewInt(991), NewInt(-991))
}.slice();

public static void TestSignZ(ж<testing.T> Ꮡt) {
    ref var zero = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡzero);
    foreach (var (_, a) in sumZZ) {
        nint s = a.z.Sign();
        nint e = a.z.Cmp(Ꮡzero);
        if (s != e) {
            Ꮡt.Errorf("got %d; want %d for z = %v"u8, s, e, a.z.OrTypedNil());
        }
    }
}

public static void TestSetZ(ж<testing.T> Ꮡt) {
    foreach (var (_, a) in sumZZ) {
        ref var z = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡz);
        Ꮡz.Set(a.z);
        if (!isNormalized(Ꮡz)) {
            Ꮡt.Errorf("%v is not normalized"u8, z);
        }
        if ((Ꮡz).Cmp(a.z) != 0) {
            Ꮡt.Errorf("got z = %v; want %v"u8, z, a.z.OrTypedNil());
        }
    }
}

public static void TestAbsZ(ж<testing.T> Ꮡt) {
    ref var zero = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡzero);
    foreach (var (_, a) in sumZZ) {
        ref var z = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡz);
        Ꮡz.Abs(a.z);
        ref var e = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡe);
        Ꮡe.Set(a.z);
        if (Ꮡe.Cmp(Ꮡzero) < 0) {
            Ꮡe.Sub(Ꮡzero, Ꮡe);
        }
        if (Ꮡz.Cmp(Ꮡe) != 0) {
            Ꮡt.Errorf("got z = %v; want %v"u8, z, e);
        }
    }
}

internal static void testFunZZ(ж<testing.T> Ꮡt, @string msg, Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>> f, argZZ a) {
    ref var z = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡz);
    f(Ꮡz, a.x, a.y);
    if (!isNormalized(Ꮡz)) {
        Ꮡt.Errorf("%s%v is not normalized"u8, msg, z);
    }
    if ((Ꮡz).Cmp(a.z) != 0) {
        Ꮡt.Errorf("%v %s %v\n\tgot z = %v; want %v"u8, a.x.OrTypedNil(), msg, a.y.OrTypedNil(), Ꮡz, a.z.OrTypedNil());
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string addZZˢ = "AddZZ"u8;
internal static readonly @string addZZSymmetricˢ = "AddZZ symmetric"u8;
internal static readonly @string subZZˢ = "SubZZ"u8;
internal static readonly @string subZZSymmetricˢ = "SubZZ symmetric"u8;

public static void TestSumZZ(ж<testing.T> Ꮡt) {
    var AddZZ = (ж<global::go.math.big_package.ΔInt> z, ж<global::go.math.big_package.ΔInt> x, ж<global::go.math.big_package.ΔInt> y) => z.Add(x, y);
    var SubZZ = (ж<global::go.math.big_package.ΔInt> z, ж<global::go.math.big_package.ΔInt> x, ж<global::go.math.big_package.ΔInt> y) => z.Sub(x, y);
    foreach (var (_, a) in sumZZ) {
        var arg = a;
        testFunZZ(Ꮡt, addZZˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(AddZZ), arg);
        arg = new argZZ(a.z, a.y, a.x);
        testFunZZ(Ꮡt, addZZSymmetricˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(AddZZ), arg);
        arg = new argZZ(a.x, a.z, a.y);
        testFunZZ(Ꮡt, subZZˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(SubZZ), arg);
        arg = new argZZ(a.y, a.z, a.x);
        testFunZZ(Ꮡt, subZZSymmetricˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(SubZZ), arg);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string mulZZˢ = "MulZZ"u8;
internal static readonly @string mulZZSymmetricˢ = "MulZZ symmetric"u8;

public static void TestProdZZ(ж<testing.T> Ꮡt) {
    var MulZZ = (ж<global::go.math.big_package.ΔInt> z, ж<global::go.math.big_package.ΔInt> x, ж<global::go.math.big_package.ΔInt> y) => z.Mul(x, y);
    foreach (var (_, a) in prodZZ) {
        var arg = a;
        testFunZZ(Ꮡt, mulZZˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(MulZZ), arg);
        arg = new argZZ(a.z, a.y, a.x);
        testFunZZ(Ꮡt, mulZZSymmetricˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(MulZZ), arg);
    }
}

// mulBytes returns x*y via grade school multiplication. Both inputs
// and the result are assumed to be in big-endian representation (to
// match the semantics of Int.Bytes and Int.SetBytes).
internal static slice<byte> mulBytes(slice<byte> x, slice<byte> y) {
    var z = new slice<byte>(len(x) + len(y));
    // multiply
    nint k0 = len(z) - 1;
    for (nint j = len(y) - 1; j >= 0; j--) {
        nint d = (nint)y[j];
        if (d != 0) {
            nint k = k0;
            nint carry = 0;
            for (nint iΔ1 = len(x) - 1; iΔ1 >= 0; iΔ1--) {
                nint t = (nint)z[k] + (nint)x[iΔ1] * d + carry;
                (z[k], carry) = ((byte)t, (t >> (int)(8)));
                k--;
            }
            z[k] = (byte)carry;
        }
        k0--;
    }
    // normalize (remove leading 0's)
    nint i = 0;
    while (i < len(z) && z[i] == 0) {
        i++;
    }
    return z.slice(i);
}

internal static bool checkMul(slice<byte> a, slice<byte> b) {
    ref var x = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡx);
    ref var y = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡy);
    ref var z1 = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡz1);
    Ꮡx.SetBytes(a);
    Ꮡy.SetBytes(b);
    Ꮡz1.Mul(Ꮡx, Ꮡy);
    ref var z2 = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡz2);
    Ꮡz2.SetBytes(mulBytes(a, b));
    return Ꮡz1.Cmp(Ꮡz2) == 0;
}

public static void TestMul(ж<testing.T> Ꮡt) {
    {
        var err = quick.Check(checkMul, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

// entirely positive ranges are covered by mulRangesN
// empty range
// empty range
// range includes 0
// range includes 0
// range includes 0
// 10!
// -20!
// -99!
// overflow situations

[GoType("dyn")] partial struct mulRangesZᴛ1 {
    internal int64 a, b;
    internal @string prod;
}
internal static slice<mulRangesZᴛ1> mulRangesZ = new mulRangesZᴛ1[]{
    new(-1, 1, "0"u8),
    new(-2, -1, "2"u8),
    new(-3, -2, "6"u8),
    new(-3, -1, "-6"u8),
    new(1, 3, "6"u8),
    new(-10, -10, "-10"u8),
    new(0, -1, "1"u8),
    new(-1, -100, "1"u8),
    new(-1, 1, "0"u8),
    new(-1000000000, 0, "0"u8),
    new(-1000000000, 1000000000, "0"u8),
    new(-10, -1, "3628800"u8),
    new(-20, -2, "-2432902008176640000"u8),
    new(-99, -1,
        "-933262154439441526816992388562667004907159682643816214685929"u8 + "638952175999932299156089414639761565182862536979208272237582"u8 + "511852109168640000000000000000000000"u8
    ),
    new(9223372036854775807L, math.MaxInt64, "9223372036854775807"u8),
    new(9223372036854775806L, math.MaxInt64, "85070591730234615838173535747377725442"u8),
    new(9223372036854775805L, math.MaxInt64, "784637716923335094969050127519550606919189611815754530810"u8),
    new(9223372036854775804L, math.MaxInt64, "7237005577332262206126809393809643289012107973151163787181513908099760521240"u8)
}.slice();

public static void TestMulRangeZ(ж<testing.T> Ꮡt) {
    ref var tmp = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡtmp);
    // test entirely positive ranges
    foreach (var (i, r) in mulRangesN) {
        // skip mulRangesN entries that overflow int64
        if ((int64)r.a < 0 || (int64)r.b < 0) {
            continue;
        }
        @string prod = Ꮡtmp.MulRange((int64)r.a, (int64)r.b).String();
        if (prod != r.prod) {
            Ꮡt.Errorf("#%da: got %s; want %s"u8, i, prod, r.prod);
        }
    }
    // test other ranges
    foreach (var (i, r) in mulRangesZ) {
        @string prod = Ꮡtmp.MulRange(r.a, r.b).String();
        if (prod != r.prod) {
            Ꮡt.Errorf("#%db: got %s; want %s"u8, i, prod, r.prod);
        }
    }
}

[GoType("dyn")] internal partial struct TestBinomial_type {
    internal int64 n, k;
    internal @string want;
}

public static void TestBinomial(ж<testing.T> Ꮡt) {
    ref var z = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡz);
    foreach (var (_, test) in new TestBinomial_type[]{
        new(0, 0, "1"u8),
        new(0, 1, "0"u8),
        new(1, 0, "1"u8),
        new(1, 1, "1"u8),
        new(1, 10, "0"u8),
        new(4, 0, "1"u8),
        new(4, 1, "4"u8),
        new(4, 2, "6"u8),
        new(4, 3, "4"u8),
        new(4, 4, "1"u8),
        new(10, 1, "10"u8),
        new(10, 9, "10"u8),
        new(10, 5, "252"u8),
        new(11, 5, "462"u8),
        new(11, 6, "462"u8),
        new(100, 10, "17310309456440"u8),
        new(100, 90, "17310309456440"u8),
        new(1000, 10, "263409560461970212832400"u8),
        new(1000, 990, "263409560461970212832400"u8)
    }.slice()) {
        {
            @string got = Ꮡz.Binomial(test.n, test.k).String(); if (got != test.want) {
                Ꮡt.Errorf("Binomial(%d, %d) = %s; want %s"u8, test.n, test.k, got, test.want);
            }
        }
    }
}

public static void BenchmarkBinomial(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    ref var z = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡz);
    for (nint i = 0; i < b.N; i++) {
        Ꮡz.Binomial(1000, 990);
    }
}

// Examples from the Go Language Spec, section "Arithmetic operators"

[GoType("dyn")] partial struct divisionSignsTestsᴛ1 {
    internal int64 x, y;
    internal int64 q, r; // T-division
    internal int64 d, m; // Euclidean division
}
internal static slice<divisionSignsTestsᴛ1> divisionSignsTests = new divisionSignsTestsᴛ1[]{
    new(5, 3, 1, 2, 1, 2),
    new(-5, 3, -1, -2, -2, 1),
    new(5, -3, -1, 2, -1, 2),
    new(-5, -3, 1, -2, 2, 1),
    new(1, 2, 0, 1, 0, 1),
    new(8, 4, 2, 0, 2, 0)
}.slice();

public static void TestDivisionSigns(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in divisionSignsTests) {
        var x = NewInt(test.x);
        var y = NewInt(test.y);
        var q = NewInt(test.q);
        var r = NewInt(test.r);
        var d = NewInt(test.d);
        var m = NewInt(test.m);
        var q1 = @new<global::go.math.big_package.ΔInt>().Quo(x, y);
        var r1 = @new<global::go.math.big_package.ΔInt>().Rem(x, y);
        if (!isNormalized(q1)) {
            Ꮡt.Errorf("#%d Quo: %v is not normalized"u8, i, q1.Value);
        }
        if (!isNormalized(r1)) {
            Ꮡt.Errorf("#%d Rem: %v is not normalized"u8, i, r1.Value);
        }
        if (q1.Cmp(q) != 0 || r1.Cmp(r) != 0) {
            Ꮡt.Errorf("#%d QuoRem: got (%s, %s), want (%s, %s)"u8, i, q1.OrTypedNil(), r1.OrTypedNil(), q.OrTypedNil(), r.OrTypedNil());
        }
        var (q2, r2) = @new<global::go.math.big_package.ΔInt>().QuoRem(x, y, @new<global::go.math.big_package.ΔInt>());
        if (!isNormalized(q2)) {
            Ꮡt.Errorf("#%d Quo: %v is not normalized"u8, i, q2.Value);
        }
        if (!isNormalized(r2)) {
            Ꮡt.Errorf("#%d Rem: %v is not normalized"u8, i, r2.Value);
        }
        if (q2.Cmp(q) != 0 || r2.Cmp(r) != 0) {
            Ꮡt.Errorf("#%d QuoRem: got (%s, %s), want (%s, %s)"u8, i, q2.OrTypedNil(), r2.OrTypedNil(), q.OrTypedNil(), r.OrTypedNil());
        }
        var d1 = @new<global::go.math.big_package.ΔInt>().Div(x, y);
        var m1 = @new<global::go.math.big_package.ΔInt>().Mod(x, y);
        if (!isNormalized(d1)) {
            Ꮡt.Errorf("#%d Div: %v is not normalized"u8, i, d1.Value);
        }
        if (!isNormalized(m1)) {
            Ꮡt.Errorf("#%d Mod: %v is not normalized"u8, i, m1.Value);
        }
        if (d1.Cmp(d) != 0 || m1.Cmp(m) != 0) {
            Ꮡt.Errorf("#%d DivMod: got (%s, %s), want (%s, %s)"u8, i, d1.OrTypedNil(), m1.OrTypedNil(), d.OrTypedNil(), m.OrTypedNil());
        }
        var (d2, m2) = @new<global::go.math.big_package.ΔInt>().DivMod(x, y, @new<global::go.math.big_package.ΔInt>());
        if (!isNormalized(d2)) {
            Ꮡt.Errorf("#%d Div: %v is not normalized"u8, i, d2.Value);
        }
        if (!isNormalized(m2)) {
            Ꮡt.Errorf("#%d Mod: %v is not normalized"u8, i, m2.Value);
        }
        if (d2.Cmp(d) != 0 || m2.Cmp(m) != 0) {
            Ꮡt.Errorf("#%d DivMod: got (%s, %s), want (%s, %s)"u8, i, d2.OrTypedNil(), m2.OrTypedNil(), d.OrTypedNil(), m.OrTypedNil());
        }
    }
}

internal static global::go.math.big_package.nat Δnorm(global::go.math.big_package.nat x) {
    nint i = len(x);
    while (i > 0 && x[i - 1] == 0) {
        i--;
    }
    return x.slice(0, i);
}

public static void TestBits(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new global::go.math.big_package.nat[]{
        default!,
        new global::go.math.big_package.Word[]{0}.slice(),
        new global::go.math.big_package.Word[]{1}.slice(),
        new global::go.math.big_package.Word[]{0, 1, 2, 3, 4}.slice(),
        new global::go.math.big_package.Word[]{4, 3, 2, 1, 0}.slice(),
        new global::go.math.big_package.Word[]{4, 3, 2, 1, 0, 0, 0, 0}.slice()
    }.slice()) {
        ref var z = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡz);
        z.neg = true;
        var got = Ꮡz.SetBits(test);
        var want = Δnorm(test);
        if ((~got).abs.cmp(want) != 0) {
            Ꮡt.Errorf("SetBits(%v) = %v; want %v"u8, test, (~got).abs, want);
        }
        if ((~got).neg) {
            Ꮡt.Errorf("SetBits(%v): got negative result"u8, test);
        }
        var bits = ((global::go.math.big_package.nat)z.Bits());
        if (bits.cmp(want) != 0) {
            Ꮡt.Errorf("%v.Bits() = %v; want %v"u8, z.abs, bits, want);
        }
    }
}

internal static bool checkSetBytes(slice<byte> b) {
    @string hex1 = hex.EncodeToString(@new<global::go.math.big_package.ΔInt>().SetBytes(b).Bytes());
    @string hex2 = hex.EncodeToString(b);
    while (len(hex1) < len(hex2)) {
        hex1 = "0"u8 + hex1;
    }
    while (len(hex1) > len(hex2)) {
        hex2 = "0"u8 + hex2;
    }
    return hex1 == hex2;
}

public static void TestSetBytes(ж<testing.T> Ꮡt) {
    {
        var err = quick.Check(checkSetBytes, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

internal static bool checkBytes(slice<byte> b) {
    // trim leading zero bytes since Bytes() won't return them
    // (was issue 12231)
    while (len(b) > 0 && b[0] == 0) {
        b = b[1..];
    }
    var b2 = @new<global::go.math.big_package.ΔInt>().SetBytes(b).Bytes();
    return bytes_package.Equal(b, b2);
}

public static void TestBytes(ж<testing.T> Ꮡt) {
    {
        var err = quick.Check(checkBytes, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

internal static bool checkQuo(slice<byte> x, slice<byte> y) {
    var u = @new<global::go.math.big_package.ΔInt>().SetBytes(x);
    var v = @new<global::go.math.big_package.ΔInt>().SetBytes(y);
    if (len((~v).abs) == 0) {
        return true;
    }
    var r = @new<global::go.math.big_package.ΔInt>();
    (var q, r) = @new<global::go.math.big_package.ΔInt>().QuoRem(u, v, r);
    if (r.Cmp(v) >= 0) {
        return false;
    }
    var uprime = @new<global::go.math.big_package.ΔInt>().Set(q);
    uprime.Mul(uprime, v);
    uprime.Add(uprime, r);
    return uprime.Cmp(u) == 0;
}


[GoType("dyn")] partial struct quoTestsᴛ1 {
    internal @string x, y;
    internal @string q, r;
}
internal static slice<quoTestsᴛ1> quoTests = new quoTestsᴛ1[]{
    new(
        "476217953993950760840509444250624797097991362735329973741718102894495832294430498335824897858659711275234906400899559094370964723884706254265559534144986498357"u8,
        "9353930466774385905609975137998169297361893554149986716853295022578535724979483772383667534691121982974895531435241089241440253066816724367338287092081996"u8,
        "50911"u8,
        "1"u8
    ),
    new(
        "11510768301994997771168"u8,
        "1328165573307167369775"u8,
        "8"u8,
        "885443715537658812968"u8
    )
}.slice();

public static void TestQuo(ж<testing.T> Ꮡt) {
    {
        var err = quick.Check(checkQuo, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    foreach (var (i, test) in quoTests) {
        var (x, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.x, 10);
        var (y, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.y, 10);
        var (expectedQ, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.q, 10);
        var (expectedR, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.r, 10);
        var r = @new<global::go.math.big_package.ΔInt>();
        (var q, r) = @new<global::go.math.big_package.ΔInt>().QuoRem(x, y, r);
        if (q.Cmp(expectedQ) != 0 || r.Cmp(expectedR) != 0) {
            Ꮡt.Errorf("#%d got (%s, %s) want (%s, %s)"u8, i, q.OrTypedNil(), r.OrTypedNil(), expectedQ.OrTypedNil(), expectedR.OrTypedNil());
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedQ64ᶜ = "18446744073709551613"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedR64ᶜ = "3138550867693340382088035895064302439801311770021610913807"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedQ32ᶜ = "4294967293"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string expectedR32ᶜ = "39614081266355540837921718287"u8;

public static void TestQuoStepD6(ж<testing.T> Ꮡt) {
    // See Knuth, Volume 2, section 4.3.1, exercise 21. This code exercises
    // a code path which only triggers 1 in 10^{-19} cases.
    var u = Ꮡ(new ΔInt(false, new nat(new global::go.math.big_package.Word[]{0, 0, unchecked((nuint)(9223372036854775809UL)), unchecked((nuint)(9223372036854775807UL))}.slice())));
    var v = Ꮡ(new ΔInt(false, new nat(new global::go.math.big_package.Word[]{5, unchecked((nuint)(9223372036854775810UL)), (global::go.math.big_package.Word)((nuint)1 << (int)((_W - 1)))}.slice())));
    var r = @new<global::go.math.big_package.ΔInt>();
    (var q, r) = @new<global::go.math.big_package.ΔInt>().QuoRem(u, v, r);
    @string expectedQ64 = expectedQ64ᶜ;
    @string expectedR64 = expectedR64ᶜ;
    @string expectedQ32 = expectedQ32ᶜ;
    @string expectedR32 = expectedR32ᶜ;
    if (q.String() != expectedQ64 && q.String() != expectedQ32 || r.String() != expectedR64 && r.String() != expectedR32) {
        Ꮡt.Errorf("got (%s, %s) want (%s, %s) or (%s, %s)"u8, q.OrTypedNil(), r.OrTypedNil(), expectedQ64, expectedR64, expectedQ32, expectedR32);
    }
}

public static void BenchmarkQuoRem(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (x, _) = @new<global::go.math.big_package.ΔInt>().SetString("153980389784927331788354528594524332344709972855165340650588877572729725338415474372475094155672066328274535240275856844648695200875763869073572078279316458648124537905600131008790701752441155668003033945258023841165089852359980273279085783159654751552359397986180318708491098942831252291841441726305535546071"u8, 0);
    var (y, _) = @new<global::go.math.big_package.ΔInt>().SetString("7746362281539803897849273317883545285945243323447099728551653406505888775727297253384154743724750941556720663282745352402758568446486952008757638690735720782793164586481245379056001310087907017524411556680030339452580238411650898523599802732790857831596547515523593979861803187084910989428312522918414417263055355460715745539358014631136245887418412633787074173796862711588221766398229333338511838891484974940633857861775630560092874987828057333663969469797013996401149696897591265769095952887917296740109742927689053276850469671231961384715398038978492733178835452859452433234470997285516534065058887757272972533841547437247509415567206632827453524027585684464869520087576386907357207827931645864812453790560013100879070175244115566800303394525802384116508985235998027327908578315965475155235939798618031870849109894283125229184144172630553554607112725169432413343763989564437170644270643461665184965150423819594083121075825"u8, 0);
    var q = @new<global::go.math.big_package.ΔInt>();
    var r = @new<global::go.math.big_package.ΔInt>();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        q.QuoRem(y, x, r);
    }
}


[GoType("dyn")] partial struct bitLenTestsᴛ1 {
    internal @string @in;
    internal nint @out;
}
internal static slice<bitLenTestsᴛ1> bitLenTests = new bitLenTestsᴛ1[]{
    new("-1"u8, 1),
    new("0"u8, 0),
    new("1"u8, 1),
    new("2"u8, 2),
    new("4"u8, 3),
    new("0xabc"u8, 12),
    new("0x8000"u8, 16),
    new("0x80000000"u8, 32),
    new("0x800000000000"u8, 48),
    new("0x8000000000000000"u8, 64),
    new("0x80000000000000000000"u8, 80),
    new("-0x4000000000000000000000"u8, 87)
}.slice();

public static void TestBitLen(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in bitLenTests) {
        var (x, ok) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, 0);
        if (!ok) {
            Ꮡt.Errorf("#%d test input invalid: %s"u8, i, test.@in);
            continue;
        }
        {
            nint n = x.BitLen(); if (n != test.@out) {
                Ꮡt.Errorf("#%d got %d want %d"u8, i, n, test.@out);
            }
        }
    }
}

// y <= 0
// m == 1
// misc
// 3663 = ModInverse(3199, 6719) Issue #25865
// test case for issue 8822
// test cases for issue 13907
// odd
// even

[GoType("dyn")] partial struct expTestsᴛ1 {
    internal @string x, y, m;
    internal @string @out;
}
internal static slice<expTestsᴛ1> expTests = new expTestsᴛ1[]{
    new("0"u8, "0"u8, ""u8, "1"u8),
    new("1"u8, "0"u8, ""u8, "1"u8),
    new("-10"u8, "0"u8, ""u8, "1"u8),
    new("1234"u8, "-1"u8, ""u8, "1"u8),
    new("1234"u8, "-1"u8, "0"u8, "1"u8),
    new("17"u8, "-100"u8, "1234"u8, "865"u8),
    new("2"u8, "-100"u8, "1234"u8, ""u8),
    new("0"u8, "0"u8, "1"u8, "0"u8),
    new("1"u8, "0"u8, "1"u8, "0"u8),
    new("-10"u8, "0"u8, "1"u8, "0"u8),
    new("1234"u8, "-1"u8, "1"u8, "0"u8),
    new("5"u8, "1"u8, "3"u8, "2"u8),
    new("5"u8, "-7"u8, ""u8, "1"u8),
    new("-5"u8, "-7"u8, ""u8, "1"u8),
    new("5"u8, "0"u8, ""u8, "1"u8),
    new("-5"u8, "0"u8, ""u8, "1"u8),
    new("5"u8, "1"u8, ""u8, "5"u8),
    new("-5"u8, "1"u8, ""u8, "-5"u8),
    new("-5"u8, "1"u8, "7"u8, "2"u8),
    new("-2"u8, "3"u8, "2"u8, "0"u8),
    new("5"u8, "2"u8, ""u8, "25"u8),
    new("1"u8, "65537"u8, "2"u8, "1"u8),
    new("0x8000000000000000"u8, "2"u8, ""u8, "0x40000000000000000000000000000000"u8),
    new("0x8000000000000000"u8, "2"u8, "6719"u8, "4944"u8),
    new("0x8000000000000000"u8, "3"u8, "6719"u8, "5447"u8),
    new("0x8000000000000000"u8, "1000"u8, "6719"u8, "1603"u8),
    new("0x8000000000000000"u8, "1000000"u8, "6719"u8, "3199"u8),
    new("0x8000000000000000"u8, "-1000000"u8, "6719"u8, "3663"u8),
    new("0xffffffffffffffffffffffffffffffff"u8, "0x12345678123456781234567812345678123456789"u8, "0x01112222333344445555666677778889"u8, "0x36168FA1DB3AAE6C8CE647E137F97A"u8),
    new(
        "2938462938472983472983659726349017249287491026512746239764525612965293865296239471239874193284792387498274256129746192347"u8,
        "298472983472983471903246121093472394872319615612417471234712061"u8,
        "29834729834729834729347290846729561262544958723956495615629569234729836259263598127342374289365912465901365498236492183464"u8,
        "23537740700184054162508175125554701713153216681790245129157191391322321508055833908509185839069455749219131480588829346291"u8
    ),
    new(
        "11001289118363089646017359372117963499250546375269047542777928006103246876688756735760905680604646624353196869572752623285140408755420374049317646428185270079555372763503115646054602867593662923894140940837479507194934267532831694565516466765025434902348314525627418515646588160955862839022051353653052947073136084780742729727874803457643848197499548297570026926927502505634297079527299004267769780768565695459945235586892627059178884998772989397505061206395455591503771677500931269477503508150175717121828518985901959919560700853226255420793148986854391552859459511723547532575574664944815966793196961286234040892865"u8,
        "0xB08FFB20760FFED58FADA86DFEF71AD72AA0FA763219618FE022C197E54708BB1191C66470250FCE8879487507CEE41381CA4D932F81C2B3F1AB20B539D50DCD"u8,
        "0xAC6BDB41324A9A9BF166DE5E1389582FAF72B6651987EE07FC3192943DB56050A37329CBB4A099ED8193E0757767A13DD52312AB4B03310DCD7F48A9DA04FD50E8083969EDB767B0CF6095179A163AB3661A05FBD5FAAAE82918A9962F0B93B855F97993EC975EEAA80D740ADBF4FF747359D041D5C33EA71D281E446B14773BCA97B43A23FB801676BD207A436C6481F1D2B9078717461A5B9D32E688F87748544523B524B0D57D5EA77A2775D2ECFA032CFBDBF52FB3786160279004E57AE6AF874E7303CE53299CCC041C7BC308D82A5698F3A8D0C38271AE35F8E9DBFBB694B5C803D89F7AE435DE236D525F54759B65E372FCD68EF20FA7111F9E4AFF73"u8,
        "21484252197776302499639938883777710321993113097987201050501182909581359357618579566746556372589385361683610524730509041328855066514963385522570894839035884713051640171474186548713546686476761306436434146475140156284389181808675016576845833340494848283681088886584219750554408060556769486628029028720727393293111678826356480455433909233520504112074401376133077150471237549474149190242010469539006449596611576612573955754349042329130631128234637924786466585703488460540228477440853493392086251021228087076124706778899179648655221663765993962724699135217212118535057766739392069738618682722216712319320435674779146070442"u8
    ),
    new(
        "-0x1BCE04427D8032319A89E5C4136456671AC620883F2C4139E57F91307C485AD2D6204F4F87A58262652DB5DBBAC72B0613E51B835E7153BEC6068F5C8D696B74DBD18FEC316AEF73985CF0475663208EB46B4F17DD9DA55367B03323E5491A70997B90C059FB34809E6EE55BCFBD5F2F52233BFE62E6AA9E4E26A1D4C2439883D14F2633D55D8AA66A1ACD5595E778AC3A280517F1157989E70C1A437B849F1877B779CC3CDDEDE2DAA6594A6C66D181A00A5F777EE60596D8773998F6E988DEAE4CCA60E4DDCF9590543C89F74F603259FCAD71660D30294FBBE6490300F78A9D63FA660DC9417B8B9DDA28BEB3977B621B988E23D4D954F322C3540541BC649ABD504C50FADFD9F0987D58A2BF689313A285E773FF02899A6EF887D1D4A0D2"u8,
        "0xB08FFB20760FFED58FADA86DFEF71AD72AA0FA763219618FE022C197E54708BB1191C66470250FCE8879487507CEE41381CA4D932F81C2B3F1AB20B539D50DCD"u8,
        "0xAC6BDB41324A9A9BF166DE5E1389582FAF72B6651987EE07FC3192943DB56050A37329CBB4A099ED8193E0757767A13DD52312AB4B03310DCD7F48A9DA04FD50E8083969EDB767B0CF6095179A163AB3661A05FBD5FAAAE82918A9962F0B93B855F97993EC975EEAA80D740ADBF4FF747359D041D5C33EA71D281E446B14773BCA97B43A23FB801676BD207A436C6481F1D2B9078717461A5B9D32E688F87748544523B524B0D57D5EA77A2775D2ECFA032CFBDBF52FB3786160279004E57AE6AF874E7303CE53299CCC041C7BC308D82A5698F3A8D0C38271AE35F8E9DBFBB694B5C803D89F7AE435DE236D525F54759B65E372FCD68EF20FA7111F9E4AFF73"u8,
        "21484252197776302499639938883777710321993113097987201050501182909581359357618579566746556372589385361683610524730509041328855066514963385522570894839035884713051640171474186548713546686476761306436434146475140156284389181808675016576845833340494848283681088886584219750554408060556769486628029028720727393293111678826356480455433909233520504112074401376133077150471237549474149190242010469539006449596611576612573955754349042329130631128234637924786466585703488460540228477440853493392086251021228087076124706778899179648655221663765993962724699135217212118535057766739392069738618682722216712319320435674779146070442"u8
    ),
    new("0xffffffff00000001"u8, "0xffffffff00000001"u8, "0xffffffff00000001"u8, "0"u8),
    new("0xffffffffffffffff00000001"u8, "0xffffffffffffffff00000001"u8, "0xffffffffffffffff00000001"u8, "0"u8),
    new("0xffffffffffffffffffffffff00000001"u8, "0xffffffffffffffffffffffff00000001"u8, "0xffffffffffffffffffffffff00000001"u8, "0"u8),
    new("0xffffffffffffffffffffffffffffffff00000001"u8, "0xffffffffffffffffffffffffffffffff00000001"u8, "0xffffffffffffffffffffffffffffffff00000001"u8, "0"u8),
    new(
        "2"u8,
        "0xB08FFB20760FFED58FADA86DFEF71AD72AA0FA763219618FE022C197E54708BB1191C66470250FCE8879487507CEE41381CA4D932F81C2B3F1AB20B539D50DCD"u8,
        "0xAC6BDB41324A9A9BF166DE5E1389582FAF72B6651987EE07FC3192943DB56050A37329CBB4A099ED8193E0757767A13DD52312AB4B03310DCD7F48A9DA04FD50E8083969EDB767B0CF6095179A163AB3661A05FBD5FAAAE82918A9962F0B93B855F97993EC975EEAA80D740ADBF4FF747359D041D5C33EA71D281E446B14773BCA97B43A23FB801676BD207A436C6481F1D2B9078717461A5B9D32E688F87748544523B524B0D57D5EA77A2775D2ECFA032CFBDBF52FB3786160279004E57AE6AF874E7303CE53299CCC041C7BC308D82A5698F3A8D0C38271AE35F8E9DBFBB694B5C803D89F7AE435DE236D525F54759B65E372FCD68EF20FA7111F9E4AFF73"u8,
        "0x6AADD3E3E424D5B713FCAA8D8945B1E055166132038C57BBD2D51C833F0C5EA2007A2324CE514F8E8C2F008A2F36F44005A4039CB55830986F734C93DAF0EB4BAB54A6A8C7081864F44346E9BC6F0A3EB9F2C0146A00C6A05187D0C101E1F2D038CDB70CB5E9E05A2D188AB6CBB46286624D4415E7D4DBFAD3BCC6009D915C406EED38F468B940F41E6BEDC0430DD78E6F19A7DA3A27498A4181E24D738B0072D8F6ADB8C9809A5B033A09785814FD9919F6EF9F83EEA519BEC593855C4C10CBEEC582D4AE0792158823B0275E6AEC35242740468FAF3D5C60FD1E376362B6322F78B7ED0CA1C5BBCD2B49734A56C0967A1D01A100932C837B91D592CE08ABFF"u8
    ),
    new(
        "2"u8,
        "0xB08FFB20760FFED58FADA86DFEF71AD72AA0FA763219618FE022C197E54708BB1191C66470250FCE8879487507CEE41381CA4D932F81C2B3F1AB20B539D50DCD"u8,
        "0xAC6BDB41324A9A9BF166DE5E1389582FAF72B6651987EE07FC3192943DB56050A37329CBB4A099ED8193E0757767A13DD52312AB4B03310DCD7F48A9DA04FD50E8083969EDB767B0CF6095179A163AB3661A05FBD5FAAAE82918A9962F0B93B855F97993EC975EEAA80D740ADBF4FF747359D041D5C33EA71D281E446B14773BCA97B43A23FB801676BD207A436C6481F1D2B9078717461A5B9D32E688F87748544523B524B0D57D5EA77A2775D2ECFA032CFBDBF52FB3786160279004E57AE6AF874E7303CE53299CCC041C7BC308D82A5698F3A8D0C38271AE35F8E9DBFBB694B5C803D89F7AE435DE236D525F54759B65E372FCD68EF20FA7111F9E4AFF72"u8,
        "0x7858794B5897C29F4ED0B40913416AB6C48588484E6A45F2ED3E26C941D878E923575AAC434EE2750E6439A6976F9BB4D64CEDB2A53CE8D04DD48CADCDF8E46F22747C6B81C6CEA86C0D873FBF7CEF262BAAC43A522BD7F32F3CDAC52B9337C77B3DCFB3DB3EDD80476331E82F4B1DF8EFDC1220C92656DFC9197BDC1877804E28D928A2A284B8DED506CBA304435C9D0133C246C98A7D890D1DE60CBC53A024361DA83A9B8775019083D22AC6820ED7C3C68F8E801DD4EC779EE0A05C6EB682EF9840D285B838369BA7E148FA27691D524FAEAF7C6ECE2A4B99A294B9F2C241857B5B90CC8BFFCFCF18DFA7D676131D5CD3855A5A3E8EBFA0CDFADB4D198B4A"u8
    )
}.slice();

public static void TestExp(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in expTests) {
        var (x, ok1) = @new<global::go.math.big_package.ΔInt>().SetString(test.x, 0);
        var (y, ok2) = @new<global::go.math.big_package.ΔInt>().SetString(test.y, 0);
        bool ok3 = default!;
        bool ok4 = default!;
        ж<global::go.math.big_package.ΔInt> @out = default!;
        ж<global::go.math.big_package.ΔInt> m = default!;
        if (len(test.@out) == 0){
            (@out, ok3) = (default!, true);
        } else {
            (@out, ok3) = @new<global::go.math.big_package.ΔInt>().SetString(test.@out, 0);
        }
        if (len(test.m) == 0){
            (m, ok4) = (default!, true);
        } else {
            (m, ok4) = @new<global::go.math.big_package.ΔInt>().SetString(test.m, 0);
        }
        if (!ok1 || !ok2 || !ok3 || !ok4) {
            Ꮡt.Errorf("#%d: error in input"u8, i);
            continue;
        }
        var z1 = @new<global::go.math.big_package.ΔInt>().Exp(x, y, m);
        if (z1 != nil && !isNormalized(z1)) {
            Ꮡt.Errorf("#%d: %v is not normalized"u8, i, z1.Value);
        }
        if (!(z1 == nil && @out == nil || z1.Cmp(@out) == 0)) {
            Ꮡt.Errorf("#%d: got %x want %x"u8, i, z1.OrTypedNil(), @out.OrTypedNil());
        }
        if (m == nil) {
            // The result should be the same as for m == 0;
            // specifically, there should be no div-zero panic.
            m = Ꮡ(new ΔInt(abs: new nat(new global::go.math.big_package.Word[]{}.slice()))); // m != nil && len(m.abs) == 0
            var z2 = @new<global::go.math.big_package.ΔInt>().Exp(x, y, m);
            if (z2.Cmp(z1) != 0) {
                Ꮡt.Errorf("#%d: got %x want %x"u8, i, z2.OrTypedNil(), z1.OrTypedNil());
            }
        }
    }
}

public static void BenchmarkExp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (x, _) = @new<global::go.math.big_package.ΔInt>().SetString("11001289118363089646017359372117963499250546375269047542777928006103246876688756735760905680604646624353196869572752623285140408755420374049317646428185270079555372763503115646054602867593662923894140940837479507194934267532831694565516466765025434902348314525627418515646588160955862839022051353653052947073136084780742729727874803457643848197499548297570026926927502505634297079527299004267769780768565695459945235586892627059178884998772989397505061206395455591503771677500931269477503508150175717121828518985901959919560700853226255420793148986854391552859459511723547532575574664944815966793196961286234040892865"u8, 0);
    var (y, _) = @new<global::go.math.big_package.ΔInt>().SetString("0xAC6BDB41324A9A9BF166DE5E1389582FAF72B6651987EE07FC3192943DB56050A37329CBB4A099ED8193E0757767A13DD52312AB4B03310DCD7F48A9DA04FD50E8083969EDB767B0CF6095179A163AB3661A05FBD5FAAAE82918A9962F0B93B855F97993EC975EEAA80D740ADBF4FF747359D041D5C33EA71D281E446B14773BCA97B43A23FB801676BD207A436C6481F1D2B9078717461A5B9D32E688F87748544523B524B0D57D5EA77A2775D2ECFA032CFBDBF52FB3786160279004E57AE6AF874E7303CE53299CCC041C7BC308D82A5698F3A8D0C38271AE35F8E9DBFBB694B5C803D89F7AE435DE236D525F54759B65E372FCD68EF20FA7111F9E4AFF72"u8, 0);
    var (n, _) = @new<global::go.math.big_package.ΔInt>().SetString("0xAC6BDB41324A9A9BF166DE5E1389582FAF72B6651987EE07FC3192943DB56050A37329CBB4A099ED8193E0757767A13DD52312AB4B03310DCD7F48A9DA04FD50E8083969EDB767B0CF6095179A163AB3661A05FBD5FAAAE82918A9962F0B93B855F97993EC975EEAA80D740ADBF4FF747359D041D5C33EA71D281E446B14773BCA97B43A23FB801676BD207A436C6481F1D2B9078717461A5B9D32E688F87748544523B524B0D57D5EA77A2775D2ECFA032CFBDBF52FB3786160279004E57AE6AF874E7303CE53299CCC041C7BC308D82A5698F3A8D0C38271AE35F8E9DBFBB694B5C803D89F7AE435DE236D525F54759B65E372FCD68EF20FA7111F9E4AFF73"u8, 0);
    var @out = @new<global::go.math.big_package.ΔInt>();
    for (nint i = 0; i < b.N; i++) {
        @out.Exp(x, y, n);
    }
}

[GoType("dyn")] internal partial struct BenchmarkExpMont_type {
    internal @string name;
    internal @string val;
}

public static void BenchmarkExpMont(ж<testing.B> Ꮡb) {
    var (x, _) = @new<global::go.math.big_package.ΔInt>().SetString("297778224889315382157302278696111964193"u8, 0);
    var (y, _) = @new<global::go.math.big_package.ΔInt>().SetString("2548977943381019743024248146923164919440527843026415174732254534318292492375775985739511369575861449426580651447974311336267954477239437734832604782764979371984246675241012538135715981292390886872929238062252506842498360562303324154310849745753254532852868768268023732398278338025070694508489163836616810661033068070127919590264734220833816416141878688318329193389865030063416339367925710474801991305827284114894677717927892032165200876093838921477120036402410731159852999623461591709308405270748511350289172153076023215"u8, 0);
    slice<BenchmarkExpMont_type> mods = new BenchmarkExpMont_type[]{
        new("Odd"u8, "0x82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF"u8),
        new("Even1"u8, "0x82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FE"u8),
        new("Even2"u8, "0x82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FC"u8),
        new("Even3"u8, "0x82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281F8"u8),
        new("Even4"u8, "0x82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281F0"u8),
        new("Even8"u8, "0x82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B21828100"u8),
        new("Even32"u8, "0x82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B00000000"u8),
        new("Even64"u8, "0x82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF82828282828200FF0000000000000000"u8),
        new("Even96"u8, "0x82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF82828283000000000000000000000000"u8),
        new("Even128"u8, "0x82828282828200FFFF28FF2B218281FF82828282828200FFFF28FF2B218281FF00000000000000000000000000000000"u8),
        new("Even255"u8, "0x82828282828200FFFF28FF2B218281FF8000000000000000000000000000000000000000000000000000000000000000"u8),
        new("SmallEven1"u8, "0x7E"u8),
        new("SmallEven2"u8, "0x7C"u8),
        new("SmallEven3"u8, "0x78"u8),
        new("SmallEven4"u8, "0x70"u8)
    }.slice();
    foreach (var (_, mod) in mods) {
        var (n, _) = @new<global::go.math.big_package.ΔInt>().SetString(mod.val, 0);
        var @out = @new<global::go.math.big_package.ΔInt>();
        var nʗ1 = n;
        var outʗ1 = @out;
        var xʗ1 = x;
        var yʗ1 = y;
        Ꮡb.Run(mod.name, (ж<testing.B> bΔ1) => {
            bΔ1.ReportAllocs();
            for (nint i = 0; i < (~bΔ1).N; i++) {
                outʗ1.Exp(xʗ1, yʗ1, nʗ1);
            }
        });
    }
}

public static void BenchmarkExp2(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (x, _) = @new<global::go.math.big_package.ΔInt>().SetString("2"u8, 0);
    var (y, _) = @new<global::go.math.big_package.ΔInt>().SetString("0xAC6BDB41324A9A9BF166DE5E1389582FAF72B6651987EE07FC3192943DB56050A37329CBB4A099ED8193E0757767A13DD52312AB4B03310DCD7F48A9DA04FD50E8083969EDB767B0CF6095179A163AB3661A05FBD5FAAAE82918A9962F0B93B855F97993EC975EEAA80D740ADBF4FF747359D041D5C33EA71D281E446B14773BCA97B43A23FB801676BD207A436C6481F1D2B9078717461A5B9D32E688F87748544523B524B0D57D5EA77A2775D2ECFA032CFBDBF52FB3786160279004E57AE6AF874E7303CE53299CCC041C7BC308D82A5698F3A8D0C38271AE35F8E9DBFBB694B5C803D89F7AE435DE236D525F54759B65E372FCD68EF20FA7111F9E4AFF72"u8, 0);
    var (n, _) = @new<global::go.math.big_package.ΔInt>().SetString("0xAC6BDB41324A9A9BF166DE5E1389582FAF72B6651987EE07FC3192943DB56050A37329CBB4A099ED8193E0757767A13DD52312AB4B03310DCD7F48A9DA04FD50E8083969EDB767B0CF6095179A163AB3661A05FBD5FAAAE82918A9962F0B93B855F97993EC975EEAA80D740ADBF4FF747359D041D5C33EA71D281E446B14773BCA97B43A23FB801676BD207A436C6481F1D2B9078717461A5B9D32E688F87748544523B524B0D57D5EA77A2775D2ECFA032CFBDBF52FB3786160279004E57AE6AF874E7303CE53299CCC041C7BC308D82A5698F3A8D0C38271AE35F8E9DBFBB694B5C803D89F7AE435DE236D525F54759B65E372FCD68EF20FA7111F9E4AFF73"u8, 0);
    var @out = @new<global::go.math.big_package.ΔInt>();
    for (nint i = 0; i < b.N; i++) {
        @out.Exp(x, y, n);
    }
}

internal static bool checkGcd(slice<byte> aBytes, slice<byte> bBytes) {
    var x = @new<global::go.math.big_package.ΔInt>();
    var y = @new<global::go.math.big_package.ΔInt>();
    var a = @new<global::go.math.big_package.ΔInt>().SetBytes(aBytes);
    var b = @new<global::go.math.big_package.ΔInt>().SetBytes(bBytes);
    var d = @new<global::go.math.big_package.ΔInt>().GCD(x, y, a, b);
    x.Mul(x, a);
    y.Mul(y, b);
    x.Add(x, y);
    return x.Cmp(d) == 0;
}

// euclidExtGCD is a reference implementation of Euclid's
// extended GCD algorithm for testing against optimized algorithms.
// Requirements: a, b > 0
internal static (ж<global::go.math.big_package.ΔInt> g, ж<global::go.math.big_package.ΔInt> x, ж<global::go.math.big_package.ΔInt> y) euclidExtGCD(ж<global::go.math.big_package.ΔInt> Ꮡa, ж<global::go.math.big_package.ΔInt> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    var A = @new<global::go.math.big_package.ΔInt>().Set(Ꮡa);
    var B = @new<global::go.math.big_package.ΔInt>().Set(Ꮡb);
    // A = Ua*a + Va*b
    // B = Ub*a + Vb*b
    var Ua = @new<global::go.math.big_package.ΔInt>().SetInt64(1);
    var Va = @new<global::go.math.big_package.ΔInt>();
    var Ub = @new<global::go.math.big_package.ΔInt>();
    var Vb = @new<global::go.math.big_package.ΔInt>().SetInt64(1);
    var q = @new<global::go.math.big_package.ΔInt>();
    var temp = @new<global::go.math.big_package.ΔInt>();
    var r = @new<global::go.math.big_package.ΔInt>();
    while (len((~B).abs) > 0) {
        (q, r) = q.QuoRem(A, B, r);
        (A, B, r) = (B, r, A);
        // Ua, Ub = Ub, Ua-q*Ub
        temp.Set(Ub);
        Ub.Mul(Ub, q);
        Ub.Sub(Ua, Ub);
        Ua.Set(temp);
        // Va, Vb = Vb, Va-q*Vb
        temp.Set(Vb);
        Vb.Mul(Vb, q);
        Vb.Sub(Va, Vb);
        Va.Set(temp);
    }
    return (A, Ua, Va);
}

internal static bool checkLehmerGcd(slice<byte> aBytes, slice<byte> bBytes) {
    var a = @new<global::go.math.big_package.ΔInt>().SetBytes(aBytes);
    var b = @new<global::go.math.big_package.ΔInt>().SetBytes(bBytes);
    if (a.Sign() <= 0 || b.Sign() <= 0) {
        return true; // can only test positive arguments
    }
    var d = @new<global::go.math.big_package.ΔInt>().lehmerGCD(nil, nil, a, b);
    var (d0, _, _) = euclidExtGCD(a, b);
    return d.Cmp(d0) == 0;
}

internal static bool checkLehmerExtGcd(slice<byte> aBytes, slice<byte> bBytes) {
    var a = @new<global::go.math.big_package.ΔInt>().SetBytes(aBytes);
    var b = @new<global::go.math.big_package.ΔInt>().SetBytes(bBytes);
    var x = @new<global::go.math.big_package.ΔInt>();
    var y = @new<global::go.math.big_package.ΔInt>();
    if (a.Sign() <= 0 || b.Sign() <= 0) {
        return true; // can only test positive arguments
    }
    var d = @new<global::go.math.big_package.ΔInt>().lehmerGCD(x, y, a, b);
    var (d0, x0, y0) = euclidExtGCD(a, b);
    return d.Cmp(d0) == 0 && x.Cmp(x0) == 0 && y.Cmp(y0) == 0;
}

// a <= 0 || b <= 0

[GoType("dyn")] partial struct gcdTestsᴛ1 {
    internal @string d, x, y, a, b;
}
internal static slice<gcdTestsᴛ1> gcdTests = new gcdTestsᴛ1[]{
    new("0"u8, "0"u8, "0"u8, "0"u8, "0"u8),
    new("7"u8, "0"u8, "1"u8, "0"u8, "7"u8),
    new("7"u8, "0"u8, "-1"u8, "0"u8, "-7"u8),
    new("11"u8, "1"u8, "0"u8, "11"u8, "0"u8),
    new("7"u8, "-1"u8, "-2"u8, "-77"u8, "35"u8),
    new("935"u8, "-3"u8, "8"u8, "64515"u8, "24310"u8),
    new("935"u8, "-3"u8, "-8"u8, "64515"u8, "-24310"u8),
    new("935"u8, "3"u8, "-8"u8, "-64515"u8, "-24310"u8),
    new("1"u8, "-9"u8, "47"u8, "120"u8, "23"u8),
    new("7"u8, "1"u8, "-2"u8, "77"u8, "35"u8),
    new("935"u8, "-3"u8, "8"u8, "64515"u8, "24310"u8),
    new("935000000000000000"u8, "-3"u8, "8"u8, "64515000000000000000"u8, "24310000000000000000"u8),
    new("1"u8, "-221"u8, "22059940471369027483332068679400581064239780177629666810348940098015901108344"u8, "98920366548084643601728869055592650835572950932266967461790948584315647051443"u8, "991"u8)
}.slice();

internal static void testGcd(ж<testing.T> Ꮡt, ж<global::go.math.big_package.ΔInt> Ꮡd, ж<global::go.math.big_package.ΔInt> Ꮡx, ж<global::go.math.big_package.ΔInt> Ꮡy, ж<global::go.math.big_package.ΔInt> Ꮡa, ж<global::go.math.big_package.ΔInt> Ꮡb) {
    ref var d = ref Ꮡd.DerefOrNull();
    ref var x = ref Ꮡx.DerefOrNull();
    ref var y = ref Ꮡy.DerefOrNull();
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    ж<global::go.math.big_package.ΔInt> X = default!;
    if (Ꮡx != nil) {
        X = @new<global::go.math.big_package.ΔInt>();
    }
    ж<global::go.math.big_package.ΔInt> Y = default!;
    if (Ꮡy != nil) {
        Y = @new<global::go.math.big_package.ΔInt>();
    }
    var D = @new<global::go.math.big_package.ΔInt>().GCD(X, Y, Ꮡa, Ꮡb);
    if (D.Cmp(Ꮡd) != 0) {
        Ꮡt.Errorf("GCD(%s, %s, %s, %s): got d = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), D.OrTypedNil(), Ꮡd.OrTypedNil());
    }
    if (Ꮡx != nil && X.Cmp(Ꮡx) != 0) {
        Ꮡt.Errorf("GCD(%s, %s, %s, %s): got x = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), X.OrTypedNil(), Ꮡx.OrTypedNil());
    }
    if (Ꮡy != nil && Y.Cmp(Ꮡy) != 0) {
        Ꮡt.Errorf("GCD(%s, %s, %s, %s): got y = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), Y.OrTypedNil(), Ꮡy.OrTypedNil());
    }
    // check results in presence of aliasing (issue #11284)
    var a2 = @new<global::go.math.big_package.ΔInt>().Set(Ꮡa);
    var b2 = @new<global::go.math.big_package.ΔInt>().Set(Ꮡb);
    a2.GCD(X, Y, a2, b2); // result is same as 1st argument
    if (a2.Cmp(Ꮡd) != 0) {
        Ꮡt.Errorf("aliased z = a GCD(%s, %s, %s, %s): got d = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), a2.OrTypedNil(), Ꮡd.OrTypedNil());
    }
    if (Ꮡx != nil && X.Cmp(Ꮡx) != 0) {
        Ꮡt.Errorf("aliased z = a GCD(%s, %s, %s, %s): got x = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), X.OrTypedNil(), Ꮡx.OrTypedNil());
    }
    if (Ꮡy != nil && Y.Cmp(Ꮡy) != 0) {
        Ꮡt.Errorf("aliased z = a GCD(%s, %s, %s, %s): got y = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), Y.OrTypedNil(), Ꮡy.OrTypedNil());
    }
    a2 = @new<global::go.math.big_package.ΔInt>().Set(Ꮡa);
    b2 = @new<global::go.math.big_package.ΔInt>().Set(Ꮡb);
    b2.GCD(X, Y, a2, b2); // result is same as 2nd argument
    if (b2.Cmp(Ꮡd) != 0) {
        Ꮡt.Errorf("aliased z = b GCD(%s, %s, %s, %s): got d = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), b2.OrTypedNil(), Ꮡd.OrTypedNil());
    }
    if (Ꮡx != nil && X.Cmp(Ꮡx) != 0) {
        Ꮡt.Errorf("aliased z = b GCD(%s, %s, %s, %s): got x = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), X.OrTypedNil(), Ꮡx.OrTypedNil());
    }
    if (Ꮡy != nil && Y.Cmp(Ꮡy) != 0) {
        Ꮡt.Errorf("aliased z = b GCD(%s, %s, %s, %s): got y = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), Y.OrTypedNil(), Ꮡy.OrTypedNil());
    }
    a2 = @new<global::go.math.big_package.ΔInt>().Set(Ꮡa);
    b2 = @new<global::go.math.big_package.ΔInt>().Set(Ꮡb);
    D = @new<global::go.math.big_package.ΔInt>().GCD(a2, b2, a2, b2); // x = a, y = b
    if (D.Cmp(Ꮡd) != 0) {
        Ꮡt.Errorf("aliased x = a, y = b GCD(%s, %s, %s, %s): got d = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), D.OrTypedNil(), Ꮡd.OrTypedNil());
    }
    if (Ꮡx != nil && a2.Cmp(Ꮡx) != 0) {
        Ꮡt.Errorf("aliased x = a, y = b GCD(%s, %s, %s, %s): got x = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), a2.OrTypedNil(), Ꮡx.OrTypedNil());
    }
    if (Ꮡy != nil && b2.Cmp(Ꮡy) != 0) {
        Ꮡt.Errorf("aliased x = a, y = b GCD(%s, %s, %s, %s): got y = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), b2.OrTypedNil(), Ꮡy.OrTypedNil());
    }
    a2 = @new<global::go.math.big_package.ΔInt>().Set(Ꮡa);
    b2 = @new<global::go.math.big_package.ΔInt>().Set(Ꮡb);
    D = @new<global::go.math.big_package.ΔInt>().GCD(b2, a2, a2, b2); // x = b, y = a
    if (D.Cmp(Ꮡd) != 0) {
        Ꮡt.Errorf("aliased x = b, y = a GCD(%s, %s, %s, %s): got d = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), D.OrTypedNil(), Ꮡd.OrTypedNil());
    }
    if (Ꮡx != nil && b2.Cmp(Ꮡx) != 0) {
        Ꮡt.Errorf("aliased x = b, y = a GCD(%s, %s, %s, %s): got x = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), b2.OrTypedNil(), Ꮡx.OrTypedNil());
    }
    if (Ꮡy != nil && a2.Cmp(Ꮡy) != 0) {
        Ꮡt.Errorf("aliased x = b, y = a GCD(%s, %s, %s, %s): got y = %s, want %s"u8, Ꮡx.OrTypedNil(), Ꮡy.OrTypedNil(), Ꮡa.OrTypedNil(), Ꮡb.OrTypedNil(), a2.OrTypedNil(), Ꮡy.OrTypedNil());
    }
}

public static void TestGcd(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in gcdTests) {
        var (d, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.d, 0);
        var (x, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.x, 0);
        var (y, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.y, 0);
        var (a, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.a, 0);
        var (b, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.b, 0);
        testGcd(Ꮡt, d, nil, nil, a, b);
        testGcd(Ꮡt, d, x, nil, a, b);
        testGcd(Ꮡt, d, nil, y, a, b);
        testGcd(Ꮡt, d, x, y, a, b);
    }
    {
        var err = quick.Check(checkGcd, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    {
        var err = quick.Check(checkLehmerGcd, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
    {
        var err = quick.Check(checkLehmerExtGcd, nil); if (err != default!) {
            Ꮡt.Error(err);
        }
    }
}

[GoType] internal partial struct intShiftTest {
    internal @string @in;
    internal nuint shift;
    internal @string @out;
}

internal static slice<intShiftTest> rshTests = new intShiftTest[]{
    new("0"u8, 0, "0"u8),
    new("-0"u8, 0, "0"u8),
    new("0"u8, 1, "0"u8),
    new("0"u8, 2, "0"u8),
    new("1"u8, 0, "1"u8),
    new("1"u8, 1, "0"u8),
    new("1"u8, 2, "0"u8),
    new("2"u8, 0, "2"u8),
    new("2"u8, 1, "1"u8),
    new("-1"u8, 0, "-1"u8),
    new("-1"u8, 1, "-1"u8),
    new("-1"u8, 10, "-1"u8),
    new("-100"u8, 2, "-25"u8),
    new("-100"u8, 3, "-13"u8),
    new("-100"u8, 100, "-1"u8),
    new("4294967296"u8, 0, "4294967296"u8),
    new("4294967296"u8, 1, "2147483648"u8),
    new("4294967296"u8, 2, "1073741824"u8),
    new("18446744073709551616"u8, 0, "18446744073709551616"u8),
    new("18446744073709551616"u8, 1, "9223372036854775808"u8),
    new("18446744073709551616"u8, 2, "4611686018427387904"u8),
    new("18446744073709551616"u8, 64, "1"u8),
    new("340282366920938463463374607431768211456"u8, 64, "18446744073709551616"u8),
    new("340282366920938463463374607431768211456"u8, 128, "1"u8)
}.slice();

public static void TestRsh(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in rshTests) {
        var (@in, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, 10);
        var (expected, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@out, 10);
        var @out = @new<global::go.math.big_package.ΔInt>().Rsh(@in, test.shift);
        if (!isNormalized(@out)) {
            Ꮡt.Errorf("#%d: %v is not normalized"u8, i, @out.Value);
        }
        if (@out.Cmp(expected) != 0) {
            Ꮡt.Errorf("#%d: got %s want %s"u8, i, @out.OrTypedNil(), expected.OrTypedNil());
        }
    }
}

public static void TestRshSelf(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in rshTests) {
        var (z, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, 10);
        var (expected, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@out, 10);
        z.Rsh(z, test.shift);
        if (!isNormalized(z)) {
            Ꮡt.Errorf("#%d: %v is not normalized"u8, i, z.Value);
        }
        if (z.Cmp(expected) != 0) {
            Ꮡt.Errorf("#%d: got %s want %s"u8, i, z.OrTypedNil(), expected.OrTypedNil());
        }
    }
}

internal static slice<intShiftTest> lshTests = new intShiftTest[]{
    new("0"u8, 0, "0"u8),
    new("0"u8, 1, "0"u8),
    new("0"u8, 2, "0"u8),
    new("1"u8, 0, "1"u8),
    new("1"u8, 1, "2"u8),
    new("1"u8, 2, "4"u8),
    new("2"u8, 0, "2"u8),
    new("2"u8, 1, "4"u8),
    new("2"u8, 2, "8"u8),
    new("-87"u8, 1, "-174"u8),
    new("4294967296"u8, 0, "4294967296"u8),
    new("4294967296"u8, 1, "8589934592"u8),
    new("4294967296"u8, 2, "17179869184"u8),
    new("18446744073709551616"u8, 0, "18446744073709551616"u8),
    new("9223372036854775808"u8, 1, "18446744073709551616"u8),
    new("4611686018427387904"u8, 2, "18446744073709551616"u8),
    new("1"u8, 64, "18446744073709551616"u8),
    new("18446744073709551616"u8, 64, "340282366920938463463374607431768211456"u8),
    new("1"u8, 128, "340282366920938463463374607431768211456"u8)
}.slice();

public static void TestLsh(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in lshTests) {
        var (@in, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, 10);
        var (expected, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@out, 10);
        var @out = @new<global::go.math.big_package.ΔInt>().Lsh(@in, test.shift);
        if (!isNormalized(@out)) {
            Ꮡt.Errorf("#%d: %v is not normalized"u8, i, @out.Value);
        }
        if (@out.Cmp(expected) != 0) {
            Ꮡt.Errorf("#%d: got %s want %s"u8, i, @out.OrTypedNil(), expected.OrTypedNil());
        }
    }
}

public static void TestLshSelf(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in lshTests) {
        var (z, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, 10);
        var (expected, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@out, 10);
        z.Lsh(z, test.shift);
        if (!isNormalized(z)) {
            Ꮡt.Errorf("#%d: %v is not normalized"u8, i, z.Value);
        }
        if (z.Cmp(expected) != 0) {
            Ꮡt.Errorf("#%d: got %s want %s"u8, i, z.OrTypedNil(), expected.OrTypedNil());
        }
    }
}

public static void TestLshRsh(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in rshTests) {
        var (@in, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, 10);
        var @out = @new<global::go.math.big_package.ΔInt>().Lsh(@in, test.shift);
        @out = @out.Rsh(@out, test.shift);
        if (!isNormalized(@out)) {
            Ꮡt.Errorf("#%d: %v is not normalized"u8, i, @out.Value);
        }
        if (@in.Cmp(@out) != 0) {
            Ꮡt.Errorf("#%d: got %s want %s"u8, i, @out.OrTypedNil(), @in.OrTypedNil());
        }
    }
    foreach (var (i, test) in lshTests) {
        var (@in, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, 10);
        var @out = @new<global::go.math.big_package.ΔInt>().Lsh(@in, test.shift);
        @out.Rsh(@out, test.shift);
        if (!isNormalized(@out)) {
            Ꮡt.Errorf("#%d: %v is not normalized"u8, i, @out.Value);
        }
        if (@in.Cmp(@out) != 0) {
            Ꮡt.Errorf("#%d: got %s want %s"u8, i, @out.OrTypedNil(), @in.OrTypedNil());
        }
    }
}

// Entries must be sorted by value in ascending order.
internal static slice<@string> cmpAbsTests = new @string[]{
    "0"u8,
    "1"u8,
    "2"u8,
    "10"u8,
    "10000000"u8,
    "2783678367462374683678456387645876387564783686583485"u8,
    "2783678367462374683678456387645876387564783686583486"u8,
    "32957394867987420967976567076075976570670947609750670956097509670576075067076027578341538"u8
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly object cmpAbsTestsEntriesNotˢ = (@string)"cmpAbsTests entries not sorted in ascending order"u8;

public static void TestCmpAbs(ж<testing.T> Ꮡt) {
    var values = new slice<ж<global::go.math.big_package.ΔInt>>(len(cmpAbsTests));
    ж<global::go.math.big_package.ΔInt> prev = default!;
    foreach (var (i, s) in cmpAbsTests) {
        var (x, ok) = @new<global::go.math.big_package.ΔInt>().SetString(s, 0);
        if (!ok) {
            Ꮡt.Fatalf("SetString(%s, 0) failed"u8, s);
        }
        if (prev != nil && prev.Cmp(x) >= 0) {
            Ꮡt.Fatal(cmpAbsTestsEntriesNotˢ);
        }
        values[i] = x;
        prev = x;
    }
    foreach (var (i, x) in values) {
        foreach (var (j, y) in values) {
            // try all combinations of signs for x, y
            for (nint k = 0; k < 4; k++) {
                ref var a = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡa);
                ref var b = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡb);
                Ꮡa.Set(x);
                Ꮡb.Set(y);
                if ((nint)(k & 1) != 0) {
                    Ꮡa.Neg(Ꮡa);
                }
                if ((nint)(k & 2) != 0) {
                    Ꮡb.Neg(Ꮡb);
                }
                nint got = a.CmpAbs(Ꮡb);
                nint want = 0;
                switch (ᐧ) {
                case {} when i > j: {
                    want = 1;
                    break;
                }
                case {} when i < j: {
                    want = -1;
                    break;
                }}

                if (got != want) {
                    Ꮡt.Errorf("absCmp |%s|, |%s|: got %d; want %d"u8, Ꮡa, Ꮡb, got, want);
                }
            }
        }
    }
}

public static void TestIntCmpSelf(ж<testing.T> Ꮡt) {
    foreach (var (_, s) in cmpAbsTests) {
        var (x, ok) = @new<global::go.math.big_package.ΔInt>().SetString(s, 0);
        if (!ok) {
            Ꮡt.Fatalf("SetString(%s, 0) failed"u8, s);
        }
        nint got = x.Cmp(x);
        nint want = 0;
        if (got != want) {
            Ꮡt.Errorf("x = %s: x.Cmp(x): got %d; want %d"u8, x.OrTypedNil(), got, want);
        }
    }
}

// int64
// not int64
internal static slice<@string> int64Tests = new @string[]{
    "0"u8,
    "1"u8,
    "-1"u8,
    "4294967295"u8,
    "-4294967295"u8,
    "4294967296"u8,
    "-4294967296"u8,
    "9223372036854775807"u8,
    "-9223372036854775807"u8,
    "-9223372036854775808"u8,
    "0x8000000000000000"u8,
    "-0x8000000000000001"u8,
    "38579843757496759476987459679745"u8,
    "-38579843757496759476987459679745"u8
}.slice();

public static void TestInt64(ж<testing.T> Ꮡt) {
    foreach (var (_, s) in int64Tests) {
        ref var x = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡx);
        var (_, ok) = Ꮡx.SetString(s, 0);
        if (!ok) {
            Ꮡt.Errorf("SetString(%s, 0) failed"u8, s);
            continue;
        }
        var (want, err) = strconv.ParseInt(s, 0, 64);
        if (err != default!) {
            if (AreEqual((~err._<ж<strconv.NumError>>()).Err, strconv.ErrRange)){
                if (x.IsInt64()) {
                    Ꮡt.Errorf("IsInt64(%s) succeeded unexpectedly"u8, s);
                }
            } else {
                Ꮡt.Errorf("ParseInt(%s) failed"u8, s);
            }
            continue;
        }
        if (!x.IsInt64()) {
            Ꮡt.Errorf("IsInt64(%s) failed unexpectedly"u8, s);
        }
        var got = x.Int64();
        if (got != want) {
            Ꮡt.Errorf("Int64(%s) = %d; want %d"u8, s, got, want);
        }
    }
}

// uint64
// not uint64
internal static slice<@string> uint64Tests = new @string[]{
    "0"u8,
    "1"u8,
    "4294967295"u8,
    "4294967296"u8,
    "8589934591"u8,
    "8589934592"u8,
    "9223372036854775807"u8,
    "9223372036854775808"u8,
    "0x08000000000000000"u8,
    "0x10000000000000000"u8,
    "-0x08000000000000000"u8,
    "-1"u8
}.slice();

public static void TestUint64(ж<testing.T> Ꮡt) {
    ref var t = ref Ꮡt.DerefOrNull();

    foreach (var (_, s) in uint64Tests) {
        ref var x = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡx);
        var (_, ok) = Ꮡx.SetString(s, 0);
        if (!ok) {
            Ꮡt.Errorf("SetString(%s, 0) failed"u8, s);
            continue;
        }
        var (want, err) = strconv.ParseUint(s, 0, 64);
        if (err != default!) {
            // check for sign explicitly (ErrRange doesn't cover signed input)
            if (s[0] == (rune)'-' || AreEqual((~err._<ж<strconv.NumError>>()).Err, strconv.ErrRange)){
                if (x.IsUint64()) {
                    Ꮡt.Errorf("IsUint64(%s) succeeded unexpectedly"u8, s);
                }
            } else {
                Ꮡt.Errorf("ParseUint(%s) failed"u8, s);
            }
            continue;
        }
        if (!x.IsUint64()) {
            Ꮡt.Errorf("IsUint64(%s) failed unexpectedly"u8, s);
        }
        var got = x.Uint64();
        if (got != want) {
            Ꮡt.Errorf("Uint64(%s) = %d; want %d"u8, s, got, want);
        }
    }
}


[GoType("dyn")] partial struct bitwiseTestsᴛ1 {
    internal @string x, y;
    internal @string and, or, xor, andNot;
}
internal static slice<bitwiseTestsᴛ1> bitwiseTests = new bitwiseTestsᴛ1[]{
    new("0x00"u8, "0x00"u8, "0x00"u8, "0x00"u8, "0x00"u8, "0x00"u8),
    new("0x00"u8, "0x01"u8, "0x00"u8, "0x01"u8, "0x01"u8, "0x00"u8),
    new("0x01"u8, "0x00"u8, "0x00"u8, "0x01"u8, "0x01"u8, "0x01"u8),
    new("-0x01"u8, "0x00"u8, "0x00"u8, "-0x01"u8, "-0x01"u8, "-0x01"u8),
    new("-0xaf"u8, "-0x50"u8, "-0xf0"u8, "-0x0f"u8, "0xe1"u8, "0x41"u8),
    new("0x00"u8, "-0x01"u8, "0x00"u8, "-0x01"u8, "-0x01"u8, "0x00"u8),
    new("0x01"u8, "0x01"u8, "0x01"u8, "0x01"u8, "0x00"u8, "0x00"u8),
    new("-0x01"u8, "-0x01"u8, "-0x01"u8, "-0x01"u8, "0x00"u8, "0x00"u8),
    new("0x07"u8, "0x08"u8, "0x00"u8, "0x0f"u8, "0x0f"u8, "0x07"u8),
    new("0x05"u8, "0x0f"u8, "0x05"u8, "0x0f"u8, "0x0a"u8, "0x00"u8),
    new("0xff"u8, "-0x0a"u8, "0xf6"u8, "-0x01"u8, "-0xf7"u8, "0x09"u8),
    new("0x013ff6"u8, "0x9a4e"u8, "0x1a46"u8, "0x01bffe"u8, "0x01a5b8"u8, "0x0125b0"u8),
    new("-0x013ff6"u8, "0x9a4e"u8, "0x800a"u8, "-0x0125b2"u8, "-0x01a5bc"u8, "-0x01c000"u8),
    new("-0x013ff6"u8, "-0x9a4e"u8, "-0x01bffe"u8, "-0x1a46"u8, "0x01a5b8"u8, "0x8008"u8),
    new(
        "0x1000009dc6e3d9822cba04129bcbe3401"u8,
        "0xb9bd7d543685789d57cb918e833af352559021483cdb05cc21fd"u8,
        "0x1000001186210100001000009048c2001"u8,
        "0xb9bd7d543685789d57cb918e8bfeff7fddb2ebe87dfbbdfe35fd"u8,
        "0xb9bd7d543685789d57ca918e8ae69d6fcdb2eae87df2b97215fc"u8,
        "0x8c40c2d8822caa04120b8321400"u8
    ),
    new(
        "0x1000009dc6e3d9822cba04129bcbe3401"u8,
        "-0xb9bd7d543685789d57cb918e833af352559021483cdb05cc21fd"u8,
        "0x8c40c2d8822caa04120b8321401"u8,
        "-0xb9bd7d543685789d57ca918e82229142459020483cd2014001fd"u8,
        "-0xb9bd7d543685789d57ca918e8ae69d6fcdb2eae87df2b97215fe"u8,
        "0x1000001186210100001000009048c2000"u8
    ),
    new(
        "-0x1000009dc6e3d9822cba04129bcbe3401"u8,
        "-0xb9bd7d543685789d57cb918e833af352559021483cdb05cc21fd"u8,
        "-0xb9bd7d543685789d57cb918e8bfeff7fddb2ebe87dfbbdfe35fd"u8,
        "-0x1000001186210100001000009048c2001"u8,
        "0xb9bd7d543685789d57ca918e8ae69d6fcdb2eae87df2b97215fc"u8,
        "0xb9bd7d543685789d57ca918e82229142459020483cd2014001fc"u8
    )
}.slice();

// type bitFun is a methodless func type — rendered inline as its base delegate

internal static void testBitFun(ж<testing.T> Ꮡt, @string msg, Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>> f, ж<global::go.math.big_package.ΔInt> Ꮡx, ж<global::go.math.big_package.ΔInt> Ꮡy, @string exp) {
    var expected = @new<global::go.math.big_package.ΔInt>();
    expected.SetString(exp, 0);
    var @out = f(@new<global::go.math.big_package.ΔInt>(), Ꮡx, Ꮡy);
    if (@out.Cmp(expected) != 0) {
        Ꮡt.Errorf("%s: got %s want %s"u8, msg, @out.OrTypedNil(), expected.OrTypedNil());
    }
}

internal static void testBitFunSelf(ж<testing.T> Ꮡt, @string msg, Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>> f, ж<global::go.math.big_package.ΔInt> Ꮡx, ж<global::go.math.big_package.ΔInt> Ꮡy, @string exp) {
    var self = @new<global::go.math.big_package.ΔInt>();
    self.Set(Ꮡx);
    var expected = @new<global::go.math.big_package.ΔInt>();
    expected.SetString(exp, 0);
    self = f(self, self, Ꮡy);
    if (self.Cmp(expected) != 0) {
        Ꮡt.Errorf("%s: got %s want %s"u8, msg, self.OrTypedNil(), expected.OrTypedNil());
    }
}

internal static nuint altBit(ж<global::go.math.big_package.ΔInt> Ꮡx, nint i) {
    var z = @new<global::go.math.big_package.ΔInt>().Rsh(Ꮡx, (nuint)i);
    z = z.And(z, NewInt(1));
    if (z.Cmp(@new<global::go.math.big_package.ΔInt>()) != 0) {
        return 1;
    }
    return 0;
}

internal static ж<global::go.math.big_package.ΔInt> altSetBit(ж<global::go.math.big_package.ΔInt> Ꮡz, ж<global::go.math.big_package.ΔInt> Ꮡx, nint i, nuint b) {
    var one = NewInt(1);
    var m = one.Lsh(one, (nuint)i);
    switch (b) {
    case 1: {
        return Ꮡz.Or(Ꮡx, m);
    }
    case 0: {
        return Ꮡz.AndNot(Ꮡx, m);
    }}

    throw panic("set bit is not 0 or 1");
}

internal static void testBitset(ж<testing.T> Ꮡt, ж<global::go.math.big_package.ΔInt> Ꮡx) {
    ref var x = ref Ꮡx.DerefOrNull();

    nint n = x.BitLen();
    var z = @new<global::go.math.big_package.ΔInt>().Set(Ꮡx);
    var z1 = @new<global::go.math.big_package.ΔInt>().Set(Ꮡx);
    for (nint i = 0; i < n + 10; i++) {
        nuint old = z.Bit(i);
        nuint old1 = altBit(z1, i);
        if (old != old1) {
            Ꮡt.Errorf("bitset: inconsistent value for Bit(%s, %d), got %v want %v"u8, z1.OrTypedNil(), i, old, old1);
        }
        var zΔ1 = @new<global::go.math.big_package.ΔInt>().SetBit(z, i, 1);
        var z1Δ1 = altSetBit(@new<global::go.math.big_package.ΔInt>(), z1, i, 1);
        if (zΔ1.Bit(i) == 0) {
            Ꮡt.Errorf("bitset: bit %d of %s got 0 want 1"u8, i, Ꮡx.OrTypedNil());
        }
        if (zΔ1.Cmp(z1Δ1) != 0) {
            Ꮡt.Errorf("bitset: inconsistent value after SetBit 1, got %s want %s"u8, zΔ1.OrTypedNil(), z1Δ1.OrTypedNil());
        }
        zΔ1.SetBit(zΔ1, i, 0);
        altSetBit(z1Δ1, z1Δ1, i, 0);
        if (zΔ1.Bit(i) != 0) {
            Ꮡt.Errorf("bitset: bit %d of %s got 1 want 0"u8, i, Ꮡx.OrTypedNil());
        }
        if (zΔ1.Cmp(z1Δ1) != 0) {
            Ꮡt.Errorf("bitset: inconsistent value after SetBit 0, got %s want %s"u8, zΔ1.OrTypedNil(), z1Δ1.OrTypedNil());
        }
        altSetBit(z1Δ1, z1Δ1, i, old);
        zΔ1.SetBit(zΔ1, i, old);
        if (zΔ1.Cmp(z1Δ1) != 0) {
            Ꮡt.Errorf("bitset: inconsistent value after SetBit old, got %s want %s"u8, zΔ1.OrTypedNil(), z1Δ1.OrTypedNil());
        }
    }
    if (z.Cmp(Ꮡx) != 0) {
        Ꮡt.Errorf("bitset: got %s want %s"u8, z.OrTypedNil(), Ꮡx.OrTypedNil());
    }
}


[GoType("dyn")] partial struct bitsetTestsᴛ1 {
    internal @string x;
    internal nint i;
    internal nuint b;
}
internal static slice<bitsetTestsᴛ1> bitsetTests = new bitsetTestsᴛ1[]{
    new("0"u8, 0, 0),
    new("0"u8, 200, 0),
    new("1"u8, 0, 1),
    new("1"u8, 1, 0),
    new("-1"u8, 0, 1),
    new("-1"u8, 200, 1),
    new("0x2000000000000000000000000000"u8, 108, 0),
    new("0x2000000000000000000000000000"u8, 109, 1),
    new("0x2000000000000000000000000000"u8, 110, 0),
    new("-0x2000000000000000000000000001"u8, 108, 1),
    new("-0x2000000000000000000000000001"u8, 109, 0),
    new("-0x2000000000000000000000000001"u8, 110, 1)
}.slice();

public static void TestBitSet(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in bitwiseTests) {
        var x = @new<global::go.math.big_package.ΔInt>();
        x.SetString(test.x, 0);
        testBitset(Ꮡt, x);
        x = @new<global::go.math.big_package.ΔInt>();
        x.SetString(test.y, 0);
        testBitset(Ꮡt, x);
    }
    foreach (var (i, test) in bitsetTests) {
        var x = @new<global::go.math.big_package.ΔInt>();
        x.SetString(test.x, 0);
        nuint b = x.Bit(test.i);
        if (b != test.b) {
            Ꮡt.Errorf("#%d got %v want %v"u8, i, b, test.b);
        }
    }
    var z = NewInt(1);
    z.SetBit(NewInt(0), 2, 1);
    if (z.Cmp(NewInt(4)) != 0) {
        Ꮡt.Errorf("destination leaked into result; got %s want 4"u8, z.OrTypedNil());
    }
}


[GoType("dyn")] partial struct tzbTestsᴛ1 {
    internal @string @in;
    internal nuint @out;
}
internal static slice<tzbTestsᴛ1> tzbTests = new tzbTestsᴛ1[]{
    new("0"u8, 0),
    new("1"u8, 0),
    new("-1"u8, 0),
    new("4"u8, 2),
    new("-8"u8, 3),
    new("0x4000000000000000000"u8, 74),
    new("-0x8000000000000000000"u8, 75)
}.slice();

public static void TestTrailingZeroBits(ж<testing.T> Ꮡt) {
    foreach (var (i, test) in tzbTests) {
        var (@in, _) = @new<global::go.math.big_package.ΔInt>().SetString(test.@in, 0);
        nuint want = test.@out;
        nuint got = @in.TrailingZeroBits();
        if (got != want) {
            Ꮡt.Errorf("#%d: got %v want %v"u8, i, got, want);
        }
    }
}

public static void BenchmarkBitset(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var z = @new<global::go.math.big_package.ΔInt>();
    z.SetBit(z, 512, 1);
    b.ResetTimer();
    for (nint i = b.N - 1; i >= 0; i--) {
        z.SetBit(z, (nint)(i & 512), 1);
    }
}

public static void BenchmarkBitsetNeg(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var z = NewInt(-1);
    z.SetBit(z, 512, 0);
    b.ResetTimer();
    for (nint i = b.N - 1; i >= 0; i--) {
        z.SetBit(z, (nint)(i & 512), 0);
    }
}

public static void BenchmarkBitsetOrig(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var z = @new<global::go.math.big_package.ΔInt>();
    altSetBit(z, z, 512, 1);
    b.ResetTimer();
    for (nint i = b.N - 1; i >= 0; i--) {
        altSetBit(z, z, (nint)(i & 512), 1);
    }
}

public static void BenchmarkBitsetNegOrig(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var z = NewInt(-1);
    altSetBit(z, z, 512, 0);
    b.ResetTimer();
    for (nint i = b.N - 1; i >= 0; i--) {
        altSetBit(z, z, (nint)(i & 512), 0);
    }
}

// tri generates the trinomial 2**(n*2) - 2**n - 1, which is always 3 mod 4 and
// 7 mod 8, so that 2 is always a quadratic residue.
internal static ж<global::go.math.big_package.ΔInt> tri(nuint n) {
    var x = NewInt(1);
    x.Lsh(x, n);
    var x2 = @new<global::go.math.big_package.ΔInt>().Lsh(x, n);
    x2.Sub(x2, x);
    x2.Sub(x2, intOne);
    return x2;
}

public static void BenchmarkModSqrt225_Tonelli(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = tri(225);
    var x = NewInt(2);
    for (nint i = 0; i < b.N; i++) {
        x.SetUint64(2);
        x.modSqrtTonelliShanks(x, p);
    }
}

public static void BenchmarkModSqrt225_3Mod4(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = tri(225);
    var x = @new<global::go.math.big_package.ΔInt>().SetUint64(2);
    for (nint i = 0; i < b.N; i++) {
        x.SetUint64(2);
        x.modSqrt3Mod4Prime(x, p);
    }
}

public static void BenchmarkModSqrt231_Tonelli(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = tri(231);
    p.Sub(p, intOne);
    p.Sub(p, intOne); // tri(231) - 2 is a prime == 5 mod 8
    var x = @new<global::go.math.big_package.ΔInt>().SetUint64(7);
    for (nint i = 0; i < b.N; i++) {
        x.SetUint64(7);
        x.modSqrtTonelliShanks(x, p);
    }
}

public static void BenchmarkModSqrt231_5Mod8(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = tri(231);
    p.Sub(p, intOne);
    p.Sub(p, intOne); // tri(231) - 2 is a prime == 5 mod 8
    var x = @new<global::go.math.big_package.ΔInt>().SetUint64(7);
    for (nint i = 0; i < b.N; i++) {
        x.SetUint64(7);
        x.modSqrt5Mod8Prime(x, p);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string andˢ = "and"u8;
internal static readonly @string andNotˢ = "andNot"u8;
internal static readonly @string xorˢ = "xor"u8;

public static void TestBitwise(ж<testing.T> Ꮡt) {
    var x = @new<global::go.math.big_package.ΔInt>();
    var y = @new<global::go.math.big_package.ΔInt>();
    foreach (var (_, test) in bitwiseTests) {
        x.SetString(test.x, 0);
        y.SetString(test.y, 0);
        testBitFun(Ꮡt, andˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(((Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>)(global::go.math.big_package.And))), x, y, test.and);
        testBitFunSelf(Ꮡt, andˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(((Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>)(global::go.math.big_package.And))), x, y, test.and);
        testBitFun(Ꮡt, andNotˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(((Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>)(global::go.math.big_package.AndNot))), x, y, test.andNot);
        testBitFunSelf(Ꮡt, andNotˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(((Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>)(global::go.math.big_package.AndNot))), x, y, test.andNot);
        testBitFun(Ꮡt, "or"u8, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(((Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>)(global::go.math.big_package.Or))), x, y, test.or);
        testBitFunSelf(Ꮡt, "or"u8, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(((Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>)(global::go.math.big_package.Or))), x, y, test.or);
        testBitFun(Ꮡt, xorˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(((Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>)(global::go.math.big_package.Xor))), x, y, test.xor);
        testBitFunSelf(Ꮡt, xorˢ, new Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>(((Func<ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>, ж<global::go.math.big_package.ΔInt>>)(global::go.math.big_package.Xor))), x, y, test.xor);
    }
}


[GoType("dyn")] partial struct notTestsᴛ1 {
    internal @string @in;
    internal @string @out;
}
internal static slice<notTestsᴛ1> notTests = new notTestsᴛ1[]{
    new("0"u8, "-1"u8),
    new("1"u8, "-2"u8),
    new("7"u8, "-8"u8),
    new("0"u8, "-1"u8),
    new("-81910"u8, "81909"u8),
    new(
        "298472983472983471903246121093472394872319615612417471234712061"u8,
        "-298472983472983471903246121093472394872319615612417471234712062"u8
    )
}.slice();

public static void TestNot(ж<testing.T> Ꮡt) {
    var @in = @new<global::go.math.big_package.ΔInt>();
    var @out = @new<global::go.math.big_package.ΔInt>();
    var expected = @new<global::go.math.big_package.ΔInt>();
    foreach (var (i, test) in notTests) {
        @in.SetString(test.@in, 10);
        expected.SetString(test.@out, 10);
        @out = @out.Not(@in);
        if (@out.Cmp(expected) != 0) {
            Ꮡt.Errorf("#%d: got %s want %s"u8, i, @out.OrTypedNil(), expected.OrTypedNil());
        }
        @out = @out.Not(@out);
        if (@out.Cmp(@in) != 0) {
            Ꮡt.Errorf("#%d: got %s want %s"u8, i, @out.OrTypedNil(), @in.OrTypedNil());
        }
    }
}

// issue #16984

[GoType("dyn")] partial struct modInverseTestsᴛ1 {
    internal @string element;
    internal @string modulus;
}
internal static slice<modInverseTestsᴛ1> modInverseTests = new modInverseTestsᴛ1[]{
    new("1234567"u8, "458948883992"u8),
    new("239487239847"u8, "2410312426921032588552076022197566074856950548502459942654116941958108831682612228890093858261341614673227141477904012196503648957050582631942730706805009223062734745341073406696246014589361659774041027169249453200378729434170325843778659198143763193776859869524088940195577346119843545301547043747207749969763750084308926339295559968882457872412993810129130294592999947926365264059284647209730384947211681434464714438488520940127459844288859336526896320919633919"u8),
    new("-10"u8, "13"u8),
    new("10"u8, "-13"u8),
    new("-17"u8, "-13"u8)
}.slice();

public static void TestModInverse(ж<testing.T> Ꮡt) {
    ref var element = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡelement);
    ref var modulus = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡmodulus);
    ref var gcd = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡgcd);
    ref var inverse = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡinverse);
    var one = NewInt(1);
    foreach (var (_, test) in modInverseTests) {
        (Ꮡelement).SetString(test.element, 10);
        (Ꮡmodulus).SetString(test.modulus, 10);
        (Ꮡinverse).ModInverse(Ꮡelement, Ꮡmodulus);
        (Ꮡinverse).Mul(Ꮡinverse, Ꮡelement);
        (Ꮡinverse).Mod(Ꮡinverse, Ꮡmodulus);
        if ((Ꮡinverse).Cmp(one) != 0) {
            Ꮡt.Errorf("ModInverse(%d,%d)*%d%%%d=%d, not 1"u8, Ꮡelement, Ꮡmodulus, Ꮡelement, Ꮡmodulus, Ꮡinverse);
        }
    }
    // exhaustive test for small values
    for (nint n = 2; n < 100; n++) {
        (Ꮡmodulus).SetInt64((int64)n);
        for (nint x = 1; x < n; x++) {
            (Ꮡelement).SetInt64((int64)x);
            (Ꮡgcd).GCD(nil, nil, Ꮡelement, Ꮡmodulus);
            if ((Ꮡgcd).Cmp(one) != 0) {
                continue;
            }
            (Ꮡinverse).ModInverse(Ꮡelement, Ꮡmodulus);
            (Ꮡinverse).Mul(Ꮡinverse, Ꮡelement);
            (Ꮡinverse).Mod(Ꮡinverse, Ꮡmodulus);
            if ((Ꮡinverse).Cmp(one) != 0) {
                Ꮡt.Errorf("ModInverse(%d,%d)*%d%%%d=%d, not 1"u8, Ꮡelement, Ꮡmodulus, Ꮡelement, Ꮡmodulus, Ꮡinverse);
            }
        }
    }
}

public static void BenchmarkModInverse(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var p = @new<global::go.math.big_package.ΔInt>().SetInt64(1); // Mersenne prime 2**1279 -1
    p.Value.abs = (~p).abs.shl((~p).abs, 1279);
    p.Sub(p, intOne);
    var x = @new<global::go.math.big_package.ΔInt>().Sub(p, intOne);
    var z = @new<global::go.math.big_package.ΔInt>();
    for (nint i = 0; i < b.N; i++) {
        z.ModInverse(x, p);
    }
}

// testModSqrt is a helper for TestModSqrt,
// which checks that ModSqrt can compute a square-root of elt^2.
internal static bool testModSqrt(ж<testing.T> Ꮡt, ж<global::go.math.big_package.ΔInt> Ꮡelt, ж<global::go.math.big_package.ΔInt> Ꮡmod, ж<global::go.math.big_package.ΔInt> Ꮡsq, ж<global::go.math.big_package.ΔInt> Ꮡsqrt) {
    ref var mod = ref Ꮡmod.DerefOrNull();

    ref var sqChk = ref heap(new global::go.math.big_package.ΔInt(), out var ᏑsqChk);
    ref var sqrtChk = ref heap(new global::go.math.big_package.ΔInt(), out var ᏑsqrtChk);
    ref var sqrtsq = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡsqrtsq);
    Ꮡsq.Mul(Ꮡelt, Ꮡelt);
    Ꮡsq.Mod(Ꮡsq, Ꮡmod);
    var z = Ꮡsqrt.ModSqrt(Ꮡsq, Ꮡmod);
    if (z != Ꮡsqrt) {
        Ꮡt.Errorf("ModSqrt returned wrong value %s"u8, z.OrTypedNil());
    }
    // test ModSqrt arguments outside the range [0,mod)
    ᏑsqChk.Add(Ꮡsq, Ꮡmod);
    z = ᏑsqrtChk.ModSqrt(ᏑsqChk, Ꮡmod);
    if (z != ᏑsqrtChk || z.Cmp(Ꮡsqrt) != 0) {
        Ꮡt.Errorf("ModSqrt returned inconsistent value %s"u8, z.OrTypedNil());
    }
    ᏑsqChk.Sub(Ꮡsq, Ꮡmod);
    z = ᏑsqrtChk.ModSqrt(ᏑsqChk, Ꮡmod);
    if (z != ᏑsqrtChk || z.Cmp(Ꮡsqrt) != 0) {
        Ꮡt.Errorf("ModSqrt returned inconsistent value %s"u8, z.OrTypedNil());
    }
    // test x aliasing z
    z = ᏑsqrtChk.ModSqrt(ᏑsqrtChk.Set(Ꮡsq), Ꮡmod);
    if (z != ᏑsqrtChk || z.Cmp(Ꮡsqrt) != 0) {
        Ꮡt.Errorf("ModSqrt returned inconsistent value %s"u8, z.OrTypedNil());
    }
    // make sure we actually got a square root
    if (Ꮡsqrt.Cmp(Ꮡelt) == 0) {
        return true; // we found the "desired" square root
    }
    Ꮡsqrtsq.Mul(Ꮡsqrt, Ꮡsqrt); // make sure we found the "other" one
    Ꮡsqrtsq.Mod(Ꮡsqrtsq, Ꮡmod);
    return Ꮡsq.Cmp(Ꮡsqrtsq) == 0;
}

public static void TestModSqrt(ж<testing.T> Ꮡt) {
    ref var elt = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡelt);
    ref var mod = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡmod);
    ref var modx4 = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡmodx4);
    ref var sq = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡsq);
    ref var sqrt = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡsqrt);
    var r = Δrand.New(Δrand.NewSource(9));
    foreach (var (i, s) in primes[1..]) {
        // skip 2, use only odd primes
        Ꮡmod.SetString(s, 10);
        Ꮡmodx4.Lsh(Ꮡmod, 2);
        // test a few random elements per prime
        for (nint x = 1; x < 5; x++) {
            Ꮡelt.Rand(r, Ꮡmodx4);
            Ꮡelt.Sub(Ꮡelt, Ꮡmod); // test range [-mod, 3*mod)
            if (!testModSqrt(Ꮡt, Ꮡelt, Ꮡmod, Ꮡsq, Ꮡsqrt)) {
                Ꮡt.Errorf("#%d: failed (sqrt(e) = %s)"u8, i, Ꮡsqrt);
            }
        }
        if (testing.Short() && i > 2) {
            break;
        }
    }
    if (testing.Short()) {
        return;
    }
    // exhaustive test for small values
    for (nint n = 3; n < 100; n++) {
        Ꮡmod.SetInt64((int64)n);
        if (!mod.ProbablyPrime(10)) {
            continue;
        }
        var isSquare = new slice<bool>(n);
        // test all the squares
        for (nint x = 1; x < n; x++) {
            Ꮡelt.SetInt64((int64)x);
            if (!testModSqrt(Ꮡt, Ꮡelt, Ꮡmod, Ꮡsq, Ꮡsqrt)) {
                Ꮡt.Errorf("#%d: failed (sqrt(%d,%d) = %s)"u8, x, Ꮡelt, Ꮡmod, Ꮡsqrt);
            }
            isSquare[sq.Uint64()] = true;
        }
        // test all non-squares
        for (nint x = 1; x < n; x++) {
            Ꮡsq.SetInt64((int64)x);
            var z = Ꮡsqrt.ModSqrt(Ꮡsq, Ꮡmod);
            if (!isSquare[x] && z != nil) {
                Ꮡt.Errorf("#%d: failed (sqrt(%d,%d) = nil)"u8, x, Ꮡsqrt, Ꮡmod);
            }
        }
    }
}

[GoType("dyn")] internal partial struct TestJacobi_testCases {
    internal int64 x, y;
    internal nint result;
}

public static void TestJacobi(ж<testing.T> Ꮡt) {
    var testCases = new TestJacobi_testCases[]{
        new(0, 1, 1),
        new(0, -1, 1),
        new(1, 1, 1),
        new(1, -1, 1),
        new(0, 5, 0),
        new(1, 5, 1),
        new(2, 5, -1),
        new(-2, 5, -1),
        new(2, -5, -1),
        new(-2, -5, 1),
        new(3, 5, -1),
        new(5, 5, 0),
        new(-5, 5, 0),
        new(6, 5, 1),
        new(6, -5, 1),
        new(-6, 5, 1),
        new(-6, -5, -1)
    }.slice();
    ref var x = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡx);
    ref var y = ref heap(new global::go.math.big_package.ΔInt(), out var Ꮡy);
    foreach (var (i, test) in testCases) {
        Ꮡx.SetInt64(test.x);
        Ꮡy.SetInt64(test.y);
        nint expected = test.result;
        nint actual = Jacobi(Ꮡx, Ꮡy);
        if (actual != expected) {
            Ꮡt.Errorf("#%d: Jacobi(%d, %d) = %d, but expected %d"u8, i, test.x, test.y, actual, expected);
        }
    }
}

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string failureMsgᶜ = "test failure"u8;

public static void TestJacobiPanic(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        @string failureMsg = failureMsgᶜ;
        defer(() => {
            var msg = recover();
            if (msg == default! || AreEqual(msg, failureMsg)) {
                throw panic(msg);
            }
            Ꮡt.Log(msg);
        }, ref ᒐ);
        var x = NewInt(1);
        var y = NewInt(2);
        // Jacobi should panic when the second argument is even.
        Jacobi(x, y);
        throw panic(failureMsg);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestIssue2607(ж<testing.T> Ꮡt) {
    // This code sequence used to hang.
    var n = NewInt(10);
    n.Rand(Δrand.New(Δrand.NewSource(9)), n);
}

public static void TestSqrt(ж<testing.T> Ꮡt) {
    nint root = 0;
    var r = @new<global::go.math.big_package.ΔInt>();
    for (nint i = 0; i < 10000; i++) {
        if ((root + 1) * (root + 1) <= i) {
            root++;
        }
        var n = NewInt((int64)i);
        r.SetInt64(-2);
        r.Sqrt(n);
        if (r.Cmp(NewInt((int64)root)) != 0) {
            Ꮡt.Errorf("Sqrt(%v) = %v, want %v"u8, n.OrTypedNil(), r.OrTypedNil(), root);
        }
    }
    for (nint i = 0; i < 1000; i += 10) {
        var (n, _) = @new<global::go.math.big_package.ΔInt>().SetString("1"u8 + strings.Repeat("0"u8, i), 10);
        var rΔ1 = @new<global::go.math.big_package.ΔInt>().Sqrt(n);
        var (rootΔ1, _) = @new<global::go.math.big_package.ΔInt>().SetString("1"u8 + strings.Repeat("0"u8, i / 2), 10);
        if (rΔ1.Cmp(rootΔ1) != 0) {
            Ꮡt.Errorf("Sqrt(1e%d) = %v, want 1e%d"u8, i, rΔ1.OrTypedNil(), i / 2);
        }
    }
    // Test aliasing.
    r.SetInt64(100);
    r.Sqrt(r);
    if (r.Int64() != 10) {
        Ꮡt.Errorf("Sqrt(100) = %v, want 10 (aliased output)"u8, r.Int64());
    }
}

// We can't test this together with the other Exp tests above because
// it requires a different receiver setup.
public static void TestIssue22830(ж<testing.T> Ꮡt) {
    var one = @new<global::go.math.big_package.ΔInt>().SetInt64(1);
    var (@base, _) = @new<global::go.math.big_package.ΔInt>().SetString("84555555300000000000"u8, 10);
    var (mod, _) = @new<global::go.math.big_package.ΔInt>().SetString("66666670001111111111"u8, 10);
    var (want, _) = @new<global::go.math.big_package.ΔInt>().SetString("17888885298888888889"u8, 10);
    slice<int64> tests = new int64[]{
        0, 1, -1
    }.slice();
    foreach (var (_, n) in tests) {
        var m = NewInt(n);
        {
            var got = m.Exp(@base, one, mod); if (got.Cmp(want) != 0) {
                Ꮡt.Errorf("(%v).Exp(%s, 1, %s) = %s, want %s"u8, n, @base.OrTypedNil(), mod.OrTypedNil(), got.OrTypedNil(), want.OrTypedNil());
            }
        }
    }
}

public static void BenchmarkSqrt(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (n, _) = @new<global::go.math.big_package.ΔInt>().SetString("1"u8 + strings.Repeat("0"u8, 1001), 10);
    b.ResetTimer();
    var t = @new<global::go.math.big_package.ΔInt>();
    for (nint i = 0; i < b.N; i++) {
        t.Sqrt(n);
    }
}

internal static void benchmarkIntSqr(ж<testing.B> Ꮡb, nint nwords) {
    ref var b = ref Ꮡb.DerefOrNull();

    var x = @new<global::go.math.big_package.ΔInt>();
    x.Value.abs = rndNat(nwords);
    var t = @new<global::go.math.big_package.ΔInt>();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        t.Mul(x, x);
    }
}

public static void BenchmarkIntSqr(ж<testing.B> Ꮡb) {
    foreach (var (_, n) in sqrBenchSizes) {
        if (isRaceBuilder && n > 1000) {
            continue;
        }
        Ꮡb.Run(fmt.Sprintf("%d"u8, n), (ж<testing.B> bΔ1) => {
            benchmarkIntSqr(bΔ1, n);
        });
    }
}

internal static void benchmarkDiv(ж<testing.B> Ꮡb, nint aSize, nint bSize) {
    ref var b = ref Ꮡb.DerefOrNull();

    ж<Δrand.Rand> r = Δrand.New(Δrand.NewSource(1234));
    var aa = randInt(r, (nuint)aSize);
    var bb = randInt(r, (nuint)bSize);
    if (aa.Cmp(bb) < 0) {
        (aa, bb) = (bb, aa);
    }
    var x = @new<global::go.math.big_package.ΔInt>();
    var y = @new<global::go.math.big_package.ΔInt>();
    b.ResetTimer();
    for (nint i = 0; i < b.N; i++) {
        x.DivMod(aa, bb, y);
    }
}

public static void BenchmarkDiv(ж<testing.B> Ꮡb) {
    var sizes = new nint[]{
        10, 20, 50, 100, 200, 500, 1000,
        10000, 100000, 1000000, 10000000
    }.slice();
    foreach (var (_, i) in sizes) {
        nint j = 2 * i;
        Ꮡb.Run(fmt.Sprintf("%d/%d"u8, j, i), (ж<testing.B> bΔ1) => {
            benchmarkDiv(bΔ1, j, i);
        });
    }
}

public static void TestFillBytes(ж<testing.T> Ꮡt) {
    void checkResult(ж<testing.T> tΔ1, slice<byte> buf, ж<global::go.math.big_package.ΔInt> want) {
        tΔ1.Helper();
        var got = @new<global::go.math.big_package.ΔInt>().SetBytes(buf);
        if (got.CmpAbs(want) != 0) {
            tΔ1.Errorf("got 0x%x, want 0x%x: %x"u8, got.OrTypedNil(), want.OrTypedNil(), buf);
        }
    }
    bool /*panic*/ panics(Action f) {
        bool panicΔ1 = default!;
        GoFrame ᒐ = default;
        try {
            defer(() => {
                panicΔ1 = recover() != default!;
            }, ref ᒐ);
            f();
            goto ᒐdone;
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
        ᒐdone: return panicΔ1;
    }
    foreach (var (_, n) in new @string[]{
        "0"u8,
        "1000"u8,
        "0xffffffff"u8,
        "-0xffffffff"u8,
        "0xffffffffffffffff"u8,
        "0x10000000000000000"u8,
        "0xabababababababababababababababababababababababababa"u8,
        "0xffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"u8
    }.slice()) {
        var checkResultʗ1 = checkResult;
        var panicsʗ1 = panics;
        Ꮡt.Run(n, (ж<testing.T> tΔ2) => {
            tΔ2.Log(n);
            var (x, ok) = @new<global::go.math.big_package.ΔInt>().SetString(n, 0);
            if (!ok) {
                throw panic("invalid test entry");
            }
            // Perfectly sized buffer.
            nint byteLen = (x.BitLen() + 7) / 8;
            ref var buf = ref heap<slice<byte>>(out var Ꮡbuf);
            Ꮡbuf.ValueSlot = new slice<byte>(byteLen);
            checkResultʗ1(tΔ2, x.FillBytes(Ꮡbuf.ValueSlot), x);
            // Way larger, checking all bytes get zeroed.
            Ꮡbuf.ValueSlot = new slice<byte>(100);
            foreach (var (i, _) in Ꮡbuf.ValueSlot) {
                Ꮡbuf.ValueSlot[i] = 0xff;
            }
            checkResultʗ1(tΔ2, x.FillBytes(Ꮡbuf.ValueSlot), x);
            // Too small.
            if (byteLen > 0) {
                Ꮡbuf.ValueSlot = new slice<byte>(byteLen - 1);
                var xʗ1 = x;
                if (!panicsʗ1(() => {
                    xʗ1.FillBytes(Ꮡbuf.ValueSlot);
                })) {
                    tΔ2.Errorf("expected panic for small buffer and value %x"u8, x.OrTypedNil());
                }
            }
        });
    }
}

public static void TestNewIntMinInt64(ж<testing.T> Ꮡt) {
    // Test for uint64 cast in NewInt.
    var want = (int64)math.MinInt64;
    {
        var got = NewInt(want).Int64(); if (got != want) {
            Ꮡt.Fatalf("wanted %d, got %d"u8, want, got);
        }
    }
}

public static void TestNewIntAllocs(ж<testing.T> Ꮡt) {
    testenv.SkipIfOptimizationOff(new big_test_package.testing_TжTB(Ꮡt));
    foreach (var (_, n) in new int64[]{0, 7, -7, ((int64)1 << (int)(30)), ((int64)(-1) << (int)(30)), 1125899906842624L, -1125899906842624L}.slice()) {
        var x = NewInt(3);
        var xʗ1 = x;
        var got = testing.AllocsPerRun(100, () => {
            // NewInt should inline, and all its allocations
            // can happen on the stack. Passing the result of NewInt
            // to Add should not cause any of those allocations to escape.
            xʗ1.Add(xʗ1, NewInt(n));
        });
        if (got != 0D) {
            Ꮡt.Errorf("x.Add(x, NewInt(%d)), wanted 0 allocations, got %f"u8, n, got);
        }
    }
}

[GoType("dyn")] internal partial struct TestFloat64_type {
    internal @string istr;
    internal float64 f;
    internal global::go.math.big_package.Accuracy acc;
}

public static void TestFloat64(ж<testing.T> Ꮡt) {
    foreach (var (_, test) in new TestFloat64_type[]{
        new("-1000000000000000000000000000000000000000000000000000000"u8, -1000000000000000078291540404596243842305360299886116864.000000D, Below),
        new("-9223372036854775809"u8, math.MinInt64, Above),
        new("-9223372036854775808"u8, -9223372036854775808D, Exact), // -2^63

        new("-9223372036854775807"u8, -9223372036854775807D, Below),
        new("-18014398509481985"u8, -18014398509481984.000000D, Above),
        new("-18014398509481984"u8, -18014398509481984.000000D, Exact), // -2^54

        new("-18014398509481983"u8, -18014398509481984.000000D, Below),
        new("-9007199254740993"u8, -9007199254740992.000000D, Above),
        new("-9007199254740992"u8, -9007199254740992.000000D, Exact), // -2^53

        new("-9007199254740991"u8, -9007199254740991.000000D, Exact),
        new("-4503599627370497"u8, -4503599627370497.000000D, Exact),
        new("-4503599627370496"u8, -4503599627370496.000000D, Exact), // -2^52

        new("-4503599627370495"u8, -4503599627370495.000000D, Exact),
        new("-12345"u8, -12345D, Exact),
        new("-1"u8, -1D, Exact),
        new("0"u8, 0D, Exact),
        new("1"u8, 1D, Exact),
        new("12345"u8, 12345D, Exact),
        new("0x1010000000000000"u8, 1157425104234217472D, Exact), // >2^53 but exact nonetheless

        new("9223372036854775807"u8, 9223372036854775808D, Above),
        new("9223372036854775808"u8, 9223372036854775808D, Exact), // +2^63

        new("1000000000000000000000000000000000000000000000000000000"u8, 1000000000000000078291540404596243842305360299886116864.000000D, Above)
    }.slice()) {
        var (i, ok) = @new<global::go.math.big_package.ΔInt>().SetString(test.istr, 0);
        if (!ok) {
            Ꮡt.Errorf("SetString(%s) failed"u8, test.istr);
            continue;
        }
        // Test against expectation.
        var (f, acc) = i.Float64();
        if (f != test.f || acc != test.acc) {
            Ꮡt.Errorf("%s: got %f (%s); want %f (%s)"u8, test.istr, f, acc, test.f, test.acc);
        }
        // Cross-check the fast path against the big.Float implementation.
        var (f2, acc2) = @new<global::go.math.big_package.Float>().SetInt(i).Float64();
        if (f != f2 || acc != acc2) {
            Ꮡt.Errorf("%s: got %f (%s); Float.Float64 gives %f (%s)"u8, test.istr, f, acc, f2, acc2);
        }
    }
}

} // end big_internal_test_package
