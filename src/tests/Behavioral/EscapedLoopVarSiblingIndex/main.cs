namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

[GoType] partial struct node {
    internal ж<node> next;
}

[GoRecv] internal static partial void link(this ref node n, ж<node> Ꮡm) {
    n.next = Ꮡm;
}

internal static partial array<nint> process(nint n) {
    array<nint> a = new(5);
    for (nint i = 0; i < n; i++) {
        a[i] = i * 10;
    }
    if (n >= 3) {
        for (nint i = 0; i < 2; i++) {
            (a[i], a[i + 1]) = (a[i + 1], a[i]);
        }
    }
    ref var nodes = ref heap(new array<node>(5), out var Ꮡnodes);
    for (nint i = 0; i < n - 1; i++) {
        nodes[i].link(Ꮡnodes.at<node>(i + 1));
    }
    return a.Clone();
}

internal static partial nint mapShadow(nint ns) {
    var m = new map<nint, nint>{};
    m[ns] = ns * 100;
    {
        nint nsΔ1 = 3;
        m[nsΔ1] = nsΔ1 * 1000;
    }
    return m[ns] * 10 + len(m);
}

internal static partial nint boxedSiblings(nint kind) {
    nint total = 0;
    switch (kind) {
    case 1: {
        for (nint iᴛ1 = 0; iᴛ1 < 3; iᴛ1++) {
            ref var i = ref heap<nint>(out var Ꮡi);
            i = iᴛ1;
            var p = Ꮡi;
            total += p.Value;
            iᴛ1 = i;
        }
        for (nint iᴛ2 = 10; iᴛ2 < 13; iᴛ2++) {
            ref var i = ref heap<nint>(out var Ꮡi);
            i = iᴛ2;
            var p = Ꮡi;
            total += p.Value * 2;
            iᴛ2 = i;
        }
        break;
    }}

    return total;
}

internal static partial nint caseSiblings(nint kind) {
    nint total = 0;
    switch (kind) {
    case 1: {
        ref var xs = ref heap(new array<node>(3), out var Ꮡxs);
        for (nint i = 0; i < 2; i++) {
            xs[i].link(Ꮡxs.at<node>(i + 1));
        }
        ref var ys = ref heap(new array<node>(3), out var Ꮡys);
        for (nint i = 0; i < 2; i++) {
            ys[i].link(Ꮡys.at<node>(i + 1));
        }
        for (var p = Ꮡxs.at<node>(0); p != nil; p = p.Value.next) {
            total++;
        }
        for (var p = Ꮡys.at<node>(0); p != nil; p = p.Value.next) {
            total += 10;
        }
        break;
    }}

    return total;
}

internal static void Main() {
    fmt.Println(process(5));
    fmt.Println(mapShadow(2));
    fmt.Println(parenIndex());
    fmt.Println(caseSiblings(1));
    fmt.Println(boxedSiblings(1));
}

} // end main_package
