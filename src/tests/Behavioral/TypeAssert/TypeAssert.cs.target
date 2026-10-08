namespace go;

using fmt = fmt_package;

partial class main_package {

partial struct box {
    internal nint n;
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object programCompletedAfterˢ = (@string)"Program completed after panic recovery"u8;

internal static void Main() {
    safeAssertions();
    assertionsWithPanic();
    pointerAssertion();
    fmt.Println(programCompletedAfterˢ);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object valueIsABoxˢ = (@string)"Value is a *box:"u8;
private static readonly object pointerValueˢ = (@string)"Pointer value:"u8;

internal static void pointerAssertion() {
    any i = Ꮡ(new box(n: 7));
    {
        var (bΔ1, ok) = i._<ж<box>>(ᐧ); if (ok) {
            fmt.Println(valueIsABoxˢ, (~bΔ1).n);
        }
    }
    var b = i._<ж<box>>();
    fmt.Println(pointerValueˢ, (~b).n);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object helloˢ = (@string)"hello"u8;
private static readonly object valueIsAStringˢ = (@string)"Value is a string:"u8;
private static readonly object valueIsNotAStringˢ = (@string)"Value is not a string"u8;
private static readonly object valueIsAnIntˢ = (@string)"Value is an int:"u8;
private static readonly object valueIsNotAnIntˢ = (@string)"Value is not an int"u8;

internal static void safeAssertions() {
    any i = helloˢ;
    {
        var (s, ok) = i._<@string>(ᐧ); if (ok){
            fmt.Println(valueIsAStringˢ, s);
        } else {
            fmt.Println(valueIsNotAStringˢ);
        }
    }
    {
        var (n, ok) = i._<nint>(ᐧ); if (ok){
            fmt.Println(valueIsAnIntˢ, n);
        } else {
            fmt.Println(valueIsNotAnIntˢ);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object recoveredFromPanicˢ = (@string)"Recovered from panic:"u8;
private static readonly object stringValueˢ = (@string)"String value:"u8;
private static readonly object integerValueˢ = (@string)"Integer value:"u8;

internal static void assertionsWithPanic() {
    GoFrame ᒐ = default;
    try {
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    fmt.Println(recoveredFromPanicˢ, r);
                }
            }
        }, ref ᒐ);
        any i = helloˢ;
        @string s = i._<@string>();
        fmt.Println(stringValueˢ, s);
        nint n = i._<nint>();
        fmt.Println(integerValueˢ, n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end main_package
