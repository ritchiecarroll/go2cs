namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Box<T> {
    internal T v;
}

partial struct table<T> /*[3]Box<T>*/;

partial struct vec<T> /*[2]T*/;

internal static void Main() {
    table<nint> t = default!;
    t[0].v = 5;
    t[2].v = 9;
    fmt.Println(t[0].v, t[2].v, len(t));
    vec<nint> u = default!;
    u[0] = 7;
    u[1] = 8;
    fmt.Println(u[0], u[1], len(u));
}

} // end main_package
