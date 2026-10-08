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
    return (nint)(0);
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string intˢ = "int"u8;
private static readonly @string int32ˢ = "int32"u8;
private static readonly @string float64ˢ = "float64"u8;
private static readonly @string stringˢ = "string"u8;
private static readonly @string otherˢ = "other"u8;

internal static @string kindOf(any v) {
    switch (v.type()) {
    case nint: {
        return intˢ;
    }
    case int32: {
        return int32ˢ;
    }
    case float64: {
        return float64ˢ;
    }
    case @string: {
        return stringˢ;
    }}

    return otherˢ;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string seedˢ = "seed"u8;

internal static any retStrL() {
    return seedˢ;
}

internal static (nint, error) readN() {
    return (0, default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object varLitZeroˢ = (@string)"var lit zero  :"u8;
private static readonly object varLitIntˢ = (@string)"var lit int   :"u8;
private static readonly object varNamedˢ = (@string)"var named     :"u8;
private static readonly object varNamed1ˢ = (@string)"var named+1   :"u8;
private static readonly object varLitExprˢ = (@string)"var lit expr  :"u8;
private static readonly object varNegLitˢ = (@string)"var neg lit   :"u8;
private static readonly object varRuneLitˢ = (@string)"var rune lit  :"u8;
private static readonly object varRuneNamedˢ = (@string)"var rune named:"u8;
private static readonly object varFloatLitˢ = (@string)"var float lit :"u8;
private static readonly object varFloatNmdˢ = (@string)"var float nmd :"u8;
private static readonly object parLitˢ = (@string)"par lit       :"u8;
private static readonly object parNamedˢ = (@string)"par named     :"u8;
private static readonly object parRuneLitˢ = (@string)"par rune lit  :"u8;
private static readonly object parFloatLitˢ = (@string)"par float lit :"u8;
private static readonly object asnLitˢ = (@string)"asn lit       :"u8;
private static readonly object asnRuneLitˢ = (@string)"asn rune lit  :"u8;
private static readonly object asnFloatLitˢ = (@string)"asn float lit :"u8;
private static readonly object slcLitˢ = (@string)"slc lit       :"u8;
private static readonly object slcRuneLitˢ = (@string)"slc rune lit  :"u8;
private static readonly object slcFloatLitˢ = (@string)"slc float lit :"u8;
private static readonly object slcNamedˢ = (@string)"slc named     :"u8;
private static readonly object mapkeyLitˢ = (@string)"mapkey lit    :"u8;
private static readonly object mapkeyLookupˢ = (@string)"mapkey lookup :"u8;
private static readonly object mapkeyMissˢ = (@string)"mapkey miss   :"u8;
private static readonly object mapvalLitˢ = (@string)"mapval lit    :"u8;
private static readonly object structKeyedˢ = (@string)"struct keyed  :"u8;
private static readonly object structPositˢ = (@string)"struct posit  :"u8;
private static readonly object chanSendˢ = (@string)"chan send     :"u8;
private static readonly object retLitˢ = (@string)"ret lit       :"u8;
private static readonly object retNamedˢ = (@string)"ret named     :"u8;
private static readonly object retRuneLitˢ = (@string)"ret rune lit  :"u8;
private static readonly object retFloatLitˢ = (@string)"ret float lit :"u8;
private static readonly object wideLitˢ = (@string)"wide lit      :"u8;
private static readonly object wideNamedˢ = (@string)"wide named    :"u8;
private static readonly object wideKindsˢ = (@string)"wide kinds    :"u8;
private static readonly object kindsLitˢ = (@string)"kinds lit     :"u8;
private static readonly object kindsNamedˢ = (@string)"kinds named   :"u8;
private static readonly object kindsValueˢ = (@string)"kinds value   :"u8;
private static readonly object assertLitˢ = (@string)"assert lit    :"u8;
private static readonly object strVarLitˢ = (@string)"str var lit   :"u8;
private static readonly object strVarCatˢ = (@string)"str var cat   :"u8;
private static readonly object strVarNamedˢ = (@string)"str var named :"u8;
private static readonly object strParLitˢ = (@string)"str par lit   :"u8;
private static readonly object strAsnLitˢ = (@string)"str asn lit   :"u8;
private static readonly object strSlcˢ = (@string)"str slc       :"u8;
private static readonly object strMapkeyˢ = (@string)"str mapkey    :"u8;
private static readonly object strMapvalˢ = (@string)"str mapval    :"u8;
private static readonly object strStructˢ = (@string)"str struct    :"u8;
private static readonly object strChanˢ = (@string)"str chan      :"u8;
private static readonly object strRetˢ = (@string)"str ret       :"u8;
private static readonly object strConvˢ = (@string)"str conv      :"u8;
private static readonly object strAssertˢ = (@string)"str assert    :"u8;
private static readonly object strKindsˢ = (@string)"str kinds     :"u8;

internal static void Main() {
    var (n, _) = readN();
    nint i = 7;
    rune r = (rune)'A';
    var f = 2.5D;
    fmt.Println(varLitZeroˢ, variadicEq(n, (nint)(0)));
    fmt.Println(varLitIntˢ, variadicEq(i, (nint)(7)));
    fmt.Println(varNamedˢ, variadicEq(i, (nint)(namedInt)));
    fmt.Println(varNamed1ˢ, variadicEq(i + 1, (nint)(namedInt + 1)));
    fmt.Println(varLitExprˢ, variadicEq(i + 1, (nint)(3 + 5)));
    fmt.Println(varNegLitˢ, variadicEq(-i, (nint)(-7)));
    fmt.Println(varRuneLitˢ, variadicEq(r, (rune)'A'));
    fmt.Println(varRuneNamedˢ, variadicEq(r, (int32)(namedRune)));
    fmt.Println(varFloatLitˢ, variadicEq(f, 2.5D));
    fmt.Println(varFloatNmdˢ, variadicEq(f, (float64)(namedFloat)));
    fmt.Println(parLitˢ, paramEq(i, (nint)(7)));
    fmt.Println(parNamedˢ, paramEq(i, (nint)(namedInt)));
    fmt.Println(parRuneLitˢ, paramEq(r, (rune)'A'));
    fmt.Println(parFloatLitˢ, paramEq(f, 2.5D));
    any av = (nint)(7);
    any ar = (rune)'A';
    any af = 2.5D;
    fmt.Println(asnLitˢ, AreEqual(av, ((any)i)));
    fmt.Println(asnRuneLitˢ, AreEqual(ar, ((any)r)));
    fmt.Println(asnFloatLitˢ, AreEqual(af, ((any)f)));
    var sl = new any[]{(nint)(7), (rune)'A', 2.5D, (nint)(namedInt)}.slice();
    fmt.Println(slcLitˢ, AreEqual(sl[0], ((any)i)));
    fmt.Println(slcRuneLitˢ, AreEqual(sl[1], ((any)r)));
    fmt.Println(slcFloatLitˢ, AreEqual(sl[2], ((any)f)));
    fmt.Println(slcNamedˢ, AreEqual(sl[3], ((any)i)));
    var mk = new map<any, @string>{[(nint)(7)] = "seven"u8, [(rune)'A'] = "letter"u8, [2.5D] = "half"u8};
    fmt.Println(mapkeyLitˢ, mk[((any)i)], mk[((any)r)], mk[((any)f)]);
    fmt.Println(mapkeyLookupˢ, mk[(nint)(7)], mk[(rune)'A'], mk[2.5D], mk[(nint)(namedInt)]);
    var (_, mkMiss) = mk[(int32)7, ꟷ];
    fmt.Println(mapkeyMissˢ, mkMiss);
    var mv = new map<@string, any>{["i"u8] = (nint)(7), ["r"u8] = (rune)'A', ["f"u8] = 2.5D};
    fmt.Println(mapvalLitˢ, AreEqual(mv["i"u8], ((any)i)), AreEqual(mv["r"u8], ((any)r)), AreEqual(mv["f"u8], ((any)f)));
    var hk = new holder(v: (nint)(7));
    var hp = new holder((nint)(7));
    fmt.Println(structKeyedˢ, AreEqual(hk.v, ((any)i)));
    fmt.Println(structPositˢ, AreEqual(hp.v, ((any)i)));
    var ch = new channel<any>(1);
    ch.ᐸꟷ((nint)(7));
    fmt.Println(chanSendˢ, AreEqual(ᐸꟷ(ch), ((any)i)));
    fmt.Println(retLitˢ, AreEqual(retLit(), ((any)n)));
    fmt.Println(retNamedˢ, AreEqual(retNamed(), ((any)i)));
    fmt.Println(retRuneLitˢ, AreEqual(retRuneL(), ((any)r)));
    fmt.Println(retFloatLitˢ, AreEqual(retFloatL(), ((any)f)));
    nint wide = unchecked((nint)(1099511627776L));
    fmt.Println(wideLitˢ, variadicEq(wide, unchecked((nint)(1099511627776L))));
    fmt.Println(wideNamedˢ, variadicEq(wide, (nint)(namedWide)));
    fmt.Println(wideKindsˢ, kindOf(unchecked((nint)(1099511627776L))), kindOf((nint)(namedWide)));
    fmt.Println(kindsLitˢ, kindOf((nint)(0)), kindOf((rune)'A'), kindOf(2.5D));
    fmt.Println(kindsNamedˢ, kindOf((nint)(namedInt)), kindOf((int32)(namedRune)), kindOf((float64)(namedFloat)));
    fmt.Println(kindsValueˢ, kindOf(i), kindOf(r), kindOf(f));
    var (iv, iok) = ((any)(nint)(7))._<nint>(ᐧ);
    var (rv, rok) = ((any)(rune)'A')._<int32>(ᐧ);
    var (fv, fok) = ((any)2.5D)._<float64>(ᐧ);
    fmt.Println(assertLitˢ, iv, iok, rv, rok, fv, fok);
    @string s = seedˢ;
    fmt.Println(strVarLitˢ, variadicEq(s, seedˢ));
    fmt.Println(strVarCatˢ, variadicEq(s, (@string)("se" + "ed")));
    fmt.Println(strVarNamedˢ, variadicEq(s, namedStr), variadicEq(s, typedStr));
    fmt.Println(strParLitˢ, paramEq(s, seedˢ), paramEq(seedˢ, seedˢ));
    any sv = seedˢ;
    any sc = (@string)("se" + "ed");
    fmt.Println(strAsnLitˢ, AreEqual(sv, ((any)s)), AreEqual(sc, ((any)s)));
    var ssl = new any[]{(@string)"seed"u8, (@string)("se" + "ed")}.slice();
    fmt.Println(strSlcˢ, AreEqual(ssl[0], ((any)s)), AreEqual(ssl[1], ((any)s)));
    var smk = new map<any, @string>{[(@string)"seed"u8] = "hit"u8};
    fmt.Println(strMapkeyˢ, smk[seedˢ], smk[((any)s)]);
    var smv = new map<@string, any>{["k"u8] = (@string)"seed"u8, ["c"u8] = (@string)("se"u8 + "ed"u8)};
    fmt.Println(strMapvalˢ, AreEqual(smv["k"u8], ((any)s)), AreEqual(smv["c"u8], ((any)s)));
    var shk = new holder(v: (@string)"seed"u8);
    var shp = new holder((@string)"seed"u8);
    var shc = new holder(v: (@string)("se"u8 + "ed"u8));
    fmt.Println(strStructˢ, AreEqual(shk.v, ((any)s)), AreEqual(shp.v, ((any)s)), AreEqual(shc.v, ((any)s)));
    var sch = new channel<any>(1);
    sch.ᐸꟷ(seedˢ);
    fmt.Println(strChanˢ, AreEqual(ᐸꟷ(sch), ((any)s)));
    fmt.Println(strRetˢ, AreEqual(retStrL(), ((any)s)));
    fmt.Println(strConvˢ, AreEqual(((any)(@string)("seed"u8)), ((any)s)));
    var (stv, stok) = ((any)(@string)("seed"u8))._<@string>(ᐧ);
    fmt.Println(strAssertˢ, stv, stok);
    fmt.Println(strKindsˢ, kindOf(seedˢ), kindOf((@string)("se" + "ed")), kindOf(namedStr), kindOf(typedStr), kindOf(s));
}

} // end main_package
