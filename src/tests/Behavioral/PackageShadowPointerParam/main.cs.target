namespace go;

using fmt = fmt_package;
using Δio = io_package;

partial class main_package {

partial struct box {
    internal nint n;
}

internal static nint helper(ref box b) {
    return b.n;
}

internal static nint consume(ref box ioΔ1, Δio.Writer w) {
    _ = w;
    return ioΔ1.n + helper(ref ioΔ1);
}

internal static nint combine(Δio.Writer ioΔ1, ref box p) {
    _ = ioΔ1;
    return p.n * 2;
}

internal static void Main() {
    var b = new box(n: 7);
    fmt.Println(consume(ref b, default!));
    fmt.Println(combine(default!, ref b));
}

} // end main_package
