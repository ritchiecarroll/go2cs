namespace go;

using json = encoding.json_package;
using xml = encoding.xml_package;
using fmt = fmt_package;
using reflect = reflect_package;
using encoding;

partial class main_package {

partial struct E {
    public nint Code;
}

public static @string Error(this E _) {
    return "e";
}

public static @string GoString(this E _) {
    return "E!";
}

partial struct S {
    public nint N;
}

public static @string String(this S _) {
    return "s";
}

partial struct w {
    /*embed*/ internal error error;
}

partial struct Pub {
    /*embed*/ public fmt_package.Stringer Stringer;
}

partial struct Named {
    public fmt.Stringer Stringer;
}

partial struct Tagged {
    /*embed*/ public fmt_package.Stringer Stringer; /*`json:"key"`*/
    public nint N;
}

partial struct Omitted {
    /*embed*/ public fmt_package.Stringer Stringer; /*`json:"-"`*/
    public nint N;
}

partial struct X {
    /*embed*/ internal error error;
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

internal static void Main() {
    describe("w", reflect.TypeOf(new w()));
    describe("Pub", reflect.TypeOf(new Pub()));
    describe("Named", reflect.TypeOf(new Named()));
    describe("X", reflect.TypeOf(new X()));

    visible("Pub", reflect.TypeOf(new Pub()));
    visible("X", reflect.TypeOf(new X()));

    var v = reflect.ValueOf(new w(new E(7)));
    fmt.Println("w.error CanInterface:", v.Field(0).CanInterface());
    fmt.Println("w.error.Elem CanInterface:", v.Field(0).Elem().CanInterface());
    fmt.Println("w.error.Elem.Code CanInterface:", v.Field(0).Elem().Field(0).CanInterface());
    fmt.Println("w.error.Elem.Code:", v.Field(0).Elem().Field(0).Int());

    var p = reflect.ValueOf(new Pub(new S(3)));
    fmt.Println("Pub.Stringer CanInterface:", p.Field(0).CanInterface());
    fmt.Println("Pub.Stringer.Elem.N CanInterface:", p.Field(0).Elem().Field(0).CanInterface());

    fmt.Printf("%v\n"u8, new w(new E(7)));
    fmt.Printf("%+v\n"u8, new Pub(new S(3)));
    fmt.Printf("%#v\n"u8, new w(new E(7)));
    fmt.Printf("%#v\n"u8, new Pub(new S(3)));
    fmt.Printf("%#v\n"u8, new Named(new S(3)));

    var twin = reflect.StructOf(new reflect.StructField[]{new(Name: "Stringer", Type: reflect.TypeOf((ж<fmt.Stringer>)nil).Elem(), Anonymous: true)}.slice());
    var plain = reflect.StructOf(new reflect.StructField[]{new(Name: "Stringer", Type: reflect.TypeOf((ж<fmt.Stringer>)nil).Elem())}.slice());
    fmt.Println("Pub -> anonymous twin:", reflect.TypeOf(new Pub()).ConvertibleTo(twin));
    fmt.Println("Pub -> named twin:", reflect.TypeOf(new Pub()).ConvertibleTo(plain));
    fmt.Println("Named -> anonymous twin:", reflect.TypeOf(new Named()).ConvertibleTo(twin));
    fmt.Println("Named -> named twin:", reflect.TypeOf(new Named()).ConvertibleTo(plain));
    fmt.Println("Pub -> Named:", reflect.TypeOf(new Pub()).ConvertibleTo(reflect.TypeOf(new Named())));

    foreach (var (_, value) in new any[]{new Pub(new S(3)), new Named(new S(3)), new Tagged(new S(3), 1), new Omitted(new S(3), 1), new Pub()}.slice()) {
        var (b, err) = json.Marshal(value);
        fmt.Printf("json %T: %s %v\n"u8, value, b, err);
    }

    foreach (var (_, value) in new any[]{new X(new E(7), 1), new Pub(new S(3)), new Named(new S(3))}.slice()) {
        var (b, err) = xml.Marshal(value);
        fmt.Printf("xml %T: %s %v\n"u8, value, b, err);
    }
}

} // end main_package
