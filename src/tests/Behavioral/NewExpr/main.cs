namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct point {
    internal nint x, y;
}

partial struct row /*[3]byte*/;

internal static nint answer() {
    return 42;
}


partial struct cfgᴛ1 /*dyn*/ {
    internal @string name;
    internal nint n;
}
internal static ж<cfgᴛ1> cfg = Ꮡ(new cfgᴛ1("cfg"u8, 3));

internal static ж<int64> limit = Ꮡ((int64)(1099511627776L));

internal static ж<nint> boxed(nint v) {
    return Ꮡ(v * 2);
}

partial struct holder {
    internal ж<@string> p;
}

internal static void Main() {
    var p = Ꮡ((nint)(42));
    var f = Ꮡ((float64)(1.5D));
    var s = Ꮡ((@string)("go"u8));
    var q = Ꮡ((int64)7);
    var r = Ꮡ(answer());
    var pt = new point(1, 2);
    var pp = Ꮡ(pt);
    var arr = new nint[]{1, 2, 3}.array();
    var pa = Ꮡ(arr.Clone());
    var rw = new row(new byte[]{9, 8, 7}.array());
    var pr = Ꮡ(rw.Clone());
    var raw = new byte[]{4, 5, 6}.array();
    var pc = Ꮡ(new row(raw.Clone()));
    (p.Value, pp.Value.x, pa.Value[0], pr.Value[0], pc.Value[0]) = (43, 10, 100, 1, 40);
    fmt.Println(p.Value, f.Value, s.Value, q.Value, r.Value);
    fmt.Println(pt, pp.Value, arr, pa.Value, rw, pr.Value, raw, pc.Value);
    fmt.Println((~cfg).name, (~cfg).n, limit.Value, boxed(21).Value, new holder(p: Ꮡ((@string)("field"u8))).p.Value);
    fmt.Printf("%T %T %T %T %T\n"u8, p.OrTypedNil(), f.OrTypedNil(), s.OrTypedNil(), pa.OrTypedNil(), pc.OrTypedNil());
}

} // end main_package
