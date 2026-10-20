namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct box {
    internal nint n;
}

internal static nint Len(this ref box b) {
    return b.n;
}

partial struct counter /*num:nint*/;

internal static nint Len(this counter c) {
    return (nint)c;
}

internal partial interface main_type /*dyn*/ {
    nint Len();
}

internal static void Main() {
    var p = Ꮡ(new box(n: 1));
    var q = Ꮡ(new box(n: 10));
    var values = new any[]{p.OrTypedNil(), q.OrTypedNil(), (counter)3, (counter)4}.slice();

    var (first, ok) = values[0]._<main_type>(ᐧ);
    fmt.Println("first assert:", ok, first.Len());

    p.Value.n = 2;
    fmt.Println("old shell after write:", first.Len());

    (var again, ok) = values[0]._<main_type>(ᐧ);
    fmt.Println("re-assert after write:", ok, again.Len());

    (var other, ok) = values[1]._<main_type>(ᐧ);
    fmt.Println("other box:", ok, other.Len());

    q.Value.n = 11;
    fmt.Println("other box after write:", other.Len(), "first box still:", again.Len());

    for (nint i = 2; i < len(values); i++) {
        var (v, okΔ1) = values[i]._<main_type>(ᐧ);
        fmt.Println("value", i, okΔ1, v.Len());
    }

    nint sum = 0;

    for (nint i = 0; i < 1000; i++) {
        if (i == 500) {
            p.Value.n = 3;
        }

        foreach (var (_, a) in values) {
            {
                var (v, okΔ2) = a._<main_type>(ᐧ); if (okΔ2) {
                    sum += v.Len();
                }
            }
        }
    }

    fmt.Println("sum:", sum);
    fmt.Println("old shells at the end:", first.Len(), again.Len(), other.Len());
}

} // end main_package
