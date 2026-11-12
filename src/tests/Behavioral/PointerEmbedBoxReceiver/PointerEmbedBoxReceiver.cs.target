namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct Inner {
    internal nint total;
}

} partial class mainᴛInner {

public static void Add(this ж<Inner> Ꮡn, nint x) {
    var p = Ꮡn.Ꮡtotal;
    p.Value += x;
}

} partial class main_package {

partial struct Outer {
    /*embed*/ public ж<Inner> Inner;
    internal @string tag;
}

internal static void main() {
    var o = Ꮡ(new Outer(Inner: Ꮡ(new Inner()), tag: "t"));
    o.Add(5);
    o.Add(3);
    fmt.Println(o.total, o.tag);
}

} // end main_package
