namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct validator {
    internal nint @base;
}

internal static nint check(this validator v, nint n) {
    return v.@base + n;
}

partial struct counter {
    internal nint n;
}

internal static nint bump(this ref counter c, nint by) {
    c.n += by;
    return c.n;
}

internal static void Main() {
    Func<nint, nint> fn = default!;
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
    Func<nint, nint> add = default!;
    ref var c = ref heap<counter>(out var Ꮡc);
    c = new counter(n: 5);
    add = Ꮡc.bump;
    fmt.Println(add(3));
    fmt.Println(add(4));
    fmt.Println(c.n);
}

} // end main_package
