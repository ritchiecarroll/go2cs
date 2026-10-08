namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint val;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string nilˢ = "nil"u8;
private static readonly @string setˢ = "set"u8;

internal static @string onlyNilCheck(ж<node> Ꮡp) {
    if (Ꮡp == nil) {
        return nilˢ;
    }
    return setˢ;
}

internal static bool inner(ж<node> Ꮡp) {
    return Ꮡp == nil;
}

internal static bool passThrough(ж<node> Ꮡp) {
    return inner(Ꮡp);
}

internal static nint usesValue(ref node p) {
    return p.val;
}

internal static void Main() {
    fmt.Println(onlyNilCheck(nil));
    fmt.Println(onlyNilCheck(Ꮡ(new node(nil))));
    fmt.Println(passThrough(nil));
    fmt.Println(passThrough(Ꮡ(new node(nil))));
    var ᴛ1 = new node(val: 9);
    fmt.Println(usesValue(ref ᴛ1));
}

} // end main_package
