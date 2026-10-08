namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static nint sumWalk(ж<node> Ꮡp, nint steps) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint total = 0;
    for (nint i = 0; i < steps; i++) {
        total += p.val;
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNull();
    }
    return total;
}

internal static ж<node> firstAfter(ж<node> Ꮡp, nint steps) {
    ref var p = ref Ꮡp.DerefOrNull();

    for (nint i = 0; i < steps; i++) {
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNull();
    }
    return Ꮡp;
}

internal static nint markSeen(ж<node> Ꮡp, map<ж<node>, bool> seen) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (seen[Ꮡp]) {
        return p.val;
    }
    seen[Ꮡp] = true;
    return -p.val;
}

internal static nint walkChain(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint count = 0;
    for (; Ꮡp != nil; Ꮡp = p.next) {
        p = ref Ꮡp.DerefOrNull();
        count += p.val;
    }
    return count;
}

internal static nint visitLocal(slice<ж<node>> nodes) {
    var visited = new map<ж<node>, bool>{};
    nint sum = 0;
    foreach (var (_, scan) in nodes) {
        var t = scan;
        if (visited[t]) {
            continue;
        }
        visited[t] = true;
        sum += t.Value.val;
    }
    return sum;
}

internal static slice<ж<node>> collect(slice<ж<node>> list, ж<node> Ꮡp) {
    if (len(list) == 0){
        list = new ж<node>[]{Ꮡp}.slice();
    } else {
        list = append(list, Ꮡp);
    }
    return list;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object chainˢ = (@string)"chain:"u8;
private static readonly object visitLocalˢ = (@string)"visitLocal:"u8;
private static readonly object collectˢ = (@string)"collect:"u8;

internal static void Main() {
    var a = Ꮡ(new node(val: 1));
    var b = Ꮡ(new node(val: 2));
    var c = Ꮡ(new node(val: 3));
    a.Value.next = b;
    b.Value.next = c;
    c.Value.next = a;
    fmt.Println(sumWalk(a, 6));
    fmt.Println((~firstAfter(a, 4)).val);
    var seen = new map<ж<node>, bool>{};
    fmt.Println(markSeen(a, seen), markSeen(a, seen), markSeen(b, seen));
    var x = Ꮡ(new node(val: 100));
    var y = Ꮡ(new node(val: 20));
    x.Value.next = y;
    fmt.Println(chainˢ, walkChain(x));
    var dup = Ꮡ(new node(val: 5));
    fmt.Println(visitLocalˢ, visitLocal(new ж<node>[]{dup, dup, x}.slice()));
    slice<ж<node>> list = default!;
    list = collect(list, a);
    list = collect(list, b);
    a.Value.val = 11;
    fmt.Println(collectˢ, (~list[0]).val, (~list[1]).val);
}

} // end main_package
