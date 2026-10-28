namespace go;

using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

// type HookType is a methodless func type — rendered inline as its base delegate

// type HookKind is a methodless func type — rendered inline as its base delegate

// type HookValue is a methodless func type — rendered inline as its base delegate

internal static (any, error) exec(any raw, reflectꓸValue from, reflectꓸValue to) {
    switch (raw.type()) {
    case Func<reflectꓸType, reflectꓸType, any, (any, error)> f: {
        return f(from.Type(), to.Type(), from.Interface());
    }
    case Func<reflectꓸKind, reflectꓸKind, any, (any, error)> f: {
        return f(from.Kind(), to.Kind(), from.Interface());
    }
    case Func<reflectꓸValue, reflectꓸValue, (any, error)> f: {
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
    case Func<reflectꓸType, reflectꓸType, any, (any, error)>: {
        return "type";
    }
    case Func<reflectꓸKind, reflectꓸKind, any, (any, error)> _:
    case Func<reflectꓸValue, reflectꓸValue, (any, error)> _: {
        return "kind or value";
    }}

    return "other";
}

internal static void Main() {
    var (from, to) = (reflect.ValueOf((nint)21), reflect.ValueOf(""));
    var hooks = new any[]{
        new Func<reflectꓸType, reflectꓸType, any, (any, error)>((reflectꓸType f, reflectꓸType t, any data) => (fmt.Sprintf("%s->%s:%v"u8, f, t, data), default)),
        new Func<reflectꓸKind, reflectꓸKind, any, (any, error)>((reflectꓸKind f, reflectꓸKind t, any data) => (fmt.Sprintf("%s->%s:%v"u8, f, t, data), default)),
        new Func<reflectꓸValue, reflectꓸValue, (any, error)>((reflectꓸValue f, reflectꓸValue t) => (f.Int() * 2, default)),
        nint (nint x) => x + 1,
        (nint)42
    }.slice();
    foreach (var (_, h) in hooks) {
        var (@out, err) = exec(h, from, to);
        fmt.Println(@out, err, kind(h));
    }

    {
        var (f, ok) = hooks[2]._<Func<reflectꓸValue, reflectꓸValue, (any, error)>>(ᐧ); if (ok) {
            var (@out, _) = f(reflect.ValueOf((nint)5), to);
            fmt.Println("asserted", @out);
        }
    }
}

} // end main_package
