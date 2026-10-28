namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint val;
}

internal static @string onlyNilCheck(ж<node> Ꮡp) {
    if (Ꮡp == nil) {
        return "nil";
    }
    return "set";
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
    fmt.Println(onlyNilCheck(Ꮡ(new node())));
    fmt.Println(passThrough(nil));
    fmt.Println(passThrough(Ꮡ(new node())));
    var ᴛ1 = new node(val: 9);
    fmt.Println(usesValue(ref ᴛ1));
}

} // end main_package
