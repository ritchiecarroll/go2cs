namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct T {
    internal nint n;
}

public static nint Foo(this T t) {
    return t.n;
}

internal partial interface main_d /*dyn*/ {
    nint Foo();
}

internal static void Main() {
    nint p = 42;
    var t = new T(7);
    var a = (any)(p);
    any b = p;
    var c = (main_d)(t);
    main_d d = t;
    fmt.Println(a, b, c.Foo(), d.Foo());
}

} // end main_package
