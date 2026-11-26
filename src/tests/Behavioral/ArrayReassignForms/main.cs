namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct N /*[4]nint*/;

partial struct S {
    internal array<nint> arr = new(4);
    internal nint n;
}

partial struct E {
    internal array<nint> v = new(2);
    internal nint n;
}

internal static (array<nint>, array<nint>) pair() { return (array<nint>([1, 2, 3, 4]), array<nint>([5, 6, 7, 8])); }

internal static (array<nint>, error) withErr() { return (array<nint>([9, 9, 9, 9]), default); }

internal static (S, error) structs() { return (new S(array<nint>([4, 3, 2, 1]), 7), default); }

internal static void main() {
    var (a, b) = (array<nint>([1, 1, 1, 1]), array<nint>([2, 2, 2, 2]));
    var (sa, sb) = (a[..], b[..]);
    (var aᴛ1, var bᴛ1) = (b.Clone(), a.Clone());
    a.Assign(aᴛ1);
    b.Assign(bᴛ1);
    fmt.Println("tuple swap:", sa, sb);

    var (c, d) = (new nint[]{}.array(4), new nint[]{}.array(4));
    var (sc, sd) = (c[..], d[..]);
    (var cᴛ1, var dᴛ1) = pair();
    c.Assign(cᴛ1);
    d.Assign(dᴛ1);
    fmt.Println("tuple call:", sc, sd);

    var m = new map<@string, array<nint>>{["k"] = array<nint>([3, 3, 3, 3])};
    var e = new nint[]{}.array(4);
    var se = e[..];
    bool ok = default;
    (var eᴛ1, ok) = m["k", () => new array<nint>(4), ꟷ];
    e.Assign(eᴛ1);
    fmt.Println("comma-ok:", se, ok);

    var f = new nint[]{}.array(4);
    var sf = f[..];
    (var fᴛ1, var err) = withErr();
    f.Assign(fᴛ1);
    fmt.Println("redeclare:", sf, err);

    var g = new nint[]{}.array(4);
    var sg = g[..];
    foreach (var (_, vᴛ1) in range(GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 2, 3, 4]), array<nint>([5, 6, 7, 8])}.slice(), 4))) {
        g.Assign(vᴛ1);

    }
    fmt.Println("range:", sg);

    var ch = new channel<array<nint>>(2, ChanCargo.Of(null, new nint[] { 4 }));
    ch.ᐸꟷ(array<nint>([4, 4, 4, 4]));
    ch.ᐸꟷ(array<nint>([6, 6, 6, 6]));
    var h = new nint[]{}.array(4);
    var sh = h[..];
    h.Assign(ᐸꟷ(ch));
    fmt.Println("receive:", sh);
    var chᴛ1 = ch;
    switch (builtin.trySelect(ᐸꟷ(chᴛ1, ꓸꓸꓸ))) {
    case 0 when chᴛ1.ꟷᐳ(out var hᴛ1): {
        h.Assign(hᴛ1);
        break;
    }
    default: {
        break;
    }}
    fmt.Println("select:", sh);

    var arrs = GoReflect.WithElemDims(new array<nint>[]{new nint[]{}.array(4), new nint[]{}.array(4), new nint[]{}.array(4)}.slice(), 4);
    var s0 = arrs[0][..];
    nint i = 0;
    var other = array<nint>([8, 8, 8, 8]);
    ref var arrsᴛ1 = ref arrs[i];
    (i, var arrsᴛ2) = (2, other.Clone());
    arrsᴛ1.Assign(arrsᴛ2);
    fmt.Println("tuple order:", i, s0, arrs);

    ref var hh = ref heap<array<array<nint>>>(new array<nint>[]{array<nint>([1, 1, 1, 1]), array<nint>([2, 2, 2, 2])}.array(), out var Ꮡhh);
    var hp = Ꮡhh;
    var sh0 = hh[0][..];
    ref var hpᴛ1 = ref (hp.Value)[0];
    ref var hpᴛ3 = ref (hp.Value)[1];
    (var hpᴛ2, var hpᴛ4) = ((hp.Value)[1].Clone(), (hp.Value)[0].Clone());
    hpᴛ1.Assign(hpᴛ2);
    hpᴛ3.Assign(hpᴛ4);
    fmt.Println("tuple through a pointer:", sh0, hh);

    S st = new();
    var sst = st.arr[..];
    (var stᴛ1, err) = structs();
    st.AssignFrom(stᴛ1);
    fmt.Println("struct tuple:", sst, st.n, err);

    var w = GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 1]), array<nint>([2, 2]), array<nint>([3, 3])}.slice(), 2);
    var p = Ꮡ(array<array<nint>>.Alias(GoReflect.WithElemDims(w[1..], 2), 2));
    var q = Ꮡ(array<array<nint>>.Alias(w, 2));
    p.Assign(q.Value);
    fmt.Println("overlap up:", w);
    w = GoReflect.WithElemDims(new array<nint>[]{array<nint>([1, 1]), array<nint>([2, 2]), array<nint>([3, 3])}.slice(), 2);
    p = Ꮡ(array<array<nint>>.Alias(GoReflect.WithElemDims(w[1..], 2), 2));
    q = Ꮡ(array<array<nint>>.Alias(w, 2));
    q.Assign(p.Value);
    fmt.Println("overlap down:", w);

    N n = default;
    var x = ((array<nint>)n).Clone();
    x[0] = 9;
    array<nint> y = new(4);
    var n2 = ((N)y).Clone();
    n2[1] = 7;
    fmt.Println("conversion:", n[0], y[1], x[0], n2[1]);

    ref var na = ref heap(new array<array<nint>>(2, () => new(2)), out var Ꮡna);
    var pna = Ꮡna.at<array<nint>>(1);
    na.Assign(new array<nint>[]{array<nint>([1, 2]), array<nint>([3, 4])}.array());
    fmt.Println("nested element:", pna.Value);

    ref var ea = ref heap(new array<E>(2, () => new()), out var Ꮡea);
    var pv = Ꮡea.at<E>(1).Ꮡv;
    ea.Assign(array<E>([new(array<nint>([1, 2]), 5), new(array<nint>([3, 4]), 6)]));
    fmt.Println("struct element:", pv.Value, ea[1].n);

    S hs = new();
    var shs = hs.arr[..];
    hs.AssignFrom(new S(array<nint>([1, 2, 3, 4]), 3));
    fmt.Println("struct:", shs, hs.n);
    N nn = default;
    var snn = nn[..];
    nn.Assign(new N(new nint[]{5, 6, 7, 8}.array()));
    fmt.Println("named:", snn);

    ref var z = ref heap<array<nint>>(new nint[]{}.array(4), out var Ꮡz);
    var sz = z[..];
    reflect.ValueOf(Ꮡz).Elem().Set(reflect.ValueOf(array<nint>([1, 2, 3, 4])));
    fmt.Println("reflect set:", sz);
}

} // end main_package
