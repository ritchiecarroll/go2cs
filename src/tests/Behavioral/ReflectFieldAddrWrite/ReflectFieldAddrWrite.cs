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

internal static ж<bool> boolFieldAddr(ж<conf> Ꮡc, @string name) {
    var v = reflect.Indirect(reflect.ValueOf(Ꮡc.OrTypedNil()));
    return (ж<bool>)(uintptr)(v.FieldByName(name).Addr().UnsafePointer());
}

internal static ж<@string> stringFieldAddr(ж<conf> Ꮡc, @string name) {
    var v = reflect.Indirect(reflect.ValueOf(Ꮡc.OrTypedNil()));
    return (ж<@string>)(uintptr)(v.FieldByName(name).Addr().UnsafePointer());
}

internal static ж<nint> intFieldAddr(ж<conf> Ꮡc, @string name) {
    var v = reflect.Indirect(reflect.ValueOf(Ꮡc.OrTypedNil()));
    return (ж<nint>)(uintptr)(v.FieldByName(name).Addr().UnsafePointer());
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string traceˢ = "trace"u8;
private static readonly @string versionˢ = "version"u8;
private static readonly @string depthˢ = "depth"u8;

internal static void Main() {
    ref var c = ref heap<conf>(out var Ꮡc);
    c = new conf(name: "cfg"u8, version: "v0"u8, depth: 3);
    boolFieldAddr(Ꮡc, traceˢ).Value = true;
    stringFieldAddr(Ꮡc, versionˢ).Value = "v1"u8;
    intFieldAddr(Ꮡc, depthˢ).Value = 7;
    fmt.Println(c.name, c.trace, c.version, c.depth);
    var p1 = boolFieldAddr(Ꮡc, traceˢ);
    var p2 = boolFieldAddr(Ꮡc, traceˢ);
    p1.Value = false;
    fmt.Println(p1.Value, p2.Value, c.trace, p1 == p2);
    fmt.Println(c.name, c.version, c.depth);
}

} // end main_package
