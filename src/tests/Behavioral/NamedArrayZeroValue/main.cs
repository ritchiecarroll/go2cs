namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType("[2]array<nint>")] [GoArrayDims(2, 3)] partial struct nn;

[GoType] partial struct wa {
    internal array<nint> a = new(3);
    internal nint n;
}

[GoType("[2]wa")] partial struct ns;

[GoType("[4]byte")] partial struct nb;

[GoType("[3]nint")] partial struct ni;

[GoType("[2]ni")] partial struct no;

[GoType] partial struct holder {
    internal nn f;
    internal ns g;
}

internal static (nn r, ns s) namedResult() {
    nn r = default!;
    ns s = default!;

    return (r, s);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object varNNˢ = (@string)"varNN:"u8;
private static readonly object varNSˢ = (@string)"varNS:"u8;
private static readonly object varNBˢ = (@string)"varNB:"u8;
private static readonly object varNOˢ = (@string)"varNO:"u8;
private static readonly object newNNˢ = (@string)"newNN:"u8;
private static readonly object newNSˢ = (@string)"newNS:"u8;
private static readonly object newNBˢ = (@string)"newNB:"u8;
private static readonly object newNOˢ = (@string)"newNO:"u8;
private static readonly object fieldNNˢ = (@string)"fieldNN:"u8;
private static readonly object fieldNSˢ = (@string)"fieldNS:"u8;
private static readonly object resNNˢ = (@string)"resNN:"u8;
private static readonly object resNSˢ = (@string)"resNS:"u8;
private static readonly object arrNNˢ = (@string)"arrNN:"u8;
private static readonly object sliceNNˢ = (@string)"sliceNN:"u8;
private static readonly object mapNNˢ = (@string)"mapNN:"u8;
private static readonly object writeNNˢ = (@string)"writeNN:"u8;
private static readonly object writeNSˢ = (@string)"writeNS:"u8;
private static readonly object writePtrNNˢ = (@string)"writePtrNN:"u8;

internal static void Main() {
    nn d = default!;
    ns sv = default!;
    nb b = default!;
    no o = default!;
    fmt.Println(varNNˢ, len(d), len(d[0]), d);
    fmt.Println(varNSˢ, len(sv), len(sv[0].a), sv);
    fmt.Println(varNBˢ, len(b), b);
    fmt.Println(varNOˢ, len(o), len(o[0]), o);
    var pd = @new<nn>();
    var ps = @new<ns>();
    var pb = @new<nb>();
    var po = @new<no>();
    fmt.Println(newNNˢ, len(pd.Value), len((pd.Value)[0]), pd.Value);
    fmt.Println(newNSˢ, len(ps.Value), len((ps.Value)[0].a), ps.Value);
    fmt.Println(newNBˢ, len(pb.Value), pb.Value);
    fmt.Println(newNOˢ, len(po.Value), len((po.Value)[0]), po.Value);
    holder h = new();
    fmt.Println(fieldNNˢ, len(h.f), len(h.f[0]), h.f);
    fmt.Println(fieldNSˢ, len(h.g), len(h.g[0].a), h.g);
    var (rr, ss) = namedResult();
    fmt.Println(resNNˢ, len(rr), len(rr[0]), rr);
    fmt.Println(resNSˢ, len(ss), len(ss[0].a), ss);
    array<nn> aa = new(2);
    fmt.Println(arrNNˢ, len(aa), len(aa[0]), len(aa[0][0]), aa);
    var sl = GoReflect.WithElemDims(new slice<nn>(2), 2, 3);
    fmt.Println(sliceNNˢ, len(sl), len(sl[0]), len(sl[0][0]), sl);
    var mp = new map<nint, nn>();
    var mv = mp[7].Clone();
    fmt.Println(mapNNˢ, len(mv), len(mv[0]), mv);
    d[1][2] = 9;
    sv[1].a[2] = 8;
    (pd.Value)[0][1] = 5;
    fmt.Println(writeNNˢ, d);
    fmt.Println(writeNSˢ, sv);
    fmt.Println(writePtrNNˢ, pd.Value);
}

} // end main_package
