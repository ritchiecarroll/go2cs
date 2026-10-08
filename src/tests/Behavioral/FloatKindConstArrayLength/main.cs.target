global using aliasArr = go.array<nint>;

namespace go;

using fmt = fmt_package;

partial class main_package {

internal static UntypedFloat globalLen => 1e2;
internal static UntypedFloat namedLen => 1e1;
internal static UntypedFloat exprLen => /* 1e3 / 1e1 */ 100;
internal static UntypedComplex cplxLen => /* 1e1 + 0i */ 10D + 0D.i();
internal static UntypedFloat overflow => 1e6;
internal static UntypedFloat shiftBy => 4e0;

internal static UntypedInt intLen => 100;

internal static array<byte> pkgArr = new(100);

internal static array<nint> pkgCplxArr = new(10);

internal static array<byte> pkgExprArr = new(100);

internal static array<byte> pkgIntArr = new(100);

/* [namedLen]byte */
partial struct floatLenArr /*[10]byte*/;

/* [cplxLen]int32 */
partial struct cplxLenArr /*[10]int32*/;

/* [exprLen]byte */
partial struct exprLenArr /*[100]byte*/;

/* [intLen]byte */
partial struct intLenArr /*[100]byte*/;

partial struct holder {
    internal array<byte> buf = new(globalLen);
    internal array<array<byte>> nest = new(namedLen, () => new(10));
}

internal static array<byte> roundTrip(/*[10]*/ array<byte> a) {
    a = a.Clone();

    a[0] = 7;
    return a.Clone();
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object localˢ = (@string)"local"u8;
private static readonly object bigˢ = (@string)"big"u8;
private static readonly object localExprˢ = (@string)"local expr"u8;
private static readonly object localLiteralˢ = (@string)"local literal"u8;
private static readonly object localComplexˢ = (@string)"local complex"u8;
private static readonly object multiˢ = (@string)"multi"u8;
private static readonly object localIntˢ = (@string)"local int"u8;
private static readonly object pkgˢ = (@string)"pkg"u8;
private static readonly object namedˢ = (@string)"named"u8;
private static readonly object fieldˢ = (@string)"field"u8;
private static readonly object aliasˢ = (@string)"alias"u8;
private static readonly object paramˢ = (@string)"param"u8;
private static readonly object compositeˢ = (@string)"composite"u8;
private static readonly object pointerˢ = (@string)"pointer"u8;
private static readonly object mapValueˢ = (@string)"map value"u8;
private static readonly object sliceElemˢ = (@string)"slice elem"u8;
private static readonly object shiftˢ = (@string)"shift"u8;

internal static void Main() {
    array<byte> localArr = new(100); /* globalLen */
    localArr[0] = 1;
    localArr[globalLen - 1] = 2;
    fmt.Println(localˢ, len(localArr), localArr[0], localArr[99]);
    array<byte> big = new(1000000); /* overflow */
    big[overflow - 1] = 3;
    fmt.Println(bigˢ, len(big), big[999999]);
    array<byte> localExpr = new(100); /* exprLen */
    fmt.Println(localExprˢ, len(localExpr));
    array<byte> localLit = new(50); /* 1e2D / 2 */
    fmt.Println(localLiteralˢ, len(localLit));
    array<nint> localCplx = new(10); /* cplxLen */
    localCplx[9] = 4;
    fmt.Println(localComplexˢ, len(localCplx), localCplx[9]);
    array<array<byte>> multi = new(10, () => new(10)); /* namedLen */
    multi[9][9] = 5;
    fmt.Println(multiˢ, len(multi), len(multi[0]), multi[9][9]);
    array<byte> localInt = new(100); /* intLen */
    fmt.Println(localIntˢ, len(localInt));
    pkgArr[0] = 6;
    pkgCplxArr[9] = 7;
    fmt.Println(pkgˢ, len(pkgArr), pkgArr[0], len(pkgCplxArr), pkgCplxArr[9], len(pkgExprArr), len(pkgIntArr));
    floatLenArr fl = default!;
    fl[9] = 8;
    cplxLenArr cl = default!;
    cl[9] = 9;
    exprLenArr el = default!;
    intLenArr il = default!;
    fmt.Println(namedˢ, len(fl), fl[9], len(cl), cl[9], len(el), len(il));
    holder h = new();
    h.buf[99] = 10;
    h.nest[9][9] = 11;
    fmt.Println(fieldˢ, len(h.buf), h.buf[99], len(h.nest), len(h.nest[0]), h.nest[9][9]);
    aliasArr al = new(10);
    al[9] = 12;
    fmt.Println(aliasˢ, len(al), al[9]);
    array<byte> arg = new(10); /* namedLen */
    var got = roundTrip(arg);
    fmt.Println(paramˢ, len(got), got[0]);
    ref var lit = ref heap<array<byte>>(out var Ꮡlit);
    lit = new byte[]{1, 2}.array(10);
    fmt.Println(compositeˢ, len(lit), lit[0], lit[9]);
    ж<array<byte>> ptr = Ꮡlit;
    fmt.Println(pointerˢ, 10, ptr.Value[1]);
    var m = new map<@string, array<byte>>{["k"u8] = lit.Clone()};
    fmt.Println(mapValueˢ, len(m["k"u8, () => new array<byte>(10)]), m["k"u8, () => new array<byte>(10)][1]);
    var sl = GoReflect.WithElemDims(new array<byte>[]{lit.Clone()}.slice(), 10);
    fmt.Println(sliceElemˢ, len(sl), len(sl[0]), sl[0][1]);
    uintptr u = 1;
    nuint n = 1;
    int64 i64 = 1;
    fmt.Println(shiftˢ, (uint64)((u << (int)(shiftBy))), (uint64)((n << (int)(shiftBy))), (i64 << (int)(shiftBy)), (nint)((1 << (int)(shiftBy))));
}

} // end main_package
