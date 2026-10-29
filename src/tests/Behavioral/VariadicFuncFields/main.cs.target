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

    @string joined = "";

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

    if (c.JoinPath != nil) {
        return c.JoinPath(elem.ꓸꓸꓸ);
    }

    return slashJoin(elem.ꓸꓸꓸ);
}

internal static void Main() {
    var ctxt = new Context(
        Name: "demo",
        JoinPath: slashJoin,
        Log: (@string format, params ꓸꓸꓸany argsʗp) => {
            var args = argsʗp.sslice();
            fmt.Printf(format + "\n"u8, args.ꓸꓸꓸ);
        }
    );

    fmt.Println(ctxt.JoinPath("a", "b", "c"));

    fmt.Println(ctxt.JoinPath() == ""u8);

    var parts = slice<@string>(["x", "y"]);
    fmt.Println(ctxt.join(parts.ꓸꓸꓸ));

    ctxt.JoinPath = @string (params ꓸꓸꓸstring elemʗp) => {
        var elem = elemʗp.sslice();
        return fmt.Sprint(len(elem));
    };
    fmt.Println(ctxt.JoinPath("p", "q"));

    ctxt.Log("%s:%d", ctxt.Name, (nint)3);
    ctxt.Log("done");
}

} // end main_package
