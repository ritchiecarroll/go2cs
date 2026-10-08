namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct sym {
    public ж<Node> Node;
    public nint Value;
    public @string Name;
}

partial struct Node {
    internal partial ref ж<sym> sym { get; }
    public nint Weight;
}

internal static void Main() {
    var a = Ꮡ(new Node(sym: Ꮡ(new sym(Value: 1, Name: "a"u8)), Weight: 10));
    var b = Ꮡ(new Node(sym: Ꮡ(new sym(Value: 2, Name: "b"u8)), Weight: 20));
    a.Value.sym.Value.Node = b;
    b.Value.sym.Value.Node = a;
    fmt.Println((~a).Value, (~a).Name, (~a).Weight);
    fmt.Println((~(~(~a).sym).Node).Name, (~(~(~a).sym).Node).Weight);
    fmt.Println((~(~(~b).sym).Node).Name, (~b).Value);
}

} // end main_package
