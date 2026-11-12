namespace go;

using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

// type HookType is a methodless func type — rendered inline as its base delegate
// type HookKind is a methodless func type — rendered inline as its base delegate
// type HookValue is a methodless func type — rendered inline as its base delegate

internal static (any, error) exec(any raw, reflect.Value from, reflect.Value to) {
    switch (raw.type()) {
    case Func<reflect.Type, reflect.Type, any, (any, error)> f: {
        return f(from.Type(), to.Type(), from.Interface());
    }
    case Func<reflect.Kind, reflect.Kind, any, (any, error)> f: {
        return f(from.Kind(), to.Kind(), from.Interface());
    }
    case Func<reflect.Value, reflect.Value, (any, error)> f: {
        return f(from, to);
    }
    case Func<nint, nint> f: {
        return (f(from.Interface()._<nint>()), default);
    }
    default: {
        var f = raw;
        return (default, errors.New("invalid decode hook signature"));
    }}
}

internal static @string kind(any raw) {
    switch (raw.type()) {
    case Func<reflect.Type, reflect.Type, any, (any, error)>: {
        return "type";
    }
    case Func<reflect.Kind, reflect.Kind, any, (any, error)> _:
    case Func<reflect.Value, reflect.Value, (any, error)> _: {
        return "kind or value";
    }}

    return "other";
}

internal static void main() {
    var (from, to) = (reflect.ValueOf((nint)21), reflect.ValueOf(""));
    var hooks = slice<any>([
        new Func<reflect.Type, reflect.Type, any, (any, error)>((reflect.Type f, reflect.Type t, any data) => (fmt.Sprintf("%s->%s:%v"u8, f, t, data), default)),
        new Func<reflect.Kind, reflect.Kind, any, (any, error)>((reflect.Kind f, reflect.Kind t, any data) => (fmt.Sprintf("%s->%s:%v"u8, f, t, data), default)),
        new Func<reflect.Value, reflect.Value, (any, error)>((reflect.Value f, reflect.Value t) => (f.Int() * 2, default)),
        nint (nint x) => x + 1,
        (nint)42
    ]);
    foreach (var (_, h) in range(hooks)) {
        var (@out, err) = exec(h, from, to);
        fmt.Println(@out, err, kind(h));
    }

    {
        var (f, ok) = hooks[2]._<Func<reflect.Value, reflect.Value, (any, error)>>(ᐧ); if (ok) {
            var (@out, _) = f(reflect.ValueOf((nint)5), to);
            fmt.Println("asserted", @out);
        }
    }
}

} // end main_package
