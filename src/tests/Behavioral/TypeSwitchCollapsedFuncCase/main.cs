namespace go;

using errors = errors_package;
using fmt = fmt_package;
using reflect = reflect_package;

partial class main_package {

public delegate (any, error) HookType(reflect.Type _Δp0, reflect.Type _Δp1, any _Δp2);
public delegate (any, error) HookKind(reflect.Kind _Δp0, reflect.Kind _Δp1, any _Δp2);
public delegate (any, error) HookValue(reflect.Value from, reflect.Value to);

internal static (any, error) exec(any raw, reflect.Value from, reflect.Value to) {
    switch (raw.type()) {
    case {} Δf when Δf._<HookType>(out var f): {
        return f(from.Type(), to.Type(), from.Interface());
    }
    case {} Δf when Δf._<HookKind>(out var f): {
        return f(from.Kind(), to.Kind(), from.Interface());
    }
    case {} Δf when Δf._<HookValue>(out var f): {
        return f(from, to);
    }
    case {} Δf when Δf._<Func<nint, nint>>(out var f): {
        return (f(from.Interface()._<nint>()), default);
    }
    default: {
        var f = raw;
        return (default, errors.New("invalid decode hook signature"));
    }}
}

internal static @string kind(any raw) {
    switch (raw.type()) {
    case {} ᴛ0 when ᴛ0._<HookType>(out var _): {
        return "type";
    }
    case {} ᴛ0 when ᴛ0._<HookKind>(out var _):
    case {} ᴛ1 when ᴛ1._<HookValue>(out var _): {
        return "kind or value";
    }}

    return "other";
}

internal static void main() {
    var (from, to) = (reflect.ValueOf((nint)21), reflect.ValueOf(""));
    var hooks = new any[]{
        new HookType([GoFunc("main.func1")] (reflect.Type f, reflect.Type t, any data) => (fmt.Sprintf("%s->%s:%v"u8, f, t, data), default)),
        new HookKind([GoFunc("main.func2")] (reflect.Kind f, reflect.Kind t, any data) => (fmt.Sprintf("%s->%s:%v"u8, f, t, data), default)),
        new HookValue([GoFunc("main.func3")] (reflect.Value f, reflect.Value t) => (f.Int() * 2, default)),
        [GoFunc("main.func4")] nint (nint x) => x + 1,
        (nint)42
    }.slice();
    foreach (var (_, h) in range(hooks)) {
        var (@out, err) = exec(h, from, to);
        fmt.Println(@out, err, kind(h));
    }

    {
        var (f, ok) = hooks[2]._<HookValue>(ᐧ); if (ok) {
            var (@out, _) = f(reflect.ValueOf((nint)5), to);
            fmt.Println("asserted", @out);
        }
    }
}

} // end main_package
