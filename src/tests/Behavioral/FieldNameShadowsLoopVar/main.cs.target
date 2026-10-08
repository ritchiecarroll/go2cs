namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct pair {
    internal nint value;
    internal nint length;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object totalLengthˢ = (@string)"total length:"u8;

internal static slice<pair> build(slice<nint> lengths) {
    var pairs = new slice<pair>(len(lengths));
    foreach (var (i, lengthΔ1) in lengths) {
        pairs[i].value = i;
        pairs[i].length = lengthΔ1;
    }
    nint length = 0;
    foreach (var (_, p) in pairs) {
        length += p.length;
    }
    fmt.Println(totalLengthˢ, length);
    return pairs;
}

internal static void Main() {
    var pairs = build(new nint[]{5, 3, 8}.slice());
    foreach (var (_, p) in pairs) {
        fmt.Println(p.value, p.length);
    }
}

} // end main_package
