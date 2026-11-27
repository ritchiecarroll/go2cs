namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void main() {
    var c = new channel<nint>(1);

    nint w = 1;
    var cʗ1 = c;
    [GoFunc("main.func1")] channel<nint> getC() {
        w = 10;
        return cʗ1;
    }
    getC().ᐸꟷ(w);
    fmt.Println("channel from a call:", ᐸꟷ(c));

    nint v = 1;
    [GoFunc("main.func2")] nint setV() {
        v = 10;
        return 2;
    }
    var evalᴛ1 = setV();
    c.ᐸꟷ(v + evalᴛ1);
    fmt.Println("value with a call:", ᐸꟷ(c));

    nint u = 1;
    [GoFunc("main.func3")] nint setU() {
        u = 10;
        return 2;
    }
    var evalᴛ2 = setU();
    var cᴛ1 = c.ᐸꟷ(u + evalᴛ2, ꓸꓸꓸ);
    switch (select(cᴛ1)) {
    case 0: {
        break;
    }}
    fmt.Println("select send:", ᐸꟷ(c));
}

} // end main_package
