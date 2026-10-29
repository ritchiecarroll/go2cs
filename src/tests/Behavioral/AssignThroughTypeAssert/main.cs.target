namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Node {
    void node();
}

partial struct Lit {
    public nint Len;
    public @string Name;
}

internal static void node(this ref Lit _) {
}

partial struct ValueSpec {
    public slice<@string> Names;
    public slice<Node> Values;
}

internal static void node(this ref ValueSpec _) {
}

internal static void walk(Node root, Func<Node, bool> f) {
    f(root);
}

internal static void fixSpec(Node root) {
    walk(root, (Node n) => {
        switch (n.type()) {
        case ж<ValueSpec> nΔ1: {
            if (len((~nΔ1).Names) == 1 && (~nΔ1).Names[0] == "Typ" && len((~nΔ1).Values) == 1) {
                (~nΔ1).Values[0]._<ж<Lit>>().Value.Len = 42;
                return false;
            }
            break;
        }}
        return true;
    });
}

internal static nint renumber(Node root, nint i) {
    walk(root, (Node n) => {
        switch (n.type()) {
        case ж<ValueSpec> nΔ1: {
            foreach (var iΔ1 in range((~nΔ1).Values)) {
                (~nΔ1).Values[iΔ1]._<ж<Lit>>().Value.Len = (iΔ1 + 1) * 10;
            }
            break;
        }}
        return true;
    });

    return i;
}

internal static void bump(Node root) {
    walk(root, (Node n) => {
        switch (n.type()) {
        case ж<ValueSpec> nΔ1: {
            (~nΔ1).Values[0]._<ж<Lit>>().Value.Len += 5;
            break;
        }}
        return true;
    });
}

internal static void tag(Node root) {
    walk(root, (Node n) => {
        switch (n.type()) {
        case ж<ValueSpec> nΔ1: {
            (~nΔ1).Values[0]._<ж<Lit>>().Value.Name = "spec";
            break;
        }
        default: {
            var nΔ1 = n;
            nΔ1._<ж<Lit>>().Value.Name = "lit";
            break;
        }}
        return true;
    });
}

internal static @string read(Node root) {
    @string got = "";

    walk(root, (Node n) => {
        switch (n.type()) {
        case ж<ValueSpec> nΔ1: {
            got = (~nΔ1).Values[0]._<ж<Lit>>().Value.Name;
            break;
        }}
        return true;
    });

    return got;
}

internal static ж<ValueSpec> newSpec() {
    return Ꮡ(new ValueSpec(
        Names: slice<@string>(["Typ"]),
        Values: slice<Node>([new LitжNode(Ꮡ(new Lit(Len: 1, Name: "a")))])
    ));
}

internal static void Main() {
    var spec = newSpec();
    fixSpec(new ValueSpecжNode(spec));
    fmt.Println("fixSpec:", (~(~spec).Values[0]._<ж<Lit>>()).Len);

    var multi = Ꮡ(new ValueSpec(
        Names: slice<@string>(["Typ", "Other"]),
        Values: slice<Node>([new LitжNode(Ꮡ(new Lit(Len: 1, Name: "a"))), new LitжNode(Ꮡ(new Lit(Len: 2, Name: "b"))), new LitжNode(Ꮡ(new Lit(Len: 3, Name: "c")))])
    ));
    nint outer = renumber(new ValueSpecжNode(multi), 7);
    fmt.Println("renumber outer:", outer);

    foreach (var k in range((~multi).Values)) {
        fmt.Println("renumber:", k, (~(~multi).Values[k]._<ж<Lit>>()).Len);
    }

    var bumped = newSpec();
    bump(new ValueSpecжNode(bumped));
    fmt.Println("bump:", (~(~bumped).Values[0]._<ж<Lit>>()).Len);

    var tagged = newSpec();
    tag(new ValueSpecжNode(tagged));
    fmt.Println("tag spec:", (~(~tagged).Values[0]._<ж<Lit>>()).Name);

    var lone = Ꮡ(new Lit(Len: 9, Name: "z"));
    tag(new LitжNode(lone));
    fmt.Println("tag default:", (~lone).Name);

    fmt.Println("read:", read(new ValueSpecжNode(tagged)));
}

} // end main_package
