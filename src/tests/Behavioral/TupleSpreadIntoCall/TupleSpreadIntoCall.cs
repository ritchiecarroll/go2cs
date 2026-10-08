namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct Node {
    internal nint a, b;
}

internal static (nint, nint) parts() {
    return (3, 4);
}

internal static ж<Node> makeNode(nint a, nint b) {
    return Ꮡ(new Node(a: a, b: b));
}

internal static nint combine(nint a, nint b) {
    return a * 10 + b;
}

internal static ж<Node> build() {
    var (ᴛ1, ᴛ2) = parts();
    var r = makeNode(ᴛ1, ᴛ2);
    return r;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recordedˢ = (@string)"recorded:"u8;

internal static void record(nint a, nint b) {
    fmt.Println(recordedˢ, a, b);
}

internal static (nint, nint) tupleᴛ1ʗ = parts();
internal static nint g = combine(tupleᴛ1ʗ.Item1, tupleᴛ1ʗ.Item2);

internal static void Main() {
    var n = build();
    var (ᴛ3, ᴛ4) = parts();
    nint s = combine(ᴛ3, ᴛ4);
    fmt.Println((~n).a, (~n).b, s);
    var (ᴛ5, ᴛ6) = parts();
    record(ᴛ5, ᴛ6);
    fmt.Println(g);
}

} // end main_package
