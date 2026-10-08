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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string secretˢ = "secret"u8;
private static readonly object bynameSecretˢ = (@string)"byname secret:"u8;
private static readonly @string exportedˢ = "Exported"u8;
private static readonly object bynameExportedˢ = (@string)"byname Exported:"u8;
private static readonly @string absentˢ = "absent"u8;
private static readonly object bynameAbsentˢ = (@string)"byname absent:"u8;
private static readonly object decodeMixedˢ = (@string)"decode mixed:      "u8;
private static readonly object decodeAllExportedˢ = (@string)"decode allExported:"u8;

internal static void Main() {
    var t = reflect.TypeOf(new mixed(nil));
    for (nint i = 0; i < t.NumField(); i++) {
        var fΔ1 = t.Field(i);
        fmt.Printf("%d %-10s exported=%-5v pkgpath=%q tag=%q\n"u8, i, fΔ1.Name, fΔ1.IsExported(), fΔ1.PkgPath, ((@string)fΔ1.Tag));
    }
    var (f, ok) = t.FieldByName(secretˢ);
    fmt.Println(bynameSecretˢ, ok, f.IsExported(), f.PkgPath);
    (f, ok) = t.FieldByName(exportedˢ);
    fmt.Println(bynameExportedˢ, ok, f.IsExported(), f.PkgPath);
    (_, ok) = t.FieldByName(absentˢ);
    fmt.Println(bynameAbsentˢ, ok);
    ref var m = ref heap(new mixed(), out var Ꮡm);
    var v = reflect.ValueOf(Ꮡm).Elem();
    for (nint i = 0; i < v.NumField(); i++) {
        fmt.Printf("%d canset=%-5v caninterface=%-5v typeExported=%-5v agree=%v\n"u8,
            i, v.Field(i).CanSet(), v.Field(i).CanInterface(), t.Field(i).IsExported(),
            v.Field(i).CanSet() == t.Field(i).IsExported());
    }
    fmt.Println(decodeMixedˢ, decode(Ꮡm));
    ref var a = ref heap(new allExported(), out var Ꮡa);
    fmt.Println(decodeAllExportedˢ, decode(Ꮡa), a);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string structureErrorStructˢ = "structure error: struct contains unexported fields"u8;
private static readonly @string nilˢ = "<nil>"u8;

internal static @string decode(any p) {
    var v = reflect.ValueOf(p).Elem();
    var t = v.Type();
    for (nint i = 0; i < t.NumField(); i++) {
        if (!t.Field(i).IsExported()) {
            return structureErrorStructˢ;
        }
    }
    for (nint i = 0; i < v.NumField(); i++) {
        v.Field(i).SetInt((int64)(i + 1));
    }
    return nilˢ;
}

} // end main_package
