namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct header {
    internal ж<node> next;
    internal nint tag;
}

partial struct node {
    internal partial ref header header { get; }
    internal nint val;
}

partial struct list {
    internal ж<node> head;
}

internal static void Main() {
    var a = Ꮡ(new node(val: 1));
    a.Value.tag = 7;
    var b = Ꮡ(new node(val: 2));
    b.Value.tag = 9;
    a.Value.next = b;
    list l = default!;
    l.head = a;
    var x = l.head;
    while (x != nil) {
        fmt.Println((~x).val, (~x).tag);
        x = x.Value.next;
    }
    var y = l.head;
    y.Value.tag = 99;
    fmt.Println((~a).tag);
}

} // end main_package
