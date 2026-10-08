namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct box {
    internal @string label;
    internal nint n;
}

internal static nint Size(this ref box b) {
    return b.n;
}

internal static @string Name(this ref box b) {
    return b.label;
}

internal static void Main() {
    var b = Ꮡ(new box(label: "widget"u8, n: 42));
    fmt.Println(describe(new boxжdescribe_thing(b)));
    var c = Ꮡ(new box(label: "gadget"u8, n: 7));
    fmt.Println(describe(new boxжdescribe_thing(c)));
}

} // end main_package
