namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface source {
    @string Pull();
}

partial interface sink {
    void Push(@string s);
}

partial struct src {
    internal slice<@string> items;
    internal nint pos;
}

internal static @string Pull(this ref src s) {
    if (s.pos >= len(s.items)) {
        return ""u8;
    }
    @string item = s.items[s.pos];
    s.pos++;
    return item;
}

partial struct dst {
    internal slice<@string> log;
}

internal static void Push(this ref dst d, @string s) {
    d.log = append(d.log, s);
}

partial struct duplex {
    /*embed*/ internal source source;
    /*embed*/ internal sink sink;
}

internal static @string Status(this ref duplex d) {
    return "ok"u8;
}

partial interface conn {
    @string Pull();
    void Push(@string s);
    @string Status();
}

internal static void Main() {
    var @out = Ꮡ(new dst(nil));
    var d = Ꮡ(new duplex(source: new srcжsource(Ꮡ(new src(items: new @string[]{"alpha"u8, "beta"u8}.slice()))), sink: new dstжsink(@out)));
    conn c = new duplexжconn(d);
    c.Push(c.Pull());
    c.Push(c.Pull());
    fmt.Println(c.Status(), (~@out).log);
}

} // end main_package
