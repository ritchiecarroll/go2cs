namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct gList {
    internal nint n;
}

internal static (gList, nint) makeList() {
    return (new gList(n: 7), 3);
}

internal static void use(ref gList g) {
    g.n++;
}

internal static (nint, nint) run() {
    var (list, delta) = makeList();
    use(ref list);
    use(ref list);
    return (list.n, delta);
}

internal static void Main() {
    var (n, delta) = run();
    fmt.Println(n, delta);
}

} // end main_package
