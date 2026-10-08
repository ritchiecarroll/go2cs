namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static nint sum(this ж<node> Ꮡn) {
    nint total = 0;
    for (var p = Ꮡn; p != nil; p = p.Value.next) {
        total += p.Value.val;
    }
    return total;
}

internal static ж<ж<node>> Ꮡhead = new StandardBox<ж<node>>(default(ж<node>));
internal static ref ж<node> head => ref Ꮡhead.ValueSlot;

internal static void insert(nint v) {
    var n = Ꮡ(new node(val: v));
    var pp = Ꮡhead;
    while (pp.ValueSlot != nil && (~(pp.ValueSlot)).val < v) {
        pp = (pp.ValueSlot).of(node.Ꮡnext);
    }
    n.Value.next = pp.ValueSlot;
    pp.ValueSlot = n;
}

internal static bool remove(nint v) {
    for (var pp = Ꮡhead; pp.ValueSlot != nil; pp = (pp.ValueSlot).of(node.Ꮡnext)) {
        if ((~(pp.ValueSlot)).val == v) {
            pp.ValueSlot = (pp.ValueSlot).Value.next;
            return true;
        }
    }
    return false;
}

internal static @string list() {
    @string s = ""u8;
    for (var p = head; p != nil; p = p.Value.next) {
        s += fmt.Sprintf("%d "u8, (~p).val);
    }
    return s;
}

internal static void Main() {
    insert(30);
    insert(10);
    insert(20);
    insert(40);
    fmt.Println(list());
    fmt.Println(head.sum());
    fmt.Println(remove(10));
    fmt.Println(remove(30));
    fmt.Println(remove(99));
    fmt.Println(list());
    fmt.Println(head.sum());
    insert(5);
    fmt.Println(list());
    fmt.Println((~head).val);
}

} // end main_package
