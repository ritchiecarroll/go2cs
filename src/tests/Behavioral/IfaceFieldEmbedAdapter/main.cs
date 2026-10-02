namespace go;

using fmt = fmt_package;

partial class main_package {

[GoType] partial interface source {
    @string Pull();
}

[GoType] partial interface sink {
    void Push(@string s);
}

[GoType] partial struct src {
    internal slice<@string> items;
    internal nint pos;
}

[GoRecv] internal static @string Pull(this ref src s) {
    if (s.pos >= len(s.items)) {
        return ""u8;
    }
    @string item = s.items[s.pos];
    s.pos++;
    return item;
}

[GoType] partial struct dst {
    internal slice<@string> log;
}

[GoRecv] internal static void Push(this ref dst d, @string s) {
    d.log = append(d.log, s);
}

[GoType] partial struct duplex {
    [GoEmbedded] internal source source;
    [GoEmbedded] internal sink sink;
}

[GoRecv] internal static @string Status(this ref duplex d) {
    return "ok"u8;
}

[GoType] partial interface conn {
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
