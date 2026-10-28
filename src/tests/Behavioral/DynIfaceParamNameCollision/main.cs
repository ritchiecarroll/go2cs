namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct pusher {
    internal @string id;
}

internal static error Push(this pusher p, @string target, nint weight) {
    fmt.Println("push", target, weight, "from", p.id);
    return default!;
}

internal static @string Label(this pusher p) {
    return "pusher:"u8 + p.id;
}

partial struct setter {
    internal @string id;
}

internal static void Set(this ref setter s, @string value) {
    fmt.Println("set", value, "on", s.id);
}

internal partial interface serve_type /*dyn*/ {
    error Push(@string target, nint weight);
    @string Label();
}

internal static void serve(any v) {
    {
        var (p, ok) = v._<serve_type>(ᐧ); if (ok) {
            fmt.Println("err:", p.Push("/style.css", 7), p.Label());
        } else {
            fmt.Println("not a pusher");
        }
    }
}

internal partial interface apply_type /*dyn*/ {
    void Set(@string value);
}

internal static void apply(any v) {
    {
        var (s, ok) = v._<apply_type>(ᐧ); if (ok) {
            s.Set("blue");
        } else {
            fmt.Println("not a setter");
        }
    }
}

internal static void Main() {
    serve(new pusher(id: "v"));

    serve(Ꮡ(new pusher(id: "p")));

    apply(Ꮡ(new setter(id: "s")));

    apply(new pusher(id: "x"));
}

} // end main_package
