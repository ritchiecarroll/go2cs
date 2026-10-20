namespace go;

using fmt = fmt_package;

partial class main_package {

internal static void Main() {
    f();
    panicValues();
    fmt.Println("Returned normally from f.");
}

internal static void f() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println("Recovered in f", r);
                }
            }
        }, ref ᒐ);
        fmt.Println("Calling g.");
        g(0);
        fmt.Println("Returned normally from g.");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object deferInGˢ = (@string)"Defer in g"u8;

internal static void g(nint i) {
    GoFrame ᒐ = default;
    try {
        if (i > 3) {
            fmt.Println("Panicking!");
            throw panic(fmt.Sprintf("%v"u8, i));
        }
        defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), deferInGˢ, i, ref ᒐ);
        fmt.Println("Printing in g", i);
        g(i + 1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static @string panicValueKind(Action f) {
    @string @out = default!;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var p = recover();
                switch (p.type()) {
                case null: {
                    @out = "no panic";
                    break;
                }
                case @string v: {
                    @out = fmt.Sprintf("string(%s) eq-x=%v"u8, v, v == "x"u8);
                    break;
                }
                case {} Δv when Δv._<error>(out var v): {
                    @out = "error("u8 + v.Error() + ")"u8;
                    break;
                }
                case nint v: {
                    @out = fmt.Sprintf("int(%d)"u8, v);
                    break;
                }
                case int32 v: {
                    @out = fmt.Sprintf("int(%d)"u8, v);
                    break;
                }
                default: {
                    var v = p;
                    @out = "other (not a plain string)";
                    break;
                }}
            }, ref ᒐ);
            f();
        }
        catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
        finally { ᒐ.Run(); }
    }))();
    return @out;
}

internal partial struct panicValues_label /*@string*/;

internal static void panicValues() {
    fmt.Println(panicValueKind(() => {
        throw panic("x");
    }));
    fmt.Println(panicValueKind(() => {
        throw panic(fmt.Sprintf("%s"u8, "x"));
    }));
    fmt.Println(panicValueKind(() => {
        @string s = "x";
        throw panic(s);
    }));
    fmt.Println(panicValueKind(() => {
        throw panic(((panicValues_label)(@string)"x"u8));
    }));
    fmt.Println(panicValueKind(() => {
        throw panic((nint)42);
    }));
    fmt.Println(panicValueKind(() => {
    }));
}

} // end main_package
