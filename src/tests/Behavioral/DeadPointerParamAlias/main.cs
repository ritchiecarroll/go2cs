namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct node {
    internal nint val;
}

internal static @string onlyNilCheck(ж<node> p) {
    if (p == nil) {
        return "nil";
    }
    return "set";
}

internal static bool inner(ж<node> p) {
    return p == nil;
}

internal static bool passThrough(ж<node> p) {
    return inner(p);
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
