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
    return ""u8;
}

partial struct typeError {
    public reflectꓸType Type;
}

internal static void show(@string name, reflectꓸType a, reflectꓸType b) {
    fmt.Printf("%-28s == %v  deep %v  (%v / %v)\n"u8, name, AreEqual(a, b), reflect.DeepEqual(a, b), a, b);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string structTypeOfTypeForˢ = "struct TypeOf/TypeFor"u8;
private static readonly @string structTypeOfXElemˢ = "struct TypeOf/&x.Elem"u8;
private static readonly @string structTypeForTypeForˢ = "struct TypeFor/TypeFor"u8;
private static readonly @string ptrTypeOfTypeForˢ = "ptr TypeOf/TypeFor"u8;
private static readonly @string ptrTypeOfXElemˢ = "ptr TypeOf/&x.Elem"u8;
private static readonly @string ptrTypeForPointerToˢ = "ptr TypeFor/PointerTo"u8;
private static readonly @string arrayTypeOfTypeForˢ = "array TypeOf/TypeFor"u8;
private static readonly @string arrayTypeOfXElemˢ = "array TypeOf/&x.Elem"u8;
private static readonly @string intTypeOfTypeForˢ = "int TypeOf/TypeFor"u8;
private static readonly @string valueTypeTypeForˢ = "value.Type/TypeFor"u8;
private static readonly @string anonStructˢ = "anon struct"u8;
private static readonly @string methodValueTypeTypeOfˢ = "method value Type/TypeOf"u8;
private static readonly @string methodValueTypeTypeForˢ = "method value Type/TypeFor"u8;
private static readonly object errorDeepEqualTypeOfˢ = (@string)"error DeepEqual TypeOf/TypeFor"u8;
private static readonly object mapKeyTypeForTypeOfˢ = (@string)"map key TypeFor->TypeOf"u8;
private static readonly object syncMapTypeOfTypeForˢ = (@string)"sync.Map TypeOf->TypeFor"u8;
private static readonly object typeForTypeOfˢ = (@string)"TypeFor->TypeOf"u8;

internal static void Main() {
    ref var s = ref heap(new T(), out var Ꮡs);
    ref var p = ref heap<ж<T>>(out var Ꮡp);
    ref var a = ref heap(new Arr(), out var Ꮡa);
    nint n = default!;

    show(structTypeOfTypeForˢ, reflect.TypeOf(s), reflect.TypeFor<T>());
    show(structTypeOfXElemˢ, reflect.TypeOf(s), reflect.TypeOf(Ꮡs).Elem());
    show(structTypeForTypeForˢ, reflect.TypeFor<T>(), reflect.TypeFor<T>());
    show(ptrTypeOfTypeForˢ, reflect.TypeOf(p.OrTypedNil()), reflect.TypeFor<ж<T>>());
    show(ptrTypeOfXElemˢ, reflect.TypeOf(p.OrTypedNil()), reflect.TypeOf(Ꮡp).Elem());
    show(ptrTypeForPointerToˢ, reflect.TypeFor<ж<T>>(), reflect.PointerTo(reflect.TypeFor<T>()));
    show(arrayTypeOfTypeForˢ, reflect.TypeOf(a), reflect.TypeFor<Arr>());
    show(arrayTypeOfXElemˢ, reflect.TypeOf(a), reflect.TypeOf(Ꮡa).Elem());
    show(intTypeOfTypeForˢ, reflect.TypeOf(n), reflect.TypeFor<nint>());
    show(valueTypeTypeForˢ, reflect.ValueOf(s).Type(), reflect.TypeFor<T>());
    show(anonStructˢ, reflect.TypeOf(new EmptyStruct()), reflect.TypeFor<EmptyStruct>());

    M mm = default!;
    var mt = reflect.ValueOf(mm).Method(0).Type();
    var mmʗ1 = mm;
    show(methodValueTypeTypeOfˢ, mt, reflect.TypeOf((nint p1) => mmʗ1.Do(p1)));
    show(methodValueTypeTypeForˢ, mt, reflect.TypeFor<Func<nint, @string>>());

    fmt.Println(errorDeepEqualTypeOfˢ, reflect.DeepEqual(Ꮡ(new typeError(reflect.TypeOf(s))), Ꮡ(new typeError(reflect.TypeFor<T>()))));

    var m = new map<reflectꓸType, nint>{[reflect.TypeFor<T>()] = 1, [reflect.TypeFor<ж<T>>()] = 2};
    var (_, ok1) = m[reflect.TypeOf(s), ꟷ];
    var (_, ok2) = m[reflect.TypeOf(p.OrTypedNil()), ꟷ];
    fmt.Println(mapKeyTypeForTypeOfˢ, ok1, ok2);

    ref var sm = ref heap(new Δsync.Map(), out var Ꮡsm);
    Ꮡsm.Store(reflect.TypeOf(s), (nint)(1));
    var (_, ok3) = Ꮡsm.Load(reflect.TypeFor<T>());
    Ꮡsm.Store(reflect.TypeFor<Arr>(), (nint)(2));
    var (_, ok4) = Ꮡsm.Load(reflect.TypeOf(a));
    fmt.Println(syncMapTypeOfTypeForˢ, ok3, typeForTypeOfˢ, ok4);
}

} // end main_package
