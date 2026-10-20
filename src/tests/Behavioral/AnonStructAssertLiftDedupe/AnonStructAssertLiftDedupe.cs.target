namespace go;

using fmt = fmt_package;

partial class main_package {

internal partial struct main_p /*dyn*/ {
    public nint X, Y;
}

internal partial struct main_s /*dyn*/ {
    public @string A;
}

internal partial struct main_type /*dyn*/ {
    public @string X, Y;
}

internal static void Main() {
    ref var p = ref heap(new main_p(), out var Ꮡp);
    p.X = 1;
    p.Y = 2;

    any i = p;

    p = i._<main_p>();
    fmt.Println("A1 assert into the original var:", p.X, p.Y);

    var q = i._<main_p>();
    fmt.Println("A2 assert into a new var:", q.X, q.Y);

    q = p;
    fmt.Println("A3 original assigns into asserted var:", q.X, q.Y);

    p = q;
    fmt.Println("A4 asserted var assigns into original:", p.X, p.Y);

    var (r, ok) = i._<main_p>(ᐧ);
    fmt.Println("A5 comma-ok:", r.X, r.Y, ok);

    any third = new main_p(X: 8, Y: 9);
    var u = third._<main_p>();
    p = u;
    fmt.Println("A6 third occurrence:", p.X, p.Y);

    fmt.Println("A7 equal across occurrences:", q == u);

    var pp = Ꮡp;
    any j = pp.OrTypedNil();
    var pq = j._<ж<main_p>>();
    fmt.Println("B1 pointer assertion:", (~pq).X, (~pq).Y);

    main_s s = default!;
    s.A = "hi";

    any k = s;
    var t = k._<main_s>();
    s = t;
    fmt.Println("B2 different shape:", s.A);

    any w = new main_type(X: "a", Y: "b");
    var x = w._<main_type>();
    fmt.Println("B3 same names different types:", x.X, x.Y);

    var (_, bad) = i._<main_s>(ᐧ);
    fmt.Println("B4 mismatched assertion:", bad);
}

} // end main_package
