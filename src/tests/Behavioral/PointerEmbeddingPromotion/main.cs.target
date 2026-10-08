namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct leaf {
    internal nint n;
}

internal static void bump(this ref leaf l) {
    l.n++;
}

internal static nint get(this ref leaf l) {
    return l.n;
}

partial struct holder {
    internal partial ref ж<leaf> leaf { get; }
    internal @string tag;
}

partial struct mid {
    internal partial ref ж<leaf> leaf { get; }
}

partial struct top {
    internal partial ref mid mid { get; }
    internal @string label;
}

internal static void Main() {
    var h = new holder(leaf: Ꮡ(new leaf(n: 0)), tag: "h"u8);
    h.bump();
    h.bump();
    fmt.Println(h.get(), h.n, h.tag);
    var t = new top(mid: new mid(leaf: Ꮡ(new leaf(n: 10))), label: "t"u8);
    t.bump();
    fmt.Println(t.get(), t.n, t.label);
}

} // end main_package
