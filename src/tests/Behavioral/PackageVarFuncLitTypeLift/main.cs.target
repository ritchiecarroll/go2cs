namespace go;

using fmt = fmt_package;
using static main_package;

partial class main_package {

partial interface Greeter {
    @string Greet();
}

partial struct namedGreeter {
    internal @string name;
}

} partial class mainᴛnamedGreeter {

internal static @string Greet(this namedGreeter g) {
    return "hello "u8 + g.name;
}

} partial class main_package {


partial struct makersᴛ1 /*dyn*/ {
    internal @string label;
    internal Func<@string, Greeter> build;
}

internal partial struct makers_type /*dyn*/ {
    /*embed*/ public Greeter Greeter;
}
[GoLazy] partial class lazyᴛmakers { internal static slice<makersᴛ1> makers = [
    new("direct", (@string s) => new namedGreeter(s)),
    new("embedded", (@string s) => new makers_type(new namedGreeter(s)))
]; }

internal static void main() {
    foreach (var (_, m) in range(makers)) {
        fmt.Println(m.label, m.build("world").Greet());
    }

    fmt.Println(varFirstLabel, varFirst.Greet());
}

} // end main_package
