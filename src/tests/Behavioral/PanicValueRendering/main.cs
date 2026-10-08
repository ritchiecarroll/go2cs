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

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recoveredˢ = (@string)"recovered:"u8;
private static readonly @string aTxtˢ = "a.txt"u8;
private static readonly @string cfgYamlˢ = "cfg.yaml"u8;
private static readonly @string simpleˢ = "simple"u8;
private static readonly @string finalTxtˢ = "final.txt"u8;

internal static void Main() {
    fmt.Println(recoveredˢ, recoverText(() => {
        throw panic(mustOpen(aTxtˢ));
    }));
    var wrapped = fmt.Errorf("load config: %w"u8, mustOpen(cfgYamlˢ));
    var wrappedʗ1 = wrapped;
    fmt.Println(recoveredˢ, recoverText(() => {
        throw panic(wrappedʗ1);
    }));
    fmt.Println(recoveredˢ, recoverText(() => {
        throw panic("plain string");
    }));
    fmt.Println(recoveredˢ, recoverText(() => {
        throw panic(errors.New(simpleˢ));
    }));
    throw panic(mustOpen(finalTxtˢ));
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noPanicˢ = "<no panic>"u8;

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
        msg = noPanicˢ;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

} // end main_package
