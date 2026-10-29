namespace go;

using errors = errors_package;
using fmt = fmt_package;
using ꓸꓸꓸany = Span<any>;
using ꓸꓸꓸnint = Span<nint>;

partial class main_package {

internal static UntypedInt letter => /* 'c' */ 99;

internal static (rune, bool) pair() {
    return ('p', true);
}

internal static void Main() {
    var noArgs = () => {
    };
    var square = (int32 x) => x * x;
    var parse = (@string s, nint n) => {
        if (len(s) > n) {
            return (false, errors.New("too long"));
        }
        return (true, default);
    };
    var sum = (params ꓸꓸꓸnint xsʗp) => {
        var xs = xsʗp.sslice();
        nint total = 0;
        foreach (var (_, x) in xs) {
            total += x;
        }
        return total;
    };
    var format = @string (@string f, params ꓸꓸꓸany argsʗp) => {
        var args = argsʗp.sslice();
        return fmt.Sprintf(f, args.ꓸꓸꓸ);
    };

    fmt.Printf("%T\n"u8, (noArgs).OrTypedNilFunc());
    fmt.Printf("%T\n"u8, (square).OrTypedNilFunc());
    fmt.Printf("%T\n"u8, (parse).OrTypedNilFunc());
    fmt.Printf("%T\n"u8, (sum).OrTypedNilFunc());
    fmt.Printf("%T\n"u8, (format).OrTypedNilFunc());
    var (ok, err) = parse("go", 5);
    fmt.Println(square(7), ok, err, sum(1, 2, 3), format("%d-%s", (nint)4, (@string)"x"u8));

    Func<int32, int32> none = default;
    any boxed = (none).OrTypedNilFunc();
    fmt.Printf("%T %v %v\n"u8, boxed, boxed, none == nil);

    var shrink = (rune rΔ1) => (rune)'a';
    var both = () => ((rune)'b', true);
    var named = rune () => letter;
    fmt.Printf("%T %T %T\n"u8, (shrink).OrTypedNilFunc(), (both).OrTypedNilFunc(), (named).OrTypedNilFunc());
    var (r, flag) = both();
    fmt.Printf("%v %v %v %v\n"u8, shrink('x'), r, flag, named());
    fmt.Printf("%T %T\n"u8, shrink('x'), r);

    any held = (shrink).OrTypedNilFunc();
    var fns = slice<Func<rune>>([() => (rune)'f']);
    var byName = new map<@string, Func<rune>>{["g"] = () => (rune)'g'};
    fmt.Printf("%T %T %T %v %v\n"u8, held, fns, byName, fns[0](), byName["g"]());
    var (p, q) = pair();
    fmt.Println(p, q, (rune)'z', (int32)(letter));
}

} // end main_package
