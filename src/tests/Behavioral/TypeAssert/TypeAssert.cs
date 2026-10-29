namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct box {
    internal nint n;
}

internal static void Main() {
    safeAssertions();

    assertionsWithPanic();

    pointerAssertion();

    fmt.Println("Program completed after panic recovery");
}

internal static void pointerAssertion() {
    any i = Ꮡ(new box(n: 7));

    {
        var (bΔ1, ok) = i._<ж<box>>(ᐧ); if (ok) {
            fmt.Println("Value is a *box:", bΔ1.n);
        }
    }

    var b = i._<ж<box>>();
    fmt.Println("Pointer value:", b.n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object helloˢ = (@string)"hello"u8;

internal static void safeAssertions() {
    any i = helloˢ;

    {
        var (s, ok) = i._<@string>(ᐧ); if (ok) {
            fmt.Println("Value is a string:", s);
        } else {
            fmt.Println("Value is not a string");
        }
    }

    {
        var (n, ok) = i._<nint>(ᐧ); if (ok) {
            fmt.Println("Value is an int:", n);
        } else {
            fmt.Println("Value is not an int");
        }
    }
}

internal static void assertionsWithPanic() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != nil) {
                    fmt.Println("Recovered from panic:", r);
                }
            }
        }, ref ᒐ);

        any i = helloˢ;

        @string s = i._<@string>();
        fmt.Println("String value:", s);

        nint n = i._<nint>();

        fmt.Println("Integer value:", n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
