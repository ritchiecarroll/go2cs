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

internal static (nint, bool) depth(ж<node> Ꮡstart) {
    ref var start = ref Ꮡstart.DerefOrNull();

    nint label = start.val;
    ж<node> current = Ꮡstart;
    var same = current == Ꮡstart;
    nint d = 0;
    while (current != nil) {
        d++;
        current = current.next;
    }
    return (label + d, same);
}

internal static (nint, nint) ends(ж<node> Ꮡa, ж<node> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    _ = a.val + b.val;
    ж<node> r = Ꮡa;
    ж<node> x = Ꮡa;
    ж<node> y = Ꮡb;
    while (r.next != nil) {
        r = r.next;
    }
    return (r.val + x.val, y.val);
}

internal static void main() {
    var third = Ꮡ(new node(val: 3));
    var second = Ꮡ(new node(val: 2, next: third));
    var first = Ꮡ(new node(val: 1, next: second));

    fmt.Println(last(first).val);
    fmt.Println(length(first));
    fmt.Println(sum(first));
    var (ᴛ1, ᴛ2) = depth(first);
    fmt.Println(ᴛ1, ᴛ2);
    var (ᴛ3, ᴛ4) = ends(first, second);
    fmt.Println(ᴛ3, ᴛ4);
}

} // end main_package
