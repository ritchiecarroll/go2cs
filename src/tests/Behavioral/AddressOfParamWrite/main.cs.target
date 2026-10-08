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

public static nint BumpedRecv(this Box bʗp) {
    ref var b = ref heap(bʗp, out var Ꮡb);

    bumpBox(ref b);
    return b.Tag;
}

public static (nint, nint) ClippedRecv(this Box bʗp) {
    ref var b = ref heap(bʗp, out var Ꮡb);

    clip(ref b.R, 5, 5);
    return (b.R.Min, b.R.Max);
}

public static (nint, nint, nint) BumpedElemRecv(this Trio aʗp) {
    ref var a = ref heap(aʗp.Clone(), out var Ꮡa);

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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object paramˢ = (@string)"param       :"u8;
private static readonly object paramFieldˢ = (@string)"param field :"u8;
private static readonly object paramElemˢ = (@string)"param elem  :"u8;
private static readonly object paramRoˢ = (@string)"param ro    :"u8;
private static readonly object localˢ = (@string)"local       :"u8;
private static readonly object plainˢ = (@string)"plain       :"u8;
private static readonly object callerˢ = (@string)"caller      :"u8;
private static readonly object recvˢ = (@string)"recv        :"u8;
private static readonly object callerˢ2 = (@string)"caller:"u8;
private static readonly object recvFieldˢ = (@string)"recv field  :"u8;
private static readonly object recvElemˢ = (@string)"recv elem   :"u8;
private static readonly object recvSliceˢ = (@string)"recv slice  :"u8;
private static readonly object recvRoˢ = (@string)"recv ro     :"u8;
private static readonly object recvPtrˢ = (@string)"recv ptr    :"u8;
private static readonly object recvPlainˢ = (@string)"recv plain  :"u8;
private static readonly object recvViaPtrˢ = (@string)"recv via ptr:"u8;
private static readonly object recvIfaceˢ = (@string)"recv iface  :"u8;

internal static void Main() {
    var p = clipParam(new Rect(0, 16));
    fmt.Println(paramˢ, p.Min, p.Max);
    var b = bumpParamField(new Box(new Rect(1, 2), 3));
    fmt.Println(paramFieldˢ, b.R.Min, b.R.Max, b.Tag);
    var a = bumpParamElem(new nint[]{7, 8, 9}.array());
    fmt.Println(paramElemˢ, a[0], a[1], a[2]);
    var (lo, hi) = readOnlyParam(new Rect(3, 4));
    fmt.Println(paramRoˢ, lo, hi);
    var l = clipLocal();
    fmt.Println(localˢ, l.Min, l.Max);
    fmt.Println(plainˢ, plainParam(new Rect(2, 20)));
    var orig = new Rect(0, 16);
    var clipped = clipParam(orig);
    fmt.Println(callerˢ, orig.Min, orig.Max, (@string)"->"u8, clipped.Min, clipped.Max);
    var rb = new Box(new Rect(0, 16), 6);
    fmt.Println(recvˢ, rb.BumpedRecv(), callerˢ2, rb.Tag);
    var (rlo, rhi) = rb.ClippedRecv();
    fmt.Println(recvFieldˢ, rlo, rhi, callerˢ2, rb.R.Min, rb.R.Max);
    var rt = new Trio(new nint[]{7, 8, 9}.array());
    var (e0, e1, e2) = rt.BumpedElemRecv();
    fmt.Println(recvElemˢ, e0, e1, e2, callerˢ2, rt[0], rt[1], rt[2]);
    var rn = new Nums(new nint[]{1, 2, 3}.slice());
    fmt.Println(recvSliceˢ, rn.BumpedElemSliceRecv(), callerˢ2, rn[1]);
    var (rro, rrt) = rb.ReadOnlyRecv();
    fmt.Println(recvRoˢ, rro, rrt);
    ref var rp = ref heap<Box>(out var Ꮡrp);
    rp = new Box(new Rect(0, 16), 6);
    fmt.Println(recvPtrˢ, Ꮡrp.BumpedPtrRecv(), callerˢ2, rp.Tag);
    fmt.Println(recvPlainˢ, rb.PlainRecv());
    var rq = Ꮡ(new Box(new Rect(0, 16), 6));
    fmt.Println(recvViaPtrˢ, (~rq).BumpedRecv(), callerˢ2, (~rq).Tag);
    Bumper iface = new Box(new Rect(0, 16), 6);
    fmt.Println(recvIfaceˢ, iface.BumpedRecv());
}

} // end main_package
