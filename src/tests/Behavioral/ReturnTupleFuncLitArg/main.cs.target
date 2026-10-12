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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string labelˢ = "label"u8;

internal static (handler, @string, error) makeHandlers(@string prefix) {
    var allowed = new @string[]{prefix + "-a"u8, prefix + "-b"u8}.slice();
    var allowedʗ1 = allowed;
    return (new handler((@string msg) => {
        fmt.Println(allowedʗ1[0] + ":" + msg, len(allowedʗ1));
    }), labelˢ, default!);
}

internal static handler makeSingle(@string tag) {
    var counts = new map<@string, nint>{["x"u8] = len(tag)};
    var countsʗ1 = counts;
    return new handler((@string msg) => {
        fmt.Println(countsʗ1["x"u8], msg);
    });
}

internal static handler makePlain(nint n) {
    var limits = new nint[]{n, n * 2}.slice();
    var limitsʗ1 = limits;
    return wrap((@string msg) => {
        fmt.Println(limitsʗ1[1], msg);
    });
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string pingˢ = "ping"u8;
private static readonly @string fourˢ = "four"u8;
private static readonly @string pongˢ = "pong"u8;
private static readonly @string plainˢ = "plain"u8;

internal static void Main() {
    var (h, label, err) = makeHandlers("go"u8);
    fmt.Println(label, err);
    h.invoke(pingˢ);

    var s = makeSingle(fourˢ);
    s(pongˢ);

    var p = makePlain(3);
    p(plainˢ);
}

} // end main_package
