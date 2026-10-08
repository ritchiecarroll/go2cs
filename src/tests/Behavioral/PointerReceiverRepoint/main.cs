namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static nint sum(this ж<node> Ꮡn) {
    ref var n = ref Ꮡn.DerefOrNull();

    nint total = 0;
    while (ᐧ) {
        total += n.val;
        if (n.next == nil) {
            break;
        }
        Ꮡn = n.next; n = ref Ꮡn.DerefOrNull();
    }
    return total;
}

internal static nint advance(this ж<node> Ꮡn, nint steps) {
    ref var n = ref Ꮡn.DerefOrNull();

    for (nint i = 0; i < steps; i++) {
        if (n.next == nil) {
            break;
        }
        Ꮡn = n.next; n = ref Ꮡn.DerefOrNull();
    }
    return n.val;
}

internal static void scale(this ж<node> Ꮡn, nint factor) {
    ref var n = ref Ꮡn.DerefOrNull();

    while (ᐧ) {
        n.val = n.val * factor;
        if (n.next == nil) {
            break;
        }
        Ꮡn = n.next; n = ref Ꮡn.DerefOrNull();
    }
}

internal static void Main() {
    var c = Ꮡ(new node(val: 3));
    var b = Ꮡ(new node(val: 2, next: c));
    var a = Ꮡ(new node(val: 1, next: b));
    fmt.Println(a.sum());
    fmt.Println(a.advance(0), a.advance(1), a.advance(2), a.advance(99));
    a.scale(10);
    fmt.Println((~a).val, (~b).val, (~c).val);
    fmt.Println(a.sum());
    fmt.Println((~a).val, (~(~a).next).val, (~(~(~a).next).next).val);
}

} // end main_package
