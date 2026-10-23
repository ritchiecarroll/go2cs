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
[GoLazy] partial class lazyᴛmakers { internal static slice<makersᴛ1> makers = new makersᴛ1[]{
    new("direct", (@string s) => new namedGreeter(s)),
    new("embedded", (@string s) => new makers_type(new namedGreeter(s)))
}.slice(); }

internal static void Main() {
    foreach (var (_, m) in makers) {
        fmt.Println(m.label, m.build("world").Greet());
    }

    fmt.Println(varFirstLabel, varFirst.Greet());
}

} // end main_package
