namespace go;

using fmt = fmt_package;

partial class main_package {

internal static array<nint> sink = new(101);

internal static void f(nint n) {
    if (n == 0) {
        return;
    }
    var ch = new channel<nint>(1);
    ch.ᐸꟷ(n);
    var chᴛ1 = ch;
    switch (select(ᐸꟷ(chᴛ1, ꓸꓸꓸ))) {
    case 0 when chᴛ1.ꟷᐳ(out sink[rec(n)]): {
        break;
    }}
}

internal static nint rec(nint n) {
    f(n - 1);
    return n;
}

internal static void main() {
    f(100);

    nint sum = 0;
    foreach (var (_, v) in range(sink.ΔRangeSnapshot())) {
        sum += v;
    }

    fmt.Println("sum:", sum);
}

} // end main_package
