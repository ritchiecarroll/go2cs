namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct S {
    public nint A;
    public @string B;
}

partial struct withArray {
    public array<array<nint>> A = new(2, () => new(3));
}

partial struct nb /*[4]byte*/;

/*[2][3]*/ partial struct nn /*[2]array<nint>*/;

partial struct ns /*[2]withArray*/;

partial struct ni /*[3]nint*/;

partial struct no /*[2]ni*/;

partial struct nsl /*[]nint*/;

partial struct nmp /*map[@string, nint]*/;

internal partial struct main_sa /*dyn*/ {
    public array<array<nint>> A = new(2, () => new(3));
}

internal static void Main() {
    var pa = slice<ж<array<byte>>>([Ꮡ(new byte[]{}.array(4))]);
    fmt.Println("pa:", len(pa), len(pa[0].Value), pa[0].Value);
    pa[0].Value[1] = 9;
    fmt.Printf("pa written: %v\n"u8, pa[0].Value);

    var paExplicit = slice<ж<array<byte>>>([Ꮡ(new byte[]{}.array(4))]);
    fmt.Println("paExplicit:", len(paExplicit), len(paExplicit[0].Value), paExplicit[0].Value);

    var paPop = slice<ж<array<byte>>>([Ꮡ(new byte[]{1, 2, 3, 4}.array())]);
    fmt.Println("paPop:", len(paPop[0].Value), paPop[0].Value);

    var paShort = slice<ж<array<byte>>>([Ꮡ(new byte[]{1, 2}.array(4))]);
    fmt.Println("paShort:", len(paShort[0].Value), paShort[0].Value);

    var pna = slice<ж<nb>>([Ꮡ(new nb(new byte[4].array()))]);
    var pnaExplicit = slice<ж<nb>>([Ꮡ(new nb(new byte[4].array()))]);
    fmt.Println("pna:", len(pna[0].Value), pna[0].Value);
    fmt.Println("pnaExplicit:", len(pnaExplicit[0].Value), pnaExplicit[0].Value);
    pna[0].Value[2] = 7;
    fmt.Printf("pna written: %v\n"u8, pna[0].Value);

    var pnsl = slice<ж<nsl>>([Ꮡ(new nsl(new nint[]{}.slice()))]);
    var pnslExplicit = slice<ж<nsl>>([Ꮡ(new nsl(new nint[]{}.slice()))]);
    fmt.Println("pnsl:", len(pnsl[0].ValueSlot), pnsl[0].ValueSlot == default, pnsl[0].ValueSlot);
    fmt.Println("pnslExplicit:", len(pnslExplicit[0].ValueSlot), pnslExplicit[0].ValueSlot);

    var pnmp = slice<ж<nmp>>([Ꮡ(new nmp(new map<@string, nint>{}))]);
    var pnmpExplicit = slice<ж<nmp>>([Ꮡ(new nmp(new map<@string, nint>{}))]);
    fmt.Println("pnmp:", len(pnmp[0].ValueSlot), pnmp[0].ValueSlot == default, pnmp[0].ValueSlot);
    fmt.Println("pnmpExplicit:", len(pnmpExplicit[0].ValueSlot), pnmpExplicit[0].ValueSlot);

    var pnaPop = slice<ж<nb>>([Ꮡ(new nb(new byte[]{1, 2, 3, 4}.array()))]);
    fmt.Println("pnaPop:", len(pnaPop[0].Value), pnaPop[0].Value);

    var pnn = slice<ж<nn>>([Ꮡ(new nn(new array<array<nint>>(2, () => new(3))))]);
    fmt.Println("pnn:", len(pnn[0].Value), len((pnn[0].Value)[0]), pnn[0].Value);
    var pnnExplicit = slice<ж<nn>>([Ꮡ(new nn(new array<array<nint>>(2, () => new(3))))]);
    fmt.Println("pnnExplicit:", len(pnnExplicit[0].Value), len((pnnExplicit[0].Value)[0]), pnnExplicit[0].Value);

    var mpn = new map<@string, ж<nb>>{["a"] = Ꮡ(new nb(new byte[4].array()))};
    fmt.Println("mpn:", len(mpn), len(mpn["a"].Value), mpn["a"].Value);
    var apn = new ж<nb>[]{Ꮡ(new nb(new byte[4].array())), Ꮡ(new nb(new byte[4].array()))}.array();
    fmt.Println("apn:", len(apn), len(apn[0].Value), apn[0].Value, apn[1].Value);

    var pnest = slice<ж<array<array<nint>>>>([Ꮡ(new array<nint>[]{}.array(2, () => new(3)))]);
    fmt.Println("pnest:", len(pnest[0].Value), len(pnest[0].Value[0]), pnest[0].Value);

    var ps = slice<ж<S>>([Ꮡ(new S())]);
    fmt.Println("ps:", len(ps), ps[0].Value);
    var ps2 = slice<ж<S>>([Ꮡ(new S(A: 7, B: "x"u8))]);
    fmt.Println("ps2:", ps2[0].Value);

    var psl = slice<ж<slice<nint>>>([Ꮡ(slice<nint>([]))]);
    fmt.Println("psl:", len(psl), len(psl[0].ValueSlot), psl[0].ValueSlot == default, psl[0].ValueSlot);

    var pm = slice<ж<map<@string, nint>>>([Ꮡ(new map<@string, nint>{})]);
    fmt.Println("pm:", len(pm), len(pm[0].ValueSlot), pm[0].ValueSlot == default, pm[0].ValueSlot);

    var mp = new map<@string, ж<array<nint>>>{["a"] = Ꮡ(new nint[]{}.array(2))};
    fmt.Println("mp:", len(mp), len(mp["a"].Value), mp["a"].Value);

    var ap = new ж<array<nint>>[]{Ꮡ(new nint[]{}.array(3)), Ꮡ(new nint[]{}.array(3))}.array();
    fmt.Println("ap:", len(ap), len(ap[0].Value), ap[0].Value, ap[1].Value);

    var nested = GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{}.array(2, () => new(3))}.slice(), 2, 3);
    fmt.Println("nested:", len(nested), len(nested[0]), len(nested[0][0]), nested[0]);
    nested[0][1][2] = 9;
    fmt.Printf("nested written: %v\n"u8, nested[0]);

    var nestedPop = GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array()}.slice(), 2, 3);
    fmt.Println("nestedPop:", len(nestedPop[0]), len(nestedPop[0][0]), nestedPop[0]);

    var nestedShort = GoReflect.WithElemDims(new array<array<nint>>[]{new array<nint>[]{new nint[]{1, 2, 3}.array()}.array(2, () => new(3))}.slice(), 2, 3);
    fmt.Println("nestedShort:", len(nestedShort[0]), len(nestedShort[0][1]), nestedShort[0]);

    var nestedKeyed = GoReflect.WithElemDims(new array<array<nint>>[]{new golib.SparseArray<array<nint>>{[1] = new nint[]{7, 8, 9}.array()}.array(2, () => new(3))}.slice(), 2, 3);
    fmt.Println("nestedKeyed:", len(nestedKeyed[0]), len(nestedKeyed[0][0]), nestedKeyed[0]);

    var structElem = GoReflect.WithElemDims(new array<withArray>[]{new withArray[]{}.array(2, () => new())}.slice(), 2);
    fmt.Println("structElem:", len(structElem[0]), len(structElem[0][1].A), len(structElem[0][1].A[0]), structElem[0]);

    var arrNested = new array<array<nint>>[]{new array<nint>[]{}.array(2, () => new(3))}.array(2, () => new(2, () => new(3)));
    fmt.Println("arrNested:", len(arrNested), len(arrNested[0]), len(arrNested[0][0]), len(arrNested[1][0]), arrNested);

    var mapNested = new map<@string, array<array<nint>>>{["k"] = new array<nint>[]{}.array(2, () => new(3))};
    fmt.Println("mapNested:", len(mapNested["k", () => new array<array<nint>>(2, () => new(3))]), len(mapNested["k", () => new array<array<nint>>(2, () => new(3))][0]), mapNested["k", () => new array<array<nint>>(2, () => new(3))]);

    var sa = slice<main_sa>([new()]);
    fmt.Println("sa:", len(sa), len(sa[0].A), len(sa[0].A[0]), sa[0]);

    var san = slice<withArray>([new()]);
    fmt.Println("san:", len(san[0].A), len(san[0].A[0]), san[0]);

    var nnLit = new nn(new array<array<nint>>(2, () => new(3)));
    fmt.Println("nnLit:", len(nnLit), len(nnLit[0]), nnLit);

    var nnPtr = Ꮡ(new nn(new array<array<nint>>(2, () => new(3))));
    fmt.Println("nnPtr:", len(nnPtr.Value), len((nnPtr.Value)[0]), nnPtr.Value);

    var nnWrite = new nn(new array<array<nint>>(2, () => new(3)));
    nnWrite[1][2] = 9;
    fmt.Printf("nnWrite: %v\n"u8, nnWrite);

    var nsLit = new ns(new array<withArray>(2, () => new()));
    fmt.Println("nsLit:", len(nsLit), len(nsLit[0].A), len(nsLit[0].A[0]), nsLit);

    var nsPtr = Ꮡ(new ns(new array<withArray>(2, () => new())));
    fmt.Println("nsPtr:", len(nsPtr.Value), len((nsPtr.Value)[0].A), len((nsPtr.Value)[0].A[0]), nsPtr.Value);

    var nnElided = GoReflect.WithElemDims(new nn[]{new array<nint>[]{}.array(2, () => new(3))}.slice(), 2, 3);
    fmt.Println("nnElided:", len(nnElided[0]), len(nnElided[0][0]), nnElided[0]);

    var nbLit = new nb(new byte[4].array());
    fmt.Println("nbLit:", len(nbLit), nbLit);

    var noLit = new no(new ni[2].array());
    fmt.Println("noLit:", len(noLit), len(noLit[0]), noLit);
    noLit[1][2] = 7;
    fmt.Printf("noWrite: %v\n"u8, noLit);

    var nnPop = new nn(new array<nint>[]{new nint[]{1, 2, 3}.array(), new nint[]{4, 5, 6}.array()}.array());
    fmt.Println("nnPop:", len(nnPop), len(nnPop[0]), nnPop);

    var nnShort = new nn(new array<nint>[]{new nint[]{1, 2, 3}.array()}.array(2, () => new(3)));
    fmt.Println("nnShort:", len(nnShort), len(nnShort[1]), nnShort);

    var nnKeyed = new nn(new array<array<nint>>(2, () => new(3)){[1] = new nint[]{7, 8, 9}.array()});
    fmt.Println("nnKeyed:", len(nnKeyed), len(nnKeyed[0]), nnKeyed);

    array<array<array<nint>>> ctrl = new(3, () => new(2, () => new(2)));
    fmt.Println("ctrl:", len(ctrl), len(ctrl[0]), len(ctrl[0][0]), ctrl);

    var ctrlLit = new array<array<nint>>[]{}.array(3, () => new(2, () => new(2)));
    fmt.Println("ctrlLit:", len(ctrlLit), len(ctrlLit[0]), len(ctrlLit[0][0]), ctrlLit);
}

} // end main_package
