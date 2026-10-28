namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct ΔHeader /*map[@string, slice<@string>]*/;

partial struct Counts /*map[@string, nint]*/;

partial struct response {
    internal ΔHeader hdr;
}

internal static ΔHeader Header(this ref response r) {
    return r.hdr;
}

internal static ж<response> newResponse() {
    return Ꮡ(new response(hdr: new ΔHeader(new map<@string, slice<@string>>{})));
}

internal static void Main() {
    var r = newResponse();

    r.Header()["Content-Type"] = new @string[]{"text/plain"}.slice();
    r.Header()["X-Trace"] = new @string[]{"a", "b"}.slice();

    fmt.Println((~r).hdr["Content-Type"], len((~r).hdr["X-Trace"]));

    var c = new Counts(new map<@string, nint>{});
    var cʗ1 = c;
    Counts get() => cʗ1;

    get()["hits"] = 3;
    get()["hits"] = get()["hits"] + 1;

    fmt.Println(c["hits"], len(c));

    fmt.Println(r.Header()["Content-Type"][0]);

    var (absent, ok) = r.Header()["absent", ꟷ];
    fmt.Println(absent == default, ok);

    delete(r.Header(), "X-Trace"u8);
    fmt.Println(len(r.Header()), len((~r).hdr));
}

} // end main_package
