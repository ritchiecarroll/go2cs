namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal @string name;
}

internal static slice<ж<node>> collect(this ж<node> Ꮡc, slice<ж<node>> chain) {
    @string describe() => Ꮡc.Value.name;

    slice<ж<node>> toCheck = default;
    toCheck = append(toCheck, Ꮡc);

    foreach (var (_, cΔ1) in chain) {
        toCheck = append(toCheck, cΔ1);
    }

    fmt.Println("describe:", describe());
    return toCheck;
}

internal static ж<node> firstOr(this ж<node> Ꮡc, slice<ж<node>> chain) {
    foreach (var (_, cΔ1) in chain) {
        return cΔ1;
    }

    return Ꮡc;
}

internal static void Main() {
    var root = Ꮡ(new node(name: "root"));
    var a = Ꮡ(new node(name: "a"));
    var b = Ꮡ(new node(name: "b"));

    foreach (var (_, n) in root.collect(slice<ж<node>>([a, b]))) {
        fmt.Println("got:", n.name);
    }

    fmt.Println("first:", root.firstOr(slice<ж<node>>([a, b])).name);
    fmt.Println("empty:", root.firstOr(default).name);
}

} // end main_package
