namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct T {
    internal nint n;
}

internal static nint use(ref T t) {
    return t.n;
}

internal static nint outer(ref T t) {
    nint r = use(ref t);
    switch (ᐧ) {
    default: {
        ж<T> tΔ2 = Ꮡ(new T(n: 5));
        r += use(ref (tΔ2).DerefOrNull());
        r += (tΔ2.Value).n;
        break;
    }}

    return r;
}

internal static void Main() {
    var ᴛ1 = new T(n: 1);
    fmt.Println(outer(ref ᴛ1));
}

} // end main_package
