namespace go;

using fmt = fmt_package;

partial class main_package {

/*[2][3]*/ partial struct nn /*[2]array<nint>*/;

partial struct wa {
    internal array<nint> a = new(3);
    internal nint n;
}

partial struct ns /*[2]wa*/;

partial struct nb /*[4]byte*/;

partial struct ni /*[3]nint*/;

partial struct no /*[2]ni*/;

partial struct holder {
    internal nn f;
    internal ns g;
}

internal static (nn r, ns s) namedResult() {
    nn r = default;
    ns s = default;

    return (r, s);
}

internal static void Main() {
    nn d = default;
    ns sv = default;
    nb b = default;
    no o = default;
    fmt.Println("varNN:", len(d), len(d[0]), d);
    fmt.Println("varNS:", len(sv), len(sv[0].a), sv);
    fmt.Println("varNB:", len(b), b);
    fmt.Println("varNO:", len(o), len(o[0]), o);

    var pd = @new<nn>();
    var ps = @new<ns>();
    var pb = @new<nb>();
    var po = @new<no>();
    fmt.Println("newNN:", len(pd.Value), len((pd.Value)[0]), pd.Value);
    fmt.Println("newNS:", len(ps.Value), len((ps.Value)[0].a), ps.Value);
    fmt.Println("newNB:", len(pb.Value), pb.Value);
    fmt.Println("newNO:", len(po.Value), len((po.Value)[0]), po.Value);

    holder h = new();
    fmt.Println("fieldNN:", len(h.f), len(h.f[0]), h.f);
    fmt.Println("fieldNS:", len(h.g), len(h.g[0].a), h.g);

    var (rr, ss) = namedResult();
    fmt.Println("resNN:", len(rr), len(rr[0]), rr);
    fmt.Println("resNS:", len(ss), len(ss[0].a), ss);

    array<nn> aa = new(2);
    fmt.Println("arrNN:", len(aa), len(aa[0]), len(aa[0][0]), aa);

    var sl = GoReflect.WithElemDims(new slice<nn>(2), 2, 3);
    fmt.Println("sliceNN:", len(sl), len(sl[0]), len(sl[0][0]), sl);
    var mp = new map<nint, nn>();
    var mv = mp[7].Clone();
    fmt.Println("mapNN:", len(mv), len(mv[0]), mv);

    d[1][2] = 9;
    sv[1].a[2] = 8;
    (pd.Value)[0][1] = 5;
    fmt.Println("writeNN:", d);
    fmt.Println("writeNS:", sv);
    fmt.Println("writePtrNN:", pd.Value);
}

} // end main_package
