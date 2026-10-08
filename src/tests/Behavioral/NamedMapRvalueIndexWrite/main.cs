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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string contentTypeˢ = "Content-Type"u8;
private static readonly @string xTraceˢ = "X-Trace"u8;
private static readonly @string hitsˢ = "hits"u8;
private static readonly @string absentˢ = "absent"u8;

internal static void Main() {
    var r = newResponse();
    r.Header()[contentTypeˢ] = new @string[]{"text/plain"u8}.slice();
    r.Header()[xTraceˢ] = new @string[]{"a"u8, "b"u8}.slice();
    fmt.Println((~r).hdr[contentTypeˢ], len((~r).hdr[xTraceˢ]));
    var c = new Counts(new map<@string, nint>{});
    var cʗ1 = c;
    Counts get() => cʗ1;
    get()[hitsˢ] = 3;
    get()[hitsˢ] = get()[hitsˢ] + 1;
    fmt.Println(c[hitsˢ], len(c));
    fmt.Println(r.Header()[contentTypeˢ][0]);
    var (absent, ok) = r.Header()[absentˢ, ꟷ];
    fmt.Println(absent == default!, ok);
    delete(r.Header(), "X-Trace"u8);
    fmt.Println(len(r.Header()), len((~r).hdr));
}

} // end main_package
