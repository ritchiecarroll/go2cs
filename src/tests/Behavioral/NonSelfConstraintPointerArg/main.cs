namespace go;

using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;
using strings = strings_package;

partial class main_package {

partial interface optionParser {
    bool ParseOption(@string key, any value);
    @string UnknownOptionDiagnostic();
}

partial struct compilerOptionsParser {
    internal slice<@string> seen;
}

internal static bool ParseOption(this ref compilerOptionsParser p, @string key, any value) {
    p.seen = append(p.seen, fmt.Sprintf("%s=%v"u8, key, value));
    return key != "bad"u8;
}

internal static @string UnknownOptionDiagnostic(this ref compilerOptionsParser p) { return "unknown compiler option"; }

internal static O convertMapToOptions<O>(map<@string, any> m, slice<@string> keys, O parser)
    where O : optionParser
{
    foreach (var (_, k) in range(keys)) {
        if (!parser.ParseOption(k, m[k])) {
            fmt.Println(parser.UnknownOptionDiagnostic(), k);
        }
    }
    return parser;
}

internal static O zeroOr<O>(bool ok, O parser) {
    O zero = GoZero<O>();
    if (ok) {
        return parser;
    }
    return zero;
}

internal static (O, error) parse<O>(O parser, @string key)
    where O : optionParser
{
    if (!parser.ParseOption(key, (nint)1)) {
        return (parser, errors.New(parser.UnknownOptionDiagnostic()));
    }
    return (parser, default);
}

internal static void each<O>(O parser, slice<@string> keys, Action<O, @string> f) {
    foreach (var (_, k) in range(keys)) {
        f(parser, k);
    }
}

partial struct label {
    internal @string name;
}

internal static @string String(this ref label l) { return "label:"u8 + l.name; }
internal static @string Name(this ref label l) { return l.name; }

partial interface named :
    fmt.Stringer
{
    @string Name();
}

internal static S show<S>(S s)
    where S : fmt.Stringer
{
    fmt.Println("show:", s.String());
    return s;
}

internal static N describe<N>(N n)
    where N : named
{
    fmt.Println("describe:", n.String(), n.Name());
    return n;
}

partial struct temp {
    internal nint c;
}

internal static @string String(this temp t) { return fmt.Sprintf("%dC"u8, t.c); }

partial struct plain {
    internal @string k;
}

internal static bool ParseOption(this plain p, @string key, any value) { return key != p.k; }
internal static @string UnknownOptionDiagnostic(this plain p) { return "plain refuses "u8 + p.k; }

partial interface Cloner<T> {
    T Clone();
}

partial struct impl {
    internal nint n;
}

internal static ж<impl> Clone(this ref impl p) { return Ꮡ(new impl(p.n + 1)); }

internal static T dup<T>(T x)
    where T : Cloner<T>
{ return x.Clone(); }

internal static void Main() {
    var original = Ꮡ(new compilerOptionsParser());
    var p = convertMapToOptions(new map<@string, any>{["strict"] = true, ["bad"] = (nint)1}, slice<@string>(["strict", "bad"]), original);
    fmt.Println("result fields:", p.seen, len(p.seen));

    p.seen = append(p.seen, (@string)"written-through-result");
    fmt.Println("identity:", p == original, strings.Join(original.seen, ","));

    fmt.Println("zero:", zeroOr(false, original) == nil, zeroOr(true, original) == original);

    var (q, err) = parse(original, "bad");
    fmt.Println("tuple:", q == original, err);
    (_, err) = parse(original, "ok");
    fmt.Println("tuple ok:", err == nil, len(original.seen));
    convertMapToOptions(default, slice<@string>(["discarded"]), original);
    fmt.Println("discarded:", original.seen[len(original.seen) - 1]);

    each(original, slice<@string>(["x", "y"]), (ж<compilerOptionsParser> o, @string k) => {
        o.seen = append(o.seen, "cb-"u8 + k);
    });
    fmt.Println("callback:", original.seen.slice(len(original.seen) - 2));

    any asAny = convertMapToOptions(default, default, original).OrTypedNil();
    fmt.Println("reflect:", reflect.TypeOf(asAny));
    switch (asAny.type()) {
    case ж<compilerOptionsParser> v: {
        fmt.Println("switch: *compilerOptionsParser", v == original);
        break;
    }
    default: {
        var v = asAny;
        fmt.Println("switch: other");
        break;
    }}

    var l = Ꮡ(new label(name: "a"));
    fmt.Println("foreign:", show(l).name, show(l) == l);
    fmt.Println("embedded:", describe(l).name);

    var t = Ꮡ(new temp(c: 21));
    show(t).c++;
    fmt.Println("value receiver:", t.c);

    fmt.Println("value arg:", convertMapToOptions(new map<@string, any>{["k"] = (nint)1}, slice<@string>(["k"]), new plain(k: "k")).k);

    fmt.Println("self route:", dup<implжCloner>(Ꮡ(new impl(n: 1))).n);
}

} // end main_package
