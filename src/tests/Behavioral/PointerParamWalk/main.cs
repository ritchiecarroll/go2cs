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
    for (; Ꮡp != nil; Ꮡp = p.next, p = ref Ꮡp.DerefOrNull()) {
        count += p.val;
    }
    return count;
}

internal static nint visitLocal(slice<ж<node>> nodes) {
    var visited = new map<ж<node>, bool>{};
    nint sum = 0;
    foreach (var (_, scan) in range(nodes)) {
        var t = scan;
        if (visited[t]) {
            continue;
        }
        visited[t] = true;
        sum += t.val;
    }
    return sum;
}

internal static slice<ж<node>> collect(slice<ж<node>> list, ж<node> p) {
    if (len(list) == 0) {
        list = [p];
    } else {
        list = append(list, p);
    }
    return list;
}

internal static nint firstAtLeast(ж<node> Ꮡp, nint limit) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint steps = 0;
    for (; Ꮡp != nil && p.val < limit; Ꮡp = p.next, p = ref Ꮡp.DerefOrNull()) {
        steps++;
    }
    if (Ꮡp == nil) {
        return -steps;
    }
    return steps;
}

internal static nint countOdd(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint count = 0;
    for (; Ꮡp != nil; Ꮡp = p.next, p = ref Ꮡp.DerefOrNull()) {
        if (p.val % 2 == 0) {
            continue;
        }
        count++;
    }
    return count;
}

internal static nint lastVal(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint depth = 0;
    for (; p.next != nil; Ꮡp = p.next, p = ref Ꮡp.DerefOrNull()) {
        depth++;
    }
    return depth * 10 + p.val;
}

internal static nint after(ж<node> Ꮡp, nint limit) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint steps = 0;
    for (Ꮡp = p.next, p = ref Ꮡp.DerefOrNull(); Ꮡp != nil && p.val < limit; Ꮡp = p.next, p = ref Ꮡp.DerefOrNull()) {
        steps++;
    }
    if (Ꮡp == nil) {
        return -steps - 1;
    }
    return steps * 10 + p.val;
}

internal static nint initOnly(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint sum = 0;
    for (Ꮡp = p.next, p = ref Ꮡp.DerefOrNull(); Ꮡp != nil; ) {
        sum += p.val;
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNull();
    }
    return sum;
}

internal static nint from(ж<node> Ꮡa, ж<node> b) {
    ref var a = ref Ꮡa.DerefOrNull();

    nint sum = 0;
    for (Ꮡa = b, a = ref Ꮡa.DerefOrNull(); Ꮡa != nil; Ꮡa = a.next, a = ref Ꮡa.DerefOrNull()) {
        sum += a.val;
    }
    return sum;
}

internal static nint swapWalk(ж<node> Ꮡa, ж<node> Ꮡb) {
    ref var a = ref Ꮡa.DerefOrNull();
    ref var b = ref Ꮡb.DerefOrNull();

    nint sum = 0;
    for ((Ꮡa, Ꮡb) = (Ꮡb, Ꮡa), a = ref Ꮡa.DerefOrNull(), b = ref Ꮡb.DerefOrNull(); Ꮡa != nil && Ꮡb != nil; (Ꮡa, Ꮡb) = (a.next, b.next), a = ref Ꮡa.DerefOrNull(), b = ref Ꮡb.DerefOrNull()) {
        sum += a.val * 10 + b.val;
    }
    return sum;
}

internal static nint nested(ж<node> Ꮡp, ж<node> start) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint sum = 0;
outer:
    for (Ꮡp = start, p = ref Ꮡp.DerefOrNull(); Ꮡp != nil; Ꮡp = p.next, p = ref Ꮡp.DerefOrNull()) {
        var m = Ꮡp;
        for (Ꮡp = p.next, p = ref Ꮡp.DerefOrNull(); Ꮡp != nil; Ꮡp = p.next, p = ref Ꮡp.DerefOrNull()) {
            if (p.val == 4) {
                Ꮡp = m; p = ref Ꮡp.DerefOrNull();
                goto continue_outer;
            }
            sum += p.val;
        }
        Ꮡp = m; p = ref Ꮡp.DerefOrNull();
        sum += 100;
continue_outer:;
    }
break_outer:;
    return sum;
}

internal static (ж<node>, nint) pair(ref node p) { return (p.next, p.val); }

internal static ж<node> pick(ж<node> n, nint k) {
    if (k < 0) {
        return default;
    }
    return n;
}

internal static nint spillWalk(ж<node> Ꮡp, nint limit) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint steps = 0;
    for (; Ꮡp != nil && p.val < limit; ) {
        steps++;
        var (ᴛ1, ᴛ2) = pair(ref (Ꮡp).DerefOrNull());
        Ꮡp = pick(ᴛ1, ᴛ2); p = ref Ꮡp.DerefOrNull();
    }
    if (Ꮡp == nil) {
        return -steps;
    }
    return steps * 10 + p.val;
}

internal static nint spillSkip(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint count = 0;
    for (; Ꮡp != nil; ) {
        if (p.val % 2 == 0) {
            goto continueᴛ1;
        }
        count++;
continueᴛ1:;
        var (ᴛ3, ᴛ4) = pair(ref (Ꮡp).DerefOrNull());
        Ꮡp = pick(ᴛ3, ᴛ4); p = ref Ꮡp.DerefOrNull();
    }
    return count;
}

internal static void main() {
    var a = Ꮡ(new node(val: 1));
    var b = Ꮡ(new node(val: 2));
    var c = Ꮡ(new node(val: 3));
    a.next = b;
    b.next = c;
    c.next = a;

    fmt.Println(sumWalk(a, 6));
    fmt.Println(firstAfter(a, 4).val);

    var seen = new map<ж<node>, bool>{};
    fmt.Println(markSeen(a, seen), markSeen(a, seen), markSeen(b, seen));

    var x = Ꮡ(new node(val: 100));
    var y = Ꮡ(new node(val: 20));
    x.next = y;
    fmt.Println("chain:", walkChain(x));

    var dup = Ꮡ(new node(val: 5));
    fmt.Println("visitLocal:", visitLocal([dup, dup, x]));

    slice<ж<node>> list = default;
    list = collect(list, a);
    list = collect(list, b);
    a.val = 11;
    fmt.Println("collect:", list[0].val, list[1].val);

    var k7 = Ꮡ(new node(val: 7));
    var k4 = Ꮡ(new node(val: 4, next: k7));
    var k1 = Ꮡ(new node(val: 1, next: k4));
    fmt.Println("cond:", firstAtLeast(k1, 7), firstAtLeast(k1, 4), firstAtLeast(k1, 9), firstAtLeast(nil, 1));
    fmt.Println("continue:", countOdd(k1), countOdd(k4), countOdd(nil));
    fmt.Println("after loop:", lastVal(k1), lastVal(k4), lastVal(k7));
    fmt.Println("init:", after(k1, 7), after(k1, 4), after(k1, 9), after(k7, 1));
    fmt.Println("init only:", initOnly(k1), initOnly(k7));
    fmt.Println("init from:", from(nil, k1), from(k1, nil));
    fmt.Println("tuple:", swapWalk(k1, k4), swapWalk(k7, k1));
    fmt.Println("nested:", nested(nil, k1), nested(k1, k7));
    fmt.Println("lowered post:", spillWalk(k1, 7), spillWalk(k1, 4), spillWalk(k1, 9), spillWalk(nil, 1));
    fmt.Println("lowered continue:", spillSkip(k1), spillSkip(k4), spillSkip(nil));
}

} // end main_package
