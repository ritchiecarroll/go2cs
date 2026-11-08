// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.math;

using math = math_package;
using testing = testing_package;
using static go.math.big_package;

partial class big_internal_test_package {

public static void TestZeroRat(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        ref var x = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡx);
        ref var y = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡy);
        ref var z = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡz);
        Ꮡy.SetFrac64(0, 42);

        if (Ꮡx.Cmp(Ꮡy) != 0) {
            t.Errorf("x and y should be both equal and zero");
        }

        {
            @string s = Ꮡx.String(); if (s != "0/1"u8) {
                t.Errorf("got x = %s, want 0/1", s);
            }
        }

        {
            @string s = Ꮡx.RatString(); if (s != "0"u8) {
                t.Errorf("got x = %s, want 0", s);
            }
        }

        Ꮡz.Add(Ꮡx, Ꮡy);
        {
            @string s = Ꮡz.RatString(); if (s != "0"u8) {
                t.Errorf("got x+y = %s, want 0", s);
            }
        }

        Ꮡz.Sub(Ꮡx, Ꮡy);
        {
            @string s = Ꮡz.RatString(); if (s != "0"u8) {
                t.Errorf("got x-y = %s, want 0", s);
            }
        }

        Ꮡz.Mul(Ꮡx, Ꮡy);
        {
            @string s = Ꮡz.RatString(); if (s != "0"u8) {
                t.Errorf("got x*y = %s, want 0", s);
            }
        }

        // check for division by zero
        defer(() => {
            var s = recover(); if (s == nil || s._<@string>() != "division by zero"u8) {
                throw panic(s);
            }
        }, ref ᒐ);
        Ꮡz.Quo(Ꮡx, Ꮡy);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

public static void TestRatSign(ж<testing.T> t) {
    var zero = NewRat(0, 1);
    foreach (var (_, a) in range(setStringTests)) {
        ref var x = ref heap<ж<global::go.math.big_package.ΔRat>>(out var Ꮡx);
        (x, var ok) = @new<global::go.math.big_package.ΔRat>().SetString(a.@in);
        if (!ok) {
            continue;
        }
        nint s = x.Sign();
        nint e = x.Cmp(zero);
        if (s != e) {
            t.Errorf("got %d; want %d for z = %v", s, e, Ꮡx);
        }
    }
}

partial struct ratCmpTestsᴛ1 /*dyn*/ {
    internal @string rat1, rat2;
    internal nint @out;
}
internal static slice<ratCmpTestsᴛ1> ratCmpTests = [
    new("0", "0/1", 0),
    new("1/1", "1", 0),
    new("-1", "-2/2", 0),
    new("1", "0", 1),
    new("0/1", "1/1", -1),
    new("-5/1434770811533343057144", "-5/1434770811533343057145", -1),
    new("49832350382626108453/8964749413", "49832350382626108454/8964749413", -1),
    new("-37414950961700930/7204075375675961", "37414950961700930/7204075375675961", -1),
    new("37414950961700930/7204075375675961", "74829901923401860/14408150751351922", 0)
];

public static void TestRatCmp(ж<testing.T> t) {
    foreach (var (i, test) in range(ratCmpTests)) {
        var (x, _) = @new<global::go.math.big_package.ΔRat>().SetString(test.rat1);
        var (y, _) = @new<global::go.math.big_package.ΔRat>().SetString(test.rat2);

        nint @out = x.Cmp(y);
        if (@out != test.@out) {
            t.Errorf("#%d got out = %v; want %v", i, @out, test.@out);
        }
    }
}

public static void TestIsInt(ж<testing.T> t) {
    var one = NewInt(1);
    foreach (var (_, a) in range(setStringTests)) {
        var (x, ok) = @new<global::go.math.big_package.ΔRat>().SetString(a.@in);
        if (!ok) {
            continue;
        }
        var i = x.IsInt();
        var e = x.Denom().Cmp(one) == 0;
        if (i != e) {
            t.Errorf("got IsInt(%v) == %v; want %v", x.OrTypedNil(), i, e);
        }
    }
}

public static void TestRatAbs(ж<testing.T> t) {
    var zero = @new<global::go.math.big_package.ΔRat>();
    foreach (var (_, a) in range(setStringTests)) {
        var (x, ok) = @new<global::go.math.big_package.ΔRat>().SetString(a.@in);
        if (!ok) {
            continue;
        }
        var e = @new<global::go.math.big_package.ΔRat>().Set(x);
        if (e.Cmp(zero) < 0) {
            e.Sub(zero, e);
        }
        var z = @new<global::go.math.big_package.ΔRat>().Abs(x);
        if (z.Cmp(e) != 0) {
            t.Errorf("got Abs(%v) = %v; want %v", x.OrTypedNil(), z.OrTypedNil(), e.OrTypedNil());
        }
    }
}

public static void TestRatNeg(ж<testing.T> t) {
    var zero = @new<global::go.math.big_package.ΔRat>();
    foreach (var (_, a) in range(setStringTests)) {
        var (x, ok) = @new<global::go.math.big_package.ΔRat>().SetString(a.@in);
        if (!ok) {
            continue;
        }
        var e = @new<global::go.math.big_package.ΔRat>().Sub(zero, x);
        var z = @new<global::go.math.big_package.ΔRat>().Neg(x);
        if (z.Cmp(e) != 0) {
            t.Errorf("got Neg(%v) = %v; want %v", x.OrTypedNil(), z.OrTypedNil(), e.OrTypedNil());
        }
    }
}

public static void TestRatInv(ж<testing.T> t) {
    var zero = @new<global::go.math.big_package.ΔRat>();
    foreach (var (_, a) in range(setStringTests)) {
        var (x, ok) = @new<global::go.math.big_package.ΔRat>().SetString(a.@in);
        if (!ok) {
            continue;
        }
        if (x.Cmp(zero) == 0) {
            continue; // avoid division by zero
        }
        var e = @new<global::go.math.big_package.ΔRat>().SetFrac(x.Denom(), x.Num());
        var z = @new<global::go.math.big_package.ΔRat>().Inv(x);
        if (z.Cmp(e) != 0) {
            t.Errorf("got Inv(%v) = %v; want %v", x.OrTypedNil(), z.OrTypedNil(), e.OrTypedNil());
        }
    }
}

// type ratBinFun is a methodless func type — rendered inline as its base delegate
internal partial struct ratBinArg {
    internal @string x, y, z;
}

internal static void testRatBin(ж<testing.T> t, nint i, @string name, Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>> f, ratBinArg a) {
    var (x, _) = @new<global::go.math.big_package.ΔRat>().SetString(a.x);
    var (y, _) = @new<global::go.math.big_package.ΔRat>().SetString(a.y);
    var (z, _) = @new<global::go.math.big_package.ΔRat>().SetString(a.z);
    var @out = f(@new<global::go.math.big_package.ΔRat>(), x, y);

    if (@out.Cmp(z) != 0) {
        t.Errorf("%s #%d got %s want %s", name, i, @out.OrTypedNil(), z.OrTypedNil());
    }
}

partial struct ratBinTestsᴛ1 /*dyn*/ {
    internal @string x, y;
    internal @string sum, prod;
}
internal static slice<ratBinTestsᴛ1> ratBinTests = [
    new("0", "0", "0", "0"),
    new("0", "1", "1", "0"),
    new("-1", "0", "-1", "0"),
    new("-1", "1", "0", "-1"),
    new("1", "1", "2", "1"),
    new("1/2", "1/2", "1", "1/4"),
    new("1/4", "1/3", "7/12", "1/12"),
    new("2/5", "-14/3", "-64/15", "-28/15"),
    new("4707/49292519774798173060", "-3367/70976135186689855734", "84058377121001851123459/1749296273614329067191168098769082663020", "-1760941/388732505247628681598037355282018369560"),
    new("-61204110018146728334/3", "-31052192278051565633/2", "-215564796870448153567/6", "950260896245257153059642991192710872711/3"),
    new("-854857841473707320655/4237645934602118692642972629634714039", "-18/31750379913563777419", "-27/133467566250814981", "15387441146526731771790/134546868362786310073779084329032722548987800600710485341"),
    new("618575745270541348005638912139/19198433543745179392300736", "-19948846211000086/637313996471", "27674141753240653/30123979153216", "-6169936206128396568797607742807090270137721977/6117715203873571641674006593837351328"),
    new("-3/26206484091896184128", "5/2848423294177090248", "15310893822118706237/9330894968229805033368778458685147968", "-5/24882386581946146755650075889827061248"),
    new("26946729/330400702820", "41563965/225583428284", "1238218672302860271/4658307703098666660055", "224002580204097/14906584649915733312176"),
    new("-8259900599013409474/7", "-84829337473700364773/56707961321161574960", "-468402123685491748914621885145127724451/396955729248131024720", "350340947706464153265156004876107029701/198477864624065512360"),
    new("575775209696864/1320203974639986246357", "29/712593081308", "410331716733912717985762465/940768218243776489278275419794956", "808/45524274987585732633"),
    new("1786597389946320496771/2066653520653241", "6269770/1992362624741777", "3559549865190272133656109052308126637/4117523232840525481453983149257", "8967230/3296219033"),
    new("-36459180403360509753/32150500941194292113930", "9381566963714/9633539", "301622077145533298008420642898530153/309723104686531919656937098270", "-3784609207827/3426986245")
];

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string addˢ2 => ref addˢ2ᵇ;
private static readonly @string addˢ2ᵇ = "Add"u8;
internal static ref readonly @string addSymmetricˢ => ref addSymmetricˢᵇ;
private static readonly @string addSymmetricˢᵇ = "Add symmetric"u8;
internal static ref readonly @string subˢ2 => ref subˢ2ᵇ;
private static readonly @string subˢ2ᵇ = "Sub"u8;
internal static ref readonly @string subSymmetricˢ => ref subSymmetricˢᵇ;
private static readonly @string subSymmetricˢᵇ = "Sub symmetric"u8;
internal static ref readonly @string mulSymmetricˢ => ref mulSymmetricˢᵇ;
private static readonly @string mulSymmetricˢᵇ = "Mul symmetric"u8;
internal static ref readonly @string quoˢ => ref quoˢᵇ;
private static readonly @string quoˢᵇ = "Quo"u8;
internal static ref readonly @string quoSymmetricˢ => ref quoSymmetricˢᵇ;
private static readonly @string quoSymmetricˢᵇ = "Quo symmetric"u8;

public static void TestRatBin(ж<testing.T> t) {
    foreach (var (i, test) in range(ratBinTests)) {
        var arg = new ratBinArg(test.x, test.y, test.sum);
        testRatBin(t, i, addˢ2, new Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>(((Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>)(global::go.math.big_package.Add))), arg);

        arg = new ratBinArg(test.y, test.x, test.sum);
        testRatBin(t, i, addSymmetricˢ, new Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>(((Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>)(global::go.math.big_package.Add))), arg);

        arg = new ratBinArg(test.sum, test.x, test.y);
        testRatBin(t, i, subˢ2, new Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>(((Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>)(global::go.math.big_package.Sub))), arg);

        arg = new ratBinArg(test.sum, test.y, test.x);
        testRatBin(t, i, subSymmetricˢ, new Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>(((Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>)(global::go.math.big_package.Sub))), arg);

        arg = new ratBinArg(test.x, test.y, test.prod);
        testRatBin(t, i, mulˢ2, new Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>(((Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>)(global::go.math.big_package.Mul))), arg);

        arg = new ratBinArg(test.y, test.x, test.prod);
        testRatBin(t, i, mulSymmetricˢ, new Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>(((Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>)(global::go.math.big_package.Mul))), arg);

        if (test.x != "0"u8) {
            arg = new ratBinArg(test.prod, test.x, test.y);
            testRatBin(t, i, quoˢ, new Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>(((Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>)(global::go.math.big_package.Quo))), arg);
        }

        if (test.y != "0"u8) {
            arg = new ratBinArg(test.prod, test.y, test.x);
            testRatBin(t, i, quoSymmetricˢ, new Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>(((Func<ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>, ж<global::go.math.big_package.ΔRat>>)(global::go.math.big_package.Quo))), arg);
        }
    }
}

public static void TestIssue820(ж<testing.T> t) {
    var x = NewRat(3, 1);
    var y = NewRat(2, 1);
    var z = y.Quo(x, y);
    var q = NewRat(3, 2);
    if (z.Cmp(q) != 0) {
        t.Errorf("got %s want %s", z.OrTypedNil(), q.OrTypedNil());
    }

    y = NewRat(3, 1);
    x = NewRat(2, 1);
    z = y.Quo(x, y);
    q = NewRat(2, 3);
    if (z.Cmp(q) != 0) {
        t.Errorf("got %s want %s", z.OrTypedNil(), q.OrTypedNil());
    }

    x = NewRat(3, 1);
    z = x.Quo(x, x);
    q = NewRat(3, 3);
    if (z.Cmp(q) != 0) {
        t.Errorf("got %s want %s", z.OrTypedNil(), q.OrTypedNil());
    }
}

partial struct setFrac64Testsᴛ1 /*dyn*/ {
    internal int64 a, b;
    internal @string @out;
}
internal static slice<setFrac64Testsᴛ1> setFrac64Tests = [
    new(0, 1, "0"),
    new(0, -1, "0"),
    new(1, 1, "1"),
    new(-1, 1, "-1"),
    new(1, -1, "-1"),
    new(-1, -1, "1"),
    new(-9223372036854775808L, -9223372036854775808L, "1")
];

public static void TestRatSetFrac64Rat(ж<testing.T> t) {
    foreach (var (i, test) in range(setFrac64Tests)) {
        var x = @new<global::go.math.big_package.ΔRat>().SetFrac64(test.a, test.b);
        if (x.RatString() != test.@out) {
            t.Errorf("#%d got %s want %s", i, x.RatString(), test.@out);
        }
    }
}

public static void TestIssue2379(ж<testing.T> t) {
    // 1) no aliasing
    var q = NewRat(3, 2);
    var x = @new<global::go.math.big_package.ΔRat>();
    x.SetFrac(NewInt(3), NewInt(2));
    if (x.Cmp(q) != 0) {
        t.Errorf("1) got %s want %s", x.OrTypedNil(), q.OrTypedNil());
    }

    // 2) aliasing of numerator
    x = NewRat(2, 3);
    x.SetFrac(NewInt(3), x.Num());
    if (x.Cmp(q) != 0) {
        t.Errorf("2) got %s want %s", x.OrTypedNil(), q.OrTypedNil());
    }

    // 3) aliasing of denominator
    x = NewRat(2, 3);
    x.SetFrac(x.Denom(), NewInt(2));
    if (x.Cmp(q) != 0) {
        t.Errorf("3) got %s want %s", x.OrTypedNil(), q.OrTypedNil());
    }

    // 4) aliasing of numerator and denominator
    x = NewRat(2, 3);
    x.SetFrac(x.Denom(), x.Num());
    if (x.Cmp(q) != 0) {
        t.Errorf("4) got %s want %s", x.OrTypedNil(), q.OrTypedNil());
    }

    // 5) numerator and denominator are the same
    q = NewRat(1, 1);
    x = @new<global::go.math.big_package.ΔRat>();
    var n = NewInt(7);
    x.SetFrac(n, n);
    if (x.Cmp(q) != 0) {
        t.Errorf("5) got %s want %s", x.OrTypedNil(), q.OrTypedNil());
    }
}

public static void TestIssue3521(ж<testing.T> t) {
    var a = @new<global::go.math.big_package.ΔInt>();
    var b = @new<global::go.math.big_package.ΔInt>();
    a.SetString("64375784358435883458348587", 0);
    b.SetString("4789759874531", 0);

    // 0) a raw zero value has 1 as denominator
    var zero = @new<global::go.math.big_package.ΔRat>();
    var one = NewInt(1);
    if (zero.Denom().Cmp(one) != 0) {
        t.Errorf("0) got %s want %s", zero.Denom().OrTypedNil(), one.OrTypedNil());
    }

    // 1a) the denominator of an (uninitialized) zero value is not shared with the value
    var s = zero.Ꮡb;
    var d = zero.Denom();
    if (d == s) {
        t.Errorf("1a) got %s (%p) == %s (%p) want different *Int values", d.OrTypedNil(), d.OrTypedNil(), s.OrTypedNil(), s.OrTypedNil());
    }

    // 1b) the denominator of an (uninitialized) value is a new 1 each time
    var d1 = zero.Denom();
    var d2 = zero.Denom();
    if (d1 == d2) {
        t.Errorf("1b) got %s (%p) == %s (%p) want different *Int values", d1.OrTypedNil(), d1.OrTypedNil(), d2.OrTypedNil(), d2.OrTypedNil());
    }

    // 1c) the denominator of an initialized zero value is shared with the value
    var x = @new<global::go.math.big_package.ΔRat>();
    x.Set(x); // initialize x (any operation that sets x explicitly will do)
    s = x.Ꮡb;
    d = x.Denom();
    if (d != s) {
        t.Errorf("1c) got %s (%p) != %s (%p) want identical *Int values", d.OrTypedNil(), d.OrTypedNil(), s.OrTypedNil(), s.OrTypedNil());
    }

    // 1d) a zero value remains zero independent of denominator
    x.Denom().Set(@new<global::go.math.big_package.ΔInt>().Neg(b));
    if (x.Cmp(zero) != 0) {
        t.Errorf("1d) got %s want %s", x.OrTypedNil(), zero.OrTypedNil());
    }

    // 1e) a zero value may have a denominator != 0 and != 1
    x.Num().Set(a);
    var qab = @new<global::go.math.big_package.ΔRat>().SetFrac(a, b);
    if (x.Cmp(qab) != 0) {
        t.Errorf("1e) got %s want %s", x.OrTypedNil(), qab.OrTypedNil());
    }

    // 2a) an integral value becomes a fraction depending on denominator
    x.SetFrac64(10, 2);
    x.Denom().SetInt64(3);
    var q53 = NewRat(5, 3);
    if (x.Cmp(q53) != 0) {
        t.Errorf("2a) got %s want %s", x.OrTypedNil(), q53.OrTypedNil());
    }

    // 2b) an integral value becomes a fraction depending on denominator
    x = NewRat(10, 2);
    x.Denom().SetInt64(3);
    if (x.Cmp(q53) != 0) {
        t.Errorf("2b) got %s want %s", x.OrTypedNil(), q53.OrTypedNil());
    }

    // 3) changing the numerator/denominator of a Rat changes the Rat
    x.SetFrac(a, b);
    a = x.Num();
    b = x.Denom();
    a.SetInt64(5);
    b.SetInt64(3);
    if (x.Cmp(q53) != 0) {
        t.Errorf("3) got %s want %s", x.OrTypedNil(), q53.OrTypedNil());
    }
}

public static void TestFloat32Distribution(ж<testing.T> t) {
    // Generate a distribution of (sign, mantissa, exp) values
    // broader than the float32 range, and check Rat.Float32()
    // always picks the closest float32 approximation.
    slice<int64> add = slice<int64>([
        0,
        1,
        3,
        5,
        7,
        9,
        11
    ]);
    uint64 winc = (uint64)5; // quick test (~60ms on x86-64)
    nint einc = 15;
    if (@long.Value) {
        (winc, einc) = ((uint64)1, 1); // soak test (~1.5s on x86-64)
    }

    foreach (var (_, sign) in (@string)"+-") {
        foreach (var (_, a) in range(add)) {
            for (var wid = (uint64)0; wid < 30; wid += winc) {
                var b = ((int64)1).Lsh(wid) + a;
                if (sign == '-') {
                    b = -b;
                }
                for (nint exp = -150; exp < 150; exp += einc) {
                    var (num, den) = (NewInt(b), NewInt(1));
                    if (exp > 0) {
                        num.Lsh(num, (nuint)exp);
                    } else {
                        den.Lsh(den, (nuint)(-exp));
                    }
                    var r = @new<global::go.math.big_package.ΔRat>().SetFrac(num, den);
                    var (f, _) = r.Float32();

                    if (!checkIsBestApprox32(t, f, r)) {
                        // Append context information.
                        t.Errorf("(input was mantissa %#x, exp %d; f = %g (%b); f ~ %g; r = %v)",
                            b, exp, f, f, math.Ldexp((float64)b, exp), r.OrTypedNil());
                    }

                    checkNonLossyRoundtrip32(t, f);
                }
            }
        }
    }
}

public static void TestFloat64Distribution(ж<testing.T> t) {
    // Generate a distribution of (sign, mantissa, exp) values
    // broader than the float64 range, and check Rat.Float64()
    // always picks the closest float64 approximation.
    slice<int64> add = slice<int64>([
        0,
        1,
        3,
        5,
        7,
        9,
        11
    ]);
    uint64 winc = (uint64)10; // quick test (~12ms on x86-64)
    nint einc = 500;
    if (@long.Value) {
        (winc, einc) = ((uint64)1, 1); // soak test (~75s on x86-64)
    }

    foreach (var (_, sign) in (@string)"+-") {
        foreach (var (_, a) in range(add)) {
            for (var wid = (uint64)0; wid < 60; wid += winc) {
                var b = ((int64)1).Lsh(wid) + a;
                if (sign == '-') {
                    b = -b;
                }
                for (nint exp = -1100; exp < 1100; exp += einc) {
                    var (num, den) = (NewInt(b), NewInt(1));
                    if (exp > 0) {
                        num.Lsh(num, (nuint)exp);
                    } else {
                        den.Lsh(den, (nuint)(-exp));
                    }
                    var r = @new<global::go.math.big_package.ΔRat>().SetFrac(num, den);
                    var (f, _) = r.Float64();

                    if (!checkIsBestApprox64(t, f, r)) {
                        // Append context information.
                        t.Errorf("(input was mantissa %#x, exp %d; f = %g (%b); f ~ %g; r = %v)",
                            b, exp, f, f, math.Ldexp((float64)b, exp), r.OrTypedNil());
                    }

                    checkNonLossyRoundtrip64(t, f);
                }
            }
        }
    }
}

// TestSetFloat64NonFinite checks that SetFloat64 of a non-finite value
// returns nil.
public static void TestSetFloat64NonFinite(ж<testing.T> t) {
    foreach (var (_, f) in range(slice<float64>([math.NaN(), math.Inf(+1), math.Inf(-1)]))) {
        ref var r = ref heap(new global::go.math.big_package.ΔRat(), out var Ꮡr);
        var r2 = Ꮡr.SetFloat64(f); if (r2 != nil) {
            t.Errorf("SetFloat64(%g) was %v, want nil", f, r2.OrTypedNil());
        }
    }
}

// checkNonLossyRoundtrip32 checks that a float->Rat->float roundtrip is
// non-lossy for finite f.
internal static void checkNonLossyRoundtrip32(ж<testing.T> t, float32 f) {
    if (!isFinite((float64)f)) {
        return;
    }
    var r = @new<global::go.math.big_package.ΔRat>().SetFloat64((float64)f);
    if (r == nil) {
        t.Errorf("Rat.SetFloat64(float64(%g) (%b)) == nil", f, f);
        return;
    }
    var (f2, exact) = r.Float32();
    if (f != f2 || !exact) {
        t.Errorf("Rat.SetFloat64(float64(%g)).Float32() = %g (%b), %v, want %g (%b), %v; delta = %b",
            f, f2, f2, exact, f, f, true, f2 - f);
    }
}

// checkNonLossyRoundtrip64 checks that a float->Rat->float roundtrip is
// non-lossy for finite f.
internal static void checkNonLossyRoundtrip64(ж<testing.T> t, float64 f) {
    if (!isFinite(f)) {
        return;
    }
    var r = @new<global::go.math.big_package.ΔRat>().SetFloat64(f);
    if (r == nil) {
        t.Errorf("Rat.SetFloat64(%g (%b)) == nil", f, f);
        return;
    }
    var (f2, exact) = r.Float64();
    if (f != f2 || !exact) {
        t.Errorf("Rat.SetFloat64(%g).Float64() = %g (%b), %v, want %g (%b), %v; delta = %b",
            f, f2, f2, exact, f, f, true, f2 - f);
    }
}

// delta returns the absolute difference between r and f.
internal static ж<global::go.math.big_package.ΔRat> delta(ж<global::go.math.big_package.ΔRat> r, float64 f) {
    var d = @new<global::go.math.big_package.ΔRat>().Sub(r, @new<global::go.math.big_package.ΔRat>().SetFloat64(f));
    return d.Abs(d);
}

// checkIsBestApprox32 checks that f is the best possible float32
// approximation of r.
// Returns true on success.
internal static bool checkIsBestApprox32(ж<testing.T> t, float32 f, ж<global::go.math.big_package.ΔRat> r) {
    if (math.Abs((float64)f) >= math.MaxFloat32) {
        // Cannot check +Inf, -Inf, nor the float next to them (MaxFloat32).
        // But we have tests for these special cases.
        return true;
    }

    // r must be strictly between f0 and f1, the floats bracketing f.
    var f0 = math.Nextafter32(f, (float32)math.Inf(-1));
    var f1 = math.Nextafter32(f, (float32)math.Inf(+1));

    // For f to be correct, r must be closer to f than to f0 or f1.
    var df = delta(r, (float64)f);
    var df0 = delta(r, (float64)f0);
    var df1 = delta(r, (float64)f1);
    if (df.Cmp(df0) > 0) {
        t.Errorf("Rat(%v).Float32() = %g (%b), but previous float32 %g (%b) is closer", r.OrTypedNil(), f, f, f0, f0);
        return false;
    }
    if (df.Cmp(df1) > 0) {
        t.Errorf("Rat(%v).Float32() = %g (%b), but next float32 %g (%b) is closer", r.OrTypedNil(), f, f, f1, f1);
        return false;
    }
    if (df.Cmp(df0) == 0 && !isEven32(f)) {
        t.Errorf("Rat(%v).Float32() = %g (%b); halfway should have rounded to %g (%b) instead", r.OrTypedNil(), f, f, f0, f0);
        return false;
    }
    if (df.Cmp(df1) == 0 && !isEven32(f)) {
        t.Errorf("Rat(%v).Float32() = %g (%b); halfway should have rounded to %g (%b) instead", r.OrTypedNil(), f, f, f1, f1);
        return false;
    }
    return true;
}

// checkIsBestApprox64 checks that f is the best possible float64
// approximation of r.
// Returns true on success.
internal static bool checkIsBestApprox64(ж<testing.T> t, float64 f, ж<global::go.math.big_package.ΔRat> r) {
    if (math.Abs(f) >= math.MaxFloat64) {
        // Cannot check +Inf, -Inf, nor the float next to them (MaxFloat64).
        // But we have tests for these special cases.
        return true;
    }

    // r must be strictly between f0 and f1, the floats bracketing f.
    var f0 = math.Nextafter(f, math.Inf(-1));
    var f1 = math.Nextafter(f, math.Inf(+1));

    // For f to be correct, r must be closer to f than to f0 or f1.
    var df = delta(r, f);
    var df0 = delta(r, f0);
    var df1 = delta(r, f1);
    if (df.Cmp(df0) > 0) {
        t.Errorf("Rat(%v).Float64() = %g (%b), but previous float64 %g (%b) is closer", r.OrTypedNil(), f, f, f0, f0);
        return false;
    }
    if (df.Cmp(df1) > 0) {
        t.Errorf("Rat(%v).Float64() = %g (%b), but next float64 %g (%b) is closer", r.OrTypedNil(), f, f, f1, f1);
        return false;
    }
    if (df.Cmp(df0) == 0 && !isEven64(f)) {
        t.Errorf("Rat(%v).Float64() = %g (%b); halfway should have rounded to %g (%b) instead", r.OrTypedNil(), f, f, f0, f0);
        return false;
    }
    if (df.Cmp(df1) == 0 && !isEven64(f)) {
        t.Errorf("Rat(%v).Float64() = %g (%b); halfway should have rounded to %g (%b) instead", r.OrTypedNil(), f, f, f1, f1);
        return false;
    }
    return true;
}

internal static bool isEven32(float32 f) { return (math.Float32bits(f) & 1) == 0; }
internal static bool isEven64(float64 f) { return (math.Float64bits(f) & 1) == 0; }

public static void TestIsFinite(ж<testing.T> t) {
    var finites = slice<float64>([
        1.0D / 3D,
        4891559871276714924261e+222D,
        math.MaxFloat64,
        math.SmallestNonzeroFloat64,
        -math.MaxFloat64,
        -math.SmallestNonzeroFloat64
    ]);
    foreach (var (_, f) in range(finites)) {
        if (!isFinite(f)) {
            t.Errorf("!IsFinite(%g (%b))", f, f);
        }
    }
    var nonfinites = slice<float64>([
        math.NaN(),
        math.Inf(-1),
        math.Inf(+1)
    ]);
    foreach (var (_, f) in range(nonfinites)) {
        if (isFinite(f)) {
            t.Errorf("IsFinite(%g, (%b))", f, f);
        }
    }
}

public static void TestRatSetInt64(ж<testing.T> t) {
    slice<int64> testCases = slice<int64>([
        0,
        1,
        -1,
        12345,
        -98765,
        math.MaxInt64,
        math.MinInt64
    ]);
    ж<global::go.math.big_package.ΔRat> r = @new<global::go.math.big_package.ΔRat>();
    foreach (var (i, want) in range(testCases)) {
        r.SetInt64(want);
        if (!r.IsInt()) {
            t.Errorf("#%d: Rat.SetInt64(%d) is not an integer", i, want);
        }
        var num = r.Num();
        if (!num.IsInt64()) {
            t.Errorf("#%d: Rat.SetInt64(%d) numerator is not an int64", i, want);
        }
        var got = num.Int64();
        if (got != want) {
            t.Errorf("#%d: Rat.SetInt64(%d) = %d, but expected %d", i, want, got, want);
        }
    }
}

public static void TestRatSetUint64(ж<testing.T> t) {
    slice<uint64> testCases = slice<uint64>([
        0,
        1,
        12345,
        ~(uint64)0
    ]);
    ж<global::go.math.big_package.ΔRat> r = @new<global::go.math.big_package.ΔRat>();
    foreach (var (i, want) in range(testCases)) {
        r.SetUint64(want);
        if (!r.IsInt()) {
            t.Errorf("#%d: Rat.SetUint64(%d) is not an integer", i, want);
        }
        var num = r.Num();
        if (!num.IsUint64()) {
            t.Errorf("#%d: Rat.SetUint64(%d) numerator is not a uint64", i, want);
        }
        var got = num.Uint64();
        if (got != want) {
            t.Errorf("#%d: Rat.SetUint64(%d) = %d, but expected %d", i, want, got, want);
        }
    }
}

public static void BenchmarkRatCmp(ж<testing.B> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    var (x, y) = (NewRat(4, 1), NewRat(7, 2));
    for (nint i = 0; i < b.N; i++) {
        x.Cmp(y);
    }
}

internal partial struct TestIssue34919_type /*dyn*/ {
    internal @string name;
    internal Action<ж<global::go.math.big_package.ΔRat>> f;
}

// TestIssue34919 verifies that a Rat's denominator is not modified
// when simply accessing the Rat value.
public static void TestIssue34919(ж<testing.T> t) {
    foreach (var (_, acc) in range(slice<TestIssue34919_type>([
        new("Float32", (ж<global::go.math.big_package.ΔRat> x) => {
            x.Float32();
        }),
        new("Float64", (ж<global::go.math.big_package.ΔRat> x) => {
            x.Float64();
        }),
        new("Inv", (ж<global::go.math.big_package.ΔRat> x) => {
            @new<global::go.math.big_package.ΔRat>().Inv(x);
        }),
        new("Sign", (ж<global::go.math.big_package.ΔRat> x) => {
            x.Sign();
        }),
        new("IsInt", (ж<global::go.math.big_package.ΔRat> x) => {
            x.IsInt();
        }),
        new("Num", (ж<global::go.math.big_package.ΔRat> x) => {
            x.Num();
        })
        // {"Denom", func(x *Rat) { x.Denom() }}, TODO(gri) should we change the API? See issue #33792.
    ]))) {
        // A denominator of length 0 is interpreted as 1. Make sure that
        // "materialization" of the denominator doesn't lead to setting
        // the underlying array element 0 to 1.
        var r = Ꮡ(new ΔRat(new ΔInt(abs: new Δnat(new global::go.math.big_package.Word[]{991}.slice())), new ΔInt(abs: new global::go.math.big_package.Δnat(0, 1))));
        acc.f(r);
        global::go.math.big_package.Word d = r.b.abs[..1][0]; if (d != 0) {
            t.Errorf("%s modified denominator: got %d, want 0", acc.name, d);
        }
    }
}

public static partial void TestDenomRace(ж<testing.T> t) {
    var x = NewRat(1, 2);
    const nint N = 3;
    var c = new channel<bool>(N);
    for (nint i = 0; i < N; i++) {
        var cʗ1 = c;
        var xʗ1 = x;
        goǃ(() => {
            // Denom (also used by Float.SetRat) used to mutate x unnecessarily,
            // provoking race reports when run in the race detector.
            xʗ1.Denom();
            @new<global::go.math.big_package.Float>().SetRat(xʗ1);
            cʗ1.ᐸꟷ(true);
        });
    }
    for (nint i = 0; i < N; i++) {
        ᐸꟷ(c);
    }
}

} // end big_internal_test_package
