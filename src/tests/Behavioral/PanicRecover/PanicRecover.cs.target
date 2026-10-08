namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object returnedNormallyFromFˢ = (@string)"Returned normally from f."u8;

internal static void Main() {
    f();
    panicValues();
    fmt.Println(returnedNormallyFromFˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recoveredInFˢ = (@string)"Recovered in f"u8;
private static readonly object callingGˢ = (@string)"Calling g."u8;
private static readonly object returnedNormallyFromGˢ = (@string)"Returned normally from g."u8;

internal static void f() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(recoveredInFˢ, r);
                }
            }
        }, ref ᒐ);
        fmt.Println(callingGˢ);
        g(0);
        fmt.Println(returnedNormallyFromGˢ);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object panickingˢ = (@string)"Panicking!"u8;
private static readonly object deferInGˢ = (@string)"Defer in g"u8;
private static readonly object printingInGˢ = (@string)"Printing in g"u8;

internal static void g(nint i) {
    GoFrame ᒐ = default;
    try {
        if (i > 3) {
            fmt.Println(panickingˢ);
            throw panic(fmt.Sprintf("%v"u8, i));
        }
        defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), deferInGˢ, i, ref ᒐ);
        fmt.Println(printingInGˢ, i);
        g(i + 1);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string noPanicˢ = "no panic"u8;
private static readonly @string otherNotAPlainStringˢ = "other (not a plain string)"u8;

internal static @string panicValueKind(Action f) {
    @string @out = default!;
    ((Action)(() => {
        GoFrame ᒐ = default;
        try {
            defer(() => {
                var p = recover();
                switch (p.type()) {
                case null: {
                    @out = noPanicˢ;
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
                    @out = otherNotAPlainStringˢ;
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
        throw panic(fmt.Sprintf("%s"u8, (@string)"x"u8));
    }));
    fmt.Println(panicValueKind(() => {
        @string s = "x"u8;
        throw panic(s);
    }));
    fmt.Println(panicValueKind(() => {
        throw panic(((panicValues_label)(@string)"x"u8));
    }));
    fmt.Println(panicValueKind(() => {
        throw panic((nint)(42));
    }));
    fmt.Println(panicValueKind(() => {
    }));
}

} // end main_package
