namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial struct validator {
    internal nint @base;
}

} partial class mainᴛvalidator {

internal static nint check(this validator v, nint n) {
    return v.@base + n;
}

} partial class main_package {

partial struct counter {
    internal nint n;
}

} partial class mainᴛcounter {

internal static nint bump(this ref counter c, nint by) {
    c.n += by;
    return c.n;
}

} partial class main_package {

internal static void main() {
    Func<nint, nint> fn = default;

    var v = new validator(@base: 100);
    var vʗ1 = v;
        fn = (nint p1) => vʗ1.check(p1);
    fmt.Println(fn(5));

    var w = new validator(@base: 10);
    switch (ᐧ) {
    case {} when true: {
        var wʗ1 = w;
                fn = (nint p1) => wʗ1.check(p1);
        break;
    }}

    fmt.Println(fn(7));

    var x = new validator(@base: 1);
    var xʗ1 = x;
        fn = (nint p1) => xʗ1.check(p1);
    fmt.Println(fn(7));

    Func<nint, nint> add = default;
    ref var c = ref heap<counter>(new counter(n: 5), out var Ꮡc);
    add = Ꮡc.bump;
    fmt.Println(add(3));
    fmt.Println(add(4));
    fmt.Println(c.n);
}

} // end main_package
