namespace go;

using fmt = fmt_package;
using reflect = reflect_package;
using @unsafe = unsafe_package;

partial class main_package {

partial struct conf {
    internal @string name;
    internal bool trace;
    internal @string version;
    internal nint depth;
}

internal static ж<bool> boolFieldAddr(ж<conf> c, @string name) {
    var v = reflect.Indirect(reflect.ValueOf(c.OrTypedNil()));
    return (ж<bool>)(uintptr)(v.FieldByName(name).Addr().UnsafePointer());
}

internal static ж<@string> stringFieldAddr(ж<conf> c, @string name) {
    var v = reflect.Indirect(reflect.ValueOf(c.OrTypedNil()));
    return (ж<@string>)(uintptr)(v.FieldByName(name).Addr().UnsafePointer());
}

internal static ж<nint> intFieldAddr(ж<conf> c, @string name) {
    var v = reflect.Indirect(reflect.ValueOf(c.OrTypedNil()));
    return (ж<nint>)(uintptr)(v.FieldByName(name).Addr().UnsafePointer());
}

internal static void Main() {
    ref var c = ref heap<conf>(new conf(name: "cfg", version: "v0", depth: 3), out var Ꮡc);

    boolFieldAddr(Ꮡc, "trace").Value = true;
    stringFieldAddr(Ꮡc, "version").Value = "v1";
    intFieldAddr(Ꮡc, "depth").Value = 7;

    fmt.Println(c.name, c.trace, c.version, c.depth);

    var p1 = boolFieldAddr(Ꮡc, "trace");
    var p2 = boolFieldAddr(Ꮡc, "trace");
    p1.Value = false;
    fmt.Println(p1.Value, p2.Value, c.trace, p1 == p2);

    fmt.Println(c.name, c.version, c.depth);
}

} // end main_package
