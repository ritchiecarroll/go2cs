namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct request {
    internal @string path;
    internal bool closed;
    internal nint depth;
}

[GoLazy] partial class lazyᴛdescribe { internal static Func<ж<request>, @string, @string> describe = (ж<request> r, @string tag) => fmt.Sprint(tag, ":", r.path, "/", r.closed, "/", r.depth, r.label()); }

[GoLazy] partial class lazyᴛdeepen { internal static Action<ж<request>> deepen = (ж<request> r) => {
    r.depth = r.depth + 2;
    r.closed = !r.closed;
}; }

internal static Func<ж<request>, @string> pathOf = (ж<request> r) => r.path;

internal static @string label(this ref request r) {
    return "["u8 + r.path + "]"u8;
}

internal static @string inside() {
    @string f(ж<request> r) => r.path + "!"u8;

    return f(Ꮡ(new request(path: "/ctl")));
}

internal static void Main() {
    var r = Ꮡ(new request(path: "/a", closed: true, depth: 1));

    fmt.Println(describe(r, "first"));

    deepen(r);

    fmt.Println(describe(r, "second"));
    fmt.Println(pathOf(r), inside());
}

} // end main_package
