namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint id;
}

internal static map<@string, ж<node>> buildValueMap(ж<node> Ꮡa, ж<node> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    return new map<@string, ж<node>>{["a"] = Ꮡa, ["b"] = Ꮡb};
}

internal static map<ж<node>, nint> buildKeyMap(ж<node> a, ж<node> b) {
    return new map<ж<node>, nint>{[a] = 10, [b] = 20};
}

internal static void Main() {
    var a = Ꮡ(new node(id: 1));
    var b = Ꮡ(new node(id: 2));

    var vm = buildValueMap(a, b);
    fmt.Println((~vm["a"]).id, (~vm["b"]).id);

    vm["a"].Value.id = 99;
    fmt.Println((~a).id);

    var km = buildKeyMap(a, b);
    fmt.Println(km[a], km[b]);
}

} // end main_package
