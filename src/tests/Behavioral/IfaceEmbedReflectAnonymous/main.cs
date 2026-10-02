namespace go;

using json = encoding.json_package;
using xml = encoding.xml_package;
using fmt = fmt_package;
using reflect = reflect_package;
using encoding;

partial class main_package {

[GoType] partial struct E {
    public nint Code;
}

public static @string Error(this E _) {
    return "e"u8;
}

public static @string GoString(this E _) {
    return "E!"u8;
}

[GoType] partial struct S {
    public nint N;
}

public static @string String(this S _) {
    return "s"u8;
}

[GoType] partial struct w {
    [GoEmbedded] internal error error;
}

[GoType] partial struct Pub {
    [GoEmbedded] public fmt_package.Stringer Stringer;
}

[GoType] partial struct Named {
    public fmt.Stringer Stringer;
}

[GoType] partial struct Tagged {
    [GoTag(@"json:""key""")]
    [GoEmbedded] public fmt_package.Stringer Stringer;
    public nint N;
}

[GoType] partial struct Omitted {
    [GoTag(@"json:""-""")]
    [GoEmbedded] public fmt_package.Stringer Stringer;
    public nint N;
}

[GoType] partial struct X {
    [GoEmbedded] internal error error;
    public nint N;
}

internal static void describe(@string label, reflectꓸType t) {
    for (nint i = 0; i < t.NumField(); i++) {
        var f = t.Field(i);
        fmt.Printf("%s field %d: name=%s anonymous=%v exported=%v pkgpath=%q index=%v\n"u8, label, i, f.Name, f.Anonymous, f.IsExported(), f.PkgPath, f.Index);
    }
}

internal static void visible(@string label, reflectꓸType t) {
    foreach (var (_, f) in reflect.VisibleFields(t)) {
        fmt.Printf("%s visible: name=%s anonymous=%v index=%v\n"u8, label, f.Name, f.Anonymous, f.Index);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pubˢ = "Pub"u8;
private static readonly @string namedˢ = "Named"u8;
private static readonly object wErrorCanInterfaceˢ = (@string)"w.error CanInterface:"u8;
private static readonly object wErrorElemCanInterfaceˢ = (@string)"w.error.Elem CanInterface:"u8;
private static readonly object wErrorElemCodeˢ = (@string)"w.error.Elem.Code CanInterface:"u8;
private static readonly object wErrorElemCodeˢ2 = (@string)"w.error.Elem.Code:"u8;
private static readonly object pubStringerCanInterfaceˢ = (@string)"Pub.Stringer CanInterface:"u8;
private static readonly object pubStringerElemNˢ = (@string)"Pub.Stringer.Elem.N CanInterface:"u8;
private static readonly object pubAnonymousTwinˢ = (@string)"Pub -> anonymous twin:"u8;
private static readonly object pubNamedTwinˢ = (@string)"Pub -> named twin:"u8;
private static readonly object namedAnonymousTwinˢ = (@string)"Named -> anonymous twin:"u8;
private static readonly object namedNamedTwinˢ = (@string)"Named -> named twin:"u8;
private static readonly object pubNamedˢ = (@string)"Pub -> Named:"u8;

internal static void Main() {
    describe("w"u8, reflect.TypeOf(new w(nil)));
    describe(pubˢ, reflect.TypeOf(new Pub(nil)));
    describe(namedˢ, reflect.TypeOf(new Named(nil)));
    describe("X"u8, reflect.TypeOf(new X(nil)));
    visible(pubˢ, reflect.TypeOf(new Pub(nil)));
    visible("X"u8, reflect.TypeOf(new X(nil)));
    var v = reflect.ValueOf(new w(new E(7)));
    fmt.Println(wErrorCanInterfaceˢ, v.Field(0).CanInterface());
    fmt.Println(wErrorElemCanInterfaceˢ, v.Field(0).Elem().CanInterface());
    fmt.Println(wErrorElemCodeˢ, v.Field(0).Elem().Field(0).CanInterface());
    fmt.Println(wErrorElemCodeˢ2, v.Field(0).Elem().Field(0).Int());
    var p = reflect.ValueOf(new Pub(new S(3)));
    fmt.Println(pubStringerCanInterfaceˢ, p.Field(0).CanInterface());
    fmt.Println(pubStringerElemNˢ, p.Field(0).Elem().Field(0).CanInterface());
    fmt.Printf("%v\n"u8, new w(new E(7)));
    fmt.Printf("%+v\n"u8, new Pub(new S(3)));
    fmt.Printf("%#v\n"u8, new w(new E(7)));
    fmt.Printf("%#v\n"u8, new Pub(new S(3)));
    fmt.Printf("%#v\n"u8, new Named(new S(3)));
    var twin = reflect.StructOf(new reflect.StructField[]{new(Name: "Stringer"u8, Type: reflect.TypeOf(((ж<fmt.Stringer>)nil)).Elem(), Anonymous: true)}.slice());
    var plain = reflect.StructOf(new reflect.StructField[]{new(Name: "Stringer"u8, Type: reflect.TypeOf(((ж<fmt.Stringer>)nil)).Elem())}.slice());
    fmt.Println(pubAnonymousTwinˢ, reflect.TypeOf(new Pub(nil)).ConvertibleTo(twin));
    fmt.Println(pubNamedTwinˢ, reflect.TypeOf(new Pub(nil)).ConvertibleTo(plain));
    fmt.Println(namedAnonymousTwinˢ, reflect.TypeOf(new Named(nil)).ConvertibleTo(twin));
    fmt.Println(namedNamedTwinˢ, reflect.TypeOf(new Named(nil)).ConvertibleTo(plain));
    fmt.Println(pubNamedˢ, reflect.TypeOf(new Pub(nil)).ConvertibleTo(reflect.TypeOf(new Named(nil))));
    foreach (var (_, value) in new any[]{new Pub(new S(3)), new Named(new S(3)), new Tagged(new S(3), 1), new Omitted(new S(3), 1), new Pub(nil)}.slice()) {
        var (b, err) = json.Marshal(value);
        fmt.Printf("json %T: %s %v\n"u8, value, b, err);
    }
    foreach (var (_, value) in new any[]{new X(new E(7), 1), new Pub(new S(3)), new Named(new S(3))}.slice()) {
        var (b, err) = xml.Marshal(value);
        fmt.Printf("xml %T: %s %v\n"u8, value, b, err);
    }
}

} // end main_package
