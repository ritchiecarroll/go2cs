namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

partial struct inner {
    internal nint k;
}

partial struct counter {
    internal nint n;
    internal inner sub;
}

internal static partial ж<counter> get(ж<counter> c) {
    return c;
}

internal static void Main() {
    var @base = Ꮡ(new counter(n: 5));
    @base.sub.k = 3;
    var c = get(@base);

    c.n++;
    c.n++;
    c.sub.k--;

    fmt.Println(@base.n, @base.sub.k);
}

} // end main_package
