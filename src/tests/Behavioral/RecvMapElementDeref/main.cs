namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct registry {
    internal map<@string, ж<nint>> handles;
}

internal static nint at(this ref registry r, @string key) {
    return r.handles[key].Value;
}

internal static void set(this ref registry r, @string key, nint val) {
    r.handles[key].Value = val;
}

internal static registry clone(this ref registry r) {
    return r;
}

internal static void Main() {
    ref var a = ref heap<nint>(out var Ꮡa);
    a = 1;
    ref var b = ref heap<nint>(out var Ꮡb);
    b = 2;
    var r = Ꮡ(new registry(handles: new map<@string, ж<nint>>{["a"] = Ꮡa, ["b"] = Ꮡb}));

    fmt.Println(r.at("a"), r.at("b"));

    r.set("a", 10);
    fmt.Println(r.at("a"), a);

    var c = r.clone();
    fmt.Println(len(c.handles));
}

} // end main_package
