namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using runtime = runtime_package;
using sort = sort_package;

partial class main_package {

partial struct Direct /*map[nint, Direct]*/;

public static @string String(this Direct d) {
    return fmt.Sprintf("Direct(%d)"u8, len(d));
}

partial struct DirectStr /*map[@string, DirectStr]*/;

partial struct ViaStruct /*map[nint, viaStructV]*/;

public partial struct viaStructV {
    internal nint n;
    internal ViaStruct m;
}

partial struct ViaNested /*map[nint, viaNestedA]*/;

public partial struct viaNestedA {
    internal viaNestedB b;
}

partial struct viaNestedB {
    internal ViaNested m;
}

partial struct ViaArray /*map[nint, array<ViaArray>]*/;

partial struct ViaSlice /*map[nint, slice<ViaSlice>]*/;

partial struct ViaMap /*map[nint, map<nint, ViaMap>]*/;

partial struct ViaNamed /*map[nint, namedSlice]*/;

public partial struct namedSlice /*[]ViaNamed*/;

partial struct Generic<T> /*map[nint, Generic<T>]*/;

partial struct ViaPtr /*map[nint, ж<ViaPtr>]*/;

partial struct DefinedOverStruct /*viaStructV*/;

partial class PDirect /*ж<Direct>*/;

internal static void direct() {
    Direct nilD = default;
    fmt.Println("direct nil:", nilD == nil, len(nilD));

    var d = new Direct(new map<nint, Direct>{[1] = new Direct(new map<nint, Direct>{[2] = default}), [3] = new map<nint, Direct>{}});
    d[4] = new Direct(new map<nint, Direct>{[5] = new Direct(new map<nint, Direct>{[6] = default})});
    fmt.Println("direct len:", len(d), len(d[1]), len(d[4][5]), d[1][2] == nil, d[3] == nil);

    var (v, ok) = d[7, ꟷ];
    fmt.Println("direct missing:", v == nil, ok);

    delete(d, 3);
    var keys = new nint[]{}.slice();
    foreach (var (k, _) in d) {
        keys = append(keys, k);
    }
    sort.Ints(keys);
    fmt.Println("direct keys:", keys);

    var m = new Direct(4);
    m[0] = m;
    fmt.Println("direct self:", len(m[0][0][0]));
}

internal static void directStr() {
    var d = new DirectStr(new map<@string, DirectStr>{["a"] = new DirectStr(new map<@string, DirectStr>{["b"] = default}), ["c"] = new map<@string, DirectStr>{}});
    d["e"] = d["a"];
    var e = d["e"];
    e["f"] = default;
    fmt.Println("directStr:", len(d), len(d["a"]), d["a"]["b"] == nil, d["c"] == nil);
}

internal static void viaStruct() {
    var s = new ViaStruct(new map<nint, viaStructV>{[1] = new(n: 10, m: new ViaStruct(new map<nint, viaStructV>{[2] = new(n: 20)}))});
    fmt.Println("viaStruct:", len(s), s[1].n, s[1].m[2].n, s[1].m[2].m == nil);
}

internal static void viaNested() {
    var x = new ViaNested(new map<nint, viaNestedA>{[1] = new(b: new viaNestedB(m: new ViaNested(new map<nint, viaNestedA>{[2] = new()})))});
    fmt.Println("viaNested:", len(x), len(x[1].b.m), x[1].b.m[2].b.m == nil);
}

internal static void viaArray() {
    var a = new ViaArray(new map<nint, array<ViaArray>>{[1] = new ViaArray[]{new ViaArray(new map<nint, array<ViaArray>>{[2] = new ViaArray[]{}.array(2)}), default}.array()});
    fmt.Println("viaArray:", len(a), len(a[1, () => new array<ViaArray>(2)][0]), a[1, () => new array<ViaArray>(2)][1] == nil);
}

internal static void viaSlice() {
    var s = new ViaSlice(new map<nint, slice<ViaSlice>>{[1] = new ViaSlice[]{new ViaSlice(new map<nint, slice<ViaSlice>>{[2] = default}), default}.slice()});
    fmt.Println("viaSlice:", len(s), len(s[1]), len(s[1][0]), s[1][1] == nil);
}

internal static void viaMap() {
    var m = new ViaMap(new map<nint, map<nint, ViaMap>>{[1] = new map<nint, ViaMap>{[2] = new ViaMap(new map<nint, map<nint, ViaMap>>{[3] = default})}});
    fmt.Println("viaMap:", len(m), len(m[1]), len(m[1][2]), m[1][2][3] == nil);
}

internal static void viaNamed() {
    var n = new ViaNamed(new map<nint, namedSlice>{[1] = new namedSlice(new ViaNamed[]{new ViaNamed(new map<nint, namedSlice>{}), default}.slice())});
    fmt.Println("viaNamed:", len(n), len(n[1]), n[1][0] != nil, n[1][1] == nil);
}

internal static void generic() {
    var g = new Generic<@string>(new map<nint, Generic<@string>>{[1] = new Generic<@string>(new map<nint, Generic<@string>>{[2] = default})});
    fmt.Println("generic:", len(g), len(g[1]), g[1][2] == nil);
}

internal static void viaPtr() {
    ref var p = ref heap<ViaPtr>(out var Ꮡp);
    p = new ViaPtr(new map<nint, ж<ViaPtr>>{});
    p[1] = Ꮡp;
    fmt.Println("viaPtr:", len(p), len(p[1].ValueSlot), (p[1].ValueSlot)[1] == p[1]);
}

internal static void reflection() {
    ref var d = ref heap<Direct>(out var Ꮡd);
    d = new Direct(new map<nint, Direct>{[1] = new Direct(new map<nint, Direct>{[2] = default})});

    var t = reflect.TypeOf(d);
    fmt.Println("reflect type:", t.Kind(), AreEqual(t.Elem(), t), t.Key().Kind(), t.Size());
    fmt.Printf("reflect %%T: %T\n"u8, d);

    var v = reflect.ValueOf(d);
    fmt.Println("reflect len:", v.Len(), v.MapIndex(reflect.ValueOf((nint)1)).Len());

    v.SetMapIndex(reflect.ValueOf((nint)3), reflect.ValueOf(new Direct(new map<nint, Direct>{})));
    fmt.Println("reflect set:", len(d), d[3] != nil);

    nint n = 0;
    for (var iter = v.MapRange(); iter.Next(); ) {
        n++;
    }
    fmt.Println("reflect range:", n);

    fmt.Println("reflect deepequal:", reflect.DeepEqual(new Direct(new map<nint, Direct>{[1] = new Direct(new map<nint, Direct>{[2] = default})}), new Direct(new map<nint, Direct>{[1] = new Direct(new map<nint, Direct>{[2] = default})})), reflect.DeepEqual(new Direct(new map<nint, Direct>{[1] = default}), new Direct(new map<nint, Direct>{[2] = default})));

    var s = new DirectStr(new map<@string, DirectStr>{["a"] = new DirectStr(new map<@string, DirectStr>{["b"] = default})});
    fmt.Printf("fmt: %v %d\n"u8, s, len(s));

    var ds = new DefinedOverStruct(new viaStructV(n: 7, m: new ViaStruct(new map<nint, viaStructV>{[1] = new()})));
    var fv = reflect.ValueOf(ds);
    fmt.Println("reflect fields:", fv.NumField(), fv.Field(0).Int(), fv.Field(1).Len());

    fmt.Stringer st = d;
    var (back, ok) = st._<Direct>(ᐧ);
    fmt.Println("adapter:", st.String(), ok, len(back));

    var p = new PDirect(Ꮡd);
    runtime.SetFinalizer(p, (PDirect _) => {
    });
    runtime.SetFinalizer(p, default);
    fmt.Println("finalizer:", p != nil, len(d));
}

internal static void Main() {
    direct();
    directStr();
    viaStruct();
    viaNested();
    viaArray();
    viaSlice();
    viaMap();
    viaNamed();
    generic();
    viaPtr();
    reflection();
}

} // end main_package
