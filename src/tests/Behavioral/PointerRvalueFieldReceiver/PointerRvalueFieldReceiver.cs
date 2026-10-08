namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct slot {
    internal nint v;
}

internal static void set(this ref slot s, nint x) {
    s.v = x;
}

partial struct node {
    internal slot s;
}

internal static ж<node> theNode = Ꮡ(new node(nil));

internal static ж<node> getNode() {
    return theNode;
}

partial struct queue {
    internal ж<node> last;
}

internal static ж<node> tail(this ref queue q) {
    return q.last;
}

internal static void Main() {
    getNode().of(node.Ꮡs).set(42);
    fmt.Println((~theNode).s.v);
    var a = Ꮡ(new node(nil));
    var b = Ꮡ(new node(nil));
    var nodes = new ж<node>[]{a, b}.slice();
    nodes[0].of(node.Ꮡs).set(7);
    nodes[1].of(node.Ꮡs).set(9);
    fmt.Println((~a).s.v, (~b).s.v);
    var q = Ꮡ(new queue(last: a));
    q.tail().of(node.Ꮡs).set(100);
    fmt.Println((~a).s.v);
    any iface = b.OrTypedNil();
    iface._<ж<node>>().of(node.Ꮡs).set(55);
    fmt.Println((~b).s.v);
}

} // end main_package
