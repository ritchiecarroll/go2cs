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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object directNilˢ = (@string)"direct nil:"u8;
private static readonly object directLenˢ = (@string)"direct len:"u8;
private static readonly object directMissingˢ = (@string)"direct missing:"u8;
private static readonly object directKeysˢ = (@string)"direct keys:"u8;
private static readonly object directSelfˢ = (@string)"direct self:"u8;

internal static void direct() {
    Direct nilD = default!;
    fmt.Println(directNilˢ, nilD == default!, len(nilD));
    var d = new Direct(new map<nint, Direct>{[1] = new Direct(new map<nint, Direct>{[2] = default!}), [3] = new map<nint, Direct>{}});
    d[4] = new Direct(new map<nint, Direct>{[5] = new Direct(new map<nint, Direct>{[6] = default!})});
    fmt.Println(directLenˢ, len(d), len(d[1]), len(d[4][5]), d[1][2] == default!, d[3] == default!);
    var (v, ok) = d[7, ꟷ];
    fmt.Println(directMissingˢ, v == default!, ok);
    delete(d, 3);
    var keys = new nint[]{}.slice();
    foreach (var (k, _) in d) {
        keys = append(keys, k);
    }
    sort.Ints(keys);
    fmt.Println(directKeysˢ, keys);
    var m = new Direct(4);
    m[0] = m;
    fmt.Println(directSelfˢ, len(m[0][0][0]));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object directStrˢ = (@string)"directStr:"u8;

internal static void directStr() {
    var d = new DirectStr(new map<@string, DirectStr>{["a"u8] = new DirectStr(new map<@string, DirectStr>{["b"u8] = default!}), ["c"u8] = new map<@string, DirectStr>{}});
    d["e"u8] = d["a"u8];
    var e = d["e"u8];
    e["f"u8] = default!;
    fmt.Println(directStrˢ, len(d), len(d["a"u8]), d["a"u8]["b"u8] == default!, d["c"u8] == default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object viaStructˢ = (@string)"viaStruct:"u8;

internal static void viaStruct() {
    var s = new ViaStruct(new map<nint, viaStructV>{[1] = new(n: 10, m: new ViaStruct(new map<nint, viaStructV>{[2] = new(n: 20)}))});
    fmt.Println(viaStructˢ, len(s), s[1].n, s[1].m[2].n, s[1].m[2].m == default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object viaNestedˢ = (@string)"viaNested:"u8;

internal static void viaNested() {
    var x = new ViaNested(new map<nint, viaNestedA>{[1] = new(b: new viaNestedB(m: new ViaNested(new map<nint, viaNestedA>{[2] = new()})))});
    fmt.Println(viaNestedˢ, len(x), len(x[1].b.m), x[1].b.m[2].b.m == default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object viaArrayˢ = (@string)"viaArray:"u8;

internal static void viaArray() {
    var a = new ViaArray(new map<nint, array<ViaArray>>{[1] = new ViaArray[]{new ViaArray(new map<nint, array<ViaArray>>{[2] = new ViaArray[]{}.array(2)}), default!}.array()});
    fmt.Println(viaArrayˢ, len(a), len(a[1, () => new array<ViaArray>(2)][0]), a[1, () => new array<ViaArray>(2)][1] == default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object viaSliceˢ = (@string)"viaSlice:"u8;

internal static void viaSlice() {
    var s = new ViaSlice(new map<nint, slice<ViaSlice>>{[1] = new ViaSlice[]{new ViaSlice(new map<nint, slice<ViaSlice>>{[2] = default!}), default!}.slice()});
    fmt.Println(viaSliceˢ, len(s), len(s[1]), len(s[1][0]), s[1][1] == default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object viaMapˢ = (@string)"viaMap:"u8;

internal static void viaMap() {
    var m = new ViaMap(new map<nint, map<nint, ViaMap>>{[1] = new map<nint, ViaMap>{[2] = new ViaMap(new map<nint, map<nint, ViaMap>>{[3] = default!})}});
    fmt.Println(viaMapˢ, len(m), len(m[1]), len(m[1][2]), m[1][2][3] == default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object viaNamedˢ = (@string)"viaNamed:"u8;

internal static void viaNamed() {
    var n = new ViaNamed(new map<nint, namedSlice>{[1] = new namedSlice(new ViaNamed[]{new ViaNamed(new map<nint, namedSlice>{}), default!}.slice())});
    fmt.Println(viaNamedˢ, len(n), len(n[1]), n[1][0] != default!, n[1][1] == default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object genericˢ = (@string)"generic:"u8;

internal static void generic() {
    var g = new Generic<@string>(new map<nint, Generic<@string>>{[1] = new Generic<@string>(new map<nint, Generic<@string>>{[2] = default!})});
    fmt.Println(genericˢ, len(g), len(g[1]), g[1][2] == default!);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object viaPtrˢ = (@string)"viaPtr:"u8;

internal static void viaPtr() {
    ref var p = ref heap<ViaPtr>(out var Ꮡp);
    p = new ViaPtr(new map<nint, ж<ViaPtr>>{});
    p[1] = Ꮡp;
    fmt.Println(viaPtrˢ, len(p), len(p[1].ValueSlot), (p[1].ValueSlot)[1] == p[1]);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object reflectTypeˢ = (@string)"reflect type:"u8;
private static readonly object reflectLenˢ = (@string)"reflect len:"u8;
private static readonly object reflectSetˢ = (@string)"reflect set:"u8;
private static readonly object reflectRangeˢ = (@string)"reflect range:"u8;
private static readonly object reflectDeepequalˢ = (@string)"reflect deepequal:"u8;
private static readonly object reflectFieldsˢ = (@string)"reflect fields:"u8;
private static readonly object adapterˢ = (@string)"adapter:"u8;
private static readonly object finalizerˢ = (@string)"finalizer:"u8;

internal static void reflection() {
    ref var d = ref heap<Direct>(out var Ꮡd);
    d = new Direct(new map<nint, Direct>{[1] = new Direct(new map<nint, Direct>{[2] = default!})});
    var t = reflect.TypeOf(d);
    fmt.Println(reflectTypeˢ, t.Kind(), AreEqual(t.Elem(), t), t.Key().Kind(), t.Size());
    fmt.Printf("reflect %%T: %T\n"u8, d);
    var v = reflect.ValueOf(d);
    fmt.Println(reflectLenˢ, v.Len(), v.MapIndex(reflect.ValueOf((nint)(1))).Len());
    v.SetMapIndex(reflect.ValueOf((nint)(3)), reflect.ValueOf(new Direct(new map<nint, Direct>{})));
    fmt.Println(reflectSetˢ, len(d), d[3] != default!);
    nint n = 0;
    for (var iter = v.MapRange(); iter.Next(); ) {
        n++;
    }
    fmt.Println(reflectRangeˢ, n);
    fmt.Println(reflectDeepequalˢ, reflect.DeepEqual(new Direct(new map<nint, Direct>{[1] = new Direct(new map<nint, Direct>{[2] = default!})}), new Direct(new map<nint, Direct>{[1] = new Direct(new map<nint, Direct>{[2] = default!})})), reflect.DeepEqual(new Direct(new map<nint, Direct>{[1] = default!}), new Direct(new map<nint, Direct>{[2] = default!})));
    var s = new DirectStr(new map<@string, DirectStr>{["a"u8] = new DirectStr(new map<@string, DirectStr>{["b"u8] = default!})});
    fmt.Printf("fmt: %v %d\n"u8, s, len(s));
    var ds = new DefinedOverStruct(new viaStructV(n: 7, m: new ViaStruct(new map<nint, viaStructV>{[1] = new()})));
    var fv = reflect.ValueOf(ds);
    fmt.Println(reflectFieldsˢ, fv.NumField(), fv.Field(0).Int(), fv.Field(1).Len());
    fmt.Stringer st = d;
    var (back, ok) = st._<Direct>(ᐧ);
    fmt.Println(adapterˢ, st.String(), ok, len(back));
    var p = new PDirect(Ꮡd);
    runtime.SetFinalizer(p, (PDirect _) => {
    });
    runtime.SetFinalizer(p, default!);
    fmt.Println(finalizerˢ, p != nil, len(d));
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
