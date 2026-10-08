namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal @string name;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object describeˢ = (@string)"describe:"u8;

internal static slice<ж<node>> collect(this ж<node> Ꮡc, slice<ж<node>> chain) {
    @string describe() => Ꮡc.Value.name;
    slice<ж<node>> toCheck = default!;
    toCheck = append(toCheck, Ꮡc);
    foreach (var (_, cΔ1) in chain) {
        toCheck = append(toCheck, cΔ1);
    }
    fmt.Println(describeˢ, describe());
    return toCheck;
}

internal static ж<node> firstOr(this ж<node> Ꮡc, slice<ж<node>> chain) {
    foreach (var (_, cΔ1) in chain) {
        return cΔ1;
    }
    return Ꮡc;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object gotˢ = (@string)"got:"u8;
private static readonly object firstˢ = (@string)"first:"u8;
private static readonly object emptyˢ = (@string)"empty:"u8;

internal static void Main() {
    var root = Ꮡ(new node(name: "root"u8));
    var a = Ꮡ(new node(name: "a"u8));
    var b = Ꮡ(new node(name: "b"u8));
    foreach (var (_, n) in root.collect(new ж<node>[]{a, b}.slice())) {
        fmt.Println(gotˢ, (~n).name);
    }
    fmt.Println(firstˢ, (~root.firstOr(new ж<node>[]{a, b}.slice())).name);
    fmt.Println(emptyˢ, (~root.firstOr(default!)).name);
}

} // end main_package
