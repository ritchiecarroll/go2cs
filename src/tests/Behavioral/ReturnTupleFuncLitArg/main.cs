namespace go;

using fmt = fmt_package;

partial class main_package {

internal delegate void handler(@string msg);

internal static void invoke(this handler h, @string msg) {
    h(msg);
}

internal static handler wrap(handler h) {
    return h;
}

internal static (handler, @string, error) makeHandlers(@string prefix) {
    var allowed = new @string[]{prefix + "-a"u8, prefix + "-b"u8}.slice();
    var allowedʗ1 = allowed;
    return (new handler((@string msg) => {
        fmt.Println(allowedʗ1[0] + ":" + msg, len(allowedʗ1));
    }), "label", default!);
}

internal static handler makeSingle(@string tag) {
    var counts = new map<@string, nint>{["x"] = len(tag)};
    var countsʗ1 = counts;
    return new handler((@string msg) => {
        fmt.Println(countsʗ1["x"], msg);
    });
}

internal static handler makePlain(nint n) {
    var limits = new nint[]{n, n * 2}.slice();
    var limitsʗ1 = limits;
    return wrap((@string msg) => {
        fmt.Println(limitsʗ1[1], msg);
    });
}

internal static void Main() {
    var (h, label, err) = makeHandlers("go");
    fmt.Println(label, err);
    h.invoke("ping");

    var s = makeSingle("four");
    s("pong");

    var p = makePlain(3);
    p("plain");
}

} // end main_package
