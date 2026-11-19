namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct T {
    internal nint n;
}

} partial class mainᴛT {

public static nint Foo(this T t) { return t.n; }

} partial class main_package {

internal partial interface main_d /*dyn*/ {
    nint Foo();
}

internal static void main() {
    nint p = 42;
    var t = new T(7);

    var a = (any)(p);

    any b = p;

    var c = (main_d)(t);

    main_d d = t;

    fmt.Println(a, b, c.Foo(), d.Foo());
}

} // end main_package
