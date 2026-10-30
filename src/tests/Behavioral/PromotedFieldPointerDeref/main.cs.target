namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct header {
    internal ж<node> next;
    internal nint tag;
}

partial struct node {
    /*embed*/ internal header header;
    internal nint val;
}

partial struct list {
    internal ж<node> head;
}

internal static void Main() {
    var a = Ꮡ(new node(val: 1));
    a.tag = 7;
    var b = Ꮡ(new node(val: 2));
    b.tag = 9;
    a.next = b;

    list l = default;
    l.head = a;

    var x = l.head;
    while (x != nil) {
        fmt.Println(x.val, x.tag);
        x = x.next;
    }

    var y = l.head;
    y.tag = 99;
    fmt.Println(a.tag);
}

} // end main_package
