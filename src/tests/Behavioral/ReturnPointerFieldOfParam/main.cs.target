namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal ж<node> next;
    internal nint val;
}

internal static ж<node> advance(ref node n) {
    return n.next;
}

internal static ж<node> self(ж<node> Ꮡn) {
    return Ꮡn;
}

internal static void Main() {
    var c = Ꮡ(new node(val: 3));
    var b = Ꮡ(new node(next: c, val: 2));
    var a = Ꮡ(new node(next: b, val: 1));
    fmt.Println((~advance(ref (a).DerefOrNull())).val);
    fmt.Println((~advance(ref (advance(ref (a).DerefOrNull())).DerefOrNull())).val);
    fmt.Println((~self(a)).val);
    fmt.Println(advance(ref (c).DerefOrNull()) == nil);
}

} // end main_package
