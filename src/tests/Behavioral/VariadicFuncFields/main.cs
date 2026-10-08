namespace go;

using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸstring = Span<@string>;

partial class main_package {

partial struct Context {
    public @string Name;
    public Funcꓸꓸꓸ<@string, @string> JoinPath;
    public Actionꓸꓸꓸ<@string, any> Log;
}

internal static @string slashJoin(params ꓸꓸꓸstring elemʗp) {
    var elem = elemʗp.sslice();

    @string joined = ""u8;
    foreach (var (i, e) in elem) {
        if (i > 0) {
            joined += "/"u8;
        }
        joined += e;
    }
    return joined;
}

internal static @string join(this ref Context c, params ꓸꓸꓸstring elemʗp) {
    var elem = elemʗp.sslice();

    if (c.JoinPath != default!) {
        return c.JoinPath(elem.ꓸꓸꓸ);
    }
    return slashJoin(elem.ꓸꓸꓸ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string doneˢ = "done"u8;

internal static void Main() {
    var ctxt = new Context(
        Name: "demo"u8,
        JoinPath: slashJoin,
        Log: (@string format, params ꓸꓸꓸany argsʗp) => {
            var args = argsʗp.sslice();
            fmt.Printf(format + "\n"u8, args.ꓸꓸꓸ);
        }
    );
    fmt.Println(ctxt.JoinPath("a"u8, "b", "c"));
    fmt.Println(ctxt.JoinPath() == ""u8);
    var parts = new @string[]{"x"u8, "y"u8}.slice();
    fmt.Println(ctxt.join(parts.ꓸꓸꓸ));
    ctxt.JoinPath = @string (params ꓸꓸꓸstring elemʗp) => {
        var elem = elemʗp.sslice();
        return fmt.Sprint(len(elem));
    };
    fmt.Println(ctxt.JoinPath("p"u8, "q"));
    ctxt.Log("%s:%d"u8, ctxt.Name, (nint)(3));
    ctxt.Log(doneˢ);
}

} // end main_package
