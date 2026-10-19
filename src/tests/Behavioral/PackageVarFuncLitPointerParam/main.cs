namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct request {
    internal @string path;
    internal bool closed;
    internal nint depth;
}

internal static Func<ж<request>, @string, @string> describe = (ж<request> r, @string tag) => fmt.Sprint(tag, (@string)":"u8, (~r).path, (@string)"/"u8, (~r).closed, (@string)"/"u8, (~r).depth, r.label());

internal static Action<ж<request>> deepen = (ж<request> r) => {
    r.Value.depth = (~r).depth + 2;
    r.Value.closed = !(~r).closed;
};

internal static Func<ж<request>, @string> pathOf = (ж<request> r) => (~r).path;

internal static @string label(this ref request r) {
    return "["u8 + r.path + "]"u8;
}

internal static @string inside() {
    @string f(ж<request> r) => (~r).path + "!"u8;

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
