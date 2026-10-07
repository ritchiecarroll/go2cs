namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

[GoType] partial struct node {
    internal nint val;
    internal ж<node> next;
}

internal static nint sumList(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint total = 0;
    while (Ꮡp != nil) {
        total += p.val;
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNull();
    }
    return total;
}

internal static void doubleList(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    while (Ꮡp != nil) {
        p.val *= 2;
        Ꮡp = p.next; p = ref Ꮡp.DerefOrNull();
    }
}

internal static ж<node> build(params ꓸꓸꓸnint valsʗp) {
    var vals = valsʗp.sslice();

    ж<node> head = default!;
    for (nint i = len(vals) - 1; i >= 0; i--) {
        head = Ꮡ(new node(val: vals[i], next: head));
    }
    return head;
}

internal static partial (ж<node>, nint) advance(ref node p) {
    return (p.next, p.val);
}

internal static nint sumEveryOther(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint total = 0;
    nint skipped = 0;
    while (Ꮡp != nil) {
        total += p.val;
        (Ꮡp, skipped) = advance(ref (Ꮡp).DerefOrNull()); p = ref Ꮡp.DerefOrNull();
        _ = skipped;
        if (Ꮡp != nil) {
            (Ꮡp, skipped) = advance(ref (Ꮡp).DerefOrNull()); p = ref Ꮡp.DerefOrNull();
            total += skipped * 0;
        }
    }
    return total;
}

internal static void bumpFirstViaTuple(ж<node> Ꮡp) {
    ref var p = ref Ꮡp.DerefOrNull();

    nint skipped = default!;
    (Ꮡp, skipped) = advance(ref (Ꮡp).DerefOrNull()); p = ref Ꮡp.DerefOrNull();
    _ = skipped;
    if (Ꮡp != nil) {
        p.val += 100;
    }
}

internal static partial ж<node> dropIfShort(ж<node> Ꮡp, nint min) {
    ref var p = ref Ꮡp.DerefOrNull();

    if (sumList(Ꮡp) < min) {
        Ꮡp = default!; p = ref Ꮡp.DerefOrNull();
    }
    if (Ꮡp != nil) {
        p.val += 1000;
    }
    return Ꮡp;
}

internal static void Main() {
    var list = build(1, 2, 3, 4);
    fmt.Println(sumList(list));
    doubleList(list);
    fmt.Println(sumList(list));
    fmt.Println(sumList(nil));
    fmt.Println(sumEveryOther(list));
    fmt.Println(sumEveryOther(nil));
    bumpFirstViaTuple(list);
    fmt.Println(sumList(list));
    var @short = build(1, 2);
    fmt.Println(dropIfShort(@short, 100) == nil);
    fmt.Println(sumList(@short));
    var kept = dropIfShort(@short, 2);
    fmt.Println(kept != nil, sumList(@short));
}

} // end main_package
