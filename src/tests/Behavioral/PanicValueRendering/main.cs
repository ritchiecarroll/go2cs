namespace go;

using errors = errors_package;
using fmt = fmt_package;

partial class main_package {

partial struct openErr {
    internal @string path;
    internal nint code;
}

internal static @string Error(this ref openErr e) {
    return fmt.Sprintf("open %s: code %d"u8, e.path, e.code);
}

internal static error mustOpen(@string path) {
    return new openErrжerror(Ꮡ(new openErr(path: path, code: 13)));
}

internal static void Main() {
    fmt.Println("recovered:", recoverText(() => {
        throw panic(mustOpen("a.txt"));
    }));

    var wrapped = fmt.Errorf("load config: %w"u8, mustOpen("cfg.yaml"));
    var wrappedʗ1 = wrapped;
    fmt.Println("recovered:", recoverText(() => {
        throw panic(wrappedʗ1);
    }));

    fmt.Println("recovered:", recoverText(() => {
        throw panic("plain string");
    }));

    fmt.Println("recovered:", recoverText(() => {
        throw panic(errors.New("simple"));
    }));

    throw panic(mustOpen("final.txt"));
}

internal static @string /*msg*/ recoverText(Action f) {
    @string msg = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    msg = fmt.Sprintf("%v"u8, r);
                }
            }
        }, ref ᒐ);
        f();
        msg = "<no panic>";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

} // end main_package
