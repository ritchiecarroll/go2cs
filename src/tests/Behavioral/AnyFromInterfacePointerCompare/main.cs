namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint id;
}

internal static @string Error(this ref node n) {
    return fmt.Sprintf("node %d"u8, n.id);
}

internal static void Main() {
    var p = Ꮡ(new node(id: 1));
    var q = Ꮡ(new node(id: 2));

    any direct = p.OrTypedNil();
    fmt.Println("direct == p:", AreEqual(direct, p));
    fmt.Println("direct != p:", !AreEqual(direct, p));
    fmt.Println("direct == q:", AreEqual(direct, q));

    error err = new nodeжerror(p);
    any widened = err;
    fmt.Println("widened == p:", AreEqual(widened, p));
    fmt.Println("widened != p:", !AreEqual(widened, p));
    fmt.Println("p == widened:", AreEqual(p, widened));
    fmt.Println("widened == q:", AreEqual(widened, q));

    fmt.Println("lookup:", lookup(err, p));
    fmt.Println("lookup other:", lookup(err, q));
}

internal static @string lookup(any key, ж<node> Ꮡwant) {
    if (AreEqual(key, Ꮡwant)) {
        return "found";
    }

    return "missing";
}

} // end main_package
