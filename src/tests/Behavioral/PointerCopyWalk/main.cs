namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static ж<node> last(ж<node> start) {
    var r = start;
    while (r.next != nil) {
        r = r.next;
    }
    return r;
}

internal static nint length(ж<node> start) {
    nint n = 0;
    for (var r = start; r != nil; r = r.next) {
        n++;
    }
    return n;
}

internal static nint sum(ж<node> start) {
    nint total = 0;
    var p = start;
    while (p != nil) {
        total += p.val;
        p = p.next;
    }
    return total;
}

internal static void Main() {
    var third = Ꮡ(new node(val: 3));
    var second = Ꮡ(new node(val: 2, next: third));
    var first = Ꮡ(new node(val: 1, next: second));

    fmt.Println(last(first).val);
    fmt.Println(length(first));
    fmt.Println(sum(first));
}

} // end main_package
