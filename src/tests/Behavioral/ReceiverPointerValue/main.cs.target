namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct ring {
    internal nint data;
    internal ж<ring> next;
}

internal static void initSelf(this ж<ring> Ꮡr) {
    ref var r = ref Ꮡr.DerefOrNull();

    r.next = Ꮡr;
}

internal static void linkTo(this ref ring r, ж<ring> Ꮡother) {
    r.next = Ꮡother;
}

internal static ж<ring> advance(this ж<ring> Ꮡr, nint n) {
    var p = Ꮡr;
    for (nint i = 0; i < n; i++) {
        p = p.Value.next;
    }
    return p;
}

internal static slice<ж<ring>> chain(this ж<ring> Ꮡr, nint n) {
    var nodes = new ж<ring>[]{Ꮡr}.slice();
    var p = Ꮡr;
    for (nint i = 0; i < n; i++) {
        p = p.Value.next;
        nodes = append(nodes, p);
    }
    return nodes;
}

internal static array<ж<ring>> pair(this ж<ring> Ꮡr, ж<ring> Ꮡother) {
    return new ж<ring>[]{Ꮡr, Ꮡother}.array();
}

internal static void Main() {
    var a = Ꮡ(new ring(data: 1));
    a.initSelf();
    a.Value.data = 42;
    fmt.Println((~(~a).next).data);
    fmt.Println((~a.advance(5)).data);
    var b = Ꮡ(new ring(data: 2));
    var c = Ꮡ(new ring(data: 3));
    b.linkTo(c);
    fmt.Println((~(~b).next).data);
    c.linkTo(b);
    fmt.Println((~b.advance(2)).data);
    var chain = b.chain(2);
    chain[0].Value.data = 7;
    fmt.Println((~b).data);
    fmt.Println((~chain[1]).data);
    var arr = b.pair(c);
    fmt.Println((~arr[0]).data, (~arr[1]).data);
}

} // end main_package
