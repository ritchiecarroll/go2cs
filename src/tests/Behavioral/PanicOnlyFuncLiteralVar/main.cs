namespace go;

using fmt = fmt_package;

partial class main_package {

internal static (any v, error err) call(Func<(any, error)> fn) {
    any v = default!;
    error err = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    err = fmt.Errorf("recovered: %v"u8, r);
                }
            }
        }, ref ᒐ);
        (v, err) = fn();
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return (v, err);
}

internal static void Main() {
    var fn = (any, error) () => {
        throw panic("boom");
    };
    var (v, err) = call(fn);
    fmt.Println(v, err);
}

} // end main_package
