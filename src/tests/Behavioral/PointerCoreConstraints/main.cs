namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct thing {
    internal nint n;
    internal @string s;
}

internal static ж<T> clone<T>(ж<T> Ꮡp)
    /* where P : *T (erased: P renders as ж<T>) */
{
    ref var p = ref Ꮡp.DerefOrNull();

    ref var c = ref heap<T>(out var Ꮡc);
    c = p;
    return Ꮡc;
}

internal static T getThrough<T>(ж<T> Ꮡp)
    /* where P : *T (erased: P renders as ж<T>) */
{
    ref var p = ref Ꮡp.DerefOrNull();

    return p;
}

internal static void setThrough<T>(ж<T> Ꮡp, T v)
    /* where P : *T (erased: P renders as ж<T>) */
{
    ref var p = ref Ꮡp.DerefOrNull();

    p = v;
}

internal static ж<T> cloneRev<T>(ж<T> Ꮡp)
    /* where P : *T (erased: P renders as ж<T>) */
{
    ref var p = ref Ꮡp.DerefOrNull();

    ref var c = ref heap<T>(out var Ꮡc);
    c = p;
    return Ꮡc;
}

internal static ж<T> pick<T>(ж<T> Ꮡa, ж<T> Ꮡb, bool useA)
    /* where P : *T (erased: P renders as ж<T>) */
{
    if (useA) {
        return Ꮡa;
    }
    return Ꮡb;
}

internal static ж<T> cloneChain<T>(ж<T> Ꮡp, nint depth)
    /* where P : *T (erased: P renders as ж<T>) */
{
    ref var p = ref Ꮡp.DerefOrNull();

    if (depth > 0) {
        return cloneChain<T>(clone<T>(Ꮡp), depth - 1);
    }
    ref var c = ref heap<T>(out var Ꮡc);
    c = p;
    return Ꮡc;
}

internal static void aliasWrite<T>(ж<T> Ꮡp, T v)
    /* where P : *T (erased: P renders as ж<T>) */
{
    var q = Ꮡp;
    q.ValueSlot = v;
}

internal static T orZero<T>(ж<T> Ꮡp)
    /* where P : *T (erased: P renders as ж<T>) */
{
    ref var p = ref Ꮡp.DerefOrNull();

    if (Ꮡp == nil) {
        T z = GoZero<T>();
        return z;
    }
    return p;
}

partial interface PtrOf<T> {
    //  Type constraints: *T
    // Derived operators: none
}

internal static ж<T> cloneNamed<T>(ж<T> Ꮡp)
    /* where P : *T (erased: P renders as ж<T>) */
{
    ref var p = ref Ꮡp.DerefOrNull();

    ref var c = ref heap<T>(out var Ꮡc);
    c = p;
    return Ꮡc;
}

internal static ж<T> cloneEmbedded<T>(ж<T> Ꮡp)
    /* where P : *T (erased: P renders as ж<T>) */
{
    ref var p = ref Ꮡp.DerefOrNull();

    ref var c = ref heap<T>(out var Ꮡc);
    c = p;
    return Ꮡc;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string changedˢ = "changed"u8;

internal static void Main() {
    ref var t1 = ref heap<thing>(out var Ꮡt1);
    t1 = new thing(n: 1, s: "one"u8);
    var p1 = clone<thing>(Ꮡt1);
    t1.n = 42;
    fmt.Println((~p1).n, (~p1).s, t1.n);
    ref var n = ref heap<nint>(out var Ꮡn);
    n = 7;
    var pn = clone<nint>(Ꮡn);
    n = 100;
    fmt.Println(pn.Value, n);
    var g = getThrough<thing>(Ꮡt1);
    fmt.Println(g.n, g.s);
    setThrough(Ꮡn, 55);
    fmt.Println(n);
    var pr = cloneRev<thing>(Ꮡt1);
    pr.Value.n = 9;
    fmt.Println((~pr).n, t1.n);
    var pa = pick<thing>(Ꮡt1, p1, true);
    fmt.Println(pa == Ꮡt1, pick<thing>(Ꮡt1, p1, false) == p1);
    var pc = cloneChain<nint>(Ꮡn, 3);
    n = 1000;
    fmt.Println(pc.Value, n);
    aliasWrite(Ꮡn, 77);
    fmt.Println(n);
    fmt.Println(orZero<nint>(nil), orZero<nint>(Ꮡn));
    setThrough<nint>(Ꮡn, 8);
    fmt.Println(n);
    var pf = clone<thing>(Ꮡt1);
    fmt.Println((~pf).s);
    var fv = clone<thing>;
    var pv = fv(Ꮡt1);
    t1.s = changedˢ;
    fmt.Println((~pv).s, t1.s);
    var pn1 = cloneNamed<thing>(Ꮡt1);
    var pn2 = cloneEmbedded<thing>(Ꮡt1);
    fmt.Println((~pn1).s, (~pn2).s);
}

} // end main_package
