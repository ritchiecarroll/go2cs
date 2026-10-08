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
            foreach (var (iΔ1, _) in (~nΔ1).Values) {
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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string specˢ = "spec"u8;
private static readonly @string litˢ = "lit"u8;

internal static void tag(Node root) {
    walk(root, (Node n) => {
        switch (n.type()) {
        case ж<ValueSpec> nΔ1: {
            (~nΔ1).Values[0]._<ж<Lit>>().Value.Name = specˢ;
            break;
        }
        default: {
            var nΔ1 = n;
            nΔ1._<ж<Lit>>().Value.Name = litˢ;
            break;
        }}
        return true;
    });
}

internal static @string read(Node root) {
    @string got = ""u8;
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
        Names: new @string[]{"Typ"u8}.slice(),
        Values: new Node[]{new LitжNode(Ꮡ(new Lit(Len: 1, Name: "a"u8)))}.slice()
    ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object fixSpecˢ = (@string)"fixSpec:"u8;
private static readonly object renumberOuterˢ = (@string)"renumber outer:"u8;
private static readonly object renumberˢ = (@string)"renumber:"u8;
private static readonly object bumpˢ = (@string)"bump:"u8;
private static readonly object tagSpecˢ = (@string)"tag spec:"u8;
private static readonly object tagDefaultˢ = (@string)"tag default:"u8;
private static readonly object readˢ = (@string)"read:"u8;

internal static void Main() {
    var spec = newSpec();
    fixSpec(new ValueSpecжNode(spec));
    fmt.Println(fixSpecˢ, (~(~spec).Values[0]._<ж<Lit>>()).Len);
    var multi = Ꮡ(new ValueSpec(
        Names: new @string[]{"Typ"u8, "Other"u8}.slice(),
        Values: new Node[]{new LitжNode(Ꮡ(new Lit(Len: 1, Name: "a"u8))), new LitжNode(Ꮡ(new Lit(Len: 2, Name: "b"u8))), new LitжNode(Ꮡ(new Lit(Len: 3, Name: "c"u8)))}.slice()
    ));
    nint outer = renumber(new ValueSpecжNode(multi), 7);
    fmt.Println(renumberOuterˢ, outer);
    foreach (var (k, _) in (~multi).Values) {
        fmt.Println(renumberˢ, k, (~(~multi).Values[k]._<ж<Lit>>()).Len);
    }
    var bumped = newSpec();
    bump(new ValueSpecжNode(bumped));
    fmt.Println(bumpˢ, (~(~bumped).Values[0]._<ж<Lit>>()).Len);
    var tagged = newSpec();
    tag(new ValueSpecжNode(tagged));
    fmt.Println(tagSpecˢ, (~(~tagged).Values[0]._<ж<Lit>>()).Name);
    var lone = Ꮡ(new Lit(Len: 9, Name: "z"u8));
    tag(new LitжNode(lone));
    fmt.Println(tagDefaultˢ, (~lone).Name);
    fmt.Println(readˢ, read(new ValueSpecжNode(tagged)));
}

} // end main_package
