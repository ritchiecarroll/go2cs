namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

internal static partial void forEach(slice<nint> items, Action<nint> f) {
    foreach (var (_, x) in items) {
        f(x);
    }
}

internal static partial (nint n, bool ok) run() {
    nint n = default!;
    bool ok = default!;

    var items = new nint[]{1, 2, 3, -1, 4}.slice();
    forEach(items, (nint x) => {
        if (x < 0) {
            return;
        }
        n += x;
    });
    ok = n > 0;
    return (n, ok);
}

internal static void Main() {
    var (n, ok) = run();
    fmt.Println(n, ok);
}

} // end main_package
