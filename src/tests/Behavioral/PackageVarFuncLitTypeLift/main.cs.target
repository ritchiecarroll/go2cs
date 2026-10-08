namespace go;

using fmt = fmt_package;

partial class main_package {

partial interface Greeter {
    @string Greet();
}

partial struct namedGreeter {
    internal @string name;
}

internal static @string Greet(this namedGreeter g) {
    return "hello "u8 + g.name;
}


partial struct makersᴛ1 /*dyn*/ {
    internal @string label;
    internal Func<@string, Greeter> build;
}

internal partial struct makers_type /*dyn*/ {
    /*embed*/ public Greeter Greeter;
}
internal static slice<makersᴛ1> makers = new makersᴛ1[]{
    new("direct"u8, (@string s) => new namedGreeter(s)),
    new("embedded"u8, (@string s) => new makers_type(new namedGreeter(s)))
}.slice();

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string worldˢ = "world"u8;

internal static void Main() {
    foreach (var (_, m) in makers) {
        fmt.Println(m.label, m.build(worldˢ).Greet());
    }
    fmt.Println(varFirstLabel, varFirst.Greet());
}

} // end main_package
