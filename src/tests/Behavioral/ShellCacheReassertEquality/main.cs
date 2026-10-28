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

internal partial interface lener_R0 /*dyn*/ {
    nint Len();
}

internal static lener_R0 lener(any a) {
    var (v, _) = a._<lener_R0>(ᐧ);
    return v;
}

internal static void Main() {
    var p = Ꮡ(new box(n: 1));
    var q = Ꮡ(new box(n: 1));
    var values = new any[]{p.OrTypedNil(), q.OrTypedNil(), (counter)3, (counter)3, (counter)4}.slice();

    fmt.Println("pointer, same any twice:", AreEqual(lener(values[0]), lener(values[0])));
    fmt.Println("pointer, same box in a new any:", AreEqual(lener(values[0]), lener((any)p.OrTypedNil())));
    fmt.Println("pointer, two boxes with equal contents:", AreEqual(lener(values[0]), lener(values[1])));

    fmt.Println("value, same any twice:", AreEqual(lener(values[2]), lener(values[2])));
    fmt.Println("value, equal values in two anys:", AreEqual(lener(values[2]), lener(values[3])));
    fmt.Println("value, different values:", AreEqual(lener(values[2]), lener(values[4])));

    fmt.Println("pointer shell against any(p):", AreEqual(((any)lener(values[0])), ((any)p.OrTypedNil())));
    fmt.Println("value shell against any(counter(3)):", AreEqual(((any)lener(values[2])), ((any)((counter)3))));
    fmt.Println("value shell against any(counter(4)):", AreEqual(((any)lener(values[2])), ((any)((counter)4))));

    nint same = 0;
    nint different = 0;

    for (nint i = 0; i < 100; i++) {
        foreach (var (j, a) in values) {
            foreach (var (k, b) in values) {
                if (AreEqual(lener(a), lener(b))) {
                    same++;
                } else if (j != k) {
                    different++;
                }
            }
        }
    }

    fmt.Println("pairs equal:", same, "pairs unequal:", different);
}

} // end main_package
