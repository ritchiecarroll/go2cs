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
    var selᴛ1 = ch;
    switch (select(ᐸꟷ(selᴛ1, ꓸꓸꓸ))) {
    case 0 when selᴛ1.ꟷᐳ(out sink[rec(n)]): {
        break;
    }}
}

internal static nint rec(nint n) {
    f(n - 1);
    return n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object sumˢ = (@string)"sum:"u8;

internal static void Main() {
    f(100);
    nint sum = 0;
    foreach (var (_, v) in sink.ΔRangeSnapshot()) {
        sum += v;
    }
    fmt.Println(sumˢ, sum);
}

} // end main_package
