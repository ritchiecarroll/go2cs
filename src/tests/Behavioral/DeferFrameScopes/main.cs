namespace go;

using fmt = fmt_package;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object manyDeferˢ = (@string)"many defer"u8;

internal static void manyDefers() {
    GoFrame ᒐ = default;
    try {
        for (nint i = 1; i <= 6; i++) {
            defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), manyDeferˢ, i, ref ᒐ);
        }
        fmt.Println("many body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object loopDeferˢ = (@string)"loop defer"u8;

internal static void loopDefers(nint n) {
    GoFrame ᒐ = default;
    try {
        for (nint i = 0; i < n; i++) {
            defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), loopDeferˢ, i, ref ᒐ);
        }
        fmt.Println("loop body", n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

internal static (@string label, nint score) classify(nint n) {
    @string label = default!;
    nint score = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            score += 100;
            label = "<"u8 + label + ">"u8;
        }, ref ᒐ);

        for (nint i = 0; i < 3; i++) {
            if (i == 2 && n == 2) {
                (label, score) = ("from-loop", i);
                goto ᒐdone;
            }
        }

        switch (n) {
        case 3: {
            (label, score) = ("from-switch", 3);
            goto ᒐdone;
        }
        case 4: {
            (label, score) = ("from-switch-expr", 4); goto ᒐdone;
        }}

        if (n > 10) {
            label = "big";
            score = n;
            goto ᒐdone;
        }

        (label, score) = ("fallthrough", n);
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (label, score);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object outerDefer1ˢ = (@string)"outer defer 1"u8;
private static readonly object innerDeferˢ = (@string)"  inner defer"u8;
private static readonly object outerDefer2ˢ = (@string)"outer defer 2"u8;

internal static void nestedDeferScopes() {
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => fmt.Println(ᴛ1), outerDefer1ˢ, ref ᒐ);
        defer(() => {
            GoFrame ᒐ = default;
            try {
                defer(ᴛ1 => fmt.Println(ᴛ1), innerDeferˢ, ref ᒐ);
                fmt.Println("  closure body");
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }, ref ᒐ);
        defer(ᴛ1 => fmt.Println(ᴛ1), outerDefer2ˢ, ref ᒐ);
        fmt.Println("nested body");
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object mixedDeferTotalˢ = (@string)"mixed defer, total ="u8;

internal static nint mixedScopes() {
    GoFrame ᒐ = default;
    try {
        nint total = 0;

        defer((ᴛ1, ᴛ2) => fmt.Println(ᴛ1, ᴛ2), mixedDeferTotalˢ, total, ref ᒐ);

        nint add(nint n) => n * 2;
        var addʗ1 = add;
        void bump() {
            total += addʗ1(3);
        }

        bump();
        bump();

        ((Action)(() => {
            GoFrame ᒐ = default;
            try {
                defer(() => {
                    total += 1;
                }, ref ᒐ);
                total += 10;
            }
            catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
            finally { ᒐ.Run(); }
        }))();

        fmt.Println("mixed body, total =", total);
        return total;
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); return default!; }
    finally { ᒐ.Run(); }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly object guardedCleanupˢ = (@string)"guarded cleanup"u8;

internal static @string /*msg*/ guarded(bool boom) {
    @string msg = default!;
    GoFrame ᒐ = default;
    try {
        defer(ᴛ1 => fmt.Println(ᴛ1), guardedCleanupˢ, ref ᒐ);
        defer(() => {
            {
                var r = recover(); if (r != default!) {
                    msg = fmt.Sprint("recovered ", r);
                }
            }
        }, ref ᒐ);

        if (boom) {
            throw panic("frame boom");
        }

        msg = "no panic";
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    return msg;
}

internal static void Main() {
    manyDefers();
    loopDefers(3);
    loopDefers(0);

    var (ᴛ1, ᴛ2) = classify(2);
    fmt.Println(ᴛ1, ᴛ2);
    var (ᴛ3, ᴛ4) = classify(3);
    fmt.Println(ᴛ3, ᴛ4);
    var (ᴛ5, ᴛ6) = classify(4);
    fmt.Println(ᴛ5, ᴛ6);
    var (ᴛ7, ᴛ8) = classify(42);
    fmt.Println(ᴛ7, ᴛ8);
    var (ᴛ9, ᴛ10) = classify(0);
    fmt.Println(ᴛ9, ᴛ10);

    nestedDeferScopes();
    fmt.Println("mixed returned", mixedScopes());

    fmt.Println(guarded(false));
    fmt.Println(guarded(true));
}

} // end main_package
