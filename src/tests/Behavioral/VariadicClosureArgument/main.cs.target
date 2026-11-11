namespace go;

using fmt = fmt_package;
using strings = strings_package;
using Δio = io_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static void header(Actionꓸꓸꓸ<@string, any> w, nint n) { w("n=%d %s", n, (@string)"x"); }

// type logFunc is a methodless func type — rendered inline as its base delegate

internal static void headerNamed(Actionꓸꓸꓸ<@string, any> w, nint n) { w("named n=%d", n); }

partial struct sink {
    internal Actionꓸꓸꓸ<@string, any> write;
}

internal static nint sum(Funcꓸꓸꓸ<nint, nint> add) { return add(1, 2, 3); }

internal static void main() {
    @string @out = "";
    Actionꓸꓸꓸ<@string, any> writeFormat = (@string format, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.sslice();
        @out += fmt.Sprintf(format, args.ꓸꓸꓸ) + ";"u8;
    };
    header(writeFormat, 2);
    headerNamed(new Actionꓸꓸꓸ<@string, any>(writeFormat), 3);
    var s = new sink(write: writeFormat);
    s.write("field %s", (@string)"ok");
    writeFormat("direct %d %d", (nint)4, (nint)5);
    fmt.Println(@out);

    ref var b = ref heap(new strings.Builder(), out var Ꮡb);
    Actionꓸꓸꓸ<@string, any> emit = (@string format, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.sslice();
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "["u8 + format + "]"u8, args.ꓸꓸꓸ);
    };
    header(emit, 6);
    emit = (@string format, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.sslice();
        fmt.Fprintf(new strings_BuilderжWriter(Ꮡb), "<"u8 + format + ">"u8, args.ꓸꓸꓸ);
    };
    header(emit, 7);
    fmt.Println(b.String());

    nint total = 0;
    Funcꓸꓸꓸ<nint, nint> add = (params ꓸꓸꓸnint xsʗp) => {
        var xs = xsʗp.sslice();
        foreach (var (_, x) in range(xs)) {
            total += x;
        }
        return total;
    };
    fmt.Println(sum(add), add(10), total);
}

} // end main_package
