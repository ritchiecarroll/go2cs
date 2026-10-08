namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface sizer {
    nint size(@string of);
}

partial struct flat {
    internal nint n;
}

internal static nint size(this flat f, @string of) {
    return f.n + len(of);
}

partial struct config {
    internal sizer s;
}

internal static sizer std = new flat(n: 100);

internal static Func<@string, nint> pickSizer(this ref config c) {
    var f = std.size;
    if (c.s != default!) {
        f = c.s.size;
    }
    c.s = new flat(n: 1000);
    return f;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string abcˢ = "abc"u8;
private static readonly @string abcdˢ = "abcd"u8;

internal static void Main() {
    var c = Ꮡ(new config(s: new flat(n: 10)));
    var f = c.pickSizer();
    fmt.Println(f(abcˢ));
    fmt.Println((~c).s.size(abcˢ));
    var c2 = Ꮡ(new config(nil));
    var g = c2.pickSizer();
    fmt.Println(g(abcdˢ));
}

} // end main_package
