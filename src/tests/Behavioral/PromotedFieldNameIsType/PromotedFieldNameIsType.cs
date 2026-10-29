namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct sym {
    public ж<Node> Node;
    public nint Value;
    public @string Name;
}

partial struct Node {
    /*embed*/ internal ж<sym> sym;
    public nint Weight;
}

internal static void Main() {
    var a = Ꮡ(new Node(sym: Ꮡ(new sym(Value: 1, Name: "a")), Weight: 10));
    var b = Ꮡ(new Node(sym: Ꮡ(new sym(Value: 2, Name: "b")), Weight: 20));
    a.sym.Node = b;
    b.sym.Node = a;

    fmt.Println((~a).Value, (~a).Name, a.Weight);
    fmt.Println((~a.sym.Node).Name, a.sym.Node.Weight);
    fmt.Println((~b.sym.Node).Name, (~b).Value);
}

} // end main_package
