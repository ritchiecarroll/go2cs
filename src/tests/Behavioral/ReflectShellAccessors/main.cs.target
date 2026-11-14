namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using sort = sort_package;
using typelib = ReflectShellAccessors.typelib_package;
using ReflectShellAccessors;

partial class main_package {

partial interface namer {
    @string Name();
}

internal static namer viaAny(any a) {
    return a._<namer>();
}

internal static void show(@string label, namer n) {
    fmt.Println(label, n.Name());
    fmt.Printf("%T\n"u8, n);
    fmt.Printf("%v\n"u8, n);
    fmt.Printf("%d\n"u8, n);
    fmt.Printf("%+v\n"u8, n);
}

internal static void reads(@string label, namer n) {
    var v = reflect.ValueOf(n);
    fmt.Println(label, v.Kind(), v.Type(), v.NumMethod(), v.IsZero());
    switch (v.Kind()) {
    case var ᴛ when ᴛ == reflect.ΔInt: {
        fmt.Println(v.Int(), v.CanInt());
        break;
    }
    case var ᴛ when ᴛ == reflect.ΔString: {
        fmt.Println(v.String(), v.Len(), v.Index(1).Uint());
        break;
    }
    case var ᴛ when ᴛ == reflect.ΔSlice: {
        fmt.Println(v.Len(), v.Cap(), v.Index(1).Int(), v.Slice(1, 3).Len(), v.IsNil());
        break;
    }
    case var ᴛ when ᴛ == reflect.Map: {
        fmt.Println(v.Len(), len(v.MapKeys()), v.MapIndex(reflect.ValueOf("a")).Int(), v.IsNil());
        var it = v.MapRange();
        while (it.Next()) {
            fmt.Println(it.Key().String(), it.Value().Int());
        }
        break;
    }
    case var ᴛ when ᴛ == reflect.Struct: {
        fmt.Println(v.NumField(), v.Field(0).String());
        break;
    }
    case var ᴛ when ᴛ == reflect.ΔPointer: {
        fmt.Println(v.IsNil(), v.Elem().Field(0).String());
        break;
    }}

}

internal static void round(@string label, namer n) {
    var v = reflect.ValueOf(n);
    var back = v.Interface();
    fmt.Println(label, back._<namer>().Name(), reflect.ValueOf(n) == reflect.ValueOf(n));
    fmt.Printf("%T\n"u8, back);
    ref var slot = ref heap<namer>(out var Ꮡslot);
    reflect.ValueOf(Ꮡslot).Elem().Set(v);
    fmt.Println(slot.Name());
}

internal static void original(@string label, namer n) {
    fmt.Println(label, AreEqual(reflect.ValueOf(n).Interface(), n));
}

internal static void each(@string label, namer n) {
    show(label, n);
    reads(label, n);
    round(label, n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string heyˢ = "hey"u8;

internal static void main() {
    namer vaList = new typelib_Listᴠnamer(typelib.NewList());
    each("va-slice", vaList);
    each("sh-slice", viaAny(typelib.NewList()));

    namer vaDict = new typelib_Dictᴠnamer(typelib.NewDict());
    each("va-map", vaDict);
    each("sh-map", viaAny(typelib.NewDict()));

    namer vaMark = new typelib_Markᴠnamer(typelib.NewMark());
    each("va-struct", vaMark);
    each("sh-struct", viaAny(typelib.NewMark()));
    original("va-struct", vaMark);
    original("sh-struct", viaAny(typelib.NewMark()));

    namer vaBox = new typelib_Boxжnamer(typelib.NewBox());
    each("va-pointer", vaBox);
    each("sh-pointer", viaAny(typelib.NewBox().OrTypedNil()));

    namer vaLabel = new typelib_Labelᴠnamer(((typelib.Label)(@string)heyˢ));
    each("va-string", vaLabel);
    each("sh-string", viaAny((typelib.Label)(@string)heyˢ));
    original("va-string", vaLabel);
    original("sh-string", viaAny((typelib.Label)(@string)heyˢ));

    namer vaCount = new typelib_Countᴠnamer(((typelib.Count)7));
    each("va-int", vaCount);
    each("sh-int", viaAny((typelib.Count)7));
    original("va-int", vaCount);
    original("sh-int", viaAny((typelib.Count)7));

    var vaSorted = typelib.NewList();
    namer vaHeld = new typelib_Listᴠnamer(vaSorted);
    var vaSortedʗ1 = vaSorted;
    sort.Slice(vaHeld, (nint i, nint j) => vaSortedʗ1[i] < vaSortedʗ1[j]);
    fmt.Println("va-sort", vaSorted);

    var shSorted = typelib.NewList();
    var shSortedʗ1 = shSorted;
    sort.Slice(viaAny(shSorted), (nint i, nint j) => shSortedʗ1[i] < shSortedʗ1[j]);
    fmt.Println("sh-sort", shSorted);
}

} // end main_package
