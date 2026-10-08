namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint id;
}

internal static map<@string, ж<node>> buildValueMap(ж<node> Ꮡa, ж<node> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    return new map<@string, ж<node>>{["a"u8] = Ꮡa, ["b"u8] = Ꮡb};
}

internal static map<ж<node>, nint> buildKeyMap(ж<node> Ꮡa, ж<node> Ꮡb) {
    return new map<ж<node>, nint>{[Ꮡa] = 10, [Ꮡb] = 20};
}

internal static void Main() {
    var a = Ꮡ(new node(id: 1));
    var b = Ꮡ(new node(id: 2));
    var vm = buildValueMap(a, b);
    fmt.Println((~vm["a"u8]).id, (~vm["b"u8]).id);
    vm["a"u8].Value.id = 99;
    fmt.Println((~a).id);
    var km = buildKeyMap(a, b);
    fmt.Println(km[a], km[b]);
}

} // end main_package
