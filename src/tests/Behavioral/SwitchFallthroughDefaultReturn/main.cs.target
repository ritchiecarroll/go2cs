namespace go;

using fmt = fmt_package;
using System.Runtime.CompilerServices;

partial class main_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string zeroˢ = "zero"u8;
private static readonly @string oneˢ = "one"u8;
private static readonly @string manyˢ = "many"u8;

internal static partial @string classify(nint n) {
    var exprᴛ1 = n;
    var matchᴛ1 = false;
    if (exprᴛ1 is 0) { matchᴛ1 = true;
        return zeroˢ;
    }
    if (exprᴛ1 is 1) { matchᴛ1 = true;
        return oneˢ;
    }
    if (exprᴛ1 is 2) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        return manyˢ;
    }
    return default!;

}

internal static partial nint nonTerminal(nint n) {
    nint acc = 0;
    var exprᴛ1 = n;
    var matchᴛ1 = false;
    if (exprᴛ1 is 0) { matchᴛ1 = true;
        acc = 10;
    }
    else if (exprᴛ1 is 1) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        acc = 20;
    }

    return acc + n;
}

internal static partial nint conditionalReturn(nint n) {
    var exprᴛ1 = n;
    var matchᴛ1 = false;
    if (exprᴛ1 is 0) { matchᴛ1 = true;
        if (n < 0) {
            return 111;
        }
    }
    else if (exprᴛ1 is 1) { matchᴛ1 = true;
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1) { /* default: */
        return n * 1000;
    }

    return 777;
}

internal static partial (nint r, bool ok) namedDefer(nint n) {
    nint r = default!;
    bool ok = default!;
    GoFrame ᒐ = default;
    try {
        defer(() => {
            if (recover() != default!) {
                (r, ok) = (-1, false);
            }
        }, ref ᒐ);
        var exprᴛ1 = n;
        var matchᴛ1 = false;
        if (exprᴛ1 is 0) { matchᴛ1 = true;
            (r, ok) = (100, true); goto ᒐdone;
        }
        if (exprᴛ1 is 1) { matchᴛ1 = true;
            fallthrough = true;
        }
        if (fallthrough || !matchᴛ1) { /* default: */
            (r, ok) = (n * 10, true); goto ᒐdone;
        }

    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
    ᒐdone: return (r, ok);
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string intervalOnlyˢ = "interval-only"u8;
private static readonly @string noneˢ = "none"u8;
private static readonly @string bothOrIdleˢ = "both-or-idle"u8;

internal static @string keepAlive(nint idle, nint interval) {
    switch (ᐧ) {
    case {} when idle < 0 && interval >= 0: {
        return intervalOnlyˢ;
    }
    case {} when idle < 0 && interval < 0: {
        return noneˢ;
    }
    case {} when idle >= 0 && interval >= 0: {
        break;
    }}

    return bothOrIdleˢ;
}

internal static partial nint leadingDefault(nint n) {
    nint v = 0;
    var exprᴛ1 = n;
    var matchᴛ1 = false;
    var matchᴛ2 = exprᴛ1 is 3 || exprᴛ1 is 2 || exprᴛ1 is 1;
    if (!matchᴛ2) { /* default: */
        v |= (nint)(8);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is 3) { matchᴛ1 = true;
        v |= (nint)(4);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is 2) {
        v |= (nint)(2);
        fallthrough = true;
    }
    if (fallthrough || !matchᴛ1 && exprᴛ1 is 1) { matchᴛ1 = true;
        v |= (nint)(1);
    }

    return v;
}

internal static nint waitObject0 = 0;

internal static nint waitFailed = -1;

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string failedˢ = "failed"u8;
private static readonly @string unexpectedˢ = "unexpected"u8;

internal static partial @string waitShape(nint s) {
    var exprᴛ1 = s;
    if (exprᴛ1 == waitObject0) {
        do {
            break;
        } while (false);
    }
    else if (exprᴛ1 == waitFailed) {
        return failedˢ;
    }
    else { /* default: */
        return unexpectedˢ;
    }

    return "ok"u8;
}

internal static void Main() {
    foreach (var (_, n) in new nint[]{0, 1, 2, 3}.slice()) {
        fmt.Println(classify(n));
    }
    foreach (var (_, n) in new nint[]{0, 1, 2, 3, 4}.slice()) {
        fmt.Println(leadingDefault(n));
    }
    fmt.Println(keepAlive(-1, 5), keepAlive(-1, -1), keepAlive(3, 5));
    foreach (var (_, n) in new nint[]{0, 1, 2}.slice()) {
        fmt.Println(nonTerminal(n));
    }
    foreach (var (_, n) in new nint[]{0, 1, 2}.slice()) {
        fmt.Println(conditionalReturn(n));
    }
    foreach (var (_, n) in new nint[]{0, 1, 2}.slice()) {
        var (r, ok) = namedDefer(n);
        fmt.Println(r, ok);
    }
    foreach (var (_, s) in new nint[]{0, -1, 258}.slice()) {
        fmt.Println(waitShape(s));
    }
}

} // end main_package
