namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Inner {
    internal nint total;
}

public static void Add(this ж<Inner> Ꮡn, nint x) {
    var p = Ꮡn.of(Inner.Ꮡtotal);
    p.Value += x;
}

partial struct Outer {
    public partial ref ж<Inner> Inner { get; }
    internal @string tag;
}

internal static void Main() {
    var o = Ꮡ(new Outer(Inner: Ꮡ(new Inner(nil)), tag: "t"u8));
    o.Add(5);
    o.Add(3);
    fmt.Println((~o).total, (~o).tag);
}

} // end main_package
