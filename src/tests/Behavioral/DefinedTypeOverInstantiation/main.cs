namespace go;

using fmt = fmt_package;
using lib = global::go.go.instlib_package;
using global::go.go;
using instlib = global::go.go.instlib_package;

partial class main_package {

public partial struct pair<T> {
    public T A;
    public nint B;
}

public static T First<T>(this pair<T> p) { return p.A; }

public partial struct duo<K, V> {
    public K Key;
    public V Val;
}

partial struct Q /*pair<nint>*/;

partial struct P<T> /*pair<T>*/;

partial struct R<U> /*pair<U>*/;

partial struct M /*global::go.go.instlib_package.Pair<@string>*/;

partial struct D /*duo<@string, nint>*/;

partial struct C<V> /*P<V>*/;

partial struct E /*C<bool>*/;

partial struct S {
    public nint A;
    public nint B;
}

internal static void Main() {
    var q = new Q(new pair<nint>(A: 3, B: 4));
    q.B++;
    var qs = slice<Q>([new(new pair<nint>(A: 5)), new(new pair<nint>(B: 6))]);
    ref var p = ref heap<P<@string>>(new P<@string>(new pair<@string>(A: "s", B: 1)), out var Ꮡp);
    var pp = Ꮡp;
    pp.Value.B = 7;
    var r = new R<nint>(new pair<nint>(A: 9));
    var m = new M(new lib.Pair<@string>(A: "m", B: 2));
    var d = new D(new duo<@string, nint>(Key: "k", Val: 8));
    var (lq, lp) = lib.Same();
    lp.B = 10;
    fmt.Println(q.A, q.B, qs[0].A, qs[1].B, p.A, p.B, r.A, m.A, m.B, d.Key, d.Val);
    fmt.Println(((pair<nint>)q).First(), ((lib.Pair<@string>)m).First(), len(lq), lq[1].A, lp.A, lp.B);

    P<float64> z = default;
    fmt.Println(z.A, z.B, r == new R<nint>(new pair<nint>(A: 9)));

    var c = new C<@string>(new pair<@string>(A: "c", B: 11));
    E e = default;
    e.A = true;
    e.B = c.B + 1;
    fmt.Println(c.A, c.B, e.A, e.B, ((pair<bool>)e).First(), ((pair<@string>)c).First(), ((P<@string>)c).A);

    var back = (C<@string>)new pair<@string>(A: "x");
    var eb = (E)new pair<bool>(B: 13);
    var s = (S)q;
    var sp = (S)new P<nint>(new pair<nint>(A: 14));
    fmt.Println(back.A, eb.B, s.A, s.B, sp.A, ((Q)s).B, ((P<nint>)s).A);
}

} // end main_package
