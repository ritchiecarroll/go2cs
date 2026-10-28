namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct inner {
    internal nint n;
    internal nint p;
}

internal static @string describe(this inner i) {
    return fmt.Sprintf("n=%d p=%d"u8, i.n, i.p);
}

partial struct mid {
    /*embed*/ internal inner inner;
    internal nint m;
}

partial struct top {
    /*embed*/ internal mid mid;
    internal nint t;
}

internal static void Main() {
    var x = new top();
    x.n = 1;
    x.p = 9;
    x.m = 2;
    x.t = 3;

    fmt.Println(x.n, x.p, x.m, x.t);
    fmt.Println(x.inner.n, x.mid.m);
    fmt.Println(x.describe());

    x.n = 42;
    fmt.Println(x.n, x.inner.n);
}

} // end main_package
