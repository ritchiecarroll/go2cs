namespace go;

using fmt = fmt_package;
using iter = iter_package;

partial class main_package {

partial interface Tree<N> {
    slice<N> Children();
    @string Label();
}

partial struct Leaf {
    internal @string name;
    internal slice<Leaf> kids;
}

public static slice<Leaf> Children(this Leaf l) {
    return l.kids;
}

public static @string Label(this Leaf l) {
    return l.name;
}

public static nint Count<N>(N n)
    where N : Tree<N>
{
    nint total = 1;

    foreach (var (_, kid) in n.Children()) {
        total += Count(kid);
    }

    return total;
}

public static slice<@string> Labels<N>(N n)
    where N : Tree<N>
{
    var @out = new @string[]{n.Label()}.slice();

    foreach (var (_, kid) in n.Children()) {
        @out = appendꓸꓸꓸ(@out, Labels(kid));
    }

    return @out;
}

public static void Rename<N>(N n, Func<N, N> rename)
    where N : Tree<N>
{
    var kids = n.Children();

    foreach (var i in range(kids)) {
        kids[i] = rename(kids[i]);
    }
}

partial interface Stepper<N> {
    Func<N> Next();
    array<N> Triple();
    iter.Seq<N> Path();
    @string Label();
}

partial struct Node {
    internal @string name;
    internal ж<Node> next;
}

public static Func<ж<Node>> Next(this ж<Node> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    if (n.next == nil) {
        return default;
    }

    return () => Ꮡn.Value.next;
}

public static array<ж<Node>> Triple(this ж<Node> Ꮡn) {
    return new ж<Node>[]{Ꮡn, Ꮡn, Ꮡn}.array();
}

public static iter.Seq<ж<Node>> Path(this ж<Node> Ꮡn) {
    return (Func<ж<Node>, bool> yield) => {
        for (var p = Ꮡn; p != nil; p = p.Value.next) {
            if (!yield(p)) {
                return;
            }
        }
    };
}

public static @string Label(this ref Node n) {
    return n.name;
}

public static slice<@string> Walk<N>(N n)
    where N : Stepper<N>
{
    var outᴷ = new stackAppend<@string>(moves: true);

    slice<@string> @out = default;

    while (ᐧ) {
        var triple = n.Triple();
        @out = append(outᴷ, @out, n.Label() + "/"u8 + triple[2].Label());

        var next = n.Next();

        if (next == nil) {
            break;
        }

        n = next();
    }

    @out = move2heap(outᴷ, @out);
    return @out;
}

public static slice<@string> Names<N>(N n)
    where N : Stepper<N>
{
    slice<@string> @out = default;

    foreach (var p in range<N>(n.Path().Invoke)) {
        @out = append(@out, p.Label());
    }

    return @out;
}

internal static void Main() {
    var root = new Leaf(name: "root", kids: new Leaf[]{new(name: "a"), new(name: "b", kids: new Leaf[]{new(name: "c")}.slice())}.slice());

    fmt.Println(Count(root));
    fmt.Println(Labels(root));

    Rename(root, Leaf (Leaf l) => {
        l.name += "!"u8;
        return l;
    });

    fmt.Println(Labels(root));

    var c = Ꮡ(new Node(name: "c"));
    var b = Ꮡ(new Node(name: "b", next: c));
    var a = Ꮡ(new Node(name: "a", next: b));

    fmt.Println(Walk<NodeжStepper>(a));
    fmt.Println(Names<NodeжStepper>(b));
}

} // end main_package
