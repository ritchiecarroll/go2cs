namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using Δsync = sync_package;

partial class main_package {

partial struct T {
    public nint X;
}

partial struct Arr /*[3]nint*/;

partial struct M {
}

public static @string Do(this M _Δp0, nint _Δp1) {
    return "";
}

partial struct typeError {
    public reflectꓸType Type;
}

internal static void show(@string name, reflectꓸType a, reflectꓸType b) {
    fmt.Printf("%-28s == %v  deep %v  (%v / %v)\n"u8, name, AreEqual(a, b), reflect.DeepEqual(a, b), a, b);
}

internal static void Main() {
    ref var s = ref heap(new T(), out var Ꮡs);
    ref var p = ref heap<ж<T>>(out var Ꮡp);
    ref var a = ref heap(new Arr(), out var Ꮡa);
    nint n = default!;

    show("struct TypeOf/TypeFor", reflect.TypeOf(s), reflect.TypeFor<T>());
    show("struct TypeOf/&x.Elem", reflect.TypeOf(s), reflect.TypeOf(Ꮡs).Elem());
    show("struct TypeFor/TypeFor", reflect.TypeFor<T>(), reflect.TypeFor<T>());
    show("ptr TypeOf/TypeFor", reflect.TypeOf(p.OrTypedNil()), reflect.TypeFor<ж<T>>());
    show("ptr TypeOf/&x.Elem", reflect.TypeOf(p.OrTypedNil()), reflect.TypeOf(Ꮡp).Elem());
    show("ptr TypeFor/PointerTo", reflect.TypeFor<ж<T>>(), reflect.PointerTo(reflect.TypeFor<T>()));
    show("array TypeOf/TypeFor", reflect.TypeOf(a), reflect.TypeFor<Arr>());
    show("array TypeOf/&x.Elem", reflect.TypeOf(a), reflect.TypeOf(Ꮡa).Elem());
    show("int TypeOf/TypeFor", reflect.TypeOf(n), reflect.TypeFor<nint>());
    show("value.Type/TypeFor", reflect.ValueOf(s).Type(), reflect.TypeFor<T>());
    show("anon struct", reflect.TypeOf(new EmptyStruct()), reflect.TypeFor<EmptyStruct>());

    M mm = default!;
    var mt = reflect.ValueOf(mm).Method(0).Type();
    var mmʗ1 = mm;
    show("method value Type/TypeOf", mt, reflect.TypeOf((nint p1) => mmʗ1.Do(p1)));
    show("method value Type/TypeFor", mt, reflect.TypeFor<Func<nint, @string>>());

    fmt.Println("error DeepEqual TypeOf/TypeFor", reflect.DeepEqual(Ꮡ(new typeError(reflect.TypeOf(s))), Ꮡ(new typeError(reflect.TypeFor<T>()))));

    var m = new map<reflectꓸType, nint>{[reflect.TypeFor<T>()] = 1, [reflect.TypeFor<ж<T>>()] = 2};
    var (_, ok1) = m[reflect.TypeOf(s), ꟷ];
    var (_, ok2) = m[reflect.TypeOf(p.OrTypedNil()), ꟷ];
    fmt.Println("map key TypeFor->TypeOf", ok1, ok2);

    ref var sm = ref heap(new Δsync.Map(), out var Ꮡsm);
    Ꮡsm.Store(reflect.TypeOf(s), (nint)(1));
    var (_, ok3) = Ꮡsm.Load(reflect.TypeFor<T>());
    Ꮡsm.Store(reflect.TypeFor<Arr>(), (nint)(2));
    var (_, ok4) = Ꮡsm.Load(reflect.TypeOf(a));
    fmt.Println("sync.Map TypeOf->TypeFor", ok3, "TypeFor->TypeOf", ok4);
}

} // end main_package
