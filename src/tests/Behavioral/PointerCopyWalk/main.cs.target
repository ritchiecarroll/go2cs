namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static ж<node> last(ж<node> Ꮡstart) {
    var r = Ꮡstart;
    while ((~r).next != nil) {
        r = r.Value.next;
    }
    return r;
}

internal static nint length(ж<node> Ꮡstart) {
    nint n = 0;
    for (var r = Ꮡstart; r != nil; r = r.Value.next) {
        n++;
    }
    return n;
}

internal static nint sum(ж<node> Ꮡstart) {
    nint total = 0;
    var p = Ꮡstart;
    while (p != nil) {
        total += p.Value.val;
        p = p.Value.next;
    }
    return total;
}

internal static void Main() {
    var third = Ꮡ(new node(val: 3));
    var second = Ꮡ(new node(val: 2, next: third));
    var first = Ꮡ(new node(val: 1, next: second));
    fmt.Println((~last(first)).val);
    fmt.Println(length(first));
    fmt.Println(sum(first));
}

} // end main_package
