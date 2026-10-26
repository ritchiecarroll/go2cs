namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Rect {
    public nint Min, Max;
}

partial struct Box {
    public Rect R;
    public nint Tag;
}

partial struct Nums /*[]nint*/;

partial struct Trio /*[3]nint*/;

internal static void clip(ref Rect r, nint lo, nint hi) {
    if (r.Min < lo) {
        r.Min = lo;
    }
    if (r.Max > hi) {
        r.Max = hi;
    }
}

internal static void bump(ref nint p) {
    p += 10;
}

internal static Rect clipParam(Rect r) {
    clip(ref r, 5, 5);
    return r;
}

internal static Box bumpParamField(Box b) {
    bump(ref b.R.Min);
    return b;
}

internal static array<nint> bumpParamElem(/*[3]*/ array<nint> a) {
    a = a.Clone();

    bump(ref a[1]);
    return a.Clone();
}

internal static (nint, nint) readOnlyParam(Rect rʗp) {
    ref var r = ref heap(rʗp, out var Ꮡr);

    var p = Ꮡr;
    return ((~p).Min, (~p).Max);
}

internal static Rect clipLocal() {
    var r = new Rect(0, 16);
    clip(ref r, 5, 5);
    return r;
}

internal static nint plainParam(Rect r) {
    return r.Max - r.Min;
}

internal static void bumpBox(ref Box b) {
    b.Tag += 10;
}

public static nint BumpedRecv(this Box b) {
    bumpBox(ref b);
    return b.Tag;
}

public static (nint, nint) ClippedRecv(this Box b) {
    clip(ref b.R, 5, 5);
    return (b.R.Min, b.R.Max);
}

public static (nint, nint, nint) BumpedElemRecv(this Trio a) {
    a = a.Clone();

    bump(ref a[1]);
    return (a[0], a[1], a[2]);
}

public static nint BumpedElemSliceRecv(this Nums n) {
    bump(ref n[1]);
    return n[1];
}

public static (nint, nint) ReadOnlyRecv(this Box bʗp) {
    ref var b = ref heap(bʗp, out var Ꮡb);

    var p = Ꮡb;
    return ((~p).R.Min, (~p).Tag);
}

public static nint BumpedPtrRecv(this ж<Box> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    bumpBox(ref (Ꮡb).DerefOrNull());
    return b.Tag;
}

public static nint PlainRecv(this Box b) {
    return b.Tag;
}

partial interface Bumper {
    nint BumpedRecv();
}

internal static void Main() {
    var p = clipParam(new Rect(0, 16));
    fmt.Println("param       :", p.Min, p.Max);

    var b = bumpParamField(new Box(new Rect(1, 2), 3));
    fmt.Println("param field :", b.R.Min, b.R.Max, b.Tag);

    var a = bumpParamElem(new nint[]{7, 8, 9}.array());
    fmt.Println("param elem  :", a[0], a[1], a[2]);

    var (lo, hi) = readOnlyParam(new Rect(3, 4));
    fmt.Println("param ro    :", lo, hi);

    var l = clipLocal();
    fmt.Println("local       :", l.Min, l.Max);

    fmt.Println("plain       :", plainParam(new Rect(2, 20)));

    var orig = new Rect(0, 16);
    var clipped = clipParam(orig);
    fmt.Println("caller      :", orig.Min, orig.Max, "->", clipped.Min, clipped.Max);

    var rb = new Box(new Rect(0, 16), 6);
    fmt.Println("recv        :", rb.BumpedRecv(), "caller:", rb.Tag);

    var (rlo, rhi) = rb.ClippedRecv();
    fmt.Println("recv field  :", rlo, rhi, "caller:", rb.R.Min, rb.R.Max);

    var rt = new Trio(new nint[]{7, 8, 9}.array());
    var (e0, e1, e2) = rt.BumpedElemRecv();
    fmt.Println("recv elem   :", e0, e1, e2, "caller:", rt[0], rt[1], rt[2]);

    var rn = new Nums(new nint[]{1, 2, 3}.slice());
    fmt.Println("recv slice  :", rn.BumpedElemSliceRecv(), "caller:", rn[1]);

    var (rro, rrt) = rb.ReadOnlyRecv();
    fmt.Println("recv ro     :", rro, rrt);

    ref var rp = ref heap<Box>(out var Ꮡrp);
    rp = new Box(new Rect(0, 16), 6);
    fmt.Println("recv ptr    :", Ꮡrp.BumpedPtrRecv(), "caller:", rp.Tag);

    fmt.Println("recv plain  :", rb.PlainRecv());

    var rq = Ꮡ(new Box(new Rect(0, 16), 6));
    fmt.Println("recv via ptr:", (~rq).BumpedRecv(), "caller:", (~rq).Tag);

    Bumper iface = new Box(new Rect(0, 16), 6);
    fmt.Println("recv iface  :", iface.BumpedRecv());
}

} // end main_package
