namespace go;

using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

partial struct mixed {
    public nint Exported;
    internal nint unexported;
    internal nint _;
    public @string Tagged; /*`probe:"yes"`*/
    internal @string secret;
}

partial struct allExported {
    public nint A;
    public nint B;
}

internal static void Main() {
    var t = reflect.TypeOf(new mixed(nil));

    for (nint i = 0; i < t.NumField(); i++) {
        var fΔ1 = t.Field(i);
        fmt.Printf("%d %-10s exported=%-5v pkgpath=%q tag=%q\n"u8, i, fΔ1.Name, fΔ1.IsExported(), fΔ1.PkgPath, ((@string)fΔ1.Tag));
    }

    var (f, ok) = t.FieldByName("secret");
    fmt.Println("byname secret:", ok, f.IsExported(), f.PkgPath);
    (f, ok) = t.FieldByName("Exported");
    fmt.Println("byname Exported:", ok, f.IsExported(), f.PkgPath);
    (_, ok) = t.FieldByName("absent");
    fmt.Println("byname absent:", ok);

    ref var m = ref heap(new mixed(), out var Ꮡm);
    var v = reflect.ValueOf(Ꮡm).Elem();
    for (nint i = 0; i < v.NumField(); i++) {
        fmt.Printf("%d canset=%-5v caninterface=%-5v typeExported=%-5v agree=%v\n"u8,
            i, v.Field(i).CanSet(), v.Field(i).CanInterface(), t.Field(i).IsExported(),
            v.Field(i).CanSet() == t.Field(i).IsExported());
    }

    fmt.Println("decode mixed:      ", decode(Ꮡm));
    ref var a = ref heap(new allExported(), out var Ꮡa);
    fmt.Println("decode allExported:", decode(Ꮡa), a);
}

internal static @string decode(any p) {
    var v = reflect.ValueOf(p).Elem();
    var t = v.Type();

    for (nint i = 0; i < t.NumField(); i++) {
        if (!t.Field(i).IsExported()) {
            return "structure error: struct contains unexported fields";
        }
    }

    for (nint i = 0; i < v.NumField(); i++) {
        v.Field(i).SetInt((int64)(i + 1));
    }

    return "<nil>";
}

} // end main_package
