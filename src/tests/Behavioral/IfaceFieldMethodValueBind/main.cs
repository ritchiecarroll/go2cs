namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial interface sizer {
    nint size(@string of);
}

partial struct flat {
    internal nint n;
}

} partial class mainᴛflat {

internal static nint size(this flat f, @string of) {
    return f.n + len(of);
}

} partial class main_package {

partial struct config {
    internal sizer s;
}

internal static sizer std = new flat(n: 100);

} partial class mainᴛconfig {

internal static Func<@string, nint> pickSizer(this ref config c) {
    var f = std.size;

    if (c.s != nil) {
        f = c.s.size;
    }

    c.s = new flat(n: 1000);
    return f;
}

} partial class main_package {

internal static void main() {
    var c = Ꮡ(new config(s: new flat(n: 10)));
    var f = c.pickSizer();
    fmt.Println(f("abc"));
    fmt.Println(c.s.size("abc"));

    var c2 = Ꮡ(new config());
    var g = c2.pickSizer();
    fmt.Println(g("abcd"));
}

} // end main_package
