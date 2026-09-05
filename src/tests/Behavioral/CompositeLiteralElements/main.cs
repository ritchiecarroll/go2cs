namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial struct S {
    public nint A;
    public @string B;
}

[GoType] partial struct withArray {
    public array<array<nint>> A = new(2, () => new(3));
}

[GoType("[4]byte")] partial struct nb;

[GoType("[2]array<nint>")] partial struct nn;

[GoType("[2]withArray")] partial struct ns;

[GoType("[3]nint")] partial struct ni;

[GoType("[2]ni")] partial struct no;

[GoType("[]nint")] partial struct nsl;

[GoType("map[@string, nint]")] partial struct nmp;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object paExplicitˢ = (@string)"paExplicit:"u8;
private static readonly object paPopˢ = (@string)"paPop:"u8;
private static readonly object paShortˢ = (@string)"paShort:"u8;
private static readonly object pnaˢ = (@string)"pna:"u8;
private static readonly object pnaExplicitˢ = (@string)"pnaExplicit:"u8;
private static readonly object pnslˢ = (@string)"pnsl:"u8;
private static readonly object pnslExplicitˢ = (@string)"pnslExplicit:"u8;
private static readonly object pnmpˢ = (@string)"pnmp:"u8;
private static readonly object pnmpExplicitˢ = (@string)"pnmpExplicit:"u8;
private static readonly object pnaPopˢ = (@string)"pnaPop:"u8;
private static readonly object pnnˢ = (@string)"pnn:"u8;
private static readonly object pnnExplicitˢ = (@string)"pnnExplicit:"u8;
private static readonly object mpnˢ = (@string)"mpn:"u8;
private static readonly object apnˢ = (@string)"apn:"u8;
private static readonly object pnestˢ = (@string)"pnest:"u8;
private static readonly object ps2ˢ = (@string)"ps2:"u8;
private static readonly object pslˢ = (@string)"psl:"u8;
private static readonly object nestedˢ = (@string)"nested:"u8;
private static readonly object nestedPopˢ = (@string)"nestedPop:"u8;
private static readonly object nestedShortˢ = (@string)"nestedShort:"u8;
private static readonly object nestedKeyedˢ = (@string)"nestedKeyed:"u8;
private static readonly object structElemˢ = (@string)"structElem:"u8;
private static readonly object arrNestedˢ = (@string)"arrNested:"u8;
private static readonly object mapNestedˢ = (@string)"mapNested:"u8;
private static readonly object sanˢ = (@string)"san:"u8;
private static readonly object nnLitˢ = (@string)"nnLit:"u8;
private static readonly object nnPtrˢ = (@string)"nnPtr:"u8;
private static readonly object nsLitˢ = (@string)"nsLit:"u8;
private static readonly object nsPtrˢ = (@string)"nsPtr:"u8;
private static readonly object nnElidedˢ = (@string)"nnElided:"u8;
private static readonly object nbLitˢ = (@string)"nbLit:"u8;
private static readonly object noLitˢ = (@string)"noLit:"u8;
private static readonly object nnPopˢ = (@string)"nnPop:"u8;
private static readonly object nnShortˢ = (@string)"nnShort:"u8;
private static readonly object nnKeyedˢ = (@string)"nnKeyed:"u8;
private static readonly object ctrlˢ = (@string)"ctrl:"u8;
private static readonly object ctrlLitˢ = (@string)"ctrlLit:"u8;

[GoType("dyn")] internal partial struct main_sa {
    public array<array<nint>> A = new(2, () => new(3));
}

internal static void Main() {
    var pa = new ж<array<byte>>[]{Ꮡ(new byte[]{}.array(4))}.slice();
    fmt.Println((@string)"pa:"u8, len(pa), len(pa[0].Value), pa[0].Value);
    pa[0].Value[1] = 9;
    fmt.Printf("pa written: %v\n"u8, pa[0].Value);
    var paExplicit = new ж<array<byte>>[]{Ꮡ(new byte[]{}.array(4))}.slice();
    fmt.Println(paExplicitˢ, len(paExplicit), len(paExplicit[0].Value), paExplicit[0].Value);
    var paPop = new ж<array<byte>>[]{Ꮡ(new byte[]{1, 2, 3, 4}.array())}.slice();
    fmt.Println(paPopˢ, len(paPop[0].Value), paPop[0].Value);
    var paShort = new ж<array<byte>>[]{Ꮡ(new byte[]{1, 2}.array(4))}.slice();
    fmt.Println(paShortˢ, len(paShort[0].Value), paShort[0].Value);
    var pna = new ж<nb>[]{Ꮡ(new nb(new byte[4].array()))}.slice();
    var pnaExplicit = new ж<nb>[]{Ꮡ(new nb(new byte[4].array()))}.slice();
    fmt.Println(pnaˢ, len(pna[0].Value), pna[0].Value);
    fmt.Println(pnaExplicitˢ, len(pnaExplicit[0].Value), pnaExplicit[0].Value);
    pna[0].Value[2] = 7;
    fmt.Printf("pna written: %v\n"u8, pna[0].Value);
    var pnsl = new ж<nsl>[]{Ꮡ(new nsl(new nint[]{}.slice()))}.slice();
    var pnslExplicit = new ж<nsl>[]{Ꮡ(new nsl(new nint[]{}.slice()))}.slice();
    fmt.Println(pnslˢ, len(pnsl[0].ValueSlot), pnsl[0].ValueSlot == default!, pnsl[0].ValueSlot);
    fmt.Println(pnslExplicitˢ, len(pnslExplicit[0].ValueSlot), pnslExplicit[0].ValueSlot);
    var pnmp = new ж<nmp>[]{Ꮡ(new nmp(new map<@string, nint>{}))}.slice();
    var pnmpExplicit = new ж<nmp>[]{Ꮡ(new nmp(new map<@string, nint>{}))}.slice();
    fmt.Println(pnmpˢ, len(pnmp[0].ValueSlot), pnmp[0].ValueSlot == default!, pnmp[0].ValueSlot);
    fmt.Println(pnmpExplicitˢ, len(pnmpExplicit[0].ValueSlot), pnmpExplicit[0].ValueSlot);
    var pnaPop = new ж<nb>[]{Ꮡ(new nb(new byte[]{1, 2, 3, 4}.array()))}.slice();
    fmt.Println(pnaPopˢ, len(pnaPop[0].Value), pnaPop[0].Value);
    var pnn = new ж<nn>[]{Ꮡ(new nn(new array<array<nint>>(2, () => new(3))))}.slice();
    fmt.Println(pnnˢ, len(pnn[0].Value), len((pnn[0].Value)[0]), pnn[0].Value);
    var pnnExplicit = new ж<nn>[]{Ꮡ(new nn(new array<array<nint>>(2, () => new(3))))}.slice();
    fmt.Println(pnnExplicitˢ, len(pnnExplicit[0].Value), len((pnnExplicit[0].Value)[0]), pnnExplicit[0].Value);
    var mpn = new map<@string, ж<nb>>{["a"u8] = Ꮡ(new nb(new byte[4].array()))};
    fmt.Println(mpnˢ, len(mpn), len(mpn["a"u8].Value), mpn["a"u8].Value);
    var apn = new ж<nb>[]{Ꮡ(new nb(new byte[4].array())), Ꮡ(new nb(new byte[4].array()))}.array();
    fmt.Println(apnˢ, len(apn), len(apn[0].Value), apn[0].Value, apn[1].Value);
    var pnest = new ж<array<array<nint>>>[]{Ꮡ(new array<nint>[]{}.array(2, () => new(3)))}.slice();
    fmt.Println(pnestˢ, len(pnest[0].Value), len(pnest[0].Value[0]), pnest[0].Value);
    var ps = new ж<S>[]{Ꮡ(new S())}.slice();
    fmt.Println((@string)"ps:"u8, len(ps), ps[0].Value);
    var ps2 = new ж<S>[]{Ꮡ(new S(A: 7, B: "x"u8))}.slice();
    fmt.Println(ps2ˢ, ps2[0].Value);
    var psl = new ж<slice<nint>>[]{Ꮡ(new nint[]{}.slice())}.slice();
    fmt.Println(pslˢ, len(psl), len(psl[0].ValueSlot), psl[0].ValueSlot == default!, psl[0].ValueSlot);
    var pm = new ж<map<@string, nint>>[]{Ꮡ(new map<@string, nint>{})}.slice();
    fmt.Println((@string)"pm:"u8, len(pm), len(pm[0].ValueSlot), pm[0].ValueSlot == default!, pm[0].ValueSlot);
    var mp = new map<@string, ж<array<nint>>>{["a"u8] = Ꮡ(new nint[]{}.array(2))};
    fmt.Println((@string)"mp:"u8, len(mp), len(mp["a"u8].Value), mp["a"u8].Value);
    var ap = new ж<array<nint>>[]{Ꮡ(new nint[]{}.array(3)), Ꮡ(new nint[]{}.array(3))}.array();
    fmt.Println((@string)"ap:"u8, len(ap), len(ap[0].Value), ap[0].Value, ap[1].Value);
    var nested = GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{}.array(2, () => new(3))}.slice(), 2, 3);
    fmt.Println(nestedˢ, len(nested), len(nested[0]), len(nested[0][0]), nested[0]);
    nested[0][1][2] = 9;
    fmt.Printf("nested written: %v\n"u8, nested[0]);
    var nestedPop = GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array()}.slice(), 2, 3);
    fmt.Println(nestedPopˢ, len(nestedPop[0]), len(nestedPop[0][0]), nestedPop[0]);
    var nestedShort = GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array()}.array(2, () => new(3))}.slice(), 2, 3);
    fmt.Println(nestedShortˢ, len(nestedShort[0]), len(nestedShort[0][1]), nestedShort[0]);
    var nestedKeyed = GoReflect.WithElemDims(new array<array<nint>>[]{new golib.SparseArray<array<nint>>{[1] = new nint[]{7, 8, 9}.array()}.array(2, () => new(3))}.slice(), 2, 3);
    fmt.Println(nestedKeyedˢ, len(nestedKeyed[0]), len(nestedKeyed[0][0]), nestedKeyed[0]);
    var structElem = GoReflect.WithElemDims(new array<withArray>[]{new withArray[]{}.array(2, () => new())}.slice(), 2);
    fmt.Println(structElemˢ, len(structElem[0]), len(structElem[0][1].A), len(structElem[0][1].A[0]), structElem[0]);
    var arrNested = new array<array<nint>>[]{new array<nint>[]{}.array(2, () => new(3))}.array(2, () => new(2, () => new(3)));
    fmt.Println(arrNestedˢ, len(arrNested), len(arrNested[0]), len(arrNested[0][0]), len(arrNested[1][0]), arrNested);
    var mapNested = new map<@string, array<array<nint>>>{["k"u8] = new array<nint>[]{}.array(2, () => new(3))};
    fmt.Println(mapNestedˢ, len(mapNested["k"u8, () => new array<array<nint>>(2, () => new(3))]), len(mapNested["k"u8, () => new array<array<nint>>(2, () => new(3))][0]), mapNested["k"u8, () => new array<array<nint>>(2, () => new(3))]);
    var sa = new main_sa[]{new()}.slice();
    fmt.Println((@string)"sa:"u8, len(sa), len(sa[0].A), len(sa[0].A[0]), sa[0]);
    var san = new withArray[]{new()}.slice();
    fmt.Println(sanˢ, len(san[0].A), len(san[0].A[0]), san[0]);
    var nnLit = new nn(new array<array<nint>>(2, () => new(3)));
    fmt.Println(nnLitˢ, len(nnLit), len(nnLit[0]), nnLit);
    var nnPtr = Ꮡ(new nn(new array<array<nint>>(2, () => new(3))));
    fmt.Println(nnPtrˢ, len(nnPtr.Value), len((nnPtr.Value)[0]), nnPtr.Value);
    var nnWrite = new nn(new array<array<nint>>(2, () => new(3)));
    nnWrite[1][2] = 9;
    fmt.Printf("nnWrite: %v\n"u8, nnWrite);
    var nsLit = new ns(new array<withArray>(2, () => new()));
    fmt.Println(nsLitˢ, len(nsLit), len(nsLit[0].A), len(nsLit[0].A[0]), nsLit);
    var nsPtr = Ꮡ(new ns(new array<withArray>(2, () => new())));
    fmt.Println(nsPtrˢ, len(nsPtr.Value), len((nsPtr.Value)[0].A), len((nsPtr.Value)[0].A[0]), nsPtr.Value);
    var nnElided = GoReflect.WithElemDims(new nn[]{new array<nint>[]{}.array(2, () => new(3))}.slice(), 2, 3);
    fmt.Println(nnElidedˢ, len(nnElided[0]), len(nnElided[0][0]), nnElided[0]);
    var nbLit = new nb(new byte[4].array());
    fmt.Println(nbLitˢ, len(nbLit), nbLit);
    var noLit = new no(new ni[2].array());
    fmt.Println(noLitˢ, len(noLit), len(noLit[0]), noLit);
    noLit[1][2] = 7;
    fmt.Printf("noWrite: %v\n"u8, noLit);
    var nnPop = new nn(new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array());
    fmt.Println(nnPopˢ, len(nnPop), len(nnPop[0]), nnPop);
    var nnShort = new nn(new array<nint>[]{new nint[]{1, 2, 3}.array()}.array(2, () => new(3)));
    fmt.Println(nnShortˢ, len(nnShort), len(nnShort[1]), nnShort);
    var nnKeyed = new nn(new array<array<nint>>(2, () => new(3)){[1] = new nint[]{7, 8, 9}.array()});
    fmt.Println(nnKeyedˢ, len(nnKeyed), len(nnKeyed[0]), nnKeyed);
    array<array<array<nint>>> ctrl = new(3, () => new(2, () => new(2)));
    fmt.Println(ctrlˢ, len(ctrl), len(ctrl[0]), len(ctrl[0][0]), ctrl);
    var ctrlLit = new array<array<nint>>[]{}.array(3, () => new(2, () => new(2)));
    fmt.Println(ctrlLitˢ, len(ctrlLit), len(ctrlLit[0]), len(ctrlLit[0][0]), ctrlLit);
}

} // end main_package
