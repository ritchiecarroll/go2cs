namespace go;

using fmt = fmt_package;
using ꓸꓸꓸV = Span<main_package.V>;
using ꓸꓸꓸжU = Span<ж<main_package.U>>;

partial class main_package {

partial struct S {
    public nint A;
}

partial struct U {
    public @string B;
}

partial struct V {
    public nint A;
}

internal static nint two(ref S p, ref U q) {
    return p.A + len(q.B);
}

internal static nint pair(S a, V b) {
    return a.A * 10 + b.A;
}

internal static nint tail(S a, params ꓸꓸꓸжU restʗp) {
    var rest = restʗp.sslice();

    return a.A + len(rest);
}

internal static nint spread(params ꓸꓸꓸV restʗp) {
    var rest = restʗp.sslice();

    return len(rest);
}

internal partial struct main_b /*dyn*/ {
    public nint A;
}

internal static void Main() {
    var (s, u) = (Ꮡ(new S(A: 1)), Ꮡ(new U(B: "xy")));
    fmt.Println("two:", two(ref (s).DerefOrNull(), ref (u).DerefOrNull()));
    fmt.Println("pair, unnamed struct into the second parameter:", pair(new S(1), new main_b(2)));
    fmt.Println("tail, variadic pointers:", tail(new S(1), u, u));
    fmt.Println("tail, expanded slice:", tail(new S(1), new ж<U>[]{u}.slice().ꓸꓸꓸ));
    fmt.Println("spread, variadic values:", spread(new V(1), new V(2)));
}

} // end main_package
