namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;

partial class main_package {

internal static UntypedInt namedInt => 7;

internal static UntypedInt namedRune => /* 'A' */ 65;

internal static UntypedFloat namedFloat => 2.5;

internal static UntypedInt namedWide => /* 1 << 40 */ 1099511627776;

internal static readonly @string namedStr = "seed"u8;

internal static readonly @string typedStr = "seed"u8;

partial struct holder {
    internal any v;
}

internal static bool variadicEq(params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    return AreEqual(args[0], args[1]);
}

internal static bool paramEq(any a, any b) {
    return AreEqual(a, b);
}

internal static any retLit() {
    return (nint)0;
}

internal static any retNamed() {
    return (nint)(namedInt);
}

internal static any retRuneL() {
    return (rune)'A';
}

internal static any retFloatL() {
    return 2.5D;
}

internal static @string kindOf(any v) {
    switch (v.type()) {
    case nint: {
        return "int";
    }
    case int32: {
        return "int32";
    }
    case float64: {
        return "float64";
    }
    case @string: {
        return "string";
    }}

    return "other";
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object seedˢ = (@string)"seed"u8;

internal static any retStrL() {
    return seedˢ;
}

internal static (nint, error) readN() {
    return (0, default);
}

internal static void Main() {
    var (n, _) = readN();
    nint i = 7;
    rune r = 'A';
    var f = 2.5D;

    fmt.Println("var lit zero  :", variadicEq(n, (nint)0));
    fmt.Println("var lit int   :", variadicEq(i, (nint)7));
    fmt.Println("var named     :", variadicEq(i, (nint)(namedInt)));
    fmt.Println("var named+1   :", variadicEq(i + 1, (nint)(namedInt + 1)));
    fmt.Println("var lit expr  :", variadicEq(i + 1, (nint)(3 + 5)));
    fmt.Println("var neg lit   :", variadicEq(-i, (nint)(-7)));
    fmt.Println("var rune lit  :", variadicEq(r, (rune)'A'));
    fmt.Println("var rune named:", variadicEq(r, (int32)(namedRune)));
    fmt.Println("var float lit :", variadicEq(f, 2.5D));
    fmt.Println("var float nmd :", variadicEq(f, (float64)(namedFloat)));

    fmt.Println("par lit       :", paramEq(i, (nint)7));
    fmt.Println("par named     :", paramEq(i, (nint)(namedInt)));
    fmt.Println("par rune lit  :", paramEq(r, (rune)'A'));
    fmt.Println("par float lit :", paramEq(f, 2.5D));

    any av = (nint)7;
    any ar = (rune)'A';
    any af = 2.5D;
    fmt.Println("asn lit       :", AreEqual(av, ((any)i)));
    fmt.Println("asn rune lit  :", AreEqual(ar, ((any)r)));
    fmt.Println("asn float lit :", AreEqual(af, ((any)f)));

    var sl = new any[]{(nint)7, (rune)'A', 2.5D, (nint)(namedInt)}.slice();
    fmt.Println("slc lit       :", AreEqual(sl[0], ((any)i)));
    fmt.Println("slc rune lit  :", AreEqual(sl[1], ((any)r)));
    fmt.Println("slc float lit :", AreEqual(sl[2], ((any)f)));
    fmt.Println("slc named     :", AreEqual(sl[3], ((any)i)));

    var mk = new map<any, @string>{[(nint)7] = "seven", [(rune)'A'] = "letter", [2.5D] = "half"};
    fmt.Println("mapkey lit    :", mk[((any)i)], mk[((any)r)], mk[((any)f)]);
    fmt.Println("mapkey lookup :", mk[(nint)7], mk[(rune)'A'], mk[2.5D], mk[(nint)(namedInt)]);
    var (_, mkMiss) = mk[(int32)7, ꟷ];
    fmt.Println("mapkey miss   :", mkMiss);

    var mv = new map<@string, any>{["i"] = (nint)7, ["r"] = (rune)'A', ["f"] = 2.5D};
    fmt.Println("mapval lit    :", AreEqual(mv["i"], ((any)i)), AreEqual(mv["r"], ((any)r)), AreEqual(mv["f"], ((any)f)));

    var hk = new holder(v: (nint)7);
    var hp = new holder((nint)7);
    fmt.Println("struct keyed  :", AreEqual(hk.v, ((any)i)));
    fmt.Println("struct posit  :", AreEqual(hp.v, ((any)i)));

    var ch = new channel<any>(1);
    ch.ᐸꟷ((nint)7);
    fmt.Println("chan send     :", AreEqual(ᐸꟷ(ch), ((any)i)));

    fmt.Println("ret lit       :", AreEqual(retLit(), ((any)n)));
    fmt.Println("ret named     :", AreEqual(retNamed(), ((any)i)));
    fmt.Println("ret rune lit  :", AreEqual(retRuneL(), ((any)r)));
    fmt.Println("ret float lit :", AreEqual(retFloatL(), ((any)f)));

    nint wide = unchecked((nint)(1099511627776L));
    fmt.Println("wide lit      :", variadicEq(wide, unchecked((nint)(1099511627776L))));
    fmt.Println("wide named    :", variadicEq(wide, (nint)(namedWide)));
    fmt.Println("wide kinds    :", kindOf(unchecked((nint)(1099511627776L))), kindOf((nint)(namedWide)));

    fmt.Println("kinds lit     :", kindOf((nint)0), kindOf((rune)'A'), kindOf(2.5D));
    fmt.Println("kinds named   :", kindOf((nint)(namedInt)), kindOf((int32)(namedRune)), kindOf((float64)(namedFloat)));
    fmt.Println("kinds value   :", kindOf(i), kindOf(r), kindOf(f));

    var (iv, iok) = ((any)(nint)7)._<nint>(ᐧ);
    var (rv, rok) = ((any)(rune)'A')._<int32>(ᐧ);
    var (fv, fok) = ((any)2.5D)._<float64>(ᐧ);
    fmt.Println("assert lit    :", iv, iok, rv, rok, fv, fok);

    @string s = "seed";
    fmt.Println("str var lit   :", variadicEq(s, "seed"));
    fmt.Println("str var cat   :", variadicEq(s, (@string)("se" + "ed")));
    fmt.Println("str var named :", variadicEq(s, namedStr), variadicEq(s, typedStr));
    fmt.Println("str par lit   :", paramEq(s, "seed"), paramEq("seed", "seed"));

    any sv = seedˢ;
    any sc = (@string)("se" + "ed");
    fmt.Println("str asn lit   :", AreEqual(sv, ((any)s)), AreEqual(sc, ((any)s)));

    var ssl = new any[]{(@string)"seed"u8, (@string)("se" + "ed")}.slice();
    fmt.Println("str slc       :", AreEqual(ssl[0], ((any)s)), AreEqual(ssl[1], ((any)s)));

    var smk = new map<any, @string>{[(@string)"seed"u8] = "hit"};
    fmt.Println("str mapkey    :", smk[seedˢ], smk[((any)s)]);

    var smv = new map<@string, any>{["k"] = (@string)"seed"u8, ["c"] = (@string)("se"u8 + "ed"u8)};
    fmt.Println("str mapval    :", AreEqual(smv["k"], ((any)s)), AreEqual(smv["c"], ((any)s)));

    var shk = new holder(v: (@string)"seed"u8);
    var shp = new holder((@string)"seed"u8);
    var shc = new holder(v: (@string)("se"u8 + "ed"u8));
    fmt.Println("str struct    :", AreEqual(shk.v, ((any)s)), AreEqual(shp.v, ((any)s)), AreEqual(shc.v, ((any)s)));

    var sch = new channel<any>(1);
    sch.ᐸꟷ(seedˢ);
    fmt.Println("str chan      :", AreEqual(ᐸꟷ(sch), ((any)s)));

    fmt.Println("str ret       :", AreEqual(retStrL(), ((any)s)));
    fmt.Println("str conv      :", AreEqual(((any)(@string)("seed"u8)), ((any)s)));

    var (stv, stok) = ((any)(@string)("seed"u8))._<@string>(ᐧ);
    fmt.Println("str assert    :", stv, stok);
    fmt.Println("str kinds     :", kindOf("seed"), kindOf((@string)("se" + "ed")), kindOf(namedStr), kindOf(typedStr), kindOf(s));
}

} // end main_package
