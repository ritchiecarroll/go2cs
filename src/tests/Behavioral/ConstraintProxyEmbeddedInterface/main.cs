namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Base {
    @string Name();
}

partial interface Middle :
    Base
{
    nint Size();
}

partial interface Constrained<T> :
    Middle
{
    T Clone();
    bool Each(@string label, Action<T> f);
    T Pick(Func<T> gen);
}

partial struct Impl {
    internal @string n;
    internal nint s;
}

public static @string Name(this ref Impl p) {
    return p.n;
}

public static nint Size(this ref Impl p) {
    return p.s;
}

public static ж<Impl> Clone(this ref Impl p) {
    return Ꮡ(new Impl(p.n, p.s + 1));
}

public static bool Each(this ref Impl p, @string label, Action<ж<Impl>> f) {
    f(Ꮡ(new Impl(p.n + "/"u8 + label, p.s)));
    return true;
}

public static ж<Impl> Pick(this ref Impl p, Func<ж<Impl>> gen) {
    return gen();
}

internal static void use<T>(T v)
    where T : Constrained<T>
{
    var c = v.Clone();
    fmt.Println(c.Name(), c.Size(), v.Name(), v.Size());

    var ok = v.Each("leaf", (T child) => {
        fmt.Println("each", child.Name(), child.Size());
    });

    var picked = v.Pick(T () => v.Clone());

    fmt.Println(ok, picked.Name(), picked.Size());
}

internal static @string second<T>(T v)
    where T : Constrained<T>
{
    var c = v.Clone();
    return fmt.Sprint(c.Name(), "/", c.Size(), "/", v.Name(), "/", v.Size());
}

internal static void callback<T>(T v, Action<T, nint> f)
    where T : Constrained<T>
{
    f(v, 7);
    f(v.Clone(), 8);
}

internal static void Main() {
    use<ImplжConstrained>(Ꮡ(new Impl("alpha", 1)));
    use<ImplжConstrained>(Ꮡ(new Impl("beta", 10)));

    fmt.Println(second<ImplжConstrained>(Ꮡ(new Impl("gamma", 100))));

    callback<ImplжConstrained>(Ꮡ(new Impl("delta", 1000)), (ImplжConstrained tΔ1Δp, nint mode) => {
        var tΔ1 = (ж<Impl>)tΔ1Δp;
        fmt.Println(tΔ1.Name(), tΔ1.Size(), mode);
    });

    @string t = "outer";

    callback<ImplжConstrained>(Ꮡ(new Impl("epsilon", 2000)), (ImplжConstrained tΔ2Δp, nint mode) => {
        var tΔ2 = (ж<Impl>)tΔ2Δp;
        fmt.Println(tΔ2.Name(), tΔ2.Size(), mode);
    });

    fmt.Println(t);
}

} // end main_package
