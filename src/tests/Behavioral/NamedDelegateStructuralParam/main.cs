namespace go;

using fmt = fmt_package;

partial class main_package {

public delegate @string Handler(nint _Δp0, @string _Δp1);

internal static @string tag(this Handler h) {
    return "handler";
}

// type Plain is a methodless func type — rendered inline as its base delegate

internal static @string invoke(Func<nint, @string, @string> f, nint n, @string s) {
    return f(n, s);
}

internal static @string describe(nint n, @string s) {
    return fmt.Sprintf("%d-%s"u8, n, s);
}

internal static void Main() {
    Handler h = describe;
    fmt.Println(invoke(new Func<nint, @string, @string>(h), 1, "a"));
    fmt.Println(h.tag());

    Func<nint, @string, @string> p = describe;
    fmt.Println(invoke(p, 2, "b"));

    fmt.Println(invoke(describe, 3, "c"));
    fmt.Println(invoke((nint n, @string s) => s + "!"u8, 4, "d"));

    Handler h2 = (nint n, @string s) => fmt.Sprintf("%s/%d"u8, s, n);
    fmt.Println(invoke(new Func<nint, @string, @string>(h2), 5, "e"));

    var sc = Ꮡ(new screen(prefix: "#"));
    
    var scʗ1 = sc;
    Handler handler = (nint p1, @string p2) => scʗ1.render(p1, p2);
    fmt.Println(invoke(new Func<nint, @string, @string>(handler), 6, "f"));
    handler = makeHandler("T:");
    fmt.Println(invoke(new Func<nint, @string, @string>(handler), 7, "g"));
}

partial struct screen {
    internal @string prefix;
}

internal static @string render(this ref screen s, nint n, @string msg) {
    return fmt.Sprintf("%s%d-%s"u8, s.prefix, n, msg);
}

internal static Handler makeHandler(@string tag) {
    return (nint n, @string s) => fmt.Sprintf("%s%d%s"u8, tag, n, s);
}

} // end main_package
