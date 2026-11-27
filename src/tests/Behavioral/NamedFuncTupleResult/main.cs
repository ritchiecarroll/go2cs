namespace go;

using context = context_package;
using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

internal delegate void cancelFunc(error _);

internal static (context.Context, cancelFunc) named(context.Context parent) {
    var (ctx, cancel) = context.WithCancelCause(parent);
    return (ctx, builtin.NilSafeDelegateConversion<cancelFunc, context.CancelCauseFunc>(cancel));
}

internal static (context.Context, Action<error>) unnamed(context.Context parent) {
    var (ᴛ1, ᴛ2) = named(parent);
    return (ᴛ1, builtin.NilSafeDelegateConversion<Action<error>, cancelFunc>(ᴛ2));
}

internal static (context.Context, Action<error>) stdlib(context.Context parent) {
    var (ᴛ1, ᴛ2) = context.WithCancelCause(parent);
    return (ᴛ1, builtin.NilSafeDelegateConversion<Action<error>, context.CancelCauseFunc>(ᴛ2));
}

internal static (context.Context, cancelFunc) back(context.Context parent) {
    var (ᴛ1, ᴛ2) = stdlib(parent);
    return (ᴛ1, builtin.NilSafeDelegateConversion<cancelFunc, Action<error>>(ᴛ2));
}

internal static (nint, cancelFunc) none() { return (0, default); }

internal static (nint, Action<error>) forwardNone() {
    var (ᴛ1, ᴛ2) = none();
    return (ᴛ1, builtin.NilSafeDelegateConversion<Action<error>, cancelFunc>(ᴛ2));
}

internal static void main() {
    foreach (var (_, f) in range(slice<Func<context.Context, (context.Context, Action<error>)>>([unnamed, stdlib]))) {
        var (ctxΔ1, cancelΔ1) = f(context.Background());
        cancelΔ1(errors.New("boom"));
        fmt.Println(ctxΔ1.Err(), context.Cause(ctxΔ1));
    }
    var (ctx, cancel) = back(context.Background());
    cancel(errors.New("back"));
    fmt.Println(ctx.Err(), context.Cause(ctx));
    var (n, c) = forwardNone();
    fmt.Println(n, c == nil);
}

} // end main_package
